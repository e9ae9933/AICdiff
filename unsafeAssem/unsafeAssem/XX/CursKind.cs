using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class CursKind
	{
		public CursKind(string _key, string image_key, string _family = "", int _cx = -1000, int _cy = -1000)
		{
			this.family = _family;
			this.cx = _cx;
			this.cy = _cy;
			this.key = _key;
			this.LoadCursImg(MTRX.getPF(image_key));
		}

		private bool LoadCursImg(PxlFrame _PF)
		{
			if (_PF == null)
			{
				return this.PF != null;
			}
			this.PF = _PF;
			if (this.PF.MeshGenerator == null)
			{
				this.PF.getDrawnMeshGenerator(false, 0f, 0f, true).FinalizeForTriCache();
			}
			PxlPose pPose = this.PF.pPose;
			if (this.cx <= -1000)
			{
				this.cx = 0;
				this.cy = 0;
			}
			else
			{
				this.cx -= pPose.width / 2;
				this.cy -= pPose.height / 2;
			}
			CURS.mxw = X.Mx(CURS.mxw, pPose.width);
			CURS.mxh = X.Mx(CURS.mxh, pPose.height);
			return true;
		}

		public Texture2D PrepareCopiedTexture(ref RenderTexture Buf, out Vector2 CursPivot)
		{
			PxlPose pPose = this.PF.pPose;
			CursPivot = new Vector2((float)(this.cx + pPose.width / 2), (float)(this.cy + pPose.height / 2));
			if (this.Tx != null)
			{
				return this.Tx;
			}
			this.Tx = new Texture2D(pPose.width, pPose.height, TextureFormat.RGBA32, false);
			Vector2[] array;
			Color32[] array2;
			Vector3[] array3;
			int[] array4;
			int num;
			int num2;
			this.PF.MeshGenerator.getRawVerticesAndTriangles(out array, out array2, out array3, out array4, out num, out num2);
			Buf = BLIT.Alloc(ref Buf, this.PF.pPose.width, this.PF.pPose.height, false, RenderTextureFormat.ARGB32, 0);
			MTRX.getMI(this.PF.pChar, false).getMtr(-1).SetPass(0);
			Graphics.SetRenderTarget(Buf);
			GL.LoadProjectionMatrix(pPose.getMatrixForProjection(-10f, 1000f));
			BLIT.RenderToGLImmediateSimple(array4, array3, array, array2, num, num2, true);
			RenderTexture.active = Buf;
			this.Tx.ReadPixels(new Rect(0f, 0f, (float)Buf.width, (float)Buf.height), 0, 0);
			this.Tx.Apply();
			RenderTexture.active = null;
			return this.Tx;
		}

		public bool valid
		{
			get
			{
				return this.PF != null;
			}
		}

		public string key;

		public string family = "";

		public PxlFrame PF;

		public int cx;

		public int cy;

		private Texture2D Tx;
	}
}
