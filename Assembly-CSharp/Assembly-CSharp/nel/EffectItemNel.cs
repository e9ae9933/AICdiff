using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class EffectItemNel : EffectItem
	{
		public EffectItemNel(IEffectSetter _EF, string _title = "", float _x = 0f, float _y = 0f, float _z = 0f, int _time = 0, int _saf = 0)
			: base(_EF, _title, _x, _y, _z, _time, _saf)
		{
		}

		public override EffectItem initEffect(string type_name = "")
		{
			return base.initEffect((type_name == "") ? "nel.EffectItemNel,Assembly-CSharp" : type_name);
		}

		public static EffectItemNel fnCreateOneNel(IEffectSetter _EF, string _title = "", float _x = 0f, float _y = 0f, float _z = 0f, int _time = 0, int _saf = 0)
		{
			return new EffectItemNel(_EF, _title, _x, _y, _z, _time, _saf);
		}

		public static void initEffectItemNel()
		{
			EffectItem.initEffectItem();
			if (EffectItemNel.init_particle)
			{
				return;
			}
			EffectItemNel.init_particle = true;
			EffectItemNel.initParticleTypeNel();
			M2DropObjectReader.assignDrawFn("EnemyBlood", new M2DropObject.FnDropObjectDraw(EffectItemNel.fnDropRunDraw_EnemyBlood));
			M2DropObjectReader.assignDrawFn("LayedEffectEgg", new M2DropObject.FnDropObjectDraw(EffectItemNel.fnDropRunDraw_LayedEffectEgg));
			M2DropObjectReader.assignDrawFn("LayedEffectEggSlime", new M2DropObject.FnDropObjectDraw(EffectItemNel.fnDropRunDraw_LayedEffectEggSlime));
			M2DropObjectReader.assignDrawFn("LayedEffectWorm", new M2DropObject.FnDropObjectDraw(EffectItemNel.fnDropRunDraw_LayedEffectWorm));
			M2DropObjectReader.assignDrawFn("LayedEffectSlime", new M2DropObject.FnDropObjectDraw(EffectItemNel.fnDropRunDraw_LayedEffectSlime));
			M2DropObjectReader.assignDrawFn("LayedEffectMush", new M2DropObject.FnDropObjectDraw(EffectItemNel.fnDropRunDraw_LayedEffectMush));
		}

		public static void initParticleTypeNel()
		{
			EfParticle.assignType("LEAF", new FnPtcInit(EffectItemNel.fnPtcInitLeaf), new FnPtcDraw(EfParticle.DrawBasicBMLR));
			EfParticle.assignType("WATER_SPLASH", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawWaterSplash));
			EfParticle.assignType("SMOKE", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawSmoke));
			EfParticle.assignType("SMOKE_L", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawSmokeL));
			EfParticle.assignType("PARTICLE_SPLASH", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawParticleSplash));
			EfParticle.assignType("PARTICLE_SPLASH_F", new FnPtcInit(EffectItemNel.fnPtcColorAndLineSize), new FnPtcDraw(EffectItemNel.fnPtcDrawParticleSplashFixed));
			EfParticle.assignType("NOEL_BREAK_CLOTH", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawNoelBreakCloth));
			EfParticle.assignType("KISS", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawKiss));
			EfParticle.assignType("BIKUBIKU", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawBikubiku));
			EfParticle.assignType("BIKUBIKU_S", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawBikubikuS));
			EfParticle.assignType("BUBBLE_S", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawBubbleS));
			EfParticle.assignType("PARTICLE_HEART", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawParticleHeart));
			EfParticle.assignType("HEART", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawHeart));
			EfParticle.assignType("SLEEPING_Z", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawParticleSleepingZ));
			EfParticle.assignType("CONFUSING", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawParticleConfusing));
			EfParticle.assignType("SABI", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawParticleSabi));
			EfParticle.assignType("SURIKEN", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawParticleSuriken));
			EfParticle.assignType("SHOCKWAVE_PIC", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawShockwavePic));
			EfParticle.assignType("FRAMERIPPLE", new FnPtcInit(EffectItemNel.fnPtcColorAndLineSize), new FnPtcDraw(EffectItemNel.fnPtcDrawFrameRipple));
			EfParticle.assignType("SNOWDROP", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawSnowDrop));
			EfParticle.assignType("BIGBOMB", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawBigBomb));
			EfParticle.assignType("ENEMYTARGETTING", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawEnemyTargetting));
			EfParticle.assignType("SPERMSHOT", EffectItemNel.fnPtcInitOnlyColor, new FnPtcDraw(EffectItemNel.fnPtcDrawSpermShot));
			EfParticleManager.reloadParticleCsv(false);
			AttackGhostDrawer.AddGhostEfFunc("magicslice", new AttackGhostDrawer.FnAgdEfDraw(EffectItemNel.fnAgdDraw_magicslice));
			AttackGhostDrawer.AddGhostEfFunc("bossslice", new AttackGhostDrawer.FnAgdEfDraw(EffectItemNel.fnAgdDraw_bossslice));
			AttackGhostDrawer.AddGhostEfFunc("normal", new AttackGhostDrawer.FnAgdEfDraw(EffectItemNel.fnAgdDraw_normalEnemy));
			AttackGhostDrawer.AddGhostEfFunc("normal_rotR_scale100", new AttackGhostDrawer.FnAgdEfDraw(EffectItemNel.fnAgdDraw_normalEnemyRotated));
		}

		public static void reinitParticleCategory()
		{
		}

		public static bool fnPtcInitLeaf(EfParticle EFP, EfParticleVarContainer mde)
		{
			if (mde != null)
			{
				EFP.initColor(mde);
			}
			EFP.BmL = MTR.SqEfLeaf;
			return true;
		}

		public static bool fnPtcColorAndLineSize(EfParticle EFP, EfParticleVarContainer mde)
		{
			if (mde != null)
			{
				EFP.initColor(mde);
				EFP.SetV0D(mde, "attr", 1600);
				EFP.SetV0D(mde, "line_len_min", 1);
				EFP.SetV0D(mde, "line_len_max", EFP.line_len_min);
				EFP.line_len_dif = EFP.line_len_max - EFP.line_len_min;
				EFP.defineZmFunc(mde, "line_ex_type", "Z1");
			}
			return true;
		}

		public static bool fnPtcDrawSomething(EffectItem E, EfParticle EP, uint ran)
		{
			return true;
		}

		public static bool fnPtcDrawWaterSplash(EffectItem E, EfParticle EP, uint ran)
		{
			PxlSequence aefWaterSplash = MTR.AEfWaterSplash;
			int length;
			if (aefWaterSplash == null || (length = aefWaterSplash.Length) == 0)
			{
				return true;
			}
			int num = (int)(X.RAN(ran, 782) * (float)(length / 6));
			return EffectItemNel.PtcDrawParticleGeneral(E, EP, ran, aefWaterSplash[6 * num + X.MMX(0, (int)(EfParticle.tz * 6f), 5)], MTRX.MIicon, 0f, false, 1f);
		}

		public static bool fnPtcDrawSmoke(EffectItem E, EfParticle EP, uint ran)
		{
			PxlSequence aefSmoke = MTR.AEfSmoke;
			if (aefSmoke == null || aefSmoke.Length == 0)
			{
				return true;
			}
			int num = (int)(ran % 3U);
			return EffectItemNel.PtcDrawParticleGeneral(E, EP, ran, aefSmoke[num * 7 + X.MMX(0, (int)(EfParticle.tz * 7f), 6)], MTRX.MIicon, 0f, false, 1f);
		}

		public static bool fnPtcDrawSmokeL(EffectItem E, EfParticle EP, uint ran)
		{
			PxlSequence aefSmokeL = MTR.AEfSmokeL;
			if (aefSmokeL == null || aefSmokeL.Length == 0)
			{
				return true;
			}
			int num = (int)(ran % 4U);
			return EffectItemNel.PtcDrawParticleGeneral(E, EP, ran, aefSmokeL[num], MTR.MIiconL, 0f, false, 1f);
		}

		public static bool fnPtcDrawBubbleS(EffectItem E, EfParticle EP, uint ran)
		{
			PxlSequence abubbleS = MTR.ABubbleS;
			int length;
			if (abubbleS == null || (length = abubbleS.Length) == 0)
			{
				return true;
			}
			int num = (int)((ulong)ran % (ulong)((long)(length / 4)) * 4UL);
			int num2 = (int)(E.af + X.RAN(ran, 936) * 256f);
			return EffectItemNel.PtcDrawParticleGeneral(E, EP, ran, abubbleS[num + X.ANM(num2, 4, X.NI(4, 7, X.RAN(ran, 599)))], MTRX.MIicon, (float)((int)(X.RAN(ran, 1501) * 4f)) / 4f * 6.2831855f, false, 1f);
		}

		public static bool fnPtcDrawParticleSplash(EffectItem E, EfParticle EP, uint ran)
		{
			PxlSequence sqParticleSplash = MTR.SqParticleSplash;
			int num;
			return sqParticleSplash == null || (num = sqParticleSplash.countFrames()) == 0 || EffectItemNel.PtcDrawParticleGeneral(E, EP, ran, sqParticleSplash.getFrame((int)((ulong)ran % (ulong)((long)num))), MTR.MIiconL, 0f, false, 1f);
		}

		public static bool fnPtcDrawParticleSplashFixed(EffectItem E, EfParticle EP, uint ran)
		{
			PxlSequence sqParticleSplashFixed = MTR.SqParticleSplashFixed;
			int num;
			if (sqParticleSplashFixed == null || (num = sqParticleSplashFixed.countFrames()) == 0)
			{
				return true;
			}
			float num2 = EfParticle.FnMUL(EP.line_len_min + EfParticle.RANMUL(EP.line_len_dif, 2592), EP.Fn_line_ex_type);
			return EffectItemNel.PtcDrawParticleGeneral(E, EP, ran, sqParticleSplashFixed.getFrame((int)((ulong)ran % (ulong)((long)num))), MTR.MIiconL, 0f, false, num2);
		}

		public static bool fnPtcDrawParticleHeart(EffectItem E, EfParticle EP, uint ran)
		{
			PxlSequence aheartS = MTR.AHeartS;
			int length;
			if (aheartS == null || (length = aheartS.Length) == 0)
			{
				return true;
			}
			int num = (int)((ulong)ran % (ulong)((long)(length / 4)) * 4UL);
			float af = E.af;
			X.RAN(ran, 936);
			return EffectItemNel.PtcDrawParticleGeneral(E, EP, ran, aheartS[num + X.MMX(0, (int)(4f * EfParticle.tz), 3)], MTRX.MIicon, 0f, false, 1f);
		}

		public static bool fnPtcDrawParticleSleepingZ(EffectItem E, EfParticle EP, uint ran)
		{
			PxlSequence asleepingZ = MTR.ASleepingZ;
			return asleepingZ == null || asleepingZ.Length < 7 || EffectItemNel.PtcDrawParticleGeneral(E, EP, ran, asleepingZ[ran % 7U], MTRX.MIicon, 0f, false, 1f);
		}

		public static bool fnPtcDrawParticleConfusing(EffectItem E, EfParticle EP, uint ran)
		{
			PxlSequence asleepingZ = MTR.ASleepingZ;
			return asleepingZ == null || asleepingZ.Length < 13 || EffectItemNel.PtcDrawParticleGeneral(E, EP, ran, asleepingZ[7U + ran % 5U], MTRX.MIicon, 0f, false, 1f);
		}

		public static bool fnPtcDrawParticleSabi(EffectItem E, EfParticle EP, uint ran)
		{
			PxlSequence sqEfSabi = MTR.SqEfSabi;
			int num;
			return sqEfSabi == null || (num = sqEfSabi.countFrames()) == 0 || EffectItemNel.PtcDrawParticleGeneral(E, EP, ran, sqEfSabi.getFrame((int)((ulong)ran % (ulong)((long)num))), MTR.MIiconL, 0f, false, 1f);
		}

		public static bool fnPtcDrawNoelBreakCloth(EffectItem E, EfParticle EP, uint ran)
		{
			PxlSequence anoelBreakCloth = MTR.ANoelBreakCloth;
			int length;
			return anoelBreakCloth == null || (length = anoelBreakCloth.Length) == 0 || EffectItemNel.PtcDrawParticleGeneral(E, EP, ran, anoelBreakCloth[(long)((ulong)ran % (ulong)((long)length))], MTR.MIiconL, 0f, false, 1f);
		}

		public static bool fnPtcDrawParticleSuriken(EffectItem E, EfParticle EP, uint ran)
		{
			PxlFrame meshSuriken = MTR.MeshSuriken;
			return meshSuriken == null || EffectItemNel.PtcDrawParticleGeneral(E, EP, ran, meshSuriken, MTR.MIiconL, 0f, false, 1f);
		}

		public static bool PtcDrawParticleGeneral(EffectItem E, EfParticle EP, uint ran, PxlFrame PF, MImage MI, float agR_add = 0f, bool flip = false, float scaley = 1f)
		{
			if (PF == null)
			{
				return true;
			}
			EP.GetMesh(E, MI, true);
			C32 color = EP.getColor(true, true);
			MeshDrawer md = EfParticle.Md;
			md.Col = C32.MulA(color.C, EfParticle.calpha);
			md.RotaPF(EfParticle.cx, EfParticle.cy, EfParticle._zm, EfParticle._zm * scaley, EfParticle._agR + agR_add, PF, flip, false, false, uint.MaxValue, false, 0);
			return true;
		}

		public static bool fnPtcDrawHeart(EffectItem E, EfParticle EP, uint ran)
		{
			PxlSequence sqLOther = MTR.SqLOther;
			return sqLOther == null || EffectItemNel.PtcDrawParticleGeneral(E, EP, ran, sqLOther.getFrame(1), MTR.MIiconL, 0f, false, 1f);
		}

		public static bool fnPtcDrawKiss(EffectItem E, EfParticle EP, uint ran)
		{
			PxlSequence sqEfLip = MTR.SqEfLip;
			float num = EfParticle._thick / 100f;
			PxlImage image = sqEfLip.getImage(0, 0);
			EP.GetMesh(E, image, MTRX.MIicon);
			MeshDrawer meshDrawer = EfParticle.Md;
			C32 color = EP.getColor(true, true);
			meshDrawer.Col = color.C;
			float num2 = EfParticle._agR + X.COSI(EfParticle.tz, 0.18f) * (1f - X.ZSIN2(EfParticle.tz - 0.1f, X.NI(0.2f, 0.33f, X.RAN(ran, 2794)))) * X.NI(0.04f, 0.07f, X.RAN(ran, 2876)) * 3.1415927f;
			Matrix4x4 currentMatrix = meshDrawer.getCurrentMatrix();
			for (int i = 0; i < 2; i++)
			{
				if (i == 1)
				{
					EfParticle.Md.setCurrentMatrix(currentMatrix, false);
					meshDrawer = E.GetMeshImg("lip_back", MTRX.MIicon, BLEND.ADD, false);
					meshDrawer.base_z = EfParticle.Md.base_z + 4E-06f;
					meshDrawer.Col = color.mulA(0.8f).C;
				}
				for (int j = 0; j < 2; j++)
				{
					PxlImage image2 = sqEfLip.getImage(j, 0);
					meshDrawer.initForImg(image2, 0);
					float num3 = EfParticle._zm * (float)image2.width * 0.015625f;
					float num4 = EfParticle._zm * (float)image2.height * 0.015625f;
					float num5 = (float)((j == 0) ? (-2) : 0) * 0.015625f;
					float num6 = num * num4 * ((j == 0) ? 0.55f : 0.4f);
					for (int k = 0; k < 2; k++)
					{
						float num7 = ((k == 0) ? 0.015625f : 0f);
						meshDrawer.setCurrentMatrix(currentMatrix, false);
						meshDrawer.TranslateP(EfParticle.cx, EfParticle.cy, true);
						meshDrawer.Rotate(num2, true);
						meshDrawer.Scale((float)((k == 1) ? (-1) : 1), (float)((j == 1) ? (-1) : 1), true);
						meshDrawer.TriRectBL(0);
						meshDrawer.Pos(num7 - num3, num5, null).Pos(num7 - num3, num5 + num4, null).Pos(num7, num5 + num4 - num6, null)
							.Pos(num7, -num6, null)
							.InputImageUv();
					}
				}
			}
			meshDrawer.setCurrentMatrix(currentMatrix, false);
			return true;
		}

		public static bool fnPtcDrawBikubiku(EffectItem E, EfParticle EP, uint ran)
		{
			return EffectItemNel.fnPtcDrawBikubiku(E, EP, ran, false);
		}

		public static bool fnPtcDrawBikubikuS(EffectItem E, EfParticle EP, uint ran)
		{
			return EffectItemNel.fnPtcDrawBikubiku(E, EP, ran, true);
		}

		private static bool fnPtcDrawBikubiku(EffectItem E, EfParticle EP, uint ran, bool is_small)
		{
			PxlSequence abikubikuChars = MTR.ABikubikuChars;
			int num;
			if (abikubikuChars == null || (num = abikubikuChars.Length) == 0)
			{
				return true;
			}
			int num2 = 0;
			if (is_small)
			{
				num2 = 11;
				num -= num2;
			}
			else
			{
				num = 11;
			}
			PxlFrame pxlFrame = abikubikuChars[num2 + (int)(X.RAN(ran, 2800) * (float)num)];
			C32 color = EP.getColor(true, true);
			EP.GetMesh(E, MTR.MIiconL, true);
			MeshDrawer md = EfParticle.Md;
			md.Col = MTRX.ColWhite;
			md.allocUv23(4, false);
			md.Uv23(color.C, false);
			float num3 = (float)((int)((EfParticle.af - (float)EfParticle.i * EP.delay) / 3f)) * 3f;
			float num4 = X.COSI(num3, 2.13f + 1.76f * X.RAN(ran, 2882)) * 3.4f;
			float num5 = X.COSI(num3, 3.13f + 2.58f * X.RAN(ran, 2440)) * 5.5f;
			md.RotaPF(EfParticle.cx + num4, EfParticle.cy + num5, EfParticle._zm, EfParticle._zm, EfParticle._agR, pxlFrame, false, false, false, uint.MaxValue, false, 0);
			md.allocUv23(0, true);
			return true;
		}

		public static bool fnPtcDrawShockwavePic(EffectItem E, EfParticle EP, uint ran)
		{
			C32 color = EP.getColor(true, true);
			EP.GetMesh(E, MTR.MIiconL, true);
			MeshDrawer md = EfParticle.Md;
			md.Col = color.C;
			md.initForImg(MTR.ImgShockWave160, 0);
			md.RotaGraph(EfParticle.cx, EfParticle.cy, EfParticle._zm / 234f * 1.5f, X.RAN(ran, 930) * 6.2831855f, null, false);
			return true;
		}

		public static bool fnPtcDrawFrameRipple(EffectItem E, EfParticle EP, uint ran)
		{
			EP.getColor(true, true);
			ShockRippleDrawer frameRipple = MTR.FrameRipple;
			EP.GetMesh(E, frameRipple.TargetMI, true);
			MeshDrawer md = EfParticle.Md;
			if (EP.gradation != "LINE")
			{
				md.ColGrd.Set(md.Col).setA(0f);
			}
			frameRipple.DivideCount(EP.attr);
			frameRipple.thick_randomize = EfParticle.FnMUL(EP.line_len_min + EfParticle.RANMUL(EP.line_len_dif, 2592), EP.Fn_line_ex_type);
			frameRipple.radius_randomize_px = EP.line_ex_time_min + EfParticle.RANMUL(EP.line_ex_time_dif, 2877);
			float num = 0f;
			float num2 = 1f;
			frameRipple.texture_h_scale = X.Mx(0.25f, EfParticle._agR);
			frameRipple.gradation_focus = 0.55f;
			frameRipple.drawTo(md, EfParticle.cx, EfParticle.cy, EfParticle._zm, X.Mn(EfParticle._zm * 2f, EfParticle._thick), num2, num);
			return true;
		}

		public static bool fnPtcDrawSnowDrop(EffectItem E, EfParticle EP, uint ran)
		{
			EP.GetMesh(E, MTR.MIiconL, true);
			MeshDrawer md = EfParticle.Md;
			md.Col = EP.getColor(true, true).C;
			md.initForImg(MTR.SqFrozen.getImage((int)((float)MTR.SqFrozen.countFrames() * X.RAN(ran, 1155)), 0), 0);
			md.RotaGraph(EfParticle.cx, EfParticle.cy, EfParticle._zm, EfParticle._agR, null, false);
			return true;
		}

		public static bool fnPtcDrawSpermShot(EffectItem E, EfParticle EP, uint ran)
		{
			PxlSequence sqSpermShot = MTR.SqSpermShot;
			int num = (int)(ran % (uint)(sqSpermShot.countFrames() / 6));
			bool flag = X.RAN(ran, 2983) < 0.5f;
			EffectItemNel.PtcDrawParticleGeneral(E, EP, ran, sqSpermShot.getFrame(num + (int)(6f * EfParticle.tz)), MTR.MIiconL, flag ? 3.1415927f : 0f, flag, 1f);
			return true;
		}

		public static bool fnPtcDrawBigBomb(EffectItem E, EfParticle EP, uint ran)
		{
			EP.GetMesh(E, MTRX.IconWhite, MTRX.MIicon);
			MeshDrawer md = EfParticle.Md;
			md.Col = EP.getColor(true, true).C;
			PxlFrame frame = MTR.SqEffectBigBomb.getFrame((int)X.Mn((float)(MTR.SqEffectBigBomb.countFrames() - 1), (float)MTR.SqEffectBigBomb.countFrames() * EfParticle.tz));
			md.RotaPF(EfParticle.cx, EfParticle.cy, EfParticle._zm, EfParticle._zm, EfParticle._agR, frame, ran % 2U == 0U, false, false, uint.MaxValue, false, 0);
			return true;
		}

		public static bool fnPtcDrawEnemyTargetting(EffectItem E, EfParticle EP, uint ran)
		{
			EP.GetMesh(E, null, false);
			MeshDrawer md = EfParticle.Md;
			md.Col = EP.getColor(true, true).C;
			md.TranslateP(EfParticle.cx, EfParticle.cy, true);
			md.Circle(0f, 0f, EfParticle._zm, EfParticle._thick, false, 0f, 0f);
			float num = 0.75f * EfParticle._zm;
			float num2 = 1.25f * EfParticle._zm;
			md.Line(-num, -num, -num2, -num2, EfParticle._thick, false, 0f, 0f);
			md.Line(-num, num, -num2, num2, EfParticle._thick, false, 0f, 0f);
			md.Line(num, -num, num2, -num2, EfParticle._thick, false, 0f, 0f);
			md.Line(num, num, num2, num2, EfParticle._thick, false, 0f, 0f);
			return true;
		}

		public static bool fnRunDraw_dash_afterimage(EffectItem E)
		{
			if (E.af >= (float)E.time)
			{
				return false;
			}
			EffectItemNel.drawEvadeAfterImage(NelM2DBase.PostEffectMesh(MTR.MtrShiftImage, E, true), (int)E.z, E.f0, 1f - E.af / (float)E.time, 1f, 0f);
			return true;
		}

		public static bool fnRunDraw_comet_afterimage(EffectItem E)
		{
			if (E.af >= (float)E.time)
			{
				return false;
			}
			MeshDrawer meshDrawer = NelM2DBase.PostEffectMesh(MTR.MtrShiftImage, E, true);
			meshDrawer.Rotate(1.5707964f, false);
			EffectItemNel.drawEvadeAfterImage(meshDrawer, (int)E.z, E.f0, 1f - E.af / (float)E.time, 0f, 1f);
			return true;
		}

		private static void drawEvadeAfterImage(MeshDrawer Md, int count, int f0, float tz, float xlvl, float ylvl)
		{
			Md.allocUv2(64, false);
			float num = (float)(-(float)(count / 2)) * 0.015625f;
			float num2 = 0.03125f;
			for (int i = 0; i < count; i += 2)
			{
				uint ran = X.GETRAN2(f0 + i, 4 + i % 13);
				float num3 = (15f + X.RAN(ran, 1555) * 38f) * 0.015625f;
				float num4 = num3 / 2f;
				float num5 = -num4 + (-30f + 60f * X.RAN(ran, 1130)) * 0.015625f;
				float num6 = num + num2;
				float num7 = (float)X.IntR((-10f + 20f * X.RAN(ran, 2613)) * tz) * 0.015625f;
				Md.Uv2(0f, 0f, false);
				Md.Uv2(0f, 0f, false);
				Md.Uv2(num7 * xlvl, num7 * ylvl, false);
				Md.Uv2(num7 * xlvl, num7 * ylvl, false);
				Md.Uv2(0f, 0f, false);
				Md.Uv2(0f, 0f, false);
				Md.Tri(0, 1, 2, false).Tri(0, 2, 3, false).Tri(3, 2, 4, false)
					.Tri(3, 4, 5, false);
				Md.Pos(num5, num, null).Pos(num5, num6, null).Pos(num5 + num4, num6, null)
					.Pos(num5 + num4, num, null)
					.Pos(num5 + num3, num6, null)
					.Pos(num5 + num3, num, null);
				num = num6;
			}
		}

		public static bool fnRunDraw_postHamonMagic(EffectItem E)
		{
			if (E.af >= (float)E.time)
			{
				return false;
			}
			EffectItem.E0 = E;
			float num = E.af / (float)E.time;
			EffectItemNel.tz = X.ZSIN(num, 0.4f);
			MeshDrawer meshDrawer = NelM2DBase.PostEffectMesh(MTR.MtrShiftImage, E, true);
			meshDrawer.Col = EffectItem.Col1.Black().C;
			meshDrawer.ColGrd.Set(MTRX.ColTrnsp);
			meshDrawer.fnMeshPointColor = (MeshDrawer Md, float x, float y, C32 DefCol, float len, float agR) => EffectItemNel.fnColPostHamonMagic(Md, x, y, DefCol, len, agR);
			meshDrawer.allocUv2(64, false);
			meshDrawer.BlurCircle2(0f, 0f, E.z + 130f * num, X.Mx(2f, E.z - 5f) * (1f - num), X.Mx(40f, E.z * 0.5f) * X.ZSIN(num, 0.3f) * (1f - num), meshDrawer.ColGrd, meshDrawer.ColGrd);
			meshDrawer.fnMeshPointColor = null;
			return true;
		}

		private static C32 fnColPostHamonMagic(MeshDrawer Md, float x, float y, C32 DefCol, float len, float agR)
		{
			float num = ((DefCol == null) ? 1f : ((float)DefCol.a / 255f));
			float num2 = X.Mx(-len, (-30f + 25f * EffectItemNel.tz) * num * 0.015625f);
			Md.Uv2(num2 * X.Cos(agR), num2 * X.Sin(agR), false);
			return DefCol;
		}

		public static bool fnRunDraw_postPopSummon(EffectItem E)
		{
			if (E.af >= (float)E.time)
			{
				return false;
			}
			EffectItem.E0 = E;
			float num = E.af / (float)E.time;
			EffectItemNel.tz = X.ZSIN(num, 0.7f);
			float num2 = 0.5f + X.ZSIN(num, 1f) * 0.5f;
			MeshDrawer meshDrawer = NelM2DBase.PostEffectMesh(MTR.MtrShiftImage, E, true);
			meshDrawer.Col = EffectItem.Col1.Black().setA1(X.ZLINE(1f - 1f * num) * 0.4f).C;
			EffectItem.Col1.setA1(X.ZLINE(1.5f * (1f - num)));
			meshDrawer.ColGrd.Set(MTRX.ColTrnsp);
			EffectItemNel.postpopsummon_agR_add = 0f;
			EffectItemNel.postpopsummon_xscale = (EffectItemNel.postpopsummon_yscale = 1f);
			meshDrawer.fnMeshPointColor = (MeshDrawer Md, float x, float y, C32 DefCol, float len, float agR) => EffectItemNel.fnColPostPopSummon(Md, x, y, DefCol, len, agR);
			meshDrawer.allocUv2(64, false).BlurCircle2(0f, 0f, E.z * 0.4f * num2, E.z * 2f, E.z * 0.6f * num2, EffectItem.Col1, meshDrawer.ColGrd);
			meshDrawer.fnMeshPointColor = null;
			return true;
		}

		private static C32 fnColPostPopSummon(MeshDrawer Md, float x, float y, C32 DefCol, float len, float agR)
		{
			float num = (float)((DefCol == null) ? Md.Col.a : DefCol.a) / 255f;
			float num2 = X.Mx(-len, (50f - 135f * EffectItemNel.tz) * num * 0.015625f);
			agR += EffectItemNel.postpopsummon_agR_add;
			Md.Uv2(num2 * X.Cos(agR) * EffectItemNel.postpopsummon_xscale, num2 * X.Sin(agR) * EffectItemNel.postpopsummon_yscale, false);
			return DefCol;
		}

		public static bool fnRunDraw_postPopSummonR20(EffectItem E)
		{
			return EffectItemNel.drawPostPopSummonR(E, (float)E.time, 20f, 0.3f, 0.9f, E.z);
		}

		private static bool drawPostPopSummonR(EffectItem E, float size, float maxt, float zmx, float zmy, float agR)
		{
			if (E.af >= maxt)
			{
				return false;
			}
			EffectItem.E0 = E;
			float num = E.af / maxt;
			EffectItemNel.tz = X.ZSIN(num, 0.7f);
			float num2 = 0.5f + X.ZSIN(num, 1f) * 0.5f;
			MeshDrawer meshDrawer = NelM2DBase.PostEffectMesh(MTR.MtrShiftImage, E, true);
			meshDrawer.Col = EffectItem.Col1.Black().setA1(X.ZLINE(1f - 1f * num) * 0.4f).C;
			EffectItem.Col1.setA1(X.ZLINE(1.5f * (1f - num)));
			meshDrawer.ColGrd.Set(MTRX.ColTrnsp);
			EffectItemNel.postpopsummon_agR_add = agR;
			EffectItemNel.postpopsummon_xscale = zmx;
			EffectItemNel.postpopsummon_yscale = zmy;
			meshDrawer.fnMeshPointColor = (MeshDrawer Md, float x, float y, C32 DefCol, float len, float agR) => EffectItemNel.fnColPostPopSummon(Md, x, y, DefCol, len, agR);
			meshDrawer.BlurCircle2(0f, 0f, size * 0.4f * num2, size * 2f, size * 0.6f * num2, EffectItem.Col1, meshDrawer.ColGrd);
			meshDrawer.fnMeshPointColor = null;
			return true;
		}

		private static Vector2 getCaneSwingMove(int i)
		{
			float num = 1f;
			if (i < 3 && (EnhancerManager.enhancer_bits & EnhancerManager.EH.long_reach) != (EnhancerManager.EH)0U)
			{
				num = X.NI(3.45f, 1f, X.ZPOW((float)i, 3f));
			}
			switch (i)
			{
			case 0:
				return new Vector2(-14f * num, -12f);
			case 1:
				return new Vector2(-7f * num, -11f);
			case 2:
				return new Vector2(-2f * num, -12f);
			case 3:
				return new Vector2(5f * num, -10f);
			case 4:
				return new Vector2(11f, -10f);
			case 5:
				return new Vector2(25f, -8f);
			case 6:
				return new Vector2(51f, -6f);
			case 7:
				return new Vector2(39f, -2f);
			case 8:
				return new Vector2(30f, 0f);
			default:
				return Vector2.zero;
			}
		}

		public static bool fnRunDraw_cane_swing(EffectItem E)
		{
			float num = X.ZSIN(E.af, (float)E.time);
			return EffectItemNel.drawCaneSwing(E.GetMesh("", MTRX.MtrMeshNormal, false), num, E.z, uint.MaxValue, 4291483616U, 1f);
		}

		public static bool fnRunDraw_cane_swing_mg(EffectItem E)
		{
			float num = X.ZSIN(E.af, (float)E.time);
			bool flag = EffectItemNel.drawCaneSwing(E.GetMesh("", MTRX.MtrMeshSub, true), num, E.z, 4288112144U, 4281538306U, 1.4f);
			EffectItemNel.drawCaneSwing(E.GetMesh("", MTRX.MtrMeshAdd, false), num, E.z, 4288271359U, 4278205695U, 1f);
			return flag;
		}

		public static bool drawCaneSwing(MeshDrawer Md, float tzs, float lax, uint col_s, uint col_e, float basescale = 1f)
		{
			float num = 0.8f + tzs * 0.2f;
			float num2 = 0.8f * (1f - tzs);
			float num3 = num - num2;
			float num4 = 1f - 0.9f * X.ZLINE(tzs, 0.3f);
			Vector2 vector = new Vector2(53f, 28f);
			float num5 = 1f + tzs * 0.25f;
			float num6 = 1f + tzs * 0.15f;
			float num7 = 1f - X.ZLINE(tzs - 0.6f, 0.39999998f);
			int num8 = 9;
			float num9 = 0f;
			float num10 = 1f / (float)(num8 - 1);
			Md.Col = MTRX.cola.Set(col_s).blend(col_e, 1f - num4).C;
			C32 c = MTRX.cola.Set(Md.Col);
			C32 c2 = MTRX.colb.Set(c);
			C32 c3 = EffectItem.Col1.Set(c);
			C32 c4 = EffectItem.Col2.Set(c);
			Matrix4x4 currentMatrix = Md.getCurrentMatrix();
			Md.Scale((float)((lax > 0f) ? 1 : (-1)), 1f, false);
			for (int i = 0; i < num8; i++)
			{
				Vector2 caneSwingMove = EffectItemNel.getCaneSwingMove(i);
				float num11 = num9 + num10;
				caneSwingMove.x *= -1f;
				if (num11 >= num3)
				{
					float num12 = X.ZSIN2(num9 - num3, num2 * 0.75f);
					float num13 = X.ZSIN2(num11 - num3, num2 * 0.75f);
					c.setA1(num7 * num12 * num4);
					c2.setA1(num7 * (1f - 2f * X.Abs(num13 - 0.5f)) * num4);
					Md.Col = c3.setA1(num7 * X.NI(num12, num13, 0.3f)).C;
					c3.setA1(num7 * num12);
					c4.setA1(num7 * num13);
					Md.BlurLine3(vector.x * num5 * basescale, vector.y * num6, (vector.x + caneSwingMove.x) * num5 * basescale, (vector.y + caneSwingMove.y) * num6, (1f + 22f * X.ZPOW3(num12 * num7)) * basescale, (3f + 5f * num12) * basescale, (1f + 22f * X.ZPOW3(num13 * num7)) * basescale, (3f + 5f * num13) * basescale, c, c3, c2, c4, 3, 6f * basescale, false);
				}
				vector += caneSwingMove;
				num9 = num11;
			}
			Md.setCurrentMatrix(currentMatrix, false);
			return tzs < 1f;
		}

		public static bool fnRunDraw_mp_crack(EffectItem E)
		{
			int particleCount = E.EF.getParticleCount(E, (int)E.x);
			float num = 1f / (float)particleCount;
			float num2 = num * 0.5f;
			float num3 = IN.w - X.Abs(M2DBase.Instance.ui_shift_x);
			float h = IN.h;
			float num4 = h / (num3 + h + num3 + h);
			float num5 = num3 / (num3 + h + num3 + h);
			float num6 = num5 + num4;
			float num7 = num5 + num4 + num4;
			float num8 = 0.5f + X.ZSIN(E.af, (float)E.time);
			float num9 = E.y * 0.5f;
			if (E.af >= (float)E.time + num9 * 2f)
			{
				return false;
			}
			MeshDrawer mesh = E.GetMesh("", MTRX.MtrMeshNormal, false);
			M2Camera cam = M2DBase.Instance.Cam;
			float scaleRev = cam.getScaleRev();
			Vector3 posMainTransform = cam.PosMainTransform;
			mesh.base_x = posMainTransform.x;
			mesh.base_y = posMainTransform.y;
			mesh.Col = mesh.ColGrd.White().setA1(1f - X.ZLINE(E.af - (float)E.time - num9, num9)).C;
			mesh.ColGrd.setA(0f);
			for (int i = 0; i < particleCount; i++)
			{
				uint ran = X.GETRAN2(E.f0 % 7 + i * 13, i + E.f0 % 117);
				float num10 = num2 + X.NI(-0.4f, 0.4f, X.RAN(ran, 2716)) * num;
				float num11;
				float num12;
				if (num10 >= num7)
				{
					num11 = -h * 0.5f;
					num12 = (0.5f - (num10 - num7) / num5) * num3;
				}
				else if (num10 >= num6)
				{
					num12 = num3 * 0.5f;
					num11 = (0.5f - (num10 - num6) / num4) * h;
				}
				else if (num10 >= num4)
				{
					num11 = h * 0.5f;
					num12 = (-0.5f + (num10 - num4) / num5) * num3;
				}
				else
				{
					num12 = -num3 * 0.5f;
					num11 = (-0.5f + num10 / num4) * h;
				}
				num12 *= scaleRev;
				num11 *= scaleRev;
				float num13 = 80f + X.RAN(ran, 2547) * 190f * scaleRev;
				float num14 = num13 * num8;
				float num15 = X.NI(50, 100, X.RAN(ran, 2884)) / 100f * num13;
				ElecDrawer elecDrawer = MTRX.Elec.BallRadius(0.1f * scaleRev, -1000f).DivideWidth(X.NI(50, 70, X.RAN(ran, 1512)) * X.NI(scaleRev, 1f, 0.5f)).Thick(X.NI(6, 10, X.RAN(ran, 2689)), 0f)
					.JumpHeight(num15, X.NI(0.3f, 0.77f, X.RAN(ran, 2342)) * num15)
					.Ran(ran)
					.JumpRatio(0.6f);
				float num16 = X.GAR(num12, num11, 0f, 0f) + X.NI(-0.1f, 0.1f, X.RAN(ran, 703)) * 6.2831855f;
				elecDrawer.finePos(num12, num11, num12 + num14 * X.Cos(num16), num11 + num14 * 7f / 9f * X.Sin(num16));
				elecDrawer.draw(mesh, 1f, true);
				num2 += num;
			}
			return true;
		}

		public static bool fnRunDraw_summon_activate(EffectItem E)
		{
			M2Camera cam = M2DBase.Instance.Cam;
			float scaleRev = cam.getScaleRev();
			Vector3 posMainTransform = cam.PosMainTransform;
			MeshDrawer mesh = E.GetMesh("summon_activate", MTRX.MtrMeshStripedSub, false);
			if (mesh.getTriMax() == 0)
			{
				mesh.base_z -= 1f;
			}
			mesh.base_x = (mesh.base_x = posMainTransform.x);
			mesh.base_y = (mesh.base_y = posMainTransform.y);
			float num = 0.5f;
			float num2 = 1f;
			if (E.af < E.x)
			{
				num *= X.ZSIN(E.af, E.x);
			}
			else
			{
				float num3 = E.af - E.x - E.y;
				if (num3 >= E.z)
				{
					return false;
				}
				if (num3 >= 0f)
				{
					num += 0.5f * X.ZSIN(num3, E.z);
				}
			}
			float num4 = IN.wh * scaleRev + 20f;
			float num5 = IN.hh * scaleRev + 20f;
			float num6 = IN.hh * 0.8f * scaleRev;
			float num8;
			float num9;
			if (num <= 0.5f)
			{
				float num7 = X.ZSIN(num, 0.5f);
				num6 *= 0.5f + 0.5f * num7;
				num8 = num6 * X.NI(1f, 0.65f, num7);
				num9 = X.ZLINE(num - 0.35f, 0.15f) * 0.15f;
				num2 = X.ZLINE(num, 0.45f);
			}
			else
			{
				num8 = num6 * 0.65f * (1f - 1.2f * X.ZPOW(num - 0.5f, 0.5f));
				num6 *= 1f + 2f * X.ZPOW3(num - 0.5f, 0.5f);
				num9 = 0.15f + X.ZSINV(num - 0.6f, 0.4f) * 0.85f;
			}
			mesh.Col = MTRX.cola.Set(4281755764U).mulA(num2).C;
			if (num6 >= IN.wh && num6 >= IN.hh)
			{
				return false;
			}
			mesh.uvRectN(X.Cos(0.7853982f), -X.Sin(0.7853982f));
			mesh.allocUv2(12, false).Uv2(10f, 1f - num9, false);
			mesh.Tri(0, 1, 5, false).Tri(1, 2, 6, false).Tri(2, 3, 7, false)
				.Tri(3, 0, 4, false)
				.Tri(0, 5, 4, false)
				.Tri(1, 6, 5, false)
				.Tri(2, 7, 6, false)
				.Tri(3, 4, 7, false);
			mesh.PosD(-num4, -num5, null).PosD(-num4, num5, null).PosD(num4, num5, null)
				.PosD(num4, -num5, null);
			mesh.PosD(0f, -num6, null).PosD(-num6, 0f, null).PosD(0f, num6, null)
				.PosD(num6, 0f, null);
			if (num8 > 0f)
			{
				mesh.Poly(0f, 0f, num8, 0f, 4, 0f, false, 0f, 0f);
			}
			mesh.allocUv2(0, true);
			return true;
		}

		public static bool fnRunDraw_summon_activate_event(EffectItem E)
		{
			M2Camera cam = M2DBase.Instance.Cam;
			MeshDrawer mesh = E.GetMesh("", uint.MaxValue, BLEND.SUB, false);
			Vector3 posMainTransform = cam.PosMainTransform;
			mesh.base_x = posMainTransform.x + cam.Qu.x * 0.015625f;
			mesh.base_y = posMainTransform.y;
			mesh.Col = MTRX.cola.Set(4281755764U).C;
			bool flag = false;
			float num = IN.h + 64f;
			for (int i = 0; i < 24; i++)
			{
				float num2 = E.af - (float)i * 0.7f;
				if (num2 < 0f)
				{
					flag = true;
					break;
				}
				uint ran = X.GETRAN2(i + (int)(E.index & 7U), 3 + (i & 11));
				float num3 = X.NI(40, 110, X.RAN(ran, 1442));
				if (num2 < num3)
				{
					float num4 = X.ZLINE(num2, 7f);
					float num5 = X.ZLINE(num3 - num2, 14f) * X.NI(3, 30, X.RAN(ran, 671)) * 0.5f * ((X.RAN(ran, 619) < 0.2f) ? 2.2f : 1f);
					float num6 = -IN.wh + IN.w * X.RAN(ran, 1405);
					mesh.RectBL(num6 - num5, -num * 0.5f, num5 * 2f, num * num4, false);
					flag = true;
				}
			}
			return flag;
		}

		public static bool fnRunDraw_summon_activate_rect(EffectItem E)
		{
			MeshDrawer mesh = E.GetMesh("", MTRX.MtrMeshMul, false);
			mesh.Col = C32.d2c(2852126720U);
			float num = X.ZSIN2(E.af, (float)E.time);
			if (num >= 1f)
			{
				return false;
			}
			mesh.Rect(0f, 0f, E.z * X.NI(1f, 0.03f, num), X.NI(300, 900, num), false);
			return true;
		}

		public static bool fnRunDraw_summon_activate_sudden(EffectItem E)
		{
			float num = (float)E.time;
			float num2 = IN.w - UIBase.Instance.ui_shift_x - 120f;
			float num3 = IN.h - 150f;
			int num4 = 14;
			Vector3 posMainTransform = M2DBase.Instance.Cam.PosMainTransform;
			MeshDrawer mesh = E.GetMesh("", MTR.MtrSummonSudden, false);
			mesh.base_x = (mesh.base_x = posMainTransform.x);
			mesh.base_y = (mesh.base_y = posMainTransform.y);
			bool flag = false;
			for (int i = 0; i < num4; i++)
			{
				uint ran = X.GETRAN2(45 + i * 7, 3 + i % 11);
				uint ran2 = X.GETRAN2(X.ANMT(3, 7f) * 29 + 51 + i * 5, 4 + i % 6);
				float num5 = X.RAN(ran, 2614) * 22f;
				float num6 = E.af - num5;
				if (num6 < 0f)
				{
					flag = true;
				}
				else
				{
					float num7 = 1f - X.ZLINE(num6 - num * 0.5f, num * 0.5f);
					if (num7 > 0f)
					{
						flag = true;
						int num8 = (int)(ran % 4U);
						Vector3 vector = X.RANBORDER(num2, num3, X.RAN(ran, 1962));
						vector.x += 84f * X.RAN(ran, 1106) + 7.5f * X.RAN(ran2, 1862);
						vector.y += 108f * X.RAN(ran, 578) + 7.5f * X.RAN(ran2, 2002);
						float num9 = X.GAR2(vector.x, vector.y, 0f, 0f);
						float num10 = CAim.get_agR(CAim.get_opposite((AIM)vector.z), 0f) + (-0.5f + X.RAN(ran, 418)) * 0.13f * 6.2831855f;
						num10 += X.angledifR(num10, num9) * (0.6f + 0.35f * X.RAN(ran, 1882) + 0.33f * X.RAN(ran2, 1899));
						int num11 = num8 * 12 + X.ANML((int)num6, 12, 4, 9);
						float num12 = X.NI(1.5f, 2.2f, X.RAN(ran, 2867));
						mesh.Scale(num12 * (1f + (-0.5f + X.RAN(ran2, 1560)) * 0.04f), num12 * (1f + (-0.5f + X.RAN(ran2, 2085)) * 0.04f), false);
						mesh.Skew(X.Tan0((X.RAN(ran2, 2649) - 0.5f) * 0.02f), X.Tan0((X.RAN(ran2, 3144) - 0.5f) * 0.02f), false);
						mesh.TranslateP(vector.x, vector.y, false);
						mesh.Col = mesh.ColGrd.White().blend(4294901760U, 0.5f + 0.25f * X.RAN(ran2, 781) + X.COSI((float)IN.totalframe + X.RAN(ran, 1705) * 200f, 31f) * 0.125f + X.COSI((float)IN.totalframe + X.RAN(ran, 450) * 128f, 11.7f) * 0.125f).setA1(num7)
							.C;
						mesh.RotaPF(0f, 0f, 1f, 1f, num10, MTR.ASuddenEye[num11], false, false, false, uint.MaxValue, false, 0);
						mesh.Identity();
					}
				}
			}
			if (E.af >= 40f)
			{
				float num13 = E.af - 40f;
				float num14 = X.ZLINE(E.af - num * 0.5f, num * 0.5f);
				if (num13 >= 0f && num14 < 1f)
				{
					flag = true;
					uint ran3 = X.GETRAN2(X.ANMT(9, 7f) * 21, 11);
					float num15 = 1.5f + 4f * (1f - X.ZLINE(num13, 14f)) - 0.125f * X.ZLINE(num13, num - 40f);
					mesh.Col = mesh.ColGrd.White().blend(4294901760U, 0.5f + 0.25f * X.RAN(ran3, 680) + X.COSI((float)IN.totalframe, 47f) * 0.125f + X.COSI((float)IN.totalframe, 13f) * 0.125f).mulA(1f - num14)
						.C;
					mesh.RotaPF(0f, 0f, num15, num15, 0f, MTRX.getPF("nel_sudden_attack"), false, false, false, uint.MaxValue, false, 0);
				}
			}
			return flag;
		}

		public static bool fnRunDraw_summon_activate_sudden_puppetrevenge(EffectItem E)
		{
			return PuppetRevenge.efDraw(E);
		}

		public static bool fnRunDraw_summoner_activate_sudden_puppetrevenge_phase(EffectItem E)
		{
			BMListChars chrNelS = MTR.ChrNelS;
			if (E.af >= 170f)
			{
				return false;
			}
			float num = X.ZLINE(E.af, 170f);
			float num2 = X.ZSIN(num, 0.11f) - X.ZPOW(num - 0.78f, 0.22000003f);
			int num3 = 3;
			MeshDrawer mesh = E.GetMesh("", uint.MaxValue, BLEND.SUB, true);
			Vector3 posMainTransform = M2DBase.Instance.Cam.PosMainTransform;
			mesh.base_x = posMainTransform.x;
			mesh.base_y = posMainTransform.y;
			for (int i = 0; i < num3; i++)
			{
				uint ran = X.GETRAN2((int)(E.af / 2f) + i * 13, 7 + i * 2);
				mesh.Col = mesh.ColGrd.Set(4288705147U).mulA(X.NI(0.2f, 0.45f, X.RAN(ran, 1050))).C;
				float num4 = X.RAN(ran, 1137) * 9f * (float)X.MPF(X.RAN(ran, 1948) < 0.5f);
				float num5 = X.NI(1f, 1.08f, X.RAN(ran, 2778)) * 98f * num2;
				mesh.Rect(0f, num4, IN.w + 50f, num5, false);
			}
			float num6 = X.ZLINE(num - 0.13f, 0.12f) - X.ZLINE(num - 0.73f, 0.21f);
			if (num6 > 0f)
			{
				MeshDrawer meshImg = E.GetMeshImg("", MTRX.MIicon, BLEND.ADD, false);
				meshImg.base_x = posMainTransform.x;
				meshImg.base_y = posMainTransform.y;
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.Add("VENGEANCE ");
					NEL.nel_num(stb, E.time + 1, false);
					int num7 = X.IntC(X.ZLINE(num - 0.13f, 0.24f) * (float)stb.Length);
					float num8 = chrNelS.DrawScaleStringTo(null, stb, 0f, 0f, 5f, 5f, ALIGN.LEFT, ALIGNY.TOP, false, 0f, 0f, null);
					stb.Length = num7;
					for (int j = 0; j < num3; j++)
					{
						uint ran2 = X.GETRAN2((int)(E.af / 2f) + 23 + j * 13, 5 + j * 3);
						meshImg.Col = meshImg.ColGrd.Set(4293488287U).mulA(X.NI(0.26f, 0.45f, X.RAN(ran2, 2041)) * (float)((X.RAN(ran2, 1097) < 0.22f) ? 3 : 1) * num6).C;
						float num9 = -num8 * 0.5f + 3f * X.RAN(ran2, 2163) * (float)X.MPF(X.RAN(ran2, 3199) < 0.5f);
						float num10 = 3f * X.RAN(ran2, 1432) * (float)X.MPF(X.RAN(ran2, 1082) < 0.5f);
						chrNelS.DrawScaleStringTo(meshImg, stb, num9, num10, 5f, 5f, ALIGN.LEFT, ALIGNY.MIDDLE, false, 0f, 0f, null);
					}
				}
			}
			return true;
		}

		public static bool fnRunDraw_mg_fireball_explode_sphere(EffectItem E)
		{
			return EffectItemNel.mg_fireball_explode_sphere(E, MTR.MtrFireBallExplode, 1.25f, 4284237210U, uint.MaxValue, uint.MaxValue);
		}

		public static bool fnRunDraw_mg_itembomb_explode_sphere(EffectItem E)
		{
			return EffectItemNel.mg_fireball_explode_sphere(E, MTR.MtrFoxChaserBallExplode, 1.125f, 4294947906U, 4292092450U, 4284883490U);
		}

		public static bool fnRunDraw_fox_chaserball_explode_sphere(EffectItem E)
		{
			return EffectItemNel.mg_fireball_explode_sphere(E, MTR.MtrFoxChaserBallExplode, 1.125f, 4294947906U, 4294946940U, 4292083712U);
		}

		private static bool mg_fireball_explode_sphere(EffectItem E, Material Mtr, float max_scale, uint tintcolor, uint basecolor = 4294967295U, uint basecolor_fade = 4294967295U)
		{
			if (E.af >= (float)E.time)
			{
				return false;
			}
			if (!E.isinCameraPtcCen(max_scale * E.z + 10f))
			{
				return true;
			}
			float num = X.ZLINE(E.af, (float)E.time);
			float num2 = X.ZSIN2(E.af, (float)E.time);
			Mtr.SetFloat("_Height", X.NI(0.1f, 0.6f, X.ZSIN(num, 0.35f)) + X.ZCOS(num - 0.3f, 0.7f) * 0.6f);
			Mtr.SetFloat("_ScrollX", 9.9f);
			Mtr.SetFloat("_Hard", X.NI(14, 4, X.ZSIN2(num, 0.3f)) - 3f * X.ZPOW(num - 0.2f, 0.8f));
			Mtr.SetColor("_TintA", EffectItem.Col1.Set(tintcolor).blend(70U, X.ZSIN2(num, 0.3f)).C);
			MeshDrawer mesh = E.GetMesh("_fireball", Mtr, false);
			mesh.setMtrTextureAndOffset("_NoiseTex", MTRX.SqEfPattern.getImage(0, 0));
			mesh.setMtrTextureAndOffset("_DistortTex", MTRX.SqEfPattern.getImage(3, 0));
			mesh.Col = EffectItem.Col1.Set(basecolor).blend(basecolor_fade, num).mulA(X.NI(1, 0, X.ZPOW(num - 0.24f, 0.66f)))
				.C;
			float num3 = E.z * X.NI(1f, max_scale, num2) * 0.015625f;
			mesh.uvRect(mesh.base_x - num3, mesh.base_y - num3, num3 * 2f, num3 * 2f, true, false);
			mesh.Circle(0f, 0f, num3, 0f, true, 0f, 0f);
			return true;
		}

		public static bool fnRunDraw_post_fireball_sphere(EffectItem E)
		{
			return EffectItemNel.mg_post_fireball_sphere(E, 1.25f);
		}

		public static bool fnRunDraw_post_fox_chaserball_sphere(EffectItem E)
		{
			return EffectItemNel.mg_post_fireball_sphere(E, 1.125f);
		}

		public static bool mg_post_fireball_sphere(EffectItem E, float max_scale)
		{
			if (E.af >= (float)E.time)
			{
				return false;
			}
			if (!E.isinCameraPtcCen(max_scale * E.z + 10f))
			{
				return true;
			}
			EffectItem.E0 = E;
			float num = X.ZLINE(E.af, (float)E.time);
			float num2 = X.ZSIN2(E.af, (float)E.time);
			float num3 = 1f - X.ZPOW(num - 0.25f, 0.75f);
			float num4 = E.z * X.NI(1f, max_scale, num2);
			float num5 = num4 * 0.015625f;
			MeshDrawer meshDrawer = NelM2DBase.PostEffectMesh(MTR.MtrShiftImage, E, true);
			meshDrawer.Col = EffectItem.Col1.Black().mulA(0.8f * num3).C;
			meshDrawer.ColGrd.Set(MTRX.ColTrnsp);
			C32 c = EffectItem.Col2.Black().mulA(num3);
			meshDrawer.fnMeshPointColor = (MeshDrawer Md, float x, float y, C32 DefCol, float len, float agR) => EffectItemNel.fnColPostPopFireBallShere(Md, x, y, DefCol, len, agR);
			EffectItemNel.postpopsummon_yscale = meshDrawer.base_y + num5;
			EffectItemNel.postsphere_wu = num5;
			EffectItemNel.postsphere_hu = num5 * (9f - 9f * X.ZSIN(num));
			meshDrawer.allocUv2(64, false).BlurCircle2(0f, 0f, num4 * 0.8f, num4, num4 * 0.2f, c, meshDrawer.ColGrd);
			meshDrawer.fnMeshPointColor = null;
			return true;
		}

		private static C32 fnColPostPopFireBallShere(MeshDrawer Md, float x, float y, C32 DefCol, float len, float agR)
		{
			float num = (float)((DefCol == null) ? Md.Col.a : DefCol.a) / 255f;
			float num2 = EffectItemNel.postsphere_wu * 0.8f;
			float num3 = X.ZLINE(EffectItemNel.postsphere_hu + EffectItemNel.postsphere_wu * (-1f + X.ZSIN(-(y - EffectItemNel.postpopsummon_yscale), EffectItemNel.postsphere_hu)), EffectItemNel.postsphere_hu);
			float num4 = num2 * 0.3f * num * num3;
			Md.Uv2(num4 * X.Cos(agR), num4 * X.Sin(agR), false);
			return DefCol;
		}

		public static bool fnRunDraw_worm_trap_player(EffectItem E)
		{
			int particleCount = E.getParticleCount(E, X.Mx(5, (E.time + 60 - 64) / 9 + 2));
			Map2d curMap = M2DBase.Instance.curMap;
			if (curMap == null || MTR.SqWormAnim0 == null)
			{
				return false;
			}
			int num = X.Abs(CAim._YD((int)E.z, 1));
			int num2 = X.Abs(CAim._XD((int)E.z, 1));
			MeshDrawer mesh = E.GetMesh("", curMap.Dgn.getChipMaterialActive(curMap, null, 1), false);
			mesh.Col = curMap.Dgn.getSpecificColor(MTRX.ColWhite);
			bool flag = false;
			for (int i = 0; i < particleCount; i++)
			{
				int num3 = (int)E.af - 9 * i;
				if (num3 < 0)
				{
					flag = true;
				}
				else
				{
					uint ran = X.GETRAN2(E.f0 % 7 + i * 13, i + E.f0 % 117);
					int num4 = 6 - (int)(X.RAN(ran, 826) * 3f);
					int num5 = num3 / num4;
					if (num5 < 9)
					{
						int num6 = (int)(X.RAN(ran, 1393) * 7f);
						PxlSequence pxlSequence;
						if (num6 >= 4)
						{
							num6 -= 4;
							pxlSequence = MTR.SqWormAnim1;
						}
						else
						{
							pxlSequence = MTR.SqWormAnim0;
						}
						float num7 = X.NI(-1, 1, X.RAN(ran, 2325)) * (float)(20 + num * 60);
						float num8 = X.NI(-1, 1, X.RAN(ran, 1169)) * (float)(20 + num2 * 70);
						float num9 = CAim.get_agR((AIM)E.z, -1.5707964f);
						PxlLayer layer = pxlSequence.getFrame(num6 * 9 + num5).getLayer(0);
						if (M2DBase.Instance.IMGS.initAtlasMd(mesh, layer.Img))
						{
							mesh.RotaGraph(num7 + layer.x, num8 - layer.y, curMap.base_scale, num9, null, X.RAN(ran, 1568) > 0.5f);
						}
						flag = true;
					}
				}
			}
			return flag;
		}

		public static bool fnRunDraw_flash(EffectItem E)
		{
			if (E.af >= E.y + E.x)
			{
				return false;
			}
			M2Camera cam = M2DBase.Instance.Cam;
			bool flag;
			MeshDrawer meshDrawer = E.EF.MeshInit("", cam.x / cam.CLEN, cam.y / cam.CLEN, MTRX.ColWhite, out flag, BLEND.NORMAL, false);
			meshDrawer.Col = EffectItem.Col1.Set((uint)(E.time | -16777216)).mulA(E.z).mulA((E.af < E.x) ? X.ZLINE(E.af, E.x) : (1f - X.ZLINE(E.af - E.x, E.y)))
				.C;
			meshDrawer.Rect(0f, 0f, IN.w + 640f, IN.h + 640f, false);
			return true;
		}

		public static bool fnRunDraw_radiation(EffectItem E)
		{
			if (E.af >= E.y + E.x)
			{
				return false;
			}
			M2Camera cam = M2DBase.Instance.Cam;
			bool flag;
			MeshDrawer meshDrawer = E.EF.MeshInit("", cam.x / cam.CLEN, cam.y / cam.CLEN, MTRX.ColWhite, out flag, BLEND.NORMAL, false);
			float num = ((E.af < E.x) ? X.ZLINE(E.af, E.x) : (1f - X.ZLINE(E.af - E.x, E.y)));
			meshDrawer.Col = EffectItem.Col1.Set((uint)(E.time | -16777216)).mulA(E.z).mulA(num)
				.C;
			MTR.DrRad.fine_intv = X.IntR(E.z);
			MTR.DrRad.min_len_ratio = X.NI(0.16f, 0.25f, num);
			MTR.DrRad.max_len_ratio = X.NI(0.3f, 0.34f, num);
			MTR.DrRad.drawTo(meshDrawer, 0f, 0f, IN.w, IN.h, 4f, cam.M2D.curMap.floort, false, 1f);
			return true;
		}

		public static bool fnRunDraw_flash_ui(EffectItem E)
		{
			if (E.af >= E.y + E.x)
			{
				return false;
			}
			bool flag;
			MeshDrawer meshDrawer = E.EF.MeshInit("", 0f, 0f, MTRX.ColWhite, out flag, BLEND.NORMAL, false);
			meshDrawer.Col = EffectItem.Col1.Set((uint)(E.time | -16777216)).mulA(E.z).mulA((E.af < E.x) ? X.ZLINE(E.af, E.x) : (1f - X.ZLINE(E.af - E.x, E.y)))
				.C;
			meshDrawer.Rect(0f, 0f, IN.w + 640f, IN.h + 640f, false);
			return true;
		}

		public static bool fnRunDraw_rectflash(EffectItem E)
		{
			if (E.af >= E.y + E.x)
			{
				return false;
			}
			M2Camera cam = M2DBase.Instance.Cam;
			bool flag;
			MeshDrawer meshDrawer = E.EF.MeshInit("", cam.x / cam.CLEN, cam.y / cam.CLEN, MTRX.ColWhite, out flag, BLEND.NORMAL, false);
			float num = ((E.af < E.x) ? X.ZLINE(E.af, E.x) : (1f - X.ZLINE(E.af - E.x, E.y)));
			meshDrawer.Col = meshDrawer.ColGrd.Set((uint)(E.time | -16777216)).mulA(E.z).mulA(num)
				.C;
			meshDrawer.ColGrd.setA(0f);
			X.NI(0.9f, 0.6f, num);
			float scaleRev = cam.getScaleRev();
			meshDrawer.Scale(scaleRev, scaleRev, false);
			meshDrawer.RectDoughnut(0f, 0f, IN.w + 40f, IN.h + 40f, 0f, 0f, IN.w * 0.6f, IN.h * 0.6f, false, 0f, 1f, false);
			meshDrawer.Identity();
			return true;
		}

		public static bool fnRunDraw_tb_flash_add(EffectItem E)
		{
			if (E.af >= E.y * 2f + E.x)
			{
				return false;
			}
			M2Camera cam = M2DBase.Instance.Cam;
			bool flag;
			MeshDrawer meshDrawer = E.EF.MeshInit("", cam.x / cam.CLEN, cam.y / cam.CLEN, MTRX.ColWhite, out flag, BLEND.ADD, false);
			float num = ((E.af < E.y) ? X.ZLINE(E.af, E.x) : ((E.af >= E.x + E.y) ? (1f - X.ZLINE(E.af - E.x - E.y, E.y)) : 1f));
			meshDrawer.Col = meshDrawer.ColGrd.Set((uint)(E.time | -16777216)).mulA(E.z).mulA(num)
				.C;
			meshDrawer.ColGrd.setA(0f);
			X.NI(0.9f, 0.6f, num);
			float scaleRev = cam.getScaleRev();
			meshDrawer.Scale(scaleRev, scaleRev, false);
			meshDrawer.RectBLGradation(-(IN.wh + 20f), IN.hh + 20f - 130f, IN.w + 40f, 130f, GRD.TOP2BOTTOM, false);
			meshDrawer.RectBLGradation(-(IN.wh + 20f), -(IN.hh + 20f), IN.w + 40f, 130f, GRD.BOTTOM2TOP, false);
			meshDrawer.Identity();
			return true;
		}

		public static bool fnRunDraw_spot_darken(EffectItem E)
		{
			float num = X.ZLINE(E.af, (float)E.time);
			float num2 = X.NI(0.3f, 1f, num);
			if (E.z < 0f)
			{
				if (E.z == -1f)
				{
					E.z = -E.af;
				}
				num *= 1f - X.ZLINE(E.af + E.z, (float)E.time);
				if (num <= 0f)
				{
					return false;
				}
			}
			MeshDrawer mesh = E.GetMesh("_spot", 4278190080U, BLEND.MUL, false);
			if (mesh.getTriMax() == 0)
			{
				mesh.base_z -= 0.5f;
			}
			M2Camera cam = M2DBase.Instance.Cam;
			float scaleRev = cam.getScaleRev();
			Vector3 posMainTransform = cam.PosMainTransform;
			mesh.base_x = posMainTransform.x;
			mesh.base_y = posMainTransform.y;
			M2Mover baseMover = cam.getBaseMover();
			float num3 = 0f;
			float num4 = 0f;
			if (baseMover != null)
			{
				float num5 = baseMover.Mp.map2globalux(baseMover.x);
				float num6 = baseMover.Mp.map2globaluy(baseMover.y);
				num3 = (num5 - posMainTransform.x) * 64f * scaleRev * num2;
				num4 = (num6 - posMainTransform.y) * 64f * scaleRev * num2;
			}
			mesh.Col = mesh.ColGrd.Set(1996488704).mulA(num).C;
			mesh.ColGrd.mulA(0f);
			float num7 = E.x * scaleRev;
			mesh.InnerCircle(0f, 0f, IN.w * 1.5f, IN.h * 1.5f, num3, num4, num7, num7, num7 * 0.5f, num7 * 0.5f, false, false, 0f, 1f);
			return true;
		}

		public static bool fnRunDraw_worm_egg_splash(EffectItem E)
		{
			if (E.af >= (float)E.time + 0.38f + 30f)
			{
				return false;
			}
			MeshDrawer meshImg = E.GetMeshImg("", MTR.MIiconL, BLEND.NORMAL, false);
			M2Camera cam = M2DBase.Instance.Cam;
			float scaleRev = cam.getScaleRev();
			Vector3 posMainTransform = cam.PosMainTransform;
			meshImg.base_x = posMainTransform.x;
			meshImg.base_y = posMainTransform.y;
			float num = IN.w - X.Abs(M2DBase.Instance.ui_shift_x);
			float h = IN.h;
			PxlSequence sqParticleSplash = MTR.SqParticleSplash;
			int num2 = sqParticleSplash.countFrames();
			for (int i = 0; i < 30; i++)
			{
				float num3 = E.af - (float)i * 0.38f;
				if (num3 >= 0f && num3 < (float)E.time)
				{
					float num4 = num3 / (float)E.time;
					uint ran = X.GETRAN2(E.f0 % 7 + i * 13, i + E.f0 % 117);
					Vector2 vector = X.RANBORDER(num, h, X.RAN(ran, 2600)) * scaleRev;
					float num5 = 50f * scaleRev * X.RAN(ran, 2055);
					float num6 = 50f * scaleRev * X.RAN(ran, 1606);
					float num7 = X.NI(1.5f, 2.5f, X.RAN(ran, 1814)) * scaleRev;
					PxlFrame frame = sqParticleSplash.getFrame((int)((ulong)ran % (ulong)((long)num2)));
					meshImg.Col = meshImg.ColGrd.Set(2864107419U).blend(2863707798U, X.RAN(ran, 2820)).mulA(1f - X.ZLINE(num4 - 0.5f, 0.5f))
						.C;
					meshImg.RotaPF(vector.x + num5, vector.y + num6, num7, num7, X.RAN(ran, 2768) * 6.2831855f, frame, false, false, false, uint.MaxValue, false, 0);
				}
			}
			return true;
		}

		public static bool fnRunDraw_magic_whittearrow_bow_remain(EffectItem E)
		{
			if (E.af >= (float)E.time)
			{
				return false;
			}
			float num = (EfParticle.tz = X.ZLINE(E.af, (float)E.time));
			MeshDrawer meshImg = E.GetMeshImg("", MTR.MIiconL, BLEND.ADD, false);
			MeshDrawer meshImg2 = E.GetMeshImg("", MTR.MIiconL, BLEND.SUB, false);
			meshImg.Scale(0.85f + X.ZSIN(num, 0.3f) * 0.15f, 1.2f - X.ZSIN(num, 0.6f) * 0.2f, false);
			EffectItemNel.drawWhiteArrowBow(null, null, E, 0f, 0f, E.z, X.ZLINE(num - 0.2f, 0.8f), true);
			MeshDrawer mesh = E.GetMesh("", C32.c2d(meshImg.Col), BLEND.ADD, false);
			mesh.setCurrentMatrix(meshImg.getCurrentMatrix(), false);
			mesh.MathLine(0f, -100f, 0f, 100f, 1f, delegate(float v)
			{
				float num2 = EfParticle.tz;
				float num3 = 1f - X.Abs(v - 0.5f) / 0.5f;
				return (-3f * (X.COSI(num2 + 0.2f, 0.143f) + X.COSI(num2 + 0.3f, 0.093f)) * X.ZSIN(num3) + X.COSI(v - 0.5f + num2 / 4f, 1.8f) * 4f * num3 - X.COSI(v - 0.3f + num2 / 6f, 2.7f) * 2f * num3) * (1f - X.ZSINV(num2));
			}, 6f, false, 0f, 0f);
			mesh.Identity();
			meshImg.Identity();
			meshImg2.Identity();
			return true;
		}

		public static bool drawWhiteArrowBow(MeshDrawer MdA, MeshDrawer MdS, EffectItem Ef, float sx, float sy, float aim_agR, float tz = 0f, bool no_reset_matrix = false)
		{
			if (MdA == null)
			{
				MdA = Ef.GetMeshImg("", MTR.MIiconL, BLEND.ADD, false);
				MdS = Ef.GetMeshImg("", MTR.MIiconL, BLEND.SUB, false);
			}
			float num = X.ZLINE(X.Mx(tz, 0f) - 0.07f + 0.035f * X.COSIT(7.83f) + 0.035f * X.COSIT(11.33f));
			MdA.Col = MdA.ColGrd.Set(4287365119U).blend(285252554U, num).C;
			MdS.Col = MdS.ColGrd.Set(4290285904U).blend(292319803U, num).C;
			PxlFrame pf = MTRX.getPF("whitearrow_bow");
			PxlFrame pf2 = MTRX.getPF("whitearrow_bow_blured");
			Matrix4x4 currentMatrix = MdA.getCurrentMatrix();
			MdA.TranslateP(sx + -28f, sy, false).Rotate(aim_agR, false);
			MdS.TranslateP(sx + -28f, sy, false).Rotate(aim_agR, false);
			MdA.RotaPF(0f, 0f, 1f, 1f, 0f, pf, false, false, false, uint.MaxValue, false, 0);
			MdS.RotaPF(0f, 0f, 1f, 1f, 0f, pf2, false, false, false, uint.MaxValue, false, 0);
			if (tz < 0f)
			{
				MdA.Col = MdA.ColGrd.mulA(-tz).C;
				MdA.RotaPF(0f, 0f, 1f, 1f, 0f, pf2, false, false, false, uint.MaxValue, false, 0);
				MdA.RotaPF(0f, 0f, 1f, 1f, 0f, pf2, false, false, false, uint.MaxValue, false, 0);
				MdA.RotaPF(0f, 0f, 1f, 1f, 0f, pf2, false, false, false, uint.MaxValue, false, 0);
			}
			if (!no_reset_matrix)
			{
				MdA.setCurrentMatrix(currentMatrix, false);
				MdS.setCurrentMatrix(currentMatrix, false);
			}
			return true;
		}

		public static bool drawWhiteArrowArrow(MeshDrawer MdA, MeshDrawer MdS, EffectItem Ef, float sx, float sy, float aim_agR, float tz = 0f, bool no_reset_matrix = false, float tz_speed = 0f)
		{
			if (MdA == null)
			{
				MdA = Ef.GetMeshImg("", MTR.MIiconL, BLEND.ADD, false);
				MdS = Ef.GetMeshImg("", MTR.MIiconL, BLEND.SUB, false);
			}
			X.ZLINE(X.Mx(tz, 0f) - 0.07f + 0.035f * X.COSIT(7.83f) + 0.035f * X.COSIT(11.33f));
			if (tz_speed > 0f)
			{
				float num = X.NI(1.2f, 1f, tz_speed);
				MdA.Scale(num, 1f, false);
				MdS.Scale(num, 1f, false);
			}
			Matrix4x4 currentMatrix = MdA.getCurrentMatrix();
			MdA.Rotate(aim_agR, false).TranslateP(sx, sy, false);
			MdS.Rotate(aim_agR, false).TranslateP(sx, sy, false);
			MdA.Col = MdA.ColGrd.Set(4287365119U).blend(1140890570U, tz).C;
			MdS.Col = MdS.ColGrd.Set(4290285904U).blend(1147957819U, tz).C;
			PxlFrame pf = MTRX.getPF((tz_speed == 0f) ? "whitearrow_arrow" : "whitearrow_arrow_float");
			PxlFrame pf2 = MTRX.getPF("whitearrow_arrow_blured");
			MdA.RotaPF(0f, 0f, 1f, 1f, 0f, pf, false, false, false, uint.MaxValue, false, 0);
			MdS.RotaPF(0f, 0f, 1f, 1f, 0f, pf2, false, false, false, uint.MaxValue, false, 0);
			if (tz < 0f)
			{
				MdA.Col = MdA.ColGrd.mulA(-tz).C;
				MdA.RotaPF(0f, 0f, 1f, 1f, 0f, pf2, false, false, false, uint.MaxValue, false, 0);
				MdA.RotaPF(0f, 0f, 1f, 1f, 0f, pf2, false, false, false, uint.MaxValue, false, 0);
				MdA.RotaPF(0f, 0f, 1f, 1f, 0f, pf2, false, false, false, uint.MaxValue, false, 0);
			}
			if (!no_reset_matrix)
			{
				MdA.setCurrentMatrix(currentMatrix, false);
				MdS.setCurrentMatrix(currentMatrix, false);
			}
			return true;
		}

		public static bool fnRunDraw_arrow_afterimage(EffectItem E)
		{
			if (E.af >= (float)E.time)
			{
				return false;
			}
			float num = 1f - E.af / (float)E.time;
			num *= num;
			MeshDrawer meshDrawer = NelM2DBase.PostEffectMesh(MTR.MtrShiftImage, E, true);
			meshDrawer.Rotate(E.z, false).allocUv2(160, false);
			for (int i = 0; i < 20; i++)
			{
				uint ran = X.GETRAN2(E.f0 + i, 4 + i % 13);
				float num2 = 24f + X.RAN(ran, 1555) * 45f;
				float num3 = (float)((int)(num2 / 2f));
				float num4 = -num3 - 40f + 80f * X.RAN(ran, 1130) + 10f;
				float num5 = -120f + 240f * X.RAN(ran, 1874);
				float num6 = 2f + 4f * X.RAN(ran, 1583);
				float num7 = (float)X.IntR((-18f + 36f * X.RAN(ran, 2613)) * num);
				float num8 = X.Cos(E.z) * num7 * 0.015625f;
				float num9 = X.Sin(E.z) * num7 * 0.015625f;
				meshDrawer.Uv2(num8, num9, false).Uv2(num8, num9, false);
				meshDrawer.Line(num4, num5, num4 + num3, num5, num6, false, 0f, 1f);
				meshDrawer.Line(num4 + num3, num5, num4 + num2, num5, num6, false, 1f, 0f);
			}
			meshDrawer.Identity();
			return true;
		}

		public static bool fnRunDraw_fake_layer_dissolve(EffectItem E)
		{
			M2DBase instance = M2DBase.Instance;
			Map2d curMap = instance.curMap;
			if (X.BTW(0f, (float)E.time, (float)curMap.count_layers) && E.af >= E.y)
			{
				return false;
			}
			float num = E.af / E.y;
			if (E.z == 0f)
			{
				num = 1f - num;
			}
			M2MapLayer layer = curMap.getLayer(E.time);
			int count_chips = layer.count_chips;
			MeshDrawer mesh = E.GetMesh("", curMap.Dgn.DGN.MtrFakeLayerDissolve, false);
			mesh.base_x = (mesh.base_y = 0f);
			mesh.Col = mesh.ColGrd.Set(layer.LayerColor).mulA(num).C;
			MImage michip = curMap.M2D.MIchip;
			for (int i = 0; i < count_chips; i++)
			{
				M2Puts chipByIndex = layer.getChipByIndex(i);
				if (chipByIndex == null)
				{
					break;
				}
				if (instance.Cam.isCoveringMp((float)chipByIndex.drawx * curMap.rCLEN, (float)chipByIndex.drawy * curMap.rCLEN, (float)(chipByIndex.drawx + chipByIndex.rwidth) * curMap.rCLEN, (float)(chipByIndex.drawy + chipByIndex.rheight) * curMap.rCLEN, 6f))
				{
					chipByIndex.draw(mesh, 1, 0f, 0f, false);
				}
			}
			return true;
		}

		public static bool fnRunDraw_debug_gameover_title(EffectItem E)
		{
			return false;
		}

		public static bool fnRunDraw_bite_triangle_ui(EffectItem E)
		{
			return EffectItemNel.drawBiteTriangle(E, (float)E.time, 82f, -2.4f, 14);
		}

		public static bool fnRunDraw_bite_triangle(EffectItem E)
		{
			return EffectItemNel.drawBiteTriangle(E, (float)E.time, 18f, -0.7f, 8);
		}

		private static bool drawBiteTriangle(EffectItem E, float maxt, float tri_hen, float margin, int dot_count)
		{
			if (E.af >= maxt)
			{
				return false;
			}
			float num = X.ZLINE(E.af, maxt);
			uint num2 = X.GETRAN2(E.f0 % 17, (int)(E.index % 13U));
			bool flag = num2 % 2U == 1U;
			bool flag2 = flag;
			int num3 = (int)X.NI(5, 9, X.RAN(num2, 2012));
			float num4 = -(tri_hen + margin) * (float)(num3 - 1) * 0.5f;
			MeshDrawer meshDrawer;
			if (num < 0.125f)
			{
				meshDrawer = E.GetMesh("", uint.MaxValue, BLEND.NORMAL, false);
				meshDrawer.Col = meshDrawer.ColGrd.Set(4281532416U).blend(4294901760U, X.ZPOW(num, 0.15f)).C;
			}
			else
			{
				meshDrawer = E.GetMesh("", uint.MaxValue, BLEND.SUB, false);
				meshDrawer.Col = meshDrawer.ColGrd.Set(4284645258U).blend(570821646U, X.ZLINE(num, 0.2f) * 0.66f + X.ZSIN(num - 0.2f, 0.8f) * 0.34f).C;
			}
			meshDrawer.Rotate(E.z, false);
			float num5 = -margin * 0.6f * X.ZLINE(num, 0.15f) + (margin * 2f + tri_hen * 1.25f) * X.ZSIN(num - 0.15f, 0.85f);
			for (int i = 0; i < num3; i++)
			{
				num2 = X.GETRAN2((int)(num2 % 255U + num2 / 13U + (uint)i), (int)(num2 % 12U + 3U));
				float num6 = num4 + (tri_hen + margin) * (float)i;
				float num7 = (flag ? num5 : (-num5));
				float num8 = X.ZLINE(num - 0.03f, X.NI(0.02f, 0.3f, X.RAN(num2, 2010)));
				float num9 = 0f;
				if (num8 < 1f)
				{
					num9 = X.COSI(tri_hen + X.RAN(num2, 999), 0.066f) * X.NI(0.12f, 0.23f, X.RAN(num2, 1982)) * 3.1415927f * (X.ZLINE(num8, 0.33f) - X.ZPOW(num8 - 0.33f, 0.66999996f));
				}
				meshDrawer.Poly(num6, num7, tri_hen / 1.7320508f * 2f, (flag ? (-1.5707964f) : 1.5707964f) + num9, 3, 0f, false, 0f, 0f);
				flag = !flag;
			}
			flag = flag2;
			int num10 = (num3 + 1) * 11;
			int num11 = 0;
			float num12 = X.ZSIN(num, 0.3f) * 0.75f + X.ZSIN(num - 0.12f, 0.88f) * 0.25f;
			for (int j = 0; j < num10; j++)
			{
				num2 = X.GETRAN2((int)(num2 % 255U + num2 / 13U + (uint)j), (int)(num2 % 12U + 3U));
				float num13 = X.RAN(num2, 1415);
				float num14 = num4 + (tri_hen + margin) * (float)num11 + tri_hen * 0.5f * (-1f + num13);
				float num15 = 0.8660254f * tri_hen * (0.5f - num13) * (float)X.MPF(flag);
				float num16 = 6.2831855f * X.RAN(num2, 2957);
				float num17 = num12 * tri_hen * X.NI(1.5f, 2f, X.RAN(num2, 2448));
				meshDrawer.Rect(num14 + num17 * X.Cos(num16), num15 + num17 * X.Sin(num16), 1f, 1f, false);
				if (j % dot_count == dot_count - 1)
				{
					num11++;
					flag = !flag;
				}
			}
			meshDrawer.Identity();
			return true;
		}

		public static bool fnRunDraw_pr_wheel_cyclone(EffectItem Ef)
		{
			return EffectItemNel.draw_pr_wheel_cyclone(Ef, false, 20f, 25f);
		}

		public static bool fnRunDraw_pr_wheel_cyclone_b(EffectItem Ef)
		{
			return EffectItemNel.draw_pr_wheel_cyclone(Ef, true, 38f, 43f);
		}

		private static bool draw_pr_wheel_cyclone(EffectItem Ef, bool is_sub, float height_min, float height_max)
		{
			float num = X.ZLINE(Ef.af, 22f);
			if (num >= 1f)
			{
				return false;
			}
			AttackGhostDrawer drGhost = MTR.DrGhost;
			drGhost.bezier_agR_hrhb = 0.15707964f;
			float num2 = 102.4f;
			drGhost.count = 2;
			drGhost.PosPx(num2, 0f, -num2, 0f, 0f, -num2, -3.1415927f);
			drGhost.BezierExtendPx(num2 * 0.6f, -1000f).ProgressPx(0f, -12f);
			drGhost.RandomizePx(9f, 9f, 4f, 4f, 0f);
			drGhost.HeightPx(20f, 25f);
			drGhost.time_center = 0.5f;
			MeshDrawer mesh = Ef.GetMesh("", is_sub ? MTR.MtrNDSub : MTR.MtrNDAdd, is_sub);
			mesh.Scale((float)X.MPF(Ef.z > 0f), 1f, false);
			uint num3 = (uint)((ulong)(-16777216) | (ulong)((long)Ef.time));
			drGhost.Col(num3, num3);
			drGhost.drawTo(mesh, 0f, 0f, num, EffectItemNel.FD_GhostDrawerUv);
			mesh.Rotate(3.1415927f, false);
			drGhost.drawTo(mesh, 0f, 0f, num, EffectItemNel.FD_GhostDrawerUv);
			return true;
		}

		public static bool fnRunDraw_ui_dmg_cut(EffectItem Ef)
		{
			float num = X.ZLINE(Ef.af, (float)Ef.time);
			if (num >= 1f)
			{
				return false;
			}
			AttackGhostDrawer drGhost = MTR.DrGhost;
			drGhost.bezier_agR_hrhb = 0.15707964f;
			drGhost.count = 3;
			drGhost.PosPx(100f, 0f, -100f, 0f, 20f, -20f, -3.1415927f);
			drGhost.BezierExtendPx(60f, 70f).ProgressPx(-20f, 80f);
			drGhost.RandomizePx(45f, 15f, 60f, 45f, 0f);
			drGhost.HeightPx(6f, 19f);
			drGhost.time_center = 0.03125f;
			MeshDrawer mesh = Ef.GetMesh("", MTR.MtrNDSub, false);
			mesh.Scale(2.4f, 2.4f * (float)X.MPF(Ef.z < 6.2831855f), false).Rotate(Ef.z, false);
			drGhost.Col(4278235886U, 4281310246U, 0U, 0U);
			drGhost.drawTo(mesh, 0f, 0f, num, EffectItemNel.FD_GhostDrawerUv);
			return true;
		}

		private static bool fnAgdDraw_magicslice(EffectItem Ef, AttackGhostDrawer _Agd)
		{
			float num = X.ZLINE(Ef.af, (float)Ef.time);
			if (num >= 1f)
			{
				return false;
			}
			AttackGhostDrawer attackGhostDrawer = MTR.DrGhostEn.copyFrom(_Agd);
			EffectItemNel.drawMagicSlicerEnemyAGD(Ef, attackGhostDrawer, 0f, 0f, num);
			return true;
		}

		private static void drawMagicSlicerEnemyAGD(EffectItem Ef, AttackGhostDrawer AGD, float x, float y, float tz)
		{
			MeshDrawer meshDrawer;
			MeshDrawer meshDrawer2;
			EffectItemNel.PrepareMeshForMagicSlicerEnemyAGD(Ef, out meshDrawer, out meshDrawer2);
			EffectItemNel.drawMagicSlicerEnemyAGD(Ef, meshDrawer, meshDrawer2, AGD, x, y, tz, 4289853674U, 4279530364U, 4294452465U, 4287561866U);
		}

		private static void PrepareMeshForMagicSlicerEnemyAGD(EffectItem Ef, out MeshDrawer MdS, out MeshDrawer MdA)
		{
			MdA = Ef.GetMesh("", MTR.MtrNDAdd, false);
			MdS = Ef.GetMesh("magic_slicer_sub", MTR.MtrNDSub, false);
			if (MdS.getTriMax() == 0)
			{
				MdS.base_z -= 0.05f;
			}
			if (Ef.z != 1f)
			{
				float num = X.Abs(Ef.z);
				MdS.Scale(Ef.z, num, false);
				MdA.Scale(Ef.z, num, false);
			}
		}

		private static void drawMagicSlicerEnemyAGD(EffectItem Ef, MeshDrawer MdS, MeshDrawer MdA, AttackGhostDrawer AGD, float x, float y, float tz, uint subcol, uint subcol1, uint addcol, uint addcol1)
		{
			AGD.Col(subcol, subcol1);
			AGD.ran0 = (int)(X.GETRAN2(Ef.f0 + 113, 4) & 16777215U);
			AGD.drawTo(MdS, x, y, tz, EffectItemNel.FD_GhostDrawerUv);
			AGD.Col(addcol, addcol1);
			AGD.ran0 = (int)(X.GETRAN2(Ef.f0 + 123, 3) & 16777215U);
			AGD.drawTo(MdA, x, y, tz, EffectItemNel.FD_GhostDrawerUv);
		}

		private static bool fnAgdDraw_bossslice(EffectItem Ef, AttackGhostDrawer _Agd)
		{
			float num = X.ZLINE(Ef.af, (float)Ef.time);
			if (num >= 1f)
			{
				return false;
			}
			AttackGhostDrawer attackGhostDrawer = MTR.DrGhostEn.copyFrom(_Agd);
			EffectItemNel.drawBossSlicerEnemyAGD(Ef, attackGhostDrawer, 0f, 0f, num);
			return true;
		}

		private static void drawBossSlicerEnemyAGD(EffectItem Ef, AttackGhostDrawer AGD, float x, float y, float tz)
		{
			MeshDrawer meshDrawer;
			MeshDrawer meshDrawer2;
			EffectItemNel.PrepareMeshForMagicSlicerEnemyAGD(Ef, out meshDrawer, out meshDrawer2);
			EffectItemNel.drawMagicSlicerEnemyAGD(Ef, meshDrawer, meshDrawer2, AGD, x, y, tz, 4289853674U, 4279530364U, 4292677391U, 4288903288U);
		}

		private static bool fnAgdDraw_normalEnemy(EffectItem Ef, AttackGhostDrawer _Agd)
		{
			float num = X.ZLINE(Ef.af, (float)Ef.time);
			if (num >= 1f)
			{
				return false;
			}
			AttackGhostDrawer attackGhostDrawer = MTR.DrGhostEn.copyFrom(_Agd);
			MeshDrawer meshDrawer;
			MeshDrawer meshDrawer2;
			EffectItemNel.PrepareMeshForNormalSlicerEnemyAGD(Ef, out meshDrawer, out meshDrawer2);
			if (Ef.z != 1f)
			{
				float num2 = X.Abs(Ef.z);
				meshDrawer.Scale(Ef.z, num2, false);
				meshDrawer2.Scale(Ef.z, num2, false);
			}
			EffectItemNel.drawNormalSlicerEnemyAGD(Ef, meshDrawer, meshDrawer2, attackGhostDrawer, 0f, 0f, num);
			return true;
		}

		private static void PrepareMeshForNormalSlicerEnemyAGD(EffectItem Ef, out MeshDrawer MdN, out MeshDrawer MdA)
		{
			MdA = Ef.GetMesh("", MTR.MtrNDAdd, false);
			MdN = Ef.GetMesh("", MTR.MtrNDNormal, false);
		}

		private static void drawNormalSlicerEnemyAGD(EffectItem Ef, MeshDrawer MdN, MeshDrawer MdA, AttackGhostDrawer AGD, float x, float y, float tz)
		{
			AGD.Col(4288585374U, 1428300322U);
			AGD.ran0 = (int)(X.GETRAN2(Ef.f0 + 83, 4) & 16777215U);
			AGD.drawTo(MdN, x, y, tz, EffectItemNel.FD_GhostDrawerUv);
			AGD.Col(4284561464U, 4278190080U);
			AGD.ran0 = (int)(X.GETRAN2(Ef.f0 + 91, 3) & 16777215U);
			AGD.drawTo(MdA, x, y, tz, EffectItemNel.FD_GhostDrawerUv);
		}

		private static bool fnAgdDraw_normalEnemyRotated(EffectItem Ef, AttackGhostDrawer _Agd)
		{
			float num = X.ZLINE(Ef.af, 25f);
			if (num >= 1f)
			{
				return false;
			}
			AttackGhostDrawer attackGhostDrawer = MTR.DrGhostEn.copyFrom(_Agd);
			MeshDrawer meshDrawer;
			MeshDrawer meshDrawer2;
			EffectItemNel.PrepareMeshForNormalSlicerEnemyAGD(Ef, out meshDrawer, out meshDrawer2);
			float num2 = (float)Ef.time / 100f;
			meshDrawer.Scale(num2, num2, false).Rotate(Ef.z, false);
			meshDrawer2.setCurrentMatrix(meshDrawer.getCurrentMatrix(), false);
			EffectItemNel.drawNormalSlicerEnemyAGD(Ef, meshDrawer, meshDrawer2, attackGhostDrawer, 0f, 0f, num);
			return true;
		}

		public static bool GhostDrawerUv(MeshDrawer MdR, bool first, uint ran, float scale_val, float inner_val = -1000f, float outer_val = -1f, float level_threshold = -1000f, int timex = 120, int timey = 133)
		{
			if (first)
			{
				MdR.allocUv2(128, false);
				MdR.allocUv3(128, false);
			}
			else
			{
				MdR.Uv2(X.RAN(ran, 995) + X.ANMPT(timex, 1f), X.RAN(ran, 769) + X.ANMPT(timey, 1f), false);
				MdR.allocUv2(0, true);
				int num = MdR.getUv3Max() + 1;
				int num2 = MdR.getVertexMax() - 1;
				if (num < num2)
				{
					MdR.Uv3(scale_val, (level_threshold == -1000f) ? scale_val : level_threshold, false);
					if (inner_val == -1000f)
					{
						MdR.allocUv3(0, true);
					}
					else
					{
						int num3 = 0;
						for (int i = num; i < num2; i++)
						{
							if (num3 == 1)
							{
								MdR.Uv3(-0.4f, -0.4f, false);
							}
							else
							{
								MdR.Uv3(-0.12f, -0.12f, false);
							}
							num3 = 1 - num3;
						}
						MdR.Uv3(scale_val, scale_val, false);
					}
				}
				else
				{
					MdR.allocUv3(0, true);
				}
			}
			return true;
		}

		public static bool fnRunDraw_dungeon_rain(EffectItem Ef)
		{
			if (Ef.z <= 0f)
			{
				return false;
			}
			MeshDrawer mesh = Ef.GetMesh("", uint.MaxValue, BLEND.ADD, true);
			M2DBase instance = M2DBase.Instance;
			mesh.base_x = instance.ux2effectScreenx(instance.curMap.pixel2ux(instance.Cam.x));
			mesh.base_y = instance.uy2effectScreeny(instance.curMap.pixel2uy(instance.Cam.y));
			mesh.Col = mesh.ColGrd.Set(2572838248U).mulA(Ef.z).C;
			int num = 24;
			float num2 = (IN.h + 1.40625f + 72f) * 0.015625f;
			float num3 = (IN.hh + 1.40625f + 70f) * 0.015625f;
			float num4 = (-IN.wh - 1.40625f) * 0.015625f;
			float num5 = (IN.w + 1.40625f) * 0.015625f;
			for (int i = 0; i < num; i++)
			{
				int num6 = (int)Ef.af - 10 * i;
				if (num6 >= 0)
				{
					int num7 = num6 / 70;
					float num8 = (float)(num6 % 70) / 70f;
					uint ran = X.GETRAN2(num7 * 8 + i * 13, i + num7 % 37);
					float num9 = num3 - num2 * num8;
					float num10 = num4 + num5 * X.RAN(ran, 2078) - num2 * 0.26666668f * num8;
					mesh.Tri(0, 2, 1, false).Tri(3, 1, 2, false);
					mesh.Pos(num10, num9, null).Pos(num10 - 0.015625f, num9, null).Pos(num10 - 0.375f, num9 - 1.40625f, null)
						.Pos(num10 - 0.375f - 0.015625f, num9 - 1.40625f, null);
				}
			}
			return true;
		}

		public static bool draw_rainfoot_puddle(EffectItem Ef, Color32 Col, BLEND blend, float scale, float eaf, int f0, int index, bool water_puddle = false)
		{
			MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, blend, false);
			return EffectItemNel.draw_rainfoot_puddle(Ef, meshImg, Col, scale, eaf, f0, index, water_puddle);
		}

		public static bool draw_rainfoot_puddle(EffectItem Ef, MeshDrawer Md, Color32 Col, float scale, float eaf, int f0, int index, bool water_puddle = false)
		{
			int num = (int)(eaf / 4f);
			if (num >= 4)
			{
				return false;
			}
			uint ran = X.GETRAN2(index + f0, 3 + index % 7);
			uint num2 = (water_puddle ? (4U + ran % 2U) : (ran % 4U));
			PxlFrame pxlFrame = MTR.ARainFoot[(long)((ulong)(4U * num2) + (ulong)((long)num))];
			Md.Col = Md.ColGrd.Set(Col).mulA(1f - X.ZPOW(eaf, 16f)).C;
			Md.RotaPF(0f, 0f, scale, scale, 0f, pxlFrame, X.RAN(ran, 1299) < 0.5f, false, false, uint.MaxValue, false, 0);
			return true;
		}

		public static bool fnRunDraw_mover_stunned(EffectItem Ef)
		{
			if (Ef.af >= 400f)
			{
				return false;
			}
			MeshDrawer mesh = Ef.GetMesh("", uint.MaxValue, BLEND.NORMAL, false);
			uint ran = X.GETRAN2((int)(Ef.index + (uint)Ef.f0), (int)(3U + Ef.index % 7U));
			float num = X.RAN(ran, 2850) * 6.2831855f + Ef.af / X.NI(100, 111, X.RAN(ran, 415)) * 6.2831855f;
			float num2 = 2.0943952f;
			float num3 = X.Mx(X.Mx(Ef.z, (float)Ef.time) / 4f, 4f);
			float num4 = (1f + 0.1f * X.COSI(X.RAN(ran, 2591) * 100f + Ef.af, 90f + X.RAN(ran, 2088) * 30f)) * Ef.z;
			mesh.Col = mesh.ColGrd.White().blend(4294962338U, 0.5f + 0.5f * X.COSI(X.RAN(ran, 1258) * 40f + Ef.af, 38f)).C;
			float num5 = 0.57595867f;
			for (int i = 0; i < 3; i++)
			{
				mesh.Star(num4 * X.Cos(num), (float)(Ef.time + 20) + num4 * X.Sin(num) * 0.12f, num3, 1.5707964f, 4, 0.3f, 0f, false, 0f, 0f);
				mesh.Arc2(0f, (float)(Ef.time + 20), num4, num4 * 0.12f, num - num5, num + num5, 2f, 0f, 0f);
				num += num2;
			}
			return true;
		}

		public static bool fnRunDraw_shield_bush(EffectItem Ef)
		{
			float num = 1f;
			if (Ef.af >= (float)Ef.time)
			{
				return false;
			}
			if (Ef.af < 12f)
			{
				num = X.ZSIN2(Ef.af, 12f);
			}
			else if (Ef.af >= (float)(Ef.time - 23))
			{
				num = X.ZPOW((float)Ef.time - Ef.af, 20f);
			}
			MeshDrawer mesh = Ef.GetMesh("", uint.MaxValue, BLEND.ADD, false);
			float num2 = Ef.af / 55f * 180f;
			X.NI(1f, 0.5f, num);
			float num3 = M2Shield.octa_w * 2f * 0.015625f;
			float num4 = num3 * 0.5f;
			float num5 = num4 * 1.7320508f * 0.5f;
			float num6 = -num4 * 0.5f;
			float num7 = num4 * X.NI(1f, 1.5f, num);
			float num8 = X.NI(0f, num7 * 0.875f, num);
			float num9 = num3 * X.NI(0f, 0.25f, num * num);
			float num10 = M2Shield.octa_h / M2Shield.octa_w;
			float num11 = num5 / 1.4142135f;
			Matrix4x4 matrix4x = Matrix4x4.Translate(new Vector3(0f, num9 + num4 * num10 * 0.5f, 0f));
			Matrix4x4 matrix4x2 = Matrix4x4.Translate(new Vector3(0f, 0.5f * (2f * num4 - num5) * num10 * num, 0f)) * matrix4x;
			Matrix4x4 matrix4x3 = Matrix4x4.Scale(new Vector3(1f, -1f, 1f));
			Matrix4x4 matrix4x4 = Matrix4x4.Scale(new Vector3(1f, num10, 1f));
			Matrix4x4 matrix4x5 = Matrix4x4.Scale(new Vector3(0.75f, 0.75f, 1f));
			Color32 c = mesh.ColGrd.White().blend(4286697983U, num).mulA((Ef.af < Ef.z) ? 1f : 0.25f)
				.C;
			Color32 c2 = mesh.ColGrd.Set(4286697983U).mulA(1f - num * 0.375f + 0.03125f * X.COSIT(23f) + 0.03125f * X.COSIT(8.4f) - ((Ef.af < Ef.z) ? 0f : 0.5f)).C;
			float num12 = X.NI(3, 1, X.ZPOW(num)) * 0.015625f;
			for (int i = 0; i < 4; i++)
			{
				Matrix4x4 matrix4x6 = matrix4x5;
				int num13 = i % 2;
				Matrix4x4 matrix4x7 = matrix4x6 * matrix4x * Matrix4x4.Rotate(Quaternion.Euler(0f, num2, 0f));
				float num14 = 0f;
				float num15 = 0f;
				int num16 = i % 2;
				Matrix4x4 matrix4x8 = matrix4x7 * Matrix4x4.Rotate(Quaternion.Euler(num14, num15, 0f)) * matrix4x4;
				for (int j = 0; j < 2; j++)
				{
					Matrix4x4 matrix4x9 = ((j == 0) ? matrix4x8 : (matrix4x3 * matrix4x8));
					mesh.Col = c2;
					mesh.Tri(0, 1, 2, false).Tri(0, 2, 1, false);
					Vector3 vector = matrix4x9.MultiplyPoint3x4(new Vector3(0f, num4, -num8));
					mesh.Pos(vector.x, vector.y, null);
					Vector3 vector2 = matrix4x9.MultiplyPoint3x4(new Vector3(num11, num6, -num7));
					mesh.Pos(vector2.x, vector2.y, null);
					Vector3 vector3 = matrix4x9.MultiplyPoint3x4(new Vector3(-num11, num6, -num7));
					mesh.Pos(vector3.x, vector3.y, null);
					mesh.Col = c;
					mesh.Line(vector2.x, vector2.y, vector3.x, vector3.y, num12, true, 0f, 0f);
					mesh.Line(vector3.x, vector3.y, vector.x, vector.y, num12, true, 0f, 0f);
					mesh.Line(vector.x, vector.y, vector2.x, vector2.y, num12, true, 0f, 0f);
				}
				num2 += 90f;
			}
			return true;
		}

		public static bool fnRunDraw_ser_decline(EffectItem Ef)
		{
			int num = X.IntC(Mathf.Log((float)Ef.time) / 0.6931472f);
			int num2 = Ef.time;
			float num3 = X.ZLINE(Ef.af, 80f);
			if (num3 >= 1f)
			{
				return false;
			}
			MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, false);
			float num4 = 1.25f - 0.25f * X.ZLINE(Ef.af, 15f);
			meshImg.Scale(num4, num4, false);
			int vertexMax = meshImg.getVertexMax();
			int num5 = 0;
			for (int i = 0; i <= num; i++)
			{
				if ((num2 & (1 << i)) != 0)
				{
					num2 &= ~(1 << i);
					float num6 = X.ZLINE(Ef.af + 1f - (float)(num5 * 12), 30f);
					if (num6 == 0f)
					{
						meshImg.ColGrd.White();
					}
					else
					{
						meshImg.ColGrd.Set(4291758859U).blend(uint.MaxValue, num6);
					}
					meshImg.Col = meshImg.ColGrd.mulA(1f - num3).C;
					meshImg.RotaPF((float)(num5 * 20), 0f, 2f, 2f, 0f, MTRX.AUiSerIcon[i], false, false, false, uint.MaxValue, false, 0);
					num5++;
					if (num2 == 0)
					{
						break;
					}
				}
			}
			float num7 = (-num4 * ((float)(num5 * 20) * 0.5f) + 15f * X.COSI(Ef.af, 33f) * (1f - X.ZLINE(Ef.af - 20f, 40f))) * 0.015625f;
			int vertexMax2 = meshImg.getVertexMax();
			Vector3[] vertexArray = meshImg.getVertexArray();
			for (int j = vertexMax; j < vertexMax2; j++)
			{
				Vector3[] array = vertexArray;
				int num8 = j;
				array[num8].x = array[num8].x + num7;
			}
			return true;
		}

		public static bool fnRunDraw_ser_awake(EffectItem Ef)
		{
			float num = 1f - X.ZLINE(Ef.af - (float)Ef.time, (float)Ef.time);
			if (num <= 0f)
			{
				return false;
			}
			MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, false);
			meshImg.Col = C32.MulA((X.ANM((int)Ef.af, 2, 4f) == 0) ? uint.MaxValue : 4294916152U, num);
			meshImg.RotaPF(0f, Ef.z, 2f, 2f, 0f, MTRX.getPF("bikkuri"), false, false, false, uint.MaxValue, false, 0);
			return true;
		}

		public static bool fnRunDraw_powerbomb_explode(EffectItem Ef)
		{
			float num = 1f - X.ZLINE(Ef.af - (float)Ef.time, 30f);
			if (num <= 0f)
			{
				return false;
			}
			float num2 = X.ZLINE(Ef.af, 45f);
			float num3 = X.ZPOW(Ef.af, Ef.z);
			return EffectItemNel.powerbomb_explode_execute(Ef, num, num2, num3);
		}

		public static bool powerbomb_explode_execute(EffectItem Ef, float alp, float blur_alp, float tz)
		{
			MeshDrawer mesh = Ef.GetMesh("", MTR.MtrPowerbomb, false);
			float base_x = mesh.base_x;
			float base_y = mesh.base_y;
			M2Camera cam = M2DBase.Instance.Cam;
			Vector3 posMainTransform = cam.PosMainTransform;
			float num = cam.getScaleRev() * 64f;
			mesh.allocUv2(64, false).Uv2((mesh.base_x - posMainTransform.x) * num / M2Camera.texture_w_with_margin, (mesh.base_y - posMainTransform.y) * num / M2Camera.texture_h_with_margin, false);
			mesh.base_x = posMainTransform.x;
			mesh.base_y = posMainTransform.y;
			mesh.initForImg(MTRX.IconWhite, 0);
			if (tz < 1f)
			{
				mesh.Col = mesh.ColGrd.Set1(blur_alp, 0f, 0f, alp).C;
				mesh.Rect(0f, 0f, M2Camera.texture_w_with_margin, M2Camera.texture_h_with_margin, false);
				mesh.Col = mesh.ColGrd.Set1(blur_alp, 1f, 0f, alp).C;
				mesh.initForImg(MTRX.EffBlurCircle245, 0);
				mesh.base_x = base_x;
				mesh.base_y = base_y;
				tz *= tz;
				float num2 = 2200f * tz;
				mesh.Rect(0f, 0f, num2, num2, false);
				mesh.initForImg(MTRX.EffCircle128, 0);
				num2 = 1400f * tz;
				mesh.Rect(0f, 0f, num2, num2, false);
			}
			else
			{
				float num3 = X.ZLINE(X.Mx(X.Abs(base_x - posMainTransform.x) * 64f / (M2Camera.texture_w_with_margin * 0.5f), X.Abs(base_y - posMainTransform.y) * 64f / (M2Camera.texture_h_with_margin * 0.5f)) - 0.55f, 0.23f);
				mesh.Col = mesh.ColGrd.Set1(blur_alp * (1f - num3 * 0.5f), 1f - num3 * 0.75f, 0f, alp).C;
				mesh.Rect(0f, 0f, M2Camera.texture_w_with_margin, M2Camera.texture_h_with_margin, false);
			}
			mesh.allocUv2(0, true);
			return true;
		}

		public static bool fnRunDraw_flashbang_self_explode(EffectItem Ef)
		{
			float num = 1f;
			if (Ef.af >= Ef.z)
			{
				num = 0.25f - 0.25f * X.ZLINE(Ef.af - Ef.z, (float)Ef.time);
				if (num <= 0f)
				{
					return false;
				}
			}
			return EffectItemNel.powerbomb_explode_execute(Ef, num, 1f, 1f);
		}

		public static bool fnRunDraw_post_powerbomb_sphere(EffectItem Ef)
		{
			float num = 1f - X.ZLINE(Ef.af - (float)Ef.time, 20f);
			if (num <= 0f)
			{
				return false;
			}
			MeshDrawer meshDrawer = NelM2DBase.PostEffectMesh(MTR.MtrShiftImage, Ef, true);
			float num2 = 170f * (0.5f + X.ZPOWN(Ef.af, (float)Ef.time, 3f));
			EffectItemNel.postsphere_wu = X.ZSIN(Ef.af + 20f, (float)(Ef.time + 20)) * 0.78f;
			EffectItemNel.postsphere_hu = num2 * 0.015625f;
			meshDrawer.Col = meshDrawer.ColGrd.Black().mulA(num).C;
			EffectItem.Col1.Set(meshDrawer.ColGrd).setA(0f);
			meshDrawer.fnMeshPointColor = (MeshDrawer Md, float x, float y, C32 DefCol, float len, float agR) => EffectItemNel.fnColPostPopPowerbombShere(Md, x, y, DefCol, len, agR);
			meshDrawer.allocUv2(64, false).BlurCircle2(0f, 0f, 1f, 1f, num2, meshDrawer.ColGrd, EffectItem.Col1);
			meshDrawer.fnMeshPointColor = null;
			return true;
		}

		private static C32 fnColPostPopPowerbombShere(MeshDrawer Md, float x, float y, C32 DefCol, float len, float agR)
		{
			if (len == 0f)
			{
				Md.Uv2(0f, 0f, false);
			}
			else
			{
				float num = X.NIL(EffectItemNel.postsphere_wu * EffectItemNel.postsphere_hu, 0f, len - 0.015625f, EffectItemNel.postsphere_hu - 0.015625f);
				Md.Uv2(num * X.Cos(agR), num * X.Sin(agR), false);
			}
			return DefCol;
		}

		public static bool fnDropRunDraw_EnemyBlood(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			uint ran = X.GETRAN2(Dro.index, Dro.index % 7);
			float num = X.Abs(Dro.z) * Dro.CLENB;
			EffectItem.Col1.Set(4288448391U).mulA(X.NI(0.4f, 0.8f, X.RAN(ran, 1938)));
			if (!Dro.on_ground)
			{
				MeshDrawer meshDrawer = Ef.GetMesh("", MTRX.MIicon.getMtr(BLEND.SUB, -1), true);
				meshDrawer.Col = EffectItem.Col1.C;
				meshDrawer.initForImg(MTRX.EffCircle128, 0);
				meshDrawer.Rect(0f, 0f, num * 2f, num * 2f, false);
			}
			else
			{
				Dro.fixToFootY(Ef);
				MeshDrawer meshDrawer;
				if (Dro.af_ground < 5f)
				{
					X.ZSIN(Dro.af_ground + 3f, 8f);
					meshDrawer = Ef.GetMesh("", MTRX.MIicon.getMtr(BLEND.SUB, -1), true);
					meshDrawer.initForImg(MTRX.EffCircle128, 0);
					meshDrawer.Col = EffectItem.Col1.C;
					meshDrawer.Rect(0f, 0f, num * X.NIXP(3.3f, 4.8f) * 2f, num * X.NIXP(0.8f, 0.002f), false);
				}
				meshDrawer = null;
				if (Dro.z < 0f && EnemySummoner.ActiveScript != null && EnemySummoner.ActiveScript.enemy_count >= 3f)
				{
					return Dro.af_ground < 5f;
				}
				float num2 = Dro.af_ground / Dro.time;
				if (num2 >= 1f)
				{
					return false;
				}
				int num3 = X.IntR(num / 6f * X.NI(1f, 1.6f, X.RAN(ran, 2336)));
				for (int i = 0; i < num3; i++)
				{
					uint ran2 = X.GETRAN2(Dro.index + 22 + i * 13, Dro.index % 2 + 4 + i % 13);
					float num4 = X.NI(-2, 4, X.RAN(ran2, 2095));
					if (Dro.af_ground >= num4)
					{
						if (meshDrawer == null)
						{
							meshDrawer = Ef.GetMesh("", MTRX.getMtr(BLEND.SUB, -1), true);
						}
						float num5 = X.NI(18, 33, X.RAN(ran2, 2461));
						DripDrawer dripDrawer = MTRX.Drip.Set(num5, X.NI(0.7f, 1.2f, X.RAN(ran2, 433)));
						float num6 = num5 * X.NI(0.2f, 0.6f, X.Pow2(X.RAN(ran2, 2604)));
						meshDrawer.Col = EffectItem.Col1.Set(4288448391U).blend(16777215U, X.NI(X.NI(0.2f, 0.5f, X.RAN(ran2, 2213)), 1f, num2)).C;
						dripDrawer.drawTo(meshDrawer, num * X.NI(-1, 1, X.RAN(ran2, 2994)) * 0.8f, 0f, num6 + num5 * X.NI(1.2f, 1.9f, X.RAN(ran2, 2220)) * num2, false);
					}
				}
			}
			return true;
		}

		public static bool fnDropRunDraw_LayedEffectEgg(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			M2DropObject.drawLayedEffectEgg(Dro, Ef, Ed, MTR.ALayedWormEgg);
			return true;
		}

		public static bool fnDropRunDraw_LayedEffectEggSlime(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			M2DropObject.drawLayedEffectEgg(Dro, Ef, Ed, MTR.ALayedSlimeEgg);
			return true;
		}

		public static bool fnDropRunDraw_LayedEffectWorm(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			M2DropObject.drawLayedEffectChild(Dro, Ef, Ed, MTR.ALayedWormChild);
			return true;
		}

		public static bool fnDropRunDraw_LayedEffectSlime(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			M2DropObject.drawLayedEffectChild(Dro, Ef, Ed, MTR.ALayedSlimeChild);
			return true;
		}

		public static bool fnDropRunDraw_LayedEffectMush(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			if (Dro.af_ground <= 0f)
			{
				M2DropObject.drawLayedEffectEgg(Dro, Ef, Ed, MTR.ALayedSlimeEgg);
				return true;
			}
			Dro.fixToFootY(Ef);
			uint ran = X.GETRAN2(Dro.index, Dro.index % 7);
			MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, false);
			PxlFrame pxlFrame = MTR.ALayedMushChild[(long)((ulong)ran % (ulong)((long)MTR.ALayedMushChild.Length))];
			float num = X.ZSIN(Dro.af_ground, 25f) * 1.25f - X.ZCOS(Dro.af_ground - 25f, 30f) * 0.25f;
			meshImg.RotaPF(0f, 0f, 1f, num, 0f, pxlFrame, X.RAN(ran, 2500) < 0.5f, false, false, uint.MaxValue, false, 0);
			return true;
		}

		public static bool fnRunDraw_NpcLoveUp(EffectItem E)
		{
			if (E.z == 0f)
			{
				E.z = 1f;
			}
			return EffectItemNel.fnRunDraw_NpcHkds(E, MTR.SqNpcLoveUp, E.z, (float)E.time);
		}

		public static bool fnRunDraw_NpcHkds(EffectItem E, PxlSequence SqAnim, float scale = 2f, float maxt = 160f)
		{
			if (E.af >= maxt)
			{
				return false;
			}
			MeshDrawer meshImg = E.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, false);
			PxlFrame frame = SqAnim.getFrame((int)X.Mn((float)(SqAnim.countFrames() - 1), E.af * (float)SqAnim.countFrames() / maxt));
			meshImg.RotaPF(0f, -20f * (1f - X.ZSIN(E.af, 22f)), scale, scale, 0f, frame, false, false, false, uint.MaxValue, false, 0);
			return true;
		}

		public static void drawFlyingMoth(MeshDrawer MdTop, MeshDrawer MdBottom, float shiftx, float shifty, float af, int count, float maxt, float xdf, float ydf, float xfly_range, float yfly_range, uint cols, uint cole, int random_seed = 44, float scale = 2f)
		{
			float num = 3.5f + 1.8f * X.absMn(X.COSIT(120f), 0.6f);
			MdTop.initForImg(MTRX.EffCircle128, 0);
			MdBottom.initForImg(MTRX.EffBlurCircle245, 0);
			for (int i = 0; i < count; i++)
			{
				int num2 = (int)(af - (float)(27 * i));
				if (num2 >= 0)
				{
					int num3 = (int)((float)num2 / maxt);
					uint ran = X.GETRAN2(random_seed + i + num3, i % 6);
					float num4 = ((float)num2 - (float)num3 * maxt) / maxt;
					float num5 = X.ZLINE(num4, 0.2f) * 1f - X.ZLINE(num4 - 0.6f, 0.2f);
					if (num5 > 0f)
					{
						MdTop.Col = MdBottom.ColGrd.Set(cole).blend(cols, num5).C;
						MdBottom.ColGrd.Set(cole).blend(cols, 0.25f).mulA(num5);
						MdBottom.Col = MdBottom.ColGrd.C;
						float num6 = X.NI(-1, 1, X.RAN(ran, 2837)) * xdf;
						float num7 = X.NI(-1, 1, X.RAN(ran, 1324)) * ydf;
						float num8 = X.RAN(ran, 939) * 6.2831855f + af * 6.2831855f / (maxt * X.NI(2f, 3f, X.RAN(ran, 1715)));
						float num9 = X.RAN(ran, 2302) * 6.2831855f + af * 6.2831855f / (maxt * X.NI(2f, 3f, X.RAN(ran, 2769)));
						float num10 = X.NI(0.6f, 1f, X.RAN(ran, 2270)) * xfly_range;
						float num11 = X.NI(0.6f, 1f, X.RAN(ran, 1338)) * yfly_range;
						float num12 = num6 + shiftx + X.Cos(num8) * num10;
						float num13 = num7 + shifty + X.Sin(num9) * num11;
						float num14 = num * scale * 2f;
						MdTop.Rect(num12, num13, scale * 2f, scale * 2f, false);
						MdBottom.Rect(num12, num13, num14, num14, false);
					}
				}
			}
		}

		private static bool init_particle = false;

		private static float tz;

		public static FnPtcInit fnPtcInitOnlyColor = delegate(EfParticle EFP, EfParticleVarContainer mde)
		{
			if (mde != null)
			{
				EFP.initColor(mde);
			}
			return true;
		};

		private static float postpopsummon_agR_add = 0f;

		private static float postpopsummon_xscale = 1f;

		private static float postpopsummon_yscale = 1f;

		private static float postsphere_wu;

		private static float postsphere_hu;

		private static Func<MeshDrawer, int, uint, bool> FD_GhostDrawerUv = (MeshDrawer MdR, int i, uint ran) => EffectItemNel.GhostDrawerUv(MdR, i < 0, ran, -0.2f, -0.4f, -0.12f, -1000f, 120, 133);
	}
}
