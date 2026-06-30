using NUnit.Framework;

[TestFixture]
public class Layer6_AISearchTests
{
    private PositionalEvaluator _pos;

    [SetUp] public void SetUp() { _pos = new PositionalEvaluator(); }

    // AlphaBeta prunes branches

    [Test] public void AlphaBeta_PruneCountIsPositiveAtDepth3()
    {
        var searcher = new AlphaBetaSearcher(_pos);
        searcher.Search(new BoardModel(), 3);
        Assert.Greater(searcher.PruneCount, 0);
    }

    // Node count grows with depth 

    [Test] public void AlphaBeta_NodeCountGrowsWithDepth()
    {
        var b  = new BoardModel();
        int n1 = new AlphaBetaSearcher(_pos).Search(b, 1).NodesVisited;
        int n2 = new AlphaBetaSearcher(_pos).Search(b, 2).NodesVisited;
        Assert.Greater(n2, n1, "More nodes visited at depth 2 than depth 1");
    }

    // Tactical correctness 

    [Test] public void AlphaBeta_CapturesUndefendedPawn_AtDepth2()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King, PieceColor.Black);
        ChessTestHelpers.Place(b, "a4", PieceType.Rook, PieceColor.White, hasMoved: true);
        ChessTestHelpers.Place(b, "e4", PieceType.Pawn, PieceColor.Black, hasMoved: true);

        var result = new AlphaBetaSearcher(_pos).Search(b, 2);

        Assert.IsTrue(result.HasMove);
        Assert.AreEqual(Square.FromAlgebraic("e4"), result.BestMove.To,
            "Rook should capture undefended pawn on e4");
    }

    [Test] public void AlphaBeta_FindsCheckmateInOne_AtDepth2()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King,  PieceColor.White);
        ChessTestHelpers.Place(b, "b6", PieceType.Queen, PieceColor.White, hasMoved: true);
        ChessTestHelpers.Place(b, "a1", PieceType.Rook,  PieceColor.White, hasMoved: true);
        ChessTestHelpers.Place(b, "a8", PieceType.King,  PieceColor.Black);

        var result = new AlphaBetaSearcher(_pos).Search(b, 2);

        Assert.IsTrue(result.HasMove);
        Assert.Greater(result.Score, 99000, "Forced mate should score near 100000");
    }

    [Test] public void AlphaBeta_ReturnedMoveIsLegal()
    {
        var b      = new BoardModel();
        var result = new AlphaBetaSearcher(_pos).Search(b, 2);

        if (!result.HasMove) return; // position is already terminal

        var legal = LegalMoveFilter.GetAllLegalMoves(b.SideToMove, b);
        bool found = legal.Exists(m => m.From == result.BestMove.From && m.To == result.BestMove.To);
        Assert.IsTrue(found, $"AI returned illegal move: {result.BestMove}");
    }

    [Test] public void AlphaBeta_DoesNotCrash_WhenKingIsInCheck()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.Rook, PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "d8", PieceType.Rook, PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "f8", PieceType.Rook, PieceColor.Black, hasMoved: true);
        ChessTestHelpers.Place(b, "a8", PieceType.King, PieceColor.Black);

        Assert.DoesNotThrow(() => new AlphaBetaSearcher(_pos).Search(b, 1));
    }

    // Iterative Deepening 

    [Test] public void ID_TimeLimitExpired_StillReturnsAMove()
    {
        var id     = new IterativeDeepeningSearcher(_pos);
        var result = id.Search(new BoardModel(), 10, timeLimitMs: 5);
        Assert.IsTrue(result.HasMove, "ID must return a move even when time expires before maxDepth");
    }

    [Test] public void ID_AccumulatesPruneCountAcrossDepths()
    {
        var id = new IterativeDeepeningSearcher(_pos);
        id.Search(new BoardModel(), 3);
        Assert.Greater(id.TotalPruneCount, 0, "TotalPruneCount must accumulate across all ID iterations");
    }

    // Legal move validation

    [Test] public void LegalMoveValidation_RejectsIllegalMove()
    {
        var board = new BoardModel();
        var fake = new Move(Square.FromAlgebraic("a4"), Square.FromAlgebraic("a5"));
        var legal = LegalMoveFilter.GetAllLegalMoves(board.SideToMove, board);
        bool isLegal = legal.Exists(m =>
            m.From == fake.From && m.To == fake.To && m.IsPromotion == fake.IsPromotion);
        Assert.IsFalse(isLegal, "Move from an empty square must be rejected as illegal");
    }

    [Test] public void LegalMoveValidation_AcceptsLegalMove()
    {
        var board = new BoardModel();
        var move = new Move(Square.FromAlgebraic("e2"), Square.FromAlgebraic("e4"));
        var legal = LegalMoveFilter.GetAllLegalMoves(board.SideToMove, board);
        bool isLegal = legal.Exists(m =>
            m.From == move.From && m.To == move.To && m.IsPromotion == move.IsPromotion);
        Assert.IsTrue(isLegal, "e2→e4 must be accepted as legal from the starting position");
    }
}
