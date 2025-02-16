using System;
using System.Collections.Generic;
using m2d;
using Spine;
using Spine.Unity;
using UnityEngine;
using XX;

namespace nel
{
	public class EnemyAnimatorSpine : EnemyAnimatorBase, EnemyAnimator.IEnemyAnimListener
	{
		public EnemyAnimatorSpine(NelEnemy _Mv, string mti_key, string _name, string json_key, string atlas_key, string _anim_name = null)
			: base(_Mv)
		{
			this.index = this.Mv.index + 1;
			this.name = _name;
			this._toString = "<EnemyAnimatorSpine>" + _name;
			this.Anm = new SpineViewer(json_key);
			this.Anm.atlas_key = ((atlas_key != null) ? atlas_key : null);
			this.Anm.attachTextMTI(MTI.LoadContainerSpine("SpineAnimEn/" + mti_key + ".atlas", atlas_key, "M2D"));
			MTIOneImage mtioneImage = MTI.LoadContainerOneImage("SpineAnimEn/" + mti_key, null, null);
			this.Anm.attachImageMTI(mtioneImage);
			this.Gob = IN.CreateGob(this.Mv.Mp.gameObject, "-Spine-" + this.name);
			this.Anm.call_complete_evnet_when_track0 = true;
			this.Anm.initGameObject(this.Gob);
			this.Anm.addListenerEvent(new AnimationState.TrackEntryEventDelegate(this.triggerTrackEvent));
			base.prepareMesh(mtioneImage.MI, this.Mv.TeCon);
			this.Anm.prepareMaterial(this.MtrBase);
			this.Anm.whole_loop_set = true;
			this.Anm.consider_playing_timescale = false;
			this.ASlotEye = new List<Slot>(1);
			this.ASlotEyeShape = new List<EnemyAnimatorSpine.SlotShape>(1);
			this.FD_drawBitmapEye = new EnemyAnimator.FnDrawBitmapEye(this.drawBitmapEye);
			if (_anim_name != null)
			{
				this.clearAnim(_anim_name, 0, null);
			}
			this.Mv.RegisterToTeCon(this, this, null);
		}

		public void fnInstruction(SkeletonRendererInstruction IMesh)
		{
		}

		public override void destruct()
		{
			base.destruct();
			if (this.Gob != null)
			{
				IN.DestroyOne(this.Gob);
				this.Gob = null;
			}
		}

		public void clearAnim(string anim_name, int loopTo_frame = -1000, string skin_name = null)
		{
			if (this.Gob == null)
			{
				return;
			}
			this.Anm.clearAnim(anim_name, loopTo_frame, skin_name);
			this.Gob.SetActive(false);
			this.loop_appeared = 0;
			this.charaAnim = this.Anm.GetSkeletonAnimation();
			if (this.Mf == null)
			{
				this.Mf = this.Gob.GetComponent<MeshFilter>();
				this.charaAnim.GenerateMeshOverride += this.fnInstruction;
				this.charaAnim.disableRenderingOnOverride = false;
				this.fine_t = 0f;
				base.prepareRendetTicket(this.check_clip ? this.Mv : null, null, null);
			}
			this.charaAnim.loop = true;
			this.fineEyeSlots();
			this.checkFrameManual(0f, true);
		}

		public TrackEntry addAnim(int id, string anim_name, int loopf = 0, float mixtime = 0f, float alpha = 1f)
		{
			return this.Anm.addAnim(id, anim_name, loopf, mixtime, alpha);
		}

		private void triggerTrackEvent(TrackEntry trackEntry, Event e)
		{
			if (e.Data.Name == "loop" && e.Int > this.loop_appeared)
			{
				this.loop_appeared = e.Int;
				this.Anm.setAnmLoopFrameSecond(trackEntry.Animation.Name, trackEntry.TrackTime);
			}
		}

		public void fineEyeSlots()
		{
			this.ASlotEye.Clear();
			this.ASlotEyeShape.Clear();
			ExposedList<Slot> slots = this.Anm.GetSkeleton().Slots;
			int count = slots.Count;
			if (this.MdEyeI != null)
			{
				this.MdEyeI.clear(false, false);
			}
			for (int i = 0; i < count; i++)
			{
				Slot slot = slots.Items[i];
				if (TX.isStart(slot.Data.Name, "eye", 0))
				{
					EnemyAnimator.EYE_DRAW_TYPE eyeDrawType = EnemyAnimator.getEyeDrawType(slot.Data.Name);
					if (eyeDrawType == EnemyAnimator.EYE_DRAW_TYPE.BITMAP)
					{
						if (this.ASlotEye.Count == 0)
						{
							if (this.MdEyeI == null)
							{
								this.MdEyeI = new MeshDrawer(null, 4, 6);
							}
							this.MdEyeI.draw_gl_only = true;
							this.MdEyeI.activate("spine_eye_i", this.MI.getMtr(BLEND.ADD, -1), false, MTRX.ColWhite, null);
						}
						this.ASlotEye.Add(slot);
					}
					else
					{
						this.ASlotEyeShape.Add(new EnemyAnimatorSpine.SlotShape(slot, eyeDrawType));
					}
					slot.A = 0f;
				}
			}
		}

		public void addListenerComplete(AnimationState.TrackEntryDelegate Fn)
		{
			this.Anm.addListenerComplete(Fn);
		}

		public void addListenerEvent(AnimationState.TrackEntryEventDelegate Fn)
		{
			this.Anm.addListenerEvent(Fn);
		}

		public void remListenerComplete(AnimationState.TrackEntryDelegate Fn)
		{
			this.Anm.remListenerComplete(Fn);
		}

		public void remListenerEvent(AnimationState.TrackEntryEventDelegate Fn)
		{
			this.Anm.remListenerEvent(Fn);
		}

		public string getBaseAnimName()
		{
			return this.Anm.getBaseAnimName();
		}

		public SpineViewer getBaseAnimator()
		{
			return this.Anm;
		}

		public void randomizeFrame()
		{
			TrackEntry track = this.Anm.getTrack(0);
			if (track != null)
			{
				track.TrackTime = X.NIXP(0f, track.TrackEnd);
			}
		}

		public override bool noNeedDraw()
		{
			return this.Mf == null || base.noNeedDraw();
		}

		public void checkFrame(EnemyAnimator AnmBase, float TS)
		{
			if (this.auto_check_frame_from_listener_attached)
			{
				this.checkFrameManual(TS, false);
			}
		}

		public override bool updateAnimator(float f)
		{
			this.checkFrameManual(f, false);
			return true;
		}

		public void checkFrameManual(float TS, bool force = false)
		{
			if (this.Anm == null || this.destructed)
			{
				return;
			}
			this.fine_t -= TS;
			if (this.fine_t <= 0f || force)
			{
				this.fine_t = this.fine_intv;
				this.Anm.updateAnim(true, TS * X.Mx(1f, this.fine_intv) * this.timescale);
				this.need_fine_mesh = true;
			}
		}

		public void fineFrameData(EnemyAnimator AnmBase, EnemyFrameDataBasic FrmData, bool created)
		{
		}

		public int createEyes(EnemyAnimator AnmBase, Matrix4x4 MxAfterMultiple, ref int eyepos_search)
		{
			if (this.alpha_ == 0f)
			{
				return 0;
			}
			int num = 0;
			int count = this.ASlotEyeShape.Count;
			int count2 = this.ASlotEye.Count;
			if (count + count2 > 0)
			{
				Matrix4x4 matrix4x = base.getTransformMatrix(true) * base.getAfterMultipleMatrix(false);
				for (int i = 0; i < 2; i++)
				{
					int num2 = ((i == 0) ? count : count2);
					for (int j = 0; j < num2; j++)
					{
						Slot slot;
						EnemyAnimator.EYE_DRAW_TYPE eye_DRAW_TYPE;
						if (i == 0)
						{
							EnemyAnimatorSpine.SlotShape slotShape = this.ASlotEyeShape[j];
							slot = slotShape.SlotRef;
							eye_DRAW_TYPE = slotShape.type;
						}
						else
						{
							slot = this.ASlotEye[j];
							eye_DRAW_TYPE = EnemyAnimator.EYE_DRAW_TYPE.BITMAP;
						}
						if (slot.Attachment != null)
						{
							Bone bone = slot.Bone;
							num++;
							EnemyAnimator.NEyePos neyePos;
							eyepos_search = AnmBase.getEyeReference(out neyePos, eyepos_search);
							eyepos_search++;
							float length = bone.Data.Length;
							Matrix4x4 matrix4x2 = Matrix4x4.identity;
							matrix4x2.m00 = bone.A;
							matrix4x2.m01 = bone.B;
							matrix4x2.m10 = bone.C;
							matrix4x2.m11 = bone.D;
							if (i == 0)
							{
								matrix4x2 *= Matrix4x4.Translate(new Vector3(length * 0.5f, 0f, 0f));
							}
							matrix4x2 = matrix4x * Matrix4x4.Translate(new Vector3(bone.WorldX, bone.WorldY, 0f)) * matrix4x2;
							neyePos.Set(length * 64f, length * 64f * 0.5f, this.CMul, eye_DRAW_TYPE, matrix4x2, this.Mp.floort, this.Mv.enlarge_level_for_animator, (float)this.CMul.a / 255f * this.alpha);
							if (i == 1)
							{
								neyePos.FnDraw = this.FD_drawBitmapEye;
								neyePos.Obj = slot.Attachment;
							}
						}
					}
				}
			}
			return 0;
		}

		private void drawBitmapEye(EnemyAnimator.NEyePos Ey, C32 C, float tz)
		{
			if (Ey.Obj is RegionAttachment)
			{
				RegionAttachment regionAttachment = Ey.Obj as RegionAttachment;
				this.MdEyeI.Col = C.C;
				float num = regionAttachment.Width * 0.5f;
				float num2 = regionAttachment.Height * 0.5f;
				TextureRegion region = regionAttachment.Region;
				this.MdEyeI.setCurrentMatrix(Ey.MatrixTransform, false).TriRectBL(0);
				for (int i = 0; i < 4; i++)
				{
					this.MdEyeI.uvRectN((i == 0 || i == 1) ? region.u : region.u2, (i == 2 || i == 1) ? region.v : region.v2);
					this.MdEyeI.Pos(regionAttachment.X - num * (float)X.MPF(i == 0 || i == 1), regionAttachment.Y + num2 * (float)X.MPF(i == 2 || i == 1), null);
				}
			}
		}

		protected override bool FnEnRenderBufferInner(Camera Cam, M2RenderTicket Tk, bool need_redraw, ref int draw_id, out MeshDrawer MdOut, ref bool paste_mesh)
		{
			if (base.FnEnRenderBufferInner(Cam, Tk, need_redraw, ref draw_id, out MdOut, ref paste_mesh))
			{
				int num = draw_id;
				return true;
			}
			return false;
		}

		protected override bool FnEnRenderBaseInner(Camera Cam, M2RenderTicket Tk, bool need_redraw, ref int draw_id, out MeshDrawer MdOut, ref bool paste_mesh)
		{
			if (base.FnEnRenderBaseInner(Cam, Tk, need_redraw, ref draw_id, out MdOut, ref paste_mesh))
			{
				return true;
			}
			if (draw_id == 0)
			{
				if (this.MdEyeI != null && this.MdEyeI.draw_triangle_count > 0)
				{
					Tk.Matrix = Matrix4x4.identity;
					MdOut = this.MdEyeI;
				}
				return true;
			}
			draw_id--;
			return false;
		}

		protected override void fnDrawEyeInner(MeshDrawer MdEye, MeshDrawer MdEyeV, MeshDrawer MdAdd)
		{
			if (this.charaAnim == null)
			{
				return;
			}
			base.fnDrawEyeInner(MdEye, MdEyeV, MdAdd);
			if (this.CAdd.rgb > 0U && this.alpha_ > 0f && MdAdd != null)
			{
				Mesh sharedMesh = this.Mf.sharedMesh;
				if (sharedMesh != null)
				{
					this.initMdAddUv2((int)sharedMesh.GetIndexCount(0), 0);
					MdAdd.RotaMesh(0f, 0f, 1f, 1f, 0f, sharedMesh, false, false, 0, -1, 0, -1);
					MdAdd.allocUv2(0, true);
					MdAdd.Identity();
				}
			}
		}

		public override string ToString()
		{
			return this._toString;
		}

		public override Matrix4x4 getAfterMultipleMatrix(float scalex, float scaley, bool ignore_rot = false)
		{
			scalex *= this.BaseScale.x;
			scaley *= this.BaseScale.y;
			return base.getAfterMultipleMatrix(scalex, scaley, ignore_rot);
		}

		public bool getBoneMapPos(string s, out Vector3 Pos)
		{
			Bone bone = this.Anm.FindBone(s);
			Pos = this.getTransformPosition();
			if (bone == null)
			{
				return false;
			}
			Pos = (this.BaseMatrix * base.getAfterMultipleMatrix(false)).MultiplyPoint3x4(new Vector3(bone.WorldX, bone.WorldY, 0f));
			Pos = new Vector3(this.Mp.globaluxToMapx(Pos.x), this.Mp.globaluyToMapy(Pos.y), bone.WorldRotationX);
			return true;
		}

		protected override void redrawBodyMeshInner()
		{
			base.redrawBodyMeshInner();
			this.need_fine_mesh = false;
			float num = this.Mv.sizex / this.Mv.sizex0;
			float num2 = this.Mv.sizey / this.Mv.sizey0;
			base.BasicColorInit(this.Md);
			this.Md.setCurrentMatrix(base.getAfterMultipleMatrix(false), false);
			this.Md.Uv2(num, num2, false).Uv3((float)this.CMul.r / 255f, (float)this.CMul.g / 255f, false);
			this.Md.RotaMesh(0f, 0f, 1f, 1f, 0f, this.Mf.sharedMesh, false, false, 0, -1, 0, -1);
			this.Md.allocUv2(0, true);
			this.Md.allocUv3(0, true);
			this.Md.Identity();
		}

		public void drawEyeInnerInit(EnemyAnimator Anm)
		{
			if (this.MdEyeI != null)
			{
				this.MdEyeI.clearSimple();
			}
		}

		public readonly string name;

		public readonly string _toString;

		private SpineViewer Anm;

		private GameObject Gob;

		private SkeletonAnimation charaAnim;

		private MeshFilter Mf;

		private MeshDrawer MdEyeI;

		public float fine_intv = 6f;

		private float fine_t;

		public float timescale = 1f;

		public bool auto_check_frame_from_listener_attached;

		private List<Slot> ASlotEye;

		private List<EnemyAnimatorSpine.SlotShape> ASlotEyeShape;

		public bool check_clip = true;

		private int loop_appeared;

		public Vector2 BaseScale = Vector2.one;

		private EnemyAnimator.FnDrawBitmapEye FD_drawBitmapEye;

		private struct SlotShape
		{
			public SlotShape(Slot _Slot, EnemyAnimator.EYE_DRAW_TYPE _type)
			{
				this.SlotRef = _Slot;
				this.type = _type;
			}

			public Slot SlotRef;

			public EnemyAnimator.EYE_DRAW_TYPE type;
		}
	}
}
