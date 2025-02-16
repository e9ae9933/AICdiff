using System;
using System.Collections.Generic;
using PixelLiner;
using UnityEngine;
using UnityEngine.InputSystem;
using XX;

namespace nel
{
	public class SandINBE : MonoBehaviour
	{
		public void Start()
		{
			this.Col = new C32();
			UIPictureBase.VALIDATION = true;
			BetobetoManager.SvTexture.force_load_immediately = true;
		}

		public void Update()
		{
			if (this.t == 0 && MTR.preparedT)
			{
				MTR.initGPxls(true);
				IN.FlgUiUse.Add("_");
				this.t = 1;
			}
			if (this.t == 1 && PxlsLoader.isLoadCompletedAll())
			{
				MTR.prepareShaderInstance();
				BetobetoManager.initBetobetoManager();
				this.t = 2;
				GameObject gameObject = IN.CreateGob(base.gameObject, "-Container");
				GameObject gameObject2 = IN.CreateGob(gameObject, "-Frame");
				this.UiP = new SandINBE.UIPictureTemp(this, gameObject.transform, delegate(UIPictureBase _This, Material _Mtr)
				{
					_This.GobMeshHolder = IN.CreateGob(_This.Gob, "-UiP");
					MeshDrawer meshDrawer2 = MeshDrawer.prepareMeshRenderer(_This.GobMeshHolder, _Mtr, 0f, -1, null, false, false);
					this.Mrd = _This.GobMeshHolder.GetComponent<MeshRenderer>();
					return meshDrawer2;
				});
				this.BetoMng = BetobetoManager.GetManager("noel");
				this.UiP.Gob.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
				MeshDrawer meshDrawer = MeshDrawer.prepareMeshRenderer(gameObject2, MTRX.MtrMeshNormal, 0f, -1, null, false, false);
				meshDrawer.Col = C32.d2c(4287927444U);
				meshDrawer.Box(0f, 0f, 340f, IN.h, 2f, false);
				meshDrawer.updateForMeshRenderer(false);
				this.PosPict = new Vector2((-IN.wh + 220f) * 0.015625f, 0f);
				IN.PosP(gameObject.transform, this.PosPict.x * 64f, 0f, 0f);
				this.Ds = IN.CreateGob(null, "-Ds").AddComponent<Designer>();
				this.Ds.gameObject.layer = IN.gui_layer;
				this.Ds.WH(IN.w * 0.55f, IN.h - 40f);
				IN.PosP(this.Ds.transform, IN.wh - this.Ds.get_swidth_px() * 0.5f - 20f, 0f, 0f);
				this.Ds.margin_in_lr = 15f;
				this.Ds.margin_in_tb = 10f;
				this.Ds.bgcol = C32.d2c(4283650899U);
				this.Ds.init();
				int num = 50;
				string[] array = (this.Aemot_titles = new string[num]);
				for (int i = 0; i < num; i++)
				{
					string[] array2 = array;
					int num2 = i;
					UIEMOT uiemot = (UIEMOT)i;
					array2[num2] = uiemot.ToString().ToLower();
				}
				this.EmotBtn = this.Ds.addButton(new DsnDataButton
				{
					title = array[0],
					name = "emot",
					w = this.Ds.use_w - 5f,
					h = 25f,
					fnDown = new FnBtnBindings(this.fnShowEmotPopup)
				});
				List<string> list = new List<string>();
				for (uint num3 = 1U; num3 < 1073741825U; num3 <<= 1)
				{
					try
					{
						UIPictureBase.EMSTATE emstate = (UIPictureBase.EMSTATE)num3;
						if (emstate != UIPictureBase.EMSTATE.NORMAL)
						{
							string text = emstate.ToString().ToLower();
							if (!REG.match(text, REG.RegAllNumber))
							{
								list.Add(text);
							}
						}
					}
					catch
					{
					}
				}
				this.Ds.Br().addChecks(new DsnDataChecks
				{
					name = "emstate",
					keysL = list,
					w = 150f,
					h = 20f,
					clms = 4,
					margin_w = 4,
					margin_h = 1,
					scale = 1f,
					fnClick = new FnBtnBindings(this.fnClickEmState)
				});
				this.Ds.Br();
				List<string> list2 = new List<string>();
				for (uint num4 = 1U; num4 < 32U; num4 <<= 1)
				{
					try
					{
						UIPictureBase.EMSTATE_ADD emstate_ADD = (UIPictureBase.EMSTATE_ADD)num4;
						if (emstate_ADD != UIPictureBase.EMSTATE_ADD.NORMAL)
						{
							string text2 = emstate_ADD.ToString().ToLower();
							if (!REG.match(text2, REG.RegAllNumber))
							{
								list2.Add(text2);
							}
						}
					}
					catch
					{
					}
				}
				this.Ds.addChecks(new DsnDataChecks
				{
					name = "emstateadd",
					keysL = list2,
					w = 150f,
					h = 20f,
					clms = 4,
					margin_w = 4,
					margin_h = 1,
					scale = 1f,
					fnClick = new FnBtnBindings(this.fnClickEmStateAdd)
				});
				this.Ds.Br();
				this.Ds.Br().addP(new DsnDataP("Fill ", false), false);
				this.MeterFill = this.Ds.addButtonT<aBtnMeter>(new DsnDataButton
				{
					name = "fill_level",
					w = 120f,
					h = 30f
				});
				this.MeterFill.initMeter(0f, 100f, 5f, 50f, 120f);
				this.Ds.Br().addP(new DsnDataP("Scale", false), false);
				this.MeterScale = this.Ds.addButtonT<aBtnMeter>(new DsnDataButton
				{
					name = "scale",
					w = 120f,
					h = 30f
				});
				this.MeterScale.initMeter(10f, 100f, 10f, 100f, 120f);
				this.Ds.Br().addP(new DsnDataP("Jump ", false), false);
				this.MeterJump = this.Ds.addButtonT<aBtnMeter>(new DsnDataButton
				{
					name = "betojump",
					w = 120f,
					h = 30f
				});
				this.MeterJump.initMeter(0f, 100f, 5f, 20f, 120f);
				this.Bc = this.Ds.Br().addButtonT<aBtnColorCell>(new DsnDataButton
				{
					name = "col1",
					w = 110f,
					h = 32f
				});
				this.Bc.use_alpha = true;
				this.Bc.setValue(C32.d2c(uint.MaxValue));
				this.Bc2 = this.Ds.addButtonT<aBtnColorCell>(new DsnDataButton
				{
					name = "col2",
					w = 110f,
					h = 32f
				});
				this.Bc2.use_alpha = true;
				this.Bc2.setValue(C32.d2c(4287795295U));
				this.Ds.addNumCounterT<aBtnNumCounter>(new DsnDataNumCounter
				{
					name = "counter",
					minval = 0,
					maxval = 200
				});
				this.Ds.Br();
				List<string> list3 = new List<string>(4);
				for (int j = 0; j < 7; j++)
				{
					list3.Add(((BetoInfo.TYPE)j).ToString().ToLower());
				}
				this.Ds.Br().addRadio(new DsnDataRadio
				{
					name = "betotype",
					keys = list3.ToArray(),
					w = 150f,
					h = 22f,
					clms = 3,
					margin_w = 4,
					margin_h = 1,
					scale = 1f
				});
				this.Ds.Br().addButtonT<aBtn>(new DsnDataButton
				{
					name = "paint",
					title = "Paint",
					w = 180f,
					h = 25f,
					fnClick = new FnBtnBindings(this.fnClickPaintBtn)
				});
				this.Ds.addButtonT<aBtn>(new DsnDataButton
				{
					name = "clear",
					title = "Clear",
					w = 180f,
					h = 25f,
					fnClick = new FnBtnBindings(this.fnClickPaintBtn)
				});
				this.Ds.Br();
				this.Ds.addP(new DsnDataP("", false)
				{
					text = "sensitive:"
				}, false);
				Designer ds = this.Ds;
				DsnDataRadio dsnDataRadio = new DsnDataRadio();
				dsnDataRadio.name = "sensitive";
				dsnDataRadio.keys = new string[] { "0", "1", "2" };
				dsnDataRadio.w = 30f;
				dsnDataRadio.h = 20f;
				dsnDataRadio.clms = 3;
				dsnDataRadio.margin_w = 0;
				dsnDataRadio.fnChanged = delegate(BtnContainerRadio<aBtn> BCon, int pre, int cur)
				{
					X.sensitive_level = (byte)cur;
					return true;
				};
				dsnDataRadio.def = (int)X.sensitive_level;
				ds.addRadio(dsnDataRadio);
				this.Ds.XSh(30f).addChecks(new DsnDataChecks
				{
					def = 0,
					w = 35f,
					h = 20f,
					keys = new string[] { "R" },
					fnClick = delegate(aBtn B)
					{
						this.UiP.is_position_right_ = B.isChecked();
						this.fineEmo1(true);
						return true;
					}
				});
				this.Ds.addP(new DsnDataP("", false)
				{
					text = "Shift+F9 to reload bodies",
					TxCol = MTRX.ColWhite
				}, false);
				this.fineEmo1(false);
			}
			if (this.t >= 2)
			{
				this.UiP.run(1f, 1f, false);
				SpineViewerNel.updateTexture();
				if (IN.getKD(Key.F9, 1))
				{
					this.UiP.reloadUiScript(true);
					X.dl("UI Picture Reloaded.", null, false, false);
					this.fineEmo1(true);
				}
			}
		}

		private void LateUpdate()
		{
			if (this.t >= 2 && this.UiP != null)
			{
				this.UiP.runPost();
			}
		}

		private bool fnShowEmotPopup(aBtn B)
		{
			if (this.EdMenu == null)
			{
				this.EdMenu = new BtnMenu<aBtn>("m2de_menu", 300f, 20f, 0);
				this.EdMenu.clms = 3;
				int num = this.Aemot_titles.Length;
				for (int i = 0; i < num; i++)
				{
					this.EdMenu.Make(this.Aemot_titles[i], "");
				}
				this.EdMenu.addSelectedFn(new BtnMenu<aBtn>.FnMenuSelectedBindings(this.fnSelectedMenu));
				aBtn aBtn = this.EdMenu.Get(B.get_Skin().title);
				if (aBtn != null)
				{
					aBtn.SetChecked(true, true);
				}
			}
			this.EdMenu.showMouse(0);
			return true;
		}

		private bool fnSelectedMenu(BtnMenu<aBtn> Menu, int selected, string selected_title)
		{
			if (this.EmotBtn != null)
			{
				aBtn aBtn = this.EdMenu.Get(this.EmotBtn.get_Skin().title);
				if (aBtn != null)
				{
					aBtn.SetChecked(false, true);
				}
				aBtn = this.EdMenu.Get(selected_title);
				if (aBtn != null)
				{
					aBtn.SetChecked(true, true);
				}
				this.EmotBtn.setSkinTitle(selected_title);
			}
			FEnum<UIEMOT>.TryParse(selected_title.ToUpper(), out this.cur_emot, true);
			this.fineEmo1(false);
			return true;
		}

		private bool fnClickEmState(aBtn B)
		{
			UIPictureBase.EMSTATE emstate;
			FEnum<UIPictureBase.EMSTATE>.TryParse(B.title.ToUpper(), out emstate, true);
			if (B.isChecked())
			{
				this.cur_state |= emstate;
			}
			else
			{
				this.cur_state &= ~emstate;
			}
			this.fineEmo1(false);
			return true;
		}

		private bool fnClickEmStateAdd(aBtn B)
		{
			UIPictureBase.EMSTATE_ADD emstate_ADD;
			FEnum<UIPictureBase.EMSTATE_ADD>.TryParse(B.title.ToUpper(), out emstate_ADD, true);
			if (B.isChecked())
			{
				this.cur_statea |= emstate_ADD;
			}
			else
			{
				this.cur_statea &= ~emstate_ADD;
			}
			this.fineEmo1(false);
			return true;
		}

		private bool fnClickPaintBtn(aBtn B)
		{
			string title = B.title;
			if (title != null)
			{
				if (!(title == "Paint"))
				{
					if (title == "Clear")
					{
						this.BetoMng.cleanAll(true);
						this.dirty_level = 0f;
						this.fineEmo1(true);
					}
				}
				else
				{
					float value = this.MeterFill.getValue();
					Color32 c = this.Col.Set(this.Bc.getColor()).C;
					Color32 c2 = this.Col.Set(this.Bc2.getColor()).C;
					float value2 = this.MeterScale.getValue();
					if (value > 0f)
					{
						BetoInfo betoInfo = new BetoInfo(1000f, c, c2, value, (BetoInfo.TYPE)X.NmI(this.Ds.getValue("betotype"), 0, false, false), value2 / 100f, 0, this.MeterJump.getValue() / 100f);
						this.BetoMng.Check(betoInfo, true, true);
						this.dirty_level += value;
					}
					X.dl(string.Concat(new string[]
					{
						"fill(level)=",
						value.ToString(),
						"%, C1=",
						C32.Color32ToCodeText(c),
						", C2=",
						C32.Color32ToCodeText(c2),
						", scl=",
						value2.ToString(),
						"%, jump=",
						this.MeterJump.getValue().ToString(),
						"%"
					}), null, false, false);
					this.fineEmo1(false);
				}
			}
			return true;
		}

		private void fineEmo1(bool force = false)
		{
			this.UiP.changeEmotIn(this.cur_emot, this.cur_state, null, (UIPictureFader.UIP_RES)0);
		}

		private int t;

		private bool draw_check = true;

		private C32 Col;

		private Material Mtr;

		private RenderTexture Tx;

		private SandINBE.UIPictureTemp UiP;

		private Vector2 PosPict;

		private MeshDrawer MdT;

		private GameObject GobMesh;

		private aBtnColorCell Bc;

		private aBtnColorCell Bc2;

		private aBtn EmotBtn;

		private MeshRenderer Mrd;

		private Designer Ds;

		private BtnMenu<aBtn> EdMenu;

		private string[] Aemot_titles;

		private UIEMOT cur_emot;

		private UIPictureBase.EMSTATE cur_state;

		private UIPictureBase.EMSTATE_ADD cur_statea;

		private aBtnMeter MeterFill;

		private aBtnMeter MeterJump;

		private aBtnMeter MeterScale;

		private BetobetoManager BetoMng;

		private float dirty_level;

		private sealed class UIPictureTemp : UIPictureBase
		{
			public UIPictureTemp(SandINBE Con, Transform TrsParent, Func<UIPictureBase, Material, MeshDrawer> FnMdCreate)
				: base(TrsParent, FnMdCreate)
			{
				this.Container = Con;
				this.Mf = this.GobMeshHolder.GetComponent<MeshFilter>();
			}

			public override void fineMeshTarget(Mesh Ms)
			{
				this.Mf.sharedMesh = Ms;
			}

			public override Vector2 getCenterUPos(bool consider_effect_shift = false)
			{
				return this.Container.PosPict;
			}

			public override float is_position_right
			{
				get
				{
					return (float)(this.is_position_right_ ? 1 : 0);
				}
			}

			public override UIPictureBase.EMSTATE_ADD getAdditionalState(bool force_fine = false)
			{
				UIPictureBase.EMSTATE_ADD pre_sta = this.pre_sta;
				this.pre_sta = UIPictureBase.EMSTATE_ADD.NORMAL;
				if (X.SENSITIVE)
				{
					this.pre_sta |= UIPictureBase.EMSTATE_ADD.SENSITIVE;
				}
				else
				{
					this.pre_sta &= ~UIPictureBase.EMSTATE_ADD.SENSITIVE;
				}
				if (X.sensitive_level >= 2)
				{
					this.pre_sta |= UIPictureBase.EMSTATE_ADD.SP_SENSITIVE;
				}
				else
				{
					this.pre_sta &= ~UIPictureBase.EMSTATE_ADD.SP_SENSITIVE;
				}
				return this.pre_sta | this.Container.cur_statea;
			}

			protected override void changeEmotFinalize(Material _Mtr, UIEMOT cemot, UIPictureBase.EMSTATE st, UIPictureFader.UIP_RES res)
			{
				if (this.Container.Mrd.sharedMaterials[0] != _Mtr)
				{
					this.Container.Mrd.sharedMaterials = new Material[] { _Mtr };
				}
				base.changeEmotFinalize(_Mtr, cemot, st, res);
			}

			private MeshFilter Mf;

			private SandINBE Container;

			public bool is_position_right_;
		}
	}
}
