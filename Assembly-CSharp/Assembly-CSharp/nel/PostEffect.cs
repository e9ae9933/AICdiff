using System;
using Kayac;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class PostEffect : Effect<PostEffectItem>
	{
		public static PostEffect IT
		{
			get
			{
				return (M2DBase.Instance as NelM2DBase).PE;
			}
		}

		public PostEffect(NelM2DBase _M2D, int alloc_cnt)
			: base(_M2D.GobBase, alloc_cnt)
		{
			this.M2D = _M2D;
			this.AMtr = new PostEffect.PEItem[30];
			this.ATimeFixedEffect = new PostEffect.TimeFixEf[4];
			PostEffect.PEMaterial.FnDrawPostScreenEffect fnDrawPostScreenEffect = new PostEffect.PEMaterial.FnDrawPostScreenEffect(PostEffectItem.drawScreen_hpreduce);
			PostEffect.PEMaterial.FnDrawPostScreenEffect fnDrawPostScreenEffect2 = new PostEffect.PEMaterial.FnDrawPostScreenEffect(PostEffectItem.drawScreen_mpreduce);
			PostEffect.PEMaterial.FnDrawPostScreenEffect fnDrawPostScreenEffect3 = new PostEffect.PEMaterial.FnDrawPostScreenEffect(PostEffectItem.drawScreen_mpabsorbed);
			PostEffect.PEMaterial.FnDrawPostScreenEffect fnDrawPostScreenEffect4 = new PostEffect.PEMaterial.FnDrawPostScreenEffect(PostEffectItem.drawScreen_default);
			PostEffect.PEMaterial.FnDrawPostScreenEffect fnDrawPostScreenEffect5 = new PostEffect.PEMaterial.FnDrawPostScreenEffect(PostEffectItem.drawScreen_wormtrapped);
			PostEffect.PEMaterial.FnDrawPostScreenEffect fnDrawPostScreenEffect6 = new PostEffect.PEMaterial.FnDrawPostScreenEffect(PostEffectItem.drawScreen_gas_applied);
			PostEffect.PEMaterial.FnDrawPostScreenEffect fnDrawPostScreenEffect7 = new PostEffect.PEMaterial.FnDrawPostScreenEffect(PostEffectItem.drawScreen_irisout);
			PostEffect.PEMaterial.FnDrawPostScreenEffect fnDrawPostScreenEffect8 = new PostEffect.PEMaterial.FnDrawPostScreenEffect(PostEffectItem.drawScreen_go_close_eye);
			PostEffect.PEMaterial.FnDrawPostScreenEffect fnDrawPostScreenEffect9 = new PostEffect.PEMaterial.FnDrawPostScreenEffect(PostEffectItem.drawScreen_wholeripple);
			this.AMtr[0] = new PostEffect.PEMaterial("poste/hp_reduce", POSTM.HP_REDUCE, fnDrawPostScreenEffect, 0, PostEffect.PE_VAR.MINMAX)
			{
				auto_refine = 1
			};
			this.AMtr[1] = new PostEffect.PEMaterial("poste/mp_reduce", POSTM.MP_REDUCE, fnDrawPostScreenEffect2, 1, PostEffect.PE_VAR.MINMAX);
			this.AMtr[2] = new PostEffect.PEMaterial("poste/mp_absorbed", POSTM.MP_ABSORBED, fnDrawPostScreenEffect3, 1, PostEffect.PE_VAR.MINMAX);
			this.AMtr[15] = new PostEffect.PEMaterial("poste/burst", POSTM.BURST, fnDrawPostScreenEffect4, 2, PostEffect.PE_VAR.MINMAX);
			this.AMtr[4] = new PostEffect.PEMaterial("poste/flash", POSTM.FLASH, fnDrawPostScreenEffect4, 2, PostEffect.PE_VAR.MINMAX);
			Material material = MTR.newMtr("Hachan/PostShiftImageWhole");
			material.SetVector("_Scale", new Vector4(3.2f, 0.15f, 0f, 0f));
			MTRX.setMaterialST(material, "_NoiseTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
			this.AMtr[3] = new PostEffect.PEMaterial(material, POSTM.WHOLERIPPLE, fnDrawPostScreenEffect9, 2, PostEffect.PE_VAR.MINMAX);
			this.AMtr[5] = new PostEffect.PEMaterial("poste/summoner_activate", POSTM.SUMMONER_ACTIVATE, fnDrawPostScreenEffect4, 2, PostEffect.PE_VAR.MINMAX);
			Material material2 = MTR.newMtr("Hachan/RakuraiGlitchPE");
			MTRX.setMaterialST(material2, "_NoiseTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
			material2.SetFloat("_GrainStrength", 38f);
			material2.SetFloat("_Vignette", 0.7f);
			material2.SetFloat("_GlitchXScale", 0.008f);
			material2.SetFloat("_GlitchYScale", 0.011f);
			material2.SetFloat("_RGBNoiseScaleX", 0.87f);
			material2.SetFloat("_RGBNoiseScaleY", 0.23f);
			material2.SetFloat("_RGBGrain", 25f);
			this.AMtr[9] = new PostEffect.PEMaterial(material2, POSTM.JAMMING, new PostEffect.PEMaterial.FnDrawPostScreenEffect(PostEffectItem.drawScreen_glitch), 2, PostEffect.PE_VAR.MINMAX);
			this.AMtr[13] = new PostEffect.PEMaterial("poste/worm_trapped", POSTM.WORM_TRAPPED, fnDrawPostScreenEffect5, 2, PostEffect.PE_VAR.MINMAX)
			{
				auto_refine = 1
			};
			this.AMtr[6] = new PostEffect.PEMaterial("poste/laying_egg", POSTM.LAYING_EGG, fnDrawPostScreenEffect2, 0, PostEffect.PE_VAR.MINMAX);
			this.AMtr[7] = new PostEffect.PEMaterial("poste/enemy_overdrive_appear", POSTM.ENEMY_OVERDRIVE_APPEAR, fnDrawPostScreenEffect4, 2, PostEffect.PE_VAR.MINMAX);
			this.AMtr[14] = new PostEffect.PEMaterialWithMover("poste/thunder_trap", POSTM.THUNDER_TRAP, fnDrawPostScreenEffect4, 2, PostEffect.PE_VAR.MINMAX);
			this.AMtr[10] = new PostEffect.PEMaterial("poste/gas_applied", POSTM.GAS_APPLIED, fnDrawPostScreenEffect6, 0, PostEffect.PE_VAR.ADD)
			{
				auto_refine = 1
			}.setMaterialTexture("_NoiseTex", MTRX.SqEfPattern.getImage(6, 0));
			this.AMtr[8] = new PostEffect.PEMaterial("poste/magic_device_activate", POSTM.MAGIC_DEVICE_ACTIVATE, fnDrawPostScreenEffect4, 2, PostEffect.PE_VAR.MINMAX);
			this.AMtr[11] = new PostEffect.PEInterrupt(new LightPostProcessor(MTR.getShdContainer()), new PostEffect.PEInterrupt.FnInterruptExecution(PostEffectItem.executePostBloom), PostEffect.PE_VAR.SCREEN);
			this.AMtr[16] = new PostEffect.PEMaterial("poste/shotgun", POSTM.SHOTGUN, fnDrawPostScreenEffect4, 2, PostEffect.PE_VAR.MINMAX);
			this.AMtr[17] = new PostEffect.PEMaterial("poste/magicselect", POSTM.MAGICSELECT, fnDrawPostScreenEffect4, 2, PostEffect.PE_VAR.MINMAX);
			this.AMtr[12] = new PostEffect.PEMaterial(MTRX.MtrMeshNormal, POSTM.IRISOUT, fnDrawPostScreenEffect7, 1, PostEffect.PE_VAR.ADD);
			this.AMtr[18] = new PostEffect.PEMaterial(MTRX.MtrMeshNormal, POSTM.GO_CLOSE_EYE, fnDrawPostScreenEffect8, 3, PostEffect.PE_VAR.SCREEN);
			this.AMtr[19] = new PostEffect.PESpecial(new PostEffect.PESpecial.FnSpecialExecution(PostEffectItem.executeTsSlow), PostEffect.PE_VAR.MINMAX);
			this.AMtr[20] = new PostEffect.PESpecial(new PostEffect.PESpecial.FnSpecialExecution(PostEffectItem.executeCamZoom2), PostEffect.PE_VAR.MINMAX);
			this.AMtr[21] = new PostEffect.PESpecial(new PostEffect.PESpecial.FnSpecialExecution(PostEffectItem.executeCamZoom2), PostEffect.PE_VAR.MINMAX);
			this.AMtr[24] = new PostEffect.PESpecial(new PostEffect.PESpecial.FnSpecialExecution(PostEffectItem.executeCamComfuse), PostEffect.PE_VAR.SCREEN);
			this.AMtr[26] = new PostEffect.PESpecial(new PostEffect.PESpecial.FnSpecialExecution(PostEffectItem.executeM2dVar0), PostEffect.PE_VAR.SCREEN);
			this.AMtr[22] = new PostEffect.PESpecial(new PostEffect.PESpecial.FnSpecialExecution(PostEffectItem.executeHeartBeat), PostEffect.PE_VAR.SCREEN);
			this.AMtr[25] = new PostEffect.PESpecial(new PostEffect.PESpecial.FnSpecialExecution(PostEffectItem.executeVolumeReduce), PostEffect.PE_VAR.MINMAX);
			this.AMtr[27] = new PostEffect.PESpecial(new PostEffect.PESpecial.FnSpecialExecution(PostEffectItem.executeBgmLower), PostEffect.PE_VAR.MINMAX);
			this.AMtr[28] = new PostEffect.PESpecial(new PostEffect.PESpecial.FnSpecialExecution(PostEffectItem.executeBgmWater), PostEffect.PE_VAR.MINMAX);
			this.AMtr[29] = new PostEffect.PESpecial(new PostEffect.PESpecial.FnSpecialExecution(PostEffectItem.executeCamFinalAlpha), PostEffect.PE_VAR.SCREEN);
			this.AMtr[23] = new PostEffect.PESpecial(new PostEffect.PESpecial.FnSpecialExecution(PostEffectItem.executeRain), PostEffect.PE_VAR.MINMAX);
			this.AMtrSuppress = new PostEffect.PEItem[]
			{
				this.AMtr[0],
				this.AMtr[1],
				this.AMtr[2],
				this.AMtr[4],
				this.AMtr[5],
				this.AMtr[6],
				this.AMtr[7],
				this.AMtr[8],
				this.AMtr[10],
				this.AMtr[11],
				this.AMtr[12],
				this.AMtr[13],
				this.AMtr[14],
				this.AMtr[15],
				this.AMtr[16],
				this.AMtr[17]
			};
			for (int i = this.AMtrSuppress.Length - 1; i >= 0; i--)
			{
				this.AMtr[i].restrict_alpha = 3;
			}
			this.useMeshDrawer = false;
			this.initEffect("PE", null, null, EFCON_TYPE.NORMAL);
			this.fineUiShift();
		}

		public void releaseMesh()
		{
			int num = 19;
			for (int i = 0; i < num; i++)
			{
				this.AMtr[i].releaseMesh();
			}
		}

		public void checkShiftPos()
		{
		}

		private PostEffectItem Pop(FnEffectRun Fn, float x, float y, float z, int time, int saf = 0)
		{
			PostEffectItem postEffectItem = base.Pop(64);
			postEffectItem.clear(this, "pe", x, y, z, time, saf);
			postEffectItem.setEffectSpecificFn(Fn);
			return postEffectItem;
		}

		public PostEffectItem setPE(POSTM postm, float z_maxt = 40f, float x_level = 1f, int saf = 0)
		{
			return this.Pop(PostEffectItem.FD_fnRunDraw_pe_basic, x_level, 0f, z_maxt, (int)postm, saf);
		}

		public PostEffectItem setSlow(float z_maxt = 40f, float x_level = 0f, int saf = 0)
		{
			PostEffectItem postEffectItem = this.Pop(PostEffectItem.FD_fnRunDraw_pe_once, 1f - x_level, 0f, z_maxt, 19, saf);
			this.addTimeFixedEffect(postEffectItem, 1f);
			return postEffectItem;
		}

		public PostEffectItem setSlowFading(float fade_frm = 40f, float hold_hrm = 40f, float x_level = 0f, int saf = 0)
		{
			PostEffectItem postEffectItem = this.Pop(PostEffectItem.FD_fnRunDraw_pe_fadeinout, 1f - x_level, fade_frm, hold_hrm, 19, saf);
			this.addTimeFixedEffect(postEffectItem, 1f);
			return postEffectItem;
		}

		public PostEffectItem setPEbounce(POSTM postm, float z_maxt = 40f, float x_level = 1f, int saf = 0)
		{
			return this.Pop(PostEffectItem.FD_fnRunDraw_pe_bounce, x_level, 0f, z_maxt, (int)postm, saf);
		}

		public PostEffectItem setPEabsorbed(POSTM postm, float fadein_time = 10f, float x_level = 1f, int saf = 0)
		{
			return this.Pop(PostEffectItem.FD_fnRunDraw_pe_absorbed, x_level, fadein_time, fadein_time * 3f, (int)postm, saf);
		}

		public PostEffectItem setPEabsorbed(POSTM postm, float fadein_time, float fadeout_time, float x_level, int saf)
		{
			return this.Pop(PostEffectItem.FD_fnRunDraw_pe_absorbed, x_level, fadein_time, fadeout_time, (int)postm, saf);
		}

		public PostEffectItem setPEfadeinout(POSTM postm, float fade_frm = 40f, float z_hold_hrm = 40f, float x_level = 1f, int saf = 0)
		{
			return this.Pop(PostEffectItem.FD_fnRunDraw_pe_fadeinout, x_level, fade_frm, z_hold_hrm, (int)postm, saf);
		}

		public PostEffectItem setPEfadeinoutZSINV(POSTM postm, float fade_frm = 40f, float hold_hrm = 40f, float x_level = 1f, int saf = 0)
		{
			return this.Pop(PostEffectItem.FD_fnRunDraw_pe_fadeinout_zsinv, x_level, fade_frm, hold_hrm, (int)postm, saf);
		}

		public override PostEffectItem Create()
		{
			return new PostEffectItem(this);
		}

		public PostEffect addTimeFixedEffect(EffectItem Ef, float factor = 1f)
		{
			if (Ef == null)
			{
				return this;
			}
			if (this.ATimeFixedEffect.Length <= this.time_fix_ef_cnt)
			{
				Array.Resize<PostEffect.TimeFixEf>(ref this.ATimeFixedEffect, this.time_fix_ef_cnt + 8);
			}
			PostEffect.TimeFixEf[] atimeFixedEffect = this.ATimeFixedEffect;
			int num = this.time_fix_ef_cnt;
			this.time_fix_ef_cnt = num + 1;
			atimeFixedEffect[num].Set(Ef, factor);
			return this;
		}

		public PostEffect addTimeFixedEffect(IAnimListener Anm, float factor = 1f)
		{
			if (Anm == null)
			{
				return this;
			}
			if (this.ATimeFixedEffect.Length <= this.time_fix_ef_cnt)
			{
				Array.Resize<PostEffect.TimeFixEf>(ref this.ATimeFixedEffect, this.time_fix_ef_cnt + 8);
			}
			PostEffect.TimeFixEf[] atimeFixedEffect = this.ATimeFixedEffect;
			int num = this.time_fix_ef_cnt;
			this.time_fix_ef_cnt = num + 1;
			atimeFixedEffect[num].Set(Anm, factor);
			return this;
		}

		public override void clear()
		{
			base.clear();
			int num = 30;
			for (int i = 0; i < num; i++)
			{
				this.AMtr[i].release();
			}
			this.ptc_eft_flag = false;
			this.PosMainMoverShift_ = Vector3.zero;
			this.ptc_slow_factor = -1f;
		}

		public void quitGame()
		{
			int num = 30;
			for (int i = 0; i < num; i++)
			{
				this.AMtr[i].destruct();
			}
			BGM.BusChn.clear(false);
			VoiceController.BusChn.clear(false);
		}

		public void fineUiShift()
		{
			float num = IN.w + 16f;
			float num2 = IN.h + 16f;
			this.VecUiShift = new Vector4(this.M2D.ui_shift_x / num, 0f, (IN.w - this.M2D.ui_shift_x) / num, IN.h / num2);
			int num3 = 30;
			for (int i = 0; i < num3; i++)
			{
				this.AMtr[i].fineUiShift(this.VecUiShift);
			}
		}

		public override void runDraw(float fcnt, bool runsetter = true)
		{
			float ts = Map2d.TS;
			float num = fcnt * ts;
			float num2 = fcnt - num;
			int num3 = this.time_fix_ef_cnt;
			int i = this.time_fix_ef_cnt - 1;
			if (this.time_fix_ef_cnt > 0)
			{
				while (i >= 0)
				{
					PostEffect.TimeFixEf timeFixEf = this.ATimeFixedEffect[i];
					if (!timeFixEf.isActive() || !timeFixEf.afAdd(num2))
					{
						PostEffect.TimeFixEf[] atimeFixedEffect = this.ATimeFixedEffect;
						int num4 = 1;
						int num5 = i;
						int num6 = this.time_fix_ef_cnt;
						this.time_fix_ef_cnt = num6 - 1;
						X.shiftEmpty<PostEffect.TimeFixEf>(atimeFixedEffect, num4, num5, num6);
					}
					i--;
				}
			}
			bool flag = this.activated;
			bool flag2 = this.restrict_activated > 0;
			if (flag)
			{
				this.checkShiftPos();
			}
			this.activated = false;
			this.restrict_activated = 0;
			this.PosMainMoverShift_.z = 0f;
			this.buf_timescale = (this.buf_cam_scale = 1f);
			this.buf_m2d_var0 = (this.buf_effect_confuse = 0f);
			this.buf_render_alpha = byte.MaxValue;
			BGM.BusChn.clear(false);
			VoiceController.BusChn.clear(false);
			PostEffectItem.PE = this;
			base.runDraw(num, runsetter);
			if ((this.restrict_activated > 0 || flag2) && CFG.posteffect_weaken > 0)
			{
				int num7 = (int)(10 - CFG.posteffect_weaken);
				int num8 = 0;
				for (int j = this.AMtrSuppress.Length - 1; j >= 0; j--)
				{
					PostEffect.PEItem peitem = this.AMtrSuppress[j];
					if (peitem.alpha > 0f)
					{
						peitem.restrict_alpha = ((this.restrict_activated <= num7) ? 3 : ((num8++ < num7) ? 2 : 0));
					}
					else
					{
						peitem.restrict_alpha = 3;
					}
				}
			}
			PostEffectItem.PE = null;
			if (this.activated || flag)
			{
				int num9 = 30;
				for (i = 0; i < num9; i++)
				{
					this.AMtr[i].fineAlpha(this, true);
				}
				Map2d.setTimeScale(this.buf_timescale, false);
				if (PostEffect.setable_camera_scale && this.M2D.Cam.getScale(false) != this.buf_cam_scale)
				{
					this.M2D.Cam.animateScaleTo(this.buf_cam_scale, 0);
				}
				if (this.M2D.curMap != null)
				{
					this.M2D.effect_variable0 = this.buf_m2d_var0;
				}
				this.M2D.Cam.effect_confuse = this.buf_effect_confuse;
				this.M2D.Cam.update_final_mesh_alpha255 = this.buf_render_alpha;
				BGM.BusChn.Check();
				VoiceController.BusChn.Check();
				this.M2D.Snd.sound_volume_for_effect = 1f - this.buf_snd_volume_reduce;
			}
			if (this.buf_timescale == 1f && num3 > 0)
			{
				this.time_fix_ef_cnt = 0;
				if (this.PtcSTCon != null)
				{
					this.PtcSTCon.destruct();
					this.PtcSTCon = null;
				}
			}
		}

		public void redrawOnlyMesh()
		{
			int num = 19;
			for (int i = 0; i < num; i++)
			{
				this.AMtr[i].redrawOnlyMesh(this);
			}
		}

		public void refineMaterialAlpha()
		{
			int num = 30;
			for (int i = 0; i < num; i++)
			{
				this.AMtr[i].fineAlpha(this, false);
			}
		}

		public void setAlpha(POSTM id, float a01)
		{
			PostEffect.PEItem peitem = this.AMtr[(int)id];
			peitem.setAlpha(a01);
			if (a01 > 0f)
			{
				this.activated = true;
				if (peitem.restrict_alpha != 1)
				{
					this.restrict_activated++;
				}
			}
		}

		public bool isRestricted(POSTM id)
		{
			if (CFG.posteffect_weaken > 0)
			{
				PostEffect.PEItem peitem = this.AMtr[(int)id];
				return peitem.restrict_alpha == 0 || peitem.restrict_alpha == 2;
			}
			return false;
		}

		public void setMap2dTimeScale(float a01)
		{
			this.buf_timescale *= a01;
			this.activated = true;
		}

		public void addMapCamZoom(float a01)
		{
			this.buf_cam_scale += a01;
			this.activated = true;
		}

		public void setDebugVariable(float a01)
		{
			BGM.BusChn.Set(BusChannelManager.CHN.BUS_WATER, a01);
			this.activated = true;
		}

		public void SetBgmWaterEffect(float a01)
		{
			BGM.BusChn.Set(BusChannelManager.CHN.BUS_WATER, a01);
			VoiceController.BusChn.Set(BusChannelManager.CHN.BUS_WATER, a01);
			this.activated = true;
		}

		public void SetBgmLowerEffect(float a01)
		{
			BGM.BusChn.Set(BusChannelManager.CHN.BUS_EFFECT, a01);
			this.activated = true;
		}

		public void setM2dVar0(float a01)
		{
			this.buf_m2d_var0 = a01;
			this.activated = true;
		}

		public void SetEffectConfusion(float a01)
		{
			this.buf_effect_confuse = a01;
			this.activated = true;
		}

		public void SetFinalRenderAlpha(byte b)
		{
			this.buf_render_alpha = b;
			this.activated = true;
		}

		public void setVolumeReduce(float a01)
		{
			this.buf_snd_volume_reduce = a01;
			this.activated = true;
		}

		public void activateFlagOn()
		{
			this.activated = true;
		}

		public Vector2 PosMainMoverShift
		{
			get
			{
				if (this.PosMainMoverShift_.z != 1f)
				{
					Vector2 posMainMoverShift = this.M2D.Cam.PosMainMoverShift;
					if (this.M2D.curMap == null || this.M2D.curMap.floort <= 20f)
					{
						this.PosMainMoverShift_ = new Vector3(posMainMoverShift.x * 64f, posMainMoverShift.y * 64f, 1f);
					}
					else
					{
						this.PosMainMoverShift_ = new Vector3(X.MULWALK(this.PosMainMoverShift_.x, posMainMoverShift.x * 64f, 0.05f), X.MULWALK(this.PosMainMoverShift_.y, posMainMoverShift.y * 64f, 0.05f), 1f);
					}
				}
				return this.PosMainMoverShift_;
			}
		}

		private bool activated;

		private int restrict_activated;

		private PostEffect.PEItem[] AMtr;

		private PostEffect.PEItem[] AMtrSuppress;

		public readonly NelM2DBase M2D;

		private float cam_shift_x;

		private float cam_shift_y;

		private PostEffect.TimeFixEf[] ATimeFixedEffect;

		private int time_fix_ef_cnt;

		private float ptc_slow_factor = -1f;

		private bool ptc_eft_flag;

		public static bool setable_camera_scale = true;

		private Vector4 VecUiShift;

		private Vector3 PosMainMoverShift_;

		private float buf_timescale = 1f;

		private float buf_cam_scale = 1f;

		private float buf_debug;

		private float buf_snd_volume_reduce;

		private float buf_m2d_var0;

		private float buf_effect_confuse;

		private byte buf_render_alpha = byte.MaxValue;

		private int debug_target_postm;

		private DebugLogBlockBase DbgB;

		private PostEffectItem[] AEfDebug;

		public abstract class PEItem
		{
			public PEItem(PostEffect.PE_VAR _var_set)
			{
				this.var_set = _var_set;
			}

			public virtual void releaseMesh()
			{
			}

			public virtual void setAlpha(float _alpha)
			{
				switch (this.var_set)
				{
				case PostEffect.PE_VAR.ADD:
					this.depalpha = X.Mn(_alpha + this.depalpha, 1f);
					return;
				case PostEffect.PE_VAR.SCREEN:
					this.depalpha = 1f - (1f - this.depalpha) * (1f - _alpha);
					return;
				case PostEffect.PE_VAR.MUL:
					this.depalpha = (this.depalpha + 1f) * (1f + _alpha) - 1f;
					return;
				default:
					this.depalpha = X.Mx(_alpha, this.depalpha);
					return;
				}
			}

			public virtual void release()
			{
				this.alpha = (this.depalpha = 0f);
			}

			public virtual void destruct()
			{
				this.release();
			}

			public abstract void fineAlpha(PostEffect PE, bool refine_alpha = true);

			public virtual void redrawOnlyMesh(PostEffect PE)
			{
			}

			public virtual void fineUiShift(Vector4 VecUiShift)
			{
			}

			public bool need_redraw = true;

			public float alpha;

			public byte restrict_alpha = 1;

			protected float depalpha;

			public readonly PostEffect.PE_VAR var_set;
		}

		public class PESpecial : PostEffect.PEItem
		{
			public PESpecial(PostEffect.PESpecial.FnSpecialExecution _fnExc, PostEffect.PE_VAR _var_set = PostEffect.PE_VAR.MINMAX)
				: base(_var_set)
			{
				this.fnExc = _fnExc;
			}

			public override void release()
			{
				if (this.alpha != 0f)
				{
					base.release();
					this.fnExc(this, M2DBase.Instance as NelM2DBase);
					return;
				}
				base.release();
			}

			public override void fineAlpha(PostEffect PE, bool refine_alpha = true)
			{
				if (!refine_alpha)
				{
					this.depalpha = this.alpha;
				}
				if (this.depalpha == 0f)
				{
					if (this.alpha != 0f)
					{
						this.release();
					}
					this.depalpha = 0f;
					return;
				}
				this.depalpha = X.Mn(this.depalpha, 1f);
				if (this.alpha != this.depalpha)
				{
					if (this.redraw_when_alpha_changing || this.alpha == 0f)
					{
						this.need_redraw = true;
					}
					this.alpha = this.depalpha;
				}
				if (this.need_redraw)
				{
					this.need_redraw = false;
					this.redraw_when_alpha_changing = this.fnExc(this, PE.M2D);
				}
				if (refine_alpha)
				{
					this.depalpha = 0f;
				}
			}

			public readonly PostEffect.PESpecial.FnSpecialExecution fnExc;

			private bool redraw_when_alpha_changing;

			public delegate bool FnSpecialExecution(PostEffect.PESpecial Mtr, NelM2DBase M2D);
		}

		public class PEMaterial : PostEffect.PEItem, ICameraRenderBinder
		{
			public float scale { get; private set; }

			public void shuffleUvPosition(int from_ver_i, PostEffect.PEMaterial.FnShufflePos fnShuffleUv)
			{
				int vertexMax = this.Md.getVertexMax();
				int num = 0;
				Vector2[] uvArray = this.Md.getUvArray();
				Vector3[] vertexArray = this.Md.getVertexArray();
				float num2 = 1f / (IN.w + 16f);
				float num3 = 1f / (IN.h + 16f);
				for (int i = from_ver_i; i < vertexMax; i++)
				{
					Vector3 vector = vertexArray[i];
					uvArray[i] = fnShuffleUv(this, num++, vector.x - this.Md.base_x, vector.y - this.Md.base_y, num2, num3, uvArray[i]);
				}
			}

			public void shuffleVertPosition(int from_ver_i, PostEffect.PEMaterial.FnShufflePos fnShuffleVert)
			{
				int vertexMax = this.Md.getVertexMax();
				int num = 0;
				Vector2[] uvArray = this.Md.getUvArray();
				Vector3[] vertexArray = this.Md.getVertexArray();
				float num2 = (IN.w + 16f) * 0.015625f;
				float num3 = (IN.h + 16f) * 0.015625f;
				for (int i = from_ver_i; i < vertexMax; i++)
				{
					Vector3 vector = vertexArray[i];
					vector = fnShuffleVert(this, num++, vector.x - this.Md.base_x, vector.y - this.Md.base_y, num2, num3, uvArray[i]);
					vector.x += this.Md.base_x;
					vector.y += this.Md.base_y;
					vertexArray[i] = vector;
				}
			}

			public PEMaterial(Material _Mtr, POSTM _type, PostEffect.PEMaterial.FnDrawPostScreenEffect _fnDraw = null, byte _top_flag = 0, PostEffect.PE_VAR _var_set = PostEffect.PE_VAR.MINMAX)
				: base(_var_set)
			{
				this.Mtr = _Mtr;
				this.type = _type;
				this.fnDraw = ((_fnDraw == null) ? new PostEffect.PEMaterial.FnDrawPostScreenEffect(PostEffectItem.drawScreen_default) : _fnDraw);
				this.top_flag = _top_flag;
			}

			public PEMaterial(string shader_name, POSTM _type, PostEffect.PEMaterial.FnDrawPostScreenEffect _fnDraw = null, byte _top_flag = 0, PostEffect.PE_VAR _var_set = PostEffect.PE_VAR.MINMAX)
				: this(MTR.newMtr(shader_name), _type, _fnDraw, _top_flag, _var_set)
			{
				if (this.top_flag > 0)
				{
					this.Mtr.renderQueue += 10;
				}
			}

			public override void releaseMesh()
			{
				if (this.Md != null)
				{
					this.Md = null;
					this.Mrd = null;
					if (this.Cam != null)
					{
						this.Cam.deassignRenderFunc(this, this.layer);
					}
					this.Cam = null;
					this.need_redraw = (this.redraw_when_alpha_changing = false);
				}
			}

			public PostEffect.PEMaterial setMaterialTexture(string key, Sprite S)
			{
				MTRX.setMaterialST(this.Mtr, key, S);
				return this;
			}

			public PostEffect.PEMaterial setMaterialTexture(string key, PxlImage S)
			{
				MTRX.setMaterialST(this.Mtr, key, S, 0f);
				return this;
			}

			public override void release()
			{
				base.release();
				this.hide();
				this.releaseMesh();
			}

			public override void fineUiShift(Vector4 VecUiShift)
			{
				this.Mtr.SetVector("_UiShift", VecUiShift);
			}

			public void hide()
			{
				this.alpha = (this.depalpha = 0f);
				this.Mtr.SetFloat("_Alpha", 0f);
				if (this.Mrd != null)
				{
					this.Mrd.enabled = false;
				}
			}

			public void fineScale()
			{
				this.scale = this.Cam.getScale(true);
				this.Mtr.SetFloat("_Scale", this.scale);
				float num = 1f / this.scale;
				this.Mtr.SetFloat("_Scale_r", num);
				float num2 = (this.adjust_wh_by_scale ? 1f : num);
				this.w = IN.w * num2;
				this.h = IN.h * num2;
				this.wh = IN.wh * num2;
				this.hh = IN.hh * num2;
				this.need_redraw = true;
			}

			public bool adjust_wh_by_scale
			{
				get
				{
					return this.top_flag >= 2;
				}
			}

			public override void fineAlpha(PostEffect PE, bool refine_alpha = true)
			{
				if (!refine_alpha)
				{
					this.depalpha = this.alpha;
				}
				if (this.depalpha <= 0f)
				{
					if (this.alpha != 0f)
					{
						this.hide();
					}
					this.depalpha = 0f;
					return;
				}
				this.depalpha = X.Mn(this.depalpha, 1f);
				if (this.Md == null)
				{
					if (this.Cam == null)
					{
						this.Cam = PE.M2D.Cam;
					}
					this.layer = ((this.top_flag > 0) ? this.Cam.getFinalRenderedLayer() : this.Cam.getFinalSourceRenderedLayer());
					this.createMesh();
					this.alpha = this.depalpha;
					this.fineScale();
				}
				else
				{
					if (this.alpha == 0f)
					{
						M2Camera cam = M2DBase.Instance.Cam;
						if (this.Mrd != null)
						{
							this.Mrd.enabled = true;
						}
						else
						{
							cam.assignRenderFunc(this, this.layer, false, null);
						}
					}
					if (this.Cam.getScale(true) != this.scale)
					{
						this.fineScale();
					}
					if (this.alpha != this.depalpha)
					{
						this.alpha = this.depalpha;
						if (this.redraw_when_alpha_changing)
						{
							this.need_redraw = true;
						}
					}
				}
				if (refine_alpha)
				{
					this.depalpha = 0f;
				}
				if (this.auto_refine >= 1)
				{
					if (this.need_redraw)
					{
						this.auto_refine = 1;
						return;
					}
					this.auto_refine += 1;
					if (this.auto_refine >= 5)
					{
						this.auto_refine = 1;
						this.need_redraw = true;
					}
				}
			}

			public override void redrawOnlyMesh(PostEffect PE)
			{
				if (this.alpha == 0f || this.restrict_alpha == 0)
				{
					return;
				}
				if (this.need_redraw)
				{
					this.need_redraw = false;
					float num = 0f;
					float num2 = 0f;
					this.Md.base_x = -num;
					this.Md.base_y = -num2;
					this.Md.clearSimple();
					this.Md.initForPostEffect(this.Cam.getFinalizedTexture(), 0f, 0f);
					this.redraw_when_alpha_changing = this.fnDraw(this.Md, this, this.Cam);
					this.Md.updateForMeshRenderer(false);
				}
				this.Mtr.SetFloat("_Alpha", this.alpha);
			}

			public virtual void createMesh()
			{
				if (X.DEBUGSTABILIZE_DRAW)
				{
					this.Md = this.Cam.createMeshDrawer(this.Cam.getCameraComponentForLayer(1U << this.layer), FEnum<POSTM>.ToStr(this.type), this.Mtr, this.layer, 0f, true);
					this.Mrd = this.Cam.GetMeshRendererFromCamera(this.Md);
				}
				else
				{
					this.Md = new MeshDrawer(null, 4, 6);
					this.Md.draw_gl_only = true;
					this.Md.activate("PE-" + this.Mtr.shader.name, this.Mtr, this.top_flag == 0, MTRX.ColWhite, null);
				}
				this.Md.base_z = ((this.top_flag > 0) ? (31f - (float)this.top_flag * 0.05f) : 37f) - (float)this.type * 0.001f;
				if (this.Md.draw_gl_only)
				{
					this.Cam.assignRenderFunc(this, this.layer, false, null);
				}
			}

			public bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
			{
				if (this.alpha == 0f || this.restrict_alpha == 0 || this.Md == null)
				{
					return true;
				}
				float num = 1f;
				CameraComponentCollecter cameraComponentCollecter = XCon as CameraComponentCollecter;
				if (cameraComponentCollecter != null)
				{
					if (cameraComponentCollecter.already_mutual_replaced && !this.adjust_wh_by_scale)
					{
						num *= this.scale;
					}
					if (this.top_flag >= 2 && this.top_flag % 2 == 0)
					{
						RenderTexture renderTexture = null;
						RenderTexture renderTexture2 = null;
						cameraComponentCollecter.replaceMutualTexture(ref renderTexture, ref renderTexture2);
						this.Md.setMtrTexture("_MainTex", renderTexture);
						Graphics.SetRenderTarget(renderTexture2);
					}
				}
				M2Camera cam = PostEffect.IT.M2D.Cam;
				Matrix4x4 cameraProjectionTransformed = JCon.CameraProjectionTransformed;
				Vector3 posMainTransform = cam.PosMainTransform;
				BLIT.RenderToGLMtr(this.Md, posMainTransform.x, posMainTransform.y, num, this.Mtr, cameraProjectionTransformed, this.Md.getTriMax(), false, false);
				return true;
			}

			public float getFarLength()
			{
				if (this.Md == null)
				{
					return 1000f;
				}
				return this.Md.base_z;
			}

			public override string ToString()
			{
				if (this._tostring == null)
				{
					STB stb = TX.PopBld(null, 0);
					this._tostring = stb.Add("PE Mtr - ").Add(this.type.ToString()).ToString();
					TX.ReleaseBld(stb);
				}
				return this._tostring;
			}

			public readonly PostEffect.PEMaterial.FnDrawPostScreenEffect fnDraw;

			public byte top_flag;

			public byte auto_refine;

			public float w;

			public float h;

			public float wh;

			public float hh;

			protected Material Mtr;

			protected MeshDrawer Md;

			protected MeshRenderer Mrd;

			private int layer;

			protected M2Camera Cam;

			private POSTM type;

			private bool redraw_when_alpha_changing;

			private string _tostring;

			public delegate bool FnDrawPostScreenEffect(MeshDrawer Md, PostEffect.PEMaterial Mtr, M2Camera Cam);

			public delegate Vector2 FnShufflePos(PostEffect.PEMaterial Mtr, int i, float posx, float posy, float texel_x, float texel_y, Vector2 PreUv);
		}

		public class PEMaterialWithMover : PostEffect.PEMaterial
		{
			public PEMaterialWithMover(string str, POSTM _type, PostEffect.PEMaterial.FnDrawPostScreenEffect _fnDraw = null, byte _top_flag = 0, PostEffect.PE_VAR _var_set = PostEffect.PE_VAR.MINMAX)
				: base(str, _type, _fnDraw, _top_flag, _var_set)
			{
			}

			public override void createMesh()
			{
				base.createMesh();
				this.Md.setMtrTexture("_MoverTex", this.Cam.CamForMover.targetTexture);
				this.Md.setMtrFloat("_ScreenMargin", 8f);
			}
		}

		public class PEInterrupt : PostEffect.PEItem, ICameraRenderBinder
		{
			public PEInterrupt(M2CameraInterrupt _Interrupt, PostEffect.PEInterrupt.FnInterruptExecution _FnExc, PostEffect.PE_VAR _var_set = PostEffect.PE_VAR.MINMAX)
				: base(_var_set)
			{
				this.FnExc = _FnExc;
				this.Interrupt = _Interrupt;
			}

			public override void releaseMesh()
			{
				this.AssignedCam = null;
			}

			public override void destruct()
			{
				base.destruct();
				this.Interrupt.destruct();
			}

			public override void release()
			{
				if (this.AssignedCam != null)
				{
					this.AssignedCam.deassignRenderFunc(this, this.layer);
					this.AssignedCam = null;
				}
				base.release();
			}

			public override void fineAlpha(PostEffect PE, bool refine_alpha = true)
			{
				if (!refine_alpha)
				{
					this.depalpha = this.alpha;
				}
				if (this.depalpha == 0f)
				{
					if (this.alpha != 0f)
					{
						this.release();
					}
					this.depalpha = 0f;
					return;
				}
				this.depalpha = X.Mn(this.depalpha, 1f);
				if (this.alpha != this.depalpha)
				{
					this.need_redraw = true;
					this.alpha = this.depalpha;
				}
				if (this.need_redraw)
				{
					this.need_redraw = false;
					this.FnExc(this, PE.M2D);
					if (this.AssignedCam == null)
					{
						this.AssignedCam = PE.M2D.Cam;
						this.layer = this.AssignedCam.getFinalRenderedLayer();
						this.AssignedCam.assignRenderFunc(this, this.layer, false, null);
					}
				}
				if (refine_alpha)
				{
					this.depalpha = 0f;
				}
			}

			public bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
			{
				if (this.alpha == 0f)
				{
					return true;
				}
				RenderTexture renderTexture = null;
				RenderTexture renderTexture2 = null;
				CameraComponentCollecter cameraComponentCollecter = XCon as CameraComponentCollecter;
				if (cameraComponentCollecter != null)
				{
					cameraComponentCollecter.replaceMutualTexture(ref renderTexture, ref renderTexture2);
					this.Interrupt.RenderImage(renderTexture, renderTexture2);
				}
				return true;
			}

			public float getFarLength()
			{
				return 31f - (float)this.var_set * 0.001f;
			}

			public override string ToString()
			{
				if (this._tostring == null)
				{
					STB stb = TX.PopBld(null, 0);
					this._tostring = stb.Add("PE Interrupt - ").ToString();
					TX.ReleaseBld(stb);
				}
				return this._tostring;
			}

			public readonly M2CameraInterrupt Interrupt;

			protected M2Camera AssignedCam;

			private PostEffect.PEInterrupt.FnInterruptExecution FnExc;

			private int layer;

			private string _tostring;

			public delegate void FnInterruptExecution(PostEffect.PEInterrupt Mtr, NelM2DBase M2D);
		}

		private struct TimeFixEf
		{
			public void Set(EffectItem _Ef, float _factor)
			{
				this.Ef = _Ef;
				this.Anm = null;
				this.s_index = this.Ef.index;
				this.factor = _factor;
				this.count = 0;
			}

			public void Set(IAnimListener _Anm, float _factor)
			{
				this.Ef = null;
				this.Anm = _Anm;
				this.count = 0;
				this.factor = _factor;
			}

			public bool isActive()
			{
				if (Map2d.TS == 1f)
				{
					byte b = this.count + 1;
					this.count = b;
					if (b >= 16)
					{
						return false;
					}
				}
				if (this.Ef != null)
				{
					return this.Ef.index == this.s_index && this.Ef.FnDef != null;
				}
				return this.Anm != null;
			}

			public bool afAdd(float af)
			{
				if (this.Ef != null)
				{
					if (this.Ef.saf > 0f)
					{
						this.Ef.saf = X.Mx(0f, this.Ef.saf - af * this.factor);
					}
					else
					{
						this.Ef.af += af * this.factor;
					}
				}
				if (this.Anm != null)
				{
					try
					{
						if (!this.Anm.updateAnimator(af * this.factor))
						{
							return false;
						}
					}
					catch
					{
						return false;
					}
					return true;
				}
				return true;
			}

			private EffectItem Ef;

			private uint s_index;

			private IAnimListener Anm;

			public float factor;

			public byte count;
		}

		public enum PE_VAR
		{
			MINMAX,
			ADD,
			SCREEN,
			MUL
		}
	}
}
