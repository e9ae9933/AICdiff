using System;

namespace XX
{
	public class OneLineBox : MsgBox
	{
		protected override void Awake()
		{
			base.Awake();
			base.margin(new float[] { 70f, 8f });
			base.Align(ALIGN.CENTER, ALIGNY.MIDDLE);
		}

		protected override MsgBox redrawBg(float w = 0f, float h = 0f, bool update_mrd = true)
		{
			if (!this.ginitted)
			{
				return this;
			}
			if (w <= 0f)
			{
				w = base.swidth;
			}
			if (h <= 0f)
			{
				h = base.sheight;
			}
			w /= 64f;
			h /= 64f;
			float num = w / 2f;
			float num2 = h / 2f;
			float num3 = this.lmarg * 0.015625f;
			float num4 = this.rmarg * 0.015625f;
			this.MdBkg.clear(false, false);
			this.MdBkg.Col = this.MdBkg.ColGrd.Set(this.bg_col).C;
			C32 c = this.MdBkg.ColGrd.mulA(0f);
			this.MdBkg.Tri(0, 1, 3, false).Tri(0, 3, 2, false).Tri(2, 3, 5, false)
				.Tri(2, 5, 4, false)
				.Tri(4, 5, 7, false)
				.Tri(4, 7, 6, false);
			this.MdBkg.Pos(-num, -num2, c).Pos(-num, num2, c).Pos(-num + num3, -num2, null)
				.Pos(-num + num3, num2, null)
				.Pos(num - num4, -num2, null)
				.Pos(num - num4, num2, null)
				.Pos(num, -num2, c)
				.Pos(num, num2, c);
			this.MMRD.GetArranger(this.MdBkg).SetWhole(true).mulAlpha(this.alpha_);
			if (update_mrd)
			{
				this.MdBkg.updateForMeshRenderer(true);
			}
			return this;
		}

		public override MsgBox make(string _tex)
		{
			return this.make(_tex, true);
		}

		public MsgBox make(string _tex, bool refining)
		{
			if (!refining && this.Tx.textIs(_tex))
			{
				return this;
			}
			base.make(_tex);
			this.wh((this.Tx != null) ? (this.Tx.get_swidth_px() + 20f) : this.w, this.Tx.size * 1.2f);
			if (base.isActive() && refining)
			{
				this.showt = 0;
				this.t = 0f;
			}
			return this;
		}
	}
}
