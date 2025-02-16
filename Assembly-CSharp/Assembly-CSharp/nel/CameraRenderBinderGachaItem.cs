using System;
using evt;
using m2d;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class CameraRenderBinderGachaItem : ICameraRenderBinder
	{
		public CameraRenderBinderGachaItem(AbsorbManagerContainer _Con, bool _is_bottom)
		{
			this.Con = _Con;
			this.Mp = this.Con.Mv.Mp;
			this.is_bottom = _is_bottom;
			this.layer = (this.is_bottom ? this.Mp.M2D.Cam.getFinalRenderedLayer() : IN.gui_layer);
			this.Md = new MeshDrawer(null, 4, 6);
			this.Md.draw_gl_only = true;
			this.Md.activate("GachaItem_" + (this.is_bottom ? "B" : "T"), MTRX.MtrMeshNormal, this.is_bottom, MTRX.ColWhite, null);
			this.need_redraw = true;
		}

		public float getFarLength()
		{
			if (!this.is_bottom)
			{
				return -4.75f;
			}
			return 401.5f;
		}

		public bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
		{
			if (!this.Con.isActive() || this.Mp.M2D.FlgRenderAfter.isActive() || !this.Con.gacha_renderable || (!this.Con.renderable_on_evt_stop_ghandle && EV.isStoppingGameHandle() && !EV.handle_randamise_key_enabled))
			{
				return true;
			}
			bool ev_assign = this.Con.ev_assign;
			if (this.need_redraw)
			{
				this.need_redraw = false;
				this.Md.clear(false, false);
				int length = this.Con.Length;
				for (int i = 0; i < length; i++)
				{
					PrGachaItem gacha = this.Con.GetManagerItem(i).get_Gacha();
					if (gacha.isUseable())
					{
						this.Md.base_x = 0f;
						this.Md.base_y = 0f;
						if (this.is_bottom)
						{
							gacha.drawGachaB(this.Md, ev_assign);
						}
						else
						{
							gacha.drawGachaT(this.Md, ev_assign);
						}
					}
				}
				this.Md.updateForMeshRenderer(true);
			}
			M2Camera cam = this.Mp.M2D.Cam;
			Matrix4x4 matrix4x = JCon.CameraProjectionTransformed;
			Vector3 vector = new Vector3(0f, 0f, 0f);
			float num = 1f;
			if (!this.is_bottom)
			{
				Matrix4x4 matrix4x2 = (ev_assign ? Matrix4x4.identity : Matrix4x4.Translate(new Vector3(-cam.cam_shift_x, -cam.cam_shift_y, 0f)));
				matrix4x = matrix4x * Matrix4x4.Scale(new Vector3(1f, 1f, 0f)) * matrix4x2;
				num = (float)(ev_assign ? 2 : 2);
			}
			BLIT.RenderToGLMtrTriMulti(this.Md, vector.x, vector.y, num, matrix4x, 1f);
			return true;
		}

		public override string ToString()
		{
			if (!this.is_bottom)
			{
				return "GachaItem";
			}
			return "GachaItemB";
		}

		private readonly AbsorbManagerContainer Con;

		private readonly Map2d Mp;

		public readonly int layer;

		public readonly bool is_bottom;

		public MeshDrawer Md;

		public bool need_redraw;
	}
}
