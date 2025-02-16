using System;
using m2d;
using XX;

namespace nel
{
	public sealed class M2LpSacredMistEffector : NelLp
	{
		public M2LpSacredMistEffector(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			this.Mst = new M2FillingMistDrawer(null, MTR.AEfSmokeL)
			{
				C = C32.d2c(4292212954U),
				len_divide = 0.0020833334f,
				thresh_len_pixel = 190f,
				draw_scale = 4f
			};
			float num = 0.5f;
			if (TX.valid(this.comment))
			{
				META meta = new META(this.comment);
				float num2 = meta.GetNm("thresh_len", -1000f, 0);
				if (num2 != -1000f)
				{
					this.Mst.thresh_len_pixel = num2;
				}
				num2 = meta.GetNm("len", -1000f, 0);
				if (num2 != -1000f)
				{
					this.Mst.len_divide = 1f / num2;
				}
				num2 = meta.GetNm("draw_scale", 0f, 0);
				if (num2 != 0f)
				{
					this.Mst.draw_scale = num2;
				}
				num2 = meta.GetNm("alpha", -1000f, 0);
				if (num2 != -1000f)
				{
					num = num2;
				}
			}
			this.Mst.C = C32.MulA(this.Mst.C, num);
			this.Mst.assignTo(this.Mp);
			this.Mst.fine();
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			if (this.Mst != null)
			{
				this.Mst.destruct();
				this.Mst = null;
			}
		}

		private M2FillingMistDrawer Mst;
	}
}
