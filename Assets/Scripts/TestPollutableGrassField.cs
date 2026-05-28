using UnityEngine;

public class TestPollutableGrassField : MonoBehaviour
{
    [Header("Pollution Result")]
    public Color healthyColor = new Color(0.22f, 0.72f, 0.18f, 1f);
    public Color stressedColor = new Color(0.12f, 0.28f, 0.10f, 1f);
    public Color pollutedColor = new Color(0.08f, 0.02f, 0.02f, 1f);
    public Color pollutionGlowColor = new Color(1f, 0.05f, 0.02f, 1f);
    public float pollutionStart = 0.34f;

    Transform[] grassBlades;
    Renderer[] grassRenderers;
    Vector3[] initialScales;
    MaterialPropertyBlock propertyBlock;

    void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        CacheGrass();
    }

    public void SetPollutionProgress(float progress)
    {
        if (grassBlades == null || grassRenderers == null || initialScales == null)
        {
            CacheGrass();
        }

        float decay = Mathf.Clamp01(progress);
        float pollution = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(pollutionStart, 1f, decay));

        for (int i = 0; i < grassBlades.Length; i++)
        {
            if (grassBlades[i] == null || grassRenderers[i] == null)
            {
                continue;
            }

            float localDelay = grassBlades.Length <= 1 ? 0f : (float)i / (grassBlades.Length - 1) * 0.22f;
            float localPollution = Mathf.Clamp01((pollution - localDelay) / Mathf.Max(0.01f, 1f - localDelay));
            Color baseColor = Color.Lerp(healthyColor, stressedColor, decay);
            Color finalColor = Color.Lerp(baseColor, pollutedColor, localPollution);
            Color glowColor = Color.Lerp(Color.black, pollutionGlowColor, Mathf.Clamp01((localPollution - 0.48f) / 0.52f));

            grassBlades[i].localScale = Vector3.Lerp(initialScales[i], new Vector3(initialScales[i].x * 0.72f, initialScales[i].y * 0.45f, initialScales[i].z * 0.72f), localPollution);
            propertyBlock.SetColor("_BaseColor", finalColor);
            propertyBlock.SetColor("_Color", finalColor);
            propertyBlock.SetColor("_EmissionColor", glowColor);
            grassRenderers[i].SetPropertyBlock(propertyBlock);
        }
    }

    void CacheGrass()
    {
        grassRenderers = GetComponentsInChildren<Renderer>(true);
        grassBlades = new Transform[grassRenderers.Length];
        initialScales = new Vector3[grassRenderers.Length];

        for (int i = 0; i < grassRenderers.Length; i++)
        {
            grassBlades[i] = grassRenderers[i].transform;
            initialScales[i] = grassBlades[i].localScale;
        }
    }
}
