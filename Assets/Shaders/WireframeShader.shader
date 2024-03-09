Shader "Custom/Geometry/Wireframe"
{
    Properties
    {
        [PowerSlider(3.0)]
        _WireframeVal ("Wireframe width", Range(0., 0.5)) = 0.05
        _FrontColor ("Front color", color) = (1., 1., 1., 1.)
        _BackColor ("Back color", color) = (1., 1., 1., 1.)
        [Toggle] _RemoveDiag("Remove diagonals?", Float) = 0.
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }

        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

            // Change "shader_feature" with "pragma_compile" if you want set this keyword from c# code
            #pragma shader_feature __ _REMOVEDIAG_ON

            #include "UnityCG.cginc"

            struct v2g {
                float4 worldPos : SV_POSITION;
            };

            struct g2f {
                float4 pos : SV_POSITION;
                float3 bary : TEXCOORD0;
            };

            v2g vert(appdata_base v) {
                v2g o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream) {
                float3 param = float3(0., 0., 0.);

                #if _REMOVEDIAG_ON
                float EdgeA = length(IN[0].worldPos - IN[1].worldPos);
                float EdgeB = length(IN[1].worldPos - IN[2].worldPos);
                float EdgeC = length(IN[2].worldPos - IN[0].worldPos);

                if(EdgeA > EdgeB && EdgeA > EdgeC)
                    param.y = 1.;
                else if (EdgeB > EdgeC && EdgeB > EdgeA)
                    param.x = 1.;
                else
                    param.z = 1.;
                #endif

                g2f o;
                o.pos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
                o.bary = float3(1., 0., 0.) + param;
                triStream.Append(o);
                o.pos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
                o.bary = float3(0., 0., 1.) + param;
                triStream.Append(o);
                o.pos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
                o.bary = float3(0., 1., 0.) + param;
                triStream.Append(o);
            }

            float _WireframeVal;
            fixed4 _BackColor;

            fixed4 frag(g2f i) : SV_Target {
            if(!any(bool3(i.bary.x < _WireframeVal, i.bary.y < _WireframeVal, i.bary.z < _WireframeVal)))
                 discard;

                return _BackColor;
            }

            ENDCG
        }

        Pass
        {
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

            // Change "shader_feature" with "pragma_compile" if you want set this keyword from c# code
            #pragma shader_feature __ _REMOVEDIAG_ON

            #include "UnityCG.cginc"

            struct v2g {
                float4 worldPos : SV_POSITION;
            };

            struct g2f {
                float4 pos : SV_POSITION;
                float3 bary : TEXCOORD0;
            };

            v2g vert(appdata_base v) {
                v2g o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream) {
                float3 param = float3(0., 0., 0.);

                #if _REMOVEDIAG_ON
                float EdgeA = length(IN[0].worldPos - IN[1].worldPos);
                float EdgeB = length(IN[1].worldPos - IN[2].worldPos);
                float EdgeC = length(IN[2].worldPos - IN[0].worldPos);

                if(EdgeA > EdgeB && EdgeA > EdgeC)
                    param.y = 1.;
                else if (EdgeB > EdgeC && EdgeB > EdgeA)
                    param.x = 1.;
                else
                    param.z = 1.;
                #endif

                g2f o;
                o.pos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
                o.bary = float3(1., 0., 0.) + param;
                triStream.Append(o);
                o.pos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
                o.bary = float3(0., 0., 1.) + param;
                triStream.Append(o);
                o.pos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
                o.bary = float3(0., 1., 0.) + param;
                triStream.Append(o);
            }

            float _WireframeVal;
            fixed4 _FrontColor;

            fixed4 frag(g2f i) : SV_Target {
            if(!any(bool3(i.bary.x <= _WireframeVal, i.bary.y <= _WireframeVal, i.bary.z <= _WireframeVal)))
                 discard;

                return _FrontColor;
            }

            ENDCG
        }
    }
}

// Shader "Custom/Wireframe"
// {
//     Properties
//     {
//         _WireColor("WireColor", Color) = (1,0,0,1)
//         _Color("Color", Color) = (1,1,1,1)
//     }

//     SubShader
//     {
//         Pass
//         {
//             CGPROGRAM
//             #include "UnityCG.cginc"
//             #pragma target 5.0
//             #pragma vertex vert
//             #pragma geometry geom
//             #pragma fragment frag

//             half4 _WireColor, _Color;

//             struct v2g
//             {
//                 float4  pos : SV_POSITION;
//                 float2  uv : TEXCOORD0;
//             };

//             struct g2f
//             {
//                 float4  pos : SV_POSITION;
//                 float2  uv : TEXCOORD0;
//                 float3 dist : TEXCOORD1;
//             };

//             v2g vert(appdata_base v)
//             {
//                 v2g OUT;
//                 OUT.pos = UnityObjectToClipPos(v.vertex);
//                 OUT.uv = v.texcoord;
//                 return OUT;
//             }

//             [maxvertexcount(3)]
//             void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
//             {

//                 float2 WIN_SCALE = float2(_ScreenParams.x/2.0, _ScreenParams.y/2.0);

//                 //frag position
//                 float2 p0 = WIN_SCALE * IN[0].pos.xy / IN[0].pos.w;
//                 float2 p1 = WIN_SCALE * IN[1].pos.xy / IN[1].pos.w;
//                 float2 p2 = WIN_SCALE * IN[2].pos.xy / IN[2].pos.w;

//                 //barycentric position
//                 float2 v0 = p2-p1;
//                 float2 v1 = p2-p0;
//                 float2 v2 = p1-p0;
//                 //triangles area
//                 float area = abs(v1.x*v2.y - v1.y * v2.x);

//                 g2f OUT;
//                 OUT.pos = IN[0].pos;
//                 OUT.uv = IN[0].uv;
//                 OUT.dist = float3(area/length(v0),0,0);
//                 triStream.Append(OUT);

//                 OUT.pos = IN[1].pos;
//                 OUT.uv = IN[1].uv;
//                 OUT.dist = float3(0,area/length(v1),0);
//                 triStream.Append(OUT);

//                 OUT.pos = IN[2].pos;
//                 OUT.uv = IN[2].uv;
//                 OUT.dist = float3(0,0,area/length(v2));
//                 triStream.Append(OUT);

//             }

//             half4 frag(g2f IN) : COLOR
//             {
//                 //distance of frag from triangles center
//                 float d = min(IN.dist.x, min(IN.dist.y, IN.dist.z));
//                 //fade based on dist from center
//                 float I = exp2(-4.0*d*d);

//                 return lerp(_Color, _WireColor, I);              
//             }
//             ENDCG
//         }
//     }
// }