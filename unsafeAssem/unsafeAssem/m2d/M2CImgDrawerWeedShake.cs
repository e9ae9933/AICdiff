using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2CImgDrawerWeedShake : M2CImgDrawer, IActivatable
	{
		public M2CImgDrawerWeedShake(MeshDrawer Md, int _lay, M2Puts _Cp, META Meta, string tag_key)
			: base(Md, _lay, _Cp, true)
		{
			this.index = this.Cp.index;
			this.use_shift = true;
			this.slice_x = X.Mx(Meta.GetI(tag_key, 0, 0), 1);
			this.slice_y = X.Mx(Meta.GetI(tag_key, 0, 1), 1);
			this.wait_min = Meta.GetI("anim_delay", 45, 0);
			this.wait_max = Meta.GetI("anim_delay", 141, 1);
			this.shift_pixel_min = Meta.GetNm("shift_pixel", 4.6f, 0);
			this.shift_pixel_max = Meta.GetNm("shift_pixel", 9.3f, 1);
			this.setWait();
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			if (Ms.vertexCount != (this.slice_x + 1) * (this.slice_y + 1))
			{
				base.SliceRectMesh(Ms, this.slice_x, this.slice_y);
			}
			return base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
		}

		public override int redraw(float fcnt)
		{
			this.wait += fcnt * (float)((this.activate_count > 0) ? 3 : 1);
			if (this.wait < 0f && !this.force_redraw)
			{
				return base.redraw(fcnt);
			}
			this.dcnt += fcnt;
			if (this.dcnt >= (float)((this.activate_count > 0) ? 2 : 8))
			{
				this.dcnt = 0f;
			}
			else if (!this.force_redraw)
			{
				return base.redraw(fcnt);
			}
			this.force_redraw = false;
			float num = this.lvl * X.ZSINV(80f - this.wait, 80f);
			int num2 = this.slice_x + 1;
			int num3 = this.slice_y + 1;
			int num4 = num2 * num3;
			for (int i = 0; i < num4; i++)
			{
				Vector3 vector = this.ASh[i];
				int num5 = i % num2;
				int num6 = i / num2;
				float num7 = X.ZSIN((float)num6 / (float)this.slice_y);
				float num8 = X.MMX(-0.5f, X.SINI(X.Mx(0f, this.wait - (float)num6 * 3.3f), 60f), 0.5f);
				vector.x = num7 * num * num8;
				this.ASh[i] = vector;
			}
			if (this.wait >= 80f)
			{
				if (this.activate_count > 0)
				{
					int num9 = this.activate_count - 1;
					this.activate_count = num9;
					if (num9 > 0)
					{
						this.wait = 0f;
						goto IL_018A;
					}
				}
				this.setWait();
				IL_018A:
				this.dcnt = 0f;
			}
			this.need_reposit_flag = true;
			return base.redraw(fcnt);
		}

		public void setWait()
		{
			this.anim_index++;
			uint ran = X.GETRAN2(this.Cp.index + this.anim_index * 13, 4 + (this.anim_index & 7));
			this.wait = (float)((int)(-(int)X.NI(this.wait_min, this.wait_max, X.RAN(ran, 2453))));
			this.lvl = X.NI(this.shift_pixel_min, this.shift_pixel_max, X.RAN(ran, 1701));
		}

		public void activate()
		{
			if (base.Mp.floort < 5f)
			{
				return;
			}
			this.activate_count = 2;
			this.dcnt = 2f;
			this.wait = 0f;
			this.force_redraw = true;
		}

		public void deactivate()
		{
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		protected int slice_x;

		protected int slice_y;

		protected int wait_min;

		protected int wait_max;

		private int index;

		private float wait = -90f;

		private float dcnt;

		private float lvl = 3f;

		private int anim_index;

		private int activate_count;

		private float shift_pixel_min;

		private float shift_pixel_max;

		protected bool force_redraw;
	}
}
