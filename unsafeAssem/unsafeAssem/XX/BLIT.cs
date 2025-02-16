using System;
using UnityEngine;

namespace XX
{
	public static class BLIT
	{
		public static void init1()
		{
			BLIT.MtrNormalClearing = MTRX.newMtr("Buffer/NormalClearing");
			BLIT.MtrJustPaste = MTRX.newMtr("Buffer/JustPaste");
			BLIT.MtrSolidColor = MTRX.newMtr("Buffer/BufferSolidColor");
			BLIT.MtrDrawDepth = MTRX.newMtr("Buffer/DrawDepth");
			BLIT.MtrNormalDrawRota = MTRX.newMtr("Buffer/NormalDrawRota");
			BLIT.MtrBufGDT = MTRX.newMtr(MTRX.ShaderGDT);
			BLIT.MtrBufGDT.EnableKeyword("NO_PIXELSNAP");
			BLIT.MtrBlur = MTRX.newMtr("Buffer/GaussianBlur");
		}

		public static RenderTexture PasteTo(RenderTexture Dest, Texture Src, float x, float y, float scale, float uvx = 0f, float uvy = 0f, float uvw = 1f, float uvh = 1f)
		{
			BLIT.MtrNormalClearing.SetVector("_NST", new Vector4(uvw, uvh, uvx, uvy));
			BLIT.DrawCenTo(Dest, Src, x, y, scale, BLIT.MtrNormalClearing);
			return Dest;
		}

		public static RenderTexture Extract(Texture Src, float scale, float uvx = 0f, float uvy = 0f, float uvw = 1f, float uvh = 1f)
		{
			int num = X.IntC((float)Src.width * uvw * scale);
			int num2 = X.IntC((float)Src.height * uvh * scale);
			return BLIT.PasteTo(new RenderTexture(num, num2, 0, RenderTextureFormat.ARGB32, Src.mipmapCount), Src, (float)(num / 2) - uvx * (float)Src.width * scale, (float)(num2 / 2) - uvy * (float)Src.height * scale, scale, uvx, uvy, uvw, uvh);
		}

		public static RenderTexture Alloc(ref RenderTexture Tx, int width, int height, bool use_filter_trilinear = false, RenderTextureFormat _format = RenderTextureFormat.ARGB32, int depth = 0)
		{
			if (Tx == null || width != Tx.width || height != Tx.height)
			{
				string text = "";
				if (Tx != null)
				{
					text = Tx.name;
					Tx.Release();
				}
				Tx = new RenderTexture(width, height, depth, _format, RenderTextureReadWrite.Default);
				Tx.name = text;
				Tx.filterMode = (use_filter_trilinear ? FilterMode.Trilinear : FilterMode.Point);
			}
			BLIT.Clear(Tx, 0U, true);
			return Tx;
		}

		public static RenderTexture Clear(RenderTexture Tx, uint col = 0U, bool clear_depth = true)
		{
			if (Tx != null)
			{
				Graphics.SetRenderTarget(Tx);
				GL.Clear(clear_depth, true, C32.d2c(col));
				Graphics.SetRenderTarget(null);
			}
			return Tx;
		}

		public static RenderTexture Clear(RenderTexture Tx, Color32 C, bool clear_depth = true)
		{
			if (Tx != null)
			{
				Graphics.SetRenderTarget(Tx);
				GL.Clear(clear_depth, true, C);
				Graphics.SetRenderTarget(null);
			}
			return Tx;
		}

		public static RenderTexture Blur(Sprite Src, int x_pixel, int y_pixel, float scale, float weight = 1f, uint transparent_color = 16777215U)
		{
			Rect textureRect = Src.textureRect;
			return BLIT.Blur(Src.texture, x_pixel, y_pixel, scale, weight, textureRect.x / (float)Src.texture.width, textureRect.y / (float)Src.texture.height, textureRect.width / (float)Src.texture.width, textureRect.height / (float)Src.texture.height, transparent_color);
		}

		public static RenderTexture Blur(Texture Src, int x_pixel, int y_pixel, float scale, float weight = 1f, float uvx = 0f, float uvy = 0f, float uvw = 1f, float uvh = 1f, uint transparent_color = 16777215U)
		{
			int num = X.IntC((float)Src.width * uvw / 2f * scale) * 2 + x_pixel * 2;
			int num2 = X.IntC((float)Src.height * uvh / 2f * scale) * 2 + y_pixel * 2;
			RenderTexture renderTexture = new RenderTexture(num, num2, 0, RenderTextureFormat.ARGB32);
			return BLIT.Blur(renderTexture, Src, x_pixel, y_pixel, (float)(renderTexture.width / 2), (float)(renderTexture.height / 2), scale, weight, uvx, uvy, uvw, uvh, transparent_color);
		}

		public static RenderTexture Blur(RenderTexture Dest, Texture Src, int x_pixel, int y_pixel, float drawx, float drawy, float scale, float weight = 1f, float uvx = 0f, float uvy = 0f, float uvw = 1f, float uvh = 1f, uint transparent_color = 16777215U)
		{
			int num = X.IntC((float)Src.width * uvw / 2f * scale) * 2 + x_pixel * 2;
			int num2 = X.IntC((float)Src.height * uvh / 2f * scale) * 2 + y_pixel * 2;
			RenderTexture renderTexture = new RenderTexture(num, num2, 0, RenderTextureFormat.ARGB32, 1);
			BLIT.MtrBlur.SetFloat("_Weight", weight);
			BLIT.MtrBlur.SetVector("_NST", new Vector4(uvw, uvh, uvx, uvy));
			BLIT.MtrBlur.SetFloat("_Resolution", (float)x_pixel);
			BLIT.MtrBlur.SetVector("_OFFSETS", new Vector4(1f / scale, 0f, 0f, 0f));
			BLIT.MtrBlur.SetColor("_Color", C32.d2c(transparent_color));
			BLIT.DrawCenTo(renderTexture, Src, (float)(renderTexture.width / 2), (float)(renderTexture.height / 2), scale, BLIT.MtrBlur);
			BLIT.MtrBlur.SetVector("_NST", new Vector4(1f, 1f, 0f, 0f));
			BLIT.MtrBlur.SetFloat("_Resolution", (float)y_pixel);
			BLIT.MtrBlur.SetVector("_OFFSETS", new Vector4(0f, 1f, 0f, 0f));
			BLIT.DrawCenTo(Dest, renderTexture, drawx, drawy, 1f, BLIT.MtrBlur);
			renderTexture.Release();
			return Dest;
		}

		public static void DrawCenTo(RenderTexture Dest, Texture Src, float x, float y, float scale = 1f, Material Mtr = null)
		{
			Vector4 vector = Mtr.GetVector("_NST");
			float num = (float)Src.width * vector.x * scale;
			float num2 = (float)Src.height * vector.y * scale;
			Vector2 vector2 = new Vector2(x - num / 2f, y - num2 / 2f);
			float num3 = vector2.x / (float)Dest.width;
			float num4 = num / (float)Dest.width;
			float num5 = vector2.y / (float)Dest.height;
			float num6 = num2 / (float)Dest.height;
			if (Mtr != null)
			{
				Mtr.SetTexture("_MainTex", Src);
				Vector4 vector3 = new Vector4(num3, num5, num4, num6);
				Mtr.SetVector("_BasePos", vector3);
				Graphics.Blit(Src, Dest, Mtr);
				Mtr.SetTexture("_MainTex", null);
				return;
			}
			Graphics.Blit(Src, Dest, new Vector2(scale, scale), vector2);
		}

		public static void RotaGraph(RenderTexture Dest, float x, float y, float scaleX, float scaleY, float rotR, Texture Src, float uvx = 0f, float uvy = 0f, float uvw = 1f, float uvh = 1f)
		{
			Material mtrNormalDrawRota = BLIT.MtrNormalDrawRota;
			mtrNormalDrawRota.SetVector("_NST", new Vector4(uvw, uvh, uvx, uvy));
			float num = (float)Src.width * uvw * scaleX;
			float num2 = (float)Src.height * uvh * scaleY;
			Vector2 vector = new Vector2(x - num / 2f, y - num2 / 2f);
			float num3 = vector.x / (float)Dest.width;
			float num4 = num / (float)Dest.width;
			float num5 = vector.y / (float)Dest.height;
			float num6 = num2 / (float)Dest.height;
			mtrNormalDrawRota.SetFloat("_CosRotR", -X.Cos(rotR));
			mtrNormalDrawRota.SetFloat("_SinRotR", -X.Sin(rotR));
			mtrNormalDrawRota.SetTexture("_MainTex", Src);
			Vector4 vector2 = new Vector4(num3, num5, num4, num6);
			mtrNormalDrawRota.SetVector("_BasePos", vector2);
			Graphics.Blit(Src, Dest, mtrNormalDrawRota);
			mtrNormalDrawRota.SetTexture("_MainTex", null);
		}

		public static void RotaGraph(RenderTexture Dest, float x, float y, float scaleX, float scaleY, float rotR, Sprite Src)
		{
			Rect textureRect = Src.textureRect;
			BLIT.RotaGraph(Dest, x, y, scaleX, scaleY, rotR, Src.texture, textureRect.x / (float)Src.texture.width, textureRect.y / (float)Src.texture.height, textureRect.width / (float)Src.texture.width, textureRect.height / (float)Src.texture.height);
		}

		public static void JustPaste(Texture Src, RenderTexture Dest)
		{
			BLIT.MtrJustPaste.mainTexture = Src;
			Graphics.SetRenderTarget(Dest);
			BLIT.MtrJustPaste.SetPass(0);
			GL.LoadOrtho();
			GL.Begin(4);
			BLIT.JustPasteOrtho(0f, 0f, 1f, 1f);
			GL.End();
		}

		public static void JustPasteOrtho(float ver_l = 0f, float ver_b = 0f, float ver_r = 1f, float ver_t = 1f)
		{
			GL.TexCoord3(0f, 0f, 0f);
			GL.Vertex3(ver_l, ver_b, 0f);
			GL.TexCoord3(0f, 1f, 0f);
			GL.Vertex3(ver_l, ver_t, 0f);
			GL.TexCoord3(1f, 1f, 0f);
			GL.Vertex3(ver_r, ver_t, 0f);
			GL.TexCoord3(0f, 0f, 0f);
			GL.Vertex3(ver_l, ver_b, 0f);
			GL.TexCoord3(1f, 1f, 0f);
			GL.Vertex3(ver_r, ver_t, 0f);
			GL.TexCoord3(1f, 0f, 0f);
			GL.Vertex3(ver_r, ver_b, 0f);
		}

		public static void JustPasteOrtho2()
		{
			GL.TexCoord3(0f, 0f, 0f);
			GL.MultiTexCoord2(1, 0f, 0f);
			GL.Vertex3(0f, 0f, 0f);
			GL.TexCoord3(0f, 1f, 0f);
			GL.MultiTexCoord2(1, 0f, 1f);
			GL.Vertex3(0f, 1f, 0f);
			GL.TexCoord3(1f, 1f, 0f);
			GL.MultiTexCoord2(1, 1f, 1f);
			GL.Vertex3(1f, 1f, 0f);
			GL.TexCoord3(0f, 0f, 0f);
			GL.MultiTexCoord2(1, 0f, 0f);
			GL.Vertex3(0f, 0f, 0f);
			GL.TexCoord3(1f, 1f, 0f);
			GL.MultiTexCoord2(1, 1f, 1f);
			GL.Vertex3(1f, 1f, 0f);
			GL.TexCoord3(1f, 0f, 0f);
			GL.MultiTexCoord2(1, 1f, 0f);
			GL.Vertex3(1f, 0f, 0f);
		}

		public static Texture nDispose(Texture Tx)
		{
			try
			{
				if (Tx != null)
				{
					if (Tx is RenderTexture)
					{
						(Tx as RenderTexture).Release();
					}
					else
					{
						IN.DestroyOne(Tx);
					}
				}
			}
			catch
			{
			}
			return null;
		}

		public static Texture2D getSnapShot(RenderTexture TxSrc, float shiftx = 0f, float shifty = 0f, float dw = 0f, float dh = 0f, float scale = 2f, bool readable = false)
		{
			if (dw <= 0f)
			{
				dw = (float)TxSrc.width * scale;
			}
			if (dh <= 0f)
			{
				dh = (float)TxSrc.height * scale;
			}
			RenderTexture renderTexture = new RenderTexture(X.IntR(dw), X.IntR(dh), 0, RenderTextureFormat.ARGB32);
			BLIT.MtrNormalClearing.SetVector("_NST", new Vector4(1f, 1f, 0f, 0f));
			BLIT.DrawCenTo(renderTexture, TxSrc, (float)((int)(dw / 2f + shiftx)), (float)((int)(dh / 2f + shifty)), scale, BLIT.MtrNormalClearing);
			Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
			RenderTexture.active = renderTexture;
			texture2D.ReadPixels(new Rect(0f, 0f, (float)renderTexture.width, (float)renderTexture.height), 0, 0);
			texture2D.Apply(false, !readable);
			RenderTexture.active = null;
			BLIT.nDispose(renderTexture);
			return texture2D;
		}

		public static RenderTexture RenderWholeMesh(MeshDrawer Md, float cx, float cy, int w, int h, int tri_max = -1, bool rewrite_md_material = false, float scale = -1f, RenderTexture Dest = null, Material DrawMtr = null, float pixel_l = 0f, float pixel_r = -1f, float pixel_b = 0f, float pixel_t = -1f)
		{
			bool flag = false;
			if (w < 0)
			{
				flag = true;
				w = -w;
				h = X.Abs(h);
			}
			int num = w / 2;
			int num2 = h / 2;
			if (pixel_r < 0f)
			{
				pixel_r = (float)w;
			}
			if (pixel_t < 0f)
			{
				pixel_t = (float)h;
			}
			if (scale < 0f)
			{
				scale = 64f;
			}
			bool flag2 = false;
			cx += (pixel_r - pixel_l) / 2f;
			cy += (pixel_t - pixel_b) / 2f;
			RenderBuffer activeColorBuffer = Graphics.activeColorBuffer;
			RenderBuffer activeDepthBuffer = Graphics.activeDepthBuffer;
			if (!flag)
			{
				if (Dest == null)
				{
					Dest = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32);
					flag2 = true;
				}
				Dest.filterMode = FilterMode.Point;
				Graphics.SetRenderTarget(Dest);
			}
			if (flag2)
			{
				GL.Clear(false, true, Color.clear);
			}
			if (DrawMtr == null)
			{
				DrawMtr = BLIT.MtrBufGDT;
				BLIT.MtrBufGDT.SetTexture("_MainTex", Md.getMaterial().GetTexture("_MainTex"));
			}
			DrawMtr.SetPass(0);
			GL.LoadPixelMatrix(pixel_l, pixel_r, pixel_b, pixel_t);
			GL.MultMatrix(Matrix4x4.Translate(new Vector3(cx, cy, 0f)) * Matrix4x4.Scale(new Vector3(scale, scale, 1f)));
			BLIT.RenderToGLImmediate(Md, tri_max);
			if (!flag)
			{
				Graphics.SetRenderTarget(activeColorBuffer, activeDepthBuffer);
			}
			return Dest;
		}

		public static void RenderToGLImmediate(MeshDrawer Md, int tri_max = -1)
		{
			BLIT.RenderToGLOneTask(Md, tri_max, true);
		}

		public unsafe static void RenderToGLOneTask(MeshDrawer Md, int tri_max = -1, bool set_beginend = false)
		{
			int num = ((tri_max < 0) ? Md.draw_triangle_count : tri_max);
			if (num <= 0)
			{
				return;
			}
			int[] triangleArray = Md.getTriangleArray();
			Vector3[] vertexArray = Md.getVertexArray();
			Vector2[] uvArray = Md.getUvArray();
			Color32[] colorArray = Md.getColorArray();
			fixed (int* ptr = ref triangleArray[0])
			{
				int* ptr2 = ptr;
				fixed (Vector3* ptr3 = &vertexArray[0])
				{
					Vector3* ptr4 = ptr3;
					fixed (Vector2* ptr5 = &uvArray[0])
					{
						Vector2* ptr6 = ptr5;
						fixed (Color32* ptr7 = &colorArray[0])
						{
							Color32* ptr8 = ptr7;
							int* ptr9 = ptr2;
							int num2 = (Md.use_uv2 ? Md.getUv2Max() : 0);
							int num3 = (Md.use_uv3 ? Md.getUv3Max() : 0);
							Vector2[] uv2Array = Md.getUv2Array(false);
							Vector2[] uv3Array = Md.getUv3Array(false);
							if (set_beginend)
							{
								GL.Begin(4);
							}
							while (--num >= 0)
							{
								int num4 = *(ptr9++);
								ref Vector3 ptr10 = ref ptr4[num4];
								GL.Color(ptr8[num4]);
								GL.TexCoord(ptr6[num4]);
								if (num2 > num4)
								{
									GL.MultiTexCoord(1, uv2Array[num4]);
								}
								if (num3 > num4)
								{
									GL.MultiTexCoord(2, uv3Array[num4]);
								}
								GL.Vertex(ptr10);
							}
							if (set_beginend)
							{
								GL.End();
							}
						}
					}
				}
			}
			if (!set_beginend)
			{
				Md.clearSimple();
			}
		}

		public unsafe static void RenderToGLImmediateSP(MeshDrawer Md, Material Mtr, int tri_max = -1)
		{
			int[] triangleArray = Md.getTriangleArray();
			Vector3[] vertexArray = Md.getVertexArray();
			Vector2[] uvArray = Md.getUvArray();
			Color32[] colorArray = Md.getColorArray();
			Mtr.SetPass(0);
			int num = ((tri_max < 0) ? Md.draw_triangle_count : tri_max);
			if (num <= 0)
			{
				return;
			}
			fixed (int* ptr = &triangleArray[0])
			{
				int* ptr2 = ptr;
				fixed (Vector3* ptr3 = &vertexArray[0])
				{
					Vector3* ptr4 = ptr3;
					fixed (Vector2* ptr5 = &uvArray[0])
					{
						Vector2* ptr6 = ptr5;
						fixed (Color32* ptr7 = &colorArray[0])
						{
							Color32* ptr8 = ptr7;
							int* ptr9 = ptr2;
							int num2 = (Md.use_uv2 ? Md.getUv2Max() : 0);
							int num3 = (Md.use_uv3 ? Md.getUv3Max() : 0);
							Vector2[] uv2Array = Md.getUv2Array(false);
							Vector2[] uv3Array = Md.getUv3Array(false);
							GL.Begin(4);
							while (--num >= 0)
							{
								int num4 = *(ptr9++);
								ref Vector3 ptr10 = ref ptr4[num4];
								GL.Color(ptr8[num4]);
								GL.TexCoord(ptr6[num4]);
								if (num2 > num4)
								{
									GL.MultiTexCoord(1, uv2Array[num4]);
								}
								if (num3 > num4)
								{
									GL.MultiTexCoord(2, uv3Array[num4]);
								}
								GL.Vertex(ptr10);
							}
							GL.End();
						}
					}
				}
			}
		}

		public unsafe static void RenderToGLImmediate001(MeshDrawer Md, int tri_max = -1, int sub_mesh = -1, bool setpass = false, bool call_begin = true, MProperty Mpb = null)
		{
			Material material = null;
			int[] array;
			if (sub_mesh == -1 || !Md.hasMultipleTriangle() || (Md.draw_gl_only && Md.getCurrentSubMeshIndex() == sub_mesh))
			{
				array = Md.getTriangleArray();
				if (tri_max < 0)
				{
					tri_max = Md.draw_triangle_count;
				}
				if (tri_max <= 0)
				{
					return;
				}
				if (Mpb != null)
				{
					material = Md.getMaterial();
					Mpb.Push(material);
				}
				if (setpass)
				{
					Md.getMaterial().SetPass(0);
				}
			}
			else
			{
				int num = 0;
				array = Md.getSubMeshData(sub_mesh, ref num);
				if (tri_max < 0)
				{
					tri_max = num;
				}
				if (array == null || tri_max <= 0)
				{
					return;
				}
				if (Mpb != null)
				{
					material = Md.getSubMeshMaterial(sub_mesh);
					Mpb.Push(material);
				}
				if (setpass)
				{
					Md.getSubMeshMaterial(sub_mesh).SetPass(0);
				}
			}
			Vector3[] vertexArray = Md.getVertexArray();
			Vector2[] uvArray = Md.getUvArray();
			Color32[] colorArray = Md.getColorArray();
			int num2 = ((tri_max < 0) ? Md.draw_triangle_count : tri_max);
			if (num2 > 0)
			{
				fixed (int* ptr = &array[0])
				{
					int* ptr2 = ptr;
					fixed (Vector3* ptr3 = &vertexArray[0])
					{
						Vector3* ptr4 = ptr3;
						fixed (Vector2* ptr5 = &uvArray[0])
						{
							Vector2* ptr6 = ptr5;
							fixed (Color32* ptr7 = &colorArray[0])
							{
								Color32* ptr8 = ptr7;
								int* ptr9 = ptr2;
								int num3 = (Md.use_uv2 ? Md.getUv2Max() : 0);
								int num4 = (Md.use_uv3 ? Md.getUv3Max() : 0);
								Vector2[] uv2Array = Md.getUv2Array(false);
								Vector2[] uv3Array = Md.getUv3Array(false);
								if (call_begin)
								{
									GL.Begin(4);
								}
								while (--num2 >= 0)
								{
									int num5 = *(ptr9++);
									Vector3 vector = ptr4[num5];
									GL.Color(ptr8[num5]);
									GL.TexCoord(ptr6[num5]);
									if (num3 > num5)
									{
										Vector2 vector2 = uv2Array[num5];
										GL.MultiTexCoord2(1, vector2.x, vector2.y);
									}
									if (num4 > num5)
									{
										Vector2 vector2 = uv3Array[num5];
										GL.MultiTexCoord2(2, vector2.x, vector2.y);
									}
									GL.Vertex3(vector.x, vector.y, vector.z);
								}
								if (call_begin)
								{
									GL.End();
								}
							}
						}
					}
				}
			}
			if (Mpb != null)
			{
				Mpb.Pop(material);
			}
		}

		public static void RenderToGLMtr(MeshDrawer Md, float cx, float cy, float scale, Material Mtr, Matrix4x4 WorldMatrix, int tri_max = -1, bool multiply_path = false, bool no_z_scale2zero = false)
		{
			WorldMatrix = WorldMatrix * Matrix4x4.Translate(new Vector3(cx, cy, 0f)) * Matrix4x4.Scale(new Vector3(scale, scale, (float)(no_z_scale2zero ? (-1) : 0)));
			GL.LoadProjectionMatrix(WorldMatrix);
			if (multiply_path)
			{
				BLIT.RenderToGLImmediateSP(Md, Mtr, tri_max);
				return;
			}
			Mtr.SetPass(0);
			BLIT.RenderToGLImmediate(Md, tri_max);
		}

		public static void RenderToGLMtrTriMulti(MeshDrawer Md, float cx, float cy, float scale, Matrix4x4 WorldMatrix, float z_scale = 0f)
		{
			GL.LoadProjectionMatrix(WorldMatrix * Matrix4x4.Translate(new Vector3(cx, cy, 0f)) * Matrix4x4.Scale(new Vector3(scale, scale, z_scale)));
			int subMeshCount = Md.getSubMeshCount(false);
			for (int i = 0; i < subMeshCount; i++)
			{
				BLIT.RenderToGLImmediate001(Md, -1, i, true, true, null);
			}
		}

		public static Matrix4x4 getMatrixForImage(Texture Tx, float scale = 1f)
		{
			float num = scale * 64f / ((float)Tx.width * 0.5f);
			float num2 = scale * 64f / ((float)Tx.height * 0.5f);
			return Matrix4x4.Scale(new Vector3(num, num2, 1f));
		}

		private static Material MtrBlur;

		private static Material MtrBufGDT;

		private static Material MtrNormalClearing;

		public static Material MtrDrawDepth;

		public static Material MtrJustPaste;

		public static Material MtrSolidColor;

		private static Material MtrNormalDrawRota;
	}
}
