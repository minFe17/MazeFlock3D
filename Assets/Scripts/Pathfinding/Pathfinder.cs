using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A* Pathfinding 핵심 로직 담당
/// OpenList를 MinHeap으로 최적화한 버전
/// GC 및 불필요 연산 제거 적용
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
        _openList = new MinHeap(_grid);

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
        _openList.Clear();
        VisitedNodeCount = 0;

        int size = _closedSet.Length;

        // Closed 초기화
        for (int i = 0; i < size; i++)
            _closedSet[i] = false;

        _grid.ResetNodes();

        // Start 설정
        PathNode startNode = _grid.GetNode(startIndex);
        startNode.CostFromStart = 0;
        startNode.CostToGoal = Heuristic(startIndex, endIndex);

        _grid.SetNode(startIndex, startNode);

        _openList.Push(startIndex);
    }
    #endregion

    #region Path Build
    public void BuildPath(int endIndex, List<int> buffer)
    {
        buffer.Clear();

        int current = endIndex;

        while (current != -1)
        {
            buffer.Add(current);
            current = _grid.GetNode(current).ParentIndex;
        }

        buffer.Reverse();
    }
    #endregion

    #region OpenList (Heap 기반)
    public int GetLowestCostNode()
    {
        while (true)
        {
            int result = _openList.Pop();

            if (result == -1)
                return -1;

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
        int size = width * height;

        PathNode currentNode = _grid.GetNode(currentIndex);
        int currentX = currentIndex % width;

        int newCost = currentNode.CostFromStart + 1;

        int up = currentIndex - width;
        if (up >= 0 && !_closedSet[up])
        {
            if (_grid.GetCell(up).Walkable)
                ProcessNeighbor(currentIndex, up, newCost);
        }

        int down = currentIndex + width;
        if (down < size && !_closedSet[down])
        {
            if (_grid.GetCell(down).Walkable)
                ProcessNeighbor(currentIndex, down, newCost);
        }

        int left = currentIndex - 1;
        if (left >= 0 && !_closedSet[left] && (left % width) == currentX - 1)
        {
            if (_grid.GetCell(left).Walkable)
                ProcessNeighbor(currentIndex, left, newCost);
        }

        int right = currentIndex + 1;
        if (right < size && !_closedSet[right] && (right % width) == currentX + 1)
        {
            if (_grid.GetCell(right).Walkable)
                ProcessNeighbor(currentIndex, right, newCost);
        }
    }
    #endregion

    #region Neighbor 처리 분리
    void ProcessNeighbor(int currentIndex, int neighborIndex, int newCost)
    {
        PathNode neighborNode = _grid.GetNode(neighborIndex);

        if (newCost < neighborNode.CostFromStart)
        {
            neighborNode.CostFromStart = newCost;
            neighborNode.CostToGoal = Heuristic(neighborIndex, _endIndex);
            neighborNode.ParentIndex = currentIndex;

            _grid.SetNode(neighborIndex, neighborNode);

            _openList.Push(neighborIndex);
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