using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class SearchMetricsLogger : MonoBehaviour
{
    public string FileName = "chess_metrics.csv";

    private readonly List<MetricEntry> _entries = new List<MetricEntry>();

    private struct MetricEntry
    {
        public string Algorithm;
        public int    MoveNumber;
        public int    Depth;
        public int    NodesVisited;
        public int    PruneCount;
        public long   ElapsedMs;
        public int    Score;
        public string BestMove;
    }

    public void Log(SearchResult result, string algorithm, int moveNumber, int pruneCount = 0)
    {
        _entries.Add(new MetricEntry
        {
            Algorithm    = algorithm,
            MoveNumber   = moveNumber,
            Depth        = result.DepthReached,
            NodesVisited = result.NodesVisited,
            PruneCount   = pruneCount,
            ElapsedMs    = result.ElapsedMs,
            Score        = result.Score,
            BestMove     = result.HasMove ? result.BestMove.ToString() : "-"
        });

        Debug.Log($"[Metrics] {algorithm} | Move {moveNumber} | Depth {result.DepthReached} | " +
                  $"Nodes {result.NodesVisited} | Pruned {pruneCount} | Time {result.ElapsedMs}ms | " +
                  $"Score {result.Score} | Best {result.BestMove}");
    }

    public void ExportCSV()
    {
        string path = Path.Combine(Application.persistentDataPath, FileName);
        var sb = new StringBuilder();
        sb.AppendLine("Algorithm,MoveNumber,Depth,NodesVisited,PruneCount,ElapsedMs,Score,BestMove");

        foreach (var e in _entries)
            sb.AppendLine($"{e.Algorithm},{e.MoveNumber},{e.Depth},{e.NodesVisited}," +
                          $"{e.PruneCount},{e.ElapsedMs},{e.Score},{e.BestMove}");

        File.WriteAllText(path, sb.ToString());
        Debug.Log($"[Metrics] CSV exported to: {path}");
    }

    public void Clear() => _entries.Clear();

    void OnApplicationQuit() => ExportCSV();
}
