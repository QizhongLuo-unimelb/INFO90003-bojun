using UnityEngine;

public class TestLeafAtrophy : MonoBehaviour
{
    public Transform leavesContainer;
    [Range(0f, 1f)] public float decayProgress;
    public Color healthyLeafColor = new Color(0.16f, 0.78f, 0.28f, 1f);
    public Color pollutedLeafColor = new Color(0.015f, 0.018f, 0.015f, 1f);
    public Color pollutionGlowColor = new Color(1f, 0.05f, 0.03f, 1f);
    public float minimumLeafScale = 0.08f;

    Transform[] leafNodes;
    Vector3[] initialScales;
    Renderer[] leafRenderers;
    MaterialPropertyBlock propertyBlock;

    void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        CacheLeaves();
    }

    void OnValidate()
    {
        minimumLeafScale = Mathf.Clamp(minimumLeafScale, 0f, 1f);
    }

    void Update()
    {
        ApplyDecay();
    }

    public void SetDecayProgress(float progress)
    {
        decayProgress = Mathf.Clamp01(progress);
        ApplyDecay();
    }

    void CacheLeaves()
    {
        if (leavesContainer == null)
        {
            leafNodes = new Transform[0];
            initialScales = new Vector3[0];
            leafRenderers = new Renderer[0];
            return;
        }

        int childCount = leavesContainer.childCount;
        leafNodes = new Transform[childCount];
        initialScales = new Vector3[childCount];
        leafRenderers = new Renderer[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform leaf = leavesContainer.GetChild(i);
            leafNodes[i] = leaf;
            initialScales[i] = leaf.localScale;
            leafRenderers[i] = leaf.GetComponent<Renderer>();
        }
    }

    void ApplyDecay()
    {
        if (leafNodes == null || initialScales == null)
        {
            return;
        }

        for (int i = 0; i < leafNodes.Length; i++)
        {
            if (leafNodes[i] == null)
            {
                continue;
            }

            float threshold = leafNodes.Length <= 1 ? 0f : (float)i / (leafNodes.Length - 1);
            float localProgress = Mathf.InverseLerp(threshold, 1f, decayProgress);
            float easedProgress = Mathf.SmoothStep(0f, 1f, localProgress);
            leafNodes[i].localScale = Vector3.Lerp(initialScales[i], initialScales[i] * minimumLeafScale, easedProgress);

            if (leafRenderers[i] != null)
            {
                Color leafColor = Color.Lerp(healthyLeafColor, pollutedLeafColor, easedProgress);
                Color glowColor = Color.Lerp(Color.black, pollutionGlowColor, Mathf.Clamp01((easedProgress - 0.55f) / 0.45f));
                propertyBlock.SetColor("_BaseColor", leafColor);
                propertyBlock.SetColor("_Color", leafColor);
                propertyBlock.SetColor("_EmissionColor", glowColor);
                leafRenderers[i].SetPropertyBlock(propertyBlock);
            }
        }
    }
}
