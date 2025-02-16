using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class EnemyAnimator : EnemyAnimatorBase
	{
		public static EnemyAnimator.EYE_DRAW_TYPE getEyeDrawType(string _name)
		{
			if (TX.isStart(_name, "eyef", 0))
			{
				return EnemyAnimator.EYE_DRAW_TYPE.EYEF;
			}
			if (TX.isStart(_name, "eyei", 0))
			{
				return EnemyAnimator.EYE_DRAW_TYPE.BITMAP;
			}
			if (TX.isStart(_name, "eye", 0))
			{
				return EnemyAnimator.EYE_DRAW_TYPE.EYE;
			}
			if (TX.isStart(_name, "eyeh_75", 0))
			{
				return EnemyAnimator.EYE_DRAW_TYPE.EYEH_75;
			}
			if (TX.isStart(_name, "eyeh_", 0))
			{
				return EnemyAnimator.EYE_DRAW_TYPE.EYEH_50;
			}
			return EnemyAnimator.EYE_DRAW_TYPE.BITMAP;
		}

		public EnemyAnimator(NelEnemy _Mv, EnemyAnimator.FnCreate _fnConvertPxlFrame, EnemyAnimator.FnFineFrame _fnFineFrame = null)
			: base(_Mv)
		{
			this.index = this.Mv.index;
			this.fnConvertPxlFrame = _fnConvertPxlFrame;
			this.fnFineFrame = _fnFineFrame;
			this.AEyePos = new List<EnemyAnimator.NEyePos>(0);
			this.TempStop = new Flagger(delegate(FlaggerT<string> V)
			{
				this.Anm.timescale = 0f;
			}, delegate(FlaggerT<string> V)
			{
				this.Anm.timescale = this.timescale_;
			});
			if (EnemyAnimator.OOFrmCache == null)
			{
				EnemyAnimator.Ood_pose = new BDic<string, string>(32);
				EnemyAnimator.OOFrmCache = new BDic<string, BDic<PxlFrame, EnemyFrameDataBasic>>();
				EnemyAnimator.BufMd = new MeshDrawer(null, 4, 6);
				EnemyAnimator.BufMd.activate();
				EnemyAnimator.DrSphere = new SphereDrawer();
			}
		}

		public virtual void initS(M2PxlAnimatorRT _Anm)
		{
			base.initS(this.Mv.Mp);
			this.Anm = _Anm;
			this.Anm.auto_assign_tecon = false;
			this.fineAnimatorOffset(-1f);
			this.TeCon = this.Mv.TeCon;
			this.TeCon.RegisterPos(this.Anm);
			string title = this.Anm.getCurrentCharacter().title;
			if (!EnemyAnimator.OOFrmCache.TryGetValue(title, out this.OFrmCache))
			{
				this.OFrmCache = (EnemyAnimator.OOFrmCache[title] = new BDic<PxlFrame, EnemyFrameDataBasic>(64));
			}
			base.prepareMesh(MTRX.getMI(this.Anm.getCurrentCharacter()), null);
			this.rot_center_y_u = this.Mv.sizey0 * this.Mp.CLENB * 0.75f * 0.015625f * this.TeScale.y;
			this.eye_time_check = 1f;
			this.af_shield_maxt = 0f;
			this.af_shield = 25f;
			base.prepareRendetTicket(this.Mv, null, this.Anm);
			this.showToFront(this.is_front, true);
			this.checkFrame(0f, true);
			if (this.RtkBuf != null)
			{
				this.Anm.gameObject.SetActive(false);
			}
			else
			{
				this.Anm.GetComponent<MeshFilter>().mesh = this.Md.getMesh();
			}
			if (!this.is_evil)
			{
				this.base_rotate_shuffle360 = 0f;
				this.scale_shuffle01 = 0f;
			}
		}

		public EnemyAnimator addAdditionalDrawer(EnemyAnimator.EnemyAdditionalDrawFrame Ead)
		{
			if (this.AEad == null)
			{
				this.AEad = new EnemyAnimator.EnemyAdditionalDrawFrame[] { Ead };
			}
			else
			{
				X.push<EnemyAnimator.EnemyAdditionalDrawFrame>(ref this.AEad, Ead, -1);
			}
			if (Ead is EnemyAnimator.IEnemyAnimListener)
			{
				this.addAdditionalListener(Ead as EnemyAnimator.IEnemyAnimListener);
			}
			return this;
		}

		public EnemyAnimator addAdditionalListener(EnemyAnimator.IEnemyAnimListener Lsn)
		{
			if (this.AEadLsn == null)
			{
				this.AEadLsn = new EnemyAnimator.IEnemyAnimListener[] { Lsn };
				if (Lsn is EnemyMeshDrawer)
				{
					(Lsn as EnemyMeshDrawer).checkframe_on_drawing = false;
				}
			}
			else
			{
				X.push<EnemyAnimator.IEnemyAnimListener>(ref this.AEadLsn, Lsn, -1);
			}
			return this;
		}

		public EnemyAnimator remAdditionalListener(EnemyAnimator.IEnemyAnimListener Lsn)
		{
			if (this.AEadLsn != null)
			{
				int num = X.isinC<EnemyAnimator.IEnemyAnimListener>(this.AEadLsn, Lsn);
				if (num >= 0)
				{
					X.splice<EnemyAnimator.IEnemyAnimListener>(ref this.AEadLsn, num, 1);
				}
			}
			return this;
		}

		public override void destruct()
		{
			base.destruct();
			if (this.AEadLsn != null)
			{
				int num = this.AEadLsn.Length;
				for (int i = 0; i < num; i++)
				{
					this.AEadLsn[i].destruct();
				}
			}
			this.AEadLsn = null;
			this.EdShield = this.Mv.Mp.remED(this.EdShield);
			this.Anm = null;
			this.CurPose = null;
			this.AEyePos = null;
			this.CurFrmData = null;
		}

		public void fineAnimatorOffset(float r = -1f)
		{
			if (this.Anm == null)
			{
				return;
			}
			r = this.Mv.enlarge_level_to_anim_scale((r < 0f) ? this.Mv.enlarge_level : r);
			this.Anm.setScale(r, r, false);
			this.need_fine_scale = true;
		}

		public override void showToFront(bool val, bool force = false)
		{
			base.showToFront(val, force);
			this.Anm.order = base.current_order;
			IN.setZ(this.Anm.Trs, base.transform_z);
		}

		public void runPre(float TS)
		{
			if (this.af_shield < this.af_shield_maxt + 25f)
			{
				this.af_shield += TS;
			}
			if (!this.checkframe_on_drawing)
			{
				this.checkFrame();
			}
		}

		public override bool updateAnimator(float f)
		{
			if (this.Anm == null)
			{
				return false;
			}
			this.Anm.updateAnimator(f * this.timescale);
			return base.updateAnimator(f);
		}

		public override bool checkFrame()
		{
			return this.checkFrame(this.Mv.TS * base.D_AF, false);
		}

		public override bool checkFrame(float TS, bool force = false)
		{
			if (force)
			{
				this.need_fine_scale = true;
			}
			this.eye_time_check -= TS;
			base.checkFrame(TS, force);
			if (this.Anm.need_fine || this.need_fine_scale)
			{
				if (this.need_fine_scale || this.CurPose != this.Anm.getCurrentPose())
				{
					this.CurPose = this.Anm.getCurrentPose();
					PxlSequence currentSequence = this.Anm.getCurrentSequence();
					this.Anm.setOffsetPixel(((float)(currentSequence.width / 2) - this.Mv.sizex * this.Mp.CLENB / this.Anm.scaleX) * (float)this.collider_align_x, ((float)(currentSequence.height / 2) - this.Mv.sizey * this.Mp.CLENB / this.Anm.scaleY) * (float)this.collider_align_y);
				}
				this.need_fine_scale = false;
			}
			if (this.AEadLsn != null)
			{
				int num = this.AEadLsn.Length;
				for (int i = 0; i < num; i++)
				{
					this.AEadLsn[i].checkFrame(this, TS);
				}
			}
			if (force || this.Anm.isCurrentFrameChanged() || this.Anm.need_fine)
			{
				this.Anm.need_fine = false;
				this.redrawMesh(this.RtkBuf == null);
				if (this.Mv.isTortureUsing() && !this.Anm.allow_pose_jump && TX.valid(this.Anm.getCurrentPose().end_jump_title) && this.Anm.get_loop_count() >= this.Anm.getCurrentPose().end_jump_loop_count)
				{
					AbsorbManager absorbManager = this.Mv.getAbsorbManager();
					if (absorbManager != null)
					{
						string text = this.Anm.getCurrentPose().end_jump_title;
						if (TX.isStart(text, "od_", 0))
						{
							text = TX.slice(text, 3);
						}
						absorbManager.changeTorturePose(text);
					}
				}
			}
			if (this.eye_time_check <= 0f && this.CurFrmData != null)
			{
				this.eye_time_check = this.eye_fine_interval;
				this.need_fine_eye_mesh = true;
				PxlFrame frame = this.Anm.getCurrentSequence().getFrame(this.Anm.getCurrentFrame());
				int num2 = frame.countLayers();
				int eye_count_total = this.CurFrmData.eye_count_total;
				if (this.need_refine_transform_on_eye == 1)
				{
					base.recalcTransformMatrix();
				}
				Matrix4x4 matrix4x = base.getTransformMatrix(true) * base.getAfterMultipleMatrix(false);
				int num3 = 0;
				for (int j = 0; j < num2; j++)
				{
					if ((this.layer_mask & (1U << j)) != 0U && this.CurFrmData.isEye(j))
					{
						PxlLayer layer = frame.getLayer(j);
						this.assignEyePosition(layer, matrix4x, ref num3);
					}
				}
				if (this.AEadLsn != null)
				{
					int num4 = this.AEadLsn.Length;
					for (int k = 0; k < num4; k++)
					{
						this.AEadLsn[k].createEyes(this, matrix4x, ref num3);
					}
				}
				this.Md.Identity();
			}
			return true;
		}

		public void assignEyePosition(PxlLayer L, Matrix4x4 MxAfterMultiple, ref int eyepos_search)
		{
			PxlLayer pxlLayer = L;
			if (L.isImport())
			{
				PxlLayer importSource = L.Img.getImportSource(L);
				if (importSource != null)
				{
					pxlLayer = importSource;
				}
			}
			MxAfterMultiple = MxAfterMultiple * Matrix4x4.Translate(new Vector3(L.x + X.NIXP(-1f, 1f) * this.eye_randomize_level, -L.y + X.NIXP(-1f, 1f) * this.eye_randomize_level, 0f) * 0.015625f) * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, (-L.rotR + X.NIXP(-1f, 1f) * 0.02f * this.eye_randomize_level * 6.2831855f) * 0.31830987f * 180f)) * Matrix4x4.Scale(new Vector3(L.zmx, L.zmy, 1f));
			EnemyAnimator.NEyePos neyePos;
			eyepos_search = this.getEyeReference(out neyePos, eyepos_search);
			eyepos_search++;
			neyePos.Set(L, pxlLayer, this.CMul, MxAfterMultiple, this.Mp.floort, this.Mv.enlarge_level_for_animator, this.alpha);
		}

		public int getEyeReference(out EnemyAnimator.NEyePos Out, int start_i = 0)
		{
			float num = 6f * this.eye_fine_interval;
			while (start_i < this.AEyePos.Count)
			{
				Out = this.AEyePos[start_i];
				if (Out == null)
				{
					Out = (this.AEyePos[start_i] = new EnemyAnimator.NEyePos());
					return start_i;
				}
				if (this.Mp.floort - (float)Out.time >= num)
				{
					return start_i;
				}
				start_i++;
			}
			start_i = this.AEyePos.Count;
			Out = new EnemyAnimator.NEyePos();
			this.AEyePos.Add(Out);
			return start_i;
		}

		public Vector2 getEyeMapPos(int i)
		{
			Vector2 vector = new Vector2(this.Mv.x, this.Mv.y);
			if (this.CurFrmData == null)
			{
				return vector;
			}
			if (i < this.CurFrmData.eye_count_pxl)
			{
				PxlFrame frame = this.Anm.getCurrentSequence().getFrame(this.Anm.getCurrentFrame());
				int num = frame.countLayers();
				Transform trs = this.Anm.Trs;
				float num2 = trs.localScale.x * this.TeScale.x / this.Mp.CLENB;
				float num3 = trs.localScale.y * this.TeScale.y / this.Mp.CLENB;
				for (int j = 0; j < num; j++)
				{
					if (this.CurFrmData.isEye(j) && i-- <= 0)
					{
						PxlLayer layer = frame.getLayer(j);
						Vector2 vector2 = new Vector2((layer.x + this.Anm.offsetPixelX) * num2, (-layer.y + this.Anm.offsetPixelY) * num3);
						vector2 = X.ROTV2e(vector2, -this.rotationR_);
						vector2.x += this.Mv.x + this.after_offset_x_ / this.Mp.CLENB;
						vector2.y += this.Mv.y - this.after_offset_y_ / this.Mp.CLENB;
						return vector2;
					}
				}
			}
			i -= this.CurFrmData.eye_count_pxl;
			return vector;
		}

		public bool getFrameData(PxlFrame curF, out EnemyFrameDataBasic fData)
		{
			if (!this.OFrmCache.TryGetValue(curF, out fData))
			{
				Dictionary<PxlFrame, EnemyFrameDataBasic> ofrmCache = this.OFrmCache;
				EnemyFrameDataBasic enemyFrameDataBasic;
				fData = (enemyFrameDataBasic = this.fnConvertPxlFrame(this, curF));
				ofrmCache[curF] = enemyFrameDataBasic;
				return true;
			}
			return false;
		}

		public static void FlushCachedFrames()
		{
			if (EnemyAnimator.OOFrmCache != null)
			{
				EnemyAnimator.OOFrmCache.Clear();
			}
		}

		private void redrawMesh(bool redrawing = true)
		{
			PxlFrame frame = this.Anm.getCurrentSequence().getFrame(this.Anm.getCurrentFrame());
			EnemyFrameDataBasic enemyFrameDataBasic = null;
			bool frameData = this.getFrameData(frame, out enemyFrameDataBasic);
			this.CurFrmData = enemyFrameDataBasic;
			if (!redrawing)
			{
				this.need_fine_mesh = true;
			}
			else
			{
				this.clearBaseMd();
				base.redrawBodyMeshInner(frame, this.CurFrmData);
				if (!this.Md.draw_gl_only)
				{
					this.Md.updateForMeshRenderer(false);
				}
			}
			this.layer_mask = uint.MaxValue;
			if (this.AEadLsn != null)
			{
				int num = this.AEadLsn.Length;
				for (int i = 0; i < num; i++)
				{
					this.AEadLsn[i].fineFrameData(this, this.CurFrmData, frameData);
				}
			}
			if (frameData && this.AEyePos.Capacity == 0)
			{
				this.AEyePos.Capacity = this.CurFrmData.eye_count_total * 6;
			}
			if (this.fnFineFrame != null)
			{
				this.fnFineFrame(this.CurFrmData, frame);
			}
		}

		public override Matrix4x4 getAfterMultipleMatrix(float scalex, float scaley, bool ignore_rot = false)
		{
			return base.getAfterMultipleMatrix(scalex, scaley, ignore_rot) * Matrix4x4.Translate(new Vector3(this.Anm.offsetPixelX, this.Anm.offsetPixelY) * 0.015625f);
		}

		protected override void clearBaseMd()
		{
			base.clearBaseMd();
			if (this.MdMask != null)
			{
				this.MdMask.clearSimple();
			}
			if (this.MdNormal != null)
			{
				this.MdNormal.clearSimple();
				this.MdNormal.Col = this.CMul.C;
			}
		}

		protected override void redrawBodyMeshInner()
		{
			base.redrawBodyMeshInner();
			base.redrawBodyMeshInner(this.getCurrentDrawnFrame(), this.CurFrmData);
		}

		protected override bool getTargetMeshDrawer(EnemyFrameDataBasic CurFrmData, int i, PxlLayer L, out bool extend, out MeshDrawer _Md)
		{
			uint num = this.layer_mask & (1U << i);
			extend = false;
			if ((CurFrmData.mask_layer & num) != 0U)
			{
				if (this.MdMask == null)
				{
					this.MdMask = new MeshDrawer(null, 4, 6);
					this.MdMask.draw_gl_only = true;
					this.MtrMaskBase = this.MI.getMtr(BLIT.MtrDrawDepth.shader, 1);
					this.MtrMaskBuffer = this.MI.getMtr(MTRX.ShaderAlphaSplice, 1);
					this.MdMask.activate("Mask", this.MtrMaskBase, false, MTRX.ColWhite, null);
					this.MdMask.base_z = -1f;
				}
				_Md = this.MdMask;
				return true;
			}
			if ((CurFrmData.normal_render_layer & num) != 0U)
			{
				if (this.MdNormal == null)
				{
					this.MdNormal = new MeshDrawer(null, 4, 6);
					this.MdNormal.draw_gl_only = true;
					Material withLightTextureMaterial = (M2DBase.Instance as NelM2DBase).getWithLightTextureMaterial(this.MI);
					this.MdNormal.activate("Normal", withLightTextureMaterial, false, MTRX.ColWhite, null);
					this.MdNormal.base_z = -1f;
				}
				_Md = this.MdNormal;
				return true;
			}
			return base.getTargetMeshDrawer(CurFrmData, i, L, out extend, out _Md);
		}

		protected override void redrawBodyMeshInnerAfter(Matrix4x4 MxAfterMultiple, PxlFrame curF = null, EnemyFrameDataBasic CurFrmData = null)
		{
			base.redrawBodyMeshInnerAfter(MxAfterMultiple, curF, CurFrmData);
			if (this.AEad != null)
			{
				int num = this.AEad.Length;
				for (int i = 0; i < num; i++)
				{
					this.Md.setCurrentMatrix(MxAfterMultiple, false);
					EnemyAnimator.EnemyAdditionalDrawFrame enemyAdditionalDrawFrame = this.AEad[i];
					if (enemyAdditionalDrawFrame.active && !enemyAdditionalDrawFrame.fnDraw(this.Md, enemyAdditionalDrawFrame))
					{
						break;
					}
				}
			}
		}

		protected override bool fnDrawEyeOnEffect(EffectItem Ef, M2DrawBinder Ed)
		{
			Ef.x = this.Mv.x;
			Ef.y = this.Mv.y;
			return !Ed.isinCamera(Ef, this.Mv.sizex + 2f, this.Mv.sizey + 2f) || base.fnDrawEyeOnEffect(Ef, Ed);
		}

		protected override void fnDrawEyeInner(MeshDrawer MdEye, MeshDrawer MdEyeV, MeshDrawer MdAdd)
		{
			if (this.CurFrmData == null)
			{
				return;
			}
			base.fnDrawEyeInner(MdEye, MdEyeV, MdAdd);
			if (this.AEadLsn != null)
			{
				int num = this.AEadLsn.Length;
				for (int i = 0; i < num; i++)
				{
					this.AEadLsn[i].drawEyeInnerInit(this);
				}
			}
			float num2 = -1000f;
			int count = this.AEyePos.Count;
			float num3 = 6f * this.eye_fine_interval;
			for (int j = 0; j < count; j++)
			{
				EnemyAnimator.NEyePos neyePos = this.AEyePos[j];
				if (neyePos == null)
				{
					break;
				}
				float num4 = this.Mp.floort - (float)neyePos.time;
				if (num4 < num3)
				{
					PxlImage pxlImage = neyePos.Img;
					C32 c = MdEyeV.ColGrd.Set(neyePos.C);
					this.Mv.setEyeColor(c, MdEye.ColGrd, ref num2);
					float num5;
					if (this.eye_fade_type == EnemyAnimator.EYE_FADE_TYPE.NORMAL)
					{
						num5 = X.ZSIN(num4, num3);
					}
					else
					{
						num5 = 1f - X.ZPOW(num3 - num4, num3);
					}
					if (num5 < 0.5f)
					{
						float num6 = num5 * 2f;
						c.blend(this.add_color_eye_fade_out, num6).mulA(X.NI(1f, neyePos.alpha, num6));
					}
					else
					{
						float num7 = (num5 - 0.5f) * 2f;
						c.Set(this.add_color_eye_fade_out).mulA(neyePos.alpha * (1f - num7 * 0.85f));
					}
					if (neyePos.FnDraw != null)
					{
						neyePos.FnDraw(neyePos, c, num5);
					}
					else
					{
						if (neyePos.drawtype != EnemyAnimator.EYE_DRAW_TYPE.BITMAP)
						{
							float num8 = neyePos.enlarge_level * X.NI(1f, 1.5f, num2);
							MdEyeV.Col = c.C;
							MdEyeV.setCurrentMatrix(neyePos.MatrixTransform, false);
							pxlImage = null;
							switch (neyePos.drawtype)
							{
							case EnemyAnimator.EYE_DRAW_TYPE.EYE:
								MdEyeV.Daia3(0f, 0f, (float)neyePos.w + this.extend_eye_wh, (float)neyePos.h + this.extend_eye_wh, 2.5f + num8, 2.5f + num8, false);
								break;
							case EnemyAnimator.EYE_DRAW_TYPE.EYEF:
								MdEyeV.Daia3(0f, 0f, (float)neyePos.w + this.extend_eye_wh, (float)neyePos.h + this.extend_eye_wh, 2.5f + num8, 2.5f + num8, false);
								MdEyeV.Daia(0f, 0f, ((float)neyePos.w + this.extend_eye_wh) * 0.5f, ((float)neyePos.h + this.extend_eye_wh) * 0.5f, false);
								break;
							case EnemyAnimator.EYE_DRAW_TYPE.EYEH_75:
								EnemyAnimator.DrawDaiaH(MdEyeV, 0f, ((float)neyePos.h + this.extend_eye_wh) / 6f, (float)neyePos.w + this.extend_eye_wh, ((float)neyePos.h + this.extend_eye_wh) * 4f / 3f, 2.5f + num8, 0.75f);
								break;
							case EnemyAnimator.EYE_DRAW_TYPE.EYEH_50:
								EnemyAnimator.DrawDaiaH(MdEyeV, 0f, 0f, (float)neyePos.w + this.extend_eye_wh, ((float)neyePos.h + this.extend_eye_wh) * 2f, 2.5f + num8, 0.5f);
								break;
							}
						}
						if (pxlImage != null)
						{
							float num9 = X.NI(1f, this.od_blink_extend_level, num2);
							MdEye.Col = c.C;
							MdEye.setCurrentMatrix(neyePos.MatrixTransform, false);
							MdEye.initForImg(this.Anm.getCurrentTexture(), pxlImage.RectIUv, false);
							MdEye.Rect(0f, 0f, ((float)neyePos.w + this.extend_eye_wh) * num9, ((float)neyePos.h + this.extend_eye_wh) * num9, false);
						}
					}
				}
			}
			MdEye.Identity();
			MdEyeV.Identity();
			if (this.CAdd.rgb > 0U && this.alpha_ > 0f && this.CurFrmData != null && MdAdd != null)
			{
				this.initMdAddUv2(4, 0);
				MdAdd.Identity();
				MdAdd.RotaMesh(0f, 0f, 1f, 1f, 0f, this.Md.getVertexArray(), null, this.Md.getUvArray(), null, null, this.Md.getTriangleArray(), false, 0, this.Md.getTriMax(), 0, this.Md.getVertexMax());
				MdAdd.allocUv2(0, true);
			}
		}

		public static void DrawDaiaH(MeshDrawer Md, float x, float y, float w, float h, float t, float hratio)
		{
			float num = w * 0.5f * 0.015625f;
			float num2 = h * 0.5f * 0.015625f;
			x *= 0.015625f;
			y *= 0.015625f;
			Md.TriRectBL(3, 2, 0, 1).TriRectBL(1, 0, 4, 5);
			if (hratio > 0.5f)
			{
				Md.TriRectBL(3, 7, 6, 2).TriRectBL(4, 8, 9, 5);
			}
			Md.Pos(x, y - num2 + t, null).Pos(x, y - num2, null);
			float num3 = ((hratio >= 0.5f) ? 1f : (hratio * 2f));
			float num4 = X.NI(-num2 + t, 0f, num3);
			float num5 = (-num + t) * num3;
			Md.Pos(x + num5, y + num4, null).Pos(x + num5 - t, y + num4, null);
			Md.Pos(x - num5, y + num4, null).Pos(x - num5 + t, y + num4, null);
			if (hratio > 0.5f)
			{
				float num6 = (hratio - 0.5f) * 2f;
				float num7 = -num + num * num6;
				float num8 = X.Mn(num7 + t, 0f);
				num4 = num2 * num6;
				Md.Pos(x + num8, y + num4, null).Pos(x + num7, y + num4, null);
				Md.Pos(x - num8, y + num4, null).Pos(x - num7, y + num4, null);
			}
		}

		private C32 fnCalcColorSphereAdd(SphereDrawer Sphere, C32 BufC, float draw_xu, float draw_yu, float in_xu, float in_yu, float radius_ratio, float agR)
		{
			float num = X.ZLINE(X.SINI(EnemyAnimator.sphere_draw_level + 12f - radius_ratio * 20f, 80f));
			return BufC.Set(this.CAdd).setA((float)X.Mn(200, this.CAdd.a) * this.alpha_ * num * num);
		}

		public override bool noNeedDraw()
		{
			float num = base.draw_margin + 1.5f;
			return this.CurFrmData == null || base.noNeedDraw() || !this.Mp.M2D.Cam.isCoveringMp(this.Mv.x - this.Mv.sizex - num, this.Mv.y - this.Mv.sizey - num, this.Mv.x + this.Mv.sizex + num, this.Mv.y + this.Mv.sizey + num, 0f);
		}

		protected override bool FnEnRenderBufferInner(Camera Cam, M2RenderTicket Tk, bool need_redraw, ref int draw_id, out MeshDrawer MdOut, ref bool paste_mesh)
		{
			bool flag = draw_id == 0;
			if (base.FnEnRenderBufferInner(Cam, Tk, need_redraw, ref draw_id, out MdOut, ref paste_mesh))
			{
				this.need_refine_transform_on_eye = 0;
				return true;
			}
			if (flag)
			{
				this.need_refine_transform_on_eye = 1;
				return false;
			}
			this.need_refine_transform_on_eye = 0;
			MdOut = null;
			if (draw_id == 0)
			{
				if (this.MdMask != null && this.CurFrmData.mask_layer != 0U)
				{
					Tk.Matrix = base.getTransformMatrix(false);
					this.MdMask.setMaterial(this.MtrMaskBuffer, false);
					MdOut = this.MdMask;
				}
				return true;
			}
			draw_id--;
			return false;
		}

		protected override bool FnEnRenderBaseInner(Camera Cam, M2RenderTicket Tk, bool need_redraw, ref int draw_id, out MeshDrawer MdOut, ref bool paste_mesh)
		{
			if (draw_id == 0 && this.MdMask != null && this.CurFrmData.mask_layer != 0U)
			{
				Tk.Matrix = base.getTransformMatrix(false);
				this.MdMask.setMaterial(this.MtrMaskBase, false);
				MdOut = this.MdMask;
				return true;
			}
			if (base.FnEnRenderBaseInner(Cam, Tk, need_redraw, ref draw_id, out MdOut, ref paste_mesh))
			{
				return true;
			}
			int num = draw_id;
			if (num == 0)
			{
				if (this.CurFrmData.normal_render_layer != 0U)
				{
					Tk.Matrix = base.getTransformMatrix(false);
					MdOut = this.MdNormal;
				}
				return true;
			}
			if (num != 1)
			{
				draw_id -= 2;
				return false;
			}
			if (this.Mv.MdHpMpBar != null && this.Mv.is_alive)
			{
				M2MoverPr aimPr = this.Mv.AimPr;
				if (aimPr != null && aimPr.is_alive)
				{
					Tk.Matrix = Matrix4x4.Translate(new Vector3(0f, (-this.Mv.sizey * this.Mp.CLENB - 14f) * 0.015625f, 0f)) * this.BaseMatrix;
					MdOut = this.Mv.MdHpMpBar;
				}
			}
			return true;
		}

		public void openShield(NelAttackInfo Atk, float f = 12f)
		{
			if (Atk != null)
			{
				this.Mv.PtcVar("x", (double)X.NI(Atk.hit_x, this.Mv.x, 0.7f)).PtcVar("y", (double)X.NI(Atk.hit_y, this.Mv.y, 0.7f)).PtcVar("ax", (double)((Atk.burst_vx == 0f) ? CAim._XD(Atk.Caster.getAimDirection(), 1) : ((Atk.burst_vx > 0f) ? 1 : (-1))))
					.PtcST("hit_tackle_hard_enemy", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
			}
			this.af_shield = 0f;
			this.af_shield_maxt = X.Mx(f, this.af_shield_maxt);
			if (this.EdShield == null)
			{
				if (this.FD_drawShield == null)
				{
					this.FD_drawShield = new M2DrawBinder.FnEffectBind(this.drawShield);
					this.FD_drawShieldSurface = new OctaHedronDrawer.FnDrawOctaHedronSurface(this.drawShieldSurface);
					this.FD_drawShieldLine = new OctaHedronDrawer.FnDrawOctaHedronLine(this.drawShieldLine);
				}
				this.EdShield = this.Mp.setED("drawshield", this.FD_drawShield, 0f);
				if (EnemyAnimator.Ocd == null)
				{
					EnemyAnimator.Ocd = new OctaHedronDrawer(1f, 1f);
					EnemyAnimator.MtrShieldBlurBehind = MTR.newMtr("Nel/ShieldBlurBehind");
				}
			}
		}

		public static void resetShieldMaterial()
		{
			if (EnemyAnimator.MtrShieldBlurBehind != null)
			{
				M2Shield.MaterialInitialize(null, EnemyAnimator.MtrShieldBlurBehind, EnemyAnimator.ColShieldSub);
			}
		}

		private static Color32 ColShieldSub
		{
			get
			{
				return C32.d2c(4282536021U);
			}
		}

		private bool drawShield(EffectItem Ef, M2DrawBinder Ed)
		{
			if (this.af_shield >= this.af_shield_maxt + 25f)
			{
				this.EdShield = null;
				return false;
			}
			Ef.x = this.Mv.drawx_map;
			Ef.y = this.Mv.drawy_map;
			if (!Ed.isinCamera(Ef, this.Mv.sizex * 2f + 1f, this.Mv.sizey * 2f + 1f))
			{
				return true;
			}
			uint ran = X.GETRAN2(this.Mv.index + 79, 8);
			EnemyAnimator.Ocd.w = (0.6f + this.Mv.sizex * 1.125f) * this.Mp.CLENB;
			EnemyAnimator.Ocd.h = (0.8f + this.Mv.sizey * 1.425f) * this.Mp.CLENB;
			EnemyAnimator.Ocd.agR = (this.Mp.floort * 0.005882353f + X.RAN(ran, 1724)) * 6.2831855f;
			EnemyAnimator.Ocd.zagR = X.SINI(this.Mp.floort + X.RAN(ran, 2394) * 200f, 49f) * 3.1415927f * 0.05f;
			MeshDrawer meshDrawer;
			if (this.af_shield < this.af_shield_maxt)
			{
				meshDrawer = Ef.GetMesh("", (this.af_shield < 4f) ? MTRX.MtrMeshAdd : MTRX.MtrMeshNormal, false);
				meshDrawer.ColGrd.Set(4290953922U).blend(3439329279U, X.COSI(this.Mp.floort, 2.78f) * 0.5f + 0.5f).setA1((this.af_shield < 4f) ? 1f : 0.6f);
			}
			else
			{
				EnemyAnimator.MtrShieldBlurBehind.SetFloat("_Scale", this.Mp.M2D.Cam.getScale(true));
				EnemyAnimator.MtrShieldBlurBehind.SetColor("_Color", EnemyAnimator.ColShieldSub);
				meshDrawer = Ef.GetMesh("", EnemyAnimator.MtrShieldBlurBehind, true);
				meshDrawer.ColGrd.Black().mulA(1f - X.ZLINE(this.af_shield - this.af_shield_maxt, 25f));
			}
			EnemyAnimator.Ocd.drawSurface(meshDrawer, 0f, 0f, this.FD_drawShieldSurface);
			if (this.af_shield < this.af_shield_maxt)
			{
				meshDrawer.ColGrd.Set(4292532954U);
				EnemyAnimator.Ocd.drawLine(meshDrawer, 0f, 0f, this.FD_drawShieldLine, 0f, 1f);
			}
			return true;
		}

		private void drawShieldSurface(OctaHedronDrawer Ocd, MeshDrawer Md, float x0, float y0, float x1, float y1, float x2, float y2, float front_level, bool is_under, bool is_front)
		{
			if (!is_front)
			{
				return;
			}
			Md.Col = C32.MulA(Md.ColGrd.C, (is_under ? 0.8f : 1f) * X.NI(0.8f, 0.4f, X.Abs(front_level)));
			if (is_under && this.Mv.hasFoot())
			{
				float drawy_map = this.Mv.drawy_map;
				float num = ((float)((int)(drawy_map + this.Mv.sizey + 0.09f)) - drawy_map) * this.Mp.CLENB;
				if (num > 0f && Ocd.h > num)
				{
					float num2 = 1f - num / Ocd.h;
					Md.TriRectBL(0);
					Md.PosD(X.NI(x0, x1, num2), X.NI(y0, y1, num2), null);
					Md.PosD(x1, y1, null).PosD(x2, y2, null);
					Md.PosD(X.NI(x0, x2, num2), X.NI(y0, y2, num2), null);
					return;
				}
			}
			Md.Tri012();
			Md.PosD(x0, y0, null).PosD(x1, y1, null).PosD(x2, y2, null);
		}

		private void drawShieldLine(OctaHedronDrawer Ocd, MeshDrawer Md, float x0, float y0, float x1, float y1, bool is_front, int to_top)
		{
			Md.Col = C32.MulA(Md.ColGrd.C, is_front ? 1f : 0.5f);
			if (to_top == -1 && this.Mv.hasFoot())
			{
				float drawy_map = this.Mv.drawy_map;
				float num = ((float)((int)(drawy_map + this.Mv.sizey + 0.09f)) - drawy_map) * this.Mp.CLENB;
				if (num > 0f && Ocd.h > num)
				{
					float num2 = num / Ocd.h;
					Md.BlurLine(x0, y0, X.NI(x0, x1, num2), X.NI(y0, y1, num2), 3f, 0, 20f, false);
					return;
				}
			}
			Md.BlurLine(x0, y0, x1, y1, 3f, 0, 20f, false);
		}

		public void setBreakerDropObject(string reader_key, float shiftx = 0f, float shifty = 0f, float cz = 0f, object Target = null)
		{
			if (this.FD_BreakerDropObject == null)
			{
				this.FD_BreakerDropObject = new M2DropObject.FnDropObjectDraw(this.BreakerDropObject);
			}
			Target = Target ?? this.Anm.getCurrentDrawnFrame();
			M2DropObjectReader.GetAndSet(this.Mp ?? M2DBase.Instance.curMap, reader_key, this.Mv.x + shiftx, this.Mv.y + shifty, 1f, 1f, cz, this.FD_BreakerDropObject, Target);
		}

		private bool BreakerDropObject(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed)
		{
			return EnemyAnimator.BreakerDropObject(Dro, Ef, Ed, this.MtrBase);
		}

		public static bool BreakerDropObject(M2DropObject Dro, EffectItem Ef, M2DrawBinder Ed, Material Mtr)
		{
			if (Dro.MyObj is PxlSequence)
			{
				PxlSequence pxlSequence = Dro.MyObj as PxlSequence;
				Dro.MyObj = pxlSequence.getFrame(X.xors(pxlSequence.countFrames()));
			}
			if (Dro.MyObj is PxlFrame)
			{
				PxlFrame pxlFrame = Dro.MyObj as PxlFrame;
				Dro.MyObj = pxlFrame.getLayer(X.xors(pxlFrame.countLayers()));
			}
			if (!(Dro.MyObj is PxlLayer))
			{
				return false;
			}
			PxlImage img = (Dro.MyObj as PxlLayer).Img;
			float num = X.ZLINE(Dro.af_ground - 10f, Dro.time - 10f);
			if (num >= 1f)
			{
				return false;
			}
			uint ran = X.GETRAN2(Dro.index, Dro.index % 7);
			float num2 = (float)X.Mn(img.width, img.height);
			num2 = X.Mn(num2, X.NI(7, 18, X.RAN(ran, 2985)));
			Texture mainTexture = Mtr.mainTexture;
			float num3 = (float)((int)(X.RAN(ran, 1335) * ((float)img.width - num2)));
			float num4 = (float)((int)(X.RAN(ran, 2018) * ((float)img.height - num2)));
			MeshDrawer mesh = Ef.GetMesh("", Mtr, true);
			mesh.initForImg(mainTexture, img.RectIUv.x * (float)mainTexture.width + num3, img.RectIUv.y * (float)mainTexture.height + num4, num2, num2);
			float num5 = 6.2831855f / X.NI(50, 200, X.RAN(ran, 569)) * (float)X.MPF(X.RAN(ran, 2293) > 0.5f);
			float num6 = X.ZLINE(Dro.af_ground, 90f);
			float num7 = X.RAN(ran, 2196) * 6.2831855f + num5 * (Dro.af - Dro.af_ground * 0.5f);
			if (num6 > 0f)
			{
				num7 = X.correctangleR(num7);
				if (num7 > 2.3561945f || num7 < -2.3561945f)
				{
					num7 += X.angledifR(num7, 3.1415927f) * X.ZPOW(num6);
				}
				else if (num7 > 0.7853982f)
				{
					num7 += X.angledifR(num7, 1.5707964f) * X.ZPOW(num6);
				}
				else if (num7 > -0.7853982f)
				{
					num7 += X.angledifR(num7, 0f) * X.ZPOW(num6);
				}
				else
				{
					num7 += X.angledifR(num7, -1.5707964f) * X.ZPOW(num6);
				}
			}
			mesh.Col = mesh.ColGrd.Set(4292072403U).blend(0U, num).C;
			mesh.RotaGraph(0f, num2 * 0.34f, 1f, num7, null, false);
			return true;
		}

		public virtual bool is_normal_render(PxlLayer L)
		{
			return TX.valid(this.normalrender_header) && TX.isStart(L.name, this.normalrender_header, 0);
		}

		public MeshDrawer getMainMeshDrawer()
		{
			return this.Md;
		}

		public void setAim(int aim, int restart_anim = -1)
		{
			this.Anm.setAim(aim, restart_anim);
		}

		public void setAim(AIM aim, int restart_anim = -1)
		{
			this.Anm.setAim((int)aim, restart_anim);
		}

		public void setPose(string title, int restart_anim = -1)
		{
			bool flag = this.Anm.poseIs("od_transform", false);
			title = this.odPose(title);
			this.Anm.read_point = this.Mv.isTortureUsing();
			this.Anm.allow_pose_jump = !this.Anm.read_point;
			this.Anm.setPose(title, restart_anim);
			bool flag2 = this.Anm.poseIs("od_transform", false);
			if (flag != flag2)
			{
				this.Anm.timescale = (flag2 ? (this.timescale_ / this.Mv.TS) : this.timescale_);
			}
		}

		public void setDuration(float duration_frame)
		{
			this.timescale = (float)this.Anm.getCurrentSequence().getDuration() / duration_frame;
		}

		public float timescale
		{
			get
			{
				return this.timescale_;
			}
			set
			{
				this.timescale_ = value;
				this.Anm.timescale = (this.TempStop.isActive() ? 0f : this.timescale_);
			}
		}

		public int get_loop_count()
		{
			return this.Anm.get_loop_count();
		}

		public int countTotalFrame()
		{
			return this.Anm.countTotalFrame();
		}

		public PxlPose getCurrentPose()
		{
			return this.Anm.getCurrentPose();
		}

		public PxlSequence getCurrentSequence()
		{
			return this.Anm.getCurrentSequence();
		}

		public string odPose(string title)
		{
			if (this.Mv.isOverDrive() && !TX.isStart(title, "od_", 0))
			{
				string text;
				if (EnemyAnimator.Ood_pose.TryGetValue(title, out text))
				{
					return text;
				}
				title = (EnemyAnimator.Ood_pose[title] = "od_" + title);
			}
			return title;
		}

		public bool poseIs(string pose, bool strict = false)
		{
			if (this.Anm.poseIs(pose, strict))
			{
				return true;
			}
			bool flag = false;
			if (!strict && this.Mv.isOverDrive() && !TX.isStart(pose, "od_", 0))
			{
				STB stb = TX.PopBld("od_", 0);
				stb += pose;
				flag = this.Anm.poseIs(stb.getRawBuilder(), strict);
				TX.ReleaseBld(stb);
			}
			return flag;
		}

		public bool poseIs(string pose0, string pose1)
		{
			return this.poseIs(pose0, false) || this.poseIs(pose1, false);
		}

		public bool poseIs(string pose0, string pose1, string pose2)
		{
			return this.poseIs(pose0, false) || this.poseIs(pose1, false) || this.poseIs(pose2, false);
		}

		public float mv_anmx
		{
			get
			{
				return this.Mv.x + this.after_offset_x_ / this.Anm.base_scale / this.Mp.CLEN;
			}
		}

		public float mv_anmy
		{
			get
			{
				return this.Mv.y - this.after_offset_y_ / this.Anm.base_scale / this.Mp.CLEN;
			}
		}

		public void animReset(int set_frame = 0, bool consider_loop = false)
		{
			PxlSequence currentSequence = this.Anm.getCurrentSequence();
			if (consider_loop && set_frame > currentSequence.countFrames())
			{
				set_frame = (set_frame - currentSequence.loop_to) % (currentSequence.countFrames() - currentSequence.loop_to) + currentSequence.loop_to;
			}
			this.Anm.animReset(set_frame);
		}

		public string pose_title
		{
			get
			{
				return this.Anm.pose_title;
			}
		}

		public int cframe
		{
			get
			{
				return this.Anm.cframe;
			}
		}

		public int pose_aim
		{
			get
			{
				return (int)this.Anm.pose_aim;
			}
		}

		public override bool need_fine
		{
			get
			{
				return this.Anm.need_fine;
			}
			set
			{
				if (value && this.Anm != null)
				{
					this.Anm.need_fine = value;
				}
			}
		}

		public Transform transform
		{
			get
			{
				return this.Anm.Trs;
			}
		}

		public float scaleX
		{
			get
			{
				return this.Anm.scaleX;
			}
		}

		public float scaleY
		{
			get
			{
				return this.Anm.scaleY;
			}
		}

		public bool isPoseExist(string p)
		{
			PxlCharacter pxlCharacter = ((this.Anm != null) ? this.Anm.getCurrentCharacter() : null);
			return pxlCharacter != null && pxlCharacter.getPoseByName(p) != null;
		}

		public bool isPoseStand()
		{
			if (this.Anm.pose_title == "stand" || this.Anm.pose_title == "od_stand")
			{
				return true;
			}
			PxlPose pxlPose = this.Anm.getCurrentPose();
			for (int i = 0; i < 8; i++)
			{
				string end_jump_title = pxlPose.end_jump_title;
				if (end_jump_title == "" || end_jump_title == null || end_jump_title == pxlPose.title)
				{
					return false;
				}
				if (end_jump_title == "stand" || end_jump_title == "od_stand")
				{
					return true;
				}
				pxlPose = this.Anm.getCurrentCharacter().getPoseByName(end_jump_title);
			}
			return false;
		}

		public void randomizeFrame()
		{
			PxlSequence currentSequence = this.Anm.getCurrentSequence();
			int num = currentSequence.countFrames();
			int num2 = this.cframe + num / 2 + X.xors(X.IntC((float)num * 0.5f));
			num2 = ((num2 < currentSequence.loop_to) ? num2 : ((num2 - currentSequence.loop_to) % (num - currentSequence.loop_to) + currentSequence.loop_to));
			this.animReset(num2, false);
		}

		public override void setAbsorbBlink(float map_pixel_x, float map_pixel_y)
		{
			base.setAbsorbBlink(map_pixel_x - this.Mv.drawx, map_pixel_y - this.Mv.drawy);
		}

		public PxlLayer[] SpGetPointsData(ref M2PxlAnimator MyAnimator, ref ITeScaler Scl, ref float rotation_plusR)
		{
			MyAnimator = this.Anm;
			rotation_plusR = this.rotationR_;
			Scl = this;
			if (this.Anm.PointData == null)
			{
				return null;
			}
			return this.Anm.PointData.GetPoints(this.Anm.getCurrentDrawnFrame(), false);
		}

		public PxlPose getPoseByName(string s)
		{
			if (this.Mv.isOverDrive() && !TX.isStart(s, "od_", 0))
			{
				s = "od_" + s;
			}
			PxlCharacter pxlCharacter = ((this.Anm != null) ? this.Anm.getCurrentCharacter() : null);
			if (pxlCharacter == null)
			{
				return null;
			}
			return pxlCharacter.getPoseByName(s);
		}

		public MImage getMI()
		{
			return this.MI;
		}

		public float anm_offset_pixel_y
		{
			get
			{
				return this.Anm.offsetPixelY;
			}
		}

		public bool looped_already
		{
			get
			{
				return this.Anm.looped_already;
			}
		}

		private float eye_fine_interval
		{
			get
			{
				return (float)(this.Mv.isOverDrive() ? 4 : 2);
			}
		}

		public PxlFrame getCurrentDrawnFrame()
		{
			return this.Anm.getCurrentSequence().getFrame(this.Anm.getCurrentFrame());
		}

		public EnemyFrameDataBasic getCurFrameData()
		{
			return this.CurFrmData;
		}

		public void setOffsetPixel(EnemyAnimator Src)
		{
			this.Anm.setOffsetPixel(Src.Anm.offsetPixelX, Src.Anm.offsetPixelY);
		}

		public Vector2 getMapPosForLayer(PxlLayer L)
		{
			Matrix4x4 transformMatrix = base.getTransformMatrix(false);
			float num;
			float num2;
			Texture2D texture2D;
			float num3;
			float num4;
			Matrix4x4 matrix4x;
			base.getTextureDataForDraw(out num, out num2, out texture2D, out num3, out num4, out matrix4x);
			return this.getMapPosForLayer(L, transformMatrix * matrix4x);
		}

		public Vector2 getMapPosForLayer(PxlLayer L, float lay_x, float lay_y)
		{
			Matrix4x4 transformMatrix = base.getTransformMatrix(false);
			float num;
			float num2;
			Texture2D texture2D;
			float num3;
			float num4;
			Matrix4x4 matrix4x;
			base.getTextureDataForDraw(out num, out num2, out texture2D, out num3, out num4, out matrix4x);
			Matrix4x4 matrix4x2 = transformMatrix * matrix4x;
			Vector3 vector;
			if (L.isTransformed())
			{
				vector = new Vector3((float)(-(float)L.Img.width) * 0.5f + lay_x, (float)L.Img.height * 0.5f - lay_y);
				vector = (Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, -L.rotR * 0.31830987f * 180f)) * Matrix4x4.Scale(new Vector3(L.zmx, L.zmy, 0f))).MultiplyPoint3x4(vector);
				vector = new Vector3((L.x + vector.x) * 0.015625f, (-L.y + vector.y) * 0.015625f, 0f);
			}
			else
			{
				vector = new Vector3((float)L.img_left + lay_x, -((float)L.img_top + lay_y), 0f) * 0.015625f;
			}
			Vector2 vector2 = matrix4x2.MultiplyPoint3x4(vector);
			return new Vector2(this.Mp.uxToMapx(this.Mp.M2D.effectScreenx2ux(vector2.x)), this.Mp.uyToMapy(this.Mp.M2D.effectScreeny2uy(vector2.y)));
		}

		public Vector2 getMapPosForLayer(PxlLayer L, Matrix4x4 TrxMtr_x_aftermultiple)
		{
			Vector2 vector = TrxMtr_x_aftermultiple.MultiplyPoint3x4(new Vector3(L.x * 0.015625f, -L.y * 0.015625f, 0f));
			return new Vector2(this.Mp.uxToMapx(this.Mp.M2D.effectScreenx2ux(vector.x)), this.Mp.uyToMapy(this.Mp.M2D.effectScreeny2uy(vector.y)));
		}

		public bool isAnimEnd()
		{
			return this.Anm.isAnimEnd();
		}

		public PxlCharacter getCurrentCharacter()
		{
			return this.Anm.getCurrentCharacter();
		}

		public int getAim()
		{
			return (int)this.Anm.pose_aim;
		}

		public bool allow_pose_jump
		{
			get
			{
				return this.Anm.allow_pose_jump;
			}
			set
			{
				this.Anm.allow_pose_jump = value;
			}
		}

		public int getEyeCount()
		{
			if (this.CurFrmData == null)
			{
				return 0;
			}
			return this.CurFrmData.eye_count_total;
		}

		public MeshDrawer getMeshNormalRender()
		{
			return this.MdNormal;
		}

		protected M2PxlAnimatorRT Anm;

		public ALIGN collider_align_x;

		public ALIGNY collider_align_y = ALIGNY.BOTTOM;

		public float eye_randomize_level = 1f;

		public float extend_eye_wh = 2f;

		public float od_blink_extend_level = 1.2f;

		public PxlPose CurPose;

		public bool need_fine_scale = true;

		private static BDic<string, BDic<PxlFrame, EnemyFrameDataBasic>> OOFrmCache;

		private static MeshDrawer BufMd;

		private static SphereDrawer DrSphere;

		private List<EnemyAnimator.NEyePos> AEyePos;

		protected EnemyFrameDataBasic CurFrmData;

		private static BDic<string, string> Ood_pose;

		private float eye_time_check;

		protected BDic<PxlFrame, EnemyFrameDataBasic> OFrmCache;

		private const int EYE_AFTIMG_MAX = 6;

		private const int EYE_FINE_INTV = 2;

		private const int EYE_FINE_INTV_OD = 4;

		public EnemyAnimator.EnemyAdditionalDrawFrame[] AEad;

		public EnemyAnimator.IEnemyAnimListener[] AEadLsn;

		public EnemyAnimator.EYE_FADE_TYPE eye_fade_type;

		private float timescale_ = 1f;

		public Flagger TempStop;

		private EnemyAnimator.FnCreate fnConvertPxlFrame;

		public EnemyAnimator.FnFineFrame fnFineFrame;

		private float af_shield;

		private float af_shield_maxt;

		private M2DrawBinder EdShield;

		private static OctaHedronDrawer Ocd;

		private static Material MtrShieldBlurBehind;

		private const float SHIELD_DISSOLVE_MAXT = 25f;

		private byte need_refine_transform_on_eye = 1;

		protected MeshDrawer MdMask;

		protected Material MtrMaskBase;

		protected Material MtrMaskBuffer;

		protected MeshDrawer MdNormal;

		private static float sphere_draw_level;

		private M2DrawBinder.FnEffectBind FD_drawShield;

		private OctaHedronDrawer.FnDrawOctaHedronSurface FD_drawShieldSurface;

		private OctaHedronDrawer.FnDrawOctaHedronLine FD_drawShieldLine;

		public M2DropObject.FnDropObjectDraw FD_BreakerDropObject;

		public delegate EnemyFrameDataBasic FnCreate(EnemyAnimator Con, PxlFrame F);

		public delegate void FnFineFrame(EnemyFrameDataBasic nF, PxlFrame F);

		public delegate void FnDrawBitmapEye(EnemyAnimator.NEyePos Ey, C32 C, float tz);

		public enum EYE_DRAW_TYPE : byte
		{
			BITMAP,
			EYE,
			EYEF,
			EYEH_75,
			EYEH_50
		}

		public enum EYE_FADE_TYPE : byte
		{
			NORMAL,
			ZPOWV
		}

		public class NEyePos
		{
			public EnemyAnimator.NEyePos Set(PxlLayer _L, PxlLayer _LSource, C32 CMul, Matrix4x4 _MatrixTransform, float floort, float _enlarge_level, float _alpha)
			{
				if (_L == _LSource || !TX.isStart(_LSource.pFrm.pPose.title, "_eye", 0))
				{
					this.drawtype = EnemyAnimator.EYE_DRAW_TYPE.BITMAP;
				}
				else
				{
					this.drawtype = EnemyAnimator.getEyeDrawType(_LSource.name);
				}
				if (this.drawtype == EnemyAnimator.EYE_DRAW_TYPE.BITMAP)
				{
					this.Img = _L.Img;
				}
				this.w = (ushort)_L.Img.width;
				this.h = (ushort)_L.Img.height;
				this.C = CMul.C;
				this.MatrixTransform = _MatrixTransform;
				this.enlarge_level = _enlarge_level;
				this.alpha = _alpha;
				this.time = (int)floort;
				this.FnDraw = null;
				this.Obj = null;
				return this;
			}

			public EnemyAnimator.NEyePos Set(float _w, float _h, C32 CMul, EnemyAnimator.EYE_DRAW_TYPE _drawtype, Matrix4x4 _MatrixTransform, float floort, float _enlarge_level, float _alpha = 1f)
			{
				this.drawtype = _drawtype;
				this.w = (ushort)_w;
				this.h = (ushort)_h;
				this.C = CMul.C;
				this.MatrixTransform = _MatrixTransform;
				this.enlarge_level = _enlarge_level;
				this.alpha = _alpha;
				this.time = (int)floort;
				this.FnDraw = null;
				this.Obj = null;
				return this;
			}

			public PxlImage Img;

			public ushort w;

			public ushort h;

			public EnemyAnimator.EYE_DRAW_TYPE drawtype;

			public Matrix4x4 MatrixTransform;

			public Color32 C;

			public int time;

			public float enlarge_level;

			public float alpha;

			public EnemyAnimator.FnDrawBitmapEye FnDraw;

			public object Obj;
		}

		public class EnemyAdditionalDrawFrame
		{
			public EnemyAdditionalDrawFrame(PxlFrame _F, EnemyAnimator.EnemyAdditionalDrawFrame.FnDrawEAD _fnDraw, bool _active = false)
			{
				this.F = _F;
				this.fnDraw = _fnDraw;
				this.active_ = _active;
			}

			public virtual bool active
			{
				get
				{
					return this.active_;
				}
				set
				{
					this.active_ = value;
				}
			}

			public PxlFrame F;

			protected bool active_;

			public EnemyAnimator.EnemyAdditionalDrawFrame.FnDrawEAD fnDraw;

			public delegate bool FnDrawEAD(MeshDrawer Md, EnemyAnimator.EnemyAdditionalDrawFrame Ead);
		}

		public interface IEnemyAnimListener
		{
			void fineFrameData(EnemyAnimator Anm, EnemyFrameDataBasic FrmData, bool created);

			int createEyes(EnemyAnimator Anm, Matrix4x4 MxAfterMultiple, ref int eyepos_search);

			void checkFrame(EnemyAnimator Anm, float TS);

			void drawEyeInnerInit(EnemyAnimator Anm);

			void destruct();
		}
	}
}
