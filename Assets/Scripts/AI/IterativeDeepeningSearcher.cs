using System.Collections.Generic;
using System.Diagnostics;

// Iterative deepening: runs AlphaBeta at depth 1, 2, and 3, sharing one timer.
// Returns the best move from the last depth that finished.If none finished, returns the partial result.
public class IterativeDeepeningSearcher
{
    private readonly IEvaluator _evaluator;
    private int _totalPruneCount;

    public int TotalPruneCount => _totalPruneCount;

    public IterativeDeepeningSearcher(IEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    // timeLimitMs: 0 means no time limit, just search to maxDepth.
    public SearchResult Search(
        BoardModel board, int maxDepth,
        long timeLimitMs = 0,
        Dictionary<string, int> posHistory = null,
        bool useNoise = false)
    {
        _totalPruneCount = 0;
        var sw = Stopwatch.StartNew();

        SearchResult best    = default;
        bool         hasBest = false;

        for (int depth = 1; depth <= maxDepth; depth++)
        {
            if (timeLimitMs > 0 && sw.ElapsedMilliseconds >= timeLimitMs) break;

            var ab = new AlphaBetaSearcher(_evaluator);
            if (timeLimitMs > 0) ab.SetTimeGuard(sw, timeLimitMs);
            if (useNoise)        ab.SetNoise(true);

            SearchResult result = ab.Search(board, depth, posHistory);
            _totalPruneCount += ab.PruneCount;

            if (ab.WasInterrupted)
            {
                // Cut short this depth may be incomplete. Fall back to it only if nothing finished yet.
                if (!hasBest && result.HasMove) best = result;
                break;
            }

            if (result.HasMove)
            {
                best    = result;
                hasBest = true;
            }
        }

        sw.Stop();
        best.ElapsedMs = sw.ElapsedMilliseconds;
        return best;
    }
}
