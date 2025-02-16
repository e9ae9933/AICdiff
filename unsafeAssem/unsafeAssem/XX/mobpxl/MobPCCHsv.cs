using System;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;

namespace XX.mobpxl
{
	internal class MobPCCHsv : MobPCC
	{
		public MobPCCHsv()
			: base(MobPCC.TYPE.HSV)
		{
			if (MobPCCHsv.Mtr == null)
			{
				MobPCCHsv.Mtr = new Material(MTRX.getShd("Hachan/HsvShift"));
			}
		}

		public override void executeToImage(PxlPartsInfo TargetPI, Texture TxP, RenderTexture Dest, RenderTexture Src, MeshDrawer MdForPc)
		{
			MobPCCHsv.Mtr.SetTexture("_MainTex", Src);
			MobPCCHsv.Mtr.SetTexture("_PartsTex", TxP);
			int num = this.flags;
			if ((num & 1) != 0)
			{
				num &= -3;
			}
			MobPCCHsv.Mtr.SetFloat("_Flag", (float)num);
			MobPCCHsv.Mtr.SetFloat("_H_Shift", (float)this.h / 60f);
			MobPCCHsv.Mtr.SetFloat("_S_Shift", (float)this.s / 100f);
			MobPCCHsv.Mtr.SetFloat("_V_Shift", (float)this.v / 100f);
			base.executeToImage(MobPCCHsv.Mtr, TargetPI, Dest, Src, MdForPc);
		}

		public override void readFromBytesInner(ByteArray Ba)
		{
			this.h = (int)Ba.readShort();
			this.s = Ba.readByte();
			if (this.s >= 127)
			{
				this.s -= 256;
			}
			this.v = Ba.readByte();
			if (this.v >= 127)
			{
				this.v -= 256;
			}
			this.flags = Ba.readByte();
		}

		public int h;

		public int s;

		public int v;

		public int flags;

		private static Material Mtr;
	}
}
