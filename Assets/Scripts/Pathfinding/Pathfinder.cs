using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A* Pathfinding 핵심 로직 담당
/// OpenList를 MinHeap으로 최적화한 버전
/// </summary>
public class Pathfinder
{
    GridSystem _grid;

    MinHeap _openList;              
    bool[] _closedSet;

    int _endIndex;

    public int VisitedNodeCount { get; private set; }

    public Pathfinder(GridSystem grid)
    {
        _grid = grid;

        int size = _grid.Width * _grid.Height;
        _closedSet = new bool[size];
    }

    #region Heuristic
    int Heuristic(int first, int second)
    {
        int width = _grid.Width;

        int firstX = first % width;
        int firstY = first / width;

        int secondX = second % width;
        int secondY = second / width;

        return Mathf.Abs(firstX - secondX) + Mathf.Abs(firstY - secondY);
    }
    #endregion

    #region Init
    public void BeginSearch(int startIndex, int endIndex)
    {
        _endIndex = endIndex;

        _openList = new MinHeap(_grid);
        VisitedNodeCount = 0;

        int size = _closedSet.Length;

        // Closed 초기화
        for (int i = 0; i < size; i++)
            _closedSet[i] = false;

        _grid.ResetNodes();

        // Node 초기화
        for (int i = 0; i < size; i++)
        {
            PathNode node = _grid.GetNode(i);
            node.CostFromStart = int.MaxValue;
            node.ParentIndex = -1;
            _grid.SetNode(i, node);
        }

        // Start 설정
        PathNode startNode = _grid.GetNode(startIndex);
        startNode.CostFromStart = 0;
        startNode.CostToGoal = Heuristic(startIndex, endIndex);

        _grid.SetNode(startIndex, startNode);

        _openList.Push(startIndex);
    }
    #endregion

    #region Path Build
    public List<int> BuildPath(int endIndex)
    {
        List<int> path = new List<int>();

        int current = endIndex;

        while (current != -1)
        {
            path.Add(current);
            current = _grid.GetNode(current).ParentIndex;
        }

        path.Reverse();
        return path;
    }
    #endregion

    #region OpenList (Heap 기반)
    // MinHeap에서 최소 비용 노드 반환
    public int GetLowestCostNode()
    {
        while (true)
        {
            int result = _openList.Pop();

            if (result == -1)
                return -1;

            // 이미 처리된 노드면 skip
            if (_closedSet[result])
                continue;

            _closedSet[result] = true;
            return result;
        }
    }
    #endregion

    #region Expand
    public void ExpandNode(int currentIndex)
    {
        int width = _grid.Width;
        int height = _grid.Height;

        int[] offsets = { -width, width, -1, 1 };

        PathNode currentNode = _grid.GetNode(currentIndex);

        for (int i = 0; i < offsets.Length; i++)
        {
            int neighborIndex = currentIndex + offsets[i];

            // 범위 체크
            if (neighborIndex < 0 || neighborIndex >= width * height)
                continue;

            int currentX = currentIndex % width;
            int neighborX = neighborIndex % width;

            // 좌우 wrap 방지
            if (Mathf.Abs(currentX - neighborX) > 1)
                continue;

            // Closed 체크
            if (_closedSet[neighborIndex])
                continue;

            // 장애물 체크
            int x = neighborIndex % width;
            int y = neighborIndex / width;

            if (!_grid.GetCell(x, y).Walkable)
                continue;

            PathNode neighborNode = _grid.GetNode(neighborIndex);

            int newCost = currentNode.CostFromStart + 1;

            // 비용 비교
            if (newCost < neighborNode.CostFromStart)
            {
                neighborNode.CostFromStart = newCost;
                neighborNode.CostToGoal = Heuristic(neighborIndex, _endIndex);
                neighborNode.ParentIndex = currentIndex;

                _grid.SetNode(neighborIndex, neighborNode);

                _openList.Push(neighborIndex);
            }
        }
    }
    #endregion

    #region Step
    public bool Step(out int currentIndex)
    {
        currentIndex = GetLowestCostNode();

        if (currentIndex == -1)
            return false;

        VisitedNodeCount++;

        if (currentIndex == _endIndex)
            return true;

        ExpandNode(currentIndex);

        return false;
    }
    #endregion

    #region Utils
    public bool IsEmpty()
    {
        return _openList.Count == 0;
    }

    public bool IsClosed(int index)
    {
        return _closedSet[index];
    }
    #endregion
}