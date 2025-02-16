using System;
using System.Collections.Generic;
using XX;

namespace m2d
{
	public sealed class M2ImagePriority
	{
		public M2ImagePriority(M2DBase _M2D)
		{
			this.M2D = _M2D;
			this.APr = new List<M2ImagePriority.ImgPriority>();
			this.reload();
		}

		public void reload()
		{
			string text = NKT.readStreamingText("m2d/__m2d_cimg_priority.dat", false);
			this.reloadScript(text);
		}

		public void reloadScript(string scrpt)
		{
			CsvReader csvReader = new CsvReader(scrpt, CsvReader.RegComma, false);
			this.default_priority = -1;
			this.APr.Clear();
			if (this.APr.Capacity < csvReader.getLength())
			{
				this.APr.Capacity = csvReader.getLength();
			}
			while (csvReader.read())
			{
				M2ImagePriority.ImgPriority imgPriority = new M2ImagePriority.ImgPriority(csvReader.cmd)
				{
					d = csvReader._1,
					val = csvReader.Int(2, 0)
				};
				if (imgPriority.type == M2ImagePriority.TYPE.DEFAULT)
				{
					this.default_priority = imgPriority.val;
				}
				this.APr.Add(imgPriority);
			}
			if (this.default_priority < 0)
			{
				M2ImagePriority.ImgPriority imgPriority2 = new M2ImagePriority.ImgPriority("DEFAULT");
				imgPriority2.d = "";
				imgPriority2.val = csvReader.Int(2, 0);
			}
		}

		public string writeScript(bool write_to_file = true)
		{
			int count = this.APr.Count;
			string text = "";
			for (int i = 0; i < count; i++)
			{
				M2ImagePriority.ImgPriority imgPriority = this.APr[i];
				text = TX.add(text, string.Concat(new string[]
				{
					imgPriority.type.ToString().ToUpper(),
					",",
					imgPriority.d,
					",",
					imgPriority.val.ToString()
				}), "\n");
			}
			if (write_to_file)
			{
				NKT.saveText("StreamingAssets/m2d/__m2d_cimg_priority.dat", text);
			}
			return text;
		}

		public bool Get(string key, out int priority)
		{
			M2ImagePriority.ImgPriority imgPriority;
			if (this.Get(key, out imgPriority))
			{
				priority = imgPriority.val;
				return true;
			}
			priority = 0;
			return false;
		}

		public bool Get(string key, out M2ImagePriority.ImgPriority Pri)
		{
			int count = this.APr.Count;
			int num = this.default_priority;
			for (int i = 0; i < count; i++)
			{
				M2ImagePriority.ImgPriority imgPriority = this.APr[i];
				if (imgPriority.d == key)
				{
					Pri = imgPriority;
					return true;
				}
			}
			Pri = null;
			return false;
		}

		public int consider(M2ChipImage Img)
		{
			int count = this.APr.Count;
			int val = this.default_priority;
			for (int i = 0; i < count; i++)
			{
				M2ImagePriority.ImgPriority imgPriority = this.APr[i];
				switch (imgPriority.type)
				{
				case M2ImagePriority.TYPE.DEFAULT:
					val = imgPriority.val;
					break;
				case M2ImagePriority.TYPE.FAMILY:
					if (Img.family == imgPriority.d)
					{
						return imgPriority.val;
					}
					break;
				case M2ImagePriority.TYPE.FAMILY_PREFIX:
					if (TX.isStart(Img.family, imgPriority.d, 0))
					{
						return imgPriority.val;
					}
					break;
				case M2ImagePriority.TYPE.IS_BG:
					if (Img.isBg())
					{
						return imgPriority.val;
					}
					break;
				}
			}
			return val;
		}

		private readonly List<M2ImagePriority.ImgPriority> APr;

		public const string priority_script_name = "__m2d_cimg_priority";

		public readonly M2DBase M2D;

		public int default_priority;

		public enum TYPE
		{
			DEFAULT,
			FAMILY,
			FAMILY_PREFIX,
			IS_BG,
			__MAX
		}

		public sealed class ImgPriority
		{
			public ImgPriority()
			{
			}

			public ImgPriority(string t)
			{
				FEnum<M2ImagePriority.TYPE>.TryParse(t, out this.type, true);
			}

			public M2ImagePriority.TYPE type = M2ImagePriority.TYPE.FAMILY;

			public string d = "";

			public int val;
		}
	}
}
