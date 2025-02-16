using System;
using UnityEngine;
using XX;

namespace nel.fatal
{
	internal class FtSpecialDrawer
	{
		public FtSpecialDrawer(FtLayer _Lay)
		{
			this.Lay = _Lay;
			MeshDrawer meshDrawer = this.Lay.getMeshDrawer();
			this.fineMaterial(meshDrawer);
		}

		protected virtual void fineMaterial(MeshDrawer Md)
		{
			Md.setMaterial(MTRX.MtrMeshNormal, false);
		}

		public virtual void destruct(MeshDrawer Md)
		{
		}

		public virtual void resetTime(bool only_minimize = false)
		{
			if (only_minimize)
			{
				this.t = X.Mn(this.t, this.maxt - 1f);
				return;
			}
			this.t = 0f;
		}

		public void finalizeAnimate(bool abort_transition)
		{
			if (this.t < this.maxt)
			{
				this.t = (abort_transition ? this.maxt : (this.maxt - 1f));
			}
		}

		public virtual bool checkRedraw(float fcnt)
		{
			if (!this.Lay.alpha_is_zero && this.t <= this.maxt + 1f)
			{
				this.t += fcnt;
				return true;
			}
			return false;
		}

		public virtual void drawTo(MeshDrawer Md)
		{
		}

		public virtual void initAlphaFade(bool fadein)
		{
		}

		public virtual void sinkTime(FtSpecialDrawer SD)
		{
			this.t = X.Mn(this.t, SD.t);
			this.maxt = SD.maxt;
		}

		public virtual bool sinkTimeIfSame(FtSpecialDrawer SD)
		{
			return false;
		}

		public float scw
		{
			get
			{
				return FatalShower.scvw;
			}
		}

		public float scsh
		{
			get
			{
				return X.NI(FatalShower.scvw * 9f / 16f, this.sch, 0.5f);
			}
		}

		public float sch
		{
			get
			{
				return FatalShower.scvh;
			}
		}

		public float scwh
		{
			get
			{
				return FatalShower.scvwh;
			}
		}

		public float schh
		{
			get
			{
				return FatalShower.scvhh;
			}
		}

		public bool enabled
		{
			get
			{
				return this.Lay.enabled;
			}
		}

		protected Color32 Col0
		{
			get
			{
				return this.Lay.Col0;
			}
			set
			{
				this.Lay.Col0 = value;
			}
		}

		public float fade_alpha
		{
			get
			{
				return this.Lay.fade_alpha;
			}
		}

		public readonly FtLayer Lay;

		protected float t;

		protected float maxt = -1f;

		private Color32 Col1;
	}
}
