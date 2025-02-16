using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public abstract class BorderVectorListener
	{
		public BorderVectorListener(Func<float, float, BDVector> _FnCreateDefault)
		{
			this.FnCreateDefault = _FnCreateDefault ?? new Func<float, float, BDVector>(BDVector.CreateDefault);
		}

		protected Func<float, float, BDVector> FnCreateDefault;

		public interface IPositionListener
		{
			void setPathCount(int i);

			void SetPath(int i, List<BDVector> Apos, FnZoom fnCalcX, FnZoom fnCalcY, int len);
		}

		public sealed class PositionListenerPolygonCollider : BorderVectorListener.IPositionListener
		{
			public PositionListenerPolygonCollider(PolygonCollider2D _Cld)
			{
				this.Cld = _Cld;
			}

			public void setPathCount(int i)
			{
				this.Cld.pathCount = i;
			}

			public void SetPath(int i, List<BDVector> Apos, FnZoom fnCalcX, FnZoom fnCalcY, int len)
			{
				Vector2[] array = new Vector2[len];
				for (int j = len - 1; j >= 0; j--)
				{
					array[j] = Apos[j].Convert(fnCalcX, fnCalcY);
				}
				this.Cld.SetPath(i, array);
			}

			public PolygonCollider2D Cld;
		}
	}
}
