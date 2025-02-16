using System;
using System.Collections.Generic;
using m2d;
using PixelLiner;
using PixelLiner.PixelLinerCore;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class NoelAnimator : ITeScaler, IAnimListener, ITeColor
	{
		public void releaseCache()
		{
			this.pre_pose_title = "stand";
		}

		public NoelAnimator(PRNoel _Pr, M2PxlAnimatorRT _Anm, PrPoseContainer _PCon)
		{
			this.Pr = _Pr;
			this.M2D = this.Pr.M2D as NelM2DBase;
			this.PCon = _PCon;
			this.SfPose = this.Pr.SfPose ?? new AnimationShuffler(this.Pr);
			this.MtrAlphaClipMask = MTRX.newMtr(MTRX.ShaderAlphaSplice);
			this.MdClip = new MeshDrawer(null, 4, 6);
			this.MdClip.draw_gl_only = true;
			this.MdClip.activate("", this.MtrAlphaClipMask, false, MTRX.ColWhite, null);
			this.FD_drawEffectMagicElec = new M2DrawBinder.FnEffectBind(this.drawEffectMagicElecInner);
			this.FD_RenderPrepareMesh = new M2RenderTicket.FnPrepareMd(this.RenderPrepareMesh);
			this.FD_ElecGetPos = new Func<Vector2>(this.getTargetRodPos);
			this.Cane = this.Pr.Mp.createMover<M2NoelCane>("NoelCane", this.Pr.x, this.Pr.y, false, false);
			this.Cane.do_not_destruct_when_remove = true;
			this.initDropGob(this.Cane);
			this.initHat();
			this.FlgDropCane = new Flagger(delegate(FlaggerT<string> V)
			{
				this.initRodErase();
			}, delegate(FlaggerT<string> V)
			{
				this.resetRodErase();
			});
			this.MtrBase = MTRX.newMtr(MTRX.ShaderGDTP3);
			this.Cane.gameObject.SetActive(false);
			this.MtrFrozen = MTR.newMtrFrozenMain();
			this.MtrFrozen.SetFloat("_ZTest", 8f);
			this.MtrStone = MTR.newMtrStone();
			this.MtrStone.SetFloat("_ZTest", 8f);
			this.initS(_Anm);
			this.setCaneDefault();
		}

		public void destruct()
		{
			IN.DestroyOne(this.MtrBase);
			IN.DestroyOne(this.MtrFrozen);
			IN.DestroyOne(this.MtrStone);
			if (this.MdFront != null)
			{
				this.MdFront.destruct();
			}
		}

		private void initHat()
		{
			this.Hat = this.Pr.Mp.createMover<M2NoelHat>("NoelHat", this.Pr.x, this.Pr.y, false, false);
			this.Hat.do_not_destruct_when_remove = true;
			this.Hat.initImage(this.PCon.getHatImgArray());
			this.Hat.gameObject.SetActive(false);
			this.initDropGob(this.Hat);
		}

		public void prepareDestruct()
		{
			this.Hat.do_not_destruct_when_remove = false;
			this.Cane.do_not_destruct_when_remove = false;
		}

		private void removeHat()
		{
			if (!this.Hat.gameObject.activeSelf)
			{
				return;
			}
			this.Mp.removeMover(this.Hat);
			this.Hat.gameObject.SetActive(false);
			this.initDropGob(this.Hat);
		}

		private void initDropGob(M2NoelDropCloth Cane)
		{
			Cane.gameObject.transform.SetParent(null, false);
		}

		public NoelAnimator initS(M2PxlAnimatorRT _Anm)
		{
			this.hidden_flag = false;
			this.Anm = _Anm;
			this.Anm.auto_assign_tecon = false;
			this.Anm.auto_replace_mesh = (this.Anm.auto_replace_matrix = false);
			this.Anm.timescale = 1f;
			this.Anm.ReplaceCurrentFrame = new Func<PxlSequence, int, PxlFrame>(this.fnReplaceCurrentFrame);
			this.Anm.allow_pose_jump = false;
			this.Anm.FnReplaceRender = this.FD_RenderPrepareMesh;
			this.pose_down_turning = (this.torture_by_invisible_enemy = false);
			this.hat_assign_recheck = true;
			this.FlgDropCane.Clear();
			if (this.CurInfo == null)
			{
				this.CurInfo = this.PCon.Get("stand");
				this.Anm.setPose(this.pose_title, -1);
			}
			else
			{
				this.Anm.setPose(this.PreTargetPose, -1);
			}
			this.need_fine_mesh = true;
			this.checkFrame(true);
			this.SfPose.initS();
			if (this.Cane.gameObject.activeSelf)
			{
				this.Cane.gameObject.SetActive(false);
			}
			if (this.RtkFront != null && this.Mp != null)
			{
				this.Mp.MovRenderer.deassignDrawable(this.RtkFront, -1);
			}
			this.RtkFront = null;
			this.HoldMagicEd = null;
			this.fineMaterialColor();
			this.Pr.TeCon.clearRegistered();
			this.Pr.TeCon.RegisterCol(this, false);
			this.Pr.TeCon.RegisterPos(this.Anm);
			this.fineFrozenAppearance(false);
			return this;
		}

		public void runPost(float f)
		{
			NoelAnimator.prepareHoldElecAndEd(this.Mp, this.Pr.getSkillManager().magic_chant_completed, ref this.HoldMagicElec, ref this.HoldMagicEd, this.FD_drawEffectMagicElec, true);
		}

		public bool updateAnimator(float f)
		{
			if (global::XX.X.D)
			{
				if (Map2d.TS == 0f || this.Pr.isTortureAbsorbed())
				{
					this.Anm.need_fine = true;
					this.Anm.runPre(0f);
				}
				else
				{
					f *= f * (float)global::XX.X.AF * this.Anm.timescale * this.Pr.animator_TS;
					this.runPost(f);
					if (this.Anm.updateAnimator(f))
					{
						this.need_fine = true;
					}
				}
				return true;
			}
			return false;
		}

		public void endS()
		{
			this.removeHat();
		}

		private void checkFrame(bool force = false)
		{
			if (this.fine_rot)
			{
				this.fine_rot = false;
			}
			if (!force && !this.need_fine)
			{
				return;
			}
			this.need_fine = false;
			PxlPose pxlPose = this.Anm.getCurrentPose();
			if (!this.isTortured() && pxlPose.end_jump_title != "" && this.Anm.get_loop_count() >= pxlPose.end_jump_loop_count)
			{
				bool flag = true;
				if (this.poseIs(POSE_TYPE.JUMP_ON_GROUND, false) && !this.Pr.canJump())
				{
					flag = false;
				}
				if (flag)
				{
					this.setPose(pxlPose.end_jump_title, 1, true);
					this.Anm.runPre(0f);
					pxlPose = this.Anm.getCurrentPose();
				}
			}
			PxlFrame pxlFrame;
			if (this.FrozenF != null && !this.Pr.frozenAnimReplaceable(true))
			{
				pxlFrame = this.FrozenF;
			}
			else
			{
				pxlFrame = this.Anm.getCurrentSequence().getFrame(this.cframe);
				if (this.FrozenF != null)
				{
					this.fineFreezeFrame(true, true);
				}
			}
			PrPoseContainer.NFrame nframe;
			if (!this.CurInfo.OFrmData.TryGetValue(pxlFrame, out nframe))
			{
				nframe = (this.CurInfo.OFrmData[pxlFrame] = new PrPoseContainer.NFrame(pxlFrame, this.Anm.Mp.rCLENB));
			}
			if (nframe == this.CurFrame)
			{
				return;
			}
			this.need_fine_mesh = true;
			if (nframe.clip_mask_layer != 0 && (this.CurInfo.type & POSE_TYPE.MARUNOMI) == POSE_TYPE.STAND)
			{
				int num = pxlFrame.countLayers();
				int num2 = nframe.clip_mask_layer;
				this.MdClip.clearSimple();
				this.MdClip.Col = this.MdClip.ColGrd.White().mulA(this.torture_by_invisible_enemy ? 0.66f : 1f).C;
				for (int i = 0; i < num; i++)
				{
					if ((num2 & (1 << i)) != 0)
					{
						num2 &= ~(1 << i);
						PxlLayer layer = pxlFrame.getLayer(i);
						this.MdClip.RotaL(0f, 0f, layer, false, false, 0);
						if (num2 == 0)
						{
							break;
						}
					}
				}
				this.MdClip.updateForMeshRenderer(false);
			}
			this.PPFrame = this.PFrame;
			this.PFrame = this.CurFrame;
			this.CurFrame = nframe;
			if (this.Mp.floort >= 4f)
			{
				if (this.PFrame != null && this.PPFrame != null)
				{
					bool flag2 = ((this.ARodErasePose != null) ? null : this.CurFrame.RodL) != null || this.poseIs(POSE_TYPE.NO_FALL_CANE, false) || this.outfit_type > PRNoel.OUTFIT.NORMAL;
					if (!flag2 && this.PFrame.RodL != null && !this.Cane.gameObject.activeSelf)
					{
						float num3 = 0f;
						float num4 = 0f;
						float num5 = 0f;
						float num6;
						if (this.PPFrame.RodL != null)
						{
							num3 = this.PFrame.rodx - this.PPFrame.rodx;
							num4 = this.PFrame.rody - this.PPFrame.rody;
							num5 = this.PFrame.RodL.rotR - this.PPFrame.RodL.rotR;
							num6 = -num5 * global::XX.X.NIXP(0.85f, 1.15f);
						}
						else
						{
							num6 = (float)global::XX.X.MPF(global::XX.X.XORSP() > 0.5f) * global::XX.X.NIXP(6f, 30f) / 180f * 3.1415927f / 60f;
						}
						num3 = global::XX.X.absMn(num3, 0.06f) + this.Pr.vx * 0.3f;
						num4 = global::XX.X.absMn(num4 + global::XX.X.absMn(this.Pr.vy, 0.05f) * 0.4f - 0.2f, 0.08f);
						this.appearCane(this.PFrame.rodx + num3 * 0.75f, this.PFrame.rody + num4 * 0.75f, num3 * global::XX.X.NIXP(0.2f, 0.5f), num4 * global::XX.X.NIXP(0.2f, 0.5f), -(this.PFrame.RodL.rotR + num5 * 0.6f), num6);
						if (this.Pr.isAbsorbState())
						{
							this.FlgDropCane.Add("ABSORB");
						}
						else if (this.Pr.isDamagingOrKo())
						{
							this.FlgDropCane.Add("DMG");
						}
					}
					else if (flag2 && this.Cane.gameObject.activeSelf)
					{
						this.hideCane();
						this.pre_rod_erase = false;
					}
				}
				if (this.CurFrame.hatlost)
				{
					this.hat_assign_recheck = false;
					if (!this.Hat.gameObject.activeSelf)
					{
						bool flag3 = true;
						if (this.cframe >= 2)
						{
							PxlFrame frame = pxlFrame.pSq.getFrame(this.cframe - 1);
							PxlFrame frame2 = pxlFrame.pSq.getFrame(this.cframe - 2);
							PxlLayer layerByName = frame.getLayerByName("hat");
							PxlLayer layerByName2 = frame2.getLayerByName("hat");
							if (layerByName != null && layerByName2 != null)
							{
								flag3 = false;
								float num7 = global::XX.X.NI(layerByName2.x, layerByName.x, 1.5f);
								float num8 = global::XX.X.NI(layerByName2.y, layerByName.y, 1.2f);
								float num9 = global::XX.X.NI(layerByName2.rotR, layerByName.rotR, 1.5f);
								this.Hat.initObject(this, layerByName.Img, 1f);
								this.appearDropObject(this.Hat, num7 * this.Mp.rCLENB, num8 * this.Mp.rCLENB, (num7 - layerByName.x) * 0.03f * this.Mp.rCLENB, (num8 - layerByName.y) * 0.03f * this.Mp.rCLENB, -num9, -(num9 - layerByName.rotR) * 0.012f);
							}
						}
						if (flag3)
						{
							Vector3 headPos = this.getHeadPos();
							this.Hat.initObject(this, this.PCon.getHatImgArray()[0], 1f);
							this.appearDropObject(this.Hat, headPos.x - this.mv_anmx, headPos.y - this.mv_anmy, 0f, 0f, global::XX.CAim.get_agR((global::XX.AIM)headPos.z, 0f) + 3.1415927f, global::XX.X.NIXP(-0.004f, 0.004f) * 3.1415927f);
						}
					}
				}
				if (this.hat_assign_recheck)
				{
					this.hat_assign_recheck = false;
					if (this.Hat.Mp != null)
					{
						this.removeHat();
					}
				}
			}
		}

		public void fineFreezeFrame(bool force = false, bool fine_texture = true)
		{
			if (force || this.Pr.isAnimationFrozen())
			{
				if (force || this.FrozenF == null)
				{
					PxlFrame frame = this.Anm.getCurrentSequence().getFrame(this.cframe);
					fine_texture = fine_texture && (this.FrozenF == null || this.FrozenF.getImageTexture() != frame.getImageTexture());
					this.FrozenF = frame;
					this.FrozenInfo = this.CurInfo;
					this.Pr.need_check_bounds = true;
				}
				if (fine_texture && (this.Anm.getRendererMaterial() == this.MtrFrozen || this.Anm.getRendererMaterial() == this.MtrStone))
				{
					this.fineFrozenAppearance(false);
					return;
				}
			}
			else
			{
				this.FrozenF = null;
			}
		}

		public byte frozen_lv
		{
			get
			{
				return this.frozen_lv_;
			}
			set
			{
				if (this.frozen_lv == value)
				{
					return;
				}
				this.frozen_lv_ = value;
				this.need_fine_frozen_appear = true;
			}
		}

		public byte stone_lv
		{
			get
			{
				return this.stone_lv_;
			}
			set
			{
				if (this.stone_lv == value)
				{
					return;
				}
				this.stone_lv_ = value;
				this.need_fine_mesh = true;
				this.need_fine_frozen_appear = true;
			}
		}

		public bool use_mtr_stone
		{
			get
			{
				return this.stone_lv > 0 && this.frozen_lv == 0;
			}
		}

		public void fineFrozenAppearance(bool force = false)
		{
			Material material;
			if ((this.frozen_lv > 0 || this.stone_lv > 0) && this.frz_state != NoelAnimator.FRZ_STATE.NORMAL)
			{
				material = (this.use_mtr_stone ? this.MtrStone : this.MtrFrozen);
				if (this.need_fine_frozen_appear || force)
				{
					this.need_fine_frozen_appear = false;
					if (this.use_mtr_stone)
					{
						material.SetColor("_BColor", MTRX.ColBlack);
						material.SetColor("_Color", C32.d2c(4288716960U));
						material.SetFloat("_Level", 0.4f + (float)global::XX.X.Mx(this.stone_lv, this.frozen_lv) * 0.16f);
					}
					else
					{
						C32 cola = MTRX.cola;
						float num = (float)(((this.frz_state & NoelAnimator.FRZ_STATE.FROZEN) != NoelAnimator.FRZ_STATE.NORMAL) ? 1 : 0);
						float num2 = (((this.frz_state & NoelAnimator.FRZ_STATE.WEB_TRAPPED) != NoelAnimator.FRZ_STATE.NORMAL) ? 0.75f : 0f);
						float num3 = (((this.frz_state & NoelAnimator.FRZ_STATE.STONE) != NoelAnimator.FRZ_STATE.NORMAL) ? 5.5f : 0f);
						material.SetColor("_Color", cola.blend3(4288008150U, 4291808711U, 4289965998U, num, num2, num3).C);
						material.SetColor("_BColor", cola.blend3(4279786863U, 4290294960U, 4286349697U, num, num2, num3).C);
						material.SetColor("_WColor", cola.blend3(4292867578U, 4293980142U, 4291940039U, num, num2, num3).C);
						material.SetFloat("_Level", 0.3f + (float)global::XX.X.Mx(this.stone_lv, this.frozen_lv) * 0.17f);
					}
				}
				Texture currentTexture = this.Anm.getCurrentTexture();
				if (currentTexture != null)
				{
					float num4 = (float)currentTexture.width / 512f * 4f;
					float num5 = (float)currentTexture.height / 512f * 4f;
					material.SetFloat("_ScaleX", num4);
					material.SetFloat("_ScaleY", num5);
				}
			}
			else
			{
				material = this.MtrBase;
			}
			this.need_fine_mesh = true;
			this.Anm.setRendererMaterial(material);
			if (this.MdFront != null)
			{
				this.MdFront.setMaterial(material, false);
			}
			this.MdClip.getMaterial().SetTexture("_MainTex", this.Anm.getCurrentDrawnTexture());
		}

		public PxlFrame fnReplaceCurrentFrame(PxlSequence pSq, int i)
		{
			if (this.FrozenF != null && !this.Pr.fine_frozen_replace)
			{
				return this.FrozenF;
			}
			if (pSq == null)
			{
				return null;
			}
			return pSq.getFrame(i);
		}

		public void appearCane(float depx, float depy, float vx, float vy, float rotR, float rotspdR)
		{
			if (this.Cane.gameObject.activeSelf)
			{
				return;
			}
			this.appearDropObject(this.Cane, depx, depy, vx, vy, rotR, rotspdR);
			if (this.CaneB != null)
			{
				this.checkBreakEffect(true);
			}
		}

		public void appearDropObject(M2NoelDropCloth Drop, float depx, float depy, float vx, float vy, float rotR, float rotspdR)
		{
			if (Drop.gameObject.activeSelf)
			{
				return;
			}
			Drop.gameObject.SetActive(true);
			this.Pr.Mp.assignMover(Drop, true);
			Drop.appearAt(this.mv_anmx + depx, this.mv_anmy + depy, vx, vy, rotR, rotspdR);
		}

		public void hideCane()
		{
			if (!this.Cane.gameObject.activeSelf)
			{
				return;
			}
			this.Pr.Mp.removeMover(this.Cane);
			if (this.CaneB != null)
			{
				this.Pr.Mp.removeMover(this.CaneB);
				this.CaneB = null;
			}
		}

		public void repairCane()
		{
			if (this.CaneB != null)
			{
				if (this.CaneB.gameObject.activeSelf)
				{
					this.Pr.Mp.removeMover(this.CaneB);
				}
				global::UnityEngine.Object gameObject = this.CaneB.gameObject;
				IN.DestroyOne(this.CaneB);
				IN.DestroyOne(gameObject);
				this.CaneB = null;
			}
			this.setCaneDefault();
		}

		public void setCaneDefault()
		{
			PxlFrame frame = this.PCon.getRodPose().getSequence(0).getFrame(0);
			this.Cane.initCane(this, frame.getLayerByName("rod_base").Img, 1f, true);
		}

		private void initRodErase()
		{
			this.resetRodErase();
			this.ARodErasePose = new List<PxlPose>();
			this.checkRodErase();
		}

		private void checkRodErase()
		{
			if (this.ARodErasePose == null)
			{
				return;
			}
			PxlPose pxlPose = ((this.FrozenF != null) ? this.FrozenF.pPose : this.Anm.getCurrentPose());
			if (this.ARodErasePose.IndexOf(pxlPose) >= 0)
			{
				return;
			}
			this.ARodErasePose.Add(pxlPose);
			int num = 0;
			while ((long)num < 8L)
			{
				if (pxlPose.isValidAim(num))
				{
					PxlSequence sequence = pxlPose.getSequence(num);
					int num2 = sequence.countFrames();
					for (int i = 0; i < num2; i++)
					{
						PxlFrame frame = sequence.getFrame(i);
						PxlLayer pxlLayer = null;
						PxlLayer specificLayer = M2PxlAnimator.getSpecificLayer(frame, "", "ROD", ref pxlLayer, null);
						if (specificLayer != null)
						{
							specificLayer.alpha = 0f;
							frame.Apply();
						}
					}
				}
				num++;
			}
		}

		private void resetRodErase()
		{
			if (this.ARodErasePose == null)
			{
				return;
			}
			for (int i = this.ARodErasePose.Count - 1; i >= 0; i--)
			{
				PxlPose pxlPose = this.ARodErasePose[i];
				int num = 0;
				while ((long)num < 8L)
				{
					if (pxlPose.isValidAim(num))
					{
						PxlSequence sequence = pxlPose.getSequence(num);
						int num2 = sequence.countFrames();
						for (int j = 0; j < num2; j++)
						{
							PxlFrame frame = sequence.getFrame(j);
							PxlLayer pxlLayer = null;
							PxlLayer specificLayer = M2PxlAnimator.getSpecificLayer(frame, "", "ROD", ref pxlLayer, null);
							if (specificLayer != null)
							{
								specificLayer.alpha = 100f;
								frame.Apply();
							}
						}
					}
					num++;
				}
			}
			this.pre_rod_erase = true;
			this.ARodErasePose = null;
		}

		public M2NoelCane checkBreakEffect(bool flag)
		{
			if (!this.Pr.is_alive && !this.cane_broken_active && flag)
			{
				Vector2 vector = this.Cane.transform.localPosition;
				if (this.CaneB == null)
				{
					this.CaneB = this.Pr.Mp.createMover<M2NoelCane>("NoelCaneB", this.Pr.x, this.Pr.y, false, false);
					this.Pr.Mp.playSnd("cane_poki", "", this.Pr.Mp.uxToMapx(vector.x), this.Pr.Mp.uyToMapy(vector.y), 1);
					this.initDropGob(this.CaneB);
				}
				PxlFrame frame = this.PCon.getRodPose().getSequence(0).getFrame(0);
				PxlLayer layerByName = frame.getLayerByName("rod_0");
				PxlLayer layerByName2 = frame.getLayerByName("rod_1");
				this.Cane.initCane(this, layerByName.Img, 1f, true);
				this.CaneB.initCane(this, layerByName2.Img, 1f, false);
				this.CaneB.gameObject.layer = this.Cane.gameObject.layer;
				if (this.Cane.gameObject.activeSelf)
				{
					this.Pr.Mp.assignMover(this.CaneB, true);
				}
				else
				{
					this.CaneB.gameObject.SetActive(false);
				}
				float num = this.Cane.transform.localEulerAngles.z / 180f * 3.1415927f;
				float num2 = (float)(layerByName.Img.width + layerByName2.Img.width);
				Vector2 vector2 = global::XX.X.ROTV2e(new Vector2(-num2 / 2f + (float)(layerByName.Img.width / 2), 0f), num) / this.Mp.base_scale * 0.015625f;
				Vector2 vector3 = global::XX.X.ROTV2e(new Vector2(num2 / 2f - (float)(layerByName2.Img.width / 2), 0f), num) / this.Mp.base_scale * 0.015625f;
				this.Cane.transform.localPosition = vector + vector2;
				this.CaneB.transform.localPosition = vector + vector3;
				float num3 = global::XX.X.XORSPS() * 1.5707964f * 0.8f + 1.5707964f;
				float num4 = global::XX.X.NIXP(0.05f, 0.1f);
				this.CaneB.appearAfterVelocity(this.Cane.vx + num4 * global::XX.X.Cos(num3), this.Cane.vy - 0.02f - num4 * global::XX.X.Sin(num3), num, this.Cane.angleSpeedR + global::XX.X.NIXP(-0.004f, 0.004f) * 3.1415927f);
				float num5 = (-1f + 2f * global::XX.X.XORSP()) * 0.03f;
				this.CaneB.getPhysic().addFoc(FOCTYPE.WALK, num5, -0.03f, -1f, 0, 0, 60, 10, 0);
				this.Cane.getPhysic().addFoc(FOCTYPE.WALK, -num5, -0.05f, -1f, 0, 0, 60, 10, 0);
				return this.CaneB;
			}
			return null;
		}

		public static void prepareHoldElecAndEd(Map2d Mp, bool flag, ref ElecTraceDrawer HoldMagicElec, ref M2DrawBinder Ed, M2DrawBinder.FnEffectBind Fn, bool is_pr = true)
		{
			if (flag)
			{
				if (HoldMagicElec == null)
				{
					ElecTraceDrawer elecTraceDrawer;
					HoldMagicElec = (elecTraceDrawer = new ElecTraceDrawer(11f, 4f));
					ElecTraceDrawer elecTraceDrawer2 = elecTraceDrawer;
					elecTraceDrawer2.kaku = 7;
					elecTraceDrawer2.min_minimize_level = 0.9f;
					elecTraceDrawer2.jump_ratio = 0.91f;
					if (is_pr)
					{
						elecTraceDrawer2.OutColor(1442825052U).OutEndColor(866678303U);
					}
					else
					{
						elecTraceDrawer2.OutColor(1432968447U).OutEndColor(857109708U);
					}
					elecTraceDrawer2.JumpHeight(20f, 14f).DivideWidth(26f).BallRadius(2f, 11f)
						.Thick(4f, 11f);
				}
				if (Ed == null)
				{
					HoldMagicElec.release();
					Ed = Mp.setED("HoldElec", Fn, 0f);
					return;
				}
			}
			else if (Ed != null)
			{
				Ed = Mp.remED(Ed);
			}
		}

		private bool drawEffectMagicElecInner(EffectItem E, M2DrawBinder Ed)
		{
			return this.HoldMagicElec != null && NoelAnimator.drawEffectMagicElecS(this.Mp, E, Ed, this.HoldMagicElec, this.FD_ElecGetPos, true);
		}

		public static bool drawEffectMagicElecS(Map2d Mp, EffectItem E, M2DrawBinder Ed, ElecTraceDrawer HoldMagicElec, Func<Vector2> fnGetPos, bool is_pr)
		{
			bool flag;
			MeshDrawer meshDrawer = E.EF.MeshInit("", 0f, 0f, MTRX.ColWhite, out flag, BLEND.ADD, false);
			MeshDrawer meshDrawer2 = E.EF.MeshInit("", 0f, 0f, MTRX.ColWhite, out flag, BLEND.SUB, false);
			meshDrawer.base_x = (meshDrawer.base_y = (meshDrawer2.base_x = (meshDrawer2.base_y = 0f)));
			if (HoldMagicElec.need_fine_pos)
			{
				Vector2 vector = fnGetPos();
				float num = Mp.ux2effectScreenx(Mp.map2ux(vector.x));
				float num2 = Mp.uy2effectScreeny(Mp.map2uy(vector.y));
				HoldMagicElec.Add(num * 64f, num2 * 64f);
			}
			C32 cola = MTRX.cola;
			float num3 = global::XX.X.Mn(1f, 0.8f + 0.16f * global::XX.X.COSI(E.af, 3.19f) + 0.16f * global::XX.X.COSI(E.af + 333f, 13.22f));
			if (is_pr)
			{
				HoldMagicElec.InnerColor(cola.Set(3438870015U).mulA(num3).rgba, cola.Set(4290442495U).mulA(num3).rgba).InnerEndColor(cola.Set(2570953121U).mulA(num3).rgba);
			}
			else
			{
				HoldMagicElec.InnerColor(cola.Set(3439125616U).mulA(num3).rgba, cola.Set(4292133523U).mulA(num3).rgba).InnerEndColor(cola.Set(2577933385U).mulA(num3).rgba);
			}
			HoldMagicElec.draw(meshDrawer, meshDrawer2, (float)global::XX.X.AF_EF);
			return true;
		}

		public void finePose(int restart_anim = 0)
		{
			this.setPose(this.pose_title0, restart_anim, false);
		}

		public void setPose(string title, int restart_anim = -1, bool loop_jumping = false)
		{
			string text = title;
			string title2 = this.Anm.getCurrentPose().title;
			title = this.SfPose.initSetPoseA(title, restart_anim, loop_jumping);
			if (title == null)
			{
				return;
			}
			bool flag = this.pose_title0 == text;
			string text2 = text;
			title = this.SfPose.initSetPoseB(title, title2, loop_jumping);
			if (title == null)
			{
				return;
			}
			if (restart_anim < 0 && flag && title == this.pre_pose_title)
			{
				return;
			}
			this.pose_title0 = text2;
			this.pre_pose_title = title;
			PxlPose pxlPose = null;
			PrPoseContainer.PoseInfo curInfo = this.CurInfo;
			PrPoseContainer.PoseInfo poseInfo = (this.CurInfo = this.getPoseInfo(text, ref title, ref pxlPose));
			this.PCon.initPoseInfo(this.CurInfo, this.Mp.rCLENB);
			this.cur_type = poseInfo.type;
			global::UnityEngine.Object currentTexture = this.Anm.getCurrentTexture();
			this.Anm.setPose(pxlPose, -1000);
			this.PreTargetPose = pxlPose;
			if (!loop_jumping)
			{
				this.Anm.timescale = 1f;
				this.hat_assign_recheck = true;
			}
			this.Pr.need_check_bounds = true;
			this.Anm.PointData = poseInfo.PointData;
			this.Anm.check_torture = poseInfo.use_torture;
			this.Anm.timescale = (float)(poseInfo.use_torture ? 0 : 1);
			Texture currentDrawnTexture = this.Anm.getCurrentDrawnTexture();
			if (currentTexture != currentDrawnTexture)
			{
				this.fineMaterialColor();
				this.fineFrozenAppearance(false);
			}
			if (poseInfo.use_front_drawing)
			{
				if (this.MdFront == null)
				{
					this.MdFront = new MeshDrawer(null, 4, 6);
					this.MdFront.draw_gl_only = true;
					this.MdFront.setMaterial(this.Anm.getRendererMaterial(), false);
					this.FD_FnPrepareFrontMd = new M2RenderTicket.FnPrepareMd(this.FnPrepareFrontMd);
				}
				if (this.RtkFront == null)
				{
					this.RtkFront = this.Mp.MovRenderer.assignDrawable(M2Mover.DRAW_ORDER.N_TOP2, this.Anm.transform, this.FD_FnPrepareFrontMd, this.MdFront, this.Pr, null);
				}
			}
			else
			{
				if (this.RtkFront != null && this.Mp != null)
				{
					this.Mp.MovRenderer.deassignDrawable(this.RtkFront, -1);
				}
				this.RtkFront = null;
			}
			if (restart_anim != -1000)
			{
				if (poseInfo.use_torture)
				{
					this.pose_down_turning = false;
					this.Anm.setAim((int)this.Pr.aim, -1);
				}
				else if (loop_jumping)
				{
					this.Anm.setAim(this.pose_aim, 1);
				}
				else
				{
					global::XX.AIM aim = (global::XX.AIM)((loop_jumping || this.Pr.fix_aim) ? this.Anm.pose_aim : ((global::PixelLiner.PixelLinerCore.AIM)this.Pr.aim));
					POSE_TYPE type = curInfo.type;
					restart_anim = ((restart_anim == -1) ? ((title2 == title) ? 0 : 1) : restart_anim);
					if ((type & POSE_TYPE.DOWN) != POSE_TYPE.STAND && (this.cur_type & POSE_TYPE.DOWN) != POSE_TYPE.STAND)
					{
						if ((type & POSE_TYPE.BACK) > POSE_TYPE.STAND != (this.cur_type & POSE_TYPE.BACK) > POSE_TYPE.STAND)
						{
							this.pose_down_turning = !this.pose_down_turning;
						}
						this.Anm.setAim(global::XX.CAim.toPxlAim(global::XX.CAim.get_aim2(0f, 0f, (float)(global::XX.CAim._XD(aim, 1) * global::XX.X.MPF(!this.pose_down_turning)), (float)global::XX.CAim._YD(aim, 1), false)), restart_anim);
					}
					else
					{
						this.pose_down_turning = false;
						this.Anm.setAim(global::XX.CAim.toPxlAim(global::XX.CAim.get_aim2(0f, 0f, (float)global::XX.CAim._XD(aim, 1), (float)global::XX.CAim._YD(aim, 1), false)), restart_anim);
					}
				}
			}
			if ((poseInfo.type & POSE_TYPE.PRESS_DAMAGE) != POSE_TYPE.STAND)
			{
				this.Anm.timescale = 0f;
				this.Anm.animReset(global::XX.X.xors(this.Anm.getCurrentSequence().countFrames()));
			}
			if (this.ARodErasePose != null)
			{
				this.checkRodErase();
			}
		}

		private PrPoseContainer.PoseInfo getPoseInfo(string title0, ref string title, ref PxlPose TargetPose)
		{
			PrPoseContainer.PoseInfo poseInfo = null;
			PrPoseContainer.PoseInfo poseInfo2 = null;
			PrPoseContainer.PoseInfo poseInfo3 = null;
			int num = global::XX.X.Mx((int)this.outfit_type, 1);
			int num2 = num;
			while (num2 >= 0 && poseInfo == null)
			{
				int i = 0;
				while (i < 2)
				{
					string text = ((i == 0) ? title : title0);
					PrPoseContainer.PoseInfo poseInfo4 = null;
					if (num2 != num)
					{
						poseInfo4 = ((i == 0) ? poseInfo2 : poseInfo3);
						goto IL_005F;
					}
					if (this.PCon.TryGetValue(text, out poseInfo4))
					{
						if (i == 0)
						{
							poseInfo2 = poseInfo4;
							goto IL_005F;
						}
						poseInfo3 = poseInfo4;
						goto IL_005F;
					}
					IL_00B5:
					i++;
					continue;
					IL_005F:
					if (poseInfo4 == null)
					{
						goto IL_00B5;
					}
					PxlPose pxlPose = poseInfo4.Get(num2);
					if (pxlPose != null && this.sensitive_check(num2))
					{
						poseInfo = poseInfo4;
						TargetPose = pxlPose;
						title = TargetPose.title;
						if (num2 == 0 && global::XX.X.SENSITIVE && poseInfo.PI_S != null)
						{
							poseInfo = poseInfo.PI_S;
							TargetPose = poseInfo.Get(0);
							title = text;
						}
						return poseInfo;
					}
					goto IL_00B5;
				}
				num2--;
			}
			if (poseInfo == null)
			{
				global::XX.X.de("Noel ポーズが見つかりません: " + title, null);
				title = "stand";
				return this.getPoseInfo(title0, ref title, ref TargetPose);
			}
			return poseInfo;
		}

		private bool sensitive_check(int _t)
		{
			return _t != 1 || (!global::XX.X.SENSITIVE && this.Pr.BetoMng.is_torned);
		}

		public Color32 getColorTe()
		{
			return MTRX.ColWhite;
		}

		public void setColorTe(C32 Buf, C32 CMul, C32 CAdd)
		{
			this.Anm.color = CMul.C;
			this.Anm.CAdd = CAdd.C;
			this.fineMaterialColor();
		}

		public void fineMaterialColor()
		{
			this.need_fine_mesh = true;
		}

		public void copyNormal3Attribute(MeshDrawer _Md, bool copy_color = true, float translate_lvl = 1f, float scale_lvl = 1f)
		{
			if (copy_color)
			{
				_Md.Col = this.Anm.color;
				_Md.Uv23(this.Anm.CAdd, false);
			}
			if (translate_lvl != 0f)
			{
				Vector2 vector = this.Anm.getShiftTe() * 0.015625f;
				_Md.Translate(vector.x * translate_lvl, vector.y * translate_lvl, true);
			}
			if (scale_lvl != 0f)
			{
				_Md.Scale((this.TeScale.x - 1f) * scale_lvl + 1f, (this.TeScale.y - 1f) * scale_lvl + 1f, true);
			}
		}

		private MeshDrawer reentryMainMesh()
		{
			MeshDrawer mainMeshDrawer = this.Anm.getMainMeshDrawer();
			if (this.need_fine_mesh)
			{
				this.need_fine_mesh = false;
				mainMeshDrawer.clearSimple();
				mainMeshDrawer.Uv23(this.Anm.CAdd, false);
				mainMeshDrawer.setMaterial(this.Anm.getRendererMaterial(), false);
				mainMeshDrawer.ColGrd.Set(this.Anm.color);
				if (this.stone_lv_ == 1 || this.stone_lv_ == 2)
				{
					mainMeshDrawer.ColGrd.blend(4278190080U, 0.33f);
				}
				mainMeshDrawer.Col = mainMeshDrawer.ColGrd.C;
				if ((this.CurInfo.type & POSE_TYPE.PRESS_DAMAGE) != POSE_TYPE.STAND)
				{
					if ((this.CurInfo.type & POSE_TYPE.DOWN) != POSE_TYPE.STAND)
					{
						Matrix4x4 currentMatrix = mainMeshDrawer.getCurrentMatrix();
						float num = 0f;
						float num2 = 0f;
						int num3 = global::XX.CAim._YD(this.pose_aim_visible, 1);
						if (num3 != 0 && this.Pr.is_alive)
						{
							float pressdamage_float_level = this.Pr.pressdamage_float_level;
							num = ((num3 == -1) ? (global::XX.X.ZSIN(pressdamage_float_level, 0.3f) * 24f) : 0f) - global::XX.X.ZCOS(pressdamage_float_level - 0.25f, 0.75f) * (float)((num3 == -1) ? 24 : 15);
							num2 = global::XX.X.SINI(global::XX.X.ZSIN(pressdamage_float_level), 1f) * (float)global::XX.CAim._XD(this.pose_aim_visible, 1) * 3.1415927f * 0.16f;
						}
						mainMeshDrawer.TranslateP(0f, -70f + num, true).Rotate(num2, false).Scale(1f, this.Pr.pressdamage_scale_level, true)
							.TranslateP(0f, 70f, true);
						mainMeshDrawer.RotaPF(0f, 0f, 1f, 1f, 0f, this.Anm.getCurrentDrawnFrame(), false, true, false, uint.MaxValue, false, 0);
						mainMeshDrawer.setCurrentMatrix(currentMatrix, false);
					}
					else
					{
						mainMeshDrawer.RotaPF(0f, 0f, this.Pr.pressdamage_scale_level, 1f, 0f, this.Anm.getCurrentDrawnFrame(), false, true, false, uint.MaxValue, false, 0);
					}
					this.need_fine_mesh = true;
				}
				else
				{
					mainMeshDrawer.RotaPF(0f, 0f, 1f, 1f, 0f, this.Anm.getCurrentDrawnFrame(), false, true, false, uint.MaxValue, false, 0);
				}
				mainMeshDrawer.allocUv23(0, true);
				mainMeshDrawer.updateForMeshRenderer(false);
				if (this.RtkFront != null)
				{
					this.drawFrontMesh();
				}
			}
			return mainMeshDrawer;
		}

		private void drawFrontMesh()
		{
			this.MdFront.clear(false, false);
			if (this.CurInfo == null)
			{
				return;
			}
			PxlFrame currentDrawnFrame = this.Anm.getCurrentDrawnFrame();
			int num = currentDrawnFrame.countLayers();
			bool flag = false;
			bool flag2 = true;
			bool flag3 = (this.CurInfo.type & POSE_TYPE.MARUNOMI) > POSE_TYPE.STAND;
			for (int i = 0; i < num; i++)
			{
				PxlLayer layer = currentDrawnFrame.getLayer(i);
				if (TX.isStart(layer.name, "enemy", 0))
				{
					flag = true;
				}
				else
				{
					if (!flag && TX.isStart(layer.name, "top_layer", 0))
					{
						flag = true;
					}
					float num2 = (float)global::XX.X.MPF(!flag3) * layer.alpha;
					if (flag && num2 > 0f)
					{
						float num3 = 1f;
						if (flag3 && CFGSP.opacity_marunomi < 100 && TX.isStart(layer.name, "marunomi", 0))
						{
							num3 *= (float)CFGSP.opacity_marunomi * 0.01f;
							if (num3 <= 0f)
							{
								goto IL_0220;
							}
						}
						if (flag2)
						{
							flag2 = false;
							this.MdFront.allocUv23(16, false);
							Vector3 localScale = this.Anm.Trs.localScale;
							this.MdFront.Scale(localScale.x * 2f, localScale.y * 2f, false);
							this.MdFront.Rotate(this.Anm.Trs.localEulerAngles.z / 180f * 3.1415927f, false);
						}
						this.MdFront.initForImg(layer.Img, 0);
						this.MdFront.ColGrd.White();
						if (TX.isStart(layer.name, "Layer", 0) || TX.isStart(layer.name, "top_layer", 0))
						{
							this.MdFront.ColGrd.Set(this.Anm.color);
						}
						this.MdFront.Col = this.MdFront.ColGrd.mulA(num2 * num3 * 0.01f).C;
						this.MdFront.RotaL(0f, 0f, layer, true, true, 0);
					}
				}
				IL_0220:;
			}
			if (!flag2)
			{
				this.MdFront.Uv23(this.Anm.CAdd, false);
				this.MdFront.allocUv23(0, true);
				this.MdFront.Identity();
			}
		}

		private bool RenderPrepareMesh(Camera Cam, M2RenderTicket Tk, bool need_redraw, int draw_id, out MeshDrawer MdOut, ref bool color_one_overwrite)
		{
			MdOut = null;
			if (this.hidden_flag)
			{
				return false;
			}
			if (draw_id == 0 && need_redraw)
			{
				Vector2 vector = this.Anm.Trs.localScale;
				Vector2 vector2 = this.Anm.Trs.localPosition;
				float num = vector.x * this.Mp.base_scale * this.TeScale.x;
				float num2 = vector.y * this.Mp.base_scale;
				float num3 = vector2.x * this.Mp.base_scale + this.Anm.offsetPixelX * 0.015625f;
				float num4 = vector2.y * this.Mp.base_scale + this.Anm.offsetPixelY * 0.015625f;
				Matrix4x4 matrix4x = Matrix4x4.identity;
				if (this.TeScale.y == 1f || !this.poseIs(POSE_TYPE.DOWN, true))
				{
					num2 *= this.TeScale.y;
				}
				else
				{
					matrix4x = Matrix4x4.Translate(new Vector3(0f, -1.09375f, 0f)) * Matrix4x4.Scale(new Vector3(1f, this.TeScale.y, 1f)) * Matrix4x4.Translate(new Vector3(0f, 1.09375f, 0f));
				}
				Tk.Matrix = Matrix4x4.Translate(new Vector3(num3, num4, 0f)) * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, this.rotationR / 3.1415927f * 180f)) * Matrix4x4.Scale(new Vector3(num, num2, 1f)) * matrix4x;
			}
			if (draw_id == 0)
			{
				this.checkFrame(false);
				MdOut = this.reentryMainMesh();
				return true;
			}
			if (draw_id != 1)
			{
				return false;
			}
			MdOut = ((this.CurFrame != null && this.CurFrame.clip_mask_layer != 0 && (this.CurInfo.type & POSE_TYPE.MARUNOMI) == POSE_TYPE.STAND) ? this.MdClip : null);
			return true;
		}

		private bool FnPrepareFrontMd(Camera Cam, M2RenderTicket Tk, bool need_redraw, int draw_id, out MeshDrawer MdOut, ref bool color_one_overwrite)
		{
			if (draw_id < 2)
			{
				MdOut = null;
				return true;
			}
			draw_id -= 2;
			if (draw_id == 0)
			{
				MdOut = this.MdFront;
				return true;
			}
			MdOut = null;
			return false;
		}

		public void clearDownTurning(bool fine_anm = true)
		{
			if (this.pose_down_turning)
			{
				this.pose_down_turning = false;
				if (fine_anm)
				{
					this.Anm.setAim(global::XX.CAim.toPxlAim(global::XX.CAim.get_aim2(0f, 0f, (float)global::XX.CAim._XD(this.Pr.aim, 1), (float)global::XX.CAim._YD(this.Pr.aim, 1), false)), 0);
				}
			}
		}

		public void setAim(int aim, int restart_anim = -1, bool clear_down_turning = false)
		{
			if (clear_down_turning)
			{
				this.pose_down_turning = false;
			}
			this.Anm.setAim(global::XX.CAim.toPxlAim(global::XX.CAim.get_aim2(0f, 0f, (float)(global::XX.CAim._XD(aim, 1) * global::XX.X.MPF(!this.pose_down_turning)), (float)global::XX.CAim._YD(aim, 1), false)), restart_anim);
		}

		public void setAim(global::XX.AIM aim, int restart_anim = -1, bool clear_down_turning = false)
		{
			if (clear_down_turning)
			{
				this.pose_down_turning = false;
			}
			this.Anm.setAim(global::XX.CAim.toPxlAim(global::XX.CAim.get_aim2(0f, 0f, (float)(global::XX.CAim._XD(aim, 1) * global::XX.X.MPF(!this.pose_down_turning)), (float)global::XX.CAim._YD(aim, 1), false)), restart_anim);
		}

		public Vector3 getHipPos()
		{
			bool flag = false;
			return this.getHipPos(ref flag);
		}

		public Vector3 getHeadPos()
		{
			Vector3 vector = this.getHipPos();
			vector.x -= this.Pr.drawx * this.Mp.rCLEN;
			vector.y -= this.Pr.drawy * this.Mp.rCLEN;
			vector *= -1f;
			vector.x += this.Pr.drawx * this.Mp.rCLEN;
			vector.y += this.Pr.drawy * this.Mp.rCLEN;
			vector.z = (float)global::XX.CAim.get_opposite((global::XX.AIM)vector.z);
			return vector;
		}

		public Vector3 getHipPos(ref bool success_point_hip)
		{
			int pose_aim_visible = this.pose_aim_visible;
			float num = (this.poseIs(POSE_TYPE.MANGURI, false) ? global::XX.AIM.T : (this.Pr.isPoseBack(false) ? global::XX.CAim.get_opposite((global::XX.AIM)pose_aim_visible) : ((global::XX.AIM)pose_aim_visible)));
			if (this.CurInfo.PointData != null)
			{
				PxlFrame currentDrawnFrame = this.Anm.getCurrentDrawnFrame();
				for (int i = currentDrawnFrame.countLayers() - 1; i >= 0; i--)
				{
					PxlLayer layer = currentDrawnFrame.getLayer(i);
					if (TX.isStart(layer.name, "point_hip", 0))
					{
						Vector3 vector = this.Anm.Trs.TransformPoint(new Vector3(layer.x * 0.015625f, -layer.y * 0.015625f));
						success_point_hip = true;
						return new Vector3(this.Mp.uxToMapx(this.Mp.M2D.effectScreenx2ux(vector.x)), this.Mp.uyToMapy(this.Mp.M2D.effectScreeny2uy(vector.y)), num);
					}
				}
			}
			success_point_hip = false;
			return new Vector3(this.Pr.drawx * this.Mp.rCLEN + (float)(global::XX.CAim._XD(this.pose_aim, 1) * global::XX.X.MPF(!this.Pr.isPoseBack(false))) * 0.12f, this.Pr.drawy * this.Mp.rCLEN + 0.19f * (float)global::XX.X.MPF(!this.poseIs(POSE_TYPE.MANGURI, false)), num);
		}

		public int get_loop_count()
		{
			return this.Anm.get_loop_count();
		}

		public int countTotalFrame()
		{
			return this.Anm.countTotalFrame();
		}

		public PxlFrame getCurrentDrawnFrame()
		{
			return this.Anm.getCurrentDrawnFrame();
		}

		public PxlPose getCurrentPose()
		{
			return this.Anm.getCurrentPose();
		}

		public int getDuration()
		{
			return this.Anm.getDuration();
		}

		public M2NoelCane getFloatCane()
		{
			if (!(this.Cane != null) || !this.Cane.gameObject.activeSelf)
			{
				return null;
			}
			return this.Cane;
		}

		public float rotationR
		{
			get
			{
				if (!(this.Anm != null))
				{
					return 0f;
				}
				return this.Anm.rotationR;
			}
			set
			{
				if (this.Anm == null || value == this.Anm.rotationR)
				{
					return;
				}
				this.Anm.rotationR = value;
				this.fine_rot = true;
			}
		}

		public PxlSequence getCurrentSequence()
		{
			return this.Anm.getCurrentSequence();
		}

		public bool poseIs(string _a, bool strict)
		{
			if (strict)
			{
				return this.strictPoseIs(_a);
			}
			return this.poseIs(_a);
		}

		public bool poseIs(string _a, string _b)
		{
			return this.poseIs(_a) || this.poseIs(_b);
		}

		public bool poseIs(string _a, string _b, string _c)
		{
			return this.poseIs(_a) || this.poseIs(_b) || this.poseIs(_c);
		}

		public bool poseIs(string _a, string _b, string _c, string _a2, string _b2, string _c2 = null, string _f = null)
		{
			return this.poseIs(_a) || this.poseIs(_b) || this.poseIs(_c) || this.poseIs(_a2) || this.poseIs(_b2) || this.poseIs(_c2) || this.poseIs(_f);
		}

		public bool strictPoseIs(string _a, string _b, string _c, string _a2, string _b2, string _c2 = null, string _f = null)
		{
			return this.strictPoseIs(_a) || this.strictPoseIs(_b) || this.strictPoseIs(_c) || this.strictPoseIs(_a2) || this.strictPoseIs(_b2) || this.strictPoseIs(_c2) || this.strictPoseIs(_f);
		}

		public bool poseIs(string _s)
		{
			if (_s == null)
			{
				return false;
			}
			PxlPose currentPose = this.getCurrentPose();
			if (currentPose == null)
			{
				return false;
			}
			if (this.pose_title0 == _s || currentPose.title == _s)
			{
				return true;
			}
			PxlPose poseByName = this.Anm.getCurrentCharacter().getPoseByName(_s);
			return poseByName != null && (poseByName.title == currentPose.title || poseByName.title == this.pose_title0);
		}

		public bool strictPoseIs(string p)
		{
			if (this.FrozenF != null)
			{
				return this.FrozenF.pPose.title == p;
			}
			return this.Anm.pose_title == p;
		}

		public bool nextPoseIs(string _a, string _b, string _c = null)
		{
			return this.nextPoseIs(_a) || this.nextPoseIs(_b) || this.nextPoseIs(_c);
		}

		public bool nextPoseIs(string _s)
		{
			return this.CurInfo != null && _s != null && this.CurInfo.end_jump_title == _s;
		}

		public bool poseIs(POSE_TYPE pose_type, bool strict = false)
		{
			if (strict && this.FrozenF != null)
			{
				POSE_TYPE type = this.FrozenInfo.type;
				if (pose_type != POSE_TYPE.STAND)
				{
					return (type & pose_type) > POSE_TYPE.STAND;
				}
				return type == POSE_TYPE.STAND;
			}
			else
			{
				if (pose_type != POSE_TYPE.STAND)
				{
					return (this.cur_type & pose_type) > POSE_TYPE.STAND;
				}
				return this.cur_type == POSE_TYPE.STAND;
			}
		}

		public void animReset(int f, float spd)
		{
			this.Anm.animReset(f);
		}

		public Vector2 getScaleTe()
		{
			return this.TeScale;
		}

		public void setScaleTe(Vector2 V)
		{
			this.TeScale = V;
			this.need_fine_mesh = true;
		}

		public float mv_anmx
		{
			get
			{
				return this.Pr.x + this.Pr.getSpShiftX() * this.Pr.Mp.rCLEN;
			}
		}

		public float mv_anmy
		{
			get
			{
				return this.Pr.y - this.Pr.getSpShiftY() * this.Pr.Mp.rCLEN * 1f;
			}
		}

		public float animator_TS
		{
			get
			{
				M2Phys physic = this.Pr.getPhysic();
				if (physic.isin_water && this.CurInfo != null && (this.CurInfo.type & POSE_TYPE.NO_SLOW_IN_WATER) == POSE_TYPE.STAND)
				{
					return (physic.water_speed_scale - 1f) * 0.4f + 1f;
				}
				return 1f;
			}
		}

		public bool cur_pose_flipped
		{
			get
			{
				return this.Anm.getCurrentPose().isFlipped((int)this.Anm.pose_aim);
			}
		}

		public float counter_shift_map_x
		{
			get
			{
				return (float)(this.CurInfo.counter_shift_x * global::XX.X.MPF(!this.cur_pose_flipped)) * this.Pr.Mp.rCLENB;
			}
		}

		public float counter_expand_x
		{
			get
			{
				return (float)(this.CurInfo.counter_expand_x * 2) * 0.01f;
			}
		}

		public float counter_shift_map_y
		{
			get
			{
				return (float)(-(float)this.CurInfo.counter_shift_y) * this.Pr.Mp.rCLENB;
			}
		}

		public int orgasm_frame_index
		{
			get
			{
				if (this.CurInfo == null || this.CurInfo.orgasm_frame_index == 255)
				{
					return -1;
				}
				return (int)this.CurInfo.orgasm_frame_index;
			}
		}

		public Vector2 getTargetRodPos()
		{
			if (this.Cane.gameObject.activeSelf)
			{
				return this.Cane.getSphereMapPos();
			}
			Vector2 vector = new Vector2(this.mv_anmx, this.mv_anmy);
			if (this.poseIs("magic_bomb_hold"))
			{
				MagicItem curMagic = this.Pr.getCurMagic();
				if (curMagic != null && curMagic.Mn != null)
				{
					vector.y += curMagic.Mn._0.y;
				}
				else
				{
					vector.y += this.Pr.sizey;
				}
				return vector;
			}
			if (this.CurFrame != null)
			{
				vector.x += this.CurFrame.target_x;
				vector.y += this.CurFrame.target_y;
			}
			return vector;
		}

		public bool isAnimEnd()
		{
			return this.Anm.isAnimEnd();
		}

		public void randomizeFrame()
		{
			PxlSequence currentSequence = this.Anm.getCurrentSequence();
			int num = currentSequence.countFrames();
			int num2 = this.cframe + num / 2 + global::XX.X.xors(global::XX.X.IntC((float)num * 0.5f));
			num2 = ((num2 < currentSequence.loop_to) ? num2 : ((num2 - currentSequence.loop_to) % (num - currentSequence.loop_to) + currentSequence.loop_to));
			this.animReset(num2);
		}

		public Vector2 getMapPosForLayer(PxlLayer L)
		{
			return this.Anm.getMapPosForLayer(L);
		}

		public void animReset(int set_frame = 0)
		{
			this.Anm.animReset(set_frame);
		}

		public bool isTortured()
		{
			return this.Anm.PointData != null;
		}

		public PxlLayer[] SpGetPointsData(ref M2PxlAnimator MyAnimator, ref ITeScaler Scl)
		{
			MyAnimator = this.Anm;
			Scl = this;
			if (this.Anm.PointData == null)
			{
				return null;
			}
			return this.Anm.PointData.GetPoints(this.Anm.getCurrentDrawnFrame(), false);
		}

		public float body_agR
		{
			get
			{
				int pose_aim_visible = this.pose_aim_visible;
				if (this.CurInfo.body_agR != -1000f)
				{
					if (global::XX.CAim._XD(pose_aim_visible, 1) >= 0)
					{
						return 3.1415927f - this.CurInfo.body_agR;
					}
					return this.CurInfo.body_agR;
				}
				else
				{
					float num;
					if (this.poseIs(POSE_TYPE.DOWN, false))
					{
						num = 2.9670596f;
					}
					else
					{
						if (!this.poseIs(POSE_TYPE.CROUCH, false))
						{
							return 1.5707964f;
						}
						num = 2.0071287f;
					}
					if (global::XX.CAim._XD(pose_aim_visible, 1) >= 0)
					{
						return 3.1415927f - num;
					}
					return num;
				}
			}
		}

		public float mpf_is_right
		{
			get
			{
				return (float)global::XX.CAim._XD((int)this.Anm.pose_aim, 1);
			}
		}

		public float mpf_is_right_visible
		{
			get
			{
				return (float)global::XX.CAim._XD(this.pose_aim_visible, 1);
			}
		}

		public string pose_title
		{
			get
			{
				return this.pre_pose_title;
			}
		}

		public bool pose_is_stand
		{
			get
			{
				return this.SfPose.pose_is_stand;
			}
		}

		public bool next_pose_is_stand
		{
			get
			{
				return this.Anm.getCurrentPose().end_jump_title == "stand";
			}
		}

		public bool looped_already
		{
			get
			{
				return this.Anm.looped_already;
			}
		}

		public int cframe
		{
			get
			{
				return this.Anm.cframe;
			}
		}

		public int cframe_strict
		{
			get
			{
				if (this.FrozenF != null)
				{
					return this.FrozenF.index;
				}
				return this.Anm.cframe;
			}
		}

		public float timescale
		{
			get
			{
				return this.Anm.timescale;
			}
			set
			{
				this.Anm.timescale = value;
			}
		}

		public PrPoseContainer.PoseInfo getPoseInfo(string s)
		{
			return this.PCon.Get(s);
		}

		public M2PxlAnimator getAnimator()
		{
			return this.Anm;
		}

		public bool cane_broken_active
		{
			get
			{
				return this.CaneB != null && this.CaneB.gameObject.activeSelf;
			}
		}

		public bool cane_broken
		{
			get
			{
				return this.CaneB != null;
			}
		}

		public int pose_aim
		{
			get
			{
				return (int)this.Anm.pose_aim;
			}
		}

		public int pose_aim_visible
		{
			get
			{
				if (this.FrozenF != null)
				{
					return this.FrozenF.pSq.aim;
				}
				return this.pose_aim;
			}
		}

		public string getManualSettedPoseTitle()
		{
			return this.pose_title0;
		}

		public bool need_fine
		{
			get
			{
				return this.Anm.need_fine;
			}
			set
			{
				this.Anm.need_fine = value;
			}
		}

		public PRNoel.OUTFIT outfit_type
		{
			get
			{
				return this.Pr.outfit_type;
			}
		}

		public Map2d Mp
		{
			get
			{
				return this.Pr.Mp;
			}
		}

		public readonly PrPoseContainer PCon;

		private readonly PRNoel Pr;

		private readonly NelM2DBase M2D;

		public bool hidden_flag;

		private PrPoseContainer.NFrame CurFrame;

		private PrPoseContainer.NFrame PFrame;

		private PrPoseContainer.NFrame PPFrame;

		private M2PxlAnimatorRT Anm;

		private POSE_TYPE cur_type;

		private M2NoelCane Cane;

		private M2NoelCane CaneB;

		private M2NoelHat Hat;

		private bool hat_assign_recheck;

		private bool pre_rod_erase;

		private bool pose_down_turning;

		private List<PxlPose> ARodErasePose;

		private ElecTraceDrawer HoldMagicElec;

		private M2DrawBinder HoldMagicEd;

		private PxlFrame FrozenF;

		private PrPoseContainer.PoseInfo FrozenInfo;

		private MeshDrawer MdFront;

		private M2RenderTicket RtkFront;

		private Material MtrBase;

		private Material MtrFrozen;

		private Material MtrStone;

		private byte frozen_lv_;

		private byte stone_lv_;

		public NoelAnimator.FRZ_STATE frz_state;

		public bool need_fine_frozen_appear;

		private string pose_title0 = "stand";

		private string pre_pose_title = "stand";

		public const string stand_pose = "stand";

		public const string crouch_pose = "crouch";

		private const string LAY_HEADER_MARUNOMI = "marunomi";

		private const string LAY_HEADER_TOP_LAYER = "top_layer";

		public Vector2 TeScale = Vector2.one;

		private bool fine_rot;

		private PrPoseContainer.PoseInfo CurInfo;

		private PxlPose PreTargetPose;

		private AnimationShuffler SfPose;

		private bool need_fine_mesh = true;

		private Material MtrAlphaClipMask;

		private MeshDrawer MdClip;

		private M2DrawBinder.FnEffectBind FD_drawEffectMagicElec;

		private Func<Vector2> FD_ElecGetPos;

		private M2RenderTicket.FnPrepareMd FD_RenderPrepareMesh;

		public Flagger FlgDropCane;

		public bool torture_by_invisible_enemy;

		public const float downpose_center_shift_y = 70f;

		private M2RenderTicket.FnPrepareMd FD_FnPrepareFrontMd;

		public enum FRZ_STATE
		{
			NORMAL,
			FROZEN,
			WEB_TRAPPED,
			STONE = 4,
			STONEOVER = 8,
			_MAX
		}
	}
}
