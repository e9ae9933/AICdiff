using System;
using XX;

namespace nel
{
	public abstract class MgFDHolderWithMemoryClass<T> : MgFDHolder where T : class, IDisposable
	{
		public MgFDHolderWithMemoryClass(Func<T> _FD_Create)
		{
			this.Pool = new ClsPool<T>(_FD_Create, 1);
		}

		public override MagicItem initFunc(MagicItem Mg)
		{
			base.initFunc(Mg);
			Mg.releasePooledObject(false, true);
			Mg.Other = this.PopMem();
			return Mg;
		}

		private T PopMem()
		{
			return this.Pool.Pool();
		}

		public T ReleaseMem(T Target)
		{
			return this.Pool.Release(Target);
		}

		private ClsPool<T> Pool;
	}
}
