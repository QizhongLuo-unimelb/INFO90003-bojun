using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestMailInteractionController : MonoBehaviour
{
    [Header("Interaction")]
    public KeyCode clearMailKey = KeyCode.L;
    public string returnSceneName = "Main game";
    public string clearPromptText = "Press the entrance button of this zone to clear the Email";
    public string sceneTitleText = "The Tree of Focus Disturbed by Emails";

    [Header("Timing")]
    public float countdownSeconds = 30f;

    [Header("References")]
    public TestMailPopupUI popupUI;
    public TestExperienceController experienceController;

    TextMeshProUGUI hudLabelText;
    TextMeshProUGUI hudValueText;
    float remainingSeconds;
    bool loadingScene;

    void Awake()
    {
        remainingSeconds = countdownSeconds;
        if (popupUI == null)
        {
            popupUI = GetComponent<TestMailPopupUI>();
        }

        if (experienceController == null)
        {
            experienceController = FindFirstObjectByType<TestExperienceController>();
        }

        BuildHud();
    }

    void Update()
    {
        if (popupUI == null)
        {
            popupUI = TestMailPopupUI.Instance;
        }

        if (Input.GetKeyDown(clearMailKey))
        {
            TriggerClearMail();
        }

        remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.deltaTime);
        UpdateHud();

        if (!loadingScene && remainingSeconds <= 0f)
        {
            loadingScene = true;
            if (popupUI != null)
            {
                GameRunState.SaveEmailStats(popupUI.TotalSpawned, popupUI.ActiveCount, popupUI.ReadCount, popupUI.UnreadCount);
            }

            GameRunState.ReturnToMainFromBranch(SceneManager.GetActiveScene().name, countdownSeconds);
        }
    }

    public void TriggerClearMail()
    {
        if (popupUI == null)
        {
            popupUI = TestMailPopupUI.Instance;
        }

        if (popupUI == null)
        {
            return;
        }

        int readNow = popupUI.MarkVisibleAsRead();
        if (experienceController != null)
        {
            experienceController.ApplyReadRelief(readNow);
        }
    }

    void BuildHud()
    {
        GameObject canvasObject = new GameObject("Mail Status HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = new GameObject("Status Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-24f, -24f);
        panelRect.sizeDelta = new Vector2(260f, 140f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.93f, 0.96f, 0.98f, 0.88f);
        panelImage.raycastTarget = false;

        hudLabelText = CreateHudText(panelObject.transform, "Status Labels", new Vector2(16f, 10f), new Vector2(-104f, -10f), TextAlignmentOptions.TopRight);
        hudValueText = CreateHudText(panelObject.transform, "Status Values", new Vector2(164f, 10f), new Vector2(-16f, -10f), TextAlignmentOptions.TopRight);

        TextMeshProUGUI titleText = CreateCanvasText(canvasObject.transform, "Email Scene Title", sceneTitleText, 38f, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
        titleText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        titleText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        titleText.rectTransform.pivot = new Vector2(0.5f, 1f);
        titleText.rectTransform.anchoredPosition = new Vector2(220f, -28f);
        titleText.rectTransform.sizeDelta = new Vector2(860f, 96f);

        GameObject promptObject = new GameObject("Clear Email Prompt", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        promptObject.transform.SetParent(canvasObject.transform, false);

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

        TextMeshProUGUI promptText = CreateHudText(promptObject.transform, "Prompt Text", new Vector2(26f, 8f), new Vector2(-24f, -8f), TextAlignmentOptions.MidlineLeft);
        promptText.text = clearPromptText;
        promptText.fontSize = 26f;
        promptText.fontStyle = FontStyles.Normal;
        promptText.color = new Color(0.94f, 0.96f, 0.98f, 1f);
        promptText.textWrappingMode = TextWrappingModes.Normal;
    }

    void UpdateHud()
    {
        if (hudLabelText == null || hudValueText == null)
        {
            return;
        }

        int unread = popupUI != null ? popupUI.UnreadCount : 0;
        int read = popupUI != null ? popupUI.ReadCount : 0;
        int active = popupUI != null ? popupUI.ActiveCount : 0;
        int total = popupUI != null ? popupUI.TotalSpawned : 0;
        int seconds = Mathf.CeilToInt(remainingSeconds);

        hudLabelText.text = "Mail\nUnread\nRead\nTime";
        hudValueText.text = active + "/" + total
            + "\n" + unread
            + "\n" + read
            + "\n" + seconds.ToString("00");
    }

    TextMeshProUGUI CreateHudText(Transform parent, string name, Vector2 offsetMin, Vector2 offsetMax, TextAlignmentOptions alignment)
    {
        TextMeshProUGUI text = CreateCanvasText(parent, name, "", 24f, FontStyles.Bold, new Color(0.08f, 0.10f, 0.13f, 1f), alignment);

        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = offsetMin;
        textRect.offsetMax = offsetMax;

        text.enableWordWrapping = false;
        return text;
    }

    TextMeshProUGUI CreateCanvasText(Transform parent, string name, string value, float fontSize, FontStyles style, Color color, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }
}
