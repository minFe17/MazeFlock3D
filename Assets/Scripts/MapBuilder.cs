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
        int x = cell % width, y = cell / width;

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
        int x = cell % width, y = cell / width;

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

    // Graph Maze (Obstacle)
    public static void CreateObstacle(GridSystem grid, int width, int height, float obstacleRate)
    {
        // 전체 오픈
        for (int i = 0; i < width * height; i++)
            grid.SetWalkable(i, true);

        int sectorSize = 8;
        int sectorsX = Mathf.Max(1, width / sectorSize);
        int sectorsY = Mathf.Max(1, height / sectorSize);
        int segLen = (width + height);

        for (int startY = 0; startY < sectorsY; startY++)
        {
            for (int startX = 0; startX < sectorsX; startX++)
            {
                int clampX = Mathf.Clamp(startX * sectorSize + Random.Range(1, sectorSize - 1), 1, width - 2);
                int clampY = Mathf.Clamp(startY * sectorSize + Random.Range(1, sectorSize - 1), 1, height - 2);
                DrunkWalk(grid, width, height, clampX, clampY, segLen);
            }
        }

        // 추가 랜덤 Walk (복잡도 증가)
        int extraWalks = (sectorsX * sectorsY) / 2;
        for (int i = 0; i < extraWalks; i++)
        {
            int randomX = Random.Range(1, width - 1);
            int randomY = Random.Range(1, height - 1);
            DrunkWalk(grid, width, height, randomX, randomY, segLen / 2);
        }

        // 경계 강제
        EnforceBorder(grid, width, height);

        // obstacleRate만큼 벽 배치 (연결성 실시간 보장)
        ApplyObstacles(grid, width, height, obstacleRate);

        // 루프 추가
        int loopCount = Mathf.Max(8, (width * height) / 40);
        AddLoops(grid, width, height, loopCount);

        // 경계 재강제
        EnforceBorder(grid, width, height);
    }

    static void DrunkWalk(GridSystem grid, int width, int height, int startX, int startY, int steps)
    {
        int x = startX;
        int y = startY;
        int randomDirection = Random.Range(0, 4);

        for (int i = 0; i < steps; i++)
        {
            if (Random.value < 0.30f)
                randomDirection = Random.Range(0, 4);

            int nextX = x + _directions[randomDirection].x;
            int nextY = y + _directions[randomDirection].y;

            if (nextX <= 0 || nextX >= width - 1 || nextY <= 0 || nextY >= height - 1)
            {
                randomDirection = Random.Range(0, 4);
                continue;
            }

            x = nextX;
            y = nextY;
            grid.SetWalkable(x, y, true);
        }
    }

    static void ApplyObstacles(GridSystem grid, int width, int height, float obstacleRate)
    {
        // walkable 셀 수집
        List<int> candidates = new List<int>();
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                if (grid.GetCell(x, y).Walkable)
                    candidates.Add(y * width + x);
            }
        }

        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        int targetWalls = Mathf.RoundToInt(candidates.Count * obstacleRate);
        int placed = 0;

        foreach (int index in candidates)
        {
            if (placed >= targetWalls) break;

            int cx = index % width;
            int cy = index / width;

            int walkNeighbors = 0;
            for (int d = 0; d < 4; d++)
            {
                int nextX = cx + _directions[d].x;
                int nextY = cy + _directions[d].y;

                if (!grid.IsInBounds(nextX, nextY))
                    continue;
                if (grid.GetCell(nextX, nextY).Walkable)
                    walkNeighbors++;
            }
            if (walkNeighbors < 3)
                continue;

            // 막아보고 연결성 깨지면 되돌리기
            grid.SetWalkable(cx, cy, false);

            if (!IsFullyConnected(grid, width, height))
            {
                grid.SetWalkable(cx, cy, true);
                continue;
            }
            placed++;
        }
    }

    // 전체 연결성 BFS 검사
    static bool IsFullyConnected(GridSystem grid, int width, int height)
    {
        int startX = -1;
        int startY = -1;
        int totalWalkable = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!grid.GetCell(x, y).Walkable)
                    continue;

                totalWalkable++;

                if (startX == -1)
                {
                    startX = x;
                    startY = y;
                }
            }
        }


        if (totalWalkable == 0) 
            return false;

        HashSet<int> visited = new HashSet<int>();
        Queue<int> queue = new Queue<int>();

        int first = startY * width + startX;
        queue.Enqueue(first);
        visited.Add(first);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            int currentX = current % width;
            int currentY = current / width;

            for (int d = 0; d < 4; d++)
            {
                int nextX = currentX + _directions[d].x;
                int nextY = currentY + _directions[d].y;

                if (!grid.IsInBounds(nextX, nextY)) 
                    continue;
                int next = nextY * width + nextX;

                if (visited.Contains(next)) 
                    continue;
                if (!grid.GetCell(nextX, nextY).Walkable) 
                    continue;

                visited.Add(next);
                queue.Enqueue(next);
            }
        }
        return visited.Count == totalWalkable;
    }

    // 루프 추가
    static void AddLoops(GridSystem grid, int width, int height, int count)
    {
        int[] ldx = { 0, 1 };
        int[] ldy = { 1, 0 };
        int tries = count * 10;

        for (int t = 0; t < tries && count > 0; t++)
        {
            int x = Random.Range(1, width - 2);
            int y = Random.Range(1, height - 2);
            int d = Random.Range(0, 2);

            int ax = x;
            int ay = y;
            int mx = x + ldx[d];
            int my = y + ldy[d];
            int bx = x + ldx[d] * 2; 
            int by = y + ldy[d] * 2;

            if (bx <= 0 || bx >= width - 1 || by <= 0 || by >= height - 1) 
                continue;

            bool aWalk = grid.GetCell(ax, ay).Walkable;
            bool bWalk = grid.GetCell(bx, by).Walkable;
            bool mWall = !grid.GetCell(mx, my).Walkable;

            if (aWalk && bWalk && mWall)
            {
                grid.SetWalkable(mx, my, true);
                count--;
            }
        }
    }

    // 경계 강제 
    static void EnforceBorder(GridSystem grid, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            grid.SetWalkable(x, 0, false);
            grid.SetWalkable(x, height - 1, false);
        }
        for (int y = 0; y < height; y++)
        {
            grid.SetWalkable(0, y, false);
            grid.SetWalkable(width - 1, y, false);
        }
    }
}