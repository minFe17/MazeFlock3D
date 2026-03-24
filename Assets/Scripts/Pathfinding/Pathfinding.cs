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
        if (start < 0 || end < 0)
        {
            Debug.LogError("Invalid start/end index");
            return null;
        }

        if (start == end)
            return new List<int> { start };

        _pathfinder.BeginSearch(start, end);

        int maxIteration = _grid.Width * _grid.Height;
        int iteration = 0;

        while (true)
        {
            iteration++;

            if (iteration > maxIteration)
            {
                Debug.LogError("Pathfinding 무한 루프 방지 (MaxIteration 초과)");
                return null;
            }

            bool reached = _pathfinder.Step(out int current);

            if (reached)
            {
                List<int> rawPath = _pathfinder.BuildPath(end);
                return SmoothPath(rawPath);
            }

            if (_pathfinder.IsEmpty())
                return null;
        }
    }

    List<int> SmoothPath(List<int> path)
    {
        if (path == null || path.Count < 3)
            return path;

        List<int> result = new List<int>();
        int width = _grid.Width;

        // 시작점 추가
        result.Add(path[0]);

        for (int i = 1; i < path.Count - 1; i++)
        {
            int prev = path[i - 1];
            int current = path[i];
            int next = path[i + 1];

            Vector2Int dir1 = GetDirection(prev, current, width);
            Vector2Int dir2 = GetDirection(current, next, width);

            // 방향이 바뀌는 지점만 유지
            if (dir1 != dir2)
                result.Add(current);
        }

        // 끝점 추가
        result.Add(path[path.Count - 1]);

        return result;
    }

    Vector2Int GetDirection(int from, int to, int width)
    {
        int fromX = from % width;
        int fromY = from / width;

        int toX = to % width;
        int toY = to / width;

        return new Vector2Int(toX - fromX, toY - fromY);
    }

    #region Gizmos
    void OnDrawGizmos()
    {
        if (_grid == null) return;

        // Grid
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

        if (_path == null) 
            return;

        // Path
        Gizmos.color = Color.green;

        for (int i = 0; i < _path.Count - 1; i++)
        {
            Vector2Int a = _grid.GetPosition(_path[i]);
            Vector2Int b = _grid.GetPosition(_path[i + 1]);

            Gizmos.DrawLine(new Vector3(a.x, 0, a.y), new Vector3(b.x, 0, b.y));
        }

        // Node 점
        foreach (int index in _path)
        {
            Vector2Int pos = _grid.GetPosition(index);
            Gizmos.DrawSphere(new Vector3(pos.x, 0, pos.y), 0.2f);
        }
    }
    #endregion
}