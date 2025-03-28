using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider slider;       // Le slider
    public float parameterVitesse;     // paramètre a modifié

    void Start()
    {
        // Initialise le slider avec la valeur actuelle
        slider.value = parameterVitesse;
        slider.onValueChanged.AddListener(UpdateParameter);
    }

    void UpdateParameter(float valeur)
    {
        parameterVitesse = valeur;  // Met à jour le paramètre
        
        NBodySimulation.Instance.simulationSpeed = valeur;
    }
}