using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class ButtonSkinFDItemRow : ButtonSkinNelUi
	{
		public ButtonSkinFDItemRow(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.fine_on_binding_changing = false;
			this.TxR = base.MakeTx("-text_r");
			this.TxR.html_mode = true;
			this.TxR.LetterSpacing(1.01f);
			this.TxR.Size(12f);
			this.TxR.aligny = ALIGNY.MIDDLE;
			this.TxR.alignx = ALIGN.RIGHT;
			this.Md.chooseSubMesh(1, false, false);
			this.Md.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, base.container_stencil_ref), false);
			this.Md.chooseSubMesh(0, false, false);
			base.bottom_line = true;
			this.Md.connectRendererToTriMulti(base.getMeshRenderer(this.Md));
		}

		public void setItem(ButtonSkinFDItemRow Src)
		{
			this.setItem(Src.FDCon, Src.M2D, Src.Fdr);
		}

		public void setItem(UiFieldGuide _FDCon, NelM2DBase _M2D, UiFieldGuide.FDR _Fdr)
		{
			this.FDCon = _FDCon;
			this.M2D = _M2D;
			this.Fdr = _Fdr;
			this.Itm = this.Fdr.Itm;
			this.grade = this.Fdr.grade;
			base.fix_text_size = this.h * 64f * 0.45f;
			if (this.Tx == null)
			{
				this.setTitle("");
			}
			else
			{
				this.Tx.size = base.fix_text_size;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				if (!this.Fdr.valid)
				{
					stb.Clear().Add(NelItem.Unknown.getLocalizedName(this.grade, null));
					this.text_alpha = 0.6f;
				}
				else if (this.Itm != null)
				{
					if (this.grade >= 0 && this.Itm.obtain_count > 0 && !this.Itm.fd_obtain_just_touched)
					{
						this.M2D.IMNG.holdItemString(stb, this.Itm, -1, true);
						this.TxR.Txt(stb);
					}
					else
					{
						this.text_alpha = 0.6f;
					}
					stb.Clear().Add(this.Itm.getLocalizedName(this.grade, null));
				}
				else if (this.Fdr.FSmn != null)
				{
					stb.Clear().Add(this.Fdr.FSmn.Summoner.name_localized);
					if (this.Fdr.FSmn.defeat_count == 0)
					{
						this.text_alpha = 0.6f;
					}
					this.Fdr.FSmn.B = this.B as aBtnFDRow;
				}
				base.setTitleTextS(stb);
			}
			this.TxR.Alpha(this.alpha_ * this.text_alpha);
			this.TxR.StencilRef(base.container_stencil_ref);
			Vector3 localPosition = this.TxR.transform.localPosition;
			localPosition.x = this.w / 2f - 0.3125f;
			localPosition.z = -0.001f;
			this.TxR.transform.localPosition = localPosition;
			IN.setZ(this.Tx.transform, -0.001f);
			this.row_right_px = 20;
			this.row_left_px = 68;
			this.fine_flag = true;
		}

		private void prepareTxC()
		{
			if (this.TxC == null)
			{
				this.TxC = base.MakeTx("-text_c");
				this.TxC.html_mode = true;
				this.TxC.Col(4283780170U);
				this.TxC.Alpha(this.alpha_ * this.text_alpha);
				this.TxC.alignx = ALIGN.CENTER;
				this.TxC.size = this.Tx.size * 0.75f;
				IN.Pos(this.TxC.transform, this.w * 0.28f, 0f, -0.01f);
			}
		}

		public void clearCenterPowerT()
		{
			if (this.TxC != null)
			{
				this.TxC.text_content = "";
			}
		}

		public void setCenterPowerTRpi(RecipeManager.RPI_EFFECT rpi, int grade)
		{
			if (!this.Fdr.valid)
			{
				return;
			}
			if (this.Itm != null && this.Itm.RecipeInfo != null && this.Itm.RecipeInfo.Oeffect100.ContainsKey(rpi))
			{
				this.prepareTxC();
				using (STB stb = TX.PopBld(null, 0))
				{
					using (STB stb2 = TX.PopBld(null, 0))
					{
						RecipeManager.getRPIEffectDescriptionTo(stb, rpi, this.Itm.getGradeVariation(grade, null).getDetailTo(stb2.Clear(), this.Itm.RecipeInfo.Oeffect100[rpi] / this.Itm.max_grade_enpower, "\n", false), 1);
						this.TxC.Txt(stb);
					}
				}
			}
		}

		public void setCenterPowerT(int t, string suffix = "")
		{
			if (!this.Fdr.valid)
			{
				return;
			}
			this.prepareTxC();
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.Add(t);
				stb.Add(suffix);
				this.TxC.Txt(stb);
			}
		}

		protected override void RowFineAfter(float w, float h)
		{
			if (this.Tx != null)
			{
				this.TxR.Col(this.Tx.TextColor).Alpha(this.alpha_);
				this.Md.chooseSubMesh(1, false, true);
				this.Itm = (this.Fdr.valid ? this.Itm : NelItem.Unknown);
				float num = -w * 0.5f + (float)this.row_left_px - 20f;
				bool flag = false;
				if (this.Itm != null)
				{
					this.Itm.drawIconTo(this.Md, null, 0, 1, num, 0f, 1f, this.alpha_ * this.text_alpha * (base.isLocked() ? 0.45f : 1f), null);
					flag = this.Itm.fd_favorite;
				}
				else if (this.Fdr.FSmn != null)
				{
					this.Md.chooseSubMesh(0, false, false);
					this.Md.Col = this.Md.ColGrd.Set(4282004532U).mulA(this.alpha_).C;
					this.Md.KadomaruRect(num, 0f, 30f, 30f, 8f, 0f, false, 0f, 0f, false);
					this.Md.chooseSubMesh(1, false, false);
					this.Md.Col = this.Md.ColGrd.Set(4294908737U).mulA(this.alpha_).C;
					this.Md.RotaPF(num, 0f, 1f, 1f, 0f, MTR.AItemIcon[29], false, false, false, uint.MaxValue, false, 0);
					flag = this.Fdr.FSmn.fd_favorite;
					int num2 = this.Fdr.FSmn.Summoner.grade;
					float num3 = w * 0.5f - 28f;
					PxlFrame pf = MTRX.getPF("marker_meter");
					for (int i = 0; i < num2; i++)
					{
						this.Md.RotaPF(num3, 0f, 1f, 1f, 0f, pf, false, false, false, uint.MaxValue, false, 0);
						num3 -= 11f;
					}
				}
				if (flag)
				{
					this.Md.chooseSubMesh(1, false, false);
					this.Md.Col = C32.MulA(MTRX.ColWhite, this.alpha_);
					this.Md.RotaPF(num - 28f, 0f, 1f, 1f, 0f, MTRX.getPF("favorite_star"), false, false, false, uint.MaxValue, false, 0);
				}
				this.Md.chooseSubMesh(0, false, false);
			}
			base.RowFineAfter(w, h);
		}

		public override float text_alpha
		{
			set
			{
				base.text_alpha = value;
				if (this.TxR != null)
				{
					this.TxR.Alpha(this.text_alpha * this.alpha_);
				}
				if (this.TxC != null)
				{
					this.TxC.Alpha(this.text_alpha * this.alpha_);
				}
			}
		}

		private UiFieldGuide FDCon;

		private NelM2DBase M2D;

		private TextRenderer TxC;

		private TextRenderer TxR;

		private NelItem Itm;

		public int grade;

		public UiFieldGuide.FDR Fdr;

		private const float off_tx_alpha = 0.6f;
	}
}
