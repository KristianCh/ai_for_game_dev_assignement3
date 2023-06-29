using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MangoPlayer : ComputerPlayer
{
    public override void OnGameStarted()
    {
        base.OnGameStarted();

        // Did you know that Mango is a tree?

        pathTarget = CurrentTile;
        behaviourTree = new BehaviourTree();

        ConditionNode CanTakePlayerPoint = new ConditionNode(delegate ()
        {
            return 
            (
                
                GameManager.Instance.GetDistanceToClosestCollectibleOfType(Blackboard.Instance.HumanPlayer.CurrentTile, CollectibleItemType.AddPoint) *
                    Blackboard.Instance.HumanPlayer.MovementSpeed >
                GameManager.Instance.GetDistanceToClosestCollectibleOfType(CurrentTile, CollectibleItemType.AddPoint) * MovementSpeed
            );
        }, behaviourTree);

        ActionNode ClosestAddPointToPlayer = new ActionNode(delegate ()
        {
            return GameManager.Instance.GetClosestCollectibleOfType(Blackboard.Instance.HumanPlayer.CurrentTile, CollectibleItemType.AddPoint);
        }, behaviourTree, CollectibleItemType.AddPoint);

        ConditionNode existsSpeedUp = ConditionNode.ExistsRespawnAll(behaviourTree);
        existsSpeedUp.SetLeft(ActionNode.GetClosestIncreaseMovementSpeed(this, behaviourTree));
        existsSpeedUp.SetRight(ActionNode.GetClosestAny(this, behaviourTree));

        ConditionNode existsRespawnAll = ConditionNode.ExistsRespawnAll(behaviourTree);
        existsRespawnAll.SetLeft(ActionNode.GetClosestRespawnAll(this, behaviourTree));
        existsRespawnAll.SetRight(ActionNode.GetClosestAny(this, behaviourTree));

        SequenceNode sequence = new SequenceNode(behaviourTree);
        sequence.AddNodeToSequence(ClosestAddPointToPlayer);
        sequence.AddNodeToSequence(existsRespawnAll);

        CanTakePlayerPoint.SetLeft(sequence);
        CanTakePlayerPoint.SetRight(existsSpeedUp);

        behaviourTree.Root = CanTakePlayerPoint;
    }

    protected override void EvaluateDecisions(Maze maze, List<AbstractPlayer> players, List<CollectibleItem> spawnedCollectibles, float remainingGameTime)
    {
        bool didFail = behaviourTree.CheckFail();

        if (pathTilesQueue.Count == 0 || behaviourTree.CurrentActionDestination != pathTarget || didFail || MarkSucceeded)
        {
            if (didFail && !(pathTilesQueue.Count == 0 || behaviourTree.CurrentActionDestination != pathTarget) && !MarkSucceeded)
            {
                behaviourTree.CurrentAction.Fail();
            } 
            if (!didFail && behaviourTree.CurrentAction.TargetItemType == CollectibleItemType.None || MarkSucceeded)
            {
                MarkSucceeded = false;
                behaviourTree.CurrentAction.Succeed();
            }
            Vector2Int start = CurrentTile;
            Vector2Int end = behaviourTree.Evaluate();
            List<Vector2Int> path = GetPathFromTo(start, end);
            if (path.Count != CurrentPathLength)
            {
                CurrentPathLength = path.Count;
                pathTilesQueue = new Queue<Vector2Int>(path);
                pathTarget = end;

                for (int i = 0; i < path.Count - 1; i++)
                {
                    Debug.DrawLine(parentMaze.GetWorldPositionForMazeTile(path[i]), parentMaze.GetWorldPositionForMazeTile(path[i + 1]), Color.blue, 1f);
                }
            }
        }
    }
}
