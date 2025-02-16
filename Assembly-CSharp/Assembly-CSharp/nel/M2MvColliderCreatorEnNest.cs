using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class M2MvColliderCreatorEnNest : M2MvColliderCreatorEn
	{
		public M2MvColliderCreatorEnNest(NelEnemy En, int capacity = 1)
			: base(En, 0.56f, 0.4f)
		{
			this.ANest = new CCNestItem[capacity];
		}

		public CCNestItem Create(string name)
		{
			CCNestItem ccnestItem = new CCNestItem(this, name);
			X.pushToEmptyRI<CCNestItem>(ref this.ANest, ccnestItem, -1);
			this.path_count++;
			this.need_recreate = true;
			return ccnestItem;
		}

		protected override void recreateExecute()
		{
			base.recreateExecute();
			Map2d mp = base.Mp;
			for (int i = 1; i < this.path_count; i++)
			{
				CCNestItem ccnestItem = this.ANest[i - 1];
				Vector2[] array = new Vector2[4];
				Vector2 vector = ccnestItem.getUSize(mp) * 0.5f;
				if (ccnestItem.rotR != 0f)
				{
					vector = X.ROTV2e(vector, ccnestItem.rotR);
				}
				Vector2 ushift = ccnestItem.getUShift(mp);
				array[0].Set(ushift.x - vector.x, ushift.y - vector.y);
				array[1].Set(ushift.x - vector.x, ushift.y + vector.y);
				array[2].Set(ushift.x + vector.x, ushift.y + vector.y);
				array[3].Set(ushift.x + vector.x, ushift.y - vector.y);
				this.Cld.SetPath(i, array);
			}
		}

		public CCNestItem[] ANest;
	}
}
