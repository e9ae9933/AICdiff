using System;
using evt;
using nel.mgm.dojo;
using nel.mgm.mgm_ttr;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel.mgm
{
	public class MgmScoreHolder
	{
		public MgmScoreHolder()
		{
			this.Dojo = new DjSaveData();
			this.AHld = new MgmScoreHolder.SHld[1];
			this.AHld[0] = new MgmScoreHolder.SHld(1).Init(0, 0, "ml");
			this.PHldTTR = new PrefsHolderInner();
		}

		public void Clear()
		{
			this.Dojo.clear();
			this.PHldTTR.DeleteAll();
			for (int i = this.AHld.Length - 1; i >= 0; i--)
			{
				this.AHld[i].Clear();
			}
		}

		public bool UpdateScore(MGMSCORE mgs, int score, int id = 0)
		{
			MgmScoreHolder.SHld shld = this.AHld[(int)mgs];
			if (id == 0)
			{
				shld.play_count += 1U;
			}
			return shld[id].UpdateScore(score);
		}

		public static int EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			if (cmd != null)
			{
				if (cmd == "MG_DOJO")
				{
					return MgmDojo.EvtCacheReadS(ER, cmd, rER);
				}
				if (cmd == "MGM_EGGRMV")
				{
					return MgmEggRemove.EvtCacheReadS(ER, cmd, rER);
				}
				if (cmd == "MGM_4ASCEND")
				{
					NelTTRBase.LoadInitTTR(rER);
					return 0;
				}
			}
			return 0;
		}

		public static bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			string cmd = rER.cmd;
			if (cmd != null)
			{
				if (cmd == "MG_DOJO")
				{
					return MgmDojo.EvtReadS(ER, rER, skipping);
				}
				if (!(cmd == "MGM_UI_ACTIVATE") && !(cmd == "MGM_UI_ACTIVATE_AFT"))
				{
					if (cmd == "MGM_UI_DEACTIVATE")
					{
						UiMgmScore.deactivateS();
						return true;
					}
				}
				else
				{
					MGMSCORE mgmscore;
					if (!FEnum<MGMSCORE>.TryParse(rER._1, out mgmscore, true))
					{
						ER.tError("不明なMGMSCORE: " + rER._1);
						return true;
					}
					UiMgmScore uiMgmScore = UiMgmScore.createInstance();
					if (rER.cmd == "MGM_UI_ACTIVATE")
					{
						uiMgmScore.Init(UiMgmScore.STATE.PRE_GAME, mgmscore);
					}
					else if (rER.cmd == "MGM_UI_ACTIVATE_AFT")
					{
						bool flag = false;
						for (int i = 2; i < rER.clength; i++)
						{
							flag = COOK.Mgm.UpdateScore(mgmscore, rER.Int(i, 0), i - 2) || flag;
						}
						uiMgmScore.InitAftGame(flag, mgmscore);
						for (int j = 2; j < rER.clength; j++)
						{
							uiMgmScore.CurScore(rER.Int(j, 0));
						}
					}
					return true;
				}
			}
			return false;
		}

		public void readFromBytes(ByteArray Ba)
		{
			this.Clear();
			int num = Ba.readByte();
			this.Dojo.readFromBytes(Ba);
			int num2 = Ba.readByte();
			for (int i = 0; i < num2; i++)
			{
				MgmScoreHolder.SHld.readFromBytes((i < this.AHld.Length) ? this.AHld[i] : null, Ba);
			}
			if (num >= 1)
			{
				this.PHldTTR.readFromBytes(Ba);
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(1);
			this.Dojo.writeBinaryTo(Ba);
			int num = this.AHld.Length;
			Ba.writeByte(num);
			for (int i = 0; i < num; i++)
			{
				this.AHld[i].writeBinaryTo(Ba);
			}
			this.PHldTTR.writeToBytes(Ba);
		}

		public readonly MgmScoreHolder.SHld[] AHld;

		public readonly DjSaveData Dojo;

		public readonly PrefsHolderInner PHldTTR;

		public class SHld
		{
			public SHld(int capacity = 1)
			{
				this.AEntry = new ScoreEntry[capacity];
			}

			public MgmScoreHolder.SHld Init(int entry_id, int default_score, string _suffix)
			{
				this.AEntry[entry_id] = new ScoreEntry(default_score, _suffix);
				return this;
			}

			public void Clear()
			{
				this.play_count = 0U;
				for (int i = this.Length - 1; i >= 0; i--)
				{
					this.AEntry[i].Clear();
				}
			}

			public int Length
			{
				get
				{
					return this.AEntry.Length;
				}
			}

			public ScoreEntry this[int i]
			{
				get
				{
					if (i >= this.Length)
					{
						return null;
					}
					return this.AEntry[i];
				}
			}

			public static void readFromBytes(MgmScoreHolder.SHld Target, ByteArray Ba)
			{
				uint num = Ba.readUInt();
				int num2 = Ba.readByte();
				for (int i = 0; i < num2; i++)
				{
					ScoreEntry.readFromBytes((Target != null) ? Target[i] : null, Ba);
				}
				if (Target != null)
				{
					Target.play_count = num;
				}
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				Ba.writeUInt(this.play_count);
				Ba.writeByte(this.Length);
				for (int i = 0; i < this.Length; i++)
				{
					this.AEntry[i].writeBinaryTo(Ba);
				}
			}

			public readonly ScoreEntry[] AEntry;

			public uint play_count;
		}
	}
}
