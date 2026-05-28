using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneFlowController : MonoBehaviour
{
    const float BranchDuration = 30f;
    const float EndDuration = 20f;

    static GameSceneFlowController instance;
    static bool playSessionInitialized;

    string activeSceneName;
    float sceneTimer;
    TextMeshProUGUI labelText;
    TextMeshProUGUI timerText;
    bool isReturning;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        EnsureInstance();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        instance = null;
        playSessionInitialized = false;
    }

    static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject("Game Scene Flow Controller");
        instance = controllerObject.AddComponent<GameSceneFlowController>();
        DontDestroyOnLoad(controllerObject);
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        ConfigureForScene(SceneManager.GetActiveScene());
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (IsBeginScene())
        {
            if (Input.anyKeyDown)
            {
                GameRunState.ResetRun();
                GameRunState.BeginRun();
                SceneManager.LoadScene(GameRunState.MainSceneName);
            }

            return;
        }

        if (IsBranchScene())
        {
            sceneTimer += Time.deltaTime;

            if (!isReturning && sceneTimer >= BranchDuration)
            {
                isReturning = true;
                SaveActiveSceneStats();
                GameRunState.ReturnToMainFromBranch(activeSceneName, sceneTimer);
            }

            return;
        }

        if (IsEndScene())
        {
            sceneTimer += Time.deltaTime;
            UpdateTimer(EndDuration - sceneTimer);

            if (sceneTimer >= EndDuration)
            {
                SceneManager.LoadScene(GameRunState.BeginSceneName);
            }
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConfigureForScene(scene);
    }

    void ConfigureForScene(Scene scene)
    {
        activeSceneName = scene.name;
        InitializePlaySessionIfNeeded(activeSceneName);
        sceneTimer = 0f;
        isReturning = false;
        DestroyFlowCanvas();

        if (IsBeginScene())
        {
            BuildCenteredCanvas("Press any button to begin", "");
            return;
        }

        if (IsEndScene())
        {
            BuildCenteredCanvas(GameRunState.BuildEndSummary(), "");
            return;
        }
    }

    void InitializePlaySessionIfNeeded(string sceneName)
    {
        if (playSessionInitialized)
        {
            return;
        }

        playSessionInitialized = true;

        if (sceneName == GameRunState.MainSceneName)
        {
            GameRunState.ResetRun();
            GameRunState.BeginRun();
        }
    }

    bool IsBeginScene()
    {
        return activeSceneName == GameRunState.BeginSceneName;
    }

    bool IsEndScene()
    {
        return activeSceneName == GameRunState.EndSceneName;
    }

    bool IsBranchScene()
    {
        return activeSceneName == "Email" || activeSceneName == "Shopping" || activeSceneName == "Ins";
    }

    void SaveActiveSceneStats()
    {
        if (activeSceneName == "Email")
        {
            TestMailPopupUI popupUI = FindObjectOfType<TestMailPopupUI>();
            if (popupUI != null)
            {
                GameRunState.SaveEmailStats(popupUI.TotalSpawned, popupUI.ActiveCount, popupUI.ReadCount, popupUI.UnreadCount);
            }
            return;
        }

        if (activeSceneName == "Shopping")
        {
            ShoppingInformationDistraction shopping = FindObjectOfType<ShoppingInformationDistraction>();
            RiverBoatGameController boat = FindObjectOfType<RiverBoatGameController>();
            GameRunState.SaveShoppingStats(
                shopping != null ? shopping.SpawnedCount : 0,
                boat != null && boat.HasFinished,
                boat != null && boat.TouchedShore);
            return;
        }

        if (activeSceneName == "Ins")
        {
            PhotoNotificationPreviewController preview = FindObjectOfType<PhotoNotificationPreviewController>();
            GameRunState.SaveInsStats(
                preview != null ? preview.ViewCount : 0,
                preview != null ? preview.NotificationCount : 0);
        }
    }

    void BuildCenteredCanvas(string mainText, string secondaryText)
    {
        GameObject canvasObject = CreateCanvas("Flow Center Canvas");
        GameObject panelObject = new GameObject("Flow Panel", typeof(RectTransform));
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.SetParent(canvasObject.transform, false);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(920f, 680f);

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.04f, 0.05f, 0.06f, 0.82f);
        panelImage.raycastTarget = false;

        labelText = AddText(panelRect, "Flow Label", mainText, 42f, TextAlignmentOptions.Center);
        labelText.rectTransform.anchorMin = Vector2.zero;
        labelText.rectTransform.anchorMax = Vector2.one;
        labelText.rectTransform.offsetMin = new Vector2(60f, 80f);
        labelText.rectTransform.offsetMax = new Vector2(-60f, -80f);

        timerText = AddText(panelRect, "Flow Timer", secondaryText, 28f, TextAlignmentOptions.Center);
        timerText.rectTransform.anchorMin = new Vector2(0f, 0f);
        timerText.rectTransform.anchorMax = new Vector2(1f, 0f);
        timerText.rectTransform.pivot = new Vector2(0.5f, 0f);
        timerText.rectTransform.anchoredPosition = new Vector2(0f, 26f);
        timerText.rectTransform.sizeDelta = new Vector2(0f, 44f);
    }

    GameObject CreateCanvas(string canvasName)
    {
        GameObject canvasObject = new GameObject(canvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        return canvasObject;
    }

    TextMeshProUGUI AddText(Transform parent, string objectName, string value, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;

        return text;
    }

    void UpdateTimer(float remainingSeconds)
    {
        if (timerText == null)
        {
            return;
        }

        timerText.text = Mathf.CeilToInt(Mathf.Max(0f, remainingSeconds)).ToString("00") + "s";
    }

    void DestroyFlowCanvas()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas != null && canvas.name.StartsWith("Flow "))
            {
                Destroy(canvas.gameObject);
            }
        }

        labelText = null;
        timerText = null;
    }
}
