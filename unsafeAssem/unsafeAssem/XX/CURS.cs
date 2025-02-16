using System;
using System.Text.RegularExpressions;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public static class CURS
	{
		public static void fineScene()
		{
			CURS.Cam = null;
			CURS.setActive(false, false);
			CURS.Active = new Flagger(delegate(FlaggerT<string> V)
			{
				CURS.setActive(true, false);
			}, delegate(FlaggerT<string> V)
			{
				CURS.setActive(false, false);
			});
			if (CURS.Gob != null)
			{
				ValotileRenderer component = CURS.Gob.GetComponent<ValotileRenderer>();
				if (component != null)
				{
					component.connectUI();
				}
				IN.setZAbs(CURS.Trs, -9.5f);
			}
		}

		public static void Omazinai()
		{
			CURS.Active.Add("Omazinai");
			CURS.Active.Rem("Omazinai");
		}

		public static void readCursCsv(TextAsset _LT)
		{
			if (_LT == null)
			{
				return;
			}
			CsvReader csvReader = new CsvReader(_LT.text, CsvReader.RegSpace, true);
			if (CURS.AStack == null)
			{
				CURS.AStack = new CursStack[32];
				CURS.stack_len = 0;
				CURS.AKind = new CursKind[0];
				CURS.ACateg = new CursCategory[64];
				CURS.defaultCategory = null;
				CURS.mxw = (CURS.mxh = 0);
				CURS.current_family = "";
				CURS.need_fine = false;
			}
			string text = "";
			Regex regex = new Regex("^\\w+$");
			while (csvReader.read())
			{
				if (csvReader.cmd == "/*" || csvReader.cmd == "/*___")
				{
					string index = csvReader.getIndex((csvReader.cmd == "/*") ? 2 : 1);
					text = "";
					if (regex.Match(index).Success)
					{
						text = index;
					}
				}
				else
				{
					if (csvReader.cmd == "%FOLLOW_MOUSE")
					{
						CURS.follow_mouse = 1;
					}
					if (csvReader.cmd == "%IMG" && csvReader._1 != "" && CURS.getCK(csvReader._1, text, true) == null && csvReader.clength >= 4)
					{
						CursKind cursKind;
						if (csvReader.clength == 4)
						{
							cursKind = new CursKind(csvReader._1, csvReader._1, text, X.NmI(csvReader._2, -1000, true, false), X.NmI(csvReader._3, -1000, true, false));
						}
						else
						{
							cursKind = new CursKind(csvReader._1, csvReader._2, text, X.NmI(csvReader._3, -1000, true, false), X.NmI(csvReader._4, -1000, true, false));
						}
						if (cursKind.valid)
						{
							X.push<CursKind>(ref CURS.AKind, cursKind, -1);
						}
					}
					if (csvReader.cmd == "%DEFAULT_BTN_HOVER")
					{
						if (csvReader.clength >= 3)
						{
							CURS.default_btn_hover_categ = csvReader._1;
							CURS.default_btn_hover_curs = csvReader._2;
						}
						else if (csvReader.clength == 2)
						{
							CURS.default_btn_hover_curs = csvReader._1;
						}
					}
					if (csvReader.cmd == "%PRIORITY")
					{
						CursCategory cc = CURS.getCC(csvReader._1, false);
						if (cc != null)
						{
							if (CURS.defaultCategory == null)
							{
								CURS.defaultCategory = cc;
							}
							cc.priority = X.NmI(csvReader._2, 0, true, false);
						}
					}
				}
			}
		}

		public static CursKind getCK(string key, string family, bool force = false)
		{
			if (TX.noe(key) || (!force && !CURS.active))
			{
				return null;
			}
			int num = CURS.AKind.Length;
			for (int i = 0; i < num; i++)
			{
				CursKind cursKind = CURS.AKind[i];
				if (cursKind == null)
				{
					break;
				}
				if (cursKind.key == key && (cursKind.family == family || family == ""))
				{
					return cursKind;
				}
			}
			return null;
		}

		public static CursCategory getCC(string key, bool no_make = true)
		{
			if (TX.noe(key) || (no_make && !CURS.active))
			{
				return null;
			}
			int num = CURS.ACateg.Length;
			CursCategory cursCategory;
			for (int i = 0; i < num; i++)
			{
				cursCategory = CURS.ACateg[i];
				if (cursCategory == null)
				{
					break;
				}
				if (cursCategory.key == key)
				{
					return cursCategory;
				}
			}
			if (no_make)
			{
				return null;
			}
			cursCategory = new CursCategory(key);
			X.pushToEmpty<CursCategory>(CURS.ACateg, cursCategory, 1);
			return cursCategory;
		}

		public static void deleteGameObject()
		{
			if (CURS.Gob == null)
			{
				return;
			}
			IN.DestroyE(CURS.Gob);
			CURS.Gob = null;
			CURS.gob_enabled = false;
			CURS.Trs = null;
			CURS.Md = null;
			CURS.Mrd = null;
			CURS.delete_flag = false;
			CURS.fineScene();
		}

		public static bool gui_draw
		{
			get
			{
				return CURS.gui_draw_;
			}
			set
			{
				if (CURS.gui_draw_ == value)
				{
					return;
				}
				CURS.gui_draw_ = value;
				if (CURS.GuiMono == null)
				{
					return;
				}
				if (!value)
				{
					IN.setZAbs(CURS.Trs, -9.5f);
				}
				CURS.GuiMono.gameObject.SetActive(CURS.gui_draw_);
			}
		}

		private static void setActive(bool f = true, bool _delete = false)
		{
			CURS.active = f;
			CURS.immediate_update = false;
			CURS.gui_draw = false;
			if (CURS.active)
			{
				CURS.delete_flag = false;
				if (!CURS.gob_enabled)
				{
					CURS.Gob = IN.CreateGob(IN._stage, "Cursor");
					CURS.gob_enabled = true;
					CURS.GuiMono = IN.CreateGob(IN._stage, "Gui").AddComponent<CURS.CursMonoBehaviourGUI>();
					CURS.GuiMono.gameObject.SetActive(CURS.gui_draw_);
					CURS.Trs = CURS.Gob.transform;
					IN.Pos(CURS.Trs, 0f, 0f, -9.5f);
					CURS.Gob.layer = LayerMask.NameToLayer(IN.gui_layer_name);
					MTRX.getPF("checked_s");
					CURS.Md = MeshDrawer.prepareMeshRenderer(CURS.Gob, MTRX.MIicon.getMtr(BLEND.NORMAL, -1), 0f, 4000, null, false, false);
					CURS.Md.Col = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
					CURS.Mrd = CURS.Gob.GetComponent<MeshRenderer>();
					CURS.Gob.AddComponent<ValotileRenderer>().Init(CURS.Md, CURS.Mrd, true);
					CURS.alpha = (float)(CURS.use_follow_mouse ? 1 : 0);
				}
				CURS.resetMousePos();
				if (CURS.use_follow_mouse)
				{
					CURS.fineMouse2();
				}
				else
				{
					CURS.Gob.SetActive(true);
				}
			}
			else
			{
				CURS.t_reserve_fine = 0f;
				if (CURS.use_follow_mouse)
				{
					Cursor.visible = true;
					if (_delete && CURS.gob_enabled)
					{
						CURS.deleteGameObject();
					}
					else if (CURS.Mrd != null)
					{
						CURS.Gob.SetActive(false);
					}
				}
				else if (_delete)
				{
					CURS.delete_flag = true;
				}
			}
			CURS.fine("", false);
		}

		public static void resetMousePos()
		{
			if (!CURS.gob_enabled)
			{
				return;
			}
			if (CURS.use_follow_mouse)
			{
				if (CURS.Cam == null)
				{
					CURS.Cam = IN.getGUICamera();
				}
				Vector2 vector = IN.MouseWorld;
				IN.Pos(CURS.Trs, vector.x, vector.y, -9.5f);
			}
		}

		public static void destruct()
		{
			CURS.setActive(false, false);
			CURS.deleteGameObject();
		}

		public static void Set(string _categ, string _curs)
		{
			if (!CURS.isActive())
			{
				return;
			}
			CursCategory cursCategory = (TX.noe(_categ) ? CURS.defaultCategory : CURS.getCC(_categ, true));
			if (cursCategory == null)
			{
				X.de("CURS.Set::不明なカテゴリー: " + _categ, null);
				return;
			}
			CursKind cursKind = CURS.getCK(_curs, CURS.current_family, false);
			if (cursKind == null)
			{
				cursKind = CURS.AKind[0];
				_curs = cursKind.key;
			}
			int i = 0;
			int num = 0;
			CursStack cursStack2;
			while (i < CURS.stack_len)
			{
				CursStack cursStack = CURS.AStack[i];
				if (cursStack == null)
				{
					CURS.stack_len = i;
					break;
				}
				if (cursStack.categ_key == _categ)
				{
					if (cursStack.curs_key != _curs)
					{
						num++;
						i++;
					}
					else
					{
						if (i == 0)
						{
							return;
						}
						X.shiftEmpty<CursStack>(CURS.AStack, 1, i, -1);
						CURS.stack_len--;
					}
				}
				else
				{
					if (cursStack.priority < cursCategory.priority)
					{
						cursStack2 = new CursStack(cursKind, cursCategory);
						i = X.Mx(i - num, 0);
						X.unshiftEmpty<CursStack>(CURS.AStack, cursStack2, i, 1, -1);
						if (i >= CURS.AStack.Length)
						{
							Array.Resize<CursStack>(ref CURS.AStack, i + 16);
						}
						CURS.stack_len++;
						if (i == 0)
						{
							CURS.fine("", false);
						}
						return;
					}
					if (cursStack.priority == cursCategory.priority)
					{
						num++;
					}
					i++;
				}
			}
			cursStack2 = new CursStack(cursKind, cursCategory);
			i = CURS.stack_len - num;
			if (i >= CURS.AStack.Length)
			{
				Array.Resize<CursStack>(ref CURS.AStack, i + 16);
			}
			X.unshiftEmpty<CursStack>(CURS.AStack, cursStack2, i, 1, -1);
			CURS.stack_len++;
			if (CURS.stack_len >= CURS.AStack.Length)
			{
				Array.Resize<CursStack>(ref CURS.AStack, CURS.stack_len + 16);
			}
			if (i == 0)
			{
				CURS.fine("", false);
			}
		}

		public static void Rem(string _categ = "", string _curs = "")
		{
			if (CURS.AStack == null)
			{
				return;
			}
			CursStack cursStack = ((CURS.stack_len > 0) ? CURS.AStack[0] : null);
			bool flag = false;
			if (_categ == "")
			{
				X.ALLN<CursStack>(CURS.AStack);
				CURS.stack_len = 0;
				flag = true;
			}
			else
			{
				cursStack = null;
				int num = CURS.stack_len;
				for (int i = num - 1; i >= 0; i--)
				{
					CursStack cursStack2 = CURS.AStack[i];
					if (cursStack2.categ_key == _categ && (!(_curs != "") || !(cursStack2.curs_key != _curs)))
					{
						if (num == 1)
						{
							cursStack = cursStack2;
						}
						X.shiftEmpty<CursStack>(CURS.AStack, 1, i, -1);
						CURS.stack_len--;
						if (i == 0)
						{
							flag = true;
						}
					}
				}
			}
			if (cursStack != null)
			{
				CursCategory categ = cursStack.Categ;
				if (categ != null && CURS.follow_mouse != 2)
				{
					categ.defineAnimation(SlideAnim.SLIDEANIM.IMMEDIATE);
				}
			}
			if (flag)
			{
				CURS.fine("", false);
			}
		}

		public static void stopFollow2()
		{
			if (CURS.follow_mouse == 2)
			{
				CURS.follow_mouse = 1;
			}
		}

		public static void Pt(string _categ, Vector2 _Pt, Transform TrsBase = null, bool immediate = false)
		{
			CURS.Loc(_categ, _Pt.x, _Pt.y, TrsBase, immediate);
		}

		public static void Loc(string _categ, float _x, float _y, Transform TrsBase = null, bool immediate = false)
		{
			if (CURS.follow_mouse == 2 && _categ.IndexOf("HOVER") >= 0)
			{
				immediate = true;
			}
			CursCategory cursCategory = ((_categ == "") ? CURS.defaultCategory : CURS.getCC(_categ, true));
			if (cursCategory == null)
			{
				X.de("CURS.Set::不明なカテゴリー:" + _categ, null);
				return;
			}
			cursCategory.defineLoc(_x, _y, TrsBase, immediate ? 0 : CURS.T_CURSMOVE);
			if (CURS.active && CURS.stack_len > 0 && CURS.active && CURS.AStack[0].Categ == cursCategory)
			{
				CURS.t_reserve_fine = X.Mx(CURS.t_reserve_fine, cursCategory.defineAnimation(immediate ? SlideAnim.SLIDEANIM.IMMEDIATE : SlideAnim.SLIDEANIM.NORMAL));
				if (CURS.t_reserve_fine > 1f && CURS.follow_mouse == 2)
				{
					CURS.follow_mouse = 1;
					CURS.fineMouse2();
				}
			}
		}

		private static void LimitVib(string _categ, AIM a)
		{
			if (!CURS.active || CURS.follow_mouse == 2)
			{
				return;
			}
			CursCategory cursCategory = ((_categ == "") ? CURS.defaultCategory : CURS.getCC(_categ, true));
			if (cursCategory == null)
			{
				X.de("CURS.Set::不明なカテゴリー:" + _categ, null);
				return;
			}
			if (CURS.stack_len > 0 && CURS.AStack[0].Categ == cursCategory)
			{
				CURS.t_reserve_fine = X.Mx(CURS.t_reserve_fine, cursCategory.defineVibrateAnimation(a));
				if (CURS.t_reserve_fine > 1f && CURS.follow_mouse == 2)
				{
					CURS.follow_mouse = 1;
					CURS.fineMouse2();
				}
			}
		}

		public static void killLoc(string _categ)
		{
			if (CURS.AStack == null)
			{
				return;
			}
			CursCategory cursCategory = ((_categ == "") ? CURS.defaultCategory : CURS.getCC(_categ, true));
			if (cursCategory == null)
			{
				X.de("CURS.Set::不明なカテゴリー: " + _categ, null);
				return;
			}
			cursCategory.defineLoc(-1000f, -1000f, null, -1);
		}

		public static void fine(string if_category_is = "", bool anim_immediate = false)
		{
			if (CURS.AStack == null || CURS.stack_len == 0)
			{
				CURS.need_fine = false;
				CURS.t_reserve_fine = 0f;
				CURS.immediate_update = false;
				CURS.gui_draw = false;
				if (CURS.use_follow_mouse)
				{
					Cursor.visible = true;
					try
					{
						CURS.Gob.SetActive(false);
					}
					catch
					{
					}
				}
				return;
			}
			if (!CURS.active)
			{
				CURS.t_reserve_fine = 0f;
				CURS.immediate_update = false;
				CURS.need_fine = true;
				CURS.gui_draw = false;
				if (CURS.use_follow_mouse)
				{
					try
					{
						CURS.fineMouse2();
					}
					catch
					{
					}
				}
				return;
			}
			CursStack cursStack = CURS.AStack[0];
			if (if_category_is != "" && cursStack.categ_key != if_category_is)
			{
				return;
			}
			CURS.t_reserve_fine = X.Mx(CURS.t_reserve_fine, cursStack.Categ.defineAnimation(anim_immediate ? SlideAnim.SLIDEANIM.IMMEDIATE : SlideAnim.SLIDEANIM.NORMAL));
			CURS.fineMouse2();
			CURS.need_fine = false;
			if (CURS.follow_mouse != 2)
			{
				float mouse_scale = CURS.mouse_scale;
				CURS.Md.RotaMesh((float)(-(float)cursStack.CK.cx) * mouse_scale, (float)cursStack.CK.cy * mouse_scale, mouse_scale, mouse_scale, 0f, cursStack.PMesh, false, false);
				CURS.Md.updateForMeshRenderer(false);
			}
		}

		public static void fineMouse2()
		{
			if (!CURS.active || CURS.AStack == null || CURS.stack_len == 0 || CURS.Md == null)
			{
				Cursor.visible = true;
				CURS.gui_draw = false;
				if (CURS.Mrd != null)
				{
					CURS.Gob.SetActive(false);
				}
				return;
			}
			if (CURS.follow_mouse == 2)
			{
				if (CURS.Cam == null)
				{
					CURS.Cam = IN.getGUICamera();
				}
				float mouse_scale = CURS.mouse_scale;
				CursStack cursStack = CURS.AStack[0];
				if (cursStack.Categ.mvx != -1000f)
				{
					cursStack.Categ.defineAnimation(SlideAnim.SLIDEANIM.IMMEDIATE);
					cursStack.Categ.mvx = -1000f;
				}
				Cursor.visible = false;
				PxlMeshDrawer pmesh = cursStack.PMesh;
				int num;
				Vector3[] rawVerticeArray = pmesh.getRawVerticeArray(out num);
				if (num == 4)
				{
					Vector2[] rawUvArray = pmesh.getRawUvArray(out num);
					PxlPose pPose = pmesh.SourceFrame.pPose;
					int num2 = (int)(rawVerticeArray[1].x * 64f) + pPose.width / 2;
					int num3 = -(int)(rawVerticeArray[1].y * 64f) + pPose.height / 2;
					float num4 = (float)(cursStack.CK.cx + pPose.width / 2);
					int num5 = cursStack.CK.cy + pPose.height / 2;
					CURS.pivot_shift_x = (-num4 + (float)num2) * mouse_scale;
					CURS.pivot_shift_y = (float)(-(float)num5 + num3) * mouse_scale;
					CURS.gui_draw = true;
					CURS.RcPos.width = (rawVerticeArray[2].x - rawVerticeArray[0].x) * mouse_scale * 64f;
					CURS.RcPos.height = (rawVerticeArray[2].y - rawVerticeArray[0].y) * mouse_scale * 64f;
					CURS.RcTexCoords.Set(rawUvArray[0].x, rawUvArray[0].y, rawUvArray[2].x - rawUvArray[0].x, rawUvArray[2].y - rawUvArray[0].y);
					CURS.Gob.SetActive(false);
				}
				else
				{
					CURS.gui_draw = false;
					CURS.Gob.SetActive(true);
					Vector2 vector = IN.MouseWorld;
					IN.Pos(CURS.Trs, vector.x, vector.y, -9.5f);
				}
				CURS.immediate_update = true;
				CURS.t_reserve_fine = 0f;
				return;
			}
			if (CURS.follow_mouse == 1)
			{
				CURS.immediate_update = false;
				CURS.Gob.SetActive(true);
				CURS.gui_draw = false;
				Cursor.visible = false;
				return;
			}
			CURS.gui_draw = false;
			CURS.Gob.SetActive(true);
		}

		public static void run(float fcnt)
		{
			if (!CURS.gob_enabled)
			{
				return;
			}
			int num = -1;
			bool flag = false;
			if (CURS.use_follow_mouse)
			{
				if (CURS.t_reserve_fine == 0f && (CURS.Gob.activeSelf || CURS.gui_draw))
				{
					if (CURS.Cam == null)
					{
						CURS.Cam = IN.getGUICamera();
					}
					Vector2 vector = IN.MouseWorld;
					if (CURS.follow_mouse == 2 || IN.use_mouse)
					{
						if (CURS.Shake != null && fcnt != 0f && CURS.Shake.update(vector))
						{
							flag = true;
						}
						if (CURS.follow_mouse < 2 || flag)
						{
							CURS.follow_mouse = 2;
							CURS.fineMouse2();
						}
						else if (!CURS.gui_draw)
						{
							IN.Pos(CURS.Trs, vector.x, vector.y, -9.5f);
						}
					}
				}
			}
			else
			{
				num = CURS.stack_len;
				float num2 = CURS.alpha;
				if ((!CURS.active || num == 0) && num2 > 0f)
				{
					num2 = X.Mx(0f, num2 - 0.025f * fcnt);
					if (num2 == 0f)
					{
						if (CURS.delete_flag)
						{
							CURS.deleteGameObject();
							return;
						}
						CURS.Gob.SetActive(false);
					}
				}
				if (CURS.active && num > 0 && num2 < 1f)
				{
					if (!CURS.Gob.activeSelf)
					{
						CURS.Gob.SetActive(true);
					}
					num2 = X.Mn(1f, num2 + 0.038f * fcnt);
				}
				CURS.alpha = num2;
				if (CURS.Shake != null)
				{
					CURS.Shake.update(CURS.Trs.position);
				}
			}
			if (CURS.t_reserve_fine > 0f)
			{
				if (num < 0)
				{
					num = CURS.stack_len;
				}
				if (num == 0)
				{
					CURS.t_reserve_fine = 0f;
					return;
				}
				CURS.t_reserve_fine -= fcnt;
				if (!CURS.AStack[0].Categ.refineAnimDep(null))
				{
					CURS.t_reserve_fine = 0f;
				}
			}
		}

		public static bool isCategoryActive(CursCategory CC)
		{
			return CURS.active && CURS.stack_len > 0 && CURS.AStack[0].Categ == CC;
		}

		public static Transform currentMoveBaseObject()
		{
			if (!CURS.active || CURS.stack_len <= 0)
			{
				return null;
			}
			return CURS.getBaseTransform();
		}

		public static bool isCursBaseObject(Transform Trs)
		{
			return CURS.active && CURS.stack_len > 0 && CURS.getBaseTransform() == Trs;
		}

		public static void reserveFineFrames(int i)
		{
			CURS.t_reserve_fine = X.Mx((float)i, CURS.t_reserve_fine);
			if (CURS.t_reserve_fine > 1f && CURS.follow_mouse == 2)
			{
				CURS.follow_mouse = 1;
				CURS.fineMouse2();
			}
		}

		public static bool isActive()
		{
			return CURS.active;
		}

		public static float getCursX()
		{
			if (!CURS.gob_enabled)
			{
				return -1000f;
			}
			if (CURS.active && CURS.stack_len > 0)
			{
				CursStack cursStack = CURS.AStack[0];
				if (cursStack.Categ.mvx != -1000f)
				{
					return cursStack.Categ.getAnimationDep().x;
				}
			}
			return CURS.Trs.localPosition.x;
		}

		public static float getCursY()
		{
			if (!CURS.gob_enabled)
			{
				return -1000f;
			}
			if (CURS.active && CURS.stack_len > 0)
			{
				CursStack cursStack = CURS.AStack[0];
				if (cursStack.Categ.mvx != -1000f)
				{
					return cursStack.Categ.getAnimationDep().y;
				}
			}
			return CURS.Trs.localPosition.y;
		}

		public static Transform getBaseTransform()
		{
			return CURS.Trs;
		}

		public static bool use_follow_mouse
		{
			get
			{
				return CURS.follow_mouse > 0;
			}
		}

		public static bool use_shake_checker
		{
			get
			{
				return CURS.Shake != null;
			}
			set
			{
				if (!value && CURS.Shake != null)
				{
					CURS.Shake = null;
				}
				if (value && CURS.Shake == null)
				{
					CURS.Shake = new ShakeChecker();
				}
			}
		}

		public static float mouse_scale
		{
			get
			{
				return CURS.mouse_base_scale * (float)((CURS.Shake != null && CURS.Shake.shaked) ? 3 : 1);
			}
		}

		public static float alpha
		{
			get
			{
				if (CURS.Md == null)
				{
					return 0f;
				}
				return (float)CURS.Md.Col.a / 255f;
			}
			set
			{
				if (CURS.Md == null)
				{
					return;
				}
				CURS.Md.Col.a = (byte)X.MMX(0f, value * 255f, 255f);
			}
		}

		public static int shake_checker_threshold
		{
			set
			{
				if (CURS.Shake != null)
				{
					CURS.Shake.threshold_pixel = value;
				}
			}
		}

		public static bool is_following_to_mouse
		{
			get
			{
				return CURS.follow_mouse == 2;
			}
		}

		public static void focusOnBtn(string categ, Transform _aBtn, bool hovered)
		{
			if (!CURS.active)
			{
				return;
			}
			CURS.Set(categ, "select");
			CURS.Loc(categ, -1.40625f, -0.171875f, _aBtn, hovered && CURS.follow_mouse == 2);
		}

		public static void focusOnBtn(aBtn _aBtn, bool hovered)
		{
			CURS.focusOnBtn(_aBtn.focus_curs_category, _aBtn, hovered);
		}

		public static void focusOnBtn(string categ, aBtn _aBtn, bool hovered)
		{
			if (!CURS.active)
			{
				return;
			}
			ButtonSkin skin = _aBtn.get_Skin();
			if (skin == null)
			{
				CURS.focusOnBtn(categ, _aBtn.transform, hovered);
				return;
			}
			CURS.Set(categ, "select");
			CURS.Loc(categ, skin.curs_level_x * skin.swidth / 2f * 0.015625f, skin.curs_level_y * skin.sheight / 2f * 0.015625f, _aBtn.transform, hovered && CURS.follow_mouse == 2);
		}

		public static void limitVib(aBtn _aBtn, AIM a = AIM.L)
		{
			if (_aBtn == null)
			{
				CURS.limitVib(a);
				return;
			}
			CURS.LimitVib(_aBtn.focus_curs_category, a);
		}

		public static void limitVib(AIM a = AIM.L)
		{
			if (CURS.AStack == null || CURS.stack_len == 0)
			{
				return;
			}
			CURS.LimitVib(CURS.AStack[0].categ_key, a);
		}

		public static Camera Cam;

		private static CursStack[] AStack;

		private static int stack_len = 0;

		private static CursKind[] AKind;

		private static CursCategory[] ACateg;

		private static CursCategory defaultCategory;

		public static string curs_dir = "CURS/";

		private static GameObject Gob;

		private static bool gob_enabled;

		private static CURS.CursMonoBehaviourGUI GuiMono;

		private static Transform Trs;

		private static MeshDrawer Md;

		private static MeshRenderer Mrd;

		private static int follow_mouse = 0;

		private static float t_reserve_fine = 0f;

		public static int mxw = 0;

		public static int mxh = 0;

		private static bool need_fine = false;

		public static readonly int T_CURSMOVE = 7;

		private static string current_family = "";

		public static Flagger Active;

		private static bool active;

		private static bool delete_flag;

		public static ShakeChecker Shake = null;

		public static float mouse_base_scale = 1f;

		public static bool immediate_update = false;

		public static string default_btn_hover_categ = "HOVER";

		public static string default_btn_hover_curs;

		public const float Z_CURS = -9.5f;

		public static bool gui_draw_ = false;

		private static Rect RcTexCoords = default(Rect);

		private static Rect RcPos = default(Rect);

		private static float pivot_shift_x;

		private static float pivot_shift_y;

		public sealed class CursMonoBehaviourGUI : MonoBehaviour
		{
			public void OnGUI()
			{
				if (Event.current.type != EventType.Repaint)
				{
					return;
				}
				CURS.RcPos.x = IN.Mouse.x + CURS.pivot_shift_x;
				CURS.RcPos.y = (float)Screen.height - IN.Mouse.y + CURS.pivot_shift_y;
				GUI.DrawTextureWithTexCoords(CURS.RcPos, MTRX.MIicon.Tx, CURS.RcTexCoords);
			}
		}
	}
}
