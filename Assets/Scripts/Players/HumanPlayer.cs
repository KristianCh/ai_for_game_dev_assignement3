using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : AbstractPlayer
{
    [SerializeField]
    private KeyCode upKey = KeyCode.W;

    [SerializeField]
    private KeyCode downKey = KeyCode.S;

    [SerializeField]
    private KeyCode leftKey = KeyCode.A;

    [SerializeField]
    private KeyCode rightKey = KeyCode.D;

    private Vector2Int _nextTileDestination;

    public override void OnGameStarted()
    {
        base.OnGameStarted();
        _nextTileDestination = CurrentTile;
    }

    protected override void Update()
    {
        if (MovementTransitionFinished())
        {
            ProcessKeyboardInput();
        }

        base.Update();       
    }

    protected override Vector2Int GetNextPathTile()
    {
        return _nextTileDestination;
    }

    private void ProcessKeyboardInput()
    {
        var desiredTile = GetDesiredTileDestination();

        if (_parentMaze.IsValidTileOfType(desiredTile, MazeTileType.Free))
        {
            _nextTileDestination = desiredTile;
        }
    }

    private Vector2Int GetDesiredTileDestination()
    {
        var currTile = CurrentTile;
        var desiredTile = _nextTileDestination;

        if (Input.GetKey(upKey))
        {
            desiredTile = new Vector2Int(currTile.x, currTile.y - 1);
        }
        else if (Input.GetKey(downKey))
        {
            desiredTile = new Vector2Int(currTile.x, currTile.y + 1);
        }
        else if (Input.GetKey(leftKey))
        {
            desiredTile = new Vector2Int(currTile.x - 1, currTile.y);
        }
        else if (Input.GetKey(rightKey))
        {
            desiredTile = new Vector2Int(currTile.x + 1, currTile.y);
        }

        return desiredTile;
    }
}
