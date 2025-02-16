using System;
using UnityEngine;
using XX;

namespace nel.gm
{
	internal class UiGMCSvd : UiGMC
	{
		internal UiGMCSvd(UiGameMenu _GM)
			: base(_GM, CATEG.SVD_SELECT, false, 0, 0, 0, 0, 1f, 1f)
		{
		}

		public override bool initAppearMain()
		{
			if (base.initAppearMain())
			{
				return true;
			}
			PR playerNoel = base.M2D.PlayerNoel;
			this.BxR.item_margin_x_px = 2f;
			bool flag = playerNoel.getNearBench(false, false) != null;
			this.EditSvd = new UiSVD(this.BxR, this.BxDesc, COOK.loaded_index, null, false, playerNoel == null || !SCN.canSave(flag) || !playerNoel.canSave(flag), false);
			this.BxR.hide();
			this.edit_focus_init_to = COOK.loaded_index;
			this.GM.EditFocusInitTo = null;
			return true;
		}

		public override void quitAppear()
		{
			base.quitAppear();
			this.EditSvd.deactivateDesigner();
		}

		internal override void initEdit()
		{
			this.BxR.bind();
			this.EditSvd.resume(true);
			if (this.edit_focus_init_to != -1)
			{
				aBtnSvd btn = this.EditSvd.GetBtn(this.edit_focus_init_to);
				if (btn != null)
				{
					btn.Select(true);
				}
			}
			Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
			if (flgUiEffectDisable != null)
			{
				flgUiEffectDisable.Add("UIGM");
			}
			this.error_showing = false;
			this.svd_shown = true;
		}

		internal override void quitEdit()
		{
			this.BxDesc.deactivate();
			this.BxR.hide();
			Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
			if (flgUiEffectDisable != null)
			{
				flgUiEffectDisable.Rem("UIGM");
			}
			this.edit_focus_init_to = ((UiSVD.last_focused >= 0) ? UiSVD.last_focused : this.edit_focus_init_to);
		}

		internal override void releaseEvac()
		{
			if (this.EditSvd != null)
			{
				this.EditSvd = this.EditSvd.destruct();
			}
			base.releaseEvac();
			if (this.svd_shown)
			{
				SVD.releaseAllThumbs();
			}
		}

		internal override GMC_RES runEdit(float fcnt, bool handle)
		{
			if (!this.error_showing)
			{
				if (TX.valid(COOK.save_failure_announce))
				{
					this.error_showing = true;
					this.BxR.hide();
					this.initErrorConfirm();
					return GMC_RES.CONTINUE;
				}
				if (!this.EditSvd.runBoxDesignerEdit(false))
				{
					int num = this.EditSvd.isLoadSuccess();
					if (num >= 0)
					{
						SVD.sFile file = SVD.GetFile(num, true);
						if (file != null)
						{
							COOK.setLoadTarget(file, this.EditSvd.ignore_svd_cfg);
							return GMC_RES.LOAD_GAME;
						}
					}
					return GMC_RES.BACK_CATEGORY;
				}
				UiSVD.STATE ui_state = this.EditSvd.ui_state;
			}
			return GMC_RES.CONTINUE;
		}

		private void initErrorConfirm()
		{
			this.error_showing = true;
			this.EditSvd.runBoxDesignerEdit(false);
			UiBoxDesigner uiBoxDesigner;
			FillBlock fillBlock;
			BtnContainer<aBtn> btnContainer;
			if (this.GM.initBxCmd(UiGameMenu.STATE.LOAD_GAME, out uiBoxDesigner))
			{
				uiBoxDesigner.margin_in_lr = 40f;
				uiBoxDesigner.margin_in_tb = 50f;
				uiBoxDesigner.WH(IN.w * 0.68f, IN.h * 0.55f);
				uiBoxDesigner.init();
				uiBoxDesigner.alignx = ALIGN.CENTER;
				Vector3 vector = this.GM.transform.localPosition * 64f;
				uiBoxDesigner.posSetA(-vector.x, -vector.y, -vector.x, -vector.y, true);
				IN.setZ(uiBoxDesigner.transform, -4f);
				fillBlock = uiBoxDesigner.addP(new DsnDataP("", false)
				{
					name = "head",
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.MIDDLE,
					TxCol = C32.d2c(4283780170U),
					swidth = uiBoxDesigner.use_w,
					sheight = uiBoxDesigner.use_h * 0.66f,
					html = true,
					text_auto_wrap = true,
					size = 18f,
					text = " "
				}, false);
				Designer designer = uiBoxDesigner;
				DsnDataButtonMulti dsnDataButtonMulti = new DsnDataButtonMulti();
				dsnDataButtonMulti.name = "svd_cmd";
				dsnDataButtonMulti.skin = "row_center";
				dsnDataButtonMulti.titles = new string[] { "&&Confirm" };
				dsnDataButtonMulti.w = 280f;
				dsnDataButtonMulti.h = 32f;
				dsnDataButtonMulti.margin_h = 0f;
				dsnDataButtonMulti.margin_w = 0f;
				dsnDataButtonMulti.clms = 2;
				dsnDataButtonMulti.fnClick = new FnBtnBindings(this.fnClickedConfirmError);
				dsnDataButtonMulti.navi_loop = 3;
				dsnDataButtonMulti.click_snd = "cancel";
				dsnDataButtonMulti.fnMaking = delegate(BtnContainer<aBtn> BCon, aBtn B)
				{
					if (B.click_snd != "cancel")
					{
						B.click_snd = "enter";
					}
					return true;
				};
				btnContainer = designer.addButtonMultiT<aBtnNel>(dsnDataButtonMulti);
			}
			else
			{
				fillBlock = uiBoxDesigner.Get("head", false) as FillBlock;
				BtnContainerRunner btnContainerRunner = uiBoxDesigner.Get("svd_cmd", false) as BtnContainerRunner;
				if (fillBlock == null || btnContainerRunner == null)
				{
					this.GM.initBxCmdClearing();
					this.initErrorConfirm();
					return;
				}
				uiBoxDesigner.activate();
				btnContainer = btnContainerRunner.BCon;
			}
			uiBoxDesigner.Focusable(true, false, null);
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.Add(NEL.error_tag).Add("<font size=\"160%\">").AddTxA("SVD_Alert_save_failure", false)
					.Add("</font>")
					.Add(NEL.error_tag_close)
					.Ret("\n")
					.Ret("\n");
				stb.AddPath(COOK.save_failure_announce, 0.5f);
				fillBlock.Txt(stb);
			}
			uiBoxDesigner.Focus();
			this.BxR.hide();
			aBtn aBtn = btnContainer.Get(0);
			aBtn.Select(true);
			aBtn.SetChecked(false, true);
			SND.Ui.play("locked", false);
		}

		private bool fnClickedConfirmError(aBtn B)
		{
			B.SetChecked(true, true);
			this.GM.BxCmd.deactivate();
			this.error_showing = false;
			this.BxR.bind();
			this.EditSvd.resume(false);
			COOK.save_failure_announce = "";
			this.EditSvd.SelectLastFocusedRow(true);
			return true;
		}

		private UiSVD EditSvd;

		private bool svd_shown;

		private int edit_focus_init_to = -1;

		private bool error_showing;
	}
}
