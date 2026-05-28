using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageBulletSpawner : MonoBehaviour
{
    [Header("Canvas Prefab Settings")]
    public RectTransform messagePrefab;
    public RectTransform canvasParent;

    [Header("Custom Messages")]
    [TextArea(2, 4)]
    public string[] messages =
    {
        "Assignment deadline updated",
        "New Canvas announcement",
        "Your score has been released",
        "Group member sent 12 messages",
        "Meeting starts in 5 minutes",
        "Unread email from tutor",
        "System notification",
        "Low battery",
        "New discussion reply",
        "Reminder: submit reflection",
        "Course update available",
        "Booking confirmed",
        "Payment required",
        "New feedback received",
        "You have 39 unread messages",
        "Calendar conflict detected",
        "Warning: task overdue",
        "New Teams message",
        "Instagram notification",
        "Amazon delivery update",
        "Grade changed",
        "Lecture recording available",
        "Quiz closes tonight",
        "Urgent: action required"
    };

    [Header("Random Materials")]
    public Material[] prefabMaterials;

    [Header("Canvas Spawn Settings")]
    public float spawnInterval = 0.12f;
    public float verticalRange = 260f;
    public float horizontalPadding = 260f;

    [Header("Canvas Movement")]
    public float minSpeed = 500f;
    public float maxSpeed = 900f;
    public float lifeTime = 8f;

    [Header("Auto Size")]
    public float minWidth = 260f;
    public float maxWidth = 760f;
    public float height = 80f;
    public float widthPerCharacter = 16f;
    public float horizontalTextPadding = 48f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnMessage();
            timer = 0f;
        }
    }

    void SpawnMessage()
    {
        if (messagePrefab == null || canvasParent == null) return;

        RectTransform obj = Instantiate(messagePrefab, canvasParent);
        obj.gameObject.SetActive(true);

        string randomMessage = "New notification";
        if (messages != null && messages.Length > 0)
        {
            randomMessage = messages[Random.Range(0, messages.Length)];
        }

        TMP_Text text = obj.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text = randomMessage;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.alignment = TextAlignmentOptions.Center;
        }

        ResizeMessageObject(obj, text, randomMessage);
        ApplyRandomMaterial(obj.gameObject);

        float canvasWidth = canvasParent.rect.width;
        float startX = -canvasWidth * 0.5f - horizontalPadding;
        float endX = canvasWidth * 0.5f + horizontalPadding;
        float randomY = Random.Range(-verticalRange, verticalRange);

        obj.anchorMin = new Vector2(0.5f, 0.5f);
        obj.anchorMax = new Vector2(0.5f, 0.5f);
        obj.pivot = new Vector2(0.5f, 0.5f);
        obj.anchoredPosition = new Vector2(startX, randomY);
        obj.localRotation = Quaternion.identity;
        obj.localScale = Vector3.one;

        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        StartCoroutine(MoveMessageAcrossScreen(obj, startX, endX, randomY, randomSpeed));
    }

    IEnumerator MoveMessageAcrossScreen(RectTransform obj, float startX, float endX, float y, float speed)
    {
        float timerInside = 0f;
        float distance = Mathf.Abs(endX - startX);
        float duration = distance / Mathf.Max(speed, 1f);

        while (obj != null && timerInside < duration && timerInside < lifeTime)
        {
            timerInside += Time.deltaTime;
            float t = Mathf.Clamp01(timerInside / duration);
            float x = Mathf.Lerp(startX, endX, t);
            obj.anchoredPosition = new Vector2(x, y);
            yield return null;
        }

        if (obj != null)
        {
            Destroy(obj.gameObject);
        }
    }

    void ResizeMessageObject(RectTransform obj, TMP_Text text, string message)
    {
        float targetWidth = minWidth;

        if (!string.IsNullOrEmpty(message))
        {
            targetWidth = Mathf.Clamp(message.Length * widthPerCharacter + horizontalTextPadding * 2f, minWidth, maxWidth);
        }

        obj.sizeDelta = new Vector2(targetWidth, height);

        if (text != null)
        {
            RectTransform textRect = text.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(horizontalTextPadding * 0.5f, 0f);
                textRect.offsetMax = new Vector2(-horizontalTextPadding * 0.5f, 0f);
            }
        }
    }

    void ApplyRandomMaterial(GameObject obj)
    {
        if (prefabMaterials == null || prefabMaterials.Length == 0) return;

        Material randomMaterial = prefabMaterials[Random.Range(0, prefabMaterials.Length)];
        if (randomMaterial == null) return;

        Image[] images = obj.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            image.material = randomMaterial;
        }
    }
}