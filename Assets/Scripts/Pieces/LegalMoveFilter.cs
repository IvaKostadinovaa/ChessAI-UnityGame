using System.Collections.Generic;

public static class LegalMoveFilter
{
    public static List<Move> GetLegalMoves(Square sq, BoardModel board)
    {
        var pseudo = MoveGenerator.GeneratePseudoLegal(sq, board);
        PieceData piece = board.GetPiece(sq);
        if (piece.Type == PieceType.King)
            pseudo.AddRange(SpecialMoveRules.GetCastlingMoves(piece.Color, board));
        return Filter(pseudo, board);
    }

    public static List<Move> GetAllLegalMoves(PieceColor color, BoardModel board)
    {
        var pseudo = MoveGenerator.GenerateAllPseudoLegal(color, board);
        pseudo.AddRange(SpecialMoveRules.GetCastlingMoves(color, board));
        return Filter(pseudo, board);
    }

    public static bool HasAnyLegalMove(PieceColor color, BoardModel board)
    {
        var pseudo = MoveGenerator.GenerateAllPseudoLegal(color, board);
        pseudo.AddRange(SpecialMoveRules.GetCastlingMoves(color, board));
        foreach (var move in pseudo)
        {
            BoardModel clone = board.Clone();
            clone.ApplyMove(move);
            if (!CheckDetector.IsKingInCheck(color, clone))
                return true;
        }
        return false;
    }

    private static List<Move> Filter(List<Move> pseudoMoves, BoardModel board)
    {
        var legal = new List<Move>();
        PieceColor moving = board.SideToMove;

        foreach (var move in pseudoMoves)
        {
            BoardModel clone = board.Clone();
            clone.ApplyMove(move);
            if (!CheckDetector.IsKingInCheck(moving, clone))
                legal.Add(move);
        }

        return legal;
    }
}
