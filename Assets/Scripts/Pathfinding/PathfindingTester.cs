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

    Vector2Int[] _directions = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

    List<PathTestCase> _testCases = new List<PathTestCase>();

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

            _testCases.Add(new PathTestCase { Start = start, End = end });
            successCount++;
        }

        Debug.Log($"테스트 케이스 생성: {successCount}");

        if (_testCases.Count == 0)
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
        int testCount = _testCases.Count;
        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < testCount; i++)
        {
            PathTestCase testCase = _testCases[i];

            _runner.RunAndGetPath(testCase.Start, testCase.End);
        }

        stopwatch.Stop();

        double time = stopwatch.Elapsed.TotalMilliseconds;
        Debug.Log($"[Single] Total: {time:F3} ms | PerPath: {time / testCount:F3} ms");
    }

    void RunJobTest()
    {
        int testCount = _testCases.Count;
        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < testCount; i++)
        {
            PathTestCase testCase = _testCases[i];
            _runner.RunJobAndBuildPath(testCase.Start, testCase.End);

        }

        stopwatch.Stop();

        double time = stopwatch.Elapsed.TotalMilliseconds;
        Debug.Log($"[Job] Total: {time:F3} ms | PerPath: {time / testCount:F3} ms");
    }

    void RunMultiJobTest()
    {
        int testCount = _testCases.Count;
        Stopwatch stopwatch = Stopwatch.StartNew();

        _runner.RunMultiJobAndCompleteAll(_testCases);
        stopwatch.Stop();

        double time = stopwatch.Elapsed.TotalMilliseconds;
        Debug.Log($"[Multi] Total: {time:F3} ms | PerPath: {time / testCount:F3} ms");
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
                start = GetNearestWalkable(grid, grid.GetIndex(centerX, centerY), width, height);
                end = GetNearestWalkable(grid, grid.GetIndex(centerX, height - 1), width, height);
                break;

            case ETestMode.CenterToBottomEdge:
                start = GetNearestWalkable(grid, grid.GetIndex(centerX, centerY), width, height);
                end = GetNearestWalkable(grid, grid.GetIndex(centerX, 0), width, height);
                break;

            case ETestMode.CenterToLeftEdge:
                start = GetNearestWalkable(grid, grid.GetIndex(centerX, centerY), width, height);
                end = GetNearestWalkable(grid, grid.GetIndex(0, centerY), width, height);
                break;

            case ETestMode.CenterToRightEdge:
                start = GetNearestWalkable(grid, grid.GetIndex(centerX, centerY), width, height);
                end = GetNearestWalkable(grid, grid.GetIndex(width - 1, centerY), width, height);
                break;

            case ETestMode.CornerToCorner_Diagonal1:
                start = GetNearestWalkable(grid, grid.GetIndex(0, 0), width, height);
                end = GetNearestWalkable(grid, grid.GetIndex(width - 1, height - 1), width, height);
                break;

            case ETestMode.CornerToCorner_Diagonal2:
                start = GetNearestWalkable(grid, grid.GetIndex(0, height - 1), width, height);
                end = GetNearestWalkable(grid, grid.GetIndex(width - 1, 0), width, height);
                break;

            case ETestMode.SameStartGoal:
                start = GetNearestWalkable(grid, grid.GetIndex(centerX, centerY), width, height);
                end = start;
                break;

            default:
                start = -1;
                end = -1;
                break;
        }
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
}