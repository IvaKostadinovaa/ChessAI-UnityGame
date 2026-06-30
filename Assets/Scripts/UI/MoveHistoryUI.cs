using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoveHistoryUI : MonoBehaviour
{
    public Transform     content;
    public TextMeshProUGUI entryPrefab;
    public ScrollRect    scrollRect;

    private List<string> _moves    = new List<string>();
    private int          _moveNum  = 1;

    public void AddMove(Move move, PieceColor color, PieceType movingPiece)
    {
        string notation = Notation.ToAlgebraic(move, movingPiece);

        if (color == PieceColor.White)
            _moves.Add($"{_moveNum}. {notation}");
        else
        {
            if (_moves.Count > 0)
                _moves[_moves.Count - 1] += $"  {notation}";
            else
                _moves.Add($"{_moveNum}... {notation}");
            _moveNum++;
        }

        RefreshUI();
    }

    public void Clear()
    {
        _moves.Clear();
        _moveNum = 1;
        foreach (Transform child in content) Destroy(child.gameObject);
    }

    private void RefreshUI()
    {
        foreach (Transform child in content) Destroy(child.gameObject);
        foreach (var line in _moves)
        {
            var entry = Instantiate(entryPrefab, content);
            entry.text = line;
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
