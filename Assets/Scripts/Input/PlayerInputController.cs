using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    public BoardView       boardView;
    public MoveHighlighter highlighter;
    public PromotionUI     promotionUI;

    public event Action<Move> OnMoveMade;

    private BoardModel    _model;
    private Square        _selected    = new Square(-1, -1);
    private List<Move>    _legalMoves  = new List<Move>();
    private bool          _hasPiece    = false;
    public  bool          Enabled      = true;

    public void Init(BoardModel model)
    {
        _model = model;
        OnMoveMade = null;
        Enabled = true;
        highlighter.Init(boardView);
    }

    void Update()
    {
        if (!Enabled || _model == null) return;
        if (!UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector2 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        Vector3 world = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
        Square clicked = boardView.WorldToSquare(world);

        Debug.Log($"[Input] Mouse Click Screen: {mousePos} | World: {world} | Square: {clicked} (Valid: {clicked.IsValid})");

        if (!clicked.IsValid)
        {
            Deselect();
            return;
        }

        HandleClick(clicked);
    }

    private void HandleClick(Square clicked)
    {
        // If a piece is already selected, try to move
        if (_hasPiece)
        {
            Move? found = FindMove(clicked);
            if (found.HasValue)
            {
                ExecuteMove(found.Value);
                return;
            }
        }

        // Try selecting a piece of the current side
        PieceData piece = _model.GetPiece(clicked);
        if (!piece.IsEmpty && piece.Color == _model.SideToMove)
        {
            Select(clicked, piece);
            return;
        }

        Deselect();
    }

    private void Select(Square sq, PieceData piece)
    {
        _selected   = sq;
        _hasPiece   = true;
        _legalMoves = LegalMoveFilter.GetLegalMoves(sq, _model);
        highlighter.Show(sq, _legalMoves, _model);
    }

    private void Deselect()
    {
        _hasPiece   = false;
        _selected   = new Square(-1, -1);
        _legalMoves.Clear();
        highlighter.Clear();
    }

    private Move? FindMove(Square to)
    {
        foreach (var m in _legalMoves)
            if (m.To == to) return m;
        return null;
    }

    private void ExecuteMove(Move move)
    {
        if (move.IsPromotion)
        {
            if (promotionUI != null)
            {
                Enabled = false;
                boardView.MovePieceVisual(move.From, move.To);
                promotionUI.Show(chosenType =>
                {
                    var promoMove = Move.Promotion(move.From, move.To, chosenType, move.Captured);
                    _model.ApplyMove(promoMove);
                    boardView.RefreshPieceSprite(move.To, _model.GetPiece(move.To));
                    OnMoveMade?.Invoke(promoMove);
                    Deselect();
                    Enabled = true;
                    Debug.Log($"Move: {promoMove}  |  Side to move: {_model.SideToMove}");
                });
                return;
            }

            Debug.LogWarning("[PlayerInput] PromotionUI not assigned in Inspector — defaulting to Queen.");
            var queenMove = Move.Promotion(move.From, move.To, PieceType.Queen, move.Captured);
            boardView.MovePieceVisual(queenMove.From, queenMove.To);
            _model.ApplyMove(queenMove);
            boardView.RefreshPieceSprite(queenMove.To, _model.GetPiece(queenMove.To));
            OnMoveMade?.Invoke(queenMove);
            Deselect();
            Debug.Log($"Move: {queenMove}  |  Side to move: {_model.SideToMove}");
            return;
        }

        boardView.MovePieceVisual(move.From, move.To);
        _model.ApplyMove(move);
        boardView.SyncPieces(_model);
        OnMoveMade?.Invoke(move);
        Deselect();
        Debug.Log($"Move: {move}  |  Side to move: {_model.SideToMove}");
    }
}
