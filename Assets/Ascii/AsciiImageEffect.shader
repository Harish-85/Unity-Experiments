Shader "Hidden/NewImageEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _outputResolution ("Output Resolution", Vector) = (192, 108, 0, 0)
        _AsciiChars ("Ascii Char Img", 2D) = "white" {}
        _AsciiCharCount ("Ascii Char Count", Float) = 16
        _testAscii ("Test Ascii", Float) = 0
        
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _AsciiChars;
            float2 _outputResolution;
            float _AsciiCharCount;
            float _testAscii;
            
            sampler2D _MainTex;

            float4 sampleText(float val,float2 uv)
            {
                float charWidth = 1.0 / _AsciiCharCount;
                float charUV = (val * charWidth) + (uv.x * charWidth);
                return tex2D(_AsciiChars, float2(charUV, uv.y));
                
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                //downsample to the _outputResolution
                float2 downscaledUV = floor( i.uv * _outputResolution) / _outputResolution;
                fixed4 col = tex2D(_MainTex, downscaledUV);
                //float luminance = dot(col.rgb, float3(0.299, 0.587, 0.114));
                //get the magnitude of the color
                float luminance = length(col.rgb);
                float2 textUV = frac(i.uv * _outputResolution);
                float4 textCol = sampleText(floor(luminance * 9),textUV);

                float4 color = tex2D(_MainTex, downscaledUV);
                
                return textCol * color;
                
    
                
                col.rgb = 1 - col.rgb;
                return sampleText(_testAscii,i.uv);
            }
            ENDCG
        }
    }
}
