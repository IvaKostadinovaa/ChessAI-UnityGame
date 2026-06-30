public struct SearchResult
{
    public Move   BestMove;
    public int    Score;
    public int    NodesVisited;
    public int    DepthReached;
    public long   ElapsedMs;
    public bool   HasMove;

    public override string ToString() =>
        $"Move={BestMove} Score={Score} Nodes={NodesVisited} Depth={DepthReached} Time={ElapsedMs}ms";
}
