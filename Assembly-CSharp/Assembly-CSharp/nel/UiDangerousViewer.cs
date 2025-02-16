using System;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class UiDangerousViewer : IValotileSetable, IRunAndDestroy
	{
		public UiDangerousViewer(NelM2DBase _M2D, NightController _Con)
		{
			this.Con = _Con;
			this.M2D = _M2D;
		}

		public void clearWithoutNightLevel()
		{
			this.reel_obtained = 0;
			this.pre_dlevel = 0;
			this.redraw_dgd_flag = false;
			this.current_ui_obtainable = 0;
		}

		public void destruct()
		{
			if (this.prepared_gob)
			{
				this.MdUi.destruct();
				this.EF.destruct();
				this.Dgd.destruct();
			}
		}

		public void activateMain()
		{
			this.activate(UiDangerousViewer.STATE.NONE);
		}

		public void activateDLevelShow()
		{
			this.activate(UiDangerousViewer.STATE.DLEVEL_SHOW);
		}

		private void activate(UiDangerousViewer.STATE st)
		{
			bool prepared_gob = this.prepared_gob;
			this.changeState(st);
			this.t_actv = 0;
			bool flag = true;
			if (!prepared_gob)
			{
				this.GobUi = new GameObject("NightCon");
				this.MdUi = MeshDrawer.prepareMeshRenderer(this.GobUi, MTRX.MtrMeshNormal, 0f, -1, null, true, false);
				this.Ma = new MdArranger(this.MdUi);
				this.Dgd = new DangerGageDrawer(this.MdUi, this.GobUi.GetComponent<MeshRenderer>(), true);
				this.GobUi.layer = IN.gui_layer;
				this.EF = new PTCThreadRunnerOnce<EffectItemNel>(this.MdUi, this.GobUi, 16);
				this.EF.initEffect("nightcon", IN.getGUICamera(), new Effect<EffectItemNel>.FnCreateEffectItem(EffectItemNel.fnCreateOneNel), EFCON_TYPE.UI);
			}
			else
			{
				flag = !this.GobUi.activeSelf;
			}
			if (flag)
			{
				this.MdUi.base_z = 0f;
				this.GobUi.SetActive(true);
				this.M2D.addValotAddition(this);
				this.repositGob(true);
				this.EF.clear();
			}
			this.Dgd.auto_clear = false;
			this.Dgd.appear_delay = ((this.pre_dlevel == 0) ? 0 : 256);
			this.Dgd.val = this.pre_dlevel;
			this.Dgd.already_show = this.pre_dlevel;
			this.Dgd.daia_delay = 3;
			if (this.UiReel != null)
			{
				this.M2D.IMNG.getReelManager().destructGob();
				this.UiReel = null;
			}
			this.UiReel = this.M2D.IMNG.getReelManager().initUiState(ReelManager.MSTATE.NIGHTCON_ADDING, this.GobUi.transform, true);
			this.fineCurrentUiObtainable();
			this.UiReel.no_draw_hidescreen = true;
			this.Mp.addRunnerObject(this);
		}

		public void deactivate(bool immediate = false)
		{
			if (!this.prepared_gob)
			{
				return;
			}
			if (this.t_actv >= 0)
			{
				this.digestObtainableCount();
			}
			if (immediate)
			{
				this.t_actv = -40;
				if (this.state != UiDangerousViewer.STATE.OFFLINE)
				{
					this.state = UiDangerousViewer.STATE.OFFLINE;
					this.redraw_dgd_flag = false;
					this.fineMeterShowLevel();
					this.M2D.remValotAddition(this);
					if (this.Mp != null)
					{
						this.Mp.remRunnerObject(this);
					}
				}
				if (this.GobUi != null)
				{
					this.GobUi.SetActive(false);
				}
				if (this.UiReel != null)
				{
					this.M2D.IMNG.getReelManager().destructGob();
					this.UiReel = null;
				}
				if (this.Mp != null)
				{
					this.Mp.remRunnerObject(this);
					return;
				}
			}
			else if (this.t_actv >= 0)
			{
				this.t_actv = X.Mn(-1, -40 + this.t_actv);
				if (this.UiReel != null)
				{
					this.UiReel.deactivate();
				}
			}
		}

		private void changeState(UiDangerousViewer.STATE st)
		{
			this.state = st;
			this.t_state = 0;
			this.s_phase = 0;
		}

		private void tempActivation(bool flag)
		{
			if (this.state <= UiDangerousViewer.STATE.OFFLINE)
			{
				return;
			}
			this.GobUi.SetActive(flag);
		}

		private void digestObtainableCount()
		{
			while (this.current_ui_obtainable > this.reel_obtained)
			{
				this.reel_obtained += 1;
				this.M2D.IMNG.getReelManager().obtainProgress((int)this.reel_obtained);
			}
		}

		public bool use_valotile
		{
			get
			{
				return this.EF != null && !this.EF.no_graphic_render;
			}
			set
			{
				if (this.EF == null)
				{
					return;
				}
				this.EF.no_graphic_render = (this.EF.draw_gl_only = !X.DEBUGSTABILIZE_DRAW && value);
				ValotileRenderer.RecheckUse(this.MdUi, this.GobUi.GetComponent<MeshRenderer>(), ref this.ValotUi, value);
			}
		}

		public void repositGob(bool force = false)
		{
			if (!force && (this.state <= UiDangerousViewer.STATE.OFFLINE || this.GobUi == null))
			{
				return;
			}
			float num = -3.875f;
			IN.PosP(this.GobUi.transform, this.M2D.ui_shift_x, 0f, num);
		}

		public bool run(float fcnt)
		{
			this.redraw_dgd_flag = false;
			if (this.t_actv >= 0)
			{
				int num = this.t_actv;
				this.t_actv = num + 1;
				if (num < 40)
				{
					this.redraw_dgd_flag = true;
				}
				if (this.state == UiDangerousViewer.STATE.DLEVEL_SHOW)
				{
					if (this.t_state >= 70 && this.pre_dlevel < this.Con.getDangerMeterVal(true, true))
					{
						this.t_state = 50;
						this.Dgd.val = this.pre_dlevel + 1;
						this.Dgd.fixAppearTime(this.pre_dlevel, (float)this.t_actv);
						DangerGageDrawer dgd = this.Dgd;
						num = this.pre_dlevel;
						this.pre_dlevel = num + 1;
						Vector2 vector = dgd.getPos(num % 16) * 0.015625f * 2f;
						vector.y += 0.65625f;
						this.EF.PtcSTsetVar("cx", (double)vector.x).PtcSTsetVar("cy", (double)vector.y).PtcST("dangerlv_add", this.M2D.PlayerNoel, PTCThread.StFollow.NO_FOLLOW, null);
						this.redraw_dgd_flag = true;
					}
					if (this.t_state >= 50 && this.t_state <= 80)
					{
						this.redraw_dgd_flag = true;
					}
					if (this.t_state >= 110 && this.s_phase < 1)
					{
						if (this.reel_obtained < this.current_ui_obtainable)
						{
							this.reel_obtained += 1;
							this.t_state = 90;
							this.M2D.IMNG.getReelManager().obtainProgress((int)this.reel_obtained);
						}
						else
						{
							this.s_phase = 2;
						}
					}
					if (this.t_state > 150 && this.UiReel != null && this.s_phase < 2)
					{
						if (this.UiReel.ReelObtainAnimating())
						{
							this.t_state = 140;
						}
						else
						{
							this.s_phase = 2;
						}
					}
					if (this.t_state >= 250)
					{
						this.deactivate(false);
					}
				}
				else if (this.t_state >= 60)
				{
					this.deactivate(false);
				}
			}
			else
			{
				int num = this.t_actv - 1;
				this.t_actv = num;
				if (num < -40)
				{
					this.deactivate(true);
					return true;
				}
				this.redraw_dgd_flag = true;
			}
			this.t_state++;
			if (X.D)
			{
				this.repositGob(false);
				bool flag = false;
				if (this.redraw_dgd_flag)
				{
					this.redrawUi((this.t_actv >= 0) ? X.ZLINE((float)this.t_actv, 40f) : X.ZLINE((float)(40 + this.t_actv), 40f));
					flag = true;
				}
				else if (this.EF.Length > 0)
				{
					this.MdUi.chooseSubMesh(4, false, false);
					this.Ma.revertVerAndTriIndexSaved(false);
					flag = true;
				}
				if (flag || this.pre_drawn)
				{
					this.MdUi.chooseSubMesh(4, false, false);
					this.EF.runDrawOrRedrawMesh(X.D, (float)X.AF, 1f);
					this.MdUi.updateForMeshRenderer(true);
					this.MdUi.base_z = 0f;
					this.pre_drawn = flag;
				}
			}
			return true;
		}

		private void redrawUi(float alpha)
		{
			this.redraw_dgd_flag = false;
			float num = 0f;
			float num2 = 42f;
			float num3 = 1f;
			float num4;
			if (this.state == UiDangerousViewer.STATE.DLEVEL_SHOW)
			{
				num4 = alpha;
				num3 = 2f;
				num2 -= 180f * X.ZPOW(1f - alpha);
			}
			else
			{
				num4 = 0f;
			}
			this.pre_drawn = false;
			this.MdUi.clear(false, false);
			this.MdUi.base_x = (this.MdUi.base_y = 0f);
			this.MdUi.chooseSubMesh(0, false, true);
			this.MdUi.Col = this.MdUi.ColGrd.Black().mulA(0.25f * alpha).C;
			this.MdUi.Rect(0f, 0f, IN.w + 32f, IN.h + 32f, false);
			this.MdUi.chooseSubMesh(4, false, false);
			this.Ma.Set(true);
			if (num4 >= 0f)
			{
				this.Dgd.Redraw(this.MdUi, (float)((this.t_actv >= 0) ? this.t_actv : 2048), true, num4);
				this.MdUi.chooseSubMesh(4, false, false);
				this.Ma.Set(false);
				this.Ma.scaleAll(num3, num3, 0f, 0f, false);
				this.Ma.translateAll(num, num2, false);
				if (alpha != 1f)
				{
					this.Ma.mulAlpha(alpha);
					return;
				}
			}
			else
			{
				this.Ma.Set(false);
			}
		}

		public void fineCurrentUiObtainable()
		{
			int num = X.Mn(this.Con.reelObtainableCount(false), this.Con.getBattleCount()) + X.Mn(this.Con.reelObtainableCount(true), 2 + this.Con.getBattleCount());
			this.current_ui_obtainable = (byte)X.Mx(0, X.Mn(255, num));
		}

		public void fineMeterShowLevel()
		{
			this.pre_dlevel = X.Mn(this.Con.getDangerMeterVal(true, true), 192);
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeUShort((ushort)this.pre_dlevel);
		}

		public void writeBinaryTo2(ByteArray Ba)
		{
			Ba.writeByte((int)this.reel_obtained);
		}

		public void readBinaryFrom(ByteReader Ba, int vers)
		{
			this.pre_dlevel = (int)Ba.readUShort();
		}

		public void readBinaryFrom2(ByteReader Ba, int vers)
		{
			this.reel_obtained = (byte)Ba.readByte();
		}

		public Map2d Mp
		{
			get
			{
				return this.M2D.curMap;
			}
		}

		public bool prepared_gob
		{
			get
			{
				return this.state > UiDangerousViewer.STATE.UNPREPARED;
			}
		}

		public bool isActive()
		{
			return this.t_actv >= 0 && this.state > UiDangerousViewer.STATE.OFFLINE;
		}

		public override string ToString()
		{
			return "UiDangerousViewer";
		}

		public readonly NelM2DBase M2D;

		public readonly NightController Con;

		private const float meter_def_shift_px_y = 42f;

		public bool pre_drawn;

		private int pre_dlevel;

		private UiDangerousViewer.STATE state;

		private bool redraw_dgd_flag;

		private int t_state;

		private DangerGageDrawer Dgd;

		private byte current_ui_obtainable;

		private PTCThreadRunnerOnce<EffectItemNel> EF;

		private const int T_FADE = 40;

		private int t_actv = -40;

		public const float DANGER_MAX = 10f;

		public const float DANGER_CALC_MAX = 5f;

		public const int LEVEL_ONE_DAY = 16;

		private byte reel_obtained;

		private MeshDrawer MdUi;

		private ValotileRenderer ValotUi;

		private GameObject GobUi;

		private UiReelManager UiReel;

		private MdArranger Ma;

		private int s_phase;

		private enum STATE : byte
		{
			UNPREPARED,
			OFFLINE,
			NONE,
			DLEVEL_SHOW
		}
	}
}
