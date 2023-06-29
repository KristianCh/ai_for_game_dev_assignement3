using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DjangoPlayer : ComputerPlayer
{
    public override void OnGameStarted()
    {
        base.OnGameStarted();

        // This will be the real Django Unchained!

        PathTarget = CurrentTile;
        _behaviourTree = new BehaviourTree();

        ConditionNode CanSpeedUp = new ConditionNode( delegate ()
        {
            return (!MaxMovementSpeedReached && GameManager.Instance.ExistsCollectibleOfType(CollectibleItemType.IncreaseMovementSpeed));
        }, _behaviourTree);

        ConditionNode IsAddPointWayCloserThanSpeedUp = new ConditionNode( delegate() 
        {
            return 
            (
                GameManager.Instance.ExistsCollectibleOfType(CollectibleItemType.AddPoint) &&
                GameManager.Instance.GetDistanceToClosestCollectibleOfType(CurrentTile, CollectibleItemType.AddPoint) * 10.0f <
                GameManager.Instance.GetDistanceToClosestCollectibleOfType(CurrentTile, CollectibleItemType.IncreaseMovementSpeed)
            );
        }, _behaviourTree);

        ConditionNode existsAddPoint = ConditionNode.ExistsAddPoint(_behaviourTree);
        ConditionNode existsRespawnAll = ConditionNode.ExistsRespawnAll(_behaviourTree);

        existsRespawnAll.SetLeft(ActionNode.GetClosestRespawnAll(this, _behaviourTree));
        existsRespawnAll.SetRight(new ActionNode(delegate ()
        {
            return _parentMaze.GetMazeTileForWorldPosition(Blackboard.Instance.HumanPlayer.transform.position);
        }, _behaviourTree, CollectibleItemType.None));

        existsAddPoint.SetLeft(ActionNode.GetClosestAddPoint(this, _behaviourTree));
        existsAddPoint.SetRight(existsRespawnAll);

        IsAddPointWayCloserThanSpeedUp.SetLeft(ActionNode.GetClosestAddPoint(this, _behaviourTree));
        IsAddPointWayCloserThanSpeedUp.SetRight(ActionNode.GetClosestIncreaseMovementSpeed(this, _behaviourTree));

        CanSpeedUp.SetLeft(IsAddPointWayCloserThanSpeedUp);
        CanSpeedUp.SetRight(existsAddPoint);


        _behaviourTree.Root = CanSpeedUp;
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

            if (end == start)
            {
                _behaviourTree.CurrentAction.Fail();
                return;
            }

            List<Vector2Int> path = GetPathFromTo(start, end);
            if (path.Count != CurrentPathLength)
            {
                CurrentPathLength = path.Count;
                pathTilesQueue = new Queue<Vector2Int>(path);
                PathTarget = end;

                for (int i = 0; i < path.Count - 1; i++)
                {
                    Debug.DrawLine(_parentMaze.GetWorldPositionForMazeTile(path[i]), _parentMaze.GetWorldPositionForMazeTile(path[i + 1]), Color.red, 1f);
                }
            }
        }
    }
}
