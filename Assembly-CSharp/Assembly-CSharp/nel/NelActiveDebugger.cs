using System;
using XX;

namespace nel
{
	public class NelActiveDebugger : ActiveDebugger
	{
		protected override bool ReloadF9(MODIF md)
		{
			if (md == MODIF.NONE || md == MODIF.OPT)
			{
				this.reloadTexts();
			}
			else if (md == MODIF.SH_OP_CT_CM)
			{
				base.ReloadF9(md);
				if (UILog.Instance != null)
				{
					UILog.Instance.AddAlert("debug.txt reloaded.", UILogRow.TYPE.ALERT);
				}
			}
			return true;
		}

		private void reloadTexts()
		{
			base.ReloadF9(MODIF.OPT);
			NelItem.localized_tx_key = "";
			if (UILog.Instance != null)
			{
				UILog.Instance.AddAlert("Text Reloaded.", UILogRow.TYPE.ALERT);
			}
			CFG.refineAllLanguageCache(true);
		}
	}
}
