using System;
using m2d;
using XX;

namespace nel
{
	public abstract class NelLpRunner : NelLp, IRunAndDestroy
	{
		public NelLpRunner(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public abstract bool run(float fcnt);

		public void destruct()
		{
			this.closeAction(true);
		}

		public override void initActionPre()
		{
			base.initActionPre();
			this.Meta = new META(this.comment);
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			this.Mp.remRunnerObject(this);
		}

		public META Meta;
	}
}
