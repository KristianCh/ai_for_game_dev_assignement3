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

        pathTarget = CurrentTile;
        behaviourTree = new BehaviourTree();

        ConditionNode GetCloserAddSpeed = new ConditionNode(delegate ()
        {
            return
            (
                GameManager.Instance.GetDistanceToClosestCollectibleOfType(CurrentTile, CollectibleItemType.AddPoint) <
                GameManager.Instance.GetDistanceToClosestCollectibleOfType(CurrentTile, CollectibleItemType.IncreaseMovementSpeed) ||
                MaxMovementSpeedReached
            );
        }, behaviourTree);

        GetCloserAddSpeed.SetLeft(ActionNode.GetClosestAddPoint(this, behaviourTree));
        GetCloserAddSpeed.SetRight(ActionNode.GetClosestIncreaseMovementSpeed(this, behaviourTree));

        SelectorNode selector = new SelectorNode(behaviourTree);
        selector.AddNodeToSequence(GetCloserAddSpeed);
        selector.AddNodeToSequence(ActionNode.GetClosestAny(this, behaviourTree));

        behaviourTree.Root = selector;
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
                    Debug.DrawLine(parentMaze.GetWorldPositionForMazeTile(path[i]), parentMaze.GetWorldPositionForMazeTile(path[i + 1]), Color.magenta, 1f);
                }
            }
        }
    }
}
