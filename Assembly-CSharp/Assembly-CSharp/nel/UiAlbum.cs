using System;
using System.Collections.Generic;
using evt;
using m2d;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class UiAlbum : UiBoxDesignerFamilyEvent, IEventWaitListener
	{
		public string[] Acateg_tab_keys
		{
			get
			{
				return new string[]
				{
					TX.Get("Album_Tab_Memory", ""),
					TX.Get("Album_Tab_Fatal", ""),
					TX.Get("MAP_house_theroom", ""),
					TX.Get("Album_Tab_Osok", "")
				};
			}
		}

		public float confw
		{
			get
			{
				return IN.w * 0.45f;
			}
		}

		public float confh
		{
			get
			{
				return IN.h * 0.5f;
			}
		}

		private float outw
		{
			get
			{
				return IN.w - 80f;
			}
		}

		private float outh
		{
			get
			{
				return IN.h - 210f;
			}
		}

		private float bxrx
		{
			get
			{
				return -this.outw * 0.5f + 373f;
			}
		}

		private float bxry
		{
			get
			{
				return -this.outh * 0.5f + 242f;
			}
		}

		private float taby
		{
			get
			{
				return this.outh * 0.5f - 14f + 22f;
			}
		}

		private float mdrx
		{
			get
			{
				return 373f;
			}
		}

		public static PxlCharacter loadMaterial(out int ev_cache_read_number)
		{
			PxlCharacter pxlCharacter = PxlsLoader.getPxlCharacter("ui_album");
			ev_cache_read_number = 0;
			if (pxlCharacter == null)
			{
				pxlCharacter = UiAlbum.loadMaterial();
				ev_cache_read_number = 2;
			}
			else if (pxlCharacter.isLoading())
			{
				ev_cache_read_number = 2;
			}
			return pxlCharacter;
		}

		public static PxlCharacter loadMaterial()
		{
			return MTRX.loadMtiPxc("ui_album", "EvImg/ui_album.pxls", "M2D", false, true);
		}

		protected override void Awake()
		{
			base.Awake();
			this.stabilize_key = "ALBUM" + IN.totalframe.ToString();
			this.debug_show_all = global::XX.X.DEBUGALBUMUNLOCK;
			this.BaGF = new ByteArray(0U);
			GF.writeSvString(this.BaGF);
			if (M2DBase.Instance != null)
			{
				M2DBase.Instance.removeLMtrEntryManual(M2DBase.LM_TYPE.PXL, "ui_album");
				M2DBase.Instance.FlagValotStabilize.Add(this.stabilize_key);
			}
			BGM.getFrontBgm(out this.def_bgm_timing, out this.def_bgm_cue);
			this.auto_deactive_gameobject = false;
			this.stt = UiAlbum.STATE.INITIALIZE;
			this.Pc = PxlsLoader.getPxlCharacter("ui_album");
			if (this.Pc == null)
			{
				this.Pc = UiAlbum.loadMaterial();
			}
			this.AAentry = new List<UiAlbum.AlbumEntry>[4];
			this.Afirst_select = new int[4];
			TRMManager.initTRMScript();
			EV.initWaitFn(this, 0);
			this.runner_assigned = true;
			this.clearEvVariable();
		}

		public void setDefBGM(string bgm_timing, string bgm_cue)
		{
			BgmKind fromSheet = BgmKind.GetFromSheet(bgm_timing);
			if (fromSheet == null)
			{
				global::XX.X.de("不明なBgm timing: " + bgm_timing, null);
				return;
			}
			if (TX.noe(bgm_cue))
			{
				bgm_cue = fromSheet.default_que;
			}
			this.def_bgm_timing = bgm_timing;
			this.def_bgm_cue = bgm_cue;
		}

		public void resetGFValue()
		{
			if (this.BaGF != null)
			{
				this.BaGF.position = 0UL;
				GF.readFromSvString(this.BaGF);
			}
		}

		public void resetBgm(bool only_load = false)
		{
			if (TX.valid(this.def_bgm_timing) && !BGM.frontBGMIs(this.def_bgm_timing, this.def_bgm_cue))
			{
				BGM.load(this.def_bgm_timing, this.def_bgm_cue, false);
				if (!only_load)
				{
					BGM.replace(120f, 100f, true, false);
					return;
				}
			}
			else if (!BGM.isFrontPlaying() && !only_load)
			{
				BGM.fadein(100f, 120f);
			}
		}

		private void clearEvVariable()
		{
			EV.getVariableContainer().define("_scene", "", true);
			EV.getVariableContainer().define("_album_categ", "", true);
		}

		public void createInstance()
		{
			if (this.Pc.getExternalTextureArray()[0].Image == null)
			{
				UiAlbum.loadMaterial();
			}
			this.base_z = -0.05f;
			this.SqRightThumb = this.Pc.getPoseByName("thumbnail").getSequence(0);
			this.MtrThumb = new Material(MTRX.ShaderGDT);
			this.MtrThumbBlur = new Material(MTRX.ShaderGDTNormalBlur);
			this.MdR = MeshDrawer.prepareMeshRenderer(base.gameObject, this.MtrThumb, 0f, -1, null, base.use_valotile, false);
			Texture image = this.Pc.getExternalTextureArray()[0].Image;
			this.MdR.initForImgAndTexture(image);
			this.MtrThumbBlur.SetTexture("_MainTex", image);
			this.MtrThumbBlur.SetFloat("_Level", 4f);
			this.MtrThumbBlur.SetColor("_Color", MTRX.ColBlack);
			this.MdR.setMaterialCloneFlag();
			this.BxL = base.Create("BxL", this.bxrx, 0f, 746f, 484f, 0, 40f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxL.margin_in_lr = 40f;
			this.BxL.margin_in_tb = 30f;
			this.BxL.use_scroll = true;
			this.BxL.init();
			this.BConList = this.BxL.addRadioT<aBtnNel>(new DsnDataRadio
			{
				name = "list",
				skin = "album_thumb",
				clms = 4,
				margin_w = 0,
				margin_h = 0,
				navi_loop = 3,
				w = this.BxL.use_w / 4f,
				h = this.BxL.use_h / 3.5f,
				fnGenerateKeys = new FnGenerateRemakeKeys(this.FnGenerateThumbKeys),
				fnMakingAfter = new BtnContainer<aBtn>.FnBtnMakingBindings(this.FnMakeListThumbBtn),
				fnHover = new FnBtnBindings(this.fnHoverThumbBtn),
				fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.FnChangedListThumb),
				def = -1
			});
			this.TxComingSoon = IN.CreateGob(this.BxL.gameObject, "-TxCS").AddComponent<TextRenderer>();
			IN.setZ(this.TxComingSoon.transform, -0.125f);
			this.TxComingSoon.text_content = TX.Get("Album_coming_soon", "");
			this.TxComingSoon.Col(C32.MulA(4288057994U, 1f)).Align(ALIGN.CENTER).AlignY(ALIGNY.MIDDLE)
				.Size(18f);
			this.TxComingSoon.gameObject.SetActive(false);
			this.BConList.APool = new List<aBtn>(4);
			this.BxL.Focusable(true, true, null);
			this.BxTab = base.Create("BxTab", this.bxrx, this.taby, 566f, 28f, 3, 70f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxTab.getBox().frametype = UiBox.FRAMETYPE.DARK_SIMPLE;
			this.BxTab.Smallest();
			this.BxTab.item_margin_x_px = 44f;
			this.BxTab.item_margin_y_px = 4f;
			this.BxTab.init();
			this.RTabbar = ColumnRow.CreateT<aBtn>(this.BxTab, "tab", "command", 0, this.Acateg_tab_keys, null, this.BxTab.use_w, 28f - this.BxTab.item_margin_y_px * 2f, false, false);
			this.RTabbar.initOneBtnClmn(new ColumnRow.FnOneBtnClmn(this.changeTab));
			this.RTabbar.LR_valotile = true;
			this.BxConfirm = base.Create("BxConfirm", 0f, 20f, this.confw, this.confh, 1, 40f, UiBoxDesignerFamily.MASKTYPE.BOX);
			this.BxConfirm.margin_in_lr = 60f;
			this.BxConfirm.margin_in_tb = 40f;
			this.BxConfirm.item_margin_y_px = 30f;
			this.BxConfirm.Focusable(true, true, null);
			this.BxConfirm.alignx = ALIGN.CENTER;
			this.BxConfirm.init();
			this.FbConfirm = this.BxConfirm.addImg(new DsnDataImg
			{
				swidth = this.BxConfirm.use_w - 40f,
				sheight = this.BxConfirm.use_h - 28f - this.BxConfirm.item_margin_y_px - 4f,
				FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.FnDrawConfirmImg),
				text_auto_wrap = true,
				text = " ",
				size = 16f,
				TxCol = C32.d2c(4283780170U),
				aligny = ALIGNY.BOTTOM,
				html = true
			});
			this.BxConfirm.Br();
			this.BConConfirm = this.BxConfirm.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
			{
				name = "confirm",
				titles = new string[] { "&&Album_Play_Memory", "&&Cancel" },
				skin = "normal",
				clms = 2,
				w = this.BxConfirm.use_w * 0.42f,
				h = 28f,
				margin_w = 10f,
				fnClick = new FnBtnBindings(this.fnClickConfirm)
			});
			this.BxConfirm.deactivate();
			base.setAutoActivate(this.BxConfirm, false);
			this.reloadScript();
			this.initTab();
		}

		public void reloadScript()
		{
			string resource = TX.getResource("Data/album", ".csv", false);
			if (resource == null)
			{
				return;
			}
			UiAlbum.CATEG cur_categ = UiAlbum.CATEG.MEMORY;
			List<UiAlbum.AlbumEntry> list = null;
			PxlSequence pxlSequence = null;
			int num = -1;
			UiAlbum.AlbumEntry CurBe = default(UiAlbum.AlbumEntry);
			CsvReader csvReader = new CsvReader(resource, CsvReader.RegSpace, false);
			Action action = delegate
			{
				if (CurBe.key != null)
				{
					if (CurBe.visible || CurBe.visible_unlock)
					{
						this.categ_activated |= 1U << (int)cur_categ;
					}
					CurBe.key = null;
				}
			};
			while (csvReader.read())
			{
				if (TX.isStart(csvReader.cmd, '#'))
				{
					string text = TX.slice(csvReader.cmd, 1);
					UiAlbum.CATEG categ;
					if (Enum.TryParse<UiAlbum.CATEG>(text, out categ))
					{
						action();
						cur_categ = categ;
						list = this.AAentry[(int)cur_categ];
						if (list == null)
						{
							list = (this.AAentry[(int)cur_categ] = new List<UiAlbum.AlbumEntry>(4));
						}
						PxlPose poseByName = this.Pc.getPoseByName(text);
						pxlSequence = null;
						CurBe = default(UiAlbum.AlbumEntry);
						num = -1;
						if (poseByName == null)
						{
							csvReader.tError("不明なPxlSequence: " + text);
						}
						else
						{
							pxlSequence = poseByName.getSequence(0);
						}
					}
					else
					{
						csvReader.tError("不明なCATEG: " + csvReader.cmd);
					}
				}
				else if (list != null && pxlSequence != null)
				{
					if (csvReader.cmd == "/*" || csvReader.cmd == "/*___")
					{
						string index = csvReader.getIndex((csvReader.cmd == "/*") ? 2 : 1);
						num = this.getEntryIndex(list, index);
						if (num >= 0)
						{
							csvReader.tError("重複エントリー:" + index);
							continue;
						}
						action();
						UiAlbum.AlbumEntry albumEntry = new UiAlbum.AlbumEntry
						{
							key = index,
							PF = pxlSequence.getFrameByName(index)
						};
						if (albumEntry.PF == null)
						{
							csvReader.tError("ui_album.pxl 内にサムネイルPFが存在しない:" + index);
						}
						if (cur_categ == UiAlbum.CATEG.MEMORY)
						{
							albumEntry.title_localized = TX.Get("Album_Memory_" + index, "");
						}
						if (cur_categ == UiAlbum.CATEG.FATAL)
						{
							albumEntry.title_localized = TX.Get("Enemy_" + index.ToUpper(), "");
						}
						if (cur_categ == UiAlbum.CATEG.FATAL || cur_categ == UiAlbum.CATEG.TROOM || cur_categ == UiAlbum.CATEG.OSOK)
						{
							albumEntry.visible = MGV.isFatalSceneAlreadyWatched(index);
							albumEntry.sensitive = true;
						}
						num = list.Count;
						TX tx = TX.getTX("Album_visible_term_" + albumEntry.key, true, true, null);
						if (tx != null)
						{
							albumEntry.ui_term = tx.text;
						}
						if (this.debug_show_all)
						{
							albumEntry.visible = true;
						}
						CurBe = albumEntry;
						list.Add(albumEntry);
					}
					if (num >= 0)
					{
						UiAlbum.AlbumEntry curBe = CurBe;
						if (csvReader.cmd == "%CLONE")
						{
							int entryIndex = this.getEntryIndex(this.AAentry[(int)cur_categ], csvReader._1);
							if (entryIndex < 0)
							{
								csvReader.tError("%CLONE: ソースが不明 " + csvReader._1);
								continue;
							}
							curBe.CopyFrom(this.AAentry[(int)cur_categ][entryIndex]);
						}
						if (csvReader.cmd == "episode")
						{
							if (cur_categ != UiAlbum.CATEG.TROOM)
							{
								csvReader.tError("episode はTROOMカテゴリのみで有効");
								continue;
							}
							curBe.Trm = TRMManager.Get(csvReader._1, false);
							if (curBe.Trm == null)
							{
								continue;
							}
							bool flag = TX.isEnd(curBe.key, "a");
							curBe.visible = curBe.visible || (flag ? curBe.Trm.watched_a : curBe.Trm.watched_b);
							curBe.visible_unlock = curBe.visible_unlock || curBe.Trm.is_active;
							curBe.title_localized = TX.Get("Trm_" + csvReader._1, "") + "\n( " + TX.Get(flag ? "TrmUi_root_a" : "TrmUi_root_b", "") + ")";
						}
						if (csvReader.cmd == "visible")
						{
							curBe.visible = curBe.visible || TX.eval(csvReader.slice_join(1, " ", ""), "") != 0.0;
						}
						if (csvReader.cmd == "visible_unlock")
						{
							curBe.visible_unlock = curBe.visible_unlock || TX.eval(csvReader.slice_join(1, " ", ""), "") != 0.0;
						}
						if (csvReader.cmd == "title_localized")
						{
							curBe.title_localized = TX.ReplaceTX(csvReader.slice_join(1, " ", ""), false);
						}
						if (csvReader.cmd == "sensitive")
						{
							curBe.sensitive = csvReader.Nm(1, 1f) != 0f;
						}
						if (csvReader.cmd == "term")
						{
							curBe.ui_term = TX.Get("Album_visible_term_" + csvReader._1, "");
						}
						list[num] = (CurBe = curBe);
					}
				}
			}
			action();
		}

		public override void destruct()
		{
			base.destruct();
			if (M2DBase.Instance != null)
			{
				M2DBase.Instance.FlagValotStabilize.Rem(this.stabilize_key);
			}
			if (this.MdR != null)
			{
				this.MdR.destruct();
				this.MdR = null;
				Object.Destroy(this.MtrThumbBlur);
			}
		}

		public override UiBoxDesignerFamily deactivate(bool immediate = false)
		{
			return this.deactivate(false, immediate);
		}

		public UiBoxDesignerFamily deactivate(bool is_temporary, bool immediate)
		{
			base.deactivate(immediate);
			if (this.t >= 0f)
			{
				this.t = global::XX.X.Mn(-1f, -30f + this.t);
			}
			this.auto_deactive_gameobject = false;
			if (!is_temporary)
			{
				this.stt = UiAlbum.STATE.DEACTIVATING;
				this.clearEvVariable();
				this.resetGFValue();
				this.BaGF = null;
				if (M2DBase.Instance != null)
				{
					M2DBase.Instance.addLMtrEntry(M2DBase.LM_TYPE.PXL, "ui_album");
				}
			}
			else
			{
				this.active = true;
				EV.getVariableContainer().define("_scene", this.FocusEntry.key ?? "", true);
				EV.getVariableContainer().define("_album_categ", this.curtab.ToString(), true);
			}
			return this;
		}

		public override bool runIRD(float fcnt)
		{
			if (this.stt == UiAlbum.STATE.INITIALIZE)
			{
				if (this.Pc == null || !this.Pc.isLoadCompleted())
				{
					return true;
				}
				this.stt = UiAlbum.STATE.MAIN;
				this.createInstance();
			}
			if (this.stt == UiAlbum.STATE.MAIN)
			{
				if (IN.isCancelPD())
				{
					SND.Ui.play("cancel", false);
					this.deactivate(false);
				}
			}
			else if (this.stt == UiAlbum.STATE.CONFIRM && (IN.isCancelPD() || this.BxL.isFocused()))
			{
				this.BConConfirm.Get(1).ExecuteOnSubmitKey();
			}
			bool flag = false;
			if (this.t >= 0f)
			{
				if (this.t < 30f)
				{
					this.t += fcnt;
					flag = true;
				}
			}
			else if (this.t > -30f)
			{
				this.t -= fcnt;
				flag = true;
				if (this.t <= -30f)
				{
					M2DBase.Instance.FlagValotStabilize.Rem(this.stabilize_key);
				}
			}
			if (this.t_tab >= 0f && this.t_tab < 20f)
			{
				this.t_tab += fcnt;
				flag = true;
			}
			if (flag)
			{
				this.redrawRightMesh();
			}
			if (!base.runIRD(fcnt) && !this.active && this.t <= -30f)
			{
				IN.DestroyE(base.gameObject);
				return false;
			}
			return true;
		}

		private bool changeTab(int _categ)
		{
			if (this.stt != UiAlbum.STATE.MAIN)
			{
				return false;
			}
			_categ %= 4;
			UiAlbum.CATEG categ = (UiAlbum.CATEG)_categ;
			if (categ != this.curtab)
			{
				this.pretab = ((categ == UiAlbum.CATEG.MEMORY && this.curtab == UiAlbum.CATEG.OSOK) ? (-1) : ((int)((categ == UiAlbum.CATEG.OSOK && this.curtab == UiAlbum.CATEG.MEMORY) ? UiAlbum.CATEG._MAX : this.curtab)));
				this.curtab = categ;
				this.t_tab = 0f;
				this.initTab();
				return true;
			}
			return false;
		}

		private void FnGenerateThumbKeys(BtnContainerBasic BCon, List<string> Adest)
		{
			List<UiAlbum.AlbumEntry> list = this.AAentry[(int)this.curtab];
			if (list == null)
			{
				return;
			}
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				Adest.Add(list[i].key);
			}
		}

		private bool FnMakeListThumbBtn(BtnContainer<aBtn> BCon, aBtn B)
		{
			int entryIndex = this.getEntryIndex(this.AAentry[(int)this.curtab], B.title);
			if (entryIndex < 0)
			{
				return false;
			}
			UiAlbum.AlbumEntry albumEntry = this.AAentry[(int)this.curtab][entryIndex];
			(B.get_Skin() as ButtonSkinNelAlbumThumb).initAlbumThumb(albumEntry, (albumEntry.sensitive && global::XX.X.sensitive_level > 0) ? this.MtrThumbBlur : this.MtrThumb);
			B.SetLocked(!albumEntry.visible_unlock, true, false);
			return true;
		}

		private void initTab()
		{
			this.BConList.RemakeT<aBtnNel>(null, "album_thumb");
			ObjCarrierConBlockBtnContainer<aBtn> objCarrierConBlockBtnContainer = this.BConList.getBaseCarr() as ObjCarrierConBlockBtnContainer<aBtn>;
			Designer.reboundCarrForBtnMulti(objCarrierConBlockBtnContainer, 4, 0f, 0f, 1f);
			this.BxL.RowRemakeHeightRecalc(objCarrierConBlockBtnContainer, null);
			if (this.BConList.Length == 0)
			{
				this.TxComingSoon.gameObject.SetActive(true);
			}
			else
			{
				this.TxComingSoon.gameObject.SetActive(false);
				this.BConList.Get(this.Afirst_select[(int)this.curtab]).Select(false);
				this.BConList.setValue(-1, false);
			}
			this.BxL.rowRemakeCheck(false);
			this.BxL.Focus();
		}

		public bool fnHoverThumbBtn(aBtn B)
		{
			int index = this.BConList.getIndex(B);
			if (index >= 0)
			{
				this.Afirst_select[(int)this.curtab] = index;
			}
			return true;
		}

		bool IEventWaitListener.EvtWait(bool is_first)
		{
			return is_first || (this.stt != UiAlbum.STATE.OFFLINE && this.stt != UiAlbum.STATE.DEACTIVATING && this.stt != UiAlbum.STATE.PLAYING);
		}

		private int getEntryIndex(List<UiAlbum.AlbumEntry> A, string key)
		{
			for (int i = A.Count - 1; i >= 0; i--)
			{
				if (A[i].key == key)
				{
					return i;
				}
			}
			return -1;
		}

		public bool FnChangedListThumb(BtnContainerRadio<aBtn> BCon, int pre, int cur)
		{
			if (cur < 0)
			{
				return true;
			}
			if (this.stt != UiAlbum.STATE.MAIN)
			{
				return false;
			}
			aBtn aBtn = BCon.Get(cur);
			if (aBtn == null)
			{
				return false;
			}
			if (aBtn.isLocked())
			{
				SND.Ui.play("locked", false);
				return false;
			}
			this.stt = UiAlbum.STATE.CONFIRM;
			this.BxConfirm.activate();
			this.BxConfirm.Focus();
			this.BxL.hide();
			this.FocusEntry = this.AAentry[(int)this.curtab][cur];
			if (!this.FocusEntry.visible)
			{
				this.BConConfirm.Get(0).SetLocked(true, true, false);
				this.BConConfirm.Get(1).Select(false);
			}
			else
			{
				this.BConConfirm.Get(0).SetLocked(false, true, false);
				this.BConConfirm.Get(0).Select(false);
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.Set((aBtn.get_Skin() as ButtonSkinNelUi).text_content);
				using (STB stb2 = TX.PopBld(null, 0))
				{
					stb.Replace("\n", stb2, 0, -1);
					if (TX.valid(this.FocusEntry.ui_term))
					{
						stb2.Clear().Add(this.FocusEntry.ui_term);
						stb.Ret("\n").Add("<font alpha=\"0.4\" size=\"85%\">").AddTxA("Album_visible_term", false)
							.TxRpl(stb2)
							.Add("</font>");
					}
					if (this.curtab == UiAlbum.CATEG.FATAL)
					{
						stb.Ret("\n").Add("<font size=\"70%\" linespacing=\"0.85\">").AddTxA("Album_fatal_hint", false)
							.Add("</font>");
					}
				}
				this.FbConfirm.Txt(stb);
			}
			MeshDrawer meshDrawer = this.FbConfirm.getMeshDrawer();
			if (meshDrawer.hasMultipleTriangle())
			{
				meshDrawer.chooseSubMesh(1, false, false);
				meshDrawer.setMaterial((this.FocusEntry.sensitive && global::XX.X.sensitive_level > 0) ? this.MtrThumbBlur : this.MtrThumb, false);
				meshDrawer.chooseSubMesh(0, false, false);
				meshDrawer.connectRendererToTriMulti(this.FbConfirm.getMeshRenderer());
			}
			this.FbConfirm.redraw_flag = true;
			IN.clearPushDown(false);
			return true;
		}

		public bool FnDrawConfirmImg(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			if (this.stt != UiAlbum.STATE.CONFIRM)
			{
				return true;
			}
			if (!Md.hasMultipleTriangle())
			{
				Md.chooseSubMesh(1, false, false);
				Md.setMaterial((this.FocusEntry.sensitive && global::XX.X.sensitive_level > 0) ? this.MtrThumbBlur : this.MtrThumb, false);
				Md.connectRendererToTriMulti(FI.getMeshRenderer());
				Md.chooseSubMesh(0, false, false);
			}
			ButtonSkinNelAlbumThumb.drawAlbumThumbS(Md, null, this.FocusEntry, alpha, 34f, 2f);
			update_meshdrawer = true;
			return true;
		}

		public bool fnClickConfirm(aBtn B)
		{
			if (B.isLocked() || this.stt != UiAlbum.STATE.CONFIRM)
			{
				SND.Ui.play("locked", false);
				return false;
			}
			string title = B.title;
			if (title != null)
			{
				if (!(title == "&&Album_Play_Memory"))
				{
					if (title == "&&Cancel")
					{
						this.stt = UiAlbum.STATE.MAIN;
						IN.clearPushDown(false);
						this.BxL.bind();
						this.BxL.Focus();
						this.BxConfirm.deactivate();
						this.BConList.setValue(-1, true);
						this.BConList.Get(this.Afirst_select[(int)this.curtab]).Select(false);
					}
				}
				else
				{
					this.deactivate(true, false);
					this.stt = UiAlbum.STATE.PLAYING;
				}
			}
			return true;
		}

		private void redrawRightMesh()
		{
			float num = ((this.t >= 0f) ? global::XX.X.ZSIN2(this.t, 30f) : global::XX.X.ZLINE(30f + this.t, 30f));
			int num2 = (int)this.curtab;
			float num3 = (float)(-(float)num2) * 460f - 40f;
			int num4 = num2 - 1;
			int num5 = num2 + 1;
			float num6 = 1f;
			if (this.t_tab < 20f && (int)this.curtab != this.pretab)
			{
				num6 = global::XX.X.ZLINE(this.t_tab, 20f);
				float num7 = (1f - global::XX.X.ZSIN2(this.t_tab, 20f)) * (float)global::XX.X.MPF((int)this.curtab > this.pretab) * 460f;
				num3 += num7;
				if ((int)this.curtab < this.pretab)
				{
					num5++;
				}
				else
				{
					num4--;
				}
			}
			float mdrx = this.mdrx;
			for (int i = 0; i < 2; i++)
			{
				for (int j = num4; j <= num5; j++)
				{
					int num8 = (j + 4) % 4;
					if (num8 == num2 == (i == 1))
					{
						PxlFrame frame = this.SqRightThumb.getFrame(num8);
						float num9 = num;
						this.MdR.ColGrd.White();
						if ((this.categ_activated & (1U << num8)) == 0U)
						{
							this.MdR.ColGrd.Set(4278190080U);
							num9 *= 0.35f;
						}
						else if (j == (int)this.curtab)
						{
							this.MdR.ColGrd.multiply(global::XX.X.Scr(num6, 0.7f), true);
						}
						else if (j == this.pretab)
						{
							this.MdR.ColGrd.multiply(global::XX.X.Scr(1f - num6, 0.7f), true);
						}
						else
						{
							this.MdR.ColGrd.multiply(0.7f, true);
						}
						this.MdR.Col = this.MdR.ColGrd.mulA(num9).C;
						this.MdR.RotaPF(mdrx, -(num3 + (float)j * 460f), 1f, 1f, 0f, frame, false, false, false, uint.MaxValue, false, 0);
					}
				}
			}
			this.MdR.updateForMeshRenderer(false);
		}

		public override bool EvtRead(EvReader ER, StringHolder rER, int skipping = 0)
		{
			if (rER.cmd == "UIALBUM_RELOAD_GF")
			{
				this.resetGFValue();
				return true;
			}
			if (rER.cmd == "UIALBUM_RELOAD")
			{
				EV.initWaitFn(this, 0);
				IMessageContainer messageContainer = EV.getMessageContainer();
				messageContainer.quitEvent();
				messageContainer.hideMsg(true);
				messageContainer.initEvent(null);
				M2DBase.Instance.FlagValotStabilize.Add(this.stabilize_key);
				this.resetBgm(false);
				this.runner_assigned = true;
				this.activate();
				base.bind();
				this.RTabbar.bind(false, false);
				if (this.t < 0f)
				{
					this.t = global::XX.X.Mx(0f, 30f + this.t);
				}
				this.stt = UiAlbum.STATE.MAIN;
				this.BxL.Focus();
				this.BConList.setValue(-1, true);
				this.BConList.Get(this.Afirst_select[(int)this.curtab]).Select(false);
				return true;
			}
			return false;
		}

		public const string pxl_name = "ui_album";

		private List<UiAlbum.AlbumEntry>[] AAentry;

		private Material MtrThumb;

		private Material MtrThumbBlur;

		private string def_bgm_timing;

		private string def_bgm_cue;

		private UiAlbum.CATEG curtab;

		private int pretab = 1;

		private PxlCharacter Pc;

		private PxlSequence SqRightThumb;

		private UiAlbum.STATE stt;

		private float t;

		private float t_tab;

		private uint categ_activated;

		private int[] Afirst_select;

		private const int r_clmn = 4;

		private TextRenderer TxComingSoon;

		private UiBoxDesigner BxL;

		private UiBoxDesigner BxConfirm;

		private UiAlbum.AlbumEntry FocusEntry;

		private BtnContainerRadio<aBtn> BConList;

		private BtnContainer<aBtn> BConConfirm;

		private FillImageBlock FbConfirm;

		private UiBoxDesigner BxTab;

		private ColumnRow RTabbar;

		private ByteArray BaGF;

		public const float thumb_w = 100f;

		public const float thumb_h = 56f;

		private const float bxrw = 746f;

		private const float bxrh = 484f;

		private const float tabh = 28f;

		private const float FADE_T = 30f;

		private const float FADE_TAB_T = 20f;

		private const string data_csv = "Data/album";

		private string stabilize_key;

		private bool debug_show_all;

		private MeshDrawer MdR;

		private enum CATEG : byte
		{
			MEMORY,
			FATAL,
			TROOM,
			OSOK,
			_MAX
		}

		private enum STATE : byte
		{
			OFFLINE,
			INITIALIZE,
			MAIN,
			CONFIRM,
			PLAYING,
			DEACTIVATING
		}

		public struct AlbumEntry
		{
			public bool visible_unlock
			{
				get
				{
					return this.visible_unlock_ || this.visible;
				}
				set
				{
					this.visible_unlock_ = value;
				}
			}

			public void CopyFrom(UiAlbum.AlbumEntry Src)
			{
				this.visible_unlock_ = Src.visible_unlock_;
				this.visible = Src.visible;
				this.ui_term = Src.ui_term;
				this.sensitive = Src.sensitive;
				this.Trm = Src.Trm;
			}

			public string key;

			public string title_localized;

			public string ui_term;

			public bool visible;

			public bool sensitive;

			private bool visible_unlock_;

			public PxlFrame PF;

			public TRMManager.TRMItem Trm;
		}
	}
}
