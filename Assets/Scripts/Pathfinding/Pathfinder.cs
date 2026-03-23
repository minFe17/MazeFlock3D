using System.Collections.Generic;
using UnityEngine;

public class Pathfinder
{
    GridSystem _grid;

    List<int> _openList = new List<int>();
    bool[] _closedSet;

    int _endIndex;

    public Pathfinder(GridSystem grid)
    {
        _grid = grid;
        _closedSet = new bool[_grid.Width * _grid.Height];
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

        for (int i = 0; i < _closedSet.Length; i++)
            _closedSet[i] = false;

        _grid.ResetNodes();

        PathNode startNode = _grid.GetNode(startIndex);
        startNode.CostFromStart = 0;
        startNode.CostToGoal = Heuristic(startIndex, endIndex);

        _grid.SetNode(startIndex, startNode);

        _openList.Add(startIndex);
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
        if (IsEmpty())
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

            PathNode neighborNode = _grid.GetNode(neighborIndex);

            // 3. CostFromStart 계산
            int moveCost = 1;
            int newCost = currentNode.CostFromStart + moveCost;

            // 4. 더 짧은 경로면 갱신
            if (newCost < neighborNode.CostFromStart)
            {
                neighborNode.CostFromStart = newCost;
                neighborNode.CostToGoal = Heuristic(neighborIndex, _endIndex);
                neighborNode.ParentIndex = currentIndex;

                _grid.SetNode(neighborIndex, neighborNode);

                // 5. OpenList 추가
                if (!_openList.Contains(neighborIndex))
                    _openList.Add(neighborIndex);
            }
        }
    }

    public bool Step(out int currentIndex)
    {
        // 가장 좋은 노드 선택
        currentIndex = GetLowestCostNode();

        // 목표 도달 체크
        if (currentIndex == _endIndex)
            return true;

        // 이웃 확장
        ExpandNode(currentIndex);

        return false;
    }

    public void Debug_AddNode(int index, int cost)
    {
        PathNode node = _grid.GetNode(index);
        node.CostFromStart = cost;
        node.CostToGoal = 0;

        _grid.SetNode(index, node);

        _openList.Add(index);
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