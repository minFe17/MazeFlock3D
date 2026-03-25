using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pathfinding 테스트 전용 클래스
/// 시작/목표 위치를 생성하고 Pathfinding 실행을 담당
/// </summary>
public class PathfindingTester : MonoBehaviour
{
    [SerializeField] Pathfinding _runner;
    [SerializeField] ETestMode _testMode;

    Vector2Int[] _directions = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

    void Start()
    {
        GridSystem grid = _runner.GetGrid();

        int width = grid.Width;
        int height = grid.Height;

        GetStartEnd(grid, width, height, out int start, out int end);

        if (start == -1 || end == -1)
        {
            Debug.LogError("유효한 Start/End 생성 실패");
            return;
        }

        Debug.Log($"Start: {start}, End: {end}");

        _runner.Run(start, end);

         List<int> path = _runner.RunAndGetPath(start, end);

        List<int> rawPath = _runner.RunRawPath(start, end);
        bool isValid = ValidatePath(rawPath, grid);

        Debug.Log($"경로 검증 결과: {isValid}");

        if (path != null)
        {
            int manhattan = GetManhattan(start, end, width);
            Debug.Log($"Path Length: {path.Count}, Manhattan: {manhattan}");
        }
    }

    void GetStartEnd(GridSystem grid, int width, int height, out int start, out int end)
    {
        switch (_testMode)
        {
            case ETestMode.Random:
                start = GetRandomWalkable(grid, width, height);
                end = GetRandomWalkable(grid, width, height);
                break;

            case ETestMode.Corner:
                start = GetNearestWalkable(grid, grid.GetIndex(0, 0), width, height);
                end = GetNearestWalkable(grid, grid.GetIndex(width - 1, height - 1), width, height);
                break;

            case ETestMode.Center:
                int center = grid.GetIndex(width / 2, height / 2);
                start = GetNearestWalkable(grid, center, width, height);
                end = GetNearestWalkable(grid, grid.GetIndex(width - 1, height - 1), width, height);
                break;

            case ETestMode.SameStartGoal:
                start = GetRandomWalkable(grid, width, height);
                end = start;
                break;

            default:
                start = 0;
                end = 0;
                break;
        }
    }

    int GetRandomWalkable(GridSystem grid, int width, int height)
    {
        int maxTry = width * height;

        for (int i = 0; i < maxTry; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            if (grid.GetCell(x, y).Walkable)
                return grid.GetIndex(x, y);
        }

        return -1;
    }

    int GetNearestWalkable(GridSystem grid, int index, int width, int height)
    {
        int startX = index % width;
        int startY = index / width;

        if (grid.GetCell(startX, startY).Walkable)
            return index;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        bool[,] visited = new bool[width, height];

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (grid.GetCell(current.x, current.y).Walkable)
                return grid.GetIndex(current.x, current.y);

            foreach (Vector2Int dir in _directions)
            {
                int nextX = current.x + dir.x;
                int nextY = current.y + dir.y;

                if (!grid.IsInBounds(nextX, nextY)) 
                    continue;
                if (visited[nextX, nextY]) 
                    continue;

                visited[nextX, nextY] = true;
                queue.Enqueue(new Vector2Int(nextX, nextY));
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
            int a = path[i];
            int b = path[i + 1];

            int x1 = a % width;
            int y1 = a / width;

            int x2 = b % width;
            int y2 = b / width;

            int dx = Mathf.Abs(x1 - x2);
            int dy = Mathf.Abs(y1 - y2);

            // 4방향 체크
            if (dx + dy != 1)
            {
                Debug.LogError("잘못된 이동 (대각선 or 점프)");
                return false;
            }

            // 장애물 체크
            if (!grid.GetCell(x2, y2).Walkable)
            {
                Debug.LogError("장애물 통과");
                return false;
            }
        }

        return true;
    }

    int GetManhattan(int start, int end, int width)
    {
        int sx = start % width;
        int sy = start / width;

        int ex = end % width;
        int ey = end / width;

        return Mathf.Abs(sx - ex) + Mathf.Abs(sy - ey);
    }
}