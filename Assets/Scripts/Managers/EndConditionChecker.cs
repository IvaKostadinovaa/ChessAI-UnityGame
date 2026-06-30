public enum GameResult { None, WhiteWins, BlackWins, Stalemate, Draw, DrawRepetition, WhiteResigns, BlackResigns, WhiteWinsOnTime, BlackWinsOnTime }

public static class EndConditionChecker
{
    public static GameResult Check(PieceColor sideToMove, BoardModel board)
    {
        bool hasLegalMove = LegalMoveFilter.HasAnyLegalMove(sideToMove, board);
        bool inCheck      = CheckDetector.IsKingInCheck(sideToMove, board);

        if (!hasLegalMove && inCheck)
            return sideToMove == PieceColor.White ? GameResult.BlackWins : GameResult.WhiteWins;

        if (!hasLegalMove && !inCheck)
            return GameResult.Stalemate;

        if (IsInsufficientMaterial(board))
            return GameResult.Draw;

        if (board.HalfMoveClock >= 100)
            return GameResult.Draw;

        return GameResult.None;
    }

    private static bool IsInsufficientMaterial(BoardModel board)
    {
        int whitePieces = 0, blackPieces = 0;
        bool whiteHasPawn = false, blackHasPawn = false;
        bool whiteHasMajor = false, blackHasMajor = false;

        for (int f = 0; f < 8; f++)
        {
            for (int r = 0; r < 8; r++)
            {
                var p = board.GetPiece(f, r);
                if (p.IsEmpty || p.Type == PieceType.King) continue;

                if (p.Color == PieceColor.White)
                {
                    whitePieces++;
                    if (p.Type == PieceType.Pawn) whiteHasPawn  = true;
                    if (p.Type == PieceType.Rook || p.Type == PieceType.Queen) whiteHasMajor = true;
                }
                else
                {
                    blackPieces++;
                    if (p.Type == PieceType.Pawn) blackHasPawn  = true;
                    if (p.Type == PieceType.Rook || p.Type == PieceType.Queen) blackHasMajor = true;
                }
            }
        }

        // Neither side has enough material to force checkmate.
        if (!whiteHasPawn && !whiteHasMajor && whitePieces <= 1 &&
            !blackHasPawn && !blackHasMajor && blackPieces <= 1) return true;

        return false;
    }
}
