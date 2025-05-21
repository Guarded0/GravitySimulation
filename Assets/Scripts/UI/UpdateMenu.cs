using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
public class UpdateMenu : MonoBehaviour
{
    //Boolean représentant si le transformToMove est activer ou désactiver
    [SerializeField] private Boolean activer = false; 
    
    public Vector3 positionOuvert = new Vector3();
    public Vector3 positionFermer = new Vector3();
    //Symbole a l'interieur du bouton
    private TMP_Text text;
    //Le transform du game object a bouger 
    RectTransform transformToMove;
    public UnityEvent<bool> stateChanged = new UnityEvent<bool>(); // Événement pour notifier les changements d'état
    /// <summary>
    /// Définit le transform du transformToMove 
    /// Définit le text du bouton 
    /// </summary>
    private void Awake()
    {
        var button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(updateEtatMenu);
        text = GetComponentInChildren<TMP_Text>();
        if (transform.parent.TryGetComponent<Canvas>(out _))
        {
            transformToMove = GetComponent<RectTransform>();
        }
        else
        {
            transformToMove = transform.parent.GetComponent<RectTransform>();
        }
    }
    /// <summary>
    /// Lorsque le bouton est appuyer la position de transformToMove vas 
    /// changer entre l'états ouvert et fermer 
    /// Vas ajuster text en fonction de l'états du tranformToMove 
    /// </summary>
    public void updateEtatMenu()
    {

        if (activer)
        {
            LeanTween.move(transformToMove, positionFermer, 0.5f).setEase(LeanTweenType.easeOutExpo);
            if (text != null && text.text == "<") text.text = ">";
            activer = false;
        }
        else
        {
            LeanTween.move(transformToMove, positionOuvert, 0.5f).setEase(LeanTweenType.easeOutExpo);
            if (text != null && text.text == ">") text.text = "<";
            activer = true;
        }
        stateChanged.Invoke(activer); // Notifier les abonnés de l'événement
    }
}
