using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MangoPlayer : ComputerPlayer
{
    public override void OnGameStarted()
    {
        base.OnGameStarted();

        // Did you know that Mango is a tree?

        PathTarget = CurrentTile;
        _behaviourTree = new BehaviourTree();

        var canTakePlayerPoint = new ConditionNode(() => (
            GameManager.Instance.GetDistanceToClosestCollectibleOfType(Blackboard.Instance.HumanPlayer.CurrentTile,
                CollectibleItemType.AddPoint) *
            Blackboard.Instance.HumanPlayer.MovementSpeed >
            GameManager.Instance.GetDistanceToClosestCollectibleOfType(CurrentTile, CollectibleItemType.AddPoint) *
            MovementSpeed
        ), _behaviourTree);

        var closestAddPointToPlayer = new ActionNode(
            () => GameManager.Instance.GetClosestCollectibleOfType(Blackboard.Instance.HumanPlayer.CurrentTile,
                CollectibleItemType.AddPoint), _behaviourTree, CollectibleItemType.AddPoint);

        var existsSpeedUp = ConditionNode.ExistsRespawnAll(_behaviourTree);
        existsSpeedUp.SetLeft(ActionNode.GetClosestIncreaseMovementSpeed(this, _behaviourTree));
        existsSpeedUp.SetRight(ActionNode.GetClosestAny(this, _behaviourTree));

        var existsRespawnAll = ConditionNode.ExistsRespawnAll(_behaviourTree);
        existsRespawnAll.SetLeft(ActionNode.GetClosestRespawnAll(this, _behaviourTree));
        existsRespawnAll.SetRight(ActionNode.GetClosestAny(this, _behaviourTree));

        var sequence = new SequenceNode(_behaviourTree);
        sequence.AddNodeToSequence(closestAddPointToPlayer);
        sequence.AddNodeToSequence(existsRespawnAll);

        canTakePlayerPoint.SetLeft(sequence);
        canTakePlayerPoint.SetRight(existsSpeedUp);

        _behaviourTree.Root = canTakePlayerPoint;
    }

    protected override void EvaluateDecisions(Maze maze, List<AbstractPlayer> players, List<CollectibleItem> spawnedCollectibles, float remainingGameTime)
    {
        var didFail = _behaviourTree.CheckFail();

        if (pathTilesQueue.Count == 0 || _behaviourTree.CurrentActionDestination != PathTarget || didFail || MarkSucceeded)
        {
            if (didFail && !(pathTilesQueue.Count == 0 || _behaviourTree.CurrentActionDestination != PathTarget) && !MarkSucceeded)
            {
                _behaviourTree.CurrentAction.Fail();
            } 
            if (!didFail && _behaviourTree.CurrentAction.TargetItemType == CollectibleItemType.None || MarkSucceeded)
            {
                MarkSucceeded = false;
                _behaviourTree.CurrentAction.Succeed();
            }
            var start = CurrentTile;
            var end = _behaviourTree.Evaluate();
            var path = GetPathFromTo(start, end);
            if (path.Count != CurrentPathLength)
            {
                CurrentPathLength = path.Count;
                pathTilesQueue = new Queue<Vector2Int>(path);
                PathTarget = end;

                for (int i = 0; i < path.Count - 1; i++)
                {
                    Debug.DrawLine(_parentMaze.GetWorldPositionForMazeTile(path[i]), _parentMaze.GetWorldPositionForMazeTile(path[i + 1]), Color.blue, 1f);
                }
            }
        }
    }
}
