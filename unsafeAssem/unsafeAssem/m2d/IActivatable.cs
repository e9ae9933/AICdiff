using System;

namespace m2d
{
	public interface IActivatable
	{
		void activate();

		void deactivate();

		string getActivateKey();
	}
}
