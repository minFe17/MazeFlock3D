using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct AStarJob : IJob
{
    public int width;
    public int height;

    public int startIndex;
    public int endIndex;

    [ReadOnly] public NativeArray<byte> walkables;
    public NativeArray<PathNode> nodes;

    public NativeArray<byte> state;
    public NativeArray<int> openList;
    public NativeArray<int> result;

    const byte STATE_NONE = 0;
    const byte STATE_OPEN = 1;
    const byte STATE_CLOSED = 2;

    public void Execute()
    {
        int openCount = 0;

        Init(ref openCount);

        while (openCount > 0)
        {
            int current = Pop(ref openCount);

            if (current == endIndex)
            {
                result[0] = 1;
                return;
            }

            ExpandNode(current, ref openCount);
        }

        result[0] = 0;
    }

    void Init(ref int openCount)
    {
        for (int i = 0; i < state.Length; i++)
            state[i] = STATE_NONE;

        for (int i = 0; i < nodes.Length; i++)
        {
            PathNode node = nodes[i];
            node.CostFromStart = int.MaxValue;
            node.CostToGoal = 0;
            node.TotalCost = int.MaxValue;
            node.ParentIndex = -1;
            nodes[i] = node;
        }

        PathNode start = nodes[startIndex];
        start.CostFromStart = 0;
        start.CostToGoal = Heuristic(startIndex, endIndex);
        start.TotalCost = start.CostFromStart + start.CostToGoal;
        start.ParentIndex = -1;

        nodes[startIndex] = start;

        openList[openCount++] = startIndex;
        state[startIndex] = STATE_OPEN;
    }

    int Heuristic(int first, int second)
    {
        int firstX = first % width;
        int firstY = first / width;

        int secondX = second % width;
        int secondY = second / width;

        return math.abs(firstX - secondX) + math.abs(firstY - secondY);
    }

    void ExpandNode(int currentIndex, ref int openCount)
    {
        int size = width * height;

        PathNode currentNode = nodes[currentIndex];
        int currentX = currentIndex % width;

        int newCost = currentNode.CostFromStart + 1;

        int up = currentIndex - width;
        if (up >= 0 && state[up] != STATE_CLOSED && walkables[up] == 1)
            ProcessNeighbor(currentIndex, up, newCost, ref openCount);

        int down = currentIndex + width;
        if (down < size && state[down] != STATE_CLOSED && walkables[down] == 1)
            ProcessNeighbor(currentIndex, down, newCost, ref openCount);

        int left = currentIndex - 1;
        if (left >= 0 && state[left] != STATE_CLOSED && (left % width) == currentX - 1 && walkables[left] == 1)
            ProcessNeighbor(currentIndex, left, newCost, ref openCount);

        int right = currentIndex + 1;
        if (right < size && state[right] != STATE_CLOSED && (right % width) == currentX + 1 && walkables[right] == 1)
            ProcessNeighbor(currentIndex, right, newCost, ref openCount);
    }

    void ProcessNeighbor(int currentIndex, int neighborIndex, int newCost, ref int openCount)
    {
        if (state[neighborIndex] == STATE_CLOSED)
            return;

        PathNode neighbor = nodes[neighborIndex];

        if (newCost < neighbor.CostFromStart)
        {
            neighbor.CostFromStart = newCost;
            neighbor.CostToGoal = Heuristic(neighborIndex, endIndex);
            neighbor.TotalCost = neighbor.CostFromStart + neighbor.CostToGoal;
            neighbor.ParentIndex = currentIndex;

            nodes[neighborIndex] = neighbor;

            if (state[neighborIndex] != STATE_OPEN)
            {
                state[neighborIndex] = STATE_OPEN;
                Push(neighborIndex, ref openCount);
            }
        }
    }

    #region Heap
    int Pop(ref int openCount)
    {
        int root = openList[0];
        openList[0] = openList[--openCount];

        HeapifyDown(0, openCount);

        state[root] = STATE_CLOSED;
        return root;
    }

    void Push(int index, ref int openCount)
    {
        openList[openCount] = index;
        HeapifyUp(openCount);
        openCount++;
    }

    void HeapifyUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) / 2;

            if (nodes[openList[parent]].TotalCost <= nodes[openList[i]].TotalCost)
                break;

            Swap(parent, i);
            i = parent;
        }
    }

    void HeapifyDown(int i, int count)
    {
        while (true)
        {
            int left = i * 2 + 1;
            int right = i * 2 + 2;
            int smallest = i;

            if (left < count &&
                nodes[openList[left]].TotalCost < nodes[openList[smallest]].TotalCost)
                smallest = left;

            if (right < count &&
                nodes[openList[right]].TotalCost < nodes[openList[smallest]].TotalCost)
                smallest = right;

            if (smallest == i)
                break;

            Swap(i, smallest);
            i = smallest;
        }
    }

    void Swap(int a, int b)
    {
        int temp = openList[a];
        openList[a] = openList[b];
        openList[b] = temp;
    }
    #endregion
}