using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public event Action<PieceColor>  OnTurnChanged;
    public event Action<GameResult>  OnGameOver;
    public event Action<PieceColor>  OnCheck;
    public event Action<Move, PieceColor> OnMoveApplied;

    public PieceColor CurrentTurn => _board != null ? _board.SideToMove : PieceColor.White;
    public bool       GameOver    { get; private set; }

    public System.Collections.Generic.IReadOnlyDictionary<string, int> PositionHistory => _positionHistory;

    private BoardModel _board;
    private Dictionary<string, int> _positionHistory = new Dictionary<string, int>();

    public void Init(BoardModel board)
    {
        OnGameOver    = null;
        OnTurnChanged = null;
        OnCheck       = null;
        OnMoveApplied = null;
        _board   = board;
        GameOver = false;
        _positionHistory.Clear();
        _positionHistory[board.GetPositionKey()] = 1;
    }

    public void RebuildPositionHistory(IEnumerable<BoardModel> history)
    {
        _positionHistory.Clear();
        foreach (var b in history)
        {
            string key = b.GetPositionKey();
            _positionHistory.TryGetValue(key, out int n);
            _positionHistory[key] = n + 1;
        }
    }

    public void TriggerGameOver(GameResult result)
    {
        GameOver = true;
        OnGameOver?.Invoke(result);
    }

    public void OnMoveMade(Move move)
    {
        PieceColor sideToMove = _board.SideToMove;

        OnMoveApplied?.Invoke(move, CheckDetector.Opponent(sideToMove));

        GameResult result = EndConditionChecker.Check(sideToMove, _board);
        if (result != GameResult.None)
        {
            GameOver = true;
            OnGameOver?.Invoke(result);
            return;
        }

        string posKey = _board.GetPositionKey();
        _positionHistory.TryGetValue(posKey, out int count);
        _positionHistory[posKey] = count + 1;
        if (count + 1 >= 3)
        {
            GameOver = true;
            OnGameOver?.Invoke(GameResult.DrawRepetition);
            return;
        }

        if (CheckDetector.IsKingInCheck(sideToMove, _board))
            OnCheck?.Invoke(sideToMove);

        OnTurnChanged?.Invoke(sideToMove);
    }
}
