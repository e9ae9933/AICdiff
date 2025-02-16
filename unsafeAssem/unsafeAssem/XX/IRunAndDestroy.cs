using System;

namespace XX
{
	public interface IRunAndDestroy
	{
		bool run(float fcnt);

		void destruct();
	}
}
