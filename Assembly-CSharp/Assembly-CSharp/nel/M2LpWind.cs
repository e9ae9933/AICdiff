using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2LpWind : NelLp, ILinerReceiver
	{
		public M2LpWind(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public override void initActionPre()
		{
			base.initActionPre();
			this.Meta = new META(this.comment);
			this.Mp.M2D.loadMaterialSnd("BGM_wind");
		}

		public override void initAction(bool normal_map)
		{
			if (this.Meta == null)
			{
				this.initActionPre();
			}
			this.pre_on = this.Meta.GetB("pre_on", true);
			float num = this.Meta.GetNm("angle", 0f, 0);
			float nm = this.Meta.GetNm("velocity", 0.3f, 0);
			float nm2 = this.Meta.GetNm("near_multiple", 1f, 0);
			bool b = this.Meta.GetB("check_wall", false);
			this.liner_receivable = this.Meta.GetB("liner_receivable", true);
			float num2 = this.height / this.width;
			float num3 = (float)this.mapw * 0.5f;
			float num4 = (float)this.maph * 0.5f;
			while (num > 180f)
			{
				num -= 360f;
			}
			while (num <= -180f)
			{
				num += 360f;
			}
			float num5;
			float num6;
			float num7;
			float num8;
			float num9;
			Vector2 vector;
			if (num == 90f || num == -90f)
			{
				num5 = ((num == 90f) ? 1.5707964f : (-1.5707964f));
				num6 = 0f;
				num7 = (float)X.MPF(num > 0f);
				num8 = num3;
				num9 = num4 * 2f;
				vector = new Vector2(0f, -num4 * num7);
			}
			else
			{
				num5 = num / 180f * 3.1415927f;
				num6 = X.Cos(num5);
				num7 = X.Sin(num5);
				float num10 = X.Abs(num7 / num6);
				if (num10 < num2)
				{
					vector = new Vector2(num3, num3 * num10);
					num8 = (num4 + vector.y) * num6;
					num9 = num3 / num6 * 2f;
				}
				else
				{
					vector = new Vector2(num4 / num10, num4);
					num8 = (num3 + vector.x) * num7;
					num9 = num4 / num7 * 2f;
				}
				vector.x *= (float)X.MPF(X.BTW(-90f, num, 90f));
				vector.y *= (float)X.MPF(num < 0f);
			}
			Vector2 vector2 = -vector;
			this.Wind = base.nM2D.WIND.Add(vector2.x + base.mapcx + num6 * 0.1f, vector2.y + base.mapcy - num7 * 0.1f, X.Abs(num8) - 0.125f, num5, num9, nm, -2f, nm2).Wind;
			this.Wind.check_wall = b;
			this.fineActivation();
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			if (this.Wind != null)
			{
				this.Wind.destruct();
				this.Wind = null;
			}
			if (this.Snd != null)
			{
				this.Snd.destruct();
				this.Snd = null;
			}
		}

		private void fineActivation()
		{
			if (this.Wind != null)
			{
				this.Wind.enabled = this.pre_on != this.liner_enabled;
				if (this.Wind.enabled)
				{
					if (this.Snd != null)
					{
						this.Snd.destruct();
						return;
					}
				}
				else if (this.Snd == null)
				{
					this.Snd = this.Mp.M2D.Snd.Environment.AddLoop("wind", base.unique_key, base.mapcx, base.mapcy, 9f, 9f, (float)(this.mapw + 4), (float)(this.maph + 4), null);
				}
			}
		}

		public void activateLiner(bool immediate)
		{
			if (!this.liner_enabled && this.liner_receivable)
			{
				this.liner_enabled = true;
				this.fineActivation();
			}
		}

		public void deactivateLiner(bool immediate)
		{
			if (this.liner_enabled && this.liner_receivable)
			{
				this.liner_enabled = false;
				this.fineActivation();
			}
		}

		public bool initEffect(bool activating, ref DRect RcEffect)
		{
			if (!this.liner_receivable)
			{
				return false;
			}
			if (RcEffect == null)
			{
				RcEffect = new DRect(base.unique_key);
			}
			RcEffect.Set((float)this.mapx, (float)this.mapy, (float)this.mapw, (float)this.maph);
			return false;
		}

		public override void activate()
		{
			this.activateLiner(false);
		}

		public override void deactivate()
		{
			this.deactivateLiner(false);
		}

		private META Meta;

		private bool pre_on;

		private bool liner_enabled;

		private WindItem Wind;

		private bool liner_receivable = true;

		private M2SndLoopItem Snd;
	}
}
