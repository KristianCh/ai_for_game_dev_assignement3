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
    public static ActionNode GetClosestAddPoint(ComputerPlayer player, BehaviourTree bTree)
    {
        return new ActionNode(
            () => GameManager.Instance.GetClosestCollectibleOfType(player.CurrentTile, CollectibleItemType.AddPoint), bTree, CollectibleItemType.AddPoint);
    }

    public static ActionNode GetClosestIncreaseMovementSpeed(ComputerPlayer player, BehaviourTree bTree)
    {
        return new ActionNode(
            () => GameManager.Instance.GetClosestCollectibleOfType(player.CurrentTile,
                CollectibleItemType.IncreaseMovementSpeed), bTree, CollectibleItemType.IncreaseMovementSpeed);
    }

    public static ActionNode GetClosestRespawnAll(ComputerPlayer player, BehaviourTree bTree)
    {
        return new ActionNode(
            () => GameManager.Instance.GetClosestCollectibleOfType(player.CurrentTile, CollectibleItemType.RespawnAll), bTree, CollectibleItemType.RespawnAll);
    }

    public static ActionNode GetClosestAny(ComputerPlayer player, BehaviourTree bTree)
    {
        return new ActionNode(
            () => GameManager.Instance.GetClosestCollectibleOfType(player.CurrentTile, CollectibleItemType.Any), bTree, CollectibleItemType.Any);
    }
}
