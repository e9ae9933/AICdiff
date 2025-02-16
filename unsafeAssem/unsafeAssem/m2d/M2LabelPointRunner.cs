using System;
using XX;

namespace m2d
{
	public abstract class M2LabelPointRunner : M2LabelPoint, IRunAndDestroy
	{
		public M2LabelPointRunner(string _key, int _index, M2MapLayer _Lay, bool add_runner = true)
			: base(_key, _index, _Lay)
		{
			if (add_runner)
			{
				this.Mp.addRunnerObject(this);
			}
		}

		public abstract bool run(float fcnt);

		public void destruct()
		{
			this.closeAction(true);
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (normal_map)
			{
				this.Meta = new META(this.comment);
			}
		}

		public META Meta;
	}
}
