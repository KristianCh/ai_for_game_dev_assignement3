using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;

[System.Serializable]
public class CollectibleProbabilityPair
{
    public CollectibleItemType type;

    [Range(0.0f, 1.0f)]
    public float probability;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Gameplay object references")]
    [SerializeField]
    private Maze maze;

    [SerializeField]
    private CollectiblesManager collectiblesManager;

    [SerializeField]
    private GameObject humanPlayerPrefab;

    [SerializeField]
    private GameObject[] computerPlayersPrefabs;

    [Header("Other objects references")]
    [SerializeField]
    private GameObject gameplayCanvas;

    [SerializeField]
    private GameObject gameOverCanvas;

    [SerializeField]
    private Transform scoreParentTransform;

    [SerializeField]
    private GameObject scoreUserInterfacePrefab;

    [SerializeField]
    private TMPro.TMP_Text timeRemainingText;

    [SerializeField]
    private TMPro.TMP_Text winnerText;

    [SerializeField]
    private TMPro.TMP_Text gameOverPoints;

    [SerializeField]
    private UnityEngine.UI.Image winnerImage;

    [Header("Gameplay settings")]
    [SerializeField]
    private float gameLengthSeconds = 60;

    [SerializeField]
    private int collectiblesMaxCount = 4;
       
    [SerializeField]
    private float collectiblesLifetimeSec = 10.0f;

    [SerializeField]
    private float collectiblesIndividualRespawnDelay = 2.0f;

    [SerializeField]
    private CollectibleProbabilityPair[] collectibleProbabilities;

    [SerializeField]
    private bool audioEnabled = true;

    [SerializeField]
    private float movementSpeed = 5.0f;

    [SerializeField]
    private float botSpeedModifier = 0.75f;

    [SerializeField]
    private float movSpeedCollIncrease = 1.1f;

    [SerializeField]
    private float maxSpeedMultiple = 2.0f;

    public Maze Maze => maze;

    public List<AbstractPlayer> Players { get; private set; } = 
        new List<AbstractPlayer>();

    public List<CollectibleItem> SpawnedCollectibles => collectiblesManager.SpawnedCollectibles;

    private Camera _mainCamera;
    public Camera MainCamera
    {
        get
        {
            if(_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            return _mainCamera;
        }
    }

    public float TimeRemaining { get; private set; }

    public float MovSpeedCollIncrease => movSpeedCollIncrease;

    public float MaxSpeedMultiple => maxSpeedMultiple;

    private float lastTimeTextUpdate = float.MaxValue;

    private float timeTextUpdateSpeed = 0.5f;

    private List<PlayerPointsUI> uiPoints = new List<PlayerPointsUI>();

    /////////////////////////////////////////
    public AStar aStar = new AStar();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple instanced of game manager occured.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Time.timeScale = 1.0f;
        TimeRemaining = gameLengthSeconds;
        
        gameplayCanvas.SetActive(true);
        gameOverCanvas.SetActive(false);

        StartNewGame();
    }

    private void Update()
    {
        TimeRemaining -= Time.deltaTime;

        TimeTextUpdater();
        CheckGameOver();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void StartNewGame()
    {
        maze.BuildMaze();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        if (computerPlayersPrefabs.Length != maze.ComputerPlayersSpawnTilePos.Count)
        {
            Debug.LogWarning("The number of CPU agent spawn positions and CPU agent prefabs do not match!");
        }

        Blackboard.Instance.HumanPlayer = SpawnPlayer(humanPlayerPrefab, maze.HumanPlayerSpawnTilePos);
        Players.Add(Blackboard.Instance.HumanPlayer);
        Blackboard.Instance.PathsCache = new List<Vector2Int>[maze.MazeTiles[0].Count, maze.MazeTiles.Count, maze.MazeTiles[0].Count, maze.MazeTiles.Count];

        for(var i = 0; i < Mathf.Min(computerPlayersPrefabs.Length, maze.ComputerPlayersSpawnTilePos.Count); ++i)
        {
            Players.Add(SpawnPlayer(computerPlayersPrefabs[i], maze.ComputerPlayersSpawnTilePos[i]));
        }

        foreach(var player in Players)
        {
            InitScore(player);
            player.PointsUpdated += OnPlayerPointsUpdated;
        }

        collectiblesManager.InitializeData(maze, collectiblesMaxCount, collectiblesLifetimeSec, 
            collectibleProbabilities, collectiblesIndividualRespawnDelay, Players.ToArray());

        collectiblesManager.RespawnAllCollectibles(1.0f);

        AudioListener.volume = audioEnabled ? 1.0f : 0.0f;

        foreach(var player in Players)
        {
            player.OnGameStarted();
        }
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private AbstractPlayer SpawnPlayer(GameObject playerPrefab, Vector2Int tilePos)
    {
        var playerGo = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        playerGo.name = playerPrefab.name; 

        var playerComp = playerGo.GetComponentInChildren<AbstractPlayer>();

        if (playerComp == null)
        {
            Debug.LogError("Invalid player prefab: " + playerPrefab.name);
        }
        else
        {
            playerComp.InitializeData(maze, movementSpeed * (playerPrefab == humanPlayerPrefab ? 1.0f : botSpeedModifier), tilePos);
        }

        return playerComp;
    }

    private void OnPlayerPointsUpdated(AbstractPlayer playerToUpdate)
    {
        foreach(var pointsObj in uiPoints)
        {
            pointsObj.UpdateScore();
        }
    }

    private void TimeTextUpdater()
    {
        if(lastTimeTextUpdate - TimeRemaining >= timeTextUpdateSpeed)
        {
            timeRemainingText.text = Mathf.Round(TimeRemaining).ToString() + " s";
            lastTimeTextUpdate = TimeRemaining;
        }
    }

    private void CheckGameOver()
    {
        if(TimeRemaining <= 0.0f)
        {
            Time.timeScale = 0;

            string winnerString = "";
            string gameOverPointsString = "";
            Sprite winnerSprite = null;
            Color winnerColor = Color.white;
            int numOfWinners = 0;

            // LINQ, pssst
            int maxScore = Players.Max(x => x.Points);

            for(var i = 0; i < Players.Count; ++i)
            {
                if(Players[i].Points == maxScore)
                {
                    winnerString += Players[i].name + " ";
                    winnerSprite = Players[i].Sprite;
                    winnerColor = Players[i].SpriteColor;
                    ++numOfWinners;
                }

                gameOverPointsString += Players[i].Points + ((i == Players.Count - 1) ? "" : ":");
            }

            if(numOfWinners == 1)
            {
                winnerText.text = "Winner is " + winnerString + "!";
                winnerImage.sprite = winnerSprite;
                winnerImage.color = winnerColor;
            } 
            else
            {
                winnerText.text = "We have a draw!";
                winnerImage.gameObject.SetActive(false);
            }

            gameOverPoints.text = gameOverPointsString;

            gameplayCanvas.SetActive(false);
            gameOverCanvas.SetActive(true);
        }
    }

    private void InitScore(AbstractPlayer player)
    {
        var pointsObject = Instantiate(scoreUserInterfacePrefab, scoreParentTransform);
        var uiPoints = pointsObject.GetComponent<PlayerPointsUI>();

        uiPoints.Initialize(player);
        this.uiPoints.Add(uiPoints);
    }

    // search functions
    public List<Vector2Int> GetPathFromTo(Vector2Int srcTile, Vector2Int destTile)
    {
        if (Blackboard.Instance.PathsCache[srcTile.x, srcTile.y, destTile.x, destTile.y] != null)
        {
            return Blackboard.Instance.PathsCache[srcTile.x, srcTile.y, destTile.x, destTile.y];
        }
        List<Vector2Int> path = aStar.CalculatePath(maze, srcTile, destTile);
        Blackboard.Instance.PathsCache[srcTile.x, srcTile.y, destTile.x, destTile.y] = path;
        return path;
    }

    public float GetDistanceFromTo(Vector2Int srcTile, Vector2Int destTile)
    {
        List<Vector2Int> path = GetPathFromTo(srcTile, destTile);
        return (float)path.Count;
    }

    public Vector2Int GetClosestCollectibleOfType(Vector2Int startPos, CollectibleItemType type)
    {
        bool found = false;
        Vector2Int closestPos = new Vector2Int(1000, 1000);
        float distance = 100000;
        foreach (CollectibleItem item in SpawnedCollectibles)
        {
            float currentDistance = GetDistanceFromTo(startPos, item.TileLocation);
            if ((item.Type == type || type == CollectibleItemType.Any) && currentDistance < distance)
            {
                distance = currentDistance;
                closestPos = item.TileLocation;
                found = true;
            }
        }
        if (!found) return startPos;
        return closestPos;
    }

    public float GetDistanceToClosestCollectibleOfType(Vector2Int startPos, CollectibleItemType type)
    {
        float distance = 100000;
        foreach (CollectibleItem item in SpawnedCollectibles)
        {
            float currentDistance = GetDistanceFromTo(startPos, item.TileLocation);
            if ((item.Type == type || type == CollectibleItemType.Any) && currentDistance < distance)
            {
                distance = currentDistance;
            }
        }
        return distance;
    }

    public bool ExistsCollectibleOfType(CollectibleItemType type)
    {
        foreach (CollectibleItem item in SpawnedCollectibles)
        {
            if (item.Type == type)
            {
                return true;
            }
        }
        return false;
    }
}
