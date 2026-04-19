using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct AStarJob : IJob
{
    public int width;
    public int height;

    public int startIndex;
    public int endIndex;

    public NativeArray<bool> walkables;
    public NativeArray<PathNode> nodes;

    public NativeArray<byte> state;

    const byte STATE_NONE = 0;
    const byte STATE_OPEN = 1;
    const byte STATE_CLOSED = 2;

    void IJob.Execute()
    {
        PathNode node = nodes[startIndex];
        node.CostFromStart = 12345;
        nodes[startIndex] = node;
    }
}