public interface IEvaluator
{
    // Score from the side-to-move's perspective (negamax convention):
    // positive = good for the player whose turn it is, negative = bad.
    int Evaluate(BoardModel board);
}
