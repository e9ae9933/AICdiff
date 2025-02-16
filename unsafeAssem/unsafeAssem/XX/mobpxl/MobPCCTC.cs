using System;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;

namespace XX.mobpxl
{
	internal class MobPCCTC : MobPCC
	{
		public MobPCCTC()
			: base(MobPCC.TYPE.TONECURVE)
		{
			this.Tce = new ToneCurveEditor();
			if (MobPCCTC.Mtr == null)
			{
				MobPCCTC.Mtr = new Material(MTRX.getShd("Hachan/ToneCurveShift"));
			}
		}

		public override void executeToImage(PxlPartsInfo TargetPI, Texture TxP, RenderTexture Dest, RenderTexture Src, MeshDrawer MdForPc)
		{
			MobPCCTC.Mtr.SetColorArray("_ColorMap", this.Tce.getColorMap());
			MobPCCTC.Mtr.SetTexture("_MainTex", Src);
			MobPCCTC.Mtr.SetTexture("_PartsTex", TxP);
			base.executeToImage(MobPCCTC.Mtr, TargetPI, Dest, Src, MdForPc);
		}

		public override void readFromBytesInner(ByteArray Ba)
		{
			this.Tce.readFromBytes(Ba);
		}

		private ToneCurveEditor Tce;

		private static Material Mtr;
	}
}
