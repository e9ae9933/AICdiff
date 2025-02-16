using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2WormTrap : MonoBehaviour, IRunAndDestroy, IM2RayHitAble, IFootable, IIrisOutListener, NearManager.INearLsnObject
	{
		public void appear(NelChipWormHead _Dr, Map2d _Mp)
		{
			this.Mp = _Mp;
			this.M2D = this.Mp.M2D as NelM2DBase;
			this.Dr = _Dr;
			this.OTrapPr = null;
			base.gameObject.isStatic = true;
			this.Cld = base.gameObject.AddComponent<BoxCollider2D>();
			this.Cld.size = new Vector2((float)this.Dr.mclms - X.Abs((float)CAim._XD(this.Dr.ahead, 1) * 0.6f), (float)this.Dr.mrows - ((this.Dr.ahead == AIM.T || this.Dr.ahead == AIM.B) ? X.Mn(0.95f, (float)this.Dr.mrows * 0.48f) : 0f)) * this.Mp.CLENB * 0.015625f;
			this.Cld.offset = new Vector2((float)(-(float)CAim._XD(this.Dr.ahead, 1)), (float)(-(float)CAim._YD(this.Dr.ahead, 1))) * this.Mp.CLENB * 0.35f * 0.015625f;
			this.Mp.M2D.loadMaterialSnd("m2darea_insect");
			this.Mp.M2D.Snd.Environment.AddLoop("areasnd_insect", this.Dr.unique_key, this.Dr.mcld_cx, this.Dr.mcld_cy, 6f, 6f, (float)this.Dr.mclms * 0.5f + 1f, (float)this.Dr.mrows * 0.5f + 1f, null);
			base.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			this.Mp.addLight(this.Lig = new M2LightFn(this.Mp, new M2LightFn.FnDrawLight(this.fnDrawLight), null, new Func<float, float, bool>(this.fnIsinCamera)), -1);
			this.Lig.Pos(this.Dr.mcld_cx, this.Dr.mcld_cy);
			this.Lig.radius = X.Mx(this.Dr.mcld_right - this.Dr.mcld_left, this.Dr.mcld_bottom - this.Dr.mcld_top) * this.Mp.CLEN;
			this.ran0 = X.xorsi();
			if (M2WormTrap.Beto == null)
			{
				M2WormTrap.Beto = BetoInfo.Worm.Pow(6, false);
			}
			this.Mp.NM.AssignForPr(this, _Dr.AttachCM != null, false);
			this.M2D.prepareSvTexture("damage_worm", false);
			this.M2D.FCutin.initPxl(false);
			this.extend_in_x = -0.4f;
			this.extend_in_y = -0.4f;
			if (CAim._XD(this.Dr.ahead, 1) != 0)
			{
				this.extend_in_x = -X.Mn(0.7f, (float)this.Dr.mclms * 0.488f);
				this.extend_in_y = -1f;
				return;
			}
			this.extend_in_x = -X.Mn(0.7f, (float)this.Dr.mclms * 0.488f);
			int num = ((this.Dr.ahead == AIM.T) ? ((int)(this.Dr.mcld_top + 0.1f)) : ((int)(this.Dr.mcld_bottom - 0.1f)));
			if (!this.Mp.canStand((int)(this.Dr.mcld_right + 0.1f), num))
			{
				this.extend_in_x += 0.25f;
				this.shift_pr_x += 0.25f;
			}
			if (!this.Mp.canStand((int)(this.Dr.mcld_left - 0.1f), num))
			{
				this.extend_in_x += 0.25f;
				this.shift_pr_x -= 0.25f;
			}
			if (this.Dr.ahead == AIM.T)
			{
				this.extend_in_y = -X.Mn(0.88f, (float)this.Dr.mrows * 0.488f);
				return;
			}
			this.extend_in_y = -X.Mn(0.7f, (float)this.Dr.mrows * 0.488f);
		}

		public void destruct()
		{
			this.iris_listener_assined = false;
			this.runner_assigned = false;
			this.remPostEffect(false);
			if (this.Mp != null)
			{
				if (this.Dr != null)
				{
					try
					{
						this.Mp.M2D.Snd.Environment.RemLoop("areasnd_insect", this.Dr.unique_key);
					}
					catch
					{
					}
				}
				this.Mp.remLight(this.Lig);
			}
			this.Dr = null;
		}

		public DRect getBounds(M2Mover Mv, DRect Dest)
		{
			if (this.Dr == null)
			{
				return null;
			}
			return Dest.Set(this.Dr.mcld_left, this.Dr.mcld_top, (float)this.Dr.mclms, (float)this.Dr.mrows);
		}

		public bool nearCheck(M2Mover Mv, NearTicket NTk)
		{
			PR pr = Mv as PR;
			if (pr == null)
			{
				return false;
			}
			if (this.isCovering(pr, 0f, 0f, 0f) && pr.canPullByWorm())
			{
				if (this.OTrapPr == null)
				{
					this.OTrapPr = new BDic<PR, M2WormTrap.TrapPr>(1);
				}
				if (X.Get<PR, M2WormTrap.TrapPr>(this.OTrapPr, pr) == null)
				{
					this.OTrapPr[pr] = new M2WormTrap.TrapPr();
				}
				this.runner_assigned = true;
				return false;
			}
			return true;
		}

		public bool run(float fcnt)
		{
			bool flag = false;
			bool flag2 = false;
			if (this.M2D.pre_map_active && this.Dr != null && this.OTrapPr != null)
			{
				foreach (KeyValuePair<PR, M2WormTrap.TrapPr> keyValuePair in this.OTrapPr)
				{
					PR key = keyValuePair.Key;
					M2WormTrap.TrapPr value = keyValuePair.Value;
					if (value.phase != -1000)
					{
						flag = true;
						if (value.t < 0f)
						{
							if (!this.isCovering(key, 0.4f, 0.4f, 0f))
							{
								value.phase = -1000;
								key.getPhysic().remLockGravity(this);
							}
							else if (!key.canPullByWorm())
							{
								value.t = X.VALWALK(value.t, 0f, Map2d.TS * 2f);
							}
							else
							{
								float num = X.ZLINE(value.t - 60f, 120f) * 0.3f;
								if (key.DMG.walkWormTrapped(this, value.t <= -20f && this.isCovering(key, this.extend_in_x + num, this.extend_in_y + num, this.shift_pr_x)))
								{
									value.t = 0f;
									value.phase = 0;
									value.submitted = false;
									value.ran = X.xors();
									if (this.EfHn == null)
									{
										this.EfHn = new EffectHandlerPE(2);
									}
									value.no_iris_out = this.M2D.FCutin.setE(key);
									if (!value.no_iris_out && !this.EfHn.isActive())
									{
										this.EfHn.Set(PostEffect.IT.setPE(POSTM.WORM_TRAPPED, 119f, 1f, 40));
										this.EfHn.Set(PostEffect.IT.setPE(POSTM.ZOOM2, 170f, 1f, 5));
									}
									key.getPhysic().getFootManager().rideInitTo(this, false);
									key.NM2D.Cam.TeCon.setBounceZoomIn(1.03125f, 10f, 30f, -4);
									float num2 = key.x + (this.Dr.mcld_cx - key.x) * 0.25f;
									float mcld_cy = this.Dr.mcld_cy;
									key.PtcVarS("attr", FEnum<MGATTR>.ToStr(MDAT.AtkWormTrap.attr)).PtcVar("hit_x", (double)num2).PtcVar("hit_y", (double)mcld_cy)
										.PtcST("hitabsorb", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
									flag2 = true;
									this.iris_listener_assined = true;
								}
								else
								{
									value.t -= Map2d.TS;
									if (value.t < -60f)
									{
										value.t += 60f;
										this.setWormEffect(key, 60, 6);
									}
								}
							}
						}
						else if (!key.isTrappedState())
						{
							if (value.no_iris_out)
							{
								this.M2D.FCutin.progressNextPhase(key);
							}
							this.forceReleaseSplashEffect(key);
							value.phase = -1000;
						}
						else
						{
							bool flag3 = this.wormTrapApplyDamageTo(key, value);
							key.DMG.walkWormTrapped(this, true);
							if (!value.submitted)
							{
								value.t += Map2d.TS;
								flag2 = true;
							}
							else
							{
								value.t += (float)(flag3 ? 1 : ((value.phase < 3) ? 3 : 2)) * Map2d.TS;
								flag2 = true;
							}
						}
					}
				}
			}
			if (!flag)
			{
				this.remPostEffect(true);
				this.iris_listener_assined = false;
				this.runner_assigned_ = false;
				this.OTrapPr.Clear();
				return false;
			}
			if (flag2 && this.EfHn != null)
			{
				this.EfHn.fine(100);
			}
			else
			{
				this.remPostEffect(true);
			}
			return true;
		}

		public void setWormEffect(PR Pr, int time, int saf)
		{
			this.Mp.setE("worm_trap_player", (this.Dr.ahead == AIM.L) ? X.Mx(Pr.mright, this.Dr.mcld_left + 0.25f) : ((this.Dr.ahead == AIM.R) ? X.Mn(Pr.mleft, this.Dr.mcld_right - 0.25f) : X.MMX(this.Dr.mcld_left + 0.5f, Pr.x, this.Dr.mcld_right - 0.5f)), (this.Dr.ahead == AIM.T) ? X.Mx(Pr.mbottom, this.Dr.mcld_top + 0.25f) : ((this.Dr.ahead == AIM.B) ? X.Mn(Pr.mtop, this.Dr.mcld_bottom - 0.25f) : X.MMX(this.Dr.mcld_top + 0.5f, Pr.y, this.Dr.mcld_bottom - 0.5f)), this.Dr.ahead, time, saf);
		}

		public void forceReleaseSplashEffect(PR Pr)
		{
			int particleCount = this.Mp.getEffect().getParticleCount(null, 13);
			float num = CAim.get_agR(this.head_aim, 0f);
			for (int i = 0; i < particleCount; i++)
			{
				float num2 = num + X.NIXP(-1f, 1f) * 0.38f * 3.1415927f;
				float num3 = X.NIXP(0.15f, 0.22f);
				M2DropObject m2DropObject = this.Mp.DropCon.Add(M2WormTrap.FD_drawReleaseWormSplash, Pr.x, Pr.y, num3 * X.Cos(num2), -num3 * X.Sin(num), 1f, 0f);
				m2DropObject.type = DROP_TYPE.NO_OPTION;
				m2DropObject.bounce_y_reduce = 0.13f;
				m2DropObject.size = 2f / Pr.CLEN;
			}
		}

		public static void setWormRelease(M2Mover Pr)
		{
			if (Pr == null)
			{
				return;
			}
			M2DropObject m2DropObject = Pr.Mp.DropCon.Add(M2WormTrap.FD_drawReleaseWormSplash, Pr.x + X.NIXP(-1f, 1f) * 1.65f * Pr.sizex, Pr.y + (-0.15f + X.NIXP(-0.2f, 0.2f)) * Pr.sizey, (float)(Pr.hasFoot() ? 1 : 0), 0f, 0f, 0f);
			m2DropObject.bounce_y_reduce = 0.13f;
			m2DropObject.type = DROP_TYPE.NO_OPTION;
			m2DropObject.vx = X.NIXP(0.008f, 0.012f) * (float)X.MPFXP();
			m2DropObject.vy = X.NIXP(0.012f, 0.016f);
			m2DropObject.size = ((m2DropObject.z > 0f) ? (Pr.sizey * X.NIXP(0.4f, 1.5f)) : 0f);
			m2DropObject.gravity_scale = 0.14f;
		}

		private static bool drawReleaseWormSplash(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			if (Dro.af >= 400f)
			{
				return false;
			}
			uint ran = X.GETRAN2(Dro.index + (int)Dro.time * 9 + 337, Dro.index % 7);
			MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, ran % 13U < 5U);
			PxlSequence awormRelease = MTR.AWormRelease;
			if (Dro.on_ground)
			{
				if (Dro.af_ground >= 140f)
				{
					return false;
				}
				meshImg.Col = meshImg.ColGrd.White().mulA(X.ZLINE(140f - Dro.af_ground, 50f)).C;
				if (Dro.size > 0.0625f && Dro.z > 0f)
				{
					int num = 4 + ((X.RAN(ran, 2005) < 0.5f) ? 1 : 0) * 4;
					int num2 = X.ANM((int)(Dro.af + X.RAN(ran, 1721) * 32f), 4, (float)X.IntR(4f + X.RAN(ran, 1336) * 2f));
					meshImg.RotaPF(0f, -1f, 2f, 2f, 0f, awormRelease[num + num2], Dro.vx > 0f, false, false, uint.MaxValue, false, 0);
					Dro.vx = X.NI(0.008f, 0.012f * X.RAN(ran, 1318), 0.5f) * (float)X.MPF(X.RAN(ran, 2322) > 0.5f);
					float size = Dro.size;
					Dro.size = X.VALWALK(Dro.size, 0.0625f, (float)X.AF * 0.04f);
					Dro.y += size - Dro.size;
				}
				else
				{
					int num3 = X.ANM((int)(Dro.af + X.RAN(ran, 721) * 32f), 4, (float)X.IntR(4f + X.RAN(ran, 1436) * 2f));
					Dro.vx = X.absMMX(0.014f, Dro.vx, X.NI(0.02f, 0.034f, X.RAN(ran, 2301)));
					meshImg.RotaPF(0f, 0f, 2f, 2f, 0f, awormRelease[num3], Dro.vx > 0f, false, false, uint.MaxValue, false, 0);
				}
			}
			else
			{
				float num4 = (float)((X.ANM((int)Dro.af, 4, 6f) + (int)(X.RAN(ran, 1227) * 4f)) * X.MPF(X.RANB(ran, 1909, 0.5f)));
				meshImg.RotaPF(0f, 0f, 2f, 2f, 1.5707964f * num4, awormRelease[12U + ran % 4U], X.RANB(ran, 2162, 0.5f), false, false, uint.MaxValue, false, 0);
			}
			return true;
		}

		public IFootable isCarryable(M2FootManager FootD)
		{
			PR pr = FootD.Mv as PR;
			if (this.OTrapPr == null || pr == null || this.Dr == null)
			{
				return null;
			}
			M2WormTrap.TrapPr trapPr = X.Get<PR, M2WormTrap.TrapPr>(this.OTrapPr, pr);
			if (trapPr == null || trapPr.t < 0f)
			{
				return null;
			}
			if (!X.isCovering(this.mcld_left, this.mcld_right, pr.mleft, pr.mright, 0.5f) || !X.isCovering(this.mcld_top, this.mcld_bottom, pr.mtop, pr.mbottom, 0.5f))
			{
				return null;
			}
			return this;
		}

		public void quitCarry(ICarryable FootD)
		{
		}

		public IFootable initCarry(ICarryable FootD)
		{
			return this;
		}

		public float get_carry_vx()
		{
			return 0f;
		}

		public float get_carry_vy()
		{
			return 0f;
		}

		public float fixToFootPos(M2FootManager FootD, float x, float y, out float dx, out float dy)
		{
			AIM head_aim = this.head_aim;
			if (head_aim == AIM.L || head_aim == AIM.R)
			{
				float num = X.Mx(FootD.sizey, 0.6f);
				dy = X.MMX(this.Dr.mcld_top + num, y, this.Dr.mcld_bottom - num);
				dx = this.Dr.mcld_cx + ((float)this.Dr.mclms * 0.5f - 0.5f) * (float)CAim._XD(this.head_aim, 1);
			}
			else
			{
				float num = X.Mx(FootD.sizex, 0.6f);
				dx = X.MMX(this.Dr.mcld_left + num, x, this.Dr.mcld_right - num);
				dy = this.Dr.mcld_cy - ((float)this.Dr.mrows * 0.5f - 0.5f) * (float)CAim._YD(this.head_aim, 1);
			}
			dx -= x;
			dy -= y;
			return 0.06f;
		}

		private bool wormTrapApplyDamageTo(PR Pr, M2WormTrap.TrapPr Trp)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			uint ran = Trp.ran;
			switch (Trp.phase)
			{
			case 0:
				flag = Trp.t >= X.NI(30, 40, X.RAN(ran, 1934));
				break;
			case 1:
				flag = Trp.t >= X.NI(67, 72, X.RAN(ran, 637));
				if (flag)
				{
					Pr.NM2D.Cam.TeCon.setBounceZoomIn(1.03125f, 10f, 30f, -4);
				}
				break;
			case 2:
				flag = Trp.t >= X.NI(80, 85, X.RAN(ran, 667));
				break;
			case 3:
				flag = Trp.t >= X.NI(90, 100, X.RAN(ran, 2598));
				break;
			case 4:
				flag = Trp.t >= X.NI(103, 115, X.RAN(ran, 1923));
				break;
			case 5:
				flag = Trp.t >= X.NI(116, 133, X.RAN(ran, 789));
				if (flag)
				{
					Pr.NM2D.Cam.TeCon.setBounceZoomIn(1.0625f, 15f, 60f, -4);
					Trp.kuchukuchu_t = X.NI(0.5f, 0.7f, X.RAN(ran, 2656));
				}
				break;
			default:
				if (Trp.phase >= 10)
				{
					if (!Pr.is_alive && Trp.phase >= 10 && X.XORSP() < 0.7f)
					{
						flag3 = true;
					}
					if (Trp.t >= 400f)
					{
						if (X.XORSP() < 0.2f + X.ZSIN(Trp.kuchukuchu_t - 0.4f, 0.3f) * 0.32f)
						{
							Trp.kuchukuchu_t = X.NI(0.2f, 0.7f, X.ZPOW(X.XORSP()));
						}
						Trp.t = 325f + 75f * X.Mn(0.94f, Trp.kuchukuchu_t + X.NI(0.02f, (!Pr.is_alive) ? 1.2f : 0.5f, (!Pr.is_alive) ? X.XORSP() : X.ZPOW(X.XORSP())));
						flag = true;
					}
				}
				else if (Trp.t >= (float)(120 + (Trp.phase - 7) * 55))
				{
					flag = true;
					Trp.t += X.NI(4, 30, X.RAN(ran, 485));
					flag2 = Trp.t >= 120f;
				}
				break;
			}
			if (flag)
			{
				int phase = Trp.phase;
				Trp.phase = phase + 1;
				MDAT.applyWormTrapDamage(Pr, phase, flag3);
				if (!flag3)
				{
					this.setWormEffect(Pr, 60, 6);
				}
			}
			return flag2;
		}

		public void fnDrawLight(MeshDrawer Md, M2LightFn Lt, float x, float y, float scale, float alpha)
		{
			uint ran = X.GETRAN2(this.ran0, 4 + this.ran0 % 7);
			float num = (this.Dr.mcld_right - this.Dr.mcld_left) * this.Mp.CLEN / 2f;
			float num2 = (this.Dr.mcld_bottom - this.Dr.mcld_top) * this.Mp.CLEN / 2f;
			Md.Col = Md.ColGrd.Set(4294453364U).mulA(0.06f + 0.009f * X.COSI(this.Mp.floort + X.RAN(ran, 1119) * 20f, 28.7f) + 0.009f * X.COSI(this.Mp.floort + X.RAN(ran, 2293) * 20f, 13.34f)).C;
			int num4;
			float num5;
			float num6;
			if (CAim._YD(this.head_aim, 1) != 0)
			{
				float num3 = X.Mx(0f, (float)this.Dr.mclms * this.Mp.CLEN - 1.8f);
				num4 = X.IntR(X.Mx(1f, num3 / 2.4f));
				x -= num3 * 0.5f;
				num5 = num3 / (float)num4;
				num6 = 0f;
			}
			else
			{
				float num7 = X.Mx(0f, (float)this.Dr.mrows * this.Mp.CLEN - 1.8f);
				num4 = X.IntR(X.Mx(1f, num7 / 2.4f));
				y -= num7 * 0.5f;
				num6 = num7 / (float)num4;
				num5 = 0f;
			}
			Md.initForImg(MTRX.EffBlurCircle245, 0);
			for (int i = 0; i < num4; i++)
			{
				uint ran2 = X.GETRAN2(this.Dr.index + 19 + i, 4 + i * 3);
				float num8 = x + 13f * X.COSI(this.Mp.floort + X.RAN(ran2, 2694) * 200f, 300f + 120f * X.RAN(ran2, 1323));
				float num9 = y + 13f * X.COSI(this.Mp.floort + X.RAN(ran2, 829) * 200f, 300f + 120f * X.RAN(ran2, 1952));
				float num10 = 2f * X.NI(44, 58, X.RAN(ran2, 553));
				Md.Rect(num8, num9, num10, num10, false);
				x += num5;
				y += num6;
			}
		}

		private bool fnIsinCamera(float meshx, float meshy)
		{
			float num = (float)this.Dr.mclms * this.Mp.CLEN;
			float num2 = (float)this.Dr.rows * this.Mp.CLEN;
			return this.Mp.M2D.Cam.isCoveringCenMeshPixel(meshx, meshy, num, num2, 40f);
		}

		public int applyHpDamage(int val, bool force = false, AttackInfo Atk = null)
		{
			return 0;
		}

		public bool isCovering(M2Mover Mv, float extend_x = 0f, float extend_y = 0f, float pr_shiftx = 0f)
		{
			return this.Dr != null && !(Mv == null) && X.isCovering(this.Dr.mcld_left, this.Dr.mcld_right, Mv.mleft + pr_shiftx, Mv.mright + pr_shiftx, extend_x) && X.isCovering(this.Dr.mcld_top, this.Dr.mcld_bottom, Mv.mtop, Mv.mbottom, extend_y);
		}

		public bool iris_listener_assined
		{
			get
			{
				return this.iris_listener_assined_;
			}
			set
			{
				if (value == this.iris_listener_assined_)
				{
					return;
				}
				this.iris_listener_assined_ = value;
				if (value)
				{
					this.NM2D.Iris.assignListener(this);
					return;
				}
				this.NM2D.Iris.deassignListener(this);
			}
		}

		public bool runner_assigned
		{
			get
			{
				return this.runner_assigned_;
			}
			set
			{
				if (value == this.runner_assigned_)
				{
					return;
				}
				this.runner_assigned_ = value;
				if (value)
				{
					this.Mp.addRunnerObject(this);
					return;
				}
				this.Mp.remRunnerObject(this);
			}
		}

		public bool initSubmitIrisOut(PR Pr, bool execute, ref bool no_iris_out)
		{
			if (this.OTrapPr == null)
			{
				return false;
			}
			foreach (KeyValuePair<PR, M2WormTrap.TrapPr> keyValuePair in this.OTrapPr)
			{
				M2WormTrap.TrapPr value = keyValuePair.Value;
				PR key = keyValuePair.Key;
				if (Pr == key && !value.submitted && value.t >= (float)(value.no_iris_out ? 30 : 40))
				{
					if (execute)
					{
						value.submitted = true;
					}
					if (value.no_iris_out)
					{
						if (execute)
						{
							this.M2D.FCutin.progressNextPhase(Pr);
						}
						no_iris_out = no_iris_out || value.no_iris_out;
					}
					return true;
				}
			}
			return false;
		}

		public bool warpInitIrisOut(PR Pr, ref PR.STATE changestate, ref string change_pose)
		{
			bool flag = false;
			if (this.OTrapPr == null)
			{
				return false;
			}
			foreach (KeyValuePair<PR, M2WormTrap.TrapPr> keyValuePair in this.OTrapPr)
			{
				M2WormTrap.TrapPr value = keyValuePair.Value;
				PR key = keyValuePair.Key;
				if (Pr == key)
				{
					if (value.submitted)
					{
						while (value.phase < 7)
						{
							M2WormTrap.TrapPr trapPr = value;
							int phase = trapPr.phase;
							trapPr.phase = phase + 1;
							MDAT.applyWormTrapDamage(Pr, phase, true);
						}
						try
						{
							this.NM2D.MGC.setMagic(Pr, MGKIND.EF_WORM_PUBLISH, MGHIT.IMMEDIATE);
						}
						catch
						{
						}
					}
					Pr.getFootManager().initJump(false, true, false);
					value.phase = -1000;
					this.remPostEffect(true);
					flag = true;
				}
			}
			return flag;
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				STB stb = TX.PopBld(null, 0);
				stb += "<WormTrap> ";
				stb += this.Dr.mcld_left;
				stb += ", ";
				stb += this.Dr.mcld_top;
				stb += ", ";
				stb += this.Dr.mclms;
				stb += ", ";
				stb += this.Dr.mrows;
				this._tostring = stb.ToString();
				TX.ReleaseBld(stb);
			}
			return this._tostring;
		}

		private void remPostEffect(bool del_instance = false)
		{
			if (this.EfHn != null)
			{
				this.EfHn.deactivate(del_instance);
			}
		}

		public float x
		{
			get
			{
				return this.Dr.mcld_cx;
			}
		}

		public float y
		{
			get
			{
				return this.Dr.mcld_cy;
			}
		}

		public float mcld_top
		{
			get
			{
				return this.Dr.mcld_top;
			}
		}

		public float mcld_bottom
		{
			get
			{
				return this.Dr.mcld_bottom;
			}
		}

		public float mcld_left
		{
			get
			{
				return this.Dr.mcld_left;
			}
		}

		public float mcld_right
		{
			get
			{
				return this.Dr.mcld_right;
			}
		}

		public float auto_target_priority(M2Mover CalcFrom)
		{
			return -1f;
		}

		public RAYHIT can_hit(M2Ray Ray)
		{
			return (((Ray.hittype & HITTYPE.CHANTED_MAGIC) == HITTYPE.NONE) ? RAYHIT.NONE : RAYHIT.KILL) | RAYHIT.NO_RETURN_MANA | RAYHIT.DO_NOT_AUTO_TARGET;
		}

		public HITTYPE getHitType(M2Ray Ray)
		{
			return HITTYPE.OTHER;
		}

		public AIM head_aim
		{
			get
			{
				return this.Dr.ahead;
			}
		}

		public NelM2DBase NM2D
		{
			get
			{
				return M2DBase.Instance as NelM2DBase;
			}
		}

		public IrisOutManager.IRISOUT_TYPE getIrisOutKey()
		{
			return IrisOutManager.IRISOUT_TYPE.WORM;
		}

		private const float zoom_scl0 = 1.5f;

		private const float zoom_scl1 = 1.75f;

		private const float zoom_scl2 = 2f;

		private Map2d Mp;

		private BoxCollider2D Cld;

		private NelM2DBase M2D;

		private NelChipWormHead Dr;

		private BDic<PR, M2WormTrap.TrapPr> OTrapPr;

		private EffectHandlerPE EfHn;

		private M2LightFn Lig;

		private const int TRAP_MAXT = 170;

		private const int CAN_RELEASE_PUSH_TIME = 40;

		private const int CAN_RELEASE_PUSH_TIME_SENSITIVE = 30;

		private int ran0;

		public static BetoInfo Beto;

		private const int beto_apply_count = 30;

		public const int worm_damage_count = 7;

		private bool iris_listener_assined_;

		private bool runner_assigned_;

		private bool assigned;

		private float extend_in_x = -0.4f;

		private float shift_pr_x;

		private float extend_in_y = -0.4f;

		private static M2DropObject.FnDropObjectDraw FD_drawReleaseWormSplash = new M2DropObject.FnDropObjectDraw(M2WormTrap.drawReleaseWormSplash);

		private string _tostring;

		private sealed class TrapPr
		{
			public float t = -1f;

			public int phase;

			public float kuchukuchu_t;

			public uint ran;

			public bool submitted;

			public bool no_iris_out;
		}
	}
}
