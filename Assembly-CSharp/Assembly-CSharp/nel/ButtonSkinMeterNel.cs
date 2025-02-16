using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class ButtonSkinMeterNel : ButtonSkinMeterNormal
	{
		public ButtonSkinMeterNel(aBtnMeter _B, float _w = 0f, float _h = 0f)
			: base(_B, (_w <= 0f) ? 200f : _w, _h)
		{
			this.do_not_write_default_title = true;
			this.MeterCol = C32.d2c(4283780170U);
			this.PFSelected = MTR.MeshMarker_Sel;
			this.PFNormal = MTR.MeshMarker;
		}

		public ButtonSkinMeterNel Darken()
		{
			this.MeterCol = MTRX.ColWhite;
			this.PFSelected = MTR.MeshMarkerDark_Sel;
			this.PFNormal = MTR.MeshMarkerDark;
			return this;
		}

		protected override void initCircleRenderer()
		{
			this.MdCircle = base.makeMesh(BLEND.NORMAL, MTRX.MIicon);
		}

		protected override void redrawMemori()
		{
			float num = this.h * 64f;
			float num2 = base.moveable_width * 64f;
			float meter_x = base.meter_x;
			this.Md.Col = this.Md.ColGrd.Set(this.MeterCol).mulA(this.alpha_ * 0.5f).C;
			float num3 = -(num * 0.45f);
			this.Md.Line(meter_x, num3, meter_x + num2, num3, 2f, false, 0f, 0f);
			int num4 = 0;
			while ((float)num4 <= this.Meter.valcnt)
			{
				if (this.Amemori[num4] != 0f)
				{
					float num5 = (float)X.IntR(meter_x + num2 * (float)num4 / this.Meter.valcnt);
					this.Md.Line(num5, num3, num5, num3 + (float)X.IntR(num * (this.Amemori[num4] * 0.65f)), 2f, false, 0f, 0f);
				}
				num4++;
			}
			if ((base.isFocused() && !base.isPushed()) || (!base.isFocused() && base.isPushed()))
			{
				bool use_mouse = IN.use_mouse;
				this.MdStripeB.Col = this.Md.ColGrd.Set(this.MeterCol).mulA(this.alpha_).C;
				float num6 = this.swidth + 9f;
				this.MdStripeB.RectDashedMBL(meter_x - 4f, -num / 2f - 3f, num6, num + 6f, 20, 1f, 0.5f, false, false);
			}
			if (base.isLocked())
			{
				this.Md.Col = this.Md.ColGrd.Set(this.MeterCol).mulA(this.alpha_).C;
				float num7 = base.meter_x + num2 / 2f;
				this.Md.Line(num7 + num2 / 2f + 6f, num / 2f + 3f, num7 - num2 / 2f - 6f, -num / 2f - 3f, 2f, false, 0f, 0f);
			}
			this.Md.updateForMeshRenderer(false);
			this.MdStripeB.updateForMeshRenderer(false);
		}

		protected override void redrawMarker()
		{
			float num = this.h * 64f;
			float num2 = base.moveable_width * 64f;
			float meter_x = base.meter_x;
			this.MdCircle.Col = this.MdCircle.ColGrd.White().mulA(this.alpha_).C;
			float num3 = meter_x + num2 * this.Meter.getCurrentIndex() / this.Meter.valcnt;
			this.MdCircle.RotaPF(num3, num * (base.isPushed() ? 0.13f : 0.25f), 1f, 1f, 0f, base.isPushed() ? this.PFSelected : this.PFNormal, false, false, false, uint.MaxValue, false, 0);
			this.MdCircle.updateForMeshRenderer(false);
		}

		public Color32 MeterCol;

		public PxlFrame PFSelected;

		public PxlFrame PFNormal;
	}
}
