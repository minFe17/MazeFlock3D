using System.Collections.Generic;
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

        int successCount = 0;
        int tryCount = 0;

        while (successCount < 50 && tryCount < 200)
        {
            tryCount++;

            if (!TryFindValidPath(grid, out int start, out int end, out List<int> rawPath))
                continue;

            _runner.StartPathfindingJobMulti(start, end);
            successCount++;
        }

        Debug.Log($"멀티 Job 요청 완료: {successCount}/50");
    }

    void Update()
    {
        _runner.UpdatePathRequests();
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

    int GetNearbyWalkable(GridSystem grid, int baseIndex, int totalNode)
    {
        for (int i = 0; i < 10; i++)
        {
            int offset = Random.Range(-10, 10);
            int index = baseIndex + offset;

            if (index < 0 || index >= totalNode)
                continue;

            if (grid.Walkables[index])
                return index;
        }

        return -1;
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

            foreach (Vector2Int direction in _directions)
            {
                int nx = currentPos.x + direction.x;
                int ny = currentPos.y + direction.y;

                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                if (visited[nx, ny])
                    continue;

                visited[nx, ny] = true;
                queue.Enqueue(new Vector2Int(nx, ny));
            }
        }

        return -1;
    }
}