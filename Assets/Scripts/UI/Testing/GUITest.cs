using UnityEngine;

[ExecuteAlways]
public class GUITest : MonoBehaviour
{
    public Rect windowRect = new Rect(20, 20, 250, 150);
    public GUISkin skin;

    private void OnGUI()
    {
        GUI.skin = skin;
        windowRect = GUILayout.Window(0, windowRect, DoMyWindow, "My IMGUI Window");
    }

    void DoMyWindow(int windowID)
    {

        GUILayout.Label("Simulation Speed");
        NBodySimulation.Instance.simulationSpeed = GUILayout.HorizontalSlider( NBodySimulation.Instance.simulationSpeed, 0f, 5f);

        // Allow the window to be dragged
        GUI.DragWindow(new Rect(0, 0, 10000, 20)); // usually just the title bar
    }
}
