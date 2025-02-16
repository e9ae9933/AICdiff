using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public static class EnhancerManager
	{
		public static void initImage()
		{
			if (EnhancerManager.SqImgSlot != null)
			{
				return;
			}
			EnhancerManager.SqImgSlot = MTRX.PxlIcon.getPoseByName("nel_enhancer_slot").getSequence(0);
			EnhancerManager.SlotDr = new SlotDrawer(EnhancerManager.SqImgSlot.getFrame(0), 30, 0);
			EnhancerManager.SlotDr.ReturnIntv(8, 6, -14);
			EnhancerManager.PFFill = MTRX.getPF("enhancer_cost");
			EnhancerManager.PFZero = MTRX.getPF("enhancer_cost_zero");
			EnhancerManager.PtcAct = new EfParticleOnce("ui_enhancer_act", EFCON_TYPE.UI);
		}

		public static void initScript()
		{
			EnhancerManager.initImage();
			EnhancerManager.SqImgIcon = MTRX.PxlIcon.getPoseByName("enhancer_icon").getSequence(0);
			EnhancerManager.AEh = new List<EnhancerManager.Enhancer>();
			CsvReaderA csvReaderA = new CsvReaderA(TX.getResource("Data/enhancer", ".csv", false), true);
			EnhancerManager.Enhancer enhancer = null;
			NelItem nelItem = null;
			int num = 61000;
			while (csvReaderA.read())
			{
				if (csvReaderA.cmd == "/*" || csvReaderA.cmd == "/*___")
				{
					if (enhancer != null && EnhancerManager.Get(csvReaderA.cmd) != null)
					{
						X.de("重複key: (key: " + enhancer.key + ")", null);
					}
					string index = csvReaderA.getIndex((csvReaderA.cmd == "/*") ? 2 : 1);
					enhancer = EnhancerManager.Get(index);
					nelItem = null;
					if (enhancer != null)
					{
						X.de("重複キー: " + index, null);
						enhancer = null;
					}
					else
					{
						enhancer = new EnhancerManager.Enhancer(index, EnhancerManager.SqImgIcon.getFrameByName(index));
						nelItem = NelItem.CreateItemEntry("Enhancer_" + index, new NelItem("Enhancer_" + index, 0, 600, 1)
						{
							category = (NelItem.CATEG)10485761U,
							FnGetName = new FnGetItemDetail(NelItem.fnGetNameEnhancer),
							FnGetDesc = new FnGetItemDetail(NelItem.fnGetDescEnhancer),
							FnGetDetail = new FnGetItemDetail(NelItem.fnGetDetailEnhancer)
						}, num++, false);
						nelItem.value = (float)enhancer.cost;
						EnhancerManager.AEh.Add(enhancer);
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
				EnhancerManager.Validate();
			}
		}

		public static void newGame()
		{
			EnhancerManager.max_slot = (EnhancerManager.using_slot = 0);
			EnhancerManager.enhancer_bits = (EnhancerManager.EH)0U;
		}

		public static EnhancerManager.Enhancer Get(string key)
		{
			for (int i = EnhancerManager.AEh.Count - 1; i >= 0; i--)
			{
				EnhancerManager.Enhancer enhancer = EnhancerManager.AEh[i];
				if (enhancer.key == key)
				{
					return enhancer;
				}
			}
			return null;
		}

		public static EnhancerManager.Enhancer Get(NelItem Itm)
		{
			if (Itm != null)
			{
				return EnhancerManager.Get(TX.slice(Itm.key, "Enhancer_".Length));
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
			EnhancerManager.max_slot = StPrecious.getCount(byId, -1);
			EnhancerManager.using_slot = 0;
			EnhancerManager.enhancer_bits = (EnhancerManager.EH)0U;
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in StEnhancer.getWholeInfoDictionary())
			{
				int top_grade = keyValuePair.Value.top_grade;
				if ((top_grade & 2) == 2)
				{
					EnhancerManager.Enhancer enhancer = EnhancerManager.Get(keyValuePair.Key);
					if (enhancer != null && EnhancerManager.using_slot + enhancer.cost <= EnhancerManager.max_slot)
					{
						EnhancerManager.using_slot += enhancer.cost;
						EnhancerManager.enhancer_bits |= enhancer.ehbit;
					}
					else
					{
						keyValuePair.Value.changeGradeForPrecious(top_grade & 1, 1);
					}
				}
			}
			EnhancerManager.SlotDr.clearFill().Fill(EnhancerManager.SqImgSlot.getFrame(1), 0, EnhancerManager.using_slot);
		}

		public static bool UiPrepareTbArea(UiBoxDesigner Ds, bool is_top, Func<EnhancerManager.Enhancer, bool> _FnEnhancerActivatable)
		{
			EnhancerManager.FnEnhancerActivatable = _FnEnhancerActivatable;
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
					FnDraw = new MeshDrawer.FnGeneralDraw(EnhancerManager.drawEnhancerSlot)
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
					text = EnhancerManager.getCostTotalString(),
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
					FnDraw = new MeshDrawer.FnGeneralDraw(EnhancerManager.drawEnhancerCost)
				});
			}
			return true;
		}

		private static bool drawEnhancerSlot(MeshDrawer Md, float alpha)
		{
			float swidthByCount = EnhancerManager.SlotDr.getSWidthByCount(24);
			float sheightByCount = EnhancerManager.SlotDr.getSHeightByCount(24);
			EnhancerManager.SlotDr.drawTo(Md, 0f, 0f, 2f, 2f, EnhancerManager.max_slot, swidthByCount, sheightByCount);
			bool flag = true;
			if (EnhancerManager.slot_act_t > 0)
			{
				int num = IN.totalframe - EnhancerManager.slot_act_t;
				if (num < 40)
				{
					flag = false;
					X.ZLINE((float)num, 40f);
					int num2 = EnhancerManager.slot_act_s;
					Md.base_z = -0.125f;
					Md.chooseSubMesh(1, false, false);
					for (int i = 0; i < EnhancerManager.slot_act_cnt; i++)
					{
						Vector2 pos = EnhancerManager.SlotDr.getPos(num2++, 2f, 2f, swidthByCount, sheightByCount);
						EnhancerManager.PtcAct.drawTo(Md, pos.x, pos.y, alpha, 40, (float)num, 0f);
						EnhancerManager.PtcAct.f0 = EnhancerManager.slot_act_t + i * 13;
					}
					Md.base_z = 0f;
					Md.chooseSubMesh(0, false, false);
				}
				else
				{
					EnhancerManager.slot_act_t = 0;
				}
			}
			Md.updateForMeshRenderer(false);
			return flag;
		}

		private static bool drawEnhancerCost(MeshDrawer Md, float alpha)
		{
			if (EnhancerManager.Selected == null)
			{
				Md.clear(false, false);
				return false;
			}
			bool flag = true;
			Md.Col = C32.MulA(MTRX.ColWhite, alpha);
			float num = 0f;
			float num2 = 0f;
			if (EnhancerManager.error_t > 0)
			{
				float num3;
				float num4;
				if (NEL.isErrorVib((float)(IN.totalframe - EnhancerManager.error_t), out num3, out num4, 14.4f))
				{
					flag = false;
					Md.Col = Md.ColGrd.Set(MTRX.ColWhite).blend(4294901760U, num3).mulA(alpha)
						.C;
					num = num4;
				}
				else
				{
					EnhancerManager.error_t = 0;
				}
			}
			int num5 = X.Mx(EnhancerManager.Selected.cost, 1);
			PxlFrame pxlFrame = ((EnhancerManager.Selected.cost == 0) ? EnhancerManager.PFZero : EnhancerManager.PFFill);
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
			EnhancerManager.Selected = EnhancerManager.Get(text);
			EnhancerManager.error_t = 0;
			if (EnhancerManager.Selected == null)
			{
				fillBlock.text_content = "!! No Enhancer: " + text;
			}
			else
			{
				fillBlock.text_content = EnhancerManager.Selected.descript;
			}
			fillBlock2.redraw_flag = true;
		}

		public static bool attachEnhancer(NelItem Itm, ItemStorage.ObtainInfo Obt, Designer BtmDs, Designer TopDs, ItemStorage StEnhancer, aBtnItemRow Row)
		{
			EnhancerManager.Enhancer enhancer = EnhancerManager.Get(Row.getItemData());
			if (EnhancerManager.Selected == null || enhancer != EnhancerManager.Selected)
			{
				return false;
			}
			if (EnhancerManager.FnEnhancerActivatable != null && !EnhancerManager.FnEnhancerActivatable(EnhancerManager.Selected))
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
				int num = X.Mn(EnhancerManager.using_slot, EnhancerManager.Selected.cost);
				EnhancerManager.using_slot -= num;
				EnhancerManager.SlotDr.Fill(null, EnhancerManager.using_slot, num);
				Obt.changeGradeForPrecious(top_grade & 1, 1);
				EnhancerManager.enhancer_bits &= ~EnhancerManager.Selected.ehbit;
				SND.Ui.play("reset_var", false);
			}
			else
			{
				if (EnhancerManager.using_slot + EnhancerManager.Selected.cost > EnhancerManager.max_slot)
				{
					SND.Ui.play("locked", false);
					EnhancerManager.error_t = IN.totalframe;
					(BtmDs.Get("cost", false) as FillImageBlock).redraw_flag = true;
					return false;
				}
				if (EnhancerManager.Selected.cost > 0)
				{
					EnhancerManager.SlotDr.Fill(EnhancerManager.SqImgSlot.getFrame(1), EnhancerManager.using_slot, EnhancerManager.Selected.cost);
					EnhancerManager.slot_act_s = EnhancerManager.using_slot;
					EnhancerManager.slot_act_cnt = EnhancerManager.Selected.cost;
					EnhancerManager.slot_act_t = IN.totalframe;
					EnhancerManager.using_slot += EnhancerManager.Selected.cost;
				}
				EnhancerManager.enhancer_bits |= EnhancerManager.Selected.ehbit;
				Obt.changeGradeForPrecious((top_grade & 1) | 2, 1);
				SND.Ui.play("enhancer_act", false);
			}
			buttonSkinEnhancerRow.fineEquip();
			(TopDs.Get("cost", false) as FillImageBlock).redraw_flag = true;
			TopDs.Get("cost_total", false).setValue(EnhancerManager.getCostTotalString());
			return true;
		}

		public static string fnDescAddition(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			if (row == UiItemManageBox.DESC_ROW.ROW_COUNT)
			{
				EnhancerManager.Enhancer enhancer = EnhancerManager.Get(Itm);
				if (enhancer != null)
				{
					return EnhancerManager.getCostString(enhancer.cost, "");
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
			if (IN.isUiAddPD() && selectedRow != null && EnhancerManager.Get(selectedRow.getItemData()) != null)
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
			return EnhancerManager.using_slot.ToString() + "/" + EnhancerManager.max_slot.ToString();
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

		private static List<EnhancerManager.Enhancer> AEh;

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

		public static EnhancerManager.EH enhancer_bits;

		private static EfParticleOnce PtcAct;

		public const int COST_CLMN_W = 70;

		public const int COST_TOTAL_TX_W = 90;

		public static int gotten;

		public const string one_icon_tag = "<img mesh=\"enhancer_cost\" width=\"17\" height=\"16\" scale=\"2\" />";

		public const string zero_icon_tag = "<img mesh=\"enhancer_cost_zero\" width=\"17\" height=\"16\" scale=\"2\" />";

		private static Func<EnhancerManager.Enhancer, bool> FnEnhancerActivatable;

		private static EnhancerManager.Enhancer Selected;

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
				FEnum<EnhancerManager.EH>.TryParse(_key, out this.ehbit, true);
			}

			public bool validate()
			{
				bool flag = true;
				if (TX.getTX("Enhancer_title_" + this.key, true, false, null) == null)
				{
					X.de("EnhancerManager: テキスト Enhancer_title_" + this.key + " がありません", null);
					flag = false;
				}
				if (TX.getTX("Enhancer_desc_" + this.key, true, false, null) == null)
				{
					X.de("EnhancerManager: テキスト Enhancer_desc_" + this.key + " がありません", null);
					flag = false;
				}
				if (this.PF == null)
				{
					X.de("EnhancerManager: nel_enhancer_slot ポーズ内にフレーム " + this.key + " がありません", null);
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

			public EnhancerManager.EH ehbit;
		}
	}
}
