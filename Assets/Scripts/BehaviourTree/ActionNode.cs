using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionNode : AbstractNode
{
    private CollectibleItemType _targetItemType;

    public CollectibleItemType TargetItemType
    {
        get => _targetItemType;
        set => _targetItemType = value;
    }

    public delegate Vector2Int EvalDestDel();
    public EvalDestDel EvalDest = delegate () { return Vector2Int.zero; };

    public ActionNode(EvalDestDel evaluateFunc, BehaviourTree BTree, CollectibleItemType TargetItemType)
    {
        this.BTree = BTree;
        this.EvalDest = evaluateFunc;
        this.TargetItemType = TargetItemType;
        NodeType = NodeType.Action;
    }

    public override Vector2Int GetActionDestination()
    {
        BTree.CurrentAction = this;
        return EvalDest();
    }

    // common nodes
    public static ActionNode GetClosestAddPoint(ComputerPlayer player, BehaviourTree BTree)
    {
        return new ActionNode(delegate ()
        {
            return GameManager.Instance.GetClosestCollectibleOfType(player.CurrentTile, CollectibleItemType.AddPoint);
        }, BTree, CollectibleItemType.AddPoint);
    }

    public static ActionNode GetClosestIncreaseMovementSpeed(ComputerPlayer player, BehaviourTree BTree)
    {
        return new ActionNode(delegate ()
        {
            return GameManager.Instance.GetClosestCollectibleOfType(player.CurrentTile, CollectibleItemType.IncreaseMovementSpeed);
        }, BTree, CollectibleItemType.IncreaseMovementSpeed);
    }

    public static ActionNode GetClosestRespawnAll(ComputerPlayer player, BehaviourTree BTree)
    {
        return new ActionNode(delegate ()
        {
            return GameManager.Instance.GetClosestCollectibleOfType(player.CurrentTile, CollectibleItemType.RespawnAll);
        }, BTree, CollectibleItemType.RespawnAll);
    }

    public static ActionNode GetClosestAny(ComputerPlayer player, BehaviourTree BTree)
    {
        return new ActionNode(delegate ()
        {
            return GameManager.Instance.GetClosestCollectibleOfType(player.CurrentTile, CollectibleItemType.Any);
        }, BTree, CollectibleItemType.Any);
    }
}
