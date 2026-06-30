using System.Collections.Generic;

public static class MoveGenerator
{
    // Directions 
    private static readonly (int, int)[] RookDirs   = { (1,0),(-1,0),(0,1),(0,-1) };
    private static readonly (int, int)[] BishopDirs = { (1,1),(1,-1),(-1,1),(-1,-1) };
    private static readonly (int, int)[] QueenDirs  = { (1,0),(-1,0),(0,1),(0,-1),(1,1),(1,-1),(-1,1),(-1,-1) };
    private static readonly (int, int)[] KnightJumps = { (2,1),(2,-1),(-2,1),(-2,-1),(1,2),(1,-2),(-1,2),(-1,-2) };
    private static readonly (int, int)[] KingDirs   = { (1,0),(-1,0),(0,1),(0,-1),(1,1),(1,-1),(-1,1),(-1,-1) };

    public static List<Move> GeneratePseudoLegal(Square sq, BoardModel board)
    {
        PieceData piece = board.GetPiece(sq);
        if (piece.IsEmpty) return new List<Move>();

        return piece.Type switch
        {
            PieceType.Rook   => Sliding(sq, piece, board, RookDirs),
            PieceType.Bishop => Sliding(sq, piece, board, BishopDirs),
            PieceType.Queen  => Sliding(sq, piece, board, QueenDirs),
            PieceType.Knight => KnightMoves(sq, piece, board),
            PieceType.King   => KingMoves(sq, piece, board),
            PieceType.Pawn   => PawnMoves(sq, piece, board),
            _                => new List<Move>()
        };
    }

    public static List<Move> GenerateAllPseudoLegal(PieceColor color, BoardModel board)
    {
        var moves = new List<Move>();
        for (int f = 0; f < 8; f++)
            for (int r = 0; r < 8; r++)
            {
                var sq = new Square(f, r);
                if (!board.GetPiece(sq).IsEmpty && board.GetPiece(sq).Color == color)
                    moves.AddRange(GeneratePseudoLegal(sq, board));
            }
        return moves;
    }

    // Sliding pieces (Rook, Bishop, Queen) 
    private static List<Move> Sliding(Square from, PieceData piece, BoardModel board, (int, int)[] dirs)
    {
        var moves = new List<Move>();
        foreach (var (df, dr) in dirs)
        {
            int f = from.File + df;
            int r = from.Rank + dr;
            while (f >= 0 && f < 8 && r >= 0 && r < 8)
            {
                var target = board.GetPiece(f, r);
                if (target.IsEmpty)
                {
                    moves.Add(new Move(from, new Square(f, r)));
                }
                else
                {
                    if (target.Color != piece.Color)
                        moves.Add(new Move(from, new Square(f, r), target));
                    break;
                }
                f += df;
                r += dr;
            }
        }
        return moves;
    }

    // Knight 
    private static List<Move> KnightMoves(Square from, PieceData piece, BoardModel board)
    {
        var moves = new List<Move>();
        foreach (var (df, dr) in KnightJumps)
        {
            int f = from.File + df;
            int r = from.Rank + dr;
            if (f < 0 || f >= 8 || r < 0 || r >= 8) continue;
            var target = board.GetPiece(f, r);
            if (target.IsEmpty)
                moves.Add(new Move(from, new Square(f, r)));
            else if (target.Color != piece.Color)
                moves.Add(new Move(from, new Square(f, r), target));
        }
        return moves;
    }

    // King (without castling — handled in SpecialMoveRules)
    private static List<Move> KingMoves(Square from, PieceData piece, BoardModel board)
    {
        var moves = new List<Move>();
        foreach (var (df, dr) in KingDirs)
        {
            int f = from.File + df;
            int r = from.Rank + dr;
            if (f < 0 || f >= 8 || r < 0 || r >= 8) continue;
            var target = board.GetPiece(f, r);
            if (target.IsEmpty)
                moves.Add(new Move(from, new Square(f, r)));
            else if (target.Color != piece.Color)
                moves.Add(new Move(from, new Square(f, r), target));
        }
        return moves;
    }

    // Pawn 
    private static List<Move> PawnMoves(Square from, PieceData piece, BoardModel board)
    {
        var moves = new List<Move>();
        int dir         = piece.Color == PieceColor.White ? 1 : -1;
        int startRank   = piece.Color == PieceColor.White ? 1 : 6;
        int promoteRank = piece.Color == PieceColor.White ? 7 : 0;

        // One step forward
        int r1 = from.Rank + dir;
        if (r1 >= 0 && r1 < 8 && board.GetPiece(from.File, r1).IsEmpty)
        {
            AddPawnMove(moves, from, new Square(from.File, r1), promoteRank);

            // Two steps from starting rank
            int r2 = from.Rank + dir * 2;
            if (from.Rank == startRank && board.GetPiece(from.File, r2).IsEmpty)
                moves.Add(new Move(from, new Square(from.File, r2)));
        }

        // Diagonal captures
        foreach (int df in new[] { -1, 1 })
        {
            int cf = from.File + df;
            int cr = from.Rank + dir;
            if (cf < 0 || cf >= 8 || cr < 0 || cr >= 8) continue;

            var target = board.GetPiece(cf, cr);
            var capSq  = new Square(cf, cr);

            if (!target.IsEmpty && target.Color != piece.Color)
                AddPawnMove(moves, from, capSq, promoteRank, target);

            // En passant
            if (board.EnPassantTarget.IsValid && board.EnPassantTarget == capSq)
            {
                int epCaptureRank = piece.Color == PieceColor.White ? cr - 1 : cr + 1;
                var epCaptured    = board.GetPiece(cf, epCaptureRank);
                moves.Add(Move.EnPassant(from, capSq, epCaptured));
            }
        }

        return moves;
    }

    private static void AddPawnMove(List<Move> moves, Square from, Square to, int promoteRank, PieceData captured = default)
    {
        if (to.Rank == promoteRank)
        {
            moves.Add(Move.Promotion(from, to, PieceType.Queen,  captured));
            moves.Add(Move.Promotion(from, to, PieceType.Rook,   captured));
            moves.Add(Move.Promotion(from, to, PieceType.Bishop, captured));
            moves.Add(Move.Promotion(from, to, PieceType.Knight, captured));
        }
        else
        {
            moves.Add(new Move(from, to, captured));
        }
    }
}
