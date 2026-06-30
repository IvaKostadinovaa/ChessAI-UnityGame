using System.Collections;
using System.Threading;
using UnityEngine;

public class AIController : MonoBehaviour
{
    // Tuning only - never exposed to the player.
    public int        SearchDepth = 3;
    public long       TimeLimitMs = 2000;
    public PieceColor AIColor     = PieceColor.Black;
    public float      MoveDelay   = 1.5f;

    private BoardModel          _board;
    private BoardView           _view;
    private TurnManager         _turnManager;
    private MoveHistoryUI       _moveHistory;
    private SearchMetricsLogger _logger;

    private SearchResult  _pendingResult;
    private int           _pendingPruneCount;
    private volatile bool _resultReady = false;
    private volatile bool _searching   = false;
    private int           _moveNumber  = 0;
    private bool          _isAIVsAI   = false;

    public bool IsThinking => _searching;

    private void OnDisable()
    {
        // Prevents _searching from staying stuck true if disabled mid-search.
        StopAllCoroutines();
        _searching   = false;
        _resultReady = false;
    }

    public void Init(BoardModel board, BoardView view, TurnManager turnManager,
                     MoveHistoryUI moveHistory = null, SearchMetricsLogger logger = null)
    {
        _board       = board;
        _view        = view;
        _turnManager = turnManager;
        _moveHistory = moveHistory;
        _logger      = logger;

        var modeManager = FindAnyObjectByType<GameModeManager>();
        _isAIVsAI = modeManager != null && modeManager.Mode == GameMode.AIVsAI;
    }

    public void TriggerIfAITurn()
    {
        if (!gameObject.activeInHierarchy) return;
        if (_searching)                    return;
        if (_turnManager.GameOver)         return;
        if (!_isAIVsAI && _board.SideToMove != AIColor) return;

        StartSearch();
    }

    private void StartSearch()
    {
        _resultReady = false;
        _searching   = true;

        BoardModel snapshot  = _board.Clone();
        int        depth     = SearchDepth;
        long       timeLimit = TimeLimitMs;
        var posHistory = new System.Collections.Generic.Dictionary<string, int>(
            _turnManager.PositionHistory);

        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                var id         = new IterativeDeepeningSearcher(new PositionalEvaluator());
                _pendingResult     = id.Search(snapshot, depth, timeLimit, posHistory, _isAIVsAI);
                _pendingPruneCount = id.TotalPruneCount;
                _resultReady = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("[AI Search Thread Error] " + e.ToString());
                _resultReady = false;
                _searching   = false;
            }
        });
    }

    void Update()
    {
        if (!_resultReady) return;
        _resultReady = false;

        if (_turnManager.GameOver)   { _searching = false; return; }
        if (!_pendingResult.HasMove) { _searching = false; return; }

        StartCoroutine(ApplyMoveAfterDelay(_pendingResult, _pendingPruneCount));
    }

    private IEnumerator ApplyMoveAfterDelay(SearchResult result, int pruneCount)
    {
        yield return new WaitForSeconds(MoveDelay);
        _searching = false;

        if (_turnManager.GameOver) yield break;

        Move move       = result.BestMove;
        var  legalMoves = LegalMoveFilter.GetAllLegalMoves(_board.SideToMove, _board);
        bool isLegal    = legalMoves.Exists(m =>
            m.From        == move.From        &&
            m.To          == move.To          &&
            m.IsPromotion == move.IsPromotion &&
            (!move.IsPromotion || m.PromotionPiece == move.PromotionPiece));

        if (!isLegal)
        {
            Debug.LogError($"[AI] Returned illegal move {move} — retrying search.");
            StartSearch();
            yield break;
        }

        PieceColor movingColor = _board.SideToMove;
        PieceType  movingType  = _board.GetPiece(move.From).Type;
        _view.MovePieceVisual(move.From, move.To);
        _board.ApplyMove(move);

        if (move.IsEnPassant || move.IsCastle)
            _view.SyncPieces(_board);

        if (move.IsPromotion)
            _view.RefreshPieceSprite(move.To, _board.GetPiece(move.To));

        _moveHistory?.AddMove(move, movingColor, movingType);
        _turnManager.OnMoveMade(move);

        _moveNumber++;
        _logger?.Log(result, "IterativeDeepening+AlphaBeta(Negamax)", _moveNumber, pruneCount);
        Debug.Log($"[AI] {result}");
    }
}
