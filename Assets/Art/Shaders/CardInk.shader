Shader "DoubleDown/CardInk"
{
    Properties
    {
        [PerRendererData] _MainTex ("Card", 2D) = "white" {}
        _InkColor ("Diamond ink", Color) = (0.95, 0.58, 0.15, 1)
        _Recolor ("Recolor diamonds", Float) = 0
        _Edition ("Edition: foil / gold / prism", Range(0,3)) = 0
        _EditionStrength ("Edition strength", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" "CanUseSpriteAtlas"="True" }
        Pass
        {
            Name "CardInk"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _InkColor;
                half _Recolor;
                half _Edition;
                half _EditionStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 pixel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half redInk = step(0.90h, pixel.r) * (1.0h - step(0.30h, pixel.g)) *
                              (1.0h - step(0.40h, pixel.b));
                pixel.rgb = lerp(pixel.rgb, _InkColor.rgb, redInk * _Recolor);
                // Restrict edition colour to pale paper so ranks and suits stay readable.
                half paper = step(0.72h, min(pixel.r, min(pixel.g, pixel.b)));
                float2 cell = floor(input.uv * 64.0);
                if (_Edition > 1.5h && _Edition < 2.5h)
                {
                    // Three moving colour bands, sampled on a fixed pixel grid.
                    // White card paper receives the tint; rank and suit ink stay intact.
                    float phase = frac((cell.x + cell.y * 0.55) / 42.0 - floor(_Time.y * 6.0) / 36.0);
                    half3 cyan = half3(0.55h, 0.89h, 0.96h);
                    half3 rose = half3(0.99h, 0.66h, 0.86h);
                    half3 amber = half3(1.0h, 0.87h, 0.55h);
                    half3 tint = phase < 0.3333 ? lerp(cyan, rose, phase * 3.0) :
                        phase < 0.6667 ? lerp(rose, amber, (phase - 0.3333) * 3.0) :
                        lerp(amber, cyan, (phase - 0.6667) * 3.0);
                    half glint = step(0.92h, phase) * 0.12h;
                    pixel.rgb = lerp(pixel.rgb, saturate(tint + glint), paper * _EditionStrength * 0.55h);
                }
                else
                {
                    half band = step(0.75h, frac(cell.x * 3.0 / 64.0 + cell.y * 2.0 / 64.0));
                    half3 foil = lerp(half3(0.52h,0.82h,0.92h),half3(1.0h,0.77h,0.35h), saturate(_Edition-1));
                    pixel.rgb = lerp(pixel.rgb, pixel.rgb * foil, paper * saturate(_Edition) * _EditionStrength * (0.10h + band * 0.16h));
                }
                return pixel * input.color;
            }
            ENDHLSL
        }
    }
}
