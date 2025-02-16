using System;

namespace m2d
{
	public interface ITortureListener
	{
		bool setTortureAnimation(string pose_name, int cframe, int loop_to);

		void setToTortureFix(float x, float y);

		void runPostTorture();
	}
}
