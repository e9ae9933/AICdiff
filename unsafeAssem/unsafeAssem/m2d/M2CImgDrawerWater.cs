using System;
using PixelLiner;
using XX;

namespace m2d
{
	public class M2CImgDrawerWater : M2CImgDrawer
	{
		public M2CImgDrawerWater(MeshDrawer Md, int _lay, M2Puts _Cp, META Meta)
			: base(Md, _lay, _Cp, false)
		{
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			if (this.layer == 0 && base.Mp.apply_chip_effect)
			{
				Md = (this.Md = base.Mp.getWaterDrawer());
				int vertexCount = Ms.vertexCount;
				float num = 2.5f;
				float num2 = 2.5f;
				float num3 = 0.5f;
				for (int i = 0; i < vertexCount; i++)
				{
					Md.Uv2(num, num2, i == 0);
					Md.Uv3(num3, num3, i == 0);
				}
				base.Mp.addUpdateMesh(2048, false);
			}
			base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			return false;
		}

		public override void initAction(bool normal_map)
		{
			if (this.no_set_effect)
			{
				return;
			}
			bool flag = base.Meta.GetI("water", 0, 0) > 0;
			if (!flag && base.Mp.getPointMetaPutsTo(this.Cp.mapx, this.Cp.mapy - 1, null, "water") == 0)
			{
				flag = true;
			}
			if (flag)
			{
				this.Ed = base.Mp.setED("WaterUpper", new M2DrawBinder.FnEffectBind(this.edDraw), 0f);
			}
		}

		public override void closeAction(bool when_map_close)
		{
			this.Ed = base.Mp.remED(this.Ed);
		}

		public bool edDraw(EffectItem Ef, M2DrawBinder Ed)
		{
			if (!Ed.isinCamera(this.Cp.mapcx, this.Cp.mapcy, 0.6f, 0.6f))
			{
				return true;
			}
			Ef.x = (float)this.Cp.drawx / base.CLEN;
			Ef.y = (float)this.Cp.drawy / base.CLEN;
			MeshDrawer mesh = Ef.GetMesh("", uint.MaxValue, BLEND.ADD, false);
			mesh.ColGrd.Set(4287732630U).setA(0f);
			mesh.Scale(base.Mp.base_scale, base.Mp.base_scale, false);
			mesh.RectBLGradation(0f, -4f, (float)this.Cp.iwidth, 4f, GRD.TOP2BOTTOM, false);
			mesh.Identity();
			return true;
		}

		private const float BOD_HEIGHT = 4f;

		public bool no_set_effect;

		private M2DrawBinder Ed;
	}
}
