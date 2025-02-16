using System;
using System.Collections.Generic;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2ChipImageNestedParent : M2ChipImageNested
	{
		public M2ChipImageNestedParent(M2ImageContainer _IMGS, string _src)
			: base(_IMGS, _src)
		{
			this.Parent = this;
		}

		public M2ChipImageNestedParent(M2ImageContainer _IMGS, string _dirname, string _basename, bool parent_is_this = false)
			: base(_IMGS, _dirname, _basename)
		{
			if (parent_is_this)
			{
				this.Parent = this;
			}
		}

		public static void calcClmsAndRows(float CLEN, int srcl, int srct, int srcw, int srch, int iclms_, int irows_, out int ns_clms_, out int ns_rows_)
		{
			ns_clms_ = X.IntC((float)(X.IntC((float)(srcl + srcw) / CLEN) - (int)((float)srcl / CLEN)) / (float)iclms_);
			ns_rows_ = X.IntC((float)(X.IntC((float)(srct + srch) / CLEN) - (int)((float)srct / CLEN)) / (float)irows_);
		}

		public override void assignLayerAndAtlas(PxlLayer Lay, M2ImageAtlas.AtlasRect _Atlas, bool apply_basic = false)
		{
			bool flag = this.SourceLayer_ == null;
			this.SourceLayer_ = Lay;
			this.SourceAtlas_ = _Atlas;
			if (Lay == null)
			{
				return;
			}
			int num = (int)this.IMGS.CLEN;
			int num2 = Lay.cvs_left % num;
			int num3 = Lay.cvs_top % num;
			if (!apply_basic && (num2 != this.srcl || num3 != this.srct || this.srcw != Lay.Img.width || this.srch != Lay.Img.height))
			{
				string text = "";
				if (num2 != this.srcl)
				{
					text = TX.add(text, "L" + this.srcl.ToString() + "=>" + num2.ToString(), " / ");
				}
				if (num3 != this.srct)
				{
					text = TX.add(text, "T" + this.srct.ToString() + "=>" + num3.ToString(), " / ");
				}
				if (this.srcw != Lay.Img.width)
				{
					text = TX.add(text, "W" + this.srcw.ToString() + "=>" + Lay.Img.width.ToString(), " / ");
				}
				if (this.srch != Lay.Img.height)
				{
					text = TX.add(text, "H" + this.srch.ToString() + "=>" + Lay.Img.height.ToString(), " / ");
				}
				X.de("CImgNestedの内容に変更あり " + base.src + ": " + text, null);
				this.ImgMain0 = null;
				int ns_len = this.ns_len;
				if (this.AChild != null)
				{
					for (int i = 0; i < ns_len; i++)
					{
						M2ChipImageNested m2ChipImageNested = this.AChild[i];
						if (m2ChipImageNested != null)
						{
							m2ChipImageNested.replaceImgMain(null);
						}
					}
				}
				apply_basic = true;
			}
			if (apply_basic || this.Aconfig == null)
			{
				int num4 = this.ns_clms_;
				int num5 = this.ns_rows_;
				M2ChipImageNested[] achild = this.AChild;
				bool flag2 = flag && apply_basic;
				this.srcl = num2;
				this.srct = num3;
				this.srcw = Lay.Img.width;
				this.srch = Lay.Img.height;
				if (this.Apreload_id != null)
				{
					uint num6 = this.Apreload_id[(this.IMGS.loading_version < 6) ? 0 : (this.Apreload_id.Length - 1)];
				}
				M2ChipImageNestedParent.calcClmsAndRows(this.IMGS.CLEN, this.srcl, this.srct, this.srcw, this.srch, this.iclms_, this.irows_, out this.ns_clms_, out this.ns_rows_);
				if (this.ns_clms_ != num4 || this.ns_rows_ != num5)
				{
					if (this.Apreload_id != null)
					{
						uint[] array = new uint[this.ns_len];
						X.copyTwoDimensionArray<uint>(array, this.ns_clms_, this.ns_rows_, this.Apreload_id, num4, num5, false);
						this.Apreload_id = array;
					}
					if (!this.need_recalc_unuse_pixels && this.Acalced_unuse != null)
					{
						bool[] array2 = new bool[this.ns_len];
						X.copyTwoDimensionArray<bool>(array2, this.ns_clms_, this.ns_rows_, this.Acalced_unuse, num4, num5, false);
						this.Acalced_unuse = array2;
					}
				}
				this.AChild = new M2ChipImageNested[this.ns_len];
				if (this.ns_clms_ != num4 || this.ns_rows_ != num5)
				{
					if (achild != null)
					{
						X.copyTwoDimensionArray<M2ChipImageNested>(this.AChild, this.ns_clms_, this.ns_rows_, achild, num4, num5, false);
					}
				}
				else if (achild != null)
				{
					Array.Copy(achild, this.AChild, X.Mn(this.ns_len, num4 * num5));
				}
				int ns_len2 = this.ns_len;
				string basename = this.basename;
				int num7 = (this.srcl + this.srcw) % num;
				if ((this.srct + this.srch) % num == 0)
				{
				}
				if (base.Meta == null)
				{
					base.setMeta(this.IMGS.getCImgMeta(ushort.MaxValue));
				}
				if (flag2 && this.iclms_ == 1 && this.irows_ == 1)
				{
					base.Meta.ignore_grid = true;
				}
				if (this.need_recalc_unuse_pixels)
				{
					this.Acalced_unuse = this.recalcUnusedPixels();
					this.need_recalc_unuse_pixels = false;
				}
				bool flag3 = false;
				for (int j = 0; j < ns_len2; j++)
				{
					M2ChipImageNested m2ChipImageNested2 = this.AChild[j];
					if ((this.Acalced_unuse != null && this.Acalced_unuse[j]) || (this.Apreload_id != null && this.Apreload_id[j] == 0U))
					{
						this.AChild[j] = null;
					}
					else
					{
						if (!flag3)
						{
							m2ChipImageNested2 = this;
							flag3 = true;
						}
						else if (m2ChipImageNested2 == null)
						{
							m2ChipImageNested2 = new M2ChipImageNested(this.IMGS, this.dirname, basename + "." + j.ToString());
							this.IMGS.assignNewImage(m2ChipImageNested2, false);
							if (this.Apreload_id != null)
							{
								m2ChipImageNested2.chip_id = this.Apreload_id[j];
							}
							this.IMGS.FixId(m2ChipImageNested2, true);
						}
						this.AChild[j] = m2ChipImageNested2;
						m2ChipImageNested2.ns_index = j;
						m2ChipImageNested2.setMeta(base.Meta);
						m2ChipImageNested2.Parent = this;
						int num8;
						int num9;
						this.getPosition(j, out num8, out num9);
						this.recalcChildBounds(num8, num9, out m2ChipImageNested2.shiftx, out m2ChipImageNested2.shifty, out m2ChipImageNested2.iwidth, out m2ChipImageNested2.iheight);
						m2ChipImageNested2.horizony = m2ChipImageNested2.iheight / 2;
					}
				}
				this.recalcConfig();
				this.copyAttributesToChildren();
				this.Apreload_id = null;
			}
			else if (flag)
			{
				this.copyAttributesToChildren();
			}
			if (this.SourceAtlas_.valid)
			{
				this.recreateImageMesh(false);
			}
		}

		public override void recreateImageMesh(bool no_error = false)
		{
			this.ImgMain0 = this.SourceAtlas_.makeMesh(this.IMGS.MIchip, 0f, 0f, this.ImgMain0);
			if (this.SourceLayer_ == null || base.width == 0)
			{
				if (!no_error)
				{
					X.de("チップリストに登録されていない画像: " + base.src, null);
				}
				return;
			}
			base.need_fine_atlas = false;
			if (this.AChild != null && this.SourceAtlas_.valid)
			{
				int ns_len = this.ns_len;
				for (int i = 0; i < ns_len; i++)
				{
					M2ChipImageNested m2ChipImageNested = this.AChild[i];
					if (m2ChipImageNested != null)
					{
						m2ChipImageNested.replaceImgMain(this.getAtlasDrawArea(i).makeMesh(this.IMGS.MIchip, 0f, 0f, m2ChipImageNested.getTargetMainImage()));
					}
				}
			}
		}

		public override void AtlasRescale(float x, float y)
		{
			if (x <= 0f || (x == 1f && y == 1f))
			{
				return;
			}
			base.AtlasRescale(x, y);
			base.AtlasRescaleExecute(this.ImgMain0, x, y);
			int num = this.ns_len;
			for (int i = 0; i < num; i++)
			{
				M2ChipImageNested m2ChipImageNested = this.AChild[i];
				if (m2ChipImageNested != null && m2ChipImageNested != this)
				{
					m2ChipImageNested.AtlasRescale(x, y);
				}
			}
			num = ((this.ACIMeshForPicture != null) ? this.ACIMeshForPicture.Length : 0);
			for (int j = 0; j < num; j++)
			{
				base.AtlasRescaleExecute(this.ACIMeshForPicture[j].Img, x, y);
			}
		}

		private void recalcChildBounds(int x, int y, out int shiftx, out int shifty, out int iwidth, out int iheight)
		{
			shiftx = ((x == 0) ? this.srcl : 0);
			int num = X.Mx(-this.srcl + x * base.CLEN * this.iclms_, 0);
			iwidth = X.Mn(base.CLEN * this.iclms_ - shiftx, base.SourceLayer.Img.width - num);
			shifty = ((y == 0) ? this.srct : 0);
			int num2 = X.Mx(-this.srct + y * base.CLEN * this.irows_, 0);
			iheight = X.Mn(base.CLEN * this.irows_ - shifty, base.SourceLayer.Img.height - num2);
		}

		public bool[] recalcUnusedPixels()
		{
			return null;
		}

		public M2ImageAtlas.AtlasRect getAtlasDrawArea(int i)
		{
			if (this.AChild == null || !X.BTW(0f, (float)i, (float)this.AChild.Length))
			{
				return base.SourceAtlas;
			}
			M2ChipImageNested m2ChipImageNested = this.AChild[i];
			if (m2ChipImageNested == null)
			{
				return new M2ImageAtlas.AtlasRect(this.SourceAtlas_.x, this.SourceAtlas_.y, 0, 0);
			}
			int num;
			int num2;
			this.getPosition(i, out num, out num2);
			int num3 = X.Mx(0, num * base.CLEN * this.iclms_ - this.srcl);
			int num4 = X.Mx(0, num2 * base.CLEN * this.irows_ - this.srct) + m2ChipImageNested.iheight;
			return new M2ImageAtlas.AtlasRect(this.SourceAtlas_.x + num3, this.SourceAtlas_.y + this.SourceAtlas_.h - num4, m2ChipImageNested.iwidth, m2ChipImageNested.iheight);
		}

		public Rect getClipArea(int i)
		{
			if (this.AChild != null && X.BTW(0f, (float)i, (float)this.AChild.Length))
			{
				int num;
				int num2;
				this.getPosition(i, out num, out num2);
				M2ChipImageNested m2ChipImageNested = this.AChild[i];
				int num5;
				int num6;
				if (m2ChipImageNested == null)
				{
					int num3;
					int num4;
					this.recalcChildBounds(num, num2, out num3, out num4, out num5, out num6);
				}
				else
				{
					int iwidth = m2ChipImageNested.iwidth;
					int iheight = m2ChipImageNested.iheight;
					num5 = iwidth;
					num6 = iheight;
				}
				float num7 = (float)X.Mx(0, num * base.CLEN * this.iclms_ - this.srcl);
				int num8 = X.Mx(0, num2 * base.CLEN * this.irows_ - this.srct) + num6;
				return new Rect(num7, (float)(this.SourceLayer_.Img.height - num8), (float)num5, (float)num6);
			}
			return new Rect(0f, 0f, (float)this.iwidth, (float)this.iheight);
		}

		public void recalcConfig()
		{
			this.config_base_x = X.Mn(this.config_base_x, this.ns_clms_ * this.iclms_);
			this.config_base_y = X.Mn(this.config_base_y, this.ns_rows_ * this.irows_);
			int ns_len = this.ns_len;
			for (int i = 0; i < ns_len; i++)
			{
				M2ChipImageNested m2ChipImageNested = this.AChild[i];
				if (m2ChipImageNested != null)
				{
					int num;
					int num2;
					this.getPosition(i, out num, out num2);
					for (int j = 0; j < this.iclms_; j++)
					{
						int num3 = j + num * this.iclms_;
						for (int k = 0; k < this.irows_; k++)
						{
							int num4 = k + num2 * this.irows_;
							int num5;
							if (num4 >= this.config_base_y)
							{
								M2ChipImageNestedParent.CONFIGCALC configcalc = (this.config_upper ? M2ChipImageNestedParent.CONFIGCALC.CANSTAND : ((num3 >= this.config_base_x) ? this.ccalc_r : this.ccalc_l));
								if (configcalc == M2ChipImageNestedParent.CONFIGCALC.CANSTAND)
								{
									num5 = 4;
								}
								else if (configcalc == M2ChipImageNestedParent.CONFIGCALC.WALL)
								{
									num5 = 128;
								}
								else if (configcalc == M2ChipImageNestedParent.CONFIGCALC.LIFT)
								{
									num5 = ((num4 == this.config_base_y) ? 120 : 4);
								}
								else
								{
									int num6 = M2ChipImageNestedParent.ccalc_divide(configcalc);
									int num7 = this.config_base_y + X.Abs(num3 - this.config_base_x + ((num3 < this.config_base_x) ? 1 : 0)) / num6;
									if (num4 < num7)
									{
										num5 = 4;
									}
									else if (num4 > num7)
									{
										num5 = ((configcalc == M2ChipImageNestedParent.CONFIGCALC.L100 || configcalc == M2ChipImageNestedParent.CONFIGCALC.L50) ? 4 : 128);
									}
									else if (num3 >= this.config_base_x)
									{
										num5 = M2ChipImageNestedParent.get_config_cmod(configcalc, (num3 - this.config_base_x) % num6, false);
									}
									else
									{
										num5 = M2ChipImageNestedParent.get_config_cmod(configcalc, num6 - 1 - (this.config_base_x - 1 - num3) % num6, true);
									}
								}
							}
							else
							{
								M2ChipImageNestedParent.CONFIGCALC configcalc2 = ((!this.config_upper) ? M2ChipImageNestedParent.CONFIGCALC.CANSTAND : ((num3 >= this.config_base_x) ? this.ccalc_r : this.ccalc_l));
								if (configcalc2 == M2ChipImageNestedParent.CONFIGCALC.CANSTAND)
								{
									num5 = 4;
								}
								else if (configcalc2 == M2ChipImageNestedParent.CONFIGCALC.WALL)
								{
									num5 = 128;
								}
								else if (configcalc2 == M2ChipImageNestedParent.CONFIGCALC.LIFT)
								{
									num5 = ((num4 == this.config_base_y - 1) ? 120 : 4);
								}
								else
								{
									int num8 = M2ChipImageNestedParent.ccalc_divide(configcalc2);
									int num9 = this.config_base_y - 1 - X.Abs(num3 - this.config_base_x + ((num3 < this.config_base_x) ? 1 : 0)) / num8;
									if (num4 > num9)
									{
										num5 = 4;
									}
									else if (num4 < num9)
									{
										num5 = ((configcalc2 == M2ChipImageNestedParent.CONFIGCALC.L100 || configcalc2 == M2ChipImageNestedParent.CONFIGCALC.L50) ? 4 : 128);
									}
									else if (num3 >= this.config_base_x)
									{
										num5 = M2ChipImageNestedParent.get_config_cmod(configcalc2, (num3 - this.config_base_x) % num8, true);
									}
									else
									{
										num5 = M2ChipImageNestedParent.get_config_cmod(configcalc2, num8 - 1 - (this.config_base_x - 1 - num3) % num8, false);
									}
								}
							}
							m2ChipImageNested.fineWH();
							if (m2ChipImageNested.Aconfig == null)
							{
								m2ChipImageNested.Aconfig = new byte[m2ChipImageNested.clms * m2ChipImageNested.rows];
							}
							if (X.BTW(0f, (float)j, (float)m2ChipImageNested.clms) && X.BTW(0f, (float)k, (float)m2ChipImageNested.rows))
							{
								m2ChipImageNested.Aconfig[k * m2ChipImageNested.clms + j] = (byte)num5;
							}
						}
					}
				}
			}
		}

		private static int ccalc_divide(M2ChipImageNestedParent.CONFIGCALC c)
		{
			switch (c)
			{
			case M2ChipImageNestedParent.CONFIGCALC.S20:
				return 5;
			case M2ChipImageNestedParent.CONFIGCALC.S25:
				return 4;
			case M2ChipImageNestedParent.CONFIGCALC.S33:
				return 3;
			case M2ChipImageNestedParent.CONFIGCALC.S50:
				return 2;
			case M2ChipImageNestedParent.CONFIGCALC.S100:
				return 1;
			case M2ChipImageNestedParent.CONFIGCALC.L50:
				return 2;
			case M2ChipImageNestedParent.CONFIGCALC.L100:
				return 1;
			}
			return 0;
		}

		private static int get_config_cmod(M2ChipImageNestedParent.CONFIGCALC c, int mod, bool to_left = false)
		{
			switch (c)
			{
			case M2ChipImageNestedParent.CONFIGCALC.S20:
			{
				int num;
				if (to_left)
				{
					switch (mod)
					{
					case 0:
						num = 105;
						break;
					case 1:
						num = 106;
						break;
					case 2:
						num = 107;
						break;
					case 3:
						num = 108;
						break;
					default:
						num = 109;
						break;
					}
					return num;
				}
				switch (mod)
				{
				case 0:
					num = 73;
					break;
				case 1:
					num = 74;
					break;
				case 2:
					num = 75;
					break;
				case 3:
					num = 76;
					break;
				default:
					num = 77;
					break;
				}
				return num;
			}
			case M2ChipImageNestedParent.CONFIGCALC.S25:
			{
				int num;
				if (to_left)
				{
					switch (mod)
					{
					case 0:
						num = 101;
						break;
					case 1:
						num = 102;
						break;
					case 2:
						num = 103;
						break;
					default:
						num = 104;
						break;
					}
					return num;
				}
				switch (mod)
				{
				case 0:
					num = 69;
					break;
				case 1:
					num = 70;
					break;
				case 2:
					num = 71;
					break;
				default:
					num = 72;
					break;
				}
				return num;
			}
			case M2ChipImageNestedParent.CONFIGCALC.S33:
			{
				int num;
				if (to_left)
				{
					if (mod != 0)
					{
						if (mod != 1)
						{
							num = 100;
						}
						else
						{
							num = 99;
						}
					}
					else
					{
						num = 98;
					}
					return num;
				}
				if (mod != 0)
				{
					if (mod != 1)
					{
						num = 68;
					}
					else
					{
						num = 67;
					}
				}
				else
				{
					num = 66;
				}
				return num;
			}
			case M2ChipImageNestedParent.CONFIGCALC.S50:
			{
				int num;
				if (to_left)
				{
					if (mod == 0)
					{
						num = 96;
					}
					else
					{
						num = 97;
					}
					return num;
				}
				if (mod == 0)
				{
					num = 64;
				}
				else
				{
					num = 65;
				}
				return num;
			}
			case M2ChipImageNestedParent.CONFIGCALC.S100:
				if (!to_left)
				{
					return 78;
				}
				return 95;
			case M2ChipImageNestedParent.CONFIGCALC.L50:
			{
				int num;
				if (to_left)
				{
					if (mod == 0)
					{
						num = 92;
					}
					else
					{
						num = 93;
					}
					return num;
				}
				if (mod == 0)
				{
					num = 80;
				}
				else
				{
					num = 79;
				}
				return num;
			}
			case M2ChipImageNestedParent.CONFIGCALC.L100:
				if (!to_left)
				{
					return 81;
				}
				return 94;
			}
			return 4;
		}

		public void getPosition(int i, out int x, out int y)
		{
			int num = this.ns_clms_ - this.splice_x;
			int num2 = this.ns_rows_ - this.splice_y;
			if (i < num * num2)
			{
				x = this.splice_x + i % num;
				y = this.splice_y + i / num;
				return;
			}
			i -= num * num2;
			if (i < num * this.splice_y)
			{
				x = this.splice_x + i % num;
				y = this.splice_y - 1 - i / num;
				return;
			}
			i -= num * this.splice_y;
			if (i < this.splice_x * num2)
			{
				x = this.splice_x - 1 - i % this.splice_x;
				y = this.splice_y + i / this.splice_x;
				return;
			}
			i -= this.splice_x * num2;
			x = this.splice_x - 1 - i % this.splice_x;
			y = this.splice_y - 1 - i / this.splice_x;
		}

		public M2ChipImageNested GetAt(int x, int y)
		{
			if (this.AChild == null)
			{
				return null;
			}
			int num = this.ns_clms_ - this.splice_x;
			int num2 = this.ns_rows_ - this.splice_y;
			if (x >= this.splice_x)
			{
				int num3 = x - this.splice_x;
				if (y >= this.splice_y)
				{
					int num4 = y - this.splice_y;
					return this.AChild[num4 * num + num3];
				}
				int num5 = num * num2;
				int num6 = this.splice_y - 1 - y;
				return this.AChild[num5 + num6 * num + num3];
			}
			else
			{
				int num7 = this.splice_x - 1 - x;
				int num8 = num * this.ns_rows_;
				if (y >= this.splice_y)
				{
					int num9 = y - this.splice_y;
					return this.AChild[num8 + num9 * num + num7];
				}
				num8 += num2 * this.splice_x;
				int num10 = this.splice_y - 1 - y;
				return this.AChild[num8 + num10 * num + num7];
			}
		}

		public int ns_len
		{
			get
			{
				return this.ns_clms_ * this.ns_rows_;
			}
		}

		public void copyAttributesToChildren()
		{
			if (this.AChild != null)
			{
				for (int i = 0; i < this.ns_len; i++)
				{
					M2ChipImageNested m2ChipImageNested = this.AChild[i];
					if (m2ChipImageNested != null && m2ChipImageNested != this)
					{
						m2ChipImageNested.copyBasicAttributesFrom(this);
						m2ChipImageNested.assignLayerAndAtlas(this.SourceLayer_, this.SourceAtlas_, false);
					}
				}
			}
		}

		public override M2ChipImage copyAttributesFrom(M2ChipImage Src)
		{
			base.copyBasicAttributesFrom(Src);
			this.copyAttributesToChildren();
			return this;
		}

		public override void copyChildrenTo(List<M2ChipImage> A)
		{
			if (this.AChild != null)
			{
				for (int i = 0; i < this.ns_len; i++)
				{
					M2ChipImageNested m2ChipImageNested = this.AChild[i];
					if (m2ChipImageNested != null)
					{
						A.Add(m2ChipImageNested);
					}
				}
			}
		}

		public override int getChipFixedShiftPos(bool is_y, int id, int set_variable = 2)
		{
			if (!is_y)
			{
				return this.shiftx;
			}
			return this.shifty;
		}

		public void reserveId()
		{
			if (this.Apreload_id != null)
			{
				for (int i = this.Apreload_id.Length - 1; i >= 0; i--)
				{
					this.IMGS.reserveId(this.Apreload_id[i]);
				}
			}
		}

		public PxlMeshDrawer getSourceMeshForPicture(int _layer)
		{
			int num = base.mesh_type - 1;
			if ((_layer == 0) ? (num <= 0) : (_layer == num))
			{
				return this.ImgMain0;
			}
			return base.getSrcMeshCI(_layer, this.ACIMeshForPicture);
		}

		protected override void MeshPushFromAdditionalLayer(PxlLayer Lay, M2ImageAtlas.AtlasRect nAtlas, int layer)
		{
			base.MeshPush(Lay, nAtlas, layer, ref this.ACIMeshForPicture);
			if (Lay.isTransformed())
			{
				X.dl("Nested へのTransformedレイヤー追加:" + base.src + " +" + Lay.ToString(), null, false, false);
			}
			float num = (float)Lay.cvs_left;
			float num2 = (float)Lay.cvs_top;
			float num3 = (float)base.SourceLayer.cvs_left;
			M2ImageAtlas.AtlasRect sourceAtlas = base.SourceAtlas;
			float num4 = (float)base.SourceLayer.cvs_top;
			M2ImageAtlas.AtlasRect sourceAtlas2 = base.SourceAtlas;
			int num5 = this.AChild.Length;
			for (int i = 0; i < num5; i++)
			{
				M2ChipImageNested m2ChipImageNested = this.AChild[i];
				if (m2ChipImageNested != null)
				{
					int num6;
					int num7;
					this.getPosition(i, out num6, out num7);
					float num8 = (float)X.Mx(0, num6 * base.CLEN * this.iclms_ - this.srcl);
					float num9 = (float)X.Mx(0, num7 * base.CLEN * this.irows_ - this.srct);
					float num10 = num8 + (float)m2ChipImageNested.iwidth;
					float num11 = num9 + (float)m2ChipImageNested.iheight;
					num8 += (float)base.SourceLayer.cvs_left - num;
					num9 += (float)base.SourceLayer.cvs_top - num2;
					num10 += (float)base.SourceLayer.cvs_left - num;
					num11 += (float)base.SourceLayer.cvs_top - num2;
					float num12 = num8;
					float num13 = num9;
					float num14 = num10;
					float num15 = num11;
					if (num6 == 0)
					{
						num12 -= 600f;
					}
					if (num6 == this.ns_clms - 1)
					{
						num14 += 600f;
					}
					if (num7 == 0)
					{
						num13 -= 600f;
					}
					if (num7 == this.ns_rows - 1)
					{
						num15 += 600f;
					}
					Rect rect = new Rect(0f, 0f, (float)nAtlas.w, (float)nAtlas.h);
					rect = X.rectMultiply(rect, num12, num13, num14 - num12, num15 - num13);
					if (rect.width > 0f && rect.height > 0f)
					{
						M2ImageAtlas.AtlasRect atlasRect = new M2ImageAtlas.AtlasRect(nAtlas.x + (int)rect.x, nAtlas.y + nAtlas.h - (int)rect.yMax, (int)rect.width, (int)rect.height);
						float num16 = X.Mx(0f, rect.x - num8);
						float num17 = X.Mx(0f, num11 - rect.yMax);
						PxlMeshDrawer pxlMeshDrawer = atlasRect.makeMesh(this.IMGS.MIchip, (float)(-(float)m2ChipImageNested.iwidth) * 0.5f + (float)atlasRect.w * 0.5f + num16, (float)(-(float)m2ChipImageNested.iheight) * 0.5f + (float)atlasRect.h * 0.5f + num17, m2ChipImageNested.getSrcMesh(layer));
						m2ChipImageNested.MeshPush(pxlMeshDrawer, layer);
					}
				}
			}
		}

		public static M2ChipImageNestedParent readFromBytesN(ByteReader Ba, byte load_ver, M2ImageContainer IMGS, string dirname, bool create = true)
		{
			string text = Ba.readPascalString("utf-8", false);
			if (load_ver <= 6)
			{
				text = X.basename_noext(text);
			}
			int num = ((load_ver < 5) ? ((int)Ba.readUShort()) : Ba.readByte());
			int num2 = ((load_ver < 5) ? ((int)Ba.readUShort()) : Ba.readByte());
			int num3 = 1;
			int num4 = 1;
			if (load_ver >= 5)
			{
				int num5 = Ba.readByte();
				num3 = (num5 >> 4) & 15;
				num4 = num5 & 15;
			}
			int num6 = (int)Ba.readUShort();
			int num7 = (int)Ba.readUShort();
			int num8 = Ba.readByte();
			int num9 = Ba.readByte();
			int num10 = Ba.readByte();
			int num11 = Ba.readByte();
			int num12 = Ba.readByte();
			M2ChipImageNestedParent.CONFIGCALC configcalc = (M2ChipImageNestedParent.CONFIGCALC)(num12 & 127);
			bool flag = (num12 & 128) != 0;
			M2ChipImageNestedParent.CONFIGCALC configcalc2 = (M2ChipImageNestedParent.CONFIGCALC)Ba.readByte();
			int num13;
			int num14;
			M2ChipImageNestedParent.calcClmsAndRows(IMGS.CLEN, num, num2, num6, num7, num3, num4, out num13, out num14);
			int num15 = num13 * num14;
			uint num16 = Ba.readUInt();
			int num17 = ((load_ver < 6) ? num15 : (num15 + 1));
			uint[] array = (create ? new uint[num17] : null);
			for (int i = 0; i < num17; i++)
			{
				uint num18 = Ba.readUInt();
				if (array != null)
				{
					array[i] = num18;
					if (num18 >= IMGS.imgs_capacity + 1000U)
					{
						array[i] = 0U;
					}
				}
			}
			ushort num19 = Ba.readUShort();
			uint num20 = ((load_ver <= 8) ? ((uint)Ba.readUShort()) : Ba.readUInt());
			METACImg cimgMeta = IMGS.getCImgMeta(Ba.readUShort());
			if (create)
			{
				M2ChipImageNestedParent m2ChipImageNestedParent = new M2ChipImageNestedParent(IMGS, dirname, text, false);
				m2ChipImageNestedParent.srcl = num;
				m2ChipImageNestedParent.srct = num2;
				m2ChipImageNestedParent.srcw = num6;
				m2ChipImageNestedParent.srch = num7;
				m2ChipImageNestedParent.iclms_ = num3;
				m2ChipImageNestedParent.irows_ = num4;
				m2ChipImageNestedParent.splice_x = num8;
				m2ChipImageNestedParent.splice_y = num9;
				m2ChipImageNestedParent.config_base_x = num10;
				m2ChipImageNestedParent.config_base_y = num11;
				m2ChipImageNestedParent.ns_clms_ = num13;
				m2ChipImageNestedParent.ns_rows_ = num14;
				m2ChipImageNestedParent.ccalc_l = configcalc;
				m2ChipImageNestedParent.ccalc_r = configcalc2;
				m2ChipImageNestedParent.config_upper = flag;
				m2ChipImageNestedParent.chip_id = num16;
				m2ChipImageNestedParent.Apreload_id = array;
				m2ChipImageNestedParent.family_index = num19;
				m2ChipImageNestedParent.need_recalc_unuse_pixels = false;
				m2ChipImageNestedParent.basic_flags = (m2ChipImageNestedParent.basic_flags & 4294965248U) | (num20 & 2047U);
				m2ChipImageNestedParent.setMeta(cimgMeta);
				return m2ChipImageNestedParent;
			}
			return null;
		}

		public int ns_clms
		{
			get
			{
				return this.ns_clms_;
			}
		}

		public int ns_rows
		{
			get
			{
				return this.ns_rows_;
			}
		}

		public int iclms
		{
			get
			{
				return this.iclms_;
			}
		}

		public int irows
		{
			get
			{
				return this.irows_;
			}
		}

		public int get_srcl()
		{
			return this.srcl;
		}

		public int get_srct()
		{
			return this.srct;
		}

		private int srcl;

		private int srct;

		private int srcw;

		private int srch;

		private int iclms_ = 1;

		private int irows_ = 1;

		public int splice_x;

		public int splice_y;

		public int config_base_x;

		public int config_base_y;

		public bool config_upper;

		public M2ChipImageNestedParent.CONFIGCALC ccalc_l;

		public M2ChipImageNestedParent.CONFIGCALC ccalc_r;

		private uint[] Apreload_id;

		private bool[] Acalced_unuse;

		public bool need_recalc_unuse_pixels = true;

		private int ns_clms_;

		private int ns_rows_;

		private M2ChipImageNested[] AChild;

		private PxlMeshDrawer ImgMain0;

		private M2ChipImage.CIMesh[] ACIMeshForPicture;

		public const string nested_expat_footer = ".pat.png";

		public enum CONFIGCALC
		{
			CANSTAND,
			WALL,
			S20,
			S25,
			S33,
			S50,
			S100,
			LIFT,
			L50,
			L100
		}

		private struct CIMeshReserve
		{
		}
	}
}
