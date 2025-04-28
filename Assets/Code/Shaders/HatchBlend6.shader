Shader "URP/Custom/SingleObjectHatch"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Hatch0   ("Hatch Texture 0", 2D) = "white" {}
        _Hatch1   ("Hatch Texture 1", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos    : TEXCOORD2;
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_Hatch0);  SAMPLER(sampler_Hatch0);
            TEXTURE2D(_Hatch1);  SAMPLER(sampler_Hatch1);
            float4 _MainTex_ST;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(float4(worldPos,1));
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normalOS);
                OUT.worldPos    = worldPos;
                return OUT;
            }

            float3 HatchingConstantScale(float2 uv, float intensity, float dist)
            {
                float log2d = log2(dist);
                float2 floorLog = floor((log2d + float2(0,1))*0.5)*2 - float2(0,1);
                float2 uvScale  = min(1, pow(2, floorLog));
                float blend     = abs(frac(log2d*0.5)*2 - 1);

                float2 uvA = uv / uvScale.x;
                float2 uvB = uv / uvScale.y;

                float3 h0A = SAMPLE_TEXTURE2D(_Hatch0, sampler_Hatch0, uvA).rgb;
                float3 h1A = SAMPLE_TEXTURE2D(_Hatch1, sampler_Hatch1, uvA).rgb;
                float3 h0B = SAMPLE_TEXTURE2D(_Hatch0, sampler_Hatch0, uvB).rgb;
                float3 h1B = SAMPLE_TEXTURE2D(_Hatch1, sampler_Hatch1, uvB).rgb;

                float3 hatch0 = lerp(h0A, h0B, blend);
                float3 hatch1 = lerp(h1A, h1B, blend);
                float3 overb  = max(0, intensity - 1);

                float3 wA = saturate(intensity*6 + float3(0,-1,-2));
                float3 wB = saturate(intensity*6 + float3(-3,-4,-5));
                wA.xy -= wA.yz; wA.z -= wB.x; wB.xy -= wB.yz;

                hatch0 *= wA; hatch1 *= wB;
                return overb + hatch0.r + hatch0.g + hatch0.b + hatch1.r + hatch1.g + hatch1.b;
            }

            float3 Hatching(float2 uv, float intensity)
            {
                float3 h0 = SAMPLE_TEXTURE2D(_Hatch0, sampler_Hatch0, uv).rgb;
                float3 h1 = SAMPLE_TEXTURE2D(_Hatch1, sampler_Hatch1, uv).rgb;
                float3 overb = max(0, intensity - 1);

                float3 wA = saturate(intensity*6 + float3(0,-1,-2));
                float3 wB = saturate(intensity*6 + float3(-3,-4,-5));
                wA.xy -= wA.yz; wA.z -= wB.x; wB.xy -= wB.yz;

                h0 *= wA; h1 *= wB;
                return overb + h0.r + h0.g + h0.b + h1.r + h1.g + h1.b;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).rgb;

                // URP’s main directional light
                Light mainLight = GetMainLight();
                float3 L       = mainLight.direction;
                float3 LC      = mainLight.color.rgb;
                float  NdotL   = max(dot(normalize(IN.worldNormal), L), 0);

                // diffuse + luminance
                float3 diff      = baseCol * LC * NdotL;
                float  intensity = dot(diff, float3(0.2326,0.7152,0.0722));

                // camera-distance scaling using built-in global
                float camDist = distance(_WorldSpaceCameraPos, IN.worldPos)
                              * unity_CameraInvProjection[0][0];

                // pick your hatching version!
                float3 hatch = HatchingConstantScale(IN.uv * 3, intensity, camDist);
                // or: float3 hatch = Hatching(IN.uv * 8, intensity);

                return half4(hatch, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
