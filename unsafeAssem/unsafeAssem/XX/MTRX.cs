using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Better;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public sealed class MTRX
	{
		public static string Read(string path_from_data_dir, string def_str = "")
		{
			TextAsset textAsset = Resources.Load<TextAsset>("Basic/Data/" + path_from_data_dir);
			if (!(textAsset != null))
			{
				return def_str;
			}
			return textAsset.text;
		}

		public static Material newMtr(string shader_name)
		{
			return new Material(MTRX.getShd(shader_name));
		}

		public static Material newMtr(Shader Shd)
		{
			return new Material(Shd);
		}

		public static Material newMtr(Material MtrSrc)
		{
			return new Material(MtrSrc);
		}

		public static void init1()
		{
			if (MTRX.loaded > 0)
			{
				return;
			}
			Bench.mark("MTRX - init1", true, false);
			MTRX.loaded = 1;
			MTI.initMTI();
			PxlsLoader.loadSpeed = 64f;
			PxlMeshDrawer.do_not_draw_hidden_layer = true;
			EfParticle.initEfParticle();
			MTRX.cola = new C32();
			MTRX.colb = new C32();
			MTRX.OOMtr = new BDic<int, BDic<Shader, Material>>(16);
			MTRX.MtiShader = new MTI("MTI_Shader", null);
			MTRX.MtiShader.addLoadKey("_", true);
			MTRX.ChrL = new BMListChars(0, 0, 0f);
			MTRX.ChrLb = new BMListChars(0, 0, 0f);
			MTRX.ChrM = new BMListChars(0, 0, 0f);
			MTRX.ChrM.marginw = 1;
			MTRX.Halo = new HaloDrawer(-1f, -1f, -1f, -1f, -1f, -1f);
			MTRX.Drip = new DripDrawer(-1f);
			MTRX.Elec = new ElecDrawer();
			MTRX.Wind = new WindDrawer();
			MTRX.UniKira = new UniKiraDrawer();
			MTRX.UniBright = new UniBrightDrawer();
			MTRX.BzPict = new BezierPictDrawer();
			MTRX.Expander = new ExpandImageDrawer();
			MTRX.OMeshImages = new BDic<string, PxlFrame>();
			MTRX.OMI = new BDic<PxlCharacter, MImage>(3);
			MTRX.PxlIcon = MTRX.loadMtiPxc("_icons", "Pxl/_icons.pxls", "_", false, false);
			MTRX.PmdM2Zero = new PhysicsMaterial2D();
			MTRX.PmdM2Zero.friction = 0f;
			MTRX.PmdM2Zero.bounciness = 0f;
			MTRX.PmdM2Bouncy = new PhysicsMaterial2D();
			MTRX.PmdM2Bouncy.friction = 0.125f;
			MTRX.PmdM2Bouncy.bounciness = 0.125f;
			MTRX.PmdM2Enemy = new PhysicsMaterial2D();
			MTRX.PmdM2Enemy.friction = 0f;
			MTRX.PmdM2Enemy.bounciness = 0f;
			MTRX.PmdM2Huttobi = new PhysicsMaterial2D();
			MTRX.PmdM2Huttobi.friction = 0.3f;
			MTRX.PmdM2Huttobi.bounciness = 0.65f;
			MTRX.PmdM2Fric = new PhysicsMaterial2D();
			MTRX.PmdM2Fric.friction = 5f;
			MTRX.PmdM2Fric.bounciness = 0.0625f;
			SND.loadSheets("1", "MTRX");
		}

		public static PxlCharacter loadMtiPxc(string pxl_name, string pxls_path, string image_mti_load_key, bool autoFlipX = true, bool load_external = true)
		{
			MTIOneImage mtioneImage;
			return MTRX.loadMtiPxc(out mtioneImage, pxl_name, pxls_path, image_mti_load_key, autoFlipX, load_external);
		}

		public static PxlCharacter loadMtiPxc(out MTIOneImage MtiI, string pxl_name, string pxls_path, string image_mti_load_key, bool autoFlipX = true, bool load_external = true)
		{
			PxlCharacter pxlCharacter = PxlsLoader.getPxlCharacter(pxl_name);
			if (pxlCharacter == null)
			{
				using (MTI mti = new MTI(pxls_path, "_"))
				{
					pxlCharacter = PxlsLoader.loadCharacterASync(pxl_name, mti.LoadBytes(X.basename(pxls_path)), null, 64f, autoFlipX);
					pxlCharacter.external_png_header = pxls_path;
					pxlCharacter.no_load_external_texture_on_first = true;
				}
			}
			MtiI = null;
			if (load_external)
			{
				string text = pxls_path + ".bytes.texture_0";
				MtiI = MTI.LoadContainerOneImage(text, image_mti_load_key, null);
				MtiI.ReplaceExternalPngForPxl(pxlCharacter, true);
				MTRX.assignMI(pxlCharacter, MtiI.MI);
			}
			return pxlCharacter;
		}

		private static void init2()
		{
			Bench.mark("MTRX - init2", true, false);
			MTRX.ShaderGDTPAdd = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTPAdd");
			MTRX.ShaderGDTAdd = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTAdd");
			MTRX.ShaderGDTAddZT = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTAddZT");
			MTRX.ShaderGDTSub = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTSub");
			MTRX.ShaderGDTZW = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTZW");
			MTRX.ShaderGDTZT = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTZT");
			MTRX.ShaderGDTP2 = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTP2");
			MTRX.ShaderGDTP3 = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTP3");
			MTRX.ShaderGDTST = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTST");
			MTRX.ShaderGDTFont = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTFont");
			MTRX.ShaderGDTFontBorder8 = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTFontBorder8");
			MTRX.ShaderGDTSTFont = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTSTFont");
			MTRX.ShaderGDTSTFontBorder8 = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTSTFontBorder8");
			MTRX.ShaderGDT = MTRX.MtiShader.LoadShader("Hachan/ShaderGDT");
			MTRX.ShaderGDTMul = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTMul");
			MTRX.ShaderGDTBorder8 = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTBorder8");
			MTRX.ShaderGDTSTBorder8 = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTSTBorder8");
			MTRX.ShaderGDTNormalGlow = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTNormalGlow");
			MTRX.ShaderGDTNormalBlur = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTNormalBlur");
			MTRX.ShaderGDTAddGlow = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTAddGlow");
			MTRX.ShaderGDTMask = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTMask");
			MTRX.ShaderAlphaSplice = MTRX.MtiShader.LoadShader("Hachan/ShaderAlphaSplice");
			MTRX.ShaderGDTErase = MTRX.MtiShader.LoadShader("Hachan/ShaderGDTErase");
			MTRX.ShaderMesh = MTRX.MtiShader.LoadShader("Hachan/ShaderMesh");
			MTRX.ShaderMeshST = MTRX.MtiShader.LoadShader("Hachan/ShaderMeshST");
			MTRX.ShaderMeshZW = MTRX.MtiShader.LoadShader("Hachan/ShaderMeshZW");
			MTRX.ShaderMeshMask = MTRX.MtiShader.LoadShader("Hachan/ShaderMeshMask");
			MTRX.ShaderMeshMaskTransparent = MTRX.MtiShader.LoadShader("Hachan/ShaderMeshMaskTransparent");
			MTRX.ShaderMeshSub = MTRX.MtiShader.LoadShader("Hachan/ShaderMeshSub");
			MTRX.ShaderMeshAdd = MTRX.MtiShader.LoadShader("Hachan/ShaderMeshAdd");
			MTRX.ShaderMeshAddZT = MTRX.MtiShader.LoadShader("Hachan/ShaderMeshAddZT");
			MTRX.ShaderMeshMul = MTRX.MtiShader.LoadShader("Hachan/ShaderMeshMul");
			MTRX.MtrSpineDefault = MTRX.MtiShader.Load<Material>("MaterialForSpine");
			MTRX.MtrMeshNormal = MTRX.newMtr(MTRX.ShaderMesh);
			MTRX.MtrMeshZW = MTRX.newMtr(MTRX.ShaderMeshZW);
			MTRX.MtrMeshAdd = MTRX.newMtr(MTRX.ShaderMeshAdd);
			MTRX.MtrMeshSub = MTRX.newMtr(MTRX.ShaderMeshSub);
			MTRX.MtrMeshAddZT = MTRX.newMtr(MTRX.ShaderMeshAddZT);
			MTRX.MtrMeshMul = MTRX.newMtr(MTRX.ShaderMeshMul);
			MTRX.MtrMeshMask = MTRX.newMtr(MTRX.ShaderMeshMask);
			MTRX.MtrMeshStriped = MTRX.newMtr(MTRX.MtiShader.LoadShader("Hachan/ShaderMeshStriped"));
			MTRX.MtrMeshStripedSub = MTRX.newMtr(MTRX.MtiShader.LoadShader("Hachan/ShaderMeshSubStriped"));
			MTRX.MtrMeshDashLine = MTRX.newMtr(MTRX.MtiShader.LoadShader("Hachan/ShaderMeshDashLine"));
			PxlsLoader.ShaderForCreateRenderImage = MTRX.MtiShader.LoadShader("PixelLiner/Copying");
			BLIT.init1();
			Font font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
			MTRX.StorageLogoTypeGothic = new FontStorageLogoTypeGothic(new MFont("logo_type_gothic"), "letterspace_logotypegothic");
			MTRX.StorageUtsukusi = new FontStorageUtsukusi(new MFont("UtsukushiFONT"), "letterspace_utsukusi");
			MTRX.StorageUnityArial = new FontStorageArial(new MFont(null, "Arial.ttf", font), "letterspace_arial");
			MTRX.StorageCabin = new FontStorageCabin(new MFont("CabinCondensed-Bold").Load(), "letterspace_cabin");
			MTRX.remakeFontStorageDictionary();
			TX.reloadFontLetterSpace();
			PxlCharacter pxlIcon = MTRX.PxlIcon;
			if (pxlIcon != null)
			{
				MTRX.ChrL.add(pxlIcon.getPoseByName("charsprite_L").getSequence(0), "0123456789/+%-:ms.xceipfNEXTGO!", 0);
				MTRX.ChrLb.add(pxlIcon.getPoseByName("charsprite_lnumb").getSequence(0), "0123456789/+%-:", 0);
				MTRX.ChrM.add(pxlIcon.getPoseByName("charsprite_m").getSequence(0), "ABCDEFGHIJKLMNOPQRSTUVWXYZ*/1234567890.:+-abcdefghijklmnopqrstuvwxyz()!?_", 0);
				MTRX.assignPxlImages(pxlIcon.getPoseByName("main"), false);
				PxlPose poseByName = pxlIcon.getPoseByName("effect");
				MTRX.AEff = MTRX.getPFArray(poseByName, 0f, 0f);
				MTRX.assignPxlImages(poseByName, false);
				MTRX.SqPattern = pxlIcon.getPoseByName("Pattern").getSequence(0);
				MTRX.IconWhite = MTRX.SqPattern.getImage(2, 0);
				MTRX.PatSelection = MTRX.SqPattern.getImage(1, 0);
				MTRX.EffCircle128 = MTRX.SqPattern.getImage(3, 0);
				MTRX.EffBlurCircle245 = MTRX.SqPattern.getImage(4, 0);
				MTRX.EffRippleCircle245 = MTRX.SqPattern.getImage(5, 0);
				MTRX.SqDotKira = pxlIcon.getPoseByName("dotkira").getSequence(0);
				MTRX.SqDotKiraAnim = pxlIcon.getPoseByName("kira_anim").getSequence(0);
				MTRX.SqImgSliderArrow = pxlIcon.getPoseByName("slider_arrow").getSequence(0);
				MTRX.SqRankStarChar = pxlIcon.getPoseByName("rankstar_char").getSequence(0);
				MTRX.SqLoadingS = pxlIcon.getPoseByName("loading_s").getSequence(0);
				MTRX.ImgMadeByUnity = pxlIcon.getPoseByName("unity_logo").getSequence(0).getImage(0, 0);
				MTRX.assignPxlImages(pxlIcon.getPoseByName("keyconfig_top"), false);
				PxlSequence sequence = pxlIcon.getPoseByName("keyconfig").getSequence(0);
				MTRX.SqFImgKCIcon = sequence.getFrame(0);
				MTRX.SqFImgKCIconS = sequence.getFrame(1);
				MTRX.SqM2dIcon = pxlIcon.getPoseByName("m2d").getSequence(0);
				MTRX.AUiSerIcon = pxlIcon.getPoseByName("ser").getSequence(0);
				MTRX.assignPxlImages(MTRX.AUiSerIcon, "AUiSerIcon");
				MTRX.MIicon = (MTRX.OMI[pxlIcon] = new MImage(MTRX.IconWhite.get_I()));
				if (MTRX.Aadditinal_pmesh_pose_title != null)
				{
					int num = MTRX.Aadditinal_pmesh_pose_title.Length;
					for (int i = 0; i < num; i++)
					{
						string text = MTRX.Aadditinal_pmesh_pose_title[i];
						if (!TX.noe(text))
						{
							MTRX.assignPxlImages(pxlIcon.getPoseByName(text), true);
						}
					}
				}
				PxlLayer[] layerArray = MTRX.getLayerArray(pxlIcon.getPoseByName("frame"), 0);
				MTRX.MsgFrame3 = new FrameDrawer();
				MTRX.MsgFrame3.BL(layerArray[0].Img).V(layerArray[1].Img, 0f, 0f).H(layerArray[2].Img, 0f, 0f);
				MTRX.MsgFrame2 = new FrameDrawer();
				MTRX.MsgFrame2.BL(layerArray[3].Img).V(layerArray[4].Img, 0f, 0f).H(layerArray[5].Img, 0f, 0f);
				MTRX.TalkerFrame3 = new FrameDrawer();
				MTRX.TalkerFrame3.BL(layerArray[6].Img).V(layerArray[7].Img, 0f, 0f).H(layerArray[8].Img, 0f, 0f);
				MTRX.ItemGetDescFrame = layerArray[9].Img;
				MTRX.SmallDescFrame = new FrameDrawer();
				MTRX.SmallDescFrame.LT(MTRX.ItemGetDescFrame);
				MTRX.ChrAnn = new BMListChars(0, 0, 0f);
				PxlLayer[] layerArray2 = MTRX.getLayerArray(pxlIcon.getPoseByName("congratulations"), 0);
				MTRX.ChrAnn.addResourceA(layerArray2, "!SILUTARGNOC", 2);
				MTRX.AImgAnnounce = layerArray2;
				MTRX.SqEfSunabokori = pxlIcon.getPoseByName("effect_sunabokori").getSequence(0);
				MTRX.releasePxlMesh(pxlIcon, new string[] { "congratulations", "charsprite_L", "charsprite_lnumb", "charsprite_m" });
			}
			EffectItem.initEffectItem();
			MTRX.PFCheckBoxChecked = MTRX.getPF("checked");
			MTRX.PFCheckBoxSemi = MTRX.getPF("semi_checked");
			CURS.readCursCsv(Resources.Load<TextAsset>("Basic/Data/_curs"));
			SND.sndInitPost();
			IN.Loa1dFinished();
		}

		public static void checkStorage()
		{
			if (MTRX.StorageLogoTypeGothic.fine_text > 0)
			{
				bool flag;
				MTRX.StorageLogoTypeGothic.FineExecute(out flag, false);
			}
		}

		public static bool prepared
		{
			get
			{
				if (MTRX.loaded == 7)
				{
					return true;
				}
				if (MTRX.loaded == 1 && MTRX.PxlIcon.isLoadCompleted() && MTRX.MtiShader.isAsyncLoadFinished())
				{
					Bench.mark(null, false, false);
					MTRX.loaded = 3;
					MTRX.init2();
					return false;
				}
				if (MTRX.loaded == 3 && SND.loaded)
				{
					MTRX.loaded = 7;
					Bench.mark(null, false, false);
					return true;
				}
				return false;
			}
		}

		public static void remakeFontStorageDictionary()
		{
			MTRX.OFontStorage = new BDic<MFont, FontStorage>();
			MTRX.OFontStorage[MTRX.StorageLogoTypeGothic.TargetFont] = MTRX.StorageLogoTypeGothic;
			MTRX.OFontStorage[MTRX.StorageUtsukusi.TargetFont] = MTRX.StorageUtsukusi;
			MTRX.OFontStorage[MTRX.StorageUnityArial.TargetFont] = MTRX.StorageUnityArial;
			MTRX.OFontStorage[MTRX.StorageCabin.TargetFont] = MTRX.StorageCabin;
		}

		public static void clearStorageListener()
		{
			if (MTRX.OFontStorage == null)
			{
				return;
			}
			foreach (KeyValuePair<MFont, FontStorage> keyValuePair in MTRX.OFontStorage)
			{
				keyValuePair.Value.clearListener();
			}
		}

		public static PxlFrame getPF(string name)
		{
			return X.Get<string, PxlFrame>(MTRX.OMeshImages, name);
		}

		public static PxlFrame getPF(string name, out MImage MI)
		{
			PxlFrame pf = MTRX.getPF(name);
			MI = MTRX.getMI(pf);
			return pf;
		}

		public static MImage getMI(PxlFrame PF)
		{
			if (PF == null)
			{
				return null;
			}
			return MTRX.getMI(PF.pChar);
		}

		public static MImage getMI(PxlImage PI)
		{
			if (PI == null)
			{
				return null;
			}
			return MTRX.getMI(PI.pChar);
		}

		public static MImage getMI(PxlCharacter Pcr)
		{
			if (Pcr == null)
			{
				return null;
			}
			MImage mimage;
			if (!MTRX.OMI.TryGetValue(Pcr, out mimage))
			{
				using (Dictionary<string, PxlImage>.Enumerator enumerator = Pcr.getImageObject().GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						KeyValuePair<string, PxlImage> keyValuePair = enumerator.Current;
						mimage = (MTRX.OMI[Pcr] = new MImage(keyValuePair.Value.get_I()));
					}
				}
			}
			return mimage;
		}

		public static MImage assignMI(PxlCharacter Pcr, MImage MI)
		{
			MTRX.OMI[Pcr] = MI;
			return MI;
		}

		public static MImage releaseMI(PxlCharacter Pcr, bool disposing = true)
		{
			MImage mimage;
			if (MTRX.OMI.TryGetValue(Pcr, out mimage))
			{
				if (disposing)
				{
					mimage.Dispose();
				}
				MTRX.OMI.Remove(Pcr);
			}
			return null;
		}

		public static Sprite getSprite(string name)
		{
			if (TX.noe(name))
			{
				return null;
			}
			Match match = REG.RegSuffixPeriodInt.Match(name);
			int num = 0;
			if (match.Success)
			{
				num = X.NmI(match.Groups[1].Value, 0, false, false);
				name = name.Substring(0, match.Index);
			}
			try
			{
				FieldInfo field = typeof(MTRX).GetField(name);
				if (field == null)
				{
					throw new Exception("no img");
				}
				object value = field.GetValue(null);
				if (value is Sprite[])
				{
					return (value as Sprite[])[num];
				}
				return value as Sprite;
			}
			catch
			{
			}
			return null;
		}

		public static Material getStencilFontMaterial(MFont F)
		{
			Material material = MTRX.newMtr(MTRX.ShaderGDTSTFont);
			material.SetTexture("_MainTex", F.material.GetTexture("_MainTex"));
			return material;
		}

		public static MFont getDefaultFont()
		{
			return MTRX.StorageLogoTypeGothic.LoadMFont();
		}

		public static MFont getCabinFont()
		{
			return MTRX.StorageCabin.LoadMFont();
		}

		public static MFont getTitleFontDefault()
		{
			return MTRX.StorageUtsukusi.LoadMFont();
		}

		public static MFont getArialFont()
		{
			return MTRX.StorageUnityArial.LoadMFont();
		}

		public static FontStorage addFontStorageBundled(Font F, MTI Mti, string font_path, float _base_height, float _yshift, float _xratio, float _xratio_1byte, float _default_renderer_size, string letterspace_script)
		{
			foreach (KeyValuePair<MFont, FontStorage> keyValuePair in MTRX.OFontStorage)
			{
				if (keyValuePair.Value.TargetFont.Target == F)
				{
					return keyValuePair.Value;
				}
			}
			MFont mfont = new MFont(Mti, font_path, F);
			return MTRX.OFontStorage[mfont] = new FontStorageBundled(mfont, _base_height, _yshift, _xratio, _xratio_1byte, _default_renderer_size, letterspace_script);
		}

		public static void addFontStorageBundled(MFont MF, FontStorage _Str)
		{
			MTRX.OFontStorage[MF] = _Str;
		}

		public static void remFontStorageBundled(MFont MF)
		{
			MTRX.OFontStorage.Remove(MF);
		}

		public static void assignPxlImages(PxlCharacter Pc)
		{
			int num = Pc.countPoses();
			for (int i = 0; i < num; i++)
			{
				MTRX.assignPxlImages(Pc.getPose(i), false);
			}
		}

		public static void assignPxlImages(PxlPose P, bool assign_frm_name = false)
		{
			uint num = 8U;
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				if (P.isValidAim(num2) && !P.isFlipped(num2))
				{
					MTRX.assignPxlImages(P.getSequence(num2), assign_frm_name);
				}
				num2++;
			}
		}

		public static void assignPxlImages(PxlSequence P, bool assign_frm_name = false)
		{
			int num = P.countFrames();
			for (int i = 0; i < num; i++)
			{
				MTRX.assignPxlImages(P.getFrame(i), i, assign_frm_name);
			}
		}

		private static void assignPxlImages(PxlFrame F, int f, bool assign_frm_name = false)
		{
			string text = F.name;
			if (text == "")
			{
				if (!assign_frm_name)
				{
					text = F.pPose.title + "." + f.ToString();
				}
				else
				{
					text = F.getLayer(0).name;
				}
			}
			MTRX.OMeshImages[text] = F;
		}

		public static void assignPxlImages(PxlSequence AMesh, string title)
		{
			int length = AMesh.Length;
			for (int i = 0; i < length; i++)
			{
				MTRX.OMeshImages[title + "." + i.ToString()] = AMesh[i];
			}
		}

		public static void assignPxlImages(string name, PxlFrame F)
		{
			MTRX.OMeshImages[name] = F;
		}

		public static PxlImage[] getPImgArray(PxlPose P)
		{
			uint num = 8U;
			PxlImage[] array = null;
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				if (P.isValidAim(num2) && !P.isFlipped(num2))
				{
					MTRX.getPImgArray(ref array, P.getSequence(num2));
				}
				num2++;
			}
			return array;
		}

		private static PxlImage[] getPImgArray(ref PxlImage[] A, PxlSequence P)
		{
			int num = P.countFrames();
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				num2 += P.getFrame(i).countLayers();
			}
			int num3 = 0;
			if (A == null)
			{
				A = new PxlImage[num2];
			}
			else
			{
				num3 = A.Length;
				Array.Resize<PxlImage>(ref A, num3 + num2);
			}
			for (int j = 0; j < num; j++)
			{
				PxlFrame frame = P.getFrame(j);
				int num4 = frame.countLayers();
				for (int k = 0; k < num4; k++)
				{
					PxlLayer layer = frame.getLayer(k);
					A[num3++] = layer.Img;
				}
			}
			return A;
		}

		public static PxlLayer[] getLayerArray(PxlPose P, int margin = 0)
		{
			uint num = 8U;
			PxlLayer[] array = null;
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				if (P.isValidAim(num2) && !P.isFlipped(num2))
				{
					MTRX.getLayerArray(ref array, P.getSequence(num2), margin);
				}
				num2++;
			}
			return array;
		}

		private static PxlLayer[] getLayerArray(ref PxlLayer[] A, PxlSequence P, int margin = 0)
		{
			int num = P.countFrames();
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				PxlFrame frame = P.getFrame(i);
				int num3 = frame.countLayers();
				for (int j = 0; j < num3; j++)
				{
					if (!frame.getLayer(j).isImport())
					{
						num2++;
					}
				}
			}
			int num4 = 0;
			if (A == null)
			{
				A = new PxlLayer[num2];
			}
			else
			{
				num4 = X.countNotEmpty<PxlLayer>(A);
				Array.Resize<PxlLayer>(ref A, num4 + num2);
			}
			for (int k = 0; k < num; k++)
			{
				PxlFrame frame2 = P.getFrame(k);
				int num5 = frame2.countLayers();
				for (int l = 0; l < num5; l++)
				{
					PxlLayer layer = frame2.getLayer(l);
					if (!layer.isImport())
					{
						A[num4++] = layer;
					}
				}
			}
			return A;
		}

		public static PxlFrame[] getPFArray(PxlPose P, float offset_x = 0f, float offset_y = 0f)
		{
			uint num = 8U;
			PxlFrame[] array = null;
			int num2 = 0;
			while ((long)num2 < (long)((ulong)num))
			{
				if (P.isValidAim(num2) && !P.isFlipped(num2))
				{
					MTRX.getPFArray(ref array, P.getSequence(num2), offset_x, offset_y);
				}
				num2++;
			}
			return array;
		}

		private static PxlFrame[] getPFArray(ref PxlFrame[] A, PxlSequence P, float offset_x = 0f, float offset_y = 0f)
		{
			int num = P.countFrames();
			int num2 = 0;
			if (A == null)
			{
				A = new PxlFrame[num];
			}
			else
			{
				num2 = A.Length;
				Array.Resize<PxlFrame>(ref A, num2 + num);
			}
			for (int i = 0; i < num; i++)
			{
				PxlFrame frame = P.getFrame(i);
				A[num2++] = frame;
			}
			return A;
		}

		public static void releasePxlMesh(PxlCharacter C, params string[] Apose)
		{
			uint num = 8U;
			int num2 = Apose.Length;
			for (int i = 0; i < num2; i++)
			{
				PxlPose poseByName = C.getPoseByName(Apose[i]);
				if (poseByName != null)
				{
					int num3 = 0;
					while ((long)num3 < (long)((ulong)num))
					{
						if (poseByName.isValidAim(num3) && !poseByName.isFlipped(num3))
						{
							MTRX.releasePxlMesh(poseByName.getSequence(num3));
						}
						num3++;
					}
				}
			}
		}

		public static void releasePxlMesh(PxlSequence Sq)
		{
			int num = Sq.countFrames();
			for (int i = 0; i < num; i++)
			{
				Sq.getFrame(i).releaseDrawnMesh();
			}
		}

		public static Material setMaterialST(Material M, string key, Sprite S)
		{
			M.SetTexture(key, S.texture);
			M.SetTextureOffset(key, new Vector2(S.rect.x / (float)S.texture.width, S.rect.y / (float)S.texture.height));
			M.SetTextureScale(key, new Vector2(S.rect.width / (float)S.texture.width, S.rect.height / (float)S.texture.height));
			return M;
		}

		public static Material setMaterialST(Material M, string key, PxlImage S, float margin_pixel = 0f)
		{
			M.SetTexture(key, S.get_I());
			M.SetTextureOffset(key, new Vector2(S.RectIUv.x + margin_pixel / (float)S.get_I().width, S.RectIUv.y + margin_pixel / (float)S.get_I().height));
			M.SetTextureScale(key, new Vector2(S.RectIUv.width - 2f * margin_pixel / (float)S.get_I().width, S.RectIUv.height - 2f * margin_pixel / (float)S.get_I().height));
			return M;
		}

		public static void DrawMeshIcon(MeshDrawer Md, float cx, float cy, float wd, string name, float val1 = 0f)
		{
			if (name != null && name == "lock")
			{
				float num = 0.55f;
				Md.Rect(cx, cy + wd * (-0.5f + num / 2f), wd, wd * num, false);
				Md.Arc(cx, cy + (-0.5f + num) * wd, (1f - num) * 0.78f * wd, 3.1415927f * (0.95f * X.ZPOW(val1, 0.4f) - 0.12f * X.ZCOS(val1 - 0.4f, 0.6f)), 3.1415927f, X.Mx(2f, wd * 0.1f));
			}
		}

		public static void kadomaruRectExtImg(MeshDrawer Md, float x, float y, float w, float h, float radius, PxlImage Img = null, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu)
			{
				x *= 0.015625f;
				y *= 0.015625f;
				w *= 0.015625f;
				h *= 0.015625f;
				radius *= 0.015625f;
			}
			Md.initForImg(Img ?? MTRX.EffCircle128, 0);
			radius = X.Mn(X.Mn(w, h) * 0.5f, radius);
			if (radius * 2.00001f >= w && radius * 2.00001f >= h)
			{
				Md.Rect(x, y, w, h, true);
				return;
			}
			float uv_left = Md.uv_left;
			float uv_top = Md.uv_top;
			float uv_width = Md.uv_width;
			float uv_height = Md.uv_height;
			float num = w * 0.5f;
			float num2 = h * 0.5f;
			float num3 = x - num;
			float num4 = y - num2;
			float num5 = x + num;
			float num6 = y + num2;
			float num7 = uv_left + uv_width;
			float num8 = uv_top + uv_height;
			if (radius * 2.00001f >= w)
			{
				float num9 = uv_top + uv_height * 0.5f;
				Md.TriRectBL(0).TriRectBL(4).Tri(5, 0, 3, false)
					.Tri(5, 3, 7, false);
				Md.PosUv(num3, num6 - radius, uv_left, num9, null).PosUv(num3, num6, uv_left, num8, null).PosUv(num5, num6, num7, num8, null)
					.PosUv(num5, num6 - radius, num7, num9, null);
				Md.PosUv(num3, num4, uv_left, uv_top, null).PosUv(num3, num4 + radius, uv_left, num9, null).PosUv(num5, num4 + radius, num7, num9, null)
					.PosUv(num5, num4, num7, uv_top, null);
				return;
			}
			if (radius * 2.00001f >= h)
			{
				float num10 = uv_left + uv_width * 0.5f;
				Md.TriRectBL(0).TriRectBL(4).Tri(3, 2, 5, false)
					.Tri(3, 5, 4, false);
				Md.PosUv(num3, num4, uv_left, uv_top, null).PosUv(num3, num6, uv_left, num8, null).PosUv(num3 + radius, num6, num10, num8, null)
					.PosUv(num3 + radius, num4, num10, uv_top, null);
				Md.PosUv(num5 - radius, num4, num10, uv_top, null).PosUv(num5 - radius, num6, num10, num8, null).PosUv(num5, num6, num7, num8, null)
					.PosUv(num5, num4, num7, uv_top, null);
				return;
			}
			float num11 = uv_left + uv_width * 0.5f;
			float num12 = uv_top + uv_height * 0.5f;
			Md.TriRectBL(0).TriRectBL(4).TriRectBL(8)
				.TriRectBL(12)
				.TriRectBL(1, 4, 7, 2)
				.TriRectBL(7, 6, 9, 8)
				.TriRectBL(13, 8, 11, 14)
				.TriRectBL(3, 2, 13, 12)
				.TriRectBL(2, 7, 8, 13);
			Md.PosUv(num3, num4, uv_left, uv_top, null).PosUv(num3, num4 + radius, uv_left, num12, null).PosUv(num3 + radius, num4 + radius, num11, num12, null)
				.PosUv(num3 + radius, num4, num11, uv_top, null);
			Md.PosUv(num3, num6 - radius, uv_left, num12, null).PosUv(num3, num6, uv_left, num8, null).PosUv(num3 + radius, num6, num11, num8, null)
				.PosUv(num3 + radius, num6 - radius, num11, num12, null);
			Md.PosUv(num5 - radius, num6 - radius, num11, num12, null).PosUv(num5 - radius, num6, num11, num8, null).PosUv(num5, num6, num7, num8, null)
				.PosUv(num5, num6 - radius, num7, num12, null);
			Md.PosUv(num5 - radius, num4, num11, uv_top, null).PosUv(num5 - radius, num4 + radius, num11, num12, null).PosUv(num5, num4 + radius, num7, num12, null)
				.PosUv(num5, num4, num7, uv_top, null);
		}

		public static void TriangleImg(MeshDrawer Md, float x0, float y0, float x1, float y1, float x2, float y2, PxlImage Img = null, AIM posa = AIM.L, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu)
			{
				x0 *= 0.015625f;
				y0 *= 0.015625f;
				x1 *= 0.015625f;
				y1 *= 0.015625f;
				x2 *= 0.015625f;
				y2 *= 0.015625f;
			}
			float uv_left = Md.uv_left;
			float uv_top = Md.uv_top;
			float uv_width = Md.uv_width;
			float uv_height = Md.uv_height;
			Md.initForImg(Img ?? MTRX.IconWhite, 0);
			Md.Tri(0, 1, 2, false).Pos(x0, y0, null).Pos(x1, y1, null)
				.Pos(x2, y2, null);
			Vector2[] uvArray = Md.getUvArray();
			int num = Md.getVertexMax() - 3;
			switch (posa)
			{
			case AIM.L:
				uvArray[num] = new Vector2(uv_left, uv_top + uv_height * 0.5f);
				uvArray[num + 1] = new Vector2(uv_left + uv_width, uv_top + uv_height);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top);
				return;
			case AIM.T:
				uvArray[num] = new Vector2(uv_left, uv_top);
				uvArray[num + 1] = new Vector2(uv_left + uv_width * 0.5f, uv_top + uv_height);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top);
				return;
			case AIM.R:
				uvArray[num] = new Vector2(uv_left, uv_top);
				uvArray[num + 1] = new Vector2(uv_left, uv_top + uv_height);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top + uv_height * 0.5f);
				return;
			case AIM.B:
				uvArray[num] = new Vector2(uv_left + uv_width * 0.5f, uv_top);
				uvArray[num + 1] = new Vector2(uv_left, uv_top + uv_height);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top + uv_height);
				return;
			case AIM.LT:
				uvArray[num] = new Vector2(uv_left, uv_top);
				uvArray[num + 1] = new Vector2(uv_left, uv_top + uv_height);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top + uv_height);
				return;
			case AIM.TR:
				uvArray[num] = new Vector2(uv_left + uv_width, uv_top);
				uvArray[num + 1] = new Vector2(uv_left, uv_top + uv_height);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top + uv_height);
				return;
			case AIM.BL:
				uvArray[num] = new Vector2(uv_left, uv_top);
				uvArray[num + 1] = new Vector2(uv_left, uv_top + uv_height);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top);
				return;
			case AIM.RB:
				uvArray[num] = new Vector2(uv_left, uv_top);
				uvArray[num + 1] = new Vector2(uv_left + uv_width, uv_top);
				uvArray[num + 2] = new Vector2(uv_left + uv_width, uv_top + uv_height);
				return;
			default:
				return;
			}
		}

		public static Shader getShd(string shader_name)
		{
			return MTRX.MtiShader.LoadShader(shader_name);
		}

		public static Material getMtr(int stencil_ref)
		{
			return MTRX.getMtr(BLEND.NORMAL, stencil_ref);
		}

		public static Material getMtr(BLEND blnd, PxlFrame PF, int stencil_ref = -1)
		{
			return MTRX.getMI(PF).getMtr(blnd, stencil_ref);
		}

		public static Material getMtr(BLEND blnd, PxlMeshDrawer PMesh, int stencil_ref = -1)
		{
			return MTRX.getMI(PMesh.SourceFrame.pChar).getMtr(blnd, stencil_ref);
		}

		public static Material getMtr(BLEND blnd = BLEND.NORMAL, int stencil_ref = -1)
		{
			if (stencil_ref >= 0)
			{
				BLEND blend;
				if (blnd != BLEND.NORMAL)
				{
					if (blnd != BLEND.NORMALBORDER8)
					{
						blend = blnd;
					}
					else
					{
						blend = BLEND.NORMALBORDER8ST;
					}
				}
				else
				{
					blend = BLEND.NORMALST;
				}
				blnd = blend;
			}
			else
			{
				BLEND blend;
				if (blnd != BLEND.NORMALST)
				{
					if (blnd != BLEND.NORMALBORDER8ST)
					{
						blend = blnd;
					}
					else
					{
						blend = BLEND.NORMALBORDER8;
					}
				}
				else
				{
					blend = BLEND.NORMAL;
				}
				blnd = blend;
			}
			switch (blnd)
			{
			case BLEND.ADD:
				return MTRX.MtrMeshAdd;
			case BLEND.MUL:
				return MTRX.MtrMeshMul;
			case BLEND.SUB:
				return MTRX.MtrMeshSub;
			case BLEND.ADDP:
			case BLEND.MULP:
			case BLEND.NORMALZT:
			case BLEND.NORMALBORDER8:
				break;
			case BLEND.NORMALZW:
				return MTRX.MtrMeshZW;
			case BLEND.ADDZT:
				return MTRX.MtrMeshAddZT;
			case BLEND.NORMALST:
				return MTRX.getMtr(MTRX.ShaderMeshST, stencil_ref);
			default:
				if (blnd == BLEND.MASK)
				{
					return MTRX.getMtr(MTRX.ShaderMeshMask, stencil_ref);
				}
				if (blnd == BLEND.MASK_TRANSPARENT)
				{
					return MTRX.getMtr(MTRX.ShaderMeshMaskTransparent, stencil_ref);
				}
				break;
			}
			return MTRX.MtrMeshNormal;
		}

		public static Material getMtr(Shader Shd, int stencil_ref = -1)
		{
			BDic<Shader, Material> bdic;
			if (!MTRX.OOMtr.TryGetValue(stencil_ref, out bdic))
			{
				bdic = (MTRX.OOMtr[stencil_ref] = new BDic<Shader, Material>(1));
			}
			Material material = X.Get<Shader, Material>(bdic, Shd);
			if (material == null)
			{
				material = (bdic[Shd] = MTRX.newMtr(Shd));
			}
			MTRX.fixMaterialStencilRef(material, Shd, stencil_ref);
			return material;
		}

		public static void fixMaterialStencilRef(Material Mtr, Shader Shd, int stencil_ref = -1)
		{
			if (stencil_ref >= 256 && stencil_ref < 511)
			{
				Mtr.SetFloat("_StencilRef", (float)(stencil_ref - 256));
				Mtr.SetFloat("_StencilComp", 4f);
				return;
			}
			if (stencil_ref >= 0)
			{
				Mtr.SetFloat("_StencilRef", (float)(stencil_ref & 255));
				Mtr.SetFloat("_StencilComp", (stencil_ref < 0 || Shd == MTRX.ShaderMeshMask || Shd == MTRX.ShaderGDTMask || Shd == MTRX.ShaderMeshMaskTransparent) ? 8f : 3f);
				if (512 <= stencil_ref && stencil_ref < 767)
				{
					Mtr.SetFloat("_StencilOp", 3f);
				}
			}
		}

		public static Shader blend2ShaderImg(BLEND blend)
		{
			switch (blend)
			{
			case BLEND.ADD:
				return MTRX.ShaderGDTAdd;
			case BLEND.MUL:
				return MTRX.ShaderGDTMul;
			case BLEND.SUB:
				return MTRX.ShaderGDTSub;
			case BLEND.ADDP:
				return MTRX.ShaderGDTPAdd;
			case BLEND.NORMALZT:
				return MTRX.ShaderGDTZT;
			case BLEND.NORMALZW:
				return MTRX.ShaderGDTZW;
			case BLEND.ADDZT:
				return MTRX.ShaderGDTAddZT;
			case BLEND.NORMALBORDER8:
				return MTRX.ShaderGDTBorder8;
			case BLEND.NORMALST:
				return MTRX.ShaderGDTST;
			case BLEND.NORMALBORDER8ST:
				return MTRX.ShaderGDTSTBorder8;
			case BLEND.GDT_NORMALGLOW:
				return MTRX.ShaderGDTNormalGlow;
			case BLEND.GDT_NORMALBLUR:
				return MTRX.ShaderGDTNormalBlur;
			case BLEND.GDT_ADDGLOW:
				return MTRX.ShaderGDTAddGlow;
			case BLEND.MASK:
				return MTRX.ShaderGDTMask;
			case BLEND.NORMALP2:
				return MTRX.ShaderGDTP2;
			case BLEND.NORMALP3:
				return MTRX.ShaderGDTP3;
			}
			return MTRX.ShaderGDT;
		}

		public static Material replaceST(Material _Mtr, int stencil = -1)
		{
			if (stencil == -1)
			{
				if (_Mtr.shader == MTRX.ShaderGDTST)
				{
					_Mtr = MTRX.newMtr(_Mtr);
					_Mtr.shader = MTRX.ShaderGDT;
				}
				if (_Mtr.shader == MTRX.ShaderMesh || _Mtr.shader == MTRX.ShaderMeshST)
				{
					_Mtr = MTRX.newMtr(_Mtr);
					_Mtr.shader = MTRX.ShaderMesh;
					_Mtr.SetFloat("_StencilComp", 8f);
					_Mtr.SetFloat("_StencilOp", 0f);
				}
			}
			else
			{
				if (_Mtr.shader == MTRX.ShaderGDT)
				{
					_Mtr = MTRX.newMtr(_Mtr);
					_Mtr.shader = MTRX.ShaderGDTST;
				}
				if (_Mtr.shader == MTRX.ShaderMesh || _Mtr.shader == MTRX.ShaderMeshST)
				{
					_Mtr = MTRX.newMtr(_Mtr);
					_Mtr.shader = MTRX.ShaderMesh;
					_Mtr.SetFloat("_StencilOp", 0f);
				}
				_Mtr.SetFloat("_StencilComp", 3f);
				_Mtr.SetFloat("_StencilRef", (float)stencil);
			}
			return _Mtr;
		}

		public static float zshift_blend(Material Mtr, string shader_name = null)
		{
			if (shader_name == null)
			{
				shader_name = Mtr.shader.name;
			}
			if (shader_name != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(shader_name);
				if (num <= 2206940855U)
				{
					if (num > 1191756596U)
					{
						if (num != 1779736195U)
						{
							if (num != 2189127554U)
							{
								if (num != 2206940855U)
								{
									goto IL_0165;
								}
								if (!(shader_name == "Hachan/ShaderMeshMask"))
								{
									goto IL_0165;
								}
							}
							else
							{
								if (!(shader_name == "Hachan/ShaderGDTAdd"))
								{
									goto IL_0165;
								}
								goto IL_015F;
							}
						}
						else if (!(shader_name == "Hachan/ShaderMeshMaskTransparent"))
						{
							goto IL_0165;
						}
						return 10f;
					}
					if (num != 493498423U)
					{
						if (num != 1191756596U)
						{
							goto IL_0165;
						}
						if (!(shader_name == "Hachan/ShaderGDTPAdd"))
						{
							goto IL_0165;
						}
						goto IL_015F;
					}
					else if (!(shader_name == "Hachan/ShaderGDTMul"))
					{
						goto IL_0165;
					}
				}
				else if (num <= 3081578899U)
				{
					if (num != 2516598822U)
					{
						if (num != 3081578899U)
						{
							goto IL_0165;
						}
						if (!(shader_name == "Hachan/ShaderMeshMul"))
						{
							goto IL_0165;
						}
					}
					else
					{
						if (!(shader_name == "Hachan/ShaderMeshAdd"))
						{
							goto IL_0165;
						}
						goto IL_015F;
					}
				}
				else
				{
					if (num != 3117273120U)
					{
						if (num != 3196970695U)
						{
							if (num != 3854204475U)
							{
								goto IL_0165;
							}
							if (!(shader_name == "Hachan/ShaderGDTSub"))
							{
								goto IL_0165;
							}
						}
						else if (!(shader_name == "Hachan/ShaderMeshSub"))
						{
							goto IL_0165;
						}
						return 2f;
					}
					if (!(shader_name == "Hachan/ShaderMeshAddZT"))
					{
						goto IL_0165;
					}
					goto IL_015F;
				}
				return 1f;
				IL_015F:
				return -1f;
			}
			IL_0165:
			return 0f;
		}

		public static PxlCharacter PxlIcon;

		private static BDic<string, PxlFrame> OMeshImages;

		private static BDic<PxlCharacter, MImage> OMI;

		public static Material MtrMeshNormal;

		public static Material MtrMeshZW;

		public static Material MtrMeshMul;

		public static Material MtrMeshAdd;

		public static Material MtrMeshSub;

		public static Material MtrMeshAddZT;

		public static Material MtrMeshMask;

		public static Material MtrMeshStriped;

		public static Material MtrMeshStripedSub;

		public static Material MtrMeshDashLine;

		public static PxlFrame PFCheckBoxChecked;

		public static PxlFrame PFCheckBoxSemi;

		public static PxlSequence SqImgSliderArrow;

		public static PxlSequence SqPattern;

		public static PxlImage IconWhite;

		public static PxlSequence SqM2dIcon;

		public static PxlFrame SqFImgKCIcon;

		public static PxlFrame SqFImgKCIconS;

		public static Color32 ColWhite = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public static Color32 ColBlack = new Color32(0, 0, 0, byte.MaxValue);

		public static Color32 ColGray = new Color32(127, 127, 127, byte.MaxValue);

		public static Color32 ColMenu = new Color32(0, 0, 0, 178);

		public static Color32 ColMenuError = new Color32(167, 64, 64, 178);

		public static Color32 ColBlackBorder = new Color32(0, 0, 0, 178);

		public static Color32 ColTrnsp = new Color32(0, 0, 0, 0);

		public static Color32 ColTrnspGray = new Color32(127, 127, 127, 0);

		public static uint text_color = 4278190080U;

		public static C32 cola;

		public static C32 colb;

		public static PxlSequence SqRankStarChar;

		public static PxlLayer[] AImgAnnounce;

		public static BDic<int, BDic<Shader, Material>> OOMtr;

		public static PxlImage ImgMadeByUnity;

		private static MTI MtiShader;

		public static Shader ShaderGDTZW;

		public static Shader ShaderGDTPAdd;

		public static Shader ShaderGDTAdd;

		public static Shader ShaderGDTAddZT;

		public static Shader ShaderGDTSub;

		public static Shader ShaderGDTP2;

		public static Shader ShaderGDTP3;

		public static Shader ShaderGDT;

		public static Shader ShaderGDTZT;

		public static Shader ShaderGDTST;

		public static Shader ShaderGDTSTBorder8;

		public static Shader ShaderGDTFont;

		public static Shader ShaderGDTFontBorder8;

		public static Shader ShaderGDTSTFont;

		public static Shader ShaderGDTSTFontBorder8;

		public static Shader ShaderGDTMask;

		public static Shader ShaderGDTMul;

		public static Shader ShaderGDTErase;

		public static Shader ShaderGDTBorder8;

		public static Shader ShaderGDTNormalGlow;

		public static Shader ShaderGDTNormalBlur;

		public static Shader ShaderGDTAddGlow;

		public static Shader ShaderMesh;

		public static Shader ShaderMeshST;

		public static Shader ShaderMeshZW;

		public static Shader ShaderMeshMask;

		public static Shader ShaderMeshMaskTransparent;

		public static Shader ShaderMeshAdd;

		public static Shader ShaderMeshAddZT;

		public static Shader ShaderMeshSub;

		public static Shader ShaderMeshMul;

		public static Shader ShaderGDTClear;

		public static Shader ShaderAlphaSplice;

		public static Material MtrSpineDefault;

		public static PxlFrame MeshArrowR;

		public static PxlFrame MeshArrowRShifted;

		public static MImage MIicon;

		public static Material MtrTxAnother;

		public static BMListChars ChrL;

		public static BMListChars ChrLb;

		public static BMListChars ChrM;

		public static BMListChars ChrAnn;

		public static PxlSequence AUiSerIcon;

		private static int loaded = 0;

		public static PxlSequence SqDotKira;

		public static PxlSequence SqDotKiraAnim;

		public static PxlSequence SqEfSunabokori;

		public static PxlSequence SqLoadingS;

		public static HaloDrawer Halo;

		public static HaloIDrawer HaloI;

		public static HaloIDrawer HaloIA;

		public static DripDrawer Drip;

		public static ElecDrawer Elec;

		public static WindDrawer Wind;

		public static UniKiraDrawer UniKira;

		public static UniBrightDrawer UniBright;

		public static ExpandImageDrawer Expander;

		public static BezierPictDrawer BzPict;

		public static PxlSequence SqEfPattern;

		public static PxlImage PatSelection;

		public static PxlImage EffCircle128;

		public static PxlImage EffBlurCircle245;

		public static PxlImage EffRippleCircle245;

		public static ShockRippleDrawer ShockRipple;

		public static PxlFrame[] AEff;

		public static PhysicsMaterial2D PmdM2Bouncy;

		public static PhysicsMaterial2D PmdM2Zero;

		public static PhysicsMaterial2D PmdM2Enemy;

		public static PhysicsMaterial2D PmdM2Huttobi;

		public static PhysicsMaterial2D PmdM2Fric;

		public static BDic<MFont, FontStorage> OFontStorage;

		public static FontStorage StorageLogoTypeGothic;

		public static FontStorage StorageUtsukusi;

		public static FontStorage StorageCabin;

		public static FontStorage StorageUnityArial;

		public static bool auto_load_efparticle = true;

		public static Triangulator Tri = new Triangulator();

		public const string data_dir = "Basic/Data/";

		public static FrameDrawer MsgFrame2;

		public static FrameDrawer MsgFrame3;

		public static FrameDrawer TalkerFrame3;

		public static FrameDrawer SmallDescFrame;

		public static PxlImage ItemGetDescFrame;

		public static string[] Aadditinal_pmesh_pose_title;

		public const int STENCIL_LESSEQ = 256;

		public const int STENCIL_INCREMENT = 512;

		public static readonly uint[] Acolors = new uint[] { uint.MaxValue, 4294948020U, 4294967040U, 4290445236U, 4290052089U, 4290034431U, 4294956786U, 4287137928U, 4282664004U, 4280427042U };
	}
}
