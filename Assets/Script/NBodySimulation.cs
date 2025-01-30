using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class NBodySimulation : MonoBehaviour
{
    static public CelestialBody[] celestialBodies = null;
    public float gravConstant = 1.0f;
    public float physicsTimeStep = 0.01f;
    public bool planetGravity = false;
    public bool isRelativeToBody = false;
    public CelestialBody relativeBody = null;
    public static NBodySimulation Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //if (!Application.isPlaying) return;

        celestialBodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.InstanceID);
        Time.fixedDeltaTime = physicsTimeStep;
    }
    private void OnValidate()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    private void FixedUpdate()
    {
        if (!Application.isPlaying) return;
        Vector3 offsetPosition = Vector3.zero;
        if (isRelativeToBody && relativeBody != null)
        {
            offsetPosition = -relativeBody.transform.position;
            relativeBody.transform.position = Vector3.zero;

            if (offsetPosition != Vector3.zero)
            {
                foreach (CelestialBody body in celestialBodies)
                {
                    if (relativeBody != body)
                    {
                        body.transform.position += offsetPosition;
                    }
                }
            }
        }
        foreach (CelestialBody body in celestialBodies)
        {
            UpdateVelocity(body);
        }
        foreach (CelestialBody body in celestialBodies)
        {
            if (isRelativeToBody && relativeBody == body)
            {
                continue;
            }
            UpdatePosition(body);
        }
    }
    public void UpdateVelocity(CelestialBody body)
    {
        Vector3 totalAcceleration = Vector3.zero;
        body.velocity += CalculateTotalAcceleration(body) * physicsTimeStep;
    }

    public void UpdatePosition(CelestialBody body)
    {
        if (body.isAnchored) return;
        Vector3 newPos = body.rb.position + body.velocity * physicsTimeStep;
        if (isRelativeToBody && relativeBody != null)
        {
            newPos -= relativeBody.velocity * physicsTimeStep;
        }

        body.rb.MovePosition(newPos);
    }
    Vector3 CalculateTotalAcceleration(CelestialBody mainBody)
    {
        Vector3 totalAcceleration = Vector3.zero;
        foreach (CelestialBody body in celestialBodies)
        {
            if (mainBody == body) continue;
            if (!body.hasGravity) continue;
            if (NBodySimulation.Instance.planetGravity == false && body.isPlanet) continue;
            Vector3 deltaPosition = body.transform.position - mainBody.transform.position;
            float sqrDistance = deltaPosition.sqrMagnitude;
            float acceleration = (NBodySimulation.Instance.gravConstant * body.mass) / sqrDistance;

            totalAcceleration += Vector3.ClampMagnitude(deltaPosition.normalized * acceleration, float.MaxValue);
        }
        return totalAcceleration;
    }



}
