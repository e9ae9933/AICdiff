using System;
using UnityEngine;
using XX;

namespace nel
{
	public class aBtnMagSel : aBtnNel
	{
		protected override void Awake()
		{
			base.Awake();
			this.hover_snd = "";
			base.addHoverFn(delegate(aBtn V)
			{
				this.showHoverDesc();
				return this;
			});
		}

		public override ButtonSkin makeButtonSkin(string key)
		{
			if (key != null && (key == "normal" || (key != null && key.Length == 0)))
			{
				this.click_snd = "tool_hand_init";
				this.Skin = (this.MsSkin = new ButtonSkinMagSel(this, this.w, this.h));
				this.MsSkin.BSel = this;
				return this.Skin;
			}
			return base.makeButtonSkin(key);
		}

		public aBtnMagSel initMagicSelector(PR _Pr, UiBoxDesigner _BxDesc)
		{
			this.Pr = _Pr;
			this.MagSel = this.Pr.Skill.MagicSel;
			this.BxDesc = _BxDesc;
			this.cursel = this.MagSel.get_cursor();
			this.Pdesc = null;
			this.desc_activated = false;
			if (this.BxDesc != null)
			{
				this.DescParent = this.BxDesc.transform.parent;
			}
			base.secureNavi();
			this.fineSel();
			return this;
		}

		public void setCursorTo(MagicSelector.MAGA maga, bool force = false)
		{
			if (!force && maga == this.cursel)
			{
				return;
			}
			this.cursel = maga;
			this.fineSel();
			SND.Ui.play("cursor", false);
			if (base.isSelected())
			{
				this.showHoverDesc();
				CURS.focusOnBtn(this, true);
			}
		}

		private void fineSel()
		{
			switch (this.cursel)
			{
			case MagicSelector.MAGA.NORMAL:
			case MagicSelector.MAGA.T:
			case MagicSelector.MAGA.B:
				this.center_sel = this.cursel;
				break;
			case MagicSelector.MAGA.BURST:
				this.left_sel = this.cursel;
				break;
			}
			this.MsSkin.fineCursor(this.cursel);
		}

		public void showHoverDesc()
		{
			if (this.BxDesc == null)
			{
				return;
			}
			if (this.Pdesc == null)
			{
				this.BxDesc.Clear();
				this.BxDesc.margin_in_lr = 35f;
				this.BxDesc.margin_in_tb = 20f;
				this.BxDesc.WH(650f, 220f);
				this.BxDesc.init();
				this.BxDesc.addP(new DsnDataP("", false)
				{
					name = "descp",
					text = " ",
					size = 14f,
					alignx = ALIGN.LEFT,
					aligny = ALIGNY.TOP,
					Col = MTRX.ColTrnsp,
					TxCol = C32.d2c(4283780170U),
					swidth = this.BxDesc.use_w,
					sheight = this.BxDesc.use_h,
					lineSpacing = 1.28f,
					text_margin_y = 0f,
					text_auto_condense = true,
					html = true
				}, false);
				this.Pdesc = this.BxDesc.Get("descp", false) as FillBlock;
				this.BxDesc.activate();
			}
			this.desc_activated = true;
			MagicSelector.Enchant dataAt = this.MagSel.getDataAt(this.cursel);
			if (dataAt.kind == MGKIND.NONE)
			{
				if (this.BxDesc.isActive())
				{
					this.BxDesc.deactivate();
					return;
				}
			}
			else
			{
				string text = dataAt.kind.ToString().ToLower();
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.Add("<b>").AddTxA("Mag_title_" + text, false).Add("</b>\n\n")
						.AddTxA("Mag_desc_" + text, false);
					this.Pdesc.Txt(stb);
				}
				if (!this.BxDesc.isActive())
				{
					this.BxDesc.activate();
				}
				Vector3 vector = ((this.BxDesc.transform.parent != null) ? this.BxDesc.transform.parent.position : Vector3.zero);
				Vector3 position = base.transform.position;
				vector -= position;
				this.BxDesc.positionD(this.Skin.swidth * this.Skin.curs_level_x * 0.4f - vector.x * 64f, this.Skin.sheight * this.Skin.curs_level_y * 0.6f + (this.MsSkin.scale * 256f + 40f) * ((this.Skin.curs_level_y > 0.25f) ? (-1.15f) : ((this.Skin.curs_level_y > -0.125f) ? (-1f) : 1.25f)) - vector.y * 64f, 1, 30f);
			}
		}

		public override void bind()
		{
			base.bind();
		}

		public override void hide()
		{
			base.hide();
			this.blurDesc();
		}

		public void blurDesc()
		{
			if (this.BxDesc != null && this.desc_activated)
			{
				this.BxDesc.deactivate();
			}
			this.desc_activated = false;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.blurDesc();
		}

		public override bool run(float fcnt)
		{
			if (!base.run(fcnt))
			{
				return false;
			}
			if (base.isActive())
			{
				if (IN.use_mouse)
				{
					this.MsSkin.checkMouseCurs();
				}
				if (IN.isCancel())
				{
					this.hide();
				}
			}
			return true;
		}

		protected override aBtn simulateNaviTranslationInner(ref int a, aBtn Dep)
		{
			if (a >= 0)
			{
				int num = CAim._XD(a, 1);
				int num2 = CAim._YD(a, 1);
				a = -1;
				if (num != 0)
				{
					switch (this.cursel)
					{
					case MagicSelector.MAGA.NORMAL:
					case MagicSelector.MAGA.T:
					case MagicSelector.MAGA.B:
						this.setCursorTo((num < 0) ? MagicSelector.MAGA.BURST : MagicSelector.MAGA.LR, false);
						break;
					case MagicSelector.MAGA.LR:
						if (num < 0)
						{
							this.setCursorTo(this.center_sel, false);
						}
						else
						{
							a = -2;
						}
						break;
					case MagicSelector.MAGA.BURST:
						if (num > 0)
						{
							this.setCursorTo(this.center_sel, false);
						}
						else
						{
							a = -2;
						}
						break;
					}
				}
				if (num2 != 0)
				{
					switch (this.cursel)
					{
					case MagicSelector.MAGA.NORMAL:
						this.setCursorTo((num2 < 0) ? MagicSelector.MAGA.B : MagicSelector.MAGA.T, false);
						break;
					case MagicSelector.MAGA.LR:
						a = -2;
						break;
					case MagicSelector.MAGA.T:
						if (num2 < 0)
						{
							this.setCursorTo(MagicSelector.MAGA.NORMAL, false);
						}
						else
						{
							a = -2;
						}
						break;
					case MagicSelector.MAGA.B:
						if (num2 > 0)
						{
							this.setCursorTo(MagicSelector.MAGA.NORMAL, false);
						}
						else
						{
							a = -2;
						}
						break;
					case MagicSelector.MAGA.BURST:
						this.setCursorTo((num2 < 0) ? MagicSelector.MAGA.NORMAL : MagicSelector.MAGA.T, false);
						break;
					}
				}
			}
			return null;
		}

		public ButtonSkinMagSel MsSkin;

		private PR Pr;

		public MagicSelector MagSel;

		private Transform DescParent;

		private UiBoxDesigner BxDesc;

		private MagicSelector.MAGA cursel;

		private MagicSelector.MAGA center_sel;

		private MagicSelector.MAGA left_sel = MagicSelector.MAGA.BURST;

		private FillBlock Pdesc;

		private bool desc_activated;
	}
}
