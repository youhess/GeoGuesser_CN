Shader "Custom/CircleArea"  
{  
    Properties  
    {  
        _MainTex ("Texture", 2D) = "white" {}  
        _Color ("Color", Color) = (1,1,1,1)  
        _Center ("Center", Vector) = (0.5,0.5,0,0) // 圆心位置  
        _Radius ("Radius", Range(0, 1)) = 0.5      // 圆的半径  
        _Softness ("Edge Softness", Range(0, 0.5)) = 0.05  // 边缘软化  
    }  
    SubShader  
    {  
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }  
        LOD 100  

        Pass  
        {  
            Blend SrcAlpha OneMinusSrcAlpha  
            
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
            };  

            sampler2D _MainTex;  
            float4 _MainTex_ST;  
            float4 _Color;  
            float2 _Center;  
            float _Radius;  
            float _Softness;  

            v2f vert (appdata v)  
            {  
                v2f o;  
                o.vertex = UnityObjectToClipPos(v.vertex);  
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);  
                return o;  
            }  

            fixed4 frag (v2f i) : SV_Target  
            {  
                // 计算当前像素到圆心的距离  
                float dist = distance(i.uv, _Center);  
                
                // 使用smoothstep来创建软边缘  
                float circle = 1 - smoothstep(_Radius - _Softness, _Radius + _Softness, dist);  
                
                // 采样主纹理  
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;  
                
                // 应用圆形遮罩  
                col.a *= circle;  
                
                return col;  
            }  
            ENDCG  
        }  
    }  
}