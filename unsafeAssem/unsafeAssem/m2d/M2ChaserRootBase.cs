using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2ChaserRootBase<T> where T : M2RegionBase
	{
		public bool is_root
		{
			get
			{
				return this.Parent == null;
			}
		}

		public M2ChaserRootBase<T> Init(float _x, float _y, T _Region)
		{
			this.Parent = null;
			this.x = _x;
			this.y = _y;
			this.Region = _Region;
			this.len_to_previous = 0f;
			return this;
		}

		public M2ChaserRootBase<T> Init(T _Region, M2ChaserRootBase<T> _Parent, float sizex, float sizey)
		{
			this.Parent = _Parent;
			this.Region = _Region;
			Rect coveringArea = this.Parent.Region.getCoveringArea(_Region);
			if (coveringArea.width < sizex * 2f || coveringArea.height < sizey * 2f)
			{
				return null;
			}
			this.x = coveringArea.x + coveringArea.width * 0.5f;
			this.y = coveringArea.y + coveringArea.height * 0.5f;
			this.len_to_previous = X.LENGTHXYS(this.Parent.x, this.Parent.y, this.x, this.y);
			return this;
		}

		public float walk_len_total
		{
			get
			{
				M2ChaserRootBase<T> m2ChaserRootBase = this;
				float num = 0f;
				while (m2ChaserRootBase != null)
				{
					num += m2ChaserRootBase.len_to_previous;
					m2ChaserRootBase = m2ChaserRootBase.Parent;
				}
				return num;
			}
		}

		public int calcGeneration()
		{
			if (this.Parent != null)
			{
				return this.Parent.calcGeneration() + 1;
			}
			return 0;
		}

		public M2ChaserRootBase<T> getParent(int generation_back)
		{
			if (generation_back == 0 || this.Parent == null)
			{
				return this;
			}
			return this.Parent.getParent(generation_back - 1);
		}

		public bool alreadyTouched(Bool1024 B)
		{
			return this.Region != null && B[this.Region.index];
		}

		public bool alreadyTouched(T Wr)
		{
			return this.Region == Wr || (this.Parent != null && this.Parent.alreadyTouched(Wr));
		}

		public float len_to_previous;

		public T Region;

		public float x;

		public float y;

		public M2ChaserRootBase<T> Parent;
	}
}
