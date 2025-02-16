using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class RainEffector : IRunAndDestroy
	{
		public RainEffector(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
			this.Mp = this.M2D.curMap;
			this.Mp.addRunnerObject(this);
			this.ABccCache = new List<M2BlockColliderContainer.BCCLine>(18);
			this.ABccUse = new List<M2BlockColliderContainer.BCCLine>(8);
			this.FD_fnDrawRainDrops = new M2DrawBinder.FnEffectBind(this.fnDrawRainDrops);
		}

		public void destruct()
		{
			if (this.Ed != null)
			{
				this.Ed.destruct();
				this.Ed = null;
			}
			this.ADrop = null;
			this.Mp.remRunnerObject(this);
		}

		public bool run(float fcnt)
		{
			if (!X.D)
			{
				return true;
			}
			if (this.need_reconsider && !this.reconsiderConfig())
			{
				return true;
			}
			while (fcnt > 0f)
			{
				float num = this.t;
				this.t -= fcnt;
				if (this.t > 0f)
				{
					break;
				}
				fcnt -= num;
				float rain_level = this.M2D.rain_level;
				this.t = X.NI(10, 1, rain_level);
				if (X.LENGTHXYN(this.M2D.Cam.x, this.M2D.Cam.y, this.pre_camx, this.pre_camy) >= 3f * this.Mp.CLEN)
				{
					this.reconsiderAreaInCamera();
				}
				if (this.lr_total == 0f)
				{
					return true;
				}
				float num2 = X.XORSP() * this.lr_total;
				int count = this.ABccUse.Count;
				int i = 0;
				while (i < count)
				{
					M2BlockColliderContainer.BCCLine bccline = this.ABccUse[i];
					num2 -= bccline.width;
					if (num2 < 0f)
					{
						if (this.ADrop == null)
						{
							this.ADrop = new List<RainEffector.RainDrop>(24);
							for (int j = 23; j >= 0; j--)
							{
								this.ADrop.Add(new RainEffector.RainDrop());
							}
							this.Ed = this.Mp.setEDC("rain", this.FD_fnDrawRainDrops, 0f);
						}
						float num3 = bccline.right + num2;
						List<RainEffector.RainDrop> adrop = this.ADrop;
						int num4 = this.raindrop_i;
						this.raindrop_i = num4 + 1;
						adrop[num4].Set(num3, bccline.is_naname ? bccline.slopeBottomY(num3, 0f, 0f, true) : bccline.y, this.Mp.floort);
						if (this.raindrop_i >= 24)
						{
							this.raindrop_i = 0;
							break;
						}
						break;
					}
					else
					{
						i++;
					}
				}
			}
			return true;
		}

		private bool reconsiderConfig()
		{
			if (this.Mp.BCC == null)
			{
				return false;
			}
			this.need_reconsider = false;
			this.pre_camx = -100f;
			this.lr_total = 0f;
			this.ABccCache.Clear();
			this.Mp.BCC.cacheNearFoot(this.ABccCache, (float)this.Mp.crop, (float)this.Mp.crop, (float)(this.Mp.clms - this.Mp.crop), (float)(this.Mp.rows - this.Mp.crop), 0f, true, 8U, true);
			return true;
		}

		private int sortBccByX(M2BlockColliderContainer.BCCLine Ca, M2BlockColliderContainer.BCCLine Cb)
		{
			float num = Ca.x - Cb.x;
			if (num < 0f)
			{
				return -1;
			}
			if (num != 0f)
			{
				return 1;
			}
			return 0;
		}

		private void reconsiderAreaInCamera()
		{
			this.lr_total = 0f;
			int count = this.ABccCache.Count;
			this.pre_camx = this.M2D.Cam.x;
			this.pre_camy = this.M2D.Cam.y;
			this.ABccUse.Clear();
			for (int i = 0; i < count; i++)
			{
				M2BlockColliderContainer.BCCLine bccline = this.ABccCache[i];
				if (this.M2D.Cam.isCoveringMp(bccline.x, bccline.y, bccline.right, bccline.bottom, 4f))
				{
					this.ABccUse.Add(bccline);
					this.lr_total += bccline.width;
				}
			}
		}

		public override string ToString()
		{
			return "RainEffector";
		}

		private bool fnDrawRainDrops(EffectItem Ef, M2DrawBinder Ed)
		{
			int num = this.raindrop_i;
			int num2 = 24;
			MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, false);
			while (--num2 >= 0)
			{
				if (num == 0)
				{
					num = 24;
				}
				RainEffector.RainDrop rainDrop = this.ADrop[--num];
				bool flag;
				Vector2 vector = Ef.EF.calcMeshXY(rainDrop.x, rainDrop.y, null, out flag);
				meshImg.base_x = vector.x;
				meshImg.base_y = vector.y;
				if (!EffectItemNel.draw_rainfoot_puddle(Ef, meshImg, meshImg.Col, 1f, this.Mp.floort - rainDrop.t0, (int)rainDrop.t0, num, false))
				{
					break;
				}
			}
			return true;
		}

		public readonly Map2d Mp;

		public readonly NelM2DBase M2D;

		public float t;

		public bool need_reconsider = true;

		private List<M2BlockColliderContainer.BCCLine> ABccCache;

		public float pre_camx;

		public float pre_camy;

		private const float cam_margin = 4f;

		private List<RainEffector.RainDrop> ADrop;

		private const int raindrop_capacity = 24;

		private int raindrop_i;

		private M2DrawBinder Ed;

		private List<M2BlockColliderContainer.BCCLine> ABccUse;

		private float lr_total;

		private M2DrawBinder.FnEffectBind FD_fnDrawRainDrops;

		private sealed class RainDrop
		{
			public RainEffector.RainDrop Set(float _x, float _y, float _t0)
			{
				this.x = _x;
				this.y = _y;
				this.t0 = _t0;
				return this;
			}

			public float x;

			public float y;

			public float t0;
		}
	}
}
