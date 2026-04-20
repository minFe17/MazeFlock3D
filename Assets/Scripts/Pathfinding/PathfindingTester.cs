using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
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
    [SerializeField] int _testSeed;

    Vector2Int[] _directions = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

    void Start()
    {
        GridSystem grid = _runner.GetGrid();

        if (!TryFindValidPath(grid, out int start, out int end, out List<int> rawPath))
        {
            Debug.LogError("경로 생성 실패 (미로 구조 문제 가능)");
            return;
        }

        //Debug.Log($"Start: {start}, End: {end}");
        //// 여기 중요
        //List<int> path = RunPathfinding(start, end);

        //ValidateAll(grid, start, end, rawPath, path);

        List<int> jobPath = _runner.RunJobAndBuildPath(start, end);

        if (jobPath == null)
            Debug.LogError("Job 경로 생성 실패");
        else
            Debug.Log($"Job Path Length: {jobPath.Count}");
    }

    bool TryFindValidPath(GridSystem grid, out int start, out int end, out List<int> rawPath)
    {
        int width = grid.Width;
        int height = grid.Height;

        const int MAX_TRY = 30;

        for (int i = 0; i < MAX_TRY; i++)
        {
            GetStartEnd(grid, width, height, out start, out end);

            Debug.Log($"[Try {i}] start={start}, end={end}");

            if (start == -1 || end == -1)
            {
                Debug.LogWarning($"[Try {i}] start/end 못 찾음");
                continue;
            }

            rawPath = _runner.RunRawPath(start, end);

            Debug.Log($"[Try {i}] rawPath={rawPath?.Count.ToString() ?? "null"}");

            if (rawPath != null)
                return true;
        }

        start = -1;
        end = -1;
        rawPath = null;
        return false;
    }

    List<int> RunPathfinding(int start, int end)
    {
        const int TEST_COUNT = 20;

        double totalTime = 0;
        double minTime = double.MaxValue;
        double maxTime = double.MinValue;

        int totalVisited = 0;

        List<int> finalPath = null;

        _runner.RunAndGetPath(start, end);

        for (int i = 0; i < TEST_COUNT; i++)
        {
            Stopwatch sw = Stopwatch.StartNew();

            finalPath = _runner.RunAndGetPath(start, end);

            sw.Stop();

            double time = sw.Elapsed.TotalMilliseconds;
            totalTime += time;

            int visited = _runner.GetVisitedNodeCount();
            totalVisited += visited;

            if (time < minTime) minTime = time;
            if (time > maxTime) maxTime = time;
        }

        double avgTime = totalTime / TEST_COUNT;
        int avgVisited = totalVisited / TEST_COUNT;

        Debug.Log($"[Pathfinding] Avg Time: {avgTime:F3} ms");
        Debug.Log($"[Pathfinding] Min Time: {minTime:F3} ms");
        Debug.Log($"[Pathfinding] Max Time: {maxTime:F3} ms");
        Debug.Log($"[Pathfinding] Avg Visited Nodes: {avgVisited}");

        return finalPath;
    }

    void ValidateAll(GridSystem grid, int start, int end, List<int> rawPath, List<int> path)
    {
        int width = grid.Width;

        bool isValid = ValidatePath(rawPath, grid);
        Debug.Log($"경로 검증 결과: {isValid}");

        if (path != null)
        {
            int manhattan = GetManhattan(start, end, width);
            Debug.Log($"Path Length: {path.Count}, Manhattan: {manhattan}");
        }

        ValidateMaze(grid);
    }

    void GetStartEnd(GridSystem grid, int width, int height, out int start, out int end)
    {
        int cx = width / 2;
        int cy = height / 2;

        switch (_testMode)
        {
            case ETestMode.CenterToTopEdge:
                start = GetNearestWalkable(grid, grid.GetIndex(cx, cy), width, height);
                end = GetNearestWalkable(grid, grid.GetIndex(cx, height - 1), width, height);
                break;

            case ETestMode.CenterToBottomEdge:
                start = GetNearestWalkable(grid, grid.GetIndex(cx, cy), width, height);
                end = GetNearestWalkable(grid, grid.GetIndex(cx, 0), width, height);
                break;

            case ETestMode.CenterToLeftEdge:
                start = GetNearestWalkable(grid, grid.GetIndex(cx, cy), width, height);
                end = GetNearestWalkable(grid, grid.GetIndex(0, cy), width, height);
                break;

            case ETestMode.CenterToRightEdge:
                start = GetNearestWalkable(grid, grid.GetIndex(cx, cy), width, height);
                end = GetNearestWalkable(grid, grid.GetIndex(width - 1, cy), width, height);
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
                start = GetNearestWalkable(grid, grid.GetIndex(cx, cy), width, height);
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

        if (grid.Walkables[startIndex])
            return startIndex;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        bool[,] visited = new bool[width, height];

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int currentPos = queue.Dequeue();

            int currentIndex = currentPos.y * width + currentPos.x;

            if (grid.Walkables[currentIndex])
                return currentIndex;

            foreach (Vector2Int dir in _directions)
            {
                int neighborX = currentPos.x + dir.x;
                int neighborY = currentPos.y + dir.y;

                if (neighborX < 0 || neighborX >= width || neighborY < 0 || neighborY >= height)
                    continue;

                if (visited[neighborX, neighborY])
                    continue;

                visited[neighborX, neighborY] = true;
                queue.Enqueue(new Vector2Int(neighborX, neighborY));
            }
        }

        return -1;
    }

    bool ValidatePath(List<int> path, GridSystem grid)
    {
        if (path == null || path.Count == 0)
            return false;

        int width = grid.Width;

        for (int i = 0; i < path.Count - 1; i++)
        {
            int currentIndex = path[i];
            int nextIndex = path[i + 1];

            int currentX = currentIndex % width;
            int currentY = currentIndex / width;

            int nextX = nextIndex % width;
            int nextY = nextIndex / width;

            int deltaX = Mathf.Abs(currentX - nextX);
            int deltaY = Mathf.Abs(currentY - nextY);

            if (deltaX + deltaY != 1)
            {
                Debug.LogError("잘못된 이동 (대각선 or 점프)");
                return false;
            }

            if (!grid.Walkables[nextIndex])
            {
                Debug.LogError("장애물 통과");
                return false;
            }
        }

        return true;
    }

    int GetManhattan(int start, int end, int width)
    {
        int startX = start % width;
        int startY = start / width;

        int endX = end % width;
        int endY = end / width;

        return Mathf.Abs(startX - endX) + Mathf.Abs(startY - endY);
    }

    void ValidateMaze(GridSystem grid)
    {
        int width = grid.Width;
        int height = grid.Height;

        NativeArray<bool> walkables = grid.Walkables;

        int total = width * height;
        int pathCount = 0;

        int startIndex = -1;

        // 통로 개수 + 시작점 찾기
        for (int i = 0; i < total; i++)
        {
            if (walkables[i])
            {
                pathCount++;

                if (startIndex == -1)
                    startIndex = i;
            }
        }

        if (startIndex == -1)
        {
            Debug.LogError("통로가 없음");
            return;
        }

        // BFS
        bool[] visited = new bool[total];
        Queue<int> queue = new Queue<int>();

        queue.Enqueue(startIndex);
        visited[startIndex] = true;

        int visitedCount = 1;

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            int x = current % width;
            int y = current / width;

            // 상
            int up = current - width;
            if (up >= 0 && !visited[up] && walkables[up])
            {
                visited[up] = true;
                visitedCount++;
                queue.Enqueue(up);
            }

            // 하
            int down = current + width;
            if (down < total && !visited[down] && walkables[down])
            {
                visited[down] = true;
                visitedCount++;
                queue.Enqueue(down);
            }

            // 좌
            int left = current - 1;
            if (left >= 0 && (left % width) == x - 1 && !visited[left] && walkables[left])
            {
                visited[left] = true;
                visitedCount++;
                queue.Enqueue(left);
            }

            // 우
            int right = current + 1;
            if (right < total && (right % width) == x + 1 && !visited[right] && walkables[right])
            {
                visited[right] = true;
                visitedCount++;
                queue.Enqueue(right);
            }
        }

        Debug.Log($"[미로 검증] 전체 통로: {pathCount}, 연결된 통로: {visitedCount}");

        bool isConnected = (pathCount == visitedCount);
        Debug.Log($"[연결성] {isConnected}");

        float ratio = (float)pathCount / total;
        Debug.Log($"[비율] Path Ratio: {ratio:F2}");
    }
}