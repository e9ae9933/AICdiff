using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NASGroundClimber : NelEnemyAssist
	{
		public NASGroundClimber(NelEnemy En, float _rotate_x_shift_pixel = 4f)
			: base(En)
		{
			this.rotate_x_shift_pixel = _rotate_x_shift_pixel;
			this.t_hitwall = new RevCounter();
			this.t_stopping = new RevCounter();
			this.t_leave = new RevCounter();
			this.t_stop_hitwall_check = new RevCounter();
		}

		public int FixAim(bool set_aim = true, float correct_ratio = 1f)
		{
			return this.FixAim(base.target_x, base.targetBCC, set_aim, correct_ratio);
		}

		public int FixAim(float target_x, M2BlockColliderContainer.BCCLine tBCC, bool set_aim = true, float correct_ratio = 1f)
		{
			M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
			if (footBCC == null || tBCC == null)
			{
				return -1;
			}
			int num = -2;
			if (footBCC == tBCC)
			{
				bool flag = X.XORSP() < correct_ratio;
				switch (footBCC.foot_aim)
				{
				case AIM.L:
					num = ((base.y > base.target_y == flag) ? 0 : 2);
					break;
				case AIM.T:
					num = ((base.x > target_x == flag) ? 2 : 0);
					break;
				case AIM.R:
					num = ((base.y > base.target_y == flag) ? 2 : 0);
					break;
				case AIM.B:
					num = ((base.x > target_x == flag) ? 0 : 2);
					break;
				}
			}
			else
			{
				int num2 = 9999;
				int num3 = 9999;
				if (footBCC.BCC == tBCC.BCC && footBCC.is_lift == tBCC.is_lift && footBCC.block_index == tBCC.block_index)
				{
					M2BlockColliderContainer.BCCLine bccline = footBCC;
					M2BlockColliderContainer.BCCLine bccline2 = footBCC;
					bool flag2 = false;
					int num4 = 0;
					while (++num4 < 30 && !flag2)
					{
						for (int i = 0; i < 2; i++)
						{
							M2BlockColliderContainer.BCCLine bccline3 = ((i == 0) ? bccline : bccline2);
							if (bccline3 != null)
							{
								if (bccline3 == tBCC)
								{
									if (i == 0)
									{
										num2 = num4;
									}
									else
									{
										num3 = num4;
									}
									flag2 = true;
								}
								else
								{
									if (i == 0)
									{
										bccline = (bccline3 = bccline3.LinkS);
									}
									else
									{
										bccline2 = (bccline3 = bccline3.LinkD);
									}
									if (footBCC == bccline3)
									{
										flag2 = true;
										break;
									}
								}
							}
						}
					}
				}
				if (num2 == num3)
				{
					num = -1;
				}
				else
				{
					num = ((num2 < num3 == X.XORSP() < correct_ratio) ? 0 : 2);
				}
			}
			if (set_aim && num >= -1)
			{
				if (num >= -1)
				{
					this.En.setAim((X.xors(2) == 0) ? AIM.L : AIM.R, false);
				}
				else
				{
					this.En.setAim((AIM)num, false);
				}
			}
			return num;
		}

		public int setAimConsideringWall(int a, bool execute = true)
		{
			if (a < 0)
			{
				return a;
			}
			M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
			if (footBCC != null)
			{
				switch (footBCC.foot_aim)
				{
				case AIM.L:
					a = (int)CAim.get_clockwise2((AIM)a, true);
					break;
				case AIM.T:
					a = (int)CAim.get_opposite((AIM)a);
					break;
				case AIM.R:
					a = (int)CAim.get_clockwise2((AIM)a, false);
					break;
				}
			}
			if (execute)
			{
				this.En.setAim((AIM)a, false);
			}
			return a;
		}

		public bool initClimbWalk(uint footable_bits = 15U)
		{
			M2BlockColliderContainer.BCCLine preBCC = this.PreBCC;
			M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
			if (footBCC == null)
			{
				return false;
			}
			this.pre_corner_slip_bits = this.Phy.corner_slip_alloc_bits;
			this.PreBCC = footBCC;
			this.aim_progress = this.En.aim;
			this.FootD.footable_bits = 15U;
			this.t_leave.Set(0f, false);
			this.t_hitwall.Set(0f, false);
			this.t_stop_hitwall_check.Set(0f, false);
			this.t_stopping.Set(0f, false);
			return true;
		}

		public void quitClimbWalk()
		{
			if (this.pre_corner_slip_bits != 32U)
			{
				this.Phy.corner_slip_alloc_bits = this.pre_corner_slip_bits;
			}
			this.pre_corner_slip_bits = 32U;
			this.FootD.footable_bits = 8U;
			this.PreBCC = null;
		}

		public bool quited
		{
			get
			{
				return this.PreBCC == null;
			}
		}

		public NASGroundClimber Turn()
		{
			this.aim_progress = CAim.get_opposite(this.aim_progress);
			this.En.setAim(CAim.get_opposite(this.En.aim), false);
			return this;
		}

		public bool progressClimbWalk(float speed)
		{
			if (this.PreBCC == null)
			{
				return false;
			}
			M2BlockColliderContainer.BCCLine bccline = this.FootD.get_FootBCC();
			if (bccline == null)
			{
				if (!this.alloc_jump_air && (((this.PreBCC.aim == AIM.B || this.PreBCC.aim == AIM.R) == (this.aim_progress == AIM.R)) ? this.PreBCC.LinkS : this.PreBCC.LinkD) == null && this.t_leave.Equals(0))
				{
					this.Turn();
				}
				this.t_leave.Add(base.TS, false, false);
				if (this.t_leave >= (float)this.leave_allocate_time)
				{
					return false;
				}
				bccline = this.PreBCC;
			}
			else
			{
				this.Phy.addLockGravityFrame(3);
			}
			if (this.PreBCC != bccline)
			{
				if (!bccline.isUseableDir(this.footable_bits))
				{
					return false;
				}
				if (!this.triggerEvent(this.Achanged, bccline))
				{
					if (this.PreBCC == null)
					{
						return false;
					}
					if (this.FootD.get_FootBCC() == null)
					{
						return true;
					}
					bccline = this.PreBCC;
					float num;
					float num2;
					this.PreBCC.fixToFootPos(this.FootD, base.x, base.y, out num, out num2);
					float num3 = X.Abs(speed) + 0.5f;
					this.En.moveBy(X.absMn(num, num3), X.absMn(num2, num3), true);
					this.FootD.rideInitTo(bccline, false);
				}
				else
				{
					this.PreBCC = bccline;
				}
			}
			Vector3 velocityDir = this.getVelocityDir();
			this.Anm.rotationR = X.VALWALKANGLER(this.Anm.rotationR, this.PreBCC.housenagR - 1.5707964f, 0.25132743f);
			if (this.rotate_x_shift_pixel != 0f)
			{
				this.Anm.after_offset_x = X.VALWALK(this.Anm.after_offset_x, (velocityDir.y != 0f) ? ((float)CAim._XD(this.PreBCC.aim, 1) * this.rotate_x_shift_pixel * this.Anm.scaleY) : 0f, 0.4f);
			}
			if (this.En.wallHitted(CAim.get_aim2(0f, 0f, velocityDir.x, -velocityDir.y, false)))
			{
				this.t_hitwall += base.TS * 1.5f;
			}
			if (X.LENGTHXYS(base.x, base.y, this.Phy.prex, this.Phy.prey) < speed * 0.3f)
			{
				this.t_stopping += base.TS;
				if (this.t_stop_hitwall_check.Get() == 0f && this.t_stopping >= 20f)
				{
					this.t_stop_hitwall_check.Set(140f, false);
					this.t_hitwall.Set(10f, false);
				}
			}
			if (this.t_hitwall >= 10f)
			{
				this.t_hitwall.Clear();
				float num4 = velocityDir.x * speed;
				M2BlockColliderContainer.BCCLine bccline2;
				float num5;
				if (num4 != 0f && this.Phy.checkSideLRBCC(ref num4, base.hasFoot(), out bccline2))
				{
					num5 = -speed;
					M2BlockColliderContainer.BCCLine bccline3 = null;
					M2BlockColliderContainer.BCCLine bccline4 = null;
					bool flag = this.PreBCC._yd > 0 || this.Phy.checkSideTBBCC(ref num5, base.hasFoot(), out bccline3);
					num5 = speed;
					bool flag2 = this.PreBCC._yd < 0 || this.Phy.checkSideTBBCC(ref num5, base.hasFoot(), out bccline4);
					if (flag == flag2 || bccline2 == null)
					{
						if (flag && this.PreBCC._yd < 0 && bccline3 != null)
						{
							this.FootD.rideInitTo(bccline3, false);
						}
						else if (flag2 && this.PreBCC._yd > 0 && bccline4 != null)
						{
							this.FootD.rideInitTo(bccline4, false);
						}
						else
						{
							this.Turn();
						}
					}
					else
					{
						if (!flag)
						{
							this.aim_progress = ((bccline2._xd < 0) ? AIM.L : AIM.R);
							this.FootD.rideInitTo(bccline2, false);
						}
						if (!flag2)
						{
							this.aim_progress = ((bccline2._xd > 0) ? AIM.L : AIM.R);
							this.FootD.rideInitTo(bccline2, false);
						}
					}
				}
				num5 = velocityDir.y * speed;
				if (num5 != 0f && this.Phy.checkSideTBBCC(ref num5, base.hasFoot(), out bccline2))
				{
					M2BlockColliderContainer.BCCLine bccline5 = null;
					M2BlockColliderContainer.BCCLine bccline6 = null;
					num4 = -speed;
					bool flag3 = this.PreBCC.aim == AIM.L || this.Phy.checkSideLRBCC(ref num4, base.hasFoot(), out bccline5);
					num4 = speed;
					bool flag4 = this.PreBCC.aim == AIM.R || this.Phy.checkSideLRBCC(ref num4, base.hasFoot(), out bccline6);
					if (flag3 == flag4 || bccline2 == null)
					{
						if (flag3 && this.PreBCC._xd > 0 && bccline5 != null)
						{
							this.FootD.rideInitTo(bccline5, false);
						}
						else if (flag4 && this.PreBCC._xd < 0 && bccline6 != null)
						{
							this.FootD.rideInitTo(bccline6, false);
						}
						else
						{
							this.Turn();
						}
					}
					else
					{
						if (!flag3)
						{
							this.aim_progress = ((-bccline2._yd > 0) ? AIM.L : AIM.R);
							this.FootD.rideInitTo(bccline2, false);
						}
						if (!flag4)
						{
							this.aim_progress = ((-bccline2._yd < 0) ? AIM.L : AIM.R);
							this.FootD.rideInitTo(bccline2, false);
						}
					}
				}
			}
			if (this.t_stopping > 45f)
			{
				this.Turn();
				this.t_stopping.Clear();
			}
			this.t_hitwall.Update(base.TS);
			this.t_stop_hitwall_check.Update(base.TS);
			this.t_stopping.Update(base.TS);
			this.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._CHECK_WALL | FOCTYPE._INDIVIDUAL, velocityDir.x * speed, velocityDir.y * speed, -1f, -1, 1, 0, -1, 0);
			if (velocityDir.z != 0f)
			{
				this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._CHECK_WALL | FOCTYPE._INDIVIDUAL, 0f, velocityDir.z * speed, -1f, -1, 1, 0, -1, 0);
			}
			if (this.FootD.get_FootBCC() == this.PreBCC)
			{
				bool flag5 = CAim._XD(this.aim_progress, 1) > 0;
				M2BlockColliderContainer.BCCLine bccline7 = (flag5 ? this.PreBCC.LinkD : this.PreBCC.LinkS);
				if (bccline7 != null && bccline7.isUseableDir(this.FootD))
				{
					bool flag6 = false;
					float num6 = (float)this.PreBCC._yd;
					if (flag5)
					{
						if (num6 > 0f)
						{
							if (bccline7.aim == AIM.L)
							{
								flag6 = this.Phy.tstacked_x - base.sizex + velocityDir.x * speed < bccline7.shifted_x;
							}
							else if (bccline7.aim == AIM.R)
							{
								flag6 = this.Phy.tstacked_x + base.sizex + velocityDir.x * speed < bccline7.shifted_x;
							}
						}
						else if (num6 < 0f)
						{
							if (bccline7.aim == AIM.R)
							{
								flag6 = this.Phy.tstacked_x + base.sizex + velocityDir.x * speed > bccline7.shifted_x;
							}
							else if (bccline7.aim == AIM.L)
							{
								flag6 = !this.alloc_jump_air && this.Phy.tstacked_x - base.sizex + velocityDir.x * speed > bccline7.shifted_x;
							}
						}
						else if (this.PreBCC.aim == AIM.L)
						{
							if (bccline7._yd < 0)
							{
								flag6 = this.Phy.tstacked_y + base.sizey + velocityDir.y * speed > this.PreBCC.shifted_bottom;
							}
							else if (bccline7._yd > 0)
							{
								flag6 = this.Phy.tstacked_y - base.sizey + velocityDir.y * speed > this.PreBCC.shifted_bottom;
							}
						}
						else if (this.PreBCC.aim == AIM.R)
						{
							if (bccline7._yd < 0)
							{
								flag6 = this.Phy.tstacked_y + base.sizey + velocityDir.y * speed < this.PreBCC.shifted_y;
							}
							else if (bccline7._yd > 0)
							{
								flag6 = this.Phy.tstacked_y - base.sizey + velocityDir.y * speed < this.PreBCC.shifted_y;
							}
						}
					}
					else if (num6 > 0f)
					{
						if (bccline7.aim == AIM.L)
						{
							flag6 = this.Phy.tstacked_x - base.sizex + velocityDir.x * speed > bccline7.shifted_x;
						}
						else if (bccline7.aim == AIM.R)
						{
							flag6 = this.Phy.tstacked_x + base.sizex + velocityDir.x * speed > bccline7.shifted_x;
						}
					}
					else if (num6 < 0f)
					{
						if (bccline7.aim == AIM.R)
						{
							flag6 = this.Phy.tstacked_x + base.sizex + velocityDir.x * speed < bccline7.shifted_x;
						}
						else if (bccline7.aim == AIM.L)
						{
							flag6 = !this.alloc_jump_air && this.Phy.tstacked_x - base.sizex + velocityDir.x * speed < bccline7.shifted_x;
						}
					}
					else if (this.PreBCC.aim == AIM.L)
					{
						if (bccline7._yd < 0)
						{
							flag6 = this.Phy.tstacked_y + base.sizey + velocityDir.y * speed < this.PreBCC.shifted_y;
						}
						else if (bccline7._yd > 0)
						{
							flag6 = this.Phy.tstacked_y - base.sizey + velocityDir.y * speed < this.PreBCC.shifted_y;
						}
					}
					else if (this.PreBCC.aim == AIM.R)
					{
						if (bccline7._yd < 0)
						{
							flag6 = this.Phy.tstacked_y + base.sizey + velocityDir.y * speed > this.PreBCC.shifted_bottom;
						}
						else if (bccline7._yd > 0)
						{
							flag6 = this.Phy.tstacked_y - base.sizey + velocityDir.y * speed > this.PreBCC.shifted_bottom;
						}
					}
					if (flag6)
					{
						this.FootD.rideInitTo(bccline7, false);
						Vector3 velocityDir2 = this.getVelocityDir(bccline7);
						this.Phy.addTranslateStack(velocityDir2.x * speed * 2f, (velocityDir2.y + velocityDir.z) * speed * 2f);
					}
				}
			}
			if (this.alloc_jump_air)
			{
				this.t_leave.Update(base.TS);
			}
			else if (this.En.hasFoot())
			{
				this.t_leave.Clear();
			}
			return true;
		}

		public Vector3 getVelocityDir()
		{
			return this.getVelocityDir(this.PreBCC);
		}

		public Vector3 getVelocityDir(M2BlockColliderContainer.BCCLine PreBCC)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			AIM aim = PreBCC.aim;
			if (aim != AIM.L)
			{
				if (aim != AIM.R)
				{
					float num4 = (float)PreBCC._yd;
					num = (float)(CAim._XD(this.aim_progress, 1) * X.MPF(num4 < 0f));
					if (num4 > 0f && !base.hasFoot())
					{
						num3 = -1f;
					}
				}
				else
				{
					num2 = (float)(-(float)CAim._XD(this.aim_progress, 1));
					if (num2 < 0f && !base.hasFoot())
					{
						num = 0.5f;
					}
				}
			}
			else
			{
				num2 = (float)CAim._XD(this.aim_progress, 1);
				if (num2 < 0f && !base.hasFoot())
				{
					num = -0.5f;
				}
			}
			return new Vector3(num, num2, num3);
		}

		public bool go_to_right()
		{
			for (M2BlockColliderContainer.BCCLine bccline = this.PreBCC; bccline != null; bccline = ((CAim._XD(this.aim_progress, 1) > 0) ? bccline.LinkD : bccline.LinkS))
			{
				AIM foot_aim = bccline.foot_aim;
				if (foot_aim == AIM.T)
				{
					return CAim._XD(this.aim_progress, 1) < 0;
				}
				if (foot_aim == AIM.B)
				{
					return CAim._XD(this.aim_progress, 1) > 0;
				}
			}
			return CAim._XD(this.aim_progress, 1) > 0;
		}

		public NASGroundClimber addChangedFn(NASGroundClimber.FnClimbEvent Fn)
		{
			if (Fn == null)
			{
				return this;
			}
			if (this.Achanged == null)
			{
				this.Achanged = new List<NASGroundClimber.FnClimbEvent>(1);
			}
			this.Achanged.Add(Fn);
			return this;
		}

		private bool triggerEvent(List<NASGroundClimber.FnClimbEvent> AEv, M2BlockColliderContainer.BCCLine Bcc)
		{
			if (AEv == null)
			{
				return true;
			}
			int count = AEv.Count;
			bool flag = true;
			for (int i = 0; i < count; i++)
			{
				flag = AEv[i](Bcc) && flag;
			}
			return flag;
		}

		public M2BlockColliderContainer.BCCLine getPreBcc()
		{
			return this.PreBCC;
		}

		private List<NASGroundClimber.FnClimbEvent> Achanged;

		private M2BlockColliderContainer.BCCLine PreBCC;

		private RevCounter t_leave;

		private RevCounter t_hitwall;

		private RevCounter t_stopping;

		private AIM aim_progress;

		private RevCounter t_stop_hitwall_check;

		private uint pre_corner_slip_bits = 32U;

		public bool alloc_jump_air;

		public uint footable_bits = 15U;

		public int leave_allocate_time = 15;

		public float rotate_x_shift_pixel;

		public bool check_ladder;

		public delegate bool FnClimbEvent(M2BlockColliderContainer.BCCLine Bcc);
	}
}
