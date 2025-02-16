using System;
using UnityEngine;

namespace XX
{
	public class ConfirmAnnouncer : TextRenderer
	{
		protected override void Awake()
		{
			base.Awake();
			base.html_mode = true;
			base.letter_spacing = 0.95f;
			base.color_apply_to_image = true;
			base.MakeValot(null, null);
		}

		protected void setDefault()
		{
			this.content_suffix = "";
			this.alpha_blink = true;
			this.base_alpha = 0.8f;
			this.Col(this.DefaultCol);
			base.BorderCol(MTRX.ColTrnsp);
			base.Size((float)this.default_size);
			base.transform.localRotation = Quaternion.identity;
		}

		public ConfirmAnnouncer deactivate()
		{
			return this.init(null, null, true);
		}

		public ConfirmAnnouncer init(ConfirmAnnouncer.IConfirmHolder _Hld, Transform _Trs = null, bool is_active = true)
		{
			if (this.Hld == _Hld)
			{
				return this;
			}
			this.Hld = _Hld;
			this.Trs = _Trs;
			if (this.Hld == null || _Trs == null)
			{
				this.trs_enabled = false;
				base.gameObject.SetActive(false);
				return this;
			}
			this.trs_enabled = true;
			this.setDefault();
			ALIGN align;
			ALIGNY aligny;
			this.Hld.initConfirmAnnouncer(this, out this.content_, out align, out aligny, out this.Pivot_);
			base.Align(align);
			base.AlignY(aligny);
			this.need_tx_content_fine = true;
			this.need_fine_position = true;
			this.need_alpha_set = true;
			this.fineCheck(true);
			base.gameObject.SetActive(is_active);
			return this;
		}

		private void fineTxcontent()
		{
			this.need_tx_content_fine = false;
			this.Stb.Set((this.content_ == null) ? this.default_content_ : this.content_);
			if (TX.valid(this.content_suffix_))
			{
				this.Stb.Add(this.content_suffix_);
			}
			base.Redraw(false);
		}

		private void finePosition()
		{
			this.need_fine_position = false;
			Vector3 vector = this.Trs.position;
			vector += new Vector3(this.Pivot_.x * 0.015625f, this.Pivot_.y * 0.015625f, this.Pivot_.z);
			bool flag = base.transform.position.z != vector.z;
			base.transform.position = vector;
			if (flag && this.use_valotile_)
			{
				base.ResortZValot();
			}
		}

		private void fineAlpha()
		{
			this.need_alpha_set = false;
			base.alpha = (this.alpha_blink ? X.saturate(this.base_alpha_ + X.COSIT(100f) * 0.2f) : this.base_alpha_);
		}

		public override void Update()
		{
			base.Update();
			this.fineCheck(false);
		}

		private void fineCheck(bool is_first = false)
		{
			if (!this.trs_enabled)
			{
				return;
			}
			if (this.need_tx_content_fine)
			{
				this.fineTxcontent();
			}
			if (this.need_fine_position)
			{
				this.finePosition();
			}
			if (is_first)
			{
				base.alpha = 0f;
				return;
			}
			if (this.need_alpha_set || this.alpha_blink)
			{
				this.fineAlpha();
			}
		}

		public Vector3 Pivot
		{
			get
			{
				return this.Pivot_;
			}
			set
			{
				if (this.Pivot != value)
				{
					this.Pivot_ = value;
					this.need_tx_content_fine = true;
					this.need_fine_position = true;
				}
			}
		}

		public string content
		{
			get
			{
				return this.content_;
			}
			set
			{
				if (this.content_ != value)
				{
					this.content_ = value;
					this.need_tx_content_fine = true;
				}
			}
		}

		public string content_suffix
		{
			get
			{
				return this.content_suffix_;
			}
			set
			{
				if (this.content_suffix != value)
				{
					this.content_suffix_ = value;
					this.need_tx_content_fine = true;
				}
			}
		}

		public string default_content
		{
			get
			{
				return this.default_content_;
			}
			set
			{
				if (this.default_content != value)
				{
					this.default_content_ = value;
					if (this.content_ == null)
					{
						this.need_tx_content_fine = true;
					}
				}
			}
		}

		public float base_alpha
		{
			get
			{
				return this.base_alpha_;
			}
			set
			{
				if (this.base_alpha != value)
				{
					this.base_alpha_ = value;
					this.need_alpha_set = true;
				}
			}
		}

		public bool alpha_blink
		{
			get
			{
				return (this.flags & 256U) > 0U;
			}
			set
			{
				if (this.alpha_blink != value)
				{
					this.flags = (this.flags & 4294967039U) | (value ? 256U : 0U);
					this.need_alpha_set = true;
				}
			}
		}

		public void fineValotile(bool default_val)
		{
			if (this.Hld is IValotileSetable)
			{
				base.use_valotile = (this.Hld as IValotileSetable).use_valotile;
				return;
			}
			base.use_valotile = default_val;
		}

		public bool targetIs(ConfirmAnnouncer.IConfirmHolder _Hld)
		{
			return this.Hld == _Hld;
		}

		public bool need_tx_content_fine
		{
			get
			{
				return (this.flags & 1U) > 0U;
			}
			set
			{
				this.flags = (this.flags & 4294967294U) | (value ? 1U : 0U);
			}
		}

		public bool need_fine_position
		{
			get
			{
				return (this.flags & 2U) > 0U;
			}
			set
			{
				this.flags = (this.flags & 4294967293U) | (value ? 2U : 0U);
			}
		}

		public bool need_alpha_set
		{
			get
			{
				return (this.flags & 4U) > 0U;
			}
			set
			{
				this.flags = (this.flags & 4294967291U) | (value ? 4U : 0U);
			}
		}

		public bool trs_enabled
		{
			get
			{
				return (this.flags & 8U) > 0U;
			}
			set
			{
				this.flags = (this.flags & 4294967287U) | (value ? 8U : 0U);
			}
		}

		private Vector3 Pivot_;

		private ConfirmAnnouncer.IConfirmHolder Hld;

		private Transform Trs;

		public Color32 DefaultCol = MTRX.ColBlack;

		public int default_size = 18;

		private string content_;

		private string default_content_ = "<key cancel/>/<key submit/>";

		public const string default_content_xs = "<key cancel/>/<key submit/>";

		public const string default_content_s = "<key submit/>";

		private string content_suffix_;

		private float base_alpha_;

		public const float BASE_ALPHA = 0.8f;

		private uint flags;

		public interface IConfirmHolder
		{
			void initConfirmAnnouncer(ConfirmAnnouncer Confirmer, out string content, out ALIGN align, out ALIGNY aligny, out Vector3 Pivot);
		}
	}
}
