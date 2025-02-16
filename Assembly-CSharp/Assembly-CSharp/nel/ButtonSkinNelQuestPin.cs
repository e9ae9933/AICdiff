using System;
using XX;

namespace nel
{
	public class ButtonSkinNelQuestPin : ButtonSkinNelUi
	{
		public ButtonSkinNelQuestPin(aBtn _B, float _w, float _h)
			: base(_B, _w, _h)
		{
		}

		public QuestTracker.Quest Q
		{
			get
			{
				return this.Q_;
			}
			set
			{
				if (this.Q == value)
				{
					return;
				}
				this.Q_ = value;
				this.fine_flag = true;
			}
		}

		protected override void fineDrawAdditional(ref float icon_shift_px_x, ref float icon_shift_px_y, ref bool shift_y_abs)
		{
			base.prepareIconMesh();
			this.MdIco.Col = ((this.Q != null) ? this.Q.PinColor : C32.d2c(4283780170U));
			NEL.drawQuestDepertPin(this.MdIco, -this.w * 64f * 0.5f + 20f, -this.h * 64f * 0.3f - 2f, this.alpha_, 1f, 0.4f, 0f);
			icon_shift_px_x += 38f;
			base.fineDrawAdditional(ref icon_shift_px_x, ref icon_shift_px_y, ref shift_y_abs);
		}

		private QuestTracker.Quest Q_;
	}
}
