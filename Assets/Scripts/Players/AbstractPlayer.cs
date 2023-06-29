using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class AbstractPlayer : MonoBehaviour
{
    private const float MovementTransitionDistanceToleranceSq = 0.03f * 0.03f;

    [FormerlySerializedAs("spriteRenderer")] 
    [SerializeField]
    protected SpriteRenderer _SpriteRenderer;

    protected BehaviourTree _behaviourTree;
    protected Blackboard _blackboard = new Blackboard();
    
    private float _movementSpeed;
    private float _initMovementSpeed;
    protected Maze _parentMaze;
    private Vector3 _nextWorldMovePos;
    private Vector2Int _transitionEndTile;

    private Sprite _sprite;

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
    public Sprite Sprite
    {
        get
        {
            if(_sprite == null)
            {
                _sprite = _SpriteRenderer.sprite;
            }

            return _sprite;
        }
    }

    public Color SpriteColor => _SpriteRenderer.color;

    public float MovementSpeed
    {
        get => _movementSpeed;
        set => _movementSpeed = Mathf.Clamp(value, 0.0f, MaxMovementSpeed);
    }

    private float MaxMovementSpeed => _initMovementSpeed * GameManager.Instance.MaxSpeedMultiple;

    protected bool MaxMovementSpeedReached => Mathf.Abs(MaxMovementSpeed - MovementSpeed) <= float.Epsilon;

    public event System.Action<AbstractPlayer> PointsUpdated;

    protected virtual void Update()
    {
        if(MovementTransitionFinished())
        {
            StartMovementTransitionToNeighboringTile(GetNextPathTile());
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, _nextWorldMovePos,
                MovementSpeed * Time.deltaTime);
        }
    }

    public virtual void InitializeData(Maze parentMaze, float movementSpeed, Vector2Int spawnTilePos)
    {
        this._parentMaze = parentMaze;
        // The multiplication below ensures that movement speed is considered in tile-units so it stays
        // consistent across different scales of the maze
        _initMovementSpeed = movementSpeed * parentMaze.GetElementsScale().x;
        MovementSpeed = _initMovementSpeed; 

        transform.position = parentMaze.GetWorldPositionForMazeTile(spawnTilePos.x, spawnTilePos.y);
        transform.localScale = parentMaze.GetElementsScale();

        CurrentTile = spawnTilePos;
        _transitionEndTile = CurrentTile;
        _nextWorldMovePos = transform.position;
    }

    public virtual void OnGameStarted() { }

    protected virtual bool MovementTransitionFinished()
    {
        if(Vector2.SqrMagnitude(_nextWorldMovePos - transform.position) <=
            MovementTransitionDistanceToleranceSq)
        {
            CurrentTile = _transitionEndTile;
            transform.position = _nextWorldMovePos;
            return true;
        }

        return false;
    }

    protected virtual void StartMovementTransitionToNeighboringTile(Vector2Int tile)
    {
        Debug.DrawLine(_parentMaze.GetWorldPositionForMazeTile(tile), _parentMaze.GetWorldPositionForMazeTile(CurrentTile), Color.white, 0.1f);
        if(Vector2Int.Distance(tile, CurrentTile) > Mathf.Sqrt(2) + 0.01f)
        {
            Debug.LogError("Cannot move to the tile which is not next to the current one!");
            return;
        }
        else if(!_parentMaze.IsValidTileOfType(tile, MazeTileType.Free))
        {
            Debug.LogError("The agent can walk only on free tiles! Error occured when trying to move to the tile: " + tile);
            return;
        }

        _transitionEndTile = tile;
        _nextWorldMovePos = _parentMaze.GetWorldPositionForMazeTile(tile);
    }

    protected virtual Vector2Int GetNextPathTile()
    {
        return CurrentTile;
    }

    public virtual void OnItemPickup(CollectibleItem item) { }
}
