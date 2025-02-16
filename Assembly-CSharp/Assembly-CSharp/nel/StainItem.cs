using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public class StainItem : DRect, IRunAndDestroy, IBCCFootListener
	{
		public StainItem()
			: base("stain")
		{
			this.ABcc = new M2BlockColliderContainer.BCCLine[4];
			this.ABccInfo = new M2BlockColliderContainer.BCCInfo[4];
			this.Afoot_len = new float[4];
			this.AFooted = new List<StainItem.FootT>(4);
		}

		public StainManager Con
		{
			get
			{
				return (M2DBase.Instance as NelM2DBase).STAIN;
			}
		}

		public StainItem Set(Map2d Mp, float mapx, float mapy, StainItem.TYPE _type, uint _footable_bits, float _maxt, M2BlockColliderContainer.BCCLine Bcc = null)
		{
			this.footable_bits = _footable_bits;
			this.type = _type;
			this.AFooted.Clear();
			this.FnRun = null;
			this.t = 0f;
			this.maxt = _maxt;
			X.ALLN<M2BlockColliderContainer.BCCLine>(this.ABcc);
			X.ALLDEF<M2BlockColliderContainer.BCCInfo>(this.ABccInfo);
			base.Set(0f, 0f, 0f, 0f);
			int num = 0;
			if (Bcc == null || Bcc.is_map_bcc)
			{
				mapx = (float)((int)mapx) + 0.5f;
				mapy = (float)((int)(mapy + 0.1f)) + 0.5f;
			}
			else
			{
				AIM aim = Bcc.aim;
				if (aim == AIM.L || aim == AIM.R)
				{
					mapx = Bcc.x + (float)CAim._XD(Bcc.aim, 1) * 0.5f;
				}
				else
				{
					mapy = Bcc.slopeBottomY(mapx, Bcc.BCC.base_shift_x, Bcc.BCC.base_shift_y, true) - 0.5f * (float)CAim._YD(Bcc.aim, 1);
				}
			}
			this.centerx = mapx;
			this.centery = mapy;
			if (Bcc != null)
			{
				int foot_aim = (int)Bcc.foot_aim;
				if (((1U << foot_aim) & this.footable_bits) == 0U)
				{
					return null;
				}
				if (this.InitBcc(Bcc, mapx, mapy, foot_aim))
				{
					num++;
				}
			}
			else
			{
				int num2 = (int)mapx;
				int num3 = (int)mapy;
				M2Pt pointPuts = Mp.getPointPuts(num2, num3, true, false);
				if (pointPuts == null)
				{
					return null;
				}
				for (int i = 0; i < 4; i++)
				{
					if ((this.footable_bits & (1U << i)) != 0U)
					{
						float num4 = (float)num2;
						float num5 = (float)num3;
						float num6 = (float)(num2 + 1);
						float num7 = (float)(num3 + 1);
						if (i == 3)
						{
							num5 = (float)num3 - 0.05f;
							num7 = (float)num3 + 0.05f;
							if (Mp.BCC.isFallable((float)num2 + 0.5f, (float)num3, 0.2f, 0.11f, out Bcc, true, true, -1f, null) < 0f)
							{
								Bcc = null;
							}
						}
						else
						{
							Bcc = pointPuts.getSideBcc(Mp, num2, num3, (AIM)i);
							if (i == 1)
							{
								num5 = (float)(num3 + 1) - 0.05f;
								num7 = (float)(num3 + 1) + 0.05f;
							}
							else if (i == 0)
							{
								num4 = (float)(num2 + 1) - 0.05f;
								num6 = (float)(num2 + 1) + 0.05f;
							}
							else
							{
								num4 = (float)num2 - 0.05f;
								num6 = (float)num2 + 0.05f;
							}
						}
						if (Bcc != null && Bcc.isCoveringXyS(num4, num5, num6, num7, 0f) && this.InitBcc(Bcc, mapx, mapy, i))
						{
							num++;
						}
					}
				}
			}
			if (num <= 0)
			{
				return null;
			}
			return this;
		}

		public bool InitBcc(M2BlockColliderContainer.BCCLine Bcc, float mapx, float mapy, int footaim)
		{
			CAim._XD(footaim, 1);
			float num = (float)CAim._YD(footaim, 1);
			if (Bcc.is_map_bcc)
			{
				float num2 = (float)((int)mapx);
				int num3 = (int)mapy;
				mapx = num2 + 0.5f;
				mapy = (float)num3 + 0.5f;
			}
			if (Bcc.AFootLsnRegistered != null)
			{
				for (int i = Bcc.AFootLsnRegistered.Count - 1; i >= 0; i--)
				{
					StainItem stainItem = Bcc.AFootLsnRegistered[i] as StainItem;
					if (stainItem != null && (stainItem.footable_bits & (1U << footaim)) != 0U && stainItem.getMapBounds(null).isin(mapx, mapy, Bcc.is_map_bcc ? (-0.05f) : (-0.2f)))
					{
						if (stainItem.type == this.type)
						{
							stainItem.t = 0f;
							stainItem.maxt = X.Mx(stainItem.maxt, this.maxt);
						}
						else if (StainManager.isConflictStainType(stainItem.type, this.type))
						{
							stainItem.maxt = 1f;
							goto IL_00FF;
						}
						return false;
					}
					IL_00FF:;
				}
			}
			float num4 = mapx;
			float num5 = mapy;
			float num6 = 0.35f;
			float num7 = num6;
			this.Afoot_len[footaim] = 0.5f;
			if (num != 0f)
			{
				num6 = 0.5f;
				if (Bcc.is_naname)
				{
					float num8;
					float num9;
					Bcc.BCC.getBaseShift(out num8, out num9);
					float num10 = Bcc.slopeBottomY((float)((int)mapx), num8, num9, true);
					float num11 = Bcc.slopeBottomY((float)((int)mapx + 1), num8, num9, true);
					num7 += (num11 - num10) * 0.5f;
					num5 = (num11 + num10) * 0.5f;
					this.Afoot_len[footaim] = (num5 - this.centery) * (float)CAim._YD(footaim, 1);
				}
				else
				{
					num7 = 0.51f;
				}
			}
			else
			{
				num7 = 0.51f;
			}
			if (!Bcc.isCoveringXyS(num4 - num6, num5 - num7, num4 + num6, num5 + num7, 0f))
			{
				return false;
			}
			base.Expand(num4 - num6, num5 - num7, num6 * 2f, num7 * 2f, false);
			this.ABcc[footaim] = Bcc;
			this.ABccInfo[footaim] = new M2BlockColliderContainer.BCCInfo(Bcc);
			Bcc.addBCCFootListener(this, false);
			return true;
		}

		public bool run(float fcnt)
		{
			if (this.t >= this.maxt)
			{
				this.maxt = 0f;
				return false;
			}
			int i = this.AFooted.Count - 1;
			while (i >= 0)
			{
				StainItem.FootT footT = this.AFooted[i];
				if (footT.t > 0f)
				{
					goto IL_0070;
				}
				footT.t = 6f;
				if (this.FtRunPre(ref footT, footT.FootD, true) != 0)
				{
					goto IL_0070;
				}
				this.AFooted.RemoveAt(i);
				IL_00A4:
				i--;
				continue;
				IL_0070:
				footT.t = X.Mx(0f, footT.t - fcnt);
				if (i < this.AFooted.Count)
				{
					this.AFooted[i] = footT;
					goto IL_00A4;
				}
				goto IL_00A4;
			}
			this.t += fcnt;
			return true;
		}

		public void destruct()
		{
			for (int i = this.AFooted.Count - 1; i >= 0; i--)
			{
				this.AFooted[i].FootD.remFootListener(this);
			}
			for (int j = 0; j < 4; j++)
			{
				M2BlockColliderContainer.BCCLine bccline = this.ABcc[j];
				this.ABcc[j] = null;
				this.ABccInfo[j] = default(M2BlockColliderContainer.BCCInfo);
				if (bccline != null)
				{
					bccline.remBCCFootListener(this, false);
				}
			}
			this.AFooted.Clear();
		}

		public M2BlockColliderContainer.BCCLine GetBccFor(IMapDamageListener Fd)
		{
			M2BlockColliderContainer.BCCLine footBCC = Fd.get_FootBCC();
			if (footBCC != null && this.ABcc[(int)footBCC.foot_aim] == footBCC)
			{
				return footBCC;
			}
			return null;
		}

		public M2BlockColliderContainer.BCCLine GetBcc(int dir)
		{
			return this.ABcc[dir];
		}

		public M2BlockColliderContainer.BCCInfo GetBccInfo(int dir)
		{
			if (this.ABcc[dir] == null)
			{
				return default(M2BlockColliderContainer.BCCInfo);
			}
			return this.ABccInfo[dir];
		}

		public void SetBcc(int dir, M2BlockColliderContainer.BCCLine _Bcc, bool removing_listener = false)
		{
			if (removing_listener && this.ABcc[dir] != null)
			{
				this.ABcc[dir].remBCCFootListener(this, false);
			}
			this.ABcc[dir] = _Bcc;
			this.ABccInfo[dir] = new M2BlockColliderContainer.BCCInfo(_Bcc);
			_Bcc.addBCCFootListener(this, false);
		}

		public uint getFootableAimBits()
		{
			return this.footable_bits;
		}

		public DRect getMapBounds(DRect BufRc)
		{
			return this;
		}

		public bool footedInit(M2BlockColliderContainer.BCCLine Bcc, IMapDamageListener Fd)
		{
			if (StainItem.FootT.IndexOf(this.AFooted, Fd) == -1)
			{
				StainItem.FootT footT = new StainItem.FootT(Fd, 0f);
				if (this.FtRunPre(ref footT, Fd, true) < 1)
				{
					return false;
				}
				this.AFooted.Add(footT);
			}
			return true;
		}

		public bool footedQuit(IMapDamageListener Fd, bool from_jump_init = false)
		{
			int num = StainItem.FootT.IndexOf(this.AFooted, Fd);
			if (num >= 0)
			{
				this.AFooted.RemoveAt(num);
			}
			return true;
		}

		private int FtRunPre(ref StainItem.FootT _Ft, IMapDamageListener Fm, bool execute_run = true)
		{
			DRect mapBounds = Fm.getMapBounds(M2BlockColliderContainer.BufRc);
			if (mapBounds == null)
			{
				return 0;
			}
			if (!mapBounds.isCovering(this, 0f))
			{
				if (!mapBounds.isCovering(this, 1f))
				{
					return 0;
				}
				return 1;
			}
			else
			{
				M2BlockColliderContainer.BCCLine bccFor = this.GetBccFor(Fm);
				if (bccFor == null)
				{
					return 0;
				}
				if (!execute_run || this.FnRun == null)
				{
					return 2;
				}
				if (!this.FnRun(this, bccFor, ref _Ft, 0f, true, Fm))
				{
					return 0;
				}
				return 2;
			}
		}

		public void rewriteFootType(M2BlockColliderContainer.BCCLine Bcc, IMapDamageListener Fm, ref string s)
		{
			string text = StainManager.stain_foottype_overwrite(this.type);
			if (text == null)
			{
				return;
			}
			for (int i = this.AFooted.Count - 1; i >= 0; i--)
			{
				StainItem.FootT footT = this.AFooted[i];
				if (footT.FootD == Fm && this.FtRunPre(ref footT, Fm, false) == 2)
				{
					s = text;
				}
			}
		}

		public StainItem.TYPE type;

		public float centerx;

		public float centery;

		public uint footable_bits;

		private const float margin = 0.35f;

		public float t;

		public float maxt;

		public uint id;

		private List<StainItem.FootT> AFooted;

		private M2BlockColliderContainer.BCCLine[] ABcc;

		private M2BlockColliderContainer.BCCInfo[] ABccInfo;

		public float[] Afoot_len;

		public StainItem.FnStainRun FnRun;

		private const float T_RECHECK = 6f;

		public delegate bool FnStainRun(StainItem Stn, M2BlockColliderContainer.BCCLine Bcc, ref StainItem.FootT Ft, float fcnt, bool firstrun, IMapDamageListener Fm);

		public enum TYPE : byte
		{
			FIRE,
			ICE,
			WEB,
			_MAX
		}

		public struct FootT
		{
			public FootT(IMapDamageListener Ft, float _t)
			{
				this.FootD = Ft;
				this.t = _t;
			}

			public void recheckImmediate()
			{
				this.t = X.Mn(1f, this.t);
			}

			public static int IndexOf(List<StainItem.FootT> A, IMapDamageListener Ft)
			{
				for (int i = A.Count - 1; i >= 0; i--)
				{
					if (A[i].FootD == Ft)
					{
						return i;
					}
				}
				return -1;
			}

			public IMapDamageListener FootD;

			public float t;
		}
	}
}
