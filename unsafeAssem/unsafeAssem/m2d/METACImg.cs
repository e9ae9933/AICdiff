using System;
using System.Collections.Generic;
using Better;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace m2d
{
	public sealed class METACImg : META
	{
		public METACImg(string com = null)
			: base(com)
		{
		}

		public METACImg(int id, string _name = "")
			: this(null)
		{
			this.meta_id = (ushort)id;
			this.name = _name;
		}

		public METACImg(METACImg Src)
			: this(null)
		{
			this.CopyAll(Src);
		}

		public override META CopyAll(META SrcMeta)
		{
			if (SrcMeta is METACImg)
			{
				METACImg metacimg = SrcMeta as METACImg;
				this.name = metacimg.name;
				this.alloc_rot_flip_ = metacimg.alloc_rot_flip_;
			}
			return base.CopyAll(SrcMeta);
		}

		public override bool isSame(META SrcMeta)
		{
			if (SrcMeta is METACImg)
			{
				METACImg metacimg = SrcMeta as METACImg;
				return this.alloc_rot_flip_ == metacimg.alloc_rot_flip_ && base.isSame(metacimg);
			}
			return false;
		}

		public override bool isEmpty()
		{
			return base.isEmpty() && this.alloc_rot_flip_ == 0U;
		}

		public override META Add(string k, string[] Av)
		{
			if (k != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(k);
				if (num <= 2106251764U)
				{
					if (num <= 1615910259U)
					{
						if (num != 219432934U)
						{
							if (num == 1615910259U)
							{
								if (k == "draw_lit_layer")
								{
									this.draw_lit_layer = global::XX.X.NmI(Av[0], 0, true, false) != 0;
									return this;
								}
							}
						}
						else if (k == "__linked")
						{
							return this;
						}
					}
					else if (num != 1705919936U)
					{
						if (num == 2106251764U)
						{
							if (k == "ignore_grid")
							{
								this.ignore_grid = global::XX.X.sumBits(Av) != 0;
								return this;
							}
						}
					}
					else if (k == "__alloc_rot_flip")
					{
						this.alloc_rot_flip_ = global::XX.X.NmUI(Av[0], 0U, true, false);
						return this;
					}
				}
				else if (num <= 2326695837U)
				{
					if (num != 2117091054U)
					{
						if (num == 2326695837U)
						{
							if (k == "no_blur_draw")
							{
								this.no_blur_draw = global::XX.X.NmI(Av[0], 0, true, false) != 0;
								return this;
							}
						}
					}
					else if (k == "multiple_input")
					{
						this.alloc_multiple_input = global::XX.X.sumBits(Av) != 0;
						return this;
					}
				}
				else if (num != 2519057040U)
				{
					if (num != 2557513032U)
					{
						if (num == 2708649949U)
						{
							if (k == "window")
							{
								if (global::XX.X.NmI(Av[0], 0, true, false) != 0)
								{
									this.alloc_rot_flip_ |= 262144U;
								}
								else
								{
									this.alloc_rot_flip_ &= 4294705151U;
								}
							}
						}
					}
					else if (k == "no_make_checkpoint")
					{
						this.no_make_checkpoint = global::XX.X.sumBits(Av) != 0;
						return this;
					}
				}
				else if (k == "invisible")
				{
					this.invisible = global::XX.X.NmI(Av[0], 0, true, false) != 0;
					return this;
				}
			}
			return base.Add(k, Av);
		}

		public void readFromBytes(ByteArray Ba, byte load_ver)
		{
			this.alloc_rot_flip_ = Ba.readUInt();
			if (load_ver >= 2)
			{
				this.name = Ba.readPascalString("utf-8", false);
			}
			int num = Ba.readByte();
			if (num > 0)
			{
				if (this.D == null)
				{
					this.D = new BDic<string, string[]>(num);
				}
				for (int i = 0; i < num; i++)
				{
					string text = Ba.readPascalString("utf-8", false);
					int num2 = Ba.readByte();
					if (num2 == 0)
					{
						this.Add(text, META.Astr1);
					}
					else
					{
						string[] array = new string[num2];
						for (int j = 0; j < num2; j++)
						{
							array[j] = Ba.readPascalString("utf-8", false);
						}
						this.Add(text, array);
					}
				}
			}
		}

		public void writeToBytes(ByteArray Ba)
		{
			Ba.writeUInt(this.alloc_rot_flip_ & 4294705151U);
			Ba.writePascalString(this.name, "utf-8");
			if (this.D != null)
			{
				Ba.writeByte(this.D.Count);
				using (Dictionary<string, string[]>.Enumerator enumerator = this.D.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<string, string[]> keyValuePair = enumerator.Current;
						Ba.writePascalString(keyValuePair.Key, "utf-8");
						int num = keyValuePair.Value.Length;
						if (num == 1 && keyValuePair.Value[0] == "1")
						{
							Ba.writeByte(0);
						}
						else
						{
							Ba.writeByte(num);
							for (int i = 0; i < num; i++)
							{
								Ba.writePascalString(keyValuePair.Value[i], "utf-8");
							}
						}
					}
					return;
				}
			}
			Ba.writeByte(0);
		}

		public static bool isReservedkey(string k)
		{
			if (k != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(k);
				if (num <= 1705919936U)
				{
					if (num <= 1271958859U)
					{
						if (num <= 219432934U)
						{
							if (num != 153987631U)
							{
								if (num != 219432934U)
								{
									return false;
								}
								if (!(k == "__linked"))
								{
									return false;
								}
							}
							else if (!(k == "fix_flips"))
							{
								return false;
							}
						}
						else if (num != 975100625U)
						{
							if (num != 1271958859U)
							{
								return false;
							}
							if (!(k == "is_small_image"))
							{
								return false;
							}
						}
						else if (!(k == "alloc_rots"))
						{
							return false;
						}
					}
					else if (num <= 1534881441U)
					{
						if (num != 1531445303U)
						{
							if (num != 1534881441U)
							{
								return false;
							}
							if (!(k == "individual_chip"))
							{
								return false;
							}
							global::XX.X.de("individual_chip ではなく multiple_input を使うこと", null);
							return false;
						}
						else if (!(k == "draw_overtop_layer"))
						{
							return false;
						}
					}
					else if (num != 1615910259U)
					{
						if (num != 1705919936U)
						{
							return false;
						}
						if (!(k == "__alloc_rot_flip"))
						{
							return false;
						}
					}
					else if (!(k == "draw_lit_layer"))
					{
						return false;
					}
				}
				else if (num <= 2519057040U)
				{
					if (num <= 2241615357U)
					{
						if (num != 2117091054U)
						{
							if (num != 2241615357U)
							{
								return false;
							}
							if (!(k == "__favorited"))
							{
								return false;
							}
						}
						else if (!(k == "multiple_input"))
						{
							return false;
						}
					}
					else if (num != 2326695837U)
					{
						if (num != 2519057040U)
						{
							return false;
						}
						if (!(k == "invisible"))
						{
							return false;
						}
					}
					else if (!(k == "no_blur_draw"))
					{
						return false;
					}
				}
				else if (num <= 2557513032U)
				{
					if (num != 2531457895U)
					{
						if (num != 2557513032U)
						{
							return false;
						}
						if (!(k == "no_make_checkpoint"))
						{
							return false;
						}
					}
					else if (!(k == "fix_rots"))
					{
						return false;
					}
				}
				else if (num != 2841372992U)
				{
					if (num != 3732281393U)
					{
						return false;
					}
					if (!(k == "alloc_flips"))
					{
						return false;
					}
				}
				else if (!(k == "odd_flip"))
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public bool ignore_grid
		{
			get
			{
				return ((this.alloc_rot_flip_ >> 9) & 1U) > 0U;
			}
			set
			{
				this.alloc_rot_flip_ = (this.alloc_rot_flip_ & 4294966783U) | ((value ? 1U : 0U) << 9);
			}
		}

		public bool alloc_multiple_input
		{
			get
			{
				return ((this.alloc_rot_flip_ >> 10) & 1U) > 0U;
			}
			set
			{
				this.alloc_rot_flip_ = (this.alloc_rot_flip_ & 4294966271U) | ((value ? 1U : 0U) << 10);
			}
		}

		public bool no_make_checkpoint
		{
			get
			{
				return ((this.alloc_rot_flip_ >> 11) & 1U) > 0U;
			}
			set
			{
				this.alloc_rot_flip_ = (this.alloc_rot_flip_ & 4294965247U) | ((value ? 1U : 0U) << 11);
			}
		}

		public bool draw_lit_layer
		{
			get
			{
				return ((this.alloc_rot_flip_ >> 12) & 1U) > 0U;
			}
			set
			{
				this.alloc_rot_flip_ = (this.alloc_rot_flip_ & 4294963199U) | ((value ? 1U : 0U) << 12);
			}
		}

		public bool invisible
		{
			get
			{
				return ((this.alloc_rot_flip_ >> 13) & 1U) > 0U;
			}
			set
			{
				this.alloc_rot_flip_ = (this.alloc_rot_flip_ & 4294959103U) | ((value ? 1U : 0U) << 13);
			}
		}

		public bool draw_overtop_layer
		{
			get
			{
				return ((this.alloc_rot_flip_ >> 14) & 1U) > 0U;
			}
			set
			{
				this.alloc_rot_flip_ = (this.alloc_rot_flip_ & 4294950911U) | ((value ? 1U : 0U) << 14);
			}
		}

		public bool no_blur_draw
		{
			get
			{
				return ((this.alloc_rot_flip_ >> 15) & 1U) > 0U;
			}
			set
			{
				this.alloc_rot_flip_ = (this.alloc_rot_flip_ & 4294934527U) | ((value ? 1U : 0U) << 15);
			}
		}

		public bool merge_to_one_layer
		{
			get
			{
				return ((this.alloc_rot_flip_ >> 16) & 1U) > 0U;
			}
			set
			{
				this.alloc_rot_flip_ = (this.alloc_rot_flip_ & 4294901759U) | ((value ? 1U : 0U) << 16);
			}
		}

		public bool is_small_image
		{
			get
			{
				return ((this.alloc_rot_flip_ >> 17) & 1U) > 0U;
			}
			set
			{
				this.alloc_rot_flip_ = (this.alloc_rot_flip_ & 4294705151U) | ((value ? 1U : 0U) << 17);
			}
		}

		public bool is_window
		{
			get
			{
				return (this.alloc_rot_flip_ & 262144U) > 0U;
			}
			set
			{
				this.alloc_rot_flip_ = (this.alloc_rot_flip_ & 4294705151U) | (value ? 262144U : 0U);
			}
		}

		public static int fnSort(M2Puts ImgA, M2Puts ImgB)
		{
			if (ImgA.index < ImgB.index)
			{
				return -1;
			}
			if (ImgA.index != ImgB.index)
			{
				return 1;
			}
			return 0;
		}

		public static int fnSortLay(M2Puts ImgA, M2Puts ImgB)
		{
			if (ImgA.Lay.index != ImgB.Lay.index)
			{
				return ImgB.Lay.index - ImgA.Lay.index;
			}
			return METACImg.fnSort(ImgA, ImgB);
		}

		public static int fnSortStamp(M2StampImage.M2StampImageItem ImgA, M2StampImage.M2StampImageItem ImgB)
		{
			if (ImgA.index < ImgB.index)
			{
				return -1;
			}
			if (ImgA.index != ImgB.index)
			{
				return 1;
			}
			return 0;
		}

		public string name_default_prefix
		{
			get
			{
				return "m" + this.alloc_rot_flip_.ToString("x") + "_";
			}
		}

		public static Vector2[] pointsRotate(M2Chip Cp, string meta_key, global::XX.AIM target, float cx = 14f, float cy = 14f)
		{
			METACImg meta = Cp.getMeta();
			if (Cp.flip && (target == global::XX.AIM.R || target == global::XX.AIM.L))
			{
				target = global::XX.CAim.get_opposite(target);
			}
			if (Cp.rotation != 0)
			{
				int num = Cp.rotation;
				while (--num >= 0)
				{
					target = global::XX.CAim.get_clockwise2(target, true);
				}
			}
			string[] array = meta.Get(meta_key + target.ToString().ToUpper());
			if (array == null)
			{
				return null;
			}
			int num2 = array.Length / 2;
			Vector2[] array2 = new Vector2[num2];
			for (int i = 0; i < num2; i++)
			{
				Vector2 vector = new Vector2(global::XX.X.Nm(array[i * 2], 0f, false) - cx, global::XX.X.Nm(array[i * 2 + 1], 0f, false) - cy);
				if (Cp.flip)
				{
					vector.x *= -1f;
				}
				if (Cp.rotation != 0)
				{
					vector = global::XX.X.ROTV2e(vector, (float)(-(float)Cp.rotation) * 1.5707964f);
				}
				vector.x += cx;
				vector.y += cy;
				array2[Cp.flip ? (num2 - 1 - i) : i] = vector;
			}
			return array2;
		}

		public ushort meta_id;

		public string name = "";

		public const int BASE_PRIORITY = 500;

		private uint alloc_rot_flip_;

		private readonly FlaggerS<uint> Link = new FlaggerS<uint>(null, null);

		private const uint window_bit = 262144U;
	}
}
