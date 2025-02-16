using System;
using m2d;

namespace nel
{
	public class MgBurst : MgFDHolder
	{
		public override bool run(MagicItem Mg, float fcnt)
		{
			if (Mg.t == 0f)
			{
				Mg.sz = 9f;
			}
			Mg.sx = Mg.Cen.x;
			Mg.sy = Mg.Cen.y + 0.9f;
			Mg.Ray.PosMap(Mg.sx, Mg.sy);
			if (!Mg.Caster.canHoldMagic(Mg) || Mg.Atk0 == null)
			{
				return false;
			}
			Mg.Atk0.resetAttackCount();
			if (fcnt > 0f)
			{
				if (!Mg.exploded)
				{
					Mg.Ray.check_other_hit = false;
					Mg.MGC.CircleCast(Mg, Mg.Ray, Mg.Atk0, HITTYPE.WALL);
					Mg.explode(false);
				}
				if (Mg.t >= 8f)
				{
					Mg.kill(-1f);
					return false;
				}
			}
			return true;
		}

		public override bool draw(MagicItem Mg, float fcnt)
		{
			return true;
		}
	}
}
