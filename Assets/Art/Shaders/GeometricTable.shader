Shader "DoubleDown/GeometricTable"
{
    Properties
    {
        _BaseColor ("Felt", Color) = (0.050, 0.215, 0.235, 1)
        _EdgeColor ("Outer felt", Color) = (0.018, 0.063, 0.091, 1)
        _GoldColor ("Brass", Color) = (0.68, 0.47, 0.22, 1)
        _AccentColor ("Centre light", Color) = (0.105, 0.32, 0.32, 1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "GeometricTable"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _EdgeColor;
                half4 _GoldColor;
                half4 _AccentColor;
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
                float2 uv = input.uv;
                float2 centred = (uv - 0.5) * float2(1.7777778, 1.0);
                float radius = length(centred);
                float light = 1.0 - smoothstep(0.08, 1.05, radius);
                half3 colour = lerp(_EdgeColor.rgb, _BaseColor.rgb, light);

                // A restrained diamond weave, drawn entirely from lines.
                float2 tile = abs(frac(uv * float2(18.0, 10.0)) - 0.5);
                float diamonds = 1.0 - smoothstep(0.045, 0.085, abs(tile.x + tile.y - 0.36));
                colour += _AccentColor.rgb * diamonds * 0.045 * light;

                // The central oval and its two brass outlines frame the hand.
                float oval = length((uv - float2(0.5, 0.52)) * float2(1.30, 2.30));
                float ovalFill = 1.0 - smoothstep(0.50, 0.54, oval);
                colour = lerp(colour, _AccentColor.rgb, ovalFill * 0.23);
                float outerRing = 1.0 - smoothstep(0.007, 0.014, abs(oval - 0.56));
                float innerRing = 1.0 - smoothstep(0.003, 0.009, abs(oval - 0.47));
                colour = lerp(colour, _GoldColor.rgb, outerRing * 0.54 + innerRing * 0.14);

                // Fine inset rectangular trim and four corner studs.
                float edge = min(min(uv.x, 1.0 - uv.x) * 1.7777778,
                                 min(uv.y, 1.0 - uv.y));
                float border = 1.0 - smoothstep(0.0025, 0.0065, abs(edge - 0.038));
                colour = lerp(colour, _GoldColor.rgb, border * 0.58);

                float2 corner = abs(uv - 0.5) - float2(0.445, 0.40);
                float stud = 1.0 - smoothstep(0.010, 0.016, length(corner));
                colour = lerp(colour, _GoldColor.rgb, stud * 0.77);

                // Very slow light breathing gives motion without warping shapes.
                float pulse = 0.5 + 0.5 * sin(_Time.y * 0.45);
                colour += _AccentColor.rgb * light * pulse * 0.018;
                return half4(saturate(colour), 1.0);
            }
            ENDHLSL
        }
    }
}
