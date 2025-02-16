using System;
using System.Collections.Generic;
using Better;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;

namespace XX.mobpxl
{
	public class MobSklt
	{
		private MobSklt(int parts_max)
		{
			this.Msp = new MobSkltPosition("-Base", null);
			this.AParts = new List<SkltParts>(parts_max);
			this.AImgSorted = new List<SkltImage>(10);
			this.OOAccPlt = new BDic<string, SkltPalette>();
			this.OOAccPlt["_"] = new SkltPalette("_", 0);
			this.ASq = new List<SkltSequence>(2);
		}

		internal SkltPalette OACCBase
		{
			get
			{
				return this.OOAccPlt["_"];
			}
		}

		internal MobSklt(string _name, PxlFrame _DefSkltFrame = null)
			: this(10)
		{
			this.name = _name;
			this.DefSkltFrame = _DefSkltFrame;
			if (this.DefSkltFrame != null)
			{
				this.baseline = (float)(-(float)this.DefSkltFrame.pSq.height) * 0.5f;
			}
			for (int i = 0; i < 10; i++)
			{
				this.AParts.Add(new SkltParts(this, (PARTS_TYPE)i, (i == 6) ? 5 : 1, 1));
			}
		}

		public void destruct()
		{
			for (int i = this.ASq.Count - 1; i >= 0; i--)
			{
				this.ASq[i].referred--;
			}
		}

		internal bool readFromBytes(MobGenerator Gen, ByteArray Ba, int vers = 13)
		{
			SkltImageSrc skltImageSrc = null;
			this.need_sort_image = true;
			int num = Ba.readByte();
			while (this.AParts.Count < num)
			{
				this.AParts.Add(null);
			}
			for (int i = 0; i < num; i++)
			{
				SkltParts skltParts = this.AParts[i];
				int num2 = (int)Ba.readUShort();
				short num3 = 0;
				string text = "";
				if (vers >= 2)
				{
					num3 = Ba.readShort();
				}
				if (vers >= 5)
				{
					text = Ba.readPascalString("utf-8", false);
				}
				if (num2 != 0)
				{
					if (skltParts == null)
					{
						skltParts = (this.AParts[i] = new SkltParts(this, (PARTS_TYPE)i, 1, num2));
					}
					skltParts.sort_order = num3;
					skltParts.ptype = text;
					for (int j = 0; j < num2; j++)
					{
						SkltImage skltImage = SkltImage.readFromBytes(Gen, skltParts, Ba, null, vers < 11, vers);
						if (skltImage != null)
						{
							skltParts.AImage.Add(skltImage);
							if (skltImage.Source.is_base_body)
							{
								skltImageSrc = skltImage.Source;
							}
						}
					}
				}
			}
			if (vers < 11)
			{
				MobPCCContainer.readFromBytes(Ba, this, "_", Gen.getBaseCharacter(), false);
			}
			else
			{
				int num4 = Ba.readByte();
				for (int k = 0; k < num4; k++)
				{
					string text2 = Ba.readPascalString("utf-8", false);
					MobPCCContainer.readFromBytes(Ba, this, text2, Gen.getBaseCharacter(), false);
				}
			}
			bool flag = false;
			if (skltImageSrc != null)
			{
				if (Gen.getBasicSkltForBodySrc(skltImageSrc, this) != null)
				{
					flag = true;
				}
				else
				{
					X.de("Joint 対象が不明:" + this.name, null);
				}
			}
			if (vers >= 4)
			{
				if (vers >= 10)
				{
					MobSkltPosition.readFromBytes(Ba, this.Msp);
				}
				for (int l = 0; l < num; l++)
				{
					SkltParts.readFromBytesJoint(this, Ba, (PARTS_TYPE)l, true);
				}
			}
			else
			{
				for (int m = 0; m < num; m++)
				{
					for (int n = 0; n < 2; n++)
					{
						SkltParts.readFromBytesJoint(this, Ba, (PARTS_TYPE)m, n == 0);
					}
				}
			}
			int num5 = Ba.readByte();
			for (int num6 = 0; num6 < num5; num6++)
			{
				SkltSequence sequence = Gen.getSequence(Ba.readPascalString("utf-8", false));
				if (sequence != null)
				{
					this.addAnimSequence(sequence, -1);
				}
			}
			if (vers >= 3)
			{
				this.Size.x = Ba.readFloat();
				this.Size.y = Ba.readFloat();
				this.Size.width = Ba.readFloat();
				this.Size.height = Ba.readFloat();
			}
			else
			{
				this.Size.Set(0f, 0f, 80f, 200f);
			}
			if (vers >= 12)
			{
				int num7 = Ba.readByte();
				if (num7 > 0)
				{
					if (this.OColVari == null)
					{
						this.OColVari = new BDic<string, SkltColVari>(num7);
					}
					for (int num8 = 0; num8 < num7; num8++)
					{
						string text3 = Ba.readPascalString("utf-8", false);
						SkltColVari skltColVari = SkltColVari.readFromBytes(Ba, text3, Gen.getBaseCharacter(), vers);
						if (skltColVari != null)
						{
							this.OColVari[text3] = skltColVari;
						}
					}
				}
			}
			return flag;
		}

		internal SkltParts this[PARTS_TYPE j]
		{
			get
			{
				int count = this.AParts.Count;
				for (int i = 0; i < count; i++)
				{
					SkltParts skltParts = this.AParts[i];
					if (skltParts != null && skltParts.type == j)
					{
						return skltParts;
					}
				}
				return null;
			}
		}

		internal SkltParts MakeParts(PARTS_TYPE j)
		{
			while (this.AParts.Count <= (int)j)
			{
				this.AParts.Add(null);
			}
			if (this.AParts[(int)j] == null)
			{
				this.AParts[(int)j] = new SkltParts(this, j, 1, 1);
			}
			return this.AParts[(int)j];
		}

		public void ClearJoint()
		{
			int count = this.AParts.Count;
			for (int i = 0; i < count; i++)
			{
				SkltParts skltParts = this.AParts[i];
				if (skltParts != null)
				{
					skltParts.ClearJoint();
				}
			}
		}

		public SkltImage getImage(int i, int j, bool slip_to_next_image = false)
		{
			if (slip_to_next_image)
			{
				for (int k = 0; k < this.parts_max; k++)
				{
					SkltParts skltParts = this.AParts[(i + k) % this.parts_max];
					if (skltParts != null && skltParts.AImage.Count != 0)
					{
						return skltParts.AImage[X.MMX(0, j, skltParts.AImage.Count - 1)];
					}
					j = 0;
				}
				return null;
			}
			if (!X.BTW(0f, (float)i, (float)this.parts_max))
			{
				return null;
			}
			SkltParts skltParts2 = this.AParts[i];
			if (skltParts2 == null || !X.BTW(0f, (float)j, (float)skltParts2.AImage.Count))
			{
				return null;
			}
			return skltParts2.AImage[j];
		}

		internal void fineJointPositionOnLoad()
		{
			int count = this.AParts.Count;
			for (int i = 0; i < count; i++)
			{
				SkltParts skltParts = this.AParts[i];
				if (skltParts != null)
				{
					skltParts.fineJointPositionOnLoad();
				}
			}
		}

		public int parts_max
		{
			get
			{
				return this.AParts.Count;
			}
		}

		internal bool fineSort(bool force = false, bool execute_fine_position = true)
		{
			if (!force && !this.need_sort_image_)
			{
				return false;
			}
			this.AImgSorted.Clear();
			this.need_sort_image_ = false;
			int count = this.AParts.Count;
			for (int i = 0; i < count; i++)
			{
				SkltParts skltParts = this.AParts[i];
				if (skltParts != null)
				{
					int count2 = skltParts.AImage.Count;
					int num = i * 256;
					for (int j = 0; j < count2; j++)
					{
						SkltImage skltImage = skltParts.AImage[j];
						skltImage.sort_index = num++;
						this.AImgSorted.Add(execute_fine_position ? skltImage.finePosition() : skltImage);
					}
				}
			}
			this.AImgSorted.Sort((SkltImage SiA, SkltImage SiB) => SiA.calced_sort_order - SiB.calced_sort_order);
			return true;
		}

		internal void fineImagePositionAll()
		{
			this.fineSort(false, false);
			this.Msp.finePositionBase();
			int count = this.AParts.Count;
			for (int i = 0; i < count; i++)
			{
				SkltParts skltParts = this.AParts[i];
				if (skltParts != null)
				{
					skltParts.sortPartsJointAll();
				}
			}
			for (int j = this.AImgSorted.Count - 1; j >= 0; j--)
			{
				this.AImgSorted[j].finePosition();
			}
		}

		public Vector2 calcPositionP2A(PARTS_TYPE _parts, Vector2 Src)
		{
			SkltParts skltParts = this.AParts[(int)_parts];
			MobSkltPosition jointBase = skltParts.getJointBase();
			return MobSkltPosition.calcPositionP2A(Src, skltParts.PartsSize, jointBase);
		}

		internal void createPccAppliedMesh(MobGenerator Gen, SkltRenderTicket Tkt, MeshDrawer Md, SkltImage Ignore_Image = null)
		{
			int count = this.AParts.Count;
			for (int i = 0; i < count; i++)
			{
				SkltParts skltParts = this.AParts[i];
				if (skltParts != null)
				{
					int count2 = skltParts.AImage.Count;
					for (int j = 0; j < count2; j++)
					{
						SkltImage skltImage = skltParts.AImage[j];
						if (skltImage != Ignore_Image)
						{
							skltImage.createPccAppliedMesh(Gen, Tkt, Md, true);
						}
					}
				}
			}
		}

		internal SkltPalette getPalette(string palette_key, bool no_make = false)
		{
			SkltPalette skltPalette;
			if (this.OOAccPlt.TryGetValue(palette_key, out skltPalette))
			{
				return skltPalette;
			}
			if (!no_make || palette_key == "_")
			{
				skltPalette = (this.OOAccPlt[palette_key] = new SkltPalette(palette_key, 0));
			}
			return skltPalette;
		}

		internal void addAnimSequence(SkltSequence Sq, int index = -1)
		{
			this.ASq.Insert((index < 0) ? this.ASq.Count : index, Sq);
			Sq.referred++;
		}

		public SkltSequence getAnim(int i)
		{
			if (this.ASq.Count <= i)
			{
				return null;
			}
			return this.ASq[i];
		}

		public SkltSequence getAnimByType(string type)
		{
			int count = this.ASq.Count;
			for (int i = 0; i < count; i++)
			{
				SkltSequence skltSequence = this.ASq[i];
				if (skltSequence.type == type)
				{
					return skltSequence;
				}
			}
			return null;
		}

		public SkltSequence prepareAnimType(string type, MobGenerator MOBG, string anim_source)
		{
			SkltSequence animByType = this.getAnimByType(type);
			if (animByType != null)
			{
				return animByType;
			}
			SkltSequence skltSequence;
			if (MOBG.getWholeAnimSequence().TryGetValue(anim_source, out skltSequence))
			{
				if (!(skltSequence.type != type))
				{
					this.ASq.Add(skltSequence);
					skltSequence.referred++;
					return skltSequence;
				}
				X.de(string.Concat(new string[] { "Sq ", anim_source, " はtype ", type, " ではありません" }), null);
			}
			else
			{
				X.de("Sq " + anim_source + " が見つかりません", null);
			}
			return null;
		}

		public Vector2 getBasePosFor(MobSkltPosition.IPosSyncable POI)
		{
			if (POI is SkltParts)
			{
				return (POI as SkltParts).getJointBase().PosAbs;
			}
			if (POI is SkltImage)
			{
				return (POI as SkltImage).getJointBase().PosAbs;
			}
			return this.Msp.PosAbs;
		}

		public MobSkltPosition getBaseMspFor(MobSkltPosition.IPosSyncable POI)
		{
			return POI.getMsp();
		}

		internal BDic<string, SkltColVari> getColVariWholeObject()
		{
			return this.OColVari;
		}

		internal SkltColVari GetColVari(string name, bool no_make = true)
		{
			if (TX.noe(name))
			{
				return null;
			}
			if (this.OColVari == null)
			{
				if (!no_make)
				{
					this.OColVari = new BDic<string, SkltColVari>(1);
					return this.OColVari[name] = new SkltColVari(name);
				}
				return null;
			}
			else
			{
				SkltColVari skltColVari;
				if (this.OColVari.TryGetValue(name, out skltColVari))
				{
					return skltColVari;
				}
				if (!no_make)
				{
					return this.OColVari[name] = new SkltColVari(name);
				}
				return null;
			}
		}

		public override string ToString()
		{
			return "<MobSklt>" + this.name;
		}

		public bool is_default_skeleton
		{
			get
			{
				return this.DefSkltFrame != null;
			}
		}

		public bool need_sort_image
		{
			get
			{
				return this.need_sort_image_;
			}
			set
			{
				if (value)
				{
					this.need_sort_image_ = true;
					this.finePartsSize();
				}
			}
		}

		public void finePartsSize()
		{
			int count = this.AParts.Count;
			for (int i = 0; i < count; i++)
			{
				SkltParts skltParts = this.AParts[i];
				if (skltParts != null)
				{
					skltParts.fine_parts_size = true;
				}
			}
		}

		public const int old_skin_max = 2;

		internal string name;

		internal readonly List<SkltParts> AParts;

		internal readonly MobSkltPosition Msp;

		public Rect Size;

		public float baseline = -1000f;

		internal readonly List<SkltImage> AImgSorted;

		internal readonly List<SkltSequence> ASq;

		internal BDic<string, SkltPalette> OOAccPlt;

		private BDic<string, SkltColVari> OColVari;

		private bool need_sort_image_ = true;

		public PxlFrame DefSkltFrame;

		public const string PLT_BASE_KEY = "_";
	}
}
