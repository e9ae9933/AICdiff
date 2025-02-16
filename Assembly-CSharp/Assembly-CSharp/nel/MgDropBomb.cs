using System;
using System.Collections.Generic;
using evt;
using m2d;
using PixelLiner;
using XX;

namespace nel
{
	public class MgDropBomb : MgFDHolderWithMemoryClass<MgDropBomb.MgDbMem>
	{
		public MgDropBomb()
			: base(() => new MgDropBomb.MgDbMem())
		{
			this.FD_RunMain = new MagicItem.FnMagicRun(this.runMain);
		}

		public override MagicNotifiear GetNotifiear()
		{
			return new MagicNotifiear(3).AddHit(new MagicNotifiear.MnHit
			{
				x = 0f,
				y = 1.1f,
				maxt = 20f,
				v0 = 0.18f,
				thick = 2.6f,
				accel_maxt = 40f,
				accel = -0.0045000003f,
				wall_hit = true,
				penetrate = true,
				other_hit = false,
				no_draw = false,
				auto_target = false,
				fnManipulateMagic = new MagicNotifiear.FnManipulateMagic(this.fnManipulateDropBomb),
				fnManipulateTargetting = new MagicNotifiear.FnManipulateTargetting(this.fnManipulateTargettingDropBomb)
			}).AddHit(new MagicNotifiear.MnHit
			{
				type = MagicNotifiear.HIT.CIRCLE,
				thick = 1.25f,
				maxt = 60f,
				penetrate = true,
				other_hit = false,
				no_draw = true,
				draw_thick_kadomaru = true,
				v0 = 4f
			}).AddHit(new MagicNotifiear.MnHit
			{
				type = MagicNotifiear.HIT.CIRCLE,
				thick = 0.2f,
				penetrate = true,
				other_hit = true,
				no_draw = true,
				wall_hit = false,
				auto_target = false
			});
		}

		public override bool run(MagicItem Mg, float fcnt)
		{
			if (Mg.phase == 0)
			{
				Mg.phase = -1;
				Mg.raypos_s = (Mg.efpos_s = true);
				Mg.sx += Mg.Mn._0.x;
				Mg.sy += Mg.Mn._0.y;
				Mg.calcAimPos(true);
				int num = (int)Mg.sz;
				float num2 = X.correctangleR(Mg.aim_agR);
				float num3 = 0f;
				bool flag = false;
				int num4 = num;
				int num5 = X.IntR(num2 / 0.7853982f);
				float num6 = 0.95f;
				float num7 = 0.16f;
				float num8 = 1f;
				float num9 = 1f;
				float num10;
				switch (num5)
				{
				case -4:
				case 0:
				case 4:
					num10 = (float)num * 3.1415927f * 0.15f * (float)X.MPF(num5 == 0);
					num2 = ((num5 == 0) ? (num10 * 0.5f) : (3.1415927f + num10 * 0.5f));
					goto IL_021A;
				case -3:
				case -1:
					num10 = (float)num * 3.1415927f * 0.18f * (float)X.MPF(num5 == -3);
					num2 = (float)num5 * 0.7853982f + num10 * 0.3f;
					num8 = 1.7f;
					goto IL_021A;
				case -2:
					num2 = -1.4922565f;
					num3 = -3.1415927f - num2;
					num4 = X.IntC((float)(num / 2));
					num10 = (float)num4 * 3.1415927f * 0.05f;
					flag = true;
					num6 = 0.1f;
					num9 = (num8 = 0.4f);
					goto IL_021A;
				case 1:
				case 3:
					num10 = (float)num * 3.1415927f * 0.18f * (float)X.MPF(num5 == 1);
					num2 = (float)num5 * 0.7853982f + num10 * 0.3f;
					num9 = 1.7f;
					num7 = 0.06f;
					goto IL_021A;
				}
				num4 = X.IntC((float)(num / 2));
				num10 = (float)(-(float)num4) * 3.1415927f * 0.19f;
				num2 = -0.50265485f - num10 * 0.5f;
				num3 = 3.1415927f - num2;
				num8 = 1.2f;
				num7 = 0.06f;
				flag = true;
				IL_021A:
				bool flag2 = true;
				float num11 = 0.4f;
				if ((int)Mg.Cen.x != (int)Mg.sx)
				{
					int config = Mg.Mp.getConfig((int)Mg.Cen.x + X.MPF(Mg.sx > Mg.Cen.x), (int)Mg.sy);
					if (!CCON.canStand(config) || CCON.isSlope(config))
					{
						num11 = 0.04f;
					}
				}
				float num12 = X.NI(Mg.Cen.x, Mg.sx, num11);
				float num13 = X.NI(Mg.Cen.y, Mg.sy, 0.1f);
				int num14 = Mg.reduce_mp / num;
				int num15 = 0;
				float num16 = (flag ? (1f / (0.001f + (float)num4 - 1f)) : (1f / (0.001f + (float)num - 1f)));
				for (int i = 0; i < num; i++)
				{
					MagicItem magicItem = ((i == 0) ? Mg : Mg.createNewMagic(Mg.kind, num12, num13, false));
					Mg.sx = num12;
					Mg.sy = num13;
					if (i > 0)
					{
						magicItem.reduce_mp = num14;
						num15 += num14;
						magicItem.Atk0.CopyFrom(Mg.Atk0);
					}
					magicItem.initFunc(this.FD_RunMain, null);
					(magicItem.Other as MgDropBomb.MgDbMem).Init(magicItem, this);
					if (flag)
					{
						if (flag2)
						{
							magicItem.sa = num2 + num10 * ((float)(i / 2) - (float)(num4 - 1) * 0.5f) * num16;
							flag2 = false;
						}
						else
						{
							magicItem.sa = num3 - num10 * ((float)(i / 2) - (float)(num4 - 1) * 0.5f) * num16;
							flag2 = true;
						}
					}
					else
					{
						magicItem.sa = num2 + num10 * ((float)i - (float)(num - 1) * 0.5f) * num16;
					}
					magicItem.dx = num8;
					magicItem.dy = num9;
					magicItem.sz = num7;
					magicItem.da = num6;
				}
				Mg.reduce_mp -= num15;
				Mg.run(0f);
				return true;
			}
			return fcnt == 0f;
		}

		private bool runMain(MagicItem Mg, float fcnt)
		{
			M2DropObject m2DropObject;
			if (Mg.phase <= 1)
			{
				if (Mg.Ray == null)
				{
					Mg.changeRay(Mg.MGC.makeRay(Mg, Mg.Mn._0.thick, false, false));
				}
				Mg.MGC.initRayCohitable(Mg.Ray);
				Mg.Ray.cohitable_allow_berserk = M2Ray.COHIT.BERSERK_N;
				Mg.Ray.HitLock(-1f, null);
				Mg.Ray.hittype_to_week_projectile = HITTYPE.NONE;
				Mg.raypos_s = (Mg.efpos_s = (Mg.target_s = true));
				Mg.wind_apply_s_level = 0.66f;
				Mg.Atk0.PublishMagic = Mg;
				Mg.Mn._0.x = (Mg.Mn._0.y = 0f);
				float num = Mg.Mn._0.Spd(0f);
				m2DropObject = Mg.createDropper(num * Mg.dx * X.Cos(Mg.sa), num * Mg.dy * X.Sin(Mg.sa), 0f, -1f, -1f);
				m2DropObject.gravity_scale = Mg.sz;
				m2DropObject.TypeAdd(DROP_TYPE.CHECK_LIFT);
				Mg.PtcVar("tx", (double)Mg.sx).PtcVar("ty", (double)Mg.sy).PtcVar("agR", (double)Mg.sa)
					.PtcST("dropbomb_shot", PTCThread.StFollow.NO_FOLLOW, false);
				Mg.PtcST("dropbomb_ball", PTCThread.StFollow.FOLLOW_S, false);
				Mg.phase = 2;
				Mg.sz = -18f;
				Mg.dz = -24f;
				Mg.MnSetRay(Mg.Ray, 2, Mg.sa, 0f).CastRayAndCollider(null);
			}
			else
			{
				m2DropObject = Mg.Dro;
			}
			if (Mg.phase <= 10 && (Mg.Ray.hittype & HITTYPE.REFLECT_BROKEN) != HITTYPE.NONE)
			{
				Mg.phase = 20;
			}
			if (fcnt == 0f)
			{
				return true;
			}
			if (m2DropObject != null)
			{
				bool flag = true;
				if (Mg.phase < 20)
				{
					int num2 = (int)Mg.sx;
					int num3 = (int)Mg.sy;
					if (CCON.isWater(Mg.Mp.getConfig(num2, num3)))
					{
						Mg.wind_apply_s_level = 0.19800001f;
						if (Mg.M2D.MIST != null && !Mg.Dro.on_ground && Mg.M2D.MIST.isFallingWater(num2, num3))
						{
							if (Mg.Dro.vy < 0.24f)
							{
								Mg.Dro.vy += 0.023f;
							}
							flag = false;
						}
						else if (Mg.Dro.vy > -0.08f)
						{
							Mg.Dro.vy -= 0.014f;
						}
					}
					else
					{
						Mg.wind_apply_s_level = 0.66f;
					}
				}
				if (Mg.phase == 2 || Mg.phase == 3)
				{
					float accel_maxt = Mg.Mn._0.accel_maxt;
					Mg.calced_aim_pos = true;
					if (Mg.Ray.reflected)
					{
						if (Mg.t >= Mg.Mn._0.accel_maxt)
						{
							Mg.reflectV(Mg.Ray, ref m2DropObject.vx, ref m2DropObject.vy, 0.18f, 1f, true);
							m2DropObject.vx = X.absMn(m2DropObject.vx, 0.15f);
							m2DropObject.vy = X.absMn(m2DropObject.vy + (float)X.MPF(m2DropObject.vy > 0f) * 0.08f, 0.14f);
							m2DropObject.gravity_scale = 0.16f;
							Mg.t = 0f;
						}
						Mg.Ray.hittype &= (HITTYPE)(-33);
					}
					if (Mg.t <= Mg.Mn._0.accel_maxt)
					{
						m2DropObject.TypeAdd(DROP_TYPE.WATER_BOUNCE);
						float num4 = ((Mg.phase == 2) ? 0.01f : 0.0028f) * fcnt;
						m2DropObject.vx = X.VALWALK(m2DropObject.vx, 0f, num4);
						m2DropObject.vy = X.VALWALK(m2DropObject.vy, 0f, num4);
						Mg.aim_agR = Mg.Mp.GAR(0f, 0f, m2DropObject.vx, m2DropObject.vy);
						m2DropObject.bounce_y_reduce = ((Mg.phase == 2) ? Mg.da : 1.12f);
						m2DropObject.size = 0.12f;
					}
					else
					{
						m2DropObject.TypeRem(DROP_TYPE.WATER_BOUNCE);
						if (Mg.phase == 2)
						{
							Mg.phase = 3;
							m2DropObject.vx = (m2DropObject.vy = 0f);
						}
						m2DropObject.bounce_y_reduce = 0.002f;
						m2DropObject.gravity_scale = 0.01f;
						m2DropObject.vx = X.VALWALK(m2DropObject.vx, 0f, 0.01f * fcnt);
						m2DropObject.size = 0.5f;
						if (flag)
						{
							if (m2DropObject.vy > 0.033f)
							{
								m2DropObject.vy = X.VALWALK(m2DropObject.vy, 0.033f, 0.009f * fcnt);
							}
							else if (m2DropObject.vy < -0.033f)
							{
								m2DropObject.vy = X.VALWALK(m2DropObject.vy, -0.033f, 0.009f * fcnt);
							}
							m2DropObject.vy = X.absMn(m2DropObject.vy, 0.04f);
						}
						Mg.aim_agR = 1.5707964f;
						Mg.sa = Mg.aim_agR + 3.1415927f;
					}
					Mg.dz += fcnt;
					Mg.sz += fcnt;
					if (Mg.sz >= 0f)
					{
						Mg.sz = -8f;
						Mg.MnSetRay(Mg.Ray, 0, Mg.sa, Mg.t);
						bool flag2 = Mg.dz >= 0f;
						if (flag2)
						{
							Mg.dz = (float)(EV.isActive(true) ? (-20) : (-40));
						}
						Mg.Ray.check_hit_wall = true;
						Mg.Ray.hittype |= (HITTYPE)16777408;
						Mg.Ray.check_other_hit = flag2;
						Mg.Ray.HitLock(0f, null);
						HITTYPE hittype = Mg.Ray.Cast(false, null, true);
						if ((hittype & HITTYPE.OTHER) != HITTYPE.NONE || ((Mg.hittype & MGHIT.PR) != (MGHIT)0 && (hittype & HITTYPE.EN) != HITTYPE.NONE) || ((Mg.hittype & MGHIT.EN) != (MGHIT)0 && (hittype & HITTYPE.PR) != HITTYPE.NONE))
						{
							IM2RayHitAble im2RayHitAble = null;
							Mg.Atk0.Caster = Mg.Caster;
							Mg.Atk0.AttackFrom = Mg.Caster as M2Attackable;
							int hittedMax = Mg.Ray.getHittedMax();
							for (int i = 0; i < hittedMax; i++)
							{
								M2Ray.M2RayHittedItem hitted = Mg.Ray.GetHitted(i);
								if (hitted.Hit != null)
								{
									if (flag2 && hitted.Mv is M2BreakableWallMover)
									{
										M2BreakableWallMover m2BreakableWallMover = hitted.Mv as M2BreakableWallMover;
										if (m2BreakableWallMover.breakableByBomb(Mg))
										{
											im2RayHitAble = m2BreakableWallMover;
											break;
										}
									}
									else
									{
										if (!(hitted.Mv is M2Attackable))
										{
											im2RayHitAble = hitted.Hit;
											break;
										}
										M2Attackable m2Attackable = hitted.Mv as M2Attackable;
										if ((m2Attackable.can_hit(Mg.Ray) & RAYHIT.HITTED) != RAYHIT.NONE)
										{
											im2RayHitAble = m2Attackable;
											break;
										}
									}
								}
							}
							Mg.Ray.clearHittedTarget();
							if (im2RayHitAble != null)
							{
								Mg.Ray.assignHittedTarget(im2RayHitAble, 0f);
								Mg.phase = 9;
							}
						}
						else
						{
							Mg.Ray.clearHittedTarget();
						}
						if (Mg.Ray.cohitable_assigned)
						{
							Mg.MnSetRay(Mg.Ray, 2, Mg.sa, 0f).CastRayAndCollider(null);
						}
					}
					else
					{
						Mg.Ray.HitLock(0f, null);
						Mg.Ray.check_other_hit = true;
						Mg.Ray.check_hit_wall = false;
						Mg.Ray.hittype &= (HITTYPE)(-16777409);
						Mg.MnSetRay(Mg.Ray, 2, Mg.sa, 0f);
						HITTYPE hittype2 = Mg.Ray.Cast(false, null, true);
						if ((hittype2 & HITTYPE.KILLED) != HITTYPE.NONE)
						{
							Mg.kill(0f);
							return false;
						}
						if ((hittype2 & (HITTYPE)12582912) != HITTYPE.NONE)
						{
							Mg.phase = 20;
						}
					}
				}
				if (Mg.phase == 9)
				{
					m2DropObject.vx *= 0.6f;
					m2DropObject.vy *= 0.6f;
					Mg.t = 0f;
					Mg.phase = 10;
					Mg.calcTargetPos();
					Mg.PtcVar("tx", (double)Mg.PosT.x).PtcVar("ty", (double)Mg.PosT.y).PtcVar("time", (double)Mg.Mn._1.maxt)
						.PtcST("dropbomb_prepare_explode", PTCThread.StFollow.FOLLOW_S, false);
				}
			}
			else if (Mg.phase != 20 && Mg.phase != 15)
			{
				return false;
			}
			if (Mg.phase == 10 || Mg.phase == 15)
			{
				if (Mg.phase == 10)
				{
					m2DropObject.gravity_scale = 0.004f;
					m2DropObject.vx = X.VALWALK(m2DropObject.vx, 0f, 0.0023f * fcnt);
					m2DropObject.vy = X.VALWALK(m2DropObject.vy, 0f, 0.0023f * fcnt);
					bool flag3 = false;
					foreach (KeyValuePair<IM2RayHitAble, float> keyValuePair in Mg.Ray.OHittedFloorT)
					{
						if (keyValuePair.Key is M2Mover)
						{
							M2Mover m2Mover = keyValuePair.Key as M2Mover;
							if (!m2Mover.isDestructed())
							{
								float num5 = X.correctangleR(Mg.Mp.GAR(Mg.sx, Mg.sy, m2Mover.x, m2Mover.y));
								Mg.sa = X.MULWALKANGLER(Mg.sa, num5, ((Mg.t <= 2f) ? 0.8f : 0.038f) * fcnt);
								flag3 = true;
								break;
							}
						}
					}
					if (!flag3)
					{
						Mg.sa = 1.5707964f;
					}
					Mg.MnSetRay(Mg.Ray, 1, Mg.sa, 0f);
					if (Mg.t >= Mg.Mn._1.maxt * 0.5f)
					{
						Mg.phase = 20;
					}
				}
				else if (Mg.t >= Mg.Mn._1.maxt)
				{
					Mg.phase = 20;
				}
			}
			if (Mg.phase == 20)
			{
				Mg.killEffect();
				Mg.Ray.hittype &= (HITTYPE)(-29360353);
				Mg.sa = X.correctangleR(Mg.sa);
				Mg.MnSetRay(Mg.Ray, 1, Mg.sa, 0f);
				if (!Mg.exploded)
				{
					Mg.explode(false);
				}
				Mg.phase = 100;
				Mg.projectile_power = 100;
				Mg.Ray.clearHittedTarget();
				Mg.Ray.HitLock(-1f, null);
				Mg.Ray.check_other_hit = true;
				Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.NONE);
				Mg.PtcVar("height", (double)(Mg.Mn._1.v0 * Mg.CLENB)).PtcVar("agR", (double)Mg.sa).PtcST("dropbomb_explode", PTCThread.StFollow.NO_FOLLOW, false);
				return true;
			}
			return Mg.phase != 100;
		}

		public override bool draw(MagicItem Mg, float fcnt)
		{
			if (Mg.phase >= 2 && Mg.phase < 20)
			{
				PxlImage img = MTRX.AEff[0].getLayer(0).Img;
				float num = ((Mg.phase == 10 || Mg.phase == 15) ? X.ZLINE(Mg.t, 30f) : (0.2f * X.COSI(Mg.t, 33f) + 0.2f));
				MeshDrawer meshImg = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.SUB, false);
				meshImg.initForImg(img, 0);
				float num2 = (30f + 60f * X.ZPOW(num - 0.25f, 0.75f)) * (0.75f + 0.125f * num * X.COSI(Mg.t, 7.65f) + 0.125f * num * X.COSI(Mg.t, 5.675f));
				meshImg.Col = meshImg.ColGrd.Set(4290482815U).C;
				meshImg.ColGrd.setA(0f);
				meshImg.RotaGraph(0f, 0f, num2 / 15f, 0f, null, false);
				MeshDrawer meshImg2 = Mg.Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, false);
				meshImg2.initForImg(img, 0);
				meshImg2.Col = meshImg2.ColGrd.Set((Mg.phase >= 10) ? 4281374970U : 4283273891U).C;
				meshImg2.ColGrd.setA(0f);
				num2 = (30f + 80f * X.ZPOW(num - 0.4f, 0.6f)) * (0.68f + 0.16f * num * X.COSI(Mg.t + 99f, 8.13f) + 0.16f * num * X.COSI(Mg.t + 233f, 6.275f));
				meshImg2.RotaGraph(0f, 0f, num2 / 18f, 0f, null, false);
				if (Mg.phase == 10)
				{
					MeshDrawer mesh = Mg.Ef.GetMesh("", 268435455U, BLEND.ADD, false);
					mesh.Col = mesh.ColGrd.Set((X.ANMT(2, 4f) == 0) ? 4281374970U : 4283273891U).C;
					mesh.Rotate(Mg.sa, false);
					float num3 = Mg.CLENB * Mg.Mn._1.v0 * 0.8f;
					float num4 = num3 * 0.33f;
					mesh.RectBL(-num4, -1f, num4 + num3, 2f, false);
					mesh.RectBL(-1f, -num4, 2f, num4 * 2f, false);
				}
			}
			return true;
		}

		public void initEatenProcess(MagicItem Mg)
		{
			Mg.phase = 15;
			Mg.t = 0f;
			Mg.wind_apply_s_level = 0f;
			if (Mg.Dro != null)
			{
				Mg.Dro.destruct(true);
				Mg.Dro = null;
			}
			Mg.calcTargetPos();
			Mg.PtcVar("tx", (double)Mg.PosT.x).PtcVar("ty", (double)Mg.PosT.y).PtcVar("time", (double)((Mg.Mn != null) ? Mg.Mn._1.maxt : 10f))
				.PtcST("dropbomb_prepare_explode_eaten", PTCThread.StFollow.FOLLOW_S, false);
		}

		public MagicNotifiear.TARGETTING fnManipulateTargettingDropBomb(MagicItem Mg, PR Pr, ref int dx, ref int dy, bool is_first)
		{
			if (Mg.isPreparingCircle)
			{
				return MagicNotifiear.TARGETTING._AIM_ALL;
			}
			return (MagicNotifiear.TARGETTING)0;
		}

		public bool fnManipulateDropBomb(MagicItem Mg, M2MagicCaster _Mv, float fcnt)
		{
			if (Mg.phase != 2)
			{
				return false;
			}
			if (_Mv == null || _Mv != Mg.Caster || base.isWrongMagic(Mg, _Mv))
			{
				return false;
			}
			if (Mg.isPreparingCircle)
			{
				return true;
			}
			int num = -1;
			if (_Mv is PR)
			{
				num = (_Mv as PR).Skill.getMagicManipulatorCursorAim(Mg);
			}
			if (num >= 0)
			{
				Mg.dx += (float)CAim._XD(num, 1) * 0.09f;
				Mg.dy -= (float)CAim._YD(num, 1) * 0.07f;
			}
			return true;
		}

		private MagicItem.FnMagicRun FD_RunMain;

		private const float dbomb_speed0 = 0.18f;

		private const float dbomb_floating_maxt = 40f;

		private const float dbomb_assign_t = 20f;

		public class MgDbMem : IDisposable, IMgBombListener
		{
			public MgDropBomb.MgDbMem Init(MagicItem _Mg, MgDropBomb _Con)
			{
				this.Con = _Con;
				this.Mg = _Mg;
				this.Mg.input_null_to_other_when_quit = true;
				return this;
			}

			public void Dispose()
			{
				this.Mg = null;
				MgDropBomb con = this.Con;
				this.Con = null;
				if (con != null)
				{
					con.ReleaseMem(this);
				}
			}

			public bool isEatableBomb(MagicItem Mg, M2MagicCaster CheckedBy, bool execute)
			{
				if (Mg.Other as MgDropBomb.MgDbMem != this || Mg.Caster == CheckedBy)
				{
					return false;
				}
				if (Mg.kind == MGKIND.DROPBOMB && Mg.Caster != CheckedBy && !Mg.killed && !Mg.isPreparingCircle && X.BTW(2f, (float)Mg.phase, 15f))
				{
					if (execute)
					{
						this.Con.initEatenProcess(Mg);
					}
					return true;
				}
				return false;
			}

			public MgDropBomb Con;

			public MagicItem Mg;
		}
	}
}
