using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationUIController : MonoBehaviour
{
    const float PanelMinWidth = 720f;
    const float PanelMaxWidth = 1180f;
    const float PanelMinHeight = 190f;
    const float PanelMaxHeight = 520f;
    const float HorizontalPadding = 62f;
    const float HeaderHeight = 86f;
    const float BodyTopPadding = 30f;
    const float BottomPadding = 38f;
    const float IconSize = 52f;
    const float IconTitleGap = 20f;
    const float BorderThickness = 12f;
    const float HeaderDividerThickness = 6f;

    static readonly Color MinecraftWoodColor = new Color(0.72f, 0.42f, 0.18f, 0.96f);
    static readonly Color MinecraftWoodDarkColor = new Color(0.18f, 0.09f, 0.035f, 1f);
    static readonly Color MinecraftWoodSideColor = new Color(0.48f, 0.25f, 0.10f, 1f);
    static readonly Color MinecraftWoodLineColor = new Color(0.10f, 0.055f, 0.025f, 1f);
    static readonly Color MinecraftTextColor = Color.white;

    [Header("UI References")]
    public RectTransform notificationPanel;
    public Image iconImage;
    public TMP_Text titleText;
    public TMP_Text messageText;
    public TMP_Text timeText;

    [Header("Minecraft Style")]
    public TMP_FontAsset minecraftFont;

    [Header("Icons")]
    public Sprite emailIcon;
    public Sprite gameIcon;
    public Sprite shoppingIcon;
    public Sprite defaultIcon;

    [Header("Audio")]
    public AudioClip notificationSound;
    public float notificationVolume = 0.75f;

    Image backgroundImage;
    RectTransform topBorder;
    RectTransform bottomBorder;
    RectTransform leftBorder;
    RectTransform rightBorder;
    RectTransform plankLine;
    AudioSource audioSource;

    void Awake()
    {
        EnsureAudioSource();
        LoadMinecraftFont();
        ApplyLayout();
    }

    void OnValidate()
    {
        LoadMinecraftFont();
        ApplyLayout();
    }

    public void Show(StepNode node)
    {
        if (node == null)
        {
            return;
        }

        NotificationKind kind = ResolveKind(node);
        string notificationTitle = node.GetNotificationTitleForTrigger();
        string nodeMessage = node.GetNodeMessageForTrigger();
        string title = string.IsNullOrWhiteSpace(notificationTitle)
            ? GetDefaultTitle(kind)
            : notificationTitle;

        ShowNotification(title, CleanMessage(nodeMessage), kind);
    }

    public void ShowInitial(StepNode node)
    {
        if (node == null)
        {
            return;
        }

        string nodeMessage = node.GetInitialNodeMessage();
        NotificationKind kind = ResolveKind(node, nodeMessage);
        string notificationTitle = node.GetInitialNotificationTitle();
        string title = string.IsNullOrWhiteSpace(notificationTitle)
            ? GetDefaultTitle(kind)
            : notificationTitle;

        ShowNotification(title, CleanMessage(nodeMessage), kind);
    }

    public void ShowSystemMessage(string title, string message)
    {
        ShowNotification(title, message, NotificationKind.System);
    }

    public void Hide()
    {
        if (titleText != null)
        {
            titleText.text = "";
        }

        if (messageText != null)
        {
            messageText.text = "";
        }

        if (timeText != null)
        {
            timeText.text = "";
        }

        if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        if (notificationPanel != null)
        {
            notificationPanel.gameObject.SetActive(false);
        }
    }

    void ShowNotification(string title, string message, NotificationKind kind)
    {
        ApplyLayout();

        if (notificationPanel != null)
        {
            notificationPanel.gameObject.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (messageText != null)
        {
            messageText.text = message;
        }

        if (timeText != null)
        {
            timeText.text = "";
        }

        if (iconImage != null)
        {
            iconImage.sprite = GetIcon(kind);
            iconImage.enabled = iconImage.sprite != null;
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
        }

        ApplyLayout();
        PlayNotificationSound();
    }

    NotificationKind ResolveKind(StepNode node)
    {
        return ResolveKind(node, node.GetNodeMessageForTrigger());
    }

    NotificationKind ResolveKind(StepNode node, string nodeMessage)
    {
        if (node.notificationKind != NotificationKind.Auto)
        {
            return node.notificationKind;
        }

        string source = (node.name + " " + nodeMessage).ToLowerInvariant();

        if (source.Contains("email") || source.Contains("gmail"))
        {
            return NotificationKind.Email;
        }

        if (source.Contains("shopping") || source.Contains("shop"))
        {
            return NotificationKind.Shopping;
        }

        if (source.Contains("game") || source.Contains("login") || source.Contains("checked in"))
        {
            return NotificationKind.Game;
        }

        return NotificationKind.System;
    }

    string GetDefaultTitle(NotificationKind kind)
    {
        switch (kind)
        {
            case NotificationKind.Email:
                return "Outlook";
            case NotificationKind.Game:
                return "Game Center";
            case NotificationKind.Shopping:
                return "Shopping";
            default:
                return "Notification";
        }
    }

    string CleanMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return "";
        }

        string trimmed = message.TrimStart();

        if (!trimmed.StartsWith("["))
        {
            return message;
        }

        int endOfCue = trimmed.IndexOf(']');
        if (endOfCue < 0)
        {
            return message;
        }

        return trimmed.Substring(endOfCue + 1).TrimStart('\r', '\n', ' ');
    }

    Sprite GetIcon(NotificationKind kind)
    {
        switch (kind)
        {
            case NotificationKind.Email:
                return emailIcon != null ? emailIcon : defaultIcon;
            case NotificationKind.Game:
                return gameIcon != null ? gameIcon : defaultIcon;
            case NotificationKind.Shopping:
                return shoppingIcon != null ? shoppingIcon : defaultIcon;
            default:
                return defaultIcon;
        }
    }

    void EnsureAudioSource()
    {
        if (audioSource != null)
        {
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (notificationSound == null)
        {
            notificationSound = Resources.Load<AudioClip>("Audio/iOSLikeNotification");
        }
    }

    void PlayNotificationSound()
    {
        EnsureAudioSource();

        if (audioSource != null && notificationSound != null)
        {
            audioSource.PlayOneShot(notificationSound, notificationVolume);
        }
    }

    void ApplyLayout()
    {
        LoadMinecraftFont();
        EnsureWoodFrame();
        ConfigureText(titleText, TextAlignmentOptions.MidlineLeft, TextWrappingModes.NoWrap, 44f, 0f);
        ConfigureText(messageText, TextAlignmentOptions.TopLeft, TextWrappingModes.Normal, 46f, -2f);
        ConfigureText(timeText, TextAlignmentOptions.Center, TextWrappingModes.NoWrap, 1f, 0f);

        Vector2 panelSize = CalculatePanelSize();

        if (notificationPanel != null)
        {
            notificationPanel.anchorMin = new Vector2(0.5f, 0.5f);
            notificationPanel.anchorMax = new Vector2(0.5f, 0.5f);
            notificationPanel.pivot = new Vector2(0.5f, 0.5f);
            notificationPanel.anchoredPosition = Vector2.zero;
            notificationPanel.sizeDelta = panelSize;

            if (backgroundImage != null)
            {
                backgroundImage.color = MinecraftWoodColor;
                backgroundImage.raycastTarget = false;
            }
        }

        ApplyWoodFrameLayout();

        RectTransform iconRect = iconImage != null ? iconImage.rectTransform : null;
        if (iconRect != null)
        {
            iconImage.enabled = iconImage.sprite != null;
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
            iconRect.anchorMin = new Vector2(0f, 1f);
            iconRect.anchorMax = new Vector2(0f, 1f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(HorizontalPadding, -HeaderHeight * 0.5f);
            iconRect.sizeDelta = new Vector2(IconSize, IconSize);
        }

        RectTransform timeRect = timeText != null ? timeText.rectTransform : null;
        if (timeRect != null)
        {
            timeText.text = "";
            timeRect.anchorMin = new Vector2(0.5f, 0.5f);
            timeRect.anchorMax = new Vector2(0.5f, 0.5f);
            timeRect.pivot = new Vector2(0.5f, 0.5f);
            timeRect.anchoredPosition = Vector2.zero;
            timeRect.sizeDelta = Vector2.zero;
        }

        RectTransform titleRect = titleText != null ? titleText.rectTransform : null;
        if (titleRect != null)
        {
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2((IconSize + IconTitleGap) * 0.5f, -HeaderHeight * 0.5f);
            titleRect.sizeDelta = new Vector2(-(HorizontalPadding * 2f + IconSize + IconTitleGap), HeaderHeight);
        }

        RectTransform messageRect = messageText != null ? messageText.rectTransform : null;
        if (messageRect != null)
        {
            messageRect.anchorMin = new Vector2(0f, 1f);
            messageRect.anchorMax = new Vector2(1f, 1f);
            messageRect.pivot = new Vector2(0.5f, 1f);
            messageRect.anchoredPosition = new Vector2(0f, -(HeaderHeight + BodyTopPadding));
            messageRect.sizeDelta = new Vector2(-HorizontalPadding * 2f, panelSize.y - HeaderHeight - BodyTopPadding - BottomPadding);
        }

    }

    void ConfigureText(TMP_Text text, TextAlignmentOptions alignment, TextWrappingModes wrapping, float fontSize, float lineSpacing)
    {
        if (text == null)
        {
            return;
        }

        text.alignment = alignment;
        text.textWrappingMode = wrapping;
        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(18f, fontSize - 10f);
        text.fontSizeMax = fontSize;
        text.lineSpacing = lineSpacing;
        text.color = MinecraftTextColor;
        text.fontStyle = FontStyles.Normal;
        text.outlineWidth = 0.14f;
        text.outlineColor = Color.black;
        if (minecraftFont != null)
        {
            text.font = minecraftFont;
            text.fontSharedMaterial = minecraftFont.material;
        }
        text.margin = Vector4.zero;
        text.raycastTarget = false;
    }

    void LoadMinecraftFont()
    {
        if (minecraftFont == null)
        {
            minecraftFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }
    }

    Vector2 CalculatePanelSize()
    {
        float maxTextWidth = PanelMaxWidth - HorizontalPadding * 2f;
        float headerTextOffset = IconSize + IconTitleGap;
        float preferredWidth = PanelMinWidth - HorizontalPadding * 2f;
        float messageHeight = 42f;

        if (titleText != null)
        {
            titleText.ForceMeshUpdate();
            preferredWidth = Mathf.Max(preferredWidth, titleText.GetPreferredValues(titleText.text, 0f, 0f).x + headerTextOffset);
        }

        if (messageText != null)
        {
            messageText.ForceMeshUpdate();
            preferredWidth = Mathf.Max(preferredWidth, Mathf.Min(messageText.GetPreferredValues(messageText.text, 0f, 0f).x, maxTextWidth));
        }

        float width = Mathf.Clamp(preferredWidth + HorizontalPadding * 2f, PanelMinWidth, PanelMaxWidth);
        if (messageText != null)
        {
            messageHeight = Mathf.Max(messageHeight, messageText.GetPreferredValues(messageText.text, width - HorizontalPadding * 2f, 0f).y);
        }

        float height = HeaderHeight + BodyTopPadding + BottomPadding + messageHeight;
        return new Vector2(width, Mathf.Clamp(height, PanelMinHeight, PanelMaxHeight));
    }

    void EnsureWoodFrame()
    {
        if (notificationPanel == null)
        {
            return;
        }

        backgroundImage = notificationPanel.GetComponent<Image>();
        topBorder = EnsurePanelStripe("MC Top Edge", MinecraftWoodDarkColor);
        bottomBorder = EnsurePanelStripe("MC Bottom Edge", MinecraftWoodDarkColor);
        leftBorder = EnsurePanelStripe("MC Left Edge", MinecraftWoodSideColor);
        rightBorder = EnsurePanelStripe("MC Right Edge", MinecraftWoodSideColor);
        plankLine = EnsurePanelStripe("MC Plank Line", MinecraftWoodLineColor);
    }

    RectTransform EnsurePanelStripe(string childName, Color color)
    {
        Transform existing = notificationPanel.Find(childName);
        GameObject stripeObject = existing != null ? existing.gameObject : new GameObject(childName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform stripeRect = stripeObject.GetComponent<RectTransform>();
        Image stripeImage = stripeObject.GetComponent<Image>();

        stripeObject.transform.SetParent(notificationPanel, false);
        stripeObject.transform.SetAsFirstSibling();
        stripeImage.color = color;
        stripeImage.raycastTarget = false;

        return stripeRect;
    }

    void ApplyWoodFrameLayout()
    {
        SetEdge(topBorder, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, BorderThickness));
        SetEdge(bottomBorder, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, BorderThickness));
        SetEdge(leftBorder, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(BorderThickness, 0f));
        SetEdge(rightBorder, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(BorderThickness, 0f));
        SetEdge(plankLine, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -HeaderHeight), new Vector2(0f, HeaderDividerThickness));
    }

    void SetEdge(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }
}
