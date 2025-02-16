using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class EfSpellRegisteration
	{
		public EfSpellRegisteration(MGKIND mgkind)
		{
			this.FD_fnDrawEf = new FnEffectRun(this.fnDrawEf);
			EfSpellRegisteration.Instance = this;
			this.kind = mgkind;
			this.Mk = MagicSelector.getKindData(mgkind);
			this.ADi = new List<EfSpellRegisteration.SpellRegisterItem>(4);
			if (EfSpellRegisteration.SqMarker == null)
			{
				EfSpellRegisteration.SqMarker = MTRX.PxlIcon.getPoseByName("nel_regist_marker").getSequence(0);
				EfSpellRegisteration.Chr = MTR.ChrNelS;
				EfSpellRegisteration.PFPoint3 = EfSpellRegisteration.SqMarker.getFrame(4);
			}
			this.ASq = new List<EfSpellRegisteration.SquareItem>(4);
			this.ARaisingChar = new List<EfSpellRegisteration.RaisingCharItem>(4);
			int capacity = this.ASq.Capacity;
			for (int i = 0; i < capacity; i++)
			{
				this.ASq.Add(new EfSpellRegisteration.SquareItem());
			}
		}

		public EfSpellRegisteration(Map2d _Mp, MGKIND mgkind, float x, float y, int delay = 0)
			: this(mgkind)
		{
			this.Mp = _Mp;
			this.M2D = this.Mp.M2D as NelM2DBase;
			this.efx = x;
			this.efy = y;
			this.Pr = this.M2D.getPrNoel();
			this.Ef = this.Mp.getEffectTop().setEffectWithSpecificFn("spell_registeration", x, y, 0f, 0, delay, this.FD_fnDrawEf);
			this.time0 = Time.time + (float)delay / 60f;
		}

		public EfSpellRegisteration(M2LabelPoint Lp, MGKIND mgkind, int delay = 0)
			: this(Lp.Mp, mgkind, Lp.mapcx, Lp.mapcy, delay)
		{
		}

		public bool fnDrawEf(EffectItem Ef)
		{
			if (this.Ef == null)
			{
				return false;
			}
			if (Time.time < this.time0)
			{
				return true;
			}
			Ef.x = this.efx;
			Ef.y = this.efy;
			return this.run((Time.time - this.time0) * 68f - this.t, Ef.GetMesh("spell_registeration", uint.MaxValue, BLEND.ADD, false), Ef.GetMeshImg("spell_registeration_c", MTRX.MIicon, BLEND.ADD, false));
		}

		public bool run(float fcnt, MeshDrawer Md = null, MeshDrawer MdC = null)
		{
			this.t += fcnt;
			this.t_item_margin -= fcnt;
			if (this.t_item_margin < 0f)
			{
				if (this.createDI() == null)
				{
					this.t_item_margin += (float)(3 + X.xors(5));
				}
				else
				{
					this.t_item_margin += (float)(20 + X.xors(10));
				}
			}
			this.t_raising_char_margin -= fcnt;
			if (this.t_raising_char_margin < 0f)
			{
				float num = (float)((this.circulation_level > 0f) ? 4 : 1);
				this.t_raising_char_margin += (float)(12 + X.xors(8)) / num;
				this.ARaisingChar.Add(new EfSpellRegisteration.RaisingCharItem
				{
					x = X.NIXP(-1f, 1f) * 1.5f * IN.wh,
					y = X.NIXP(-1.2f, 0.7f) * IN.hh,
					z = 0.5f * (float)X.xors(5),
					a = 0.25f + X.XORSP() * 0.75f,
					spd = num,
					t0 = (int)this.t
				});
			}
			int count = this.ASq.Count;
			for (int i = 0; i < count; i++)
			{
				this.ASq[i].run(fcnt);
			}
			if (Md != null)
			{
				this.drawToMesh(Md, MdC);
			}
			return true;
		}

		public void remakeCirculation()
		{
			this.Ccr.draw_ratio = this.circulation_level;
			this.Ccr.min_sp_radius = X.NI(40, 220, this.circulation_level);
			this.Ccr.max_sp_radius = X.NI(50, 380, this.circulation_level);
			this.Ccr.min_ps_length = X.NI(40, 100, this.circulation_level);
			this.Ccr.max_ps_length = X.NI(60, 520, this.circulation_level);
			this.Ccr.remake(X.IntR(2f + this.circulation_level * 2f) + X.xors(3));
		}

		public bool drawToMesh(MeshDrawer _Md, MeshDrawer _MdC)
		{
			EfSpellRegisteration.Md = _Md;
			EfSpellRegisteration.MdC = _MdC;
			if (this.circulation_level > 0f)
			{
				this.Ccr.drawTo(EfSpellRegisteration.Md);
			}
			int i;
			for (i = this.ARaisingChar.Count - 1; i >= 0; i--)
			{
				if (!this.drawRaisingChar(this.ARaisingChar[i]))
				{
					this.ARaisingChar.RemoveAt(i);
				}
			}
			this.ColLvl(1f);
			int num = this.ASq.Count;
			for (i = 0; i < num; i++)
			{
				this.drawSq(this.ASq[i], i);
			}
			num = this.ADi.Count;
			i = 0;
			while (i < num)
			{
				EfSpellRegisteration.SpellRegisterItem spellRegisterItem = this.ADi[i];
				bool flag = spellRegisterItem.isActive();
				if (!this.drawToMeshDi(spellRegisterItem))
				{
					if (flag)
					{
						this.active_ri_bits &= ~(1 << (int)spellRegisterItem.aim);
					}
					this.ADi.RemoveAt(i);
					num--;
				}
				else
				{
					if (flag && !spellRegisterItem.isActive())
					{
						this.active_ri_bits &= ~(1 << (int)spellRegisterItem.aim);
					}
					i++;
				}
			}
			return true;
		}

		private void drawSq(EfSpellRegisteration.SquareItem Sq, int index)
		{
			float num = (float)(30 + index * 8) / 1.4142135f;
			float num2 = X.ZLINE(this.t - (float)(index * 23), 40f);
			if (num2 <= 0f)
			{
				return;
			}
			Vector2 vector = Vector2.zero;
			Vector2 vector2 = Vector2.zero;
			AIM aim = AIM.LT;
			for (int i = 0; i < 4; i++)
			{
				AIM aim2 = CAim.get_clockwise2(aim, false);
				for (int j = 0; j < 2; j++)
				{
					if (i >= 1 && j == 0 && num2 >= 1f)
					{
						Vector2 vector3;
						vector = (vector3 = vector2);
					}
					else
					{
						Vector2 vector3;
						if (j == 0)
						{
							vector3 = new Vector2(num * (float)CAim._XD(aim, 1), num * (float)CAim._YD(aim, 1));
						}
						else
						{
							Vector2 vector4 = new Vector2(num * (float)CAim._XD(aim2, 1), num * (float)CAim._YD(aim2, 1));
							if (num2 >= 1f)
							{
								vector3 = vector4;
							}
							else
							{
								vector3 = new Vector2(num * (float)CAim._XD(aim, 1), num * (float)CAim._YD(aim, 1));
								vector3.x = X.NI(vector3.x, vector4.x, num2);
								vector3.y = X.NI(vector3.y, vector4.y, num2);
							}
						}
						vector3 = X.ROTV2e(vector3, Sq.zagR);
						vector3.x *= X.Cos(Sq.xagR);
						vector3 = X.ROTV2e(vector3, Sq.yagR);
						if (j == 0)
						{
							vector = vector3;
						}
						else
						{
							vector2 = vector3;
						}
					}
				}
				EfSpellRegisteration.Md.Line(vector.x, vector.y, vector2.x, vector2.y, 1f, false, 0f, 0f);
				aim = aim2;
			}
		}

		private EfSpellRegisteration.SpellRegisterItem createDI()
		{
			AIM aim = AIM.L;
			int num = 4 - (((this.active_ri_bits & 16) != 0) ? 1 : 0) - (((this.active_ri_bits & 32) != 0) ? 1 : 0) - (((this.active_ri_bits & 64) != 0) ? 1 : 0) - (((this.active_ri_bits & 128) != 0) ? 1 : 0);
			if (num <= 1)
			{
				return null;
			}
			int num2 = X.xors(num);
			for (int i = 0; i < 4; i++)
			{
				if ((this.active_ri_bits & (16 << i)) == 0 && num2-- <= 0)
				{
					aim = AIM.LT + (uint)i;
					break;
				}
			}
			EfSpellRegisteration.SpellRegisterItem spellRegisterItem = new EfSpellRegisteration.SpellRegisterItem();
			this.ADi.Add(spellRegisterItem);
			spellRegisterItem.t0 = this.t;
			spellRegisterItem.aim = aim;
			this.active_ri_bits |= 1 << (int)aim;
			float num3 = (0.12f + X.XORSP() * 0.27f) * 3.1415927f;
			float num4 = 30f + 20f * X.XORSP();
			spellRegisterItem.sx = num4 * X.Cos(num3) * (float)CAim._XD(spellRegisterItem.aim, 1);
			spellRegisterItem.sy = num4 * X.Sin(num3) * (float)CAim._YD(spellRegisterItem.aim, 1);
			spellRegisterItem.l1 = (int)X.NIXP(14f, 28f);
			spellRegisterItem.l2 = (int)X.NIXP(20f, 34f);
			spellRegisterItem.ran0 = (uint)X.xors(1048576);
			bool flag = false;
			if (X.XORSP() < 0.03f)
			{
				flag = true;
			}
			else
			{
				switch (this.di_content_level)
				{
				case 0:
					spellRegisterItem.type = EfSpellRegisteration.DTYPE.STRING;
					spellRegisterItem.header_text = TX.GetA("Magic_Register_1", this.kind.ToString());
					spellRegisterItem.text = TX.Get("Magic_Register_1_1", "");
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_header_text, true);
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_text, false);
					break;
				case 1:
					spellRegisterItem.type = EfSpellRegisteration.DTYPE.PERSONAL_INFO;
					spellRegisterItem.header_text = TX.Get("Magic_Register_2", "");
					spellRegisterItem.text = TX.Get("Magic_Register_2_1", "");
					spellRegisterItem.text2 = TX.Get("Magic_Register_2_2", "");
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_text, true);
					spellRegisterItem.sw += 32f;
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_header_text, true);
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_text2, false);
					break;
				case 2:
					spellRegisterItem.type = EfSpellRegisteration.DTYPE.NOTICE;
					spellRegisterItem.header_text = TX.Get("Magic_Register_3", "");
					spellRegisterItem.text = TX.Get("Magic_Register_3_1", "");
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_header_text, true);
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_text, false);
					break;
				case 3:
					spellRegisterItem.type = EfSpellRegisteration.DTYPE.DECODE;
					spellRegisterItem.header_text = TX.Get("Magic_Register_4", "");
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_header_text, true);
					this.decodeMeterWH(spellRegisterItem, false);
					break;
				case 4:
					spellRegisterItem.type = EfSpellRegisteration.DTYPE.CONFORMITY;
					spellRegisterItem.header_text = TX.Get("Magic_Register_5", "");
					spellRegisterItem.text = TX.GetA("Magic_Register_5_1", NEL.nel_num((this.Pr != null) ? ((int)this.Pr.get_maxmp()) : 150, false));
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_header_text, true);
					spellRegisterItem.sh += 68f;
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_text, false);
					break;
				case 5:
					spellRegisterItem.type = EfSpellRegisteration.DTYPE.AUTHENTICATION;
					spellRegisterItem.header_text = TX.Get("Magic_Register_6", "");
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_header_text, true);
					spellRegisterItem.sh += 40f;
					break;
				case 6:
					spellRegisterItem.type = EfSpellRegisteration.DTYPE.CONSISTENCY;
					spellRegisterItem.header_text = TX.Get("Magic_Register_7", "");
					spellRegisterItem.text = TX.GetA("Magic_Register_7_1", this.kind.ToString(), NEL.nel_num(this.Mk.reduce_mp, false), NEL.nel_num(this.Mk.casttime, false), NEL.nel_num(X.IntR(this.Mk.mp_crystalize), false) + "%", NEL.nel_num(X.IntR(this.Mk.crystalize_neutral_ratio * 100f), false) + "%");
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_header_text, true);
					spellRegisterItem.sh += 33f;
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_text, false);
					break;
				case 7:
					spellRegisterItem.type = EfSpellRegisteration.DTYPE.SOCKETINFO;
					spellRegisterItem.header_text = TX.Get("Magic_Register_8", "");
					spellRegisterItem.text = TX.Get("Magic_Register_8_1", "");
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_header_text, true);
					spellRegisterItem.sh += 38f;
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_text, false);
					break;
				default:
					flag = true;
					break;
				}
				this.di_content_level++;
			}
			if (flag)
			{
				if (X.XORSP() < 0.4f)
				{
					spellRegisterItem.type = EfSpellRegisteration.DTYPE.HPZ_SIMU;
					int num5 = 3 + X.xors(6);
					spellRegisterItem.Hpz = new HpzSimulator(num5, (22 + X.xors(12)) / num5);
					spellRegisterItem.sw = (float)(spellRegisterItem.Hpz.hole_clms * 18);
					spellRegisterItem.sh = (float)(spellRegisterItem.Hpz.hole_rows * 18);
				}
				else
				{
					spellRegisterItem.type = EfSpellRegisteration.DTYPE.STRING;
					spellRegisterItem.text = TX.Get("Magic_Register_t" + X.xors(6).ToString(), "");
					this.checkSentenceWH(spellRegisterItem, spellRegisterItem.Stb_text, false);
				}
			}
			return spellRegisterItem;
		}

		private void ColLvl(float lvl = 1f)
		{
			EfSpellRegisteration.Md.Col = (EfSpellRegisteration.MdC.Col = EfSpellRegisteration.Md.ColGrd.Set(EfSpellRegisteration.Md.Col).setA1(lvl * EfSpellRegisteration.color_level).C);
		}

		private bool drawToMeshDi(EfSpellRegisteration.SpellRegisterItem Di)
		{
			float num;
			if (Di.t0_deactivate >= 0f)
			{
				num = X.ZLINE(this.t - Di.t0_deactivate, 40f);
				if (num >= 1f)
				{
					return false;
				}
				EfSpellRegisteration.color_level = 1f - num;
			}
			else
			{
				EfSpellRegisteration.color_level = 1f;
			}
			this.ColLvl(1f);
			float num2 = this.t - Di.t0;
			uint ran = Di.ran0;
			if (num2 < 14f)
			{
				EfSpellRegisteration.MdC.RotaPF(Di.sx, Di.sy, 1f, 1f, 0f, EfSpellRegisteration.SqMarker.getFrame((int)(num2 / 2f)), false, false, false, uint.MaxValue, false, 0);
			}
			else
			{
				EfSpellRegisteration.MdC.RotaPF(Di.sx, Di.sy, 1f, 1f, 0f, EfSpellRegisteration.SqMarker.getFrame(7 + X.ANM((int)(num2 - 14f), 5, 2f)), false, false, false, uint.MaxValue, false, 0);
			}
			if (num2 <= 14f)
			{
				return true;
			}
			num2 -= 14f;
			float num3 = (float)X.IntC((float)(Di.l1 / 8));
			num = X.ZLINE(num2, num3);
			float num4 = 6f + (float)Di.l1 * num;
			EfSpellRegisteration.Md.Line(Di.sx + 6f * Di._xd, Di.sy + 6f * Di._yd, Di.sx + num4 * Di._xd, Di.sy + num4 * Di._yd, 1f, false, 0f, 0f);
			if (num2 < num3)
			{
				return true;
			}
			num2 -= num3;
			num3 = (float)X.IntC((float)(Di.l2 / 8));
			float num5 = X.ZLINE(num2, num3 - 2f) * (float)Di.l2;
			EfSpellRegisteration.Md.Line(Di.sx + num4 * Di._xd, Di.sy + num4 * Di._yd, Di.sx + (num4 + num5) * Di._xd, Di.sy + num4 * Di._yd, 1f, false, 0f, 0f);
			if (num2 < num3)
			{
				return true;
			}
			num2 -= num3;
			bool flag = false;
			float num6 = Di.sx + (num4 + num5) * Di._xd + (10f + ((Di._xd < 0f) ? Di.sw : 0f)) * Di._xd;
			float num7 = Di.sy + num4 * Di._yd + Di.sh / 2f;
			float num8 = num7;
			if (Di.Stb_header_text != null)
			{
				float num9 = (float)(Di.Stb_header_text.Length + 2) * 0.5f;
				flag = this.drawHeader(Di, num6, ref num7, num2) || flag;
				num2 -= num9;
			}
			switch (Di.type)
			{
			case EfSpellRegisteration.DTYPE.STRING:
				flag = this.drawSentence(num6, ref num7, num2, Di.Stb_text) || flag;
				break;
			case EfSpellRegisteration.DTYPE.PERSONAL_INFO:
				EfSpellRegisteration.MdC.initForImg(MTR.ImgRegisterNoel, 0);
				EfSpellRegisteration.MdC.DrawDiffuseImage(num6 + 16f, num7 - EfSpellRegisteration.Chr.getLineDrawHeight(3) / 2f, 1f, X.ZLINE(num2, 40f), null, AIM.LT);
				flag = this.drawSentence(num6 + 32f, ref num7, num2, Di.Stb_text) || flag;
				num2 -= (float)(Di.Stb_text.Length + 3) * 0.5f;
				flag = this.drawSentence(num6, ref num7, num2, Di.Stb_text2) || flag;
				break;
			case EfSpellRegisteration.DTYPE.NOTICE:
			{
				float num10 = 10f;
				float num11 = X.ZLINE(num2 + (float)(Di.Stb_header_text.Length + 2) * 0.5f, 20f);
				float num12 = X.ANMPT(40, 1f);
				float num13 = num11 * num10;
				EfSpellRegisteration.Md.BoxBL(num6, num8 + (8f + num10) - num13 - 2f, Di.sw * num11, num13, 1f, false);
				EfSpellRegisteration.Md.StripedRectBL(num6, num8 + (8f + num10) - num13 - 2f, Di.sw * num11, num13, num12, 0.5f, 8f, false);
				EfSpellRegisteration.Md.BoxBL(num6 + Di.sw - Di.sw * num11, num8 + 2f - (Di.sh + 8f + num10), Di.sw * num11, num13, 1f, false);
				EfSpellRegisteration.Md.StripedRectBL(num6 + Di.sw - Di.sw * num11, num8 + 2f - (Di.sh + 8f + num10), Di.sw * num11, num13, X.frac(1f - num12), 0.5f, 8f, false);
				flag = this.drawSentence(num6, ref num7, num2, Di.Stb_text) || flag;
				break;
			}
			case EfSpellRegisteration.DTYPE.DECODE:
				flag = this.drawDecodeMeter(num6, ref num7, num2, 40f) || flag;
				break;
			case EfSpellRegisteration.DTYPE.CONFORMITY:
				this.drawCircleGraph(num6 + Di.sw / 2f, ref num7, num2);
				flag = this.drawSentence(num6, ref num7, num2 - 20f, Di.Stb_text) || flag;
				break;
			case EfSpellRegisteration.DTYPE.AUTHENTICATION:
				flag = this.drawAuth(num6, ref num7, Di.sw, num2 - 5f, 90f) || flag;
				break;
			case EfSpellRegisteration.DTYPE.CONSISTENCY:
				flag = this.drawConsistency(num6, ref num7, Di.sw, num2 - 5f, 70f) || flag;
				flag = this.drawSentence(num6, ref num7, num2 - 5f, Di.Stb_text) || flag;
				break;
			case EfSpellRegisteration.DTYPE.SOCKETINFO:
				flag = this.drawSocketInfo(num6 + Di.sw / 2f, ref num7, num2 - 5f, 44f) || flag;
				flag = this.drawSentence(num6, ref num7, num2 - 35f, Di.Stb_text) || flag;
				break;
			case EfSpellRegisteration.DTYPE.HPZ_SIMU:
				if (Di.header_w <= 0f)
				{
					Di.header_w = 10f;
					Di.Hpz.plotPoints(0);
				}
				else
				{
					Di.header_w -= 1f;
				}
				flag = num2 < 100f;
				Di.Hpz.drawBL(EfSpellRegisteration.Md, num6, num7 - Di.sh, Di.sw, Di.sh, 12f * X.ZLINE(num2, 15f));
				break;
			}
			if (!flag && Di.isActive())
			{
				Di.deactivate(this.t);
			}
			return true;
		}

		private void checkSentenceWH(EfSpellRegisteration.SpellRegisterItem Di, STB Stb, bool add_margin_h = false)
		{
			if (Stb == null)
			{
				Di.sw = 0f;
				Di.sh = 16f;
				return;
			}
			Di.sw = X.Mx(Di.sw, EfSpellRegisteration.Chr.DrawStringTo(null, Stb.ToUpper(), 0f, 0f, ALIGN.LEFT, ALIGNY.TOP, false, 0f, 0f, null));
			int num = TX.countLine(Stb);
			Di.sh += EfSpellRegisteration.Chr.getLineDrawHeight(num) + (float)(add_margin_h ? 8 : 0);
		}

		private bool drawHeader(EfSpellRegisteration.SpellRegisterItem Di, float x, ref float y, float t)
		{
			if (Di.header_w == 0f)
			{
				Di.header_w = EfSpellRegisteration.Chr.DrawStringTo(null, Di.Stb_header_text.ToUpper(), 0f, 0f, ALIGN.LEFT, ALIGNY.TOP, false, 0f, 0f, null);
			}
			return this.drawSentence(x + Di.sw / 2f - Di.header_w / 2f, ref y, t, Di.Stb_header_text);
		}

		private bool drawSentence(float x, ref float y, float t, STB Stb)
		{
			if (t < 0f)
			{
				return true;
			}
			EfSpellRegisteration.ABuf.Clear();
			List<int> list = Stb.splitIndex("\n", EfSpellRegisteration.ABuf);
			int count = list.Count;
			float num = y - EfSpellRegisteration.Chr.getLineDrawHeight(count) - 8f;
			y -= EfSpellRegisteration.Chr.ch;
			int num2 = X.IntC(t / 0.5f);
			STB stb = TX.PopBld(null, 0);
			bool flag = false;
			int num3 = 0;
			while (Stb.splitByIndex(num3, list, "\n", stb))
			{
				int num4 = stb.Length + 1;
				if (num2 <= num4)
				{
					if (num2 < num4)
					{
						stb.Clip(0, num2);
					}
					EfSpellRegisteration.Chr.DrawStringTo(EfSpellRegisteration.MdC, stb.Add(","), x, y, ALIGN.LEFT, ALIGNY.BOTTOM, false, 0f, 0f, null);
					y = num;
					flag = true;
					break;
				}
				float num5 = EfSpellRegisteration.Chr.DrawStringTo(EfSpellRegisteration.MdC, stb, x, y, ALIGN.LEFT, ALIGNY.BOTTOM, false, 0f, 0f, null);
				num2 -= num4;
				if (num3 < count - 1)
				{
					y -= EfSpellRegisteration.Chr.ch + (float)EfSpellRegisteration.Chr.marginh;
				}
				else
				{
					x += num5 + (float)EfSpellRegisteration.Chr.marginw;
				}
				num3++;
			}
			if (!flag && num2 < 15)
			{
				stb.Set(",");
				if (num2 / 3 % 2 == 0)
				{
					EfSpellRegisteration.Chr.DrawStringTo(EfSpellRegisteration.MdC, stb, x, y, ALIGN.LEFT, ALIGNY.BOTTOM, false, 0f, 0f, null);
				}
				y = num;
				flag = true;
			}
			if (flag)
			{
				return true;
			}
			TX.ReleaseBld(stb);
			y = num;
			return num2 < 15;
		}

		private float title_height
		{
			get
			{
				return EfSpellRegisteration.Chr.ch + 8f;
			}
		}

		private void decodeMeterWH(EfSpellRegisteration.SpellRegisterItem Di, bool add_margin_h = false)
		{
			STB stb = TX.PopBld("100%", 0);
			Di.sw = X.Mx(Di.sw, 103f + EfSpellRegisteration.Chr.DrawStringTo(null, stb, 0f, 0f, ALIGN.LEFT, ALIGNY.TOP, false, 0f, 0f, null));
			TX.ReleaseBld(stb);
			Di.sh += 10f + (float)(add_margin_h ? 8 : 0);
		}

		private bool drawDecodeMeter(float x, ref float y, float t, float maxt)
		{
			t -= 20f;
			y -= 10f;
			if (t < 0f)
			{
				y -= 8f;
				return true;
			}
			this.ColLvl(1f);
			float num = X.ZLINE(t - 10f, maxt);
			int num2 = X.IntC(num * 94f);
			STB stb = TX.PopBld(null, 0);
			if (num2 < 94)
			{
				this.ColLvl(0.25f);
				EfSpellRegisteration.Md.RectBL(x + 3f + (float)num2, y + 1f, (float)(94 - num2), 1f, false);
				this.ColLvl(0.5f);
				EfSpellRegisteration.Md.RectBL(x + 3f, y + 1f, (float)num2, 7f, false);
				this.ColLvl(1f);
				EfSpellRegisteration.Md.StripedRectBL(x + 3f, y + 1f, (float)num2, 7f, 1f - X.ANMPT(20, 1f), 0.5f, 8f, false);
				stb += NEL.nel_num(X.IntC(100f * num), false);
				stb += "%";
			}
			else
			{
				bool flag = X.ANM((int)(t - maxt), 2, 6f) == 1;
				this.ColLvl(flag ? 1f : 0.85f);
				EfSpellRegisteration.Md.RectBL(x + 3f, y + 1f, (float)num2, 7f, false);
				if (flag)
				{
					NEL.nel_num(stb, 100, false);
					stb += "% ";
				}
			}
			EfSpellRegisteration.Md.RectBL(x, y, 3f, 10f, false);
			EfSpellRegisteration.Md.RectBL(x + 100f - 3f, y, 3f, 10f, false);
			if (stb.Length > 0)
			{
				EfSpellRegisteration.Chr.DrawStringTo(EfSpellRegisteration.MdC, stb, x + 100f + 3f + EfSpellRegisteration.Chr.getFixedDrawWidth(4), y + 3f, ALIGN.RIGHT, ALIGNY.BOTTOM, false, 0f, 0f, null);
			}
			TX.ReleaseBld(stb);
			y -= 8f;
			return t <= maxt + 40f;
		}

		public void drawCircleGraph(float x, ref float y, float t)
		{
			if (t <= 0f)
			{
				y -= 68f;
				return;
			}
			float num = 30f * X.ZSIN(t, 18f);
			this.ColLvl(0.2f);
			y -= 30f;
			float[] array = new float[5];
			float num2 = this.time0 * 60f;
			for (int i = 1; i <= 5; i++)
			{
				uint ran = X.GETRAN2(23 + i * 17 + (int)num2, i + (int)num2 % 7);
				EfSpellRegisteration.Md.Poly(x, y, (float)i / 5f * num, 1.5707964f, 5, 0.5f, false, 0f, 0f);
				array[i - 1] = X.NI(0.6f, 0.85f, X.RAN(ran, 2798));
				if (i == 4)
				{
					array[i - 1] = 1f - array[i - 1];
				}
			}
			this.ColLvl(0.4f);
			EfSpellRegisteration.Md.Circle(x, y, num, 1f, false, 0f, 0f);
			this.ColLvl(0.65f);
			float num3 = X.ZSIN(t, 50f);
			EfSpellRegisteration.Md.Tri(0, 1, 2, false).Tri(0, 2, 4, false).Tri(4, 2, 3, false);
			for (int j = 0; j < 5; j++)
			{
				float num4 = num3;
				float num5 = array[j] * num * num4;
				float num6 = 1.5707964f - 6.2831855f * (float)j / 5f;
				array[j] = num5;
				float num7 = num5 * (X.MMX(-0.5f, X.COSI(t - (float)(j * 55), 70f), 0.5f) * 0.25f + 1f);
				EfSpellRegisteration.Md.PosD(x + num7 * X.Cos(num6), y + num7 * X.Sin(num6), null);
			}
			this.ColLvl(1f);
			float num8 = 0f;
			float num9 = 0f;
			float num10 = 0f;
			float num11 = 0f;
			for (int k = 0; k < 5; k++)
			{
				float num12 = array[k];
				float num13 = 1.5707964f - 6.2831855f * (float)k / 5f;
				float num14 = x + num12 * X.Cos(num13);
				float num15 = y + num12 * X.Sin(num13);
				if (k == 0)
				{
					num10 = num14;
					num11 = num15;
				}
				else
				{
					EfSpellRegisteration.Md.Line(num8, num9, num14, num15, 1f, false, 0f, 0f);
				}
				num8 = num14;
				num9 = num15;
			}
			EfSpellRegisteration.Md.Line(num10, num11, num8, num9, 1f, false, 0f, 0f);
			y -= 38f;
		}

		public bool drawAuth(float x, ref float y, float sw, float t, float maxt)
		{
			if (t <= 0f)
			{
				y -= 48f;
				return true;
			}
			sw = (float)(X.IntC(sw / 8f) * 8);
			this.ColLvl(0.25f);
			int num = (int)sw / 8;
			int num2 = 5;
			float num3 = x;
			float num4 = y;
			float num5;
			for (int i = 0; i <= num; i++)
			{
				num5 = X.ZLINE(t - (float)(i * 2), 18f);
				if (num5 <= 0f)
				{
					break;
				}
				EfSpellRegisteration.Md.Line(num3, num4, num3, num4 - 40f * num5, 0.5f, false, 0f, 0f);
				num3 += 8f;
			}
			for (int j = 0; j <= num2; j++)
			{
				num5 = X.ZLINE(t - 3f - (float)(j * 5), 40f);
				if (num5 <= 0f)
				{
					break;
				}
				EfSpellRegisteration.Md.Line(x, num4, x + sw * num5, num4, 0.5f, false, 0f, 0f);
				num4 -= 8f;
			}
			this.ColLvl(1f);
			t -= 10f;
			num4 = y - 20f;
			y -= 48f;
			if (t <= 0f)
			{
				return true;
			}
			int num6 = 6;
			int num7 = 4;
			Vector2[] array = new Vector2[num7];
			float num8 = x + 14f;
			EfSpellRegisteration.MdC.RotaPF(num8, num4, 1f, 1f, 0f, EfSpellRegisteration.PFPoint3, false, false, false, uint.MaxValue, false, 0);
			num5 = X.ZLINE(t, maxt) * (float)num6;
			float num9 = this.time0 * 60f;
			for (int k = 0; k < num6; k++)
			{
				float num10 = (float)k * 0.23f * 3.1415927f + t / 97f * 6.2831855f;
				uint ran = X.GETRAN2((int)(num9 + (float)(k * 21) + 9f), 5 + k % 7);
				float num11 = x + 14f + ((float)(k + 1) + ((k == num6 - 1) ? 0f : (-0.2f + 0.4f * X.RAN(ran, 2551)))) * (sw - 28f) / (float)num6;
				for (int l = 0; l < num7; l++)
				{
					uint ran2 = X.GETRAN2((int)(num9 + (float)(k * 43) + (float)(l * 7)), 3 + k % 11 + l % 7);
					Vector2 vector = ((k == 0) ? Vector2.zero : array[l]);
					Vector2 zero = Vector2.zero;
					float num12 = num4;
					bool flag = false;
					if (k < num6 - 1)
					{
						zero = new Vector2(X.correctangleR(num10 + 6.2831855f * ((float)l - 0.3f + 0.6f * X.RAN(ran2, 2903)) / (float)num7), X.NI(10, 19, X.RAN(ran2, 589)));
						array[l] = zero;
						num12 = num4 + X.Cos(zero.x) * zero.y;
						if (num5 >= 1f)
						{
							flag = true;
						}
					}
					this.ColLvl(1f + X.Mn(X.Mn(vector.x, zero.x) / 3.1415927f, 0f) * 0.6f);
					if (flag)
					{
						EfSpellRegisteration.MdC.RotaPF(num11, num12, 1f, 1f, 0f, EfSpellRegisteration.PFPoint3, false, false, false, uint.MaxValue, false, 0);
					}
					float num13 = num4 + X.Cos(vector.x) * vector.y;
					if (num5 >= 1f)
					{
						EfSpellRegisteration.Md.Line(num8, num13, num11, num12, 1f, false, 0f, 0f);
					}
					else
					{
						EfSpellRegisteration.Md.Line(num8, num13, X.NI(num8, num11, num5), X.NI(num13, num12, num5), 1f, false, 0f, 0f);
					}
				}
				if (num5 < 1f)
				{
					return true;
				}
				num8 = num11;
				num5 -= 1f;
			}
			EfSpellRegisteration.MdC.RotaPF(x + (sw - 14f), num4, 1f, 1f, 0f, EfSpellRegisteration.PFPoint3, false, false, false, uint.MaxValue, false, 0);
			return t < maxt + 30f;
		}

		public bool drawConsistency(float x, ref float y, float sw, float t, float maxt)
		{
			if (t <= 0f)
			{
				y -= 33f;
				return true;
			}
			sw = (float)(X.IntC(sw / 5f) * 5);
			this.ColLvl(1f);
			int num = (int)sw / 5;
			int num2 = 5;
			float num3 = x;
			float num4 = y;
			for (int i = 0; i <= num; i++)
			{
				float num5 = X.ZLINE(t - (float)(i * 2), 14f);
				if (num5 <= 0f)
				{
					break;
				}
				EfSpellRegisteration.Md.Line(num3, num4, num3, num4 - 25f * num5, 0.5f, false, 0f, 0f);
				num3 += 5f;
			}
			for (int j = 0; j <= num2; j++)
			{
				float num5 = X.ZLINE(t - 3f - (float)(j * 3), 18f);
				if (num5 <= 0f)
				{
					break;
				}
				EfSpellRegisteration.Md.Line(x, num4, x + sw * num5, num4, 0.5f, false, 0f, 0f);
				num4 -= 5f;
			}
			num4 = y - 5f;
			int num6 = 0;
			int num7 = 0;
			float num8 = X.ZLINE(t, maxt);
			int num9 = X.IntC((float)(num * num2) * num8);
			for (int k = 0; k < num2; k++)
			{
				num3 = x + 1f;
				for (int l = 0; l < num; l++)
				{
					uint num10;
					if (num7 >= num9)
					{
						num10 = X.GETRAN2((int)(t / 3f) + l * 7 + k * 11, l % 4 + k % 3);
					}
					else
					{
						num10 = X.GETRAN2(l * 9 + k * 4, l % 3 + k % 2);
					}
					if (X.RAN(num10, 1562) < 0.6f)
					{
						num6++;
					}
					else if (num6 > 0)
					{
						EfSpellRegisteration.Md.RectBL(num3 - (float)(num6 * 5) - 1f, num4, (float)(num6 * 5), 5f, false);
						num6 = 0;
					}
					num3 += 5f;
					num7++;
				}
				if (num6 > 0)
				{
					EfSpellRegisteration.Md.RectBL(num3 - (float)(num6 * 5) - 1f, num4, (float)(num6 * 5), 5f, false);
					num6 = 0;
				}
				num4 -= 5f;
			}
			y -= 33f;
			return t < maxt + 15f;
		}

		public bool drawSocketInfo(float x, ref float y, float t, float maxt)
		{
			if (t <= 0f)
			{
				y -= 38f;
				return true;
			}
			float num = 15f;
			y -= num;
			float num2 = num - 2f;
			for (int i = 0; i < 2; i++)
			{
				float num3 = x - (float)X.MPF(i == 1) * (8f + num);
				float num4 = t - (float)i * 18f;
				if (num4 < 0f)
				{
					break;
				}
				this.ColLvl(1f);
				float num5 = X.ZLINE(num4, maxt * 0.875f);
				if (num5 >= 1f)
				{
					EfSpellRegisteration.Md.Circle(num3, y, num, 1f, false, 0f, 0f);
				}
				else
				{
					EfSpellRegisteration.Md.Arc(num3, y, num, 1.5707964f - 6.2831855f * num5, 1.5707964f, 1f);
				}
				num4 -= 12f;
				if (num4 > 0f)
				{
					num5 = X.ZLINE(num4, maxt);
					float num6 = 1.5707964f + ((i == 1) ? 0.7853982f : 0f);
					if (num5 >= 1f)
					{
						EfSpellRegisteration.Md.Poly(num3, y, num2, num6, 4, 1.5f, false, 0f, 0f);
					}
					else
					{
						this.ColLvl(num5);
						for (int j = 0; j < 4; j++)
						{
							float num7 = num6 + 1.5707964f;
							float num8 = num3 + num2 * X.Cos(num6);
							float num9 = y + num2 * X.Sin(num6);
							float num10 = num3 + num2 * X.Cos(num7);
							float num11 = y + num2 * X.Sin(num7);
							num10 = X.NI(num8, num10, num5);
							num11 = X.NI(num9, num11, num5);
							EfSpellRegisteration.Md.Line(num8, num9, num10, num11, 1.5f, false, 0f, 0f);
							num6 = num7;
						}
					}
				}
			}
			this.ColLvl(1f);
			y -= num + 8f;
			return t < maxt + 18f + 15f;
		}

		private bool drawRaisingChar(EfSpellRegisteration.RaisingCharItem V)
		{
			float num = this.t - (float)V.t0;
			float num2 = 38f;
			float num3 = 3f;
			float num4 = num3 / V.spd;
			float num5 = num / 162f;
			float num6 = V.y + num5 * 50f * V.z;
			int num7 = (int)(162f / num3);
			bool flag = false;
			float num8 = V.a * X.ZLINE(num5 * 4f);
			for (int i = 0; i < num7; i++)
			{
				float num9 = num - num4 * (float)i;
				float num10 = 1f - X.ZLINE(num9 - 28f, num2);
				if (num10 > 0f)
				{
					if (num9 < 0f)
					{
						return true;
					}
					flag = true;
					uint ran = X.GETRAN2(V.t0 + (int)(num / 4f) * 13 + i * 7, (int)(X.Abs(V.x) + X.Abs(V.y)) % 11);
					this.ColLvl(num10 * num8);
					BMListChars.ChrImage charMesh = EfSpellRegisteration.Chr.getCharMesh((char)(65 + (int)(X.RAN(ran, 1581) * 24f)));
					if (charMesh != null)
					{
						charMesh.DrawTo(EfSpellRegisteration.MdC, V.x, num6, V.z, V.z);
					}
				}
				num6 += V.z * (EfSpellRegisteration.Chr.ch + 1f);
			}
			return flag;
		}

		public EfSpellRegisteration destruct()
		{
			this.Ef = null;
			return null;
		}

		private List<EfSpellRegisteration.SquareItem> ASq;

		private List<EfSpellRegisteration.RaisingCharItem> ARaisingChar;

		private List<EfSpellRegisteration.SpellRegisterItem> ADi;

		private float time0;

		private float t;

		private float t_item_margin = 40f;

		private float t_raising_char_margin = 70f;

		private int active_ri_bits;

		private EffectItem Ef;

		private Map2d Mp;

		private NelM2DBase M2D;

		private MGKIND kind;

		private MagicSelector.KindData Mk;

		private int di_content_level;

		private static PxlSequence SqMarker;

		private static EfSpellRegisteration Instance;

		private static BMListChars Chr;

		private static PxlFrame PFPoint3;

		public PRNoel Pr;

		public CoolCirculation Ccr = new CoolCirculation();

		public float circulation_level;

		public float efx;

		public float efy;

		private FnEffectRun FD_fnDrawEf;

		private static MeshDrawer Md;

		private static MeshDrawer MdC;

		private const float marker_anmf = 2f;

		private const int marker_f_intro = 7;

		private const int marker_f_loop = 5;

		private const int block_h_margin = 8;

		private const float personal_info_thumbnail_w = 32f;

		private const float char_anmf = 0.5f;

		private static float color_level = 1f;

		private static List<int> ABuf = new List<int>(8);

		private const int decode_meter_w = 100;

		private const float decode_meter_h = 10f;

		private const int meter_head_w = 3;

		private const float meter_margin_w = 3f;

		private const float circlegraph_wh = 60f;

		private const float auth_area_h = 40f;

		private const int auth_cell_wh = 8;

		private const float ccs_area_h = 25f;

		private const int ccs_cell_wh = 5;

		private const float skt_h = 30f;

		private class SquareItem
		{
			public void run(float fcnt)
			{
				this.xagR += this.x_spd * fcnt;
				this.yagR += this.y_spd * fcnt;
				this.zagR += this.z_spd * fcnt;
			}

			public float xagR = X.XORSP() * 6.2831855f;

			public float yagR = X.XORSP() * 6.2831855f;

			public float zagR = X.XORSP() * 6.2831855f;

			public float x_spd = 1f / X.NIXP(220f, 350f) * 6.2831855f;

			public float y_spd = 1f / X.NIXP(220f, 350f) * 6.2831855f;

			public float z_spd = 1f / X.NIXP(220f, 350f) * 6.2831855f;
		}

		private class SpellRegisterItem
		{
			public string text
			{
				set
				{
					if (TX.valid(value))
					{
						if (this.Stb_text == null)
						{
							this.Stb_text = new STB(value.Length);
						}
						else
						{
							this.Stb_text.Clear();
						}
						this.Stb_text += value;
					}
				}
			}

			public string text2
			{
				set
				{
					if (TX.valid(value))
					{
						if (this.Stb_text2 == null)
						{
							this.Stb_text2 = new STB(value.Length);
						}
						else
						{
							this.Stb_text2.Clear();
						}
						this.Stb_text2 += value;
					}
				}
			}

			public string header_text
			{
				set
				{
					if (TX.valid(value))
					{
						if (this.Stb_header_text == null)
						{
							this.Stb_header_text = new STB(value.Length);
						}
						else
						{
							this.Stb_header_text.Clear();
						}
						this.Stb_header_text += value;
					}
				}
			}

			public bool isActive()
			{
				return this.t0_deactivate < 0f;
			}

			public void deactivate(float t)
			{
				if (this.t0_deactivate < 0f)
				{
					this.t0_deactivate = t;
				}
			}

			public float _xd
			{
				get
				{
					return (float)CAim._XD(this.aim, 1);
				}
			}

			public float _yd
			{
				get
				{
					return (float)CAim._YD(this.aim, 1);
				}
			}

			public float t0_deactivate = -1f;

			public float t0;

			public AIM aim;

			public int l1;

			public int l2;

			public uint ran0;

			public float sw;

			public float sh;

			public float sx;

			public float sy;

			public EfSpellRegisteration.DTYPE type;

			public HpzSimulator Hpz;

			public STB Stb_text;

			public STB Stb_text2;

			public STB Stb_header_text;

			public float header_w;
		}

		private class RaisingCharItem
		{
			public float x;

			public float y;

			public float z;

			public float a;

			public float spd;

			public int t0;
		}

		private enum DTYPE
		{
			NOUSE,
			STRING,
			PERSONAL_INFO,
			NOTICE,
			DECODE,
			CONFORMITY,
			AUTHENTICATION,
			CONSISTENCY,
			SOCKETINFO,
			HPZ_SIMU
		}
	}
}
