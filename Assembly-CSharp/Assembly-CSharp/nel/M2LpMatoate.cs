using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class M2LpMatoate : NelLp, ISfListener, IPuzzRevertable, IPuzzActivationListener, IRunAndDestroy
	{
		public event M2LpMatoate.FnMatoateFinished EvtMatoateFinished = delegate(MatoateReader.MatoatePlayer<M2MatoateTarget> _Player, bool _successed)
		{
		};

		public M2LpMatoate(string _key, int _index, M2MapLayer _Lay)
			: base(_key, _index, _Lay)
		{
		}

		public override void initActionPre()
		{
			base.initActionPre();
			this.initReader(TX.slice(this.key, "PuzzMato_".Length));
		}

		public MatoateReader initReader(string reader_key)
		{
			this.Reader = MatoateReader.Get(reader_key);
			this.Mp.M2D.loadMaterialSnd("m2d_puzzle_timer");
			return this.Reader;
		}

		public override string comment
		{
			set
			{
				base.comment = value;
				this.Meta = null;
			}
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			if (this.Reader == null)
			{
				return;
			}
			if (this.Meta == null)
			{
				this.Meta = new META(this.comment);
			}
			this.ATimer = null;
			this.sf_key = this.Meta.GetS("sf_key");
			int i = this.Meta.GetI("pre_on", 1, 0);
			this.walk_in_activate = this.Meta.GetI("walk_in", -1000, 0);
			this.no_complete_effect = this.Meta.GetI("no_complete_effect", 0, 0) != 0;
			this.ManageArea = PUZ.IT.isBelongTo(this);
			this.pre_on = i > 0;
			List<M2LabelPoint> labelPointAll = this.Lay.getLabelPointAll((M2LabelPoint V) => V is M2LpCamFocus, null);
			if (labelPointAll != null)
			{
				this.FocusLp = labelPointAll[0] as M2LpCamFocus;
				this.FocCen = new Vector2(this.FocusLp.focx, this.FocusLp.focy);
			}
			string s = this.Meta.GetS("success_activate");
			if (TX.valid(s))
			{
				this.LpActivateSend = this.Mp.getPoint(s, false);
			}
			this.fineSF(false);
		}

		public override void closeAction(bool when_map_close)
		{
			base.closeAction(when_map_close);
			this.deactivate();
			if (this.PoolTarget != null)
			{
				this.PoolTarget.destruct();
				this.PoolTarget = null;
			}
			this.Meta = null;
			this.auto_activated = 0;
			this.activate_sended = false;
		}

		public bool fineSF(bool play_effect = false)
		{
			return this.fineSF(play_effect, false, false);
		}

		public bool fineSF(bool play_effect, bool check_player_exision, bool reset_player = false)
		{
			if (this.Player != null && !check_player_exision)
			{
				return true;
			}
			if (this.Reader == null)
			{
				return false;
			}
			bool flag = this.pre_on;
			if (this.sf_key != null)
			{
				this.sf_active = COOK.getSF(this.sf_key) != 0;
			}
			else
			{
				this.sf_active = true;
			}
			if (!this.sf_active)
			{
				flag = false;
			}
			if (this.ManageArea != null && (!PUZ.IT.barrier_active || this.auto_activated == 2))
			{
				flag = false;
			}
			if (this.walk_in_activate != -1000)
			{
				if (flag)
				{
					if (!this.isWalkinPr())
					{
						if (this.Player != null)
						{
							this.deactivate();
						}
						this.runner_assigned = true;
						return false;
					}
				}
				else
				{
					this.runner_assigned = false;
				}
			}
			if (flag)
			{
				if (this.Player != null && reset_player)
				{
					this.deactivate();
				}
				this.activate();
			}
			else
			{
				this.deactivate();
			}
			return flag;
		}

		public void changePuzzleActivation(bool activated)
		{
			this.fineSF(activated, !activated, false);
		}

		public bool initEffect(bool activating, ref DRect RcEffect)
		{
			return false;
		}

		public override void activate()
		{
			if (this.Player != null || this.Reader == null)
			{
				return;
			}
			base.activate();
			if (this.PoolTarget == null)
			{
				this.PoolTarget = new ObjPool<M2MatoateTarget>("Pool-" + base.unique_key, this.Mp.gameObject.transform, 8, -1);
			}
			this.Player = new MatoateReader.MatoatePlayer<M2MatoateTarget>(this.Mp, this.Reader, this.PoolTarget, base.mapfocx, base.mapfocy, ref this.play_t, ref this.mato_appeared);
			this.runner_assigned = true;
			if (this.FocusLp != null)
			{
				this.FocusLp.focx = this.FocCen.x;
				this.FocusLp.focy = this.FocCen.y;
			}
			if (this.auto_activated == 0)
			{
				this.auto_activated = 1;
			}
			if (this.ATimer == null)
			{
				this.ATimer = new List<M2CImgDrawerTimer>(1);
				int num = this.mapx;
				int num2 = this.mapy;
				int num3 = this.mapw;
				int num4 = this.maph;
				if (PUZ.IT.barrier_active)
				{
					M2LpPuzzManageArea calledLp = PUZ.IT.getCalledLp();
					if (calledLp != null)
					{
						num = calledLp.mapx;
						num2 = calledLp.mapy;
						num3 = calledLp.mapw;
						num4 = calledLp.maph;
					}
				}
				this.Lay.getAreaCastDrawer<M2CImgDrawerTimer>(num, num2, num3, num4, this.ATimer);
			}
			for (int i = this.ATimer.Count - 1; i >= 0; i--)
			{
				this.ATimer[i].activate((float)this.Reader.whole_time);
			}
		}

		public override void deactivate()
		{
			this.runner_assigned = false;
			if (this.Player == null)
			{
				return;
			}
			base.deactivate();
			this.Player.releaseAll();
			this.Player = null;
			if (this.auto_activated == 1)
			{
				this.auto_activated = 2;
			}
			if (this.FocusLp != null)
			{
				this.FocusLp.focx = this.FocCen.x;
				this.FocusLp.focy = this.FocCen.y;
			}
			if (this.ATimer != null)
			{
				for (int i = this.ATimer.Count - 1; i >= 0; i--)
				{
					this.ATimer[i].deactivate();
				}
			}
		}

		public bool isWalkinPr()
		{
			M2MoverPr pr = this.Mp.Pr;
			return !(pr == null) && this.walk_in_activate != -1000 && base.isContainingMover(pr, (float)this.walk_in_activate * base.CLEN);
		}

		public bool runner_assigned
		{
			get
			{
				return this.runner_assigned_;
			}
			set
			{
				if (this.runner_assigned_ == value)
				{
					return;
				}
				this.runner_assigned_ = value;
				if (value)
				{
					this.Mp.addRunnerObject(this);
					return;
				}
				this.Mp.remRunnerObject(this);
			}
		}

		public bool run(float fcnt)
		{
			if (this.Player == null)
			{
				if (this.walk_in_activate != -1000)
				{
					if (this.isWalkinPr())
					{
						this.fineSF(true);
					}
					return true;
				}
				return false;
			}
			else
			{
				if (!this.Player.run(fcnt, ref this.play_t, ref this.mato_appeared))
				{
					bool flag = this.Player.isSuccess();
					if (flag)
					{
						if (!this.activate_sended)
						{
							this.activate_sended = true;
							if (this.LpActivateSend != null)
							{
								if (this.LpActivateSend is ILinerReceiver)
								{
									(this.LpActivateSend as ILinerReceiver).activateLiner(false);
								}
								else
								{
									this.LpActivateSend.activate();
								}
							}
						}
						if (!this.no_complete_effect)
						{
							this.Mp.PtcSTsetVar("cx", (double)base.mapfocx).PtcSTsetVar("cy", (double)base.mapfocy).PtcSTsetVar("difx", (double)((float)this.mapw * 0.5f * this.Mp.CLENB))
								.PtcSTsetVar("dify", (double)((float)this.maph * 0.5f * this.Mp.CLENB))
								.PtcST("matoate_complete", null, PTCThread.StFollow.NO_FOLLOW);
						}
					}
					if (this.EvtMatoateFinished != null)
					{
						this.EvtMatoateFinished(this.Player, flag);
					}
					this.deactivate();
					return true;
				}
				if (this.FocusLp != null)
				{
					this.Player.setFocusPoint(this.FocusLp);
				}
				return true;
			}
		}

		public void destruct()
		{
			this.deactivate();
		}

		public bool initted
		{
			get
			{
				return this.walk_in_activate >= -1000;
			}
		}

		public void makeSnapShot(PuzzSnapShot.RevertItem Rvi)
		{
			Rvi.x = (float)(this.activate_sended ? 1 : 0);
			Rvi.y = (float)this.walk_in_activate;
			Rvi.time = (int)this.auto_activated;
		}

		public void puzzleRevert(PuzzSnapShot.RevertItem Rvi)
		{
			int num = (int)Rvi.x;
			this.activate_sended = (num & 1) != 0;
			this.walk_in_activate = (int)Rvi.y;
			this.auto_activated = (byte)Rvi.time;
			this.fineSF(true, true, true);
		}

		public MatoateReader.MatoatePlayer<M2MatoateTarget> getPlayer()
		{
			return this.Player;
		}

		private MatoateReader Reader;

		private M2LpPuzzManageArea ManageArea;

		public META Meta;

		private ObjPool<M2MatoateTarget> PoolTarget;

		private MatoateReader.MatoatePlayer<M2MatoateTarget> Player;

		private string sf_key;

		private bool pre_on;

		private bool sf_active = true;

		private float play_t;

		public bool no_complete_effect;

		private int mato_appeared;

		private M2LpCamFocus FocusLp;

		private Vector2 FocCen;

		private bool activate_sended;

		private byte auto_activated;

		private M2LabelPoint LpActivateSend;

		private List<M2CImgDrawerTimer> ATimer;

		private int walk_in_activate = -1001;

		private bool runner_assigned_;

		public delegate void FnMatoateFinished(MatoateReader.MatoatePlayer<M2MatoateTarget> Player, bool successed);
	}
}
