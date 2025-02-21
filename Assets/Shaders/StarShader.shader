// This shader fills the mesh shape with a color predefined in the code.
Shader "Example/URPUnlitShaderBasic"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    { 
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1) // A color property
        _NoiseScale ("Noise Scale", Float) = 1.0        // A float property
        _NoiseSpeed ("Noise Speed", Float) = 1.0        // A float property
        }

    // The SubShader block containing the Shader code.
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            // This line defines the name of the vertex shader.
            #pragma vertex vert
            // This line defines the name of the fragment shader.
            #pragma fragment frag

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                float4 positionOS : POSITION; // Object space position
            };
            struct v2f
            {
                float4 positionHCS : SV_POSITION; // Clip space position
                float3 worldPos : TEXCOORD0;     // World space position
            };

            float4 _BaseColor;
            float _NoiseScale;
            float _NoiseSpeed;


            // The vertex shader definition with properties defined in the v2f
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            v2f vert(Attributes IN)
            {
                v2f OUT;
                // Transform object space position to clip space
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // Transform object space position to world space
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            float random (float2 _st) {
                return frac(sin(dot(_st.xy,
                         float2(12.9898,78.233)))*
                    43758.5453123);
            }
            // Based on Morgan McGuire @morgan3d
            // https://www.shadertoy.com/view/4dS3Wd
            float noise (float2 _st) {
                float2 i = floor(_st);
                float2 f = frac(_st);

                // Four corners in 2D of a tile
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x) +
                        (c - a)* u.y * (1.0 - u.x) +
                        (d - b) * u.x * u.y;
            }

            #define NUM_OCTAVES 5
            float fbm (float2 _st) {
                float v = 0.0;
                float a = 0.5;
                float2 shift = float2(100.0,100.0);
                // Rotate to reduce axial bias
                float2x2 rot = float2x2(cos(0.5), sin(0.5),
                                -sin(0.5), cos(0.50));
                for (int i = 0; i < NUM_OCTAVES; ++i) {
                    v += a * noise(_st);
                    _st = mul(rot, _st) * 2.0 + shift;
                    a *= 0.5;
                }
                return v;
             }

            // The fragment shader definition.
                        half4 frag(v2f i) : SV_Target
            {
                // Use world space position for 3D noise
                float3 worldPos = i.worldPos;

                // Scale the world position to control the texture density
                float3 st = worldPos * 0.5;

                float3 color = float3(0.0, 0.0, 0.0);

                float3 q = float3(0.0, 0.0, 0.0);
                q.x = fbm(st + 0.00 * _Time.y);
                q.y = fbm(st + float3(1.0, 1.0, 1.0));

                float3 r = float3(0.0, 0.0, 0.0);
                r.x = fbm(st + 1.0 * q + float3(1.7, 9.2, 3.3) + _NoiseSpeed * _Time.y);
                r.y = fbm(st + 1.0 * q + float3(8.3, 2.8, 1.4) + _NoiseSpeed * _Time.y);

                float f = fbm(st + r);
                for (int i = 0; i < 50; i++)
                {
                    f = fbm(f + r);
                }

                color = lerp(float3(2.101961, 0.619608, 0.666667),
                             float3(0.666667, 0.666667, 0.498039),
                             clamp((f * f) * 4.0, 0.0, 1.0));

                color = lerp(color,
                             float3(0, 0, 0.164706),
                             clamp(length(q), 0.0, 1.0));

                color = lerp(color,
                             float3(0.666667, 1, 1),
                             clamp(length(r.x), 0.0, 1.0));

                float randomShi = (f * f * f + 0.6 * f * f + 0.5 * f);
                half4 customColor = half4((randomShi * color).xyz, 1);
                return customColor;
            }
            ENDHLSL
        }
    }
}