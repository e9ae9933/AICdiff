using System;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class UIPictCutin : IEventWaitListener
	{
		public UIPictCutin(UIPicture _Con)
		{
			this.Con = _Con;
			this.Base = this.Con.Base;
			this.M2D = this.Base.M2D;
			this.FD_LoadAfterEvImg = new LoadTicketManager.FnLoadProgress(this.LoadAfterEvImg);
		}

		public void initEvent()
		{
			EV.addWaitListener("UICUTIN", this);
		}

		public void newGame()
		{
			this.killBenchCutin();
		}

		public void killBenchCutin()
		{
			if (this.EfBenchHp != null)
			{
				this.EfBenchHp.destruct();
			}
			if (this.EfBenchClt != null)
			{
				this.EfBenchClt.destruct();
			}
			this.EfBenchHp = (this.EfBenchClt = null);
		}

		private EffectNel EF
		{
			get
			{
				return this.Base.getEffect();
			}
		}

		private PR CurPr
		{
			get
			{
				return UIPicture.getPr();
			}
		}

		public string fader_key
		{
			get
			{
				return this.Con.getCurrentFadeKey(true);
			}
		}

		public bool isActive()
		{
			return this.Con.isActive();
		}

		public void applyWetten(UIPictureBase.EMWET wet_pat, string fader_key)
		{
			if (!this.isActive())
			{
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
			this.Base.CutinAssignFader(animateCutin, this.CurPr, fader_key, UIPictureBase.EMSTATE.NORMAL);
			animateCutin.TS_spv = this.Con.TS_animation;
			if (UIBase.uipic_mpf >= 0f)
			{
				IN.PosP2(animateCutin.transform, IN.w * (this.CurPr.EpCon.Cutin.isRightCutinActive() ? (-0.2f) : 0.36f), 0f);
				return;
			}
			IN.PosP2(animateCutin.transform, IN.w * (this.CurPr.EpCon.Cutin.isLeftCutinActive() ? 0.2f : (-0.36f)), 0f);
		}

		private bool fnDrawWettenCutin(AnimateCutin Cti, Map2d Mp, MeshDrawer Md, MeshDrawer MdT, float t, float anim_t, ref bool update_meshdrawer)
		{
			update_meshdrawer = true;
			bool flag = Cti.fader_key == "wetten_milk";
			if (t == 0f)
			{
				Md.setMaterial(MTRX.getMtr(-1), false);
				MdT.setMaterial(MTR.MIiconL.getMtr(BLEND.NORMAL, -1), false);
				if (Cti.TS_spv == 0f)
				{
					Cti.setTimePositionAll((flag ? 1.15f : 1f) + X.XORSPS() * 0.13f);
				}
			}
			Md.clearSimple();
			MdT.clearSimple();
			float num = X.ZSIN(anim_t, 20f);
			float num2 = X.ZLINE(210f - t, 55f);
			if (num2 <= 0f)
			{
				return false;
			}
			if (anim_t < 30f)
			{
				Cti.setMulColor(4278190080U, 0.8f * (1f - num));
			}
			if (num < 1f)
			{
				Cti.setMulColor(uint.MaxValue, 1f - num);
			}
			float num3 = ((anim_t < 46f) ? 0f : (1f - X.ZSIN(anim_t, 46f)));
			if (num2 < 1f)
			{
				Cti.setMulColor(4290690750U, 1f - num2);
				if (Cti.stencil_ref == -1)
				{
					Cti.stencil_ref = 69;
					Md.setMaterial(MTRX.getMtr(BLEND.MASK, Cti.stencil_ref), false);
				}
			}
			float num4 = X.ZSIN2(anim_t, 25f);
			if (anim_t < 35f)
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
					goto IL_0211;
				}
				if (num2 < 1f)
				{
					Md.Col = MTRX.ColTrnsp;
					Md.Scale(1.5f, 1.5f, false);
					goto IL_0211;
				}
				IL_026B:
				i++;
				continue;
				IL_0211:
				Md.TriRectBL(0);
				Md.PosD(-num5 - num6, -IN.hh - num7, null).PosD(num5, IN.hh + 10f, null).PosD(num5 + num6, IN.hh + num7, null)
					.PosD(-num5, -IN.hh - 10f, null);
				goto IL_026B;
			}
			Md.Identity();
			MdT.Identity();
			this.EfpCutinPeeSplash.drawTo(MdT, 0f, 0f, 0f, 0, anim_t, 0f);
			if (flag)
			{
				float num8 = anim_t - 15f;
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

		public void applyLayingEggCutin(bool is_liquid, float is_right_mpf = -1f, bool is_large = false, bool from_egg_remove = false)
		{
			if (!this.isActive())
			{
				return;
			}
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
			float num = is_right_mpf * (is_large ? 0.5f : 1f);
			if (this.Base.event_center_uipic)
			{
				IN.PosP2Abs(animateCutin.transform, num * IN.wh * 0.3f, -IN.hh * 0.68f);
			}
			else
			{
				if (this.Base.cutin_act_count == 1)
				{
					num *= 0.25f;
				}
				IN.PosP2Abs(animateCutin.transform, this.CurPr.M2D.ui_shift_x + num * (IN.w - X.Abs(this.CurPr.M2D.ui_shift_x * 2f)) * UIBase.uipic_mpf * 0.12f, this.suitable_y * (is_large ? 0.05f : 1.4f));
			}
			float num2 = (is_large ? 2.5f : 1.5f);
			animateCutin.transform.localScale = new Vector3(num2, num2, 1f);
			SND.Ui.play("splash_egg", false);
			this.CurPr.VO.playEggRemoveCutin(from_egg_remove);
		}

		private bool fnDrawLayEggCutin(AnimateCutin Cti, Map2d Mp, MeshDrawer Md, MeshDrawer MdT, float t, float anim_t, ref bool update_meshdrawer)
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
				Cti.TS_spv = this.Con.TS_animation;
			}
			uint num = (uint)Cti.xorsp * 16777215U;
			float num2 = X.ZSIN(anim_t, 20f);
			float num3 = X.ZLINE(130f - anim_t, 55f);
			if (num3 <= 0f)
			{
				return false;
			}
			if (Cti.TS_spv > 0f)
			{
				Cti.TS_spv = this.Con.TS_animation * (1f + X.NI(-0.07f, 0.09f, X.RAN(num, 454)) + X.NI(0.63f, 1f, X.RAN(num, 2617)) * (1f - num2));
			}
			float num4 = 1.5f / Cti.transform.localScale.x;
			Md.clear(false, false);
			MdT.clear(false, false);
			float num5 = (UIBase.Instance.event_center_uipic ? 1f : UIBase.uipic_mpf);
			Md.Scale(num5, 1f, false);
			Md.chooseSubMesh(0, false, false);
			Md.TriRectBL(0);
			Md.Col = MTRX.ColTrnsp;
			this.PP_LayEgg.t = X.NI(Cti.restarted ? 0.75f : 0.25f, 1f, num2) * num3;
			this.PP_LayEgg.drawExtendAreaTo(Md, 0f, 0f);
			Md.chooseSubMesh(1, false, false);
			Md.Col = C32.d2c(4290690750U);
			this.PP_LayEgg.drawTo(Md, 0f, 0f, 0f, num3);
			Vector3 vector;
			if (UIPictureBodySpine.getEffectReposition(Cti.getViewer(), Cti.FindBone("vagina_C2"), out vector))
			{
				Vector3 position = Cti.transform.position;
				position.z = 0f;
				vector = new Vector3((vector.x - position.x) * num4 - 0.28125f, (vector.y - position.y) * num4 - 0.125f, vector.z);
				this.EfpOnceLayEggCutinCircle.drawTo(MdT, vector.x * 64f, vector.y * 64f, vector.z, 0, anim_t, 0f);
				this.EfpOnceLayEggCutinDrip.drawTo(MdT, vector.x * 64f, vector.y * 64f, vector.z, 0, anim_t, 0f);
				if (Cti.fader_key == "cuts_layegg_liquid" && anim_t >= 5f)
				{
					if (this.EfpOnceLayEggCutinDripLiquid == null)
					{
						this.EfpOnceLayEggCutinDripLiquid = new EfParticleOnce("ui_layegg_cutin_drip_liquid", EFCON_TYPE.UI);
					}
					this.EfpOnceLayEggCutinDripLiquid.drawTo(MdT, vector.x * 64f + 20f * num5, vector.y * 64f, vector.z, 0, anim_t - 5f, 0f);
				}
			}
			return true;
		}

		private float eff_base_x(float ratio_x, out float shifty)
		{
			float num = IN.w - X.Abs(this.M2D.ui_shift_x);
			float num2 = 0f;
			shifty = 0f;
			if (this.Base.gm_slide_active)
			{
				if (this.M2D.GM.isBenchMenuActive())
				{
					num = 374f;
					num2 = 0f;
					shifty = -120f;
				}
				else
				{
					num2 -= IN.w * 0.4f;
					num = IN.w - 408.00003f;
				}
			}
			else if (CFGSP.uipic_lr != CFGSP.UIPIC_LR.NONE)
			{
				if (!this.Base.draw_letter_box)
				{
					num2 -= IN.wh * (float)X.MPF(CFGSP.uipic_lr == CFGSP.UIPIC_LR.R);
				}
				else
				{
					num2 += UIBase.uiinvisible_default_pos_u * 64f * (float)X.MPF(CFGSP.uipic_lr == CFGSP.UIPIC_LR.R);
				}
				num = X.NI(num, IN.w, 0.66f);
			}
			else
			{
				num2 -= UIBase.uiinvisible_default_pos_u * 64f;
			}
			return num2 + num * ratio_x * 0.5f;
		}

		private bool LoadAfterEvImg(LoadTicketManager.LoadTicket Tk)
		{
			if (this.CurPr == null || !this.CurPr.isBenchState())
			{
				return false;
			}
			if (Tk.name == "initBenchCureHp")
			{
				this.initBenchCureHp(X.NmI(Tk.name2, 0, false, false), false, true);
			}
			if (Tk.name == "initBenchCureHpTorned")
			{
				this.initBenchCureHp(X.NmI(Tk.name2, 0, false, false), true, true);
			}
			if (Tk.name == "initBenchCureCloth")
			{
				this.initBenchCureCloth(X.NmI(Tk.name2, 0, false, false), false, true);
			}
			if (Tk.name == "initBenchCureClothHpLow")
			{
				this.initBenchCureCloth(X.NmI(Tk.name2, 0, false, false), true, true);
			}
			return false;
		}

		public void initBenchCureHp(int delay, bool torned, bool force = false)
		{
			PR curPr = this.CurPr;
			if (!force && (curPr.hp_ratio > 0.66f || !this.isActive()))
			{
				return;
			}
			if (this.EfBenchHp != null)
			{
				this.EfBenchHp.destruct();
			}
			EvImg pic = EV.Pics.getPic("benchcure/hp_cure_0", true, true);
			if (pic == null)
			{
				return;
			}
			if (!pic.PF.pChar.isLoadCompleted() || pic.PF.TxUsing == null)
			{
				EV.getLoaderForPxl(pic.PF.pChar).preparePxlChar(true, true);
				LoadTicketManager.AddTicket(torned ? "initBenchCureHpTorned" : "initBenchCureHp", delay.ToString(), this.FD_LoadAfterEvImg, null, 100);
				return;
			}
			if (this.FD_EfDrawBenchCureHp == null)
			{
				this.PP_BenchHp = new PopPolyDrawer("4|-129|-130|-138|29|134|44|141|-116,0|2|0|0,0|0,156,15,2,45", 4);
				this.FD_EfDrawBenchCureHp = delegate(EffectItem E)
				{
					if (!this.EfDrawBenchCure(E))
					{
						if (E == this.EfBenchHp)
						{
							this.EfBenchHp = null;
						}
						return false;
					}
					return true;
				};
			}
			this.EfBenchHp = this.EF.setEffectWithSpecificFn("cure_hp", -0.55f, 75f, 0f, torned ? 1 : 0, delay, new FnEffectRun(this.EfDrawBenchCure));
		}

		public void initBenchCureCloth(int delay, bool hp_low, bool force = false)
		{
			if (X.SENSITIVE)
			{
				return;
			}
			PR curPr = this.CurPr;
			if (!force && (!curPr.BetoMng.is_torned || !this.isActive()))
			{
				return;
			}
			if (this.EfBenchClt != null)
			{
				this.EfBenchClt.destruct();
			}
			EvImg pic = EV.Pics.getPic("benchcure/cloth_cure_0", true, true);
			if (pic == null)
			{
				return;
			}
			if (!pic.PF.pChar.isLoadCompleted() || pic.PF.TxUsing == null)
			{
				EV.getLoaderForPxl(pic.PF.pChar).preparePxlChar(true, true);
				LoadTicketManager.AddTicket(hp_low ? "initBenchCureClothHpLow" : "initBenchCureCloth", delay.ToString(), this.FD_LoadAfterEvImg, null, 100);
				return;
			}
			if (this.FD_EfDrawBenchCureClt == null)
			{
				this.PP_BenchCloth = new PopPolyDrawer("4|-117|-113|-109|27|125|20|117|-127,0|2|0|0,0|0,156,15,2,45", 4);
				this.FD_EfDrawBenchCureClt = delegate(EffectItem E)
				{
					if (!this.EfDrawBenchCure(E))
					{
						if (E == this.EfBenchClt)
						{
							this.EfBenchClt = null;
						}
						return false;
					}
					return true;
				};
			}
			this.EfBenchClt = this.EF.setEffectWithSpecificFn("cure_cloth", 0.57f, -2f, 0f, hp_low ? 3 : 2, delay, this.FD_EfDrawBenchCureClt);
		}

		private bool EfDrawBenchCure(EffectItem E)
		{
			if (this.Base.gm_slide_active && !this.Base.gmb_slide_active)
			{
				return false;
			}
			EvImg evImg = null;
			PopPolyDrawer popPolyDrawer = null;
			PR curPr = this.CurPr;
			if (E.af >= 140f)
			{
				return false;
			}
			float num = X.ZLINE(140f - E.af, 30f);
			float num2 = -50f * (1f - X.ZSIN(E.af, 20f)) + X.ZPOW(E.af - 116f, 24f) * 105f;
			bool flag = false;
			if (E.time == 0 || E.time == 1)
			{
				flag = X.sensitive_level == 0 && E.time == 1;
				evImg = EV.Pics.getPic(flag ? "benchcure/hp_cure_1" : "benchcure/hp_cure_0", true, true);
				popPolyDrawer = this.PP_BenchHp;
			}
			else if (E.time == 2 || E.time == 3)
			{
				if (X.sensitive_level > 0)
				{
					return false;
				}
				flag = E.time == 3;
				evImg = EV.Pics.getPic(flag ? "benchcure/cloth_cure_1" : "benchcure/cloth_cure_0", true, true);
				popPolyDrawer = this.PP_BenchCloth;
			}
			if (evImg == null)
			{
				return false;
			}
			int num3 = 25 + E.time;
			MeshDrawer mesh = E.GetMesh("", MTRX.getMtr(BLEND.MASK, num3), false);
			MeshDrawer mesh2 = E.GetMesh("", evImg.MI.getMtr(BLEND.NORMALP3, num3), false);
			uint ran = X.GETRAN2(E.index + 33U, E.index & 63U);
			float num4 = 3.7f * X.COSI(X.RAN(ran, 2711) * 40f + (float)IN.totalframe, 110f + X.RAN(ran, 1973) * 300f);
			float num5 = 3.7f * X.COSI(X.RAN(ran, 1431) * 60f + 30f + (float)IN.totalframe, 110f + X.RAN(ran, 1740) * 300f);
			float num6;
			mesh.base_x = (mesh2.base_x = this.eff_base_x(E.x, out num6) * 0.015625f);
			mesh.base_y = (mesh2.base_y = (num2 + E.y + num6) * 0.015625f);
			float num7 = X.ZSINV(E.af, 16f) * 145f - 55f * X.ZLINE(E.af - 16f, 4f) + 10f * X.ZSIN(E.af - 20f, 25f);
			float num8 = X.ZPOW(E.af, 22f) * 120f - 25f * X.ZLINE(E.af - 22f, 13f) + 5f * X.ZSIN(E.af - 35f, 30f);
			popPolyDrawer.t = popPolyDrawer.anim_maxt;
			num8 *= 0.015f;
			num7 *= 0.015f;
			mesh.Scale(num8, num7, false);
			mesh.Col = MTRX.ColTrnsp;
			popPolyDrawer.drawExtendAreaTo(mesh, 0f, 0f);
			mesh.Col = mesh.ColGrd.Set(flag ? 4285098345U : 4289177511U).C;
			popPolyDrawer.drawTo(mesh, num4, num5, 0f, num);
			mesh.Col = mesh.ColGrd.multiply(0.44f, false).C;
			popPolyDrawer.drawTo(mesh, num4, num5, 1f + 8f * X.ZSIN(E.af, 20f), num);
			mesh2.Col = C32.WMulA(num);
			float num9 = 3.7f * X.COSI(X.RAN(ran, 560) * 40f + (float)IN.totalframe, 110f + X.RAN(ran, 2765) * 220f);
			float num10 = 3.7f * X.COSI(X.RAN(ran, 1034) * 60f + 30f + (float)IN.totalframe, 110f + X.RAN(ran, 1251) * 220f);
			mesh2.allocUv23(4, false);
			mesh2.Uv23(mesh2.ColGrd.White().blend(0U, X.ZSIN(E.af - 5f, 35f)).C, false);
			mesh2.RotaPF(num9, num10 + X.Mn(num2, 0f) * 1.54f, 1f, 1f, 0f, evImg.PF, false, false, false, uint.MaxValue, false, 0);
			mesh2.allocUv23(0, true);
			return true;
		}

		public bool EvtWait(bool is_first = false)
		{
			if (is_first)
			{
				return true;
			}
			bool flag = false;
			if (this.EfBenchClt != null)
			{
				flag = flag || this.EfBenchClt.af < 98f;
			}
			if (this.EfBenchHp != null)
			{
				flag = flag || this.EfBenchHp.af < 98f;
			}
			return flag;
		}

		public float suitable_y
		{
			get
			{
				M2Camera cam = this.CurPr.M2D.Cam;
				return (float)X.MPF(-(this.CurPr.drawy - cam.y) * cam.getScaleRev() < cam.get_h() * 0.18f) * IN.hh * 0.4f;
			}
		}

		public readonly UIPicture Con;

		public readonly UIBase Base;

		public const float cutin_hp_ratio = 0.66f;

		public readonly NelM2DBase M2D;

		private LoadTicketManager.FnLoadProgress FD_LoadAfterEvImg;

		private AnimateCutin.FnDrawAC FD_fnDrawWettenCutin;

		private AnimateCutin.FnDrawAC FD_fnDrawLayEggCutin;

		private EfParticleOnce EfpCutinPeeSplash;

		private EfParticleOnce EfpCutinSplashMilkL;

		private EfParticleOnce EfpCutinSplashMilkR;

		private PopPolyDrawer PP_LayEgg;

		private EfParticleOnce EfpOnceLayEggCutinCircle;

		private EfParticleOnce EfpOnceLayEggCutinDripLiquid;

		private EfParticleOnce EfpOnceLayEggCutinDrip;

		private EffectItem EfBenchHp;

		private FnEffectRun FD_EfDrawBenchCureHp;

		private PopPolyDrawer PP_BenchHp;

		private EffectItem EfBenchClt;

		private FnEffectRun FD_EfDrawBenchCureClt;

		private PopPolyDrawer PP_BenchCloth;

		private const string benchcure_hp_evimg = "benchcure/hp_cure_0";

		private const string benchcure_hp_evimg_torned = "benchcure/hp_cure_1";

		private const string benchcure_cloth_evimg = "benchcure/cloth_cure_0";

		private const string benchcure_cloth_evimg_hplow = "benchcure/cloth_cure_1";

		private const float benchcure_maxt = 140f;
	}
}
