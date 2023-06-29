using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree
{
    private Vector2Int _currentActionDestination;
    private bool _wasRootSet = false;
    
    public AbstractNode Root { get; set; }
    public AbstractNode CurrentRoot { get; set; }
    public ActionNode CurrentAction { get; set; }
    
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
            CurrentRoot = Root;
        }

        AbstractNode currentNode = CurrentRoot;
        
        while (currentNode.NodeType != NodeType.Action)
        {
            currentNode = currentNode.GetNextNode();
        }
        _currentActionDestination = currentNode.GetActionDestination();
        return _currentActionDestination;
    }

    public bool CheckFail()
    {
        if (CurrentAction == null) return true;
        switch (CurrentAction.TargetItemType)
        {
            case CollectibleItemType.None:
            case CollectibleItemType.Any when Blackboard.Instance.CollectiblesAtLocations[_currentActionDestination.x, _currentActionDestination.y] != null:
                return false;
        }

        return Blackboard.Instance.CollectiblesAtLocations[_currentActionDestination.x, _currentActionDestination.y] == null ||
               CurrentAction.TargetItemType != Blackboard.Instance.CollectiblesAtLocations[_currentActionDestination.x, _currentActionDestination.y].Type;
    }
}
