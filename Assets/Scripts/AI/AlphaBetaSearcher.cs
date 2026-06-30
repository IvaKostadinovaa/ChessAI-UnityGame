using System.Collections.Generic;
using System.Diagnostics;

// Fail-soft alpha-beta negamax. Prunes when bestScore >= beta.
// Evaluator score is always from the side-to-move's perspective.
public class AlphaBetaSearcher
{
    private readonly IEvaluator              _evaluator;
    private int                              _nodesVisited;
    private int                              _pruneCount;
    private Dictionary<string, int>          _posHistory;

    // Optional time guard set by IterativeDeepeningSearcher before calling Search().
    private Stopwatch _externalTimer;
    private long      _timeLimitMs;
    private bool      _interrupted;

    // Random offset on root moves only, keeps AI vs AI games from repeating.
    private bool          _addNoise;
    private int           _rootDepth;
    private System.Random _rng;

    public int  PruneCount     => _pruneCount;
    public bool WasInterrupted => _interrupted;

    public AlphaBetaSearcher(IEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    public void SetNoise(bool addNoise)
    {
        _addNoise = addNoise;
        if (addNoise) _rng = new System.Random();
    }

    // Set by IterativeDeepeningSearcher to share its timer + budget — without it, search has no time limit.
    public void SetTimeGuard(Stopwatch timer, long timeLimitMs)
    {
        _externalTimer = timer;
        _timeLimitMs   = timeLimitMs;
        _interrupted   = false;
    }

    public SearchResult Search(BoardModel board, int depth, Dictionary<string, int> posHistory = null)
    {
        _nodesVisited = 0;
        _pruneCount   = 0;
        _interrupted  = false;
        _posHistory   = posHistory;
        _rootDepth    = depth;

        var sw = Stopwatch.StartNew();
        var (score, bestMove, hasMove) = AlphaBeta(board, depth, -int.MaxValue, int.MaxValue);
        sw.Stop();

        return new SearchResult
        {
            BestMove     = bestMove,
            Score        = score,
            NodesVisited = _nodesVisited,
            DepthReached = depth,
            ElapsedMs    = sw.ElapsedMilliseconds,
            HasMove      = hasMove
        };
    }

    private (int score, Move bestMove, bool hasMove) AlphaBeta(
        BoardModel board, int depth, int alpha, int beta)
    {
        if (TimedOut()) return (0, default, false);

        _nodesVisited++;

        if (depth == 0)
            return (Quiescence(board, alpha, beta), default, false);

        List<Move> moves = LegalMoveFilter.GetAllLegalMoves(board.SideToMove, board);

        if (moves.Count == 0)
        {
            // No legal moves: checkmate if in check, otherwise stalemate (draw).
            return CheckDetector.IsKingInCheck(board.SideToMove, board)
                ? (-100000, default, false)
                : (0,       default, false);
        }

        OrderMoves(moves);

        Move bestMove  = moves[0]; // fallback if interrupted before any move is evaluated
        int  bestScore = -int.MaxValue;

        foreach (var move in moves)
        {
            if (TimedOut()) break;

            BoardModel clone = board.Clone();
            clone.ApplyMove(move);

            int childScore = RepetitionDraw(clone)
                ? 0
                : -AlphaBeta(clone, depth - 1, -beta, -System.Math.Max(alpha, bestScore)).score;

            if (_addNoise && depth == _rootDepth)
                childScore += _rng.Next(-10, 11);

            if (childScore > bestScore) { bestScore = childScore; bestMove = move; }
            if (bestScore >= beta)      { _pruneCount++; break; }
        }

        return (bestScore, bestMove, true);
    }

    // Captures-only search past the depth limit and avoids horizon-effect blunders.
    private int Quiescence(BoardModel board, int alpha, int beta)
    {
        if (TimedOut()) return 0;

        _nodesVisited++;

        // Stand-pat: if the static evaluation already exceeds beta, prune immediately.
        int standPat = _evaluator.Evaluate(board);

        if (standPat >= beta)  return standPat;
        if (standPat > alpha)  alpha = standPat;

        List<Move> captures = LegalMoveFilter.GetAllLegalMoves(board.SideToMove, board);
        captures.RemoveAll(m => m.Captured.IsEmpty && !m.IsEnPassant);
        if (captures.Count == 0) return alpha;

        OrderMoves(captures);

        foreach (var move in captures)
        {
            if (TimedOut()) break;

            BoardModel clone = board.Clone();
            clone.ApplyMove(move);

            int score = -Quiescence(clone, -beta, -alpha);
            if (score >= beta) return score;
            if (score > alpha) alpha = score;
        }

        return alpha;
    }

    // MVV-LVA: highest-value victim first; non-captures sorted last.
    internal static void OrderMoves(List<Move> moves)
    {
        moves.Sort((a, b) => MoveScore(b).CompareTo(MoveScore(a)));
    }

    private static int MoveScore(Move m)
    {
        if (m.IsEnPassant)      return MaterialEvaluator.PawnValue * 10;
        if (m.Captured.IsEmpty) return 0;
        return MaterialEvaluator.PieceValue(m.Captured.Type) * 10;
    }

    private bool RepetitionDraw(BoardModel board)
    {
        if (_posHistory == null) return false;
        _posHistory.TryGetValue(board.GetPositionKey(), out int cnt);
        return cnt >= 2;
    }

    private bool TimedOut()
    {
        if (_externalTimer == null || _timeLimitMs <= 0) return false;
        if (_interrupted) return true;
        _interrupted = _externalTimer.ElapsedMilliseconds >= _timeLimitMs;
        return _interrupted;
    }
}
