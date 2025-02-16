using System;
using m2d;

namespace nel
{
	public class NelNNpcPuppet : MvNelNNEAListener.NelNNpcEventAssign
	{
		public override void Awake()
		{
			base.Awake();
			this.Anm.normalrender_header = "normal";
			this.Anm.after_offset_y = 20f;
			this.killable = true;
		}

		public override void appear(Map2d _Mp)
		{
			base.appear(_Mp);
			this.throw_ray_magic_attack = false;
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
			PuppetRevenge.defeatShopper(this);
			return base.initDeath();
		}
	}
}
