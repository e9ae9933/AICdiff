using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerTrmBed : M2CImgDrawer, IActivatable
	{
		public M2CImgDrawerTrmBed(MeshDrawer Md, int lay, M2Puts _Cp)
			: base(Md, lay, _Cp, true)
		{
			this.use_color = true;
			this.rewriteable_alpha = true;
		}

		protected override void entryMainPicToMeshInner(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			base.entryMainPicToMeshInner(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			Color32 col = Md.Col;
			Md.Col = this.ManColor;
			base.entryMainPicToMeshInner(Md, meshx, meshy, _zmx, _zmy, _rotR, this.Cp.Img.MeshB);
			Md.Col = col;
		}

		public Color32 ManColor
		{
			get
			{
				Color32 c = this.Cp.Lay.LayerColor.C;
				if (this.is_active)
				{
					c.a = 0;
				}
				return c;
			}
		}

		public void activate()
		{
			this.is_active = true;
			this.need_reposit_flag = true;
		}

		public void deactivate()
		{
			this.is_active = false;
			this.need_reposit_flag = true;
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		public override int redraw(float fcnt)
		{
			if (this.need_reposit_flag)
			{
				Color32 manColor = this.ManColor;
				for (int i = 4; i < base.Length; i++)
				{
					this.ACol[i] = manColor;
				}
			}
			return base.redraw(fcnt);
		}

		private bool is_active;
	}
}
