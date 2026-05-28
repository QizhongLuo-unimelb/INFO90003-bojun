using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShoppingInformationDistraction : MonoBehaviour
{
    [Header("Spawn Area")]
    public Vector2 xRange = new Vector2(-10.5f, 10.5f);
    public float northBankZ = 3.85f;
    public float southBankZ = -3.85f;
    public float shoreSurfaceY = 0.5f;
    public float bottomClearance = 0.12f;

    [Header("Refresh")]
    public float spawnInterval = 2.4f;
    public int maxVisibleMessages = 4;
    public float messageLifetime = 5.8f;
    public float minimumSpacing = 4.5f;

    [Header("Display")]
    public Vector2 popupSize = new Vector2(360f, 210f);
    public float worldScale = 0.0065f;
    public float popInDuration = 0.22f;
    public float popOutDuration = 0.28f;
    public float floatAmplitude = 0.045f;
    public Color cardColor = new Color(1f, 0.985f, 0.94f, 1f);
    public Color headerColor = new Color(0.93f, 0.07f, 0.12f, 1f);
    public Color discountColor = new Color(1f, 0.78f, 0.03f, 1f);
    public Color textColor = new Color(0.06f, 0.06f, 0.07f, 1f);
    public Color mutedTextColor = new Color(0.43f, 0.43f, 0.45f, 1f);
    public Color buttonColor = new Color(0.03f, 0.55f, 0.28f, 1f);
    public Color shadowColor = new Color(0f, 0f, 0f, 0.3f);

    [Header("Shopping Copy")]
    public string[] sellers =
    {
        "PixelMart",
        "BlockBay",
        "CartCraft",
        "MineMall",
        "FlashBox",
        "DealDock"
    };

    public string[] products =
    {
        "Noise-Cancel Headset",
        "Cherry Keyboard",
        "Smart Watch",
        "Gaming Mouse",
        "USB-C Hub",
        "Cloud Sneakers",
        "Energy Drink Pack",
        "Mystery Loot Box"
    };

    public string[] discounts =
    {
        "44% OFF",
        "35% OFF",
        "50% OFF",
        "$15 OFF",
        "DEAL",
        "FLASH SALE"
    };

    readonly Queue<GameObject> spawnedMessages = new Queue<GameObject>();
    readonly Dictionary<string, Texture2D> productTextures = new Dictionary<string, Texture2D>();
    Sprite whiteSprite;
    float spawnTimer;
    int nextBankIndex;

    public int SpawnedCount { get; private set; }

    void Awake()
    {
        whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), Vector2.one * 0.5f);
    }

    void Start()
    {
        for (int i = 0; i < Mathf.Min(2, maxVisibleMessages); i++)
        {
            SpawnMessage();
        }
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnMessage();
        }

        RemoveExpiredQueueHeads();
    }

    void SpawnMessage()
    {
        float z = GetNextBankZ();
        Vector3 position = FindSpawnPosition(z);
        GameObject popup = new GameObject("Shopping_UI_Popup");
        popup.transform.SetParent(transform, false);
        popup.transform.position = position;
        popup.transform.localScale = Vector3.zero;

        string seller = Pick(sellers);
        string product = Pick(products);
        string discount = Pick(discounts);
        int dollars = Random.Range(18, 129);
        int cents = Random.Range(0, 99);
        float originalPrice = Mathf.Round((dollars * Random.Range(1.35f, 2.1f)) * 100f) / 100f;

        BuildPopupUi(popup.transform, seller, product, discount, dollars, cents, originalPrice);
        SpawnedCount++;

        ShoppingPopupMotion motion = popup.AddComponent<ShoppingPopupMotion>();
        motion.Lifetime = messageLifetime;
        motion.PopInDuration = popInDuration;
        motion.PopOutDuration = popOutDuration;
        motion.FloatAmplitude = floatAmplitude;

        spawnedMessages.Enqueue(popup);
        while (spawnedMessages.Count > maxVisibleMessages)
        {
            GameObject oldMessage = spawnedMessages.Dequeue();
            if (oldMessage != null)
            {
                Destroy(oldMessage);
            }
        }
    }

    void BuildPopupUi(Transform parent, string seller, string product, string discount, int dollars, int cents, float originalPrice)
    {
        GameObject canvasObject = new GameObject("PopupCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(parent, false);
        canvasObject.transform.localScale = Vector3.one * worldScale;

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 50;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = popupSize;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        RectTransform shadow = AddPanel(canvasRect, "Shadow", shadowColor, new Vector2(8f, -9f), popupSize);
        shadow.SetAsFirstSibling();

        RectTransform card = AddPanel(canvasRect, "Card", cardColor, Vector2.zero, popupSize);
        card.gameObject.AddComponent<RectMask2D>();
        AddPanel(card, "Header", headerColor, new Vector2(0f, 82f), new Vector2(popupSize.x, 42f));
        AddText(card, "Sponsored", "Sponsored - " + seller, new Vector2(-62f, 82f), new Vector2(218f, 32f), 17f, Color.white, TextAlignmentOptions.Left);
        AddText(card, "Close", "x", new Vector2(160f, 82f), new Vector2(28f, 28f), 24f, Color.white, TextAlignmentOptions.Center);

        RectTransform productTile = AddPanel(card, "ProductTile", new Color(0.96f, 0.93f, 0.86f, 1f), new Vector2(-106f, -18f), new Vector2(108f, 112f));
        productTile.gameObject.AddComponent<RectMask2D>();
        BuildProductIcon(productTile, product);

        AddText(card, "ProductName", Shorten(product, 22), new Vector2(58f, 44f), new Vector2(166f, 32f), 21f, textColor, TextAlignmentOptions.Left);
        AddPanel(card, "DiscountBadge", discountColor, new Vector2(13f, 8f), new Vector2(98f, 28f));
        AddText(card, "DiscountText", discount, new Vector2(13f, 8f), new Vector2(92f, 24f), 15f, textColor, TextAlignmentOptions.Center);
        AddText(card, "Countdown", "Ends in " + Random.Range(1, 8).ToString("00") + ":" + Random.Range(0, 60).ToString("00"), new Vector2(108f, 8f), new Vector2(104f, 24f), 14f, headerColor, TextAlignmentOptions.Left);

        AddText(card, "Price", "$" + dollars + "." + cents.ToString("00"), new Vector2(24f, -42f), new Vector2(112f, 34f), 29f, headerColor, TextAlignmentOptions.Left);
        AddText(card, "OriginalPrice", "$" + originalPrice.ToString("0.00"), new Vector2(111f, -39f), new Vector2(68f, 24f), 15f, mutedTextColor, TextAlignmentOptions.Left);
        AddPanel(card, "PriceStrike", mutedTextColor, new Vector2(109f, -39f), new Vector2(52f, 3f));

        AddPanel(card, "ShopNowButton", buttonColor, new Vector2(83f, -78f), new Vector2(116f, 30f));
        AddText(card, "ShopNowText", "SHOP NOW", new Vector2(83f, -78f), new Vector2(108f, 26f), 16f, Color.white, TextAlignmentOptions.Center);
    }

    void BuildProductIcon(RectTransform parent, string product)
    {
        Texture2D productTexture = GetProductTexture(product);
        if (productTexture != null)
        {
            AddProductPhoto(parent, productTexture);
            return;
        }

        Color main = new Color(1f, 0.67f, 0.08f, 1f);
        Color dark = new Color(0.12f, 0.16f, 0.24f, 1f);
        Color light = new Color(1f, 1f, 1f, 1f);

        if (product.Contains("Watch"))
        {
            AddPanel(parent, "Band", dark, Vector2.zero, new Vector2(24f, 84f));
            AddPanel(parent, "Face", main, Vector2.zero, new Vector2(58f, 58f));
            AddPanel(parent, "Screen", light, Vector2.zero, new Vector2(38f, 32f));
            return;
        }

        if (product.Contains("Mouse"))
        {
            AddPanel(parent, "MouseBody", light, new Vector2(0f, 0f), new Vector2(70f, 86f));
            AddPanel(parent, "MouseStripe", dark, new Vector2(0f, 15f), new Vector2(5f, 38f));
            AddPanel(parent, "MouseWheel", main, new Vector2(0f, 35f), new Vector2(12f, 12f));
            return;
        }

        if (product.Contains("Sneakers"))
        {
            AddPanel(parent, "Sole", dark, new Vector2(0f, -24f), new Vector2(82f, 18f));
            AddPanel(parent, "Upper", main, new Vector2(-8f, -5f), new Vector2(58f, 42f));
            AddPanel(parent, "Lace", light, new Vector2(8f, 2f), new Vector2(26f, 5f));
            return;
        }

        if (product.Contains("Keyboard"))
        {
            AddPanel(parent, "KeyboardBase", dark, Vector2.zero, new Vector2(86f, 48f));
            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    AddPanel(parent, "Key", light, new Vector2(-32f + column * 16f, 13f - row * 13f), new Vector2(10f, 7f));
                }
            }
            return;
        }

        AddPanel(parent, "Box", main, Vector2.zero, new Vector2(66f, 66f));
        AddPanel(parent, "TapeVertical", dark, Vector2.zero, new Vector2(12f, 72f));
        AddPanel(parent, "TapeHorizontal", dark, Vector2.zero, new Vector2(72f, 12f));
        AddPanel(parent, "Label", light, new Vector2(18f, -18f), new Vector2(26f, 16f));
    }

    void AddProductPhoto(RectTransform parent, Texture2D texture)
    {
        GameObject photoObject = new GameObject("ProductPhoto", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        RectTransform rect = photoObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(98f, 102f);

        RawImage image = photoObject.GetComponent<RawImage>();
        image.texture = texture;
        image.color = Color.white;
        image.raycastTarget = false;

        AspectRatioFitter fitter = photoObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = Mathf.Max(0.01f, (float)texture.width / texture.height);
    }

    Texture2D GetProductTexture(string product)
    {
        string resourceName = GetProductResourceName(product);
        if (productTextures.TryGetValue(resourceName, out Texture2D cachedTexture))
        {
            return cachedTexture;
        }

        Texture2D texture = Resources.Load<Texture2D>("ShoppingAds/" + resourceName);
        productTextures[resourceName] = texture;
        return texture;
    }

    string GetProductResourceName(string product)
    {
        if (product.Contains("Headset"))
        {
            return "headphones";
        }

        if (product.Contains("Keyboard"))
        {
            return "keyboard";
        }

        if (product.Contains("Watch"))
        {
            return "smart_watch";
        }

        if (product.Contains("Mouse"))
        {
            return "mouse";
        }

        if (product.Contains("Hub"))
        {
            return "usb_c_hub";
        }

        if (product.Contains("Sneakers"))
        {
            return "sneakers";
        }

        if (product.Contains("Energy"))
        {
            return "energy_drink";
        }

        return "box";
    }

    RectTransform AddPanel(RectTransform parent, string name, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = panel.GetComponent<Image>();
        image.sprite = whiteSprite;
        image.color = color;
        image.raycastTarget = false;

        return rect;
    }

    TextMeshProUGUI AddText(RectTransform parent, string name, string text, Vector2 anchoredPosition, Vector2 size, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = alignment;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = false;

        return label;
    }

    float GetNextBankZ()
    {
        nextBankIndex++;
        return nextBankIndex % 2 == 0 ? northBankZ : southBankZ;
    }

    Vector3 FindSpawnPosition(float z)
    {
        for (int i = 0; i < 16; i++)
        {
            Vector3 candidate = new Vector3(Random.Range(xRange.x, xRange.y), GetPopupCenterY(), z);
            if (HasEnoughSpacing(candidate))
            {
                return candidate;
            }
        }

        return new Vector3(Random.Range(xRange.x, xRange.y), GetPopupCenterY(), z);
    }

    float GetPopupCenterY()
    {
        float inheritedScaleY = Mathf.Max(0.01f, Mathf.Abs(transform.lossyScale.y));
        float popupWorldHeight = popupSize.y * worldScale * inheritedScaleY;
        return shoreSurfaceY + (popupWorldHeight * 0.5f) + bottomClearance;
    }

    bool HasEnoughSpacing(Vector3 candidate)
    {
        foreach (GameObject message in spawnedMessages)
        {
            if (message == null)
            {
                continue;
            }

            if (Vector3.Distance(candidate, message.transform.position) < minimumSpacing)
            {
                return false;
            }
        }

        return true;
    }

    void RemoveExpiredQueueHeads()
    {
        while (spawnedMessages.Count > 0 && spawnedMessages.Peek() == null)
        {
            spawnedMessages.Dequeue();
        }
    }

    string Shorten(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value.Substring(0, maxLength - 3) + "...";
    }

    string Pick(string[] values)
    {
        if (values == null || values.Length == 0)
        {
            return "";
        }

        return values[Random.Range(0, values.Length)];
    }
}

public class ShoppingPopupMotion : MonoBehaviour
{
    public float Lifetime = 5.8f;
    public float PopInDuration = 0.22f;
    public float PopOutDuration = 0.28f;
    public float FloatAmplitude = 0.045f;

    Vector3 startPosition;
    float age;
    float phase;

    void Awake()
    {
        startPosition = transform.position;
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        age += Time.deltaTime;
        FaceCamera();

        transform.localScale = Vector3.one * GetPopupScale();
        transform.position = startPosition + Vector3.up * (Mathf.Sin((Time.time * 2.3f) + phase) * FloatAmplitude);

        if (age >= Lifetime)
        {
            Destroy(gameObject);
        }
    }

    float GetPopupScale()
    {
        if (age < PopInDuration)
        {
            float t = Mathf.Clamp01(age / Mathf.Max(0.01f, PopInDuration));
            return Mathf.LerpUnclamped(0f, 1f, EaseOutBack(t));
        }

        float remaining = Lifetime - age;
        if (remaining < PopOutDuration)
        {
            float t = Mathf.Clamp01(remaining / Mathf.Max(0.01f, PopOutDuration));
            return Mathf.SmoothStep(0f, 1f, t);
        }

        return 1f;
    }

    float EaseOutBack(float t)
    {
        const float overshoot = 1.55f;
        t -= 1f;
        return 1f + ((overshoot + 1f) * t + overshoot) * t * t;
    }

    void FaceCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        Vector3 direction = transform.position - camera.transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction.normalized);
        }
    }
}
