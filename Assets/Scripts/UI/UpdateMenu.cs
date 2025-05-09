using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UpdateMenu : MonoBehaviour
{
    private Boolean activer =  true; 
    public Vector3 positionOuvert = new Vector3();
    public Vector3 positionFermer = new Vector3();
    private TMP_Text text;
    RectTransform transformToMove;
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
    public void updateEtatMenu(){

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

    }
}
