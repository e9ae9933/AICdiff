using System;

namespace XX
{
	public interface IDesignerPosSetableBlock
	{
		float get_swidth_px();

		float get_sheight_px();

		void posSetA(float _sx, float _sy, float _dx, float _dy, bool no_reset_time = false);
	}
}
