using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class EnMtrManager : IRunAndDestroy
	{
		public EnMtrManager(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
			this.AEnemyDark = new List<Material>();
		}

		public void destruct()
		{
			if (this.M2D.curMap != null)
			{
				this.M2D.curMap.remRunnerObject(this);
			}
			this.runner_assigned_ = false;
			this.AEnemyDark.Clear();
			this.enemydark_t = -1;
		}

		public void assign(Material Mtr)
		{
			MTRX.setMaterialST(Mtr, "_DarkTex", MTRX.SqEfPattern.getImage(9, 0), 0f);
			if (this.AEnemyDark.Count == 0)
			{
				if (this.M2D.curMap != null && !this.runner_assigned_)
				{
					this.M2D.curMap.addRunnerObject(this);
					this.runner_assigned_ = true;
				}
				this.AEnemyDark.Add(Mtr);
				return;
			}
			if (this.AEnemyDark.IndexOf(Mtr) == -1)
			{
				this.AEnemyDark.Add(Mtr);
			}
		}

		public void remove(Material Mtr)
		{
			this.AEnemyDark.Remove(Mtr);
		}

		public bool run(float fcnt)
		{
			if (this.AEnemyDark != null)
			{
				int num = (int)this.M2D.curMap.floort >> 3;
				if (this.enemydark_t != num)
				{
					this.enemydark_t = num;
					this.fineEnemyDarkTexture();
				}
			}
			return true;
		}

		private void fineEnemyDarkTexture()
		{
			if (this.AEnemyDark == null)
			{
				return;
			}
			Map2d curMap = this.M2D.curMap;
			EnemyMeshDrawer.add_color_white_blend_level = 0.5f + X.COSI(curMap.floort, 7.4f) * 0.25f + X.COSI(curMap.floort, 11.3f) * 0.25f;
			float num = 256f * (0.5f + 0.2f * X.SINI((float)this.enemydark_t, 332f) + 0.2f * X.SINI((float)(this.enemydark_t + 190), 275f));
			float num2 = 256f * X.frac(1f + X.ANMP(this.enemydark_t, 149, 1f) + 0.2f * X.COSI((float)this.enemydark_t, 393f));
			for (int i = this.AEnemyDark.Count - 1; i >= 0; i--)
			{
				Material material = this.AEnemyDark[i];
				material.SetFloat("_DarkOffsetX", num);
				material.SetFloat("_DarkOffsetY", num2);
			}
		}

		public override string ToString()
		{
			return "EnMtrManager";
		}

		public int enemydark_t = -1;

		public readonly List<Material> AEnemyDark;

		public NelM2DBase M2D;

		private bool runner_assigned_;
	}
}
