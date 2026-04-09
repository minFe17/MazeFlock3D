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
    [SerializeField] EMapType _mapType;

    [Header("Map Size")]
    [SerializeField] int _width = 50;
    [SerializeField] int _height = 50;

    GridSystem _grid;
    Pathfinder _pathfinder;

    List<int> _path = new List<int>();
    List<int> _rawPathBuffer = new List<int>();
    List<int> _smoothBuffer = new List<int>();

    public GridSystem GetGrid() => _grid;

    void Awake()
    {
        Debug.Log($"Map Seed: {_mapSeed}");
        Random.InitState(_mapSeed);

        _grid = new GridSystem();
        _grid.CreateGrid(_width, _height);

        _pathfinder = new Pathfinder(_grid);

        switch (_mapType)
        {
            case EMapType.PrimMaze:
                MapBuilder.CreatePrimMaze(_grid, _width, _height);
                break;

            case EMapType.Obstacle:
                MapBuilder.CreateObstacle(_grid, _width, _height, 0.7f);
                break;
        }
    }

    #region SmoothPath (GC 제거)
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
    #endregion

    Vector2Int GetDirection(int from, int to, int width)
    {
        int fromX = from % width;
        int fromY = from / width;

        int toX = to % width;
        int toY = to / width;

        return new Vector2Int(toX - fromX, toY - fromY);
    }

    #region Run Path
    public List<int> RunAndGetPath(int start, int end)
    {
        if (start < 0 || end < 0)
        {
            Debug.LogError("Invalid start/end index");
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

            bool reached = _pathfinder.Step(out int current);

            if (reached)
            {
                _rawPathBuffer.Clear();
                _pathfinder.BuildPath(end, _rawPathBuffer);

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

        if (start == end)
        {
            _rawPathBuffer.Clear();
            _rawPathBuffer.Add(start);
            return _rawPathBuffer;
        }

        _pathfinder.BeginSearch(start, end);

        while (true)
        {
            bool reached = _pathfinder.Step(out int current);

            if (reached)
            {
                _rawPathBuffer.Clear();
                _pathfinder.BuildPath(end, _rawPathBuffer);
                return _rawPathBuffer;
            }

            if (_pathfinder.IsEmpty())
                return null;
        }
    }
    #endregion

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