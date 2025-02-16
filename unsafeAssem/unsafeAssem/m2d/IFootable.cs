using System;

namespace m2d
{
	public interface IFootable
	{
		IFootable isCarryable(M2FootManager FootD);

		void quitCarry(ICarryable FootD);

		IFootable initCarry(ICarryable FootD);

		float get_carry_vx();

		float get_carry_vy();

		float fixToFootPos(M2FootManager FootD, float x, float y, out float dx, out float dy);
	}
}
