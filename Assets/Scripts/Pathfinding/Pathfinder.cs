using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// A* Pathfinding 핵심 로직 담당
/// OpenList를 MinHeap으로 최적화한 버전
/// GC 및 불필요 연산 제거 적용
/// </summary>
public class Pathfinder
{
    readonly GridSystem _grid;
    readonly NativeArray<bool> _walkables;
    NativeArray<PathNode> _nodes;

    MinHeap _openList;
    bool[] _closedSet;

    int _endIndex;

    public int VisitedNodeCount { get; private set; }

    public Pathfinder(GridSystem grid)
    {
        _grid = grid;
        _walkables = grid.Walkables;
        _nodes = grid.Nodes;

        _openList = new MinHeap(_nodes);

        int size = _grid.Width * _grid.Height;
        _closedSet = new bool[size];
    }

    int Heuristic(int first, int second)
    {
        int width = _grid.Width;

        int firstX = first % width;
        int firstY = first / width;

        int secondX = second % width;
        int secondY = second / width;

        return Mathf.Abs(firstX - secondX) + Mathf.Abs(firstY - secondY);
    }

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
        PathNode startNode = _nodes[startIndex];
        startNode.CostFromStart = 0;
        startNode.CostToGoal = Heuristic(startIndex, endIndex);

        _nodes[startIndex] = startNode;

        _openList.Push(startIndex);
    }

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

    public void ExpandNode(int currentIndex)
    {
        int width = _grid.Width;
        int height = _grid.Height;
        int size = width * height;

        PathNode currentNode = _nodes[currentIndex];
        int currentX = currentIndex % width;

        int newCost = currentNode.CostFromStart + 1;

        int up = currentIndex - width;
        if (up >= 0 && !_closedSet[up] && _walkables[up])
            ProcessNeighbor(currentIndex, up, newCost);

        int down = currentIndex + width;
        if (down < size && !_closedSet[down] && _walkables[down])
            ProcessNeighbor(currentIndex, down, newCost);

        int left = currentIndex - 1;
        if (left >= 0 && !_closedSet[left] && (left % width) == currentX - 1 && _walkables[left])
            ProcessNeighbor(currentIndex, left, newCost);

        int right = currentIndex + 1;
        if (right < size && !_closedSet[right] && (right % width) == currentX + 1 && _walkables[right])
            ProcessNeighbor(currentIndex, right, newCost);
    }

    void ProcessNeighbor(int currentIndex, int neighborIndex, int newCost)
    {
        PathNode neighborNode = _nodes[neighborIndex];

        if (newCost < neighborNode.CostFromStart)
        {
            neighborNode.CostFromStart = newCost;
            neighborNode.CostToGoal = Heuristic(neighborIndex, _endIndex);
            neighborNode.ParentIndex = currentIndex;

            _nodes[neighborIndex] = neighborNode;

            _openList.Push(neighborIndex);
        }
    }

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