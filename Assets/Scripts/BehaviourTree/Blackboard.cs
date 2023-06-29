using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackboard
{
    public static Blackboard Instance;
    
    private AbstractPlayer _humanPlayer;
    private List<Vector2Int>[,,,] _pathsCache;
    private CollectibleItem[,] _collectiblesAtLocations;

    public AbstractPlayer HumanPlayer
    {
        get => _humanPlayer;
        set => _humanPlayer = value;
    }

    public List<Vector2Int>[,,,] PathsCache
    {
        get => _pathsCache;
        set => _pathsCache = value;
    }

    public CollectibleItem[,] CollectiblesAtLocations
    {
        get => _collectiblesAtLocations;
        set => _collectiblesAtLocations = value;
    }

    public Blackboard()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}
