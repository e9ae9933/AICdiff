using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2BlurImage
	{
		public M2BlurImage(int _level, M2ChipImage Img, uint colortransp)
		{
			this.level = _level;
			Img.getMainMesh();
			M2ImageAtlas.AtlasRect sourceAtlas = Img.SourceAtlas;
			float num = (float)sourceAtlas.w;
			float num2 = (float)sourceAtlas.h;
			MImage michip = Img.IMGS.MIchip;
			float num3 = 1f / (float)michip.width;
			float num4 = 1f / (float)michip.height;
			X.NI(num, num2, 0.5f);
			float num5 = 2f;
			this.level = X.IntC((float)this.level * num5);
			this.Tx = BLIT.Blur(michip.Tx, this.level, this.level, num5, 1f, (float)sourceAtlas.x * num3, (float)sourceAtlas.y * num4, num * num3, num2 * num4, colortransp & 16777215U);
			RenderTexture.active = null;
			this.Mesh = MeshDrawer.makeBasicSpriteMesh(this.Tx, 0f, 0f, 1f, 1f, uint.MaxValue, false, 64f * num5);
		}

		public void Dispose()
		{
			BLIT.nDispose(this.Tx);
			this.Tx = null;
		}

		public RenderTexture getTexture()
		{
			return this.Tx;
		}

		public bool destructed
		{
			get
			{
				return this.Tx == null;
			}
		}

		public readonly PxlMeshDrawer Mesh;

		public readonly int level;

		private RenderTexture Tx;
	}
}
