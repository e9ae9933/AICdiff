using System;
using XX;

namespace nel
{
	public class aBtnNelRowSkill : aBtnNel
	{
		public aBtnNelRowSkill setContainer(UiSkillManageBox _Con)
		{
			this.Con = _Con;
			if (this.UseCheck == null && this.Sk != null && !this.Sk.always_enable)
			{
				this.UseCheck = IN.CreateGob(this, "-Check").AddComponent<aBtnNel>();
				this.UseCheck.title = this.title + "-check";
				this.UseCheck.w = this.Skin.sheight * 1.8f;
				this.UseCheck.h = this.Skin.sheight;
				this.UseCheck.SetChecked(this.Sk.enabled, true);
				this.UseCheck.addClickFn(new FnBtnBindings(this.fnCheckClick));
				this.UseCheck.Container = this.Container;
				this.UseCheck.hover_to_select = (this.UseCheck.click_to_select = false);
				this.UseCheck.initializeSkin("checkbox", this.UseCheck.title);
				this.UseCheck.secureNavi();
				Designer.initSkinTitle(this.UseCheck, this.UseCheck.title, "&&CheckBox_Use");
				(this.UseCheck.get_Skin() as ButtonSkinCheckBox).setScale(1f);
				if (base.isActive())
				{
					this.UseCheck.bind();
				}
				IN.PosP(this.UseCheck.transform, this.Skin.swidth / 2f - this.Skin.sheight * 1.8f / 2f, 0f, -0.2f);
			}
			return this;
		}

		public override ButtonSkin makeButtonSkin(string key)
		{
			if (this.Sk == null)
			{
				this.Sk = SkillManager.Get(this.title);
			}
			if (this.Sk == null)
			{
				return base.makeButtonSkin(key);
			}
			this._SkinRow = (this.SkS = new ButtonSkinNelRowSkill(this, this.w, this.h));
			this.Skin = this._SkinRow;
			bool new_icon = this.Sk.new_icon;
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
			if (this.Con == null)
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
