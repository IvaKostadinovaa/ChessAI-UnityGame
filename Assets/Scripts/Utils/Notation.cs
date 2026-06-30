public static class Notation
{
    public static string ToAlgebraic(Move move, PieceType movingPiece)
    {
        if (move.IsCastle)
            return move.To.File == 6 ? "O-O" : "O-O-O";

        string piece   = move.IsPromotion ? "" : PieceLetter(movingPiece);
        string capture = !move.Captured.IsEmpty || move.IsEnPassant ? "x" : "";
        string dest    = move.To.ToAlgebraic();
        string promo   = move.IsPromotion ? $"={PieceChar(move.PromotionPiece)}" : "";

        return $"{piece}{capture}{dest}{promo}";
    }

    private static string PieceLetter(PieceType type) => type switch
    {
        PieceType.Knight => "N",
        PieceType.Bishop => "B",
        PieceType.Rook   => "R",
        PieceType.Queen  => "Q",
        PieceType.King   => "K",
        _                => ""
    };

    private static char PieceChar(PieceType type) => type switch
    {
        PieceType.Queen  => 'Q',
        PieceType.Rook   => 'R',
        PieceType.Bishop => 'B',
        PieceType.Knight => 'N',
        _                => '?'
    };
}
