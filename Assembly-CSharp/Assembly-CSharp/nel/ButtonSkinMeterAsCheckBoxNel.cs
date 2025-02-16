using System;
using PixelLiner;
using XX;

namespace nel
{
	public class ButtonSkinMeterAsCheckBoxNel : ButtonSkinMeterAsCheckBox
	{
		public ButtonSkinMeterAsCheckBoxNel(aBtnMeter _B, float _w = 0f, float _h = 0f, bool force_multiply_view = false)
			: base(_B, _w, _h, force_multiply_view)
		{
			this.LineColor = C32.d2c(4283780170U);
			this.BaseColor = C32.d2c(4293321691U);
			this.BaseCheckedColor = C32.d2c(4278246796U);
			this.MarkerColor = C32.d2c(4291611332U);
			this.MarkerPushedColor = C32.d2c(4283780170U);
			this.LockedLineColor = C32.d2c(4288057994U);
			this.LockedBaseColor = C32.d2c(4290689711U);
			this.LockedBaseCheckedColor = C32.d2c(3707764736U);
			this.PFCheckMark = MTRX.getPF("nel_check");
		}

		private int changed_f0 = -1;

		private bool pre_checked_flag;

		private const int T_ANM = 40;

		private PxlFrame PFCheckMark;
	}
}
