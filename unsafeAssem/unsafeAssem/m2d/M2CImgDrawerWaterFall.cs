using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2CImgDrawerWaterFall : M2CImgDrawer
	{
		public M2CImgDrawerWaterFall(MeshDrawer Md, int _lay, M2Puts _Cp, META Meta)
			: base(Md, _lay, _Cp, false)
		{
			string[] array = Meta.Get("waterfall");
			int shiftx = this.Cp.Img.shiftx;
			if (array == null || array.Length >= 4)
			{
				Vector2 vector = new Vector2(this.Cp.mapcx, this.Cp.mapcy);
				Vector2 vector2 = new Vector2(1f, -1f) * (base.Mp.CLEN * 0.015625f);
				this.Pos_lt = this.Cp.PixelToMapPoint(X.Nm(array[0], 0f, false), 0f) - vector;
				this.Pos_bl = this.Cp.PixelToMapPoint(X.Nm(array[2], 0f, false), (float)this.Cp.iheight) - vector;
				this.Pos_tr = this.Cp.PixelToMapPoint(X.Nm(array[1], 0f, false), 0f) - vector;
				this.Pos_rb = this.Cp.PixelToMapPoint(X.Nm(array[3], 0f, false), (float)this.Cp.iheight) - vector;
				this.Pos_bl *= vector2;
				this.Pos_lt *= vector2;
				this.Pos_tr *= vector2;
				this.Pos_rb *= vector2;
				this.initted = true;
				this.V_Fall = (this.Pos_bl - this.Pos_lt) / (float)this.Cp.Img.iheight * X.Mn((float)this.Cp.Img.iheight * 0.13f, 3.3f);
				this.line_h_cnt = Meta.GetI("waterfall", -1, 4);
				if (this.line_h_cnt < 0)
				{
					this.line_h_cnt = X.Mx(1, X.IntC((float)this.Cp.iwidth / 2.5f));
				}
			}
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			if (base.Mp.apply_chip_effect)
			{
				return false;
			}
			base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			return false;
		}

		public override void initAction(bool normal_map)
		{
			if (this.Ed != null)
			{
				this.Ed = base.Mp.remED(this.Ed);
			}
			if (this.initted)
			{
				int targetLayer = base.Mp.getEffectForChip().getTargetLayer(false);
				this.is_submap_layer = !base.Mp.M2D.Cam.isFinalizeLayer(targetLayer);
				this.Ed = (this.is_submap_layer ? base.Mp.setEDC("WaterFall", new M2DrawBinder.FnEffectBind(this.edDraw), 0f) : ((base.Mp.SubMapData != null) ? base.Mp.SubMapData.getBaseMap() : base.Mp).setEDC("WaterFall", new M2DrawBinder.FnEffectBind(this.edDraw), 0f));
			}
			if (base.Mp.mode == MAPMODE.NORMAL)
			{
				base.Mp.M2D.Snd.Environment.AddLoop("areasnd_river", base.unique_key, this.Cp.mapcx, this.Cp.mapcy, 8f, 6f, 7f, 4f, null);
			}
		}

		public override void closeAction(bool when_map_close)
		{
			this.Ed = base.Mp.remED(this.Ed);
			base.Mp.M2D.Snd.Environment.RemLoop("areasnd_river", base.unique_key);
		}

		public bool edDraw(EffectItem Ef, M2DrawBinder Ed)
		{
			if (!Ed.isinCamera(this.Cp.mapcx, this.Cp.mapcy, 0.6f, 0.6f))
			{
				return true;
			}
			if (!this.is_submap_layer)
			{
				bool flag = base.Mp.SubMapData != null;
			}
			Ef.x = this.Cp.mapcx;
			Ef.y = this.Cp.mapcy;
			MeshDrawer meshDrawer = Ef.GetMesh("waterfall", this.is_submap_layer ? base.Mp.Dgn.DGN.MtrWaterFall : base.Mp.Dgn.DGN.MtrWaterFall, base.Mp.SubMapData == null);
			Vector4 vector = M2DBase.Instance.Cam.PosMainTransform * 64f;
			vector.x /= IN.w;
			vector.y /= IN.h;
			base.Mp.Dgn.DGN.MtrWaterFall.SetVector("_CamPos", vector);
			meshDrawer.Col = C32.d2c(3719674821U);
			meshDrawer.TriRectBL(0);
			if (this.Cp.flip)
			{
				meshDrawer.Pos(this.Pos_rb.x, this.Pos_rb.y, null).Pos(this.Pos_tr.x, this.Pos_tr.y, null).Pos(this.Pos_lt.x, this.Pos_lt.y, null)
					.Pos(this.Pos_bl.x, this.Pos_bl.y, null);
			}
			else
			{
				meshDrawer.Pos(this.Pos_bl.x, this.Pos_bl.y, null).Pos(this.Pos_lt.x, this.Pos_lt.y, null).Pos(this.Pos_tr.x, this.Pos_tr.y, null)
					.Pos(this.Pos_rb.x, this.Pos_rb.y, null);
			}
			if (this.line_h_cnt > 0 && this.is_submap_layer)
			{
				meshDrawer = Ef.GetMesh("", uint.MaxValue, BLEND.ADD, base.Mp.SubMapData == null);
				if (!this.is_submap_layer)
				{
					meshDrawer.Scale(2f, 2f, false);
				}
				uint num = X.GETRAN2(this.Cp.index * 7 + (int)(((base.Mp.SubMapData != null) ? base.Mp.SubMapData.getBaseMap() : base.Mp).floort / 4f), this.Cp.index & 3);
				int num2 = this.line_h_cnt + X.Mx((int)(num % 4U - 1U), 0);
				float num3 = 1f / (float)num2;
				float num4 = X.NI(0f, 0.12f, X.RAN(num, 502));
				Vector2 vector2 = X.NI(this.Pos_lt, this.Pos_bl, num4);
				Vector2 vector3 = X.NI(this.Pos_tr, this.Pos_rb, num4);
				for (int i = 0; i <= num2; i++)
				{
					num = X.GETRAN2(i * 11, i & 7) + num;
					float num5 = num3 * ((float)i + (-0.5f + X.RAN(num, 2013)) * 0.125f);
					float num6 = X.NI(-1, 1, X.RAN(num, 1938)) * 0.015625f * 0.5f;
					float num7 = X.NI(-2, 2, X.RAN(num, 2355)) * 0.015625f;
					float num8 = num7 * 0.5f + X.NI(-1, 1, X.RAN(num, 751)) * 0.015625f;
					Vector2 vector4 = X.NI(vector2, vector3, num5);
					Vector2 vector5 = vector4 + this.V_Fall;
					meshDrawer.Line(vector4.x + num6, vector4.y + num7, vector5.x + num6, vector5.y + num8, 0.0078125f, true, 0f, 0f);
				}
			}
			return true;
		}

		private Vector2 Pos_bl;

		private Vector2 Pos_lt;

		private Vector2 Pos_tr;

		private Vector2 Pos_rb;

		private Vector2 V_Fall;

		private int line_h_cnt;

		private bool initted;

		private bool is_submap_layer;

		private M2DrawBinder Ed;
	}
}
