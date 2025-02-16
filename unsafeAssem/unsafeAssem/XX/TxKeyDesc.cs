using System;
using System.Collections.Generic;

namespace XX
{
	public class TxKeyDesc : TextRendererRBKeyDesc
	{
		public TxKeyDesc.KDTicket AddTicket(byte priority, TxKeyDesc.FnGetKD FD, object Target = null)
		{
			this.need_reposit = true;
			int count = this.ATk.Count;
			int num = 0;
			while (num < count && this.ATk[num].priority >= priority)
			{
				num++;
			}
			TxKeyDesc.KDTicket kdticket = this.PoolTk.Pool().Set(priority, FD, Target);
			this.ATk.Insert(num, kdticket);
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
				this.Alpha(0f);
			}
			return kdticket;
		}

		public bool isFront(TxKeyDesc.KDTicket Tk)
		{
			return this.ATk.Count > 0 && this.ATk[0] == Tk;
		}

		public void startFadein()
		{
			this.t = X.Mn(this.t, 0f);
		}

		public void Clear()
		{
			this.t = -30f;
			this.CurTicket = null;
			this.ATk.Clear();
			base.runner_assigned = false;
			base.gameObject.SetActive(false);
		}

		protected override void Awake()
		{
			base.Awake();
			this.PoolTk = new ClsPool<TxKeyDesc.KDTicket>(() => new TxKeyDesc.KDTicket(this), 2);
			this.ATk = new List<TxKeyDesc.KDTicket>(2);
			this.Alpha(0f);
		}

		public override bool run(float fcnt)
		{
			if (this.need_reposit)
			{
				int num = this.ATk.Count;
				int i = 0;
				TxKeyDesc.KDTicket kdticket = null;
				while (i < num)
				{
					TxKeyDesc.KDTicket kdticket2 = this.ATk[i];
					if (kdticket2.FD != null)
					{
						kdticket = kdticket2;
						break;
					}
					this.PoolTk.Release(kdticket2);
					this.ATk.RemoveAt(i);
					num--;
				}
				if (kdticket != this.CurTicket)
				{
					this.CurTicket = kdticket;
					if (this.CurTicket == null)
					{
						if (this.t >= 0f)
						{
							this.t = X.Mn(-1f, -30f + this.t);
						}
					}
					else
					{
						this.t = 0f;
						this.Alpha(0f);
					}
				}
				if (this.CurTicket != null)
				{
					base.Size(this.CurTicket.size);
					base.shift_pixel_y = this.CurTicket.shift_pixel_y;
					using (STB stb = TX.PopBld(null, 0))
					{
						this.CurTicket.FD(stb, this.CurTicket.Target);
						if (this.t > 0f && !this.Stb.Equals(stb))
						{
							this.t = 0f;
						}
						base.Txt(stb);
					}
					base.top_center = this.CurTicket.top_center;
					IN.setZ(base.transform, this.CurTicket.showable_front_ui ? this.z_front : this.z_bottom);
					this.need_fine_alpha = true;
				}
			}
			if (X.D)
			{
				if (this.t >= 0f && this.CurTicket != null)
				{
					this.need_fine_alpha = this.need_fine_alpha || this.CurTicket.hold_blink;
					if (this.t < 30f)
					{
						this.t += (float)X.AF * fcnt;
						this.need_fine_alpha = true;
					}
					if (this.need_fine_alpha)
					{
						this.need_fine_alpha = false;
						float num2 = X.ZLINE(this.t, 30f);
						if (this.CurTicket.hold_blink)
						{
							num2 *= 0.5f + X.COSIT(16f) * 0.1f;
						}
						this.Alpha(num2);
					}
				}
				else
				{
					this.need_fine_alpha = false;
					this.t -= (float)X.AF * fcnt;
					this.Alpha(X.ZLINE(30f + this.t, 30f));
					if (this.t <= -30f)
					{
						this.runner_assigned_ = false;
						base.gameObject.SetActive(false);
						return false;
					}
				}
			}
			return base.run(fcnt);
		}

		public override string ToString()
		{
			return "TxKD";
		}

		private bool need_fine_alpha;

		public float z_front = -9.4f;

		public float z_bottom = -1f;

		public const float T_FADE = 30f;

		public const byte PRI_GO = 150;

		public const byte PRI_CAM = 160;

		public const byte PRI_MGM = 170;

		private float t = -30f;

		private ClsPool<TxKeyDesc.KDTicket> PoolTk;

		private List<TxKeyDesc.KDTicket> ATk;

		private TxKeyDesc.KDTicket CurTicket;

		public delegate void FnGetKD(STB Stb, object Target);

		public class KDTicket
		{
			public KDTicket(TxKeyDesc _Con)
			{
				this.Con = _Con;
			}

			public TxKeyDesc.KDTicket Set(byte _priority, TxKeyDesc.FnGetKD _FD, object _Target = null)
			{
				this.priority = _priority;
				this.FD = _FD;
				this.Target = _Target;
				this.top_center_ = (this.showable_front_ui_ = (this.hold_blink_ = false));
				this.size_ = 14f;
				this.shift_pixel_y_ = 0f;
				return this;
			}

			public bool need_fine
			{
				set
				{
					if (value && this.Con.isFront(this))
					{
						this.Con.need_reposit = true;
					}
				}
			}

			public bool is_front
			{
				get
				{
					return this.Con.isFront(this);
				}
			}

			public float size
			{
				get
				{
					return this.size_;
				}
				set
				{
					if (this.size_ == value)
					{
						return;
					}
					this.size_ = value;
					if (this.Con.isFront(this))
					{
						this.Con.need_reposit = true;
					}
				}
			}

			public float shift_pixel_y
			{
				get
				{
					return this.shift_pixel_y_;
				}
				set
				{
					if (this.shift_pixel_y_ == value)
					{
						return;
					}
					this.shift_pixel_y_ = value;
					if (this.Con.isFront(this))
					{
						this.Con.need_reposit = true;
					}
				}
			}

			public bool top_center
			{
				get
				{
					return this.top_center_;
				}
				set
				{
					if (this.top_center_ == value)
					{
						return;
					}
					this.top_center_ = value;
					if (this.Con.isFront(this))
					{
						this.Con.need_reposit = true;
					}
				}
			}

			public bool showable_front_ui
			{
				get
				{
					return this.showable_front_ui_;
				}
				set
				{
					if (this.showable_front_ui_ == value)
					{
						return;
					}
					this.showable_front_ui_ = value;
					if (this.Con.isFront(this))
					{
						this.Con.need_reposit = true;
					}
				}
			}

			public bool hold_blink
			{
				get
				{
					return this.hold_blink_;
				}
				set
				{
					if (this.hold_blink_ == value)
					{
						return;
					}
					this.hold_blink_ = value;
					if (this.Con.isFront(this))
					{
						this.Con.need_fine_alpha = true;
					}
				}
			}

			public TxKeyDesc.KDTicket destruct()
			{
				this.FD = null;
				this.Target = null;
				if (this.Con.isFront(this))
				{
					this.Con.need_reposit = true;
				}
				return null;
			}

			public void stopShake()
			{
				if (this.Con.isFront(this))
				{
					this.Con.stopShake();
				}
			}

			public void startShake()
			{
				if (this.Con.isFront(this))
				{
					this.Con.Shake();
				}
			}

			public void startFadein()
			{
				if (this.Con.isFront(this))
				{
					this.Con.startFadein();
				}
			}

			public bool isShaking()
			{
				return this.Con.isShaking();
			}

			public readonly TxKeyDesc Con;

			public object Target;

			public TxKeyDesc.FnGetKD FD;

			public byte priority;

			private bool showable_front_ui_;

			private float size_ = 14f;

			private bool top_center_;

			private bool hold_blink_;

			private float shift_pixel_y_;
		}
	}
}
