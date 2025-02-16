using System;
using System.Collections.Generic;
using Better;
using m2d;
using Spine;
using UnityEngine;
using XX;

namespace nel
{
	public class UIPicture : UIPictureBase
	{
		public UIPicture(UIBase _Base)
			: base(_Base.TrsLeft, null)
		{
			if (UIPicture.Instance == null)
			{
				UIPicture.Instance = this;
			}
			this.Base = _Base;
			float num = (this.Base.get_MMRD().base_z = -0.13000011f);
			this.LockCntDmgAttr = new LockCounter<MGATTR>(10);
			this.GobMeshHolder = IN.CreateGob(this.Gob, "-NoelChanKawaisou");
			this.MdKarioki = MeshDrawer.prepareMeshRenderer(this.GobMeshHolder, this.MtrPxl, 0f, -1, null, false, false);
			this.Filter = this.GobMeshHolder.GetComponent<MeshFilter>();
			IN.setZ(this.GobMeshHolder.transform, num);
			this.Valot = this.GobMeshHolder.AddComponent<ValotileRendererFilter>();
			this.Valot.Init(this.MdKarioki, this.GobMeshHolder.GetComponent<MeshRenderer>(), true);
			this.CutinMng = new UIPictCutin(this);
			this.TeCon = new TransEffecter("UIPIC", null, 64, this.Base.layer, this.Base.layer, EFCON_TYPE.UI);
			this.TeCon.is_player = true;
			this.TeCon.RegisterPos(this.GobMeshHolder).RegisterCol(this, false).RegisterScl(this);
			base.reloadUiScript(false);
		}

		public void gameMenuSlide(bool flag)
		{
		}

		public void changePlayerIn(PR Pr)
		{
			Pr.UP = this;
			this.Base.Status.changePlayerIn(Pr);
			this.LockCntDmgAttr.clear();
			if (Pr == this.CurPr)
			{
				return;
			}
			this.pre_sta = UIPictureBase.EMSTATE_ADD._NO_CHECK;
			this.CurPr = Pr;
			this.TeCon.clear();
			this.can_transfer = true;
			if (this.pre_emot == UIEMOT._OFFLINE)
			{
				this.changeEmotDefault(false, false);
				return;
			}
			base.recheck(0, 0);
		}

		public override void newGame()
		{
			base.newGame();
			this.CutinMng.newGame();
		}

		public override bool isFacingEnemy()
		{
			return this.CurPr.isFacingEnemy();
		}

		public override void run(float fcnt, float fcnt_picture, bool can_transfer_uipic = true)
		{
			this.CurPr.EggCon.checkDripUi(fcnt);
			this.TeCon.runDrawOrRedrawMesh(X.D_EF, (float)X.AF_EF, UIPicture.tecon_TS);
			base.run(fcnt, fcnt_picture, this.Base.can_transfer_uipic && can_transfer_uipic);
		}

		public override void runPost()
		{
			base.runPost();
			if (this.TeTextureUpdating != null)
			{
				if (SpineViewerNel.updateTexture())
				{
					this.TeTextureUpdating.af = 15f;
					this.TeTextureUpdating = null;
					return;
				}
				this.TeTextureUpdating.af = 0f;
			}
		}

		public void activateTextureUpdatingHide(bool reset_body = false)
		{
			if (this.TeTextureUpdating == null)
			{
				this.TeTextureUpdating = this.TeCon.setFadeIn(30f, 0.125f);
				if (reset_body)
				{
					UIPictureBodySpine uipictureBodySpine = this.CurBodyData as UIPictureBodySpine;
					if (uipictureBodySpine != null)
					{
						SpineViewerNel.setNeedUpdate(uipictureBodySpine.getViewer());
					}
				}
			}
			if (this.TeTextureUpdating != null)
			{
				this.TeTextureUpdating.af = 0f;
			}
		}

		public override bool FineBetobeto(BetobetoManager.SvTexture CurSvt)
		{
			bool flag = base.FineBetobeto(CurSvt);
			if (flag)
			{
				this.activateTextureUpdatingHide(false);
			}
			return flag;
		}

		public override float TS
		{
			get
			{
				return base.TS;
			}
		}

		public override float TS_animation
		{
			get
			{
				return base.TS_animation * this.CurPr.uipicture_TS(base.isTortureEmot(this.pre_emot));
			}
		}

		public override UIPictureBase.EMSTATE checkPlayerState()
		{
			return this.CurPr.getEmotState();
		}

		public override bool changeEmotDefault(bool immediate = false, bool force_change = false)
		{
			if (!base.changeEmotDefault(false, false) || this.CurPr == null || UIBase.FlgEmotDefaultLock.isActive())
			{
				return false;
			}
			UIPictureBase.EMSTATE emstate = UIPictureBase.EMSTATE.NORMAL;
			string text;
			if (!this.CurPr.getEmotDefault(out text, ref emstate, ref force_change))
			{
				return false;
			}
			this.setFade(text, emstate, immediate, force_change, false);
			return true;
		}

		public override bool setFade(string fade_key, UIPictureBase.EMSTATE add_state = UIPictureBase.EMSTATE.NORMAL, bool immediate = false, bool force_change = false, bool state_absolute = false)
		{
			if (!force_change && fade_key == this.FDCon.getCurrentFadeKey(true) && this.CurPr.isAnimationFrozen())
			{
				if (!state_absolute)
				{
					state_absolute = true;
					add_state |= this.checkPlayerState();
				}
				if (this.pre_emstate0 == add_state)
				{
					return false;
				}
			}
			return base.setFade(fade_key, add_state, immediate, force_change, state_absolute);
		}

		public override bool fader_restartable
		{
			get
			{
				return !this.CurPr.isAnimationFrozen();
			}
		}

		public override UIPictureBase.EMSTATE_ADD getAdditionalState(bool force_fine = false)
		{
			if (this.pre_sta == UIPictureBase.EMSTATE_ADD._NO_CHECK || force_fine)
			{
				this.pre_sta = UIPictureBase.EMSTATE_ADD.NORMAL;
				if (this.CurPr.isAnimationFrozen() && (this.CurPr.is_alive || !this.CurPr.isStoneSer()))
				{
					this.pre_sta |= UIPictureBase.EMSTATE_ADD.FROZEN;
				}
				if (this.CurPr.BetoMng.isActive())
				{
					byte ui_effect_dirty = CFG.ui_effect_dirty;
				}
				if (X.sensitive_level > 0)
				{
					this.pre_sta |= UIPictureBase.EMSTATE_ADD.SENSITIVE;
					if (X.sensitive_level > 1)
					{
						this.pre_sta |= UIPictureBase.EMSTATE_ADD.SP_SENSITIVE;
					}
					else
					{
						this.pre_sta &= ~UIPictureBase.EMSTATE_ADD.SP_SENSITIVE;
					}
				}
				else
				{
					this.pre_sta &= ~(UIPictureBase.EMSTATE_ADD.SENSITIVE | UIPictureBase.EMSTATE_ADD.SP_SENSITIVE);
				}
			}
			return this.pre_sta;
		}

		public override void applyWetten(UIPictureBase.EMWET wet_pat, bool force_cutin_use = false)
		{
			string text = ((X.sensitive_level <= 1 && wet_pat == UIPictureBase.EMWET.MILK) ? "wetten_milk" : ((X.sensitive_level > 0) ? "damage" : "wetten"));
			if (X.sensitive_level > 0 && wet_pat != UIPictureBase.EMWET.NORMAL && wet_pat != UIPictureBase.EMWET.MILK)
			{
				wet_pat = UIPictureBase.EMWET.NORMAL;
			}
			if (force_cutin_use || this.Base.event_center_uipic || !this.Base.showing_pict || CFGSP.juice_cutin || base.getCurrentFadeKey(true) == "wetten_osgm")
			{
				this.CutinMng.applyWetten(wet_pat, text);
				return;
			}
			this.TeCon.setQuake(5f, 5, 1f, 0);
			this.TeCon.setQuake(11f, 23, 2f, 25);
			this.TeCon.removeSpecific(TEKIND.BOUNCE_ZOOM_IN);
			this.TeCon.setBounceZoomIn(1.06f, 31f, 90f, -2);
			this.TeSetColorBlink(5f, 22f, 0.25f, (wet_pat == UIPictureBase.EMWET.PEE) ? 12898733 : 14211288, 0);
			this.setFade(text, UIPictureBase.EMSTATE.NORMAL, false, false, false);
			this.Base.PtcVar("mp_ratio", this.CurPr.mp_ratio).PtcVar("pee", (float)((wet_pat == UIPictureBase.EMWET.PEE) ? 1 : 0)).PtcST("ui_dmg_wetten", null, PTCThread.StFollow.NO_FOLLOW);
		}

		public override bool faderEnable(UIPictureFader.UIPFader Fader)
		{
			return !Fader.ignore_in_frozen || !this.CurPr.isAnimationFrozen();
		}

		public void applyEmotionEffect(MGATTR attr, UIPictureBase.EMSTATE st_add, float blink_alpha = 1f, bool ui_darken = true, bool decline_additional_effect = false, float quake_sinx = 0f, float quake_siny = 0f, bool setfade = false)
		{
			UIPictureBase.EMSTATE emstate = this.checkPlayerState() | st_add;
			string text = "damage";
			this.applyEmotionEffect(attr, ref emstate, ref text, blink_alpha, ui_darken, decline_additional_effect, quake_sinx, quake_siny);
			if (setfade)
			{
				this.setFade(text, emstate, true, false, false);
			}
		}

		protected override void applyEmotionEffect(MGATTR attr, ref UIPictureBase.EMSTATE st, ref string fade_key, float blink_alpha, bool ui_darken, bool decline_additional_effect = false, float quake_sinx = 0f, float quake_siny = 0f)
		{
			float num = 8f;
			float num2 = 10f;
			bool flag = false;
			switch (attr)
			{
			case MGATTR.SPERMA:
				if (!this.LockCntDmgAttr.isLocked(attr))
				{
					this.LockCntDmgAttr.Add(attr, 20f);
					this.Base.PtcST("ui_dmg_sperma", null, PTCThread.StFollow.NO_FOLLOW);
					goto IL_0B50;
				}
				goto IL_0B50;
			case MGATTR.WIP:
			{
				float num3 = X.NIXP(0.2f, 0.3f) * 3.1415927f * (float)X.MPFXP();
				float num4 = X.XORSP() * 6.2831855f;
				float num5 = X.Cos(num4);
				float num6 = X.Sin(num4);
				num2 = 28f;
				if (!this.LockCntDmgAttr.isLocked(attr))
				{
					this.LockCntDmgAttr.Add(attr, 20f);
					this.Base.PtcVar("hp_ratio", this.CurPr.hp_ratio).PtcVar("x0", num5).PtcVar("y0", num6)
						.PtcVar("rot_agR", num3)
						.PtcST("ui_dmg_wip", null, PTCThread.StFollow.NO_FOLLOW);
					goto IL_0B50;
				}
				goto IL_0B50;
			}
			case MGATTR.GRAB:
			{
				float num7 = 1.5707964f + X.XORSPS() * 0.08f * 3.1415927f;
				float num8 = X.XORSP() * 6.2831855f;
				float num9 = X.Cos(num8);
				float num10 = X.Sin(num8);
				num2 = 100f;
				if (!this.LockCntDmgAttr.isLocked(attr))
				{
					this.LockCntDmgAttr.Add(attr, 20f);
					this.Base.PtcVar("hp_ratio", this.CurPr.hp_ratio).PtcVar("x0", num9).PtcVar("y0", num10)
						.PtcVar("rot_agR", num7)
						.PtcST("ui_dmg_grab", null, PTCThread.StFollow.NO_FOLLOW);
					goto IL_0B50;
				}
				goto IL_0B50;
			}
			case MGATTR.CUT_H:
			{
				num2 = 70f;
				float num11 = (X.NIXP(0.2f, 0.38f) + 0f) * 3.1415927f * (float)X.MPFXP() + ((X.xors(2) == 0) ? 6.2831855f : (-3.1415927f));
				float num12 = X.XORSP() * 6.2831855f;
				float num13 = X.Cos(num12);
				float num14 = X.Sin(num12);
				if (!this.LockCntDmgAttr.isLocked(attr))
				{
					this.LockCntDmgAttr.Add(attr, 20f);
					this.Base.PtcVar("hp_ratio", this.CurPr.hp_ratio).PtcVar("x0", num13).PtcVar("y0", num14)
						.PtcVar("rot_agR", num11)
						.PtcST("ui_dmg_cut", null, PTCThread.StFollow.NO_FOLLOW);
					goto IL_0B50;
				}
				goto IL_0B50;
			}
			case MGATTR.STAB:
			{
				float num15 = X.XORSP() * 6.2831855f;
				float num16 = X.Cos(num15);
				float num17 = X.Sin(num15);
				num2 = 32f;
				if (!this.LockCntDmgAttr.isLocked(attr))
				{
					this.LockCntDmgAttr.Add(attr, 20f);
					this.Base.PtcVar("hp_ratio", this.CurPr.hp_ratio).PtcVar("x0", num16).PtcVar("y0", num17)
						.PtcVar("hp_ratio", this.CurPr.hp_ratio)
						.PtcST("ui_dmg_stab", null, PTCThread.StFollow.NO_FOLLOW);
					goto IL_0B50;
				}
				goto IL_0B50;
			}
			case MGATTR.BITE:
			case MGATTR.BITE_V:
				if (!this.LockCntDmgAttr.isLocked(attr))
				{
					this.LockCntDmgAttr.Add(attr, 20f);
					float num18 = ((attr == MGATTR.BITE) ? X.NIXP(0.58f, 0.78f) : (1f + X.NIXP(-0.14f, 0.14f))) * 1.5707964f * (float)X.MPFXP();
					this.Base.PtcVar("hp_ratio", this.CurPr.hp_ratio).PtcVar("rot_agR", num18).PtcST("ui_dmg_bite", null, PTCThread.StFollow.NO_FOLLOW);
					goto IL_0B50;
				}
				goto IL_0B50;
			case MGATTR.PRESS:
			case MGATTR.PRESS_PENETRATE:
				break;
			case MGATTR.BOMB:
				if (!this.LockCntDmgAttr.isLocked(attr))
				{
					this.LockCntDmgAttr.Add(attr, 25f);
					float num19 = X.XORSPS() * 3.1415927f;
					float num20 = X.NIXP(30f, 320f);
					this.Base.PtcVar("cx", num20 * 0.4f * X.Cos(num19)).PtcVar("cy", num20 * X.Sin(num19)).PtcST("ui_dmg_bomb", null, PTCThread.StFollow.NO_FOLLOW);
					goto IL_0B50;
				}
				goto IL_0B50;
			default:
				switch (attr)
				{
				case MGATTR.FIRE:
					if (this.CurBodyData != null && !this.LockCntDmgAttr.isLocked(attr))
					{
						this.LockCntDmgAttr.Add(attr, 20f);
						Vector2 position = this.CurBodyData.getPosition(UIPictureBodyData.POS.UNDER);
						if (this.CurPr.getAbsorbContainer().isActive() == X.XORSP() < 0.9f)
						{
							this.Base.PtcVar("cx", position.x).PtcVar("cy", position.y).PtcST("ui_dmg_fire_absorbed", null, PTCThread.StFollow.NO_FOLLOW);
						}
						else if ((st & UIPictureBase.EMSTATE.SMASH) == UIPictureBase.EMSTATE.NORMAL)
						{
							this.Base.PtcVar("cx", position.x).PtcVar("cy", position.y).PtcST("ui_dmg_fire", null, PTCThread.StFollow.NO_FOLLOW);
						}
						else
						{
							this.Base.PtcVar("cx", position.x).PtcVar("cy", position.y).PtcST("ui_dmg_fire_smashed", null, PTCThread.StFollow.NO_FOLLOW);
						}
					}
					num = 0f;
					num2 = 60f;
					goto IL_0B50;
				case MGATTR.ICE:
					if (!this.LockCntDmgAttr.isLocked(attr))
					{
						this.LockCntDmgAttr.Add(attr, 20f);
						float num21 = X.XORSPS() * 3.1415927f;
						float num22 = X.NIXP(30f, 240f);
						this.Base.PtcVar("cx", num22 * 0.4f * X.Cos(num21)).PtcVar("cy", num22 * X.Sin(num21)).PtcST("ui_dmg_ice", null, PTCThread.StFollow.NO_FOLLOW);
					}
					num2 = 100f;
					goto IL_0B50;
				case MGATTR.THUNDER:
					if (!this.LockCntDmgAttr.isLocked(attr))
					{
						this.LockCntDmgAttr.Add(attr, 40f);
						this.Base.PtcVar("hp_ratio", this.CurPr.hp_ratio).PtcST("ui_dmg_thunder", null, PTCThread.StFollow.NO_FOLLOW);
					}
					this.TeSetDmgBlink(attr, 170f, 1f, 1f, (int)(num2 - 20f));
					num2 = 100f;
					goto IL_0B50;
				case MGATTR.POISON:
					num2 = 120f;
					blink_alpha = 0.3f;
					if (!this.LockCntDmgAttr.isLocked(attr))
					{
						this.LockCntDmgAttr.Add(attr, 20f);
						this.Base.PtcST("ui_dmg_poison", null, PTCThread.StFollow.NO_FOLLOW);
						this.TeCon.setQuakeSinH(X.NIXP(6f, 10f), 30, X.NIXP(23f, 45f), 0f, 0);
						goto IL_0B50;
					}
					goto IL_0B50;
				default:
					switch (attr)
					{
					case MGATTR.ABSORB:
					case MGATTR.ABSORB_V:
						st |= UIPictureBase.EMSTATE.ABSORBED;
						ui_darken = false;
						blink_alpha = (((this.pre_emstate & UIPictureBase.EMSTATE.ABSORBED) == UIPictureBase.EMSTATE.NORMAL || X.XORSP() < 0.15f) ? 1f : X.NIXP(0.03f, 0.4f));
						flag = true;
						this.TeCon.setEnlargeAbsorbed(1f + X.Mx(0f, X.NIXP(-0.01f, 0.04f)), 1f + X.Mx(0f, X.NIXP(-0.07f, 0.06f)), X.NIXP(14f, 22f), 0);
						goto IL_0B50;
					case MGATTR.WORM:
						fade_key = "insected";
						if (!decline_additional_effect)
						{
							this.Base.PtcVar("mp_ratio", this.CurPr.mp_ratio).PtcVar("cnt0", 18f).PtcVar("cnt1", 28f)
								.PtcVar("cnt2", 16f)
								.PtcST("ui_dmg_worm", null, PTCThread.StFollow.NO_FOLLOW);
						}
						this.TeCon.setEnlargeAbsorbed(1f + X.Mx(0f, X.NIXP(-0.01f, 0.04f)), 1f + X.Mx(0f, X.NIXP(-0.07f, 0.06f)), X.NIXP(14f, 22f), 0);
						goto IL_0B50;
					case MGATTR.EATEN:
						ui_darken = false;
						flag = true;
						blink_alpha = (((this.pre_emstate & UIPictureBase.EMSTATE.ABSORBED) == UIPictureBase.EMSTATE.NORMAL || X.XORSP() < 0.15f) ? 1f : X.NIXP(0.03f, 0.4f));
						st |= UIPictureBase.EMSTATE.ABSORBED | UIPictureBase.EMSTATE.TORNED | UIPictureBase.EMSTATE.DEAD | ((X.xors() % 4U == 0U) ? UIPictureBase.EMSTATE.SMASH : UIPictureBase.EMSTATE.NORMAL) | ((X.xors() % 4U == 0U) ? UIPictureBase.EMSTATE.SHAMED : UIPictureBase.EMSTATE.NORMAL);
						if (!this.LockCntDmgAttr.isLocked(attr))
						{
							this.LockCntDmgAttr.Add(attr, 20f);
							if (!decline_additional_effect)
							{
								this.Base.PtcVar("mp_ratio", this.CurPr.mp_ratio).PtcVar("cnt0", 10f).PtcVar("cnt1", 19f)
									.PtcVar("cnt2", 10f)
									.PtcST("ui_dmg_worm", null, PTCThread.StFollow.NO_FOLLOW);
							}
						}
						this.TeCon.setEnlargeAbsorbed(1f + X.Mx(0f, X.NIXP(-0.01f, 0.04f)), 1f + X.Mx(0f, X.NIXP(-0.07f, 0.06f)), X.NIXP(14f, 22f), 0);
						goto IL_0B50;
					case MGATTR.MUD:
						if (!this.LockCntDmgAttr.isLocked(attr))
						{
							this.LockCntDmgAttr.Add(attr, 20f);
							this.Base.PtcST("ui_dmg_mud", null, PTCThread.StFollow.NO_FOLLOW);
							goto IL_0B50;
						}
						goto IL_0B50;
					case MGATTR.ACME:
						if (!this.LockCntDmgAttr.isLocked(attr))
						{
							this.LockCntDmgAttr.Add(attr, 20f);
							float num23 = X.XORSPS() * 3.1415927f;
							float num24 = X.NIXP(30f, 320f);
							this.Base.PtcVar("cx", num24 * 0.4f * X.Cos(num23)).PtcVar("cy", num24 * X.Sin(num23)).PtcST("ui_dmg_acme", null, PTCThread.StFollow.NO_FOLLOW);
							this.TeCon.setQuakeSinH(X.NIXP(6f, 10f), 30, X.NIXP(23f, 45f), 0f, 0);
						}
						num2 = 120f;
						blink_alpha = 0.7f;
						goto IL_0B50;
					case MGATTR.STONE:
						if (!this.LockCntDmgAttr.isLocked(attr))
						{
							this.LockCntDmgAttr.Add(attr, 120f);
							this.Base.PtcST("ui_dmg_stone", null, PTCThread.StFollow.NO_FOLLOW);
							goto IL_0B50;
						}
						goto IL_0B50;
					}
					break;
				}
				break;
			}
			if (!this.LockCntDmgAttr.isLocked(attr))
			{
				this.LockCntDmgAttr.Add(attr, 15f);
				this.Base.PtcVar("hp_ratio", this.CurPr.hp_ratio).PtcST("ui_dmg_normal", null, PTCThread.StFollow.NO_FOLLOW);
			}
			IL_0B50:
			if (ui_darken)
			{
				this.TeSetUiDmgDarken(attr, 30f, blink_alpha);
			}
			if (flag && !decline_additional_effect)
			{
				this.initAbsorbEffect(fade_key, attr);
			}
			this.TeSetDmgBlink(attr, num2, blink_alpha, 0f, 0);
			if (num > 0f)
			{
				this.TeCon.setFadeOut_in(num, 0.5f, 0);
			}
			this.TeCon.setQuake(4f, 9, 2f, 0);
			if (quake_sinx > 0f)
			{
				this.TeCon.setQuakeSinH(quake_sinx, 38, 22f, 0f, 0);
			}
			if (quake_siny > 0f)
			{
				this.TeCon.setQuakeSinV(quake_siny, 31, 19f, 0f, 0);
			}
		}

		protected override void changeEmotFinalize(Material _Mtr, UIEMOT cemot, UIPictureBase.EMSTATE st, UIPictureFader.UIP_RES res)
		{
			float scale = this.CurBodyData.scale;
			this.Base.get_MMRD().setMaterial(this.MdKarioki, _Mtr, false);
			this.Base.fineEffectCenterPos();
			if (SpineViewerNel.need_update_texture)
			{
				this.activateTextureUpdatingHide(false);
			}
			this.TeCon.removeSpecific(TEKIND.COLOR_FADEOUT);
			if ((res & (UIPictureFader.UIP_RES.DROPPED | UIPictureFader.UIP_RES.STANDUP | UIPictureFader.UIP_RES.IMMEDIATE)) == (UIPictureFader.UIP_RES)0)
			{
				this.TeCon.setColorFadeOut(18f, 6908265U, 1f);
			}
			if (cemot != UIEMOT.WETTEN && (st & UIPictureBase.EMSTATE.ABSORBED) != UIPictureBase.EMSTATE.NORMAL && ((this.pre_emstate & UIPictureBase.EMSTATE.ABSORBED) == UIPictureBase.EMSTATE.NORMAL || X.XORSP() < 0.12f))
			{
				this.TeCon.removeSpecific(TEKIND.BOUNCE_ZOOM_IN);
				this.TeCon.setBounceZoomIn(1.04f, 15f, 33f, -4);
			}
		}

		protected override void closeEmot(UIPictureBodyData Next)
		{
			if (this.CurBodyData != null)
			{
				base.closeEmot(Next);
				if (this.TeTextureUpdating != null && !SpineViewerNel.need_update_texture)
				{
					this.TeTextureUpdating.destruct();
					this.TeTextureUpdating = null;
				}
			}
		}

		public bool applyGasDamage(MistManager.MistKind Mist, bool damage_applied)
		{
			if (damage_applied)
			{
				this.TeCon.setQuake(5f, 5, 1f, 0);
				if (base.isTortureEmot(this.pre_emot) == 0 && !this.CurPr.isSleepingDownState())
				{
					this.TeCon.removeSpecific(TEKIND.BOUNCE_ZOOM_IN);
					this.setFade("damage_gas_hit", UIPictureBase.EMSTATE.NORMAL, false, false, false);
				}
				this.TeSetColorBlink(2f, 42f, 0.5f, (int)(C32.c2d(Mist.color0) & 16777215U), 0);
				this.CurPr.TeCon.setGasColorBlink(2f, 42f, 0.5f, (int)(C32.c2d(Mist.color0) & 16777215U), 0);
				this.Base.PtcVar("col0", (float)(C32.c2d(Mist.color0) & 16777215U)).PtcST("ui_dmg_gas_applied", null, PTCThread.StFollow.NO_FOLLOW);
			}
			else
			{
				if (this.TeCon.existSpecific(TEKIND.GAS_COLOR_BLINK))
				{
					return false;
				}
				this.TeSetGasColorBlink(20f, 32f, 0.6f, (int)(C32.c2d(Mist.color0) & 16777215U), 0);
				this.CurPr.TeCon.setGasColorBlink(20f, 32f, 0.6f, (int)(C32.c2d(Mist.color0) & 16777215U), 0);
				this.Base.PtcVar("col0", (float)(C32.c2d(Mist.color0) & 16777215U)).PtcST("ui_dmg_gas", null, PTCThread.StFollow.NO_FOLLOW);
			}
			return true;
		}

		public void applyDrip(bool with_speed)
		{
			if (this.CurBodyData == null)
			{
				return;
			}
			Vector3 vector;
			if (this.CurBodyData.getEffectReposition("follow_hip", out vector))
			{
				Vector2 centerUPos = this.getCenterUPos(true);
				this.Base.PtcVar("x", (vector.x - centerUPos.x) * 64f);
				this.Base.PtcVar("y", (vector.y - centerUPos.y) * 64f);
				this.Base.PtcVar("agR", vector.z);
				this.Base.PtcVar("aiming", (float)(this.CurBodyData.drip_aiming ? 1 : 0));
				this.Base.PtcVar("drop", (float)((this.FDCon.Cur != null && this.FDCon.Cur.drop) ? 1 : 0));
				this.PtcST("ui_noel_drip", null);
			}
		}

		public void applyPlantedEgg()
		{
			this.CurBodyData.getPosition(UIPictureBodyData.POS.UNDER);
			this.Base.PtcST("ui_plant_egg", null, PTCThread.StFollow.FOLLOW_HIP);
			this.changeEmotDefault(false, false);
		}

		public void applyUiPressDamage()
		{
			this.PtcST("ui_press_damage", null);
		}

		public TransEffecterItem TeSetDmgBlink(MGATTR attr, float maxt = 0f, float mul_ratio = 1f, float add_ratio = 0f, int _saf = 0)
		{
			maxt *= (float)CFGSP.dmgte_ui_duration * 0.01f;
			mul_ratio *= (float)CFGSP.dmgte_ui_density * 0.01f;
			add_ratio *= (float)CFGSP.dmgte_ui_density * 0.01f;
			return this.TeCon.setDmgBlink(attr, maxt, mul_ratio, add_ratio, _saf);
		}

		public TransEffecterItem TeSetUiDmgDarken(MGATTR attr, float maxt = 0f, float alpha = 1f)
		{
			maxt *= (float)CFGSP.dmgte_ui_duration * 0.01f;
			alpha *= (float)CFGSP.dmgte_ui_density * 0.01f;
			return this.TeCon.setUiDmgDarken(attr, maxt, alpha);
		}

		public TransEffecterItem TeSetColorBlink(float fadein_t, float fadeout_t, float alpha, int color_without_alpha, int _saf = 0)
		{
			fadein_t *= (float)CFGSP.dmgte_ui_duration * 0.01f;
			fadeout_t *= (float)CFGSP.dmgte_ui_duration * 0.01f;
			alpha = X.MulOrScr(alpha, (float)CFGSP.dmgte_ui_density * 0.01f);
			return this.TeCon.setColorBlink(fadein_t, fadeout_t, alpha, color_without_alpha, _saf);
		}

		public TransEffecterItem TeSetColorBlinkAdd(float fadein_t, float fadeout_t, float alpha, int color_without_alpha, int _saf = 0)
		{
			fadein_t *= (float)CFGSP.dmgte_ui_duration * 0.01f;
			fadeout_t *= (float)CFGSP.dmgte_ui_duration * 0.01f;
			alpha = X.MulOrScr(alpha, (float)CFGSP.dmgte_ui_density * 0.01f);
			return this.TeCon.setColorBlinkAdd(fadein_t, fadeout_t, alpha, color_without_alpha, _saf);
		}

		public TransEffecterItem TeSetGasColorBlink(float fadein_t, float fadeout_t, float alpha, int color_without_alpha, int _saf = 0)
		{
			fadein_t *= (float)CFGSP.dmgte_ui_duration * 0.01f;
			fadeout_t *= (float)CFGSP.dmgte_ui_duration * 0.01f;
			alpha = X.MulOrScr(alpha, (float)CFGSP.dmgte_ui_density * 0.01f);
			return this.TeCon.setGasColorBlink(fadein_t, fadeout_t, alpha, color_without_alpha, _saf);
		}

		public void applyLayingEgg(bool force = false)
		{
			this.CurPr.BetoMng.Check(BetoInfo.EggLay, false, true);
			this.TeSetColorBlink(5f, 22f, X.NIXP(0.125f, 0.25f), 15103907, 0);
			this.TeCon.setQuake(4f, 9, 2f, 0);
			this.TeCon.setEnlargeAbsorbed(1f + X.Mx(0f, X.NIXP(-0.01f, 0.04f)), 1f + X.Mx(0f, X.NIXP(-0.07f, 0.06f)), X.NIXP(14f, 22f), 0);
			base.recheck_emot = true;
			bool flag = this.setFade("laying_egg", (X.XORSP() < 0.5f) ? UIPictureBase.EMSTATE.SMASH : UIPictureBase.EMSTATE.NORMAL, force, force, false);
			Vector3 vector;
			if (!this.CurBodyData.getEffectReposition("follow_hip", out vector))
			{
				vector = this.CurBodyData.getPosition(UIPictureBodyData.POS.UNDER);
			}
			this.Base.PtcVar("cx", vector.x).PtcVar("cy", vector.y).PtcVar("count", (float)(flag ? 2 : 1));
			this.Base.PtcST("ui_laying_egg", null, PTCThread.StFollow.FOLLOW_HIP);
		}

		public void applyBurned(bool burnemot, bool decline_apply_emot = false)
		{
			if (burnemot && this.pre_emot != UIEMOT.BURNED)
			{
				if (!decline_apply_emot)
				{
					base.applyDamage(MGATTR.FIRE, 15f, 0f, UIPictureBase.EMSTATE.NORMAL, false, "burned", false);
				}
				this.TeCon.removeSpecific(TEKIND.DMG_BLINK);
				return;
			}
			this.applyEmotionEffect(MGATTR.FIRE, UIPictureBase.EMSTATE.SMASH, 1f, true, false, 0f, 0f, !decline_apply_emot);
		}

		public void applyVoreBurned()
		{
			this.TeCon.setQuakeSinV(X.NIXP(45f, 60f), (int)X.NIXP(30f, 60f), X.NIXP(24f, 53f), -1f, 0);
			this.Base.PtcVar("cx", 0f).PtcVar("cy", 0f).PtcST("ui_vore_burned", null, PTCThread.StFollow.NO_FOLLOW);
		}

		public void applyYdrgDamage()
		{
			this.applyEmotionEffect(MGATTR.POISON, UIPictureBase.EMSTATE.NORMAL, 1f, true, false, 0f, 0f, false);
		}

		private void initAbsorbEffect(string fade_key, MGATTR attr)
		{
			float num = X.NIXP(-1f, 1f) * 70f;
			float num2 = ((attr == MGATTR.ABSORB_V) ? X.NIXP(-190f, -160f) : X.NIXP(-200f, 60f));
			if (fade_key != null)
			{
				if (!(fade_key == "shrimp"))
				{
					if (!(fade_key == "torture_drilln_2") && !(fade_key == "torture_romero"))
					{
						if (fade_key == "torture_groundbury" || fade_key == "torture_backinj")
						{
							num2 = -num2;
						}
					}
					else
					{
						num2 = -num2 - 50f;
					}
				}
				else
				{
					num2 = -num2 + 50f;
				}
			}
			this.Base.PtcVar("mp_ratio", this.CurPr.mp_ratio).PtcVar("x0", num).PtcVar("y0", num2)
				.PtcST("ui_dmg_absorb", null, PTCThread.StFollow.NO_FOLLOW);
		}

		public void applyMasturbate(float level, float fadein_time, float fadeout_time)
		{
			this.TeCon.setQuake(4f, 9, 1f, 0);
			this.TeSetUiDmgDarken(MGATTR.ACME, fadein_time + fadeout_time, level);
		}

		public void killPtc(string k)
		{
			this.Base.getEffect().killPtc(k, null);
		}

		public void initSyncCutin(Vector3 _PosPx, Vector3 _BaseSlidePx, int _appear_dir_aim = -1, float _appear_len = 0f)
		{
			if (this.SyncCutin == null)
			{
				this.SyncCutin = IN.CreateGobGUI(this.Base.gameObject, "-SyncCutin").AddComponent<AnimateSyncCutin>();
				IN.setZAbs(this.SyncCutin.transform, -4.15f);
			}
			this.SyncCutin.Init(this.CurPr, this, _PosPx, _BaseSlidePx, _appear_dir_aim, _appear_len);
		}

		public void quitSyncCutin()
		{
			if (this.SyncCutin != null && this.SyncCutin.isActive())
			{
				this.SyncCutin.deactivate(false);
				this.PosSyncSlide = new Vector3(0f, 0f, 1f);
			}
		}

		protected override void redrawMainMesh(UIEMOT v, UIPictureBase.EMSTATE st, int first = 0)
		{
			base.redrawMainMesh(v, st, first);
			if (this.SyncCutin != null)
			{
				this.SyncCutin.Sync();
			}
		}

		public void useItem(NelItem Itm, string type = null)
		{
			if (type != null)
			{
				if (type == "food")
				{
					this.Base.PtcST("ui_use_food", null, PTCThread.StFollow.NO_FOLLOW);
					this.TeCon.setColorBlinkAdd(5f, 25f, 0.3f, 16777215, 0);
					return;
				}
				if (type == "inventory_expand")
				{
					this.Base.PtcST("ui_use_item_inventory_expand", null, PTCThread.StFollow.NO_FOLLOW);
					return;
				}
			}
			if (Itm.is_bomb)
			{
				return;
			}
			this.Base.PtcST("ui_use_item", null, PTCThread.StFollow.NO_FOLLOW);
			base.fineEmState();
			base.force_recheck_allocate = true;
			this.TeCon.setColorBlink(6f, 14f, 0.7f, 12648422, 0);
		}

		public UIPicture PtcVar(string key, float v)
		{
			this.Base.PtcVar(key, v);
			return this;
		}

		public UIPicture PtcVarS(string key, string v)
		{
			this.Base.PtcVarS(key, v);
			return this;
		}

		public UIPicture PtcVarS(string key, MGATTR v)
		{
			return this.PtcVarS(key, FEnum<MGATTR>.ToStr(v));
		}

		public PTCThread PtcST(string ptcst_name, IEfPInteractale Listener = null)
		{
			return this.Base.PtcST(ptcst_name, null, PTCThread.StFollow.NO_FOLLOW);
		}

		public override bool SpineListenerEvent(UIPictureBodySpine _Body, TrackEntry Entry, Event e)
		{
			if (base.SpineListenerEvent(_Body, Entry, e))
			{
				return true;
			}
			if (TX.isStart(e.Data.Name, "VO_", 0))
			{
				StringKey stringKey;
				if (!UIPictureBase.OSpineEventPtcKey.TryGetValue(e.Data.Name, out stringKey))
				{
					Dictionary<string, StringKey> ospineEventPtcKey = UIPictureBase.OSpineEventPtcKey;
					string name = e.Data.Name;
					StringKey stringKey2 = new StringKey(TX.slice(e.Data.Name, 3));
					ospineEventPtcKey[name] = stringKey2;
					stringKey = stringKey2;
				}
				this.CurPr.playVo(stringKey, false, false);
				return true;
			}
			if (e.Data.Name == "VO" && TX.valid(e.String))
			{
				this.CurPr.playVo(e.String, false, false);
				return true;
			}
			if (e.Data.Name == "PTC" && TX.valid(e.String))
			{
				this.PtcVar("_vi", (float)e.Int);
				this.PtcVar("_vf", e.Float);
				this.PtcST(e.String, null);
				return true;
			}
			return false;
		}

		protected override void setSpinePtcSetter(StringKey Key, int vi, float vf)
		{
			this.PtcVar("_vi", (float)vi).PtcVar("_vf", vf);
			this.Base.PtcST(Key, null, PTCThread.StFollow.NO_FOLLOW);
		}

		public bool isActive()
		{
			return true;
		}

		public override void fineMeshTarget(Mesh Ms)
		{
			this.Filter.sharedMesh = Ms;
			this.useValotileFilterMode(false);
		}

		public override ValotileRenderer useValotileFilterMode(bool flag)
		{
			if (this.Valot != null)
			{
				this.Valot.changerFilterMode(flag ? this.Filter : null);
			}
			return this.Valot;
		}

		public override bool isPlayerStateNormal()
		{
			return this.CurPr == null || this.CurPr.isMagicExistState() || this.CurPr.isPunchState() || this.CurPr.isDownState();
		}

		public static void changePlayer(PR Pr)
		{
			UIPicture.Instance.changePlayerIn(Pr);
		}

		public static void Recheck(int delay)
		{
			if (UIPicture.Instance != null)
			{
				UIPicture.Instance.recheck(delay, 0);
			}
		}

		public static PR getPr()
		{
			if (UIPicture.Instance == null)
			{
				return null;
			}
			return UIPicture.Instance.CurPr;
		}

		public static bool isPr(PR Pr)
		{
			return UIPicture.Instance != null && UIPicture.Instance.CurPr == Pr;
		}

		public static bool isPr(BetobetoManager BetoMng)
		{
			return UIPicture.Instance != null && UIPicture.Instance.CurPr.BetoMng == BetoMng;
		}

		public override IEfPtcSetable getEffectTarget()
		{
			return this.Base;
		}

		public override Vector2 getCenterUPos(bool consider_effect_shift = false)
		{
			return consider_effect_shift ? this.Base.getEffectCenterPos() : this.Base.PosPict;
		}

		public override float is_position_right
		{
			get
			{
				return ((CFGSP.uipic_lr == CFGSP.UIPIC_LR.R) ? 1f : this.Base.gamemenu_slide_z) * (1f - this.Base.gamemenu_bench_slide_z);
			}
		}

		public float alpha
		{
			get
			{
				return this.TeCon.base_alpha;
			}
			set
			{
				this.TeCon.base_alpha = value;
			}
		}

		public bool use_valotile
		{
			get
			{
				return this.Valot.enabled;
			}
			set
			{
				this.Valot.enabled = value;
			}
		}

		public override void destruct()
		{
			base.destruct();
			if (UIPicture.Instance == this)
			{
				UIPicture.Instance = null;
			}
			if (this.MdKarioki != null)
			{
				this.MdKarioki.destruct();
			}
		}

		public readonly UIBase Base;

		public static UIPicture Instance;

		public readonly TransEffecter TeCon;

		public static float tecon_TS = 1f;

		private TransEffecterItem TeTextureUpdating;

		private ValotileRendererFilter Valot;

		private MeshFilter Filter;

		private AnimateSyncCutin SyncCutin;

		public readonly UIPictCutin CutinMng;

		public readonly LockCounter<MGATTR> LockCntDmgAttr;

		private PR CurPr;
	}
}
