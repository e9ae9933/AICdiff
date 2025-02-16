using System;

namespace m2d
{
	public interface IBCCEventListener
	{
		void BCCInitializing(M2BlockColliderContainer BCC);

		bool isBCCListenerActive(M2BlockColliderContainer.BCCLine BCC);

		void BCCtouched(M2BlockColliderContainer.BCCLine BCC, M2FootManager FootD);

		bool runBCCEvent(M2BlockColliderContainer.BCCLine BCC, M2FootManager FootD);
	}
}
