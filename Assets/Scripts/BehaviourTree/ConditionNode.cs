using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionNode : AbstractNode
{
    public delegate bool TriggerFuncDel();

    private readonly TriggerFuncDel _doesTrigger;

    private AbstractNode Left { get; set; }
    private AbstractNode Right { get; set; }

    public ConditionNode(TriggerFuncDel triggerFunc, BehaviourTree bTree)
    {
        BTree = bTree;
        _doesTrigger = triggerFunc;
        NodeType = NodeType.Condition;
    }

    public void SetLeft(AbstractNode node)
    {
        Left = node;
        Left.Parent = this;
    }

    public void SetRight(AbstractNode node)
    {
        Right = node;
        Right.Parent = this;
    }

    public override AbstractNode GetNextNode()
    {
        return _doesTrigger() ?
            //Debug.Log("Left");
            Left :
            //Debug.Log("Right");
            Right;
    }

    // common nodes
    public static ConditionNode ExistsAddPoint(BehaviourTree bTree)
    {
        return new ConditionNode(() => GameManager.Instance.ExistsCollectibleOfType(CollectibleItemType.AddPoint), bTree);
    }

    public static ConditionNode ExistsIncreaseMovementSpeed(BehaviourTree bTree)
    {
        return new ConditionNode(
            () => GameManager.Instance.ExistsCollectibleOfType(CollectibleItemType.IncreaseMovementSpeed), bTree);
    }

    public static ConditionNode ExistsRespawnAll(BehaviourTree bTree)
    {
        return new ConditionNode(() => GameManager.Instance.ExistsCollectibleOfType(CollectibleItemType.RespawnAll), bTree);
    }
}
