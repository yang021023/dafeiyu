Shader "Custom/Sonar2D"
{
    Properties
    {
        _DarkColor   ("暗色", Color) = (0, 0, 0, 0.97)
        _WaveColor   ("波前颜色", Color) = (0.55, 1, 1, 1)
        _Center      ("圆心（世界坐标）", Vector) = (0, 0, 0, 0)
        _LightRadius ("常亮半径", Float) = 3
        _WaveRadius  ("波前半径", Float) = 4
        _RingWidth   ("波前宽度", Float) = 0.5
        _TrailLength ("余晖长度", Float) = 5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings  { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };

            CBUFFER_START(UnityPerMaterial)
                float4 _DarkColor;
                float4 _WaveColor;
                float4 _Center;
                float  _LightRadius;
                float  _WaveRadius;
                float  _RingWidth;
                float  _TrailLength;
            CBUFFER_END

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 ws = TransformObjectToWorld(v.positionOS.xyz);
                o.positionWS = ws;
                o.positionCS = TransformWorldToHClip(ws);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float d = distance(i.positionWS.xy, _Center.xy);

                // ① 常亮圈
                float lit = 1.0 - smoothstep(_LightRadius - 0.2, _LightRadius + 0.2, d);

                // ② 波后余晖：波前扫过越多，越亮；越远越暗
                float behind = _WaveRadius - d;
                float trail  = saturate(behind / max(_TrailLength, 0.01));

                // ③ 波前亮环（高斯衰减）
                float ring = exp(-pow((d - _WaveRadius) / max(_RingWidth, 0.01), 2.0));

                // 合成：可见度越高，黑幕越淡
                float visible  = max(lit, trail * 0.85);
                float darkness = 1.0 - visible;

                half3 col = lerp(_DarkColor.rgb, _WaveColor.rgb, saturate(ring));

                half4 outc;
                outc.rgb = col;
                outc.a   = saturate(max(darkness, ring));   // 亮环会盖过黑暗
                return outc;
            }
            ENDHLSL
        }
    }
    Fallback Off
}