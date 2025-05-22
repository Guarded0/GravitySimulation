using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
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


    public CelestialBody referenceBody = null;

    public float simulationSpeed = 1.0f;
    public bool simulate = true;
    public GameObject planetTemplate;
    public bool drawOrbits = false;
    public float orbitPredictionSteps = 1000f;
    public static NBodySimulation Instance { get; private set; }
    private GameObject bodyContainer = null;
    public static float simulationDeltaTime { get; private set;} = 1.0f;

    private DiagnosticChronometer chronometer = new DiagnosticChronometer();
    public double averageTimeMiliseconds;

    private BodyPoolingSystem bodyPoolingSystem = new BodyPoolingSystem();

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
        Time.fixedDeltaTime = physicsTimeStep;
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
    }
    private void OnValidate()
    {
        Init();
    }
    private void FixedUpdate()
    {
        if (!Application.isPlaying) return;
        if (!simulate) return;
        chronometer.Start();
        simulationDeltaTime = physicsTimeStep * simulationSpeed;

        Vector3 offsetPosition = Vector3.zero;
        // TODO: DO THIS IN FUNCTION AND NOT HERE
        if (referenceBody != null)
        {
            offsetPosition = -referenceBody.transform.position;
            referenceBody.transform.position = Vector3.zero;

            if (offsetPosition != Vector3.zero)
            {
                foreach (CelestialBody body in celestialBodies)
                {
                    if (referenceBody != body)
                    {
                        body.transform.position += offsetPosition;
                    }
                }
            }
        }
        UpdateBodiesJobs(celestialBodies);

        chronometer.Stop();
        averageTimeMiliseconds = chronometer.GetMeanTimeMiliseconds();
    }

    private void UpdateBodiesJobs(List<CelestialBody> celestialBodies)
    {

        // FIXME: SKIP ANCHORED BODIES
        NativeArray<BodyData> bodyDatas = new NativeArray<BodyData>(celestialBodies.Count, Allocator.TempJob);
        NativeArray<Vector3> velocities = new NativeArray<Vector3>(celestialBodies.Count, Allocator.TempJob);
        NativeArray<Vector3> positions = new NativeArray<Vector3>(celestialBodies.Count, Allocator.TempJob);
        int relativeIndex = -1;

        for (int i = 0; i < celestialBodies.Count; i++)
        {
            if (celestialBodies[i] == referenceBody) relativeIndex = i;
            bodyDatas[i] = new BodyData
            {
                mass = celestialBodies[i].planetSettings.mass,
                hasGravity = celestialBodies[i].planetSettings.hasGravity,
                isAnchored = celestialBodies[i].planetSettings.isAnchored,
            };
            velocities[i] = celestialBodies[i].planetSettings.velocity;
            positions[i] = celestialBodies[i].transform.position;
        }
        VelocityJob velocityJob = new VelocityJob
        {
            bodyDatas = bodyDatas,
            velocities = velocities,
            positions = positions,
            gravConstant = gravConstant,
            deltaTime = simulationDeltaTime,
        };

        JobHandle velocityHandle = velocityJob.Schedule(celestialBodies.Count, 64);
        velocityHandle.Complete();

        Vector3 referenceVelocityOffset = relativeIndex != -1 ? velocities[relativeIndex] : Vector3.zero;
        for (int i = 0; i < celestialBodies.Count; i++)
        {
            if (bodyDatas[i].isAnchored) continue;
            celestialBodies[i].planetSettings.velocity = velocities[i];
            // update Position 
            celestialBodies[i].rb.MovePosition(positions[i] + (velocities[i] - referenceVelocityOffset) * simulationDeltaTime);
        }

        bodyDatas.Dispose();
        velocities.Dispose();
        positions.Dispose();
    }

    public GameObject CreatePlanet(Vector3 position, PlanetSettings planetSettings, string name = "New planet")
    {
        GameObject newPlanet;
        if (!bodyPoolingSystem.isEmpty)
        {
            newPlanet = bodyPoolingSystem.GetFromPool();
            newPlanet.transform.position = position;
        }
        else
        {
            newPlanet = Instantiate(planetTemplate, position, Quaternion.identity);
        }
        newPlanet.transform.parent = bodyContainer.transform;
        newPlanet.name = name;
        newPlanet.GetComponent<CelestialBody>().planetSettings = planetSettings;
        newPlanet.GetComponent<CelestialBody>().shouldUpdateSettings = true;
        newPlanet.GetComponent<PlanetGenerator>().CreateUniqueMaterial();
        planetAdded.Invoke(newPlanet);
        return newPlanet;
    }
    public void DestroyBody(GameObject gameObject)
    {
        if (gameObject == null) return;
        CelestialBody celestialBody = gameObject.GetComponent<CelestialBody>();

        if (celestialBody != null)
        {
            if (referenceBody == celestialBody)
            {
                referenceBody = null;
            }
            celestialBodies.Remove(celestialBody);
            if (celestialBody.planetSettings.bodyType == BodyType.Star)
            {
                stars.Remove(celestialBody);
            }

            gameObject.SetActive(false);
            bodyPoolingSystem.AddToPool(gameObject);
        }
        planetRemoved.Invoke(gameObject);
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

public struct BodyData
{
    public float mass;
    public bool hasGravity;
    public bool isAnchored;
    // TODO: IMPLEMENT BODY TYPE...
    //public BodyType bodyType;
}
[BurstCompile]
public struct VelocityJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<BodyData> bodyDatas;
    public NativeArray<Vector3> velocities;
    [ReadOnly] public NativeArray<Vector3> positions;
    public float gravConstant;
    public float deltaTime;
    public void Execute(int index)
    {
        if (bodyDatas[index].isAnchored) return; // skip anchored bodies
        Vector3 acceleration = Vector3.zero;
        for (int j = 0; j < bodyDatas.Length; j++)
        {
            if (index == j) continue; // if the same body
            if (!bodyDatas[j].hasGravity) continue; // if no gravity
            Vector3 deltaPosition = positions[j] - positions[index];
            float sqrDistance = deltaPosition.sqrMagnitude;
            acceleration += deltaPosition.normalized * gravConstant * bodyDatas[j].mass / sqrDistance;
        }
        velocities[index] += acceleration * deltaTime;
    }
}