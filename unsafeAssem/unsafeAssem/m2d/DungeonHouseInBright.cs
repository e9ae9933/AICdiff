using System;

namespace m2d
{
	public class DungeonHouseInBright : DungeonHouse
	{
		public DungeonHouseInBright(string _key, DungeonContainer _DGN)
			: base(_key, _DGN)
		{
			this.foot_type = "woodboard";
			this.use_window_remover = true;
			this.color_family_key = "house_in_bright";
			this.Achip_top_light_level[0] = 1f;
			this.Achip_top_light_level[1] = 0.9f;
			this.Achip_top_light_level[2] = 0.8f;
			this.Aborder_light_level[0] = 1f;
			this.Aborder_light_level[1] = 0.8f;
			this.Aborder_light_level[2] = 0.8f;
		}
	}
}
