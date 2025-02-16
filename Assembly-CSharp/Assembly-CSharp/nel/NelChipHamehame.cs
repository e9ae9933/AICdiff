using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class NelChipHamehame : NelChip
	{
		public NelChipHamehame(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (NelChipHamehame.PAnim == null)
			{
				NelChipHamehame.PAnim = this.Img.SourceLayer.pChar.getPoseByName("_anim_hamehame").getSequence(0);
			}
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			this.removeEffect();
			this.mng_id = -1;
			NelChipHamehame.PAnim = null;
		}

		public void runHameDecided()
		{
			if (this.Mp.floort >= this.eff_set_decided)
			{
				this.eff_set_decided = this.Mp.floort + X.NIXP(50f, 70f);
				this.setPtc("puzz_hamehame_aula_hold");
			}
		}

		public NelChipHamehame checkHame(M2BoxMover Box, NelChipPuzzleBox.ConnectMem[] ACnct, bool decided = false)
		{
			if (this.TargetBox != null && Box != this.TargetBox)
			{
				return null;
			}
			this.TargetBox = null;
			if (this.mng_id < 0 || this.ManageTo == null)
			{
				return null;
			}
			NelChipHamehame[] connection = this.ManageTo.getConnection(this.mng_id);
			if (connection == null || connection.Length != ACnct.Length)
			{
				return null;
			}
			uint num = 0U;
			for (int i = ACnct.Length - 1; i >= 0; i--)
			{
				NelChipPuzzleBox cp = ACnct[i].Cp;
				int num2 = (int)cp.mapcx;
				int num3 = (int)cp.mapcy;
				bool flag = false;
				for (int j = connection.Length - 1; j >= 0; j--)
				{
					NelChipHamehame nelChipHamehame = connection[j];
					if ((num & (1U << j)) == 0U && nelChipHamehame.mapx == num2 && nelChipHamehame.mapy == num3)
					{
						flag = true;
						num |= 1U << j;
						break;
					}
				}
				if (!flag)
				{
					return null;
				}
			}
			this.TargetBox = Box;
			for (int k = connection.Length - 1; k >= 0; k--)
			{
				NelChipHamehame.M2CImgHamehame m2CImgHamehame = connection[k].CastDrawer<NelChipHamehame.M2CImgHamehame>();
				if (m2CImgHamehame != null)
				{
					m2CImgHamehame.anmtype = (decided ? NelChipHamehame.ANMTYPE.DECIDED : NelChipHamehame.ANMTYPE.HOLD);
				}
			}
			if (decided)
			{
				bool flag2 = false;
				if (this.ManageTo.DecideBox(this.mng_id))
				{
					flag2 = true;
					this.setPtc("puzz_hamehame_finished");
					this.Mp.PtcSTsetVar("fromme", 0.0);
					this.ManageTo.setPtcAll("puzz_hamehame_finished", this.mng_id);
				}
				this.eff_set_decided = this.Mp.floort + X.NIXP(100f, 150f);
				this.Mp.PtcSTsetVar("fullfilled", (double)(flag2 ? 1 : 0));
				this.setPtc("puzz_hamehame_aula");
			}
			else
			{
				this.ManageTo.RemoveBox(this.mng_id);
			}
			if (this.Ed == null)
			{
				if (this.FD_fnEfDraw == null)
				{
					this.FD_fnEfDraw = new M2DrawBinder.FnEffectBind(this.fnEfDraw);
				}
				this.Ed = this.Mp.setED("hamehame", this.FD_fnEfDraw, 0f);
			}
			this.Ed.t = 0f;
			return this;
		}

		public void removeEffect()
		{
			this.TargetBox = null;
			this.Ed = this.Mp.remED(this.Ed);
			if (this.mng_id < 0 || this.ManageTo == null)
			{
				return;
			}
			NelChipHamehame[] connection = this.ManageTo.getConnection(this.mng_id);
			if (connection == null)
			{
				return;
			}
			this.ManageTo.RemoveBox(this.mng_id);
			for (int i = connection.Length - 1; i >= 0; i--)
			{
				NelChipHamehame.M2CImgHamehame m2CImgHamehame = connection[i].CastDrawer<NelChipHamehame.M2CImgHamehame>();
				if (m2CImgHamehame != null)
				{
					m2CImgHamehame.anmtype = NelChipHamehame.ANMTYPE.OFFLINE;
				}
			}
		}

		public void setPtc(string ptcst_key)
		{
			if (this.mng_id < 0 || this.ManageTo == null)
			{
				return;
			}
			NelChipHamehame[] connection = this.ManageTo.getConnection(this.mng_id);
			if (connection == null)
			{
				return;
			}
			for (int i = connection.Length - 1; i >= 0; i--)
			{
				NelChipHamehame nelChipHamehame = connection[i];
				this.Mp.PtcSTsetVar("cx", (double)nelChipHamehame.mapcx).PtcSTsetVar("cy", (double)nelChipHamehame.mapcy).PtcSTsetVar("fromme", (double)((nelChipHamehame == this) ? 1 : 0))
					.PtcST(ptcst_key, null, PTCThread.StFollow.NO_FOLLOW);
			}
		}

		public bool fnEfDraw(EffectItem Ef, M2DrawBinder Ed)
		{
			NelChipHamehame[] connection;
			if (this.TargetBox == null || this.mng_id < 0 || this.ManageTo == null || (connection = this.ManageTo.getConnection(this.mng_id)) == null)
			{
				if (Ed == this.Ed)
				{
					this.Ed = null;
				}
				return false;
			}
			DRect connectionRect = this.ManageTo.getConnectionRect(this.mng_id);
			if (!Ed.isinCamera(connectionRect.cx, connectionRect.cy, connectionRect.width, connectionRect.height))
			{
				return true;
			}
			MeshDrawer meshImg = Ef.GetMeshImg("", base.IMGS.MIchip, BLEND.NORMAL, false);
			meshImg.base_x = (meshImg.base_y = 0f);
			meshImg.ColGrd.Set(4287102939U);
			int num;
			if (!this.TargetBox.hameDecided())
			{
				num = 1 + X.ANM((int)this.Mp.floort, NelChipHamehame.PAnim.countFrames() - 1, 5f);
				meshImg.Col = meshImg.ColGrd.setA1(0.5f + 0.3f * X.COSI(this.Mp.floort, 140f)).C;
			}
			else
			{
				num = 0;
				meshImg.Col = meshImg.ColGrd.setA1(1f - X.ANMP((int)this.Mp.floort, 20, 1f)).C;
			}
			base.IMGS.Atlas.getAtlasData(NelChipHamehame.PAnim.getImage(num, 0)).initAtlasMd(meshImg, base.IMGS.MIchip);
			for (int i = connection.Length - 1; i >= 0; i--)
			{
				NelChipHamehame nelChipHamehame = connection[i];
				meshImg.RotaGraph(nelChipHamehame.draw_effectx, nelChipHamehame.draw_effecty, this.Mp.base_scale, 0f, null, false);
			}
			if (this.TargetBox.hameDecided())
			{
				bool fullfilled = this.ManageTo.fullfilled;
				if (Ed.t < 60f || fullfilled)
				{
					MeshDrawer mesh = Ef.GetMesh("", MTRX.MtrMeshStriped, false);
					mesh.Col = meshImg.ColGrd.setA1(0.65f + 0.13f * X.COSI(Ed.t, 17.55f) + 0.1f * X.COSI(Ed.t, 11.37f)).C;
					mesh.base_x = (mesh.base_y = 0f);
					mesh.uvRectN(X.Cos(0.7853982f), X.Sin(0.7853982f));
					mesh.Uv2(0f, X.Mx(1f - X.ZLINE(Ed.t, 60f), fullfilled ? 0.5f : 0f), true);
					for (int j = connection.Length - 1; j >= 0; j--)
					{
						NelChipHamehame nelChipHamehame2 = connection[j];
						mesh.Rect(nelChipHamehame2.draw_effectx, nelChipHamehame2.draw_effecty, base.CLENB, base.CLENB, false);
					}
					mesh.allocUv2(0, true);
				}
			}
			return true;
		}

		protected override M2CImgDrawer CreateDrawer(ref MeshDrawer Md, int lay, METACImg Meta, M2CImgDrawer Pre_Drawer)
		{
			if (lay < 3)
			{
				return new NelChipHamehame.M2CImgHamehame(Md, lay, this);
			}
			return null;
		}

		public M2LpHamehame ManageTo;

		public int mng_id = -1;

		private M2DrawBinder Ed;

		private M2BoxMover TargetBox;

		private static PxlSequence PAnim;

		public const int FADEIN_T = 20;

		private float eff_set_decided;

		private M2DrawBinder.FnEffectBind FD_fnEfDraw;

		private enum ANMTYPE
		{
			OFFLINE,
			HOLD,
			DECIDED
		}

		private class M2CImgHamehame : M2CImgDrawer
		{
			public M2CImgHamehame(MeshDrawer Md, int lay, NelChipHamehame Cp)
				: base(Md, lay, Cp, true)
			{
				this.Con = Cp;
				this.use_color = true;
			}

			public NelChipHamehame.ANMTYPE anmtype
			{
				get
				{
					return this.anmtype_;
				}
				set
				{
					if (this.anmtype_ == value)
					{
						return;
					}
					if (value == NelChipHamehame.ANMTYPE.OFFLINE != (this.anmtype_ == NelChipHamehame.ANMTYPE.OFFLINE))
					{
						if (value != NelChipHamehame.ANMTYPE.OFFLINE)
						{
							this.need_reposit_flag = true;
						}
						else
						{
							this.anmt = -1;
						}
					}
					this.anmtype_ = value;
				}
			}

			public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
			{
				base.Set(false);
				Md.RotaMesh(meshx, meshy, _zmx, _zmy, _rotR, Ms, false, false);
				if (this.layer <= 2 && NelChipHamehame.PAnim != null)
				{
					this.Cp.IMGS.Atlas.getAtlasData(NelChipHamehame.PAnim.getImage(0, 0)).initAtlasMd(Md, this.Cp.IMGS.MIchip);
					Md.RotaGraph3(meshx, meshy, 0.5f, 0.5f, _zmx, _zmy, _rotR, null, false);
				}
				base.Set(false);
				if ((this.redraw_flag || this.use_shift || this.use_color) && this.index_last > this.index_first)
				{
					base.initMdArray();
				}
				this.rewriteable_alpha = true;
				if (this.Cp.active_removed)
				{
					base.repositActiveRemoveFlag();
				}
				return this.redraw_flag;
			}

			public override int redraw(float fcnt)
			{
				if (this.anmtype_ == NelChipHamehame.ANMTYPE.OFFLINE)
				{
					int num = X.ANM((int)base.Mp.floort, 6, 12f);
					if (num != this.anmt)
					{
						this.anmt = num;
						this.need_reposit_flag = true;
					}
				}
				if (this.need_reposit_flag)
				{
					float num2 = ((this.anmtype == NelChipHamehame.ANMTYPE.OFFLINE) ? ((X.Abs((float)this.anmt - 2.5f) + 0.5f) * 0.33f * 0.8f) : 0f);
					byte b = (byte)(255f * num2);
					for (int i = this.ACol.Length - 1; i >= 4; i--)
					{
						Color32 color = this.ACol[i];
						color.a = b;
						this.ACol[i] = color;
					}
				}
				return base.redraw(fcnt);
			}

			private NelChipHamehame.ANMTYPE anmtype_;

			private NelChipHamehame Con;

			private int anmt = -1;
		}
	}
}
