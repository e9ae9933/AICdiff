using System;
using evt;
using UnityEngine;
using XX;

namespace nel
{
	public class SandDrawCheck : MonoBehaviour, INelMSG
	{
		public void progressReserved()
		{
		}

		public void executeRestMsgCmd(int count)
		{
		}

		public bool all_char_shown { get; set; }

		public string talker_snd { get; }

		public void setHkdsType(NelMSG.HKDSTYPE _type)
		{
		}

		public EMOT default_emot
		{
			get
			{
				return EMOT.FADEIN;
			}
		}

		public uint default_col
		{
			get
			{
				return uint.MaxValue;
			}
		}

		public bool is_temporary_hiding
		{
			get
			{
				return false;
			}
		}

		public void FixTextContent(STB Stb)
		{
		}

		public void TextRendererUpdated()
		{
		}
	}
}
