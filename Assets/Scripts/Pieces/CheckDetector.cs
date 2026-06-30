public static class CheckDetector
{
    public static bool IsKingInCheck(PieceColor color, BoardModel board)
    {
        Square kingSquare = board.FindKing(color);
        if (!kingSquare.IsValid) return false;
        return IsSquareAttacked(kingSquare, Opponent(color), board);
    }

    public static bool IsSquareAttacked(Square target, PieceColor byColor, BoardModel board)
    {
        return AttackedBySlidingPiece(target, byColor, board)
            || AttackedByKnight(target, byColor, board)
            || AttackedByPawn(target, byColor, board)
            || AttackedByKing(target, byColor, board);
    }

    // Sliding pieces (Rook/Queen on straights, Bishop/Queen on diagonals)
    private static bool AttackedBySlidingPiece(Square target, PieceColor byColor, BoardModel board)
    {
        (int, int)[] straightDirs = { (1,0),(-1,0),(0,1),(0,-1) };
        (int, int)[] diagonalDirs = { (1,1),(1,-1),(-1,1),(-1,-1) };

        foreach (var (df, dr) in straightDirs)
            if (SlidingAttack(target, byColor, board, df, dr, PieceType.Rook)) return true;

        foreach (var (df, dr) in diagonalDirs)
            if (SlidingAttack(target, byColor, board, df, dr, PieceType.Bishop)) return true;

        return false;
    }

    private static bool SlidingAttack(Square target, PieceColor byColor, BoardModel board,
                                      int df, int dr, PieceType lineType)
    {
        int f = target.File + df;
        int r = target.Rank + dr;
        while (f >= 0 && f < 8 && r >= 0 && r < 8)
        {
            var piece = board.GetPiece(f, r);
            if (!piece.IsEmpty)
            {
                if (piece.Color == byColor &&
                    (piece.Type == lineType || piece.Type == PieceType.Queen))
                    return true;
                break;
            }
            f += df;
            r += dr;
        }
        return false;
    }

    // Knight 
    private static bool AttackedByKnight(Square target, PieceColor byColor, BoardModel board)
    {
        (int, int)[] jumps = { (2,1),(2,-1),(-2,1),(-2,-1),(1,2),(1,-2),(-1,2),(-1,-2) };
        foreach (var (df, dr) in jumps)
        {
            int f = target.File + df;
            int r = target.Rank + dr;
            if (f < 0 || f >= 8 || r < 0 || r >= 8) continue;
            var piece = board.GetPiece(f, r);
            if (!piece.IsEmpty && piece.Color == byColor && piece.Type == PieceType.Knight)
                return true;
        }
        return false;
    }

    // Pawn (attacks diagonally, direction depends on color) 
    private static bool AttackedByPawn(Square target, PieceColor byColor, BoardModel board)
    {
        // Checks whether the target square is attacked by a pawn of the specified color.
        int dir = byColor == PieceColor.White ? -1 : 1;
        int r   = target.Rank + dir;
        if (r < 0 || r >= 8) return false;

        foreach (int df in new[] { -1, 1 })
        {
            int f = target.File + df;
            if (f < 0 || f >= 8) continue;
            var piece = board.GetPiece(f, r);
            if (!piece.IsEmpty && piece.Color == byColor && piece.Type == PieceType.Pawn)
                return true;
        }
        return false;
    }

    // King
    private static bool AttackedByKing(Square target, PieceColor byColor, BoardModel board)
    {
        (int, int)[] dirs = { (1,0),(-1,0),(0,1),(0,-1),(1,1),(1,-1),(-1,1),(-1,-1) };
        foreach (var (df, dr) in dirs)
        {
            int f = target.File + df;
            int r = target.Rank + dr;
            if (f < 0 || f >= 8 || r < 0 || r >= 8) continue;
            var piece = board.GetPiece(f, r);
            if (!piece.IsEmpty && piece.Color == byColor && piece.Type == PieceType.King)
                return true;
        }
        return false;
    }

    public static PieceColor Opponent(PieceColor color) =>
        color == PieceColor.White ? PieceColor.Black : PieceColor.White;
}
