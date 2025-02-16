using System;
using PixelLiner;
using XX;

namespace m2d
{
	public readonly struct M2ChipImagePrefixed
	{
		private M2ChipImagePrefixed(PxlLayer _Lay, M2ImageAtlas.AtlasRect _Atlas, string _dirname, string _slice_str)
		{
			this.Atlas = _Atlas;
			this.Lay = _Lay;
			this.dirname = _dirname;
			this.slice_str = _slice_str;
		}

		public bool valid
		{
			get
			{
				return this.Lay != null;
			}
		}

		public static M2ChipImagePrefixed checkPrefix(string dirname, PxlLayer Lay, M2ImageAtlas.AtlasRect Atlas)
		{
			string name = Lay.name;
			if (TX.isStart(name, "_background_", 0))
			{
				return new M2ChipImagePrefixed(Lay, Atlas, dirname, "_background_");
			}
			if (TX.isStart(name, "_light_", 0))
			{
				return new M2ChipImagePrefixed(Lay, Atlas, dirname, "_light_");
			}
			if (TX.isStart(name, "_ground_", 0))
			{
				return new M2ChipImagePrefixed(Lay, Atlas, dirname, "_ground_");
			}
			if (TX.isStart(name, "_overtop_", 0))
			{
				return new M2ChipImagePrefixed(Lay, Atlas, dirname, "_overtop_");
			}
			if (TX.isStart(name, "_top_", 0))
			{
				return new M2ChipImagePrefixed(Lay, Atlas, dirname, "_top_");
			}
			if (TX.isStart(name, "_Rbackground_", 0))
			{
				return new M2ChipImagePrefixed(Lay, Atlas, dirname, "_Rbackground_");
			}
			return default(M2ChipImagePrefixed);
		}

		public string target_laysrc
		{
			get
			{
				return this.dirname + this.target_layname + ".png";
			}
		}

		public string target_layname
		{
			get
			{
				return TX.slice(this.Lay.name, this.slice_str.Length);
			}
		}

		public void assignAtlas(M2ChipImage Target)
		{
			if (Target == null)
			{
				X.de(this.Lay.ToString() + " に対応するマップチップ(" + this.target_laysrc + ")が見つかりません", null);
				return;
			}
			string text = this.slice_str;
			if (text != null)
			{
				if (text == "_background_")
				{
					Target.initAdditionalLayer(this.Lay, 0, this.Atlas, false);
					return;
				}
				if (text == "_ground_")
				{
					Target.initAdditionalLayer(this.Lay, 1, this.Atlas, false);
					return;
				}
				if (text == "_light_")
				{
					Target.initAdditionalLayer(this.Lay, 3, this.Atlas, false);
					return;
				}
				if (text == "_overtop_")
				{
					Target.initAdditionalLayer(this.Lay, 4, this.Atlas, false);
					return;
				}
				if (text == "_top_")
				{
					Target.initAdditionalLayer(this.Lay, 2, this.Atlas, false);
					return;
				}
				if (!(text == "_Rbackground_"))
				{
					return;
				}
				Target.initAdditionalLayer(this.Lay, 0, this.Atlas, true);
			}
		}

		private readonly M2ImageAtlas.AtlasRect Atlas;

		private readonly PxlLayer Lay;

		public readonly string dirname;

		private readonly string slice_str;

		private const string HEADER_BG = "_background_";

		private const string HEADER_RMV_BG = "_Rbackground_";

		private const string HEADER_LIGHT = "_light_";

		private const string HEADER_OTOP = "_overtop_";

		private const string HEADER_TOP = "_top_";

		private const string HEADER_GROUND = "_ground_";
	}
}
