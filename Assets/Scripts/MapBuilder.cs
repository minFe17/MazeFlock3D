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
        // 1. 전체 벽
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridCell cell = grid.GetCell(x, y);
                cell.Walkable = false;
                grid.SetCell(x, y, cell);
            }
        }

        // 2. 시작점 (짝수 좌표)
        int startX = Random.Range(0, width / 2) * 2;
        int startY = Random.Range(0, height / 2) * 2;

        SetPath(grid, startX, startY);

        List<Vector2Int> frontier = new List<Vector2Int>();
        HashSet<Vector2Int> frontierSet = new HashSet<Vector2Int>();
        AddFrontier(grid, frontier, frontierSet, startX, startY, width, height);

        int maxIteration = width * height * 4;
        int iteration = 0;

        while (frontier.Count > 0)
        {
            iteration++;

            if (iteration > maxIteration)
            {
                Debug.LogError("Maze Generation 무한 루프 방지");
                break;
            }

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

    #region Helper
    // 해당 좌표를 통로(길)로 변경
    static void SetPath(GridSystem grid, int x, int y)
    {
        GridCell cell = grid.GetCell(x, y);
        cell.Walkable = true; 
        grid.SetCell(x, y, cell);
    }

    // 현재 셀 기준 2칸 떨어진 Frontier 후보 추가 (중복 방지)
    static void AddFrontier(GridSystem grid, List<Vector2Int> frontier, HashSet<Vector2Int> frontierSet, int x, int y, int width, int height)
    {
        foreach (Vector2Int dir in _directions)
        {
            // 2칸 이동 (벽 사이 구조 유지)
            Vector2Int direction = dir * 2; 
            int nextX = x + direction.x;
            int nextY = y + direction.y;

            // Grid 범위 밖 제외
            if (nextX < 0 || nextY < 0 || nextX >= width || nextY >= height)
                continue;

            // 아직 벽인 셀만 대상
            if (!grid.GetCell(nextX, nextY).Walkable)
            {
                Vector2Int next = new Vector2Int(nextX, nextY);

                if (frontierSet.Add(next))
                    frontier.Add(next);
            }
        }
    }

    // 현재 셀에서 연결 가능한 2칸 떨어진 통로 셀 탐색
    static List<Vector2Int> GetNeighbors(GridSystem grid, Vector2Int pos, int width, int height)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (Vector2Int dir in _directions)
        {
            // 2칸 이동
            Vector2Int direction = dir * 2; 
            int nextX = pos.x + direction.x;
            int nextY = pos.y + direction.y;

            // Grid 범위 체크
            if (nextX < 0 || nextY < 0 || nextX >= width || nextY >= height)
                continue; 

            // 이미 연결된 통로만 반환
            if (grid.GetCell(nextX, nextY).Walkable)
                result.Add(new Vector2Int(nextX, nextY)); 
        }

        return result;
    }

    // 주변 1칸 내 통로 개수를 체크하여 사이클 생성 여부 판단
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

            // 주변 통로 개수 카운트
            if (grid.GetCell(nextX, nextY).Walkable)
                pathCount++; 
        }

        // 2개 이상 연결 시 사이클 발생 → 제한
        return pathCount <= 1; 
    }
    #endregion
}