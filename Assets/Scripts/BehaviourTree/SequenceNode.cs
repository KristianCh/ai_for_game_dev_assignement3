using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceNode : AbstractNode
{
    private List<AbstractNode> ActionSequence = new List<AbstractNode>();
    private int SequencePosition;

    public SequenceNode(BehaviourTree btree)
    {
        BTree = btree;
        NodeType = NodeType.Sequence;
    }

    public override void Succeed()
    {
        if (Parent != null)
        {
            Parent.ChildExit(true);
        }
    }

    public override void Fail()
    {
        SequencePosition = ActionSequence.Count;
        AdvanceInSequence();
        if (Parent != null)
        {
            Parent.ChildExit(false);
        }
    }

    public override void ChildExit(bool outcome)
    {
        if (outcome && !InverterNode || !outcome && InverterNode)
        {
            AdvanceInSequence();
            if (SequencePosition == 0)
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
        ActionSequence.Add(node);
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
        if (ActionSequence.Count > 0)
        {
            return ActionSequence[SequencePosition];
        }
        return null;
    }

    public override void AdvanceInSequence()
    {
        SequencePosition++;
        if (SequencePosition >= ActionSequence.Count)
        {
            SequencePosition = 0;
            if (BTree.CurrentRoot != HeldRoot)
            {
                IsRoot = false;
            }
            BTree.CurrentRoot = HeldRoot;
            HeldRoot = null;
        }
    }
}
