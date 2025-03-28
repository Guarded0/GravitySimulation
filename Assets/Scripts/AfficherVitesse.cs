using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VitesseController : MonoBehaviour
{
    public TMP_InputField inputField;     
    public Slider slider;                 
    public float vitesse = 0f;            

    void Start()
    {
        // Initialiser slider et boite de texte
        inputField.text = vitesse.ToString("F2");  // Limite à 2 décimales
        slider.value = vitesse;

        
        inputField.onEndEdit.AddListener(ValidateInput); //écouter changements de la boite de texte
        slider.onValueChanged.AddListener(UpdateFromSlider); //écouter les changements du slider
    }

    // Met à jour vitesse lorsque l'utilisateur entre un nombre
    void ValidateInput(string input)
    {
        if (float.TryParse(input, out float result))
        {
            
            vitesse = Mathf.Round(result * 100f) / 100f;
            
            if(vitesse >= 0 && vitesse <=5){
                NBodySimulation.Instance.simulationSpeed = vitesse; // appliquer la nouvelle vitesse
            }
            else{
                vitesse = 0;
            }
            slider.value = vitesse;  // Mettre à jour le slider
            inputField.text = vitesse.ToString("F2"); // Mettre à jour la boite de texte
           
        }
        else
        {
            // Si l'entrée est invalide
            inputField.text = vitesse.ToString("F2");
        }
    }

    // Met à jour vitesse lorsque l'utilisateur déplace le slider
    void UpdateFromSlider(float value)
    {
        
        vitesse = Mathf.Round(value * 100f) / 100f;  
        inputField.text = vitesse.ToString("F2"); // Mettre à jour l'input field
        NBodySimulation.Instance.simulationSpeed = vitesse; // Appliquer la nouvelle vitesse à la simulation
    }
}