using System;
using XX;

namespace nel
{
	public class M2CastableEventOther : M2CastableEventBase
	{
		protected override MGKIND get_magic_kind()
		{
			return this.target_mgkind;
		}

		public override string get_voice_family_name()
		{
			return null;
		}

		public override string get_parent_name()
		{
			return this.target_mover_key;
		}

		public override void EvtRead(StringHolder rER)
		{
			string _ = rER._2;
			if (_ != null)
			{
				if (!(_ == "CAST"))
				{
					if (!(_ == "CAST_SCALE"))
					{
						return;
					}
					this.casting_time_scale = rER.Nm(3, this.casting_time_scale);
				}
				else
				{
					if (rER.clength < 4)
					{
						base.abortMagic();
						return;
					}
					MGKIND mgkind;
					if (!FEnum<MGKIND>.TryParse(rER._3, out mgkind, true))
					{
						rER.tError("不明なMGKIND: " + mgkind.ToString());
						return;
					}
					this.target_mgkind = mgkind;
					base.initChant();
					return;
				}
			}
		}

		public string target_mover_key;

		private MGKIND target_mgkind = MGKIND.FIREBALL;
	}
}
