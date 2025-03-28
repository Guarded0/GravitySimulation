using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider slider; 
    public float parameterVitesse; 

    void Start()// Initialiser
    {
        
        slider.value = parameterVitesse;
        parameterVitesse = slider.value;
        slider.onValueChanged.AddListener(UpdateParameter);
    }

    void UpdateParameter(float valeurVitesse)
    {
        parameterVitesse = valeurVitesse;  // modifier
        
        NBodySimulation.Instance.simulationSpeed = valeurVitesse;
    }
    void Update()
    {
        
    }
}