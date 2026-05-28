using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameRunState
{
    public const string MainSceneName = "Main game";
    public const string BeginSceneName = "Begin";
    public const string EndSceneName = "End";

    const int PerfectFocusScore = 9999;
    const float PerfectTimeSeconds = 90f;

    const string Prefix = "IgnoreMe.";
    const string ReturnSceneKey = "ReturnSceneName";
    const string ReturnNodeKey = "ReturnNodeName";
    const string VisitedNodeNamesKey = "VisitedNodeNames";
    const char VisitedNodeSeparator = '|';

    public struct FocusScoreResult
    {
        public int Score;
        public int TimePenalty;
        public int StepPenalty;
        public int BranchPenalty;
        public int DistractionPenalty;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetRunAtPlayStart()
    {
        ResetRun();
    }

    public static void ResetRun()
    {
        string[] keys =
        {
            "RunStarted",
            "RunStartTime",
            "MainTimeSeconds",
            "MainActualSteps",
            "MainIdealSteps",
            "MainExtraSteps",
            "MainMazeRotationCount",
            "EmailEntered",
            "EmailCompleted",
            "EmailDuration",
            "EmailTotalSpawned",
            "EmailActive",
            "EmailRead",
            "EmailUnread",
            "ShoppingEntered",
            "ShoppingCompleted",
            "ShoppingDuration",
            "ShoppingPopups",
            "ShoppingFinished",
            "ShoppingTouchedShore",
            "InsEntered",
            "InsCompleted",
            "InsDuration",
            "InsViews",
            "InsNotifications"
        };

        foreach (string key in keys)
        {
            PlayerPrefs.DeleteKey(Prefix + key);
        }

        PlayerPrefs.DeleteKey(ReturnSceneKey);
        PlayerPrefs.DeleteKey(ReturnNodeKey);
        PlayerPrefs.DeleteKey(Prefix + VisitedNodeNamesKey);
        PlayerPrefs.Save();
        RunCounterManager.SyncActiveCounters();
    }

    public static void BeginRun()
    {
        SetInt("RunStarted", 1);
        SetFloat("RunStartTime", Time.time);
        PlayerPrefs.Save();
        RunCounterManager.SyncActiveCounters();
    }

    public static void EnsureRunStarted()
    {
        if (GetInt("RunStarted", 0) > 0)
        {
            return;
        }

        BeginRun();
    }

    public static float GetRunElapsedSeconds()
    {
        return Mathf.Max(0f, Time.time - GetFloat("RunStartTime", Time.time));
    }

    public static int GetMainActualSteps()
    {
        return GetInt("MainActualSteps", 0);
    }

    public static int RecordMainStep(int idealSteps)
    {
        int actualSteps = GetMainActualSteps() + 1;
        SaveMainMazeStats(GetRunElapsedSeconds(), actualSteps, idealSteps);
        return actualSteps;
    }

    public static bool HasVisitedNode(string nodeName)
    {
        if (string.IsNullOrEmpty(nodeName))
        {
            return false;
        }

        string visitedNodeNames = GetString(VisitedNodeNamesKey, "");
        string[] names = visitedNodeNames.Split(VisitedNodeSeparator);

        foreach (string name in names)
        {
            if (name == nodeName)
            {
                return true;
            }
        }

        return false;
    }

    public static void MarkNodeVisited(string nodeName)
    {
        if (string.IsNullOrEmpty(nodeName) || HasVisitedNode(nodeName))
        {
            return;
        }

        string visitedNodeNames = GetString(VisitedNodeNamesKey, "");
        if (string.IsNullOrEmpty(visitedNodeNames))
        {
            SetString(VisitedNodeNamesKey, nodeName);
        }
        else
        {
            SetString(VisitedNodeNamesKey, visitedNodeNames + VisitedNodeSeparator + nodeName);
        }

        PlayerPrefs.Save();
    }

    public static bool HasEnteredBranch(string sceneName)
    {
        string scenePrefix = GetScenePrefix(sceneName);
        return !string.IsNullOrEmpty(scenePrefix) && GetInt(scenePrefix + "Entered", 0) > 0;
    }

    public static void MarkBranchEntered(string sceneName)
    {
        string scenePrefix = GetScenePrefix(sceneName);
        if (string.IsNullOrEmpty(scenePrefix))
        {
            return;
        }

        SetInt(scenePrefix + "Entered", 1);
        PlayerPrefs.Save();
        RunCounterManager.SyncActiveCounters();
    }

    public static bool CanEnterBranch(string sceneName)
    {
        return sceneName == EndSceneName || !HasEnteredBranch(sceneName);
    }

    public static void SaveReturnPoint(string currentSceneName, string returnNodeName)
    {
        PlayerPrefs.SetString(ReturnSceneKey, currentSceneName);
        PlayerPrefs.SetString(ReturnNodeKey, returnNodeName);
        PlayerPrefs.Save();
    }

    public static void ReturnToMainFromBranch(string sceneName, float durationSeconds)
    {
        CompleteBranch(sceneName, durationSeconds);

        string returnSceneName = PlayerPrefs.GetString(ReturnSceneKey, MainSceneName);
        if (string.IsNullOrEmpty(returnSceneName))
        {
            returnSceneName = MainSceneName;
        }

        SceneManager.LoadScene(returnSceneName);
    }

    public static void CompleteBranch(string sceneName, float durationSeconds)
    {
        string scenePrefix = GetScenePrefix(sceneName);
        if (string.IsNullOrEmpty(scenePrefix))
        {
            return;
        }

        SetInt(scenePrefix + "Completed", 1);
        SetFloat(scenePrefix + "Duration", durationSeconds);
        PlayerPrefs.Save();
    }

    public static void SaveMainMazeStats(float timeSeconds, int actualSteps, int idealSteps)
    {
        SetFloat("MainTimeSeconds", timeSeconds);
        SetInt("MainActualSteps", actualSteps);
        SetInt("MainIdealSteps", idealSteps);
        SetInt("MainExtraSteps", Mathf.Max(0, actualSteps - idealSteps));
        PlayerPrefs.Save();
    }

    public static int GetMainMazeRotationCount()
    {
        return GetInt("MainMazeRotationCount", 0);
    }

    public static bool HasMainMazeRotated()
    {
        return GetMainMazeRotationCount() > 0;
    }

    public static int AdvanceMainMazeRotationCount()
    {
        int rotationCount = GetMainMazeRotationCount() + 1;
        SetInt("MainMazeRotationCount", rotationCount);
        PlayerPrefs.Save();
        RunCounterManager.SyncActiveCounters();
        return rotationCount;
    }

    public static int GetVisitedBranchCount()
    {
        int count = 0;
        count += HasEnteredBranch("Email") ? 1 : 0;
        count += HasEnteredBranch("Shopping") ? 1 : 0;
        count += HasEnteredBranch("Ins") ? 1 : 0;
        return count;
    }

    public static void SaveEmailStats(int totalSpawned, int active, int read, int unread)
    {
        SetInt("EmailTotalSpawned", totalSpawned);
        SetInt("EmailActive", active);
        SetInt("EmailRead", read);
        SetInt("EmailUnread", unread);
        PlayerPrefs.Save();
    }

    public static void SaveShoppingStats(int popups, bool finished, bool touchedShore)
    {
        SetInt("ShoppingPopups", popups);
        SetInt("ShoppingFinished", finished ? 1 : 0);
        SetInt("ShoppingTouchedShore", touchedShore ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void SaveInsStats(int views, int notifications)
    {
        SetInt("InsViews", views);
        SetInt("InsNotifications", notifications);
        PlayerPrefs.Save();
    }

    public static string BuildEndSummary()
    {
        RunCounterManager.SyncActiveCounters();
        FocusScoreResult score = CalculateFocusScore();

        return "Run summary\n\n"
            + "Focus score: " + score.Score.ToString("0000") + "/" + PerfectFocusScore + "\n"
            + "Deductions\n"
            + "Time: -" + score.TimePenalty + "\n"
            + "Backtracking: -" + score.StepPenalty + "\n"
            + "Special spaces: -" + score.BranchPenalty + "\n"
            + "Distractions: -" + score.DistractionPenalty + "\n\n"
            + "Main game\n"
            + "Time: " + GetFloat("MainTimeSeconds", 0f).ToString("0") + "s\n"
            + "Actual steps: " + GetInt("MainActualSteps", 0) + "\n"
            + "Ideal path: " + GetInt("MainIdealSteps", 0) + "\n"
            + "Extra steps: " + GetInt("MainExtraSteps", 0) + "\n"
            + "Maze rotated: " + YesNo(HasMainMazeRotated()) + "\n"
            + "Rotation count: " + GetMainMazeRotationCount() + "\n"
            + "Special spaces visited: " + GetVisitedBranchCount() + "/3\n\n"
            + "Email\n"
            + "Visited: " + YesNo(GetInt("EmailEntered", 0) > 0) + "\n"
            + "Mail: " + GetInt("EmailActive", 0) + "/" + GetInt("EmailTotalSpawned", 0) + "\n"
            + "Unread: " + GetInt("EmailUnread", 0) + "\n"
            + "Read: " + GetInt("EmailRead", 0) + "\n\n"
            + "Shopping\n"
            + "Visited: " + YesNo(GetInt("ShoppingEntered", 0) > 0) + "\n"
            + "Shopping popups: " + GetInt("ShoppingPopups", 0) + "\n\n"
            + "Ins\n"
            + "Visited: " + YesNo(GetInt("InsEntered", 0) > 0) + "\n"
            + "Photo views: " + GetInt("InsViews", 0) + "\n"
            + "Notifications: " + GetInt("InsNotifications", 0);
    }

    public static FocusScoreResult CalculateFocusScore()
    {
        float timeSeconds = GetFloat("MainTimeSeconds", 0f);
        int extraSteps = GetInt("MainExtraSteps", 0);

        bool emailEntered = GetInt("EmailEntered", 0) > 0;
        bool shoppingEntered = GetInt("ShoppingEntered", 0) > 0;
        bool insEntered = GetInt("InsEntered", 0) > 0;

        int timePenalty = Mathf.Clamp(
            Mathf.CeilToInt(Mathf.Max(0f, timeSeconds - PerfectTimeSeconds) * 18f),
            0,
            2600);

        int stepPenalty = Mathf.Clamp(extraSteps * 520, 0, 2600);

        int branchPenalty = 0;
        branchPenalty += emailEntered ? 900 + Mathf.RoundToInt(GetFloat("EmailDuration", 0f) * 10f) : 0;
        branchPenalty += shoppingEntered ? 900 + Mathf.RoundToInt(GetFloat("ShoppingDuration", 0f) * 10f) : 0;
        branchPenalty += insEntered ? 900 + Mathf.RoundToInt(GetFloat("InsDuration", 0f) * 10f) : 0;
        branchPenalty = Mathf.Clamp(branchPenalty, 0, 3600);

        int distractionPenalty = 0;
        distractionPenalty += GetInt("EmailRead", 0) * 160;
        distractionPenalty += GetInt("EmailUnread", 0) * 45;
        distractionPenalty += GetInt("ShoppingPopups", 0) * 45;
        distractionPenalty += GetInt("ShoppingTouchedShore", 0) > 0 ? 700 : 0;
        distractionPenalty += GetInt("InsViews", 0) * 200;
        distractionPenalty += GetInt("InsNotifications", 0) * 25;
        distractionPenalty = Mathf.Clamp(distractionPenalty, 0, 3200);

        int score = Mathf.Clamp(
            PerfectFocusScore - timePenalty - stepPenalty - branchPenalty - distractionPenalty,
            0,
            PerfectFocusScore);

        return new FocusScoreResult
        {
            Score = score,
            TimePenalty = timePenalty,
            StepPenalty = stepPenalty,
            BranchPenalty = branchPenalty,
            DistractionPenalty = distractionPenalty
        };
    }

    static string GetScenePrefix(string sceneName)
    {
        if (sceneName == "Email")
        {
            return "Email";
        }

        if (sceneName == "Shopping")
        {
            return "Shopping";
        }

        if (sceneName == "Ins")
        {
            return "Ins";
        }

        return "";
    }

    static string YesNo(bool value)
    {
        return value ? "Yes" : "No";
    }

    static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(Prefix + key, value);
    }

    static int GetInt(string key, int fallback)
    {
        return PlayerPrefs.GetInt(Prefix + key, fallback);
    }

    static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(Prefix + key, value);
    }

    static float GetFloat(string key, float fallback)
    {
        return PlayerPrefs.GetFloat(Prefix + key, fallback);
    }

    static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(Prefix + key, value);
    }

    static string GetString(string key, string fallback)
    {
        return PlayerPrefs.GetString(Prefix + key, fallback);
    }
}
