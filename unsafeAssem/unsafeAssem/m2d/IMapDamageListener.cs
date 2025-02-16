using System;
using XX;

namespace m2d
{
	public interface IMapDamageListener
	{
		DRect getMapBounds(DRect BufRc);

		M2BlockColliderContainer.BCCLine get_FootBCC();

		void remFootListener(IBCCFootListener Lsn);

		void applyMapDamage(M2MapDamageContainer.M2MapDamageItem MapDmg, M2BlockColliderContainer.BCCLine Bcc);

		void applyVelocity(FOCTYPE type, float velocity_x, float velocity_y);

		bool isCenterPr();
	}
}
