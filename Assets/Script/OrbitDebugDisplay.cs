using UnityEngine;

[ExecuteInEditMode]
public class OrbitDebugDisplay : MonoBehaviour
{

    public int numSteps = 1000;
    public float timeStep = 0.1f;
    public bool usePhysicsTimeStep;

    public float width = 100;
    public bool useThickLines;
    public bool isRelativeToBody = false;
    public CelestialBody relativeBody = null;

    public bool drawOrbits = false;
    void Start()
    {
        if (Application.isPlaying)
        {
            HideOrbits();
        }
    }

    void Update()
    {
        if (!drawOrbits) { 
            HideOrbits();
            return;
        }
        DrawOrbits();
    }

    void DrawOrbits()
    {
        CelestialBody[] bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.InstanceID);
        var virtualBodies = new VirtualBody[bodies.Length];
        var drawPoints = new Vector3[bodies.Length][];
        int relativeIndex = 0;
        Vector3 relativeBodyInitialPosition = Vector3.zero;
        // Initialize virtual bodies (don't want to move the actual bodies)
        for (int i = 0; i < virtualBodies.Length; i++)
        {
            virtualBodies[i] = new VirtualBody(bodies[i]);

            drawPoints[i] = new Vector3[numSteps];

            if (bodies[i] == relativeBody && isRelativeToBody)
            {
                relativeIndex = i;
                relativeBodyInitialPosition = virtualBodies[i].position;
            }
        }

        // Simulate
        for (int step = 0; step < numSteps; step++)
        {
            Vector3 relativeBodyPosition = (isRelativeToBody) ? virtualBodies[relativeIndex].position : Vector3.zero;
            // Update velocities
            for (int i = 0; i < virtualBodies.Length; i++)
            {
              if (virtualBodies[i].isAnchored) { continue; }
                virtualBodies[i].velocity += CalculateAcceleration(i, virtualBodies) * timeStep;
            }
            // Update positions
            for (int i = 0; i < virtualBodies.Length; i++)
            {
                if (virtualBodies[i].isAnchored) { continue; }
                Vector3 newPos = virtualBodies[i].position + virtualBodies[i].velocity * timeStep;
                virtualBodies[i].position = newPos;
                // account for relative body
                if (isRelativeToBody)
                {
                    var relativeOffset = relativeBodyPosition - relativeBodyInitialPosition;
                    newPos -= relativeOffset;
                }
                if (isRelativeToBody && i == relativeIndex)
                {
                    newPos = relativeBodyInitialPosition;
                }

                drawPoints[i][step] = newPos;
            }
        }

        // Draw paths
        for (int bodyIndex = 0; bodyIndex < virtualBodies.Length; bodyIndex++)
        {
            if (virtualBodies[bodyIndex].isAnchored) { continue; }
            var pathColour = bodies[bodyIndex].gameObject.GetComponentInChildren<MeshRenderer>().sharedMaterial.color;
            for (int i = 0; i < drawPoints[bodyIndex].Length - 1; i++)
            {
                Debug.DrawLine(drawPoints[bodyIndex][i], drawPoints[bodyIndex][i + 1], pathColour);
            }
        }
    }

    Vector3 CalculateAcceleration(int i, VirtualBody[] virtualBodies)
    {
        Vector3 totalAcceleration = Vector3.zero;
        foreach (var body in virtualBodies)
        {
            if (virtualBodies[i] == body) continue;
            if (!body.hasGravity) continue;
            if (NBodySimulation.Instance.planetGravity == false && body.isPlanet) continue;
            Vector3 deltaPosition = body.position - virtualBodies[i].position;
            float sqrDistance = deltaPosition.sqrMagnitude;
            float acceleration = (NBodySimulation.Instance.gravConstant * body.mass) / sqrDistance;

            totalAcceleration += Vector3.ClampMagnitude(deltaPosition.normalized * acceleration, float.MaxValue);
        }
        return totalAcceleration;
    }

    void HideOrbits()
    {
        CelestialBody[] bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.InstanceID);

        // Draw paths
        for (int bodyIndex = 0; bodyIndex < bodies.Length; bodyIndex++)
        {
            var lineRenderer = bodies[bodyIndex].gameObject.GetComponentInChildren<LineRenderer>();
            if ( lineRenderer)
            {
                lineRenderer.positionCount = 0;
            }
        }
    }

    void OnValidate()
    {
        if (usePhysicsTimeStep)
        {
            timeStep = Time.fixedDeltaTime;
        }
    }

    class VirtualBody
    {

        public Vector3 position;
        public Vector3 velocity;
        public float mass;
        public bool isAnchored;
        public bool isPlanet;
        public bool hasGravity; 

        public VirtualBody(CelestialBody body)
        {
            position = body.transform.position;
            if (Application.isPlaying)
            {
                velocity = body.velocity;

            }
            else
            {
                velocity = body.initialVelocity;
            }
            mass = body.mass;
            isAnchored = body.isAnchored;
            isPlanet = body.isPlanet;
            hasGravity = body.hasGravity;
        }
    }
}