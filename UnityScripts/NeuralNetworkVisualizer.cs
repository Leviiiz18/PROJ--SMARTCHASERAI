using UnityEngine;
using UnityEngine.UI;

public class NeuralNetworkVisualizer : MonoBehaviour
{
    [Header("Mode")]
    public bool demoMode = false;   // 🔥 TURN ON ONLY FOR DEMO

    [Header("Input Nodes")]
    public Image inputDX;
    public Image inputDZ;

    [Header("Hidden Nodes")]
    public Image[] hiddenNodes;

    [Header("Output Nodes")]
    public Image outputX;
    public Image outputZ;

    [Header("Visual Settings")]
    public float inputScale = 20f;
    public float smoothSpeed = 6f;

    float[] hiddenValues;

    void Awake()
    {
        hiddenValues = new float[hiddenNodes.Length];

        // 🔥 Hide NN UI completely during training
        gameObject.SetActive(demoMode);
    }

    public void UpdateNetwork(float dx, float dz, float outX, float outZ)
    {
        // ❌ DO NOTHING DURING TRAINING
        if (!demoMode)
            return;

        // ---------- INPUT NORMALIZATION ----------
        float ndx = Mathf.Clamp(dx / inputScale, -1f, 1f);
        float ndz = Mathf.Clamp(dz / inputScale, -1f, 1f);

        SetNode(inputDX, ndx);
        SetNode(inputDZ, ndz);

        // ---------- HIDDEN ACTIVATION (RESPONSIVE) ----------
        for (int i = 0; i < hiddenNodes.Length; i++)
        {
            float target =
                Mathf.Sin(ndx * (i + 1) * 1.3f + ndz * 0.7f) *
                Mathf.Cos(Time.time * 0.8f + i);

            hiddenValues[i] = Mathf.Lerp(
                hiddenValues[i],
                target,
                Time.deltaTime * smoothSpeed
            );

            SetNode(hiddenNodes[i], hiddenValues[i]);
        }

        // ---------- OUTPUT ----------
        SetNode(outputX, outX);
        SetNode(outputZ, outZ);
    }

    // ================= VISUAL =================

    void SetNode(Image img, float value)
    {
        float intensity = Mathf.Clamp01(Mathf.Abs(value));

        // 🔵 Positive = Cyan, 🔴 Negative = Magenta
        Color baseColor = value >= 0f ? Color.cyan : Color.magenta;

        img.color = Color.Lerp(Color.black, baseColor, intensity);

        // 🔥 Pulse scale
        float scale = Mathf.Lerp(0.9f, 1.15f, intensity);
        img.rectTransform.localScale = Vector3.one * scale;
    }
}
