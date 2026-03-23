#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Grid 기반 Pathfinding에서 현재 셀 주변의 이웃 노드를 탐색하는 클래스
/// </summary>
public class Pathfinding : MonoBehaviour
{
    // 4방향 이동
    readonly Vector2Int[] _neighbors = { Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up };

    int _width = 10;
    int _height = 10;

    GridSystem _grid;
    Pathfinder _pathfinder;

    List<int> _path;

    // Benchmark
    BenchmarkManager _benchmark = new BenchmarkManager();
    BenchmarkResult _result = new BenchmarkResult();

    int _visitedNodes;

    public int Width { get => _width; }
    public int Height { get => _height; }

    void Start()
    {
        InitGrid();
        CreateObstacle();

        int start = _grid.GetIndex(0, 0);
        int end = _grid.GetIndex(9, 9);

        _pathfinder.BeginSearch(start, end);

        while (true)
        {
            bool reached = _pathfinder.Step(out int current);

            if (reached)
            {
                Debug.Log("목표 도달!");

                _path = _pathfinder.BuildPath(end);
                break;
            }

            if (_pathfinder.IsEmpty())
            {
                Debug.Log("경로 없음");
                break;
            }
        }
    }

    // Grid 생성 및 초기화
    void InitGrid()
    {
        _grid = new GridSystem();
        _grid.CreateGrid(_width, _height);

        _pathfinder = new Pathfinder(_grid);
    }

    void CreateObstacle()
    {
        for (int x = 0; x < _width; x++)
        {
            GridCell cell = _grid.GetCell(x, 5);
            cell.Walkable = false;
            _grid.SetCell(x, 5, cell);
        }

        GridCell gap = _grid.GetCell(4, 5);
        gap.Walkable = true;
        _grid.SetCell(4, 5, gap);
    }

    void ExploreNeighbors(int x, int y)
    {
        foreach (Vector2Int dir in _neighbors)
        {
            int nextX = x + dir.x;
            int nextY = y + dir.y;

            if (!_grid.IsInBounds(nextX, nextY))
                continue;

            GridCell neighbor = _grid.GetCell(nextX, nextY);

            if (!neighbor.Walkable)
                continue;

            _visitedNodes++;

            Debug.Log($"Neighbor: {nextX}, {nextY}");
        }
    }

    void OnDrawGizmos()
    {
        if (_grid == null)
            return;

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

        // Path 표시
        if (_path == null)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < _path.Count - 1; i++)
        {
            Vector2Int a = _grid.GetPosition(_path[i]);
            Vector2Int b = _grid.GetPosition(_path[i + 1]);

            Vector3 posA = new Vector3(a.x, 0, a.y);
            Vector3 posB = new Vector3(b.x, 0, b.y);

            Gizmos.DrawLine(posA, posB);
        }

        // 노드 점 표시
        foreach (int index in _path)
        {
            Vector2Int pos = _grid.GetPosition(index);
            Gizmos.DrawSphere(new Vector3(pos.x, 0, pos.y), 0.2f);
        }
    }
}