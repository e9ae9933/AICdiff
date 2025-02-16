using System;
using XX;

namespace nel.gm
{
	public interface IStatDangerousDescriptor
	{
		bool createGMDesc(UiGMCStat Gmc, Designer Ds, out aBtn FirstBtn, out aBtn LastBtn);

		string getTabIconKey(UiGMCStat Gmc, out uint color);
	}
}
