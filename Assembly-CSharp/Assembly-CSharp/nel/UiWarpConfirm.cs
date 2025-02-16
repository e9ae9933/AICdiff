using System;
using System.Collections.Generic;
using evt;
using m2d;
using nel.gm;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class UiWarpConfirm : UiBoxDesignerFamily, IEventWaitListener
	{
		public bool isAlreadyChecked(UiWarpConfirm.CTYPE type)
		{
			return (this.already_checked & (1U << (int)type)) > 0U;
		}

		public static bool isAlreadyChecked(UiWarpConfirm ContinueChecking, UiWarpConfirm.CTYPE type)
		{
			return ContinueChecking != null && ContinueChecking.isAlreadyChecked(type);
		}

		public static bool need_recheck_ctype(UiWarpConfirm.CTYPE type)
		{
			return type == UiWarpConfirm.CTYPE.FOODLOST || type == UiWarpConfirm.CTYPE.GOHOME_GQ;
		}

		public static UiWarpConfirm.CTYPE checkUseConfirm(WholeMapItem CurWM, WholeMapItem NextWM, out object Target, UiWarpConfirm ContinueChecking = null)
		{
			Target = null;
			ContinueChecking == null;
			if (CurWM == NextWM)
			{
				return UiWarpConfirm.CTYPE.OFFLINE;
			}
			if (CurWM == null || NextWM == null)
			{
				X.de("checkUseConfirm:: WholeMapItem 取得エラー", null);
				return UiWarpConfirm.CTYPE.OFFLINE;
			}
			UiWarpConfirm.M2D = M2DBase.Instance as NelM2DBase;
			if (CurWM.safe_area == NextWM.safe_area)
			{
				return UiWarpConfirm.CTYPE.OFFLINE;
			}
			if (NextWM.safe_area)
			{
				Vector2Int vector2Int;
				if (!UiWarpConfirm.isAlreadyChecked(ContinueChecking, UiWarpConfirm.CTYPE.GOHOME_GQ) && UiWarpConfirm.M2D.GUILD.checkDepertTargetNotFinished(CurWM, out vector2Int, true))
				{
					Target = vector2Int;
					return UiWarpConfirm.CTYPE.GOHOME_GQ;
				}
				if (!UiWarpConfirm.isAlreadyChecked(ContinueChecking, UiWarpConfirm.CTYPE.TREASUREBOX))
				{
					List<ItemStorage.IRow> list = new List<ItemStorage.IRow>();
					UiWarpConfirm.M2D.IMNG.getInventory().getItemCountFn((ItemStorage.IRow IRow) => IRow.Data.is_reelmbox, list);
					if (list.Count != 0)
					{
						Target = list;
						return UiWarpConfirm.CTYPE.TREASUREBOX;
					}
				}
			}
			else
			{
				WholeMapItem wholeMapItem;
				if (!NextWM.safe_area && !UiWarpConfirm.isAlreadyChecked(ContinueChecking, UiWarpConfirm.CTYPE.GOOUT_GQ) && UiWarpConfirm.M2D.GUILD.hasAnotherDepertTarget(NextWM, out wholeMapItem))
				{
					Target = wholeMapItem;
					return UiWarpConfirm.CTYPE.GOOUT_GQ;
				}
				if (!UiWarpConfirm.isAlreadyChecked(ContinueChecking, UiWarpConfirm.CTYPE.FOODLOST))
				{
					if (UiWarpConfirm.AFoodRow == null)
					{
						UiWarpConfirm.AFoodRow = new List<ItemStorage.IRow>(4);
					}
					else
					{
						UiWarpConfirm.AFoodRow.Clear();
					}
					if (UiWarpConfirm.M2D.IMNG.getInventory().getItemCountFn(NelItem.isRawFood, UiWarpConfirm.AFoodRow) > 0)
					{
						return UiWarpConfirm.CTYPE.FOODLOST;
					}
				}
			}
			return UiWarpConfirm.CTYPE.OFFLINE;
		}

		private float btn_w
		{
			get
			{
				return (this.main_w - 100f) / 2f;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			UiWarpConfirm.M2D = M2DBase.Instance as NelM2DBase;
			UiWarpConfirm.M2D.FlagValotStabilize.Add("WARPCONFIRM");
			if (UiWarpConfirm.M2D.GM.isActive())
			{
				UIBase.FlgUiEffectDisable.Add("WARPCONFIRM");
			}
			IN.setZ(base.transform, -4.9f);
			this.auto_deactive_gameobject = false;
			base.gameObject.layer = IN.gui_layer;
			this.BxC = base.Create("main_cmd", 0f, 0f, this.main_w, this.main_h, 3, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxC.anim_time(22);
			this.BxC.use_scroll = false;
			this.BxC.Focusable(false, false, null);
			this.BxC.getBox().frametype = UiBox.FRAMETYPE.MAIN;
			this.BxC.animate_maxt = 0;
			this.deactivate(true);
		}

		public override void OnDestroy()
		{
			UiWarpConfirm.M2D.FlagValotStabilize.Rem("WARPCONFIRM");
			Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
			if (flgUiEffectDisable != null)
			{
				flgUiEffectDisable.Rem("WARPCONFIRM");
			}
			base.OnDestroy();
		}

		public override T CreateT<T>(string name, float pixel_x, float pixel_y, float pixel_w, float pixel_h, int appear_dir_aim = -1, float appear_len = 30f, UiBoxDesignerFamily.MASKTYPE mask = UiBoxDesignerFamily.MASKTYPE.BOX)
		{
			T t = base.CreateT<T>(name, pixel_x, pixel_y, pixel_w, pixel_h, appear_dir_aim, appear_len, UiBoxDesignerFamily.MASKTYPE.NO_MASK);
			t.Focusable(false, false, null);
			t.WHanim(t.get_swidth_px(), t.get_sheight_px(), false, false);
			return t;
		}

		public bool EvtWait(bool is_first = false)
		{
			return is_first || this.active;
		}

		public override UiBoxDesignerFamily deactivate(bool immediate = false)
		{
			base.deactivate(immediate);
			IN.clearPushDown(true);
			return this;
		}

		public UiWarpConfirm Init(UiWarpConfirm.CTYPE _type, object OTarget = null, WholeMapItem _NextWM = null)
		{
			SND.Ui.play("paper", false);
			this.type = _type;
			this.NextWM = _NextWM;
			this.already_checked |= 1U << (int)_type;
			this.activate();
			this.BxC.Clear();
			this.BxC.alignx = ALIGN.CENTER;
			this.BxC.btn_height = 30f;
			this.BxC.margin_in_lr = 50f;
			this.BxC.item_margin_y_px = 4f;
			this.BxC.selectable_loop |= 2;
			this.BxC.init();
			if (this.NextWM != null)
			{
				this.BxC.margin_in_tb = 20f;
				this.BxC.addImg(new DsnDataImg
				{
					name = "warpTo",
					alignx = ALIGN.RIGHT,
					aligny = ALIGNY.MIDDLE,
					TxCol = C32.d2c(4294966715U),
					swidth = this.BxC.use_w * 0.6f,
					sheight = 52f,
					size = 20f,
					text = this.NextWM.localized_name_areatitle,
					FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.fnDrawInFIBWarpTo)
				});
				this.BxC.Br();
				this.BxC.addHr(new DsnDataHr().H(18f));
			}
			else
			{
				this.BxC.margin_in_tb = 70f;
			}
			List<string> list = new List<string>(4);
			DsnDataP dsnDataP = new DsnDataP("", false)
			{
				name = "head",
				alignx = ALIGN.CENTER,
				aligny = ALIGNY.MIDDLE,
				TxCol = C32.d2c(4283780170U),
				swidth = this.BxC.use_w,
				size = 18f
			};
			this.cancel_key = null;
			this.ATreasure = null;
			int num = 0;
			switch (this.type)
			{
			case UiWarpConfirm.CTYPE.TREASUREBOX:
			{
				this.ATreasure = OTarget as List<ItemStorage.IRow>;
				this.BxC.add(dsnDataP.Text(TX.GetA("current_dangerous", UiWarpConfirm.M2D.NightCon.getDangerMeterVal(false, false).ToString()))).Br();
				string text = "&&Submit_gohome";
				if (this.ATreasure.Count > 0)
				{
					int count = this.ATreasure.Count;
					for (int i = 0; i < count; i++)
					{
						bool flag = false;
						ItemStorage.IRow row = this.ATreasure[i];
						for (int j = i - 1; j >= 0; j--)
						{
							if (this.ATreasure[j].Data == row.Data)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							FillImageBlock fillImageBlock = this.BxC.addImg(new DsnDataImg
							{
								swidth = 38f + ((row.Info.total > 1) ? 16f : 0f),
								sheight = 40f,
								name = "mbox_" + i.ToString()
							});
							MeshDrawer meshDrawer = fillImageBlock.getMeshDrawer();
							MeshRenderer meshRenderer = fillImageBlock.getMeshRenderer();
							meshDrawer.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, fillImageBlock.stencil_ref), false);
							meshDrawer.activation_key = i.ToString();
							if (meshRenderer != null)
							{
								meshRenderer.sharedMaterials = meshDrawer.getMaterialArray(false);
							}
							fillImageBlock.FnDraw = new MeshDrawer.FnGeneralDraw(this.fnDrawReel);
							fillImageBlock.redraw_flag = true;
						}
					}
					this.BxC.Br();
					this.BxC.add(dsnDataP.Text(TX.Get("WarpConfirm_treasurebox", ""))).Br();
					text = "&&Submit_gohome_opening_box";
				}
				else
				{
					this.BxC.addHr(new DsnDataHr
					{
						line_height = 0f,
						margin_t = 38f,
						margin_b = 0f
					});
					this.BxC.add(dsnDataP.Text(TX.Get("WarpConfirm_returnback", ""))).Br();
				}
				list.Add(text);
				break;
			}
			case UiWarpConfirm.CTYPE.FOODLOST:
				if (UiWarpConfirm.AFoodRow == null)
				{
					UiWarpConfirm.AFoodRow = new List<ItemStorage.IRow>(1);
				}
				this.BxC.add(dsnDataP.Text(TX.GetA("WarpConfirm_having_food", UiWarpConfirm.AFoodRow.Count.ToString()))).Br();
				list.Add("&&Submit_deposit_goout");
				list.Add("&&Submit_goout");
				break;
			case UiWarpConfirm.CTYPE.WAIT_NIGHTINGALE:
				this.BxC.add(dsnDataP.Text(TX.Get("WarpConfirm_nightingale", ""))).Br();
				list.Add("&&Submit_wait");
				break;
			case UiWarpConfirm.CTYPE.REEL_DISCARD:
			{
				List<ReelExecuter> list2 = OTarget as List<ReelExecuter>;
				if (list2 != null)
				{
					this.BxC.add(dsnDataP.Text(TX.GetA("Reel_subtract_confirm", list2.Count.ToString()))).Br();
					int count2 = list2.Count;
					for (int k = 0; k < count2; k++)
					{
						aBtnNel aBtnNel = this.BxC.addButtonT<aBtnNel>(new DsnDataButton
						{
							skin = "reelinfo",
							w = 60f,
							h = 40f,
							title = ((int)list2[k].getEType()).ToString()
						});
						(aBtnNel.get_Skin() as ButtonSkinNelReelInfo).initReel(list2[k]);
						aBtnNel.hide();
					}
				}
				list.Add("&&Submit_discard");
				break;
			}
			case UiWarpConfirm.CTYPE.GOOUT_GQ:
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					WholeMapItem wholeMapItem = OTarget as WholeMapItem;
					stb.AddTxA("Guild_alert_goout_another_area", false).TxRpl(wholeMapItem.localized_name).TxRpl(this.NextWM.localized_name);
					this.BxC.add(dsnDataP.Text(stb)).Br();
					list.Add("&&Submit_goout");
					list.Add("&&Guild_Btn_open_quest_tab");
				}
				num = 2;
				this.cancel_key = "&&Store_desc_cancel";
				break;
			}
			case UiWarpConfirm.CTYPE.GOHOME_GQ:
			{
				using (STB stb2 = TX.PopBld(null, 0))
				{
					Vector2Int vector2Int = (Vector2Int)OTarget;
					stb2.AddTxA("Guild_alert_backhome_without_clear", false).TxRpl(vector2Int.y).TxRpl(vector2Int.x);
					this.BxC.add(dsnDataP.Text(stb2)).Br();
					list.Add("&&Submit_gohome_abort_quest");
					list.Add("&&Guild_Btn_open_quest_tab");
				}
				num = 2;
				this.cancel_key = "&&Store_desc_cancel";
				break;
			}
			}
			this.BxC.addHr(new DsnDataHr
			{
				line_height = 0f,
				margin_t = 50f,
				margin_b = 0f
			});
			if (this.cancel_key == null)
			{
				this.cancel_key = ((list.Count == 0) ? "&&Submit" : "&&Cancel");
			}
			list.Add(this.cancel_key);
			Designer bxC = this.BxC;
			DsnDataButtonMulti dsnDataButtonMulti = new DsnDataButtonMulti();
			dsnDataButtonMulti.skin = "row_center";
			dsnDataButtonMulti.titles = list.ToArray();
			dsnDataButtonMulti.w = this.btn_w;
			dsnDataButtonMulti.h = 30f;
			dsnDataButtonMulti.margin_h = 0f;
			dsnDataButtonMulti.margin_w = 0f;
			dsnDataButtonMulti.clms = 2;
			dsnDataButtonMulti.fnClick = new FnBtnBindings(this.fnClickedBtn);
			dsnDataButtonMulti.navi_loop = 3;
			dsnDataButtonMulti.fnMaking = delegate(BtnContainer<aBtn> BCon, aBtn B)
			{
				if (B.click_snd != "cancel")
				{
					B.click_snd = "enter";
				}
				return true;
			};
			(this.BCon = bxC.addButtonMultiT<aBtnNel>(dsnDataButtonMulti)).Get(num).Select(true);
			this.BxC.positionD(X.Mx(0f, (!UiWarpConfirm.M2D.GM.isActive()) ? UiWarpConfirm.M2D.ui_shift_x : 0f), 0f, 1, 30f);
			IN.clearPushDown(true);
			return this;
		}

		public override bool runIRD(float fcnt)
		{
			if (!base.runIRD(fcnt))
			{
				this.active = false;
				IN.DestroyOne(base.gameObject);
				return false;
			}
			if (IN.isCancel())
			{
				SND.Ui.play("cancel", false);
				aBtn aBtn = this.BCon.Get(this.cancel_key);
				if (aBtn.isSelected())
				{
					aBtn.ExecuteOnSubmitKey();
				}
				else
				{
					aBtn.Select(true);
				}
			}
			return true;
		}

		private bool fnClickedBtn(aBtn B)
		{
			string text = null;
			string title = B.title;
			if (title != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(title);
				if (num <= 1221219917U)
				{
					if (num <= 186111811U)
					{
						if (num != 183237725U)
						{
							if (num != 186111811U)
							{
								goto IL_030C;
							}
							if (!(title == "&&Cancel"))
							{
								goto IL_030C;
							}
						}
						else if (!(title == "&&Store_desc_cancel"))
						{
							goto IL_030C;
						}
					}
					else if (num != 1109359485U)
					{
						if (num != 1170277763U)
						{
							if (num != 1221219917U)
							{
								goto IL_030C;
							}
							if (!(title == "&&Submit"))
							{
								goto IL_030C;
							}
						}
						else
						{
							if (!(title == "&&Submit_gohome"))
							{
								goto IL_030C;
							}
							goto IL_0306;
						}
					}
					else
					{
						if (!(title == "&&Submit_deposit_goout"))
						{
							goto IL_030C;
						}
						int num2 = 0;
						int i = 0;
						int count = UiWarpConfirm.AFoodRow.Count;
						while (i < count)
						{
							ItemStorage.IRow row = UiWarpConfirm.AFoodRow[i];
							if (row.Data.RecipeInfo != null && row.Data.RecipeInfo.DishInfo != null)
							{
								NelItem data = row.Data;
								int calced_grade = row.Data.RecipeInfo.DishInfo.calced_grade;
								int num3 = X.Mn(row.total, UiWarpConfirm.M2D.IMNG.getInventory().getCount(row.Data, calced_grade));
								if (num3 > 0)
								{
									UiWarpConfirm.M2D.IMNG.getInventory().Reduce(data, num3, calced_grade, false);
									UiWarpConfirm.M2D.IMNG.getHouseInventory().Add(data, num3, calced_grade, true, true);
									num2 += num3;
								}
							}
							i++;
						}
						if (num2 > 0)
						{
							UiWarpConfirm.M2D.IMNG.getInventory().fineRows(false);
						}
						UILog.Instance.AddAlert(TX.GetA("Alert_depositted_food", num2.ToString()), UILogRow.TYPE.ALERT);
						text = "1";
						goto IL_030C;
					}
					text = "0";
					goto IL_030C;
				}
				if (num <= 3575607824U)
				{
					if (num != 1235684477U)
					{
						if (num != 2259569736U)
						{
							if (num != 3575607824U)
							{
								goto IL_030C;
							}
							if (!(title == "&&Submit_discard"))
							{
								goto IL_030C;
							}
							goto IL_0306;
						}
						else if (!(title == "&&Submit_goout"))
						{
							goto IL_030C;
						}
					}
					else
					{
						if (!(title == "&&Guild_Btn_open_quest_tab"))
						{
							goto IL_030C;
						}
						text = "0";
						UiWarpConfirm.M2D.GM.activateScenario(UiGameMenu.SCENARIO_CTG.SUB_QUEST);
						goto IL_030C;
					}
				}
				else if (num != 3675670019U)
				{
					if (num != 4031996910U)
					{
						if (num != 4109424027U)
						{
							goto IL_030C;
						}
						if (!(title == "&&Submit_wait"))
						{
							goto IL_030C;
						}
						goto IL_0306;
					}
					else
					{
						if (!(title == "&&Submit_gohome_opening_box"))
						{
							goto IL_030C;
						}
						goto IL_0306;
					}
				}
				else if (!(title == "&&Submit_gohome_abort_quest"))
				{
					goto IL_030C;
				}
				text = "1";
				goto IL_030C;
				IL_0306:
				text = "1";
			}
			IL_030C:
			if (text != null)
			{
				if (text == "1" && this.NextWM != null)
				{
					object obj;
					UiWarpConfirm.CTYPE ctype = UiWarpConfirm.checkUseConfirm(UiWarpConfirm.M2D.WM.CurWM, this.NextWM, out obj, this);
					if (ctype != UiWarpConfirm.CTYPE.OFFLINE)
					{
						this.BxC.WHanim(this.main_w, this.main_h, true, true);
						this.BxC.wh_animZero(true, true);
						this.Init(ctype, obj, this.NextWM);
						return true;
					}
				}
				this.prompt_result = text;
				UiWarpConfirm.AFoodRow = null;
				this.deactivate(false);
				if (this.input_to_varcon)
				{
					EV.getVariableContainer().define("_result", text, true);
				}
			}
			return true;
		}

		private bool fnDrawInFIBWarpTo(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			float num = FI.get_text_swidth_px(true);
			if (num == 0f)
			{
				return false;
			}
			float num2 = num + 80f;
			float num3 = (FI.get_swidth_px() - num2) * 0.5f;
			if (FI.margin_x != num3)
			{
				FI.margin_x = num3;
				FI.fineTxTransform();
			}
			Md.Col = C32.MulA(4283780170U, alpha);
			Md.KadomaruRect(0f, 0f, num2 + 160f, FI.get_sheight_px() - 12f, 30f, 0f, false, 0f, 0f, false);
			Md.Col = C32.MulA(4294966715U, alpha);
			Md.Arrow(-num2 * 0.5f + 10f + 20f * X.ZSIN(X.ANMPT(90, 1f), 0.35f), 0f, 16f, 0f, 5f, false);
			update_meshdrawer = true;
			return false;
		}

		private bool fnDrawReel(MeshDrawer Md, float alpha)
		{
			bool flag = true;
			try
			{
				alpha *= this.BxC.animating_alpha;
				if (alpha <= 0f)
				{
					Md.clearSimple();
					return false;
				}
				if (this.ATreasure == null)
				{
					return true;
				}
				int num = X.NmI(Md.activation_key, 0, false, false);
				ItemStorage.IRow row = (X.BTW(0f, (float)num, (float)this.ATreasure.Count) ? this.ATreasure[num] : null);
				if (row == null)
				{
					return true;
				}
				ItemStorage.ObtainInfo info = row.Info;
				ReelManager.ItemReelContainer ir = ReelManager.GetIR(row.Data);
				flag = this.BxC.animating_alpha == 1f;
				ir.drawSmallIcon(Md, (info.total > 1) ? (-8f) : 0f, 0f, alpha, 2f, false);
				if (info.total > 1)
				{
					Md.Col = C32.MulA(4283780170U, alpha);
					STB stb = TX.PopBld(null, 0);
					stb += "x";
					stb += info.total;
					MTRX.ChrM.DrawScaleStringTo(Md, stb, 38f, -21f, 2f, 2f, ALIGN.RIGHT, ALIGNY.BOTTOM, false, 0f, 0f, null);
					TX.ReleaseBld(stb);
				}
				Md.updateForMeshRenderer(false);
			}
			catch
			{
				Md.updateForMeshRenderer(false);
				return true;
			}
			return flag;
		}

		private static NelM2DBase M2D;

		private static List<ItemStorage.IRow> AFoodRow;

		public uint already_checked;

		private UiBoxDesigner BxC;

		private UiWarpConfirm.CTYPE type;

		private readonly float main_w = IN.w * 0.6f;

		private readonly float main_h = IN.h * 0.55f;

		private const float margin_in_x = 50f;

		private const float btn_h = 30f;

		private const int btn_clms = 2;

		private string cancel_key;

		private const float TREASURE_SWIDTH = 38f;

		private const float TREASURE_COUNT_MARGIN = 16f;

		private BtnContainer<aBtn> BCon;

		private WholeMapItem NextWM;

		private List<ItemStorage.IRow> ATreasure;

		public bool input_to_varcon = true;

		public string prompt_result;

		public enum CTYPE
		{
			OFFLINE,
			TREASUREBOX,
			FOODLOST,
			WAIT_NIGHTINGALE,
			REEL_DISCARD,
			GOOUT_GQ,
			GOHOME_GQ
		}
	}
}
