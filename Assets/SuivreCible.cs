using System;
using System.Collections;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.SocialPlatforms;

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
    private Boolean repositionerCamera = false;
    public float vitesseMax = 50f;
    public float vitesseZ = 0f;
    public float vitesseX = 0f;
    public float vitesseY = 0f;
    public float acceleration = 50f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     cible = NBodySimulation.Instance.relativeBody.gameObject.transform;
     transform.LookAt(cible);
    }
    void Awake()
    {
        cam = Camera.main;
    }


    // Update is called once per frame
    void Update()
    {
        choisirCible();    
        if(repositionerCamera == false){   
        verifierMouvement();
        updateVitesse();
        updateMouvement();
        
    }
    }

    
    

    void choisirCible(){
        //Si tu click sur un objet ta cameras va le regarder
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit)){
                offset = transform.position - cible.position;
                cible = hit.transform;
                transform.position = cible.transform.position + offset;
                transform.LookAt(cible);
            } 
        }
        //si tu click sur escape tu retour au centre du system
        if(Input.GetKey(KeyCode.Escape)){
            cible = NBodySimulation.Instance.relativeBody.gameObject.transform;
            transform.LookAt(cible);

        }
    }
    void updateVitesse(){
        //Si les booléan sont vrais augmenter la vitesse sinon la diminuer 
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
        //Change la vitesse si les boolean sont true 
        if(avant){
            transform.position += cam.transform.forward*vitesseZ*Time.deltaTime;
        } else if(arriere){
            transform.position -= cam.transform.forward*vitesseZ*Time.deltaTime;
        }
        if(droite){
            transform.RotateAround(cible.transform.position, Vector3.down , vitesseX * Time.deltaTime);
        } else if(gauche){
            transform.RotateAround(cible.transform.position, Vector3.up , vitesseX * Time.deltaTime);
        }
        if(haut && transform.eulerAngles.x < 85 ){
            offset = transform.position - cible.position;
            Vector3 horizontal = new Vector3(-offset.z, 0, offset.x);
            transform.RotateAround(cible.transform.position, horizontal, vitesseY * Time.deltaTime);
        }
        if(bas && transform.eulerAngles.x > -85){
            offset = transform.position - cible.position;
            Vector3 horizontal = new Vector3(offset.z, 0, -offset.x);
            transform.RotateAround(cible.transform.position, horizontal, vitesseY * Time.deltaTime);
            
        }
          
    }
    
    void verifierMouvement(){
        //Update les boolean
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

