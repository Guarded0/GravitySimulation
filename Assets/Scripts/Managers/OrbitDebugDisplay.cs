using UnityEngine;
using System.Collections.Generic;
// THIS FUNCTION BASICALLY DOES THE SAME THING AS THE SIMULATION JUST AT A FASTER RATE AND TRACKS THE POINT ON EACH STEP
// STEP
// CALCULATE VELOCITY
// CALCULATE POSITION
// STORE POSITION IN ARRAY
// DO IT AGAIN


[ExecuteAlways]
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

    // well... if u want to draw the damn orbits
    public bool drawOrbits = false;

    public Material trajectoryMaterial;

    void Start()
    {
        if (!Application.isPlaying) return;
        foreach(CelestialBody body in NBodySimulation.celestialBodies)
        {
            body.trailRenderer.material = trajectoryMaterial; // this shouldnt be here but for now
            body.trajectoryRenderer.material = trajectoryMaterial;
        }
    }

    void Update()
    {
        if (drawOrbits) DrawOrbits();
    }

    void DrawOrbits()
    {
        // gets all the bodies
        List<CelestialBody> bodies = NBodySimulation.celestialBodies;
        if (!Application.isPlaying && (bodies[0] == null))
        {
            bodies = new List<CelestialBody>(FindObjectsByType<CelestialBody>(FindObjectsSortMode.InstanceID));
        }
        // creates virtual body array
        var virtualBodies = new VirtualBody[bodies.Count];
        // create array for storing positions in each step (array[celestialBodyIndex][step number] = position at that step)
        var drawPoints = new Vector3[bodies.Count][];
        // index of the body thats relative (have to find it first)
        int relativeIndex = 0;
        Vector3 relativeBodyInitialPosition = Vector3.zero;

        // Initialize virtual bodies (don't want to move the actual bodies)
        for (int i = 0; i < virtualBodies.Length; i++)
        {
            virtualBodies[i] = new VirtualBody(bodies[i]);

            drawPoints[i] = new Vector3[numSteps];

            // when the relative body is found
            if (isRelativeToBody && bodies[i] == relativeBody)
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

        // Draw paths
        for (int bodyIndex = 0; bodyIndex < virtualBodies.Length; bodyIndex++)
        {
            //if (bodies[bodyIndex] == null) { continue; }
            if (virtualBodies[bodyIndex].isAnchored) { continue; }
            // gets color of the material (has to have the name as color)
            //var pathColour = bodies[bodyIndex].gameObject.GetComponentInChildren<MeshRenderer>().sharedMaterial.color;
            var pathColor = Color.white;
            if (Application.isPlaying)
            {
                bodies[bodyIndex].trajectoryRenderer.positionCount = drawPoints[bodyIndex].Length;
                bodies[bodyIndex].trajectoryRenderer.SetPositions(drawPoints[bodyIndex]);
            }else
            {
                for (int i = 0; i < drawPoints[bodyIndex].Length - 1; i++)
                {
                    Debug.DrawLine(drawPoints[bodyIndex][i], drawPoints[bodyIndex][i + 1], pathColor);
                }
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
            if (NBodySimulation.Instance.planetGravity == false && body.bodyType == bodyType.Planet) continue;
            Vector3 deltaPosition = body.position - virtualBodies[i].position;
            float sqrDistance = deltaPosition.sqrMagnitude;
            float acceleration = NBodySimulation.Instance.gravConstant * body.mass / sqrDistance;

            totalAcceleration += Vector3.ClampMagnitude(deltaPosition.normalized * acceleration, float.MaxValue);
        }
        return totalAcceleration;
    }

    void HideOrbits()
    {
        // for when we use line renderers instead of Debug.DrawLine()
    }

    void OnValidate()
    {
        if (usePhysicsTimeStep)
        {
            timeStep = NBodySimulation.physicsTimeStep;
        }
    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        // draw initial velocity ray
        foreach (CelestialBody celestialBody in FindObjectsByType<CelestialBody>(FindObjectsSortMode.InstanceID))
        {
            //Gizmos.color = celestialBody.gameObject.GetComponentInChildren<MeshRenderer>().sharedMaterial.color;
            Gizmos.DrawRay(celestialBody.transform.position, celestialBody.initialVelocity);
        }
    }

    // basically a copy of CelestialBody for the orbit sim 
    class VirtualBody
    {

        public Vector3 position;
        public Vector3 velocity;
        public float mass;
        public bool isAnchored;
        public bodyType bodyType;
        public bool hasGravity; 

        public VirtualBody(CelestialBody body)
        {
            if (body == null) return;
            position = body.transform.position;
            if (Application.isPlaying)
            {
                velocity = body.velocity;

            }
            else
            {
                velocity = body.initialVelocity;
            }
            mass = body.planetSettings.mass;
            isAnchored = body.isAnchored;
            bodyType = body.bodyType;
            hasGravity = body.hasGravity;
        }
    }
}