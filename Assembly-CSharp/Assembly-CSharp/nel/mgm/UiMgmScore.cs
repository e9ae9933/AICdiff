using System;
using System.Collections.Generic;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel.mgm
{
	public class UiMgmScore : MsgBox
	{
		protected override void Awake()
		{
			base.Awake();
			this.Acurrent_score = new List<int>(3);
			base.gameObject.layer = IN.gui_layer;
			this.gradation(new Color32[]
			{
				C32.d2c(3453277396U),
				C32.d2c(3449851562U)
			}, null);
			base.bkg_scale(false, false, false);
			this.html_mode = true;
			this.make(" ");
			this.TxR = IN.CreateGob(base.gameObject, "-txR").AddComponent<TextRenderer>();
			this.MdBkg.chooseSubMesh(1, false, false);
			this.MdBkg.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, -1), false);
			this.MdBkg.chooseSubMesh(2, false, false);
			this.MdBkg.setMaterial(MTRX.getMtr(BLEND.MASK, 230), false);
			this.MdBkg.chooseSubMesh(0, false, false);
			this.MdBkg.connectRendererToTriMulti(this.MMRD.GetMeshRenderer(this.MdBkg));
			this.M2D = M2DBase.Instance as NelM2DBase;
			if (this.M2D != null)
			{
				this.M2D.addValotAddition(this);
			}
		}

		public override bool use_valotile
		{
			set
			{
				base.use_valotile = value;
				this.TxR.use_valotile = value;
			}
		}

		public UiMgmScore InitAftGame(bool _updated, MGMSCORE _msc)
		{
			return this.Init(_updated ? UiMgmScore.STATE.AFT_GAME_HIGHSCORE : UiMgmScore.STATE.AFT_GAME, _msc);
		}

		public UiMgmScore Init(UiMgmScore.STATE _state, MGMSCORE _msc)
		{
			this.maxt_pos = (this.maxt_pos_hide = 30);
			IN.setZAbs(base.transform, -3.275f);
			this._bkg_scale &= 4294967071U;
			this.Acurrent_score.Clear();
			this.msc = _msc;
			this.Tx.gameObject.SetActive(true);
			this.TxR.gameObject.SetActive(false);
			this.Tx.alignx = (this.TxR.alignx = (this.alignx = ALIGN.LEFT));
			this.Tx.aligny = (this.TxR.aligny = (this.aligny = ALIGNY.BOTTOM));
			this.Tx.line_spacing = (this.TxR.line_spacing = 1.25f);
			base.TxCol(NEL.ColText);
			this.Tx.TextColor = (this.TxR.TextColor = NEL.ColText);
			this.Tx.html_mode = (this.TxR.html_mode = true);
			this.tmarg = (this.bmarg = 20f);
			this.lmarg = (this.rmarg = 40f);
			this.Tx.size = (this.TxR.size = 14f);
			this.Tx.StencilRef(-1);
			base.scaling_alpha_set(false, false);
			if (_state != UiMgmScore.STATE.PRE_GAME)
			{
				if (_state - UiMgmScore.STATE.AFT_GAME > 1)
				{
					return this;
				}
				base.posSetA(0f, 25f, 0f, 25f, false);
				base.scaling_alpha_set(false, true);
				this.lmarg = (this.rmarg = 30f);
				this.bmarg = 30f;
				this.tmarg = 280f - this.bmarg - 40f;
				base.swh_anim(550f, 280f, false, false);
				this.Tx.size = 16f;
				if (_state == UiMgmScore.STATE.AFT_GAME_HIGHSCORE)
				{
					this.Tx.StencilRef(230);
					this.Tx.Txt(TX.Get("Mgm_ui_updated_highscore", ""));
				}
				else
				{
					this.fineAftGameString();
				}
				SND.Ui.play("mgm_tsukuten", false);
			}
			else
			{
				this.maxt_pos = (this.maxt_pos_hide = 80);
				this.finePreGameString();
				base.posSetA(0f, IN.hh + 90f, 0f, IN.hh * 0.8f - base.get_sheight_px() * 0.5f, false);
			}
			this.t_hide = this.maxt_pos_hide;
			this.stt = _state;
			base.fineTxPos();
			this.activate();
			return this;
		}

		public UiMgmScore CurScore(int s)
		{
			this.Acurrent_score.Add(s);
			return this;
		}

		public override MsgBox deactivate()
		{
			if (base.isActive())
			{
				UiMgmScore.STATE state = this.stt;
				if (state - UiMgmScore.STATE.AFT_GAME <= 1)
				{
					base.posSetA(0f, -65f, 0f, 25f, false);
				}
			}
			return base.deactivate();
		}

		private void finePreGameString()
		{
			this.aligny = ALIGNY.MIDDLE;
			this.bmarg = (this.tmarg = 14f);
			this.TxR.gameObject.SetActive(true);
			MgmScoreHolder.SHld shld = COOK.Mgm.AHld[(int)this.msc];
			int length = shld.Length;
			using (STB stb = TX.PopBld(null, 0))
			{
				using (STB stb2 = TX.PopBld(null, 0))
				{
					stb.AddTxA("Mgm_ui_highscore", false);
					for (int i = 0; i < length; i++)
					{
						stb.Ret("\n");
						stb2.Add(" ", shld[i].score, shld[i].suffix).Ret("\n");
					}
					stb.AddTxA("Mgm_ui_playcount", false);
					stb2.AddTxA("Mgm_ui_playcount_content", false).TxRpl(shld.play_count);
					this.Tx.Txt(stb);
					this.TxR.Txt(stb2);
					this.TxR.Align(ALIGN.RIGHT);
					this.Tx.AlignY(this.aligny);
					this.TxR.AlignY(this.aligny);
				}
			}
			base.swh_anim(350f, this.bmarg + this.tmarg + (float)(16 * (length + 1)), false, false);
			base.fineTxPos(this.TxR, ALIGN.RIGHT, ALIGNY.MIDDLE);
		}

		private void fineAftGameString()
		{
			MgmScoreHolder.SHld shld = COOK.Mgm.AHld[(int)this.msc];
			int length = shld.Length;
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AddTxA("Mgm_ui_highscore", false).Add(" ");
				for (int i = 0; i < length; i++)
				{
					stb.Add("", shld[i].score, shld[i].suffix);
					if (i < length - 1)
					{
						stb.Add(" / ");
					}
				}
				this.Tx.Txt(stb);
				this.Tx.Col(MTRX.ColTrnsp);
			}
		}

		public override void destruct()
		{
			base.destruct();
			if (this.M2D != null)
			{
				this.M2D.remValotAddition(this);
			}
			if (UiMgmScore.Instance == this)
			{
				UiMgmScore.Instance = null;
			}
		}

		public override MsgBox run(float fcnt, bool force_draw)
		{
			UiMgmScore.STATE state = this.stt;
			if (state - UiMgmScore.STATE.AFT_GAME <= 1)
			{
				force_draw = true;
			}
			if (base.run(fcnt, force_draw) == null)
			{
				if (!EV.isActive(false))
				{
					IN.DestroyE(base.gameObject);
					if (UiMgmScore.Instance == this)
					{
						UiMgmScore.Instance = null;
					}
				}
				return null;
			}
			return this;
		}

		protected override MsgBox redrawBg(float w = 0f, float h = 0f, bool update_mrd = true)
		{
			if (!this.ginitted)
			{
				return this;
			}
			if (w <= 0f)
			{
				w = base.swidth;
			}
			if (h <= 0f)
			{
				h = base.sheight;
			}
			this.MdBkg.clear(false, false);
			this.MdBkg.chooseSubMesh(0, false, false);
			MdArranger mdArranger = this.MMRD.GetArranger(this.MdBkg).clear(this.MdBkg);
			UiMgmScore.STATE state = this.stt;
			if (state - UiMgmScore.STATE.AFT_GAME <= 1)
			{
				float num = 1f;
				if (base.isActive())
				{
					float num2 = 0.44f + 0.56f * X.ZPOW(this.t, 23f) + (-X.ZSIN(this.t - 23f, 14f) + X.ZSINV(this.t - 23f - 14f, 14f)) * 0.12f;
					num *= 3f - X.ZLINE(this.t, 15f) * 2f;
					w *= num2;
					h *= num2;
				}
				base.redrawBg(w, h, false);
				this.MdBkg.Col = C32.MulA(4294966715U, base.alpha * 0.5f);
				this.MdBkg.Scale(1f, 0.8f, false).TranslateP(0f, 14f, false);
				this.MdBkg.Star(0f, 0f, 110f * num, X.ANMPT(120, 1f) * 6.2831855f, 12, 0.66f, 0f, false, 0f, 0f);
				this.MdBkg.Identity();
				float num3 = ((!base.isActive()) ? 1000f : (this.t - 50f));
				if (num3 >= 0f)
				{
					if (this.stt == UiMgmScore.STATE.AFT_GAME_HIGHSCORE)
					{
						this.MdBkg.Col = C32.MulA(4294966715U, base.alpha);
						float num4 = base.swidth - this.lmarg - this.rmarg - 40f;
						float num5 = -base.swidth * 0.5f;
						float num6 = -base.sheight * 0.5f + this.bmarg - 2f;
						if ((this._bkg_scale & 32U) == 0U && base.isActive())
						{
							this._bkg_scale |= 32U;
							SND.Ui.play("mgm_highscore", false);
						}
						if (num3 < 8f)
						{
							this.MdBkg.RectBL(num5, num6, num4, 20f, false);
							if (num3 >= 3f && num3 <= 5f)
							{
								this.MdBkg.Col = C32.MulA(uint.MaxValue, base.alpha);
								this.MdBkg.StripedRectBL(num5, num6, num4, 20f, 0f, 0.5f, 10f, false);
							}
						}
						else
						{
							this.Tx.Col((X.ANMT(2, 10f) == 0) ? NEL.ColFocus : MTRX.ColWhite);
							float num7 = X.ZLINE(num3 - 8f, 20f);
							float num8 = (float)((int)num4) * (1f - num7);
							float num9 = num5 + (num4 - num8);
							if (num7 < 1f)
							{
								this.MdBkg.RectBL(num5, num6, num4, 20f, false);
							}
							this.MdBkg.chooseSubMesh(2, false, false);
							this.MdBkg.Col = MTRX.ColTrnsp;
							this.MdBkg.RectBL(num5, num6, num9 - num5, 20f, false);
							this.MdBkg.chooseSubMesh(0, false, false);
							this.MdBkg.Col = C32.MulA(NEL.ColText, base.alpha);
							this.MdBkg.RectBL(num5, num6, num9 - num5, 20f, false);
						}
					}
					else
					{
						float num10 = X.ZSIN(num3, 25f);
						this.Tx.Col(C32.MulA(NEL.ColText, num10));
					}
				}
				float num11;
				float num12;
				float num13;
				NEL.BouncyPinZT(out num11, out num12, out num13, 0.25f, -0.5f);
				this.MdBkg.chooseSubMesh(1, false, false);
				mdArranger.Set(true);
				this.MdBkg.Col = C32.WMulA(base.alpha);
				this.MdBkg.ColGrd.Black().mulA(base.alpha);
				MgmScoreHolder.SHld shld = COOK.Mgm.AHld[(int)this.msc];
				float num14 = 0f;
				float num15 = 14f - (MTRX.ChrL.ch * 0.5f + 8f) * 4f + num13 * 10f - 40f * (base.isActive() ? (1f - X.ZSIN(this.t, 30f)) : 0f);
				using (STB stb = TX.PopBld(null, 0))
				{
					int length = shld.Length;
					for (int i = 0; i < length; i++)
					{
						stb.Clear();
						if (i < this.Acurrent_score.Count)
						{
							stb.Add(this.Acurrent_score[i]);
						}
						else
						{
							stb.Add("???");
						}
						num14 += MTRX.ChrL.DrawScaleStringTo(this.MdBkg, stb, num14, num15, 4f * num11, 4f * num12, ALIGN.LEFT, ALIGNY.BOTTOM, false, 0f, 0f, null);
						stb.Set(shld[i].suffix);
						num14 += 12f * num11;
						num14 += MTRX.ChrM.DrawBorderedScaleStringTo(this.MdBkg, stb, num14, num15 + 8f * num12, 4f * num11, 4f * num12, ALIGN.LEFT, ALIGNY.BOTTOM, false, 0f, 0f, null);
						if (i < length - 1)
						{
							stb.Set("/");
							num14 += MTRX.ChrL.DrawScaleStringTo(this.MdBkg, stb, num14, num15, 4f * num11, 4f * num12, ALIGN.LEFT, ALIGNY.BOTTOM, false, 0f, 0f, null);
						}
					}
				}
				mdArranger.Set(false).translateAll(-num14 * 0.5f, 0f, false);
				mdArranger.SetWhole(true);
				if (update_mrd)
				{
					this.MdBkg.updateForMeshRenderer(true);
				}
			}
			else
			{
				base.redrawBg(w, h, update_mrd);
			}
			return this;
		}

		public static UiMgmScore createInstance()
		{
			if (UiMgmScore.Instance == null)
			{
				UiMgmScore.Instance = IN.CreateGobGUI(null, "MgmScore").AddComponent<UiMgmScore>();
			}
			return UiMgmScore.Instance;
		}

		public static void evQuit()
		{
			if (UiMgmScore.Instance != null)
			{
				UiMgmScore.Instance.deactivate();
				UiMgmScore.Instance = null;
			}
		}

		public static void deactivateS()
		{
			if (UiMgmScore.Instance != null)
			{
				UiMgmScore.Instance.deactivate();
			}
		}

		private const int MESH_N = 0;

		private const int MESH_CHR = 1;

		private const int MESH_MASK = 2;

		private const int STENCIL_MASK = 230;

		private const float sheight_aftgame = 280f;

		private const float swidth_aftgame = 550f;

		private const float centery_aftgame = 25f;

		private NelM2DBase M2D;

		private UiMgmScore.STATE stt;

		private MGMSCORE msc;

		public List<int> Acurrent_score;

		private TextRenderer TxR;

		private static UiMgmScore Instance;

		public enum STATE
		{
			PRE_GAME,
			AFT_GAME,
			AFT_GAME_HIGHSCORE
		}
	}
}
