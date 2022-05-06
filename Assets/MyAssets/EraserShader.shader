Shader "Custom/EraserShader"
{
	Properties
	{
		_MainTex("MainTex",2D)="white"{}
		_Brush("Brush",2D) = "white"{}
		_CenterX("CenterX",int) = -1000
		_CenterY("CenterY",int) = -1000
		_BrushWidth("BrushWidth",int) = 0
		_BrushHeight("BrushHeight",int) = 0
		_BrushSize("BrushSize", Range(0,1)) = 0.4
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
				sampler2D _Brush;
				half _CenterX;
				half _CenterY;
				half4 _MainTex_TexelSize;
				half _BrushWidth;
				half _BrushHeight;
				half _BrushSize;
									//	//unity的rendertexture有个问题，就是用透明的rgba贴图设置的时候，
					//                           //它会将所有的透明通道信息单独拿出来全部做个乘法叠加，
					//                           //depth only的渲染机制导致，会分开存储透明信息。
				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col;

					half2 pixelPos = half2(i.uv.x * _MainTex_TexelSize.z, i.uv.y * _MainTex_TexelSize.w);
					col = tex2D(_MainTex, i.uv);
					
					clip(col.a - 0.6); //裁切透明通道

					half width = _BrushWidth * _BrushSize;
					half height = _BrushHeight * _BrushSize;
					if (pixelPos.x > _CenterX - width && pixelPos.x < _CenterX + width && pixelPos.y > _CenterY - width && pixelPos.y < _CenterY + width)
					{
						half2 uv;
						uv.x = (pixelPos.x - _CenterX + width/2) / width;
						uv.y = (pixelPos.y - _CenterY + height/2) / height;
					   if (tex2D(_Brush,uv).a > 0.2)
					   {
					       col.a = 0.5;	
					   }
					}
					return col;
				}
				ENDCG
			}
		}
}
