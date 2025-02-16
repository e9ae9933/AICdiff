using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class ButtonSkinNumCounter : ButtonSkin
	{
		public ButtonSkinNumCounter(aBtnNumCounter _B, float _w = 0f, float _h = 0f)
			: base(_B, 0f, 0f)
		{
			this.BCnt = _B;
			this.w = ((_w > 0f) ? _w : 24f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 48f) * 0.015625f;
			this.fine_continue_flags = 5U;
			this.Md = base.makeMesh(MTRX.MtrMeshNormal);
			this.MdC = base.makeMesh(BLEND.NORMALP2, MTRX.MIicon);
			this.MdCursor = base.makeMesh(BLEND.NORMAL, MTRX.MIicon);
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f)
			{
				return this;
			}
			float num = this.w * 0.5f;
			float num2 = this.h * 0.5f;
			float num3 = X.ZPOW(X.Abs(this.BCnt.slide_t), 20f);
			float num4 = num3 * (float)X.MPF(this.BCnt.slide_t > 0f);
			this.MdC.clear(true, false);
			this.MdC.Col = C32.MulA(MTRX.ColWhite, 1f);
			float num5 = 0f + num2 * 64f * num4;
			float num6 = this.alpha;
			this.MdC.ColGrd.Set(4278190080U);
			if (base.isHoveredOrPushOut())
			{
				this.MdC.ColGrd.blend(uint.MaxValue, 0.25f + 0.25f * X.COSIT(40f));
			}
			if (this.BCnt.unuse_digit)
			{
				num6 *= 0.2f;
			}
			this.MdC.Uv23(C32.MulA(this.MdC.ColGrd.C, num6 * (1f - num3)), false);
			STB stb = TX.PopBld(null, 0);
			if (this.effect_confusion)
			{
				stb += '?';
			}
			else
			{
				stb += this.BCnt.getValue();
			}
			this.BCnt.Chr.DrawScaleStringTo(this.MdC, stb, 0f, num5, this.BCnt.chr_scale, this.BCnt.chr_scale, ALIGN.CENTER, ALIGNY.MIDDLE, false, 0f, 0f, null);
			this.MdC.allocUv23(0, true);
			if (num3 > 0f)
			{
				this.draw_main_reel = true;
				stb.Clear();
				if (this.effect_confusion)
				{
					stb += '?';
				}
				else
				{
					stb += (this.BCnt.getValue() + X.MPF(num4 > 0f) + 10) % 10;
				}
				num5 -= num2 * 64f * (float)X.MPF(num4 > 0f);
				this.MdC.Col = C32.MulA(this.MdC.ColGrd.setA(1f).C, num6 * num3);
				this.MdC.Uv23(this.MdC.ColGrd.Set(4278190080U).C, false);
				this.BCnt.Chr.DrawScaleStringTo(this.MdC, stb, 0f, num5, this.BCnt.chr_scale, this.BCnt.chr_scale, ALIGN.CENTER, ALIGNY.MIDDLE, false, 0f, 0f, null);
				this.MdC.allocUv23(0, true);
			}
			this.MdC.updateForMeshRenderer(false);
			if (base.isFocused())
			{
				this.cursor_wrote = true;
				float num7 = (float)(X.ANMT(2, 15f) * 6);
				this.MdCursor.Col = C32.MulA(this.CArrow, this.alpha);
				float num8 = num2 * 1.25f * 64f;
				float num9 = -num8;
				PxlFrame pxlFrame = MTRX.MeshArrowR;
				PxlFrame pxlFrame2 = MTRX.MeshArrowR;
				if (this.BCnt.slide_t > 16f)
				{
					pxlFrame2 = MTRX.MeshArrowRShifted;
					num9 -= 12f;
				}
				if (this.BCnt.slide_t < -16f)
				{
					pxlFrame = MTRX.MeshArrowRShifted;
					num8 += 12f;
				}
				this.MdCursor.RotaPF(0f, num8 + num7, 1f, 1f, 1.5707964f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
				this.MdCursor.RotaPF(0f, num9 - num7, 1f, 1f, -1.5707964f, pxlFrame2, false, false, false, uint.MaxValue, false, 0);
				this.MdCursor.updateForMeshRenderer(false);
			}
			else if (this.cursor_wrote)
			{
				this.cursor_wrote = false;
				this.MdCursor.updateForMeshRenderer(false);
			}
			TX.ReleaseBld(stb);
			if (this.draw_main_reel)
			{
				this.draw_main_reel = false;
				this.Md.clear(false, false);
				this.Md.Col = this.Md.ColGrd.Set(this.CFront).blend(this.CFrontHilighted, num3).mulA(this.alpha)
					.C;
				this.Md.ColGrd.Set(this.COut).blend(this.COutHilighted, num3).mulA(this.alpha);
				this.Md.TriRectBL(0).Tri(0, 3, 4, false).Tri(4, 3, 5, false);
				this.Md.Pos(-num, 0f, null).Pos(-num, num2, this.Md.ColGrd).Pos(num, num2, this.Md.ColGrd)
					.Pos(num, 0f, null)
					.Pos(-num, -num2, this.Md.ColGrd)
					.Pos(num, -num2, this.Md.ColGrd);
				this.Md.Col = this.Md.ColGrd.Set(this.COut).mulA(this.alpha).C;
				if (this.B.carr_index == 0)
				{
					this.Md.Line(-num, -num2, -num, num2, 0.015625f, true, 0f, 0f);
				}
				this.Md.Line(num, -num2, num, num2, 0.015625f, true, 0f, 0f);
				this.Md.updateForMeshRenderer(false);
			}
			return base.Fine();
		}

		public override float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				if (this.alpha_ != value)
				{
					base.alpha = value;
					this.draw_main_reel = true;
				}
			}
		}

		private aBtnNumCounter BCnt;

		protected MeshDrawer Md;

		protected MeshDrawer MdC;

		protected MeshDrawer MdCursor;

		private bool draw_main_reel = true;

		public bool effect_confusion;

		public Color32 CFront = C32.d2c(4292728272U);

		public Color32 CFrontHilighted = C32.d2c(4294506744U);

		public Color32 COut = C32.d2c(2858508640U);

		public Color32 COutHilighted = C32.d2c(2862389397U);

		public Color32 CDigitUnused = C32.d2c(1140850688U);

		public Color32 CArrow = C32.d2c(uint.MaxValue);

		private bool cursor_wrote;

		private const float SLIDE_ANIM_T = 20f;
	}
}
