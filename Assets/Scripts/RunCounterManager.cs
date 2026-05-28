using UnityEngine;
using UnityEngine.SceneManagement;

public class RunCounterManager : MonoBehaviour
{
    public static RunCounterManager Instance { get; private set; }

    [Header("Main Maze Counters")]
    public bool mainMazeRotated;
    public int mainMazeRotationCount;

    [Header("Special Space Counters")]
    public bool emailVisited;
    public bool shoppingVisited;
    public bool insVisited;
    public int visitedBranchCount;

    [Header("Runtime Sync")]
    public bool keepSynced = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SyncFromRunState();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SyncFromRunState();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (keepSynced)
        {
            SyncFromRunState();
        }
    }

    public static void SyncActiveCounters()
    {
        if (Instance != null)
        {
            Instance.SyncFromRunState();
        }
    }

    public void SyncFromRunState()
    {
        mainMazeRotationCount = GameRunState.GetMainMazeRotationCount();
        mainMazeRotated = GameRunState.HasMainMazeRotated();

        emailVisited = GameRunState.HasEnteredBranch("Email");
        shoppingVisited = GameRunState.HasEnteredBranch("Shopping");
        insVisited = GameRunState.HasEnteredBranch("Ins");
        visitedBranchCount = GameRunState.GetVisitedBranchCount();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SyncFromRunState();
    }
}
