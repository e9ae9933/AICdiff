using System;
using m2d;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public class WanderingManager
	{
		public WanderingManager(NelM2DBase _M2D)
		{
			this.M2D = _M2D;
			this.AItm = new WanderingNPC[4];
			WanderingNPC[] aitm = this.AItm;
			int num = 0;
			WanderingNPC wanderingNPC = new WanderingNPC(this, WanderingManager.TYPE.NIG, "Nightingale", "NIG0", 4f, 8f, 4f, 0.33f, "sub_t", "harmonic");
			wanderingNPC.FD_CalcHasBell = (WanderingNPC Npc) => !Npc.alreadyMeet() || this.M2D.IMNG.getInventory().getCount(NelItem.GetById("nightingale_bell", false), -1) > 0;
			wanderingNPC.FD_CreateEventMover = (string name, M2EventContainer EVC) => EVC.CreateAndAssignT<MvNightingale>(name, true);
			wanderingNPC.FD_CreateEventMoverAfter = delegate(M2EventItem Mv)
			{
				(Mv as MvNightingale).appearAfter();
			};
			aitm[num] = wanderingNPC;
			this.AItm[1] = new WanderingNPC(this, WanderingManager.TYPE.COF, "CoffeeMaker", "COF0", 6f, 10f, 40f, 0.33f, "sub_cm", "stand");
			this.AItm[2] = new WanderingNPC(this, WanderingManager.TYPE.TLD, "TildeNpc", "TLD_N0", 1f, 2f, 9.5f, 0.8f, "sub_d", "camp");
			WanderingNPC[] aitm2 = this.AItm;
			int num2 = 3;
			WanderingNPC wanderingNPC2 = new WanderingNPC(this, WanderingManager.TYPE.PUP, "PuppetNpc", "PUP0", 1f, 6f, 13f, 0.65f, null, "stand");
			wanderingNPC2.FD_CreateEventMover = delegate(string name, M2EventContainer EVC)
			{
				if (SCN.isPuppetWNpcDefeated())
				{
					return null;
				}
				return EVC.CreateAndAssignT<MvNelNNEAListener>(name, false).prepareCreateNNEA<NelNNpcPuppet>("sub_golem_npc", false);
			};
			wanderingNPC2.FD_overrideCalcableLength = new Func<float, float>(PuppetRevenge.WdrOverrideCalcableLength);
			aitm2[num2] = wanderingNPC2;
		}

		public void newGame()
		{
			this.flush();
		}

		public void flush()
		{
			for (int i = 3; i >= 0; i--)
			{
				this.AItm[i].flush();
			}
		}

		public WanderingNPC Get(WanderingManager.TYPE type)
		{
			return this.AItm[(int)type];
		}

		public WanderingNPC getNightingale()
		{
			return this.Get(WanderingManager.TYPE.NIG);
		}

		public WanderingNPC getPuppet()
		{
			return this.Get(WanderingManager.TYPE.PUP);
		}

		public bool isNightingaleHere(Map2d Mp)
		{
			return this.getNightingale().isHere(Mp);
		}

		public void fineFirstPosition(bool force = false)
		{
			for (int i = 3; i >= 0; i--)
			{
				this.AItm[i].fineFirstPosition(force);
			}
		}

		public void walkAround(bool force = false)
		{
			if (this.M2D.curMap == null)
			{
				return;
			}
			for (int i = 3; i >= 0; i--)
			{
				WanderingNPC wanderingNPC = this.AItm[i];
				if (force || wanderingNPC.walk_around_flag)
				{
					wanderingNPC.walkAround();
				}
			}
		}

		public void setWalkAroundFlag(bool allow_not_here = false)
		{
			for (int i = 3; i >= 0; i--)
			{
				WanderingNPC wanderingNPC = this.AItm[i];
				if ((allow_not_here || !wanderingNPC.isHere(this.M2D.curMap)) && SCN.isWNpcWalkAroundEnable(this.M2D, wanderingNPC.type))
				{
					wanderingNPC.walk_around_flag = true;
				}
			}
		}

		public void setFineBellFlag()
		{
			for (int i = 3; i >= 0; i--)
			{
				this.AItm[i].setFineBellFlag();
			}
		}

		public void readBinaryFrom(ByteArray Ba, bool old_ver)
		{
			this.newGame();
			int num;
			if (old_ver)
			{
				num = 0;
			}
			else
			{
				num = Ba.readByte();
			}
			int num2;
			if (num == 0)
			{
				num2 = 1;
			}
			else
			{
				num2 = 4;
			}
			int num3 = num2;
			for (int i = 0; i < num3; i++)
			{
				this.AItm[i].readBinaryFrom(Ba, num);
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(1);
			int num = this.AItm.Length;
			for (int i = 0; i < num; i++)
			{
				this.AItm[i].writeBinaryTo(Ba);
			}
		}

		public bool isOtherNpcAppearHere(Map2d Mp, WanderingNPC From)
		{
			for (int i = this.AItm.Length - 1; i >= 0; i--)
			{
				WanderingNPC wanderingNPC = this.AItm[i];
				if (wanderingNPC != From && wanderingNPC.isHere(Mp))
				{
					return true;
				}
			}
			return false;
		}

		public void blurDecidedOtherFrom(Map2d Mp, WanderingNPC From)
		{
			for (int i = this.AItm.Length - 1; i >= 0; i--)
			{
				WanderingNPC wanderingNPC = this.AItm[i];
				if (wanderingNPC != From && wanderingNPC.isHere(Mp))
				{
					wanderingNPC.blurDecided(Mp.key);
				}
			}
		}

		public bool alreadyMeet(WanderingManager.TYPE type)
		{
			return this.Get(type).alreadyMeet();
		}

		public void debugFlush(WanderingManager.TYPE type, string[] Acmd)
		{
			WanderingNPC wanderingNPC = this.Get(type);
			wanderingNPC.flush();
			wanderingNPC.appear_ratio = 1f;
			if (Acmd.Length > 2)
			{
				float num = global::XX.X.Nm(Acmd[1], 0f, false);
				float num2 = global::XX.X.Nm(Acmd[2], 0f, false);
				wanderingNPC.setTo(num, num2);
				global::XX.X.dl(string.Concat(new string[]
				{
					wanderingNPC.type.ToString(),
					" 位置を",
					num.ToString(),
					",",
					num2.ToString(),
					"に設定"
				}), null, false, false);
				return;
			}
			if (Acmd.Length == 2)
			{
				wanderingNPC.setToHere();
				global::XX.X.dl(wanderingNPC.type.ToString() + " 位置を現在位置に設定", null, false, false);
				return;
			}
			global::XX.X.dl(wanderingNPC.type.ToString() + " キャッシュクリア", null, false, false);
		}

		public readonly NelM2DBase M2D;

		private WanderingNPC[] AItm;

		public enum TYPE
		{
			NIG,
			COF,
			TLD,
			PUP,
			_MAX,
			NIGHTINGALE = 0,
			COFFEEMAKER,
			TILDE,
			PUPPET,
			_MAX_V0 = 1,
			_MAX_V1 = 4
		}
	}
}
