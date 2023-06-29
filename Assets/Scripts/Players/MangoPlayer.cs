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

        ConditionNode CanTakePlayerPoint = new ConditionNode(delegate ()
        {
            return 
            (
                
                GameManager.Instance.GetDistanceToClosestCollectibleOfType(Blackboard.Instance.HumanPlayer.CurrentTile, CollectibleItemType.AddPoint) *
                    Blackboard.Instance.HumanPlayer.MovementSpeed >
                GameManager.Instance.GetDistanceToClosestCollectibleOfType(CurrentTile, CollectibleItemType.AddPoint) * MovementSpeed
            );
        }, _behaviourTree);

        ActionNode ClosestAddPointToPlayer = new ActionNode(delegate ()
        {
            return GameManager.Instance.GetClosestCollectibleOfType(Blackboard.Instance.HumanPlayer.CurrentTile, CollectibleItemType.AddPoint);
        }, _behaviourTree, CollectibleItemType.AddPoint);

        ConditionNode existsSpeedUp = ConditionNode.ExistsRespawnAll(_behaviourTree);
        existsSpeedUp.SetLeft(ActionNode.GetClosestIncreaseMovementSpeed(this, _behaviourTree));
        existsSpeedUp.SetRight(ActionNode.GetClosestAny(this, _behaviourTree));

        ConditionNode existsRespawnAll = ConditionNode.ExistsRespawnAll(_behaviourTree);
        existsRespawnAll.SetLeft(ActionNode.GetClosestRespawnAll(this, _behaviourTree));
        existsRespawnAll.SetRight(ActionNode.GetClosestAny(this, _behaviourTree));

        SequenceNode sequence = new SequenceNode(_behaviourTree);
        sequence.AddNodeToSequence(ClosestAddPointToPlayer);
        sequence.AddNodeToSequence(existsRespawnAll);

        CanTakePlayerPoint.SetLeft(sequence);
        CanTakePlayerPoint.SetRight(existsSpeedUp);

        _behaviourTree.Root = CanTakePlayerPoint;
    }

    protected override void EvaluateDecisions(Maze maze, List<AbstractPlayer> players, List<CollectibleItem> spawnedCollectibles, float remainingGameTime)
    {
        bool didFail = _behaviourTree.CheckFail();

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
            Vector2Int start = CurrentTile;
            Vector2Int end = _behaviourTree.Evaluate();
            List<Vector2Int> path = GetPathFromTo(start, end);
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
