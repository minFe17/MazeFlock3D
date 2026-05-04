using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// A* Pathfinding 핵심 로직 담당
/// </summary>
public class Pathfinder
{
    readonly NativeArray<byte> _walkables;
    NativeArray<PathNode> _nodes;

    readonly int _width;
    readonly int _height;

    MinHeap _openList;
    NativeArray<byte> _state;

    const byte STATE_NONE = 0;
    const byte STATE_OPEN = 1;
    const byte STATE_CLOSED = 2;

    int _endIndex;

    public int VisitedNodeCount { get; private set; }

    public Pathfinder(NativeArray<byte> walkables, NativeArray<PathNode> nodes, int width, int height)
    {
        _walkables = walkables;
        _nodes = nodes;
        _width = width;
        _height = height;

        int size = width * height;

        _state = new NativeArray<byte>(size, Allocator.Persistent);
        _openList = new MinHeap(_nodes);
    }

    int Heuristic(int first, int second)
    {
        int firstX = first % _width;
        int firstY = first / _width;

        int secondX = second % _width;
        int secondY = second / _width;

        return Mathf.Abs(firstX - secondX) + Mathf.Abs(firstY - secondY);
    }

    public void BeginSearch(int startIndex, int endIndex)
    {
        _endIndex = endIndex;
        VisitedNodeCount = 0;

        for (int i = 0; i < _state.Length; i++)
            _state[i] = STATE_NONE;

        // ResetNodes 직접 처리 (GridSystem 제거)
        for (int i = 0; i < _nodes.Length; i++)
        {
            PathNode node = _nodes[i];
            node.Init();
            _nodes[i] = node;
        }

        // Start 설정
        PathNode startNode = _nodes[startIndex];
        startNode.CostFromStart = 0;
        startNode.CostToGoal = Heuristic(startIndex, endIndex);
        startNode.TotalCost = startNode.CostFromStart + startNode.CostToGoal;

        _nodes[startIndex] = startNode;

        _state[startIndex] = STATE_OPEN;
        _openList.Clear();
        _openList.Push(startIndex);
    }

    public void BuildPath(int endIndex, List<int> buffer)
    {
        buffer.Clear();

        int current = endIndex;

        while (current != -1)
        {
            buffer.Add(current);
            current = _nodes[current].ParentIndex;
        }

        buffer.Reverse();
    }

    int GetLowestCostNode()
    {
        while (_openList.Count > 0)
        {
            int index = _openList.Pop();

            if (_state[index] == STATE_CLOSED)
                continue;

            _state[index] = STATE_CLOSED;
            return index;
        }

        return -1;
    }

    public void ExpandNode(int currentIndex)
    {
        int size = _width * _height;

        PathNode currentNode = _nodes[currentIndex];
        int currentX = currentIndex % _width;

        int newCost = currentNode.CostFromStart + 1;

        int up = currentIndex - _width;
        if (up >= 0 && _state[up] != STATE_CLOSED && _walkables[up] == 1)
            ProcessNeighbor(currentIndex, up, newCost);

        int down = currentIndex + _width;
        if (down < size && _state[down] != STATE_CLOSED && _walkables[down] == 1)
            ProcessNeighbor(currentIndex, down, newCost);

        int left = currentIndex - 1;
        if (left >= 0 && _state[left] != STATE_CLOSED && (left % _width) == currentX - 1 && _walkables[left] == 1)
            ProcessNeighbor(currentIndex, left, newCost);

        int right = currentIndex + 1;
        if (right < size && _state[right] != STATE_CLOSED && (right % _width) == currentX + 1 && _walkables[right] == 1)
            ProcessNeighbor(currentIndex, right, newCost);
    }

    void ProcessNeighbor(int currentIndex, int neighborIndex, int newCost)
    {
        if (_state[neighborIndex] == STATE_CLOSED)
            return;

        PathNode neighborNode = _nodes[neighborIndex];

        if (newCost < neighborNode.CostFromStart)
        {
            neighborNode.CostFromStart = newCost;
            neighborNode.CostToGoal = Heuristic(neighborIndex, _endIndex);
            neighborNode.TotalCost = neighborNode.CostFromStart + neighborNode.CostToGoal;
            neighborNode.ParentIndex = currentIndex;

            _nodes[neighborIndex] = neighborNode;

            _state[neighborIndex] = STATE_OPEN;
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

    public void Dispose()
    {
        if (_state.IsCreated)
            _state.Dispose();
    }

    #region Utils
    public bool IsEmpty()
    {
        return _openList.Count == 0;
    }

    public bool IsClosed(int index)
    {
        return _state[index] == STATE_CLOSED;
    }
    #endregion
}