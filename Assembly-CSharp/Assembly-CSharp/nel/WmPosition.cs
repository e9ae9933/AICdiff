using System;
using System.Collections.Generic;
using XX;

namespace nel
{
	public struct WmPosition
	{
		public WmPosition(WholeMapItem _Wm, WholeMapItem.WMItem _Wmi, WMSpecialIcon _SpIco)
		{
			this.Wm = _Wm;
			this.Wmi = _Wmi;
			this.SpIco = _SpIco;
		}

		public bool valid
		{
			get
			{
				return this.Wm != null;
			}
		}

		public bool valid2
		{
			get
			{
				return this.Wm != null && this.Wmi != null;
			}
		}

		public bool isSame(WmPosition P)
		{
			return this.Wm == P.Wm && this.Wmi == P.Wmi && this.SpIco.Equals(P.SpIco);
		}

		public bool getPos(NelM2DBase M2D, WholeMapItem Showing_From, List<MapPosition> APos)
		{
			if (Showing_From == this.Wm)
			{
				if (!this.SpIco.valid)
				{
					if (this.Wmi == null)
					{
						return false;
					}
					APos.Add(new MapPosition(this.Wmi.Rc.cx, this.Wmi.Rc.cy, this.Wmi.SrcMap));
				}
				else
				{
					APos.Add(new MapPosition(this.SpIco.SrcLP.mapfocx, this.SpIco.SrcLP.mapfocy, this.Wmi.SrcMap));
				}
				return true;
			}
			bool flag = false;
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				M2D.WA.copyDepert(M2D, Showing_From, blist, this.Wm.text_key);
				if (blist.Count > 0)
				{
					int count = blist.Count;
					for (int i = 0; i < count; i++)
					{
						WholeMapItem.WMItem wmi = Showing_From.GetWmi(M2D.Get(blist[i], false), null);
						if (wmi != null)
						{
							LabelPointListener<WholeMapItem.WMTransferPoint> labelPointListener = wmi.WpCon.beginAll(WmPosition.LplBuffer);
							while (labelPointListener.next())
							{
								int rectLength = labelPointListener.cur.getRectLength();
								for (int j = 0; j < rectLength; j++)
								{
									WholeMapItem.WMTransferPoint.WMRectItem rect = labelPointListener.cur.getRect(j);
									if (rect.index < 0 && M2D.WM.GetWholeFor(rect.Mp, true) == this.Wm)
									{
										AIM aim = rect.getAim();
										APos.Add(new MapPosition(wmi.Rc.x + rect.cx + (float)CAim._XD(aim, 1) * 1.5f, wmi.Rc.y + rect.cy - (float)CAim._YD(aim, 1) * 1.5f, wmi.SrcMap));
										flag = true;
									}
								}
							}
							List<WMSpecialIcon> specialPositionList = Showing_From.getSpecialPositionList(blist[i]);
							if (specialPositionList != null)
							{
								for (int k = specialPositionList.Count - 1; k >= 0; k--)
								{
									WMSpecialIcon wmspecialIcon = specialPositionList[k];
									if (wmspecialIcon.go_other_wm == this.Wm.text_key)
									{
										APos.Add(new MapPosition(wmspecialIcon.SrcLP.mapcx, wmspecialIcon.SrcLP.mapcy - 0.5f, wmspecialIcon.Wmi.SrcMap));
										flag = true;
									}
								}
							}
						}
					}
				}
			}
			return flag;
		}

		public WholeMapItem Wm;

		public WholeMapItem.WMItem Wmi;

		public WMSpecialIcon SpIco;

		private static readonly LabelPointListener<WholeMapItem.WMTransferPoint> LplBuffer = new LabelPointListener<WholeMapItem.WMTransferPoint>();
	}
}
