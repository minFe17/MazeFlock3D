using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Pathfinding 테스트 전용 클래스
/// 시작/목표 위치를 생성하고 Pathfinding 실행을 담당
/// </summary>
public class PathfindingTester : MonoBehaviour
{
    [SerializeField] Pathfinding _runner;
    [SerializeField] ETestMode _testMode;
    [SerializeField] EPerformanceMode _performanceMode;
    [SerializeField] int _testSeed;

    [Header("Prefab")]
    [SerializeField] GameObject _agentPrefab;

    Vector2Int[] _directions = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

    List<AgentPathData> _agents = new List<AgentPathData>();

    void Start()
    {
        GridSystem grid = _runner.GetGrid();

        int successCount = 0;
        int tryCount = 0;

        // 여러 테스트 케이스 생성
        while (successCount < 1000 && tryCount < 5000)
        {
            tryCount++;

            if (!TryFindValidPath(grid, out int start, out int end, out List<int> rawPath))
                continue;

            _agents.Add(new AgentPathData { Start = start, End = end });
            successCount++;
        }

        Debug.Log($"테스트 케이스 생성: {successCount}");

        if (_agents.Count == 0)
        {
            Debug.LogError("유효한 테스트 케이스 없음");
            return;
        }

        RunSelectedMode();
    }

    void RunSelectedMode()
    {
        switch (_performanceMode)
        {
            case EPerformanceMode.Single:
                RunSingleTest();
                break;

            case EPerformanceMode.Job:
                RunJobTest();
                break;

            case EPerformanceMode.MultiJob:
                RunMultiJobTest();
                break;
        }
    }

    void RunSingleTest()
    {
        int testCount = _agents.Count;
        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < testCount; i++)
        {
            AgentPathData testCase = _agents[i];
            _runner.RunAndGetPath(testCase.Start, testCase.End);
        }

        stopwatch.Stop();

        double time = stopwatch.Elapsed.TotalMilliseconds;
        Debug.Log($"[Single] Total: {time:F3} ms | PerPath: {time / testCount:F3} ms");
    }

    void RunJobTest()
    {
        int testCount = _agents.Count;
        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < testCount; i++)
        {
            AgentPathData testCase = _agents[i];
            _runner.RunJobAndBuildPath(testCase.Start, testCase.End);
        }

        stopwatch.Stop();

        double time = stopwatch.Elapsed.TotalMilliseconds;
        Debug.Log($"[Job] Total: {time:F3} ms | PerPath: {time / testCount:F3} ms");
    }

    void RunMultiJobTest()
    {
        int testCount = _agents.Count;
        Stopwatch stopwatch = Stopwatch.StartNew();

        _runner.RunMultiJobAndCompleteAll(_agents);
        stopwatch.Stop();

        double time = stopwatch.Elapsed.TotalMilliseconds;
        Debug.Log($"[Multi] Total: {time:F3} ms | PerPath: {time / testCount:F3} ms");

        SpawnAgents();
    }

    bool TryFindValidPath(GridSystem grid, out int start, out int end, out List<int> rawPath)
    {
        int width = grid.Width;
        int height = grid.Height;

        const int MAX_TRY = 30;

        for (int i = 0; i < MAX_TRY; i++)
        {
            GetStartEnd(grid, width, height, out start, out end);

            if (start == -1 || end == -1)
                continue;

            rawPath = _runner.RunRawPath(start, end);

            if (rawPath != null)
                return true;
        }

        start = -1;
        end = -1;
        rawPath = null;
        return false;
    }

    void GetStartEnd(GridSystem grid, int width, int height, out int start, out int end)
    {
        int centerX = width / 2;
        int centerY = height / 2;

        switch (_testMode)
        {
            case ETestMode.CenterToTopEdge:
                start = GetRandomWalkableInArea(grid, centerX - 5, centerX + 5, centerY - 5, centerY + 5);
                end = GetRandomWalkableInArea(grid, 0, width - 1, height - 6, height - 1);
                break;

            case ETestMode.CenterToBottomEdge:
                start = GetRandomWalkableInArea(grid, centerX - 5, centerX + 5, centerY - 5, centerY + 5);
                end = GetRandomWalkableInArea(grid, 0, width - 1, 0, 5);
                break;

            case ETestMode.CenterToLeftEdge:
                start = GetRandomWalkableInArea(grid, centerX - 5, centerX + 5, centerY - 5, centerY + 5);
                end = GetRandomWalkableInArea(grid, 0, 5, 0, height - 1);
                break;

            case ETestMode.CenterToRightEdge:
                start = GetRandomWalkableInArea(grid, centerX - 5, centerX + 5, centerY - 5, centerY + 5);
                end = GetRandomWalkableInArea(grid, width - 6, width - 1, 0, height - 1);
                break;

            case ETestMode.CornerToCorner_Diagonal1:
                start = GetRandomWalkableInArea(grid, 0, 5, 0, 5);
                end = GetRandomWalkableInArea(grid, width - 6, width - 1, height - 6, height - 1);
                break;

            case ETestMode.CornerToCorner_Diagonal2:
                start = GetRandomWalkableInArea(grid, 0, 5, height - 6, height - 1);
                end = GetRandomWalkableInArea(grid, width - 6, width - 1, 0, 5);
                break;

            case ETestMode.SameStartGoal:
                start = GetRandomWalkableInArea(grid, centerX - 5, centerX + 5, centerY - 5, centerY + 5);
                end = start;
                break;

            default:
                start = -1;
                end = -1;
                break;
        }
    }

    int GetRandomWalkableInArea(GridSystem grid, int minX, int maxX, int minY, int maxY)
    {
        int width = grid.Width;
        int height = grid.Height;

        minX = Mathf.Clamp(minX, 0, width - 1);
        maxX = Mathf.Clamp(maxX, 0, width - 1);

        minY = Mathf.Clamp(minY, 0, height - 1);
        maxY = Mathf.Clamp(maxY, 0, height - 1);

        const int MAX_TRY = 30;

        for (int i = 0; i < MAX_TRY; i++)
        {
            int randomX = Random.Range(minX, maxX + 1);
            int randomY = Random.Range(minY, maxY + 1);

            int index = grid.GetIndex(randomX, randomY);

            int walkableIndex = GetNearestWalkable(grid, index, width, height);

            if (walkableIndex != -1)
                return walkableIndex;
        }

        return -1;
    }

    int GetNearestWalkable(GridSystem grid, int startIndex, int width, int height)
    {
        int startX = startIndex % width;
        int startY = startIndex / width;

        if (grid.Walkables[startIndex] == 1)
            return startIndex;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        bool[,] visited = new bool[width, height];

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int currentPos = queue.Dequeue();
            int currentIndex = currentPos.y * width + currentPos.x;

            if (grid.Walkables[currentIndex] == 1)
                return currentIndex;

            foreach (Vector2Int direction in _directions)
            {
                int nextX = currentPos.x + direction.x;
                int nextY = currentPos.y + direction.y;

                if (nextX < 0 || nextX >= width || nextY < 0 || nextY >= height)
                    continue;

                if (visited[nextX, nextY])
                    continue;

                visited[nextX, nextY] = true;
                queue.Enqueue(new Vector2Int(nextX, nextY));
            }
        }

        return -1;
    }

    void SpawnAgents()
    {
        GridSystem grid = _runner.GetGrid();

        for (int i = 0; i < _agents.Count; i++)
        {
            AgentPathData agent = _agents[i];

            if (agent.Path == null || agent.Path.Count == 0)
                continue;

            GameObject agentObject = Instantiate(_agentPrefab);
            Vector3 startPosition = IndexToWorld(agent.Start, grid.Width);
            agentObject.transform.position = startPosition;

            AgentMover mover = agentObject.GetComponent<AgentMover>();
            mover.SetPath(agent.Path, grid.Width);
        }
    }

    Vector3 IndexToWorld(int index, int gridWidth)
    {
        int x = index % gridWidth;
        int y = index / gridWidth;

        return new Vector3(x, 0, y);
    }
}