Shader "DoubleDown/SelectionOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Card mask", 2D) = "white" {}
        _OutlineColor ("Fine outline", Color) = (0.96, 0.74, 0.39, 0.46)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" "CanUseSpriteAtlas"="True" }
        Pass
        {
            Name "SelectionOutline"
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
            float4 _MainTex_TexelSize;
            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
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
                float2 stepUV = _MainTex_TexelSize.xy;
                half centre = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
                half nearby = 0.0h;
                nearby = max(nearby, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(stepUV.x, 0)).a);
                nearby = max(nearby, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - float2(stepUV.x, 0)).a);
                nearby = max(nearby, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, stepUV.y)).a);
                nearby = max(nearby, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - float2(0, stepUV.y)).a);
                half edge = step(0.5h, nearby) * (1.0h - step(0.5h, centre));
                return half4(_OutlineColor.rgb, _OutlineColor.a * edge);
            }
            ENDHLSL
        }
    }
}
