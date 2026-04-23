using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// A* Pathfinding 실행 및 Job System 기반 비동기 경로 탐색 처리
/// </summary>
public class Pathfinding : MonoBehaviour
{
    [SerializeField] int _mapSeed;

    [Header("Map Size")]
    [SerializeField] int _width = 50;
    [SerializeField] int _height = 50;

    GridSystem _grid;
    Pathfinder _pathfinder;

    List<int> _finalPath = new List<int>();
    List<int> _rawPathBuffer = new List<int>();
    List<int> _smoothedPathBuffer = new List<int>();

    NativeArray<byte> _nodeStateArray;
    NativeArray<int> _openListArray;
    NativeArray<int> _jobResultArray;

    JobHandle _pathfindingJobHandle;
    bool _isJobRunning;
    int _pendingEndIndex;

    List<PathRequest> _requests = new List<PathRequest>();

    const int MaxPathBuildSafety = 100000;

    public GridSystem GetGrid() => _grid;

    void Awake()
    {
        Debug.Log($"Map Seed: {_mapSeed}");
        Random.InitState(_mapSeed);

        _grid = new GridSystem();
        _grid.CreateGrid(_width, _height);

        _pathfinder = new Pathfinder(_grid.Walkables, _grid.Nodes, _grid.Width, _grid.Height);
        MapBuilder.CreatePrimMaze(_grid, _width, _height);

        int totalNodeCount = _grid.Width * _grid.Height;

        _nodeStateArray = new NativeArray<byte>(totalNodeCount, Allocator.Persistent);
        _openListArray = new NativeArray<int>(totalNodeCount, Allocator.Persistent);
        _jobResultArray = new NativeArray<int>(1, Allocator.Persistent);
    }

    void OnDestroy()
    {
        if (_grid != null)
            _grid.Dispose();

        if (_pathfinder != null)
            _pathfinder.Dispose();

        if (_nodeStateArray.IsCreated)
            _nodeStateArray.Dispose();

        if (_openListArray.IsCreated)
            _openListArray.Dispose();

        if (_jobResultArray.IsCreated)
            _jobResultArray.Dispose();
    }

    // Job 시작 (비동기)
    public void StartPathfindingJob(int startIndex, int endIndex)
    {
        _pendingEndIndex = endIndex;

        for (int i = 0; i < _nodeStateArray.Length; i++)
            _nodeStateArray[i] = 0;

        for (int i = 0; i < _openListArray.Length; i++)
            _openListArray[i] = 0;

        _jobResultArray[0] = 0;

        AStarJob job = new AStarJob
        {
            width = _grid.Width,
            height = _grid.Height,
            startIndex = startIndex,
            endIndex = endIndex,
            walkables = _grid.Walkables,
            nodes = _grid.Nodes,
            state = _nodeStateArray,
            openList = _openListArray,
            result = _jobResultArray
        };

        _pathfindingJobHandle = job.Schedule();
        _isJobRunning = true;
    }

    // Job 완료 체크
    public bool TryCompletePathfinding(out List<int> resultPath)
    {
        resultPath = null;

        if (!_isJobRunning)
            return false;

        if (!_pathfindingJobHandle.IsCompleted)
            return false;

        _pathfindingJobHandle.Complete();
        _isJobRunning = false;

        if (_jobResultArray[0] != 1)
            return false;

        if (!TryBuildRawPath(_pendingEndIndex, _rawPathBuffer))
            return false;

        _finalPath = SmoothPath(_rawPathBuffer);
        resultPath = _finalPath;

        return true;
    }

    // 병렬 Job 시작
    public void StartPathfindingJobMulti(int startIndex, int endIndex)
    {
        int totalNodeCount = _grid.Width * _grid.Height;

        PathRequest request = new PathRequest
        {
            NodeState = new NativeArray<byte>(totalNodeCount, Allocator.TempJob),
            OpenList = new NativeArray<int>(totalNodeCount, Allocator.TempJob),
            Result = new NativeArray<int>(1, Allocator.TempJob),
            Nodes = new NativeArray<PathNode>(_grid.Nodes, Allocator.TempJob),
            StartIndex = startIndex,
            EndIndex = endIndex

        };

        AStarJob job = new AStarJob
        {
            width = _grid.Width,
            height = _grid.Height,
            startIndex = startIndex,
            endIndex = endIndex,
            walkables = _grid.Walkables,
            nodes = request.Nodes,
            state = request.NodeState,
            openList = request.OpenList,
            result = request.Result
        };

        request.Handle = job.Schedule();

        _requests.Add(request);
    }

    // 병렬 Job 완료 처리
    public void UpdatePathRequests()
    {
        for (int i = _requests.Count - 1; i >= 0; i--)
        {
            PathRequest request = _requests[i];

            if (!request.Handle.IsCompleted)
                continue;

            request.Handle.Complete();

            if (request.Result[0] == 1)
            {
                if (TryBuildRawPath(request.EndIndex, _rawPathBuffer))
                {
                    _finalPath = SmoothPath(_rawPathBuffer);
                    Debug.Log($"[Multi] Path 완료: {_finalPath.Count}");
                }
            }

            request.NodeState.Dispose();
            request.OpenList.Dispose();
            request.Result.Dispose();
            request.Nodes.Dispose();

            _requests.RemoveAt(i);
        }
    }

    public List<int> RunRawPath(int startIndex, int endIndex)
    {
        if (startIndex < 0 || endIndex < 0)
            return null;

        if (!_grid.Walkables[startIndex] || !_grid.Walkables[endIndex])
            return null;

        _pathfinder.BeginSearch(startIndex, endIndex);

        int maxIterationCount = _grid.Width * _grid.Height;

        for (int iteration = 0; iteration < maxIterationCount; iteration++)
        {
            bool reachedGoal = _pathfinder.Step(out _);

            if (reachedGoal)
            {
                if (!TryBuildRawPath(endIndex, _rawPathBuffer))
                    return null;

                return _rawPathBuffer;
            }

            if (_pathfinder.IsEmpty())
                return null;
        }

        return null;
    }

    public List<int> RunAndGetPath(int startIndex, int endIndex)
    {
        if (startIndex < 0 || endIndex < 0)
            return null;

        if (!_grid.Walkables[startIndex] || !_grid.Walkables[endIndex])
            return null;

        _pathfinder.BeginSearch(startIndex, endIndex);

        int maxIteration = _grid.Width * _grid.Height;

        for (int i = 0; i < maxIteration; i++)
        {
            bool reached = _pathfinder.Step(out _);

            if (reached)
            {
                if (!TryBuildRawPath(endIndex, _rawPathBuffer))
                    return null;

                _finalPath = SmoothPath(_rawPathBuffer);
                return _finalPath;
            }

            if (_pathfinder.IsEmpty())
                return null;
        }

        return null;
    }

    public List<int> RunJobAndBuildPath(int startIndex, int endIndex)
    {
        int totalNodeCount = _grid.Width * _grid.Height;

        NativeArray<byte> nodeState = new NativeArray<byte>(totalNodeCount, Allocator.TempJob);
        NativeArray<int> openList = new NativeArray<int>(totalNodeCount, Allocator.TempJob);
        NativeArray<int> result = new NativeArray<int>(1, Allocator.TempJob);

        AStarJob job = new AStarJob
        {
            width = _grid.Width,
            height = _grid.Height,
            startIndex = startIndex,
            endIndex = endIndex,
            walkables = _grid.Walkables,
            nodes = _grid.Nodes,
            state = nodeState,
            openList = openList,
            result = result
        };

        JobHandle handle = job.Schedule();
        handle.Complete();

        if (result[0] != 1)
        {
            nodeState.Dispose();
            openList.Dispose();
            result.Dispose();
            return null;
        }

        if (!TryBuildRawPath(endIndex, _rawPathBuffer))
        {
            nodeState.Dispose();
            openList.Dispose();
            result.Dispose();
            return null;
        }

        List<int> finalPath = SmoothPath(_rawPathBuffer);

        nodeState.Dispose();
        openList.Dispose();
        result.Dispose();

        return finalPath;
    }

    public int GetVisitedNodeCount()
    {
        return _pathfinder.VisitedNodeCount;
    }

    bool TryBuildRawPath(int endIndex, List<int> outputPath)
    {
        outputPath.Clear();

        int currentIndex = endIndex;
        int safetyCounter = 0;

        while (currentIndex != -1)
        {
            safetyCounter++;

            if (safetyCounter > MaxPathBuildSafety)
            {
                Debug.LogError("Parent cycle 감지");
                outputPath.Clear();
                return false;
            }

            outputPath.Add(currentIndex);
            currentIndex = _grid.Nodes[currentIndex].ParentIndex;
        }

        outputPath.Reverse();
        return true;
    }

    List<int> SmoothPath(List<int> originalPath)
    {
        if (originalPath == null || originalPath.Count < 3)
            return originalPath;

        _smoothedPathBuffer.Clear();

        int gridWidth = _grid.Width;

        _smoothedPathBuffer.Add(originalPath[0]);

        for (int index = 1; index < originalPath.Count - 1; index++)
        {
            int previousIndex = originalPath[index - 1];
            int currentIndex = originalPath[index];
            int nextIndex = originalPath[index + 1];

            Vector2Int previousDirection = GetDirection(previousIndex, currentIndex, gridWidth);
            Vector2Int nextDirection = GetDirection(currentIndex, nextIndex, gridWidth);

            if (previousDirection != nextDirection)
                _smoothedPathBuffer.Add(currentIndex);
        }

        _smoothedPathBuffer.Add(originalPath[originalPath.Count - 1]);

        return _smoothedPathBuffer;
    }

    Vector2Int GetDirection(int fromIndex, int toIndex, int gridWidth)
    {
        int fromX = fromIndex % gridWidth;
        int fromY = fromIndex / gridWidth;

        int toX = toIndex % gridWidth;
        int toY = toIndex / gridWidth;

        return new Vector2Int(toX - fromX, toY - fromY);
    }

    #region Gizmos
    void OnDrawGizmos()
    {
        if (_grid == null)
            return;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                int nodeIndex = y * _width + x;
                bool isWalkable = _grid.Walkables[nodeIndex];

                Vector3 worldPosition = new Vector3(x, 0, y);

                Gizmos.color = isWalkable ? Color.white : Color.red;
                Gizmos.DrawWireCube(worldPosition, Vector3.one);
            }
        }

        if (_finalPath == null)
            return;

        Gizmos.color = Color.green;

        for (int pathIndex = 0; pathIndex < _finalPath.Count - 1; pathIndex++)
        {
            int startIndex = _finalPath[pathIndex];
            int endIndex = _finalPath[pathIndex + 1];

            Vector3 startPosition = new Vector3(startIndex % _width, 0, startIndex / _width);
            Vector3 endPosition = new Vector3(endIndex % _width, 0, endIndex / _width);

            Gizmos.DrawLine(startPosition, endPosition);
        }
    }
    #endregion
}