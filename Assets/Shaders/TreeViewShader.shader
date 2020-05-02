Shader "Custom/TreeViewShader"
{
    Properties
    {
        _Tex1Color ("Texture 1 Color", Color) = (1,1,1,1)
        _Tex1Alpha ("Texture 1 Alpha", float) = 1
        _Tex1 ("Texture 1", 2D) = "white" {}
        _Tex2Color ("Texture 2 Color", Color) = (1,1,1,1)
        _Tex2Alpha ("Texture 2 Alpha", float) = 1
        _Tex2 ("Texture 2", 2D) = "white" {}
        _Tex3Color ("Texture 3 Color", Color) = (1,1,1,1)
        _Tex3Alpha ("Texture 3 Alpha", float) = 1
        _Tex3 ("Texture 3", 2D) = "white" {}
        _Tex4Color ("Texture 4 Color", Color) = (1,1,1,1)
        _Tex4Alpha ("Texture 4 Alpha", float) = 1
        _Tex4 ("Texture 4", 2D) = "white" {}
        _Tex5Color ("Texture 5 Color", Color) = (1,1,1,1)
        _Tex5Alpha ("Texture 5 Alpha", float) = 1
        _Tex5 ("Texture 5", 2D) = "white" {}
        _Tex6Color ("Texture 6 Color", Color) = (1,1,1,1)
        _Tex6Alpha ("Texture 6 Alpha", float) = 1
        _Tex6 ("Texture 6", 2D) = "white" {}
        _Tex7Color ("Texture 7 Color", Color) = (1,1,1,1)
        _Tex7Alpha ("Texture 7 Alpha", float) = 1
        _Tex7 ("Texture 7", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            //レンダリング順に関する指示
            "Queue"      = "Transparent"
            "RenderType" = "Transparent"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha 
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _Tex1, _Tex2, _Tex3, _Tex4, _Tex5, _Tex6, _Tex7;
            float4 _Tex1_ST;
            float4 _Tex1Color, _Tex2Color, _Tex3Color, _Tex4Color, _Tex5Color, _Tex6Color, _Tex7Color;
            float _Tex1Alpha, _Tex2Alpha, _Tex3Alpha, _Tex4Alpha, _Tex5Alpha, _Tex6Alpha, _Tex7Alpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Tex1);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col1 = tex2D(_Tex1, i.uv) * _Tex1Color;
                float4 col2 = tex2D(_Tex2, i.uv) * _Tex2Color;
                float4 col3 = tex2D(_Tex3, i.uv) * _Tex3Color;
                float4 col4 = tex2D(_Tex4, i.uv) * _Tex4Color;
                float4 col5 = tex2D(_Tex5, i.uv) * _Tex5Color;
                float4 col6 = tex2D(_Tex6, i.uv) * _Tex6Color;
                float4 col7 = tex2D(_Tex7, i.uv) * _Tex7Color;
                col1.a = col1.a * _Tex1Alpha;
                col2.a = col2.a * _Tex2Alpha;
                col3.a = col3.a * _Tex3Alpha;
                col4.a = col4.a * _Tex4Alpha;
                col5.a = col5.a * _Tex5Alpha;
                col6.a = col6.a * _Tex6Alpha;
                col7.a = col7.a * _Tex7Alpha;

                // SrcAlpha OneMinusSrcAlpha みたいなブレンドをする
                fixed4 col = col1 * (1 - col2.a) + col2 * col2.a;
                col = col * (1 - col3.a) + col3 * col3.a;
                col = col * (1 - col4.a) + col4 * col4.a;
                col = col * (1 - col5.a) + col5 * col5.a;
                col = col * (1 - col6.a) + col6 * col6.a;
                col = col * (1 - col7.a) + col7 * col7.a;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
