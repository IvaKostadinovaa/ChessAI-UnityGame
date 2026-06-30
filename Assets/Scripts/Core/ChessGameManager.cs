using UnityEngine;

public class ChessGameManager : MonoBehaviour
{
    public BoardView             boardView;
    public PlayerInputController inputController;
    public TurnManager           turnManager;
    public GameModeManager       gameModeManager;
    public AIController          aiController;
    public GameOverUI            gameOverUI;
    public MoveHistoryUI         moveHistoryUI;
    public InGameHUDController   hudController;

    public event System.Action OnGameStarted;

    private BoardModel _model;
    private GameMode   _selectedMode;

    public void StartWithMode(GameMode mode)
    {
        _selectedMode = mode;
        gameModeManager.Mode = mode;

        _model = new BoardModel();

        boardView.Initialize(_model);
        inputController.Init(_model);
        turnManager.Init(_model);
        aiController.Init(_model, boardView, turnManager, moveHistoryUI, gameModeManager.metricsLogger);
        gameModeManager.Init(turnManager);

        if (gameOverUI != null) turnManager.OnGameOver += gameOverUI.Show;
        turnManager.OnGameOver += _ => inputController.Enabled = false;
        turnManager.OnCheck    += color => Debug.Log($"{color} is in check!");

        inputController.OnMoveMade += move =>
        {
            moveHistoryUI?.AddMove(move, CheckDetector.Opponent(_model.SideToMove), _model.GetPiece(move.To).Type);
            turnManager.OnMoveMade(move);
        };

        aiController.TriggerIfAITurn();
        hudController?.SetupHUD();
        OnGameStarted?.Invoke();
        Debug.Log($"Game started: {mode}");
    }

    public void RestartCurrentMode() => StartWithMode(_selectedMode);
}
