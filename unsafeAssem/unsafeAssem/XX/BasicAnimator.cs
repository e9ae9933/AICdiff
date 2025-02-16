using System;
using UnityEngine;

namespace XX
{
	public class BasicAnimator : MonoBehaviour, IAnimListener
	{
		private void Awake()
		{
			if (this.Aframe_snd == null)
			{
				this.Aframe_snd = new string[this.maisu];
			}
		}

		public virtual void Update()
		{
			if (this.auto_consider)
			{
				this.updateAnimator(1f);
			}
		}

		public void copyValuesTo(BasicAnimator destE, bool copy_anim_data = false)
		{
			destE.maisu = this.maisu;
			destE.loop_to = this.loop_to;
			destE.auto_consider = this.auto_consider;
			if (copy_anim_data)
			{
				this.calculateFrameSpeed();
				destE.motionSetFrom(this);
			}
		}

		public void calculateFrameSpeed()
		{
			int num = 0;
			this.loop_to = X.Mn(this.maisu - 1, this.loop_to);
			if (this.Aframe_snd != null)
			{
				Array.Resize<string>(ref this.Aframe_snd, this.maisu);
			}
			for (int i = 0; i < this.maisu; i++)
			{
				num += this.getAf(i);
				this.setEndAf(i, num);
			}
			this.fineShiftLoop(-1);
		}

		public void setLoopFrame(int val)
		{
			this.calculateFrameSpeed();
			this.loop_to = X.Mn(this.maisu - 1, val);
			this.fineShiftLoop(-1);
		}

		public void fineShiftLoop(int set_val = -1)
		{
			this.shiftloopafter60 = ((set_val < 0) ? (this.getEndAf(this.maisu - 1) - this.getEndAf(this.loop_to - 1)) : set_val);
		}

		protected virtual int getAf(int i)
		{
			return 1;
		}

		protected virtual int getEndAf(int i)
		{
			if (i < 0)
			{
				return 0;
			}
			return 1;
		}

		protected virtual void setAf(int i, int _f)
		{
		}

		protected virtual void setEndAf(int i, int _f)
		{
		}

		public int getDuration()
		{
			return this.getEndAf(this.maisu - 1);
		}

		public void extendDuration(int dur60)
		{
			float num = (float)(dur60 / this.getEndAf(this.maisu - 1));
			for (int i = 0; i < this.maisu; i++)
			{
				this.setAf(i, (int)Mathf.Round((float)this.getAf(i) * num));
				this.setEndAf(i, (int)Mathf.Round((float)this.getEndAf(i) * num));
			}
			this.keika60 *= num;
			this.shiftloopafter60 = (int)Mathf.Round((float)this.shiftloopafter60 * num);
		}

		public bool updateAnimator(float fcnt = 1f)
		{
			bool flag = false;
			bool flag2 = false;
			this.changed = 0;
			if (this.maisu == 0)
			{
				return false;
			}
			if (this.stopped)
			{
				this.stopped = false;
				this.changed++;
				flag = true;
				flag2 = true;
			}
			else
			{
				this.keika60 += fcnt;
				this.cframe_is_looped = false;
				while (this.keika60 >= (float)this.getEndAf(this.cframe))
				{
					this.changed++;
					if (this.cframe >= this.maisu - 1)
					{
						int num = this.cframe;
						this.loop_count++;
						this.keika60 -= (float)((this.shiftloopafter60 != 0) ? this.shiftloopafter60 : this.getEndAf(this.cframe));
						this.cframe = this.loop_to;
						this.cframe_is_looped = true;
						if (this.cframe != num)
						{
							flag = true;
						}
					}
					else
					{
						this.cframe++;
						flag = (flag2 = true);
					}
				}
			}
			if (flag2 && this.Aframe_snd != null)
			{
				string text = this.Aframe_snd[this.cframe];
			}
			return flag;
		}

		public bool isChanged()
		{
			return this.changed > 0;
		}

		public bool isAnimEnd()
		{
			return this.loop_count > 0;
		}

		public bool isAnimInLooping()
		{
			return this.loop_to <= 0 || this.keika60 >= (float)this.getDuration();
		}

		public int get_loop_count()
		{
			return this.loop_count;
		}

		public void motionReset(int set_frame = 0)
		{
			this.loop_count = 0;
			this.cframe = X.MMX(0, set_frame, this.maisu - 1);
			this.keika60 = (float)((this.cframe <= 0) ? 0 : this.getEndAf(this.cframe - 1));
			this.stopped = true;
			this.changed = 0;
			this.cframe_is_looped = false;
		}

		public void motionSetFrom(BasicAnimator _A)
		{
			this.loop_count = 0;
			this.keika60 = _A.keika60;
			this.stopped = _A.stopped;
			this.cframe = 0;
			bool flag = this.stopped;
			this.updateAnimator(0f);
			if (flag)
			{
				this.updateAnimator(0f);
			}
			this.changed = 0;
			this.cframe_is_looped = false;
		}

		public virtual void destruct()
		{
			this.Aframe_snd = null;
		}

		[HideInInspector]
		public int width;

		[HideInInspector]
		public int height;

		[HideInInspector]
		public int maisu;

		[HideInInspector]
		public int loop_to;

		[HideInInspector]
		public int loop_count;

		[HideInInspector]
		public bool stopped = true;

		private float keika60;

		private int changed;

		[HideInInspector]
		public int cframe;

		[HideInInspector]
		public bool cframe_is_looped;

		[HideInInspector]
		public int shiftloopafter60;

		[HideInInspector]
		public bool auto_consider = true;

		[HideInInspector]
		public string[] Aframe_snd;
	}
}
