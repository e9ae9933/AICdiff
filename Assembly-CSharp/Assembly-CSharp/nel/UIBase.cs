using System;
using Better;
using evt;
using m2d;
using nel.gm;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class UIBase : MonoBehaviour, IEfPtcSetable, IEfPInteractale
	{
		public void Awake()
		{
			if (UIBase.Instance == null)
			{
				UIBase.Instance = this;
			}
			UIBase.FlgEmotDefaultLock.Clear();
			UIBase.FlgValotileDisable = new Flagger(delegate(FlaggerT<string> Flg)
			{
				this.valotile_enabled = false;
			}, delegate(FlaggerT<string> Flg)
			{
				this.valotile_enabled = true;
			});
			UIBase.FlgUiEffectGmDisable = new Flagger(delegate(FlaggerT<string> Flg)
			{
				this.finePictValotize();
				this.CutinTempVisible(false);
			}, delegate(FlaggerT<string> Flg)
			{
				this.finePictValotize();
				this.CutinTempVisible(true);
			});
			UIBase.FlgUiEffectDisable = new Flagger(delegate(FlaggerT<string> Flg)
			{
				this.finePictValotize();
				this.ValotMdBg.enabled = this.Pict.use_valotile;
			}, delegate(FlaggerT<string> Flg)
			{
				this.finePictValotize();
				this.ValotMdBg.enabled = this.Pict.use_valotile;
			});
			this.FlgNoelAreaDisable = new Flagger(delegate(FlaggerT<string> Flg)
			{
				this.Pict.GobMeshHolder.SetActive(false);
				this.redraw_flag_ |= 524288U;
			}, delegate(FlaggerT<string> Flg)
			{
				this.Pict.GobMeshHolder.SetActive(true);
				this.redraw_flag_ |= 524288U;
			});
			UIBase.FlgLetterBoxFade = new Flagger(delegate(FlaggerT<string> Flg)
			{
				this.redraw_flag_ |= 2U;
			}, delegate(FlaggerT<string> Flg)
			{
				this.redraw_flag_ |= 2U;
			});
			this.FlgFrontLog = new Flagger(delegate(FlaggerT<string> Flg)
			{
				CameraBidingsBehaviour.UiBind.need_sort_binds = true;
				if (this.EfLog != null)
				{
					this.EfLog.z_far = -4.8120003f;
				}
			}, delegate(FlaggerT<string> Flg)
			{
				CameraBidingsBehaviour.UiBind.need_sort_binds = true;
				if (this.EfLog != null)
				{
					this.EfLog.z_far = -3.6f;
				}
			});
			base.gameObject.layer = LayerMask.NameToLayer("GUI");
			this.MMRD = base.gameObject.AddComponent<MultiMeshRenderer>();
			IN.PosP(base.transform, 0f, 0f, -4f);
			this.PoolCutin = new ObjPool<AnimateCutin>("-ACutin", base.transform, 4, 0);
			this.M2D = M2DBase.Instance as NelM2DBase;
			this.M2D.Ui = this;
			this.MMRD.stencil_ref = 225;
			this.MMRD.use_valotile = true;
			this.MtrBg = MTR.newMtr("Nel/UiBg");
			this.MdBg = this.MMRD.Make(MTRX.ColWhite, BLEND.MASK, this.MtrBg, null);
			this.MdBg.InitSubMeshContainer(0);
			this.MdBg.chooseSubMesh(1, false, false);
			this.MdBg.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, 225), true);
			this.MdBg.chooseSubMesh(0, false, false);
			this.MdBg.connectRendererToTriMulti(this.MMRD.GetMeshRenderer(this.MdBg));
			this.ValotMdBg = this.MMRD.GetValotileRenderer(this.MdBg);
			this.GobLeft = this.MMRD.GetGob(this.MdBg);
			this.TrsLeft = this.GobLeft.transform;
			this.MtrBg.SetFloat("_StencilRef", 225f);
			this.MMRD.stencil_ref = -1;
			this.MdTop = this.MMRD.Make(MTRX.ColWhite, BLEND.NORMAL, null, null);
			this.ValotMdTop = this.MMRD.GetValotileRenderer(this.MdTop);
			IN.setZAbs(this.MMRD.GetGob(this.MdTop).transform, -3.15f);
			this.MdTop.base_z = 0f;
			this.GobLog = IN.CreateGob(base.gameObject, "-Log");
			IN.setZ(this.GobLog.transform, 0.4000001f);
			this.LogBox = new UILog(this, this.GobLog);
			this.QuestBox = new UiQuestTracker(this, base.gameObject);
			this.M2D.QUEST.Ui = this.QuestBox;
			UIBase.FlgHideLog = new Flagger(delegate(FlaggerT<string> V)
			{
				this.LogBox.deactivateEventTemporary();
			}, delegate(FlaggerT<string> V)
			{
				this.LogBox.activateEventTemporary(false);
			});
			this.drawBg();
			this.MMRD.base_z = -0.02f;
			this.Pict = new UIPicture(this);
			this.Status = new UIStatus(this);
			this.VarP = new VariableP(8);
			this.EF = new EffectNel(base.gameObject, 320);
			this.EF.initEffect("UIBase", IN.getGUICamera(), new Effect<EffectItemNel>.FnCreateEffectItem(EffectItemNel.fnCreateOneNel), EFCON_TYPE.UI);
			this.EF.setLayer(this.layer, this.layer);
			this.EF.topBaseZ = -0.4f;
			this.EF.bottomBaseZ = -0.01f;
			this.EF.nomap_ratio_x = (this.EF.nomap_ratio_y = 0.015625f);
			if (X.DEBUGSTABILIZE_DRAW)
			{
				this.EfMMRD = IN.CreateGob(this.Pict.Gob, "-ef_mmrd").AddComponent<MultiMeshRenderer>();
				this.EF.assignMMRDForMeshDrawerContainer(this.EfMMRD);
			}
			else
			{
				this.EF.topBaseZ += -4f;
				this.EF.bottomBaseZ += -4f;
				this.EF.no_graphic_render = (this.EF.draw_gl_only = true);
				this.EfBindT = new CameraRenderBinderFunc("UIEF-top", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
				{
					if (UIBase.FlgUiEffectDisable.isActive() || !base.enabled)
					{
						return true;
					}
					this.EF.RenderOneSide(false, JCon.CameraProjectionTransformed, Cam, false);
					if (!this.FlgFrontLog.isActive())
					{
						this.LogBox.RenderOneSide(false, JCon.CameraProjectionTransformed, Cam);
					}
					return true;
				}, this.EF.topBaseZ);
				this.EfBindB = new CameraRenderBinderFunc("UIEF-bottom", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
				{
					if (UIBase.FlgUiEffectDisable.isActive() || !base.enabled)
					{
						return true;
					}
					this.EF.RenderOneSide(true, JCon.CameraProjectionTransformed, Cam, false);
					return true;
				}, this.EF.bottomBaseZ);
				this.EfLog = new CameraRenderBinderFunc("UI-Log-B", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
				{
					bool flag = this.FlgFrontLog.isActive();
					if (!base.enabled)
					{
						return true;
					}
					this.LogBox.RenderOneSide(true, JCon.CameraProjectionTransformed, Cam);
					if (flag)
					{
						this.LogBox.RenderOneSide(false, JCon.CameraProjectionTransformed, Cam);
					}
					return true;
				}, -3.6f);
				IN.getGUICamera();
				this.effect_binded = true;
			}
			this.M2D.Cam.vareawh_focus_max = (IN.wh - 170f) / this.M2D.Cam.base_scale;
			this.uiResetLeftPos();
			this.FD_MapDmgCntConEffectEnable = () => !this.event_center_uipic_ || this.EfDamageCounter == null || this.EfDamageCounter.z == 0f;
		}

		public bool effect_binded
		{
			get
			{
				return this.effect_binded_;
			}
			set
			{
				if (this.effect_binded_ == value || this.EfBindT == null)
				{
					return;
				}
				this.effect_binded_ = value;
				if (value)
				{
					CameraBidingsBehaviour.UiBind.assignPostRenderFunc(this.EfBindT);
					CameraBidingsBehaviour.UiBind.assignPostRenderFunc(this.EfBindB);
					CameraBidingsBehaviour.UiBind.assignPostRenderFunc(this.EfLog);
					return;
				}
				CameraBidingsBehaviour.UiBind.deassignPostRenderFunc(this.EfBindT);
				CameraBidingsBehaviour.UiBind.deassignPostRenderFunc(this.EfBindB);
				CameraBidingsBehaviour.UiBind.deassignPostRenderFunc(this.EfLog);
			}
		}

		public void setEnabled(bool flag)
		{
			base.enabled = flag;
			base.gameObject.SetActive(flag);
		}

		public bool valotile_enabled
		{
			get
			{
				return this.use_valotile_;
			}
			private set
			{
				this.use_valotile_ = !X.DEBUGSTABILIZE_DRAW && value;
				if (this.draw_letter_box)
				{
					this.ValotMdTop.enabled = this.use_valotile_;
				}
				this.finePictValotize();
				this.ValotMdBg.enabled = this.Pict.use_valotile;
				this.LogBox.valotile_enabled = value;
			}
		}

		private void finePictValotize()
		{
			if (!this.use_valotile_ && (UIBase.FlgUiEffectDisable.isActive() || UIBase.FlgUiEffectGmDisable.isActive()))
			{
				this.Pict.use_valotile = false;
				return;
			}
			this.Pict.use_valotile = true;
		}

		public void destruct()
		{
			if (UIBase.Instance == this)
			{
				UIBase.Instance = null;
			}
			if (this.Pict != null)
			{
				this.Pict.destruct();
			}
			if (this.LogBox != null)
			{
				this.LogBox.destruct();
			}
			if (this.Status != null)
			{
				this.Status.destruct();
			}
			if (this.EfBindT != null)
			{
				IN.getGUICamera();
				this.effect_binded = false;
			}
			if (this.MtrBg != null)
			{
				IN.DestroyOne(this.MtrBg);
				this.MtrBg = null;
			}
			if (this.QuestBox != null)
			{
				this.QuestBox.destruct();
			}
			if (this.EF != null)
			{
				this.EF.destruct();
			}
			UIBase.FlgUiEffectDisable = null;
			UIBase.FlgUiEffectGmDisable = null;
			UIBase.FlgHideLog = null;
		}

		public void setDrawBgFlag()
		{
			this.redraw_flag_ |= 524288U;
		}

		private void drawBg()
		{
			if (this.Fd == null)
			{
				PxlFrame pf = MTRX.getPF("ui_monyo");
				this.Fd = new FrameDrawer();
				this.Fd.BL(pf.getLayer(0).Img);
				this.MaBg = new MdArranger(this.MdBg);
				this.redraw_flag_ |= 524288U;
			}
			if (this.FlgNoelAreaDisable.isActive())
			{
				this.MdBg.clear(false, false);
				this.MaBg.Set(true);
				this.MdBg.updateForMeshRenderer(true);
				this.redraw_flag_ &= 4294311935U;
				return;
			}
			bool flag = false;
			bool flag2 = this.gmb_slide_t >= 0f && !this.event_center_uipic_;
			if ((this.redraw_flag_ & 524288U) != 0U)
			{
				this.MdBg.clearSimple();
				this.MdBg.resetVertexCount();
				this.MaBg.Set(true);
				this.MdBg.ColGrd.White().blend(4278190080U, 1f - X.ZLINE(X.Abs(this.transition_darken_t_) - 1f, (float)((this.transition_darken_t_ >= 0f) ? 50 : 20)));
				if (flag2)
				{
					float scale = M2DBase.Instance.Cam.getScale(true);
					float num = X.ZSIN(this.gmb_slide_t, 24f);
					float num2 = IN.w * num * 0.2f * 0.015625f;
					float num3 = X.Mx(0f, -this.ui_shift_x * X.NI(1f, scale, num * 0.75f)) * 0.015625f;
					float num4 = -UIBase.phh * 0.015625f;
					float num5 = -num4;
					float num6;
					if (CFGSP.uipic_lr != CFGSP.UIPIC_LR.R)
					{
						num6 = (UIBase.pwh + 340f + IN.wh) * 0.015625f;
					}
					else
					{
						num6 = -5.3125f;
					}
					this.MdBg.Col = C32.MulA(this.MdBg.ColGrd.C, 0f);
					float num7 = num6 - (num2 + num3) * 0.5f;
					float num8 = (IN.wh + 8f) * 0.015625f * scale;
					float num9 = (IN.hh + 8f) * 0.015625f * scale;
					this.MdBg.uvRect(num7 - num8, 0f - num9, num8 * 2f, num9 * 2f, true, false);
					this.MdBg.Tri(0, 1, 3, false).Tri(0, 3, 2, false).Tri(2, 3, 5, false)
						.Tri(2, 5, 4, false);
					this.MdBg.Pos(num6 - num2 - num3, num4, null).Pos(num6 - num2 - num3, num5, null);
					this.MdBg.Col = this.MdBg.ColGrd.C;
					this.MdBg.Pos(num6 - num3, num4, null).Pos(num6 - num3, num5, null);
					this.MdBg.Pos(num6, num4, null).Pos(num6, num5, null);
					this.MaBg.Set(false);
					if (this.gmb_slide_t >= 24f)
					{
						this.redraw_flag_ &= 4294443007U;
					}
				}
				else
				{
					this.MdBg.base_z = 0f;
					this.MdBg.Col = this.MdBg.ColGrd.C;
					this.MdBg.Rect(0f, 0f, 340f, UIBase.ph, false);
					this.MaBg.Set(false);
					flag = true;
					this.redraw_flag_ &= 4294443007U;
					CameraBidingsBehaviour.UiBind.need_sort_binds_post = true;
				}
			}
			else
			{
				this.MaBg.revertVerAndTriIndexSaved(false);
			}
			this.MdBg.chooseSubMesh(1, false, true);
			this.MdBg.Col = MTRX.ColWhite;
			if (this.event_center_uipic_)
			{
				float gamemenu_slide_z = this.gamemenu_slide_z;
				this.MdBg.Col = this.MdBg.ColGrd.White().mulA(gamemenu_slide_z).C;
				float num10 = 1f + (-1f + gamemenu_slide_z) * 0.25f;
				this.Fd.DrawTo(this.MdBg, 340f + UIBase.pwh, 0f, UIBase.pw * num10, UIBase.ph * num10);
			}
			else if (!flag2)
			{
				float num11 = 1.01f;
				float num12 = 300f * (-1f + ((this.visib_t < 0f) ? 1f : X.ZSIN(this.visib_t, 48f)));
				this.Fd.DrawTo(this.MdBg, 0f, 0f, 340f * num11 + num12, UIBase.ph * num11 + num12);
			}
			this.MdBg.chooseSubMesh(0, false, false);
			if (flag)
			{
				this.fineBgUPos(false);
			}
			this.MdBg.updateForMeshRenderer(true);
			this.redraw_flag_ &= 4294836223U;
			if (this.transition_darken_t_ >= 0f)
			{
				if (this.transition_darken_t_ < 50f)
				{
					this.redraw_flag_ |= 524288U;
					if (this.transition_darken_t_ > 2f || !M2DBase.Instance.transferring_game_stopping)
					{
						this.transition_darken_t_ += (float)X.AF;
						return;
					}
				}
			}
			else if (this.transition_darken_t_ < -1f)
			{
				this.redraw_flag_ |= 524288U;
				this.transition_darken_t_ = X.Mn(this.transition_darken_t_ + (float)X.AF, -1f);
			}
		}

		public bool transition_darken
		{
			set
			{
				if (value)
				{
					this.transition_darken_t_ = 0f;
					this.drawBg();
					this.redraw_flag_ |= 524288U;
					return;
				}
				if (this.transition_darken_t_ < 0f)
				{
					return;
				}
				this.transition_darken_t_ = -19f;
				this.redraw_flag_ |= 524288U;
			}
		}

		public void clear()
		{
			this.EClear();
			this.redraw_flag_ |= 131071U;
			this.LogBox.run(1f);
			this.ReleasePoolCutinAll();
		}

		public void endS()
		{
			this.ReleasePoolCutinAll();
			this.Pict.CutinMng.killBenchCutin();
		}

		public bool ui_particle_active
		{
			get
			{
				return this.gmb_slide_t > -24f || this.visib_t != -1f;
			}
		}

		public float gamemenu_slide_z
		{
			get
			{
				if (this.gm_slide_t < 0f)
				{
					return X.ZPOW(this.gm_slide_t + 24f, 24f);
				}
				return X.ZSIN2(this.gm_slide_t, 24f);
			}
		}

		public float gamemenu_bench_slide_z
		{
			get
			{
				if (this.gmb_slide_t < 0f)
				{
					return X.ZPOW(this.gmb_slide_t + 24f, 24f);
				}
				return X.ZSIN2(this.gmb_slide_t, 24f);
			}
		}

		public float visib_status_z
		{
			get
			{
				return X.ZLINE((this.visib_t < 0f) ? (-this.visib_t - 1f) : this.visib_t, 18f);
			}
		}

		public bool showing_pict
		{
			get
			{
				return (CFGSP.uipic_lr != CFGSP.UIPIC_LR.NONE && !this.draw_letter_box) || this.gm_slide_t >= 0f || this.gmb_slide_t >= 0f || this.event_center_uipic;
			}
		}

		public static float uiinvisible_default_pos_u
		{
			get
			{
				return (-UIBase.pwh - 340f) * 0.015625f;
			}
		}

		private void uiResetLeftPos()
		{
			Vector3 localPosition = this.TrsLeft.localPosition;
			float gamemenu_slide_z = this.gamemenu_slide_z;
			float num = ((CFGSP.uipic_lr == CFGSP.UIPIC_LR.NONE) ? 0f : UIBase.uipic_mpf);
			float num2 = X.Abs(num);
			float num3 = (float)X.MPF(num >= 0f);
			float visib_status_z = this.visib_status_z;
			float num4 = visib_status_z * num2;
			float num5 = X.Mx(gamemenu_slide_z, num4);
			localPosition.x = (-UIBase.pwh - 340f + 510f * (num5 - gamemenu_slide_z)) * 0.015625f * num3;
			if (this.event_center_uipic_)
			{
				this.Pict.gm_shift_x = X.NI((170f + UIBase.pw) * gamemenu_slide_z, 340f + UIBase.pwh, this.gamemenu_bench_slide_z) * num3;
			}
			else
			{
				float num6 = gamemenu_slide_z;
				if (this.gmb_slide_t > -24f)
				{
					float gamemenu_bench_slide_z = this.gamemenu_bench_slide_z;
					num6 *= 1f - gamemenu_bench_slide_z * 0.125f * num3;
				}
				if (num >= 0f)
				{
					this.Pict.gm_shift_x = (170f + UIBase.pw) * num6;
				}
				else
				{
					this.Pict.gm_shift_x = -510f * num6;
				}
			}
			this.TrsLeft.localPosition = localPosition;
			this.Pict.gm_shift_x *= 0.015625f;
			this.PosPict = localPosition;
			this.PosPict.x = this.PosPict.x + this.Pict.gm_shift_x;
			this.fineEffectCenterPos();
			if (!UIStatus.FlgStatusHide.isActive())
			{
				this.Status.fineBasePos(X.Mx(gamemenu_slide_z, visib_status_z));
			}
			if (this.gmb_slide_t < 0f || this.event_center_uipic_)
			{
				num5 = 1f - X.ZLINE(X.Abs(this.letter_box_t), 40f);
				this.M2D.ui_shift_x = (this.ui_shift_x = num * 170f * num5);
			}
			else
			{
				this.M2D.ui_shift_x = (this.ui_shift_x = -255f);
			}
			this.uiResetLogPos(visib_status_z, gamemenu_slide_z);
		}

		public void fineLogPos()
		{
			if (this.LogBox.Length > 0)
			{
				this.redraw_flag_ |= 262144U;
			}
		}

		public void uiResetLogPos(float tz = -1f, float gmtz = -1f)
		{
			if (tz < 0f)
			{
				gmtz = this.gamemenu_slide_z;
				tz = (this.event_center_uipic_ ? 1f : this.visib_status_z);
			}
			Transform transform = this.GobLog.transform;
			Vector3 localPosition = transform.localPosition;
			localPosition.x = (-UIBase.pwh + 140f + 340f * ((CFGSP.uipic_lr == CFGSP.UIPIC_LR.L) ? (tz - gmtz) : 0f)) * 0.015625f;
			localPosition.y = (-UIBase.phh + X.Mx((1f - tz) * 58f, X.NI(0f, 65f, this.Status.gage_tz)) + 25f + 200f) * 0.015625f;
			transform.localPosition = localPosition;
			this.redraw_flag_ &= 4294705151U;
			this.QuestBox.need_fine_pos = true;
		}

		public bool changeBgMaterial(Camera FinalRenderCam = null, Camera FinalCam = null, M2DBase Bas = null)
		{
			M2Camera cam = this.M2D.Cam;
			if (cam != null)
			{
				RenderTexture renderTexture = null;
				RenderTexture renderTexture2 = null;
				cam.getTextureForUiBg(out renderTexture, out renderTexture2);
				this.MtrBg.SetTexture("_MainTex", renderTexture);
				this.MtrBg.SetTexture("_MainTex2", renderTexture2);
				this.MtrBg.SetColor("_BaseColor", cam.transparent_color);
				this.fineBgUPos(true);
			}
			return true;
		}

		private void fineBgUPos(bool update_mesh = true)
		{
			if (this.M2D.Cam.getBaseMover() == null || this.MaBg.Length != 4)
			{
				return;
			}
			float num = 1f / (UIBase.pw + 16f);
			float num2 = 1f / (UIBase.ph + 16f);
			float num3 = 340f * num * 0.5f;
			float num4 = UIBase.ph * num2 * 0.5f;
			Vector2 vector = new Vector2(0.5f, 0.5f);
			this.MaBg.setUv(0, vector + new Vector2(-num3, -num4));
			this.MaBg.setUv(1, vector + new Vector2(-num3, num4));
			this.MaBg.setUv(2, vector + new Vector2(num3, num4));
			this.MaBg.setUv(3, vector + new Vector2(num3, -num4));
			if (update_mesh)
			{
				this.MdBg.updateForMeshRenderer(true);
			}
		}

		public void run(float fcnt)
		{
			if (!base.enabled)
			{
				return;
			}
			Bench.P("UI-Main");
			if (this.draw_letter_box)
			{
				if (this.letter_box_t <= 0f)
				{
					this.letter_box_t = -this.letter_box_t;
					this.ValotMdTop.enabled = this.use_valotile_;
					this.M2D.MapTitle.FlgNotShow.Add("EVENT");
				}
				if (this.letter_box_t < 40f)
				{
					this.letter_box_t = X.VALWALK(this.letter_box_t, 40f, fcnt);
					this.redraw_flag_ |= 32768U;
					if (this.MaLB == null)
					{
						this.redraw_flag_ |= 1U;
					}
					else
					{
						this.redraw_flag_ |= 2U;
					}
				}
				if (this.visib_t >= 0f)
				{
					this.redraw_flag_ |= 262144U;
					this.visib_t = -X.Mn(this.visib_t, 19f);
					this.QuestBox.FlgHide.Add("__EVENT");
				}
				if (this.logbox_hide_flag)
				{
					this.logbox_hide_flag = false;
					UIBase.FlgHideLog.Add("__EVENT");
				}
				if (this.visib_t < -1f)
				{
					this.redraw_flag_ |= 262144U;
					this.visib_t = X.VALWALK(this.visib_t, -1f, fcnt);
				}
			}
			else
			{
				if (this.letter_box_t > 0f)
				{
					this.letter_box_t = -this.letter_box_t;
				}
				if (this.letter_box_t < 0f)
				{
					this.letter_box_t = X.Mn(this.letter_box_t + 3f * fcnt, 0f);
					this.redraw_flag_ |= 32768U;
					if (this.MaLB == null || this.letter_box_t == 0f)
					{
						this.redraw_flag_ |= 1U;
					}
					else
					{
						this.redraw_flag_ |= 2U;
					}
				}
				if (this.visib_t <= 0f)
				{
					this.visib_t = 0f;
					this.redraw_flag_ |= 262144U;
					this.QuestBox.FlgHide.Rem("__EVENT");
				}
				if (this.visib_t < 48f)
				{
					this.visib_t = X.VALWALK(this.visib_t, 48f, fcnt);
					this.redraw_flag_ |= 425984U;
				}
			}
			if (this.gm_slide_t >= 0f)
			{
				if (this.gm_slide_t < 24f)
				{
					this.gm_slide_t = X.Mn(this.gm_slide_t + fcnt, 24f);
					this.redraw_flag_ |= 32768U;
					this.Status.need_reposit = true;
				}
			}
			else if (this.gm_slide_t > -24f)
			{
				this.gm_slide_t = X.Mx(this.gm_slide_t - fcnt, -24f);
				this.redraw_flag_ |= 32768U;
				this.Status.need_reposit = true;
			}
			if (this.gmb_slide_t >= 0f)
			{
				if (this.gmb_slide_t < 24f)
				{
					this.gmb_slide_t = X.Mn(this.gmb_slide_t + fcnt, 24f);
					this.redraw_flag_ |= 32768U;
					if (this.event_center_uipic_)
					{
						this.redraw_flag_ |= 524288U;
					}
					this.Status.need_reposit = true;
				}
			}
			else if (this.gmb_slide_t > -24f)
			{
				this.gmb_slide_t = X.Mx(this.gmb_slide_t - fcnt, -24f);
				this.redraw_flag_ |= 32768U;
				if (this.event_center_uipic_)
				{
					this.redraw_flag_ |= 524288U;
				}
				this.Status.need_reposit = true;
			}
			Bench.Pend("UI-Main");
			if (X.D && !COOK.reloading)
			{
				Bench.P("UILog");
				this.LogBox.run((float)X.AF);
				Bench.Pend("UILog");
			}
			Bench.P("draw");
			if (X.D || (this.redraw_flag_ & 65536U) > 0U)
			{
				if (this.visib_t > 0f)
				{
					this.fineBgUPos(true);
				}
				if (this.redraw_flag_ != 0U)
				{
					this.redraw();
				}
				Bench.P("quest");
				this.QuestBox.run((float)X.AF);
				Bench.Pend("quest");
			}
			Bench.Pend("draw");
			if (this.Status.isPlayerAssigned())
			{
				Bench.P("status");
				this.Status.run(fcnt);
				Bench.Pend("status");
				Bench.P("UIPict");
				this.Pict.run(fcnt, 1f, true);
				Bench.Pend("UIPict");
				Bench.P("UI-EF");
				this.EF.runDrawOrRedrawMesh(X.D_EF, (float)X.AF_EF, UILog.uilog_frame_base_speed);
				Bench.Pend("UI-EF");
				Bench.P("UIPict-post");
				this.Pict.runPost();
				Bench.Pend("UIPict-post");
			}
		}

		private int gameMenuSlideInner(ref float time, bool flag, bool immediate = false)
		{
			int num = 0;
			if (flag && time < 0f)
			{
				time = X.Mx(0f, 24f + time);
				this.Pict.gameMenuSlide(true);
				num = 1;
			}
			else if (!flag && time >= 0f)
			{
				time = X.Mn(-1f, -24f + time);
				this.Pict.gameMenuSlide(false);
				num = -1;
			}
			if (immediate && (num != 0 || (flag ? (time < 28f) : (time > -28f))))
			{
				this.redraw_flag_ |= 131071U;
				time = (float)(flag ? 29 : (-29));
				this.redraw();
			}
			return num;
		}

		public void gameMenuSlide(bool flag, bool immediate = false)
		{
			this.gameMenuSlideInner(ref this.gm_slide_t, flag, immediate);
			if (!flag && this.gmb_slide_t >= 0f)
			{
				this.gameMenuBenchSlide(false, immediate);
			}
		}

		public void gameMenuBenchSlide(bool flag, bool immediate = false)
		{
			int num = this.gameMenuSlideInner(ref this.gmb_slide_t, flag, immediate);
			if (num != 0)
			{
				this.redraw_flag_ |= 557056U;
				if (num == -1)
				{
					this.changeBgMaterial(null, null, null);
					return;
				}
				if (this.M2D.Cam != null)
				{
					CameraBidingsBehaviour.UiBind.need_sort_binds_post = true;
					this.MdBg.base_z = 1.0001f;
					this.MtrBg.SetTexture("_MainTex", this.M2D.Cam.getFinalizedTexture());
				}
			}
		}

		public void finalizeSlideFrame()
		{
			this.gameMenuSlideInner(ref this.gm_slide_t, this.gm_slide_t >= 0f, true);
			this.gameMenuSlideInner(ref this.gmb_slide_t, this.gmb_slide_t >= 0f, true);
		}

		public bool event_center_uipic
		{
			get
			{
				return this.event_center_uipic_;
			}
			set
			{
				if (!this.event_center_uipic && !value)
				{
					return;
				}
				this.event_center_uipic_ = value;
				if (value)
				{
					if (this.gm_slide_t < 0f)
					{
						this.gameMenuSlide(true, false);
					}
					if (this.gmb_slide_t < 0f)
					{
						this.gameMenuBenchSlide(true, false);
					}
					else
					{
						this.gmb_slide_t = 10f;
					}
					this.draw_letter_box = true;
					this.MdBg.chooseSubMesh(1, false, false);
					this.MdBg.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, -1), false);
					this.MdBg.chooseSubMesh(0, false, false);
					UIBase.FlgHideLog.Rem("__EVENT");
					UIBase.FlgEmotDefaultLock.Add("__EVENT");
					this.logbox_hide_flag = false;
				}
				else
				{
					if (this.gm_slide_t >= 0f && !this.M2D.GM.isActive())
					{
						this.gameMenuSlide(false, false);
					}
					if (this.gmb_slide_t >= 0f && (!this.M2D.GM.isActive() || UiBenchMenu.can_transfer_uipic))
					{
						this.gameMenuSlide(false, false);
					}
					UIBase.FlgEmotDefaultLock.Rem("__EVENT");
					this.MdBg.chooseSubMesh(1, false, false);
					this.MdBg.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMAL, 225), false);
					this.MdBg.chooseSubMesh(0, false, false);
				}
				this.M2D.fineHideEffectDisableByHideScreen();
				this.redraw_flag_ |= 819200U;
			}
		}

		public bool fnEvOpenUI(bool is_init)
		{
			if (is_init)
			{
				this.draw_letter_box = true;
				this.logbox_hide_flag = true;
			}
			return true;
		}

		public bool fnEvCloseUI(bool is_end)
		{
			if (is_end)
			{
				if (this.event_center_uipic)
				{
					this.event_center_uipic = false;
					this.Pict.recheck_emot = true;
				}
				if (this.draw_letter_box)
				{
					this.draw_letter_box = false;
					this.Pict.recheck_emot = true;
				}
				if (this.EfDamageCounter != null)
				{
					this.EfDamageCounter.z = 0f;
				}
				this.fineUiDmgCounterDraw();
				this.logbox_hide_flag = false;
				UIBase.FlgHideLog.Rem("__EVENT");
				UIBase.FlgUiEffectDisable.Rem("__EVENT");
				UIStatus.FlgStatusHide.Rem("__EVENT");
				UIBase.FlgLetterBoxFade.Rem("__EVENT");
				this.FlgNoelAreaDisable.Rem("__EVENT");
				this.GobLeft.SetActive(true);
			}
			return true;
		}

		public bool fnEvReadUI(EvReader ER, StringHolder rER, int skipping = 0)
		{
			string cmd = rER.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				if (num <= 1159865517U)
				{
					if (num <= 701987725U)
					{
						if (num <= 463400181U)
						{
							if (num != 62389229U)
							{
								if (num != 334281426U)
								{
									if (num == 463400181U)
									{
										if (cmd == "HIDE_LOGBOX")
										{
											UIBase.FlgHideLog.Add("EVENT_READ");
											return true;
										}
									}
								}
								else if (cmd == "SHOW_LOGBOX")
								{
									UIBase.FlgHideLog.Rem("EVENT_READ");
									return true;
								}
							}
							else if (cmd == "UIP_PTCST_REMOVE")
							{
								PTCThreadRunner threadRunner = this.EF.getThreadRunner();
								if (threadRunner != null)
								{
									threadRunner.removeThread(rER._1);
								}
								return true;
							}
						}
						else if (num != 569088426U)
						{
							if (num != 666570639U)
							{
								if (num == 701987725U)
								{
									if (cmd == "UIP_STOP_VALOTIZE")
									{
										UIBase.FlgUiEffectDisable.Add("__EVENT");
										return true;
									}
								}
							}
							else if (cmd == "UIP_EVENT_SETFADE")
							{
								bool b = rER._B3;
								this.event_center_uipic = true;
								if (TX.valid(rER._1))
								{
									if (rER.Nm(1, -1f) == 0f)
									{
										this.event_center_uipic = false;
									}
									else
									{
										bool flag = false;
										string text = rER._2 ?? "";
										if (TX.isStart(text, '!'))
										{
											text = TX.slice(text, 1);
											flag = true;
										}
										this.Pict.setFade(rER._1, UIPicture.Instance.getMultipleEmot(text), b, rER._B4, flag);
									}
								}
								if (EV.skipping > 0 || b)
								{
									this.finalizeSlideFrame();
								}
								this.eventDamageCounterEffect(true);
								return true;
							}
						}
						else if (cmd == "UIP_REPOSIT")
						{
							this.Pict.PosSyncSlide = new Vector3(rER.Nm(1, 0f) * 0.015625f, rER.Nm(2, 0f) * 0.015625f, rER.Nm(3, 1f));
							this.EClear();
							return true;
						}
					}
					else if (num <= 958114529U)
					{
						if (num != 759291017U)
						{
							if (num != 955666776U)
							{
								if (num == 958114529U)
								{
									if (cmd == "UIP_CUTIN_LAYEGG")
									{
										this.Pict.CutinMng.applyLayingEggCutin(rER._B1, rER.Nm(2, 0f), rER._B3, rER._B4);
										return true;
									}
								}
							}
							else if (cmd == "HIDE_STATUS")
							{
								UIStatus.FlgStatusHide.Add("__EVENT");
								return true;
							}
						}
						else if (cmd == "START_LETTERBOX")
						{
							this.draw_letter_box = true;
							return true;
						}
					}
					else if (num != 1103045678U)
					{
						if (num != 1146802598U)
						{
							if (num == 1159865517U)
							{
								if (cmd == "UIP_START_VALOTIZE")
								{
									UIBase.FlgUiEffectDisable.Rem("__EVENT");
									return true;
								}
							}
						}
						else if (cmd == "UIP_EVENT_TEMP_HIDE")
						{
							bool flag2 = rER._B1 || !this.M2D.GM.isActive();
							if (this.gm_slide_t >= 0f && flag2)
							{
								this.gameMenuSlide(false, false);
							}
							if (this.gmb_slide_t >= 0f && (flag2 || UiBenchMenu.can_transfer_uipic))
							{
								this.gameMenuBenchSlide(false, false);
							}
							return true;
						}
					}
					else if (cmd == "SHOW_LETTERBOX_A")
					{
						UIBase.FlgLetterBoxFade.Rem("__EVENT");
						return true;
					}
				}
				else if (num <= 2718431624U)
				{
					if (num <= 2107570124U)
					{
						if (num != 1562212955U)
						{
							if (num != 1725362744U)
							{
								if (num == 2107570124U)
								{
									if (cmd == "UIP_FADE")
									{
										UIBase.FlgHideLog.Add("EVENT_READ");
										return true;
									}
								}
							}
							else if (cmd == "UIPICT_GOB_DEACTIVE")
							{
								if (rER.Nm(1, 1f) != 0f)
								{
									this.FlgNoelAreaDisable.Add("__EVENT");
								}
								else
								{
									this.FlgNoelAreaDisable.Rem("__EVENT");
								}
								return true;
							}
						}
						else if (cmd == "SHOW_STATUS")
						{
							UIStatus.FlgStatusHide.Rem("__EVENT");
							return true;
						}
					}
					else if (num != 2561082619U)
					{
						if (num != 2676778604U)
						{
							if (num == 2718431624U)
							{
								if (cmd == "UIP_PTCST")
								{
									PTCThreadRunner.PreVar(rER, 2);
									this.PtcST(rER._1, null, PTCThread.StFollow.NO_FOLLOW);
									return true;
								}
							}
						}
						else if (cmd == "UIP_SETFADE")
						{
							if (TX.noe(rER._1))
							{
								this.Pict.recheck(0, 10);
							}
							else
							{
								this.Pict.setFade(rER._1, UIPicture.Instance.getMultipleEmot(rER._2), rER._B3, rER._B4, false);
							}
							return true;
						}
					}
					else if (cmd == "HIDE_LETTERBOX_A")
					{
						UIBase.FlgLetterBoxFade.Add("__EVENT");
						return true;
					}
				}
				else if (num <= 2907062351U)
				{
					if (num != 2741336697U)
					{
						if (num != 2820716051U)
						{
							if (num == 2907062351U)
							{
								if (cmd == "STOP_LETTERBOX")
								{
									if (this.event_center_uipic_)
									{
										return true;
									}
									this.draw_letter_box = false;
									this.logbox_hide_flag = false;
									return true;
								}
							}
						}
						else if (cmd == "UIP_PTC_VAR_S")
						{
							for (int i = 1; i < rER.clength; i += 2)
							{
								this.PtcVarS(rER.getIndex(i), rER.getIndex(i + 1));
							}
							return true;
						}
					}
					else if (cmd == "UIP_PTC_VAR")
					{
						for (int j = 1; j < rER.clength; j += 2)
						{
							this.PtcVar(rER.getIndex(j), rER.Nm(j + 1, 0f));
						}
						return true;
					}
				}
				else if (num != 3648081542U)
				{
					if (num != 3681457637U)
					{
						if (num == 4277748791U)
						{
							if (cmd == "HIDE_LETTERBOX")
							{
								if (this.letter_box_t == 0f || this.event_center_uipic_)
								{
									return true;
								}
								this.draw_letter_box = false;
								this.letter_box_t = 0f;
								this.redraw_flag_ |= 32769U;
								return true;
							}
						}
					}
					else if (cmd == "UIP_SYNC_CUTIN")
					{
						if (rER._1 == "HIDE")
						{
							this.Pict.quitSyncCutin();
						}
						else
						{
							this.Pict.initSyncCutin(new Vector3(rER.Nm(1, 0f), rER.Nm(2, 0f), rER.Nm(3, 1f)), new Vector3(rER.Nm(4, 0f), rER.Nm(5, 0f), rER.Nm(6, 1f)), CAim.parseString(rER._7, 0), rER.Nm(8, 0f));
						}
						return true;
					}
				}
				else if (cmd == "SHOW_LETTERBOX")
				{
					if (this.letter_box_t >= 40f)
					{
						return true;
					}
					this.draw_letter_box = true;
					this.letter_box_t = 40f;
					this.visib_t = -1f;
					this.redraw_flag_ |= 294913U;
					return true;
				}
			}
			return false;
		}

		public void EClear()
		{
			this.EF.clear();
			this.Pict.CutinMng.killBenchCutin();
			if (UIStatus.Instance != null)
			{
				UIStatus.Instance.playerUiEffectClear();
			}
			this.EfDamageCounter = null;
			this.fineUiDmgCounterDraw();
		}

		public IEfPtcSetable PtcVar(string key, float v)
		{
			this.VarP.Add(key, (double)v);
			return this;
		}

		public IEfPtcSetable PtcVarS(string key, string v)
		{
			this.VarP.AddStringItem(key, v);
			return this;
		}

		public IEfPtcSetable PtcVarS(string key, MGATTR v)
		{
			return this.PtcVarS(key, FEnum<MGATTR>.ToStr(v));
		}

		public EffectNel getEffect()
		{
			return this.EF;
		}

		public PTCThread PtcST(string ptcst_name, IEfPInteractale Listener = null, PTCThread.StFollow _follow = PTCThread.StFollow.NO_FOLLOW)
		{
			if (this.EF == null)
			{
				return null;
			}
			return this.EF.PtcST(ptcst_name, Listener ?? this, _follow, this.VarP);
		}

		public void killPtc(string ptcst_name, IEfPInteractale Listener = null)
		{
			if (this.EF == null)
			{
				return;
			}
			this.EF.killPtc(ptcst_name, Listener ?? this);
		}

		public bool getEffectReposition(PTCThread St, PTCThread.StFollow follow, float fcnt, out Vector3 V)
		{
			V = Vector2.zero;
			UIPictureBodyData bodyData = this.Pict.getBodyData();
			if (bodyData != null && bodyData.getEffectReposition(follow, fcnt, out V))
			{
				Vector3 effectCenterPos = this.getEffectCenterPos();
				if (effectCenterPos.z != 0f && effectCenterPos.z != 1f)
				{
					effectCenterPos.z = 1f / effectCenterPos.z;
				}
				V.Set((V.x - effectCenterPos.x) * effectCenterPos.z * 64f, (V.y - effectCenterPos.y) * effectCenterPos.z * 64f, V.z);
				return true;
			}
			return false;
		}

		public bool readPtcScript(PTCThread rER)
		{
			bool flag = false;
			string cmd = rER.cmd;
			if (cmd != null)
			{
				if (!(cmd == "%SND"))
				{
					if (cmd == "%HOLD")
					{
						PTCThread.StFollow stFollow;
						if (!FEnum<PTCThread.StFollow>.TryParse(rER._1, out stFollow, true))
						{
							rER.tError("不明な StFollow: " + rER._1);
						}
						else
						{
							rER.follow = stFollow;
						}
						return true;
					}
					if (cmd == "%PVIB")
					{
						for (int i = 1; i < rER.clength; i++)
						{
							UIPicture.getPr().PadVib(rER.getIndex(i), 1f);
						}
						return true;
					}
				}
				else
				{
					if (X.DEBUGNOSND)
					{
						return true;
					}
					bool flag2;
					StringKey soundKey = rER.getSoundKey(1, 0, out flag2, out flag);
					if (flag)
					{
						M2SoundPlayerItem m2SoundPlayerItem = this.M2D.Snd.createAbs("ui");
						m2SoundPlayerItem.play(soundKey, false);
						rER.StockSound(m2SoundPlayerItem);
					}
					else
					{
						SND.Ui.play(soundKey, false);
					}
					return true;
				}
			}
			UIPictureBodyData bodyData = this.Pict.getBodyData();
			return bodyData != null && bodyData.readPtcScript(rER);
		}

		public string getSoundKey()
		{
			return "UIBase";
		}

		public bool isSoundActive(SndPlayer S)
		{
			return S is M2SoundPlayerItem && (S as M2SoundPlayerItem).key == "ui" && S.isPlaying();
		}

		public bool initSetEffect(PTCThread Thread, EffectItem Ef)
		{
			return true;
		}

		public void eventDamageCounterEffect(bool from_event)
		{
			if (this.EfDamageCounter == null)
			{
				if (this.FD_drawDamageCounter == null)
				{
					this.FD_drawDamageCounter = new FnEffectRun(this.drawDamageCounter);
				}
				this.EfDamageCounter = this.EF.setEffectWithSpecificFn("ui_damage_counter", 0f, 0f, 0f, 0, 0, this.FD_drawDamageCounter);
			}
			if (from_event)
			{
				this.EfDamageCounter.z = 1f;
				this.EfDamageCounter.y = -240f;
				return;
			}
			this.EfDamageCounter.y = 180f;
		}

		public void fineUiDmgCounterDraw()
		{
			if ((CFGSP.dmgcounter_position & CFGSP.UIPIC_DMGCNT.UI) != CFGSP.UIPIC_DMGCNT.NONE)
			{
				this.eventDamageCounterEffect(false);
				this.EfDamageCounter.z = (float)(EV.isActive(false) ? 1 : 0);
			}
			else if (this.EfDamageCounter != null && this.EfDamageCounter.z == 0f)
			{
				this.EfDamageCounter.destruct();
				this.EfDamageCounter = null;
			}
			this.fineDmgCounterRunable(false);
		}

		public void fineDmgCounterRunable(bool givingup = false)
		{
			if (this.M2D.curMap != null && this.M2D.curMap.DmgCntCon != null)
			{
				M2DmgCounterContainer dmgCntCon = this.M2D.curMap.DmgCntCon;
				dmgCntCon.runable = ((this.EfDamageCounter != null) ? (givingup ? 3 : 2) : 1);
				if ((CFGSP.dmgcounter_position & CFGSP.UIPIC_DMGCNT.MAP) == CFGSP.UIPIC_DMGCNT.NONE)
				{
					dmgCntCon.MvIgnore = this.M2D.PlayerNoel;
				}
				else
				{
					dmgCntCon.MvIgnore = null;
				}
				dmgCntCon.TS = 1f;
			}
		}

		public bool event_damage_counter_effect
		{
			get
			{
				return this.EfDamageCounter != null;
			}
		}

		public AnimateCutin PopPoolCutin(string cutin_name, AnimateCutin.FnDrawAC _fnDrawAC, bool check_dupe = true)
		{
			if (check_dupe)
			{
				AnimateCutin animateCutin;
				while ((animateCutin = this.getAnimateCutin(cutin_name)) != null)
				{
					animateCutin.deactivate(false);
				}
			}
			AnimateCutin animateCutin2 = this.PoolCutin.Next();
			if (animateCutin2.FD_Deactivate == null)
			{
				animateCutin2.FD_Deactivate = delegate(AnimateCutin Cti)
				{
					this.ReleasePoolCutin(Cti);
				};
			}
			animateCutin2.Init(this.M2D.curMap, cutin_name, _fnDrawAC);
			if (UIBase.FlgUiEffectGmDisable.isActive())
			{
				animateCutin2.gameObject.SetActive(false);
			}
			IN.setZAbs(animateCutin2.transform, -4.28f);
			return animateCutin2;
		}

		public int cutin_act_count
		{
			get
			{
				return this.PoolCutin.count_act;
			}
		}

		public AnimateCutin getAnimateCutin(string cutin_name)
		{
			for (int i = this.PoolCutin.count_act - 1; i >= 0; i--)
			{
				AnimateCutin on = this.PoolCutin.GetOn(i);
				if (on.cutin_name == cutin_name)
				{
					return on;
				}
			}
			return null;
		}

		public AnimateCutin ReleasePoolCutin(AnimateCutin Cti)
		{
			if (Cti != null)
			{
				this.PoolCutin.Release(Cti);
			}
			return null;
		}

		public void ReleasePoolCutinAll()
		{
			for (int i = this.PoolCutin.count_act - 1; i >= 0; i--)
			{
				this.PoolCutin.GetOn(i).deactivate(false);
			}
		}

		public void CutinTempVisible(bool f)
		{
			for (int i = this.PoolCutin.count_act - 1; i >= 0; i--)
			{
				this.PoolCutin.GetOn(i).gameObject.SetActive(f);
			}
		}

		public AnimateCutin CutinAssignFader(AnimateCutin Cutin, PR Pr, string fd_key, UIPictureBase.EMSTATE sta = UIPictureBase.EMSTATE.NORMAL)
		{
			UIPictureFader.UIPFader fader = this.Pict.getFader(fd_key);
			if (fader != null)
			{
				UIEMOT emot = fader.getEmot();
				UIPictureBase.EMSTATE emstate = Pr.getEmotState() | fader.add_state | sta;
				UIPictureBase.PrEmotion.BodyPaint bodyPaint = this.Pict.GetEmot(emot, ref emstate).Get(emstate);
				UIPictureBodyData uipictureBodyData = bodyPaint.Body.getReplaceTerm() ?? bodyPaint.Body;
				if (uipictureBodyData is UIPictureBodySpine)
				{
					UIPictureBodySpine uipictureBodySpine = uipictureBodyData as UIPictureBodySpine;
					Cutin.initUIPictBody(fd_key, uipictureBodySpine, Pr, emstate);
				}
			}
			return Cutin;
		}

		private void redraw()
		{
			Bench.P("Log/Bg");
			if ((this.redraw_flag_ & 32768U) > 0U)
			{
				this.uiResetLeftPos();
			}
			else if ((this.redraw_flag_ & 262144U) > 0U)
			{
				this.uiResetLogPos(-1f, -1f);
			}
			if ((this.redraw_flag_ & 655360U) > 0U)
			{
				this.drawBg();
			}
			Bench.Pend("Log/Bg");
			Bench.P("Letterbox");
			if ((this.redraw_flag_ & 1U) > 0U)
			{
				this.MdTop.clear(false, false);
				this.redraw_flag_ |= 256U;
				this.redraw_flag_ &= 4294967293U;
				if (this.letter_box_t != 0f)
				{
					if (this.MaLB == null)
					{
						this.MaLB = new MdArranger(this.MdTop);
					}
					this.redrawLetterBox(true);
				}
				else
				{
					this.MaLB = null;
				}
			}
			if ((this.redraw_flag_ & 258U) > 0U)
			{
				if (this.letter_box_t != 0f && (this.redraw_flag_ & 2U) > 0U)
				{
					this.redrawLetterBox(false);
				}
				this.MdTop.updateForMeshRenderer(true);
			}
			this.redraw_flag_ &= 524288U;
			Bench.Pend("Letterbox");
		}

		private void redrawLetterBox(bool first = false)
		{
			int num = 0;
			int num2 = 0;
			if (first)
			{
				this.MaLB.Set(true);
			}
			else
			{
				if (this.MaLB == null)
				{
					return;
				}
				this.MaLB.revertVerAndTriIndex(ref num, ref num2);
			}
			float num3 = X.ZLINE(X.Abs(this.letter_box_t), 40f) * 48f;
			this.MdTop.Col = MTRX.ColBlack;
			if (this.event_center_uipic_)
			{
				float gamemenu_bench_slide_z = this.gamemenu_bench_slide_z;
				this.MdTop.Col = C32.MulA(MTRX.ColBlack, 1f - gamemenu_bench_slide_z);
			}
			if (UIBase.FlgLetterBoxFade.isActive())
			{
				num3 = 0f;
			}
			this.MdTop.RectBL(-UIBase.pwh, UIBase.phh - num3 + 2f, UIBase.pw, num3, false);
			this.MdTop.RectBL(-UIBase.pwh, -UIBase.phh - 2f, UIBase.pw, num3, false);
			this.MaLB.Set(false);
			if (!first)
			{
				this.MaLB.revertVerAndTriIndexAfter(num, num2, false);
			}
		}

		public void fineHpMpRatio(float hp_ratio, float mp_ratio)
		{
			this.MtrBg.SetFloat("_HpRatio", hp_ratio);
			this.MtrBg.SetFloat("_MpRatio", mp_ratio);
			if (hp_ratio <= 0f)
			{
				if (this.t_gameover < 0)
				{
					this.initGameOver();
					return;
				}
			}
			else if (hp_ratio > 0f && this.t_gameover >= 0)
			{
				this.t_gameover = -1;
			}
		}

		private bool drawDamageCounter(EffectItem E)
		{
			PR pr = UIPicture.getPr();
			if (pr == null || pr.Mp == null || pr.Mp.DmgCntCon == null)
			{
				return true;
			}
			M2Camera cam = this.M2D.Cam;
			pr.Mp.DmgCntCon.drawDmgCounterForSpecificMv(pr, E, E.x * 0.015625f, E.y * 0.015625f, (E.z == 1f) ? 1.8f : 1.4f, 1f, -0.65f, true);
			return true;
		}

		public void initGameOver()
		{
		}

		public bool runGameOver(int fcnt)
		{
			return false;
		}

		public void fineEffectCenterPos()
		{
			if (this.EF != null)
			{
				Vector3 effectCenterPos = this.getEffectCenterPos();
				this.EF.setEffectBasePos(effectCenterPos.x, effectCenterPos.y, effectCenterPos.z);
			}
		}

		public Vector3 getEffectCenterPos()
		{
			if (this.EfBindT == null)
			{
				return this.Pict.getEffectShift(false);
			}
			Vector3 effectShift = this.Pict.getEffectShift(true);
			Vector2 vector = this.PosPict;
			return new Vector3(vector.x + effectShift.x, vector.y + effectShift.y, effectShift.z);
		}

		public MeshDrawer setBaseToScreenCenter(MeshDrawer Md)
		{
			Vector3 effectCenterPos = this.getEffectCenterPos();
			Md.base_x = -effectCenterPos.x;
			Md.base_y = -effectCenterPos.y;
			return Md;
		}

		public MultiMeshRenderer get_MMRD()
		{
			return this.MMRD;
		}

		public GameObject GetGob(MeshDrawer Md)
		{
			return this.MMRD.GetGob(Md);
		}

		public bool can_transfer_uipic
		{
			get
			{
				return this.gm_slide_t <= -24f || (this.gmb_slide_t > 0f && UiBenchMenu.can_transfer_uipic);
			}
		}

		public bool gm_slide_active
		{
			get
			{
				return this.gm_slide_t >= 0f;
			}
		}

		public bool gmb_slide_active
		{
			get
			{
				return this.gmb_slide_t >= 0f;
			}
		}

		public int layer
		{
			get
			{
				return base.gameObject.layer;
			}
		}

		public static float uipic_mpf
		{
			get
			{
				return (float)((CFGSP.uipic_lr == CFGSP.UIPIC_LR.R) ? (-1) : 1);
			}
		}

		public MeshDrawer MakeMesh(Color32 Col, BLEND blnd, Material Mtr = null)
		{
			return this.MMRD.Make(Col, blnd, Mtr, null);
		}

		public MeshDrawer MakeMesh(BLEND blnd, MImage MI)
		{
			return this.MMRD.Make(blnd, MI);
		}

		public const float Z_UI = -4f;

		public const float Z_LOG_BACK = -3.6f;

		public const float Z_PIC = -4.13f;

		public const float Z_CUTIN = -4.28f;

		public const float Z_GM = -4.125f;

		public const float Z_TITLECALL = -3.875f;

		public const float Z_BLURSC = -3.25f;

		public const float Z_LETTERBOX = -3.15f;

		public const float Z_UI_GACHA_T = -4.75f;

		public bool draw_letter_box;

		public float letter_box_t;

		public float ui_shift_x;

		private float transition_darken_t_;

		private MultiMeshRenderer MMRD;

		private MeshDrawer MdTop;

		private MdArranger MaLB;

		private EffectNel EF;

		public UIStatus Status;

		public GameObject GobLeft;

		public Transform TrsLeft;

		public NelM2DBase M2D;

		private Transform TrsMapCamera;

		private UIPicture Pict;

		public UiSummonerAnnounce SummonerAnn;

		public GameObject GobStatus;

		private GameObject GobLog;

		public UILog LogBox;

		public UiQuestTracker QuestBox;

		public Vector3 PosPict;

		public static Flagger FlgHideLog;

		public Flagger FlgFrontLog;

		private bool logbox_hide_flag;

		private uint redraw_flag_ = 131071U;

		public const uint REENTRY_MESHBIT_T = 1U;

		public const uint REDRAW_LETTERBOX_BIT = 2U;

		public const uint UPDATE_MESHBIT_T = 256U;

		public const uint REDRAW_IMMEDIATE_BIT = 65536U;

		public const uint RESETPOS_UI = 32768U;

		public const uint RESETPOS_LOG = 262144U;

		public const uint REDRAW_UI_BG_FRAME = 131072U;

		public const uint _REDRAW_ALL_BIT = 131071U;

		public const uint REDRAW_BG_MESH = 524288U;

		private const float LETTERBOX_HEIGHT = 48f;

		private const int T_LETTERBOX = 40;

		private const int T_LTB_FADE = 40;

		private const int T_LEFT_DARKEN = 50;

		private const int T_LEFT_DARKEN_SHOWING = 20;

		public readonly float status_x = -IN.wh + 340f + 36f;

		public readonly float status_y = -IN.hh + 76f;

		private const int T_UIHIDE = 18;

		private const int T_VISIB_MAP = 48;

		private float visib_t;

		private const int T_GAMEOVER = 140;

		private int t_gameover = -280;

		public static readonly float pw = IN.w;

		public static readonly float pwh = IN.w / 2f;

		public static readonly float ph = IN.h;

		public static readonly float phh = IN.h / 2f;

		public const float uiw = 340f;

		public const float uiwh = 170f;

		public static readonly float gamew = IN.w - 340f;

		public static readonly float gamewh = UIBase.gamew / 2f;

		private Material MtrBg;

		private MeshDrawer MdBg;

		private MdArranger MaBg;

		private ValotileRenderer ValotMdBg;

		private ValotileRenderer ValotMdTop;

		private MultiMeshRenderer EfMMRD;

		private CameraRenderBinderFunc EfBindB;

		private CameraRenderBinderFunc EfLog;

		private CameraRenderBinderFunc EfBindT;

		private EffectItem EfDamageCounter;

		private FnEffectRun FD_drawDamageCounter;

		private ObjPool<AnimateCutin> PoolCutin;

		public static UIBase Instance;

		public Flagger FlgNoelAreaDisable;

		public static Flagger FlgUiEffectGmDisable;

		public static Flagger FlgUiEffectDisable;

		public static Flagger FlgValotileDisable;

		public static readonly Flagger FlgEmotDefaultLock = new Flagger(null, null);

		public Func<bool> FD_MapDmgCntConEffectEnable;

		public static Flagger FlgLetterBoxFade;

		private VariableP VarP;

		private bool event_center_uipic_;

		private const int GM_SLIDE_MAXT = 24;

		private float gm_slide_t = -29f;

		private float gmb_slide_t = -29f;

		private bool effect_binded_;

		private bool use_valotile_ = true;

		private FrameDrawer Fd;
	}
}
