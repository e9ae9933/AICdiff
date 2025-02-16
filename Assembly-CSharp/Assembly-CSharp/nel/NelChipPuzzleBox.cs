using System;
using System.Collections.Generic;
using m2d;
using XX;

namespace nel
{
	public class NelChipPuzzleBox : NelChipEvent, IPuzzRevertable, IPuzzActivationListener
	{
		public NelChipPuzzleBox(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base("BOX_", _Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			this.FD_FnEdBind = new M2DrawBinder.FnEffectBind(this.fnDrawLight);
			this.FD_DrawFocusBorder = new M2DrawBinder.FnEffectBind(this.fnDrawFocusBorder);
		}

		public override void initAction(bool normal_map)
		{
			if (!this.assigned)
			{
				List<M2Puts> list = this.Mp.findPutsFilling(this.mapx, this.mapy, delegate(M2Puts Cp, List<M2Puts> _APt)
				{
					if (Cp is NelChipPuzzleBox && Cp.Lay == this.Lay)
					{
						(Cp as NelChipPuzzleBox).assigned = true;
						return true;
					}
					return false;
				}, null, null);
				if (list == null || list.Count == 0)
				{
					return;
				}
				this.ACmem = new NelChipPuzzleBox.ConnectMem[list.Count];
				float num = 0f;
				float num2 = 0f;
				for (int i = list.Count - 1; i >= 0; i--)
				{
					NelChipPuzzleBox.ConnectMem connectMem = (this.ACmem[i] = new NelChipPuzzleBox.ConnectMem(list[i] as NelChipPuzzleBox));
					num += connectMem.mapcx;
					num2 += connectMem.mapcy;
				}
				this.Mv = this.Mp.createMover<M2BoxMover>("CM-" + base.unique_key, num / (float)list.Count, num2 / (float)list.Count, false, false);
				this.Mv.Parent = this;
				this.Mv.FnColliderBlockCanStand = new Func<int, bool>(this.FnColliderBlockCanStand);
				this.Mv.collider_margin = -0.1f;
				this.Mv.attractChips(list, true);
				this.Mv.attractedAfter();
				this.ManageArea = PUZ.IT.isBelongTo(this);
				if (this.ManageArea != null)
				{
					PUZ.IT.addRevertableObject(this, false);
				}
				this.fineTriggerVisible();
			}
			else
			{
				this.ACmem = null;
			}
			base.initAction(normal_map);
		}

		private bool FnColliderBlockCanStand(int c)
		{
			return c != (int)this.Img.Aconfig[0];
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			if (this.ACmem != null)
			{
				this.ACmem = null;
				PUZ.IT.remRevertableObject(this);
			}
			this.Ed = this.Mp.remED(this.Ed);
			this.EdFocusBorder = this.Mp.remED(this.EdFocusBorder);
			this.assigned = false;
			if (this.Mv != null)
			{
				this.Mv.close_action_destruction = true;
				this.Mp.removeMover(this.Mv);
				this.Mv = null;
			}
		}

		public override bool canSetEvent()
		{
			return this.ACmem != null;
		}

		public override bool EvtM2Reload(Map2d Mp)
		{
			if (!this.canSetEvent())
			{
				return false;
			}
			base.EvtM2Reload(Mp);
			this.Ev.talk_with_magic_key = true;
			if (this.Mv != null)
			{
				this.Mv.resetEventPosition(this.Ev, true);
			}
			if (this.Ev is NelChipPuzzleBox.M2EventItemBox)
			{
				(this.Ev as NelChipPuzzleBox.M2EventItemBox).Parent = this;
			}
			this.Ev.assign("CHECK", this.getEventContent(), false);
			this.Ev.check_desc_name = "EV_access_carry_block";
			return true;
		}

		protected override M2EventItem fnMakeEventT(Map2d Mp)
		{
			return Mp.getEventContainer().CreateAndAssignT<NelChipPuzzleBox.M2EventItemBox>(base.event_key, false);
		}

		public void run()
		{
			bool event_trigger_visible = this.event_trigger_visible;
		}

		public override void activate()
		{
			if (this.Mv != null)
			{
				this.Mv.activate(true);
				for (int i = this.ACmem.Length - 1; i >= 0; i--)
				{
					this.ACmem[i].Cp.activateToDrawer();
				}
				this.fineTriggerVisible();
				if (this.Ed == null)
				{
					this.Ed = this.Mp.setED("Box", this.FD_FnEdBind, 0f);
				}
				else
				{
					this.Ed.t = 0f;
				}
				this.EyePtc("puzz_box_activate_eye");
			}
		}

		public override void deactivate()
		{
			if (this.Mv != null)
			{
				this.Mv.deactivate(true);
				for (int i = this.ACmem.Length - 1; i >= 0; i--)
				{
					this.ACmem[i].Cp.deactivateToDrawer();
				}
				if (this.Ed != null)
				{
					this.Ed.t = 0f;
				}
			}
		}

		public bool event_trigger_visible
		{
			get
			{
				return this.Ev != null && this.Ev.trigger_visible;
			}
		}

		public bool fineTriggerVisible()
		{
			if (this.Ev == null || this.Mv == null)
			{
				return false;
			}
			bool flag = !this.Mv.beingCarriedByPr() && this.Ed == null;
			if (flag && this.ManageArea != null)
			{
				flag = PUZ.IT.barrier_active;
			}
			return this.Ev.trigger_visible = flag;
		}

		public void makeSnapShot(PuzzSnapShot.RevertItem Rvi)
		{
			if (this.Mv == null)
			{
				return;
			}
			Rvi.x = this.Mv.x;
			Rvi.y = this.Mv.y;
		}

		public void puzzleRevert(PuzzSnapShot.RevertItem Rvi)
		{
			if (this.Mv == null)
			{
				return;
			}
			this.Mv.getPhysic().killSpeedForce(true, true, true, false, false);
			this.Mv.getPhysic().clampSpeed(FOCTYPE.WALK, -1f, -1f, 1f);
			this.deactivate();
			if (X.LENGTHXYS(this.Mv.x, this.Mv.y, Rvi.x, Rvi.y) > 0.125f)
			{
				this.Mv.setTo(Rvi.x, Rvi.y);
				this.Mv.resetChipsDrawPosition(0f, 0f);
				this.EyePtc("puzz_box_revert");
			}
		}

		public void changePuzzleActivation(bool activated)
		{
			if (this.Ev == null)
			{
				return;
			}
			if (activated)
			{
				this.fineTriggerVisible();
				return;
			}
			this.Ev.trigger_visible = false;
			this.deactivate();
		}

		public void setBorderEd()
		{
			if (this.EdFocusBorder == null)
			{
				this.EdFocusBorder = this.Mp.setED("BoxBorder", this.FD_DrawFocusBorder, 0f);
			}
		}

		public void remBorderEd()
		{
			if (this.EdFocusBorder != null)
			{
				this.EdFocusBorder = this.Mp.remED(this.EdFocusBorder);
			}
		}

		public string getEventContent()
		{
			return NelChipPuzzleBox.getEventContentS(this);
		}

		public static string getEventContentS(M2Chip Cp)
		{
			string text;
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AR("STOP_LETTERBOX");
				stb.AR("VALOTIZE");
				stb.AR("TUTO_REM_ACTIVE_FLAG");
				stb.AR("DENY_SKIP");
				stb.Add("CHIP_ACTIVATE ", Cp.Lay.name).Add(" ", Cp.index, "").AR("");
				stb.AR("#MS_ % 'P[interact2stand] w15'");
				stb.AR("WAIT_MOVE");
				text = stb.ToString();
			}
			return text;
		}

		protected override M2CImgDrawer CreateDrawer(ref MeshDrawer Md, int lay, METACImg Meta, M2CImgDrawer Pre_Drawer)
		{
			if (lay <= 2)
			{
				return base.CreateDrawer(ref Md, lay, Meta, Pre_Drawer);
			}
			return null;
		}

		public bool fnDrawLight(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.Mv == null)
			{
				if (Ed == this.Ed)
				{
					this.Ed = null;
				}
				this.fineTriggerVisible();
				return false;
			}
			if (!this.Mv.drawLight(Ef, Ed, this.ACmem))
			{
				if (Ed == this.Ed)
				{
					this.Ed = null;
				}
				this.fineTriggerVisible();
				return false;
			}
			return true;
		}

		public bool fnDrawFocusBorder(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.Mv == null)
			{
				if (Ed == this.EdFocusBorder)
				{
					this.EdFocusBorder = null;
				}
				return false;
			}
			this.Mv.drawBorder(Ef, null, 1f);
			return true;
		}

		public void EyePtc(string ptcst_key)
		{
			if (this.Mv == null)
			{
				return;
			}
			for (int i = this.ACmem.Length - 1; i >= 0; i--)
			{
				NelChipPuzzleBox cp = this.ACmem[i].Cp;
				this.Mp.PtcSTsetVar("cx", (double)cp.mapcx).PtcSTsetVar("cy", (double)cp.mapcy).PtcST(ptcst_key, null, PTCThread.StFollow.NO_FOLLOW);
			}
		}

		public M2DrawBinder getDrawBinder()
		{
			return this.Ed;
		}

		public NelChipPuzzleBox.ConnectMem[] getConnects()
		{
			return this.ACmem;
		}

		private bool assigned;

		private NelChipPuzzleBox.ConnectMem[] ACmem;

		private M2BoxMover Mv;

		private M2DrawBinder Ed;

		private M2DrawBinder EdFocusBorder;

		public M2LpPuzzManageArea ManageArea;

		private readonly M2DrawBinder.FnEffectBind FD_FnEdBind;

		private readonly M2DrawBinder.FnEffectBind FD_DrawFocusBorder;

		public class ConnectMem
		{
			public ConnectMem(NelChipPuzzleBox _Cp)
			{
				this.Cp = _Cp;
				int[] dirs = this.Cp.getMeta().getDirs("puzzle_box", this.Cp.rotation, this.Cp.flip, 0);
				for (int i = dirs.Length - 1; i >= 0; i--)
				{
					this.dir_bits |= 1 << dirs[i];
				}
			}

			public float mapcx
			{
				get
				{
					return this.Cp.mapcx;
				}
			}

			public float mapcy
			{
				get
				{
					return this.Cp.mapcy;
				}
			}

			public readonly NelChipPuzzleBox Cp;

			public int dir_bits;
		}

		public class M2EventItemBox : M2EventItem
		{
			public override void FocusTalkable()
			{
				base.FocusTalkable();
				if (this.Parent != null)
				{
					this.Parent.setBorderEd();
				}
			}

			public override void BlurTalkable(bool event_init)
			{
				base.BlurTalkable(event_init);
				if (this.Parent != null)
				{
					this.Parent.remBorderEd();
				}
			}

			public NelChipPuzzleBox Parent;
		}
	}
}
