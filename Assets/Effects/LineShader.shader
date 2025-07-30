Shader "Unlit/LineShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType"="Opaque" }
        ZTest Always
        ZWrite Off
        Lighting Off
        Cull Off
        Pass
        {
            Color [_Color]
        }
    }
}