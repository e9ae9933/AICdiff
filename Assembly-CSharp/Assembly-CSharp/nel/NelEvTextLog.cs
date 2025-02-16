using System;
using System.Collections.Generic;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public class NelEvTextLog : EvTextLog
	{
		public NelEvTextLog(int max = 192)
			: base(max)
		{
			this.DsnMain.TxCol = C32.d2c(4283780170U);
			this.DsnSelection.Col = C32.d2c(4283780170U);
			this.NM2D = M2DBase.Instance as NelM2DBase;
			this.DsnReloader = new DsnDataImg
			{
				sheight = 60f,
				FnDraw = new MeshDrawer.FnGeneralDraw(this.fnDrawReloader),
				MI = MTRX.MIicon,
				Col = MTRX.ColWhite
			};
		}

		public override void evQuit()
		{
			base.evQuit();
			this.deactivateEventContainer();
		}

		public override void destruct()
		{
			if (this.NM2D != null)
			{
				this.NM2D.FlagValotStabilize.Rem("TEXTLOG");
				this.NM2D.FlgStopEvtMsgValotizeOverride.Rem("TEXTLOG");
				Flagger flgValotileDisable = UIBase.FlgValotileDisable;
				if (flgValotileDisable != null)
				{
					flgValotileDisable.Rem("TEXTLOG");
				}
				Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
				if (flgUiEffectDisable != null)
				{
					flgUiEffectDisable.Rem("TEXTLOG");
				}
			}
			if (this.GobHide != null)
			{
				IN.DestroyOne(this.GobHide);
				this.GobHide = null;
				this.MdHide.destruct();
				this.MdHide = null;
			}
			if (this.DsEvtCon != null)
			{
				IN.DestroyOne(this.DsEvtCon.gameObject);
			}
			base.destruct();
		}

		protected override bool getSentence(string person, string key, List<string> Adest, string[] Ainj = null)
		{
			Adest.Clear();
			if (!NelMSGResource.getContent(key, Adest, false, false, false))
			{
				return false;
			}
			if (Ainj != null)
			{
				NelMSGResource.replaceTxInjection(Adest, Ainj);
			}
			return true;
		}

		public Designer createContainer(Designer DsCon, float h = 0f, int stencil_ref = -1)
		{
			if (h <= 0f)
			{
				h = DsCon.use_h + h;
			}
			float use_w = DsCon.use_w;
			Designer designer = (this.DsShower = DsCon.addTab("TextLog", use_w, h, use_w, h, true));
			if (stencil_ref >= 0)
			{
				designer.stencil_ref = stencil_ref;
			}
			designer.Small();
			designer.item_margin_y_px = 12f;
			designer.item_margin_x_px = 0f;
			designer.margin_in_lr = 0f;
			designer.margin_in_tb = 0f;
			designer.scrolling_margin_in_lr = 4f;
			designer.scrolling_margin_in_tb = 20f;
			this.FiReloader = null;
			designer.init();
			this.DsScb = designer.getScrollBox();
			this.DsScb.alloc_grab = true;
			this.scrolled = 0;
			this.reload_delay = 0f;
			this.RBox = IN.GetOrAdd<NelEvTextLog.RunnerBox>(this.DsShower.gameObject);
			this.RBox.Con = this;
			this.createNext(false);
			this.DsScb.setScrollLevelTo(0f, 1f, false);
			this.DsScb.addChangedFn(new ScrollBox.FnScrollBoxBindings(this.fnContainerScrollChanged));
			DsCon.endTab(true);
			DsCon.Br();
			return this.DsShower;
		}

		protected void createNext(bool async_load = false)
		{
			int num = 24;
			if (this.scrolled + 24 >= base.LEN)
			{
				num = base.LEN - this.scrolled;
				this.RBox.enabled = false;
			}
			else
			{
				this.RBox.enabled = true;
			}
			if (num == 0)
			{
				return;
			}
			BList<Designer.EvacuateMem> blist = null;
			float num2 = 0f;
			if (async_load)
			{
				blist = this.DsShower.EvacuateMemory(ListBuffer<Designer.EvacuateMem>.Pop(0), (Designer Ds, DesignerRowMem.DsnMem DMem) => DMem.Blk is Designer, false) as BList<Designer.EvacuateMem>;
				num2 = this.DsScb.moveable_y * 64f - this.DsScb.scrolled_pixel_y;
			}
			this.scrolled += num;
			this.DsShower.Clear();
			this.reload_delay = 0f;
			this.pre_drawn_frame = -1;
			float use_w = this.DsShower.use_w;
			if (this.RBox.enabled)
			{
				this.DsnReloader.swidth = use_w;
				this.FiReloader = this.DsShower.addImg(this.DsnReloader);
				this.FiReloader.enabled = false;
				this.DsShower.Br();
			}
			else
			{
				this.FiReloader = null;
			}
			Designer designer = this.DsShower.addTab("Log" + this.scrolled.ToString(), use_w, 0f, use_w, 0f, false);
			designer.Smallest();
			designer.stencil_ref = this.DsShower.stencil_ref;
			designer.margin_in_tb = 6f;
			designer.item_margin_y_px = 8f;
			designer.init();
			base.createToDesigner(designer, -this.scrolled, num);
			this.DsShower.endTab(true);
			if (blist != null)
			{
				this.DsScb.Fine(true);
				this.DsShower.Br();
				this.DsShower.ReassignEvacuatedMemory(blist, null, false);
				this.DsShower.rowRemakeCheck(false);
				this.DsScb.scrolled_pixel_y = this.DsScb.moveable_y * 64f - num2;
				this.DsScb.scrollBarDraggingPosReset();
				blist.Dispose();
			}
		}

		public bool fnDrawReloader(MeshDrawer Md, float alpha)
		{
			int num = X.ANMT(MTRX.SqLoadingS.countFrames(), 4f);
			if (num != this.pre_drawn_frame)
			{
				Md.RotaPF(0f, 0f, 2f, 2f, 0f, MTRX.SqLoadingS.getFrame(num), false, false, false, uint.MaxValue, false, 0);
				Md.updateForMeshRenderer(false);
			}
			return false;
		}

		public void runContainer(float fcnt)
		{
			if (this.reload_delay > 0f)
			{
				this.reload_delay += fcnt;
				if (this.reload_delay >= 25f)
				{
					this.createNext(true);
					return;
				}
			}
			else if (this.reload_delay < 0f)
			{
				this.reload_delay += 1f;
			}
		}

		public bool fnContainerScrollChanged(ScrollBox Scb, aBtn B)
		{
			if (this.FiReloader == null || !this.RBox.enabled)
			{
				return true;
			}
			this.FiReloader.enabled = (this.FiReloader.redraw_flag = Scb.isShowing(this.FiReloader, 0f, 0f, 0f, 0f));
			if (Scb.scrolled_pixel_y < 30f)
			{
				if (this.reload_delay <= 0f)
				{
					this.reload_delay = 1f - this.reload_delay;
				}
			}
			else if (this.reload_delay > 0f)
			{
				this.reload_delay = -this.reload_delay;
			}
			return true;
		}

		public override void runMesh(float fcnt)
		{
			if (this.MdHide == null)
			{
				return;
			}
			if (this.evcon_f >= 0f && (!EV.canEventHandle() || !EV.isStoppingGameHandle()))
			{
				this.deactivateEventContainer();
			}
			float num;
			if (-13f < this.evcon_f && this.evcon_f < 0f)
			{
				this.evcon_f -= fcnt;
				num = X.ZSIN(13f + this.evcon_f, 13f);
				if (num == 0f)
				{
					if (this.NM2D != null)
					{
						this.NM2D.FlagValotStabilize.Rem("TEXTLOG");
						this.NM2D.FlgStopEvtMsgValotizeOverride.Rem("TEXTLOG");
						Flagger flgValotileDisable = UIBase.FlgValotileDisable;
						if (flgValotileDisable != null)
						{
							flgValotileDisable.Rem("TEXTLOG");
						}
						Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
						if (flgUiEffectDisable != null)
						{
							flgUiEffectDisable.Rem("TEXTLOG");
						}
					}
					this.GobHide.SetActive(false);
					return;
				}
			}
			else
			{
				if (this.evcon_f < 0f || this.evcon_f >= 13f)
				{
					return;
				}
				this.evcon_f += fcnt;
				num = X.ZSIN(this.evcon_f, 13f);
			}
			this.MdHide.Col = C32.MulA(4278190080U, num * 0.7f);
			this.MdHide.Rect(0f, 0f, IN.w + 20f, IN.h + 20f, false);
			this.MdHide.updateForMeshRenderer(false);
			if (this.DsEvtCon != null)
			{
				IN.PosP(this.DsEvtCon.transform, 0f, X.NI(-IN.h * 0.8f, 10f, num), 0f);
			}
		}

		public override bool runInEvent(IMessageContainer Msg, EvMsgCommand Mtl)
		{
			if (this.evcon_f < 0f)
			{
				if (!EV.canEventHandle() || !EV.isStoppingGameHandle())
				{
					return false;
				}
				if (!IN.isUiAddPD())
				{
					return false;
				}
				if (Mtl != null && !Msg.isAllCharsShown())
				{
					Msg.showImmediate(false, false);
					return false;
				}
				if (!this.allow_msglog)
				{
					return false;
				}
				this.evcon_f = X.MMX(0f, 13f + this.evcon_f, 12f);
				this.NM2D = M2DBase.Instance as NelM2DBase;
				if (this.NM2D != null)
				{
					this.NM2D.FlagValotStabilize.Add("TEXTLOG");
					this.NM2D.FlgStopEvtMsgValotizeOverride.Add("TEXTLOG");
					Flagger flgValotileDisable = UIBase.FlgValotileDisable;
					if (flgValotileDisable != null)
					{
						flgValotileDisable.Add("TEXTLOG");
					}
					Flagger flgUiEffectDisable = UIBase.FlgUiEffectDisable;
					if (flgUiEffectDisable != null)
					{
						flgUiEffectDisable.Add("TEXTLOG");
					}
				}
				if (this.MdHide == null)
				{
					this.GobHide = new GameObject("Log_Hide");
					this.GobHide.layer = IN.gui_layer;
					IN.setZ(this.GobHide.transform, -5.175f);
					this.MdHide = MeshDrawer.prepareMeshRenderer(this.GobHide, MTRX.MtrMeshNormal, 0f, -1, null, false, false);
				}
				if (this.DsEvtCon == null)
				{
					float num = IN.w * 0.55f;
					float num2 = IN.h * 0.7f;
					this.DsEvtCon = IN.CreateGob(this.GobHide, "-child").AddComponent<Designer>();
					IN.setZ(this.DsEvtCon.transform, -0.01f);
					this.DsEvtCon.stencil_ref = 11;
					this.DsEvtCon.bgcol = MTRX.ColTrnsp;
					this.DsEvtCon.WH(num, num2);
					this.DsEvtCon.Smallest();
					this.DsEvtCon.margin_in_lr = 26f;
					this.DsEvtCon.margin_in_tb = 18f;
					this.TxDesc = IN.CreateGob(this.DsEvtCon.gameObject, "-child").AddComponent<TextRenderer>();
					this.TxDesc.alignx = ALIGN.RIGHT;
					IN.PosP(this.TxDesc.transform, num * 0.5f - 20f, -num2 * 0.5f - 20f, 0f);
					this.TxDesc.html_mode = true;
					this.TxDesc.size = 14f;
					this.TxDesc.Col(MTRX.ColWhite);
				}
				base.puchCache();
				if (Msg != null)
				{
					this.slice_index = Msg.getCurrentMsgKey(out this.slice_person_key, out this.slice_label);
				}
				else
				{
					this.slice_index = -1;
					this.slice_label = null;
				}
				this.TxDesc.text_content = TX.Get("TextLog_Desc", "");
				this.DsEvtCon.gameObject.SetActive(true);
				this.GobHide.SetActive(true);
				this.DsEvtCon.Clear();
				this.DsEvtCon.init();
				this.createContainer(this.DsEvtCon, 0f, 11);
				this.DsScb.BView.Select(true);
				SND.Ui.play("cursor_gear_reset", false);
				this.runMesh(0f);
				MeshDrawer meshDrawer = this.DsEvtCon.createTempMMRD(true).Make(MTRX.MtrMeshNormal);
				meshDrawer.Col = MTRX.ColWhite;
				meshDrawer.Box(0f, 0f, this.DsEvtCon.w, this.DsEvtCon.h, 1f, false).updateForMeshRenderer(false);
				return this.evcon_f >= 0f;
			}
			else
			{
				if (IN.isSubmitPD(1) && this.DsShower != null)
				{
					SND.Ui.play("cursor_gear_reset", false);
					this.DsShower.getScrollBox().setScrollLevelTo(0f, 1f, true);
					return true;
				}
				if (!IN.isCancel() && !IN.isUiAddPD() && !IN.isUiRemPD() && this.allow_msglog)
				{
					return true;
				}
				IN.clearPushDown(true);
				this.deactivateEventContainer();
				return true;
			}
		}

		public void deactivateEventContainer()
		{
			if (this.evcon_f >= 0f)
			{
				this.evcon_f = X.Mn(-1f, (!EV.isStoppingGameHandle()) ? (-12f) : (-12f + this.evcon_f));
				SND.Ui.play("cursor_gear_reset", false);
				if (EV.Sel.isActive())
				{
					EV.Sel.use_valotile = EV.use_valotile;
				}
			}
		}

		private NelEvTextLog.RunnerBox RBox;

		private DsnDataImg DsnReloader;

		private NelM2DBase NM2D;

		private Designer DsShower;

		private ScrollBox DsScb;

		private FillImageBlock FiReloader;

		private const int RELOAD_MAXT = 25;

		private const int one_scroll_count = 24;

		private const float item_margin_y = 8f;

		private int pre_drawn_frame = -1;

		private float reload_delay;

		private int scrolled;

		private float evcon_f = -13f;

		private const float EVCON_MAXT = 13f;

		private Designer DsEvtCon;

		private GameObject GobHide;

		private MeshDrawer MdHide;

		private TextRenderer TxDesc;

		private class RunnerBox : MonoBehaviourAutoRun
		{
			protected override bool runIRD(float fcnt)
			{
				if (this.Con == null || !base.enabled)
				{
					return false;
				}
				this.Con.runContainer(fcnt);
				return true;
			}

			public override void OnDestroy()
			{
				this.Con = null;
				base.OnDestroy();
			}

			public NelEvTextLog Con;
		}
	}
}
