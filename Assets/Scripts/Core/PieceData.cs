public struct PieceData
{
    public PieceType  Type;
    public PieceColor Color;
    public bool       HasMoved;

    public PieceData(PieceType type, PieceColor color)
    {
        Type     = type;
        Color    = color;
        HasMoved = false;
    }

    public bool IsEmpty => Type == PieceType.None;

    public static readonly PieceData Empty = new PieceData(PieceType.None, PieceColor.White);

    public override string ToString()
    {
        if (IsEmpty) return ".";
        char c = Type switch
        {
            PieceType.Pawn   => 'P',
            PieceType.Knight => 'N',
            PieceType.Bishop => 'B',
            PieceType.Rook   => 'R',
            PieceType.Queen  => 'Q',
            PieceType.King   => 'K',
            _                => '.'
        };
        return Color == PieceColor.White ? char.ToUpper(c).ToString() : char.ToLower(c).ToString();
    }
}
