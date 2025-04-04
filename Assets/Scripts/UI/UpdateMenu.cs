using System;
using System.Collections.Generic;
using UnityEngine;

public class UpdateMenu : MonoBehaviour
{
    private Boolean activer =  true; 
    public Vector3 positionOuvert = new Vector3();
    public Vector3 positionFermer = new Vector3();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    void Awake()
    {
      
       
    }


    // Update is called once per frame
    void Update()
    {
        
    }
    public void updateEtatMenu(){
         if(activer){
            LeanTween.move(gameObject.GetComponent<RectTransform>(), positionFermer, 0.5f).setEase(LeanTweenType.easeOutExpo);
            activer = false;
        } else{
            LeanTween.move(gameObject.GetComponent<RectTransform>(), positionOuvert, 0.5f).setEase(LeanTweenType.easeOutExpo);
            activer = true;  
        }

    }
}
