using System;
using System.Collections.Generic;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class UiReelManager : MonoBehaviourAutoRun, IValotileSetable
	{
		private float ik_opening_y
		{
			get
			{
				return (float)((this.AReel.Count == 0) ? 20 : 140);
			}
		}

		public UiReelManager InitUiReelManager(ReelManager _Con)
		{
			base.gameObject.layer = IN.gui_layer;
			IN.setZAbs(base.transform, -4.7f);
			this.Con = _Con;
			this.AReelCon = this.Con.getReelVector();
			this.AReel = new List<ReelExecuter>(8);
			this.material_fine_flag = false;
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			if (nelM2DBase != null)
			{
				if (!this.isUiGMActive())
				{
					this.basex_u = nelM2DBase.ui_shift_x * 0.015625f;
				}
				float value = nelM2DBase.IMNG.StmNoel.getValue(RCP.RPI_EFFECT.REEL_SPEED);
				this.reel_speed_rcp = value;
			}
			this.MdBg = MeshDrawer.prepareMeshRenderer(base.gameObject, MTRX.MtrMeshNormal, 0f, -1, null, false, false);
			this.MrdBack = base.gameObject.GetComponent<MeshRenderer>();
			this.MdBg.base_z = 0.1f;
			GameObject gameObject = IN.CreateGob(base.gameObject, "-Main");
			this.MdMain = MeshDrawer.prepareMeshRenderer(gameObject, MTRX.MtrMeshNormal, 0f, -1, null, false, false);
			this.MdMain.InitSubMeshContainer(0);
			this.MrdMain = gameObject.GetComponent<MeshRenderer>();
			this.MdMain.connectRendererToTriMulti(this.MrdMain);
			this.AUseMaterial = new List<UiReelManager.UMtr>(1)
			{
				new UiReelManager.UMtr(this.MdMain.getMaterial(), false)
			};
			this.EF = new Effect<EffectItem>(base.gameObject, 240);
			this.EF.initEffect("DesignerEf", Camera.main, new Effect<EffectItem>.FnCreateEffectItem(EffectItem.fnCreateOne), EFCON_TYPE.NORMAL);
			this.EF.setLayer(base.gameObject.layer, base.gameObject.layer);
			this.EF.topBaseZ = -0.33f;
			this.EF.bottomBaseZ = 0.25f;
			if (X.DEBUGSTABILIZE_DRAW)
			{
				MultiMeshRenderer multiMeshRenderer = IN.CreateGob(base.gameObject, "-EF").AddComponent<MultiMeshRenderer>();
				this.EF.assignMMRDForMeshDrawerContainer(multiMeshRenderer);
			}
			else
			{
				this.EfBindT = new CameraRenderBinderFunc("ReelEF-top", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
				{
					if (!this.EF.no_graphic_render)
					{
						return true;
					}
					this.EF.RenderOneSide(false, JCon.CameraProjectionTransformed, Cam, false);
					return true;
				}, this.EF.topBaseZ + -4.7f);
				this.EfBindB = new CameraRenderBinderFunc("ReelEF-bottom", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
				{
					if (!this.EF.no_graphic_render)
					{
						return true;
					}
					this.EF.RenderOneSide(true, JCon.CameraProjectionTransformed, Cam, false);
					return true;
				}, this.EF.bottomBaseZ + -4.7f);
				CameraBidingsBehaviour.UiBind.assignPostRenderFunc(this.EfBindT);
				CameraBidingsBehaviour.UiBind.assignPostRenderFunc(this.EfBindB);
				this.EF.no_graphic_render = (this.EF.draw_gl_only = true);
			}
			this.recreateUi(false);
			this.start_reel_count = this.AReel.Count;
			this.bgaf = 0;
			this.SetXy();
			this.FlgMainStabilize = new Flagger(delegate(FlaggerT<string> F)
			{
				ValotileRenderer.RecheckUse(this.MdMain, this.MrdMain, ref this.ValotMain, false);
				if (this.TxKD != null)
				{
					this.TxKD.use_valotile = false;
				}
			}, delegate(FlaggerT<string> F)
			{
				ValotileRenderer.RecheckUse(this.MdMain, this.MrdMain, ref this.ValotMain, true);
				if (this.TxKD != null)
				{
					this.TxKD.use_valotile = true;
				}
			});
			this.FlgMainStabilize.FnDeactivate(this.FlgMainStabilize);
			this.M2D.FlgAreaTitleHide.Add("REEL");
			UIStatus.FlgStatusHide.Add("REEL");
			return this;
		}

		public bool use_valotile
		{
			get
			{
				return this.ValotMain != null && this.ValotMain.enabled;
			}
			set
			{
				value = !X.DEBUGSTABILIZE_DRAW && value;
				ValotileRenderer.RecheckUse(this.MdBg, this.MrdBack, ref this.ValotBack, value);
				if (this.MdAdd != null)
				{
					ValotileRenderer.RecheckUse(this.MdAdd, this.MrdAdd, ref this.ValotAdd, value);
				}
			}
		}

		public MeshDrawer getAddMd()
		{
			if (this.MdAdd == null)
			{
				GameObject gameObject = IN.CreateGob(base.gameObject, "Add");
				this.MdAdd = MeshDrawer.prepareMeshRenderer(gameObject, MTRX.MtrMeshAdd, -0.15f, -1, null, false, false);
				this.MrdAdd = gameObject.GetComponent<MeshRenderer>();
				ValotileRenderer.RecheckUse(this.MdAdd, this.MrdAdd, ref this.ValotAdd, this.use_valotile);
			}
			return this.MdAdd;
		}

		private void disposeMaterial()
		{
			if (this.AUseMaterial != null)
			{
				for (int i = this.AUseMaterial.Count - 1; i >= 0; i--)
				{
					UiReelManager.UMtr umtr = this.AUseMaterial[i];
					if (umtr.copied)
					{
						IN.DestroyOne(umtr.Mtr);
					}
				}
				this.AUseMaterial.Clear();
				this.AUseMaterial.Add(new UiReelManager.UMtr(MTRX.MtrMeshNormal, false));
			}
		}

		private void recreateUi(bool clear_material = false)
		{
			this.Con.destructExecuterReels(false);
			this.AReel.Clear();
			int num = X.Mx(this.AReelCon.Count - 8, 0);
			int num2 = X.Mn(this.AReelCon.Count, num + 8);
			for (int i = num; i < num2; i++)
			{
				this.AReel.Add(this.AReelCon[i]);
			}
			if (clear_material)
			{
				this.disposeMaterial();
			}
			for (int j = this.AReel.Count - 1; j >= 0; j--)
			{
				this.AReel[j].initUi(this, j);
			}
		}

		public void SetXy()
		{
			IN.Pos2Abs(base.transform, this.x_u + this.basex_u, this.y_u + this.basey_u);
			if (this.TxKD != null)
			{
				this.TxKD.transform.position = new Vector3((IN.wh - 48f) * 0.015625f, (-IN.hh + 33f) * 0.015625f, -5.0499997f);
			}
		}

		private void SetXyPx(float px, float py)
		{
			this.x_u = px * 0.015625f;
			this.y_u = py * 0.015625f;
			this.SetXy();
		}

		public int getMaterialId(Material Mtr, bool copied = false)
		{
			int num = -1;
			for (int i = this.AUseMaterial.Count - 1; i >= 0; i--)
			{
				if (this.AUseMaterial[i].Mtr == Mtr)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				num = this.AUseMaterial.Count;
				this.MdMain.chooseSubMesh(num, false, true);
				this.AUseMaterial.Add(new UiReelManager.UMtr(Mtr, copied));
				this.MdMain.setMaterial(Mtr, false);
				this.material_fine_flag = true;
			}
			return num;
		}

		public int getMaterialId(Shader Shd, ReelExecuter.ETYPE etype)
		{
			STB stb = TX.PopBld(null, 0);
			stb += Shd.name;
			stb += "__";
			stb += FEnum<ReelExecuter.ETYPE>.ToStr(etype);
			int count = this.AUseMaterial.Count;
			int num = -1;
			for (int i = 0; i < count; i++)
			{
				UiReelManager.UMtr umtr = this.AUseMaterial[i];
				if (umtr.Mtr.shader == Shd && stb.Equals(umtr.name))
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				num = this.AUseMaterial.Count;
				this.MdMain.chooseSubMesh(num, false, true);
				Material material = MTRX.newMtr(Shd);
				material.name = stb.ToString();
				this.AUseMaterial.Add(new UiReelManager.UMtr(material, true));
				this.MdMain.setMaterial(material, false);
				this.MdMain.initForImgAndTexture(MTRX.MIicon);
				this.material_fine_flag = true;
				ReelExecuter.fineMaterialCol(material, etype);
			}
			TX.ReleaseBld(stb);
			return num;
		}

		public UiReelManager changeState(ReelManager.MSTATE _mstt, bool no_clear_push_down = false)
		{
			if (this.tstate >= 0)
			{
				this.tstate = 0;
			}
			if (_mstt == ReelManager.MSTATE.OPENING && this.reel_overhold)
			{
				_mstt = ReelManager.MSTATE.DETAIL_REEL;
			}
			if (this.MdAdd != null)
			{
				this.MdAdd.clear(false, false);
			}
			if (!no_clear_push_down)
			{
				IN.clearPushDown(true);
			}
			ReelManager.MSTATE mstate = this.mstt;
			this.mstt = _mstt;
			this.rotate_decide_id = -2;
			ReelExecuter.ESTATE estate = ReelExecuter.ESTATE.NONE;
			this.BConEfReel = null;
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			int num2 = 0;
			bool flag3 = false;
			switch (mstate)
			{
			case ReelManager.MSTATE.DETAIL_REEL:
				if (this.M2D != null)
				{
					this.M2D.FlagValotStabilize.Rem("REELMNG");
				}
				if (this.BxL != null)
				{
					this.BxL.deactivate();
					this.BxDesc.deactivate();
					this.BxDialog.deactivate();
					this.LTabBar.LrInput(false);
				}
				this.reel_effect_hold = 0;
				if (!this.need_refine_pos_on_detail)
				{
					flag2 = true;
				}
				this.FlgMainStabilize.Rem("_STATE");
				break;
			case ReelManager.MSTATE.DETAIL_RECIPEBOOK:
				if (this.RecipeBook != null && this.RecipeBook.isActive())
				{
					this.RecipeBook.deactivate(false);
				}
				this.RecipeBook = null;
				this.FlgMainStabilize.Rem("_STATE");
				break;
			case ReelManager.MSTATE.REMOVE_REELS:
				this.reel_speed_rcp = 0f;
				this.Con.destructExecuterReels(true);
				this.AReel.Clear();
				break;
			}
			bool flag4 = this.isRotatingState();
			if (flag4 && this.Con.getCurrentItemReel() != null)
			{
				if (this.ReelIK == null)
				{
					this.ReelIK = new ReelExecuter(this.Con, ReelExecuter.ETYPE.ITEMKIND);
				}
				this.ReelIK.initUi(this, -1);
				this.ReelIK.initState(ReelExecuter.ESTATE.OPENING, 0);
			}
			switch (this.mstt)
			{
			case ReelManager.MSTATE.DETAIL_REEL:
				if (this.M2D != null)
				{
					this.M2D.FlagValotStabilize.Add("REELMNG");
				}
				flag = (this.need_txkd_fine = true);
				if (mstate != ReelManager.MSTATE.DETAIL_RECIPEBOOK)
				{
					this.rltab = (this.reel_list_back_to_opening ? UiReelManager.RLTAB.ITEM : UiReelManager.RLTAB.EFFECT);
				}
				if (mstate == ReelManager.MSTATE.PREPARE)
				{
					this.Adiscard_ir = null;
				}
				UIBase.FlgUiEffectDisable.Add("REEL");
				this.FlgMainStabilize.Add("_STATE");
				this.initDetailBox();
				this.BxL.activate();
				this.BxDesc.activate();
				this.selectDetailBox();
				this.LTabBar.LrInput(true);
				break;
			case ReelManager.MSTATE.DETAIL_RECIPEBOOK:
				flag = (this.need_txkd_fine = true);
				if (this.RecipeBook != null)
				{
					IN.DestroyOne(this.RecipeBook.gameObject);
				}
				UIBase.FlgUiEffectDisable.Add("REEL");
				this.FlgMainStabilize.Add("_STATE");
				this.RecipeBook = new GameObject("RBK" + IN.totalframe.ToString()).AddComponent<UiFieldGuide>();
				SND.Ui.play("tool_hand_init", false);
				IN.setZAbs(this.RecipeBook.transform, base.transform.position.z - 0.125f);
				break;
			case ReelManager.MSTATE.NIGHTCON_ADDING:
			case ReelManager.MSTATE.REMOVE_REELS:
			case ReelManager.MSTATE.PREPARE:
				if (this.ReelIK != null)
				{
					this.ReelIK.deactivate();
				}
				this.reel_list_listl_index = -2;
				this.SelectedIR = (this.FocusIR = null);
				this.need_refine_pos_on_detail = false;
				estate = ReelExecuter.ESTATE.NORMAL;
				break;
			case ReelManager.MSTATE.OPENING:
			case ReelManager.MSTATE.OPENING_AUTO:
				NelItem.fineNameLocalizedWhole();
				estate = ReelExecuter.ESTATE.OPENING;
				this.decide_delay = -30f;
				flag2 = false;
				this.reel_list_listl_index = -2;
				this.SelectedIR = (this.FocusIR = null);
				if (mstate != ReelManager.MSTATE.OPENING && mstate != ReelManager.MSTATE.OPENING_AUTO)
				{
					flag3 = true;
				}
				if (this.mstt == ReelManager.MSTATE.OPENING_AUTO)
				{
					num2 = 20;
					num = 2;
					this.decide_delay = 60f;
				}
				else if (mstate == ReelManager.MSTATE.PREPARE)
				{
					num2 = 30;
					num = 14;
				}
				else
				{
					num = 10;
				}
				break;
			default:
				estate = ReelExecuter.ESTATE.NORMAL;
				break;
			}
			int count = this.AReel.Count;
			float num3 = 0.1f;
			float num4 = this.reel_speed_rcp;
			float num5 = X.Mn(num4, num3 * 2f);
			num4 -= num5;
			float num6 = num5 / (num3 * 2f);
			for (int i = 0; i < count; i++)
			{
				ReelExecuter reelExecuter = this.AReel[i];
				reelExecuter.activate();
				if (estate != ReelExecuter.ESTATE.NONE)
				{
					reelExecuter.initState(estate, num2);
				}
				if (flag4)
				{
					reelExecuter.rotateInit();
				}
				num5 = X.Mn(num4, num3);
				num4 -= num5;
				reelExecuter.fineSpeed(num5 / num3);
				num2 += num;
			}
			if (this.AIrbox != null)
			{
				this.repositIRBox(flag3);
			}
			if (flag4)
			{
				this.rotate_decide_id = -1;
				this.ReelIK.initReelContent(this.Con.getCurrentItemReel());
				this.ReelIK.fineSpeed(num6);
				this.initSnd("slot_loop");
			}
			else if (this.Snd != null)
			{
				this.Snd.Dispose();
				this.Snd = null;
			}
			if (this.mstt >= ReelManager.MSTATE.PREPARE || this.mstt == ReelManager.MSTATE.DETAIL_REEL)
			{
				if (this.TxKD == null)
				{
					TextRenderer textRenderer = (this.TxKD = IN.CreateGob(base.gameObject, "-KD").AddComponent<TextRenderer>());
					this.TxKD.html_mode = true;
					this.TxKD.MakeValot(null, null);
					this.TxKD.use_valotile = !this.FlgMainStabilize.isActive();
					textRenderer.Align(ALIGN.RIGHT).AlignY(ALIGNY.BOTTOM).Alpha(1f)
						.Size(16f)
						.Col(4293190884U)
						.BorderCol(4282004532U);
					this.SetXy();
				}
				this.need_txkd_fine = true;
			}
			if (flag)
			{
				for (int j = this.AReel.Count - 1; j >= 0; j--)
				{
					this.AReel[j].deactivate();
				}
				if (this.ReelIK != null)
				{
					this.ReelIK.deactivate();
				}
			}
			else if (!flag2)
			{
				this.fineItemPosition(0, -1, false);
			}
			CURS.Omazinai();
			return this;
		}

		private void initSnd(string cue_key = null)
		{
			if (!this.play_snd)
			{
				return;
			}
			if (this.Snd == null)
			{
				this.Snd = new SndPlayer("reel_manager", SndPlayer.SNDTYPE.SND);
			}
			if (cue_key != null)
			{
				this.Snd.play(cue_key, false);
			}
		}

		public void playSnd(string p)
		{
			if (!this.play_snd)
			{
				return;
			}
			SND.Ui.play(p, false);
		}

		public void PadVib(string vib_key, float l = 1f)
		{
			if (!this.play_snd)
			{
				return;
			}
			NEL.PadVib(vib_key, l);
		}

		private static int fnSortMaterial(UiReelManager.UMtr Ma, UiReelManager.UMtr Mb)
		{
			return ReelExecuter.getShaderId(Ma.Mtr.shader.name) - ReelExecuter.getShaderId(Mb.Mtr.shader.name);
		}

		public void prepareMBoxDrawer()
		{
			if (this.AIrbox == null)
			{
				List<ReelManager.ItemReelContainer> itemReelCacheVector = this.Con.getItemReelCacheVector();
				this.AIrbox = new List<ReelMBoxDrawer>(itemReelCacheVector.Count);
				int count = itemReelCacheVector.Count;
				for (int i = 0; i < count; i++)
				{
					ReelManager.ItemReelContainer itemReelContainer = itemReelCacheVector[i];
					this.AIrbox.Add(new ReelMBoxDrawer(this.Con, this, itemReelContainer, i));
				}
				this.repositIRBox(false);
			}
		}

		private float prepare_ir_y
		{
			get
			{
				return IN.hh * 0.2f;
			}
		}

		private bool irbox_prepare_use_scroll
		{
			get
			{
				return this.AIrbox.Count >= 7;
			}
		}

		private void repositIRBox(bool splice_head_deactivating = false)
		{
			if (this.AIrbox == null)
			{
				return;
			}
			int num = this.AIrbox.Count;
			switch (this.mstt)
			{
			case ReelManager.MSTATE.DETAIL_REEL:
			case ReelManager.MSTATE.DETAIL_RECIPEBOOK:
			case ReelManager.MSTATE.REMOVE_REELS:
			{
				float num2 = (float)(-(float)(num - 1) * 90) * 0.5f;
				for (int i = 0; i < num; i++)
				{
					this.AIrbox[i].position(num2 + (float)(i * 90), -IN.hh * 1.6f, false);
				}
				break;
			}
			case ReelManager.MSTATE.NIGHTCON_ADDING:
				break;
			case ReelManager.MSTATE.PREPARE:
			{
				float num3 = (float)num * 240f;
				float num4 = (float)(-(float)(num - 1)) * 240f * 0.5f;
				for (int j = 0; j < num; j++)
				{
					ReelMBoxDrawer reelMBoxDrawer = this.AIrbox[j];
					float num5;
					for (num5 = -num4 + 240f * (float)j - (this.irbox_prepare_use_scroll ? ((float)this.tstate * 2.1f) : 0f); num5 < 0f; num5 += num3)
					{
					}
					num5 = num5 % num3 + num4;
					reelMBoxDrawer.position(num5, this.prepare_ir_y, false);
				}
				return;
			}
			case ReelManager.MSTATE.OPENING:
			case ReelManager.MSTATE.OPENING_AUTO:
			{
				if (splice_head_deactivating)
				{
					int k = 0;
					while (k < num)
					{
						if (!this.AIrbox[k].isActive())
						{
							num--;
							this.AIrbox.RemoveAt(k);
						}
						else
						{
							k++;
						}
					}
				}
				if (num <= 0)
				{
					return;
				}
				float num6 = (float)(-(float)(num - 2) * 60) * 0.5f;
				this.AIrbox[0].position(0f, IN.hh * 0.7f, false);
				for (int l = 1; l < num; l++)
				{
					this.AIrbox[l].position(num6 + (float)((l - 1) * 60), IN.hh * 1.6f, false);
				}
				return;
			}
			default:
				return;
			}
		}

		private void scrollIRBoxPrepare(int fcnt)
		{
			if (this.AIrbox == null || this.mstt != ReelManager.MSTATE.PREPARE)
			{
				return;
			}
			int count = this.AIrbox.Count;
			float num = (float)count * 240f;
			float num2 = (float)(-(float)(count - 1)) * 240f * 0.5f;
			for (int i = 0; i < count; i++)
			{
				ReelMBoxDrawer reelMBoxDrawer = this.AIrbox[i];
				float num3 = reelMBoxDrawer.dx - 2.1f * (float)fcnt;
				if (num3 < num2)
				{
					num3 += num;
					reelMBoxDrawer.position(num3, this.prepare_ir_y, true);
				}
				else
				{
					reelMBoxDrawer.position(num3, this.prepare_ir_y, false);
				}
			}
		}

		public void UiPictureStabilize()
		{
			UIBase.FlgUiEffectDisable.Add("REEL");
		}

		protected override bool runIRD(float fcnt)
		{
			if (this.BxConfirm != null && !this.BxConfirm.runIRD(fcnt))
			{
				this.BxConfirm = null;
			}
			int num = 1;
			bool flag = true;
			int num8;
			if (this.tstate >= 0)
			{
				switch (this.mstt)
				{
				case ReelManager.MSTATE.DETAIL_REEL:
					flag = this.tstate < 50;
					if (this.reel_effect_hold != 0 && !IN.isSubmitOn(0))
					{
						this.reel_effect_hold = 0;
					}
					if (!(this.BxConfirm != null) || !this.BxConfirm.isActive())
					{
						if (this.BxConfirm != null && this.BxConfirm.prompt_result != null)
						{
							aBtn aBtn = null;
							if (this.BxConfirm.prompt_result == "1" && this.Adiscard_ir != null)
							{
								this.Con.destructExecuterReels(false);
								this.Adiscard_ir.Sort(X.FnSortIntager);
								for (int i = this.Adiscard_ir.Count - 1; i >= 0; i--)
								{
									int num2 = this.Adiscard_ir[i];
									if (X.BTW(0f, (float)num2, (float)this.AReelCon.Count))
									{
										ReelExecuter reelExecuter = this.AReelCon[num2];
										this.AReelCon.RemoveAt(num2);
									}
								}
								this.Adiscard_ir.Clear();
								this.recreateUi(true);
								this.need_refine_pos_on_detail = true;
								this.changeState(this.first_reelopen ? ReelManager.MSTATE.OPENING : ReelManager.MSTATE.PREPARE, false);
								this.playSnd("reset_var");
							}
							else
							{
								BtnContainer<aBtn> bcon = (this.BxL.getTab("L_scr").Get("list", false) as BtnContainerRunner).BCon;
								if (this.Adiscard_ir != null && this.Adiscard_ir.Count > 0)
								{
									aBtn = bcon.Get(this.Adiscard_ir[this.Adiscard_ir.Count - 1]);
								}
								if (aBtn == null)
								{
									aBtn = bcon.Get(0);
								}
								this.BxL.bind();
							}
							this.BxConfirm.enabled = true;
							this.BxConfirm = null;
							if (aBtn != null)
							{
								aBtn.Select(true);
							}
						}
						else if (this.FocusIR != null && !IN.isUiSortPD() && !this.M2D.isRbkPD())
						{
							if (this.M2D.isRbkPD())
							{
								UiFieldGuide.NextRevealAtAwake = null;
								if (this.IRContentLastSelected != null)
								{
									int num3 = X.NmI(this.IRContentLastSelected.title, 0, false, false);
									if (X.BTW(0f, (float)num3, (float)this.FocusIR.IR.Count))
									{
										UiFieldGuide.NextRevealAtAwake = this.FocusIR.IR[num3];
									}
								}
								if (UiFieldGuide.NextRevealAtAwake == null)
								{
									ReelMBoxDrawer focusIR = this.FocusIR;
									UiFieldGuide.NextRevealAtAwake = ((focusIR != null) ? focusIR.IR : null);
								}
								this.changeState(ReelManager.MSTATE.DETAIL_RECIPEBOOK, false);
							}
							else if (this.rltab != UiReelManager.RLTAB.ITEM || IN.isL() || IN.isR() || IN.isCancel() || IN.isUiAddPD() || IN.isLTabPD() || IN.isRTabPD())
							{
								this.quitIRContentRowFocused(true);
							}
							else if (this.AIrbox.Count > 1 && (IN.isRTabPD() || IN.isLTabPD()))
							{
								int num4 = this.AIrbox.IndexOf(this.SelectedIR);
								if (num4 >= 0)
								{
									num4 = (num4 + this.AIrbox.Count + (IN.isRTabPD() ? 1 : 0) + (IN.isLTabPD() ? (-1) : 0)) % this.AIrbox.Count;
									BtnContainer<aBtn> btnContainer = this.selectDetailIR(num4);
									if (btnContainer.Length > 0)
									{
										btnContainer.Get(0).Select(true);
									}
								}
							}
						}
						else if (this.M2D.isRbkPD())
						{
							if (this.rltab == UiReelManager.RLTAB.ITEM)
							{
								ReelMBoxDrawer selectedIR = this.SelectedIR;
								UiFieldGuide.NextRevealAtAwake = ((selectedIR != null) ? selectedIR.IR : null);
							}
							this.changeState(ReelManager.MSTATE.DETAIL_RECIPEBOOK, false);
							this.IRContentLastSelected = null;
						}
						else if (IN.isUiSortPD() || IN.isCancel() || (this.discard_selection_finished && IN.isSubmitPD(1)))
						{
							bool flag2 = true;
							if (this.reel_overhold && this.Adiscard_ir != null && this.Adiscard_ir.Count > 0)
							{
								flag2 = false;
								if (this.AReelCon.Count - this.Adiscard_ir.Count <= 8)
								{
									this.BxL.hide();
									this.initReelConfirmBox();
								}
								else
								{
									this.playSnd("locked");
								}
							}
							if (flag2)
							{
								this.changeState(this.reel_list_back_to_opening ? ReelManager.MSTATE.OPENING : ReelManager.MSTATE.PREPARE, false);
								this.playSnd("cancel");
							}
						}
						else if (IN.isUiAddO() && this.reel_overhold)
						{
							if (aBtn.PreSelected != null && aBtn.PreSelected.get_Skin() is ButtonSkinNelReelInfo)
							{
								ButtonSkinNelReelInfo buttonSkinNelReelInfo = aBtn.PreSelected.get_Skin() as ButtonSkinNelReelInfo;
								if (!buttonSkinNelReelInfo.hilighted)
								{
									if (this.discard_selection_finished)
									{
										this.removeFromDiscardList(this.BConEfReel.Get(this.Adiscard_ir[0]).get_Skin() as ButtonSkinNelReelInfo, null, false);
									}
									if (!this.discard_selection_finished)
									{
										this.addToDiscardList(buttonSkinNelReelInfo, null);
									}
								}
							}
						}
						else if (IN.isUiRemO() && this.reel_overhold)
						{
							if (aBtn.PreSelected != null && aBtn.PreSelected.get_Skin() is ButtonSkinNelReelInfo)
							{
								if (IN.isUiShiftO())
								{
									if (this.Adiscard_ir != null && this.Adiscard_ir.Count > 0)
									{
										for (int j = 0; j < this.Adiscard_ir.Count; j++)
										{
											this.removeFromDiscardList(this.BConEfReel.Get(this.Adiscard_ir[j]).get_Skin() as ButtonSkinNelReelInfo, this.BConEfReel, true);
										}
									}
								}
								else
								{
									ButtonSkinNelReelInfo buttonSkinNelReelInfo2 = aBtn.PreSelected.get_Skin() as ButtonSkinNelReelInfo;
									if (buttonSkinNelReelInfo2.hilighted)
									{
										this.removeFromDiscardList(buttonSkinNelReelInfo2, null, false);
									}
								}
							}
						}
						else if (aBtn.PreSelected != null && IN.isSubmitPD(1))
						{
							aBtn.PreSelected.ExecuteOnSubmitKey();
						}
						else if (this.rltab == UiReelManager.RLTAB.ITEM && this.SelectedIR != null && (IN.isL() || IN.isR()))
						{
							this.initFocusIR(0);
						}
					}
					break;
				case ReelManager.MSTATE.DETAIL_RECIPEBOOK:
					flag = this.tstate < 50;
					if (this.RecipeBook == null || !this.RecipeBook.isActive())
					{
						int num5 = ((this.IRContentLastSelected == null) ? (-1) : X.NmI(this.IRContentLastSelected.title, -1, false, false));
						int num6 = this.reel_list_listl_index;
						this.changeState((num6 >= -1) ? ReelManager.MSTATE.DETAIL_REEL : ReelManager.MSTATE.PREPARE, false);
						if (num6 >= 0)
						{
							aBtn aBtn2 = (this.BxL.getTab("L_scr").Get("list", false) as BtnContainerRunner).BCon.Get(num6);
							if (aBtn2 != null)
							{
								aBtn2.Select(true);
								if (this.rltab == UiReelManager.RLTAB.ITEM && num5 >= 0)
								{
									this.initFocusIR(num5);
								}
							}
						}
					}
					break;
				case ReelManager.MSTATE.REMOVE_REELS:
					if (this.tstate >= 45)
					{
						int num7 = (this.tstate - 45) / 20;
						if (num7 < this.AReel.Count)
						{
							if (!this.AReel[num7].isDisappearingState())
							{
								this.AReel[num7].initState(ReelExecuter.ESTATE.DISAPPEARING, 0);
							}
						}
						else if (this.tstate - 45 - num7 * this.AReel.Count >= 40)
						{
							if (this.Con.getCurrentItemReel() == null)
							{
								this.deactivate();
								return true;
							}
							this.changeState(this.after_clearreels ? ReelManager.MSTATE.OPENING_AUTO : ReelManager.MSTATE.OPENING, false);
						}
					}
					break;
				case ReelManager.MSTATE.PREPARE:
					if (X.D && this.AIrbox != null && this.irbox_prepare_use_scroll)
					{
						this.scrollIRBoxPrepare(X.AF);
					}
					if (this.M2D.isRbkPD())
					{
						this.reel_list_listl_index = -2;
						this.changeState(ReelManager.MSTATE.DETAIL_RECIPEBOOK, false);
						this.playSnd("enter_small");
					}
					else if (IN.isUiSortPD())
					{
						this.reel_list_back_to_opening = false;
						this.changeState(ReelManager.MSTATE.DETAIL_REEL, false);
						this.playSnd("enter_small");
					}
					else if ((IN.ketteiPD(1) || IN.isMenuPD(1)) && this.Con.getCurrentItemReel() != null)
					{
						this.changeState(ReelManager.MSTATE.OPENING, false);
						this.playSnd((this.mstt == ReelManager.MSTATE.OPENING) ? "enter" : "enter_small");
					}
					else if (IN.isCancel() && this.manual_deactivatable)
					{
						this.deactivate();
						this.playSnd("cancel");
						return true;
					}
					break;
				case ReelManager.MSTATE.OPENING:
					if (this.cancelable)
					{
						if (this.rotate_decide_id < this.AReel.Count && IN.isUiSortPD())
						{
							this.reel_list_back_to_opening = true;
							this.playSnd("enter_small");
							this.changeState(ReelManager.MSTATE.DETAIL_REEL, false);
							this.initFocusIR(0);
						}
						else if (IN.isCancel() && !IN.isMenuO(0))
						{
							this.playSnd("cancel");
							if (this.rotate_decide_id >= this.AReel.Count)
							{
								if (this.progressReelStack() == null)
								{
									this.deactivate();
									return true;
								}
								if (this.AIrbox != null && this.AIrbox.Count > 0)
								{
									this.AIrbox.RemoveAt(0);
								}
								this.changeState(ReelManager.MSTATE.PREPARE, false);
							}
							else
							{
								this.changeState(ReelManager.MSTATE.PREPARE, false);
							}
						}
					}
					break;
				}
				this.tstate++;
				if (this.rotate_decide_id >= -1 && this.manipulable)
				{
					bool flag3 = IN.isMenuO(0);
					if (flag3 && IN.isMenuO(30))
					{
						num = (IN.isMenuO(60) ? 6 : 3);
					}
					if (!this.play_snd && this.mstt == ReelManager.MSTATE.OPENING_AUTO)
					{
						num = X.Mx(3, num);
					}
					if (this.decide_delay > 0f && (IN.ketteiPD(1) || IN.isMenuPD(1) || (flag3 && this.decide_delay < 8f)))
					{
						this.decide_delay = 0f;
					}
					else if (this.decide_delay != 0f)
					{
						this.decide_delay = X.VALWALK(this.decide_delay, 0f, (float)num * fcnt);
					}
					if (this.decide_delay == 0f)
					{
						bool flag4 = true;
						bool flag5 = IN.ketteiOn();
						if (this.mstt == ReelManager.MSTATE.OPENING_AUTO && this.autodecide_progressable && !flag5)
						{
							flag4 = false;
							flag5 = true;
						}
						if (flag5 || flag3)
						{
							if (this.rotate_decide_id >= this.AReel.Count)
							{
								if (this.AIrbox != null && this.AIrbox.Count > 0)
								{
									this.AIrbox[0].deactivate().position(1120f, this.ik_opening_y, false);
								}
								if (flag4)
								{
									this.playSnd("cancel");
								}
								if (this.progressReelStack() == null && (!this.after_clearreels || this.AReel.Count <= 0))
								{
									this.ReelIK.posSetA(1120f, this.ik_opening_y, 0f, this.ik_opening_y, false);
									this.deactivate();
								}
								else
								{
									this.ReelIK.posSetA(0f, this.ik_opening_y, 1120f, this.ik_opening_y, false);
									this.rotate_decide_id = -3;
									this.decide_delay = 12f;
								}
							}
							else
							{
								ReelExecuter reelExecuter2 = ((this.rotate_decide_id == -1) ? this.ReelIK : this.AReel[this.rotate_decide_id]);
								if (reelExecuter2 == null || reelExecuter2.decideRotate(this.mstt == ReelManager.MSTATE.OPENING_AUTO && this.autodecide_progressable))
								{
									this.decide_delay = (float)((flag3 || !this.play_snd) ? 6 : 15);
									if (this.rotate_decide_id == -1 && (this.mstt != ReelManager.MSTATE.OPENING_AUTO || !this.autodecide_progressable))
									{
										this.need_txkd_fine = true;
									}
									if (this.rotate_decide_id >= 0 && this.ReelIK != null)
									{
										this.ReelIK.applyEffectToIK(reelExecuter2);
									}
									num8 = this.rotate_decide_id + 1;
									this.rotate_decide_id = num8;
									if (num8 >= this.AReel.Count)
									{
										if (this.mstt == ReelManager.MSTATE.OPENING_AUTO)
										{
											this.decide_delay = (float)((!this.play_snd) ? 40 : 120);
										}
										else
										{
											this.decide_delay *= 2f;
										}
										this.need_txkd_fine = true;
										this.PadVib("reel_decide_1", 1f);
										if (this.AIrbox != null && this.AIrbox.Count > 0)
										{
											this.AIrbox[0].animOpen();
										}
										if (this.Snd != null)
										{
											this.Snd.Stop();
										}
									}
								}
							}
						}
					}
				}
				else if (this.rotate_decide_id == -3)
				{
					bool flag6 = IN.isMenuO(0);
					if (flag6 && IN.isMenuO(30))
					{
						num = (IN.isMenuO(60) ? 6 : 3);
					}
					if (!this.play_snd)
					{
						num = X.Mx(3, X.IntR((float)num * 1.5f));
					}
					if (this.decide_delay > 0f && (IN.kettei() || flag6))
					{
						this.decide_delay = 0f;
					}
					else if (this.decide_delay != 0f)
					{
						this.decide_delay = X.VALWALK(this.decide_delay, 0f, (float)num * fcnt);
					}
					if (this.decide_delay == 0f)
					{
						if (this.AIrbox != null && this.AIrbox.Count > 0)
						{
							this.AIrbox.RemoveAt(0);
						}
						if (this.Con.getCurrentItemReel() == null)
						{
							this.changeState(this.after_clearreels ? ReelManager.MSTATE.REMOVE_REELS : ReelManager.MSTATE.PREPARE, false);
						}
						else
						{
							this.changeState((this.mstt == ReelManager.MSTATE.OPENING_AUTO) ? ReelManager.MSTATE.OPENING_AUTO : ReelManager.MSTATE.OPENING, true);
						}
					}
				}
				if (!(this.TxKD != null) || !this.need_txkd_fine)
				{
					goto IL_1055;
				}
				using (STB stb = TX.PopBld(null, 0))
				{
					this.need_txkd_fine = false;
					switch (this.mstt)
					{
					case ReelManager.MSTATE.DETAIL_REEL:
						if (this.reel_overhold && this.rltab == UiReelManager.RLTAB.EFFECT)
						{
							if (this.Adiscard_ir != null && this.AReelCon.Count - this.Adiscard_ir.Count <= 8)
							{
								stb.AddTxA("KeyDesc_discard_add_submit", false);
								if (this.Adiscard_ir.Count > 0)
								{
									stb.AddTxA("KeyDesc_discard_rem", false);
								}
							}
							else
							{
								stb.AddTxA("KeyDesc_discard_add", false);
								stb.AppendTxA("KeyDesc_discard_rem", " ");
								stb.AppendTxA("KeyDesc_discard_enable_submit", " ");
							}
						}
						else
						{
							stb.AddTxA("Reel_keydesc_cancel", false);
						}
						if (this.rltab == UiReelManager.RLTAB.ITEM)
						{
							stb.AppendTxA("KeyDesc_reel_item_detail", " ");
						}
						if (this.M2D.IMNG.has_recipe_collection)
						{
							stb.AppendTxA("KD_show_catalog", "\n");
						}
						break;
					case ReelManager.MSTATE.PREPARE:
						stb.AddTxA("Reel_keydesc_prepare", false).AppendTxA("Reel_keydesc_detail", " ");
						if (this.M2D.IMNG.has_recipe_collection)
						{
							stb.AppendTxA("KD_show_catalog", "\n");
						}
						break;
					case ReelManager.MSTATE.OPENING:
						if (this.rotate_decide_id >= this.AReel.Count)
						{
							stb.AddTxA("Reel_keydesc_opening_finished", false);
						}
						else
						{
							stb.AddTxA("Reel_keydesc_opening", false);
							if (this.cancelable)
							{
								stb.AppendTxA("Reel_keydesc_detail", " ").AppendTxA("KD_cancel", " ");
							}
						}
						break;
					case ReelManager.MSTATE.OPENING_AUTO:
						if (this.play_snd && this.rotate_decide_id >= this.AReel.Count)
						{
							stb.AddTxA("Reel_keydesc_opening_finished", false);
						}
						break;
					}
					this.TxKD.Txt(stb);
					goto IL_1055;
				}
			}
			if (this.Con == null)
			{
				return false;
			}
			num8 = this.tstate - 1;
			this.tstate = num8;
			if (num8 <= -35)
			{
				this.Con.destructGob();
				return false;
			}
			IL_1055:
			this.EF.runDrawOrRedrawMesh(X.D_EF, (float)X.AF_EF, 1f);
			this.EF.setEffectMatrix(base.transform.localToWorldMatrix * ValotileRenderer.Mx_z_zero);
			if (this.material_fine_flag)
			{
				while (this.material_fine_flag)
				{
					this.material_fine_flag = false;
					this.AUseMaterial.Sort((UiReelManager.UMtr a, UiReelManager.UMtr b) => UiReelManager.fnSortMaterial(a, b));
					for (int k = this.AUseMaterial.Count - 1; k >= 0; k--)
					{
						this.MdMain.chooseSubMesh(k, false, true);
						this.MdMain.setMaterial(this.AUseMaterial[k].Mtr, false);
					}
					for (int l = this.AReel.Count - 1; l >= 0; l--)
					{
						this.AReel[l].fineMeshId();
					}
					if (this.AIrbox != null)
					{
						for (int m = this.AIrbox.Count - 1; m >= 0; m--)
						{
							this.AIrbox[m].fineMeshId();
						}
					}
					if (this.ReelIK != null)
					{
						this.ReelIK.fineMeshId();
					}
				}
			}
			if (X.D)
			{
				bool flag7 = true;
				if (this.bgaf >= 0)
				{
					if (this.bgaf < 35)
					{
						this.bgaf += X.AF;
						flag7 = true;
					}
					else
					{
						flag7 = false;
					}
				}
				else
				{
					this.bgaf -= X.AF;
				}
				if (flag7)
				{
					this.MdBg.clear(false, false);
					if (!this.no_draw_hidescreen)
					{
						this.MdBg.Col = C32.MulA(4278190080U, 0.7f * ((this.bgaf >= 0) ? X.ZLINE((float)this.bgaf, 35f) : X.ZLINE((float)(35 + this.bgaf), 35f)));
						this.MdBg.base_x = (this.MdBg.base_y = 0f);
						this.MdBg.Rect(0f, 0f, IN.w + 340f + 40f, IN.h + 40f, false);
					}
					this.MdBg.updateForMeshRenderer(false);
				}
				if (flag)
				{
					this.MdMain.clear(false, false);
					this.MdMain.chooseSubMesh(0, false, false);
					int num9 = this.AReel.Count;
					for (int n = 0; n < num9; n++)
					{
						this.AReel[n].runDraw(X.AF * num, this.MdMain, this.EF);
					}
					if (this.AIrbox != null)
					{
						num9 = this.AIrbox.Count;
						for (int num10 = 0; num10 < num9; num10++)
						{
							this.AIrbox[num10].runDraw((float)(X.AF * num), this.MdMain, this.EF);
						}
					}
					if (this.ReelIK != null)
					{
						this.ReelIK.runDraw(X.AF * num, this.MdMain, this.EF);
					}
					this.MdMain.updateForMeshRenderer(false);
				}
				if (this.redraw_mdadd)
				{
					this.redraw_mdadd = false;
					if (this.MdAdd != null)
					{
						this.MdAdd.updateForMeshRenderer(false);
					}
				}
			}
			return true;
		}

		private void initReelConfirmBox()
		{
			if (this.BxConfirm != null)
			{
				IN.DestroyOne(this.BxConfirm.gameObject);
			}
			this.BxConfirm = new GameObject(base.name + "-Confirm").AddComponent<UiWarpConfirm>();
			this.BxConfirm.enabled = false;
			this.BxConfirm.input_to_varcon = false;
			List<ReelExecuter> list = new List<ReelExecuter>(this.Adiscard_ir.Count);
			int count = this.Adiscard_ir.Count;
			for (int i = 0; i < count; i++)
			{
				list.Add(this.AReelCon[this.Adiscard_ir[i]]);
			}
			this.BxConfirm.Init(UiWarpConfirm.CTYPE.REEL_DISCARD, list, null);
			IN.Pos2(this.BxConfirm.transform, this.x_u - this.basex_u, this.y_u - this.basey_u);
			IN.setZAbs(this.BxConfirm.transform, -5.2f);
			SND.Ui.play("enter_small", false);
		}

		public void obtain(ReelExecuter Re)
		{
			int count = this.AReel.Count;
			this.AReel.Add(Re);
			Re.initUi(this, count);
			bool flag = false;
			ReelManager.MSTATE mstate = this.mstt;
			if (mstate == ReelManager.MSTATE.OBTAIN || mstate == ReelManager.MSTATE.NIGHTCON_ADDING)
			{
				Re.initState(ReelExecuter.ESTATE.OBTAIN_APPEAR, 80 + 20 * (count - 1 - this.start_reel_count));
				if (this.mstt == ReelManager.MSTATE.NIGHTCON_ADDING)
				{
					flag = true;
				}
			}
			else
			{
				Re.initState(ReelExecuter.ESTATE.NORMAL, 0);
			}
			if (flag)
			{
				this.fineItemPosition(0, -1, true);
				return;
			}
			this.fineItemPosition(count, 1, true);
		}

		private void fineItemPosition(int si = 0, int cnt = -1, bool obtain = false)
		{
			float num = -265f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			int num6 = X.Mx(1, this.AReel.Count);
			int num7 = -1;
			int num8 = -1;
			switch (this.mstt)
			{
			case ReelManager.MSTATE.OBTAIN:
				num6 = 8;
				num2 = -IN.wh * 0.23f;
				if (obtain)
				{
					num7 = 3;
					num5 = 60f;
				}
				break;
			case ReelManager.MSTATE.NIGHTCON_ADDING:
			case ReelManager.MSTATE.REMOVE_REELS:
				num6 = X.MMX(1, this.AReel.Count, 4);
				num4 = -90f;
				num2 = ((this.mstt == ReelManager.MSTATE.NIGHTCON_ADDING) ? (-IN.wh * 0.34f) : 0f) - ((num6 < this.AReel.Count) ? (num4 * 0.5f) : 0f);
				num3 = (float)((this.mstt == ReelManager.MSTATE.NIGHTCON_ADDING) ? 160 : 200);
				num = -num3 * (float)(num6 - 1) * 0.5f;
				num8 = 2;
				break;
			case ReelManager.MSTATE.PREPARE:
				num2 = -IN.wh * 0.33f;
				break;
			case ReelManager.MSTATE.OPENING:
			case ReelManager.MSTATE.OPENING_AUTO:
				num6 = X.MMX(1, this.AReel.Count, 4);
				num4 = -120f;
				num2 = -110f + ((num6 < this.AReel.Count) ? (-num4 * 0.5f) : 0f);
				if (this.ReelIK != null)
				{
					this.ReelIK.posSetA(-1120f, this.ik_opening_y, 0f, this.ik_opening_y, false);
				}
				break;
			}
			if (num3 == 0f && num6 > 1)
			{
				num3 = 530f / (float)(num6 - 1);
			}
			int num9 = ((cnt < 0) ? this.AReel.Count : (si + cnt));
			int num10 = si % num6;
			float num11 = num2;
			num2 += num4 * (float)(si / num6);
			for (int i = si; i < num9; i++)
			{
				float num12 = num + num3 * (float)num10;
				float num13 = num2;
				ReelExecuter reelExecuter = this.AReel[i];
				int num14 = num7;
				if (num8 >= 0 && i >= 8)
				{
					num12 -= (float)CAim._XD(num8, 1) * (num * 2f + (float)((i - 8 - 1) * 90));
					num13 = num11 + num4 * 0.5f + (float)CAim._YD(num8, 1) * (-num3 * 2f);
					num6 = num10 + 2;
					num14 = -1;
				}
				if (num14 != -1)
				{
					reelExecuter.posSetA(num12 - (float)CAim._XD(num14, 1) * num5, num13 - (float)CAim._YD(num14, 1) * num5, num12, num13, false);
				}
				else
				{
					reelExecuter.position(num12, num13, -1000f, -1000f, false);
				}
				if (++num10 >= num6)
				{
					num10 = 0;
					num2 += num4;
				}
			}
		}

		private float detail_left_w
		{
			get
			{
				return this.detail_bounds_w - 420f - 12f;
			}
		}

		private float detail_pos_lx
		{
			get
			{
				return -this.basex_u * 64f - this.detail_bounds_w * 0.5f + this.detail_left_w * 0.5f;
			}
		}

		private float detail_pos_rx
		{
			get
			{
				return -this.basex_u * 64f + this.detail_bounds_w * 0.5f - 210f;
			}
		}

		private void initDetailBox()
		{
			float num = this.detail_bounds_w;
			float num2 = this.detail_bounds_h;
			float detail_left_w = this.detail_left_w;
			bool flag = false;
			if (this.BxL == null)
			{
				this.BxL = IN.CreateGob(base.gameObject, "-BxL").AddComponent<UiBoxDesigner>();
				IN.setZ(this.BxL.transform, -0.34f);
				this.BxDesc = IN.CreateGob(base.gameObject, "-BxDesc").AddComponent<UiBoxDesigner>();
				IN.setZ(this.BxDesc.transform, -0.36f);
				this.BxDialog = IN.CreateGob(base.gameObject, "-BxDialog").AddComponent<UiBoxDesigner>();
				IN.setZ(this.BxDialog.transform, -0.38f);
				this.BxL.WHanim(detail_left_w, num2, true, true);
				this.BxDesc.WHanim(420f, num2, true, true);
				this.BxL.stencil_ref = (this.BxL.box_stencil_ref_mask = (this.BxDesc.stencil_ref = (this.BxDesc.box_stencil_ref_mask = (this.BxDialog.stencil_ref = (this.BxDialog.box_stencil_ref_mask = -1)))));
				this.BxDialog.WHanim(520f, num2, true, true);
				this.FD_fnIRContentRowFocused = new FnBtnBindings(this.fnIRContentRowFocused);
				flag = true;
			}
			else
			{
				this.BxL.activate();
				this.BxDesc.activate();
				this.BxL.Clear();
				this.BxDesc.Clear();
			}
			this.BxL.posSetDA(this.detail_pos_lx, 0f, 2, 900f, false);
			this.BxDesc.posSetDA(this.detail_pos_rx, 0f, 0, 900f, false);
			this.BxL.Clear();
			this.BxL.Small();
			this.BxL.margin_in_lr = 38f;
			this.BxL.margin_in_tb = 21f;
			this.BxL.init();
			float use_w = this.BxL.use_w;
			if (this.reel_overhold)
			{
				this.BxL.addP(new DsnDataP("", false)
				{
					name = "reel_need_discard",
					text = this.getReelNeedDiscardText(),
					swidth = use_w,
					size = 14f,
					html = true,
					TxCol = C32.d2c(4283780170U)
				}, false);
				this.BxL.Br();
			}
			this.LTabBar = ColumnRowNel.NCreateT<aBtnNel>(this.BxL, "ctg_tab", "row_tab", (int)this.rltab, new string[]
			{
				"&&Reel_tab_item",
				(this.reel_overhold ? NEL.error_img : "") + TX.Get("Reel_tab_effect", "")
			}, new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnReelTabChanged), 0f, 0f, false, false);
			this.LTabBar.LrInput(false);
			this.BxL.Br();
			this.BxL.addTab("L_scr", this.BxL.use_w, this.BxL.use_h, this.BxL.use_w, this.BxL.use_h, false);
			this.BxL.endTab(true);
			this.BxDesc.margin_in_lr = 32f;
			this.BxDesc.margin_in_tb = 28f;
			this.BxDesc.item_margin_x_px = (this.BxDesc.item_margin_y_px = 2f);
			this.BxDesc.init();
			this.BxDesc.addP(new DsnDataP("", false)
			{
				text = TX.Get("Reel_detail_reel_title", ""),
				TxCol = C32.d2c(4283780170U),
				size = 16f,
				alignx = ALIGN.LEFT,
				swidth = this.BxDesc.use_w - 4f
			}, false);
			this.BxDesc.Br();
			UiReelManager.prepareDescTabList(this.BxDesc.addTab("R_info", this.BxDesc.use_w, this.BxDesc.use_h, this.BxDesc.use_w, this.BxDesc.use_h, false), null);
			this.BxDesc.endTab(true);
			if (flag)
			{
				this.BxDialog.Clear();
				this.BxDialog.item_margin_y_px = 0f;
				this.BxDialog.margin_in_lr = 24f;
				this.BxDialog.margin_in_tb = 22f;
				this.BxDialog.init();
				BtnContainerRadio<aBtn> btnContainerRadio;
				UiItemManageBox.createItemDescDesignerBasic(this.BxDialog, false, true, out btnContainerRadio, out this.FbDialogDesc);
				this.BxDialog.posSetDA(-this.basex_u * 64f + this.detail_bounds_w * 0.5f - this.BxDialog.w * 0.5f, 0f, 0, 150f, true);
			}
			this.BxDialog.deactivate();
			this.initDetailL();
		}

		public string getReelNeedDiscardText()
		{
			return NEL.error_img + TX.GetA("Reel_need_subtract", 8.ToString(), (this.AReelCon.Count - 8 - ((this.Adiscard_ir != null) ? this.Adiscard_ir.Count : 0)).ToString());
		}

		public static void prepareDescTabList(Designer TabR, Designer AddTo = null)
		{
			TabR.Smallest();
			TabR.init();
			Designer designer = ((AddTo == null) ? TabR : AddTo);
			DsnDataRadio dsnDataRadio = new DsnDataRadio();
			dsnDataRadio.name = "list";
			dsnDataRadio.keys = new string[0];
			dsnDataRadio.w = TabR.use_w - 24f;
			dsnDataRadio.h = 32f;
			dsnDataRadio.navi_loop = 2;
			dsnDataRadio.unselectable = 2;
			dsnDataRadio.fnChanged = (BtnContainerRadio<aBtn> _BCon, int pre, int cur) => false;
			dsnDataRadio.SCA = new ScrollAppend(239, TabR.use_w, TabR.use_h, 4f, 6f, 0);
			dsnDataRadio.APoolEvacuated = new List<aBtn>(8);
			designer.addRadioT<aBtnNel>(dsnDataRadio.RowMode("reel_pict"));
		}

		public static void prepareDescTabList(Designer TabR, ReelExecuter.ETYPE etype)
		{
			string[] aeffect = ReelManager.OAreel_content[(int)etype].Aeffect;
			BtnContainer<aBtn> bcon = (TabR.Get("list", false) as BtnContainerRunner).BCon;
			bcon.default_h = 32f;
			if (bcon.OuterScrollBox != null)
			{
				bcon.OuterScrollBox.item_h = bcon.default_h;
			}
			string text = "reel_pict";
			if (text != bcon.skin_default)
			{
				bcon.Destroy();
				bcon.skin_default = text;
			}
			bcon.RemakeT<aBtnNel>(aeffect, text);
		}

		public static BtnContainer<aBtn> prepareDescTabList(Designer TabR, ReelManager.ItemReelContainer IR, FnBtnBindings FnHover = null)
		{
			BtnContainer<aBtn> bcon = (TabR.Get("list", false) as BtnContainerRunner).BCon;
			bcon.default_h = 48f;
			if (bcon.OuterScrollBox != null)
			{
				bcon.OuterScrollBox.item_h = bcon.default_h;
			}
			string text = "row";
			if (text != bcon.skin_default)
			{
				bcon.Destroy();
				bcon.skin_default = text;
			}
			bcon.RemakeT<aBtnNel>(X.makeToStringed<int>(X.makeCountUpArray(IR.Count, 0, 1)), text);
			using (STB stb = TX.PopBld(null, 0))
			{
				NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
				for (int i = 0; i < IR.Count; i++)
				{
					ButtonSkinNelUi buttonSkinNelUi = bcon.Get(i).get_Skin() as ButtonSkinNelUi;
					if (buttonSkinNelUi != null)
					{
						stb.Clear();
						IR.getOneRowDetail(stb, i, "\n", nelM2DBase.IMNG, true);
						buttonSkinNelUi.fix_text_size = 14f;
						buttonSkinNelUi.setTitle(stb.ToString());
						buttonSkinNelUi.getBtn().hover_snd = "cursor";
						if (FnHover != null)
						{
							buttonSkinNelUi.getBtn().addHoverFn(FnHover);
						}
					}
				}
			}
			return bcon;
		}

		public void initDetailL()
		{
			Designer tab = this.BxL.getTab("L_scr");
			tab.Clear();
			tab.Smallest();
			tab.init();
			this.BConEfReel = null;
			this.reel_list_listl_index = -1;
			this.SelectedIR = (this.FocusIR = null);
			this.IRContentLastSelected = null;
			if (this.rltab == UiReelManager.RLTAB.EFFECT)
			{
				string[] array = X.makeToStringed<int>(X.makeCountUpArray(this.AReelCon.Count, 0, 1));
				this.BConEfReel = tab.addRadioT<aBtnNel>(new DsnDataRadio
				{
					name = "list",
					keys = array,
					fnChanged = new BtnContainerRadio<aBtn>.FnRadioBindings(this.fnClickEffectReel),
					w = tab.use_w - 24f,
					h = 38f,
					navi_loop = 2,
					z_push_click = false,
					fnHover = new FnBtnBindings(this.fineReelDetail),
					fnMaking = new BtnContainer<aBtn>.FnBtnMakingBindings(this.fnMakingReelDetail),
					SCA = new ScrollAppend(239, tab.use_w, tab.use_h, 4f, 6f, 0)
				}.RowMode("reelinfo"));
			}
			else
			{
				string[] array2 = X.makeToStringed<int>(X.makeCountUpArray(this.AIrbox.Count, 0, 1));
				Designer designer = tab;
				DsnDataRadio dsnDataRadio = new DsnDataRadio();
				dsnDataRadio.name = "list";
				dsnDataRadio.keys = array2;
				dsnDataRadio.fnChanged = (BtnContainerRadio<aBtn> _BCon, int pre, int cur) => cur < 0;
				dsnDataRadio.w = tab.use_w - 24f;
				dsnDataRadio.h = 38f;
				dsnDataRadio.navi_loop = 2;
				dsnDataRadio.z_push_click = false;
				dsnDataRadio.fnHover = new FnBtnBindings(this.fineReelDetail);
				dsnDataRadio.fnMaking = new BtnContainer<aBtn>.FnBtnMakingBindings(this.fnMakingReelDetail);
				dsnDataRadio.fnClick = delegate(aBtn B)
				{
					if (this.BxDialog.isActive())
					{
						this.quitIRContentRowFocused(true);
					}
					else
					{
						this.initFocusIR(0);
					}
					return true;
				};
				dsnDataRadio.SCA = new ScrollAppend(239, tab.use_w, tab.use_h, 4f, 6f, 0);
				designer.addRadioT<aBtnItemRow>(dsnDataRadio.RowMode("normal"));
			}
			this.selectDetailBox();
		}

		private void selectDetailBox()
		{
			BtnContainer<aBtn> bcon = (this.BxL.getTab("L_scr").Get("list", false) as BtnContainerRunner).BCon;
			if (bcon.Length > 0)
			{
				bcon.Get(0).Select(true);
				return;
			}
			aBtn.PreSelected = null;
		}

		private bool fnMakingReelDetail(BtnContainer<aBtn> _BCon, aBtn _B)
		{
			int num = X.NmI(_B.title, 0, false, false);
			if (this.rltab == UiReelManager.RLTAB.EFFECT)
			{
				_B.z_push_click = false;
				ButtonSkinNelReelInfo buttonSkinNelReelInfo = _B.get_Skin() as ButtonSkinNelReelInfo;
				buttonSkinNelReelInfo.initReel(this.AReelCon[num]);
				if (this.Adiscard_ir != null && this.Adiscard_ir.IndexOf(num) >= 0)
				{
					buttonSkinNelReelInfo.hilighted = true;
					_B.SetChecked(true, true);
				}
			}
			else
			{
				ReelMBoxDrawer reelMBoxDrawer = this.AIrbox[num];
				(_B.get_Skin() as ButtonSkinItemRow).setItem(null, null, reelMBoxDrawer.createItemRow());
			}
			return true;
		}

		private bool fineReelDetail(aBtn B)
		{
			int num = X.NmI(B.title, 0, false, false);
			this.reel_list_listl_index = num;
			this.quitIRContentRowFocused(false);
			if (this.rltab == UiReelManager.RLTAB.EFFECT)
			{
				if (X.BTW(0f, (float)num, (float)this.AReelCon.Count))
				{
					Designer tab = this.BxDesc.getTab("R_info");
					ReelExecuter.ETYPE etype = this.AReelCon[num].getEType();
					UiReelManager.prepareDescTabList(tab, etype);
					if (this.reel_overhold && this.reel_effect_hold != 0)
					{
						ButtonSkinNelReelInfo buttonSkinNelReelInfo = B.get_Skin() as ButtonSkinNelReelInfo;
						if (this.reel_effect_hold < 0)
						{
							int num2 = this.Adiscard_ir.IndexOf(num);
							if (num2 >= 0)
							{
								buttonSkinNelReelInfo.hilighted = false;
								B.SetChecked(false, true);
								this.Adiscard_ir.RemoveAt(num2);
								this.addOrRemoveDiscardEffectReel(false, false);
							}
						}
						else
						{
							buttonSkinNelReelInfo.hilighted = true;
							B.SetChecked(true, true);
							BtnContainer<aBtn> bcon = (this.BxL.getTab("L_scr").Get("list", false) as BtnContainerRunner).BCon;
							this.addEffectReelDiscardList(bcon, num);
						}
					}
				}
			}
			else if (X.BTW(0f, (float)num, (float)this.AIrbox.Count))
			{
				this.selectDetailIR(num);
			}
			return true;
		}

		private BtnContainer<aBtn> selectDetailIR(int i)
		{
			if (i < 0)
			{
				return null;
			}
			Designer tab = this.BxDesc.getTab("R_info");
			ReelMBoxDrawer reelMBoxDrawer = this.AIrbox[i];
			this.SelectedIR = reelMBoxDrawer;
			((this.BxL.getTab("L_scr").Get("list", false) as BtnContainerRunner).BCon as BtnContainerRadio<aBtn>).setValue(i, false);
			int num = -1;
			if (this.IRContentLastSelected != null)
			{
				num = X.NmI(this.IRContentLastSelected.title, -1, false, false);
			}
			this.IRContentLastSelected = null;
			BtnContainer<aBtn> btnContainer = UiReelManager.prepareDescTabList(tab, reelMBoxDrawer.IR, new FnBtnBindings(this.fnIRContentRowFocused));
			if (num >= 0 && btnContainer.Length > 0)
			{
				this.IRContentLastSelected = btnContainer.Get(X.MMX(0, num, btnContainer.Length - 1));
			}
			return btnContainer;
		}

		private bool fnIRContentRowFocused(aBtn B)
		{
			if (this.mstt != ReelManager.MSTATE.DETAIL_REEL || this.SelectedIR == null)
			{
				return false;
			}
			NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
			float num = this.detail_bounds_w;
			float num2 = this.detail_bounds_h;
			float detail_left_w = this.detail_left_w;
			if (this.FocusIR == null)
			{
				this.BxDialog.activate();
				float num3 = -this.BxDialog.w - 22f;
				this.BxL.position(this.detail_pos_lx + num3, 0f, -1000f, -1000f, false);
				this.BxDesc.position(this.detail_pos_rx + num3, 0f, -1000f, -1000f, false);
				this.FocusIR = this.SelectedIR;
			}
			this.IRContentLastSelected = B;
			int num4 = X.NmI(B.title, 0, false, false);
			if (X.BTW(0f, (float)num4, (float)this.FocusIR.IR.Count))
			{
				NelItemEntry nelItemEntry = this.FocusIR.IR[num4];
				UiItemManageBox.fineItemDetailS(this.BxDialog, this.FbDialogDesc, nelM2DBase.IMNG.getHouseInventory(), nelItemEntry.Data, null, (int)nelItemEntry.grade, true, true, true, new UiItemManageBox.FnDescAddition(this.fnDescAdditionDetailItem), null, true, true);
			}
			return true;
		}

		private void initFocusIR(int def_index = 0)
		{
			if (this.IRContentLastSelected != null)
			{
				this.IRContentLastSelected.Select(true);
				return;
			}
			Designer tab = this.BxDesc.getTab("R_info");
			if (tab != null)
			{
				this.LTabBar.LrInput(false);
				BtnContainer<aBtn> bcon = (tab.Get("list", false) as BtnContainerRunner).BCon;
				if (bcon != null && bcon.Length > 0)
				{
					bcon.Get(X.MMX(0, def_index, bcon.Length - 1)).Select(true);
				}
				this.need_txkd_fine = true;
			}
		}

		private void quitIRContentRowFocused(bool selecting = false)
		{
			if (this.mstt != ReelManager.MSTATE.DETAIL_REEL || !this.BxDialog.isActive())
			{
				return;
			}
			BtnContainerRadio<aBtn> btnContainerRadio = (this.BxL.getTab("L_scr").Get("list", false) as BtnContainerRunner).BCon as BtnContainerRadio<aBtn>;
			if (!selecting)
			{
				this.LTabBar.LrInput(true);
				this.BxL.position(this.detail_pos_lx, 0f, -1000f, -1000f, false);
				this.BxDesc.position(this.detail_pos_rx, 0f, -1000f, -1000f, false);
				this.BxDialog.deactivate();
				this.playSnd("cursor");
				btnContainerRadio.setValue(-1, false);
				this.FocusIR = null;
				IN.clearPushDown(false);
				return;
			}
			if (this.SelectedIR == null)
			{
				this.quitIRContentRowFocused(false);
				return;
			}
			int num = this.AIrbox.IndexOf(this.SelectedIR);
			if (num >= 0)
			{
				btnContainerRadio.Get(num).Select(true);
				return;
			}
			this.quitIRContentRowFocused(false);
		}

		private string fnDescAdditionDetailItem(NelItem Itm, UiItemManageBox.DESC_ROW row, string def_string, int grade, ItemStorage.ObtainInfo Obt, int count)
		{
			if (row == UiItemManageBox.DESC_ROW.NAME)
			{
				using (STB stb = TX.PopBld(null, 0))
				{
					stb.Add("  <font size=\"12\">");
					this.M2D.IMNG.holdItemString(stb, Itm, -1, true);
					stb.Add("</font>");
					def_string += stb.ToString();
				}
			}
			return def_string;
		}

		private bool fnClickEffectReel(BtnContainer<aBtn> _BCon, int pre, int cur)
		{
			if (this.reel_overhold)
			{
				if (this.Adiscard_ir == null)
				{
					this.Adiscard_ir = new List<int>(this.AReelCon.Count - 8);
				}
				int num = this.Adiscard_ir.IndexOf(cur);
				ButtonSkinNelReelInfo buttonSkinNelReelInfo = _BCon.Get(cur).get_Skin() as ButtonSkinNelReelInfo;
				if (num >= 0)
				{
					this.addToDiscardList(buttonSkinNelReelInfo, _BCon);
				}
				else
				{
					buttonSkinNelReelInfo.hilighted = true;
					buttonSkinNelReelInfo.getBtn().SetChecked(true, true);
					this.reel_effect_hold = 1;
					this.addEffectReelDiscardList(_BCon, cur);
				}
			}
			return false;
		}

		private void addToDiscardList(ButtonSkinNelReelInfo _Sk, BtnContainer<aBtn> _BCon = null)
		{
			if (!this.reel_overhold || _Sk == null)
			{
				return;
			}
			if (_BCon == null)
			{
				_BCon = this.BConEfReel;
				if (_BCon == null)
				{
					return;
				}
			}
			if (this.Adiscard_ir == null)
			{
				this.Adiscard_ir = new List<int>(this.AReelCon.Count - 8);
			}
			int num = X.NmI(_Sk.getBtn().title, 0, false, false);
			if (this.Adiscard_ir.IndexOf(num) < 0)
			{
				_Sk.hilighted = true;
				_Sk.getBtn().SetChecked(true, true);
				this.reel_effect_hold = 1;
				this.addEffectReelDiscardList(_BCon, num);
			}
		}

		private void removeFromDiscardList(ButtonSkinNelReelInfo _Sk, BtnContainer<aBtn> _BCon = null, bool no_snd = false)
		{
			if (!this.reel_overhold || _Sk == null)
			{
				return;
			}
			if (_BCon == null)
			{
				_BCon = this.BConEfReel;
				if (_BCon == null)
				{
					return;
				}
			}
			if (this.Adiscard_ir == null)
			{
				this.Adiscard_ir = new List<int>(this.AReelCon.Count - 8);
			}
			int num = X.NmI(_Sk.getBtn().title, 0, false, false);
			int num2 = this.Adiscard_ir.IndexOf(num);
			if (num2 >= 0)
			{
				this.reel_effect_hold = -1;
				_Sk.hilighted = false;
				_Sk.getBtn().SetChecked(false, true);
				this.Adiscard_ir.RemoveAt(num2);
				this.addOrRemoveDiscardEffectReel(false, no_snd);
			}
		}

		private void addEffectReelDiscardList(BtnContainer<aBtn> _BCon, int cur)
		{
			if (this.Adiscard_ir == null)
			{
				this.Adiscard_ir = new List<int>(this.AReelCon.Count - 8);
			}
			while (this.AReelCon.Count - this.Adiscard_ir.Count <= 8 && this.Adiscard_ir.Count > 0)
			{
				int num = this.Adiscard_ir[0];
				ButtonSkinNelReelInfo buttonSkinNelReelInfo = _BCon.Get(num).get_Skin() as ButtonSkinNelReelInfo;
				buttonSkinNelReelInfo.hilighted = false;
				buttonSkinNelReelInfo.getBtn().SetChecked(false, true);
				this.Adiscard_ir.RemoveAt(0);
			}
			this.Adiscard_ir.Add(cur);
			this.addOrRemoveDiscardEffectReel(true, false);
		}

		private void addOrRemoveDiscardEffectReel(bool adding = true, bool no_snd = false)
		{
			if (!no_snd)
			{
				if (adding)
				{
					this.playSnd("tool_drag_init");
				}
				else
				{
					this.playSnd("tool_drag_quit");
				}
			}
			this.BxL.setValueTo("reel_need_discard", this.getReelNeedDiscardText());
			this.need_txkd_fine = true;
		}

		private bool fnReelTabChanged(BtnContainer<aBtn> _BCon, int pre, int cur)
		{
			this.quitIRContentRowFocused(true);
			this.rltab = (UiReelManager.RLTAB)cur;
			this.initDetailL();
			if (this.reel_overhold)
			{
				this.need_txkd_fine = true;
			}
			return true;
		}

		private ReelManager.ItemReelContainer progressReelStack()
		{
			NelItemEntry[] array = null;
			if (this.ReelIK != null && this.ReelIK.IKRow != null)
			{
				if (this.create_strage)
				{
					if (this.StDecided == null)
					{
						this.StDecided = new ItemStorage("reel_decided", 29);
						this.StDecided.infinit_stockable = (this.StDecided.water_stockable = true);
					}
					this.StDecided.Add(this.ReelIK.IKRow.Data, this.ReelIK.IKRow.count, (int)this.ReelIK.IKRow.grade, true, true);
				}
				else
				{
					array = new NelItemEntry[] { this.ReelIK.IKRow };
				}
			}
			this.digest_reel_count++;
			if (this.fnItemReelProgressing != null)
			{
				this.fnItemReelProgressing(this.Con.getCurrentItemReel());
			}
			return this.Con.digestItemEntry(array).progressReelStack();
		}

		public void deactivate()
		{
			if (this.TxKD != null)
			{
				this.TxKD.text_content = "";
			}
			if (this.ReelIK != null && this.isRotatingState())
			{
				this.ReelIK.deactivate();
			}
			for (int i = this.AReel.Count - 1; i >= 0; i--)
			{
				this.AReel[i].deactivate();
			}
			if (this.AIrbox != null)
			{
				for (int j = this.AIrbox.Count - 1; j >= 0; j--)
				{
					this.AIrbox[j].deactivate();
				}
			}
			if (this.after_clearreels)
			{
				this.after_clearreels = false;
				this.Con.clearReels(false, false, true);
			}
			if (TX.valid(this.digest_var_name))
			{
				EV.getVariableContainer().define(this.digest_var_name, this.digest_reel_count.ToString(), true);
			}
			this.rotate_decide_id = -1;
			this.tstate = -1;
			this.bgaf = -1;
		}

		public ItemStorage subtractStorage()
		{
			ItemStorage stDecided = this.StDecided;
			this.Con.digestObtainedMoney();
			this.StDecided = null;
			return stDecided;
		}

		public bool isActive()
		{
			return this.tstate >= 0;
		}

		public override void destruct()
		{
			if (this.AReel == null)
			{
				return;
			}
			if (this.EfBindT != null)
			{
				CameraBidingsBehaviour.UiBind.deassignPostRenderFunc(this.EfBindT);
				CameraBidingsBehaviour.UiBind.deassignPostRenderFunc(this.EfBindB);
			}
			this.disposeMaterial();
			if (this.ValotMain != null)
			{
				this.ValotMain.OnDestroy();
				this.ValotMain = null;
			}
			this.EfBindT = (this.EfBindB = null);
			this.Con.destructExecuterReels(false);
			this.Con = null;
			if (this.MdMain != null)
			{
				this.MdMain.destruct();
			}
			if (this.MdAdd != null)
			{
				this.MdAdd.destruct();
			}
			if (this.MdBg != null)
			{
				this.MdBg.destruct();
			}
			if (this.RecipeBook != null)
			{
				IN.DestroyOne(this.RecipeBook.gameObject);
			}
			if (this.Snd != null)
			{
				this.Snd.Dispose();
				this.Snd = null;
			}
			Flagger flgAreaTitleHide = this.M2D.FlgAreaTitleHide;
			if (flgAreaTitleHide != null)
			{
				flgAreaTitleHide.Rem("REEL");
			}
			Flagger flgStatusHide = UIStatus.FlgStatusHide;
			if (flgStatusHide != null)
			{
				flgStatusHide.Rem("REEL");
			}
			Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
			if (flgUiEffectDisable != null)
			{
				flgUiEffectDisable.Rem("REEL");
			}
			if (this.M2D != null)
			{
				this.M2D.FlagValotStabilize.Rem("REELMNG");
				if (EV.isActive(false))
				{
					Flagger flgAreaTitleHide2 = this.M2D.FlgAreaTitleHide;
					if (flgAreaTitleHide2 != null)
					{
						flgAreaTitleHide2.Add("EVENT");
					}
				}
			}
			this.AReel = null;
		}

		public bool cancelable
		{
			get
			{
				return this.tstate >= 0 && (this.rotate_decide_id < 0 || (this.AReel != null && this.rotate_decide_id >= this.AReel.Count));
			}
		}

		public bool discard_selection_finished
		{
			get
			{
				return this.reel_overhold && this.Adiscard_ir != null && this.AReelCon.Count - this.Adiscard_ir.Count <= 8;
			}
		}

		public bool reel_overhold
		{
			get
			{
				return this.AReelCon.Count > 8;
			}
		}

		public bool isRotatingState()
		{
			return this.isRotatingState(this.mstt);
		}

		public bool isRotatingState(ReelManager.MSTATE mstt)
		{
			return mstt >= ReelManager.MSTATE.OPENING;
		}

		public NelM2DBase M2D
		{
			get
			{
				return M2DBase.Instance as NelM2DBase;
			}
		}

		public bool isUiGMActive()
		{
			NelM2DBase m2D = this.M2D;
			return m2D != null && m2D.GM.isActive();
		}

		public Effect<EffectItem> getEffect()
		{
			return this.EF;
		}

		public void AddReelSpeedRecipeEffect(float l01)
		{
			this.reel_speed_rcp = X.MMX(-1f, this.reel_speed_rcp + l01, 1f);
		}

		public bool ReelObtainAnimating()
		{
			if (this.AReel != null)
			{
				for (int i = this.AReel.Count - 1; i >= 0; i--)
				{
					if (this.AReel[i].isReelObtainState())
					{
						return true;
					}
				}
			}
			return false;
		}

		private Effect<EffectItem> EF;

		private List<ReelExecuter> AReel;

		private List<ReelExecuter> AReelCon;

		public ReelManager Con;

		private MeshDrawer MdBg;

		private MeshDrawer MdMain;

		private MeshDrawer MdAdd;

		private MeshRenderer MrdMain;

		private MeshRenderer MrdBack;

		private MeshRenderer MrdAdd;

		private ValotileRenderer ValotMain;

		private ValotileRenderer ValotAdd;

		private ValotileRenderer ValotBack;

		private ReelManager.MSTATE mstt;

		private ReelExecuter ReelIK;

		private List<ReelMBoxDrawer> AIrbox;

		private const int MAXT_BG = 30;

		private int bgaf = -30;

		private int tstate;

		public const int FADEOUT_T = 35;

		private const float max_swidth = 530f;

		private int start_reel_count;

		public float x_u;

		public float y_u;

		public float basex_u;

		public float basey_u;

		private const float Z_MAIN = -4.7f;

		private List<UiReelManager.UMtr> AUseMaterial;

		private bool material_fine_flag;

		private int rotate_decide_id = -2;

		private float decide_delay = -30f;

		private const float ik_opening_x = 0f;

		private bool need_txkd_fine;

		private SndPlayer Snd;

		private UiBoxDesigner BxL;

		private UiBoxDesigner BxDesc;

		private UiBoxDesigner BxDialog;

		private ColumnRowNel LTabBar;

		private TextRenderer TxKD;

		private bool need_refine_pos_on_detail = true;

		public bool create_strage;

		public bool no_draw_hidescreen;

		public int added_money;

		public bool manual_deactivatable = true;

		public bool after_clearreels;

		public ItemStorage StDecided;

		public ReelManager.FnItemReelProgressing fnItemReelProgressing;

		public bool manipulable = true;

		public bool play_snd = true;

		public bool autodecide_progressable = true;

		private float reel_speed_rcp;

		private UiReelManager.RLTAB rltab;

		private List<int> Adiscard_ir;

		public const float tab_lr_input_width = 30f;

		private UiWarpConfirm BxConfirm;

		public bool first_reelopen;

		public int reel_effect_hold;

		private CameraRenderBinderFunc EfBindT;

		private CameraRenderBinderFunc EfBindB;

		private ReelMBoxDrawer FocusIR;

		private ReelMBoxDrawer SelectedIR;

		private aBtn IRContentLastSelected;

		private FillBlock FbDialogDesc;

		private BtnContainer<aBtn> BConEfReel;

		private Flagger FlgMainStabilize;

		private UiFieldGuide RecipeBook;

		private bool reel_list_back_to_opening;

		private int reel_list_listl_index = -2;

		private const int MAX_OBTAIN = 8;

		public string digest_var_name;

		public int digest_reel_count;

		public bool redraw_mdadd;

		private const float PREPARE_IR_X_INTV = 240f;

		private const float PREPARE_IR_SCROLL = 2.1f;

		private readonly float detail_bounds_w = IN.w * 0.8f;

		private readonly float detail_bounds_h = IN.h * 0.76f;

		private const float desc_w = 420f;

		private FnBtnBindings FD_fnIRContentRowFocused;

		public const float DETAIL_BTN_H = 38f;

		public enum RLTAB : byte
		{
			ITEM,
			EFFECT
		}

		public struct UMtr
		{
			public UMtr(Material _Mtr, bool _copied = false)
			{
				this.Mtr = _Mtr;
				this.name = this.Mtr.name;
				this.copied = _copied;
			}

			public Material Mtr;

			public bool copied;

			public string name;
		}
	}
}
