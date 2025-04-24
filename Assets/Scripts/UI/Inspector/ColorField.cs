using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.Events;
public class ColorField : MonoBehaviour
{
    public GameObject colorPickerPrefab;
    private Canvas canvas;
    private Image colorImage;
    private Button button;

    public UnityEvent<Color> onColorChanged = new UnityEvent<Color>();
    private void Awake()
    {
        colorImage = GetComponent<Image>();
        button = GetComponent<Button>();
        canvas = transform.root.GetComponent<Canvas>();
        button.onClick.AddListener(OnButtonClick);
    }
    void OnButtonClick()
    {
        StartCoroutine(PickColor());
    }
    IEnumerator PickColor()
    {
        Color oldColor = colorImage.color;
        GameObject colorPicker = Instantiate(colorPickerPrefab, canvas.transform);
        colorPicker.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        FlexibleColorPicker colorPickerScript = colorPicker.GetComponent<FlexibleColorPicker>();
        colorPickerScript.SetColor(colorImage.color);

        bool colorPicked = false;
        Action action = () => colorPicked = true;
        Action actionCancel = () =>
        {
            colorPicked = false;
            colorPickerScript.SetColor(colorImage.color);
        };
        colorPicker.transform.Find("Accept").GetComponent<Button>().onClick.AddListener(action.Invoke);
        colorPicker.transform.Find("Cancel").GetComponent<Button>().onClick.AddListener(actionCancel.Invoke);
        yield return new WaitUntil(()=>colorPicked);
        colorPicker.transform.Find("Accept").GetComponent<Button>().onClick.RemoveListener(action.Invoke);
        colorPicker.transform.Find("Cancel").GetComponent<Button>().onClick.RemoveListener(actionCancel.Invoke);
        colorImage.color = colorPickerScript.GetColor();
        Destroy(colorPicker);

        if (colorImage.color != oldColor)
        {
            onColorChanged.Invoke(colorImage.color);
        }
    }

    public void SetColor(Color color)
    {
        colorImage.color = color;
    }
}
