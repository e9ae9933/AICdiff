using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Better;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public class NelItem
	{
		public byte obtain_count
		{
			get
			{
				return this.obtain_count_;
			}
			set
			{
				if (this.obtain_count_ == value)
				{
					return;
				}
				this.obtain_count_ = value;
			}
		}

		public NelItem(string _key, int rare, int price, int _stock)
		{
			this.key = _key;
			this.rare = rare;
			this.price = price;
			this.stock = _stock;
		}

		public int getIcon(ItemStorage Storage, ItemStorage.IRow IR = null)
		{
			int num;
			if (this.specific_icon_id != -1)
			{
				num = this.specific_icon_id;
			}
			else if ((this.category & NelItem.CATEG.BOTTLE) != NelItem.CATEG.OTHER)
			{
				num = 4;
			}
			else if ((this.category & NelItem.CATEG.WATER) != NelItem.CATEG.OTHER)
			{
				if ((this.category & NelItem.CATEG.FOOD) != NelItem.CATEG.OTHER)
				{
					num = 59;
				}
				else
				{
					num = 6;
				}
			}
			else if ((this.category & NelItem.CATEG.FOOD) != NelItem.CATEG.OTHER)
			{
				if ((this.category & NelItem.CATEG.TOOL) != NelItem.CATEG.OTHER)
				{
					num = 23;
				}
				else
				{
					num = RecipeManager.getDefaultIcon(this);
				}
			}
			else if ((this.category & NelItem.CATEG.TOOL) != NelItem.CATEG.OTHER)
			{
				num = 51;
			}
			else if ((this.category & NelItem.CATEG.ANC) != NelItem.CATEG.OTHER)
			{
				num = 12;
			}
			else if (this.category == (NelItem.CATEG)25165824U)
			{
				num = 46;
			}
			else if (this.category == NelItem.CATEG.BOMB)
			{
				num = 45;
			}
			else if (this.is_enhancer)
			{
				num = 19;
			}
			else if ((this.category & NelItem.CATEG.SPECIAL) != NelItem.CATEG.OTHER)
			{
				num = 14;
			}
			else
			{
				num = ((this.category == NelItem.CATEG.INDIVIDUAL_GRADE) ? 14 : 0);
			}
			if (IR != null && Storage != null && Storage.FD_RowIconAddition != null)
			{
				num = Storage.FD_RowIconAddition(IR, Storage, num);
			}
			return num;
		}

		public Color32 getColor(ItemStorage Storage)
		{
			Color32 color;
			if (this.SpecificColor.a > 0)
			{
				color = this.SpecificColor;
			}
			else
			{
				int icon = this.getIcon(Storage, null);
				if (icon <= 33)
				{
					switch (icon)
					{
					case 0:
						color = C32.d2c(4284045916U);
						goto IL_019A;
					case 1:
					case 2:
					case 3:
						color = C32.d2c(4294730130U);
						goto IL_019A;
					case 4:
						color = C32.d2c(4286681744U);
						goto IL_019A;
					case 5:
						color = C32.d2c(4284843219U);
						goto IL_019A;
					case 6:
						color = C32.d2c(4284070852U);
						goto IL_019A;
					case 7:
						color = C32.d2c(4294289762U);
						goto IL_019A;
					case 8:
						color = C32.d2c(4284514673U);
						goto IL_019A;
					case 9:
						color = C32.d2c(4291131377U);
						goto IL_019A;
					case 10:
						color = C32.d2c(4283668060U);
						goto IL_019A;
					case 11:
					case 12:
					case 15:
					case 16:
					case 17:
					case 18:
						break;
					case 13:
						color = C32.d2c(4283730868U);
						goto IL_019A;
					case 14:
						color = C32.d2c(4280236621U);
						goto IL_019A;
					case 19:
						color = C32.d2c(4285883391U);
						goto IL_019A;
					default:
						if (icon == 33)
						{
							color = C32.d2c(4288848546U);
							goto IL_019A;
						}
						break;
					}
				}
				else
				{
					if (icon == 34)
					{
						color = C32.d2c(4294935086U);
						goto IL_019A;
					}
					if (icon == 45)
					{
						color = C32.d2c(4293925469U);
						goto IL_019A;
					}
					if (icon - 46 <= 4)
					{
						color = C32.d2c(4284630737U);
						goto IL_019A;
					}
				}
				color = C32.d2c(4283780170U);
			}
			IL_019A:
			if (this.FnGetColor != null)
			{
				color = this.FnGetColor(this, Storage, (Storage != null) ? Storage.getTopGrade(this) : 0, color);
			}
			return color;
		}

		public bool nest_multiple
		{
			get
			{
				return true;
			}
		}

		public bool isWLinkUser(out bool must_link)
		{
			if (this.is_water)
			{
				must_link = true;
				return true;
			}
			must_link = false;
			return this.is_food || this.isEmptyBottle() || this.isEmptyLunchBox();
		}

		public bool connectableWLink(NelItem Another, out bool is_outer)
		{
			is_outer = true;
			if (this.connectableWLinkOuter(Another))
			{
				return true;
			}
			is_outer = false;
			return Another.connectableWLinkOuter(this);
		}

		public bool connectableWLinkOuter(NelItem Inner)
		{
			return (this.isEmptyBottle() && Inner.is_water) || (this.isEmptyLunchBox() && Inner.is_food && !Inner.is_water);
		}

		public int getValueFor(NelItem.CATEG categ, int grade)
		{
			NelItem.GrdVariation gradeVariation = this.getGradeVariation(grade, null);
			NelItem.ValMemoS[] array = NelItem.BufSet(this, gradeVariation, true);
			if (this.is_food)
			{
				global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1);
			}
			if (this.has(NelItem.CATEG.CURE_HP) && !this.has(NelItem.CATEG.CURE_MP_CRACK))
			{
				NelItem.ValMemoS valMemoS = global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1);
				if (categ == NelItem.CATEG.CURE_HP)
				{
					return valMemoS.getMinValue();
				}
				if (this.has(NelItem.CATEG.CURE_MP))
				{
					valMemoS = global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1);
					if (categ == NelItem.CATEG.CURE_MP)
					{
						return valMemoS.getMinValue();
					}
				}
			}
			else
			{
				if (this.has(NelItem.CATEG.CURE_HP))
				{
					NelItem.ValMemoS valMemoS = global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1);
					if (categ == NelItem.CATEG.CURE_HP)
					{
						return valMemoS.getMinValue();
					}
				}
				if (this.has(NelItem.CATEG.CURE_MP_CRACK))
				{
					NelItem.ValMemoS valMemoS = global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1);
					if (categ == NelItem.CATEG.CURE_MP_CRACK)
					{
						return valMemoS.getMinValue();
					}
				}
				if (this.has(NelItem.CATEG.CURE_MP))
				{
					NelItem.ValMemoS valMemoS = global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1);
					if (categ == NelItem.CATEG.CURE_MP)
					{
						return valMemoS.getMinValue();
					}
				}
			}
			if (this.has(NelItem.CATEG.CURE_EP))
			{
				NelItem.ValMemoS valMemoS = global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1);
				if (categ == NelItem.CATEG.CURE_EP)
				{
					return valMemoS.getMinValue();
				}
			}
			if (this.has(NelItem.CATEG.SER_CURE))
			{
				NelItem.ValMemoS valMemoS = global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1);
				if (categ == NelItem.CATEG.SER_CURE)
				{
					return global::XX.X.IntR(valMemoS.v_origin);
				}
			}
			if (this.has(NelItem.CATEG.SER_APPLY))
			{
				NelItem.ValMemoS valMemoS = global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1);
				if (categ == NelItem.CATEG.SER_APPLY)
				{
					return global::XX.X.IntR(valMemoS.v_origin);
				}
			}
			return 0;
		}

		public string getDetail(ItemStorage Storage, int grade = -1, ItemStorage.ObtainInfo Obt = null, bool detail_main_item_effect = true, bool detail_stock = true, bool detail_recipe = true)
		{
			STB stb = TX.PopBld(null, 0);
			NelItem.GrdVariation grdVariation = default(NelItem.GrdVariation);
			if (detail_main_item_effect)
			{
				if (!this.is_precious && detail_stock)
				{
					stb.AddTxA("Item_dbox_stock", false).TxRpl(this.stock);
				}
				if (this.value != 0f || this.value2 != 0f || this.value3 != 0f || (this.RecipeInfo != null && this.RecipeInfo.Oeffect100.Count != 0))
				{
					grdVariation = this.getGradeVariation(grade, Obt);
				}
				NelItem.ValMemoS[] array = NelItem.BufSet(this, grdVariation, true);
				if (this.is_food)
				{
					global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1);
				}
				if (this.has(NelItem.CATEG.CURE_HP) && !this.has(NelItem.CATEG.CURE_MP_CRACK))
				{
					global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1).detailAppendTxA(stb, "Item_detail_cure_hp", "\n");
					if (this.has(NelItem.CATEG.CURE_MP))
					{
						global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1).detailAppendTxA(stb, "Item_detail_cure_mp", "  /  ");
					}
				}
				else
				{
					if (this.has(NelItem.CATEG.CURE_HP))
					{
						global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1).detailAppendTxA(stb, "Item_detail_cure_hp", "\n");
					}
					if (this.has(NelItem.CATEG.CURE_MP_CRACK))
					{
						global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1).detailAppendTxA(stb, "Item_detail_cure_mp_crack", "\n");
					}
					if (this.has(NelItem.CATEG.CURE_MP))
					{
						global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1).detailAppendTxA(stb, "Item_detail_cure_mp", "\n");
					}
				}
				if (this.has(NelItem.CATEG.CURE_EP))
				{
					NelItem.ValMemoS valMemoS = global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1);
					if (valMemoS.v_origin != 0f)
					{
						STB stb2 = TX.PopBld(null, 0);
						valMemoS.getDetailTo(stb2);
						if (valMemoS.v_origin > 0f)
						{
							stb.AppendTxA("Item_detail_cure_ep", "\n").TxRpl(stb2);
						}
						else
						{
							stb2.RemoveChar('-', 0, -1);
							stb.AppendTxA("Item_detail_dmg_ep", "\n").TxRpl(stb2);
						}
						TX.ReleaseBld(stb2);
					}
				}
				if (this.has(NelItem.CATEG.SER_CURE))
				{
					global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1);
					ulong num;
					if (NelItem.getSerCureBits(this.key, out num))
					{
						using (STB stb3 = TX.PopBld(null, 0))
						{
							stb3.Clear();
							M2Ser.listupAllTitle(num, stb3);
							if (stb3.Length != 0)
							{
								stb.AppendTxA("Item_detail_cure_ser", "\n").TxRpl(stb3);
							}
						}
					}
				}
				if (this.has(NelItem.CATEG.SER_APPLY))
				{
					global::XX.X.shiftNotInput<NelItem.ValMemoS>(array, 1, 0, -1);
					ulong num2;
					if (NelItem.getSerApplyBits(this.key, out num2))
					{
						using (STB stb4 = TX.PopBld(null, 0))
						{
							stb4.Clear();
							M2Ser.listupAllTitle(num2, stb4);
							if (stb4.Length != 0)
							{
								stb.AppendTxA("Item_detail_dmg_ser", "\n").TxRpl(stb4);
							}
						}
					}
				}
				if (!this.nest_multiple)
				{
					if (stb.Length != 0)
					{
						stb.Add("\n");
					}
					stb.Add(NEL.error_tag, TX.Get("Item_detail_cannot_nest_multiple", ""), NEL.error_tag_close);
				}
				if (this.has(NelItem.CATEG.WATER) && Storage != null && !Storage.water_stockable)
				{
					stb.AppendTxA("Item_detail_filled_bottle", "\n");
				}
			}
			else
			{
				grdVariation = this.getGradeVariation(grade, Obt);
			}
			if (this.RecipeInfo != null && detail_recipe)
			{
				if (stb.Length != 0)
				{
					stb.Add("\n");
				}
				this.RecipeInfo.getDetailTo(stb, this, grdVariation);
			}
			string text = stb.ToString();
			TX.ReleaseBld(stb);
			if (this.FnGetDetail != null)
			{
				text = this.FnGetDetail(this, (Storage != null) ? Storage.getTopGrade(this) : 0, text);
			}
			return text;
		}

		public int Use(PR Pr, ItemStorage Storage, int grade, NelItem.IItemUser Usr = null)
		{
			bool flag = false;
			bool flag2 = false;
			NelItem.ValMemo[] array = this.BufSetFloat(grade, null, true);
			int num = 0;
			bool flag3 = false;
			string text = null;
			if (this.is_food)
			{
				global::XX.X.shiftNotInput<NelItem.ValMemo>(array, 1, 0, -1);
			}
			if (this.has(NelItem.CATEG.CURE_HP))
			{
				int v_int = global::XX.X.shiftNotInput<NelItem.ValMemo>(array, 1, 0, -1).v_int;
				if (Pr == null || Pr.NelItemUseableInt(this, NelItem.CATEG.CURE_HP, grade, v_int))
				{
					flag3 = (flag = true);
				}
				if (flag3 && Usr != null)
				{
					Usr.NelItemUseInt(this, NelItem.CATEG.CURE_HP, grade, v_int, ref num, ref text, ref flag2);
				}
			}
			flag3 = false;
			if (this.has(NelItem.CATEG.CURE_MP_CRACK))
			{
				int v_int2 = global::XX.X.shiftNotInput<NelItem.ValMemo>(array, 1, 0, -1).v_int;
				if (Pr == null || Pr.NelItemUseableInt(this, NelItem.CATEG.CURE_MP_CRACK, grade, v_int2))
				{
					flag3 = (flag = true);
				}
				if (flag3 && Usr != null)
				{
					Usr.NelItemUseInt(this, NelItem.CATEG.CURE_MP_CRACK, grade, v_int2, ref num, ref text, ref flag2);
				}
			}
			flag3 = false;
			if (this.has(NelItem.CATEG.CURE_MP))
			{
				int v_int3 = global::XX.X.shiftNotInput<NelItem.ValMemo>(array, 1, 0, -1).v_int;
				if (Pr == null || Pr.NelItemUseableInt(this, NelItem.CATEG.CURE_MP, grade, v_int3))
				{
					flag3 = (flag = true);
				}
				if (flag3 && Usr != null)
				{
					Usr.NelItemUseInt(this, NelItem.CATEG.CURE_MP, grade, v_int3, ref num, ref text, ref flag2);
				}
			}
			flag3 = false;
			if (this.has(NelItem.CATEG.CURE_EP))
			{
				int v_int4 = global::XX.X.shiftNotInput<NelItem.ValMemo>(array, 1, 0, -1).v_int;
				if (Pr == null || Pr.NelItemUseableInt(this, NelItem.CATEG.CURE_EP, grade, v_int4))
				{
					flag3 = (flag = true);
				}
				if (flag3 && Usr != null)
				{
					Usr.NelItemUseInt(this, NelItem.CATEG.CURE_EP, grade, v_int4, ref num, ref text, ref flag2);
				}
			}
			flag3 = false;
			if (this.has(NelItem.CATEG.SER_CURE))
			{
				int num2 = (int)global::XX.X.shiftNotInput<NelItem.ValMemo>(array, 1, 0, -1).v_origin;
				ulong num3;
				if (NelItem.getSerCureBits(this.key, out num3))
				{
					if (Pr == null || Pr.NelItemUseableUint(this, NelItem.CATEG.SER_CURE, grade, num3))
					{
						flag3 = (flag = true);
					}
					if (flag3 && Usr != null)
					{
						Usr.NelItemUseUint(this, NelItem.CATEG.SER_CURE, grade, num3, ref num, ref text, num2);
					}
				}
			}
			flag3 = false;
			if (this.has(NelItem.CATEG.SER_APPLY))
			{
				int num4 = (int)global::XX.X.shiftNotInput<NelItem.ValMemo>(array, 1, 0, -1).v_origin;
				ulong num5;
				if (NelItem.getSerApplyBits(this.key, out num5))
				{
					if (Pr == null || Pr.NelItemUseableUint(this, NelItem.CATEG.SER_APPLY, grade, num5))
					{
						flag3 = (flag = true);
					}
					if (flag3 && Usr != null)
					{
						Usr.NelItemUseUint(this, NelItem.CATEG.SER_APPLY, grade, num5, ref num, ref text, num4);
					}
				}
			}
			if (this.is_bomb && Storage != null && Pr != null && !Storage.infinit_stockable && Pr.canUseBombState(this))
			{
				flag = true;
				if (Usr != null)
				{
					Usr.NelItemUseInt(this, NelItem.CATEG.BOMB, grade, 0, ref num, ref text, ref flag2);
				}
			}
			SND.Ui.play(text, false);
			if (flag && flag2)
			{
				return 2;
			}
			if (!flag)
			{
				return 0;
			}
			return 1;
		}

		public int autoFixGradeSelection(ItemStorage.ObtainInfo Obt, int grade_cursor, PR Pr)
		{
			if (NelItem.AAvalFixGrade == null)
			{
				NelItem.AAvalFixGrade = new NelItem.ValMemo[5][];
				for (int i = 0; i < 5; i++)
				{
					NelItem.AAvalFixGrade[i] = new NelItem.ValMemo[3];
				}
			}
			NelItem.ValMemo[][] aavalFixGrade = NelItem.AAvalFixGrade;
			for (int j = 0; j < 5; j++)
			{
				if (Obt.getCount(j) != 0)
				{
					this.BufSetFloat(j, aavalFixGrade[j], false);
				}
			}
			int num = -1;
			int num2 = -1;
			if ((this.category & NelItem.CATEG.CURE_HP) != NelItem.CATEG.OTHER)
			{
				int num3 = -1;
				int num4 = -1;
				int num5 = (int)(Pr.get_maxhp() - Pr.get_hp());
				if (num5 > 0)
				{
					for (int k = 0; k < 5; k++)
					{
						if (Obt.getCount(k) != 0)
						{
							if (global::XX.X.shiftNotInput<NelItem.ValMemo>(aavalFixGrade[k], 1, 0, -1).v_int > num5)
							{
								num4 = k;
								break;
							}
							num3 = k;
						}
					}
				}
				if (num3 >= 0)
				{
					num = ((num == -1) ? num3 : global::XX.X.Mn(num3, num));
				}
				if (num4 >= 0)
				{
					num2 = ((num2 == -1) ? num4 : global::XX.X.Mn(num4, num2));
				}
			}
			if ((this.category & NelItem.CATEG.CURE_MP) != NelItem.CATEG.OTHER)
			{
				int num6 = -1;
				int num7 = -1;
				int num8 = (int)(Pr.get_maxmp() - Pr.get_mp());
				if (num8 > 0)
				{
					for (int l = 0; l < 5; l++)
					{
						if (Obt.getCount(l) != 0)
						{
							if (global::XX.X.shiftNotInput<NelItem.ValMemo>(aavalFixGrade[l], 1, 0, -1).v_int > num8)
							{
								num7 = l;
								break;
							}
							num6 = l;
						}
					}
				}
				if (num6 >= 0)
				{
					num = ((num == -1) ? num6 : global::XX.X.Mn(num6, num));
				}
				if (num7 >= 0)
				{
					num2 = ((num2 == -1) ? num7 : global::XX.X.Mn(num7, num2));
				}
			}
			if ((this.category & NelItem.CATEG.CURE_EP) != NelItem.CATEG.OTHER)
			{
				int num9 = -1;
				int num10 = -1;
				int num11 = (int)(1000f - (float)Pr.ep);
				if (num11 > 0)
				{
					for (int m = 0; m < 5; m++)
					{
						if (Obt.getCount(m) != 0)
						{
							if (global::XX.X.shiftNotInput<NelItem.ValMemo>(aavalFixGrade[m], 1, 0, -1).v_int > num11)
							{
								num10 = m;
								break;
							}
							num9 = m;
						}
					}
				}
				if (num9 >= 0)
				{
					num = ((num == -1) ? num9 : global::XX.X.Mn(num9, num));
				}
				if (num10 >= 0)
				{
					num2 = ((num2 == -1) ? num10 : global::XX.X.Mn(num10, num2));
				}
			}
			if (num >= 0)
			{
				grade_cursor = num;
			}
			else if (num2 >= 0)
			{
				grade_cursor = num2;
			}
			return grade_cursor;
		}

		public bool useable
		{
			get
			{
				return this.has(NelItem.CATEG.CURE_HP) || this.has(NelItem.CATEG.CURE_MP) || this.has(NelItem.CATEG.CURE_EP) || this.has(NelItem.CATEG.CURE_MP_CRACK) || this.has(NelItem.CATEG.SER_CURE) || this.has(NelItem.CATEG.SER_APPLY) || this.is_bomb;
			}
		}

		private static NelItem.ValMemoS[] BufSet(NelItem I, NelItem.GrdVariation Gvr, bool _consider_haniwa = false)
		{
			if (NelItem.Buf == null)
			{
				NelItem.Buf = new NelItem.ValMemoS[3];
			}
			NelItem.Buf[0] = new NelItem.ValMemoS(I.value, Gvr, _consider_haniwa);
			NelItem.Buf[1] = new NelItem.ValMemoS(I.value2, Gvr, _consider_haniwa);
			NelItem.Buf[2] = new NelItem.ValMemoS(I.value3, Gvr, _consider_haniwa);
			return NelItem.Buf;
		}

		private NelItem.ValMemo[] BufSetFloat(int grade, NelItem.ValMemo[] Dest = null, bool consider_haniwa = false)
		{
			if (Dest == null)
			{
				if (NelItem.BufU == null)
				{
					NelItem.BufU = new NelItem.ValMemo[3];
				}
				Dest = NelItem.BufU;
			}
			float num = this.getGradeMultiply(grade) * 1f;
			Dest[0] = new NelItem.ValMemo(this.value, num);
			Dest[1] = new NelItem.ValMemo(this.value2, num);
			Dest[2] = new NelItem.ValMemo(this.value3, num);
			return Dest;
		}

		public NelItem.GrdVariation getGradeVariation(int grade = -1, ItemStorage.ObtainInfo Obt = null)
		{
			if (grade == -1)
			{
				int num = -1;
				int num2 = -1;
				if (Obt == null || Obt.total < 0)
				{
					num = 0;
					num2 = 4;
				}
				else
				{
					int num3 = 5;
					for (int i = 0; i < num3; i++)
					{
						if (Obt.getCount(i) != 0)
						{
							if (num == -1)
							{
								num = i;
							}
							num2 = global::XX.X.Mx(num2, i);
						}
					}
					if (num == -1)
					{
						num = 0;
						num2 = 4;
					}
				}
				if (num >= 0 && num == num2)
				{
					grade = num;
				}
				if (grade == -1)
				{
					return new NelItem.GrdVariation(this.getGradeMultiply(num), this.getGradeMultiply(num2));
				}
			}
			return new NelItem.GrdVariation(this.getGradeMultiply(grade));
		}

		public bool isEmptyBottle()
		{
			return this.key == "mtr_bottle0";
		}

		public bool isEmptyLunchBox()
		{
			return this.key == "lunchbox";
		}

		public bool has(NelItem.CATEG categ)
		{
			return (this.category & categ) > NelItem.CATEG.OTHER;
		}

		public bool is_water
		{
			get
			{
				return this.has(NelItem.CATEG.WATER);
			}
		}

		public bool is_reelmbox
		{
			get
			{
				return this.category == (NelItem.CATEG)2097281U;
			}
		}

		public bool is_precious
		{
			get
			{
				return this.category == NelItem.CATEG.SPECIAL || this.category == (NelItem.CATEG)2097153U || (this.category & (NelItem.CATEG)3145729U) == (NelItem.CATEG)3145729U || this.is_enhancer;
			}
		}

		public bool is_enhancer
		{
			get
			{
				return this.category == (NelItem.CATEG)10485761U;
			}
		}

		public bool is_bomb
		{
			get
			{
				return this.has(NelItem.CATEG.BOMB) && !this.has(NelItem.CATEG.ENHANCER);
			}
		}

		public bool is_skillbook
		{
			get
			{
				return (this.category & NelItem.CATEG.SPECIAL) != NelItem.CATEG.OTHER && TX.isStart(this.key, "skillbook_", 0);
			}
		}

		public bool is_recipe
		{
			get
			{
				return (this.category & NelItem.CATEG.SPECIAL) != NelItem.CATEG.OTHER && TX.isStart(this.key, "Recipe_", 0);
			}
		}

		public bool is_workbench_craft
		{
			get
			{
				return this.is_precious && TX.isStart(this.key, "workbench_", 0);
			}
		}

		public bool is_trm_episode
		{
			get
			{
				return this.is_precious && TX.isStart(this.key, "TrmItem_", 0);
			}
		}

		public bool is_barunder_spconfig
		{
			get
			{
				return this.is_precious && TX.isStart(this.key, "spconfig_", 0);
			}
		}

		public bool is_cache_item
		{
			get
			{
				return TX.isStart(this.key, "__", 0);
			}
		}

		public bool is_food
		{
			get
			{
				NelItem.CATEG categ = this.category & (NelItem.CATEG)4294950911U;
				return categ == NelItem.CATEG.FOOD || categ == (NelItem.CATEG)135168U;
			}
		}

		public bool is_noel_water
		{
			get
			{
				return this.is_water && this.RecipeInfo != null && (this.RecipeInfo.categ & RecipeManager.RPI_CATEG.FROMNOEL) != (RecipeManager.RPI_CATEG)0;
			}
		}

		public bool is_tool
		{
			get
			{
				return (this.category & NelItem.CATEG.TOOL) == NelItem.CATEG.TOOL;
			}
		}

		public bool individual_grade
		{
			get
			{
				return this.has(NelItem.CATEG.INDIVIDUAL_GRADE);
			}
		}

		public bool dropable
		{
			get
			{
				return !this.is_precious;
			}
		}

		public static NelItem.CATEG calcCateg(string _categ_str)
		{
			string[] array = TX.split(_categ_str, "|");
			NelItem.CATEG categ = NelItem.CATEG.OTHER;
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				NelItem.CATEG categ2;
				if (FEnum<NelItem.CATEG>.TryParse(array[i], out categ2, true))
				{
					categ |= categ2;
				}
			}
			return categ;
		}

		public override string ToString()
		{
			return "<NelItem> " + this.ToSimpleString();
		}

		public string ToSimpleString()
		{
			return this.key + " (" + this.name_localized_ + ")";
		}

		public string fineNameLocalized()
		{
			if (this.RecipeInfo != null && this.RecipeInfo.DishInfo != null)
			{
				this.RecipeInfo.DishInfo.flushLocalizeTitle();
				return this.RecipeInfo.DishInfo.title;
			}
			TX tx = TX.getTX("_NelItem_name_" + this.key, true, true, null);
			this.name_localized_ = ((tx != null) ? tx.text : this.key);
			return this.name_localized_;
		}

		public string getLocalizedName(int grade, ItemStorage Storage = null)
		{
			string text;
			if (this.FnGetName != null)
			{
				text = this.FnGetName(this, grade, this.name_localized_);
			}
			else
			{
				text = this.name_localized_;
			}
			return text;
		}

		public string getDescLocalized(ItemStorage Storage, int grade)
		{
			TX tx = TX.getTX("_NelItem_desc_" + this.key, true, true, null);
			string text = ((tx != null) ? tx.text : "???");
			if ((this.category & NelItem.CATEG.WATER) != NelItem.CATEG.OTHER)
			{
				if (Storage == null || Storage.water_stockable)
				{
					text = TX.add(text, TX.Get("Item_desc_suffix_water_needs_empty_bottle", ""), "\n");
				}
				else
				{
					text = TX.add(text, TX.Get("Item_desc_suffix_bottled_water", ""), "\n");
				}
			}
			if (this.FnGetDesc != null)
			{
				text = this.FnGetDesc(this, grade, text);
			}
			return text;
		}

		public static STB getGradeMeshTxTo(STB Stb, int grade, int id = 0, int width = 34)
		{
			Stb.Add("<img mesh=\"nel_item_grade.", grade + id * 5, "\" width=\"");
			Stb.Add("", width, "\" tx_color />");
			return Stb;
		}

		public void defineValueReplacer(CsvVariableContainer VarCon, bool v1, bool v2, bool v3)
		{
			if (v1)
			{
				VarCon.define("_VALUE", " " + this.value.ToString() + " ", true);
			}
			if (v2)
			{
				VarCon.define("_VALUE2", " " + this.value2.ToString() + " ", true);
			}
			if (v3)
			{
				VarCon.define("_VALUE3", " " + this.value3.ToString() + " ", true);
			}
		}

		public static STB addCategoryDescTo(STB Stb, NelItem.CATEG target_ni_category)
		{
			Stb.AddTxA((target_ni_category == (NelItem.CATEG)1179648U) ? "alchemy_tool_for_cook" : "alchemy_tool_column", false);
			return Stb;
		}

		public void drawIconTo(MeshDrawer Md, ItemStorage Storage, int submesh_line, int submesh_icon, float x, float y, float scale = 1f, float alpha = 1f, ItemStorage.IRow IR = null)
		{
			Md.Col = C32.MulA(4283780170U, alpha);
			if (submesh_line != -1)
			{
				Md.chooseSubMesh(submesh_line, false, false);
			}
			if (IR == null || IR.hidden == ItemStorage.ROWHID.VISIBLE)
			{
				Md.Box(x, y, 30f * scale, 30f * scale, 1f, false);
			}
			else
			{
				Md.RectDashed(x, y, 30f * scale, 30f * scale, 0.5f, global::XX.X.IntR(60f), 1f, false, false);
			}
			if (submesh_icon != -1 && submesh_line != submesh_icon)
			{
				Md.chooseSubMesh(submesh_icon, false, false);
			}
			bool flag = Storage != null && Storage.isMngEffectConfusion();
			if (this.is_reelmbox && !flag)
			{
				ReelManager.ItemReelContainer ir = ReelManager.GetIR(this);
				if (ir != null)
				{
					ir.drawSmallIcon(Md, x, y, alpha, scale, false);
					return;
				}
			}
			if (!flag && this.is_precious && TX.isStart(this.key, "mapmarker_", 0))
			{
				Md.Col = C32.MulA(MTRX.ColWhite, alpha);
				UiGmMapMarker.drawTo(Md, x, y, this);
				return;
			}
			int num = (flag ? 36 : this.getIcon(Storage, IR));
			if (num >= 0)
			{
				Md.Col = (flag ? C32.MulA(4283780170U, alpha) : C32.MulA(this.getColor(Storage), alpha));
				Md.RotaPF(x, y, scale, scale, 0f, MTR.AItemIcon[num], false, false, false, uint.MaxValue, false, 0);
			}
		}

		public float getGradeMultiply(int grade)
		{
			if (!this.individual_grade)
			{
				return global::XX.X.NI(1f, this.max_grade_enpower, global::XX.X.ZLINE((float)grade, 4f));
			}
			return this.max_grade_enpower;
		}

		public float getGradePriceMultiply(int grade)
		{
			return global::XX.X.NI(1f, (this.max_price_enpower <= 0f) ? global::XX.X.NI(this.max_grade_enpower, this.max_grade_enpower * this.max_grade_enpower, 0.66f) : this.max_price_enpower, global::XX.X.ZLINE((float)grade, 4f));
		}

		public float getPrice(int grade)
		{
			return (float)this.price * this.getGradePriceMultiply(grade);
		}

		public float getSellPrice(int grade)
		{
			return this.getPrice(grade) * (float)this.sell_ratio / 127f;
		}

		public string getCountString(int count, ItemStorage Storage)
		{
			string text;
			using (STB stb = TX.PopBld(null, 0))
			{
				this.getCountString(stb, count, Storage);
				text = stb.ToString();
			}
			return text;
		}

		public STB getCountString(STB Stb, int count, ItemStorage Storage)
		{
			Stb.Add("x").spr0(count, 2, ' ');
			if (Storage != null && !Storage.infinit_stockable)
			{
				int itemStockable = Storage.getItemStockable(this);
				if (itemStockable < 100)
				{
					Stb.Add(" <font size=\"10\">/").spr0(itemStockable, 2, ' ').Add("</font>");
				}
			}
			return Stb;
		}

		public void prepareResource(M2DBase M2D)
		{
			if (this.is_bomb)
			{
				M2D.loadMaterialSnd("item_bomb");
			}
		}

		public NelItem addObtainCount(int v = 1)
		{
			if ((this.obtain_count & 127) == 127)
			{
				this.obtain_count &= 128;
			}
			this.obtain_count = (byte)((int)(this.obtain_count & 128) | global::XX.X.Mn(100, (int)(this.obtain_count & 127) + v));
			return this;
		}

		public NelItem touchObtainCount()
		{
			if (this.obtain_count == 0)
			{
				this.obtain_count = 127;
			}
			return this;
		}

		public bool fd_obtain_just_touched
		{
			get
			{
				return (this.obtain_count & 127) == 127;
			}
		}

		public bool fd_favorite
		{
			get
			{
				return (this.obtain_count & 128) > 0;
			}
			set
			{
				this.obtain_count = (this.obtain_count & 127) | (value ? 128 : 0);
			}
		}

		public int visible_obtain_count
		{
			get
			{
				int num = (int)(this.obtain_count & 127);
				if (num != 127)
				{
					return num;
				}
				return 0;
			}
			set
			{
				if (value == 0 && (this.obtain_count & 127) > 0)
				{
					this.obtain_count_ = (byte)((this.fd_favorite ? 128 : 0) | 127);
				}
			}
		}

		public static void readItemScript(string txt)
		{
			NelItem.OData = new NDic<NelItem>("NelItem", 0);
			NelItem.Oser_cure = new BDic<string, ulong>();
			NelItem.Oser_apply = new BDic<string, ulong>();
			BDic<string, int> bdic = new BDic<string, int>();
			CsvReader csvReader = new CsvReader(txt, CsvReader.RegSpace, false);
			csvReader.VarCon = new CsvVariableContainer();
			NelItem nelItem = null;
			Type type = Type.GetType("nel.NelItem,Assembly-CSharp");
			NelItem.cache_item_count = 0;
			ushort num = 1;
			for (;;)
			{
				bool flag = !csvReader.read();
				if (flag || csvReader.cmd == "}")
				{
					if (nelItem != null)
					{
						if (nelItem.key == "mtr_bottle0")
						{
							NelItem.Bottle = nelItem;
						}
						else if (nelItem.key == "mtr_noel_juice0")
						{
							NelItem.NoelJuice = nelItem;
						}
						else if (nelItem.key == "mtr_noel_milk")
						{
							NelItem.NoelMilk = nelItem;
						}
						else if (nelItem.key == "lunchbox")
						{
							NelItem.LunchBox = nelItem;
						}
						else if (nelItem.key == "workbench_bottle")
						{
							NelItem.HolderBottle = nelItem;
						}
					}
					nelItem = null;
					if (flag)
					{
						break;
					}
				}
				else if (csvReader.cmd == "%ID")
				{
					num = (ushort)global::XX.X.NmI(csvReader._1, (int)num, true, false);
				}
				else if (nelItem == null && REG.match(csvReader.slice_join(0, " ", ""), NelItem.RegItemHeader))
				{
					string r = REG.R2;
					if (REG.R1 == "@")
					{
						bdic[r] = csvReader.get_cur_line();
					}
					int num2 = global::XX.X.NmI(REG.R3, 0, false, false);
					int num3 = global::XX.X.NmI(REG.R4, 0, false, false);
					int num4 = global::XX.X.Mx(1, global::XX.X.NmI(REG.R5, 0, false, false));
					string text = r;
					NelItem nelItem2 = new NelItem(r, num2, num3, num4);
					ushort num5 = num;
					num = num5 + 1;
					nelItem = NelItem.CreateItemEntry(text, nelItem2, (int)num5, false);
				}
				else if (nelItem != null)
				{
					NelItem.readItemData(nelItem, csvReader, type, bdic);
				}
			}
			NelItem.Unknown = new NelItem("__unknown", 0, 0, 99);
			NelItem.Unknown.id = ushort.MaxValue;
			NelItem.Unknown.specific_icon_id = -2;
			NelItem.Unknown.SpecificColor = MTRX.ColTrnsp;
			NelItem.Unknown.FnGetName = (NelItem Itm, int grade, string def) => TX.Get("Talker_Unknown", "");
		}

		private static void readItemData(NelItem CurItm, CsvReader CR, Type TypeItem, BDic<string, int> Oitem_line)
		{
			if (!NelItem.readItemDataInner(CurItm, CR, TypeItem))
			{
				if (TX.isStart(CR.cmd, '@'))
				{
					string text = TX.slice(CR.cmd, 1);
					int num;
					if (!Oitem_line.TryGetValue(text, out num))
					{
						CR.tError("NelItem @指定されていないアイテム: " + text);
						return;
					}
					num++;
					int num2 = CR.get_cur_line() + 1;
					using (BList<string> blist = ListBuffer<string>.Pop(CR.clength - 1))
					{
						CR.copyDataTo(blist, 1);
						CR.seek_set(num);
						int num3 = num2 - num;
						int num4 = num;
						int num5 = 0;
						while (num5 < num3 && CR.readCorrectly())
						{
							if (CR.getLastStr() == "}")
							{
								num4 = CR.get_cur_line();
								break;
							}
							num5++;
						}
						CR.seek_set(num);
						while (CR.read() && CR.get_cur_line() < num4)
						{
							if (blist.Count <= 0 || blist.IndexOf(CR.cmd) != -1)
							{
								NelItem.readItemData(CurItm, CR, TypeItem, Oitem_line);
							}
						}
					}
					CR.seek_set(num2);
					return;
				}
				else
				{
					CR.tError("NelItem 不明なコマンド: " + CR.cmd);
				}
			}
		}

		private static bool readItemDataInner(NelItem CurItm, CsvReader CR, Type TypeItem)
		{
			string cmd = CR.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 2090980678U)
				{
					if (num <= 1538843864U)
					{
						if (num <= 622060074U)
						{
							if (num != 1639381U)
							{
								if (num != 622060074U)
								{
									return false;
								}
								if (!(cmd == "VALUE"))
								{
									return false;
								}
								CurItm.value = (float)CR.IntE(1, 0);
								CurItm.defineValueReplacer(CR.VarCon, true, false, false);
								return true;
							}
							else
							{
								if (!(cmd == "SELL_RATIO"))
								{
									return false;
								}
								CurItm.sell_ratio = (byte)(CR.Nm(1, 1f) * 127f);
								return true;
							}
						}
						else if (num != 856066574U)
						{
							if (num != 1538843864U)
							{
								return false;
							}
							if (!(cmd == "MAX_PRICE_ENPOWER"))
							{
								return false;
							}
							CurItm.max_price_enpower = CR.Nm(1, CurItm.max_price_enpower);
							return true;
						}
						else
						{
							if (!(cmd == "MAX_GRADE_ENPOWER"))
							{
								return false;
							}
							CurItm.max_grade_enpower = CR.Nm(1, CurItm.max_grade_enpower);
							return true;
						}
					}
					else if (num <= 1872097003U)
					{
						if (num != 1548537093U)
						{
							if (num != 1872097003U)
							{
								return false;
							}
							if (!(cmd == "NAME_FN"))
							{
								return false;
							}
						}
						else
						{
							if (!(cmd == "SELL"))
							{
								return false;
							}
							CurItm.sell_ratio = (byte)((float)CR.Int(1, 0) / 0.4f / (float)CurItm.price * 127f);
							return true;
						}
					}
					else if (num != 1984752584U)
					{
						if (num != 2001530203U)
						{
							if (num != 2090980678U)
							{
								return false;
							}
							if (!(cmd == "SER_APPLY"))
							{
								return false;
							}
							ulong num2;
							if (!NelItem.Oser_apply.TryGetValue(CurItm.key, out num2))
							{
								num2 = 0UL;
							}
							for (int i = 1; i < CR.clength; i++)
							{
								SER ser;
								if (FEnum<SER>.TryParse(CR.getIndex(i), out ser, true))
								{
									num2 |= 1UL << ((int)ser & 31);
								}
								else
								{
									CR.tError("不明なser:" + CR._1);
								}
							}
							NelItem.Oser_apply[CurItm.key] = num2;
							return true;
						}
						else
						{
							if (!(cmd == "VALUE3"))
							{
								return false;
							}
							CurItm.value3 = (float)CR.IntE(1, 0);
							CurItm.defineValueReplacer(CR.VarCon, false, false, true);
							return true;
						}
					}
					else
					{
						if (!(cmd == "VALUE2"))
						{
							return false;
						}
						CurItm.value2 = (float)CR.IntE(1, 0);
						CurItm.defineValueReplacer(CR.VarCon, false, true, false);
						return true;
					}
				}
				else if (num <= 3810419663U)
				{
					if (num <= 2716703379U)
					{
						if (num != 2245486407U)
						{
							if (num != 2716703379U)
							{
								return false;
							}
							if (!(cmd == "CATEG"))
							{
								return false;
							}
							for (int j = 1; j < CR.clength; j++)
							{
								NelItem.CATEG categ;
								if (!FEnum<NelItem.CATEG>.TryParse(CR.getIndex(j), out categ, true))
								{
									CR.de("不明なカテゴリー ");
								}
								else
								{
									CurItm.category |= categ;
								}
							}
							return true;
						}
						else
						{
							if (!(cmd == "MAX_GRADE_VALUE2"))
							{
								return false;
							}
							if (CurItm.value2 == 0f)
							{
								CR.tError("VALUE2 を設定したあとに呼ぶこと");
								return true;
							}
							CurItm.max_grade_enpower = (float)CR.IntE(1, 0) / CurItm.value2;
							return true;
						}
					}
					else if (num != 3595368097U)
					{
						if (num != 3615321837U)
						{
							if (num != 3810419663U)
							{
								return false;
							}
							if (!(cmd == "MAX_GRADE_VALUE"))
							{
								return false;
							}
							if (CurItm.value == 0f)
							{
								CR.tError("VALUE を設定したあとに呼ぶこと");
								return true;
							}
							CurItm.max_grade_enpower = (float)CR.IntE(1, 0) / CurItm.value;
							return true;
						}
						else if (!(cmd == "DESC_FN"))
						{
							return false;
						}
					}
					else if (!(cmd == "DETAIL_FN"))
					{
						return false;
					}
				}
				else if (num <= 3878524667U)
				{
					if (num != 3828435440U)
					{
						if (num != 3878524667U)
						{
							return false;
						}
						if (!(cmd == "RECIPEINFO"))
						{
							return false;
						}
						if (CurItm.RecipeInfo == null)
						{
							CurItm.RecipeInfo = new RecipeManager.RecipeItemInfo(CurItm, CR._1, CR.Int(2, 1), CR._3, CR._4);
						}
						if (CurItm.RecipeInfo.categ == (RecipeManager.RPI_CATEG)0)
						{
							CR.tError("レシピカテゴリエラー: " + CR._1);
							return true;
						}
						return true;
					}
					else
					{
						if (!(cmd == "ICON"))
						{
							return false;
						}
						CurItm.specific_icon_id = CR.Int(1, -1);
						return true;
					}
				}
				else if (num != 3888318712U)
				{
					if (num != 4002881653U)
					{
						if (num != 4158601515U)
						{
							return false;
						}
						if (!(cmd == "SER_CURE"))
						{
							return false;
						}
						ulong num2;
						if (!NelItem.Oser_cure.TryGetValue(CurItm.key, out num2))
						{
							num2 = 0UL;
						}
						for (int k = 1; k < CR.clength; k++)
						{
							SER ser2;
							if (FEnum<SER>.TryParse(CR.getIndex(k), out ser2, true))
							{
								num2 |= 1UL << ((int)ser2 & 31);
							}
							else
							{
								CR.tError("不明なser:" + CR._1);
							}
						}
						NelItem.Oser_cure[CurItm.key] = num2;
						return true;
					}
					else
					{
						if (!(cmd == "COLOR_FN"))
						{
							return false;
						}
						goto IL_06A4;
					}
				}
				else
				{
					if (!(cmd == "COLOR"))
					{
						return false;
					}
					CurItm.SpecificColor = C32.d2c(CR.get_color(1));
					return true;
				}
				try
				{
					MethodInfo method = TypeItem.GetMethod(CR._1);
					if (!(method != null))
					{
						throw new Exception("No Specific Method: " + CR._1);
					}
					FnGetItemDetail fnGetItemDetail = (FnGetItemDetail)Delegate.CreateDelegate(typeof(FnGetItemDetail), method);
					if (fnGetItemDetail == null)
					{
						throw new Exception("No Available Method: " + CR._1);
					}
					if (CR.cmd == "NAME_FN")
					{
						CurItm.FnGetName = fnGetItemDetail;
					}
					if (CR.cmd == "DESC_FN")
					{
						CurItm.FnGetDesc = fnGetItemDetail;
					}
					if (CR.cmd == "DETAIL_FN")
					{
						CurItm.FnGetDetail = fnGetItemDetail;
					}
					return true;
				}
				catch (Exception ex)
				{
					CR.tError(ex.ToString());
					return true;
				}
				IL_06A4:
				try
				{
					MethodInfo method2 = TypeItem.GetMethod(CR._1);
					if (!(method2 != null))
					{
						throw new Exception("No Specific Method: " + CR._1);
					}
					FnGetItemColor fnGetItemColor = (FnGetItemColor)Delegate.CreateDelegate(typeof(FnGetItemColor), method2);
					if (fnGetItemColor == null)
					{
						throw new Exception("No Available Method: " + CR._1);
					}
					CurItm.FnGetColor = fnGetItemColor;
					return true;
				}
				catch (Exception ex2)
				{
					CR.tError(ex2.ToString());
					return true;
				}
				return false;
			}
			return false;
		}

		public static NelItem CreateItemEntry(string key, NelItem Itm, int id, bool no_error = false)
		{
			if (!NelItem.OData.ContainsKey(key))
			{
				NelItem.OData[key] = Itm;
				Itm.id = (ushort)id;
				if (Itm.is_cache_item)
				{
					NelItem.cache_item_count++;
				}
				return Itm;
			}
			if (no_error)
			{
				return NelItem.OData[key];
			}
			global::XX.X.de("アイテム名の重複: " + key, null);
			return null;
		}

		public static void flush(NelItem Itm)
		{
			if (Itm.is_cache_item)
			{
				NelItem.OData.Remove(Itm.key);
				if (NelItem.cache_item_count > 0)
				{
					NelItem.cache_item_count--;
				}
			}
		}

		public static void clearCacheItem()
		{
			BList<string> blist = null;
			foreach (KeyValuePair<string, NelItem> keyValuePair in NelItem.OData)
			{
				keyValuePair.Value.obtain_count = 0;
				if (keyValuePair.Value.is_cache_item)
				{
					if (blist == null)
					{
						blist = ListBuffer<string>.Pop(NelItem.cache_item_count);
					}
					blist.Add(keyValuePair.Key);
				}
			}
			if (blist != null)
			{
				for (int i = blist.Count - 1; i >= 0; i--)
				{
					NelItem.OData.Remove(blist[i]);
					NelItem.Oser_apply.Remove(blist[i]);
					NelItem.Oser_cure.Remove(blist[i]);
				}
				ListBuffer<string>.Release(blist);
			}
			NelItem.cache_item_count = 0;
		}

		public static void fineNameLocalizedWhole()
		{
			if (NelItem.localized_tx_key == TX.getCurrentFamilyName())
			{
				return;
			}
			NelItem.localized_tx_key = TX.getCurrentFamilyName();
			RecipeManager.fineNameLocalizedWhole();
			NelItem.tx_item_get = TX.Get("Item_get", "");
			foreach (KeyValuePair<string, NelItem> keyValuePair in NelItem.OData)
			{
				keyValuePair.Value.fineNameLocalized();
			}
		}

		public static NelItem GetById(string item_key, bool no_error = false)
		{
			if (item_key == null)
			{
				return null;
			}
			NelItem nelItem;
			if (NelItem.OData.TryGetValue(item_key, out nelItem))
			{
				return nelItem;
			}
			if (!no_error)
			{
				global::XX.X.de("不明なアイテム キー:" + item_key, null);
			}
			return null;
		}

		public static NelItem GetByUId(ushort uid, bool no_error = false)
		{
			foreach (KeyValuePair<string, NelItem> keyValuePair in NelItem.OData)
			{
				if (keyValuePair.Value.id == uid)
				{
					return keyValuePair.Value;
				}
			}
			if (!no_error)
			{
				global::XX.X.de("不明なアイテム UID:" + uid.ToString(), null);
			}
			return null;
		}

		public static bool getSerCureBits(string item_key, out ulong _out)
		{
			return NelItem.Oser_cure.TryGetValue(item_key, out _out);
		}

		public static bool getSerApplyBits(string item_key, out ulong _out)
		{
			return NelItem.Oser_apply.TryGetValue(item_key, out _out);
		}

		public static void setSerApplyBits(string item_key, ulong v)
		{
			NelItem.Oser_apply[item_key] = v;
		}

		public static void setSerCureBits(string item_key, ulong v)
		{
			NelItem.Oser_cure[item_key] = v;
		}

		public static bool preparedData()
		{
			return NelItem.OData != null;
		}

		public static NDic<NelItem> getWholeDictionary()
		{
			return NelItem.OData;
		}

		public static string fnGetNameNoelJuice(NelItem Itm, int grade, string def)
		{
			return TX.GetA("noel_juice_grade" + grade.ToString(), def);
		}

		public static string fnGetNameNoelCloth(NelItem Itm, int grade, string def)
		{
			if (grade >= 3)
			{
				return def;
			}
			string text = "";
			if ((grade & 1) == 0)
			{
				text = TX.Get("prefix_item_splashed", "");
			}
			if ((grade & 2) == 0)
			{
				text = ((text == "") ? TX.Get("prefix_item_broken", "") : TX.GetA("__adding", text, TX.Get("prefix_item_broken", "")));
				def = TX.Get("item_abb_noel_cloth", "");
			}
			return TX.GetA("__adding", text, def);
		}

		public static string fnGetDescNoelCloth(NelItem Itm, int grade, string def)
		{
			if (grade >= 3)
			{
				return def + "\n" + TX.Get("desc_noel_cloth_0", "");
			}
			if ((grade & 1) == 0)
			{
				def = def + "\n" + TX.Get("desc_noel_cloth_1", "");
			}
			if ((grade & 2) == 0)
			{
				def = def + "\n" + TX.Get("desc_noel_cloth_2", "");
			}
			return def;
		}

		public static Color32 fnGetColorNoelCloth(NelItem Itm, ItemStorage Str, int grade, Color32 C)
		{
			if (grade >= 3 || Str == null)
			{
				return C32.d2c(4289374890U);
			}
			return C32.d2c(4288833383U);
		}

		public static string fnGetDetailNoelCloth(NelItem Itm, int grade, string def)
		{
			if (grade >= 3)
			{
				return TX.Get("detail_noel_cloth_0", "");
			}
			return TX.Get("detail_noel_cloth_1", "");
		}

		public static string fnGetDetailCoffeeMakerTicket(NelItem Itm, int grade, string def)
		{
			return TX.Get("detail_coffeemaker_ticket", "");
		}

		public static string fnGetDetailScapecat(NelItem Itm, int grade, string def)
		{
			string text;
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AddTxA("detail_scapecat", false);
				int num;
				int num2;
				float num3;
				int num4;
				MDAT.getScapecatReversalHpMp(grade, null, out num, out num2, out num3, out num4);
				stb.TxRpl(num).TxRpl(num2).TxRpl(num4);
				text = stb.ToString();
			}
			return text;
		}

		public static string fnGetNameNoelShorts(NelItem Itm, int grade, string def)
		{
			if (grade >= 3)
			{
				return def;
			}
			return TX.Get("name_noel_shorts_1", "");
		}

		public static string fnGetDescNoelShorts(NelItem Itm, int grade, string def)
		{
			if (grade >= 3)
			{
				return def + "\n" + TX.Get("desc_noel_shorts_0", "");
			}
			if ((grade & 1) == 0)
			{
				def = def + "\n" + TX.Get("desc_noel_shorts_1", "");
			}
			if ((grade & 2) == 0)
			{
				def = def + "\n" + TX.Get("desc_noel_shorts_2", "");
			}
			return def;
		}

		public static string fnGetNameSkillBook(NelItem Itm, int grade, string def)
		{
			PrSkill prSkill = SkillManager.Get(TX.slice(Itm.key, 10));
			if (prSkill != null && prSkill.is_sp_map)
			{
				return prSkill.title;
			}
			return TX.GetA("name_skill_book", (prSkill != null) ? prSkill.title : "???");
		}

		public static string fnGetDescSkillBook(NelItem Itm, int grade, string def)
		{
			PrSkill prSkill = SkillManager.Get(TX.slice(Itm.key, 10));
			if (prSkill != null && prSkill.is_sp_map)
			{
				return prSkill.descript;
			}
			return TX.GetA("desc_skill_book", (prSkill != null) ? prSkill.title : "???");
		}

		public static string fnGetDetailSkillBook(NelItem Itm, int grade, string def)
		{
			string text = TX.slice(Itm.key, 10);
			PrSkill prSkill = SkillManager.Get(text);
			if (prSkill == null)
			{
				return "<ERROR> No Specfic Skill:\n" + text;
			}
			if (!prSkill.is_sp_map)
			{
				return prSkill.descript;
			}
			return "";
		}

		public static string fnGetNameEnhancer(NelItem Itm, int grade, string def)
		{
			EnhancerManager.Enhancer enhancer = EnhancerManager.Get(TX.slice(Itm.key, "Enhancer_".Length));
			return TX.GetA("name_enhancer_device", (enhancer != null) ? enhancer.title : "???");
		}

		public static string fnGetDescEnhancer(NelItem Itm, int grade, string def)
		{
			EnhancerManager.Enhancer enhancer = EnhancerManager.Get(TX.slice(Itm.key, "Enhancer_".Length));
			return TX.GetA("desc_enhancer_device", (enhancer != null) ? enhancer.title : "???");
		}

		public static string fnGetDetailEnhancer(NelItem Itm, int grade, string def)
		{
			string text = TX.slice(Itm.key, "Enhancer_".Length);
			EnhancerManager.Enhancer enhancer = EnhancerManager.Get(text);
			if (enhancer != null)
			{
				return TX.GetA("enhancer_cost", EnhancerManager.getCostString(enhancer.cost, "")) + "\n" + enhancer.descript;
			}
			return "<ERROR> No Specfic Enhancer:\n" + text;
		}

		public static string fnGetNameRecipe(NelItem Itm, int grade, string def)
		{
			RecipeManager.Recipe recipe = RecipeManager.Get(TX.slice(Itm.key, "Recipe_".Length));
			if (recipe == null)
			{
				return "???";
			}
			return TX.GetA("name_recipe_" + recipe.categ.ToString().ToLower(), recipe.title);
		}

		public static string fnGetDescRecipe(NelItem Itm, int grade, string def)
		{
			RecipeManager.Recipe recipe = RecipeManager.Get(TX.slice(Itm.key, "Recipe_".Length));
			if (recipe == null)
			{
				return "???";
			}
			string text;
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AddTxA((recipe.categ == RecipeManager.RP_CATEG.ALCHEMY || recipe.categ == RecipeManager.RP_CATEG.ALCHEMY_WORKBENCH) ? "desc_recipe_alchemy" : "desc_recipe_cook", false).TxRpl(recipe.title + ((recipe.create_count > 1) ? (" x" + recipe.create_count.ToString()) : ""));
				if (recipe.is_water)
				{
					stb.Add("\n").AddTxA("Item_desc_suffix_water_needs_empty_bottle", false);
				}
				text = stb.ToString();
			}
			return text;
		}

		public static string fnGetDetailRecipe(NelItem Itm, int grade, string def)
		{
			string text = TX.slice(Itm.key, "Recipe_".Length);
			RecipeManager.Recipe recipe = RecipeManager.Get(text);
			if (recipe != null)
			{
				return recipe.listupIngredients("／", false, false);
			}
			return "<ERROR> No Specfic Recipe:\n" + text;
		}

		public static string fnGetNameFood(NelItem Itm, int grade, string def)
		{
			if (Itm.RecipeInfo != null && Itm.RecipeInfo.DishInfo != null)
			{
				return Itm.RecipeInfo.DishInfo.title;
			}
			return TX.Get("Food_unknown", "");
		}

		public static string fnGetDescFood(NelItem Itm, int grade, string def)
		{
			if (Itm.RecipeInfo != null && Itm.RecipeInfo.DishInfo != null)
			{
				return Itm.RecipeInfo.DishInfo.descript;
			}
			return TX.Get("Food_unknown_desc", "");
		}

		public static string fnGetDetailInventoryEnlarge(NelItem Itm, int grade, string def)
		{
			return TX.GetA("Item_detail_inventory_increase2", Itm.value.ToString(), 24.ToString());
		}

		public static string fnGetDetailDangerousMeter(NelItem Itm, int grade, string def)
		{
			return TX.Get("desc_dangerous_meter", "");
		}

		public static string fnGetNameItemReel(NelItem Itm, int grade, string def)
		{
			string text = TX.slice(Itm.key, "itemreelC_".Length);
			if (ReelManager.GetIR(text, false) == null)
			{
				return "???";
			}
			string text2 = "";
			if (grade > 0)
			{
				if (TX.isStart(Itm.key, "itemreelC_", 0))
				{
					text2 = text2 + "+" + grade.ToString();
				}
				else
				{
					text2 = text2 + "+<img mesh=\"nel_item_grade." + (10 + (grade - 1)).ToString() + "\" width=\"34\" tx_color/>";
				}
			}
			return TX.GetA("ItemReel_Name_Base", TX.Get("_ItemReel_name_" + text, ""), text2);
		}

		public static string fnGetDescItemReel(NelItem Itm, int grade, string def)
		{
			return TX.Get("ItemReel_Desc_Base", "");
		}

		public static string fnGetDetailItemReel(NelItem Itm, int grade, string def)
		{
			string text = TX.slice(Itm.key, "itemreelC_".Length);
			ReelManager.ItemReelContainer ir = ReelManager.GetIR(text, false);
			if (ir != null)
			{
				return ir.listupItems("／", false);
			}
			return "<ERROR> No Specfic ItemReel:\n" + text;
		}

		public static Color32 fnGetColorItemReel(NelItem Itm, ItemStorage Str, int grade, Color32 C)
		{
			ReelManager.ItemReelContainer ir = ReelManager.GetIR(TX.slice(Itm.key, "itemreelC_".Length), false);
			if (ir != null)
			{
				return C32.d2c(ir.ColSet.top);
			}
			return C32.d2c(4294639477U);
		}

		public static string fnGetNameMapMarker(NelItem Itm, int grade, string def)
		{
			string text = TX.slice(Itm.key, "mapmarker_".Length);
			string text2;
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AddTxA("mapmarker_name_suffix", false).TxRpl(TX.Get("mapmarker_name_" + text, ""));
				text2 = stb.ToString();
			}
			return text2;
		}

		public static string fnGetDescMapMarker(NelItem Itm, int grade, string def)
		{
			return TX.Get("mapmarker_Desc", "");
		}

		public static string fnGetNameSiphon(NelItem Itm, int grade, string def)
		{
			if (grade == 4)
			{
				return TX.Get("Item_name_siphon_by_ticket", "");
			}
			return def;
		}

		public static string fnGetDescSiphon(NelItem Itm, int grade, string def)
		{
			if (grade == 4)
			{
				return TX.Get("Item_desc_siphon_by_ticket", "");
			}
			return def;
		}

		public static string fnGetNameSpConfig(NelItem Itm, int grade, string def)
		{
			return (TX.Get("Config_" + Itm.key, "") ?? "").Replace("\n", "");
		}

		public static string fnGetDescSpConfig(NelItem Itm, int grade, string def)
		{
			string text;
			if (TX.isStart(Itm.key, "spconfig_ep_boost_", out text, 0))
			{
				return TX.GetA("Config_desc_spconfig_ep_boost", TX.Get("EP_Targ_" + text, ""));
			}
			return TX.Get("Config_desc_" + Itm.key, "");
		}

		public static string fnGetDetailSpConfig(NelItem Itm, int grade, string def)
		{
			return TX.Get("Item_special_config_detail", "");
		}

		public static void readBinaryFrom(ByteArray Ba, bool old_ver, bool fix_ver024 = false)
		{
			ushort num = Ba.readUShort();
			for (int i = 0; i < (int)num; i++)
			{
				NelItem nelItem;
				NelItem.readBinaryGetKey(Ba, out nelItem, true, fix_ver024);
				int num2 = (int)Ba.readUByte();
				if (nelItem != null)
				{
					nelItem.obtain_count = (byte)(old_ver ? global::XX.X.Mn(global::XX.X.Mx(num2, (int)nelItem.obtain_count), 100) : num2);
				}
			}
		}

		public static ByteArray writeBinaryTo(ByteArray Ba)
		{
			using (BList<NelItem> blist = ListBuffer<NelItem>.Pop(0))
			{
				foreach (KeyValuePair<string, NelItem> keyValuePair in NelItem.OData)
				{
					NelItem nelItem = keyValuePair.Value;
					if (!nelItem.is_cache_item && nelItem.obtain_count > 0)
					{
						blist.Add(nelItem);
					}
				}
				Ba.writeUShort((ushort)blist.Count);
				int count = blist.Count;
				for (int i = 0; i < count; i++)
				{
					NelItem nelItem2 = blist[i];
					NelItem.writeBinaryItemKey(Ba, nelItem2);
					Ba.writeByte((int)nelItem2.obtain_count);
				}
			}
			return Ba;
		}

		public static void readBinaryGetKey(ByteArray Ba, out NelItem Kind, bool no_error = false, bool fix_ver024 = false)
		{
			ushort num = Ba.readUShort();
			if (num == 0)
			{
				Kind = null;
				return;
			}
			if (num == 65535)
			{
				Kind = NelItem.GetById(Ba.readPascalString("utf-8", false), no_error);
				return;
			}
			if (fix_ver024 && global::XX.X.BTW(61613f, (float)num, 61700f))
			{
				num += 8;
			}
			Kind = NelItem.GetByUId(num, no_error);
		}

		public static void writeBinaryItemKey(ByteArray Ba, NelItem Itm)
		{
			if (Itm == null)
			{
				Ba.writeUShort(0);
				return;
			}
			if (Itm.is_recipe)
			{
				Ba.writeUShort(ushort.MaxValue);
				Ba.writePascalString(Itm.key, "utf-8");
				return;
			}
			Ba.writeUShort(Itm.id);
			if (Itm.id == 65535)
			{
				Ba.writePascalString(Itm.key, "utf-8");
			}
		}

		public const string empty_bottle_key = "mtr_bottle0";

		public const string empty_lunchbox_key = "lunchbox";

		public const string noel_juice_key = "mtr_noel_juice0";

		public const string noel_milk_key = "mtr_noel_milk";

		public const string enhancer_slot_key = "enhancer_slot";

		public const string noel_egg_key = "mtr_noel_egg";

		public const string noel_egg_liquid_key = "mtr_essence0";

		public const string itemreelC_header = "itemreelC_";

		public const string itemreelG_header = "itemreelG_";

		public const string bomb_fire_key = "throw_bomb";

		public const string holder_bottle_key = "workbench_bottle";

		public const string bomb_thunder_key = "throw_lightbomb";

		public const string bomb_magic_key = "throw_magicbomb";

		public const string scapecat_key = "scapecat";

		public const string eggremover_key = "precious_egg_remover";

		public const string money_key = "%%MONEY";

		public const int GRADE_MAX = 5;

		public readonly string key;

		public readonly int rare;

		public int price;

		public byte sell_ratio = 127;

		public static string localized_tx_key = "";

		private string name_localized_;

		public int stock;

		public NelItem.CATEG category;

		public float value;

		public float value2;

		public float value3;

		public float max_grade_enpower = 3f;

		public float max_price_enpower = 3f;

		public ushort id;

		public int specific_icon_id = -1;

		public Color32 SpecificColor = MTRX.ColTrnsp;

		public const float icon_rect_w = 30f;

		public static NelItem Bottle;

		public static NelItem HolderBottle;

		public static NelItem NoelJuice;

		public static NelItem NoelMilk;

		public static NelItem LunchBox;

		public FnGetItemDetail FnGetName;

		public FnGetItemDetail FnGetDesc;

		public FnGetItemDetail FnGetDetail;

		public FnGetItemColor FnGetColor;

		public RecipeManager.RecipeItemInfo RecipeInfo;

		public const int IDTOP_REEL = 60000;

		public const int IDTOP_ENHANCER = 61000;

		public const int IDTOP_SKILL = 61200;

		public const int IDTOP_RECIPE = 61600;

		public const int IDTOP_TRM_ITEM = 62000;

		public const int IDTOP_MARKER = 62200;

		public const int ID_CACHEITEM = 65535;

		private byte obtain_count_;

		private static NelItem.ValMemo[][] AAvalFixGrade;

		private static NelItem.ValMemoS[] Buf;

		private static NelItem.ValMemo[] BufU;

		public static int cache_item_count = 0;

		public static NDic<NelItem> OData;

		public static BDic<string, ulong> Oser_cure;

		public static BDic<string, ulong> Oser_apply;

		private static Regex RegItemHeader = new Regex("^(\\@?)([a-zA-Z0-9_]+)[ \\t]+(\\d+)[ \\t]+(\\d+)[ \\t]+(\\d+)[ \\t]*\\{$");

		public static Regex RegItemBottleOld = new Regex("_bottle$");

		public static NelItem Unknown;

		public static string tx_item_get;

		public static ItemStorage.FnCheckItemDataAndInfo isRawFood = (ItemStorage.IRow IRow) => IRow.Data.is_food && !IRow.has_wlink;

		public static ItemStorage.FnCheckItemDataAndInfo isTreasureBox = (ItemStorage.IRow IRow) => IRow.Data.is_reelmbox;

		public interface IItemUser
		{
			void NelItemUseInt(NelItem Itm, NelItem.CATEG categ, int grade, int v_int, ref int ef_delay, ref string play_snd, ref bool quit_flag);

			void NelItemUseUint(NelItem Itm, NelItem.CATEG categ, int grade, ulong v_uint, ref int ef_delay, ref string play_snd, int level);
		}

		public enum CATEG : uint
		{
			OTHER,
			INDIVIDUAL_GRADE,
			CURE_HP = 16U,
			CURE_MP = 32U,
			CURE_EP = 64U,
			MTR = 128U,
			WATER = 4096U,
			CURE_MP_CRACK = 8192U,
			SER_APPLY = 16384U,
			SER_CURE = 32768U,
			FRUIT = 65536U,
			FOOD = 131072U,
			DUST = 262144U,
			ANC = 524288U,
			TOOL = 1048576U,
			SPECIAL = 2097152U,
			ENHANCER = 8388608U,
			SPECIAL_USE = 4194304U,
			BOMB = 16777216U,
			BOTTLE = 268435456U
		}

		private struct ValMemoS
		{
			public ValMemoS(float value, NelItem.GrdVariation _Gvr, bool __consider_haniwa)
			{
				this.v_origin = value;
				this.Gvr = _Gvr;
				this._consider_haniwa = __consider_haniwa;
			}

			public string detail
			{
				get
				{
					return this.Gvr.getDetail(this.v_origin, "〜", this._consider_haniwa);
				}
			}

			public int getMinValue()
			{
				return this.Gvr.getMinValue(this.v_origin);
			}

			public STB getDetailTo(STB Stb)
			{
				this.Gvr.getDetailTo(Stb, this.v_origin, "〜", this._consider_haniwa);
				return Stb;
			}

			public STB detailAppendTxA(STB Stb, string txa_key, string delimiter = "\n")
			{
				Stb.AppendTxA(txa_key, delimiter);
				STB stb = TX.PopBld(null, 0);
				Stb.TxRpl(this.getDetailTo(stb));
				TX.ReleaseBld(stb);
				return Stb;
			}

			public float v_origin;

			public NelItem.GrdVariation Gvr;

			public bool _consider_haniwa;
		}

		private struct ValMemo
		{
			public ValMemo(float v_org, float mul)
			{
				this.v_origin = v_org;
				this.v = v_org * mul + 0.0001f;
			}

			public int v_int
			{
				get
				{
					return (int)this.v;
				}
			}

			public ulong v_org_ulong
			{
				get
				{
					return (ulong)this.v_origin;
				}
			}

			private float v;

			public float v_origin;
		}

		public struct GrdVariation
		{
			public GrdVariation(float val)
			{
				this.max = val;
				this.min = val;
				this.one_grade = true;
				this.useable = true;
			}

			public GrdVariation(float _min, float _max)
			{
				this.min = _min;
				this.max = _max;
				this.one_grade = false;
				this.useable = true;
			}

			public string getDetail(float val, string delimiter = "〜", bool consider_haniwa = false)
			{
				STB stb = TX.PopBld(null, 0);
				this.getDetailTo(stb, val, delimiter, consider_haniwa);
				string text = stb.ToString();
				TX.ReleaseBld(stb);
				return text;
			}

			public int getMinValue(float val)
			{
				return (int)(val * this.min * 1f + 0.0001f);
			}

			public STB getDetailTo(STB Stb, float val, string delimiter = "〜", bool consider_haniwa = false)
			{
				if (!this.useable)
				{
					Stb.Add(val);
				}
				else if (this.one_grade)
				{
					Stb.Add((int)(val * this.min * 1f + 0.0001f));
				}
				else
				{
					Stb.Add((int)(val * this.min * 1f + 0.0001f)).Add(delimiter).Add((int)(val * this.max * 1f + 0.0001f));
				}
				return Stb;
			}

			public float min;

			public float max;

			public bool useable;

			public bool one_grade;
		}
	}
}
