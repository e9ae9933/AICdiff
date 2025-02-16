using System;
using m2d;

namespace nel
{
	public class NelNEvBarten : MvNelNNEAListener.NelNNpcEventAssign
	{
		public override void Awake()
		{
			base.Awake();
			this.Anm.normalrender_header = "normal";
		}

		public override void appear(Map2d _Mp)
		{
			base.appear(_Mp);
			this.Phy.addLockGravity("EVENT", 0f, -1f);
			this.Anm.setPose("bar_washing", -1);
			this.Anm.scale_shuffle01 = 0.005f;
			this.Anm.base_rotate_shuffle360 = 0.2f;
		}

		protected override NOD.BasicData initializeFirst(string npc_key)
		{
			return base.initializeFirst(npc_key ?? "NPC_ENEMY_BARTEN");
		}

		public override float applyHpDamageRatio(AttackInfo Atk)
		{
			if (PuppetRevenge.first_meet || !Map2d.can_handle)
			{
				return 0f;
			}
			return base.applyHpDamageRatio(Atk);
		}

		public override bool initDeath()
		{
			return false;
		}

		protected override bool fnSleepConsider(NAI Nai)
		{
			return true;
		}
	}
}
