using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceNode : AbstractNode
{
    private readonly List<AbstractNode> _actionSequence = new List<AbstractNode>();
    private int _sequencePosition;

    public SequenceNode(BehaviourTree btree)
    {
        BTree = btree;
        NodeType = NodeType.Sequence;
    }

    public override void Succeed()
    {
        Parent?.ChildExit(true);
    }

    public override void Fail()
    {
        _sequencePosition = _actionSequence.Count;
        AdvanceInSequence();
        Parent?.ChildExit(false);
    }

    public override void ChildExit(bool outcome)
    {
        if (outcome && !InverterNode || !outcome && InverterNode)
        {
            AdvanceInSequence();
            if (_sequencePosition == 0)
            {
                Succeed();
            }
        }
        else
        {
            Fail();
        }
    }

    public void AddNodeToSequence(AbstractNode node)
    {
        _actionSequence.Add(node);
        node.Parent = this;
    }

    public override AbstractNode GetNextNode()
    {
        if (HeldRoot == null)
        {
            HeldRoot = BTree.CurrentRoot;
            IsRoot = true;
            BTree.CurrentRoot = this;
        }
        return _actionSequence.Count > 0 ? _actionSequence[_sequencePosition] : null;
    }

    public override void AdvanceInSequence()
    {
        _sequencePosition++;
        if (_sequencePosition < _actionSequence.Count) 
            return;
        
        _sequencePosition = 0;
        if (BTree.CurrentRoot != HeldRoot)
        {
            IsRoot = false;
        }
        BTree.CurrentRoot = HeldRoot;
        HeldRoot = null;
    }
}
