using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectibleItemType : byte
{
    AddPoint,
    RespawnAll,
    IncreaseMovementSpeed,
    Any,
    None
}

public class CollectibleItem : MonoBehaviour
{
    [SerializeField]
    private CollectibleItemType type;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private AudioClip pickupSound;

    public event System.Action<CollectibleItem> Destroyed;

    public Vector2Int TileLocation { get; private set; }

    public CollectibleItemType Type => type;

    public float InitLifetime { get; private set; }

    public float RemainingLifetime { get; private set; }

    public bool MarkedForDestroy { get; private set; }

    private CollectiblesManager manager;

    private void Update()
    {
        CheckLifetime();
    }

    private void OnDestroy()
    {
        Destroyed?.Invoke(this);
    }

    public void InitializeData(CollectiblesManager manager, Vector2Int tileLocation, float lifetime)
    {
        TileLocation = tileLocation;
        this.manager = manager;

        transform.position = GameManager.Instance.Maze.GetWorldPositionForMazeTile(tileLocation);
        transform.localScale = GameManager.Instance.Maze.GetElementsScale();

        InitLifetime = lifetime;
        RemainingLifetime = lifetime;
    }

    // It would be generally better to separate collectibles into individual subclasses
    // than to have one "super class" managing their functionality.
    // However, let's have it like this for simplicity now.
    public void CheckPickUpByPlayer(AbstractPlayer player)
    {
        if (MarkedForDestroy) { return; }

        if (player.CurrentTile == TileLocation)
        {
            if (type == CollectibleItemType.AddPoint)
            {
                ++player.Points;
            }
            else if (type == CollectibleItemType.RespawnAll)
            {
                manager.RespawnAllCollectibles();
            }
            else if (type == CollectibleItemType.IncreaseMovementSpeed)
            {
                player.MovementSpeed *= GameManager.Instance.MovSpeedCollIncrease;
            }
            player.OnItemPickup(this);

            AudioSource.PlayClipAtPoint(pickupSound, GameManager.Instance.MainCamera.transform.position, 1.0f);

            Destroy(gameObject);
            MarkedForDestroy = true;
        }
    }

    private void CheckLifetime()
    {
        RemainingLifetime -= Time.deltaTime;

        var color = spriteRenderer.color;
        color.a = Mathf.Lerp(0.05f, 1.0f, RemainingLifetime / InitLifetime);
        spriteRenderer.color = color;

        if (RemainingLifetime <= 0)
        {
            Destroy(gameObject);
            MarkedForDestroy = true;
        }
    }
}
