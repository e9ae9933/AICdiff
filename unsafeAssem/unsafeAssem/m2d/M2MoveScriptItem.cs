using System;
using System.Text.RegularExpressions;
using evt;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2MoveScriptItem
	{
		public static int calcTime(int t)
		{
			return t * M2MoveScriptItem.base_frame_repeat;
		}

		public static float calcSpeed(float t)
		{
			return t / (float)M2MoveScriptItem.base_frame_repeat;
		}

		private string _R1
		{
			get
			{
				return this.Reg.Groups[1].Value;
			}
		}

		private string _R2
		{
			get
			{
				return this.Reg.Groups[2].Value;
			}
		}

		private string _R3
		{
			get
			{
				return this.Reg.Groups[3].Value;
			}
		}

		private string _R4
		{
			get
			{
				return this.Reg.Groups[4].Value;
			}
		}

		private string _R5
		{
			get
			{
				return this.Reg.Groups[5].Value;
			}
		}

		private string _R6
		{
			get
			{
				return this.Reg.Groups[6].Value;
			}
		}

		private string _R7
		{
			get
			{
				return this.Reg.Groups[7].Value;
			}
		}

		private string _R8
		{
			get
			{
				return this.Reg.Groups[8].Value;
			}
		}

		public M2MoveScriptItem(M2MoveScriptItem.TYPE _type, Match _Reg)
		{
			this.type = _type;
			this.Reg = _Reg;
			M2MoveScriptItem.TYPE type = this.type;
			if (type <= M2MoveScriptItem.TYPE.CAM_FINEIMMEDIATE)
			{
				switch (type)
				{
				case M2MoveScriptItem.TYPE.WAIT:
					this.mv_t = ((!M2MoveScriptItem.no_set_time) ? M2MoveScriptItem.calcTime(X.NmI(this._R1, 0, false, false)) : 0);
					return;
				case M2MoveScriptItem.TYPE.POSE:
					this.pose_key = this._R1;
					this.fix_pose_break_walk = this._R2.Length;
					this.vx = -1f;
					if (this._R3 != "")
					{
						this.pose_frame = X.NmI(this._R3, 0, false, false);
					}
					else
					{
						this.pose_frame = -1;
					}
					if (this._R4 != "")
					{
						this.vx = (float)X.NmI(this._R5, 0, false, false);
						return;
					}
					return;
				case M2MoveScriptItem.TYPE.MOVE:
					this.pose_frame = ((this._R8 == "`") ? (-2) : (-1));
					if (this._R1 != "")
					{
						this.add_x = (this.add_y = 1f);
					}
					if (this._R2 == "+=")
					{
						this.add_x = 1f;
					}
					else if (this._R2 == "-=")
					{
						this.add_x = -1f;
					}
					if (this._R4 == "+=")
					{
						this.add_y = 1f;
					}
					else if (this._R4 == "-=")
					{
						this.add_y = -1f;
					}
					this.mv_x = X.Nm(this._R3, 0f, false);
					this.mv_y = X.Nm(this._R5, 0f, false);
					this.setTimeOrSpeed(this._R6, this._R7);
					return;
				case M2MoveScriptItem.TYPE.MOVE_PT:
				case M2MoveScriptItem.TYPE.MOVE_PTX:
					this.pose_frame = ((this._R8 == "`") ? (-2) : (-1));
					if (!(this._R1 == "@"))
					{
						this.setTimeOrSpeed(this._R6, this._R7);
						return;
					}
					this.type = M2MoveScriptItem.TYPE.AIM;
					if (this._R6 == ":")
					{
						this.mv_t = M2MoveScriptItem.calcTime(X.NmI(this._R7, 0, false, false));
						return;
					}
					return;
				case M2MoveScriptItem.TYPE.MOVE_VELOCITY:
					this.mv_x = X.Nm(this._R1, 0f, true);
					this.mv_y = X.Nm(this._R2, 0f, true);
					if (TX.valid(this._R3) || TX.valid(this._R4))
					{
						this.vx = X.Nm(this._R3, 0f, false);
						this.vy = X.Nm(this._R4, 0f, false);
						return;
					}
					return;
				case M2MoveScriptItem.TYPE.MOVE_A:
					this.mv_x = 1f;
					this.add_x = X.Nm(this._R1, 0f, true);
					this.setTimeOrSpeed(this._R2, this._R3);
					return;
				case (M2MoveScriptItem.TYPE)7:
				case (M2MoveScriptItem.TYPE)8:
				case (M2MoveScriptItem.TYPE)9:
				case (M2MoveScriptItem.TYPE)12:
				case (M2MoveScriptItem.TYPE)24:
				case (M2MoveScriptItem.TYPE)26:
				case (M2MoveScriptItem.TYPE)27:
				case (M2MoveScriptItem.TYPE)28:
				case (M2MoveScriptItem.TYPE)29:
					break;
				case M2MoveScriptItem.TYPE.FLOAT:
					this.flag = this._R1 == "^";
					return;
				case M2MoveScriptItem.TYPE.FIXAIM:
					this.flag = this._R1 == "!";
					return;
				case M2MoveScriptItem.TYPE.AIM:
					this.aim = CAim.parseString(this._R1, 0);
					if (this._R2 != "")
					{
						this.mv_t = M2MoveScriptItem.calcTime(X.NmI(this._R2, 0, false, false));
						return;
					}
					return;
				case M2MoveScriptItem.TYPE.MOVEPAT:
					this.pose_key = this._R1.ToUpper();
					return;
				case M2MoveScriptItem.TYPE.SPRITE:
					this.pose_key = this._R1;
					return;
				case M2MoveScriptItem.TYPE.TURN:
					this.speed = (float)((this._R1.ToUpper() == "L") ? (-1) : 1);
					this.mv_x = -1f;
					this.mv_t = M2MoveScriptItem.calcTime(X.NmI(this._R2, 20, true, false));
					return;
				case M2MoveScriptItem.TYPE.SND:
					this.pose_key = this._R2;
					this.mv_x = (float)((this._R1 == "!") ? 0 : 1);
					return;
				case M2MoveScriptItem.TYPE.MANPU:
					this.pose_key = this._R1;
					this.manpu_wait = this._R2 != "";
					this.mv_t = M2MoveScriptItem.calcTime(X.NmI(this._R3, 0, false, false));
					return;
				case M2MoveScriptItem.TYPE.FOOTWAIT:
					return;
				case M2MoveScriptItem.TYPE.JUMP:
					this.mv_y = X.Nm(this._R1, 0f, false);
					this.mv_t = M2MoveScriptItem.calcTime(X.NmI(this._R2, 0, false, false));
					return;
				case M2MoveScriptItem.TYPE.AIM_TO_X:
					this.aim = -1;
					this.mv_x = X.Nm(this._R1, 0f, false);
					this.mv_y = X.Nm(this._R2, 0f, true);
					if (this._R3 != "")
					{
						this.mv_t = M2MoveScriptItem.calcTime(X.NmI(this._R3, 0, false, false));
						return;
					}
					return;
				case M2MoveScriptItem.TYPE.GRAVITY_TOGGLE:
					this.mv_x = (float)((this._R1 == "+") ? 0 : 1);
					return;
				case M2MoveScriptItem.TYPE.PTCST:
					this.pose_key = this._R2;
					if (this._R1 == "C")
					{
						this.aim = 1;
						return;
					}
					if (this._R1 == "T")
					{
						this.aim = 2;
						return;
					}
					this.aim = 0;
					return;
				case M2MoveScriptItem.TYPE.AIM_TO_MV:
					this.aim = -1;
					this.pose_key = this._R1;
					this.mv_y = X.Nm(this._R2, 0f, true);
					if (this._R4 != "")
					{
						this.mv_x = (float)((this._R3 == "::") ? 1 : 0);
						this.mv_t = M2MoveScriptItem.calcTime(X.NmI(this._R4, 0, false, false));
						return;
					}
					return;
				case M2MoveScriptItem.TYPE.ANIM_TS:
					this.speed = X.Nm(this._R1, 1f, true);
					return;
				default:
					if (type - M2MoveScriptItem.TYPE.CAM_FOCUS <= 2)
					{
						return;
					}
					break;
				}
			}
			else
			{
				if (type == M2MoveScriptItem.TYPE.LOADWAIT)
				{
					return;
				}
				if (type == M2MoveScriptItem.TYPE.DEBUG)
				{
					this.mv_t = M2MoveScriptItem.calcTime(X.NmI(this._R1, 0, false, false));
					return;
				}
				if (type == M2MoveScriptItem.TYPE.RESET)
				{
					return;
				}
			}
			X.de("M2MoveScriptItem - 不明な TYPE :" + this.type.ToString(), null);
		}

		private void setTimeOrSpeed(string header, string v)
		{
			if (header == null || M2MoveScriptItem.no_set_time)
			{
				this.mv_t = 0;
				return;
			}
			if (header.IndexOf("<<") == 0)
			{
				this.speed = M2MoveScriptItem.calcSpeed(X.Nm(v, 0f, false) * M2MoveScriptItem.Mp.CLEN);
				this.mv_t = -1;
				return;
			}
			if (header == "<")
			{
				this.speed = M2MoveScriptItem.calcSpeed(X.Nm(v, 0f, false));
				this.mv_t = -1;
				return;
			}
			this.mv_t = M2MoveScriptItem.calcTime(X.NmI(v, 0, false, false));
		}

		private bool setMoveDeparture(M2Mover Mv)
		{
			if (this.type != M2MoveScriptItem.TYPE.MOVE_PT && this.type != M2MoveScriptItem.TYPE.MOVE_PTX)
			{
				return true;
			}
			Map2d mp = Mv.Mp;
			Vector2 pos = mp.getPos(this._R2 + ((this._R3 != "") ? ("." + this._R3) : ""), 0f, 0f, Mv);
			if (pos.x == -1000f)
			{
				return false;
			}
			this.mv_x = pos.x * mp.CLEN + X.Nm(this._R4, 0f, false);
			if (this.type == M2MoveScriptItem.TYPE.MOVE_PTX)
			{
				this.add_y = 1E-05f + X.Nm(this._R5, 0f, false);
			}
			else
			{
				this.mv_y = pos.y * mp.CLEN + X.Nm(this._R5, 0f, false);
			}
			return true;
		}

		public static void initScript(M2Mover _Mv, bool _no_set_time = false)
		{
			M2MoveScriptItem.Mv = _Mv;
			M2MoveScriptItem.Phy = M2MoveScriptItem.Mv.getPhysic();
			M2MoveScriptItem.Mp = M2MoveScriptItem.Mv.Mp;
			M2MoveScriptItem.show_unknown_error = true;
			M2MoveScriptItem.no_set_time = _no_set_time;
		}

		public static void endScript()
		{
			M2MoveScriptItem.Mv = null;
			M2MoveScriptItem.Mp = null;
			M2MoveScriptItem.no_set_time = false;
		}

		public static M2MoveScriptItem readScript(string str)
		{
			M2MoveScriptItem m2MoveScriptItem = null;
			if (str == null)
			{
				return null;
			}
			bool flag = false;
			if (REG.match(str, M2MoveScriptItem.RegMultiplyCommand))
			{
				flag = true;
				str = (M2MoveScriptItem.next_script = REG.rightContext);
			}
			if (REG.match(str, M2MoveScriptItem.RegSpaceHead))
			{
				str = (M2MoveScriptItem.next_script = REG.rightContext);
			}
			else if (REG.match(str, M2MoveScriptItem.RegSetPose))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.POSE, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegPtcST))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.PTCST, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegMove))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.MOVE, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegMovePt))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.MOVE_PT, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegMovePtX))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.MOVE_PTX, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegFloat))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.FLOAT, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegLoadWait))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.LOADWAIT, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegFootWait))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.FOOTWAIT, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegFixAim))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.FIXAIM, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegMoveVelocity))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.MOVE_VELOCITY, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegMoveToAim))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.MOVE_A, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegWait))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.WAIT, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegAim))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.AIM, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegAimX))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.AIM_TO_X, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegAimMv))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.AIM_TO_MV, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegMovePattern))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.MOVEPAT, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegAnimTS))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.ANIM_TS, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegTurn))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.TURN, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegSnd))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.SND, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegManpu))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.MANPU, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegCamFocus))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.CAM_FOCUS, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegCamBlur))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.CAM_BLUR, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegCamFineImmediate))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.CAM_FINEIMMEDIATE, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegJump))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.JUMP, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegDebug))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.DEBUG, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegGravityToggle))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.GRAVITY_TOGGLE, REG.Get());
			}
			else if (REG.match(str, M2MoveScriptItem.RegReset))
			{
				M2MoveScriptItem.next_script = REG.rightContext;
				m2MoveScriptItem = new M2MoveScriptItem(M2MoveScriptItem.TYPE.RESET, REG.Get());
			}
			else
			{
				if (M2MoveScriptItem.show_unknown_error)
				{
					X.de("#MS ... 不明なスクリプトブロック..." + str, null);
					M2MoveScriptItem.show_unknown_error = false;
				}
				M2MoveScriptItem.next_script = TX.slice(str, 1);
			}
			if (m2MoveScriptItem != null)
			{
				M2MoveScriptItem.show_unknown_error = false;
				m2MoveScriptItem.multiply_command = flag;
			}
			if (m2MoveScriptItem == null || !m2MoveScriptItem.isActive())
			{
				return null;
			}
			return m2MoveScriptItem;
		}

		public bool run(M2MoveScript Ms, M2Mover Mv, float TS)
		{
			M2MoveScriptItem.TYPE type = this.type;
			if (type <= M2MoveScriptItem.TYPE.CAM_FINEIMMEDIATE)
			{
				switch (type)
				{
				case M2MoveScriptItem.TYPE.WAIT:
					this.setEventStop(Mv, true);
					this.t += TS;
					return this.t >= (float)this.mv_t;
				case M2MoveScriptItem.TYPE.POSE:
					if (this.t == 0f)
					{
						Mv.SpSetPose((this.pose_key == "") ? null : this.pose_key, -1, this.pose_key, false);
						if (this.pose_frame >= 0)
						{
							Mv.SpMotionReset(this.pose_frame);
						}
						if (this.fix_pose_break_walk > 0 && this.pose_key != "")
						{
							Mv.breakPoseFixOnWalk(this.fix_pose_break_walk);
						}
					}
					if (this.vx >= 0f)
					{
						this.t += TS;
						return (this.vx > 0f && this.vx <= this.t) || !Mv.SpPoseIs(this.pose_key);
					}
					return true;
				case M2MoveScriptItem.TYPE.MOVE:
				case M2MoveScriptItem.TYPE.MOVE_PT:
				case M2MoveScriptItem.TYPE.MOVE_PTX:
				case M2MoveScriptItem.TYPE.MOVE_A:
					if (this.t == 0f)
					{
						this.setEventStop(Mv, true);
						if (!this.setMoveDeparture(Mv))
						{
							return true;
						}
						if (this.add_x != 0f)
						{
							if (this.type == M2MoveScriptItem.TYPE.MOVE_A)
							{
								this.mv_x = Mv.mpf_is_right;
							}
							this.mv_x = Mv.drawx + this.mv_x * this.add_x;
						}
						if (this.add_y != 0f)
						{
							this.mv_y = Mv.drawy + this.mv_y * this.add_y;
						}
						else if (this.type == M2MoveScriptItem.TYPE.MOVE_A)
						{
							this.mv_y = Mv.drawy;
						}
						float num = this.mv_x - Mv.drawx;
						float num2 = this.mv_y - Mv.drawy;
						if (this.mv_t < 0 && this.speed > 0f)
						{
							this.mv_t = X.IntC(X.LENGTHXY(0f, 0f, num, num2) / this.speed);
						}
						if (this.mv_t > 2 && (num == 0f || num2 == 0f || X.Abs(num) == X.Abs(num2)))
						{
							this.aim = (int)CAim.get_aim2(0f, 0f, num, -num2, false);
							this.speed = X.LENGTHXY(0f, 0f, num / M2MoveScriptItem.Mp.CLEN, num2 / M2MoveScriptItem.Mp.CLEN) / (float)this.mv_t;
						}
						else
						{
							this.aim = -1;
							this.vx = num / (float)X.Mx(this.mv_t, 1) / M2MoveScriptItem.Mp.CLEN;
							this.vy = num2 / (float)X.Mx(this.mv_t, 1) / M2MoveScriptItem.Mp.CLEN;
						}
						M2MoveScriptItem.Mp.M2D.Cam.blurCenterIfFocusing(Mv);
						this.mv_x /= M2MoveScriptItem.Mp.CLEN;
						this.mv_y /= M2MoveScriptItem.Mp.CLEN;
						if (this.type == M2MoveScriptItem.TYPE.MOVE_PT || this.mv_t == 0)
						{
							this.add_x = (this.add_y = 0f);
						}
					}
					if (this.t >= (float)this.mv_t)
					{
						if (this.add_x == 0f && this.add_y == 0f && this.pose_frame == -1)
						{
							if (Mv.canStand((int)this.mv_x, (int)this.mv_y))
							{
								if (M2MoveScriptItem.Phy != null && Mv.base_gravity > 0f && M2MoveScriptItem.Phy.gravity_apply_velocity(1f) > 0f)
								{
									float footableY = M2MoveScriptItem.Mp.getFootableY(this.mv_x, (int)(this.mv_y - 0.5f), 12, true, this.mv_y, false, true, true, 0f);
									if (footableY > 0f)
									{
										this.mv_y = footableY - Mv.sizey;
									}
								}
								Mv.moveBy(this.mv_x - Mv.x, this.mv_y - Mv.y, true);
							}
							else
							{
								X.dl("M2MoveScriptItem <TYPE.MOVE> :: 移動先が通行できないため、移動を確定しません。", null, false, false);
							}
						}
						if (this.aim >= 0)
						{
							Mv.walkByAim(-1, 0f);
							if (M2MoveScriptItem.Phy != null)
							{
								M2MoveScriptItem.Phy.setWalkXSpeed(0f, false, true);
							}
						}
						else
						{
							Mv.walkBySpeed(0f, 0f);
							if (M2MoveScriptItem.Phy != null)
							{
								M2MoveScriptItem.Phy.setWalkXSpeed(0f, false, true);
							}
						}
						return true;
					}
					this.t += TS;
					if (this.aim >= 0)
					{
						if (M2MoveScriptItem.Phy != null)
						{
							float num3 = (float)CAim._XD(this.aim, 1) * this.speed;
							float num4 = (float)(-(float)CAim._YD(this.aim, 1)) * this.speed;
							if (num3 != 0f && X.Abs(this.vy) < 0.08f)
							{
								M2MoveScriptItem.Phy.setWalkXSpeed(num3 * TS, false, true);
								if (!Mv.fix_aim)
								{
									Mv.setAim((num3 > 0f) ? AIM.R : AIM.L, false);
								}
							}
							else
							{
								M2MoveScriptItem.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._NO_CONSIDER_WATER | FOCTYPE._CHECK_WALL | FOCTYPE._INDIVIDUAL, num3 * TS, num4 * TS, -1f, -1, 1, 0, -1, 0);
							}
						}
						else
						{
							Mv.walkByAim(this.aim, this.speed * TS);
						}
					}
					else
					{
						if (M2MoveScriptItem.Phy != null)
						{
							if (this.vx != 0f && X.Abs(this.vy) < 0.08f)
							{
								M2MoveScriptItem.Phy.setWalkXSpeed(this.vx * TS, false, true);
								if (!Mv.fix_aim)
								{
									Mv.setAim((this.vx > 0f) ? AIM.R : AIM.L, false);
								}
							}
							else
							{
								M2MoveScriptItem.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._NO_CONSIDER_WATER | FOCTYPE._CHECK_WALL | FOCTYPE._INDIVIDUAL, this.vx * TS, this.vy * TS, -1f, -1, 1, 0, -1, 0);
							}
						}
						else
						{
							Mv.walkBySpeed(this.vx * TS, this.vy * TS);
						}
						Mv.setAim(CAim.get_aim(0f, 0f, this.vx, this.vy, Mv.SpIsTetra()), false);
					}
					return false;
				case M2MoveScriptItem.TYPE.MOVE_VELOCITY:
					if (M2MoveScriptItem.Phy != null)
					{
						if (this.vx > 0f || this.vy > 0f)
						{
							M2MoveScriptItem.Phy.addFoc(FOCTYPE.WALK, this.mv_x, this.mv_y, -1f, (int)this.vx, (int)this.vy, 0, -1, 0);
						}
						else
						{
							M2MoveScriptItem.Phy.addFoc(FOCTYPE.WALK | FOCTYPE._RELEASE, this.mv_x, this.mv_y, -1f, -1, 1, 0, -1, 0);
						}
					}
					return true;
				case (M2MoveScriptItem.TYPE)7:
				case (M2MoveScriptItem.TYPE)8:
				case (M2MoveScriptItem.TYPE)9:
				case (M2MoveScriptItem.TYPE)12:
				case M2MoveScriptItem.TYPE.SPRITE:
				case (M2MoveScriptItem.TYPE)24:
				case (M2MoveScriptItem.TYPE)26:
				case (M2MoveScriptItem.TYPE)27:
				case (M2MoveScriptItem.TYPE)28:
				case (M2MoveScriptItem.TYPE)29:
					break;
				case M2MoveScriptItem.TYPE.FLOAT:
					Mv.floating = this.flag;
					if (M2MoveScriptItem.Phy != null)
					{
						if (this.flag)
						{
							M2MoveScriptItem.Phy.addLockWallHitting(Mv, -1f);
						}
						else
						{
							M2MoveScriptItem.Phy.remLockWallHitting(Mv);
						}
					}
					return true;
				case M2MoveScriptItem.TYPE.FIXAIM:
					Mv.fix_aim = this.flag;
					return true;
				case M2MoveScriptItem.TYPE.AIM:
				case M2MoveScriptItem.TYPE.AIM_TO_X:
				case M2MoveScriptItem.TYPE.AIM_TO_MV:
					if (this.type != M2MoveScriptItem.TYPE.AIM_TO_MV || this.mv_x != 1f)
					{
						if (this.t == 0f)
						{
							this.setEventStop(Mv, true);
							if (this.aim == -1 && this.Reg != null)
							{
								if (this.type == M2MoveScriptItem.TYPE.AIM_TO_MV)
								{
									M2Mover moverByName = M2MoveScriptItem.Mp.getMoverByName(this.pose_key, false);
									if (moverByName == null || moverByName == Mv)
									{
										return true;
									}
									this.aim = (int)CAim.get_aim2(0f, 0f, (Mv.x == moverByName.x) ? Mv.mpf_is_right : (moverByName.x - Mv.x), -this.mv_y, false);
								}
								else if (this.type == M2MoveScriptItem.TYPE.AIM_TO_X)
								{
									float num5 = Mv.drawx / Mv.CLEN;
									this.aim = (int)CAim.get_aim2(0f, 0f, (num5 == this.mv_x) ? Mv.mpf_is_right : (this.mv_x - num5), -this.mv_y, false);
								}
								else
								{
									if (!this.setMoveDeparture(Mv))
									{
										return true;
									}
									if (this.mv_x != Mv.drawx || this.mv_y != Mv.drawy)
									{
										this.aim = (int)CAim.get_aim(0f, -Mv.drawy, (Mv.drawx == this.mv_x) ? Mv.mpf_is_right : (this.mv_x - Mv.drawx), -this.mv_y, Mv.SpIsTetra());
									}
								}
								if (this.mv_t <= 0)
								{
									this.t = (float)(this.mv_t = 1);
								}
							}
							if (this.mv_t != 0)
							{
								this.mv_x = (float)Mv.SpAimGet();
								this.mv_y = 0f;
								this.speed = (float)((this.aim >= 0) ? CAim.get_dif(Mv.aim, (AIM)this.aim, 4) : 0);
							}
						}
						if (this.t >= (float)this.mv_t)
						{
							if (this.aim >= 0)
							{
								Mv.setAim((AIM)this.aim, true);
							}
							return true;
						}
						this.t += TS;
						if (this.speed != 0f && Mv.SpAimGet() != (AIM)this.aim && this.mv_y < (float)((int)(this.t / X.Mx((float)this.mv_t / X.Abs(this.speed), 1f))))
						{
							this.mv_y += 1f;
							AIM aim = CAim.get_clockwiseN((AIM)this.mv_x, (int)((float)((this.speed > 0f) ? 1 : (-1)) * this.mv_y));
							Mv.setAim(aim, true);
						}
					}
					else
					{
						if (this.t == 0f)
						{
							this.setEventStop(Mv, true);
							this.aim = 0;
						}
						if (this.aim <= (int)(this.t / 16f) || this.t >= (float)this.mv_t)
						{
							this.aim++;
							M2Mover moverByName2 = M2MoveScriptItem.Mp.getMoverByName(this.pose_key, false);
							if (moverByName2 == null || moverByName2 == Mv)
							{
								return true;
							}
							Mv.setAim(CAim.get_aim2(Mv.x, 0f, moverByName2.x, -this.mv_y, false), false);
						}
						if (this.t >= (float)this.mv_t)
						{
							return true;
						}
						this.t += TS;
					}
					return false;
				case M2MoveScriptItem.TYPE.MOVEPAT:
					if (Mv is M2EventItem)
					{
						(Mv as M2EventItem).setMovePattern(this.pose_key);
					}
					return true;
				case M2MoveScriptItem.TYPE.TURN:
					if (this.t == 0f)
					{
						this.setEventStop(Mv, true);
						this.mv_x = (float)Mv.SpAimGet();
						this.mv_y = 0f;
					}
					if (this.t >= (float)this.mv_t)
					{
						Mv.setAim((AIM)this.mv_x, true);
						return true;
					}
					this.t += TS;
					if (this.mv_y < (float)((int)(this.t / X.Mx((float)this.mv_t / 8f, 1f))))
					{
						this.mv_y += 1f;
						AIM aim2 = CAim.get_clockwiseN((AIM)this.mv_x, (int)((float)((this.speed > 0f) ? 1 : (-1)) * this.mv_y));
						Mv.setAim(aim2, true);
					}
					return false;
				case M2MoveScriptItem.TYPE.SND:
					if (this.mv_x == 1f)
					{
						Mv.playSndPos(this.pose_key, 1);
					}
					else
					{
						M2MoveScriptItem.Mp.M2D.Snd.stop(this.pose_key, Mv.snd_key);
					}
					return true;
				case M2MoveScriptItem.TYPE.MANPU:
					if (this.t == 0f)
					{
						M2Manpu m2Manpu = Mv.Mp.assignManpu(this.pose_key, Mv, this.mv_t);
						if (m2Manpu == null || !this.manpu_wait)
						{
							return true;
						}
						this.mv_t = m2Manpu.get_maxt();
					}
					if (this.t >= (float)this.mv_t)
					{
						return true;
					}
					this.t += TS;
					return false;
				case M2MoveScriptItem.TYPE.FOOTWAIT:
					return M2MoveScriptItem.Phy == null || Mv.hasFoot();
				case M2MoveScriptItem.TYPE.JUMP:
					if (this.t == 0f)
					{
						this.setEventStop(Mv, true);
						Mv.jumpByMoveScript(this.mv_y, this.t, (float)this.mv_t);
					}
					this.t += TS;
					return this.t >= (float)(this.mv_t * 2);
				case M2MoveScriptItem.TYPE.GRAVITY_TOGGLE:
					if (M2MoveScriptItem.Phy != null)
					{
						if (this.mv_x != 0f)
						{
							M2MoveScriptItem.Phy.addLockGravity(M2MoveScriptItem.TYPE.GRAVITY_TOGGLE, 0f, -1f);
						}
						else
						{
							M2MoveScriptItem.Phy.remLockGravity(M2MoveScriptItem.TYPE.GRAVITY_TOGGLE);
						}
					}
					return true;
				case M2MoveScriptItem.TYPE.PTCST:
					if (Mv is M2Attackable)
					{
						(Mv as M2Attackable).PtcVar("by", (double)Mv.mbottom).PtcST(this.pose_key, PtcHolder.PTC_HOLD.ACT, (PTCThread.StFollow)this.aim);
					}
					else
					{
						M2MoveScriptItem.Mp.PtcSTsetVar("cx", (double)Mv.x).PtcSTsetVar("cy", (double)Mv.y).PtcSTsetVar("by", (double)Mv.mbottom)
							.PtcST(this.pose_key, null, PTCThread.StFollow.NO_FOLLOW);
					}
					return true;
				case M2MoveScriptItem.TYPE.ANIM_TS:
					Ms.lock_remove_ms = true;
					Ms.ms_timescale = this.speed;
					return true;
				default:
					switch (type)
					{
					case M2MoveScriptItem.TYPE.CAM_FOCUS:
						if (EV.isActive(false))
						{
							M2MoveScriptItem.Mp.M2D.Cam.assignBaseMover(Mv, -1);
						}
						return true;
					case M2MoveScriptItem.TYPE.CAM_BLUR:
						if (EV.isActive(false) && M2MoveScriptItem.Mp.M2D.Cam.isBaseMover(Mv))
						{
							M2MoveScriptItem.Mp.M2D.Cam.assignBaseMover(null, -1);
						}
						return true;
					case M2MoveScriptItem.TYPE.CAM_FINEIMMEDIATE:
						if (EV.isActive(false))
						{
							M2MoveScriptItem.Mp.M2D.Cam.fineImmediately();
						}
						return true;
					}
					break;
				}
			}
			else
			{
				if (type == M2MoveScriptItem.TYPE.LOADWAIT)
				{
					return M2MoveScriptItem.Phy == null || M2MoveScriptItem.Mp.BCC == null || M2MoveScriptItem.Mp.BCC.is_prepared;
				}
				if (type != M2MoveScriptItem.TYPE.DEBUG)
				{
					if (type == M2MoveScriptItem.TYPE.RESET)
					{
						Mv.floating = Ms.pre_float;
						this.setEventStop(Mv, false);
						Mv.fix_aim = false;
						Map2d mp = Mv.Mp;
						Ms.lock_remove_ms = false;
						Ms.ms_timescale = 1f;
						if (mp != null)
						{
							mp.assignManpu("", Mv, 0);
						}
						Mv.breakPoseFixOnWalk(2);
						if (EV.isActive(false))
						{
							M2MoveScriptItem.Mp.M2D.Cam.resetBaseMoverIfFocusing(Mv, true);
						}
						if (M2MoveScriptItem.Phy != null)
						{
							M2MoveScriptItem.Phy.remLockGravity(M2MoveScriptItem.TYPE.GRAVITY_TOGGLE);
							M2MoveScriptItem.Phy.remLockWallHitting(Mv);
						}
						return true;
					}
				}
				else
				{
					if (this.t >= (float)this.mv_t)
					{
						return true;
					}
					this.t += TS;
					return false;
				}
			}
			return true;
		}

		public void setEventStop(M2Mover Mv, bool f)
		{
			if (Mv is M2EventItem)
			{
				(Mv as M2EventItem).event_stop = f;
			}
		}

		public void submit(M2MoveScript Ms, M2Mover Mv)
		{
			if (this.type == M2MoveScriptItem.TYPE.MOVE || this.type == M2MoveScriptItem.TYPE.MOVE_PT)
			{
				if (this.t == 0f && this.run(Ms, Mv, 1f))
				{
					return;
				}
				this.mv_t = 0;
				this.add_x = (this.add_y = 0f);
			}
			this.quit(Ms, Mv);
		}

		public void quit(M2MoveScript Ms, M2Mover Mv)
		{
			if (this.type == M2MoveScriptItem.TYPE.WAIT)
			{
				return;
			}
			if (this.type == M2MoveScriptItem.TYPE.MANPU)
			{
				return;
			}
			if (this.type == M2MoveScriptItem.TYPE.JUMP)
			{
				return;
			}
			if ((this.type == M2MoveScriptItem.TYPE.MOVE || this.type == M2MoveScriptItem.TYPE.MOVE_PT) && this.mv_t != 0)
			{
				M2Phys physic = Mv.getPhysic();
				if (physic != null)
				{
					physic.walk_xspeed = 0f;
				}
				return;
			}
			if (this.type == M2MoveScriptItem.TYPE.TURN || this.type == M2MoveScriptItem.TYPE.AIM || this.type == M2MoveScriptItem.TYPE.DEBUG)
			{
				this.mv_t = 0;
			}
			this.run(Ms, Mv, 1f);
		}

		public bool hasTime()
		{
			if (this.type == M2MoveScriptItem.TYPE.MANPU)
			{
				return this.pose_key != "" && this.manpu_wait;
			}
			return (this.type == M2MoveScriptItem.TYPE.WAIT || this.type == M2MoveScriptItem.TYPE.TURN || this.type == M2MoveScriptItem.TYPE.AIM || this.type == M2MoveScriptItem.TYPE.MOVE || this.type == M2MoveScriptItem.TYPE.MOVE_PT || this.type == M2MoveScriptItem.TYPE.JUMP) && this.mv_t > 0;
		}

		public bool TypeIs(M2MoveScriptItem.TYPE t)
		{
			return this.type == t;
		}

		public bool isActive()
		{
			return this.type >= M2MoveScriptItem.TYPE.WAIT;
		}

		public bool flag
		{
			get
			{
				return this.mv_t != 0;
			}
			set
			{
				this.mv_t = (value ? 1 : 0);
			}
		}

		public bool manpu_wait
		{
			get
			{
				return this.vx != 0f;
			}
			set
			{
				this.vx = (float)(value ? 1 : 0);
			}
		}

		public float speed
		{
			get
			{
				return this.vx;
			}
			set
			{
				this.vx = value;
			}
		}

		public int fix_pose_break_walk
		{
			get
			{
				return this.mv_t;
			}
			set
			{
				this.mv_t = value;
			}
		}

		public bool multiply_command;

		private float t;

		private readonly M2MoveScriptItem.TYPE type = M2MoveScriptItem.TYPE.NONE;

		internal string pose_key = "";

		internal float add_x;

		internal float add_y;

		internal float mv_x;

		internal float mv_y;

		internal int mv_t;

		internal float vx;

		internal float vy;

		internal int aim = -1;

		internal int pose_frame = -1;

		public static int base_frame_repeat = 1;

		private readonly Match Reg;

		private static bool show_unknown_error = false;

		private static bool no_set_time;

		private static M2Mover Mv;

		private static M2Phys Phy;

		private static Map2d Mp;

		private static readonly Regex RegSpaceHead = new Regex("^[ \\s\\t\\,]+");

		private static readonly Regex RegMultiplyCommand = new Regex("^ *\\*+ *");

		private static readonly Regex RegWait = new Regex("^[Ww]\\:?(\\d+)");

		private static readonly Regex RegSetPose = new Regex("^[Pp]\\[ *([\\$\\w]*) *(\\~*) *(?:\\=(\\d+))? *\\](?:(\\:)(\\d+)?)?");

		private static readonly Regex RegMove = new Regex("^>(\\+?)\\[ *([\\+\\-]\\=)? *([\\-\\d\\.]+)[\\, ]+([\\+\\-]\\=)? *([\\-\\d\\.]+) *(?:([\\:<]+) *([\\d\\.]+))? *\\](\\`?)");

		private static readonly Regex RegMoveToAim = new Regex("^>\\@\\[ *([\\-\\d\\.]+) *(?:([\\:<]+) *([\\d\\.]+))? *\\]");

		private static readonly Regex RegMovePt = new Regex("^(\\@?)>>\\[ *((?:\\#< *)?(?:\\%|\\%?[\\$\\w]+)(?: *>)?) *(?:\\. *(\\w+))? *(?:\\+?([\\-\\d]+)[\\, ]+\\+?([\\-\\d]+))? *(?:([\\:<]+) *([\\d\\.]+))? *\\](\\`?)");

		private static readonly Regex RegMovePtX = new Regex("^(\\@?)>>[xX]\\[ *((?:\\#< *)?(?:\\%|\\%?[\\$\\w]+)(?: *>)?) *(?:\\. *(\\w+))? *(?:\\+?([\\-\\d]+)[\\, ]+\\+?([\\-\\d]+))? *(?:([\\:<]+) *([\\d\\.]+))? *\\](\\`?)");

		private static readonly Regex RegMoveVelocity = new Regex("^<<\\[ *([\\-\\d\\.]+)[\\, ]+([\\-\\d\\.]+) *(?:\\:(\\d+)(?: *\\:(\\d+)))? *]");

		private static readonly Regex RegFloat = new Regex("^([\\^_])");

		private static readonly Regex RegFootWait = new Regex("^F");

		private static readonly Regex RegLoadWait = new Regex("^LOAD");

		private static readonly Regex RegFixAim = new Regex("^([\\!\\?])");

		private static readonly Regex RegAim = new Regex("^\\@(\\d+|[LTRB][LTRB]?) *(?:\\:(\\d+))?");

		private static readonly Regex RegAimX = new Regex("^\\@[xX]\\[ *(\\-?[\\d\\.]+)(?: *\\, *(\\-?[\\d\\.]+))? *(?:\\:(\\d+))? *\\]");

		private static readonly Regex RegAimMv = new Regex("^\\@\\#\\[ *([\\w\\%]+)(?: *\\, *(\\-?[\\d\\.]+))? *(?:(\\:\\:?)(\\d+))? *\\]");

		private static readonly Regex RegMovePattern = new Regex("^[mM]\\[ *([\\w\\$]*) *\\]");

		private static readonly Regex RegSetSprite = new Regex("^[sS]\\[ *(\\&?[\\w\\$]*) *\\]");

		private static readonly Regex RegAnimTS = new Regex("^[Tt][Ss]\\[ *([\\d\\.\\-]+) *\\]");

		private static readonly Regex RegTurn = new Regex("^[Tt]([RrLl]?)\\:?(\\d*)");

		private static readonly Regex RegSnd = new Regex("^[Ss][Nn][Dd](\\!)?\\[ *(\\w*) *\\]");

		private static readonly Regex RegManpu = new Regex("^[Mm][Pp]\\[ *([\\w]*) *(?:\\: *([Ww]?) *(\\d*))? *\\]");

		private static readonly Regex RegPtcST = new Regex("^[Pp][Tt][Cc]([CT]?)\\[ *([\\w]+) *\\]");

		private static readonly Regex RegJump = new Regex("^[jJ]\\[ *>* *([\\d\\.]+) *\\: *(\\d+) *\\]");

		private static readonly Regex RegCamFocus = new Regex("^\\#\\#");

		private static readonly Regex RegCamBlur = new Regex("^\\#\\~");

		private static readonly Regex RegCamFineImmediate = new Regex("^\\#\\;");

		private static readonly Regex RegDebug = new Regex("^[Dd]\\:?(\\d*)");

		private static readonly Regex RegReset = new Regex("^\\.");

		private static readonly Regex RegGravityToggle = new Regex("^([\\-\\+])G");

		public static string next_script;

		public enum TYPE
		{
			NONE = -1,
			WAIT,
			POSE,
			MOVE,
			MOVE_PT,
			MOVE_PTX,
			MOVE_VELOCITY,
			MOVE_A,
			FLOAT = 10,
			FIXAIM,
			AIM = 13,
			AIM_TO_X = 21,
			AIM_TO_MV = 25,
			MOVEPAT = 14,
			SPRITE,
			TURN,
			SND,
			MANPU,
			FOOTWAIT,
			JUMP,
			PTCST = 23,
			ANIM_TS = 30,
			GRAVITY_TOGGLE = 22,
			CAM_FOCUS = 50,
			CAM_BLUR,
			CAM_FINEIMMEDIATE,
			LOADWAIT = 60,
			DEBUG = 98,
			RESET
		}
	}
}
