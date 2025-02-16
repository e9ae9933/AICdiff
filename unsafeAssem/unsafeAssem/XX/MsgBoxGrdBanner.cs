using System;

namespace XX
{
	public class MsgBoxGrdBanner : MsgBox
	{
		public MsgBoxGrdBanner GradationDirect(float grd_lr, float grd_tb)
		{
			this.grd_h = grd_lr;
			this.grd_v = grd_tb;
			return this;
		}

		protected override void redrawBgMeshInner(MeshDrawer Md, MdArranger Ma, float wpx, float hpx)
		{
			if (this.grd_h <= 0f && this.grd_v <= 0f)
			{
				base.redrawBgMeshInner(Md, Ma, wpx, hpx);
				return;
			}
			Ma.Set(true);
			Md.ColGrd.Set(Md.Col).setA(0f);
			if (this.grd_h <= 0f || this.grd_v <= 0f)
			{
				if (this.grd_h > 0f)
				{
					Md.BluredBanner(0f, -hpx / 2f, 0f, hpx / 2f, wpx - this.grd_h * 2f, wpx, Md.ColGrd, false);
				}
				else if (this.grd_v > 0f)
				{
					Md.BluredBanner(-wpx / 2f, 0f, wpx / 2f, 0f, hpx - this.grd_v * 2f, hpx, Md.ColGrd, false);
				}
				Ma.Set(false);
				return;
			}
			if (this.grd_h > this.grd_v)
			{
				float num = this.grd_v / this.grd_h;
				Md.Scale(1f, num, false).KadomaruRect(0f, 0f, wpx, hpx / num, this.grd_h, 0f, false, 1f, 0f, false).Identity();
				return;
			}
			float num2 = this.grd_h / this.grd_v;
			Md.Scale(num2, 1f, false).KadomaruRect(0f, 0f, wpx / num2, hpx, this.grd_v, 0f, false, 1f, 0f, false).Identity();
		}

		public float grd_h;

		public float grd_v;
	}
}
