using UnityEngine;

public enum GameMode { HumanVsHuman, HumanVsAI, AIVsAI }

public class GameModeManager : MonoBehaviour
{
    public GameMode              Mode         = GameMode.HumanVsAI;
    public AIController          aiController;
    public PlayerInputController inputController;
    public SearchMetricsLogger   metricsLogger;

    private TurnManager _turnManager;

    public void Init(TurnManager turnManager)
    {
        _turnManager = turnManager;
        _turnManager.OnTurnChanged += OnTurnChanged;
        _turnManager.OnGameOver    += OnGameOver;
        ApplyMode();
    }

    public void SwitchMode(GameMode newMode)
    {
        Mode = newMode;
        ApplyMode();
    }

    private void ApplyMode()
    {
        switch (Mode)
        {
            case GameMode.HumanVsHuman:
                inputController.Enabled = true;
                aiController.gameObject.SetActive(false);
                break;

            case GameMode.HumanVsAI:
                inputController.Enabled = true;
                aiController.gameObject.SetActive(true);
                break;

            case GameMode.AIVsAI:
                inputController.Enabled = false;
                aiController.gameObject.SetActive(true);
                metricsLogger?.Clear();
                aiController.TriggerIfAITurn();
                break;
        }
    }

    private void OnTurnChanged(PieceColor color)
    {
        if (_turnManager.GameOver) return;

        bool isAITurn = Mode == GameMode.AIVsAI ||
                        (Mode == GameMode.HumanVsAI && color == aiController.AIColor);

        if (isAITurn)
        {
            inputController.Enabled = false;
            aiController.TriggerIfAITurn();
        }
        else
        {
            inputController.Enabled = true;
        }
    }

    private void OnGameOver(GameResult result)
    {
        if (Mode == GameMode.AIVsAI)
        {
            metricsLogger?.ExportCSV();
            Debug.Log($"[Benchmark] Game over: {result}");
        }
    }
}
