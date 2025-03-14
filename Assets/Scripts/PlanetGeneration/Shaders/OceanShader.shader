Shader "Hidden/OceanShader"
{
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "OceanPass"

            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment MainFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

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
                float3 viewVector = mul(unity_CameraInvProjection, float4(uv * 2 - 1, 0, -1));
                output.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                return output;
            }


            float4 _colorA;
            float4 _colorB;
            float _depthMultiplier;
            float _alphaMultiplier;
            float _oceanRadius;
            float3 _planetPosition;




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


            //
            float4 MainFrag(v2f input) : SV_Target
            {
                float2 uv = input.texcoord;
                float4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
                float depth = SampleSceneDepth(uv);
                float viewLength = length(input.viewVector);
                float sceneDepth = LinearEyeDepth(depth, _ZBufferParams) * viewLength;

                float3 sphereCenter = _planetPosition;//


                float3 rayPos = _WorldSpaceCameraPos;

				float3 rayDir = input.viewVector / viewLength;
                float2 result = raySphere(sphereCenter, _oceanRadius, rayPos, rayDir);
                float distanceToOcean = result.x;
                float distanceThroughOcean = result.y;
                float3 rayOceanIntersectPos = rayPos + rayDir * distanceToOcean  - sphereCenter;


                // dst that view ray travels through ocean (before hitting terrain / exiting ocean)
				float oceanViewDepth = min(distanceThroughOcean, sceneDepth - distanceToOcean);
                //return float4(sceneDepth/100,0,0,1);
                if (oceanViewDepth > 0)
                {

                    float opticalDepth = 1 - exp(-oceanViewDepth * _depthMultiplier);
                    float alpha = 1 - exp(-oceanViewDepth * _alphaMultiplier);
                    float4 oceanColor = lerp(_colorA, _colorB, opticalDepth);
                    return lerp(sceneColor, oceanColor, alpha);
                }
                //
                return sceneColor;
            }



            ENDHLSL
        }
    }
}
