using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grid 맵 생성 및 장애물, 미로 생성 담당 클래스
/// </summary>
public static class MapBuilder
{
    static Vector2Int[] _directions = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

    // 랜덤 장애물 생성 + 연결성 보장
    public static void CreateObstacle(GridSystem grid, int width, int height, float noiseRate = 0.1f)
    {
        // Prim으로 기본 구조 생성 (항상 연결됨)
        CreatePrimMaze(grid, width, height);

        // 랜덤 노이즈 추가 (자연스러움)
        AddNoise(grid, width, height, noiseRate);

        // 연결성 보정 (혹시 끊긴 경우)
        EnsureConnectivity(grid, width, height);
    }

    #region Noise
    public static void AddNoise(GridSystem grid, int width, int height, float noiseRate)
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                GridCell cell = grid.GetCell(x, y);

                if (!cell.Walkable && Random.value < noiseRate)
                    cell.Walkable = true;
                else if (cell.Walkable && Random.value < noiseRate * 0.05f)
                    cell.Walkable = false;

                grid.SetCell(x, y, cell);
            }
        }
    }
    #endregion

    #region Connectivity Fix
    static void EnsureConnectivity(GridSystem grid, int width, int height)
    {
        bool[,] visited = new bool[width, height];

        List<Vector2Int> largest = new List<Vector2Int>();

        // 모든 컴포넌트 탐색
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!grid.GetCell(x, y).Walkable || visited[x, y])
                    continue;

                List<Vector2Int> current = BFS(grid, x, y, visited, width, height);

                if (current.Count > largest.Count)
                    largest = current;
            }
        }

        // largest 제외 전부 막기
        bool[,] keep = new bool[width, height];

        foreach (Vector2Int p in largest)
            keep[p.x, p.y] = true;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!keep[x, y])
                {
                    GridCell cell = grid.GetCell(x, y);
                    cell.Walkable = false;
                    grid.SetCell(x, y, cell);
                }
            }
        }
    }

    static List<Vector2Int> BFS(GridSystem grid, int startX, int startY, bool[,] visited, int width, int height)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        List<Vector2Int> result = new List<Vector2Int>();

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);

            foreach (var direction in _directions)
            {
                int nextX = current.x + direction.x;
                int nextY = current.y + direction.y;

                if (!grid.IsInBounds(nextX, nextY)) 
                    continue;
                if (visited[nextX, nextY]) 
                    continue;
                if (!grid.GetCell(nextX, nextY).Walkable) 
                    continue;

                visited[nextX, nextY] = true;
                queue.Enqueue(new Vector2Int(nextX, nextY));
            }
        }

        return result;
    }
    #endregion

    #region Prim
    public static void CreatePrimMaze(GridSystem grid, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridCell cell = grid.GetCell(x, y);
                cell.Walkable = false;
                grid.SetCell(x, y, cell);
            }
        }

        int startX = Random.Range(0, width / 2) * 2;
        int startY = Random.Range(0, height / 2) * 2;

        SetPath(grid, startX, startY);

        List<Vector2Int> frontier = new List<Vector2Int>();
        HashSet<Vector2Int> frontierSet = new HashSet<Vector2Int>();

        AddFrontier(grid, frontier, frontierSet, startX, startY, width, height);

        while (frontier.Count > 0)
        {
            int rand = Random.Range(0, frontier.Count);
            Vector2Int current = frontier[rand];

            frontier.RemoveAt(rand);
            frontierSet.Remove(current);

            if (grid.GetCell(current.x, current.y).Walkable)
                continue;

            List<Vector2Int> neighbors = GetNeighbors(grid, current, width, height);
            if (neighbors.Count == 0)
                continue;

            Vector2Int chosen = neighbors[Random.Range(0, neighbors.Count)];

            int wallX = (current.x + chosen.x) / 2;
            int wallY = (current.y + chosen.y) / 2;

            SetPath(grid, wallX, wallY);
            SetPath(grid, current.x, current.y);

            AddFrontier(grid, frontier, frontierSet, current.x, current.y, width, height);
        }
    }
    #endregion

    #region Helper
    static void SetPath(GridSystem grid, int x, int y)
    {
        GridCell cell = grid.GetCell(x, y);
        cell.Walkable = true;
        grid.SetCell(x, y, cell);
    }

    static void AddFrontier(GridSystem grid, List<Vector2Int> frontier, HashSet<Vector2Int> set, int x, int y, int w, int h)
    {
        foreach (Vector2Int direction in _directions)
        {
            Vector2Int d = direction * 2;

            int nextX = x + d.x;
            int nextY = y + d.y;

            if (nextX < 0 || nextY < 0 || nextX >= w || nextY >= h)
                continue;

            if (!grid.GetCell(nextX, nextY).Walkable)
            {
                Vector2Int next = new Vector2Int(nextX, nextY);

                if (set.Add(next))
                    frontier.Add(next);
            }
        }
    }

    static List<Vector2Int> GetNeighbors(GridSystem grid, Vector2Int pos, int width, int height)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (Vector2Int direction in _directions)
        {
            Vector2Int d = direction * 2;

            int nextX = pos.x + d.x;
            int nextY = pos.y + d.y;

            if (nextX < 0 || nextY < 0 || nextX >= width || nextY >= height)
                continue;

            if (grid.GetCell(nextX, nextY).Walkable)
                result.Add(new Vector2Int(nextX, nextY));
        }

        return result;
    }
    #endregion
}