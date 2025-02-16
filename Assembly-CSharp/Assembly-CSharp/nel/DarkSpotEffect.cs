using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class DarkSpotEffect
	{
		public DarkSpotEffect(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
		}

		public void endS()
		{
			if (this.ED != null)
			{
				this.ED.destruct();
			}
			for (int i = this.using_cnt - 1; i >= 0; i--)
			{
				this.AItems[i].deactivate();
			}
			this.ED = null;
			this.using_cnt = 0;
			this.need_fine_material = true;
		}

		private void Init()
		{
			if (this.AItems == null)
			{
				this.AItems = new List<DarkSpotEffect.DarkSpotEffectItem>(1);
			}
		}

		private void Stock(int cnt = 1)
		{
			this.Init();
			while (--cnt >= 0)
			{
				this.AItems.Add(new DarkSpotEffect.DarkSpotEffectItem());
			}
		}

		private DarkSpotEffect.DarkSpotEffectItem Pop()
		{
			this.Init();
			while (this.AItems.Count <= this.using_cnt)
			{
				this.Stock(X.Mx(this.using_cnt - this.AItems.Count + 1, 1));
			}
			List<DarkSpotEffect.DarkSpotEffectItem> aitems = this.AItems;
			int num = this.using_cnt;
			this.using_cnt = num + 1;
			return aitems[num];
		}

		private DarkSpotEffect.DarkSpotEffectItem Release(DarkSpotEffect.DarkSpotEffectItem Target)
		{
			if (Target == null || this.AItems == null)
			{
				return null;
			}
			return this.Release(this.AItems.IndexOf(Target));
		}

		private DarkSpotEffect.DarkSpotEffectItem Release(int i)
		{
			if (i < 0 || this.AItems == null)
			{
				return null;
			}
			if (i >= 0 && i < this.using_cnt)
			{
				DarkSpotEffect.DarkSpotEffectItem darkSpotEffectItem = this.AItems[i];
				this.AItems.RemoveAt(i);
				List<DarkSpotEffect.DarkSpotEffectItem> aitems = this.AItems;
				int num = this.using_cnt - 1;
				this.using_cnt = num;
				aitems.Insert(num, darkSpotEffectItem);
			}
			return null;
		}

		public DarkSpotEffect.DarkSpotEffectItem Set(M2Mover _BaseMv, DarkSpotEffect.SPOT spot, out bool created)
		{
			created = false;
			for (int i = this.using_cnt - 1; i >= 0; i--)
			{
				DarkSpotEffect.DarkSpotEffectItem darkSpotEffectItem = this.AItems[i];
				if (darkSpotEffectItem.isActive() && darkSpotEffectItem.BaseMv == _BaseMv && darkSpotEffectItem.spot == spot)
				{
					return darkSpotEffectItem;
				}
			}
			created = true;
			return this.Set(_BaseMv, spot);
		}

		public DarkSpotEffect.DarkSpotEffectItem Set(M2Mover _BaseMv, DarkSpotEffect.SPOT spot)
		{
			DarkSpotEffect.DarkSpotEffectItem darkSpotEffectItem = this.Pop().InitEffect(this, _BaseMv, spot);
			if (this.ED == null)
			{
				this.ED = this.M2D.curMap.setED("spot_dark", new M2DrawBinder.FnEffectBind(this.fnDraw), 0f);
			}
			return darkSpotEffectItem;
		}

		public void deactivateFor(M2Mover _BaseMv, DarkSpotEffect.SPOT spot)
		{
			for (int i = this.using_cnt - 1; i >= 0; i--)
			{
				DarkSpotEffect.DarkSpotEffectItem darkSpotEffectItem = this.AItems[i];
				if (darkSpotEffectItem.spot == spot && darkSpotEffectItem.BaseMv == _BaseMv)
				{
					darkSpotEffectItem.deactivate();
				}
			}
		}

		private Material initMaterial()
		{
			if (this.Mtr == null)
			{
				this.need_fine_material = true;
				this.Mtr = new Material(MTR.getShd("nel/MapDarkSpot"));
			}
			if (this.need_fine_material)
			{
				this.need_fine_material = false;
				this.Mtr.SetTexture("_LightTex", this.M2D.Cam.getLightTexture());
			}
			return this.Mtr;
		}

		private bool fnDraw(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.using_cnt == 0)
			{
				this.ED = null;
				return false;
			}
			MeshDrawer mesh = Ef.GetMesh("darkspot", this.initMaterial(), false);
			mesh.base_z -= 0.05f;
			M2Camera cam = this.M2D.Cam;
			mesh.base_x = cam.PosMainTransform.x;
			mesh.base_y = cam.PosMainTransform.y;
			float num = 1f;
			for (int i = this.using_cnt - 1; i >= 0; i--)
			{
				float num2;
				if (!this.AItems[i].drawTo(mesh, out num2))
				{
					this.Release(i);
				}
				else
				{
					num -= num2;
					if (num <= 0f)
					{
						break;
					}
				}
			}
			return true;
		}

		private Map2d Mp
		{
			get
			{
				return this.M2D.curMap;
			}
		}

		private List<DarkSpotEffect.DarkSpotEffectItem> AItems;

		private int using_cnt;

		public readonly NelM2DBase M2D;

		private Material Mtr;

		private M2DrawBinder ED;

		public bool need_fine_material;

		public enum SPOT
		{
			FILL,
			CIRCLE,
			TRIANGLE,
			FLUSHBANG
		}

		public sealed class DarkSpotEffectItem
		{
			public DarkSpotEffect.DarkSpotEffectItem InitEffect(DarkSpotEffect _Con, M2Mover _BaseMv, DarkSpotEffect.SPOT _spot)
			{
				this.Con = _Con;
				this.BaseCol = MTRX.ColBlack;
				this.fade_t = 22f;
				this.spot = _spot;
				this.t0 = ((this.Con.Mp.floort < 22f) ? (-this.fade_t) : this.Con.Mp.floort);
				this.is_active = true;
				this.BaseCol = MTRX.ColBlack;
				this.BaseMv = _BaseMv;
				this.add_light_color = 0f;
				this.mul_light_color = 1.5f;
				this.sub_alpha = 0.2f;
				if (this.spot == DarkSpotEffect.SPOT.FLUSHBANG)
				{
					BGM.addHalfFlag("FLASHBANG");
				}
				return this;
			}

			public Map2d Mp
			{
				get
				{
					return this.Con.Mp;
				}
			}

			public DarkSpotEffect.DarkSpotEffectItem fineTime()
			{
				this.t0 = X.Mn(this.t0, this.Con.Mp.floort - this.fade_t);
				return this;
			}

			public DarkSpotEffect.DarkSpotEffectItem resetTime()
			{
				this.t0 = this.Con.Mp.floort;
				return this;
			}

			public void deactivate()
			{
				if (this.is_active)
				{
					if (this.PeHn != null)
					{
						this.PeHn.deactivate(true);
					}
					this.is_active = false;
					this.t0 = this.Mp.floort;
					if (this.spot == DarkSpotEffect.SPOT.FLUSHBANG)
					{
						BGM.remHalfFlag("FLASHBANG");
					}
				}
			}

			public bool isActive()
			{
				return this.is_active;
			}

			public bool drawTo(MeshDrawer Md, out float drawn_alpha)
			{
				drawn_alpha = 0f;
				if (this.is_active)
				{
					this.PeHn.fine(70);
					float num = this.Con.Mp.floort - this.t0;
					if (this.spot == DarkSpotEffect.SPOT.FLUSHBANG)
					{
						if (num < 5f)
						{
							return true;
						}
						drawn_alpha = X.NIL(0.75f, 1f, num, this.fade_t);
						if (num >= 680f)
						{
							this.deactivate();
						}
					}
					else
					{
						drawn_alpha = X.ZLINE(num, this.fade_t);
					}
				}
				else
				{
					drawn_alpha = 1f - X.ZLINE(this.Con.Mp.floort - this.t0, this.fade_t);
					if (this.fade_t <= 0f)
					{
						return false;
					}
				}
				float num2 = IN.w + 1.875f;
				float num3 = IN.h + 1.875f;
				float num4 = num2 * 0.5f;
				float num5 = num3 * 0.5f;
				M2Camera cam = this.Con.M2D.Cam;
				Md.allocUv2(4, false);
				Md.allocUv3(4, false);
				switch (this.spot)
				{
				case DarkSpotEffect.SPOT.FILL:
				case DarkSpotEffect.SPOT.FLUSHBANG:
					Md.Col = Md.ColGrd.Set(this.BaseCol).mulA(drawn_alpha).C;
					Md.Rect(0f, 0f, num2, num3, true);
					break;
				case DarkSpotEffect.SPOT.CIRCLE:
				{
					Md.ColGrd.mulA(0f);
					float num6 = 2.65625f;
					float num7 = (this.BaseMv.drawx - cam.x) * this.Mp.base_scale * 0.015625f;
					float num8 = -(this.BaseMv.drawy - cam.y) * this.Mp.base_scale * 0.015625f;
					Md.InnerPoly(0f, 0f, num2, num3, num7, num8, num6, num6, num6 * 0.6f, num6 * 0.6f, 24, 0f, false, true, 0f, 1f);
					break;
				}
				case DarkSpotEffect.SPOT.TRIANGLE:
				{
					Md.Col = Md.ColGrd.Set(this.BaseCol).mulA(X.NI(this.is_active ? 0.5f : 0f, 1f, drawn_alpha)).C;
					float num9 = 1.8f * this.Mp.CLENB * 0.015625f * X.NI(0.3f, 1f, X.ZSIN(drawn_alpha));
					float num10 = 13f * this.Mp.CLENB * 0.015625f;
					float num7 = (this.BaseMv.drawx - cam.x) * this.Mp.base_scale * 0.015625f;
					float num8 = -(this.BaseMv.mbottom * this.Mp.CLEN - cam.y) * this.Mp.base_scale * 0.015625f;
					Md.TriRectBL(0, 1, 5, 4).TriRectBL(0, 4, 6, 3).TriRectBL(6, 5, 2, 3);
					if (num8 + num10 < IN.hh)
					{
						Md.Tri(5, 1, 2, false);
					}
					Md.Pos(-num4, -num5, null).Pos(-num4, num5, null).Pos(num4, num5, null)
						.Pos(num4, -num5, null);
					Md.Pos(num7 - num9, num8, null).Pos(num7, num8 + num10, null).Pos(num7 + num9, num8, null);
					break;
				}
				default:
					return false;
				}
				Md.Uv3(this.sub_alpha, this.sub_alpha, false).allocUv3(0, true);
				Md.Uv2(this.add_light_color, this.mul_light_color, false).allocUv2(0, true);
				return true;
			}

			private DarkSpotEffect Con;

			public DarkSpotEffect.SPOT spot;

			public Color32 BaseCol;

			private float t0;

			private bool is_active;

			public float fade_t;

			public M2Mover BaseMv;

			public float add_light_color;

			public float mul_light_color;

			public float sub_alpha;

			public readonly EffectHandlerPE PeHn = new EffectHandlerPE(2);
		}
	}
}
