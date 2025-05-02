using UnityEngine;
using UnityEngine.UI;
public class GridToggleButton : MonoBehaviour
{
    bool showGrid = false;
    public GameObject grid; // Assign the grid prefab in the inspector
    public GameObject enabledImage;
    public GameObject disabledImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        ToggleButton();
    }
    void ToggleButton()
    {
        showGrid = !showGrid;
        grid.GetComponent<MeshRenderer>().enabled = showGrid;
        enabledImage.SetActive(showGrid);
        disabledImage.SetActive(!showGrid);
    }
}
