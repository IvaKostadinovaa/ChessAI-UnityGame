public struct Move
{
    public Square    From;
    public Square    To;
    public PieceData Captured;
    public PieceType PromotionPiece;

    public bool IsEnPassant  { get; private set; }
    public bool IsCastle     { get; private set; }
    public bool IsPromotion  { get; private set; }

    public Move(Square from, Square to, PieceData captured = default)
    {
        From           = from;
        To             = to;
        Captured       = captured;
        PromotionPiece = PieceType.None;
        IsEnPassant    = false;
        IsCastle       = false;
        IsPromotion    = false;
    }

    public static Move EnPassant(Square from, Square to, PieceData captured)
    {
        var m = new Move(from, to, captured);
        m.IsEnPassant = true;
        return m;
    }

    public static Move Castle(Square from, Square to)
    {
        var m = new Move(from, to);
        m.IsCastle = true;
        return m;
    }

    public static Move Promotion(Square from, Square to, PieceType promoteTo, PieceData captured = default)
    {
        var m = new Move(from, to, captured);
        m.IsPromotion    = true;
        m.PromotionPiece = promoteTo;
        return m;
    }

    public override string ToString() =>
        $"{From}{To}{(IsPromotion ? PromotionPiece.ToString()[0].ToString().ToLower() : "")}";
}
