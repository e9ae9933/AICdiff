using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public class NelChipPuzzleConnector : NelChipWithEffect
	{
		public NelChipPuzzleConnector(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			if (NelChipPuzzleConnector.Abuf == null)
			{
				NelChipPuzzleConnector.Abuf = new List<M2Puts>(5);
			}
			this.set_effect_on_initaction = false;
			this.effect_2_ed = true;
			base.arrangeable = true;
			this.invisible = this.Img.Meta.invisible;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			PuzzLiner.Assign(this);
			this.ARailStart = null;
		}

		public void activateConnector(int id, bool immediate)
		{
			if (this.ARailStart == null)
			{
				this.ARailStart = new NelChipPuzzleRailStart[8];
			}
			bool flag = false;
			int i = base.Meta.GetI("puzzle_connector", 0, 0);
			NelChipPuzzleConnector.Abuf.Clear();
			this.Mp.getPointMetaPutsTo(this.mapx, this.mapy, NelChipPuzzleConnector.Abuf, "puzzle_rail_start");
			for (int j = 0; j < 8; j++)
			{
				if ((i & (1 << j)) != 0)
				{
					for (int k = 0; k < NelChipPuzzleConnector.Abuf.Count; k++)
					{
						NelChipPuzzleRailStart nelChipPuzzleRailStart = NelChipPuzzleConnector.Abuf[k] as NelChipPuzzleRailStart;
						if (nelChipPuzzleRailStart != null && (int)nelChipPuzzleRailStart.aim == j && (int)nelChipPuzzleRailStart.puzzle_id == id)
						{
							nelChipPuzzleRailStart.getMeta();
							nelChipPuzzleRailStart.activateRailStart(immediate);
							this.ARailStart[j] = nelChipPuzzleRailStart;
							flag = true;
							break;
						}
					}
				}
			}
			if (flag)
			{
				if (this.Ed == null && !this.invisible)
				{
					this.setEffect();
				}
				if (this.Ed != null)
				{
					this.Ed.t = 0f;
				}
				this.Mp.PtcSTsetVar("x", (double)this.mapcx).PtcSTsetVar("y", (double)this.mapcy).PtcSTsetVar("col", (double)id)
					.PtcST("puzzle_connector_activate", null, PTCThread.StFollow.NO_FOLLOW);
				this.activated |= 1 << id;
			}
		}

		public void recheckLiner(bool immediate = false)
		{
			if (this.ARailStart == null || this.activated == 0)
			{
				return;
			}
			for (int i = 0; i < 8; i++)
			{
				NelChipPuzzleRailStart nelChipPuzzleRailStart = this.ARailStart[i];
				if (nelChipPuzzleRailStart != null)
				{
					nelChipPuzzleRailStart.activateRailStart(immediate);
				}
			}
		}

		public bool deactivateConnector(int id, bool immediate, bool no_recheck_liner = false)
		{
			if (this.ARailStart == null || (this.activated & (1 << id)) == 0)
			{
				return false;
			}
			this.activated &= ~(1 << id);
			bool flag = false;
			this.Mp.PtcSTsetVar("x", (double)this.mapcx).PtcSTsetVar("y", (double)this.mapcy).PtcSTsetVar("col", (double)id)
				.PtcST("puzzle_connector_deactivate", null, PTCThread.StFollow.NO_FOLLOW);
			for (int i = 0; i < 8; i++)
			{
				if (this.ARailStart[i] != null)
				{
					this.ARailStart[i] = null;
					flag = true;
				}
			}
			if (flag && !no_recheck_liner)
			{
				PuzzLiner.recheckLiner(id, immediate);
			}
			return flag;
		}

		protected override bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			Ef.x = this.mapcx;
			Ef.y = this.mapcy;
			if (this.activated == 0)
			{
				this.Ed = null;
				return false;
			}
			int num = X.bit_count((uint)this.activated);
			float num2 = Ed.t % 20f / 20f;
			float num3 = 1f - num2;
			float num4 = X.ZSIN2(Ed.t, 20f);
			int num5 = (int)(Ed.t / 20f) % num;
			int num6 = 0;
			for (int i = 0; i < 5; i++)
			{
				if ((this.activated & (1 << i)) != 0)
				{
					num6 = i;
					if (num5-- == 0)
					{
						break;
					}
				}
			}
			float num7 = num4 * 1.4f;
			float num8 = (1.8f - 0.79999995f * num4) * 1.4f;
			MeshDrawer mesh = Ef.GetMesh("", MTRX.MIicon.getMtr(BLEND.ADD, -1), false);
			mesh.Col = C32.MulA(MTR.Apuzzle_switch_col[num6], num3 * 0.7f);
			mesh.RotaPF(0f, 0f, num7, num8, 0f, MTR.APuzzleMarker[num6 + 5], false, false, false, uint.MaxValue, false, 0);
			mesh.Col = C32.MulA(MTR.Apuzzle_switch_col[num6], num3 * 0.3f);
			mesh.RotaPF(0f, 0f, num7, num8, 0f, MTR.APuzzleMarker[num6], false, false, false, uint.MaxValue, false, 0);
			MeshDrawer meshDrawer = null;
			for (int j = 0; j < 8; j++)
			{
				NelChipPuzzleRailStart nelChipPuzzleRailStart = this.ARailStart[j];
				if (nelChipPuzzleRailStart != null)
				{
					if (meshDrawer == null)
					{
						meshDrawer = Ef.GetMesh("", this.Mp.M2D.MIchip.getMtr(BLEND.NORMAL, -1), false);
					}
					meshDrawer.Col = MTRX.ColWhite;
					if (nelChipPuzzleRailStart.Img.initAtlasMd(meshDrawer, 0U))
					{
						meshDrawer.RotaGraph((nelChipPuzzleRailStart.get_real_draw_map_cx() - this.mapcx) * this.Mp.CLENB, -(nelChipPuzzleRailStart.get_real_draw_map_cy() - this.mapcy) * this.Mp.CLENB, this.Mp.base_scale, 0f, null, false);
					}
				}
			}
			MeshDrawer mesh2 = Ef.GetMesh("", uint.MaxValue, BLEND.ADD, false);
			mesh2.Col = C32.MulA(MTR.Apuzzle_switch_col[num6], X.NI(0.6f, 0f, X.ZLINE(num2 - 0.3f, 0.7f)));
			mesh2.Daia2(0f, 0f, X.NI(5, 12, X.ZLINE(num2, 0.5f)) * 2f, X.NI(6, 0, X.ZLINE(num2 - 0.3f, 0.5f)) * 2f, false);
			return true;
		}

		private readonly bool invisible;

		private int activated;

		public const int FADEIN_T = 20;

		private NelChipPuzzleRailStart[] ARailStart;

		private static List<M2Puts> Abuf;
	}
}
