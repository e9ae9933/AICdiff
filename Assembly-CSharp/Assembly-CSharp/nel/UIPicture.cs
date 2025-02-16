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
			this.GobMeshHolder = IN.CreateGob(this.Gob, "-NoelChanKawaisou");
			this.MdKarioki = MeshDrawer.prepareMeshRenderer(this.GobMeshHolder, this.MtrPxl, 0f, -1, null, false, false);
			this.Filter = this.GobMeshHolder.GetComponent<MeshFilter>();
			IN.setZ(this.GobMeshHolder.transform, num);
			this.Valot = this.GobMeshHolder.AddComponent<ValotileRendererFilter>();
			this.Valot.Init(this.MdKarioki, this.GobMeshHolder.GetComponent<MeshRenderer>(), true);
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

		public override bool isFacingEnemy()
		{
			return this.CurPr.isFacingEnemy();
		}

		public override void run(float fcnt, bool can_transfer_uipic = true)
		{
			this.CurPr.EggCon.checkDripUi(fcnt);
			this.TeCon.runDrawOrRedrawMesh(X.D_EF, (float)X.AF_EF, UIPicture.tecon_TS);
			base.run(fcnt, this.Base.can_transfer_uipic && can_transfer_uipic);
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
				return this.CurPr.uipicture_TS * base.TS;
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
			base.setFade(text, emstate, immediate, force_change, false);
			return true;
		}

		public override UIPictureBase.EMSTATE_ADD getAdditionalState(bool force_fine = false)
		{
			if (this.pre_sta == UIPictureBase.EMSTATE_ADD._NO_CHECK || force_fine)
			{
				this.pre_sta = UIPictureBase.EMSTATE_ADD.EMPTY;
				if (this.CurPr.isFrozen())
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
						this.pre_sta &= (UIPictureBase.EMSTATE_ADD)(-9);
					}
				}
				else
				{
					this.pre_sta &= (UIPictureBase.EMSTATE_ADD)(-13);
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
			if (!force_cutin_use && !this.Base.event_center_uipic && this.Base.showing_pict && !CFG.sp_juice_cutin && !(base.getCurrentFadeKey(true) == "wetten_osgm"))
			{
				this.TeCon.setQuake(5f, 5, 1f, 0);
				this.TeCon.setQuake(11f, 23, 2f, 25);
				this.TeCon.removeSpecific(TEKIND.BOUNCE_ZOOM_IN);
				this.TeCon.setBounceZoomIn(1.06f, 31f, 90f, -2);
				this.TeSetColorBlink(5f, 22f, 0.25f, (wet_pat == UIPictureBase.EMWET.PEE) ? 12898733 : 14211288, 0);
				base.setFade(text, UIPictureBase.EMSTATE.NORMAL, false, false, false);
				this.Base.PtcVar("mp_ratio", this.CurPr.mp_ratio).PtcVar("pee", (float)((wet_pat == UIPictureBase.EMWET.PEE) ? 1 : 0)).PtcST("ui_dmg_wetten", null);
				return;
			}
			if (this.FD_fnDrawWettenCutin == null)
			{
				this.FD_fnDrawWettenCutin = new AnimateCutin.FnDrawAC(this.fnDrawWettenCutin);
			}
			if (this.EfpCutinPeeSplash == null)
			{
				this.EfpCutinPeeSplash = new EfParticleOnce(null, EFCON_TYPE.UI);
			}
			if (wet_pat == UIPictureBase.EMWET.MILK && this.EfpCutinSplashMilkL == null)
			{
				this.EfpCutinSplashMilkL = new EfParticleOnce("ui_wetten_milk_splash_L", EFCON_TYPE.UI);
				this.EfpCutinSplashMilkR = new EfParticleOnce("ui_wetten_milk_splash_R", EFCON_TYPE.UI);
			}
			this.EfpCutinPeeSplash.key = ((wet_pat == UIPictureBase.EMWET.PEE) ? "ui_wetten_splash_yellow" : "ui_wetten_splash");
			AnimateCutin animateCutin = this.Base.PopPoolCutin("ui_wetten_cutin", this.FD_fnDrawWettenCutin, true);
			this.Base.CutinAssignFader(animateCutin, this.CurPr, text, UIPictureBase.EMSTATE.NORMAL);
			if (UIBase.uipic_mpf >= 0f)
			{
				IN.PosP2(animateCutin.transform, IN.w * (this.CurPr.EpCon.Cutin.isRightCutinActive() ? (-0.2f) : 0.36f), 0f);
				return;
			}
			IN.PosP2(animateCutin.transform, IN.w * (this.CurPr.EpCon.Cutin.isLeftCutinActive() ? 0.2f : (-0.36f)), 0f);
		}

		public override bool faderEnable(UIPictureFader.UIPFader Fader)
		{
			return !Fader.ignore_in_frozen || !this.CurPr.isFrozen();
		}

		public void applyEmotionEffect(MGATTR attr, UIPictureBase.EMSTATE st_add, float blink_alpha = 1f, bool ui_darken = true, bool decline_additional_effect = false, float quake_sinx = 0f, float quake_siny = 0f, bool setfade = false)
		{
			UIPictureBase.EMSTATE emstate = this.checkPlayerState() | st_add;
			string text = "damage";
			this.applyEmotionEffect(attr, ref emstate, ref text, blink_alpha, ui_darken, decline_additional_effect, quake_sinx, quake_siny);
			if (setfade)
			{
				base.setFade(text, emstate, true, false, false);
			}
		}

		protected override void applyEmotionEffect(MGATTR attr, ref UIPictureBase.EMSTATE st, ref string fade_key, float blink_alpha, bool ui_darken, bool decline_additional_effect = false, float quake_sinx = 0f, float quake_siny = 0f)
		{
			float num = 8f;
			float num2 = 10f;
			bool flag = false;
			switch (attr)
			{
			case MGATTR.WIP:
			{
				float num3 = X.NIXP(0.2f, 0.3f) * 3.1415927f * (float)X.MPFXP();
				float num4 = X.XORSP() * 6.2831855f;
				float num5 = X.Cos(num4);
				float num6 = X.Sin(num4);
				num2 = 28f;
				this.Base.PtcVar("hp_ratio", this.CurPr.hp_ratio).PtcVar("x0", num5).PtcVar("y0", num6)
					.PtcVar("rot_agR", num3)
					.PtcST("ui_dmg_wip", null);
				goto IL_08E9;
			}
			case MGATTR.GRAB:
			{
				float num7 = 1.5707964f + X.XORSPS() * 0.08f * 3.1415927f;
				float num8 = X.XORSP() * 6.2831855f;
				float num9 = X.Cos(num8);
				float num10 = X.Sin(num8);
				num2 = 100f;
				this.Base.PtcVar("hp_ratio", this.CurPr.hp_ratio).PtcVar("x0", num9).PtcVar("y0", num10)
					.PtcVar("rot_agR", num7)
					.PtcST("ui_dmg_grab", null);
				goto IL_08E9;
			}
			case MGATTR.CUT_H:
			{
				float num11 = (X.NIXP(0.2f, 0.38f) + 0f) * 3.1415927f * (float)X.MPFXP() + ((X.xors(2) == 0) ? 6.2831855f : (-3.1415927f));
				float num12 = X.XORSP() * 6.2831855f;
				float num13 = X.Cos(num12);
				float num14 = X.Sin(num12);
				num2 = 70f;
				this.Base.PtcVar("hp_ratio", this.CurPr.hp_ratio).PtcVar("x0", num13).PtcVar("y0", num14)
					.PtcVar("rot_agR", num11)
					.PtcST("ui_dmg_cut", null);
				goto IL_08E9;
			}
			case MGATTR.STAB:
			{
				float num15 = X.XORSP() * 6.2831855f;
				float num16 = X.Cos(num15);
				float num17 = X.Sin(num15);
				num2 = 32f;
				this.Base.PtcVar("hp_ratio", this.CurPr.hp_ratio).PtcVar("x0", num16).PtcVar("y0", num17)
					.PtcVar("hp_ratio", this.CurPr.hp_ratio)
					.PtcST("ui_dmg_stab", null);
				goto IL_08E9;
			}
			case MGATTR.BITE:
			case MGATTR.BITE_V:
			{
				float num18 = ((attr == MGATTR.BITE) ? X.NIXP(0.58f, 0.78f) : (1f + X.NIXP(-0.14f, 0.14f))) * 1.5707964f * (float)X.MPFXP();
				this.Base.PtcVar("hp_ratio", this.CurPr.hp_ratio).PtcVar("rot_agR", num18).PtcST("ui_dmg_bite", null);
				goto IL_08E9;
			}
			case MGATTR.PRESS:
			case MGATTR.PRESS_PENETRATE:
				break;
			case MGATTR.BOMB:
			{
				float num19 = X.XORSPS() * 3.1415927f;
				float num20 = X.NIXP(30f, 320f);
				this.Base.PtcVar("cx", num20 * 0.4f * X.Cos(num19)).PtcVar("cy", num20 * X.Sin(num19)).PtcST("ui_dmg_bomb", null);
				goto IL_08E9;
			}
			default:
				switch (attr)
				{
				case MGATTR.FIRE:
					if (this.CurBodyData != null)
					{
						Vector2 position = this.CurBodyData.getPosition(UIPictureBodyData.POS.UNDER);
						if (this.CurPr.getAbsorbContainer().isActive() == X.XORSP() < 0.9f)
						{
							this.Base.PtcVar("cx", position.x).PtcVar("cy", position.y).PtcST("ui_dmg_fire_absorbed", null);
						}
						else if ((st & UIPictureBase.EMSTATE.SMASH) == UIPictureBase.EMSTATE.NORMAL)
						{
							this.Base.PtcVar("cx", position.x).PtcVar("cy", position.y).PtcST("ui_dmg_fire", null);
						}
						else
						{
							this.Base.PtcVar("cx", position.x).PtcVar("cy", position.y).PtcST("ui_dmg_fire_smashed", null);
						}
					}
					num = 0f;
					num2 = 60f;
					goto IL_08E9;
				case MGATTR.ICE:
				{
					float num21 = X.XORSPS() * 3.1415927f;
					float num22 = X.NIXP(30f, 240f);
					this.Base.PtcVar("cx", num22 * 0.4f * X.Cos(num21)).PtcVar("cy", num22 * X.Sin(num21)).PtcST("ui_dmg_ice", null);
					num2 = 100f;
					goto IL_08E9;
				}
				case MGATTR.THUNDER:
					this.Base.PtcVar("hp_ratio", this.CurPr.hp_ratio).PtcST("ui_dmg_thunder", null);
					num2 = 100f;
					this.TeSetDmgBlink(attr, 170f, 1f, 1f, (int)(num2 - 20f));
					goto IL_08E9;
				case MGATTR.POISON:
					this.Base.PtcST("ui_dmg_poison", null);
					num2 = 120f;
					blink_alpha = 0.3f;
					this.TeCon.setQuakeSinH(X.NIXP(6f, 10f), 30, X.NIXP(23f, 45f), 0f, 0);
					goto IL_08E9;
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
						goto IL_08E9;
					case MGATTR.WORM:
						fade_key = "insected";
						if (!decline_additional_effect)
						{
							this.Base.PtcVar("mp_ratio", this.CurPr.mp_ratio).PtcVar("cnt0", 18f).PtcVar("cnt1", 28f)
								.PtcVar("cnt2", 16f)
								.PtcST("ui_dmg_worm", null);
						}
						this.TeCon.setEnlargeAbsorbed(1f + X.Mx(0f, X.NIXP(-0.01f, 0.04f)), 1f + X.Mx(0f, X.NIXP(-0.07f, 0.06f)), X.NIXP(14f, 22f), 0);
						goto IL_08E9;
					case MGATTR.EATEN:
						ui_darken = false;
						blink_alpha = (((this.pre_emstate & UIPictureBase.EMSTATE.ABSORBED) == UIPictureBase.EMSTATE.NORMAL || X.XORSP() < 0.15f) ? 1f : X.NIXP(0.03f, 0.4f));
						st |= UIPictureBase.EMSTATE.ABSORBED | UIPictureBase.EMSTATE.TORNED | UIPictureBase.EMSTATE.DEAD | ((X.xors() % 4U == 0U) ? UIPictureBase.EMSTATE.SMASH : UIPictureBase.EMSTATE.NORMAL) | ((X.xors() % 4U == 0U) ? UIPictureBase.EMSTATE.SHAMED : UIPictureBase.EMSTATE.NORMAL);
						if (!decline_additional_effect)
						{
							this.Base.PtcVar("mp_ratio", this.CurPr.mp_ratio).PtcVar("cnt0", 10f).PtcVar("cnt1", 19f)
								.PtcVar("cnt2", 10f)
								.PtcST("ui_dmg_worm", null);
						}
						this.TeCon.setEnlargeAbsorbed(1f + X.Mx(0f, X.NIXP(-0.01f, 0.04f)), 1f + X.Mx(0f, X.NIXP(-0.07f, 0.06f)), X.NIXP(14f, 22f), 0);
						flag = true;
						goto IL_08E9;
					case MGATTR.MUD:
						this.Base.PtcST("ui_dmg_mud", null);
						goto IL_08E9;
					case MGATTR.ACME:
					{
						float num23 = X.XORSPS() * 3.1415927f;
						float num24 = X.NIXP(30f, 320f);
						this.Base.PtcVar("cx", num24 * 0.4f * X.Cos(num23)).PtcVar("cy", num24 * X.Sin(num23)).PtcST("ui_dmg_acme", null);
						num2 = 120f;
						blink_alpha = 0.7f;
						this.TeCon.setQuakeSinH(X.NIXP(6f, 10f), 30, X.NIXP(23f, 45f), 0f, 0);
						goto IL_08E9;
					}
					}
					break;
				}
				break;
			}
			this.Base.PtcVar("hp_ratio", this.CurPr.hp_ratio).PtcST("ui_dmg_normal", null);
			IL_08E9:
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
					base.setFade("damage_gas_hit", UIPictureBase.EMSTATE.NORMAL, false, false, false);
				}
				this.TeSetColorBlink(2f, 42f, 0.5f, (int)(C32.c2d(Mist.color0) & 16777215U), 0);
				this.CurPr.TeCon.setGasColorBlink(2f, 42f, 0.5f, (int)(C32.c2d(Mist.color0) & 16777215U), 0);
				this.Base.PtcVar("col0", (float)(C32.c2d(Mist.color0) & 16777215U)).PtcST("ui_dmg_gas_applied", null);
			}
			else
			{
				if (this.TeCon.existSpecific(TEKIND.GAS_COLOR_BLINK))
				{
					return false;
				}
				this.TeSetGasColorBlink(20f, 32f, 0.6f, (int)(C32.c2d(Mist.color0) & 16777215U), 0);
				this.CurPr.TeCon.setGasColorBlink(20f, 32f, 0.6f, (int)(C32.c2d(Mist.color0) & 16777215U), 0);
				this.Base.PtcVar("col0", (float)(C32.c2d(Mist.color0) & 16777215U)).PtcST("ui_dmg_gas", null);
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
			Vector2 position = this.CurBodyData.getPosition(UIPictureBodyData.POS.UNDER);
			this.Base.PtcVar("cx", position.x).PtcVar("cy", position.y).PtcST("ui_plant_egg", null);
			this.changeEmotDefault(false, false);
		}

		public void applyUiPressDamage()
		{
			this.PtcST("ui_press_damage", null);
		}

		public TransEffecterItem TeSetDmgBlink(MGATTR attr, float maxt = 0f, float mul_ratio = 1f, float add_ratio = 0f, int _saf = 0)
		{
			maxt *= (float)CFG.sp_dmgte_ui_duration * 0.01f;
			mul_ratio *= (float)CFG.sp_dmgte_ui_density * 0.01f;
			add_ratio *= (float)CFG.sp_dmgte_ui_density * 0.01f;
			return this.TeCon.setDmgBlink(attr, maxt, mul_ratio, add_ratio, _saf);
		}

		public TransEffecterItem TeSetUiDmgDarken(MGATTR attr, float maxt = 0f, float alpha = 1f)
		{
			maxt *= (float)CFG.sp_dmgte_ui_duration * 0.01f;
			alpha *= (float)CFG.sp_dmgte_ui_density * 0.01f;
			return this.TeCon.setUiDmgDarken(attr, maxt, alpha);
		}

		public TransEffecterItem TeSetColorBlink(float fadein_t, float fadeout_t, float alpha, int color_without_alpha, int _saf = 0)
		{
			fadein_t *= (float)CFG.sp_dmgte_ui_duration * 0.01f;
			fadeout_t *= (float)CFG.sp_dmgte_ui_duration * 0.01f;
			alpha = X.MulOrScr(alpha, (float)CFG.sp_dmgte_ui_density * 0.01f);
			return this.TeCon.setColorBlink(fadein_t, fadeout_t, alpha, color_without_alpha, _saf);
		}

		public TransEffecterItem TeSetColorBlinkAdd(float fadein_t, float fadeout_t, float alpha, int color_without_alpha, int _saf = 0)
		{
			fadein_t *= (float)CFG.sp_dmgte_ui_duration * 0.01f;
			fadeout_t *= (float)CFG.sp_dmgte_ui_duration * 0.01f;
			alpha = X.MulOrScr(alpha, (float)CFG.sp_dmgte_ui_density * 0.01f);
			return this.TeCon.setColorBlinkAdd(fadein_t, fadeout_t, alpha, color_without_alpha, _saf);
		}

		public TransEffecterItem TeSetGasColorBlink(float fadein_t, float fadeout_t, float alpha, int color_without_alpha, int _saf = 0)
		{
			fadein_t *= (float)CFG.sp_dmgte_ui_duration * 0.01f;
			fadeout_t *= (float)CFG.sp_dmgte_ui_duration * 0.01f;
			alpha = X.MulOrScr(alpha, (float)CFG.sp_dmgte_ui_density * 0.01f);
			return this.TeCon.setGasColorBlink(fadein_t, fadeout_t, alpha, color_without_alpha, _saf);
		}

		public void applyLayingEgg(bool force = false)
		{
			this.CurPr.BetoMng.Check(BetoInfo.EggLay, false, true);
			this.TeSetColorBlink(5f, 22f, X.NIXP(0.125f, 0.25f), 15103907, 0);
			this.TeCon.setQuake(4f, 9, 2f, 0);
			this.TeCon.setEnlargeAbsorbed(1f + X.Mx(0f, X.NIXP(-0.01f, 0.04f)), 1f + X.Mx(0f, X.NIXP(-0.07f, 0.06f)), X.NIXP(14f, 22f), 0);
			base.recheck_emot = true;
			bool flag = base.setFade("laying_egg", (X.XORSP() < 0.5f) ? UIPictureBase.EMSTATE.SMASH : UIPictureBase.EMSTATE.NORMAL, force, force, false);
			Vector3 vector;
			if (!this.CurBodyData.getEffectReposition("follow_hip", out vector))
			{
				vector = this.CurBodyData.getPosition(UIPictureBodyData.POS.UNDER);
			}
			this.Base.PtcVar("cx", vector.x).PtcVar("cy", vector.y).PtcVar("count", (float)(flag ? 2 : 1))
				.PtcST("ui_laying_egg", null);
		}

		public void applyLayingEggCutin(bool is_liquid, bool is_right = false, bool from_egg_remove = false)
		{
			if (X.sensitive_level > 0)
			{
				return;
			}
			if (this.FD_fnDrawLayEggCutin == null)
			{
				this.FD_fnDrawLayEggCutin = new AnimateCutin.FnDrawAC(this.fnDrawLayEggCutin);
				this.PP_LayEgg = new PopPolyDrawer(4);
				StringHolder stringHolder = new StringHolder("4|-198|-62|-189|75|206|44|193|-94,0|2|4|0,0.7999420166|2.39999771118,78,20,0,1", CsvReader.RegComma);
				this.PP_LayEgg.loadFromSH(stringHolder, 0);
				this.EfpOnceLayEggCutinCircle = new EfParticleOnce("ui_layegg_cutin_circle", EFCON_TYPE.UI);
				this.EfpOnceLayEggCutinDrip = new EfParticleOnce("ui_layegg_cutin_drip", EFCON_TYPE.UI);
			}
			string text = (is_liquid ? "cuts_layegg_liquid" : "cuts_layegg");
			AnimateCutin animateCutin = this.Base.getAnimateCutin("cuts_layegg");
			if (animateCutin != null)
			{
				animateCutin.restart(8f);
			}
			else
			{
				animateCutin = this.Base.PopPoolCutin("cuts_layegg", this.FD_fnDrawLayEggCutin, true);
				animateCutin.TS_spv = X.NIXP(0.6f, 0.875f);
				animateCutin.stencil_ref = 252;
				this.Base.CutinAssignFader(animateCutin, this.CurPr, text, UIPictureBase.EMSTATE.NORMAL);
			}
			if (this.Base.event_center_uipic)
			{
				IN.PosP2Abs(animateCutin.transform, -IN.wh * 0.3f, -IN.hh * 0.68f);
			}
			else
			{
				IN.PosP2Abs(animateCutin.transform, this.CurPr.M2D.ui_shift_x - (IN.w - X.Abs(this.CurPr.M2D.ui_shift_x * 2f)) * UIBase.uipic_mpf * 0.12f, IN.hh * 0.4f);
			}
			animateCutin.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
			SND.Ui.play("splash_egg", false);
			this.CurPr.playVo(from_egg_remove ? "dmgx_eggremove" : "dmgx", false, false);
		}

		public void applyBurned(bool burnemot, bool decline_apply_emot = false)
		{
			if (!burnemot)
			{
				this.applyEmotionEffect(MGATTR.FIRE, UIPictureBase.EMSTATE.SMASH, 1f, true, false, 0f, 0f, !decline_apply_emot);
				return;
			}
			if (this.pre_emot != UIEMOT.BURNED)
			{
				if (!decline_apply_emot)
				{
					base.applyDamage(MGATTR.FIRE, 15f, 0f, UIPictureBase.EMSTATE.NORMAL, false, "burned", false);
				}
				this.TeCon.removeSpecific(TEKIND.DMG_BLINK);
				return;
			}
			this.applyEmotionEffect(MGATTR.FIRE, UIPictureBase.EMSTATE.NORMAL, 1f, true, false, 0f, 0f, !decline_apply_emot);
		}

		public void applyVoreBurned()
		{
			this.TeCon.setQuakeSinV(X.NIXP(45f, 60f), (int)X.NIXP(30f, 60f), X.NIXP(24f, 53f), -1f, 0);
			this.Base.PtcVar("cx", 0f).PtcVar("cy", 0f).PtcST("ui_vore_burned", null);
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
						if (fade_key == "torture_groundbury")
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
				.PtcST("ui_dmg_absorb", null);
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

		private bool fnDrawWettenCutin(AnimateCutin Cti, Map2d Mp, MeshDrawer Md, MeshDrawer MdT, float t, ref bool update_meshdrawer)
		{
			update_meshdrawer = true;
			if (t == 0f)
			{
				Md.setMaterial(MTRX.getMtr(-1), false);
				MdT.setMaterial(MTR.MIiconL.getMtr(BLEND.NORMAL, -1), false);
			}
			bool flag = Cti.fader_key == "wetten_milk";
			Md.clearSimple();
			MdT.clearSimple();
			float num = X.ZSIN(t, 20f);
			float num2 = X.ZLINE(210f - t, 55f);
			if (num2 <= 0f)
			{
				return false;
			}
			if (t < 30f)
			{
				Cti.setMulColor(4278190080U, 0.8f * (1f - num));
			}
			if (num < 1f)
			{
				Cti.setMulColor(uint.MaxValue, 1f - num);
			}
			float num3 = ((t < 46f) ? 0f : (1f - X.ZSIN(t, 46f)));
			if (num2 < 1f)
			{
				Cti.setMulColor(4290690750U, 1f - num2);
				if (Cti.stencil_ref == -1)
				{
					Cti.stencil_ref = 69;
					Md.setMaterial(MTRX.getMtr(BLEND.MASK, Cti.stencil_ref), false);
				}
			}
			float num4 = X.ZSIN2(t, 25f);
			if (t < 35f)
			{
				Cti.setBase((float)(flag ? (-100) : 0), -90f * (1f - num4), 1f);
			}
			num4 *= num2;
			float num5 = -40f - 220f * num4;
			float num6 = 320f * num4;
			float num7 = 30f + 90f * num4;
			int i = 0;
			while (i < 2)
			{
				if (i != 0)
				{
					Md.Identity();
					Md.Col = Md.ColGrd.White().blend(4290690750U, num * (1f - num3)).C;
					goto IL_01E3;
				}
				if (num2 < 1f)
				{
					Md.Col = MTRX.ColTrnsp;
					Md.Scale(1.5f, 1.5f, false);
					goto IL_01E3;
				}
				IL_023D:
				i++;
				continue;
				IL_01E3:
				Md.TriRectBL(0);
				Md.PosD(-num5 - num6, -IN.hh - num7, null).PosD(num5, IN.hh + 10f, null).PosD(num5 + num6, IN.hh + num7, null)
					.PosD(-num5, -IN.hh - 10f, null);
				goto IL_023D;
			}
			Md.Identity();
			MdT.Identity();
			this.EfpCutinPeeSplash.drawTo(MdT, 0f, 0f, 0f, 0, t, 0f);
			if (flag)
			{
				float num8 = t - 15f;
				if (num8 >= 0f)
				{
					Vector3 position = Cti.transform.position;
					position.z = 0f;
					Vector3 vector;
					if (UIPictureBodySpine.getEffectReposition(Cti.getViewer(), Cti.FindBone("follow_s"), out vector))
					{
						vector -= position;
						this.EfpCutinSplashMilkL.drawTo(MdT, vector.x * 64f, vector.y * 64f, vector.z, 26, num8, 0f);
					}
					if (UIPictureBodySpine.getEffectReposition(Cti.getViewer(), Cti.FindBone("follow_d"), out vector))
					{
						vector -= position;
						this.EfpCutinSplashMilkR.drawTo(MdT, vector.x * 64f, vector.y * 64f, vector.z, 26, num8 - 3f, 0f);
					}
				}
			}
			return true;
		}

		private bool fnDrawLayEggCutin(AnimateCutin Cti, Map2d Mp, MeshDrawer Md, MeshDrawer MdT, float t, ref bool update_meshdrawer)
		{
			update_meshdrawer = true;
			if (t == 0f)
			{
				Md.chooseSubMesh(0, false, false);
				Md.setMaterial(MTRX.getMtr(BLEND.MASK, Cti.stencil_ref), false);
				Md.chooseSubMesh(1, false, false);
				Md.setMaterial(MTRX.getMtr(BLEND.NORMAL, Cti.stencil_ref), false);
				MdT.setMaterial(MTRX.MIicon.getMtr(-1), false);
				Cti.getMeshRenderer(MdT).sharedMaterial = MdT.getMaterial();
			}
			float num = X.ZSIN(t, 20f);
			float num2 = X.ZLINE(130f - t, 55f);
			if (num2 <= 0f)
			{
				return false;
			}
			Md.clear(false, false);
			MdT.clear(false, false);
			float num3 = (UIBase.Instance.event_center_uipic ? 1f : UIBase.uipic_mpf);
			Md.Scale(num3, 1f, false);
			Md.chooseSubMesh(0, false, false);
			Md.TriRectBL(0);
			Md.Col = MTRX.ColTrnsp;
			this.PP_LayEgg.t = X.NI(Cti.restarted ? 0.75f : 0.25f, 1f, num) * num2;
			this.PP_LayEgg.drawExtendAreaTo(Md, 0f, 0f);
			Md.chooseSubMesh(1, false, false);
			Md.Col = C32.d2c(4290690750U);
			this.PP_LayEgg.drawTo(Md, 0f, 0f, 0f, num2);
			Vector3 vector;
			if (UIPictureBodySpine.getEffectReposition(Cti.getViewer(), Cti.FindBone("vagina_C2"), out vector))
			{
				Vector3 position = Cti.transform.position;
				position.z = 0f;
				vector -= position;
				this.EfpOnceLayEggCutinCircle.drawTo(MdT, vector.x * 64f, vector.y * 64f, vector.z, 0, t, 0f);
				this.EfpOnceLayEggCutinDrip.drawTo(MdT, vector.x * 64f, vector.y * 64f, vector.z, 0, t, 0f);
				if (Cti.fader_key == "cuts_layegg_liquid" && t >= 5f)
				{
					if (this.EfpOnceLayEggCutinDripLiquid == null)
					{
						this.EfpOnceLayEggCutinDripLiquid = new EfParticleOnce("ui_layegg_cutin_drip_liquid", EFCON_TYPE.UI);
					}
					this.EfpOnceLayEggCutinDripLiquid.drawTo(MdT, vector.x * 64f + 20f * num3, vector.y * 64f, vector.z, 0, t - 5f, 0f);
				}
			}
			return true;
		}

		public void useItem(NelItem Itm, string type = null)
		{
			if (type != null)
			{
				if (type == "food")
				{
					this.Base.PtcST("ui_use_food", null);
					this.TeCon.setColorBlinkAdd(5f, 25f, 0.3f, 16777215, 0);
					return;
				}
				if (type == "inventory_expand")
				{
					this.Base.PtcST("ui_use_item_inventory_expand", null);
					return;
				}
			}
			if (Itm.is_bomb)
			{
				return;
			}
			this.Base.PtcST("ui_use_item", null);
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
			return this.Base.PtcST(ptcst_name, null);
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
			this.Base.PtcST(Key, null);
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
			return this.CurPr == null || this.CurPr.isNormalState();
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
				return ((CFG.sp_uipic_lr == CFG.UIPIC_LR.R) ? 1f : this.Base.gamemenu_slide_z) * (1f - this.Base.gamemenu_bench_slide_z);
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

		private PR CurPr;

		private AnimateCutin.FnDrawAC FD_fnDrawWettenCutin;

		private AnimateCutin.FnDrawAC FD_fnDrawLayEggCutin;

		private EfParticleOnce EfpCutinPeeSplash;

		private EfParticleOnce EfpCutinSplashMilkL;

		private EfParticleOnce EfpCutinSplashMilkR;

		private PopPolyDrawer PP_LayEgg;

		private EfParticleOnce EfpOnceLayEggCutinCircle;

		private EfParticleOnce EfpOnceLayEggCutinDripLiquid;

		private EfParticleOnce EfpOnceLayEggCutinDrip;
	}
}
