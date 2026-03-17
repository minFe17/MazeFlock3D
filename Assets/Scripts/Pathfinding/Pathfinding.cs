#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

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

    // Benchmark
    BenchmarkManager _benchmark = new BenchmarkManager();
    BenchmarkResult _result = new BenchmarkResult();

    int _visitedNodes;

    public int Width { get => _width; }
    public int Height { get => _height; }


    void Start()
    {
        InitGrid();

        int startIndex = _grid.GetIndex(5, 5);

        _pathfinder.BeginSearch(startIndex);

        // 테스트용 더미 노드 추가
        _pathfinder.Debug_AddNode(_grid.GetIndex(0, 0), 30);
        _pathfinder.Debug_AddNode(_grid.GetIndex(1, 0), 10);
        _pathfinder.Debug_AddNode(_grid.GetIndex(2, 0), 50);
        _pathfinder.Debug_AddNode(_grid.GetIndex(3, 0), 20);

        while (true)
        {
            int node = _pathfinder.GetLowestFCostNode();
            Debug.Log($"선택: {node}");

            if (_pathfinder.IsEmpty())
                break;
        }
    }

    // Grid 생성 및 초기화
    void InitGrid()
    {
        _grid = new GridSystem();
        _grid.CreateGrid(_width, _height);

        _pathfinder = new Pathfinder(_grid);
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
    }
}