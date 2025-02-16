using System;
using System.Collections.Generic;
using Better;
using UnityEngine;
using XX;

namespace m2d
{
	public sealed class M2BorderCldCreator : BorderDrawer<M2Pt>, BorderVectorListener.IPositionListener
	{
		public bool isExistM2Pt(int x, int y, M2Pt[,] _AAPt)
		{
			M2Pt m2Pt = _AAPt[x, y];
			if (m2Pt == null)
			{
				return false;
			}
			m2Pt.clearBccSideCache();
			if (this.lift_check && m2Pt.isLift() && M2BorderCldCreator.TargetBCC != null)
			{
				M2Pt m2Pt2 = ((y > 0) ? _AAPt[x, y - 1] : null);
				if (m2Pt2 == null || m2Pt2.canStand())
				{
					M2BorderCldCreator.OLiftBuf[Map2d.xy2b(x, y)] = new M2BorderCldCreator.LiftBuffer(m2Pt);
				}
			}
			return !this.FnCanStand(m2Pt.cfg);
		}

		public static bool FnCanStandDefault(int cfg)
		{
			return CCON.canStand(cfg) && !CCON.isBlockSlope(cfg);
		}

		public M2BorderCldCreator(Map2d _Mp, int _slope_left_x = 0, int _slope_left_y = 0, float _scale = -1f, bool _read_config_from_AAPt = false)
			: base(null, new Func<float, float, BDVector>(M2BorderPt.CreateDefaultBPT))
		{
			this.scale = _scale;
			this.FnIsExist = new Func<int, int, M2Pt[,], bool>(this.isExistM2Pt);
			this.BPtPool = new M2BorderCldCreator.BorderPtPool(32);
			if (M2BorderCldCreator.OLiftBuf == null)
			{
				M2BorderCldCreator.OLiftBuf = new BDic<uint, M2BorderCldCreator.LiftBuffer>();
			}
			this.slope_left_x = _slope_left_x;
			this.slope_top_y = _slope_left_y;
			this.ABufSlpPt = new List<M2BlockColliderContainer.M2PtPos>(16);
			this.read_config_from_AAPt = _read_config_from_AAPt;
			this.FD_fnSlopePositionGetter = new BorderDrawer<M2Pt>.FnCalcSlopePos(this.fnSlopePositionGetter);
			if (_Mp != null)
			{
				this.initS(_Mp);
			}
		}

		public M2BorderCldCreator initS(Map2d _Mp)
		{
			this.Mp = _Mp;
			this.ABufSlpPt.Clear();
			if (this.scale < 0f)
			{
				this.scale = this.Mp.base_scale;
			}
			return this;
		}

		public void createCollider(M2Pt[,] Abits, PolygonCollider2D Cld, M2BlockColliderContainer BCC, FnZoom fnCalcX = null, FnZoom fnCalcY = null)
		{
			if (this.FnCanStand == null)
			{
				this.FnCanStand = new Func<int, bool>(M2BorderCldCreator.FnCanStandDefault);
			}
			if (this.scale < 0f)
			{
				this.scale = this.Mp.base_scale;
			}
			M2BorderCldCreator.OLiftBuf.Clear();
			this.BPtPool.releaseAll(true, false);
			M2BorderCldCreator.TargetCld = Cld;
			M2BorderCldCreator.TargetBCC = BCC;
			if (BCC != null && BCC.BelongTo != null)
			{
				BCC.b_shiftx = (float)this.slope_left_x - BCC.BelongTo.x;
				BCC.b_shifty = (float)this.slope_top_y - BCC.BelongTo.y;
			}
			M2BorderCldCreator.AAPt = Abits;
			base.checkOutPath(Abits, false);
			if (fnCalcX == null)
			{
				if (this.FD_map2uxInner == null)
				{
					this.FD_map2uxInner = new FnZoom(this.map2uxInner);
				}
				fnCalcX = this.FD_map2uxInner;
			}
			if (fnCalcY == null)
			{
				if (this.FD_map2uyInner == null)
				{
					this.FD_map2uyInner = new FnZoom(this.map2uyInner);
				}
				fnCalcY = this.FD_map2uyInner;
			}
			base.makePathes(this, fnCalcX, fnCalcY, this.FD_fnSlopePositionGetter);
			M2BorderCldCreator.AAPt = null;
			M2BorderCldCreator.TargetBCC = null;
			M2BorderCldCreator.TargetCld = null;
			if (BCC != null)
			{
				BCC.finalizeBorderCreate();
				BCC.calcLift(M2BorderCldCreator.OLiftBuf, this.BPtPool);
				BCC.finalizeBorderCreateLiftAfter();
			}
			M2BorderCldCreator.OLiftBuf.Clear();
		}

		private M2Pt getConfigInner(int _x, int _y)
		{
			if (this.read_config_from_AAPt)
			{
				if (X.BTW(0f, (float)_x, (float)M2BorderCldCreator.AAPt.GetLength(0)) && X.BTW(0f, (float)_y, (float)M2BorderCldCreator.AAPt.GetLength(1)))
				{
					M2Pt m2Pt = M2BorderCldCreator.AAPt[_x, _y];
					if (m2Pt != null)
					{
						return m2Pt;
					}
				}
				return null;
			}
			return this.Mp.getPointPuts(_x + this.slope_left_x, _y + this.slope_top_y, false, false);
		}

		private List<BDVector> fnSlopePositionGetter(List<BDVector> ABuf, int bx, int by, BDCorner C, BDCorner NextC)
		{
			int aim = NextC.aim;
			int num = (int)X.LENGTHXYS(C.x, C.y, NextC.x, NextC.y);
			int num2 = CAim._XD(aim, 1);
			int num3 = -CAim._YD(aim, 1);
			bool flag = false;
			this.ABufSlpPt.Clear();
			if (this.ABufSlpPt.Capacity < num)
			{
				this.ABufSlpPt.Capacity = num;
			}
			for (int i = 0; i < num; i++)
			{
				int num4 = bx + num2 * i;
				int num5 = by + num3 * i;
				M2Pt configInner = this.getConfigInner(num4, num5);
				if (configInner != null)
				{
					this.ABufSlpPt.Add(new M2BlockColliderContainer.M2PtPos(num4 + this.slope_left_x, num5 + this.slope_top_y, configInner));
					if (CCON.isBlockSlope(configInner.cfg))
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				ABuf.Add(this.BPtPool.Pool(C.x, C.y, this.ABufSlpPt));
				ABuf.Add(this.BPtPool.Pool(NextC.x, NextC.y));
				return ABuf;
			}
			int num6 = (int)C.x;
			int num7 = (int)C.y;
			for (int j = 0; j < num; j++)
			{
				M2BlockColliderContainer.M2PtPos m2PtPos = this.ABufSlpPt[j];
				int cfg = m2PtPos.cfg;
				int num8 = num6;
				int num9 = num7;
				num6 += num2;
				num7 += num3;
				if (!CCON.isBlockSlope(cfg))
				{
					ABuf.Add(this.BPtPool.Pool((float)num8, (float)num9, m2PtPos));
					ABuf.Add(this.BPtPool.Pool((float)num6, (float)num7, m2PtPos));
				}
				else
				{
					float tiltLevel = CCON.getTiltLevel(cfg);
					float slopeLevel = CCON.getSlopeLevel(cfg, false);
					float slopeLevel2 = CCON.getSlopeLevel(cfg, true);
					if (aim == 1)
					{
						if (tiltLevel < 0f)
						{
							if (j == 0)
							{
								ABuf.Add(this.BPtPool.Pool((float)num8, (float)(num9 - 1) + slopeLevel, m2PtPos));
								if (C.aim == 0)
								{
									ABuf.Add(this.BPtPool.Pool((float)num8, (float)num7, m2PtPos));
								}
							}
							else
							{
								ABuf.Add(this.BPtPool.Pool((float)num8, (float)num9, m2PtPos));
								if (j < num - 1)
								{
									ABuf.Add(this.BPtPool.Pool((float)num6, (float)num7, m2PtPos));
								}
								else
								{
									ABuf.Add(this.BPtPool.Pool((float)num8, (float)(num9 - 1) + slopeLevel, m2PtPos));
								}
							}
						}
						else if (j == 0)
						{
							ABuf.Add(this.BPtPool.Pool((float)num8, (float)(num9 - 1) + slopeLevel, m2PtPos));
						}
						else if (j == num - 1)
						{
							ABuf.Add(this.BPtPool.Pool((float)num8, (float)num9, m2PtPos));
							ABuf.Add(this.BPtPool.Pool((float)num8, (float)(num9 - 1) + slopeLevel, m2PtPos));
						}
						else
						{
							ABuf.Add(this.BPtPool.Pool((float)num8, (float)num9, m2PtPos));
							ABuf.Add(this.BPtPool.Pool((float)num8, (float)num7, m2PtPos));
						}
					}
					else if (aim == 2)
					{
						ABuf.Add(this.BPtPool.Pool((float)num8, (float)num9 + slopeLevel, m2PtPos));
						ABuf.Add(this.BPtPool.Pool((float)num6, (float)num9 + slopeLevel2, m2PtPos));
					}
					else if (aim == 3)
					{
						if (tiltLevel < 0f)
						{
							if (j == 0)
							{
								ABuf.Add(this.BPtPool.Pool((float)num8, (float)num9, m2PtPos));
								if (C.aim == 2)
								{
									ABuf.Add(this.BPtPool.Pool((float)num8, (float)num9 + slopeLevel2, m2PtPos));
								}
							}
							else
							{
								ABuf.Add(this.BPtPool.Pool((float)num8, (float)num9, m2PtPos));
								if (j < num - 1)
								{
									ABuf.Add(this.BPtPool.Pool((float)num8, (float)num7, m2PtPos));
								}
								else
								{
									ABuf.Add(this.BPtPool.Pool((float)num8, (float)num9 + slopeLevel2, m2PtPos));
								}
							}
						}
						else if (j == 0)
						{
							ABuf.Add(this.BPtPool.Pool((float)num8, (float)num9 + slopeLevel2, m2PtPos));
						}
						else if (j == num - 1)
						{
							ABuf.Add(this.BPtPool.Pool((float)num8, (float)num9, m2PtPos));
							ABuf.Add(this.BPtPool.Pool((float)num8, (float)num9 + slopeLevel2, m2PtPos));
						}
						else
						{
							ABuf.Add(this.BPtPool.Pool((float)num8, (float)num9, m2PtPos));
							ABuf.Add(this.BPtPool.Pool((float)num8, (float)num7, m2PtPos));
						}
					}
					else if (aim == 0)
					{
						ABuf.Add(this.BPtPool.Pool((float)num8, (float)(num9 - 1) + slopeLevel2, m2PtPos));
						ABuf.Add(this.BPtPool.Pool((float)num6, (float)(num9 - 1) + slopeLevel, m2PtPos));
					}
				}
			}
			return ABuf;
		}

		private float map2uxInner(float mapx)
		{
			return (mapx - (float)M2BorderCldCreator.AAPt.GetLength(0) * 0.5f) * this.Mp.CLEN * this.scale * 0.015625f;
		}

		private float map2uyInner(float mapy)
		{
			return -(mapy - (float)M2BorderCldCreator.AAPt.GetLength(1) * 0.5f) * this.Mp.CLEN * this.scale * 0.015625f;
		}

		public void setPathCount(int i)
		{
			M2BorderCldCreator.TargetCld.pathCount = i;
			if (M2BorderCldCreator.TargetBCC != null)
			{
				M2BorderCldCreator.TargetBCC.setPathCount(i);
			}
		}

		public void SetPath(int i, List<BDVector> Apos, FnZoom fnCalcX, FnZoom fnCalcY, int len)
		{
			Vector2[] array = new Vector2[len];
			for (int j = len - 1; j >= 0; j--)
			{
				Vector2 vector = Apos[j].Convert(fnCalcX, fnCalcY);
				array[j] = vector;
			}
			if (this.collider_margin_u != 0f)
			{
				BDVector bdvector = Apos[0];
				int num = 0;
				int num2 = -1;
				for (int k = len - 1; k >= 0; k--)
				{
					BDVector bdvector2 = Apos[k];
					int num3;
					if (bdvector2.x != bdvector.x)
					{
						num3 = ((bdvector2.x < bdvector.x) ? 1 : 3);
					}
					else
					{
						num3 = ((bdvector2.y < bdvector.y) ? 2 : 0);
					}
					if (num3 == num2)
					{
						M2BorderCldCreator.Vadd(ref array[k], this.collider_margin_u * (float)CAim._XD(num3, 1), this.collider_margin_u * (float)CAim._YD(num3, 1));
					}
					else
					{
						M2BorderCldCreator.Vadd(ref array[k], ref array[num], this.collider_margin_u * (float)CAim._XD(num3, 1), this.collider_margin_u * (float)CAim._YD(num3, 1));
					}
					bdvector = bdvector2;
					num = k;
					num2 = num3;
				}
			}
			M2BorderCldCreator.TargetCld.SetPath(i, array);
			if (M2BorderCldCreator.TargetBCC != null)
			{
				M2BorderCldCreator.TargetBCC.SetPath(i, Apos, fnCalcX, fnCalcY);
			}
		}

		private static void Vadd(ref Vector2 V0, ref Vector2 V1, float dx, float dy)
		{
			V0.x += dx;
			V0.y += dy;
			V1.x += dx;
			V1.y += dy;
		}

		private static void Vadd(ref Vector2 V0, float dx, float dy)
		{
			V0.x += dx;
			V0.y += dy;
		}

		private Map2d Mp;

		private int slope_left_x;

		private int slope_top_y;

		public float scale = 1f;

		private static M2Pt[,] AAPt;

		private static BDic<uint, M2BorderCldCreator.LiftBuffer> OLiftBuf;

		public bool read_config_from_AAPt;

		public bool lift_check = true;

		public Func<int, bool> FnCanStand;

		public float collider_margin_u;

		private FnZoom FD_map2uxInner;

		private FnZoom FD_map2uyInner;

		private static PolygonCollider2D TargetCld;

		private static M2BlockColliderContainer TargetBCC;

		private List<M2BlockColliderContainer.M2PtPos> ABufSlpPt;

		private BorderDrawer<M2Pt>.FnCalcSlopePos FD_fnSlopePositionGetter;

		private M2BorderCldCreator.BorderPtPool BPtPool;

		public struct LiftBuffer
		{
			public LiftBuffer(M2Pt _Pt)
			{
				this.Pt = _Pt;
				this.AttachedBcc = null;
			}

			public M2BorderCldCreator.LiftBuffer SetChecker(M2BlockColliderContainer.BCCLine _Attached)
			{
				this.AttachedBcc = _Attached;
				return this;
			}

			public int getSlopeCfg(int x, int y)
			{
				int count = this.Pt.count;
				for (int i = 0; i < count; i++)
				{
					M2Chip c = this.Pt.GetC(i);
					int num;
					if (this.Pt.getInnerConfigXy(c, x, y, out num, true) && CCON.isLift(num))
					{
						return num;
					}
				}
				return 0;
			}

			public int getSlopeCfg(int x, int y, int pre_cfg, int pre_y, bool check_right)
			{
				int count = this.Pt.count;
				bool flag = CCON.isSlope(pre_cfg);
				for (int i = 0; i < count; i++)
				{
					M2Chip c = this.Pt.GetC(i);
					int num;
					if (this.Pt.getInnerConfigXy(c, x, y, out num, true))
					{
						if (flag)
						{
							if (CCON.isSlope(num) && (check_right ? ((float)y + CCON.getSlopeLevel(num, true) == (float)pre_y + CCON.getSlopeLevel(pre_cfg, false)) : ((float)y + CCON.getSlopeLevel(num, false) == (float)pre_y + CCON.getSlopeLevel(pre_cfg, true))))
							{
								return num;
							}
						}
						else if (num == pre_cfg)
						{
							return num;
						}
					}
				}
				return this.Pt.cfg;
			}

			public readonly M2Pt Pt;

			public M2BlockColliderContainer.BCCLine AttachedBcc;
		}

		public class BorderPtPool : ClsPool<M2BorderPt>
		{
			public BorderPtPool(int initial_stock = 2)
				: base(() => new M2BorderPt(), initial_stock)
			{
			}

			public M2BorderPt Pool(float _x, float _y, List<M2BlockColliderContainer.M2PtPos> _APt)
			{
				return base.Pool().Init(_x, _y, _APt);
			}

			public M2BorderPt Pool(float _x, float _y, M2BlockColliderContainer.M2PtPos _Pt)
			{
				return base.Pool().Init(_x, _y, _Pt);
			}

			public M2BorderPt Pool(float _x, float _y)
			{
				return base.Pool().Init(_x, _y);
			}
		}
	}
}
