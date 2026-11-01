Shader "DoubleDown/CardInk"
{
    Properties
    {
        [PerRendererData] _MainTex ("Card", 2D) = "white" {}
        _InkColor ("Diamond ink", Color) = (0.95, 0.58, 0.15, 1)
        _Recolor ("Recolor diamonds", Float) = 0
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
                return pixel * input.color;
            }
            ENDHLSL
        }
    }
}
