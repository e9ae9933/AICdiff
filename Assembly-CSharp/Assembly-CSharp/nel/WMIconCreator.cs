using System;
using m2d;
using XX;

namespace nel
{
	public sealed class WMIconCreator
	{
		public WMIconCreator(M2LabelPoint _Lp, WMIcon.TYPE type, string sf_key = null)
			: this(_Lp.Mp, (int)_Lp.mapfocx, (int)_Lp.mapfocy, type, sf_key, null)
		{
		}

		public WMIconCreator(M2Chip _Cp, WMIcon.TYPE type, string sf_key = null, string sppos_key = null)
			: this(_Cp.Mp, (int)_Cp.mapcx, (int)_Cp.mapcy, type, sf_key, sppos_key)
		{
		}

		public WMIconCreator(Map2d _Mp, int x, int y, WMIcon.TYPE type, string sf_key = null, string sppos_key = null)
		{
			this.Mp = _Mp;
			this.Wm = (this.Mp.M2D as NelM2DBase).WM.GetWholeFor(this.Mp, false);
			if (this.Wm == null)
			{
				return;
			}
			WholeMapItem.WMItem wmi = this.Wm.GetWmi(this.Mp, null);
			WholeMapItem.WMSpecialIcon wmspecialIcon = default(WholeMapItem.WMSpecialIcon);
			Map2d map2d = null;
			if (wmi == null || wmi.SpecialPos.valid)
			{
				if (type != WMIcon.TYPE.BENCH)
				{
					return;
				}
				string s = _Mp.Meta.GetS("wholemap_pos");
				if (TX.noe(s) || TX.noe(sppos_key))
				{
					X.de("WMIconCreator::ctor - WM レイヤーを指定しない場合、 Map側Metaに wholemap_pos で WMSpecialIcon を指定しなければなりません", null);
					return;
				}
				WholeMapItem.WMSpecialIcon specialPosition = this.Wm.getSpecialPosition(s);
				if (!specialPosition.valid)
				{
					return;
				}
				WholeMapItem.WMSpecialIcon specialPosition2 = this.Wm.getSpecialPosition(specialPosition.Wmi.Lay.name + ".." + sppos_key);
				if (!specialPosition2.valid)
				{
					return;
				}
				this.Wm.assignHiddenIconDeperture(sppos_key, type, this.Mp, x, y, null);
				wmspecialIcon = specialPosition2;
				map2d = (this.Mp = specialPosition2.Wmi.SrcMap);
			}
			else
			{
				STB stb = TX.PopBld(_Mp.key, 0) + "_";
				if (sppos_key != null && TX.isStart(sppos_key, _Mp.key, 0))
				{
					STB stb2 = TX.PopBld(sppos_key, 0);
					stb2.Splice(0, _Mp.key.Length + 1);
					stb2.Insert(0, "SPI_");
					WholeMapItem.WMSpecialIcon specialPosition3 = this.Wm.getSpecialPosition(_Mp.key, stb2, true);
					if (specialPosition3.valid)
					{
						wmspecialIcon = specialPosition3;
						sppos_key = stb2.ToString();
						this.Wm.assignHiddenIconDeperture(sppos_key, type, this.Mp, x, y, null);
					}
					else
					{
						sppos_key = null;
					}
					TX.ReleaseBld(stb2);
				}
				else
				{
					sppos_key = null;
				}
				TX.ReleaseBld(stb);
			}
			if (wmspecialIcon.valid)
			{
				if (map2d == null)
				{
					map2d = this.Mp;
				}
				map2d.prepared = true;
				x = X.IntR((wmspecialIcon.SrcLP.mapfocx - wmspecialIcon.Wmi.Rc.x) / wmspecialIcon.Wmi.Rc.width * (float)(map2d.clms - map2d.crop * 2) + (float)map2d.crop);
				y = X.IntR((wmspecialIcon.SrcLP.mapfocy - wmspecialIcon.Wmi.Rc.y) / wmspecialIcon.Wmi.Rc.height * (float)(map2d.rows - map2d.crop * 2) + (float)map2d.crop);
			}
			this.Ico = this.Wm.GetIconFor(this.Mp, x, y, type, sf_key, 0);
			if (this.Ico == null)
			{
				this.Ico = new WMIcon(type);
				this.Ico.x = (ushort)x;
				this.Ico.y = (ushort)y;
			}
			if (sf_key != null)
			{
				this.Ico.sf_key = sf_key;
			}
			if (sppos_key != null)
			{
				this.Ico.sppos_key = sppos_key;
			}
		}

		public WMIconCreator notice()
		{
			if (this.Ico == null || this.Ico.noticed)
			{
				return this;
			}
			this.Ico.noticed = true;
			this.Wm.assignIcon(this.Mp, this.Ico);
			return this;
		}

		public WMIcon getIcon()
		{
			return this.Ico;
		}

		public WMIconCreator recheck()
		{
			if (this.Ico == null)
			{
				return this;
			}
			this.notice();
			this.Ico.cleared = false;
			return this;
		}

		public readonly Map2d Mp;

		private WholeMapItem Wm;

		private WMIcon Ico;
	}
}
