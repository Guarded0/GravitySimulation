using UnityEngine;

public class PrimaryLightSource : MonoBehaviour
{ 
    public static Transform FindMainLightSource(Vector3 position)
    {
        (CelestialBody, float) closestStar = (null, float.MaxValue);
        foreach (CelestialBody celestialBody in NBodySimulation.stars)
        {
            float distance = Vector3.Distance(celestialBody.transform.position, position);
            if (distance < closestStar.Item2)
            {
                closestStar.Item1 = celestialBody;
                closestStar.Item2 = distance;
            }
        }
        return closestStar.Item1 != null ? closestStar.Item1.transform : null;
    }
}
