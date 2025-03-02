using UnityEngine;
using UnityEngine.UI;

public class SineDebugUI : MonoBehaviour
{
    private Text debugText;
    private SineController controller;

    void Start()
    {
        // Create UI Text element
        var canvas = new GameObject("DebugCanvas", typeof(Canvas));
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        
        var textObj = new GameObject("DebugText", typeof(Text));
        textObj.transform.SetParent(canvas.transform);
        debugText = textObj.GetComponent<Text>();
        debugText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        debugText.fontSize = 20;
        
        controller = FindObjectOfType<SineController>();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Generation Time: {controller.evaluationTimer:F1}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Population Size: {controller.populationSize}");
    }
}
