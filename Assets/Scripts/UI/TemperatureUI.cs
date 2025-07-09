using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TemperatureUI : MonoBehaviour
{
    public Heat heat;
    public Slider thermometerSlider;
    public Image thermometerFill;
    public TextMeshProUGUI temperatureText;

    public float coldMin = 28f;
    public float hotMax = 43f;

    [SerializeField] private Color coldColor = new Color(0.5f, 0.8f, 1f);
    [SerializeField] private Color normalColor = Color.red;
    [SerializeField] private Color hotColor = Color.yellow;

    private void Update()
    {
        float temp = heat.bodyTemp;

        float tNorm = Mathf.InverseLerp(coldMin, hotMax, temp);
        thermometerSlider.value = Mathf.Lerp(0.21f, 1f, tNorm);

        Color currentColor;

        if (tNorm < 0.5f)
        {
            currentColor = Color.Lerp(coldColor, normalColor, tNorm * 2f);
        }
        else
        {
            currentColor = Color.Lerp(normalColor, hotColor, (tNorm - 0.5f) * 2f);
        }

        thermometerFill.color = currentColor;
        temperatureText.text = $"<mspace=0.6em>{temp:F1}°C";
    }
}
