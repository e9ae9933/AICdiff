using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace nel.gm
{
	internal class UiGMCCfg : UiGMC
	{
		internal UiGMCCfg(UiGameMenu _GM)
			: base(_GM, CATEG.CONFIG, false, 0, 0, 0, 0, 1f, 1f)
		{
			this.selectable_loop = 2;
		}

		public override bool initAppearMain()
		{
			if (this.EditCfg != null)
			{
				this.EditCfg.setBoxMainStencil();
			}
			this.BxR.alignx = ALIGN.CENTER;
			if (base.initAppearMain())
			{
				aBtn firstFocusButton = this.EditCfg.getFirstFocusButton();
				if (firstFocusButton != null)
				{
					this.EditCfg.reveal(firstFocusButton.transform, 0f, 0f, true);
				}
				return true;
			}
			this.BxR.margin_in_lr = 14f;
			this.BxR.scrolling_margin_in_lr = 2f;
			this.BxR.item_margin_x_px = 2f;
			this.EditCfg = new CFG(this.BxR, this.BxDesc, null, true, true, delegate(Designer Ds, string key)
			{
				if (key == "MAIN")
				{
					this.Btn2Title = Ds.Br().addButtonT<aBtnNel>(new DsnDataButton
					{
						name = "quit",
						title = "quit",
						skin_title = TX.Get("Config_Btn_QuitGame", ""),
						w = 246f,
						h = 32f,
						fnClick = new FnBtnBindings(this.initGameQuiting),
						fnHover = delegate(aBtn V)
						{
							this.BxDesc.deactivate();
							return true;
						}
					});
				}
			});
			this.BxR.hide();
			return true;
		}

		public override void quitAppear()
		{
			base.quitAppear();
			BGM.addHalfFlag("UIGM");
		}

		internal override void initEdit()
		{
			BGM.remHalfFlag("UIGM");
			this.BxR.bind();
			this.BxR.Focus();
			this.EditCfg.resume();
			Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
			if (flgUiEffectDisable != null)
			{
				flgUiEffectDisable.Add("UIGM");
			}
			this.gres = GMC_RES.CONTINUE;
		}

		internal override void quitEdit()
		{
			this.assignSelection();
			this.fineUiPicAlpha(true);
			if (base.M2D.GameOver != null)
			{
				base.M2D.GameOver.TxReposit();
			}
			this.EditCfg.deactivateDesigner();
			this.EditCfg.submitData();
			BGM.addHalfFlag("UIGM");
			this.GM.FlgStatusHide.Rem("CFG");
			Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
			if (flgUiEffectDisable != null)
			{
				flgUiEffectDisable.Rem("UIGM");
			}
			this.BxR.hide();
			if (EnemySummoner.ActiveScript != null)
			{
				EnemySummoner.ActiveScript.getSummonedArea().fineUsingDifficultyRestrict(false);
			}
			if (this.KC != null && this.KC.isActive())
			{
				this.KC.deactivate();
			}
		}

		private void assignSelection()
		{
		}

		internal override GMC_RES runEdit(float fcnt, bool handle)
		{
			if (this.gres == GMC_RES.QUIT_GAME || this.gres == GMC_RES.QUIT_TO_TITLE)
			{
				if (!(this.BxCmd != null))
				{
					return this.gres;
				}
				if (IN.isCancel())
				{
					BtnContainer<aBtn> bcon = (this.BxCmd.Get("cmd_b", false) as BtnContainerRunner).BCon;
					aBtn aBtn = ((bcon != null) ? bcon.Get("&&Cancel") : null);
					if (aBtn != null)
					{
						aBtn.ExecuteOnSubmitKey();
					}
				}
			}
			else if (this.EditCfg.ui_state == CFG.STATE.KEYCON)
			{
				if (this.Btn2Title.isActive())
				{
					this.EditCfg.deactivateDesigner();
					this.GM.setPosType(UiGameMenu.POSTYPE.KEYCON);
					this.GM.hide();
					if (this.KC != null)
					{
						IN.DestroyOne(this.KC);
					}
					this.KC = new GameObject("KC").AddComponent<KeyConDesignerNel>();
					this.KC.gameObject.layer = LayerMask.NameToLayer(IN.gui_layer_name);
					IN.setZ(this.KC.transform, -4.25f);
					this.KC.activate();
					this.GM.FlgStatusHide.Add("CFG");
				}
				else if (!this.KC.isActive())
				{
					this.GM.bind();
					this.GM.setPosType(UiGameMenu.POSTYPE.NORMAL);
					this.EditCfg.resume();
					this.GM.FlgStatusHide.Rem("CFG");
				}
			}
			else if (!this.EditCfg.runBoxDesignerEdit())
			{
				return GMC_RES.BACK_CATEGORY;
			}
			if (this.KC != null)
			{
				if (!this.KC.gameObject.activeSelf)
				{
					IN.DestroyOne(this.KC);
					this.KC = null;
				}
				this.fineUiPicAlpha(false);
			}
			return GMC_RES.CONTINUE;
		}

		internal override void hideEditTemporary()
		{
		}

		private void fineUiPicAlpha(bool force_zero = false)
		{
			if (this.GM.isActive() && UIPicture.Instance != null)
			{
				if (this.KC == null || force_zero)
				{
					UIPicture.Instance.alpha = 1f;
					UIStatus.Instance.fineBasePos(1f);
					return;
				}
				UIPicture.Instance.alpha = 1f - this.KC.animating_alpha;
				UIStatus.Instance.fineBasePos(1f - this.KC.animating_alpha);
			}
		}

		internal override void releaseEvac()
		{
			base.releaseEvac();
			this.EditCfg = this.EditCfg.destruct();
			if (this.KC != null)
			{
				IN.DestroyOne(this.KC);
				this.KC = null;
			}
			this.fineUiPicAlpha(false);
		}

		public bool initGameQuiting(aBtn B)
		{
			if (!this.GM.general_button_handleable || this.EditCfg.ui_state == CFG.STATE.KEYCON)
			{
				return false;
			}
			this.gres = GMC_RES.QUIT_GAME;
			this.EditCfg.deactivateDesigner();
			this.GM.setPosType(UiGameMenu.POSTYPE.KEYCON);
			this.GM.hide();
			BtnContainer<aBtn> btnContainer;
			if (this.GM.initBxCmd(UiGameMenu.STATE.QUIT_GAME_TO_QUIT, out this.BxCmd))
			{
				this.BxCmd.getBox().frametype = UiBox.FRAMETYPE.MAIN;
				this.BxCmd.WH(620f, 220f);
				this.BxCmd.margin_in_lr = 30f;
				this.BxCmd.margin_in_tb = 60f;
				this.BxCmd.positionD(0f, 20f, 3, 30f);
				this.BxCmd.item_margin_x_px = 20f;
				this.BxCmd.selectable_loop = 1;
				this.BxCmd.alignx = ALIGN.CENTER;
				this.BxCmd.init();
				this.BxCmd.P(TX.Get("Confirm_QuitGame", ""), ALIGN.CENTER, 0f, false, 0f, "");
				this.BxCmd.Br();
				this.BxCmd.addHr(new DsnDataHr().H(42f));
				btnContainer = this.BxCmd.Br().addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
				{
					name = "cmd_b",
					skin = "row_center",
					titles = new string[] { "&&Submit_Quit", "&&Submit_GoToTitle", "&&Cancel" },
					fnClick = new FnBtnBindings(this.fnClickQuitCmd),
					navi_loop = 1,
					margin_h = 0f,
					margin_w = 0f,
					w = this.BxCmd.use_w / 3f - 1f,
					h = 28f
				});
			}
			else
			{
				this.BxCmd.activate();
				btnContainer = (this.BxCmd.Get("cmd_b", false) as BtnContainerRunner).BCon;
				btnContainer.setValue("0");
			}
			this.GM.FlgStatusHide.Add("CFG");
			this.BxCmd.Focusable(false, true, null);
			this.BxCmd.Focus();
			btnContainer.Get(0).Select(false);
			return true;
		}

		public bool fnClickQuitCmd(aBtn B)
		{
			B.SetChecked(true, true);
			if (B.title == "&&Cancel")
			{
				this.cancelQuitCmd(B);
				return true;
			}
			this.gres = ((B.title == "&&Submit_Quit") ? GMC_RES.QUIT_GAME : GMC_RES.QUIT_TO_TITLE);
			this.BxCmd.deactivate();
			this.BxCmd = null;
			return true;
		}

		public void cancelQuitCmd(aBtn B = null)
		{
			this.GM.bind();
			if (B == null)
			{
				B = (this.BxCmd.Get("cmd_b", false) as BtnContainerRunner).Get("&&Cancel");
			}
			if (B != null)
			{
				B.SetChecked(true, true);
			}
			this.BxCmd.deactivate();
			this.BxCmd = null;
			this.gres = GMC_RES.CONTINUE;
			this.GM.setPosType(UiGameMenu.POSTYPE.NORMAL);
			this.EditCfg.resume();
			SND.Ui.play("cancel", false);
			this.GM.FlgStatusHide.Rem("CFG");
			this.BxR.Focus();
			this.Btn2Title.SetChecked(false, true);
			this.Btn2Title.Select(false);
		}

		private CFG EditCfg;

		private KeyConDesignerNel KC;

		private aBtnNel Btn2Title;

		private UiBoxDesigner BxCmd;

		private GMC_RES gres;

		private List<aBtn> ABtnBottom = new List<aBtn>(2);
	}
}
