using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameRunState
{
    public const string MainSceneName = "Main game";
    public const string BeginSceneName = "Begin";
    public const string EndSceneName = "End";

    const string Prefix = "IgnoreMe.";
    const string ReturnSceneKey = "ReturnSceneName";
    const string ReturnNodeKey = "ReturnNodeName";

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
        PlayerPrefs.Save();
    }

    public static void BeginRun()
    {
        SetInt("RunStarted", 1);
        SetFloat("RunStartTime", Time.time);
        PlayerPrefs.Save();
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
        return "Run summary\n\n"
            + "Main game\n"
            + "Time: " + GetFloat("MainTimeSeconds", 0f).ToString("0") + "s\n"
            + "Actual steps: " + GetInt("MainActualSteps", 0) + "\n"
            + "Ideal path: " + GetInt("MainIdealSteps", 0) + "\n"
            + "Extra steps: " + GetInt("MainExtraSteps", 0) + "\n\n"
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
}
