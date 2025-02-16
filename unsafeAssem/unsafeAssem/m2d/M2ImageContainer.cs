using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Better;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2ImageContainer
	{
		public M2ImageContainer(M2DBase _M2D)
		{
			this.M2D = _M2D;
			this.MIchip = new MImage(null);
			this.Atlas = new M2ImageAtlas(this.M2D, this);
			this.PriorityCon = new M2ImagePriority(this.M2D);
			this.AImgsId = null;
			this.Otag_name_chip_create = new NDic<M2ImageContainer.FnCreateOneChip>("Otag_name_chip_create", 22);
			this.Otag_name_drawer_create = new NDic<M2CImgDrawer.FnCreateDrawer>("Otag_name_drawer_create", 22);
		}

		public float CLEN
		{
			get
			{
				return this.M2D.CLEN;
			}
		}

		public void destruct()
		{
			this.MIchip.Dispose();
			this.Atlas.destruct();
		}

		public void prepareChipsScript()
		{
			if (this.M2D.FnCreateChipPreparation != null)
			{
				this.M2D.FnCreateChipPreparation(this.Otag_name_chip_create, this.Otag_name_drawer_create);
			}
			byte[] array = NKT.readSpecificFileBinary(Path.Combine(Application.streamingAssetsPath, "m2d/__m2d_chips.dat"), 0, 0, false);
			ByteArray byteArray = (this.BaLoad = new ByteArray(array, false, true));
			this.Otag_name_chip_create.scriptFinalize();
			this.Otag_name_drawer_create.scriptFinalize();
			this.chips_id_max = 1U;
			this.loading_version = (byte)byteArray.readByte();
			int num = (int)byteArray.readUInt();
			byteArray.readUInt();
			int num2 = (int)byteArray.readUShort();
			this.Afamily_mem = new List<string>(num2);
			for (int i = 0; i < num2; i++)
			{
				this.Afamily_mem.Add(byteArray.readPascalString("utf-8", false));
			}
			if (this.loading_version >= 7)
			{
				int num3 = (int)byteArray.readUShort();
				this.OOImgs = new BDic<string, M2ImageContainer.ImgDir>(num3);
				for (int j = 0; j < num3; j++)
				{
					string text = byteArray.readPascalString("utf-8", false);
					uint num4 = byteArray.readUInt();
					this.OOImgs[text] = new M2ImageContainer.ImgDir((int)num4);
				}
			}
			else
			{
				this.OOImgs = new BDic<string, M2ImageContainer.ImgDir>(64);
			}
			if (this.loading_version == 1)
			{
				this.AMeta_mem = new List<METACImg>(128);
			}
			else
			{
				int num5 = (int)byteArray.readUShort();
				this.AMeta_mem = new List<METACImg>(num5);
				for (int k = 0; k < num5; k++)
				{
					METACImg metacimg = new METACImg(k, "");
					metacimg.readFromBytes(byteArray, this.loading_version);
					this.AMeta_mem.Add(metacimg);
				}
			}
			this.MetaEmpty = new METACImg(65535, "_");
			this.AImgsId = new IM2CLItem[num];
			this.loading_dir = "";
		}

		public bool progressChipsScriptRead(int count = -1)
		{
			ByteArray baLoad = this.BaLoad;
			if (baLoad == null)
			{
				return false;
			}
			bool flag = this.OPat != null;
			while (baLoad.bytesAvailable > 0UL && (count < 0 || --count >= 0))
			{
				M2ImageContainer.BA_TYPE ba_TYPE = (M2ImageContainer.BA_TYPE)baLoad.readByte();
				if (ba_TYPE == M2ImageContainer.BA_TYPE.DIR)
				{
					this.loading_dir = baLoad.readPascalString("utf-8", false);
				}
				else if (ba_TYPE != (M2ImageContainer.BA_TYPE)0)
				{
					uint num = 1U;
					switch (ba_TYPE)
					{
					case M2ImageContainer.BA_TYPE.CM:
					{
						M2ChipImage m2ChipImage = M2ChipImage.readFromBytes(baLoad, this.loading_version, this, this.loading_dir, true);
						if (m2ChipImage != null)
						{
							num = this.FixId(this.assignNewImage(m2ChipImage, false), false);
						}
						break;
					}
					case M2ImageContainer.BA_TYPE.PAT:
					{
						M2ChipPattern m2ChipPattern = M2ChipPattern.readFromBytes(baLoad, this.loading_version, this, this.loading_dir, flag);
						if (m2ChipPattern != null)
						{
							num = this.FixId(this.assignNewImage(m2ChipPattern, true), false);
						}
						break;
					}
					case M2ImageContainer.BA_TYPE.SM:
					{
						M2SmartChipImage m2SmartChipImage = M2SmartChipImage.readFromBytes(baLoad, this.loading_version, this, this.loading_dir, flag);
						if (m2SmartChipImage != null)
						{
							num = this.FixId(this.assignNewImage(m2SmartChipImage, false), false);
						}
						break;
					}
					case M2ImageContainer.BA_TYPE.STAMP:
					case M2ImageContainer.BA_TYPE.STAMP_BUILT_IN:
					{
						M2StampImage m2StampImage = M2StampImage.readFromBytes(baLoad, this.loading_version, this, this.loading_dir, flag, ba_TYPE == M2ImageContainer.BA_TYPE.STAMP_BUILT_IN);
						if (m2StampImage != null)
						{
							num = this.FixId(this.assignNewImage(m2StampImage, false), false);
						}
						break;
					}
					case M2ImageContainer.BA_TYPE.CM_NESTED:
					{
						M2ChipImageNestedParent m2ChipImageNestedParent = M2ChipImageNestedParent.readFromBytesN(baLoad, this.loading_version, this, this.loading_dir, true);
						if (m2ChipImageNestedParent != null)
						{
							num = this.FixId(this.assignNewImage(m2ChipImageNestedParent, false), false);
							m2ChipImageNestedParent.reserveId();
						}
						break;
					}
					default:
						global::XX.X.de("不明なtype" + ba_TYPE.ToString() + " position: " + (baLoad.position - 1UL).ToString(), null);
						this.BaLoad.position = baLoad.Length;
						return false;
					}
					this.chips_id_max = global::XX.X.Mx(this.chips_id_max, num);
				}
			}
			return baLoad.bytesAvailable != 0UL;
		}

		public M2ChipImage assignNewImage(M2ChipImage I, bool checking = false)
		{
			M2ImageContainer.ImgDir imgDir;
			return this.assignNewImage(out imgDir, I, checking);
		}

		private M2ChipImage assignNewImage(out M2ImageContainer.ImgDir OImgsD, M2ChipImage I, bool checking = false)
		{
			if (!this.OOImgs.TryGetValue(I.dirname, out OImgsD))
			{
				Dictionary<string, M2ImageContainer.ImgDir> ooimgs = this.OOImgs;
				string dirname = I.dirname;
				M2ImageContainer.ImgDir imgDir;
				OImgsD = (imgDir = new M2ImageContainer.ImgDir(64));
				ooimgs[dirname] = imgDir;
			}
			OImgsD[I.basename] = I;
			return I;
		}

		public M2ChipImageMulti assignNewImage(M2ChipImageMulti Pat, bool checking = false)
		{
			return Pat;
		}

		public M2ChipImage Get(string src)
		{
			int num = src.IndexOf("/");
			if (num == -1)
			{
				return null;
			}
			num++;
			string text = TX.slice(src, 0, num);
			int num2 = src.IndexOf(".");
			if (num2 == -1)
			{
				num2 = src.Length;
			}
			string text2 = TX.slice(src, num, num2);
			return this.Get(text, text2);
		}

		public M2ImageContainer.ImgDir GetImgDir(string dirname)
		{
			if (TX.noe(dirname))
			{
				return null;
			}
			M2ImageContainer.ImgDir imgDir;
			if (!this.OOImgs.TryGetValue(dirname, out imgDir))
			{
				return null;
			}
			return imgDir;
		}

		public M2ChipImage Get(string dirname, string basename_noext)
		{
			M2ImageContainer.ImgDir imgDir;
			return this.Get(out imgDir, dirname, basename_noext);
		}

		private M2ChipImage Get(out M2ImageContainer.ImgDir Dir, string dirname, string basename_noext)
		{
			if (!this.OOImgs.TryGetValue(dirname, out Dir))
			{
				return null;
			}
			return global::XX.X.Get<string, M2ChipImage>(Dir, basename_noext);
		}

		public IM2CLItem GetById(uint chip_id)
		{
			if ((ulong)chip_id >= (ulong)((long)this.AImgsId.Length))
			{
				return null;
			}
			return this.AImgsId[(int)chip_id];
		}

		public M2ChipImage load(string src)
		{
			src = this.prefix + src;
			return this.Get(src);
		}

		public void flushAdditinal()
		{
			foreach (KeyValuePair<string, M2ImageContainer.ImgDir> keyValuePair in this.OOImgs)
			{
				foreach (KeyValuePair<string, M2ChipImage> keyValuePair2 in keyValuePair.Value)
				{
					keyValuePair2.Value.flushAdditinal();
				}
			}
		}

		public void flushCreatorFunc()
		{
			foreach (KeyValuePair<string, M2ImageContainer.ImgDir> keyValuePair in this.OOImgs)
			{
				foreach (KeyValuePair<string, M2ChipImage> keyValuePair2 in keyValuePair.Value)
				{
					keyValuePair2.Value.need_fine_create_chip_fn = (keyValuePair2.Value.need_fine_create_drawer_fn = true);
				}
			}
		}

		public string getFamilyName(ushort i)
		{
			if (i == 65535 || !global::XX.X.BTW(0f, (float)i, (float)this.Afamily_mem.Count))
			{
				return "";
			}
			return this.Afamily_mem[(int)i];
		}

		public ushort getFamilyIndex(string t)
		{
			if (TX.noe(t))
			{
				return ushort.MaxValue;
			}
			int num = this.Afamily_mem.IndexOf(t);
			if (num < 0)
			{
				num = this.Afamily_mem.Count;
				this.Afamily_mem.Add(t);
			}
			return (ushort)num;
		}

		public METACImg getCImgMeta(ushort i = 65535)
		{
			if (i == 65535 || !global::XX.X.BTW(0f, (float)i, (float)this.AMeta_mem.Count))
			{
				return this.MetaEmpty;
			}
			return this.AMeta_mem[(int)i];
		}

		public int getMetaCount()
		{
			return this.AMeta_mem.Count;
		}

		public ushort getCImgMetaIndex(ref METACImg Meta)
		{
			if (Meta.isEmpty())
			{
				Meta = this.MetaEmpty;
				return Meta.meta_id;
			}
			Meta.name = this.getIndividualMetaName(Meta, null);
			Meta.meta_id = (ushort)this.AMeta_mem.Count;
			this.AMeta_mem.Add(Meta);
			return Meta.meta_id;
		}

		public string getIndividualMetaName(METACImg Meta, string new_name = null)
		{
			return new_name ?? Meta.name;
		}

		public bool initAtlasMd(MeshDrawer Md, PxlImage PImg)
		{
			M2ImageAtlas.AtlasRect atlasData = this.Atlas.getAtlasData(PImg);
			if (atlasData.valid)
			{
				Md.initForImg(this.Atlas.IMGS.MIchip.Tx, (float)atlasData.x, (float)atlasData.y, (float)atlasData.w, (float)atlasData.h);
				return true;
			}
			return false;
		}

		public void releaseAtlas(bool release_layer = false, List<PxlCharacter> ATargetPxl = null)
		{
			this.Atlas.releaseFnDrawForDropObject();
			this.Atlas.releaseBlurImages();
			foreach (KeyValuePair<string, M2ImageContainer.ImgDir> keyValuePair in this.OOImgs)
			{
				foreach (KeyValuePair<string, M2ChipImage> keyValuePair2 in keyValuePair.Value)
				{
					keyValuePair2.Value.need_fine_atlas = true;
					if (release_layer && ATargetPxl != null && keyValuePair2.Value.SourceLayer != null && ATargetPxl.IndexOf(keyValuePair2.Value.SourceLayer.pChar) >= 0)
					{
						keyValuePair2.Value.releaseSourceLayer();
					}
				}
			}
		}

		public void assignPxlLayerInitialize()
		{
		}

		public M2ChipImage assignPxlLayerToImage(PxlLayer Lay, string dirname, M2ImageAtlas.AtlasRect Atlas, float atlas_rescale_x, float atlas_rescale_y, bool consider_prefix_data = true)
		{
			M2ChipImagePrefixed m2ChipImagePrefixed = (consider_prefix_data ? M2ChipImagePrefixed.checkPrefix(dirname, Lay, Atlas) : default(M2ChipImagePrefixed));
			if (m2ChipImagePrefixed.valid)
			{
				if (this.ABackGround == null)
				{
					this.ABackGround = new List<M2ChipImagePrefixed>(16);
				}
				this.ABackGround.Add(m2ChipImagePrefixed);
				return null;
			}
			if (TX.isStart(Lay.name, "_", 0))
			{
				return null;
			}
			M2ImageContainer.ImgDir imgDir;
			M2ChipImage m2ChipImage;
			if ((m2ChipImage = this.Get(out imgDir, dirname, Lay.name)) != null)
			{
				if (m2ChipImage.chip_id == 0U)
				{
					this.FixId(m2ChipImage, false);
				}
				if (m2ChipImage.is_atlas_empty)
				{
					m2ChipImage.assignLayerAndAtlas(Lay, Atlas, false);
				}
				else if (m2ChipImage.need_fine_atlas || atlas_rescale_x <= 0f)
				{
					m2ChipImage.recreateImageMesh(Atlas);
				}
				else
				{
					if (m2ChipImage.SourceLayer != null && m2ChipImage.SourceLayer != Lay)
					{
						global::XX.X.de(string.Concat(new string[]
						{
							"重複したレイヤー名: ",
							Lay.name,
							" (",
							Lay.pFrm.ToString(),
							")"
						}), null);
					}
					m2ChipImage.AtlasRescale(atlas_rescale_x, atlas_rescale_y);
				}
				return m2ChipImage;
			}
			return null;
		}

		public void assignPxlLayerFinalizeOnPose()
		{
			if (this.ABackGround != null && this.ABackGround.Count > 0)
			{
				int count = this.ABackGround.Count;
				for (int i = 0; i < count; i++)
				{
					M2ChipImagePrefixed m2ChipImagePrefixed = this.ABackGround[i];
					m2ChipImagePrefixed.assignAtlas(this.Get(m2ChipImagePrefixed.dirname, m2ChipImagePrefixed.target_layname));
				}
				this.ABackGround.Clear();
			}
		}

		public void assignPxlLayerFinalize()
		{
			this.assignPxlLayerFinalizeOnPose();
			this.BaLoad = null;
			this.ABackGround = null;
		}

		public uint FixId(IM2CLItem Ip, bool update_id = false)
		{
			if (Ip is M2ChipImage)
			{
				M2ChipImage m2ChipImage = Ip as M2ChipImage;
				if (m2ChipImage.chip_id == 0U)
				{
					M2ChipImage m2ChipImage2 = m2ChipImage;
					uint num = this.chips_id_max + 1U;
					this.chips_id_max = num;
					m2ChipImage2.chip_id = num;
				}
				else if (update_id)
				{
					this.chips_id_max = global::XX.X.Mx(this.chips_id_max, m2ChipImage.chip_id);
				}
				if ((long)this.AImgsId.Length <= (long)((ulong)m2ChipImage.chip_id))
				{
					Array.Resize<IM2CLItem>(ref this.AImgsId, (int)(m2ChipImage.chip_id + 64U));
				}
				this.AImgsId[(int)m2ChipImage.chip_id] = m2ChipImage;
				return m2ChipImage.chip_id;
			}
			if (Ip is M2ChipImageMulti)
			{
				M2ChipImageMulti m2ChipImageMulti = Ip as M2ChipImageMulti;
				if (m2ChipImageMulti.chip_id == 0U)
				{
					M2ChipImageMulti m2ChipImageMulti2 = m2ChipImageMulti;
					uint num = this.chips_id_max + 1U;
					this.chips_id_max = num;
					m2ChipImageMulti2.chip_id = num;
				}
				else if (update_id)
				{
					this.chips_id_max = global::XX.X.Mx(this.chips_id_max, m2ChipImageMulti.chip_id);
				}
				if ((long)this.AImgsId.Length <= (long)((ulong)m2ChipImageMulti.chip_id))
				{
					Array.Resize<IM2CLItem>(ref this.AImgsId, (int)(m2ChipImageMulti.chip_id + 64U));
				}
				this.AImgsId[(int)m2ChipImageMulti.chip_id] = m2ChipImageMulti;
				return m2ChipImageMulti.chip_id;
			}
			return 0U;
		}

		public void reserveId(uint chip_id)
		{
			this.chips_id_max = global::XX.X.Mx(this.chips_id_max, chip_id);
		}

		public void SetFavForInputtable(IM2CLItem Item, bool flag)
		{
			if (Item is M2ChipImage)
			{
				(Item as M2ChipImage).favorited = flag;
			}
			if (Item is M2ChipImageMulti)
			{
				(Item as M2ChipImageMulti).favorited = flag;
			}
		}

		public void LinkSourcesForInputtable(IM2CLItem Item, string key, bool need_fine_editor_buttons)
		{
		}

		public void UnlinkSourcesForInputtable(IM2CLItem Item, string key, bool need_fine_editor_buttons)
		{
		}

		public M2ImageContainer.FnCreateOneChip getCreateOneChipFunc(M2ChipImage Img)
		{
			BDic<string, string[]> dataObject = Img.Meta.getDataObject();
			if (dataObject != null)
			{
				foreach (KeyValuePair<string, string[]> keyValuePair in dataObject)
				{
					M2ImageContainer.FnCreateOneChip fnCreateOneChip = global::XX.X.Get<string, M2ImageContainer.FnCreateOneChip>(this.Otag_name_chip_create, keyValuePair.Key);
					if (fnCreateOneChip != null)
					{
						return fnCreateOneChip;
					}
				}
			}
			return global::XX.X.Get<string, M2ImageContainer.FnCreateOneChip>(this.Otag_name_chip_create, "_");
		}

		public M2CImgDrawer.FnCreateDrawer getCreateOneDrawerFunc(M2ChipImage Img)
		{
			BDic<string, string[]> dataObject = Img.Meta.getDataObject();
			if (dataObject != null)
			{
				foreach (KeyValuePair<string, string[]> keyValuePair in dataObject)
				{
					M2CImgDrawer.FnCreateDrawer fnCreateDrawer = global::XX.X.Get<string, M2CImgDrawer.FnCreateDrawer>(this.Otag_name_drawer_create, keyValuePair.Key);
					if (fnCreateDrawer != null)
					{
						return fnCreateDrawer;
					}
				}
			}
			return global::XX.X.Get<string, M2CImgDrawer.FnCreateDrawer>(this.Otag_name_drawer_create, "_");
		}

		public readonly M2DBase M2D;

		private BDic<string, M2ImageContainer.ImgDir> OOImgs;

		private BDic<string, M2ChipImageMulti> OPat;

		private List<M2ChipImagePrefixed> ABackGround;

		private IM2CLItem[] AImgsId;

		private readonly NDic<M2ImageContainer.FnCreateOneChip> Otag_name_chip_create;

		private readonly NDic<M2CImgDrawer.FnCreateDrawer> Otag_name_drawer_create;

		private List<string> Afamily_mem;

		private List<METACImg> AMeta_mem;

		private METACImg MetaEmpty;

		private ByteArray BaLoad;

		public byte loading_version;

		private string loading_dir;

		public string prefix = "";

		public const string OBJ_CHIP_DIR = "obj/";

		public const string DUMMY_CHIP_BASENAME = "DUMMY_CHIP_IMAGE";

		public M2ImagePriority PriorityCon;

		public readonly M2ImageAtlas Atlas;

		public readonly MImage MIchip;

		public static readonly Regex RegAutoPatNestedSuffix = new Regex("_npat(?:(\\d+)x(\\d+))?(?:_\\d+)?(?:\\.png)?$");

		public static readonly Regex RegAutoPatSuffix = new Regex("_pat([A-Z]*)_(\\d+)(?:\\.png)?$");

		private static readonly Regex RegSuffixNumPng = new Regex("_(\\d+)(?:\\.png)?$");

		private static readonly Regex RegHeadW = new Regex("^(\\w+)");

		private uint chips_id_max;

		public delegate M2Chip FnCreateOneChip(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img);

		public delegate void FnCreateChipPrepartion(BDic<string, M2ImageContainer.FnCreateOneChip> Otg, BDic<string, M2CImgDrawer.FnCreateDrawer> Odr);

		public class ImgDir : BDic<string, M2ChipImage>
		{
			public ImgDir(int capacity)
				: base(capacity)
			{
			}
		}

		public enum BA_TYPE
		{
			DIR = 1,
			CM,
			PAT,
			SM,
			STAMP,
			CM_NESTED,
			STAMP_BUILT_IN
		}
	}
}
