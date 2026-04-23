using Unity.Collections;
using Unity.Jobs;

class PathRequest
{
    public NativeArray<byte> NodeState;
    public NativeArray<int> OpenList;
    public NativeArray<int> Result;

    public NativeArray<PathNode> Nodes;

    public JobHandle Handle;

    public int StartIndex;
    public int EndIndex;
}