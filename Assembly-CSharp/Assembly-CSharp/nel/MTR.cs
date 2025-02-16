using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public static class MTR
	{
		public static string[][] Anoel_pxls
		{
			get
			{
				return new string[][]
				{
					new string[] { "noel", "noel_magic", "noel_r18" },
					new string[] { "noel_t", "noel_magic_torned", "noel_r18_torned" },
					new string[] { "noel_babydall" },
					new string[] { "noel_dojo" }
				};
			}
		}

		public static void init1()
		{
			if (MTRX.Aadditinal_pmesh_pose_title != null)
			{
				return;
			}
			MTRX.Aadditinal_pmesh_pose_title = new string[] { "nel", "nel_wholemap", "nel_l", "nel_magic", "lang_btn", "nel_curs" };
			TX.EvReadLocalize += NEL.readLocalizeTxItemScript;
			LoadTicketManager.PrepareLoadManager();
			MTIOneImage mtioneImage;
			PxlCharacter pxlCharacter = MTRX.loadMtiPxc(out mtioneImage, "_icons_l", "Pxl/_icons_l.pxls", "_", false, true, true);
			LoadTicketManager.Instance.AddTicketInner(pxlCharacter, mtioneImage, 0);
			MTIOneImage mtioneImage2;
			PxlCharacter pxlCharacter2 = MTRX.loadMtiPxc(out mtioneImage2, "_patterns", "Pxl/_patterns.pxls", "_", false, true, true);
			LoadTicketManager.Instance.AddTicketInner(pxlCharacter2, mtioneImage2, 0);
		}

		public static bool prepare1
		{
			get
			{
				MTR.init1();
				if (MTR.loaded_1)
				{
					return true;
				}
				if (!MTRX.prepared)
				{
					return false;
				}
				PxlCharacter pxlCharacter = PxlsLoader.getPxlCharacter("_icons_l");
				PxlCharacter pxlCharacter2 = PxlsLoader.getPxlCharacter("_patterns");
				if (!pxlCharacter.isLoadCompleted() || !MTRX.prepared || !pxlCharacter2.isLoadCompleted())
				{
					return false;
				}
				NelItem.readItemScript(Resources.Load<TextAsset>("Data/item").text);
				IN.initKeyAndTextLocalization(true, true);
				MImage mi = MTRX.getMI(pxlCharacter, true);
				MImage mi2 = MTRX.getMI(pxlCharacter2, true);
				if (mi == null || mi2 == null)
				{
					return false;
				}
				NEL.createListenerEval();
				MTR.loaded_1 = true;
				MTRX.SqEfPattern = pxlCharacter2.getPoseByName("pattern_nel").getSequence(0);
				MTR.MIiconL = mi;
				MTR.MIiconP = mi2;
				ReelManager.initReelScript();
				NelMSGResource.initResource(false);
				MTR.DrGhost = new AttackGhostDrawer();
				MTR.DrGhostEn = new AttackGhostDrawer();
				EffectItemNel.initEffectItemNel();
				ENHA.initImage();
				UiGmMapMarker.initItem();
				MTR.DrRad = new RadiationDrawer();
				MTR.SqNelCuteLine = MTRX.PxlIcon.getPoseByName("nel_cute_line").getSequence(0);
				return true;
			}
		}

		public static void prepareShaderMti(bool async = true)
		{
			if ((MTR.loaded_g & 16) != 0)
			{
				return;
			}
			MTR.loaded_g |= 16;
			MTR.MtiShader = new MTI("MTI_ShaderNel", null);
			MTR.MtiShader.addLoadKey("_", async);
		}

		public static bool prepareShaderInstance()
		{
			if ((MTR.loaded_g & 8) != 0)
			{
				return true;
			}
			MTR.prepareShaderMti(false);
			if (!MTR.MtiShader.isAsyncLoadFinished())
			{
				return false;
			}
			PxlCharacter pxlCharacter = PxlsLoader.getPxlCharacter("_icons_l");
			PxlCharacter pxlCharacter2 = PxlsLoader.getPxlCharacter("_patterns");
			if (!pxlCharacter.isLoadCompleted() || !pxlCharacter2.isLoadCompleted())
			{
				return false;
			}
			MTR.loaded_g |= 8;
			MTR.ShaderGDTGradationMap = MTR.getShd("Hachan/ShaderGDTGradationMap");
			MTR.ShaderGDTGradationMapAdd = MTR.getShd("Hachan/ShaderGDTGradationMapAdd");
			MTR.ShaderGDTWaveColor = MTR.getShd("Hachan/ShaderGDTWaveColor");
			MTR.ShaderShiftImage = MTR.getShd("Hachan/PostShiftImage");
			MTR.MtrShiftImage = MTRX.newMtr(MTR.ShaderShiftImage);
			MTR.MtrGDTFinalized = MTRX.newMtr(MTRX.ShaderGDT);
			MTR.MtrFireWall = MTRX.newMtr(MTR.getShd("nel/firewall"));
			MTR.MtrFireBallExplode = MTRX.newMtr(MTR.MtiShader.Load<Material>("MtrFireBallExplode"));
			MTR.MtrFoxChaserBallExplode = MTRX.newMtr(MTR.MtiShader.Load<Material>("MtrFoxChaserBallExplode"));
			MTR.MtrFireWallAdd = MTRX.newMtr(MTR.getShd("nel/firewall_add"));
			MTR.MtrSummonSudden = MTRX.newMtr(MTR.getShd("Hachan/ShaderGDTAddNoiseDissolve"));
			MTR.MtrSummonSudden.SetTexture("_MainTex", MTR.MIiconL.Tx);
			MTRX.setMaterialST(MTR.MtrSummonSudden, "_NoiseTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
			MTR.MtrNDNormal = MTRX.newMtr(MTR.getShd("Hachan/ShaderMeshNormalNoiseDissolve"));
			MTR.MtrNDAdd = MTRX.newMtr(MTR.getShd("Hachan/ShaderMeshAddNoiseDissolve"));
			MTR.MtrNDSub = MTRX.newMtr(MTR.getShd("Hachan/ShaderMeshSubNoiseDissolve"));
			MTR.MtrNDSub2 = MTRX.newMtr(MTR.getShd("Hachan/ShaderMeshSubNoiseDissolve2"));
			MTR.MIiconP = MTR.MIiconP ?? MTRX.getMI(pxlCharacter2, false);
			MTR.MtrConfuseCurtain = MTR.newMtr("Hachan/ConfuseCurtain");
			MTRX.setMaterialST(MTR.MtrConfuseCurtain, "_PatTex", MTRX.SqEfPattern.getImage(7, 0), 0f);
			MTR.MtrConfuseCurtain.SetVector("_Intv", new Vector4(78f, 78f, 2f, 2f));
			MTR.MtrConfuseCurtain.SetColor("_Color", C32.d2c(4284176466U));
			MTR.MtrNoisyMessage = MTR.newMtr("Hachan/NoisyMessage");
			MTRX.setMaterialST(MTR.MtrNoisyMessage, "_MainTex", MTRX.SqEfPattern.getImage(2, 0), 0f);
			MTR.MtrPowerbomb = MTR.newMtr("Hachan/powerbomb");
			MTR.MtrPowerbomb.SetTexture("_MainTex", MTRX.MIicon.Tx);
			MTR.MtrBeam = MTR.newMtr("hachan/Beam");
			MTRX.setMaterialST(MTR.MtrBeam, "_MainTex", MTRX.SqEfPattern.getImage(1, 0), 0f);
			MTR.MtrWaterReflec = MTR.newMtr("Hachan/ShaderWaterReflect");
			MTRX.setMaterialST(MTR.MtrWaterReflec, "_MainTex", MTRX.SqEfPattern.getImage(0, 0), 0f);
			MTR.ANoelBreakCloth = pxlCharacter.getPoseByName("noel_break_cloth").getSequence(0);
			MTRX.setMaterialST(MTR.MtrNDAdd, "_NoiseTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
			MTRX.setMaterialST(MTR.MtrNDSub, "_NoiseTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
			MTRX.setMaterialST(MTR.MtrNDSub2, "_NoiseTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
			MTRX.ShockRipple = new ShockRippleDrawer(MTRX.SqEfPattern.getImage(4, 0), MTR.MIiconP);
			MTR.FrameRipple = new ShockRippleDrawer(MTRX.EffCircle128, MTRX.MIicon);
			MTRX.HaloI = new HaloIDrawer(MTRX.SqEfPattern.getImage(10, 0));
			MTRX.HaloIA = new HaloIDrawer(MTRX.SqEfPattern.getImage(11, 0));
			M2EventItem.PFTalkTarget = MTRX.getPF("talk_target_ttt");
			return true;
		}

		public static void initT()
		{
		}

		public static void finePxlLoadSpeed()
		{
			PxlsLoader.loadSpeed = 0.015625f;
		}

		public static void initGPxls(bool async = true)
		{
			if (async)
			{
				PxlsLoader.loadSpeed = 2f;
			}
			if ((MTR.loaded_g & 4) != 0)
			{
				return;
			}
			MTR.loaded_g |= 4;
			MTR.prepareShaderMti(true);
			MTIOneImage mtioneImage;
			MTR.NoelUiPic = MTRX.loadMtiPxc(out mtioneImage, "NoelUiPic", "PxlNoel/noel_uipic.pxls", "_", false, true, true);
			LoadTicketManager.Instance.AddTicketInner(MTR.NoelUiPic, mtioneImage, 0);
		}

		public static void initG()
		{
			if ((MTR.loaded_g & 1) != 0)
			{
				return;
			}
			Bench.mark("MTR - initG", true, false);
			if ((MTR.loaded_g & 4) == 0)
			{
				MTR.initGPxls(false);
			}
			if (MGV.temp_kisekae > 0)
			{
				NKT.readSpecificFileBinary(MGV.kisekaeBinaryPath(MGV.temp_kisekae), 0, 0, false);
			}
			MTR.initNoelAnimatorPxl();
			SND.loadSheets("m2d", "m2d");
			MTR.VcNoelSource = NEL.prepareVoiceController("noel");
			NDAT.prepareData();
			WeatherItem.initWeather();
			MTR.BkEnhancer = new Block3DDrawer();
			MTR.BkEnhancer.defineBlock2D(new int[] { 0, 0, 0, 1, 1, 1, 2, 1, 1, 2 });
			MTR.BkEnhancer.point_lgt = 0.078125f;
			MTR.BkEnhancer.point_blur_lgt = 0.34375f;
			MTR.BkEnhancer.point_blur_in_alp = 0.25f;
			MTR.BkEnhancer.hen_blur_in_alp = 0.25f;
			MTR.BkEnhancer.hen_thick = 0.015625f;
			MTR.BkEnhancer.block_size_u = 2.1875f;
			MTR.loaded_g |= 1;
		}

		public static void initNoelAnimatorPxl()
		{
			if (MTR.PConNoelAnim != null)
			{
				return;
			}
			string[][] anoel_pxls = MTR.Anoel_pxls;
			LoadTicketManager.PrepareLoadManager();
			LoadTicketManager instance = LoadTicketManager.Instance;
			int num = anoel_pxls.Length;
			for (int i = 0; i < num; i++)
			{
				int num2 = anoel_pxls[i].Length;
				for (int j = 0; j < num2; j++)
				{
					string text = "PxlNoel/" + anoel_pxls[i][j] + ".pxls";
					MTIOneImage mtioneImage;
					PxlCharacter pxlCharacter = MTRX.loadMtiPxc(out mtioneImage, anoel_pxls[i][j], text, "_", true, true, true);
					instance.AddTicketInner(pxlCharacter, mtioneImage, 0);
				}
			}
			MTR.PConNoelAnim = new PrPoseContainer("noel");
		}

		public static bool is_noel_picture_prepared
		{
			get
			{
				if (!MTR.preparedG)
				{
					return false;
				}
				int num = MTR.Anoel_pxls.Length;
				for (int i = 0; i < num; i++)
				{
					int num2 = MTR.Anoel_pxls[i].Length;
					for (int j = 0; j < num2; j++)
					{
						if (MTRX.getMI(PxlsLoader.getPxlCharacter(MTR.Anoel_pxls[i][j]), true) == null)
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		public static void unloadG()
		{
			if (MTR.VcNoelSource != null)
			{
				SND.unloadSheets("m2d", "m2d");
				MTR.VcNoelSource.destruct();
				MTR.VcNoelSource = null;
			}
		}

		public static bool preparedT
		{
			get
			{
				if (!MTR.prepare1)
				{
					return false;
				}
				if (MTR.MeshMarker != null)
				{
					return true;
				}
				PxlCharacter pxlIcon = MTRX.PxlIcon;
				PxlCharacter pxlCharacter = PxlsLoader.getPxlCharacter("_icons_l");
				if (!MTRX.prepared || !pxlIcon.isLoadCompleted() || !pxlCharacter.isLoadCompleted())
				{
					return false;
				}
				IN.initKeyAndTextLocalization(true, true);
				MagicSelector.reloadKindData();
				MTR.MeshMarker = MTRX.getPF("marker_meter");
				MTR.MeshMarker_Sel = MTRX.getPF("marker_meter_selected");
				MTR.MeshMarkerDark = MTRX.getPF("marker_meter_dark");
				MTR.MeshMarkerDark_Sel = MTRX.getPF("marker_meter_selected_dark");
				MTRX.MeshArrowR = MTRX.getPF("arrow_nel");
				MTRX.MeshArrowRShifted = MTRX.getPF("arrow_nel_shifted");
				MTR.MeshCursPoint = MTRX.getPF("wm_curs");
				MTR.SqEfLip = pxlIcon.getPoseByName("lip").getSequence(0);
				MTR.AItemIcon = pxlIcon.getPoseByName("itemrow_category").getSequence(0);
				MTRX.assignPxlImages(MTR.AItemIcon, "itemrow_category");
				MTR.ABubbleS = pxlIcon.getPoseByName("bubble").getSequence(0);
				MTR.AHeartS = pxlIcon.getPoseByName("heart").getSequence(0);
				MTRX.assignPxlImages(pxlCharacter.getPoseByName("nel_magic_l"), false);
				MTR.ASleepingZ = pxlIcon.getPoseByName("sleeping_z").getSequence(0);
				MTR.AEfSmoke = pxlIcon.getPoseByName("gas").getSequence(0);
				MTR.AEfSmokeL = pxlCharacter.getPoseByName("smoke_l").getSequence(0);
				MTR.SqEfSabi = pxlCharacter.getPoseByName("sabi").getSequence(0);
				MTR.SqBezierCutted = pxlCharacter.getPoseByName("bezier_cutted").getSequence(0);
				MTR.ASuddenEye = pxlCharacter.getPoseByName("sudden_eye").getSequence(0);
				MTR.AItemGradeStars = pxlIcon.getPoseByName("nel_item_grade").getSequence(0);
				MTRX.assignPxlImages(MTR.AItemGradeStars, "nel_item_grade");
				MTRX.assignPxlImages("itemicon_empty_bottle", MTR.AItemIcon[4]);
				MTRX.assignPxlImages("itemicon_empty_bottle_filled", MTR.AItemIcon[11]);
				MTR.AImgMapMarker = pxlIcon.getPoseByName("map_marker").getSequence(0);
				MTRX.assignPxlImages("money_crafts", MTR.AImgMapMarker[12]);
				MTRX.assignPxlImages(pxlIcon.getPoseByName("weather"), false);
				MTRX.assignPxlImages(pxlCharacter.getPoseByName("nel_l"), false);
				MTRX.assignPxlImages(pxlCharacter.getPoseByName("nel_l2"), false);
				MTR.AReelEffect = pxlIcon.getPoseByName("nel_reel_effect").getSequence(0);
				MTRX.assignPxlImages(MTR.AReelEffect, "nel_reel_effect");
				PxlPose poseByName = pxlCharacter.getPoseByName("particle_splash");
				MTR.SqParticleSplash = poseByName.getSequence(0);
				MTR.SqParticleSplashFixed = poseByName.getSequence(1);
				MTR.SqParticleSperm = pxlCharacter.getPoseByName("particle_sperm").getSequence(0);
				MTR.SqParticleBetoWebTrapped = pxlCharacter.getPoseByName("nel_beto_web_trapped").getSequence(0);
				MTR.MeshSuriken = MTRX.getPF("suriken");
				MTR.ANelChars = pxlIcon.getPoseByName("nel_character").getSequence(0);
				MTR.AMsgTalkerBg = pxlIcon.getPoseByName("nel_other").getSequence(0);
				MTR.SqPatHeartVoltage = pxlIcon.getPoseByName("pt_heart_voltage").getSequence(0);
				MTR.ImgNew = MTRX.getPF("new_icon");
				MTR.ImgGroundShockWave = MTRX.getPF("ground_shockwave").getLayer(0).Img;
				MTR.SqLOther = pxlCharacter.getPoseByName("l_other_sq").getSequence(0);
				MTR.SqEfStainBurning = pxlIcon.getPoseByName("effect_burning").getSequence(0);
				PxlPose poseByName2 = pxlIcon.getPoseByName("effect_icefloor");
				MTR.SqEfStainIce = poseByName2.getSequence(0);
				MTR.SqEfStainIcePtc = poseByName2.getSequence(1);
				MTR.SqEfStainWebFloor = pxlIcon.getPoseByName("effect_webfloor").getSequence(0);
				MTR.ImgShockWave160 = MTR.SqLOther.getImage(0, 0);
				MTR.SqCheckPointBubble = pxlIcon.getPoseByName("checkpoint_bubble").getSequence(0);
				MTR.SqAlchemyRowSlot = pxlIcon.getPoseByName("nel_recipe_slot").getSequence(0);
				MTR.SqReelIcon = pxlIcon.getPoseByName("nel_reel_icon").getSequence(0);
				MTR.SqMatoateTarget = pxlIcon.getPoseByName("_anim_puzzle_mato").getSequence(0);
				MTR.SqPeeSlot = pxlIcon.getPoseByName("nel_pee_slot").getSequence(0);
				MTR.SqRebagacha = pxlCharacter.getPoseByName("nel_rebagacha").getSequence(0);
				PxlSequence sequence = pxlCharacter.getPoseByName("mbox").getSequence(0);
				MTR.DrMBox = new MBoxDrawer().Create(12, sequence.getImage(0, 0), sequence.getImage(1, 0), sequence.getImage(2, 0), sequence.getImage(3, 0));
				MTR.DrReelBox = new NelMBoxDrawer(MTR.DrMBox);
				MTR.ChrNelS = new BMListChars(5, 5, 64f);
				MTR.ChrNelS.marginh = 4;
				MTR.ChrNelS.marginw = 1;
				MTR.ChrNelS.frexible = 0f;
				MTR.ChrNelS.add(pxlIcon.getPoseByName("nel_character_2").getSequence(0), "1234567890!#$ABCDEFGHIJKLMNOPQRSTUVWXYZ.:-[]/_,%", 0);
				MTR.ImgRegisterNoel = pxlIcon.getPoseByName("nel").getSequence(0).getFrameByName("register_noel")
					.getLayer(0)
					.Img;
				SkillManager.initScript();
				ENHA.initScript();
				RCP.initScript();
				QuestTracker.initQuestScript();
				MeshDrawer meshDrawer = new MeshDrawer(null, 4, 6);
				meshDrawer.draw_gl_only = true;
				meshDrawer.activate("", MTRX.MtrMeshNormal, false, MTRX.ColWhite, null);
				UniKiraDrawer uniKiraDrawer = new UniKiraDrawer();
				uniKiraDrawer.kaku = 9;
				uniKiraDrawer.Radius(80f, 71f).Dent(0.5f, 0.6f).Focus(0.4f, 0.6f);
				uniKiraDrawer.drawTo(meshDrawer, 0f, 0f, 0.25132743f, false, 0f, 0f);
				MTR.PMeshGachaUni = meshDrawer.createSimplePxlMesh(null, false, false, true);
				meshDrawer.clearSimple();
				uniKiraDrawer.kaku = 11;
				uniKiraDrawer.Dent(1.24f, 1.3f).Focus(0.5f, 0.5f);
				uniKiraDrawer.drawTo(meshDrawer, 0f, 0f, 0.25132743f, false, 0f, 0f);
				MTR.PMeshGachaCircle = meshDrawer.createSimplePxlMesh(null, false, false, true);
				meshDrawer.destruct();
				NelItem.getWholeDictionary().scriptFinalize();
				return true;
			}
		}

		public static bool preparedG
		{
			get
			{
				if (!MTR.prepare1 || !MTR.preparedT)
				{
					return false;
				}
				if (MTR.PConNoelAnim == null)
				{
					MTR.initNoelAnimatorPxl();
				}
				if ((MTR.loaded_g & 1) == 0)
				{
					return false;
				}
				if ((MTR.loaded_g & 2) == 0)
				{
					PxlCharacter pxlIcon = MTRX.PxlIcon;
					PxlCharacter pxlCharacter = PxlsLoader.getPxlCharacter("_icons_l");
					PxlCharacter pxlCharacter2 = PxlsLoader.getPxlCharacter("_patterns");
					if (!pxlIcon.isLoadCompleted() || !pxlCharacter.isLoadCompleted() || !pxlCharacter2.isLoadCompleted() || !SND.loaded || !MTR.NoelUiPic.isLoadCompleted() || !MTR.MtiShader.isAsyncLoadFinished())
					{
						return false;
					}
					MImage mi = MTRX.getMI(MTR.NoelUiPic, true);
					if (mi == null || mi.Tx == null)
					{
						return false;
					}
					if (!MTR.PConNoelAnim.iniPxlResources<PRNoel.OUTFIT>(MTR.Anoel_pxls, 56f))
					{
						return false;
					}
					MTR.loaded_g |= 2;
					MTR.prepareShaderInstance();
					MTR.SqEfLeaf = pxlIcon.getPoseByName("effect_leaf").getSequence(0);
					MTR.AEfWaterSplash = pxlIcon.getPoseByName("effect_splash").getSequence(0);
					MTR.AEfKagaribiActivate = pxlIcon.getPoseByName("kagaribi_activate").getSequence(0);
					MTR.SqEfManaLight = pxlIcon.getPoseByName("mana_crystal").getSequence(0);
					MTR.AEfManaTargetting = pxlIcon.getPoseByName("mana_crystal_targetting").getSequence(0);
					MTR.AGauge = MTRX.getLayerArray(pxlIcon.getPoseByName("gage_bar"), 0);
					MTR.SqUiBarBright = pxlIcon.getPoseByName("bar_bright").getSequence(0);
					MTR.SqGageCrack = pxlIcon.getPoseByName("gage_crack").getSequence(0);
					MTR.SqNpcLoveUp = pxlIcon.getPoseByName("nel_npc_love_up").getSequence(0);
					MTR.SqNpcSmoke = pxlIcon.getPoseByName("nel_npc_smoke").getSequence(0);
					MTR.AGaugeSplit = pxlIcon.getPoseByName("gage_split").getSequence(0);
					MTR.ALayedWormChild = pxlIcon.getPoseByName("worm_child").getSequence(0);
					MTR.ALayedWormEgg = pxlIcon.getPoseByName("layed_egg").getSequence(0);
					MTR.ALayedSlimeChild = pxlIcon.getPoseByName("slime_child").getSequence(0);
					MTR.ALayedMushChild = pxlIcon.getPoseByName("mush_child").getSequence(0);
					MTR.ALayedSlimeEgg = pxlIcon.getPoseByName("layed_egg_slime").getSequence(0);
					MTR.ARainFoot = pxlIcon.getPoseByName("rainfoot").getSequence(0);
					MTR.ABikubikuChars = pxlCharacter.getPoseByName("bikubiku_char").getSequence(0);
					MTR.AWormRelease = pxlIcon.getPoseByName("worm_release").getSequence(0);
					MTR.AOcSlots = pxlIcon.getPoseByName("nel_oc_slot").getSequence(0);
					MTR.PFQuestPin = MTRX.getPF("quest_pin");
					MTR.PFQuestPinBorder = MTRX.getPF("quest_pin_border");
					MTR.AMagicIconS = pxlIcon.getPoseByName("MAGIC_ICON").getSequence(0);
					MTR.AMagicIconL = pxlCharacter.getPoseByName("MAGIC_ICON_L").getSequence(0);
					MTR.AWholeMapGoto = pxlIcon.getPoseByName("wholemap_goto").getSequence(0);
					MTR.SqEffectFireBall = pxlIcon.getPoseByName("effect_fireball").getSequence(0);
					MTR.SqEffectFrozenBullet = pxlIcon.getPoseByName("effect_freezeshot").getSequence(0);
					MTR.SqEffectBigBomb = pxlIcon.getPoseByName("effect_bigbomb").getSequence(0);
					MTR.SqEffectItemBomb = pxlIcon.getPoseByName("effect_itembomb").getSequence(0);
					MTR.SqEffectCeilFall = pxlIcon.getPoseByName("effect_ceilfall").getSequence(0);
					MTR.ShaderEnemyDark = MTR.getShd("nel/enemydark");
					MTR.ShaderEnemyDarkWhiter = MTR.getShd("nel/enemydark_whiter");
					MTR.ShaderDarkWhiteParlin = MTR.getShd("nel/enemywhiteparlin");
					MTR.ShaderTransparentParlin = MTR.getShd("nel/enemytransparentparlin");
					MTR.ShaderEnemyAulaAdd = MTR.getShd("nel/EnemyAulaAdd");
					MTR.ShaderEnemyBuffer = MTR.getShd("nel/EnemyBuffer");
					MTR.SqFrozen = pxlCharacter.getPoseByName("frozen").getSequence(0);
					MTR.SqSpermShot = pxlCharacter.getPoseByName("particle_shotsperm").getSequence(0);
					MTRX.setMaterialST(MTR.MtrFireWall, "_DarkTex", MTRX.SqEfPattern.getImage(0, 0), 0f);
					MTRX.setMaterialST(MTR.MtrFireWallAdd, "_DarkTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
					MTR.DrWorm = new WormDrawer(MTRX.getLayerArray(pxlIcon.getPoseByName("worm"), 0)[0].Img).DefineSplit(1f, new float[] { 0f, 5f, 20f, 32f, 48f, 62f, 76f });
					MTR.APuzzleMarker = pxlIcon.getPoseByName("puzzle_marker").getSequence(0);
					ReelExecuter.initReelExecuter();
					if (Bench.now_stream_key == "MTR - initG")
					{
						Bench.mark(null, false, false);
					}
					MTR.EplPendulumTapCircle = new EfParticleOnce("general_white_circle", EFCON_TYPE.NORMAL);
					WAManager.initWAScript(pxlCharacter.getPoseByName("nel_wa").getSequence(0));
					NelTreasureBoxDrawer.initAnimT();
				}
				return true;
			}
		}

		public static Shader getShd(string shader_name)
		{
			return MTR.MtiShader.LoadShader(shader_name);
		}

		public static MTI getShdContainer()
		{
			return MTR.MtiShader;
		}

		public static Material newMtr(string shader_name)
		{
			return MTRX.newMtr(MTR.getShd(shader_name));
		}

		public static Material newMtrStone()
		{
			Material material = MTR.newMtr("hachan/Stone");
			MTRX.setMaterialST(material, "_NoiseTex", MTRX.SqEfPattern.getImage(2, 0), 0f);
			MTRX.setMaterialST(material, "_NoiseTex2", MTRX.SqEfPattern.getImage(6, 0), 0f);
			MTRX.setMaterialST(material, "_WaveTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
			return material;
		}

		public static Material newMtrFrozenMain()
		{
			Material material = MTR.newMtr("hachan/Frozen");
			MTRX.setMaterialST(material, "_NoiseTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
			return material;
		}

		public static void newMtrFrozenAdditional(out Material MtrFrozenGdtA, out Material MtrFrozenGdtS)
		{
			MtrFrozenGdtA = MTR.newMtr("hachan/FrozenGDTA");
			MTRX.setMaterialST(MtrFrozenGdtA, "_NoiseTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
			MtrFrozenGdtS = MTR.newMtr("hachan/FrozenGDTS");
			MTRX.setMaterialST(MtrFrozenGdtS, "_NoiseTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
		}

		public static void initCameraAfter()
		{
			M2Camera cam = (M2DBase.Instance as NelM2DBase).Cam;
			MTR.MtrGDTFinalized.SetTexture("_MainTex", cam.getFinalizedTexture());
			MTR.MtrPowerbomb.SetTexture("_BorderTex", cam.CurDgn.getBorderTexture() ?? cam.getFinalizedTexture());
			MTR.MtrPowerbomb.SetTexture("_BgTex", cam.getBackGroundRendered());
			MTR.MtrPowerbomb.SetTexture("_MoverTex", cam.getMoverTexture());
		}

		public static void mapActionInitted()
		{
			PxlCharacter pxlCharacter = PxlsLoader.getPxlCharacter("MapChip_worm");
			MTR.SqWormAnim0 = (MTR.SqWormAnim1 = null);
			if (pxlCharacter != null)
			{
				PxlPose poseByName = pxlCharacter.getPoseByName("_anim_worm");
				MTR.SqWormAnim0 = poseByName.getSequence(0);
				MTR.SqWormAnim1 = poseByName.getSequence(1);
			}
		}

		public static PxlImage[] getPImgArray(PxlPose P)
		{
			return MTRX.getPImgArray(P);
		}

		public static string Read(string path_from_data_dir, string def_str = "", string ext = ".csv")
		{
			string resource = TX.getResource("Data/" + path_from_data_dir, ext, false);
			if (!TX.valid(resource))
			{
				return def_str;
			}
			return resource;
		}

		public static PxlCharacter NoelUiPic;

		public static PxlCharacter PxlM2DGeneral;

		public static PrPoseContainer PConNoelAnim;

		public static PxlSequence SqEfLeaf;

		public static PxlSequence ANoelBreakCloth;

		public static PxlSequence SqEfManaLight;

		public static PxlSequence SqEfLip;

		public static PxlSequence AEfManaTargetting;

		public static PxlSequence AEfWaterSplash;

		public static PxlSequence AEfSmoke;

		public static PxlSequence SqEfSabi;

		public static PxlSequence AEfSmokeL;

		public static PxlSequence AEfKagaribiActivate;

		public static PxlSequence SqBezierCutted;

		public static PxlSequence AGaugeSplit;

		public static PxlSequence SqWormAnim0;

		public static PxlSequence SqWormAnim1;

		public static PxlSequence SqEffectFireBall;

		public static PxlSequence SqEffectCeilFall;

		public static PxlSequence SqEffectFrozenBullet;

		public static PxlSequence SqEffectBigBomb;

		public static PxlSequence SqEffectItemBomb;

		public static PxlSequence AItemIcon;

		public static PxlSequence AReelEffect;

		public static PxlSequence ALayedWormChild;

		public static PxlSequence ALayedWormEgg;

		public static PxlSequence ALayedSlimeChild;

		public static PxlSequence ALayedMushChild;

		public static PxlSequence ALayedSlimeEgg;

		public static PxlSequence ARainFoot;

		public static PxlSequence ABikubikuChars;

		public static PxlSequence AMagicIconS;

		public static PxlSequence AMagicIconL;

		public static PxlSequence ABubbleS;

		public static PxlSequence AHeartS;

		public static PxlSequence ASleepingZ;

		public static PxlSequence AWormRelease;

		public static PxlSequence AMsgTalkerBg;

		public static PxlSequence ASuddenEye;

		public static PxlSequence AItemGradeStars;

		public static PxlSequence SqParticleSplash;

		public static PxlSequence SqParticleSplashFixed;

		public static PxlSequence SqParticleSperm;

		public static PxlSequence SqParticleBetoWebTrapped;

		public static PxlSequence SqRebagacha;

		public static PxlSequence AImgMapMarker;

		public static PxlSequence SqPatHeartVoltage;

		public static PxlSequence SqCheckPointBubble;

		public static PxlSequence SqAlchemyRowSlot;

		public static PxlSequence SqReelIcon;

		public static PxlSequence SqMatoateTarget;

		public static PxlSequence SqNelCuteLine;

		public static PxlLayer[] AGauge;

		public static PxlSequence SqUiBarBright;

		public static PxlSequence SqGageCrack;

		public static PxlSequence SqNpcLoveUp;

		public static PxlSequence SqNpcSmoke;

		public static PxlSequence ANelChars;

		public static PxlImage ImgRegisterNoel;

		public static PxlFrame ImgNew;

		public static PxlImage ImgGroundShockWave;

		public static PxlImage ImgShockWave160;

		public static PxlSequence SqFrozen;

		public static PxlSequence SqSpermShot;

		public static PxlSequence AOcSlots;

		public static PxlFrame NoticeExcPF;

		public static PxlFrame PFQuestPin;

		public static PxlFrame PFQuestPinBorder;

		public static PxlMeshDrawer PMeshGachaUni;

		public static PxlMeshDrawer PMeshGachaCircle;

		public static VoiceController VcNoelSource;

		public static PxlSequence SqPeeSlot;

		private static MTI MtiShader;

		public static Material MtrGDTFinalized;

		public static Material MtrNDAdd;

		public static Material MtrNDSub;

		public static Material MtrNDSub2;

		public static Material MtrNDNormal;

		public static PxlFrame MeshMarker;

		public static PxlFrame MeshMarker_Sel;

		public static PxlFrame MeshMarkerDark;

		public static PxlFrame MeshMarkerDark_Sel;

		public static PxlFrame MeshCursPoint;

		public static PxlFrame MeshSuriken;

		public static PxlSequence APuzzleMarker;

		public static PxlSequence AWholeMapGoto;

		public static PxlSequence SqLOther;

		public static PxlSequence SqEfStainBurning;

		public static PxlSequence SqEfStainIce;

		public static PxlSequence SqEfStainIcePtc;

		public static PxlSequence SqEfStainWebFloor;

		public static Shader ShaderShiftImage;

		public static Shader ShaderGDTGradationMap;

		public static Shader ShaderGDTGradationMapAdd;

		public static Shader ShaderGDTWaveColor;

		public static Material MtrShiftImage;

		public static Shader ShaderEnemyDark;

		public static Shader ShaderEnemyDarkWhiter;

		public static Shader ShaderDarkWhiteParlin;

		public static Shader ShaderTransparentParlin;

		public static Material MtrFireWall;

		public static Material MtrFireWallAdd;

		public static Material MtrGaussianBlur;

		public static Material MtrSummonSudden;

		public static Material MtrConfuseCurtain;

		public static Material MtrNoisyMessage;

		public static Material MtrPowerbomb;

		public static Shader ShaderEnemyAulaAdd;

		public static Shader ShaderEnemyBuffer;

		public static Material MtrBeam;

		public static Material MtrWaterReflec;

		public static ShockRippleDrawer FrameRipple;

		public static Material MtrFireBallExplode;

		public static Material MtrFoxChaserBallExplode;

		public static Block3DDrawer BkEnhancer;

		public static WormDrawer DrWorm;

		public static AttackGhostDrawer DrGhost;

		public static AttackGhostDrawer DrGhostEn;

		public static MBoxDrawer DrMBox;

		public static NelMBoxDrawer DrReelBox;

		public static BMListChars ChrNelS;

		public static MImage MIiconL;

		public static MImage MIiconP;

		public static EfParticleOnce EplPendulumTapCircle;

		public static RadiationDrawer DrRad;

		public static uint col_pr_mana = 4287299550U;

		public static uint col_pr_no_mana = 3439329279U;

		public static uint col_en_mana = 4294914983U;

		public static uint col_blood = 4292220952U;

		public static uint col_blood_ep = 4292930028U;

		public static uint col_blood_egg = 4290042551U;

		public const int STENCIl_REF_SUMMONER = 8;

		public const string nel_data_dir = "Data/";

		private static bool loaded_1 = false;

		private static int loaded_g = 0;

		public static readonly uint[] Apuzzle_switch_col = new uint[] { 4287430536U, 4288413695U, 4293852974U, 4294945840U, 4291047240U };

		public static readonly uint[] Apuzzle_switch_col_dark = new uint[] { 4278241839U, 4284394751U, 4292852224U, 4294258944U, 4287103597U };

		public static readonly uint[] Apuzzle_switch_col_sub = new uint[] { 4285288900U, 4288431429U, 4283201216U, 4281437590U, 4280966992U };
	}
}
