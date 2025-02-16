using System;
using UnityEngine;

namespace XX
{
	public class ButtonSkinMeterScroll : ButtonSkinMeterNormal
	{
		public ButtonSkinMeterScroll(aBtnMeter _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
			this.fine_on_binding_changing = false;
		}

		protected override void initCircleRenderer()
		{
		}

		public void setColor(Color32 _BarColor, Color32 _PushColor)
		{
			this.BarColor = _BarColor;
			this.BarPushedColor = _PushColor;
			this.fine_flag = true;
		}

		protected override void redrawMemori()
		{
			this.Md.Col = (base.isPushed() ? this.BarPushedColor : (base.isFocused() ? MTRX.cola.Set(this.BarColor).blend(this.BarPushedColor, 0.25f).C : this.BarColor));
			this.Md.ColGrd.Set(this.Md.Col).mulA(0.5f * this.alpha_);
			float num = this.h * 64f;
			float moveable_width = base.moveable_width;
			float num2 = base.meter_x * (float)(base.vertical ? (-1) : 1);
			float num3 = num * 0.3f;
			if (base.vertical)
			{
				this.Md.RectBL(-num3, 0f, num3 * 2f, 1f, false);
			}
			else
			{
				this.Md.RectBL(0f, -num3, 1f, num3 * 2f, false);
			}
			float num4 = base.moveable_width * 64f * this.Meter.getCurrentIndex() / this.Meter.valcnt;
			float num5 = num4 + this.Meter.px_bar_width;
			if (num4 > 0f)
			{
				if (base.vertical)
				{
					this.Md.LineDif(0f, num2, 0f, -num4, 1f, false, 1f, 0f);
				}
				else
				{
					this.Md.LineDif(num2, 0f, num4, 0f, 1f, false, 1f, 0f);
				}
			}
			if (num5 < this.w * 64f)
			{
				if (base.vertical)
				{
					this.Md.LineDif(0f, num2 - num5, 0f, -(this.w * 64f - num5), 1f, false, 0f, 1f);
				}
				else
				{
					this.Md.LineDif(num2 + num5, 0f, this.w * 64f - num5, 0f, 1f, false, 0f, 1f);
				}
			}
			if (base.vertical)
			{
				this.Md.KadomaruRect(0f, num2 - (num5 + num4) / 2f, num, this.Meter.px_bar_width, num, 0f, false, 0f, 0f, false);
				this.Md.Circle(0f, num2 - 1f, 1.8f, 0f, false, 0f, 0f);
				this.Md.Circle(0f, num2 - (this.w * 64f - 1f), 1.8f, 0f, false, 0f, 0f);
			}
			else
			{
				this.Md.KadomaruRect(num2 + (num5 + num4) / 2f, 0f, this.Meter.px_bar_width, num, num, 0f, false, 0f, 0f, false);
				this.Md.Circle(num2 + 1f, 0f, 1.8f, 0f, false, 0f, 0f);
				this.Md.Circle(num2 + this.w * 64f - 1f, 0f, 1.8f, 0f, false, 0f, 0f);
			}
			this.Md.updateForMeshRenderer(false);
		}

		protected override void redrawMarker()
		{
		}

		public override void bindChanged(bool f)
		{
			base.bindChanged(f);
			this.setEnable(f);
		}

		private Color32 BarColor = C32.d2c(2868312793U);

		private Color32 BarPushedColor = C32.d2c(4289247231U);
	}
}
