using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class updateMenuPlanete : MonoBehaviour
{
    private Boolean activer = true; 
    public GameObject menu;
    public List<GameObject> children;

    void Start()
    {
        
    }
    void Awake()
    {
        foreach (Transform child in transform){
            children.Add(child.gameObject);
        }
        menu = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void updateEtatMenu(){
         if(activer){
             menu.LeanMoveLocalY(-267.99f, 0.5f).setEaseOutExpo();
            activer = false;
        } else{
            menu.LeanMoveLocalY(-22.40f, 0.5f).setEaseOutExpo();
            activer = true;  
        }

    }
}
