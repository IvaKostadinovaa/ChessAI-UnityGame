using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AISettingsUI : MonoBehaviour
{
    [Header("References")]
    public AIController      aiController;
    public GameModeManager   gameModeManager;
    public TurnManager       turnManager;

    [Header("Mode Buttons")]
    public Button            hvhButton;
    public Button            hvaiButton;
    public Button            aivaiButton;

    [Header("Status")]
    public TextMeshProUGUI   statusText;
    public TextMeshProUGUI   turnText;

    void Start()
    {
        if (hvhButton   != null) hvhButton  .onClick.AddListener(() => SwitchMode(GameMode.HumanVsHuman));
        if (hvaiButton  != null) hvaiButton .onClick.AddListener(() => SwitchMode(GameMode.HumanVsAI));
        if (aivaiButton != null) aivaiButton.onClick.AddListener(() => SwitchMode(GameMode.AIVsAI));

        if (turnManager != null)
        {
            if (turnText != null)
                turnManager.OnTurnChanged += color => UpdateTurnText(color);

            if (statusText != null)
            {
                turnManager.OnCheck       += color  => statusText.text = $"{color} is in Check!";
                turnManager.OnTurnChanged += _      =>
                {
                    if (!aiController.IsThinking) statusText.text = "";
                };
            }
        }

        if (turnText != null) UpdateTurnText(PieceColor.White);
    }

    void Update()
    {
        if (statusText != null && aiController.IsThinking)
            statusText.text = $"AI thinking... (depth {aiController.SearchDepth})";
    }

    private void SwitchMode(GameMode mode)
    {
        gameModeManager.SwitchMode(mode);
        Debug.Log($"[UI] Mode: {mode}");
    }

    private void UpdateTurnText(PieceColor color) =>
        turnText.text = $"{color}'s Turn";
}
