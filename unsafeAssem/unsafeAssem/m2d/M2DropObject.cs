using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2DropObject : IRunAndDestroy, ICarryable
	{
		public M2DropObject(M2DropObjectContainer _Con)
		{
			this.Con = _Con;
			this.AFoc = new List<Vector3>(1);
		}

		public M2DropObject Set(M2DropObject.FnDropObjectDraw _Fndraw, float _x, float _y, float _vx, float _vy, float _z = -1f, float _time = 60f)
		{
			this.x = _x;
			this.y = _y;
			this.vx = _vx;
			this.vy = _vy;
			this.z = _z;
			this.size = 0.125f;
			this.af = 0f;
			this.wall_in = -1;
			this.af_ground = -1f;
			this.time = _time;
			this.Foot = (this.PreStackBcc = null);
			this.gravity_scale = 0.5f;
			this.check_thresh = 100f;
			this.bounce_x_reduce = 0.7f;
			this.bounce_x_reduce_when_ground = 1f;
			this.bounce_y_reduce = 0.65f;
			this.type = DROP_TYPE.KILL_IF_STOP;
			this.snd_key = "";
			this.snd_counter = 1;
			this.FnDraw = _Fndraw;
			this.x_bounced = (this.y_bounced = false);
			this.FnRun = null;
			this.Foot = null;
			this.AFoc.Clear();
			this.MyObj = null;
			this.FD_DropObjectBccTouch = null;
			return this;
		}

		public M2DropObject Type(DROP_TYPE d)
		{
			this.type = d;
			return this;
		}

		public M2DropObject TypeAdd(DROP_TYPE d)
		{
			this.type |= d;
			return this;
		}

		public M2DropObject TypeRem(DROP_TYPE d)
		{
			this.type &= ~d;
			return this;
		}

		public M2DropObject BounceReduce(float _bounce_x_reduce, float _bounce_y_reduce)
		{
			this.bounce_x_reduce = _bounce_x_reduce;
			this.bounce_y_reduce = _bounce_y_reduce;
			return this;
		}

		public bool run(float fcnt)
		{
			if ((this.type & DROP_TYPE.KILLED) != DROP_TYPE.NO_OPTION)
			{
				return this.destructTemp();
			}
			if (this.af < 0f)
			{
				this.af = X.Mn(this.af + fcnt, 0f);
				return true;
			}
			if (this.FnRun != null && !this.FnRun(this, fcnt))
			{
				return this.destructTemp();
			}
			this.y_bounced = (this.x_bounced = false);
			for (int i = this.AFoc.Count - 1; i >= 0; i--)
			{
				Vector3 vector = this.AFoc[i];
				if (vector.x * this.vx > 0f)
				{
					this.vx = X.VALWALK(this.vx, 0f, X.Abs(vector.x));
				}
				if (vector.y * this.vy > 0f)
				{
					this.vy = X.VALWALK(this.vy, 0f, X.Abs(vector.y));
				}
			}
			for (int j = this.AFoc.Count - 1; j >= 0; j--)
			{
				Vector3 vector2 = this.AFoc[j];
				if (vector2.z == 0f)
				{
					this.AFoc.RemoveAt(j);
				}
				else
				{
					if (vector2.z >= 0f)
					{
						vector2.z = X.VALWALK(vector2.z, 0f, fcnt);
						this.AFoc[j] = vector2;
					}
					this.vx += vector2.x;
					this.vy += vector2.y;
				}
			}
			M2BlockColliderContainer.BCCLine bccline = this.PreStackBcc;
			this.PreStackBcc = null;
			if (this.Mp.BCC == null)
			{
				return true;
			}
			if (this.size < 0f)
			{
				if (this.Foot != null)
				{
					this.changeRiding(this.Foot, true);
				}
				if (this.af_ground >= 0f)
				{
					this.af_ground = -1f;
				}
				this.y += this.vy * fcnt;
				this.wall_in = -1;
			}
			else
			{
				bool flag = false;
				int num = (int)this.x;
				if (this.wall_in < 0)
				{
					this.wall_in = (CCON.canStand(this.Mp.getConfig((int)this.x, (int)this.y)) ? 0 : 10);
				}
				if (this.wall_in > 0)
				{
					if (this.Foot != null)
					{
						this.changeRiding(this.Foot, true);
					}
					this.vx = (this.vy = 0f);
					Vector3 vector3 = M2BlockColliderContainer.extractFromStuck(this.Mp, this.x, this.y, ref bccline, 15, 0.5f, -1f, false);
					if (vector3.z < 0f)
					{
						this.wall_in--;
					}
					else
					{
						this.PreStackBcc = bccline;
						this.wall_in = 10;
						this.x = X.VALWALK(this.x, vector3.x, 0.14f);
						this.y = X.VALWALK(this.y, vector3.y, 0.14f);
						if (this.af_ground < 0f)
						{
							this.af_ground = 0f;
							flag = true;
						}
					}
				}
				else
				{
					bccline = null;
					if (this.Foot != null)
					{
						float num2 = ((this.vy != 0f) ? 0f : this.Foot.isFallable(this.x, this.y, this.size, this.size + 0.03f));
						if (num2 <= 0f)
						{
							this.check_thresh = X.Mx(0f, 0.25f - X.Abs(this.vx) - X.Abs(this.vy));
							this.changeRiding(this.Foot, false);
						}
						else
						{
							this.y = X.VALWALK(this.y, num2 + 0.031f, 0.02f * fcnt);
						}
					}
					if (this.Foot == null && (this.vy > 0f || (this.vy == 0f && this.gravity_scale != 0f)))
					{
						float num3 = 0f;
						float num4 = this.y;
						float num5 = 0.125f + X.Abs(this.vy * fcnt);
						int num6 = (int)(num4 + num5 + this.size);
						this.Foot = null;
						M2Pt pointPuts = this.Mp.getPointPuts(num, num6, true, false);
						if (pointPuts != null)
						{
							if ((this.type & DROP_TYPE.WATER_FLOAT) != DROP_TYPE.NO_OPTION && CCON.isWater(pointPuts.cfg))
							{
								num3 = ((!CCON.isWater(this.Mp.getConfig(num - 1, num6)) && !CCON.isWater(this.Mp.getConfig(num + 1, num6))) ? (this.y + 0.008f) : ((float)num6 - 0.008f));
							}
							if ((this.type & DROP_TYPE.CHECK_LIFT) != DROP_TYPE.NO_OPTION && pointPuts.isLift())
							{
								this.Mp.BCC.isFallable(this.x, num4, this.size, num5 + this.size + 0.5f, out this.Foot, false, true, -1f);
								if (this.Foot != null)
								{
									num3 = (float)num6 - this.size;
								}
							}
							if (num3 <= 0f)
							{
								M2BlockColliderContainer.BCCLine sideBcc = pointPuts.getSideBcc(this.Mp, num, num6, AIM.B);
								if (sideBcc != null)
								{
									num3 = sideBcc.isFallable(this.x, num4, this.size, this.size + num5);
									if (num3 > 0f)
									{
										this.Foot = sideBcc;
										num3 += num5;
									}
								}
								if (num3 <= 0f && pointPuts.bcc_line_cfg)
								{
									this.wall_in = -1;
								}
							}
						}
						if (num3 <= 0f && (this.type & DROP_TYPE.CHECK_OTHER_BCC) != DROP_TYPE.NO_OPTION)
						{
							num5 += 0.875f;
							int count_carryable_bcc = this.Mp.count_carryable_bcc;
							if (count_carryable_bcc > 0 && ((this.type & DROP_TYPE.REMOVE_MANUAL) != DROP_TYPE.NO_OPTION || this.camera_in))
							{
								this.check_thresh += X.Abs(this.vx) + X.Abs(this.vy) + 0.01f;
								if (this.check_thresh >= 0.25f)
								{
									num5 = X.Mx(0.3f, (this.check_thresh >= 100f) ? (this.check_thresh - 100f) : this.check_thresh);
									this.check_thresh = 0f;
									for (int k = count_carryable_bcc - 1; k >= 0; k--)
									{
										num3 = this.Mp.getCarryableBCCByIndex(k).isFallable(this.x, this.y, this.size, this.size + num5, out this.Foot, true, true, -1f);
										if (this.Foot != null)
										{
											num3 += num5;
											break;
										}
									}
								}
							}
						}
						if (num3 > 0f)
						{
							if (this.bounce_y_reduce == 0f)
							{
								this.y = num3;
								this.vy = 0f;
							}
							else
							{
								this.y = (((this.type & DROP_TYPE.REMOVE_MANUAL) != DROP_TYPE.NO_OPTION) ? X.VALWALK(this.y, num3, X.Abs(X.Mx(0.02f, this.vy) * fcnt)) : num3);
								this.vy = -this.vy * this.bounce_y_reduce;
							}
							this.y_bounced = true;
							if ((this.type & DROP_TYPE.KILL_IF_BOUNCE) != DROP_TYPE.NO_OPTION)
							{
								return this.destructTemp();
							}
							this.vx *= this.bounce_x_reduce_when_ground;
							if (this.snd_counter == 0 && this.snd_key != "")
							{
								this.Mp.playSnd(this.snd_key, "", this.x, this.y, 1);
							}
							if (this.vy > -0.005f)
							{
								this.vy = 0f;
								this.changeRiding(this.Foot, true);
								if (this.af_ground < 0f)
								{
									this.af_ground = 0f;
									flag = true;
								}
							}
							else
							{
								this.af_ground = -1f;
								if (this.FD_DropObjectBccTouch != null)
								{
									this.FD_DropObjectBccTouch(this, this.Foot, true);
								}
								this.Foot = null;
							}
						}
						else if (this.af_ground >= 0f)
						{
							this.changeRiding(this.Foot, false);
							this.af_ground = -1f;
						}
					}
					else if (this.Foot == null && this.af_ground >= 0f)
					{
						this.af_ground = -1f;
					}
				}
				if (this.af_ground >= 0f)
				{
					this.vy = 0f;
					this.check_thresh = 0f;
					if ((this.type & DROP_TYPE.GROUND_STOP_X) != DROP_TYPE.NO_OPTION)
					{
						this.vx = 0f;
					}
					else
					{
						this.vx = X.VALWALK(this.vx, 0f, 0.003f * fcnt);
					}
					this.y_bounced = true;
					if (this.af_ground < 0f)
					{
						this.af_ground = 0f;
					}
					if (!flag)
					{
						this.af_ground += fcnt;
					}
					this.snd_counter = 6;
				}
				else
				{
					this.vy += M2DropObject.getGravityVelocity(this.Mp, this.gravity_scale * fcnt);
					if (this.vy < 0f)
					{
						float num7 = this.y + this.vy - this.size - 0.02f;
						int num6;
						M2Pt pointPuts2 = this.Mp.getPointPuts(num, num6 = (int)num7, true, false);
						M2BlockColliderContainer.BCCLine bccline2 = ((pointPuts2 != null) ? pointPuts2.getSideBcc(this.Mp, num, num6, AIM.T) : null);
						if (bccline2 != null)
						{
							Vector3 vector4 = bccline2.crosspointL(1f, 0f, -this.x, 1f, this.size, this.size);
							if (vector4.z >= 2f && X.Abs(vector4.x - this.x) < this.size && X.Abs(vector4.y - 0.375f - this.y) < X.Abs(this.vy) + this.size + 0.375f)
							{
								this.y_bounced = true;
								if ((this.type & DROP_TYPE.KILL_IF_BOUNCE) != DROP_TYPE.NO_OPTION)
								{
									return this.destructTemp();
								}
								this.vy = -this.vy * this.bounce_y_reduce;
							}
						}
					}
					this.y += this.vy * fcnt;
					if (this.af_ground > 0f)
					{
						this.af_ground = -1f;
					}
					else
					{
						this.af_ground -= fcnt;
					}
					if (this.snd_counter > 0)
					{
						this.snd_counter--;
					}
					if (this.y >= (float)(this.Mp.rows - this.Mp.get_crop_value()) + this.size + 0.125f)
					{
						if ((this.type & DROP_TYPE.REMOVE_MANUAL) == DROP_TYPE.NO_OPTION)
						{
							return this.destructTemp();
						}
						this.y -= 0.0001f;
					}
				}
				if (this.vx != 0f)
				{
					int num8 = (int)this.y;
					int num6;
					M2Pt pointPuts3 = this.Mp.getPointPuts(num6 = (int)(this.x + this.vx + (this.size + 0.02f) * (float)X.MPF(this.vx > 0f)), num8, true, false);
					if (pointPuts3 != null)
					{
						if ((this.type & DROP_TYPE.WATER_BOUNCE_X) != DROP_TYPE.NO_OPTION && CCON.isWater(pointPuts3.cfg))
						{
							this.x_bounced = !CCON.isWater(this.Mp.getConfig(num6 - ((this.vx > 0f) ? 1 : (-1)), num8)) || this.x_bounced;
						}
						if (!this.x_bounced)
						{
							M2BlockColliderContainer.BCCLine sideBcc2 = pointPuts3.getSideBcc(this.Mp, num6, num8, (this.vx > 0f) ? AIM.R : AIM.L);
							if (sideBcc2 != null)
							{
								Vector3 vector5 = sideBcc2.crosspointL(0f, 1f, -this.y, 1f, this.size, this.size);
								if (vector5.z >= 2f && X.Abs(vector5.x + (float)X.MPF(this.vx > 0f) * 0.375f - this.x) < X.Abs(this.vx) + 0.375f + this.size && X.Abs(vector5.y - this.y) < this.size)
								{
									this.x_bounced = true;
								}
							}
						}
					}
					if (this.x_bounced)
					{
						if ((this.type & DROP_TYPE.KILL_IF_BOUNCE) != DROP_TYPE.NO_OPTION)
						{
							return this.destructTemp();
						}
						this.vx = -this.vx * this.bounce_x_reduce;
					}
				}
			}
			this.x += this.vx * fcnt;
			this.af += fcnt;
			return (this.type & DROP_TYPE.KILL_IF_STOP) == DROP_TYPE.NO_OPTION || this.af_ground < this.time || this.destructTemp();
		}

		public static float getGravityVelocity(Map2d Mp, float fcnt)
		{
			return fcnt * -Physics2D.gravity.y / 60f / 20f * 64f * Mp.rCLENB;
		}

		public void addFoc(float vx, float vy, float maxt)
		{
			this.AFoc.Add(new Vector3(vx, vy, maxt));
		}

		public void draw(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.FnDraw != null)
			{
				Ef.x = this.x;
				Ef.y = this.y;
				if (!Ed.isinCamera(Ef, 2f + this.size, 2f + this.size))
				{
					this.camera_in = false;
					return;
				}
				this.camera_in = true;
				if (!this.FnDraw(this, Ef, Ed))
				{
					this.destruct();
				}
			}
		}

		public bool moveWithFoot(float dx, float dy, Collider2D _Collider, M2BlockColliderContainer BCCCarrier, bool no_collider_lock = false)
		{
			if (this.killed)
			{
				return false;
			}
			this.x += dx;
			this.y += dy;
			return true;
		}

		public void setShiftPixel(IFootable F, float pixel_x, float pixel_y)
		{
		}

		public void initJump(bool recheck_foot = false, bool no_footstamp_snd = false, bool remain_foot_margin = false)
		{
			if (this.af_ground >= 0f)
			{
				this.af_ground = -1f;
			}
			this.changeRiding(this.Foot, false);
		}

		public M2BlockColliderContainer.BCCLine get_FootBcc()
		{
			return this.Foot;
		}

		public void destruct()
		{
			this.changeRiding(this.Foot, false);
			this.FnDraw = null;
			this.FnRun = null;
			this.type |= DROP_TYPE.KILLED;
		}

		private void changeRiding(M2BlockColliderContainer.BCCLine F, bool footing)
		{
			M2BlockColliderContainer.BCCLine foot = this.Foot;
			if (footing)
			{
				if (F == null)
				{
					return;
				}
				if (F.BCC.BelongTo != null)
				{
					if (F.BCC.BelongTo.initCarry(this) != null)
					{
						this.Foot = F;
					}
				}
				else
				{
					this.Foot = F;
				}
			}
			else
			{
				if (F != null)
				{
					F.quitCarry(this);
				}
				this.Foot = null;
			}
			if (foot != this.Foot && this.FD_DropObjectBccTouch != null)
			{
				this.FD_DropObjectBccTouch(this, this.Foot, false);
			}
		}

		public M2DropObject destruct(bool completely)
		{
			if (completely)
			{
				this.type &= (DROP_TYPE)(-33);
			}
			this.destruct();
			return null;
		}

		public bool destructTemp()
		{
			this.destruct();
			return this.continue_assign_runner;
		}

		public void colliderFined()
		{
			if ((this.type & DROP_TYPE.REMOVE_MANUAL) != DROP_TYPE.NO_OPTION)
			{
				this.wall_in = -1;
			}
			if (this.Foot != null && this.Foot.BCC == this.Mp.BCC)
			{
				this.Foot = null;
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.Con.Mp;
			}
		}

		public bool killed
		{
			get
			{
				return (this.type & DROP_TYPE.KILLED) > DROP_TYPE.NO_OPTION;
			}
		}

		public bool on_ground
		{
			get
			{
				return this.af_ground >= 0f;
			}
		}

		private bool continue_assign_runner
		{
			get
			{
				return !this.killed || (this.type & DROP_TYPE.REMOVE_MANUAL) > DROP_TYPE.NO_OPTION;
			}
		}

		public float CLEN
		{
			get
			{
				return this.Con.Mp.CLEN;
			}
		}

		public float CLENB
		{
			get
			{
				return this.Con.Mp.CLENB;
			}
		}

		public void fixToFootY(EffectItem Ef)
		{
			if (this.Foot != null)
			{
				if (this.Foot.aim == AIM.L || this.Foot.aim == AIM.R)
				{
					Ef.x = this.Foot.shifted_x;
					return;
				}
				Ef.y = this.Foot.slopeBottomY(Ef.x, this.Foot.BCC.base_shift_x, this.Foot.BCC.base_shift_y, true);
			}
		}

		public static bool fnDropRunDraw_PlayerDrip(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			uint ran = X.GETRAN2(Dro.index, Dro.index % 7);
			C32 c = EffectItem.Col1.Set(4278190080U | (uint)Dro.time).mulA(X.NI(0.8f, 1f, X.RAN(ran, 1938)));
			if (!Dro.on_ground)
			{
				MeshDrawer mesh = Ef.GetMesh("", MTRX.MIicon.getMtr(BLEND.NORMAL, -1), true);
				Ef.y += X.Mx(0f, Dro.size);
				mesh.initForImg(MTRX.EffCircle128, 0);
				mesh.Col = c.C;
				float num = X.Mx(1f, (float)(X.IntR(Dro.z) * 2));
				mesh.Rect(0f, 0f, num, num, false);
				if (Dro.af >= 180f)
				{
					Dro.destruct();
					return false;
				}
			}
			else
			{
				if (X.ZSIN(Dro.af_ground - 5f, 50f) >= 1f)
				{
					Dro.destruct();
					return false;
				}
				Dro.fixToFootY(Ef);
				MeshDrawer mesh2 = Ef.GetMesh("", MTRX.getMtr(BLEND.NORMAL, -1), true);
				mesh2.Col = c.C;
				mesh2.Scale(1f, 0.5f, false);
				mesh2.Arc(0f, 0f, 4f, -3.1415927f, 0f, 0f);
				mesh2.Identity();
			}
			return true;
		}

		public static void drawLayedEffectEgg(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed, PxlSequence AEgg)
		{
			uint ran = X.GETRAN2(Dro.index, Dro.index % 7);
			bool flag = X.RAN(ran, 2251) < 0.5f;
			int num = (int)(X.RAN(ran, 1956) * 4f);
			if (!Dro.on_ground)
			{
				MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, true);
				float num2 = X.GAR(0f, 0f, Dro.vx, Dro.vy + 0.02f);
				meshImg.RotaPF(0f, 0f, 2f, 2f, num2, AEgg[num + ((Dro.z > 0f) ? 8 : 0)], false, false, false, uint.MaxValue, false, 0);
				return;
			}
			Dro.fixToFootY(Ef);
			Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, true).RotaPF(0f, 0f, 2f, 2f, 0f, AEgg[num + 4], flag, false, false, uint.MaxValue, false, 0);
		}

		public static bool drawLayedEffectChild(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed, PxlSequence AChild)
		{
			uint ran = X.GETRAN2(Dro.index, Dro.index % 7);
			bool flag = Dro.z != 0f;
			if (!Dro.on_ground)
			{
				MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, false);
				int num = (int)(X.RAN(ran, 1956) * 8f);
				meshImg.RotaPF(0f, 0f, 2f, 2f, Dro.af / 40f * 6.2831855f * (float)(flag ? 1 : (-1)), AChild[8 + num], flag, false, false, uint.MaxValue, false, 0);
			}
			else
			{
				Dro.fixToFootY(Ef);
				MeshDrawer meshImg2 = Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, false);
				int num2 = (int)(X.RAN(ran, 1945) * 2f) * 4;
				meshImg2.RotaPF(0f, 0f, 2f, 2f, 0f, AChild[num2 + X.ANM((int)Dro.af_ground, 4, X.NI(3, 6, X.RAN(ran, 2560)))], flag, false, false, uint.MaxValue, false, 0);
			}
			return true;
		}

		public static bool runDraw_splash(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed, float size_min, float size_max, float ground_size_level, bool bottom_flag = true, float ground_fadein_t = 10f, float ground_hold_t = 20f, float ground_fadeout_t = 24f, float ground_yshift = -8f)
		{
			uint ran = X.GETRAN2(Dro.index, Dro.index % 7);
			int num = X.IntR(X.NI(size_min, size_max, X.RAN(ran, 1590)));
			C32 c = EffectItem.Col1.Set((uint)((ulong)(-16777216) | (ulong)((long)((int)Dro.time)))).mulA(X.NI(0.9f, 1f, X.RAN(ran, 1938)) * Dro.z);
			if (!Dro.on_ground)
			{
				Ef.y += X.Mx(0f, Dro.size);
				MeshDrawer mesh = Ef.GetMesh("", MTRX.MIicon.getMtr(BLEND.NORMAL, -1), bottom_flag || ran % 3U < 1U);
				mesh.initForImg(MTRX.EffCircle128, 0);
				mesh.Col = c.C;
				float num2 = (float)X.Mx(1, num * 2);
				mesh.Rect(0f, 0f, num2, num2, false);
				if (Dro.af >= 180f)
				{
					Dro.destruct();
					return false;
				}
			}
			else
			{
				float num3 = Dro.af_ground - 4f;
				if (num3 >= ground_fadein_t + ground_hold_t + ground_fadeout_t)
				{
					Dro.destruct();
					return false;
				}
				Dro.fixToFootY(Ef);
				float num4 = X.ZLINE(num3, ground_fadein_t);
				float num5 = X.ZLINE(num3 - ground_hold_t - ground_fadein_t, ground_fadeout_t);
				MeshDrawer mesh2 = Ef.GetMesh("", MTRX.getMtr(BLEND.NORMAL, -1), bottom_flag);
				mesh2.Col = c.mulA(num4 - num5).C;
				if (ground_yshift > 0f)
				{
					ground_yshift *= X.RAN(ran, 654);
				}
				mesh2.Scale(1f, 0.5f, false);
				mesh2.Arc(0f, ground_yshift, 1f + X.NI(0.5f, 1f, num4) * (float)num * ground_size_level, -3.1415927f, 0f, 0f);
				mesh2.Identity();
			}
			return true;
		}

		public static bool fnDropRunDraw_splash_blood(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			Dro.type |= DROP_TYPE.ALLOC_FOREVER;
			return M2DropObject.runDraw_splash(Dro, Ef, Ed, 0.5f, 4.5f, 3f, false, 4f, 90f, 130f, -7f);
		}

		public static bool fnDropRunDraw_splash_love_juice(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			return M2DropObject.runDraw_splash(Dro, Ef, Ed, 0.5f, 1.3f, 2f, false, 30f, 60f, 40f, -8f);
		}

		public static bool fnDropRunDraw_splash_sperma(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			float num = 1f;
			if (!Dro.on_ground)
			{
				num = 2f - X.ZSIN(Dro.af, 9f);
			}
			return M2DropObject.runDraw_splash(Dro, Ef, Ed, 1.5f * num, 5.3f * num, 2f, false, 4f, 50f, 40f, -8f);
		}

		public static bool fnDrawAtlasForDropObject(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed, M2ImageAtlas.AtlasRect _Atlas, MImage MI)
		{
			if (!_Atlas.valid || Dro.af_ground < -120f)
			{
				return false;
			}
			float num = X.ZLINE(Dro.af_ground - 10f, Dro.time - 10f);
			if (num >= 1f)
			{
				return false;
			}
			uint ran = X.GETRAN2(Dro.index, Dro.index % 7);
			float num2 = (float)((X.RAN(ran, 2985) < 0.5f) ? 4 : 2);
			float num3 = (float)((int)(X.RAN(ran, 1335) * num2)) / num2 * (float)_Atlas.w;
			float num4 = (float)((int)(X.RAN(ran, 2018) * num2)) / num2 * (float)_Atlas.h;
			MeshDrawer meshImg = Ef.GetMeshImg("", MI, BLEND.NORMAL, true);
			meshImg.initForImg(MI.Tx, (float)_Atlas.x + num3, (float)_Atlas.y + num4, (float)_Atlas.w / num2, (float)_Atlas.h / num2);
			float num5 = 6.2831855f / X.NI(50, 200, X.RAN(ran, 569)) * (float)X.MPF(X.RAN(ran, 2293) > 0.5f);
			float num6 = X.ZLINE(Dro.af_ground, 90f);
			float num7 = X.RAN(ran, 2196) * 6.2831855f + num5 * (Dro.af - Dro.af_ground * 0.5f);
			if (num6 > 0f)
			{
				num7 = X.correctangleR(num7);
				if (num7 > 2.3561945f || num7 < -2.3561945f)
				{
					num7 += X.angledifR(num7, 3.1415927f) * X.ZPOW(num6);
				}
				else if (num7 > 0.7853982f)
				{
					num7 += X.angledifR(num7, 1.5707964f) * X.ZPOW(num6);
				}
				else if (num7 > -0.7853982f)
				{
					num7 += X.angledifR(num7, 0f) * X.ZPOW(num6);
				}
				else
				{
					num7 += X.angledifR(num7, -1.5707964f) * X.ZPOW(num6);
				}
			}
			meshImg.Col = meshImg.ColGrd.Set(4292072403U).blend(0U, num).C;
			meshImg.RotaGraph(0f, (float)((num2 == 4f) ? 2 : 7), 1f, num7, null, false);
			return true;
		}

		public readonly M2DropObjectContainer Con;

		public int index;

		public float x;

		public float y;

		public float vx;

		public float vy;

		public float z;

		public float size;

		public float time;

		public float af;

		public float af_ground;

		public float gravity_scale = 0.5f;

		public float bounce_x_reduce = 0.7f;

		public float bounce_y_reduce = 0.65f;

		public float bounce_x_reduce_when_ground = 1f;

		public string snd_key = "";

		public float check_thresh;

		public int snd_counter;

		public DROP_TYPE type;

		public M2DropObject.FnDropObjectDraw FnDraw;

		public M2DropObject.FnDropObjectRun FnRun;

		private M2BlockColliderContainer.BCCLine Foot;

		private M2BlockColliderContainer.BCCLine PreStackBcc;

		public object MyObj;

		private readonly List<Vector3> AFoc;

		public M2DropObject.FnDropObjectBccTouch FD_DropObjectBccTouch;

		public bool x_bounced;

		public bool y_bounced;

		public bool water_float;

		public bool camera_in;

		public int wall_in;

		public delegate bool FnDropObjectDraw(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed);

		public delegate bool FnDropObjectRun(M2DropObject Dro, float fcnt);

		public delegate void FnDropObjectBccTouch(M2DropObject Dro, M2BlockColliderContainer.BCCLine Bcc, bool bounce);
	}
}
