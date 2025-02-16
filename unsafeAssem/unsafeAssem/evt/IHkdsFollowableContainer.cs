using System;

namespace evt
{
	public interface IHkdsFollowableContainer
	{
		IHkdsFollowable FindHkdsFollowableObject(string key);
	}
}
