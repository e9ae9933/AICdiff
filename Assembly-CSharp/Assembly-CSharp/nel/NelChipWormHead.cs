using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelChipWormHead : NelChip
	{
		public NelChipWormHead(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			this.Ed = this.Mp.remED(this.Ed);
			if (this.GobCld != null)
			{
				this.Worm.destruct();
				IN.DestroyOne(this.GobCld);
				this.GobCld = null;
				this.Worm = null;
			}
			this.mclms = 0;
		}

		public override void initAction(bool normal_map)
		{
			this.closeAction(true, true);
			this.mclms = 0;
			base.initAction(normal_map);
			this.ahead = (AIM)base.Meta.getDirs("worm_head", this.rotation, this.flip, 0)[0];
			AIM aim = ((CAim._XD(this.ahead, 1) == 0) ? AIM.L : AIM.T);
			this.aright = CAim.get_opposite(aim);
			M2Puts m2Puts = this.Lay.findChip(this.mapx + CAim._XD(aim, 1), this.mapy - CAim._YD(aim, 1), "worm_head");
			if (m2Puts != null && m2Puts.getMeta().getDirs("worm_head", m2Puts.rotation, m2Puts.flip, 0)[0] == (int)this.ahead)
			{
				return;
			}
			this.mclms = 1;
			for (;;)
			{
				M2Chip m2Chip = this.Lay.findChip(this.mapx + this.mclms * CAim._XD(this.aright, 1), this.mapy - this.mclms * CAim._YD(this.aright, 1), "worm_head");
				if (m2Chip == null || m2Chip.getMeta().getDirs("worm_head", m2Chip.rotation, m2Chip.flip, 0)[0] != (int)this.ahead)
				{
					break;
				}
				this.mclms++;
			}
			this.mrows = 1;
			while (this.mrows <= 2 && this.Lay.findChip(this.mapx - this.mrows * CAim._XD(this.ahead, 1), this.mapy + this.mrows * CAim._YD(this.ahead, 1), "worm_area") != null)
			{
				this.mrows++;
			}
			AIM aim2 = this.ahead;
			if (CAim._XD(this.ahead, 1) != 0)
			{
				int num = this.mclms;
				int num2 = this.mrows;
				this.mrows = num;
				this.mclms = num2;
			}
			this.Ed = this.Mp.setEDC("uneune", new M2DrawBinder.FnEffectBind(this.fnDrawUneuneWorms), 0f);
			this.GobCld = this.Mp.createMoverGob<M2WormTrap>("-worm_head-" + this.index.ToString(), this.mcld_cx, this.mcld_cy, false);
			this.Worm = this.GobCld.AddComponent<M2WormTrap>();
			this.Worm.appear(this, this.Mp);
		}

		public float mcld_left
		{
			get
			{
				return this.mleft - (float)((CAim._XD(this.ahead, 1) > 0) ? (this.mclms - 1) : 0);
			}
		}

		public float mcld_top
		{
			get
			{
				return this.mtop - (float)((CAim._YD(this.ahead, 1) < 0) ? (this.mrows - 1) : 0);
			}
		}

		public float mcld_right
		{
			get
			{
				return this.mcld_left + (float)this.mclms;
			}
		}

		public float mcld_bottom
		{
			get
			{
				return this.mcld_top + (float)this.mrows;
			}
		}

		public float mcld_cx
		{
			get
			{
				return this.mcld_left + (float)this.mclms * 0.5f;
			}
		}

		public float mcld_cy
		{
			get
			{
				return this.mcld_top + (float)this.mrows * 0.5f;
			}
		}

		private bool fnDrawUneuneWorms(EffectItem Ef, M2DrawBinder Ed)
		{
			int num = ((CAim._XD(this.ahead, 1) == 0) ? this.mclms : this.mrows);
			int num2 = ((CAim._XD(this.ahead, 1) == 0) ? this.mrows : this.mclms);
			int num3 = X.MMX(1, num2, 2);
			Ef.x = this.mcld_cx;
			Ef.y = this.mcld_cy;
			if (MTR.SqWormAnim1 == null)
			{
				return true;
			}
			if (!Ed.isinCamera(Ef, (float)(this.mclms + 2), (float)(this.mrows + 2)))
			{
				return true;
			}
			int num4 = num3;
			int particleCount = Ef.getParticleCount(Ef, (int)(1.2f * (float)num * (float)num4));
			MeshDrawer mesh = Ef.GetMesh("worm", this.Mp.Dgn.getChipMaterialActive(this.Mp, null, 1), false);
			MeshDrawer mesh2 = Ef.GetMesh("wormb", this.Mp.Dgn.getChipMaterialActive(this.Mp, null, 1), true);
			for (int i = 0; i < particleCount; i++)
			{
				int num5 = (int)Ed.t + 72 * particleCount - i % 5 * 15;
				int num6 = num5 / 72;
				uint ran = X.GETRAN2(this.index * 13 + i * 11 + num6 * 9, 4 + i % 7);
				int num7 = num5 - num6 * 72;
				float num8 = (float)(i % (2 * num));
				float num9 = 0.5f;
				float num10 = num9 / (float)num4;
				float num11 = 1f - X.ZPOW(1f - num10);
				bool flag = i % 6 == 0;
				float num12 = (float)(-(float)num) * 0.5f + (float)num * (num8 + 0.5f) / (float)(2 * num) + X.NI(-0.25f, 0.25f, X.RAN(ran, 1080));
				float num13 = (float)(-(float)num3) + num9 * (float)num3;
				PxlSequence pxlSequence;
				int num14;
				float num15;
				if (flag || X.RAN(ran, 1568) < 0.25f)
				{
					pxlSequence = MTR.SqWormAnim1;
					num14 = (int)(X.RAN(ran, 531) * 3f);
					num15 = -1f + X.NI(0, -8, X.RAN(ran, 677));
				}
				else
				{
					pxlSequence = MTR.SqWormAnim0;
					num14 = (int)(X.RAN(ran, 2422) * 4f);
					num15 = -3f + X.NI(0, X.Mx(-20 * num2, -55), X.ZPOW(X.RAN(ran, 2671)));
				}
				MeshDrawer meshDrawer = (flag ? mesh2 : mesh);
				meshDrawer.Col = this.Mp.Dgn.getSpecificColor(meshDrawer.ColGrd.Set(4281408788U).blend(uint.MaxValue, 0.5f + num11 * 0.35f).C);
				switch (this.ahead)
				{
				case AIM.L:
					num15 = ((float)(-(float)this.mclms) * 0.5f - num13) * base.CLEN - num15;
					num13 = num12 * base.CLEN;
					num12 = num15;
					break;
				case AIM.T:
					num12 *= base.CLEN;
					num13 = ((float)this.mrows * 0.5f + num13) * base.CLEN + num15;
					break;
				case AIM.R:
					num15 = ((float)this.mclms * 0.5f + num13) * base.CLEN + num15;
					num13 = num12 * base.CLEN;
					num12 = num15;
					break;
				case AIM.B:
					num12 *= base.CLEN;
					num13 = ((float)(-(float)this.mrows) * 0.5f - num13) * base.CLEN + num15;
					break;
				}
				float num16 = CAim.get_agR(this.ahead, -1.5707964f);
				PxlLayer layer = pxlSequence.getFrame(num14 * 9 + num7 / 8).getLayer(0);
				if (base.M2D.IMGS.initAtlasMd(meshDrawer, layer.Img))
				{
					meshDrawer.RotaGraph(num12 + layer.x, num13 - layer.y, 1f, num16, null, X.RAN(ran, 1667) >= 0.5f);
				}
			}
			return true;
		}

		public const uint dark_color = 4281408788U;

		public AIM ahead;

		private AIM aright;

		private M2DrawBinder Ed;

		public int mclms;

		public int mrows = 1;

		private GameObject GobCld;

		private M2WormTrap Worm;
	}
}
