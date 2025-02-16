using System;
using m2d;
using PixelLiner;
using XX;

namespace nel
{
	public class MgNCeilDrop : MgFDHolder
	{
		public static bool setCeilDrop(Map2d Mp, NelAttackInfo Atk, MGHIT mg_hit, M2BlockColliderContainer.BCCLine TargetBcc, float cx, float cy, float range_x, int count, float min_dropstart_t, float max_dropstart_t, float drop_vy = 0.25f)
		{
			MGContainer mgc = (Mp.M2D as NelM2DBase).MGC;
			float num = ((range_x == 0f || count <= 1) ? 0f : (range_x / (float)count));
			float num2 = -range_x * 0.5f;
			int num3 = Mp.crop - 1;
			int num4 = Mp.crop;
			int num5 = Mp.clms - Mp.crop;
			int num6 = (int)cy;
			float num7 = -1000f;
			float num8 = -1000f;
			if (EnemySummoner.isActiveBorder())
			{
				M2LpSummon summonedArea = EnemySummoner.ActiveScript.getSummonedArea();
				if (summonedArea != null)
				{
					num3 = X.Mx(num3, summonedArea.mapy - 1);
					num4 = X.Mx(num4, summonedArea.mapx);
					num5 = X.Mn(num5, summonedArea.mapx + summonedArea.mapw);
				}
			}
			if (TargetBcc != null)
			{
				M2BlockColliderContainer.BCCLine bccline;
				M2BlockColliderContainer.BCCLine bccline2;
				TargetBcc.getLinearWalkableArea(0f, out num7, out num8, out bccline, out bccline2, 12f);
			}
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				float num9 = -1000f;
				for (int j = 0; j < 20; j++)
				{
					float num10 = ((j == 0) ? (cx - num2 + ((float)i + 0.5f + X.XORSPSH()) * num) : X.NIXP(X.Mx((float)num4 + 0.3f, cx - num2), X.Mn((float)num5 - 0.3f, cx + num2)));
					if (X.BTW((float)num4, num10, (float)num5))
					{
						int num11 = (int)num10;
						if (X.BTW(num7, (float)num11, num8) && X.XORSP() < 0.65f)
						{
							M2BlockColliderContainer.BCCLine sideBcc = Mp.getSideBcc(num11, (int)(TargetBcc.y - 1f), AIM.T);
							if (sideBcc != null)
							{
								num9 = sideBcc.slopeBottomY(num10);
							}
						}
						if (num9 <= 0f)
						{
							int k = num6;
							while (k >= num3)
							{
								if (Mp.canStand(num11, k))
								{
									M2BlockColliderContainer.BCCLine sideBcc2 = Mp.getSideBcc(num11, k - 1, AIM.T);
									if (sideBcc2 != null)
									{
										num9 = sideBcc2.slopeBottomY(num10);
										break;
									}
									break;
								}
								else
								{
									k--;
								}
							}
						}
						if (num9 > 0f)
						{
							MagicItem magicItem = mgc.setMagic(Atk.Caster, MGKIND.CEILDROP, mg_hit | MGHIT.IMMEDIATE);
							magicItem.sx = num10;
							magicItem.sy = num9;
							magicItem.sa = X.NIXP(min_dropstart_t, max_dropstart_t);
							magicItem.dy = drop_vy;
							magicItem.Atk0 = Atk;
							flag = true;
							break;
						}
					}
				}
			}
			if (flag)
			{
				M2Mover baseMover = Mp.M2D.Cam.getBaseMover();
				if (baseMover == null)
				{
					Mp.M2D.Snd.playAt("ground_gogogo", "ground", cx, cy, SndPlayer.SNDTYPE.SND, 1);
				}
				else
				{
					float num12 = X.MMX(cx - num2, baseMover.x, cx + num2);
					Mp.M2D.Snd.playAt("ground_gogogo", "ground", num12, cy, SndPlayer.SNDTYPE.SND, 1);
				}
			}
			return flag;
		}

		public override bool run(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 0)
			{
				Mg.phase = 1;
				Mg.sz = (float)((1 + X.xors(MTR.SqEffectCeilFall.countFrames())) * X.MPFXP());
				Mg.changeRay(Mg.MGC.makeRay(Mg, 0.3f, true, true));
				Mg.Ray.HitLock(-1f, null);
				Mg.Ray.check_hit_wall = true;
				Mg.Ray.check_other_hit = false;
				Mg.raypos_s = (Mg.efpos_s = (Mg.aimagr_calc_s = true));
				Mg.wind_apply_s_level = 1.75f;
				Mg.Ray.hittype |= HITTYPE.WATER_CUT;
				Mg.Ray.LenM(0.01f);
				Mg.Ray.AngleR(-1.5707964f);
				Mg.projectile_power = 100;
				Mg.Ray.hittype_to_week_projectile = HITTYPE.BREAK;
				Mg.PtcVar("sx", (double)Mg.sx).PtcVar("sy", (double)Mg.sy).PtcST("mg_ceildrop_prepare", PTCThread.StFollow.NO_FOLLOW, false);
				return true;
			}
			if (Mg.Atk0 == null)
			{
				return Mg.t <= 20f;
			}
			if (Mg.phase == 1 && Mg.t >= Mg.sa)
			{
				Mg.phase = 10;
				Mg.sa = 0f;
			}
			if (Mg.phase == 10)
			{
				Mg.sa += fcnt;
				float num = Mg.dy * X.NI(0.2f, 1f, X.ZPOW(Mg.sa, 50f));
				Mg.Ray.LenM(num);
				Mg.setRayStartPos(Mg.Ray);
				if (Mg.sa > 15f)
				{
					M2Attackable m2Attackable = Mg.Caster as M2Attackable;
					if (m2Attackable == null || m2Attackable.destructed || !m2Attackable.is_alive)
					{
						Mg.Ray.hit_en = false;
						Mg.Ray.hit_pr = false;
						Mg.Ray.check_other_hit = false;
					}
					HITTYPE hittype = Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.NONE);
					if ((hittype & HITTYPE.KILLED) != HITTYPE.NONE)
					{
						return Mg.kill(Mg.Ray.lenmp);
					}
					if ((hittype & (HITTYPE.WALL | HITTYPE.REFLECTED | HITTYPE.BREAK | HITTYPE.REFLECT_BROKEN | HITTYPE.REFLECT_KILLED)) != HITTYPE.NONE)
					{
						Mg.PtcVar("sx", (double)Mg.sx).PtcVar("sy", (double)Mg.sy).PtcST("golem_shot_broken", PTCThread.StFollow.NO_FOLLOW, false);
						return false;
					}
				}
				Mg.sy += num;
			}
			return true;
		}

		public override bool draw(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 0)
			{
				return true;
			}
			MeshDrawer meshImg = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, false);
			PxlFrame frame = MTR.SqEffectCeilFall.getFrame((int)X.Abs(Mg.sz) - 1);
			float num = Mg.t + (float)(Mg.id % 47 * 18);
			X.COSI((float)((int)(num / 5f) * 5), 31.7f + (float)(Mg.id % 13) * 0.66f);
			float num2 = 2f;
			float num3 = 1f;
			if (Mg.phase < 10)
			{
				float num4 = X.ZPOW(Mg.t, 40f);
				num3 *= num4;
			}
			meshImg.Col = meshImg.ColGrd.Set(uint.MaxValue).blend(4287137928U, X.ZLINE(0.7f + 0.4f * X.COSI(num, 21.3f))).C;
			meshImg.RotaPF(0f, 0f, num2, num2 * num3, 0f, frame, false, false, false, uint.MaxValue, false, 0);
			MeshDrawer meshImg2 = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, true);
			float num5 = ((Mg.phase < 10) ? X.ZLINE(Mg.t, 60f) : 1f);
			num3 = 100f * (1f + 0.15f * X.COSI(num, 17.2f) + 0.05f * X.COSI(num, 9.1f));
			meshImg2.Col = meshImg2.ColGrd.Set(4294901820U).blend(4288020515U, 0.5f + X.COSI(num, 11.7f) * 0.5f).mulA(num5)
				.C;
			meshImg2.initForImg(MTRX.EffBlurCircle245, 0).Rect(0f, 0f, num3, num3, false);
			if (Mg.phase >= 10)
			{
				PxlLayer layer = frame.getLayer(0);
				float num6 = (float)layer.Img.width;
				float x = layer.x;
				float num7 = (X.ZCOS(Mg.sa, 30f) * 0.3f + X.ZPOW(Mg.sa - 20f, 80f) * 0.7f) * 120f;
				MeshDrawer mesh = Mg.Ef.GetMesh("", 1442840575U, BLEND.NORMAL, true);
				mesh.ColGrd.Set(16777215);
				mesh.Scale(num2, num2, false);
				mesh.RectBLGradation(x - num6 * 0.5f, -layer.y, num6, num7, GRD.BOTTOM2TOP, false);
			}
			return true;
		}
	}
}
