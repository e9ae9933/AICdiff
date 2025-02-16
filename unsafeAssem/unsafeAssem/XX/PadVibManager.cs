using System;
using Better;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Haptics;

namespace XX
{
	public class PadVibManager : RBase<PadVibManager.PadVibItem>, IRunAndDestroy
	{
		public PadVibManager()
			: base(5, true, false, false)
		{
			IN.addRunner(this);
			IN.getCurrentKeyAssignObject().addSwitchedDeviceFunc(new Action(this.finePad));
			this.reloadVibInfoScript();
			this.finePad();
		}

		public override PadVibManager.PadVibItem Create()
		{
			return new PadVibManager.PadVibItem(this);
		}

		public override void clear()
		{
			InputSystem.ResetHaptics();
			if (this.Pad != null)
			{
				this.Pad.SetMotorSpeeds(0f, 0f);
			}
			this.pre_left = (this.pre_right = 0f);
			base.clear();
		}

		public void clear(PadVibManager.VIB vib)
		{
			for (int i = base.Length - 1; i >= 0; i--)
			{
				if ((this.AItems[i].type & vib) != (PadVibManager.VIB)0)
				{
					base.clearAt(i);
				}
			}
		}

		public void finePad()
		{
			this.clear();
			this.Pad = Gamepad.current;
			InputSystem.ResumeHaptics();
		}

		public void reloadVibInfoScript()
		{
			if (this.OVib == null)
			{
				this.OVib = new BDic<string, PadVibManager.VibInfo>();
			}
			else
			{
				this.OVib.Clear();
			}
			CsvReader csvReader = new CsvReader(NKT.readStreamingText("_vibration.csv", false), CsvReader.RegSpace, false);
			PadVibManager.VIB vib = (PadVibManager.VIB)0;
			while (csvReader.read())
			{
				if (csvReader.cmd == "%CATEG")
				{
					vib = (PadVibManager.VIB)0;
					for (int i = 1; i < csvReader.clength; i++)
					{
						PadVibManager.VIB vib2;
						if (Enum.TryParse<PadVibManager.VIB>(csvReader.getIndex(i), true, out vib2))
						{
							vib |= vib2;
						}
					}
				}
				else
				{
					this.OVib[csvReader.cmd] = new PadVibManager.VibInfo(vib, csvReader.Nm(1, 0f), csvReader.Nm(2, 0f) * 0.01f);
				}
			}
		}

		public override void destruct()
		{
			this.clear();
			try
			{
				IN.remRunner(this);
			}
			catch
			{
			}
		}

		public PadVibManager Add(string vib_key, float _level = 1f)
		{
			if (this.base_level <= 0f)
			{
				return this;
			}
			PadVibManager.VibInfo vibInfo;
			if (!this.OVib.TryGetValue(vib_key, out vibInfo))
			{
				X.de("不明なvib: " + vib_key, null);
				return this;
			}
			vibInfo.level *= _level;
			for (int i = base.Length - 1; i >= 0; i--)
			{
				PadVibManager.PadVibItem padVibItem = this.AItems[i];
				if (padVibItem.type == vibInfo.type && padVibItem.level == vibInfo.level)
				{
					padVibItem.t = X.Mx(padVibItem.t, vibInfo.time);
					return this;
				}
			}
			base.Pop(16).Init(vibInfo.type, vibInfo.time, vibInfo.level);
			return this;
		}

		public override bool run(float fcnt)
		{
			float num = this.pre_left;
			float num2 = this.pre_right;
			this.pre_left = (this.pre_right = 0f);
			base.run(fcnt);
			this.pre_left *= this.base_level;
			this.pre_right *= this.base_level;
			if ((this.pre_left != num || this.pre_right != num2) && this.Pad != null)
			{
				this.Pad.SetMotorSpeeds(this.pre_left, this.pre_right);
			}
			return true;
		}

		public override string ToString()
		{
			return "<PadManager>";
		}

		private BDic<string, PadVibManager.VibInfo> OVib;

		private float pre_left;

		private float pre_right;

		public float base_level = 1f;

		private IDualMotorRumble Pad;

		public class PadVibItem : IRunAndDestroy
		{
			public PadVibItem(PadVibManager _Con)
			{
				this.Con = _Con;
			}

			public void Init(PadVibManager.VIB _type, float _t, float _level)
			{
				this.type = _type;
				this.t = _t;
				this.level = _level;
			}

			public void destruct()
			{
			}

			public bool run(float fcnt)
			{
				this.t -= fcnt;
				if (this.t <= 0f)
				{
					return false;
				}
				if ((this.type & PadVibManager.VIB._PAUSE) != (PadVibManager.VIB)0)
				{
					return true;
				}
				if ((this.type & PadVibManager.VIB.R) != (PadVibManager.VIB)0)
				{
					this.Con.pre_right = X.Mx(this.Con.pre_right, this.level);
				}
				else
				{
					this.Con.pre_left = X.Mx(this.Con.pre_left, this.level);
				}
				return true;
			}

			public readonly PadVibManager Con;

			public PadVibManager.VIB type;

			public float t;

			public float level;
		}

		[Flags]
		public enum VIB
		{
			R = 1,
			IN_GAME = 2,
			ACT = 4,
			DAMAGE = 8,
			_PAUSE = 128
		}

		private struct VibInfo
		{
			public VibInfo(PadVibManager.VIB _type, float _time, float _level)
			{
				this.type = _type;
				this.time = _time;
				this.level = _level;
			}

			public PadVibManager.VIB type;

			public float time;

			public float level;
		}
	}
}
