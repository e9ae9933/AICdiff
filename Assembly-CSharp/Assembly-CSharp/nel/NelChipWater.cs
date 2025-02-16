using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public class NelChipWater : NelChip, IActivatableByMv
	{
		public NelChipWater(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			base.arrangeable = true;
		}

		protected override M2CImgDrawer CreateDrawer(ref MeshDrawer Md, int lay, METACImg Meta, M2CImgDrawer Pre_Drawer)
		{
			M2CImgDrawer m2CImgDrawer = base.CreateDrawer(ref Md, lay, Meta, Pre_Drawer);
			if (m2CImgDrawer is M2CImgDrawerWater)
			{
				(m2CImgDrawer as M2CImgDrawerWater).no_set_effect = false;
			}
			return m2CImgDrawer;
		}

		public static MistManager.MistKind getWaterKind(Dungeon Dgn, MistManager.MISTTYPE type, int behind_influence, bool alloc_reduce = true, bool alloc_bottom_raise = true, int surface_top = -1, float water_raise_alloc_level = 1f)
		{
			if (NelChipWater.AMistk == null)
			{
				NelChipWater.AMistk = new List<MistManager.MistKind>(1);
			}
			MistManager.MistKind mistKind = null;
			for (int i = NelChipWater.AMistk.Count - 1; i >= 0; i--)
			{
				MistManager.MistKind mistKind2 = NelChipWater.AMistk[i];
				if (mistKind2.type == type && mistKind2.max_behind_influence == behind_influence && mistKind2.reduce255 > 0 == alloc_reduce && mistKind2.water_bottom_raise == alloc_bottom_raise && mistKind2.water_surface_top == surface_top && water_raise_alloc_level == mistKind2.raise_alloc_lv)
				{
					mistKind = mistKind2;
					break;
				}
			}
			if (mistKind == null)
			{
				NelChipWater.AMistk.Add(mistKind = new MistManager.MistKind(type));
				mistKind.max_behind_influence = behind_influence;
				mistKind.water_bottom_raise = alloc_bottom_raise;
				mistKind.water_surface_top = surface_top;
				if (!alloc_reduce)
				{
					mistKind.reduce255 = 0;
				}
				if (water_raise_alloc_level >= 0f)
				{
					mistKind.raise_alloc_lv = water_raise_alloc_level;
				}
			}
			if (mistKind.type == MistManager.MISTTYPE.WATER)
			{
				mistKind.color0 = Dgn.ColWater;
				mistKind.color1 = Dgn.ColWaterBottom;
			}
			return mistKind;
		}

		public override int entryChipMesh(MeshDrawer MdB, MeshDrawer MdG, MeshDrawer MdT, MeshDrawer MdL, MeshDrawer MdTT, float sx, float sy, float _zm, float _rotR = 0f)
		{
			if (this.no_draw && !Map2d.editor_decline_lighting)
			{
				return 0;
			}
			return base.entryChipMesh(MdB, MdG, MdT, MdL, MdTT, sx, sy, _zm, _rotR);
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (!normal_map || base.hasActiveRemoveKey("GENERATE_MIST") || base.hasActiveRemoveKey("GENERATE_MIST_BASE"))
			{
				return;
			}
			if (this.K == null)
			{
				int num = base.Meta.GetI("water_level", -1, 0);
				bool flag = false;
				this.water_amount = base.Meta.GetI("water_amount", 255, 0);
				bool flag2 = false;
				float num2 = 1f;
				int num3 = -1;
				MistManager.MISTTYPE misttype = MistManager.MISTTYPE.WATER;
				M2LpWater m2LpWater = null;
				for (int i = this.Lay.LP.Length - 1; i >= 0; i--)
				{
					M2LabelPoint m2LabelPoint = this.Lay.LP.Get(i);
					if (m2LabelPoint is M2LpWater && m2LabelPoint.isin((float)this.drawx, (float)this.drawy, 0f))
					{
						m2LpWater = m2LabelPoint as M2LpWater;
						break;
					}
				}
				if (m2LpWater != null)
				{
					int water_level = m2LpWater.water_level;
					if (water_level >= -1)
					{
						num = water_level;
					}
					this.water_amount = m2LpWater.water_amount;
					flag2 = m2LpWater.water_bottom_raise;
					num3 = m2LpWater.water_surface;
					this.static_clip_top = (m2LpWater.water_static ? m2LpWater.mapy : (-1));
					num2 = m2LpWater.water_raise_alloc_level;
					misttype = m2LpWater.water_type;
					flag = true;
				}
				if (num < 0)
				{
					return;
				}
				this.K = NelChipWater.getWaterKind(this.Mp.Dgn, misttype, num, flag, flag2, num3, num2);
			}
			NelM2DBase nelM2DBase = base.M2D as NelM2DBase;
			nelM2DBase.MIST.addMistGeneratorChip(this.K, this.water_amount, this.mapx, this.mapy, this, this.static_clip_top);
			if (this.K.type == MistManager.MISTTYPE.LAVA)
			{
				nelM2DBase.prepareSvTexture("damage_behind", false);
			}
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			if (base.hasActiveRemoveKey("GENERATE_MIST_BASE"))
			{
				base.remActiveRemoveKey("GENERATE_MIST_BASE", false);
				this.no_draw = false;
			}
			this.K = null;
		}

		public void activate(M2Mover Mv)
		{
			if (base.NM2D.MIST != null)
			{
				base.NM2D.MIST.waveActEffect(X.MMX((float)this.mapx, Mv.x, (float)(this.mapx + this.clms)), X.MMX((float)this.mapy + 0.125f, Mv.mbottom - 0.125f, (float)(this.mapy + this.rows)));
			}
		}

		public bool no_draw;

		private static List<MistManager.MistKind> AMistk;

		public MistManager.MistKind K;

		public int water_amount = 255;

		public int static_clip_top = -1;
	}
}
