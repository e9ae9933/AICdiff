using System;

namespace nel
{
	public interface IWindApplyable
	{
		float getWindApplyLevel(WindItem Wind);

		void applyWindFoc(WindItem Wind, float vx, float vy);
	}
}
