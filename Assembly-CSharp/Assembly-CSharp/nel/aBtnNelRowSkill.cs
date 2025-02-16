using System;
using XX;

namespace nel
{
	public class aBtnNelRowSkill : aBtnNel
	{
		public aBtnNelRowSkill setContainer(UiSkillManageBox _Con)
		{
			this.Con = _Con;
			return this;
		}

		public override ButtonSkin makeButtonSkin(string key)
		{
			if (this.SkS == null)
			{
				this._SkinRow = (this.SkS = new ButtonSkinNelRowSkill(this, this.w, this.h));
				this.Skin = this._SkinRow;
			}
			return this.Skin;
		}

		public override ButtonSkin initializeSkin(string _skin, string _title = "")
		{
			base.initializeSkin(_skin, _title);
			this.Sk = SkillManager.Get(this.title);
			if (this.SkS != null)
			{
				this.SkS.initSkill(this.Sk);
			}
			if (this.Sk != null && !this.Sk.always_enable)
			{
				if (this.UseCheck == null)
				{
					this.UseCheck = IN.CreateGob(this, "-Check").AddComponent<aBtnNel>();
					this.UseCheck.w = this.Skin.sheight * 1.8f;
					this.UseCheck.h = this.Skin.sheight;
					this.UseCheck.addClickFn(new FnBtnBindings(this.fnCheckClick));
					this.UseCheck.Container = this.Container;
					this.UseCheck.hover_to_select = (this.UseCheck.click_to_select = false);
					this.UseCheck.secureNavi();
				}
				this.UseCheck.SetChecked(this.Sk.enabled, true);
				this.UseCheck.title = this.title + "-check";
				this.UseCheck.initializeSkin("checkbox", this.UseCheck.title);
				Designer.initSkinTitle(this.UseCheck, this.UseCheck.title, "&&CheckBox_Use");
				(this.UseCheck.get_Skin() as ButtonSkinCheckBox).setScale(1f);
				if (base.isActive())
				{
					this.UseCheck.bind();
				}
				else
				{
					this.UseCheck.hide();
				}
				this.UseCheck.gameObject.SetActive(true);
				IN.PosP(this.UseCheck.transform, this.Skin.swidth / 2f - this.Skin.sheight * 1.8f / 2f, 0f, -0.2f);
			}
			else if (this.UseCheck != null)
			{
				this.UseCheck.hide();
				this.UseCheck.gameObject.SetActive(false);
			}
			return this.Skin;
		}

		public override bool run(float fcnt)
		{
			if (!base.run(fcnt))
			{
				return false;
			}
			if (this.SkS != null && this.SkS.show_new_icon > 0)
			{
				this.SkS.fineNewIconBlink();
			}
			return true;
		}

		private bool fnCheckClick(aBtn Check)
		{
			if (this.Con == null || this.Sk == null)
			{
				return false;
			}
			if (!this.Sk.always_enable && (this.Sk.visible || X.DEBUGALLSKILL))
			{
				if (!this.Con.canEnableCheckClick(this.Sk))
				{
					return false;
				}
				Check.SetChecked(!Check.isChecked(), true);
				this.Sk.enabled = Check.isChecked();
				this.Con.fnSkillEnableChanged(this.Sk, false, true);
			}
			return true;
		}

		public override void bind()
		{
			base.bind();
			if (this.UseCheck != null && base.isActive())
			{
				this.UseCheck.bind();
			}
		}

		public override void ExecuteOnSubmitKey()
		{
			if (base.isChecked())
			{
				if (this.UseCheck != null)
				{
					this.UseCheck.ExecuteOnSubmitKey();
					return;
				}
			}
			else
			{
				base.ExecuteOnSubmitKey();
			}
		}

		public override void hide()
		{
			base.hide();
			if (this.UseCheck != null)
			{
				this.UseCheck.hide();
			}
			if (this.Sk != null && this.SkS != null && this.SkS.show_new_icon > 0 && !this.Sk.new_icon)
			{
				this.SkS.show_new_icon = 0;
			}
		}

		public override aBtn SetChecked(bool f, bool fine_flag = true)
		{
			if (!f && base.isChecked() && this.Sk != null && this.SkS != null && this.SkS.show_new_icon > 0 && !this.Sk.new_icon)
			{
				this.SkS.show_new_icon = 0;
			}
			return base.SetChecked(f, fine_flag);
		}

		public const float CHECK_W_RATE = 1.8f;

		public aBtnNel UseCheck;

		public PrSkill Sk;

		private UiSkillManageBox Con;

		private ButtonSkinNelRowSkill SkS;
	}
}
