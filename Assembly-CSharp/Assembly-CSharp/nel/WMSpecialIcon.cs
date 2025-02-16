using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public struct WMSpecialIcon
	{
		public readonly string key
		{
			get
			{
				return this.SrcLP.key;
			}
		}

		public WMSpecialIcon(M2LabelPoint LP, WholeMapItem.WMItem _Wmi)
		{
			this.Wmi = _Wmi;
			this.SrcLP = LP;
			this.arrow = -1;
			this.PF = null;
			this.Adashline_pos = null;
			this.go_other_wm = null;
			if (TX.valid(LP.comment))
			{
				META meta = new META(LP.comment);
				this.arrow = meta.getDirsI("arrow", 0, false, 0, -1);
				this.go_other_wm = meta.GetSi(0, "go_other_wm");
				if (this.arrow < 0)
				{
					string[] array = meta.Get("dashline");
					if (array != null)
					{
						this.Adashline_pos = new Vector2[X.IntR((float)array.Length * 0.5f)];
						int num = 0;
						for (int i = 0; i < array.Length; i += 2)
						{
							float num2 = X.Nm(array[i], 0f, false);
							float num3 = X.Nm(array[i + 1], 0f, false);
							this.Adashline_pos[num++] = new Vector2(num2, num3);
						}
						return;
					}
					string s = meta.GetS("icon");
					this.PF = (TX.valid(s) ? MTRX.getPF(s) : null);
				}
			}
		}

		public bool valid
		{
			get
			{
				return this.Wmi != null;
			}
		}

		public bool Equals(WMSpecialIcon Ico)
		{
			return this.SrcLP == Ico.SrcLP;
		}

		public void drawTo(MeshDrawer MdFill, MeshDrawer MdLine, MeshDrawer MdIco, float center_mapx, float center_mapy, float cell_size, float alpha, float min_mapx, float min_mapy, float max_mapx, float max_mapy)
		{
			if (this.PF != null)
			{
				if (!X.isCovering(min_mapx, max_mapx, this.SrcLP.mapfocx, this.SrcLP.mapfocx, 2f) || !X.isCovering(min_mapy, max_mapy, this.SrcLP.mapfocy, this.SrcLP.mapfocy, 2f))
				{
					return;
				}
				float num = WMSpecialIcon.map2meshx(this.SrcLP.mapfocx, center_mapx, cell_size);
				float num2 = WMSpecialIcon.map2meshy(this.SrcLP.mapfocy, center_mapy, cell_size);
				MdIco.RotaPF(num, num2, 1f, 1f, 0f, this.PF, false, false, false, uint.MaxValue, false, 0);
				return;
			}
			else
			{
				if (this.arrow < 0)
				{
					if (this.Adashline_pos != null)
					{
						if (!X.isCovering(min_mapx, max_mapx, (float)this.SrcLP.mapx, (float)(this.SrcLP.mapx + this.SrcLP.mapw), 2f) || !X.isCovering(min_mapy, max_mapy, (float)this.SrcLP.mapy, (float)(this.SrcLP.mapy + this.SrcLP.maph), 2f))
						{
							return;
						}
						int num3 = this.Adashline_pos.Length;
						Vector2 vector = this.Adashline_pos[0];
						Vector2 vector2 = new Vector2(WMSpecialIcon.map2meshx((float)this.SrcLP.mapx + vector.x, center_mapx, cell_size), WMSpecialIcon.map2meshy((float)this.SrcLP.mapy + vector.y, center_mapy, cell_size));
						MdFill.Col = C32.WMulA(alpha);
						for (int i = 1; i < num3; i++)
						{
							vector = this.Adashline_pos[i];
							Vector2 vector3 = new Vector2(WMSpecialIcon.map2meshx((float)this.SrcLP.mapx + vector.x, center_mapx, cell_size), WMSpecialIcon.map2meshy((float)this.SrcLP.mapy + vector.y, center_mapy, cell_size));
							MdFill.LineDashed(vector2.x, vector2.y, vector3.x, vector3.y, 0f, X.IntC(X.LENGTHXYS(vector3.x, vector3.y, vector2.x, vector2.y) / cell_size * 5f), 2f, false, 0.5f);
							vector2 = vector3;
						}
					}
					return;
				}
				if (!X.isCovering(min_mapx, max_mapx, this.SrcLP.mapfocx, this.SrcLP.mapfocx, 2f) || !X.isCovering(min_mapy, max_mapy, this.SrcLP.mapfocy, this.SrcLP.mapfocy, 2f))
				{
					return;
				}
				float num4 = WMSpecialIcon.map2meshx(this.SrcLP.mapfocx, center_mapx, cell_size);
				float num5 = WMSpecialIcon.map2meshy(this.SrcLP.mapfocy, center_mapy, cell_size);
				float num6 = (cell_size - 2f) / 2f;
				MdLine.Arrow(num4, num5, num6, CAim.get_agR((AIM)this.arrow, 0f), 2f, false);
				return;
			}
		}

		public static float map2meshx(float mappos_x, float center_mapx, float size)
		{
			return WholeMapItem.map2meshx(mappos_x, center_mapx, size);
		}

		public static float map2meshy(float mappos_y, float center_mapy, float size)
		{
			return WholeMapItem.map2meshy(mappos_y, center_mapy, size);
		}

		public readonly PxlFrame PF;

		public readonly WholeMapItem.WMItem Wmi;

		public readonly M2LabelPoint SrcLP;

		public readonly string go_other_wm;

		public readonly int arrow;

		public readonly Vector2[] Adashline_pos;
	}
}
