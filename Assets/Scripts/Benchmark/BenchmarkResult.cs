/// <summary>
/// Pathfinding Benchmark 결과 데이터를 저장하는 클래스
/// </summary>
public class BenchmarkResult
{
    long _searchTime;     // 경로 탐색에 걸린 시간
    int _visitedNodes;    // 탐색 과정에서 방문한 노드 수

    public long SearchTime { get => _searchTime; set => _searchTime = value; }
    public int VisitedNodes { get => _visitedNodes; set => _visitedNodes = value; }
}