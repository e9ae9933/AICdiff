using System;
using System.Collections.Generic;

namespace nel
{
	public sealed class PuzzSnapShot
	{
		public PuzzSnapShot(int capacity = 1)
		{
			this.AItm = new List<PuzzSnapShot.RevertItem>(capacity);
		}

		public bool isSame(PuzzSnapShot S)
		{
			int count = this.AItm.Count;
			for (int i = 0; i < count; i++)
			{
				PuzzSnapShot.RevertItem revertItem = this.AItm[i];
				PuzzSnapShot.RevertItem revertItem2 = S.Get(revertItem.Target);
				if (revertItem2 == null || !revertItem2.isSame(revertItem))
				{
					return false;
				}
			}
			return true;
		}

		public PuzzSnapShot Add(PuzzSnapShot.RevertItem Rvi)
		{
			this.AItm.Add(Rvi);
			return this;
		}

		public PuzzSnapShot Add(IPuzzRevertable _Target)
		{
			PuzzSnapShot.RevertItem revertItem = new PuzzSnapShot.RevertItem(_Target);
			_Target.makeSnapShot(revertItem);
			this.AItm.Add(revertItem);
			return this;
		}

		public PuzzSnapShot Add(List<IPuzzRevertable> ATarget)
		{
			if (ATarget == null)
			{
				return this;
			}
			int count = ATarget.Count;
			if (this.AItm.Capacity < this.AItm.Count + count)
			{
				this.AItm.Capacity = this.AItm.Count + count;
			}
			for (int i = 0; i < count; i++)
			{
				this.Add(ATarget[i]);
			}
			return this;
		}

		public PuzzSnapShot.RevertItem Get(int i)
		{
			return this.AItm[i];
		}

		public PuzzSnapShot.RevertItem Get(IPuzzRevertable P)
		{
			for (int i = this.AItm.Count - 1; i >= 0; i--)
			{
				PuzzSnapShot.RevertItem revertItem = this.AItm[i];
				if (revertItem.Target == P)
				{
					return revertItem;
				}
			}
			return null;
		}

		public PuzzSnapShot.RevertItem GetLast()
		{
			return this.AItm[this.AItm.Count - 1];
		}

		public void revertExecute(NelM2DBase M2D)
		{
			int count = this.AItm.Count;
			for (int i = 0; i < count; i++)
			{
				PuzzSnapShot.RevertItem revertItem = this.AItm[i];
				try
				{
					revertItem.Target.puzzleRevert(revertItem);
				}
				catch
				{
				}
			}
			if (M2D.MIST != null)
			{
				M2D.MIST.initFirstProgress(30);
			}
		}

		public List<PuzzSnapShot.RevertItem> AItm;

		public sealed class RevertItem
		{
			public RevertItem(IPuzzRevertable _Targ)
			{
				this.Target = _Targ;
			}

			public bool isSame(PuzzSnapShot.RevertItem I)
			{
				return this.Target == I.Target && this.x == I.x && this.y == I.y && this.z == I.z && this.time == I.time;
			}

			public IPuzzRevertable Target;

			public float x;

			public float y;

			public float z;

			public int time;
		}
	}
}
