using System;
using System.Collections;
using System.ComponentModel.Design.Serialization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class MouvementCamera : MonoBehaviour
{
    public float vitesseCamera = 100f;
    public float positionLerpSpeed = 10f;
    public float rotationLerpSpeed = 10f;
    public Vector2 distanceLimite = new Vector2(7f, float.PositiveInfinity);

    public float sensibilite = 50f;

    public float angleMaximum = 30f;

    public bool orbitMode = true;
    public static bool softUnlock = false;
    [SerializeField]
    private float3 inputAxis;
    //La derniere position de la cible
    private Vector3 dernierePosition = Vector3.zero;

    private Transform targetTransform;
    private bool movingToOtherCible = false;

    private void Start()
    {
        GameObject target = new GameObject();
        target.transform.position = transform.position;
        targetTransform = target.transform;
        Cible.cibleChanged.AddListener(CibleChanged);
    }
    void Update()
    {
        inputAxis = new float3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), Input.GetAxis("Mouse ScrollWheel") * 10);
        // orbitMode
        if (Cible.current && orbitMode && !softUnlock)
        {
            FollowCible();
            UpdateMouvementOrbite(Cible.current);

        }
        else
        {
            UpdateMouvementLibre();
        }

        MoveToTarget();
    }
    void FollowCible()
    {
        var deltaPos = Cible.current.position - dernierePosition;
        targetTransform.position += deltaPos;

        if (movingToOtherCible)
        {
            if (DistanceFromTarget() < 3f) movingToOtherCible = false;
        }
        else
        {
            transform.position += deltaPos;
        }

        dernierePosition = Cible.current.position;
    }
    void CibleChanged(Transform newTransform)
    {
        if (newTransform)
        {
            movingToOtherCible = true;
        }
    }
    void UpdateMouvementOrbite(Transform cible)
    {
        
        float mouseX = Input.GetAxis("Mouse X") * sensibilite * 2f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilite * 2f * Time.deltaTime;
        // ADDITIONNER LA SOURIS AVEC WASD POUR FACILITER LE MOUVEMENT
        Vector3 mixedInput;
        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            mixedInput = new Vector3(Mathf.Clamp(-inputAxis.x + mouseX,-1,1), Mathf.Clamp(-inputAxis.y + mouseY, -1, 1), inputAxis.z);
        }else
        {
            Cursor.lockState = CursorLockMode.None;
            mixedInput = new Vector3(-inputAxis.x, -inputAxis.y, inputAxis.z);
        }
        //-------------------- HORIZONTAL AXIS --------------------//
        targetTransform.RotateAround(cible.transform.position, Vector3.up,  mixedInput.x * vitesseCamera * Time.deltaTime);

        //-------------------- VERTICAL AXIS   --------------------//
        float distance = (targetTransform.position - cible.position).magnitude;
        float maxHauteur = Mathf.Sin(math.radians(angleMaximum)) * distance;

        // upper limit
        if (mixedInput.y < 0 && targetTransform.position.y - cible.position.y < (maxHauteur - maxHauteur * 0.05f))
        {
            targetTransform.RotateAround(cible.transform.position, Vector3.Cross(targetTransform.forward, Vector3.up), mixedInput.y * vitesseCamera * Time.deltaTime);
        }

        // bottom limit
        if (mixedInput.y > 0 && targetTransform.position.y - cible.position.y > -(maxHauteur - maxHauteur * 0.05f))
        {
            targetTransform.RotateAround(cible.transform.position, Vector3.Cross(targetTransform.forward, Vector3.up), mixedInput.y * vitesseCamera * Time.deltaTime);
        }

        // Z CORRECTION FOR CLAMPING
        targetTransform.position = new float3(targetTransform.position.x, Mathf.Clamp(targetTransform.position.y, cible.position.y - maxHauteur, cible.position.y + maxHauteur), targetTransform.position.z);
        float newDistance = (targetTransform.position - cible.position).magnitude;
        targetTransform.forward = (cible.position - targetTransform.position).normalized;
        targetTransform.position -= targetTransform.forward * (distance - newDistance);

        //-------------------- Z AXIS --------------------//
        // FAR BOUND
        if (mixedInput.z > 0 && distance > (distanceLimite.x + Cible.current.GetComponent<CelestialBody>().planetSettings.radius))
        {
            targetTransform.position += targetTransform.forward * mixedInput.z * vitesseCamera * Time.deltaTime * 2f;
        }
        // CLOSE BOUND
        else if (mixedInput.z < 0 && distance < (distanceLimite.y + Cible.current.GetComponent<CelestialBody>().planetSettings.radius))
        {
            targetTransform.position += targetTransform.forward * mixedInput.z * vitesseCamera * Time.deltaTime * 2f;
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

        float mouseX = Input.GetAxis("Mouse X") * sensibilite * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilite * Time.deltaTime;

        Vector3 rot = targetTransform.localRotation.eulerAngles;
        float yRotation = rot.y + mouseX;
        float xRotation = rot.x - mouseY;

        //xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        targetTransform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);


        Vector3 input = (targetTransform.forward * inputAxis.y + targetTransform.right * inputAxis.x + targetTransform.up * Input.GetAxis("Depth"));


        // transform.LeanMove(transform.position + (transform.forward * inputAxis.y + transform.right * inputAxis.x + transform.up * Input.GetAxis("Depth")) * vitesseCamera * Time.deltaTime, 0.1f);
        targetTransform.position += input * vitesseCamera * Time.deltaTime;
    }

    void MoveToTarget()
    {
        transform.position = Vector3.Lerp(transform.position, targetTransform.position, positionLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetTransform.rotation, rotationLerpSpeed * Time.deltaTime);
    }
    float DistanceFromTarget()
    {
        return Vector3.Distance(transform.position, targetTransform.position);
    }
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.DrawSphere(targetTransform.position, 0.5f);
    }
}

