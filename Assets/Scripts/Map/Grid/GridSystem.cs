using Unity.Collections;
using UnityEngine;

/// <summary>
/// 2D Grid 기반의 맵 데이터를 관리하는 시스템
/// </summary>
public class GridSystem
{
    NativeArray<bool> _walkables;
    NativeArray<PathNode> _nodes;

    int _width;
    int _height;

    public NativeArray<bool> Walkables => _walkables;
    public NativeArray<PathNode> Nodes => _nodes;
    public int Width => _width;
    public int Height => _height;

    public void CreateGrid(int width, int height)
    {
        Dispose();

        _width = width;
        _height = height;

        int size = width * height;
        _walkables = new NativeArray<bool>(size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        _nodes = new NativeArray<PathNode>(size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        for (int i = 0; i < size; i++)
            _walkables[i] = true;

        // PathNode 초기화
        ResetNodes();
    }

    public void Dispose()
    {
        if (_walkables.IsCreated)
            _walkables.Dispose();

        if (_nodes.IsCreated)
            _nodes.Dispose();
    }

    public void ResetNodes()
    {
        for (int i = 0; i < _nodes.Length; i++)
        {
            PathNode node = default;
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
        return new GridCell { Walkable = _walkables[index] };
    }

    public void SetCell(int x, int y, GridCell cell)
    {
        int index = GetIndex(x, y);
        _walkables[index] = cell.Walkable;
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
        _walkables[index] = walkable;
    }

    public void SetWalkable(int index, bool walkable)
    {
        _walkables[index] = walkable;
    }

    public GridCell GetCell(int index)
    {
        return new GridCell { Walkable = _walkables[index] };
    }
}