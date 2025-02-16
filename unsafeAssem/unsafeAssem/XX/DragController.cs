using System;
using UnityEngine;

namespace XX
{
	public sealed class DragController : IClickable, ICameraRenderBinder
	{
		public static DragController Init(aBtn B, FnDragReleased fnDragReleased)
		{
			if (DragController.Instance == null)
			{
				DragController.Instance = new DragController();
			}
			else
			{
				DragController.Instance.Rem();
			}
			DragController.Instance.DragInit(B, fnDragReleased);
			return DragController.Instance;
		}

		private void Rem()
		{
			X.REMLOCK("DRAGGER");
			CameraBidingsBehaviour.UiBind.deassignPostRenderFunc(this);
			this.Exploded = null;
			this.MdScr.clear(false, false);
		}

		private DragController()
		{
			this.ALP = new LabelPointContainer<DRect>();
			this.MdScr = new MeshDrawer(null, 4, 6);
			this.MdScr.draw_gl_only = true;
			this.MdScr.activate("drag_controller", MTRX.MtrMeshNormal, false, MTRX.ColWhite, null);
		}

		private void DragInit(aBtn B, FnDragReleased _fnDragReleased = null)
		{
			this.Exploded = B;
			CameraBidingsBehaviour.UiBind.assignPostRenderFunc(this);
			this.PrePos = B.transform.position;
			this.MouseS = IN.getMouseScreenPos();
			this.lp_count = 0;
			this.base_expand_px = 8f;
			X.SCLOCK("DRAGGER");
			SND.Ui.play("tool_drag_init", false);
			this.t = 0;
			this.MdScr.clear(false, false);
			this.fnDragReleased = _fnDragReleased;
			IN.Click.initDragManual(this);
		}

		public bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
		{
			if (this.Exploded == null)
			{
				return false;
			}
			Vector2 mouseScreenPos = IN.getMouseScreenPos();
			Matrix4x4 matrix4x = Matrix4x4.Translate(new Vector3((mouseScreenPos.x - this.MouseS.x) * 0.015625f, (mouseScreenPos.y - this.MouseS.y) * 0.015625f, 0f)) * this.Exploded.transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(1f, 1f, 0f));
			GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed * matrix4x);
			this.Exploded.get_Skin().RenderToCamManual();
			BLIT.RenderToGLMtr(this.MdScr, 0f, 0f, 1f, this.MdScr.getMaterial(), JCon.CameraProjectionTransformed, -1, false, false);
			return true;
		}

		public float getFarLength()
		{
			return -9.48f;
		}

		public DragController SetRect(DRect Rc)
		{
			LabelPointContainer<DRect> alp = this.ALP;
			int num = this.lp_count;
			this.lp_count = num + 1;
			alp.Insert(Rc, num);
			return this;
		}

		public DragController SetRect(string key, float x, float y, float w, float h, float extend = 0f)
		{
			if (this.lp_count < this.ALP.Length)
			{
				LabelPointContainer<DRect> alp = this.ALP;
				int num = this.lp_count;
				this.lp_count = num + 1;
				DRect drect = alp.Get(num);
				drect.key = key;
				drect.Set(x, y, w, h);
				drect.extend_pixel = extend + this.base_expand_px * 0.015625f;
				drect.active = true;
			}
			else
			{
				DRect drect2 = new DRect(key, x, y, w, h, extend + this.base_expand_px * 0.015625f);
				this.ALP.Add(drect2);
				this.lp_count++;
			}
			return this;
		}

		public DragController SetRect(string key, IDesignerBlock Blk, float extend_pixel = 0f)
		{
			Vector2 vector = Blk.getTransform().position;
			float num = Blk.get_swidth_px() * 0.015625f;
			float num2 = Blk.get_sheight_px() * 0.015625f;
			return this.SetRect(key, vector.x - num / 2f, vector.y - num2 / 2f, num, num2, extend_pixel * 0.015625f);
		}

		public DragController SetRectSide(string key, IDesignerBlock Blk, AIM aim, float expand_pixel = 0f, float width_px = 0f)
		{
			Vector2 vector = Blk.getTransform().position;
			float num = Blk.get_swidth_px() * 0.015625f;
			float num2 = Blk.get_sheight_px() * 0.015625f;
			float num3 = num / 2f;
			float num4 = num2 / 2f;
			float num5 = num;
			float num6 = num2;
			float num7 = vector.x;
			float num8 = vector.y;
			float num9 = (float)CAim._XD(aim, 1);
			float num10 = (float)CAim._YD(aim, 1);
			if (num9 != 0f)
			{
				num5 = width_px * 0.015625f * num9;
				num7 += (num3 + (expand_pixel - width_px / 2f) * 0.015625f) * num9;
			}
			if (num10 != 0f)
			{
				num6 = width_px * 0.015625f * num10;
				num8 += (num4 + (expand_pixel - width_px / 2f) * 0.015625f) * num10;
			}
			return this.SetRect(key, num7, num8, num5, num6, 0f);
		}

		public bool getClickable(Vector2 PosU, out IClickable Res)
		{
			Res = this.Exploded;
			if (this.Exploded == null || !this.Exploded.isActive())
			{
				this.Rem();
				return true;
			}
			while (this.lp_count < this.ALP.Length)
			{
				LabelPointContainer<DRect> alp = this.ALP;
				int num = this.lp_count;
				this.lp_count = num + 1;
				alp.Get(num).active = false;
			}
			Vector2 mouseScreenPos = IN.getMouseScreenPos();
			this.t++;
			DRect cur = this.Cur;
			float num2 = IN.screen_visible_w * 0.5f;
			float num3 = IN.screen_visible_h * 0.5f;
			this.Cur = this.ALP.findStanding((mouseScreenPos.x - num2) * 0.015625f, (mouseScreenPos.y - num3) * 0.015625f, (float)this.lp_count, null, -1);
			this.ALP.findStanding((mouseScreenPos.x - num2) * 0.015625f, (mouseScreenPos.y - num3) * 0.015625f, (float)this.lp_count, null, -1);
			if (cur != this.Cur && cur != null && this.Cur != null)
			{
				this.ALP.findStanding((mouseScreenPos.x - num2) * 0.015625f, (mouseScreenPos.y - num3) * 0.015625f, (float)this.lp_count, null, -1);
			}
			this.MdScr.clearSimple();
			this.MdScr.Col = C32.d2c(3445513822U);
			this.MdScr.Box(this.PrePos.x, this.PrePos.y, this.Exploded.get_swidth_px() * 0.015625f, this.Exploded.get_sheight_px() * 0.015625f, 0f, true);
			if (this.Cur != null)
			{
				this.MdScr.Col = MTRX.cola.Set(4294901760U).setA1(0.7f + 0.3f * X.COSIT(27f)).C;
				this.MdScr.Box(this.Cur.x, this.Cur.y, this.Cur.w, this.Cur.h, 0.03125f, true);
			}
			this.MdScr.updateForMeshRenderer(false);
			if (IN.isMenuPD(1))
			{
				IN.Click.quitDraggingManual(false);
			}
			return true;
		}

		public void OnPointerUp(bool clicking)
		{
			if (this.Exploded == null)
			{
				return;
			}
			Vector2 mouseScreenPos = IN.getMouseScreenPos();
			SND.Ui.Stop();
			if (this.t < 20 || (this.t < 30 && X.LENGTHXYS(this.MouseS.x, this.MouseS.y, mouseScreenPos.x, mouseScreenPos.y) < 20f))
			{
				this.Cur = null;
			}
			else
			{
				SND.Ui.play("tool_drag_quit", false);
				if (IN.isMenuPD(1))
				{
					this.Cur = null;
				}
			}
			if (this.Cur == null)
			{
				this.Exploded.ExecuteOnClick();
			}
			else if (this.fnDragReleased != null)
			{
				this.fnDragReleased(this, this.Exploded, this.Cur);
			}
			this.Rem();
		}

		public void OnPointerEnter()
		{
		}

		public void OnPointerExit()
		{
		}

		public bool OnPointerDown()
		{
			return false;
		}

		private static DragController Instance;

		private aBtn Exploded;

		private Vector2 PrePos;

		private Vector2 MouseS;

		private LabelPointContainer<DRect> ALP;

		private int lp_count;

		private DRect Cur;

		private MeshDrawer MdScr;

		private int t;

		private FnDragReleased fnDragReleased;

		public float base_expand_px;
	}
}
