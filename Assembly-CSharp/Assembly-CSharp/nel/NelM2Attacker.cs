using System;
using m2d;

namespace nel
{
	public interface NelM2Attacker
	{
		int applyDamage(NelAttackInfo Atk, bool force);

		void applyGasDamage(MistManager.MistKind K, MistAttackInfo Atk);

		float getMpDesireRatio(int add_mp);

		float getMpDesireMaxValue();

		void addMpFromMana(M2Mana Mn, float val);

		bool canGetMana(M2Mana Mn, bool is_focus);

		bool initAbsorb(NelAttackInfo Atk, NelM2Attacker MvAnother = null, AbsorbManager Abs = null, bool penetrate = false);

		bool releaseAbsorb(AbsorbManager Abm);

		int getAbsorbWeight();

		void getMouthPosition(out float x, out float y);

		bool canApplyMistDamage();

		bool canApplySer(SER ser);

		M2Ser getSer();
	}
}
