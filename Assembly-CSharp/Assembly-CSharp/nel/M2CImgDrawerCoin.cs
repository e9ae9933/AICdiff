using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerCoin : M2CImgDrawer, NearManager.INearLsnObject
	{
		public M2CImgDrawerCoin(MeshDrawer Md, int _lay, M2Puts _Cp)
			: base(Md, _lay, _Cp, false)
		{
		}

		public byte coin_id
		{
			get
			{
				byte b = (byte)base.Meta.GetI("coin_id", 255, 0);
				if (b == 255)
				{
					X.de("coin には coin_id を他の M2ChipImage とかぶらない数値に設定しておく必要があります ", null);
				}
				return b;
			}
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			return !base.Mp.apply_chip_effect && base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			this.pre_drawx = this.Cp.mapcx;
			this.pre_drawy = this.Cp.mapcy;
			if (base.Mp.NM == null)
			{
				return;
			}
			if (CoinStorage.isAlreadyTaken(this))
			{
				return;
			}
			this.Sq = MTRX.PxlIcon.getPoseByName("_anim_" + this.Cp.Img.basename).getSequence(0);
			CoinStorage.AssignCoin(this, this.Sq);
			base.Mp.NM.AssignForPr(this, false, false);
		}

		public override void closeAction(bool normal_map)
		{
			base.initAction(normal_map);
			CoinStorage.DeassignCoin(this, this.Sq);
			this.Sq = null;
			this.pre_drawx = this.Cp.mapcx;
			this.pre_drawy = this.Cp.mapcy;
		}

		public bool drawCoin(EffectItem Ef, M2DrawBinder Ed, ref MeshDrawer Md, ref PxlImage PImg)
		{
			float num = this.pre_drawx;
			float num2 = this.pre_drawy;
			if (this.Sq == null)
			{
				return false;
			}
			if (!Ed.isinCamera(num, num2, 1f, 1f))
			{
				return true;
			}
			if (Md == null)
			{
				Md = Ef.GetMeshImg("", MTRX.MIicon, BLEND.NORMAL, true);
			}
			if (PImg == null)
			{
				PImg = this.Sq.getFrame(X.ANM((int)base.Mp.floort, this.Sq.countFrames(), 3f)).getLayer(0).Img;
				Md.initForImg(PImg, 0);
			}
			bool flag;
			Vector2 vector = Ef.EF.calcMeshXY(num, num2, Md, out flag);
			Md.base_x = vector.x;
			Md.base_y = vector.y;
			Md.RotaGraph(0f, 0f, base.Mp.base_scale, this.Cp.draw_rotR, null, this.Cp.flip);
			return true;
		}

		public DRect getBounds(M2Mover Mv, DRect Dest)
		{
			return Dest.Set(this.pre_drawx - 1.7f, this.pre_drawy - 1.7f, 3.4f, 3.4f);
		}

		public bool nearCheck(M2Mover Mv, NearTicket NTk)
		{
			if (this.Sq == null)
			{
				return false;
			}
			float num = this.pre_drawx;
			float num2 = this.pre_drawy;
			if (!NTk.moving_always)
			{
				if (Mv.isCovering(num, num, num2, num2, 1.7f))
				{
					NTk.moving_always = true;
					this.time = base.Mp.floort;
				}
			}
			else
			{
				if (Mv.isCovering(num, num, num2, num2, 0.4f))
				{
					CoinStorage.takeCoin(this, this.Sq, true);
					base.Mp.PtcSTsetVar("x", (double)num).PtcSTsetVar("y", (double)num2).PtcST("get_coin", null, PTCThread.StFollow.NO_FOLLOW);
					base.Mp.NM.Deassign(this);
					this.Sq = null;
					return false;
				}
				float num3 = base.Mp.GAR(num, num2, Mv.x, Mv.y);
				float num4 = X.ZLINE(base.Mp.floort - this.time, 30f) * 0.5f + 0.5f;
				float num5 = 0.22f * X.Cos(num3) * num4;
				float num6 = -0.22f * X.Sin(num3) * num4;
				this.pre_drawx += num5;
				this.pre_drawy += num6;
			}
			return true;
		}

		private PxlSequence Sq;

		private const float absorb_len = 1.7f;

		private const float absorb_speed = 0.22f;

		private const float catch_len = 0.4f;

		private float pre_drawx;

		private float pre_drawy;

		private float time;
	}
}
