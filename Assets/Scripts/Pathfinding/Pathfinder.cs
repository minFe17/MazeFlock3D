using System.Collections.Generic;
using UnityEngine;

public class Pathfinder
{
    GridSystem _grid;

    List<int> _openList = new List<int>();
    bool[] _closedSet;
    bool[] _inOpenSet; 

    int _endIndex;

    public int VisitedNodeCount { get; private set; }

    public Pathfinder(GridSystem grid)
    {
        _grid = grid;

        int size = _grid.Width * _grid.Height;
        _closedSet = new bool[size];
        _inOpenSet = new bool[size];
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

        for (int i = 0; i < size; i++)
        {
            _closedSet[i] = false;
            _inOpenSet[i] = false; 
        }

        _grid.ResetNodes();

        for (int i = 0; i < size; i++)
        {
            PathNode node = _grid.GetNode(i);
            node.CostFromStart = int.MaxValue;
            node.ParentIndex = -1;
            _grid.SetNode(i, node);
        }

        PathNode startNode = _grid.GetNode(startIndex);
        startNode.CostFromStart = 0;
        startNode.CostToGoal = Heuristic(startIndex, endIndex);

        _grid.SetNode(startIndex, startNode);

        _openList.Add(startIndex);
        _inOpenSet[startIndex] = true; 
    }

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

    public int GetLowestCostNode()
    {
        if (_openList.Count == 0)
            return -1;

        int bestIndex = 0;
        int bestNodeIndex = _openList[0];
        PathNode bestNode = _grid.GetNode(bestNodeIndex);

        for (int i = 1; i < _openList.Count; i++)
        {
            int nodeIndex = _openList[i];
            PathNode node = _grid.GetNode(nodeIndex);

            if (node.TotalCost < bestNode.TotalCost)
            {
                bestNode = node;
                bestIndex = i;
            }
        }

        int result = _openList[bestIndex];

        _openList.RemoveAt(bestIndex);
        _closedSet[result] = true;
        _inOpenSet[result] = false;

        return result;
    }

    public void ExpandNode(int currentIndex)
    {
        int width = _grid.Width;
        int height = _grid.Height;

        int[] offsets = { -width, width, -1, 1 };

        PathNode currentNode = _grid.GetNode(currentIndex);

        for (int i = 0; i < offsets.Length; i++)
        {
            int neighborIndex = currentIndex + offsets[i];

            // 1. 범위 체크
            if (neighborIndex < 0 || neighborIndex >= width * height)
                continue;

            int currentX = currentIndex % width;
            int neighborX = neighborIndex % width;

            // 좌우 wrap 방지
            if (Mathf.Abs(currentX - neighborX) > 1)
                continue;

            // 2. Closed 체크
            if (_closedSet[neighborIndex])
                continue;

            // 3. 장애물 체크
            int x = neighborIndex % width;
            int y = neighborIndex / width;

            if (!_grid.GetCell(x, y).Walkable)
                continue;

            PathNode neighborNode = _grid.GetNode(neighborIndex);

            int newCost = currentNode.CostFromStart + 1;

            // 4. 비용 비교
            if (newCost < neighborNode.CostFromStart)
            {
                neighborNode.CostFromStart = newCost;
                neighborNode.CostToGoal = Heuristic(neighborIndex, _endIndex);
                neighborNode.ParentIndex = currentIndex;

                _grid.SetNode(neighborIndex, neighborNode);

                // Contains 제거 → O(1)
                if (!_inOpenSet[neighborIndex])
                {
                    _openList.Add(neighborIndex);
                    _inOpenSet[neighborIndex] = true;
                }
            }
        }
    }

    public bool Step(out int currentIndex)
    {
        currentIndex = GetLowestCostNode();

        // 안전 처리
        if (currentIndex == -1)
            return false;

        VisitedNodeCount++;

        if (currentIndex == _endIndex)
            return true;

        ExpandNode(currentIndex);

        return false;
    }

    public bool IsEmpty()
    {
        return _openList.Count == 0;
    }

    public bool IsClosed(int index)
    {
        return _closedSet[index];
    }
}