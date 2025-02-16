using System;

namespace m2d
{
	public class DungeonHouseIn : DungeonHouse
	{
		public DungeonHouseIn(string _key, DungeonContainer _DGN)
			: base(_key, _DGN)
		{
			this.foot_type = "floor";
			this.use_window_remover = true;
			this.color_family_key = "house_in";
			this.Achip_top_light_level[0] = 1f;
			this.Achip_top_light_level[1] = 0.7f;
			this.Achip_top_light_level[2] = 0.4f;
			this.Aborder_light_level[0] = 1f;
			this.Aborder_light_level[1] = 0.7f;
			this.Aborder_light_level[2] = 0.4f;
		}
	}
}
