using System;
using System.Collections;
using System.ComponentModel.Design.Serialization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class MouvementCamera : MonoBehaviour
{
    public float vitesseCamera = 100f;
    public Vector2 distanceLimite = new Vector2(1f, 50f);
    public float sensitivity = 1f;
    public float angleMaximum = 25f;
    public bool orbitMode = true;
    public static bool softUnlock = false;
    public float3 inputAxis;
    //La derniere position de la cible
    private Vector3 dernierePosition = Vector3.zero;
    void Update()
    {
        inputAxis = new float3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), Input.GetAxis("Mouse ScrollWheel"));
        // orbitMode
        if (orbitMode && !softUnlock)
        {
            if (Cible.current)
            {
                var deltaPos = Cible.current.position - dernierePosition;
                transform.position += deltaPos;

                updateMouvementOrbite(Cible.current);
                dernierePosition = Cible.current.position;
            }
        }
        else
        {
            
            UpdateMouvementLibre();
        } 
    }
//
    void updateMouvementOrbite(Transform cible) {
        // HORIZONTAL AXIS
        transform.RotateAround(cible.transform.position, Vector3.up, -inputAxis.x * vitesseCamera * Time.deltaTime);

        // VERTICAL AXIS
        float distance = (transform.position - cible.position).magnitude;
        float maxHauteur = Mathf.Sin(math.radians(angleMaximum)) * distance;
        // upper limit
        if (inputAxis.y > 0 && transform.position.y - cible.position.y < (maxHauteur - maxHauteur * 0.05f))
        {
            transform.RotateAround(cible.transform.position, Vector3.Cross(transform.forward, Vector3.up), -inputAxis.y * vitesseCamera * Time.deltaTime);
        }
        // bottom limit
        if (inputAxis.y < 0 && transform.position.y - cible.position.y > -(maxHauteur - maxHauteur * 0.05f))
        {
            transform.RotateAround(cible.transform.position, Vector3.Cross(transform.forward, Vector3.up), -inputAxis.y * vitesseCamera * Time.deltaTime);
        }
        // Z CORRECTION FOR CLAMPING
        transform.position = new float3(transform.position.x, Mathf.Clamp(transform.position.y, cible.position.y - maxHauteur, cible.position.y + maxHauteur), transform.position.z);
        float newDistance = (transform.position - cible.position).magnitude;
        transform.forward = (cible.position - transform.position).normalized;
        transform.position -= transform.forward * (distance - newDistance);


        // Z AXIS
        // FAR BOUND
        if (inputAxis.z > 0 && distance > distanceLimite.x)
        {
            transform.position += transform.forward * inputAxis.z * vitesseCamera * Time.deltaTime * 2f;
        }
        // CLOSE BOUND
        else if (inputAxis.z < 0 && distance < distanceLimite.y)
        {
            transform.position += transform.forward * inputAxis.z * vitesseCamera * Time.deltaTime * 2f;
        }
    }
    void UpdateMouvementLibre()
    {
        if (!Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.None;
            return;
        }
        Cursor.lockState = CursorLockMode.Locked;
        
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        Vector3 rot = transform.localRotation.eulerAngles;
        float yRotation = rot.y + mouseX;
        float xRotation = rot.x - mouseY;

        //xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
       // transform.LeanMove(transform.position + (transform.forward * inputAxis.y + transform.right * inputAxis.x + transform.up * Input.GetAxis("Depth")) * vitesseCamera * Time.deltaTime, 0.1f);
        transform.position += (transform.forward * inputAxis.y + transform.right * inputAxis.x + transform.up * Input.GetAxis("Depth")) * vitesseCamera * Time.deltaTime;
    }
}

