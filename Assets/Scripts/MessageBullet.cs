using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageBullet : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform rectTransform;
    public TMP_Text messageText;
    public Image backgroundImage;

    [Header("Auto Size")]
    public float minWidth = 260f;
    public float maxWidth = 760f;
    public float height = 80f;
    public float widthPerCharacter = 16f;
    public float horizontalTextPadding = 48f;

    void Reset()
    {
        rectTransform = GetComponent<RectTransform>();
        messageText = GetComponentInChildren<TMP_Text>(true);
        backgroundImage = GetComponent<Image>();
    }

    void Awake()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (messageText == null)
        {
            messageText = GetComponentInChildren<TMP_Text>(true);
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
    }

    public void Setup(string message, Material material)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.textWrappingMode = TextWrappingModes.Normal;
            messageText.alignment = TextAlignmentOptions.Center;
        }

        if (backgroundImage != null && material != null)
        {
            backgroundImage.material = material;
        }

        ResizeToMessage(message);
    }

    public void ResizeToMessage(string message)
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        float targetWidth = minWidth;

        if (!string.IsNullOrEmpty(message))
        {
            targetWidth = Mathf.Clamp(message.Length * widthPerCharacter + horizontalTextPadding * 2f, minWidth, maxWidth);
        }

        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(targetWidth, height);
        }

        if (messageText != null)
        {
            RectTransform textRect = messageText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(horizontalTextPadding * 0.5f, 0f);
                textRect.offsetMax = new Vector2(-horizontalTextPadding * 0.5f, 0f);
            }
        }
    }
}