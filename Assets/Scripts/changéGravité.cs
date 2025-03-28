using UnityEngine;
using TMPro;

public class ParamManager : MonoBehaviour
{
    public TMP_InputField inputField;   // Champ TMP
    public float paramValue = 0f;       // Valeur par défaut

    void Start()
    {
        inputField.text = paramValue.ToString();
        inputField.onEndEdit.AddListener(ValidateInput);
    }

    void ValidateInput(string input)
    {
        if (float.TryParse(input, out float result))
        {
            paramValue = result;
            NBodySimulation.Instance.gravConstant = result;
        }
        else
        {
            Debug.LogWarning("Entrée invalide. Seuls les chiffres sont autorisés.");
            inputField.text = paramValue.ToString();  // Réinitialisation
        }
    }
}