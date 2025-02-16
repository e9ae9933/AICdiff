using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public abstract class MgItemBomb : MgFDHolderWithMemoryClass<MgItemBomb.MgBombMem>
	{
		public MgItemBomb()
			: base(() => new MgItemBomb.MgBombMem())
		{
		}

		public override MagicItem initFunc(MagicItem Mg)
		{
			base.initFunc(Mg);
			(Mg.Other as MgItemBomb.MgBombMem).Init(Mg, this);
			return Mg;
		}

		public override MagicNotifiear GetNotifiear()
		{
			return new MagicNotifiear(3).AddHit(new MagicNotifiear.MnHit
			{
				type = MagicNotifiear.HIT.CIRCLE,
				maxt = 200f,
				thick = 0.25f,
				accel = 0.2f,
				parabola = true,
				v0 = 0.17f,
				penetrate = true,
				other_hit = true,
				auto_target = true,
				cast_on_autotarget = false,
				auto_target_fixable = true,
				fnManipulateMagic = new MagicNotifiear.FnManipulateMagic(this.fnManipulate)
			}).AddHit(new MagicNotifiear.MnHit
			{
				type = MagicNotifiear.HIT.CIRCLE,
				other_hit = false
			});
		}

		public void InitItem(MgItemBomb.MgBombMem Mem)
		{
			float num = (float)Mem.grade / 4f;
			this.InitItemInner(Mem, num);
		}

		protected abstract void InitItemInner(MgItemBomb.MgBombMem Mem, float gratio);

		public void initBombPhase0(MagicItem Mg, MgItemBomb.MgBombMem Mem)
		{
			Mg.Ray.RadiusM(0.12f).HitLock(40f, null);
			Mg.Ray.projectile_power = -10;
			Mg.Ray.hittype_to_week_projectile = HITTYPE.REFLECTED;
			Mg.wind_apply_s_level = 0f;
			Mg.efpos_s = (Mg.raypos_s = true);
			Mg.Atk0.PublishMagic = Mg;
			float sa = Mg.sa;
			Mg.sa = X.XORSPS() * 3.1415927f * 0.025f;
			Mg.da = -1000f;
			Mg.PtcVar("agR", (double)sa).PtcVar("hagR", (double)(sa + 1.5707964f));
			Mg.aimagr_calc_s = true;
			if (Mg.Dro == null)
			{
				Mg.createDropper(Mg.dx, Mg.dy, 0.125f, -1f, -1f);
			}
			Mg.Dro.type |= (DROP_TYPE)384;
			Mg.Dro.size = Mg.Mn._0.thick;
			Mg.Dro.gravity_scale = Mg.Mn._0.accel;
			Mg.Dro.bounce_x_reduce = 0.7f;
			Mg.Dro.bounce_y_reduce = 0.37f;
			Mg.Dro.snd_key = this.snd_key_dro;
			Mg.Dro.FD_DropObjectBccTouch = Mem.FD_DropObjectBccTouch;
			Mg.phase++;
			Mg.calcAimPos(false);
			Mg.da = Mg.aim_agR;
		}

		public override bool run(MagicItem Mg, float fcnt)
		{
			if (Mg.phase < 100)
			{
				return true;
			}
			MgItemBomb.MgBombMem mgBombMem = Mg.Other as MgItemBomb.MgBombMem;
			if (mgBombMem == null)
			{
				return Mg.t < 40f;
			}
			PR pr = Mg.Caster as PR;
			Map2d mp = Mg.Mp;
			if (pr == null)
			{
				return false;
			}
			int num = Mg.phase / 100;
			int num2 = Mg.phase % 100;
			if (num2 == 0)
			{
				this.initBombPhase0(Mg, mgBombMem);
				num2 = 1;
			}
			if (num2 < 10)
			{
				Mg.Dro.vx = 0f;
				Mg.Dro.vy = 0f;
				if (!Mg.Caster.canHoldMagic(Mg))
				{
					this.initThrow(Mg, true, false);
					num2 = Mg.phase % 100;
				}
			}
			if (num2 == 1)
			{
				Mg.Dro.x = Mg.Cen.x;
				Mg.Dro.y = Mg.Cen.y;
			}
			if (num2 == 2)
			{
				if (Mg.da == -1000f)
				{
					Mg.calcAimPos(false);
				}
				else
				{
					Mg.calced_aim_pos = true;
					Mg.aim_agR = Mg.da;
				}
				float num3 = (float)(-(float)CAim._XD(pr.getAimForCaster(), 1)) * ((-10f + 33f * X.ZSINV(Mg.t, 15f)) * mp.rCLENB);
				float num4 = -(89f * X.ZSIN(Mg.t, 15f) * mp.rCLENB);
				Mg.Dro.x = (Mg.sx = Mg.Cen.x + num3);
				Mg.Dro.y = (Mg.sy = Mg.Cen.y + num4);
				Mg.Ray.PosMap(Mg.sx, Mg.sy);
			}
			if (num2 < 90)
			{
				mgBombMem.recheckFoot();
			}
			if (num2 >= 10 && num2 < 90)
			{
				Mg.sz += fcnt;
				bool flag = Mg.sz >= 4f;
				Mg.sa += Mg.da;
				float num5 = 0.015f;
				if (Mg.Dro.on_ground)
				{
					num5 = 0f;
				}
				if (!this.runFlying(Mg, fcnt, Mg.Dro.on_ground))
				{
					num2 = 95;
					Mg.phase = num * 100 + num2;
					Mg.t = 0f;
				}
				else
				{
					if (Mg.Dro.x_bounced)
					{
						Mg.da *= -1f;
						flag = true;
					}
					if (Mg.Dro.y_bounced)
					{
						Mg.da *= 0.88f;
						flag = true;
					}
					Mg.Dro.gravity_scale = X.VALWALK(Mg.Dro.gravity_scale, 0.2f, 0.008f);
					if (num2 == 10 && Mg.t >= Mg.Mn._0.maxt - 100f)
					{
						num2 = 11;
						Mg.t = 0f;
						Mg.phase = num * 100 + num2;
						if (this.ptc_key_explode_prepare != null)
						{
							Mg.PtcST(this.ptc_key_explode_prepare, PTCThread.StFollow.NO_FOLLOW, false);
						}
					}
					if (num2 == 11 && Mg.t >= 100f)
					{
						mgBombMem.Con.initExplode(Mg);
						num2 = Mg.phase % 100;
					}
					if (num2 == 15 && Mg.t >= 200f)
					{
						mgBombMem.Con.initExplode(Mg);
						num2 = Mg.phase % 100;
					}
					if (num2 >= 10 && num2 < 90)
					{
						Mg.calcAimPos(false);
						Mg.MnSetRay(Mg.Ray, 0, Mg.aim_agR, 0f);
						Mg.Ray.LenM(num5);
						if (Mg.Atk0 != null)
						{
							Mg.Ray.check_hit_wall = false;
							Mg.Ray.check_mv_hit = true;
							Mg.Ray.check_other_hit = false;
							Mg.Ray.hittype &= ~(HITTYPE.AUTO_TARGET | HITTYPE.GUARD_IGNORE | HITTYPE.TARGET_CHECKER);
							if (!this.ReflectCheck(Mg, Mg.Ray.hittype, num2))
							{
								num2 = 95;
								Mg.phase = num * 100 + num2;
								Mg.t = 0f;
							}
							Mg.Ray.clearReflectBuffer();
							HITTYPE hittype = Mg.Ray.Cast(false, null, true);
							if ((hittype & HITTYPE.KILLED) != HITTYPE.NONE || Mg.t >= 280f)
							{
								return false;
							}
							if ((hittype & HITTYPE.BREAK) != HITTYPE.NONE)
							{
								Mg.Dro.vx *= -0.5f;
							}
						}
						if (flag)
						{
							Mg.sz = 0f;
							mgBombMem.Stroker.Add(Mg.sx, Mg.sy);
						}
					}
				}
			}
			if (num2 == 90)
			{
				Mg.sa += Mg.da;
				Mg.sa = X.VALWALK(Mg.sa, 0f, 0.001256637f);
				if (Mg.t >= 100f)
				{
					num2 = 95;
					Mg.phase = num * 100 + num2;
					Mg.t = 0f;
				}
			}
			if (num2 >= 95)
			{
				bool flag2 = false;
				if (num2 == 95)
				{
					num2 = 96;
					Mg.phase = num * 100 + num2;
					Mg.t = 0f;
					Mg.killEffect();
					Mg.PtcVar("radius", (double)(Mg.Mn._1.thick * Mg.CLENB)).PtcVar("tx", (double)Mg.sx).PtcVar("ty", (double)Mg.sy)
						.PtcVar("zr", (double)(Mg.Mn._1.thick / this.explode_radius_min));
					if (this.ptc_key_explode != null)
					{
						Mg.PtcST(this.ptc_key_explode, PTCThread.StFollow.NO_FOLLOW, false);
					}
					Mg.Ray.clearHittedTarget();
					Mg.Ray.HitLock(-1f, null);
					if (Mg.Dro != null)
					{
						Mg.Dro.destruct(true);
						Mg.Dro = null;
					}
					Mg.Mn._1.wall_hit = true;
					Mg.Mn._1.other_hit = EnemySummoner.isActiveBorder();
					flag2 = true;
					Mg.projectile_power = 100;
				}
				this.explodeExecute(Mg, flag2);
				return Mg.t < this.explode_maxt;
			}
			return true;
		}

		protected virtual void explodeExecute(MagicItem Mg, bool first)
		{
			Mg.MnSetRay(Mg.Ray.PosMap(Mg.sx, Mg.sy), 1, Mg.sa, Mg.t);
			Mg.Ray.check_mv_hit = true;
			Mg.Ray.hittype &= ~(HITTYPE.REFLECTED | HITTYPE.REFLECT_BROKEN | HITTYPE.REFLECT_KILLED);
			if ((Mg.hittype & MGHIT.PR) != (MGHIT)0)
			{
				Mg.Ray.hittype |= HITTYPE.PR;
				Mg.Atk0.pr_myself_fire = true;
			}
			else
			{
				Mg.Ray.hittype &= ~HITTYPE.PR;
				Mg.Atk0.pr_myself_fire = false;
			}
			Mg.Ray.Atk = Mg.Atk0;
			Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0.Burst(0.2f, -0.04f), HITTYPE.NONE);
			Mg.Mn._1.wall_hit = (Mg.Mn._1.other_hit = false);
		}

		public override bool draw(MagicItem Mg, float fcnt)
		{
			EffectItem ef = Mg.Ef;
			if (Mg.phase < 100)
			{
				return true;
			}
			MgItemBomb.MgBombMem mgBombMem = Mg.Other as MgItemBomb.MgBombMem;
			int num = Mg.phase / 100;
			int num2 = Mg.phase % 100;
			if (num2 <= 1 || num2 >= 95 || mgBombMem == null)
			{
				return true;
			}
			MeshDrawer mesh = ef.GetMesh("", MTRX.MIicon.getMtr(BLEND.NORMALP3, -1), true);
			MeshDrawer mesh2 = ef.GetMesh("", uint.MaxValue, BLEND.ADD, true);
			MeshDrawer mesh3 = ef.GetMesh("", uint.MaxValue, BLEND.SUB, true);
			mesh.allocUv23(4, false);
			mesh.ColGrd.Set(uint.MaxValue);
			mesh.Col = mesh.ColGrd.C;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			if (num2 >= 11)
			{
				mesh.ColGrd.blend(4294001732U, (0.5f + 0.5f * X.ZPOW(Mg.t, 100f)) * (0.5f * X.COSI(Mg.t, 13.3f)));
				if (num2 == 15)
				{
					num3 = X.NI(1f, 0.25f, X.ZSINV(200f - Mg.t, 200f));
				}
				else
				{
					num3 = X.NI(0.125f, 1f, X.ZSIN(Mg.t, 100f));
				}
				mesh.Uv23(mesh.ColGrd.Set(15811652).multiply(num3, true).C, false);
				if (num2 == 11)
				{
					MeshDrawer mesh4 = ef.GetMesh("", MTRX.MIicon.getMtr(BLEND.SUB, -1), true);
					mesh4.Col = mesh4.ColGrd.Set(this.col_explode_sub_circle).mulA(num3).C;
					mesh4.initForImg(MTRX.EffBlurCircle245, 0);
					float num6 = Mg.Mn._1.thick * Mg.CLENB;
					float num7 = X.NI(0.6f, 1f, num3);
					mesh4.Rect(0f, 0f, num6 * 2f, num6 * 2f, false);
					mesh4.initForImg(MTRX.EffCircle128, 0);
					mesh4.Col = mesh4.ColGrd.mulA(0.25f).C;
					mesh4.Rect(0f, 0f, num6 * 2f * num7, num6 * 2f * num7, false);
					this.drawExplodePrepare(Mg, mesh2, num3, num6, num7);
				}
				num4 = X.COSI(Mg.t, 3.3f) * 2.3f * num3;
				num5 = X.COSI(Mg.t, 4.13f) * 2.3f * num3;
			}
			else
			{
				mesh.Uv23(mesh.ColGrd.Set(0).C, false);
			}
			PxlFrame frame = MTR.SqEffectItemBomb.getFrame(((num2 >= 10) ? 1 : 0) + this.pf_start_id);
			float num8 = 1f + num3 * 0.35f;
			if (num3 > 0f)
			{
				MTRX.Expander.Divide(2).drawTo(mesh, 0f, 0f, 1f, Mg.sa, frame.getLayer(0), num3, num8);
			}
			else
			{
				mesh.RotaPF(num4, num5, num8, num8, Mg.sa, frame, false, false, false, 1U, false, 0);
			}
			mesh.RotaPF(num4, num5, 1f, 1f, Mg.sa, frame, false, false, false, 65534U, false, 0);
			mesh.allocUv23(0, true);
			mesh2.base_x = (mesh2.base_y = 0f);
			mesh3.base_x = (mesh3.base_y = 0f);
			mgBombMem.Stroker.BlurLineWTo(mesh2, 0f, 0f, 2f, 16f, 0f, C32.d2c(this.col_stroke_add0), C32.d2c(this.col_stroke_add1), 0.5f);
			mgBombMem.Stroker.BlurLineWTo(mesh3, 0f, 0f, 4f, 23f, 0f, C32.d2c(this.col_stroke_sub0), C32.d2c(this.col_stroke_sub1), 0.5f);
			return true;
		}

		public abstract void drawExplodePrepare(MagicItem Mg, MeshDrawer MdAdd, float count_tz, float r, float scl);

		public bool fnDrawSelfExplodeCutin(MagicItem Mg, EffectItem Ef)
		{
			MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, false);
			Vector3 posMainTransform = Mg.M2D.Cam.PosMainTransform;
			float num = X.ZSIN(Ef.af, 13f);
			float num2 = X.ZLINE(98.27778f - Ef.af, 10f);
			if (num2 <= 0f)
			{
				return false;
			}
			Color32 c = meshImg.ColGrd.Set(Ef.time).setA1(num * num2).C;
			meshImg.Col = meshImg.ColGrd.Set(4286019447U).mulA(num * num2).C;
			MeshDrawer meshDrawer = meshImg;
			MeshDrawer meshDrawer2 = meshImg;
			float num3 = posMainTransform.x;
			float num4 = posMainTransform.y;
			meshDrawer.base_x = num3;
			meshDrawer2.base_y = num4;
			float num5 = 70f + 60f * X.ZLINE(Ef.af, 98.27778f);
			float num6 = 0f;
			if (Ef.af >= 15f)
			{
				num6 = (float)X.IntR(8f * X.ZSINV(Ef.af - 25f, 30f) * (X.COSIT(5.13f) * 0.75f + X.COSIT(11.38f) * 0.25f));
			}
			PxlFrame pxlFrame = MTR.AItemIcon[(int)Ef.y];
			meshImg.initForImg(MTRX.EffCircle128, 0).Rect(0f, 0f, num5 * 2f, num5 * 2f, false);
			meshImg.Col = c;
			meshImg.RotaPF(num6, 0f, 5f, 5f, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
			if (Ef.af >= 20f)
			{
				MeshDrawer mesh = Ef.GetMesh("selfex_mask", MTRX.getMtr(BLEND.MASK, 21), false);
				MeshDrawer mesh2 = Ef.GetMesh("selfex_mask_mt", MTRX.MIicon.getMtr(BLEND.NORMAL, 21), false);
				meshDrawer = mesh;
				num4 = posMainTransform.x;
				num3 = posMainTransform.y;
				meshDrawer.base_x = num4;
				mesh.base_y = num3;
				meshDrawer = mesh2;
				MeshDrawer meshDrawer3 = mesh2;
				num3 = posMainTransform.x;
				num4 = posMainTransform.y;
				meshDrawer.base_x = num3;
				meshDrawer3.base_y = num4;
				mesh.base_z -= 0.02f;
				mesh2.base_z -= 0.022f;
				float num7 = X.ZLINE(Ef.af - 20f, 58.27778f);
				mesh.Col = mesh.ColGrd.Set(c).setA1(1f).blend(4278190080U, 0.5f + 0.5f * X.COSIT(28f))
					.mulA(num * num2)
					.C;
				mesh.Circle(0f, 0f, num5 * num7, 0f, false, 0f, 0f);
				mesh2.Col = mesh2.ColGrd.Black().mulA(num * num2).C;
				mesh2.RotaPF(num6, 0f, 5f, 5f, 0f, pxlFrame, false, false, false, uint.MaxValue, false, 0);
			}
			return true;
		}

		public bool isHoldPrepare(MagicItem Mg)
		{
			return Mg.phase % 100 == 2;
		}

		public bool initHoldPrepare(MagicItem Mg)
		{
			int num = Mg.phase / 100;
			if (Mg.phase % 100 == 1)
			{
				Mg.phase = num * 100 + 2;
				Mg.t = 0f;
				return true;
			}
			return false;
		}

		public bool initEatenProcess(MagicItem Mg)
		{
			int num = Mg.phase / 100;
			int num2 = Mg.phase % 100;
			Mg.phase = num * 100 + 90;
			Mg.t = 0f;
			Mg.wind_apply_s_level = 0f;
			Mg.da = 6.2831855f * X.NIXP(20f, 44f) * (float)X.MPFXP();
			Mg.hittype &= (MGHIT)(-6);
			if (Mg.Dro != null)
			{
				Mg.Dro.destruct(true);
				Mg.Dro = null;
			}
			if (num2 < 11)
			{
				Mg.PtcST("itembomb_explode_prepare", PTCThread.StFollow.NO_FOLLOW, false);
			}
			return true;
		}

		public virtual void initThrow(MagicItem Mg, bool dropping = false, bool self_explode_bomb = false)
		{
			int num = Mg.phase / 100;
			Mg.phase = num * 100 + 10;
			Mg.t = 0f;
			(Mg.Other as MgItemBomb.MgBombMem).item_back_to_storage = false;
			Mg.aimagr_calc_s = (Mg.aimagr_calc_vector_d = true);
			if (!dropping)
			{
				Mg.da = (float)(-(float)CAim._XD(Mg.Caster.getAimForCaster(), 1)) * (1f / X.NIXP(25f, 50f)) * 6.2831855f;
				Mg.Dro.vx = Mg.Mn._0.v0 * X.Cos(Mg.Mn._0.agR);
				Mg.Dro.vy = Mg.Mn._0.v0 * -X.Sin(Mg.Mn._0.agR);
			}
			Mg.Dro.gravity_scale = 0.2f;
			Mg.MGC.initRayCohitable(Mg.Ray);
			Mg.Ray.cohitable_allow_berserk = M2Ray.COHIT.BERSERK_N;
			Mg.aimagr_calc_vector_d = true;
			Mg.calcAimPos(false);
			if (!dropping && !self_explode_bomb)
			{
				Mg.PtcVar("agR", (double)Mg.aim_agR).PtcST("itembomb_activate", PTCThread.StFollow.NO_FOLLOW, false);
			}
			Mg.wind_apply_s_level = 0.15f;
		}

		public bool initExplode(MagicItem Mg)
		{
			int num = Mg.phase / 100;
			if (Mg.phase % 100 < 95)
			{
				Mg.phase = num * 100 + 95;
				Mg.t = 0f;
				return true;
			}
			return false;
		}

		public void initSelfExplodeThrow(MgItemBomb.MgBombMem Mem, NelItem Itm, bool first)
		{
			MagicItem mg = Mem.Mg;
			int num = mg.phase / 100;
			if (mg.phase % 100 < 15)
			{
				if (first)
				{
					mg.M2D.loadMaterialSnd("item_bomb");
				}
				if (mg.Dro == null)
				{
					this.initBombPhase0(mg, Mem);
					this.initThrow(mg, true, true);
				}
				mg.phase = num * 100 + 15;
				mg.Dro.vx = X.NIXP(-0.04f, 0.04f);
				mg.Dro.vy = ((X.XORSP() < 0.3f) ? X.NIXP(-0.02f, -0.08f) : X.NIXP(-0.04f, 0.04f));
				mg.t = 187f + X.NIXP(-5f, 5f) * 0.144f;
				mg.Dro.snd_key = "";
				if (first)
				{
					mg.M2D.loadMaterialSnd("item_bomb");
					(Mem.Mg.Caster as M2Mover).playSndPos("itembomb_selfexplode_tick", 1);
					mg.M2D.PE.setSlow(90.27778f, 0.144f, 0);
					mg.M2D.PE.addTimeFixedEffect(mg.Mp.getEffectTop().setEffectWithSpecificFn("self_explode_bomb", 0f, (float)Itm.getIcon(null, null), 0f, (int)(C32.c2d(Itm.getColor(null)) & 16777215U), 0, Mem.FD_drawSelfExplodeCutin), 1f);
					mg.M2D.PE.addTimeFixedEffect(mg.M2D.PE.setPEfadeinout(POSTM.SHOTGUN, 15f, 130.27777f, 0.5f, 0), 1f);
				}
			}
		}

		public bool alreadyAppear(MagicItem Mg)
		{
			return Mg.phase % 100 >= 2;
		}

		public bool alreadyThrown(MagicItem Mg)
		{
			return Mg.phase % 100 >= 10;
		}

		public bool alreadyThrownSelfExplode(MagicItem Mg)
		{
			int num = Mg.phase % 100;
			return num == 15 || num == 16;
		}

		public bool alreadyExploded(MagicItem Mg)
		{
			return Mg.phase % 100 >= 95;
		}

		public bool alreadyEatenProcess(MagicItem Mg)
		{
			return Mg.phase % 100 == 90;
		}

		protected virtual bool runFlying(MagicItem Mg, float fcnt, bool on_ground)
		{
			if (on_ground)
			{
				Mg.Dro.vx = X.VALWALK(Mg.Dro.vx, 0f, 0.6f * fcnt);
				Mg.da = 0f;
			}
			return true;
		}

		protected virtual bool ReflectCheck(MagicItem Mg, HITTYPE hitres, int fphase)
		{
			if ((hitres & (HITTYPE.REFLECTED | HITTYPE.REFLECT_BROKEN)) != HITTYPE.NONE)
			{
				if (!this.alreadyThrownSelfExplode(Mg))
				{
					Mg.reflectV(Mg.Ray, ref Mg.Dro.vx, ref Mg.Dro.vy, 0.15f, 1f, true);
					Mg.Dro.vx = X.absMn(Mg.Dro.vx, 0.15f);
					Mg.Dro.vy = X.absMn(Mg.Dro.vy + (float)X.MPF(Mg.Dro.vy > 0f) * 0.08f, 0.14f);
					Mg.Dro.bounce_y_reduce = 0.5f;
					Mg.da = (float)X.MPF(Mg.Dro.vx > 0f) * (1f / X.NIXP(25f, 50f)) * 6.2831855f;
					Mg.Dro.gravity_scale = 0.1f;
				}
				Mg.Ray.hittype &= ~(HITTYPE.REFLECTED | HITTYPE.REFLECT_BROKEN);
			}
			return true;
		}

		public virtual void applyMapDamage(MagicItem Mg, M2MapDamageContainer.M2MapDamageItem MapDmg, M2BlockColliderContainer.BCCLine Bcc)
		{
		}

		public bool fnManipulate(MagicItem Mg, M2MagicCaster _Mv, float fcnt)
		{
			if (_Mv == null || _Mv != Mg.Caster || base.isWrongMagic(Mg, _Mv) || Mg.already_reflected)
			{
				return false;
			}
			if (Mg.phase % 100 >= 10)
			{
				return false;
			}
			MagicNotifiear mn = Mg.Mn;
			Map2d mp = Mg.Mp;
			if (_Mv is PR)
			{
				PR pr = _Mv as PR;
				int aimDirection = pr.Skill.getAimDirection();
				if (aimDirection >= 0)
				{
					mn._0.v0 = 0.17f;
					Mg.da = CAim.get_agR((AIM)aimDirection, 0f);
					if (aimDirection == 3 || aimDirection == 1)
					{
						Mg.da = (1.5707964f - pr.mpf_is_right * ((aimDirection == 1) ? 0.026f : 0.07f) * 3.1415927f) * (float)X.MPF(aimDirection == 1);
					}
					else
					{
						int num = CAim._YD(aimDirection, 1);
						Mg.da = 1.5707964f + X.angledifR(1.5707964f, Mg.da) * ((num < 0) ? 0.66f : ((num > 0) ? 0.3f : 0.5f));
					}
				}
				else
				{
					Vector2 aimPos = pr.Skill.getAimPos(Mg);
					float num2 = aimPos.x - Mg.sx;
					float num3 = aimPos.y - Mg.sy;
					float num4 = mp.GAR(0f, 0f, num2, num3);
					if (X.Abs(num2) < 1f)
					{
						Mg.da = (mn._0.agR = num4);
						mn._0.v0 = X.NIL(0.008f, 0.17f, X.LENGTHXYS(0f, 0f, num2, num3) - 1f, 3f);
					}
					else
					{
						float num5 = X.NIL(0.008f, 0.17f, X.LENGTHXYS(0f, 0f, num2, num3) - 1.5f, 6f) * (float)X.MPF(num2 > 0f);
						float num6 = num2 / num5;
						float num7 = (X.Mx(0f, num3 / X.Abs(num2)) + 1f) * X.Abs(num5);
						float num8 = X.Mx((num3 - 0.3f) / num6 - 0.5f * M2DropObject.getGravityVelocity(mp, mn._0.accel) * num6, -num7);
						float num9 = mp.GAR(0f, 0f, num5, num8);
						Mg.da = num9;
						Mg.calced_aim_pos = true;
						float num10 = X.Sin(num9);
						mn._0.v0 = ((num10 == 0f) ? 0.17f : X.MMX(0.008f, X.Abs(num8 / num10), 0.17f));
					}
				}
				mn._0.aim_fixed = true;
				Mg.aim_agR = (mn._0.agR = Mg.da);
			}
			return true;
		}

		protected string init_ptcst_key = "";

		private const float v0_min = 0.008f;

		private const float v0_max = 0.17f;

		private const float gravity_scale = 0.2f;

		private const float rotate_spd_ratio = 1f;

		private const float reduce_ax_in_ground = 0.6f;

		private const float countdown_t = 100f;

		private const float wind_apply_level = 0.15f;

		protected float explode_radius_min = 2.5f;

		protected float explode_radius_max = 2.5f;

		protected int pf_start_id;

		protected string ptc_key_explode;

		protected string ptc_key_explode_prepare;

		protected string snd_key_dro = "";

		protected float explode_maxt = 4f;

		protected uint col_stroke_add0 = 4294119531U;

		protected uint col_stroke_add1 = 15929451U;

		protected uint col_stroke_sub0 = 2568080882U;

		protected uint col_stroke_sub1 = 1146610U;

		protected uint col_explode_sub_circle;

		private const float selfexp_maxt = 13f;

		private const float selfexp_TS = 0.144f;

		public class MgBombMem : IDisposable, IMapDamageListener, IMgBombListener
		{
			public M2BlockColliderContainer.BCCLine CurBCC { get; private set; }

			public void Init(MagicItem _Mg, MgItemBomb _Con)
			{
				this.item_back_to_storage = false;
				this.Con = _Con;
				this.Mg = _Mg;
				this.Mg.input_null_to_other_when_quit = true;
				this.Stroker.Clear();
				this.CurBCC = null;
				this.ABccFoot.Clear();
				if (this.FD_DropObjectBccTouch == null)
				{
					this.FD_DropObjectBccTouch = delegate(M2DropObject Dro, M2BlockColliderContainer.BCCLine Bcc, bool bounced)
					{
						if (this.Mg == null)
						{
							return;
						}
						this.recheckCurFoot(Bcc);
						if (bounced)
						{
							this.check_lock = this.Mg.Mp.floort + 4f;
						}
					};
				}
			}

			public void InitItem(NelItem _Data, int _grade)
			{
				this.Data = _Data;
				this.grade = _grade;
				this.Con.InitItem(this);
			}

			public void Dispose()
			{
				this.Consume();
				MgItemBomb con = this.Con;
				this.Mg = null;
				this.Con = null;
				this.Data = null;
				if (con != null)
				{
					M2FootManager.BccFootCheckDL(this, this.CurBCC, this.ABccFoot, 8U);
					this.ABccFoot.Clear();
					con.ReleaseMem(this);
				}
				this.CurBCC = null;
			}

			public void Consume()
			{
				if (this.Data != null && this.item_back_to_storage)
				{
					NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
					if (nelM2DBase != null)
					{
						nelM2DBase.IMNG.getInventory().Add(this.Data, 1, this.grade, true, true);
						nelM2DBase.IMNG.need_fine_bomb_self = true;
					}
				}
				this.item_back_to_storage = false;
			}

			public DRect getMapBounds(DRect BufRc)
			{
				if (this.Mg == null || this.Mg.Dro == null)
				{
					return null;
				}
				BufRc = BufRc ?? M2BlockColliderContainer.BufRc;
				return BufRc.Set(this.Mg.Dro.x - this.Mg.Dro.z, this.Mg.Dro.y - this.Mg.Dro.z, this.Mg.Dro.z * 2f, this.Mg.Dro.z * 2f);
			}

			public M2BlockColliderContainer.BCCLine get_FootBCC()
			{
				return this.CurBCC;
			}

			public void recheckFoot()
			{
				if (this.Mg.Mp.floort >= this.check_lock)
				{
					this.recheckCurFoot((this.Mg.Dro != null) ? this.Mg.Dro.get_FootBcc() : null);
				}
				M2FootManager.BccFootRunDL(this, this.CurBCC, this.ABccFoot, 8U);
			}

			private void recheckCurFoot(M2BlockColliderContainer.BCCLine _Bcc)
			{
				if (_Bcc != this.CurBCC)
				{
					this.CurBCC = _Bcc;
					M2FootManager.BccFootCheckDL(this, this.CurBCC, this.ABccFoot, 8U);
				}
			}

			public void remFootListener(IBCCFootListener Lsn)
			{
				this.ABccFoot.Remove(Lsn);
			}

			public void addOnIce()
			{
			}

			public void applyMapDamage(M2MapDamageContainer.M2MapDamageItem MapDmg, M2BlockColliderContainer.BCCLine Bcc)
			{
				if (this.Mg == null || this.Mg.Dro == null)
				{
					return;
				}
				this.Con.applyMapDamage(this.Mg, MapDmg, Bcc);
			}

			public bool isCenterPr()
			{
				return false;
			}

			public bool isEatableBomb(MagicItem Mg, M2MagicCaster CheckedBy, bool execute)
			{
				if (Mg.Other as MgItemBomb.MgBombMem != this || Mg.Caster == CheckedBy)
				{
					return false;
				}
				if (this.Con.alreadyExploded(Mg) || this.Con.alreadyEatenProcess(Mg) || !this.Con.alreadyAppear(Mg))
				{
					return false;
				}
				if (execute)
				{
					this.Con.initEatenProcess(Mg);
				}
				return true;
			}

			public bool forceExplode(MagicItem Mg)
			{
				return Mg.Other as MgItemBomb.MgBombMem == this && this.Con.initExplode(Mg);
			}

			public void applyVelocity(FOCTYPE type, float velocity_x, float velocity_y)
			{
				if (this.Mg != null && this.Mg.Dro != null)
				{
					this.Mg.Dro.vx += velocity_x;
					this.Mg.Dro.vy += velocity_y;
				}
			}

			public FnEffectRun FD_drawSelfExplodeCutin
			{
				get
				{
					if (this.FD_drawSelfExplodeCutin_ == null)
					{
						this.FD_drawSelfExplodeCutin_ = (EffectItem Ef) => this.Mg != null && this.Con.fnDrawSelfExplodeCutin(this.Mg, Ef);
					}
					return this.FD_drawSelfExplodeCutin_;
				}
			}

			public MgBombMem()
			{
				TrajectoryDrawer trajectoryDrawer = new TrajectoryDrawer(16);
				trajectoryDrawer.FD_PosConvert = delegate(Vector2 V)
				{
					M2DBase instance = M2DBase.Instance;
					if (instance != null && instance.curMap != null)
					{
						return new Vector2(instance.curMap.map2meshx(V.x), instance.curMap.map2meshy(V.y)) * instance.curMap.base_scale;
					}
					return V;
				};
				this.Stroker = trajectoryDrawer;
				base..ctor();
			}

			public MgItemBomb Con;

			public MagicItem Mg;

			public NelItem Data;

			public int grade;

			private float check_lock;

			public bool item_back_to_storage;

			private List<IBCCFootListener> ABccFoot = new List<IBCCFootListener>(1);

			public M2DropObject.FnDropObjectBccTouch FD_DropObjectBccTouch;

			public readonly TrajectoryDrawer Stroker;

			private FnEffectRun FD_drawSelfExplodeCutin_;
		}
	}
}
