using System;
using System.Collections.Generic;
using PixelLiner;
using XX;

namespace nel.mgm.dojo
{
	public class DjUI : UiBoxDesignerFamily
	{
		public float out_wh
		{
			get
			{
				return this.out_w * 0.5f;
			}
		}

		public float desc_w
		{
			get
			{
				return this.out_w - this.main_w - 12f;
			}
		}

		public float confirm_h
		{
			get
			{
				return 100f;
			}
		}

		public float confirm_y
		{
			get
			{
				return this.main_y - this.main_h * 0.5f - 20f - this.confirm_h * 0.5f;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			this.base_z = -0.25f;
			this.auto_deactive_gameobject = false;
		}

		public void activateMenu()
		{
			if (this.DsMoney == null)
			{
				this.DsMoney = this.CreateT<UiBoxMoney>("Money", 0f, 0f, 30f, 30f, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.DsMain = base.Create("Main", 0f, this.main_y, this.main_w, this.main_h, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.DsMain.init();
				this.BConSkill = this.DsMain.addRadioT<aBtnDJ>(new DsnDataRadio
				{
					all_function_same = true,
					name = "skill",
					skin = "normal",
					value_return_name = true,
					w = this.DsMain.use_w - 16f - 8f,
					h = 32f,
					margin_h = 0,
					margin_w = 0,
					navi_loop = 2,
					fnGenerateKeys = new FnGenerateRemakeKeys(this.fnGenerateSkillKeys),
					fnHover = new FnBtnBindings(this.fnHoverMainSkill),
					fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnClickMainSkill),
					fnMakingAfter = delegate(BtnContainer<aBtn> BCon, aBtn B)
					{
						(B.get_Skin() as ButtonSkinDJRow).initSkill(this.DJ);
						return true;
					},
					SCA = new ScrollAppend(239, this.DsMain.use_w, this.DsMain.use_h - 10f, 4f, 6f, 0)
				});
				this.DsDesc = base.Create("Desc", 0f, 30f, this.desc_w, this.main_h, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.DsDesc.item_margin_y_px = 0f;
				this.DsDesc.margin_in_tb = 8f;
				this.DsDesc.margin_in_lr = 32f;
				this.DsDesc.init();
				this.DsDesc.addImg(new DsnDataImg
				{
					name = "skill_img",
					FnDrawInFIB = new FillImageBlock.FnDrawInFIB(this.fnDrawSkillIcon),
					swidth = this.DsDesc.use_w,
					sheight = 145f,
					MI = MTR.MIiconL
				});
				this.DsDesc.Br();
				this.DsDesc.addP(new DsnDataP("", false)
				{
					name = "skill_desc",
					text = " ",
					TxCol = C32.d2c(4283780170U),
					swidth = this.DsDesc.use_w,
					sheight = this.DsDesc.use_h - 70f - this.DsDesc.item_margin_y_px * 2f,
					size = 14f,
					lineSpacing = 1.07f,
					alignx = ALIGN.LEFT,
					html = true,
					text_margin_x = 4f,
					text_margin_y = 0f
				}, false);
				this.DsDesc.Br().addHr(new DsnDataHr
				{
					margin_b = 4f,
					margin_t = 4f,
					Col = C32.d2c(4283780170U)
				});
				this.DsDesc.Br();
				this.DsDesc.addP(new DsnDataP("", false)
				{
					name = "gen_desc",
					text = " ",
					TxCol = C32.d2c(4283780170U),
					swidth = this.DsDesc.use_w,
					sheight = this.DsDesc.use_h,
					size = 12.5f,
					lineSpacing = 1.07f,
					alignx = ALIGN.LEFT,
					html = true,
					text_margin_x = 10f,
					text_margin_y = 0f
				}, false);
				this.DsConfirm = base.Create("Confirm", 0f, 30f, this.out_w, this.confirm_h, -1, 30f, UiBoxDesignerFamily.MASKTYPE.BOX);
				this.DsConfirm.getBox().frametype = UiBox.FRAMETYPE.ONELINE;
				this.DsConfirm.margin_in_lr = 30f;
				this.DsConfirm.margin_in_tb = 8f;
				this.DsConfirm.init();
				this.DsConfirm.addP(new DsnDataP("", false)
				{
					name = "confirm_p",
					text = " ",
					TxCol = C32.d2c(4283780170U),
					swidth = this.DsConfirm.use_w,
					sheight = this.DsConfirm.use_h,
					alignx = ALIGN.LEFT,
					text_margin_x = 0f,
					text_margin_y = 0f
				}, false);
				for (int i = 0; i < 2; i++)
				{
					aBtnNel aBtnNel = IN.CreateGobGUI(this.DsConfirm.gameObject, "-OK").AddComponent<aBtnNel>();
					aBtnNel.w = 220f;
					aBtnNel.h = 30f;
					if (i == 0)
					{
						this.BCfOK = aBtnNel;
						aBtnNel.title = "OK";
						aBtnNel.initializeSkin("normal", "");
						aBtnNel.setSkinTitle(TX.Get("Mgm_dojo_confirm_btn_ok", ""));
						aBtnNel.click_snd = "dojo_game_init";
					}
					else
					{
						this.BCfCancel = aBtnNel;
						aBtnNel.title = "Cancel";
						aBtnNel.initializeSkin("normal", "");
						aBtnNel.setSkinTitle(TX.Get("Mgm_dojo_confirm_btn_cancel", ""));
						aBtnNel.click_snd = "cancel";
						aBtnNel.setNaviR(this.BCfOK, true, true);
						aBtnNel.setNaviL(this.BCfOK, true, true);
					}
					aBtnNel.addClickFn(new FnBtnBindings(this.FnClickConfirmBtn));
					aBtnNel.setAlpha(0f);
					IN.PosP(aBtnNel.transform, this.DsConfirm.w * 0.5f - 20f - (aBtnNel.w + 26f) * (1.5f - (float)i), 0f, -0.01f);
				}
			}
			else if (this.need_create_skill_rows)
			{
				this.BConSkill.RemakeT<aBtnDJ>(null, "");
			}
			this.need_reveal = true;
			this.DsMain.rowRemakeCheck(false);
			this.activate();
			this.DsMain.posSetDA(-this.out_wh + this.main_w * 0.5f, this.main_y, 2, 40f, false);
			this.DsDesc.posSetDA(this.out_wh - this.desc_w * 0.5f, this.main_y, 0, 40f, false);
			this.DsConfirm.posSetDA(0f, this.confirm_y, 1, 20f, false);
			this.need_create_skill_rows = false;
			this.result = null;
			this.changeState(DjUI.STATE.MAIN);
		}

		private void changeState(DjUI.STATE _stt)
		{
			IN.clearPushDown(true);
			this.stt = _stt;
			this.t_state = 0f;
			if (this.stt == DjUI.STATE.CONFIRM)
			{
				this.DsMain.hide();
				this.BCfOK.gameObject.SetActive(true);
				this.BCfCancel.gameObject.SetActive(true);
				this.BCfOK.setAlpha(0f);
				this.BCfCancel.setAlpha(0f);
				this.BCfOK.Select(true);
				return;
			}
			if (this.stt == DjUI.STATE.MAIN)
			{
				this.BConSkill.setValue(-1, false);
				this.BCfOK.gameObject.SetActive(false);
				this.BCfCancel.gameObject.SetActive(false);
				this.DsMain.bind();
				this.BConSkill.Get(this.pre_selected_skill).Select(true);
				return;
			}
			this.deactivate(false);
			this.DsConfirm.hide();
		}

		private void fnGenerateSkillKeys(BtnContainerBasic BCon, List<string> Adest)
		{
			Adest.Add("_tuto");
			foreach (KeyValuePair<string, DjHkdsGenerator> keyValuePair in this.DJ.GM.HK.getWholeGeneratorDictionary())
			{
				PrSkill prSkill = SkillManager.Get(keyValuePair.Key);
				if (prSkill != null && (prSkill.always_enable || prSkill.visible || keyValuePair.Value.term_enabled))
				{
					Adest.Add(keyValuePair.Key);
				}
			}
			Adest.Sort((string A, string B) => this.FnSortSkillKeys(A, B));
		}

		private int FnSortSkillKeys(string sa, string sb)
		{
			if (sa == "_tuto" || sb == "_tuto")
			{
				if (sa == sb)
				{
					return 0;
				}
				if (!(sa == "_tuto"))
				{
					return 1;
				}
				return -1;
			}
			else
			{
				DjHkdsGenerator gen = this.DJ.GM.HK.GetGen(sa);
				DjHkdsGenerator gen2 = this.DJ.GM.HK.GetGen(sb);
				PrSkill prSkill = SkillManager.Get(sa);
				PrSkill prSkill2 = SkillManager.Get(sb);
				if (prSkill == null || prSkill2 == null)
				{
					if (prSkill == prSkill2)
					{
						return 0;
					}
					if (prSkill != null)
					{
						return 1;
					}
					return -1;
				}
				else if (prSkill.visible != prSkill2.visible)
				{
					if (!prSkill.visible)
					{
						return -1;
					}
					return 1;
				}
				else
				{
					int grade = gen.grade;
					int grade2 = gen2.grade;
					if (grade == grade2)
					{
						return (int)(prSkill.id - prSkill2.id);
					}
					if (grade >= grade2)
					{
						return 1;
					}
					return -1;
				}
			}
		}

		private bool fnHoverMainSkill(aBtn B)
		{
			if (this.stt != DjUI.STATE.MAIN)
			{
				return false;
			}
			this.pre_selected_skill = B.title;
			FillImageBlock fillImageBlock = this.DsDesc.Get("skill_img", false) as FillImageBlock;
			if (fillImageBlock != null)
			{
				fillImageBlock.redraw_flag = true;
			}
			FillBlock fillBlock = this.DsDesc.Get("skill_desc", false) as FillBlock;
			FillBlock fillBlock2 = this.DsDesc.Get("gen_desc", false) as FillBlock;
			FillBlock fillBlock3 = this.DsConfirm.Get("confirm_p", false) as FillBlock;
			ButtonSkinDJRow buttonSkinDJRow = B.get_Skin() as ButtonSkinDJRow;
			DjSaveData.SD data = COOK.Mgm.Dojo.GetData(B.title);
			if (B.title == "_tuto")
			{
				if (fillBlock != null)
				{
					fillBlock.text_content = TX.Get("Mgm_dojo_tutorial_desc", "");
				}
				if (fillBlock3 != null)
				{
					fillBlock3.text_content = TX.Get("Mgm_dojo_confirm_tutorial", "");
				}
			}
			else
			{
				PrSkill sk = buttonSkinDJRow.Sk;
				if (sk != null && fillBlock != null)
				{
					fillBlock.text_content = sk.descript;
				}
				if (fillBlock3 != null)
				{
					fillBlock3.text_content = (sk.visible ? TX.Get("Mgm_dojo_confirm_learned", "") : ((data.play_count > 0) ? TX.Get("Mgm_dojo_confirm_not_learned", "") : TX.Get("Mgm_dojo_confirm_not_learned_first", "")));
				}
			}
			if (fillBlock2 != null)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					if (buttonSkinDJRow.grade >= 0)
					{
						using (STB stb2 = TX.PopBld(null, 0))
						{
							NelItem.getGradeMeshTxTo(stb2, buttonSkinDJRow.grade, 0, 52);
							stb.AddTxA("Mgm_dojo_generator_desc", false).TxRpl(stb2);
							stb.Ret("\n");
						}
					}
					stb.AddTxA((data.play_count == 0 || data.minimum_miss >= 90) ? "Mgm_dojo_generator_count_not_played" : "Mgm_dojo_generator_count", false);
					stb.TxRpl((int)data.play_count).TxRpl((int)data.win_count).TxRpl((int)data.minimum_miss);
					fillBlock2.Txt(stb);
				}
			}
			return true;
		}

		private bool fnClickMainSkill(BtnContainer<aBtn> BCon, int pre, int cur)
		{
			if (this.stt != DjUI.STATE.MAIN)
			{
				return false;
			}
			if (cur < 0)
			{
				return true;
			}
			aBtn aBtn = BCon.Get(cur);
			ButtonSkinDJRow buttonSkinDJRow = aBtn.get_Skin() as ButtonSkinDJRow;
			if (aBtn.isLocked())
			{
				SND.Ui.play("locked", false);
				if ((long)buttonSkinDJRow.price > (long)((ulong)CoinStorage.getCount(CoinStorage.CTYPE.GOLD)))
				{
					this.DsMoney.initEnough();
				}
				return false;
			}
			this.pre_selected_skill = aBtn.title;
			FillBlock fillBlock = this.DsConfirm.Get("confirm_p", false) as FillBlock;
			using (STB stb = TX.PopBld(null, 0))
			{
				if (this.pre_selected_skill == "_tuto")
				{
					stb.AddTxA("Mgm_dojo_confirm_choose_tutorial", false);
				}
				else
				{
					stb.AddTxA("Mgm_dojo_confirm_choose", false).TxRpl(buttonSkinDJRow.Sk.title);
				}
				fillBlock.Txt(stb);
			}
			this.changeState(DjUI.STATE.CONFIRM);
			SND.Ui.play("enter_dojo", false);
			return true;
		}

		public bool fnDrawSkillIcon(MeshDrawer Md, FillImageBlock FI, float alpha, ref bool update_meshdrawer)
		{
			update_meshdrawer = true;
			PrSkill prSkill = ((this.pre_selected_skill != null) ? SkillManager.Get(this.pre_selected_skill) : null);
			if (prSkill != null)
			{
				PxlFrame pf = prSkill.getPF();
				Md.RotaPF(0f, 0f, 1f, 1f, 0f, pf, false, false, false, uint.MaxValue, false, 0);
			}
			return true;
		}

		private bool FnClickConfirmBtn(aBtn B)
		{
			if (this.stt != DjUI.STATE.CONFIRM)
			{
				return false;
			}
			if (B.title == "OK")
			{
				this.result = this.pre_selected_skill;
				this.DsMain.posSetDA(-this.out_wh + this.main_w * 0.5f, this.main_y, 2, this.out_w * 0.6f, false);
				this.DsDesc.posSetDA(this.out_wh - this.desc_w * 0.5f, this.main_y, 0, this.out_w * 0.6f, false);
				this.DsConfirm.posSetDA(0f, this.confirm_y, 1, 160f, false);
				this.changeState(DjUI.STATE.HIDING);
			}
			else if (B.title == "Cancel")
			{
				this.changeState(DjUI.STATE.MAIN);
			}
			return true;
		}

		internal void initG(string _skill_key)
		{
			aBtn aBtn = this.BConSkill.Get(_skill_key);
			if (aBtn != null)
			{
				ButtonSkinDJRow buttonSkinDJRow = aBtn.get_Skin() as ButtonSkinDJRow;
				this.DsMoney.reduceCount(buttonSkinDJRow.price);
				DjSaveData.SD data = COOK.Mgm.Dojo.GetData(_skill_key);
				if (data.play_count == 0)
				{
					data.minimum_miss = 99;
					if (_skill_key != "_tuto")
					{
						this.need_create_skill_rows = true;
					}
				}
				data.play_count = (ushort)X.Mn(9999, (int)(data.play_count + 1));
				COOK.Mgm.Dojo.WriteData(_skill_key, data);
			}
		}

		public override bool runIRD(float fcnt)
		{
			if (!base.runIRD(fcnt))
			{
				return false;
			}
			this.t_state += fcnt;
			if (this.stt == DjUI.STATE.MAIN)
			{
				if (IN.isCancelPD())
				{
					SND.Ui.play("cancel", false);
					this.result = "_cancel";
					this.changeState(DjUI.STATE.HIDING);
				}
				if (this.need_reveal && this.pre_selected_skill != null && this.BConSkill != null && this.BConSkill.OuterScrollBox != null)
				{
					this.need_reveal = false;
					aBtn aBtn = this.BConSkill.Get(this.pre_selected_skill);
					if (aBtn != null)
					{
						this.BConSkill.OuterScrollBox.getScrollBox().reveal(aBtn, false, REVEALTYPE.CENTER);
					}
				}
			}
			else if (this.stt == DjUI.STATE.CONFIRM)
			{
				float num = X.NIL(0.5f, 1f, this.t_state, 25f);
				this.BCfCancel.setAlpha(num);
				this.BCfOK.setAlpha(num);
				if (IN.isCancelPD())
				{
					SND.Ui.play("cancel", false);
					this.changeState(DjUI.STATE.MAIN);
				}
			}
			return true;
		}

		public MgmDojo DJ;

		private UiBoxMoney DsMoney;

		private UiBoxDesigner DsMain;

		private UiBoxDesigner DsDesc;

		private UiBoxDesigner DsConfirm;

		public bool need_create_skill_rows;

		public bool need_reveal;

		public string result;

		private BtnContainerRadio<aBtn> BConSkill;

		private float t_state;

		private DjUI.STATE stt;

		private aBtn BCfOK;

		private aBtn BCfCancel;

		public float out_w = IN.w * 0.7f;

		public float main_w = IN.w * 0.4f;

		public float main_h = IN.h * 0.48f;

		public float main_y = 80f;

		public const string tutorial_key = "_tuto";

		public const string canceled_result = "_cancel";

		private string pre_selected_skill = "_tuto";

		private enum STATE
		{
			HIDING,
			MAIN,
			CONFIRM
		}
	}
}
