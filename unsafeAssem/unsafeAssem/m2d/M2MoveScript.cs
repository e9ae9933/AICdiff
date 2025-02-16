using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using evt;
using XX;

namespace m2d
{
	public class M2MoveScript
	{
		public M2MoveScript(M2Mover _Mv, string str = "")
		{
			this.Mv = _Mv;
			this.pre_float = this.Mv.floating;
			this.ASc = new List<M2MoveScriptItem>();
			if (str != "")
			{
				this.addScript(str);
			}
		}

		public M2MoveScript addScript(string str)
		{
			if (str == "" || this.Mv.Mp == null)
			{
				return this;
			}
			M2MoveScriptItem.initScript(this.Mv, false);
			while (str != "")
			{
				if (REG.match(str, M2MoveScript.RegQuit))
				{
					str = REG.rightContext;
					this.quitStack(false, false, true);
				}
				else if (REG.match(str, M2MoveScript.RegSubmit))
				{
					str = REG.rightContext;
					this.submit();
				}
				else
				{
					M2MoveScriptItem m2MoveScriptItem = M2MoveScriptItem.readScript(str);
					if (m2MoveScriptItem != null)
					{
						this.ASc.Add(m2MoveScriptItem);
					}
					str = M2MoveScriptItem.next_script;
				}
			}
			M2MoveScriptItem.endScript();
			this.run(true);
			return this;
		}

		public bool run(bool only_notime = false)
		{
			int num = this.ASc.Count;
			if (num == 0)
			{
				return false;
			}
			int i = 0;
			bool flag = false;
			M2MoveScriptItem.initScript(this.Mv, false);
			int num2 = EV.isWaitMoveSkipping();
			float num3 = Map2d.TS * (float)(1 + ((num2 == 0) ? 0 : X.Mx(4, num2)));
			while (i < num)
			{
				if (only_notime && this.ASc[i].hasTime())
				{
					M2MoveScriptItem.endScript();
					return true;
				}
				if (i != 0 && !this.ASc[i].multiply_command)
				{
					break;
				}
				if (this.ASc[i].run(this, this.Mv, num3))
				{
					this.ASc.RemoveAt(i);
					num--;
				}
				else
				{
					flag = true;
					i++;
					if (i < num)
					{
						bool multiply_command = this.ASc[i].multiply_command;
					}
				}
			}
			M2MoveScriptItem.endScript();
			return flag;
		}

		public void savePreFloat()
		{
			this.pre_float = this.Mv.floating;
		}

		public void submit()
		{
			int count = this.ASc.Count;
			for (int i = 0; i < count; i++)
			{
				this.ASc[i].submit(this, this.Mv);
			}
			this.ASc.Clear();
		}

		public bool quitStack(bool run_once = false, bool reset_values = true, bool run_in_adding = false)
		{
			int count = this.ASc.Count;
			if (!run_in_adding)
			{
				M2MoveScriptItem.initScript(this.Mv, true);
			}
			if (count > 0)
			{
				if (run_once)
				{
					for (int i = 0; i < count; i++)
					{
						this.ASc[i].quit(this, this.Mv);
					}
				}
				this.ASc.Clear();
			}
			if (reset_values)
			{
				this.addScript(".");
				this.run(false);
			}
			else
			{
				this.Mv.fineDrawPosition();
				this.Mv.Mp.assignManpu("", this.Mv, 0);
			}
			if (!run_in_adding)
			{
				M2MoveScriptItem.endScript();
			}
			return true;
		}

		public bool isWalking()
		{
			return this.isActive() && this.ASc[0].TypeIs(M2MoveScriptItem.TYPE.MOVE);
		}

		public bool isActive()
		{
			return this.ASc.Count >= 1;
		}

		private readonly M2Mover Mv;

		private readonly List<M2MoveScriptItem> ASc;

		public bool pre_float;

		public float ms_timescale = 1f;

		public bool lock_remove_ms;

		private static readonly Regex RegSubmit = new Regex("^ *\\= *");

		private static readonly Regex RegQuit = new Regex("^ *[Qq] *");
	}
}
