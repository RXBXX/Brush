Shader "Hidden/Brush"
{
	Properties
	{
		_MainTex("MainTex",2D)="white"{}
		_CenterX("CenterX",int) = 0
		_CenterY("CenterY",int) = 0
		_Radius("Radius",int) = 30
	}
		SubShader
		{
		Tags {
				"Queue" = "Transparent"
                "RenderType" = "Transparent"
			}
			Blend  SrcAlpha OneMinusSrcAlpha
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

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				sampler2D _MainTex;
				half _CenterX;
				half _CenterY;
				half _Radius;
				half4 _MainTex_TexelSize;
				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col;

					half2 pixelPos = half2(i.uv.x*_MainTex_TexelSize.z,i.uv.y*_MainTex_TexelSize.w);
					half2 dis = pixelPos - half2(_CenterX,_CenterY);
					col = tex2D(_MainTex, i.uv);

					clip(col.a - 0.9);
					if (sqrt(dis.x*dis.x + dis.y*dis.y) < _Radius)
						col.a = 0.5;          //unity的renderTexture有个问题，就是用透明的RGBA贴图设置的时候，
					                           //它会将所有的透明通道信息单独拿出来全部做个乘法叠加，
					                           //depth only的渲染机制导致，会分开存储透明信息。
					return col;
				}
				ENDCG
			}
		}
}
