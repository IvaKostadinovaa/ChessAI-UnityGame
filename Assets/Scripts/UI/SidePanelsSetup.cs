using UnityEngine;
using UnityEngine.UI;

public class SidePanelsSetup : MonoBehaviour
{
    void Start()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        Transform gameplayUI = canvas.transform.Find("GameplayUI");
        Transform parent = gameplayUI != null ? gameplayUI : canvas.transform;

        Color bg = new Color(0.08f, 0.08f, 0.08f, 0.80f);
        float w = 0.17f;

        CreatePanel(parent, "LeftSidePanel",  new Vector2(0f,   0f), new Vector2(w,     1f), bg);
        CreatePanel(parent, "RightSidePanel", new Vector2(1f-w, 0f), new Vector2(1f,    1f), bg);
    }

    private void CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color = color;
    }
}
