using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InGameHUDController : MonoBehaviour
{
    [Header("References")]
    public ChessGameManager gameManager;
    public BoardView boardView;
    public AIController aiController;
    public GameModeManager gameModeManager;
    public TurnManager turnManager;

    [Header("White Player Panel")]
    public UnityEngine.UI.Image whiteAvatarImage;
    public TextMeshProUGUI whiteNameText;
    public TextMeshProUGUI whiteTimerText;

    [Header("Black Player Panel")]
    public UnityEngine.UI.Image blackAvatarImage;
    public TextMeshProUGUI blackNameText;
    public TextMeshProUGUI blackTimerText;

    [Header("Captured Panels")]
    public Transform capturedWhiteParent; 
    public Transform capturedBlackParent; 
    public GameObject capturedPiecePrefab; 

    [Header("Control Buttons")]
    public Button undoButton;
    public Button resignButton;
    public Button settingsButton;
    public Button hintButton;
    public Button soundButton;
    public TextMeshProUGUI muteOverlayText;
    public Button mainMenuButton;
    public StartMenuUI startMenuUI;

    [Header("Settings Overlay")]
    public GameObject aiSettingsPanel;

    [Header("Icons")]
    public Sprite humanIcon;
    public Sprite robotIcon;
    public Sprite robotIconWhite; // alternate avatar for White AI in AIvsAI mode

    [Header("Timers")]
    public float StartTimeSeconds = 120f; // 2 minutes

    [Header("Audio")]
    public AudioClip timerTickSound;
    public float lowTimeThreshold = 20f;

    private AudioSource _audio;
    private int _lastTickSecond = -1;

    private float _whiteTimeLeft;
    private float _blackTimeLeft;
    private bool _timersActive = false;

    private List<BoardModel> _boardHistory = new List<BoardModel>();
    private List<(Move move, PieceColor color, PieceType pieceType)> _moveHistoryList = new List<(Move move, PieceColor color, PieceType pieceType)>();

    private List<PieceData> _capturedWhitePieces = new List<PieceData>();
    private List<PieceData> _capturedBlackPieces = new List<PieceData>();

    private Coroutine _whiteNamePulse;
    private Coroutine _blackNamePulse;
    private bool _isMuted = false;

    void Awake()
    {
        if (gameManager != null)
            gameManager.OnGameStarted += SetupHUD;
    }

    void Start()
    {
        _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.spatialBlend = 0f;

        if (undoButton != null)    undoButton.onClick.AddListener(OnUndoClicked);
        if (resignButton != null)  resignButton.onClick.AddListener(OnResignClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(ToggleSettings);
        if (hintButton != null)    hintButton.onClick.AddListener(OnHintClicked);
        if (soundButton != null)    soundButton.onClick.AddListener(OnSoundToggled);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (muteOverlayText != null) muteOverlayText.gameObject.SetActive(false);
    }

    public void SetupHUD()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnChanged -= OnTurnChanged;
            turnManager.OnGameOver    -= OnGameOver;
            turnManager.OnMoveApplied -= OnMoveApplied;
            turnManager.OnTurnChanged += OnTurnChanged;
            turnManager.OnGameOver    += OnGameOver;
            turnManager.OnMoveApplied += OnMoveApplied;
        }

        _whiteTimeLeft = StartTimeSeconds;
        _blackTimeLeft = StartTimeSeconds;
        _timersActive = true;

        _boardHistory.Clear();
        _moveHistoryList.Clear();
        _capturedWhitePieces.Clear();
        _capturedBlackPieces.Clear();

        ClearCapturedUI();

        BoardModel currentBoard = GetBoardRef();
        if (currentBoard != null)
        {
            _boardHistory.Add(currentBoard.Clone());
        }

        GameMode mode = gameModeManager.Mode;
        if (mode == GameMode.HumanVsHuman)
        {
            whiteNameText.text = "WHITE";
            if (whiteAvatarImage != null) { whiteAvatarImage.sprite = humanIcon; whiteAvatarImage.preserveAspect = true; }

            blackNameText.text = "BLACK";
            if (blackAvatarImage != null) { blackAvatarImage.sprite = humanIcon; blackAvatarImage.preserveAspect = true; }
        }
        else if (mode == GameMode.HumanVsAI)
        {
            whiteNameText.text = "WHITE (YOU)";
            if (whiteAvatarImage != null) { whiteAvatarImage.sprite = humanIcon; whiteAvatarImage.preserveAspect = true; }

            blackNameText.text = "BLACK (AI)";
            if (blackAvatarImage != null) { blackAvatarImage.sprite = robotIcon; blackAvatarImage.preserveAspect = true; }
        }
        else if (mode == GameMode.AIVsAI)
        {
            whiteNameText.text = "WHITE (AI)";
            if (whiteAvatarImage != null) { whiteAvatarImage.sprite = robotIconWhite != null ? robotIconWhite : robotIcon; whiteAvatarImage.preserveAspect = true; }

            blackNameText.text = "BLACK (AI)";
            if (blackAvatarImage != null) { blackAvatarImage.sprite = robotIcon; blackAvatarImage.preserveAspect = true; }
        }

        PieceColor startTurn = turnManager != null ? turnManager.CurrentTurn : PieceColor.White;
        UpdateNamePulse(startTurn);
    }

    void Update()
    {
        if (turnManager == null || turnManager.GameOver || !_timersActive) return;

        if (turnManager.CurrentTurn == PieceColor.White)
        {
            _whiteTimeLeft -= Time.deltaTime;
            if (_whiteTimeLeft <= 0)
            {
                _whiteTimeLeft = 0;
                _timersActive = false;
                turnManager.TriggerGameOver(GameResult.BlackWinsOnTime);
                Debug.Log("White lost on time!");
            }
        }
        else
        {
            _blackTimeLeft -= Time.deltaTime;
            if (_blackTimeLeft <= 0)
            {
                _blackTimeLeft = 0;
                _timersActive = false;
                turnManager.TriggerGameOver(GameResult.WhiteWinsOnTime);
                Debug.Log("Black lost on time!");
            }
        }

        UpdateTimerTexts();
        UpdateTimerTick();
    }

    private void UpdateTimerTick()
    {
        float activeTime = turnManager.CurrentTurn == PieceColor.White ? _whiteTimeLeft : _blackTimeLeft;
        if (activeTime > 0 && activeTime <= lowTimeThreshold)
        {
            int second = Mathf.CeilToInt(activeTime);
            if (second != _lastTickSecond)
            {
                _lastTickSecond = second;
                if (timerTickSound != null && _audio != null)
                    _audio.PlayOneShot(timerTickSound);
            }
        }
        else
        {
            _lastTickSecond = -1;
        }
    }

    private void UpdateTimerTexts()
    {
        if (whiteTimerText != null) whiteTimerText.text = FormatTime(_whiteTimeLeft);
        if (blackTimerText != null) blackTimerText.text = FormatTime(_blackTimeLeft);
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnMoveApplied(Move move, PieceColor color)
    {
        BoardModel currentBoard = GetBoardRef();
        if (currentBoard != null)
            _boardHistory.Add(currentBoard.Clone());

        PieceType movedType = move.IsPromotion ? PieceType.Pawn : currentBoard?.GetPiece(move.To).Type ?? PieceType.Pawn;
        _moveHistoryList.Add((move, color, movedType));

        if (boardView != null) boardView.ShowLastMove(move.From, move.To);

        if (!move.Captured.IsEmpty)
            AddCapturedPiece(move.Captured);
    }

    private void AddCapturedPiece(PieceData piece)
    {
        if (piece.Color == PieceColor.White)
            _capturedWhitePieces.Add(piece);
        else
            _capturedBlackPieces.Add(piece);
        RefreshCapturedUI();
    }

    private void RefreshCapturedUI()
    {
        ClearCapturedUI();

        // White captures (Black pieces captured by White) are shown in capturedBlackParent
        foreach (var piece in _capturedBlackPieces)
        {
            CreateCapturedIcon(piece, capturedBlackParent);
        }

        // Black captures (White pieces captured by Black) are shown in capturedWhiteParent
        foreach (var piece in _capturedWhitePieces)
        {
            CreateCapturedIcon(piece, capturedWhiteParent);
        }
    }

    private void CreateCapturedIcon(PieceData piece, Transform parent)
    {
        if (parent == null) return;

        GameObject go = capturedPiecePrefab != null
            ? Instantiate(capturedPiecePrefab, parent)
            : new GameObject("CapturedIcon", typeof(RectTransform));

        if (go.transform.parent != parent)
            go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(44, 44);
        rt.anchoredPosition = Vector2.zero;

        var img = go.GetComponent<UnityEngine.UI.Image>();
        if (img == null) img = go.AddComponent<UnityEngine.UI.Image>();
        img.sprite         = GetPieceSprite(piece);
        img.color          = Color.white;
        img.preserveAspect = true;
    }

    private void ClearCapturedUI()
    {
        if (capturedWhiteParent != null)
            for (int i = capturedWhiteParent.childCount - 1; i >= 0; i--)
                Destroy(capturedWhiteParent.GetChild(i).gameObject);
        if (capturedBlackParent != null)
            for (int i = capturedBlackParent.childCount - 1; i >= 0; i--)
                Destroy(capturedBlackParent.GetChild(i).gameObject);
    }

    private Sprite GetPieceSprite(PieceData piece)
    {
        if (boardView == null) boardView = FindAnyObjectByType<BoardView>();
        if (boardView == null) return null;
        if (piece.Color == PieceColor.White)
        {
            return piece.Type switch
            {
                PieceType.Pawn => boardView.whitePawn,
                PieceType.Knight => boardView.whiteKnight,
                PieceType.Bishop => boardView.whiteBishop,
                PieceType.Rook => boardView.whiteRook,
                PieceType.Queen => boardView.whiteQueen,
                PieceType.King => boardView.whiteKing,
                _ => null
            };
        }
        else
        {
            return piece.Type switch
            {
                PieceType.Pawn => boardView.blackPawn,
                PieceType.Knight => boardView.blackKnight,
                PieceType.Bishop => boardView.blackBishop,
                PieceType.Rook => boardView.blackRook,
                PieceType.Queen => boardView.blackQueen,
                PieceType.King => boardView.blackKing,
                _ => null
            };
        }
    }

    private void OnTurnChanged(PieceColor color)
    {
        if (color == PieceColor.White)
            _whiteTimeLeft = StartTimeSeconds;
        else
            _blackTimeLeft = StartTimeSeconds;

        UpdateNamePulse(color);
    }

    private void OnGameOver(GameResult result)
    {
        _timersActive = false;
        StopNamePulses();
        if (whiteNameText != null) whiteNameText.color = Color.white;
        if (blackNameText != null) blackNameText.color = Color.white;
    }

    private void OnUndoClicked()
    {
        if (turnManager == null || turnManager.GameOver) return;
        if (aiController != null && aiController.IsThinking) return;

        int popCount = (gameModeManager.Mode == GameMode.HumanVsAI) ? 2 : 1;

        if (_boardHistory.Count > popCount)
        {
            for (int i = 0; i < popCount; i++)
            {
                _boardHistory.RemoveAt(_boardHistory.Count - 1);
                _moveHistoryList.RemoveAt(_moveHistoryList.Count - 1);
            }

            BoardModel restoredModel = _boardHistory[_boardHistory.Count - 1].Clone();

            SetBoardRef(restoredModel);
            RebuildCapturedListFromHistory();

            boardView.SyncPieces(restoredModel);
            boardView.ClearHighlights();
            boardView.ClearLastMove();

            if (gameManager.moveHistoryUI != null)
            {
                gameManager.moveHistoryUI.Clear();
                foreach (var record in _moveHistoryList)
                {
                    gameManager.moveHistoryUI.AddMove(record.move, record.color, record.pieceType);
                }
            }

            _whiteTimeLeft = StartTimeSeconds;
            _blackTimeLeft = StartTimeSeconds;
            _timersActive = true;

            gameManager.inputController.Init(restoredModel);
            turnManager.Init(restoredModel);
            turnManager.RebuildPositionHistory(_boardHistory);
            aiController.Init(restoredModel, boardView, turnManager, gameManager.moveHistoryUI, gameModeManager.metricsLogger);

            aiController.TriggerIfAITurn();
        }
    }

    private void RebuildCapturedListFromHistory()
    {
        _capturedWhitePieces.Clear();
        _capturedBlackPieces.Clear();
        foreach (var record in _moveHistoryList)
        {
            if (!record.move.Captured.IsEmpty)
            {
                if (record.move.Captured.Color == PieceColor.White)
                    _capturedWhitePieces.Add(record.move.Captured);
                else
                    _capturedBlackPieces.Add(record.move.Captured);
            }
        }
        RefreshCapturedUI();
    }

    private void OnHintClicked()
    {
        if (turnManager == null || turnManager.GameOver) return;
        if (aiController != null && aiController.IsThinking) return;
        if (gameModeManager != null && gameModeManager.Mode == GameMode.AIVsAI) return;
        StartCoroutine(ShowHintCoroutine());
    }

    private System.Collections.IEnumerator ShowHintCoroutine()
    {
        if (hintButton != null) hintButton.interactable = false;

        var board = GetBoardRef();
        var posHistory = new System.Collections.Generic.Dictionary<string, int>(turnManager.PositionHistory);
        var boardSnap = board.Clone();

        SearchResult hint = default;
        bool done = false;

        System.Threading.ThreadPool.QueueUserWorkItem(_ =>
        {
            var ab = new AlphaBetaSearcher(new PositionalEvaluator());
            hint = ab.Search(boardSnap, 3, posHistory);
            done = true;
        });

        while (!done) yield return null;

        if (hint.HasMove && boardView != null)
        {
            boardView.ClearHighlights();
            boardView.HighlightMove(hint.BestMove.From);
            boardView.HighlightMove(hint.BestMove.To);
        }

        if (hintButton != null) hintButton.interactable = true;
    }

    private void OnResignClicked()
    {
        if (turnManager == null || turnManager.GameOver) return;
        _timersActive = false;
        GameResult result = (turnManager.CurrentTurn == PieceColor.White) ? GameResult.WhiteResigns : GameResult.BlackResigns;
        turnManager.TriggerGameOver(result);
    }

    private void ToggleSettings()
    {
        if (aiSettingsPanel != null)
        {
            aiSettingsPanel.SetActive(!aiSettingsPanel.activeSelf);
        }
    }

    private void OnSoundToggled()
    {
        _isMuted = !_isMuted;
        AudioListener.volume = _isMuted ? 0f : 1f;
        if (muteOverlayText != null)
            muteOverlayText.gameObject.SetActive(_isMuted);
    }

    private void OnMainMenuClicked()
    {
        _timersActive = false;
        if (startMenuUI != null)
            startMenuUI.ShowMenu();
    }

    private BoardModel GetBoardRef()
    {
        var field = typeof(ChessGameManager).GetField("_model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (BoardModel)field.GetValue(gameManager);
    }

    private void SetBoardRef(BoardModel restoredModel)
    {
        var field = typeof(ChessGameManager).GetField("_model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(gameManager, restoredModel);
    }

    private void UpdateNamePulse(PieceColor activeTurn)
    {
        StopNamePulses();

        if (activeTurn == PieceColor.White)
        {
            if (whiteNameText != null) _whiteNamePulse = StartCoroutine(PulseNameGlow(whiteNameText));
            if (blackNameText  != null) blackNameText.color  = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            if (blackNameText  != null) _blackNamePulse = StartCoroutine(PulseNameGlow(blackNameText));
            if (whiteNameText  != null) whiteNameText.color  = new Color(0.5f, 0.5f, 0.5f);
        }
    }

    private void StopNamePulses()
    {
        if (_whiteNamePulse != null) { StopCoroutine(_whiteNamePulse); _whiteNamePulse = null; }
        if (_blackNamePulse != null) { StopCoroutine(_blackNamePulse); _blackNamePulse = null; }
    }

    private System.Collections.IEnumerator PulseNameGlow(TextMeshProUGUI text)
    {
        Color bright = new Color(0.2f, 1f, 0.2f);
        Color normal = Color.white;
        while (true)
        {
            float t = (Mathf.Sin(Time.time * 3f) + 1f) * 0.5f;
            text.color = Color.Lerp(normal, bright, t);
            yield return null;
        }
    }
}