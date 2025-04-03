using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.Rendering.DebugUI;
public class SimVariableController : MonoBehaviour
{
    //public MonoBehaviour script;
    public string nomVariable;
    public Slider slider;
    public TMP_InputField inputField;
    private void Start()
    {
        if (nomVariable == null) return;
        slider.value = (float)NBodySimulation.Instance.GetType().GetField(nomVariable).GetValue(NBodySimulation.Instance);
        inputField.onEndEdit.AddListener(UpdateFromInputField); //écouter changements de la boite de texte
        slider.onValueChanged.AddListener(UpdateFromSlider); //écouter les changements du slider
    }
    public void UpdateFromInputField(string input)
    {
        float nouvelleValeur = (float)NBodySimulation.Instance.GetType().GetField(nomVariable).GetValue(NBodySimulation.Instance);
        if (float.TryParse(input, out float result))
        {

            nouvelleValeur = Mathf.Round(result * 10f) / 10f;

            if (nouvelleValeur >= 0 && nouvelleValeur <= 5)
            {
                NBodySimulation.Instance.simulationSpeed = nouvelleValeur; // appliquer la nouvelle vitesse
            }
            else
            {
                nouvelleValeur = 0;
            }
            slider.value = nouvelleValeur;  // Mettre à jour le slider
            inputField.text = nouvelleValeur.ToString("F1"); // Mettre à jour la boite de texte

        }
        else
        {
            // Si l'entrée est invalide
            inputField.text = nouvelleValeur.ToString("F1");
        }
    }
    public void UpdateFromSlider(float number)
    {
        NBodySimulation.Instance.GetType().GetField(nomVariable).SetValue(NBodySimulation.Instance, number);
        number = Mathf.Round(number * 10f) / 10f;
        inputField.text = number.ToString("F1"); // Mettre à jour l'input field
    }    
}
