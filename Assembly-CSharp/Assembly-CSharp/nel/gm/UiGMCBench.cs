using System;
using XX;

namespace nel.gm
{
	internal class UiGMCBench : UiGMC
	{
		internal UiGMCBench(UiGameMenu _GM)
			: base(_GM, CATEG.BENCH, false, 0, 0, 0, 0, 1f, 1f)
		{
		}

		public override bool initAppearMain()
		{
			if (base.initAppearMain())
			{
				return true;
			}
			this.BxR.init();
			this.BxR.addP(new DsnDataP("", false)
			{
				text = TX.GetA("GameMenu_Bench_Front", UiBenchMenu.getCommandListTitle()),
				size = 18f,
				alignx = ALIGN.CENTER,
				aligny = ALIGNY.MIDDLE,
				Col = MTRX.ColTrnsp,
				TxCol = C32.d2c(4283780170U),
				swidth = this.BxR.use_w,
				sheight = this.BxR.use_h - 10f,
				html = true
			}, false);
			return true;
		}

		internal override void initEdit()
		{
		}

		internal override void quitEdit()
		{
		}
	}
}
