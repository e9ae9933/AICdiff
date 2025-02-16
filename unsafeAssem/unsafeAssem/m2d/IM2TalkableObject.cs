using System;
using UnityEngine;

namespace m2d
{
	public interface IM2TalkableObject
	{
		bool SubmitTalkable(M2MoverPr SubmitFrom);

		int canTalkable(bool when_battle_busy);

		void FocusTalkable();

		void BlurTalkable(bool event_init);

		Vector4 getMapPosAndSizeTalkableObject();
	}
}
