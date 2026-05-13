using System.Collections.Generic;
using UnityEngine;

public class AgentMover : MonoBehaviour
{
    List<int> _path;

    int _currentPathIndex;
    int _gridWidth;

    float _moveSpeed = 5f;

    void Update()
    {
        MoveAlongPath();
    }

    public void SetPath(List<int> path, int gridWidth)
    {
        _path = path;
        _gridWidth = gridWidth;
        _currentPathIndex = 0;
    }

    void MoveAlongPath()
    {
        if (_path == null)
            return;

        if (_currentPathIndex >= _path.Count)
            return;

        int targetIndex = _path[_currentPathIndex];
        Vector3 targetPosition = IndexToWorld(targetIndex);

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, _moveSpeed * Time.deltaTime);
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance < 0.01f)
            _currentPathIndex++;
    }

    Vector3 IndexToWorld(int index)
    {
        int x = index % _gridWidth;
        int y = index / _gridWidth;

        return new Vector3(x, 0, y);
    }
}