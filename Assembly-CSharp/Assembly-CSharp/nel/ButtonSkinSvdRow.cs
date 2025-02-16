using System;
using XX;

namespace nel
{
	public class ButtonSkinSvdRow : ButtonSkinNelUi
	{
		public float shift_left_px
		{
			get
			{
				return (float)((this.index == 0) ? 85 : 20);
			}
		}

		public ButtonSkinSvdRow(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.fine_continue_flags |= 10U;
			base.fix_text_size = 20f;
			this.row_left_px = 10;
			this.alignx_ = ALIGN.LEFT;
			this.aligny_ = ALIGNY.MIDDLE;
		}

		public override ButtonSkin setTitle(string str)
		{
			int num = X.NmI(str, -1, false, false);
			if (num > 0)
			{
				str = X.spr0(num, 2, ' ') + ".";
			}
			else if (num == 0)
			{
				str = TX.Get("SVD_0", "");
			}
			if (this.Svd != null && this.Svd.loadstate == SVD.sFile.STATE.ERROR)
			{
				str = str + " " + TX.Get("SVD_Error_Occured", "");
			}
			return base.setTitle(str);
		}

		public SVD.sFile getSvdData()
		{
			return this.Svd;
		}

		public override void destruct()
		{
			base.destruct();
			if (this.MdSavedEffect != null)
			{
				this.MdSavedEffect.destruct();
			}
		}

		public void fineMarker()
		{
			bool flag = COOK.loaded_index == this.index;
			if (flag != this.marked)
			{
				this.fine_flag = true;
				this.marked = flag;
				if (this.marked)
				{
					if (this.MdMark == null)
					{
						this.MdMark = base.makeMesh(MTRX.getMtr(MTRX.MtrMeshDashLine.shader, base.container_stencil_ref));
					}
					this.redrawMarker();
					return;
				}
				if (this.MdMark != null)
				{
					this.MdMark.clear(false, false);
					this.MdMark.updateForMeshRenderer(false);
				}
			}
		}

		public void redrawMarker()
		{
			if (!this.marked)
			{
				return;
			}
			float num = this.w * 64f - 2f;
			float num2 = this.h * 64f - 2f;
			this.MdMark.Col = this.MdMark.ColGrd.Set(this.tx_col_normal).mulA(this.alpha_).C;
			this.MdMark.RectDashedM(0f, 0f, num, num2, 38, 1f, 0.5f, false, false);
			this.MdMark.updateForMeshRenderer(false);
		}

		public void setData(int _index, SVD.sFile _Svd, bool saved = false)
		{
			this.Svd = _Svd;
			this.index = _index;
			this.setTitle(this.index.ToString());
			if (this.Svd == null || !this.Svd.header_prepared)
			{
				return;
			}
			if (this.Svd != null)
			{
				if (this.ATx == null)
				{
					this.ATx = new TextRenderer[4];
					this.ATx[0] = IN.CreateGob(this.Gob, "-NoelTitle").AddComponent<TextRenderer>();
					this.ATx[1] = IN.CreateGob(this.Gob, "-TR").AddComponent<TextRenderer>();
					this.ATx[2] = IN.CreateGob(this.Gob, "-BL").AddComponent<TextRenderer>();
					this.ATx[3] = IN.CreateGob(this.Gob, "-RB").AddComponent<TextRenderer>();
					for (int i = 0; i < 4; i++)
					{
						this.ATx[i].CopyFrom(this.Tx, false);
					}
					this.ATx[0].aligny = ALIGNY.MIDDLE;
					this.ATx[0].html_mode = true;
					this.ATx[1].alignx = ALIGN.CENTER;
					this.ATx[3].alignx = ALIGN.RIGHT;
					this.ATx[3].letter_spacing = 0.94f;
					this.ATx[3].line_spacing = 0.92f;
					this.ATx[3].html_mode = true;
					this.ATx[0].size = (this.ATx[1].size = 12f);
					this.ATx[2].size = 18f;
					this.ATx[3].size = (float)(X.ENG_MODE ? 14 : 18);
					this.ATx[2].auto_condense = true;
					this.ATx[2].max_swidth_px = this.w * 64f - 75f - this.shift_left_px - 210f;
				}
				if (this.index == 0)
				{
					this.Tx.LetterSpacing(0.96f);
				}
			}
			if (saved)
			{
				SND.Ui.play("saved", false);
				this.PtcSavedEffect = new EfParticleOnce("ui_saved", EFCON_TYPE.UI);
				if (this.MdSavedEffect == null)
				{
					this.MdSavedEffect = MeshDrawer.prepareMeshRenderer(IN.CreateGob(this.B.gameObject, "-MdSVE"), MTRX.MIicon.getMtr(-1), -0.5f, -1, null, true, true);
				}
			}
			this.reposit(false);
		}

		public void reposit(bool only_alpha = false)
		{
			if (this.Svd == null || !this.Svd.header_prepared || this.ATx == null)
			{
				return;
			}
			float num = this.w * 64f;
			float num2 = this.h * 64f;
			float num3 = num / 2f;
			float num4 = num2 / 2f;
			float num5 = -num3 + 75f + this.shift_left_px;
			float num6 = num4 - num2 * 0.28f;
			float num7 = -num4 + num2 * 0.38f;
			if (!only_alpha)
			{
				IN.PosP(this.ATx[0], num5, num6, -0.04f);
				IN.PosP(this.ATx[1], num3 - 130f, num6, -0.04f);
				IN.PosP(this.ATx[2], num5 - 10f, num7, -0.04f);
				IN.PosP(this.ATx[3], num3 - 23f, num7, -0.04f);
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.AddTxA("Name_Noel", false).Add("\n@");
					stb.AddTxA("Area_" + this.Svd.whole_map_key, false);
					this.ATx[0].Txt(stb);
				}
				using (STB stb2 = TX.PopBld(null, 0))
				{
					stb2.AddTxA("SVD_topright", false).TxRpl((int)this.Svd.hp_noel).TxRpl((int)this.Svd.maxhp_noel)
						.TxRpl((int)this.Svd.mp_noel)
						.TxRpl((int)this.Svd.maxmp_noel);
					this.ATx[1].Txt(stb2);
				}
				using (STB stb3 = TX.PopBld(null, 0))
				{
					stb3.Set("- ").Add(SCN.getScenarioPhaseTitle((int)this.Svd.phase));
					this.ATx[2].Txt(stb3);
				}
				this.fineStr();
			}
			int num8 = this.ATx.Length;
			for (int i = 0; i < num8; i++)
			{
				this.ATx[i].Alpha(this.alpha_);
			}
			base.prepareIconMesh();
		}

		public void fineStr()
		{
			if (this.ATx == null)
			{
				return;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				if (SVD.show_explore_timer)
				{
					if (this.Svd.explore_timer == 0U)
					{
						stb.Set(" --- ");
					}
					else
					{
						stb.AddTxA("Explore_timer", false).TxRpl((int)(this.Svd.explore_timer / 3600U)).TxRpl(X.spr0((int)(this.Svd.explore_timer / 60U % 60U), 2, '0'))
							.Add(" ");
					}
				}
				else if (this.Svd.modified.Year < 2000)
				{
					stb.Set(" --- ");
				}
				else
				{
					stb.Add(this.Svd.modified);
				}
				this.ATx[3].Txt(stb);
			}
		}

		public override ButtonSkin Fine()
		{
			base.Fine();
			if (this.ATx != null)
			{
				int num = this.ATx.Length;
				for (int i = 0; i < num; i++)
				{
					this.ATx[i].Alpha(this.alpha_).Col(ButtonSkinRow.Col.C);
				}
			}
			if (this.index == 0)
			{
				this.Tx.max_swidth_px = 96f;
				this.Tx.auto_condense_line = true;
			}
			if (this.Svd != null && this.Svd.loadstate == SVD.sFile.STATE.NO_LOAD)
			{
				this.fine_flag = true;
			}
			if (this.PtcSavedEffect != null)
			{
				this.fine_flag = true;
			}
			return this;
		}

		protected override void RowFineAfter(float w, float h)
		{
			float num = w / 2f;
			float num2 = h / 2f;
			float num3 = -num + 75f + this.shift_left_px;
			float num4 = num2 - h * 0.28f;
			if (this.Svd == null)
			{
				this.Md.Line(-num + 50f, 0f, -num + 90f, 0f, 1f, false, 0f, 0f);
			}
			else if (this.Svd.loadstate == SVD.sFile.STATE.NO_LOAD)
			{
				NEL.loadDrawingRow(this.Md, 20f, 0f, 4283780170U, 26, 30f, 1f);
			}
			if (base.isChecked())
			{
				this.Md.ColGrd.Set(this.tx_col_normal);
				if (base.isLocked())
				{
					this.Md.ColGrd.Set(this.tx_col_locked);
				}
				else if (this.isPushDown())
				{
					this.Md.ColGrd.Set(this.tx_col_pushdown);
				}
				else if (base.isChecked())
				{
					this.Md.ColGrd.Set(this.tx_col_checked);
				}
				else if (base.isHoveredOrPushOut())
				{
					this.Md.ColGrd.blend(this.tx_col_locked, 0.3f + 0.3f * X.COSIT(40f));
				}
				this.MdStripe.Col = C32.MulA(this.Md.ColGrd.C, this.alpha_ * 0.14f);
				this.MdStripe.uvRectN(X.Cos(0.7853982f), X.Sin(-0.7853982f));
				this.MdStripe.allocUv2(6, false).Uv2(3f, 0.5f, false);
				this.MdStripe.Rect(0f, 0f, w - 4f, h - 4f, false);
				this.MdStripe.allocUv2(0, true);
			}
			if (this.PtcSavedEffect != null)
			{
				int myAf = this.PtcSavedEffect.myAf;
				this.Md.Col = this.Md.ColGrd.Set(uint.MaxValue).mulA(this.alpha_).C;
			}
			this.Md.Col = this.Md.ColGrd.Set(4290689711U).mulA(this.alpha_).C;
			this.Md.Line(-num, -num2, num, -num2, 1f, false, 0f, 0f);
			if (this.MdIco != null)
			{
				this.MdIco.Col = C32.MulA(uint.MaxValue, this.alpha_);
				this.MdIco.RotaPF(num3 - 26f, num4, 2f, 2f, 0f, MTRX.getPF("IconNoel0"), false, false, false, uint.MaxValue, false, 0);
			}
			if (this.MdSavedEffect != null && this.MdSavedEffect.getVertexMax() > 0)
			{
				this.MdSavedEffect.clear(false, false);
			}
			if (this.PtcSavedEffect != null)
			{
				this.Md.Box(0f, 0f, num * 2f, num2 * 2f, 0f, false);
				if (!this.PtcSavedEffect.drawTo(this.MdSavedEffect, 0f, 0f, this.alpha_, (int)num2, (float)this.PtcSavedEffect.myAf, 0f))
				{
					this.PtcSavedEffect = null;
				}
				this.MdSavedEffect.updateForMeshRenderer(false);
			}
			base.RowFineAfter(w, h);
		}

		protected override void drawCheckedIcon(float sht_clk_pixel = 0f)
		{
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
					if (this.ATx != null)
					{
						this.reposit(true);
					}
					this.redrawMarker();
				}
			}
		}

		public SVD.sFile Svd;

		public int index;

		public bool current_file;

		private TextRenderer[] ATx;

		public const float DEFAULT_H_ROW = 88f;

		private bool marked;

		private EfParticleOnce PtcSavedEffect;

		private MeshDrawer MdMark;

		private MeshDrawer MdSavedEffect;
	}
}
