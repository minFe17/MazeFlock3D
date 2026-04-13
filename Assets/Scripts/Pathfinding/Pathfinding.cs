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
    [SerializeField] int _mapSeed;

    [Header("Map Size")]
    [SerializeField] int _width = 50;
    [SerializeField] int _height = 50;

    GridSystem _grid;
    Pathfinder _pathfinder;

    List<int> _path = new List<int>();
    List<int> _rawPathBuffer = new List<int>();
    List<int> _smoothBuffer = new List<int>();

    public GridSystem GetGrid() => _grid;

    const int MaxPathBuildSafety = 100000;

    void Awake()
    {
        Debug.Log($"Map Seed: {_mapSeed}");
        Random.InitState(_mapSeed);

        _grid = new GridSystem();
        _grid.CreateGrid(_width, _height);

        _pathfinder = new Pathfinder(_grid);
        MapBuilder.CreatePrimMaze(_grid, _width, _height);
    }

    void OnDestroy()
    {
        if (_grid != null)
            _grid.Dispose();
    }


    List<int> SmoothPath(List<int> path)
    {
        if (path == null || path.Count < 3)
            return path;

        _smoothBuffer.Clear();

        int width = _grid.Width;

        // 시작점
        _smoothBuffer.Add(path[0]);

        for (int i = 1; i < path.Count - 1; i++)
        {
            int prev = path[i - 1];
            int current = path[i];
            int next = path[i + 1];

            Vector2Int dir1 = GetDirection(prev, current, width);
            Vector2Int dir2 = GetDirection(current, next, width);

            if (dir1 != dir2)
                _smoothBuffer.Add(current);
        }

        // 끝점
        _smoothBuffer.Add(path[path.Count - 1]);

        return _smoothBuffer;
    }

    Vector2Int GetDirection(int from, int to, int width)
    {
        int fromX = from % width;
        int fromY = from / width;

        int toX = to % width;
        int toY = to / width;

        return new Vector2Int(toX - fromX, toY - fromY);
    }

    bool IsWalkableIndex(int index)
    {
        if (index < 0 || index >= _grid.Width * _grid.Height)
            return false;

        return _grid.GetCell(index).Walkable;
    }

    bool TryBuildRawPath(int end, List<int> output)
    {
        output.Clear();

        int current = end;
        int safety = 0;

        while (current != -1)
        {
            if (++safety > MaxPathBuildSafety)
            {
                Debug.LogError("Path build safety break triggered (possible parent cycle)");
                output.Clear();
                return false;
            }

            output.Add(current);
            current = _grid.GetNode(current).ParentIndex;
        }

        output.Reverse();
        return true;
    }

    public List<int> RunAndGetPath(int start, int end)
    {
        if (start < 0 || end < 0)
        {
            Debug.LogError("Invalid start/end index");
            return null;
        }

        if (!IsWalkableIndex(start) || !IsWalkableIndex(end))
        {
            Debug.LogWarning("Start or end is blocked, skip pathfinding");
            return null;
        }

        if (start == end)
        {
            _path.Clear();
            _path.Add(start);
            return _path;
        }

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

            bool reached = _pathfinder.Step(out _);

            if (reached)
            {
                if (!TryBuildRawPath(end, _rawPathBuffer))
                    return null;

                _path = SmoothPath(_rawPathBuffer);
                return _path;
            }

            if (_pathfinder.IsEmpty())
                return null;
        }
    }

    public List<int> RunRawPath(int start, int end)
    {
        if (start < 0 || end < 0)
            return null;

        if (!IsWalkableIndex(start) || !IsWalkableIndex(end))
            return null;

        if (start == end)
        {
            _rawPathBuffer.Clear();
            _rawPathBuffer.Add(start);
            return _rawPathBuffer;
        }

        _pathfinder.BeginSearch(start, end);

        int maxIteration = _grid.Width * _grid.Height;
        int iteration = 0;

        while (true)
        {
            iteration++;
            if (iteration > maxIteration)
            {
                Debug.LogWarning("RunRawPath iteration exceeded max iteration");
                return null;
            }

            bool reached = _pathfinder.Step(out _);

            if (reached)
            {
                if (!TryBuildRawPath(end, _rawPathBuffer))
                    return null;
                return _rawPathBuffer;
            }

            if (_pathfinder.IsEmpty())
                return null;
        }
    }

    public int GetVisitedNodeCount()
    {
        return _pathfinder.VisitedNodeCount;
    }

    #region Gizmos
    void OnDrawGizmos()
    {
        if (_grid == null)
            return;

        // Grid
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                GridCell cell = _grid.GetCell(x, y);

                Vector3 pos = new Vector3(x, 0, y);

                Gizmos.color = cell.Walkable ? Color.white : Color.red;
                Gizmos.DrawWireCube(pos, Vector3.one);
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