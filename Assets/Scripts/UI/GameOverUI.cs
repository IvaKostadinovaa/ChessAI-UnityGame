using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI resultText;
    public Button restartButton;
    public Button mainMenuButton;
    public StartMenuUI startMenu;
    public ChessGameManager gameManager;

    [Header("Ending Backgrounds")]
    public Image panelBackground;
    public Sprite whiteWinsBackground;
    public Sprite blackWinsBackground;
    public Sprite drawBackground;

    void Awake()
    {
        panel.SetActive(false);
        restartButton.onClick.AddListener(() => { panel.SetActive(false); gameManager.RestartCurrentMode(); });
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    void GoToMainMenu()
    {
        panel.SetActive(false);
        if (startMenu != null) startMenu.ShowMenu();
    }

    public void Show(GameResult result)
    {
        panel.SetActive(true);
        resultText.text = result switch
        {
            GameResult.WhiteWins       => "White wins by Checkmate!",
            GameResult.BlackWins       => "Black wins by Checkmate!",
            GameResult.Stalemate       => "Stalemate - Draw!",
            GameResult.Draw            => "Draw - Insufficient Material!",
            GameResult.DrawRepetition  => "Draw - Threefold Repetition!",
            GameResult.WhiteResigns    => "Black wins - White Resigned!",
            GameResult.BlackResigns    => "White wins - Black Resigned!",
            GameResult.WhiteWinsOnTime => "White wins - Black ran out of time!",
            GameResult.BlackWinsOnTime => "Black wins - White ran out of time!",
            _                          => ""
        };

        if (panelBackground != null)
        {
            bool blackWins = result == GameResult.BlackWins ||
                             result == GameResult.WhiteResigns ||
                             result == GameResult.BlackWinsOnTime;

            bool isDraw = result == GameResult.Stalemate ||
                          result == GameResult.Draw ||
                          result == GameResult.DrawRepetition;

            if (isDraw)
                panelBackground.sprite = drawBackground;
            else if (blackWins)
                panelBackground.sprite = blackWinsBackground;
            else
                panelBackground.sprite = whiteWinsBackground;
        }
    }
}
