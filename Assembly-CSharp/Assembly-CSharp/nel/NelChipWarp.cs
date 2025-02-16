using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelChipWarp : NelChipWithEffect, IActivatable, ILinerReceiver, IRunAndDestroy, IPuzzActivationListener, IBCCFootListener
	{
		public NelChipWarp(M2MapLayer _Lay, int drawx, int drawy, int opacity, int rotation, bool flip, M2ChipImage _Img = null)
			: base(_Lay, drawx, drawy, opacity, rotation, flip, _Img)
		{
			this.set_effect_on_initaction = false;
			this.effect_2_ed = true;
			NelChipWarp.getCrystalSq();
			this.ran0 = (uint)X.xors(256);
			NelChipWarp.getWarpDrawer(ref NelChipWarp.MtrBg);
		}

		public static WarpDoorDrawer getWarpDrawer(ref Material _MtrBg)
		{
			if (NelChipWarp.DrwWarp == null)
			{
				NelChipWarp.DrwWarp = new WarpDoorDrawer(50f, 50f, 2);
				NelChipWarp.MtrBg = M2DBase.newMtr("m2d/ShaderGDTFilling");
			}
			_MtrBg = NelChipWarp.MtrBg;
			return NelChipWarp.DrwWarp;
		}

		public static void fineWarpColor()
		{
			if (NelChipWarp.DrwWarp != null && NelChipWarp.need_fine_color)
			{
				C32 cola = MTRX.cola;
				NelChipWarp.DrwWarp.LineColorIn = cola.Set(4282007792U).blend(4294932429U, X.COSI((float)(IN.totalframe + 90), 93f)).C;
				NelChipWarp.DrwWarp.LineColorOut = cola.Set(4282007792U).blend(4294932429U, X.COSI((float)(IN.totalframe + 290), 134f)).C;
				NelChipWarp.need_fine_color = false;
			}
		}

		public static PxlSequence getCrystalSq()
		{
			if (NelChipWarp.SqAnim == null)
			{
				NelChipWarp.SqAnim = MTRX.PxlIcon.getPoseByName("crystal_puzzle_worp").getSequence(0);
			}
			return NelChipWarp.SqAnim;
		}

		public override void initAction(bool normal_map)
		{
			base.initAction(normal_map);
			this.first_activation = base.Meta.GetI("warp_transporter", 0, 0) != 0;
			this.LpPma = PUZ.IT.isBelongTo(this);
			if (this.MvEvent == null)
			{
				M2EventItem m2EventItem = (this.MvEvent = this.Mp.getEventContainer().CreateAndAssign(base.unique_key));
				m2EventItem.setToArea(this.mleft, this.mtop, this.mright - this.mleft, this.mbottom - this.mtop);
				m2EventItem.assign("TALK", NelChipWarp.getEventContent(this.mapcx, (int)(this.mbottom + 0.25f), (-1).ToString() ?? ""), true);
				m2EventItem.check_desc_name = "EV_access_warp_oneway";
			}
			this.fineActivation();
			this.Mp.addBCCFootListener(this);
		}

		public override void closeAction(bool when_map_close, bool do_not_remove_drawer)
		{
			base.closeAction(when_map_close, do_not_remove_drawer);
			this.Mp.remBCCFootListener(this);
			this.destruct();
		}

		public void activateLiner(bool immediate)
		{
			this.has_liner = true;
			this.fineActivation();
		}

		public void deactivateLiner(bool immediate)
		{
			this.has_liner = false;
			this.fineActivation();
		}

		public void changePuzzleActivation(bool activated)
		{
			this.has_puzz_activation = activated;
			this.fineActivation();
		}

		public void fineActivation()
		{
			if (this.is_active)
			{
				this.activate();
				return;
			}
			this.deactivate();
		}

		public void activate()
		{
			if (!this.is_runner_assigned)
			{
				this.is_runner_assigned = true;
				this.Mp.addRunnerObject(this);
				if (this.Ed == null)
				{
					this.setEffect();
				}
			}
			this.MvEvent.setExecutable(M2EventItem.CMD.TALK, true);
		}

		public void deactivate()
		{
			this.MvEvent.setExecutable(M2EventItem.CMD.TALK, false);
		}

		public bool run(float fcnt)
		{
			bool flag = this.is_active && NelChipWarp.isPlayerFooting(this.NearPR, this);
			if (flag)
			{
				NelChipBarrierLit nelChipBarrierLit = (base.M2D as NelM2DBase).CheckPoint.get_CurCheck() as NelChipBarrierLit;
				if (nelChipBarrierLit != null)
				{
					nelChipBarrierLit.warp_target_s0 = this.Mp.floort + 60f;
					if (this.t >= 0f)
					{
						nelChipBarrierLit.warp_appear_t = this.t;
					}
				}
				else
				{
					flag = false;
				}
			}
			if (!NelChipWarp.runWarpPortal(ref this.t, this, fcnt, flag) && !this.is_active)
			{
				this.is_runner_assigned = false;
				if (this.Ed != null)
				{
					this.Ed.destruct();
					this.Ed = null;
				}
				return false;
			}
			return true;
		}

		public static bool runWarpPortal(ref float t, M2Chip Cp, float fcnt, bool player_footing)
		{
			if (t >= 0f)
			{
				if (!player_footing)
				{
					if (t >= 1f)
					{
						t = -1f;
						if (NelChipWarp.reduce_release_effect < Cp.Mp.floort)
						{
							NelChipWarp.PtcST(Cp, "warp_portal_close");
						}
					}
					else
					{
						t = -33f;
					}
				}
			}
			else if (player_footing)
			{
				t = 0f;
			}
			if (t > -33f)
			{
				NelChipWarp.fineWarpColor();
			}
			if (t >= 0f)
			{
				if (t < 40f)
				{
					float num = t + fcnt;
					if (t < 1f && num >= 1f)
					{
						NelChipWarp.PtcST(Cp, "warp_portal_open");
					}
					if (t < 18f && num >= 18f)
					{
						NelChipWarp.PtcST(Cp, "warp_portal_open2");
					}
					t = X.Mn(num, 40f);
				}
			}
			else if (t < 0f)
			{
				if (t <= -33f)
				{
					t = -33f;
					return false;
				}
				t -= fcnt;
			}
			return true;
		}

		public void destruct()
		{
			if (this.Ed != null)
			{
				this.Ed.destruct();
				this.Ed = null;
			}
			if (this.is_runner_assigned)
			{
				this.is_runner_assigned = false;
				this.Mp.remRunnerObject(this);
			}
			if (this.MvEvent != null)
			{
				this.MvEvent.destruct();
			}
			this.MvEvent = null;
			PUZ.IT.removePaListener(this);
		}

		public bool is_active
		{
			get
			{
				return (this.LpPma == null || this.has_puzz_activation) && this.has_liner != this.first_activation;
			}
		}

		public uint getFootableAimBits()
		{
			return 8U;
		}

		public DRect getMapBounds(DRect BufRc)
		{
			return BufRc.Set(this.mleft, this.mtop - 0.5f, (float)this.iwidth * base.rCLEN, (float)this.iheight * base.rCLEN + 1f);
		}

		public virtual bool footedInit(M2BlockColliderContainer.BCCLine Bcc, IMapDamageListener Fd)
		{
			if (Fd.isCenterPr())
			{
				this.NearPR = base.M2D.Cam.getBaseMover() as M2Attackable;
				return true;
			}
			return false;
		}

		public bool footedQuit(IMapDamageListener Fd, bool from_jump_init = false)
		{
			return true;
		}

		public void rewriteFootType(M2BlockColliderContainer.BCCLine Bcc, IMapDamageListener Fd, ref string s)
		{
		}

		public static bool isPlayerFooting(M2Attackable NearPR, M2Chip Cp)
		{
			return NearPR != null && X.BTW(Cp.mtop - 0.85f, NearPR.mbottom, Cp.mbottom + 0.125f) && X.BTW(Cp.mleft - 1.5f, NearPR.x, Cp.mright + 1.5f);
		}

		public bool initEffect(bool activating, ref DRect RcEffect)
		{
			if (RcEffect != null)
			{
				RcEffect.x = this.mleft;
				RcEffect.y = this.mtop;
				RcEffect.width = (float)this.iwidth / base.CLEN;
				RcEffect.height = (float)this.iheight / base.CLEN;
			}
			return true;
		}

		public static void PtcST(M2Chip Cp, string ptcst_name)
		{
			Vector2 vector = Cp.PixelToMapPoint(16f, 36f);
			Cp.Mp.PtcSTsetVar("x", (double)vector.x).PtcSTsetVar("y", (double)(Cp.mbottom - 172f * Cp.rCLENB * 0.5f)).PtcSTsetVar("w", 108.0)
				.PtcSTsetVar("h", 172.0)
				.PtcST(ptcst_name, null, PTCThread.StFollow.NO_FOLLOW);
		}

		protected override bool fnDrawOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			base.fnDrawOnEffect(Ef, Ed);
			if (!Ed.isinCamera(Ef, 108f * base.rCLENB, 202f * base.rCLENB))
			{
				return true;
			}
			Ef.x = base.M2D.Cam.x * this.Mp.rCLEN;
			Ef.y = base.M2D.Cam.y * this.Mp.rCLEN;
			Vector2 vector = Vector2.zero;
			bool flag = false;
			if (this.is_active && this.t < 40f)
			{
				MeshDrawer meshImg = Ef.GetMeshImg("", MTRX.MIicon, BLEND.ADD, true);
				meshImg.base_z += 1.5E-05f;
				meshImg.Col = meshImg.ColGrd.White().mulA((this.t > 0f) ? (1f - X.ZLINE(this.t - 18f, 22f)) : X.ZLINE(-this.t, 28f)).C;
				int num = NelChipWarp.SqAnim.countFrames() - 7;
				PxlFrame frame = NelChipWarp.SqAnim.getFrame(7 + X.ANMT(num, 8f));
				if (!flag)
				{
					vector = base.PixelToMapPoint(16f, 36f);
					vector.x = (vector.x - Ef.x) * this.Mp.CLENB;
					vector.y = -(vector.y - Ef.y) * this.Mp.CLENB;
					flag = true;
				}
				meshImg.RotaPF(vector.x, vector.y, this.Mp.base_scale, this.Mp.base_scale, this.draw_rotR, frame, this.flip, false, false, uint.MaxValue, false, 0);
			}
			if (this.t > -33f)
			{
				if (!flag)
				{
					vector = base.PixelToMapPoint(16f, 36f);
					vector.x = (vector.x - Ef.x) * this.Mp.CLENB;
					vector.y = -(vector.y - Ef.y) * this.Mp.CLENB;
				}
				NelChipWarp.fnDrawOnEffectWarp(this, vector, this.t, this.ran0, Ef, Ed);
			}
			return true;
		}

		public static bool fnDrawOnEffectWarp(M2Chip Cp, Vector2 PCen, float t, uint ran0, EffectItem Ef, M2DrawBinder Ed)
		{
			M2DBase m2D = Cp.M2D;
			Map2d mp = Cp.Mp;
			float num = -(Cp.mbottom - Ef.y) * mp.CLENB;
			float num2 = num + 172f;
			MeshDrawer meshDrawer = null;
			RenderTexture renderTexture = null;
			float num3 = 1f;
			NelChipWarp.getWarpDrawer(ref NelChipWarp.MtrBg);
			if (t >= 18f)
			{
				renderTexture = m2D.Cam.GetCameraCollecter(0).Cam.targetTexture;
				if (renderTexture != null)
				{
					NelChipWarp.MtrBg.SetTexture("_MainTex", renderTexture);
					num3 = 0.875f + 0.0625f * X.COSI(mp.floort, 38f) + 0.0625f * X.COSI(mp.floort, 91f);
					NelChipWarp.MtrBg.SetColor("_Color", MTRX.cola.Set(m2D.Cam.transparent_color).multiply(num3, false).setA1(1f)
						.C);
					meshDrawer = Ef.GetMesh("", NelChipWarp.MtrBg, true);
					meshDrawer.base_z += 1E-05f;
				}
			}
			if (t >= 0f && t < 18f)
			{
				MeshDrawer mesh = Ef.GetMesh("", MTRX.getMtr(BLEND.ADD, -1), true);
				mesh.Col = MTRX.ColWhite;
				float num4 = X.ZSIN(t, 18f);
				num = X.NI(PCen.y, num, num4);
				num2 = X.NI(PCen.y, num2, num4);
				mesh.RectBL(PCen.x - 1f, num, 2f, num2 - num, false);
			}
			else
			{
				MeshDrawer mesh2 = Ef.GetMesh("", MTRX.getMtr(BLEND.ADD, -1), true);
				NelChipWarp.need_fine_color = true;
				NelChipWarp.DrwWarp.ran0 = ran0;
				float num5;
				if (t >= 0f)
				{
					num5 = X.ZSIN2(t - 18f, 22f);
				}
				else
				{
					num5 = 1f - X.ZPOW(-t, 33f);
				}
				NelChipWarp.DrwWarp.BaseColor = mesh2.ColGrd.White().mulA(X.ZLINE(1f - num5)).C;
				NelChipWarp.DrwWarp.Radius(num5 * 30f, 45f).WH(num5 * 108f, 172f);
				int num6 = NelChipWarp.DrwWarp.drawTo(mesh2, mp.floort, PCen.x, num + 86f);
				if (meshDrawer != null && num6 > 0)
				{
					NelChipWarp.shakeWarpBehindTexture(mp, mesh2, meshDrawer, renderTexture, NelChipWarp.DrwWarp, PCen.x * 0.015625f, PCen.y * 0.015625f, num6, num3, 1f, Cp.index);
				}
			}
			return true;
		}

		public static void shakeWarpBehindTexture(Map2d Mp, MeshDrawer Md, MeshDrawer MdBg, RenderTexture RdBg, WarpDoorDrawer DrwWarp, float cx_u, float cy_u, int vcnt, float bg_lighten, float alpha, int cp_index)
		{
			Vector3[] vertexArray = Md.getVertexArray();
			int num = Md.getVertexMax() - vcnt;
			int directionMax = DrwWarp.getDirectionMax();
			MdBg.uvRect(MdBg.base_x + (IN.wh + 8f) * 0.015625f, MdBg.base_y + (IN.hh + 8f) * 0.015625f, -(IN.w + 16f) * 0.015625f, -(IN.h + 16f) * 0.015625f, true, false);
			MdBg.base_x = (MdBg.base_y = 0f);
			int num2 = MdBg.getVertexMax();
			MdBg.allocVer(num2 + directionMax, 1);
			float num3 = 1f / (float)RdBg.width;
			float num4 = 1f / (float)RdBg.height;
			MdBg.Col = MdBg.ColGrd.White().multiply(bg_lighten, false).mulA(alpha)
				.C;
			for (int i = 0; i < directionMax; i++)
			{
				Vector3 vector = vertexArray[num + i];
				MdBg.Pos(vector.x, vector.y, null);
				Vector2[] uvArray = MdBg.getUvArray();
				float directionR = DrwWarp.getDirectionR(i);
				vector = uvArray[num2];
				uint ran = X.GETRAN2(i + 73 + cp_index * 11, 13 + i % 4);
				float num5 = X.NI(3, 7, X.RAN(ran, 2812)) * X.COSI(Mp.floort, X.NI(160, 200, X.RAN(ran, 2679)));
				vector.x += num5 * X.Cos(directionR) * num3;
				vector.y += num5 * X.Sin(directionR) * num4;
				uvArray[num2] = vector;
				num2++;
			}
			DrwWarp.makeFillTri(MdBg, directionMax, Md.base_x + cx_u, Md.base_y + cy_u);
		}

		public string getActivateKey()
		{
			return base.unique_key;
		}

		public static string getEventContent(float mapcx, int mbottom, string warp_id_content)
		{
			return string.Concat(new string[]
			{
				"STOP_LETTERBOX\nVALOTIZE\nDENY_SKIP\nTE_COLORBLINK_ % ",
				30f.ToString(),
				" 30 1 0xffffff 15\n#MS_ % '>[ ",
				((int)(mapcx * M2DBase.Instance.CLEN)).ToString(),
				",+=0 :",
				45f.ToString(),
				" ]'\nWAIT_MOVE\nPUZZ_WARP ",
				warp_id_content,
				"\nWAIT 2"
			});
		}

		private bool has_liner;

		private bool has_puzz_activation;

		private bool first_activation;

		private bool is_runner_assigned;

		private float t = -33f;

		public const float T_FADEIN = 40f;

		public const float T_FADEOUT = 33f;

		private const float portal_center_chip_x = 16f;

		private const float portal_center_chip_y = 36f;

		private const float maxw = 108f;

		private const float maxh = 172f;

		public static float reduce_release_effect;

		public M2Attackable NearPR;

		private static PxlSequence SqAnim;

		private M2LpPuzzManageArea LpPma;

		private readonly uint ran0;

		private M2EventItem MvEvent;

		public const int extending_h_time = 18;

		public static bool need_fine_color = true;

		private static WarpDoorDrawer DrwWarp;

		private static Material MtrBg;

		public const float event_walk_t = 45f;
	}
}
