using System;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

public class MouvementCamera : MonoBehaviour
{
    private Camera cam;
    private Transform cible;  
    public Vector3 offset;
    private Boolean avant = false;
    private Boolean arriere = false;
    private Boolean droite = false;
    private Boolean gauche = false;
    private Boolean haut = false;
    private Boolean bas = false;
    public float vitesseMax = 50f;
    public float vitesseZ = 0f;
    public float vitesseX = 0f;
    public float vitesseY = 0f;
    public float acceleration = 50f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     cible = NBodySimulation.Instance.relativeBody.gameObject.transform;

    }
    void Awake()
    {
        cam = Camera.main;
    }


    // Update is called once per frame
    void Update()
    {
        choisirCible();           
        verifierMouvement();
        updateVitesse();
        updateMouvement();
        transform.position = cible.position + offset;
        transform.LookAt(cible);
    }
    

    void choisirCible(){
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit)){
                cible = hit.transform;
            } 
        }
        if(Input.GetKey(KeyCode.Escape)){
            cible = NBodySimulation.Instance.relativeBody.gameObject.transform;

        }
    }
    void updateVitesse(){
        if(avant||arriere){
            vitesseZ += acceleration*Time.deltaTime;
            vitesseZ = Math.Min(vitesseZ, vitesseMax);
        }else{
            vitesseZ -= 2*acceleration*Time.deltaTime;
            vitesseZ = Math.Max(vitesseZ,0);
        }
           if(haut||bas){
            vitesseY += acceleration*Time.deltaTime;
            vitesseY = Math.Min(vitesseY, vitesseMax);
        }else{
            vitesseY -= 2*acceleration*Time.deltaTime;
            vitesseY = Math.Max(vitesseZ,0);
        }
             if(droite||gauche){
            vitesseX += acceleration*Time.deltaTime;
            vitesseX = Math.Min(vitesseX, vitesseMax);
        }else{
            vitesseX -= 2*acceleration*Time.deltaTime;
            vitesseX = Math.Max(vitesseZ,0);
        }
    }
    
    void updateMouvement(){
        if(avant){
            offset += cam.transform.forward*vitesseZ*Time.deltaTime;
        } else if(arriere){
            offset -= cam.transform.forward*vitesseZ*Time.deltaTime;
        }
        
    }
    void verifierMouvement(){
        if(Input.GetKey("q")){
            avant = true;
        }else{
            avant = false;
        }
        if(Input.GetKey("e")){
            arriere = true;
        }else{
            arriere = false;
        }
        if(Input.GetKey("w")){
            haut = true;
        } else{
            haut = false;
        }
          if(Input.GetKey("s")){
            bas = true;
        } else{
            bas = false;
        }
            if(Input.GetKey("a")){
            gauche = true;
        } else{
            gauche = false;
        }
            if(Input.GetKey("d")){
            droite = true;
        } else{
            droite = false;
        }
    }



    
}
