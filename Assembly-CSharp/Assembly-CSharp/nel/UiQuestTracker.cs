using System;
using System.Collections.Generic;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class UiQuestTracker : RBase<UiQuestTrackerRow>
	{
		public bool need_fine_item_pos
		{
			get
			{
				return this.item_pos_y != -1000f;
			}
			set
			{
				if (!value)
				{
					this.item_pos_y = -1000f;
				}
			}
		}

		public UiQuestTracker(UIBase _Base, GameObject _Gob)
			: base(2, false, false, false)
		{
			this.Base = _Base;
			this.Gob = IN.CreateGob(_Gob, "-QUEST");
			IN.setZ(this.Gob.transform, -0.2f);
			this.ATrack = new List<UiQuestTracker.TrackTask>(4);
			this.FlgHide = new Flagger(delegate(FlaggerT<string> V)
			{
				this.deactivate();
			}, delegate(FlaggerT<string> V)
			{
				this.activate();
			});
		}

		public override void destruct()
		{
			base.destruct();
		}

		public bool use_valotile
		{
			get
			{
				return this.use_valotile_;
			}
			set
			{
				this.use_valotile_ = value;
				int len = this.LEN;
				for (int i = 0; i < len; i++)
				{
					this.AItems[i].use_valotile = value;
				}
			}
		}

		public override UiQuestTrackerRow Create()
		{
			return new UiQuestTrackerRow(this, this.LEN);
		}

		public void clearTask(bool clear_ui = true)
		{
			if (clear_ui)
			{
				for (int i = 0; i < this.LEN; i++)
				{
					this.AItems[i].deactivate(true);
				}
			}
			this.ATrack.Clear();
			this.check_position_stack = true;
			this.t_trackread_lock = 0;
			this.can_progress = false;
			this.need_fine_pos = true;
		}

		public void AddStack(QuestTracker.QuestProgress Prog, QuestTracker.INVISIBLE t, int phase, uint _written_row_bits = 0U)
		{
			int count = this.ATrack.Count;
			int i = 0;
			while (i < count)
			{
				UiQuestTracker.TrackTask trackTask = this.ATrack[i];
				if (trackTask.Q == Prog.Q)
				{
					if (t == QuestTracker.INVISIBLE.POS && trackTask.type != QuestTracker.INVISIBLE.POS)
					{
						if (trackTask.TargetRow == null)
						{
							return;
						}
						break;
					}
					else
					{
						trackTask.type = t;
						trackTask.dphase = phase;
						if (t != QuestTracker.INVISIBLE.UPDATE && t != QuestTracker.INVISIBLE.END)
						{
							trackTask.sphase = phase;
						}
						if (trackTask.TargetRow == null || trackTask.TargetRow.updateTask(trackTask, _written_row_bits))
						{
							this.ATrack.RemoveAt(i);
							this.ATrack.Add(trackTask);
							return;
						}
						break;
					}
				}
				else
				{
					i++;
				}
			}
			this.ATrack.Add(new UiQuestTracker.TrackTask(t, Prog, phase, _written_row_bits));
			if (this.t_trackread_lock == -1000)
			{
				this.t_trackread_lock = 4;
			}
			if (this.t > 18f)
			{
				this.t = 18f;
			}
		}

		public void HideInactivePositionStack(Map2d PreMp)
		{
			for (int i = this.ATrack.Count - 1; i >= 0; i--)
			{
				UiQuestTracker.TrackTask trackTask = this.ATrack[i];
				if (trackTask.type == QuestTracker.INVISIBLE.POS)
				{
					QuestTracker.QuestDeperture currentDepert = trackTask.Prog.CurrentDepert;
					if (currentDepert.isActiveMap() && currentDepert.real_map_target == PreMp.key)
					{
						this.deactivateRow(i, false);
					}
				}
			}
		}

		public override bool run(float fcnt)
		{
			if (this.t >= 0f)
			{
				if (this.ATrack.Count > 0 && this.t_trackread_lock != -1000)
				{
					if (this.t_trackread_lock > 0)
					{
						this.t_trackread_lock--;
					}
					else
					{
						this.t_trackread_lock = 4;
						int count = this.ATrack.Count;
						int i = 0;
						while (i < count)
						{
							UiQuestTracker.TrackTask trackTask = this.ATrack[i];
							if (trackTask.TargetRow == null)
							{
								base.Pop(64).initialize(ref trackTask);
								this.ATrack[i] = trackTask;
								if (i == count - 1)
								{
									this.t_trackread_lock = -1000;
									break;
								}
								break;
							}
							else
							{
								i++;
							}
						}
					}
				}
				this.can_progress = this.t >= 9f;
				if (this.t <= 18f)
				{
					this.t += fcnt;
					this.need_fine_pos = true;
				}
			}
			else
			{
				this.can_progress = false;
				if (this.t > -18f)
				{
					this.t -= fcnt;
					this.need_fine_pos = true;
				}
			}
			this.need_fine_item_pos = false;
			base.run(fcnt);
			if (this.need_fine_pos)
			{
				this.need_fine_pos = false;
				float num = this.Base.visib_status_z;
				num = X.Mn(num, (this.t >= 0f) ? X.ZSIN2(this.t, 18f) : (1f - X.ZPOW(-this.t, 18f)));
				float num2;
				float num3;
				if (UiQuestTracker.position_is_right)
				{
					num2 = -1f;
					num3 = 2f;
				}
				else
				{
					num2 = 1f;
					num3 = 18f;
				}
				float num4 = -UIBase.pwh + (170f + num3) * (-1.25f + 2.25f * num);
				IN.PosP2(this.Gob.transform, num2 * num4, 0f);
			}
			return true;
		}

		public float getItemY(UiQuestTrackerRow Itm, float shiftlevel = 1f)
		{
			float num = Itm.rh * 0.5f;
			this.need_fine_item_pos = true;
			float num2 = -UIBase.phh + 50f + num;
			if (Itm == this.AItems[0])
			{
				this.item_pos_y = num2;
			}
			else
			{
				this.item_pos_y -= (this.pre_item_rh + num + 24f) * this.pre_item_shiftlevel;
			}
			this.pre_item_shiftlevel = shiftlevel;
			this.pre_item_rh = num;
			return this.item_pos_y;
		}

		public bool CanProgress(bool remove_flag = true)
		{
			if (this.can_progress)
			{
				if (remove_flag)
				{
					this.can_progress = false;
				}
				return true;
			}
			return false;
		}

		public void activate()
		{
			if (this.t < 0f)
			{
				this.t = X.Mx(0f, 18f + this.t);
				this.t_trackread_lock = 4;
			}
		}

		public void deactivate()
		{
			if (this.t >= 0f)
			{
				this.t = X.Mn(-1f, -18f + this.t);
			}
		}

		public void deactivateRow(QuestTracker.QuestProgress Prog, bool immediate = false)
		{
			int count = this.ATrack.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.ATrack[i].Prog == Prog)
				{
					this.deactivateRow(i, immediate);
					return;
				}
			}
		}

		public void deactivateRow(UiQuestTrackerRow Row, bool immediate = false)
		{
			int count = this.ATrack.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.ATrack[i].TargetRow == Row)
				{
					this.deactivateRow(i, immediate);
					return;
				}
			}
		}

		private void deactivateRow(int index, bool immediate = false)
		{
			UiQuestTrackerRow targetRow = this.ATrack[index].TargetRow;
			if (targetRow != null)
			{
				targetRow.deactivate(immediate);
			}
			this.ATrack.RemoveAt(index);
		}

		public bool isShowing(bool strict = false)
		{
			if (!strict)
			{
				return this.t > -18f;
			}
			return this.t >= 0f;
		}

		public void progressLock()
		{
			if (this.t_trackread_lock < 4 && this.t_trackread_lock != -1000)
			{
				this.t_trackread_lock = 4;
			}
		}

		public void fineFonts()
		{
			for (int i = 0; i < this.AItems.Length; i++)
			{
				UiQuestTrackerRow uiQuestTrackerRow = this.AItems[i];
				if (uiQuestTrackerRow != null)
				{
					uiQuestTrackerRow.fineFonts();
				}
			}
		}

		public QuestTracker.QuestDeperture getFrontDepert(out QuestTracker.QuestProgress Prog)
		{
			QuestTracker.QuestDeperture questDeperture = default(QuestTracker.QuestDeperture);
			Prog = null;
			for (int i = 0; i < this.LEN; i++)
			{
				UiQuestTrackerRow uiQuestTrackerRow = this.AItems[i];
				if (uiQuestTrackerRow.isActive())
				{
					QuestTracker.QuestDeperture currentDepert = uiQuestTrackerRow.Prog.CurrentDepert;
					if (currentDepert.isActiveMap())
					{
						Prog = uiQuestTrackerRow.Prog;
						return currentDepert;
					}
				}
			}
			return questDeperture;
		}

		public static bool position_is_right
		{
			get
			{
				return CFGSP.uipic_lr == CFGSP.UIPIC_LR.R;
			}
		}

		public override string ToString()
		{
			return "UiQuestTracker";
		}

		public readonly UIBase Base;

		public readonly GameObject Gob;

		private List<UiQuestTracker.TrackTask> ATrack;

		public bool check_position_stack;

		private bool can_progress;

		private bool use_valotile_ = true;

		private float t;

		private int t_trackread_lock;

		public bool need_fine_pos = true;

		private float item_pos_y;

		private float pre_item_rh;

		private float pre_item_shiftlevel;

		private const float MAXT_FADE = 18f;

		public readonly Flagger FlgHide;

		public struct TrackTask
		{
			public TrackTask(QuestTracker.INVISIBLE _type, QuestTracker.QuestProgress _Prog, int phase, uint _written_row_bits = 0U)
			{
				this.type = _type;
				this.Prog = _Prog;
				this.dphase = phase;
				this.sphase = phase;
				this.written_row_bits = _written_row_bits;
				if ((this.type == QuestTracker.INVISIBLE.UPDATE || this.type == QuestTracker.INVISIBLE.END) && this.sphase > 0)
				{
					this.sphase--;
				}
				this.TargetRow = null;
			}

			public QuestTracker.Quest Q
			{
				get
				{
					return this.Prog.Q;
				}
			}

			public QuestTracker.INVISIBLE type;

			public QuestTracker.QuestProgress Prog;

			public int sphase;

			public int dphase;

			public uint written_row_bits;

			public UiQuestTrackerRow TargetRow;
		}
	}
}
