using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public static class ENHA
	{
		public static void initImage()
		{
			if (ENHA.SqImgSlot != null)
			{
				return;
			}
			ENHA.SqImgSlot = MTRX.PxlIcon.getPoseByName("nel_enhancer_slot").getSequence(0);
			ENHA.SlotDr = new SlotDrawer(ENHA.SqImgSlot.getFrame(0), 30, 0);
			ENHA.SlotDr.ReturnIntv(8, 6, -14);
			ENHA.PFFill = MTRX.getPF("enhancer_cost");
			ENHA.PFZero = MTRX.getPF("enhancer_cost_zero");
			ENHA.PtcAct = new EfParticleOnce("ui_enhancer_act", EFCON_TYPE.FIXED);
		}

		public static void initScript()
		{
			ENHA.initImage();
			ENHA.SqImgIcon = MTRX.PxlIcon.getPoseByName("enhancer_icon").getSequence(0);
			ENHA.AEh = new List<ENHA.Enhancer>();
			CsvReaderA csvReaderA = new CsvReaderA(TX.getResource("Data/enhancer", ".csv", false), true);
			ENHA.Enhancer enhancer = null;
			NelItem nelItem = null;
			int num = 61000;
			while (csvReaderA.read())
			{
				if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
				{
					if (enhancer != null && ENHA.Get(csvReaderA.cmd) != null)
					{
						X.de("重複key: (key: " + enhancer.key + ")", null);
					}
					string index = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1);
					enhancer = ENHA.Get(index);
					nelItem = null;
					if (enhancer != null)
					{
						X.de("重複キー: " + index, null);
						enhancer = null;
					}
					else
					{
						enhancer = new ENHA.Enhancer(index, ENHA.SqImgIcon.getFrameByName(index));
						nelItem = NelItem.CreateItemEntry("Enhancer_" + index, new NelItem("Enhancer_" + index, 0, 600, 1)
						{
							category = (NelItem.CATEG)10485761U,
							FnGetName = NelItem.fnGetNameEnhancer,
							FnGetDesc = NelItem.fnGetDescEnhancer,
							FnGetDetail = NelItem.fnGetDetailEnhancer
						}, num++, false);
						nelItem.value = (float)enhancer.cost;
						ENHA.AEh.Add(enhancer);
					}
				}
				if (enhancer != null)
				{
					string cmd = csvReaderA.cmd;
					if (cmd != null)
					{
						if (!(cmd == "cost"))
						{
							if (cmd == "price")
							{
								nelItem.price = csvReaderA.Int(1, 0);
							}
						}
						else
						{
							nelItem.value = (float)(enhancer.cost = csvReaderA.Int(1, 1));
						}
					}
				}
			}
			if (SkillManager.VALIDATION)
			{
				ENHA.Validate();
			}
		}

		public static void newGame()
		{
			ENHA.max_slot = (ENHA.using_slot = 0);
			ENHA.enhancer_bits = (ENHA.EH)0U;
		}

		public static ENHA.Enhancer Get(string key)
		{
			for (int i = ENHA.AEh.Count - 1; i >= 0; i--)
			{
				ENHA.Enhancer enhancer = ENHA.AEh[i];
				if (enhancer.key == key)
				{
					return enhancer;
				}
			}
			return null;
		}

		public static ENHA.Enhancer Get(NelItem Itm)
		{
			if (Itm != null)
			{
				return ENHA.Get(TX.slice(Itm.key, "Enhancer_".Length));
			}
			return null;
		}

		public static void Validate()
		{
		}

		public static void prepareDebug(ItemStorage StPrecious, ItemStorage StEnhancer, bool fine_flag = false)
		{
		}

		public static void fineEnhancerStorage(ItemStorage StPrecious, ItemStorage StEnhancer)
		{
			NelItem byId = NelItem.GetById("enhancer_slot", false);
			ENHA.max_slot = StPrecious.getCount(byId, -1);
			ENHA.using_slot = 0;
			ENHA.enhancer_bits = (ENHA.EH)0U;
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in StEnhancer.getWholeInfoDictionary())
			{
				int top_grade = keyValuePair.Value.top_grade;
				if ((top_grade & 2) == 2)
				{
					ENHA.Enhancer enhancer = ENHA.Get(keyValuePair.Key);
					if (enhancer != null && ENHA.using_slot + enhancer.cost <= ENHA.max_slot)
					{
						ENHA.using_slot += enhancer.cost;
						ENHA.enhancer_bits |= enhancer.ehbit;
					}
					else
					{
						keyValuePair.Value.changeGradeForPrecious(top_grade & 1, 1);
					}
				}
			}
			ENHA.SlotDr.clearFill().Fill(ENHA.SqImgSlot.getFrame(1), 0, ENHA.using_slot);
		}

		public static bool UiPrepareTbArea(UiBoxDesigner Ds, bool is_top, Func<ENHA.Enhancer, bool> _FnEnhancerActivatable)
		{
			ENHA.FnEnhancerActivatable = _FnEnhancerActivatable;
			Ds.Clear();
			Ds.item_margin_x_px = 0f;
			Ds.init();
			float use_h = Ds.use_h;
			if (is_top)
			{
				FillImageBlock fillImageBlock = Ds.addImg(new DsnDataImg
				{
					name = "cost",
					MI = MTRX.MIicon,
					swidth = Ds.use_w - 90f - 2f,
					sheight = use_h,
					FnDraw = new MeshDrawer.FnGeneralDraw(ENHA.drawEnhancerSlot)
				});
				MeshDrawer meshDrawer = fillImageBlock.getMeshDrawer();
				meshDrawer.chooseSubMesh(1, false, false);
				meshDrawer.setMaterial(MTRX.MtrMeshNormal, false);
				meshDrawer.chooseSubMesh(0, false, false);
				meshDrawer.connectRendererToTriMulti(fillImageBlock.getMeshRenderer());
				Ds.addP(new DsnDataP("", false)
				{
					TxCol = C32.d2c(4283780170U),
					name = "cost_total",
					text = ENHA.getCostTotalString(),
					html = true,
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.MIDDLE,
					swidth = 90f,
					sheight = use_h,
					size = 16f
				}, false);
			}
			else
			{
				Ds.addP(new DsnDataP("", false)
				{
					TxCol = C32.d2c(4283780170U),
					name = "desc",
					text = "\u3000",
					html = true,
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.MIDDLE,
					swidth = Ds.use_w - 2f - 70f,
					sheight = use_h,
					size = 18f
				}, false);
				Ds.addImg(new DsnDataImg
				{
					MI = MTRX.MIicon,
					name = "cost",
					swidth = 70f,
					sheight = use_h,
					FnDraw = new MeshDrawer.FnGeneralDraw(ENHA.drawEnhancerCost)
				});
			}
			return true;
		}

		private static bool drawEnhancerSlot(MeshDrawer Md, float alpha)
		{
			float swidthByCount = ENHA.SlotDr.getSWidthByCount(24);
			float sheightByCount = ENHA.SlotDr.getSHeightByCount(24);
			ENHA.SlotDr.drawTo(Md, 0f, 0f, 2f, 2f, ENHA.max_slot, swidthByCount, sheightByCount);
			bool flag = true;
			if (ENHA.slot_act_t > 0)
			{
				int num = IN.totalframe - ENHA.slot_act_t;
				if (num < 40)
				{
					flag = false;
					X.ZLINE((float)num, 40f);
					int num2 = ENHA.slot_act_s;
					Md.base_z = -0.125f;
					Md.chooseSubMesh(1, false, false);
					for (int i = 0; i < ENHA.slot_act_cnt; i++)
					{
						Vector2 pos = ENHA.SlotDr.getPos(num2++, 2f, 2f, swidthByCount, sheightByCount);
						ENHA.PtcAct.drawTo(Md, pos.x, pos.y, alpha, 40, (float)num, 0f);
						ENHA.PtcAct.f0 = ENHA.slot_act_t + i * 13;
					}
					Md.base_z = 0f;
					Md.chooseSubMesh(0, false, false);
				}
				else
				{
					ENHA.slot_act_t = 0;
				}
			}
			Md.updateForMeshRenderer(false);
			return flag;
		}

		private static bool drawEnhancerCost(MeshDrawer Md, float alpha)
		{
			if (ENHA.Selected == null)
			{
				Md.clear(false, false);
				return false;
			}
			bool flag = true;
			Md.Col = C32.MulA(MTRX.ColWhite, alpha);
			float num = 0f;
			float num2 = 0f;
			if (ENHA.error_t > 0)
			{
				float num3;
				float num4;
				if (NEL.isErrorVib((float)(IN.totalframe - ENHA.error_t), out num3, out num4, 14.4f))
				{
					flag = false;
					Md.Col = Md.ColGrd.Set(MTRX.ColWhite).blend(4294901760U, num3).mulA(alpha)
						.C;
					num = num4;
				}
				else
				{
					ENHA.error_t = 0;
				}
			}
			int num5 = X.Mx(ENHA.Selected.cost, 1);
			PxlFrame pxlFrame = ((ENHA.Selected.cost == 0) ? ENHA.PFZero : ENHA.PFFill);
			float num6 = X.NIL(60f, 80f, (float)(num5 - 1) / 3f, 1f);
			for (int i = 0; i < num5; i++)
			{
				float num7 = -0.5f + (0.5f + (float)i) / (float)num5;
				Md.RotaPF(num, num2 + num6 * num7, 2f, 2f, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
			}
			Md.updateForMeshRenderer(false);
			return flag;
		}

		public static void showEnhancerDesc(NelItem Itm, ItemStorage.ObtainInfo Obt, Designer BtmDs)
		{
			FillBlock fillBlock = BtmDs.Get("desc", false) as FillBlock;
			FillBlock fillBlock2 = BtmDs.Get("cost", false) as FillImageBlock;
			string text = TX.slice(Itm.key, "Enhancer_".Length);
			ENHA.Selected = ENHA.Get(text);
			ENHA.error_t = 0;
			if (ENHA.Selected == null)
			{
				fillBlock.text_content = "!! No Enhancer: " + text;
			}
			else
			{
				fillBlock.text_content = ENHA.Selected.descript;
			}
			fillBlock2.redraw_flag = true;
		}

		public static bool attachEnhancer(NelItem Itm, ItemStorage.ObtainInfo Obt, Designer BtmDs, Designer TopDs, ItemStorage StEnhancer, aBtnItemRow Row)
		{
			ENHA.Enhancer enhancer = ENHA.Get(Row.getItemData());
			if (ENHA.Selected == null || enhancer != ENHA.Selected)
			{
				return false;
			}
			if (ENHA.FnEnhancerActivatable != null && !ENHA.FnEnhancerActivatable(ENHA.Selected))
			{
				return false;
			}
			if (EnemySummoner.isActiveBorder())
			{
				BtmDs.Get("desc", false).setValue(NEL.error_tag + TX.Get("Enhancer_cannot_change", "") + NEL.error_tag_close);
				SND.Ui.play("locked", false);
				return false;
			}
			ButtonSkinEnhancerRow buttonSkinEnhancerRow = Row.get_Skin() as ButtonSkinEnhancerRow;
			int top_grade = Obt.top_grade;
			if ((top_grade & 2) == 2)
			{
				int num = X.Mn(ENHA.using_slot, ENHA.Selected.cost);
				ENHA.using_slot -= num;
				ENHA.SlotDr.Fill(null, ENHA.using_slot, num);
				Obt.changeGradeForPrecious(top_grade & 1, 1);
				ENHA.enhancer_bits &= ~ENHA.Selected.ehbit;
				SND.Ui.play("reset_var", false);
			}
			else
			{
				if (ENHA.using_slot + ENHA.Selected.cost > ENHA.max_slot)
				{
					SND.Ui.play("locked", false);
					ENHA.error_t = IN.totalframe;
					(BtmDs.Get("cost", false) as FillImageBlock).redraw_flag = true;
					return false;
				}
				if (ENHA.Selected.cost > 0)
				{
					ENHA.SlotDr.Fill(ENHA.SqImgSlot.getFrame(1), ENHA.using_slot, ENHA.Selected.cost);
					ENHA.slot_act_s = ENHA.using_slot;
					ENHA.slot_act_cnt = ENHA.Selected.cost;
					ENHA.slot_act_t = IN.totalframe;
					ENHA.using_slot += ENHA.Selected.cost;
				}
				ENHA.enhancer_bits |= ENHA.Selected.ehbit;
				Obt.changeGradeForPrecious((top_grade & 1) | 2, 1);
				SND.Ui.play("enhancer_act", false);
			}
			buttonSkinEnhancerRow.fineEquip();
			(TopDs.Get("cost", false) as FillImageBlock).redraw_flag = true;
			TopDs.Get("cost_total", false).setValue(ENHA.getCostTotalString());
			return true;
		}

		public static string fnDescAddition(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			if (row == UiItemManageBox.DESC_ROW.ROW_COUNT)
			{
				ENHA.Enhancer enhancer = ENHA.Get(Itm);
				if (enhancer != null)
				{
					return ENHA.getCostString(enhancer.cost, "");
				}
			}
			if (row == UiItemManageBox.DESC_ROW.TOPRIGHT_COUNTER)
			{
				return TX.Get("Enhancer_desc_fav", "");
			}
			return def_string;
		}

		public static void fnItemRowCreate(aBtnItemRow B, ItemStorage.IRow IRow)
		{
			if (EnemySummoner.isActiveBorder())
			{
				B.locked_click = true;
				B.SetLocked(true, true, false);
			}
		}

		public static void runEnhancerUi(UiItemManageBox ItemMng)
		{
			aBtnItemRow selectedRow = ItemMng.Inventory.SelectedRow;
			if (IN.isUiAddPD() && selectedRow != null && ENHA.Get(selectedRow.getItemData()) != null)
			{
				ItemStorage.ObtainInfo itemInfo = selectedRow.getItemInfo();
				int num = itemInfo.top_grade;
				if ((num & 1) == 0)
				{
					SND.Ui.play("tool_drag_init", false);
					num = (num & 2) | 1;
				}
				else
				{
					num &= 2;
					SND.Ui.play("tool_drag_quit", false);
				}
				itemInfo.changeGradeForPrecious(num, 1);
				(selectedRow.get_Skin() as ButtonSkinEnhancerRow).fineEquip();
			}
		}

		public static string getCostString(int c, string delimiter = "")
		{
			string text = "";
			if (c == 0)
			{
				return "<img mesh=\"enhancer_cost_zero\" width=\"17\" height=\"16\" scale=\"2\" />";
			}
			while (--c >= 0)
			{
				text = text + ((text == "") ? "" : delimiter) + "<img mesh=\"enhancer_cost\" width=\"17\" height=\"16\" scale=\"2\" />";
			}
			return text;
		}

		public static string getCostTotalString()
		{
			return ENHA.using_slot.ToString() + "/" + ENHA.max_slot.ToString();
		}

		public const float magic_neutrize_max = 0.875f;

		public const float atk_add_level_normal = 0.75f;

		public const float atk_add_level_magic = 1f;

		public const float atk_add_overspell_max = 1.5f;

		public const float atk_overspell_adding_level = 0.66f;

		public const float atk_add_split_mp = 1f;

		public const float shield_power_reduce_min = 0.25f;

		public const float shield_damage_reduce_min = 0.8f;

		public const float shield_damage_reduce_max = 0.1f;

		public const float evade_nodamage_extend_max = 0.875f;

		public const float evade_on_clt_break_scr_level = 0.5f;

		public const float arrest_hpdamage_min = 0.25f;

		public const float arrest_hpdamage_max = 2f;

		public const float arrest_mpdamage_min = 0.25f;

		public const float arrest_mpdamage_max = 2f;

		public const float smoke_resist_min = 0.1f;

		public const float arrest_escape_speed_max = 3f;

		public const float arrest_escape_speed_min = 0.1f;

		public const float arrest_escape_speed_ni_max = 0.5f;

		public const float attr_hpdamage_min = 0.25f;

		public const float attr_hpdamage_max = 2f;

		public const float attr_bomb_selffire_re_multiple = 1.4f;

		public const float attr_bomb_selffire_ratio_multiple_minus = 1.5f;

		public const float ser_speed_min = 0.0625f;

		public const float ser_speed_max = 2f;

		public const float chant_speed_overhold_min = 2.66f;

		public const float chant_speed_max = 2f;

		public const float reel_speed_min = 0.35f;

		public const float anchor_adding_level = 0.44f;

		public const float sink_reduce_margin_maxt = 90f;

		public const float ser_resist_reduce_ratio_min = 0.25f;

		public const float lost_mp_when_chanting_min = 0f;

		public const float mp_gage_resist_min = 0f;

		public const float singletask_adding_level = 0.92f;

		public const float juice_server_scr = 0.15f;

		public const float juice_server_lovejuice_scr = 0.25f;

		public const float juice_server_unlock_scr = 0.15f;

		public const float egg_reduce_higher_max = 2.5f;

		public const float dropitem_higher_max = 5f;

		public const float dropitem_higher_max_ratio = 0.66f;

		private static List<ENHA.Enhancer> AEh;

		private const string enhancer_script = "Data/enhancer";

		public const string enhancer_item_header = "Enhancer_";

		public static PxlSequence SqImgIcon;

		public static PxlFrame PFFill;

		public static PxlFrame PFZero;

		public const int DEBUG_ENHANCER_COUNT = 5;

		private static SlotDrawer SlotDr;

		private static int max_slot;

		private static int using_slot;

		public static PxlSequence SqImgSlot;

		public static ENHA.EH enhancer_bits;

		private static EfParticleOnce PtcAct;

		public const int COST_CLMN_W = 70;

		public const int COST_TOTAL_TX_W = 90;

		public static int gotten;

		public const string one_icon_tag = "<img mesh=\"enhancer_cost\" width=\"17\" height=\"16\" scale=\"2\" />";

		public const string zero_icon_tag = "<img mesh=\"enhancer_cost_zero\" width=\"17\" height=\"16\" scale=\"2\" />";

		private static Func<ENHA.Enhancer, bool> FnEnhancerActivatable;

		private static ENHA.Enhancer Selected;

		private static int error_t;

		private static int slot_act_t;

		private static int slot_act_s;

		private static int slot_act_cnt;

		public enum EH : uint
		{
			hp_eye = 1U,
			cliff_stopper,
			overspell = 4U,
			anchor = 8U,
			long_reach = 16U,
			secure_absorb = 32U,
			soul_eater = 64U,
			falling_cat = 128U,
			double_evade = 256U,
			sway_sliding = 512U,
			raincaller = 1024U,
			shield_cat = 2048U,
			singletask = 4096U,
			juice_server = 8192U,
			_MAX
		}

		public class Enhancer
		{
			public Enhancer(string _key, PxlFrame F)
			{
				this.key = _key;
				if (F != null)
				{
					this.PF = F;
				}
				FEnum<ENHA.EH>.TryParse(_key, out this.ehbit, true);
			}

			public bool validate()
			{
				bool flag = true;
				if (TX.getTX("Enhancer_title_" + this.key, true, false, null) == null)
				{
					X.de("ENHA: テキスト Enhancer_title_" + this.key + " がありません", null);
					flag = false;
				}
				if (TX.getTX("Enhancer_desc_" + this.key, true, false, null) == null)
				{
					X.de("ENHA: テキスト Enhancer_desc_" + this.key + " がありません", null);
					flag = false;
				}
				if (this.PF == null)
				{
					X.de("ENHA: nel_enhancer_slot ポーズ内にフレーム " + this.key + " がありません", null);
					flag = false;
				}
				return flag;
			}

			public string title
			{
				get
				{
					return TX.Get("Enhancer_title_" + this.key, "");
				}
			}

			public string descript
			{
				get
				{
					return TX.Get("Enhancer_desc_" + this.key, "");
				}
			}

			public readonly string key;

			public PxlFrame PF;

			public int cost = 1;

			public ENHA.EH ehbit;
		}
	}
}
