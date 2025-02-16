using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2ExtenderChecker : DRect, IRunAndDestroy, IBCCEventListener
	{
		public M2ExtenderChecker(string _name, Map2d _Mp, AIM _aim, float _shot_sx, float _shot_sy, float _hit_radius = 0.5f)
			: base(_name, 0f, 0f, 0f, 0f, 0f)
		{
			this.SetPos(_shot_sx, _shot_sy);
			this.ABccInFront = new List<M2BlockColliderContainer.BCCLine>();
			this.aim = _aim;
			this.hit_radius = _hit_radius;
			this.RcBuf = new DRect("buffer");
			this.Mp = _Mp;
			this.runner_assigned_ = true;
			this.Mp.addRunnerObject(this);
			this.Mp.addBCCEventListener(this);
			if (M2ExtenderChecker.ABufHit == null)
			{
				M2ExtenderChecker.ABufHit = new List<M2BlockColliderContainer.BCCHitInfo>(4);
			}
		}

		public M2Mover ActiveCarryCM
		{
			get
			{
				return this.ActiveCarryCM_;
			}
			set
			{
				if (this.ActiveCarryCM_ == value)
				{
					return;
				}
				this.ActiveCarryCM_ = value;
				if (this.ActiveCarryCM_ != null)
				{
					if (this.ABccInFront != null)
					{
						this.ABccInFront = null;
						this.Mp.remBCCEventListener(this);
						return;
					}
				}
				else if (this.ABccInFront == null)
				{
					this.ABccInFront = new List<M2BlockColliderContainer.BCCLine>();
					this.Mp.addBCCEventListener(this);
				}
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
				if (this.runner_assigned == value)
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

		public void destruct()
		{
			this.runner_assigned = false;
			if (this.ActiveCarryCM_ = null)
			{
				this.Mp.remBCCEventListener(this);
			}
		}

		public M2ExtenderChecker SetPos(float _shot_sx, float _shot_sy)
		{
			this.shot_sx = _shot_sx;
			this.shot_sy = _shot_sy;
			return this;
		}

		public bool calcShotStartPos(M2Chip Cp, out float shot_sx, out float shot_sy, int pivot_slide = 0)
		{
			switch (this.aim)
			{
			case AIM.L:
				shot_sx = Cp.mleft + (float)pivot_slide;
				shot_sy = Cp.mapcy;
				break;
			case AIM.T:
				shot_sy = Cp.mtop + (float)pivot_slide;
				shot_sx = Cp.mapcx;
				break;
			case AIM.R:
				shot_sx = Cp.mright - (float)pivot_slide;
				shot_sy = Cp.mapcy;
				break;
			case AIM.B:
				shot_sy = Cp.mbottom - (float)pivot_slide;
				shot_sx = Cp.mapcx;
				break;
			default:
				shot_sx = (shot_sy = 0f);
				return false;
			}
			return true;
		}

		public void BCCInitializing(M2BlockColliderContainer BCC)
		{
			this.RcBuf.key = "";
			if (this.ABccInFront == null || this.fully_extended)
			{
				return;
			}
			this.Fine(this.bcc_init_auto_fine_intv, false);
			this.need_fine_map_bcc = true;
			this.ABccInFront.Clear();
			if (this.ChipCheckingFn != null)
			{
				return;
			}
			if (!this.initted_ || this.reachable_max > 0f)
			{
				this.fineArea(this.RcBuf, (this.reachable_max < 0f) ? ((float)((CAim._XD(this.aim, 1) != 0) ? this.Mp.clms : this.Mp.rows)) : this.reachable_max);
				this.RcBuf.key = "_buf";
			}
		}

		public bool isBCCListenerActive(M2BlockColliderContainer.BCCLine BCC)
		{
			if (this.RcBuf.key == "")
			{
				return false;
			}
			if (BCC.is_lift)
			{
				return false;
			}
			if (BCC.isCovering(this.RcBuf, 0f))
			{
				if (((BCC._xd == 0) ? (BCC.y == this.shot_sy) : (BCC.x == this.shot_sx)) && BCC.aim == CAim.get_opposite(this.aim))
				{
					return false;
				}
				this.ABccInFront.Add(BCC);
			}
			return false;
		}

		public void BCCtouched(M2BlockColliderContainer.BCCLine BCC, M2FootManager FootD)
		{
		}

		public bool runBCCEvent(M2BlockColliderContainer.BCCLine BCC, M2FootManager FootD)
		{
			return false;
		}

		public void checkReachable()
		{
			bool flag = false;
			bool flag2 = false;
			if (!this.initted_)
			{
				flag = (this.initted_ = true);
				this.t_fine = -1f;
				if (this.aim < AIM.L)
				{
					return;
				}
				if (this.reachable_max <= 0f)
				{
					this.reachable_max = (float)((CAim._XD(this.aim, 1) != 0) ? this.Mp.clms : this.Mp.rows);
					this.calcBccReachableCached(ref this.reachable_max, true);
					this.reachable_max = (float)X.IntC(this.reachable_max);
					if (this.reachable_len_ >= 0f)
					{
						this.reachable_len_ = X.Mn(this.reachable_len_, this.reachable_max);
					}
					this.need_fine_map_bcc = false;
				}
				else
				{
					this.need_fine_map_bcc = true;
				}
				if (this.reachable_max <= 0f)
				{
					return;
				}
				if (this.reachable_len_ < 0f)
				{
					if (!this.auto_descend)
					{
						this.reachable_len_ = 0f;
					}
					else
					{
						this.reachable_len_ = this.reachable_max;
					}
				}
				this.fineArea(this.reachable_max);
				for (int i = this.Mp.count_carryable_bcc - 1; i >= 0; i--)
				{
					M2BlockColliderContainer carryableBCCByIndex = this.Mp.getCarryableBCCByIndex(i);
					Rect rect = carryableBCCByIndex.getBoundsShifted();
					if (!(carryableBCCByIndex.BelongTo == this.ActiveCarryCM_))
					{
						if (carryableBCCByIndex.BelongTo is IBCCCarriedMover)
						{
							rect = (carryableBCCByIndex.BelongTo as IBCCCarriedMover).getBccAppliedArea(rect);
						}
						if (base.isCovering(rect, 0f))
						{
							if (this.ABCCNeedCheck == null)
							{
								this.ABCCNeedCheck = new List<M2BlockColliderContainer>(1);
							}
							this.ABCCNeedCheck.Add(carryableBCCByIndex);
						}
					}
				}
			}
			this.t_fine = ((this.ABCCNeedCheck == null) ? (-1f) : this.fine_interval);
			if (this.auto_descend || this.reachable_len < this.reachable_max)
			{
				float num = (this.auto_ascend_ ? this.reachable_max : this.reachable_len_);
				if (this.need_fine_map_bcc)
				{
					this.need_fine_map_bcc = false;
					if (num > 0f)
					{
						this.calcBccReachableCached(ref num, false);
					}
				}
				if (num > 0f && this.ABCCNeedCheck != null)
				{
					for (int j = this.ABCCNeedCheck.Count - 1; j >= 0; j--)
					{
						this.calcBccReachable(this.ABCCNeedCheck[j], ref num, false);
					}
				}
				if (this.auto_ascend_ && num > this.reachable_len_)
				{
					this.reachable_len_ = num;
					flag2 = true;
				}
				if (this.auto_descend && num < this.reachable_len_)
				{
					this.reachable_len_ = num;
					flag2 = true;
				}
			}
			this.fineArea(-1f);
			if (this.fully_extended)
			{
				this.t_fine = -1f;
			}
			if (this.fnReachFined != null)
			{
				this.fnReachFined(flag, flag2);
			}
		}

		private void calcBccReachable(M2BlockColliderContainer BCC, ref float reachable, bool arrangeable_check = false)
		{
			this.calcBccReachable(BCC, this.shot_sx, this.shot_sy, this.shot_dx_cur(reachable), this.shot_dy_cur(reachable), ref reachable, arrangeable_check);
		}

		private void calcBccReachable(M2BlockColliderContainer BCC, float shot_sx, float shot_sy, float shot_dx, float shot_dy, ref float reachable, bool arrangeable_check = false)
		{
			M2ExtenderChecker.ABufHit.Clear();
			BCC.crosspoint(shot_sx, shot_sy, shot_dx, shot_dy, this.hit_radius + 0.125f, this.hit_radius + 0.125f, M2ExtenderChecker.ABufHit, true, null);
			if (M2ExtenderChecker.ABufHit.Count > 0)
			{
				for (int i = M2ExtenderChecker.ABufHit.Count - 1; i >= 0; i--)
				{
					M2BlockColliderContainer.BCCHitInfo bcchitInfo = M2ExtenderChecker.ABufHit[i];
					if (this.crossPointInfo(bcchitInfo.Hit, bcchitInfo.x, bcchitInfo.y, arrangeable_check))
					{
						switch (this.aim)
						{
						case AIM.L:
						case AIM.R:
							reachable = X.MMX(0f, (float)X.IntR(X.Abs(shot_sx - bcchitInfo.x)), reachable);
							break;
						case AIM.T:
						case AIM.B:
							reachable = X.MMX(0f, (float)X.IntR(X.Abs(shot_sy - bcchitInfo.y)), reachable);
							break;
						}
					}
				}
			}
		}

		private bool crossPointInfo(M2BlockColliderContainer.BCCLine C, float vx, float vy, bool arrangeable_check)
		{
			bool flag = false;
			M2BlockColliderContainer.BCCPos bccpos = default(M2BlockColliderContainer.BCCPos);
			if (this.mist_passable)
			{
				if (!flag)
				{
					flag = true;
					bccpos = M2BlockColliderContainer.BCCHitInfo.getHitChipS(C, vx, vy);
				}
				if (bccpos.valid && !CCON.isBlockSlope(bccpos.cfg) && CCON.mistPassable(bccpos.cfg, 2))
				{
					return false;
				}
			}
			if (arrangeable_check)
			{
				if (!flag)
				{
					bccpos = M2BlockColliderContainer.BCCHitInfo.getHitChipS(C, vx, vy);
				}
				if (bccpos.valid && bccpos.Cp.arrangeable)
				{
					this.map_bcc_dirty = true;
					return false;
				}
			}
			return true;
		}

		private void calcBccReachableCached(ref float reachable, bool arrangeable_check = false)
		{
			if (this.ChipCheckingFn != null)
			{
				int num = (int)(this.shot_sx + ((this.aim == AIM.L) ? (-0.001f) : 0f));
				int num2 = (int)(this.shot_sy + ((this.aim == AIM.T) ? (-0.001f) : 0f));
				int num3 = CAim._XD(this.aim, 1);
				int num4 = CAim._YD(this.aim, 1);
				int i = 0;
				while (i < (int)reachable)
				{
					M2Pt pointPuts = this.Mp.getPointPuts(num, num2, false, false);
					if (pointPuts != null && (!this.mist_passable || CCON.isBlockSlope(pointPuts.cfg) || !CCON.mistPassable(pointPuts.cfg, 2)) && !this.ChipCheckingFn(num, num2, pointPuts))
					{
						reachable = (float)i;
						return;
					}
					i++;
					num += num3;
					num2 -= num4;
				}
				return;
			}
			if (this.ABccInFront == null)
			{
				this.calcBccReachable(this.Mp.BCC, ref reachable, arrangeable_check);
				return;
			}
			if (this.ABccInFront.Count == 0)
			{
				return;
			}
			float num5 = 0f;
			float num6 = 0f;
			float num7 = 0f;
			float num8 = -1f;
			float shot_dx = this.shot_dx;
			float shot_dy = this.shot_dy;
			for (int j = this.ABccInFront.Count - 1; j >= 0; j--)
			{
				M2BlockColliderContainer.BCCLine bccline = this.ABccInFront[j];
				if (bccline.TwoPointCrossing(this.shot_sx, this.shot_sy, shot_dx, shot_dy, this.hit_radius, this.hit_radius, true))
				{
					if (num8 < 0f)
					{
						X.calcLineGraphVariable(this.shot_sx, this.shot_sy, shot_dx, shot_dy, out num5, out num8, out num6, out num7);
					}
					Vector3 vector = bccline.crosspointL(num5, num8, num6, num7, this.hit_radius, this.hit_radius);
					if (vector.z >= 2f && this.crossPointInfo(bccline, vector.x, vector.y, arrangeable_check))
					{
						switch (this.aim)
						{
						case AIM.L:
						case AIM.R:
							reachable = X.MMX(0f, (float)X.IntR(X.Abs(this.shot_sx - vector.x)), reachable);
							break;
						case AIM.T:
						case AIM.B:
							reachable = X.MMX(0f, (float)X.IntR(X.Abs(this.shot_sy - vector.y)), reachable);
							break;
						}
					}
				}
			}
		}

		public bool run(float fcnt)
		{
			if (this.t_fine > 0f)
			{
				this.t_fine = ((fcnt < 0f) ? 0f : X.Mx(this.t_fine - ((this.camera_clip && !this.isinCamera(2f)) ? 0.02f : 1f) * fcnt, 0f));
			}
			if (this.t_fine == 0f)
			{
				this.checkReachable();
			}
			if (this.t_fine < 0f && this.ActiveCarryCM_ == null)
			{
				this.runner_assigned_ = false;
				return false;
			}
			return true;
		}

		public void fineArea(float len = -1f)
		{
			this.fineArea(this, len);
		}

		private void fineArea(DRect Rc, float len)
		{
			if (len < 0f)
			{
				len = this.reachable_len_;
			}
			switch (this.aim)
			{
			case AIM.L:
				Rc.Set(this.shot_sx - len, this.shot_sy, len, 0f);
				return;
			case AIM.T:
				Rc.Set(this.shot_sx, this.shot_sy - len, 0f, len);
				return;
			case AIM.R:
				Rc.Set(this.shot_sx, this.shot_sy, len, 0f);
				return;
			case AIM.B:
				Rc.Set(this.shot_sx, this.shot_sy, 0f, len);
				return;
			default:
				return;
			}
		}

		public void Fine(float _time = 5f, bool abs_delay = false)
		{
			if (this.t_fine < 0f || abs_delay)
			{
				this.t_fine = _time;
			}
			else
			{
				this.t_fine = X.Mn(this.t_fine, _time);
			}
			this.runner_assigned = true;
		}

		public float reachable_len
		{
			get
			{
				return this.reachable_len_;
			}
			set
			{
				if (this.reachable_len == value)
				{
					return;
				}
				this.reachable_len_ = value;
				this.Fine(5f, false);
			}
		}

		public bool auto_ascend
		{
			get
			{
				return this.auto_ascend_;
			}
			set
			{
				if (this.auto_ascend == value)
				{
					return;
				}
				this.auto_ascend_ = value;
				if (value)
				{
					this.need_fine_map_bcc = true;
				}
			}
		}

		public bool isinCamera(float margin = 2f)
		{
			float num = this.reachable_max * 0.5f;
			float num2 = this.shot_dx_cur(num);
			float num3 = this.shot_dy_cur(num);
			float num4;
			float num5;
			if (CAim._XD(this.aim, 1) != 0)
			{
				num4 = num + this.hit_radius + margin;
				num5 = this.hit_radius + margin;
			}
			else
			{
				num4 = this.hit_radius + margin;
				num5 = num + this.hit_radius + margin;
			}
			return this.Mp.M2D.Cam.isCoveringMp(num2 - num4, num3 - num5, num2 + num4, num3 + num5, 0f);
		}

		public bool initted
		{
			get
			{
				return this.initted_;
			}
		}

		public float shot_dx
		{
			get
			{
				return this.shot_sx + this.reachable_max * (float)CAim._XD(this.aim, 1);
			}
		}

		public float shot_dy
		{
			get
			{
				return this.shot_sy - this.reachable_max * (float)CAim._YD(this.aim, 1);
			}
		}

		public bool fully_extended
		{
			get
			{
				return this.initted && ((!this.auto_ascend && this.reachable_len_ == 0f) || (!this.auto_descend && this.reachable_len_ >= this.reachable_max));
			}
		}

		public bool need_check_other_bcc
		{
			get
			{
				return this.ABCCNeedCheck != null;
			}
		}

		public float shot_dx_cur(float len = -1f)
		{
			if (len < 0f)
			{
				len = this.reachable_len_;
			}
			return this.shot_sx + len * (float)CAim._XD(this.aim, 1);
		}

		public float shot_dy_cur(float len = -1f)
		{
			if (len < 0f)
			{
				len = this.reachable_len_;
			}
			return this.shot_sy - len * (float)CAim._YD(this.aim, 1);
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				STB stb = TX.PopBld(null, 0);
				stb += "<ExtChecker> ";
				stb += this.key;
				this._tostring = stb.ToString();
				TX.ReleaseBld(stb);
			}
			return this._tostring;
		}

		public readonly Map2d Mp;

		private M2Mover ActiveCarryCM_;

		private bool auto_ascend_ = true;

		public bool auto_descend = true;

		public bool camera_clip;

		public bool mist_passable;

		public Func<int, int, M2Pt, bool> ChipCheckingFn;

		public float fine_interval = 10f;

		public float bcc_init_auto_fine_intv = 10f;

		public float shot_sx;

		public float shot_sy;

		public float hit_radius;

		public AIM aim;

		private bool initted_;

		public float reachable_max = -1f;

		private float reachable_len_ = -2f;

		private List<M2BlockColliderContainer.BCCLine> ABccInFront;

		private bool map_bcc_dirty;

		private static List<M2BlockColliderContainer.BCCHitInfo> ABufHit;

		private List<M2BlockColliderContainer> ABCCNeedCheck;

		public M2ExtenderChecker.FnReachFined fnReachFined;

		public bool need_fine_map_bcc;

		private bool runner_assigned_;

		private DRect RcBuf;

		private float t_fine;

		private string _tostring;

		public delegate void FnReachFined(bool initialized, bool changed);
	}
}
