using System;
using PixelLiner;
using UnityEngine;
using XX;

namespace evt
{
	public class ButtonSkinEvEmot : ButtonSkin
	{
		public ButtonSkinEvEmot(aBtn _B, float _w, float _h)
			: base(_B, 0f, 0f)
		{
			this.Md = base.makeMesh(null);
			this.MdIco = base.makeMesh(null);
			this.MdDash = base.makeMesh(MTRX.getMtr(MTRX.MtrMeshDashLine.shader, base.container_stencil_ref));
			this.w = _w * 0.015625f;
			this.h = _h * 0.015625f;
			MTRX.getPF("checked_s");
			this.MdImg = base.makeMesh(MTRX.getMtr(BLEND.NORMAL, -1));
			this.MdIco = base.makeMesh(MTRX.MIicon.getMtr(BLEND.NORMAL, -1));
			if (ButtonSkinEvEmot.Col == null)
			{
				ButtonSkinEvEmot.Col = new C32();
			}
			this.fine_continue_flags = 13U;
		}

		public void InitEmot(EvDebugger _Edb, EvEmotVisibility _Emot, PxlFrame _FaceEmotionFrame = null, PxlFrame _PF = null)
		{
			this.Edb = _Edb;
			this.Emot = _Emot;
			this.FaceEmotionFrame = _FaceEmotionFrame;
			Transform transform = this.MMRD.GetGob(this.MdImg).transform;
			int container_stencil_ref = base.container_stencil_ref;
			this.shift_x = (this.shift_y = 0f);
			Material material;
			if (_Emot == null)
			{
				material = MTRX.getMI(_PF).getMtr(BLEND.NORMAL, container_stencil_ref);
				this.PFImg = _PF;
				this.scale = this.swidth / (float)_PF.pSq.width;
			}
			else
			{
				material = MTRX.getMI(this.Emot.SourceF).getMtr(BLEND.NORMAL, container_stencil_ref);
				if (this.FaceEmotionFrame == null)
				{
					this.scale = this.swidth / (float)this.Emot.SourceF.pSq.width;
				}
				else
				{
					this.PFFaceMesh = this.FaceEmotionFrame;
					this.scale = this.swidth / (float)this.FaceEmotionFrame.pSq.width;
				}
				Vector2 faceShift = _Emot.getFaceShift(0f);
				this.shift_x = faceShift.x * this.scale;
				this.shift_y = faceShift.y * this.scale;
			}
			this.MMRD.setMaterial(this.MdImg, material, false);
			transform.localScale = new Vector3(this.scale, this.scale, 1f);
			this.fine_flag = (this.fine_emo = true);
		}

		public override ButtonSkin Fine()
		{
			if (this.alpha == 0f || this.w == 0f || (this.Emot == null && this.PFImg == null))
			{
				return this;
			}
			float num = this.w * 64f + 0.5f;
			float num2 = this.h * 64f + 0.5f;
			float num3 = num / 2f;
			float num4 = num2 / 2f;
			if (this.isPushDown())
			{
				this.Md.Col = ButtonSkinEvEmot.Col.Set(1438833919).C;
				this.Md.StripedRect(0f, 0f, num - 4f, num2 - 4f, X.ANMPT(40, 1f), 0.5f, 8f, false);
			}
			else if (base.isHoveredOrPushOut())
			{
				this.Md.Col = ButtonSkinEvEmot.Col.Set(10077695).setA1(0.7f + 0.3f * X.COSIT(40f)).C;
				this.Md.Box(0f, 0f, num, num2, 1f, false);
			}
			if (base.isChecked())
			{
				this.MdDash.Col = ButtonSkinEvEmot.Col.Set(4294915438U).C;
				this.MdDash.RectDashedM(0f, 0f, num + 2f, num2 + 2f, X.IntC((num + num2) / 7f), 2f, 0.5f, false, false);
				ButtonSkinEvEmot.Col.Set(uint.MaxValue);
				if (base.isLocked())
				{
					ButtonSkinEvEmot.Col.Set(3431499912U);
				}
				this.MdIco.Col = C32.MulA(ButtonSkinEvEmot.Col.C, this.alpha);
				this.MdIco.RotaPF(num / 2f - 4f, -num2 / 2f + 4f, 1f, 1f, 0f, MTRX.getPF("checked_s"), false, false, false, uint.MaxValue, false, 0);
			}
			this.Md.updateForMeshRenderer(false);
			this.MdDash.updateForMeshRenderer(false);
			this.MdIco.updateForMeshRenderer(false);
			if (this.fine_emo)
			{
				if (this.PFImg != null)
				{
					this.MdImg.RotaPF(0f, 0f, 1f, 1f, 0f, this.PFImg, false, false, false, uint.MaxValue, false, 0);
				}
				else if (this.PFFaceMesh == null)
				{
					string text = this.Edb.cur_face_emotion;
					if (this.Emot.getEmotInfoByName(text) == null)
					{
						EvPerson.EmotLayer[] faceEmotionArray = this.Emot.getFaceEmotionArray();
						if (faceEmotionArray != null && faceEmotionArray.Length != 0)
						{
							text = faceEmotionArray[0].key;
						}
					}
					this.Emot.drawTo(this.MdImg, 0f, this.Emot.shift_y, 1f, text, true);
				}
				else
				{
					this.MdImg.RotaPF(this.shift_x, this.shift_y, 1f, 1f, 0f, this.PFFaceMesh, false, false, false, uint.MaxValue, false, 0);
				}
				this.MdImg.updateForMeshRenderer(false);
				this.fine_emo = false;
			}
			return base.Fine();
		}

		protected MeshDrawer Md;

		private MeshDrawer MdImg;

		private MeshDrawer MdIco;

		private MeshDrawer MdDash;

		public static C32 Col;

		private float scale = 1f;

		private EvDebugger Edb;

		private EvEmotVisibility Emot;

		private PxlFrame FaceEmotionFrame;

		private PxlFrame PFFaceMesh;

		private PxlFrame PFImg;

		private bool fine_emo;

		private float shift_x;

		private float shift_y;
	}
}
