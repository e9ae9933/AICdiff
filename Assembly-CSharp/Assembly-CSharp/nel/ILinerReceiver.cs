using System;
using XX;

namespace nel
{
	public interface ILinerReceiver
	{
		void activateLiner(bool immediate);

		void deactivateLiner(bool immediate);

		bool initEffect(bool activating, ref DRect RcEffect);
	}
}
