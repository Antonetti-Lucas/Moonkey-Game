Shader "Custom/InvertedCullUnlit"
{
    Properties
    {
        _MainTex ("Cubemap (Skybox)", Cube) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        // Render back faces instead of front faces
        Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float3 worldPos : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            samplerCUBE _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // Calculate world position for cubemap sampling
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate direction from camera to world position for cubemap sampling
                float3 viewDir = normalize(i.worldPos - _WorldSpaceCameraPos);
                
                // Sample the cubemap
                fixed4 col = texCUBE(_MainTex, viewDir) * _Color;
                return col;
            }
            ENDCG
        }
    }
}