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
        _State ("Round state", Range(-1,1)) = 0
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
                float _State;
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
                float2 p = uv * float2(1.7777778, 1.0);
                float time = _Time.y * _Speed;
                // Slanted, warped colour bands. Every contour stays continuous
                // across the play field; no polar angle or visible vortex centre.
                float2 flow = p;
                flow.x += 0.10 * sin(5.0 * p.y + 1.6 * p.x - time * 0.16);
                flow.y += 0.13 * sin(4.0 * p.x - 3.3 * p.y + time * 0.12);
                flow += 0.045 * float2(
                    sin(12.0 * flow.y + 6.0 * flow.x + time * 0.22),
                    cos(11.0 * flow.x - 5.0 * flow.y - time * 0.15));
                float ribbons = sin(9.0 * flow.x + 5.0 * flow.y +
                    1.4 * sin(3.8 * flow.y - 2.6 * flow.x + time * 0.10) - time * 0.18);
                float veins = sin(17.0 * flow.x - 7.0 * flow.y - time * 0.11);
                float field = 0.46 + 0.29 * ribbons + 0.10 * veins +
                    0.10 * sin(5.4 * flow.y - 2.4 * flow.x + time * 0.13);
                field += _Pulse * 0.13;
                field = floor(saturate(field) * 9.0 + 0.5) / 9.0;

                half3 colour = lerp(_Night.rgb, _Teal.rgb, saturate(field / 0.34));
                colour = lerp(colour, _Blue.rgb, saturate((field - 0.34) / 0.24));
                colour = lerp(colour, _Coral.rgb, saturate((field - 0.58) / 0.23));
                colour = lerp(colour, _Gold.rgb, saturate((field - 0.81) / 0.19));

                // The cards sit on a calm centre, while the colour lives in the edges.
                float tableMask = 1.0 - smoothstep(0.16, 0.65,
                    length((uv - float2(0.34, 0.49)) * float2(1.2, 1.0)));
                colour *= lerp(0.82, 0.48, tableMask);
                colour = floor(colour * 24.0 + 0.5) / 24.0;
                colour = lerp(colour, colour * half3(1.22, 0.88, 0.65), max(0, _State) * 0.35);
                colour = lerp(colour, colour * half3(0.65, 0.70, 0.88), max(0, -_State) * 0.55);
                return half4(colour, 1.0);
            }
            ENDHLSL
        }
    }
}
