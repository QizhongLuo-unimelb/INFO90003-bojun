using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndSceneMinecraftUI : MonoBehaviour
{
    const float SummaryHorizontalPadding = 88f;
    const float SummaryVerticalPadding = 56f;
    const float ScrollStartDelay = 0.45f;
    const float ScrollEndHold = 0.65f;

    public TMP_Text summaryText;
    public TMP_Text timerText;
    public TMP_Text focusScoreText;
    public ScrollRect summaryScrollRect;

    public void SetSummary(string value)
    {
        EnsureFocusScoreDisplay();
        SetFocusScore(GameRunState.CalculateFocusScore().Score);

        if (summaryText != null)
        {
            summaryText.text = value;
            FitSummaryContentToText(value);
        }

        if (summaryScrollRect != null)
        {
            summaryScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    void FitSummaryContentToText(string value)
    {
        if (summaryScrollRect == null || summaryScrollRect.content == null || summaryText == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        RectTransform viewportRect = summaryScrollRect.viewport != null
            ? summaryScrollRect.viewport
            : summaryScrollRect.GetComponent<RectTransform>();
        RectTransform contentRect = summaryScrollRect.content;
        RectTransform textRect = summaryText.rectTransform;

        float viewportWidth = viewportRect != null ? viewportRect.rect.width : 830f;
        float viewportHeight = viewportRect != null ? viewportRect.rect.height : 520f;
        float textWidth = Mathf.Max(1f, viewportWidth - SummaryHorizontalPadding);
        float preferredHeight = summaryText.GetPreferredValues(value, textWidth, 0f).y;
        float contentHeight = Mathf.Max(viewportHeight, preferredHeight + SummaryVerticalPadding);

        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, contentHeight);

        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = new Vector2(0f, -SummaryVerticalPadding * 0.5f);
        textRect.sizeDelta = new Vector2(-SummaryHorizontalPadding, preferredHeight);

        summaryText.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();
    }

    void EnsureFocusScoreDisplay()
    {
        if (focusScoreText != null)
        {
            return;
        }

        RectTransform parent = transform as RectTransform;
        if (parent == null)
        {
            return;
        }

        GameObject panelObject = new GameObject("Focus Score Transistor Display", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(parent, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -18f);
        panelRect.sizeDelta = new Vector2(620f, 132f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.015f, 0.025f, 0.022f, 0.94f);
        panelImage.raycastTarget = false;

        AddDisplayStripe(panelRect, "Display Top Glow", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -3f), new Vector2(0f, 6f), new Color(0.25f, 1f, 0.66f, 0.72f));
        AddDisplayStripe(panelRect, "Display Bottom Glow", Vector2.zero, new Vector2(1f, 0f), new Vector2(0f, 3f), new Vector2(0f, 5f), new Color(0.1f, 0.55f, 0.36f, 0.55f));
        AddDisplayStripe(panelRect, "Display Left Edge", Vector2.zero, new Vector2(0f, 1f), new Vector2(3f, 0f), new Vector2(5f, 0f), new Color(0.1f, 0.8f, 0.52f, 0.6f));
        AddDisplayStripe(panelRect, "Display Right Edge", new Vector2(1f, 0f), Vector2.one, new Vector2(-3f, 0f), new Vector2(5f, 0f), new Color(0.1f, 0.8f, 0.52f, 0.6f));

        TextMeshProUGUI labelText = AddDisplayText(panelRect, "Focus Score Label", "FOCUS SCORE", 24f, new Color(0.58f, 1f, 0.78f, 0.88f));
        labelText.rectTransform.anchorMin = new Vector2(0f, 1f);
        labelText.rectTransform.anchorMax = new Vector2(1f, 1f);
        labelText.rectTransform.pivot = new Vector2(0.5f, 1f);
        labelText.rectTransform.anchoredPosition = new Vector2(0f, -12f);
        labelText.rectTransform.sizeDelta = new Vector2(-40f, 34f);
        labelText.characterSpacing = 10f;

        focusScoreText = AddDisplayText(panelRect, "Focus Score Digits", "9999", 78f, new Color(0.34f, 1f, 0.58f, 1f));
        focusScoreText.rectTransform.anchorMin = Vector2.zero;
        focusScoreText.rectTransform.anchorMax = Vector2.one;
        focusScoreText.rectTransform.offsetMin = new Vector2(28f, 8f);
        focusScoreText.rectTransform.offsetMax = new Vector2(-28f, -38f);
        focusScoreText.characterSpacing = 18f;
        focusScoreText.fontStyle = FontStyles.Bold;
    }

    void SetFocusScore(int score)
    {
        if (focusScoreText != null)
        {
            focusScoreText.text = Mathf.Clamp(score, 0, 9999).ToString("0000");
        }
    }

    static TextMeshProUGUI AddDisplayText(Transform parent, string objectName, string value, float fontSize, Color color)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;

        return text;
    }

    static void AddDisplayStripe(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
    {
        GameObject stripeObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        stripeObject.transform.SetParent(parent, false);

        RectTransform stripeRect = stripeObject.GetComponent<RectTransform>();
        stripeRect.anchorMin = anchorMin;
        stripeRect.anchorMax = anchorMax;
        stripeRect.pivot = new Vector2(0.5f, 0.5f);
        stripeRect.anchoredPosition = anchoredPosition;
        stripeRect.sizeDelta = sizeDelta;

        Image stripeImage = stripeObject.GetComponent<Image>();
        stripeImage.color = color;
        stripeImage.raycastTarget = false;
    }

    public void UpdateDisplay(float elapsedSeconds, float totalSeconds)
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(Mathf.Max(0f, totalSeconds - elapsedSeconds)).ToString("00") + "s";
        }

        if (summaryScrollRect == null)
        {
            return;
        }

        float scrollDuration = Mathf.Max(1f, totalSeconds - ScrollStartDelay - ScrollEndHold);
        float progress = Mathf.Clamp01((elapsedSeconds - ScrollStartDelay) / scrollDuration);
        summaryScrollRect.verticalNormalizedPosition = Mathf.Lerp(1f, 0f, progress);
    }
}
