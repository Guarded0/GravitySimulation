Shader "Unlit/TestShader"
{
    Properties
    {
        _BlackHoleRadius ("Radius of the black part relative to size", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"    

			struct Attributes
			{
				float4 posOS	: POSITION;
			};

			struct v2f
			{
				float4 posCS		: SV_POSITION;
				float3 posWS		: TEXCOORD0;

				float3 center		: TEXCOORD1;
				float3 objectScale	: TEXCOORD2;
			};

			static const float maxFloat = 3.402823466e+38;

            v2f vert (Attributes IN)
            {
				v2f OUT = (v2f)0;

				VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.posOS.xyz);

				OUT.posCS = vertexInput.positionCS;
				OUT.posWS = vertexInput.positionWS;

				// Object information, based upon Unity's shadergraph library functions
				OUT.center = UNITY_MATRIX_M._m03_m13_m23;
				OUT.objectScale = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)));

				return OUT;
            }
			
			
			
			
			float _BlackHoleRadius;

            // Based upon https://viclw17.github.io/2018/07/16/raytracing-ray-sphere-intersection/#:~:text=When%20the%20ray%20and%20sphere,equations%20and%20solving%20for%20t.
			// Returns dstToSphere, dstThroughSphere
			// If inside sphere, dstToSphere will be 0
			// If ray misses sphere, dstToSphere = max float value, dstThroughSphere = 0
			// Given rayDir must be normalized
			float2 intersectSphere(float3 rayOrigin, float3 rayDir, float3 center, float radius) {

				float3 offset = rayOrigin - center;
				const float a = 1;
				float b = 2 * dot(offset, rayDir);
				float c = dot(offset, offset) - radius * radius;

				float discriminant = b * b - 4 * a*c;
				// No intersections: discriminant < 0
				// 1 intersection: discriminant == 0
				// 2 intersections: discriminant > 0
				if (discriminant > 0) {
					float s = sqrt(discriminant);
					float dstToSphereNear = max(0, (-b - s) / (2 * a));
					float dstToSphereFar = (-b + s) / (2 * a);

					if (dstToSphereFar >= 0) {
						return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
					}
				}
				// Ray did not intersect sphere
				return float2(maxFloat, 0);
			}

			float2 MirrorUVCoordinates(float2 UVs)
			{
				float2 NewUVs = float2(0,0);
				// Mirror U coordinate
				if (UVs.x < 0.0 || UVs.x > 1.0)
					NewUVs.x = 1.0 - abs((UVs.x - 2.0 * floor(UVs.x / 2.0)) - 1.0);
				else
					NewUVs.x = UVs.x;

				// Mirror V coordinate
				if (UVs.y < 0.0 || UVs.y > 1.0)
					NewUVs.y = 1.0 - abs((UVs.y - 2.0 * floor(UVs.y / 2.0)) - 1.0);
				else
					NewUVs.y = UVs.y;

				return NewUVs;
			}



            float4 frag (v2f i) : SV_Target
            {
                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.posWS - _WorldSpaceCameraPos);

                float sphereRadius = i.objectScale.x * _BlackHoleRadius;
                float sphereCenter = i.center;

				float2 screenUV = (i.posCS.xy + normalize(ComputeScreenPos(sphereCenter) - i.posWS) * 5000)  / _ScreenParams.xy;

				
				float4 sceneColor = float4(SampleSceneColor(MirrorUVCoordinates(screenUV)), 1);
                // sample the texture
                float2 sphereIntersection = intersectSphere(rayOrigin, rayDir, i.center, sphereRadius);
				float4 col;

			
				if (sphereIntersection.y>0)
				{
					col = float4(0,0,0,0);
				}else{
					col = sceneColor;
				}
             
                return col;
            }
            ENDHLSL
        }
    }
}
