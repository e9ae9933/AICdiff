using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public class SVD
	{
		public static string getDir()
		{
			return Application.persistentDataPath;
		}

		public static string getFileName(int index)
		{
			return "savedata_" + global::XX.X.spr0(index, 2, '0') + ".aicsave";
		}

		public static int thumb_w
		{
			get
			{
				return (int)((IN.w - 240f) * 0.3333333f);
			}
		}

		public static int thumb_h
		{
			get
			{
				return (int)((IN.h - 180f) * 0.3333333f);
			}
		}

		public static void prepareList(bool force = true)
		{
			if (!force && SVD.AFiles != null)
			{
				return;
			}
			SVD.exists_file_count = 0;
			SVD.AFiles = new List<SVD.sFile>();
			SVD.APreparingList = null;
			SVD.loaded_last_total_frame = 0;
			string[] array = Directory.GetFiles(SVD.getDir(), "*.aicsave", SearchOption.TopDirectoryOnly);
			int num = array.Length;
			if (num == 0)
			{
				if (!false)
				{
					return;
				}
				array = Directory.GetFiles(SVD.getDir(), "*.aicsave", SearchOption.TopDirectoryOnly);
				num = array.Length;
			}
			for (int i = 0; i < num; i++)
			{
				if (REG.match(array[i], SVD.RegSdFile))
				{
					int num2 = global::XX.X.NmI(REG.R1, 0, false, false);
					if (global::XX.X.BTWW(0f, (float)num2, 99f))
					{
						int num3 = global::XX.X.Mn(num2 + 7, 99);
						if (SVD.AFiles.Capacity <= num3)
						{
							SVD.AFiles.Capacity = num3 + 1;
						}
						while (SVD.AFiles.Count <= num2)
						{
							SVD.AFiles.Add(null);
						}
						SVD.AFiles[num2] = new SVD.sFile(num2, false);
						SVD.exists_file_count++;
					}
				}
			}
		}

		public static bool isLastFocusedRow(SVD.sFile Sf)
		{
			return SVD.last_focused == Sf.index;
		}

		public static bool initPreparingFileHeader(SVD.sFile Sf, bool force = false)
		{
			if (!force && IN.totalframe - SVD.loaded_last_total_frame <= 6)
			{
				return false;
			}
			if (SVD.ui_active && !force && SVD.last_focused != Sf.index)
			{
				SVD.sFile file = SVD.GetFile(SVD.last_focused, true);
				if (file != null && file.loadstate == SVD.sFile.STATE.NO_LOAD)
				{
					return false;
				}
			}
			SVD.loaded_last_total_frame = IN.totalframe;
			bool loadstate = Sf.loadstate != SVD.sFile.STATE.NO_LOAD;
			bool flag = COOK.readFileHeader(Sf, Path.Combine(SVD.getDir(), SVD.getFileName(Sf.index)));
			if (!loadstate)
			{
				Sf.loadstate = (flag ? SVD.sFile.STATE.LOADED : SVD.sFile.STATE.ERROR);
				if (flag && SVD.ui_active && Sf.index == SVD.last_focused)
				{
					SVD.Instance.fineDescString();
				}
			}
			return true;
		}

		public static SVD.sFile GetFile(int index = 0, bool no_make = true)
		{
			if (index >= SVD.AFiles.Count)
			{
				return null;
			}
			if (SVD.AFiles[index] == null && !no_make)
			{
				SVD.sFile sFile = (SVD.AFiles[index] = new SVD.sFile(index, true));
				SVD.exists_file_count++;
				return sFile;
			}
			return SVD.AFiles[index];
		}

		public static void releaseAllThumbs()
		{
			if (SVD.AFiles == null)
			{
				return;
			}
			int count = SVD.AFiles.Count;
			for (int i = 0; i < count; i++)
			{
				SVD.sFile sFile = SVD.AFiles[i];
				if (sFile != null)
				{
					sFile.releaseThumb();
				}
			}
		}

		public static ByteArray loadFileContent(SVD.sFile Sf)
		{
			return new ByteArray(NKT.readSpecificFileBinary(Path.Combine(SVD.getDir(), SVD.getFileName(Sf.index)), 0, 0, false), false, false);
		}

		public static SVD.sFile CopyFile(SVD.sFile Sf, int dest)
		{
			return SVD.createFile(dest, false).CopyFrom(Sf, dest);
		}

		public static SVD.sFile createFile(int dest, bool no_destruct = false)
		{
			int num = global::XX.X.Mn(dest + 7, 99);
			if (SVD.AFiles.Capacity <= num)
			{
				SVD.AFiles.Capacity = num + 1;
			}
			while (SVD.AFiles.Count <= dest)
			{
				SVD.AFiles.Add(null);
			}
			if (SVD.AFiles[dest] != null)
			{
				if (!no_destruct)
				{
					SVD.AFiles[dest].destruct();
				}
			}
			else
			{
				SVD.exists_file_count++;
			}
			return SVD.AFiles[dest] = new SVD.sFile(dest, true);
		}

		public static void replaceSaveFile(int index, SVD.sFile OvrFile)
		{
			SVD.sFile sFile = SVD.AFiles[index];
			if (sFile == OvrFile)
			{
				return;
			}
			if (sFile != null)
			{
				sFile.destruct();
			}
			SVD.AFiles[index] = OvrFile;
			if (OvrFile != null)
			{
				OvrFile.index = index;
			}
		}

		public static string saveBinary(SVD.sFile Sf, ByteArray Ba)
		{
			string text = null;
			try
			{
				string text2 = "temporary_" + SVD.getFileName(Sf.index);
				string text3 = Path.Combine(Application.persistentDataPath, SVD.getFileName(Sf.index));
				text = NKT.writeSdBinary(text2, Ba, true);
				if (TX.valid(text))
				{
					text = TX.getLine(text, 0);
					return text;
				}
				try
				{
					if (File.Exists(text3))
					{
						File.Delete(text3);
					}
				}
				catch
				{
				}
				File.Move(Path.Combine(Application.persistentDataPath, text2), text3);
				if (SVD.ui_active)
				{
					SVD.Instance.checkSavingBinary(Sf);
				}
				CFG.saveSdFile(null);
				MGV.last_saved = (byte)Sf.index;
				MGV.saveSdFile(null);
			}
			catch (Exception ex)
			{
				string line = TX.getLine(ex.Message, 0);
				global::XX.X.de("SAVE ERROR:" + ex.Message, null);
				text = ((text == null) ? line : (text + "\n" + line));
			}
			return text;
		}

		public static ByteArray changeOnlyMemo(SVD.sFile Sf)
		{
			if (Sf.thumb_position < 0)
			{
				global::XX.X.de("初期化されていません", null);
				return null;
			}
			string text = Path.Combine(SVD.getDir(), SVD.getFileName(Sf.index));
			string text2 = Path.Combine(SVD.getDir(), ".temp.aicsave");
			ByteArray byteArray = null;
			try
			{
				File.Copy(text, text2, true);
				ByteArray byteArray2 = new ByteArray(NKT.readSpecificFileBinary(text2, (int)Sf.thumb_position, 0, false), false, false);
				ByteArray byteArray3 = new ByteArray(0U);
				COOK.writeHeader(byteArray3, Sf);
				byteArray3.writeBytes(byteArray2, 0, -1);
				NKT.writeSdBinary(SVD.getFileName(Sf.index), byteArray3, false);
				byteArray = byteArray3;
			}
			catch (Exception ex)
			{
				global::XX.X.de(ex.ToString(), null);
			}
			try
			{
				File.Delete(text2);
			}
			catch
			{
			}
			return byteArray;
		}

		private static bool prepareCompanyMove()
		{
			string persistentDataPath = Application.persistentDataPath;
			bool flag = false;
			try
			{
				int i = 0;
				while (i < 3)
				{
					string text = ((i == 0) ? "jp.NanameHacha.AliceInCradle" : ((i == 1) ? "jp.nonamehacha.AliceInCradle" : "jp.nanamehacha.AliceInCradle"));
					string text2 = Path.Combine(Path.GetDirectoryName(persistentDataPath), text);
					if (Directory.Exists(text2))
					{
						goto IL_0060;
					}
					text2 = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(persistentDataPath)), text);
					if (Directory.Exists(text2))
					{
						goto IL_0060;
					}
					IL_0123:
					i++;
					continue;
					IL_0060:
					string text3 = Path.Combine(text2, "AliceInCradle");
					if (Directory.Exists(text3))
					{
						text2 = text3;
					}
					if (!(text2 == persistentDataPath))
					{
						try
						{
							string[] files = Directory.GetFiles(text2, "*", SearchOption.TopDirectoryOnly);
							int num = files.Length;
							for (int j = 0; j < num; j++)
							{
								try
								{
									string text4 = files[j];
									string fileName = Path.GetFileName(text4);
									if (!TX.isStart(fileName, ".", 0))
									{
										if (fileName == "config.cfg" || fileName == "whole.data" || REG.match(fileName, SVD.RegSdFile))
										{
											string text5 = Path.Combine(persistentDataPath, fileName);
											if (!File.Exists(text5))
											{
												File.Copy(text4, text5);
												flag = true;
											}
										}
									}
								}
								catch
								{
								}
							}
						}
						catch
						{
						}
						goto IL_0123;
					}
					goto IL_0123;
				}
			}
			catch
			{
			}
			return flag;
		}

		public float desc_h
		{
			get
			{
				return (float)SVD.thumb_h + 80f + 40f + 90f;
			}
		}

		public static bool ui_active
		{
			get
			{
				return SVD.Instance != null;
			}
		}

		public void descZReposit()
		{
			if (this.BxDesc != null)
			{
				IN.setZ(this.BxDesc.transform, this.z_desc_pre);
			}
		}

		public SVD destruct()
		{
			if (this.BxMain == null)
			{
				return null;
			}
			this.descZReposit();
			this.BxDesc = (this.BxMain = null);
			this.DsSubmittion = null;
			this.RowCon = null;
			this.DescThumb = null;
			if (SVD.Instance == this)
			{
				SVD.Instance = null;
			}
			return null;
		}

		private void activateMaterial()
		{
			if (this.activated_id != null)
			{
				return;
			}
			int num = ++SVD.activated_id_count;
			this.activated_id = num.ToString();
			this.Mti = MTI.LoadContainer("MTI_svd", this.activated_id);
			SND.loadSheets("saveload", "svd");
			this.MILogo = this.Mti.LoadImage("logo");
		}

		public static void unloadSound()
		{
			SND.unloadSheets("saveload", "svd");
		}

		public void deactivateDesigner()
		{
			if (this.Hd != null)
			{
				IN.DestroyOne(this.Hd.gameObject);
			}
			if (this.ThumbRow != null)
			{
				this.create_thumb_delay = -1;
				this.ThumbRow = null;
			}
			if (!this.desc_first_assign)
			{
				this.saveMemo();
				this.desc_first_assign = true;
			}
			if (this.activated_id != null)
			{
				this.Mti = MTI.ReleaseContainer("MTI_svd", this.activated_id);
				this.activated_id = null;
			}
			this.descZReposit();
			this.BxDesc.use_canvas = false;
			this.BxDesc.deactivate();
			this.Hd = null;
			this.ui_state = SVD.STATE.OFFLINE;
		}

		public SVD(UiBoxDesigner _Bx, UiBoxDesigner _BxDesc, int first_index, Designer _DsSubmittion = null, bool _only_load = false, bool _cannot_save = false, bool _ignore_svd_cfg = false)
		{
			if (SVD.AFiles == null)
			{
				SVD.prepareList(true);
			}
			if (SVD.Instance != null)
			{
				global::XX.X.de("SVD 多重起動", null);
			}
			SVD.Instance = this;
			this.only_load = _only_load;
			this.cannot_save = _cannot_save;
			this.ignore_svd_cfg = _ignore_svd_cfg;
			SVD.show_explore_timer = false;
			this.BxMain = _Bx;
			this.BxDesc = _BxDesc;
			this.DsSubmittion = _DsSubmittion;
			this.BxMain.alignx = ALIGN.CENTER;
			this.BaseTrs = this.BxMain.transform.parent;
			this.f0 = IN.totalframe;
			if (this.BxDesc != null)
			{
				this.z_desc_pre = this.BxDesc.transform.localPosition.z;
				IN.setZ(this.BxDesc.transform, -0.86f);
			}
			SVD.last_focused = global::XX.X.MMX(0, first_index, 99);
			this.pre_marked = COOK.loaded_index;
			this.ui_state = SVD.STATE.INITTED;
			_Bx.use_scroll = false;
			this.BxMain.margin_in_lr = 18f;
			this.BxMain.margin_in_tb = 20f;
			this.BxMain.item_margin_y_px = 4f;
			this.BxMain.item_margin_x_px = 0f;
			this.BxDesc.Clear();
			this.createDescription(this.BxDesc, false);
			this.BxMain.init();
			this.RowCon = this.createBoxDesignerContent();
			int num = SVD.AFiles.Capacity - 1;
			if (this.only_load)
			{
				int num2 = SVD.AFiles.Count - 1;
				aBtn aBtn = null;
				if (this.DsSubmittion != null)
				{
					using (BList<aBtn> blist = this.DsSubmittion.getRowManager().PopLastLineSelectable(false))
					{
						aBtn = blist[0];
					}
				}
				if (aBtn == null)
				{
					aBtn = this.FirstExistRow ?? this.GetBtn(0);
				}
				aBtn aBtn2 = this.LastExistRow ?? aBtn;
				for (int i = num2 + 1; i < num; i++)
				{
					aBtnSvd btn = this.GetBtn(i);
					btn.setNaviT(aBtn2, false, true);
					btn.setNaviB(aBtn, false, true);
				}
				num = num2;
			}
			this.BxDesc.deactivate();
		}

		private void fineSubmitionNavi()
		{
			aBtn aBtn = (this.only_load ? this.FirstExistRow : this.BtAs);
			aBtn aBtn2 = (this.only_load ? this.LastExistRow : this.RowCon.Get(this.RowCon.Length - 1));
			if (this.BtTop != null)
			{
				this.BtAs.setNaviB(this.BtTop, true, true);
			}
			if (this.DsSubmittion == null || aBtn == null || aBtn2 == null)
			{
				return;
			}
			using (BList<aBtn> blist = this.DsSubmittion.getRowManager().PopFirstLineSelectable(false))
			{
				int count = blist.Count;
				for (int i = 0; i < count; i++)
				{
					blist[i].setNaviT(aBtn2, false, true);
				}
			}
			using (BList<aBtn> blist2 = this.DsSubmittion.getRowManager().PopLastLineSelectable(false))
			{
				int count2 = blist2.Count;
				for (int j = 0; j < count2; j++)
				{
					blist2[j].setNaviB(aBtn, false, true);
				}
			}
		}

		public aBtnSvd GetBtn(int i)
		{
			if (i == 0)
			{
				return this.BtAs;
			}
			return this.RowCon.Get(i - 1) as aBtnSvd;
		}

		private BtnContainerRadio<aBtn> createBoxDesignerContent()
		{
			this.desc_first_assign = true;
			this.BtAs = this.BxMain.addButtonT<aBtnSvd>(new DsnDataButton
			{
				name = "svd_as",
				title = "0",
				w = this.BxMain.use_w - 2f - 16f,
				h = 88f
			});
			IN.setZ(this.BtAs.transform, -0.125f);
			this.BxMain.Hr(0.8f, 10f, 12f, 1f);
			this.FnMakeSvdList(null, this.BtAs);
			this.BxMain.Br();
			return this.BxMain.addRadioT<aBtnSvd>(new DsnDataRadio
			{
				name = "svd_list_c",
				def = -1,
				w = this.BxMain.use_w - 20f - 16f,
				skin = "normal",
				clms = 1,
				margin_h = 0,
				margin_w = 0,
				h = 88f,
				keys = global::XX.X.makeToStringed<int>(global::XX.X.makeCountUpArray(SVD.AFiles.Capacity - 1, 1, 1)),
				fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnClickScdRow1),
				fnMakingAfter = new BtnContainer<aBtn>.FnBtnMakingBindings(this.FnMakeSvdList),
				SCA = new ScrollAppend(239, this.BxMain.use_w, this.BxMain.use_h - 8f, 8f, 8f, 0)
			});
		}

		private bool FnMakeSvdList(BtnContainer<aBtn> BCon, aBtn _B)
		{
			int num = ((BCon == null) ? 0 : BCon.Length);
			aBtnSvd aBtnSvd = _B as aBtnSvd;
			SVD.sFile file = SVD.GetFile(num, true);
			aBtnSvd.setData(num, file, false);
			aBtnSvd.ContainerSVD = this;
			aBtnSvd.addHoverFn(new FnBtnBindings(this.fnHoverScdRow));
			aBtnSvd.addOutFn(new FnBtnBindings(this.fnOutScdRow));
			if (num == 0)
			{
				aBtnSvd.addClickFn(new FnBtnBindings(this.fnClickScdRow));
			}
			if (file != null)
			{
				if (this.FirstExistRow == null || this.FirstExistRow.getSvdIndex() >= file.index)
				{
					this.FirstExistRow = aBtnSvd;
				}
				if (this.LastExistRow == null || this.LastExistRow.getSvdIndex() <= file.index)
				{
					this.LastExistRow = aBtnSvd;
				}
			}
			if (this.BtTop == null && num >= 1 && (!this.only_load || file != null))
			{
				this.BtTop = aBtnSvd;
			}
			if (this.pre_marked == num)
			{
				aBtnSvd.fineMarker();
			}
			return true;
		}

		private bool fnClickScdRow1(BtnContainer<aBtn> BCon, int pre, int aft)
		{
			return this.fnClickScdRow(BCon.Get(aft));
		}

		private bool fnClickScdRow(aBtn B)
		{
			if (this.BxDesc == null || B.isLocked() || this.ui_state > SVD.STATE.ACTIVE)
			{
				return true;
			}
			aBtnSvd aBtnSvd = B as aBtnSvd;
			return !(aBtnSvd == null) && aBtnSvd.getSvdIndex() == SVD.last_focused && this.selectSvdRow(aBtnSvd);
		}

		public aBtn simulateNaviTranslationInner(ref int a, aBtn Dep, int index)
		{
			if (this.FirstExistRow == null || this.LastExistRow == null || !this.only_load)
			{
				return Dep;
			}
			if (a == 1 && index <= this.FirstExistRow.getSvdIndex())
			{
				if (this.DsSubmittion != null)
				{
					using (BList<aBtn> blist = this.DsSubmittion.getRowManager().PopFirstLineSelectable(false))
					{
						if (blist != null && blist.Count > 0)
						{
							return blist[0];
						}
					}
				}
				return this.LastExistRow;
			}
			if (a == 3 && index >= this.LastExistRow.getSvdIndex())
			{
				if (this.DsSubmittion != null)
				{
					using (BList<aBtn> blist2 = this.DsSubmittion.getRowManager().PopFirstLineSelectable(false))
					{
						if (blist2 != null && blist2.Count > 0)
						{
							return blist2[0];
						}
					}
				}
				return this.FirstExistRow;
			}
			return Dep;
		}

		private bool fnHoverScdRow(aBtn B)
		{
			if (this.BxDesc == null || B.isLocked() || this.ui_state > SVD.STATE.ACTIVE)
			{
				return true;
			}
			aBtnSvd aBtnSvd = B as aBtnSvd;
			if (!this.desc_first_assign)
			{
				this.saveMemo();
			}
			SVD.last_focused = aBtnSvd.getSvdIndex();
			this.desc_first_assign = false;
			this.fineDescString();
			this.DescThumb.redraw_flag = true;
			Vector2 vector = new Vector2(-this.BxMain.swidth / 2f - this.BxDesc.swidth / 2f - 18f + this.BxMain.getBox().get_deperture_x(), 20f + this.BxMain.getBox().get_deperture_y());
			if (this.BxDesc.isActive())
			{
				this.BxDesc.position(vector.x, vector.y, -1000f, -1000f, false);
			}
			else
			{
				this.BxDesc.activate();
				this.BxDesc.positionD(vector.x, vector.y, 2, 40f);
			}
			this.desc_hide = false;
			if (aBtnSvd.getSvdData() != null)
			{
				this.create_thumb_delay = 20;
				this.ThumbRow = aBtnSvd;
			}
			else
			{
				this.create_thumb_delay = 0;
				this.ThumbRow = null;
			}
			return true;
		}

		private void fineDescString()
		{
			if (this.BxDesc == null)
			{
				return;
			}
			SVD.sFile file = SVD.GetFile(SVD.last_focused, true);
			string text = "";
			if (file != null)
			{
				if (file.header_prepared)
				{
					text = file.getDescStringForUi();
				}
			}
			else
			{
				text = ((SVD.last_focused == 0) ? TX.Get("SaveDataDesc_Empty_auto", "") : TX.GetA("SaveDataDesc_Empty", global::XX.X.spr0(SVD.last_focused, 2, '0')));
			}
			IVariableObject variableObject = this.BxDesc.Get("descp", false);
			if (variableObject != null)
			{
				variableObject.setValue(text);
			}
			LabeledInputField labeledInputField = this.BxDesc.Get("descmemo", false) as LabeledInputField;
			if (labeledInputField != null)
			{
				if (file != null)
				{
					labeledInputField.setValue(file.memo);
					labeledInputField.SetLocked(false, true, false);
					return;
				}
				labeledInputField.setValue("");
				labeledInputField.SetLocked(true, true, false);
			}
		}

		private bool fnOutScdRow(aBtn B)
		{
			if (this.BxDesc == null || B.isLocked())
			{
				return true;
			}
			aBtnSvd aBtnSvd = B as aBtnSvd;
			if (SVD.last_focused == aBtnSvd.getSvdIndex())
			{
				this.saveMemo();
			}
			return true;
		}

		private void saveMemo()
		{
			SVD.sFile file = SVD.GetFile(SVD.last_focused, true);
			IVariableObject variableObject = this.BxDesc.Get("descmemo", false);
			if (file == null || variableObject == null)
			{
				return;
			}
			bool header_prepared = file.header_prepared;
			string text = variableObject.getValueString();
			if (text.Length > 2047)
			{
				text = TX.slice(text, 0, 2047);
			}
			if (file.memo != text)
			{
				file.memo = text;
				SVD.changeOnlyMemo(file);
			}
		}

		public bool runBoxDesignerEdit(bool ignore_key = false)
		{
			if (this.ui_state == SVD.STATE.LOAD_SUCCESS)
			{
				return false;
			}
			if (this.activated_id == null)
			{
				this.activateMaterial();
			}
			if (this.create_thumb_delay >= 0 && this.ThumbRow != null)
			{
				SVD.sFile svdData = this.ThumbRow.getSvdData();
				if (svdData.Thumbnail == null && !svdData.thumb_error && svdData.header_prepared)
				{
					int num = this.create_thumb_delay - 1;
					this.create_thumb_delay = num;
					if (num < 0)
					{
						COOK.readBinaryThumbnailFromLocal(svdData);
					}
				}
			}
			if (this.ui_state == SVD.STATE.SELECTING_CANCEL || this.ui_state == SVD.STATE.SELECTING_CANCEL_NO_WRITE_MEMO)
			{
				if (this.ui_state == SVD.STATE.SELECTING_CANCEL)
				{
					this.saveMemo();
				}
				this.createDescription(this.BxDesc, true);
				this.ui_state = SVD.STATE.INITTED;
				this.desc_hide = false;
			}
			if (this.ui_state == SVD.STATE.SELECTING_TO_LOAD_ASK_AGAIN)
			{
				this.loadAskAgain();
			}
			if ((this.ui_state == SVD.STATE.LOAD_ASK_AGAIN || this.ui_state == SVD.STATE.SELECTING) && IN.isCancelPD())
			{
				this.ui_state = SVD.STATE.SELECTING_CANCEL;
				SND.Ui.play("cancel", false);
			}
			if (this.ui_state == SVD.STATE.INITTED)
			{
				this.ui_state = SVD.STATE.ACTIVE;
				if (this.Hd != null)
				{
					this.Hd.deactivate(false);
				}
				if (this.BxMain != null)
				{
					this.BxMain.activate();
					this.BxMain.Focus();
				}
				if (this.DsSubmittion != null)
				{
					this.DsSubmittion.bind();
				}
				this.fineSubmitionNavi();
				aBtnSvd btn = this.GetBtn(SVD.last_focused);
				if (btn != null)
				{
					if (!btn.isLocked())
					{
						btn.Select(true);
					}
					if (SVD.last_focused > 0 && this.RowCon.BelongScroll != null)
					{
						this.RowCon.BelongScroll.reveal(btn, false, REVEALTYPE.ALWAYS);
					}
				}
				this.BtAs.SetChecked(false, true);
				this.RowCon.setValue(-1, false);
			}
			else if (this.ui_state == SVD.STATE.ACTIVE)
			{
				if (this.desc_hide && this.BxDesc != null)
				{
					this.BxDesc.deactivate();
					this.desc_hide = false;
				}
				if (!ignore_key)
				{
					if (IN.isCancel())
					{
						return false;
					}
					if (IN.isMapPD(1))
					{
						SVD.show_explore_timer = !SVD.show_explore_timer;
						this.BtAs.RowSkin.fineStr();
						BtnContainerRunner btnContainerRunner = this.BxMain.Get("svd_list_c", false) as BtnContainerRunner;
						if (btnContainerRunner != null)
						{
							for (int i = btnContainerRunner.BCon.Length - 1; i >= 0; i--)
							{
								ButtonSkinSvdRow buttonSkinSvdRow = btnContainerRunner.BCon.Get(i).get_Skin() as ButtonSkinSvdRow;
								if (buttonSkinSvdRow != null)
								{
									buttonSkinSvdRow.fineStr();
								}
							}
						}
					}
				}
			}
			return true;
		}

		public void resume(bool recreate_bxdesc = false)
		{
			this.ui_state = SVD.STATE.INITTED;
			this.BxMain.Focus();
			this.BxMain.bind();
			if (this.BxDesc != null)
			{
				IN.setZ(this.BxDesc.transform, -0.86f);
				if (recreate_bxdesc)
				{
					this.createDescription(this.BxDesc, false);
				}
			}
		}

		private FillImageBlock createDescription(UiBoxDesigner Bx, bool wh_animate = false)
		{
			Bx.Clear();
			if (wh_animate)
			{
				this.BxDesc.WHanim(420f, this.desc_h, true, true);
			}
			else
			{
				this.BxDesc.WH(420f, this.desc_h);
			}
			Bx.use_canvas = true;
			Bx.margin_in_tb = 20f;
			Bx.margin_in_lr = 20f;
			Bx.item_margin_y_px = 15f;
			Bx.item_margin_x_px = 0f;
			Bx.init();
			Bx.alignx = ALIGN.CENTER;
			float num = 420f - Bx.margin_in_lr * 2f;
			float num2 = global::XX.X.Mx((float)SVD.thumb_w, num * 0.35f);
			FillImageBlock fillImageBlock = Bx.addImg(new DsnDataImg
			{
				FnDraw = new MeshDrawer.FnGeneralDraw(this.FnDrawThumbArea),
				text = " ",
				swidth = num,
				sheight = (float)SVD.thumb_h
			});
			Bx.Br();
			Designer designer = Bx.addTab("thumb_r", num, 80f, num - num2, 80f, false);
			designer.Smallest();
			designer.margin_in_lr = 30f;
			designer.margin_in_tb = 8f;
			designer.alignx = ALIGN.LEFT;
			designer.init();
			Bx.addP(new DsnDataP("", false)
			{
				name = "descp",
				text = " ",
				size = 11.7f,
				alignx = ALIGN.LEFT,
				aligny = ALIGNY.TOP,
				Col = MTRX.ColTrnsp,
				TxCol = C32.d2c(4283780170U),
				swidth = designer.use_w,
				sheight = 80f - designer.margin_in_tb * 2f,
				lineSpacing = 1.1f,
				text_margin_y = 0f,
				text_auto_condense = true,
				html = true
			}, false);
			Bx.Br();
			Bx.addInput(new DsnDataInput
			{
				name = "descmemo",
				bounds_w = designer.use_w - 20f,
				multi_line = 16,
				h = 64f,
				label = TX.Get("SVD_label_memo", ""),
				return_blur = false
			}).SetLocked(true, true, false);
			Bx.endTab(true);
			this.DescThumb = fillImageBlock;
			this.desc_first_assign = true;
			return fillImageBlock;
		}

		private bool FnDrawThumbArea(MeshDrawer Md, float alpha)
		{
			if (this.DescThumb == null)
			{
				return true;
			}
			if (this.ui_state == SVD.STATE.INITTED)
			{
				return false;
			}
			SVD.sFile file = SVD.GetFile(SVD.last_focused, true);
			if (file == null)
			{
				this.DescThumb.replaceMaterialManual(MTRX.getMtr(this.DescThumb.stencil_ref));
				Md.Col = C32.MulA(4283780170U, alpha);
				Md.RectDashed(0f, 0f, (float)SVD.thumb_w, (float)SVD.thumb_h, 0.5f, 20, 2f, false, false);
				Md.updateForMeshRenderer(false);
				return true;
			}
			if (file.thumb_error || file.Thumbnail == null)
			{
				if (this.MILogo != null && this.MILogo.Tx != null)
				{
					this.DescThumb.MI = this.MILogo;
					Md.initForImg(this.MILogo.Tx, 0.5f * (float)this.MILogo.Tx.width - (float)SVD.thumb_w * 0.5f, 0.5f * (float)this.MILogo.Tx.height - (float)SVD.thumb_h * 0.5f, (float)SVD.thumb_w, (float)SVD.thumb_h);
					Md.Col = C32.MulA(MTRX.ColWhite, alpha);
					Md.Rect(0f, 0f, (float)SVD.thumb_w, (float)SVD.thumb_h, false);
					Md.updateForMeshRenderer(false);
				}
				else
				{
					this.DescThumb.MI = MTRX.MIicon;
					Md.initForImg(MTRX.IconWhite, 0);
					Md.Col = C32.MulA(4282004532U, alpha);
					Md.Rect(0f, 0f, (float)SVD.thumb_w, (float)SVD.thumb_h, false);
					Md.updateForMeshRenderer(false);
				}
				return true;
			}
			if (this.MILogo != null && this.MILogo.Tx != null)
			{
				this.DescThumb.MI = this.MILogo;
				this.DescThumb.replaceMainTextureManual(file.Thumbnail);
				Md.Col = C32.MulA(MTRX.ColWhite, alpha);
				Md.initForImg(file.Thumbnail);
				Md.DrawCen(0f, 0f, null);
				Md.updateForMeshRenderer(false);
			}
			return true;
		}

		public static void descRedraw()
		{
			if (SVD.Instance != null && SVD.Instance.DescThumb != null && SVD.Instance.DescThumb.isActiveAndEnabled)
			{
				SVD.Instance.DescThumb.redraw_flag = true;
			}
		}

		private bool selectSvdRow(aBtnSvd Row)
		{
			if (this.BxDesc == null || !this.BxDesc.isActive())
			{
				return false;
			}
			IN.clearPushDown(false);
			if (Row.getSvdData() == null)
			{
				if (this.only_load)
				{
					return false;
				}
				if (!this.cannot_save)
				{
					this.executeSave(Row.getSvdIndex());
					this.fnHoverScdRow(Row);
					return false;
				}
			}
			UiBoxDesigner.BlurFrom(this.BxMain);
			if (this.Hd == null)
			{
				this.Hd = new GameObject("HideScreen").AddComponent<HideScreen>();
				this.Hd.gameObject.layer = IN.LAY(IN.gui_layer_name);
				this.Hd.Col = C32.d2c(1996488704U);
				IN.setZ(this.Hd.transform, this.BxMain.transform.position.z - 0.24f);
			}
			this.Hd.activate();
			this.ui_state = SVD.STATE.SELECTING;
			if (this.DsSubmittion != null)
			{
				this.DsSubmittion.hide();
			}
			this.BxMain.hide();
			Vector3 vector = this.BaseTrs.InverseTransformPoint(new Vector3(0f, 1.171875f, 0f));
			vector.x *= 64f;
			vector.y *= 64f;
			this.BxDesc.position(vector.x, vector.y, -1000f, -1000f, false);
			this.BxDesc.margin_in_tb = 50f;
			string text = "";
			if (!this.only_load && this.cannot_save)
			{
				text = TX.Get("SVD_need_bench", "");
			}
			else if (!this.only_load && Row.getSvdIndex() == 0)
			{
				text = "<font color=\"#E70B0B\">" + TX.Get("SVD_Alert_manual_save_on_auto_save_slot", "") + "</font>";
			}
			if (TX.valid(text))
			{
				this.BxDesc.WHanim(756f, this.desc_h + 120f + 48f, true, true);
				this.BxDesc.Br().addP(new DsnDataP("", false)
				{
					name = "need_bench",
					text = text,
					size = 15f,
					alignx = ALIGN.CENTER,
					aligny = ALIGNY.TOP,
					Col = MTRX.ColTrnsp,
					TxCol = C32.d2c(4283780170U),
					swidth = this.BxDesc.use_w,
					sheight = 48f,
					text_margin_y = 0f,
					text_auto_condense = true,
					html = true
				}, false);
			}
			else
			{
				this.BxDesc.WHanim(756f, this.desc_h + 120f, true, true);
			}
			if (this.only_load)
			{
				this.createIgnoringCheck();
			}
			Designer bxDesc = this.BxDesc;
			DsnDataButtonMulti dsnDataButtonMulti = new DsnDataButtonMulti();
			dsnDataButtonMulti.name = "cmds";
			DsnDataButtonMulti dsnDataButtonMulti2 = dsnDataButtonMulti;
			string[] array2;
			if (!this.only_load)
			{
				string[] array = new string[3];
				array[0] = "&&SVD_cmd_save";
				array[1] = "&&Cancel";
				array2 = array;
				array[2] = "&&SVD_cmd_load";
			}
			else
			{
				string[] array3 = new string[2];
				array3[0] = "&&SVD_cmd_load";
				array2 = array3;
				array3[1] = "&&Cancel";
			}
			dsnDataButtonMulti2.titles = array2;
			dsnDataButtonMulti.w = 180f;
			dsnDataButtonMulti.margin_w = 20f;
			dsnDataButtonMulti.h = 30f;
			dsnDataButtonMulti.clms = (this.only_load ? 2 : 3);
			dsnDataButtonMulti.fnClick = new FnBtnBindings(this.fnClickSelecting);
			BtnContainer<aBtn> btnContainer = bxDesc.addButtonMultiT<aBtnNel>(dsnDataButtonMulti);
			if (!this.only_load && this.cannot_save)
			{
				btnContainer.Get(0).SetLocked(true, true, false);
			}
			btnContainer.Get(this.only_load ? 0 : 1).Select(false);
			if (Row.getSvdData() == null)
			{
				btnContainer.Get("&&SVD_cmd_load").SetLocked(true, true, false);
			}
			return true;
		}

		public aBtn SelectLastFocusedRow(bool return_empty)
		{
			aBtnSvd aBtnSvd = this.GetBtn(SVD.last_focused);
			if (aBtnSvd == null && return_empty)
			{
				aBtnSvd = this.GetBtn(0);
			}
			if (aBtnSvd != null)
			{
				aBtnSvd.Select(false);
			}
			return aBtnSvd;
		}

		public void createIgnoringCheck()
		{
			this.BxDesc.Br().addButtonT<aBtnNel>(new DsnDataButton
			{
				name = "ignore_svd_cfg",
				title = "&&SVD_ignore_svd_cfg",
				skin = "checkbox",
				w = 350f,
				h = 16f,
				def = (this.ignore_svd_cfg || global::XX.X.DEBUGNOCFG),
				fnClick = new FnBtnBindings(this.fnClickIgnoreCfg)
			});
			this.BxDesc.Br();
		}

		private void loadAskAgain()
		{
			if (this.BxDesc == null || !this.BxDesc.isActive())
			{
				return;
			}
			IN.clearPushDown(false);
			this.ui_state = SVD.STATE.LOAD_ASK_AGAIN;
			Vector3 vector = (vector = this.BaseTrs.InverseTransformPoint(new Vector3(0f, 0.078125f, 0f)));
			vector.x *= 64f;
			vector.y *= 64f;
			this.BxDesc.item_margin_y_px = 18f;
			this.BxDesc.Clear();
			this.BxDesc.position(vector.x, vector.y, -1000f, -1000f, false);
			this.BxDesc.margin_in_tb = 40f;
			this.BxDesc.WHanim(756f, this.desc_h - 80f, true, true);
			this.BxDesc.init();
			this.BxDesc.alignx = ALIGN.CENTER;
			this.BxDesc.addP(new DsnDataP("", false)
			{
				text = TX.GetA("SVD_Load_Ask_Again", SVD.last_focused.ToString() ?? ""),
				alignx = ALIGN.CENTER,
				aligny = ALIGNY.MIDDLE,
				Col = MTRX.ColTrnsp,
				TxCol = C32.d2c(4283780170U),
				size = 18f,
				swidth = this.BxDesc.use_w,
				sheight = 40f,
				lineSpacing = 1.1f,
				text_auto_condense = true,
				html = true
			}, false);
			if (!this.only_load)
			{
				this.createIgnoringCheck();
			}
			this.BxDesc.addButtonMultiT<aBtnNel>(new DsnDataButtonMulti
			{
				name = "cmds",
				titles = new string[] { "&&SVD_cmd_load_do", "&&Cancel" },
				w = 240f,
				h = 32f,
				margin_w = 20f,
				clms = 2,
				fnClick = new FnBtnBindings(this.fnClickSelecting)
			}).Get(0).Select(false);
		}

		private bool fnClickIgnoreCfg(aBtn B)
		{
			B.SetChecked(!B.isChecked(), true);
			if (B.title == "&&SVD_ignore_svd_cfg")
			{
				this.ignore_svd_cfg = B.isChecked();
			}
			return true;
		}

		private bool executeSave(int index)
		{
			if (this.only_load || this.cannot_save)
			{
				return false;
			}
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			if (nelM2DBase == null)
			{
				return false;
			}
			SVD.sFile file = SVD.GetFile(index, true);
			SVD.sFile sFile = SVD.createFile(index, true);
			bool flag = true;
			COOK.createNewSave(sFile, nelM2DBase, flag);
			aBtnSvd btn = this.GetBtn(index);
			bool flag2 = true;
			if (TX.noe(COOK.save_failure_announce))
			{
				if (file != null)
				{
					file.destruct();
				}
				btn.setData(index, sFile, true);
				global::XX.X.dl("saved", null, false, false);
			}
			else
			{
				SVD.replaceSaveFile(index, file);
				COOK.replaceCurFileTo(file);
				global::XX.X.dl("save Failure!!", null, false, false);
				flag2 = false;
			}
			btn.SetLocked(false, true, false);
			if (sFile.index >= 1 && (this.BtTop == null || this.BtTop.getSvdIndex() >= sFile.index))
			{
				this.BtTop = btn;
			}
			if (this.FirstExistRow == null || this.FirstExistRow.getSvdIndex() >= sFile.index)
			{
				this.FirstExistRow = btn;
			}
			if (this.LastExistRow == null || this.LastExistRow.getSvdIndex() <= sFile.index)
			{
				this.LastExistRow = btn;
			}
			this.fineSubmitionNavi();
			return flag2;
		}

		private bool fnClickSelecting(aBtn B)
		{
			if (this.ui_state == SVD.STATE.SELECTING || this.ui_state == SVD.STATE.LOAD_ASK_AGAIN)
			{
				string title = B.title;
				if (title != null)
				{
					if (!(title == "&&Cancel"))
					{
						if (title == "&&SVD_cmd_save")
						{
							bool flag = this.executeSave(SVD.last_focused);
							this.ui_state = (flag ? SVD.STATE.SELECTING_CANCEL_NO_WRITE_MEMO : SVD.STATE.SELECTING_CANCEL);
							return flag;
						}
						if (title == "&&SVD_cmd_load" || title == "&&SVD_cmd_load_do")
						{
							if (SVD.GetFile(SVD.last_focused, true) == null)
							{
								return true;
							}
							this.saveMemo();
							if (!this.only_load && this.ui_state != SVD.STATE.LOAD_ASK_AGAIN)
							{
								this.ui_state = SVD.STATE.SELECTING_TO_LOAD_ASK_AGAIN;
							}
							else
							{
								this.ui_state = SVD.STATE.LOAD_SUCCESS;
							}
						}
					}
					else
					{
						this.ui_state = SVD.STATE.SELECTING_CANCEL;
					}
				}
			}
			return true;
		}

		private void checkSavingBinary(SVD.sFile Sf)
		{
			aBtnSvd btn = this.GetBtn(Sf.index);
			if (btn != null)
			{
				btn.setData(Sf.index, Sf, false);
			}
			if (COOK.loaded_index != this.pre_marked)
			{
				aBtnSvd aBtnSvd = this.GetBtn(this.pre_marked);
				if (aBtnSvd != null)
				{
					aBtnSvd.fineMarker();
				}
				aBtnSvd = this.GetBtn(COOK.loaded_index);
				if (aBtnSvd != null)
				{
					aBtnSvd.fineMarker();
				}
				this.pre_marked = COOK.loaded_index;
			}
		}

		public int isLoadSuccess()
		{
			if (this.ui_state != SVD.STATE.LOAD_SUCCESS)
			{
				return -1;
			}
			return SVD.last_focused;
		}

		private static List<SVD.sFile> AFiles;

		public const int AUTOSAVE_ID = 0;

		public const string fn_header = "savedata_";

		public const string fn_ext = ".aicsave";

		private static Regex RegSdFile = new Regex("savedata_(\\d\\d)\\.aicsave$");

		public const int file_max = 99;

		public const int list_margin_max = 7;

		public static int exists_file_count;

		private static List<SVD.sFile> APreparingList;

		private static int loaded_last_total_frame = 0;

		public const float thumb_scale = 0.3333333f;

		private UiBoxDesigner BxMain;

		private UiBoxDesigner BxDesc;

		private bool only_load;

		private Designer DsSubmittion;

		public const float desc_w = 420f;

		private const float thumb_caption_h = 80f;

		public int f0;

		public const float desc_margin_tb = 20f;

		private FillImageBlock DescThumb;

		private Transform BaseTrs;

		public SVD.STATE ui_state = SVD.STATE.INITTED;

		private aBtnSvd BtAs;

		private BtnContainerRadio<aBtn> RowCon;

		public static int last_focused = 0;

		private bool cannot_save;

		private int t_main_hide;

		private float z_desc_pre;

		private HideScreen Hd;

		private int pre_marked;

		private static SVD Instance;

		private aBtnSvd BtTop;

		private aBtnSvd FirstExistRow;

		private aBtnSvd LastExistRow;

		private bool desc_hide;

		public bool ignore_svd_cfg;

		private bool desc_first_assign;

		public static bool show_explore_timer;

		public aBtnSvd ThumbRow;

		private int create_thumb_delay = -1;

		private static int activated_id_count = 0;

		private string activated_id;

		private const float Z_DESC = -0.86f;

		private MTI Mti;

		private MImage MILogo;

		public sealed class sFile
		{
			public sFile(int _index, bool created = false)
			{
				this.index = _index;
				this.loadstate = (created ? SVD.sFile.STATE.CREATED : SVD.sFile.STATE.NO_LOAD);
			}

			public SVD.sFile CopyFrom(SVD.sFile Src, int _index)
			{
				this.index = _index;
				this.first_version = Src.first_version;
				this.loadstate = SVD.sFile.STATE.CREATED;
				this.playtime = Src.playtime;
				this.memo = Src.memo;
				this.safe_area_memory = Src.safe_area_memory;
				this.modified = Src.modified;
				this.playstart = Src.playstart;
				this.thumb_position = Src.thumb_position;
				this.explore_timer = Src.explore_timer;
				return this;
			}

			public SVD.sFile saveInit()
			{
				this.releaseThumb();
				this.version = 33;
				this.loadstate = SVD.sFile.STATE.CREATED;
				int num = (int)((this.modified = DateTime.Now) - this.playstart).TotalSeconds;
				if (num > 0)
				{
					this.playtime += (uint)num;
				}
				this.revert_pos = false;
				this.playInit();
				NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
				COOK.addTimer(nelM2DBase, false);
				nelM2DBase.WDR.walkAround(false);
				Map2d curMap = nelM2DBase.curMap;
				PRNoel prNoel = nelM2DBase.getPrNoel();
				prNoel.saveInit();
				if (nelM2DBase.IMNG != null)
				{
					nelM2DBase.IMNG.digestDiscardStack(nelM2DBase.curMap.Pr);
				}
				this.hp_noel = (ushort)prNoel.get_hp();
				this.maxhp_noel = (ushort)prNoel.get_maxhp();
				this.mp_noel = (ushort)prNoel.get_mp();
				this.maxmp_noel = (ushort)prNoel.get_maxmp();
				string pvv = GF.getPVV();
				this.phase = (ushort)(TX.valid(pvv) ? global::XX.X.NmI(pvv, 0, false, false) : 0);
				WholeMapItem wholeFor = nelM2DBase.WM.GetWholeFor(curMap, false);
				this.whole_map_key = ((wholeFor != null) ? wholeFor.text_key : "???");
				this.explore_timer = (uint)COOK.calced_timer;
				return this;
			}

			public void revertPos()
			{
				if (!this.revert_pos)
				{
					return;
				}
				NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
				PRNoel prNoel = nelM2DBase.getPrNoel();
				if (this.last_saved_x > 0f && nelM2DBase.curMap != null && nelM2DBase.curMap.key == this.last_map_key)
				{
					Vector3 vector = prNoel.setToLoadGame(this.last_saved_x + 0.5f, this.last_saved_y - prNoel.sizey);
					if (vector.z > 0f)
					{
						this.last_saved_x = vector.x;
						this.last_saved_y = vector.y;
					}
				}
				this.revert_pos = false;
			}

			public void playInit()
			{
				this.first_load = false;
				this.playstart = DateTime.Now;
				this.assignRevertPosition(M2DBase.Instance as NelM2DBase);
				if (this.first_version == 0)
				{
					this.first_version = this.version;
				}
			}

			public void assignRevertPosition(NelM2DBase M2D)
			{
				if (M2D != null && M2D.curMap != null)
				{
					this.last_map_key = M2D.curMap.key;
					PRNoel prNoel = M2D.getPrNoel();
					this.last_saved_x = prNoel.x;
					this.last_saved_y = M2D.curMap.getFootableY((float)((int)prNoel.x), (int)prNoel.y, 12, true, -1f, false, true, true, prNoel.sizex);
				}
			}

			public void destruct()
			{
				this.releaseThumb();
			}

			public void setThumb(Texture2D Tx)
			{
				if (Tx == this.Thumbnail)
				{
					return;
				}
				this.releaseThumb();
				this.Thumbnail = Tx;
				SVD.descRedraw();
			}

			public void releaseThumb()
			{
				this.thumb_error = false;
				if (this.Thumbnail != null)
				{
					Object.Destroy(this.Thumbnail);
				}
			}

			public Texture2D createThumbnail(NelM2DBase M2D)
			{
				this.releaseThumb();
				float num = 0.3333333f;
				RenderTexture finalizedTexture = M2D.Cam.getFinalizedTexture();
				int thumb_w = SVD.thumb_w;
				this.Thumbnail = BLIT.getSnapShot(finalizedTexture, (float)0 / IN.w * 0.86f, 0f, (float)SVD.thumb_w, (float)SVD.thumb_h, num, true);
				return this.Thumbnail;
			}

			public bool header_prepared
			{
				get
				{
					return this.loadstate == SVD.sFile.STATE.LOADED || this.loadstate == SVD.sFile.STATE.CREATED;
				}
			}

			public string getDescStringForUi()
			{
				if (this.index != 0)
				{
					return TX.GetA("SaveDataDesc", global::XX.X.spr0(this.index, 2, '0'), this.strPlayTime);
				}
				return TX.GetA("SaveDataDesc_auto", this.strPlayTime);
			}

			public string strPlayTime
			{
				get
				{
					uint num = this.playtime / 3600U;
					uint num2 = this.playtime / 60U % 60U;
					uint num3 = this.playtime % 60U;
					return ((num > 0U) ? (num.ToString() + ":") : "") + global::XX.X.spr0((int)num2, 2, '0') + ":" + global::XX.X.spr0((int)num3, 2, '0');
				}
			}

			public int index;

			public SVD.sFile.STATE loadstate;

			public bool has_alice;

			public bool thumb_error;

			public byte version = 33;

			public byte first_version;

			public uint playtime;

			public ushort phase;

			public ushort hp_noel = 150;

			public ushort maxhp_noel = 150;

			public ushort mp_noel = 200;

			public ushort maxmp_noel = 200;

			public string whole_map_key = "???";

			public string memo = "";

			public DateTime modified = global::XX.X.TimeEpoch;

			public Texture2D Thumbnail;

			public DateTime playstart;

			public float last_saved_x = -1f;

			public float last_saved_y = -1f;

			public string last_map_key;

			public short thumb_position = -1;

			public string safe_area_memory = "";

			public uint explore_timer;

			public bool revert_pos;

			public bool first_load;

			public enum STATE
			{
				NO_LOAD,
				LOADED,
				ERROR,
				CREATED
			}
		}

		public enum STATE
		{
			OFFLINE,
			INITTED,
			ACTIVE,
			SELECTING,
			SELECTING_CANCEL,
			SELECTING_CANCEL_NO_WRITE_MEMO,
			SELECTING_TO_LOAD_ASK_AGAIN,
			LOAD_ASK_AGAIN,
			LOAD_SUCCESS
		}
	}
}
