using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ComputerPlayer : AbstractPlayer
{
    protected Queue<Vector2Int> pathTilesQueue = new Queue<Vector2Int>();

    public Vector2Int PathTarget { get; set; }
    public float CurrentPathLength { get; set; } = 0;
    public bool MarkSucceeded { get; set; } = false;

    protected override void Update()
    {
        base.Update();

        EvaluateDecisions(
            _parentMaze,
            GameManager.Instance.Players,
            GameManager.Instance.SpawnedCollectibles,
            GameManager.Instance.TimeRemaining
            );
    }

    protected override Vector2Int GetNextPathTile()
    {
        if (pathTilesQueue.Count > 0)
        {
            return pathTilesQueue.Dequeue();
        }

        return base.GetNextPathTile();
    }

    public List<Vector2Int> GetPathFromTo(Vector2Int srcTile, Vector2Int destTile)
    {
        // TODO: Implement this method to make it retrieve the path from the source tile to destination tile
        //       so you can use it in the "derived bots"
        return GameManager.Instance.GetPathFromTo(srcTile, destTile);
    }

    // TODO: To complete this assignment, you will need to write 
    //       a fair amount of code. It is recommended to create
    //       custom classes/functions to decouple the computations.

    //       The method EvaluateDecisions should be the place where the final decision
    //       should be computed. The bot automatically follows the path which is
    //       described in the "pathTilesQueue" variable. All neighboring values inside
    //       this queue must be 4-neighbors, i.e., the bot can walk only
    //       up/down/left/right with a step of one.
    //      
    //       You do not have to use all arguments of this function 
    //       and you can add even more parameters if you would like to do so.
    //       Tiles of the maze can be accessed via maze.MazeTiles.
    //       Human player is the first player in the "players" list.
    //       CollectibleItem class contains TileLocation property providing information about collectible's position
    //       and Type property retrieving its type.
    //       Good luck. May your bots remain unbeaten by a human player. :)
    protected abstract void EvaluateDecisions(Maze maze, List<AbstractPlayer> players,
        List<CollectibleItem> spawnedCollectibles, float remainingGameTime);

    public override void OnItemPickup(CollectibleItem item) 
    {
        if (CurrentTile == _behaviourTree.CurrentActionDestination)
        {
            if (_behaviourTree.CurrentAction.TargetItemType == CollectibleItemType.Any ||
                _behaviourTree.CurrentAction.TargetItemType == item.Type)
            {
                MarkSucceeded = true;
            }
        }
    }
}
