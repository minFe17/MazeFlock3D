/// <summary>
/// Pathfinding 알고리즘에서 사용하는 노드 데이터 구조
/// </summary>
public struct PathNode
{
    public int CostFromStart;
    public int CostToGoal;
    public int ParentIndex;

    public int TotalCost => CostFromStart + CostToGoal;

    public void Init()
    {
        CostFromStart = int.MaxValue;
        CostToGoal = 0;
        ParentIndex = -1;
    }
}