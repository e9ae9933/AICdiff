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
			this.MdIco.chooseSubMesh(2, false, false);
			this.MdIco.setMaterial(MTRX.MIicon.getMtr(container_stencil_ref), false);
			this.MdIco.chooseSubMesh(0, false, false);
			this.MdIco.connectRendererToTriMulti(this.MMRD.GetMeshRenderer(this.MdIco));
		}

		public void initReel(ReelExecuter _Reel)
		{
			this.initReel(_Reel.getEType());
		}

		public void initReel(ReelExecuter.ETYPE _type)
		{
			this.reeltype = _type;
			this.IR = null;
			ReelExecuter.fineMaterialCol(this.MtrG, this.reeltype);
			ReelExecuter.fineMaterialCol(this.MtrW, this.reeltype);
			this.MdIco.chooseSubMesh(0, false, false);
			this.row_left_px = (int)(this.clip_left * 1.75f);
			this.setTitleText(this.B.title);
		}

		public void initIR(ReelManager.ItemReelContainer _IR)
		{
			this.reeltype = ReelExecuter.ETYPE._MAX;
			this.IR = _IR;
			this.row_left_px = (int)(this.clip_left * 1.66f);
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
			if (this.w * 64f < 100f)
			{
				return;
			}
			if (this.reeltype != ReelExecuter.ETYPE._MAX)
			{
				base.setTitleText(TX.GetA("Reel_etype_prefix", TX.Get("Reel_etype_" + this.reeltype.ToString(), "")));
			}
			if (this.IR != null)
			{
				base.setTitleText(TX.ReplaceTX(this.IR.tx_key, false));
			}
		}

		protected override void drawCheckedIcon(float sht_clk_pixel = 0f)
		{
		}

		public float clip_left
		{
			get
			{
				return (float)((this.w * 64f < 210f) ? 10 : 28);
			}
		}

		protected override void RowFineAfter(float w, float h)
		{
			if (this.reeltype != ReelExecuter.ETYPE._MAX)
			{
				this.MdIco.chooseSubMesh(0, false, false);
				float num = 12f;
				float num2 = ((w < 100f) ? 0f : (-w * 0.5f + this.clip_left - 6f));
				if (w >= 210f)
				{
					ReelExecuter.drawFrameS(this.MdIco, this.reeltype, num2, 0f, num, 1f, this.alpha_);
				}
				this.MdIco.chooseSubMesh(1, false, false);
				ReelExecuter.drawIconS(this.MdIco, this.reeltype, num2, 0f, num, 1f, 0f, this.alpha_);
				this.MdIco.chooseSubMesh(0, false, false);
			}
			if (this.IR != null)
			{
				float num3 = ((w < 100f) ? 0f : (-w * 0.5f + this.clip_left));
				this.MdIco.chooseSubMesh(2, false, false);
				this.IR.drawSmallIcon(this.MdIco, num3, 0f, this.alpha_, 1f, false);
				this.MdIco.chooseSubMesh(0, false, false);
			}
			if (base.hilighted)
			{
				base.drawCheckedIcon(0f);
			}
			base.RowFineAfter(w, h);
		}

		private ReelExecuter.ETYPE reeltype = ReelExecuter.ETYPE._MAX;

		private Material MtrG;

		private Material MtrW;

		private ReelManager.ItemReelContainer IR;
	}
}
