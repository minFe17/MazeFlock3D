#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A* 실행, Grid 관리, Gizmos 시각화 담당
/// </summary>
public class Pathfinding : MonoBehaviour
{
    int _width = 10;
    int _height = 10;

    GridSystem _grid;
    Pathfinder _pathfinder;

    List<int> _path;

    public GridSystem GetGrid() => _grid;

    void Awake()
    {
        _grid = new GridSystem();
        _grid.CreateGrid(_width, _height);

        _pathfinder = new Pathfinder(_grid);

        MapBuilder.CreateObstacle(_grid, _width, _height);
    }

    public void Run(int start, int end)
    {
        _path = RunAndGetPath(start, end);
    }

    public List<int> RunAndGetPath(int start, int end)
    {
        if (start == -1 || end == -1)
            return null;

        if (start == end)
            return new List<int> { start };

        _pathfinder.BeginSearch(start, end);

        while (true)
        {
            bool reached = _pathfinder.Step(out int current);

            if (reached)
                return _pathfinder.BuildPath(end);

            if (_pathfinder.IsEmpty())
                return null;
        }
    }

    void OnDrawGizmos()
    {
        if (_grid == null) return;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                GridCell cell = _grid.GetCell(x, y);

                Vector3 pos = new Vector3(x, 0, y);

                Gizmos.color = cell.Walkable ? Color.white : Color.red;
                Gizmos.DrawWireCube(pos, Vector3.one);
#if UNITY_EDITOR
                Handles.Label(pos, $"{x},{y}");
#endif
            }
        }

        if (_path == null) return;

        Gizmos.color = Color.green;

        for (int i = 0; i < _path.Count - 1; i++)
        {
            Vector2Int a = _grid.GetPosition(_path[i]);
            Vector2Int b = _grid.GetPosition(_path[i + 1]);

            Gizmos.DrawLine(new Vector3(a.x, 0, a.y), new Vector3(b.x, 0, b.y));
        }

        foreach (int index in _path)
        {
            Vector2Int pos = _grid.GetPosition(index);
            Gizmos.DrawSphere(new Vector3(pos.x, 0, pos.y), 0.2f);
        }
    }
}