using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartMenuUI : MonoBehaviour
{
    public GameObject        menuPanel;
    public Button            hvhButton;
    public Button            hvaiButton;
    public Button            aivaiButton;
    public ChessGameManager  gameManager;
    public GameObject        inGameUI;

    [Header("Audio")]
    public AudioClip         buttonClickSound;

    private AudioSource _audio;

    void Start()
    {
        if (FindAnyObjectByType<AudioListener>() == null)
            Camera.main.gameObject.AddComponent<AudioListener>();

        _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake          = false;
        _audio.ignoreListenerPause  = true;
        _audio.spatialBlend         = 0f;
        _audio.volume               = 1f;

        menuPanel.SetActive(true);
        if (inGameUI != null)
            inGameUI.SetActive(false);
        Time.timeScale = 0f;

        if (hvhButton   == null) hvhButton   = FindButtonInPanel("HvsH_button");
        if (hvaiButton  == null) hvaiButton  = FindButtonInPanel("HvsAI_button");
        if (aivaiButton == null) aivaiButton = FindButtonInPanel("AIvsAI_button");

        hvhButton  ?.onClick.AddListener(() => { PlayClick(); StartGame(GameMode.HumanVsHuman); });
        hvaiButton ?.onClick.AddListener(() => { PlayClick(); StartGame(GameMode.HumanVsAI); });
        aivaiButton?.onClick.AddListener(() => { PlayClick(); StartGame(GameMode.AIVsAI); });
    }

    void PlayClick()
    {
        if (buttonClickSound == null) { Debug.LogWarning("[StartMenuUI] No click sound assigned!"); return; }
        AudioListener.pause = false;
        _audio.PlayOneShot(buttonClickSound);
        Debug.Log("[StartMenuUI] Playing click sound");
    }

    Button FindButtonInPanel(string childName)
    {
        var t = menuPanel.transform.Find(childName);
        return t != null ? t.GetComponent<Button>() : null;
    }

    void StartGame(GameMode mode)
    {
        menuPanel.SetActive(false);
        if (inGameUI != null)
        {
            inGameUI.SetActive(true);
        }
        Time.timeScale = 1f;
        gameManager.StartWithMode(mode);
    }

    public void ShowMenu()
    {
        menuPanel.SetActive(true);
        if (inGameUI != null) inGameUI.SetActive(false);
        Time.timeScale = 0f;
    }
}
