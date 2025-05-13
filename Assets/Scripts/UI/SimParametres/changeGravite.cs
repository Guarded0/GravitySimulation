using UnityEngine;
using TMPro;

public class changeGravite : MonoBehaviour
{
    public TMP_InputField inputField;   
    public float gravite = 0f;       // gravité initale

    void Start()
    {
        inputField.text = gravite.ToString();
        inputField.onEndEdit.AddListener(ValidateInput);
    }

    void ValidateInput(string input)
    {
        if (float.TryParse(input, out float resultat))
        {
            gravite = resultat;
            NBodySimulation.Instance.gravConstant = resultat;
        }
        else
        {
            Debug.LogWarning("Entrée invalide. Seuls les chiffres sont autorisés.");
            inputField.text = gravite.ToString();  // Réinitialisation
        }
    }
}