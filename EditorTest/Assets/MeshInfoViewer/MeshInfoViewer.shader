Shader "Custom/MeshInfoViewer"
{
    Properties
    {
        [KeywordEnum(UV0, UV1, VertexColorRGB, VertexColorAlpha, WorldNormal)]_MeshInfo("Mesh Info", float)     = 0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           #pragma multi_compile _MESHINFO_UV0 _MESHINFO_UV1 _MESHINFO_VERTEXCOLORRGB _MESHINFO_VERTEXCOLORALPHA _MESHINFO_WORLDNORMAL
            
           #include "UnityCG.cginc"
            struct appdata
            {
                float4 vertex       : POSITION;
                float2 texcoord0    : TEXCOORD0;
                float2 texcoord1    : TEXCOORD1;
                float4 color        : COLOR;
                float4 normal       : NORMAL;
            };
            struct v2f
            {
                float4 vertex       : SV_POSITION;
                float2 uv0          : TEXCOORD0;
                float2 uv1          : TEXCOORD1;
                float4 color        : COLOR;
                float3 normal       : NORMAL;
            };
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex    = UnityObjectToClipPos(v.vertex);
                o.uv0       = v.texcoord0;
                o.uv1       = v.texcoord1;
                o.color     = v.color;
                o.normal    = UnityObjectToWorldNormal(v.normal);
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
               #if _MESHINFO_UV0
                    return fixed4(i.uv0.xy, 0, 1);
               #elif _MESHINFO_UV1
                    return fixed4(i.uv1.xy, 0, 1);
               #elif _MESHINFO_VERTEXCOLORRGB
                    return fixed4(i.color.rgb, 1);
               #elif _MESHINFO_VERTEXCOLORALPHA
                    return fixed4(i.color.aaa, 1);
               #elif _MESHINFO_WORLDNORMAL
                    return fixed4(i.normal, 1);
               #endif
                return 1;
            }
            ENDCG
        }
    }
}