using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2Phys : IPauseable
	{
		private static M2Phys createPhys(M2Mover _Mv, Rigidbody2D _Rgd = null)
		{
			return new M2Phys(_Mv, _Rgd);
		}

		public static Func<M2Mover, Rigidbody2D, M2Phys> FD_createPhys
		{
			get
			{
				if (M2Phys.FD_createPhys_ == null)
				{
					M2Phys.FD_createPhys_ = new Func<M2Mover, Rigidbody2D, M2Phys>(M2Phys.createPhys);
				}
				return M2Phys.FD_createPhys_;
			}
		}

		public M2Phys(M2Mover _Mv, Rigidbody2D _Rgd = null)
		{
			this.Mv = _Mv;
			if (this.AFoc == null)
			{
				this.Rgd = _Rgd;
				if (this.Rgd == null)
				{
					this.gameObject.TryGetComponent<Rigidbody2D>(out this.Rgd);
				}
				if (this.Rgd != null)
				{
					this.Rgd.freezeRotation = true;
					this.Rgd.gravityScale = 0f;
					this.Rgd.mass = ((this.Mv.weight < 0f) ? 80f : this.Mv.weight);
					this.Rgd.isKinematic = true;
					this.Rgd.sharedMaterial = MTRX.PmdM2Zero;
					this.Rgd.simulated = true;
				}
				this.default_layer_ = this.Mv.gameObject.layer;
				this.AFoc = new List<M2Phys.P2Foc>();
				this.Pause();
			}
			this.wall_stuck_count = new RevCounter();
			this.base_gravity_ = this.Mv.base_gravity;
			this.force_velocity_x = (this.force_velocity_y = 0f);
			this.AFoc.Clear();
			this.LockMoverHitting = new FlagCounterR<int>();
			this.CheckBCC = new FloatCounter<M2BlockColliderContainer>(2);
			this.LockWallHitting = new FlagCounterR<object>();
			this.LockBlockHitting = new FloatCounter<Collider2D>(4);
			this.LockBlockHitting.ADeleted = new List<Collider2D>(2);
			this.LockGravity = new FloatCounter<object>(4);
			this.snd_t = 0f;
			if (!this.Mv.floating)
			{
				if (this.FootD == null)
				{
					this.FootD = new M2FootManager(this);
				}
				else
				{
					this.FootD.initS();
				}
			}
			this.hit_mover_threshold_ = 1;
			this.fineGravityScale();
		}

		public M2Phys clearLock()
		{
			this.LockMoverHitting.Clear();
			this.LockGravity.Clear(false);
			this.CheckBCC.Clear(false);
			this.LockWallHitting.Clear();
			this.clearColliderLock();
			this.quitSoftFall(0f);
			this.frame_gravity_lock = 0;
			this.pre_release_vel_x = (this.pre_release_vel_y = 0f);
			this.current_gravity_scale_ = -1f;
			this.fineGravityScale();
			if (!this.hasFoot() && this.auto_air_mvhitting)
			{
				this.LockMoverHitting.Add(128, 180f);
			}
			this.Mv.fineHittingLayer();
			return this;
		}

		public void appear(Map2d _Mp)
		{
			this.Mv.killSpeedForce(true, true, false);
			this.clearLock();
			this.no_play_footsnd_animator = false;
			this.Mp.assignPhysicsObject(this);
			this.wall_stuck_count.Clear();
			this.Resume();
			if (this.FootD != null)
			{
				this.FootD.initS();
			}
			this.prex = this.Mv.x;
			this.prey = this.Mv.y;
		}

		public void fineTransformToPos(bool recheck_foot = true)
		{
			Vector2 vector = new Vector2(0f, 0f);
			vector.x = this.Mp.map2meshx(this.x) * 0.015625f;
			vector.y = this.Mp.map2meshy(this.y) * 0.015625f;
			if (this.Rgd != null && M2Phys._immediate_translate)
			{
				vector *= this.Mp.M2D.Cam.base_scale;
				this.Rgd.MovePosition(vector);
			}
			else
			{
				this.Mv.gameObject.transform.localPosition = vector;
			}
			if (recheck_foot)
			{
				this.recheckFoot(0f);
			}
		}

		public M2Phys initSoftFall(float dep_level, float maxt = 0f)
		{
			if (this.softfall_dep_ratio == dep_level && (maxt > 0f || this.softfall_t >= this.softfall_maxt))
			{
				return this;
			}
			if (maxt <= 0f)
			{
				this.softfall_maxt = (this.softfall_t = 0f);
				this.softfall_dep_ratio = dep_level;
				this.softfall_start_ratio = dep_level;
			}
			else
			{
				this.softfall_start_ratio = ((this.softfall_maxt <= 0f) ? this.softfall_dep_ratio : X.NI(this.softfall_start_ratio, this.softfall_dep_ratio, X.ZLINE(this.softfall_t, this.softfall_maxt)));
				this.softfall_dep_ratio = dep_level;
				this.softfall_t = 0f;
				this.softfall_maxt = maxt;
			}
			this.current_gravity_scale_ = -1f;
			return this;
		}

		public bool isSoftFalling()
		{
			return this.softfall_dep_ratio < 1f || this.softfall_t < this.softfall_maxt;
		}

		public M2Phys quitSoftFall(float maxt = 0f)
		{
			return this.initSoftFall(1f, maxt);
		}

		public float fineGravityScale()
		{
			if (this.current_gravity_scale_ < 0f)
			{
				this.current_gravity_scale_ = X.Mx(0f, 1f - this.LockGravity.getMaxLevel()) * ((this.softfall_maxt <= 0f) ? this.softfall_dep_ratio : X.NI(this.softfall_start_ratio, this.softfall_dep_ratio, X.ZLINE(this.softfall_t, this.softfall_maxt)));
			}
			return this.current_gravity_scale_;
		}

		public float current_gravity_scale
		{
			get
			{
				return this.current_gravity_scale_;
			}
		}

		public void changeRiding(IFootable F, FOOTRES footres)
		{
			if (this.FootD == null)
			{
				return;
			}
			if (footres != FOOTRES.FOOTED)
			{
				if (footres - FOOTRES.JUMPED <= 1 && this.auto_air_mvhitting)
				{
					this.addLockMoverHitting(HITLOCK.AIR, -1f);
				}
			}
			else
			{
				if (!this.temporary_no_clear_v_gravity)
				{
					this.v_gravity = 0f;
				}
				this.remLockMoverHitting(HITLOCK.AIR);
			}
			this.Mv.changeRiding(F, footres);
		}

		public void Pause()
		{
			if (this.Rgd != null)
			{
				if (this.PauseMemory == null)
				{
					this.PauseMemory = new PauseMemItemRigidbody(this.Rgd);
				}
				this.PauseMemory.Pause();
			}
			this.is_pause = true;
		}

		public void Resume()
		{
			if (this.Rgd != null)
			{
				if (this.PauseMemory != null)
				{
					this.PauseMemory.isKinematic = false;
					this.PauseMemory.Resume();
				}
				else
				{
					this.Rgd.isKinematic = false;
				}
			}
			this.is_pause = false;
		}

		public void destruct()
		{
			if (this.Mp == null)
			{
				return;
			}
			if (this.FootD != null)
			{
				M2FootManager footD = this.FootD;
				this.FootD = null;
				footD.lockPlayFootStamp(200);
				footD.destruct();
			}
			if (this.Rgd != null)
			{
				this.Rgd.isKinematic = true;
			}
			this.Mp.deassignPhysicsObject(this);
		}

		public void memoryPrePosition()
		{
			if (this.prex != this.x || this.prey != this.y)
			{
				if (this.FootD == null)
				{
					this.Mv.positionChanged(this.prex, this.prey);
					this.prex = this.x;
					this.prey = this.y;
					return;
				}
				this.FootD.need_recheck_current_pos = true;
				int num = (int)this.prex;
				int num2 = (int)this.prey;
				this.Mv.positionChanged(this.prex, this.prey);
				this.FootD.positionChanged(this.prex, this.prey);
				this.prex = this.x;
				this.prey = this.y;
				if ((int)this.prex != num || (int)this.prey != num2)
				{
					this.Mv.IntPositionChanged(num, num2);
					this.FootD.IntPositionChanged(this.prex, this.prey);
				}
			}
		}

		private void runPhysicsInner(float fcnt, ref float vx, ref float vy, ref float drawx_, ref float drawy_, float pre_fall_y)
		{
			if (this.frame_gravity_lock > 0)
			{
				this.frame_gravity_lock -= 1;
			}
			if (this.LockGravity.run(fcnt))
			{
				this.current_gravity_scale_ = -1f;
			}
			if (this.FootD != null)
			{
				this.FootD.runPre();
			}
			bool flag = false;
			if (this.LockMoverHitting.Count > 0)
			{
				this.LockMoverHitting.run(fcnt);
				if (this.LockMoverHitting.Count == 0)
				{
					flag = true;
				}
			}
			if (this.CheckBCC.Count > 0)
			{
				this.CheckBCC.run(fcnt);
			}
			if (this.LockWallHitting.Count > 0)
			{
				this.LockWallHitting.run(fcnt);
				if (this.LockWallHitting.Count == 0)
				{
					flag = true;
				}
			}
			if (this.LockBlockHitting.run(fcnt))
			{
				this.fineDeletedBlockHitting();
			}
			if (this.Mp.BCC != null && this.Mp.BCC.is_prepared)
			{
				this.wall_stuck_count.Update(fcnt);
			}
			if (this.softfall_maxt > this.softfall_t)
			{
				this.current_gravity_scale_ = -1f;
				this.softfall_t += fcnt;
				if (this.softfall_t >= this.softfall_maxt)
				{
					this.softfall_start_ratio = this.softfall_dep_ratio;
					this.softfall_maxt = (this.softfall_t = 0f);
				}
			}
			if (flag)
			{
				this.Mv.fineHittingLayer();
			}
			if (this.current_gravity_scale_ < 0f)
			{
				this.fineGravityScale();
			}
			if (this.Rgd != null)
			{
				Vector2 velocity = this.Rgd.velocity;
				this.fineSpeedValue(velocity, ref vx, ref vy);
			}
			if (this.clampvx_l >= 0f)
			{
				if (vx < -this.clampvx_l)
				{
					vx = -this.clampvx_l;
				}
				this.clampvx_l = -1f;
			}
			if (this.clampvy_t >= 0f)
			{
				if (vy < -this.clampvy_t)
				{
					vy = -this.clampvy_t;
				}
				this.clampvy_t = -1f;
			}
			if (this.clampvx_r >= 0f)
			{
				if (vx > this.clampvx_r)
				{
					vx = this.clampvx_r;
				}
				this.clampvx_r = -1f;
			}
			if (this.clampvy_b >= 0f)
			{
				if (vy > this.clampvy_b)
				{
					this.v_gravity = X.Mx(this.v_gravity - (vy - this.clampvy_b), 0f);
					vy = this.clampvy_b;
				}
				this.clampvy_b = -1f;
			}
			this.considerVelocity(fcnt, ref vx, ref vy, ref drawx_, ref drawy_);
			for (int i = this.AFoc.Count - 1; i >= 0; i--)
			{
				M2Phys.P2Foc p2Foc = this.AFoc[i];
				if (!p2Foc.isActive())
				{
					if ((p2Foc.foctype & FOCTYPE._GRAVITY_LOCK) != (FOCTYPE)0U)
					{
						this.current_gravity_scale_ = -1f;
					}
					this.AFoc.RemoveAt(i);
				}
				else if (p2Foc.t >= (float)p2Foc.maxt)
				{
					p2Foc.velocity_x = (p2Foc.velocity_y = 0f);
				}
			}
			float num = (this.isin_water ? this.water_speed_scale : 1f);
			float num2 = vy - this.v_gravity;
			num2 = ((this.force_velocity_y > 0f) ? X.Mx(num2 - this.force_velocity_y, X.Mn(num2, 0f)) : ((this.force_velocity_y < 0f) ? X.Mn(num2 - this.force_velocity_y, X.Mx(num2, 0f)) : num2));
			num2 += this.v_gravity;
			if (this.base_gravity_ != 0f && this.FootD != null)
			{
				if (this.Mv.floating)
				{
					if (this.hasFoot())
					{
						this.FootD.rideInitTo(null, false);
					}
				}
				else if (this.unity_physics_mode_)
				{
					this.temporary_no_clear_v_gravity = true;
					float num3 = this.Mv.transform.localEulerAngles.z * 3.1415927f / 180f;
					if (this.rgd_agR_ != num3)
					{
						this.rgd_agR_ = num3;
						this.rgd_cos_ = X.Abs(X.Cos(this.rgd_agR_));
						this.rgd_sin_ = X.Abs(X.Sin(this.rgd_agR_));
					}
					if (this.recheckFoot(pre_fall_y))
					{
						num2 = X.Mn(num2, 0f);
						this.v_gravity = 0f;
					}
					this.temporary_no_clear_v_gravity = false;
					if (this.Rgd != null)
					{
						this.Rgd.gravityScale = this.gravity_apply_velocity(fcnt * num) * 50f;
					}
				}
				else
				{
					this.temporary_no_clear_v_gravity = true;
					if (this.recheckFoot(pre_fall_y))
					{
						this.temporary_no_clear_v_gravity = false;
						num2 -= this.v_gravity;
						this.v_gravity = 0f;
					}
					else
					{
						this.temporary_no_clear_v_gravity = false;
						this.FootD.rideInitTo(null, false);
						float num4 = this.current_ySpeedMax * num;
						float num5;
						if (this.frame_gravity_lock > 0 || (num5 = this.gravity_apply_velocity(fcnt * num)) == 0f)
						{
							if (this.v_gravity > 0f)
							{
								num2 = X.Mx(num2 - this.v_gravity, X.Mn(num2, 0f));
								this.v_gravity = 0f;
							}
						}
						else if (num2 < num4 && (this.Mp.BCC == null || this.Mp.BCC.is_prepared))
						{
							float num6 = X.Mn(num2 + num5, num4);
							this.v_gravity += num6 - num2;
							num2 = num6;
						}
						else if (num2 > num4)
						{
							this.v_gravity -= num2 - num4;
							num2 = num4;
						}
					}
				}
			}
			float num7 = vx;
			if (this.isin_water_t > 0)
			{
				num7 -= this.force_velocity_x;
				if (num7 != 0f)
				{
					num7 = X.VALWALK(num7, 0f, 0.015f);
				}
				num7 += this.force_velocity_x;
				num2 -= this.v_gravity;
				if (num2 != 0f)
				{
					num2 = X.VALWALK(num2, 0f, (num2 < 0f) ? 0.002f : 0.008f);
				}
				num2 += this.v_gravity;
			}
			if (this.hit_wall_collider)
			{
				this.v_gravity = X.Mx(0f, X.Mn(this.v_gravity, num2));
				num7 = this.force_velocity_x;
				num2 = this.force_velocity_y + this.v_gravity;
			}
			else if (this.hit_mover_friction_)
			{
				num7 = this.force_velocity_x;
				num2 = this.force_velocity_y + this.v_gravity;
			}
			else
			{
				num2 += this.force_velocity_y;
			}
			vy = num2;
			vx = num7;
			if (!this.unity_physics_mode_ || this.force_velocity_y != 0f || this.force_velocity_x != 0f)
			{
				this.fixRigidBodySpeed(vx, vy);
			}
			if (this.snd_t > 0f)
			{
				this.snd_t = X.VALWALK(this.snd_t, 0f, this.Mv.TS);
			}
			if (this.Rgd != null && !this.Rgd.freezeRotation && this.hasFoot())
			{
				this.Rgd.angularVelocity = X.VALWALK(this.Rgd.angularVelocity, 0f, 4f * this.Mv.TS);
			}
			if (this.Mv.isRingOut() && Map2d.can_handle)
			{
				this.Mv.setToDefaultPosition(false, null);
			}
			if (this.isin_water_t > 0)
			{
				byte b = this.isin_water_t - 1;
				this.isin_water_t = b;
				if (b == 0)
				{
					if (this.Mv is M2Attackable)
					{
						(this.Mv as M2Attackable).setWaterReleaseEffect(this.isin_water_id_);
					}
					this.isin_water_id_ = -2;
				}
			}
		}

		private bool considerVelocity(float fcnt, ref float vx, ref float vy, ref float drawx_, ref float drawy_)
		{
			bool flag = this.Mv.considerFricOnVelocityCalc();
			bool flag2 = this.hasFoot();
			this.pre_x_attached = (this.pre_y_attached = (FOCTYPE)0U);
			float num = vy - this.v_gravity;
			float num2 = ((this.force_velocity_x > 0f) ? X.Mx(vx - this.force_velocity_x, X.Mn(vx, 0f)) : ((this.force_velocity_x < 0f) ? X.Mn(vx - this.force_velocity_x, X.Mx(vx, 0f)) : vx));
			num = ((this.force_velocity_y > 0f) ? X.Mx(num - this.force_velocity_y, X.Mn(num, 0f)) : ((this.force_velocity_y < 0f) ? X.Mn(num - this.force_velocity_y, X.Mx(num, 0f)) : num));
			bool flag3 = this.fric_t > 0f && X.Abs(this.pre_release_vel_x - num2) > 0.002f;
			if (this.v_gravity > 0f && num < -this.v_gravity * 0.5f)
			{
				num += this.v_gravity;
				this.v_gravity = 0f;
			}
			bool flag4 = this.AFoc.Count > 0;
			if (flag && num2 != 0f && this.fric_reduce_x != 0f)
			{
				num2 = X.VALWALK(num2, 0f, this.fric_reduce_x);
				flag4 = true;
			}
			this.force_velocity_x = (this.force_velocity_y = 0f);
			float num3 = 0f;
			float num4 = 0f;
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			this.pre_adjust_vy = 0f;
			float num5 = ((this.isin_water_t > 0) ? this.water_speed_scale : 1f);
			for (int i = this.AFoc.Count - 1; i >= 0; i--)
			{
				M2Phys.P2Foc p2Foc = this.AFoc[i];
				if (p2Foc.isActive() && (p2Foc.velocity_x != 0f || p2Foc.velocity_y != 0f))
				{
					if (p2Foc.fric_time == 0 && flag)
					{
						p2Foc.deactivate();
					}
					else
					{
						float num6 = ((p2Foc.fric_time > 0) ? (1f - X.ZLINE(p2Foc.t_fric - (float)p2Foc.fric_ignore, (float)p2Foc.fric_time)) : 1f);
						if (p2Foc.t_attack >= 0)
						{
							if (p2Foc.t < (float)p2Foc.t_attack)
							{
								num6 *= X.ZLINE(p2Foc.t, (float)p2Foc.t_attack);
							}
							else if (p2Foc.t >= (float)(p2Foc.t_attack + p2Foc.t_hold))
							{
								if (p2Foc.t_release > 0)
								{
									num6 *= 1f - X.ZLINE(p2Foc.t - (float)p2Foc.t_attack - (float)p2Foc.t_hold, (float)p2Foc.t_release);
								}
								else
								{
									num6 = 0f;
								}
							}
						}
						if (this.hit_wall_collider && p2Foc.t >= 1f && p2Foc.velocity_x != 0f && (p2Foc.foctype & (FOCTYPE.WALK | FOCTYPE.JUMP)) == (FOCTYPE)0U)
						{
							p2Foc.velocity_x = X.absMn(p2Foc.velocity_x, X.Abs(vx));
						}
						float num7 = this.speedRatio(p2Foc);
						float num8 = num7;
						if (num5 != 1f)
						{
							num8 = (((p2Foc.foctype & (FOCTYPE.CARRY | FOCTYPE.RESIZE | FOCTYPE._NO_CONSIDER_WATER)) != (FOCTYPE)0U) ? 1f : num5);
							if (p2Foc.t_attack >= 0)
							{
								p2Foc.t += fcnt * num8;
							}
							else
							{
								p2Foc.t += fcnt * num7;
							}
						}
						else
						{
							p2Foc.t += fcnt * num7;
						}
						bool flag8 = false;
						if (p2Foc.t_attack < 0)
						{
							if (p2Foc.t_attack == -1)
							{
								flag8 = true;
							}
							if (p2Foc.t_attack == -2)
							{
								p2Foc.t_attack = -3;
							}
							if (p2Foc.t_attack <= -2)
							{
								p2Foc.t = 0f;
							}
						}
						else
						{
							flag8 = p2Foc.t >= (float)p2Foc.maxt;
						}
						if ((p2Foc.foctype & FOCTYPE._KILL_Y) != (FOCTYPE)0U)
						{
							num = 0f;
						}
						if ((p2Foc.foctype & FOCTYPE._GRAVITY_LOCK) != (FOCTYPE)0U && this.frame_gravity_lock == 0)
						{
							this.frame_gravity_lock += 1;
						}
						if (num6 > 0f)
						{
							float num9 = p2Foc.velocity_x * num6 * num8;
							if (num9 != 0f)
							{
								float num10 = ((p2Foc.max < 0f) ? (X.Abs(num9) * -p2Foc.max) : p2Foc.max);
								float num11 = this.force_velocity_x;
								if (num9 > 0f)
								{
									this.force_velocity_x = X.Mn(this.force_velocity_x + num9, X.Mx(this.force_velocity_x, X.MMX(0f, num10 - num2, num10)));
								}
								else if (num9 < 0f)
								{
									this.force_velocity_x = X.Mx(this.force_velocity_x + num9, X.Mn(this.force_velocity_x, X.MMX(-num10, -num10 - num2, 0f)));
								}
								if (flag8 && this.releaseVelocity(p2Foc.foctype, false))
								{
									num3 += this.force_velocity_x - num11;
								}
								flag7 = flag7 || (p2Foc.foctype & FOCTYPE._CLIFF_STOPPER) > (FOCTYPE)0U;
								flag5 = flag5 || (p2Foc.foctype & FOCTYPE._CHECK_WALL) > (FOCTYPE)0U;
								this.pre_x_attached |= p2Foc.foctype & ~FOCTYPE.__ADDITIONAL;
							}
							num6 *= (((p2Foc.foctype & FOCTYPE.JUMP) != (FOCTYPE)0U) ? 1f : num8);
							num9 = p2Foc.velocity_y * num6;
							if (num9 != 0f)
							{
								float num12 = this.force_velocity_y;
								float num13 = ((p2Foc.max < 0f) ? (X.Abs(num9) * -p2Foc.max) : p2Foc.max);
								if (num9 > 0f)
								{
									this.force_velocity_y = X.Mn(this.force_velocity_y + num9, X.Mx(this.force_velocity_y, X.MMX(0f, num13 - num, num13)));
								}
								else if (num9 < 0f)
								{
									this.force_velocity_y = X.Mx(this.force_velocity_y + num9, X.Mn(this.force_velocity_y, X.MMX(-num13, -num13 - num, 0f)));
								}
								if (flag8 && this.releaseVelocity(p2Foc.foctype, true))
								{
									num4 += this.force_velocity_y - num12;
								}
								flag6 = flag6 || (p2Foc.foctype & FOCTYPE._CHECK_WALL) > (FOCTYPE)0U;
								this.pre_y_attached |= p2Foc.foctype & ~FOCTYPE.__ADDITIONAL;
								if ((p2Foc.foctype & FOCTYPE.CARRY) != (FOCTYPE)0U)
								{
									float num14 = this.force_velocity_y - num12;
									this.pre_adjust_vy += num14;
									this.force_velocity_y = num12;
								}
							}
						}
						if (p2Foc.t_attack == -1)
						{
							p2Foc.deactivate();
						}
						else if (p2Foc.fric_time > 0)
						{
							if (((p2Foc.foctype & FOCTYPE._FRIC_STRICT) != (FOCTYPE)0U) ? this.hasFoot() : flag)
							{
								p2Foc.t_fric += fcnt * num7;
								if (p2Foc.t_fric >= (float)(p2Foc.fric_ignore + p2Foc.fric_time))
								{
									p2Foc.deactivate();
								}
							}
							else if (p2Foc.t_fric > 0f)
							{
								p2Foc.t_fric = X.Mx(0f, p2Foc.t_fric - fcnt * num7);
							}
						}
					}
				}
			}
			if ((flag5 || flag6) && this.LockWallHitting.Count > 0)
			{
				flag6 = (flag5 = false);
			}
			if (flag7 && this.FootD != null && this.FootD.hasFoot())
			{
				M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
				if (footBCC != null && !footBCC.isWall())
				{
					M2BlockColliderContainer.BCCLine bccline = ((this.force_velocity_x < 0f) ? footBCC.SideL : footBCC.SideR);
					if (bccline == null || !bccline.isUseableDir(this.FootD))
					{
						float num15 = this.force_velocity_x;
						float num16;
						float num17;
						footBCC.BCC.getBaseShift(out num16, out num17);
						if (this.force_velocity_x < 0f)
						{
							this.force_velocity_x = X.Mx(this.force_velocity_x, footBCC.x - num16 - this.mleft);
						}
						else
						{
							this.force_velocity_x = X.Mn(this.force_velocity_x, footBCC.right - num16 - this.mright);
						}
						if (this.force_velocity_x != num15)
						{
							bool is_lift = footBCC.is_lift;
							M2BlockColliderContainer.BCCLine bccline2;
							footBCC.BCC.isFallable((float)X.MPF(num15 > 0f) * (this.Mv.sizex + 0.35f) + this.x, this.Mv.mbottom, 0.001f, 0.25f, out bccline2, is_lift, !is_lift, -1f);
							if (bccline2 != null && bccline2 != footBCC)
							{
								this.force_velocity_x = num15;
							}
						}
					}
				}
			}
			if (flag5 && this.force_velocity_x != 0f)
			{
				if (Map2d.can_handle)
				{
					float num18 = (float)X.Mx(this.Mp.crop - 2, 0) + 0.04f;
					if (this.force_velocity_x < 0f && this.mleft + this.force_velocity_x < num18)
					{
						this.force_velocity_x = 0f;
					}
					if (this.force_velocity_x > 0f && this.mright + this.force_velocity_x > (float)this.Mp.clms - num18)
					{
						this.force_velocity_x = 0f;
					}
				}
			}
			else
			{
				this.cfg_pre_slope = 0;
			}
			if (this.fric_t > 0f)
			{
				num2 = this.pre_release_vel_x;
				num = this.pre_release_vel_y;
				if (!flag3)
				{
					this.fric_t = X.Mx(this.fric_t - fcnt, 0f);
				}
				if (num2 != 0f)
				{
					num2 = X.VALWALK(num2, 0f, 0.25f);
				}
				if (num != 0f)
				{
					num = X.VALWALK(num, 0f, 0.25f);
				}
			}
			else if (flag)
			{
				if (num2 != 0f)
				{
					num2 = X.VALWALK(num2, 0f, 0.05f);
				}
				if (num != 0f)
				{
					num = X.VALWALK(num, 0f, 0.05f);
				}
			}
			vx = num2 + this.force_velocity_x;
			this.force_velocity_y += this.pre_adjust_vy;
			vy = num + this.force_velocity_y + this.v_gravity;
			if (flag5 || flag6)
			{
				bool flag9 = false;
				float num19 = vx;
				float num20 = vy;
				if (flag5)
				{
					M2BlockColliderContainer.BCCLine bccline3;
					flag9 = this.checkSideLRBCC(ref num19, flag2, out bccline3) || flag9;
				}
				if (flag6)
				{
					M2BlockColliderContainer.BCCLine bccline3;
					flag9 = this.checkSideTBBCC(ref num20, flag2, out bccline3) || flag9;
				}
				if (flag9)
				{
					vx = num19;
					this.force_velocity_x = vx - num2;
					vy = num20;
					this.force_velocity_y = vy - num - this.v_gravity;
					flag4 = true;
				}
			}
			this.pre_release_vel_x = num2;
			this.pre_release_vel_y = num;
			if (num3 != 0f)
			{
				this.force_velocity_x -= num3;
				this.pre_release_vel_x += num3;
				flag4 = true;
			}
			if (num4 != 0f)
			{
				this.force_velocity_y -= num4;
				this.pre_release_vel_y += num4;
				flag4 = true;
			}
			if (this.Mv.using_draw_assist && (vx != 0f || vy != 0f))
			{
				drawx_ += vx * fcnt * this.CLEN;
				drawy_ += vy * fcnt * this.CLEN;
			}
			if (vx == 0f)
			{
				float num21 = vy;
			}
			return flag4;
		}

		protected virtual float speedRatio(M2Phys.P2Foc Foc)
		{
			return 1f;
		}

		public bool checkSideLRBCC(ref float __vx, bool has_foot, out M2BlockColliderContainer.BCCLine HitBcc)
		{
			bool flag = false;
			AIM aim = ((__vx > 0f) ? AIM.R : AIM.L);
			float num = (has_foot ? 0.1f : 0.2f) + X.Abs(__vx);
			float num2;
			if (!this.Mv.canGoToSideLB(out HitBcc, out num2, aim, num, -0.01f, false, false, false))
			{
				flag = this.checkSideLRBCC(ref __vx, ref num, HitBcc, X.Abs(num2), has_foot, aim);
			}
			foreach (KeyValuePair<M2BlockColliderContainer, FloatCounter<M2BlockColliderContainer>.FloatCounterItem> keyValuePair in this.CheckBCC.getRawObject())
			{
				M2BlockColliderContainer key = keyValuePair.Key;
				bool flag2 = this.FootD != null && this.FootD.FootTargetIsBcc(key);
				if ((num2 = key.canGoToSide(out HitBcc, this.FootD, 0f, 0f, aim, num, flag2 ? (-0.25f) : (-0.1f), this.hasFoot(), false)) != 0f)
				{
					flag = this.checkSideLRBCC(ref __vx, ref num, HitBcc, X.Abs(num2), has_foot, aim);
				}
			}
			return flag;
		}

		public bool checkSideTBBCC(ref float __vy, bool has_foot, out M2BlockColliderContainer.BCCLine HitBcc)
		{
			bool flag = false;
			AIM aim = ((__vy > 0f) ? AIM.B : AIM.T);
			float num = 0.2f + X.Abs(__vy);
			float num2;
			if (!this.Mv.canGoToSideLB(out HitBcc, out num2, aim, num, -0.01f, false, false, false))
			{
				flag = this.checkSideTBBCC(ref __vy, ref num, HitBcc, X.Abs(num2), has_foot, aim);
			}
			foreach (KeyValuePair<M2BlockColliderContainer, FloatCounter<M2BlockColliderContainer>.FloatCounterItem> keyValuePair in this.CheckBCC.getRawObject())
			{
				M2BlockColliderContainer key = keyValuePair.Key;
				bool flag2 = this.FootD != null && this.FootD.FootTargetIsBcc(key);
				if ((num2 = key.canGoToSide(out HitBcc, this.FootD, 0f, 0f, aim, num, flag2 ? (-0.25f) : (-0.01f), this.hasFoot(), false)) != 0f)
				{
					flag = this.checkSideTBBCC(ref __vy, ref num, HitBcc, X.Abs(num2), has_foot, aim);
				}
			}
			return flag;
		}

		private bool checkSideLRBCC(ref float __vx, ref float xmargin, M2BlockColliderContainer.BCCLine HitBcc, float backlen, bool has_foot, AIM chka)
		{
			float num = __vx;
			__vx = ((this.FootD != null) ? X.VALWALK(__vx, 0f, backlen) : 0f);
			xmargin = X.VALWALK(xmargin, 0f, backlen);
			this.Mv.endDrawAssist(1);
			bool flag = true;
			if (HitBcc != null && !HitBcc.is_naname)
			{
				flag = false;
				if (((this.corner_slip_alloc_bits & 2U) > 0U || has_foot) && HitBcc.y + 0.3f > this.mbottom && !HitBcc.isUseableDir(this.FootD) && (HitBcc.L_is_270 || HitBcc.L_is_same_yd))
				{
					bool flag2 = false;
					if (has_foot)
					{
						flag2 = HitBcc.SideRidingCheck(this.FootD, this.x, this.y, 0f, -1f, true, 0.2f, true) != null;
						if (flag2)
						{
							this.translate_stack_x += num;
						}
					}
					if (!flag2)
					{
						this.translate_stack_y -= X.MMX(0.01f, backlen, 0.04f);
					}
				}
				else if ((this.corner_slip_alloc_bits & 8U) != 0U && HitBcc.bottom - 0.3f < this.mtop && !HitBcc.isUseableDir(this.FootD) && (HitBcc.R_is_270 || HitBcc.R_is_same_yd))
				{
					bool flag3 = false;
					if (has_foot)
					{
						flag3 = HitBcc.SideRidingCheck(this.FootD, this.x, this.y, 0f, 1f, true, 0.2f, true) != null;
						if (flag3)
						{
							this.translate_stack_x += num;
						}
					}
					if (!flag3)
					{
						this.translate_stack_y += X.MMX(0.01f, backlen, 0.04f);
					}
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				this.Mv.setWallHitted(chka);
			}
			return true;
		}

		private bool checkSideTBBCC(ref float __vy, ref float xmargin, M2BlockColliderContainer.BCCLine HitBcc, float backlen, bool has_foot, AIM chka)
		{
			__vy = ((this.FootD != null) ? X.VALWALK(__vy, 0f, backlen) : 0f);
			xmargin = X.VALWALK(xmargin, 0f, backlen);
			this.Mv.endDrawAssist(1);
			bool flag = true;
			if (HitBcc != null && !HitBcc.is_naname && !this.FootD.FootIsLadder())
			{
				flag = false;
				if ((this.corner_slip_alloc_bits & 1U) != 0U && HitBcc.x + 0.3f > this.mleft && !HitBcc.isUseableDir(this.FootD) && HitBcc.L_is_270)
				{
					this.translate_stack_x -= X.MMX(0.01f, backlen, 0.04f);
				}
				else if ((this.corner_slip_alloc_bits & 4U) != 0U && HitBcc.right - 0.3f < this.mright && !HitBcc.isUseableDir(this.FootD) && HitBcc.R_is_270)
				{
					this.translate_stack_x += X.MMX(0.01f, backlen, 0.04f);
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				this.Mv.setWallHitted(chka);
			}
			return true;
		}

		public void clipWalkXSpeed()
		{
			if (this.walk_xspeed_manageable_air >= 0f)
			{
				this.walk_xspeed_ = X.MMX(-this.walk_xspeed_manageable_air, this.walk_xspeed_, this.walk_xspeed_manageable_air);
			}
		}

		public float walk_xspeed
		{
			get
			{
				return this.walk_xspeed_;
			}
			set
			{
				this.setWalkXSpeed(value, true, false);
			}
		}

		public void setWalkXSpeed(float value, bool consider_water_scale = true, bool force_onfoot = false)
		{
			if (force_onfoot || this.FootD == null || this.FootD.canStartRunning() || this.walk_xspeed_manageable_air < 0f || X.BTWW(-this.walk_xspeed_manageable_air, value, this.walk_xspeed_manageable_air))
			{
				this.walk_xspeed_ = value * ((consider_water_scale && this.isin_water) ? this.water_speed_scale : 1f);
				return;
			}
			float num = X.Mn(-this.walk_xspeed_manageable_air, this.walk_xspeed_);
			float num2 = X.Mx(this.walk_xspeed_manageable_air, this.walk_xspeed_);
			this.walk_xspeed_ = X.MMX(num, value * ((consider_water_scale && this.isin_water) ? this.water_speed_scale : 1f), num2);
		}

		public void runPre()
		{
			for (int i = this.AFoc.Count - 1; i >= 0; i--)
			{
				M2Phys.P2Foc p2Foc = this.AFoc[i];
				if (p2Foc.t_attack == -3)
				{
					p2Foc.deactivate();
					p2Foc.velocity_x = (p2Foc.velocity_y = 0f);
				}
			}
		}

		public void runPost(float vx, float vy)
		{
			if (this.main_updated_count <= 0)
			{
				this.main_updated_count = 1;
				return;
			}
			this.main_updated_count += 1;
		}

		public void runPhysics(float fcnt, ref float vx, ref float vy, ref float drawx_, ref float drawy_, List<ICarryable> ACarry)
		{
			if (this.is_pause)
			{
				return;
			}
			float num;
			if (this.main_updated_count <= 0)
			{
				num = this.Mv.y;
				this.Mv.finePositionFromTransform();
				num = this.Mv.y - num;
			}
			else
			{
				num = this.prey - this.Mv.y;
				this.hit_mover = 0;
				this.hit_mover_friction_ = false;
				this.memoryPrePosition();
			}
			if (this.walk_xspeed_ != 0f)
			{
				if (this.FootD != null)
				{
					this.FootD.need_recheck_current_pos = true;
				}
				float num2 = this.force_velocity_x;
				float num3 = ((this.walk_xspeed_ > 0f) ? X.Mx(this.walk_xspeed_, this.force_velocity_x + this.walk_xspeed_) : X.Mn(this.walk_xspeed_, this.force_velocity_x + this.walk_xspeed_));
				this.addFoc(FOCTYPE.WALK | FOCTYPE._NO_CONSIDER_WATER | FOCTYPE._CHECK_WALL, num3 - num2, 0f, -1f, -1, 1, 0, -1, 0);
			}
			this.runPhysicsInner(fcnt, ref vx, ref vy, ref drawx_, ref drawy_, num);
			if (this.FootD != null)
			{
				this.FootD.runPhysics(fcnt);
			}
			if (ACarry != null && this.TS != 0f && (vx != 0f || vy != 0f))
			{
				int count = ACarry.Count;
				Collider2D myCollider = this.MyCollider;
				for (int i = count - 1; i >= 0; i--)
				{
					if (!ACarry[i].moveWithFoot(vx, vy, myCollider, this.MyBCC, this.carrying_no_collider_lock))
					{
						ACarry.RemoveAt(i);
					}
				}
			}
			if (this.Mp.BCC != null && this.Mp.BCC.is_prepared)
			{
				if (this.wall_stuck_count >= 40f)
				{
					int num4 = this.Mv.checkStuckInWall(ref this.PreStackBCC, this.wall_stuck_count >= 44f);
					if (num4 < 1)
					{
						if (num4 == 0 && (this.FootD == null || this.FootD.get_Foot() == null) && this.Mv.isStuckInWall(this.wall_stuck_count >= 48f))
						{
							this.Mv.stuckExtractFailure(new Rect((float)this.Mp.crop, (float)this.Mp.crop, (float)(this.Mp.clms - this.Mp.crop * 2), (float)(this.Mp.rows - this.Mp.crop * 2)));
							if (this.wall_stuck_count >= 54f)
							{
								this.wall_stuck_count.Set(54f, true);
							}
						}
						else
						{
							this.wall_stuck_count.Set((this.Mp.floort < 50f) ? 40f : 20f, true);
						}
					}
					else if (this.wall_stuck_count >= 47f)
					{
						this.wall_stuck_count.Set(47f, true);
					}
				}
				else
				{
					this.PreStackBCC = null;
				}
			}
			if (this.translate_stack_x != 0f || this.translate_stack_y != 0f)
			{
				bool flag = false;
				float ts = this.TS;
				if (this.Rgd != null && ts != 0f && this.Mp.floort > 10f)
				{
					flag = true;
					this.translate_stack_x /= ts;
					this.translate_stack_y /= ts;
				}
				this.force_velocity_x += this.translate_stack_x;
				this.force_velocity_y += this.translate_stack_y;
				vx += this.translate_stack_x;
				vy += this.translate_stack_y;
				if (flag)
				{
					this.fixRigidBodySpeed(vx, vy);
				}
				else
				{
					this.Mv.moveBy(this.translate_stack_x, this.translate_stack_y, true);
				}
				this.translate_stack_x = (this.translate_stack_y = 0f);
			}
			if (this.main_updated_count > 0)
			{
				this.main_updated_count = 0;
				return;
			}
			this.main_updated_count -= 1;
		}

		public void addTranslateStack(float x, float y)
		{
			this.translate_stack_x += x;
			this.translate_stack_y += y;
		}

		public void immidiateCheckStuck()
		{
			this.wall_stuck_count.Set(40f, false);
		}

		public void checkStuckInWall(float dx, float dy, ref float vy, bool kill_gravity = false)
		{
			if (this.Mv.isStuckInWall(true))
			{
				this.fineWallStuck(false);
			}
			this.addCollirderLock(this.Mp.getCollider(), 2f, null, 0.5f);
			if (this.v_gravity > 0f && kill_gravity && X.LENGTHXYS(0f, 0f, dx, dy) > 0.0001f && (dx != 0f || dy < 0f))
			{
				this.addLockGravityFrame(3);
				vy = X.Mx(vy - this.v_gravity, 0f);
				this.v_gravity = 0f;
			}
			dx = X.absMn(dx, 0.14f);
			dy = X.absMn(dy, 0.14f);
			this.translate_stack_x += dx;
			this.translate_stack_y += dy;
		}

		public M2Phys addFocFallingY(FOCTYPE type, float vy, float gravity_level = 1f, int _fric_time = 0)
		{
			if (vy < 0f)
			{
				if (this.base_gravity > 0f)
				{
					this.addFoc(type, 0f, vy, -1f, 0, 0, (int)X.MMX(1f, -vy / M2Phys.getGravityApplyVelocity(this.Mp, this.base_gravity, gravity_level), 40f), _fric_time, 0);
				}
			}
			else if (vy > 0f)
			{
				this.addFoc(type | FOCTYPE._RELEASE, 0f, vy, -1f, -1, 1, 0, _fric_time, 0);
			}
			return this;
		}

		public M2Phys addFocToSmooth(FOCTYPE type, float depx, float depy, int maxt, int fric_time = -1, int fric_ignore = 0, float max_abs = -1f)
		{
			if (maxt <= 0)
			{
				return this;
			}
			float num = depx - this.x;
			float num2 = depy - this.y;
			return this.addFoc(type, num * 2f / (float)maxt, num2 * 2f / (float)maxt, max_abs, 0, 0, maxt, fric_time, fric_ignore);
		}

		public M2Phys addFoc(FOCTYPE type, float velocity_x, float velocity_y, float max_abs = -1f, int t_attack = -1, int t_hold = 1, int t_release = 0, int fric_time = -1, int fric_ignore = 0)
		{
			if ((type & FOCTYPE._IMMEDIATE) != (FOCTYPE)0U || t_attack < 0 || velocity_x == 0f || velocity_y == 0f)
			{
				return this.addFocXy(type, velocity_x, velocity_y, max_abs, t_attack, t_hold, t_release, fric_time, fric_ignore);
			}
			if ((type & FOCTYPE._INDIVIDUAL) > (FOCTYPE)0U)
			{
				type &= ~FOCTYPE._INDIVIDUAL;
				this.remFoc(type & ~FOCTYPE.__ADDITIONAL, true);
				if ((type & FOCTYPE.DAMAGE) != (FOCTYPE)0U)
				{
					this.remFocForAim(type & ~FOCTYPE.__ADDITIONAL, CAim.get_aim2(0f, 0f, velocity_x, -velocity_y, false));
				}
			}
			if ((type & (FOCTYPE.WALK | FOCTYPE.CARRY | FOCTYPE.RESIZE | FOCTYPE._INDIVIDUAL)) != (FOCTYPE)0U || this.Mv.base_gravity == 0f)
			{
				this.addFocXy(type, velocity_x, velocity_y, max_abs, t_attack, t_hold, t_release, fric_time, fric_ignore);
			}
			else
			{
				if (velocity_y != 0f)
				{
					this.addFocXy(type, 0f, velocity_y, -1f, -1, 1, 0, -1, 0);
				}
				if (velocity_x != 0f)
				{
					this.addFocXy(type, velocity_x, 0f, max_abs, t_attack, t_hold, t_release, fric_time, fric_ignore);
				}
			}
			return this;
		}

		public virtual M2Phys addFocXy(FOCTYPE type, float velocity_x, float velocity_y, float max_abs = -1f, int t_attack = -1, int t_hold = 1, int t_release = 0, int fric_time = -1, int fric_ignore = 0)
		{
			if (velocity_x == 0f && velocity_y == 0f)
			{
				return this;
			}
			if ((type & (FOCTYPE.HIT | FOCTYPE.KNOCKBACK | FOCTYPE.DAMAGE)) != (FOCTYPE)0U)
			{
				if (this.FnHitting != null && !this.FnHitting(this, type, ref velocity_x, ref velocity_y))
				{
					return this;
				}
				if (this.damage_foc_decline_when_move_script_attached && this.Mv.move_script_attached && (type & (FOCTYPE.HIT | FOCTYPE.DAMAGE)) != (FOCTYPE)0U)
				{
					return this;
				}
			}
			M2Phys.P2Foc p2Foc = null;
			bool flag = (type & FOCTYPE._INDIVIDUAL) > (FOCTYPE)0U;
			if ((type & FOCTYPE.DAMAGE) != (FOCTYPE)0U && this.FootD != null && this.FootD.FootIsLadder())
			{
				this.FootD.initJump(false, false, false);
			}
			if (flag)
			{
				type &= ~FOCTYPE._INDIVIDUAL;
				this.remFoc(type & ~FOCTYPE.__ADDITIONAL, true);
				if ((type & FOCTYPE.DAMAGE) != (FOCTYPE)0U)
				{
					this.remFocForAim(type & ~FOCTYPE.__ADDITIONAL, CAim.get_aim2(0f, 0f, velocity_x, -velocity_y, false));
				}
			}
			if (this.FootD != null && (type & (FOCTYPE.CARRY | FOCTYPE.RESIZE)) == (FOCTYPE)0U && this.FootD.reverseVelocityApplied(velocity_x, velocity_y))
			{
				this.FootD.initJump(false, false, false);
			}
			if ((type & FOCTYPE._IMMEDIATE) != (FOCTYPE)0U)
			{
				if ((type & FOCTYPE._CHECK_WALL) != (FOCTYPE)0U)
				{
					this.Mv.getMovableLen(ref velocity_x, ref velocity_y, -0.1f, false, false);
				}
				this.Mv.moveBy(velocity_x * this.TS, velocity_y * this.TS, false);
				M2Phys._immediate_translate = false;
				this.Mv.endDrawAssist(1);
				return this;
			}
			if ((type & FOCTYPE.RESIZE) != (FOCTYPE)0U)
			{
				if (t_attack < 0 && t_hold == 1 && t_release == 0)
				{
					this.translate_stack_x += velocity_x;
					if (velocity_y != 0f)
					{
						this.translate_stack_y += velocity_y;
						this.resized_y = true;
					}
					return this;
				}
				X.dl("FOCTYPE.RESIZE は時間を指定して呼ぶべきではない", null, false, false);
			}
			for (int i = this.AFoc.Count - 1; i >= 0; i--)
			{
				M2Phys.P2Foc p2Foc2 = this.AFoc[i];
				if (flag && (p2Foc2.foctype & ~FOCTYPE.__ADDITIONAL) == type)
				{
					p2Foc = p2Foc2;
					break;
				}
				if (p2Foc2.isUseless() && p2Foc2.foctype == type)
				{
					p2Foc = p2Foc2;
					break;
				}
			}
			if (p2Foc == null)
			{
				p2Foc = new M2Phys.P2Foc();
				this.AFoc.Add(p2Foc);
			}
			p2Foc.velocity_x = velocity_x;
			p2Foc.velocity_y = velocity_y;
			p2Foc.t = (p2Foc.t_fric = 0f);
			p2Foc.foctype = type;
			p2Foc.max = max_abs;
			p2Foc.t_attack = (short)((t_attack < 0) ? (-1) : t_attack);
			p2Foc.t_hold = (short)t_hold;
			p2Foc.t_release = (short)t_release;
			p2Foc.maxt = ((t_attack < 0) ? 1 : (p2Foc.t_attack + p2Foc.t_hold + p2Foc.t_release));
			p2Foc.fric_time = (short)fric_time;
			p2Foc.fric_ignore = (short)fric_ignore;
			return this;
		}

		public bool isFlying()
		{
			return this.Mv.vy - this.pre_adjust_vy < -0.00390625f;
		}

		public M2Phys remFocExactly(FOCTYPE type)
		{
			for (int i = this.AFoc.Count - 1; i >= 0; i--)
			{
				M2Phys.P2Foc p2Foc = this.AFoc[i];
				if (p2Foc.foctype == type)
				{
					p2Foc.t = (float)(p2Foc.maxt = 0);
				}
			}
			return this;
		}

		public M2Phys remFoc(FOCTYPE type, bool check_or = true)
		{
			for (int i = this.AFoc.Count - 1; i >= 0; i--)
			{
				M2Phys.P2Foc p2Foc = this.AFoc[i];
				if (check_or ? ((p2Foc.foctype & type) > (FOCTYPE)0U) : ((p2Foc.foctype & type) == type))
				{
					p2Foc.t = (float)(p2Foc.maxt = 0);
				}
			}
			return this;
		}

		public M2Phys remFocForAim(FOCTYPE type, AIM a)
		{
			int num = CAim._XD(a, 1);
			int num2 = -CAim._YD(a, 1);
			for (int i = this.AFoc.Count - 1; i >= 0; i--)
			{
				M2Phys.P2Foc p2Foc = this.AFoc[i];
				if ((p2Foc.foctype & type) == (FOCTYPE)0U && p2Foc.maxt > 0 && ((num > 0 && p2Foc.velocity_x > 0f) || (num2 > 0 && p2Foc.velocity_y > 0f) || (num < 0 && p2Foc.velocity_x < 0f) || (num2 < 0 && p2Foc.velocity_y < 0f)))
				{
					p2Foc.t = (float)(p2Foc.maxt = 0);
				}
			}
			return this;
		}

		public M2Phys reverseSpeed(bool _x, bool _y)
		{
			for (int i = this.AFoc.Count - 1; i >= 0; i--)
			{
				M2Phys.P2Foc p2Foc = this.AFoc[i];
				if (_x)
				{
					p2Foc.velocity_x *= -1f;
				}
				if (_y)
				{
					p2Foc.velocity_y *= -1f;
				}
			}
			return this;
		}

		public float calcFocVelocityX(FOCTYPE type, bool check_or = false)
		{
			return this.calcFocVelocity(type, check_or, false).x;
		}

		public Vector2 calcFocVelocity(FOCTYPE type, bool check_or = false, bool check_not = false)
		{
			Vector2 zero = Vector2.zero;
			for (int i = this.AFoc.Count - 1; i >= 0; i--)
			{
				M2Phys.P2Foc p2Foc = this.AFoc[i];
				if (p2Foc.maxt > 0 && (check_or ? ((p2Foc.foctype & type) > (FOCTYPE)0U) : ((p2Foc.foctype & type) == type)) != check_not && p2Foc.maxt > 0)
				{
					if (p2Foc.velocity_x > 0f)
					{
						if (p2Foc.max < 0f)
						{
							zero.x = X.Mn(-p2Foc.max * p2Foc.velocity_x, zero.x + p2Foc.velocity_x);
						}
						else if (p2Foc.max > 0f)
						{
							zero.x = X.Mn(p2Foc.max, zero.x + p2Foc.velocity_x);
						}
						else
						{
							zero.x += p2Foc.velocity_x;
						}
					}
					else if (p2Foc.velocity_x < 0f)
					{
						if (p2Foc.max < 0f)
						{
							zero.x = X.Mx(-p2Foc.max * p2Foc.velocity_x, zero.x + p2Foc.velocity_x);
						}
						else if (p2Foc.max > 0f)
						{
							zero.x = X.Mx(-p2Foc.max, zero.x + p2Foc.velocity_x);
						}
						else
						{
							zero.x += p2Foc.velocity_x;
						}
					}
					if (p2Foc.velocity_y > 0f)
					{
						if (p2Foc.max < 0f)
						{
							zero.y = X.Mn(-p2Foc.max * p2Foc.velocity_y, zero.y + p2Foc.velocity_y);
						}
						else if (p2Foc.max > 0f)
						{
							zero.y = X.Mn(p2Foc.max, zero.y + p2Foc.velocity_y);
						}
						else
						{
							zero.y += p2Foc.velocity_y;
						}
					}
					else if (p2Foc.velocity_y < 0f)
					{
						if (p2Foc.max < 0f)
						{
							zero.y = X.Mx(-p2Foc.max * p2Foc.velocity_y, zero.y + p2Foc.velocity_y);
						}
						else if (p2Foc.max > 0f)
						{
							zero.y = X.Mx(-p2Foc.max, zero.y + p2Foc.velocity_y);
						}
						else
						{
							zero.y += p2Foc.velocity_y;
						}
					}
				}
			}
			return zero;
		}

		public void clampSpeed(FOCTYPE type, float max_abs_x = -1f, float max_abs_y = -1f, float clip_ratio = 1f)
		{
			if (max_abs_x >= 0f)
			{
				this.clampvx_r = max_abs_x;
				this.clampvx_l = max_abs_x;
			}
			if (max_abs_y >= 0f)
			{
				this.clampvy_b = max_abs_y;
				this.clampvy_t = max_abs_y;
			}
		}

		public M2Phys addLockGravityFrame(int t)
		{
			this.frame_gravity_lock = (byte)X.Mx(t, (int)this.frame_gravity_lock);
			return this;
		}

		public bool addableGravity()
		{
			return this.frame_gravity_lock <= 0;
		}

		public byte getLockGravityFrame()
		{
			return this.frame_gravity_lock;
		}

		public M2Phys addLockGravity(object Key, float dep_level, float t)
		{
			this.LockGravity.Merge(Key, 1f - dep_level, t, null);
			if (this.current_gravity_scale_ != -1f && dep_level < this.current_gravity_scale_)
			{
				this.current_gravity_scale_ = X.Mn(dep_level, this.current_gravity_scale_);
			}
			return this;
		}

		public M2Phys addLockWallHitting(object Key, float t = 120f)
		{
			bool flag = this.LockWallHitting.Count > 0;
			this.LockWallHitting.Add(Key, t);
			if (!flag)
			{
				this.Mv.fineHittingLayer();
			}
			return this;
		}

		public M2Phys addLockMoverHitting(HITLOCK hittype, float t = -1f)
		{
			bool flag = this.LockMoverHitting.Count > 0;
			this.LockMoverHitting.Add((int)hittype, t);
			if (!flag)
			{
				this.Mv.fineHittingLayer();
			}
			return this;
		}

		public bool hasLockMoverHitting(HITLOCK hittype)
		{
			return this.LockMoverHitting.Has((int)hittype);
		}

		public bool hasCollirderLock(Collider2D Cld)
		{
			return this.LockBlockHitting.Has(Cld);
		}

		public M2Phys addCollirderLock(Collider2D Cld, float lock_time = 2f, FloatCounter<Collider2D>.FnNoReduce _FnNoReduce = null, float level = 1f)
		{
			if (Cld == null)
			{
				return this;
			}
			if (level <= 0.5f)
			{
				M2Phys m2Phys;
				if (this.Mp.Gob2Phys(Cld.gameObject, out m2Phys))
				{
					if (m2Phys.MyBCC != null && this.CheckBCC.NotHas(m2Phys.MyBCC))
					{
						return this;
					}
				}
				else if (Cld.gameObject == this.Mp.gameObject && this.CheckBCC.NotHas(this.Mp.BCC))
				{
					return this;
				}
			}
			if (this.LockBlockHitting.Merge(Cld, level, lock_time, _FnNoReduce))
			{
				Collider2D myCollider = this.MyCollider;
				if (myCollider != null)
				{
					Physics2D.IgnoreCollision(Cld, myCollider, true);
				}
			}
			return this;
		}

		public M2Phys addBCCCheck(M2BlockColliderContainer TargetBCC, float lock_time = 2f)
		{
			if (TargetBCC == null)
			{
				return this;
			}
			this.CheckBCC.Merge(TargetBCC, 1f, lock_time, null);
			return this;
		}

		public M2Phys remBCCCheck(M2BlockColliderContainer TargetBCC)
		{
			if (TargetBCC == null)
			{
				return this;
			}
			this.CheckBCC.Rem(TargetBCC);
			return this;
		}

		private void fineDeletedBlockHitting()
		{
			Collider2D myCollider = this.MyCollider;
			if (myCollider != null)
			{
				for (int i = this.LockBlockHitting.ADeleted.Count - 1; i >= 0; i--)
				{
					Collider2D collider2D = this.LockBlockHitting.ADeleted[i];
					if (collider2D != null)
					{
						Physics2D.IgnoreCollision(myCollider, collider2D, false);
					}
				}
			}
			this.LockBlockHitting.ADeleted.Clear();
		}

		public M2Phys remLockMoverHitting(HITLOCK hittype)
		{
			bool flag = this.LockMoverHitting.Count > 0;
			this.LockMoverHitting.Rem((int)hittype);
			if (flag && this.LockMoverHitting.Count == 0)
			{
				this.Mv.fineHittingLayer();
			}
			return this;
		}

		public M2Phys remLockWallHitting(object Key)
		{
			bool flag = this.LockWallHitting.Count > 0;
			this.LockWallHitting.Rem(Key);
			if (flag && this.LockWallHitting.Count == 0)
			{
				this.Mv.moveToStandablePoint(this.x, this.y);
				this.Mv.fineHittingLayer();
			}
			return this;
		}

		public M2Phys remLockGravity(object Key0)
		{
			this.LockGravity.Rem(Key0);
			this.current_gravity_scale_ = -1f;
			this.fineGravityScale();
			return this;
		}

		public M2Phys remColliderLock(Collider2D Cld)
		{
			this.LockBlockHitting.Rem(Cld);
			this.fineDeletedBlockHitting();
			return this;
		}

		public M2Phys clearLockGravity()
		{
			this.LockGravity.Clear(false);
			this.frame_gravity_lock = 0;
			this.current_gravity_scale_ = -1f;
			this.fineGravityScale();
			return this;
		}

		public M2Phys clearColliderLock()
		{
			this.LockBlockHitting.Clear(false);
			this.fineDeletedBlockHitting();
			return this;
		}

		public M2Phys killSpeedForce(bool x_flag = true, bool y_flag = true, bool apply_to_mv = true, bool use_clamp = false, bool kill_phy_translate_stack = false)
		{
			if (x_flag)
			{
				this.force_velocity_x = 0f;
				if (use_clamp)
				{
					this.clampvx_l = (this.clampvx_r = 0f);
				}
				this.pre_release_vel_x = 0f;
				if (kill_phy_translate_stack)
				{
					this.translate_stack_x = 0f;
				}
			}
			if (y_flag)
			{
				this.force_velocity_y = (this.v_gravity = 0f);
				this.pre_release_vel_y = 0f;
				if (use_clamp)
				{
					this.clampvy_t = (this.clampvy_b = 0f);
				}
				this.quitSoftFall(0f);
				if (kill_phy_translate_stack)
				{
					this.translate_stack_y = 0f;
				}
			}
			this.AFoc.Clear();
			if (this.Rgd != null)
			{
				Vector2 velocity = this.Rgd.velocity;
				if (x_flag)
				{
					velocity.x = 0f;
				}
				if (y_flag)
				{
					velocity.y = 0f;
				}
				this.Rgd.velocity = velocity;
			}
			if (apply_to_mv)
			{
				this.Mv.killSpeedFromPhysicsAbsolutely(x_flag, y_flag);
			}
			return this;
		}

		public bool checkExistsForce(bool left = true, bool top = true, bool right = true, bool bottom = true)
		{
			for (int i = this.AFoc.Count - 1; i >= 0; i--)
			{
				M2Phys.P2Foc p2Foc = this.AFoc[i];
				if (!p2Foc.isUseless() && ((left && p2Foc.velocity_x < 0f) || (right && p2Foc.velocity_x < 0f) || (top && p2Foc.velocity_y < 0f) || (bottom && p2Foc.velocity_y < 0f)))
				{
					return true;
				}
			}
			return false;
		}

		public void OnCollisionEnter2D(Collision2D col)
		{
			string tag = this.Mp.getTag(col.gameObject);
			if (this.Rgd != null)
			{
				if (tag == "Ground")
				{
					this.addFric(10f);
					this.fineWallStuck(false);
					if (this.FootD != null && this.FootD.get_FootBCC() != null && this.FootD.get_FootBCC().BCC != this.Mp.BCC)
					{
						this.FootD.lockFootFix(3);
					}
				}
				else
				{
					this.hit_mover_friction_ = true;
				}
				if (tag == "Block")
				{
					this.addFric(10f);
					this.hit_wall_collider = true;
					if (this.FootD != null && this.FootD.get_FootBCC() != null && this.FootD.get_FootBCC().BCC == this.Mp.BCC)
					{
						this.FootD.lockFootFix(3);
					}
					if (this.LockBlockHitting.NotHas(col.collider) && this.LockBlockHitting.clipByLevel(0.5f))
					{
						this.fineDeletedBlockHitting();
					}
				}
				else if (tag.IndexOf("Mover") == 0)
				{
					this.addLockMoverHitting(HITLOCK.MOVER_ENTER, 28f);
					this.addFric(10f);
				}
			}
			if (this.sound != "" && tag != "Mover" && col.gameObject != this.pre_sound_hit_Gob)
			{
				this.pre_sound_hit_Gob = col.gameObject;
				if (this.snd_t == 0f)
				{
					this.Mv.playSndPos(this.sound, 1);
				}
				this.snd_t = 20f;
			}
		}

		public void fineWallStuck(bool soft = false)
		{
			if (!this.unity_physics_mode_ && !this.Mv.floating)
			{
				this.wall_stuck_count.Add((this.wall_stuck_count >= 48f) ? X.Mx(48f - this.wall_stuck_count.Get(), 0f) : 1f, false, soft);
			}
		}

		public bool recheckFoot(float pre_fall_y = 0f)
		{
			if (this.FootD != null)
			{
				if (this.FootD.canFootOnCurrentFoot())
				{
					return true;
				}
				if (this.Mv.checkFootObject(X.MMX(0f, pre_fall_y, 0.5f)) != null)
				{
					return true;
				}
			}
			return false;
		}

		public bool canConsiderCheckHit()
		{
			return (this.LockMoverHitting.Count == 0 || !this.LockMoverHitting.NotHas(1)) && this.hit_mover < this.hit_mover_threshold;
		}

		public bool checkHitTo(M2Phys TPy)
		{
			if (!TPy.canConsiderCheckHit())
			{
				return false;
			}
			if ((TPy.hasFoot() && TPy.FootD.FootIs(this.Mv)) || (this.hasFoot() && this.FootD.FootIs(TPy.Mv)))
			{
				return false;
			}
			if (!X.isCovering(this.mleft, this.mright, TPy.mleft, TPy.mright, 0f) || !X.isCovering(this.mtop, this.mbottom, TPy.mtop, TPy.mbottom, 0f) || this.Mv.cannotHitTo(TPy.Mv) || TPy.Mv.cannotHitTo(this.Mv))
			{
				return false;
			}
			if (this.Mv.weight < 0f && TPy.Mv.weight < 0f)
			{
				return false;
			}
			float num = this.mass + (float)(this.hit_mover * this.hit_mover / 2) * TPy.mass;
			float num2 = TPy.mass + (float)(TPy.hit_mover * TPy.hit_mover / 2) * this.mass;
			float num3 = 1f / (num + num2);
			bool flag = !this.Mv.export_other_mover_right(TPy.Mv);
			float num4 = (float)X.MPF(flag) * num2 * num3 * 0.13f;
			float num5 = (float)(-(float)X.MPF(flag)) * num * num3 * 0.13f;
			if (TPy.hasFoot() && !this.hasFoot())
			{
				float num6 = X.ZSIN(X.LENGTHXYS(TPy.x, TPy.y, this.x, this.y), 0.01f + TPy.Mv.sizex + TPy.Mv.sizey);
				num4 *= num6;
			}
			if (!TPy.hasFoot() && this.hasFoot())
			{
				float num7 = X.ZSIN(X.LENGTHXYS(TPy.x, TPy.y, this.x, this.y), 0.01f + this.Mv.sizex + this.Mv.sizey);
				num5 *= num7;
			}
			float num8 = X.MMX(0f, flag ? (TPy.mright - this.mleft) : (this.mright - TPy.mleft), 1.5f);
			float num9 = 0f;
			num5 *= num8;
			num4 *= num8;
			TPy.Mv.getMovableLen(ref num5, ref num9, -0.1f, false, false);
			float num10 = num4;
			this.Mv.getMovableLen(ref num4, ref num9, -0.1f, false, false);
			num5 -= num10 - num4;
			this.hit_mover++;
			TPy.hit_mover++;
			this.Mv.moveByHitCheck(TPy, FOCTYPE.HIT, num4, 0f);
			TPy.Mv.moveByHitCheck(this, FOCTYPE.HIT, num5, 0f);
			return true;
		}

		public void moveByHitCheck(FOCTYPE foctype, float map_dx, float map_dy)
		{
			this.addFoc(foctype, map_dx * 0.125f, map_dy * 0.125f, -1000f, 0, 0, 4, -1, 0);
		}

		public bool setWaterDunk(int gen_id, int misttype)
		{
			if (this.isin_water_t < 8)
			{
				if (gen_id != this.isin_water_id_ && this.Mv is M2Attackable && !(this.Mv as M2Attackable).setWaterDunk(this.isin_water_id_, misttype))
				{
					return false;
				}
				if (gen_id > this.isin_water_id_)
				{
					this.isin_water_id_ = gen_id;
					this.clampSpeed((FOCTYPE)4294967295U, X.Abs(this.Mv.vx * this.water_speed_scale), X.Abs(this.Mv.vy * this.water_speed_scale), 1f);
				}
				this.isin_water_t = 8;
			}
			return true;
		}

		public void releaseWaterDunk()
		{
			if (this.isin_water_t > 0)
			{
				this.isin_water_t = 0;
				this.isin_water_id_ = -1;
			}
		}

		public int moveWithFoot(float dx, float dy, Collider2D _Cld, M2BlockColliderContainer BCCCarrier, ref float vx, ref float vy, bool no_collider_lock = false)
		{
			if (this.FootD == null)
			{
				return 0;
			}
			this.addBCCCheck(BCCCarrier, 60f);
			if (!no_collider_lock)
			{
				this.addCollirderLock(_Cld, 2f, null, (BCCCarrier == null) ? 1f : 0.5f);
			}
			this.translate_stack_x += dx * Map2d.TS;
			this.translate_stack_y += dy * Map2d.TS;
			return 1;
		}

		private void fineSpeedValue(Vector2 Spd, ref float vx_, ref float vy_)
		{
			vx_ = this.Mp.uVelocityxToMapx(Spd.x * this.preTS_rev);
			float num = this.Mp.uVelocityyToMapy(Spd.y * this.preTS_rev);
			if (this.always_rewrite_velocity)
			{
				vy_ = num;
				return;
			}
			if (num != vy_)
			{
				if (vy_ >= 0f && num >= 0f)
				{
					if (num < vy_)
					{
						this.v_gravity = X.Mx(this.v_gravity + num - vy_, 0f);
						vy_ = num;
						return;
					}
				}
				else if (vy_ < 0f)
				{
				}
			}
		}

		public float rgd_agR
		{
			get
			{
				return this.rgd_agR_;
			}
			set
			{
				if (this.rgd_agR == value)
				{
					return;
				}
				this.rgd_agR_ = value;
				this.rgd_cos_ = X.Abs(X.Cos(this.rgd_agR_));
				this.rgd_sin_ = X.Abs(X.Sin(this.rgd_agR_));
				this.Mv.transform.localEulerAngles = new Vector3(0f, 0f, value * 0.31830987f * 180f);
			}
		}

		private void fixRigidBodySpeed(float vx_, float vy_)
		{
			if (this.Rgd != null)
			{
				this.Rgd.velocity = new Vector2(this.Mp.mapvxToUVelocityx(vx_ * this.TS), this.Mp.mapvyToUVelocityy(vy_ * this.TS));
				this.preTS_rev = 1f / this.TS;
				return;
			}
			this.translate_stack_x += vx_ * this.TS;
			this.translate_stack_y += vy_ * this.TS;
		}

		public bool canJump()
		{
			return this.Mv.canJump();
		}

		public bool hasFoot()
		{
			return this.Mv.hasFoot();
		}

		public Rigidbody2D getRigidbody()
		{
			return this.Rgd;
		}

		public float base_gravity
		{
			get
			{
				return this.base_gravity_;
			}
			set
			{
				if (this.base_gravity_ == value)
				{
					return;
				}
				if (this.FootD != null && this.base_gravity_ == 0f && value > 0f)
				{
					this.FootD.initJump(false, false, false);
				}
				this.base_gravity_ = value;
				this.fineGravityScale();
			}
		}

		public float gravity_apply_velocity(float fcnt)
		{
			this.fineGravityScale();
			return M2Phys.getGravityApplyVelocity(this.Mp, this.current_gravity_scale_ * this.base_gravity_, fcnt);
		}

		public static float getGravityApplyVelocity(Map2d Mp, float f, float fcnt)
		{
			return Mp.uVelocityyToMapy(f * Physics2D.gravity.y) / 50f * fcnt;
		}

		public string default_layer_name
		{
			get
			{
				return LayerMask.LayerToName(this.default_layer_);
			}
			set
			{
				int num = LayerMask.NameToLayer(value);
				if (num != this.default_layer_)
				{
					this.default_layer_ = num;
					this.Mv.fineHittingLayer();
				}
			}
		}

		public int default_layer
		{
			get
			{
				return this.default_layer_;
			}
			set
			{
				if (value != this.default_layer_)
				{
					this.default_layer_ = value;
					this.Mv.fineHittingLayer();
				}
			}
		}

		public bool isLockWallHittingActive()
		{
			return this.LockWallHitting.Count > 0;
		}

		public bool isLockMoverHittingActive()
		{
			return this.LockMoverHitting.Count > 0;
		}

		public M2Phys addFric(float t = 5f)
		{
			this.fric_t = X.Mx(t, this.fric_t);
			return this;
		}

		public M2Phys clearFric()
		{
			this.fric_t = 0f;
			return this;
		}

		public bool unity_physics_mode
		{
			get
			{
				return this.unity_physics_mode_;
			}
			set
			{
				this.unity_physics_mode_ = value;
				if (!this.is_pause && this.Rgd != null)
				{
					this.Rgd.isKinematic = false;
				}
			}
		}

		public float current_ySpeedMax
		{
			get
			{
				return this.ySpeedMax * ((this.softfall_maxt <= 0f) ? this.softfall_dep_ratio : X.NI(this.softfall_start_ratio, this.softfall_dep_ratio, X.ZLINE(this.softfall_t, this.softfall_maxt)));
			}
		}

		public bool releaseVelocity(FOCTYPE type, bool is_y)
		{
			return (type & (FOCTYPE.DAMAGE | FOCTYPE._RELEASE)) > (FOCTYPE)0U;
		}

		public bool isPausing()
		{
			return this.is_pause;
		}

		public float angleSpeedR
		{
			get
			{
				if (!(this.Rgd == null))
				{
					return this.Rgd.angularVelocity * 3.1415927f / 180f / 60f;
				}
				return 0f;
			}
			set
			{
				if (this.Rgd != null)
				{
					this.Rgd.angularVelocity = value / 3.1415927f * 180f * 60f;
				}
			}
		}

		public float mass
		{
			get
			{
				if (!(this.Rgd == null))
				{
					return this.Rgd.mass;
				}
				return -1f;
			}
			set
			{
				if (this.Rgd != null)
				{
					this.Rgd.mass = value;
				}
			}
		}

		public bool freezeRotation
		{
			get
			{
				return this.Rgd.freezeRotation;
			}
			set
			{
				this.Rgd.freezeRotation = value;
			}
		}

		public PhysicsMaterial2D sharedMaterial
		{
			get
			{
				if (!(this.Rgd != null))
				{
					return null;
				}
				return this.Rgd.sharedMaterial;
			}
			set
			{
				if (this.Rgd != null)
				{
					this.Rgd.sharedMaterial = value;
				}
			}
		}

		public GameObject gameObject
		{
			get
			{
				return this.Mv.gameObject;
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.Mv.Mp;
			}
		}

		public M2FootManager getFootManager()
		{
			return this.FootD;
		}

		public float CLEN
		{
			get
			{
				return this.Mv.CLEN;
			}
		}

		public float CLENM
		{
			get
			{
				return this.Mv.CLENM;
			}
		}

		public float mleft
		{
			get
			{
				return this.Mv.mleft;
			}
		}

		public float mright
		{
			get
			{
				return this.Mv.mright;
			}
		}

		public float rgdleft
		{
			get
			{
				return this.Mv.x - this.rgd_cos_ * this.Mv.sizex - this.rgd_sin_ * this.Mv.sizey;
			}
		}

		public float rgdright
		{
			get
			{
				return this.Mv.x + this.rgd_cos_ * this.Mv.sizex + this.rgd_sin_ * this.Mv.sizey;
			}
		}

		public float rgdtop
		{
			get
			{
				return this.Mv.y - this.rgd_cos_ * this.Mv.sizey - this.rgd_sin_ * this.Mv.sizex;
			}
		}

		public float rgdbottom
		{
			get
			{
				return this.Mv.y + this.rgd_cos_ * this.Mv.sizey + this.rgd_sin_ * this.Mv.sizex;
			}
		}

		public float gravity_added_velocity
		{
			get
			{
				return this.v_gravity;
			}
		}

		public float x
		{
			get
			{
				return this.Mv.x;
			}
		}

		public float y
		{
			get
			{
				return this.Mv.y;
			}
		}

		public float move_depert_x
		{
			get
			{
				return this.Mv.x + this.force_velocity_x;
			}
		}

		public float move_depert_y
		{
			get
			{
				return this.Mv.y + this.force_velocity_y + this.v_gravity;
			}
		}

		public float tstacked_x
		{
			get
			{
				return this.Mv.x + this.translate_stack_x;
			}
		}

		public float tstacked_y
		{
			get
			{
				return this.Mv.y + this.translate_stack_y;
			}
		}

		public float release_velocity_x
		{
			get
			{
				if (this.Mv.vx > 0f)
				{
					return X.Mx(this.Mv.vx - this.force_velocity_x, 0f);
				}
				if (this.Mv.vx < 0f)
				{
					return X.Mn(this.Mv.vx - this.force_velocity_x, 0f);
				}
				return 0f;
			}
		}

		public float move_depert_tstack_x
		{
			get
			{
				return this.Mv.x + this.force_velocity_x * Map2d.TS + this.translate_stack_x;
			}
		}

		public float move_depert_tstack_x_w
		{
			get
			{
				return this.Mv.x + this.force_velocity_x * Map2d.TS + this.translate_stack_x + this.walk_xspeed_;
			}
		}

		public float move_depert_tstack_y
		{
			get
			{
				return this.Mv.y + (this.force_velocity_y + this.v_gravity) * Map2d.TS + this.translate_stack_y;
			}
		}

		public float pre_force_velocity_x
		{
			get
			{
				return this.force_velocity_x;
			}
		}

		public float pre_force_velocity_y
		{
			get
			{
				return this.force_velocity_y;
			}
		}

		public float force_velocity_y_with_gravity
		{
			get
			{
				return this.force_velocity_y + ((this.frame_gravity_lock == 0 && this.fineGravityScale() > 0f) ? this.v_gravity : 0f);
			}
		}

		public float TS
		{
			get
			{
				return this.Mv.TS;
			}
		}

		public AIM aim
		{
			get
			{
				return this.Mv.aim;
			}
		}

		public float mtop
		{
			get
			{
				return this.Mv.mtop;
			}
		}

		public float mbottom
		{
			get
			{
				return this.Mv.mbottom;
			}
		}

		public float footbottom
		{
			get
			{
				return this.Mv.footbottom;
			}
		}

		public bool hit_wall_collider
		{
			get
			{
				return this.Mv.hit_wall_collider;
			}
			set
			{
				if (value)
				{
					this.Mv.hit_wall_collider = true;
				}
			}
		}

		public bool hit_mover_friction
		{
			get
			{
				return this.hit_mover_friction_;
			}
			set
			{
				if (value)
				{
					this.hit_mover_friction_ = true;
				}
			}
		}

		public int walk_auto_assisting
		{
			get
			{
				return this.Mv.walk_auto_assisting;
			}
			set
			{
				this.Mv.walk_auto_assisting = value;
			}
		}

		public int hit_mover_threshold
		{
			get
			{
				return this.hit_mover_threshold_;
			}
			set
			{
				this.hit_mover_threshold_ = X.Mx(this.hit_mover_threshold_, value);
			}
		}

		public Vector2 releasedVelocity
		{
			get
			{
				float num = X.VALWALK(this.Mv.vx, X.Abs(this.force_velocity_x), 0f);
				float num2 = X.VALWALK(this.Mv.vy, X.Abs(this.force_velocity_y), 0f);
				return new Vector2(num, num2);
			}
		}

		public Collider2D MyCollider
		{
			get
			{
				if (this.MyCollider_ == null)
				{
					this.MyCollider_ = this.gameObject.GetComponent<Collider2D>();
				}
				return this.MyCollider_;
			}
		}

		public bool isin_water
		{
			get
			{
				return this.isin_water_t > 0;
			}
		}

		public override string ToString()
		{
			return "<Phys>-" + this.Mv.ToString();
		}

		public bool pre_position_moved
		{
			get
			{
				return this.x != this.prex || this.y != this.prey;
			}
		}

		public bool get_is_pause()
		{
			return this.is_pause;
		}

		public bool is_stucking
		{
			get
			{
				return this.wall_stuck_count >= 40f;
			}
		}

		public static bool fixed_updating;

		public readonly M2Mover Mv;

		private List<M2Phys.P2Foc> AFoc;

		public float maxt = -1f;

		public float fadet = 30f;

		public string sound = "";

		private float force_velocity_x;

		private float force_velocity_y;

		private float pre_release_vel_x;

		private float pre_release_vel_y;

		private M2BlockColliderContainer.BCCLine PreStackBCC;

		public float fric_reduce_x = 0.04f;

		private int default_layer_;

		public float ySpeedMax = 0.38f;

		public float clampvx_l = -1f;

		public float clampvx_r = -1f;

		public float clampvy_t = -1f;

		public float clampvy_b = -1f;

		public bool no_play_footsnd_animator;

		protected float base_gravity_;

		protected float softfall_dep_ratio = 1f;

		protected float softfall_start_ratio = 1f;

		protected float softfall_maxt;

		protected float softfall_t;

		public float water_speed_scale = 0.3333f;

		public bool always_rewrite_velocity;

		public bool carrying_no_collider_lock;

		protected float snd_t;

		private GameObject pre_sound_hit_Gob;

		public int hit_mover;

		private int hit_mover_threshold_ = 1;

		private float v_gravity;

		public FOCTYPE pre_x_attached;

		public FOCTYPE pre_y_attached;

		private float fric_t;

		private byte frame_gravity_lock;

		private int cfg_pre_slope;

		private int pre_slope_mapx = -1;

		public float pre_adjust_vy;

		public short main_updated_count;

		public bool auto_air_mvhitting;

		public bool damage_foc_decline_when_move_script_attached;

		protected M2FootManager FootD;

		protected Rigidbody2D Rgd;

		public float rgd_agR_;

		public float rgd_cos_ = 1f;

		public float rgd_sin_;

		private FloatCounter<object> LockGravity;

		private FloatCounter<M2BlockColliderContainer> CheckBCC;

		private FlagCounterR<object> LockWallHitting;

		private FloatCounter<Collider2D> LockBlockHitting;

		private FlagCounterR<int> LockMoverHitting;

		private float current_gravity_scale_ = -1f;

		public M2Phys.FnHittingVelocity FnHitting;

		public M2BlockColliderContainer MyBCC;

		private bool hit_mover_friction_;

		private bool unity_physics_mode_;

		private bool is_pause = true;

		private byte isin_water_t;

		private int isin_water_id_ = -1;

		public float prex;

		public float prey;

		private float preTS_rev = 1f;

		private Collider2D MyCollider_;

		private PauseMemItemRigidbody PauseMemory;

		private float walk_xspeed_;

		public float walk_xspeed_manageable_air = -1f;

		private bool resized_y;

		public float translate_stack_x;

		public float translate_stack_y;

		private RevCounter wall_stuck_count;

		public const float gravity_multiply = 50f;

		private const float STACK_COUNT_MAX = 40f;

		public uint corner_slip_alloc_bits = 15U;

		private bool temporary_no_clear_v_gravity;

		private static Func<M2Mover, Rigidbody2D, M2Phys> FD_createPhys_;

		private static bool _immediate_translate;

		public delegate bool FnHittingVelocity(M2Phys P, FOCTYPE type, ref float velocity_x, ref float velocity_y);

		protected class P2Foc
		{
			public bool isActive()
			{
				return this.velocity_x != 0f || this.velocity_y != 0f;
			}

			public bool isUseless()
			{
				return !this.isActive() || this.t >= (float)this.maxt;
			}

			public void deactivate()
			{
				this.t_attack = (this.t_hold = (this.t_release = (this.maxt = 0)));
			}

			public FOCTYPE foctype;

			public float velocity_x;

			public float velocity_y;

			public float max;

			public float t;

			public short t_attack;

			public short t_hold;

			public short t_release;

			public short maxt;

			public float t_fric;

			public short fric_time;

			public short fric_ignore;
		}
	}
}
