using System;

namespace m2d
{
	internal class M2MovePatCrawlAround : M2MovePatWalkAround
	{
		public M2MovePatCrawlAround(M2EventItem _Mv)
			: base(_Mv, M2EventItem.MOV_PAT.CRAWLAROUND_LR)
		{
			this.movtf = -1f;
			this.wait_time_min = (this.wait_time_max = -1f);
			this.walk_time_min = 70f;
			this.walk_time_max = 140f;
			this.walk_speed0 = 0.018f;
			this.reactivate_move_delay = -1f;
			this.dep_walk_pose = "crawl";
		}
	}
}
