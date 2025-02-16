using System;
using System.Text.RegularExpressions;

namespace XX
{
	public class PTCThreadRunner : RBase<PTCThread>
	{
		public PTCThreadRunner(IEffectSetter _Ef)
			: base(16, false, false, false)
		{
			this.Ef = _Ef;
			this.cur_reload_count = EfParticleManager.reload_count;
			if (PTCThreadRunner.PreP == null)
			{
				PTCThreadRunner.PreP = new VariableP(16);
			}
		}

		public override PTCThread Create()
		{
			return new PTCThread(this);
		}

		public void killPtc(string key, IEfPInteractale Listener)
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				PTCThread ptcthread = this.AItems[i];
				if (ptcthread.key == key && (Listener == null || ptcthread.Listener == Listener))
				{
					ptcthread.destruct();
				}
			}
		}

		public PTCThread getRunningSetter(string key, IEfPInteractale Listener = null)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				PTCThread ptcthread = this.AItems[i];
				if (ptcthread.key == key && (Listener == null || ptcthread.Listener == Listener))
				{
					return ptcthread;
				}
			}
			return null;
		}

		public void runSetter(float fcnt = 1f)
		{
			if (this.cur_reload_count < EfParticleManager.reload_count)
			{
				this.ReloadScript();
			}
			if (this.LEN > 0)
			{
				this.run(fcnt);
			}
		}

		private void ReloadScript()
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				PTCThread ptcthread = this.AItems[i];
				if (ptcthread != null)
				{
					ptcthread.ReloadScript();
				}
			}
			this.cur_reload_count = EfParticleManager.reload_count;
		}

		public static void PreVar(string key, double f)
		{
			if (PTCThreadRunner.PreP == null)
			{
				PTCThreadRunner.PreP = new VariableP(16);
			}
			PTCThreadRunner.PreP.Add(key, f);
		}

		public static void PreVar(string key, string v)
		{
			if (PTCThreadRunner.PreP == null)
			{
				PTCThreadRunner.PreP = new VariableP(16);
			}
			PTCThreadRunner.PreP.AddStringItem(key, v);
		}

		public static void PreVar(StringHolder rER, int start = 1)
		{
			PTCThreadRunner.PreVar(rER.slice_join(start, ";", ""));
		}

		public static void PreVar(string t)
		{
			if (PTCThreadRunner.PreP == null)
			{
				PTCThreadRunner.PreP = new VariableP(16);
			}
			string[] array = TX.split(t, new Regex("[\\n;]"));
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				if (REG.match(array[i], REG.RegValueInput))
				{
					PTCThreadRunner.PreP.Add(REG.R1, REG.R3);
				}
			}
		}

		public static void clearVars()
		{
			if (PTCThreadRunner.PreP != null)
			{
				PTCThreadRunner.PreP.Clear();
			}
		}

		public PTCThread makeST(string key, IEfPInteractale Listener = null, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW, VariableP InputVar = null)
		{
			if (key != null)
			{
				EfSetterP setterScript = EfParticleManager.GetSetterScript(key);
				if (setterScript != null)
				{
					PTCThread ptcthread = this.makeSTPre(InputVar);
					if (ptcthread.Set(setterScript, Listener, follow).run(1f))
					{
						this.LEN++;
						return ptcthread;
					}
					return null;
				}
				else
				{
					PTCThreadRunner.PreP.Clear();
				}
			}
			else
			{
				PTCThreadRunner.PreP.Clear();
			}
			return null;
		}

		protected virtual PTCThread makeSTPre(VariableP InputVar)
		{
			if (this.LEN >= this.AItems.Length)
			{
				Array.Resize<PTCThread>(ref this.AItems, this.LEN + 8);
			}
			PTCThread ptcthread = this.AItems[this.LEN];
			if (ptcthread == null)
			{
				ptcthread = (this.AItems[this.LEN] = this.Create());
			}
			ptcthread.VPClear();
			ptcthread.PreDefine(PTCThreadRunner.PreP);
			if (InputVar != null)
			{
				ptcthread.PreDefine(InputVar);
			}
			return ptcthread;
		}

		public void removeThread(string _key)
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				PTCThread ptcthread = this.AItems[i];
				if (ptcthread.Target != null && ptcthread.Target.key == _key)
				{
					ptcthread.destruct();
				}
			}
		}

		public bool hasDefinedVar()
		{
			return PTCThreadRunner.PreP.hasDefinedVar();
		}

		public IEffectSetter Ef;

		private int cur_reload_count;

		private static VariableP PreP;
	}
}
