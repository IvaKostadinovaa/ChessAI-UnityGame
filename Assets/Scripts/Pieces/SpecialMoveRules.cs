using System.Collections.Generic;

public static class SpecialMoveRules
{
    public static List<Move> GetCastlingMoves(PieceColor color, BoardModel board)
    {
        var moves = new List<Move>();

        int rank = color == PieceColor.White ? 0 : 7;
        var kingSq = new Square(4, rank);
        PieceData king = board.GetPiece(kingSq);

        if (king.IsEmpty || king.Type != PieceType.King || king.HasMoved) return moves;
        if (CheckDetector.IsKingInCheck(color, board)) return moves;

        PieceColor opponent = CheckDetector.Opponent(color);

        // King-side castling 
        var rookKSq = new Square(7, rank);
        PieceData rookK = board.GetPiece(rookKSq);
        if (!rookK.IsEmpty && rookK.Type == PieceType.Rook && !rookK.HasMoved)
        {
            bool pathClear = board.GetPiece(5, rank).IsEmpty && board.GetPiece(6, rank).IsEmpty;
            bool pathSafe  = !CheckDetector.IsSquareAttacked(new Square(5, rank), opponent, board)
                          && !CheckDetector.IsSquareAttacked(new Square(6, rank), opponent, board);
            if (pathClear && pathSafe)
                moves.Add(Move.Castle(kingSq, new Square(6, rank)));
        }

        // Queen-side castling 
        var rookQSq = new Square(0, rank);
        PieceData rookQ = board.GetPiece(rookQSq);
        if (!rookQ.IsEmpty && rookQ.Type == PieceType.Rook && !rookQ.HasMoved)
        {
            bool pathClear = board.GetPiece(1, rank).IsEmpty
                          && board.GetPiece(2, rank).IsEmpty
                          && board.GetPiece(3, rank).IsEmpty;
            bool pathSafe  = !CheckDetector.IsSquareAttacked(new Square(3, rank), opponent, board)
                          && !CheckDetector.IsSquareAttacked(new Square(2, rank), opponent, board);
            if (pathClear && pathSafe)
                moves.Add(Move.Castle(kingSq, new Square(2, rank)));
        }

        return moves;
    }
}
