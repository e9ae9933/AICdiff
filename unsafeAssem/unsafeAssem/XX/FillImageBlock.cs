using System;
using PixelLiner;
using UnityEngine;

namespace XX
{
	[RequireComponent(typeof(BoxCollider2D))]
	public class FillImageBlock : FillBlock
	{
		public override void StartFb(string text, STB Stb, bool text_html_mode)
		{
			if (this.initted)
			{
				return;
			}
			base.StartFb(text, Stb, text_html_mode);
			if (this.Mrd != null && this.Mpb != null)
			{
				this.Mpb.Mrd = this.Mrd;
				this.Mpb.fineMpb();
			}
		}

		protected override Material replaceMaterial(Material _Mtr = null)
		{
			if (_Mtr == null && this.MI_ != null)
			{
				if (this.stencil_ref >= 0)
				{
					if (this.stencil_lessequal)
					{
						_Mtr = MTRX.MIicon.getMtr(BLEND.NORMALST, this.stencil_ref + 256);
					}
					else
					{
						_Mtr = MTRX.MIicon.getMtr(BLEND.NORMALST, this.stencil_ref);
					}
				}
				else
				{
					_Mtr = MTRX.MIicon.getMtr(BLEND.NORMAL, -1);
				}
			}
			return base.replaceMaterial(_Mtr);
		}

		public void replaceMaterialManual(Material _Mtr)
		{
			this.MI_ = null;
			if (this.Mpb != null)
			{
				this.Mpb.Clear(true);
			}
			base.replaceMaterial(_Mtr);
		}

		public void replaceMainTextureManual(Texture Tx)
		{
			this.MI_ = null;
			if (this.Mpb == null)
			{
				this.Mpb = new MProperty(this.Mrd, 0);
				if (this.Valot != null)
				{
					this.Valot.Mpb = this.Mpb;
				}
			}
			this.Mpb.SetTexture("_MainTex", Tx, true);
		}

		public MImage MI
		{
			get
			{
				return this.MI_;
			}
			set
			{
				if (this.MI_ == value)
				{
					return;
				}
				bool flag = this.MI_ != null;
				this.MI_ = value;
				bool flag2 = this.MI_ != null;
				if (flag != flag2)
				{
					this.replaceMaterial(null);
				}
				if (flag2)
				{
					this.textureOverride(this.MI_.Tx);
				}
			}
		}

		public void textureOverride(Texture Tx)
		{
			if (this.Mpb == null)
			{
				this.Mpb = new MProperty(this.Mrd, 0);
				if (this.Valot != null)
				{
					this.Valot.Mpb = this.Mpb;
				}
			}
			this.Mpb.SetTexture("_MainTex", Tx, true);
		}

		protected override bool use_mesh_renderer
		{
			get
			{
				return true;
			}
		}

		protected override void redrawMesh()
		{
			this.Md.Col = C32.MulA(this.Col, base.alpha);
			if (this.FnDrawFIB != null)
			{
				bool flag = true;
				if (!this.FnDrawFIB(this.Md, this, base.alpha, ref flag))
				{
					this.redraw_flag = true;
				}
				if (flag)
				{
					this.Md.updateForMeshRenderer(false);
					return;
				}
			}
			else if (this.FnDraw != null)
			{
				if (!this.FnDraw(this.Md, base.alpha))
				{
					this.redraw_flag = true;
					return;
				}
			}
			else if (this.MI_ != null)
			{
				this.Md.initForImg(this.MI_.Tx, this.UvRect, true);
				if (this.PF == null)
				{
					this.Md.DrawScaleGraph(0f, 0f, this.scale, this.scale, null);
				}
				else
				{
					this.Md.RotaPF(0f, 0f, this.scale, this.scale, 0f, this.PF, false, false, false, uint.MaxValue, false, 0);
				}
				this.Md.updateForMeshRenderer(false);
			}
		}

		public override bool use_valotile
		{
			set
			{
				if (base.use_valotile == value)
				{
					return;
				}
				base.use_valotile = value;
				if (value && this.Valot != null)
				{
					this.Valot.Mpb = this.Mpb;
				}
			}
		}

		public override void OnDestroy()
		{
			this.MI_ = null;
			this.FnDraw = null;
			this.FnDrawFIB = null;
			base.OnDestroy();
		}

		private MImage MI_;

		public PxlFrame PF;

		public bool stencil_lessequal = true;

		public Rect UvRect = new Rect(0f, 0f, 1f, 1f);

		public float scale = 1f;

		public MeshDrawer.FnGeneralDraw FnDraw;

		public FillImageBlock.FnDrawInFIB FnDrawFIB;

		private MProperty Mpb;

		public delegate bool FnDrawInFIB(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer);

		private enum MTR_DP
		{
			PRE_MADE,
			CREATED,
			CREATED_ST
		}
	}
}
