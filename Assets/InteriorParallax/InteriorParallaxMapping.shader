Shader "Custom/InteriorParallaxMapping"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
 
        _RoomTop ("Room Top", 2D) = "white" {}
        _RoomFloor ("Room Floor", 2D) = "white" {}
        _RoomLeft ("Room Left", 2D) = "white" {}
        _RoomRight ("Room Right", 2D) = "white" {}
        _RoomBack ("Room Back", 2D) = "white" {}
        
        _RoomCount ("Room Count",vector) = (1,1,1,1)
 
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };
        

        sampler2D _RoomTop;
        sampler2D _RoomFloor;
        sampler2D _RoomLeft;
        sampler2D _RoomRight;
        sampler2D _RoomBack;

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        struct Ray
        {
            float3 position;
            float3 direction;
        };

        void HorizontalPlaneIntersect(Ray r,float heightWS)
        {
            
        }


     
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
