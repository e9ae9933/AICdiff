using System;
using PixelLiner;
using XX;

namespace nel
{
	public class ButtonSkinMiniSortNel : ButtonSkinMiniNel
	{
		public ButtonSkinMiniSortNel(aBtn _B, float _w = 0f, float _h = 0f)
			: base(_B, _w, _h)
		{
		}

		public override ButtonSkin setTitle(string str)
		{
			this.title = str;
			this.PF_Offline = MTRX.getPF(str);
			this.PF_Ascend = MTRX.getPF(str + "_ascend");
			this.PF_Descend = MTRX.getPF(str + "_descend");
			return this;
		}

		public override ButtonSkin Fine()
		{
			this.PF = ((!base.isChecked()) ? this.PF_Offline : (this.is_descend ? this.PF_Descend : this.PF_Ascend));
			return base.Fine();
		}

		protected override void drawChecked()
		{
		}

		public bool is_descend;

		private PxlFrame PF_Offline;

		private PxlFrame PF_Ascend;

		private PxlFrame PF_Descend;
	}
}
