using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectiblesManager : MonoBehaviour
{
    [SerializeField]
    private Maze parentMaze;

    [SerializeField]
    private GameObject[] collectiblePrefabs;

    private int collectiblesMaxCount;

    private float collectiblesLifetime;

    private float collectiblesIndividualRespawnDelay;

    private AbstractPlayer[] players;

    private int collectiblesToReplace = 0;

    private bool respawnAllInProgress = false;

    public List<CollectibleItem> SpawnedCollectibles { get; } = new List<CollectibleItem>();

    public static CollectiblesManager Instance;

    private HashSet<Vector2Int> availableSpawnLocations = new HashSet<Vector2Int>();

    private CollectibleProbabilityPair[] collectibleProbabilities;

    private void Update()
    {
        CheckPickUps();
        CheckCollectiblesCount();
    }

    public void InitializeData(Maze parentMaze, int collectiblesMaxCount, float collectiblesLifetime,
        CollectibleProbabilityPair[] collectibleProbabilities, float individualRespawnDelay, params AbstractPlayer[] players)
    {
        this.parentMaze = parentMaze;
        this.collectiblesMaxCount = collectiblesMaxCount;
        this.collectiblesLifetime = collectiblesLifetime;
        this.collectibleProbabilities = collectibleProbabilities;
        this.collectiblesIndividualRespawnDelay = individualRespawnDelay;
        this.players = players;

        Instance = this;
        Blackboard.Instance.CollectiblesAtLocations = new CollectibleItem[parentMaze.MazeTiles[0].Count, parentMaze.MazeTiles.Count];
        InitializeAvailableSpawnLocations();        
    }

    public void RespawnAllCollectibles(float generalRespawnDelay = 0.0f)
    {
        StartCoroutine(
            RespawnAllCollectiblesSequence(generalRespawnDelay));
    }

    private IEnumerator RespawnAllCollectiblesSequence(float generalRespawnDelay)
    {
        respawnAllInProgress = true;

        for (int i = 0; i < SpawnedCollectibles.Count; ++i)
        {
            Destroy(SpawnedCollectibles[i].gameObject);
        }

        yield return new WaitForSeconds(generalRespawnDelay);

        for(int i = 0; i < collectiblesMaxCount; ++i)
        {
            SpawnNewCollectible();
            yield return new WaitForSeconds(collectiblesIndividualRespawnDelay);
        }

        respawnAllInProgress = false;
    }

    private void SpawnNewCollectible()
    {
        if(SpawnedCollectibles.Count >= collectiblesMaxCount) { return; }

        Vector2Int spawnLocation = GenerateRandomLocation();

        if (spawnLocation.x >= 0)
        {
            GameObject collectibleObjToSpawn = null;

            float currProb = 0.0f;
            float currRndVal = Random.value;
            for(var i = 0; i < collectibleProbabilities.Length; ++i)
            {
                currProb += collectibleProbabilities[i].probability;

                if(currRndVal <= currProb)
                {
                    collectibleObjToSpawn = collectiblePrefabs.FirstOrDefault(x => x.GetComponent<CollectibleItem>().Type == collectibleProbabilities[i].type);

                    if(collectibleObjToSpawn == null)
                    {
                        Debug.LogError("Given collectible not found: " + collectibleProbabilities[i].type);
                    }

                    break;
                }
            }

            GameObject collectibleGo = Instantiate(collectibleObjToSpawn, Vector3.zero, Quaternion.identity, transform);

            var collItComp = collectibleGo.GetComponent<CollectibleItem>();
            collItComp.InitializeData(this, spawnLocation, collectiblesLifetime);
            collItComp.Destroyed += OnCollectibleDestroyed;

            Blackboard.Instance.CollectiblesAtLocations[spawnLocation.x, spawnLocation.y] = collItComp;
            availableSpawnLocations.Remove(spawnLocation);
            SpawnedCollectibles.Add(collItComp);
        }
    }

    private Vector2Int GenerateRandomLocation()
    {
        const int numberOfTrialsThreshold = 32;

        var availableLocations = availableSpawnLocations.ToArray();
        int chosenLocation = -1;
        int numberOfTrials = 0;
        
        while(availableLocations.Length > players.Length && 
              (chosenLocation < 0 || AnyPlayerAtLocation(availableLocations[chosenLocation])))
        {
            chosenLocation = Random.Range(0, availableLocations.Length);
            ++numberOfTrials;

            if(numberOfTrials > numberOfTrialsThreshold)
            {
                chosenLocation = -1;
                break;
            }
        }

        return chosenLocation < 0 ? new Vector2Int(int.MinValue, int.MinValue) : availableLocations[chosenLocation];
    }

    private bool AnyPlayerAtLocation(Vector2Int location)
    {
        for(int i = 0; i < players.Length; ++i)
        {
            if(players[i].CurrentTile == location)
            {
                return true;
            }
        }

        return false;
    }

    private void InitializeAvailableSpawnLocations()
    {
        var mazeTiles = parentMaze.MazeTiles;

        for(int y = 0; y < mazeTiles.Count; ++y)
        {
            for(int x = 0; x < mazeTiles[y].Count; ++x)
            {
                if(mazeTiles[y][x] == MazeTileType.Free)
                {
                    availableSpawnLocations.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    private void CheckPickUps()
    {
        // The purpose of this code is to ensure that the list
        // will be traversed in alternating order (front-to-back, back-to-front)
        // during the gameplay.
        // The reason is to provide a very simple "solution" to a situation
        // in which more players are at the tile with bonus.
        // The bonus will be given to the first player being evaluated
        // in the loop below. By alternating the loop order every frame,
        // both players will have an "equal chance" of being the first one
        // in the long term.
        bool isFrontToBackDir = Time.frameCount % 2 == 0;
        int indexStart = 0, indexEnd = players.Length, indexStep = 1;
        System.Func<int, int, bool> comparator = (int x, int y) => x < y;

        if(!isFrontToBackDir)
        {
            indexStart = players.Length - 1;
            indexEnd = 0;
            indexStep = -1;
            comparator = (int x, int y) => x >= y;
        }

        for (int i = indexStart; comparator(i, indexEnd); i += indexStep)
        {
            for(int j = 0; j < SpawnedCollectibles.Count; ++j)
            {
                SpawnedCollectibles[j].CheckPickUpByPlayer(players[i]);
            }
        }
    }

    private void CheckCollectiblesCount()
    {
        if(respawnAllInProgress) { return; }

        // Spawn of "replacement" collectibles is intentionally here and not
        // immediately in OnCollectibleDestroyed since when the application was exiting,
        // the Unity was having issues with immediate creation of existing collectibles.
        // The reason probably lied in the fact that OnCollectibleDestroyed is called
        // from collectible's OnDestroy.
        for(; collectiblesToReplace > 0; --collectiblesToReplace)
        {
            SpawnNewCollectible();
        }
    }

    private void OnCollectibleDestroyed(CollectibleItem collectible)
    {
        Blackboard.Instance.CollectiblesAtLocations[collectible.TileLocation.x, collectible.TileLocation.y] = null;
        SpawnedCollectibles.Remove(collectible);
        availableSpawnLocations.Add(collectible.TileLocation);
        ++collectiblesToReplace;
    }
}
