using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class MgNGroundShockwave : MgFDHolderWithMemoryClass<MgNGroundShockwave.GswMem>
	{
		public MgNGroundShockwave()
			: base(() => new MgNGroundShockwave.GswMem())
		{
		}

		public override MagicItem initFunc(MagicItem Mg)
		{
			base.initFunc(Mg);
			(Mg.Other as MgNGroundShockwave.GswMem).Init(Mg, this);
			return Mg;
		}

		public static MagicItem Init(MagicItem Mg, float bx, float xshift_abs, float by, AIM a, float map_high, float spd, NelAttackInfo Atk, M2BlockColliderContainer.BCCLine CurFoot = null)
		{
			(Mg.Other as MgNGroundShockwave.GswMem).Init2(a, CurFoot);
			int num = CAim._XD(a, 1);
			Mg.sx = bx + (float)num * xshift_abs;
			Mg.sy = by;
			Mg.sz = map_high;
			Mg.sa = spd;
			Mg.Atk0 = Atk;
			Atk.burst_vx = (float)num * 0.26f;
			Atk.burst_vy = -0.18f;
			Mg.run(0f);
			return Mg;
		}

		public override bool run(MagicItem Mg, float fcnt)
		{
			MgNGroundShockwave.GswMem gswMem = Mg.Other as MgNGroundShockwave.GswMem;
			if (gswMem == null)
			{
				return Mg.t < 2f;
			}
			Map2d mp = Mg.Mp;
			if (fcnt > 0f && Mg.phase == 0)
			{
				Mg.efpos_s = true;
				Mg.raypos_c = (Mg.raypos_s = false);
				if (Mg.Ray == null)
				{
					Mg.changeRay(Mg.MGC.makeRay(Mg, 0f, false, false));
				}
				Mg.Ray.HitLock(-1f, null);
				Mg.projectile_power = 200;
				Mg.phase = 1;
				Mg.Ray.AngleR(CAim.get_agR(gswMem.a, 0f)).HitLock(400f, null);
				Mg.Ray.shape = RAYSHAPE.RECT;
				Mg.PtcVar("cx", (double)Mg.sx).PtcVar("cy", (double)Mg.sy).PtcST("ground_shockwave_init", PTCThread.StFollow.NO_FOLLOW, false);
				Mg.dx = -1f;
				gswMem.fineAngle();
				Mg.da = gswMem.dep_agR;
			}
			if (Mg.phase >= 1)
			{
				float num = (0.013f + (Mg.sa - 0.013f) * X.ZSIN(Mg.t - 11f, 40f)) * fcnt;
				Mg.dz = X.ZSIN(Mg.t, 20f) * Mg.sz;
				float sx = Mg.sx;
				int num2 = CAim._XD(gswMem.a, 1);
				Mg.sx += num * (float)num2;
				if (Mg.phase == 1 && gswMem.CurBCC == null)
				{
					mp.BCC.isFallable(Mg.sx, Mg.sy - Mg.sz * 0.375f, Mg.dz * ((Mg.t > 20f) ? 0.5f : 1f), Mg.sz * 0.5f, out gswMem.CurBCC, true, true, -1f);
					if (gswMem.CurBCC == null)
					{
						for (int i = mp.count_carryable_bcc - 1; i >= 0; i--)
						{
							mp.getCarryableBCCByIndex(i).isFallable(Mg.sx, Mg.sy - Mg.sz * 0.375f, Mg.dz, Mg.sz * 0.5f, out gswMem.CurBCC, true, true, -1f);
							if (gswMem.CurBCC != null)
							{
								break;
							}
						}
					}
					if (gswMem.CurBCC == null)
					{
						gswMem.EndSlide(Mg.sx + (float)num2 * Mg.sz);
					}
					else
					{
						gswMem.fineAngle();
					}
				}
				if (Mg.phase == 1)
				{
					if (num2 > 0 && Mg.sx >= gswMem.CurBCC.right)
					{
						if (gswMem.CurBCC.R_is_90)
						{
							gswMem.EndSlide(gswMem.CurBCC.right);
						}
						else
						{
							gswMem.CurBCC = gswMem.CurBCC.SideR;
							if (gswMem.CurBCC._yd == 0)
							{
								gswMem.CurBCC = null;
							}
							gswMem.fineAngle();
						}
					}
					if (num2 < 0 && Mg.sx <= gswMem.CurBCC.left)
					{
						if (gswMem.CurBCC.L_is_90)
						{
							gswMem.EndSlide(gswMem.CurBCC.left);
						}
						else
						{
							gswMem.CurBCC = gswMem.CurBCC.SideL;
							if (gswMem.CurBCC._yd == 0)
							{
								gswMem.CurBCC = null;
							}
							gswMem.fineAngle();
						}
					}
				}
				else if ((num2 > 0) ? (Mg.sx - Mg.dz * 2f > Mg.dx) : (Mg.sx + Mg.dz * 2f < Mg.dx))
				{
					Mg.sx -= (Mg.dz * 2f + 0.8f) * (float)num2 * fcnt;
					Mg.explodeS();
					return false;
				}
				Mg.sy += num * Mg.dy * (float)num2;
				Mg.Ray.RadiusM(Mg.dz * 0.5f).PosMap((Mg.phase == 2) ? (Mg.dx - (float)num2 * Mg.dz * 0.5f) : (Mg.sx - (float)num2 * Mg.dz * 0.5f), Mg.sy - Mg.dz * 0.5f);
				if ((Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.BurstDir((float)num2), HITTYPE.NONE) & (HITTYPE)4259840) != HITTYPE.NONE)
				{
					Mg.kill(0.125f);
					Mg.explodeS();
					return false;
				}
				Mg.da = X.VALWALKANGLER(Mg.da, gswMem.dep_agR, 0.04712389f);
			}
			return true;
		}

		public override bool draw(MagicItem Mg, float fcnt)
		{
			MgNGroundShockwave.GswMem gswMem = Mg.Other as MgNGroundShockwave.GswMem;
			if (gswMem == null)
			{
				return true;
			}
			PxlImage imgGroundShockWave = MTR.ImgGroundShockWave;
			float num = Mg.CLENB * Mg.dz * 2f;
			float num2 = Mg.CLENB * Mg.sz;
			float num3 = 1f;
			int num4 = CAim._XD(gswMem.a, 1);
			if (Mg.phase == 2)
			{
				num3 = X.ZSIN((num4 > 0) ? (Mg.dx - (Mg.sx - Mg.dz * 2f)) : (Mg.sx + Mg.dz * 2f - Mg.dx), Mg.dz * 2f);
				if (num3 < 0f)
				{
					return true;
				}
			}
			float num5 = num / 205f;
			float num6 = imgGroundShockWave.RectIUv.xMin + imgGroundShockWave.RectIUv.width * num3;
			float num7 = 62f - (float)imgGroundShockWave.width * (1f - num3);
			MeshDrawer meshDrawer = null;
			for (int i = 0; i < 2; i++)
			{
				float num8 = num2 / 99f * (0.88f + 0.06f * X.COSI(Mg.t + (float)(i * 75), 3.2f + 0.9f * (float)i) + 0.06f * X.COSI(Mg.t + 30f + (float)(i * 15), 2.4f - 0.9f * (float)i));
				if (i == 0)
				{
					meshDrawer = Mg.Ef.GetMeshImg("", MTR.MIiconL, BLEND.ADD, false);
					meshDrawer.Col = C32.d2c(4294452465U);
					meshDrawer.ColGrd.Set(4287561866U);
					num8 *= 1.05f;
					meshDrawer.TranslateP(0f, (float)(-(float)imgGroundShockWave.height) * 0.5f, false);
					meshDrawer.Rotate(Mg.da, false);
					meshDrawer.TranslateP(0f, (float)imgGroundShockWave.height * 0.5f - 21f, false);
					meshDrawer.Scale(num5 * (float)num4, num8, false);
				}
				else
				{
					Matrix4x4 currentMatrix = meshDrawer.getCurrentMatrix();
					meshDrawer = Mg.Ef.GetMeshImg("magic_slicer_sub", MTR.MIiconL, BLEND.SUB, false);
					meshDrawer.setCurrentMatrix(currentMatrix, false);
					if (meshDrawer.getTriMax() == 0)
					{
						meshDrawer.base_z -= 0.05f;
					}
					meshDrawer.Col = C32.d2c(4289853674U);
					meshDrawer.ColGrd.Set(4279530364U);
				}
				meshDrawer.Tri(0, 2, 1, false).Tri(0, 3, 2, false);
				meshDrawer.uvRectN(num6, imgGroundShockWave.RectIUv.yMin).PosD(num7, 0f, null);
				meshDrawer.uvRectN(num6, imgGroundShockWave.RectIUv.yMax).PosD(num7, (float)imgGroundShockWave.height * num3, meshDrawer.ColGrd);
				meshDrawer.uvRectN(imgGroundShockWave.RectIUv.x, imgGroundShockWave.RectIUv.yMax).PosD(62f - (float)imgGroundShockWave.width, (float)imgGroundShockWave.height, meshDrawer.ColGrd);
				meshDrawer.uvRectN(imgGroundShockWave.RectIUv.x, imgGroundShockWave.RectIUv.yMin).PosD(62f - (float)imgGroundShockWave.width, 0f, null);
			}
			return true;
		}

		public class GswMem : IDisposable
		{
			public void Init(MagicItem _Mg, MgNGroundShockwave _Con)
			{
				this.Con = _Con;
				this.Mg = _Mg;
			}

			public void Init2(AIM _a, M2BlockColliderContainer.BCCLine _CurBCC)
			{
				this.a = _a;
				this.CurBCC = _CurBCC;
			}

			public void fineAngle()
			{
				if (this.CurBCC == null || this.CurBCC._yd == 0)
				{
					this.dep_agR = 0f;
					this.Mg.dy = 0f;
					return;
				}
				this.Mg.sy = this.CurBCC.slopeBottomY(this.Mg.sx);
				this.Mg.dy = this.CurBCC.line_a;
				this.dep_agR = this.CurBCC.sd_agR * (float)CAim._XD(this.a, 1);
			}

			public void EndSlide(float dx)
			{
				this.CurBCC = null;
				this.Mg.phase = 2;
				this.Mg.dx = dx;
				this.Mg.dy = 0f;
			}

			public void Dispose()
			{
				MgNGroundShockwave con = this.Con;
				this.Mg = null;
				this.Con = null;
				this.CurBCC = null;
				if (con != null)
				{
					con.ReleaseMem(this);
				}
			}

			public AIM a;

			public MgNGroundShockwave Con;

			public MagicItem Mg;

			public M2BlockColliderContainer.BCCLine CurBCC;

			public float dep_agR;
		}
	}
}
