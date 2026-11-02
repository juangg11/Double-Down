Shader "DoubleDown/PixelGlass"
{
    Properties
    {
        [PerRendererData] _MainTex ("UI texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0,1)) = 0.25
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Cull Off ZWrite Off ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            float _Intensity;
            struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            V Vert(A v) { V o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz); o.uv=v.uv; return o; }
            half4 Frag(V v):SV_Target
            {
                float2 p=v.uv*2-1;
                float edge=smoothstep(0.45,1.35,length(p*float2(0.75,1)));
                float scan=fmod(floor(v.uv.y*360),2);
                return half4(0.005,0.012,0.02,saturate(_Intensity)*(edge*0.3+scan*0.12));
            }
            ENDHLSL
        }
    }
}
