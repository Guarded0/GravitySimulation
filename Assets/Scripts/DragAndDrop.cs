using System;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    public Transform pointeur;
    private Boolean activer = true;
    private Vector3 coordone;
    public LayerMask layerMask; 
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
        if(activer){
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, float.MaxValue, layerMask)){
                coordone = hit.point; 
                pointeur.position = coordone; 
            }
            
        }
    }

    public void demarerConstruction(CelestialBody body){
        activer = true;
    }
}
