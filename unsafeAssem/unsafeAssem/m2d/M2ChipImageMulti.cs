using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace m2d
{
	public abstract class M2ChipImageMulti : IIdvName, IM2CLItem, IM2Inputtable
	{
		public string family
		{
			get
			{
				return this.IMGS.getFamilyName(this.family_index);
			}
			set
			{
				this.family_index = this.IMGS.getFamilyIndex(value);
			}
		}

		public int CLEN
		{
			get
			{
				return (int)this.IMGS.CLEN;
			}
		}

		public M2DBase M2D
		{
			get
			{
				return this.IMGS.M2D;
			}
		}

		private M2ChipImageMulti(M2ImageContainer _IMGS, char _headkey)
		{
			this.IMGS = _IMGS;
			this.Link = new Flagger(null, null);
			this.headkey = _headkey;
		}

		public M2ChipImageMulti(M2ImageContainer _IMGS, char _headkey, string _dirname, string _basename)
			: this(_IMGS, _headkey)
		{
			this.dirname_ = _dirname;
			this.basename_ = _basename;
		}

		public M2ChipImageMulti(M2ImageContainer _IMGS, char _headkey, string _src)
			: this(_IMGS, _headkey)
		{
			this.changePatternBasenameAndDirname(_src);
		}

		public M2ChipImageMulti changePatternBasenameAndDirname(string __dirname, string __basename)
		{
			this.dirname_ = __dirname;
			this.basename_ = __basename;
			return this;
		}

		public M2ChipImageMulti changePatternBasenameAndDirname(string _src)
		{
			if (_src.IndexOf("^") == _src.Length - 1)
			{
				_src = TX.slice(_src, 0, _src.Length - 1);
				this.favorited = true;
			}
			this.dirname_ = X.dirname(_src);
			this.basename_ = X.basename(_src).Replace(this.headkey.ToString() ?? "", "");
			return this;
		}

		public override string ToString()
		{
			return this.src;
		}

		public string getTitle()
		{
			return this.src;
		}

		public string get_individual_key()
		{
			return this.src;
		}

		public string src
		{
			get
			{
				return this.dirname_ + this.headkey.ToString() + this.basename_;
			}
		}

		public string dirname
		{
			get
			{
				return this.dirname_;
			}
		}

		public string basename
		{
			get
			{
				return this.basename_;
			}
		}

		public bool isLinked()
		{
			return this.Link.isActive();
		}

		public bool isFavorited()
		{
			return this.favorited;
		}

		public string getDirName()
		{
			return this.dirname;
		}

		public string getBaseName()
		{
			return this.basename_;
		}

		public uint getChipId()
		{
			return this.chip_id;
		}

		public abstract bool isBg();

		public abstract void LinkSources(bool need_fine_editor_buttons = true);

		public abstract void UnlinkSources(bool need_fine_editor_buttons = true);

		public abstract List<M2Picture> MakePicture(M2MapLayer Lay, float x, float y, int opacity, int rotation, bool flip);

		public abstract bool isSame(M2Puts Cp, bool in_editor = false, int editor_curs_x = -1, int editor_curs_y = -1);

		public abstract Vector2Int getClmsAndRows(INPUT_CR cr);

		public abstract M2ChipImage getFirstImage();

		public abstract List<M2Chip> MakeChip(M2MapLayer Lay, int x, int y, int opacity, int rotation, bool flip);

		public abstract IM2CLItem spoitImageAt(int j, ref int rotation, ref bool flip);

		public abstract void writeAt(int j, IM2CLItem Img, int rotation, bool flip);

		public abstract void eraseAt(int j);

		public readonly M2ImageContainer IMGS;

		public readonly char headkey;

		private string dirname_;

		private string basename_;

		public Flagger Link;

		public bool favorited;

		protected int is_bg = -1;

		public M2ChipImage Thumbnail;

		public uint chip_id;

		public ushort family_index = ushort.MaxValue;
	}
}
