using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerBrainVat : M2CImgDrawer
	{
		public M2CImgDrawerBrainVat(MeshDrawer Md, int lay, M2Puts _Cp)
			: base(Md, lay, _Cp, true)
		{
			this.use_shift = true;
			this.target_lay = this.Cp.getMeta().GetI("brainvat", -1, 0);
			this.bubble_shift_y = this.Cp.getMeta().GetI("brainvat", 25, 1);
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			base.Set(false);
			for (int i = 0; i < 3; i++)
			{
				this.ver_bubble += 4;
				PxlMeshDrawer srcMesh = this.Cp.Img.getSrcMesh(i);
				if (i == this.target_lay)
				{
					this.base_iwidth = srcMesh.mesh_width;
					this.base_iheight = srcMesh.mesh_height;
				}
				this.entryMainPicToMeshInner(Md, meshx, meshy, _zmx, _zmy, _rotR, srcMesh);
			}
			base.M2D.IMGS.Atlas.initForRectWhite(Md);
			Color32 col = Md.Col;
			Md.Col = C32.MulA(Md.Col, 0.4f);
			for (int j = 0; j < 10; j++)
			{
				Md.Rect(meshx + (-0.5f + ((float)j + 0.5f) / 10f) * (this.base_iwidth - 4f), meshy - this.base_iheight * 0.5f + 2f, 1f, 1f, false);
			}
			Md.Col = col;
			base.Set(false);
			base.initMdArray();
			if (this.Cp.active_removed)
			{
				base.repositActiveRemoveFlag();
			}
			return this.redraw_flag;
		}

		public override int redraw(float fcnt)
		{
			try
			{
				float num = (float)(2 + X.IntR(X.COSI(base.Mp.floort, 180f) * 2.4f));
				for (int i = this.ver_bubble - 8; i < this.ver_bubble; i++)
				{
					if (i != this.ver_bubble - 3 && i != this.ver_bubble - 2)
					{
						this.ASh[i] = new Vector3(0f, num, 0f);
					}
				}
				int num2 = this.ver_bubble;
				int num3 = 0;
				while (num3 < 10 && num2 < this.ASh.Length)
				{
					uint ran = X.GETRAN2(this.Cp.index + num3 * 7, num3 % 9);
					float num4 = 3f * (-1f + 2f * X.RAN(ran, 679));
					float num5 = (float)(num3 * 39) + X.RAN(ran, 1516) * 65f;
					float num6 = X.NI(45, 55, X.RAN(ran, 1190));
					float num7 = (base.Mp.floort + num5) % num6 / num6;
					float num8 = num4 + (float)X.IntR(X.NI(0.9f, 1.77f, X.RAN(ran, 1650)) * X.COSI(base.Mp.floort + num7 * num6, X.NI(28.4f, 42.7f, X.RAN(ran, 2562))));
					int num9 = 0;
					while (num9 < 4 && num2 < this.ASh.Length)
					{
						this.ASh[num2++] = new Vector3(num8, (float)this.bubble_shift_y * num7, 0f);
						num9++;
					}
					num3++;
				}
				this.need_reposit_flag = true;
			}
			catch
			{
			}
			return base.redraw(fcnt);
		}

		private const int BUBBLE_MAX = 10;

		private int ver_bubble;

		private int target_lay;

		private int bubble_shift_y;

		private float base_iwidth;

		private float base_iheight;
	}
}
