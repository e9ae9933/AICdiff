using System;
using System.Collections.Generic;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public class UiGuildReelBox : UiBoxDesigner
	{
		public void InitReelBox(NelM2DBase _M2D, string _enable_gq_key)
		{
			this.M2D = _M2D;
			this.enable_gq_key = _enable_gq_key;
			this.margin_in_lr = 8f;
			this.margin_in_tb = 34f;
			base.item_margin_y_px = 0f;
			this.Enbl = this.M2D.GUILD.GetEnblContainer(this.enable_gq_key, true, false);
			this.recreateUi();
		}

		private void recreateUi()
		{
			this.TabR = null;
			this.Clear();
			this.init();
			this.need_remake_list = true;
			this.FbTop = base.addP(new DsnDataP("", false)
			{
				swidth = base.use_w,
				sheight = 20f,
				size = 13f,
				html = true,
				text = "<key cancel/> " + TX.Get("Guild_reward", ""),
				TxCol = NEL.ColText,
				alignx = ALIGN.CENTER,
				aligny = ALIGNY.MIDDLE
			}, false);
			base.Br();
			this.TopHr = base.addHr(new DsnDataHr
			{
				swidth = base.use_w,
				margin_t = 7f,
				margin_b = 8f,
				draw_width_rate = 0.85f,
				Col = NEL.ColText
			});
			this.TabList = this.addTab("TabList", base.use_w, base.use_h, base.use_w, base.use_h, false);
			this.TabList.Smallest();
			this.TabList.item_margin_y_px = 0f;
			this.TabList.scrolling_margin_in_tb = 8f;
			this.TabList.use_button_connection = true;
			this.TabList.init();
			Designer tabList = this.TabList;
			DsnDataRadio dsnDataRadio = new DsnDataRadio();
			dsnDataRadio.name = "list";
			dsnDataRadio.skin = "reelinfo";
			dsnDataRadio.def = -1;
			dsnDataRadio.margin_h = 0;
			dsnDataRadio.fnChanged = (BtnContainerRadio<aBtn> _BCon, int pre, int cur) => cur < 0;
			dsnDataRadio.w = this.TabList.use_w - 30f;
			dsnDataRadio.h = 38f;
			dsnDataRadio.fnHover = new FnBtnBindings(this.fineReelDetail);
			dsnDataRadio.z_push_click = false;
			dsnDataRadio.navi_loop = 2;
			dsnDataRadio.SCA = new ScrollAppend(239, this.TabList.use_w, this.TabList.use_h, 4f, 6f, 0);
			dsnDataRadio.APoolEvacuated = new List<aBtn>();
			this.BConL = tabList.addRadioT<aBtnItemRow>(dsnDataRadio);
			base.endTab(true);
		}

		public override void destruct()
		{
			this.quitUiReel(false);
			base.destruct();
		}

		private void checkRemake()
		{
			if (!this.need_remake_list)
			{
				return;
			}
			this.need_remake_list = false;
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				int num = this.Enbl.AReelEf.Count;
				for (int i = 0; i < num; i++)
				{
					blist.Add("e" + i.ToString());
				}
				num = this.Enbl.AReelItem.Count;
				for (int j = 0; j < num; j++)
				{
					blist.Add("i" + j.ToString());
				}
				this.BConL.RemakeLT<aBtnNel>(blist, "reelinfo");
				num = this.BConL.Length;
				for (int k = 0; k < num; k++)
				{
					ButtonSkinNelReelInfo buttonSkinNelReelInfo = this.BConL.Get(k).get_Skin() as ButtonSkinNelReelInfo;
					if (k < this.Enbl.AReelEf.Count)
					{
						buttonSkinNelReelInfo.initReel(this.Enbl.AReelEf[k]);
					}
					else
					{
						int num2 = k - this.Enbl.AReelEf.Count;
						buttonSkinNelReelInfo.initIR(this.Enbl.AReelItem[num2]);
					}
					buttonSkinNelReelInfo.fix_text_size = 13f;
					buttonSkinNelReelInfo.getBtn().SetLocked(false, true, false);
				}
			}
		}

		private bool fineReelDetail(aBtn B)
		{
			this.BLSel = B;
			this.BRSel = null;
			if (this.TabDesc != null && this.stt == UiGuildReelBox.STATE.REEL_FOCUS)
			{
				if (TX.isStart(B.title, 'e'))
				{
					int num = X.NmI(TX.slice(B.title, 1), -1, false, false);
					if (num >= 0)
					{
						UiReelManager.prepareDescTabList(this.TabDesc, this.Enbl.AReelEf[num]);
					}
				}
				else if (TX.isStart(B.title, 'i'))
				{
					int num2 = X.NmI(TX.slice(B.title, 1), -1, false, false);
					if (num2 >= 0)
					{
						UiReelManager.prepareDescTabList(this.TabDesc, this.Enbl.AReelItem[num2], null);
					}
				}
			}
			return true;
		}

		public override bool run(float fcnt)
		{
			if (!base.initted)
			{
				return false;
			}
			if (!base.run(fcnt) && !this.isActiveReelOpening())
			{
				return false;
			}
			if (base.isActive())
			{
				this.checkRemake();
				if (this.isActiveReelFocus())
				{
					this.runReelFocus();
				}
				if (this.isActiveReelRemoving())
				{
					this.runReelRemoving(fcnt);
				}
			}
			if (this.isActiveReelOpening() && this.UiReel != null && !this.UiReel.isActive())
			{
				this.quitUiReel(false);
			}
			return true;
		}

		public void activateMain()
		{
			this.stt = UiGuildReelBox.STATE.MAIN;
			this.BLSel = null;
			this.BRSel = null;
			base.WHanim(UiGuildReelBox.smallw, base.get_sheight_px(), true, true);
			this.activate();
			if (this.DsDesc != null)
			{
				this.DsDesc.deactivate();
			}
			this.recreateUi();
		}

		public bool activateReelFocus(UiBoxDesignerFamily DsFam)
		{
			IN.clearPushDown(false);
			this.BLSel = null;
			this.BRSel = null;
			if (this.Enbl.AReelEf.Count == 0 && this.Enbl.AReelItem.Count == 0)
			{
				return false;
			}
			if (this.DsDesc == null)
			{
				this.DsDesc = DsFam.Create("Reel_desc", 297f, 0f, 230f, base.get_sheight_px(), 2, 40f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.DsDesc.margin_in_lr = 12f;
				this.DsDesc.margin_in_tb = 28f;
				this.TabDesc = this.DsDesc.addTab("R_info", this.DsDesc.use_w, this.DsDesc.use_h, this.DsDesc.use_w, this.DsDesc.use_h, false);
				this.DsDesc.endTab(true);
				UiReelManager.prepareDescTabList(this.TabDesc, null);
			}
			this.stt = UiGuildReelBox.STATE.REEL_FOCUS;
			base.WHanim(580f, base.get_sheight_px(), true, true);
			this.activate();
			this.DsDesc.activate();
			base.position(-122f, 0f, -1000f, -1000f, false);
			this.checkRemake();
			this.FbTop.widthPixel = base.get_swidth_px() - this.margin_in_lr * 2f;
			this.FbTop.Txt(TX.Get("Guild_reward", ""));
			base.RowRemakeHeightRecalc(this.FbTop, null);
			this.TopHr.width_px = base.get_swidth_px() - this.margin_in_lr * 2f;
			this.TopHr.redraw();
			base.RowRemakeHeightRecalc(this.TopHr, null);
			base.addHr(new DsnDataHr
			{
				vertical = true,
				swidth = this.TabList.get_sheight_px(),
				draw_width_rate = 1f,
				Col = NEL.ColText,
				margin_t = 2f,
				margin_b = 10f
			});
			this.TabR = this.addTab("TabR", base.use_w, this.TabList.get_sheight_px(), base.use_w, this.TabList.get_sheight_px(), false);
			this.TabR.Smallest();
			this.TabR.use_button_connection = true;
			this.TabR.selectable_loop = 2;
			this.TabR.alignx = ALIGN.CENTER;
			this.TabR.margin_in_tb = 50f;
			this.TabR.margin_in_lr = 10f;
			this.TabR.item_margin_y_px = 25f;
			this.TabR.init();
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AddTxA("Guild_reel_hold", false).TxRpl(this.Enbl.AReelEf.Count).TxRpl(this.Enbl.AReelItem.Count)
					.Ret("\n");
				if (this.failed)
				{
					stb.Add(NEL.error_tag).AddTxA("Guild_reel_failure_penalty", false).Add(NEL.error_tag_close)
						.Ret("\n");
				}
				else if (this.Enbl.AReelItem.Count > 5)
				{
					stb.Add(NEL.error_tag).AddTxA("Guild_reel_hold_over", false).TxRpl(5)
						.Add(NEL.error_tag_close)
						.Ret("\n");
				}
				else
				{
					stb.AddTxA("Guild_reel_hold_2", false).TxRpl(5).Ret("\n");
				}
				base.addP(new DsnDataP("", false)
				{
					swidth = this.TabR.use_w,
					size = 15f,
					Stb = stb,
					html = true,
					TxCol = NEL.ColText,
					alignx = ALIGN.LEFT,
					aligny = ALIGNY.MIDDLE,
					text_margin_y = 30f,
					text_margin_x = 10f
				}, false);
			}
			this.TabR.Br();
			aBtn aBtn = this.TabR.addButtonT<aBtnNel>(new DsnDataButton
			{
				name = "b0",
				title = "&&Guild_reel_submit",
				click_snd = "enter_small",
				w = this.TabR.use_w * 0.95f,
				h = 48f,
				navi_auto_fill = false,
				fnClick = new FnBtnBindings(this.fnExecuteReelOpen),
				fnHover = delegate(aBtn B)
				{
					this.BLSel = null;
					this.BRSel = B;
					return true;
				}
			});
			this.TabR.Br();
			FnBtnBindings fnBtnBindings = delegate(aBtn B)
			{
				this.BLSel = null;
				this.BRSel = B;
				return true;
			};
			FnBtnBindings fnBtnBindings2 = delegate(aBtn B)
			{
				if (B.isLocked() || this.stt != UiGuildReelBox.STATE.REEL_FOCUS || !base.isActive())
				{
					return false;
				}
				if (this.FD_Quit == null)
				{
					return true;
				}
				if (!this.FD_Quit(B.title == "&&Guild_back_quest_selection"))
				{
					SND.Ui.play("locked", false);
					CURS.limitVib(B, AIM.R);
					return false;
				}
				return true;
			};
			aBtn aBtn2 = this.TabR.addButtonT<aBtnNel>(new DsnDataButton
			{
				name = "b1",
				title = "&&Guild_whole_quit",
				w = this.TabR.use_w * 0.8f,
				h = 28f,
				click_snd = "cancel",
				locked_click = true,
				navi_auto_fill = false,
				fnHover = fnBtnBindings,
				fnClick = fnBtnBindings2
			});
			this.TabR.Br();
			aBtn aBtn3;
			if (this.failed || this.Enbl.AReelItem.Count > 5)
			{
				this.TabR.addButtonT<aBtnNel>(new DsnDataButton
				{
					name = "b2",
					title = "&&Guild_back_quest_selection",
					w = this.TabR.use_w * 0.8f,
					h = 28f,
					click_snd = "cancel",
					navi_auto_fill = false,
					locked_click = true,
					fnHover = fnBtnBindings,
					fnClick = fnBtnBindings2
				});
				aBtn2.SetLocked(true, true, true);
				aBtn3 = aBtn;
			}
			else
			{
				if (this.Enbl.AReelItem.Count == 0)
				{
					aBtn.SetLocked(true, true, true);
				}
				aBtn3 = aBtn2;
			}
			aBtn3.Select(true);
			BtnContainer<aBtn> btnContainer = this.TabR.getBtnContainer();
			int length = this.BConL.Length;
			int length2 = btnContainer.Length;
			for (int i = 0; i < length; i++)
			{
				this.BConL.Get(i).setNaviR(aBtn3, false, false);
			}
			for (int j = 0; j < length2; j++)
			{
				btnContainer.Get(j).setNaviL(this.BConL.Get(0), false, true);
			}
			this.TabR.fineNaviConnection();
			base.endTab(true);
			return true;
		}

		public void runReelFocus()
		{
			if (IN.isCancelPD() && this.FD_Quit != null)
			{
				SND.Ui.play("cancel", false);
				if (this.hasBackButton())
				{
					this.FD_Quit(true);
					return;
				}
				this.FD_Quit(false);
			}
		}

		public object getFieldGuideTarget()
		{
			if (this.isActiveReelFocus() && this.BLSel != null && TX.isStart(this.BLSel.title, 'i'))
			{
				int num = X.NmI(TX.slice(this.BLSel.title, 1), -1, false, false);
				if (num >= 0)
				{
					return this.Enbl.AReelItem[num][0].Data;
				}
			}
			return null;
		}

		public override Designer deactivate()
		{
			if (this.stt >= UiGuildReelBox.STATE.REEL_FOCUS)
			{
				base.posSetDA(-122f, 0f, 3, IN.h, true);
			}
			base.deactivate();
			if (this.DsDesc != null)
			{
				this.DsDesc.deactivate();
			}
			return this;
		}

		public bool fnExecuteReelOpen(aBtn B)
		{
			if (B.isLocked())
			{
				CURS.limitVib(B, AIM.L);
				SND.Ui.play("locked", false);
				return false;
			}
			this.deactivateUiReel();
			if (this.Enbl.AReelItem.Count == 0)
			{
				this.stt = UiGuildReelBox.STATE.REEL_EFF_REMOVING;
				base.position(0f, 0f, -1000f, -1000f, false);
				if (this.DsDesc != null)
				{
					this.DsDesc.deactivate();
				}
				this.t_removing = 0f;
				base.hide();
				return true;
			}
			this.deactivate();
			this.stt = UiGuildReelBox.STATE.REEL_OPENING;
			this.BaPreEfReel = new ByteArray(0U);
			ReelManager reelManager = this.M2D.IMNG.getReelManager();
			reelManager.writeBinaryTo(this.BaPreEfReel);
			reelManager.clearReels(false, true, false);
			reelManager.overwriteEfReel(this.Enbl.AReelEf);
			reelManager.assignCurrentItemReel(this.Enbl.AReelItem, false, false);
			this.UiReel = reelManager.initUiState(ReelManager.MSTATE.PREPARE, null, true);
			this.UiReel.create_strage = true;
			this.UiReel.fnItemReelProgressing = new ReelManager.FnItemReelProgressing(this.fnReelProcess);
			this.UiReel.prepareMBoxDrawer();
			return true;
		}

		private bool fnReelProcess(ReelManager.ItemReelContainer _Reel)
		{
			this.UiReel.manual_deactivatable = false;
			if (this.Enbl.AReelItem.Count > 0)
			{
				this.Enbl.AReelItem.RemoveAt(0);
			}
			return true;
		}

		private void deactivateUiReel()
		{
			if (this.UiReel != null)
			{
				this.Enbl.AReelEf.Clear();
				if (!this.UiReel.manual_deactivatable)
				{
					this.Enbl.AReelItem.Clear();
				}
				else
				{
					List<ReelExecuter> reelVector = this.M2D.IMNG.getReelManager().getReelVector();
					int count = reelVector.Count;
					for (int i = 0; i < count; i++)
					{
						this.Enbl.AReelEf.Add(reelVector[i].getEType());
					}
				}
				if (this.BaPreEfReel != null)
				{
					this.M2D.IMNG.getReelManager().readBinaryFrom(this.BaPreEfReel, 9);
				}
				this.BaPreEfReel = null;
				this.UiReel = null;
			}
		}

		private void quitUiReel(bool do_not_destruct_element = false)
		{
			this.deactivateUiReel();
			if (!do_not_destruct_element)
			{
				this.M2D.IMNG.getReelManager().destructGob();
			}
			if (this.stt == UiGuildReelBox.STATE.REEL_OPENING)
			{
				this.stt = UiGuildReelBox.STATE.REEL_FOCUS;
				if (this.Enbl.AReelItem.Count == 0)
				{
					if (this.FD_Quit != null)
					{
						this.FD_Quit(false);
					}
					return;
				}
				this.activate();
				this.DsDesc.activate();
				BtnContainer<aBtn> btnContainer = this.TabR.getBtnContainer();
				int length = btnContainer.Length;
				for (int i = 0; i < length; i++)
				{
					aBtn aBtn = btnContainer.Get(i);
					if (!aBtn.isLocked())
					{
						aBtn.Select(true);
						return;
					}
				}
			}
		}

		public void runReelRemoving(float fcnt)
		{
			this.t_removing += fcnt;
			int length = this.BConL.Length;
			for (int i = 0; i < length; i++)
			{
				aBtn aBtn = this.BConL.Get(i);
				if (aBtn.gameObject.activeSelf && aBtn.get_skin_alpha() < 0.75f)
				{
					float num = X.VALWALK(aBtn.get_skin_alpha(), 0f, 0.04f);
					aBtn.get_Skin().alpha = num;
					if (num <= 0f)
					{
						aBtn.gameObject.SetActive(false);
					}
				}
			}
			if (this.t_removing < 1000f)
			{
				if (this.t_removing >= 80f)
				{
					this.t_removing -= 20f;
					bool flag = false;
					for (int j = 0; j < length; j++)
					{
						aBtn aBtn2 = this.BConL.Get(j);
						if (aBtn2.get_skin_alpha() > 0.75f)
						{
							aBtn2.get_Skin().alpha = 0.7f;
							SND.Ui.play("slot_lost", false);
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						this.t_removing = 1000f;
						return;
					}
				}
			}
			else if (this.t_removing >= 1060f)
			{
				this.deactivate();
				this.Enbl.AReelItem.Clear();
				this.Enbl.AReelEf.Clear();
				if (this.FD_Quit != null)
				{
					this.FD_Quit(false);
				}
				return;
			}
		}

		public bool hasBackButton()
		{
			return this.TabR != null && this.TabR.getBtn("&&Guild_back_quest_selection") != null;
		}

		public bool isActiveReelFocus()
		{
			return this.stt == UiGuildReelBox.STATE.REEL_FOCUS;
		}

		public bool isActiveReelOpening()
		{
			return this.stt == UiGuildReelBox.STATE.REEL_OPENING;
		}

		public bool isActiveReelRemoving()
		{
			return this.stt == UiGuildReelBox.STATE.REEL_EFF_REMOVING && base.isActive();
		}

		private NelM2DBase M2D;

		private UiGuildReelBox.STATE stt;

		private string enable_gq_key;

		public static float smallw = 200f;

		public const float largew = 580f;

		public const float desc_w = 230f;

		public const float outw = 824f;

		private FillBlock FbTop;

		private DesignerHr TopHr;

		private Designer TabList;

		private Designer TabR;

		private aBtn BLSel;

		private aBtn BRSel;

		private BtnContainerRadio<aBtn> BConL;

		private UiBoxDesigner DsDesc;

		private Designer TabDesc;

		public bool need_remake_list = true;

		private UiReelManager UiReel;

		public bool failed;

		public float t_removing;

		private GuildManager.EnblContainer Enbl;

		public Func<bool, bool> FD_Quit;

		private DsnDataButton DsnBtnEf;

		private ByteArray BaPreEfReel;

		private enum STATE
		{
			MAIN,
			REEL_FOCUS,
			REEL_OPENING,
			REEL_EFF_REMOVING
		}
	}
}
