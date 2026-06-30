using System.Collections.Generic;

public static class ChessTestHelpers
{
    public static BoardModel EmptyBoard()
    {
        var board = new BoardModel();
        for (int f = 0; f < 8; f++)
            for (int r = 0; r < 8; r++)
                board.SetPiece(new Square(f, r), PieceData.Empty);
        return board;
    }

    public static void Place(BoardModel board, string sq, PieceType type, PieceColor color, bool hasMoved = false)
    {
        var piece = new PieceData(type, color);
        piece.HasMoved = hasMoved;
        board.SetPiece(Square.FromAlgebraic(sq), piece);
    }

    public static bool HasMoveTo(List<Move> moves, string to)
    {
        var target = Square.FromAlgebraic(to);
        foreach (var m in moves)
            if (m.To == target) return true;
        return false;
    }

    public static bool HasMoveFromTo(List<Move> moves, string from, string to)
    {
        var f = Square.FromAlgebraic(from);
        var t = Square.FromAlgebraic(to);
        foreach (var m in moves)
            if (m.From == f && m.To == t) return true;
        return false;
    }
}
