using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneFlowController : MonoBehaviour
{
    public const float EmailBranchDuration = 15f;
    public const float ShoppingBranchDuration = 15f;
    public const float ShoppingNaturalDistractionSeconds = 8f;
    public const float InsBranchDuration = 15f;
    const float EndDuration = 15f;
    const float ShoppingDistractionReturnDelay = 3f;

    static GameSceneFlowController instance;
    static bool playSessionInitialized;

    string activeSceneName;
    float sceneTimer;
    float shoppingDistractionTimer;
    EndSceneMinecraftUI endSceneUI;
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
            float branchDuration = GetBranchDuration(activeSceneName);

            if (ShouldReturnAfterShoppingDistraction())
            {
                shoppingDistractionTimer += Time.deltaTime;
                if (shoppingDistractionTimer >= ShoppingDistractionReturnDelay)
                {
                    ReturnFromActiveBranch();
                }
            }
            else
            {
                shoppingDistractionTimer = 0f;
            }

            if (!isReturning && sceneTimer >= branchDuration)
            {
                ReturnFromActiveBranch();
            }

            return;
        }

        if (IsEndScene())
        {
            sceneTimer += Time.deltaTime;
            if (endSceneUI != null)
            {
                endSceneUI.UpdateDisplay(sceneTimer, EndDuration);
            }

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
        shoppingDistractionTimer = 0f;
        isReturning = false;
        DestroyFlowCanvas();

        if (IsBeginScene())
        {
            return;
        }

        if (IsEndScene())
        {
            endSceneUI = FindObjectOfType<EndSceneMinecraftUI>();
            if (endSceneUI != null)
            {
                endSceneUI.SetSummary(GameRunState.BuildEndSummary());
                endSceneUI.UpdateDisplay(0f, EndDuration);
            }
            return;
        }

        if (activeSceneName == "Ins")
        {
            BuildBranchCanvas(
                "Reading Flow Interrupted by App Notifications",
                "Press the entrance button of this zone to See the photo",
                true);
            return;
        }

        if (activeSceneName == "Shopping")
        {
            RiverBoatGameController boat = FindFirstObjectByType<RiverBoatGameController>();
            if (boat != null)
            {
                boat.travelSeconds = ShoppingBranchDuration;
                boat.distractionReachSeconds = ShoppingNaturalDistractionSeconds;
            }

            BuildBranchCanvas(
                "A Mind Ship Distracted by Website Shopping Notifications",
                "Press the entrance button of this zone to Keep the ship in the middle",
                false);
            return;
        }
    }

    public static float GetBranchDuration(string sceneName)
    {
        if (sceneName == "Email")
        {
            return EmailBranchDuration;
        }

        if (sceneName == "Shopping")
        {
            return ShoppingBranchDuration;
        }

        if (sceneName == "Ins")
        {
            return InsBranchDuration;
        }

        return InsBranchDuration;
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
            GameRunState.EnsureRunStarted();
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

    bool ShouldReturnAfterShoppingDistraction()
    {
        if (isReturning || activeSceneName != "Shopping")
        {
            return false;
        }

        RiverBoatGameController boat = FindFirstObjectByType<RiverBoatGameController>();
        return boat != null && boat.TouchedShore;
    }

    void ReturnFromActiveBranch()
    {
        if (isReturning)
        {
            return;
        }

        isReturning = true;
        SaveActiveSceneStats();
        GameRunState.ReturnToMainFromBranch(activeSceneName, sceneTimer);
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

    void BuildBranchCanvas(string title, string prompt, bool placeTitleOnRight)
    {
        GameObject canvasObject = CreateCanvas("Flow Branch UI Canvas");

        BuildBranchTitle(canvasObject.transform, title, placeTitleOnRight);
        BuildLeftPrompt(canvasObject.transform, prompt);
    }

    void BuildBranchTitle(Transform parent, string title, bool placeOnRight)
    {
        TextMeshProUGUI titleText = AddText(parent, "Branch Title", title, placeOnRight ? 32f : 38f, TextAlignmentOptions.Center);
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        titleText.raycastTarget = false;
        titleText.textWrappingMode = TextWrappingModes.Normal;
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = placeOnRight ? 22f : 28f;
        titleText.fontSizeMax = placeOnRight ? 32f : 38f;

        if (placeOnRight)
        {
            titleText.rectTransform.anchorMin = new Vector2(1f, 1f);
            titleText.rectTransform.anchorMax = new Vector2(1f, 1f);
            titleText.rectTransform.pivot = new Vector2(1f, 1f);
            titleText.rectTransform.anchoredPosition = new Vector2(-42f, -330f);
            titleText.rectTransform.sizeDelta = new Vector2(360f, 130f);
            return;
        }

        titleText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        titleText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        titleText.rectTransform.pivot = new Vector2(0.5f, 1f);
        titleText.rectTransform.anchoredPosition = new Vector2(220f, -28f);
        titleText.rectTransform.sizeDelta = new Vector2(860f, 96f);
    }

    void BuildLeftPrompt(Transform parent, string prompt)
    {
        GameObject promptObject = new GameObject("Zone Action Prompt", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        promptObject.transform.SetParent(parent, false);

        RectTransform promptRect = promptObject.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0f, 1f);
        promptRect.anchorMax = new Vector2(0f, 1f);
        promptRect.pivot = new Vector2(0f, 1f);
        promptRect.anchoredPosition = new Vector2(32f, -24f);
        promptRect.sizeDelta = new Vector2(620f, 82f);

        Image promptImage = promptObject.GetComponent<Image>();
        promptImage.color = new Color(0.06f, 0.075f, 0.095f, 0.9f);
        promptImage.raycastTarget = false;

        GameObject accentObject = new GameObject("Prompt Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        accentObject.transform.SetParent(promptObject.transform, false);

        RectTransform accentRect = accentObject.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(6f, 0f);

        Image accentImage = accentObject.GetComponent<Image>();
        accentImage.color = new Color(0.58f, 0.66f, 0.76f, 1f);
        accentImage.raycastTarget = false;

        TextMeshProUGUI promptText = AddText(promptRect, "Prompt Text", prompt, 26f, TextAlignmentOptions.MidlineLeft);
        promptText.color = new Color(0.94f, 0.96f, 0.98f, 1f);
        promptText.fontStyle = FontStyles.Normal;
        promptText.textWrappingMode = TextWrappingModes.Normal;
        promptText.raycastTarget = false;
        promptText.rectTransform.anchorMin = Vector2.zero;
        promptText.rectTransform.anchorMax = Vector2.one;
        promptText.rectTransform.offsetMin = new Vector2(26f, 8f);
        promptText.rectTransform.offsetMax = new Vector2(-24f, -8f);
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

        endSceneUI = null;
    }
}
