using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class ShotCalcurater
	{
		public ShotCalcurater(M2Mover _Parent)
		{
			this.Parent = _Parent;
			this.AHitBufForSearch = new List<M2Ray.M2RayHittedItem>(4);
		}

		public ShotCalcurater Clear()
		{
			this.count = 0;
			this.Mn = null;
			this.divide = 3;
			return this;
		}

		public ShotCalcurater Set(MagicItem _Mg, NelPlayerCursor.M2RayChecker _Ray, M2Ray _RayDest, List<M2Ray.M2RayHittedItem> _AHitRT, int _hit_rt_max, IM2RayHitAble _Target, Vector2 _ShotStart)
		{
			if (this.count == -2)
			{
				return this;
			}
			this.Mg = _Mg;
			this.Ray = _Ray;
			this.RayDest = _RayDest;
			this.AHitRT = _AHitRT;
			this.hit_rt_max = _hit_rt_max;
			this.magic_af = -1f;
			this.ShotStart = _ShotStart;
			this.Mn = this.Mg.Mn;
			this.Target = _Target;
			this.TargetMv = _Target as M2Mover;
			return this;
		}

		private int CastRay(float agR)
		{
			if (this.magic_af < 0f)
			{
				if (this.Mg.isPreparingCircle)
				{
					this.magic_af = 0f;
					this.UxRayFirst = this.Ray.Pos;
				}
				else
				{
					this.magic_af = X.Mx(0f, this.Mg.t);
					this.UxRayFirst = (this.Mg.Ray ?? this.Ray).Pos;
				}
			}
			this.Ray.Pos = this.UxRayFirst;
			this.Ray.AngleR(agR);
			this.AHitBufForSearch.Clear();
			NelPlayerCursor.M2RayChecker.ABufChk = this.AHitBufForSearch;
			NelPlayerCursor.M2RayChecker.buf_frexible = -1;
			this.Mn.drawTo(null, this.Ray.Mp, this.Ray.getMapPos(0f), this.magic_af, agR, false, 0f, this.Ray, this.AHitBuf);
			int buf_frexible = NelPlayerCursor.M2RayChecker.buf_frexible;
			NelPlayerCursor.M2RayChecker.buf_frexible = -2;
			NelPlayerCursor.M2RayChecker.ABufChk = null;
			Vector3 vector = new Vector3(0f, 0f, -1f);
			for (int i = 0; i < this.hit_rt_max; i++)
			{
				M2Ray.M2RayHittedItem m2RayHittedItem = this.AHitRT[i];
				float num = X.LENGTHXY2(this.Ray.Pos.x, this.Ray.Pos.y, m2RayHittedItem.hit_ux, m2RayHittedItem.hit_uy);
				if (vector.z < 0f || num < vector.z)
				{
					vector.Set(m2RayHittedItem.hit_ux, m2RayHittedItem.hit_uy, num);
				}
			}
			for (int j = 0; j < buf_frexible; j++)
			{
				M2Ray.M2RayHittedItem m2RayHittedItem2 = this.AHitBufForSearch[j];
				if (vector.z < 0f || X.LENGTHXY2(this.Ray.Pos.x, this.Ray.Pos.y, m2RayHittedItem2.hit_ux, m2RayHittedItem2.hit_uy) < vector.z + this.Ray.radius * this.Ray.radius)
				{
					if (m2RayHittedItem2.Hit == this.Target)
					{
						ShotCalcurater.BreakPos.Set(m2RayHittedItem2.hit_ux, m2RayHittedItem2.hit_uy);
						return 1;
					}
					if ((m2RayHittedItem2.type & (HITTYPE.WALL | HITTYPE.REFLECTED | HITTYPE.BREAK | HITTYPE.KILLED)) != HITTYPE.NONE)
					{
						ShotCalcurater.BreakPos.Set(m2RayHittedItem2.hit_ux, m2RayHittedItem2.hit_uy);
						return 0;
					}
				}
			}
			return -1;
		}

		public bool calcProgress(Map2d Mp, float thit_mapx, float thit_mapy)
		{
			if (this.Mn == null || this.Target == null)
			{
				if (this.RayDest != null)
				{
					this.RayDest.AngleR(this.last_search_agR);
				}
				return false;
			}
			float num = thit_mapx - this.ShotStart.x;
			float num2 = thit_mapy - this.ShotStart.y;
			if (this.count == -1)
			{
				if (this.calcTargetPos.x == thit_mapx && this.calcTargetPos.y == thit_mapy && this.calcShotStart.Equals(this.ShotStart))
				{
					this.RayDest.AngleR(this.last_search_agR);
					return false;
				}
				if (this.CastRay(this.last_search_agR) == 1)
				{
					this.calcShotStart = this.ShotStart;
					this.calcTargetPos.Set(thit_mapx, thit_mapy);
					return true;
				}
				this.count = 0;
			}
			float num3 = Mp.GAR(0f, 0f, num, num2);
			if (this.CastRay(num3) == 1)
			{
				this.last_search_agR = (this.agR_target = num3);
				this.count = 0;
				this.Mn = null;
				return true;
			}
			if (this.count > 0 && X.Abs(X.angledifR(this.agR_target, num3)) > 0.3926991f)
			{
				this.count = 0;
			}
			this.agR_target = num3;
			if (this.count == 0)
			{
				int num4 = this.Mn.Count;
				float num5 = -1f;
				for (int i = 0; i < num4; i++)
				{
					MagicNotifiear.MnHit hit = this.Mn.GetHit(i);
					if (hit.hasLength())
					{
						num5 = hit.thick * 1.5f;
						break;
					}
				}
				if (num5 < 0f)
				{
					this.Mn = null;
					this.count = -2;
					return true;
				}
				this.divide = 29;
				this.hit_wall_bit = 0;
				if (num >= 0f && num2 >= 0f)
				{
					this.calcAngleMinMax(Mp, thit_mapx, thit_mapy, -1f, 1f, num5);
				}
				else if (num < 0f && num2 >= 0f)
				{
					this.calcAngleMinMax(Mp, thit_mapx, thit_mapy, -1f, -1f, num5);
				}
				else if (num >= 0f && num2 < 0f)
				{
					this.calcAngleMinMax(Mp, thit_mapx, thit_mapy, 1f, 1f, num5);
				}
				else
				{
					this.calcAngleMinMax(Mp, thit_mapx, thit_mapy, -1f, 1f, num5);
				}
				this.count++;
			}
			this.Mn.saveReachableLen(ref this.Asaved_len);
			for (int j = 0; j < 4; j++)
			{
				int num6 = (this.count - 1) / 2;
				bool flag = this.count % 2 == 1;
				float num7 = (this.last_search_agR = this.agR_target + (flag ? this.agR_min : this.agR_max) * (1f - X.ZLINE((float)num6, (float)this.divide)));
				if (this.CastRay(num7) == 1)
				{
					this.count = -1;
					float num8 = X.LENGTHXY(Mp.map2globalux(this.ShotStart.x), Mp.map2globaluy(this.ShotStart.y), ShotCalcurater.BreakPos.x, ShotCalcurater.BreakPos.y) * 64f * Mp.rCLENB;
					this.Calced.x = this.ShotStart.x + num8 * X.Cos(num7);
					this.Calced.y = this.ShotStart.y - num8 * X.Sin(num7);
					this.calcShotStart = this.ShotStart;
					this.calcTargetPos.Set(thit_mapx, thit_mapy);
					this.agR_target = num7;
					this.RayDest.AngleR(num7);
					return true;
				}
				int num9 = this.count + 1;
				this.count = num9;
				if (num9 > this.divide * 2)
				{
					this.divide += 12;
					this.agR_min *= 1.15f;
					this.agR_max *= 1.15f;
					this.count = 1;
				}
			}
			this.Mn.loadReachableLen(this.Asaved_len);
			return true;
		}

		private void calcAngleMinMax(Map2d Mp, float thit_mapx, float thit_mapy, float x_mpf, float y_mpf, float marg)
		{
			float num = 0f;
			float num2 = 0f;
			if (this.TargetMv != null)
			{
				num = this.TargetMv.sizex;
				num2 = this.TargetMv.sizey;
			}
			this.agR_min = X.angledifR(this.agR_target, Mp.GAR(this.ShotStart.x, this.ShotStart.y, thit_mapx + (num + marg) * x_mpf, thit_mapy + (num2 + marg) * y_mpf));
			this.agR_max = X.angledifR(this.agR_target, Mp.GAR(this.ShotStart.x, this.ShotStart.y, thit_mapx - (num + marg) * x_mpf, thit_mapy - (num2 + marg) * y_mpf));
		}

		public bool calc_finished
		{
			get
			{
				return this.count == -1;
			}
		}

		public bool is_error
		{
			get
			{
				return this.count == -2;
			}
		}

		public bool is_searching
		{
			get
			{
				return this.count > 0;
			}
		}

		public Vector2 CalcedAimPos
		{
			get
			{
				return this.Calced;
			}
		}

		public void drawDebug(MeshDrawer Md)
		{
			if (this.Mn != null && !(this.TargetMv == null) && !this.TargetMv.destructed)
			{
				int num = this.count;
			}
		}

		private int count;

		public readonly M2Mover Parent;

		private NelPlayerCursor.M2RayChecker Ray;

		private M2Ray RayRT;

		private MagicNotifiear Mn;

		private IM2RayHitAble Target;

		private M2Mover TargetMv;

		public RaycastHit2D[] AHitBuf;

		public float[] Asaved_len;

		private const int INJECT_SEARCH_BEHIND_COUNT = 16;

		private Vector2 Calced;

		private List<M2Ray.M2RayHittedItem> AHitBufForSearch;

		private Vector2 ShotStart;

		private Vector2 UxRayFirst;

		private MagicItem Mg;

		private static Vector2 BreakPos;

		private Vector2 calcShotStart;

		private Vector2 calcTargetPos;

		private float agR_target;

		private float agR_min;

		private float agR_max;

		private float magic_af;

		private int divide;

		public float last_search_agR;

		private M2Ray RayDest;

		private List<M2Ray.M2RayHittedItem> AHitRT;

		private int hit_rt_max;

		private int hit_wall_bit;
	}
}
