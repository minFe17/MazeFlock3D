using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class MinHeap
{
    readonly List<int> _heap;
    readonly NativeArray<PathNode> _nodes;

    public int Count => _heap.Count;

    public MinHeap(NativeArray<PathNode> nodes)
    {
        _heap = new List<int>();
        _nodes = nodes;
    }


    bool IsLess(int aIndex, int bIndex)
    {
        PathNode a = _nodes[aIndex];
        PathNode b = _nodes[bIndex];

        if (a.TotalCost != b.TotalCost)
            return a.TotalCost < b.TotalCost;

        return a.CostToGoal < b.CostToGoal;
    }

    void HeapifyDown(int current)
    {
        int count = _heap.Count;

        while (true)
        {
            int left = current * 2 + 1;
            int right = current * 2 + 2;
            int smallest = current;

            if (left < count && IsLess(_heap[left], _heap[smallest]))
                smallest = left;

            if (right < count && IsLess(_heap[right], _heap[smallest]))
                smallest = right;

            if (smallest == current)
                break;

            (_heap[current], _heap[smallest]) = (_heap[smallest], _heap[current]);
            current = smallest;
        }
    }

    public void Push(int index)
    {
        _heap.Add(index);

        int current = _heap.Count - 1;

        while (current > 0)
        {
            int parent = (current - 1) / 2;

            if (IsLess(_heap[current], _heap[parent]))
            {
                (_heap[current], _heap[parent]) = (_heap[parent], _heap[current]);
                current = parent;
            }
            else
                break;
        }
    }

    public int Pop()
    {
        if (_heap.Count == 0)
            return -1;

        int root = _heap[0];

        int last = _heap[_heap.Count - 1];
        _heap.RemoveAt(_heap.Count - 1);

        if (_heap.Count > 0)
        {
            _heap[0] = last;
            HeapifyDown(0);
        }

        return root;
    }

    public void Clear()
    {
        _heap.Clear();
    }
}