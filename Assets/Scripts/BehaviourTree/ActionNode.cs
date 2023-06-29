using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionNode : AbstractNode
{
    public CollectibleItemType TargetItemType { get; }
    public delegate Vector2Int EvalDestDel();

    private readonly EvalDestDel _evalDest;

    public ActionNode(EvalDestDel evaluateFunc, BehaviourTree bTree, CollectibleItemType targetItemType)
    {
        BTree = bTree;
        _evalDest = evaluateFunc;
        TargetItemType = targetItemType;
        NodeType = NodeType.Action;
    }

    public override Vector2Int GetActionDestination()
    {
        BTree.CurrentAction = this;
        return _evalDest();
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
