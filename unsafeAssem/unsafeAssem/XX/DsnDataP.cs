using System;
using UnityEngine;

namespace XX
{
	public class DsnDataP : DsnData
	{
		public DsnDataP(string t = "", bool _html = false)
		{
			this.text = t;
			this.html = _html;
		}

		public DsnDataP Text(string t)
		{
			this.text = t;
			return this;
		}

		public float swidth;

		public float sheight;

		public float size;

		public float radius;

		public float lineSpacing = -1f;

		public float letterSpacing = -1f;

		public bool text_auto_wrap = TX.isEnglishLang();

		public bool text_auto_condense = true;

		public float text_margin_x = 5f;

		public float text_margin_y = 3f;

		public bool use_collider = true;

		public int use_valotile = -1;

		public char[] Aword_splitter;

		public MFont TargetFont;

		public STB Stb;

		public ALIGN alignx;

		public ALIGNY aligny;

		public ALIGNY image_aligny;

		public Color32 Col = new Color32(0, 0, 0, 0);

		public Color32 TxCol = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public Color32 TxBorderCol = new Color32(0, 0, 0, 0);

		public string text = "";

		public bool do_not_error_unknown_tag;

		public bool html;
	}
}
