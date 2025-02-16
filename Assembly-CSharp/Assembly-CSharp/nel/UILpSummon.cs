using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class UILpSummon : IPauseable, IValotileSetable
	{
		public UILpSummon(M2LpSummon _Lp)
		{
			this.Lp = _Lp;
			this.M2D = this.Lp.nM2D;
			this.Reader = this.Lp.Reader;
			this.StbDesc = new STB(100);
			this.createUi(out this.BxT, out this.BxDL, out this.BxDB, out this.BxDR, out this.ConfirmSkin);
		}

		public void destruct()
		{
			this.deactivate();
			IN.DestroyOne(this.BxT.gameObject);
			IN.DestroyOne(this.BxDL.gameObject);
			IN.DestroyOne(this.BxDB.gameObject);
			IN.DestroyOne(this.BxDR.gameObject);
			this.StbDesc.Clear();
			this.bxdb_created = 0;
		}

		private void createUi(out MsgBoxGrdBanner BxT, out UiBoxDesigner BxDL, out UiBoxDesigner BxDB, out UiBox BxDR, out ButtonSkinNormalNel ConfirmSkin)
		{
			string unique_key = this.Lp.unique_key;
			BxT = null;
			BxDL = null;
			BxDB = null;
			BxDR = null;
			ConfirmSkin = null;
			for (int i = 0; i < 4; i++)
			{
				UiBox uiBox = null;
				MsgBox msgBox;
				GameObject gameObject;
				if (i == 0)
				{
					MsgBoxGrdBanner msgBoxGrdBanner;
					BxT = (msgBoxGrdBanner = new GameObject("LpSummon-" + unique_key + "-Box-" + i.ToString()).AddComponent<MsgBoxGrdBanner>());
					msgBox = msgBoxGrdBanner;
					msgBox.use_valotile = this.M2D.use_valotile;
					gameObject = msgBox.gameObject;
				}
				else
				{
					if (i == 1 || i == 3)
					{
						UiBoxDesigner uiBoxDesigner = new GameObject("LpSummon-" + unique_key + "-Box-" + i.ToString()).AddComponent<UiBoxDesigner>();
						gameObject = uiBoxDesigner.gameObject;
						if (i == 1)
						{
							BxDL = uiBoxDesigner;
						}
						else
						{
							BxDB = uiBoxDesigner;
							BxDB.box_stencil_ref_mask = 250;
						}
						uiBoxDesigner.Focusable(false, true, null);
						uiBoxDesigner.use_valotile = true;
						uiBox = uiBoxDesigner.getBox();
						uiBoxDesigner.alignx = ALIGN.CENTER;
					}
					else
					{
						UiBox uiBox2;
						BxDR = (uiBox2 = new GameObject("LpSummon-" + unique_key + "-Box-" + i.ToString()).AddComponent<UiBox>());
						uiBox = uiBox2;
						uiBox.use_valotile = this.M2D.use_valotile;
						gameObject = BxDR.gameObject;
						BxDR.use_focus = false;
						BxDR.mouse_click_focus = false;
					}
					msgBox = uiBox;
					msgBox.delayt = 40 + (i - 1) * 8;
					uiBox.bkg_scale(true, true, false);
				}
				IN.setZAbs(gameObject.transform, 27f);
				msgBox.gradation(ItemDescBox.Acol_normal, null).TxCol(uint.MaxValue).Align(ALIGN.CENTER, ALIGNY.MIDDLE);
				if (uiBox != null)
				{
					uiBox.frametype = UiBox.FRAMETYPE.NO_OVERRIDE;
				}
			}
			BxT.gameObject.layer = (BxDL.gameObject.layer = (BxDB.gameObject.layer = (BxDR.gameObject.layer = IN.gui_layer)));
			BxT.TargetFont = TX.getTitleFont();
			BxT.margin(new float[] { 60f, 10f }).TxSize(30f).bkg_scale(false, true, false)
				.AlignY(ALIGNY.MIDDLE);
			BxT.html_mode = true;
			BxT.LineSpacing(0.7f);
			BxT.TxCol(this.Reader.is_dangerous ? 4279500800U : uint.MaxValue);
			BxT.col(this.Reader.is_dangerous ? C32.d2c(4292874256U) : MTRX.ColMenu);
			BxT.wh(X.Mx(BxT.get_text_swidth_px(), 460f) + (float)(this.Reader.is_dangerous ? 45 : 0), (float)(this.Reader.is_dangerous ? 50 : 34));
			BxT.make(this.Reader.name_localized + (this.Reader.is_dangerous ? ("\n" + TX.Get("Alert_dangerous", "")) : ""));
			BxT.GradationDirect(180f, 0f);
			BxDR.margin(new float[] { 62f, 24f });
			BxDL.margin(new float[] { 30f, 24f });
			BxDB.margin(new float[] { 60f, 6f });
			BxDL.WH(320f, 120f);
			BxDR.swh(320f, 120f);
			BxDB.WH(684f, 40f);
			BxDB.alignx = ALIGN.LEFT;
			BxDB.margin_in_lr = 38f;
			BxDB.margin_in_tb = 6f;
			BxDL.init();
			BxDR.make("");
			this.bxdb_created = 0;
			aBtnNel aBtnNel = IN.CreateGobGUI(BxDR.gameObject, "LpSummon-" + unique_key + "Bt").AddComponent<aBtnNel>();
			aBtnNel.title = "summon_submit";
			aBtnNel.w = BxDL.use_w;
			aBtnNel.h = BxDL.use_h + (float)(X.ENG_MODE ? 8 : 0);
			aBtnNel.initializeSkin("normal_dark", "");
			aBtnNel.get_Skin().html_mode = true;
			aBtnNel.get_Skin().setTitle(TX.Get("Summoner__initialize", ""));
			aBtnNel.unselectable(true);
			aBtnNel.click_snd = "";
			BxDL.addP(new DsnDataP("", false)
			{
				size = 13f,
				TxCol = C32.d2c(uint.MaxValue),
				html = true,
				text = " ",
				alignx = ALIGN.LEFT,
				aligny = ALIGNY.MIDDLE,
				swidth = BxDL.use_w,
				sheight = BxDL.use_h,
				name = "descl"
			}, false);
			ConfirmSkin = aBtnNel.get_Skin() as ButtonSkinNormalNel;
			ConfirmSkin.html_mode = true;
		}

		public void releaseTextCache()
		{
			this.StbDesc.Clear();
			this.bxdb_created = 0;
		}

		public void deactivate()
		{
			IN.FlgUiUse.Rem("LPSUMMON");
			NEL.stopPressingSound("LP");
			this.deactivateLinks();
			this.BxT.deactivate();
			this.BxDL.deactivate();
			this.BxDB.deactivate();
			this.BxDR.deactivate();
			this.ConfirmSkin.getBtn().hide();
			this.ConfirmSkin.getBtn().gameObject.SetActive(false);
			this.StbDesc.Clear();
			this.hold_submit_time = X.Mn(this.hold_submit_time, 0);
		}

		public void activate()
		{
			IN.FlgUiUse.Add("LPSUMMON");
			this.BxT.activate();
			this.BxDL.activate();
			this.BxDB.activate();
			this.BxDR.activate();
			this.hold_submit_time = 0;
			this.attach_links = true;
			byte b = (this.M2D.NightCon.alreadyCleardInThisSession(this.Lp) ? 2 : 1);
			if (this.bxdb_created != b)
			{
				this.bxdb_created = b;
				this.Reader.recreateMBoxList(this.BxDB, this.bxdb_created == 2);
			}
			if (this.StbDesc.Length == 0)
			{
				this.Reader.getDescription(this.Lp.Mp, this.StbDesc, true);
				this.BxDL.checkInit();
				(this.BxDL.Get("descl", false) as FillBlock).Txt(this.StbDesc);
			}
		}

		public void deactivateLinks()
		{
			this.attach_links = false;
		}

		public bool isActive()
		{
			return this.attach_links_;
		}

		public bool run(float fcnt, bool first)
		{
			Map2d mp = this.Lp.Mp;
			Vector4 vector = new Vector4(this.Lp.mapfocx, this.Lp.mapfocy, 0.0001f, 0.0001f);
			M2BoxOneLine.fineBoxPosOnMap(this.BxT, mp.M2D, vector, first, true, 0f, 0f);
			M2BoxOneLine.fineBoxPosOnMap(this.BxDL, mp.M2D, vector, first, true, -342f + this.BxDL.swidth / 2f, -100f);
			M2BoxOneLine.fineBoxPosOnMap(this.BxDR, mp.M2D, vector, first, true, 342f - this.BxDR.swidth / 2f, -100f);
			M2BoxOneLine.fineBoxPosOnMap(this.BxDB, mp.M2D, vector, first, true, 0f, -166f - this.BxDB.get_sheight_px() * 0.5f);
			if (this.BxDR.isActive() && !this.BxDR.show_delaying && !this.ConfirmSkin.getBtn().gameObject.activeSelf)
			{
				this.ConfirmSkin.getBtn().bind();
				this.ConfirmSkin.getBtn().gameObject.SetActive(true);
			}
			if (X.D || first)
			{
				this.ConfirmSkin.getBtn().setAlpha(this.BxDR.showing_alpha);
			}
			bool flag = (mp.Pr.hasFoot() && mp.Pr.isNormalState()) || this.ConfirmSkin.isChecked();
			bool flag2 = this.BxDR.isShown() && this.M2D.t_lock_check_push_up == 0f && (this.ConfirmSkin.isChecked() ? mp.Pr.isCheckO(0) : mp.Pr.isCheckPD(1));
			if (flag2 && flag && !mp.Pr.isLO(0) && !mp.Pr.isRO(0))
			{
				mp.Pr.jump_hold_lock = true;
			}
			if (this.BxDR.isShown() && NEL.confirmHold("LP", ref this.hold_submit_time, 120, this.ConfirmSkin, false, flag ? (this.ConfirmSkin.isPushDown() ? 3 : (flag2 ? 2 : 0)) : 0, false))
			{
				return true;
			}
			IN.isSubmitOn(0);
			return false;
		}

		public bool attach_links
		{
			get
			{
				return this.attach_links_;
			}
			set
			{
				if (this.attach_links == value)
				{
					return;
				}
				this.attach_links_ = value;
				if (value)
				{
					this.M2D.AssignPauseableP(this);
					this.M2D.addValotAddition(this);
					return;
				}
				this.M2D.DeassignPauseable(this);
				this.M2D.remValotAddition(this);
			}
		}

		public void Pause()
		{
			this.BxT.enabled = false;
			this.BxDL.Pause();
			this.BxDR.enabled = false;
			this.BxDB.Pause();
		}

		public void Resume()
		{
			this.BxT.enabled = true;
			this.BxDL.Resume();
			this.BxDR.enabled = true;
			this.BxDB.Resume();
		}

		public bool use_valotile
		{
			get
			{
				return this.use_valotile_;
			}
			set
			{
				this.use_valotile_ = value;
				this.BxT.use_valotile = value;
				this.BxDL.use_valotile = value;
				this.BxDR.use_valotile = value;
				this.BxDB.use_valotile = value;
				this.ConfirmSkin.use_valotile = value;
			}
		}

		public readonly EnemySummoner Reader;

		public readonly M2LpSummon Lp;

		public NelM2DBase M2D;

		private readonly MsgBoxGrdBanner BxT;

		private readonly UiBoxDesigner BxDL;

		private readonly UiBoxDesigner BxDB;

		private readonly UiBox BxDR;

		private readonly ButtonSkinNormalNel ConfirmSkin;

		public const float desc_w = 320f;

		public const float desc_h = 120f;

		private bool attach_links_;

		private const float shiftx = 342f;

		private readonly STB StbDesc;

		private int hold_submit_time = -20;

		private byte bxdb_created;

		private bool use_valotile_;
	}
}
