using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class aBtnNelPopper : aBtnNel
	{
		protected override void Awake()
		{
			base.Awake();
			base.unselectable(true);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.Snd != null)
			{
				this.Snd.Dispose();
			}
		}

		public aBtnNelPopper position(float _sx, float _sy, float _dx = -1000f, float _dy = -1000f, bool no_reset_time = false)
		{
			Vector3 vector = base.transform.localPosition * 64f;
			if (this.af >= 0)
			{
				if (this.af < 2)
				{
					if (_dx != -1000f)
					{
						IN.PosP2(base.transform, vector.x = _sx, vector.y = _sy);
					}
				}
				else
				{
					if (_dx == -1000f)
					{
						_dx = (this.dx = _sx);
						_dy = (this.dy = _sy);
					}
					_sx = vector.x;
					_sy = vector.y;
					IN.PosP2(base.transform, _sx, vector.y = _sy);
				}
			}
			else if (this.af < -2 || _dx == -1000f)
			{
				_dx = vector.x;
				_dy = vector.y;
			}
			else
			{
				IN.PosP2(base.transform, vector.x = _sx, vector.y = _sy);
			}
			if (_dx == -1000f)
			{
				_dx = (this.dx = _sx);
				_dy = (this.dy = _sy);
			}
			else
			{
				this.sx = _sx;
				this.dx = _dx;
				this.sy = _sy;
				this.dy = _dy;
			}
			if (!no_reset_time || this.post == -1)
			{
				this.post = 0;
			}
			if (no_reset_time && this.post >= this.maxt_pos)
			{
				this.post = this.maxt_pos;
			}
			return this;
		}

		public void posSetA(float _sx, float _sy, float _dx, float _dy, bool no_reset_time = false)
		{
			if (_sx != -1000f)
			{
				this.sx = _sx;
			}
			if (_sy != -1000f)
			{
				this.sy = _sy;
			}
			if (_dx != -1000f)
			{
				this.dx = _dx;
			}
			if (_dy != -1000f)
			{
				this.dy = _dy;
			}
			if (!no_reset_time || this.post == -1)
			{
				this.post = 0;
			}
			if (no_reset_time && this.post >= this.maxt_pos)
			{
				this.post = this.maxt_pos;
			}
		}

		public override bool run(float fcnt)
		{
			if (!base.run(fcnt))
			{
				return false;
			}
			if (this.pop_execute_count != 0)
			{
				this.t_execute -= fcnt;
				if (this.t_execute <= 0f)
				{
					this.popExecute(this.pop_execute_count > 0);
					this.pop_execute_count = X.VALWALK(this.pop_execute_count, 0, 1);
					this.t_execute = ((this.pop_execute_count == 0) ? 0f : this.execute_interval);
				}
			}
			if (this.post >= 0 && this.post < this.maxt_pos && X.D)
			{
				this.post += X.AF;
				this.calcPosition(-1f);
			}
			if (this.pitch_f > 0)
			{
				this.pitch_f -= 1;
			}
			return true;
		}

		public override ButtonSkin makeButtonSkin(string key)
		{
			this.Skin = base.makeButtonSkin(key);
			if (this.Skin is ButtonSkinPopper)
			{
				this.SkinPopper = this.Skin as ButtonSkinPopper;
			}
			return this.Skin;
		}

		public void resetPopPitch()
		{
			this.pitch_now = 0;
			this.pitch_f = 0;
		}

		public override void bind()
		{
			bool flag = base.isActive();
			base.bind();
			if (!flag && this.post >= 0)
			{
				this.post = 0;
			}
			this.resetPopPitch();
		}

		public override void hide()
		{
			bool flag = base.isActive();
			base.hide();
			this.pop_execute_count = 0;
			this.t_execute = 0f;
			if (flag && this.post >= 0)
			{
				this.post = 0;
			}
			this.resetPopPitch();
		}

		public void initPopping(int count)
		{
			this.initPopping(count > 0, X.Abs(count));
		}

		public void initPopping(bool forward, int count = 1)
		{
			if (count == 0)
			{
				return;
			}
			int num = count * X.MPF(forward);
			if (this.pop_execute_count * num > 0)
			{
				this.pop_execute_count += num;
				return;
			}
			this.pop_execute_count = num;
			this.t_execute = 0f;
		}

		private void popExecute(bool forward)
		{
			if (this.SkinPopper != null)
			{
				this.SkinPopper.initPopping(forward);
				string text = (forward ? this.pop_forward_snd : this.pop_backward_snd);
				if (TX.valid(text))
				{
					if (!this.use_pitch)
					{
						SND.Ui.play(text, false);
						return;
					}
					if (this.Snd == null)
					{
						this.Snd = new SndPlayer("popper_" + base.gameObject.name, SndPlayer.SNDTYPE.SND);
					}
					if (this.pitch_f == 0 || !forward)
					{
						this.pitch_now = 0;
					}
					if (this.Snd.prepare(text, false))
					{
						this.Snd.getPlayerInstance().SetPitch((float)SND.count2pitch((int)this.pitch_now));
						this.Snd.playDefault();
						if (forward)
						{
							this.pitch_now += 1;
							this.pitch_f = 40;
						}
					}
				}
			}
		}

		protected virtual void calcPosition(float tzs = -1f)
		{
			if (tzs < 0f)
			{
				if (this.af >= 0)
				{
					tzs = X.ZSIN2((float)this.post, (float)this.maxt_pos);
				}
				else
				{
					tzs = 1f - X.ZSIN((float)this.post, (float)this.maxt_pos);
				}
			}
			this.salpha = tzs;
			this.SetXyPx(X.NAIBUN_I(this.sx, this.dx, tzs), X.NAIBUN_I(this.sy, this.dy, tzs));
		}

		public virtual aBtnNelPopper SetXyPx(float x, float y)
		{
			IN.PosP2(base.transform, x, y);
			return this;
		}

		public float salpha
		{
			get
			{
				return this.salpha_;
			}
			private set
			{
				if (this.salpha == value)
				{
					return;
				}
				this.salpha_ = value;
				base.Fine(false);
			}
		}

		public PxlFrame IconPF
		{
			get
			{
				if (this.SkinPopper == null)
				{
					return null;
				}
				return this.SkinPopper.PF;
			}
		}

		public float icon_scale
		{
			get
			{
				if (this.SkinPopper == null)
				{
					return 1f;
				}
				return this.SkinPopper.drawn_ico_scale;
			}
		}

		public float icon_shift_x
		{
			get
			{
				if (this.SkinPopper == null)
				{
					return 1f;
				}
				return this.SkinPopper.iconx;
			}
			set
			{
				if (this.SkinPopper != null)
				{
					this.SkinPopper.iconx = value;
				}
			}
		}

		public float icon_shift_y
		{
			get
			{
				if (this.SkinPopper == null)
				{
					return 1f;
				}
				return this.SkinPopper.icony;
			}
			set
			{
				if (this.SkinPopper != null)
				{
					this.SkinPopper.icony = value;
				}
			}
		}

		public bool move_animating
		{
			get
			{
				return this.post >= 0 && this.post < this.maxt_pos;
			}
		}

		public override bool btn_enabled
		{
			get
			{
				return base.btn_enabled;
			}
			set
			{
				bool flag = value != base.btn_enabled;
				base.btn_enabled = value;
				if (flag)
				{
					this.post = ((this.post < 0) ? (-1) : 0);
				}
			}
		}

		protected float sx;

		protected float sy;

		protected float dx;

		protected float dy;

		protected int post = -1;

		public int maxt_pos = 12;

		public string pop_forward_snd = "store_cart_in";

		public string pop_backward_snd = "store_cart_out";

		public const float DEFAULT_W = 130f;

		private ButtonSkinPopper SkinPopper;

		private SndPlayer Snd;

		protected byte pitch_now;

		protected byte pitch_f;

		protected float t_execute;

		public float execute_interval = 4f;

		protected int pop_execute_count;

		public bool use_pitch = true;

		private float salpha_;
	}
}
