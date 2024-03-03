Shader "Unlit/WaterShader"
{
    Properties
    { 
        _Rim ("Rim", Range(0, 1)) = 0.2
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _Roughness ("Roughness", Range(0, 1)) = 0.01
        _Albedo ("Albedo", Color) = (0.1, 0.3, 0.5, 1)
        _Amplitude ("Amplitude", Range(0, 1)) = 0.3
        _WaveTexture ("Wave Texture", 2D) = "white" {}
        _Speed ("Speed", Range(-200, 200)) = 100
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vertexFunc
            #pragma exclude_renderers gles xbox360 ps3
            #pragma fragment fragmentFunc

            #include "UnityCG.cginc"

            struct vertexInput
            {
                float4 vertex : POSITION;
            };

            struct vertexOutput
            {
                float4 pos : POSITION;
                float4 color : COLOR;
            };

            float _Rim;
            float _Metallic;
            float _Roughness;
            float4 _Albedo;
            float _Amplitude;
            sampler2D _WaveTexture;
            float _Speed;

            float wave(float2 position)
            {
                float2 wv = 1.0 - abs(sin(position));
                return pow(1.0 - pow(wv.x * wv.y, 0.65), 4.0);
            }

            float height(float2 position, float time)
            {
                float d = wave((position + time) * 0.4) * _Amplitude;
                d += wave((position - time) * 0.3) * _Amplitude;
                d += wave((position + time) * 0.5) * _Amplitude;
                d += wave((position - time) * 0.6) * _Amplitude;
                return d;
            }

            vertexOutput vertexFunc(vertexInput v)
            {
                vertexOutput o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float2 pos = v.vertex.xz / 2.0 + 0.5;
                float time = _Time.y * 0.3;
                o.pos.y = height(pos, time);
                float3 normal = normalize(float3(o.pos.y - height(pos + float2(0.1, 0.0), time), 0.1, o.pos.y - height(pos + float2(0.0, 0.1), time)));
                o.color.rgb = _Albedo.rgb * (0.2 * sqrt(1.0 - dot(normal, float3(0,1,0))));
                return o;
            }

            float4 fragmentFunc(vertexOutput i) : COLOR
            {
                float fresnel = sqrt(1.0 - dot(i.color.rgb, float3(0,1,0)));
                return _Albedo * (0.2 * fresnel);
            }

            ENDCG
        }
    }
}