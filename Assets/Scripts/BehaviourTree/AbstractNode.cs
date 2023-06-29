using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Condition,
    Action,
    Sequence
}

public class AbstractNode
{
    private BehaviourTree _bTree;
    private AbstractNode _parent;
    private AbstractNode _heldRoot;
    private NodeType _nodeType;
    private bool _inverterNode = false;
    private bool _isRoot;

    public BehaviourTree BTree
    {
        get => _bTree;
        set => _bTree = value;
    }

    public AbstractNode Parent
    {
        get => _parent;
        set => _parent = value;
    }

    public AbstractNode HeldRoot
    {
        get => _heldRoot;
        set => _heldRoot = value;
    }

    public NodeType NodeType
    {
        get => _nodeType;
        set => _nodeType = value;
    }

    public bool InverterNode
    {
        get => _inverterNode;
        set => _inverterNode = value;
    }

    public bool IsRoot
    {
        get => _isRoot;
        set => _isRoot = value;
    }

    public virtual AbstractNode GetNextNode() { return null; }
    public virtual Vector2Int GetActionDestination() { return Vector2Int.zero; }
    public virtual void AdvanceInSequence() { }

    public virtual void Succeed()
    {
        if (_parent != null)
        {
            _parent.ChildExit(true);
        }
    }

    public virtual void Fail()
    {
        if (_parent != null)
        {
            _parent.ChildExit(false);
        }
    }

    public virtual void ChildExit(bool outcome) 
    {
        if (outcome && !_inverterNode || !outcome && _inverterNode)
        {
            Succeed();
        }
        else
        {
            Fail();
        }
    }
}
