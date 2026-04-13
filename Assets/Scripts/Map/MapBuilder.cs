using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grid 맵 생성 및 장애물, 미로 생성 담당 클래스
/// </summary>
public static class MapBuilder
{
    static readonly Vector2Int[] _directions = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

    // Prim Maze
    public static void CreatePrimMaze(GridSystem grid, int width, int height)
    {
        for (int i = 0; i < width * height; i++)
            grid.SetWalkable(i, false);

        HashSet<int> visited = new HashSet<int>();
        List<int> frontier = new List<int>();

        int start = 1 + width;
        visited.Add(start);
        grid.SetWalkable(start, true);

        AddPrimFrontier(start, grid, width, height, visited, frontier);

        while (frontier.Count > 0)
        {
            int index = Random.Range(0, frontier.Count);
            int cell = frontier[index];
            frontier.RemoveAt(index);

            if (visited.Contains(cell))
                continue;

            List<int> neighbors = GetVisitedNeighbors(cell, grid, width, height, visited);
            if (neighbors.Count == 0)
                continue;

            int neighbor = neighbors[Random.Range(0, neighbors.Count)];
            int between = (cell + neighbor) / 2;

            grid.SetWalkable(cell, true);
            grid.SetWalkable(between, true);
            visited.Add(cell);

            AddPrimFrontier(cell, grid, width, height, visited, frontier);
        }
    }

    static void AddPrimFrontier(int cell, GridSystem grid, int width, int height, HashSet<int> visited, List<int> frontier)
    {
        int x = cell % width;
        int y = cell / width;

        for (int d = 0; d < 4; d++)
        {
            int nextX = x + _directions[d].x * 2;
            int nextY = y + _directions[d].y * 2;

            if (nextX < 1 || nextX >= width - 1 || nextY < 1 || nextY >= height - 1)
                continue;

            int next = nextY * width + nextX;

            if (!visited.Contains(next) && !frontier.Contains(next))
                frontier.Add(next);
        }
    }

    static List<int> GetVisitedNeighbors(int cell, GridSystem grid, int width, int height, HashSet<int> visited)
    {
        List<int> result = new List<int>();

        int x = cell % width;
        int y = cell / width;

        for (int d = 0; d < 4; d++)
        {
            int nextX = x + _directions[d].x * 2;
            int nextY = y + _directions[d].y * 2;

            if (nextX < 1 || nextX >= width - 1 || nextY < 1 || nextY >= height - 1)
                continue;

            int next = nextY * width + nextX;

            if (visited.Contains(next))
                result.Add(next);
        }

        return result;
    }
}