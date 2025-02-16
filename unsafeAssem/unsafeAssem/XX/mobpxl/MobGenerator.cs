using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Better;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;

namespace XX.mobpxl
{
	public class MobGenerator
	{
		public string pathGenSaved
		{
			get
			{
				return Path.Combine(Application.streamingAssetsPath, "mobg.bytes");
			}
		}

		public MobGenerator(bool create_mi = false)
		{
			if (create_mi)
			{
				this.MI = new MImage(null);
			}
		}

		public virtual void destruct()
		{
			this.CalcAtlas.destruct();
			this.MdBuf.destruct();
			IN.DestroyOne(this.MtrMd);
			if (this.MI != null)
			{
				MTRX.releaseMI(this.BasePcr, true);
			}
		}

		public PxlCharacter prepareResourcePxl(bool raw_pxl = false)
		{
			if (this.BasePcr == null)
			{
				this.BasePcr = MTRX.loadMtiPxc("sub_mob_general", "MapChars/sub_mob_general.pxls", "MOBG", false, true, false);
				this.OImgSource = new BDic<PxlImage, SkltImageSrc>();
				this.OAnimSq = new BDic<string, SkltSequence>();
				if (this.CalcAtlas != null)
				{
					this.CalcAtlas.destruct();
				}
				this.CalcAtlas = new RectAtlasTexture(512, 512, "", false, 0, RenderTextureFormat.ARGB32);
				this.OAtlasTicket = new BDic<SkltRenderKey, SkltRenderTicket>(6);
				this.PCC = new MobPCCContainer();
				if (this.MdBuf == null)
				{
					this.MtrMd = this.MtrMd ?? MTRX.newMtr(MTRX.getShd("Buffer/JustPaste"));
					this.MtrMd.EnableKeyword("NO_PIXELSNAP");
					int num = 1;
					for (int i = 0; i < num; i++)
					{
						MeshDrawer meshDrawer = new MeshDrawer(null, 4, 6)
						{
							draw_gl_only = true
						};
						meshDrawer.activate("GenMd-" + i.ToString(), this.MtrMd, true, MTRX.ColWhite, null);
						this.MdBuf = meshDrawer;
					}
				}
				if (this.MI != null)
				{
					this.MI.Tx = this.CalcAtlas.Tx;
				}
			}
			return this.BasePcr;
		}

		public bool sklt_loaded
		{
			get
			{
				return this.OSklt != null;
			}
		}

		internal static string[] Abuf_atc_header_name
		{
			get
			{
				if (MobGenerator.Abuf_atc_header_name_ == null)
				{
					MobGenerator.Abuf_atc_header_name_ = new string[9];
					for (int i = 8; i >= 0; i--)
					{
						string[] abuf_atc_header_name_ = MobGenerator.Abuf_atc_header_name_;
						int num = i;
						ATC_TYPE atc_TYPE = (ATC_TYPE)i;
						abuf_atc_header_name_[num] = atc_TYPE.ToString() + "_";
					}
				}
				return MobGenerator.Abuf_atc_header_name_;
			}
		}

		public void prepareParts()
		{
			if (this.OSklt != null)
			{
				return;
			}
			this.OSklt = new BDic<string, MobSklt>();
			this.BaseFirstPose = null;
			int num = this.BasePcr.countPoses();
			for (int i = 0; i < num; i++)
			{
				PxlPose pose = this.BasePcr.getPose(i);
				if (!TX.isStart(pose.title, "_", 0) && !TX.isStart(pose.title, "PLT", 0))
				{
					if (this.BaseFirstPose == null)
					{
						this.BaseFirstPose = pose;
					}
					PxlSequence sequence = pose.getSequence(0);
					int num2 = sequence.countFrames();
					for (int j = 0; j < num2; j++)
					{
						PxlFrame frame = sequence.getFrame(j);
						string name = frame.name;
						if (TX.noe(name))
						{
							X.de("フレームの名前を設定して下さい。 " + frame.ToString(), null);
						}
						else if (this.OSklt.ContainsKey(name))
						{
							X.de("フレーム名が重複しています " + frame.ToString(), null);
						}
						else
						{
							Dictionary<string, MobSklt> osklt = this.OSklt;
							string text = name;
							MobSklt mobSklt = new MobSklt(name, frame);
							mobSklt.Size = new Rect(0f, 0f, (float)pose.width, (float)pose.height);
							MobSklt mobSklt2 = mobSklt;
							osklt[text] = mobSklt;
							MobSklt mobSklt3 = mobSklt2;
							if (this.DefaultSklt == null)
							{
								this.DefaultSklt = mobSklt3;
							}
							string text2 = name;
							if (REG.match(text2, REG.RegFirstAlphabetUnderBar))
							{
								text2 = REG.R1;
							}
							int num3 = frame.countLayers();
							for (int k = 0; k < 2; k++)
							{
								for (int l = 0; l < num3; l++)
								{
									PxlLayer layer = frame.getLayer(l);
									if (!layer.isGroup())
									{
										if (TX.isStart(layer.name, "point_", 0))
										{
											if (k != 1)
											{
												PARTS_TYPE parts_TYPE = MobGenerator.name2PT(layer.name, 6);
												if (parts_TYPE == PARTS_TYPE._OTHER || parts_TYPE == PARTS_TYPE.BODY)
												{
													X.de("point の接続が不明: " + layer.ToString(), null);
												}
												else
												{
													new SkltJoint(layer.name, layer.x, layer.y, layer.rotR, mobSklt3, parts_TYPE);
												}
											}
										}
										else if (k != 0)
										{
											PARTS_TYPE parts_TYPE2 = MobGenerator.name2PT(layer.name, 0);
											SkltImageSrc skltImageSrc;
											if (!this.assignAttachLayer(layer, text2, out skltImageSrc))
											{
												PARTS_TYPE parts_TYPE = parts_TYPE2;
												if (parts_TYPE == PARTS_TYPE._OTHER)
												{
													if (!layer.isImport() || !(layer.name == "noel"))
													{
														X.de("パーツ画像の定義エラー: " + layer.ToString(), null);
														goto IL_02F0;
													}
													goto IL_02F0;
												}
												else
												{
													string text3 = "";
													if (REG.match(layer.name, this.RegMainBodyHeader))
													{
														text3 = REG.R2;
													}
													if (!this.OImgSource.TryGetValue(layer.Img, out skltImageSrc))
													{
														skltImageSrc = (this.OImgSource[layer.Img] = new SkltImageSrc(layer, text3, text2, parts_TYPE, ATC_TYPE._OTHER));
													}
												}
											}
											SkltParts skltParts = mobSklt3[skltImageSrc.follow_to];
											if (skltParts != null)
											{
												skltParts.addImageOnLoad(layer, skltImageSrc);
											}
										}
									}
									IL_02F0:;
								}
								if (k == 0)
								{
									mobSklt3.fineJointPositionOnLoad();
								}
							}
						}
					}
				}
			}
			this.pcr_base_parts_bits = 0U;
			num = this.BasePcr.APartsInfo.Length;
			for (int m = 0; m < num; m++)
			{
				if (MobGenerator.is_base_oacc_parts(this.BasePcr.APartsInfo[m].name))
				{
					this.pcr_base_parts_bits |= 1U << m;
				}
			}
			this.PCC.name = "_";
			this.PCC.chr_name = this.BasePcr.title;
			this.PCC.Init(this, this.BasePcr.getExternalTextureArray()[1].Image);
			this.loadMobgData(out this.first_sklt_key);
			this.ClearTexture(false);
		}

		public BDic<string, MobSklt> getWholeSkltObject()
		{
			return this.OSklt;
		}

		public MobSklt getSklt(string s)
		{
			return X.Get<string, MobSklt>(this.OSklt, s);
		}

		public bool ClearTexture(bool completely = false)
		{
			bool flag = false;
			foreach (KeyValuePair<SkltRenderKey, SkltRenderTicket> keyValuePair in this.OAtlasTicket)
			{
				if (keyValuePair.Value.atlas_created)
				{
					flag = true;
				}
				keyValuePair.Value.Clear();
			}
			if (completely)
			{
				this.OAtlasTicket.Clear();
			}
			this.CalcAtlas.Clear(512, 512);
			this.fineMITexture();
			return flag;
		}

		public BDic<PxlImage, SkltImageSrc> getWholeImageSrcObjectA()
		{
			return this.OImgSource;
		}

		internal SkltImageSrc getImageSrc(string idstr, bool no_error = false)
		{
			PxlImage image = this.BasePcr.getImage(idstr);
			if (image == null)
			{
				if (!no_error)
				{
					X.de("不明なイメージ " + idstr, null);
				}
				return null;
			}
			return this.getImageSrc(image);
		}

		internal SkltImageSrc getImageSrc(PxlImage Img)
		{
			if (Img == null)
			{
				return null;
			}
			return X.Get<PxlImage, SkltImageSrc>(this.OImgSource, Img);
		}

		internal MobSklt getBasicSkltForBodySrc(SkltImageSrc Src, MobSklt Target = null)
		{
			foreach (KeyValuePair<string, MobSklt> keyValuePair in this.OSklt)
			{
				MobSklt value = keyValuePair.Value;
				if (value.is_default_skeleton && value.AParts.Count > 6)
				{
					SkltParts skltParts = value.AParts[6];
					if (skltParts != null)
					{
						int count = skltParts.AImage.Count;
						for (int i = 0; i < count; i++)
						{
							SkltImageSrc source = skltParts.AImage[i].Source;
							if (source.is_skin && source.isFamily(Src))
							{
								if (Target != null)
								{
									int num = X.Mn(Target.AParts.Count, value.AParts.Count);
									Target.ClearJoint();
									for (int j = 0; j < num; j++)
									{
										SkltParts skltParts2 = Target.AParts[j];
										SkltParts skltParts3 = value.AParts[j];
										if (skltParts2 != null && skltParts3 != null)
										{
											skltParts2.ReplaceJoint(skltParts3);
										}
									}
									Target.baseline = value.baseline;
								}
								return value;
							}
						}
					}
				}
			}
			return null;
		}

		protected virtual bool fineMITexture()
		{
			if (this.MI != null && this.MI.Tx != this.CalcAtlas.Tx)
			{
				this.MI.Tx = this.CalcAtlas.Tx;
				return true;
			}
			return false;
		}

		public RectAtlasTexture getAtlasCalculator()
		{
			return this.CalcAtlas;
		}

		public bool alreadyAtlasCreated(MobSklt Sklt, string colvari_key)
		{
			return this.getAtlasCreatedTicket(Sklt, colvari_key) != null;
		}

		public SkltRenderTicket getAtlasCreatedTicket(MobSklt Sklt, string colvari_key)
		{
			SkltRenderKey skltRenderKey = new SkltRenderKey(Sklt, colvari_key);
			SkltRenderTicket skltRenderTicket;
			if (!this.OAtlasTicket.TryGetValue(skltRenderKey, out skltRenderTicket) || !skltRenderTicket.atlas_created)
			{
				return null;
			}
			return skltRenderTicket;
		}

		internal SkltRenderTicket createAtlas(MobSklt Sklt, string colvari_key)
		{
			SkltRenderKey skltRenderKey = new SkltRenderKey(Sklt, colvari_key);
			SkltRenderTicket skltRenderTicket;
			if (!this.OAtlasTicket.TryGetValue(skltRenderKey, out skltRenderTicket))
			{
				skltRenderTicket = (this.OAtlasTicket[skltRenderKey] = new SkltRenderTicket());
			}
			skltRenderTicket.createAtlas(this.CalcAtlas, Sklt);
			this.fineMITexture();
			SkltImage skltImage = null;
			Sklt.createPccAppliedMesh(this, skltRenderTicket, this.MdBuf, (this.Apply_Buffer_Redrawing != null) ? skltImage : null);
			this.MdBuf.clearSimple();
			return skltRenderTicket;
		}

		public RenderTexture GetAtlasTexture()
		{
			return this.CalcAtlas.Tx;
		}

		public MobPCCContainer get_PCC()
		{
			return this.PCC;
		}

		internal RenderTexture GetAtlasTextureForPccApplyInit(out MeshDrawer MdForPc, string target_parts_key, RenderTexture BufTexture)
		{
			MdForPc = this.MdBuf;
			bool flag = BufTexture == null;
			RenderTexture renderTexture = ((this.Apply_Buffer_Redrawing == null) ? this.CalcAtlas.Tx : this.Apply_Buffer_Redrawing);
			GL.LoadProjectionMatrix(Matrix4x4.Ortho(0f, (float)renderTexture.width, 0f, (float)renderTexture.height, -1f, 100f));
			if (!flag)
			{
				return renderTexture;
			}
			Texture image = this.BasePcr.getExternalTextureArray()[0].Image;
			MdForPc.initForImgAndTexture("_MainTex", image);
			Graphics.SetRenderTarget(renderTexture);
			this.MtrMd.SetPass(0);
			BLIT.RenderToGLImmediate(MdForPc, MdForPc.getTriMax());
			Graphics.SetRenderTarget(null);
			Vector3[] vertexArray = MdForPc.getVertexArray();
			Vector2[] uvArray = MdForPc.getUvArray();
			float num = 1f / (float)renderTexture.width;
			float num2 = 1f / (float)renderTexture.height;
			int vertexMax = MdForPc.getVertexMax();
			for (int i = 0; i < vertexMax; i++)
			{
				Vector3 vector = vertexArray[i];
				uvArray[i].Set(vector.x * num, vector.y * num2);
			}
			return renderTexture;
		}

		public static bool is_base_oacc_parts(string parts_name)
		{
			if (parts_name != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(parts_name);
				if (num <= 2250715248U)
				{
					if (num != 862676408U)
					{
						if (num != 1060812303U)
						{
							if (num != 2250715248U)
							{
								return false;
							}
							if (!(parts_name == "b_matsuge"))
							{
								return false;
							}
						}
						else if (!(parts_name == "hB"))
						{
							return false;
						}
					}
					else if (!(parts_name == "skin"))
					{
						return false;
					}
				}
				else if (num <= 3827966920U)
				{
					if (num != 2739729488U)
					{
						if (num != 3827966920U)
						{
							return false;
						}
						if (!(parts_name == "eye_white"))
						{
							return false;
						}
					}
					else if (!(parts_name == "hair_b"))
					{
						return false;
					}
				}
				else if (num != 3876335077U)
				{
					if (num != 3977000791U)
					{
						return false;
					}
					if (!(parts_name == "h"))
					{
						return false;
					}
				}
				else if (!(parts_name == "b"))
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public BDic<string, SkltSequence> getWholeAnimSequence()
		{
			return this.OAnimSq;
		}

		public SkltSequence getSequence(string sq)
		{
			return X.Get<string, SkltSequence>(this.OAnimSq, sq);
		}

		internal void loadMobgData(out string first_sklt_key)
		{
			first_sklt_key = null;
			if (!File.Exists(this.pathGenSaved))
			{
				return;
			}
			ByteArray byteArray = new ByteArray(NKT.readSpecificFileBinary(this.pathGenSaved, 0, 0, false), false, false);
			int num = byteArray.readByte();
			int num2 = (int)byteArray.readUShort();
			for (int i = 0; i < num2; i++)
			{
				string text = byteArray.readPascalString("utf-8", false);
				if (!TX.noe(text))
				{
					SkltSequence skltSequence = new SkltSequence(text, null, byteArray, num);
					this.OAnimSq[text] = skltSequence;
				}
			}
			num2 = (int)byteArray.readUShort();
			first_sklt_key = byteArray.readPascalString("utf-8", false);
			for (int j = 0; j < num2; j++)
			{
				string text2 = byteArray.readString("utf-8", false);
				if (!TX.noe(text2))
				{
					MobSklt mobSklt = new MobSklt(text2, null);
					if (mobSklt.readFromBytes(this, byteArray, num))
					{
						this.OSklt[text2] = mobSklt;
					}
				}
			}
			num2 = (int)byteArray.readUShort();
			for (int k = 0; k < num2; k++)
			{
				uint num3 = byteArray.readUInt();
				double num4 = byteArray.readDouble();
				PxlImage image = this.BasePcr.getImage(PxlImage.getIdString(num3, num4));
				SkltImageSrc.readFromBytes((image != null) ? X.Get<PxlImage, SkltImageSrc>(this.OImgSource, image) : null, byteArray);
			}
		}

		public PxlCharacter getBaseCharacter()
		{
			return this.BasePcr;
		}

		public int pxlparts_max
		{
			get
			{
				return this.BasePcr.APartsInfo.Length;
			}
		}

		public PxlPartsInfo getPxlParts(int i)
		{
			return this.BasePcr.APartsInfo[i];
		}

		public PxlPartsInfo getPxlParts(string s)
		{
			int partsInfoIndex = this.BasePcr.getPartsInfoIndex(s, true);
			if (partsInfoIndex < 0)
			{
				return null;
			}
			return this.BasePcr.APartsInfo[partsInfoIndex];
		}

		internal static PARTS_TYPE name2PT(string t, int si = 0)
		{
			if (TX.isStart(t, "ra2_", si) || TX.isMatch(t, "ra2", si))
			{
				return PARTS_TYPE.RARM2;
			}
			if (TX.isStart(t, "la2_", si) || TX.isMatch(t, "la2", si))
			{
				return PARTS_TYPE.LARM2;
			}
			if (TX.isStart(t, "rf2_", si) || TX.isMatch(t, "rf2", si))
			{
				return PARTS_TYPE.RFOOT2;
			}
			if (TX.isStart(t, "lf2_", si) || TX.isMatch(t, "lf2", si))
			{
				return PARTS_TYPE.LFOOT2;
			}
			if (TX.isStart(t, "ra_", si) || TX.isMatch(t, "ra", si))
			{
				return PARTS_TYPE.RARM;
			}
			if (TX.isStart(t, "la_", si) || TX.isMatch(t, "la", si))
			{
				return PARTS_TYPE.LARM;
			}
			if (TX.isStart(t, "rf_", si) || TX.isMatch(t, "rf", si))
			{
				return PARTS_TYPE.RFOOT;
			}
			if (TX.isStart(t, "lf_", si) || TX.isMatch(t, "lf", si))
			{
				return PARTS_TYPE.LFOOT;
			}
			if (TX.isStart(t, "b_", si) || TX.isMatch(t, "b", si))
			{
				return PARTS_TYPE.BODY;
			}
			if (TX.isStart(t, "f_", si) || TX.isMatch(t, "f", si) || TX.isStart(t, "face", si))
			{
				return PARTS_TYPE.FACE;
			}
			return PARTS_TYPE._OTHER;
		}

		private PARTS_TYPE getDefaultFollowTo(ATC_TYPE atc)
		{
			switch (atc)
			{
			case ATC_TYPE.h:
				return PARTS_TYPE.FACE;
			case ATC_TYPE.hb:
				return PARTS_TYPE.FACE;
			case ATC_TYPE.mouse:
				return PARTS_TYPE.FACE;
			case ATC_TYPE.shoe:
				return PARTS_TYPE.LFOOT2;
			case ATC_TYPE.eyeb:
				return PARTS_TYPE.FACE;
			}
			return PARTS_TYPE.BODY;
		}

		private bool assignAttachLayer(PxlLayer L, string img_header, out SkltImageSrc ImgSrc)
		{
			string name = L.name;
			int num = 9;
			ImgSrc = null;
			for (int i = 0; i < num; i++)
			{
				if (TX.isStart(name, MobGenerator.Abuf_atc_header_name[i], 0))
				{
					ATC_TYPE atc_TYPE = (ATC_TYPE)i;
					PARTS_TYPE parts_TYPE = PARTS_TYPE._OTHER;
					string text = null;
					if (REG.match(name, this.RegAttachHeader) && REG.R1 == MobGenerator.Abuf_atc_header_name[i])
					{
						parts_TYPE = MobGenerator.name2PT(REG.R2, 0);
						if (parts_TYPE != PARTS_TYPE._OTHER)
						{
							text = REG.R3;
						}
					}
					if (text == null)
					{
						text = TX.slice(name, MobGenerator.Abuf_atc_header_name[i].Length);
					}
					if (!TX.noe(text))
					{
						if (parts_TYPE == PARTS_TYPE._OTHER)
						{
							parts_TYPE = this.getDefaultFollowTo(atc_TYPE);
						}
						if (!this.OImgSource.TryGetValue(L.Img, out ImgSrc))
						{
							ImgSrc = (this.OImgSource[L.Img] = new SkltImageSrc(L, text, img_header, parts_TYPE, atc_TYPE));
						}
						return true;
					}
					X.de("atc には名称が必要です: " + L.ToString(), null);
				}
			}
			return false;
		}

		public const int old_skin_max = 2;

		public const string gen_pxl = "sub_mob_general";

		private PxlCharacter BasePcr;

		internal uint pcr_base_parts_bits;

		public PxlPose BaseFirstPose;

		private Color32[,] APartColorsSource_;

		internal MobSklt DefaultSklt;

		private BDic<string, MobSklt> OSklt;

		private BDic<PxlImage, SkltImageSrc> OImgSource;

		private BDic<string, SkltSequence> OAnimSq;

		protected RectAtlasTexture CalcAtlas;

		private BDic<SkltRenderKey, SkltRenderTicket> OAtlasTicket;

		private MeshDrawer MdBuf;

		public string first_sklt_key;

		private Material MtrMd;

		private MobPCCContainer PCC;

		internal SkltColVari CurColVari;

		private static string[] Abuf_atc_header_name_;

		private readonly Regex RegAttachHeader = new Regex("^([a-zA-Z]+_)_*([a-zA-Z]+[0-9]*)_+(\\w+)");

		private readonly Regex RegMainBodyHeader = new Regex("^([a-zA-Z]+[0-9]*)_+(\\w+)");

		public readonly MImage MI;

		private RenderTexture Apply_Buffer_Redrawing;

		public const int MOBG_VERS = 13;
	}
}
