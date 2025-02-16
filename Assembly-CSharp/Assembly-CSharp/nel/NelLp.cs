using System;
using m2d;
using nel.mgm.farm;
using XX;

namespace nel
{
	public class NelLp : M2LabelPoint
	{
		public NelLp(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public NelM2DBase nM2D
		{
			get
			{
				if (this.Mp == null)
				{
					return M2DBase.Instance as NelM2DBase;
				}
				return this.Mp.M2D as NelM2DBase;
			}
		}

		private const string head_LpBreakable = "Breakable";

		private const string head_LpBreakable_treasure_hunt = "THunt";

		private const string head_LpEffector = "Effector";

		private const string head_LpParticleDrawer = "Particle";

		private const string head_LpPuzzBarrier = "PuzzBarrier";

		private const string head_LpPuzzSwitchContainer = "SwitchContainer";

		public const string head_LpCheckPoint = "Check";

		public const string head_LpTreasure = "Treasure";

		public const string head_LpThunder = "Thunder";

		public const string head_LpMatoate = "PuzzMato";

		public const string head_LpSupply = "Supply";

		public const string head_LpExtender = "Extender";

		public const string head_LpCarrier = "Carrier";

		public const string head_LpWater = "Water";

		public const string head_LpWaterSlicer = "WaterSlicer";

		public const string head_LpTimer = "Timer";

		public const string head_LpWind = "Wind";

		public const string head_LpActivator = "Activator";

		public const string head_LpSpecialEvent = "Ev";

		public const string head_LpDrawBridgeContainer = "DrawBridgeContainer";

		public const string head_LpCheckDecline = "CheckDecline";

		public const string head_LpHame = "Hame";

		public const string head_LpMuseum = "Museum";

		public const string head_LpMuseumEvacuator = "MuseumEvac";

		public const string head_LpSacredMist = "SacredMist";

		public static M2LabelPoint.FnCreateLp NelCreateLabelPoint = delegate(string cmd, M2MapLayer Lay)
		{
			if (cmd == null)
			{
				return new M2LabelPoint("", 0, Lay);
			}
			if (TX.headerIs(cmd, "Breakable", 0, '_', true))
			{
				return new M2LpBreakable(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "THunt", 0, '_', true))
			{
				return new M2LpTreasureHunt(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "Effector", 0, '_', true))
			{
				return new M2LpEffector(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "Particle", 0, '_', true))
			{
				return new M2LpParticleDrawer(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "PuzzBarrier", 0, '_', true))
			{
				return new M2LpPuzzManageArea(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "SwitchContainer", 0, '_', true))
			{
				return new M2LabelPointSwitchContainer(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "Treasure", 0, '_', true))
			{
				return new M2LpPuzzTreasure(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "Check", 0, '_', true))
			{
				return new M2LabelPointCheck(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "Thunder", 0, '_', true))
			{
				return new M2LabelPointThunder(cmd, 0, Lay, null);
			}
			if (TX.headerIs(cmd, "Supply", 0, '_', true))
			{
				return new M2LpItemSupplier(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "Extender", 0, '_', true))
			{
				return new M2LpPuzzExtender(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "Carrier", 0, '_', true))
			{
				return new M2LpPuzzCarrier(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "Hame", 0, '_', true))
			{
				return new M2LpHamehame(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "PuzzMato", 0, '_', true))
			{
				return new M2LpMatoate(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "WaterSlicer", 0, '_', true))
			{
				return new M2LpWaterSlicer(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "Water", 0, '_', true))
			{
				return new M2LpWater(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "Timer", 0, '_', true))
			{
				return new M2LpTimer(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "Wind", 0, '_', true))
			{
				return new M2LpWind(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "Activator", 0, '_', true))
			{
				return new M2LpActivator(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "DrawBridgeContainer", 0, '_', true))
			{
				return new M2LpDrawBridgeContainer(cmd, 0, Lay);
			}
			if (TX.headerIs(cmd, "Summon", 0, '_', false))
			{
				EnemySummoner enemySummoner = EnemySummoner.Get(EnemySummoner.Lp2smn(cmd), false);
				if (enemySummoner != null)
				{
					return new M2LpSummon(cmd, 0, Lay, enemySummoner);
				}
				X.de("Summon スクリプトが不明: " + REG.R1, null);
			}
			else
			{
				if (TX.headerIs(cmd, "CheckDecline", 0, '_', true))
				{
					return new M2LpCheckDecline(cmd, 0, Lay);
				}
				if (TX.isStart(cmd, "Exit", 0))
				{
					if (REG.match(cmd, M2LpMapTransfer.RegTransferDoorLp))
					{
						return new M2LpMapTransferDoor(cmd, 0, Lay);
					}
					if (REG.match(cmd, M2LpMapTransfer.RegTransferLp))
					{
						return new M2LpMapTransfer(REG.R1, REG.R2, Lay, cmd);
					}
					if (REG.match(cmd, M2LpMapTransfer.RegWorpLp))
					{
						return new M2LpMapTransferWarp(cmd, 0, Lay);
					}
				}
				else
				{
					if (cmd == "MGFarm")
					{
						return new M2LpMgmFarm(cmd, 0, Lay);
					}
					if (TX.headerIs(cmd, "Museum", 0, '_', true))
					{
						return new M2LpMuseum(cmd, 0, Lay);
					}
					if (TX.headerIs(cmd, "MuseumEvac", 0, '_', true))
					{
						return new M2LpMuseumEvacuator(cmd, 0, Lay);
					}
					if (TX.headerIs(cmd, "SacredMist", 0, '_', true))
					{
						return new M2LpSacredMistEffector(cmd, 0, Lay);
					}
				}
			}
			return M2LabelPoint.CreateLpDefault(cmd, Lay);
		};
	}
}
