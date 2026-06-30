using NUnit.Framework;
using System.Collections.Generic;

[TestFixture]
public class Layer2_MoveGenerationTests
{
    // Rook 

    [Test] public void Rook_MovesInAllFourDirections()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "d4", PieceType.Rook,  PieceColor.White);
        ChessTestHelpers.Place(b, "e1", PieceType.King,  PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King,  PieceColor.Black);

        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("d4"), b);

        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "d1"), "South to d1");
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "d8"), "North to d8");
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "a4"), "West to a4");
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "h4"), "East to h4");
    }

    [Test] public void Rook_StopsBeforeFriendlyPiece_CannotLandOnIt()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "d4", PieceType.Rook,  PieceColor.White);
        ChessTestHelpers.Place(b, "d6", PieceType.Pawn,  PieceColor.White);
        ChessTestHelpers.Place(b, "e1", PieceType.King,  PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King,  PieceColor.Black);

        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("d4"), b);

        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves,  "d5"), "Can reach d5");
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "d6"), "Cannot land on friendly d6");
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "d7"), "Cannot go beyond blocker");
    }

    [Test] public void Rook_CanCaptureEnemyButNotGoFurther()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "d4", PieceType.Rook,  PieceColor.White);
        ChessTestHelpers.Place(b, "d7", PieceType.Pawn,  PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "e1", PieceType.King,  PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King,  PieceColor.Black);

        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("d4"), b);

        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves,  "d7"), "Can capture d7");
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "d8"), "Cannot go beyond d7");
    }

    // Bishop

    [Test] public void Bishop_MovesInAllFourDiagonals()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "d4", PieceType.Bishop, PieceColor.White);
        ChessTestHelpers.Place(b, "e1", PieceType.King,   PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King,   PieceColor.Black);

        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("d4"), b);

        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "g7"), "NE diagonal");
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "a1"), "SW diagonal");
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "a7"), "NW diagonal");
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "h8"), "NE to corner");
    }

    [Test] public void Bishop_BlockedByFriendlyPieceOnDiagonal()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "d4", PieceType.Bishop, PieceColor.White);
        ChessTestHelpers.Place(b, "f6", PieceType.Pawn,   PieceColor.White);
        ChessTestHelpers.Place(b, "e1", PieceType.King,   PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King,   PieceColor.Black);

        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("d4"), b);

        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves,  "e5"), "Can reach e5");
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "f6"), "Cannot land on friendly f6");
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "g7"), "Cannot go beyond f6");
    }

    // Queen 

    [Test] public void Queen_CombinesRookAndBishopDirections()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "d4", PieceType.Queen, PieceColor.White);
        ChessTestHelpers.Place(b, "e1", PieceType.King,  PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King,  PieceColor.Black);

        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("d4"), b);

        // Rook directions
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "d1"), "South");
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "h4"), "East");
        // Bishop directions
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "g7"), "NE diagonal");
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "a1"), "SW diagonal");
    }

    // Knight

    [Test] public void Knight_AllEightJumpsFromCenter()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "d4", PieceType.Knight, PieceColor.White);
        ChessTestHelpers.Place(b, "e1", PieceType.King,   PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King,   PieceColor.Black);

        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("d4"), b);
        Assert.AreEqual(8, moves.Count, "Knight on d4 should have 8 moves");
    }

    [Test] public void Knight_CornerA1HasTwoMoves()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "a1", PieceType.Knight, PieceColor.White);
        ChessTestHelpers.Place(b, "e1", PieceType.King,   PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King,   PieceColor.Black);

        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("a1"), b);
        Assert.AreEqual(2, moves.Count, "Knight on a1 has only 2 moves (b3, c2)");
    }

    [Test] public void Knight_JumpsOverBlockingPieces()
    {
        var b = new BoardModel();
        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("b1"), b);
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "a3"), "Jump to a3");
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "c3"), "Jump to c3");
    }

    // King 

    [Test] public void King_EightAdjacentSquaresFromCenter()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "d4", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King, PieceColor.Black);

        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("d4"), b);
        Assert.AreEqual(8, moves.Count, "King in center has 8 pseudo-legal moves");
    }

    [Test] public void King_DoesNotGenerateCastlingOrTwoSquareMoves()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King, PieceColor.Black);

        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("e1"), b);

        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "g1"), "Kingside castle not in pseudo-legal");
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "c1"), "Queenside castle not in pseudo-legal");
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "e3"), "Cannot move 2 squares");
    }

    [Test] public void King_CannotLandOnOwnPiece()
    {
        var b = new BoardModel();
        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("e1"), b);
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "e2"), "Cannot land on own pawn e2");
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "d1"), "Cannot land on own queen d1");
    }

    // Pawn 

    [Test] public void WhitePawn_OneSqForward()
    {
        var b = new BoardModel();
        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("e2"), b);
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "e3"));
    }

    [Test] public void WhitePawn_DoubleStepFromStartRank()
    {
        var b = new BoardModel();
        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("e2"), b);
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "e4"));
    }

    [Test] public void WhitePawn_DoubleStepBlockedIfSquareOccupied()
    {
        var b = new BoardModel();
        b.SetPiece(Square.FromAlgebraic("e3"), new PieceData(PieceType.Pawn, PieceColor.Black));
        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("e2"), b);
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "e3"), "Blocked by piece on e3");
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "e4"), "Double step also blocked");
    }

    [Test] public void WhitePawn_DiagonalCaptureOnlyWhenEnemyPresent()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e4", PieceType.Pawn, PieceColor.White, hasMoved: true);
        ChessTestHelpers.Place(b, "d5", PieceType.Pawn, PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King, PieceColor.Black);

        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("e4"), b);

        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves,  "d5"), "Capture d5");
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "f5"), "No capture on empty f5");
    }

    [Test] public void Pawn_NoForwardCapture()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e4", PieceType.Pawn, PieceColor.White, hasMoved: true);
        ChessTestHelpers.Place(b, "e5", PieceType.Pawn, PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King, PieceColor.Black);

        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("e4"), b);
        Assert.IsFalse(ChessTestHelpers.HasMoveTo(moves, "e5"), "Cannot capture forward");
    }

    [Test] public void BlackPawn_MovesDownwardTowardRank1()
    {
        var b = new BoardModel();
        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("e7"), b);
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "e6"), "One step");
        Assert.IsTrue(ChessTestHelpers.HasMoveTo(moves, "e5"), "Double step");
    }

    [Test] public void Pawn_EnPassantCandidateGeneratedAfterDoubleStep()
    {
        var b = new BoardModel();
        b.ApplyMove(new Move(Square.FromAlgebraic("e2"), Square.FromAlgebraic("e4")));
        b.ApplyMove(new Move(Square.FromAlgebraic("a7"), Square.FromAlgebraic("a6")));
        b.ApplyMove(new Move(Square.FromAlgebraic("e4"), Square.FromAlgebraic("e5")));
        b.ApplyMove(new Move(Square.FromAlgebraic("d7"), Square.FromAlgebraic("d5"))); // double step

        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("e5"), b);
        Assert.IsTrue(moves.Exists(m => m.IsEnPassant && m.To == Square.FromAlgebraic("d6")),
            "En passant capture on d6");
    }

    [Test] public void Pawn_FourPromotionMovesAtLastRank()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e7", PieceType.Pawn, PieceColor.White, hasMoved: true);
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "a8", PieceType.King, PieceColor.Black);

        var moves = MoveGenerator.GeneratePseudoLegal(Square.FromAlgebraic("e7"), b);
        int promoToE8 = moves.FindAll(m => m.IsPromotion && m.To == Square.FromAlgebraic("e8")).Count;
        Assert.AreEqual(4, promoToE8, "4 promotion choices to e8");
    }

    // Cross-cutting

    [Test] public void NoMoveGenerator_ReturnsOffBoardDestination()
    {
        var b = new BoardModel();
        for (int f = 0; f < 8; f++)
            for (int r = 0; r < 8; r++)
            {
                var sq = new Square(f, r);
                if (b.GetPiece(sq).IsEmpty) continue;
                foreach (var m in MoveGenerator.GeneratePseudoLegal(sq, b))
                    Assert.IsTrue(m.To.IsValid, $"Off-board destination {m.To} from {sq}");
            }
    }

    [Test] public void NoMoveGenerator_ReturnsFriendlyCapture()
    {
        var b = new BoardModel();
        for (int f = 0; f < 8; f++)
            for (int r = 0; r < 8; r++)
            {
                var sq    = new Square(f, r);
                var piece = b.GetPiece(sq);
                if (piece.IsEmpty) continue;
                foreach (var m in MoveGenerator.GeneratePseudoLegal(sq, b))
                {
                    var dest = b.GetPiece(m.To);
                    if (!dest.IsEmpty)
                        Assert.AreNotEqual(piece.Color, dest.Color,
                            $"Friendly capture: {sq} -> {m.To}");
                }
            }
    }
}
