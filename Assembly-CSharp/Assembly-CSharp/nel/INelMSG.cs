using System;
using evt;
using XX;

namespace nel
{
	public interface INelMSG
	{
		void progressReserved();

		void executeRestMsgCmd(int count);

		bool all_char_shown { get; set; }

		string talker_snd { get; }

		void setHkdsType(NelMSG.HKDSTYPE _type);

		EMOT default_emot { get; }

		uint default_col { get; }

		bool is_temporary_hiding { get; }

		void FixTextContent(STB Stb);

		void TextRendererUpdated();
	}
}
