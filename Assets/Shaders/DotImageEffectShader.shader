// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DotImageEffectShader" {
    Properties{
        _MainTex("", 2D) = "" {}
    }
    SubShader{

        ZTest Always
        ZWrite Off
        Cull Off
        Fog{ Mode Off }

        CGINCLUDE
        #include "UnityCG.cginc"
        #pragma target 3.0
        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
            float2 uv2 : TEXCOORD1;
        };			

        sampler2D _MainTex;
        float4 _MainTex_ST;
        float4 _MainTex_TexelSize;
        sampler2D _CameraDepthNormalsTexture;
        float4 _CameraDepthNormalsTexture_ST;

        v2f vert(appdata_img v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
            o.uv2 = TRANSFORM_TEX(v.texcoord, _CameraDepthNormalsTexture);

            if (_MainTex_TexelSize.y < 0)
            o.uv2.y = 1.0 - o.uv2.y;

            return o;

        }				
        

        float GetDepth(half2 uv)
        {
            float4 depthnormal = tex2D(_CameraDepthNormalsTexture, uv);
            float3 viewNorm;
            float depth;
            DecodeDepthNormal(depthnormal, depth, viewNorm);
            return depth;
        }
        #pragma vertex vert
        ENDCG

        // Pass0 RGB+D(Depth*(1-a))
        Pass{

            CGPROGRAM
            #pragma fragment frag

            half4 frag(v2f i) : SV_Target
            {
                half4 centerCol = tex2D(_MainTex, i.uv);
                centerCol.a = (1 - centerCol.a) * GetDepth(i.uv2);
                return centerCol;
            }
            ENDCG
        } // Pass0


        // Pass1 DepthOnly
        Pass{

            CGPROGRAM
            #pragma fragment frag

            half4 frag(v2f i) : SV_Target
            {
                return GetDepth(i.uv2);
            }
            ENDCG
        } // Pass1
        
        // Pass2 MinDepth
        Pass{

            CGPROGRAM
            #pragma fragment frag

            uniform float4 _PixelSize;
            half4 frag(v2f i) : SV_Target
            {
                float minDepth = 9999;
                i.uv -= 0.5 * _PixelSize.xy;

                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        float2 uv2 = i.uv + _PixelSize.xy * float2(x, y);
                        minDepth = min(minDepth, tex2D(_MainTex,uv2).r);
                    }
                }
                return minDepth;
            }
            ENDCG
        } // Pass2


        // Pass3 DotEffect
        Pass{

            CGPROGRAM
            #pragma fragment frag


            uniform float4 _PixelSize;
            uniform float _ValidDepthThreshold;
            uniform float _EdgeLuminanceThreshold;
            uniform float _SharpEdge;

            half4 frag(v2f i) : SV_Target
            {
                half4 centerCol = tex2D(_MainTex, i.uv);
                half4 nearCol;
                float4 cols[4];

                i.uv -= 0.5 * _PixelSize.xy;

                // minDepth Detection
                float minDepth = 9999;

                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        float2 uv2 = i.uv + _PixelSize.xy * float2(x, y);
                        float4 c = tex2D(_MainTex, uv2);
                        cols[y * 2 + x] = c;

                        if (minDepth > c.a)
                        {
                            minDepth = c.a;
                            nearCol = c;
                        }
                    }
                }

                float4 col = 0;
                float weights = 0;
                float minLum = 9999;
                float maxLum = 0;

                for (int j = 0; j < 4; j++)
                {
                    half depth = cols[j].a;

                    // 有効ピクセルのみ処理する
                    if (abs(depth - minDepth) < _ValidDepthThreshold)
                    {
                        float Lum = Luminance(cols[j].rgb);
                        float weight = pow(Lum - 0.5, 2) * 4;	// 白と黒を強調する
                        weight = lerp(1, weight, _SharpEdge);

                        col += cols[j] * weight;
                        weights += weight;

                        minLum = min(minLum, Lum);
                        maxLum = max(maxLum, Lum);
                    }
                }
                col /= weights;


                // 輝度差が大きい場合は平均化を行わない
                if (maxLum - minLum > _EdgeLuminanceThreshold)
                {
                    col = nearCol;
                }

                return col;
            }
            ENDCG
        } // pass3


        // Pass4 SliceComposit
        Pass{

            CGPROGRAM
            #pragma fragment frag

            uniform float _SliceDepth;

            sampler2D _MinDepth;

            half4 frag(v2f i) : SV_Target
            {
                half4 c = tex2D(_MainTex, i.uv);
                float depth = tex2D(_MinDepth,i.uv).r;
                clip(_SliceDepth - depth);

                return c;
            }
            ENDCG
        } // pass4

    }
}
