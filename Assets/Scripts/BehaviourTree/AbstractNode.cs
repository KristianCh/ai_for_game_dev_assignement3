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
    protected BehaviourTree BTree { get; set; }
    protected AbstractNode HeldRoot { get; set; }
    protected bool InverterNode { get; set; } = false;
    public AbstractNode Parent { get; set; }
    public NodeType NodeType { get; set; }
    public bool IsRoot { get; set; }

    public virtual AbstractNode GetNextNode() { return null; }
    public virtual Vector2Int GetActionDestination() { return Vector2Int.zero; }
    public virtual void AdvanceInSequence() { }

    public virtual void Succeed()
    {
        if (Parent != null)
        {
            Parent.ChildExit(true);
        }
    }

    public virtual void Fail()
    {
        if (Parent != null)
        {
            Parent.ChildExit(false);
        }
    }

    public virtual void ChildExit(bool outcome) 
    {
        if (outcome && !InverterNode || !outcome && InverterNode)
        {
            Succeed();
        }
        else
        {
            Fail();
        }
    }
}
