using System;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;

namespace XX.mobpxl
{
	public abstract class MobPCC
	{
		public MobPCC(MobPCC.TYPE _type)
		{
			this.type = _type;
		}

		public abstract void executeToImage(PxlPartsInfo TargetPI, Texture TxP, RenderTexture Dest, RenderTexture Src, MeshDrawer MdForPc);

		public void executeToImage(Material Mtr, PxlPartsInfo TargetPI, RenderTexture Dest, RenderTexture Src, MeshDrawer MdForPc)
		{
			Color32 color = C32.d2c(TargetPI.topcolor);
			if (MdForPc != null)
			{
				GL.PushMatrix();
				BLIT.JustPaste(Src, Dest);
				GL.PopMatrix();
				Graphics.SetRenderTarget(Dest);
				int vertexMax = MdForPc.getVertexMax();
				Color32[] colorArray = MdForPc.getColorArray();
				for (int i = 0; i < vertexMax; i++)
				{
					colorArray[i] = color;
				}
				BLIT.RenderToGLImmediateSP(MdForPc, Mtr, MdForPc.getTriMax());
			}
			else
			{
				Graphics.SetRenderTarget(Dest);
				GL.LoadOrtho();
				Mtr.SetPass(0);
				GL.Begin(4);
				GL.Color(color);
				BLIT.JustPasteOrtho2();
				GL.End();
			}
			GL.Flush();
		}

		public static MobPCC readFromBytes(ByteArray Ba)
		{
			MobPCC.TYPE type = (MobPCC.TYPE)Ba.readByte();
			if (type == MobPCC.TYPE.HSV)
			{
				MobPCCHsv mobPCCHsv = new MobPCCHsv();
				mobPCCHsv.readFromBytesInner(Ba);
				return mobPCCHsv;
			}
			if (type != MobPCC.TYPE.TONECURVE)
			{
				return null;
			}
			MobPCCTC mobPCCTC = new MobPCCTC();
			mobPCCTC.readFromBytesInner(Ba);
			return mobPCCTC;
		}

		public abstract void readFromBytesInner(ByteArray Ba);

		public readonly MobPCC.TYPE type;

		public bool visible = true;

		public enum TYPE
		{
			NONE,
			HSV,
			TONECURVE
		}
	}
}
