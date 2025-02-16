using System;
using System.Collections.Generic;
using PixelLiner.PixelLinerLib;

namespace nel
{
	public struct ItemStorageMem
	{
		public ItemStorageMem(ItemStorage _Inv, ByteArray SrcBa = null)
		{
			this.Inv = _Inv;
			this.Ba = SrcBa ?? new ByteArray((uint)this.Inv.bytearray_about_bits);
			this.Ba.position = 0UL;
			this.Ba.Length = 0UL;
			this.Inv.writeBinaryTo(this.Ba);
		}

		public void Undo()
		{
			if (this.Ba != null && this.Inv != null)
			{
				this.Ba.position = 0UL;
				this.Inv.readBinaryFrom(this.Ba, true, true, false, 9, false);
			}
		}

		public static void Prepare(ref List<ItemStorageMem> A, NelM2DBase M2D)
		{
			List<ItemStorageMem> list = A ?? new List<ItemStorageMem>(2);
			int num = 0;
			int i = 0;
			while (i < 2)
			{
				ItemStorage itemStorage;
				if (i == 0)
				{
					itemStorage = M2D.IMNG.getInventory();
					goto IL_003B;
				}
				if (M2D.canAccesableToHouseInventory())
				{
					itemStorage = M2D.IMNG.getHouseInventory();
					goto IL_003B;
				}
				IL_00AC:
				i++;
				continue;
				IL_003B:
				num++;
				if (A != null)
				{
					for (int j = A.Count - 1; j >= 0; j--)
					{
						if (A[j].Inv == itemStorage || A[j].Inv == null)
						{
							list[j] = new ItemStorageMem(itemStorage, A[j].Ba);
							itemStorage = null;
							break;
						}
					}
				}
				if (itemStorage != null)
				{
					list.Add(new ItemStorageMem(itemStorage, null));
					goto IL_00AC;
				}
				goto IL_00AC;
			}
			for (int k = list.Count - 1; k >= num; k--)
			{
				ItemStorageMem itemStorageMem = A[k];
				itemStorageMem.Inv = null;
				A[k] = itemStorageMem;
			}
			if (A == null)
			{
				A = list;
			}
		}

		public static void Release(List<ItemStorageMem> A)
		{
			if (A != null)
			{
				for (int i = A.Count - 1; i >= 0; i--)
				{
					ItemStorageMem itemStorageMem = A[i];
					itemStorageMem.Inv = null;
					A[i] = itemStorageMem;
				}
			}
		}

		public static void UndoAll(List<ItemStorageMem> A)
		{
			if (A != null)
			{
				for (int i = A.Count - 1; i >= 0; i--)
				{
					A[i].Undo();
				}
			}
		}

		public ItemStorage Inv;

		public readonly ByteArray Ba;
	}
}
