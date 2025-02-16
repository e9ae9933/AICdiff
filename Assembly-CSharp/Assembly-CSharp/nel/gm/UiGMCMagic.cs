using System;
using XX;

namespace nel.gm
{
	internal class UiGMCMagic : UiGMC
	{
		internal UiGMCMagic(UiGameMenu _GM)
			: base(_GM, CATEG.MAGIC, false, 0, 0, 0, 0, 1f, 1f)
		{
		}

		public override bool initAppearMain()
		{
			if (base.initAppearMain())
			{
				return true;
			}
			this.BxR.margin_in_lr = 50f;
			this.BxR.init();
			this.MagSel = this.BxR.addButtonT<aBtnMagSel>(new DsnDataButton
			{
				name = "magsel",
				title = "magsel",
				w = this.BxR.use_w,
				h = this.BxR.use_h
			});
			this.MagSel.initMagicSelector(this.Pr, this.BxDesc);
			this.MagSel.hide();
			return true;
		}

		internal override void initEdit()
		{
			this.MagSel.bind();
			this.MagSel.Select(true);
		}

		internal override void quitEdit()
		{
			this.MagSel.blurDesc();
			this.MagSel.hide();
		}

		internal override GMC_RES runEdit(float fcnt, bool handle)
		{
			if (!this.MagSel.isActive())
			{
				return GMC_RES.BACK_CATEGORY;
			}
			return GMC_RES.CONTINUE;
		}

		private aBtnMagSel MagSel;
	}
}
