using System;
using System.Collections.Generic;
using XX;

namespace m2d
{
	public sealed class M2BorderPt : BDVector
	{
		public M2BorderPt Init(float _x, float _y, List<M2BlockColliderContainer.M2PtPos> _APt)
		{
			base.Set(_x, _y);
			if (this.APt == null)
			{
				this.APt = new List<M2BlockColliderContainer.M2PtPos>(_APt.Count);
			}
			else
			{
				this.APt.Clear();
			}
			this.APt.AddRange(_APt);
			return this;
		}

		public M2BorderPt Init(float _x, float _y, M2BlockColliderContainer.M2PtPos _Pt)
		{
			base.Set(_x, _y);
			if (this.APt == null)
			{
				this.APt = new List<M2BlockColliderContainer.M2PtPos>(1);
			}
			else
			{
				this.APt.Clear();
			}
			this.APt.Add(_Pt);
			return this;
		}

		public M2BorderPt Init(float _x, float _y)
		{
			base.Set(_x, _y);
			if (this.APt == null)
			{
				this.APt = new List<M2BlockColliderContainer.M2PtPos>(1);
			}
			else
			{
				this.APt.Clear();
			}
			return this;
		}

		public override void Merge(BDVector Src)
		{
			if (Src is M2BorderPt)
			{
				List<M2BlockColliderContainer.M2PtPos> apt = (Src as M2BorderPt).APt;
				if (this.APt != apt)
				{
					this.APt.AddRange(apt);
				}
			}
		}

		public static M2BorderPt CreateDefaultBPT(float _x, float _y)
		{
			return new M2BorderPt().Init(_x, _y);
		}

		public List<M2BlockColliderContainer.M2PtPos> APt;
	}
}
