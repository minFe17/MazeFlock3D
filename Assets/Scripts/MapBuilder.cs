using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grid 맵 생성 및 장애물, 미로 생성 담당 클래스
/// </summary>
public static class MapBuilder
{
    static Vector2Int[] _directions = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

    // 기존 장애물 생성
    public static void CreateObstacle(GridSystem grid, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            GridCell cell = grid.GetCell(x, 5);
            cell.Walkable = false;
            grid.SetCell(x, 5, cell);
        }

        GridCell gap = grid.GetCell(4, 5);
        gap.Walkable = true;
        grid.SetCell(4, 5, gap);
    }

    // Prim 미로 생성
    public static void CreatePrimMaze(GridSystem grid, int width, int height)
    {
        // 1. 전체를 벽으로 초기화
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridCell cell = grid.GetCell(x, y);
                cell.Walkable = false;
                grid.SetCell(x, y, cell);
            }
        }

        // 2. 시작 셀 선택
        int startX = Random.Range(0, width);
        int startY = Random.Range(0, height);

        SetPath(grid, startX, startY);

        // 3. Frontier 리스트
        List<Vector2Int> frontier = new List<Vector2Int>();
        AddFrontier(frontier, startX, startY, width, height);

        // 4. 반복
        while (frontier.Count > 0)
        {
            int randIndex = Random.Range(0, frontier.Count);
            Vector2Int current = frontier[randIndex];
            frontier.RemoveAt(randIndex);

            // 연결 가능한지 체크
            if (CanConnect(grid, current.x, current.y, width, height))
            {
                SetPath(grid, current.x, current.y);
                AddFrontier(frontier, current.x, current.y, width, height);
            }
        }
    }

    #region Helper
    // 해당 좌표를 통로(길)로 변경
    static void SetPath(GridSystem grid, int x, int y)
    {
        GridCell cell = grid.GetCell(x, y);
        cell.Walkable = true;
        grid.SetCell(x, y, cell);
    }

    // 현재 셀 기준 인접 셀을 Frontier 리스트에 추가 (중복 방지)
    static void AddFrontier(List<Vector2Int> frontier, int x, int y, int width, int height)
    {
        foreach (Vector2Int direction in _directions)
        {
            int nextX = x + direction.x;
            int nextY = y + direction.y;

            // Grid 범위 체크
            if (nextX < 0 || nextY < 0 || nextX >= width || nextY >= height)
                continue;

            Vector2Int next = new Vector2Int(nextX, nextY);

            // 이미 추가된 Frontier는 제외
            if (!frontier.Contains(next))
                frontier.Add(next);
        }
    }

    // 인접 통로 개수를 체크하여 사이클 생성 여부 판단
    static bool CanConnect(GridSystem grid, int x, int y, int width, int height)
    {
        int pathCount = 0;

        foreach (Vector2Int direction in _directions)
        {
            int nextX = x + direction.x;
            int nextY = y + direction.y;

            // Grid 범위 체크
            if (nextX < 0 || nextY < 0 || nextX >= width || nextY >= height)
                continue;

            // 주변에 이미 통로가 있는 경우 카운트
            if (grid.GetCell(nextX, nextY).Walkable)
                pathCount++;
        }

        // 1개 이하만 연결 허용 (사이클 방지)
        return pathCount <= 1;
    }
    #endregion
}