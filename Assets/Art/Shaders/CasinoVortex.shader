Shader "DoubleDown/CasinoVortex"
{
    Properties
    {
        _Night ("Deep navy", Color) = (0.012, 0.029, 0.060, 1)
        _Teal ("Petrol", Color) = (0.025, 0.20, 0.25, 1)
        _Blue ("Electric blue", Color) = (0.075, 0.37, 0.57, 1)
        _Coral ("Coral", Color) = (0.72, 0.18, 0.21, 1)
        _Gold ("Warm highlight", Color) = (0.95, 0.49, 0.25, 1)
        _Speed ("Flow speed", Range(0, 2)) = 0.55
        _Pulse ("Score pulse", Range(0, 1)) = 0
        _PixelGrid ("Pixel grid XY", Vector) = (320, 180, 0, 0)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "CasinoVortex"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Night;
                half4 _Teal;
                half4 _Blue;
                half4 _Coral;
                half4 _Gold;
                float _Speed;
                float _Pulse;
                float4 _PixelGrid;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // One sample per 2x2 logical pixels at the 640x360 reference size.
                float2 cell = floor(input.uv * _PixelGrid.xy);
                float2 uv = (cell + 0.5) / _PixelGrid.xy;
                float2 p = (uv - 0.5) * float2(1.7777778, 1.0);
                float radius = length(p);
                float angle = atan2(p.y, p.x);
                float time = _Time.y * _Speed;

                // Domain warp and spiral ribbons. The bounded field uses
                // independent waves, without source code from the reference.
                float spiral = angle + 3.35 * radius - 0.16 * time;
                float2 flow = p + 0.18 * float2(
                    cos(3.0 * spiral + 0.37 * time),
                    sin(4.0 * spiral - 0.21 * time));
                flow += 0.10 * float2(
                    sin(7.0 * flow.y + 3.0 * flow.x + 0.13 * time),
                    cos(6.0 * flow.x - 4.0 * flow.y - 0.17 * time));

                float ribbon = sin(5.0 * spiral + 4.0 * flow.x - 2.0 * flow.y +
                                   0.65 * sin(7.0 * flow.y + 0.2 * time) - 0.34 * time);
                float tide = sin(4.6 * flow.x + 3.8 * flow.y + 0.23 * time) *
                             cos(3.1 * flow.y - 2.2 * flow.x - 0.18 * time);
                float field = 0.48 + 0.29 * ribbon + 0.18 * tide +
                              0.10 * sin(9.0 * radius - 2.0 * spiral + 0.11 * time);
                field += _Pulse * (1.0 - saturate(radius)) * 0.14;
                field = floor(saturate(field) * 7.0 + 0.5) / 7.0;

                half3 colour = lerp(_Night.rgb, _Teal.rgb, saturate(field / 0.34));
                colour = lerp(colour, _Blue.rgb, saturate((field - 0.34) / 0.24));
                colour = lerp(colour, _Coral.rgb, saturate((field - 0.58) / 0.23));
                colour = lerp(colour, _Gold.rgb, saturate((field - 0.81) / 0.19));

                // Restrict brightness behind cards and keep a limited palette.
                float vignette = 1.0 - smoothstep(0.48, 1.15, radius);
                colour *= lerp(0.50, 0.88, vignette);
                colour = floor(colour * 24.0 + 0.5) / 24.0;
                return half4(colour, 1.0);
            }
            ENDHLSL
        }
    }
}
