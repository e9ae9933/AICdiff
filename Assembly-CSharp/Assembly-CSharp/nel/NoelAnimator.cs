using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner;
using PixelLiner.PixelLinerLib;
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

		public static bool initNoelAnimator(string[][] Anoel_pxls, float CLENB)
		{
			if (NoelAnimator.OPoseType != null)
			{
				return true;
			}
			float num = 1f / CLENB;
			int num2 = Anoel_pxls.Length;
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				int num4 = Anoel_pxls[i].Length;
				for (int j = 0; j < num4; j++)
				{
					PxlCharacter pxlCharacter = PxlsLoader.getPxlCharacter(Anoel_pxls[i][j]);
					if (!pxlCharacter.isLoadCompleted())
					{
						return false;
					}
					num3 += pxlCharacter.countPoses();
				}
			}
			NoelAnimator.MtrAlphaClipMask = MTRX.newMtr(MTRX.ShaderAlphaSplice);
			NoelAnimator.MdClip = new MeshDrawer(null, 4, 6);
			NoelAnimator.MdClip.draw_gl_only = true;
			NoelAnimator.MdClip.activate("", NoelAnimator.MtrAlphaClipMask, false, MTRX.ColWhite, null);
			NoelAnimator.OPoseType = new BDic<string, NoelAnimator.PoseInfo>(num3);
			List<NoelAnimator.PoseInfo> list = new List<NoelAnimator.PoseInfo>();
			CsvReader csvReader = new CsvReader(null, CsvReader.RegSpace, false);
			int num5 = 0;
			int num6 = 0;
			for (int k = 0; k < num2; k++)
			{
				int num7 = Anoel_pxls[k].Length;
				for (int l = 0; l < num7; l++)
				{
					PxlCharacter pxlCharacter2 = PxlsLoader.getPxlCharacter(Anoel_pxls[k][l]);
					if (l == 0 && k == 0)
					{
						NoelAnimator.MainPxl = pxlCharacter2;
						NoelAnimator.RodPose = pxlCharacter2.getPoseByName("RODRODRODROD");
						PxlFrame frameByName = NoelAnimator.RodPose.getSequence(0).getFrameByName("hat");
						if (frameByName != null)
						{
							NoelAnimator.AHatImg = new PxlImage[]
							{
								frameByName.getLayer(0).Img,
								frameByName.getLayer(1).Img
							};
						}
					}
					int num8 = pxlCharacter2.countPoses();
					for (int m = 0; m < num8; m++)
					{
						PxlPose pose = pxlCharacter2.getPose(m);
						if (!(pose.title == "RODRODRODROD"))
						{
							NoelAnimator.PoseInfo poseInfo = null;
							if (k == 0)
							{
								if (NoelAnimator.OPoseType.TryGetValue(pose.title, out poseInfo))
								{
									if (pose.title != "stand")
									{
										global::XX.X.dl("ポーズ名が重複している: " + pose.ToString(), null, false, false);
										goto IL_07EA;
									}
									goto IL_07EA;
								}
								else
								{
									poseInfo = (NoelAnimator.OPoseType[pose.title] = new NoelAnimator.PoseInfo(pose));
								}
							}
							else
							{
								if (!NoelAnimator.OPoseType.TryGetValue(pose.title, out poseInfo))
								{
									NoelAnimator.OUTFIT outfit = (NoelAnimator.OUTFIT)k;
									global::XX.X.dl(outfit.ToString() + " ポーズの指定先が存在しない: " + pose.ToString(), null, false, false);
									goto IL_07EA;
								}
								if (pose.title == "stand" && l != 0)
								{
									goto IL_07EA;
								}
								poseInfo.addPose(k, pose);
								num5++;
							}
							string title = pose.title;
							if (k == 0)
							{
								csvReader.parseText(pose.comment.ToLower());
								while (csvReader.read())
								{
									string cmd = csvReader.cmd;
									if (cmd != null)
									{
										uint num9 = <PrivateImplementationDetails>.ComputeStringHash(cmd);
										if (num9 <= 2273894358U)
										{
											if (num9 <= 1035581717U)
											{
												if (num9 <= 329414684U)
												{
													if (num9 != 72357410U)
													{
														if (num9 == 329414684U)
														{
															if (cmd == "press_damage")
															{
																poseInfo.type |= POSE_TYPE.PRESS_DAMAGE;
															}
														}
													}
													else if (cmd == "manguri")
													{
														poseInfo.type |= POSE_TYPE.MANGURI;
													}
												}
												else if (num9 != 502659160U)
												{
													if (num9 != 811740619U)
													{
														if (num9 == 1035581717U)
														{
															if (cmd == "down")
															{
																poseInfo.type |= POSE_TYPE.DOWN;
															}
														}
													}
													else if (cmd == "crouchb")
													{
														poseInfo.type |= POSE_TYPE.CROUCH | POSE_TYPE.BACK;
													}
												}
												else if (cmd == "damage_reset")
												{
													poseInfo.type |= POSE_TYPE.DAMAGE_RESET;
												}
											}
											else if (num9 <= 1313842571U)
											{
												if (num9 != 1203378667U)
												{
													if (num9 == 1313842571U)
													{
														if (cmd == "crouch")
														{
															poseInfo.type |= POSE_TYPE.CROUCH;
														}
													}
												}
												else if (cmd == "onground")
												{
													poseInfo.type |= POSE_TYPE.ONGROUND;
												}
											}
											else if (num9 != 1436210240U)
											{
												if (num9 != 1628932526U)
												{
													if (num9 == 2273894358U)
													{
														if (cmd == "no_fall_cane")
														{
															poseInfo.type |= POSE_TYPE.NO_FALL_CANE;
														}
													}
												}
												else if (cmd == "use_top_layer")
												{
													poseInfo.type |= POSE_TYPE.USE_TOP_LAYER;
												}
											}
											else if (cmd == "no_slow_in_water")
											{
												poseInfo.type |= POSE_TYPE.NO_SLOW_IN_WATER;
											}
										}
										else if (num9 <= 2770559742U)
										{
											if (num9 <= 2721425853U)
											{
												if (num9 != 2341128772U)
												{
													if (num9 == 2721425853U)
													{
														if (cmd == "orgasm_frame_index")
														{
															poseInfo.orgasm_frame_index = (byte)csvReader.Int(1, 255);
														}
													}
												}
												else if (cmd == "orgasm")
												{
													poseInfo.type |= POSE_TYPE.ORGASM;
												}
											}
											else if (num9 != 2724132437U)
											{
												if (num9 != 2730307407U)
												{
													if (num9 == 2770559742U)
													{
														if (cmd == "absorb_default")
														{
															poseInfo.type |= POSE_TYPE.ABSORB_DEFAULT;
														}
													}
												}
												else if (cmd == "sensitive")
												{
													if (!TX.isEnd(pose.title, "_sensitive"))
													{
														poseInfo.type |= POSE_TYPE.SENSITIVE;
														list.Add(poseInfo);
													}
												}
											}
											else if (cmd == "downb")
											{
												poseInfo.type |= POSE_TYPE.DOWN | POSE_TYPE.BACK;
											}
										}
										else if (num9 <= 2872309231U)
										{
											if (num9 != 2805947405U)
											{
												if (num9 != 2855531612U)
												{
													if (num9 == 2872309231U)
													{
														if (cmd == "counter_shift_x")
														{
															poseInfo.counter_shift_x = csvReader.Int(1, 0);
														}
													}
												}
												else if (cmd == "counter_shift_y")
												{
													poseInfo.counter_shift_y = csvReader.Int(1, 0);
												}
											}
											else if (cmd == "jump")
											{
												poseInfo.type |= POSE_TYPE.JUMP;
											}
										}
										else if (num9 != 2904147653U)
										{
											if (num9 != 2980534815U)
											{
												if (num9 == 4166649696U)
												{
													if (cmd == "fall")
													{
														poseInfo.type |= POSE_TYPE.FALL;
													}
												}
											}
											else if (cmd == "marunomi")
											{
												poseInfo.type |= POSE_TYPE.MARUNOMI;
											}
										}
										else if (cmd == "body_angle")
										{
											poseInfo.body_agR = global::XX.X.GAR(0f, 0f, csvReader.Nm(1, 0f), csvReader.Nm(2, 0f));
										}
									}
								}
							}
							num6 += NoelAnimator.countImageForDebug(pose);
						}
						IL_07EA:;
					}
				}
			}
			global::XX.X.dl(string.Concat(new string[]
			{
				"Noel ポーズ数: ",
				NoelAnimator.OPoseType.Count.ToString(),
				" / 服ビリポーズ数:",
				num5.ToString(),
				"/ イメージ数: ",
				num6.ToString()
			}), null, false, false);
			if (list.Count != 0)
			{
				int count = list.Count;
				for (int n = 0; n < count; n++)
				{
					NoelAnimator.PoseInfo poseInfo2 = list[n];
					string text = poseInfo2.title + "_sensitive";
					NoelAnimator.PoseInfo poseInfo3;
					if (NoelAnimator.OPoseType.TryGetValue(text, out poseInfo3))
					{
						poseInfo2.PI_S = poseInfo3.copyBasicDataFrom(poseInfo2);
						NoelAnimator.OPoseType.Remove(text);
					}
					else
					{
						global::XX.X.de("sensitive ポーズ " + text + " が見つかりません", null);
					}
				}
			}
			return true;
		}

		public static void initPoseInfo(NoelAnimator.PoseInfo PI, float rCLENB)
		{
			if (PI.initted)
			{
				return;
			}
			PI.initted = true;
			PxlPose pxlPose = PI.APose[0];
			int num = 0;
			while ((long)num < 8L)
			{
				if (pxlPose.isValidAim(num))
				{
					PxlSequence sequence = pxlPose.getSequence(num);
					int num2 = sequence.countFrames();
					bool flag = false;
					for (int i = 0; i < num2; i++)
					{
						PxlFrame frame = sequence.getFrame(i);
						NoelAnimator.NFrame nframe;
						if (!PI.OFrmData.TryGetValue(frame, out nframe))
						{
							nframe = (PI.OFrmData[frame] = new NoelAnimator.NFrame(frame, rCLENB));
						}
						if (!flag)
						{
							flag = nframe.hatlost;
						}
						else
						{
							nframe.hatlost = true;
						}
					}
				}
				num++;
			}
			if (TX.isStart(pxlPose.title, "torture", 0))
			{
				PI.PointData = new M2PxlPointContainer.PosePointsData();
				bool flag2 = (PI.type & POSE_TYPE.MARUNOMI) > POSE_TYPE.STAND;
				for (int j = 0; j < PI.APose.Length; j++)
				{
					pxlPose = PI.APose[j];
					int num3 = 0;
					while ((long)num3 < 8L)
					{
						if (pxlPose.isValidAim(num3))
						{
							PxlSequence sequence2 = pxlPose.getSequence(num3);
							for (int k = sequence2.countFrames() - 1; k >= 0; k--)
							{
								PxlFrame frame2 = sequence2.getFrame(k);
								int num4 = -1;
								bool flag3 = false;
								NoelAnimator.NFrame nframe2;
								if (!PI.OFrmData.TryGetValue(frame2, out nframe2))
								{
									nframe2 = (PI.OFrmData[frame2] = new NoelAnimator.NFrame(frame2, rCLENB));
								}
								int num5 = frame2.countLayers();
								for (int l = 0; l < num5; l++)
								{
									PxlLayer layer = frame2.getLayer(l);
									if (TX.isStart(layer.name, "mask", 0))
									{
										nframe2.clip_mask_layer |= 1 << l;
										layer.alpha = 0f;
										frame2.releaseDrawnMesh();
									}
									else if (TX.isStart(layer.name, "point_", 0) || layer.alpha == 0f)
									{
										layer.alpha = 0f;
										frame2.releaseDrawnMesh();
									}
									else
									{
										if (TX.isStart(layer.name, "enemy", 0))
										{
											layer.alpha = 0f;
											num4 = l;
											if (flag3)
											{
												nframe2.clip_mask_layer |= 1 << num4;
											}
										}
										else
										{
											flag3 = true;
											if (num4 >= 0)
											{
												PI.use_front_drawing = true;
												nframe2.clip_mask_layer |= 1 << num4;
											}
										}
										if (flag2 && nframe2.clip_mask_layer > 0)
										{
											layer.alpha = -global::XX.X.Abs(layer.alpha);
										}
									}
								}
							}
						}
						num3++;
					}
				}
				return;
			}
			if ((PI.type & POSE_TYPE.USE_TOP_LAYER) != POSE_TYPE.STAND)
			{
				PI.use_front_drawing = true;
			}
		}

		public static NoelAnimator.PoseInfo getPoseInfo(string s)
		{
			return global::XX.X.Get<string, NoelAnimator.PoseInfo>(NoelAnimator.OPoseType, s);
		}

		public NoelAnimator(PRNoel _Pr, M2PxlAnimatorRT _Anm)
		{
			this.Pr = _Pr;
			this.M2D = this.Pr.M2D as NelM2DBase;
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
			this.pose_is_stand_t = 0f;
			this.MtrFrozen = MTRX.newMtr(MTR.MtrFrozen);
			this.MtrFrozen.SetFloat("_ZTest", 8f);
			this.MtrFrozen.SetColor("_Color", C32.d2c(4288008150U));
			this.MtrFrozen.SetColor("_BColor", C32.d2c(4279786863U));
			this.MtrFrozen.SetColor("_WColor", C32.d2c(4292867578U));
			this.initS(_Anm);
			PxlFrame frame = NoelAnimator.RodPose.getSequence(0).getFrame(0);
			this.Cane.initCane(this, frame.getLayerByName("rod_base").Img, 1f, true);
		}

		private void initHat()
		{
			this.Hat = this.Pr.Mp.createMover<M2NoelHat>("NoelHat", this.Pr.x, this.Pr.y, false, false);
			this.Hat.do_not_destruct_when_remove = true;
			this.Hat.initImage(NoelAnimator.AHatImg);
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
			this.pose_down_turning = false;
			this.hat_assign_recheck = true;
			this.FlgDropCane.Clear();
			if (this.CurInfo == null)
			{
				this.CurInfo = NoelAnimator.OPoseType["stand"];
				this.Anm.setPose(this.pose_title, -1);
			}
			else
			{
				this.Anm.setPose(this.PreTargetPose, -1);
			}
			this.need_fine_mesh = true;
			this.checkFrame(true);
			if (this.pose_is_stand_t > 0f)
			{
				this.pose_is_stand_t = 1f;
			}
			if (this.Cane.gameObject.activeSelf)
			{
				this.Cane.gameObject.SetActive(false);
			}
			this.FrontDrawEd = null;
			this.HoldMagicEd = null;
			this.fineMaterialColor();
			this.Pr.TeCon.clearRegistered();
			this.Pr.TeCon.RegisterCol(this, false);
			this.Pr.TeCon.RegisterPos(this.Anm);
			this.fineFrozenAppearance();
			return this;
		}

		public void runPost(float f)
		{
			NoelAnimator.prepareHoldElecAndEd(this.Mp, this.Pr.getSkillManager().magic_chant_completed, ref this.HoldMagicElec, ref this.HoldMagicEd, this.FD_drawEffectMagicElec, true);
			if (this.pose_is_stand_t > 0f)
			{
				this.pose_is_stand_t = (Map2d.can_handle ? (this.pose_is_stand_t + f) : global::XX.X.Mn(this.pose_is_stand_t, 3000f));
				this.Anm.setPose(this.getWaitingPoseTitle(), -1);
			}
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
				this.setPose(pxlPose.end_jump_title, 1, true);
				this.Anm.runPre(0f);
				pxlPose = this.Anm.getCurrentPose();
			}
			PxlFrame pxlFrame;
			if (this.FrozenF != null && !this.Pr.frozenAnimReplaceable())
			{
				pxlFrame = this.FrozenF;
			}
			else
			{
				pxlFrame = this.Anm.getCurrentSequence().getFrame(this.cframe);
				if (this.FrozenF != null)
				{
					this.FrozenF = pxlFrame;
					this.FrozenInfo = this.CurInfo;
					this.Pr.need_check_bounds = true;
				}
			}
			NoelAnimator.NFrame nframe;
			if (!this.CurInfo.OFrmData.TryGetValue(pxlFrame, out nframe))
			{
				nframe = (this.CurInfo.OFrmData[pxlFrame] = new NoelAnimator.NFrame(pxlFrame, this.Anm.Mp.rCLENB));
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
				NoelAnimator.MdClip.clearSimple();
				for (int i = 0; i < num; i++)
				{
					if ((num2 & (1 << i)) != 0)
					{
						num2 &= ~(1 << i);
						PxlLayer layer = pxlFrame.getLayer(i);
						NoelAnimator.MdClip.RotaL(0f, 0f, layer, false, false, 0);
						if (num2 == 0)
						{
							break;
						}
					}
				}
				NoelAnimator.MdClip.updateForMeshRenderer(false);
			}
			this.PPFrame = this.PFrame;
			this.PFrame = this.CurFrame;
			this.CurFrame = nframe;
			if (this.Mp.floort >= 4f)
			{
				if (this.PFrame != null && this.PPFrame != null)
				{
					bool flag = ((this.ARodErasePose != null) ? null : this.CurFrame.RodL) != null || this.poseIs(POSE_TYPE.NO_FALL_CANE, false) || this.outfit_type > NoelAnimator.OUTFIT.NORMAL;
					if (!flag && this.PFrame.RodL != null && !this.Cane.gameObject.activeSelf)
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
					else if (flag && this.Cane.gameObject.activeSelf)
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
						bool flag2 = true;
						if (this.cframe >= 2)
						{
							PxlFrame frame = pxlFrame.pSq.getFrame(this.cframe - 1);
							PxlFrame frame2 = pxlFrame.pSq.getFrame(this.cframe - 2);
							PxlLayer layerByName = frame.getLayerByName("hat");
							PxlLayer layerByName2 = frame2.getLayerByName("hat");
							if (layerByName != null && layerByName2 != null)
							{
								flag2 = false;
								float num7 = global::XX.X.NI(layerByName2.x, layerByName.x, 1.5f);
								float num8 = global::XX.X.NI(layerByName2.y, layerByName.y, 1.2f);
								float num9 = global::XX.X.NI(layerByName2.rotR, layerByName.rotR, 1.5f);
								this.Hat.initObject(this, layerByName.Img, 1f);
								this.appearDropObject(this.Hat, num7 * this.Mp.rCLENB, num8 * this.Mp.rCLENB, (num7 - layerByName.x) * 0.03f * this.Mp.rCLENB, (num8 - layerByName.y) * 0.03f * this.Mp.rCLENB, -num9, -(num9 - layerByName.rotR) * 0.012f);
							}
						}
						if (flag2)
						{
							Vector3 headPos = this.getHeadPos();
							this.Hat.initObject(this, NoelAnimator.AHatImg[0], 1f);
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

		public void fineFreezeFrame()
		{
			if (this.Pr.isFrozen())
			{
				this.FrozenF = this.Anm.getCurrentSequence().getFrame(this.cframe);
				this.FrozenInfo = this.CurInfo;
				return;
			}
			this.FrozenF = null;
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
				this.fineFrozenAppearance();
			}
		}

		public void fineFrozenAppearance()
		{
			Material material;
			if (this.frozen_lv > 0)
			{
				material = this.MtrFrozen;
				Texture currentTexture = this.Anm.getCurrentTexture();
				if (currentTexture != null)
				{
					float num = (float)currentTexture.width / 512f * 4f;
					float num2 = (float)currentTexture.height / 512f * 4f;
					material.SetFloat("_ScaleX", num);
					material.SetFloat("_ScaleY", num2);
				}
				material.SetFloat("_Level", 0.3f + (float)this.frozen_lv * 0.17f);
			}
			else
			{
				material = this.MtrBase;
			}
			this.Anm.setRendererMaterial(material);
		}

		public PxlFrame fnReplaceCurrentFrame(PxlSequence pSq, int i)
		{
			if (this.FrozenF != null)
			{
				return this.FrozenF;
			}
			if (pSq == null)
			{
				return null;
			}
			return pSq.getFrame(i);
		}

		public string getWaitingPoseTitle()
		{
			return this.getWaitingPoseTitle(this.pose_title0);
		}

		public string getWaitingPoseTitle(string pose0)
		{
			if (this.pose_is_stand_t < 0f)
			{
				return this.Anm.getCurrentPose().title;
			}
			if (pose0 == "ladder" || pose0 == "ladder_wait")
			{
				if (this.pose_is_stand_t >= 1400f)
				{
					this.pose_is_stand_t = 1f;
				}
				if (this.pose_is_stand_t >= 400f)
				{
					return this.Anm.pose_title;
				}
				if (this.pose_is_stand_t >= 360f)
				{
					return "ladder_wait";
				}
				return "ladder";
			}
			else
			{
				if (this.pose_is_stand_t < 180f)
				{
					return "stand";
				}
				if (360f <= this.pose_is_stand_t && this.pose_is_stand_t < 2000f && this.Anm.looped_in_preveious_frame)
				{
					this.pose_is_stand_t = 2000f;
					return "stand_slow";
				}
				if (2000f > this.pose_is_stand_t)
				{
					return "stand";
				}
				if (this.pose_is_stand_t < 2220f)
				{
					return "stand_slow";
				}
				if (this.pose_is_stand_t < 2500f)
				{
					this.pose_is_stand_t = 3000f;
				}
				if (this.pose_is_stand_t < 3520f)
				{
					return "stand_ev";
				}
				if (this.Pr.BetoMng.is_torned || this.pose_aim == 7 || this.pose_aim == 6 || this.outfit_type != NoelAnimator.OUTFIT.NORMAL)
				{
					this.pose_is_stand_t = 3000f;
					return "stand_ev";
				}
				if (this.pose_is_stand_t < 3610f)
				{
					return "stand_wait_normal_0";
				}
				if (this.pose_is_stand_t < 3660f)
				{
					return "stand_wait_normal_1";
				}
				this.pose_is_stand_t = 2600f;
				return "stand_ev";
			}
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
				Object gameObject = this.CaneB.gameObject;
				IN.DestroyOne(this.CaneB);
				IN.DestroyOne(gameObject);
				this.CaneB = null;
			}
			PxlFrame frame = NoelAnimator.RodPose.getSequence(0).getFrame(0);
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
				PxlFrame frame = NoelAnimator.RodPose.getSequence(0).getFrame(0);
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

		private bool fnDrawFrontMesh(EffectItem E, M2DrawBinder Ed)
		{
			if (this.CurInfo == null)
			{
				return true;
			}
			MeshDrawer meshDrawer = null;
			PxlFrame currentDrawnFrame = this.Anm.getCurrentDrawnFrame();
			int num = currentDrawnFrame.countLayers();
			bool flag = false;
			bool flag2 = (this.CurInfo.type & POSE_TYPE.MARUNOMI) > POSE_TYPE.STAND;
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
					float num2 = (float)global::XX.X.MPF(!flag2) * layer.alpha;
					if (flag && num2 > 0f)
					{
						float num3 = 1f;
						if (flag2 && CFG.sp_opacity_marunomi < 100 && TX.isStart(layer.name, "marunomi", 0))
						{
							num3 *= (float)CFG.sp_opacity_marunomi * 0.01f;
							if (num3 <= 0f)
							{
								goto IL_022F;
							}
						}
						if (meshDrawer == null)
						{
							meshDrawer = E.GetMesh("", this.Anm.getRendererMaterial(), false);
							meshDrawer.allocUv23(16, false);
							Vector3 vector = this.Anm.Trs.localPosition;
							meshDrawer.base_x = vector.x * this.Mp.base_scale;
							meshDrawer.base_y = vector.y * this.Mp.base_scale;
							vector = this.Anm.Trs.localScale;
							meshDrawer.Scale(vector.x * 2f, vector.y * 2f, false);
							meshDrawer.Rotate(this.Anm.Trs.localEulerAngles.z / 180f * 3.1415927f, false);
						}
						meshDrawer.initForImg(layer.Img, 0);
						meshDrawer.ColGrd.White();
						if (TX.isStart(layer.name, "Layer", 0))
						{
							meshDrawer.ColGrd.Set(this.Anm.color);
						}
						meshDrawer.Col = meshDrawer.ColGrd.mulA(num2 * num3 * 0.01f).C;
						meshDrawer.RotaL(0f, 0f, layer, true, true, 0);
					}
				}
				IL_022F:;
			}
			if (meshDrawer != null)
			{
				meshDrawer.Uv23(this.Anm.CAdd, false);
				meshDrawer.allocUv23(0, true);
				meshDrawer.Identity();
			}
			return true;
		}

		public void finePose(int restart_anim = 0)
		{
			this.setPose(this.pose_title0, restart_anim, false);
		}

		public void clearStandPoseTime()
		{
			this.pose_is_stand_t = global::XX.X.Mn(0f, this.pose_is_stand_t);
		}

		public void setPose(string title, int restart_anim = -1, bool loop_jumping = false)
		{
			string text = title;
			string title2 = this.Anm.getCurrentPose().title;
			if (title == "stand")
			{
				if (this.Pr.isBreatheStop(false, false))
				{
					title = (this.Pr.isBreatheStop(true, false) ? "dmg_breathe" : "stand_hardbreathe");
					this.pose_is_stand_t = -1f;
				}
				else if (this.Pr.applying_wind)
				{
					title = "stand_wind";
					this.pose_is_stand_t = -1f;
				}
				else if (this.Pr.BetoMng.is_torned && !global::XX.X.SENSITIVE)
				{
					title = "stand";
					this.pose_is_stand_t = -1f;
				}
				else if (this.isWetPose() || this.isWeakPose())
				{
					title = "stand_wet_weak";
					this.pose_is_stand_t = -1f;
				}
				else
				{
					if (this.pose_is_stand_t > 0f && restart_anim > -2 && restart_anim != 1)
					{
						return;
					}
					this.pose_is_stand_t = 1f;
					string title3 = this.Anm.getCurrentPose().title;
					if (!loop_jumping && (this.Pr.isPoseCrouch(false) || title3 == "crouch2stand"))
					{
						title = "crouch2stand";
					}
					else
					{
						title = this.getWaitingPoseTitle("stand");
					}
				}
			}
			else if (title == "ladder")
			{
				if (this.Pr.BetoMng.is_torned)
				{
					this.pose_is_stand_t = -1f;
				}
				else if (this.Pr.applying_wind)
				{
					this.pose_is_stand_t = -1f;
					title = "ladder_wind";
				}
				else
				{
					if (this.pose_is_stand_t > 0f && restart_anim > -2 && restart_anim != 1 && !loop_jumping)
					{
						return;
					}
					this.pose_is_stand_t = 1f;
					title = this.getWaitingPoseTitle("ladder");
				}
			}
			else
			{
				this.pose_is_stand_t = 0f;
			}
			bool flag = this.pose_title0 == text;
			string text2 = text;
			if (title != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(title);
				if (num <= 1325572695U)
				{
					if (num <= 508974720U)
					{
						if (num != 231209156U)
						{
							if (num != 508974720U)
							{
								goto IL_061E;
							}
							if (!(title == "crawl"))
							{
								goto IL_061E;
							}
							if (this.Pr.isBreatheStop(false, false))
							{
								title = "crawl_hardbreathe";
								goto IL_061E;
							}
							title = ((this.isWetPose() || this.isWeakPose()) ? "crawl_wet_weak" : "crawl");
							goto IL_061E;
						}
						else if (!(title == "magic_hold"))
						{
							goto IL_061E;
						}
					}
					else if (num != 718098122U)
					{
						if (num != 1313842571U)
						{
							if (num != 1325572695U)
							{
								goto IL_061E;
							}
							if (!(title == "stand2bench"))
							{
								goto IL_061E;
							}
							if (this.Pr.Ser.isShamed() || this.Pr.Ser.has(SER.MP_REDUCE))
							{
								title = "bench_shamed";
								goto IL_061E;
							}
							goto IL_061E;
						}
						else
						{
							if (!(title == "crouch"))
							{
								goto IL_061E;
							}
							string title4 = this.Anm.getCurrentPose().title;
							bool flag2 = TX.isStart(title4, "crawl", 0);
							title = (flag2 ? "crawl2crouch" : "stand2crouch");
							if (loop_jumping || (!(title4 == title) && !(title4 == "crouch2stand") && !flag2 && this.Pr.isPoseCrouch(false)))
							{
								title = "crouch";
							}
							if (this.Pr.isBreatheStop(false, false))
							{
								if (title == "stand2crouch")
								{
									title = "stand2crouch_hardbreathe";
								}
								if (title == "crouch")
								{
									title = "crouch_hardbreathe";
									goto IL_061E;
								}
								goto IL_061E;
							}
							else
							{
								if (title == "crouch" && this.Pr.applying_wind)
								{
									title = "crouch_wind";
									goto IL_061E;
								}
								goto IL_061E;
							}
						}
					}
					else
					{
						if (!(title == "run"))
						{
							goto IL_061E;
						}
						if (this.Pr.isBreatheStop(false, false))
						{
							title = "run_hardbreathe";
							goto IL_061E;
						}
						if (this.Pr.applying_wind)
						{
							title = "run_wind";
							goto IL_061E;
						}
						title = ((this.isWetPose() || this.isWeakPose()) ? "run_wet_weak" : "run");
						goto IL_061E;
					}
				}
				else if (num <= 2502056623U)
				{
					if (num != 2248295223U)
					{
						if (num != 2502056623U)
						{
							goto IL_061E;
						}
						if (!(title == "stand_ev"))
						{
							goto IL_061E;
						}
						this.pose_is_stand_t = 3000f;
						goto IL_061E;
					}
					else if (!(title == "magic_init"))
					{
						goto IL_061E;
					}
				}
				else if (num != 2686853648U)
				{
					if (num != 2805947405U)
					{
						if (num != 4166649696U)
						{
							goto IL_061E;
						}
						if (!(title == "fall"))
						{
							goto IL_061E;
						}
						if (!loop_jumping && this.Anm.getCurrentPose().end_jump_title == "fall2" && !TX.isStart(title2, "fall", 0))
						{
							return;
						}
						if (this.Pr.isBreatheStop(false, false))
						{
							title = "fall_hardbreathe";
							if (title2 == "fall_hardbreathe2")
							{
								return;
							}
							goto IL_061E;
						}
						else
						{
							if (title2 == "fall2")
							{
								return;
							}
							goto IL_061E;
						}
					}
					else
					{
						if (!(title == "jump"))
						{
							goto IL_061E;
						}
						if (!loop_jumping && this.Anm.getCurrentPose().end_jump_title == "fall2")
						{
							return;
						}
						goto IL_061E;
					}
				}
				else
				{
					if (!(title == "walk"))
					{
						goto IL_061E;
					}
					if (this.Pr.isBreatheStop(false, false))
					{
						title = (this.Pr.isBreatheStop(true, false) ? "walk_dmg_breathe" : "walk_hardbreathe");
						goto IL_061E;
					}
					if (this.Pr.applying_wind)
					{
						title = "walk_wind";
						goto IL_061E;
					}
					title = (this.isWetPose() ? "walk_wet_weak" : (this.isWeakPose() ? "walk_weak" : "walk"));
					goto IL_061E;
				}
				title = this.getSpecialMagicPose(this.Pr.getCurMagic(), title == "magic_init");
			}
			IL_061E:
			if (restart_anim < 0 && flag && title == this.pre_pose_title)
			{
				return;
			}
			this.pose_title0 = text2;
			this.pre_pose_title = title;
			PxlPose pxlPose = null;
			NoelAnimator.PoseInfo curInfo = this.CurInfo;
			NoelAnimator.PoseInfo poseInfo = (this.CurInfo = this.getPoseInfo(text, ref title, ref pxlPose));
			NoelAnimator.initPoseInfo(this.CurInfo, this.Mp.rCLENB);
			this.cur_type = poseInfo.type;
			Object currentTexture = this.Anm.getCurrentTexture();
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
			Texture currentTexture2 = this.Anm.getCurrentTexture();
			if (currentTexture != currentTexture2)
			{
				NoelAnimator.MtrAlphaClipMask.SetTexture("_MainTex", this.Anm.getCurrentTexture());
				this.fineMaterialColor();
				this.fineFrozenAppearance();
			}
			if (poseInfo.use_front_drawing)
			{
				if (this.FD_fnDrawFrontMesh == null)
				{
					this.FD_fnDrawFrontMesh = new M2DrawBinder.FnEffectBind(this.fnDrawFrontMesh);
				}
				if (this.FrontDrawEd == null)
				{
					this.FrontDrawEd = this.Mp.setED("noel frontdraw", this.FD_fnDrawFrontMesh, 0f);
				}
			}
			else if (this.FrontDrawEd != null)
			{
				this.FrontDrawEd = this.Mp.remED(this.FrontDrawEd);
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
					global::XX.AIM aim = (global::XX.AIM)((loop_jumping || this.Pr.fix_aim) ? this.Anm.pose_aim : ((global::PixelLiner.PixelLinerLib.AIM)this.Pr.aim));
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

		private NoelAnimator.PoseInfo getPoseInfo(string title0, ref string title, ref PxlPose TargetPose)
		{
			NoelAnimator.PoseInfo poseInfo = null;
			NoelAnimator.PoseInfo poseInfo2 = null;
			NoelAnimator.PoseInfo poseInfo3 = null;
			int num = global::XX.X.Mx((int)this.outfit_type, 1);
			int num2 = num;
			while (num2 >= 0 && poseInfo == null)
			{
				int i = 0;
				while (i < 2)
				{
					string text = ((i == 0) ? title : title0);
					NoelAnimator.PoseInfo poseInfo4 = null;
					if (num2 != num)
					{
						poseInfo4 = ((i == 0) ? poseInfo2 : poseInfo3);
						goto IL_0061;
					}
					if (NoelAnimator.OPoseType.TryGetValue(text, out poseInfo4))
					{
						if (i == 0)
						{
							poseInfo2 = poseInfo4;
							goto IL_0061;
						}
						poseInfo3 = poseInfo4;
						goto IL_0061;
					}
					IL_00CB:
					i++;
					continue;
					IL_0061:
					if (poseInfo4 == null)
					{
						goto IL_00CB;
					}
					PxlPose pxlPose = poseInfo4.Get(num2);
					if (pxlPose != null && (num2 != 1 || (!global::XX.X.SENSITIVE && this.Pr.BetoMng.is_torned)))
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
					goto IL_00CB;
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
				mainMeshDrawer.Col = this.Anm.color;
				if ((this.CurInfo.type & POSE_TYPE.PRESS_DAMAGE) != POSE_TYPE.STAND)
				{
					if ((this.CurInfo.type & POSE_TYPE.DOWN) != POSE_TYPE.STAND)
					{
						Matrix4x4 currentMatrix = mainMeshDrawer.getCurrentMatrix();
						float num = 0f;
						float num2 = 0f;
						int num3 = global::XX.CAim._YD((int)this.Anm.pose_aim, 1);
						if (num3 != 0 && this.Pr.is_alive)
						{
							float pressdamage_float_level = this.Pr.pressdamage_float_level;
							num = ((num3 == -1) ? (global::XX.X.ZSIN(pressdamage_float_level, 0.3f) * 24f) : 0f) - global::XX.X.ZCOS(pressdamage_float_level - 0.25f, 0.75f) * (float)((num3 == -1) ? 24 : 15);
							num2 = global::XX.X.SINI(global::XX.X.ZSIN(pressdamage_float_level), 1f) * (float)global::XX.CAim._XD((int)this.Anm.pose_aim, 1) * 3.1415927f * 0.16f;
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
			}
			return mainMeshDrawer;
		}

		private bool RenderPrepareMesh(Camera Cam, M2RenderTicket Tk, bool need_redraw, int draw_id, out MeshDrawer MdOut, ref bool paste_mesh)
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
			MdOut = ((this.CurFrame != null && this.CurFrame.clip_mask_layer != 0 && (this.CurInfo.type & POSE_TYPE.MARUNOMI) == POSE_TYPE.STAND) ? NoelAnimator.MdClip : null);
			return true;
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

		public int getAim()
		{
			return (int)this.Anm.pose_aim;
		}

		public string getSpecialMagicPose(MagicItem Mg, bool is_init)
		{
			if (Mg != null)
			{
				MGKIND kind = Mg.kind;
				if (kind != MGKIND.FIREBALL)
				{
					if (kind == MGKIND.DROPBOMB)
					{
						if (!is_init)
						{
							return "magic_bomb_hold";
						}
						return "magic_bomb_init";
					}
				}
				else if (is_init)
				{
					return "magic_fireball_init";
				}
			}
			if (!is_init)
			{
				return "magic_hold";
			}
			return "magic_init";
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
			float num = (this.poseIs(POSE_TYPE.MANGURI, false) ? global::XX.AIM.T : (this.Pr.isPoseBack(false) ? global::XX.CAim.get_opposite((global::XX.AIM)this.Anm.pose_aim) : ((global::XX.AIM)this.Anm.pose_aim)));
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

		public void fineSerState()
		{
			if (this.pose_is_stand_t != 0f)
			{
				float num = this.pose_is_stand_t;
				this.setPose(this.pose_title0, -2, false);
				if (this.pose_is_stand_t > 0f && num > 0f)
				{
					this.pose_is_stand_t = global::XX.X.Mx(this.pose_is_stand_t, num);
					return;
				}
			}
			else
			{
				string text = this.pose_title0;
				if (text != null && (text == "stand_ev" || text == "run" || text == "walk" || text == "crawl" || text == "crouch"))
				{
					this.setPose(this.pose_title0, -2, false);
				}
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

		public bool isWetPose()
		{
			return this.Pr.isWetPose();
		}

		public bool isWeakPose()
		{
			return this.Pr.isWeakPose();
		}

		public float body_agR
		{
			get
			{
				if (this.CurInfo.body_agR != -1000f)
				{
					if (global::XX.CAim._XD((int)this.Anm.pose_aim, 1) >= 0)
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
					if (global::XX.CAim._XD((int)this.Anm.pose_aim, 1) >= 0)
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
				return this.pose_is_stand_t != 0f;
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

		public NoelAnimator.OUTFIT outfit_type
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

		public static int countImageForDebug(PxlPose P)
		{
			return 0;
		}

		private const bool CHECK_DEBUG_COUNT = true;

		private readonly PRNoel Pr;

		private readonly NelM2DBase M2D;

		public bool hidden_flag;

		private NoelAnimator.NFrame CurFrame;

		private NoelAnimator.NFrame PFrame;

		private NoelAnimator.NFrame PPFrame;

		private M2PxlAnimatorRT Anm;

		private POSE_TYPE cur_type;

		private float pose_is_stand_t;

		private M2NoelCane Cane;

		private M2NoelCane CaneB;

		private M2NoelHat Hat;

		private bool hat_assign_recheck;

		private bool pre_rod_erase;

		private bool pose_down_turning;

		private List<PxlPose> ARodErasePose;

		private ElecTraceDrawer HoldMagicElec;

		private M2DrawBinder HoldMagicEd;

		private M2DrawBinder FrontDrawEd;

		private PxlFrame FrozenF;

		private NoelAnimator.PoseInfo FrozenInfo;

		private Material MtrBase;

		private Material MtrFrozen;

		private byte frozen_lv_;

		private string pose_title0 = "stand";

		private string pre_pose_title = "stand";

		private const int stand_ev_t = 3000;

		public const string stand_pose = "stand";

		public const string crouch_pose = "crouch";

		public const string rod_pose_title = "RODRODRODROD";

		private const string LAY_HEADER_MARUNOMI = "marunomi";

		private const string LAY_HEADER_TOP_LAYER = "top_layer";

		public Vector2 TeScale = Vector2.one;

		private bool fine_rot;

		private NoelAnimator.PoseInfo CurInfo;

		private PxlPose PreTargetPose;

		private static PxlImage[] AHatImg;

		private static BDic<string, NoelAnimator.PoseInfo> OPoseType;

		public static PxlCharacter MainPxl;

		private static PxlPose RodPose;

		private bool need_fine_mesh = true;

		private static Material MtrAlphaClipMask;

		private static MeshDrawer MdClip;

		private M2DrawBinder.FnEffectBind FD_drawEffectMagicElec;

		private Func<Vector2> FD_ElecGetPos;

		private M2RenderTicket.FnPrepareMd FD_RenderPrepareMesh;

		public Flagger FlgDropCane;

		private M2DrawBinder.FnEffectBind FD_fnDrawFrontMesh;

		public const float downpose_center_shift_y = 70f;

		public sealed class NFrame
		{
			public NFrame(PxlFrame F, float rCLENB)
			{
				this.RodL = M2PxlAnimator.getRodPosS(rCLENB, F, ref this.target_x, ref this.target_y, "rod", "ROD", 0.5f, 0f, ALIGN.LEFT, ALIGNY.MIDDLE, 2);
				if (this.RodL != null)
				{
					this.rodx = this.RodL.x * rCLENB;
					this.rody = this.RodL.y * rCLENB;
				}
				this.hatlost = TX.isStart(F.name, "hat_lost", 0);
			}

			public override string ToString()
			{
				return "<NFrame:" + ((this.RodL != null) ? this.RodL.ToString() : "-") + "> ";
			}

			public PxlLayer RodL;

			public float rodx;

			public float rody;

			public float target_x;

			public float target_y;

			public int clip_mask_layer;

			public bool hatlost;
		}

		public enum OUTFIT
		{
			NORMAL,
			TORNED,
			BABYDOLL,
			DOJO,
			_MAX
		}

		public sealed class PoseInfo
		{
			public PoseInfo(PxlPose P)
			{
				this.APose = new PxlPose[] { P };
				this.OFrmData = new BDic<PxlFrame, NoelAnimator.NFrame>();
			}

			public PxlPose Get(int outfit_type)
			{
				if (this.APose.Length <= outfit_type)
				{
					return null;
				}
				return this.APose[outfit_type];
			}

			public void addPose(int index, PxlPose P)
			{
				if (this.APose.Length <= index)
				{
					Array.Resize<PxlPose>(ref this.APose, index + 1);
				}
				this.APose[index] = P;
			}

			public NoelAnimator.PoseInfo copyBasicDataFrom(NoelAnimator.PoseInfo Src)
			{
				this.type = Src.type;
				this.counter_shift_x = Src.counter_shift_x;
				this.counter_shift_y = Src.counter_shift_y;
				this.body_agR = Src.body_agR;
				return this;
			}

			public string title
			{
				get
				{
					return this.APose[0].title;
				}
			}

			public string end_jump_title
			{
				get
				{
					return this.APose[0].end_jump_title;
				}
			}

			public bool use_torture
			{
				get
				{
					return this.PointData != null;
				}
			}

			public override string ToString()
			{
				return "<Info:" + this.title + "> -" + this.type.ToString();
			}

			public PxlPose[] APose;

			public NoelAnimator.PoseInfo PI_S;

			public POSE_TYPE type;

			public int counter_shift_x;

			public int counter_shift_y;

			public float body_agR = -1000f;

			public M2PxlPointContainer.PosePointsData PointData;

			public bool use_front_drawing;

			public BDic<PxlFrame, NoelAnimator.NFrame> OFrmData;

			public byte orgasm_frame_index = byte.MaxValue;

			public bool initted;
		}
	}
}
