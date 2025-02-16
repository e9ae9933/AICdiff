using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace nel.fatal
{
	internal class FtSpecialDrawerBiteTransition : FtSpecialDrawer
	{
		public FtSpecialDrawerBiteTransition(FtLayer Lay, FtLayer _Target, StringHolder CR, int maxt_index)
			: base(Lay)
		{
			this.Target = _Target;
			this.maxt = X.Nm(CR.getIndex(maxt_index), 0f, false);
			this.ABiteSwap = new List<Vector2>(CR.clength - maxt_index + 1);
			this.ABiteSwap.Add(new Vector2(this.Target.base_x, this.Target.base_y));
			for (int i = maxt_index + 1; i < CR.clength; i += 2)
			{
				float num = CR.Nm(i, 0f);
				this.ABiteSwap.Add(new Vector2(num, CR.Nm(i + 1, num)));
			}
		}

		protected override void fineMaterial(MeshDrawer Md)
		{
			Md.setMaterial(FatalShower.MtrBiteTransS, false);
		}

		public override void destruct(MeshDrawer Md)
		{
			Md.setMaterial(MTRX.MtrMeshNormal, false);
			base.destruct(Md);
		}

		public void abortAnim()
		{
			this.aborted = true;
		}

		public override void initAlphaFade(bool fadein)
		{
			this.aborted = !fadein;
			if (!fadein)
			{
				this.pos_update_flag = true;
			}
		}

		public override bool checkRedraw(float fcnt)
		{
			if (this.maxt <= 0f || !base.enabled)
			{
				return false;
			}
			this.t += fcnt;
			float num = X.ZLINE(this.t % this.maxt, this.maxt);
			if ((num < 0.23f && this.t >= this.maxt) || num > 0.85f)
			{
				return true;
			}
			int num2 = (int)this.t / (int)this.maxt;
			if (this.aborted || this.Lay.alpha_is_zero)
			{
				this.t = X.Mn(((float)num2 + 0.23f) * this.maxt + 1f, this.t);
			}
			return this.pre_tindex != num2 * 3 + 1;
		}

		public override void drawTo(MeshDrawer Md)
		{
			float num = X.ZLINE(this.t % this.maxt, this.maxt);
			int num2 = (int)this.t / (int)this.maxt;
			int num3 = num2 % this.ABiteSwap.Count;
			float num4 = -1000f;
			bool flag = false;
			Vector2 vector = this.ABiteSwap[num3];
			if (num < 0.23f && this.t >= this.maxt)
			{
				if (this.pre_tindex != num2 * 3)
				{
					if (this.aborted && this.pre_tindex < num2 * 3 - 1)
					{
						return;
					}
					this.pre_tindex = num2 * 3;
					this.pos_update_flag = true;
				}
				float fade_alpha = this.Lay.fade_alpha;
				if (this.pre_drawn_alpha != fade_alpha)
				{
					this.pre_drawn_alpha = fade_alpha;
					flag = true;
					num4 = this.pre_agR;
				}
				FatalShower.drawBiteTransition(Md, this.pre_agR, 0.5f + 0.5f * X.ZSIN2(num, 0.23f), fade_alpha);
			}
			else if (num >= 0.85f)
			{
				if (this.pre_tindex != num2 * 3 + 2)
				{
					if (this.aborted)
					{
						return;
					}
					this.pre_tindex = num2 * 3 + 2;
					int num5 = (num2 + 1) % this.ABiteSwap.Count;
					Vector2 vector2 = this.ABiteSwap[num5];
					num4 = (this.pre_agR = X.GAR(vector.x, vector.y, vector2.x, vector2.y));
					flag = (this.pos_update_flag = true);
				}
				float fade_alpha2 = this.Lay.fade_alpha;
				if (this.pre_drawn_alpha != fade_alpha2)
				{
					flag = true;
					this.pre_drawn_alpha = fade_alpha2;
					num4 = this.pre_agR;
				}
				FatalShower.drawBiteTransition(Md, num4, X.ZPOW(num - 0.85f, 0.23999996f) * 0.5f, this.Lay.fade_alpha);
			}
			else if (this.pre_tindex != num2 * 3 + 1)
			{
				this.pre_tindex = num2 * 3 + 1;
				Md.clear(false, false);
				flag = (this.pos_update_flag = true);
				this.pre_drawn_alpha = 0f;
			}
			if (flag)
			{
				Md.updateForMeshRenderer(false);
			}
			if (this.pos_update_flag && !this.aborted && this.Lay.fade_alpha >= 1f)
			{
				this.Target.base_x = vector.x;
				this.Target.base_y = vector.y;
				this.Target.Reposit(true);
				this.pos_update_flag = false;
			}
		}

		public bool aborted;

		private FtLayer Target;

		private int pre_tindex;

		private float pre_agR;

		private float pre_drawn_alpha;

		private const float fade_lvl = 0.23f;

		private const float fade_after_lvl = 0.85f;

		private bool pos_update_flag;

		private List<Vector2> ABiteSwap;
	}
}
