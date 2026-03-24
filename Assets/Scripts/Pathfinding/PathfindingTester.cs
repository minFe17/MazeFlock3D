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

        // 재탐색 테스트

        int goal1 = GetRandomWalkable(grid, width, height);
        int goal2 = GetRandomWalkable(grid, width, height);

        var path1 = _runner.RunAndGetPath(start, goal1);
        var path2 = _runner.RunAndGetPath(start, goal2);

        Debug.Log($"[재탐색] Goal1: {goal1}, Length: {path1?.Count}");
        Debug.Log($"[재탐색] Goal2: {goal2}, Length: {path2?.Count}");

        // 가까운 vs 먼 목표 비교

        int startX = start % width;
        int startY = start / width;

        // 가까운 목표
        int nearX = Mathf.Clamp(startX + 1, 0, width - 1);
        int nearY = startY;

        int nearGoal = grid.GetIndex(nearX, nearY);

        if (!grid.GetCell(nearX, nearY).Walkable)
            nearGoal = GetNearestWalkable(grid, nearGoal, width, height);

        List<int> nearPath = _runner.RunAndGetPath(start, nearGoal);

        // 먼 목표
        int farGoal = grid.GetIndex(width - 1, height - 1);
        List<int> farPath = _runner.RunAndGetPath(start, farGoal);

        Debug.Log($"[거리비교] Near Length: {nearPath?.Count}");
        Debug.Log($"[거리비교] Far Length: {farPath?.Count}");
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
}