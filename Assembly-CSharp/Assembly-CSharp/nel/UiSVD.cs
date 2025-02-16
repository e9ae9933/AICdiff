using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class UiSVD
	{
		public static float thumb_w
		{
			get
			{
				return (float)SVD.thumb_w;
			}
		}

		public static float thumb_h
		{
			get
			{
				return (float)SVD.thumb_h;
			}
		}

		public float desc_h
		{
			get
			{
				return UiSVD.thumb_h + 80f + 40f + 90f;
			}
		}

		public static bool ui_active
		{
			get
			{
				return UiSVD.Instance != null;
			}
		}

		public void descZReposit()
		{
			if (this.BxDesc != null)
			{
				IN.setZ(this.BxDesc.transform, this.z_desc_pre);
			}
		}

		public UiSVD destruct()
		{
			if (this.BxMain == null)
			{
				return null;
			}
			this.descZReposit();
			this.BxDesc = (this.BxMain = null);
			this.DsSubmittion = null;
			this.RowCon = null;
			this.DescThumb = null;
			if (UiSVD.Instance == this)
			{
				UiSVD.Instance = null;
			}
			return null;
		}

		private void activateMaterial()
		{
			if (this.activated_id != null)
			{
				return;
			}
			int num = ++UiSVD.activated_id_count;
			this.activated_id = num.ToString();
			this.Mti = MTI.LoadContainer("MTI_svd", this.activated_id);
			SND.loadSheets("saveload", "svd");
			this.MILogo = this.Mti.LoadImage("logo");
		}

		public static void unloadSound()
		{
			SND.unloadSheets("saveload", "svd");
		}

		public void deactivateDesigner()
		{
			if (this.Hd != null)
			{
				IN.DestroyOne(this.Hd.gameObject);
			}
			if (this.ThumbRow != null)
			{
				this.create_thumb_delay = -1;
				this.ThumbRow = null;
			}
			if (!this.desc_first_assign)
			{
				this.saveMemo();
				this.desc_first_assign = true;
			}
			if (this.activated_id != null)
			{
				this.Mti = MTI.ReleaseContainer("MTI_svd", this.activated_id);
				this.activated_id = null;
			}
			if (this.SkRevealLocal != null)
			{
				this.SkRevealLocal.getBtn().hide();
				this.SkRevealLocal = null;
			}
			this.descZReposit();
			this.BxDesc.use_canvas = false;
			this.BxDesc.deactivate();
			this.Hd = null;
			this.ui_state = UiSVD.STATE.OFFLINE;
		}

		public UiSVD(UiBoxDesigner _Bx, UiBoxDesigner _BxDesc, int first_index, Designer _DsSubmittion = null, bool _only_load = false, bool _cannot_save = false, bool _ignore_svd_cfg = false)
		{
			SVD.prepareList(false);
			if (UiSVD.Instance != null)
			{
				X.de("SVD 多重起動", null);
			}
			UiSVD.Instance = this;
			this.only_load = _only_load;
			this.cannot_save = _cannot_save;
			this.ignore_svd_cfg = _ignore_svd_cfg;
			UiSVD.show_explore_timer = false;
			this.BxMain = _Bx;
			this.BxDesc = _BxDesc;
			this.DsSubmittion = _DsSubmittion;
			this.BxMain.alignx = ALIGN.CENTER;
			this.BaseTrs = this.BxMain.transform.parent;
			this.f0 = IN.totalframe;
			if (this.BxDesc != null)
			{
				this.z_desc_pre = this.BxDesc.transform.localPosition.z;
				IN.setZ(this.BxDesc.transform, -0.86f);
			}
			UiSVD.last_focused = X.MMX(0, first_index, 99);
			this.pre_marked = COOK.loaded_index;
			this.ui_state = UiSVD.STATE.INITTED;
			_Bx.use_scroll = false;
			this.BxMain.margin_in_lr = 18f;
			this.BxMain.margin_in_tb = 20f;
			this.BxMain.item_margin_y_px = 4f;
			this.BxMain.item_margin_x_px = 0f;
			this.BxDesc.Clear();
			this.createDescription(this.BxDesc, false);
			this.BxMain.init();
			this.RowCon = this.createBoxDesignerContent();
			this.fineRowConNavi();
			this.BxDesc.deactivate();
		}

		private void fineRowConNavi()
		{
			List<SVD.sFile> list = SVD.prepareList(false);
			int num = list.Capacity - 1;
			if (this.only_load)
			{
				int num2 = list.Count - 1;
				aBtn aBtn = null;
				if (this.DsSubmittion != null)
				{
					using (BList<aBtn> blist = this.DsSubmittion.getRowManager().PopLastLineSelectable(false))
					{
						aBtn = blist[0];
					}
				}
				if (aBtn == null)
				{
					aBtn = this.FirstExistRow ?? this.GetBtn(0);
				}
				aBtn aBtn2 = this.LastExistRow ?? aBtn;
				for (int i = num2 + 1; i < num; i++)
				{
					aBtnSvd btn = this.GetBtn(i);
					btn.setNaviT(aBtn2, false, true);
					btn.setNaviB(aBtn, false, true);
				}
				num = num2;
			}
		}

		private void fineSubmitionNavi()
		{
			aBtn aBtn = (this.only_load ? this.FirstExistRow : this.BtAs);
			aBtn aBtn2 = (this.only_load ? this.LastExistRow : this.RowCon.Get(this.RowCon.Length - 1));
			if (this.BtTop != null)
			{
				this.BtAs.setNaviB(this.BtTop, true, true);
			}
			if (this.DsSubmittion == null || aBtn == null || aBtn2 == null)
			{
				return;
			}
			using (BList<aBtn> blist = this.DsSubmittion.getRowManager().PopFirstLineSelectable(false))
			{
				int count = blist.Count;
				for (int i = 0; i < count; i++)
				{
					blist[i].setNaviT(aBtn2, false, true);
				}
			}
			using (BList<aBtn> blist2 = this.DsSubmittion.getRowManager().PopLastLineSelectable(false))
			{
				int count2 = blist2.Count;
				for (int j = 0; j < count2; j++)
				{
					blist2[j].setNaviB(aBtn, false, true);
				}
			}
		}

		public aBtnSvd GetBtn(int i)
		{
			if (i == 0)
			{
				return this.BtAs;
			}
			return this.RowCon.Get(i - 1) as aBtnSvd;
		}

		private BtnContainerRadio<aBtn> createBoxDesignerContent()
		{
			this.desc_first_assign = true;
			this.BtAs = this.BxMain.addButtonT<aBtnSvd>(new DsnDataButton
			{
				name = "svd_as",
				title = "0",
				w = this.BxMain.use_w - 2f - 16f,
				h = 88f
			});
			IN.setZ(this.BtAs.transform, -0.125f);
			this.BxMain.Hr(0.8f, 10f, 12f, 1f);
			this.FnMakeSvdList(null, this.BtAs);
			this.BxMain.Br();
			Designer bxMain = this.BxMain;
			DsnDataRadio dsnDataRadio = new DsnDataRadio();
			dsnDataRadio.name = "svd_list_c";
			dsnDataRadio.def = -1;
			dsnDataRadio.w = this.BxMain.use_w - 20f - 16f;
			dsnDataRadio.skin = "normal";
			dsnDataRadio.clms = 1;
			dsnDataRadio.margin_h = 0;
			dsnDataRadio.margin_w = 0;
			dsnDataRadio.h = 88f;
			dsnDataRadio.fnGenerateKeys = delegate(BtnContainerBasic BCon, List<string> A)
			{
				int num = SVD.prepareList(false).Capacity - 1;
				for (int i = 1; i < num; i++)
				{
					A.Add(i.ToString());
				}
			};
			dsnDataRadio.fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnClickScdRow1);
			dsnDataRadio.fnMakingAfter = new BtnContainer<aBtn>.FnBtnMakingBindings(this.FnMakeSvdList);
			dsnDataRadio.SCA = new ScrollAppend(239, this.BxMain.use_w, this.BxMain.use_h - 8f, 8f, 8f, 0);
			return bxMain.addRadioT<aBtnSvd>(dsnDataRadio);
		}

		private bool FnMakeSvdList(BtnContainer<aBtn> BCon, aBtn _B)
		{
			int num = ((BCon == null) ? 0 : BCon.Length);
			aBtnSvd aBtnSvd = _B as aBtnSvd;
			SVD.sFile file = SVD.GetFile(num, true);
			aBtnSvd.setData(num, file, false);
			aBtnSvd.ContainerSVD = this;
			aBtnSvd.addHoverFn(new FnBtnBindings(this.fnHoverScdRow));
			aBtnSvd.addOutFn(new FnBtnBindings(this.fnOutScdRow));
			if (num == 0)
			{
				aBtnSvd.addClickFn(new FnBtnBindings(this.fnClickScdRow));
			}
			if (file != null)
			{
				if (this.FirstExistRow == null || this.FirstExistRow.getSvdIndex() >= file.index)
				{
					this.FirstExistRow = aBtnSvd;
				}
				if (this.LastExistRow == null || this.LastExistRow.getSvdIndex() <= file.index)
				{
					this.LastExistRow = aBtnSvd;
				}
			}
			if (this.BtTop == null && num >= 1 && (!this.only_load || file != null))
			{
				this.BtTop = aBtnSvd;
			}
			if (this.pre_marked == num)
			{
				aBtnSvd.fineMarker();
			}
			return true;
		}

		private bool fnClickScdRow1(BtnContainer<aBtn> BCon, int pre, int aft)
		{
			return this.fnClickScdRow(BCon.Get(aft));
		}

		private bool fnClickScdRow(aBtn B)
		{
			if (this.BxDesc == null || B.isLocked() || this.ui_state > UiSVD.STATE.ACTIVE)
			{
				return true;
			}
			aBtnSvd aBtnSvd = B as aBtnSvd;
			return !(aBtnSvd == null) && aBtnSvd.getSvdIndex() == UiSVD.last_focused && this.selectSvdRow(aBtnSvd);
		}

		public aBtn simulateNaviTranslationInner(ref int a, aBtn Dep, int index)
		{
			if (this.FirstExistRow == null || this.LastExistRow == null || !this.only_load)
			{
				return Dep;
			}
			if (a == 1 && index <= this.FirstExistRow.getSvdIndex())
			{
				if (this.DsSubmittion != null)
				{
					using (BList<aBtn> blist = this.DsSubmittion.getRowManager().PopFirstLineSelectable(false))
					{
						if (blist != null && blist.Count > 0)
						{
							return blist[0];
						}
					}
				}
				return this.LastExistRow;
			}
			if (a == 3 && index >= this.LastExistRow.getSvdIndex())
			{
				if (this.DsSubmittion != null)
				{
					using (BList<aBtn> blist2 = this.DsSubmittion.getRowManager().PopFirstLineSelectable(false))
					{
						if (blist2 != null && blist2.Count > 0)
						{
							return blist2[0];
						}
					}
				}
				return this.FirstExistRow;
			}
			return Dep;
		}

		private bool fnHoverScdRow(aBtn B)
		{
			if (this.BxDesc == null || B.isLocked() || this.ui_state > UiSVD.STATE.ACTIVE)
			{
				return true;
			}
			aBtnSvd aBtnSvd = B as aBtnSvd;
			if (!this.desc_first_assign)
			{
				this.saveMemo();
			}
			UiSVD.last_focused = aBtnSvd.getSvdIndex();
			this.desc_first_assign = false;
			this.fineDescString();
			this.DescThumb.redraw_flag = true;
			Vector2 vector = new Vector2(-this.BxMain.swidth / 2f - this.BxDesc.swidth / 2f - 18f + this.BxMain.getBox().get_deperture_x(), 20f + this.BxMain.getBox().get_deperture_y());
			if (this.BxDesc.isActive())
			{
				this.BxDesc.position(vector.x, vector.y, -1000f, -1000f, false);
			}
			else
			{
				this.BxDesc.activate();
				this.BxDesc.positionD(vector.x, vector.y, 2, 40f);
			}
			this.desc_hide = false;
			if (aBtnSvd.getSvdData() != null)
			{
				this.create_thumb_delay = 20;
				this.ThumbRow = aBtnSvd;
			}
			else
			{
				this.create_thumb_delay = 0;
				this.ThumbRow = null;
			}
			return true;
		}

		private void fineDescString()
		{
			if (this.BxDesc == null)
			{
				return;
			}
			SVD.sFile file = SVD.GetFile(UiSVD.last_focused, true);
			string text = "";
			if (file != null)
			{
				if (file.header_prepared)
				{
					text = file.getDescStringForUi();
				}
			}
			else
			{
				text = ((UiSVD.last_focused == 0) ? TX.Get("SaveDataDesc_Empty_auto", "") : TX.GetA("SaveDataDesc_Empty", X.spr0(UiSVD.last_focused, 2, '0')));
			}
			IVariableObject variableObject = this.BxDesc.Get("descp", false);
			if (variableObject != null)
			{
				variableObject.setValue(text);
			}
			LabeledInputField labeledInputField = this.BxDesc.Get("descmemo", false) as LabeledInputField;
			if (labeledInputField != null)
			{
				if (file != null)
				{
					labeledInputField.setValue(file.memo);
					labeledInputField.SetLocked(false, true, false);
					return;
				}
				labeledInputField.setValue("");
				labeledInputField.SetLocked(true, true, false);
			}
		}

		private bool fnOutScdRow(aBtn B)
		{
			if (this.BxDesc == null || B.isLocked())
			{
				return true;
			}
			aBtnSvd aBtnSvd = B as aBtnSvd;
			if (UiSVD.last_focused == aBtnSvd.getSvdIndex())
			{
				this.saveMemo();
			}
			return true;
		}

		private void saveMemo()
		{
			if (this.save_memo_lock)
			{
				return;
			}
			SVD.sFile file = SVD.GetFile(UiSVD.last_focused, true);
			IVariableObject variableObject = this.BxDesc.Get("descmemo", false);
			if (file == null || variableObject == null)
			{
				return;
			}
			bool header_prepared = file.header_prepared;
			string text = variableObject.getValueString();
			if (text.Length > 2047)
			{
				text = TX.slice(text, 0, 2047);
			}
			if (file.memo != text)
			{
				file.memo = text;
				SVD.changeOnlyMemo(file);
			}
		}

		public bool runBoxDesignerEdit(bool ignore_key = false)
		{
			if (this.ui_state == UiSVD.STATE.LOAD_SUCCESS)
			{
				return false;
			}
			if (this.activated_id == null)
			{
				this.activateMaterial();
			}
			if (this.create_thumb_delay >= 0 && this.ThumbRow != null)
			{
				SVD.sFile svdData = this.ThumbRow.getSvdData();
				if (svdData.Thumbnail == null && !svdData.thumb_error && svdData.header_prepared)
				{
					int num = this.create_thumb_delay - 1;
					this.create_thumb_delay = num;
					if (num < 0)
					{
						COOK.readBinaryThumbnailFromLocal(svdData);
					}
				}
			}
			if (this.ui_state == UiSVD.STATE.SELECTING_CANCEL || this.ui_state == UiSVD.STATE.SELECTING_CANCEL_NO_WRITE_MEMO)
			{
				if (this.ui_state == UiSVD.STATE.SELECTING_CANCEL)
				{
					this.saveMemo();
				}
				this.createDescription(this.BxDesc, true);
				this.ui_state = UiSVD.STATE.INITTED;
				this.desc_hide = false;
			}
			if (this.ui_state == UiSVD.STATE.SELECTING_TO_LOAD_ASK_AGAIN)
			{
				this.loadAskAgain();
			}
			if ((this.ui_state == UiSVD.STATE.LOAD_ASK_AGAIN || this.ui_state == UiSVD.STATE.SELECTING) && IN.isCancelPD())
			{
				this.ui_state = UiSVD.STATE.SELECTING_CANCEL;
				SND.Ui.play("cancel", false);
			}
			if (this.ui_state == UiSVD.STATE.INITTED)
			{
				this.ui_state = UiSVD.STATE.ACTIVE;
				if (this.Hd != null)
				{
					this.Hd.deactivate(false);
				}
				if (this.BxMain != null)
				{
					this.BxMain.activate();
					this.BxMain.Focus();
				}
				if (this.DsSubmittion != null)
				{
					this.DsSubmittion.bind();
				}
				this.fineSubmitionNavi();
				aBtnSvd btn = this.GetBtn(UiSVD.last_focused);
				if (btn != null)
				{
					if (!btn.isLocked())
					{
						btn.Select(true);
					}
					if (UiSVD.last_focused > 0 && this.RowCon.BelongScroll != null)
					{
						this.RowCon.BelongScroll.reveal(btn, false, REVEALTYPE.ALWAYS);
					}
				}
				this.BtAs.SetChecked(false, true);
				this.RowCon.setValue(-1, false);
			}
			else if (this.ui_state == UiSVD.STATE.ACTIVE)
			{
				if (this.desc_hide && this.BxDesc != null)
				{
					this.BxDesc.deactivate();
					this.desc_hide = false;
				}
				if (!ignore_key)
				{
					if (IN.isCancel())
					{
						return false;
					}
					if (IN.isMapPD(1))
					{
						UiSVD.show_explore_timer = !UiSVD.show_explore_timer;
						this.BtAs.RowSkin.fineStr();
						BtnContainerRunner btnContainerRunner = this.BxMain.Get("svd_list_c", false) as BtnContainerRunner;
						if (btnContainerRunner != null)
						{
							for (int i = btnContainerRunner.BCon.Length - 1; i >= 0; i--)
							{
								ButtonSkinSvdRow buttonSkinSvdRow = btnContainerRunner.BCon.Get(i).get_Skin() as ButtonSkinSvdRow;
								if (buttonSkinSvdRow != null)
								{
									buttonSkinSvdRow.fineStr();
								}
							}
						}
					}
				}
			}
			return true;
		}

		public void resume(bool recreate_bxdesc = false)
		{
			this.ui_state = UiSVD.STATE.INITTED;
			this.BxMain.Focus();
			this.BxMain.bind();
			if (this.BxDesc != null)
			{
				IN.setZ(this.BxDesc.transform, -0.86f);
				if (recreate_bxdesc)
				{
					this.createDescription(this.BxDesc, false);
				}
			}
		}

		private FillImageBlock createDescription(UiBoxDesigner Bx, bool wh_animate = false)
		{
			Bx.Clear();
			if (wh_animate)
			{
				this.BxDesc.WHanim(420f, this.desc_h, true, true);
			}
			else
			{
				this.BxDesc.WH(420f, this.desc_h);
			}
			Bx.use_canvas = true;
			Bx.margin_in_tb = 20f;
			Bx.margin_in_lr = 20f;
			Bx.item_margin_y_px = 15f;
			Bx.item_margin_x_px = 0f;
			Bx.init();
			Bx.alignx = ALIGN.CENTER;
			float num = 420f - Bx.margin_in_lr * 2f;
			float num2 = X.Mx(UiSVD.thumb_w, num * 0.35f);
			FillImageBlock fillImageBlock = Bx.addImg(new DsnDataImg
			{
				FnDraw = new MeshDrawer.FnGeneralDraw(this.FnDrawThumbArea),
				text = " ",
				swidth = num,
				sheight = UiSVD.thumb_h
			});
			Bx.Br();
			Designer designer = Bx.addTab("thumb_r", num, 80f, num - num2, 80f, false);
			designer.Smallest();
			designer.margin_in_lr = 30f;
			designer.margin_in_tb = 8f;
			designer.alignx = ALIGN.LEFT;
			designer.init();
			Bx.addP(new DsnDataP("", false)
			{
				name = "descp",
				text = " ",
				size = 11.7f,
				alignx = ALIGN.LEFT,
				aligny = ALIGNY.TOP,
				Col = MTRX.ColTrnsp,
				TxCol = C32.d2c(4283780170U),
				swidth = designer.use_w,
				sheight = 80f - designer.margin_in_tb * 2f,
				lineSpacing = 1.1f,
				text_margin_y = 0f,
				text_auto_condense = true,
				html = true
			}, false);
			Bx.Br();
			Bx.addInput(new DsnDataInput
			{
				name = "descmemo",
				bounds_w = designer.use_w - 20f,
				multi_line = 16,
				h = 64f,
				label = TX.Get("SVD_label_memo", ""),
				return_blur = false
			}).SetLocked(true, true, false);
			Bx.endTab(true);
			this.DescThumb = fillImageBlock;
			this.desc_first_assign = true;
			return fillImageBlock;
		}

		private bool FnDrawThumbArea(MeshDrawer Md, float alpha)
		{
			if (this.DescThumb == null)
			{
				return true;
			}
			if (this.ui_state == UiSVD.STATE.INITTED)
			{
				return false;
			}
			SVD.sFile file = SVD.GetFile(UiSVD.last_focused, true);
			if (file == null)
			{
				this.DescThumb.replaceMaterialManual(MTRX.getMtr(this.DescThumb.stencil_ref));
				Md.Col = C32.MulA(4283780170U, alpha);
				Md.RectDashed(0f, 0f, UiSVD.thumb_w, UiSVD.thumb_h, 0.5f, 20, 2f, false, false);
				Md.updateForMeshRenderer(false);
				return true;
			}
			if (file.thumb_error || file.Thumbnail == null)
			{
				if (this.MILogo != null && this.MILogo.Tx != null)
				{
					this.DescThumb.MI = this.MILogo;
					Md.initForImg(this.MILogo.Tx, 0.5f * (float)this.MILogo.Tx.width - UiSVD.thumb_w * 0.5f, 0.5f * (float)this.MILogo.Tx.height - UiSVD.thumb_h * 0.5f, UiSVD.thumb_w, UiSVD.thumb_h);
					Md.Col = C32.MulA(MTRX.ColWhite, alpha);
					Md.Rect(0f, 0f, UiSVD.thumb_w, UiSVD.thumb_h, false);
					Md.updateForMeshRenderer(false);
				}
				else
				{
					this.DescThumb.MI = MTRX.MIicon;
					Md.initForImg(MTRX.IconWhite, 0);
					Md.Col = C32.MulA(4282004532U, alpha);
					Md.Rect(0f, 0f, UiSVD.thumb_w, UiSVD.thumb_h, false);
					Md.updateForMeshRenderer(false);
				}
				return true;
			}
			if (this.MILogo != null && this.MILogo.Tx != null)
			{
				this.DescThumb.MI = this.MILogo;
				this.DescThumb.replaceMainTextureManual(file.Thumbnail);
				Md.Col = C32.MulA(MTRX.ColWhite, alpha);
				Md.initForImg(file.Thumbnail);
				Md.DrawCen(0f, 0f, null);
				Md.updateForMeshRenderer(false);
			}
			return true;
		}

		public static void descRedraw()
		{
			if (UiSVD.Instance != null && UiSVD.Instance.DescThumb != null && UiSVD.Instance.DescThumb.isActiveAndEnabled)
			{
				UiSVD.Instance.DescThumb.redraw_flag = true;
			}
		}

		public static void fineDescStringS()
		{
			if (UiSVD.Instance != null)
			{
				UiSVD.Instance.fineDescString();
			}
		}

		public static void checkSavingBinaryS(SVD.sFile Sf)
		{
			if (UiSVD.ui_active)
			{
				UiSVD.Instance.checkSavingBinary(Sf);
			}
		}

		private bool selectSvdRow(aBtnSvd Row)
		{
			if (this.BxDesc == null || !this.BxDesc.isActive())
			{
				return false;
			}
			IN.clearPushDown(false);
			if (Row.getSvdData() == null)
			{
				if (this.only_load)
				{
					return false;
				}
				if (!this.cannot_save)
				{
					this.executeSave(Row.getSvdIndex());
					this.fnHoverScdRow(Row);
					return false;
				}
			}
			UiBoxDesigner.BlurFrom(this.BxMain);
			if (this.Hd == null)
			{
				this.Hd = new GameObject("HideScreen").AddComponent<HideScreen>();
				this.Hd.gameObject.layer = IN.LAY(IN.gui_layer_name);
				this.Hd.Col = C32.d2c(1996488704U);
				IN.setZ(this.Hd.transform, this.BxMain.transform.position.z - 0.24f);
			}
			this.Hd.activate();
			this.ui_state = UiSVD.STATE.SELECTING;
			if (this.DsSubmittion != null)
			{
				this.DsSubmittion.hide();
			}
			this.BxMain.hide();
			Vector3 vector = this.BaseTrs.InverseTransformPoint(new Vector3(0f, 1.171875f, 0f));
			vector.x *= 64f;
			vector.y *= 64f;
			this.BxDesc.position(vector.x, vector.y, -1000f, -1000f, false);
			this.BxDesc.margin_in_tb = 50f;
			string text = "";
			if (!this.only_load && this.cannot_save)
			{
				text = TX.Get("SVD_need_bench", "");
			}
			else if (!this.only_load && Row.getSvdIndex() == 0)
			{
				text = "<font color=\"#E70B0B\">" + TX.Get("SVD_Alert_manual_save_on_auto_save_slot", "") + "</font>";
			}
			if (TX.valid(text))
			{
				this.BxDesc.WHanim(756f, this.desc_h + 120f + 48f, true, true);
				this.BxDesc.Br().addP(new DsnDataP("", false)
				{
					name = "need_bench",
					text = text,
					size = 15f,
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.TOP,
					Col = MTRX.ColTrnsp,
					TxCol = C32.d2c(4283780170U),
					swidth = this.BxDesc.use_w,
					sheight = 48f,
					text_margin_y = 0f,
					text_auto_condense = true,
					html = true
				}, false);
			}
			else
			{
				this.BxDesc.WHanim(756f, this.desc_h + 120f, true, true);
			}
			if (this.only_load)
			{
				this.createIgnoringCheck();
			}
			Designer bxDesc = this.BxDesc;
			DsnDataButtonMulti dsnDataButtonMulti = new DsnDataButtonMulti();
			dsnDataButtonMulti.name = "cmds";
			DsnDataButtonMulti dsnDataButtonMulti2 = dsnDataButtonMulti;
			string[] array2;
			if (!this.only_load)
			{
				string[] array = new string[3];
				array[0] = "&&SVD_cmd_save";
				array[1] = "&&Cancel";
				array2 = array;
				array[2] = "&&SVD_cmd_load";
			}
			else
			{
				string[] array3 = new string[2];
				array3[0] = "&&SVD_cmd_load";
				array2 = array3;
				array3[1] = "&&Cancel";
			}
			dsnDataButtonMulti2.titles = array2;
			dsnDataButtonMulti.w = 180f;
			dsnDataButtonMulti.margin_w = 20f;
			dsnDataButtonMulti.h = 30f;
			dsnDataButtonMulti.clms = (this.only_load ? 2 : 3);
			dsnDataButtonMulti.fnClick = new FnBtnBindings(this.fnClickSelecting);
			BtnContainer<aBtn> btnContainer = bxDesc.addButtonMultiT<aBtnNel>(dsnDataButtonMulti);
			if (!this.only_load && this.cannot_save)
			{
				btnContainer.Get(0).SetLocked(true, true, false);
			}
			btnContainer.Get(this.only_load ? 0 : 1).Select(true);
			if (Row.getSvdData() == null)
			{
				btnContainer.Get("&&SVD_cmd_load").SetLocked(true, true, false);
			}
			return true;
		}

		public aBtn SelectLastFocusedRow(bool return_empty)
		{
			aBtnSvd aBtnSvd = this.GetBtn(UiSVD.last_focused);
			if (aBtnSvd == null && return_empty)
			{
				aBtnSvd = this.GetBtn(0);
			}
			if (aBtnSvd != null)
			{
				aBtnSvd.Select(true);
			}
			return aBtnSvd;
		}

		public void createIgnoringCheck()
		{
			this.BxDesc.Br().addButtonT<aBtnNel>(new DsnDataButton
			{
				name = "ignore_svd_cfg",
				title = "&&SVD_ignore_svd_cfg",
				skin = "checkbox",
				w = 350f,
				h = 16f,
				def = (this.ignore_svd_cfg || X.DEBUGNOCFG),
				fnClick = new FnBtnBindings(this.fnClickIgnoreCfg)
			});
			this.BxDesc.Br();
		}

		private void loadAskAgain()
		{
			if (this.BxDesc == null || !this.BxDesc.isActive())
			{
				return;
			}
			IN.clearPushDown(false);
			this.ui_state = UiSVD.STATE.LOAD_ASK_AGAIN;
			Vector3 vector = (vector = this.BaseTrs.InverseTransformPoint(new Vector3(0f, 0.078125f, 0f)));
			vector.x *= 64f;
			vector.y *= 64f;
			this.BxDesc.item_margin_y_px = 18f;
			this.BxDesc.Clear();
			this.BxDesc.position(vector.x, vector.y, -1000f, -1000f, false);
			this.BxDesc.margin_in_tb = 40f;
			this.BxDesc.WHanim(756f, this.desc_h - 80f, true, true);
			this.BxDesc.init();
			this.BxDesc.alignx = ALIGN.CENTER;
			this.BxDesc.addP(new DsnDataP("", false)
			{
				text = TX.GetA("SVD_Load_Ask_Again", UiSVD.last_focused.ToString() ?? ""),
				alignx = ALIGN.CENTER,
				aligny = ALIGNY.MIDDLE,
				Col = MTRX.ColTrnsp,
				TxCol = C32.d2c(4283780170U),
				size = 18f,
				swidth = this.BxDesc.use_w,
				sheight = 40f,
				lineSpacing = 1.1f,
				text_auto_condense = true,
				html = true
			}, false);
			if (!this.only_load)
			{
				this.createIgnoringCheck();
			}
			this.BxDesc.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
			{
				name = "cmds",
				titles = new string[] { "&&SVD_cmd_load_do", "&&Cancel" },
				w = 240f,
				h = 32f,
				margin_w = 20f,
				clms = 2,
				fnClick = new FnBtnBindings(this.fnClickSelecting)
			}).Get(0).Select(true);
		}

		private bool fnClickIgnoreCfg(aBtn B)
		{
			B.SetChecked(!B.isChecked(), true);
			if (B.title == "&&SVD_ignore_svd_cfg")
			{
				this.ignore_svd_cfg = B.isChecked();
			}
			return true;
		}

		private bool executeSave(int index)
		{
			if (this.only_load || this.cannot_save)
			{
				return false;
			}
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			if (nelM2DBase == null)
			{
				return false;
			}
			SVD.sFile file = SVD.GetFile(index, true);
			SVD.sFile sFile = SVD.createFile(index, true);
			bool flag = true;
			COOK.createNewSave(sFile, nelM2DBase, flag);
			aBtnSvd btn = this.GetBtn(index);
			bool flag2 = true;
			if (TX.noe(COOK.save_failure_announce))
			{
				if (file != null)
				{
					file.destruct();
				}
				btn.setData(index, sFile, true);
				X.dl("saved", null, false, false);
			}
			else
			{
				SVD.replaceSaveFile(index, file);
				COOK.replaceCurFileTo(file);
				X.dl("save Failure!!", null, false, false);
				flag2 = false;
			}
			btn.SetLocked(false, true, false);
			if (sFile.index >= 1 && (this.BtTop == null || this.BtTop.getSvdIndex() >= sFile.index))
			{
				this.BtTop = btn;
			}
			if (this.FirstExistRow == null || this.FirstExistRow.getSvdIndex() >= sFile.index)
			{
				this.FirstExistRow = btn;
			}
			if (this.LastExistRow == null || this.LastExistRow.getSvdIndex() <= sFile.index)
			{
				this.LastExistRow = btn;
			}
			this.fineSubmitionNavi();
			return flag2;
		}

		private bool fnClickSelecting(aBtn B)
		{
			if (this.ui_state == UiSVD.STATE.SELECTING || this.ui_state == UiSVD.STATE.LOAD_ASK_AGAIN)
			{
				string title = B.title;
				if (title != null)
				{
					if (!(title == "&&Cancel"))
					{
						if (title == "&&SVD_cmd_save")
						{
							bool flag = this.executeSave(UiSVD.last_focused);
							this.ui_state = (flag ? UiSVD.STATE.SELECTING_CANCEL_NO_WRITE_MEMO : UiSVD.STATE.SELECTING_CANCEL);
							return flag;
						}
						if (title == "&&SVD_cmd_load" || title == "&&SVD_cmd_load_do")
						{
							if (SVD.GetFile(UiSVD.last_focused, true) == null)
							{
								return true;
							}
							this.saveMemo();
							if (!this.only_load && this.ui_state != UiSVD.STATE.LOAD_ASK_AGAIN)
							{
								this.ui_state = UiSVD.STATE.SELECTING_TO_LOAD_ASK_AGAIN;
							}
							else
							{
								this.ui_state = UiSVD.STATE.LOAD_SUCCESS;
							}
						}
					}
					else
					{
						this.ui_state = UiSVD.STATE.SELECTING_CANCEL;
					}
				}
			}
			return true;
		}

		private void checkSavingBinary(SVD.sFile Sf)
		{
			aBtnSvd btn = this.GetBtn(Sf.index);
			if (btn != null)
			{
				btn.setData(Sf.index, Sf, false);
			}
			if (COOK.loaded_index != this.pre_marked)
			{
				aBtnSvd aBtnSvd = this.GetBtn(this.pre_marked);
				if (aBtnSvd != null)
				{
					aBtnSvd.fineMarker();
				}
				aBtnSvd = this.GetBtn(COOK.loaded_index);
				if (aBtnSvd != null)
				{
					aBtnSvd.fineMarker();
				}
				this.pre_marked = COOK.loaded_index;
			}
		}

		public int isLoadSuccess()
		{
			if (this.ui_state != UiSVD.STATE.LOAD_SUCCESS)
			{
				return -1;
			}
			return UiSVD.last_focused;
		}

		public void initRevealLocalButton()
		{
			if (this.SkRevealLocal != null)
			{
				return;
			}
			aBtnNel aBtnNel = IN.CreateGobGUI(this.BxMain.gameObject, "-Reveal").AddComponent<aBtnNel>();
			aBtnNel.w = 28f;
			aBtnNel.h = 28f;
			IN.PosP2(aBtnNel.transform, this.BxMain.w * 0.5f + 20f, this.BxMain.h * 0.5f - aBtnNel.h * 0.5f);
			aBtnNel.unselectable(true);
			aBtnNel.initializeSkin("mini_dark", "directory");
			aBtnNel.addHoverFn(delegate(aBtn B)
			{
				B.setSkinTitle("directory_open");
				return true;
			});
			aBtnNel.addOutFn(delegate(aBtn B)
			{
				B.setSkinTitle("directory");
				return true;
			});
			aBtnNel.addClickFn(delegate(aBtn B)
			{
				if (this.ui_state != UiSVD.STATE.ACTIVE)
				{
					return false;
				}
				SVD.revealLocalDirectory();
				return true;
			});
			this.BxMain.addGameObject(aBtnNel.gameObject, "__RevealLocal", -0.01f, false);
			this.SkRevealLocal = aBtnNel.get_Skin() as ButtonSkinMiniNelDark;
			this.SkRevealLocal.bg_color = C32.Color32ToCode(C32.MulA(4282004532U, 0.7f));
		}

		public void unfocus()
		{
			if (this.GetBtn(UiSVD.last_focused) != null)
			{
				this.saveMemo();
			}
		}

		public void reloadLocalFiles()
		{
			if (this.ui_state >= UiSVD.STATE.LOAD_SUCCESS || this.ui_state < UiSVD.STATE.ACTIVE)
			{
				return;
			}
			if (this.ui_state == UiSVD.STATE.LOAD_ASK_AGAIN || this.ui_state == UiSVD.STATE.SELECTING)
			{
				this.ui_state = UiSVD.STATE.SELECTING_CANCEL;
				this.runBoxDesignerEdit(false);
			}
			if (this.ui_state != UiSVD.STATE.ACTIVE)
			{
				return;
			}
			this.save_memo_lock = true;
			this.desc_first_assign = true;
			bool flag = false;
			if (aBtn.PreSelected is aBtnSvd)
			{
				flag = true;
				aBtn.PreSelected.Deselect(true);
			}
			SVD.prepareList(true);
			SVD.sFile file = SVD.GetFile(0, true);
			this.BtAs.setData(0, file, false);
			if (this.RowCon.APool == null)
			{
				this.RowCon.APool = new List<aBtn>(this.RowCon.Length);
			}
			this.BtTop = null;
			this.RowCon.RemakeT<aBtnSvd>(null, "");
			UiSVD.last_focused = X.MMX(0, UiSVD.last_focused, this.RowCon.Length + 1);
			if (flag)
			{
				this.SelectLastFocusedRow(true);
			}
			this.fineRowConNavi();
			this.fineSubmitionNavi();
			this.save_memo_lock = false;
		}

		private UiBoxDesigner BxMain;

		private UiBoxDesigner BxDesc;

		private bool only_load;

		private Designer DsSubmittion;

		public const int file_max = 99;

		public const float desc_w = 420f;

		private const float thumb_caption_h = 80f;

		public int f0;

		public const float desc_margin_tb = 20f;

		private FillImageBlock DescThumb;

		private Transform BaseTrs;

		public UiSVD.STATE ui_state = UiSVD.STATE.INITTED;

		private aBtnSvd BtAs;

		private BtnContainerRadio<aBtn> RowCon;

		private ButtonSkinMiniNelDark SkRevealLocal;

		public static int last_focused;

		private bool cannot_save;

		private int t_main_hide;

		private float z_desc_pre;

		private HideScreen Hd;

		private int pre_marked;

		private static UiSVD Instance;

		private aBtnSvd BtTop;

		private aBtnSvd FirstExistRow;

		private aBtnSvd LastExistRow;

		private bool desc_hide;

		public bool ignore_svd_cfg;

		private bool desc_first_assign;

		public static bool show_explore_timer;

		public aBtnSvd ThumbRow;

		private int create_thumb_delay = -1;

		private static int activated_id_count;

		private string activated_id;

		private const float Z_DESC = -0.86f;

		private MTI Mti;

		private MImage MILogo;

		private bool save_memo_lock;

		public byte[] Abuffer_for_header;

		public enum STATE
		{
			OFFLINE,
			INITTED,
			ACTIVE,
			SELECTING,
			SELECTING_CANCEL,
			SELECTING_CANCEL_NO_WRITE_MEMO,
			SELECTING_TO_LOAD_ASK_AGAIN,
			LOAD_ASK_AGAIN,
			LOAD_SUCCESS
		}
	}
}
