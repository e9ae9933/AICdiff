using System;
using System.Collections.Generic;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class MuseumViewer : IRunAndDestroy, IValotileSetable
	{
		public MuseumViewer(GameObject Base = null)
		{
			IN.addRunner(this);
			this.M2D = M2DBase.Instance as NelM2DBase;
			this.Gob = IN.CreateGobGUI(Base, "-Museum");
			this.Md = MeshDrawer.prepareMeshRenderer(this.Gob, MTRX.MtrMeshNormal, 0.01f, -1, null, true, false);
			this.Md.chooseSubMesh(1, false, false);
			this.Md.chooseSubMesh(2, false, false);
			this.Md.setMaterial(MTRX.MtrMeshNormal, false);
			MeshRenderer component = this.Gob.GetComponent<MeshRenderer>();
			this.Md.connectRendererToTriMulti(component);
			this.Valot = this.Gob.GetComponent<ValotileRenderer>();
			this.Valot.Init(this.Md, component, true);
			IN.setZAbs(this.Gob.transform, -2.8f);
			this.TxKD = IN.CreateGobGUI(this.Gob, "-TxKD").AddComponent<TextRenderer>();
			this.TxKD.html_mode = true;
			this.TxKD.size = 12f;
			this.TxKD.alignx = ALIGN.RIGHT;
			this.TxKD.aligny = ALIGNY.BOTTOM;
			this.TxKD.Col(NEL.ColText);
			this.TxKD.BorderCol(C32.WMulA(0.8f));
			this.TxNm = IN.CreateGobGUI(this.Gob, "-TxNm").AddComponent<TextRenderer>();
			this.TxNm.size = 55f;
			this.TxNm.TargetFont = TX.getTitleFont();
			this.TxNm.html_mode = true;
			this.TxNm.alignx = ALIGN.RIGHT;
			this.TxNm.aligny = ALIGNY.BOTTOM;
			this.TxNm.Col(C32.d2c(1728053247U));
			this.TxNm.BorderCol(C32.MulA(4278190080U, 1f));
			this.WhLis = new WheelListener(true);
			this.WhLis.Movable.Set(0f, 0f, 0f, 0f);
			this.WhLis.grab_enabled = true;
			this.WhLis.alloc_hovering_wheel = true;
			this.WhLis.calc_super_min_scale = true;
			this.WhLis.keyboard_translate = -0.46875f;
			this.WhLis.z_keyboard_shift_scaling = 0.06f;
			this.WhLis.x_scroll_level = 64f;
			this.WhLis.y_scroll_level = 64f;
			this.WhLis.moveable_margin_x = (this.WhLis.moveable_margin_y = 0f);
			this.WhLis.seeable_margin_x = (this.WhLis.seeable_margin_y = 40f);
			this.WhLis.addChangedFn(new FnWheelBindings(this.fnScrollAccessChanged));
			this.WhLis.bind();
			if (this.M2D != null)
			{
				this.M2D.addValotAddition(this);
				this.M2D.FlgHideWholeScreen.Add("MUSEUM");
				this.M2D.hideAreaTitle(true);
			}
		}

		public void addImg(EvImg Img)
		{
			if (this.AImg == null)
			{
				this.AImg = new List<EvImg>(1);
			}
			this.AImg.Add(Img);
		}

		public void destruct()
		{
			this.destruct(true);
		}

		public void destruct(bool deassign_runner)
		{
			if (deassign_runner)
			{
				IN.remRunner(this);
			}
			if (this.t >= 0f)
			{
				this.deactivate(true);
			}
			this.Md.destruct();
			if (this.M2D != null)
			{
				this.M2D.remValotAddition(this);
				this.M2D.FlgHideWholeScreen.Rem("MUSEUM");
			}
			IN.DestroyOne(this.Gob);
			this.WhLis.destruct();
		}

		public bool run(float fcnt)
		{
			if (this.AImg == null)
			{
				this.destruct(false);
				return false;
			}
			bool flag = false;
			bool flag2 = false;
			if (this.t >= 0f)
			{
				if (this.index >= -1)
				{
					if (this.index < 0)
					{
						this.chooseImage(0);
					}
					if (this.t <= 20f)
					{
						this.need_redraw = true;
					}
					this.t += fcnt;
					if (this.t_tx >= 0f)
					{
						if (!this.event_playing && this.t >= 12f)
						{
							if (IN.isMenuPD(1))
							{
								SND.Ui.play("cancel", false);
								EV.getVariableContainer().define("_museum_evt", "", true);
								this.deactivate(false);
								return true;
							}
							if (IN.isUiRemPD())
							{
								this.setTxKDVisible(false);
								SND.Ui.play("talk_progress", false);
							}
							else if (IN.isSubmit() || IN.isCancel())
							{
								if (this.index >= this.AImg.Count - 1)
								{
									SND.Ui.play("cancel", false);
									EV.getVariableContainer().define("_museum_evt", "", true);
									this.deactivate(false);
									return true;
								}
								this.chooseImage(this.index + 1);
							}
						}
					}
					else if (!this.event_playing && (IN.isUiRemPD() || IN.isMenuPD(1) || IN.isCancelPD() || IN.isUiAddPD() || IN.isSubmitUp(8) || IN.isLP(1) || IN.isRP(1) || IN.isTP(1) || IN.isBP(1)))
					{
						this.setTxKDVisible(true);
						SND.Ui.play("talk_progress", false);
						IN.clearPushDown(true);
					}
					if (this.t >= 12f && !this.event_playing)
					{
						if (this.index > 0 && IN.isLTabPD())
						{
							this.chooseImage(this.index - 1);
						}
						else if (this.index < this.AImg.Count - 1 && IN.isRTabPD())
						{
							this.chooseImage(this.index + 1);
						}
						if (TX.valid(this.ev_comment) && IN.isTargettingPD(1))
						{
							this.event_playing = true;
							SND.Ui.play("tool_drag_quit", false);
							EV.getVariableContainer().define("_museum_evt", this.ev_comment, true);
						}
					}
				}
				else
				{
					this.t += fcnt;
					this.need_redraw = true;
					if (this.t >= 75f)
					{
						this.deactivate(true);
					}
				}
			}
			else
			{
				this.need_redraw = true;
				this.t -= fcnt;
				if (this.t <= -80f)
				{
					if (this.auto_destruct)
					{
						this.destruct(false);
					}
					else
					{
						this.Gob.SetActive(false);
					}
					return false;
				}
			}
			if (X.D)
			{
				if (this.need_redraw)
				{
					this.need_redraw = false;
					this.redraw();
				}
				if (this.t_tx >= 0f)
				{
					if (this.t_tx < 20f)
					{
						flag = true;
						this.t_tx += fcnt * (float)X.AF;
					}
				}
				else if (this.t_tx > -20f)
				{
					flag = true;
					this.t_tx -= fcnt * (float)X.AF;
				}
				if (flag)
				{
					this.repositTxKD();
				}
				if (this.t_num >= 0f)
				{
					if (this.t_num < 100f)
					{
						if (this.t_num < 20f)
						{
							flag2 = true;
						}
						this.t_num += fcnt * (float)X.AF;
						if (this.t_num >= 100f)
						{
							this.t_num = -1f;
							flag2 = true;
						}
					}
				}
				else if (this.t_num > -20f)
				{
					flag2 = true;
					this.t_num -= fcnt * (float)X.AF;
				}
				if (flag2)
				{
					this.repositTxNm();
				}
			}
			return true;
		}

		public bool isActive()
		{
			return this.t >= 0f;
		}

		private void chooseImage(int _index)
		{
			if (this.index >= 0)
			{
				SND.Ui.play("paper", false);
			}
			this.index = X.MMX(0, _index, this.AImg.Count - 1);
			EvImg evImg = this.AImg[this.index];
			using (STB stb = TX.PopBld(null, 0))
			{
				stb.AddTxA("KD_museum_viewer", false);
				if (this.AImg.Count > 1)
				{
					stb.Add(" ");
					stb.Add((this.index == 0) ? "<key_s ltab/>" : "<key ltab/>");
					stb.Add((this.index == this.AImg.Count - 1) ? "<key_s rtab/>" : "<key rtab/>");
					stb.AddTxA("KD_museum_viewer_move", false);
				}
				if (TX.valid(this.ev_comment))
				{
					stb.Add(" ");
					stb.AddTxA("KD_museum_viewer_comment", false);
				}
				stb.Add(" ");
				stb.AddTxA("KD_museum_viewer_quit", false);
				this.TxKD.Txt(stb);
				stb.Clear();
				stb.AddTxA("Museum_image_index", false).TxRpl(this.index + 1).TxRpl(this.AImg.Count);
				this.TxNm.Txt(stb);
			}
			PxlSequence pSq = evImg.PF.pSq;
			float num = X.Mx((float)pSq.width, IN.w);
			float num2 = X.Mx((float)pSq.height, IN.h);
			this.WhLis.Movable = new Rect(-num * 0.5f, -num2 * 0.5f, num, num2);
			this.WhLis.resetShowingArea(0f, 0f, 1f, IN.w, IN.h);
			float num3 = X.Mx(0f, X.Mn(1f, X.Mn(IN.w / (float)pSq.width, IN.h / (float)pSq.height)));
			this.WhLis.setMinZScale(num3);
			this.WhLis.resetShowingArea(-1000f, -1000f, num3, -1000f, -1000f);
			this.WhLis.max_z_scale = 3f;
			this.need_fine_texture = true;
			this.TxNm.alpha = 0f;
			this.t = 0f;
			this.t_num = 0f;
			if (this.t_tx >= 0f)
			{
				this.t_tx = X.Mn(this.t_tx, 14f);
			}
			IN.clearPushDown(false);
		}

		public void deactivate(bool phase_after = false)
		{
			this.setTxKDVisible(false);
			this.WhLis.keyboard_translate = (this.WhLis.z_keyboard_shift_scaling = 0f);
			if (this.t_num >= 0f)
			{
				this.t_num = -1f;
			}
			if (!phase_after)
			{
				this.t = 0f;
				this.index = -2 - this.index;
				return;
			}
			if (this.t >= 0f)
			{
				if (this.M2D != null)
				{
					this.M2D.remValotAddition(this);
					this.M2D.FlgHideWholeScreen.Rem("MUSEUM");
				}
				this.WhLis.hide();
				this.t = -1f;
				this.TxKD.gameObject.SetActive(false);
				this.TxNm.gameObject.SetActive(false);
			}
		}

		public void setTxKDVisible(bool flag)
		{
			if (this.t_tx < 0f && flag)
			{
				this.t_tx = 0f;
				return;
			}
			if (this.t_tx >= 0f && !flag)
			{
				this.t_tx = -1f;
				if (this.t_num >= 0f)
				{
					this.t_num = -1f;
				}
			}
		}

		private void repositTxKD()
		{
			float num = ((this.t_tx >= 0f) ? X.ZSIN(this.t_tx, 20f) : X.ZLINE(20f + this.t_tx, 20f));
			IN.PosP2(this.TxKD.transform, IN.wh - 8f, -IN.hh + 5f - 32f * (1f - num));
			this.TxKD.alpha = num;
		}

		private void repositTxNm()
		{
			float num = ((this.t_num >= 0f) ? X.ZSIN(this.t_num, 20f) : X.ZLINE(20f + this.t_num, 20f));
			IN.PosP2(this.TxNm.transform, IN.wh - 78f + 140f * (1f - num) * ((this.t_num >= 0f) ? 0.4f : 1f), IN.hh - 90f);
			this.TxNm.alpha = num;
		}

		public bool use_valotile
		{
			get
			{
				return this.use_valotile_;
			}
			set
			{
				if (this.use_valotile_ == value)
				{
					return;
				}
				if (this.TxKD != null)
				{
					this.TxKD.use_valotile = value;
					this.TxNm.use_valotile = value;
					this.Valot.enabled = value;
				}
			}
		}

		private void redraw()
		{
			this.Md.clear(false, false);
			this.Md.chooseSubMesh(0, false, false);
			if (this.t >= 0f)
			{
				int num = this.index;
				float num2;
				Color32 color;
				if (this.index >= -1)
				{
					num2 = X.NI(0.4f, 1f, X.ZSIN(this.t, 16f));
					color = MTRX.ColBlack;
				}
				else
				{
					num = -num - 2;
					num2 = 1f - X.ZLINE(this.t, 61f);
					color = this.Md.ColGrd.Black().blend(4282004532U, 1f - num2).C;
				}
				this.Md.Col = MTRX.ColBlack;
				this.Md.Rect(0f, 0f, IN.w + 60f, IN.h + 60f, false);
				this.Md.Col = this.Md.ColGrd.Black().blend(this.ColBg, num2).C;
				float z_scale = this.WhLis.z_scale;
				float num3 = -this.WhLis.Showing.cx * z_scale;
				float num4 = -this.WhLis.Showing.cy * z_scale;
				PxlFrame pf = this.AImg[num].PF;
				this.Md.Rect(num3, num4, (float)pf.pSq.width * z_scale, (float)pf.pPose.height * z_scale, false);
				this.Md.Col = MTRX.ColWhite;
				this.Md.chooseSubMesh(1, false, false);
				if (this.need_fine_texture)
				{
					this.Md.setMaterial(MTRX.getMI(pf).getMtr(BLEND.NORMAL, -1), false);
					this.need_fine_texture = false;
				}
				this.Md.RotaPF(num3, num4, z_scale, z_scale, 0f, pf, false, false, false, uint.MaxValue, false, 0);
				this.Md.chooseSubMesh(2, false, false);
				this.Md.Col = C32.MulA(color, 1f - num2);
				this.Md.Rect(0f, 0f, IN.w + 60f, IN.h + 60f, false);
			}
			else
			{
				this.Md.chooseSubMesh(2, false, false);
				float num5 = X.ZLINE(80f + this.t, 80f);
				this.Md.Col = C32.MulA(4282004532U, num5);
				this.Md.Rect(0f, 0f, IN.w + 60f, IN.h + 60f, false);
			}
			this.Md.updateForMeshRenderer(false);
		}

		private bool fnScrollAccessChanged(WheelListener WhLis, float x, float y, float z)
		{
			this.need_redraw = true;
			return true;
		}

		public override string ToString()
		{
			return "MuseumViewer";
		}

		public bool auto_destruct = true;

		public string ev_comment;

		public bool event_playing;

		public Color32 ColBg = MTRX.ColBlack;

		private readonly NelM2DBase M2D;

		private float t;

		private float t_tx;

		private float t_num;

		private const float MAXT_FADEOUT = 80f;

		private const float MAXT_TX = 20f;

		private const float MAXT_FADE = 16f;

		private const float MAXT_NM_HOLD = 80f;

		private const float MAXT_NM_FADE = 20f;

		private const float MAXT_DARKEN = 75f;

		private readonly GameObject Gob;

		private readonly MeshDrawer Md;

		private readonly ValotileRenderer Valot;

		private readonly TextRenderer TxKD;

		private readonly TextRenderer TxNm;

		private List<EvImg> AImg;

		private int index = -1;

		private bool need_redraw;

		private bool need_fine_texture;

		private WheelListener WhLis;

		public const string ev_var_name = "_museum_evt";

		private bool use_valotile_;
	}
}
