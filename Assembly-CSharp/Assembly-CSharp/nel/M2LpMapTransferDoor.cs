using System;
using m2d;
using XX;

namespace nel
{
	public sealed class M2LpMapTransferDoor : M2LpMapTransferBase
	{
		public M2LpMapTransferDoor(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
			this.need_wm_rect = false;
			this.use_out_collider = 1;
		}

		public override void getDepertureRect()
		{
		}

		public override void activate()
		{
			base.activate();
			this.Lay.Mp.activateToChip(this.mapx, this.mapy, this.mapw, this.maph, false, 1UL << this.Lay.index, null);
		}

		public override void initAction(bool normal_map)
		{
			if (!normal_map)
			{
				return;
			}
			base.initAction(normal_map);
			this.walk_aim = this.Meta.getDirsI("walk_aim", 0, false, 0, -1);
			this.screen_top_layer = this.Meta.GetB("screen_top_layer", false);
			Map2d mp = this.Lay.Mp;
			Map2d map2d = this.M2D.Get(this.Meta.GetS("goto") ?? "", false);
			if (map2d == null || map2d == mp)
			{
				return;
			}
			bool flag = this.Meta.GetI("is_out", 0, 0) != 0;
			int dirsI = this.Meta.getDirsI("aim", 0, false, 0, -1);
			int num = this.Meta.getDirsI("aim", 0, false, 1, -1);
			M2EventContainer eventContainer = mp.getEventContainer();
			if (!eventContainer.Get(this.key, true, true))
			{
				this.Ev = eventContainer.CreateAndAssign(this.key);
				this.Ev.setToArea(this);
				string text;
				if (this.walk_aim >= 0)
				{
					AIM aim = (AIM)this.walk_aim;
					text = aim.ToString();
				}
				else
				{
					text = "-1";
				}
				string text2 = text;
				string text3 = text2;
				int dirsI2 = this.Meta.getDirsI("d_walk_aim", 0, false, 0, -1);
				if (dirsI2 >= 0)
				{
					AIM aim = (AIM)dirsI2;
					text3 = aim.ToString();
					if (num < 0)
					{
						num = dirsI2;
					}
				}
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.Add("SEND_EVENT_CORRUPTION ").AR("PRE_UNLOAD");
					base.event_script_transfer_head(stb, text2, true, false, map2d, this.walk_aim, false);
					stb.ARd("COOK_ADD_WALK_COUNT", " ", map2d.key, null, null, null);
					if (text2 == "-1")
					{
						stb.Add("#MS_ % '>[ ", (int)(base.mapcx * base.CLEN), " ,+=0 :").Add(13).Add(" ] '")
							.Ret("\n");
					}
					string text4 = this.Meta.GetSi(0, "jump_at");
					if (text4 != null)
					{
						text4 = "!" + text4;
					}
					else
					{
						text4 = this.key;
					}
					base.event_script_transfer_body(stb, text3, text4, false);
					if (num >= 0)
					{
						num = (int)CAim.get_opposite((AIM)num);
						base.event_script_transfer_foot(stb, num, null, 70, false);
					}
					else
					{
						if (dirsI >= 0)
						{
							int num2 = CAim._XD(dirsI, 1) * 22;
							stb.Add("  #MS_ % '>+[", num2, ",0 :22 ]' ").Ret("\n");
						}
						else
						{
							stb.Add("  #MS_ % 'P[walk~~] W22' ").Ret("\n");
						}
						base.event_script_transfer_foot_evt(stb);
					}
					stb.AR("WAIT_MOVE");
					if (this.walk_aim == -1)
					{
						string[] array = this.Meta.Get("check_desc");
						string text5 = ((array != null) ? array[0] : null);
						if (TX.valid(text5) && array.Length >= 2 && TX.eval(array[1], "") == 0.0)
						{
							text5 = null;
						}
						if (TX.noe(text5))
						{
							string text6 = null;
							this.Ev.check_desc_name = null;
							if (TX.eval(this.Meta.GetS("check_desc_map"), "") != 0.0)
							{
								text6 = M2MapTitle.get_tx_key(map2d);
							}
							if (text6 == null)
							{
								text6 = (flag ? "EV_access_goout" : "EV_access_into");
							}
							this.Ev.check_desc_name = text6;
						}
						else
						{
							this.Ev.check_desc_name = text5;
						}
						this.Ev.assign("CHECK", stb.ToString(), true);
					}
					else
					{
						this.Ev.assign("STAND", stb.ToString(), true);
					}
				}
				base.fineSF();
			}
		}

		private bool wm_change;

		public int walk_aim = -1;
	}
}
