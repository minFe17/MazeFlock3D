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
        ResetNodes();
    }

    public void ResetNodes()
    {
        for (int i = 0; i < _nodes.Length; i++)
        {
            PathNode node = _nodes[i];
            node.Init();
            _nodes[i] = node;
        }
    }

    public int GetIndex(int x, int y)
    {
        return y * _width + x;
    }

    public Vector2Int GetPosition(int index)
    {
        int x = index % _width;
        int y = index / _width;

        return new Vector2Int(x, y);
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height;
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

    public PathNode GetNode(int index)
    {
        return _nodes[index];
    }

    public void SetNode(int x, int y, PathNode node)
    {
        int index = GetIndex(x, y);
        _nodes[index] = node;
    }

    public void SetNode(int index, PathNode node)
    {
        _nodes[index] = node;
    }

    public void SetWalkable(int x, int y, bool walkable)
    {
        int index = GetIndex(x, y);
        GridCell cell = _cells[index];
        cell.Walkable = walkable;
        _cells[index] = cell;
    }

    public void SetWalkable(int index, bool walkable)
    {
        GridCell cell = _cells[index];
        cell.Walkable = walkable;
        _cells[index] = cell;
    }

    public GridCell GetCell(int index)
    {
        return _cells[index];
    }
}