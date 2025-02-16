using System;
using UnityEngine;
using XX;

namespace nel
{
	public class ButtonSkinUItemSel : ButtonSkin
	{
		public ButtonSkinUItemSel(aBtn _B, float _w, float _h)
			: base(_B, _w * 0.015625f, _h * 0.015625f)
		{
			this.Md = base.makeMesh(MTRX.MtrMeshNormal);
			this.MdIco = base.makeMesh(BLEND.NORMAL, MTRX.MIicon);
			this.MdStripe = base.makeMesh(MTRX.MtrMeshStriped);
			if (_w == 0f)
			{
				this.w = 7.8125f;
			}
			if (_h == 0f)
			{
				this.h = 7.8125f;
			}
			this.cell_size = X.Mn(this.w, this.h) / 3.125f;
			this.TxC = IN.CreateGob(this.Gob, "-text").AddComponent<TextRenderer>();
			this.TxC.Align(ALIGN.CENTER).AlignY(ALIGNY.MIDDLE).Size(13f)
				.LetterSpacing(0.95f)
				.LineSpacing(1.05f);
			this.TxC.use_valotile = true;
			this.TxC.html_mode = true;
			this.TxC.auto_wrap = true;
			this.TxC.max_swidth_px = this.cell_size * 1.5f * 64f - 14f;
			if (this.B.Container != null && this.B.Container.stencil_ref >= 0)
			{
				this.TxC.StencilRef(this.B.Container.stencil_ref);
			}
			this.fine_continue_flags |= 1U;
			this.curs_level_x = 0f;
			this.curs_level_y = 0f;
		}

		public override bool use_valotile
		{
			get
			{
				return this.use_valotile_;
			}
			set
			{
				if (this.use_valotile == value)
				{
					return;
				}
				base.use_valotile = value;
				this.TxC.use_valotile = value;
			}
		}

		public override ButtonSkin setTitle(string str)
		{
			this.title = str;
			return this;
		}

		public override void setEnable(bool f)
		{
			base.setEnable(f);
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f || this.BSel == null)
			{
				return this;
			}
			this.BSel.USel.drawMesh(0f, 0f, 2f, -1f * this.alpha, this.Md, this.MdIco, this.MdStripe, this.cell_size * 64f, this.BSel.USel.getSelectedIndex(), -2);
			this.MdIco.updateForMeshRenderer(false);
			this.MdStripe.updateForMeshRenderer(false);
			this.Md.updateForMeshRenderer(false);
			base.Fine();
			return this;
		}

		public void fineCursor()
		{
			this.fine_flag = true;
			int selectedIndex = this.BSel.USel.getSelectedIndex();
			if (selectedIndex < 0)
			{
				this.curs_level_x = 0f;
				this.curs_level_y = 0f;
				return;
			}
			this.curs_level_x = (float)CAim._XD(selectedIndex, 1) * 0.61f + 0.14f;
			this.curs_level_y = (float)CAim._YD(selectedIndex, 1) * 0.61f - 0.14f;
		}

		public void checkMouseCurs()
		{
			Vector3 vector = base.global2local(IN.getMousePos(null));
			if (X.BTW(this.cell_size * 0.5625f, X.LENGTHXYN(0f, 0f, vector.x, vector.y), this.cell_size * 1.5625f))
			{
				int num = (X.BTW(-this.cell_size * 0.5f, vector.x, this.cell_size * 0.5f) ? 0 : X.MPF(vector.x > 0f));
				int num2 = (X.BTW(-this.cell_size * 0.5f, vector.y, this.cell_size * 0.5f) ? 0 : X.MPF(vector.y > 0f));
				if (num != 0 || num2 != 0)
				{
					this.BSel.USel.focusManual((int)CAim.get_aim2(0f, 0f, (float)num, (float)num2, false));
				}
			}
		}

		public override float alpha
		{
			get
			{
				return this.alpha_;
			}
			set
			{
				if (this.alpha_ != value)
				{
					base.alpha = value;
					if (this.TxC != null)
					{
						this.TxC.alpha = value;
					}
				}
			}
		}

		protected MeshDrawer Md;

		protected MeshDrawer MdIco;

		protected MeshDrawer MdStripe;

		public aBtnUItemSel BSel;

		public float cell_size;

		public TextRenderer TxC;
	}
}
