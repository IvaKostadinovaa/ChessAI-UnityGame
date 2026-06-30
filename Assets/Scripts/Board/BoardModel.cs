using System.Text;

public class BoardModel
{
    private PieceData[,] _board = new PieceData[8, 8];

    public Square EnPassantTarget { get; private set; } = new Square(-1, -1);
    public PieceColor SideToMove  { get; private set; } = PieceColor.White;
    public int HalfMoveClock      { get; private set; } = 0;
    public int FullMoveNumber     { get; private set; } = 1;

    public BoardModel()
    {
        SetupStartingPosition();
    }

    private BoardModel(PieceData[,] board, Square enPassant, PieceColor side, int halfMove, int fullMove)
    {
        for (int f = 0; f < 8; f++)
            for (int r = 0; r < 8; r++)
                _board[f, r] = board[f, r];
        EnPassantTarget = enPassant;
        SideToMove      = side;
        HalfMoveClock   = halfMove;
        FullMoveNumber  = fullMove;
    }

    public PieceData GetPiece(Square sq) => _board[sq.File, sq.Rank];
    public PieceData GetPiece(int file, int rank) => _board[file, rank];

    public void SetPiece(Square sq, PieceData piece) => _board[sq.File, sq.Rank] = piece;
    public void SetPiece(int file, int rank, PieceData piece) => _board[file, rank] = piece;

    public bool IsEmpty(Square sq) => _board[sq.File, sq.Rank].IsEmpty;

    public void ApplyMove(Move move)
    {
        PieceData piece = GetPiece(move.From);
        piece.HasMoved = true;

        // En passant capture
        if (move.IsEnPassant)
        {
            int capturedRank = piece.Color == PieceColor.White ? move.To.Rank - 1 : move.To.Rank + 1;
            SetPiece(new Square(move.To.File, capturedRank), PieceData.Empty);
        }

        // Castling - move the rook
        if (move.IsCastle)
        {
            bool kingSide = move.To.File == 6;
            int rookFromFile = kingSide ? 7 : 0;
            int rookToFile   = kingSide ? 5 : 3;
            int rank = piece.Color == PieceColor.White ? 0 : 7;
            PieceData rook = GetPiece(rookFromFile, rank);
            rook.HasMoved = true;
            SetPiece(rookFromFile, rank, PieceData.Empty);
            SetPiece(rookToFile,   rank, rook);
        }

        // Move piece
        SetPiece(move.From, PieceData.Empty);

        // Promotion
        if (move.IsPromotion)
            piece.Type = move.PromotionPiece;

        SetPiece(move.To, piece);

        // Update en passant target
        bool isPawnDoubleStep = piece.Type == PieceType.Pawn &&
                                System.Math.Abs(move.To.Rank - move.From.Rank) == 2;
        EnPassantTarget = isPawnDoubleStep
            ? new Square(move.From.File, (move.From.Rank + move.To.Rank) / 2)
            : new Square(-1, -1);

        HalfMoveClock = (piece.Type == PieceType.Pawn || !move.Captured.IsEmpty)
            ? 0
            : HalfMoveClock + 1;

        if (SideToMove == PieceColor.Black) FullMoveNumber++;
        SideToMove = SideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }

    public BoardModel Clone()
    {
        return new BoardModel(_board, EnPassantTarget, SideToMove, HalfMoveClock, FullMoveNumber);
    }

    public string GetPositionKey()
    {
        var sb = new StringBuilder(80);
        for (int r = 0; r < 8; r++)
            for (int f = 0; f < 8; f++)
            {
                var p = _board[f, r];
                sb.Append(p.IsEmpty ? 0 : (int)p.Color * 7 + (int)p.Type);
                sb.Append(',');
            }
        sb.Append(SideToMove == PieceColor.White ? 'W' : 'B');
        if (CanCastle(PieceColor.White, 7)) sb.Append('K');
        if (CanCastle(PieceColor.White, 0)) sb.Append('Q');
        if (CanCastle(PieceColor.Black, 7)) sb.Append('k');
        if (CanCastle(PieceColor.Black, 0)) sb.Append('q');
        if (EnPassantTarget.IsValid && IsEnPassantCapturable())
            sb.Append(EnPassantTarget.File).Append(EnPassantTarget.Rank);
        return sb.ToString();
    }

    // Only include en passant if a pawn could actually capture it ,otherwise identical positions get different keys.
    private bool IsEnPassantCapturable()
    {
        int capturingRank = SideToMove == PieceColor.White ? EnPassantTarget.Rank - 1 : EnPassantTarget.Rank + 1;
        if (capturingRank < 0 || capturingRank >= 8) return false;

        foreach (int df in new[] { -1, 1 })
        {
            int f = EnPassantTarget.File + df;
            if (f < 0 || f >= 8) continue;
            var piece = GetPiece(f, capturingRank);
            if (!piece.IsEmpty && piece.Color == SideToMove && piece.Type == PieceType.Pawn)
                return true;
        }
        return false;
    }

    private bool CanCastle(PieceColor color, int rookFile)
    {
        int rank = color == PieceColor.White ? 0 : 7;
        var king = GetPiece(4, rank);
        var rook = GetPiece(rookFile, rank);
        return !king.HasMoved && king.Type == PieceType.King
            && !rook.HasMoved && rook.Type == PieceType.Rook && rook.Color == color;
    }

    public Square FindKing(PieceColor color)
    {
        for (int f = 0; f < 8; f++)
            for (int r = 0; r < 8; r++)
                if (_board[f, r].Type == PieceType.King && _board[f, r].Color == color)
                    return new Square(f, r);
        return new Square(-1, -1);
    }

    private void SetupStartingPosition()
    {
        for (int f = 0; f < 8; f++)
            for (int r = 0; r < 8; r++)
                _board[f, r] = PieceData.Empty;

        PieceType[] backRank = {
            PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen,
            PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook
        };

        for (int f = 0; f < 8; f++)
        {
            _board[f, 0] = new PieceData(backRank[f], PieceColor.White);
            _board[f, 1] = new PieceData(PieceType.Pawn, PieceColor.White);
            _board[f, 6] = new PieceData(PieceType.Pawn, PieceColor.Black);
            _board[f, 7] = new PieceData(backRank[f], PieceColor.Black);
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("  a b c d e f g h");
        for (int r = 7; r >= 0; r--)
        {
            sb.Append($"{r + 1} ");
            for (int f = 0; f < 8; f++)
                sb.Append(_board[f, r].ToString() + " ");
            sb.AppendLine($"{r + 1}");
        }
        sb.AppendLine("  a b c d e f g h");
        sb.AppendLine($"Side to move: {SideToMove}");
        sb.AppendLine($"En passant: {(EnPassantTarget.IsValid ? EnPassantTarget.ToAlgebraic() : "-")}");
        return sb.ToString();
    }
}
