using System;
using UnityEngine;

namespace m2d
{
	public interface ICarryable
	{
		bool moveWithFoot(float dx, float dy, Collider2D _Collider, M2BlockColliderContainer BCCCarrier, bool no_collider_lock = false);

		void setShiftPixel(IFootable F, float pixel_x, float pixel_y);

		void initJump(bool recheck_foot = false, bool no_footstamp_snd = false, bool remain_foot_margin = false);
	}
}
