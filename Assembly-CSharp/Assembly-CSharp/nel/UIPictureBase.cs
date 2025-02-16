using System;
using System.Collections.Generic;
using Better;
using PixelLiner;
using Spine;
using UnityEngine;
using XX;

namespace nel
{
	public class UIPictureBase : ITeColor, ITeScaler
	{
		public static int getFamilyLower(UIPictureBase.EMSTATE t)
		{
			int num = (int)t;
			int num2 = UIPictureBase.Afamily.Length;
			int num3 = X.beki_cnt(4194304U);
			for (int i = 0; i < num2; i++)
			{
				UIPictureBase.EMSTATE emstate = UIPictureBase.Afamily[i];
				if ((emstate & t) != UIPictureBase.EMSTATE.NORMAL)
				{
					int num4 = 0;
					while (num4 <= num3 && 1 << num4 <= (int)t)
					{
						if ((emstate & (UIPictureBase.EMSTATE)(1 << num4)) != UIPictureBase.EMSTATE.NORMAL)
						{
							num |= 1 << num4;
						}
						num4++;
					}
				}
			}
			return num;
		}

		public UIPictureBase(Transform TrsParent, Func<UIPictureBase, Material, MeshDrawer> FnMdCreate)
		{
			UIPictureBase.base_y = -IN.hh * 1.4f * 0.015625f;
			this.Gob = new GameObject("noel_tyan_kawaisou");
			this.Mosaic = new MosaicShower(this.Gob.transform);
			if (TrsParent != null)
			{
				this.Gob.transform.SetParent(TrsParent, false);
				this.Gob.layer = TrsParent.gameObject.layer;
			}
			this.MtrPxl = MTR.newMtr("Hachan/UiPicture");
			this.MtrPxl.name = "MtrPxl";
			UIPictureBase.FlgStopAutoFade = new Flagger(null, null);
			if (UIPictureBase.OSpineEventPtcKey == null)
			{
				UIPictureBase.OSpineEventPtcKey = new BDic<string, StringKey>(4);
			}
			if (FnMdCreate != null)
			{
				this.MdKarioki = FnMdCreate(this, this.MtrPxl);
				this.reloadUiScript(false);
			}
		}

		public void reloadUiScript(bool fine_front = false)
		{
			int num = 50;
			UIPictureBodyData.initUiPictureBodyData();
			this.AEmot = new UIPictureBase.PrEmotion[num];
			for (int i = 0; i < num; i++)
			{
				PxlCharacter noelUiPic = MTR.NoelUiPic;
				UIEMOT uiemot = (UIEMOT)i;
				PxlPose poseByName = noelUiPic.getPoseByName(uiemot.ToString().ToLower());
				this.AEmot[i] = new UIPictureBase.PrEmotion((UIEMOT)i, this, (poseByName != null) ? poseByName.getSequence(0) : null);
			}
			BetobetoManager.initBetobetoManager();
			UIPictureBodyData.readBodyDataScript(this);
			Color32 color = C32.d2c(4285101161U);
			this.MdKarioki.setMtrColor("_BaseColor", color);
			if (this.MtrSpine != null)
			{
				this.MtrSpine.name = "MtrSpine";
				this.MtrSpine.SetColor("_BaseColor", color);
				if (UIPictureBase.VALIDATION)
				{
					for (int j = 0; j < num; j++)
					{
						this.AEmot[j].validateSpineCsv(this.MtrSpine);
					}
				}
			}
			if (fine_front)
			{
				this.pre_emot = UIEMOT._OFFLINE;
				this.changeEmotDefault(false, false);
			}
		}

		public void assignSpineBodyData(UIPictureBodySpine BdSp, UIEMOT emot, UIPictureBase.EMSTATE st, SpineViewer Vw, bool matching_or)
		{
			this.AEmot[(int)emot].assignState(BdSp, st, matching_or);
			if (Vw != null)
			{
				this.SpineViewerInitObject(Vw);
			}
		}

		public void SpineViewerInitObject(SpineViewer Vw)
		{
			Vw.initGameObject(this.GobMeshHolder);
			if (this.MtrSpine == null)
			{
				this.MtrSpine = MTRX.newMtr(MTRX.MtrSpineDefault);
			}
		}

		public void fineCurrentBodyMaterial()
		{
			if (this.CurBodyData != null)
			{
				this.CurBodyData.fineMaterial(this.MdKarioki);
			}
		}

		public virtual bool isFacingEnemy()
		{
			return false;
		}

		public virtual float TS
		{
			get
			{
				return 1.25f;
			}
		}

		public virtual float TS_animation
		{
			get
			{
				return 1f;
			}
		}

		public virtual void run(float fcnt, float fcnt_picture, bool can_transfer_uipic = true)
		{
			if (this.can_transfer && (can_transfer_uipic || this.t_force_recheck_allocate > 0f))
			{
				if (X.D || this.t_force_recheck_allocate > 0f)
				{
					float num = this.TS * (X.D ? ((float)X.AF) : 0.5f) * 1.7f;
					this.FDCon.run(this, num, this.TS_animation);
					if (this.TS_animation > 0f && this.t_recheck >= 0f && (this.t_recheck -= num) < 0f && !this.changeEmotDefault(false, this.t_force_recheck_allocate > 0f))
					{
						this.t_recheck = 24f;
					}
					if (this.t_force_recheck_allocate > 0f)
					{
						this.t_force_recheck_allocate = X.Mx(0f, this.t_force_recheck_allocate - num);
					}
				}
			}
			else if (X.D)
			{
				this.FDCon.runGroundLevel(this, (float)X.AF * this.TS * 1.7f, false);
			}
			if (X.D && this.CurBodyData != null)
			{
				this.CurBodyData.run(this.Gob.transform, (float)X.AF, this.TS * fcnt_picture, false);
			}
			if (this.need_uipic_texture_remake)
			{
				this.fineTexture();
			}
		}

		public virtual void runPost()
		{
			if (X.D)
			{
				if (this.t_flush_fade < 22f)
				{
					if ((this.FLUSH_FADE_COL & 16777215U) != 16777215U)
					{
						this.t_flush_fade += (float)X.AF;
						this.need_color_fine = true;
					}
					else
					{
						this.t_flush_fade = 22f;
					}
				}
				if (this.need_color_fine)
				{
					this.setMulColor(this.MulColor);
				}
				if (this.PosTeScale.z != 0f)
				{
					this.PosTeScale.z = 0f;
				}
			}
		}

		public int isTortureEmot()
		{
			return this.isTortureEmot(this.pre_emot);
		}

		public int isPreTortureEmot()
		{
			return this.isTortureEmot(this.pre_emot);
		}

		public int isTortureEmot(UIEMOT em)
		{
			switch (em)
			{
			case UIEMOT.LAYING_EGG:
			case UIEMOT.TORTURE_GROUNDBURY:
				return 2;
			case UIEMOT.TORTURE_SLIME_0:
			case UIEMOT.TORTURE_SLIME_1:
			case UIEMOT.TORTURE_TENTACLE_0:
			case UIEMOT.TORTURE_TENTACLE_1:
			case UIEMOT.TORTURE_TENTACLE_2:
			case UIEMOT.TORTURE_SNAKE_0:
			case UIEMOT.TORTURE_SNAKE_1:
			case UIEMOT.TORTURE_KETSUDASI:
			case UIEMOT.TORTURE_GOLEM_INJECT:
			case UIEMOT.TORTURE_MKB:
			case UIEMOT.TORTURE_MKB_ACME:
			case UIEMOT.TORTURE_ROMERO:
			case UIEMOT.TORTURE_SWALLOWED:
			case UIEMOT.TORTURE_DRILLN:
			case UIEMOT.TORTURE_DRILLN_2:
			case UIEMOT.TORTURE_MKB_URCHIN:
			case UIEMOT.TORTURE_BACKINJ:
			case UIEMOT.SHRIMP:
				return 1;
			}
			return 0;
		}

		public bool isCrouchOrDownEmot(UIEMOT em)
		{
			return em - UIEMOT.CROUCH <= 4 || em == UIEMOT.WETTEN_MILK;
		}

		public void fineEmState()
		{
			this.changeEmotIn(this.pre_emot, this.checkPlayerState(), null, (UIPictureFader.UIP_RES)0);
		}

		public bool isPoseStand()
		{
			return this.pre_emot == UIEMOT.STAND;
		}

		public bool isPoseLayingEgg()
		{
			return this.pre_emot == UIEMOT.LAYING_EGG;
		}

		public virtual UIPictureBase.EMSTATE checkPlayerState()
		{
			return this.pre_emstate;
		}

		public virtual void newGame()
		{
			this.recheck_emot = true;
			this.FDCon.Blur();
			this.pre_emot = UIEMOT._OFFLINE;
			this.fineCurrentBodyMaterial();
		}

		public virtual bool changeEmotDefault(bool immediate = false, bool force_change = false)
		{
			this.t_recheck = -1f;
			return true;
		}

		public virtual bool faderEnable(UIPictureFader.UIPFader Fader)
		{
			return true;
		}

		public virtual bool setFade(string fade_key, UIPictureBase.EMSTATE add_state = UIPictureBase.EMSTATE.NORMAL, bool immediate = false, bool force_change = false, bool state_absolute = false)
		{
			Bench.P("UIP-Fade");
			Bench.P(fade_key);
			bool flag = false;
			UIPictureFader.UIP_RES uip_RES = this.FDCon.Explode(this, fade_key, immediate, force_change, !force_change && !this.fader_restartable);
			this.t_recheck = -1f;
			if ((uip_RES & UIPictureFader.UIP_RES.CHANGED) != (UIPictureFader.UIP_RES)0)
			{
				this.readFader(this.FDCon.Cur, uip_RES | (state_absolute ? UIPictureFader.UIP_RES.STATE_ABSOLUTE : ((UIPictureFader.UIP_RES)0)), add_state, force_change);
				flag = true;
			}
			else
			{
				if ((uip_RES & UIPictureFader.UIP_RES.REDRAW) != (UIPictureFader.UIP_RES)0)
				{
					this.redrawMainMesh(this.pre_emot, this.pre_emstate, 0);
				}
				if ((uip_RES & UIPictureFader.UIP_RES.TO_DROP) != (UIPictureFader.UIP_RES)0)
				{
					flag = true;
				}
			}
			Bench.Pend(fade_key);
			Bench.Pend("UIP-Fade");
			return flag;
		}

		public virtual UIPictureFader.UIP_RES readFader(UIPictureFader.UIPFader Fader, UIPictureFader.UIP_RES res, UIPictureBase.EMSTATE add_state = UIPictureBase.EMSTATE.NORMAL, bool force_change = false)
		{
			if (Fader == null)
			{
				return (UIPictureFader.UIP_RES)0;
			}
			this.need_color_fine = true;
			if ((res & UIPictureFader.UIP_RES.STATE_ABSOLUTE) == (UIPictureFader.UIP_RES)0)
			{
				add_state |= this.checkPlayerState();
			}
			UIPictureFader.UIP_RES uip_RES = this.changeEmotIn(Fader.getEmot(), Fader.add_state | add_state, Fader, res);
			if ((uip_RES == UIPictureFader.UIP_RES.CHANGED || (res & UIPictureFader.UIP_RES.REDRAW) != (UIPictureFader.UIP_RES)0) && this.fader_restartable && X.xors(100) < (int)Fader.same_restart)
			{
				this.redrawMainMesh(this.pre_emot, this.pre_emstate, 1);
			}
			else if (uip_RES == UIPictureFader.UIP_RES.NOW_LOADING)
			{
				this.FDCon.AssignGroundNext(Fader, 90);
			}
			return uip_RES;
		}

		public virtual bool fader_restartable
		{
			get
			{
				return true;
			}
		}

		public string getCurrentFadeKey(bool consider_next = true)
		{
			return this.FDCon.getCurrentFadeKey(consider_next);
		}

		public UIPictureFader.UIPFader getFader(string key)
		{
			return this.FDCon.Get(key, true, false);
		}

		public UIPictureBase.PrEmotion GetEmot(UIEMOT v, ref UIPictureBase.EMSTATE st)
		{
			if (X.SP_SENSITIVE)
			{
				v = UIPictureBase.emotSuperSensitive(v, ref st);
				st &= ~UIPictureBase.EMSTATE.WET;
			}
			UIPictureBase.PrEmotion prEmotion = this.AEmot[(int)v];
			if (X.SENSITIVE && (st & UIPictureBase.EMSTATE.TORNED) != UIPictureBase.EMSTATE.NORMAL)
			{
				st &= ~UIPictureBase.EMSTATE.TORNED;
				st |= UIPictureBase.EMSTATE.LOWHP | UIPictureBase.EMSTATE.SER | UIPictureBase.EMSTATE.LOWMP;
			}
			if (X.SENSITIVE && (st & UIPictureBase.EMSTATE.DEAD) != UIPictureBase.EMSTATE.NORMAL)
			{
				st &= ~UIPictureBase.EMSTATE.DEAD;
				st |= UIPictureBase.EMSTATE.SER | UIPictureBase.EMSTATE.SHAMED | UIPictureBase.EMSTATE.LOWMP;
			}
			if (X.sensitive_level >= 2)
			{
				st |= UIPictureBase.EMSTATE.SP_SENSITIVE;
			}
			st = prEmotion.fineSt(st);
			return prEmotion;
		}

		public UIPictureFader.UIP_RES changeEmotIn(UIEMOT v, UIPictureBase.EMSTATE st, UIPictureFader.UIPFader Fd = null, UIPictureFader.UIP_RES uip_res = (UIPictureFader.UIP_RES)0)
		{
			UIPictureBase.EMSTATE emstate = st;
			UIPictureBase.PrEmotion emot = this.GetEmot(v, ref st);
			UIPictureBase.EMSTATE_ADD emstate_ADD = this.pre_sta;
			UIPictureBase.EMSTATE_ADD additionalState = this.getAdditionalState(true);
			if (this.pre_emot == v && this.pre_emstate == st && emstate_ADD == additionalState)
			{
				return (UIPictureFader.UIP_RES)0;
			}
			UIPictureBase.PrEmotion.BodyPaint bodyPaint = emot.Get(st);
			UIPictureBodyData uipictureBodyData = bodyPaint.Body.getReplaceTerm() ?? bodyPaint.Body;
			if ((uip_res & UIPictureFader.UIP_RES.JUST_PREPARE) != (UIPictureFader.UIP_RES)0)
			{
				uipictureBodyData.isPreparedResource();
				return UIPictureFader.UIP_RES.JUST_PREPARE;
			}
			if (uipictureBodyData.isPreparedResource())
			{
				this.closeEmot(uipictureBodyData);
				this.CurBodyData = uipictureBodyData;
				this.CurBodyData.initEmot(st, this.MdKarioki);
				this.redrawMainMesh(v, st, 2);
				this.changeEmotFinalize(this.CurBodyData.Mtr, v, st, uip_res);
				this.pre_emot = v;
				this.pre_emstate0 = emstate;
				this.pre_emstate = st;
				this.CurBodyData.run(this.Gob.transform, 0f, 0f, true);
				return UIPictureFader.UIP_RES.CHANGED;
			}
			UIEMOT uiemot = UIPictureBase.emotSuperSensitive(v, ref st);
			st &= ~UIPictureBase.EMSTATE.WET;
			if (uiemot != v)
			{
				return this.changeEmotIn(uiemot, st, Fd, uip_res);
			}
			return UIPictureFader.UIP_RES.NOW_LOADING;
		}

		public void fineTexture()
		{
			if (this.CurBodyData != null)
			{
				this.need_uipic_texture_remake = false;
				this.CurBodyData.fineMaterial(this.MdKarioki);
			}
		}

		public void initFlushFade(float delay = 0f)
		{
			this.t_flush_fade = X.Mn(0f, -delay);
		}

		public virtual ValotileRenderer useValotileFilterMode(bool flag)
		{
			return null;
		}

		protected virtual void changeEmotFinalize(Material _Mtr, UIEMOT cemot, UIPictureBase.EMSTATE st, UIPictureFader.UIP_RES res)
		{
		}

		protected virtual void closeEmot(UIPictureBodyData Next)
		{
			if (this.CurBodyData != null)
			{
				this.CurBodyData.closeEmot(Next);
			}
		}

		public Color32 getColorTe()
		{
			return MTRX.ColWhite;
		}

		public void setColorTe(C32 Col, C32 CMul, C32 CAdd)
		{
			this.MtrPxl.SetColor("_AddColor", CAdd.C);
			int num = ((CAdd.C.r > 0 || CAdd.C.g > 0 || CAdd.C.b > 0) ? 1 : 0);
			this.MtrPxl.SetFloat("_UseAddColor", (float)num);
			if (this.MtrSpine != null)
			{
				this.MtrSpine.SetColor("_AddColor", CAdd.C);
				this.MtrSpine.SetFloat("_UseAddColor", (float)num);
			}
			this.setMulColor(CMul.C);
		}

		private void setMulColor(Color32 CMul)
		{
			this.need_color_fine = false;
			this.MulColor = CMul;
			float num = (1f - (float)X.Mn(X.Mn(X.Mn(CMul.a, CMul.g), CMul.b), CMul.r) / 255f) * 0.25f;
			float num2 = ((this.ground_level_ >= 0f) ? (1f - X.ZLINE(this.ground_level_)) : X.ZLINE(-this.ground_level_));
			CMul.r = (byte)((float)CMul.r * num2);
			CMul.g = (byte)((float)CMul.g * num2);
			CMul.b = (byte)((float)CMul.b * num2);
			num = X.NI(num, 0.5f, 1f - num2);
			if (this.t_flush_fade < 22f)
			{
				float num3 = 1f - X.ZPOW(this.t_flush_fade, 22f);
				num = X.Scr(num, num3);
				CMul = (this.MulColor = MTRX.colb.Set(CMul).blend(this.FLUSH_FADE_COL, num3).C);
			}
			this.MtrPxl.SetColor("_Color", CMul);
			this.MtrSpine.SetColor("_FillColor", CMul);
			this.MtrSpine.SetFloat("_FillPhase", num);
		}

		public Vector2 getScaleTe()
		{
			return this.PosTeScale;
		}

		public void setScaleTe(Vector2 V)
		{
			this.PosTeScale.Set(V.x, V.y, 1f);
		}

		protected virtual void redrawMainMesh(UIEMOT v, UIPictureBase.EMSTATE st, int first = 0)
		{
			UIPictureBase.PrEmotion.BodyPaint bodyPaint = this.AEmot[(int)v].Get(st);
			float num = 0f;
			this.need_color_fine = true;
			if (!this.CurBodyData.Has(UIPictureBodyData.ANIM.ROTATE))
			{
				this.draw_agR = 0f;
			}
			else if (first >= 1)
			{
				this.draw_agR = X.NIXP(-1f, 1f) * 0.006f * 3.1415927f;
			}
			else
			{
				num = X.NIXP(-1f, 1f) * 0.002f * 3.1415927f;
			}
			this.PosSyncSlide = new Vector3(0f, 0f, 1f);
			this.Gob.transform.localEulerAngles = new Vector3(0f, 0f, (this.draw_agR + num) / 3.1415927f * 180f);
			this.CurBodyData.redraw(this.MdKarioki, bodyPaint, st, first, this.getEffectTarget());
			if (first >= 1)
			{
				this.Mosaic.setTarget(this.CurBodyData, first >= 2);
				this.CurBodyData.run(this.Gob.transform, 0f, 0f, true);
			}
		}

		public virtual IEfPtcSetable getEffectTarget()
		{
			return null;
		}

		public virtual UIPictureBase.EMSTATE getCurrentState()
		{
			return this.pre_emstate;
		}

		public virtual UIPictureBase.EMSTATE_ADD getAdditionalState(bool force_fine = false)
		{
			return this.pre_sta;
		}

		public static UIEMOT emotSuperSensitive(UIEMOT v, ref UIPictureBase.EMSTATE st)
		{
			if (v > UIEMOT.CROUCH && v - UIEMOT.DOWN > 3)
			{
				switch (v)
				{
				case UIEMOT.DAMAGE_THUNDER:
				case UIEMOT.DAMAGE_PRESS:
				case UIEMOT.DAMAGE_PRESS_T:
				case UIEMOT.CUTS_COW_0:
				case UIEMOT.CUTS_COW_1:
				case UIEMOT.CUTS_IN_STOMACH:
				case UIEMOT.CUTS_PEEREMOVE:
				case UIEMOT.CUTS_LAYEGG:
				case UIEMOT.CUTS_EGGREMOVE:
				case UIEMOT.CUTS_IYAPAN:
					return v;
				case UIEMOT.PAJAMA:
					return UIEMOT.STAND;
				case UIEMOT.WEB_TRAPPED:
				case UIEMOT.WEB_TRAPPED_DOWN:
					return UIEMOT.CROUCH;
				}
				if (IN.totalframe % 90 < 75)
				{
					st |= UIPictureBase.EMSTATE.ABSORBED;
				}
				if (IN.totalframe % 73 < 41)
				{
					st |= UIPictureBase.EMSTATE.SMASH;
				}
				return UIEMOT.DAMAGE_0 + IN.totalframe % 2;
			}
			return v;
		}

		public virtual bool FineBetobeto(BetobetoManager.SvTexture Svt)
		{
			return this.CurBodyData is UIPictureBodySpine && !(this.CurBodyData as UIPictureBodySpine).FineBetobeto(true);
		}

		public bool applyDamageDebug(string t)
		{
			MGATTR mgattr;
			if (!FEnum<MGATTR>.TryParse(t.ToUpper(), out mgattr, true))
			{
				X.de("MGATTR にない enum 値: " + t, null);
				return false;
			}
			this.applyDamage(mgattr, X.NIXP(1f, 10f), X.NIXP(1f, 10f), UIPictureBase.EMSTATE.NORMAL, false, null, false);
			return true;
		}

		public virtual void applyWetten(UIPictureBase.EMWET wet_pat, bool force_cutin_use = false)
		{
			this.setFade("wetten", UIPictureBase.EMSTATE.NORMAL, false, false, false);
		}

		public void applyDamage(MGATTR attr, float quake_sinx = 0f, float quake_siny = 0f, UIPictureBase.EMSTATE add_emot = UIPictureBase.EMSTATE.NORMAL, bool decline_additional_effect = false, string fade_key = null, bool fade_force = false)
		{
			UIPictureBase.EMSTATE emstate = add_emot | this.checkPlayerState();
			if (TX.noe(fade_key))
			{
				fade_key = "damage";
			}
			else if (TX.isStart(fade_key, "!", 0))
			{
				fade_key = TX.slice(fade_key, 1);
				fade_force = true;
			}
			this.applyEmotionEffect(attr, ref emstate, ref fade_key, 1f, true, decline_additional_effect, quake_sinx, quake_siny);
			this.setFade(fade_key, emstate, fade_force, fade_force, false);
		}

		protected virtual void applyEmotionEffect(MGATTR attr, ref UIPictureBase.EMSTATE st, ref string fade_key, float blink_alpha, bool ui_darken, bool decline_additional_effect = false, float quake_sinx = 0f, float quake_siny = 0f)
		{
		}

		public virtual void destruct()
		{
			if (this.Mosaic != null)
			{
				this.Mosaic.destruct();
				this.Mosaic = null;
			}
			this.MdKarioki = null;
			this.AEmot = null;
		}

		public UIPictureBase.EMSTATE getMultipleEmot(string state)
		{
			if (state == "" || state == "0")
			{
				return UIPictureBase.EMSTATE.NORMAL;
			}
			string[] array = state.ToUpper().Split(new char[] { '|' });
			int num = array.Length;
			UIPictureBase.EMSTATE emstate = UIPictureBase.EMSTATE.NORMAL;
			for (int i = 0; i < num; i++)
			{
				UIPictureBase.EMSTATE emstate2 = UIPictureBase.EMSTATE.NORMAL;
				if (!(array[i] == ""))
				{
					if (!FEnum<UIPictureBase.EMSTATE>.TryParse(array[i], out emstate2, true))
					{
						X.de("不明なEMSTATE: " + array[i], null);
					}
					else
					{
						emstate |= emstate2;
					}
				}
			}
			return emstate;
		}

		public virtual bool SpineListenerEvent(UIPictureBodySpine _Body, TrackEntry Entry, Event e)
		{
			if (TX.isStart(e.Data.Name, "PTC_", 0))
			{
				StringKey stringKey;
				if (!UIPictureBase.OSpineEventPtcKey.TryGetValue(e.Data.Name, out stringKey))
				{
					Dictionary<string, StringKey> ospineEventPtcKey = UIPictureBase.OSpineEventPtcKey;
					string name = e.Data.Name;
					StringKey stringKey2 = new StringKey(TX.slice(e.Data.Name, 4));
					ospineEventPtcKey[name] = stringKey2;
					stringKey = stringKey2;
				}
				this.setSpinePtcSetter(stringKey, e.Int, e.Float);
				return true;
			}
			if (e.Data.Name == "loop" && e.Int >= 1)
			{
				_Body.getViewer().setAnmLoopFrameSecond(Entry.Animation.Name, Entry.AnimationTime);
			}
			return false;
		}

		protected virtual void setSpinePtcSetter(StringKey Key, int vi, float vf)
		{
			X.dl("PTC set to UI:" + vi.ToString() + "/" + vf.ToString(), null, false, false);
		}

		public float ground_level
		{
			get
			{
				return this.ground_level_;
			}
			set
			{
				if (value != this.ground_level_)
				{
					this.ground_level_ = value;
					this.need_color_fine = true;
				}
			}
		}

		public UIPictureBase.EMSTATE_ADD getMultipleAdditionalEmot(string state)
		{
			if (state == "")
			{
				return UIPictureBase.EMSTATE_ADD.NORMAL;
			}
			bool flag = false;
			if (TX.isStart(state, "!", 0))
			{
				flag = true;
				state = TX.slice(state, 1);
			}
			string[] array = state.ToUpper().Split(new char[] { '|' });
			int num = array.Length;
			UIPictureBase.EMSTATE_ADD emstate_ADD = UIPictureBase.EMSTATE_ADD.NORMAL;
			for (int i = 0; i < num; i++)
			{
				UIPictureBase.EMSTATE_ADD emstate_ADD2 = UIPictureBase.EMSTATE_ADD.NORMAL;
				if (!FEnum<UIPictureBase.EMSTATE_ADD>.TryParse(array[i], out emstate_ADD2, true))
				{
					X.de("不明なEMSTATE_ADD: " + array[i], null);
				}
				else
				{
					emstate_ADD |= emstate_ADD2;
				}
			}
			if (!flag)
			{
				return emstate_ADD;
			}
			return ~emstate_ADD;
		}

		public Vector3 getEffectShift(bool unstabilize)
		{
			if (this.CurBodyData == null)
			{
				return Vector2.zero;
			}
			if (!unstabilize)
			{
				return new Vector3(this.CurBodyData.effect_shift_ux, this.CurBodyData.effect_shift_uy, 1f / this.CurBodyData.scale);
			}
			return new Vector3(this.CurBodyData.shift_ux + this.CurBodyData.effect_shift_ux * this.CurBodyData.scale, this.CurBodyData.shift_uy + this.CurBodyData.effect_shift_uy * this.CurBodyData.scale, 1f);
		}

		public UIPictureBodyData getBodyData()
		{
			return this.CurBodyData;
		}

		public virtual Vector2 getCenterUPos(bool consider_effect_shift = false)
		{
			return Vector3.zero;
		}

		public virtual bool isPlayerStateNormal()
		{
			return true;
		}

		public virtual void fineMeshTarget(Mesh Ms)
		{
		}

		public virtual float is_position_right
		{
			get
			{
				return 0f;
			}
		}

		public bool recheck_emot
		{
			get
			{
				return this.t_recheck >= 0f;
			}
			set
			{
				if (value && this.t_recheck < 0f)
				{
					this.t_recheck = 0f;
				}
			}
		}

		public bool force_recheck_allocate
		{
			set
			{
				if (!value)
				{
					return;
				}
				this.t_force_recheck_allocate = 3f;
				this.t_recheck = 0f;
			}
		}

		public UIPictureBase recheck(int delay = 0, int _force_rechec_alloc = 0)
		{
			this.t_recheck = ((this.t_recheck < 0f) ? ((float)delay) : X.Mn((float)delay, this.t_recheck));
			if (_force_rechec_alloc > 0)
			{
				this.t_force_recheck_allocate = X.Mx((float)_force_rechec_alloc, this.t_force_recheck_allocate);
			}
			return this;
		}

		public UIEMOT getCurEmot()
		{
			return this.pre_emot;
		}

		public static void doAllBitPattern(int st0, UIPictureBase.FnStateBitRoler Fn)
		{
			int num = st0;
			int i = 0;
			int num2 = X.beki_cnt((uint)num);
			while (i <= num2)
			{
				if ((num & (1 << i)) != 0)
				{
					num &= ~(1 << i);
					for (int j = 0; j < i; j++)
					{
						if ((st0 & (1 << j)) != 0)
						{
							num |= 1 << j;
						}
					}
					if (num != 0)
					{
						Fn((UIPictureBase.EMSTATE)num);
					}
					i = 0;
				}
				else
				{
					i++;
				}
			}
		}

		public static bool VALIDATION = false;

		public static UIPictureBase.EMSTATE[] Afamily = new UIPictureBase.EMSTATE[0];

		public GameObject Gob;

		public GameObject GobMeshHolder;

		protected MeshDrawer MdKarioki;

		private MdArranger Ma;

		protected UIEMOT pre_emot = UIEMOT._OFFLINE;

		protected UIPictureBase.EMSTATE pre_emstate0;

		protected UIPictureBase.EMSTATE pre_emstate;

		protected UIPictureBase.EMSTATE_ADD pre_sta = UIPictureBase.EMSTATE_ADD._NO_CHECK;

		protected UIPictureBase.PrEmotion[] AEmot;

		public bool can_transfer = true;

		private float draw_agR;

		public static float base_y;

		public float gm_shift_x;

		protected UIPictureBodyData CurBodyData;

		public Material MtrPxl;

		public Material MtrSpine;

		public UIPictureFader FDCon;

		private float t_recheck;

		private float t_force_recheck_allocate;

		public const float GROUND_SHIFT_PX = 50f;

		public const float STANDUP_SHIFT_PX = 50f;

		public const float mainTS = 1.25f;

		public const float Fader_TS = 1.7f;

		public float t_flush_fade = 22f;

		public const float FLUSH_FADE_MAXT = 22f;

		public uint FLUSH_FADE_COL = 16777215U;

		public Vector3 PosTeScale = new Vector3(1f, 1f, 1f);

		private Color32 MulColor = MTRX.ColWhite;

		private bool need_color_fine;

		private float ground_level_;

		public static Flagger FlgStopAutoFade;

		private MosaicShower Mosaic;

		public bool need_uipic_texture_remake;

		protected static BDic<string, StringKey> OSpineEventPtcKey;

		public Vector3 PosSyncSlide = new Vector3(0f, 0f, 1f);

		public enum EMWET
		{
			NORMAL,
			PEE,
			MILK
		}

		[Flags]
		public enum EMSTATE : uint
		{
			NORMAL = 0U,
			DIRT = 1U,
			PROG0 = 2U,
			PROG1 = 4U,
			PROG2 = 8U,
			LOWHP = 32U,
			BATTLE = 64U,
			SER = 128U,
			SHAMED = 256U,
			SMASH = 512U,
			ABSORBED = 1024U,
			STUNNED = 4096U,
			LOWMP = 8192U,
			EGGED = 16384U,
			WET = 32768U,
			BOTE = 65536U,
			ORGASM = 131072U,
			CONFUSED = 262144U,
			OSGM = 524288U,
			TORNED = 1048576U,
			SLEEP = 2097152U,
			DEAD = 4194304U,
			STONEOVER = 8388608U,
			SP_SENSITIVE = 1073741824U,
			_ALL = 1073741825U
		}

		[Flags]
		public enum EMSTATE_ADD : uint
		{
			NORMAL = 0U,
			DIRT0 = 1U,
			FROZEN = 2U,
			SENSITIVE = 8U,
			SP_SENSITIVE = 16U,
			_ALL = 32U,
			_NO_CHECK = 33U
		}

		public class PrEmotion
		{
			public PrEmotion(UIEMOT _emot_id, UIPictureBase _PCon, PxlSequence Sq)
			{
				this.ODrawer = new BDic<UIPictureBase.EMSTATE, UIPictureBase.PrEmotion.BodyPaint>();
				this.emot_id = _emot_id;
				if (Sq == null)
				{
					return;
				}
				int num = Sq.countFrames();
				int i = 0;
				while (i < num)
				{
					PxlFrame frame = Sq.getFrame(i);
					if (i > 0 && frame.name == "")
					{
						i++;
					}
					else
					{
						string[] array = frame.name.Split(new char[] { '_' });
						int num2 = array.Length;
						UIPictureBase.EMSTATE emstate = UIPictureBase.EMSTATE.NORMAL;
						for (int j = 0; j < num2; j++)
						{
							UIPictureBase.EMSTATE emstate2;
							if (!FEnum<UIPictureBase.EMSTATE>.TryParse(array[j].ToUpper(), out emstate2, true))
							{
								emstate |= emstate2;
							}
						}
						if (frame.name != "" && frame.name != "normal" && emstate == UIPictureBase.EMSTATE.NORMAL)
						{
							X.de("フレーム名の指定エラー ... pose " + Sq.pPose.title + ":" + i.ToString(), null);
						}
						if (this.ODrawer.ContainsKey(emstate))
						{
							X.de("フレーム感情値の重複 ... pose " + Sq.pPose.title + ":" + i.ToString(), null);
							i++;
						}
						else
						{
							this.avail_state |= emstate;
							int num3 = 1;
							int num4 = i + 1;
							while (num4 < num && !(Sq.getFrame(num4).name != ""))
							{
								num3++;
								num4++;
							}
							PxlFrame[] array2 = (this.assignState(UIPictureBodyData.Get(frame, _PCon), emstate, false).AFrm = new PxlFrame[num3]);
							for (int k = 0; k < num3; k++)
							{
								array2[k] = Sq.getFrame(i + k);
							}
							if (this.DefaultTx == null)
							{
								this.DefaultTx = array2[0].getLayer(0).Img.get_I();
							}
							i += num3;
						}
					}
				}
			}

			public UIPictureBase.PrEmotion.BodyPaint assignState(UIPictureBodyData Bd, UIPictureBase.EMSTATE state, bool matching_or = false)
			{
				UIPictureBase.PrEmotion.BodyPaint bodyPaint = new UIPictureBase.PrEmotion.BodyPaint();
				bodyPaint.Body = Bd;
				if (bodyPaint.Body.Emot != null && bodyPaint.Body.Emot != this)
				{
					X.de("複数の UIEMOT に同じ body の登録はできません。", null);
				}
				bodyPaint.Body.Emot = this;
				this.ODrawer[state] = bodyPaint;
				this.avail_state |= state;
				if (this.Default == null)
				{
					this.Default = bodyPaint;
				}
				if (matching_or)
				{
					UIPictureBase.doAllBitPattern((int)state, delegate(UIPictureBase.EMSTATE _s)
					{
						this.assignState(Bd, _s, false);
					});
				}
				return bodyPaint;
			}

			public UIPictureBase.EMSTATE fineSt(UIPictureBase.EMSTATE st0)
			{
				UIPictureBase.EMSTATE emstate = st0 & this.avail_state;
				UIPictureBase.EMSTATE emstate2 = st0 & ~this.avail_state;
				UIPictureBase.PrEmotion.BodyPaint bodyPaint;
				if (this.ODrawer.TryGetValue(emstate, out bodyPaint))
				{
					return emstate | (emstate2 & bodyPaint.Body.variation_bits);
				}
				uint num = (uint)emstate;
				UIPictureBase.PrEmotion.BodyPaint bodyPaint2 = null;
				UIPictureBase.EMSTATE emstate3 = UIPictureBase.EMSTATE.NORMAL;
				if (num != 0U)
				{
					int num2 = X.beki_cnt(num);
					int i = 0;
					while (i <= num2)
					{
						if ((num & (1U << i)) != 0U)
						{
							num &= ~(1U << i);
							for (int j = 0; j < i; j++)
							{
								if ((emstate & (UIPictureBase.EMSTATE)(1 << j)) != UIPictureBase.EMSTATE.NORMAL)
								{
									num |= 1U << j;
								}
							}
							UIPictureBase.EMSTATE emstate4 = (UIPictureBase.EMSTATE)num;
							UIPictureBase.PrEmotion.BodyPaint bodyPaint3;
							if ((bodyPaint2 == null || emstate3 < emstate4) && this.ODrawer.TryGetValue(emstate4, out bodyPaint3))
							{
								bodyPaint2 = bodyPaint3;
								emstate3 = emstate4;
							}
							i = 0;
						}
						else
						{
							i++;
						}
					}
				}
				if (bodyPaint2 != null)
				{
					this.ODrawer[emstate] = bodyPaint2;
					return emstate | (emstate2 & bodyPaint2.Body.variation_bits);
				}
				return emstate;
			}

			public UIPictureBase.PrEmotion.BodyPaint Get(UIPictureBase.EMSTATE st)
			{
				st &= this.avail_state;
				return X.GetS<UIPictureBase.EMSTATE, UIPictureBase.PrEmotion.BodyPaint>(this.ODrawer, st, this.Default);
			}

			public override string ToString()
			{
				return "<EMOT>" + this.emot_id.ToString();
			}

			public void validateSpineCsv(Material MtrSpine)
			{
				foreach (KeyValuePair<UIPictureBase.EMSTATE, UIPictureBase.PrEmotion.BodyPaint> keyValuePair in this.ODrawer)
				{
					if (keyValuePair.Value.Body is UIPictureBodySpine)
					{
						(keyValuePair.Value.Body as UIPictureBodySpine).validateCsv(MtrSpine);
					}
				}
			}

			public void prepareMaterial(object SvT)
			{
				foreach (KeyValuePair<UIPictureBase.EMSTATE, UIPictureBase.PrEmotion.BodyPaint> keyValuePair in this.ODrawer)
				{
					if (keyValuePair.Value.Body is UIPictureBodySpine)
					{
						(keyValuePair.Value.Body as UIPictureBodySpine).prepareMaterial(SvT);
					}
				}
			}

			public readonly UIEMOT emot_id;

			public UIPictureBase.EMSTATE avail_state;

			public Texture DefaultTx;

			public readonly BDic<UIPictureBase.EMSTATE, UIPictureBase.PrEmotion.BodyPaint> ODrawer;

			public UIPictureBase.PrEmotion.BodyPaint Default;

			public class BodyPaint
			{
				public UIPictureBodyData Body;

				public PxlFrame[] AFrm;
			}
		}

		public delegate void FnStateBitRoler(UIPictureBase.EMSTATE _s);
	}
}
