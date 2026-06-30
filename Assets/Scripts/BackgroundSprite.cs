using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundSprite : MonoBehaviour
{
    void Start()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr.sprite != null) return;

#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("play_background t:Texture2D");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (obj is Sprite s && s.name == "play_background_0")
                {
                    sr.sprite = s;
                    return;
                }
            }
        }
#endif
    }
}
