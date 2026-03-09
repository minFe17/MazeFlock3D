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

    GridCell[,] _grid;

    public int Width { get => _width; }
    public int Height { get => _height; }


    void Start()
    {
        InitGrid();

        // 테스트용
        ExploreNeighbors(5, 5);
    }

    // Grid 생성 및 초기화
    void InitGrid()
    {
        _grid = new GridCell[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
                _grid[x, y].Walkable = true;
        }
    }

    bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < _width && y < _height;
    }

    void ExploreNeighbors(int x, int y)
    {
        foreach (Vector2Int dir in _neighbors)
        {
            int nextX = x + dir.x;
            int nextY = y + dir.y;

            if (!InBounds(nextX, nextY))
                continue;

            GridCell neighbor = _grid[nextX, nextY];

            if (!neighbor.Walkable)
                continue;

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
                Vector3 pos = new Vector3(x, 0, y);

                Gizmos.color = _grid[x, y].Walkable ? Color.white : Color.red;

                Gizmos.DrawWireCube(pos, Vector3.one);

#if UNITY_EDITOR
                Handles.Label(pos, $"{x},{y}");
#endif
            }
        }
    }
}