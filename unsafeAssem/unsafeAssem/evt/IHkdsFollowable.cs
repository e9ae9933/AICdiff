using System;
using UnityEngine;

namespace evt
{
	public interface IHkdsFollowable
	{
		Vector3 getHkdsDepertPos();

		bool checkPositionMoved(IMessageContainer Msg);
	}
}
