using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2LpPuzzCarrier : NelLp, ISfListener, IPuzzRevertable, IPuzzActivationListener
	{
		public M2LpPuzzCarrier(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public override void initActionPre()
		{
			base.initActionPre();
			this.Meta = new META(this.comment);
			List<M2Puts> list = (this.ATargetPuts = this.Mp.getAllPointMetaPutsTo(this.mapx, this.mapy, this.mapw, this.maph, null, (M2Puts V, List<M2Puts> _List) => V.Lay == this.Lay && V.getMeta().Get("rail") == null && V.getMeta().GetI("no_carryable", 0, 0) == 0 && !V.active_removed));
			if (list == null || list.Count == 0)
			{
				X.de("Carrier の移動対象となるマップチップが見つかりませんでした " + this.key, null);
				this.ATargetPuts = null;
				return;
			}
			M2Puts.flagArrangeable<M2Puts>(this.ATargetPuts);
		}

		public override void initAction(bool normal_map)
		{
			if (this.Mv != null)
			{
				this.closeAction(true);
			}
			base.initAction(normal_map);
			this.sf_key = this.Meta.GetS("sf_key");
			int dirsI = this.Meta.getDirsI("aim", 0, false, 0, 2);
			this.ride_on = this.Meta.GetB("ride_on", false);
			this.loopback = this.Meta.GetB("loopback", false);
			this.off_turn = (this.Meta.GetB("off_turn", false) ? M2LpPuzzCarrier.OFF_TURN._ALL : M2LpPuzzCarrier.OFF_TURN.NOUSE) | (this.Meta.GetB("off_turn_ride", false) ? M2LpPuzzCarrier.OFF_TURN.RIDE : M2LpPuzzCarrier.OFF_TURN.NOUSE) | (this.Meta.GetB("off_turn_liner", false) ? M2LpPuzzCarrier.OFF_TURN.LINER : M2LpPuzzCarrier.OFF_TURN.NOUSE);
			this.activate_delay = this.Meta.GetI("activate_delay", 20, 0);
			this.activate_switch_id = this.Meta.GetI("activate_switch_id", -1, 0);
			this.press_irisout = this.Meta.GetB("press_irisout", false);
			this.press_stop_same_layer_carrier = this.Meta.GetB("press_stop_same_layer_carrier", false);
			int i = this.Meta.GetI("pressback", -1, 0);
			this.pressback = ((i < 0) ? this.loopback : (i != 0));
			bool b = this.Meta.GetB("spike", false);
			int num = this.Meta.GetI("pre_on", -1, 0);
			this.duration_for_one_rail = this.Meta.GetI("speed", 20, 0);
			this.stop_maxt = this.Meta.GetI("speed", 90, 1);
			this.ManageArea = PUZ.IT.isBelongTo(this);
			if (num == -1)
			{
				num = ((this.ride_on || this.ManageArea != null || TX.valid(this.sf_key)) ? 0 : 1);
			}
			this.pre_on = num > 0;
			string[] array = null;
			string[] array2 = this.Meta.Get("ptc_on_rail_stop");
			if (array2 != null)
			{
				int num2 = array2.Length;
				for (int j = 0; j < num2; j += 2)
				{
					int num3 = -1;
					string text;
					if (j == num2 - 1)
					{
						text = array2[j];
					}
					else
					{
						num3 = CAim.parseString(array2[j], 0);
						text = array2[j + 1];
					}
					if (!TX.noe(text))
					{
						if (array == null)
						{
							array = new string[4];
						}
						if (num3 < 0)
						{
							array = new string[] { text };
							break;
						}
						if (array.Length <= num3)
						{
							Array.Resize<string>(ref array, num3 + 1);
						}
						array[num3] = text;
					}
				}
			}
			if (this.ATargetPuts != null)
			{
				this.Mv = this.Mp.createMover<M2PuzzCarrierMover>("CM-" + base.unique_key, base.mapfocx, base.mapfocy, false, false);
				this.Mv.initLp(this, CarrierRailBlock.getRailRoad(this.Lay, (int)base.mapfocx, (int)base.mapfocy, dirsI, true), dirsI, array);
				this.Mv.attractChips(this.ATargetPuts, false);
				if (this.Meta.GetB("colorful", false))
				{
					this.Mv.colorful = true;
				}
				if (this.Mv.getRailBlock() != null)
				{
					this.fineSF(true);
					M2Phys physic = this.Mv.getPhysic();
					if (physic != null)
					{
						physic.carrying_no_collider_lock = b;
					}
					if (CFG.sp_use_uipic_press_gimmick)
					{
						base.nM2D.prepareSvTexture("damage_press", true);
						return;
					}
				}
				else
				{
					this.Mp.removeMover(this.Mv);
					this.Mv = null;
				}
			}
		}

		public Vector2 getMoverTranslatedMp()
		{
			if (this.Mv == null)
			{
				return Vector2.zero;
			}
			return this.Mv.getTranslatedMp();
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			if (this.Mv != null)
			{
				this.Mv.close_action_destruction = true;
				this.Mp.removeMover(this.Mv);
				this.Mv = null;
			}
		}

		public bool fineSF(bool play_effect = true)
		{
			if (this.sf_key != null)
			{
				this.sf_active = COOK.getSF(this.sf_key) != 0;
			}
			else
			{
				this.sf_active = true;
			}
			return !(this.Mv == null) && this.Mv.fineMovingState(play_effect);
		}

		public void changePuzzleActivation(bool activated)
		{
			this.fineSF(activated);
		}

		public void changePuzzleSwitchActivation(int id, bool activated)
		{
			if (id == this.activate_switch_id && this.Mv != null && this.switch_activated != activated)
			{
				this.switch_activated = activated;
				this.Mv.fineMovingState(true);
			}
		}

		public bool initEffect(bool activating, ref DRect RcEffect)
		{
			return false;
		}

		public void makeSnapShot(PuzzSnapShot.RevertItem Rvi)
		{
			if (this.Mv == null)
			{
				return;
			}
			this.Mv.makeSnapShot(Rvi);
		}

		public void puzzleRevert(PuzzSnapShot.RevertItem Rvi)
		{
			if (this.Mv == null)
			{
				return;
			}
			this.Mv.puzzleRevert(Rvi);
		}

		private string sf_key;

		public bool sf_active;

		private META Meta;

		public int activate_delay = 20;

		public bool ride_on;

		public bool loopback;

		public bool pressback;

		public bool press_irisout;

		public bool press_stop_same_layer_carrier;

		private int activate_switch_id;

		public M2LpPuzzCarrier.OFF_TURN off_turn;

		public bool pre_on;

		public int stop_maxt;

		public int duration_for_one_rail = 15;

		public bool switch_activated;

		private List<M2Puts> ATargetPuts;

		private M2PuzzCarrierMover Mv;

		public M2LpPuzzManageArea ManageArea;

		public enum OFF_TURN
		{
			NOUSE,
			RIDE,
			LINER,
			_ALL
		}
	}
}
