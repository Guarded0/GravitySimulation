using UnityEngine;

public class FullscreenToggle : MonoBehaviour
{
    public void SetFullscreen()
    {
        Screen.fullScreen = true; // Met en plein écran
        Debug.Log("Plein écran activé");
    }
}