using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace nel
{
	public class UiGQConfirmFamily : UiBoxDesignerFamily
	{
		public void Init(NelM2DBase _M2D, float z)
		{
			this.M2D = _M2D;
			this.base_z = z;
			this.ABxGQ = new UiGQCard[2];
			this.auto_deactive_gameobject = false;
			SND.loadSheets("ev_guildquest", "_GQ");
		}

		public override bool runIRD(float fcnt)
		{
			bool flag = base.runIRD(fcnt);
			if (!flag && this.GqPrePosition.z == 1f && this.ABxGQ[0] != null)
			{
				flag = this.ABxGQ[0].getBox().visible;
			}
			if (!flag && this.stt == UiGQConfirmFamily.STATE.ITEM_CONFIRM)
			{
				flag = this.FamItemConfirm.runItemMove(fcnt);
			}
			if (!flag)
			{
				return false;
			}
			if (this.stt == UiGQConfirmFamily.STATE.CONFIRM_RECEIVE_B && this.fine_navi_index != -1)
			{
				this.BConMain.Get(1 - this.fine_navi_index).setNaviT(this.BConMain.Get(this.fine_navi_index).getNaviAim(1), true, false);
				this.BConMain.Get(1 - this.fine_navi_index).setNaviB(this.BConMain.Get(this.fine_navi_index).getNaviAim(3), true, false);
				this.fine_navi_index = -1;
			}
			if (this.stt == UiGQConfirmFamily.STATE.CONFIRM_ABORT)
			{
				this.runConfirmAbort();
			}
			return true;
		}

		private void prepareBxMain(bool flag = true)
		{
			if (!flag)
			{
				if (this.BxMain != null)
				{
					this.BxMain.deactivate();
				}
				return;
			}
			this.FbMain = null;
			this.BConMain = null;
			if (this.BxMain != null)
			{
				return;
			}
			this.BxMain = base.Create("Main", 0f, 0f, 500f, 100f, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxMain.auto_destruct_when_deactivate = false;
			this.BxMain.Focusable(false, false, null);
			this.BxMain.deactivate();
		}

		private UiGQCard prepareBxGq(bool index1, bool flag = true)
		{
			int num = (index1 ? 1 : 0);
			UiGQCard uiGQCard = this.ABxGQ[num];
			if (!flag)
			{
				if (uiGQCard != null)
				{
					uiGQCard.deactivate();
				}
				return uiGQCard;
			}
			if (uiGQCard == null)
			{
				uiGQCard = (this.ABxGQ[num] = this.CreateT<UiGQCard>("GQ" + num.ToString(), 0f, 0f, UiGQCard.defaultw, UiGQCard.defaulth, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX));
				uiGQCard.auto_destruct_when_deactivate = false;
			}
			uiGQCard.deactivate();
			return uiGQCard;
		}

		public bool activateConfirmReceiveBottom()
		{
			if (this.auto_deactive_gameobject)
			{
				return false;
			}
			base.gameObject.SetActive(true);
			this.active = true;
			if (this.stt == UiGQConfirmFamily.STATE.CONFIRM_RECEIVE_B)
			{
				this.BxMain.activate();
				this.BxMain.positionD(0f, -IN.h * 0.5f + 45f, 1, 40f);
				return false;
			}
			this.stt = UiGQConfirmFamily.STATE.CONFIRM_RECEIVE_B;
			this.t_state = 0f;
			this.fine_navi_index = -1;
			this.prepareBxMain(true);
			this.BxMain.WHanim(IN.w * 0.56f, 60f, true, true);
			this.BxMain.Clear();
			this.BxMain.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
			this.BxMain.positionD(0f, -IN.h * 0.5f + 45f, 1, 40f);
			this.BxMain.margin_in_lr = 40f;
			this.BxMain.margin_in_tb = 12f;
			this.BxMain.activate();
			this.BxMain.alignx = ALIGN.CENTER;
			this.BxMain.init();
			this.FbMain = null;
			this.BConMain = this.BxMain.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
			{
				clms = 2,
				titles = new string[] { "&&Guild_Btn_confirm_quest", "&&Guild_Btn_confirm_quest_cancel" },
				w = 230f,
				h = 24f,
				margin_w = 30f,
				fnHover = delegate(aBtn B)
				{
					this.fine_navi_index = B.carr_index;
					return true;
				},
				fnClick = (aBtn B) => this.FnDecidedRB == null || this.FnDecidedRB(B.title == "&&Guild_Btn_confirm_quest")
			});
			return true;
		}

		private float entrycard_sx
		{
			get
			{
				return IN.w * 0.5f + 10f + UiGQCard.defaultw * 0.5f;
			}
		}

		private float entrycard_ex
		{
			get
			{
				return -this.entrycard_sx;
			}
		}

		public bool activateConfirmShowingReceive(List<GuildManager.GQEntry> _AGQEntry, UiGQCard _BxGqCard = null)
		{
			return this.activateConfirmShowing(_AGQEntry, UiGQConfirmFamily.STATE.SHOWING_RECEIVE, UiGQCard.STATE.NONE, _BxGqCard);
		}

		public bool activateConfirmShowingSuccess(List<GuildManager.GQEntry> _AGQEntry, UiGQCard _BxGqCard = null)
		{
			return this.activateConfirmShowing(_AGQEntry, UiGQConfirmFamily.STATE.SHOWING_SUCCESS, UiGQCard.STATE.RECEIVED, _BxGqCard);
		}

		public bool activateConfirmShowingFailed(List<GuildManager.GQEntry> _AGQEntry, UiGQCard _BxGqCard = null)
		{
			return this.activateConfirmShowing(_AGQEntry, UiGQConfirmFamily.STATE.SHOWING_FAILED, UiGQCard.STATE.RECEIVED, _BxGqCard);
		}

		private bool activateConfirmShowing(List<GuildManager.GQEntry> _AGQEntry, UiGQConfirmFamily.STATE depstate, UiGQCard.STATE first_state, UiGQCard _BxGqCard = null)
		{
			if (this.auto_deactive_gameobject)
			{
				return false;
			}
			this.prepareBxMain(false);
			base.gameObject.SetActive(true);
			this.active = true;
			IN.clearPushDown(true);
			if (this.stt == depstate)
			{
				this.ABxGQ[0].activate();
				return false;
			}
			this.AGQEntry = _AGQEntry;
			this.stt = depstate;
			this.t_state = 0f;
			this.prepareGQCards(_BxGqCard, first_state);
			this.prepareBxGq(true, true);
			return true;
		}

		private void prepareGQCards(UiGQCard _BxGqCard = null, UiGQCard.STATE first_state = UiGQCard.STATE.NONE)
		{
			SND.Ui.play("paper", false);
			if (!(_BxGqCard != null) || !(this.ABxGQ[0] == null))
			{
				this.GqPrePosition.z = 0f;
				this.prepareBxGq(false, true);
				this.ABxGQ[0].posSetA(this.entrycard_sx, 20f, 0f, 20f, false);
				this.ABxGQ[0].Set(this.AGQEntry[0], first_state);
				this.ABxGQ[0].activate();
				return;
			}
			this.ABxGQ[0] = _BxGqCard;
			UiBox box = _BxGqCard.getBox();
			bool flag = false;
			this.GqPrePosition = new Vector3(box.get_deperture_x(), box.get_deperture_y(), 1f);
			if (this.AGQEntry.IndexOf(_BxGqCard.SelectedGQ) > 0)
			{
				this.AGQEntry.Remove(_BxGqCard.SelectedGQ);
				this.AGQEntry.Insert(0, _BxGqCard.SelectedGQ);
			}
			else
			{
				flag = true;
			}
			this.ABxGQ[0].activate();
			this.ABxGQ[0].position(0f, 20f, -1000f, -1000f, false);
			if (!flag)
			{
				this.ABxGQ[0].changeState(first_state, true);
				return;
			}
			this.ABxGQ[0].Set(this.AGQEntry[0], first_state);
		}

		public bool runShowingReceive(float fcnt)
		{
			if (!this.runShowingEntries(fcnt, false, UiGQCard.STATE.NONE, UiGQCard.STATE.RECEIVED))
			{
				this.stt = UiGQConfirmFamily.STATE.SHOWING_RECEIVE_FINISHED;
				SND.Ui.play("cancel", false);
				return false;
			}
			return true;
		}

		private bool runShowingEntries(float fcnt, bool returning_back, UiGQCard.STATE first_state, UiGQCard.STATE end_state)
		{
			fcnt *= IN.skippingTS();
			this.t_state += fcnt;
			bool flag = end_state == UiGQCard.STATE.SUCCESS;
			int num = (this.ABxGQ[0].isActive() ? 0 : 1);
			UiGQCard uiGQCard = this.ABxGQ[num];
			if (this.t_state >= 75f && uiGQCard.state != end_state)
			{
				uiGQCard.changeState(end_state, false);
			}
			if (this.t_state >= 110f && this.t_state < 1000f)
			{
				this.t_state = 1960f;
				if (flag)
				{
					int num2 = uiGQCard.SelectedGQ.reward_money;
					int num3 = X.Mn((int)(GF.getC("GLD_RENZOKU") + 1U), 5);
					GF.setC2("GLD_RENZOKU", num3);
					if (num3 > 1)
					{
						this.t_state -= 50f;
						int num4 = (num3 - 1) * 50;
						num2 += num4;
						uiGQCard.setRenzokuTip(num4);
					}
					CoinStorage.addCount(num2, true);
				}
			}
			if (this.t_state >= 2000f)
			{
				this.AGQEntry.RemoveAt(0);
				if (this.FD_ProgressShowingEntry != null)
				{
					this.FD_ProgressShowingEntry(uiGQCard.SelectedGQ);
				}
				if (this.AGQEntry.Count == 0)
				{
					if (this.GqPrePosition.z == 1f)
					{
						this.GqPrePosition.z = 0f;
						if (num == 0 && returning_back)
						{
							uiGQCard.position(this.GqPrePosition.x, this.GqPrePosition.y, -1000f, -1000f, false);
						}
						else
						{
							uiGQCard.posSetA(this.entrycard_ex, 20f, -1000f, -1000f, false);
							uiGQCard.deactivate();
							if (num == 1)
							{
								this.ABxGQ[0].posSetA(-1000f, -1000f, this.GqPrePosition.x, this.GqPrePosition.y, true);
								this.ABxGQ[0].activate();
							}
						}
						this.ABxGQ[0] = null;
					}
					return false;
				}
				uiGQCard.posSetA(this.entrycard_ex, 20f, -1000f, -1000f, false);
				uiGQCard.deactivate();
				SND.Ui.play("paper", false);
				UiGQCard uiGQCard2 = this.ABxGQ[1 - num];
				uiGQCard2.activate();
				uiGQCard2.posSetA(this.entrycard_sx, 20f, 0f, 20f, false);
				uiGQCard2.Set(this.AGQEntry[0], first_state);
				this.t_state = -45f;
			}
			return true;
		}

		public bool runShowingSuccess(float fcnt)
		{
			if (!this.runShowingEntries(fcnt, true, UiGQCard.STATE.RECEIVED, UiGQCard.STATE.SUCCESS))
			{
				this.stt = UiGQConfirmFamily.STATE.SHOWING_SUCCESS_FINISHED;
				return false;
			}
			return true;
		}

		public bool runShowingFailed(float fcnt)
		{
			if (!this.runShowingEntries(fcnt, true, UiGQCard.STATE.RECEIVED, UiGQCard.STATE.FAILED))
			{
				this.stt = UiGQConfirmFamily.STATE.SHOWING_FAILED_FINISHED;
				return false;
			}
			return true;
		}

		public bool activateConfirmAbort(List<GuildManager.GQEntry> _AGQEntry)
		{
			if (this.auto_deactive_gameobject)
			{
				return false;
			}
			SND.Ui.play("tool_drag_init", false);
			base.gameObject.SetActive(true);
			this.active = true;
			if (this.stt == UiGQConfirmFamily.STATE.CONFIRM_ABORT)
			{
				this.BxMain.activate();
				this.BConMain.setValueBits(0U);
				this.SelectFirst();
				this.BxMain.WH(IN.w * 0.66f, IN.h * 0.7f);
				return false;
			}
			this.stt = UiGQConfirmFamily.STATE.CONFIRM_ABORT;
			this.t_state = 0f;
			this.AGQEntry = _AGQEntry;
			this.prepareBxMain(true);
			this.BxMain.WH(IN.w * 0.66f, IN.h * 0.7f);
			this.BxMain.Clear();
			this.BxMain.getBox().frametype = UiBox.FRAMETYPE.MAIN;
			this.BxMain.positionD(0f, 0f, 1, 60f);
			this.BxMain.margin_in_lr = 40f;
			this.BxMain.margin_in_tb = 82f;
			this.BxMain.item_margin_y_px = 30f;
			this.BxMain.activate();
			this.BxMain.alignx = ALIGN.CENTER;
			this.BxMain.init();
			using (STB stb = TX.PopBld(null, 0))
			{
				int count = _AGQEntry.Count;
				int num = 0;
				for (int i = 0; i < count; i++)
				{
					stb.AddTxA("quest_dot", false).Add(" ");
					GuildManager.GQEntry gqentry = this.AGQEntry[i];
					gqentry.ItemEntry.getLocalizedName(stb, gqentry.grade);
					num += gqentry.lost_gp;
					stb.Ret("\n");
				}
				stb.Ret("\n").AddTxA("Guild_alert_progressing_one", false);
				stb.TxRpl(count).TxRpl(num);
				this.FbMain = this.BxMain.addP(new DsnDataP("", false)
				{
					name = "mainp",
					alignx = ALIGN.LEFT,
					TxCol = NEL.ColText,
					Stb = stb,
					size = 16f,
					html = true,
					swidth = this.BxMain.use_w * 0.75f
				}, false);
			}
			this.BConMain = this.BxMain.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
			{
				clms = 2,
				titles = new string[] { "&&Store_desc_cancel", "&&Guild_Btn_abort_quest" },
				w = 230f,
				h = 30f,
				margin_w = 40f,
				margin_h = 20f,
				fnClick = delegate(aBtn B)
				{
					if (this.FD_ConfirmAbort == null || this.FD_ConfirmAbort((B.title == "&&Guild_Btn_abort_quest") ? this.AGQEntry : null))
					{
						B.SetChecked(true, true);
						return true;
					}
					return false;
				}
			});
			this.BConMain.Get(0).click_snd = "cancel";
			this.SelectFirst();
			return true;
		}

		private void runConfirmAbort()
		{
			if (IN.isCancel())
			{
				aBtn aBtn = this.BConMain.Get(0);
				if (aBtn.isSelected())
				{
					aBtn.ExecuteOnSubmitKey();
					return;
				}
				aBtn.Select(true);
			}
		}

		public bool activateItemConfirm(GuildManager.GQEntry _Entry)
		{
			this.prepareBxMain(false);
			base.gameObject.SetActive(true);
			this.active = true;
			if (this.FamItemConfirm == null)
			{
				this.FamItemConfirm = IN.CreateGobGUI(base.gameObject, "-FamItemConfirm").AddComponent<UiGQItemConfirm>();
				this.FamItemConfirm.base_z = this.base_z - 0.125f;
				this.FamItemConfirm.auto_deactive_gameobject = true;
				this.FamItemConfirm.FD_fnItemConfirmFinishedGQ = new UiGQItemConfirm.FnItemConfirmFinishedGQ(this.fnItemConfirmFinished);
			}
			this.FamItemConfirm.InitItemConfirm(this.M2D, _Entry);
			this.stt = UiGQConfirmFamily.STATE.ITEM_CONFIRM;
			this.t_state = 0f;
			return true;
		}

		private bool fnItemConfirmFinished(GuildManager.GQEntry GQ, bool item_delivered)
		{
			if (this.FD_FnItemConfirmFinishedGQ == null || this.FD_FnItemConfirmFinishedGQ(GQ, item_delivered))
			{
				if (this.stt == UiGQConfirmFamily.STATE.ITEM_CONFIRM)
				{
					this.stt = UiGQConfirmFamily.STATE.OFFLINE;
				}
				return true;
			}
			return false;
		}

		public override UiBoxDesignerFamily deactivate(bool immediate = false)
		{
			base.deactivate(immediate);
			if (this.FamItemConfirm != null)
			{
				this.FamItemConfirm.deactivate(immediate);
			}
			return this;
		}

		public void setNaviToMainBCon(aBtn NaviT, aBtn NaviB)
		{
			if (NaviT == null || NaviB == null)
			{
				return;
			}
			int set_clmn = this.BConMain.getMainCarr().set_clmn;
			int length = this.BConMain.Length;
			int num = X.IntC((float)length / (float)set_clmn);
			for (int i = length - 1; i >= 0; i--)
			{
				aBtn aBtn = this.BConMain.Get(i);
				int num2 = aBtn.carr_index / set_clmn;
				if (num2 == 0 && NaviT != null)
				{
					aBtn.setNaviT(NaviT, true, true);
				}
				if (num2 == num - 1 && NaviB != null)
				{
					aBtn.setNaviB(NaviB, true, true);
				}
			}
		}

		public void SelectFirst()
		{
			this.BConMain.Get(0).Select(true);
		}

		public bool isActiveReceiveBottom()
		{
			return this.stt == UiGQConfirmFamily.STATE.CONFIRM_RECEIVE_B && this.isActive();
		}

		public bool isActiveShowingReceive()
		{
			return this.stt == UiGQConfirmFamily.STATE.SHOWING_RECEIVE && this.isActive();
		}

		public bool isActiveShowingSuccess()
		{
			return this.stt == UiGQConfirmFamily.STATE.SHOWING_SUCCESS && this.isActive();
		}

		public bool isActiveShowingFailed()
		{
			return this.stt == UiGQConfirmFamily.STATE.SHOWING_FAILED && this.isActive();
		}

		public bool isActiveItemConfirm()
		{
			return this.stt == UiGQConfirmFamily.STATE.ITEM_CONFIRM && this.isActive();
		}

		public bool isActiveConfirmAbort()
		{
			return this.stt == UiGQConfirmFamily.STATE.CONFIRM_ABORT && this.isActive();
		}

		public bool isShowingReceiveFinished()
		{
			return this.stt == UiGQConfirmFamily.STATE.SHOWING_RECEIVE_FINISHED;
		}

		public override string ToString()
		{
			return "GQConfirmFamily";
		}

		private NelM2DBase M2D;

		private UiGQConfirmFamily.STATE stt;

		private BtnContainer<aBtn> BConMain;

		private float t_state;

		private UiBoxDesigner BxMain;

		private int fine_navi_index = -1;

		private UiGQCard[] ABxGQ;

		private Vector3 GqPrePosition;

		private FillBlock FbMain;

		public Action<GuildManager.GQEntry> FD_ProgressShowingEntry;

		public Func<List<GuildManager.GQEntry>, bool> FD_ConfirmAbort;

		public const string gfc_renzoku_bonus = "GLD_RENZOKU";

		public const int renzoku_bonus_maxcount = 5;

		public const int renzoku_money_multiple = 50;

		private UiGQItemConfirm FamItemConfirm;

		public const float btnh = 24f;

		public const float btnw = 230f;

		public Func<bool, bool> FnDecidedRB;

		public UiGQItemConfirm.FnItemConfirmFinishedGQ FD_FnItemConfirmFinishedGQ;

		private List<GuildManager.GQEntry> AGQEntry;

		private const float entrycard_y = 20f;

		private enum STATE
		{
			OFFLINE,
			CONFIRM_RECEIVE_B,
			CONFIRM_ABORT,
			SHOWING_RECEIVE,
			SHOWING_RECEIVE_FINISHED,
			SHOWING_SUCCESS,
			SHOWING_SUCCESS_FINISHED,
			SHOWING_FAILED,
			SHOWING_FAILED_FINISHED,
			ITEM_CONFIRM
		}
	}
}
