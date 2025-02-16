using System;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2GradationRect : M2Rect, IM2OrderDrawItem
	{
		public M2GradationRect(int _index, M2MapLayer _Lay)
			: base("_", _index, _Lay.Mp)
		{
			this.Lay = _Lay;
			this.key = TX.ToUnixTime(DateTime.Now).ToString() + "_" + (X.xors() % 65536U).ToString();
			this.ColS = new C32(MTRX.ColBlack);
			this.ColD = new C32(MTRX.ColTrnsp);
		}

		public override DRect finePos()
		{
			if (this.Lay != null)
			{
				if (base.w <= this.Lay.Mp.CLEN / 2f)
				{
					base.w = this.Lay.Mp.CLEN / 2f;
				}
				if (base.h <= this.Lay.Mp.CLEN / 2f)
				{
					base.h = this.Lay.Mp.CLEN / 2f;
				}
			}
			return base.finePos();
		}

		public M2GradationRect SetG(M2GradationRect Src)
		{
			if (Src == null)
			{
				return this;
			}
			this.ColS.Set(Src.ColS);
			this.ColD.Set(Src.ColD);
			this.direction = Src.direction;
			this.order = Src.order;
			this.Gsl = Src.Gsl;
			return this;
		}

		public M2GradationRect setColor(Color32 C, bool to_d = false)
		{
			C32 c = (to_d ? this.ColD : this.ColS);
			C32 c2 = (to_d ? this.ColS : this.ColD);
			c.Set(C);
			if (c2.a == 0)
			{
				c2.rgb = c.rgb;
			}
			this.drawn_s_a = this.ColS.a;
			this.drawn_d_a = this.ColD.a;
			return this;
		}

		public void Order(int _order)
		{
			this.order = (M2GradationRect.GRDORDER)_order;
			if (this.chip_layer_order)
			{
				this.Lay.GRD.use_chip_layer_order = true;
			}
		}

		public int get_order()
		{
			return (int)this.order;
		}

		public void drawTo(float alpha, MeshDrawer Md, ref int redrawing, bool is_chip_layer = false)
		{
			if (base.isEmpty())
			{
				return;
			}
			Color32 c = this.ColS.C;
			Color32 c2 = this.ColD.C;
			if (is_chip_layer)
			{
				c.r /= 2;
				c.g /= 2;
				c.b /= 2;
				c2.r /= 2;
				c2.g /= 2;
				c2.b /= 2;
			}
			Md.ColGrd.Set(c).mulA(alpha);
			Md.Col = Md.ColGrd.C;
			if (this.direction != 8)
			{
				if (this.direction == 13)
				{
					if (this.Gsl != null)
					{
						float num = this.Mp.pixel2meshx(this.x + this.width / 2f) * 0.015625f;
						float num2 = this.Mp.pixel2meshy(base.y + this.height / 2f) * 0.015625f;
						Md.Translate(num, num2, false);
						float num3 = X.Mx(base.w, base.h) * 0.5f * 0.015625f;
						if (is_chip_layer)
						{
							this.Lay.M2D.IMGS.Atlas.initForRectWhite(Md, num - num3, num2 - num3, num3 * 2f, num3 * 2f, true, false);
						}
						if (redrawing == -1)
						{
							this.Gsl.drawTo(Md, this.width, this.height, c, c2, false);
							Md.Identity();
							return;
						}
					}
				}
				else
				{
					M2GradationRect.GRD_DIR grd_DIR = (M2GradationRect.GRD_DIR)this.direction;
					bool flag = grd_DIR == M2GradationRect.GRD_DIR.BL || grd_DIR == M2GradationRect.GRD_DIR.TR || grd_DIR == M2GradationRect.GRD_DIR.SBL || grd_DIR == M2GradationRect.GRD_DIR.STR;
					int num4;
					Color32[] array;
					if (redrawing == -1)
					{
						num4 = Md.getVertexMax();
						Matrix4x4 currentMatrix = Md.getCurrentMatrix();
						float num5 = base.w * 0.5f * 0.015625f;
						float num6 = base.h * 0.5f * 0.015625f;
						if (is_chip_layer)
						{
							this.Lay.M2D.IMGS.Atlas.initForRectWhite(Md, -num5, -num6, num5 * 2f, num6 * 2f, true, true);
						}
						Md.TranslateP(this.Mp.pixel2meshx(this.x + this.width / 2f), this.Mp.pixel2meshy(base.y + this.height / 2f), true);
						if (flag)
						{
							Md.Tri(0, 1, 3, false).Tri(3, 1, 2, false);
						}
						else
						{
							Md.TriRectBL(0);
						}
						Md.Pos(-num5, -num6, null).Pos(-num5, num6, null).Pos(num5, num6, null)
							.Pos(num5, -num6, null);
						Md.setCurrentMatrix(currentMatrix, false);
						array = Md.getColorArray();
					}
					else
					{
						num4 = redrawing;
						redrawing += 4;
						array = Md.getColorArray();
						for (int i = num4; i < redrawing; i++)
						{
							array[i] = Md.Col;
						}
					}
					if (9 <= this.direction && this.direction <= 12)
					{
						int num7;
						switch (this.direction)
						{
						case 9:
							num7 = 1;
							break;
						case 10:
							num7 = 2;
							break;
						case 11:
							num7 = 0;
							break;
						default:
							num7 = 3;
							break;
						}
						int num8 = num7;
						for (int j = 0; j < 4; j++)
						{
							if (j != num8)
							{
								array[num4 + j] = C32.MulA(c2, alpha);
							}
						}
						return;
					}
					if (4 <= this.direction && this.direction <= 7)
					{
						C32 c3 = MTRX.colb.Set(c).blend(c2, 0.5f).mulA(alpha);
						switch (this.direction)
						{
						case 4:
							array[num4 + 2] = (array[num4] = c3.C);
							array[num4 + 1] = C32.MulA(c2, alpha);
							return;
						case 5:
							array[num4 + 1] = (array[num4 + 3] = c3.C);
							array[num4 + 2] = C32.MulA(c2, alpha);
							return;
						case 6:
							array[num4 + 1] = (array[num4 + 3] = c3.C);
							array[num4] = C32.MulA(c2, alpha);
							return;
						case 7:
							array[num4 + 2] = (array[num4] = c3.C);
							array[num4 + 3] = C32.MulA(c2, alpha);
							return;
						default:
							return;
						}
					}
					else
					{
						switch (this.direction)
						{
						case 0:
							array[num4] = (array[num4 + 1] = C32.MulA(c2, alpha));
							return;
						case 1:
							array[num4 + 1] = (array[num4 + 2] = C32.MulA(c2, alpha));
							return;
						case 2:
							array[num4 + 2] = (array[num4 + 3] = C32.MulA(c2, alpha));
							return;
						case 3:
							array[num4 + 3] = (array[num4] = C32.MulA(c2, alpha));
							break;
						default:
							return;
						}
					}
				}
				return;
			}
			Md.ColGrd.Set(c2).mulA(alpha).multiply(is_chip_layer ? 0.5f : 1f, false);
			float num9 = X.Mx(base.w, base.h) / 2f;
			int num10 = X.MMX(6, X.IntC(num9 * 2f * 3.1415927f / 5f), 32);
			if (redrawing == -1)
			{
				if (base.w > base.h)
				{
					Md.Scale(1f, base.h / base.w, false);
				}
				else
				{
					Md.Scale(base.w / base.h, 1f, false);
				}
				float num11 = this.Mp.pixel2meshx(this.x + this.width / 2f) * 0.015625f;
				float num12 = this.Mp.pixel2meshy(base.y + this.height / 2f) * 0.015625f;
				num9 *= 0.015625f;
				if (is_chip_layer)
				{
					this.Lay.M2D.IMGS.Atlas.initForRectWhite(Md, num11 - num9, num12 - num9, num9 * 2f, num9 * 2f, true, false);
				}
				Md.Translate(num11, num12, false);
				Md.Poly(0f, 0f, num9, 0f, num10, 0f, true, 1f, 0f);
				Md.Identity();
				return;
			}
			int num13 = redrawing;
			Color32[] colorArray = Md.getColorArray();
			colorArray[num13] = Md.Col;
			int num14 = num13 + num10;
			for (int k = num13 + 1; k <= num14; k++)
			{
				colorArray[k] = Md.ColGrd.C;
			}
			redrawing += num10 + 1;
		}

		public int layer2update_flag
		{
			get
			{
				switch (this.order)
				{
				case M2GradationRect.GRDORDER.SKY:
					return 16;
				case M2GradationRect.GRDORDER.BACK:
					return 32;
				case M2GradationRect.GRDORDER.GROUND:
					return 64;
				case M2GradationRect.GRDORDER.TOP:
					return 128;
				default:
					return 16;
				}
			}
		}

		public bool chip_layer_order
		{
			get
			{
				return this.order == M2GradationRect.GRDORDER.CHIP_B || this.order == M2GradationRect.GRDORDER.CHIP_G || this.order == M2GradationRect.GRDORDER.CHIP_T;
			}
		}

		public static M2GradationRect readBytesContentGrd(ByteArray Ba, M2MapLayer Lay, bool key_replace = false, int shift_drawx = 0, int shift_drawy = 0)
		{
			bool flag = Lay == null;
			string text = Ba.readPascalString("utf-8", flag);
			int num = (int)Ba.readShort() + shift_drawx;
			int num2 = (int)Ba.readShort() + shift_drawy;
			int num3 = (int)Ba.readShort();
			int num4 = (int)Ba.readShort();
			int num5 = Ba.readByte();
			int num6 = Ba.readByte();
			uint num7 = Ba.readUInt();
			uint num8 = Ba.readUInt();
			M2GrdSlicer m2GrdSlicer = null;
			if (num6 == 13)
			{
				m2GrdSlicer = M2GrdSlicer.readBytesContentGrdSlicer(Ba, null);
			}
			if (Lay != null)
			{
				M2GradationRect m2GradationRect = new M2GradationRect(0, Lay);
				if (key_replace)
				{
					text = Lay.GRD.fineIndividualName(text, null);
				}
				m2GradationRect.Set((float)num, (float)num2, (float)num3, (float)num4);
				m2GradationRect.index = Lay.GRD.Length;
				m2GradationRect.order = ((Lay.Mp.get_binary_version() < 2) ? M2GradationRect.GRDORDER.SKY : M2GradationRect.GRDORDER.CHIP_B) + num5;
				m2GradationRect.direction = num6;
				m2GradationRect.ColS.rgba = num7;
				m2GradationRect.ColD.rgba = num8;
				m2GradationRect.drawn_s_a = m2GradationRect.ColS.a;
				m2GradationRect.drawn_d_a = m2GradationRect.ColD.a;
				m2GradationRect.Gsl = m2GrdSlicer;
				Lay.GRD.Add(m2GradationRect);
				if (m2GradationRect.chip_layer_order)
				{
					Lay.GRD.use_chip_layer_order = true;
				}
				return m2GradationRect;
			}
			return null;
		}

		public void writeSaveBytesTo(ByteArray Ba)
		{
			Ba.writeByte(85);
			Ba.writePascalString(this.key, "utf-8");
			Ba.writeShort((short)this.x);
			Ba.writeShort((short)base.y);
			Ba.writeShort((short)base.w);
			Ba.writeShort((short)base.h);
			Ba.writeByte((int)this.order);
			Ba.writeByte(this.direction);
			Ba.writeUInt(this.ColS.rgba);
			Ba.writeUInt(this.ColD.rgba);
			if (this.direction == 13)
			{
				if (this.Gsl == null)
				{
					Ba.writeByte(0);
					return;
				}
				this.Gsl.writeBytesTo(Ba);
			}
		}

		public static string[] Agrd_keys = new string[]
		{
			"arrow_l", "arrow_t", "arrow_r", "arrow_b", "arrow_lt", "arrow_tr", "arrow_bl", "arrow_rb", "gradation_circle", "arrow_slt",
			"arrow_str", "arrow_sbl", "arrow_srb", "gradation_slicer"
		};

		public static string[] Agrd_descs = new string[]
		{
			"左へ", "上へ", "右へ", "下へ", "左上へ", "右上へ", "左下へ", "右下へ", "円形", "左上(S)",
			"右上(S)", "左下(S)", "右下(S)", "カスタム..."
		};

		public M2MapLayer Lay;

		public int direction = 3;

		public C32 ColS;

		public C32 ColD;

		public M2GradationRect.GRDORDER order = M2GradationRect.GRDORDER.SKY;

		public byte drawn_s_a;

		public byte drawn_d_a;

		public M2GrdSlicer Gsl;

		public enum GRDORDER
		{
			CHIP_B,
			CHIP_G,
			CHIP_T,
			SKY,
			BACK,
			GROUND,
			TOP,
			_MAX
		}

		public enum GRD_DIR
		{
			L,
			T,
			R,
			B,
			LT,
			TR,
			BL,
			RB,
			CIRCLE,
			SLT,
			STR,
			SBL,
			SRB,
			SLICER,
			_ALL
		}
	}
}
