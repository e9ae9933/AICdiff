using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class M2RebagachaAnnounce : M2PrAssistant, IPauseable, IRunAndDestroy, IValotileSetable
	{
		public M2RebagachaAnnounce(PR _Pr)
			: base(_Pr)
		{
		}

		public void initG()
		{
			if (this.MdBg != null)
			{
				return;
			}
			this.Pr.M2D.addValotAddition(this);
			this.Gob = IN.CreateGobGUI(null, "GachaAnnounce");
			this.Trs = this.Gob.transform;
			IN.setZAbs(this.Trs, -4.84f);
			this.MdBg = MeshDrawer.prepareMeshRenderer(this.Gob, MTRX.getMtr(BLEND.MASK, 230), 0f, -1, null, this.use_valotile_, false);
			this.MdBg.chooseSubMesh(0, true, false);
			this.MdBg.chooseSubMesh(1, true, false);
			this.MdBg.base_z = -0.01f;
			this.MdBg.setMaterial(MTR.MIiconL.getMtr(230), false);
			this.MdBg.chooseSubMesh(0, false, false);
			this.MdBg.connectRendererToTriMulti(this.Gob.GetComponent<MeshRenderer>());
			this.Valot = IN.GetOrAdd<ValotileRenderer>(this.Gob);
			this.Valot.enabled = this.use_valotile_;
			this.Tx = IN.CreateGob(this.Gob, "-Tx").AddComponent<TextRenderer>();
			this.Tx.size = 16f;
			this.Tx.stencil_ref = 230;
			this.Tx.Col(MTRX.ColWhite);
			this.Tx.BorderCol(MTRX.ColBlack);
			this.Tx.alignx = ALIGN.CENTER;
			this.Tx.aligny = ALIGNY.MIDDLE;
			this.Tx.use_valotile = this.use_valotile_;
			this.Tx.auto_wrap = true;
			this.Tx.auto_condense = true;
			this.Pr.M2D.AssignPauseableP(this);
			IN.PosP(this.Tx.transform, 30f, 0f, -0.01f);
			this.Gob.SetActive(false);
			this.fineFont();
		}

		public void fineFont()
		{
			if (this.Tx != null)
			{
				this.Tx.Txt(TX.Get("Alert_rebagacha", ""));
				this.Tx.TargetFont = TX.getDefaultFont();
			}
			this.box_w = (X.ENG_MODE ? 320f : 260f);
			this.need_reposit = (this.need_redraw_box = true);
		}

		public void Pause()
		{
			if (this.Gob != null)
			{
				this.Gob.SetActive(false);
			}
			if (this.t_hit > 1f)
			{
				this.t_hit = 1f;
			}
			if (this.t < 0f)
			{
				this.t = -30f;
				this.runner_assigned = false;
				this.MdBg.clear(false, false);
			}
		}

		public void Resume()
		{
			if (this.t >= 80f && this.Gob != null)
			{
				this.Gob.SetActive(true);
			}
		}

		public bool isGobActive()
		{
			return this.t >= 80f && this.MdBg != null && this.Gob.activeSelf;
		}

		public bool runner_assigned
		{
			get
			{
				return this.runner_assigned_;
			}
			set
			{
				if (this.runner_assigned == value || base.Mp == null)
				{
					return;
				}
				this.runner_assigned_ = value;
				if (!value)
				{
					if (base.Mp != null)
					{
						base.Mp.remRunnerObject(this);
					}
					return;
				}
				if (base.Mp == null)
				{
					this.runner_assigned_ = false;
					return;
				}
				base.Mp.addRunnerObject(this);
			}
		}

		public bool use_valotile
		{
			get
			{
				return this.use_valotile_;
			}
			set
			{
				if (this.use_valotile_ == value)
				{
					return;
				}
				this.use_valotile_ = value;
				if (this.MdBg != null)
				{
					this.Valot.enabled = value;
					this.Tx.use_valotile = value;
				}
			}
		}

		public void destruct()
		{
			this.Pr.M2D.remValotAddition(this);
		}

		public void endS()
		{
			this.runner_assigned = false;
			this.runner_assigned_ = false;
			this.actv = M2RebagachaAnnounce.ACTV.OFFLINE;
			this.t = -30f;
			if (this.MdBg != null)
			{
				this.Gob.SetActive(false);
				this.MdBg.clear(false, false);
			}
		}

		public void evInit()
		{
			this.actv |= M2RebagachaAnnounce.ACTV._EVENT_LOCK;
		}

		public void evQuit()
		{
			this.actv &= ~M2RebagachaAnnounce.ACTV._EVENT_LOCK;
			this.fineEnable();
		}

		public bool ev_locked
		{
			get
			{
				return (this.actv & M2RebagachaAnnounce.ACTV._EVENT_LOCK) > M2RebagachaAnnounce.ACTV.OFFLINE;
			}
		}

		public void fineEnable()
		{
			if (this.ev_locked || this.Pr.Ser == null)
			{
				return;
			}
			M2RebagachaAnnounce.ACTV actv = M2RebagachaAnnounce.ACTV.OFFLINE;
			if (this.Pr.is_alive)
			{
				if (this.Pr.Ser.has(SER.FROZEN))
				{
					actv |= M2RebagachaAnnounce.ACTV.FROZEN;
					if (this.Pr.isFrozen())
					{
						actv |= M2RebagachaAnnounce.ACTV.FROZEN_HARD;
					}
				}
				if (this.Pr.Ser.has(SER.STONE))
				{
					actv |= M2RebagachaAnnounce.ACTV.STONE;
					if (this.Pr.isStoneSer())
					{
						actv |= M2RebagachaAnnounce.ACTV.STONE_HARD;
					}
				}
				if (this.Pr.isWebTrappedState(false) && this.Pr.Ser.has(SER.WEB_TRAPPED) && !this.Pr.EpCon.isOrgasm())
				{
					actv |= M2RebagachaAnnounce.ACTV.WEB_TRAPPED;
				}
			}
			if (this.actv != actv)
			{
				this.actv = actv;
				if (this.actv > M2RebagachaAnnounce.ACTV.OFFLINE)
				{
					this.runner_assigned = true;
					if ((this.actv & M2RebagachaAnnounce.ACTV._FORCE_SHOW) != M2RebagachaAnnounce.ACTV.OFFLINE)
					{
						if (this.t < 80f)
						{
							if (this.MdBg != null)
							{
								this.Gob.SetActive(true);
							}
							this.t = 80f;
							this.draw_ni = byte.MaxValue;
							return;
						}
					}
					else if (this.t < 0f)
					{
						this.t = 0f;
						return;
					}
				}
				else if (this.t >= 0f)
				{
					this.t = X.Mn(-1f, -30f + (this.t - 80f));
				}
			}
		}

		public bool run(float fcnt)
		{
			if (this.t >= 0f && this.Pr.is_alive && !this.Pr.Ser.hasBit(720896UL))
			{
				this.runRebagacha(fcnt);
			}
			if (!X.D)
			{
				return true;
			}
			fcnt = (float)X.AF;
			if (this.t >= 0f)
			{
				if ((this.actv & M2RebagachaAnnounce.ACTV._FORCE_SHOW) == M2RebagachaAnnounce.ACTV.OFFLINE)
				{
					bool flag = this.Pr.getAbsorbContainer().isActive();
					if ((this.Pr.isDamagingOrKo() && !flag) || this.Pr.isPunchState() || this.Pr.isMagicState())
					{
						if (this.t >= 80f)
						{
							this.t = 0f;
							this.t_hit = 0f;
							this.Gob.SetActive(false);
							this.MdBg.clear(false, false);
						}
						return true;
					}
					bool flag2 = this.t < 80f;
					if (flag2 && flag)
					{
						this.t = 80f;
					}
					else
					{
						this.t += fcnt;
					}
					if (this.t >= 80f)
					{
						if (flag2)
						{
							this.Gob.SetActive(true);
							this.draw_ni = byte.MaxValue;
						}
						this.need_reposit = true;
						this.need_redraw_box = this.t < 116f;
					}
				}
				else
				{
					this.t += fcnt;
					this.need_reposit = true;
					this.need_redraw_box = this.t < 116f;
				}
			}
			else
			{
				this.t -= fcnt;
				if (this.t <= -30f)
				{
					this.runner_assigned_ = false;
					this.Gob.SetActive(false);
					this.MdBg.clear(false, false);
					this.t_hit = 0f;
					return false;
				}
				this.need_reposit = (this.need_redraw_box = true);
			}
			if (this.t_hit > 0f)
			{
				this.t_hit = X.Mx(0f, this.t_hit - fcnt);
				this.need_redraw_box = true;
			}
			if (this.need_reposit)
			{
				this.need_reposit = false;
				if (this.need_redraw_box)
				{
					this.need_redraw_box = false;
					this.redrawBox();
					this.draw_ni = byte.MaxValue;
				}
				Vector2 vector = base.NM2D.Cam.PosMainTransform;
				Vector2 vector2 = new Vector2(base.Mp.map2globalux(this.Pr.x), base.Mp.map2globaluy(this.Pr.y)) - vector;
				vector2 *= base.NM2D.Cam.getScaleRev();
				float num = (float)X.MPF(vector2.y < IN.hh * 0.5f * 0.015625f) * 100f * (0.4f + X.Mn(2f, base.NM2D.Cam.getScale(false) * 0.6f)) * 0.015625f;
				if (this.Pr.getAbsorbContainer().isActive())
				{
					num = -X.Abs(num) * 0.9f;
				}
				vector2.y += num;
				vector2.y = X.MMX(-IN.hh * 0.6f * 0.015625f, vector2.y, IN.hh * 0.8f * 0.015625f);
				IN.Pos2(this.Trs, vector2.x + base.NM2D.ui_shift_x * 0.015625f, vector2.y);
				byte b = (((IN.totalframe & 31) <= 15) ? 0 : 1);
				if (this.draw_ni != b)
				{
					this.draw_ni = b;
					this.redrawGachaStick();
					this.MdBg.updateForMeshRenderer(true);
				}
			}
			return true;
		}

		public bool tap_hitted
		{
			get
			{
				return this.t_hit > 0f;
			}
		}

		private void runRebagacha(float TS)
		{
			bool flag = false;
			if (this.Pr.isBP() || this.Pr.isLP(1) || this.Pr.isRP(1) || this.Pr.isTP(1) || this.Pr.isJumpPD(1))
			{
				if (this.freeze_lock_t == 0f)
				{
					flag = true;
				}
				else if (this.freeze_lock_t < 7f)
				{
					this.freeze_lock_t = -X.Abs(this.freeze_lock_t);
				}
			}
			if (this.freeze_lock_t != 0f)
			{
				bool flag2 = this.freeze_lock_t < 0f;
				this.freeze_lock_t = X.VALWALK(this.freeze_lock_t, 0f, TS);
				if (this.freeze_lock_t == 0f && flag2)
				{
					flag = true;
				}
			}
			if (flag)
			{
				this.t_hit = 18f;
				this.Pr.TeCon.setQuake(1f, 7, 0f, 0);
				this.Pr.TeCon.setQuakeSinH(4f, 13, 5.54f, 1f, 0);
				if ((this.actv & M2RebagachaAnnounce.ACTV.FROZEN) != M2RebagachaAnnounce.ACTV.OFFLINE || (this.actv & M2RebagachaAnnounce.ACTV.STONE) != M2RebagachaAnnounce.ACTV.OFFLINE)
				{
					this.freeze_lock_t = 8f;
					int level = this.Pr.Ser.getLevel(SER.FROZEN);
					float num = (((this.actv & M2RebagachaAnnounce.ACTV.FROZEN) != M2RebagachaAnnounce.ACTV.OFFLINE && (this.actv & M2RebagachaAnnounce.ACTV.STONE) != M2RebagachaAnnounce.ACTV.OFFLINE) ? 0.5f : 1f);
					if ((this.actv & M2RebagachaAnnounce.ACTV.FROZEN) != M2RebagachaAnnounce.ACTV.OFFLINE)
					{
						this.Pr.PtcVar("cx", (double)(this.Pr.x + X.XORSPS() * this.Pr.sizex * 0.7f)).PtcVar("cy", (double)(this.Pr.y + X.XORSPS() * this.Pr.sizey * 0.7f)).PtcST("frozen_gacha", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.Pr.Ser.CureTime(SER.FROZEN, (int)(X.NIL(165f, 180f, (float)level, 2f) * num), false);
					}
					if ((this.actv & M2RebagachaAnnounce.ACTV.STONE) != M2RebagachaAnnounce.ACTV.OFFLINE)
					{
						this.Pr.PtcVar("cx", (double)(this.Pr.x + X.XORSPS() * this.Pr.sizex * 0.7f)).PtcVar("cy", (double)(this.Pr.y + X.XORSPS() * this.Pr.sizey * 0.7f)).PtcST("stone_ser_gacha", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
						this.Pr.Ser.CureTime(SER.STONE, (int)(X.NIL(160f, 160f, (float)level, 2f) * num), false);
						return;
					}
				}
				else if ((this.actv & M2RebagachaAnnounce.ACTV.WEB_TRAPPED) != M2RebagachaAnnounce.ACTV.OFFLINE)
				{
					this.freeze_lock_t = 22f;
					int level2 = this.Pr.Ser.getLevel(SER.WEB_TRAPPED);
					this.Pr.Ser.CureTime(SER.WEB_TRAPPED, X.IntC(200f / X.NIL(4f, 10f, (float)level2, 2f)), false);
					this.Pr.PtcVar("cx", (double)(this.Pr.x + X.XORSPS() * this.Pr.sizex * 0.3f)).PtcVar("by", (double)(this.Pr.mbottom - X.XORSP() * 0.2f * this.Pr.sizey)).PtcST("pr_web_trapped_struggle", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				}
			}
		}

		private void redrawBox()
		{
			this.MdBg.clear(false, false);
			this.MdBg.chooseSubMesh(0, false, false);
			MdArranger mdArranger = this.MdBg.chooseSubMeshArranger(0);
			mdArranger.Set(true);
			if (this.t >= 80f)
			{
				float num = this.t - 80f;
				X.ZPOW(num, 15f);
				X.ZSIN(num - 15f, 7.5f);
				X.ZSIN(num - 22.5f, 7.5f);
			}
			else if (this.t < 0f)
			{
				X.ZSIN(30f + this.t, 30f);
			}
			this.MdBg.ColGrd.Black();
			float num2 = 1f;
			if (this.t_hit > 0f)
			{
				float num3 = X.ZPOW(this.t_hit, 18f);
				this.MdBg.ColGrd.blend(4294966715U, 0.5f * num3);
				num2 += num3 * 0.4f;
			}
			this.MdBg.Col = this.MdBg.ColGrd.mulA(0.6f).C;
			this.MdBg.KadoPolyRect(0f, 0f, this.box_w * num2, 60f * num2, 22f * num2, 10, 0f, false, 0f, 0f, false);
			mdArranger.Set(false);
			this.MdBg.chooseSubMesh(1, false, false);
			this.MdBg.chooseSubMeshArranger(1).Set(true);
		}

		private void redrawGachaStick()
		{
			MdArranger mdArranger = this.MdBg.chooseSubMeshArranger(1);
			this.MdBg.chooseSubMesh(1, false, false);
			mdArranger.revertVerAndTriIndexFirstSaved(false);
			this.MdBg.ColGrd.Set(4293321691U);
			float num = 1f;
			if (this.t_hit > 0f)
			{
				float num2 = X.ZPOW(this.t_hit, 18f);
				this.MdBg.ColGrd.blend(4294966715U, num2);
				num += num2 * 0.04f;
			}
			this.MdBg.Col = this.MdBg.ColGrd.C;
			this.MdBg.RotaPF(-this.box_w * 0.5f + 30f + 12f, 0f, num, num, 0f, MTR.SqRebagacha[(int)this.draw_ni], false, false, false, uint.MaxValue, false, 0);
		}

		private bool runner_assigned_;

		private M2RebagachaAnnounce.ACTV actv;

		private float t = -30f;

		public const float T_SHOWDELAY = 80f;

		public const float T_FADE = 30f;

		public const float T_HIT_ANIM = 18f;

		public const float T_HIT_LOCK_FROZEN = 8f;

		public const float T_HIT_LOCK_WEB_TRAPPED = 22f;

		private float freeze_lock_t;

		private float t_hit;

		private bool use_valotile_;

		private byte draw_ni;

		private GameObject Gob;

		private Transform Trs;

		private MeshDrawer MdBg;

		private ValotileRenderer Valot;

		private TextRenderer Tx;

		private const float box_w_default = 260f;

		private const float box_w_default_long = 320f;

		private float box_w = 260f;

		private const float ico_w = 60f;

		private const float box_h = 60f;

		private const float box_radius = 22f;

		private const float box_shift_y = 100f;

		public bool need_reposit;

		private bool need_redraw_box;

		[Flags]
		private enum ACTV : byte
		{
			OFFLINE = 0,
			FROZEN = 1,
			FROZEN_HARD = 2,
			WEB_TRAPPED = 4,
			STONE = 8,
			STONE_HARD = 16,
			_FORCE_SHOW = 22,
			_EVENT_LOCK = 64
		}
	}
}
