using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel.mgm.smncr
{
	public class UiSmncEnemyUnlocker
	{
		public UiSmncEnemyUnlocker(UiSmncEnemyEditor _Con, UiBoxDesignerFamily _DsFam, M2LpUiSmnCreator _LpArea)
		{
			this.DsFam = _DsFam;
			this.Con = _Con;
			this.LpArea = _LpArea;
			this.M2D = this.LpArea.nM2D;
			this.DsFam.base_z -= 0.1f;
			this.Bx = this.DsFam.Create("EnemyUnlocker", 0f, 0f, 420f, 340f, 2, 40f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.Bx.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
			this.Bx.margin_in_lr = 20f;
			this.Bx.margin_in_tb = 34f;
			this.Bx.item_margin_y_px = 4f;
			this.Bx.deactivate(true);
		}

		public void destruct()
		{
		}

		public void createUiMain()
		{
			if (this.BConItm != null)
			{
				return;
			}
			this.Bx.init();
			DsnDataP dsnDataP = new DsnDataP("", false)
			{
				text = TX.Get("Smnc_ui_enemy_kind_unlock_how", ""),
				size = 14f,
				swidth = this.Bx.use_w,
				text_margin_x = 10f,
				alignx = ALIGN.LEFT,
				TxCol = C32.MulA(4283780170U, 0.5f)
			};
			this.P_Kind = this.Bx.addP(dsnDataP, false);
			this.Bx.Br();
			this.BItmCount = this.Bx.addButtonT<aBtnNel>(new DsnDataButton
			{
				title = "count",
				skin = "row",
				skin_title = " ",
				w = this.Bx.use_w,
				h = 24f,
				fnHover = new FnBtnBindings(this.fnHoverBtnMain)
			});
			this.Bx.Br();
			float num = this.Bx.use_h - 24f - this.Bx.item_margin_y_px - 4f;
			this.TabItem = this.Bx.addTab("NeedItem", this.Bx.use_w, num, this.Bx.use_w, num, false);
			this.TabItem.margin_in_lr = 8f;
			this.TabItem.margin_in_tb = 4f;
			this.TabItem.init();
			dsnDataP.text = TX.Get("Smnc_ui_enemy_kind_unlock_item", "");
			dsnDataP.text_margin_x = 18f;
			this.TabItem.addP(dsnDataP, false);
			this.TabItem.Br();
			this.BConItm = this.TabItem.addButtonMultiT<aBtnItemRow>(new DsnDataButtonMulti
			{
				name = "items",
				skin = "normal",
				fnGenerateKeys = new FnGenerateRemakeKeys(this.fnGenerateEENeedItems),
				w = this.TabItem.use_w,
				h = 24f,
				clms = 1,
				margin_h = 0f,
				APoolEvacuated = new List<aBtn>(4),
				fnHover = new FnBtnBindings(this.fnHoverBtnMain),
				fnClick = delegate(aBtn B)
				{
					this.activateItemMove();
					return true;
				}
			});
			this.Bx.endTab(false);
			this.Bx.Br();
			this.P_KD = this.Bx.addP(new DsnDataP("", false)
			{
				text = " ",
				alignx = ALIGN.RIGHT,
				swidth = this.Bx.use_w,
				html = true,
				size = 14f,
				TxCol = NEL.ColText,
				text_margin_x = 12f
			}, false);
			this.Bx.Br();
			Designer designer = this.Bx.addTab("_center", this.Bx.use_w, 52f, this.Bx.use_w, 52f, false);
			designer.alignx = ALIGN.CENTER;
			this.TabItem.margin_in_tb = 10f;
			designer.init();
			this.BCancel = designer.addButtonT<aBtnNel>(new DsnDataButton
			{
				title = "&&Cancel",
				skin = "normal",
				w = 193.2f,
				h = 28f,
				fnHover = new FnBtnBindings(this.fnHoverBtnMain),
				fnClick = delegate(aBtn B)
				{
					this.deactivate();
					this.Con.quitUnlocker(null);
					return true;
				}
			});
			this.Bx.endTab(true);
			this.BItmCount.setNaviT(this.BCancel, true, true);
		}

		private void fnGenerateEENeedItems(BtnContainerBasic BCon, List<string> A)
		{
			List<NelItemEntry> list = ((this.CurEE != null) ? this.CurEE.ANeedUnlockItems : null);
			if (list == null)
			{
				return;
			}
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				A.Add(i.ToString());
			}
		}

		public void activateTemp()
		{
			if (this.isActiveItemMove())
			{
				this.BxItm.activate();
				this.BxItm.auto_deactive_gameobject = true;
				return;
			}
			this.Bx.activate();
			this.Bx.Focus();
		}

		internal void CopyTerm(STB Stb, EnemyEntry _EE)
		{
			List<NelItemEntry> aneedUnlockItems = _EE.ANeedUnlockItems;
			int need_defeat_count = _EE.need_defeat_count;
			if (need_defeat_count > 0 && aneedUnlockItems != null)
			{
				this.CopyKind(Stb, _EE);
				if (need_defeat_count > 0)
				{
					Stb.Ret("\n");
					Stb.AddTxA("Smnc_ui_enemy_kind_unlock_count", false).TxRpl(need_defeat_count);
				}
				if (aneedUnlockItems != null)
				{
					Stb.Ret("\n");
					int count = aneedUnlockItems.Count;
					for (int i = 0; i < count; i++)
					{
						if (i > 0)
						{
							Stb.Add(", ");
						}
						NelItemEntry nelItemEntry = aneedUnlockItems[i];
						nelItemEntry.Data.getLocalizedName(Stb, (int)nelItemEntry.grade);
						Stb.Add(" x").Add(nelItemEntry.count);
					}
				}
			}
		}

		private STB CopyKind(STB Stb, EnemyEntry _EE)
		{
			NOD.BasicData nod = _EE.Nod;
			if (nod == null)
			{
				return Stb;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				int num = ((nod.Asmnc_family != null) ? nod.Asmnc_family.Count : 0);
				for (int i = ((nod.Asmnc_family != null) ? 0 : (-1)); i < num; i++)
				{
					if (i > 0)
					{
						stb.Add(" / ");
					}
					ENEMYID enemyid;
					if (i < 0)
					{
						enemyid = _EE.id;
					}
					else
					{
						enemyid = nod.Asmnc_family[i] | (_EE.is_od ? ((ENEMYID)2147483648U) : ((ENEMYID)0U));
					}
					stb.Add(NDAT.getEnemyName(enemyid, true));
				}
				Stb.AddTxA("Smnc_ui_enemy_kind_unlock_how", false).TxRpl(stb);
			}
			return Stb;
		}

		internal void activateMain(EnemyEntry _CurEE)
		{
			this.CurEE = _CurEE;
			this.createUiMain();
			this.Bx.activate();
			aBtn btn = this.CurEE.Sk.getBtn();
			Transform transform = this.CurEE.Sk.getBtn().transform;
			this.Bx.positionD(transform.position.x * 64f + btn.get_swidth_px() * 1.25f, transform.position.y * 64f, 2, 50f);
			using (STB stb = TX.PopBld(null, 0))
			{
				this.P_Kind.Txt(this.CopyKind(stb, this.CurEE));
				stb.Clear();
				ButtonSkinRow buttonSkinRow = this.BItmCount.get_Skin() as ButtonSkinRow;
				stb.AddTxA("Smnc_ui_enemy_kind_unlock_count", false).TxRpl(this.CurEE.need_defeat_count);
				stb.Add("  ").AddTxA("Smnc_ui_enemy_kind_unlock_count_rest", false).TxRpl(this.CurEE.need_defeat_count - this.CurEE.defeat_count);
				buttonSkinRow.setTitleTextS(stb);
				buttonSkinRow.row_left_px = 24;
			}
			this.TabItem.gameObject.SetActive(true);
			this.BConItm.RemakeT<aBtnItemRow>(null, "normal");
			DesignerRowMem.DsnMem designerBlockMemory = this.Bx.getDesignerBlockMemory(this.TabItem);
			if (this.BConItm.Length == 0)
			{
				designerBlockMemory.active = false;
				this.TabItem.gameObject.SetActive(false);
				this.BItmCount.setNaviB(this.BCancel, true, true);
			}
			else
			{
				designerBlockMemory.active = true;
				this.TabItem.reboundCarrForBtnMulti(this.BConItm, true);
				this.TabItem.rowRemakeCheck(false);
				this.TabItem.cropBounds(this.TabItem.w, 0f);
				this.Bx.RowRemakeHeightRecalc(this.TabItem, null);
				this.BItmCount.setNaviB(this.BConItm.Get(0), true, true);
				this.BCancel.setNaviT(this.BConItm.Get(this.BConItm.Length - 1), true, true);
				int length = this.BConItm.Length;
				List<NelItemEntry> aneedUnlockItems = this.CurEE.ANeedUnlockItems;
				for (int i = 0; i < length; i++)
				{
					ButtonSkinItemRow buttonSkinItemRow = this.BConItm.Get(i).get_Skin() as ButtonSkinItemRow;
					NelItemEntry nelItemEntry = aneedUnlockItems[i];
					ItemStorage.ObtainInfo obtainInfo = new ItemStorage.ObtainInfo(nelItemEntry);
					ItemStorage.IRow row = new ItemStorage.IRow(nelItemEntry.Data, obtainInfo, false)
					{
						total = obtainInfo.total
					};
					buttonSkinItemRow.setItem(null, null, row);
				}
			}
			this.Bx.row_remake_flag = true;
			this.Bx.rowRemakeCheck(false);
			this.Bx.cropBounds(this.Bx.w, 0f);
			this.Bx.Focusable(true, true, null);
			this.Bx.Focus();
			this.BItmCount.Select(true);
		}

		public void deactivate()
		{
			IN.clearPushDown(true);
			this.Bx.deactivate();
			if (this.BxItm != null)
			{
				this.BxItm.deactivate(false);
			}
		}

		private bool fnHoverBtnMain(aBtn B)
		{
			using (STB stb = TX.PopBld(null, 0))
			{
				if (B is aBtnItemRow)
				{
					stb.Add("<key submit/>").AddTxA("Guild_Btn_deliver_ask", false);
				}
				if (this.M2D.IMNG.has_recipe_collection)
				{
					stb.AppendTxA("Guild_Btn_recipebook", " ");
				}
				this.P_KD.Txt(stb);
			}
			return true;
		}

		public void activateItemMove()
		{
			if (this.CurEE == null)
			{
				return;
			}
			List<NelItemEntry> aneedUnlockItems = this.CurEE.ANeedUnlockItems;
			if (aneedUnlockItems == null)
			{
				return;
			}
			if (this.BxItm == null)
			{
				this.BxItm = IN.CreateGobGUI(this.DsFam.gameObject, "-BxItm").AddComponent<UiItemConfirm>();
				this.BxItm.FD_fnItemConfirmFinished = new UiItemConfirm.FnItemConfirmFinished(this.finishItemDeliver);
				this.BxItm.auto_deactive_gameobject = true;
			}
			this.Bx.deactivate();
			this.Con.deactivateTemp();
			this.BxItm.InitItemConfirm(this.M2D, aneedUnlockItems);
		}

		private bool finishItemDeliver(NelItemEntry IE, bool item_delivered)
		{
			if (item_delivered)
			{
				this.deactivate();
				this.Con.quitUnlocker(this.CurEE);
			}
			else
			{
				this.Bx.activate();
				this.Con.activateTemp();
				this.Bx.Focus();
				this.BConItm.Get(0).Select(true);
			}
			return true;
		}

		public bool isActiveItemMove()
		{
			return this.BxItm != null && this.BxItm.isActive();
		}

		public void deactivateTemp()
		{
			bool flag = this.isActiveItemMove();
			this.Bx.deactivate();
			if (this.BxItm != null && flag)
			{
				this.BxItm.deactivateTemp();
			}
		}

		public void runMain(float fcnt, bool force_cancel = false)
		{
			if (this.isActiveItemMove())
			{
				this.BxItm.runItemMove(fcnt);
				return;
			}
			if (IN.isCancel() || !this.Bx.isFocused() || force_cancel)
			{
				this.BCancel.ExecuteOnSubmitKey();
				return;
			}
		}

		public bool isTransition()
		{
			return this.BxItm != null && this.BxItm.isTransition();
		}

		public bool fineFieldGuideReveal()
		{
			if (this.BConItm != null)
			{
				if (this.isActiveItemMove())
				{
					NelItemEntry curEntry = this.BxItm.CurEntry;
					if (curEntry == null)
					{
						return false;
					}
					UiFieldGuide.NextRevealAtAwake = curEntry.Data;
					return true;
				}
				else if (aBtn.PreSelected is aBtnItemRow)
				{
					ButtonSkinItemRow buttonSkinItemRow = aBtn.PreSelected.get_Skin() as ButtonSkinItemRow;
					if (buttonSkinItemRow != null)
					{
						UiFieldGuide.NextRevealAtAwake = buttonSkinItemRow.getItemData();
						return true;
					}
				}
			}
			return false;
		}

		public override string ToString()
		{
			return "UiSmncEnemyUnlocker";
		}

		public Map2d Mp
		{
			get
			{
				return this.LpArea.Mp;
			}
		}

		public readonly UiSmncEnemyEditor Con;

		public readonly NelM2DBase M2D;

		public readonly UiBoxDesigner Bx;

		public readonly UiBoxDesignerFamily DsFam;

		private readonly M2LpUiSmnCreator LpArea;

		private UiItemConfirm BxItm;

		private EnemyEntry CurEE;

		private const float bx_w = 420f;

		private const float bx_h = 340f;

		private const float btn_h = 24f;

		private const float cancelbtn_h = 28f;

		private BtnContainer<aBtn> BConItm;

		private aBtn BItmCount;

		private aBtn BCancel;

		private Designer TabItem;

		private FillBlock P_KD;

		private FillBlock P_Kind;
	}
}
