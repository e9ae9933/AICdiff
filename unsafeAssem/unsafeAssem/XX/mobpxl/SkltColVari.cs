using System;
using Better;
using PixelLiner;
using PixelLiner.PixelLinerLib;

namespace XX.mobpxl
{
	internal class SkltColVari
	{
		internal SkltColVari(string _key)
		{
			this.key = _key;
			this.Opltkey2Plt = new BDic<string, SkltColVari.SkltPaletteC>();
			this.Orewrite_pltkey = new BDic<SkltSequence.SkltDesc, string>();
		}

		internal SkltColVari.SkltPaletteC Get(SkltImage Img, bool no_make = true)
		{
			return this.Get(this.getTargetPaletteKey(Img), no_make);
		}

		public string getTargetPaletteKey(SkltImage Img)
		{
			if (this.Orewrite_pltkey.Count > 0)
			{
				SkltSequence.SkltDesc sqDescKey = Img.getSqDescKey();
				string text;
				if (this.Orewrite_pltkey.TryGetValue(sqDescKey, out text))
				{
					return text;
				}
			}
			return Img.palette_key;
		}

		internal SkltColVari.SkltPaletteC Get(string palette_key, bool no_make = true)
		{
			SkltColVari.SkltPaletteC skltPaletteC;
			if (this.Opltkey2Plt.TryGetValue(palette_key, out skltPaletteC))
			{
				return skltPaletteC;
			}
			if (!no_make)
			{
				return this.Opltkey2Plt[palette_key] = new SkltColVari.SkltPaletteC(palette_key, 0);
			}
			return null;
		}

		internal void removePltKeyReplacer(SkltImage Img)
		{
			if (this.Orewrite_pltkey.Count > 0)
			{
				SkltSequence.SkltDesc sqDescKey = Img.getSqDescKey();
				this.Orewrite_pltkey.Remove(sqDescKey);
			}
		}

		internal SkltColVari.SkltPaletteC setRewritePltKey(SkltImage Img, string key)
		{
			if (key == Img.palette_key)
			{
				this.removePltKeyReplacer(Img);
			}
			else
			{
				SkltSequence.SkltDesc sqDescKey = Img.getSqDescKey();
				this.Orewrite_pltkey[sqDescKey] = key;
			}
			return this.Get(key, false);
		}

		internal static SkltColVari readFromBytes(ByteArray Ba, string key, PxlCharacter Pcr, int vers = 13)
		{
			SkltColVari skltColVari = new SkltColVari(key);
			int num = Ba.readByte();
			for (int i = 0; i < num; i++)
			{
				string text = Ba.readPascalString("utf-8", false);
				SkltColVari.SkltPaletteC skltPaletteC = new SkltColVari.SkltPaletteC(text, 0);
				skltPaletteC.readFromBytes(Ba, -1, Pcr);
				if (!skltPaletteC.isEmpty() || skltPaletteC.overwrite_basic_color)
				{
					skltColVari.Opltkey2Plt[text] = skltPaletteC;
				}
			}
			if (vers >= 13)
			{
				num = Ba.readByte();
				for (int j = 0; j < num; j++)
				{
					SkltSequence.SkltDesc skltDesc = new SkltSequence.SkltDesc(Ba);
					string text2 = Ba.readPascalString("utf-8", false);
					skltColVari.Orewrite_pltkey[skltDesc] = text2;
				}
			}
			else
			{
				skltColVari.Orewrite_pltkey.Clear();
			}
			return skltColVari;
		}

		public string key;

		public readonly BDic<string, SkltColVari.SkltPaletteC> Opltkey2Plt;

		public readonly BDic<SkltSequence.SkltDesc, string> Orewrite_pltkey;

		internal class SkltPaletteC : SkltPalette
		{
			internal SkltPaletteC(string _key, int _capacity)
				: base(_key, _capacity)
			{
				this.key = _key;
			}

			public override string get_individual_key()
			{
				return this.key;
			}

			internal override void readFromBytes(ByteArray Ba, int ARMX = -1, PxlCharacter Pcr = null)
			{
				base.readFromBytes(Ba, ARMX, Pcr);
				this.overwrite_basic_color = Ba.readBoolean();
			}

			public string key;

			public bool overwrite_basic_color;
		}
	}
}
