using NUnit.Framework;

[TestFixture]
public class Layer3_CheckDetectionTests
{
    // Sliding piece (Rook/Queen) 

    [Test] public void RookAttack_StraightLineUnblocked()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "a4", PieceType.Rook, PieceColor.Black, hasMoved: true);

        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("d4"), PieceColor.Black, b));
    }

    [Test] public void RookAttack_BlockedByIntermediatePiece()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "a4", PieceType.Rook, PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "c4", PieceType.Pawn, PieceColor.White); // blocker

        Assert.IsFalse(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("d4"), PieceColor.Black, b));
    }

    [Test] public void QueenAttack_StraightLine()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "a4", PieceType.Queen, PieceColor.Black, hasMoved: true);

        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("h4"), PieceColor.Black, b));
    }

    [Test] public void QueenAttack_Diagonal()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "a1", PieceType.Queen, PieceColor.Black);

        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("d4"), PieceColor.Black, b));
        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("h8"), PieceColor.Black, b));
    }

    [Test] public void BishopAttack_DiagonalUnblocked()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "a1", PieceType.Bishop, PieceColor.White);

        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("h8"), PieceColor.White, b));
    }

    [Test] public void BishopAttack_BlockedByPiece()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "a1", PieceType.Bishop, PieceColor.White);
        ChessTestHelpers.Place(b, "d4", PieceType.Pawn,   PieceColor.White); // blocker

        Assert.IsFalse(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("h8"), PieceColor.White, b));
    }

    // Knight 

    [Test] public void KnightAttack_CorrectLShapeSquares()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "b1", PieceType.Knight, PieceColor.White);

        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("c3"), PieceColor.White, b), "c3 attacked");
        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("a3"), PieceColor.White, b), "a3 attacked");
        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("d2"), PieceColor.White, b), "d2 attacked (+2,+1)");
        Assert.IsFalse(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("c2"), PieceColor.White, b), "c2 not attacked (diagonal only)");
        Assert.IsFalse(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("b3"), PieceColor.White, b), "b3 not attacked (same file)");
    }

    [Test] public void KnightAttack_JumpsOverInterveningPieces()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "b1", PieceType.Knight, PieceColor.White);
        foreach (var sq in new[] { "a1","c1","a2","b2","c2" })
            ChessTestHelpers.Place(b, sq, PieceType.Pawn, PieceColor.White);

        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("c3"), PieceColor.White, b),
            "Knight still attacks c3 despite surrounding pawns");
    }

    // King 

    [Test] public void KingAttack_AdjacentSquaresOnly()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "d4", PieceType.King, PieceColor.Black);

        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("e4"), PieceColor.Black, b), "e4 adjacent");
        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("d5"), PieceColor.Black, b), "d5 adjacent");
        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("e5"), PieceColor.Black, b), "e5 diagonal");
        Assert.IsFalse(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("f4"), PieceColor.Black, b), "f4 two away");
        Assert.IsFalse(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("d6"), PieceColor.Black, b), "d6 two away");
    }

    // Pawn 

    [Test] public void PawnAttack_WhitePawnAttacksDiagonallyForward()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e4", PieceType.Pawn, PieceColor.White, hasMoved: true);

        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("d5"), PieceColor.White, b), "Attacks d5");
        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("f5"), PieceColor.White, b), "Attacks f5");
        Assert.IsFalse(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("e5"), PieceColor.White, b), "Does NOT attack e5");
        Assert.IsFalse(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("d3"), PieceColor.White, b), "Does NOT attack backward d3");
    }

    [Test] public void PawnAttack_BlackPawnAttacksDiagonallyForward()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e5", PieceType.Pawn, PieceColor.Black, hasMoved: true);

        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("d4"), PieceColor.Black, b), "Attacks d4");
        Assert.IsTrue(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("f4"), PieceColor.Black, b), "Attacks f4");
        Assert.IsFalse(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("e4"), PieceColor.Black, b), "Does NOT attack e4");
        Assert.IsFalse(CheckDetector.IsSquareAttacked(Square.FromAlgebraic("d6"), PieceColor.Black, b), "Does NOT attack backward d6");
    }

    // IsKingInCheck 

    [Test] public void IsKingInCheck_FalseInStartingPosition()
    {
        var b = new BoardModel();
        Assert.IsFalse(CheckDetector.IsKingInCheck(PieceColor.White, b));
        Assert.IsFalse(CheckDetector.IsKingInCheck(PieceColor.Black, b));
    }

    [Test] public void IsKingInCheck_TrueWhenRookOnSameFile()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.Rook, PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "a8", PieceType.King, PieceColor.Black);

        Assert.IsTrue(CheckDetector.IsKingInCheck(PieceColor.White, b));
    }

    [Test] public void IsKingInCheck_TrueWhenKnightGivesCheck()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King,   PieceColor.White);
        ChessTestHelpers.Place(b, "d3", PieceType.Knight, PieceColor.Black, hasMoved: true); // d3 attacks e1
        ChessTestHelpers.Place(b, "a8", PieceType.King,   PieceColor.Black);

        Assert.IsTrue(CheckDetector.IsKingInCheck(PieceColor.White, b));
    }

    [Test] public void IsKingInCheck_TrueWhenPawnGivesCheck()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e4", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "d5", PieceType.Pawn, PieceColor.Black, hasMoved: true); // attacks e4
        ChessTestHelpers.Place(b, "a8", PieceType.King, PieceColor.Black);

        Assert.IsTrue(CheckDetector.IsKingInCheck(PieceColor.White, b));
    }

    [Test] public void IsKingInCheck_TrueInDoubleCheck()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e4", PieceType.King,   PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.Rook,   PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "a8", PieceType.Bishop, PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "h1", PieceType.King,   PieceColor.Black);

        Assert.IsTrue(CheckDetector.IsKingInCheck(PieceColor.White, b));
    }

    [Test] public void IsKingInCheck_FalseWhenCheckIsBlocked()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "e4", PieceType.Pawn, PieceColor.White, hasMoved: true); // blocks
        ChessTestHelpers.Place(b, "e8", PieceType.Rook, PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "a8", PieceType.King, PieceColor.Black);

        Assert.IsFalse(CheckDetector.IsKingInCheck(PieceColor.White, b));
    }
}
