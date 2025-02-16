using System;
using System.Collections.Generic;
using Better;
using m2d;
using PixelLiner;
using XX;

namespace nel
{
	public class PrPoseContainer
	{
		public PrPoseContainer(string _name)
		{
			this.name = _name;
		}

		public bool iniPxlResources<OUTFIT_T>(string[][] Anoel_pxls, float CLENB) where OUTFIT_T : Enum
		{
			if (this.OPoseType != null)
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
			this.OPoseType = new BDic<string, PrPoseContainer.PoseInfo>(num3);
			List<PrPoseContainer.PoseInfo> list = new List<PrPoseContainer.PoseInfo>();
			CsvReader csvReader = new CsvReader(null, CsvReader.RegSpace, false);
			int num5 = 0;
			for (int k = 0; k < num2; k++)
			{
				int num6 = Anoel_pxls[k].Length;
				for (int l = 0; l < num6; l++)
				{
					PxlCharacter pxlCharacter2 = PxlsLoader.getPxlCharacter(Anoel_pxls[k][l]);
					if (l == 0 && k == 0)
					{
						this.MainPxl = pxlCharacter2;
						this.RodPose = pxlCharacter2.getPoseByName("RODRODRODROD");
						PxlFrame frameByName = this.RodPose.getSequence(0).getFrameByName("hat");
						if (frameByName != null)
						{
							this.AHatImg = new PxlImage[]
							{
								frameByName.getLayer(0).Img,
								frameByName.getLayer(1).Img
							};
						}
					}
					int num7 = pxlCharacter2.countPoses();
					for (int m = 0; m < num7; m++)
					{
						PxlPose pose = pxlCharacter2.getPose(m);
						if (!(pose.title == "RODRODRODROD"))
						{
							PrPoseContainer.PoseInfo poseInfo = null;
							if (k == 0)
							{
								if (this.OPoseType.TryGetValue(pose.title, out poseInfo))
								{
									if (pose.title != "stand")
									{
										X.dl("ポーズ名が重複している: " + pose.ToString(), null, false, false);
										goto IL_087C;
									}
									goto IL_087C;
								}
								else
								{
									Dictionary<string, PrPoseContainer.PoseInfo> oposeType = this.OPoseType;
									string title = pose.title;
									PrPoseContainer.PoseInfo poseInfo2 = new PrPoseContainer.PoseInfo(pose);
									poseInfo2.type = (TX.isStart(pose.title, "torture", 0) ? POSE_TYPE.USE_POINT : POSE_TYPE.STAND);
									PrPoseContainer.PoseInfo poseInfo3 = poseInfo2;
									oposeType[title] = poseInfo2;
									poseInfo = poseInfo3;
								}
							}
							else
							{
								if (!this.OPoseType.TryGetValue(pose.title, out poseInfo))
								{
									X.dl(Enum.ToObject(typeof(OUTFIT_T), k).ToString() + " ポーズの指定先が存在しない: " + pose.ToString(), null, false, false);
									goto IL_087C;
								}
								if (pose.title == "stand" && l != 0)
								{
									goto IL_087C;
								}
								poseInfo.addPose(k, pose);
								num5++;
							}
							string title2 = pose.title;
							if (k == 0)
							{
								csvReader.parseText(pose.comment.ToLower());
								while (csvReader.read())
								{
									string cmd = csvReader.cmd;
									if (cmd != null)
									{
										uint num8 = <PrivateImplementationDetails>.ComputeStringHash(cmd);
										if (num8 <= 2273894358U)
										{
											if (num8 <= 811740619U)
											{
												if (num8 <= 329414684U)
												{
													if (num8 != 72357410U)
													{
														if (num8 != 146273485U)
														{
															if (num8 == 329414684U)
															{
																if (cmd == "press_damage")
																{
																	poseInfo.type |= POSE_TYPE.PRESS_DAMAGE;
																}
															}
														}
														else if (cmd == "use_point")
														{
															poseInfo.type |= POSE_TYPE.USE_POINT;
														}
													}
													else if (cmd == "manguri")
													{
														poseInfo.type |= POSE_TYPE.MANGURI;
													}
												}
												else if (num8 != 344109131U)
												{
													if (num8 != 502659160U)
													{
														if (num8 == 811740619U)
														{
															if (cmd == "crouchb")
															{
																poseInfo.type |= POSE_TYPE.CROUCH | POSE_TYPE.BACK;
															}
														}
													}
													else if (cmd == "damage_reset")
													{
														poseInfo.type |= POSE_TYPE.DAMAGE_RESET;
													}
												}
												else if (cmd == "counter_expand_x")
												{
													poseInfo.counter_expand_x = (byte)csvReader.Int(1, 0);
												}
											}
											else if (num8 <= 1313842571U)
											{
												if (num8 != 1035581717U)
												{
													if (num8 != 1203378667U)
													{
														if (num8 == 1313842571U)
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
												else if (cmd == "down")
												{
													poseInfo.type |= POSE_TYPE.DOWN;
												}
											}
											else if (num8 != 1436210240U)
											{
												if (num8 != 1628932526U)
												{
													if (num8 == 2273894358U)
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
										else if (num8 <= 2805947405U)
										{
											if (num8 <= 2724132437U)
											{
												if (num8 != 2341128772U)
												{
													if (num8 != 2721425853U)
													{
														if (num8 == 2724132437U)
														{
															if (cmd == "downb")
															{
																poseInfo.type |= POSE_TYPE.DOWN | POSE_TYPE.BACK;
															}
														}
													}
													else if (cmd == "orgasm_frame_index")
													{
														poseInfo.orgasm_frame_index = (byte)csvReader.Int(1, 255);
													}
												}
												else if (cmd == "orgasm")
												{
													poseInfo.type |= POSE_TYPE.ORGASM;
												}
											}
											else if (num8 != 2730307407U)
											{
												if (num8 != 2770559742U)
												{
													if (num8 == 2805947405U)
													{
														if (cmd == "jump")
														{
															poseInfo.type |= POSE_TYPE.JUMP;
														}
													}
												}
												else if (cmd == "absorb_default")
												{
													poseInfo.type |= POSE_TYPE.ABSORB_DEFAULT;
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
										else if (num8 <= 2904147653U)
										{
											if (num8 != 2855531612U)
											{
												if (num8 != 2872309231U)
												{
													if (num8 == 2904147653U)
													{
														if (cmd == "body_angle")
														{
															poseInfo.body_agR = X.GAR(0f, 0f, csvReader.Nm(1, 0f), csvReader.Nm(2, 0f));
														}
													}
												}
												else if (cmd == "counter_shift_x")
												{
													poseInfo.counter_shift_x = csvReader.Int(1, 0);
												}
											}
											else if (cmd == "counter_shift_y")
											{
												poseInfo.counter_shift_y = csvReader.Int(1, 0);
											}
										}
										else if (num8 != 2980534815U)
										{
											if (num8 != 3195815179U)
											{
												if (num8 == 4166649696U)
												{
													if (cmd == "fall")
													{
														poseInfo.type |= POSE_TYPE.FALL;
													}
												}
											}
											else if (cmd == "jump_on_ground")
											{
												poseInfo.type |= POSE_TYPE.JUMP_ON_GROUND;
											}
										}
										else if (cmd == "marunomi")
										{
											poseInfo.type |= POSE_TYPE.MARUNOMI;
										}
									}
								}
							}
						}
						IL_087C:;
					}
				}
			}
			if (list.Count != 0)
			{
				int count = list.Count;
				for (int n = 0; n < count; n++)
				{
					PrPoseContainer.PoseInfo poseInfo4 = list[n];
					string text = poseInfo4.title + "_sensitive";
					PrPoseContainer.PoseInfo poseInfo5;
					if (this.OPoseType.TryGetValue(text, out poseInfo5))
					{
						poseInfo4.PI_S = poseInfo5.copyBasicDataFrom(poseInfo4);
						this.OPoseType.Remove(text);
					}
					else
					{
						X.de("sensitive ポーズ " + text + " が見つかりません", null);
					}
				}
			}
			return true;
		}

		public void initPoseInfo(PrPoseContainer.PoseInfo PI, float rCLENB)
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
						PrPoseContainer.NFrame nframe;
						if (!PI.OFrmData.TryGetValue(frame, out nframe))
						{
							nframe = (PI.OFrmData[frame] = new PrPoseContainer.NFrame(frame, rCLENB));
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
			if ((PI.type & POSE_TYPE.USE_POINT) != POSE_TYPE.STAND)
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
								PrPoseContainer.NFrame nframe2;
								if (!PI.OFrmData.TryGetValue(frame2, out nframe2))
								{
									nframe2 = (PI.OFrmData[frame2] = new PrPoseContainer.NFrame(frame2, rCLENB));
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
											layer.alpha = -X.Abs(layer.alpha);
										}
									}
								}
							}
						}
						num3++;
					}
				}
			}
			if ((PI.type & POSE_TYPE.USE_TOP_LAYER) != POSE_TYPE.STAND)
			{
				PI.use_front_drawing = true;
			}
		}

		public PxlPose getRodPose()
		{
			return this.RodPose;
		}

		public PxlImage[] getHatImgArray()
		{
			return this.AHatImg;
		}

		public PrPoseContainer.PoseInfo Get(string s)
		{
			return X.Get<string, PrPoseContainer.PoseInfo>(this.OPoseType, s);
		}

		public bool TryGetValue(string s, out PrPoseContainer.PoseInfo _Info)
		{
			return this.OPoseType.TryGetValue(s, out _Info);
		}

		public override string ToString()
		{
			return "PrPoseContainer:" + this.name;
		}

		public static int countImageForDebug(PxlPose P)
		{
			return 0;
		}

		private const bool CHECK_DEBUG_COUNT = false;

		public readonly string name;

		public const string rod_pose_title = "RODRODRODROD";

		public const string stand_pose = "stand";

		private BDic<string, PrPoseContainer.PoseInfo> OPoseType;

		public PxlCharacter MainPxl;

		private PxlPose RodPose;

		private PxlImage[] AHatImg;

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

		public sealed class PoseInfo
		{
			public PoseInfo(PxlPose P)
			{
				this.APose = new PxlPose[] { P };
				this.OFrmData = new BDic<PxlFrame, PrPoseContainer.NFrame>();
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

			public PrPoseContainer.PoseInfo copyBasicDataFrom(PrPoseContainer.PoseInfo Src)
			{
				this.type = Src.type;
				this.counter_shift_x = Src.counter_shift_x;
				this.counter_expand_x = Src.counter_expand_x;
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

			public PrPoseContainer.PoseInfo PI_S;

			public POSE_TYPE type;

			public int counter_shift_x;

			public int counter_shift_y;

			public byte counter_expand_x = 50;

			public float body_agR = -1000f;

			public M2PxlPointContainer.PosePointsData PointData;

			public bool use_front_drawing;

			public BDic<PxlFrame, PrPoseContainer.NFrame> OFrmData;

			public byte orgasm_frame_index = byte.MaxValue;

			public bool initted;
		}
	}
}
