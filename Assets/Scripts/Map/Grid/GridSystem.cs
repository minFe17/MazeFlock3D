using Unity.Collections;

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

        _walkables = new NativeArray<bool>(size, Allocator.Persistent);
        _nodes = new NativeArray<PathNode>(size, Allocator.Persistent);

        for (int i = 0; i < size; i++)
            _walkables[i] = true;

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

    public void SetWalkable(int index, bool walkable)
    {
        _walkables[index] = walkable;
    }
}