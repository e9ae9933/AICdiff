using System;
using UnityEngine;

namespace XX
{
	public class DsnDataColorCell : DsnDataBtnBase
	{
		public Color32 def;

		public bool open_prompt = true;

		public bool use_text = true;

		public bool use_alpha = true;

		public string title = "";

		public string skin = "colorcell";

		public string skin_title;

		public float w = 70f;

		public float h = 20f;

		public aBtnColorCell.FnColorCellBindings fnPromptDone;
	}
}
