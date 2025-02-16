using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NASSmallJumper : NelEnemyAssist
	{
		public NASSmallJumper(NelEnemy _En, float _run_speed, float _jump_height_max)
			: base(_En)
		{
			this.run_speed = _run_speed;
			this.jump_height_max = _jump_height_max;
		}

		public bool runWalk(bool init_flag, NaTicket Tk, ref float t)
		{
			int num = this.En.walkThroughLift(init_flag, Tk, 0);
			if (num >= 0)
			{
				return num == 0;
			}
			return this.runWalkMain(init_flag, Tk, ref t);
		}

		public void quitTicket(NaTicket Tk)
		{
			this.En.Anm.timescale = 1f;
			this.walk_st = -1;
		}

		private void initTicket(NaTicket Tk)
		{
			Tk.check_nearplace_error = 0;
			this.JumpTargetTo = null;
			this.walk_st = 0;
			this.force_jump = false;
		}

		private bool runWalkMain(bool init_flag, NaTicket Tk, ref float t)
		{
			if (init_flag)
			{
				if (!this.force_jump)
				{
					this.initTicket(Tk);
					if (Tk.DepBCC == null && Tk.depx < 0f)
					{
						return false;
					}
					t = -1f;
					if (this.FD_InitWalkMain != null && !this.FD_InitWalkMain(Tk))
					{
						return false;
					}
				}
				this.force_jump = false;
				t = 0f;
			}
			M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
			int num = this.walk_st % 10;
			Vector4 zero = Vector4.zero;
			if (num == 0)
			{
				if (this.FD_RunPrepare != null && !this.FD_RunPrepare(Tk))
				{
					this.walk_st = this.walk_st / 10 * 10 + 1;
					t = 0f;
					this.JumpTargetTo = null;
					this.t_error = 0f;
				}
			}
			else if (num == 1)
			{
				if (footBCC == null)
				{
					return this.fallInit(Tk, zero, 0f);
				}
				float num2 = this.jump_clip_margin;
				M2BlockColliderContainer.BCCLine bccline = ((base.aim == AIM.L) ? footBCC.SideL : footBCC.SideR);
				bool flag = false;
				bool flag2 = false;
				this.t_bcc_highjump_check += base.TS;
				if (this.t_bcc_highjump_check >= 4f)
				{
					this.t_bcc_highjump_check -= 4f;
					if (this.highJumpCheckInRunning(Tk, ref t))
					{
						return true;
					}
				}
				if (this.En.wallHittedA())
				{
					this.t_error += base.TS;
					if (this.t_error >= 14f)
					{
						Tk.error();
						return false;
					}
				}
				else if (this.t_error > 0f)
				{
					this.t_error = X.Mx(this.t_error - base.TS, 0f);
				}
				if (bccline != null)
				{
					flag = ((base.aim == AIM.L) ? footBCC.L_is_90 : footBCC.R_is_90) && bccline.foot_aim != footBCC.foot_aim;
					flag2 = this.jump_270 && ((base.aim == AIM.L) ? footBCC.L_is_270 : footBCC.R_is_270) && bccline.foot_aim != footBCC.foot_aim;
					if (flag)
					{
						num2 += bccline.height * this.jump_clip_margin_height_mul;
					}
					else if (flag2)
					{
						num2 -= base.sizex * 0.66f;
					}
				}
				if (((base.aim == AIM.L) ? (base.mleft < footBCC.shifted_x + num2) : (base.mright > footBCC.shifted_right - num2)) && Tk.DepBCC != null)
				{
					if (Tk.DepBCC == footBCC)
					{
						return false;
					}
					if (bccline == null || flag)
					{
						this.walk_st = this.walk_st / 10 * 10 + 2;
						t = 0f;
						return true;
					}
					if (bccline == null || flag2)
					{
						if (!this.highJumpCheckInRunning(Tk, ref t))
						{
							this.walk_st = this.walk_st / 10 * 10 + 3;
							t = 0f;
						}
						return true;
					}
				}
				if (X.Abs(Tk.depx - base.x) < 0.125f)
				{
					return false;
				}
				float num3 = X.ZLINE(t, 25f);
				this.SpSetPose(this.walk_pose);
				this.Anm.timescale = X.NI(0.125f, 1f, num3);
				this.En.setWalkXSpeed(num3 * (this.run_speed * ((this.run_speed_reach_time <= 0f) ? 1f : X.ZPOW(t, this.run_speed_reach_time))) * base.mpf_is_right, true, false);
				if (this.FD_RunWalking != null && !this.FD_RunWalking(Tk))
				{
					return false;
				}
			}
			else if (num == 2)
			{
				if (footBCC == null)
				{
					return this.fallInit(Tk, zero, 0f);
				}
				this.En.Anm.timescale = 1f;
				if (this.FD_JumpPrepare == null || !this.FD_JumpPrepare(Tk))
				{
					this.JumpTargetTo = null;
					M2BlockColliderContainer.BCCLine bccline2 = ((base.aim == AIM.L) ? footBCC.SideL : footBCC.SideR);
					float num4 = Tk.depx;
					float num5 = 0f;
					if (bccline2 != null && ((base.aim == AIM.L) ? footBCC.L_is_90 : footBCC.R_is_90))
					{
						M2BlockColliderContainer.BCCLine bccline3 = ((base.aim == AIM.L) ? bccline2.LinkS : bccline2.LinkD);
						if (bccline3 != null && bccline3.foot_aim == footBCC.foot_aim)
						{
							this.JumpTargetTo = bccline3;
							num4 = X.MMX2(this.JumpTargetTo.shifted_x + 0.25f, base.x, this.JumpTargetTo.shifted_right - 0.25f);
							num5 = this.JumpTargetTo.slopeBottomY(num4);
						}
					}
					if (num5 == 0f)
					{
						M2BlockColliderContainer.BCCLine bccline4 = (this.JumpTargetTo = base.Mp.getSideBcc((int)num4, (int)Tk.depy, AIM.B));
						if (this.JumpTargetTo != null)
						{
							this.JumpTargetTo = bccline4;
							num4 = X.MMX2(this.JumpTargetTo.shifted_x + 0.25f, base.x, this.JumpTargetTo.shifted_right - 0.25f);
							num5 = this.JumpTargetTo.slopeBottomY(num4);
						}
						else
						{
							num5 = Tk.depy;
						}
					}
					float num6 = num5 - base.mbottom;
					float num7 = X.MMX(this.jump_height_min, X.Abs(num5 - base.mbottom) + this.jump_height_margin, this.jump_height_max);
					float num8 = this.Phy.gravity_apply_velocity(1f);
					Vector4 jumpVelocity = M2Mover.getJumpVelocity(base.Mp, X.absMMX(1f, num4 - base.x, this.jumpable_x_range_max), num6, num7, base.base_gravity, num8);
					return this.fallInit(Tk, jumpVelocity, num6);
				}
			}
			else if (num == 3)
			{
				if (footBCC == null)
				{
					return this.fallInit(Tk, zero, 0f);
				}
				this.En.Anm.timescale = 1f;
				if (this.FD_JumpPrepare == null || !this.FD_JumpPrepare(Tk))
				{
					M2BlockColliderContainer.BCCLine bccline5 = ((base.aim == AIM.L) ? footBCC.SideL : footBCC.SideR);
					float num9 = Tk.depx;
					float num10 = 0f;
					if (this.JumpTargetTo == null)
					{
						if (bccline5 != null && ((base.aim == AIM.L) ? footBCC.L_is_270 : footBCC.R_is_270))
						{
							M2BlockColliderContainer.BCCLine bccline6 = ((base.aim == AIM.L) ? bccline5.LinkS : bccline5.LinkD);
							if (bccline6 != null && bccline6.foot_aim == footBCC.foot_aim)
							{
								this.JumpTargetTo = bccline6;
							}
						}
						if (num10 == 0f)
						{
							M2BlockColliderContainer.BCCLine bccline7 = (this.JumpTargetTo = base.Mp.getSideBcc((int)num9, (int)Tk.depy, AIM.B));
							if (this.JumpTargetTo != null)
							{
								this.JumpTargetTo = bccline7;
							}
							else
							{
								num10 = Tk.depy;
							}
						}
						if (this.JumpTargetTo != null)
						{
							float num11 = ((this.JumpTargetTo == footBCC) ? Tk.depx : base.x);
							num9 = ((base.aim == AIM.L) ? X.Mn(num11, this.JumpTargetTo.shifted_right - 0.25f - base.sizex) : X.Mx(num11, this.JumpTargetTo.shifted_x + 0.25f + base.sizex));
							num10 = this.JumpTargetTo.slopeBottomY(num9);
						}
					}
					else if (this.JumpTargetTo != null)
					{
						float num12 = ((this.JumpTargetTo == footBCC) ? Tk.depx : base.x);
						float num13 = (((base.aim == AIM.L) ? this.JumpTargetTo.R_is_90 : this.JumpTargetTo.L_is_90) ? base.sizex : 0f) + 0.25f;
						num9 = ((base.aim == AIM.L) ? X.Mn(num12, this.JumpTargetTo.shifted_right - num13) : X.Mx(num12, this.JumpTargetTo.shifted_x + num13));
						num10 = this.JumpTargetTo.slopeBottomY(num9);
					}
					float num14 = X.absMMX(1f, num9 - base.x, this.jumpable_x_range_max);
					float num15 = num10 - base.mbottom;
					float num16 = X.Mx(X.Mx(X.Mx(this.jump_height_min, this.jump_height_margin), -num15 + 0.8f), X.NIL(this.jump_height_min, this.jump_height_max, X.Abs(num14) * 0.5f, this.jumpable_x_range_max));
					float num17 = this.Phy.gravity_apply_velocity(1f);
					Vector4 jumpVelocity2 = M2Mover.getJumpVelocity(base.Mp, num14, num15, num16, base.base_gravity, num17);
					return this.fallInit(Tk, jumpVelocity2, num15);
				}
			}
			else if (num == 4)
			{
				this.Anm.timescale = 1f;
				base.Nai.delay += base.TS;
				if ((this.FD_RunJumping == null) ? (footBCC != null) : (!this.FD_RunJumping(Tk)))
				{
					t = 0f;
					this.walk_st = this.walk_st / 10 * 10 + ((this.FD_Land == null) ? 6 : 5);
					this.SpSetPose(this.land_pose);
				}
			}
			else if (num == 5)
			{
				if (this.FD_Land == null || !this.FD_Land(Tk))
				{
					this.walk_st = this.walk_st / 10 * 10 + 6;
					t = 0f;
				}
			}
			else if (num == 6)
			{
				this.walk_st = this.walk_st - num + 10;
				if (this.walk_st >= 10 * this.MAX_JUMP_COUNT_ON_ONETICKET)
				{
					return false;
				}
				if (t >= this.land_delay)
				{
					t = 0f;
					this.En.setAim((base.x < Tk.depx) ? AIM.R : AIM.L, false);
					if (this.FD_InitWalkMain != null && !this.FD_InitWalkMain(Tk))
					{
						return false;
					}
				}
			}
			return true;
		}

		public void forceJumpPrepareInit(NaTicket Tk, M2BlockColliderContainer.BCCLine TargetBcc, ref float t)
		{
			if (this.walk_st < 0)
			{
				this.initTicket(Tk);
			}
			this.JumpTargetTo = TargetBcc;
			this.walk_st = this.walk_st / 10 * 10 + 3;
			t = 0f;
			this.force_jump = true;
		}

		private bool highJumpCheckInRunning(NaTicket Tk, ref float t)
		{
			if (base.Nai == null)
			{
				return false;
			}
			M2BlockColliderContainer.BCCLine targetLastBcc = base.Nai.TargetLastBcc;
			M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
			float num = this.jumpable_x_range_max;
			float num2 = ((base.mpf_is_right > 0f) ? base.mleft : base.mleft);
			if (targetLastBcc == null || footBCC == null || footBCC == targetLastBcc || (footBCC.isLinearWalkableTo(targetLastBcc, 5) & ((base.mpf_is_right <= 0f || 2 != 0) ? 1 : 0)) != 0)
			{
				return false;
			}
			for (int i = 0; i < 2; i++)
			{
				M2BlockColliderContainer.BCCLine bccline = targetLastBcc;
				for (int j = 0; j < 5; j++)
				{
					if (bccline == null || footBCC == null)
					{
						return false;
					}
					if (bccline.foot_aim == footBCC.foot_aim)
					{
						if (j > 0 && bccline == targetLastBcc)
						{
							break;
						}
						if (X.BTW(bccline.shifted_x - num, num2, bccline.shifted_right + num))
						{
							float num3 = ((base.mpf_is_right > 0f) ? bccline.shifted_left_y : bccline.shifted_right_y);
							if (num3 >= base.mbottom - this.jump_height_max)
							{
								float num4 = num3 - base.sizey - 0.125f;
								float num5 = ((base.mpf_is_right > 0f) ? bccline.shifted_x : bccline.shifted_right);
								if (num3 >= base.mbottom - 0.125f || base.Mp.canThroughRectAgR(base.mleft, base.mtop - 0.125f, base.sizex * 2f, base.sizey * 2f, X.LENGTHXYQ(base.x, base.y - 0.125f, num5, num4), base.Mp.GAR(base.x, base.y - 0.125f, num5, num4)))
								{
									this.JumpTargetTo = bccline;
									this.walk_st = this.walk_st / 10 * 10 + 3;
									t = 0f;
									return true;
								}
							}
						}
					}
					bccline = ((i == 1) ? targetLastBcc.LinkD : targetLastBcc.LinkS);
				}
			}
			return false;
		}

		private bool fallInit(NaTicket Tk, Vector4 V, float ypos = -1000f)
		{
			if (this.FD_JumpInit != null && !this.FD_JumpInit(Tk, ref V))
			{
				return false;
			}
			if (V.z > 0f)
			{
				this.En.jumpInit(V, ypos, false);
				this.SpSetPose(this.jump_pose);
				this.walk_st = this.walk_st / 10 * 10 + 4;
				base.Nai.delay += 20f;
			}
			this.walk_st = this.walk_st / 10 * 10 + 4;
			return true;
		}

		public void SpSetPose(string pose)
		{
			if (TX.valid(pose))
			{
				this.En.SpSetPose(pose, -1, null, false);
			}
		}

		public float run_speed;

		public float run_speed_reach_time;

		public float jumpable_x_range_max = 6f;

		public float jump_height_min = 2.5f;

		public float jump_height_max = 7f;

		public float jump_height_margin = 2.5f;

		public float jump_clip_margin = 0.25f;

		public float jump_clip_margin_height_mul;

		public float land_delay = 24f;

		public bool jump_270;

		public int MAX_JUMP_COUNT_ON_ONETICKET = 3;

		private int walk_st;

		private float t_error;

		public NAI.FnTicketRun FD_InitWalkMain;

		public NAI.FnTicketRun FD_RunPrepare;

		public NAI.FnTicketRun FD_JumpPrepare;

		public NASSmallJumper.FnJumpInit FD_JumpInit;

		public NAI.FnTicketRun FD_RunJumping;

		public NAI.FnTicketRun FD_Land;

		public NAI.FnTicketRun FD_RunWalking;

		public string walk_pose = "walk";

		public string land_pose = "land";

		public string jump_pose = "jump";

		private const int WID_RUN_PREPARE = 0;

		private const int WID_RUNNING = 1;

		private const int WID_JUMP_PREPARE = 2;

		private const int WID_JUMP_PREPARE_270 = 3;

		private const int WID_JUMP = 4;

		private const int WID_LAND = 5;

		private const int WID_LAND_DELAY = 6;

		private float t_bcc_highjump_check;

		private bool force_jump;

		public M2BlockColliderContainer.BCCLine JumpTargetTo;

		public delegate bool FnJumpInit(NaTicket Tk, ref Vector4 VJump);
	}
}
