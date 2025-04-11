
// Returns vector (dstToSphere, dstThroughSphere)
	        // If ray origin is inside sphere, dstToSphere = 0
	        // If ray misses sphere, dstToSphere = maxValue; dstThroughSphere = 0
void RaySphere_float(float3 sphereCenter, float sphereRadius, float3 rayOrigin, float3 rayDirection, out float2 returnVal)
{
    const float maxFloat = 3.402823466e+38;
    float3 offset = rayOrigin - sphereCenter;
    float a = 1; 
    float b = 2 * dot(offset, rayDirection);
    float c = dot(offset, offset) - sphereRadius * sphereRadius;
    float d = b * b - 4 * a * c; // Discriminant from quadratic formula

		        // Number of intersections: 0 when d < 0; 1 when d = 0; 2 when d > 0
    if (d > 0)
    {
        float s = sqrt(d);
        float dstToSphereNear = max(0, (-b - s) / (2 * a));
        float dstToSphereFar = (-b + s) / (2 * a);

			        // Ignore intersections that occur behind the ray
        if (dstToSphereFar >= 0)
        {
            returnVal = float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
            return;
        }
    }
		        // Ray did not intersect sphere
    returnVal = float2(maxFloat, 0);
}
