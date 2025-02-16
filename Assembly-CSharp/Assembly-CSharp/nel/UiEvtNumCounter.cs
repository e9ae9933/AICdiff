using System;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class UiEvtNumCounter : UiEvtDesignerBox
	{
		protected override string reserve_key
		{
			get
			{
				return "ENC";
			}
		}

		protected override float main_w
		{
			get
			{
				return IN.w * 0.45f;
			}
		}

		protected override float main_h
		{
			get
			{
				return IN.h * 0.26f;
			}
		}

		protected override void Awake()
		{
			this.margin_in_y = 74f;
			this.z_stencil = -0.5f;
			this.MaStencil = new MdArranger(null);
			base.Awake();
			if (this.M2D != null)
			{
				this.M2D.loadMaterialSnd("ev_city");
			}
		}

		public UiEvtNumCounter Init(int min, int max, int def, int _correct)
		{
			this.correct_score = _correct;
			this.BxC.item_margin_x_px = 34f;
			base.Init();
			this.cancel_pressable_x = false;
			this.BxC.alignx = ALIGN.CENTER;
			this.BxC.XSh(30f);
			this.BConSlider = this.BxC.addNumCounterT<aBtnNumCounterNel>(new DsnDataNumCounter
			{
				minval = min,
				maxval = max,
				def = X.MMX(min, def, max),
				name = "counter",
				h = 34f,
				slide_cur_digit_only = true
			});
			this.CancelBtn = this.BxC.addButtonT<aBtnNel>(new DsnDataButton
			{
				w = 240f,
				h = 30f,
				title = "Submit",
				skin_title = "&&Submit",
				fnClick = new FnBtnBindings(this.fnClickedBtn)
			});
			this.CancelBtn.setNaviL(this.BConSlider.Get(this.BConSlider.Length - 1), true, true);
			this.CancelBtn.setNaviR(this.BConSlider.Get(0), true, true);
			this.BConSlider.Get(0).Select(false);
			return this;
		}

		private bool fnClickedBtn(aBtn B)
		{
			string title = B.title;
			if (title != null && title == "Submit")
			{
				base.prompt_result = this.BConSlider.getValueString();
				if (this.correct_score < 0)
				{
					this.deactivate(false);
				}
				else
				{
					bool flag = this.correct_score == this.BConSlider.getDigitValue();
					this.t_rslt = (float)X.MPF(flag);
					CURS.Rem(B.hover_curs_category, "");
					B.Deselect(true);
					this.BxC.hide();
					if (flag)
					{
						SND.Ui.play("unlock_dial", false);
						this.MaSL = new MdArranger(this.MdStencil).Set(0, 0, this.MaStencil.getEndVerIndex() / 2, this.MaStencil.getEndTriIndex() / 2);
						this.MaSR = new MdArranger(this.MdStencil).Set(this.MaStencil.getEndVerIndex() / 2, this.MaStencil.getEndTriIndex() / 2, this.MaStencil.getEndVerIndex(), this.MaStencil.getEndTriIndex());
					}
					else
					{
						SND.Ui.play("locked", false);
					}
				}
			}
			return true;
		}

		public override bool runIRD(float fcnt)
		{
			if (!base.runIRD(fcnt))
			{
				return false;
			}
			if (X.D && this.t_rslt != 0f && X.Abs(this.t_rslt) < 75f)
			{
				MdArranger maStencil = this.MaStencil;
				UiBoxDesigner.createStencilTB(this.MdStencil.clearSimple(), this.BxC.h, 42f, true);
				if (this.t_rslt > 0f)
				{
					this.t_rslt += (float)X.AF;
					float num = X.ZSIN(this.t_rslt, 13f) * 8f;
					float num2 = X.ZLINE(this.t_rslt - 5f, 62f);
					float num3 = 110f * X.ZPOW(num2);
					if (this.isActive() && this.t_rslt >= 55f)
					{
						this.deactivate(false);
					}
					this.MaSL.translateAll(-num, -num3, false);
					this.MaSR.translateAll(num, num3, false);
					maStencil.setColAll(this.MdStencil.ColGrd.Set(4283780170U).mulA(1f - num2).C, false);
				}
				else
				{
					this.t_rslt -= (float)X.AF;
					if (this.isActive() && this.t_rslt <= -50f)
					{
						this.deactivate(false);
					}
					float num4 = 0f;
					float num5;
					Color32 color;
					if (NEL.isErrorVib(-this.t_rslt, out num5, out num4, 18.5f))
					{
						color = this.MdStencil.ColGrd.Set(4283780170U).blend(4294901760U, num5).C;
					}
					else
					{
						color = NEL.ColText;
					}
					maStencil.setColAll(color, false);
					maStencil.translateAll(num4, 0f, false);
				}
				this.MdStencil.updateForMeshRenderer(true);
			}
			return true;
		}

		private BtnContainerNumCounter<aBtnNumCounter> BConSlider;

		private float t_rslt;

		private MdArranger MaSL;

		private MdArranger MaSR;

		private int correct_score = -1;
	}
}
