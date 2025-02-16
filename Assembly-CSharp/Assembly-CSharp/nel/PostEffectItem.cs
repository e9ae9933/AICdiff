using System;
using Kayac;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class PostEffectItem : EffectItem
	{
		public PostEffectItem(PostEffect _PE)
			: base(_PE, "pe", 0f, 0f, 0f, 0, 0)
		{
		}

		public override void clear(IEffectSetter _EF, string _title = "", float _x = 0f, float _y = 0f, float _z = 0f, int _time = 0, int _saf = 0)
		{
			base.clear(_EF, _title, _x, _y, _z, _time, _saf);
			this.do_not_consider_timeout = false;
			this.timeout = 240;
		}

		public override EffectItem initEffect(string type_name = "")
		{
			this.timeout = 240;
			return base.initEffect(null);
		}

		public void fine(int timeout_t = 120)
		{
			if (this.timeout < timeout_t)
			{
				this.timeout = timeout_t;
			}
		}

		public override bool run(float fcnt = 1f)
		{
			if (this.saf > 0f)
			{
				this.saf = X.Mx(0f, this.saf - fcnt);
				if (this.saf == 0f && base.FnDef == null)
				{
					this.initEffect("");
				}
			}
			else
			{
				if (!this.do_not_consider_timeout && fcnt > 0f)
				{
					int num = this.timeout - 1;
					this.timeout = num;
					if (num <= 0)
					{
						this.deactivate(false);
					}
				}
				if (base.FnDef == null || !base.FnDef(this))
				{
					PostEffectItem.PE.setAlpha((POSTM)this.time, 0f);
					return false;
				}
			}
			return true;
		}

		public PostEffectItem deactivate(bool immediate = false)
		{
			this.do_not_consider_timeout = false;
			if (immediate)
			{
				base.FnDef = null;
			}
			else if (base.FnDef == PostEffectItem.FD_fnRunDraw_pe_basic)
			{
				if (this.z >= 0f)
				{
					this.z = X.Mn(-1f, -this.z);
				}
			}
			else if ((base.FnDef == PostEffectItem.FD_fnRunDraw_pe_fadeinout || base.FnDef == PostEffectItem.FD_fnRunDraw_pe_fadeinout_zsinv) && this.z < 0f)
			{
				this.y = X.Mn(this.y, 150f);
				this.af = this.y;
				this.z = 0f;
			}
			else
			{
				base.FnDef = null;
			}
			this.do_not_consider_timeout = false;
			return null;
		}

		public static bool executeTsSlow(PostEffect.PESpecial Mtr, NelM2DBase M2D)
		{
			((PostEffectItem.PE != null) ? PostEffectItem.PE : PostEffect.IT).setMap2dTimeScale(1f - Mtr.alpha);
			Mtr.need_redraw = true;
			return true;
		}

		public static bool executeCamZoom2(PostEffect.PESpecial Mtr, NelM2DBase M2D)
		{
			PostEffect postEffect = ((PostEffectItem.PE != null) ? PostEffectItem.PE : PostEffect.IT);
			Mtr.need_redraw = true;
			postEffect.addMapCamZoom(Mtr.alpha);
			return true;
		}

		public static bool executeCamComfuse(PostEffect.PESpecial Mtr, NelM2DBase M2D)
		{
			PostEffect postEffect = ((PostEffectItem.PE != null) ? PostEffectItem.PE : PostEffect.IT);
			Mtr.need_redraw = true;
			postEffect.SetEffectConfusion(Mtr.alpha);
			return true;
		}

		public static bool executeDebugVariable(PostEffect.PESpecial Mtr, NelM2DBase M2D)
		{
			PostEffect postEffect = ((PostEffectItem.PE != null) ? PostEffectItem.PE : PostEffect.IT);
			Mtr.need_redraw = true;
			postEffect.setDebugVariable(Mtr.alpha);
			return true;
		}

		public static bool executeBgmLower(PostEffect.PESpecial Mtr, NelM2DBase M2D)
		{
			PostEffect postEffect = ((PostEffectItem.PE != null) ? PostEffectItem.PE : PostEffect.IT);
			Mtr.need_redraw = true;
			postEffect.SetBgmLowerEffect(Mtr.alpha);
			return true;
		}

		public static bool executeBgmWater(PostEffect.PESpecial Mtr, NelM2DBase M2D)
		{
			PostEffect postEffect = ((PostEffectItem.PE != null) ? PostEffectItem.PE : PostEffect.IT);
			Mtr.need_redraw = true;
			postEffect.SetBgmWaterEffect(Mtr.alpha);
			return true;
		}

		public static bool executeM2dVar0(PostEffect.PESpecial Mtr, NelM2DBase M2D)
		{
			PostEffect postEffect = ((PostEffectItem.PE != null) ? PostEffectItem.PE : PostEffect.IT);
			Mtr.need_redraw = true;
			postEffect.setM2dVar0(Mtr.alpha);
			return true;
		}

		public static bool executeHeartBeat(PostEffect.PESpecial Mtr, NelM2DBase M2D)
		{
			if (PostEffectItem.PE == null)
			{
				PostEffect it = PostEffect.IT;
			}
			else
			{
				PostEffect pe = PostEffectItem.PE;
			}
			Mtr.need_redraw = true;
			int num = (int)X.NI(140, 60, Mtr.alpha);
			if (M2D.curMap != null && M2D.curMap.floort - PostEffectItem.t_heartbeat >= (float)num)
			{
				PostEffectItem.t_heartbeat = M2D.curMap.floort;
				SND.Ui.play("heartbeat", false);
				NEL.PadVib("heartbeat", 1f);
			}
			return true;
		}

		public static bool executeVolumeReduce(PostEffect.PESpecial Mtr, NelM2DBase M2D)
		{
			PostEffect postEffect = ((PostEffectItem.PE != null) ? PostEffectItem.PE : PostEffect.IT);
			Mtr.need_redraw = true;
			postEffect.setVolumeReduce(Mtr.alpha);
			return true;
		}

		public static bool executeRain(PostEffect.PESpecial Mtr, NelM2DBase M2D)
		{
			if (PostEffectItem.PE == null)
			{
				PostEffect it = PostEffect.IT;
			}
			else
			{
				PostEffect pe = PostEffectItem.PE;
			}
			M2D.rain_level = Mtr.alpha;
			return true;
		}

		public static void executePostBloom(PostEffect.PEInterrupt Mtr, NelM2DBase M2D)
		{
			if (PostEffectItem.PE == null)
			{
				PostEffect it = PostEffect.IT;
			}
			else
			{
				PostEffect pe = PostEffectItem.PE;
			}
			LightPostProcessor lightPostProcessor = Mtr.Interrupt as LightPostProcessor;
			lightPostProcessor.BloomStrength = Mtr.alpha;
			lightPostProcessor.maxBloomLevelCount = X.IntR(X.NI(4, 7, Mtr.alpha));
		}

		public static bool executeCamFinalAlpha(PostEffect.PESpecial Mtr, NelM2DBase M2D)
		{
			PostEffect postEffect = ((PostEffectItem.PE != null) ? PostEffectItem.PE : PostEffect.IT);
			Mtr.need_redraw = true;
			byte b = (byte)(255f - Mtr.alpha * 255f);
			postEffect.SetFinalRenderAlpha(b);
			return true;
		}

		public static bool drawScreen_default(MeshDrawer Md, PostEffect.PEMaterial Mtr, M2Camera Cam)
		{
			Md.Rect(0f, 0f, Mtr.w, Mtr.h, false);
			return false;
		}

		public static bool drawScreen_withAlpha(MeshDrawer Md, PostEffect.PEMaterial Mtr, M2Camera Cam)
		{
			Md.Col = Md.ColGrd.Set(uint.MaxValue).mulA(X.ZLINE(Mtr.alpha)).C;
			Md.Rect(0f, 0f, Mtr.w, Mtr.h, false);
			return true;
		}

		public static bool drawScreen_glitch(MeshDrawer Md, PostEffect.PEMaterial Mtr, M2Camera Cam)
		{
			uint ran = X.GETRAN2(0, 1);
			Md.allocUv2(4, false).allocUv3(4, false);
			Md.Uv2(256f / (24f + 8f * X.RAN(ran, 550)), 256f / (4f + 7f * X.RAN(ran, 1592)), false);
			Md.Uv3(256f / (200f + 40f * X.RAN(ran, 2705)), 256f / (165f + 30f * X.RAN(ran, 2964)), false);
			Md.Rect(0f, 0f, Mtr.w, Mtr.h, false);
			Md.allocUv2(0, true).allocUv3(0, true);
			return false;
		}

		public static bool drawScreen_hpreduce(MeshDrawer Md, PostEffect.PEMaterial Mtr, M2Camera Cam)
		{
			Md.Col = Md.ColGrd.Set(4278190080U).mulA(X.ZSIN(Mtr.alpha)).C;
			Vector2 posMainMoverShift = PostEffect.IT.PosMainMoverShift;
			Md.ColGrd.Set(0);
			float num = X.ZSIN(Mtr.alpha);
			float num2 = 1.2f;
			float num3 = 1.4f;
			Md.InnerCircle(0f, 0f, Mtr.w * num2, Mtr.h * num3, posMainMoverShift.x, posMainMoverShift.y, Mtr.wh * X.NI(num2, 1.1f, Mtr.alpha), Mtr.hh * X.NI(num3, 1.3f, Mtr.alpha), Mtr.wh * X.NI(num2, 0.56f, num), Mtr.hh * X.NI(num3, 0.9f, num), false, false, 0f, 1f);
			return true;
		}

		public static bool drawScreen_mpreduce(MeshDrawer Md, PostEffect.PEMaterial Mtr, M2Camera Cam)
		{
			Md.Col = Md.ColGrd.Set(4278190080U).mulA(X.ZSIN(Mtr.alpha)).C;
			Md.ColGrd.Set(0);
			Vector2 posMainMoverShift = PostEffect.IT.PosMainMoverShift;
			int vertexMax = Md.getVertexMax();
			float num = X.ZSIN(Mtr.alpha);
			float num2 = 1.2f;
			float num3 = 1.4f;
			Md.InnerPoly(0f, 0f, Mtr.w * num2, Mtr.h * num3, posMainMoverShift.x, posMainMoverShift.y, Mtr.wh * X.NI(num2, 1.1f, Mtr.alpha), Mtr.hh * X.NI(num3, 1.3f, Mtr.alpha), Mtr.wh * X.NI(num2, 0.4f, num), Mtr.hh * X.NI(num3, 0.7f, num), 32, 0f, false, false, 0f, 1f);
			Mtr.shuffleVertPosition(vertexMax + 4, PostEffectItem.FD_fnShuffleVert32);
			Mtr.shuffleUvPosition(vertexMax, PostEffectItem.FD_fnShuffleUv);
			Mtr.need_redraw = true;
			return true;
		}

		private static Vector2 fnShuffleUv(PostEffect.PEMaterial Mtr, int i, float posx, float posy, float texel_x, float texel_y, Vector2 PreUv)
		{
			posx *= 64f;
			posy *= 64f;
			float num = ((i < 4) ? 0.25f : 1f);
			PreUv.x += X.COSI((float)IN.totalframe + 50f * X.COSI(posx, 80f) + 50f * X.COSI(posy, 170f), 340f + 40f * X.COSI(posx, 140f) + 30f * X.COSI(posy, 210f)) * 3f * texel_x * num;
			PreUv.y += X.COSI((float)(IN.totalframe + 90) + 50f * X.COSI(posx, 70f) + 50f * X.COSI(posy, 110f), 490f + 50f * X.COSI(posx, 231f) + 30f * X.COSI(posy, 98f)) * 2f * texel_y * num;
			return PreUv;
		}

		private static Vector2 fnShuffleVert32(PostEffect.PEMaterial Mtr, int i, float posx, float posy, float texel_x, float texel_y, Vector2 PreUv)
		{
			int num = i / 2;
			uint ran = X.GETRAN2(num * 13, num + 7);
			float num2 = (float)IN.totalframe;
			float num3 = 0.3f + 0.1f * X.SINI(num2 + X.RAN(ran, 1353) * 420f, 400f + 90f * X.RAN(ran, 1221) + 110f * X.RAN(ran, 2667)) * (float)X.MPF(num % 2 == 1);
			float num4 = X.Sin((200f * X.RAN(ran, 2066) + 270f * X.SINI(num2 + X.RAN(ran, 1892) * 300f, 380f + 60f * X.RAN(ran, 663)) + 70f * X.SINI(num2 + X.RAN(ran, 1965) * 220f, 270f + 70f * X.RAN(ran, 663))) / 360f * 6.2831855f);
			num4 = num4 * num4 * (float)X.MPF(num4 > 0f);
			if (i % 2 == 0)
			{
				num4 = (num4 + 1f) * 0.5f * num3 * Mtr.alpha;
			}
			else
			{
				num4 *= 0.7f * Mtr.alpha;
			}
			posx *= 1f + 0.1f * num4;
			posy *= 1f + 0.07f * num4;
			return new Vector2(posx, posy);
		}

		public static bool drawScreen_mpabsorbed(MeshDrawer Md, PostEffect.PEMaterial Mtr, M2Camera Cam)
		{
			Md.Col = Md.ColGrd.Set(4278190080U).mulA(X.ZSIN(Mtr.alpha)).C;
			Md.ColGrd.Set(0);
			int vertexMax = Md.getVertexMax();
			float num = X.ZSIN(Mtr.alpha);
			float num2 = 1.2f;
			float num3 = 1.4f;
			Md.InnerPoly(0f, 0f, Mtr.w * num2, Mtr.h * num3, 0f, 0f, Mtr.wh * X.NI(num2, 1.1f, Mtr.alpha), Mtr.hh * X.NI(num3, 1.3f, Mtr.alpha), Mtr.wh * X.NI(0.8f, 0.75f, num), Mtr.hh * X.NI(0.95f, 0.9f, num), 24, 0f, false, false, 0f, 1f);
			Mtr.shuffleVertPosition(vertexMax + 4, PostEffectItem.FD_fnShuffleVert24);
			Mtr.need_redraw = true;
			return true;
		}

		private static Vector2 fnShuffleVert24(PostEffect.PEMaterial Mtr, int i, float posx, float posy, float texel_x, float texel_y, Vector2 PreUv)
		{
			int num = i / 2;
			uint ran = X.GETRAN2(num * 13, num + 7);
			float num2 = (float)IN.totalframe;
			float num3 = 0.3f + 0.1f * X.SINI(num2 + X.RAN(ran, 1353) * 420f, 400f + 90f * X.RAN(ran, 1221) + 110f * X.RAN(ran, 2667));
			float num4 = X.Sin((200f * X.RAN(ran, 2066) + 270f * X.SINI(num2 + X.RAN(ran, 1892) * 300f, 380f + 60f * X.RAN(ran, 663)) + 70f * X.SINI(num2 + X.RAN(ran, 1965) * 220f, 270f + 70f * X.RAN(ran, 663))) / 360f * 6.2831855f);
			num4 = num4 * num4 * (float)X.MPF(num4 > 0f);
			float num5 = 0.95f + 0.05f * Mtr.alpha;
			if (i % 2 == 0)
			{
				num4 = (num4 + 1f) * 0.5f * num3 * num5;
			}
			else
			{
				num4 = (num4 + 1f) * num3 * num5 * ((num % 2 == 1) ? 3.5f : 2.1f);
			}
			posx *= 1f + 0.1f * num4;
			posy *= 1f + 0.07f * num4;
			return new Vector2(posx, posy);
		}

		public static bool drawScreen_wormtrapped(MeshDrawer Md, PostEffect.PEMaterial Mtr, M2Camera Cam)
		{
			if (Mtr.alpha >= 1f)
			{
				Md.Col = MTRX.ColBlack;
				PostEffectItem.drawScreen_default(Md, Mtr, Cam);
				return true;
			}
			Vector2 posMainMoverShift = PostEffect.IT.PosMainMoverShift;
			Md.Col = Md.ColGrd.Set(4278190080U).mulA(X.ZSIN(Mtr.alpha)).C;
			float num = X.ZPOW(Mtr.alpha - 0.5f, 0.5f);
			Md.ColGrd.Set(4278190080U).mulA(num);
			float num2 = X.ZSIN(Mtr.alpha, 0.5f);
			float num3 = num2 * num2;
			float num4 = 1.2f;
			float num5 = 1.7f;
			Md.InnerCircle(0f, 0f, Mtr.w * num4, Mtr.h * num5, posMainMoverShift.x, posMainMoverShift.y, Mtr.wh * X.NI(num4, 0.6f, Mtr.alpha), Mtr.hh * X.NI(num5, 0.8f, Mtr.alpha), Mtr.wh * X.NI(num4 * 0.5f, 0f, num3), Mtr.hh * X.NI(num5 * 0.5f, 0f, num3), true, false, 0f, 1f);
			return true;
		}

		public static bool drawScreen_gas_applied(MeshDrawer Md, PostEffect.PEMaterial Mtr, M2Camera Cam)
		{
			Md.Col = Md.ColGrd.Set(16777215).blend(4292796126U, X.ZLINE(Mtr.alpha, 0.5f)).C;
			Md.ColGrd.Set(16777215).blend(1439485132U, X.ZLINE(Mtr.alpha - 0.5f, 0.5f));
			Vector2 posMainMoverShift = PostEffect.IT.PosMainMoverShift;
			float num = X.ZSIN(Mtr.alpha, 0.5f);
			float num2 = num * num;
			float num3 = 1.4f;
			float num4 = 1.5f;
			int num5 = Md.getVertexMax() + 4;
			Md.Uv2(0f, 0f, true).Uv2(0f, 0f, false).Uv2(0f, 0f, false)
				.Uv2(0f, 0f, false);
			Md.InnerCircle(0f, 0f, Mtr.w * num3, Mtr.h * num4, posMainMoverShift.x, posMainMoverShift.y, Mtr.wh * X.NI(num3, 1.1f, Mtr.alpha), Mtr.hh * X.NI(num4, 1.2f, Mtr.alpha), Mtr.wh * X.NI(num3 * 0.9f, 0f, num2), Mtr.hh * X.NI(num4 * 1f, 0f, num2), Mtr.alpha > 0.5f, false, 0f, 1f);
			float num6 = 40f;
			float num7 = num6 / IN.w;
			float num8 = num6 / IN.h;
			for (int i = Md.getVertexMax() - 1; i >= num5; i--)
			{
				Md.Uv2(num7, num8, false);
			}
			return true;
		}

		public static bool drawScreen_irisout(MeshDrawer Md, PostEffect.PEMaterial Mtr, M2Camera Cam)
		{
			if (Mtr.alpha == 0f)
			{
				return true;
			}
			Md.Col = C32.d2c(4282004532U);
			if (Mtr.alpha >= 1f)
			{
				Md.Rect(0f, 0f, Mtr.w * 1.25f, Mtr.h * 1.25f, false);
			}
			else
			{
				float num = Mtr.hh * 1.5f;
				Vector2 posMainMoverShift = PostEffect.IT.PosMainMoverShift;
				num += X.LENGTHXYS(0f, 0f, posMainMoverShift.x, posMainMoverShift.y) * 0.8f;
				num *= 1f - Mtr.alpha;
				Md.InnerPoly(0f, 0f, Mtr.w * 1.25f, Mtr.h * 1.25f, posMainMoverShift.x, posMainMoverShift.y, num, num, num * 4f, num * 4f, 24, 0f, false, false, 0f, 0f);
			}
			Md.Identity();
			return true;
		}

		public static bool drawScreen_go_close_eye(MeshDrawer Md, PostEffect.PEMaterial Mtr, M2Camera Cam)
		{
			if (Mtr.alpha <= 0f)
			{
				return true;
			}
			Md.Col = Md.ColGrd.Set(4282004532U).setA1(X.ZLINE(Mtr.alpha, 0.25f)).C;
			if (PostEffectItem.ClsDr == null)
			{
				PostEffectItem.ClsDr = new ClosingEyeDrawer();
				PostEffectItem.ClsDr.bounds_hh = IN.hh * 2.5f * 0.015625f;
			}
			PostEffectItem.ClsDr.drawTo(Md, 0f, 0f, IN.w * 0.7f * Cam.getScale(true), IN.h * 0.7f * Cam.getScale(true), Mtr.alpha);
			return true;
		}

		public static bool drawScreen_wholeripple(MeshDrawer Md, PostEffect.PEMaterial Mtr, M2Camera Cam)
		{
			if (Mtr.alpha <= 0f)
			{
				return true;
			}
			Md.Col = Md.ColGrd.White().setA1(X.ZLINE(Mtr.alpha, 0.5f)).C;
			Md.allocUv23(4, false);
			Md.Uv2(11f * Mtr.alpha, 2f * Mtr.alpha, false);
			Md.Uv3(1.9f, 3.4f, false);
			Md.Rect(0f, 0f, Mtr.w, Mtr.h, false);
			Md.allocUv23(0, true);
			return true;
		}

		public static bool fnRunDraw_pe_debug(EffectItem E)
		{
			PostEffectItem.PE.setAlpha((POSTM)E.time, 1f);
			return true;
		}

		public static bool fnRunDraw_pe_basic(EffectItem E)
		{
			if (PostEffectItem.PE.isRestricted((POSTM)E.time) && E.z >= 0f)
			{
				E.z = -30f;
			}
			if (E.z >= 0f)
			{
				E.af += 1f;
				PostEffectItem.PE.setAlpha((POSTM)E.time, E.x * X.ZLINE(E.af, E.z));
			}
			else
			{
				if (E.af >= 0f)
				{
					E.af = X.Mn(E.z + E.af, 0f);
				}
				E.af -= 1f;
				if (E.af <= E.z)
				{
					return false;
				}
				PostEffectItem.PE.setAlpha((POSTM)E.time, E.x * X.ZLINE(-E.z + E.af, -E.z));
			}
			return true;
		}

		public static bool fnRunDraw_pe_once(EffectItem E)
		{
			if (E.z <= 0f)
			{
				return false;
			}
			if (E.af < E.z)
			{
				PostEffectItem.PE.setAlpha((POSTM)E.time, E.x);
				return true;
			}
			return false;
		}

		public static bool fnRunDraw_pe_bounce(EffectItem E)
		{
			if (E.z <= 0f)
			{
				return false;
			}
			if (E.af < E.z)
			{
				PostEffectItem.PE.setAlpha((POSTM)E.time, E.x * (1f - X.ZPOW(E.z - E.af, E.z)));
			}
			else
			{
				if (PostEffectItem.PE.isRestricted((POSTM)E.time))
				{
					return false;
				}
				float num = E.af - E.z;
				if (num >= E.z)
				{
					return false;
				}
				PostEffectItem.PE.setAlpha((POSTM)E.time, E.x * (1f - X.ZPOW(num, E.z)));
			}
			E.af += 1f;
			return true;
		}

		public static bool fnRunDraw_pe_absorbed(EffectItem E)
		{
			(E as PostEffectItem).timeout = 240;
			if (E.af < E.y)
			{
				PostEffectItem.PE.setAlpha((POSTM)E.time, E.x * (1f - X.ZPOW(E.z - E.af, E.y)));
			}
			else
			{
				if (PostEffectItem.PE.isRestricted((POSTM)E.time))
				{
					return false;
				}
				float num = E.af - E.y;
				if (num >= E.z)
				{
					return false;
				}
				PostEffectItem.PE.setAlpha((POSTM)E.time, E.x * (1f - X.ZSIN(num, E.z)));
			}
			E.af += 1f;
			return true;
		}

		public static bool fnRunDraw_pe_fadeinout(EffectItem E)
		{
			if (E.z >= 0f)
			{
				(E as PostEffectItem).timeout = 240;
			}
			if (E.af < E.y)
			{
				PostEffectItem.PE.setAlpha((POSTM)E.time, E.x * X.ZLINE(E.af, E.y));
			}
			else
			{
				float num = E.af - E.y;
				if (num < E.z || E.z < 0f)
				{
					PostEffectItem.PE.setAlpha((POSTM)E.time, E.x);
					if (E.z < 0f)
					{
						return true;
					}
				}
				else
				{
					if (PostEffectItem.PE.isRestricted((POSTM)E.time))
					{
						return false;
					}
					num -= E.z;
					if (num >= E.y)
					{
						return false;
					}
					PostEffectItem.PE.setAlpha((POSTM)E.time, E.x * (1f - X.ZLINE(num, E.y)));
				}
			}
			E.af += 1f;
			return true;
		}

		public static bool fnRunDraw_pe_fadeinout_zsinv(EffectItem E)
		{
			if (E.z >= 0f)
			{
				(E as PostEffectItem).timeout = 240;
			}
			if (E.af < E.y)
			{
				PostEffectItem.PE.setAlpha((POSTM)E.time, E.x * X.ZSINV(E.af, E.y));
			}
			else
			{
				float num = E.af - E.y;
				if (num < E.z || E.z < 0f)
				{
					PostEffectItem.PE.setAlpha((POSTM)E.time, E.x);
					if (E.z < 0f)
					{
						return true;
					}
				}
				else
				{
					if (PostEffectItem.PE.isRestricted((POSTM)E.time))
					{
						return false;
					}
					num -= E.z;
					if (num >= E.y)
					{
						return false;
					}
					PostEffectItem.PE.setAlpha((POSTM)E.time, E.x * (1f - X.ZSINV(num, E.y)));
				}
			}
			E.af += 1f;
			return true;
		}

		public static PostEffect PE;

		public int timeout;

		public bool do_not_consider_timeout;

		private static ClosingEyeDrawer ClsDr;

		public static FnEffectRun FD_fnRunDraw_pe_basic = new FnEffectRun(PostEffectItem.fnRunDraw_pe_basic);

		public static FnEffectRun FD_fnRunDraw_pe_once = new FnEffectRun(PostEffectItem.fnRunDraw_pe_once);

		public static FnEffectRun FD_fnRunDraw_pe_bounce = new FnEffectRun(PostEffectItem.fnRunDraw_pe_bounce);

		public static FnEffectRun FD_fnRunDraw_pe_absorbed = new FnEffectRun(PostEffectItem.fnRunDraw_pe_absorbed);

		public static FnEffectRun FD_fnRunDraw_pe_fadeinout = new FnEffectRun(PostEffectItem.fnRunDraw_pe_fadeinout);

		public static FnEffectRun FD_fnRunDraw_pe_fadeinout_zsinv = new FnEffectRun(PostEffectItem.fnRunDraw_pe_fadeinout_zsinv);

		public static float t_heartbeat = 0f;

		private static PostEffect.PEMaterial.FnShufflePos FD_fnShuffleVert32 = new PostEffect.PEMaterial.FnShufflePos(PostEffectItem.fnShuffleVert32);

		private static PostEffect.PEMaterial.FnShufflePos FD_fnShuffleUv = new PostEffect.PEMaterial.FnShufflePos(PostEffectItem.fnShuffleUv);

		private static PostEffect.PEMaterial.FnShufflePos FD_fnShuffleVert24 = new PostEffect.PEMaterial.FnShufflePos(PostEffectItem.fnShuffleVert24);
	}
}
