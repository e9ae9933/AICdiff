using System;
using UnityEngine;
using XX;

namespace nel
{
	public class ButtonSkinRowLangNel : ButtonSkinRowNelDark
	{
		public ButtonSkinRowLangNel(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			base.alignx = ALIGN.CENTER;
			this.Mtr = MTRX.newMtr(MTRX.ShaderGDT);
		}

		public override void destruct()
		{
			IN.DestroyOne(this.Mtr);
			this.Mtr = null;
			base.destruct();
		}

		protected override void fineDrawAdditional(ref float shift_px_x, ref float shift_px_y, ref bool shift_y_abs)
		{
			base.fineDrawAdditional(ref shift_px_x, ref shift_px_y, ref shift_y_abs);
			if (this.TxtLang != null)
			{
				if (shift_px_y != 0f)
				{
					shift_px_y += 4f;
				}
				this.MdLangIco.DrawCen(0f, shift_px_y + (float)(this.TxtLang.height / 2), null);
				shift_px_y += (float)this.TxtLang.height;
				this.MdLangIco.updateForMeshRenderer(false);
			}
		}

		public void addTextureIco(Texture Txt)
		{
			if (Txt == null)
			{
				return;
			}
			if (this.MdLangIco == null)
			{
				this.MdLangIco = base.makeMesh(this.Mtr);
			}
			this.MdLangIco.setMtrTexture("_MainTex", Txt);
			this.MdLangIco.use_cache = false;
			this.MdLangIco.clear(false, false);
			this.TxtLang = Txt;
			this.MdLangIco.initForImg(Txt);
		}

		protected MeshDrawer MdLangIco;

		private Texture TxtLang;

		private Material Mtr;
	}
}
