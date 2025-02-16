using System;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class ReelMBoxDrawer : IEfPInteractale
	{
		public ReelMBoxDrawer(ReelManager _Con, UiReelManager _Ui, ReelManager.ItemReelContainer _IR, int _index)
		{
			this.Con = _Con;
			this.Ui = _Ui;
			this.IR = _IR;
			this.index = _index;
			this.Dr = new NelMBoxDrawer(MTR.DrMBox);
			ReelManager.ItemReelColor colSet = this.IR.ColSet;
			this.Dr.col_base_light = colSet.top;
			this.Dr.col_base_dark = colSet.bottom;
			this.Dr.col_pic_light = colSet.icon;
			this.Dr.col_pic_dark = MTRX.cola.Set(colSet.icon).multiply(0.6f, 0.6f, 0.6f, 1f).rgba;
			this.Dr.col_inner_light = MTRX.cola.Set(colSet.top).multiply(0.2f, 0.3f, 0.4f, 1f).rgba;
			this.Dr.col_inner_dark = MTRX.cola.Set(colSet.bottom).multiply(0.2f, 0.3f, 0.4f, 1f).rgba;
			this.fineMeshId();
		}

		public ItemStorage.IRow createItemRow()
		{
			ItemStorage.IRow row = new ItemStorage.IRow(NelItem.GetById("itemreelC_" + this.IR.key, false), new ItemStorage.ObtainInfo(), false);
			row.AddCount(1, 0);
			return row;
		}

		public void fineMeshId()
		{
			this.Dr.mid_base = this.Ui.getMaterialId(MTRX.MtrMeshNormal, false);
			this.Dr.mid_pic = this.Ui.getMaterialId(MTR.MIiconL.getMtr(BLEND.NORMAL, -1), false);
			if (this.UbDr != null || this.IR.is_rare)
			{
				this.MdAdd = this.Ui.getAddMd();
			}
		}

		public ReelMBoxDrawer position(float _x, float _y, bool immediate = false)
		{
			if (this.af >= 0f && this.af <= 2f)
			{
				immediate = true;
			}
			this.dx = _x;
			this.dy = _y;
			if (immediate)
			{
				this.x = _x;
				this.y = _y;
				this.pos_check = false;
			}
			else
			{
				this.pos_check = true;
			}
			this.outpos = !X.BTW(-IN.wh - 140f, this.x, IN.wh + 140f) || !X.BTW(-IN.hh - 100f, this.y, IN.hh + 100f);
			return this;
		}

		public void runDraw(float fcnt, MeshDrawer Md, Effect<EffectItem> EF)
		{
			if (this.pos_check)
			{
				this.x = X.MULWALK(this.x, this.dx, 0.18f);
				this.y = X.MULWALK(this.y, this.dy, 0.18f);
				if (X.LENGTHXYS(this.x, this.y, this.dx, this.dy) < 0.1f)
				{
					this.position(this.dx, this.dy, true);
					this.pos_check = false;
					if (this.outpos)
					{
						return;
					}
				}
			}
			else if (this.outpos)
			{
				return;
			}
			Md.base_px_x = this.x;
			Md.base_px_y = this.y;
			Md.chooseSubMesh(this.Dr.mid_base, false, false);
			float num = ((this.af >= 0f) ? X.ZLINE(this.af, 30f) : X.ZLINE(30f + this.af, 30f));
			float num2 = X.ZSIN(this.t_open - 8f, 30f);
			uint ran = X.GETRAN2(this.index * 3, 5);
			if (num2 < 1f)
			{
				Md.Col = Md.ColGrd.Set(this.IR.ColSet.bottom).mulA(num * (1f - num2)).C;
				for (int i = 0; i < 2; i++)
				{
					uint ran2 = X.GETRAN2(this.index + i * 8, 7 + i);
					Md.Identity();
					Md.Scale(X.COSI((float)IN.totalframe + X.RAN(ran2, 2215) * 150f, 273f + X.RAN(ran2, 603) * 93f), 1f, false).Rotate(((float)IN.totalframe + X.RAN(ran2, 1287) * 400f) / X.NI(320f, 490f, X.RAN(ran2, 723)) * 3.1415927f, false);
					Md.Circle(0f, 0f, 120f + 60f * num2, 1f, false, 0f, 0f);
				}
			}
			Md.Identity();
			float num3 = 0f;
			Matrix4x4 matrix4x = Matrix4x4.Scale(new Vector3(2.7f, 2.7f, 2.7f)) * X.RotMxZXY360(X.COSIT(177f + X.RAN(ran, 3321) * 98f) * 8f, X.COSIT(231f + X.RAN(ran, 2462) * 41f) * 13f, X.COSIT(498f + X.RAN(ran, 1553) * 21f) * 4f);
			Matrix4x4 matrix4x2 = Matrix4x4.identity;
			if (this.t_open >= 0f)
			{
				float num4 = this.t_open - 4f;
				num3 = X.ZPOW(num4, 20f) * 0.6f - X.ZSIN(num4 - 20f, 37f) * 0.14f + X.ZCOS(num4 - 40f, 58f) * 0.18f - X.SINI(num4 - 20f, 68f) * 0.1f * (1f - X.ZPOW(num4 - 20f, 100f));
				float num5 = -0.2f - X.ZSIN(this.t_open, 8f) * 0.2f + X.ZSIN(this.t_open - 8f, 13f) * 0.65f - X.ZCOS(this.t_open - 16f, 20f) * 0.25f;
				matrix4x2 = Matrix4x4.Scale(new Vector3(1f - num5 * 1.2f, 1f + num5 * 1f, 1f));
				matrix4x = matrix4x2 * matrix4x;
				this.t_open += fcnt;
			}
			this.Dr.drawTo(Md, 0f, 0f, matrix4x, num3, num);
			if (this.IR.is_rare)
			{
				if (ReelMBoxDrawer.PtcKira == null)
				{
					ReelMBoxDrawer.PtcKira = new EfParticleOnce("reel_mbox_kira", EFCON_TYPE.UI);
				}
				ReelMBoxDrawer.PtcKira.drawTo(this.MdAdd, Md.base_x * 64f, Md.base_y * 64f, 0f, (int)(this.IR.ColSet.top & 16777215U), this.af, 120f);
				this.Ui.redraw_mdadd = true;
			}
			if (this.t_open >= 4f)
			{
				this.MdAdd.base_x = Md.base_x;
				this.MdAdd.base_y = Md.base_y;
				this.MdAdd.setCurrentMatrix(matrix4x2, false);
				float num6 = 0.25f + 0.75f * X.ZSIN2(this.t_open - 4f, 28f);
				this.UbDr.Radius(180f * num6, 240f * num6).Col(C32.MulA(this.IR.ColSet.bottom, X.ZLINE(this.t_open - 4f, 40f) * ((this.af >= 0f) ? 1f : X.ZLINE(20f + this.af, 20f))), 1f);
				this.UbDr.drawTo(this.MdAdd, 0f, 20f, this.t_open, false);
				this.MdAdd.Identity();
				this.Ui.redraw_mdadd = true;
			}
			if (this.af >= 0f)
			{
				this.af += fcnt;
				return;
			}
			this.af -= fcnt;
		}

		public void animOpen()
		{
			if (this.t_open < 0f)
			{
				this.t_open = 0f;
				this.UbDr = new UniBrightDrawer();
				this.UbDr.Thick(30f, 58f).CenterCicle(55f, 70f, 160f).Count(13);
				this.UbDr.agR_visible_center = 1.5707964f;
				this.UbDr.agR_visible_range = 1.2566371f;
				this.UbDr.agR_visible_margin = 0.9424779f;
				this.fineMeshId();
				this.Ui.playSnd("slot_decided");
				this.PtcOpen = this.Ui.getEffect().PtcSTsetVar("cx", (double)(0.015625f * this.x)).PtcSTsetVar("cy", (double)(this.y * 0.015625f))
					.PtcSTsetVar("color", (double)(18446744071578845183UL | (ulong)(this.IR.ColSet.top & 16777215U)))
					.PtcST("reel_mbox_decide", null, PTCThread.StFollow.NO_FOLLOW, null);
			}
		}

		public ReelMBoxDrawer deactivate()
		{
			if (this.af >= 0f)
			{
				this.af = X.Mn(-30f + this.af, -1f);
				if (this.PtcOpen != null)
				{
					this.PtcOpen.kill(false);
					this.PtcOpen = null;
				}
			}
			return this;
		}

		public bool isActive()
		{
			return this.af >= 0f;
		}

		public bool getEffectReposition(PTCThread St, PTCThread.StFollow follow, float fcnt, out Vector3 V)
		{
			V = new Vector3(this.x * 0.015625f, this.y * 0.015625f, 0f);
			return true;
		}

		public bool isSoundActive(SndPlayer S)
		{
			M2SoundPlayerItem m2SoundPlayerItem = S as M2SoundPlayerItem;
			return m2SoundPlayerItem != null && TX.isStart(m2SoundPlayerItem.key, this.getSoundKey(), 0);
		}

		public string getSoundKey()
		{
			return "ReelMBoxDrawer";
		}

		public bool readPtcScript(PTCThread Thread)
		{
			return false;
		}

		public bool initSetEffect(PTCThread Thread, EffectItem Ef)
		{
			return true;
		}

		private ReelManager Con;

		private UiReelManager Ui;

		public readonly ReelManager.ItemReelContainer IR;

		private static EfParticleOnce PtcKira;

		private float x;

		private float y;

		public float dx;

		public float dy;

		private bool pos_check;

		private float af;

		private int index;

		private float t_open = -1f;

		private NelMBoxDrawer Dr;

		private UniBrightDrawer UbDr;

		private MeshDrawer MdAdd;

		private PTCThread PtcOpen;

		private bool outpos;
	}
}
