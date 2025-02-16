using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using PixelLiner;
using UnityEngine;
using UnityEngine.InputSystem;
using XX;

namespace evt
{
	public class EvDebugger : MonoBehaviourAutoRun, IEventListener
	{
		public static float dsl_h
		{
			get
			{
				return IN.h - 70f - 30f;
			}
		}

		public void initEvDebugger()
		{
			string text = PlayerPrefs.GetString("EVDEBUGGER_SUBCMD");
			if (!TX.noe(text))
			{
				EvDebugger.sub_cmd = text;
			}
			this.Operson_pose = new BDic<string, int>();
			this.Operson_face_emot = new BDic<string, int>();
			this.MMRD = base.gameObject.AddComponent<MultiMeshRenderer>();
			this.Ds = IN.CreateGob(base.gameObject, "-DsTopLeft").AddComponent<Designer>();
			this.DsL = IN.CreateGob(base.gameObject, "-DsL").AddComponent<Designer>();
			this.DsR = IN.CreateGob(base.gameObject, "-DsR").AddComponent<Designer>();
			MeshDrawer meshDrawer = this.MMRD.Make(MTRX.cola.Black().setA1(0.3f).C, BLEND.NORMAL, null, null);
			this.MtrPreview = MTRX.newMtr(MTRX.ShaderGDT);
			this.MtrPreview.EnableKeyword("NO_PIXELSNAP");
			this.MdPreview = this.MMRD.Make(this.MtrPreview);
			meshDrawer.Rect(0f, 0f, IN.w, IN.h, false).updateForMeshRenderer(false);
			IN.PosP(base.gameObject, 0f, 0f, -2.5f);
			IN.PosP(this.MMRD.GetGob(meshDrawer), 0f, 0f, 0.1f);
			IN.PosP(this.Ds.gameObject, (float)(-(float)EV.pw / 2) + 400f + 10f, (float)(EV.ph / 2) - 35f - 10f, -0.1f);
			IN.PosP(this.MMRD.GetGob(this.MdPreview), ((float)(EV.pw / 2) + (this.dsr_x + 215f)) / 2f - 4f, 0f, -0.01f);
			this.Ds.w = 800f;
			this.Ds.h = 70f;
			this.Ds.bgcol = MTRX.ColTrnsp;
			this.Ds.Small();
			IN.PosP(this.DsL.gameObject, this.dsl_x, this.dsl_y, -0.02f);
			this.DsL.w = 520f;
			this.DsL.h = EvDebugger.dsl_h;
			this.DsL.radius = (this.DsR.radius = 20f);
			this.DsL.item_margin_x_px = (this.DsR.item_margin_x_px = 3f);
			this.DsL.item_margin_y_px = (this.DsR.item_margin_y_px = 5f);
			this.DsL.margin_in_lr = (this.DsR.margin_in_lr = 2f);
			this.DsL.margin_in_tb = (this.DsR.margin_in_tb = 3f);
			this.DsL.animate_maxt = (this.DsR.animate_maxt = 0);
			this.DsL.use_scroll = (this.DsR.use_scroll = true);
			this.DsL.stencil_ref = 240;
			this.DsL.bgcol = (this.DsR.bgcol = MTRX.cola.Black().setA1(0.8f).C);
			IN.PosP(this.DsR.gameObject, this.dsr_x, this.dsl_y, -0.02f);
			this.DsR.w = 430f;
			this.DsR.h = EvDebugger.dsl_h;
			this.DsR.stencil_ref = 241;
			this.LiL = this.Ds.addInput(new DsnDataInput
			{
				name = "sub",
				bounds_w = 50f,
				alloc_char = new Regex("[A-Za-z0-9_\\%\\#\\&\\$]"),
				def = EvDebugger.sub_cmd,
				max = 1.0,
				fnChangedDelay = new FnFldBindings(this.fnTriggerSub),
				fnBlur = new FnFldBindings(this.fnBlurSub)
			});
			text = PlayerPrefs.GetString("EVDEBUGGER_LIR");
			if (TX.valid(text))
			{
				for (;;)
				{
					string text2 = text.Replace("\n\n", "\n");
					if (!(text2 != text))
					{
						break;
					}
					text = text2;
				}
				text = TX.trim(text);
				this.Aconsole_memory = new List<string>(TX.split(text, "\n"));
			}
			else
			{
				this.Aconsole_memory = new List<string>(10);
			}
			if (this.Aconsole_memory.Count == 0)
			{
				this.Aconsole_memory.Add("");
			}
			this.console_mem_id = this.Aconsole_memory.Count - 1;
			this.LiR = this.Ds.addInput(new DsnDataInput
			{
				name = "normal",
				bounds_w = 690f,
				def = this.Aconsole_memory[this.console_mem_id],
				fnReturn = new FnFldBindings(this.executeLiRCommand),
				fnInputtingKeyDown = new FnFldKeyInputBindings(this.fnKeyInputLiR)
			});
			this.Ds.Br().add(new DsnDataButton
			{
				name = "q",
				title = "？",
				w = 22f,
				h = 16f,
				z_push_click = false,
				hover_to_select = false,
				fnClick = new FnBtnBindings(this.fnClickChangeModeBt)
			});
			if (this.ActionAwake != null)
			{
				this.ActionAwake(this.Ds);
			}
			this.DsT = this.Ds.addTab("topleft", this.Ds.use_w - 10f, 40f, this.Ds.use_w - 10f, 30f, false);
			this.Ds.endTab(true);
			string @string = PlayerPrefs.GetString("EVDEBUGGER_VPORDER");
			if (TX.valid(@string))
			{
				this.Aperson_memory = new List<string>(TX.split(@string, "\n"));
			}
			else
			{
				this.Aperson_memory = new List<string>(16);
			}
			this.GobCanvas = new GameObject("GUI Canvas EvDebugger");
			this.GobCanvas.layer = IN.LAY("UI");
			this.GobCanvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
			CURS.Omazinai();
			EV.addListener(this);
			this.changeActivate(false);
		}

		public override void OnEnable()
		{
			base.OnEnable();
			if (this.GobCanvas != null)
			{
				this.GobCanvas.SetActive(true);
			}
		}

		public override void OnDisable()
		{
			base.OnDisable();
			if (this.GobCanvas != null)
			{
				this.GobCanvas.SetActive(false);
			}
		}

		public override void destruct()
		{
			if (this.GobCanvas != null)
			{
				IN.DestroyOne(this.GobCanvas);
				this.GobCanvas = null;
			}
			EV.remListener(this);
			base.destruct();
		}

		private void showSubMain()
		{
			if (this.mode == EvDebugger.MODE.NORMAL)
			{
				return;
			}
			this.mode = EvDebugger.MODE.NORMAL;
			this.DsL.Clear();
			this.DsR.Clear();
			this.DsT.Clear();
			this.MdPreview.clear(false, false);
			if (this.emot_enable)
			{
				this.DsL.add(new DsnDataButton
				{
					name = "&",
					skin = "normal",
					title = "&",
					skin_title = "&:汎用イメージ",
					z_push_click = false,
					hover_to_select = false,
					fnClick = new FnBtnBindings(this.fnClickChangeMode)
				}).Br();
				foreach (KeyValuePair<string, EvPerson> keyValuePair in EvPerson.getPersonDictionary())
				{
					EvPerson value = keyValuePair.Value;
					if (value.getPoseList().Count != 0)
					{
						if (this.Aperson_memory.IndexOf(value.key) == -1)
						{
							this.Aperson_memory.Add(value.key);
						}
						this.DsL.add(new DsnDataButton
						{
							name = value.key,
							skin = "normal",
							title = value.key,
							skin_title = value.key + ":" + keyValuePair.Value.talker_name,
							z_push_click = false,
							hover_to_select = false,
							fnClick = new FnBtnBindings(this.fnClickChangeMode)
						});
					}
				}
				if (this.Aperson_memory.IndexOf("&") == -1)
				{
					this.Aperson_memory.Add("&");
				}
				this.DsL.addHr(new DsnDataHr
				{
					margin_t = 12f,
					margin_b = 18f,
					line_height = 1f,
					Col = C32.d2c(2298478591U)
				});
			}
			this.DsL.Br();
			this.DsL.addP(new DsnDataP("_debug.txt", false)
			{
				size = 12f,
				swidth = 100f
			}, false);
			this.DsL.add(new DsnDataChecks
			{
				name = "debugchecks",
				keys = new string[] { "mighty", "nodamage", "weak", "allskill" },
				w = 230f,
				h = 24f,
				margin_h = 0,
				clms = 1,
				def = ((X.DEBUGMIGHTY ? 1 : 0) | (X.DEBUGNODAMAGE ? 2 : 0) | (X.DEBUGWEAK ? 4 : 0) | (X.DEBUGALLSKILL ? 8 : 0)),
				unselectable = 2,
				fnClick = new FnBtnBindings(this.fnClickDebugChecks)
			}).Br();
			this.DsL.addHr(new DsnDataHr
			{
				margin_t = 12f,
				margin_b = 18f,
				line_height = 1f,
				Col = C32.d2c(2298478591U)
			});
			Designer designer = this.DsL.Br().addTab("submain_after", this.DsL.use_w, this.DsL.use_h, this.DsL.use_w, 140f, false);
			this.fineSubMainArea(designer);
			this.DsL.endTab(true);
			EV.getVariableContainer().show(this.DsR);
		}

		private void fineSubMainArea(Designer Tab)
		{
			if (Tab == null)
			{
				Tab = this.DsL.getTab("submain_after");
				if (Tab == null)
				{
					return;
				}
			}
			Tab.Clear();
			Tab.Smallest();
			Tab.alignx = ALIGN.CENTER;
			Tab.margin_in_lr = 10f;
			Tab.margin_in_tb = 9f;
			Tab.item_margin_y_px = 12f;
			Tab.init();
			Tab.addInput(new DsnDataInput
			{
				bounds_w = Tab.use_w - 50f,
				label = "直前evt",
				editable = false,
				size = 14,
				def = this.last_event
			});
			aBtn aBtn = Tab.addButton(new DsnDataButton
			{
				title = "play",
				skin = "mini",
				w = 20f,
				h = 20f,
				unselectable = 2,
				fnClick = delegate(aBtn B)
				{
					this.stackLastEvent();
					return true;
				}
			});
			if (TX.noe(this.last_event))
			{
				aBtn.SetLocked(true, true, false);
			}
			Tab.Br();
			EventLineDebugger.prepareBasicPad(Tab, null, false);
		}

		protected override bool runIRD(float fcnt)
		{
			if (this.first_activate == 2 || this.first_activate == 3)
			{
				if (EV.isLoading())
				{
					return true;
				}
				if (this.emot_enable && EV.preLoadExternalImagesAfter())
				{
					return true;
				}
				if (this.first_activate == 3)
				{
					this.changeMode("n");
				}
				this.first_activate = 0;
			}
			if (EV.isActive(false) && this.first_activate == 0)
			{
				this.evLineCache(null, false);
				EvReader currentEvent = EV.getCurrentEvent();
				if (currentEvent != null)
				{
					this.evLineCache(currentEvent, false);
					base.gameObject.SetActive(false);
				}
				return true;
			}
			if (this.mode == EvDebugger.MODE.NONE && REG.match(this.LiR.text, this.RegPictureInitialize))
			{
				this.executeLiRCommand(null);
			}
			if (this.mode == EvDebugger.MODE.NONE)
			{
				this.fnTriggerSub(null, false, false);
			}
			Keyboard current = Keyboard.current;
			if (LabeledInputField.focus_exist)
			{
				this.lock_return_execute = true;
			}
			else if (current != null && !LabeledInputField.focus_exist)
			{
				if (this.lock_return_execute)
				{
					if (!current.enterKey.isPressed && !current.numpadEnterKey.isPressed)
					{
						this.lock_return_execute = false;
					}
				}
				else if (current.enterKey.wasPressedThisFrame || current.numpadEnterKey.wasPressedThisFrame)
				{
					this.executeLiRCommand(null);
				}
				if (current.cKey.wasPressedThisFrame)
				{
					GUIUtility.systemCopyBuffer = this.LiR.text;
					SND.Ui.play("tool_wand", false);
					X.dl("コピー:" + this.LiR.text, null, false, false);
				}
				if (current.rKey.wasPressedThisFrame && this.stackLastEvent())
				{
					return true;
				}
				if (current.escapeKey.wasPressedThisFrame)
				{
					this.changeMode("_");
				}
			}
			this.general_image_flag = 0;
			return true;
		}

		public bool fnTriggerSub(LabeledInputField LI)
		{
			return this.fnTriggerSub(LI, true, false);
		}

		private bool fnTriggerSub(LabeledInputField LI, bool fine_lir, bool force = false)
		{
			if (LI != null)
			{
				if ((EvDebugger.sub_cmd == LI.text && !force) || LI.text == "")
				{
					return true;
				}
				EvDebugger.sub_cmd = LI.text;
				PlayerPrefs.SetString("EVDEBUGGER_SUBCMD", EvDebugger.sub_cmd);
			}
			else
			{
				this.LiL.text = EvDebugger.sub_cmd;
			}
			string text = EvDebugger.sub_cmd;
			if (text != null)
			{
				if (text == "_")
				{
					this.showSubMain();
					goto IL_00E8;
				}
				if (text == "&")
				{
					this.showGeneralImage(fine_lir);
					goto IL_00E8;
				}
				if (text == "%")
				{
					this.showGF(fine_lir);
					goto IL_00E8;
				}
			}
			if (this.ActionTabInit(EvDebugger.sub_cmd, this.DsT, this.DsL, this.DsR))
			{
				this.mode = EvDebugger.MODE.OTHER;
			}
			else if (!this.showVP(fine_lir))
			{
				this.showSubMain();
			}
			IL_00E8:
			IN.save_prefs = true;
			return true;
		}

		private bool fnBlurSub(LabeledInputField LI)
		{
			if (LI.text == "")
			{
				LI.text = EvDebugger.sub_cmd;
			}
			return true;
		}

		public bool fnClickDebugChecks(aBtn B)
		{
			string title = B.title;
			if (title != null)
			{
				if (!(title == "mighty"))
				{
					if (!(title == "nodamage"))
					{
						if (!(title == "weak"))
						{
							if (title == "allskill")
							{
								X.DEBUGALLSKILL = B.isChecked();
							}
						}
						else
						{
							X.DEBUGWEAK = B.isChecked();
						}
					}
					else
					{
						X.DEBUGNODAMAGE = B.isChecked();
					}
				}
				else
				{
					X.DEBUGMIGHTY = B.isChecked();
				}
			}
			return true;
		}

		private bool showVP(bool fine_lir)
		{
			EvPerson person = EvPerson.getPerson(EvDebugger.sub_cmd, null);
			if (person == null || !this.emot_enable)
			{
				return false;
			}
			if (this.mode == EvDebugger.MODE.VP && person == this.Person)
			{
				return true;
			}
			List<EvEmotVisibility> poseList = person.getPoseList();
			int count = poseList.Count;
			if (count == 0)
			{
				return false;
			}
			this.emot_pose_listup = "";
			this.DsT.Clear();
			this.DsL.Clear();
			this.mode = EvDebugger.MODE.VP;
			this.Person = person;
			int num = 0;
			if (!this.Operson_pose.ContainsKey(this.Person.key))
			{
				this.Operson_pose[this.Person.key] = 0;
			}
			else
			{
				num = this.Operson_pose[this.Person.key];
			}
			num = (this.Operson_pose[this.Person.key] = X.MMX(0, num, count - 1));
			for (int i = 0; i < count; i++)
			{
				EvEmotVisibility evEmotVisibility = poseList[i];
				if (!evEmotVisibility.dont_appear_on_editor)
				{
					float num2 = evEmotVisibility.editor_swidth_px;
					float num3 = evEmotVisibility.editor_sheight_px;
					if (num2 > 220f)
					{
						num3 *= 220f / num2;
						num2 = 220f;
					}
					if (num3 > 180f)
					{
						num2 *= 180f / num3;
						num3 = 180f;
					}
					(this.DsL.addButtonT<aBtnEv>(new DsnDataButton
					{
						name = "l" + i.ToString(),
						title = "l" + i.ToString(),
						def = (i == num),
						w = num2,
						h = num3,
						skin = "ev_emot",
						unselectable = 2,
						fnClick = new FnBtnBindings(this.clickPose)
					}).get_Skin() as ButtonSkinEvEmot).InitEmot(this, evEmotVisibility, null, null);
					if (i == num)
					{
						this.CurPose = evEmotVisibility;
					}
				}
			}
			this.revealCurFaceEmot(true, false);
			if (!this.showVPFaceEmotion(fine_lir))
			{
				this.clickFaceEmotion(null, fine_lir);
			}
			else
			{
				this.revealCurFaceEmot(false, true);
			}
			this.fineVPTopOrder(true);
			return true;
		}

		private void fineVPTopOrder(bool recreate = false)
		{
			if (EvDebugger.sub_cmd != "&" && EvPerson.getPerson(EvDebugger.sub_cmd, null) == null)
			{
				return;
			}
			if (this.Aperson_memory[0] != EvDebugger.sub_cmd)
			{
				this.Aperson_memory.Remove(EvDebugger.sub_cmd);
				this.Aperson_memory.Insert(0, EvDebugger.sub_cmd);
				PlayerPrefs.SetString("EVDEBUGGER_VPORDER", TX.join<string>("\n", this.Aperson_memory, 0, -1));
				IN.save_prefs = true;
			}
			else if (!recreate)
			{
				return;
			}
			this.DsT.Clear();
			int count = this.Aperson_memory.Count;
			for (int i = 0; i < count; i++)
			{
				this.DsT.add(new DsnDataButton
				{
					name = this.Aperson_memory[i],
					skin = "normal",
					w = 30f,
					h = 26f,
					title = this.Aperson_memory[i],
					fnClick = new FnBtnBindings(this.fnClickChangeMode),
					unselectable = 2
				});
			}
			if (EvDebugger.sub_cmd == "&")
			{
				this.DsT.add(new DsnDataInput
				{
					name = "selector",
					label = "selector",
					def = "",
					w = 130f,
					fnChangedDelay = new FnFldBindings(this.fnGeneralImageSelector),
					unselectable = 2
				});
			}
		}

		private bool fnClickTCheckVP(aBtn B)
		{
			this.pic_replacing = this.DsT.getValueI("t_check", this.pic_replacing, true);
			if (this.mode == EvDebugger.MODE.VP)
			{
				this.clickFaceEmotion(null, true);
			}
			else if (this.mode == EvDebugger.MODE.IMAGES)
			{
				this.fineGeneralImage(null, false, false);
			}
			return true;
		}

		private bool showVPFaceEmotion(bool fine_lir)
		{
			if (this.CurPose == null || !this.emot_enable)
			{
				return false;
			}
			string text = this.CurPose.emot_pose_listup;
			if (text == this.emot_pose_listup)
			{
				return false;
			}
			this.DsR.Clear();
			this.emot_pose_listup = text;
			if (this.emot_pose_listup == null)
			{
				return false;
			}
			EvPerson.EmotLayer[] faceEmotionArray = this.CurPose.getFaceEmotionArray();
			int num = 0;
			if (!this.Operson_face_emot.ContainsKey(this.emot_pose_listup))
			{
				this.Operson_face_emot[this.emot_pose_listup] = 0;
			}
			else
			{
				num = this.Operson_face_emot[this.emot_pose_listup];
			}
			int num2 = ((faceEmotionArray != null) ? faceEmotionArray.Length : 0);
			if (num2 > 0)
			{
				float draw_scale = this.CurPose.draw_scale;
				num = (this.Operson_face_emot[this.emot_pose_listup] = X.MMX(0, num, num2 - 1));
				float num3 = this.DsR.use_w / 3f - this.DsR.item_margin_x_px * 2f - 10f;
				for (int i = 0; i < num2; i++)
				{
					EvPerson.EmotLayer emotLayer = faceEmotionArray[i];
					PxlPose pPose = emotLayer.F.pPose;
					PxlSequence pSq = emotLayer.F.pSq;
					float num4 = (float)pSq.width * this.face_emot_scale * this.CurPose.draw_scale;
					float num5 = (float)pSq.height * this.face_emot_scale * this.CurPose.draw_scale;
					Vector2 faceShift = this.CurPose.getFaceShift(0f);
					num4 -= X.Abs(faceShift.x);
					num5 -= X.Abs(faceShift.y);
					if (num4 > num3)
					{
						num5 *= num3 / num4;
						num4 = num3;
					}
					(this.DsR.addButtonT<aBtnEv>(new DsnDataButton
					{
						name = emotLayer.key,
						title = emotLayer.key,
						def = (i == num),
						w = num4,
						h = num5,
						fnClick = new FnBtnBindings(this.clickFaceEmotion),
						z_push_click = false,
						hover_to_select = false,
						skin = "ev_emot"
					}).get_Skin() as ButtonSkinEvEmot).InitEmot(this, this.CurPose, emotLayer.F, null);
					if (i == num)
					{
						this.cur_face_emotion = emotLayer.key;
					}
				}
			}
			this.clickFaceEmotion(null, fine_lir);
			return true;
		}

		private bool clickPose(aBtn B)
		{
			return this.clickPose(B, true);
		}

		private bool clickPose(aBtn B, bool fine_lir)
		{
			if (this.CurPose != null)
			{
				int num = this.Person.getPoseList().IndexOf(this.CurPose);
				if (num >= 0)
				{
					this.DsL.getBtnContainer().Get("l" + num.ToString()).SetChecked(false, true);
				}
			}
			string text = ((this.CurPose != null) ? this.CurPose.emot_pose_listup : null);
			int num2 = X.NmI(TX.slice(B.title, 1), 0, false, false);
			this.CurPose = this.Person.getPoseList()[num2];
			B.SetChecked(true, true);
			this.Operson_pose[this.Person.key] = num2;
			if (!this.showVPFaceEmotion(fine_lir))
			{
				this.clickFaceEmotion(null, true);
			}
			else if (text != this.CurPose.emot_pose_listup)
			{
				this.revealCurFaceEmot(false, true);
			}
			return true;
		}

		private bool clickFaceEmotion(aBtn B = null)
		{
			return this.clickFaceEmotion(B, true);
		}

		private bool clickFaceEmotion(aBtn B, bool fine_lir)
		{
			if (B != null)
			{
				if (this.cur_face_emotion != "")
				{
					aBtn btn = this.DsR.getBtn(this.cur_face_emotion);
					if (btn != null)
					{
						btn.SetChecked(false, true);
					}
				}
				this.cur_face_emotion = B.title;
				B.SetChecked(true, true);
				string text = this.CurPose.emot_pose_listup;
				if (text != null)
				{
					this.Operson_face_emot[text] = X.isinC<EvPerson.EmotLayer>(this.CurPose.getFaceEmotionArray(), this.CurPose.getEmotInfoByName(this.cur_face_emotion));
				}
				else
				{
					this.cur_face_emotion = "";
				}
			}
			this.DsL.getBtnContainer().fineAll(false);
			if (fine_lir)
			{
				this.LiR.text = string.Concat(new string[]
				{
					"PIC   ",
					EvDebugger.sub_cmd,
					" ",
					this.Person.getIdentifier(this.CurPose, this.cur_face_emotion),
					"    ",
					((this.pic_replacing & 1) == 0) ? "" : "N"
				});
			}
			this.MdPreview.initForImgAndTexture(this.CurPose.texture);
			this.CurPose.drawTo(this.MdPreview, 0f, this.CurPose.get_draw_y(1f), this.CurPose.draw_scale, this.cur_face_emotion, false);
			this.MdPreview.updateForMeshRenderer(false);
			return true;
		}

		private bool showGeneralImage(bool fine_lir)
		{
			if (this.mode == EvDebugger.MODE.IMAGES || !this.emot_enable)
			{
				return false;
			}
			this.mode = EvDebugger.MODE.IMAGES;
			this.DsT.Clear();
			this.DsR.Clear();
			List<EvImg> imageList = EV.Pics.getImageList();
			int num = 0;
			int count = imageList.Count;
			if (!this.Operson_pose.ContainsKey("&"))
			{
				this.Operson_pose["&"] = 0;
			}
			else
			{
				num = this.Operson_pose["&"];
			}
			num = (this.Operson_pose["&"] = X.MMX(0, num, count - 1));
			string[] Akeys = new string[count];
			this.AListImgCurrent = new List<EvImg>();
			int i = 0;
			imageList.ForEach(delegate(EvImg e)
			{
				string[] akeys = Akeys;
				int j = i;
				i = j + 1;
				akeys[j] = e.key;
				this.AListImgCurrent.Add(e);
			});
			this.RemakeGenealDsL();
			this.DsR.add(new DsnDataRadio
			{
				name = "list_name",
				def = num,
				skin = "row",
				keys = Akeys,
				w = this.DsR.use_w,
				h = 18f,
				scale = 1f,
				clms = 1,
				margin_w = 0,
				margin_h = 0,
				fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnChangedGeneralImageTitleList)
			});
			this.general_image_flag = 0;
			this.fineGeneralImage(null, true, true);
			this.fineVPTopOrder(true);
			return true;
		}

		private void RemakeGenealDsL()
		{
			List<EvImg> imageList = EV.Pics.getImageList();
			if (this.Operson_pose["&"] >= imageList.Count)
			{
				this.Operson_pose["&"] = imageList.Count - 1;
			}
			EvImg evImg = ((this.Operson_pose["&"] >= 0) ? imageList[this.Operson_pose["&"]] : null);
			this.DsL.Clear();
			int count = this.AListImgCurrent.Count;
			int num = this.AListImgCurrent.IndexOf(evImg);
			for (int i = 0; i < count; i++)
			{
				EvImg evImg2 = this.AListImgCurrent[i];
				float num2 = evImg2.swidth_px;
				float num3 = evImg2.sheight_px;
				if (num2 > 220f)
				{
					num3 *= 220f / num2;
					num2 = 220f;
				}
				if (num3 > 180f)
				{
					num2 *= 180f / num3;
					num3 = 180f;
				}
				(this.DsL.addButtonT<aBtnEv>(new DsnDataButton
				{
					name = evImg2.key,
					title = evImg2.key,
					def = (i == num),
					w = num2,
					h = num3,
					skin = "ev_emot",
					z_push_click = false,
					hover_to_select = false,
					fnClick = new FnBtnBindings(this.fnClickGeneralDsL)
				}).get_Skin() as ButtonSkinEvEmot).InitEmot(this, null, null, evImg2.PF);
			}
		}

		private bool fnChangedGeneralImageTitleList(BtnContainerRadio<aBtn> _B, int pre_value, int cur_value)
		{
			if ((this.general_image_flag & 1) > 0)
			{
				return true;
			}
			this.general_image_flag |= 1;
			EvImg evImg = this.AListImgCurrent[cur_value];
			if (this.general_image_flag != 3)
			{
				int num = this.AListImgCurrent.IndexOf(evImg);
				if (num >= 0)
				{
					this.fnClickGeneralDsL(this.DsL.getBtn(num));
				}
				this.fineGeneralImage(evImg, false, false);
				this.revealGeneral(true, false);
			}
			return true;
		}

		private bool fnClickGeneralDsL(aBtn B)
		{
			if ((this.general_image_flag & 2) > 0)
			{
				return true;
			}
			this.general_image_flag |= 2;
			EvImg pic = EV.Pics.getPic(B.title, false, true);
			if (pic == null)
			{
				return true;
			}
			EvImg evImg = EV.Pics.getImageList()[this.Operson_pose["&"]];
			int num = this.AListImgCurrent.IndexOf(evImg);
			if (num >= 0)
			{
				this.DsL.getBtn(num).SetChecked(false, true);
			}
			int num2 = this.AListImgCurrent.IndexOf(pic);
			this.DsL.getBtn(num2).SetChecked(true, true);
			if (this.general_image_flag != 3)
			{
				this.fineGeneralImage(pic, false, false);
				this.revealGeneral(false, true);
				this.DsR.Get("list_name", false).setValue(num2.ToString());
			}
			return true;
		}

		private void fineGeneralImage(EvImg I, bool reveal_l = false, bool reveal_r = false)
		{
			if (I == null)
			{
				I = EV.Pics.getImageList()[this.Operson_pose["&"]];
			}
			else
			{
				this.Operson_pose["&"] = EV.Pics.getImageList().IndexOf(I);
			}
			this.LiR.setValue("PIC   &0   " + I.key + "   " + (((this.pic_replacing & 1) == 0) ? "" : "N"));
			if (reveal_l || reveal_r)
			{
				this.revealGeneral(reveal_l, reveal_r);
			}
			this.MdPreview.initForImgAndTexture(I.PF.getImageTexture());
			this.MdPreview.RotaPF(0f, 0f, 1f, 1f, 0f, I.PF, false, false, false, uint.MaxValue, false, 0);
			this.MdPreview.updateForMeshRenderer(false);
		}

		private void revealGeneral(bool rev_l = true, bool rev_r = true)
		{
			if (this.mode != EvDebugger.MODE.IMAGES)
			{
				return;
			}
			int num = this.AListImgCurrent.IndexOf(EV.Pics.getImageList()[this.Operson_pose["&"]]);
			if (num == -1)
			{
				return;
			}
			if (rev_l)
			{
				aBtn btn = this.DsL.getBtn(num);
				if (btn != null)
				{
					this.DsL.reveal(btn.transform, 0f, 0f, true);
				}
			}
			if (rev_r)
			{
				aBtn aBtn = (this.DsR.Get("list_name", false) as BtnContainerRunner).Get(num);
				if (aBtn != null)
				{
					this.DsR.reveal(aBtn.transform, 0f, 0f, true);
				}
			}
		}

		private bool fnGeneralImageSelector(LabeledInputField LI)
		{
			string text = LI.text;
			List<EvImg> imageList = EV.Pics.getImageList();
			int count = imageList.Count;
			if (text == "" == (this.AListImgCurrent.Count == count))
			{
				return true;
			}
			this.AListImgCurrent.Clear();
			string[] Akeys;
			if (text == "")
			{
				int i = 0;
				Akeys = new string[count];
				imageList.ForEach(delegate(EvImg e)
				{
					string[] akeys = Akeys;
					int i2 = i;
					i = i2 + 1;
					akeys[i2] = e.key;
					this.AListImgCurrent.Add(e);
				});
			}
			else
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					for (int k = 0; k < count; k++)
					{
						EvImg evImg = imageList[k];
						stb.Set(evImg.key).ToLower();
						if (TX.qsCheckRelative(stb, text, false) > 0f)
						{
							this.AListImgCurrent.Add(evImg);
						}
					}
				}
				int j = 0;
				Akeys = new string[this.AListImgCurrent.Count];
				this.AListImgCurrent.ForEach(delegate(EvImg e)
				{
					string[] akeys2 = Akeys;
					int j2 = j;
					j = j2 + 1;
					akeys2[j2] = e.key;
				});
			}
			this.RemakeGenealDsL();
			(this.DsR.Get("list_name", false) as BtnContainerRunner).BCon.Remake(Akeys, "");
			return true;
		}

		private bool showGF(bool fine_lir)
		{
			if (this.mode == EvDebugger.MODE.GF)
			{
				return false;
			}
			this.mode = EvDebugger.MODE.GF;
			this.DsT.Clear();
			this.DsR.Clear();
			this.DsL.Clear();
			this.DsR.add(new DsnDataP("", false)
			{
				name = "gfc_data",
				swidth = this.DsR.use_w,
				text_auto_condense = true,
				size = 13f,
				html = true,
				TxCol = MTRX.ColWhite,
				text = GF.getDebugStringForTextRenderer()
			});
			if (this.Agfc_index_mem == null)
			{
				this.Agfc_index_mem = new string[8];
				this.Agfb_index_mem = new string[8];
				string[] array = TX.split(PlayerPrefs.GetString("DEBUG_GFC_MEM"), "\n");
				string[] array2 = TX.split(PlayerPrefs.GetString("DEBUG_GFB_MEM"), "\n");
				int num = X.Mn(8, array.Length);
				for (int i = 0; i < num; i++)
				{
					this.Agfc_index_mem[i] = array[i];
				}
				for (int j = num; j < 8; j++)
				{
					this.Agfc_index_mem[j] = "";
				}
				num = X.Mn(8, array2.Length);
				for (int k = 0; k < num; k++)
				{
					this.Agfb_index_mem[k] = array2[k];
				}
				for (int l = num; l < 8; l++)
				{
					this.Agfb_index_mem[l] = "";
				}
			}
			for (int m = 0; m < 8; m++)
			{
				this.DsL.addInput(new DsnDataInput
				{
					name = "gfc_key" + m.ToString(),
					label = "GFC",
					def = this.Agfc_index_mem[m],
					w = 220f,
					size = 16,
					fnChangedDelay = new FnFldBindings(this.fineGfc)
				});
				this.DsL.addInput(new DsnDataInput
				{
					name = "gfc_val" + m.ToString(),
					w = this.DsL.use_w - 10f,
					size = 16,
					fnBlur = new FnFldBindings(this.fineGfcValue)
				});
			}
			for (int n = 0; n < 8; n++)
			{
				this.DsL.Br().addInput(new DsnDataInput
				{
					name = "gfb_key" + n.ToString(),
					label = "GFB",
					def = this.Agfb_index_mem[n],
					w = 220f,
					size = 16,
					fnChangedDelay = new FnFldBindings(this.fineGfb)
				});
				this.DsL.addInput(new DsnDataInput
				{
					name = "gfb_val" + n.ToString(),
					w = this.DsL.use_w - 10f,
					size = 16,
					fnBlur = new FnFldBindings(this.fineGfbValue)
				});
			}
			this.DsL.addHr(new DsnDataHr
			{
				Col = MTRX.ColWhite
			});
			LabeledInputField labeledInputField = this.DsL.addInput(new DsnDataInput
			{
				name = "pvv_val",
				label = "PVV",
				w = this.DsL.use_w - 10f,
				size = 16,
				fnBlur = new FnFldBindings(this.finePvvValue)
			});
			this.fineGfc(-1);
			this.fineGfb(-1);
			this.finePvvValue(labeledInputField, false);
			return true;
		}

		private void fineGfc(int si = -1)
		{
			for (int i = 0; i < 8; i++)
			{
				if (i == si || si < 0)
				{
					int num = X.NmI(this.Agfc_index_mem[i], -1, false, false);
					if (num == -1)
					{
						num = GF.getReplaceKeyForC(this.Agfc_index_mem[i]);
					}
					if (X.BTW(0f, (float)num, 128f))
					{
						this.DsL.Get("gfc_val" + i.ToString(), false).setValue(GF.getC(num).ToString());
					}
					else
					{
						this.DsL.Get("gfc_val" + i.ToString(), false).setValue("??");
					}
				}
			}
		}

		private bool fineGfc(LabeledInputField Tr)
		{
			string name = this.DsL.getName(Tr);
			int num = ((name == null) ? (-1) : X.NmI(TX.slice(name, 7), -1, false, false));
			if (num < 0)
			{
				return false;
			}
			this.Agfc_index_mem[num] = Tr.text;
			PlayerPrefs.SetString("DEBUG_GFC_MEM", TX.join<string>("\n", this.Agfc_index_mem, 0, -1));
			IN.save_prefs = true;
			this.fineGfc(num);
			return true;
		}

		private void fineGfb(int si = -1)
		{
			for (int i = 0; i < 8; i++)
			{
				if (i == si || si < 0)
				{
					int num = X.NmI(this.Agfb_index_mem[i], -1, false, false);
					if (num == -1)
					{
						num = GF.getReplaceKeyForB(this.Agfb_index_mem[i]);
					}
					if (X.BTW(0f, (float)num, 128f))
					{
						this.DsL.Get("gfb_val" + i.ToString(), false).setValue(GF.getB(num) ? "1" : "0");
					}
					else
					{
						this.DsL.Get("gfb_val" + i.ToString(), false).setValue("??");
					}
				}
			}
		}

		private bool fineGfb(LabeledInputField Tr)
		{
			string name = this.DsL.getName(Tr);
			int num = ((name == null) ? (-1) : X.NmI(TX.slice(name, 7), -1, false, false));
			if (num < 0)
			{
				return false;
			}
			this.Agfb_index_mem[num] = Tr.text;
			PlayerPrefs.SetString("DEBUG_GFB_MEM", TX.join<string>("\n", this.Agfb_index_mem, 0, -1));
			IN.save_prefs = true;
			this.fineGfb(num);
			return true;
		}

		private bool fineGfcValue(LabeledInputField Tr)
		{
			return this.fineGfcValue(Tr, TX.valid(Tr.text));
		}

		private bool fineGfcValue(LabeledInputField Tr, bool write_mode)
		{
			string name = this.DsL.getName(Tr);
			int num = ((name == null) ? (-1) : X.NmI(TX.slice(name, 7), -1, false, false));
			if (num < 0)
			{
				return false;
			}
			int num2 = X.NmI(this.Agfc_index_mem[num], -1, false, false);
			if (num2 == -1)
			{
				num2 = GF.getReplaceKeyForC(this.Agfc_index_mem[num]);
			}
			if (X.BTW(0f, (float)num2, 128f))
			{
				if (write_mode)
				{
					uint num3 = (uint)X.MMX(0, X.NmI(Tr.text, 0, false, false), 31);
					GF.setC(num2, num3);
					this.DsR.Get("gfc_data", false).setValue(GF.getDebugStringForTextRenderer());
				}
				else
				{
					Tr.text = GF.getC(num2).ToString();
				}
			}
			return true;
		}

		private bool fineGfbValue(LabeledInputField Tr)
		{
			return this.fineGfbValue(Tr, TX.valid(Tr.text));
		}

		private bool fineGfbValue(LabeledInputField Tr, bool write_mode)
		{
			string name = this.DsL.getName(Tr);
			int num = ((name == null) ? (-1) : X.NmI(TX.slice(name, 7), -1, false, false));
			if (num < 0)
			{
				return false;
			}
			int num2 = X.NmI(this.Agfb_index_mem[num], -1, false, false);
			if (num2 == -1)
			{
				num2 = GF.getReplaceKeyForB(this.Agfb_index_mem[num]);
			}
			if (X.BTW(0f, (float)num2, 128f))
			{
				if (write_mode)
				{
					int num3 = X.MMX(0, X.NmI(Tr.text, 0, false, false), 1);
					GF.setB(num2, num3 > 0);
					this.DsR.Get("gfc_data", false).setValue(GF.getDebugStringForTextRenderer());
				}
				else
				{
					Tr.text = (GF.getB(num2) ? "1" : "0");
				}
			}
			return true;
		}

		private bool finePvvValue(LabeledInputField Tr)
		{
			return this.finePvvValue(Tr, TX.valid(Tr.text));
		}

		private bool finePvvValue(LabeledInputField Tr, bool write_mode)
		{
			if (write_mode)
			{
				string text = GF.setPVV(X.NmI(Tr.text, -1, true, false), true);
				if (text != null)
				{
					X.de(text, null);
				}
				else
				{
					this.DsR.Get("gfc_data", false).setValue(GF.getDebugStringForTextRenderer());
				}
			}
			else
			{
				Tr.text = GF.TxEvalContentForDebug("PVV");
			}
			return true;
		}

		private void saveLiRText()
		{
			this.console_mem_id = X.MMX(0, this.console_mem_id, this.Aconsole_memory.Count - 1);
			if (this.Aconsole_memory.Count > 0 && this.Aconsole_memory[this.console_mem_id] == this.LiR.text)
			{
				return;
			}
			this.console_mem_id++;
			this.Aconsole_memory.RemoveRange(this.console_mem_id, this.Aconsole_memory.Count - this.console_mem_id);
			this.Aconsole_memory.Add(this.LiR.text);
			if (this.Aconsole_memory.Count > 10)
			{
				this.Aconsole_memory.RemoveRange(0, this.Aconsole_memory.Count - 10);
			}
			PlayerPrefs.SetString("EVDEBUGGER_LIR", TX.join<string>("\n", this.Aconsole_memory, 0, -1));
			IN.save_prefs = true;
		}

		private bool executeLiRCommand(LabeledInputField LI = null)
		{
			string[] array = CsvReader.RegSpace.Split(this.LiR.text);
			if (array.Length == 0 || this.LiR.text == "")
			{
				return true;
			}
			this.saveLiRText();
			string text = array[0].ToUpper();
			if (text != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
				if (num <= 2124021762U)
				{
					if (num <= 1499723975U)
					{
						if (num != 1020392786U)
						{
							if (num != 1499723975U)
							{
								goto IL_0527;
							}
							if (!(text == "GFC_PUT"))
							{
								goto IL_0527;
							}
							goto IL_04A5;
						}
						else
						{
							if (!(text == "GFC_SET_MX"))
							{
								goto IL_0527;
							}
							goto IL_04A5;
						}
					}
					else if (num != 1855579858U)
					{
						if (num != 1962566587U)
						{
							if (num != 2124021762U)
							{
								goto IL_0527;
							}
							if (!(text == "PIC_B"))
							{
								goto IL_0527;
							}
						}
						else
						{
							if (!(text == "VP"))
							{
								goto IL_0527;
							}
							if (!this.emot_enable)
							{
								this.emot_enable = true;
								this.first_activate = 3;
								EV.preLoadExternalImages();
								goto IL_0544;
							}
							this.changeMode("n");
							goto IL_0544;
						}
					}
					else if (!(text == "PIC_R"))
					{
						goto IL_0527;
					}
				}
				else if (num <= 2636597474U)
				{
					if (num != 2223327817U)
					{
						if (num != 2479854664U)
						{
							if (num != 2636597474U)
							{
								goto IL_0527;
							}
							if (!(text == "GFC_SET"))
							{
								goto IL_0527;
							}
							goto IL_04A5;
						}
						else if (!(text == "GFB_PUT"))
						{
							goto IL_0527;
						}
					}
					else if (!(text == "GFB_SET"))
					{
						goto IL_0527;
					}
					if (array.Length < 3)
					{
						return true;
					}
					GF.commandGfbSet(array[1], array[2]);
					this.fineGfField();
					goto IL_0544;
				}
				else if (num != 3908569443U)
				{
					if (num != 4015488642U)
					{
						if (num != 4110780549U)
						{
							goto IL_0527;
						}
						if (!(text == "GFC_PUT_MX"))
						{
							goto IL_0527;
						}
						goto IL_04A5;
					}
					else
					{
						if (!(text == "EVT"))
						{
							goto IL_0527;
						}
						if (array.Length <= 1 && this.stackLastEvent())
						{
							return false;
						}
						if (!EV.getEventContent(array[1], null))
						{
							X.dl("イベントファイル evt/" + array[1] + "が見当たりません", null, false, false);
							goto IL_0544;
						}
						this.evLineCache(null, false);
						EV.stack(array[1], 0, -1, (array.Length >= 3) ? X.slice<string>(array, 2) : null, null);
						goto IL_0544;
					}
				}
				else if (!(text == "PIC"))
				{
					goto IL_0527;
				}
				if (array.Length <= 1 || !this.emot_enable)
				{
					return true;
				}
				if (array[1].IndexOf("&") == 0)
				{
					if (array.Length <= 2)
					{
						this.changeMode("&");
						return true;
					}
					EvImg pic = EV.Pics.getPic(array[2], false, true);
					if (pic == null)
					{
						this.changeMode("&");
						return true;
					}
					this.Operson_pose["&"] = EV.Pics.getImageList().IndexOf(pic);
					if (this.mode != EvDebugger.MODE.IMAGES || this.AListImgCurrent == null)
					{
						this.changeMode("&");
						goto IL_0544;
					}
					int num2 = this.AListImgCurrent.IndexOf(pic);
					if (num2 >= 0)
					{
						this.fnClickGeneralDsL(this.DsL.getBtn(num2));
					}
					this.revealGeneral(true, false);
					goto IL_0544;
				}
				else
				{
					EvPerson person = EvPerson.getPerson(array[1], null);
					if (person == null || person.getPoseList().Count == 0)
					{
						X.dl("EvPerson " + array[1] + "が見当たりません", null, false, false);
						return true;
					}
					EvDebugger.sub_cmd = array[1];
					EvEmotVisibility evEmotVisibility = null;
					PxlFrame pxlFrame = null;
					if (array.Length > 2 && REG.match(array[2], EvPerson.RegPicDirAndAM))
					{
						string rightContext = REG.rightContext;
						string r = REG.R1;
						evEmotVisibility = person.getPoseByName(r);
						if (evEmotVisibility != null)
						{
							this.Operson_pose[person.key] = person.getPoseList().IndexOf(evEmotVisibility);
							EvPerson.EmotLayer emotInfoByName = evEmotVisibility.getEmotInfoByName(rightContext);
							if (emotInfoByName != null)
							{
								pxlFrame = emotInfoByName.F;
								this.Operson_face_emot[evEmotVisibility.emot_pose_listup] = evEmotVisibility.getEmotByFace(pxlFrame);
							}
							else
							{
								pxlFrame = null;
							}
						}
					}
					if (this.mode != EvDebugger.MODE.VP || person != this.Person)
					{
						this.changeMode(EvDebugger.sub_cmd);
						goto IL_0544;
					}
					if (pxlFrame != null && evEmotVisibility == this.CurPose)
					{
						this.clickFaceEmotion(this.DsR.getBtn(this.Operson_face_emot[evEmotVisibility.emot_pose_listup]), false);
						this.revealCurFaceEmot(false, true);
						goto IL_0544;
					}
					if (evEmotVisibility != null && person == this.Person)
					{
						this.clickPose(this.DsL.getBtn(this.Operson_pose[person.key]), false);
						if (pxlFrame != null)
						{
							this.clickFaceEmotion(this.DsR.getBtn(this.Operson_face_emot[evEmotVisibility.emot_pose_listup]), false);
						}
						this.revealCurFaceEmot(false, true);
						goto IL_0544;
					}
					goto IL_0544;
				}
				IL_04A5:
				if (array.Length < 3)
				{
					return true;
				}
				GF.commandGfcSet(array[1], array[2], array[0] == "GFC_SET_MX" || array[0] == "GFC_PUT_MX");
				this.fineGfField();
				goto IL_0544;
			}
			IL_0527:
			if (this.ActionCommand != null && !this.ActionCommand(array))
			{
				this.changeActivate(false);
			}
			IL_0544:
			IN.save_prefs = true;
			return true;
		}

		private bool fnKeyInputLiR(LabeledInputField Li, KeyCode k)
		{
			if (k == KeyCode.UpArrow)
			{
				if (this.console_mem_id > 0)
				{
					LabeledInputField liR = this.LiR;
					List<string> aconsole_memory = this.Aconsole_memory;
					int num = this.console_mem_id - 1;
					this.console_mem_id = num;
					liR.setValue(aconsole_memory[num], false, true);
				}
				return false;
			}
			if (k != KeyCode.DownArrow)
			{
				return true;
			}
			if (this.console_mem_id < this.Aconsole_memory.Count - 1)
			{
				LabeledInputField liR2 = this.LiR;
				List<string> aconsole_memory2 = this.Aconsole_memory;
				int num = this.console_mem_id + 1;
				this.console_mem_id = num;
				liR2.setValue(aconsole_memory2[num], false, true);
			}
			return false;
		}

		private bool fnClickChangeMode(aBtn B)
		{
			this.changeMode(B.title);
			return true;
		}

		private bool fnClickChangeModeBt(aBtn B)
		{
			string title = B.title;
			if (title != null && title == "_PTC")
			{
				this.changeMode(B.title);
			}
			else
			{
				this.changeMode("_");
			}
			return true;
		}

		public bool stackLastEvent()
		{
			if (this.last_event == "" || !EV.getEventContent(this.last_event, null))
			{
				return false;
			}
			this.evLineCache(null, false);
			EV.stack(this.last_event, 0, -1, null, null);
			return true;
		}

		public bool evLineCache(EvReader ER, bool only_if_isactive = false)
		{
			if (this.DsEL == null)
			{
				if (only_if_isactive)
				{
					return false;
				}
				this.DsEL = new GameObject("EventLineDebugger").AddComponent<EventLineDebugger>();
			}
			else if (!this.DsEL.gameObject.activeSelf)
			{
				if (only_if_isactive)
				{
					return false;
				}
				this.DsEL.gameObject.SetActive(true);
			}
			if (ER != null)
			{
				this.DsEL.evLineCache(ER);
			}
			return true;
		}

		public void evLineRelease()
		{
			EventLineDebugger.saveLineDebuggerData();
			this.BreakPointER = null;
			if (this.DsEL == null)
			{
				return;
			}
			this.DsEL.evLineRelease();
		}

		public bool evLineCanProgress(EvReader ER)
		{
			if (ER == this.BreakPointER && ER != null)
			{
				if (this.first_activate != 0 || !(this.DsEL != null) || !this.DsEL.BreakPointConsidered(ER))
				{
					return false;
				}
				this.BreakPointER = null;
			}
			return this.DsEL == null || this.DsEL.evLineCanProgress(ER);
		}

		public bool initBreakPoint(EvReader ER)
		{
			if (!EventLineDebugger.use_breakpoint)
			{
				return false;
			}
			if (this.DsEL != null && this.DsEL.isEventReading())
			{
				this.DsEL.BreakPointConsidered(ER);
				return true;
			}
			this.BreakPointER = ER;
			this.changeActivate(true);
			return true;
		}

		public void addIgnoreEventHeader(string s)
		{
			if (this.AIgnoreEventHeader == null)
			{
				this.AIgnoreEventHeader = new List<string>(1);
			}
			this.AIgnoreEventHeader.Add(s);
		}

		bool IEventListener.EvtRead(EvReader ER, StringHolder rER, int skipping)
		{
			return false;
		}

		bool IEventListener.EvtOpen(bool is_first_or_end)
		{
			if (is_first_or_end)
			{
				this.last_event_current = null;
			}
			if (this.last_event_current == null)
			{
				EvReader currentEvent = EV.getCurrentEvent();
				this.last_event_current = currentEvent.name;
				if (this.AIgnoreEventHeader != null)
				{
					for (int i = this.AIgnoreEventHeader.Count - 1; i >= 0; i--)
					{
						if (TX.isStart(this.last_event_current, this.AIgnoreEventHeader[i], 0))
						{
							this.last_event_current = null;
							break;
						}
					}
				}
				if (this.last_event_current != null)
				{
					this.last_event = this.last_event_current;
				}
			}
			return true;
		}

		bool IEventListener.EvtClose(bool is_first_or_end)
		{
			return true;
		}

		int IEventListener.EvtCacheRead(EvReader ER, string cmd, CsvReader rER)
		{
			return 0;
		}

		bool IEventListener.EvtMoveCheck()
		{
			return true;
		}

		public Designer get_DsT()
		{
			return this.DsT;
		}

		public Designer get_DsL()
		{
			return this.DsL;
		}

		public Designer get_DsR()
		{
			return this.DsR;
		}

		public void changeMode(string s)
		{
			if (TX.noe(s))
			{
				return;
			}
			int num;
			if (s.IndexOf("_") != 0 && (num = s.IndexOf(":")) != -1)
			{
				s = TX.slice(s, 0, num);
			}
			this.LiL.text = s;
			this.fnTriggerSub(this.LiL, true, true);
		}

		private void revealCurFaceEmot(bool rev_l = true, bool rev_r = true)
		{
			if (this.mode != EvDebugger.MODE.VP || this.CurPose == null)
			{
				return;
			}
			if (rev_l)
			{
				int num = this.Person.getPoseList().IndexOf(this.CurPose);
				if (num >= 0)
				{
					aBtn aBtn = this.DsL.getBtnContainer().Get("l" + num.ToString());
					this.DsL.reveal(aBtn.transform, 0f, 0f, true);
				}
			}
			if (rev_r && this.CurPose.getFaceEmotionArray() != null)
			{
				aBtn aBtn2 = this.DsR.getBtnContainer().Get(this.cur_face_emotion);
				this.DsR.reveal(aBtn2.transform, 0f, 0f, true);
			}
		}

		public void changeActivate(bool f)
		{
			if (f && this.first_activate == 1)
			{
				this.first_activate = 0;
				if (this.emot_enable && EV.preLoadExternalImages())
				{
					this.first_activate = 2;
				}
			}
			if (f)
			{
				this.lock_return_execute = true;
				if (EV.ActionEvDebugEnableChanged != null)
				{
					EV.ActionEvDebugEnableChanged(true);
				}
				if (EV.isActive(false) && this.first_activate == 0)
				{
					EvReader currentEvent = EV.getCurrentEvent();
					this.evLineCache(currentEvent, false);
					if (currentEvent != null)
					{
						base.gameObject.SetActive(false);
						return;
					}
				}
				base.gameObject.SetActive(f);
				IN.FlgUiUse.Add("EVDEBUGGER");
				if (this.ActionActivate != null)
				{
					this.ActionActivate(this, true);
				}
				EvDebugger.MODE mode = this.mode;
				if (mode == EvDebugger.MODE.NORMAL)
				{
					this.DsR.Clear();
					EV.getVariableContainer().show(this.DsR);
					this.fineSubMainArea(null);
					return;
				}
				if (mode == EvDebugger.MODE.GF)
				{
					this.fineGfField();
					return;
				}
				if (mode != EvDebugger.MODE.OTHER)
				{
					return;
				}
				if (this.ActionTabInit != null)
				{
					this.ActionTabInit(this.LiL.text, this.DsT, this.DsL, this.DsR);
					return;
				}
			}
			else
			{
				if (EV.ActionEvDebugEnableChanged != null)
				{
					EV.ActionEvDebugEnableChanged(false);
				}
				IN.FlgUiUse.Rem("EVDEBUGGER");
				if (this.ActionActivate != null)
				{
					this.ActionActivate(this, false);
				}
				base.gameObject.SetActive(f);
				if (this.DsEL != null)
				{
					this.DsEL.gameObject.SetActive(f);
				}
			}
		}

		private void fineGfField()
		{
			if (this.mode == EvDebugger.MODE.GF)
			{
				IVariableObject variableObject = this.DsR.Get("gfc_data", false);
				if (variableObject != null)
				{
					variableObject.setValue(GF.getDebugStringForTextRenderer());
				}
				this.fineGfc(-1);
				this.fineGfb(-1);
				this.finePvvValue(this.DsL.Get("pvv_val", false) as LabeledInputField, false);
			}
		}

		public void setRightPromptField(string t)
		{
			this.LiR.text = t;
		}

		public bool isActive()
		{
			return base.gameObject.activeSelf;
		}

		public bool isActive(EvDebugger.MODE _mode)
		{
			return base.gameObject.activeSelf && this.mode == _mode;
		}

		public bool isELActive()
		{
			return this.DsEL != null && this.DsEL.gameObject.activeSelf;
		}

		public bool isELKettei()
		{
			return this.DsEL != null && this.DsEL.isKettei();
		}

		public EventLineDebugger getLineDebugger()
		{
			return this.DsEL;
		}

		private MultiMeshRenderer MMRD;

		private Designer Ds;

		private Designer DsL;

		private Designer DsR;

		private Designer DsT;

		private EventLineDebugger DsEL;

		protected GameObject GobCanvas;

		private LabeledInputField LiR;

		private LabeledInputField LiL;

		private MeshDrawer MdPreview;

		private const float ds_w = 800f;

		private const float ds_h = 70f;

		private const float dsl_w = 520f;

		public const float dsr_w = 430f;

		private const float marg = 10f;

		private readonly float dsl_x = (float)(-(float)EV.pw / 2) + 260f + 10f;

		private readonly float dsl_y = (float)(-(float)EV.ph / 2) + (IN.h - 70f - 30f) / 2f + 10f;

		private readonly float dsr_x = (float)(-(float)EV.pw / 2) + 520f + 10f + 5f + 215f;

		public static string sub_cmd = "_";

		private EvDebugger.MODE mode = EvDebugger.MODE.NONE;

		private EvPerson Person;

		private Material MtrPreview;

		private EvEmotVisibility CurPose;

		private string emot_pose_listup = "";

		private BDic<string, int> Operson_pose;

		private BDic<string, int> Operson_face_emot;

		private List<string> Aperson_memory;

		public string cur_face_emotion = "";

		private Regex RegPictureInitialize = new Regex("^PIC[\\t \\s]");

		private string[] Agfc_index_mem;

		private string[] Agfb_index_mem;

		private List<string> Aconsole_memory;

		private const int CONSOLE_MAX = 10;

		private int console_mem_id;

		public string last_event = "";

		private string last_event_current;

		public EvDebugger.FnDebugConsole ActionCommand = (string[] Acmd) => true;

		public Action<Designer> ActionAwake = delegate(Designer DsTL)
		{
		};

		public Action<EvDebugger, bool> ActionActivate = delegate(EvDebugger Dbg, bool activate)
		{
		};

		public Func<string, Designer, Designer, Designer, bool> ActionTabInit = (string category, Designer _DsT, Designer _DsL, Designer _DsR) => false;

		private int pic_replacing;

		public float face_emot_scale = 0.66f;

		private byte first_activate = 1;

		public bool lock_return_execute;

		private bool emot_enable;

		private RenderTexture TxBuf;

		private int general_image_flag;

		private List<EvImg> AListImgCurrent;

		private const int GF_fld_max = 8;

		public EvReader BreakPointER;

		private List<string> AIgnoreEventHeader;

		public delegate bool FnDebugConsole(string[] Acmd);

		public enum MODE
		{
			NONE = -1,
			NORMAL,
			VP,
			PARTICLE,
			IMAGES,
			GF,
			OTHER
		}
	}
}
