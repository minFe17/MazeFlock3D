using UnityEngine;

/// <summary>
/// 2D Grid 기반의 맵 데이터를 관리하는 시스템
/// </summary>
public class GridSystem
{
    GridCell[] _cells;

    int _width;
    int _height;

    public int Width { get => _width; }
    public int Height { get => _height; }

    public void CreateGrid(int width, int height)
    {
        _width = width;
        _height = height;

        _cells = new GridCell[width * height];

        for (int i = 0; i < _cells.Length; i++)
        {
            _cells[i].Walkable = true;
        }
    }

    public int GetIndex(int x, int y)
    {
        return y * Width + x;
    }

    public Vector2Int GetPosition(int index)
    {
        int x = index % Width;
        int y = index / Width;

        return new Vector2Int(x, y);
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < Width &&
               y >= 0 && y < Height;
    }

    public GridCell GetCell(int x, int y)
    {
        int index = GetIndex(x, y);
        return _cells[index];
    }

    public void SetCell(int x, int y, GridCell cell)
    {
        int index = GetIndex(x, y);
        _cells[index] = cell;
    }
}