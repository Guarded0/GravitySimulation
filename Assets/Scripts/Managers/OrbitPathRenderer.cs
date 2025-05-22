using UnityEngine;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Profiling;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine.UIElements;


// THIS FUNCTION BASICALLY DOES THE SAME THING AS THE SIMULATION JUST AT A FASTER RATE AND TRACKS THE POINT ON EACH STEP
// STEP
// CALCULATE VELOCITY
// CALCULATE POSITION
// STORE POSITION IN ARRAY
// DO IT AGAIN


public class OrbitPathRenderer : MonoBehaviour
{
    // time in between each step
    public float timeStep = 0.1f;
    // if it should simply use the physics step (recommended)
    public bool usePhysicsTimeStep;

    public Material trajectoryMaterial;
    private bool lineRenderersEmpty = false;

    static private GameObject privateOrbitRenderer;
    static private VirtualBody temporaryVirtualBody;

    private CelestialBody currentReferenceBody;
    public DiagnosticChronometer chronometer = new DiagnosticChronometer();



    // THREAD STUFF
    ManualResetEventSlim positionsReady = new ManualResetEventSlim(false);
    ManualResetEventSlim positionsWritten = new ManualResetEventSlim(true);
    ManualResetEventSlim startSimulationEvent = new ManualResetEventSlim(false);
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    CancellationToken cancellationToken;

    NativeArray<BodyData> bodyDatas;
    NativeArray<Vector3> velocities;
    NativeArray<Vector3> positions;

    // amount of steps itll run the sim for
    int numSteps = 1000;
    float gravConstant = 1.0f;
    int referenceIndex = -1;
    Vector3 referenceBodyOffset = Vector3.zero;

    Thread calculationThread;
    Thread writeThread;
    List<VirtualBody> virtualBodies;
    Vector3[][] drawPoints;

    [SerializeField] bool simulationComplete = false;

    const bool CANCEL_THREAD = false;
    const int TIMEOUT_MILISECONDS = 60000;
    int lastCount = 0;
    private void Awake()
    {
        privateOrbitRenderer = GameObject.Find("Private Orbit Renderer") ?? CreatePrivateOrbitRenderer();
        cancellationToken = cancellationTokenSource.Token;
        calculationThread = new Thread(CalculatePositions);
        writeThread = new Thread(WritePositions);
    }
    void Start()
    {
        foreach (CelestialBody body in NBodySimulation.celestialBodies)
        {
            body.trailRenderer.material = trajectoryMaterial; // this shouldnt be here but for now
            body.trajectoryRenderer.material = trajectoryMaterial;
        }
    }

    void FixedUpdate()
    {
        if (simulationComplete) DrawPaths(virtualBodies, drawPoints);
        if (NBodySimulation.Instance.drawOrbits && !startSimulationEvent.IsSet) DrawOrbits();
        if (!NBodySimulation.Instance.drawOrbits && !lineRenderersEmpty) HideOrbits();
    }

    void DrawOrbits()
    {
        // get data from NBodySimulation
        currentReferenceBody = NBodySimulation.Instance.referenceBody;
        numSteps = (int)NBodySimulation.Instance.orbitPredictionSteps;
        gravConstant = NBodySimulation.Instance.gravConstant;

        // creates virtual body array
        virtualBodies = CreateVirtualBodies();

        // create array for storing positions in each step (array[celestialBodyIndex][step number] = position at that step)
        drawPoints = new Vector3[virtualBodies.Count][];
     
        // populate array
        for (int i = 0; i < virtualBodies.Count; i++)
        {
            drawPoints[i] = new Vector3[numSteps];
        }

        // create native arrays if not exist
        if(virtualBodies.Count != lastCount || (bodyDatas.IsCreated || velocities.IsCreated || positions.IsCreated) == false)
        {
            bodyDatas = new NativeArray<BodyData>(virtualBodies.Count, Allocator.Persistent);
            velocities = new NativeArray<Vector3>(virtualBodies.Count, Allocator.Persistent);
            positions = new NativeArray<Vector3>(virtualBodies.Count, Allocator.Persistent);
            lastCount = virtualBodies.Count;
        }

        // populate native arrays
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

        // reference offset to apply for draw points
        referenceBodyOffset = referenceIndex != -1 ? positions[referenceIndex] : Vector3.zero;

        // if cancellation token source is somehow gone or cancelled???
        if (cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested)
        {
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }

        // Start threads if never started
        if(calculationThread.ThreadState == ThreadState.Unstarted)
        {
            calculationThread.Start();
            writeThread.Start();
        }

        startSimulationEvent.Set();
    }

    void CalculatePositions()
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // WAIT FOR START SIMULATION FLAG
            if (WaitForManualResetEvent(startSimulationEvent) == CANCEL_THREAD) break;

            // ENTER LOOP
            while (!simulationComplete)
            {
                // CALCULATE VELOCITIES
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


                // WAIT FOR POSITIONS TO BE WRITTEN TO DRAWPOINTS ARRAY
                if (WaitForManualResetEvent(positionsWritten) == CANCEL_THREAD) break;
                positionsWritten.Reset();

                // IF SIMULATION IS COMPLETE BREAK OUT OF LOOP (BEFORE DOING USELESS WORK)
                if (simulationComplete) break;

                // UPDATE POSITIONS
                for (int i = 0; i < virtualBodies.Count; i++)
                {
                    if (virtualBodies[i].isAnchored) continue;

                    // Store position
                    Vector3 newPosition = positions[i] + velocities[i] * timeStep;
                    positions[i] = newPosition;
                }

                positionsReady.Set();
            }
            positionsWritten.Set(); // W fix
            // SIMULATION END
        }
    }
    void WritePositions()
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // WAIT FOR SIMULATION START
            if (WaitForManualResetEvent(startSimulationEvent) == CANCEL_THREAD) break;
            // START LOOP
            int currentStep = 0;
            while (currentStep < numSteps)
            {
                // WAIT POSITIONS TO BE READY TO WRITE
                if (WaitForManualResetEvent(positionsReady) == CANCEL_THREAD) break;
                positionsReady.Reset();

                // WRITE POSITIONS TO DRAWPOINTS ARRAY
                referenceBodyOffset = referenceIndex != -1 ? positions[referenceIndex] : Vector3.zero;
                for (int i = 0; i < virtualBodies.Count; i++)
                {
                    if (virtualBodies[i].isAnchored) { continue; }
                    drawPoints[i][currentStep] = positions[i] - referenceBodyOffset;
                }


                currentStep++;
                positionsWritten.Set();
            }

            // SIMULATION END
            startSimulationEvent.Reset();
            lineRenderersEmpty = false;
            simulationComplete = true;
            positionsWritten.Set();
        }
    }
    // Returns false if break
    // Returns true if waited successfully
    bool WaitForManualResetEvent(ManualResetEventSlim manualEvent)
    {
        bool timedOut;
        try
        {
            timedOut = !manualEvent.Wait(TIMEOUT_MILISECONDS, cancellationToken);
        }catch(OperationCanceledException)
        {
            return false;
        }
        if (timedOut)
        {
            Debug.LogWarning("THREAD TIMED OUT");
            return false;
        }
        return true;
    }
    List<VirtualBody> CreateVirtualBodies()
    {
        referenceIndex = -1;
        List<CelestialBody> bodies = NBodySimulation.celestialBodies;
        List<VirtualBody> virtualBodies = new List<VirtualBody>();
        for (int i = 0; i < bodies.Count; i++)
        {
            if (currentReferenceBody == bodies[i])
            {
                referenceIndex = i;
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
            }
            else
            {
                // if its a temporary body
                var lineRenderer = privateOrbitRenderer.GetComponent<LineRenderer>();
                lineRenderer.positionCount = drawPoints[bodyIndex].Length;
                lineRenderer.SetPositions(drawPoints[bodyIndex]);
            }
        }
        simulationComplete = false;

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

    private void OnDestroy()
    {
        cancellationTokenSource.Cancel();
        // JOIN THREADS IF RUNNING
        if (calculationThread != null && calculationThread.IsAlive)
        { 
            calculationThread.Join();
        }
        if (writeThread != null && writeThread.IsAlive)
        {
            writeThread.Join();
        }

        cancellationTokenSource.Dispose();
        positionsReady.Dispose();
        positionsWritten.Dispose();
        bodyDatas.Dispose();
        velocities.Dispose();
        positions.Dispose();
    }
}