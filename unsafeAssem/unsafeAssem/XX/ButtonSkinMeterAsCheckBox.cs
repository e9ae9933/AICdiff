using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class ButtonSkinMeterAsCheckBox : ButtonSkinMeterNormal
	{
		public ButtonSkinMeterAsCheckBox(aBtnMeter _B, float _w = 0f, float _h = 0f, bool _force_multiply_view = false)
			: base(_B, _w, _h)
		{
			this.fine_continue_flags = 5U;
			this.do_not_write_default_title = true;
			this.force_multiply_view = _force_multiply_view;
		}

		protected override void initCircleRenderer()
		{
			this.MdCircle = base.makeMesh(BLEND.NORMAL, MTRX.MIicon);
		}

		protected override void redrawMarker()
		{
		}

		protected override void redrawMemori()
		{
		}

		protected virtual void drawCheckMark(MeshDrawer Md, float x, float y, float level)
		{
			if (level == 0f)
			{
				Md.clear(false, false);
				return;
			}
			if (this.PFCheckMark == null)
			{
				this.PFCheckMark = MTRX.PFCheckBoxChecked;
			}
			Md.Col = Md.ColGrd.Set(base.isLocked() ? this.LockedLineColor : this.LineColor).mulA(level).C;
			Md.RotaPF(x, y, 1f, 1f, 0f, this.PFCheckMark, false, false, false, uint.MaxValue, false, 0);
			Md.updateForMeshRenderer(false);
		}

		public override ButtonSkin Fine()
		{
			base.Fine();
			float num = this.h * 64f;
			float num2 = base.meter_x + base.moveable_width * 64f / 2f;
			int num3 = (int)this.Meter.getValue();
			if (this.pre_drawn_value != num3)
			{
				this.pre_drawn_value = num3;
				if (this.changed_f0 < 0)
				{
					this.changed_f0 = IN.totalframe;
				}
				else
				{
					this.changed_f0 = IN.totalframe + 12;
				}
			}
			this.Md.Col = C32.MulA(base.isLocked() ? this.LockedLineColor : this.LineColor, this.alpha_);
			float num4 = ((this.changed_f0 < 0) ? 1f : (1f - X.ZLINE((float)(this.changed_f0 - IN.totalframe), 12f)));
			if ((int)this.Meter.maxval <= 1 && !this.force_multiply_view)
			{
				if (!this.Md.hasMultipleTriangle())
				{
					this.Md.chooseSubMesh(1, false, false);
					this.Md.setMaterial(MTRX.getMtr(MTRX.MtrMeshDashLine.shader, base.container_stencil_ref), false);
					this.Md.connectRendererToTriMulti(base.getMeshRenderer(this.Md));
				}
				this.Md.chooseSubMesh(0, false, false);
				float num5 = X.Mn(base.moveable_width * 64f, num * 2.1f);
				bool flag = num3 > 0;
				this.Md.KadomaruRect(num2, 0f, num5, num, num, 1f, false, 0f, 0f, false);
				if (base.isHoveredOrPushOut())
				{
					this.Md.chooseSubMesh(1, false, false);
					this.Md.ButtonKadomaruDashedM(num2, 0f, num5, num, num, 24, 2f, false, 0.5f, -1);
				}
				this.Md.chooseSubMesh(0, false, false);
				float num6 = num - 4f;
				float num7 = num5 - 4f;
				float num8 = num6 / 2f;
				float num9 = num7 - num6;
				float num10 = num2 - num7 / 2f + num8;
				Color32 color = (base.isLocked() ? (flag ? this.BaseCheckedColor : this.LockedBaseColor) : (flag ? this.BaseCheckedColor : this.BaseColor));
				Color32 color2 = (base.isLocked() ? ((!flag) ? this.LockedBaseCheckedColor : this.LockedBaseColor) : ((!flag) ? this.BaseCheckedColor : this.BaseColor));
				if (num4 < 1f)
				{
					this.Md.Col = this.Md.ColGrd.Set(color2).blend(color, num4).mulA(this.alpha_)
						.C;
				}
				else
				{
					this.Md.Col = C32.MulA(color, this.alpha_);
				}
				this.Md.KadomaruRect(num2, 0f, num7, num6, num, 0f, false, 0f, 0f, false);
				float num11 = (float)(flag ? 1 : 0);
				if (num4 < 1f)
				{
					this.Md.Col = this.Md.ColGrd.Set(color2).blend(color, num4).mulA(this.alpha_)
						.C;
					num11 = X.NI(1f - num11, num11, num4);
				}
				else
				{
					this.Md.Col = C32.MulA(color, this.alpha_);
				}
				num10 += num11 * num9;
				if (this.isPushDown())
				{
					this.Md.Col = this.Md.ColGrd.Set(this.MarkerPushedColor).mulA(this.alpha_).C;
					this.Md.Circle(num10, 0f, num8, 0f, false, 0f, 0f);
					this.Md.Col = this.Md.ColGrd.Set(this.LineColor).blend(this.MarkerPushedColor, 0.33f).mulA(this.alpha_)
						.C;
					this.Md.Circle(num10, 0f, num8, 1f, false, 0f, 0f);
				}
				else
				{
					color2 = C32.MulA(base.isLocked() ? this.Md.ColGrd.Set(this.MarkerColor).blend(this.LockedBaseColor, 0.6f).C : this.MarkerColor, this.alpha_);
					if (base.isHoveredOrPushOut())
					{
						this.Md.Col = C32.MulA(this.Md.ColGrd.Set(this.MarkerPushedColor).blend(color2, 0.66f + 0.33f * X.COSIT(40f)).C, this.alpha_);
					}
					else
					{
						this.Md.Col = C32.MulA(color2, this.alpha_);
					}
					this.Md.Circle(num10, 0f, num8, 0f, false, 0f, 0f);
					this.Md.Col = C32.MulA(this.Md.ColGrd.Set(base.isLocked() ? this.LockedLineColor : this.LineColor).C, this.alpha_);
					this.Md.Circle(num10, 0f, num8, 1f, false, 0f, 0f);
				}
				this.drawCheckMark(this.MdCircle, num2 + num5 / 2f + num * 0.57f, 0f, num11);
				if (base.isLocked())
				{
					this.Md.Col = C32.MulA(this.LineColor, this.alpha_);
					this.Md.Line(num2 + num5 / 2f + 6f, num / 2f + 3f, num2 - num5 / 2f - 6f, -num / 2f - 3f, 2f, false, 0f, 0f);
				}
			}
			else
			{
				float num12 = base.moveable_width * 64f;
				float num13 = num12 / this.Meter.maxval;
				float num14 = X.MMX(num * 0.55f, num13 * 0.75f, num * 0.75f);
				float num15 = num14 / 2f;
				num13 = X.Mn(num13, num14);
				int num16 = X.IntR(this.Meter.maxval);
				float num17 = base.meter_x + num12 / 2f - num13 * (float)(num16 - 1) * 0.5f;
				Color32 col = this.Md.Col;
				for (int i = 0; i <= num16; i++)
				{
					this.Md.Daia2(num17, 0f, num15 + 1f, 2f, false);
					if (i == num3)
					{
						this.Md.Col = (this.isPushDown() ? C32.MulA(this.BaseCheckedColor, this.alpha_) : col);
						this.Md.Daia2(num17, 0f, num15 * 0.5f, 0f, false);
						this.Md.Col = col;
						this.Md.Tri012().PosD(num17, num15 + 4f, null).PosD(num17 - 6f, num15 + 4f + 6f, null)
							.PosD(num17 + 6f, num15 + 4f + 6f, null);
						this.Md.Tri012().PosD(num17, -num15 - 4f, null).PosD(num17 + 6f, -num15 - 4f - 6f, null)
							.PosD(num17 - 6f, -num15 - 4f - 6f, null);
					}
					num17 += num13;
				}
			}
			this.Md.updateForMeshRenderer(false);
			if (num4 < 1f)
			{
				this.fine_flag = true;
			}
			return this;
		}

		protected Color32 LineColor = C32.d2c(4278190080U);

		protected Color32 BaseColor = C32.d2c(4293388263U);

		protected Color32 BaseCheckedColor = C32.d2c(4285249791U);

		protected Color32 MarkerColor = C32.d2c(uint.MaxValue);

		protected Color32 MarkerPushedColor = C32.d2c(4288132607U);

		protected Color32 LockedLineColor = C32.d2c(4284900966U);

		protected Color32 LockedBaseColor = C32.d2c(4292532954U);

		protected Color32 LockedBaseCheckedColor = C32.d2c(4288388527U);

		private int changed_f0 = -1;

		private int pre_drawn_value;

		private const int T_ANM = 12;

		public bool force_multiply_view;

		private PxlFrame PFCheckMark;
	}
}
