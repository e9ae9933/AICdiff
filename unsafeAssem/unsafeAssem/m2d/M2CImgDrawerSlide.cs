using System;
using PixelLiner;
using XX;

namespace m2d
{
	public class M2CImgDrawerSlide : M2CImgDrawer
	{
		public M2CImgDrawerSlide(MeshDrawer Md, int _layer, M2Puts _Cp, bool _redraw_flag = false)
			: base(Md, _layer, _Cp, _redraw_flag)
		{
			this.use_shift = true;
			_Cp.arrangeable = true;
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			this.cur_meshx = meshx;
			this.base_meshx = meshx;
			this.cur_meshy = meshy;
			this.base_meshy = meshy;
			return base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
		}

		public void quake(float xlevel, float ylevel)
		{
			this.quake_x = X.SINI((float)((int)(base.Mp.floort / 2f)), 3.34f) * xlevel;
			this.quake_y = X.SINI((float)((int)(base.Mp.floort / 2f)), 2.39f) * ylevel;
			this.fineShiftValue();
		}

		public bool slideToByAnimating(float meshx, float meshy, float spd)
		{
			if (meshx == this.cur_meshx && meshy == this.cur_meshy)
			{
				return false;
			}
			this.cur_meshx = X.VALWALK(this.cur_meshx, meshx, spd);
			this.cur_meshy = X.VALWALK(this.cur_meshy, meshy, spd);
			this.fineShiftValue();
			return true;
		}

		public bool slideTo(float meshx, float meshy)
		{
			if (meshx == this.cur_meshx && meshy == this.cur_meshy)
			{
				return false;
			}
			this.cur_meshx = meshx;
			this.cur_meshy = meshy;
			this.fineShiftValue();
			return true;
		}

		public virtual void fineShiftValue()
		{
			if (this.ASh == null)
			{
				return;
			}
			int num = this.ASh.Length;
			float num2 = this.cur_meshx - this.base_meshx + this.quake_x;
			float num3 = this.cur_meshy - this.base_meshy + this.quake_y;
			for (int i = 0; i < num; i++)
			{
				this.ASh[i].Set(num2, num3, 0f);
			}
			this.need_reposit_flag = true;
			base.Mp.addUpdateMesh(this.redraw(0f), false);
		}

		protected float base_meshx;

		protected float base_meshy;

		protected float cur_meshx;

		protected float cur_meshy;

		protected float quake_x;

		protected float quake_y;
	}
}
