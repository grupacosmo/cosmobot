Shader "Water/Waer"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _WaveHeight ("Wave Height", Range(0,1)) = 0.5
        _WaveSpeed ("Wave Speed", Range(0,10)) = 2
        _WaveLength ("Wave Length", float) = 3
        _WaveCount ("Wave Detail Level", int) = 8
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenSpace : TEXCOORD0;
            };

            float4 _Color;
            float _WaveHeight;
            float _WaveSpeed;
            float _WaveLength;
            int _WaveCount;

            v2f vert (appdata v)
            {
                v2f o;

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                          
                float wave = 0;
                float waveL = 2/_WaveLength;
                float waveH = _WaveHeight/2;
                float2 dir;

                for (int i = 0; i < _WaveCount; i++)
                {
                    dir.x = cos(i * _WaveCount);
                    dir.y = sin(i * _WaveCount);
                    dir = normalize(dir);
                    wave += waveH * sin(mul(dir, worldPos.xz) * waveL + _Time.y * _WaveSpeed * waveL);

                    waveH *= 0.727;
                    waveL *= 1.337;
                }

                v.vertex.y += wave;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.screenSpace = ComputeScreenPos(o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = _Color;
                return col;
            }
            ENDHLSL
        }
    }
}