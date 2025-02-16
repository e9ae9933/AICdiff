using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelChipJumperBoard : NelChip, IBCCEventListener, IBCCFootListener, IRunAndDestroy
	{
		public NelChipJumperBoard(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			base.arrangeable = true;
			if (NelChipJumperBoard.OCon == null)
			{
				NelChipJumperBoard.OCon = new BDic<string, NelChipJumperBoard.JFrameContainer>(1);
			}
			if (this.Img.SourceLayer == null || !this.Img.SourceLayer.isImport())
			{
				X.dl("JumperBoard にはインポートレイヤーが必要です", null, false, true);
				return;
			}
			PxlLayer importSource = this.Img.SourceLayer.getImportSource();
			string title = importSource.pFrm.pPose.title;
			if (!NelChipJumperBoard.OCon.TryGetValue(title, out this.FCon))
			{
				this.FCon = (NelChipJumperBoard.OCon[title] = new NelChipJumperBoard.JFrameContainer(this.Mp, importSource.pFrm.pSq));
			}
			this.ARideFootD = new List<IMapDamageListener>(1);
		}

		public override void initAction(bool normal_map)
		{
			if (base.active_closed || this.FCon == null || this.Lay.is_fake)
			{
				return;
			}
			base.initAction(normal_map);
			if (this.CB != null)
			{
				this.CB.destruct();
			}
			this.CB = new BCCLineChipBased(this, AIM.B, true)
			{
				assign_runner = false,
				y_follow_speed = 1f,
				ARider = this.ARideFootD
			};
			this.cframe_ = 0;
			this.CB.y_follow_speed = 1f;
			this.t_charge = -1f;
			this.t_spring = 0f;
			this.pump_velocity = this.Img.Meta.GetNm("jumper", 0.35f, 0);
			this.pump_velocity_min = this.Img.Meta.GetNm("jumper", this.pump_velocity * 0.33f, 1);
			this.jumper_charge_ptc = this.Img.Meta.GetS("jumper_charge_ptc");
			this.jumper_pump_ptc = this.Img.Meta.GetS("jumper_pump_ptc");
			if (this.Dr != null)
			{
				this.Dr.need_reposit_flag = true;
			}
			Vector2Int shift = this.getShift();
			switch (this.rotation)
			{
			case 0:
				this.shift_mesh_drawy = base.CLEN - (float)shift.y;
				this.shift_mesh_drawx = (float)this.iwidth * 0.5f;
				break;
			case 1:
				this.shift_mesh_drawy = (float)this.iheight * 0.5f;
				this.shift_mesh_drawx = (float)(-(float)shift.x);
				break;
			case 2:
				this.shift_mesh_drawy = (float)(-(float)shift.y);
				this.shift_mesh_drawx = (float)this.iwidth * 0.5f;
				break;
			default:
				this.shift_mesh_drawx = base.CLEN - (float)shift.x;
				this.shift_mesh_drawy = (float)this.iheight * 0.5f;
				break;
			}
			this.CB.initAction();
			this.Mp.addRunnerObject(this);
			this.fineBoardPosition();
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			if (this.CB != null)
			{
				this.CB.closeAction();
			}
			if (this.cframe >= 0)
			{
				this.cframe_ = -1;
				this.Mp.remRunnerObject(this);
			}
		}

		public void destruct()
		{
		}

		public void BCCInitializing(M2BlockColliderContainer BCC = null)
		{
		}

		public bool isBCCListenerActive(M2BlockColliderContainer.BCCLine BCC)
		{
			return BCC == this.Bcc;
		}

		public void BCCtouched(M2BlockColliderContainer.BCCLine BCC, M2FootManager FootD)
		{
		}

		public bool runBCCEvent(M2BlockColliderContainer.BCCLine BCC, M2FootManager FootD)
		{
			return !base.active_closed;
		}

		public uint getFootableAimBits()
		{
			return 1U << (int)this.Bcc.foot_aim;
		}

		public DRect getMapBounds(DRect BufRc)
		{
			if (base.active_closed)
			{
				return null;
			}
			BufRc.Set((float)this.drawx * base.rCLEN, (float)this.drawy * base.rCLEN, (float)this.clms, (float)this.rows);
			return BufRc;
		}

		public bool footedInit(M2BlockColliderContainer.BCCLine Bcc, IMapDamageListener FootD)
		{
			if (base.active_closed || this.Dr == null)
			{
				return false;
			}
			if (this.t_charge >= this.FCon.t_pump_init || this.t_charge < -1f)
			{
				return false;
			}
			if (!this.isCharging())
			{
				this.cframe = 1;
				this.t_charge = 0f;
				if (TX.valid(this.jumper_charge_ptc))
				{
					this.Mp.PtcSTsetVar("cx", (double)this.mapcx).PtcSTsetVar("cy", (double)this.mapcy).PtcSTsetVar("_xd", (double)CAim._XD(Bcc.aim, 1))
						.PtcSTsetVar("_yd", (double)(-(double)CAim._YD(Bcc.aim, 1)));
					this.Mp.PtcST(this.jumper_charge_ptc, null, PTCThread.StFollow.NO_FOLLOW);
				}
			}
			if (this.ARideFootD.IndexOf(FootD) < 0)
			{
				this.ARideFootD.Add(FootD);
			}
			return true;
		}

		public bool footedQuit(IMapDamageListener FootD, bool from_jump_init = false)
		{
			int num = this.ARideFootD.IndexOf(FootD);
			if (num >= 0)
			{
				this.ARideFootD.RemoveAt(num);
				if (from_jump_init && this.t_charge >= 0f && (this.t_charge >= this.FCon.t_pump_init || this.ARideFootD.Count == 0))
				{
					float num2 = X.NI(0f, this.pump_velocity, X.ZSINV(this.t_charge - 4f, this.FCon.t_pump_init - 11f));
					FootD.applyVelocity(FOCTYPE.WALK, (float)(-(float)CAim._XD(this.Bcc.aim, 1)) * num2, (float)CAim._YD(this.Bcc.aim, 1) * num2);
					this.t_charge = X.Mx(this.t_charge, this.FCon.t_pump_init);
				}
			}
			return true;
		}

		public void rewriteFootType(M2BlockColliderContainer.BCCLine Bcc, IMapDamageListener Fd, ref string s)
		{
		}

		private float draw_basex
		{
			get
			{
				return (float)this.drawx + this.shift_mesh_drawx;
			}
		}

		private float draw_basey
		{
			get
			{
				return (float)this.drawy + this.shift_mesh_drawy;
			}
		}

		private float draw_cx
		{
			get
			{
				return this.draw_basex - (float)this.Bcc._xd * this.FCon.base_y;
			}
		}

		private float draw_cy
		{
			get
			{
				return this.draw_basey + (float)this.Bcc._yd * this.FCon.base_y;
			}
		}

		public int cframe
		{
			get
			{
				return this.cframe_;
			}
			set
			{
				if (this.cframe_ == value)
				{
					return;
				}
				this.cframe_ = value;
				this.CB.y_follow_speed = ((this.cframe_ <= 0) ? 1f : (this.isCharging() ? 0.24f : 0.55f));
				if (this.cframe_ < 0)
				{
					return;
				}
				this.Dr.need_reposit_flag = true;
				this.fineBoardPosition();
				this.t_spring = ((this.cframe_ == 0) ? 0f : (this.t_spring - (float)this.FCon.rest_crf60(this.cframe)));
			}
		}

		private void fineBoardPosition()
		{
			Vector2Int shift = this.Img.getShift(0, false);
			this.CB.shift_mapy = ((float)(-(float)shift.y) - (float)this.Img.iheight * 0.5f) * base.rCLEN + 1f - this.getCurrentJFrame().board_y;
		}

		public override void translateByChipMover(float ux, float uy, C32 AddCol, int drawx0, int drawy0, int move_drawx = 0, int move_drawy = 0, bool stabilize_move_map = false)
		{
			base.translateByChipMover(ux, uy, AddCol, drawx0, drawy0, move_drawx, move_drawy, stabilize_move_map);
			if (this.CB != null)
			{
				this.CB.positionUpdatedByChipMover();
			}
		}

		public bool run(float fcnt)
		{
			if (base.active_closed || this.Bcc == null)
			{
				return false;
			}
			if (this.Dr == null)
			{
				return true;
			}
			this.CB.run(fcnt);
			if (this.cframe > 0)
			{
				float num = fcnt;
				if (this.t_charge >= 0f)
				{
					this.t_charge += fcnt;
					if (this.t_charge >= this.FCon.t_pump_init + 2f)
					{
						this.t_charge = -1f;
						int count = this.ARideFootD.Count;
						int i = this.ARideFootD.Count - 1;
						while (i >= 0)
						{
							IMapDamageListener mapDamageListener = this.ARideFootD[i];
							if (!(mapDamageListener is M2FootManager))
							{
								goto IL_00C8;
							}
							M2FootManager m2FootManager = mapDamageListener as M2FootManager;
							m2FootManager.initJump(false, true, false);
							if (m2FootManager.Mv.canApplyCarryVelocity())
							{
								goto IL_00C8;
							}
							IL_0106:
							i--;
							continue;
							IL_00C8:
							mapDamageListener.applyVelocity(FOCTYPE.JUMP, (float)(-(float)CAim._XD(this.Bcc.aim, 1)) * this.pump_velocity_min, (float)CAim._YD(this.Bcc.aim, 1) * this.pump_velocity_min);
							goto IL_0106;
						}
						this.ARideFootD.Clear();
					}
					if (this.t_charge >= this.FCon.t_pump_init && this.cframe_ < this.FCon.pump_frame)
					{
						num = (this.t_spring = 0f);
						this.cframe_ = this.FCon.pump_frame - 1;
					}
				}
				this.t_spring += num;
				if (this.t_spring >= 0f)
				{
					if (this.cframe_ >= this.FCon.max_frame - 1)
					{
						this.t_charge = -1f;
						this.cframe = 0;
					}
					else
					{
						this.cframe = this.cframe_ + 1;
						if (this.cframe == this.FCon.pump_frame && TX.valid(this.jumper_pump_ptc))
						{
							this.Mp.PtcSTsetVar("cx", (double)this.mapcx).PtcSTsetVar("cy", (double)this.mapcy).PtcSTsetVar("_xd", (double)CAim._XD(this.Bcc.aim, 1))
								.PtcSTsetVar("_yd", (double)(-(double)CAim._YD(this.Bcc.aim, 1)));
							this.Mp.PtcST(this.jumper_pump_ptc, null, PTCThread.StFollow.NO_FOLLOW);
						}
					}
				}
			}
			return true;
		}

		protected override M2CImgDrawer CreateDrawer(ref MeshDrawer Md, int lay, METACImg Meta, M2CImgDrawer Pre_Drawer)
		{
			if (this.FCon != null && this.Img.mesh_layer == lay)
			{
				return this.Dr = new NelChipJumperBoard.M2CImgDrawerJumperBoard(Md, lay, this);
			}
			return base.CreateDrawer(ref Md, lay, Meta, Pre_Drawer);
		}

		public bool isCharging()
		{
			return this.cframe > 0 && this.t_charge >= 0f;
		}

		private NelChipJumperBoard.JFrame getCurrentJFrame()
		{
			if (this.FCon == null)
			{
				return null;
			}
			return this.FCon.getFrame(X.Mx(0, this.cframe));
		}

		public bool isPumpedOut()
		{
			return this.cframe >= this.FCon.pump_frame;
		}

		private M2BlockColliderContainer.BCCLine Bcc
		{
			get
			{
				if (this.CB == null)
				{
					return null;
				}
				return this.CB.Bcc;
			}
		}

		private BCCLineChipBased CB;

		private static BDic<string, NelChipJumperBoard.JFrameContainer> OCon;

		private NelChipJumperBoard.JFrameContainer FCon;

		public const float MAXT_CHARGE = 15f;

		private int cframe_ = -1;

		private float t_spring;

		private float t_charge;

		private NelChipJumperBoard.M2CImgDrawerJumperBoard Dr;

		private float shift_mesh_drawx;

		private float shift_mesh_drawy;

		private float pump_velocity;

		private float pump_velocity_min;

		private string jumper_charge_ptc;

		private string jumper_pump_ptc;

		private const float T_PUMP_MARGIN = 2f;

		private List<IMapDamageListener> ARideFootD;

		private class JFrame
		{
			public JFrame(Map2d Mp, PxlFrame _Source, float base_y)
			{
				this.Source = _Source;
				int num = this.Source.countLayers();
				bool flag = false;
				for (int i = 0; i < num; i++)
				{
					PxlLayer layer = this.Source.getLayer(i);
					if (TX.isStart(layer.name, "jumper", 0))
					{
						flag = true;
						this.board_y = base_y - layer.rot_center_y;
						break;
					}
				}
				this.board_y *= Mp.rCLEN;
				if (!flag)
				{
					X.dl("jumper レイヤーが不明:" + this.Source.ToString(), null, false, false);
					this.Source = null;
					return;
				}
			}

			public bool valid
			{
				get
				{
					return this.Source != null;
				}
			}

			public readonly PxlFrame Source;

			public readonly float board_y;
		}

		private class JFrameContainer
		{
			public JFrameContainer(Map2d Mp, PxlSequence Sq)
			{
				this.max_frame = Sq.countFrames();
				this.AFrm = new List<NelChipJumperBoard.JFrame>(this.max_frame);
				this.base_y = (float)Sq.height * 0.5f;
				int i = 0;
				while (i < this.max_frame)
				{
					PxlFrame frame = Sq.getFrame(i);
					if (this.layers == 0)
					{
						this.layers = frame.countLayers();
						goto IL_0084;
					}
					if (frame.countLayers() == this.layers)
					{
						goto IL_0084;
					}
					X.dl("レイヤー数が不定:" + frame.ToString(), null, false, false);
					IL_00F0:
					i++;
					continue;
					IL_0084:
					NelChipJumperBoard.JFrame jframe = new NelChipJumperBoard.JFrame(Mp, frame, this.base_y);
					if (jframe.valid)
					{
						if (this.pump_frame == 0)
						{
							if (frame.name == "pump")
							{
								this.pump_frame = this.AFrm.Count;
							}
							else if (i > 0)
							{
								this.t_pump_init += (float)jframe.Source.crf60;
							}
						}
						this.AFrm.Add(jframe);
						goto IL_00F0;
					}
					goto IL_00F0;
				}
				this.max_frame = this.AFrm.Count;
			}

			public int rest_crf60(int frame)
			{
				if (frame >= 0 && frame < this.max_frame)
				{
					return this.AFrm[frame].Source.crf60;
				}
				return 0;
			}

			public NelChipJumperBoard.JFrame getFrame(int frame)
			{
				if (frame >= 0 && frame < this.max_frame)
				{
					return this.AFrm[frame];
				}
				return null;
			}

			public readonly int pump_frame;

			public readonly float t_pump_init;

			public readonly int max_frame;

			public readonly int layers;

			public float base_y;

			private List<NelChipJumperBoard.JFrame> AFrm;
		}

		private class M2CImgDrawerJumperBoard : M2CImgDrawer
		{
			public M2CImgDrawerJumperBoard(MeshDrawer Md, int _layer, M2Puts _Cp)
				: base(Md, _layer, _Cp, true)
			{
				this.Con = _Cp as NelChipJumperBoard;
			}

			protected override void entryMainPicToMeshInner(MeshDrawer Md, float meshx, float meshy, float _zmx, float _zmy, float _rotR, PxlMeshDrawer Ms)
			{
				if (this.entryMainPicToMeshInner(false))
				{
					this.spring_draw = true;
					return;
				}
				this.spring_draw = false;
				base.entryMainPicToMeshInner(Md, meshx, meshy, _zmx, _zmy, _rotR, Ms);
			}

			private bool entryMainPicToMeshInner(bool redrawing)
			{
				if (this.Con.FCon != null)
				{
					NelChipJumperBoard.JFrame currentJFrame = this.Con.getCurrentJFrame();
					if (this.Con.Bcc != null)
					{
						base.RotaPF(base.Mp.pixel2meshx(this.Con.draw_cx), base.Mp.pixel2meshy(this.Con.draw_cy), currentJFrame.Source, redrawing, false, false, uint.MaxValue, false);
					}
					else
					{
						base.RotaPF(base.Mp.pixel2meshx(this.Con.draw_basex), base.Mp.pixel2meshy(this.Con.draw_basey), currentJFrame.Source, redrawing, false, false, uint.MaxValue, false);
					}
					return true;
				}
				return false;
			}

			public override void translateByChipMover(float ux, float uy, C32 AddCol)
			{
				this.need_reposit_flag = true;
				base.Mp.addUpdateMesh(this.redraw(0f), false);
			}

			public override int redraw(float fcnt)
			{
				if (this.spring_draw && this.Con.CB != null && this.need_reposit_flag)
				{
					this.entryMainPicToMeshInner(true);
					this.need_reposit_flag = false;
					return base.layer2update_flag;
				}
				return base.redraw(fcnt);
			}

			private NelChipJumperBoard Con;

			private bool spring_draw;
		}
	}
}
