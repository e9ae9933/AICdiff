using System;
using m2d;
using PixelLiner;
using XX;

namespace nel
{
	public class M2CImgDrawerDoorTrm : M2CImgDrawerDoor
	{
		public M2CImgDrawerDoorTrm(MeshDrawer Md, int lay, M2Puts _Cp)
			: base(Md, lay, _Cp, "door_trm", 1)
		{
			this.Dr.FnDrawFront = new Action<MeshDrawer, DoorDrawer, float>(this.fnDoorDrawFront);
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			return base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
		}

		public void fnDoorDrawFront(MeshDrawer Md, DoorDrawer Dr, float center_x)
		{
			if (Map2d.editor_decline_lighting)
			{
				return;
			}
			byte b = TRMManager.hasNewerItem(false);
			if (b > 0)
			{
				M2ChipImage m2ChipImage = this.Cp.IMGS.Get("house_in/", "trm_active_board_" + b.ToString());
				if (m2ChipImage != null)
				{
					M2ImageAtlas.AtlasRect atlasData = this.Cp.IMGS.Atlas.getAtlasData(m2ChipImage.SourceLayer.Img);
					if (atlasData.valid)
					{
						atlasData.initAtlasMd(Md, this.Cp.IMGS.MIchip);
						Md.RotaGraph(center_x, base.Meta.GetNm("door_trm", 0f, 0), 1f, 0f, null, false);
					}
				}
			}
		}
	}
}
