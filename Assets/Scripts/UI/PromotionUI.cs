using System;
using UnityEngine;
using UnityEngine.UI;

public class PromotionUI : MonoBehaviour
{
    public GameObject panel;
    public Button     queenButton;
    public Button     rookButton;
    public Button     bishopButton;
    public Button     knightButton;

    private Action<PieceType> _callback;

    void Awake()
    {
        queenButton .onClick.AddListener(() => Choose(PieceType.Queen));
        rookButton  .onClick.AddListener(() => Choose(PieceType.Rook));
        bishopButton.onClick.AddListener(() => Choose(PieceType.Bishop));
        knightButton.onClick.AddListener(() => Choose(PieceType.Knight));
        panel.SetActive(false);
    }

    public void Show(Action<PieceType> callback)
    {
        _callback = callback;
        panel.SetActive(true);
    }

    private void Choose(PieceType type)
    {
        panel.SetActive(false);
        _callback?.Invoke(type);
        _callback = null;
    }
}
