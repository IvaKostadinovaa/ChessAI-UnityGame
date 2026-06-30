using NUnit.Framework;

[TestFixture]
public class Layer5_EvaluationTests
{
    private MaterialEvaluator  _mat;
    private PositionalEvaluator _pos;

    [SetUp] public void SetUp()
    {
        _mat = new MaterialEvaluator();
        _pos = new PositionalEvaluator();
    }

    // Material evaluator

    [Test] public void Material_StartingPositionIsZero()
    {
        Assert.AreEqual(0, _mat.Evaluate(new BoardModel()));
    }

    [Test] public void Material_RemoveWhiteQueen_ScoreNegative()
    {
        var b = new BoardModel();
        b.SetPiece(Square.FromAlgebraic("d1"), PieceData.Empty);
        Assert.AreEqual(-MaterialEvaluator.QueenValue, _mat.Evaluate(b));
    }

    [Test] public void Material_RemoveBlackQueen_ScorePositive()
    {
        var b = new BoardModel();
        b.SetPiece(Square.FromAlgebraic("d8"), PieceData.Empty);
        Assert.AreEqual(MaterialEvaluator.QueenValue, _mat.Evaluate(b));
    }

    [Test] public void Material_RemoveWhiteRook_ReducesByRookValue()
    {
        var b = new BoardModel();
        b.SetPiece(Square.FromAlgebraic("a1"), PieceData.Empty);
        Assert.AreEqual(-MaterialEvaluator.RookValue, _mat.Evaluate(b));
    }

    [Test] public void Material_RemoveBlackKnight_IncreasesByKnightValue()
    {
        var b = new BoardModel();
        b.SetPiece(Square.FromAlgebraic("b8"), PieceData.Empty);
        Assert.AreEqual(MaterialEvaluator.KnightValue, _mat.Evaluate(b));
    }

    [Test] public void Material_RemoveOneWhiteOneBla_PieceSameType_ScoreIsZero()
    {
        var b = new BoardModel();
        b.SetPiece(Square.FromAlgebraic("d1"), PieceData.Empty); // White queen
        b.SetPiece(Square.FromAlgebraic("d8"), PieceData.Empty); // Black queen
        Assert.AreEqual(0, _mat.Evaluate(b));
    }

    [Test] public void Material_IsDeterministic()
    {
        var b = new BoardModel();
        Assert.AreEqual(_mat.Evaluate(b), _mat.Evaluate(b));
    }

    // Positional evaluator

    [Test] public void Positional_StartingPosition_NearZero()
    {
        int score = _pos.Evaluate(new BoardModel());
        Assert.That(score, Is.InRange(-10, 10), "Symmetric starting position should score within noise range");
    }

    [Test] public void Positional_SignFlipsWithSideToMove()
    {
        var b = ChessTestHelpers.EmptyBoard();
        ChessTestHelpers.Place(b, "e1", PieceType.King, PieceColor.White);
        ChessTestHelpers.Place(b, "e8", PieceType.King, PieceColor.Black);
        ChessTestHelpers.Place(b, "a1", PieceType.Rook, PieceColor.White, hasMoved: true);

        Assert.Greater(_pos.Evaluate(b), 0, "White has extra rook + White to move → positive");

        b.ApplyMove(new Move(Square.FromAlgebraic("e1"), Square.FromAlgebraic("d1")));

        Assert.Less(_pos.Evaluate(b), 0, "White has extra rook + Black to move → negative");
    }

    [Test] public void Positional_RemoveWhitePiece_ScoreDecreases()
    {
        var b    = new BoardModel();
        int base_ = _pos.Evaluate(b);
        b.SetPiece(Square.FromAlgebraic("d1"), PieceData.Empty);
        Assert.Less(_pos.Evaluate(b), base_, "Removing white queen should decrease score");
    }

    [Test] public void Positional_RemoveBlackPiece_ScoreIncreases()
    {
        var b    = new BoardModel();
        int base_ = _pos.Evaluate(b);
        b.SetPiece(Square.FromAlgebraic("d8"), PieceData.Empty);
        Assert.Greater(_pos.Evaluate(b), base_, "Removing black queen should increase score");
    }

    [Test] public void Material_PieceValuesAreExpected()
    {
        Assert.AreEqual(100,  MaterialEvaluator.PawnValue);
        Assert.AreEqual(320,  MaterialEvaluator.KnightValue);
        Assert.AreEqual(330,  MaterialEvaluator.BishopValue);
        Assert.AreEqual(500,  MaterialEvaluator.RookValue);
        Assert.AreEqual(900,  MaterialEvaluator.QueenValue);
    }
}
