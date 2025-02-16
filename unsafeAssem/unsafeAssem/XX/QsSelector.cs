using System;
using System.Collections.Generic;

namespace XX
{
	public sealed class QsSelector
	{
		public QsSelector(LabeledInputField _Li)
		{
			this.Li = _Li;
			this.ARemake = new List<IKeyArrayRemakeable>(1);
		}

		public QsSelector Add(IKeyArrayRemakeable I)
		{
			this.ARemake.Add(I);
			return this;
		}

		public void Fine(int def_time = 22)
		{
			int count = this.ARemake.Count;
			for (int i = 0; i < count; i++)
			{
				this.ARemake[i].RemakeDefault(X.Mx(0, def_time));
			}
		}

		public void Dispose()
		{
			int count = this.ARemake.Count;
			for (int i = 0; i < count; i++)
			{
				this.ARemake[i].RemakeDefault(-1);
			}
		}

		public static LabeledInputField CreateTo(Designer Ds, string name, float w, IKeyArrayRemakeable Item, string def = null)
		{
			LabeledInputField LiQs = Ds.addInput(new DsnDataInput
			{
				unselectable = 2,
				name = name,
				skin = "qs",
				label = "セレクタ",
				def = "",
				bounds_w = w - Ds.item_margin_x_px - 20f,
				size = 12,
				alloc_char = REG.RegW
			});
			Ds.addButton(new DsnDataButton
			{
				unselectable = 2,
				w = 20f,
				h = 20f,
				name = "delete_" + name,
				title = "delete_" + name,
				skin_title = "x",
				fnClick = delegate(aBtn B)
				{
					LiQs.setValue("", true, false);
					return true;
				}
			});
			QsSelector.AssignTo(LiQs, Item, def);
			return LiQs;
		}

		public static void AssignTo(LabeledInputField LI, IKeyArrayRemakeable Item, string def = null)
		{
			if (LI.get_Skin() == null)
			{
				LI.makeButtonSkin(LI.skin);
			}
			ISelectorHolder selectorHolder = LI.get_Skin() as ISelectorHolder;
			if (selectorHolder != null)
			{
				selectorHolder.getSelector().Add(Item);
			}
			else
			{
				X.de("LI のスキンに qs を設定すること", null);
			}
			if (TX.valid(def))
			{
				LI.setValue(def);
				selectorHolder.getSelector().Fine(0);
			}
		}

		public static int Splice(List<string> Akeys, string s, bool strict = true, QsSelector.FnStringConvert fnStringConvert = null)
		{
			if (Akeys == null || TX.noe(s))
			{
				return -1;
			}
			int count = Akeys.Count;
			int num = 0;
			using (STB stb = TX.PopBld(null, 0))
			{
				for (int i = 0; i < count; i++)
				{
					string text = Akeys[i];
					stb.Clear();
					if (fnStringConvert != null)
					{
						fnStringConvert(text, stb);
					}
					else
					{
						stb.Set(text);
					}
					if (TX.qsCheckRelative(stb, s, strict) > 0f)
					{
						Akeys[num++] = text;
					}
				}
			}
			if (num < count)
			{
				Akeys.RemoveRange(num, count - num);
			}
			return num;
		}

		public readonly LabeledInputField Li;

		private List<IKeyArrayRemakeable> ARemake;

		public delegate void FnStringConvert(string src, STB Stb);
	}
}
