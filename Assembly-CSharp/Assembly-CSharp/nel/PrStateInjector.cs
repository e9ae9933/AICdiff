using System;
using System.Collections.Generic;

namespace nel
{
	public sealed class PrStateInjector : M2PrAssistant
	{
		public PrStateInjector(PR Pr, int capacity = 4)
			: base(Pr)
		{
			this.AItem = new List<PrStateInjector.InjItem>(capacity);
		}

		public void Add(PR.STATE state, int priority, byte available_only_normalstate, PrStateInjector.fnInitState FD_InitState)
		{
			int count = this.AItem.Count;
			int num = 0;
			while (num < count && this.AItem[num].priority >= priority)
			{
				num++;
			}
			this.AItem.Insert(num, new PrStateInjector.InjItem
			{
				priority = priority,
				state = state,
				available_only_normalstate = available_only_normalstate,
				FD_InitState = FD_InitState
			});
		}

		public void Rem(PR.STATE state, PrStateInjector.fnInitState FD_InitState)
		{
			for (int i = this.AItem.Count - 1; i >= 0; i--)
			{
				PrStateInjector.InjItem injItem = this.AItem[i];
				if (injItem.state == state && injItem.FD_InitState == FD_InitState)
				{
					this.AItem.RemoveAt(i);
				}
			}
		}

		public bool check(PR.STATE state, ref float t_state, bool execute_change_state)
		{
			PR.STATE state2;
			return this.check(state, ref t_state, execute_change_state, out state2);
		}

		public bool check(PR.STATE state, ref float t_state, bool execute_change_state, out PR.STATE target_state)
		{
			int count = this.AItem.Count;
			target_state = PR.STATE.NORMAL;
			bool flag = this.Pr.isInjectableNormalState(state);
			for (int i = 0; i < count; i++)
			{
				PrStateInjector.InjItem injItem = this.AItem[i];
				if (injItem.available_only_normalstate <= 0 || ((injItem.available_only_normalstate < 2 || state == PR.STATE.NORMAL) && (injItem.available_only_normalstate < 1 || flag)))
				{
					bool flag2 = execute_change_state;
					PR.STATE state2 = injItem.state;
					if (injItem.FD_InitState(state, ref t_state, ref flag2, ref state2))
					{
						if (flag2 && this.Pr.get_current_state() != state2)
						{
							this.Pr.changeState(state2);
						}
						target_state = state2;
						return true;
					}
				}
			}
			return false;
		}

		private readonly List<PrStateInjector.InjItem> AItem;

		public const int PRI_ABSORB = 200;

		public const int PRI_WORM_TRAPPED = 195;

		public const int PRI_EV_GACHA = 180;

		public const int PRI_WATER_CHOCKED = 175;

		public const int PRI_BURST = 170;

		public const int PRI_BURNED = 160;

		public const int PRI_ORGASM = 100;

		public const int PRI_LAYEGG = 150;

		public const int PRI_DEAD = 30;

		public const int PRI_SHIELD_BREAK = 30;

		public bool need_check_on_runpre;

		public delegate bool fnInitState(PR.STATE state, ref float t_state, ref bool execute_change_state, ref PR.STATE target_state);

		private struct InjItem
		{
			public int priority;

			public PR.STATE state;

			public byte available_only_normalstate;

			public PrStateInjector.fnInitState FD_InitState;
		}
	}
}
