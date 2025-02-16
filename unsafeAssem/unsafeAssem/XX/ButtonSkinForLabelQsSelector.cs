using System;

namespace XX
{
	public class ButtonSkinForLabelQsSelector : ButtonSkinForLabel, ISelectorHolder
	{
		public ButtonSkinForLabelQsSelector(LabeledInputField _B, float _w, float _h)
			: base(_B, _w, _h)
		{
			this.Qs = new QsSelector(_B);
		}

		public QsSelector getSelector()
		{
			return this.Qs;
		}

		public override void destruct()
		{
			base.destruct();
			this.Qs.Dispose();
		}

		public override bool CharInputted(string new_string, string pre_string)
		{
			if (!base.CharInputted(new_string, pre_string))
			{
				return false;
			}
			if (this.Qs != null)
			{
				this.Qs.Fine(22);
			}
			return true;
		}

		public QsSelector Qs;
	}
}
