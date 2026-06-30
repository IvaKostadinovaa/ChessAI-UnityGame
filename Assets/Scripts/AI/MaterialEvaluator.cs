public class MaterialEvaluator : IEvaluator
{
    // Standard centipawn values (1 pawn = 100)
    public static readonly int PawnValue   = 100;
    public static readonly int KnightValue = 320;
    public static readonly int BishopValue = 330;
    public static readonly int RookValue   = 500;
    public static readonly int QueenValue  = 900;
    public static readonly int KingValue   = 20000;

    public int Evaluate(BoardModel board)
    {
        int score = 0;

        for (int f = 0; f < 8; f++)
        {
            for (int r = 0; r < 8; r++)
            {
                PieceData piece = board.GetPiece(f, r);
                if (piece.IsEmpty) continue;

                int value = PieceValue(piece.Type);
                score += piece.Color == PieceColor.White ? value : -value;
            }
        }

        return score;
    }

    public static int PieceValue(PieceType type) => type switch
    {
        PieceType.Pawn   => PawnValue,
        PieceType.Knight => KnightValue,
        PieceType.Bishop => BishopValue,
        PieceType.Rook   => RookValue,
        PieceType.Queen  => QueenValue,
        PieceType.King   => KingValue,
        _                => 0
    };
}
