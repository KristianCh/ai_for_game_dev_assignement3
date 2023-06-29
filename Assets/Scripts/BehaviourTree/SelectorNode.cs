using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorNode : AbstractNode
{
    private List<AbstractNode> ActionSequence { get; set; } = new List<AbstractNode>();
    private int SequencePosition { get; set; }

    public SelectorNode(BehaviourTree btree)
    {
        BTree = btree;
        NodeType = NodeType.Sequence;
    }

    public override void Succeed()
    {
        SequencePosition = ActionSequence.Count;
        AdvanceInSequence();
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
            Succeed();
        }
        else
        {
            AdvanceInSequence();
            if (SequencePosition == 0)
            {
                Fail();
            }
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
            if (HeldRoot != null)
            {
                BTree.CurrentRoot = HeldRoot;
            }
            HeldRoot = null;
        }
    }
}
