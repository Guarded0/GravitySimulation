using UnityEngine;
#if UNITY_EDITOR
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
        GUILayout.Label("Simulation time");
        GUILayout.Label(NBodySimulation.Instance.averageTimeMiliseconds.ToString("F2") + " ms");
        GUILayout.Label("Orbit Draw time");
        GUILayout.Label("Mean Time: " + NBodySimulation.Instance.gameObject.GetComponent<OrbitDebugDisplay>().chronometer.GetMeanTimeMiliseconds().ToString("F2") + " ms");
        GUILayout.Label("Last Time: " + NBodySimulation.Instance.gameObject.GetComponent<OrbitDebugDisplay>().chronometer.GetElapsedTimeMiliseconds().ToString("F2") + " ms");
        // Allow the window to be dragged
        GUI.DragWindow(new Rect(0, 0, 10000, 20)); // usually just the title bar
    }
}
#endif
