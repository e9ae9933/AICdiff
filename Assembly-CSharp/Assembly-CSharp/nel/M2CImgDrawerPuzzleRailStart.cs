using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class M2CImgDrawerPuzzleRailStart : M2CImgDrawer
	{
		public M2CImgDrawerPuzzleRailStart(MeshDrawer Md, int _lay, M2Puts _Cp)
			: base(Md, _lay, _Cp, false)
		{
			this.aim = (byte)base.Meta.getDirsI("puzzle_rail_start", this.Cp.rotation, this.Cp.flip, 1, 1);
			this.Cp.arrangeable = true;
		}

		public override bool entryMainPicToMesh(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
		{
			if (!base.Mp.apply_chip_effect)
			{
				return base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			}
			if (this.aim == 255)
			{
				return base.entryMainPicToMesh(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			}
			Color32 col = Md.Col;
			if (this.layer < 3)
			{
				Md.Col = C32.d2c(4281545523U);
			}
			float num = (float)base.Meta.GetI("puzzle_rail_start", 4, 2);
			bool flag = base.entryMainPicToMesh(Md, meshx + (float)CAim._XD((int)this.aim, 1) * num, meshy + (float)CAim._YD((int)this.aim, 1) * num, _zmx, _zmy, _rotR, Ms);
			Md.Col = col;
			return flag;
		}

		public static float get_real_draw_map_cx(byte aim, M2Chip Cp, float shift_px)
		{
			if (aim != 255)
			{
				return Cp.mapcx + (float)CAim._XD((int)aim, 1) * shift_px / Cp.CLEN;
			}
			return Cp.mapcx;
		}

		public static float get_real_draw_map_cy(byte aim, M2Chip Cp, float shift_px)
		{
			if (aim != 255)
			{
				return Cp.mapcy - (float)CAim._YD((int)aim, 1) * shift_px / Cp.CLEN;
			}
			return Cp.mapcy;
		}

		private M2PuzzleSwitch MvHit;

		private byte aim = byte.MaxValue;

		public const int shift_px_default = 4;
	}
}
