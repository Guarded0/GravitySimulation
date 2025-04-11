using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
[ExecuteInEditMode]
public class NBodySimulation : MonoBehaviour
{
    // array of celestial bodies
    public static UnityEvent<GameObject> planetAdded;
    public static UnityEvent<GameObject> planetRemoved;
    public static List<CelestialBody> celestialBodies { get; private set; } = null;
    public float gravConstant = 1.0f;
    public static float physicsTimeStep { get; private set; } = 0.01f;
    public bool planetGravity = false;
    public bool isRelativeToBody = false;
    public CelestialBody relativeBody = null;
    public float simulationSpeed = 1.0f;
    public bool simulate = true;
    public GameObject planetTemplate;
    public static NBodySimulation Instance { get; private set; }
    void CreateEvent()
    {
        if (planetAdded == null)
            planetAdded = new UnityEvent<GameObject>();
        planetAdded.AddListener(OnPlanetAdded);
        if (planetRemoved == null)
            planetRemoved= new UnityEvent<GameObject>();
        planetRemoved.AddListener(OnPlanetRemoved);
    }
    void Init()
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

        celestialBodies = new List<CelestialBody>(FindObjectsByType<CelestialBody>(FindObjectsSortMode.InstanceID));

        CreateEvent();
    }
    private void Awake()
    {
        Init();
        Time.fixedDeltaTime = physicsTimeStep;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }
    private void OnValidate()
    {
        Init();
    }
    private void FixedUpdate()
    {
        if (!Application.isPlaying) return;
        if (!simulate) return;
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
        body.planetSettings.velocity += CalculateTotalAcceleration(body) * physicsTimeStep *   simulationSpeed;
    }

    public void UpdatePosition(CelestialBody body)
    {
        if (body.isAnchored) return;
        Vector3 newPos = body.rb.position + body.planetSettings.velocity * physicsTimeStep * simulationSpeed;
        if (isRelativeToBody && relativeBody != null)
        {
            newPos -= relativeBody.planetSettings.velocity * physicsTimeStep * simulationSpeed;
        }

        body.rb.MovePosition(newPos);
    }
    public void CreatePlanet(Vector3 position, PlanetSettings planetSettings, string name = "New planet")
    {
        GameObject newPlanet = Instantiate(planetTemplate, position, Quaternion.identity);
        newPlanet.transform.parent = null;
        newPlanet.name = name;
        newPlanet.GetComponent<CelestialBody>().planetSettings = planetSettings;
        newPlanet.GetComponent<CelestialBody>().shouldUpdateSettings = true;
    }
    Vector3 CalculateTotalAcceleration(CelestialBody mainBody)
    {
        Vector3 totalAcceleration = Vector3.zero;
        foreach (CelestialBody body in celestialBodies)
        {
            if (mainBody == body) continue;
            if (!body.hasGravity) continue;
            if (planetGravity == false && body.bodyType == bodyType.Planet) continue;
            Vector3 deltaPosition = body.transform.position - mainBody.transform.position;
            float sqrDistance = deltaPosition.sqrMagnitude;
            float acceleration = gravConstant * body.planetSettings.mass / sqrDistance;

            totalAcceleration += Vector3.ClampMagnitude(deltaPosition.normalized * acceleration, float.MaxValue);
        }
        return totalAcceleration;
    }

    void OnPlanetAdded(GameObject gameObject)
    {
        foreach (var celestialBody in celestialBodies)
        {
            if (celestialBody.gameObject == gameObject) return;
        }
        celestialBodies.Add(gameObject.GetComponent<CelestialBody>());
    }
    void OnPlanetRemoved(GameObject gameObject)
    {
        celestialBodies.Remove(gameObject.GetComponent<CelestialBody>());

    }
}
