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
    public static List<CelestialBody> stars { get; private set; } = null;
    public float gravConstant = 1.0f;
    public static float physicsTimeStep { get; private set; } = 0.01f;
    public bool planetGravity = false;
    public bool isRelativeToBody = false;
    public CelestialBody relativeBody = null;
    public float simulationSpeed = 1.0f;
    public bool simulate = true;
    public GameObject planetTemplate;

    public OrbitDebugDisplay orbitDebugDisplay;
    public bool drawOrbits = false;
    public static NBodySimulation Instance { get; private set; }

    private GameObject bodyContainer = null;
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
            return;
        }
        else
        {
            Instance = this;
        }
        
        bodyContainer = GameObject.Find("BodyContainer") ?? new GameObject("BodyContainer");
        celestialBodies = new List<CelestialBody>(FindObjectsByType<CelestialBody>(FindObjectsSortMode.InstanceID));
        stars = new List<CelestialBody>();
        foreach (var celestialBody in celestialBodies)
        {
            if(celestialBody.planetSettings.bodyType == BodyType.Star)
            {
                stars.Add(celestialBody);
            }
        }
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
        if (orbitDebugDisplay != null)
        {
            orbitDebugDisplay.drawOrbits = drawOrbits;
        }
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
    public GameObject CreatePlanet(Vector3 position, PlanetSettings planetSettings, string name = "New planet")
    {
        GameObject newPlanet = Instantiate(planetTemplate, position, Quaternion.identity);
        newPlanet.transform.parent = bodyContainer.transform;
        newPlanet.name = name;
        newPlanet.GetComponent<CelestialBody>().planetSettings = planetSettings;
        newPlanet.GetComponent<CelestialBody>().shouldUpdateSettings = true;
        newPlanet.GetComponent<PlanetGenerator>().CreateUniqueMaterial();
        return newPlanet;
    }
    Vector3 CalculateTotalAcceleration(CelestialBody mainBody)
    {
        Vector3 totalAcceleration = Vector3.zero;
        foreach (CelestialBody body in celestialBodies)
        {
            if (mainBody == body) continue;
            if (!body.hasGravity) continue;
            if (planetGravity == false && body.planetSettings.bodyType == BodyType.Planet) continue;
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
        CelestialBody celestialBody1 = gameObject.GetComponent<CelestialBody>();
        celestialBodies.Add(celestialBody1);
        if (celestialBody1.planetSettings.bodyType == BodyType.Star) stars.Add(celestialBody1);
    }
    void OnPlanetRemoved(GameObject gameObject)
    {
        CelestialBody celestialBody = gameObject.GetComponent<CelestialBody>();
        celestialBodies.Remove(celestialBody);
        if (celestialBody.planetSettings.bodyType == BodyType.Star) stars.Remove(celestialBody);
    }
}
