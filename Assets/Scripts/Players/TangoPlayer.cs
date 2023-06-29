using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TangoPlayer : ComputerPlayer
{
    public override void OnGameStarted()
    {
        base.OnGameStarted();

        // Wikipedia says:
        // Tango is a partner dance and social dance that originated in the 1880s
        // along the Río de la Plata, the natural border between Argentina and Uruguay.
        // ---
        // Is this relevant? Probably not but it is nice to learn something, right?

        PathTarget = CurrentTile;
        _behaviourTree = new BehaviourTree();

        var getCloserAddSpeed = new ConditionNode(() => (
            GameManager.Instance.GetDistanceToClosestCollectibleOfType(CurrentTile, CollectibleItemType.AddPoint) <
            GameManager.Instance.GetDistanceToClosestCollectibleOfType(CurrentTile,
                CollectibleItemType.IncreaseMovementSpeed) ||
            MaxMovementSpeedReached
        ), _behaviourTree);

        getCloserAddSpeed.SetLeft(ActionNode.GetClosestAddPoint(this, _behaviourTree));
        getCloserAddSpeed.SetRight(ActionNode.GetClosestIncreaseMovementSpeed(this, _behaviourTree));

        var selector = new SelectorNode(_behaviourTree);
        selector.AddNodeToSequence(getCloserAddSpeed);
        selector.AddNodeToSequence(ActionNode.GetClosestAny(this, _behaviourTree));

        _behaviourTree.Root = selector;
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
                    Debug.DrawLine(_parentMaze.GetWorldPositionForMazeTile(path[i]), _parentMaze.GetWorldPositionForMazeTile(path[i + 1]), Color.magenta, 1f);
                }
            }
        }
    }
}
