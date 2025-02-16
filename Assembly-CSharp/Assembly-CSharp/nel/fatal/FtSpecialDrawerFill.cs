using System;
using UnityEngine;
using XX;

namespace nel.fatal
{
	internal class FtSpecialDrawerFill : FtSpecialDrawer
	{
		public FtSpecialDrawerFill(FtLayer Lay, uint cols, uint cole, float _maxt, float _saf)
			: base(Lay)
		{
			base.Col0 = C32.d2c(cols);
			this.Col1 = C32.d2c(cole);
			this.t = -_saf;
			this.maxt = _maxt;
			Lay.is_effect = true;
		}

		public override bool sinkTimeIfSame(FtSpecialDrawer SD)
		{
			if (SD is FtSpecialDrawerFill && (SD as FtSpecialDrawerFill).isSame(C32.c2d(base.Col0), C32.c2d(this.Col1), this.maxt))
			{
				this.sinkTime(SD);
				return true;
			}
			return false;
		}

		public bool isSame(uint cols, uint cole, float _maxt)
		{
			return _maxt == this.maxt && cols == C32.c2d(base.Col0) && cole == C32.c2d(this.Col1);
		}

		internal FtSpecialDrawerFill initDrawFunc(PIC ptype)
		{
			switch (ptype)
			{
			case PIC.GRD_FI:
				this.FnDraw = delegate(MeshDrawer Md, float tz)
				{
					if (tz > 0f)
					{
						Md.Col = Md.ColGrd.Set(base.Col0).mulA(tz * base.fade_alpha).C;
						Md.ColGrd.Set(this.Col1).mulA(tz * base.fade_alpha);
						Md.RectGradation(0f, 0f, base.scw + 10f, base.sch + 10f, GRD.BOTTOM2TOP, false);
					}
				};
				return this;
			case PIC.GRD_FO:
				this.FnDraw = delegate(MeshDrawer Md, float tz)
				{
					Md.Col = Md.ColGrd.Set(base.Col0).mulA((1f - tz) * ((this.t >= 0f) ? 1f : X.ZPOW(this.maxt + this.t, this.maxt * 0.35f)) * base.fade_alpha).C;
					Md.ColGrd.Set(this.Col1).mulA((1f - tz) * base.fade_alpha);
					Md.RectGradation(0f, 0f, base.scw + 10f, base.sch + 10f, GRD.BOTTOM2TOP, false);
				};
				return this;
			case PIC.RADIATION:
				this.FnDraw = delegate(MeshDrawer Md, float tz)
				{
					Md.Col = Md.ColGrd.Set(base.Col0).blend(this.Col1, tz).mulA(base.fade_alpha)
						.C;
					this.Lay.FtCon.DrRad.drawTo(Md, 0f, 0f, base.scw, base.sch, 2f, (float)IN.totalframe, false, 1f);
				};
				return this;
			}
			this.FnDraw = delegate(MeshDrawer Md, float tz)
			{
				Md.Col = Md.ColGrd.Set(base.Col0).blend(this.Col1, tz).mulA(base.fade_alpha)
					.C;
				Md.Rect(0f, 0f, base.scw + 40f, base.sch + 40f, false);
			};
			return this;
		}

		public override void drawTo(MeshDrawer Md)
		{
			float num = ((this.t >= this.maxt) ? 1f : X.ZLINE(this.t, this.maxt));
			this.FnDraw(Md, num);
			Md.updateForMeshRenderer(false);
		}

		private Action<MeshDrawer, float> FnDraw;

		protected Color32 Col1;
	}
}
