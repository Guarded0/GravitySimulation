// This shader fills the mesh shape with a color predefined in the code.
Shader "Unlit_GasShader"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    { 
        _Temperature ("Temperature", int) = 1
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1) // A color property
        _NoiseScale ("Noise Scale", Float) = 1.0        // A float property
        _NoiseSpeed ("Noise Speed", Float) = 1.0        // A float property
        _Brightness ("Brightness", Float) = 1.0

        // Add new color properties for lerping
        [HDR] _LerpColor1 ("Lerp Color 1", Color) = (0.666667,0.666667,0.498039, 1) // First lerp color
        [HDR] _LerpColor2 ("Lerp Color 2", Color) = (0, 0, 0.164706, 1) // Second lerp color
        [HDR] _LerpColor3 ("Lerp Color 3", Color) = (0.666667, 1, 1, 1) // Third lerp color

        }

    // The SubShader block containing the Shader code.
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

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
                float3 normalOS : NORMAL;
                float2 uv         : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION; // Clip space position
                float3 worldPos : TEXCOORD0;     // World space position
                float3 viewVector : TEXCOORD1; // View vector
                float3 normalWS : TEXCOORD2;
                float4 positionOS : TEXCOORD3;
            };
            int _Temperature;
            float4 _BaseColor;
            float _NoiseScale;
            float _NoiseSpeed;
            float _Brightness;
            float4 _LerpColor1;
            float4 _LerpColor2;
            float4 _LerpColor3;


            // The vertex shader definition with properties defined in the Varyings
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // Transform object space position to clip space
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // Transform object space position to world space
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);

                float3 viewVector = mul(unity_CameraInvProjection, float4(IN.uv.xy * 2 - 1, 0, -1));
                OUT.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionOS = IN.positionOS;
                return OUT;
            }

            float3 sampleGradientColor(int value)
            {
                float3 colors[6] = {float3(0,0,0),float3(1,0,0), float3(1,0.5,0),float3(1,1,0),float3(1, 1, 1),float3(0,0,1)};
                int positions[6] = {0, 3000, 4000, 6000, 10000, 25000};
                for(int i = 1; i < 6; i++)
                {
                    if(value < positions[i])
                    {
                        float position = (float)(value - positions[i - 1]) / (float)(positions[i] - positions[i - 1]);
                        return lerp(colors[i-1], colors[i], position);
                    }
                }
                return float3(0,0,0);
            }

            float random(float3 _st)
            {
                return frac(sin(dot(_st.xyz, float3(12.9898, 78.233, 45.543))) * 43758.5453123);
            }
            // Based on Morgan McGuire @morgan3d
            // https://www.shadertoy.com/view/4dS3Wd
            //float noise (float2 _st) {
            //   float2 i = floor(_st);
            //    float2 f = frac(_st);

                // Four corners in 2D of a tile
            //    float a = random(i);
            //    float b = random(i + float2(1.0, 0.0));
            //    float c = random(i + float2(0.0, 1.0));
            //    float d = random(i + float2(1.0, 1.0));

            //    float2 u = f * f * (3.0 - 2.0 * f);

            //    return lerp(a, b, u.x) +
            //            (c - a)* u.y * (1.0 - u.x) +
            //            (d - b) * u.x * u.y;
            // }
            float hash(float n)
            {
             return frac(sin(n) * 43758.5453);
            }
            float noise(float3 x) {
                float3 step = float3(110, 241, 171);

                float3 i = floor(x);
                float3 f = frac(x);
 
                // For performance, compute the base input to a 1D hash from the integer part of the argument and the 
                // incremental change to the 1D based on the 3D -> 1D wrapping
                float n = dot(i, step);

                float3 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(lerp( hash(n + dot(step, float3(0, 0, 0))), hash(n + dot(step, float3(1, 0, 0))), u.x),
                                lerp( hash(n + dot(step, float3(0, 1, 0))), hash(n + dot(step, float3(1, 1, 0))), u.x), u.y),
                            lerp(lerp( hash(n + dot(step, float3(0, 0, 1))), hash(n + dot(step, float3(1, 0, 1))), u.x),
                                lerp( hash(n + dot(step, float3(0, 1, 1))), hash(n + dot(step, float3(1, 1, 1))), u.x), u.y), u.z);
            }


            #define NUM_OCTAVES 5
            float fbm (float3 _st) {
                float v = 0.0;
                float a = 0.5;
                float3 shift = float3(100.0,100.0,100.0);
                
                for (int i = 0; i < NUM_OCTAVES; ++i) {
                    v += a * noise(_st);
                    _st =  _st * 2.0 + shift;
                    a *= 0.5;
                }
                return v;
             }
            // The fragment shader definition.
            float4 frag(Varyings input) : SV_Target
            {
                // Use world space position for 3D noise
                float3 worldPos = input.worldPos;
                
                // Scale the world position to control the texture density
                float3 st = worldPos * _NoiseScale;
                float layer = noise(float3(0, input.positionOS.y, 0) * 50) * noise(float3(0, input.positionOS.y, 0) * 30);
                layer = pow(layer, 3);
                float3 color = _BaseColor;
                color = float3(clamp(color.x, 0,1), clamp(color.y, 0,1), clamp(color.z, 0,1));

                float3 q = float3(0.0, 0.0, 0.0);
                q.x = fbm(st + 0.00 * _Time.y);
                q.y = fbm(st + float3(1.0, 1.0, 1.0));
                float3 r = float3(0.0, 0.0, 0.0);
                r.x = fbm(st + 5.0 * layer * q + float3(1.7, 9.2, 3.3)* layer + _NoiseSpeed * _Time.y);
                r.y = fbm(st + 5.0 * layer * q + float3(8.3, 2.8, 1.4) * layer+ _NoiseSpeed * _Time.y);
                
                //if (layer > 0.5)
                //{
                //    r.x = fbm(st + 10.0 * q + float3(1.7, 9.2, 3.3) + _NoiseSpeed * _Time.y);
                //    r.y = fbm(st + 10.0 * q + float3(8.3, 2.8, 1.4) + _NoiseSpeed * _Time.y);

                //}
                //else{
                //    r.x = fbm(st + 1.0 * q + float3(1.7, 9.2, 3.3) + _NoiseSpeed * _Time.y);
                //    r.y = fbm(st + 1.0 * q + float3(8.3, 2.8, 1.4) + _NoiseSpeed * _Time.y);
                //}


                float f = fbm(st + r);
                
                float randomShi = (f * f * f + 0.6 * f * f + 0.5 * f);
                float4 customColor = float4((randomShi * color).xyz * _Brightness, 1);
                //return float4(1,1,1,1) * layer;
                return customColor;
            }
            ENDHLSL
        }
    }
}