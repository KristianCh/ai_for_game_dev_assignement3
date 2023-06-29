using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackboard
{
    public static Blackboard Instance;

    public AbstractPlayer HumanPlayer { get; set; }

    public List<Vector2Int>[,,,] PathsCache { get; set; }

    public CollectibleItem[,] CollectiblesAtLocations { get; set; }

    public Blackboard()
    {
        Instance ??= this;
    }
}
