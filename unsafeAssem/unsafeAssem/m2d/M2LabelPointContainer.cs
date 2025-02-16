using System;
using System.Text.RegularExpressions;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2LabelPointContainer : LabelPointContainer<M2LabelPoint>
	{
		public M2LabelPointContainer(M2MapLayer _Lay, M2LabelPoint.FnCreateLp _fnCreateLabelPoint)
		{
			this.Lay = _Lay;
			this.fnCreateLabelPoint = _fnCreateLabelPoint;
		}

		public float CLEN
		{
			get
			{
				return this.Lay.CLEN;
			}
		}

		public override LabelPointContainer<M2LabelPoint> Clear()
		{
			for (int i = 0; i < base.Length; i++)
			{
				base.Get(i).index = -1;
				base.Get(i).closeAction(false);
			}
			return base.Clear();
		}

		public override M2LabelPoint Rem(string key)
		{
			M2LabelPoint m2LabelPoint = base.Rem(key);
			if (m2LabelPoint != null)
			{
				m2LabelPoint.index = -1;
			}
			return m2LabelPoint;
		}

		public override M2LabelPoint Get(string key, bool no_make = true, bool no_error = false)
		{
			M2LabelPoint m2LabelPoint = base.Get(key, true, no_error);
			if (!no_make && m2LabelPoint == null)
			{
				m2LabelPoint = this.fnCreateLabelPoint(null, this.Lay);
				m2LabelPoint.key = key;
				base.Add(m2LabelPoint);
			}
			return m2LabelPoint;
		}

		public override void reindex()
		{
			for (int i = 0; i < base.Length; i++)
			{
				base.Get(i).index = i;
			}
		}

		public void activateAll()
		{
			for (int i = 0; i < base.Length; i++)
			{
				base.Get(i).deactivate();
			}
		}

		public void deactivateAll()
		{
			for (int i = 0; i < base.Length; i++)
			{
				base.Get(i).deactivate();
			}
		}

		public Vector2 getPosMv(string key, float _shx = 0f, float _shy = 0f, M2Mover Mv = null)
		{
			M2Mover mvPos = M2LabelPointContainer._MvPos;
			M2LabelPointContainer._MvPos = Mv;
			Vector2 pos = base.getPos(key, _shx, _shy);
			M2LabelPointContainer._MvPos = mvPos;
			return pos;
		}

		public override Vector2 getPosSideCheck(string pt_pos, DRect Pt)
		{
			Vector2 vector = base.getPosSideCheck(pt_pos, Pt);
			if (M2LabelPointContainer._MvPos != null)
			{
				if (pt_pos.IndexOf("W") != -1)
				{
					vector = (Pt as M2LabelPoint).getWalkable(M2LabelPointContainer._MvPos, ((vector.x == -1000f) ? Pt.getRandom() : vector) * this.Lay.Mp.rCLEN) * this.Lay.CLEN;
				}
				if (pt_pos.IndexOf("b") >= 0)
				{
					if (vector.x == -1000f)
					{
						vector.x = Pt.cx;
					}
					if (vector.y == -1000f)
					{
						vector.y = Pt.cy;
					}
					vector.y = this.Lay.Mp.getFootableY((float)((int)vector.x) * this.Lay.Mp.rCLEN, (int)(vector.y * this.Lay.Mp.rCLEN), this.Lay.Mp.rows, true, Pt.bottom * this.Lay.Mp.rCLEN, false, true, true, 0f) * this.Lay.CLEN - M2LabelPointContainer._MvPos.sizey * this.Lay.CLEN;
				}
			}
			return vector;
		}

		public readonly M2MapLayer Lay;

		public M2LabelPoint.FnCreateLp fnCreateLabelPoint;

		private static readonly Regex RegFindLabelPoint = new Regex("(\\w+)(?: *\\. *(\\.\\w))?");

		private static M2Mover _MvPos;
	}
}
