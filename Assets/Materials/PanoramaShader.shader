Shader "Custom/PanoramaShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.texcoord = normalize(v.vertex.xyz);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Invert the x coordinate for correct horizontal orientation
                float3 texcoord = float3(-i.texcoord.x, i.texcoord.yz);

                float2 latLong = float2(atan2(texcoord.z, texcoord.x), asin(texcoord.y));
                latLong *= float2(0.1591, 0.3183); // (1/2PI, 1/PI)
                latLong += 0.5;
                return tex2D(_MainTex, latLong);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}