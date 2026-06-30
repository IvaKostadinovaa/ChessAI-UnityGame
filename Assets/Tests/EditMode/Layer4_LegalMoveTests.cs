using NUnit.Framework;

[TestFixture]
public class Layer4_LegalMoveTests
{
    // Pinned pieces 

    [Test] public void PinnedPiece_CannotMoveOffPinLine()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "e4", PieceType.Rook, PieceColor.White, hasMoved: true);
        ChessTestHelpers.Place(b, "e8", PieceType.Rook, PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "a8", PieceType.King, PieceColor.Black);

        var moves = LegalMoveFilter.GetLegalMoves(Square.FromAlgebraic("e4"), b);

        foreach (var m in moves)
            Assert.AreEqual(4, m.To.File, $"Pinned rook must stay on e-file, moved to {m.To}");
    }

    [Test] public void PinnedPiece_CanMoveAlongPinLine()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "e4", PieceType.Rook, PieceColor.White, hasMoved: true);
        ChessTestHelpers.Place(b, "e8", PieceType.Rook, PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "a8", PieceType.King, PieceColor.Black);

        var moves = LegalMoveFilter.GetLegalMoves(Square.FromAlgebraic("e4"), b);

        Assert.IsTrue(moves.Count > 0,   "Pinned rook has legal moves along pin line");
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "e8"), "Can capture pinner");
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "e2"), "Can retreat along pin");
    }

    // King safety 

    [Test] public void King_CannotMoveIntoAttackedSquare()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e4", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "f8", PieceType.Rook, PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "a8", PieceType.King, PieceColor.Black);

        var moves = LegalMoveFilter.GetAllLegalMoves(PieceColor.White, b);

        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "f3"), "f3 attacked by rook on f8");
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "f4"), "f4 attacked by rook on f8");
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "f5"), "f5 attacked by rook on f8");
    }

    [Test] public void King_CannotCaptureDefendedPiece()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King,  PieceColor.White);
        ChessTestHelpers.Place(b, "d2", PieceType.Queen, PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "d8", PieceType.Rook,  PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "a8", PieceType.King,  PieceColor.Black);

        var moves = LegalMoveFilter.GetAllLegalMoves(PieceColor.White, b);
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "d2"), "King cannot capture defended queen");
    }

    [Test] public void InCheck_AllReturnedMovesResolveCheck()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.Rook, PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "a8", PieceType.King, PieceColor.Black);

        var moves = LegalMoveFilter.GetAllLegalMoves(PieceColor.White, b);

        Assert.IsTrue(moves.Count > 0, "There must be at least one legal move to escape check");
        foreach (var m in moves)
        {
            var clone = b.Clone();
            clone.ApplyMove(m);
            Assert.IsFalse(CheckDetector.IsKingInCheck(PieceColor.White, clone),
                $"Move {m} does not resolve check");
        }
    }

    // Castling 

    [Test] public void Castling_Kingside_AvailableWhenConditionsMet()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "h1", PieceType.Rook, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King, PieceColor.Black);

        var castles = SpecialMoveRules.GetCastlingMoves(PieceColor.White, b);
        Assert.IsTrue(castles.Exists(m => m.IsCastle && m.To == Square.FromAlgebraic("g1")));
    }

    [Test] public void Castling_Queenside_AvailableWhenConditionsMet()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "a1", PieceType.Rook, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King, PieceColor.Black);

        var castles = SpecialMoveRules.GetCastlingMoves(PieceColor.White, b);
        Assert.IsTrue(castles.Exists(m => m.IsCastle && m.To == Square.FromAlgebraic("c1")));
    }

    [Test] public void Castling_IllegalWhenKingInCheck()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "h1", PieceType.Rook, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.Rook, PieceColor.Black, hasMoved: true); // checks e1
        ChessTestHelpers.Place(b, "a8", PieceType.King, PieceColor.Black);

        Assert.AreEqual(0, SpecialMoveRules.GetCastlingMoves(PieceColor.White, b).Count);
    }

    [Test] public void Castling_IllegalWhenTransitSquareAttacked()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "h1", PieceType.Rook, PieceColor.White);
        ChessTestHelpers.Place(b, "f8", PieceType.Rook, PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "a8", PieceType.King, PieceColor.Black);

        var castles = SpecialMoveRules.GetCastlingMoves(PieceColor.White, b);
        Assert.IsFalse(castles.Exists(m => m.IsCastle && m.To == Square.FromAlgebraic("g1")));
    }

    [Test] public void Castling_IllegalWhenKingHasMoved()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White, hasMoved: true);
        ChessTestHelpers.Place(b, "h1", PieceType.Rook, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King, PieceColor.Black);

        Assert.AreEqual(0, SpecialMoveRules.GetCastlingMoves(PieceColor.White, b).Count);
    }

    [Test] public void Castling_IllegalWhenRookHasMoved()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "h1", PieceType.Rook, PieceColor.White, hasMoved: true);
        ChessTestHelpers.Place(b, "e8", PieceType.King, PieceColor.Black);

        Assert.AreEqual(0, SpecialMoveRules.GetCastlingMoves(PieceColor.White, b).Count);
    }

    [Test] public void Castling_IllegalWhenPathOccupied()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King,   PieceColor.White);
        ChessTestHelpers.Place(b, "h1", PieceType.Rook,   PieceColor.White);
        ChessTestHelpers.Place(b, "f1", PieceType.Bishop, PieceColor.White); // f1 blocks kingside
        ChessTestHelpers.Place(b, "e8", PieceType.King,   PieceColor.Black);

        var castles = SpecialMoveRules.GetCastlingMoves(PieceColor.White, b);
        Assert.IsFalse(castles.Exists(m => m.IsCastle && m.To == Square.FromAlgebraic("g1")));
    }

    [Test] public void Castling_AppliedCorrectly_KingAndRookBothMove()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "h1", PieceType.Rook, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King, PieceColor.Black);

        b.ApplyMove(Move.Castle(Square.FromAlgebraic("e1"), Square.FromAlgebraic("g1")));

        Assert.AreEqual(PieceType.King, b.GetPiece(Square.FromAlgebraic("g1")).Type, "King on g1");
        Assert.AreEqual(PieceType.Rook, b.GetPiece(Square.FromAlgebraic("f1")).Type, "Rook on f1");
        Assert.IsTrue(b.GetPiece(Square.FromAlgebraic("e1")).IsEmpty, "e1 empty");
        Assert.IsTrue(b.GetPiece(Square.FromAlgebraic("h1")).IsEmpty, "h1 empty");
    }

    // En passant timing 

    [Test] public void EnPassant_LegalImmediatelyAfterDoubleStep()
    {
        var b = new BoardModel();
        b.ApplyMove(new Move(Square.FromAlgebraic("e2"), Square.FromAlgebraic("e4")));
        b.ApplyMove(new Move(Square.FromAlgebraic("a7"), Square.FromAlgebraic("a6")));
        b.ApplyMove(new Move(Square.FromAlgebraic("e4"), Square.FromAlgebraic("e5")));
        b.ApplyMove(new Move(Square.FromAlgebraic("d7"), Square.FromAlgebraic("d5")));

        var moves = LegalMoveFilter.GetLegalMoves(Square.FromAlgebraic("e5"), b);
        Assert.IsTrue(moves.Exists(m => m.IsEnPassant), "En passant legal immediately after double step");
    }

    [Test] public void EnPassant_IllegalOneTurnLater()
    {
        var b = new BoardModel();
        b.ApplyMove(new Move(Square.FromAlgebraic("e2"), Square.FromAlgebraic("e4")));
        b.ApplyMove(new Move(Square.FromAlgebraic("a7"), Square.FromAlgebraic("a6")));
        b.ApplyMove(new Move(Square.FromAlgebraic("e4"), Square.FromAlgebraic("e5")));
        b.ApplyMove(new Move(Square.FromAlgebraic("d7"), Square.FromAlgebraic("d5"))); 
        b.ApplyMove(new Move(Square.FromAlgebraic("a2"), Square.FromAlgebraic("a3"))); 
        b.ApplyMove(new Move(Square.FromAlgebraic("h7"), Square.FromAlgebraic("h6"))); 

        var moves = LegalMoveFilter.GetLegalMoves(Square.FromAlgebraic("e5"), b);
        Assert.IsFalse(moves.Exists(m => m.IsEnPassant), "En passant expired after one turn");
    }

    [Test] public void EnPassant_CapturedPawnRemovedFromCorrectSquare()
    {
        var b = new BoardModel();
        b.ApplyMove(new Move(Square.FromAlgebraic("e2"), Square.FromAlgebraic("e4")));
        b.ApplyMove(new Move(Square.FromAlgebraic("a7"), Square.FromAlgebraic("a6")));
        b.ApplyMove(new Move(Square.FromAlgebraic("e4"), Square.FromAlgebraic("e5")));
        b.ApplyMove(new Move(Square.FromAlgebraic("d7"), Square.FromAlgebraic("d5")));

        var epMoves = LegalMoveFilter.GetLegalMoves(Square.FromAlgebraic("e5"), b);
        var ep = epMoves.Find(m => m.IsEnPassant);
        b.ApplyMove(ep);

        Assert.IsTrue(b.GetPiece(Square.FromAlgebraic("d5")).IsEmpty, "d5 pawn removed");
        Assert.AreEqual(PieceType.Pawn, b.GetPiece(Square.FromAlgebraic("d6")).Type, "White pawn on d6");
    }

    // Checkmate/ Stalemate 

    [Test] public void Checkmate_KingInCheckWithZeroLegalMoves()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "a8", PieceType.King,  PieceColor.Black);
        ChessTestHelpers.Place(b, "b6", PieceType.Queen, PieceColor.White, hasMoved: true);
        ChessTestHelpers.Place(b, "a1", PieceType.Rook,  PieceColor.White, hasMoved: true);
        ChessTestHelpers.Place(b, "e1", PieceType.King,  PieceColor.White);

        Assert.IsTrue(CheckDetector.IsKingInCheck(PieceColor.Black, b));
        Assert.IsFalse(LegalMoveFilter.HasAnyLegalMove(PieceColor.Black, b));
        Assert.AreEqual(GameResult.WhiteWins, EndConditionChecker.Check(PieceColor.Black, b));
    }

    [Test] public void Stalemate_KingNotInCheckWithZeroLegalMoves()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "a8", PieceType.King,  PieceColor.Black);
        ChessTestHelpers.Place(b, "b6", PieceType.Queen, PieceColor.White, hasMoved: true);
        ChessTestHelpers.Place(b, "c6", PieceType.King,  PieceColor.White, hasMoved: true);

        Assert.IsFalse(CheckDetector.IsKingInCheck(PieceColor.Black, b));
        Assert.IsFalse(LegalMoveFilter.HasAnyLegalMove(PieceColor.Black, b));
        Assert.AreEqual(GameResult.Stalemate, EndConditionChecker.Check(PieceColor.Black, b));
    }

    [Test] public void StartingPosition_NotGameOver()
    {
        Assert.AreEqual(GameResult.None, EndConditionChecker.Check(PieceColor.White, new BoardModel()));
    }
}
