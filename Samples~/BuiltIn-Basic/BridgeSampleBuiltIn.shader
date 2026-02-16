Shader "Samples/Bridge/BuiltInBasic"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "BridgeSampleBuiltIn.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return GetBuiltInBridgeColor();
            }
            ENDCG
        }
    }
}
