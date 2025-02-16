using System;
using XX;

namespace m2d
{
	public class DungeonHouseAtelier : DungeonHouse
	{
		public DungeonHouseAtelier(string _key, DungeonContainer _DGN)
			: base(_key, _DGN)
		{
			this.foot_type = "glass";
			this.color_family_key = "house_in_atelier";
			X.ALLV<float>(this.Achip_top_light_level, 1f);
			X.ALLV<float>(this.Aborder_light_level, 0f);
			this.is_lower_bgm_ = true;
		}
	}
}
