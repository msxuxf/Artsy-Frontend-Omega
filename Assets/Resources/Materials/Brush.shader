// Shader "Unlit/Brush"
// {
//     Properties
//     {
//         _MainTex("Texture", 2D) = "white" {}
//         _BrushTex("Brush Texture",2D) = "white" {}
//         _Color("Color",Color) = (1,1,1,1)
//         _UV("UV",Vector) = (0,0,0,0)
//         // _Size("Size",Range(1,1000)) = 1
//     }
//     SubShader
//     {
//         Tags { "RenderType"="Transparent" }
//         LOD 100
//         ZTest Always Cull Off ZWrite Off Fog{ Mode Off }
//         Blend SrcAlpha OneMinusSrcAlpha
//         //Blend One DstColor
//         Pass
//         {
//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag

//             #include "UnityCG.cginc"

//             struct appdata
//             {
//                 float4 vertex : POSITION;
//                 float2 uv : TEXCOORD0;
//             };

//             struct v2f
//             {
//                 float2 uv : TEXCOORD0;
//                 float4 vertex : SV_POSITION;
//             };

//             sampler2D _MainTex;
//             float4 _MainTex_ST;
//             sampler2D _BrushTex;
//             fixed4 _UV;
//             // float _Size;
//             fixed4 _Color;

//             v2f vert (appdata v)
//             {
//                 v2f o;
//                 o.vertex = UnityObjectToClipPos(v.vertex);
//                 o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//                 return o;
//             }
            
//             fixed4 frag (v2f i) : SV_Target
//             {
//                 // float size = _Size;
//                 // float2 uv = i.uv + (0.5f/size);
//                 // uv = uv - _UV.xy;
//                 // uv *= size;
//                 // fixed4 col = tex2D(_BrushTex,uv);
//                 fixed4 col = tex2D(_BrushTex,_UV);
//                 col.rgb = 1;
//                 col *= _Color;
//                 return col;
//             }
//             ENDCG
//         }
//     }
// }

//source: https://github.com/TwoTailsGames/Unity-Built-in-Shaders/tree/master/DefaultResourcesExtra/Mobile

//Alpha Blended
Shader "Unlit/Brush" {
Properties {
    _MainTex ("Particle Texture", 2D) = "white" {}
}

Category {
    Tags { "Queue"="Overlay" "IgnoreProjector"="False" "RenderType"="Overlay" "PreviewType"="Plane" }
    // Blend SrcFactor DstFactor
    // The generated color is multiplied by the SrcFactor. 
    // The color already on screen is multiplied by DstFactor and the two are added together.
    // Source refers to the calculated color, Destination is the color already on the screen
    // SrcAlpha: The value of this stage is multiplied by the source alpha value.
    // OneMinusSrcAlpha	The value of this stage is multiplied by (1 - source alpha).
    Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
    Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
    // BindChannels { Bind "source", target }
    // Specifies that vertex data source maps to hardware target.
    BindChannels {
        Bind "Color", color
        Bind "Vertex", vertex
        Bind "TexCoord", texcoord
    }

    SubShader {
        Pass {
            // SetTexture [TextureName] {Texture Block}
            // src1 * src2: Multiplies src1 and src2 together. The result will be darker than either input.
            // Primary is the color from the lighting calculation or the vertex color if it is bound.
            // Texture is the color of the texture specified by TextureName in the SetTexture.
            SetTexture [_MainTex] { combine texture * primary }
        }
    }
}
}

// //Multiply
// Shader "Unlit/Brush" {
// Properties {
//     _MainTex ("Particle Texture", 2D) = "white" {}
// }

// Category {
//     Tags { "Queue"="Overlay" "IgnoreProjector"="False" "RenderType"="Transparent" "PreviewType"="Plane" }
//     Blend Zero SrcColor
//     Cull Off Lighting Off ZWrite Off Fog { Color (1,1,1,1) }

//     BindChannels {
//         Bind "Color", color
//         Bind "Vertex", vertex
//         Bind "TexCoord", texcoord
//     }

//     SubShader {
//         Pass {
//             SetTexture [_MainTex] {
//                 combine texture * primary
//             }
//             SetTexture [_MainTex] {
//                 constantColor (1,1,1,0.1)
//                 combine previous lerp (previous) constant
//             }
//         }
//     }
// }
// }

// // Add
// Shader "Unlit/Brush" {
// Properties {
//     _MainTex ("Particle Texture", 2D) = "white" {}
// }

// Category {
//     Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
//     Blend SrcAlpha One
//     Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }

//     BindChannels {
//         Bind "Color", color
//         Bind "Vertex", vertex
//         Bind "TexCoord", texcoord
//     }

//     SubShader {
//         Pass {
//             SetTexture [_MainTex] {
//                 combine texture * primary
//             }
//         }
//     }
// }
// }