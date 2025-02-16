using System;
using UnityEngine;

namespace XX
{
	public class MFont
	{
		public MFont(MTI _Mti, string _mti_path, Font _Target = null)
		{
			this.Mti = _Mti;
			this.mti_path = _mti_path;
			this.Target_ = _Target;
		}

		public MFont(string mti_key)
		{
			this.Mti = new MTI("Font/" + mti_key, null);
			this.mti_path = mti_key;
		}

		public MFont Load()
		{
			if (this.Target_ == null)
			{
				this.Mti.addLoadKey("_", false);
				this.Target_ = this.Mti.Load<Font>(this.mti_path);
				this.Target_.material.mainTexture.filterMode = this.filterMode_;
			}
			return this;
		}

		public void Release()
		{
			if (this.Mti != null && this.Target_ != null)
			{
				this.Mti.remLoadKey("_");
				IN.DestroyOne(this.Target_);
				this.Target_ = null;
			}
		}

		public Material material
		{
			get
			{
				return this.Target_.material;
			}
		}

		public FilterMode filterMode
		{
			get
			{
				return this.filterMode_;
			}
			set
			{
				this.filterMode_ = value;
				if (this.Target_ != null)
				{
					this.Target_.material.mainTexture.filterMode = this.filterMode_;
				}
			}
		}

		public Font Target
		{
			get
			{
				return this.Target_;
			}
		}

		public bool isLoaded()
		{
			return this.Target_ != null;
		}

		public int fontSize
		{
			get
			{
				return this.Target.fontSize;
			}
		}

		private Font Target_;

		private FilterMode filterMode_;

		private const string sta_font_path = "Font/";

		public readonly string mti_path;

		public readonly MTI Mti;
	}
}
