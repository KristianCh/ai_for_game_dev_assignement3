using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DjangoPlayer : ComputerPlayer
{
    public override void OnGameStarted()
    {
        base.OnGameStarted();

        // This will be the real Django Unchained!

        pathTarget = CurrentTile;
        behaviourTree = new BehaviourTree();

        ConditionNode CanSpeedUp = new ConditionNode( delegate ()
        {
            return (!MaxMovementSpeedReached && GameManager.Instance.ExistsCollectibleOfType(CollectibleItemType.IncreaseMovementSpeed));
        }, behaviourTree);

        ConditionNode IsAddPointWayCloserThanSpeedUp = new ConditionNode( delegate() 
        {
            return 
            (
                GameManager.Instance.ExistsCollectibleOfType(CollectibleItemType.AddPoint) &&
                GameManager.Instance.GetDistanceToClosestCollectibleOfType(CurrentTile, CollectibleItemType.AddPoint) * 10.0f <
                GameManager.Instance.GetDistanceToClosestCollectibleOfType(CurrentTile, CollectibleItemType.IncreaseMovementSpeed)
            );
        }, behaviourTree);

        ConditionNode existsAddPoint = ConditionNode.ExistsAddPoint(behaviourTree);
        ConditionNode existsRespawnAll = ConditionNode.ExistsRespawnAll(behaviourTree);

        existsRespawnAll.SetLeft(ActionNode.GetClosestRespawnAll(this, behaviourTree));
        existsRespawnAll.SetRight(new ActionNode(delegate ()
        {
            return parentMaze.GetMazeTileForWorldPosition(Blackboard.Instance.HumanPlayer.transform.position);
        }, behaviourTree, CollectibleItemType.None));

        existsAddPoint.SetLeft(ActionNode.GetClosestAddPoint(this, behaviourTree));
        existsAddPoint.SetRight(existsRespawnAll);

        IsAddPointWayCloserThanSpeedUp.SetLeft(ActionNode.GetClosestAddPoint(this, behaviourTree));
        IsAddPointWayCloserThanSpeedUp.SetRight(ActionNode.GetClosestIncreaseMovementSpeed(this, behaviourTree));

        CanSpeedUp.SetLeft(IsAddPointWayCloserThanSpeedUp);
        CanSpeedUp.SetRight(existsAddPoint);


        behaviourTree.Root = CanSpeedUp;
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

            if (end == start)
            {
                behaviourTree.CurrentAction.Fail();
                return;
            }

            List<Vector2Int> path = GetPathFromTo(start, end);
            if (path.Count != CurrentPathLength)
            {
                CurrentPathLength = path.Count;
                pathTilesQueue = new Queue<Vector2Int>(path);
                pathTarget = end;

                for (int i = 0; i < path.Count - 1; i++)
                {
                    Debug.DrawLine(parentMaze.GetWorldPositionForMazeTile(path[i]), parentMaze.GetWorldPositionForMazeTile(path[i + 1]), Color.red, 1f);
                }
            }
        }
    }
}
