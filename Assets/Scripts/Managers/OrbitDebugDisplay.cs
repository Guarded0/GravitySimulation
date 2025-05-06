using UnityEngine;
using System.Collections.Generic;
// THIS FUNCTION BASICALLY DOES THE SAME THING AS THE SIMULATION JUST AT A FASTER RATE AND TRACKS THE POINT ON EACH STEP
// STEP
// CALCULATE VELOCITY
// CALCULATE POSITION
// STORE POSITION IN ARRAY
// DO IT AGAIN


public class OrbitDebugDisplay : MonoBehaviour
{
    // amount of steps itll run the sim for
    public int numSteps = 1000;
    // time in between each step
    public float timeStep = 0.1f;
    // if it should simply use the physics step (recommended)
    public bool usePhysicsTimeStep;

    // relative to somethin...
    public bool isRelativeToBody = false;
    public CelestialBody relativeBody = null;

    public Material trajectoryMaterial;
    private bool lineRenderersEmpty = false;

    static private GameObject privateOrbitRenderer;
    static private VirtualBody temporaryVirtualBody;
    private void Awake()
    {
        privateOrbitRenderer = GameObject.Find("Private Orbit Renderer") ?? CreatePrivateOrbitRenderer();
    }
    void Start()
    {
        foreach(CelestialBody body in NBodySimulation.celestialBodies)
        {
            body.trailRenderer.material = trajectoryMaterial; // this shouldnt be here but for now
            body.trajectoryRenderer.material = trajectoryMaterial;
        }
    }

    void Update()
    {
        if (NBodySimulation.Instance.drawOrbits) DrawOrbits();
        if (!NBodySimulation.Instance.drawOrbits && !lineRenderersEmpty) HideOrbits();
    }

    void DrawOrbits()
    {
        // creates virtual body array
        List<VirtualBody> virtualBodies = CreateVirtualBodies();
        // create array for storing positions in each step (array[celestialBodyIndex][step number] = position at that step)
        Vector3[][] drawPoints = new Vector3[virtualBodies.Count][];
        for (int i = 0; i < virtualBodies.Count; i++)
        {
            drawPoints[i] = new Vector3[numSteps];
        }
        // index of the body thats relative (have to find it first)
        int relativeIndex = FindRelativeBody();
        Vector3 relativeBodyInitialPosition = virtualBodies[relativeIndex].position;



        // Simulate
        for (int step = 0; step < numSteps; step++)
        {
            Vector3 relativeBodyPosition = (isRelativeToBody) ? virtualBodies[relativeIndex].position : Vector3.zero;
            // Update velocities
            for (int i = 0; i < virtualBodies.Count; i++)
            {
                if (virtualBodies[i].isAnchored) { continue; }
                virtualBodies[i].velocity += CalculateAcceleration(i, virtualBodies) * timeStep;
            }
            // Update positions
            for (int i = 0; i < virtualBodies.Count; i++)
            {
                if (virtualBodies[i].isAnchored) { continue; }
                // calculate new position
                Vector3 newPos = virtualBodies[i].position + virtualBodies[i].velocity * timeStep;
                virtualBodies[i].position = newPos;
                // account for relative body
                if (isRelativeToBody)
                {
                    // if its the relative body
                    if (i == relativeIndex)
                    {
                        newPos = relativeBodyInitialPosition;
                    }
                    else // if its not...
                    {
                        // apply offset
                        var relativeOffset = relativeBodyPosition - relativeBodyInitialPosition;
                        newPos -= relativeOffset;
                    }
                }

                // Store position
                drawPoints[i][step] = newPos;
            }
        }
        lineRenderersEmpty = false;
        // Draw paths
        DrawPaths(virtualBodies, drawPoints);
    }

    List<VirtualBody> CreateVirtualBodies()
    {
        List<CelestialBody> bodies = NBodySimulation.celestialBodies;
        List<VirtualBody> virtualBodies = new List<VirtualBody>();
        for (int i = 0; i < bodies.Count; i++)
        {
            virtualBodies.Add(new VirtualBody(bodies[i].transform.position, bodies[i].planetSettings));
        }
        if (temporaryVirtualBody != null) virtualBodies.Add(temporaryVirtualBody);
        return virtualBodies;
    }

    void DrawPaths(in List<VirtualBody> virtualBodies, in Vector3[][] drawPoints)
    {
        var bodies = NBodySimulation.celestialBodies;
        for (int bodyIndex = 0; bodyIndex < virtualBodies.Count; bodyIndex++)
        {
            //if (bodies[bodyIndex] == null) { continue; }
            if (virtualBodies[bodyIndex].isAnchored) { continue; }
            // gets color of the material (has to have the name as color)
            if (bodyIndex < bodies.Count)
            {
                bodies[bodyIndex].trajectoryRenderer.positionCount = drawPoints[bodyIndex].Length;
                bodies[bodyIndex].trajectoryRenderer.SetPositions(drawPoints[bodyIndex]);
            }else
            {
                // if its a temporary body
                var lineRenderer = privateOrbitRenderer.GetComponent<LineRenderer>();
                lineRenderer.positionCount = drawPoints[bodyIndex].Length;
                lineRenderer.SetPositions(drawPoints[bodyIndex]);
            }

        }
    }
    int FindRelativeBody()
    {
        List<CelestialBody> bodies = NBodySimulation.celestialBodies;
        for (int i = 0; i < bodies.Count; i++)
        {
            if (bodies[i] == relativeBody)
            {
                return i;
            }
        }
        return -1;
    }
    Vector3 CalculateAcceleration(in int i, in List<VirtualBody> virtualBodies)
    {
        Vector3 totalAcceleration = Vector3.zero;
        for (int j = 0; j < virtualBodies.Count; j++)
        {
            if (i == j) continue; // if the same body
            var body = virtualBodies[j];
            if (!body.hasGravity) continue; // if no gravity
            if (NBodySimulation.Instance.planetGravity == false && body.bodyType == BodyType.Planet) continue; // if planet gravity is disabled and its a planet
            Vector3 deltaPosition = body.position - virtualBodies[i].position;
            float sqrDistance = deltaPosition.sqrMagnitude;
            float acceleration = NBodySimulation.Instance.gravConstant * body.mass / sqrDistance;

            totalAcceleration += Vector3.ClampMagnitude(deltaPosition.normalized * acceleration, float.MaxValue);
        }
        return totalAcceleration;
    }
    static public void CreateTemporaryVirtualBody(in Vector3 position, in PlanetSettings planetSettings)
    {
        temporaryVirtualBody = new VirtualBody(position, planetSettings);
    }
    static public void ClearTemporaryVirtualBodies()
    {
        temporaryVirtualBody = null;
    }
    void HideOrbits()
    {
        foreach (var body in NBodySimulation.celestialBodies)
        {
            body.trajectoryRenderer.positionCount = 0;
        }
        lineRenderersEmpty = true;
    }

    void OnValidate()
    {
        if (usePhysicsTimeStep)
        {
            timeStep = NBodySimulation.physicsTimeStep;
        }
    }
    private GameObject CreatePrivateOrbitRenderer()
    {
        var orbitRenderer = new GameObject("Private Orbit Renderer");
        orbitRenderer.transform.SetParent(transform);
        var lineRenderer = orbitRenderer.AddComponent<LineRenderer>();
        lineRenderer.material = trajectoryMaterial;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        return orbitRenderer;
    }

    // basically a copy of CelestialBody for the orbit sim 
    class VirtualBody
    {
        public Vector3 position;
        public Vector3 velocity;
        public float mass;
        public bool isAnchored;
        public BodyType bodyType;
        public bool hasGravity; 

        public VirtualBody(in Vector3 startPosition, in PlanetSettings planetSettings)
        {
            position = startPosition;
            velocity = planetSettings.velocity;
            mass = planetSettings.mass;
            isAnchored = planetSettings.isAnchored;
            bodyType = planetSettings.bodyType;
            hasGravity = planetSettings.hasGravity;
        }
    }
}