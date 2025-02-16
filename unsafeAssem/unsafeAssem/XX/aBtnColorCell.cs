using System;
using UnityEngine;

namespace XX
{
	public class aBtnColorCell : aBtn
	{
		protected override void Awake()
		{
			this.Col = new C32();
			base.Awake();
		}

		protected override void StartBtn()
		{
			base.StartBtn();
		}

		public override ButtonSkin makeButtonSkin(string key)
		{
			if (key != null)
			{
				if (!(key == "normal") && !(key == "colorcell"))
				{
					if (key == null)
					{
						goto IL_0043;
					}
					int length = key.Length;
				}
				return this.SkinCl = new ButtonSkinColorCell(this, this.w, this.h);
			}
			IL_0043:
			return this.SkinCl = new ButtonSkinColorCell(this, this.w, this.h);
		}

		public aBtn addPromptDoneFn(aBtnColorCell.FnColorCellBindings Fn)
		{
			aBtn.addFnT<aBtnColorCell.FnColorCellBindings>(ref this.AFnValPromptDone, Fn);
			return this;
		}

		private bool runFnColorCellBindings(aBtnColorCell.FnColorCellBindings[] AFn, Color32 pre_value, Color32 cur_value)
		{
			if (AFn == null)
			{
				return true;
			}
			bool flag = true;
			int num = AFn.Length;
			for (int i = 0; i < num; i++)
			{
				aBtnColorCell.FnColorCellBindings fnColorCellBindings = AFn[i];
				if (fnColorCellBindings == null)
				{
					return flag;
				}
				flag = fnColorCellBindings(this, pre_value, cur_value) && flag;
			}
			return flag;
		}

		public Color32 getColor()
		{
			if (!this.use_alpha)
			{
				this.Col.setA1(1f);
			}
			return this.Col.C;
		}

		public override void setValue(string s)
		{
			this.Col.rgbax = s;
			if (!this.use_alpha)
			{
				this.Col.setA1(1f);
			}
			if (this.SkinCl != null)
			{
				this.SkinCl.fine_color_flag = true;
				base.Fine(false);
			}
		}

		public void setValue(Color32 C)
		{
			this.Col.Set(C);
			if (!this.use_alpha)
			{
				this.Col.setA1(1f);
			}
			if (this.SkinCl != null)
			{
				this.SkinCl.fine_color_flag = true;
				base.Fine(false);
			}
		}

		public override string getValueString()
		{
			string text = this.Col.rgbax;
			while (this.use_alpha && text.Length < 8)
			{
				text = "0" + text;
			}
			return text;
		}

		private C32 Col;

		public bool open_prompt = true;

		public bool use_text = true;

		public bool use_alpha = true;

		private ButtonSkinColorCell SkinCl;

		private aBtnColorCell.FnColorCellBindings[] AFnValPromptDone;

		private DsPrmp Ds;

		public static int colmem_sort;

		public delegate bool FnColorCellBindings(aBtnColorCell _B, Color32 pre_value, Color32 cur_value);
	}
}
