using System;
using XX;

namespace nel
{
	public abstract class MgFDHolder
	{
		public MgFDHolder()
		{
			this.FD_Run = new MagicItem.FnMagicRun(this.run);
			this.FD_Draw = new MagicItem.FnMagicRun(this.draw);
		}

		public abstract bool run(MagicItem Mg, float fcnt);

		public abstract bool draw(MagicItem Mg, float fcnt);

		protected static HaloDrawer Halo
		{
			get
			{
				return MagicItem.Halo;
			}
		}

		public virtual MagicItem initFunc(MagicItem Mg)
		{
			Mg.initFunc(this.FD_Run, this.FD_Draw);
			return Mg;
		}

		public virtual MagicNotifiear GetNotifiear()
		{
			return null;
		}

		protected bool isWrongMagic(MagicItem Mg, M2MagicCaster Mv)
		{
			return Mg.MGC.Notf.isWrongMagic(Mg, Mv);
		}

		protected readonly MagicItem.FnMagicRun FD_Run;

		protected readonly MagicItem.FnMagicRun FD_Draw;
	}
}
