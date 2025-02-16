using System;
using System.Collections.Generic;
using Better;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class UiLunchTimeBase : UiBoxDesignerFamily, IEventWaitListener
	{
		public static float costbx_w
		{
			get
			{
				return IN.w * 0.6f;
			}
		}

		protected override void Awake()
		{
			this.AwakeLunch();
		}

		protected virtual ItemStorage AwakeLunch()
		{
			base.Awake();
			this.M2D = M2DBase.Instance as NelM2DBase;
			this.auto_deactive_gameobject = false;
			this.base_z = -0.04f;
			this.stabilize_key = "Lunch" + IN.totalframe.ToString();
			this.GobNoel = IN.CreateGob(base.gameObject, "-noelmd");
			IN.setZ(this.GobNoel.transform, -0.002f);
			this.MdNoel = MeshDrawer.prepareMeshRenderer(this.GobNoel, MTRX.MtrMeshNormal, 0f, -1, null, false, false);
			this.MdNoel.chooseSubMesh(1, false, false);
			this.MdNoel.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, -1), false);
			this.MdNoel.chooseSubMesh(0, false, false);
			this.MdNoel.connectRendererToTriMulti(this.GobNoel.GetComponent<MeshRenderer>());
			this.GobEf = IN.CreateGob(base.gameObject, "-ef");
			IN.setZ(this.GobEf.transform, -0.01f);
			this.MdEf = MeshDrawer.prepareMeshRenderer(this.GobEf, MTR.MIiconL.getMtr(BLEND.NORMAL, -1), 0f, -1, null, true, true);
			this.AEaten = new List<UiLunchTimeBase.ConsumeResult>();
			this.St = new ItemStorage("lunch", 99);
			this.St.check_quest_target = true;
			this.St.infinit_stockable = (this.St.water_stockable = (this.St.grade_split = true));
			this.St.sort_button_bits = 23;
			this.St.setSort(ItemStorage.SORT_TYPE.NEWER, false);
			this.OSrcRow = new BDic<ItemStorage.ObtainInfo, UiLunchTimeBase.RowPackCnt>(4);
			if (this.M2D == null)
			{
				if (Stomach.Temporary == null)
				{
					Stomach.Temporary = new Stomach(null, null);
				}
				this.Stm = Stomach.Temporary;
			}
			else
			{
				this.M2D.FlagValotStabilize.Add(this.stabilize_key);
				this.M2D.Ui.FlgFrontLog.Add(this.stabilize_key);
				NelItemManager imng = this.M2D.IMNG;
				this.Stm = imng.StmNoel;
				if (this.load_my_inventory)
				{
					this.addExternal(imng.getInventoryIngredientArray(false));
				}
				this.St.FD_RowNameAddition = new UiItemManageBox.FnRowNameAddition(this.fnRowNameAddition);
				this.St.FD_RowIconAddition = new UiItemManageBox.FnRowIconAddition(this.fnRowIconAddition);
			}
			this.StmTemp = new Stomach(this.Stm, null);
			this.ItemMng = new UiItemManageBox((this.M2D != null) ? this.M2D.IMNG : null, null)
			{
				cmd_w = 390f,
				fnItemRowInitAfter = new UiItemManageBox.FnItemRowInitAfter(this.fnLunchItemMngRowInitAfter)
			};
			return this.St;
		}

		protected void AwakeEvImgTicket(EvImg Img, int delay = 0)
		{
			this.t_noel = X.Mn((float)(-100 - delay), this.t_noel);
			LoadTicketManager.AddTicket("LUNCH", "AWAKE", delegate(LoadTicketManager.LoadTicket Tk)
			{
				if (this.t_noel < -100f)
				{
					return true;
				}
				(Tk.Target as UiLunchTimeBase).runNoelEvImgFinalize(Img);
				return false;
			}, this, 200);
		}

		protected virtual Material runNoelEvImgFinalize(EvImg Img)
		{
			this.MdNoel.chooseSubMesh(1, false, false);
			Material material = MTRX.newMtr(MTRX.getMI(Img.PF).getMtr(BLEND.NORMAL, -1));
			material.SetFloat("_UseAddColor", 1f);
			material.EnableKeyword("_USEADDCOLOR_ON");
			this.MdNoel.setMaterial(material, true);
			this.MdNoel.chooseSubMesh(0, false, false);
			if (this.t_noel < 0f)
			{
				this.t_noel = 0f;
			}
			this.need_redraw_noel = true;
			return material;
		}

		public override void OnDestroy()
		{
			if (this.AEaten != null)
			{
				this.consumeEaten();
			}
			if (this.MdNoel != null)
			{
				this.MdNoel.destruct();
				this.MdEf.destruct();
			}
			if (this.M2D != null && TX.valid(this.stabilize_key))
			{
				this.M2D.Ui.FlgFrontLog.Rem(this.stabilize_key);
				this.M2D.FlagValotStabilize.Rem(this.stabilize_key);
			}
			global::UnityEngine.Object.Destroy(this.GobNoel);
			base.OnDestroy();
		}

		public virtual int consumeEaten()
		{
			if (this.AEaten == null)
			{
				return 0;
			}
			int num = 0;
			int count = this.AEaten.Count;
			this.AEaten.Sort(delegate(UiLunchTimeBase.ConsumeResult A, UiLunchTimeBase.ConsumeResult B)
			{
				if (A.packed_food == B.packed_food)
				{
					return 0;
				}
				if (!A.packed_food)
				{
					return 1;
				}
				return -1;
			});
			int count2 = this.AInventorySrc.Count;
			for (int i = 0; i < count; i++)
			{
				UiLunchTimeBase.ConsumeResult consumeResult = this.AEaten[i];
				NelItem nelItem = consumeResult.Itm;
				for (int j = 0; j < count2; j++)
				{
					ItemStorage itemStorage = this.AInventorySrc[j];
					ItemStorage.ObtainInfo info = itemStorage.getInfo(nelItem);
					if (info != null && info.total > 0 && this.ReduceFromInventorySrc(itemStorage, nelItem, info))
					{
						if (consumeResult.packed_food)
						{
							itemStorage.removeWLink(nelItem, 1);
						}
						num |= 1 << j;
						nelItem = null;
						break;
					}
				}
				if (nelItem != null && this.AAExternal != null)
				{
					for (int k = this.AAExternal.Count - 1; k >= 0; k--)
					{
						List<NelItem> list = this.AAExternal[k];
						int num2 = list.IndexOf(nelItem);
						if (num2 >= 0)
						{
							list.RemoveAt(num2);
							nelItem = null;
							break;
						}
					}
				}
				if (nelItem != null)
				{
					X.de("アイテムの削除に失敗: " + nelItem.getLocalizedName(consumeResult.grade), null);
				}
			}
			for (int l = this.AInventorySrc.Count - 1; l >= 0; l--)
			{
				if ((num & (1 << l)) != 0)
				{
					this.AInventorySrc[l].fineRows(false);
				}
			}
			this.AEaten = null;
			return num;
		}

		protected virtual bool ReduceFromInventorySrc(ItemStorage Str, NelItem _Itm, ItemStorage.ObtainInfo StrObt)
		{
			Str.Reduce(_Itm, 1, StrObt.top_grade, false);
			return true;
		}

		private void addDish(NelItem Itm, int cnt, ItemStorage _St = null, ItemStorage.IRow _R = null)
		{
			if (Itm.is_food && Itm.RecipeInfo != null && Itm.RecipeInfo.DishInfo != null)
			{
				RCP.RecipeDish dishInfo = Itm.RecipeInfo.DishInfo;
				ItemStorage.IRow row;
				this.St.Add(Itm, cnt, dishInfo.calced_grade, out row, true, true);
				if (_R != null && _St != null && !_St.water_stockable && row != null)
				{
					UiLunchTimeBase.RowPackCnt rowPackCnt;
					if (!this.OSrcRow.TryGetValue(row.Info, out rowPackCnt))
					{
						rowPackCnt = (this.OSrcRow[row.Info] = new UiLunchTimeBase.RowPackCnt(_R, _St));
					}
					rowPackCnt.count += cnt;
					rowPackCnt.cached_is_packed += (_R.has_wlink ? 1 : 0);
					return;
				}
			}
			else if (_St != null && _St.grade_split && this.alloc_not_food_on_row)
			{
				this.St.Add(Itm, _R.total, (int)_R.splitted_grade, true, true);
			}
		}

		public void addExternal(List<NelItem> A)
		{
			if (A == null)
			{
				return;
			}
			if (this.AAExternal == null)
			{
				this.AAExternal = new List<List<NelItem>>(1);
			}
			this.AAExternal.Add(A);
			int count = A.Count;
			for (int i = 0; i < count; i++)
			{
				this.addDish(A[i], 1, null, null);
			}
		}

		public void addExternal(ItemStorage _Src, bool fine_newer = true)
		{
			if (this.AInventorySrc.IndexOf(_Src) == -1)
			{
				this.AInventorySrc.Add(_Src);
				_Src.getVisibleRowCount();
				int wholeRowCount = _Src.getWholeRowCount();
				for (int i = 0; i < wholeRowCount; i++)
				{
					ItemStorage.IRow rowByIndex = _Src.getRowByIndex(i);
					this.addDish(rowByIndex.Data, rowByIndex.total, _Src, rowByIndex);
				}
				this.St.copyNewer(_Src, fine_newer);
			}
		}

		public void addExternal(ItemStorage[] ASrc)
		{
			int num = ASrc.Length;
			this.St.do_not_input_newer = true;
			for (int i = 0; i < num; i++)
			{
				this.addExternal(ASrc[i], false);
			}
			this.St.do_not_input_newer = false;
			this.St.fineNewerIndex();
		}

		public void setSort(ItemStorage.SORT_TYPE s, bool descend = false)
		{
			this.St.setSort(s, descend);
		}

		protected virtual void initBoxes()
		{
			UiItemStore.prepareOneSet(this, this.out_w, this.out_h, this.right_h, this.right_h_margin, ref this.BxR, ref this.BxDesc, ref this.BxCmd, this.shiftx, -30f);
			UiBox box = this.BxDesc.getBox();
			box.frametype = UiBox.FRAMETYPE.ONELINE;
			this.desc_main_x = box.get_deperture_x();
			this.desc_main_y = box.get_deperture_y();
			this.desc_main_w = box.get_swidth_px();
			this.desc_main_h = box.get_sheight_px();
			this.initDesc(false);
			this.BxCmd.posSetDA(this.shiftx, -30f - this.out_h * 0.5f + this.cmd_h * 0.5f, 1, 200f, true);
			this.BxCmd.WHanim(this.cmd_w, this.cmd_h, false, false);
			this.BxCmd.deactivate();
			this.ItemTab = this.BxR.addTab("status_area", this.BxR.use_w, this.BxR.use_h, this.BxR.use_w, this.BxR.use_h, false);
			this.ItemTab.Smallest();
			this.ItemTab.margin_in_tb = 6f;
			this.ItemMng.slice_height = 0f;
			this.BxR.endTab(false);
			this.BxR.Br();
			this.BxCost = base.Create("cost", this.shiftx - 70f, 18f + this.out_h * 0.5f - UiLunchTimeBase.costbx_h * 0.5f, UiLunchTimeBase.costbx_w, UiLunchTimeBase.costbx_h, 2, 1400f, UiBoxDesignerFamily.MASKTYPE.BOX);
			UiBox box2 = this.BxCost.getBox();
			box2.delayt = 20;
			box2.frametype = UiBox.FRAMETYPE.NONE;
			this.BxCost.Small();
			this.BxCost.margin_in_lr = 30f;
			this.BxCost.margin_in_tb = 2f;
			this.BxCost.item_margin_x_px = 10f;
			this.BxCost.alignx = ALIGN.RIGHT;
			this.BxCost.init();
			float use_h = this.BxCost.use_h;
			this.MMRDCost = this.BxCost.createTempMMRD(true);
			this.MdCostMask = this.MMRDCost.Make(MTRX.getMtr(BLEND.MASK, 230));
			this.MdCost = this.MMRDCost.Make(MTRX.MIicon.getMtr(BLEND.NORMAL, -1));
			this.MdCost.chooseSubMesh(1, false, false);
			this.MdCost.setMaterial(MTRX.getMtr(BLEND.NORMALST, 230), false);
			this.MdCost.chooseSubMesh(2, false, false);
			this.MdCost.connectRendererToTriMulti(this.MMRDCost.GetMeshRenderer(this.MdCost));
			this.drawCostMeter(true);
			this.FbCostStr = this.BxCost.addP(new DsnDataP("", false)
			{
				swidth = 140f,
				sheight = use_h,
				alignx = ALIGN.CENTER,
				aligny = ALIGNY.MIDDLE,
				text_auto_wrap = false,
				text_auto_condense = true,
				html = true,
				text = this.Stm.cost.ToString() + " / " + this.Stm.cost_max.ToString(),
				size = 18f,
				letterSpacing = 0.88f,
				TxCol = C32.d2c(4283780170U)
			}, false);
			if (TX.valid(this.default_select_item_key))
			{
				this.St.select_row_key = this.default_select_item_key;
			}
			this.initItemTab(this.St, this.ItemMng, false);
		}

		public override T CreateT<T>(string name, float pixel_x, float pixel_y, float pixel_w, float pixel_h, int appear_dir_aim = -1, float appear_len = 30f, UiBoxDesignerFamily.MASKTYPE mask = UiBoxDesignerFamily.MASKTYPE.BOX)
		{
			T t = base.CreateT<T>(name, pixel_x, pixel_y, pixel_w, pixel_h, appear_dir_aim, appear_len, UiBoxDesignerFamily.MASKTYPE.NO_MASK);
			t.stencil_ref = -1;
			t.Focusable(false, false, null);
			return t;
		}

		protected virtual void initItemTab(ItemStorage St, UiItemManageBox ItemMng, bool clear = false)
		{
			Designer itemTab = this.ItemTab;
			if (clear)
			{
				itemTab.Clear();
			}
			ItemMng.quitDesigner(false, false);
			ItemMng.use_grade_stars = false;
			ItemMng.auto_select_on_adding_row = false;
			ItemMng.fnCommandPrepare = new UiItemManageBox.FnCommandPrepare(this.fnDishChoosen);
			ItemMng.fnDescAddition = new UiItemManageBox.FnDescAddition(this.fnLunchDescAddition);
			ItemMng.fnDetailPrepare = new UiItemManageBox.FnDetailPrepare(this.fnDetailPrepare);
			ItemMng.fnWholeRowsPrepare = new UiItemManageBox.FnWholeRowsPrepare(this.fnLunchWholeRowsPrepare);
			ItemMng.row_height = 30f;
			ItemMng.item_row_skin = this.item_row_skin;
			ItemMng.title_text_content = "";
			ItemMng.stencil_ref = 229;
			ItemMng.initDesigner(St, itemTab, null, null, true, true, true);
			ItemMng.ParentBoxDesigner = this.BxR;
		}

		public override bool runIRD(float fcnt)
		{
			bool flag = base.runIRD(fcnt);
			if (this.af >= this.INIT_DELAY)
			{
				if (this.BxR == null)
				{
					this.initBoxes();
				}
				UiLunchTimeBase.STATE state = this.stt;
				if (state != UiLunchTimeBase.STATE.MAIN)
				{
					if (state == UiLunchTimeBase.STATE.EATING)
					{
						if (X.D)
						{
							this.drawCostMeter(false);
						}
						if (IN.isCancel())
						{
							SND.Ui.play("cancel", false);
							this.changeState(UiLunchTimeBase.STATE.MAIN);
						}
					}
				}
				else
				{
					if (X.D)
					{
						this.drawCostMeter(false);
					}
					this.ItemMng.runEditItem();
					if (IN.isUiAddPD() && this.AppliedDish != null)
					{
						SND.Ui.play("tool_hand_init", false);
						bool flag2 = this.desc_mode == UiLunchTimeBase.DESC.INGREDIENT;
						this.desc_mode = (UiLunchTimeBase.DESC.INGREDIENT + (int)this.desc_mode) % UiLunchTimeBase.DESC._MAX;
						if (this.desc_mode == UiLunchTimeBase.DESC.INGREDIENT != flag2)
						{
							this.initDesc(false);
						}
						else
						{
							this.need_fine_desc_fb = 2;
						}
						this.fineDescFb();
					}
					else if (IN.isCancel())
					{
						SND.Ui.play("cancel", false);
						this.changeState(UiLunchTimeBase.STATE.NONE);
					}
					else if (this.need_fine_desc_fb > 0)
					{
						this.fineDescFb();
					}
				}
			}
			if (this.stt == UiLunchTimeBase.STATE.NONE)
			{
				if (!flag && this.af <= -30)
				{
					global::UnityEngine.Object.Destroy(base.gameObject);
					return false;
				}
				this.need_redraw_noel = true;
			}
			if (X.D)
			{
				this.drawBackground((float)this.af, false);
				this.runNoelImg(this.MdNoel, ref this.need_redraw_noel);
				this.t_noel += (float)X.AF;
				if (-100f < this.t_noel && this.t_noel < -50f)
				{
					this.t_noel = -100f;
				}
				bool flag3 = false;
				if (this.need_redraw_noel)
				{
					flag3 = this.drawNoelImg(this.MdNoel, (this.af >= 0) ? 1f : X.ZLINE((float)(30 + this.af), 30f), ref this.need_redraw_noel);
				}
				this.MdEf.clear(false, false);
				if (this.af_effect >= 0)
				{
					this.af_effect += X.AF;
					if (!this.runEffect(this.MdEf, (float)this.af_effect))
					{
						this.af_effect = -1;
					}
				}
				this.MdEf.updateForMeshRenderer(false);
				this.MdNoel.chooseSubMesh(0, false, false);
				if (flag3)
				{
					this.MdNoel.updateForMeshRenderer(true);
				}
			}
			if (this.af >= 0)
			{
				this.af++;
			}
			else
			{
				this.af--;
			}
			return true;
		}

		protected virtual bool runEffect(MeshDrawer MdEf, float af_effect)
		{
			return false;
		}

		private void initDesc(bool set_pos = true)
		{
			this.BxDesc.Clear();
			this.BxDesc.stencil_ref = 230;
			this.BxDesc.use_scroll = true;
			this.BxDesc.scrolling_margin_in_lr = 4f;
			this.BxDesc.scrolling_margin_in_tb = 4f;
			this.BxDesc.item_margin_x_px = 0f;
			this.BxDesc.item_margin_y_px = 0f;
			this.FbDescTR = null;
			this.FbDescTL = null;
			this.need_fine_desc_fb = 2;
			if (this.stt == UiLunchTimeBase.STATE.MAIN)
			{
				this.BxDesc.margin_in_lr = 10f;
				this.BxDesc.margin_in_tb = 20f;
				if (set_pos)
				{
					this.BxDesc.position(this.desc_main_x, this.desc_main_y, -1000f, -1000f, false);
					this.BxDesc.WHanim(this.desc_main_w, this.desc_main_h, true, true);
				}
				this.BxDesc.init();
				DsnDataP dsnDataP = NelDsn.PT_LT(14, true);
				dsnDataP.swidth = this.BxDesc.use_w - 48f;
				dsnDataP.text = TX.Get("KD_change_info", "");
				this.FbDescTL = this.BxDesc.addP(dsnDataP, false);
				this.FbDescTR = this.BxDesc.addImg(new DsnDataImg
				{
					swidth = this.BxDesc.use_w,
					FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.FnDrawMainTR),
					MI = MTRX.MIicon
				});
				this.BxDesc.Br();
				this.BxDesc.Hr(0.75f, 4f, 10f, 1f).Br();
				float num = this.BxDesc.use_h * 0.5f - 52f;
				bool flag = false;
				if (this.desc_mode == UiLunchTimeBase.DESC.STOMACH || this.desc_mode == UiLunchTimeBase.DESC.ITEMDATA)
				{
					dsnDataP = NelDsn.PT_LT(14, true);
					dsnDataP.swidth = this.BxDesc.use_w;
					dsnDataP.sheight = num;
					dsnDataP.name = "pre";
					dsnDataP.text_auto_wrap = true;
					dsnDataP.lineSpacing = 1.3f;
					this.FbPre = this.BxDesc.addPExtendH(dsnDataP, ref flag, true);
					this.BxDesc.Br();
					this.BxDesc.Hr(0.75f, 24f, 24f, 1f);
					this.BxDesc.Br();
					dsnDataP.name = "aft";
					this.FbAft = this.BxDesc.addPExtendH(dsnDataP, ref flag, true);
				}
				else
				{
					this.BxDesc.item_margin_y_px = 6f;
					dsnDataP = NelDsn.PT_LT(14, true);
					dsnDataP.swidth = this.BxDesc.use_w;
					dsnDataP.text = TX.Get("lunch_detail_effect", "");
					dsnDataP.lineSpacing = 1.3f;
					this.BxDesc.addP(dsnDataP, false);
					this.BxDesc.Br();
					this.BxDesc.XSh(18f);
					dsnDataP = NelDsn.PT_LT(14, true);
					dsnDataP.swidth = this.BxDesc.use_w;
					dsnDataP.sheight = num;
					dsnDataP.name = "pre";
					dsnDataP.text_auto_wrap = true;
					dsnDataP.lineSpacing = 1.3f;
					this.FbPre = this.BxDesc.addPExtendH(dsnDataP, ref flag, true);
					this.BxDesc.Br();
					this.BxDesc.Hr(0.75f, 10f, 22f, 1f).Br();
					dsnDataP = NelDsn.PT_LT(14, true);
					dsnDataP.swidth = this.BxDesc.use_w;
					dsnDataP.text = TX.Get("lunch_detail_ingredient", "");
					this.BxDesc.addP(dsnDataP, false);
					this.BxDesc.Br();
					this.BxDesc.XSh(18f);
					dsnDataP = NelDsn.PT_LT(14, true);
					dsnDataP.swidth = this.BxDesc.use_w;
					dsnDataP.name = "aft";
					dsnDataP.text_auto_wrap = true;
					dsnDataP.Aword_splitter = new char[] { '\b' };
					dsnDataP.lineSpacing = 1.3f;
					this.FbAft = this.BxDesc.addPExtendH(dsnDataP, ref flag, true);
				}
			}
			else
			{
				this.BxDesc.margin_in_lr = 28f;
				this.BxDesc.margin_in_tb = 20f;
				this.BxDesc.position(this.shiftx, 28f, -1000f, -1000f, false);
				this.BxDesc.WHanim(this.out_w, this.out_h - 120f, true, true);
				this.BxDesc.item_margin_x_px = 50f;
				this.BxDesc.alignx = ALIGN.CENTER;
				this.BxDesc.init();
				float use_h = this.BxDesc.use_h;
				float num2 = use_h - 50f;
				float num3 = 0f;
				string text = TX.Get("lunch_stomach_pre", "");
				using (STB stb = TX.PopBld(null, 0))
				{
					float num4 = this.BxDesc.use_w * 0.5f - this.BxDesc.item_margin_x_px - 2f;
					Designer designer = this.BxDesc.addTab("pre_tab", num4, num2, num4, num2, false);
					designer.Smallest();
					DsnDataP dsnDataP2;
					if (this.AppliedDish != null)
					{
						dsnDataP2 = NelDsn.PT_LT(15, true);
						this.getEffectListupTo(stb, this.StmTemp);
						dsnDataP2.text = TX.add(text, this.aft_header, "\n");
						dsnDataP2.swidth = num4;
						dsnDataP2.sheight = 0f;
						dsnDataP2.text_auto_wrap = true;
						dsnDataP2.name = "pre_pf";
						dsnDataP2.lineSpacing = 1.3f;
						FillBlock fillBlock = this.BxDesc.addP(dsnDataP2, false);
						num3 = fillBlock.get_sheight_px();
						fillBlock.text_content = text;
						this.BxDesc.Br();
						this.BxDesc.Hr(0.875f, 15f, 16f, 1f).Br();
					}
					dsnDataP2 = NelDsn.PT_LT(15, true);
					using (STB stb2 = TX.PopBld(null, 0))
					{
						dsnDataP2.swidth = num4;
						dsnDataP2.text_auto_wrap = true;
						dsnDataP2.name = "pre";
						if (this.AppliedDish != null)
						{
							dsnDataP2.Stb = this.getEffectListupTo(stb2, this.Stm);
						}
						else
						{
							this.AppliedIR.Data.getDetail(stb2, this.St, (int)this.AppliedIR.splitted_grade, null, true, false, false);
							dsnDataP2.Stb = stb2;
						}
						dsnDataP2.lineSpacing = 1.3f;
						this.FbPre = this.BxDesc.addP(dsnDataP2, true);
					}
					this.BxDesc.endTab(true);
					designer = this.BxDesc.addTab("aft_tab", num4, num2, num4, num2, false);
					designer.Smallest();
					if (this.AppliedDish != null)
					{
						dsnDataP2 = NelDsn.PT_LT(15, true);
						dsnDataP2.swidth = num4;
						dsnDataP2.sheight = num3;
						dsnDataP2.text_auto_wrap = true;
						dsnDataP2.name = "aft_pf";
						dsnDataP2.text = TX.add(TX.Get("lunch_stomach_after", ""), this.aft_header, "\n");
						dsnDataP2.lineSpacing = 1.3f;
						this.BxDesc.addP(dsnDataP2, false);
						this.BxDesc.Br();
						this.BxDesc.Hr(0.875f, 15f, 16f, 1f).Br();
					}
					dsnDataP2 = NelDsn.PT_LT(15, true);
					dsnDataP2.swidth = num4;
					dsnDataP2.name = "aft";
					if (this.AppliedDish != null)
					{
						dsnDataP2.Stb = stb;
					}
					else
					{
						this.AppliedIR.Data.getDescLocalized(stb.Clear(), this.St, (int)this.AppliedIR.splitted_grade);
						dsnDataP2.Stb = stb;
					}
					dsnDataP2.text_auto_wrap = true;
					dsnDataP2.lineSpacing = 1.3f;
					this.FbAft = this.BxDesc.addP(dsnDataP2, true);
					this.BxDesc.endTab(true);
					num2 = X.Mx(num2, designer.get_sheight_px());
				}
				MultiMeshRenderer multiMeshRenderer = this.BxDesc.createTempMMRD(true);
				MeshDrawer meshDrawer = multiMeshRenderer.Make(MTRX.MtrMeshNormal);
				meshDrawer.Col = C32.d2c(4283780170U);
				if (this.AppliedDish != null)
				{
					meshDrawer.Line(0f, use_h * 0.5f, 0f, 14f, 1f, false, 0f, 0f);
					meshDrawer.Line(0f, -use_h * 0.5f, 0f, -14f, 1f, false, 0f, 0f);
					meshDrawer.chooseSubMesh(1, false, false);
					meshDrawer.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, -1), false);
					meshDrawer.RotaPF(0f, 0f, 1f, 1f, 0f, MTRX.getPF("arrow_nel_5"), false, false, false, uint.MaxValue, false, 0);
					meshDrawer.chooseSubMesh(0, false, false);
				}
				else
				{
					meshDrawer.Line(0f, -use_h * 0.5f, 0f, use_h * 0.5f, 1f, false, 0f, 0f);
				}
				meshDrawer.connectRendererToTriMulti(multiMeshRenderer.GetMeshRenderer(meshDrawer));
				meshDrawer.updateForMeshRenderer(true);
			}
			this.BxDesc.getScrollBox().setScrollLevelTo(0f, 0f, false);
			this.BxDesc.getScrollBox().startAutoScroll(30);
			this.BxDesc.Br();
		}

		private void initCmd()
		{
			this.BxCmd.Clear();
			this.BxCmd.Small();
			this.BxCmd.margin_in_lr = 40f;
			this.BxCmd.margin_in_tb = 12f;
			using (STB stb = TX.PopBld(null, 0))
			{
				if (this.AppliedDish != null)
				{
					this.copyCmdPromptTo(stb, this.Stm.applyableDoubleBonus(this.AppliedDish), this.AppliedDish.Rcp.is_water, this.StmTemp.cost_applied_level, this.StmTemp.isAnyEffectDuped());
				}
				else
				{
					this.copyCmdPromptTo(stb, false, this.AppliedIR.Data.is_water, 1f, false);
				}
				stb.TxRpl(this.SelectedIR.Data.getLocalizedName((int)this.SelectedIR.splitted_grade));
				this.BxCmd.alignx = ALIGN.CENTER;
				this.BxCmd.init();
				this.BxCmd.activate();
				this.BxCmd.addP(new DsnDataP("", false)
				{
					Stb = stb,
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.MIDDLE,
					TxCol = C32.d2c(4283780170U),
					text_auto_wrap = false,
					text_auto_condense = true,
					swidth = this.BxCmd.use_w,
					size = 16f,
					html = true
				}, false);
			}
			this.BxCmd.Br();
			BtnContainer<aBtn> btnContainer = this.BxCmd.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
			{
				name = "btns",
				titles = new string[] { "Eat", "&&Cancel" },
				skin = "row_center",
				clms = 2,
				w = this.BxCmd.use_w * 0.5f - 20f,
				h = 24f,
				margin_h = 0f,
				margin_w = 0f,
				fnClick = new FnBtnBindings(this.fnClickEatingCmd)
			});
			aBtn aBtn = btnContainer.Get(0);
			if (this.StmTemp.cost_applied_level == 0f)
			{
				aBtn.SetLocked(true, true, false);
				btnContainer.Get(1).Select(true);
			}
			else
			{
				aBtn.Select(true);
			}
			this.setConfirmBtnTitle(aBtn, this.AppliedIR);
		}

		protected virtual void copyCmdPromptTo(STB Stb, bool is_double, bool is_water, float cost_applied_level, bool is_duped)
		{
			if (is_double)
			{
				Stb.AddTxA("lunch_confirm_double", false).Ret("\n");
				Stb.AddTxA("lunch_confirm", false);
				return;
			}
			if (!is_water)
			{
				if (cost_applied_level >= 0f && cost_applied_level < 1f)
				{
					Stb.AddTxA((cost_applied_level == 0f) ? "lunch_confirm_full" : "lunch_confirm_burst", false);
					return;
				}
				if (is_duped)
				{
					Stb.AddTxA("lunch_confirm_dupe", false);
					return;
				}
				Stb.AddTxA("lunch_confirm", false);
				return;
			}
			else
			{
				if (cost_applied_level >= 0f && cost_applied_level < 1f)
				{
					Stb.AddTxA((cost_applied_level == 0f) ? "lunch_drink_confirm_full" : "lunch_drink_confirm_burst", false);
					return;
				}
				if (is_duped)
				{
					Stb.AddTxA("lunch_drink_confirm_dupe", false);
					return;
				}
				Stb.AddTxA("lunch_drink_confirm", false);
				return;
			}
		}

		protected virtual void setConfirmBtnTitle(aBtn B, ItemStorage.IRow AppliedIR)
		{
			B.setSkinTitle(TX.Get(AppliedIR.Data.is_water ? "Eat_Drink" : "Eat", ""));
		}

		public void fnLunchWholeRowsPrepare(UiItemManageBox IMng, List<ItemStorage.IRow> ASource, List<ItemStorage.IRow> ADest)
		{
			ADest.AddRange(ASource);
		}

		public void fnLunchItemMngRowInitAfter(aBtnItemRow B, ItemStorage.IRow IRow)
		{
			NelItem itemData = B.getItemData();
			B.SetLocked(!UiLunchTimeBase.isUseable(itemData), true, false);
		}

		public static bool isUseable(NelItem Itm)
		{
			RCP.RecipeItemInfo recipeInfo = Itm.RecipeInfo;
			return recipeInfo == null || recipeInfo.DishInfo == null || UiLunchTimeBase.isUseable(recipeInfo.DishInfo.Rcp);
		}

		public static bool isUseable(RCP.Recipe Rcp)
		{
			return Rcp == null || !EnemySummoner.isActiveBorder() || Rcp.edible_in_battle || Rcp.is_water;
		}

		protected virtual void fnRowNameAddition(STB Stb, ItemStorage.IRow IR, ItemStorage Storage)
		{
			UiLunchTimeBase.RowPackCnt rowPackCnt;
			if (this.OSrcRow.TryGetValue(IR.Info, out rowPackCnt))
			{
				Stb.Clear();
				IR.Data.getLocalizedName(Stb, (int)IR.splitted_grade);
				if (rowPackCnt.St.FD_RowNameAddition != null)
				{
					rowPackCnt.St.FD_RowNameAddition(Stb, rowPackCnt.Row, rowPackCnt.St);
				}
			}
		}

		public int fnRowIconAddition(ItemStorage.IRow IR, ItemStorage Storage, int def_ico)
		{
			UiLunchTimeBase.RowPackCnt rowPackCnt;
			if (this.OSrcRow.TryGetValue(IR.Info, out rowPackCnt) && rowPackCnt.cached_is_packed > 0)
			{
				return NelItemManager.item_icon_2_packed(def_ico);
			}
			return def_ico;
		}

		public virtual string fnLunchDescAddition(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			if (row == UiItemManageBox.DESC_ROW.NAME)
			{
				int num = -1;
				if (Itm.RecipeInfo != null && Itm.RecipeInfo.DishInfo != null)
				{
					num = Itm.RecipeInfo.DishInfo.cost;
				}
				using (STB stb = TX.PopBld(def_string, 0))
				{
					if (!this.grade_on_desc_tab)
					{
						NelItem.getGradeMeshTxTo(stb, grade, 1, 38);
					}
					if (num >= 0)
					{
						stb.Add("<img mesh=\"recipe_cost_3\" width=\"22\" height=\"17\"/><font size=80%>", num, "</font>");
					}
					return stb.ToString();
				}
				return def_string;
			}
			return def_string;
		}

		protected virtual STB getIngredientDescPre(STB Stb, ItemStorage Inventory, ItemStorage.IRow AppliedIR)
		{
			if (AppliedIR.Data.is_food)
			{
				RCP.getDishDetailTo(Stb, AppliedIR.Data.RecipeInfo.DishInfo, "\n");
			}
			return Stb;
		}

		private void changeState(UiLunchTimeBase.STATE _stt)
		{
			int num = (int)this.stt;
			this.stt = _stt;
			if (num == 2)
			{
				this.ItemMng.need_blur_checked_row = true;
			}
			UiLunchTimeBase.STATE state = this.stt;
			if (state != UiLunchTimeBase.STATE.MAIN)
			{
				if (state == UiLunchTimeBase.STATE.EATING)
				{
					this.ItemMng.need_blur_checked_row = false;
					this.BxR.deactivate();
					this.initDesc(true);
					this.initCmd();
				}
				else
				{
					this.FbAft = null;
					this.drawBackground((float)this.af, true);
					this.need_fine_desc_fb = 0;
					this.af = -1;
					this.Stm.finePrStatus(false);
					this.deactivate(false);
				}
			}
			else
			{
				this.SelectedIR = null;
				this.SelectedDish = null;
				this.BxR.activate();
				this.BxCmd.deactivate();
				this.AppliedDish = null;
				this.AppliedIR = null;
				this.initDesc(true);
				aBtnItemRow selectingRowBtn = this.ItemMng.getSelectingRowBtn();
				if (selectingRowBtn != null)
				{
					this.ItemMng.BlurSelectingRowBtn();
					selectingRowBtn.Select(true);
				}
			}
			IN.clearPushDown(false);
		}

		private bool fnDishChoosen(UiItemManageBox IMng, UiBoxDesigner BxCmd, aBtnItemRow BRow)
		{
			NelItem itemData = BRow.getItemData();
			BRow.getItemInfo();
			if (this.stt != UiLunchTimeBase.STATE.MAIN)
			{
				return false;
			}
			SND.Ui.play("enter_small", false);
			if (this.AppliedIR == null)
			{
				return false;
			}
			this.SelectedIR = BRow.getItemRow();
			this.SelectedDish = ((itemData.RecipeInfo != null) ? itemData.RecipeInfo.DishInfo : null);
			if (this.AppliedDish != this.SelectedDish)
			{
				this.AppliedDish = this.SelectedDish;
				this.StmTemp.initForTemporary();
				if (this.AppliedDish != null)
				{
					this.StmTemp.addEffect(this.AppliedDish, false, true, false);
				}
				using (STB stb = TX.PopBld(null, 0))
				{
					this.FbCostStr.Txt(this.getCostStrTo(stb));
				}
			}
			this.changeState(UiLunchTimeBase.STATE.EATING);
			return false;
		}

		public void fnDetailPrepare(NelItem Itm, ItemStorage.ObtainInfo Obt, ItemStorage.IRow IR)
		{
			if (this.stt != UiLunchTimeBase.STATE.MAIN)
			{
				return;
			}
			this.StmTemp.initForTemporary();
			this.need_fine_desc_fb = (byte)X.Mx((int)this.need_fine_desc_fb, 1);
			this.AppliedIR = IR;
			this.AppliedDish = null;
			if (Itm.is_food)
			{
				RCP.RecipeDish recipeDish = ((Itm.RecipeInfo != null) ? Itm.RecipeInfo.DishInfo : null);
				if (recipeDish != null)
				{
					this.AppliedDish = recipeDish;
					this.StmTemp.addEffect(recipeDish, false, true, false);
				}
			}
		}

		private void fineDescFb()
		{
			bool flag = this.need_fine_desc_fb >= 2;
			this.need_fine_desc_fb = 0;
			this.FbDescTR.redraw_flag = true;
			bool flag2 = false;
			if (this.AppliedDish == null)
			{
				if (this.AppliedIR != null)
				{
					flag2 = true;
				}
				else
				{
					this.FbAft.text_content = "  ";
					if (this.desc_mode == UiLunchTimeBase.DESC.INGREDIENT)
					{
						this.FbPre.text_content = "  ";
					}
					flag = false;
				}
				this.FbDescTL.gameObject.SetActive(false);
			}
			else
			{
				this.FbDescTL.gameObject.SetActive(true);
				if (this.desc_mode == UiLunchTimeBase.DESC.STOMACH)
				{
					STB stb = TX.PopBld(null, 0);
					STB stb2 = TX.PopBld(null, 0);
					this.getEffectListupTo(stb2, this.StmTemp);
					stb.AddTxA("lunch_stomach_after", false);
					if (TX.valid(this.aft_header))
					{
						stb.Append(this.aft_header, "\n");
					}
					stb.Append(stb2, "\n", 0, -1);
					this.FbAft.Txt(stb);
					TX.ReleaseBld(stb2);
					TX.ReleaseBld(stb);
				}
				else
				{
					if (this.desc_mode == UiLunchTimeBase.DESC.INGREDIENT)
					{
						using (STB stb3 = TX.PopBld(null, 0))
						{
							this.FbPre.Txt(this.getIngredientDescPre(stb3, this.St, this.AppliedIR));
							this.FbAft.Txt(RCP.getIngredientListupTo(stb3.Clear(), this.AppliedDish, "\n"));
							goto IL_0169;
						}
					}
					flag2 = true;
				}
			}
			IL_0169:
			if (flag2)
			{
				if (this.AppliedIR == null)
				{
					goto IL_0330;
				}
				this.FbPre.margin_x = (this.FbPre.margin_y = 6f);
				this.FbAft.margin_x = (this.FbAft.margin_y = 6f);
				this.FbPre.fineTxTransform();
				this.FbAft.fineTxTransform();
				using (STB stb4 = TX.PopBld(null, 0))
				{
					this.getIngredientDescPre(stb4, this.St, this.AppliedIR);
					this.AppliedIR.Data.getDetail(stb4.Ret("\n"), this.St, (int)this.AppliedIR.splitted_grade, null, true, false, false);
					this.FbPre.Txt(stb4);
					stb4.Clear();
					this.AppliedIR.Data.getDescLocalized(stb4, this.St, (int)this.AppliedIR.splitted_grade);
					this.FbAft.Txt(stb4);
					goto IL_0330;
				}
			}
			if (this.stt == UiLunchTimeBase.STATE.MAIN)
			{
				this.FbPre.margin_x = (this.FbPre.margin_y = 0f);
				this.FbAft.margin_x = (this.FbAft.margin_y = 0f);
				this.FbPre.fineTxTransform();
				this.FbAft.fineTxTransform();
			}
			if (this.desc_mode == UiLunchTimeBase.DESC.STOMACH && flag)
			{
				using (STB stb5 = TX.PopBld(null, 0))
				{
					stb5.AddTxA("lunch_stomach_pre", false).Add("\n");
					this.getEffectListupTo(stb5, this.Stm);
					this.FbPre.Txt(stb5);
				}
			}
			IL_0330:
			this.BxDesc.getScrollBox().setScrollLevelTo(0f, 0f, false);
			this.BxDesc.getScrollBox().startAutoScroll(50);
			this.BxDesc.RowRemakeHeightRecalc(this.FbAft, null);
			this.BxDesc.RowRemakeHeightRecalc(this.FbPre, null);
			using (STB stb6 = TX.PopBld(null, 0))
			{
				this.FbCostStr.Txt(this.getCostStrTo(stb6));
			}
		}

		private bool FnDrawMainTR(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			update_meshdrawer = true;
			if (this.stt != UiLunchTimeBase.STATE.MAIN)
			{
				return true;
			}
			if (this.AppliedIR == null)
			{
				return false;
			}
			if (!this.AppliedIR.Data.is_food)
			{
				return true;
			}
			PxlFrame pf = MTRX.getPF("minicircle_4");
			PxlFrame pf2 = MTRX.getPF("minicircle_6");
			for (int i = 0; i < 3; i++)
			{
				float num = (float)((-1 + i) * 10);
				if (i == (int)this.desc_mode)
				{
					Md.Col = C32.MulA(4283780170U, alpha);
					Md.RotaPF(num, 0f, 1f, 1f, 0f, pf2, false, false, false, uint.MaxValue, false, 0);
				}
				else
				{
					Md.Col = C32.MulA(4283780170U, 0.45f * alpha);
					Md.RotaPF(num, 0f, 1f, 1f, 0f, pf, false, false, false, uint.MaxValue, false, 0);
				}
			}
			return true;
		}

		private string getCostStr()
		{
			string text;
			using (STB stb = TX.PopBld(null, 0))
			{
				this.getCostStrTo(stb);
				text = stb.ToString();
			}
			return text;
		}

		private STB getCostStrTo(STB Stb)
		{
			NelItem nelItem = null;
			if (this.stt == UiLunchTimeBase.STATE.MAIN)
			{
				nelItem = ((this.St.SelectedRow != null) ? this.St.SelectedRow.getItemData() : null);
			}
			else if (this.stt == UiLunchTimeBase.STATE.EATING)
			{
				nelItem = ((this.SelectedIR != null) ? this.SelectedIR.Data : null);
			}
			if (nelItem == null || !nelItem.is_food || this.StmTemp.cost_applied_level == 0f)
			{
				Stb.Add(this.Stm.cost_str).Add(" / ", this.Stm.cost_max, "");
			}
			else if (this.StmTemp.cost_applied_level >= 0f)
			{
				string text = "";
				if (this.StmTemp.cost_applied_level < 1f)
				{
					text = TX.Get("lunch_tag_burst", "");
				}
				Stb.Add(this.Stm.cost_str, " → ", text).Add(this.StmTemp.cost_str + " / ").Add(this.StmTemp.cost_max)
					.Add((text != "") ? "</font>" : "");
			}
			return Stb;
		}

		private string getEffectListup(Stomach Stm)
		{
			STB stb = TX.PopBld(null, 0);
			this.getEffectListupTo(stb, Stm);
			string text = stb.ToString();
			TX.ReleaseBld(stb);
			return text;
		}

		private STB getEffectListupTo(STB Stb, Stomach Stm)
		{
			string text = "\n";
			string text2 = TX.Get("lunch_tag_added", "");
			string text3 = TX.Get("lunch_tag_dupe", "");
			if (Stm == this.StmTemp)
			{
				STB stb = TX.PopBld(null, 0);
				if (this.Stm.applyableDoubleBonus(this.AppliedDish))
				{
					stb.AddTxA("lunch_effect_double", false);
					stb.TxRpl(1.5f);
				}
				if (this.StmTemp.cost_applied_level > 0f && this.StmTemp.isAnyEffectDuped())
				{
					stb.AppendTxA("lunch_suffix_dupe", "\n");
				}
				if (this.StmTemp.cost_applied_level >= 0f && this.StmTemp.cost_applied_level < 1f)
				{
					if (this.StmTemp.cost_applied_level == 0f)
					{
						stb.Clear().AddTxA("lunch_confirm_full_header", false);
					}
					else
					{
						stb.AppendTxA("lunch_cost_burst", "\n");
						stb.TxRpl(X.IntR(100f - 100f * this.StmTemp.cost_applied_level));
					}
				}
				this.aft_header = stb.ToString();
				TX.ReleaseBld(stb);
			}
			BDic<RCP.RPI_EFFECT, float> levelDictionary = Stm.getLevelDictionary01();
			if (levelDictionary.Count == 0)
			{
				Stb.Add("  ");
				Stb.Add((Stm == this.StmTemp) ? TX.Get("stomach_no_effect", "") : "--");
				return Stb;
			}
			STB stb2 = TX.PopBld(null, 0);
			foreach (KeyValuePair<RCP.RPI_EFFECT, float> keyValuePair in levelDictionary)
			{
				string text4 = "";
				string text5 = "";
				string text6 = "";
				int num = (int)(keyValuePair.Value * 100f);
				if (Stm == this.StmTemp)
				{
					if (this.Stm.getValue(keyValuePair.Key) < keyValuePair.Value)
					{
						text4 = text2;
					}
					if (this.Stm.getValue(keyValuePair.Key) > keyValuePair.Value)
					{
						text4 = NEL.error_tag;
					}
					if (Stm.isEffectDuped(keyValuePair.Key))
					{
						text6 = text3;
					}
					if (Stm.isSame(this.Stm, keyValuePair.Key))
					{
						text5 = "<font alpha=\"0.33\">";
					}
				}
				Stb.Add("  ", text5).AddTxA("Item_for_food_effect_content", false);
				stb2.Clear().AddTxA("recipe_effect_" + FEnum<RCP.RPI_EFFECT>.ToStr(keyValuePair.Key).ToLower(), false);
				Stb.TxRpl(stb2);
				stb2.Clear();
				stb2.Add(text6);
				stb2.Add(text4);
				RCP.getRPIEffectDescriptionTo(stb2, keyValuePair.Key, num, 1);
				if (text4 != "")
				{
					stb2.Add("</font>");
				}
				if (text6 != "")
				{
					stb2.Add("</font>");
				}
				Stb.TxRpl(stb2);
				Stb.Add(text);
				if (text5 != "")
				{
					Stb.Add("</font>");
				}
			}
			TX.ReleaseBld(stb2);
			return Stb;
		}

		private bool fnClickEatingCmd(aBtn B)
		{
			string title = B.title;
			if (title != null && title == "Eat")
			{
				RCP.Recipe recipe = ((this.SelectedDish != null) ? this.SelectedDish.Rcp : null);
				if (B.isLocked() || this.SelectedIR == null || !UiLunchTimeBase.isUseable(recipe) || !this.FD_EatExecute(this.SelectedDish) || !this.executeEat(B, this.SelectedIR, this.SelectedDish))
				{
					SND.Ui.play("locked", false);
					CURS.limitVib(B, AIM.L);
					return false;
				}
				this.SelectedDish = null;
				this.SelectedIR = null;
				this.changeState(UiLunchTimeBase.STATE.MAIN);
			}
			else
			{
				this.changeState(UiLunchTimeBase.STATE.MAIN);
			}
			return true;
		}

		protected virtual bool executeEat(aBtn B, ItemStorage.IRow IR, RCP.RecipeDish SelectedDish)
		{
			if (IR == null)
			{
				return false;
			}
			if (SelectedDish != null)
			{
				this.Stm.addEffect(SelectedDish, true, true, true);
				this.StmTemp.initForTemporary();
				if (SelectedDish.Rcp.eaten < 99)
				{
					RCP.Recipe rcp = SelectedDish.Rcp;
					rcp.eaten += 1;
				}
			}
			else
			{
				if (!IR.Data.useable)
				{
					return false;
				}
				IR.Data.Use(this.Stm.Pr, this.St, (int)IR.splitted_grade, this.Stm.Pr);
			}
			bool flag = false;
			UiLunchTimeBase.RowPackCnt rowPackCnt;
			if (this.OSrcRow.TryGetValue(IR.Info, out rowPackCnt))
			{
				flag = rowPackCnt.cached_is_packed > 0;
			}
			this.AEaten.Add(new UiLunchTimeBase.ConsumeResult(IR.Data, (int)IR.splitted_grade, flag));
			int splitted_grade = (int)IR.splitted_grade;
			if (!this.St.Reduce(IR.Data, 1, splitted_grade, true) && flag)
			{
				UiLunchTimeBase.RowPackCnt rowPackCnt2 = rowPackCnt;
				int num = rowPackCnt2.cached_is_packed - 1;
				rowPackCnt2.cached_is_packed = num;
				if (num <= 0)
				{
					this.St.fineRows(false);
				}
			}
			this.ItemMng.BlurSelectingRowBtn();
			this.executeEatAfter();
			SND.Ui.play("use_food", false);
			this.af_effect = 0;
			return true;
		}

		protected virtual void executeEatAfter()
		{
		}

		protected virtual void drawBackground(float af, bool force = false)
		{
		}

		protected virtual void runNoelImg(MeshDrawer MdNoel, ref bool need_redraw_noel)
		{
		}

		protected virtual bool drawNoelImg(MeshDrawer MdNoel, float alpha, ref bool need_redraw_noel)
		{
			return true;
		}

		public UiLunchTimeBase MK(float px, float py)
		{
			px = (px - 377.5f) * 1.125f;
			py = -(py - 478.5f) * 1.08f;
			this.MdNoel.Pos(px * 0.015625f * this.noel_scale, py * 0.015625f * this.noel_scale, null);
			return this;
		}

		protected void MKTri(int ver_pre)
		{
			int num = this.MdNoel.getVertexMax() - ver_pre;
			for (int i = 0; i < num; i++)
			{
				this.MdNoel.Tri(-num + (i + 1) % num, -num + i, 0, false);
			}
			this.MdNoel.Pos(0f, 0f, null);
		}

		public override bool isActive()
		{
			return this.stt > UiLunchTimeBase.STATE.NONE;
		}

		private void drawCostMeter(bool init = false)
		{
			if (init)
			{
				this.MdCostMask.base_px_x = (this.MdCost.base_px_x = -88f);
			}
			MeshDrawer meshDrawer = (init ? this.MdCostMask : this.MdCost);
			UiLunchTimeBase.drawCostMeterS(meshDrawer, init, this.Stm, this.StmTemp);
			meshDrawer.updateForMeshRenderer(false);
		}

		public static void drawCostMeterS(MeshDrawer Md, bool drawmask, Stomach Stm, Stomach StmTemp = null)
		{
			UiLunchTimeBase.drawCostMeterS(Md, drawmask, Stm.cost_max, Stm.getEatenCostArray(), (StmTemp != null) ? StmTemp.cost_applied_level : (-1f), (StmTemp != null) ? (StmTemp.cost - Stm.cost) : 0f);
		}

		public static void drawCostMeterS(MeshDrawer Md, int submesh_base, int submesh_block, int submesh_outline, float _alpha, Stomach Stm, Stomach StmTemp = null)
		{
			UiLunchTimeBase.drawCostMeterS(Md, submesh_base, submesh_block, submesh_outline, Stm.cost_max, _alpha, Stm.getEatenCostArray(), (StmTemp != null) ? StmTemp.cost_applied_level : (-1f), (StmTemp != null) ? (StmTemp.cost - Stm.cost) : 0f);
		}

		public static void drawCostMeterS(MeshDrawer Md, bool drawmask, int cost_max, List<Stomach.CostData> Aeaten_cost, float temp_applied_level = -1f, float temp_eaten_cost = 0f)
		{
			if (drawmask)
			{
				UiLunchTimeBase.drawCostMeterS(Md, -2, 0, 0, cost_max, 1f, Aeaten_cost, temp_applied_level, temp_eaten_cost);
				return;
			}
			UiLunchTimeBase.drawCostMeterS(Md, 0, 1, 2, cost_max, 1f, Aeaten_cost, temp_applied_level, temp_eaten_cost);
		}

		public static void drawCostMeterS(MeshDrawer Md, int submesh_base, int submesh_block, int submesh_outline, int cost_max, float _alpha, List<Stomach.CostData> Aeaten_cost, float temp_applied_level = -1f, float temp_eaten_cost = 0f)
		{
			float num = (float)((int)(UiLunchTimeBase.costbx_w * 0.5f - 30f) * 2) - 140f - 10f - 4f;
			float num2 = UiLunchTimeBase.costbx_h - 4f;
			float num3 = num * 0.5f;
			float num4 = num2 * 0.5f;
			float num5 = num - 48f;
			float num6 = num5 * 0.5f;
			float num7 = num3 - num5 * 0.5f;
			if (UiLunchTimeBase.PFCost == null)
			{
				UiLunchTimeBase.PFCost = MTRX.getPF("recipe_cost_3");
			}
			if (submesh_base < 0)
			{
				Md.Col = C32.MulA(2281701376U, _alpha);
				Md.KadomaruRect(num7, 0f, num5, num2, num4, 0f, false, 0f, 0f, false);
				return;
			}
			float num8 = 1f / (float)cost_max;
			Md.chooseSubMesh(submesh_base, false, false);
			Md.Col = C32.MulA(uint.MaxValue, _alpha);
			Md.RotaPF(-num3 + 14f, 0f, 2f, 2f, 0f, UiLunchTimeBase.PFCost, false, false, false, uint.MaxValue, false, 0);
			Md.chooseSubMesh(submesh_block, false, false);
			float num9 = 0f;
			int count = Aeaten_cost.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				num9 += Aeaten_cost[i].cost;
			}
			float num10 = (float)X.IntR(num5 * num9 * num8);
			int j;
			if (temp_applied_level == 0f)
			{
				Md.Col = Md.ColGrd.Set(4292558336U).mulA(_alpha * (0.6f + 0.4f * X.COSIT(140f))).C;
				Md.RectBL(-num6 + num7, -num4, num10, num2, false);
			}
			else
			{
				int num11 = -1;
				float num12 = -num6 + num7;
				for (j = 0; j < count; j++)
				{
					Stomach.CostData costData = Aeaten_cost[j];
					int num13 = (costData.is_water ? 2 : 0) | (costData.bonused ? 1 : 0);
					if (num11 != num13)
					{
						num11 = num13;
						switch (num11)
						{
						case 1:
							Md.ColGrd.Set(4289789820U).mulA(_alpha);
							Md.Col = C32.MulA(4294620235U, _alpha);
							break;
						case 2:
							Md.ColGrd.Set(4285366215U).mulA(_alpha);
							Md.Col = C32.MulA(4288065508U, _alpha);
							break;
						case 3:
							Md.ColGrd.Set(4281705720U).mulA(_alpha);
							Md.Col = C32.MulA(4284481535U, _alpha);
							break;
						default:
							Md.ColGrd.Set(4293190046U).mulA(_alpha);
							Md.Col = C32.MulA(4294620235U, _alpha);
							break;
						}
					}
					float num14 = (float)X.IntR(num5 * costData.cost * num8);
					Md.RectBLGradation(num12, -num4, num14, num2, GRD.LEFT2RIGHT, false);
					num12 += num14;
				}
			}
			if (temp_applied_level > 0f)
			{
				float num15 = X.Mn(num5 - num10, num5 * temp_eaten_cost * num8);
				Md.ColGrd.Set((temp_applied_level < 1f) ? 4292558336U : 4287561569U);
				Md.Col = Md.ColGrd.mulA(_alpha * (0.6f + 0.4f * X.COSIT((float)((temp_applied_level < 1f) ? 140 : 90)))).C;
				Md.RectBL(-num6 + num7 + num10, -num4, num15, num2, false);
			}
			j = 0;
			float num16 = num7 - num6;
			do
			{
				num16 += num5 * num8;
				j++;
				Md.Col = C32.MulA(uint.MaxValue, _alpha * ((j % 4 == 0) ? 1f : 0.2f));
				Md.RectBL(num16, -num4, 1f, num2, false);
			}
			while (j < cost_max);
			Md.chooseSubMesh(submesh_outline, false, false);
			Md.KadomaruRect(num7, 0f, num5, num2, num4, 1f, false, 0f, 0f, false);
		}

		internal void syncNoelPos2GobEfPos()
		{
			IN.Pos2(this.GobEf.transform, this.MdNoel.base_x, this.MdNoel.base_y);
		}

		public bool isActiveL()
		{
			return this.af >= 0;
		}

		public bool EvtWait(bool is_first = false)
		{
			if (is_first || this.isActive())
			{
				return true;
			}
			this.consumeEaten();
			return false;
		}

		private UiLunchTimeBase.STATE stt = UiLunchTimeBase.STATE.MAIN;

		public NelM2DBase M2D;

		private GameObject GobNoel;

		private MeshDrawer MdNoel;

		private GameObject GobEf;

		private MeshDrawer MdEf;

		private bool need_redraw_noel;

		protected float noel_scale = 0.75f;

		protected int INIT_DELAY = 40;

		protected string item_row_skin = "lunchtime";

		protected bool grade_on_desc_tab;

		protected bool alloc_not_food_on_row;

		private const float line_spacing = 1.3f;

		private const float desc_w = 360f;

		private readonly float out_w = IN.w * 0.69f;

		private readonly float right_w = IN.w * 0.69f - 360f - 12f;

		private readonly float out_h = IN.h * 0.8f;

		private readonly float right_h = IN.h * 0.8f - 40f;

		private readonly float right_h_margin = 12f;

		private readonly float cmd_w = IN.w * 0.69f;

		private readonly float cmd_h = 110f;

		public static readonly float costbx_h = 30f;

		private const float costbx_margin_lr = 30f;

		private const float costbx_margin_tb = 2f;

		private const float costbx_margin_x = 10f;

		private const float cost_str_w = 140f;

		private readonly float shiftx = IN.w * 0.13f;

		private readonly string UiItemStorestr_arrow5_icon;

		private UiBoxDesigner BxR;

		private UiBoxDesigner BxDesc;

		private UiBoxDesigner BxCmd;

		private UiBoxDesigner BxCost;

		private Stomach Stm;

		private Stomach StmTemp;

		private ItemStorage St;

		protected List<List<NelItem>> AAExternal;

		private List<UiLunchTimeBase.ConsumeResult> AEaten;

		private List<ItemStorage> AInventorySrc = new List<ItemStorage>();

		private UiItemManageBox ItemMng;

		protected Designer ItemTab;

		private ItemStorage.IRow SelectedIR;

		private ItemStorage.IRow AppliedIR;

		private RCP.RecipeDish SelectedDish;

		private RCP.RecipeDish AppliedDish;

		private string aft_header = "";

		private FillBlock FbPre;

		private FillBlock FbAft;

		private FillBlock FbCostStr;

		private FillBlock FbDescTL;

		private FillImageBlock FbDescTR;

		private MultiMeshRenderer MMRDCost;

		private MeshDrawer MdCostMask;

		private MeshDrawer MdCost;

		private static PxlFrame PFCost;

		private byte need_fine_desc_fb;

		private UiLunchTimeBase.DESC desc_mode;

		protected bool load_my_inventory = true;

		private int af;

		protected float t_noel;

		private float desc_main_x;

		private float desc_main_y;

		private float desc_main_w;

		private float desc_main_h;

		private int af_effect = -1;

		public string default_select_item_key;

		private BDic<ItemStorage.ObtainInfo, UiLunchTimeBase.RowPackCnt> OSrcRow;

		private string stabilize_key;

		public UiLunchTimeBase.FnEatExecute FD_EatExecute = (RCP.RecipeDish _AD) => true;

		public delegate bool FnEatExecute(RCP.RecipeDish AppliedDish);

		public enum STATE
		{
			NONE,
			MAIN,
			EATING
		}

		private enum DESC
		{
			STOMACH,
			INGREDIENT,
			ITEMDATA,
			_MAX
		}

		private sealed class RowPackCnt
		{
			public RowPackCnt(ItemStorage.IRow _SrcRow, ItemStorage _St)
			{
				this.Row = _SrcRow;
				this.St = _St;
			}

			public readonly ItemStorage.IRow Row;

			public readonly ItemStorage St;

			public int count;

			public int cached_is_packed;
		}

		private struct ConsumeResult
		{
			public ConsumeResult(NelItem _Itm, int _grade, bool _packed_food)
			{
				this.Itm = _Itm;
				this.grade = _grade;
				this.packed_food = _packed_food;
			}

			public readonly NelItem Itm;

			public readonly int grade;

			public bool packed_food;
		}
	}
}
