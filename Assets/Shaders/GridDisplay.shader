Shader "Cosmobot/Unlit/GridDisplay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GridColor ("Grid Color", Color) = (1,1,1,1)
        _CellSize ("Cell Size", Vector) = (1,1,1)
        _Offset ("Offset", Vector) = (0,0,0)
        _Thickness ("Thickness", Float) = 0.05
        _Radius ("Radius", Float) = 3
        _FadeStrength ("Fade Strength", Range(0.01,1)) = 0.5
        _Center ("Center", Vector) = (0,0,0)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 wPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GridColor;
            float2 _CellSize;
            float2 _Offset;
            float _Thickness;
            float _Radius;
            float _FadeStrength;
            float3 _Center;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.wPosition = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            bool isOnGrid(float2 pos)
            {
                float xCorrection = 0;
                float yCorrection = 0;
                if(pos.x + _Offset.x + _Thickness/2 < 0) xCorrection = _Thickness;
                if(pos.y + _Offset.y + _Thickness/2 < 0) yCorrection = _Thickness;
                if(fmod(abs(pos.x + _Offset.x + _Thickness/2) + xCorrection, _CellSize.x/2) <= _Thickness || fmod(abs(pos.y + _Offset.y + _Thickness/2) + yCorrection, _CellSize.y/2) <= _Thickness)
                    return true;
                return false;
            }


            float fadeCircle(float2 pos)
            {
                float x = distance(_Center.xz, pos)/_Radius * (1 + (1 - _FadeStrength)/_FadeStrength) - (1 - _FadeStrength)/_FadeStrength;
                return clamp(pow(1-x, 5), 0, 1);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;
                col = tex2D(_MainTex, i.wPosition.xz) * _GridColor * isOnGrid(i.wPosition.xz);
                col.a *= fadeCircle(i.wPosition.xz);
                return col;
            }
            ENDCG
        }
    }
}
