using System;
using System.Collections.Generic;
using Better;
using evt;
using UnityEngine;
using XX;

namespace nel
{
	public class UiGQManageBox : UiItemManageBox, IEventListener, IEventWaitListener, IRunAndDestroy
	{
		public float outw
		{
			get
			{
				return UiGQCard.defaultw + UiGuildReelBox.smallw + 14f;
			}
		}

		public float mainw
		{
			get
			{
				return UiGQCard.defaultw;
			}
		}

		public float outh
		{
			get
			{
				return IN.h - 80f;
			}
		}

		public float mainx
		{
			get
			{
				return -this.outw * 0.5f + this.mainw * 0.5f;
			}
		}

		public float reelx
		{
			get
			{
				return this.outw * 0.5f - UiGuildReelBox.smallw * 0.5f;
			}
		}

		public float toph
		{
			get
			{
				return UiGQCard.defaulth;
			}
		}

		public float topy
		{
			get
			{
				return this.outh * 0.5f - this.toph * 0.5f;
			}
		}

		public float btmh
		{
			get
			{
				return this.outh - this.toph - 16f;
			}
		}

		public float btmy
		{
			get
			{
				return -this.outh * 0.5f + this.btmh * 0.5f;
			}
		}

		public float lbw
		{
			get
			{
				return this.mainw * 0.7f;
			}
		}

		public float lbx
		{
			get
			{
				return -this.outw * 0.5f + this.lbw * 0.5f;
			}
		}

		public float rbw
		{
			get
			{
				return this.mainw - this.lbw - 10f;
			}
		}

		public float rbx
		{
			get
			{
				return -this.outw * 0.5f + this.mainw - this.rbw * 0.5f;
			}
		}

		public float confirm_w
		{
			get
			{
				return IN.w * 0.82f;
			}
		}

		public UiGQManageBox(NelM2DBase _M2D, string _enable_gq_key, bool _digesting, Transform _TrsEvacuateTo = null)
			: base(_M2D.IMNG, _TrsEvacuateTo)
		{
			this.enable_gq_key = _enable_gq_key;
			this.digesting = _digesting;
			this.M2D = _M2D;
			this.stabilize_key = "_GUILD" + IN.totalframe.ToString();
			this.M2D.FlagValotStabilize.Add(this.stabilize_key);
			this.M2D.GUILD.checkFineGQ(this.digesting ? "" : this.enable_gq_key);
			this.Gob = IN.CreateGobGUI(null, "GUILDQUEST");
			this.use_grade_stars = false;
			IN.setZ(this.Gob.transform, -3.65f);
			this.DsFam = this.Gob.AddComponent<UiBoxDesignerFamily>();
			this.DsFam.enabled = false;
			this.DsFam.auto_deactive_gameobject = false;
			this.createUi();
			IN.addRunner(this);
			EV.addListener(this);
			EV.initWaitFn(this, 0);
			EV.getVariableContainer().define("_newquest", "0", true);
			this.checkDigesting();
		}

		public void destruct()
		{
			this.M2D.FlagValotStabilize.Rem(this.stabilize_key);
			if (this.RecipeBook != null)
			{
				IN.DestroyOne(this.RecipeBook.gameObject);
				this.RecipeBook = null;
			}
			IN.DestroyE(this.Gob);
		}

		private void createUi()
		{
			int num = 0;
			int num2 = 0;
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				this.AStorage = new List<ItemStorage>(3);
				this.AAEntry = new List<GuildManager.GQEntryList>(3);
				if (!this.digesting)
				{
					foreach (KeyValuePair<string, GuildManager.GQEntryList> keyValuePair in this.M2D.GUILD.getCurrentEntryWholeObject())
					{
						if (keyValuePair.Value.gq_enable_type == this.enable_gq_key)
						{
							this.AAEntry.Add(keyValuePair.Value);
							ItemStorage itemStorage = new ItemStorage("St_" + keyValuePair.Key, 20)
							{
								infinit_stockable = true,
								grade_split = true,
								water_stockable = true
							};
							this.AStorage.Add(itemStorage);
							keyValuePair.Value.copyRowsToStorage(keyValuePair.Key, itemStorage);
							WholeMapItem byTextKey = this.M2D.WM.GetByTextKey(keyValuePair.Key);
							blist.Add(byTextKey.localized_name);
						}
					}
				}
				this.StBuy = new ItemStorage("StBuy", 20)
				{
					infinit_stockable = true,
					grade_split = true,
					water_stockable = true
				};
				foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair2 in this.IMNG.getInventoryPrecious().getWholeInfoDictionary())
				{
					if (TX.isStart(keyValuePair2.Key.key, "__guildquest_", 0))
					{
						GuildManager.GQEntry gqEntry = this.getGqEntry(keyValuePair2.Key);
						if (gqEntry != null && (!this.digesting || gqEntry.Qt != null))
						{
							this.StBuy.Add(keyValuePair2.Key, 1, keyValuePair2.Value.top_grade, true, true);
						}
					}
				}
				this.recalcObtaining(out num, out num2);
				this.StBuy.fineRows(false);
				this.AStorage.Add(this.StBuy);
				blist.Add("&&Guild_tab_current_prgoress");
				this.DsT = this.DsFam.CreateT<UiGQCard>("DsT", this.mainx, this.topy, this.mainw, this.toph, 3, 440f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.DsT.M2D = this.M2D;
				this.DsLB = this.DsFam.Create("DsLB", this.lbx, this.btmy, this.lbw, this.btmh, 2, 340f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.DsRB = this.DsFam.CreateT<UiGQPointBox>("DsRB", this.rbx, this.btmy, this.rbw, this.btmh, 0, 540f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.DsRB.Con = this;
				this.DsReel = this.DsFam.CreateT<UiGuildReelBox>("DsReel", this.reelx, 0f, UiGuildReelBox.smallw, this.outh, 0, 320f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.DsReel.InitReelBox(this.M2D, this.enable_gq_key);
				this.DsReel.FD_Quit = new Func<bool, bool>(this.fnReelBoxQuit);
				this.DsFam.base_z -= 0.25f;
				this.DsCmd = this.DsFam.CreateT<UiGuildDepartBox>("DsCmd", 0f, 0f, UiGuildDepartBox.outw, UiGuildDepartBox.outh, 3, 20f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.DsCmd.auto_destruct_when_deactivate = false;
				this.DsCmd.FnClickedCmd = new FnBtnBindings(this.fnClickDepertCmd);
				this.DsCmd.cancelable = false;
				this.DsCmd.deactivate();
				this.DsLB.margin_in_lr = 30f;
				this.DsLB.margin_in_tb = 8f;
				this.DsLB.item_margin_x_px = 0f;
				this.DsLB.item_margin_y_px = 0f;
				this.DsLB.init();
				this.RTabBar = ColumnRowNel.NCreateT<aBtnNel>(this.DsLB, "ctg_tab", "row_tab", 0, blist.ToArray(), new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnItemTabChanged), this.DsLB.use_w, 0f, false, false);
				this.RTabBar.LR_valotile = true;
				this.RTabBar.LrInput(true);
				this.DsLB.Br();
				this.TabLBMain = this.DsLB.addTab("status_area", this.DsLB.use_w, this.DsLB.use_h - 4f, this.DsLB.use_w, this.DsLB.use_h - 4f, false);
				this.TabLBMain.Smallest();
				this.TabLBMain.margin_in_tb = 6f;
				this.DsLB.endTab(false);
			}
			this.DsRB.createUi(this.M2D, true);
			this.DsRB.gp_obtained = num;
			this.DsRB.money_obtained = num2;
			this.DsRB.gq_receivable = this.StBuy.getVisibleRowCount();
			this.DsFam.base_z -= 0.125f;
			this.FamConfirm = IN.CreateGobGUI(this.Gob, "-Confirm").AddComponent<UiGQConfirmFamily>();
			this.FamConfirm.Init(this.M2D, this.DsFam.base_z);
			this.FamConfirm.enabled = false;
			this.FamConfirm.gameObject.SetActive(false);
			this.FamConfirm.FnDecidedRB = new Func<bool, bool>(this.FnDecideConfirmBox);
			this.FamConfirm.FD_FnItemConfirmFinishedGQ = new UiGQItemConfirm.FnItemConfirmFinishedGQ(this.fnItemConfirmFinished);
			this.FamConfirm.FD_ProgressShowingEntry = new Action<GuildManager.GQEntry>(this.fnProgressShowingEntry);
			this.FamConfirm.FD_ConfirmAbort = new Func<List<GuildManager.GQEntry>, bool>(this.fnConfirmAbort);
			this.BxMoney = this.DsFam.CreateT<UiBoxMoney>("Money", 0f, 0f, 30f, 30f, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxMoney.set_aim = AIM.TR;
			this.fnItemRowInitAfter = new UiItemManageBox.FnItemRowInitAfter(this.fnItemRowInitAfterGQM);
			this.fnItemRowRemakedAfter = new UiItemManageBox.FnStorageRun(this.fnItemRowRemakeAfterGQM);
			this.topright_desc_width = 180f;
			this.fnDescAddition = new UiItemManageBox.FnDescAddition(this.fnDescAdditionGQM);
			this.slice_height = 38f;
			this.hide_list_buttons_when_using = true;
			this.auto_reposit_bxcmd = false;
			this.detail_auto_condence = false;
			this.use_topright_counter = true;
			this.fnSortInjectMng = new UiItemManageBox.FnSortOverride(this.fnSortGQM);
			this.initItemStorage();
		}

		private void fnItemRowRemakeAfterGQM(ItemStorage Inv)
		{
			if (this.confirmbox_enabled)
			{
				this.FamConfirm.activateConfirmReceiveBottom();
				int itemRowBtnCount = this.Inventory.getItemRowBtnCount();
				if (itemRowBtnCount > 0)
				{
					this.FamConfirm.setNaviToMainBCon(this.Inventory.getItemRowBtnByIndex(itemRowBtnCount - 1), this.Inventory.getItemRowBtnByIndex(0));
				}
			}
		}

		private void recalcObtaining(out int gp_obtained, out int money_obtained)
		{
			gp_obtained = (money_obtained = 0);
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in this.StBuy.getWholeInfoDictionary())
			{
				if (TX.isStart(keyValuePair.Key.key, "__guildquest_", 0))
				{
					GuildManager.GQEntry gqEntry = this.getGqEntry(keyValuePair.Key);
					if (gqEntry != null)
					{
						gp_obtained += gqEntry.reward_gp;
						money_obtained += gqEntry.reward_money;
					}
				}
			}
		}

		private void initItemStorage()
		{
			this.item_row_skin = "guildquest";
			this.selectable_loop = !this.confirmbox_enabled;
			ItemStorage itemStorage = this.AStorage[this.tab_id];
			base.initDesigner(itemStorage, this.TabLBMain, this.DsT, this.DsCmd, false, true, true);
			this.initItemStorageAfter(false);
		}

		private void initItemStorageAfter(bool force_reposit = false)
		{
			if (this.confirmbox_enabled && (force_reposit || !this.FamConfirm.isActiveReceiveBottom()) && !base.isUsingState())
			{
				this.FamConfirm.activateConfirmReceiveBottom();
			}
			if (this.confirmbox_enabled && this.FamConfirm.isActiveReceiveBottom())
			{
				if (this.Inventory.getItemRowBtnCount() == 0)
				{
					this.FamConfirm.SelectFirst();
				}
			}
			else
			{
				this.FamConfirm.deactivate(false);
			}
			if (this.confirmbox_enabled)
			{
				this.fnItemRowRemakeAfterGQM(this.StBuy);
			}
		}

		public override void createItemDescDesigner(Designer BxDesc, out BtnContainerRadio<aBtn> BConItemStars, out FillBlock FbDesc)
		{
			BConItemStars = null;
			FbDesc = null;
		}

		private bool fnItemTabChanged(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if (cur_value == -1)
			{
				return true;
			}
			this.tab_id = cur_value;
			this.initItemStorage();
			return true;
		}

		private void fnItemRowInitAfterGQM(aBtnItemRow B, ItemStorage.IRow IRow)
		{
			bool flag = this.IMNG.getInventoryPrecious().getCount(B.getItemData(), -1) > 0;
			B.locked_click = true;
			(B.get_Skin() as ButtonSkinGuildQuestItemRow).received = ((this.StBuy.getCount(B.getItemData(), -1) > 0) ? (flag ? ButtonSkinGuildQuestItemRow.RECIEVE.RECIEVED : ButtonSkinGuildQuestItemRow.RECIEVE.RECIEVED_TEMP) : ButtonSkinGuildQuestItemRow.RECIEVE.NONE);
			if (this.sell_mode)
			{
				B.SetLocked(false, true, true);
				return;
			}
			B.SetLocked(flag, true, true);
		}

		private bool fnSortGQM(ItemStorage.IRow Ra, ItemStorage.IRow Rb, ItemStorage.SORT_TYPE sort_type, out int ret)
		{
			ret = 0;
			if (sort_type == ItemStorage.SORT_TYPE.KIND)
			{
				GuildManager.GQEntry gqEntry = this.getGqEntry(Ra.Data);
				GuildManager.GQEntry gqEntry2 = this.getGqEntry(Rb.Data);
				if (gqEntry.categ == gqEntry2.categ)
				{
					int grade = gqEntry.grade;
					int grade2 = gqEntry2.grade;
					if (grade == grade2)
					{
						ret = (int)(gqEntry.ItemEntry.id - gqEntry2.ItemEntry.id);
					}
					else
					{
						ret = grade - grade2;
					}
				}
				else
				{
					ret = gqEntry.categ - gqEntry2.categ;
				}
				return true;
			}
			return false;
		}

		public string fnDescAdditionGQM(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			if (row != UiItemManageBox.DESC_ROW.TOPRIGHT_COUNTER)
			{
				return def_string;
			}
			if (this.StBuy.getCount(Itm, -1) != 0)
			{
				return TX.Get("KD_guild_rem", "");
			}
			return TX.Get("KD_guild_add", "");
		}

		protected override bool fineItemUsingCommand(aBtnItemRow Bi)
		{
			if (this.SelectedGQ == null)
			{
				return false;
			}
			this.DsCmd.createUi(this.M2D);
			bool flag = false;
			object targetItem = this.SelectedGQ.TargetItem;
			this.DsLB.hide();
			this.RTabBar.hide(false, false);
			this.RTabBar.lr_input = false;
			bool flag2 = this.IMNG.getInventoryPrecious().getCount(this.SelectedGQ.ItemEntry, -1) > 0;
			if (this.SelectedGQ.categ == GuildManager.CATEG.COLLECT)
			{
				flag = true;
			}
			else if (this.SelectedGQ.categ == GuildManager.CATEG.COLLECTNEW && flag2)
			{
				flag = true;
			}
			bool show_target_without_know_icon = this.GUILD.getCateg(this.SelectedGQ.categ).show_target_without_know_icon;
			if (this.sell_mode || this.StBuy.getCount(this.SelectedGQ.ItemEntry, -1) > 0)
			{
				this.DsCmd.activateButtons(true, flag2 ? UiGuildDepartBox.RECIEVE.ABORT : UiGuildDepartBox.RECIEVE.ABORT_TEMP, flag, this.SelectedGQ.WM, targetItem, false);
			}
			else
			{
				this.DsCmd.activateButtons(true, UiGuildDepartBox.RECIEVE.RECIEVE, flag, this.SelectedGQ.WM, targetItem, false);
			}
			this.FamConfirm.deactivate(false);
			return true;
		}

		private bool fnClickDepertCmd(aBtn B)
		{
			GuildManager.GQEntry gqEntry = this.getGqEntry(this.UsingTarget);
			if (gqEntry == null)
			{
				return false;
			}
			string title = B.title;
			if (title != null)
			{
				if (!(title == "&&Guild_Btn_recieve_quest"))
				{
					if (title == "&&Guild_Btn_recipebook")
					{
						this.activateRecipeBook();
						return true;
					}
					if (!(title == "&&Guild_Btn_abort_quest_temp"))
					{
						if (!(title == "&&Guild_Btn_deliver"))
						{
							if (title == "&&Guild_Btn_abort_quest_ask")
							{
								this.DsCmd.hide();
								List<GuildManager.GQEntry> list = new List<GuildManager.GQEntry>(1);
								list.Add(gqEntry);
								this.FamConfirm.activateConfirmAbort(list);
								return true;
							}
						}
						else
						{
							this.DsFam.deactivate(false);
							this.FamConfirm.activateItemConfirm(gqEntry);
						}
					}
					else
					{
						this.quitReceiveTemporary(gqEntry);
					}
				}
				else if (!this.initReceiveTemporary(gqEntry))
				{
					return false;
				}
			}
			this.DsCmd.deactivate();
			return true;
		}

		private bool initReceiveTemporary(GuildManager.GQEntry Gq)
		{
			if (Gq == null)
			{
				return false;
			}
			string text;
			if (this.GUILD.getCateg(Gq.categ).cannotReceive(Gq, out text))
			{
				this.DsCmd.errorMessageToDesc(TX.Get(text, ""));
				SND.Ui.play("locked", false);
				CURS.limitVib(aBtn.PreSelected, AIM.R);
				return false;
			}
			using (BList<aBtnItemRow> blist = this.Inventory.PopGetItemRowBtnsFor(Gq.ItemEntry))
			{
				int recievableGQ = this.GUILD.getRecievableGQ(this.GUILD.current_grank);
				if (this.StBuy.getVisibleRowCount() >= recievableGQ)
				{
					if (base.isUsingState())
					{
						this.DsCmd.errorMessageToDesc(TX.GetA("Guild_alert_cannotreceive_max", recievableGQ.ToString()));
					}
					else
					{
						SND.Ui.play("locked", false);
						CURS.limitVib(aBtn.PreSelected, AIM.R);
					}
					return false;
				}
				SND.Ui.play("pr_lockon", false);
				this.StBuy.Add(Gq.ItemEntry, 1, Gq.grade, true, true);
				for (int i = blist.Count - 1; i >= 0; i--)
				{
					(blist[i].get_Skin() as ButtonSkinItemRow).hilighted = true;
				}
				this.DsRB.gp_obtained += Gq.reward_gp;
				this.DsRB.money_obtained += Gq.reward_money;
				this.DsRB.gp_obtaining = 0;
				this.DsRB.gq_receivable = this.StBuy.getVisibleRowCount();
				this.DsRB.fineGuildPointString();
				this.DsT.changeState(UiGQCard.STATE.RECEIVED_TEMP, false);
				base.fineTopRightCounter();
			}
			return true;
		}

		private void quitReceiveTemporary(GuildManager.GQEntry Gq)
		{
			if (Gq == null)
			{
				return;
			}
			SND.Ui.play("reset_var", false);
			using (BList<aBtnItemRow> blist = this.Inventory.PopGetItemRowBtnsFor(Gq.ItemEntry))
			{
				for (int i = blist.Count - 1; i >= 0; i--)
				{
					(blist[i].get_Skin() as ButtonSkinItemRow).hilighted = false;
				}
				this.StBuy.Reduce(Gq.ItemEntry, 1, Gq.grade, true);
				this.DsRB.gp_obtained -= Gq.reward_gp;
				this.DsRB.gp_obtaining += Gq.reward_gp;
				this.DsRB.money_obtained -= Gq.reward_money;
				this.DsRB.gq_receivable = this.StBuy.getVisibleRowCount();
				this.DsRB.fineGuildPointString();
				this.DsT.changeState(UiGQCard.STATE.NONE, false);
				base.fineTopRightCounter();
			}
		}

		protected override void closeUsingState(NelItem Itm, ItemStorage.ObtainInfo Obt)
		{
			base.closeUsingState(Itm, Obt);
			this.DsLB.bind();
			this.RTabBar.bind(false, false);
			this.RTabBar.lr_input = true;
			if (this.confirmbox_enabled)
			{
				this.FamConfirm.activateConfirmReceiveBottom();
			}
		}

		public bool confirmbox_enabled
		{
			get
			{
				return this.sell_mode && this.hasAnyBuyingContent();
			}
		}

		public void activateRecipeBook()
		{
			if (this.RecipeBook != null)
			{
				IN.DestroyOne(this.RecipeBook.gameObject);
				this.RecipeBook = null;
			}
			if (this.DsReel.isActiveReelFocus())
			{
				UiFieldGuide.NextRevealAtAwake = this.DsReel.getFieldGuideTarget();
			}
			else
			{
				GuildManager.GQEntry gqentry = (base.isUsingState() ? this.getGqEntry(this.UsingTarget) : this.SelectedGQ);
				if (gqentry == null)
				{
					return;
				}
				UiFieldGuide.NextRevealAtAwake = this.GUILD.getCateg(gqentry.categ).getFieldGuideTarget(gqentry);
			}
			this.RTabBar.lr_input = false;
			this.Rbk_PreSelected = aBtn.PreSelected;
			this.RecipeBook = new GameObject("RBK" + IN.totalframe.ToString()).AddComponent<UiFieldGuide>();
			this.RecipeBook.auto_deactive_gameobject = false;
			this.RecipeBook.activate();
			this.RecipeBook.gameObject.SetActive(true);
			SND.Ui.play("tool_hand_init", false);
			this.DsFam.deactivate(false);
			this.FamConfirm.deactivate(false);
			this.recipe_active = true;
			IN.setZAbs(this.RecipeBook.transform, this.DsFam.transform.position.z - 0.125f);
		}

		public bool run(float fcnt)
		{
			bool flag = this.FamConfirm.runIRD(fcnt);
			if (!this.DsFam.runIRD(fcnt) && !flag && this.isClosing() && !this.recipe_active)
			{
				this.destruct();
				return false;
			}
			if (this.recipe_active)
			{
				if (this.RecipeBook == null || !this.RecipeBook.isActive())
				{
					if (this.RecipeBook != null)
					{
						this.RecipeBook.auto_deactive_gameobject = true;
					}
					this.recipe_active = false;
					this.DsReel.activate();
					if (!this.DsReel.isActiveReelFocus())
					{
						this.DsLB.activate();
						this.BxMoney.activate();
						this.DsRB.activate();
						this.DsT.activate();
						if (base.isUsingState())
						{
							this.DsCmd.activate();
						}
						else
						{
							this.RTabBar.lr_input = true;
							if (this.confirmbox_enabled)
							{
								this.FamConfirm.activateConfirmReceiveBottom();
							}
						}
					}
					IN.clearSubmitPushDown(true);
					this.RecipeBook = null;
					if (this.Rbk_PreSelected != null)
					{
						if (!base.isUsingState())
						{
							this.Rbk_PreSelected.Select(true);
						}
						else
						{
							this.DsCmd.SelectRB(this.Rbk_PreSelected.title);
						}
					}
				}
				return true;
			}
			if (this.isClosing())
			{
				return true;
			}
			if (this.DsReel.isActiveReelFocus())
			{
				if (this.M2D.isRbkPD())
				{
					this.activateRecipeBook();
					return true;
				}
			}
			else if (!this.FamConfirm.isShowingReceiveFinished() && !this.FamConfirm.isActiveItemConfirm() && !this.FamConfirm.isActiveConfirmAbort() && !this.DsReel.isActiveReelOpening() && !this.DsReel.isActiveReelRemoving())
			{
				if (this.FamConfirm.isActiveShowingReceive())
				{
					if (!this.FamConfirm.runShowingReceive(fcnt) && !this.isClosing())
					{
						if (this.DsReel.failed)
						{
							this.deactivateToReelFocus();
						}
						else
						{
							this.EvtClose(true);
						}
					}
				}
				else if (this.FamConfirm.isActiveShowingSuccess())
				{
					if (!this.FamConfirm.runShowingSuccess(fcnt))
					{
						this.confirmShowingEntries(true);
					}
				}
				else if (this.FamConfirm.isActiveShowingFailed())
				{
					if (!this.FamConfirm.runShowingFailed(fcnt))
					{
						this.confirmShowingEntries(false);
					}
				}
				else if (!this.DsRB.isAnimating())
				{
					if (this.M2D.isRbkPD())
					{
						this.activateRecipeBook();
						return true;
					}
					if (!base.runEditItem())
					{
						if (!this.hasAnyBuyingContent())
						{
							this.deactivateToReelFocus();
						}
						else
						{
							if (this.sell_mode && this.FamConfirm.isActiveReceiveBottom())
							{
								SND.Ui.play("cursor", false);
							}
							else
							{
								this.RTabBar.setValue(this.AStorage.Count - 1, true);
							}
							base.runSelection(false);
							if (this.FamConfirm.isActiveReceiveBottom())
							{
								this.FamConfirm.SelectFirst();
							}
						}
					}
					else if (base.isUsingState())
					{
						if (!this.DsCmd.isFocusingToMapArea())
						{
							if (IN.isUiAddPD())
							{
								this.DsCmd.ExecuteReceiveQuest();
							}
							else if (IN.isUiRemPD())
							{
								this.DsCmd.ExecuteAbortQuest();
							}
						}
						this.DsCmd.runUsing(fcnt);
					}
					else if (base.SelectingTarget != null)
					{
						if (IN.isUiAddPD() && this.StBuy.getCount(base.SelectingTarget, -1) == 0)
						{
							this.initReceiveTemporary(this.getGqEntry(base.SelectingTarget));
						}
						else if (IN.isUiRemPD() && this.StBuy.getCount(base.SelectingTarget, -1) > 0)
						{
							GuildManager.GQEntry gqEntry = this.getGqEntry(base.SelectingTarget);
							if (gqEntry != null)
							{
								if (this.M2D.IMNG.getInventoryPrecious().getCount(base.SelectingTarget, -1) == 0)
								{
									this.quitReceiveTemporary(gqEntry);
									this.initItemStorageAfter(false);
								}
								else
								{
									this.RTabBar.lr_input = false;
									List<GuildManager.GQEntry> list = new List<GuildManager.GQEntry>(1);
									list.Add(gqEntry);
									this.FamConfirm.activateConfirmAbort(list);
								}
							}
						}
					}
				}
			}
			return true;
		}

		public override void fineItemDetailInner(bool using_mode, NelItem Data, ItemStorage.ObtainInfo Obt, ItemStorage.IRow IR, int grade, bool fine_name = false, bool fine_desc = false, bool fine_detail = false)
		{
			GuildManager.GQEntry entry = this.M2D.GUILD.getEntry(Data);
			if (entry == null)
			{
				return;
			}
			NelItem itemEntry = entry.ItemEntry;
			this.DsT.Set(entry, (this.StBuy.getCount(itemEntry, -1) > 0) ? ((this.IMNG.getInventoryPrecious().getCount(itemEntry, -1) > 0) ? UiGQCard.STATE.RECEIVED : UiGQCard.STATE.RECEIVED_TEMP) : UiGQCard.STATE.NONE);
			this.DsRB.Get("money", false);
			int num;
			if (!this.sell_mode && this.StBuy.getCount(Data, -1) == 0)
			{
				this.DsRB.gp_obtaining = entry.reward_gp;
				num = entry.reward_money;
			}
			else
			{
				num = (this.DsRB.gp_obtaining = 0);
			}
			this.DsRB.money_obtaining = num;
			this.DsRB.fineGuildPointString();
			base.fineTopRightCounter();
		}

		public bool FnDecideConfirmBox(bool confirm_execute)
		{
			if (!this.FamConfirm.isActiveReceiveBottom())
			{
				return false;
			}
			if (confirm_execute)
			{
				this.RTabBar.lr_input = false;
				this.DsRB.deactivate();
				this.DsLB.deactivate();
				this.DsReel.deactivate();
				BDic<NelItem, ItemStorage.ObtainInfo> wholeInfoDictionary = this.StBuy.getWholeInfoDictionary();
				List<GuildManager.GQEntry> list = new List<GuildManager.GQEntry>(wholeInfoDictionary.Count);
				foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in wholeInfoDictionary)
				{
					GuildManager.GQEntry gqEntry = this.getGqEntry(keyValuePair.Key);
					if (gqEntry != null && this.IMNG.getInventoryPrecious().getCount(keyValuePair.Key, -1) == 0)
					{
						list.Add(gqEntry);
						this.GUILD.receiveQuest(gqEntry);
						this.IMNG.getInventoryPrecious().Add(keyValuePair.Key, 1, gqEntry.grade, true, true);
					}
				}
				if (list.Count > 0)
				{
					this.FamConfirm.activateConfirmShowingReceive(list, this.DsT);
					EV.getVariableContainer().define("_newquest", list.Count.ToString(), true);
				}
				else
				{
					this.deactivateToReelFocus();
				}
			}
			else
			{
				this.deactivateToReelFocus();
			}
			return true;
		}

		private bool fnItemConfirmFinished(GuildManager.GQEntry Gq, bool decided)
		{
			if (!decided)
			{
				this.returnBackFromShowing(false);
				return true;
			}
			List<GuildManager.GQEntry> list = new List<GuildManager.GQEntry>(1);
			list.Add(Gq);
			SND.Ui.play("editor_close", false);
			this.initConfirmShowingSuccess(list, false);
			return true;
		}

		public void initConfirmShowingSuccess(List<GuildManager.GQEntry> L, bool first_digesting = false)
		{
			this.removeGq(L, true);
			this.DsLB.deactivate();
			this.DsReel.deactivate();
			this.DsRB.deactivate();
			this.BxMoney.activate();
			this.AEntryShowing = new List<GuildManager.GQEntry>(L);
			this.successcount += L.Count;
			COOK.CurAchive.Add(ACHIVE.MENT.city_gq_success, 1);
			this.FamConfirm.activateConfirmShowingSuccess(L, this.DsT);
		}

		public void initConfirmShowingFailed(List<GuildManager.GQEntry> AGq, bool first_digesting = false)
		{
			this.DsReel.failed = true;
			this.removeGq(AGq, false);
			this.DsRB.deactivate();
			this.DsLB.deactivate();
			this.DsReel.deactivate();
			this.DsCmd.deactivate();
			this.AEntryShowing = new List<GuildManager.GQEntry>(AGq);
			this.abortcount += AGq.Count;
			COOK.CurAchive.Add(ACHIVE.MENT.city_gq_failure, 1);
			this.FamConfirm.activateConfirmShowingFailed(AGq, this.DsT);
		}

		public void removeGq(List<GuildManager.GQEntry> AGq, bool success = false)
		{
			for (int i = AGq.Count - 1; i >= 0; i--)
			{
				this.removeGq(AGq[i], success);
			}
		}

		public void removeGq(GuildManager.GQEntry Gq, bool success = false)
		{
			for (int i = this.AStorage.Count - 1; i >= 0; i--)
			{
				this.AStorage[i].Reduce(Gq.ItemEntry, 1, Gq.grade, true);
			}
			this.IMNG.getInventoryPrecious().Reduce(Gq.ItemEntry, 99, Gq.grade, true);
			this.GUILD.removeQuest(Gq);
			if (success)
			{
				if (Gq.reward_etype > ReelExecuter.ETYPE.ITEMKIND)
				{
					this.GUILD.addReward(Gq.reward_etype, this.enable_gq_key);
				}
				if (Gq.RewardIR != null)
				{
					this.GUILD.addReward(Gq.RewardIR, this.enable_gq_key);
				}
				this.DsReel.need_remake_list = true;
			}
		}

		public void returnBackFromShowing(bool quit_using_state = false)
		{
			IN.clearPushDown(true);
			if (this.checkDigesting())
			{
				return;
			}
			this.DsLB.activate();
			this.DsRB.activate();
			this.DsT.activate();
			this.DsReel.activate();
			this.BxMoney.activate();
			this.FamConfirm.deactivate(false);
			this.DsRB.position(this.rbx, this.btmy, -1000f, -1000f, false);
			this.recalcObtaining(out this.DsRB.gp_obtained, out this.DsRB.money_obtained);
			this.DsRB.fineGuildPointString();
			this.DsRB.fineMoneyString();
			if (base.isUsingState())
			{
				if (!quit_using_state)
				{
					this.DsCmd.activate();
					this.DsCmd.SelectRB("&&Guild_Btn_deliver");
					return;
				}
				base.changeStateToSelect();
			}
			if (this.confirmbox_enabled)
			{
				this.fnItemRowRemakeAfterGQM(this.Inventory);
				if (this.FamConfirm.isActiveReceiveBottom() && this.Inventory.getItemRowBtnCount() == 0)
				{
					base.runSelection(false);
					this.FamConfirm.SelectFirst();
				}
				else
				{
					base.blurDescTarget(true);
				}
			}
			else
			{
				base.blurDescTarget(true);
			}
			base.fineTopRightCounter();
		}

		public void fnProgressShowingEntry(GuildManager.GQEntry Gq)
		{
		}

		public void confirmShowingEntries(bool success)
		{
			if (this.AEntryShowing == null)
			{
				this.returnBackFromShowing(true);
				return;
			}
			this.DsRB.gq_receivable = this.StBuy.getVisibleRowCount();
			this.DsRB.activate();
			this.DsRB.position(0f, 0f, -1000f, -1000f, false);
			this.DsT.deactivate();
			this.BxMoney.activate();
			if (this.DsRB.FD_fnAnimateFinished == null)
			{
				this.DsRB.FD_fnAnimateFinished = delegate
				{
					this.returnBackFromShowing(true);
				};
			}
			this.DsRB.initAnimation(this.AEntryShowing, success, 70f);
			this.AEntryShowing = null;
		}

		public bool fnConfirmAbort(List<GuildManager.GQEntry> AGq)
		{
			IN.clearPushDown(true);
			if (AGq != null)
			{
				this.initConfirmShowingFailed(AGq, false);
			}
			else
			{
				this.FamConfirm.deactivate(false);
				if (base.isUsingState())
				{
					this.DsCmd.activate();
					this.DsCmd.bind();
					this.DsCmd.SelectRB("&&Guild_Btn_abort_quest_ask");
				}
				else
				{
					this.RTabBar.lr_input = true;
					this.initItemStorageAfter(true);
					base.blurDescTarget(true);
				}
			}
			return true;
		}

		private bool checkDigesting()
		{
			if (!this.digesting)
			{
				return false;
			}
			List<GuildManager.GQEntry> list = null;
			BDic<NelItem, ItemStorage.ObtainInfo> wholeInfoDictionary = this.StBuy.getWholeInfoDictionary();
			if (wholeInfoDictionary.Count == 0)
			{
				this.deactivateToReelFocus();
				SND.Ui.play("cancel", false);
				return true;
			}
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in wholeInfoDictionary)
			{
				GuildManager.GQEntry gqEntry = this.getGqEntry(keyValuePair.Key);
				if (gqEntry != null && gqEntry.isQTFinishedAuto(true))
				{
					if (list == null)
					{
						list = new List<GuildManager.GQEntry>(1);
					}
					list.Add(gqEntry);
				}
			}
			if (list != null && list.Count > 0)
			{
				this.initConfirmShowingSuccess(list, false);
				return true;
			}
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair2 in wholeInfoDictionary)
			{
				GuildManager.GQEntry gqEntry2 = this.getGqEntry(keyValuePair2.Key);
				if (gqEntry2 != null && (gqEntry2.auto_abort_in_progressing || gqEntry2.isQTAbortedAuto()))
				{
					if (list == null)
					{
						list = new List<GuildManager.GQEntry>(1);
					}
					list.Add(gqEntry2);
				}
			}
			if (list != null && list.Count > 0)
			{
				this.initConfirmShowingFailed(list, false);
				return true;
			}
			return false;
		}

		public void deactivateToReelFocus()
		{
			if (base.isUsingState())
			{
				base.changeStateToSelect();
			}
			if (!this.DsReel.activateReelFocus(this.DsFam))
			{
				this.EvtClose(true);
				return;
			}
			this.DsT.deactivate();
			this.DsLB.deactivate();
			this.DsRB.deactivate();
		}

		public bool fnReelBoxQuit(bool is_cancel_keypush)
		{
			if (is_cancel_keypush)
			{
				if (!this.hasEnableStorageRow())
				{
					return false;
				}
				this.DsReel.activateMain();
				this.DsReel.position(this.reelx, 0f, -1000f, -1000f, false);
				this.DsT.activate();
				this.DsLB.activate();
				this.DsRB.activate();
				this.DsRB.position(this.rbx, this.btmy, -1000f, -1000f, false);
				base.blurDescTarget(true);
				base.changeStateToSelect();
			}
			else
			{
				this.EvtClose(true);
			}
			return true;
		}

		public bool hasAnyBuyingContent()
		{
			foreach (KeyValuePair<NelItem, ItemStorage.ObtainInfo> keyValuePair in this.StBuy.getWholeInfoDictionary())
			{
				if (this.IMNG.getInventoryPrecious().getCount(keyValuePair.Key, -1) == 0)
				{
					return true;
				}
			}
			return false;
		}

		public bool sell_mode
		{
			get
			{
				return this.tab_id == this.AStorage.Count - 1;
			}
		}

		public bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			return false;
		}

		public bool EvtOpen(bool is_first_or_end)
		{
			return true;
		}

		public bool EvtClose(bool is_first_or_end)
		{
			if (is_first_or_end && !this.isClosing())
			{
				if (this.DsFam.isActive())
				{
					IN.clearPushDown(true);
				}
				base.clearFn();
				this.DsFam.deactivate(false);
				this.FamConfirm.deactivate(false);
				base.quitDesigner(false, false);
				this.DsFam.auto_deactive_gameobject = true;
				this.FamConfirm.auto_deactive_gameobject = true;
				EV.remListener(this);
				EV.getVariableContainer().define("_abortcount", this.abortcount.ToString(), true);
				EV.getVariableContainer().define("_successcount", this.successcount.ToString(), true);
				EV.getVariableContainer().define("_levelupcount", this.levelupcount.ToString(), true);
				EV.getVariableContainer().define("_cur_gq_level", this.M2D.GUILD.current_grank.ToString(), true);
			}
			return true;
		}

		public int EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			return 0;
		}

		public bool EvtMoveCheck()
		{
			return true;
		}

		public bool EvtWait(bool is_first = false)
		{
			return is_first || !base.isOffline();
		}

		public bool hasEnableStorageRow()
		{
			for (int i = this.AStorage.Count - 1; i >= 0; i--)
			{
				if (this.AStorage[i].getVisibleRowCount() > 0)
				{
					return true;
				}
			}
			return false;
		}

		private GuildManager GUILD
		{
			get
			{
				return this.M2D.GUILD;
			}
		}

		public GuildManager.GQEntry SelectedGQ
		{
			get
			{
				return this.DsT.SelectedGQ;
			}
		}

		public bool isClosing()
		{
			return this.DsFam.auto_deactive_gameobject;
		}

		private GuildManager.GQEntry getGqEntry(NelItem Itm)
		{
			return this.M2D.GUILD.getEntry(Itm);
		}

		public override string ToString()
		{
			return "UiGQManageBox";
		}

		private readonly NelM2DBase M2D;

		private readonly string enable_gq_key;

		private readonly GameObject Gob;

		private readonly UiBoxDesignerFamily DsFam;

		private readonly bool digesting;

		public const string var_name_newquest = "_newquest";

		public const string var_name_abort = "_abortcount";

		public const string var_name_success = "_successcount";

		public const string var_name_levelup = "_levelupcount";

		public const string var_name_curlevel = "_cur_gq_level";

		public int abortcount;

		public int successcount;

		public int levelupcount;

		private UiGQCard DsT;

		private UiBoxDesigner DsLB;

		private Designer TabLBMain;

		private UiGQPointBox DsRB;

		private UiGuildDepartBox DsCmd;

		private UiGuildReelBox DsReel;

		private UiGQConfirmFamily FamConfirm;

		private UiBoxMoney BxMoney;

		private ColumnRowNel RTabBar;

		private ItemStorage StBuy;

		private List<ItemStorage> AStorage;

		private List<GuildManager.GQEntryList> AAEntry;

		private List<GuildManager.GQEntry> AEntryShowing;

		private int tab_id;

		public const uint col_obtain = 4282470655U;

		private readonly string stabilize_key;

		private aBtn Rbk_PreSelected;

		private UiFieldGuide RecipeBook;

		private bool recipe_active;
	}
}
