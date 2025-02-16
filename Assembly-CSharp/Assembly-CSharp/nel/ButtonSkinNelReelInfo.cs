using System;
using UnityEngine;
using XX;

namespace nel
{
	public class ButtonSkinNelReelInfo : ButtonSkinNelUi
	{
		public ButtonSkinNelReelInfo(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.fix_text_size_ = 18f;
			int container_stencil_ref = base.container_stencil_ref;
			this.MdIco = base.makeMesh(this.MtrG = MTRX.newMtr(MTR.ShaderGDTGradationMap));
			this.MdIco.initForImgAndTexture(MTRX.MIicon);
			if (container_stencil_ref >= 0)
			{
				this.MtrG.SetFloat("_StencilRef", (float)container_stencil_ref);
				this.MtrG.SetFloat("_StencilComp", 3f);
			}
			this.MdIco.chooseSubMesh(1, false, false);
			this.MdIco.setMaterial(this.MtrW = MTRX.newMtr(MTR.ShaderGDTWaveColor), false);
			this.MdIco.initForImgAndTexture(MTRX.MIicon);
			if (container_stencil_ref >= 0)
			{
				this.MtrW.SetFloat("_StencilRef", (float)container_stencil_ref);
				this.MtrW.SetFloat("_StencilComp", 3f);
			}
			this.MdIco.chooseSubMesh(0, false, false);
			this.MdIco.connectRendererToTriMulti(this.MMRD.GetMeshRenderer(this.MdIco));
		}

		public void initReel(ReelExecuter _Reel)
		{
			this.Reel = _Reel;
			ReelExecuter.fineMaterialCol(this.MtrG, this.Reel.getEType());
			ReelExecuter.fineMaterialCol(this.MtrW, this.Reel.getEType());
			this.MdIco.chooseSubMesh(0, false, false);
			this.row_left_px = 68;
			this.setTitleText(this.B.title);
		}

		public override void destruct()
		{
			if (this.MtrG != null)
			{
				IN.DestroyOne(this.MtrG);
				IN.DestroyOne(this.MtrW);
			}
			base.destruct();
		}

		protected override void setTitleText(string str)
		{
			if (this.Reel == null || this.w * 64f < 100f)
			{
				return;
			}
			base.setTitleText(TX.GetA("Reel_etype_prefix", TX.Get("Reel_etype_" + this.Reel.getEType().ToString(), "")));
		}

		protected override void drawCheckedIcon(float sht_clk_pixel = 0f)
		{
		}

		protected override void RowFineAfter(float w, float h)
		{
			if (this.Reel != null)
			{
				this.MdIco.chooseSubMesh(0, false, false);
				float num = 12f;
				float num2 = ((w < 100f) ? 0f : (-w * 0.5f + h * 0.5f + 20f));
				this.Reel.drawFrame(this.MdIco, num2, 0f, num, 1f, this.alpha_);
				this.MdIco.chooseSubMesh(1, false, false);
				this.Reel.drawIcon(this.MdIco, num2, 0f, num, 1f, 0f, this.alpha_);
				this.MdIco.chooseSubMesh(0, false, false);
			}
			if (base.hilighted)
			{
				base.drawCheckedIcon(0f);
			}
			base.RowFineAfter(w, h);
		}

		private ReelExecuter Reel;

		private Material MtrG;

		private Material MtrW;
	}
}
