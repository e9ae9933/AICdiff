using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2CImgDrawerWhole : M2CImgDrawer
	{
		public M2CImgDrawerWhole(MeshDrawer Md, int _lay, M2Puts _Cp, META Meta)
			: base(Md, _lay, _Cp, true)
		{
			this.use_color = true;
		}

		public override int redraw(float fcnt)
		{
			return this.resetColor(-1, false);
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			bool flag = base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			this.resetColor(-1, true);
			return flag;
		}

		private int resetColor(int d = -1, bool force = false)
		{
			bool flag = ((d == -1) ? this.isLayFocus() : (d == 1));
			if (this.lay_focus != flag || force)
			{
				this.lay_focus = flag;
				int num = this.index_last - this.index_first;
				for (int i = 0; i < num; i++)
				{
					this.ACol[i] = (this.lay_focus ? MTRX.ColWhite : M2CImgDrawerWhole.ColDark);
				}
				this.need_reposit_flag = true;
				return base.redraw(0f);
			}
			return 0;
		}

		public bool isLayFocus()
		{
			if (M2CImgDrawerWhole.fine_flag)
			{
				M2CImgDrawerWhole.fine_flag = false;
				M2CImgDrawerWhole.ColDark = C32.d2c(4286743190U);
				M2CImgDrawerWhole.LayerCurrent = null;
				if (M2CImgDrawerWhole.LayerCurrent == null)
				{
					Map2d curMap = base.M2D.curMap;
					M2CImgDrawerWhole.LayerCurrent = ((curMap != null) ? base.Mp.getLayer(curMap.key) : null);
				}
			}
			return this.Cp.Lay == M2CImgDrawerWhole.LayerCurrent;
		}

		private static M2MapLayer LayerCurrent = null;

		public static bool fine_flag = true;

		public bool lay_focus = true;

		public static Color32 ColDark;
	}
}
