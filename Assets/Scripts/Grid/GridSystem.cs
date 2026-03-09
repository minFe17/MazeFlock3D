using UnityEngine;

/// <summary>
/// 2D Grid 기반의 맵 데이터를 관리하는 시스템
/// </summary>
public class GridSystem
{
    GridCell[] _cells;
    PathNode[] _nodes;

    int _width;
    int _height;

    public int Width { get => _width; }
    public int Height { get => _height; }

    public void CreateGrid(int width, int height)
    {
        _width = width;
        _height = height;

        _cells = new GridCell[width * height];
        _nodes = new PathNode[width * height];

        // GridCell 초기화
        for (int i = 0; i < _cells.Length; i++)
            _cells[i].Walkable = true;

        // PathNode 초기화
        for (int i = 0; i < _nodes.Length; i++)
        {
            _nodes[i] = new PathNode
            {
                CostFromStart = int.MaxValue,
                CostToGoal = 0,
                ParentIndex = -1
            };
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

    public PathNode GetNode(int x, int y)
    {
        int index = GetIndex(x, y);
        return _nodes[index];
    }

    public void SetNode(int x, int y, PathNode node)
    {
        int index = GetIndex(x, y);
        _nodes[index] = node;
    }
}