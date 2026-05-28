using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailDanmakuCanvas : MonoBehaviour
{
    [Header("Icon")]
    public Sprite emailIcon;

    [Header("Spawn")]
    public float spawnInterval = 0.055f;
    public int laneCount = 11;
    public int warmStartCount = 46;
    public int maxMessagesOnScreen = 90;
    public float horizontalPadding = 420f;

    [Header("Movement")]
    public float minSpeed = 360f;
    public float maxSpeed = 760f;

    [Header("Card Size")]
    public float minWidth = 460f;
    public float maxWidth = 920f;
    public float cardHeight = 76f;
    public float widthPerCharacter = 13f;

    [Header("Colors")]
    public Color backgroundColor = new Color(0.04f, 0.055f, 0.075f, 0.78f);
    public Color urgentBackgroundColor = new Color(0.16f, 0.045f, 0.055f, 0.84f);
    public Color textColor = new Color(0.96f, 0.98f, 1f, 1f);
    public Color mutedTextColor = new Color(0.68f, 0.74f, 0.82f, 1f);

    readonly List<RectTransform> activeMessages = new List<RectTransform>();
    RectTransform canvasRect;
    RectTransform messageLayer;
    RectTransform cardTemplate;
    float timer;
    int nextLane;

    readonly string[] senders =
    {
        "Canvas", "Tutor", "Subject Admin", "Library", "Group Project",
        "Student Portal", "Unread Email", "Calendar", "LMS", "Assessment"
    };

    readonly string[] subjects =
    {
        "Assignment deadline updated",
        "New feedback has been released",
        "Meeting starts in 5 minutes",
        "Your draft needs revision",
        "Unread reply from your tutor",
        "Submission reminder: due tonight",
        "Group member mentioned you",
        "Course announcement posted",
        "Extension request received",
        "Quiz closes at midnight",
        "Action required before 5pm",
        "New rubric comment available",
        "Inbox storage almost full",
        "Booking confirmation attached",
        "Important update for tomorrow",
        "Reading list changed",
        "You have 39 unread messages",
        "Calendar conflict detected"
    };

    void Start()
    {
        BuildCanvas();
        BuildTemplate();
        StartCoroutine(WarmStart());
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnMessage(false);
        }
    }

    void BuildCanvas()
    {
        GameObject canvasObject = new GameObject("Email Danmaku Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasRect = canvasObject.GetComponent<RectTransform>();

        GameObject backdrop = new GameObject("Email Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform backdropRect = backdrop.GetComponent<RectTransform>();
        backdropRect.SetParent(canvasRect, false);
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;
        Image backdropImage = backdrop.GetComponent<Image>();
        backdropImage.color = new Color(0.035f, 0.045f, 0.062f, 0.18f);
        backdropImage.raycastTarget = false;

        BuildCenterStatus(canvasRect);

        GameObject layerObject = new GameObject("Email Message Layer", typeof(RectTransform));
        messageLayer = layerObject.GetComponent<RectTransform>();
        messageLayer.SetParent(canvasRect, false);
        messageLayer.anchorMin = Vector2.zero;
        messageLayer.anchorMax = Vector2.one;
        messageLayer.offsetMin = Vector2.zero;
        messageLayer.offsetMax = Vector2.zero;
    }

    void BuildCenterStatus(RectTransform parent)
    {
        GameObject statusObject = new GameObject("Email Space Status", typeof(RectTransform));
        RectTransform statusRect = statusObject.GetComponent<RectTransform>();
        statusRect.SetParent(parent, false);
        statusRect.anchorMin = new Vector2(0.5f, 0.5f);
        statusRect.anchorMax = new Vector2(0.5f, 0.5f);
        statusRect.pivot = new Vector2(0.5f, 0.5f);
        statusRect.anchoredPosition = new Vector2(0f, 96f);
        statusRect.sizeDelta = new Vector2(520f, 180f);

        GameObject titleObject = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.SetParent(statusRect, false);
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(0f, 52f);
        TextMeshProUGUI titleText = titleObject.GetComponent<TextMeshProUGUI>();
        ConfigureText(titleText, 34f, FontStyles.Bold, textColor, TextAlignmentOptions.Center, TextWrappingModes.NoWrap);
        titleText.text = "EMAIL SPACE";

        GameObject countRowObject = new GameObject("Unread Count Row", typeof(RectTransform));
        RectTransform countRowRect = countRowObject.GetComponent<RectTransform>();
        countRowRect.SetParent(statusRect, false);
        countRowRect.anchorMin = new Vector2(0.5f, 1f);
        countRowRect.anchorMax = new Vector2(0.5f, 1f);
        countRowRect.pivot = new Vector2(0.5f, 1f);
        countRowRect.anchoredPosition = new Vector2(0f, -54f);
        countRowRect.sizeDelta = new Vector2(116f, 36f);

        GameObject countIconObject = new GameObject("Unread Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform countIconRect = countIconObject.GetComponent<RectTransform>();
        countIconRect.SetParent(countRowRect, false);
        countIconRect.anchorMin = new Vector2(0f, 0.5f);
        countIconRect.anchorMax = new Vector2(0f, 0.5f);
        countIconRect.pivot = new Vector2(0f, 0.5f);
        countIconRect.anchoredPosition = new Vector2(0f, 0f);
        countIconRect.sizeDelta = new Vector2(30f, 30f);
        Image countIcon = countIconObject.GetComponent<Image>();
        countIcon.sprite = emailIcon;
        countIcon.color = textColor;
        countIcon.preserveAspect = true;
        countIcon.raycastTarget = false;

        GameObject countObject = new GameObject("Unread Count", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform countRect = countObject.GetComponent<RectTransform>();
        countRect.SetParent(countRowRect, false);
        countRect.anchorMin = new Vector2(0f, 0f);
        countRect.anchorMax = new Vector2(1f, 1f);
        countRect.offsetMin = new Vector2(38f, 0f);
        countRect.offsetMax = Vector2.zero;
        TextMeshProUGUI countText = countObject.GetComponent<TextMeshProUGUI>();
        ConfigureText(countText, 23f, FontStyles.Bold, textColor, TextAlignmentOptions.Center, TextWrappingModes.NoWrap);
        countText.text = "127";

        GameObject promptObject = new GameObject("Prompt", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform promptRect = promptObject.GetComponent<RectTransform>();
        promptRect.SetParent(statusRect, false);
        promptRect.anchorMin = new Vector2(0f, 1f);
        promptRect.anchorMax = new Vector2(1f, 1f);
        promptRect.pivot = new Vector2(0.5f, 1f);
        promptRect.anchoredPosition = new Vector2(0f, -98f);
        promptRect.sizeDelta = new Vector2(0f, 32f);
        TextMeshProUGUI promptText = promptObject.GetComponent<TextMeshProUGUI>();
        ConfigureText(promptText, 18f, FontStyles.Normal, textColor, TextAlignmentOptions.Center, TextWrappingModes.NoWrap);
        promptText.text = "You have unread messages.";
    }

    void BuildTemplate()
    {
        GameObject cardObject = new GameObject("Email Message Template", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        cardTemplate = cardObject.GetComponent<RectTransform>();
        cardTemplate.SetParent(messageLayer, false);
        cardTemplate.anchorMin = new Vector2(0.5f, 0.5f);
        cardTemplate.anchorMax = new Vector2(0.5f, 0.5f);
        cardTemplate.pivot = new Vector2(0.5f, 0.5f);
        cardTemplate.sizeDelta = new Vector2(minWidth, cardHeight);

        Image background = cardObject.GetComponent<Image>();
        background.color = backgroundColor;
        background.raycastTarget = false;

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.SetParent(cardTemplate, false);
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(20f, 0f);
        iconRect.sizeDelta = new Vector2(38f, 38f);
        Image iconImage = iconObject.GetComponent<Image>();
        iconImage.sprite = emailIcon;
        iconImage.color = textColor;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        GameObject senderObject = new GameObject("Sender", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform senderRect = senderObject.GetComponent<RectTransform>();
        senderRect.SetParent(cardTemplate, false);
        senderRect.anchorMin = new Vector2(0f, 1f);
        senderRect.anchorMax = new Vector2(1f, 1f);
        senderRect.pivot = new Vector2(0f, 1f);
        senderRect.offsetMin = new Vector2(72f, -34f);
        senderRect.offsetMax = new Vector2(-118f, -10f);
        TextMeshProUGUI senderText = senderObject.GetComponent<TextMeshProUGUI>();
        ConfigureText(senderText, 18f, FontStyles.Bold, textColor, TextAlignmentOptions.Left, TextWrappingModes.NoWrap);

        GameObject timeObject = new GameObject("Time", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform timeRect = timeObject.GetComponent<RectTransform>();
        timeRect.SetParent(cardTemplate, false);
        timeRect.anchorMin = new Vector2(1f, 1f);
        timeRect.anchorMax = new Vector2(1f, 1f);
        timeRect.pivot = new Vector2(1f, 1f);
        timeRect.anchoredPosition = new Vector2(-20f, -12f);
        timeRect.sizeDelta = new Vector2(92f, 24f);
        TextMeshProUGUI timeText = timeObject.GetComponent<TextMeshProUGUI>();
        ConfigureText(timeText, 15f, FontStyles.Normal, mutedTextColor, TextAlignmentOptions.Right, TextWrappingModes.NoWrap);

        GameObject subjectObject = new GameObject("Subject", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform subjectRect = subjectObject.GetComponent<RectTransform>();
        subjectRect.SetParent(cardTemplate, false);
        subjectRect.anchorMin = new Vector2(0f, 0f);
        subjectRect.anchorMax = new Vector2(1f, 1f);
        subjectRect.offsetMin = new Vector2(72f, 10f);
        subjectRect.offsetMax = new Vector2(-24f, -34f);
        TextMeshProUGUI subjectText = subjectObject.GetComponent<TextMeshProUGUI>();
        ConfigureText(subjectText, 23f, FontStyles.Bold, textColor, TextAlignmentOptions.Left, TextWrappingModes.NoWrap);

        cardObject.SetActive(false);
    }

    IEnumerator WarmStart()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < warmStartCount; i++)
        {
            SpawnMessage(true);
        }
    }

    void SpawnMessage(bool warmStart)
    {
        if (cardTemplate == null || messageLayer == null || canvasRect == null)
        {
            return;
        }

        CleanupMissingMessages();
        if (activeMessages.Count >= maxMessagesOnScreen)
        {
            return;
        }

        string sender = senders[Random.Range(0, senders.Length)];
        string subject = subjects[Random.Range(0, subjects.Length)];
        bool urgent = subject.Contains("required") || subject.Contains("deadline") || subject.Contains("tonight") || subject.Contains("midnight");

        RectTransform card = Instantiate(cardTemplate, messageLayer);
        card.gameObject.SetActive(true);
        activeMessages.Add(card);

        float targetWidth = Mathf.Clamp(subject.Length * widthPerCharacter + 210f, minWidth, maxWidth);
        card.sizeDelta = new Vector2(targetWidth, cardHeight);

        Image background = card.GetComponent<Image>();
        if (background != null)
        {
            background.color = urgent ? urgentBackgroundColor : backgroundColor;
        }

        TextMeshProUGUI[] texts = card.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.name == "Sender")
            {
                text.text = sender;
            }
            else if (text.name == "Time")
            {
                text.text = Random.Range(0, 4) == 0 ? "now" : Random.Range(1, 59) + "m ago";
            }
            else if (text.name == "Subject")
            {
                text.text = subject;
            }
        }

        float width = canvasRect.rect.width;
        float height = canvasRect.rect.height;
        float laneHeight = Mathf.Max(cardHeight + 10f, (height - 120f) / Mathf.Max(1, laneCount));
        int lane = warmStart ? Random.Range(0, laneCount) : nextLane;
        nextLane = (nextLane + 1) % Mathf.Max(1, laneCount);

        float top = height * 0.5f - 70f;
        float y = top - lane * laneHeight - Random.Range(0f, Mathf.Max(0f, laneHeight - cardHeight));
        float startX = -width * 0.5f - horizontalPadding;
        float endX = width * 0.5f + horizontalPadding;

        card.anchoredPosition = new Vector2(warmStart ? Random.Range(startX, endX) : startX, y);
        card.localRotation = Quaternion.identity;
        card.localScale = Vector3.one * Random.Range(0.92f, 1.08f);

        float speed = Random.Range(minSpeed, maxSpeed);
        StartCoroutine(MoveAcross(card, endX, speed));
    }

    IEnumerator MoveAcross(RectTransform card, float endX, float speed)
    {
        while (card != null && card.anchoredPosition.x < endX)
        {
            card.anchoredPosition += Vector2.right * (speed * Time.deltaTime);
            yield return null;
        }

        if (card != null)
        {
            activeMessages.Remove(card);
            Destroy(card.gameObject);
        }
    }

    void CleanupMissingMessages()
    {
        for (int i = activeMessages.Count - 1; i >= 0; i--)
        {
            if (activeMessages[i] == null)
            {
                activeMessages.RemoveAt(i);
            }
        }
    }

    void ConfigureText(TMP_Text text, float fontSize, FontStyles style, Color color, TextAlignmentOptions alignment, TextWrappingModes wrapping)
    {
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = wrapping;
        text.margin = Vector4.zero;
        text.raycastTarget = false;
    }
}
