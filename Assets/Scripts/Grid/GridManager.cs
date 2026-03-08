using UnityEngine;
using Unity.Collections;

public class GridManager : MonoBehaviour
{
    // 추후에 싱글턴 + 툴과 연동
    int _width = 50;
    int _height = 50;

    public int Width { get => _width; }
    public int Height { get => _height; }

    NativeArray<GridCell> _grid;

    void Start()
    {
        _grid = new NativeArray<GridCell>(_width * _height, Allocator.Persistent);

        for (int i = 0; i < _grid.Length; i++)
        {
            _grid[i] = new GridCell
            {
                Walkable = true
            };
        }
    }

    public int GetIndex(int x, int y)
    {
        return x + y * _width;
    }

    public  void GetXY(int index, out int x, out int y)
    {
        x = index % _width;
        y = index / _width;
    }

    void OnDestroy()
    {
        if (_grid.IsCreated)
            _grid.Dispose();
    }
}