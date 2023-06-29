using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractPlayer : MonoBehaviour
{
    protected const float movementTransitionDistanceToleranceSq = 0.03f * 0.03f;
    public BehaviourTree behaviourTree;
    public Blackboard blackboard = new Blackboard();

    [SerializeField]
    protected SpriteRenderer spriteRenderer;

    public Vector2Int CurrentTile { get; private set; }

    private int _points = 0;
    public int Points
    {
        get => _points;
        set
        {
            _points = value;

            PointsUpdated?.Invoke(this);
        }
    }

    private Sprite _sprite;
    public Sprite Sprite
    {
        get
        {
            if(_sprite == null)
            {
                _sprite = spriteRenderer.sprite;
            }

            return _sprite;
        }
    }

    public Color SpriteColor => spriteRenderer.color;

    public float MovementSpeed
    {
        get => movementSpeed;
        set => movementSpeed = Mathf.Clamp(value, 0.0f, MaxMovementSpeed);
    }

    public float MaxMovementSpeed => initMovementSpeed * GameManager.Instance.MaxSpeedMultiple;

    public bool MaxMovementSpeedReached => Mathf.Abs(MaxMovementSpeed - MovementSpeed) <= float.Epsilon;

    public event System.Action<AbstractPlayer> PointsUpdated;

    protected float movementSpeed;

    protected float initMovementSpeed;

    public Maze parentMaze;

    protected Vector3 nextWorldMovePos;

    protected Vector2Int transitionEndTile;

    protected virtual void Update()
    {
        if(MovementTransitionFinished())
        {
            StartMovementTransitionToNeighboringTile(GetNextPathTile());
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, nextWorldMovePos,
                MovementSpeed * Time.deltaTime);
        }
    }

    public virtual void InitializeData(Maze parentMaze, float movementSpeed, Vector2Int spawnTilePos)
    {
        this.parentMaze = parentMaze;
        // The multiplication below ensures that movement speed is considered in tile-units so it stays
        // consistent across different scales of the maze
        initMovementSpeed = movementSpeed * parentMaze.GetElementsScale().x;
        MovementSpeed = initMovementSpeed; 

        transform.position = parentMaze.GetWorldPositionForMazeTile(spawnTilePos.x, spawnTilePos.y);
        transform.localScale = parentMaze.GetElementsScale();

        CurrentTile = spawnTilePos;
        transitionEndTile = CurrentTile;
        nextWorldMovePos = transform.position;
    }

    public virtual void OnGameStarted() { }

    protected virtual bool MovementTransitionFinished()
    {
        if(Vector2.SqrMagnitude(nextWorldMovePos - transform.position) <=
            movementTransitionDistanceToleranceSq)
        {
            CurrentTile = transitionEndTile;
            transform.position = nextWorldMovePos;
            return true;
        }

        return false;
    }

    protected virtual void StartMovementTransitionToNeighboringTile(Vector2Int tile)
    {
        Debug.DrawLine(parentMaze.GetWorldPositionForMazeTile(tile), parentMaze.GetWorldPositionForMazeTile(CurrentTile), Color.white, 0.1f);
        if(Vector2Int.Distance(tile, CurrentTile) > Mathf.Sqrt(2) + 0.01f)
        {
            Debug.LogError("Cannot move to the tile which is not next to the current one!");
            return;
        }
        else if(!parentMaze.IsValidTileOfType(tile, MazeTileType.Free))
        {
            Debug.LogError("The agent can walk only on free tiles! Error occured when trying to move to the tile: " + tile);
            return;
        }

        transitionEndTile = tile;
        nextWorldMovePos = parentMaze.GetWorldPositionForMazeTile(tile);
    }

    protected virtual Vector2Int GetNextPathTile()
    {
        return CurrentTile;
    }

    public virtual void OnItemPickup(CollectibleItem item) { }
}
