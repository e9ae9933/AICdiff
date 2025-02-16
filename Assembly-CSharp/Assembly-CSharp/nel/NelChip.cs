using System;
using Better;
using m2d;
using XX;

namespace nel
{
	public class NelChip : M2Chip
	{
		public NelChip(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
		}

		public METACImg Meta
		{
			get
			{
				return this.Img.Meta;
			}
		}

		public NelM2DBase NM2D
		{
			get
			{
				return this.Mp.M2D as NelM2DBase;
			}
		}

		public static M2ImageContainer.FnCreateChipPrepartion NelChipPreparation = delegate(BDic<string, M2ImageContainer.FnCreateOneChip> Otg, BDic<string, M2CImgDrawer.FnCreateDrawer> Odr)
		{
			Otg["puzzle_connector"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipPuzzleConnector(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["puzzle_switch"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipPuzzleSwitch(_Lay, drawx, drawy, opacity, rotation, flip, _Img, "puzzle_switch");
			Otg["puzzle_saisenswitch"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipPuzzleSaisenSwitch(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["puzzle_rail_start"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipPuzzleRailStart(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["puzzle_rail"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipPuzzleRail(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["puzzle_box"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipPuzzleBox(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["rail"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipCarrierRail(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["worm_head"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipWormHead(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["mana_weed"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipManaWeed(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["bench"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipBench(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["ukiwa"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipUkiwa(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["drawbridge"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipDrawBridge(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["drawbridge_piece"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipDrawBridgePiece(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["warp_transporter"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipWarp(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["thunder_shooter"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipThunderShooter(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["lavawater_shooter"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipWaterShooter(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["puzzle_barrier_lit"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipBarrierLit(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["water_level"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipWater(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["check_point"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipCheckPoint(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["ladder"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipLadder(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["foot_switch"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipFootSwitch(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["slime_decoy"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipSlimeDecoy(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["hame"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipHamehame(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["wander_tilde"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipWanderTilde(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["wander_puppet"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipWanderPuppet(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["wander_npc"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipWanderNpcSpot(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["jumper"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChipJumperBoard(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			Otg["_"] = (M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img) => new NelChip(_Lay, drawx, drawy, opacity, rotation, flip, _Img);
			M2CImgDrawer.CreateDrawerPreparationDefault(Otg, Odr);
			M2CImgDrawer.FnCreateDrawer DrDef = Odr["_"];
			Odr["mana_weed"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerManaWeed(Md, lay, Cp, Meta);
			};
			Odr["worm_head"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerNoDraw(Md, lay, Cp);
			};
			Odr["worm_area"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerWormArea(Md, lay, Cp);
			};
			Odr["worm_bg"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerWormBg(Md, lay, Cp, Meta, "worm_bg");
			};
			Odr["puzzle_rail_start"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerPuzzleRailStart(Md, lay, Cp);
			};
			Odr["extender"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerPuzzleExtender(Md, lay, Cp, true);
			};
			Odr["check_point"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				if (Meta.Get("bonfire") != null)
				{
					return new M2CImgDrawerCheckBonfire(Md, lay, Cp);
				}
				if (Meta.Get("check_glacier") != null)
				{
					return new M2CImgDrawerCheckGlacier(Md, lay, Cp);
				}
				return DrDef(ref Md, lay, Cp, Meta, Pre_Drawer);
			};
			Odr["item_supply"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				if (lay != 3)
				{
					return new M2CImgDrawerItemSupply(Md, lay, Cp);
				}
				return DrDef(ref Md, lay, Cp, Meta, Pre_Drawer);
			};
			Odr["fire_place"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				int i = Meta.GetI("fire_place", 2, 2);
				if (lay == i)
				{
					return new M2CImgDrawerFirePlace(Md, lay, Cp);
				}
				return DrDef(ref Md, lay, Cp, Meta, Pre_Drawer);
			};
			Odr["alchemy_pot"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				if (lay == 2)
				{
					return new M2CImgDrawerAlchemyPot(Md, lay, Cp);
				}
				return DrDef(ref Md, lay, Cp, Meta, Pre_Drawer);
			};
			Odr["puzzle_barrier_lit"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerBarrierLit(Md, lay, Cp);
			};
			Odr["coin"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerCoin(Md, lay, Cp);
			};
			Odr["bench"] = (Odr["wander_npc"] = (Odr["wander_puppet"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerBench(Md, lay, Cp);
			}));
			Odr["plant_aura"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerPlantAura(Md, lay, Cp, Cp.Img.Meta);
			};
			Odr["wander_tilde"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				if (lay == 2)
				{
					return DrDef(ref Md, lay, Cp, Meta, Pre_Drawer);
				}
				return new M2CImgDrawerBench(Md, lay, Cp);
			};
			Odr["house_footbell"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerFootBell(Md, lay, Cp);
			};
			Odr["drawbridge"] = (Odr["drawbridge_piece"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new NelChipDrawBridge.M2CImgDrawerDrawBridge(Md, lay, Cp);
			});
			Odr["door_trm"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				string[] array = Meta.Get("door_trm");
				if (array.Length >= 1 && X.NmI(array[1], 0, false, false) == lay)
				{
					return new M2CImgDrawerDoorTrm(Md, lay, Cp);
				}
				return DrDef(ref Md, lay, Cp, Meta, Pre_Drawer);
			};
			Odr["door"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				if (!Cp.Mp.is_submap)
				{
					string[] array2 = Meta.Get("door");
					if (array2.Length >= 1 && X.NmI(array2[0], 0, false, false) == lay)
					{
						return new M2CImgDrawerDoor(Md, lay, Cp, "door", 0);
					}
				}
				return DrDef(ref Md, lay, Cp, Meta, Pre_Drawer);
			};
			Odr["hourglass"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				if (lay <= 2)
				{
					return new M2CImgDrawerTimer(Md, lay, Cp);
				}
				return DrDef(ref Md, lay, Cp, Meta, Pre_Drawer);
			};
			Odr["aloma_diffuser"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				if (lay == 0)
				{
					return null;
				}
				return new M2CImgDrawerAlomaDiffuser(Md, lay, Cp);
			};
			Odr["the_room_bed"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				if (lay == 0)
				{
					return null;
				}
				return new M2CImgDrawerTrmBed(Md, lay, Cp);
			};
			Odr["pentachoron"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				return new M2CImgDrawerPentachoron4D(Md, lay, Cp);
			};
			Odr["brainvat"] = delegate(ref MeshDrawer Md, int lay, M2Puts Cp, METACImg Meta, M2CImgDrawer Pre_Drawer)
			{
				if (lay != Cp.getMeta().GetI("brainvat", -1, 0))
				{
					return null;
				}
				return new M2CImgDrawerBrainVat(Md, lay, Cp);
			};
		};
	}
}
