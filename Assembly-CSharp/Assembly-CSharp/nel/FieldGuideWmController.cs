using System;
using System.Collections.Generic;
using Better;
using nel.gm;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class FieldGuideWmController : UiWmSkinController
	{
		public ButtonSkinWholeMapArea WmSkin { get; private set; }

		public FieldGuideWmController(NelM2DBase _M2D, Func<bool> _Fn_is_active)
			: base(_M2D)
		{
			this.always_unselectable = true;
			this.auto_hide_wa_switch_tx = true;
			this.Fn_is_active = _Fn_is_active;
			this.icon_base_alpha = 0.25f;
			this.OItmTreasureCache = new BDic<NelItem, ReelManager.ReelDecription>();
			this.OIRDesc = new BDic<ReelManager.ItemReelContainer, SupplyManager.SupplyDescription>(4);
			this.IRDesc_Merged = new SupplyManager.SupplyDescriptionMulti();
			base.ClearFocusTarget();
		}

		public FieldGuideWmController createUiTo(Designer BxD, float swidth, float sheight)
		{
			float num = -1000f;
			float num2 = -1000f;
			WholeMapItem curWM = this.M2D.WM.CurWM;
			curWM.fixPlayerPosOnWM(this.M2D.getPrNoel(), ref num, ref num2);
			aBtnNelMapArea aBtnNelMapArea = BxD.addButtonT<aBtnNelMapArea>(new DsnDataButton
			{
				name = "map_area",
				title = "map_area",
				skin = "whole_map_area",
				w = swidth,
				h = sheight,
				fnDown = delegate(aBtn B)
				{
					if (this.isActive())
					{
						this.WmSkin.dragInit();
					}
					return true;
				}
			});
			this.WmSkin = aBtnNelMapArea.get_Skin() as ButtonSkinWholeMapArea;
			this.WmSkin.show_topleft_text = false;
			this.WmSkin.inner_margin = 15f;
			this.WmSkin.setWholeMapTarget(curWM, num, num2);
			this.WmSkin.topRightTxVisible(false);
			base.initAppear(this.WmSkin, num, num2);
			this.WmSkin.FnQuitDragging = new Action<bool, bool, bool>(base.fineDragAfter);
			this.WmSkin.icon_base_alpha = 0.25f;
			ButtonSkinWholeMapArea wmSkin = this.WmSkin;
			wmSkin.FD_FnDrawIcon = (ButtonSkinWholeMapArea.FnDrawIcon)Delegate.Combine(wmSkin.FD_FnDrawIcon, new ButtonSkinWholeMapArea.FnDrawIcon(this.fnDrawDetailWholeMapIcon));
			return this;
		}

		public void clearTarget()
		{
			this.DetailMapSmn = null;
			this.DetailMapIR = null;
			this.DetailCurIR = null;
			this.PickupTarget = null;
			this.need_recreate_detail_IR = true;
		}

		public override bool runAppearing()
		{
			bool flag = base.runAppearing();
			if (this.WmSkin != null)
			{
				this.WmSkin.getWholeMapTarget().prepareAllVisitted(true);
			}
			if (this.WmSkin != null && this.WmSkin.is_detail)
			{
				WholeMapItem wholeMapTarget = this.WmSkin.getWholeMapTarget();
				if (this.DetailCurrentWM != wholeMapTarget)
				{
					this.DetailCurrentWM = wholeMapTarget;
					this.need_recreate_detail_IR = true;
				}
				if (this.need_recreate_detail_IR)
				{
					this.need_recreate_detail_IR = false;
					this.WmSkin.fine_flag = true;
					this.OIRDesc.Clear();
					this.IRDesc_Merged.Clear();
					if (this.DetailBaseIR != null)
					{
						SupplyManager.SupplyDescription supplyDescription = (this.OIRDesc[this.DetailBaseIR] = SupplyManager.listupForIR(this.M2D, wholeMapTarget, this.DetailBaseIR, default(SupplyManager.SupplyDescription), true));
						this.IRDesc_Merged.Merge(supplyDescription);
					}
					object obj = this.PickupTarget;
					if (obj is NelItem)
					{
						NelItem nelItem = obj as NelItem;
						ReelManager.ReelDecription reelDecription;
						if (nelItem != null && this.OItmTreasureCache.TryGetValue(nelItem, out reelDecription))
						{
							this.IRDesc_Merged.Merge(reelDecription, this.OIRDesc, wholeMapTarget);
						}
					}
					EnemySummonerManager enemySummonerManager = null;
					if (obj is ENEMYID)
					{
						enemySummonerManager = enemySummonerManager ?? EnemySummonerManager.GetManager(wholeMapTarget.text_key);
						if (enemySummonerManager != null)
						{
							obj = enemySummonerManager.getAppearList((ENEMYID)obj);
						}
					}
					if (obj is SummonerList)
					{
						enemySummonerManager = enemySummonerManager ?? EnemySummonerManager.GetManager(wholeMapTarget.text_key);
						if (enemySummonerManager != null)
						{
							enemySummonerManager.listupAll();
							SummonerList summonerList = obj as SummonerList;
							int count = summonerList.Count;
							for (int i = 0; i < count; i++)
							{
								string text = summonerList[i];
								if (enemySummonerManager.getSummonerDescription(text, true).valid)
								{
									SupplyManager.SupplyDescription supplyDescription2 = SupplyManager.AddSummoner(this.M2D, wholeMapTarget, text, default(SupplyManager.SupplyDescription), !this.show_target_without_know_icon);
									this.IRDesc_Merged.Merge(supplyDescription2);
									this.IRDesc_Merged.is_summoner_target = true;
								}
							}
						}
					}
					if (obj is FDSummonerInfo)
					{
						enemySummonerManager = enemySummonerManager ?? EnemySummonerManager.GetManager(wholeMapTarget.text_key);
						if (enemySummonerManager != null)
						{
							string summoner_key = (obj as FDSummonerInfo).summoner_key;
							if (enemySummonerManager.getSummonerDescription(summoner_key, true).valid)
							{
								SupplyManager.SupplyDescription supplyDescription3 = SupplyManager.AddSummoner(this.M2D, wholeMapTarget, summoner_key, default(SupplyManager.SupplyDescription), !this.show_target_without_know_icon);
								this.IRDesc_Merged.Merge(supplyDescription3);
								this.IRDesc_Merged.is_summoner_target = true;
							}
						}
					}
					this.IRDesc_Merged.pickupFocusPosition(wholeMapTarget, base.ClearFocusTarget());
					return true;
				}
			}
			return flag;
		}

		public bool fineMapKD(FillBlock DMapKD, bool no_return = false, bool show_ir_detail = true)
		{
			this.DetailMapSmn = null;
			this.DetailMapIR = null;
			if (this.isActive())
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					bool flag = this.fineMapKD(stb, no_return, show_ir_detail);
					DMapKD.Txt(stb);
					return flag;
				}
				return false;
			}
			return false;
		}

		public bool fineMapKD(STB Stb, bool no_return, bool show_ir_detail = true)
		{
			this.DetailMapSmn = null;
			this.DetailMapIR = null;
			if (aBtn.PreSelected != this.WmSkin.getBtn())
			{
				Stb.AddTxA("KD_map_scroll", false);
				return false;
			}
			bool flag = false;
			if (this.WmSkin.is_detail)
			{
				if (show_ir_detail)
				{
					NightController.SummonerData summonerData;
					WMIcon wmicon;
					this.DetailMapSmn = base.getCurrentFocusEnemySummoner(out summonerData, out wmicon);
					if (this.DetailMapSmn != null)
					{
						Stb.Add(" <key ltab/>");
						Stb.AddTxA("KD_enemyspot", false);
						flag = true;
					}
					string text;
					ReelManager.ItemReelContainer focusIRTargetOnCursor = this.getFocusIRTargetOnCursor(out text);
					if (focusIRTargetOnCursor != null)
					{
						this.detail_map_ir_map = text;
						this.DetailMapIR = focusIRTargetOnCursor;
						Stb.Add("<key submit/>").AddTxA("KD_reel_detail", false);
						flag = true;
					}
				}
				if (flag && !no_return)
				{
					Stb.Ret("\n");
				}
			}
			base.fineMapKD(Stb);
			return flag;
		}

		public void fineCurCR(ReelManager.ItemReelContainer IR)
		{
			ReelManager.ItemReelContainer itemReelContainer = IR ?? this.DetailBaseIR;
			if (this.DetailCurIR != itemReelContainer)
			{
				this.DetailCurIR = itemReelContainer;
			}
		}

		private ReelManager.ItemReelContainer getFocusIRTargetOnCursor(out string map_key)
		{
			WholeMapItem detailCurrentWM = this.DetailCurrentWM;
			Vector2 cursorMapPos = this.WmSkin.getCursorMapPos();
			float num = (float)((int)cursorMapPos.x) + 0.5f;
			float num2 = (float)((int)cursorMapPos.y) + 0.5f;
			float num3 = 1.5f;
			ReelManager.ItemReelContainer itemReelContainer = null;
			map_key = null;
			using (BList<MapPosition> blist = ListBuffer<MapPosition>.Pop(0))
			{
				foreach (KeyValuePair<WmPosition, List<ReelManager.ItemReelContainer>> keyValuePair in this.IRDesc_Merged.OAPosSpl)
				{
					blist.Clear();
					if (keyValuePair.Key.getPos(this.M2D, detailCurrentWM, blist))
					{
						float num4 = X.LENGTHXYN(num, num2, blist[0].x, blist[0].y);
						if (num4 < num3)
						{
							num3 = num4;
							itemReelContainer = ((this.DetailCurIR != null && keyValuePair.Value.IndexOf(this.DetailCurIR) >= 0) ? this.DetailCurIR : keyValuePair.Value[0]);
							map_key = keyValuePair.Key.Wmi.SrcMap.key;
						}
					}
				}
			}
			foreach (KeyValuePair<WMIconPosition, List<ReelManager.ItemReelContainer>> keyValuePair2 in this.IRDesc_Merged.OAPosSmn)
			{
				WMIconPosition key = keyValuePair2.Key;
				float num5 = X.LENGTHXYN(num, num2, key.wmx, key.wmy);
				if (num5 < num3)
				{
					num3 = num5;
					itemReelContainer = ((this.DetailCurIR != null && keyValuePair2.Value.IndexOf(this.DetailCurIR) >= 0) ? this.DetailCurIR : keyValuePair2.Value[0]);
					map_key = keyValuePair2.Key.getDepertureMap().key;
				}
			}
			return itemReelContainer;
		}

		public bool getDescPointAverage(out Vector2 V)
		{
			V = Vector2.zero;
			WholeMapItem detailCurrentWM = this.DetailCurrentWM;
			if (detailCurrentWM == null)
			{
				return false;
			}
			int num = 0;
			using (BList<MapPosition> blist = ListBuffer<MapPosition>.Pop(0))
			{
				if (this.IRDesc_Merged.OAPosSpl.Count > 0)
				{
					foreach (KeyValuePair<WmPosition, List<ReelManager.ItemReelContainer>> keyValuePair in this.IRDesc_Merged.OAPosSpl)
					{
						blist.Clear();
						if (keyValuePair.Key.getPos(this.M2D, detailCurrentWM, blist))
						{
							V.x += blist[0].x;
							V.y += blist[0].y;
							num++;
						}
					}
				}
				if (this.IRDesc_Merged.OAPosSmn.Count > 0)
				{
					foreach (KeyValuePair<WMIconPosition, List<ReelManager.ItemReelContainer>> keyValuePair2 in this.IRDesc_Merged.OAPosSmn)
					{
						WMIconPosition key = keyValuePair2.Key;
						V.x += key.wmx;
						V.y += key.wmy;
						num++;
					}
				}
			}
			if (num > 0)
			{
				V.x /= (float)num;
				V.y /= (float)num;
				return true;
			}
			return false;
		}

		public static float map2meshx(float mappos_x, float center_mapx, float size)
		{
			return WholeMapItem.map2meshx(mappos_x, center_mapx, size);
		}

		public static float map2meshy(float mappos_y, float center_mapy, float size)
		{
			return WholeMapItem.map2meshy(mappos_y, center_mapy, size);
		}

		public void fnDrawDetailWholeMapIcon(ButtonSkinWholeMapArea WmSkin, MeshDrawer MdIco, float blink_alpha, float mappos_x, float mappos_y, float cell_size)
		{
			if (!this.isActive() || this.DetailCurrentWM == null)
			{
				return;
			}
			WholeMapItem detailCurrentWM = this.DetailCurrentWM;
			float num = blink_alpha * blink_alpha;
			float num2 = X.Mn(1f, num * 2f);
			float num3 = num * 0.5f;
			if (this.IRDesc_Merged.OAPosSpl.Count > 0)
			{
				Color32 c = MdIco.ColGrd.Set(4278304339U).setA1(num2).C;
				Color32 c2 = MdIco.ColGrd.Set(uint.MaxValue).setA1(num2).C;
				Color32 color = C32.MulA(4278304339U, num3);
				Color32 color2 = C32.MulA(uint.MaxValue, num3);
				PxlFrame pf = MTRX.getPF("mbox_base_i");
				using (BList<MapPosition> blist = ListBuffer<MapPosition>.Pop(0))
				{
					foreach (KeyValuePair<WmPosition, List<ReelManager.ItemReelContainer>> keyValuePair in this.IRDesc_Merged.OAPosSpl)
					{
						blist.Clear();
						if (keyValuePair.Key.getPos(this.M2D, detailCurrentWM, blist))
						{
							float num4 = FieldGuideWmController.map2meshx(blist[0].x, mappos_x, cell_size);
							float num5 = FieldGuideWmController.map2meshy(blist[0].y, mappos_y, cell_size);
							bool flag = this.DetailCurIR == null || keyValuePair.Value.IndexOf(this.DetailCurIR) >= 0;
							MdIco.Col = (flag ? c : color);
							MdIco.initForImg(MTRX.EffCircle128, 0);
							MdIco.Rect(num4, num5, 32f, 32f, false);
							MdIco.Col = (flag ? c2 : color2);
							MdIco.RotaPF(num4, num5, 1f, 1f, 0f, pf, false, false, false, uint.MaxValue, false, 0);
						}
					}
				}
			}
			if (this.IRDesc_Merged.OAPosSmn.Count > 0)
			{
				Color32 c3 = MdIco.ColGrd.Set(4283780170U).setA1(num).C;
				Color32 color3 = C32.MulA(4283780170U, num3);
				foreach (KeyValuePair<WMIconPosition, List<ReelManager.ItemReelContainer>> keyValuePair2 in this.IRDesc_Merged.OAPosSmn)
				{
					WMIconPosition key = keyValuePair2.Key;
					float num6 = FieldGuideWmController.map2meshx(key.wmx, mappos_x, cell_size);
					float num7 = FieldGuideWmController.map2meshy(key.wmy, mappos_y, cell_size);
					if (this.IRDesc_Merged.is_summoner_target)
					{
						MdIco.Col = color3;
						MdIco.initForImg(MTRX.EffCircle128, 0);
						MdIco.Rect(num6, num7, 32f, 32f, false);
						MdIco.Col = MdIco.ColGrd.Set(MdIco.Col).Scr(C32.d2c(16777215U), 1f).ScrA(0.6f)
							.C;
						NEL.QuestNoticeExc(MdIco, num6, num7);
					}
					else
					{
						bool flag2 = this.DetailCurIR == null || keyValuePair2.Value.IndexOf(this.DetailCurIR) >= 0;
						MdIco.Col = (flag2 ? c3 : color3);
						MdIco.initForImg(MTRX.EffCircle128, 0);
						MdIco.Rect(num6, num7, 32f, 32f, false);
						((flag2 && this.DetailCurIR != null) ? this.DetailCurIR : keyValuePair2.Value[0]).drawSmallIcon(MdIco, num6, num7, flag2 ? num : num3, 1f, false);
					}
				}
			}
		}

		public object PickupTarget
		{
			get
			{
				return this.PickupTarget_;
			}
			set
			{
				if (this.PickupTarget == value)
				{
					return;
				}
				this.PickupTarget_ = value;
				this.need_recreate_detail_IR = true;
				if (this.PickupTarget_ is NelItem)
				{
					NelItem nelItem = this.PickupTarget as NelItem;
					ReelManager.ReelDecription reelDecription;
					if (!this.OItmTreasureCache.TryGetValue(nelItem, out reelDecription))
					{
						this.OItmTreasureCache[nelItem] = ReelManager.listupItemSupplier(nelItem, false);
					}
				}
			}
		}

		public ReelManager.ReelDecription getReelDescriptionForCurrentItem()
		{
			if (this.PickupTarget_ is NelItem)
			{
				return this.OItmTreasureCache[this.PickupTarget_ as NelItem];
			}
			return default(ReelManager.ReelDecription);
		}

		public bool isActive()
		{
			return this.Fn_is_active == null || this.Fn_is_active();
		}

		public bool is_detail
		{
			get
			{
				return this.WmSkin.is_detail;
			}
		}

		public bool fine_flag
		{
			get
			{
				return this.WmSkin.fine_flag;
			}
			set
			{
				this.WmSkin.fine_flag = value;
			}
		}

		public void bind()
		{
			this.WmSkin.getBtn().bind();
		}

		public void hide()
		{
			this.WmSkin.getBtn().hide();
		}

		private object PickupTarget_;

		private bool need_recreate_detail_IR = true;

		private WholeMapItem DetailCurrentWM;

		public EnemySummoner DetailMapSmn;

		public ReelManager.ItemReelContainer DetailMapIR;

		public ReelManager.ItemReelContainer DetailBaseIR;

		public ReelManager.ItemReelContainer DetailCurIR;

		private BDic<NelItem, ReelManager.ReelDecription> OItmTreasureCache;

		private SupplyManager.SupplyDescriptionMulti IRDesc_Merged;

		private BDic<ReelManager.ItemReelContainer, SupplyManager.SupplyDescription> OIRDesc;

		private Func<bool> Fn_is_active;

		public float icon_base_alpha;

		public string detail_map_ir_map;

		public bool show_target_without_know_icon;
	}
}
