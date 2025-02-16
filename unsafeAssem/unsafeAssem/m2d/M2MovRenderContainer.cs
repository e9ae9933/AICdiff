using System;
using System.Collections.Generic;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2MovRenderContainer
	{
		public M2MovRenderContainer(M2Camera _Cam)
		{
			this.Cam = _Cam;
			this.AADob = new List<M2RenderTicket>[4];
			this.Aneed_sort = new bool[4];
			for (int i = 0; i < 4; i++)
			{
				this.AADob[i] = new List<M2RenderTicket>(2);
			}
			this.ADobBuf = new List<M2RenderTicket>(8);
			if (M2MovRenderContainer.MdInsert == null)
			{
				MeshDrawer meshDrawer = (M2MovRenderContainer.MdInsert = new MeshDrawer(null, 4, 6));
				meshDrawer.draw_gl_only = true;
				meshDrawer.activate("for_mover", M2DBase.newMtr("m2d/ShaderGDTForMover"), true, MTRX.ColWhite, null);
			}
		}

		public void initS(Map2d _Mp)
		{
			this.pre_floort = 0f;
			this.Mp = _Mp;
			this.binding |= 4080U;
			for (int i = 0; i < 4; i++)
			{
				this.AADob[i].Clear();
			}
		}

		public void clearCameraComponent()
		{
			for (int i = 0; i < 4; i++)
			{
				List<M2RenderTicket> list = this.AADob[i];
				int count = list.Count;
				for (int j = 0; j < count; j++)
				{
					list[j].releaseFromGlRendering();
				}
			}
			if (this.BindB != null)
			{
				this.Cam.deassignRenderFunc(this.BindB, M2MovRenderContainer.drawer_t_layer);
				this.binding |= 1U;
			}
			if (this.BindT != null)
			{
				this.Cam.deassignRenderFunc(this.BindT, M2MovRenderContainer.drawer_t_layer);
				this.binding |= 4U;
			}
			if (this.BindBf != null)
			{
				this.Cam.deassignRenderFunc(this.BindBf, M2MovRenderContainer.drawer_mask_layer);
				this.binding |= 2U;
			}
			if (this.BindCM != null)
			{
				this.Cam.deassignRenderFunc(this.BindCM, M2MovRenderContainer.draw_cm_layer);
				this.binding |= 8U;
			}
			if ((this.binding & 4096U) != 0U)
			{
				this.binding &= 4294963199U;
			}
			this.BindB = (this.BindT = (this.BindBf = (this.BindCM = null)));
		}

		public void initCameraFinalize(CameraComponentCollecter[] ACam, XCameraBase[] AXc)
		{
			if (this.Cam.stabilize_drawing)
			{
				return;
			}
			CameraComponentCollecter moverCameraCC = this.Cam.getMoverCameraCC();
			if (moverCameraCC == null)
			{
				return;
			}
			this.pre_floort = 0f;
			Camera cam = moverCameraCC.Cam;
			M2MovRenderContainer.MdInsert.clear(false, false);
			M2MovRenderContainer.MdInsert.initForImgAndTexture(cam.targetTexture);
			M2MovRenderContainer.MdInsert.DrawCen(0f, 0f, null);
			if ((this.binding & 1U) != 0U)
			{
				this.bindDrawer2Cam(0);
			}
			if ((this.binding & 4U) != 0U)
			{
				this.bindDrawer2Cam(2);
			}
			if ((this.binding & 2U) != 0U)
			{
				this.bindDrawer2Cam(1);
			}
			if ((this.binding & 8U) != 0U)
			{
				this.bindDrawer2Cam(3);
			}
			for (int i = 0; i < 4; i++)
			{
				List<M2RenderTicket> list = this.AADob[i];
				int count = list.Count;
				for (int j = 0; j < count; j++)
				{
					list[j].initGlRendering();
				}
			}
		}

		private void bindDrawer2Cam(int i)
		{
			CameraRenderBinderFunc cameraRenderBinderFunc;
			switch (i)
			{
			case 0:
				cameraRenderBinderFunc = this.BindB;
				break;
			case 1:
				cameraRenderBinderFunc = this.BindBf;
				break;
			case 2:
				cameraRenderBinderFunc = this.BindT;
				break;
			default:
				cameraRenderBinderFunc = this.BindCM;
				break;
			}
			if (cameraRenderBinderFunc == null)
			{
				CameraRenderBinderFunc cameraRenderBinderFunc2;
				if (i == 0)
				{
					cameraRenderBinderFunc2 = (this.BindB = new CameraRenderBinderFunc("MovRender::BindB", (XCameraBase XCon, ProjectionContainer JCon, Camera Cam) => this.RenderWholeMover(JCon, Cam, 0, M2Mover.DRAW_ORDER._ALL), 301f));
				}
				else if (i == 1)
				{
					cameraRenderBinderFunc2 = (this.BindBf = new CameraRenderBinderFunc("MovRender::BindBf", (XCameraBase XCon, ProjectionContainer JCon, Camera Cam) => this.RenderWholeMover(JCon, Cam, 1, M2Mover.DRAW_ORDER.PR0), 301f));
				}
				else if (i == 2)
				{
					cameraRenderBinderFunc2 = (this.BindT = new CameraRenderBinderFunc("MovRender::BindT", (XCameraBase XCon, ProjectionContainer JCon, Camera Cam) => this.RenderWholeMover(JCon, Cam, 2, M2Mover.DRAW_ORDER._ALL), 300f));
				}
				else
				{
					cameraRenderBinderFunc2 = (this.BindCM = new CameraRenderBinderFunc("MovRender::BindCM", (XCameraBase XCon, ProjectionContainer JCon, Camera Cam) => this.RenderWholeMover(JCon, Cam, 3, M2Mover.DRAW_ORDER._ALL), 300f));
				}
				M2Camera cam = this.Cam;
				ICameraRenderBinder cameraRenderBinder = cameraRenderBinderFunc2;
				int num;
				if (i != 1)
				{
					if (i != 3)
					{
						num = M2MovRenderContainer.drawer_t_layer;
					}
					else
					{
						num = M2MovRenderContainer.draw_cm_layer;
					}
				}
				else
				{
					num = M2MovRenderContainer.drawer_mask_layer;
				}
				cam.assignRenderFunc(cameraRenderBinder, num, false, null);
				uint num2 = this.binding;
				uint num3;
				switch (i)
				{
				case 0:
					num3 = 257U;
					break;
				case 1:
					num3 = 514U;
					break;
				case 2:
					num3 = 1028U;
					break;
				default:
					num3 = 2056U;
					break;
				}
				this.binding = num2 | num3;
				if ((this.binding & 4096U) == 0U)
				{
					this.binding |= 4096U;
				}
				if (i == 1 && (this.binding & 4U) == 0U)
				{
					this.bindDrawer2Cam(2);
				}
			}
		}

		public bool RenderWholeMover(ProjectionContainer JCon, Camera Cam, int draw_id, M2Mover.DRAW_ORDER clip_dod = M2Mover.DRAW_ORDER._ALL)
		{
			List<M2RenderTicket> list = this.AADob[draw_id];
			if (this.Aneed_sort[draw_id])
			{
				this.Aneed_sort[draw_id] = false;
				list.Sort((M2RenderTicket a, M2RenderTicket b) => M2MovRenderContainer.fnSortDrawTicket(a, b));
			}
			if (this.Mp.floort != this.pre_floort)
			{
				this.pre_floort = this.Mp.floort;
				this.binding |= 240U;
			}
			bool flag;
			switch (draw_id)
			{
			case 0:
				flag = (this.binding & 16U) > 0U;
				break;
			case 1:
				flag = (this.binding & 32U) > 0U;
				break;
			case 2:
				flag = (this.binding & 64U) > 0U;
				break;
			default:
				flag = (this.binding & 128U) > 0U;
				break;
			}
			bool flag2 = flag;
			switch (draw_id)
			{
			case 0:
				flag = (this.binding & 256U) > 0U;
				break;
			case 1:
				flag = (this.binding & 512U) > 0U;
				break;
			case 2:
				flag = (this.binding & 1024U) > 0U;
				break;
			default:
				flag = (this.binding & 2048U) > 0U;
				break;
			}
			bool flag3 = flag;
			Material material = null;
			this.ADobBuf.Clear();
			this.ADobBuf.AddRange(list);
			if (draw_id == 2 && this.AADob[1].Count > 0)
			{
				this.insertMoverCameraRendering(JCon, Cam);
			}
			int count = this.ADobBuf.Count;
			int num = 0;
			for (int i = 0; i < 2; i++)
			{
				int j = 0;
				int num2 = count;
				while (j <= 255)
				{
					bool flag4 = false;
					for (int k = num; k < num2; k++)
					{
						M2RenderTicket m2RenderTicket = this.ADobBuf[k];
						if (m2RenderTicket != null)
						{
							if (m2RenderTicket.order >= clip_dod)
							{
								num2 = k;
								break;
							}
							if (m2RenderTicket.clipCheck(this.Mp, j == 0 && flag3))
							{
								if (m2RenderTicket.Draw(JCon, Cam, flag2 && j == 0, j, ref material))
								{
									flag4 = true;
								}
								else
								{
									this.ADobBuf[k] = null;
								}
							}
						}
					}
					if (!flag4)
					{
						break;
					}
					j++;
				}
				if (num2 >= count)
				{
					break;
				}
				clip_dod = M2Mover.DRAW_ORDER._ALL;
				num = num2;
			}
			if (draw_id == 0)
			{
				this.binding &= 4294967023U;
			}
			if (draw_id == 1)
			{
				this.binding &= 4294966751U;
			}
			if (draw_id == 2)
			{
				this.binding &= 4294966207U;
			}
			if (draw_id == 3)
			{
				this.binding &= 4294965119U;
			}
			return true;
		}

		public M2RenderTicket assignDrawable(M2Mover.DRAW_ORDER order, Transform Trs, M2RenderTicket.FnPrepareMd FnMd, MeshDrawer MdMain, M2Mover AssignMover, IPosLitener MapPosLsn = null)
		{
			if (X.DEBUGSTABILIZE_DRAW)
			{
				return null;
			}
			return this.assignDrawable(new M2RenderTicket(order, Trs, FnMd, MdMain, AssignMover, MapPosLsn));
		}

		public M2RenderTicket assignDrawable(M2RenderTicket Tk)
		{
			if (X.DEBUGSTABILIZE_DRAW)
			{
				return null;
			}
			int num = this.dod2Id(Tk.order);
			this.bindDrawer2Cam(num);
			Tk.clipCheck(this.Mp, true);
			List<M2RenderTicket> list = this.AADob[num];
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				if (list[i].order > Tk.order)
				{
					list.Insert(i, Tk);
					return Tk;
				}
			}
			list.Add(Tk);
			return Tk;
		}

		public static int fnSortDrawTicket(M2RenderTicket Tka, M2RenderTicket Tkb)
		{
			if (Tka.order == Tkb.order)
			{
				return 0;
			}
			if (Tka.order >= Tkb.order)
			{
				return 1;
			}
			return -1;
		}

		public M2RenderTicket deassignDrawable(M2RenderTicket Tk, int _draw_id = -1)
		{
			if (Tk == null)
			{
				return null;
			}
			if (_draw_id < 0)
			{
				_draw_id = this.dod2Id(Tk.order);
			}
			List<M2RenderTicket> list = this.AADob[_draw_id];
			int num = list.IndexOf(Tk);
			if (num == -1)
			{
				return null;
			}
			list.RemoveAt(num);
			return null;
		}

		public int dod2Id(M2Mover.DRAW_ORDER d)
		{
			if (d >= M2Mover.DRAW_ORDER.CM0)
			{
				return 3;
			}
			if (d < M2Mover.DRAW_ORDER.BUF_0)
			{
				return 0;
			}
			if (d <= M2Mover.DRAW_ORDER.PR2)
			{
				return 1;
			}
			return 2;
		}

		public void changeOrder(M2RenderTicket Tk, ref M2Mover.DRAW_ORDER oreder_ptr, M2Mover.DRAW_ORDER order)
		{
			M2Mover.DRAW_ORDER order2 = Tk.order;
			int num = this.dod2Id(order2);
			int num2 = this.dod2Id(order);
			if (num == num2)
			{
				oreder_ptr = order;
				this.Aneed_sort[num] = true;
				return;
			}
			this.deassignDrawable(Tk, this.dod2Id(order2));
			oreder_ptr = order;
			this.assignDrawable(Tk);
		}

		private void insertMoverCameraRendering(ProjectionContainer JCon, Camera Cam)
		{
			Vector3 posMainTransform = this.Cam.PosMainTransform;
			BLIT.RenderToGLMtr(M2MovRenderContainer.MdInsert, posMainTransform.x, posMainTransform.y, 1f, M2MovRenderContainer.MdInsert.getMaterial(), JCon.CameraProjectionTransformed, M2MovRenderContainer.MdInsert.getTriMax(), false, false);
		}

		public bool need_clip_check
		{
			set
			{
				if (value)
				{
					this.binding |= 3840U;
				}
			}
		}

		public readonly M2Camera Cam;

		private Map2d Mp;

		public static readonly int drawer_mask_layer = IN.LAY("MoverRender");

		public static readonly int draw_cm_layer = IN.LAY("ChipsSubBottom");

		public static readonly int drawer_t_layer = IN.LAY("ForFinalRender");

		private List<M2RenderTicket>[] AADob;

		private List<M2RenderTicket> ADobBuf;

		private CameraRenderBinderFunc BindB;

		private CameraRenderBinderFunc BindBf;

		private CameraRenderBinderFunc BindT;

		private CameraRenderBinderFunc BindCM;

		private uint binding;

		private float pre_floort;

		private const M2Mover.DRAW_ORDER dod_buf = M2Mover.DRAW_ORDER.BUF_0;

		private const M2Mover.DRAW_ORDER dod_buf_end = M2Mover.DRAW_ORDER.PR2;

		private bool[] Aneed_sort;

		private const uint EXISTS_B = 1U;

		private const uint EXISTS_BUF = 2U;

		private const uint EXISTS_T = 4U;

		private const uint EXISTS_CM = 8U;

		private const uint ASSIGNED_XCTX = 4096U;

		private const uint NEED_PREPARE_MESH_B = 16U;

		private const uint NEED_PREPARE_MESH_BUF = 32U;

		private const uint NEED_PREPARE_MESH_T = 64U;

		private const uint NEED_PREPARE_MESH_CM = 128U;

		private const uint NEED_PREPARE_MESH_ALL = 240U;

		private const uint CLIP_CHECK_B = 256U;

		private const uint CLIP_CHECK_BUF = 512U;

		private const uint CLIP_CHECK_T = 1024U;

		private const uint CLIP_CHECK_CM = 2048U;

		private const uint CLIP_CHECK_ALL = 3840U;

		private static MeshDrawer MdInsert;
	}
}
