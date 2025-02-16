using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class RenderTexturePool
	{
		public RenderTexturePool(int _w, int _h, int _depth, RenderTextureFormat textureFormat = RenderTextureFormat.ARGB32)
		{
			this.ARd = new List<RenderTexture>(4);
			this.w = _w;
			this.h = _h;
			this.depth = _depth;
			this.TxFormat = textureFormat;
		}

		public bool isSame(int _w, int _h, int _depth, RenderTextureFormat textureFormat)
		{
			return this.w == _w && this.h == _h && this.depth == _depth && this.TxFormat == textureFormat;
		}

		public RenderTexture Pop()
		{
			if (this.max < this.ARd.Count)
			{
				List<RenderTexture> ard = this.ARd;
				int num = this.max;
				this.max = num + 1;
				return ard[num];
			}
			RenderTexture renderTexture;
			this.ARd.Add(renderTexture = new RenderTexture(this.w, this.h, this.depth, this.TxFormat));
			this.max = this.ARd.Count;
			return renderTexture;
		}

		public bool Release(RenderTexture Rd)
		{
			int num = this.ARd.IndexOf(Rd);
			if (num >= 0)
			{
				this.ARd.RemoveAt(num);
				this.ARd.Add(Rd);
				this.max--;
				return true;
			}
			return false;
		}

		public void releaseAll()
		{
			this.max = 0;
		}

		public void dispose()
		{
			int count = this.ARd.Count;
			for (int i = 0; i < count; i++)
			{
				BLIT.nDispose(this.ARd[i]);
			}
			this.ARd.Clear();
			this.max = 0;
		}

		public static RenderTexture PopA(List<RenderTexturePool> A, int _w, int _h, int _depth, RenderTextureFormat textureFormat = RenderTextureFormat.ARGB32)
		{
			int count = A.Count;
			for (int i = 0; i < count; i++)
			{
				RenderTexturePool renderTexturePool = A[i];
				if (renderTexturePool.isSame(_w, _h, _depth, textureFormat))
				{
					return renderTexturePool.Pop();
				}
			}
			RenderTexturePool renderTexturePool2 = new RenderTexturePool(_w, _h, _depth, textureFormat);
			A.Add(renderTexturePool2);
			return renderTexturePool2.Pop();
		}

		public static void ReleaseA(List<RenderTexturePool> A)
		{
			int count = A.Count;
			for (int i = 0; i < count; i++)
			{
				A[i].releaseAll();
			}
		}

		private List<RenderTexture> ARd;

		private int max;

		private int w;

		private int h;

		private int depth;

		private RenderTextureFormat TxFormat;
	}
}
