using System;
using UnityEngine;
using XX;

namespace nel
{
	public class ButtonSkinMagSel : ButtonSkin
	{
		public ButtonSkinMagSel(aBtn _B, float _w, float _h)
			: base(_B, 0f, 0f)
		{
			this.Md = base.makeMesh(MTRX.MtrMeshNormal);
			this.MdIco = base.makeMesh(BLEND.NORMAL, MTR.MIiconL);
			this.w = ((_w > 0f) ? _w : 540f) * 0.015625f;
			this.h = ((_h > 0f) ? _h : 480f) * 0.015625f;
			float num = X.Mn(this.h * 64f / 768f, this.w * 64f / 640f);
			this.scale = X.Mn(1f, num);
			this.stx = -this.scale * 256f * 0.25f;
			this.fine_continue_flags |= 1U;
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f || this.BSel == null || this.BSel.MagSel == null)
			{
				return this;
			}
			X.Mn(540f, this.w * 64f);
			X.Mn(480f, this.h * 64f);
			this.Md.Col = this.Md.ColGrd.Set(4283780170U).mulA(this.alpha_).C;
			this.BSel.MagSel.drawWholeButtonsTo(this.Md, this.MdIco, null, this.stx, 0f, this.scale, true, 1f, true, (float)(this.B.isActive() ? (-1) : 0), this.cursel, false, false);
			this.MdIco.updateForMeshRenderer(false);
			this.Md.updateForMeshRenderer(false);
			base.Fine();
			return this;
		}

		public void fineCursor(MagicSelector.MAGA cursel)
		{
			this.cursel = cursel;
			this.fine_flag = true;
			float num = 0f;
			float num2 = 0f;
			switch (cursel)
			{
			case MagicSelector.MAGA.LR:
				num = 1f;
				break;
			case MagicSelector.MAGA.T:
				num2 = 1f;
				break;
			case MagicSelector.MAGA.B:
				num2 = -1f;
				break;
			case MagicSelector.MAGA.BURST:
				num = -0.5f;
				num2 = 0.5f;
				break;
			}
			this.curs_level_x = (this.stx + this.scale * 256f * num) * 0.015625f / (this.w / 2f);
			this.curs_level_y = (0f + this.scale * 256f * num2) * 0.015625f / (this.h / 2f);
		}

		public void checkMouseCurs()
		{
			Vector3 vector = base.global2local(IN.getMousePos(null));
			float num = 256f * this.scale / 2f;
			float num2 = (vector.x * 64f - this.stx) / num;
			float num3 = vector.y * 64f / num;
			if (X.LENGTHXYS(num2, num3, 0f, 0f) < 1f)
			{
				this.BSel.setCursorTo(MagicSelector.MAGA.NORMAL, false);
				return;
			}
			if (X.LENGTHXYS(num2, num3, 0f, 1f) < 1f)
			{
				this.BSel.setCursorTo(MagicSelector.MAGA.T, false);
				return;
			}
			if (X.LENGTHXYS(num2, num3, 0f, -1f) < 1f)
			{
				this.BSel.setCursorTo(MagicSelector.MAGA.B, false);
				return;
			}
			if (X.LENGTHXYS(num2, num3, 1f, 0f) < 1f)
			{
				this.BSel.setCursorTo(MagicSelector.MAGA.LR, false);
				return;
			}
			if (X.LENGTHXYS(num2, num3, -0.5f, 0.5f) < 1f)
			{
				this.BSel.setCursorTo(MagicSelector.MAGA.BURST, false);
			}
		}

		protected MeshDrawer Md;

		protected MeshDrawer MdIco;

		private MagicSelector.MAGA cursel;

		public aBtnMagSel BSel;

		public const float BOX_SIZE_W = 540f;

		public const float BOX_SIZE_H = 480f;

		public float scale;

		public float stx;

		public const float DAIA_WH = 256f;
	}
}
