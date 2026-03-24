using UnityEngine;

/// <summary>
/// Grid 맵 생성 및 장애물 배치 담당 클래스
/// </summary>
public static class MapBuilder
{
    public static void CreateObstacle(GridSystem grid, int width, int height)
    {
        // 가로 벽
        for (int x = 0; x < width; x++)
        {
            GridCell cell = grid.GetCell(x, 5);
            cell.Walkable = false;
            grid.SetCell(x, 5, cell);
        }

        // 좁은 통로
        GridCell gap = grid.GetCell(4, 5);
        gap.Walkable = true;
        grid.SetCell(4, 5, gap);
    }
}