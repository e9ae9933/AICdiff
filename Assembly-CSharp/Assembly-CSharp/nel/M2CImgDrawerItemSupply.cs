using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerItemSupply : M2CImgDrawer, IActivatable
	{
		public M2CImgDrawerItemSupply(MeshDrawer Md, int _lay, M2Puts _Cp)
			: base(Md, _lay, _Cp, true)
		{
			this.Cp.arrangeable = true;
			this.use_color = true;
			this.dcnt = 100f;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			M2CImgDrawer anyDrawer = this.Cp.getAnyDrawer(8U);
			if (anyDrawer != null)
			{
				Color32[] colorArray = anyDrawer.get_Md().getColorArray();
				int startVerIndex = anyDrawer.getStartVerIndex();
				int endVerIndex = anyDrawer.getEndVerIndex();
				for (int i = startVerIndex; i < endVerIndex; i++)
				{
					colorArray[i] = MTRX.ColTrnsp;
				}
			}
		}

		public override int redraw(float fcnt)
		{
			if (this.Cp.active_removed || this.LpCon == null)
			{
				return base.redraw(fcnt);
			}
			M2CImgDrawer anyDrawer = this.Cp.getAnyDrawer(8U);
			if (anyDrawer == null)
			{
				return base.redraw(fcnt);
			}
			Color32 color = MTRX.ColTrnsp;
			if (this.bright_t > 0f)
			{
				this.bright_t -= fcnt;
				color = C32.MulA(uint.MaxValue, X.ZSIN(this.bright_t, 30f));
			}
			else if (!this.LpCon.already_collected)
			{
				this.dcnt += fcnt;
				if (this.dcnt < 4f)
				{
					return base.redraw(fcnt);
				}
				if (!base.Mp.isinCamera((float)this.Cp.mapx, (float)this.Cp.mapy, (float)this.Cp.rwidth * base.Mp.rCLEN, (float)this.Cp.rheight * base.Mp.rCLEN, 20f))
				{
					this.dcnt = 4f;
					return base.redraw(fcnt);
				}
				this.dcnt = 0f;
				color = C32.MulA(uint.MaxValue, 0.2f + 0.2f * X.COSI(base.Mp.floort, 90f));
			}
			Color32[] colorArray = anyDrawer.get_Md().getColorArray();
			int startVerIndex = anyDrawer.getStartVerIndex();
			int endVerIndex = anyDrawer.getEndVerIndex();
			for (int i = startVerIndex; i < endVerIndex; i++)
			{
				colorArray[i] = color;
			}
			return base.redraw(fcnt) | base.Mp.getLayer2UpdateFlag(anyDrawer.get_Md());
		}

		public void activate()
		{
			this.dcnt = 100f;
			this.bright_t = 30f;
		}

		public void deactivate()
		{
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		public M2LpItemSupplier LpCon;

		public float bright_t;

		public float dcnt;
	}
}
