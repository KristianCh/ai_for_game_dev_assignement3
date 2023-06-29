using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionNode : AbstractNode
{
    private AbstractNode _left;
    private AbstractNode _right;
    
    public delegate bool TriggerFuncDel();
    public TriggerFuncDel DoesTrigger = delegate () { return true; };

    public AbstractNode Left
    {
        get => _left;
        set => _left = value;
    }

    public AbstractNode Right
    {
        get => _right;
        set => _right = value;
    }

    public ConditionNode(TriggerFuncDel triggerFunc, BehaviourTree BTree)
    {
        this.BTree = BTree;
        this.DoesTrigger = triggerFunc;
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
        if (DoesTrigger())
        {
            //Debug.Log("Left");
            return Left;
        }
        else
        {
            //Debug.Log("Right");
            return Right;
        }
    }

    // common nodes
    public static ConditionNode ExistsAddPoint(BehaviourTree BTree)
    {
        return new ConditionNode(delegate ()
        {
            return GameManager.Instance.ExistsCollectibleOfType(CollectibleItemType.AddPoint);
        }, BTree);
    }

    public static ConditionNode ExistsIncreaseMovementSpeed(BehaviourTree BTree)
    {
        return new ConditionNode(delegate ()
        {
            return GameManager.Instance.ExistsCollectibleOfType(CollectibleItemType.IncreaseMovementSpeed);
        }, BTree);
    }

    public static ConditionNode ExistsRespawnAll(BehaviourTree BTree)
    {
        return new ConditionNode(delegate ()
        {
            return GameManager.Instance.ExistsCollectibleOfType(CollectibleItemType.RespawnAll);
        }, BTree);
    }
}
