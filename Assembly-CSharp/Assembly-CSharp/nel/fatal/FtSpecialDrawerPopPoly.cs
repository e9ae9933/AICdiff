using System;
using UnityEngine;
using XX;

namespace nel.fatal
{
	internal class FtSpecialDrawerPopPoly : FtSpecialDrawer
	{
		public FtSpecialDrawerPopPoly(FtLayer Lay, uint col_i, uint col_t, int _stencil_ref, StringHolder CR, int cr_start_index)
			: base(Lay)
		{
			this.PP = new PopPolyDrawer(4);
			this.PP.loadFromSH(CR, cr_start_index);
			this.stencil_ref = X.Mx(-1, _stencil_ref);
			this.FillCol = C32.d2c(col_i);
			this.PP.ColLine = C32.d2c(col_t);
			this.fineMaterial(Lay.getMeshDrawer());
		}

		public override bool sinkTimeIfSame(FtSpecialDrawer SD)
		{
			if (SD is FtSpecialDrawerPopPoly)
			{
				FtSpecialDrawerPopPoly ftSpecialDrawerPopPoly = SD as FtSpecialDrawerPopPoly;
				if (this.isSame(ftSpecialDrawerPopPoly))
				{
					this.sinkTime(ftSpecialDrawerPopPoly);
					return true;
				}
			}
			return false;
		}

		public override void sinkTime(FtSpecialDrawer SD)
		{
			if (SD is FtSpecialDrawerPopPoly)
			{
				this.PP.sinkTime((SD as FtSpecialDrawerPopPoly).PP);
			}
		}

		public bool isSame(FtSpecialDrawerPopPoly _SD)
		{
			return C32.isEqual(this.FillCol, _SD.FillCol) && this.PP.isSame(_SD.PP) && this.stencil_ref == _SD.stencil_ref;
		}

		protected override void fineMaterial(MeshDrawer Md)
		{
			if (this.stencil_ref >= -1)
			{
				if (this.stencil_ref == -1 || this.stop_stencil)
				{
					Md.chooseSubMesh(0, false, false);
					Md.setMaterial(MTRX.MtrMeshNormal, false);
					return;
				}
				Md.chooseSubMesh(0, false, false);
				Md.setMaterial(MTRX.getMtr(BLEND.MASK, this.stencil_ref), false);
				Md.chooseSubMesh(1, false, false);
				Md.setMaterial(MTRX.MtrMeshNormal, false);
			}
		}

		public override void initAlphaFade(bool fadein)
		{
			if (this.stencil_ref >= 0 && this.Lay.is_effect)
			{
				this.PP.redraw_flag = true;
				this.Lay.fineMrdMaterial();
				this.stop_stencil = !fadein;
				this.fineMaterial(this.Lay.getMeshDrawer());
			}
		}

		public override void destruct(MeshDrawer Md)
		{
			Md.clear(false, true);
			Md.chooseSubMesh(0, false, false);
			base.destruct(Md);
		}

		public override bool checkRedraw(float fcnt)
		{
			return !this.Lay.alpha_is_zero && this.PP.drawRecheck(fcnt);
		}

		public override void resetTime(bool only_minimize = false)
		{
			this.PP.restartAnim(only_minimize);
		}

		public override void drawTo(MeshDrawer Md)
		{
			float tz = this.PP.getTZ();
			Md.chooseSubMesh(0, false, true);
			if (this.stencil_ref > -1 && !this.stop_stencil)
			{
				Md.Col = MTRX.ColTrnsp;
				this.PP.drawExtendAreaTo(Md, 0f, 0f);
				Md.chooseSubMesh(1, false, true);
			}
			Md.Col = this.FillCol;
			this.PP.drawTo(Md, 0f, 0f, 13f * X.ZSIN(tz, 0.5f), base.fade_alpha);
			Md.updateForMeshRenderer(false);
		}

		private PopPolyDrawer PP;

		private Color32 FillCol;

		private int stencil_ref = -2;

		private bool stop_stencil;
	}
}
