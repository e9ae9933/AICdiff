using System;
using XX;

namespace m2d
{
	public interface IBCCFootListener
	{
		uint getFootableAimBits();

		DRect getMapBounds(DRect BufRc);

		bool footedInit(M2BlockColliderContainer.BCCLine Bcc, IMapDamageListener Fd);

		bool footedQuit(IMapDamageListener Fd, bool from_jump_init = false);

		void rewriteFootType(M2BlockColliderContainer.BCCLine Bcc, IMapDamageListener Fd, ref string s);
	}
}
