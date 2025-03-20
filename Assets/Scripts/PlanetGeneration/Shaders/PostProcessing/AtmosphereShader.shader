Shader "Hidden/AtmosphereShader"
{
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "AtmospherePass"

            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment MainFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "../Includes/Triplanar.hlsl"
            static const float maxFloat = 3.402823466e+38;

            struct Attri
            {
                uint vertexID : SV_VertexID;
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord   : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(Attri input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
                output.positionCS = pos;
                output.texcoord   = DYNAMIC_SCALING_APPLY_SCALEBIAS(uv);
                float3 viewVector = mul(unity_CameraInvProjection, float4(uv.xy * 2 - 1, 0, -1));
                output.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                return output;
            }
            
            float3 _planetCenter;
            float _planetRadius;
            float _atmosphereRadius;
            int _inScatteringPoints;
            int _opticalDepthPoints;
            float _densityFalloff;
            float _oceanRadius;
            float3 _directionToSun;
            
            float3 _scatteringCoefficients;
            float _intensity;

	        // Returns vector (dstToSphere, dstThroughSphere)
	        // If ray origin is inside sphere, dstToSphere = 0
	        // If ray misses sphere, dstToSphere = maxValue; dstThroughSphere = 0
	        float2 raySphere(float3 sphereCentre, float sphereRadius, float3 rayOrigin, float3 rayDir) {
		        float3 offset = rayOrigin - sphereCentre;
		        float a = 1; // Set to dot(rayDir, rayDir) if rayDir might not be normalized
		        float b = 2 * dot(offset, rayDir);
		        float c = dot (offset, offset) - sphereRadius * sphereRadius;
		        float d = b * b - 4 * a * c; // Discriminant from quadratic formula

		        // Number of intersections: 0 when d < 0; 1 when d = 0; 2 when d > 0
		        if (d > 0) {
			        float s = sqrt(d);
			        float dstToSphereNear = max(0, (-b - s) / (2 * a));
			        float dstToSphereFar = (-b + s) / (2 * a);

			        // Ignore intersections that occur behind the ray
			        if (dstToSphereFar >= 0) {
				        return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
			        }
		        }
		        // Ray did not intersect sphere
		        return float2(maxFloat, 0);
	        }

            float densityAtPoint(float3 densitySamplePoint) {
				float heightAboveSurface = length(densitySamplePoint - _planetCenter) - _planetRadius;
				float height1 = heightAboveSurface / (_atmosphereRadius - _planetRadius);
				float localDensity = exp(-height1 * _densityFalloff) * (1 - height1);
				return localDensity;
			}

            float opticalDepth(float3 rayOrigin, float3 rayDir, float rayLength) {
				float3 densitySamplePoint = rayOrigin;
				float stepSize = rayLength / (_opticalDepthPoints - 1);
				float opticalDepth = 0;

				for (int i = 0; i < _opticalDepthPoints; i ++) {
					float localDensity = densityAtPoint(densitySamplePoint);
					opticalDepth += localDensity * stepSize;
					densitySamplePoint += rayDir * stepSize;
				}
				return opticalDepth;
			}
            //
            float4 MainFrag(v2f input) : SV_Target
            {
                float intensity = 1;
                _directionToSun = float3(-1,0,0);

                float2 uv = input.texcoord;
                float4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
                float depth = SampleSceneDepth(uv);
                float viewLength = length(input.viewVector);
                float sceneDepth = LinearEyeDepth(depth, _ZBufferParams) * viewLength;
                float3 rayPos = _WorldSpaceCameraPos;
				float3 rayDir =  normalize(input.viewVector);

                float distanceToOcean = raySphere(_planetCenter, _oceanRadius, rayPos, rayDir);
                float distanceToSurface = min(sceneDepth, distanceToOcean);

                float2 result = raySphere(_planetCenter, _atmosphereRadius, rayPos, rayDir);
                float distanceToAtmosphere = result.x;
                
                float distanceThroughAtmosphere = min(result.y, distanceToSurface - distanceToAtmosphere);
                //float3 rayAtmosphereIntersectPos = rayPos + rayDir * distanceToAtmosphere  - _planetCenter;

                if (distanceThroughAtmosphere > 0)
                {

                    const float epsilon = 0.0001;

                    float3 pointInAtmosphere = rayPos + rayDir * (distanceToAtmosphere + epsilon);
                    // calculate Lighting
                    float3 inScatterPoint = pointInAtmosphere;
                    float rayLength = (distanceThroughAtmosphere - epsilon * 2);
                    float stepSize =  rayLength / (_inScatteringPoints - 1);
                    float3 inScatteredLight = 0;
                    float viewRayOpticalDepth = 0;
                    for (int i = 0; i < _inScatteringPoints; i++)
                    {
                        float sunRayLength = raySphere(_planetCenter, _atmosphereRadius, inScatterPoint, _directionToSun).y;
                        float sunRayOpticalDepth = opticalDepth(inScatterPoint, _directionToSun, sunRayLength);
                        viewRayOpticalDepth = opticalDepth(inScatterPoint, -rayDir, stepSize * i);
                        
                        //return sunRayOpticalDepth;
                        float3 transmittance = exp(-(sunRayOpticalDepth + viewRayOpticalDepth) * _scatteringCoefficients);
                        float localDensity = densityAtPoint(inScatterPoint);
                        inScatteredLight += localDensity * transmittance * _scatteringCoefficients * stepSize * _intensity;
                        
                        inScatterPoint += rayDir * stepSize;
                        
                    }
                    inScatteredLight /= _planetRadius;

                    float sceneColorTransmittance = exp(-viewRayOpticalDepth);
                    float3 finalColor = sceneColor * sceneColorTransmittance + inScatteredLight;
                   return float4(finalColor, 0);
                   // return sceneColor + 
                    
                   //return sceneColor * (1- inScatteredLight) + inScatteredLight;
                }
                return sceneColor;
            }



            ENDHLSL
        }
    }
}
