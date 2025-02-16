using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2PuzzCarrierMover : ChipMover, IIrisOutListener, IBCCCarriedMover
	{
		public void initLp(M2LpPuzzCarrier _Lp, CarrierRailBlock Blk, int saim, string[] _Aptc_on_rail_stop = null)
		{
			this.Lp = _Lp;
			this.seed = (int)(X.xors() % 50U);
			this.Aptc_on_rail_stop = _Aptc_on_rail_stop;
			if (Blk != null && Blk.Length > 0)
			{
				this.aim = CAim.get_opposite((AIM)saim);
				this.off_turn_forward = false;
				this.RailBlk = Blk;
				this.move_maxt = Blk[0].len * this.Lp.duration_for_one_rail;
				this.movet = 0f;
				Blk.RegisterMover(this);
				this.state = M2PuzzCarrierMover.STATE.DEACTIVATE;
			}
			else
			{
				this.cur_rail_id = -1;
			}
			base.initTransEffecter();
			if (this.Phy == null)
			{
				this.Phy = new M2Phys(this, null);
				this.Phy.base_gravity = 0f;
			}
			this.Phy.water_speed_scale = 1f;
		}

		public override void appear(Map2d Mp)
		{
			base.appear(Mp);
			this.recalcPos((int)this.aim);
		}

		public bool recalcPos(int _aim)
		{
			if (this.RailBlk == null)
			{
				return false;
			}
			int length = this.RailBlk.Length;
			int num = (int)base.x;
			int num2 = (int)base.y;
			this.need_fine_position = false;
			this.cur_rail_id = -1;
			for (int i = 0; i < 2; i++)
			{
				int j = length;
				while (j >= 0)
				{
					if (!this.off_turn_forward)
					{
						if (j != length || this.first_rail_id >= 0)
						{
							if (j != this.first_rail_id)
							{
								goto IL_006A;
							}
						}
					}
					else if (j != length)
					{
						goto IL_006A;
					}
					IL_00CB:
					j--;
					continue;
					IL_006A:
					if (this.RailBlk[(j == length) ? this.first_rail_id : j].isOnRail(num, num2, (j == length) ? (-1) : _aim) >= 0)
					{
						this.cur_rail_id = ((j == length) ? this.first_rail_id : j);
						if (this.first_rail_id < 0)
						{
							this.first_rail_id = this.cur_rail_id;
						}
						this.recalcSpeed(false);
						return true;
					}
					goto IL_00CB;
				}
				if (_aim == -1)
				{
					break;
				}
				_aim = -1;
			}
			return false;
		}

		public bool recalcSpeed(bool check_turn_connection = false)
		{
			if (this.need_fine_position)
			{
				return this.recalcPos((int)this.aim);
			}
			if (this.cur_rail_id >= 0)
			{
				this.movet = 0f;
				int num = this.RailBlk.Length + 2;
				Vector2 zero = Vector2.zero;
				for (int i = 0; i < num; i++)
				{
					CarrierRailBlock.CarrierRail carrierRail = this.RailBlk[this.cur_rail_id];
					this.aim = (AIM)carrierRail.aim;
					Vector2 depertMapPos = this.getDepertMapPos();
					int num2 = (int)(X.LENGTHXYN(base.x, base.y, depertMapPos.x, depertMapPos.y) * (float)this.Lp.duration_for_one_rail);
					if (num2 > 1)
					{
						this.move_maxt = num2;
						return true;
					}
					zero = new Vector2(depertMapPos.x - base.x, depertMapPos.y - base.y);
					this.Phy.killSpeedForce(true, true, true, true, false);
					this.Phy.addFoc(FOCTYPE.RESIZE, zero.x, zero.y, -1f, -1, 1, 0, -1, 0);
					if (!this.off_turn_forward && check_turn_connection && this.first_rail_id >= 0 && this.RailBlk[this.first_rail_id].isOnRail((int)base.x, (int)base.y, -1) >= 0)
					{
						if (this.cur_rail_id == this.first_rail_id)
						{
							return false;
						}
						this.cur_rail_id = this.first_rail_id;
					}
					else if (this.off_turn_forward)
					{
						int num3 = this.cur_rail_id;
						if (this.cur_rail_id == this.RailBlk.Length - 1)
						{
							this.cur_rail_id = this.RailBlk.loop_to;
						}
						else
						{
							this.cur_rail_id++;
						}
						if (!this.Lp.loopback)
						{
							CarrierRailBlock.CarrierRail carrierRail2 = carrierRail;
							carrierRail = this.RailBlk[this.cur_rail_id];
							if (carrierRail2.mapx == carrierRail.mapx + CAim._XD(carrierRail.aim, carrierRail.len) && carrierRail2.mapy == carrierRail.mapy - CAim._YD(carrierRail.aim, carrierRail.len))
							{
								this.cur_rail_id = num3;
								return false;
							}
						}
					}
					else
					{
						if (this.cur_rail_id == this.first_rail_id)
						{
							return false;
						}
						if (this.cur_rail_id < this.first_rail_id)
						{
							if (this.cur_rail_id == this.RailBlk.Length - 1)
							{
								this.cur_rail_id = this.RailBlk.loop_to;
							}
							else
							{
								this.cur_rail_id++;
							}
						}
						else
						{
							if (this.cur_rail_id == 0)
							{
								return false;
							}
							this.cur_rail_id--;
						}
					}
				}
			}
			return false;
		}

		private Vector2 getDepertMapPos()
		{
			if (!this.off_turn_forward && this.cur_rail_id == this.first_rail_id)
			{
				return new Vector2(this.drawx0 / base.CLEN, this.drawy0 / base.CLEN);
			}
			CarrierRailBlock.CarrierRail carrierRail = this.RailBlk[this.cur_rail_id];
			if (this.off_turn_forward || this.cur_rail_id <= this.first_rail_id)
			{
				return new Vector2((float)carrierRail.mapx + 0.5f + (float)(carrierRail.len * CAim._XD(carrierRail.aim, 1)), (float)carrierRail.mapy + 0.5f - (float)(carrierRail.len * CAim._YD(carrierRail.aim, 1)));
			}
			return new Vector2((float)carrierRail.mapx + 0.5f, (float)carrierRail.mapy + 0.5f);
		}

		public override void runPre()
		{
			base.runPre();
			if (this.press_t > 0f)
			{
				if (this.EfHn != null)
				{
					this.EfHn.fine(100);
				}
				if (this.AMvPressing != null && this.CC != null)
				{
					for (int i = this.AMvPressing.Count - 1; i >= 0; i--)
					{
						M2Mover m2Mover = this.AMvPressing[i];
						if (m2Mover.isDestructed())
						{
							this.AMvPressing.Remove(m2Mover);
						}
						else
						{
							M2Phys physic = m2Mover.getPhysic();
							if (physic != null)
							{
								physic.addCollirderLock(this.CC.Cld, 30f, null, 1f);
							}
						}
					}
				}
				if (this.press_t >= 1000f)
				{
					this.press_t = X.Mx(this.press_t - 1f, 1000f);
				}
				else
				{
					bool flag = this.press_t > 120f;
					this.press_t -= Map2d.TS;
					if (flag && this.press_t <= 120f && this.AMvPressing != null)
					{
						for (int j = this.AMvPressing.Count - 1; j >= 0; j--)
						{
							M2Mover m2Mover2 = this.AMvPressing[j];
							if (m2Mover2.isDestructed())
							{
								this.AMvPressing.Remove(m2Mover2);
							}
							else if (this.AMvPressing.IndexOf(m2Mover2) >= 0 && !this.Phy.carrying_no_collider_lock)
							{
								m2Mover2.getPhysic().addCollirderLock((this.CC != null) ? this.CC.Cld : null, 122f, null, 0.5f);
							}
						}
					}
					if (this.press_t <= 0f)
					{
						this.stopPress();
					}
				}
			}
			else if (this.cur_rail_id >= 0)
			{
				if (this.ride_delay != 0f)
				{
					this.ride_delay = X.VALWALK(this.ride_delay, 0f, this.TS);
					if (this.ride_delay == 0f)
					{
						this.fineMovingState(true);
					}
				}
				switch (this.state)
				{
				case M2PuzzCarrierMover.STATE.DEACTIVATING:
				case M2PuzzCarrierMover.STATE.STOPPING:
				case M2PuzzCarrierMover.STATE.TURNING:
				{
					this.press_rail_id = -1;
					this.movet += Map2d.TS;
					Vector2 stabilizeDepertPos = base.getStabilizeDepertPos();
					float num = 1f / (float)this.Lp.duration_for_one_rail;
					this.Phy.addFoc(FOCTYPE.RESIZE, X.VALWALKMXR(base.x, stabilizeDepertPos.x, num, 0.25f) - base.x, X.VALWALKMXR(base.y, stabilizeDepertPos.y, num, 0.25f) - base.y, -1f, -1, 1, 0, -1, 0);
					if (this.state == M2PuzzCarrierMover.STATE.DEACTIVATING)
					{
						if (X.LENGTHXYS(stabilizeDepertPos.x, stabilizeDepertPos.y, base.x, base.y) < 0.0078125f)
						{
							this.changeState(M2PuzzCarrierMover.STATE.DEACTIVATE);
						}
					}
					else if (this.movet >= 20f)
					{
						if (this.state == M2PuzzCarrierMover.STATE.STOPPING)
						{
							this.changeState(M2PuzzCarrierMover.STATE.STOP);
						}
						else
						{
							this.changeState(M2PuzzCarrierMover.STATE.MOVING);
						}
					}
					break;
				}
				case M2PuzzCarrierMover.STATE.MOVING:
					if (this.move_maxt < 0)
					{
						this.movet -= Map2d.TS;
						if (this.movet <= (float)this.move_maxt)
						{
							if (!this.recalcSpeed(true))
							{
								this.changeState(M2PuzzCarrierMover.STATE.TURNED_HOME);
							}
							else
							{
								this.press_rail_id = this.cur_rail_id;
							}
						}
					}
					else
					{
						int k = 0;
						while (k < 3)
						{
							this.press_rail_id = this.cur_rail_id;
							if (this.movet >= (float)this.move_maxt)
							{
								int num2 = this.cur_rail_id;
								if (!this.recalcSpeed(true))
								{
									this.changeState(M2PuzzCarrierMover.STATE.TURNED_HOME);
									break;
								}
								if (num2 != this.cur_rail_id && this.RailBlk[this.cur_rail_id].stop)
								{
									if (num2 >= 0 && this.Aptc_on_rail_stop != null)
									{
										string text = null;
										int aim = this.RailBlk[num2].aim;
										if (this.Aptc_on_rail_stop.Length == 1)
										{
											text = this.Aptc_on_rail_stop[0];
										}
										else if (this.Aptc_on_rail_stop.Length > aim)
										{
											text = this.Aptc_on_rail_stop[aim];
										}
										if (TX.valid(text))
										{
											this.Mp.PtcSTsetVar("x", (double)base.x).PtcSTsetVar("y", (double)base.y);
											M2BlockColliderContainer bcccon = base.getBCCCon();
											if (bcccon != null)
											{
												Rect boundsShifted = bcccon.getBoundsShifted();
												this.Mp.PtcSTsetVar("mleft", (double)boundsShifted.x).PtcSTsetVar("mtop", (double)boundsShifted.y).PtcSTsetVar("mright", (double)boundsShifted.xMax)
													.PtcSTsetVar("mbottom", (double)boundsShifted.yMax);
											}
											this.Mp.PtcST(text, null, PTCThread.StFollow.NO_FOLLOW);
										}
									}
									this.Phy.killSpeedForce(true, true, true, true, false);
									this.move_maxt = X.Mn(-this.Lp.stop_maxt, -1);
									break;
								}
							}
							if (this.movet < (float)this.move_maxt)
							{
								this.movet += Map2d.TS;
								Vector2 depertMapPos = this.getDepertMapPos();
								float num3 = 1f / (float)this.Lp.duration_for_one_rail;
								float num4 = this.Mp.GAR(base.x, base.y, depertMapPos.x, depertMapPos.y);
								this.Phy.addFoc(FOCTYPE.JUMP | FOCTYPE._INDIVIDUAL, X.Cos(num4) * num3, -X.Sin(num4) * num3, -10f, -1, 1, 0, -1, 0);
								if (this.SndLoop != null && IN.totalframe % this.Lp.duration_for_one_rail == 0)
								{
									this.SndLoop.setPos(this.Lp.unique_key, this.snd_x, this.snd_y);
									break;
								}
								break;
							}
							else
							{
								k++;
							}
						}
					}
					break;
				case M2PuzzCarrierMover.STATE.STOP:
					this.press_rail_id = -1;
					break;
				}
			}
			if (this.fine_initialize_effect > 0)
			{
				if (this.fine_initialize_effect == 2)
				{
					if (this.Mp.floort >= 10f)
					{
						this.Mp.M2D.Snd.playAt("carrier_init", "", this.snd_x, this.snd_y, SndPlayer.SNDTYPE.SND, 1);
					}
					this.SndLoop = this.Mp.M2D.Snd.Environment.AddLoop("carrier_moving", this.Lp.unique_key, this.snd_x, this.snd_y, 6f, 6f, (float)this.Lp.mapw * 0.5f + 1f, (float)this.Lp.maph * 0.5f + 1f, null);
					base.particle_auto_setting = true;
				}
				else
				{
					this.SndLoop = this.Mp.M2D.Snd.Environment.RemLoop("carrier_moving", this.Lp.unique_key);
					this.Mp.M2D.Snd.playAt("carrier_stop", "", this.snd_x, this.snd_y, SndPlayer.SNDTYPE.SND, 1);
					base.particle_auto_setting = false;
				}
				this.fine_initialize_effect = 0;
			}
			if (this.colorful)
			{
				float num5 = X.ANMPT(20 + this.seed, 1f) * 6.2831855f;
				this.ColTemp.HSV(X.ANMPT(130 + this.seed, 1f) * 360f, 60f + X.COSIT((float)(110 + this.seed)) * 40f, 70f + X.COSIT((float)(99 + this.seed)) * 30f).setA1(1f);
				base.setShift(28f * X.Cos(num5) - 1f, 22f * X.Sin(num5));
				base.Col = this.ColTemp.C;
			}
		}

		public bool isOnRail()
		{
			return this.RailBlk != null;
		}

		public CarrierRailBlock getRailBlock()
		{
			return this.RailBlk;
		}

		public bool isActive()
		{
			return !this.isDeactiveState(this.state);
		}

		public Rect getBccAppliedArea(Rect BccRect)
		{
			if (this.RailBlk == null)
			{
				return BccRect;
			}
			BccRect.x -= (float)((int)base.x);
			BccRect.y -= (float)((int)base.y);
			DRect rc = this.RailBlk.Rc;
			BccRect.x += rc.x;
			BccRect.y += rc.y;
			BccRect.width += rc.width;
			BccRect.height += rc.height;
			return BccRect;
		}

		public void traceAreaAndSetDangerFlag()
		{
			if (!(this.CC is M2MvColliderCreatorCM) || this.RailBlk == null)
			{
				return;
			}
			Rect boundsShifted = (this.CC as M2MvColliderCreatorCM).BCC.getBoundsShifted();
			boundsShifted.x -= base.x;
			boundsShifted.y -= base.y;
			int length = this.RailBlk.Length;
			for (int i = 0; i < length; i++)
			{
				CarrierRailBlock.CarrierRail carrierRail = this.RailBlk[i];
				int num = carrierRail.mapx;
				int num2 = carrierRail.mapy;
				int num3 = num + 1;
				int num4 = num2 + 1;
				int num5 = CAim._XD(carrierRail.aim, 1);
				if (num5 != -1)
				{
					if (num5 == 1)
					{
						num += carrierRail.len;
					}
				}
				else
				{
					num -= carrierRail.len;
				}
				num5 = CAim._YD(carrierRail.aim, 1);
				if (num5 != -1)
				{
					if (num5 == 1)
					{
						num2 -= carrierRail.len;
					}
				}
				else
				{
					num2 += carrierRail.len;
				}
				this.Mp.setDangerousFlag(carrierRail.mapx + (int)boundsShifted.x, carrierRail.mapy + (int)boundsShifted.y, num3 - num + (int)boundsShifted.width, num4 - num2 + (int)boundsShifted.height);
			}
		}

		private void changeState(M2PuzzCarrierMover.STATE _state)
		{
			if (_state == M2PuzzCarrierMover.STATE.MOVING && !this.off_turn_forward && this.cur_rail_id == this.first_rail_id)
			{
				Vector2 depertMapPos = this.getDepertMapPos();
				if (X.LENGTHXYN(base.x, base.y, depertMapPos.x, depertMapPos.y) <= 0.0625f)
				{
					_state = M2PuzzCarrierMover.STATE.TURNED_HOME;
				}
			}
			if (_state == this.state)
			{
				return;
			}
			this.Phy.killSpeedForce(true, true, true, false, false);
			M2PuzzCarrierMover.STATE state = this.state;
			bool flag = base.particle_auto_setting || this.fine_initialize_effect == 2;
			this.state = _state;
			if (this.state == M2PuzzCarrierMover.STATE.MOVING)
			{
				if (this.state == M2PuzzCarrierMover.STATE.TURNING)
				{
					this.recalcPos((int)this.aim);
				}
				else
				{
					this.movet = X.Mn(this.movet, -1f);
					this.move_maxt = ((state == M2PuzzCarrierMover.STATE.TURNING) ? (-1) : (-X.Mx(1, this.Lp.activate_delay + 1)));
				}
			}
			else
			{
				this.movet = 0f;
			}
			if ((this.state == M2PuzzCarrierMover.STATE.STOP || this.state == M2PuzzCarrierMover.STATE.NOUSE) && this.EfHn != null)
			{
				this.EfHn.deactivate(false);
				this.EfHn = null;
			}
			if (flag != this.playsnd)
			{
				this.fine_initialize_effect = (this.playsnd ? 2 : 1);
			}
		}

		public bool fineMovingState(bool play_effect)
		{
			bool flag = this.Lp.sf_active && this.RailBlk != null;
			bool flag2 = this.Lp.pre_on;
			if (!this.Lp.ride_on && this.Lp.loopback && this.Lp.pre_on && this.Lp.ManageArea != null)
			{
				flag2 = this.Lp.ManageArea.isActivePuzzleArea();
				flag = flag && flag2;
			}
			if (flag)
			{
				flag = true;
				bool flag3 = false;
				int num = 0;
				if (this.Lp.ride_on)
				{
					if ((this.Lp.off_turn & M2LpPuzzCarrier.OFF_TURN.RIDE) == M2LpPuzzCarrier.OFF_TURN.NOUSE)
					{
						flag = flag && base.isCarryingAttacker() != flag2;
					}
					else
					{
						num += ((base.isCarryingAttacker() != flag2) ? (-100) : 1);
					}
				}
				if ((this.Lp.off_turn & M2LpPuzzCarrier.OFF_TURN.LINER) == M2LpPuzzCarrier.OFF_TURN.NOUSE)
				{
					flag = flag && this.RailBlk.liner_activated != flag2;
				}
				else
				{
					num += ((this.RailBlk.liner_activated != flag2) ? (-100) : 1);
				}
				bool flag4 = false;
				bool flag5 = false;
				if (flag && this.off_turn_forward != num <= 0)
				{
					this.need_fine_position = true;
					this.aim = CAim.get_opposite(this.aim);
					if (num <= 0)
					{
						this.off_turn_forward = true;
					}
					else
					{
						this.off_turn_forward = false;
					}
					flag5 = true;
				}
				if (this.isDeactiveState(this.state) || flag == this.isTempStopState(this.state))
				{
					if (flag)
					{
						if (this.state != M2PuzzCarrierMover.STATE.TURNING)
						{
							this.changeState(M2PuzzCarrierMover.STATE.MOVING);
						}
					}
					else
					{
						this.changeState((!this.isStasisState(this.state)) ? M2PuzzCarrierMover.STATE.STOPPING : M2PuzzCarrierMover.STATE.STOP);
					}
					flag3 = true;
				}
				else if (flag5)
				{
					if (this.isStasisState(this.state))
					{
						this.changeState(M2PuzzCarrierMover.STATE.MOVING);
					}
					else if (this.state != M2PuzzCarrierMover.STATE.TURNING && !flag4)
					{
						this.changeState(M2PuzzCarrierMover.STATE.TURNING);
					}
					flag3 = true;
				}
				return flag3;
			}
			if (!this.isDeactiveState(this.state))
			{
				this.changeState(M2PuzzCarrierMover.STATE.DEACTIVATING);
				return true;
			}
			return false;
		}

		protected bool isDeactiveState(M2PuzzCarrierMover.STATE _state)
		{
			return _state == M2PuzzCarrierMover.STATE.DEACTIVATING || _state == M2PuzzCarrierMover.STATE.DEACTIVATE || _state == M2PuzzCarrierMover.STATE.NOUSE;
		}

		protected bool isStoppingAnimState(M2PuzzCarrierMover.STATE _state)
		{
			return _state == M2PuzzCarrierMover.STATE.DEACTIVATING || _state == M2PuzzCarrierMover.STATE.STOPPING || _state == M2PuzzCarrierMover.STATE.TURNING;
		}

		protected bool isMovingState(M2PuzzCarrierMover.STATE _state)
		{
			return _state == M2PuzzCarrierMover.STATE.MOVING || _state == M2PuzzCarrierMover.STATE.TURNING;
		}

		protected bool isTempStopState(M2PuzzCarrierMover.STATE _state)
		{
			return _state == M2PuzzCarrierMover.STATE.STOP || _state == M2PuzzCarrierMover.STATE.STOPPING;
		}

		protected bool isStasisState(M2PuzzCarrierMover.STATE _state)
		{
			return _state == M2PuzzCarrierMover.STATE.STOP || _state == M2PuzzCarrierMover.STATE.DEACTIVATE || _state == M2PuzzCarrierMover.STATE.TURNED_HOME;
		}

		public bool liner_activated
		{
			get
			{
				return (this.RailBlk != null && this.RailBlk.liner_activated) || this.Lp.switch_activated;
			}
		}

		public override void runPost()
		{
			base.runPost();
		}

		public override int getPressAim(M2Mover Mv)
		{
			if (this.floating || this.press_t > 0f)
			{
				return -2;
			}
			if (this.press_rail_id < 0)
			{
				return -2;
			}
			return this.RailBlk[this.press_rail_id].aim;
		}

		public override bool publishPress(M2Attackable MvTarget, List<M2BlockColliderContainer.BCCLine> ATargetBcc, out bool stop_carrier)
		{
			int pressAim = this.getPressAim(MvTarget);
			bool is_alive = MvTarget.is_alive;
			stop_carrier = false;
			if (pressAim >= 0 && base.publishPress(MvTarget, ATargetBcc, out stop_carrier))
			{
				bool flag = MvTarget is PR;
				M2Phys physic = MvTarget.getPhysic();
				if (physic != null && this.CC != null)
				{
					physic.addCollirderLock(this.CC.Cld, 100f, null, 1f);
				}
				if (flag && !stop_carrier && ATargetBcc != null)
				{
					Vector2 zero = Vector2.zero;
					for (int i = ATargetBcc.Count - 1; i >= 0; i--)
					{
						M2BlockColliderContainer.BCCLine bccline = ATargetBcc[i];
						if (bccline.BCC.BelongTo as ChipMover == this)
						{
							switch (bccline.aim)
							{
							case AIM.L:
								zero.x = X.Mx(zero.x, MvTarget.mright - bccline.shifted_x);
								break;
							case AIM.T:
								zero.y = X.Mx(zero.y, MvTarget.mbottom - bccline.shifted_y);
								break;
							case AIM.R:
								zero.x = X.Mn(zero.x, MvTarget.mleft - bccline.shifted_x);
								break;
							case AIM.B:
								zero.y = X.Mn(zero.y, MvTarget.mtop - bccline.shifted_y);
								break;
							case AIM.LT:
							case AIM.TR:
							{
								float num = X.Mx(bccline.slopeBottomY(bccline.x_MMX_shifted(MvTarget.mleft)), bccline.slopeBottomY(bccline.x_MMX_shifted(MvTarget.mright)));
								zero.y = X.Mx(zero.y, MvTarget.mbottom - num);
								break;
							}
							case AIM.BL:
							case AIM.RB:
							{
								float num = X.Mn(bccline.slopeBottomY(bccline.x_MMX_shifted(MvTarget.mleft)), bccline.slopeBottomY(bccline.x_MMX_shifted(MvTarget.mright)));
								zero.y = X.Mn(zero.y, MvTarget.mtop - num);
								break;
							}
							}
						}
					}
					this.Phy.addTranslateStack(zero.x, zero.y);
				}
				else if (!stop_carrier)
				{
					return true;
				}
				this.press_t = (float)(flag ? ((!stop_carrier) ? 120 : ((!is_alive) ? 70 : 250)) : 140);
				AIM aim = CAim.get_opposite((AIM)pressAim);
				this.Phy.addFoc(FOCTYPE.WALK, 0.0005f * (float)CAim._XD(aim, 1), -0.012f * (float)CAim._YD(aim, 1), -2f, 0, 2, 24, -1, 0);
				if (this.AMvPressing == null)
				{
					this.AMvPressing = new List<M2Mover>(1);
				}
				this.AMvPressing.Add(MvTarget);
				this.initPress();
				if (this.Lp.pressback)
				{
					this.aim = aim;
					this.recalcPos((int)aim);
				}
				if (this.Lp.press_irisout && flag)
				{
					if (this.EfHn == null)
					{
						this.EfHn = new EffectHandlerPE(2);
					}
					this.EfHn.Set(PostEffect.IT.setPE(POSTM.ZOOM2, 150f, 1f, 5));
					this.iris_listener_assined = true;
					this.press_t = 1050f;
				}
				if (this.Lp.press_stop_same_layer_carrier)
				{
					for (int j = this.Mp.count_movers - 1; j >= 0; j--)
					{
						M2PuzzCarrierMover m2PuzzCarrierMover = this.Mp.getMv(j) as M2PuzzCarrierMover;
						if (!(m2PuzzCarrierMover == null) && m2PuzzCarrierMover.Lp.Lay == this.Lp.Lay)
						{
							m2PuzzCarrierMover.initPress();
							m2PuzzCarrierMover.press_t = this.press_t;
						}
					}
				}
				return true;
			}
			return false;
		}

		private void initPress()
		{
			if (this.press_rail_id >= 0)
			{
				int aim = this.RailBlk[this.press_rail_id].aim;
				int num = CAim._XD(aim, 1);
				int num2 = CAim._YD(aim, 1);
				if (num != 0)
				{
					this.TeCon.setQuakeSinH((float)(10 * num), 23, 0f, -7f, 0);
				}
				else
				{
					this.TeCon.setQuakeSinV((float)(-10 * num2), 23, 0f, -7f, 0);
				}
			}
			this.TeCon.setQuake(9f, 12, 5f, 0);
			this.TeCon.setQuake(2f, 80, 1f, 12);
			this.Mp.M2D.Snd.Environment.RemLoop("carrier_moving", this.Lp.unique_key);
			base.particle_auto_setting = false;
		}

		private void stopPress()
		{
			this.press_t = 0f;
			this.press_rail_id = this.cur_rail_id;
			this.AMvPressing = null;
			if (this.EfHn != null)
			{
				this.EfHn.deactivate(false);
				this.EfHn = null;
			}
			this.iris_listener_assined = false;
			if (this.playsnd)
			{
				this.SndLoop = this.Mp.M2D.Snd.Environment.AddLoop("carrier_moving", this.Lp.unique_key, this.snd_x, this.snd_y, 6f, 6f, (float)this.Lp.mapw * 0.5f + 1f, (float)this.Lp.maph * 0.5f + 1f, null);
				base.particle_auto_setting = true;
			}
		}

		public bool fineSF(bool apply_effect)
		{
			if (this.RailBlk == null)
			{
				return false;
			}
			this.Lp.fineSF(apply_effect);
			return true;
		}

		public override IFootable initCarry(ICarryable FootD)
		{
			IFootable footable = base.initCarry(FootD);
			if (footable != null)
			{
				if (this.Lp.ride_on && FootD is M2FootManager && (FootD as M2FootManager).Mv is M2Attackable)
				{
					this.ride_delay = ((this.ride_delay > 0f) ? this.ride_delay : 3f);
				}
				this.TeCon.setInitCarryBouncy(6f, 24, 0);
			}
			return footable;
		}

		public override void quitCarry(ICarryable FootD)
		{
			base.quitCarry(FootD);
			if (this.Lp == null)
			{
				return;
			}
			if (this.Lp.ride_on && !base.isCarryingAttacker())
			{
				this.ride_delay = ((this.ride_delay < 0f) ? this.ride_delay : (-3f));
			}
			if (this.TeCon != null)
			{
				this.TeCon.setInitCarryBouncy(4f, 24, 0);
			}
		}

		public void makeSnapShot(PuzzSnapShot.RevertItem Rvi)
		{
			Rvi.x = (float)X.IntR((this.drawx - this.drawx0) / base.CLEN);
			Rvi.y = (float)X.IntR((this.drawy - this.drawy0) / base.CLEN);
			Rvi.z = (float)(this.off_turn_forward ? 1 : 0);
			Rvi.time = (int)this.aim;
		}

		public void puzzleRevert(PuzzSnapShot.RevertItem Rvi)
		{
			if (base.destructed)
			{
				return;
			}
			this.setTo(this.drawx0 / base.CLEN + Rvi.x, this.drawy0 / base.CLEN + Rvi.y);
			this.setAim((AIM)Rvi.time, false);
			this.off_turn_forward = Rvi.z > 0f;
			this.resetChipsDrawPosition(0f, 0f);
			this.recalcPos((int)this.aim);
			if (this.press_t > 0f)
			{
				this.stopPress();
			}
		}

		public override void destruct()
		{
			if (base.destructed)
			{
				return;
			}
			this.iris_listener_assined = false;
			if (this.SndLoop != null)
			{
				this.SndLoop = this.Mp.M2D.Snd.Environment.RemLoop("carrier_moving", this.Lp.unique_key);
			}
			if (this.TeCon != null)
			{
				this.TeCon.destruct();
				this.TeCon = null;
				this.Mp.M2D.Snd.Environment.RemLoop("areasnd_carrier", this.Lp.unique_key);
			}
			if (this.EfHn != null)
			{
				this.EfHn.deactivate(false);
				this.EfHn = null;
			}
			if (this.RailBlk != null)
			{
				this.RailBlk.UnregisterMover(this);
			}
			this.RailBlk = null;
			base.destruct();
			this.cur_rail_id = -1;
			this.Lp = null;
		}

		public bool playsnd
		{
			get
			{
				return (this.state == M2PuzzCarrierMover.STATE.MOVING || this.state == M2PuzzCarrierMover.STATE.TURNING) && this.press_t <= 0f && this.off_turn_forward;
			}
		}

		public float snd_x
		{
			get
			{
				return base.dif_map_x + this.Lp.mapcx;
			}
		}

		public float snd_y
		{
			get
			{
				return base.dif_map_y + this.Lp.mapcy;
			}
		}

		public NelM2DBase NM2D
		{
			get
			{
				return M2DBase.Instance as NelM2DBase;
			}
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

		public bool initSubmitIrisOut(PR Pr, bool execute, ref bool no_iris_out)
		{
			return this.press_t > 0f;
		}

		public bool warpInitIrisOut(PR Pr, ref PR.STATE changestate, ref string change_pose)
		{
			if (this.press_t <= 0f)
			{
				return false;
			}
			change_pose = "water_choked_release2";
			changestate = PR.STATE.WATER_CHOKED_RELEASE;
			this.stopPress();
			this.NM2D.Iris.deassignListener(this);
			return true;
		}

		public IrisOutManager.IRISOUT_TYPE getIrisOutKey()
		{
			return IrisOutManager.IRISOUT_TYPE.PRESS;
		}

		public void debugLogRoot()
		{
			if (this.RailBlk == null)
			{
				return;
			}
			int length = this.RailBlk.Length;
			string text = "";
			for (int i = 0; i < length; i++)
			{
				CarrierRailBlock.CarrierRail carrierRail = this.RailBlk[i];
				string text2 = text;
				AIM aim = (AIM)carrierRail.aim;
				text = TX.add(text2, aim.ToString() + "へ" + carrierRail.len.ToString(), ", ");
			}
			X.dl(string.Concat(new string[]
			{
				this.RailBlk[0].mapx.ToString(),
				",",
				this.RailBlk[0].mapy.ToString(),
				" から ",
				text
			}), null, false, false);
		}

		private M2LpPuzzCarrier Lp;

		private CarrierRailBlock RailBlk;

		private int seed;

		private int cur_rail_id = -1;

		private int stop_delay;

		private int move_maxt;

		private float movet;

		private bool off_turn_forward;

		private bool need_fine_position = true;

		private const int TEMP_STOP_MAXT = 20;

		protected float press_t;

		private byte fine_initialize_effect;

		public int press_rail_id = -1;

		public bool colorful;

		public float ride_delay;

		public const int pressing_quit_carry_t = 120;

		private string[] Aptc_on_rail_stop;

		private List<M2Mover> AMvPressing;

		protected M2PuzzCarrierMover.STATE state;

		private M2SndLoopItem SndLoop;

		private int first_rail_id = -1;

		private C32 ColTemp = new C32();

		private EffectHandlerPE EfHn;

		private bool iris_listener_assined_;

		protected enum STATE
		{
			NOUSE,
			DEACTIVATING,
			DEACTIVATE,
			MOVING,
			STOP,
			STOPPING,
			TURNING,
			TURNED_HOME
		}
	}
}
