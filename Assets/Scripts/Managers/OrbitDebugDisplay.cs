using UnityEngine;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Profiling;
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
    public CelestialBody referenceBody = null;

    public Material trajectoryMaterial;
    private bool lineRenderersEmpty = false;

    static private GameObject privateOrbitRenderer;
    static private VirtualBody temporaryVirtualBody;

    public DiagnosticChronometer chronometer = new DiagnosticChronometer();
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
        chronometer.Start();
        if (NBodySimulation.Instance.drawOrbits) DrawOrbits();
        if (!NBodySimulation.Instance.drawOrbits && !lineRenderersEmpty) HideOrbits();
        chronometer.Stop();
    }

    void DrawOrbits()
    {
        // index of the body thats relative (have to find it first)
        int relativeIndex;
        // creates virtual body array
        List<VirtualBody> virtualBodies = CreateVirtualBodies(out relativeIndex);
        // create array for storing positions in each step (array[celestialBodyIndex][step number] = position at that step)
        Vector3[][] drawPoints = new Vector3[virtualBodies.Count][];
        // populate array
        for (int i = 0; i < virtualBodies.Count; i++)
        {
            drawPoints[i] = new Vector3[numSteps];
        }

        NativeArray<BodyData> bodyDatas = new NativeArray<BodyData>(virtualBodies.Count, Allocator.TempJob);
        NativeArray<Vector3> velocities = new NativeArray<Vector3>(virtualBodies.Count, Allocator.TempJob);
        NativeArray<Vector3> positions = new NativeArray<Vector3>(virtualBodies.Count, Allocator.TempJob);
        float gravConstant = NBodySimulation.Instance.gravConstant;
        for (int i = 0; i < virtualBodies.Count; i++)
        {
            bodyDatas[i] = new BodyData
            {
                mass = virtualBodies[i].mass,
                isAnchored = virtualBodies[i].isAnchored,
                hasGravity = virtualBodies[i].hasGravity
            };
            velocities[i] = virtualBodies[i].velocity;
            positions[i] = virtualBodies[i].position;
        }


        for (int step = 0; step < numSteps; step++)
        {
            VelocityJob velocityJob = new VelocityJob
            {
                bodyDatas = bodyDatas,
                velocities = velocities,
                positions = positions,
                gravConstant = gravConstant,
                deltaTime = timeStep
            };
            JobHandle velocityHandle = velocityJob.Schedule(virtualBodies.Count, 64);
            velocityHandle.Complete();

            Vector3 referenceBodyOffset = relativeIndex != -1 ? positions[relativeIndex] : Vector3.zero;
            // Update positions
            for (int i = 0; i < virtualBodies.Count; i++)
            {
                if (virtualBodies[i].isAnchored) { continue; }

                // Store position
                Vector3 newPosition = positions[i] + velocities[i] * timeStep;
                positions[i] = newPosition;
                // account for relative body
                
                drawPoints[i][step] = positions[i] - referenceBodyOffset;
            }

        }

        // dispose
        bodyDatas.Dispose();
        velocities.Dispose();
        positions.Dispose();
        lineRenderersEmpty = false;
        // Draw paths
        DrawPaths(virtualBodies, drawPoints);
    }
    List<VirtualBody> CreateVirtualBodies(out int relativeIndex)
    {
        relativeIndex = -1;
        List<CelestialBody> bodies = NBodySimulation.celestialBodies;
        List<VirtualBody> virtualBodies = new List<VirtualBody>();
        for (int i = 0; i < bodies.Count; i++)
        {
            if (referenceBody == bodies[i])
            {
                relativeIndex = i;
            }
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
    static public void CreateTemporaryVirtualBody(in Vector3 position, in PlanetSettings planetSettings)
    {
        temporaryVirtualBody = new VirtualBody(position, planetSettings);
        privateOrbitRenderer.SetActive(true);
    }
    static public void ClearTemporaryVirtualBodies()
    {
        temporaryVirtualBody = null;
        privateOrbitRenderer.SetActive(false);
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