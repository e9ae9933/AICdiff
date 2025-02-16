using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerManaWeed : M2CImgDrawerWeedShake
	{
		public M2CImgDrawerManaWeed(MeshDrawer Md, int _lay, M2Puts _Cp, META Meta)
			: base(Md, _lay, _Cp, Meta, "mana_weed")
		{
			this.slice_y = X.Mx(4, this.slice_y);
			this.CpW = this.Cp as NelChipManaWeed;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.Ed != null)
			{
				this.Ed = base.Mp.remED(this.Ed);
			}
			this.Ed = base.Mp.setED("Weed", new M2DrawBinder.FnEffectBind(this.fnDrawWeedLight), 0f);
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			bool flag = base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			this.force_redraw = true;
			return flag;
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			if (this.Ed != null)
			{
				this.Ed = base.Mp.remED(this.Ed);
			}
		}

		public override int redraw(float fcnt)
		{
			if (Map2d.editor_decline_lighting)
			{
				return base.redraw(fcnt);
			}
			if (this.is_active != this.CpW.isActive())
			{
				this.is_active = !this.is_active;
				this.force_redraw = true;
				if (this.is_active)
				{
					this.grow_anim_t = 24f;
				}
			}
			float num = 0f;
			if (this.grow_anim_t > 0f)
			{
				this.grow_anim_t -= fcnt;
				if (this.grow_anim_t <= 0f)
				{
					this.grow_anim_t = 0f;
				}
				else
				{
					num = 1f - X.ZLINE(this.grow_anim_t, 24f);
					num = 1f - (X.ZSIN(num, 0.4f) * 1.2f - X.ZCOS(num - 0.4f, 0.6f) * 0.2f);
				}
				this.force_redraw = true;
			}
			if (this.force_redraw)
			{
				int num2 = this.APos.Length;
				int num3 = this.slice_x + 1;
				int num4 = this.slice_y + 1;
				for (int i = 0; i < num3; i++)
				{
					float y = this.APos[num3 + i].y;
					for (int j = 2; j < num4; j++)
					{
						int num5 = j * num3 + i;
						Vector3 vector = this.APos[num5];
						Vector3 vector2 = this.ASh[num5];
						float num6 = y - vector.y;
						vector2.y = (this.is_active ? num : 1f) * num6;
						this.ASh[num5] = vector2;
					}
				}
			}
			return base.redraw(fcnt);
		}

		private bool fnDrawWeedLight(EffectItem Ef, M2DrawBinder Ed)
		{
			Ef.x = this.Cp.mapcx;
			Ef.y = this.Cp.mapcy;
			if (!this.is_active || !Ed.isinCamera(Ef, 3f, 3f))
			{
				return true;
			}
			MeshDrawer meshDrawer = Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, true);
			MeshDrawer meshImg = Ef.GetMeshImg("_", MTRX.MIicon, BLEND.ADD, true);
			meshImg.base_z = 314.9969f;
			Bench.P("FLYMOTH");
			EffectItemNel.drawFlyingMoth(meshImg, meshDrawer, 0f, 28f, base.Mp.floort, 4, 220f, 28f, 8f, 40f, 22f, 4292931769U, 14741689U, this.Cp.index * 17, 2f);
			Bench.Pend("FLYMOTH");
			Bench.P("RenderWeed");
			meshDrawer = Ef.GetMeshImg("", Ed.Mp.M2D.MIchip, BLEND.ADD, true);
			meshDrawer.base_x = (meshDrawer.base_y = 0f);
			meshDrawer.Col = meshDrawer.ColGrd.Set((X.ANMT(2, 16f) == 1) ? 4281545523U : 4282664004U).C;
			meshDrawer.Scale(base.Mp.base_scale, base.Mp.base_scale, false);
			meshDrawer.RotaTempMeshDrawer(0f, 0f, 1f, 1f, 0f, this.Md, false, false, this.tri_first, this.tri_last, this.index_first, this.index_last);
			meshDrawer.Identity();
			Bench.Pend("RenderWeed");
			return true;
		}

		protected override float getSliceYCount(float y, float maxy)
		{
			if (y == 1f)
			{
				return X.NIXP(0.2f, 1.6f);
			}
			return y;
		}

		private float cld_maph = 1.6f;

		private bool is_active = true;

		private float grow_anim_t;

		private const float GROW_ANIM_MAXT = 24f;

		private M2DrawBinder Ed;

		private NelChipManaWeed CpW;
	}
}
