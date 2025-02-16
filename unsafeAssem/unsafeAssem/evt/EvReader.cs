using System;
using XX;

namespace evt
{
	public class EvReader : CsvReaderA
	{
		public EvReader(string _name, int _start_line = 0, string[] _Avariables = null, EvReader cloneFrom = null)
			: base(null, EV.getVariableContainer())
		{
			this.name = _name;
			this.Apreserve_states = new EV.ST[0];
			this.Avariables = _Avariables;
			this.start_line = _start_line;
			this.tilde_replace = true;
			if (cloneFrom != null)
			{
				this.Olabels = cloneFrom.Olabels;
				this.Adata = cloneFrom.Adata;
				this.Apreserve_states = X.concat<EV.ST>(cloneFrom.Apreserve_states, null, -1, -1);
				if (this.start_line >= 0)
				{
					this.cur_line = this.start_line;
				}
				this.start_line = -1;
				base.last_str = null;
				return;
			}
			EV.getEventContent(_name, this);
		}

		public override CsvReader parseText(string basetext)
		{
			base.parseText(basetext);
			if (this.start_line >= 0)
			{
				this.cur_line = this.start_line;
			}
			this.start_line = -1;
			return this;
		}

		public string progLines(int i = 1)
		{
			string text;
			using (STB stb = TX.PopBld(null, 0))
			{
				using (STB stb2 = TX.PopBld(null, 0))
				{
					while (--i >= 0)
					{
						if (!base.readCorrectly())
						{
							return stb.ToString();
						}
						stb2.Set(base.last_str);
						if (this.VarCon != null)
						{
							this.VarCon.replace_simple(stb2, false, false);
						}
						stb.Append(stb2, "\n", 0, -1);
					}
				}
				text = stb.ToString();
			}
			return text;
		}

		public virtual bool open(bool is_init)
		{
			return true;
		}

		public virtual bool close(bool is_last)
		{
			return true;
		}

		public override bool tNote(string t, bool is_error = false)
		{
			if (t == null)
			{
				return true;
			}
			base.tNote(t, is_error);
			X.dl(" event: " + this.name, null, is_error, false);
			return true;
		}

		public void statePreserve(EV.ST s)
		{
			X.push<EV.ST>(ref this.Apreserve_states, s, -1);
		}

		public bool hasPreservedState()
		{
			return this.Apreserve_states.Length != 0;
		}

		public EV.ST popPreservedState()
		{
			return X.shift<EV.ST>(ref this.Apreserve_states, 1);
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				this.name,
				" :",
				this.cur_line.ToString(),
				" <cmd:",
				base.cmd,
				">"
			});
		}

		protected override bool falseJumpEvaluate()
		{
			return base.falseJumpEvaluate();
		}

		public string name;

		public int start_line;

		public string[] Avariables;

		public EV.ST[] Apreserve_states;

		public bool no_init_load;

		public bool do_not_announce;
	}
}
