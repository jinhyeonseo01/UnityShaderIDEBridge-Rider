Shader "Samples/Bridge/HDRPBasic"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "BridgeSampleHDRP.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = input.positionOS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return GetHdrpBridgeColor();
            }
            ENDHLSL
        }
    }
}
