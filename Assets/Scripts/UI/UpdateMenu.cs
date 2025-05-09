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

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(updateEtatMenu);
        text = GetComponentInChildren<TMP_Text>();
    }
    public void updateEtatMenu(){
         if(activer){
            LeanTween.move(transform.parent.GetComponent<RectTransform>(), positionFermer, 0.5f).setEase(LeanTweenType.easeOutExpo);
            text.text = ">";
            activer = false;
        } else{
            LeanTween.move(transform.parent.GetComponent<RectTransform>(), positionOuvert, 0.5f).setEase(LeanTweenType.easeOutExpo);
            text.text = "<";
            activer = true;  
        }

    }
}
