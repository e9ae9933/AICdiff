using System;
using XX;

namespace nel.gm
{
	internal class UiGMCSkill : UiGMC
	{
		internal UiGMCSkill(UiGameMenu _GM)
			: base(_GM, CATEG.SKILL, true, 0, 0, 1, 1, 1f, 5f)
		{
		}

		public override bool initAppearMain()
		{
			if (base.initAppearMain())
			{
				this.SkillMng.initAppear();
				return true;
			}
			this.SkillMng = new UiSkillManageBox();
			this.SkillMng.CreateTo(this.BxR, (PrSkill Sk) => this.GM.isEditState() && this.GM.isShowingGMC(this));
			return true;
		}

		protected override bool initAppearSubAreaInner(UiBoxDesigner Ds, int i, bool is_top)
		{
			Ds.alignx = ALIGN.CENTER;
			if (base.initAppearSubAreaInner(Ds, i, is_top))
			{
				return true;
			}
			this.SkillMng.CreateDescTo(Ds);
			return true;
		}

		public override void quitAppear()
		{
			base.quitAppear();
		}

		internal override void initEdit()
		{
			if (this.SkillMng.FirstFocus != null)
			{
				this.SkillMng.FirstFocus.Select(true);
			}
			this.SkillMng.activateEdit();
		}

		internal override void quitEdit()
		{
			this.SkillMng.deactivateEdit();
		}

		internal override void releaseEvac()
		{
			if (this.SkillMng != null)
			{
				this.SkillMng.deactivateDesigner();
				this.SkillMng = this.SkillMng.destruct();
			}
			base.releaseEvac();
		}

		internal override void runAppearing()
		{
			base.runAppearing();
			this.SkillMng.runAppearSkillManager();
		}

		internal override GMC_RES runEdit(float fcnt, bool handle)
		{
			if (this.SkillMng == null || !this.SkillMng.runSkillManager(handle))
			{
				return GMC_RES.BACK_CATEGORY;
			}
			return GMC_RES.CONTINUE;
		}

		public bool isTabOpening(SkillManager.SKILL_CTG categ)
		{
			return this.SkillMng.isTabOpening(categ);
		}

		private aBtnMagSel MagSel;

		private UiSkillManageBox SkillMng;
	}
}
