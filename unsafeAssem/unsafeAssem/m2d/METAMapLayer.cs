using System;
using XX;

namespace m2d
{
	public sealed class METAMapLayer : META
	{
		public METAMapLayer(string com = null)
			: base(com)
		{
			this.clearCache();
		}

		public void clearCache()
		{
			METAMapLayer.OVERTOP overtop;
			switch (base.GetI("overtop", 0, 0))
			{
			case -1:
				overtop = METAMapLayer.OVERTOP.ALLT;
				goto IL_003E;
			case 1:
				overtop = METAMapLayer.OVERTOP.T2TT;
				goto IL_003E;
			case 2:
				overtop = METAMapLayer.OVERTOP.ALLTT;
				goto IL_003E;
			case 3:
				overtop = METAMapLayer.OVERTOP.ALLT;
				goto IL_003E;
			}
			overtop = METAMapLayer.OVERTOP.NONE;
			IL_003E:
			this.overtop = overtop;
			this.on_light = base.GetB("light", false);
			this.dat_byte = 0;
			this.auto_declare = base.GetI("auto_declare", 0, 0) != 0;
			this.no_remover = base.GetI("no_remover", 0, 0) != 0;
			this.no_consider_config = base.GetI("no_consider_config", 0, 0) != 0;
			this.no_draw = base.GetI("no_draw", 0, 0) != 0;
			int ie = base.GetIE("no_chiplight", 0, 0);
			this.no_chiplight = ie != 0;
			float nm = base.GetNm("window_remover", -1f, 0);
			if (nm != -1f)
			{
				this.opt_window_remover_setted = true;
				this.window_remover = nm != 0f;
			}
			else
			{
				this.opt_window_remover_setted = false;
			}
			this.is_chip_arrangeable_ = 2;
			if (base.GetIE("fake", 0, 0) != 0)
			{
				this.fakewall = METAMapLayer.FAKEWALL.FAKE;
			}
		}

		public bool is_fake
		{
			get
			{
				return this.fakewall == METAMapLayer.FAKEWALL.FAKE;
			}
			set
			{
				if (value && this.fakewall < METAMapLayer.FAKEWALL.FAKE)
				{
					this.fakewall = METAMapLayer.FAKEWALL.FAKE;
				}
			}
		}

		public bool is_decrared
		{
			get
			{
				return this.fakewall == METAMapLayer.FAKEWALL.DECLEARED;
			}
			set
			{
				if (value)
				{
					if (this.fakewall < METAMapLayer.FAKEWALL.DECLEARED)
					{
						this.fakewall = METAMapLayer.FAKEWALL.DECLEARED;
						return;
					}
				}
				else if (this.fakewall == METAMapLayer.FAKEWALL.DECLEARED)
				{
					this.fakewall = METAMapLayer.FAKEWALL.FAKE;
				}
			}
		}

		public bool do_not_consider_config
		{
			get
			{
				return this.fakewall > METAMapLayer.FAKEWALL.REALITY;
			}
		}

		public bool do_not_consider_draw_bounds
		{
			get
			{
				return base.GetB("no_draw_bounds", false);
			}
		}

		public bool is_chip_arrangeable
		{
			get
			{
				if (this.is_chip_arrangeable_ == 2)
				{
					this.is_chip_arrangeable_ = ((this.fakewall != METAMapLayer.FAKEWALL.REALITY || base.GetB("summon_hide", false) || base.GetB("summon_show", false) || base.GetB("arrangeable", false)) ? 1 : 0);
				}
				return this.is_chip_arrangeable_ > 0;
			}
			set
			{
				this.is_chip_arrangeable_ = (value ? 1 : 2);
			}
		}

		public bool auto_declare
		{
			get
			{
				return (this.dat_byte & 1) != 0;
			}
			set
			{
				this.dat_byte = (value ? (this.dat_byte | 1) : (this.dat_byte & -2));
			}
		}

		public bool no_chiplight
		{
			get
			{
				return (this.dat_byte & 2) != 0;
			}
			set
			{
				this.dat_byte = (value ? (this.dat_byte | 2) : (this.dat_byte & -3));
			}
		}

		public bool no_remover
		{
			get
			{
				return (this.dat_byte & 4) != 0;
			}
			set
			{
				this.dat_byte = (value ? (this.dat_byte | 4) : (this.dat_byte & -5));
			}
		}

		public bool opt_window_remover_setted
		{
			get
			{
				return (this.dat_byte & 8) != 0;
			}
			set
			{
				this.dat_byte = (value ? (this.dat_byte | 8) : (this.dat_byte & -9));
			}
		}

		private bool window_remover
		{
			get
			{
				return (this.dat_byte & 16) != 0;
			}
			set
			{
				this.dat_byte = (value ? (this.dat_byte | 16) : (this.dat_byte & -17));
			}
		}

		private bool no_consider_config
		{
			get
			{
				return (this.dat_byte & 32) != 0;
			}
			set
			{
				this.dat_byte = (value ? (this.dat_byte | 32) : (this.dat_byte & -33));
			}
		}

		public bool no_draw
		{
			get
			{
				return (this.dat_byte & 64) != 0;
			}
			set
			{
				this.dat_byte = (value ? (this.dat_byte | 64) : (this.dat_byte & -65));
			}
		}

		public bool useWindowRemover(Map2d Mp)
		{
			if (!this.opt_window_remover_setted)
			{
				return Mp.Dgn.use_window_remover;
			}
			return this.window_remover;
		}

		private METAMapLayer.FAKEWALL fakewall;

		public METAMapLayer.OVERTOP overtop;

		public bool on_light;

		private int dat_byte;

		private byte is_chip_arrangeable_ = 2;

		private enum FAKEWALL
		{
			REALITY,
			FAKE,
			DECLEARED
		}

		public enum OVERTOP
		{
			NONE,
			T2TT,
			ALLTT,
			ALLT
		}
	}
}
