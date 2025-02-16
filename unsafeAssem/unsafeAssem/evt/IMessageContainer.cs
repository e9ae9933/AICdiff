using System;
using System.Collections.Generic;
using XX;

namespace evt
{
	public interface IMessageContainer
	{
		int getLoadLineCount(EvReader ER, CsvReader rER, List<string> Adest = null, bool no_error = false);

		void hideMsg(bool immediate = false);

		void hideMsg(StringHolder rER_without, bool immediate = false);

		void hold();

		void setHandle(bool f);

		int makeFromEvent(StringHolder rER, EvReader curEv);

		int showImmediate(bool show_only_first_char = false, bool no_snd = false);

		int getCurrentMsgKey(out string person_key, out string label);

		void clearValues();

		void initEvent(EvMsgCommand CmdListener);

		void quitEvent();

		bool progressNextParagraph();

		bool isAllCharsShown();

		float getAppearTime();

		bool isActive();

		bool run(float fcnt, bool skipping);

		void destructGob();

		void fineTargetFont();

		void fineFrontConfirmer();

		bool isHidingTemporary();

		bool checkHideVisibilityTemporary(bool hideable, bool force_change = false);
	}
}
