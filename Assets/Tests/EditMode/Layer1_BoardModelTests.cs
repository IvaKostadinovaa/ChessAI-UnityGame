using NUnit.Framework;

[TestFixture]
public class Layer1_BoardModelTests
{
    // Starting position 

    [Test] public void StartingPosition_WhiteRooksOnA1H1()
    {
        var b = new BoardModel();
        Assert.AreEqual(PieceType.Rook,  b.GetPiece(Square.FromAlgebraic("a1")).Type);
        Assert.AreEqual(PieceColor.White, b.GetPiece(Square.FromAlgebraic("a1")).Color);
        Assert.AreEqual(PieceType.Rook,  b.GetPiece(Square.FromAlgebraic("h1")).Type);
        Assert.AreEqual(PieceColor.White, b.GetPiece(Square.FromAlgebraic("h1")).Color);
    }

    [Test] public void StartingPosition_BlackRooksOnA8H8()
    {
        var b = new BoardModel();
        Assert.AreEqual(PieceType.Rook,  b.GetPiece(Square.FromAlgebraic("a8")).Type);
        Assert.AreEqual(PieceColor.Black, b.GetPiece(Square.FromAlgebraic("a8")).Color);
        Assert.AreEqual(PieceType.Rook,  b.GetPiece(Square.FromAlgebraic("h8")).Type);
    }

    [Test] public void StartingPosition_WhiteKingOnE1_BlackKingOnE8()
    {
        var b = new BoardModel();
        var wk = b.GetPiece(Square.FromAlgebraic("e1"));
        var bk = b.GetPiece(Square.FromAlgebraic("e8"));
        Assert.AreEqual(PieceType.King, wk.Type);  Assert.AreEqual(PieceColor.White, wk.Color);
        Assert.AreEqual(PieceType.King, bk.Type);  Assert.AreEqual(PieceColor.Black, bk.Color);
    }

    [Test] public void StartingPosition_WhiteQueenOnD1_BlackQueenOnD8()
    {
        var b = new BoardModel();
        Assert.AreEqual(PieceType.Queen,  b.GetPiece(Square.FromAlgebraic("d1")).Type);
        Assert.AreEqual(PieceColor.White, b.GetPiece(Square.FromAlgebraic("d1")).Color);
        Assert.AreEqual(PieceType.Queen,  b.GetPiece(Square.FromAlgebraic("d8")).Type);
        Assert.AreEqual(PieceColor.Black, b.GetPiece(Square.FromAlgebraic("d8")).Color);
    }

    [Test] public void StartingPosition_AllWhitePawnsOnRank2()
    {
        var b = new BoardModel();
        for (int f = 0; f < 8; f++)
        {
            Assert.AreEqual(PieceType.Pawn,  b.GetPiece(f, 1).Type,  $"file {f} rank 2");
            Assert.AreEqual(PieceColor.White, b.GetPiece(f, 1).Color, $"file {f} rank 2");
        }
    }

    [Test] public void StartingPosition_AllBlackPawnsOnRank7()
    {
        var b = new BoardModel();
        for (int f = 0; f < 8; f++)
        {
            Assert.AreEqual(PieceType.Pawn,  b.GetPiece(f, 6).Type,  $"file {f} rank 7");
            Assert.AreEqual(PieceColor.Black, b.GetPiece(f, 6).Color, $"file {f} rank 7");
        }
    }

    [Test] public void StartingPosition_Ranks3To6AreEmpty()
    {
        var b = new BoardModel();
        for (int f = 0; f < 8; f++)
            for (int r = 2; r <= 5; r++)
                Assert.IsTrue(b.GetPiece(f, r).IsEmpty, $"({f},{r}) should be empty");
    }

    // Board addressing 

    [Test] public void AllCornerSquaresAddressable()
    {
        var b = new BoardModel();
        Assert.DoesNotThrow(() => b.GetPiece(new Square(0, 0)));
        Assert.DoesNotThrow(() => b.GetPiece(new Square(7, 0)));
        Assert.DoesNotThrow(() => b.GetPiece(new Square(0, 7)));
        Assert.DoesNotThrow(() => b.GetPiece(new Square(7, 7)));
    }

    [Test] public void SquareValidity_InsideBoard()
    {
        Assert.IsTrue(new Square(0, 0).IsValid);
        Assert.IsTrue(new Square(7, 7).IsValid);
        Assert.IsTrue(new Square(3, 4).IsValid);
    }

    [Test] public void SquareValidity_OutsideBoard()
    {
        Assert.IsFalse(new Square(-1,  0).IsValid);
        Assert.IsFalse(new Square( 0, -1).IsValid);
        Assert.IsFalse(new Square( 8,  0).IsValid);
        Assert.IsFalse(new Square( 0,  8).IsValid);
    }

    // ApplyMove 

    [Test] public void ApplyMove_SourceBecomesEmpty()
    {
        var b = new BoardModel();
        var from = Square.FromAlgebraic("e2");
        b.ApplyMove(new Move(from, Square.FromAlgebraic("e4")));
        Assert.IsTrue(b.GetPiece(from).IsEmpty);
    }

    [Test] public void ApplyMove_DestinationContainsMovedPiece()
    {
        var b = new BoardModel();
        b.ApplyMove(new Move(Square.FromAlgebraic("e2"), Square.FromAlgebraic("e4")));
        var dest = b.GetPiece(Square.FromAlgebraic("e4"));
        Assert.AreEqual(PieceType.Pawn,  dest.Type);
        Assert.AreEqual(PieceColor.White, dest.Color);
    }

    [Test] public void ApplyMove_CaptureRemovesEnemyPiece()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King, PieceColor.Black);
        ChessTestHelpers.Place(b, "a1", PieceType.Rook, PieceColor.White);
        ChessTestHelpers.Place(b, "a5", PieceType.Pawn, PieceColor.Black);

        var captured = b.GetPiece(Square.FromAlgebraic("a5"));
        b.ApplyMove(new Move(Square.FromAlgebraic("a1"), Square.FromAlgebraic("a5"), captured));

        Assert.IsTrue(b.GetPiece(Square.FromAlgebraic("a1")).IsEmpty);
        Assert.AreEqual(PieceType.Rook,  b.GetPiece(Square.FromAlgebraic("a5")).Type);
        Assert.AreEqual(PieceColor.White, b.GetPiece(Square.FromAlgebraic("a5")).Color);
    }

    [Test] public void ApplyMove_SideToMoveFlips()
    {
        var b = new BoardModel();
        Assert.AreEqual(PieceColor.White, b.SideToMove);
        b.ApplyMove(new Move(Square.FromAlgebraic("e2"), Square.FromAlgebraic("e4")));
        Assert.AreEqual(PieceColor.Black, b.SideToMove);
        b.ApplyMove(new Move(Square.FromAlgebraic("e7"), Square.FromAlgebraic("e5")));
        Assert.AreEqual(PieceColor.White, b.SideToMove);
    }

    // Clone 

    [Test] public void Clone_MutatingCloneDoesNotAffectOriginal()
    {
        var b     = new BoardModel();
        var clone = b.Clone();
        clone.ApplyMove(new Move(Square.FromAlgebraic("e2"), Square.FromAlgebraic("e4")));

        Assert.AreEqual(PieceType.Pawn, b.GetPiece(Square.FromAlgebraic("e2")).Type, "Original pawn still on e2");
        Assert.IsTrue(b.GetPiece(Square.FromAlgebraic("e4")).IsEmpty, "Original e4 still empty");
    }

    [Test] public void Clone_PreservesAllPiecePositions()
    {
        var b     = new BoardModel();
        var clone = b.Clone();
        for (int f = 0; f < 8; f++)
            for (int r = 0; r < 8; r++)
            {
                var orig = b.GetPiece(f, r);
                var copy = clone.GetPiece(f, r);
                Assert.AreEqual(orig.Type,  copy.Type,  $"Type at ({f},{r})");
                Assert.AreEqual(orig.Color, copy.Color, $"Color at ({f},{r})");
            }
    }

    // HasMoved flags 

    [Test] public void HasMoved_FreshPiecesAreFalse()
    {
        var b = new BoardModel();
        Assert.IsFalse(b.GetPiece(Square.FromAlgebraic("e1")).HasMoved, "White king");
        Assert.IsFalse(b.GetPiece(Square.FromAlgebraic("a1")).HasMoved, "White rook a1");
        Assert.IsFalse(b.GetPiece(Square.FromAlgebraic("e2")).HasMoved, "White e2 pawn");
    }

    [Test] public void HasMoved_TrueAfterFirstMove()
    {
        var b = new BoardModel();
        b.ApplyMove(new Move(Square.FromAlgebraic("e2"), Square.FromAlgebraic("e4")));
        Assert.IsTrue(b.GetPiece(Square.FromAlgebraic("e4")).HasMoved);
    }

    [Test] public void HasMoved_KingFlipsTrue()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King, PieceColor.Black);
        b.ApplyMove(new Move(Square.FromAlgebraic("e1"), Square.FromAlgebraic("f1")));
        Assert.IsTrue(b.GetPiece(Square.FromAlgebraic("f1")).HasMoved);
    }

    // En passant target 

    [Test] public void EnPassantTarget_SetAfterDoublePawnPush()
    {
        var b = new BoardModel();
        b.ApplyMove(new Move(Square.FromAlgebraic("e2"), Square.FromAlgebraic("e4")));
        Assert.IsTrue(b.EnPassantTarget.IsValid);
        Assert.AreEqual(Square.FromAlgebraic("e3"), b.EnPassantTarget);
    }

    [Test] public void EnPassantTarget_ClearedAfterNextMove()
    {
        var b = new BoardModel();
        b.ApplyMove(new Move(Square.FromAlgebraic("e2"), Square.FromAlgebraic("e4")));
        b.ApplyMove(new Move(Square.FromAlgebraic("d7"), Square.FromAlgebraic("d6")));
        Assert.IsFalse(b.EnPassantTarget.IsValid);
    }
}
