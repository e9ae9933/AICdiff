using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class TreasureBoxDrawer
	{
		public TreasureBoxDrawer(float _w = -1f)
		{
			this.ColT = new C32(4291812578U);
			this.ColB = new C32(4287601826U);
			this.ColLine = new C32(4289180826U);
			this.ColIcon = new C32(4284572774U);
			if (TreasureBoxDrawer.Ma == null)
			{
				TreasureBoxDrawer.Ma = new MdArranger(null);
				TreasureBoxDrawer.px = 0.015625f;
				TreasureBoxDrawer.lt = TreasureBoxDrawer.px * 2f;
			}
			this.Set(_w, 0f);
		}

		public TreasureBoxDrawer Set(float pixel_w, float pixel_center_w = 0f)
		{
			this.w = pixel_w * 0.015625f;
			this.center_w = ((pixel_center_w == 0f) ? (this.w * 0.35f) : (pixel_center_w * 0.015625f));
			return this;
		}

		public TreasureBoxDrawer Level(float _level)
		{
			this.level = X.MMX(0f, _level, 2f);
			return this;
		}

		public virtual TreasureBoxDrawer drawTo(MeshDrawer Md, MeshDrawer MdS, MeshDrawer MdIcon, float x, float y, float scale = 1f)
		{
			TreasureBoxDrawer.cx = x * 0.015625f;
			TreasureBoxDrawer.cy = y * 0.015625f;
			float num = this.w / 2f;
			float num2 = this.center_w / 2f;
			float num3 = 1f - X.ZLINE(this.level - 1.5f, 0.5f);
			TreasureBoxDrawer.Ma.clear(Md).Set(true);
			Matrix4x4 currentMatrix = Md.getCurrentMatrix();
			Md.Scale(scale, scale, false);
			TreasureBoxDrawer.PreTrs = Md.getCurrentMatrix();
			MdS.setCurrentMatrix(TreasureBoxDrawer.PreTrs, false);
			MdS.Col = MdS.ColGrd.Set(this.ColLine).mulA(num3).C;
			float num4 = 1f;
			float num5 = 1f;
			float num6 = 0f;
			if (this.level == 0f)
			{
				this.drawGlass(Md, MdS, 0f, true);
			}
			else
			{
				float num7 = num * 0.2f;
				float num8 = 0.046875f;
				if (this.level < 0.55f)
				{
					num4 = 1f - X.ZLINE(this.level - 0.3f, 0.25f);
					num5 = 1f + X.ZSIN2(this.level, 0.2f) * 0.25f + X.ZSIN2(this.level - 0.3f, 0.25f) * 0.5f;
					num6 = X.ZSIN(this.level - 0.3f, 0.25f) * 0.7853982f;
					float num9 = 1.5f * X.ZSIN(this.level, 0.12f) - 0.5f * X.ZSIN2(this.level - 0.12f, 0.2f);
					this.drawGlass(Md, MdS, num7 * num9, false);
					this.drawBullet(Md, MdS, num7 * num9, num8 * num9, num8 * num9, 0f);
					this.drawInnerBox(MdS, 0f);
				}
				else
				{
					num4 = 0f;
					float num10 = 0.38f;
					float num11 = X.ZPOW(this.level - 0.55f, num10);
					float num12 = X.ZPOW(this.level - 0.78f, 0.22000003f);
					num7 *= 1f + num11 * 1.7f - 0.4f * num12;
					this.drawGlass(Md, MdS, num7, false);
					this.drawInnerBox(MdS, num11 * 1.02f - X.ZSIN2(this.level - 0.55f - num10, 0.04f) * 0.02f);
					float num13 = 1f - num12 + (-X.ZPOW(this.level - 0.55f - num10, 0.03f) + X.ZSIN(this.level - 0.55f - num10 - 0.03f, 0.03f)) * 0.88f;
					this.drawBullet(Md, MdS, num7, num8 * num13, num8 * num13, num11);
				}
			}
			TreasureBoxDrawer.Ma.Set(false);
			TreasureBoxDrawer.Ma.setColAllGrdation(Md.base_y + TreasureBoxDrawer.cy - num * scale, Md.base_y + TreasureBoxDrawer.cy + num * scale, C32.MulA(this.ColB.C, num3), C32.MulA(this.ColT.C, num3), GRD.BOTTOM2TOP, true, false);
			if (num4 > 0f)
			{
				this.drawCenter(Md, MdS, num5, num6, num4 * num3);
				if (MdIcon != null && this.CenterIcon != null)
				{
					MdIcon.setCurrentMatrix(TreasureBoxDrawer.PreTrs, false);
					MdIcon.Col = C32.MulA(this.ColIcon.C, num4 * num3);
					MdIcon.RotaPF(x, y, num5, num5, num6, this.CenterIcon, false, false, false, uint.MaxValue, false, 0);
					MdIcon.setCurrentMatrix(currentMatrix, false);
				}
			}
			Md.setCurrentMatrix(currentMatrix, false);
			MdS.setCurrentMatrix(currentMatrix, false);
			return this;
		}

		private void drawCenter(MeshDrawer Md, MeshDrawer MdS, float zm = 1f, float agR = 0f, float alp = 1f)
		{
			float num = this.center_w * zm - TreasureBoxDrawer.px * 2f;
			MdS.Translate(TreasureBoxDrawer.cx, TreasureBoxDrawer.cy, true).Rotate(agR, true);
			Md.Translate(TreasureBoxDrawer.cx, TreasureBoxDrawer.cy, true).Rotate(agR, true);
			MdS.Col = MdS.ColGrd.Set(this.ColLine).mulA(alp).C;
			Md.Col = Md.ColGrd.Set(this.ColT).mulA(alp).C;
			MdS.Box(0f, 0f, num + TreasureBoxDrawer.px * 2f, num, 0f, true);
			MdS.Box(0f, 0f, num, num + TreasureBoxDrawer.px * 2f, 0f, true);
			Md.Box(0f, 0f, num, num, 0f, true);
			MdS.setCurrentMatrix(TreasureBoxDrawer.PreTrs, false);
			Md.setCurrentMatrix(TreasureBoxDrawer.PreTrs, false);
		}

		private void drawGlass(MeshDrawer Md, MeshDrawer MdS, float shifty, bool draw_under = true)
		{
			float num = this.w / 2f;
			float num2 = this.center_w / 2f + TreasureBoxDrawer.px;
			for (int i = 0; i < 2; i++)
			{
				MdS.Translate(TreasureBoxDrawer.cx, TreasureBoxDrawer.cy + shifty * (float)X.MPF(i == 0), true).Scale(1f, (float)((i == 1) ? (-1) : 1), true);
				Md.Translate(TreasureBoxDrawer.cx, TreasureBoxDrawer.cy + shifty * (float)X.MPF(i == 0), true).Scale(1f, (float)((i == 1) ? (-1) : 1), true);
				Md.Tri(0, 2, 1, false).Tri(2, 3, 1, false);
				if (draw_under)
				{
					Md.Tri(2, 0, 4, false).Tri(4, 0, 5, false);
					Md.Tri(1, 3, 7, false).Tri(1, 7, 6, false);
				}
				Md.Pos(-num2, num2, null).Pos(num2, num2, null).Pos(-num, num, null)
					.Pos(num, num, null);
				if (draw_under)
				{
					Md.Pos(-num, TreasureBoxDrawer.px, null).Pos(-num2, TreasureBoxDrawer.px, null).Pos(num2, TreasureBoxDrawer.px, null)
						.Pos(num, TreasureBoxDrawer.px, null);
				}
				MdS.Line(-num, num, num, num, TreasureBoxDrawer.lt, true, 0f, 0f);
				if (draw_under)
				{
					MdS.Line(-num, TreasureBoxDrawer.px, -num, num, TreasureBoxDrawer.lt, true, 0f, 0f).Line(num, TreasureBoxDrawer.px, num, num, TreasureBoxDrawer.lt, true, 0f, 0f).Line(-num, TreasureBoxDrawer.px, -num2, TreasureBoxDrawer.px, TreasureBoxDrawer.lt, true, 0f, 0f)
						.Line(num, TreasureBoxDrawer.px, num2, TreasureBoxDrawer.px, TreasureBoxDrawer.lt, true, 0f, 0f)
						.Line(-num2, TreasureBoxDrawer.px, -num2, num2, TreasureBoxDrawer.lt, true, 0f, 0f)
						.Line(num2, TreasureBoxDrawer.px, num2, num2, TreasureBoxDrawer.lt, true, 0f, 0f);
				}
				else
				{
					MdS.Line(-num, num, -num2, num2, TreasureBoxDrawer.lt, true, 0f, 0f).Line(num, num, num2, num2, TreasureBoxDrawer.lt, true, 0f, 0f);
				}
				MdS.Line(-num2, num2, num2, num2, TreasureBoxDrawer.lt, true, 0f, 0f);
				MdS.setCurrentMatrix(TreasureBoxDrawer.PreTrs, false);
				Md.setCurrentMatrix(TreasureBoxDrawer.PreTrs, false);
			}
			if (shifty == 0f && draw_under)
			{
				MdS.Translate(TreasureBoxDrawer.cx, TreasureBoxDrawer.cy, true);
				MdS.Line(-num, -num, -num, num, TreasureBoxDrawer.lt, true, 0f, 0f);
				MdS.Line(num, -num, num, num, TreasureBoxDrawer.lt, true, 0f, 0f);
				MdS.setCurrentMatrix(TreasureBoxDrawer.PreTrs, false);
			}
		}

		private void drawBullet(MeshDrawer Md, MeshDrawer MdS, float slide_y, float shiftx, float shifty, float rot_lv)
		{
			float num = this.w / 2f;
			float num2 = this.center_w / 2f;
			float num3 = num - num2;
			float num4 = -4.712389f * rot_lv;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					MdS.Translate(TreasureBoxDrawer.cx + (float)X.MPF(j == 1) * num, TreasureBoxDrawer.cy + (float)X.MPF(i == 0) * (num + slide_y), true).Scale((float)((j == 1) ? (-1) : 1), (float)((i == 1) ? (-1) : 1), true);
					MdS.Rotate(num4, true).Translate(shiftx * (-1f + 2f * rot_lv), -shifty, true);
					Md.setCurrentMatrix(MdS.getCurrentMatrix(), false);
					Md.Tri(0, 2, 1, false).Tri(1, 2, 3, false);
					Md.Pos(0f, 0f, null).Pos(0f, -num, null).Pos(num3, -num3, null)
						.Pos(num3, -num, null);
					MdS.Line(0f, 0f, 0f, -num, TreasureBoxDrawer.lt, true, 0f, 0f).Line(0f, -num, num3, -num, TreasureBoxDrawer.lt, true, 0f, 0f).Line(num3, -num, num3, -num3, TreasureBoxDrawer.lt, true, 0f, 0f)
						.Line(num3, -num, 0f, 0f, TreasureBoxDrawer.lt, true, 0f, 0f);
					MdS.setCurrentMatrix(TreasureBoxDrawer.PreTrs, false);
					Md.setCurrentMatrix(TreasureBoxDrawer.PreTrs, false);
				}
			}
		}

		private void drawInnerBox(MeshDrawer MdS, float level)
		{
			float num = this.w / 2f;
			float num2 = this.center_w / 2f;
			float num3 = num * 0.8f;
			if (level == 0f)
			{
				MdS.Translate(TreasureBoxDrawer.cx, TreasureBoxDrawer.cy, true);
				MdS.Box(0f, 0f, num3 * 2f, num3 * 2f, 0f, true);
				MdS.setCurrentMatrix(TreasureBoxDrawer.PreTrs, false);
				return;
			}
			float num4 = num3 * 0.9f;
			float num5 = level * 1.5707964f;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					MdS.Translate(TreasureBoxDrawer.cx, TreasureBoxDrawer.cy, true).Scale((float)((j == 1) ? (-1) : 1), (float)((i == 1) ? (-1) : 1), true);
					MdS.Poly(-num4 + (num4 - num3 / 2f) * X.Cos(num5), num3 / 2f * (1f - X.ZSINV(level) * 0.24f) + num3 / 2f * 0.75f * X.Sin(num5), num3 / 2f * 1.4142135f, (-1f + level) * 0.7853982f, 4, 0f, true, 0f, 0f);
					MdS.setCurrentMatrix(TreasureBoxDrawer.PreTrs, false);
				}
			}
		}

		public float w = 0.9375f;

		public float center_w;

		public float level;

		public C32 ColT;

		public C32 ColB;

		public C32 ColLine;

		public C32 ColIcon;

		public static MdArranger Ma;

		public PxlFrame CenterIcon;

		private static float cx;

		private static float cy;

		protected static float px;

		protected static float lt;

		private static Matrix4x4 PreTrs;

		protected const float unlock_level = 0.55f;
	}
}
