using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NASJumper : NelEnemyAssist
	{
		public NASJumper(NelEnemy _En, float _run_speed, float _suitable_len)
			: base(_En)
		{
			this.ADep = new List<NASJumper.RegInfo>(32);
			this.ADepSrc = new List<NASJumper.RegInfo>(32);
			this.run_speed = _run_speed;
			this.suitable_len = _suitable_len;
			this.ABcc0 = new List<M2BlockColliderContainer.BCCLine>(4);
			this.ARcDepertBuf = new DRect[2];
			this.ARcDepertBuf[0] = new DRect("depert0");
			this.ARcDepertBuf[1] = new DRect("depert1");
			this.En.getAI().no_check_move_fall_slide = true;
			if (this.RcWalk0 == null)
			{
				this.RcWalk0 = new DRect("RectS");
			}
			this.FD_fnSortDeperture = new Comparison<NASJumper.RegInfo>(this.fnSortDeperture);
		}

		public void quitTicket(NaTicket Tk)
		{
			this.setErrorDepert();
			this.dep_decided = false;
			this.ADep.Clear();
			this.RectDepert = null;
		}

		public bool Prog(NASJumper.JPROG p)
		{
			return this.fnJumpProgress == null || this.fnJumpProgress(this, p);
		}

		protected int fnSortDeperture(NASJumper.RegInfo Ra, NASJumper.RegInfo Rb)
		{
			float num = X.Abs(Ra.lgt - this.suitable_len) + (float)Ra.point_addition;
			float num2 = X.Abs(Rb.lgt - this.suitable_len) + (float)Rb.point_addition;
			if (num == num2)
			{
				return 0;
			}
			if (num >= num2)
			{
				return 1;
			}
			return -1;
		}

		public bool runJumpingWalk(bool init_flag, NaTicket Tk, ref float t, ref int walk_st, ref float walk_time)
		{
			if (init_flag)
			{
				if (base.Summoner == null)
				{
					return false;
				}
				t = 0f;
				walk_st = -1;
				walk_time = 0f;
				this.ADep.Clear();
				this.dep_decided = false;
				M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
				if (footBCC == null)
				{
					return this.runWalkError(null);
				}
				this.ABcc0.Clear();
				this.ABcc0.Add(footBCC);
				this.RcWalk0.Set(footBCC);
				for (int i = 0; i < 2; i++)
				{
					M2BlockColliderContainer.BCCLine bccline = footBCC;
					for (;;)
					{
						bccline = ((i == 0) ? bccline.SideL : bccline.SideR);
						if (bccline == null || bccline.foot_aim != footBCC.foot_aim)
						{
							break;
						}
						this.RcWalk0.ExpandRc(bccline, false);
					}
				}
				if (this.ADepSrc.Count == 0)
				{
					M2BlockColliderContainer.BCCLine[] footableBcc = base.Summoner.getFootableBcc(AIM.B);
					if (footableBcc.Length == 0)
					{
						return this.runWalkError(null);
					}
					int num = footableBcc.Length;
					this.ADepSrc.Capacity = (this.ADep.Capacity = num);
					for (int j = 0; j < num; j++)
					{
						this.ADepSrc.Add(new NASJumper.RegInfo(footableBcc[j], 0f));
					}
				}
			}
			if (Tk.prog == PROG.ACTIVE)
			{
				if (walk_st == -1)
				{
					walk_st = 0;
					if (Tk.type == NAI.TYPE.WALK_TO_WEED)
					{
						if (Tk.DepBCC == null)
						{
							return this.runWalkError(Tk);
						}
						this.ADep.Add(new NASJumper.RegInfo(Tk.DepBCC, (float)((int)Tk.depx)));
					}
					else
					{
						this.ADep.AddRange(this.ADepSrc);
						X.shuffle<NASJumper.RegInfo>(this.ADep, -1, null);
					}
					this.Prog(NASJumper.JPROG.INIT);
				}
				else if (walk_st >= 0)
				{
					int num2 = 20;
					M2BlockColliderContainer.BCCLine bccline2 = this.ABcc0[0];
					if (this.FootD.get_FootBCC() != bccline2)
					{
						return this.runWalkError(Tk);
					}
					while (walk_st < this.ADep.Count && --num2 > 0)
					{
						NASJumper.RegInfo regInfo = this.ADep[walk_st];
						bool flag = false;
						if (regInfo.getBcc() == bccline2 && Tk.type == NAI.TYPE.BACKSTEP)
						{
							flag = true;
						}
						else if (!regInfo.hasSafeArea())
						{
							flag = true;
						}
						else if (base.Nai.isDecliningXy(regInfo.cx, regInfo.cy, regInfo.w * 0.5f - 0.5f, regInfo.h * 0.5f - 0.5f))
						{
							flag = true;
						}
						else if (!regInfo.decidePosition(base.sizex, base.x, (float)X.IntR(base.mbottom), Tk.depx, bccline2, this.RcWalk0))
						{
							flag = true;
						}
						else if (!X.BTW(this.RcWalk0.x - bccline2.BCC.base_shift_x - this.jumpable_x_len, regInfo.posx, this.RcWalk0.right - bccline2.BCC.base_shift_x + this.jumpable_x_len))
						{
							flag = true;
						}
						else if (this.ADep.Count > 1)
						{
							regInfo.lgt = X.LENGTHXY2(base.Nai.target_x, base.Nai.target_lastfoot_bottom, regInfo.posx, regInfo.y);
							float num3 = regInfo.posx;
							regInfo.point_addition = 0;
							if (regInfo.getBcc() == bccline2)
							{
								regInfo.point_addition += 2000;
								num3 += this.En.getTargetPos().x - this.En.x;
							}
							else
							{
								num3 += X.Abs(this.En.getTargetPos().x - this.En.x) * (float)X.MPF(regInfo.posx < base.Nai.target_x);
							}
							this.calcPointAddition(Tk, regInfo);
						}
						if (flag)
						{
							this.ADep.RemoveAt(walk_st);
						}
						else
						{
							walk_st++;
						}
					}
					if (this.ADep.Count == 0)
					{
						return this.runWalkError(Tk);
					}
					if (walk_st >= this.ADep.Count)
					{
						this.ADep.Sort(this.FD_fnSortDeperture);
						walk_st = -2;
					}
				}
				else
				{
					NASJumper.RegInfo regInfo2 = this.ADep[0];
					M2BlockColliderContainer.BCCLine bccline3 = this.ABcc0[0];
					if (this.FootD.get_FootBCC() != bccline3)
					{
						return this.runWalkError(Tk);
					}
					if (regInfo2.getBcc() == bccline3)
					{
						Tk.prog = PROG.PROG2;
						t = 60f;
						walk_st = 0;
						this.RectDepert = null;
						this.En.AimToLr((regInfo2.posx > base.x) ? 2 : 0);
					}
					else
					{
						int num4 = 0;
						while (++num4 < 10)
						{
							float num5 = (float)X.IntR(base.mbottom);
							float num6 = -1f;
							float y = regInfo2.getBcc().linePosition(regInfo2.posx, base.y, 0f, 0f).y;
							this.RectDepert = null;
							for (int k = 0; k < 2; k++)
							{
								float num7 = (float)((k == 0 == regInfo2.posx < base.x) ? (-1) : 1);
								if (!((num7 < 0f) ? (this.RcWalk0.right - bccline3.BCC.base_shift_x < regInfo2.x) : (this.RcWalk0.x - bccline3.BCC.base_shift_x > regInfo2.r)))
								{
									bool flag2 = true;
									AIM aim = ((num7 < 0f) ? AIM.L : AIM.R);
									DRect drect = this.ARcDepertBuf[k];
									if (y > num5)
									{
										int num8 = (int)base.mtop - 1;
										int num9 = (int)(num5 - 1f);
										int num10 = (int)regInfo2.posx;
										int num11 = (int)base.x;
										for (int l = num8; l <= num9; l++)
										{
											M2Pt pointPuts = base.Mp.getPointPuts(num10, l, true, false);
											if (pointPuts == null || pointPuts.bcc_line_cfg || (l == num8 && pointPuts.getSideBcc(base.Mp, num10, l, AIM.B) != regInfo2.getBcc()))
											{
												flag2 = false;
												break;
											}
											M2BlockColliderContainer.BCCLine sideBcc = base.Mp.getSideBcc(num11, l, CAim.get_opposite(aim));
											if (sideBcc != null)
											{
												Vector3 vector = sideBcc.crosspointL(0f, 1f, -((float)l + 0.5f), 0f, 0f, 0f);
												if (vector.z == 2f && !((this.ABcc0.IndexOf(sideBcc) >= 0) ? this.RcWalk0.isin(vector.x + bccline3.BCC.base_shift_x, vector.y + bccline3.BCC.base_shift_y, -0.5f) : ((num7 > 0f) ? (vector.x < base.x) : (vector.x > base.x))))
												{
													flag2 = false;
													break;
												}
											}
										}
										if (!flag2)
										{
											goto IL_09C4;
										}
										float num12 = ((num7 < 0f) ? X.Mn(base.x - 0.0625f, this.RcWalk0.right - base.sizex - bccline3.BCC.base_shift_x) : X.Mx(base.x + 0.0625f, this.RcWalk0.x + base.sizex - bccline3.BCC.base_shift_x));
										drect.Set(num12 - base.sizex, base.mbottom - base.sizey * 2f, base.sizex * 2f, base.sizey * 2f);
									}
									else
									{
										float num13 = ((num7 < 0f) ? (regInfo2.r + base.sizex + 0.25f) : (regInfo2.x - base.sizex - 0.25f));
										if (!X.BTW(this.RcWalk0.x - bccline3.BCC.base_shift_x, num13, this.RcWalk0.right - bccline3.BCC.base_shift_x))
										{
											goto IL_09C4;
										}
										int num14 = (int)(num13 - base.sizex);
										int num15 = X.IntC(num13 + base.sizex) - 1;
										int num16 = (int)(y - base.sizey * 3f + 0.5f);
										for (int m = num14; m <= num15; m++)
										{
											M2Pt pointPuts2 = base.Mp.getPointPuts(m, num16, true, false);
											if (pointPuts2 == null || pointPuts2.bcc_line_cfg)
											{
												break;
											}
											M2BlockColliderContainer.BCCLine sideBcc2 = pointPuts2.getSideBcc(base.Mp, m, num16, AIM.B);
											if (sideBcc2 == null)
											{
												break;
											}
											if (((num7 < 0f) ? sideBcc2.shifted_right_y : sideBcc2.shifted_left_y) < this.RcWalk0.y - bccline3.BCC.base_shift_y)
											{
												break;
											}
											if (m == ((num7 < 0f) ? num14 : num15))
											{
												M2BlockColliderContainer.BCCLine sideBcc3 = pointPuts2.getSideBcc(base.Mp, m, num16, aim);
												if (sideBcc3 != null && !((num7 < 0f) ? (sideBcc3.shifted_right < regInfo2.posx) : (regInfo2.posx < sideBcc3.shifted_x)))
												{
													break;
												}
											}
										}
										drect.Set(num13 - base.sizex, (float)num16, base.sizex * 2f, base.sizey * 2f);
									}
									float cx = drect.cx;
									float num17 = X.Abs(cx - base.x);
									if (num6 < 0f || X.Abs(num6 - base.x) > num17)
									{
										num6 = cx;
										this.RectDepert = drect;
									}
								}
								IL_09C4:;
							}
							if (this.RectDepert == null)
							{
								if (walk_st == -3)
								{
									return true;
								}
								this.ADep.RemoveAt(0);
								if (this.ADep.Count == 0)
								{
									return this.runWalkError(Tk);
								}
								if (num4 > 5)
								{
									return true;
								}
							}
							else
							{
								Tk.Progress(ref t, 0, true);
								walk_st = 0;
								this.En.AimToLr((this.RectDepert.cx > base.x) ? 2 : 0);
								this.Prog(NASJumper.JPROG.SEARCH_DECIDE);
								this.dep_decided = true;
								if (Tk.type != NAI.TYPE.WALK_TO_WEED)
								{
									Tk.depx = this.ADep[0].posx;
									Tk.depy = this.ADep[0].y - 0.5f;
									break;
								}
								break;
							}
						}
					}
				}
			}
			if (Tk.prog == PROG.PROG0)
			{
				if (this.ADep.Count == 0 || this.RectDepert == null)
				{
					return false;
				}
				if (walk_time < 0f)
				{
					walk_time += base.TS;
				}
				else
				{
					bool footBCC2 = this.FootD.get_FootBCC() != null;
					float mpf_is_right = base.mpf_is_right;
					this.Phy.walk_xspeed = mpf_is_right * this.run_speed;
					this.Prog(NASJumper.JPROG.RUN0);
					if (footBCC2)
					{
						this.executeJumpInit(Tk, ref t, ref walk_st, ref walk_time, t >= 200f || this.En.wallHittedVX(mpf_is_right), base.Nai.RANtk(493) < 0.5f && X.LENGTHXYS(base.Nai.target_x, base.Nai.target_y, base.x + (base.sizex + 2.5f) * mpf_is_right, base.y) <= 1.5f);
					}
				}
			}
			if (Tk.prog == PROG.PROG1)
			{
				if (this.ADep.Count == 0)
				{
					return false;
				}
				walk_time += base.TS;
				if (!this.executeJumpFlying(Tk, ref t, ref walk_st, ref walk_time))
				{
					return false;
				}
			}
			if (Tk.prog == PROG.PROG2)
			{
				if (this.ADep.Count == 0)
				{
					return false;
				}
				if (t <= 30f)
				{
					return true;
				}
				this.Prog(NASJumper.JPROG.RUN1);
				NASJumper.RegInfo regInfo3 = this.ADep[0];
				float num18 = X.MMX(regInfo3.x + base.sizex, regInfo3.posx, regInfo3.r - base.sizex);
				if (base.hasFoot() && (X.Abs(num18 - base.x) < 1.2f || t >= 120f || this.En.wallHittedVX(this.Phy.walk_xspeed)))
				{
					this.dep_decided = false;
					Tk.AfterDelay(this.land_afterdelay);
					this.Phy.walk_xspeed = 0f;
					this.Prog(NASJumper.JPROG.FINISH);
					return false;
				}
				this.En.AimToLr((num18 > base.x) ? 2 : 0);
				float mpf_is_right2 = base.mpf_is_right;
				if (base.Nai.RANtk(371) < 0.8f && X.LENGTHXYS(base.Nai.target_x, base.Nai.target_y, base.x + (base.sizex + 2.5f) * mpf_is_right2, base.y) <= 1.5f)
				{
					this.En.jumpInit(X.absmax(num18 - base.x, 4.5f), 0f, X.NIXP(4f, 6f), false);
					this.Prog(NASJumper.JPROG.JUMP_FINISH);
					this.Phy.walk_xspeed = 0f;
					this.dep_decided = false;
					return false;
				}
				this.Phy.walk_xspeed = mpf_is_right2 * this.run_speed;
			}
			return true;
		}

		protected void calcPointAddition(NaTicket Tk, NASJumper.RegInfo _Dep)
		{
			_Dep.point_addition += ((base.Mp.canThroughBcc(_Dep.posx, _Dep.y - base.sizey * 2.2f, base.Nai.target_x, base.Nai.target_y, 2.66f, 0.5f, -1, false, false, null, true, null) == (Tk.type == NAI.TYPE.WALK)) ? 0 : 3000);
		}

		protected void executeJumpInit(NaTicket Tk, ref float t, ref int walk_st, ref float walk_time, bool timeouted, bool near_target)
		{
			float mpf_is_right = base.mpf_is_right;
			NASJumper.RegInfo regInfo = this.ADep[0];
			if (timeouted || near_target || ((mpf_is_right > 0f) ? (base.x > this.RectDepert.cx) : (base.x < this.RectDepert.cx)))
			{
				walk_st = 0;
				Vector4 jumpVelocity = M2Mover.getJumpVelocity(base.Mp, X.MMX2(regInfo.x + base.sizex, regInfo.posx, regInfo.r - base.sizex) - base.x, regInfo.y - base.mbottom, base.mbottom - X.Mn(regInfo.y - 1.25f - base.sizey, X.Mn(this.RectDepert.cy - base.sizey, this.RectDepert.y - 0.125f)), base.base_gravity, this.Phy.gravity_apply_velocity(1f));
				int num = X.IntR(jumpVelocity.w);
				float num2 = (float)num * jumpVelocity.x;
				if (!regInfo.getBcc().is_lift && ((jumpVelocity.x < 0f) ? (base.x + num2 < regInfo.r + base.sizex - 0.8f) : (regInfo.x - base.sizex + 0.8f < base.x + num2)))
				{
					walk_st = 1;
					walk_time = (float)(-(float)num);
				}
				else
				{
					int num3 = X.IntR(jumpVelocity.z);
					this.Phy.addFoc(FOCTYPE.WALK, jumpVelocity.x, 0f, -1f, 0, num3, num3, 1, 0);
				}
				this.FootD.initJump(false, false, false);
				this.Phy.addLockGravityFrame(num);
				this.Phy.addFoc(FOCTYPE.JUMP, 0f, jumpVelocity.y, -1f, 0, 1, num, -1, 0);
				this.Prog(NASJumper.JPROG.JUMP_INIT);
				this.En.setSkipLift((int)(regInfo.y - 1.25f), true);
				Tk.Progress(true);
				t = 0f;
				this.Phy.walk_xspeed = 0f;
			}
		}

		protected bool executeJumpFlying(NaTicket Tk, ref float t, ref int walk_st, ref float walk_time)
		{
			M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
			if (walk_time >= 0f && walk_st == 1 && footBCC == null)
			{
				float num = this.ADep[0].posx - base.x;
				walk_st = 0;
				if (X.Abs(num) > 0.2f)
				{
					this.Phy.walk_xspeed = (float)X.MPF(num > 0f) * 0.12f;
				}
				else
				{
					this.Phy.walk_xspeed = 0f;
				}
			}
			else
			{
				this.Phy.walk_xspeed = 0f;
			}
			this.Prog(NASJumper.JPROG.JUMPING);
			if (footBCC != null && Tk.Progress(ref t, 25, true))
			{
				this.Prog(NASJumper.JPROG.LANDED);
				walk_st = 0;
				this.Phy.walk_xspeed = 0f;
				if (this.ADep.Count == 0 || footBCC != this.ADep[0].getBcc())
				{
					Tk.AfterDelay(this.land_afterdelay);
					this.dep_decided = false;
					return false;
				}
			}
			return true;
		}

		public void setErrorDepert()
		{
			if (this.ADep.Count > 0 && this.dep_decided)
			{
				M2BlockColliderContainer.BCCLine bcc = this.ADep[0].getBcc();
				base.Nai.addDeclineArea(bcc.shifted_x, bcc.shifted_y - 1f, bcc.w, bcc.h + 2f, 160f);
			}
		}

		protected bool runWalkError(NaTicket Tk)
		{
			if (Tk != null)
			{
				base.Nai.addDeclineArea(Tk.depx + 0.25f, Tk.depy + 0.25f, 0.5f, 0.5f, 100f);
				if (Tk.type == NAI.TYPE.WALK_TO_WEED)
				{
					Tk.Recreate(NAI.TYPE.WALK, -1, true, null);
					return true;
				}
			}
			this.Prog(NASJumper.JPROG.ERROR);
			return false;
		}

		public void jumpEffectBasic()
		{
			this.En.PtcVar("by", (double)(base.y + base.sizey * 0.75f)).PtcST("fox_jump_init", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
		}

		public float suitable_len = 8f;

		public float run_speed = 0.15f;

		public float jumpable_x_len = 6f;

		public float land_afterdelay = 25f;

		public NASJumper.FnListenJumpProgress fnJumpProgress;

		private List<NASJumper.RegInfo> ADep;

		private List<NASJumper.RegInfo> ADepSrc;

		private Comparison<NASJumper.RegInfo> FD_fnSortDeperture;

		private Rect RcAirCovering;

		private DRect RcWalk0;

		private DRect[] ARcDepertBuf;

		private Vector4 VcFly;

		private List<M2BlockColliderContainer.BCCLine> ABcc0;

		private bool dep_decided;

		private DRect RectDepert;

		public delegate bool FnListenJumpProgress(NASJumper Jp, NASJumper.JPROG jprog);

		public enum JPROG
		{
			INIT,
			SEARCHING,
			SEARCH_DECIDE,
			RUN0,
			JUMP_INIT,
			JUMPING,
			LANDED,
			RUN1,
			JUMP_FINISH,
			FINISH,
			ERROR
		}

		protected class RegInfo
		{
			public RegInfo(M2BlockColliderContainer.BCCLine _Reg, float _posx = 0f)
			{
				this.Reg = _Reg;
				this.posx = _posx;
			}

			public bool hasSafeArea()
			{
				if (this.dangerleft == -1)
				{
					if (this.Reg.AMapDmg == null)
					{
						this.dangerleft = -2;
					}
					else
					{
						this.dangerleft = 0;
						for (int i = this.Reg.AMapDmg.Count - 1; i >= 0; i--)
						{
							M2MapDamageContainer.M2MapDamageItem m2MapDamageItem = this.Reg.AMapDmg[i];
							this.dangerleft = (int)X.Mn((float)this.dangerleft, m2MapDamageItem.x);
							this.dangerright = (int)X.Mx((float)this.dangerright, m2MapDamageItem.right);
						}
					}
				}
				return (int)this.Reg.left < this.dangerleft || this.dangerright < (int)this.Reg.right;
			}

			public bool decidePosition(float sizex, float mvx, float mvbtm, float target_x, M2BlockColliderContainer.BCCLine B0, DRect RcWalk0)
			{
				float num;
				float num2;
				this.Reg.BCC.getBaseShift(out num, out num2);
				this.posx = X.MMX(this.Reg.x + sizex + 0.125f, target_x, this.Reg.right - sizex - 0.125f) - num;
				if (this.Reg.AMapDmg != null)
				{
					float num3 = -100f;
					float num4 = -1f;
					bool flag = this.Reg.foot_aim == AIM.B;
					for (int i = 0; i < 2; i++)
					{
						float num5 = this.posx + num;
						int count = this.Reg.AMapDmg.Count;
						for (int j = 0; j < count; j++)
						{
							M2MapDamageContainer.M2MapDamageItem m2MapDamageItem = this.Reg.AMapDmg[(i == 0 == flag) ? (count - 1 - j) : j];
							if (X.BTW(m2MapDamageItem.x, num5, m2MapDamageItem.right))
							{
								if (i == 0)
								{
									num5 = m2MapDamageItem.x - 0.25f - sizex;
									if (num5 < this.Reg.x)
									{
										num5 = -100f;
										break;
									}
								}
								else
								{
									num5 = m2MapDamageItem.right + 0.25f - sizex;
									if (num5 >= this.Reg.right)
									{
										num5 = -100f;
										break;
									}
								}
							}
						}
						if (num5 != -100f)
						{
							num5 -= num;
							float num6 = X.Abs(mvx - num5);
							if (num4 < 0f || num4 > num6)
							{
								num4 = num5;
								num3 = num5;
							}
						}
					}
					if (!X.BTW(this.Reg.x + sizex, num3 + num, this.Reg.right - sizex))
					{
						this.posx = -1f;
						return false;
					}
					this.posx = num3;
				}
				if (B0 != this.Reg)
				{
					float y = this.Reg.linePosition(this.posx, mvbtm, 0f, 0f).y;
					float base_shift_x = B0.BCC.base_shift_x;
					float num7 = RcWalk0.x - base_shift_x;
					float num8 = RcWalk0.right - base_shift_x;
					if (mvbtm < y)
					{
						num7 -= sizex;
						num8 += sizex;
						if (X.isContaining(num7, num8, this.Reg.x - num, this.Reg.right - num, 0f))
						{
							return false;
						}
						if (X.BTW(num7, this.posx, num8))
						{
							float num9 = -1f;
							float num10 = this.posx;
							for (int k = 0; k < 2; k++)
							{
								float num11 = ((k == 0 == this.posx < mvx) ? num8 : num7);
								if (X.BTW(this.Reg.x + sizex, num11 + num, this.Reg.right - sizex))
								{
									float num12 = X.Abs(num11 - this.posx);
									if (num9 < 0f || num12 < num9)
									{
										num9 = num12;
										num10 = num11;
									}
								}
							}
							if (num9 < 0f)
							{
								return false;
							}
							this.posx = num10;
						}
					}
					else if (X.isContaining(this.Reg.x - num - sizex, this.Reg.right - num + sizex, num7, num8, 0f))
					{
						return false;
					}
				}
				return true;
			}

			public float x
			{
				get
				{
					return this.Reg.shifted_x;
				}
			}

			public float y
			{
				get
				{
					return this.Reg.shifted_y;
				}
			}

			public float w
			{
				get
				{
					return this.Reg.w;
				}
			}

			public float h
			{
				get
				{
					return this.Reg.h;
				}
			}

			public float r
			{
				get
				{
					return this.Reg.shifted_right;
				}
			}

			public float cx
			{
				get
				{
					return this.Reg.shifted_cx;
				}
			}

			public float cy
			{
				get
				{
					return this.Reg.shifted_cy;
				}
			}

			public bool has_danger
			{
				get
				{
					return this.Reg.has_danger_chip;
				}
			}

			public M2BlockColliderContainer.BCCLine getBcc()
			{
				return this.Reg;
			}

			protected readonly M2BlockColliderContainer.BCCLine Reg;

			public int point_addition;

			public float lgt;

			public float posx;

			public int dangerleft = -1;

			public int dangerright;
		}
	}
}
