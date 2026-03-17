using System.Collections.Generic;
using UnityEngine;

public class Pathfinder
{
    GridSystem _grid;

    List<int> _openList = new List<int>();
    bool[] _closedSet;

    public Pathfinder(GridSystem grid)
    {
        _grid = grid;
        _closedSet = new bool[_grid.Width * _grid.Height];
    }

    public void BeginSearch(int startIndex)
    {
        _openList.Clear();

        for (int i = 0; i < _closedSet.Length; i++)
            _closedSet[i] = false;

        PathNode startNode = _grid.GetNode(startIndex);
        startNode.CostFromStart = 0;
        startNode.CostToGoal = 0;

        _grid.SetNode(startIndex, startNode);

        _openList.Add(startIndex);
    }

    public int GetLowestFCostNode()
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
        return result;
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