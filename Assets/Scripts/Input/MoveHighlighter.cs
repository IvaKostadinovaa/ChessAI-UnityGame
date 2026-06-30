using System.Collections.Generic;
using UnityEngine;

public class MoveHighlighter : MonoBehaviour
{
    private BoardView _view;

    public void Init(BoardView view)
    {
        _view = view;
    }

    public void Show(Square selected, List<Move> moves, BoardModel board)
    {
        _view.ClearHighlights();
        _view.HighlightSelected(selected);

        foreach (var move in moves)
        {
            if (!move.Captured.IsEmpty || move.IsEnPassant)
                _view.HighlightCapture(move.To);
            else
                _view.HighlightMove(move.To);
        }
    }

    public void Clear()
    {
        _view.ClearHighlights();
    }
}
