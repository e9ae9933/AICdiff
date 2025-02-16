using System;
using System.Collections.Generic;
using XX;

namespace m2d
{
	public class BCCLineChipBased : IRunAndDestroy
	{
		public M2BlockColliderContainer BCC
		{
			get
			{
				return this.Bcc.BCC;
			}
		}

		public BCCLineChipBased(M2Chip _Cp, AIM base_aim, bool is_lift = true)
		{
			this.Cp = _Cp;
			this.is_tate = this.Cp.rotation % 2 != 0;
			AIM aim = CAim.get_clockwiseN(base_aim, this.Cp.rotation * 2);
			this.width = (float)((CAim._XD(aim, 1) != 0) ? this.Cp.iheight : this.Cp.iwidth) / this.Mp.CLEN;
			M2BlockColliderContainer m2BlockColliderContainer = ((this.Cp.AttachCM != null) ? this.Cp.AttachCM.getBCCCon() : this.Mp.BCC);
			this.Bcc = new M2BlockColliderContainer.BCCLine(m2BlockColliderContainer);
			this.Bcc.Initialize(-1, new M2BorderPt().Init(0f, 0f), new M2BorderPt().Init(1f, 0f), false, is_lift);
			this.Bcc.aim = aim;
			this.Bcc.fixFootStampChip(this.Cp, is_lift ? 120 : 128);
			if (this.Cp is IBCCEventListener)
			{
				this.Bcc.addBCCEventListener(this.Cp as IBCCEventListener, false);
			}
			if (this.Cp is IBCCFootListener)
			{
				this.Bcc.addBCCFootListener(this.Cp as IBCCFootListener, false);
			}
		}

		public BCCLineChipBased initAction()
		{
			this.need_fine_pos |= 4;
			this.Bcc.BCC.addAdditionalLine(this.Bcc, true);
			if (this.assign_runner)
			{
				this.Mp.addRunnerObject(this);
			}
			this.finePositionExecute(true);
			return this;
		}

		private void deactivate()
		{
			this.Bcc.BCC.remAdditionalLine(this.Bcc);
			this.Mp.remRunnerObject(this);
		}

		public void closeAction()
		{
			this.deactivate();
			this.finePositionExecute(true);
		}

		public void finePositionExecute(bool immediate = false)
		{
			AIM aim = this.Bcc.aim;
			if (this.need_fine_pos == 0)
			{
				return;
			}
			if ((this.need_fine_pos & 4) != 0)
			{
				this.need_fine_pos = 0;
				float num = this.width * 0.5f;
				float num2 = this.mapcx;
				float mapcy = this.mapcy;
				float num3 = num;
				float num4 = num2;
				float num5 = mapcy;
				AIM aim2 = CAim.get_clockwise2(aim, false);
				AIM aim3 = CAim.get_clockwise2(aim, true);
				bool flag;
				this.Bcc.initPosition(num4 + (float)CAim._XD(aim2, 1) * num3, num5 - (float)CAim._YD(aim2, 1) * num3, num4 + (float)CAim._XD(aim3, 1) * num3, num5 - (float)CAim._YD(aim3, 1) * num3, out flag);
			}
			if ((this.need_fine_pos & 2) != 0)
			{
				this.need_fine_pos -= 2;
				if (CAim.is_naname(aim))
				{
					this.need_fine_pos |= 1;
				}
				else
				{
					float num2 = this.width * 0.5f;
					float num = this.mapcx;
					float mapcy2 = this.mapcy;
					float num3 = num2;
					float num4 = num;
					float num5 = mapcy2;
					float num6 = 0f;
					bool flag2 = this.fixBccFromAim2(aim, num4, num5, num3, ref num6, immediate);
					if ((this.need_fine_pos & 16) != 0)
					{
						this.need_fine_pos -= 16;
						if (this.ARider != null && num6 != 0f)
						{
							float num7 = 0f;
							if (aim == AIM.L || aim == AIM.R)
							{
								float num8 = num6;
								num6 = 0f;
								num7 = num8;
							}
							for (int i = this.ARider.Count - 1; i >= 0; i--)
							{
								M2FootManager m2FootManager = this.ARider[i] as M2FootManager;
								if (m2FootManager != null)
								{
									m2FootManager.Mv.moveWithFoot(num6, num7, null, this.BCC, true, false);
								}
							}
						}
					}
					if (flag2)
					{
						this.need_fine_pos |= 2;
					}
				}
			}
			if ((this.need_fine_pos & 1) != 0)
			{
				this.need_fine_pos -= 1;
				float num9 = 0f;
				float num10 = 0f;
				bool flag3 = (this.need_fine_pos & 24) > 0;
				bool flag4;
				switch (aim)
				{
				case AIM.LT:
				{
					float num2 = this.width * 0.5f;
					float num = this.mapcx;
					float mapcy3 = this.mapcy;
					float num3 = num2;
					float num4 = num;
					float num5 = mapcy3;
					flag4 = this.fixBccFromAim2(AIM.T, num4, num5, num3, ref num9, immediate);
					flag4 = this.fixBccFromAim2(AIM.L, num4, num5, num3, ref num10, immediate) || flag4;
					break;
				}
				case AIM.TR:
				{
					float num2 = this.width * 0.5f;
					float num = this.mapcx;
					float mapcy4 = this.mapcy;
					float num3 = num2;
					float num4 = num;
					float num5 = mapcy4;
					flag4 = this.fixBccFromAim2(AIM.T, num4, num5, num3, ref num9, immediate);
					flag4 = this.fixBccFromAim2(AIM.R, num4, num5, num3, ref num10, immediate) || flag4;
					break;
				}
				case AIM.BL:
				{
					float num = this.width * 0.5f;
					float num2 = this.mapcx;
					float mapcy5 = this.mapcy;
					float num3 = num;
					float num4 = num2;
					float num5 = mapcy5;
					flag4 = this.fixBccFromAim2(AIM.B, num4, num5, num3, ref num9, immediate);
					flag4 = this.fixBccFromAim2(AIM.L, num4, num5, num3, ref num10, immediate) || flag4;
					break;
				}
				case AIM.RB:
				{
					float num = this.width * 0.5f;
					float num2 = this.mapcx;
					float mapcy6 = this.mapcy;
					float num3 = num;
					float num4 = num2;
					float num5 = mapcy6;
					flag4 = this.fixBccFromAim2(AIM.B, num4, num5, num3, ref num9, immediate);
					flag4 = this.fixBccFromAim2(AIM.R, num4, num5, num3, ref num10, immediate) || flag4;
					break;
				}
				default:
					flag3 = (this.need_fine_pos & 8) > 0;
					flag4 = this.fixBccFromAim1(aim, ref num9, immediate);
					if (aim == AIM.B || aim == AIM.T)
					{
						float num11 = num9;
						num9 = 0f;
						num10 = num11;
					}
					break;
				}
				if (flag3)
				{
					this.need_fine_pos = (byte)((int)this.need_fine_pos & -25);
					if (this.ARider != null && (num9 != 0f || num10 != 0f))
					{
						for (int j = this.ARider.Count - 1; j >= 0; j--)
						{
							M2FootManager m2FootManager2 = this.ARider[j] as M2FootManager;
							if (m2FootManager2 != null)
							{
								m2FootManager2.Mv.moveWithFoot(num9, num10, null, this.BCC, true, false);
							}
						}
					}
				}
				if (flag4)
				{
					this.need_fine_pos |= 1;
				}
			}
		}

		private bool fixBccFromAim2(AIM aim, float cx, float cy, float wh, ref float translated, bool immediate = false)
		{
			bool flag = false;
			switch (aim)
			{
			case AIM.L:
			{
				float num = cy - wh;
				float y = this.Bcc.y;
				float num2 = num;
				float num3 = y;
				if (this.y_follow_speed_ >= 1f || immediate || X.Abs(this.Bcc.y - num2) < 0.04f)
				{
					this.Bcc.y = (this.Bcc.sy = num2);
				}
				else
				{
					this.Bcc.y = (this.Bcc.sy = X.MULWALK(this.Bcc.y, num2, this.y_follow_speed_));
					flag = true;
				}
				this.Bcc.dy = this.Bcc.y + this.width;
				translated = this.Bcc.y - num3;
				break;
			}
			case AIM.T:
			{
				float num = cx - wh;
				float x = this.Bcc.x;
				float num2 = num;
				float num3 = x;
				if (this.x_follow_speed_ >= 1f || immediate || X.Abs(this.Bcc.y - num2) < 0.04f)
				{
					this.Bcc.x = (this.Bcc.dx = num2);
				}
				else
				{
					this.Bcc.x = (this.Bcc.dx = X.MULWALK(this.Bcc.x, num2, this.x_follow_speed_));
					flag = true;
				}
				this.Bcc.sx = this.Bcc.x + this.width;
				translated = this.Bcc.x - num3;
				break;
			}
			case AIM.R:
			{
				float num = cy - wh;
				float y2 = this.Bcc.y;
				float num2 = num;
				float num3 = y2;
				if (this.y_follow_speed_ >= 1f || immediate || X.Abs(this.Bcc.x - num2) < 0.04f)
				{
					this.Bcc.y = (this.Bcc.dy = num2);
				}
				else
				{
					this.Bcc.y = (this.Bcc.dy = X.MULWALK(this.Bcc.y, num2, this.y_follow_speed_));
					flag = true;
				}
				this.Bcc.sy = this.Bcc.y + this.width;
				translated = this.Bcc.y - num3;
				break;
			}
			case AIM.B:
			{
				float num = cx - wh;
				float x2 = this.Bcc.x;
				float num2 = num;
				float num3 = x2;
				if (this.x_follow_speed_ >= 1f || immediate || X.Abs(this.Bcc.x - num2) < 0.04f)
				{
					this.Bcc.x = (this.Bcc.sx = num2);
				}
				else
				{
					this.Bcc.x = (this.Bcc.sx = X.MULWALK(this.Bcc.x, num2, this.x_follow_speed_));
					flag = true;
				}
				this.Bcc.dx = this.Bcc.x + this.width;
				translated = this.Bcc.x - num3;
				break;
			}
			}
			return flag;
		}

		private bool fixBccFromAim1(AIM aim, ref float translated, bool immediate = false)
		{
			bool flag = false;
			if (aim == AIM.T || aim == AIM.B)
			{
				float num = this.mapcy;
				float y = this.Bcc.y;
				float num2 = num;
				float num3 = y;
				if (this.y_follow_speed_ >= 1f || immediate || X.Abs(this.Bcc.y - num2) < 0.04f)
				{
					this.Bcc.y = (this.Bcc.sy = (this.Bcc.dy = num2));
				}
				else
				{
					this.Bcc.y = (this.Bcc.sy = (this.Bcc.dy = X.MULWALK(this.Bcc.y, this.mapcy, this.y_follow_speed_)));
					flag = true;
				}
				translated = this.Bcc.y - num3;
			}
			else
			{
				float num = this.mapcx;
				float x = this.Bcc.x;
				float num2 = num;
				float num3 = x;
				if (this.x_follow_speed_ >= 1f || immediate || X.Abs(this.Bcc.x - num2) < 0.04f)
				{
					this.Bcc.x = (this.Bcc.sx = (this.Bcc.dx = num2));
				}
				else
				{
					this.Bcc.x = (this.Bcc.sx = (this.Bcc.dx = X.MULWALK(this.Bcc.x, this.mapcx, this.x_follow_speed_)));
					flag = true;
				}
				translated = this.Bcc.x - num3;
			}
			return flag;
		}

		public void positionUpdatedByChipMover()
		{
			this.need_fine_pos |= 3;
		}

		public bool run(float fcnt)
		{
			this.finePositionExecute(false);
			return true;
		}

		public void destruct()
		{
			this.deactivate();
		}

		public Map2d Mp
		{
			get
			{
				return this.Cp.Mp;
			}
		}

		public float shifted_mapcx
		{
			get
			{
				switch (this.Cp.rotation)
				{
				case 0:
					return this.Cp.mapcx + (float)X.MPF(!this.Cp.flip) * this.shift_mapx_;
				case 1:
					return this.Cp.mapcx - this.shift_mapy_;
				case 2:
					return this.Cp.mapcx - (float)X.MPF(!this.Cp.flip) * this.shift_mapx_;
				default:
					return this.Cp.mapcx + this.shift_mapy_;
				}
			}
		}

		public float shifted_mapcy
		{
			get
			{
				switch (this.Cp.rotation)
				{
				case 0:
					return this.Cp.mapcy + this.shift_mapy_;
				case 1:
					return this.Cp.mapcy + (float)X.MPF(!this.Cp.flip) * this.shift_mapx_;
				case 2:
					return this.Cp.mapcy - this.shift_mapy_;
				default:
					return this.Cp.mapcy - (float)X.MPF(!this.Cp.flip) * this.shift_mapx_;
				}
			}
		}

		public float mapcx
		{
			get
			{
				return this.shifted_mapcx + this.BCC.base_shift_x;
			}
		}

		public float mapcy
		{
			get
			{
				return this.shifted_mapcy + this.BCC.base_shift_y;
			}
		}

		public bool is_naname
		{
			get
			{
				return this.Bcc.is_naname;
			}
		}

		public float x_follow_speed
		{
			get
			{
				if (!this.is_tate)
				{
					return this.x_follow_speed_;
				}
				return this.y_follow_speed_;
			}
			set
			{
				if (this.is_tate)
				{
					this.y_follow_speed_ = value;
					return;
				}
				this.x_follow_speed_ = value;
			}
		}

		public float y_follow_speed
		{
			get
			{
				if (!this.is_tate)
				{
					return this.y_follow_speed_;
				}
				return this.x_follow_speed_;
			}
			set
			{
				if (this.is_tate)
				{
					this.x_follow_speed_ = value;
					return;
				}
				this.y_follow_speed_ = value;
			}
		}

		public float shift_mapx
		{
			get
			{
				return this.shift_mapx_;
			}
			set
			{
				this.shift_mapx_ = value;
				if (this.is_tate)
				{
					this.need_fine_pos |= 9;
					return;
				}
				this.need_fine_pos |= 18;
			}
		}

		public float shift_mapy
		{
			get
			{
				return this.shift_mapy_;
			}
			set
			{
				this.shift_mapy_ = value;
				if (this.is_tate)
				{
					this.need_fine_pos |= 18;
					return;
				}
				this.need_fine_pos |= 9;
			}
		}

		public readonly M2BlockColliderContainer.BCCLine Bcc;

		public readonly M2Chip Cp;

		public readonly bool is_tate;

		public List<IMapDamageListener> ARider;

		public bool assign_runner = true;

		public float width = 1f;

		private byte need_fine_pos = 7;

		private float x_follow_speed_ = 1f;

		private float y_follow_speed_ = 1f;

		private float shift_mapx_;

		private float shift_mapy_;
	}
}
