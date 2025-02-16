using System;
using m2d;
using XX;

namespace nel
{
	public class M2LabelPointSwitchContainer : M2LabelPoint
	{
		public M2LabelPointSwitchContainer(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public override void initActionPre()
		{
			base.initActionPre();
			this.switch_index = 0;
			this.Meta = new META(this.comment);
		}

		public META Meta;

		public int switch_index;
	}
}
