using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2BoxMover : ChipMover, IBCCEventListener, IBCCCarriedMover
	{
		public override void appear(Map2d Mp)
		{
			this.FD_ThroughablePixel = new Func<int, int, M2BlockColliderContainer.BCCLine, bool>(this.ThroughablePixel);
			this.FD_ThroughablePixelWithSlide = new Func<int, int, M2BlockColliderContainer.BCCLine, bool>(this.ThroughablePixelWithSlide);
			this.FD_FnHitReturnableBcc = new Func<M2BlockColliderContainer.BCCLine, Vector3, bool>(this.FnHitReturnableBcc);
			Rigidbody2D orAdd = IN.GetOrAdd<Rigidbody2D>(base.gameObject);
			this.floating = true;
			base.appear(Mp);
			if (M2BoxMover.HitBcc == null)
			{
				M2BoxMover.HitBcc = new List<M2BlockColliderContainer.BCCLine>(4);
				M2BoxMover.AHamePt = new List<M2Puts>(1);
				M2BoxMover.ASlideChecking = new List<M2BlockColliderContainer.BCCLine>(4);
				M2BoxMover.Aput_buf = new List<uint>(8);
			}
			if (this.Phy == null)
			{
				this.Phy = new M2Phys(this, orAdd);
			}
			this.fineWeight();
			base.base_gravity = 0.4f;
			this.floating = false;
			this.Afooted_t = new float[4];
			this.AFootedLine = new List<M2BlockColliderContainer.BCCLine>[4];
			this.Asoft_fix_pos = new float[4];
			for (int i = 0; i < 4; i++)
			{
				this.AFootedLine[i] = new List<M2BlockColliderContainer.BCCLine>(2);
			}
			this.ASlideDepert = new List<Vector4>(2);
			this.t_footstamp_lock = 70f;
			this.Apos_mem = new Vector3[5];
			this.fineCarryable();
		}

		public void fineWeight()
		{
			this.Phy.mass = (this.beingCarriedByPr() ? (this.isPuttingByPr() ? 10f : 0.5f) : 4f);
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			if (this.FollowTo != null)
			{
				this.deactivate(true);
				this.deactivateCompletely();
			}
			base.destruct();
		}

		public void attractedAfter()
		{
			base.initTransEffecter();
			base.gameObject.layer = IN.LAY("ChipsUCol");
			this.Mp.setTag(base.gameObject, "ChipsUColBox");
			this.parent_shift_x = this.Parent.mapcx - base.x;
			this.parent_shift_y = this.Parent.mapcy - base.y;
			if (this.CC is M2MvColliderCreatorCM)
			{
				this.MyBCC = (this.CC as M2MvColliderCreatorCM).BCC;
			}
			if (this.MyBCC != null)
			{
				M2BlockColliderContainer.BCCIterator bcciterator;
				this.MyBCC.ItrInit(out bcciterator, true);
				while (bcciterator.Next())
				{
					bcciterator.Cur.is_lift = true;
				}
			}
		}

		public override void fineHittingLayer()
		{
			base.gameObject.layer = (this.hameDecided() ? 2 : IN.LAY("ChipsUCol"));
		}

		public bool footStampBox(M2BlockColliderContainer.BCCLine _Bcc, bool reverse = false)
		{
			int num = (int)_Bcc.aim;
			if (num - 4 > 1)
			{
				if (num - 6 <= 1)
				{
					num = 1;
				}
				else
				{
					num = (int)CAim.get_opposite((AIM)num);
				}
			}
			else
			{
				num = 3;
			}
			List<M2BlockColliderContainer.BCCLine> list = this.AFootedLine[num];
			if (this.Afooted_t[num] == 0f)
			{
				list.Clear();
			}
			if (list.IndexOf(_Bcc) >= 0)
			{
				return true;
			}
			if (reverse)
			{
				this.Afooted_t[num] = -1f;
				this.playfoot_bits |= 1U << num;
			}
			else
			{
				list.Add(_Bcc);
				if (this.t_footstamp_lock == 0f && this.Afooted_t[num] == 0f)
				{
					this.PtcFootStamp(_Bcc);
				}
				this.Afooted_t[num] = (this.beingCarriedByPr() ? 30f : X.Mn(4f, this.Afooted_t[num]));
			}
			if (num == 3 && this.v_gravity != 0f)
			{
				this.v_gravity = 0f;
				this.fineCarryable();
			}
			return true;
		}

		private void PtcFootStamp(M2BlockColliderContainer.BCCLine _Bcc)
		{
			this.Mp.PtcSTsetVar("cx", (double)_Bcc.shifted_cx).PtcSTsetVar("cy", (double)_Bcc.shifted_cy).PtcSTsetVar("agR", (double)_Bcc.sd_agR)
				.PtcST("puzz_ground_bump", null, PTCThread.StFollow.NO_FOLLOW);
		}

		public override M2Mover setTo(float depx, float depy)
		{
			base.setTo(depx, depy);
			if (this.Afooted_t != null)
			{
				X.ALL0(this.Asoft_fix_pos);
				this.resetEventPosition(null, false);
				this.recheckAim(AIM.B, false, 1).recheckAim(AIM.R, false, 1).recheckAim(AIM.L, false, 1)
					.recheckAim(AIM.T, false, 1);
				this.t_footstamp_lock = 10f;
			}
			return this;
		}

		public void fineCarryable()
		{
			base.carryable_other_object = !this.hameDecided() && this.v_gravity == 0f && this.FollowTo == null;
		}

		public override void changeRiding(IFootable _PD, FOOTRES footres)
		{
			base.changeRiding(_PD, footres);
			M2BlockColliderContainer.BCCLine footBCC = this.FootD.get_FootBCC();
			if (footBCC != null)
			{
				this.footStampBox(footBCC, false);
			}
			if (footres - FOOTRES.FOOTED <= 2)
			{
				this.fineCarryable();
			}
		}

		public override IFootable canFootOn(IFootable F)
		{
			if (!this.beingCarriedByPr())
			{
				return F;
			}
			return null;
		}

		public override bool considerFricOnVelocityCalc()
		{
			return true;
		}

		protected override void OnCollisionEnter2D(Collision2D col)
		{
			if (!this.MyBCC.active)
			{
				return;
			}
			base.OnCollisionEnter2D(col);
			string tag = this.Mp.getTag(col.gameObject);
			if (tag != null && (tag == "Ground" || tag == "Block" || tag == "ChipsUColBox"))
			{
				if (!this.beingCarriedByPr())
				{
					this.recheckAim(AIM.B, this.Afooted_t[3] <= 0f, 1).recheckAim(AIM.R, this.Afooted_t[2] <= 0f, 1).recheckAim(AIM.L, this.Afooted_t[0] <= 0f, 1)
						.recheckAim(AIM.T, this.Afooted_t[1] <= 0f, 1);
					this.t_fix_position = 42f;
					this.t_fix_delay = 12f;
					this.t_velocity_check = 12f;
					this.Phy.killSpeedForce(true, false, true, false, true);
					return;
				}
				ContactPoint2D[] contacts = col.contacts;
				for (int i = contacts.Length - 1; i >= 0; i--)
				{
					Vector2 point = contacts[i].point;
					point.Set(this.Mp.uxToMapx(base.M2D.effectScreenx2ux(point.x)), this.Mp.uyToMapy(base.M2D.effectScreeny2uy(point.y)));
					M2BoxMover.HitBcc.Clear();
					float num = 0.125f + X.LENGTHXYN(0f, 0f, base.vx, base.vy);
					this.MyBCC.getNear(point.x, point.y, num, num, -1, M2BoxMover.HitBcc, false, false, 0f);
					for (int j = M2BoxMover.HitBcc.Count - 1; j >= 0; j--)
					{
						M2BlockColliderContainer.BCCLine bccline = M2BoxMover.HitBcc[j];
						this.footStampBox(bccline, true);
					}
				}
			}
		}

		protected override void OnCollisionStay2D(Collision2D col)
		{
			if (!this.MyBCC.active)
			{
				return;
			}
			base.OnCollisionStay2D(col);
			string tag = this.Mp.getTag(col.gameObject);
			if (tag != null && (tag == "Ground" || tag == "Block" || tag == "ChipsUColBox") && !this.beingCarriedByPr())
			{
				this.t_velocity_check = 12f;
			}
		}

		public void resetEventPosition(M2EventItem Ev = null, bool size_change = false)
		{
			if (this.Parent == null || base.destructed)
			{
				return;
			}
			Ev = Ev ?? this.Parent.getEventMover();
			if (Ev == null)
			{
				return;
			}
			base.getBoundsRect();
			if (size_change)
			{
				Ev.Size(this.sizex * base.CLENM, (this.sizey + 0.5f) * base.CLENM, ALIGN.CENTER, ALIGNY.MIDDLE, false);
			}
			Ev.setTo(base.x, base.y - 0.5f);
			Ev.event_center_shift_y = X.Mx(0f, this.sizey * 2f - 2f);
		}

		private void runSideCheck(int i, float fcnt)
		{
			float num = this.Afooted_t[i];
			bool flag = false;
			bool flag2 = false;
			if (this.beingCarriedByPr())
			{
				if (i == 0 || i == 2)
				{
					if (this.t_carryslide_y > 0f)
					{
						fcnt *= 6f;
					}
				}
				else if (this.t_carryslide_x > 0f)
				{
					fcnt *= 6f;
				}
			}
			if (num < 0f)
			{
				if ((num = X.VALWALK(num, 0f, fcnt)) == 0f)
				{
					List<M2BlockColliderContainer.BCCLine> list = this.AFootedLine[i];
					list.Clear();
					AIM aim = CAim.get_opposite((AIM)i);
					M2BlockColliderContainer.BCCIterator bcciterator;
					this.MyBCC.ItrInit(out bcciterator, true);
					while (bcciterator.Next())
					{
						if (bcciterator.Cur.aim == aim)
						{
							list.Add(bcciterator.Cur);
						}
					}
					flag = true;
				}
				else
				{
					this.Afooted_t[i] = num;
				}
			}
			if (num > 0f || flag)
			{
				if ((this.playfoot_bits & (1U << i)) != 0U)
				{
					this.playfoot_bits &= ~(1U << i);
					flag2 = true;
				}
				if ((this.Afooted_t[i] = num - fcnt) <= 0f)
				{
					num = (this.Afooted_t[i] = 0f);
					List<M2BlockColliderContainer.BCCLine> list2 = this.AFootedLine[i];
					this.MyBCC.active = false;
					M2BlockColliderContainer.BCCLine bccline = null;
					for (int j = list2.Count - 1; j >= 0; j--)
					{
						M2BlockColliderContainer.BCCLine bccline2 = list2[j];
						float shifted_cx = bccline2.shifted_cx;
						float shifted_cy = bccline2.shifted_cy;
						float num2;
						float num3;
						if (i == 0 || i == 2)
						{
							num2 = bccline2.h * 0.5f - 0.125f;
							num3 = 0.125f;
						}
						else
						{
							num3 = bccline2.w * 0.5f - 0.125f;
							num2 = 0.125f;
						}
						if (!bccline2.EachPixel(this.FD_ThroughablePixel, true, 0.08f))
						{
							this.Asoft_fix_pos[i] = ((CAim._XD(i, 1) != 0) ? ((float)((int)(base.x + this.parent_shift_x)) + 0.5f - this.parent_shift_x) : ((float)((int)(base.y + this.parent_shift_y)) + 0.5f - this.parent_shift_y));
							num = (this.Afooted_t[i] = (float)(this.beingCarriedByPr() ? 30 : 4));
							bccline = bccline2;
							break;
						}
						if (!this.Mp.canThroughBccAnother(shifted_cx + (float)bccline2._xd * 0.5f, shifted_cy - (float)bccline2._yd * 0.5f, shifted_cx - (float)bccline2._xd * 0.125f, shifted_cy + (float)bccline2._yd * 0.125f, num3, num2, (int)CAim.get_opposite(bccline2.aim), true, this.FD_FnHitReturnableBcc, true, null))
						{
							num = (this.Afooted_t[i] = (float)(this.beingCarriedByPr() ? 30 : 20));
							bccline = bccline2;
							break;
						}
						list2.RemoveAt(j);
					}
					if (num == 0f)
					{
						this.Asoft_fix_pos[i] = 0f;
					}
					this.MyBCC.active = this.bcc_enabled;
					if (bccline != null && flag2)
					{
						this.PtcFootStamp(bccline);
					}
				}
			}
		}

		public override void runPre()
		{
			base.runPre();
			if (base.destructed)
			{
				return;
			}
			this.Parent.run();
			float num = 0f;
			M2DrawBinder drawBinder = this.Parent.getDrawBinder();
			if (this.HameTarget != null && this.FollowTo == null)
			{
				this.HameTarget.runHameDecided();
			}
			else
			{
				for (int i = 0; i < 4; i++)
				{
					this.runSideCheck(i, this.TS);
				}
				num = this.Afooted_t[3];
				if (num == 0f && !this.beingCarriedByPr())
				{
					if (this.Phy.addableGravity())
					{
						bool flag = this.v_gravity == 0f;
						float num2 = base.vy;
						float num3 = this.Phy.gravity_apply_velocity(this.TS * (this.Phy.isin_water ? this.Phy.water_speed_scale : 1f));
						if (num2 > 0.005f && this.t_velocity_check > 0f)
						{
							num2 = X.Mx(0.005f, X.Mn(num2, base.y - this.Phy.prey + 0.015f + num3));
						}
						this.v_gravity = X.Mn(this.Phy.current_ySpeedMax, num2 + num3);
						if (flag && this.v_gravity != 0f)
						{
							this.fineCarryable();
						}
					}
				}
				else
				{
					if (this.v_gravity != 0f)
					{
						this.v_gravity = 0f;
						this.fineCarryable();
						if (!this.beingCarriedByPr())
						{
							this.t_fix_position = 40f;
						}
					}
					if (!this.beingCarriedByPr() && base.vx != 0f && drawBinder == null)
					{
						this.Phy.clampSpeed(FOCTYPE.WALK, X.Abs(base.vx * 0.81f), -1f, 1f);
					}
				}
			}
			if (this.t_footstamp_lock > 0f)
			{
				this.t_footstamp_lock = X.Mx(0f, this.t_footstamp_lock - this.TS);
			}
			if (this.t_carryslide_x > 0f)
			{
				this.t_carryslide_x = X.Mx(0f, this.t_carryslide_x - this.TS);
			}
			if (this.t_carryslide_y > 0f)
			{
				this.t_carryslide_y = X.Mx(0f, this.t_carryslide_y - this.TS);
			}
			if (this.t_velocity_check > 0f)
			{
				this.t_velocity_check = X.Mx(0f, this.t_velocity_check - this.TS);
			}
			if (this.t_putting != 0f)
			{
				this.runPutting(this.TS);
			}
			if (!this.beingCarriedByPr())
			{
				if (this.t_fix_position < 0f && num == 0f)
				{
					this.t_fix_position = 40f;
				}
				if (this.t_fix_position > 0f)
				{
					if (this.t_fix_delay_ > 0f)
					{
						this.t_fix_delay_ = X.Mx(this.t_fix_delay_ - this.TS, 0f);
					}
					else
					{
						this.t_fix_position = X.Mx(0f, this.t_fix_position - this.TS);
						if (this.t_fix_position < 40f)
						{
							float num4 = (float)X.IntR(base.x + this.parent_shift_x - 0.5f) - this.parent_shift_x + 0.5f;
							float num5 = ((this.v_gravity == 0f) ? ((float)X.IntR(base.y + this.parent_shift_y - 0.5f) - this.parent_shift_y + 0.5f) : base.y);
							this.Phy.addFoc(FOCTYPE.WALK, (num4 - base.x) * 0.4f, (num5 - base.y) * 0.4f, -1f, -1, 1, 0, -1, 0);
						}
					}
				}
			}
			else
			{
				this.t_following += 1f;
			}
			if (drawBinder != null)
			{
				if (this.beingCarriedByPr() && !this.isPuttingByPr())
				{
					this.t_eyepos_trace += this.TS;
					if (this.Afooted_t[1] == 0f && num == 0f)
					{
						this.t_vib += this.TS;
						this.floating_vib_pixel = X.COSI(this.Mp.floort, 110f) * X.ZLINE(this.t_vib, 50f) * 3.5f;
					}
					else
					{
						this.floating_vib_pixel = 0f;
						this.t_vib = 0f;
					}
				}
				else
				{
					this.t_vib = 0f;
					this.t_eyepos_trace += this.TS * 2f;
					this.floating_vib_pixel = X.VALWALK(this.floating_vib_pixel, 0f, 0.25f);
				}
				if (this.t_eyepos_trace >= 6f)
				{
					this.t_eyepos_trace = 0f;
					float num6;
					if (this.beingCarriedByPr())
					{
						num6 = X.ZSIN(drawBinder.t, 40f) * 0.85f + 0.07f * X.COSI(this.Mp.floort, 2.78f) + 0.07f * X.COSI(this.Mp.floort, 11.83f);
					}
					else
					{
						num6 = (1f - X.ZSINV(drawBinder.t, 25f)) * 0.5f;
					}
					Vector3[] apos_mem = this.Apos_mem;
					int num7 = this.pos_mem_i;
					this.pos_mem_i = num7 + 1;
					apos_mem[num7] = new Vector3(base.x, base.y - this.floating_vib_pixel * this.Mp.rCLEN, num6);
					if (this.pos_mem_i >= 5)
					{
						this.pos_mem_i = 0;
					}
					if (this.FollowTo != null && this.t_pfollow == 0f)
					{
						Vector2 followPos = this.getFollowPos();
						if (X.LENGTHXYN(followPos.x, followPos.y, base.x, base.y) >= 0.3f)
						{
							this.t_pfollow = 20f;
						}
					}
				}
			}
		}

		public override void runPhysics(float fcnt)
		{
			if (this.v_gravity != 0f)
			{
				this.walkBy(FOCTYPE.WALK, 0f, this.v_gravity, true);
			}
			for (int i = 0; i < 4; i++)
			{
				float num = this.Asoft_fix_pos[i];
				if (num != 0f)
				{
					switch (i)
					{
					case 0:
					{
						float num2 = this.Phy.tstacked_x;
						if (num2 < num)
						{
							this.Phy.addFoc(FOCTYPE.RESIZE, num - num2, 0f, -1f, -1, 1, 0, -1, 0);
						}
						break;
					}
					case 1:
					{
						float num2 = this.Phy.tstacked_y + this.v_gravity;
						if (num2 < num)
						{
							this.Phy.addFoc(FOCTYPE.RESIZE, 0f, num - num2, -1f, -1, 1, 0, -1, 0);
						}
						break;
					}
					case 2:
					{
						float num2 = this.Phy.tstacked_x;
						if (num2 > num)
						{
							this.Phy.addFoc(FOCTYPE.RESIZE, num - num2, 0f, -1f, -1, 1, 0, -1, 0);
						}
						break;
					}
					case 3:
					{
						float num2 = this.Phy.tstacked_y + this.v_gravity;
						if (num2 > num)
						{
							this.Phy.addFoc(FOCTYPE.RESIZE, 0f, num - num2, -1f, -1, 1, 0, -1, 0);
						}
						break;
					}
					}
				}
			}
			if (this.FollowTo != null && !this.isPuttingByPr() && this.t_pfollow > 0f)
			{
				Vector2 followPos = this.getFollowPos();
				this.t_pfollow = X.Mx(0f, this.t_pfollow - this.TS);
				this.followPosition(followPos.x, followPos.y);
			}
			base.runPhysics(fcnt);
		}

		public override void positionChanged(float prex, float prey)
		{
			base.positionChanged(prex, prey);
			int num = (int)base.x;
			int num2 = (int)base.y;
			int num3 = (int)prex;
			int num4 = (int)prey;
			for (int i = 0; i < 4; i++)
			{
				float num5 = this.Afooted_t[i];
				if (num5 <= 0f)
				{
					switch (i)
					{
					case 0:
					case 2:
						if (num3 != num)
						{
							this.Afooted_t[i] = ((num5 < 0f) ? X.Mx(num5, -5f) : ((float)((num5 == 0f) ? (-1) : (-5))));
						}
						break;
					case 1:
					case 3:
						if (num4 != num2)
						{
							this.Afooted_t[i] = ((num5 < 0f) ? X.Mx(num5, -5f) : ((float)((num5 == 0f) ? (-1) : (-5))));
						}
						break;
					}
				}
			}
		}

		private void followPosition(float dx, float dy)
		{
			this.walkBy(FOCTYPE.WALK | FOCTYPE.ABSORB, dx - base.x, dy - base.y, true);
		}

		public override void walkBy(FOCTYPE foctype, float dx, float dy, bool check_wall = false)
		{
			if (dx != 0f || dy != 0f)
			{
				if (this.Parent.ManageArea != null)
				{
					if (dx < 0f && base.mleft < (float)this.Parent.ManageArea.mapx + 0.1f)
					{
						this.Afooted_t[0] = -3000f;
					}
					if (dx > 0f && base.mright > (float)(this.Parent.ManageArea.mapx + this.Parent.ManageArea.mapw) - 0.1f)
					{
						this.Afooted_t[2] = -3000f;
					}
					if (dy < 0f && base.mtop < (float)this.Parent.ManageArea.mapy + 0.1f)
					{
						this.Afooted_t[1] = -3000f;
					}
					if (dy > 0f && base.mbottom > (float)(this.Parent.ManageArea.mapy + this.Parent.ManageArea.maph) - 0.1f)
					{
						this.Afooted_t[3] = -3000f;
					}
				}
				if (check_wall && (foctype & FOCTYPE.ABSORB) != (FOCTYPE)0U)
				{
					this.slideshift_aim = -1;
					if (dx != 0f)
					{
						this.SlideShift = Vector3.zero;
						M2BlockColliderContainer.BCCIterator bcciterator;
						this.MyBCC.ItrInit(out bcciterator, true);
						this.slideshift_aim = ((dx < 0f) ? 0 : 2);
						AIM aim = CAim.get_opposite((AIM)this.slideshift_aim);
						int num = this.slideshift_aim;
						while (this.SlideShift.z == 0f && bcciterator.Next())
						{
							if (bcciterator.Cur.aim == aim)
							{
								this.SlideShift = Vector3.zero;
								if (!bcciterator.Cur.EachPixel(this.FD_ThroughablePixelWithSlide, true, 0.08f) && this.Afooted_t[num] == 0f)
								{
									this.footStampBox(bcciterator.Cur, false);
								}
							}
						}
					}
					if (dy != 0f)
					{
						this.SlideShift = Vector3.zero;
						this.slideshift_aim = ((dy < 0f) ? 1 : 3);
						int num2 = this.slideshift_aim;
						AIM aim2 = CAim.get_opposite((AIM)this.slideshift_aim);
						M2BlockColliderContainer.BCCIterator bcciterator2;
						this.MyBCC.ItrInit(out bcciterator2, true);
						while (this.SlideShift.z == 0f && bcciterator2.Next())
						{
							if (bcciterator2.Cur.aim == aim2)
							{
								this.SlideShift = Vector3.zero;
								if (!bcciterator2.Cur.EachPixel(this.FD_ThroughablePixelWithSlide, true, 0.08f) && this.Afooted_t[num2] == 0f)
								{
									this.footStampBox(bcciterator2.Cur, false);
								}
							}
						}
					}
				}
				this.clipWalkVelocity(ref dx, ref dy);
				if (this.beingCarriedByPr())
				{
					if (this.t_putting > 0f)
					{
						if (foctype == FOCTYPE.RESIZE)
						{
							dx = X.absMn(dx, 1f);
							dy = X.absMn(dy, 1f);
						}
						else if (this.PuttingNear.z == 3f)
						{
							float num3 = 0.46f;
							dx = X.absMn(dx * 0.53f, num3);
							dy = X.absMn(dy * 0.53f, num3);
						}
						else
						{
							float num4 = 0.16f;
							dx = X.absMn(dx * 0.23f, num4);
							dy = X.absMn(dy * 0.23f, num4);
						}
					}
					else
					{
						dx = X.absMn(dx, X.ZPOW(this.t_following - 20f, 40f) * 0.08f);
						dy = X.absMn(dy, X.Mx(X.ZLINE(this.t_following, 30f) * 0.08f, X.Abs(this.v_gravity)));
					}
				}
				this.Phy.addFoc(foctype, dx, 0f, -1f, -1, 1, 0, -1, 0);
				this.Phy.addFoc(foctype, 0f, dy, -1f, -1, 1, 0, -1, 0);
			}
		}

		public void clipWalkVelocity(ref float dx, ref float dy)
		{
			bool flag = true;
			if (this.Afooted_t[0] != 0f && this.Afooted_t[2] != 0f)
			{
				int num = X.IntR(base.mleft);
				int num2 = X.IntR(base.mright);
				DRect boundsRect = base.getBoundsRect();
				if ((float)(num2 - num) == boundsRect.w)
				{
					flag = false;
					float num3 = X.NI((float)num, (float)num2, 0.5f);
					if (X.Abs(num3 - base.x) > 0.005f)
					{
						dx = num3 - base.x;
					}
					else
					{
						dx = 0f;
					}
				}
			}
			if (flag)
			{
				if (dx < 0f && this.Afooted_t[0] != 0f)
				{
					float num4 = X.frac(this.Phy.tstacked_x + this.parent_shift_x);
					dx = X.Mx(dx, -X.Mx(0f, num4 - 0.5f));
				}
				if (dx > 0f && this.Afooted_t[2] != 0f)
				{
					float num5 = X.frac(this.Phy.tstacked_x + this.parent_shift_x);
					dx = X.Mn(dx, X.Mx(0f, 0.5f - num5));
				}
			}
			flag = true;
			if (this.Afooted_t[1] != 0f && this.Afooted_t[3] != 0f)
			{
				int num6 = X.IntR(base.mtop);
				int num7 = X.IntR(base.mbottom);
				DRect boundsRect2 = base.getBoundsRect();
				if ((float)(num7 - num6) == boundsRect2.h)
				{
					flag = false;
					float num8 = X.NI((float)num6, (float)num7, 0.5f);
					if (X.Abs(num8 - base.y) > 0.005f)
					{
						dy = num8 - base.y;
					}
					else
					{
						dy = 0f;
					}
				}
			}
			if (flag)
			{
				if (dy < 0f && this.Afooted_t[1] != 0f)
				{
					float num9 = X.frac(this.Phy.tstacked_y + this.parent_shift_y);
					dy = X.Mx(dy, -X.Mx(0f, num9 - 0.5f));
				}
				if (dy > 0f && this.Afooted_t[3] != 0f)
				{
					float num10 = X.frac(this.Phy.tstacked_y + this.parent_shift_y);
					dy = X.Mn(dy, X.Mx(0f, 0.5f - num10));
				}
			}
		}

		private bool ThroughablePixel(int mapx, int mapy, M2BlockColliderContainer.BCCLine Tgt)
		{
			int config = this.Mp.getConfig(mapx, mapy);
			return CCON.canStandAndNoBlockSlope(config) && CCON.mistPassable(config, 2);
		}

		private bool ThroughablePixelWithSlide(int mapx, int mapy, M2BlockColliderContainer.BCCLine Tgt)
		{
			M2Pt pointPuts = this.Mp.getPointPuts(mapx, mapy, false, false);
			if (pointPuts != null && !CCON.canStand(pointPuts.cfg) && this.slideshift_aim >= 0 && this.beingCarriedByPr() && !this.isPuttingByPr())
			{
				M2BlockColliderContainer.BCCLine sideBcc = pointPuts.getSideBcc(this.Mp, mapx, mapy, (AIM)this.slideshift_aim);
				if (sideBcc == null)
				{
					return false;
				}
				bool flag;
				switch (sideBcc.aim)
				{
				case AIM.L:
					flag = mapx + 1 == (int)sideBcc.x;
					break;
				case AIM.T:
					flag = mapy + 1 == (int)sideBcc.y;
					break;
				case AIM.R:
					flag = mapx == (int)sideBcc.x;
					break;
				case AIM.B:
					flag = mapy == (int)sideBcc.y;
					break;
				default:
				{
					float num = sideBcc.slopeBottomY((float)mapx + 0.5f);
					flag = X.BTWW(num - 1f, (float)mapy + 0.5f, num + 1f);
					break;
				}
				}
				if (!flag)
				{
					return false;
				}
				CAim._XD(this.slideshift_aim, 1);
				CAim._YD(this.slideshift_aim, 1);
				Vector3 slideCarryAim = sideBcc.getSlideCarryAim((AIM)this.slideshift_aim, this.FollowTo.x, Tgt.shifted_cy);
				if (slideCarryAim.z == 1f)
				{
					if (slideCarryAim.x != 0f)
					{
						float num2 = ((slideCarryAim.x > 0f) ? Tgt.shifted_x : Tgt.shifted_right);
						float num3 = ((slideCarryAim.x > 0f) ? sideBcc.shifted_right : sideBcc.shifted_x);
						slideCarryAim.x = num3 - num2;
					}
					if (slideCarryAim.y != 0f)
					{
						float num4 = ((slideCarryAim.y > 0f) ? Tgt.shifted_y : Tgt.shifted_bottom);
						float num5 = ((slideCarryAim.y > 0f) ? sideBcc.shifted_bottom : sideBcc.shifted_y);
						slideCarryAim.y = num5 - num4;
					}
					this.addSlidePos(slideCarryAim.x, slideCarryAim.y, this.slideshift_aim);
					this.slideshift_aim = -1;
					return false;
				}
			}
			return true;
		}

		private void addSlidePos(float shiftx, float shifty, int a)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			if (shiftx != 0f)
			{
				if (shiftx > 6f)
				{
					shiftx = 0f;
				}
				else
				{
					num = X.Mx(num, X.Abs(shiftx / 0.08f) + 15f);
					num2 = shiftx + base.x;
				}
			}
			if (shifty != 0f)
			{
				if (shifty > 6f)
				{
					shifty = 0f;
				}
				else
				{
					num = X.Mx(num, X.Abs(shifty / 0.08f) + 15f);
					num3 = shifty + base.y;
				}
			}
			if (shiftx == 0f && shifty == 0f)
			{
				return;
			}
			int count = this.ASlideDepert.Count;
			num += this.Mp.floort;
			for (int i = 0; i < count; i++)
			{
				Vector4 vector = this.ASlideDepert[i];
				if (X.Abs(vector.x - shiftx) < 0.014f && X.Abs(vector.y - shifty) < 0.014f && vector.w == (float)a)
				{
					vector.z = X.Mx(vector.z, num);
					this.ASlideDepert[i] = vector;
					return;
				}
				if (num2 != 0f && num3 == 0f && vector.x != 0f && vector.y == 0f)
				{
					if (num2 > base.x && vector.x > base.x)
					{
						vector.x = X.Mx(num2, vector.x);
						vector.z = X.Mx(vector.z, num);
						this.ASlideDepert[i] = vector;
						return;
					}
					if (num2 < base.x && vector.x < base.x)
					{
						vector.x = X.Mn(num2, vector.x);
						vector.z = X.Mx(vector.z, num);
						this.ASlideDepert[i] = vector;
						return;
					}
				}
				if (num2 == 0f && num3 != 0f && vector.x == 0f && vector.y != 0f)
				{
					if (num3 > base.y && vector.y > base.y)
					{
						vector.y = X.Mx(num3, vector.y);
						vector.z = X.Mx(vector.z, num);
						this.ASlideDepert[i] = vector;
						return;
					}
					if (num3 < base.y && vector.y < base.y)
					{
						vector.y = X.Mn(num3, vector.y);
						vector.z = X.Mx(vector.z, num);
						this.ASlideDepert[i] = vector;
						return;
					}
				}
			}
			this.ASlideDepert.Add(new Vector4(num2, num3, num, (float)a));
		}

		private bool FnHitReturnableBcc(M2BlockColliderContainer.BCCLine _C, Vector3 Hit)
		{
			int aim = (int)_C.aim;
			if (!this.beingCarriedByPr() && aim != M2BoxMover.slide_checking_aim)
			{
				return true;
			}
			M2BoxMover m2BoxMover = _C.BCC.BelongTo as M2BoxMover;
			if (m2BoxMover != null)
			{
				if (M2BoxMover.ASlideChecking.IndexOf(_C) >= 0)
				{
					return false;
				}
				M2BoxMover.ASlideChecking.Add(_C);
				if (m2BoxMover.beingCarriedByPr())
				{
					return false;
				}
				if (M2BoxMover.slide_checking_aim >= 0)
				{
					m2BoxMover.Afooted_t[aim] = -1f;
				}
				m2BoxMover.runSideCheck(aim, 3000f);
				if (m2BoxMover.Afooted_t[aim] == 0f)
				{
					m2BoxMover.t_fix_delay = 15f;
					return false;
				}
			}
			return true;
		}

		public override void resetChipsDrawPosition(float add_shiftx = 0f, float add_shifty = 0f)
		{
			base.resetChipsDrawPosition(add_shiftx, add_shifty - this.floating_vib_pixel);
			if (this.Parent.event_trigger_visible)
			{
				this.resetEventPosition(null, false);
			}
		}

		public void activate(bool from_boxparent = false)
		{
			if (!from_boxparent)
			{
				this.Parent.activate();
				return;
			}
			PRNoel prNoel = this.nM2D.getPrNoel();
			if ((this.FollowTo = prNoel) != null)
			{
				prNoel.getSkillManager().addCarryingBox(this);
				this.Phy.addLockGravity(this.Parent, 0f, -1f);
				this.t_eyepos_trace = 6f;
				this.t_pfollow = 0f;
				this.t_fix_delay_ = 0f;
				this.t_vib = 0f;
				this.t_following = 0f;
				this.t_carryslide_x = (this.t_carryslide_y = 0f);
				this.ASlideDepert.Clear();
				for (int i = 4; i >= 0; i--)
				{
					this.Apos_mem[i] = Vector3.zero;
				}
				this.recheckAim(AIM.B, false, 1).recheckAim(AIM.R, true, 1).recheckAim(AIM.L, true, 1)
					.recheckAim(AIM.T, true, 1);
				this.pos_mem_i = 0;
				Vector3[] apos_mem = this.Apos_mem;
				int num = this.pos_mem_i;
				this.pos_mem_i = num + 1;
				apos_mem[num] = new Vector3(base.x, base.y, 0.125f);
				this.v_gravity = 0f;
				this.fineWeight();
				this.releasePutting(true, true);
				this.fineCarryable();
				this.fineHittingLayer();
				this.MyBCC.active = this.bcc_enabled;
				this.FollowTo.PtcST("puzz_box_activate", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
			}
		}

		public void deactivate(bool from_boxparent = false)
		{
			if (!from_boxparent)
			{
				this.Parent.deactivate();
				return;
			}
			NelChipHamehame nelChipHamehame = null;
			NelChipHamehame hameTarget = this.HameTarget;
			if (this.FollowTo != null)
			{
				this.FollowTo.killPtc("puzz_box_activate", true);
				this.FollowTo.PtcST("puzz_box_deactivate", PtcHolder.PTC_HOLD.ACT, PTCThread.StFollow.FOLLOW_T);
				this.t_fix_delay_ = 0f;
				this.Phy.killSpeedForce(true, false, true, false, true);
				if (this.PuttingNear.z >= 2f)
				{
					float num = base.y;
					float num2 = this.PuttingNear.x - this.parent_shift_x - base.x;
					num = this.PuttingNear.y - this.parent_shift_y - base.y;
					this.clipWalkVelocity(ref num2, ref num);
					this.Phy.addFocToSmooth(FOCTYPE.WALK, num2 + base.x, num + base.y, 15, -1, 0, -1f);
					this.t_fix_position = -1f;
					if (this.PuttingNear.z == 2f && this.HameTarget != null)
					{
						nelChipHamehame = this.HameTarget.checkHame(this, this.Parent.getConnects(), true);
					}
				}
				for (int i = 3; i >= 0; i--)
				{
					if (this.Afooted_t[i] > 4f)
					{
						this.Afooted_t[i] = 4f;
					}
				}
				this.recheckAim(AIM.B, false, 1);
			}
			this.releasePutting(false, false);
			if (this.FollowTo is PRNoel)
			{
				(this.FollowTo as PRNoel).getSkillManager().remCarryingBox(this);
			}
			this.ASlideDepert.Clear();
			this.FollowTo = null;
			this.fineWeight();
			if (nelChipHamehame != null)
			{
				this.releasePutting(true, false);
				this.HameTarget = nelChipHamehame;
				this.v_gravity = 0f;
				this.recheckAim(AIM.B, false, 1).recheckAim(AIM.R, false, 1).recheckAim(AIM.L, false, 1)
					.recheckAim(AIM.T, false, 1);
			}
			else if (hameTarget != null)
			{
				hameTarget.removeEffect();
			}
			this.fineCarryable();
			if (this.Phy != null)
			{
				this.Phy.remLockGravity(this.Parent);
			}
			this.MyBCC.active = this.bcc_enabled;
			this.fineHittingLayer();
		}

		public M2BoxMover recheckAim(AIM a, bool playfoot = false, int delay = 1)
		{
			this.Afooted_t[(int)a] = (float)(-(float)delay);
			this.AFootedLine[(int)a].Clear();
			if (playfoot)
			{
				this.playfoot_bits |= 1U << (int)a;
			}
			return this;
		}

		public void deactivateCompletely()
		{
			this.t_eyepos_trace = -1f;
			this.floating_vib_pixel = 0f;
			if (this.t_putting != 0f)
			{
				this.releasePutting(true, true);
			}
			if (!base.destructed)
			{
				this.resetEventPosition(null, false);
			}
		}

		public bool runBoxPositionSet(float fcnt, bool holding_x, bool initting = false)
		{
			if (!holding_x)
			{
				if (this.PuttingNear.z < 1f || (this.t_putting > 10f && this.can_put_block_pr()))
				{
					this.deactivate(false);
				}
				return false;
			}
			if (!this.can_put_block_pr(true))
			{
				return true;
			}
			if (this.t_putting < 1f)
			{
				this.t_putting = 1f;
				this.ASlideDepert.Clear();
				SND.Ui.play("tool_sel_init", false);
				initting = true;
				this.fineWeight();
			}
			if (initting)
			{
				this.finePuttingPos(initting);
			}
			return true;
		}

		private void runPutting(float fcnt)
		{
			if (this.t_putting <= 0f)
			{
				this.t_putting = X.Mn(this.t_putting + fcnt, 0f);
				if (this.t_putting >= 0f)
				{
					this.releasePutting(true, true);
					return;
				}
			}
			if (this.Oputpos == null)
			{
				this.releasePutting(true, true);
				return;
			}
			if (!this.can_put_block_pr())
			{
				this.releasePutting(false, true);
				return;
			}
			this.t_carryslide_x = (this.t_carryslide_y = 0f);
			if (this.PuttingNear.z >= 1f)
			{
				int num = 0;
				int num2 = 0;
				if (this.PuttingNear.z < 3f)
				{
					if (IN.isL())
					{
						num--;
					}
					else if (IN.isR())
					{
						num++;
					}
					else if (IN.isT())
					{
						num2--;
					}
					else if (IN.isB())
					{
						num2++;
					}
				}
				if (num != 0 || num2 != 0)
				{
					int num3 = (int)CAim.get_aim_tetra(0, 0, num, -num2);
					AIM aim = CAim.get_aim_tetra(0, 0, -num, num2);
					int num4 = (int)this.PuttingNear.x + num;
					int num5 = (int)this.PuttingNear.y + num2;
					uint num6 = M2BoxMover.xy2b(num4, num5);
					int num7;
					bool flag = !this.Oputpos.TryGetValue(num6, out num7) || (num7 & 16) == 0;
					if (!flag)
					{
						DRect bounds = this.MyBCC.Bounds;
						float num8 = this.PuttingNear.x - this.parent_shift_x - bounds.width * 0.5f;
						float num9 = this.PuttingNear.y - this.parent_shift_y - bounds.height * 0.5f;
						this.MyBCC.active = false;
						M2BoxMover.ASlideChecking.Clear();
						M2BoxMover.slide_checking_aim = num3;
						flag = false;
						M2BlockColliderContainer.BCCIterator bcciterator;
						this.MyBCC.ItrInit(out bcciterator, true);
						M2BlockColliderContainer.BCCLine bccline;
						while (bcciterator.Next(out bccline))
						{
							if (bccline.aim == aim)
							{
								float num10 = num8 + bccline.cx - (float)num * 0.5f;
								float num11 = num9 + bccline.cy - (float)num2 * 0.5f;
								float num12 = ((num != 0) ? 0.125f : (bccline.width * 0.5f - 0.125f));
								float num13 = ((num2 != 0) ? 0.125f : (bccline.height * 0.5f - 0.125f));
								if (!this.FD_ThroughablePixel((int)(num10 + (float)num), (int)(num11 + (float)num2), null) || !this.Mp.canThroughBccAnother(num10, num11, num10 + (float)num, num11 + (float)num2, num12, num13, num3, true, this.FD_FnHitReturnableBcc, true, null))
								{
									flag = true;
									break;
								}
							}
						}
						M2BoxMover.slide_checking_aim = -1;
						this.MyBCC.active = this.bcc_enabled;
					}
					if (flag)
					{
						SND.Ui.play("toggle_button_limit", false);
						this.TeCon.setColorBlink(1f, 8f, 0.45f, 14311006, 0);
						this.TeCon.setQuake(3f, 6, 2f, 0);
					}
					else
					{
						SND.Ui.play("toggle_button_close", false);
						if (this.PuttingNear.z == 3f)
						{
							this.Phy.killSpeedForce(true, true, true, false, true);
							this.walkBy(FOCTYPE.RESIZE, this.PuttingNear.x - base.x - this.parent_shift_x, this.PuttingNear.y - base.y - this.parent_shift_y, true);
						}
						this.PuttingNear.x = this.PuttingNear.x + (float)num;
						this.PuttingNear.y = this.PuttingNear.y + (float)num2;
						if (this.PuttingNear.z == 2f)
						{
							this.PuttingNear.z = 3f;
						}
						if (this.HameTarget != null)
						{
							this.HameTarget.removeEffect();
							this.HameTarget = null;
						}
					}
				}
			}
			if (this.t_putting < 30f)
			{
				this.t_putting += fcnt;
			}
			if (this.PuttingNear.z >= 1f)
			{
				this.walkBy(FOCTYPE.WALK, this.PuttingNear.x - (base.x + this.parent_shift_x), this.PuttingNear.y - (base.y + this.parent_shift_y), true);
				if (this.PuttingNear.z != 2f && X.LENGTHXYN(this.PuttingNear.x, this.PuttingNear.y, base.x + this.parent_shift_x, base.y + this.parent_shift_y) < ((this.PuttingNear.z == 3f) ? 0.04f : 0.25f))
				{
					this.PuttingNear.z = 2f;
					M2BoxMover.AHamePt.Clear();
					this.Mp.getPointMetaPutsTo((int)this.PuttingNear.x, (int)this.PuttingNear.y, M2BoxMover.AHamePt, "hame");
					for (int i = M2BoxMover.AHamePt.Count - 1; i >= 0; i--)
					{
						NelChipHamehame nelChipHamehame = M2BoxMover.AHamePt[i] as NelChipHamehame;
						if (nelChipHamehame != null)
						{
							this.HameTarget = nelChipHamehame.checkHame(this, this.Parent.getConnects(), false);
							return;
						}
					}
				}
			}
		}

		private void finePuttingPos(bool force = false)
		{
			if (this.FollowTo == null)
			{
				return;
			}
			int num = (int)(base.x + this.parent_shift_x);
			int num2 = (int)(base.y + this.parent_shift_y);
			if (!force && num == this.putting_x && num2 == this.putting_y)
			{
				return;
			}
			this.putting_x = num;
			this.putting_y = num2;
			int num3 = (int)(X.absMn(this.FollowTo.x + this.FollowTo.mpf_is_right + 1.6f - ((float)num + 0.5f), 6f) + ((float)num + 0.5f));
			int num4 = (int)(X.absMn(this.FollowTo.y - ((float)num2 + 0.5f), 5f) + ((float)num2 + 0.5f));
			if (this.Oputpos == null)
			{
				this.Oputpos = new BDic<uint, int>(61);
			}
			M2BoxMover.Aput_buf.Clear();
			this.Oputpos.Clear();
			int num5 = 0;
			for (int i = -1; i < 4; i++)
			{
				int num6 = ((i == -1) ? this.putting_x : (this.putting_x + CAim._XD(i, 1) * X.MPF((int)base.x <= this.putting_x)));
				int num7 = ((i == -1) ? this.putting_y : (this.putting_y - CAim._YD(i, 1) * X.MPF((int)base.y <= this.putting_y)));
				if (this.FD_ThroughablePixel(num6, num7, null))
				{
					this.Oputpos[M2BoxMover.xy2b(this.putting_x, this.putting_y)] = 0;
					M2BoxMover.Aput_buf.Add(M2BoxMover.xy2b(this.putting_x, this.putting_y));
					break;
				}
			}
			new List<uint>(8);
			int num8 = 6 + ((this.Parent.ManageArea != null) ? this.Parent.ManageArea.box_manageable_len : 0);
			while (++num5 <= num8 + 14)
			{
				int count = M2BoxMover.Aput_buf.Count;
				for (int j = count - 1; j >= 0; j--)
				{
					uint num9 = M2BoxMover.Aput_buf[j];
					int num10 = (int)Map2d.b2x(num9);
					int num11 = (int)Map2d.b2y(num9);
					for (int k = 0; k < 4; k++)
					{
						int num12 = num10 + CAim._XD(k, 1);
						int num13 = num11 - CAim._YD(k, 1);
						uint num14 = M2BoxMover.xy2b(num12, num13);
						int num15;
						if (this.Oputpos.TryGetValue(num14, out num15))
						{
							this.Oputpos[num14] = num15 | (1 << (int)CAim.get_opposite((AIM)k));
						}
						else
						{
							float num16 = X.LENGTHXYS((float)this.putting_x, (float)this.putting_y, (float)num12, (float)num13);
							float num17 = X.LENGTHXYS((float)num3, (float)num4, (float)num12, (float)num13);
							if (X.Mn(num16, num17) < (float)num8 && this.FD_ThroughablePixel(num12, num13, null))
							{
								this.Oputpos[num14] = 1 << (int)CAim.get_opposite((AIM)k);
								BDic<uint, int> bdic = this.Oputpos;
								uint num18 = num9;
								bdic[num18] |= 1 << k;
								M2BoxMover.Aput_buf.Add(num14);
							}
						}
					}
				}
				M2BoxMover.Aput_buf.RemoveRange(0, count);
				if (M2BoxMover.Aput_buf.Count == 0)
				{
					break;
				}
			}
			M2BoxMover.Aput_buf.Clear();
			NelChipPuzzleBox.ConnectMem[] connects = this.Parent.getConnects();
			int num19 = connects.Length;
			int num20 = 0;
			for (int l = 0; l < num19; l++)
			{
				if (connects[l].Cp == this.Parent)
				{
					num20 = connects[l].dir_bits;
					break;
				}
			}
			Vector3 vector = new Vector3(0f, 0f, 0f);
			foreach (KeyValuePair<uint, int> keyValuePair in this.Oputpos)
			{
				if (num20 <= 0 || (keyValuePair.Value & num20) == num20)
				{
					bool flag = true;
					uint key = keyValuePair.Key;
					int num21 = (int)Map2d.b2x(key);
					int num22 = (int)Map2d.b2y(key);
					for (int m = 0; m < num19; m++)
					{
						NelChipPuzzleBox.ConnectMem connectMem = connects[m];
						if (connectMem.Cp != this.Parent)
						{
							int num23 = num21;
							int num24 = num22;
							if (connectMem.Cp != this.Parent)
							{
								num23 += connectMem.Cp.mapx - this.Parent.mapx;
								num24 += connectMem.Cp.mapy - this.Parent.mapy;
								uint num25 = M2BoxMover.xy2b(num23, num24);
								int num26;
								if (!this.Oputpos.TryGetValue(num25, out num26) || (num26 & connectMem.dir_bits) != connectMem.dir_bits)
								{
									flag = false;
									break;
								}
							}
						}
					}
					if (flag)
					{
						float num27 = X.LENGTHXYS((float)num21 + 0.5f, (float)num22 + 0.5f, base.x + this.parent_shift_x, base.y + this.parent_shift_y);
						if (M2BoxMover.Aput_buf.Count == 0 || num27 < vector.z)
						{
							vector.Set((float)num21 + 0.5f, (float)num22 + 0.5f, num27);
						}
						M2BoxMover.Aput_buf.Add(keyValuePair.Key);
					}
				}
			}
			if (M2BoxMover.Aput_buf.Count > 0)
			{
				this.PuttingNear.Set(vector.x, vector.y, 1f);
				for (int n = M2BoxMover.Aput_buf.Count - 1; n >= 0; n--)
				{
					BDic<uint, int> bdic = this.Oputpos;
					uint num18 = M2BoxMover.Aput_buf[n];
					bdic[num18] |= 16;
				}
				return;
			}
			this.PuttingNear.Set(base.x, base.y, 0f);
		}

		private static uint xy2b(int x, int y)
		{
			return Map2d.xy2b(x, y);
		}

		private bool can_put_block_pr()
		{
			PR pr = this.FollowTo as PR;
			return pr != null && this.can_put_block_pr(pr.magic_chanting_or_preparing);
		}

		private bool can_put_block_pr(bool magic_chanting)
		{
			if (this.FollowTo is PR && magic_chanting)
			{
				PR pr = this.FollowTo as PR;
				M2PrSkill skillManager = pr.getSkillManager();
				return pr.hasFoot() && pr.isNormalState() && skillManager.magic_progressable;
			}
			return false;
		}

		private void releasePutting(bool immediate = false, bool remove_hame_effect = true)
		{
			if (immediate)
			{
				this.t_putting = 0f;
				this.fineWeight();
			}
			if (this.t_putting > 0f)
			{
				this.t_putting = -30f;
				this.t_following = 0f;
				this.fineWeight();
			}
			if (this.HameTarget != null && remove_hame_effect)
			{
				this.HameTarget.removeEffect();
			}
			this.HameTarget = null;
			this.PuttingNear.z = 0f;
		}

		public bool drawLight(EffectItem Ef, M2DrawBinder Ed, NelChipPuzzleBox.ConnectMem[] ACmem)
		{
			bool flag = this.FollowTo != null;
			MeshDrawer meshDrawer = null;
			int num = 5;
			int num2 = this.pos_mem_i;
			while (--num >= 0)
			{
				if (num2 <= 0)
				{
					num2 = 5;
				}
				Vector3 vector = this.Apos_mem[--num2];
				if (vector.z > 0f)
				{
					vector.x += this.parent_shift_x + (float)(-(float)this.Parent.drawx) * this.Mp.rCLEN;
					vector.y += this.parent_shift_y + (float)(-(float)this.Parent.drawy) * this.Mp.rCLEN;
					if (meshDrawer == null)
					{
						meshDrawer = Ef.GetMeshImg("", base.M2D.MIchip, BLEND.ADD, true);
					}
					meshDrawer.Col = meshDrawer.ColGrd.White().mulA(X.ZLINE((float)(num + 1), 5f) * vector.z).C;
					for (int i = ACmem.Length - 1; i >= 0; i--)
					{
						NelChipPuzzleBox cp = ACmem[i].Cp;
						bool flag2;
						Vector3 vector2 = Ef.EF.calcMeshXY(vector.x + (float)cp.drawx * this.Mp.rCLEN, vector.y + (float)cp.drawy * this.Mp.rCLEN, null, out flag2);
						meshDrawer.base_x = vector2.x;
						meshDrawer.base_y = vector2.y;
						PxlMeshDrawer srcMesh = cp.Img.getSrcMesh(3);
						meshDrawer.RotaMesh(0f, 0f, this.Mp.base_scale, this.Mp.base_scale, cp.draw_rotR, srcMesh, cp.flip, false);
					}
					flag = true;
				}
			}
			if (this.t_putting != 0f && this.Oputpos != null)
			{
				meshDrawer = Ef.GetMesh("", MTRX.MtrMeshNormal, true);
				float num3 = X.ZLINE(X.Abs(this.t_putting) - 1f, 30f);
				meshDrawer.ColGrd.Set((this.PuttingNear.z < 1f) ? 4290822336U : 4287102939U);
				foreach (KeyValuePair<uint, int> keyValuePair in this.Oputpos)
				{
					float num4 = Map2d.b2x(keyValuePair.Key);
					float num5 = Map2d.b2y(keyValuePair.Key);
					uint ran = X.GETRAN2((int)num4 + 33, (int)num5 * 2 + 57);
					bool flag3;
					Vector3 vector3 = Ef.EF.calcMeshXY(num4 + 0.5f, num5 + 0.5f, null, out flag3);
					meshDrawer.base_x = vector3.x;
					meshDrawer.base_y = vector3.y;
					meshDrawer.Col = meshDrawer.ColGrd.setA1(0.25f + 0.125f * X.COSI(this.Mp.floort + X.RAN(ran, 2880) * 200f, 140f + 60f * X.RAN(ran, 1593))).mulA(num3).C;
					meshDrawer.Box(0f, 0f, this.Mp.CLENB, this.Mp.CLENB, 1f, false);
					if (this.t_putting > 0f)
					{
						float num6 = (this.Mp.floort + 4192f - (num4 + num5) * 4f) % 170f;
						if (num6 < 50f)
						{
							meshDrawer.Col = meshDrawer.ColGrd.setA1(0.3f * (1f - X.ZLINE(num6, 50f))).C;
							meshDrawer.Rect(0f, 0f, this.Mp.CLENB, this.Mp.CLENB, false);
						}
					}
				}
				meshDrawer.Col = meshDrawer.ColGrd.setA1(1f).C;
				this.MyBCC.LineDashed(Ef, meshDrawer, (this.PuttingNear.x - (base.x + this.parent_shift_x)) * this.Mp.CLENB, -(this.PuttingNear.y - (base.y + this.parent_shift_y)) * this.Mp.CLENB, X.ANMPT(18, 1f), 2f, 0.5f, 0.5f);
			}
			if (!flag)
			{
				this.deactivateCompletely();
			}
			return flag;
		}

		public bool drawBorder(EffectItem Ef, MeshDrawer Md, float lvl)
		{
			if (Md == null)
			{
				Md = Ef.GetMesh("", MTRX.MtrMeshNormal, false);
			}
			if (this.t_putting != 0f && this.Oputpos.Count == 0)
			{
				Md.ColGrd.Set(4290822336U);
			}
			else
			{
				Md.ColGrd.Set(4287102939U);
			}
			Md.Col = Md.ColGrd.mulA(lvl).C;
			this.MyBCC.LineDashed(Ef, Md, 0f, 0f, X.ANMPT(18, 1f), 2f, 0.5f, 0.5f);
			return true;
		}

		public Vector2 getFollowPos()
		{
			DRect boundsRect = base.getBoundsRect();
			Vector4 zero = Vector4.zero;
			float num = this.FollowTo.x - (1f + boundsRect.width * 0.5f) * this.FollowTo.mpf_is_right;
			if (this.ASlideDepert.Count > 0)
			{
				for (int i = this.ASlideDepert.Count - 1; i >= 0; i--)
				{
					Vector4 vector = this.ASlideDepert[i];
					if (vector.z <= this.Mp.floort)
					{
						this.ASlideDepert.RemoveAt(i);
					}
					else
					{
						if (vector.x != 0f && X.Abs(vector.x - this.FollowTo.x) >= 1.5f + boundsRect.width)
						{
							if (X.Abs(vector.x - base.x) < 0.014f)
							{
								vector.x = 0f;
							}
							else
							{
								zero.x += vector.x;
								zero.z += 1f;
							}
						}
						if (vector.y != 0f)
						{
							if (X.Abs(vector.y - base.y) < 0.014f)
							{
								vector.y = 0f;
							}
							else
							{
								zero.y += vector.y;
								zero.w += 1f;
							}
						}
						if (vector.x == 0f && vector.y == 0f)
						{
							vector.z = X.Mn(this.Mp.floort + 4f, vector.z);
							int num2 = CAim._XD((int)vector.w, 1);
							int num3 = CAim._YD((int)vector.w, 1);
							if (num2 != 0)
							{
								zero.x += base.x + (float)num2;
								zero.z += 1f;
							}
							if (num3 != 0)
							{
								zero.y += base.y - (float)num3;
								zero.w += 1f;
							}
						}
					}
				}
			}
			if (zero.z == 0f && zero.w == 0f)
			{
				zero.x = num;
				if (this.t_carryslide_y > 0f)
				{
					zero.y = (float)X.IntR(base.y) + X.saturate(this.parent_shift_y);
				}
				else
				{
					zero.y = this.FollowTo.mbottom - boundsRect.height * 0.5f - 0.8f;
				}
			}
			else
			{
				if (zero.z == 0f)
				{
					zero.x = base.x;
				}
				else
				{
					zero.x /= zero.z;
				}
				if (zero.w == 0f)
				{
					zero.y = base.y;
				}
				else
				{
					zero.y /= zero.w;
				}
				zero.x = X.absMn(zero.x - this.FollowTo.x, 6f) + this.FollowTo.x;
				zero.y = X.absMn(zero.y - this.FollowTo.y, 3.5f) + this.FollowTo.y;
			}
			return zero;
		}

		public void BCCInitializing(M2BlockColliderContainer BCC)
		{
			for (int i = 0; i < 4; i++)
			{
				float num = this.Afooted_t[i];
			}
		}

		public bool isBCCListenerActive(M2BlockColliderContainer.BCCLine BCC)
		{
			return false;
		}

		public void BCCtouched(M2BlockColliderContainer.BCCLine BCC, M2FootManager FootD)
		{
		}

		public bool runBCCEvent(M2BlockColliderContainer.BCCLine BCC, M2FootManager FootD)
		{
			return false;
		}

		public Rect getBccAppliedArea(Rect BccRect)
		{
			if (this.Parent.ManageArea != null)
			{
				return new Rect((float)this.Parent.ManageArea.mapx, (float)this.Parent.ManageArea.mapy, (float)this.Parent.ManageArea.mapw, (float)this.Parent.ManageArea.maph);
			}
			if (this.Mp == null)
			{
				return new Rect(0f, 0f, 0f, 0f);
			}
			return new Rect((float)this.Mp.crop, (float)this.Mp.crop, (float)(this.Mp.clms - this.Mp.crop * 2), (float)(this.Mp.rows - this.Mp.crop * 2));
		}

		public float floating_vib_pixel
		{
			get
			{
				return this.floating_vib_pixel_;
			}
			set
			{
				if (this.floating_vib_pixel == value)
				{
					return;
				}
				this.floating_vib_pixel_ = value;
				this.pos_reset = true;
			}
		}

		public bool beingCarriedByPr()
		{
			return this.FollowTo != null;
		}

		public NelM2DBase nM2D
		{
			get
			{
				return base.M2D as NelM2DBase;
			}
		}

		public bool bcc_enabled
		{
			get
			{
				return !this.hameDecided();
			}
		}

		public bool isPuttingByPr()
		{
			return this.FollowTo != null && this.t_putting > 0f;
		}

		public bool hameDecided()
		{
			return this.FollowTo == null && this.HameTarget != null;
		}

		public float t_fix_delay
		{
			get
			{
				return this.t_fix_delay_;
			}
			set
			{
				this.t_fix_delay_ = X.Mx(this.t_fix_delay_, value);
			}
		}

		public override int checkStuckInWall(ref M2BlockColliderContainer.BCCLine PreStackBcc_, bool extending = false)
		{
			return -1;
		}

		public NelChipPuzzleBox Parent;

		private float[] Afooted_t;

		private List<M2BlockColliderContainer.BCCLine>[] AFootedLine;

		private List<Vector4> ASlideDepert;

		private uint playfoot_bits;

		private M2BlockColliderContainer MyBCC;

		private M2MoverPr FollowTo;

		private const int POS_MEM_MAX = 5;

		private const int POS_MEM_FINE_INTV = 6;

		private float t_eyepos_trace = -1f;

		private Vector3[] Apos_mem;

		private int pos_mem_i;

		private float parent_shift_x;

		private float parent_shift_y;

		private float t_pfollow;

		private float t_following;

		private float t_footstamp_lock;

		private float v_gravity;

		private float t_vib;

		private float t_velocity_check;

		private float[] Asoft_fix_pos;

		private float floating_vib_pixel_;

		private static List<M2BlockColliderContainer.BCCLine> HitBcc;

		private BDic<uint, int> Oputpos;

		private static List<uint> Aput_buf;

		private int putting_x;

		private int putting_y;

		private Vector3 PuttingNear;

		private float t_putting;

		private float t_carryslide_x;

		private float t_carryslide_y;

		private const float TPUT_FADE = 30f;

		private const int PUT_MAX_SLEN = 6;

		private const int PUT_CAPCACITY = 61;

		private const float FLOAT_MARGIN = 0.08f;

		private const float FLOAT_CLIP_SPEED = 0.08f;

		private const float T_FIX_FADING = 40f;

		private float t_fix_position;

		private float t_fix_delay_;

		private static List<M2Puts> AHamePt;

		private static int slide_checking_aim = -1;

		public static List<M2BlockColliderContainer.BCCLine> ASlideChecking;

		private NelChipHamehame HameTarget;

		private int slideshift_aim;

		private Vector3 SlideShift;

		public Func<int, int, M2BlockColliderContainer.BCCLine, bool> FD_ThroughablePixel;

		public Func<int, int, M2BlockColliderContainer.BCCLine, bool> FD_ThroughablePixelWithSlide;

		private Func<M2BlockColliderContainer.BCCLine, Vector3, bool> FD_FnHitReturnableBcc;
	}
}
