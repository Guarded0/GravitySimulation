using System;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private Boolean activer = false;
    float enter;
    private Plane plane;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    void Awake()
    {
        plane = new Plane(new Vector3(0,1,0), 0);
    }
    // Update is called once per frame
    void Update()
    {
        if(activer){
            plane.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out enter); 
            
        }
    }

    public void demarerConstruction(CelestialBody body){
        activer = false;
    }
}
