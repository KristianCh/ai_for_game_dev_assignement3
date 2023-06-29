using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree
{
    private AbstractNode _root;
    private AbstractNode _currentRoot;
    private ActionNode _currentAction;
    private Vector2Int _currentActionDestination;

    private bool _wasRootSet = false;

    public AbstractNode Root
    {
        get => _root;
        set => _root = value;
    }

    public AbstractNode CurrentRoot
    {
        get => _currentRoot;
        set => _currentRoot = value;
    }

    public ActionNode CurrentAction
    {
        get => _currentAction;
        set => _currentAction = value;
    }

    public Vector2Int CurrentActionDestination
    {
        get => _currentActionDestination;
        set => _currentActionDestination = value;
    }

    public Vector2Int Evaluate()
    {
        if (!_wasRootSet)
        {
            _wasRootSet = true;
            _currentRoot = _root;
        }

        AbstractNode currentNode = _currentRoot;
        
        while (currentNode.NodeType != NodeType.Action)
        {
            currentNode = currentNode.GetNextNode();
        }
        _currentActionDestination = currentNode.GetActionDestination();
        return _currentActionDestination;
    }

    public bool CheckFail()
    {
        if (_currentAction == null) return true;
        switch (_currentAction.TargetItemType)
        {
            case CollectibleItemType.None:
            case CollectibleItemType.Any when Blackboard.Instance.CollectiblesAtLocations[_currentActionDestination.x, _currentActionDestination.y] != null:
                return false;
        }

        return Blackboard.Instance.CollectiblesAtLocations[_currentActionDestination.x, _currentActionDestination.y] == null ||
               _currentAction.TargetItemType != Blackboard.Instance.CollectiblesAtLocations[_currentActionDestination.x, _currentActionDestination.y].Type;
    }
}
