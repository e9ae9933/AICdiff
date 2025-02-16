using System;
using System.Collections.Generic;
using Better;
using Spine;
using UnityEngine;
using UnityEngine.Rendering;
using XX;

namespace nel
{
	public class UIPictureBodySpine : UIPictureBodyData
	{
		public UIPictureBodySpine(string _name, UIPictureBase _PCon)
			: base(_name, _PCon, null)
		{
			this.AAnim = new List<UIPictureBodySpine.StateVariation>();
			this.OSkin = new BDic<string, UIPictureBodySpine.StateVariation>();
			if (UIPictureBodySpine.ABufSkin == null)
			{
				UIPictureBodySpine.ABufSkin = new List<string>(2);
				UIPictureBodySpine.ABufSkinSt = new List<uint>(2);
			}
		}

		protected override void readSpineData(CsvReader CR, string chara_key)
		{
			string cmd = CR.cmd;
			if (cmd != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(cmd);
				UIPictureBodySpine.StateVariation stateVariation;
				UIPictureBodySpine.StateVariation stateVariation2;
				if (num <= 1211284114U)
				{
					if (num <= 310769283U)
					{
						if (num <= 185818534U)
						{
							if (num != 56074798U)
							{
								if (num != 185818534U)
								{
									return;
								}
								if (!(cmd == "%SPINE_JSON_KEY"))
								{
									return;
								}
								this.Spv.replace_json_key = CR._1;
								return;
							}
							else if (!(cmd == "%SPINE_SKIN_REMOVE"))
							{
								return;
							}
						}
						else if (num != 201341301U)
						{
							if (num != 259756148U)
							{
								if (num != 310769283U)
								{
									return;
								}
								if (!(cmd == "%SPINE_INITIALIZE_LOAD"))
								{
									return;
								}
								this.Spv.initializeLoad(this.PCon.MtrSpine);
								return;
							}
							else
							{
								if (!(cmd == "%SPINE_CULL"))
								{
									return;
								}
								this.Cull = (CullMode)Enum.Parse(typeof(CullMode), CR._1);
								return;
							}
						}
						else
						{
							if (!(cmd == "%SPINE_TS"))
							{
								return;
							}
							if (TX.isStart(CR._1, '*'))
							{
								this.timeScale *= X.Nm(TX.slice(CR._1, 1), 1f, false);
								return;
							}
							this.timeScale = CR.Nm(1, 1f);
							return;
						}
					}
					else if (num <= 505035822U)
					{
						if (num != 360074430U)
						{
							if (num != 361836841U)
							{
								if (num != 505035822U)
								{
									return;
								}
								if (!(cmd == "%SPINE_ADD_ANIM_REMOVE"))
								{
									return;
								}
								goto IL_089A;
							}
							else
							{
								if (!(cmd == "%SPINE_ANIM_REMOVE_WITH_SKIN"))
								{
									return;
								}
								goto IL_07F6;
							}
						}
						else
						{
							if (!(cmd == "%SPINE_AF_START_RANGE"))
							{
								return;
							}
							this.af_range_min = CR.Int(1, 0);
							this.af_range_max = CR.Int(2, 0);
							return;
						}
					}
					else
					{
						if (num != 828640909U)
						{
							if (num != 1098746962U)
							{
								if (num != 1211284114U)
								{
									return;
								}
								if (!(cmd == "%SPINE_ANIM_REMOVE"))
								{
									return;
								}
								goto IL_0722;
							}
							else if (!(cmd == "%SPINE_ADD_SKIN_REMOVE"))
							{
								return;
							}
						}
						else if (!(cmd == "%SPINE_ADD_SKIN"))
						{
							return;
						}
						stateVariation = new UIPictureBodySpine.StateVariation
						{
							Aname = CR._2.Split(new char[] { '|' }),
							anim_id = CR.Int(4, 0),
							state = UIPictureBase.EMSTATE.NORMAL,
							st_add = this.PCon.getMultipleAdditionalEmot(CR._1),
							check_or = (CR._1.IndexOf("||") >= 0),
							alpha = -1f,
							reverse_flag = (CR.cmd == "%SPINE_ADD_SKIN_REMOVE")
						};
						stateVariation2 = stateVariation;
						this.OSkin[stateVariation2.key_name] = stateVariation2;
						if (CR.clength >= 4)
						{
							this.Adefault_skin = CR._3.Split(new char[] { '|' });
							return;
						}
						return;
					}
				}
				else if (num <= 1739399806U)
				{
					if (num <= 1529538705U)
					{
						if (num != 1225533133U)
						{
							if (num != 1285604463U)
							{
								if (num != 1529538705U)
								{
									return;
								}
								if (!(cmd == "%SPINE_SKIN"))
								{
									return;
								}
							}
							else
							{
								if (!(cmd == "%SPINE_AF_RESET_PROV"))
								{
									return;
								}
								this.reset_prob_on_same_anim = CR.Int(1, 0);
								return;
							}
						}
						else
						{
							if (!(cmd == "%SPINE_ANIM"))
							{
								return;
							}
							goto IL_0722;
						}
					}
					else if (num != 1695751543U)
					{
						if (num != 1729134622U)
						{
							if (num != 1739399806U)
							{
								return;
							}
							if (!(cmd == "%SPINE_RSHIFT"))
							{
								return;
							}
							this.rightshift_px = CR.Nm(1, this.rightshift_px);
							return;
						}
						else
						{
							if (!(cmd == "%SPINE_ANIM_WITH_SKIN"))
							{
								return;
							}
							goto IL_07F6;
						}
					}
					else
					{
						if (!(cmd == "%SPINE_MOSAIC_BONE"))
						{
							return;
						}
						UIPictureBodyData.MosaicPos mosaicPos = base.addMosaicPosData(CR._1, CR._3, CR._4, false, false);
						this.mosaic_radius_px = CR.Int(2, this.mosaic_radius_px);
						this.variation_bits |= mosaicPos.state;
						return;
					}
				}
				else if (num <= 2097867959U)
				{
					if (num != 1876063477U)
					{
						if (num != 1979138251U)
						{
							if (num != 2097867959U)
							{
								return;
							}
							if (!(cmd == "%SPINE_SENSITIVE_BONE"))
							{
								return;
							}
							UIPictureBodyData.MosaicPos mosaicPos2 = base.addMosaicPosData(CR._1, CR._2, CR._3, true, false);
							this.variation_bits |= mosaicPos2.state;
							return;
						}
						else
						{
							if (!(cmd == "%SPINE_MOSAIC_RADIUS"))
							{
								return;
							}
							this.mosaic_radius_px = CR.Int(1, 30);
							return;
						}
					}
					else
					{
						if (!(cmd == "%SPINE_PTC_KEY"))
						{
							return;
						}
						this.Aptc_key = CR.slice(1, -1000);
						return;
					}
				}
				else if (num != 2262537873U)
				{
					if (num != 2815240503U)
					{
						if (num != 3625517095U)
						{
							return;
						}
						if (!(cmd == "%SPINE_MAIN_ANIM"))
						{
							return;
						}
						List<string> list = new List<string>();
						for (int i = 1; i < CR.clength; i++)
						{
							list.AddRange(CR.getIndex(i).Split(new char[] { '|' }));
						}
						if (this.main_loopf != -1000)
						{
							for (int j = list.Count - 1; j >= 0; j--)
							{
								this.Spv.setAnmLoopFrame(list[j], this.main_loopf);
							}
						}
						this.Abase_anim = list.ToArray();
						return;
					}
					else
					{
						if (!(cmd == "%SPINE"))
						{
							return;
						}
						this.Spv = new SpineViewerNel(this, CR._1, chara_key);
						this.first_line = CR.get_cur_line();
						if (!FEnum<UIEMOT>.TryParse(CR._2U, out this.base_emo, true))
						{
							CR.de("emotion エラー: " + CR._2);
						}
						bool flag = false;
						if (CR._3 != "_")
						{
							this.base_state = this.PCon.getMultipleEmot(CR._3);
							flag = true;
						}
						this.scale_ = CR.Nm(6, 0.5f);
						this.swidth = CR.Nm(4, 0f);
						this.sheight = CR.Nm(5, 0f);
						string text = CR.getIndex(7);
						if (TX.isStart(text, "!", 0))
						{
							text = TX.slice(text, 1);
							this.Spv.whole_loop_set = true;
						}
						this.main_loopf = X.NmI(text, -1000, false, false);
						this.variation_bits |= this.base_state;
						if (flag)
						{
							this.PCon.assignSpineBodyData(this, this.base_emo, this.base_state, this.Spv, CR._3.IndexOf("||") >= 0);
						}
						else
						{
							this.PCon.SpineViewerInitObject(this.Spv);
						}
						this.Spv.deactivate();
						if (this.Spv.is_initialize_svtexture)
						{
							this.Spv.initializeLoad(this.PCon.MtrSpine);
						}
						else
						{
							this.Spv.setMaterial(this.PCon.MtrSpine);
						}
						this.Spv.addListenerEvent(new AnimationState.TrackEntryEventDelegate(this.fnSpineListenerEvent));
						return;
					}
				}
				else
				{
					if (!(cmd == "%SPINE_ADD_ANIM"))
					{
						return;
					}
					goto IL_089A;
				}
				UIPictureBase.EMSTATE emstate = this.PCon.getMultipleEmot(CR._1);
				this.variation_bits |= emstate;
				stateVariation = new UIPictureBodySpine.StateVariation
				{
					Aname = CR._2.Split(new char[] { '|' }),
					anim_id = CR.Int(4, 0),
					state = emstate,
					check_or = (CR._1.IndexOf("||") >= 0),
					alpha = -1f,
					reverse_flag = (CR.cmd == "%SPINE_SKIN_REMOVE")
				};
				stateVariation2 = stateVariation;
				this.OSkin[stateVariation2.key_name] = stateVariation2;
				if (CR.clength >= 4 && TX.valid(CR._3) && CR._3 != "''")
				{
					this.Adefault_skin = CR._3.Split(new char[] { '|' });
					return;
				}
				return;
				IL_0722:
				emstate = this.PCon.getMultipleEmot(CR._1);
				this.variation_bits |= emstate;
				int num2 = CR.Int(3, 1);
				this.max_anim_id = X.Mx(num2, this.max_anim_id);
				List<UIPictureBodySpine.StateVariation> aanim = this.AAnim;
				stateVariation = new UIPictureBodySpine.StateVariation(CR._2.Split(new char[] { '|' }), this.Spv, CR.Int(5, -1000))
				{
					anim_id = num2,
					state = emstate,
					check_or = (CR._1.IndexOf("||") >= 0),
					alpha = CR.Nm(4, 1f),
					reverse_flag = (CR.cmd == "%SPINE_ANIM_REMOVE")
				};
				aanim.Add(stateVariation);
				return;
				IL_07F6:
				int num3 = CR.Int(3, 1);
				this.max_anim_id = X.Mx(num3, this.max_anim_id);
				List<UIPictureBodySpine.StateVariation> aanim2 = this.AAnim;
				stateVariation = new UIPictureBodySpine.StateVariation(CR._2.Split(new char[] { '|' }), this.Spv, CR.Int(5, -1000))
				{
					anim_id = num3,
					state = UIPictureBase.EMSTATE.NORMAL,
					enable_skin = CR._1,
					alpha = CR.Nm(4, 1f),
					reverse_flag = (CR.cmd == "%SPINE_ANIM_REMOVE_WITH_SKIN")
				};
				aanim2.Add(stateVariation);
				return;
				IL_089A:
				int num4 = CR.Int(3, 1);
				this.max_anim_id = X.Mx(num4, this.max_anim_id);
				List<UIPictureBodySpine.StateVariation> aanim3 = this.AAnim;
				stateVariation = new UIPictureBodySpine.StateVariation(CR._2.Split(new char[] { '|' }), this.Spv, CR.Int(5, -1000))
				{
					anim_id = num4,
					st_add = this.PCon.getMultipleAdditionalEmot(CR._1),
					state = UIPictureBase.EMSTATE.NORMAL,
					check_or = (CR._1.IndexOf("||") >= 0),
					alpha = CR.Nm(4, 1f),
					reverse_flag = (CR.cmd == "%SPINE_ADD_ANIM_REMOVE")
				};
				aanim3.Add(stateVariation);
				return;
			}
		}

		public void prepareMaterial(object _SvT)
		{
			BetobetoManager.SvTexture svTexture = _SvT as BetobetoManager.SvTexture;
			if (svTexture != null && this.Spv.isSame(svTexture))
			{
				this.Spv.initializeLoad(this.PCon.MtrSpine);
			}
		}

		public override void run(Transform Tr, float fcnt_d, float TS, bool force = false)
		{
			base.run(Tr, UIPicture.tecon_TS * fcnt_d, TS, force);
			if (force)
			{
				this.Spv.updateAnim(true, 0f);
				return;
			}
			this.Spv.updateAnim(true, TS * fcnt_d * this.timeScale * 1f * this.PCon.TS_animation);
		}

		public override bool redraw(MeshDrawer Md, UIPictureBase.PrEmotion.BodyPaint Bp, UIPictureBase.EMSTATE st, int first, IEfPtcSetable EfSt)
		{
			if (first >= 1)
			{
				if (first == 1 && this.Spv.getAnmPlayTime(0) <= 0.25f && X.xors(100) >= this.reset_prob_on_same_anim)
				{
					return false;
				}
				this.animRandomize(this.Spv, st, out this.pre_skin, out this.current_state);
				this.PCon.MtrSpine.SetFloat("_Cull", (float)this.Cull);
			}
			if (first == 0)
			{
				int trackLength = this.Spv.getTrackLength();
				for (int i = 0; i < trackLength; i++)
				{
					this.Spv.setTimePosition(0f);
				}
			}
			this.PtcSetTo(EfSt);
			return false;
		}

		public void PtcSetTo(IEfPtcSetable EfSt)
		{
			if (this.Aptc_key != null && EfSt != null)
			{
				int num = this.Aptc_key.Length;
				for (int i = 0; i < num; i++)
				{
					EfSt.PtcST(this.Aptc_key[i], null, PTCThread.StFollow.NO_FOLLOW);
				}
			}
		}

		private void assignSpineBodyData(UIPictureBase.EMSTATE st)
		{
			this.PCon.assignSpineBodyData(this, this.base_emo, st, null, false);
		}

		public override Material initEmot(UIPictureBase.EMSTATE st, MeshDrawer Md)
		{
			base.initEmot(st, null);
			return this.Mtr;
		}

		public override void fineMaterial(MeshDrawer Md)
		{
			this.Spv.prepareMaterial(this.Spv.getMaterial());
			if (this.PCon.useValotileFilterMode(true) != null)
			{
				this.Spv.force_enable_mesh_render = true;
			}
		}

		public void animRandomize(SpineViewer Spv, UIPictureBase.EMSTATE st, out string pre_skin, out UIPictureBase.EMSTATE current_state)
		{
			pre_skin = "";
			current_state = UIPictureBase.EMSTATE.NORMAL;
			if (this.Abase_anim == null)
			{
				X.de("ベースアニメを指定していない: " + this.first_line.ToString() + "行の定義", null);
				return;
			}
			if (UIPictureBodySpine.Aaf_cache == null || UIPictureBodySpine.Aaf_cache.Length <= this.max_anim_id)
			{
				UIPictureBodySpine.Aaf_cache = new float[this.max_anim_id + 1];
			}
			float num = -1f;
			for (int i = 0; i <= this.max_anim_id; i++)
			{
				float num2 = -1f;
				if (num2 < 0f)
				{
					if (num < 0f)
					{
						num = X.Mx(0f, X.NI(this.af_range_min, this.af_range_max, X.XORSP()) / 60f);
					}
					num2 = num;
				}
				UIPictureBodySpine.Aaf_cache[i] = num2;
			}
			if (this.ATempList == null && this.max_anim_id > 0)
			{
				this.ATempList = new UIPictureBodySpine.StateVariation[this.max_anim_id];
				this.ATempState = new int[this.max_anim_id];
			}
			UIPictureBodySpine.ABufSkinSt.Clear();
			UIPictureBodySpine.ABufSkin.Clear();
			UIPictureBodySpine.ABufSkin.Add(null);
			UIPictureBodySpine.ABufSkinSt.Add(0U);
			foreach (KeyValuePair<string, UIPictureBodySpine.StateVariation> keyValuePair in this.OSkin)
			{
				UIPictureBodySpine.StateVariation value = keyValuePair.Value;
				if (value.Aname != null && value.Aname.Length != 0)
				{
					int anim_id = value.anim_id;
					while (UIPictureBodySpine.ABufSkin.Count <= anim_id)
					{
						UIPictureBodySpine.ABufSkin.Add(null);
						UIPictureBodySpine.ABufSkinSt.Add(0U);
					}
					if (value.checkState(st) && !value.disableSTA(this.current_sta))
					{
						uint num3 = (uint)(st & value.state);
						if (value.check_or)
						{
							num3 = 1U << X.max_bit(num3);
						}
						if (UIPictureBodySpine.ABufSkin[anim_id] == null || UIPictureBodySpine.ABufSkinSt[anim_id] < num3)
						{
							UIPictureBodySpine.ABufSkinSt[anim_id] = num3;
							UIPictureBodySpine.ABufSkin[anim_id] = value.Aname[X.xors(value.Aname.Length)];
						}
					}
				}
			}
			string text = UIPictureBodySpine.ABufSkin[0];
			if (text == null && this.Adefault_skin != null)
			{
				text = this.Adefault_skin[X.xors(this.Adefault_skin.Length)];
			}
			Spv.clearAnim(this.Abase_anim[X.xors(this.Abase_anim.Length)], -1000, text);
			if (UIPictureBodySpine.ABufSkin.Count >= 1)
			{
				for (int j = 1; j < UIPictureBodySpine.ABufSkin.Count; j++)
				{
					string text2 = UIPictureBodySpine.ABufSkin[j];
					if (TX.valid(text2))
					{
						Spv.GetSkeleton().MergeSkin(text2);
					}
				}
				Spv.GetSkeleton().UpdateCache();
			}
			Spv.setTimePosition(0, UIPictureBodySpine.Aaf_cache[0]);
			pre_skin = text;
			current_state = st;
			if (this.max_anim_id > 0)
			{
				X.ALLM1(this.ATempState);
				int count = this.AAnim.Count;
				for (int k = 0; k < count; k++)
				{
					UIPictureBodySpine.StateVariation stateVariation = this.AAnim[k];
					if (stateVariation.checkState(st))
					{
						int anim_id2 = stateVariation.anim_id;
						uint num4 = (uint)((stateVariation.reverse_flag ? (~st) : st) & stateVariation.state);
						if (!TX.valid(stateVariation.enable_skin) || pre_skin == stateVariation.enable_skin != stateVariation.reverse_flag)
						{
							if (stateVariation.st_add > UIPictureBase.EMSTATE_ADD.NORMAL)
							{
								if (stateVariation.disableSTA(this.current_sta))
								{
									goto IL_03F9;
								}
								num4 = (uint)((stateVariation.reverse_flag ? (~this.current_sta) : this.current_sta) & stateVariation.st_add);
							}
							if (stateVariation.check_or && num4 != 0U)
							{
								num4 = 1U << X.max_bit(num4);
							}
							if ((ulong)num4 > (ulong)((long)this.ATempState[anim_id2 - 1]))
							{
								this.ATempState[anim_id2 - 1] = (int)num4;
								this.ATempList[anim_id2 - 1] = stateVariation;
							}
						}
					}
					IL_03F9:;
				}
				for (int l = 0; l < this.max_anim_id; l++)
				{
					if (this.ATempState[l] >= 0)
					{
						UIPictureBodySpine.StateVariation stateVariation2 = this.ATempList[l];
						Spv.addAnim(stateVariation2.anim_id, stateVariation2.Aname[X.xors(stateVariation2.Aname.Length)], -1000, 0f, stateVariation2.alpha);
						Spv.setTimePosition(stateVariation2.anim_id, UIPictureBodySpine.Aaf_cache[stateVariation2.anim_id]);
					}
				}
			}
		}

		public bool FineBetobeto(bool fine_material)
		{
			this.Spv.fineTextureReloadImmediately(ref fine_material);
			return this.Spv.checkNeedUpdateTexture(fine_material);
		}

		public override void closeEmot(UIPictureBodyData NextBd)
		{
			this.Spv.closeEmot();
			UIPictureBodySpine uipictureBodySpine = NextBd as UIPictureBodySpine;
		}

		public override bool isPreparedResource()
		{
			return this.Spv.isPreparedResource();
		}

		public void releaseWithSvTextureSpine(BetobetoManager.SvTexture Svt)
		{
			if (this.Spv != null)
			{
				this.Spv.releaseWithSvTextureSpine(Svt);
			}
		}

		public override bool readPtcScript(PTCThread rER)
		{
			if (base.readPtcScript(rER))
			{
				return true;
			}
			string cmd = rER.cmd;
			if (cmd != null && cmd == "%DEFINE_BONE_POS")
			{
				Bone bone = this.Spv.FindBone(rER._1);
				if (bone == null)
				{
					X.dl("ボーンの名称不明:" + rER._1, null, false, false);
				}
				else
				{
					Vector3 vector = this.Spv.gameObject.transform.TransformPoint(new Vector3(bone.WorldX, bone.WorldY));
					Vector3 vector2 = this.PCon.getCenterUPos(true);
					rER.Def(TX.valid(rER._2) ? rER._2 : "_x", (vector.x - vector2.x) * 64f);
					rER.Def(TX.valid(rER._3) ? rER._3 : "_y", (vector.y - vector2.y) * 64f);
				}
				return true;
			}
			return false;
		}

		public override bool getEffectReposition(string s, out Vector3 V)
		{
			if (UIPictureBodySpine.getEffectReposition(this.Spv, this.Spv.FindBone(s), out V))
			{
				return true;
			}
			if (s != null && s == "follow_hip")
			{
				UIPictureBodyData.MosaicPos mosaicPosDataAppearAlways = base.getMosaicPosDataAppearAlways();
				if (mosaicPosDataAppearAlways.valid && UIPictureBodySpine.getEffectReposition(this.Spv, this.Spv.FindBone(mosaicPosDataAppearAlways.bone_key), out V))
				{
					return true;
				}
			}
			return false;
		}

		public static bool getEffectReposition(SpineViewer Spv, Bone _Bone, out Vector3 V)
		{
			V = Vector2.zero;
			if (_Bone != null && UIBase.Instance != null)
			{
				V = Spv.gameObject.transform.TransformPoint(new Vector3(_Bone.WorldX, _Bone.WorldY));
				V.z = _Bone.WorldRotationX * 0.017453292f;
				return true;
			}
			return false;
		}

		public override float base_swidth
		{
			get
			{
				return this.swidth * this.scale * this.PCon.PosTeScale.x;
			}
		}

		public override float base_sheight
		{
			get
			{
				return this.sheight * this.scale * this.PCon.PosTeScale.y;
			}
		}

		public override float scale
		{
			get
			{
				return this.scale_ * base.PosSyncSlide.z;
			}
		}

		public override float shift_ux
		{
			get
			{
				float num = -this.swidth * this.scale * 0.015625f * 0.5f;
				if (this.rightshift_px != 0f)
				{
					num += this.PCon.is_position_right * this.rightshift_px * 0.015625f;
				}
				return num + base.PosSyncSlide.x * this.scale;
			}
		}

		public override float shift_uy
		{
			get
			{
				return this.sheight * this.scale * 0.015625f * 0.5f + base.PosSyncSlide.y * this.scale;
			}
		}

		public override Material Mtr
		{
			get
			{
				return this.PCon.MtrSpine;
			}
		}

		public override Texture texture
		{
			get
			{
				return this.Spv.getTexture();
			}
		}

		public SpineViewerNel getViewer()
		{
			return this.Spv;
		}

		public string viewer_key
		{
			get
			{
				return this.Spv.key;
			}
		}

		protected override float base_scale
		{
			get
			{
				return 1f;
			}
		}

		private void fnSpineListenerEvent(TrackEntry Entry, Event e)
		{
			this.PCon.SpineListenerEvent(this, Entry, e);
		}

		public override bool getSensitiveOrMosaicRect(ref Matrix4x4 Out, int id, ref MeshAttachment OutMesh, ref Slot BelongSlot)
		{
			return this.getSensitiveOrMosaicRect(ref Out, id, ref OutMesh, ref BelongSlot, this.Spv);
		}

		public bool getSensitiveOrMosaicRect(ref Matrix4x4 Out, int id, ref MeshAttachment OutMesh, ref Slot BelongSlot, SpineViewer TargetSpv)
		{
			if (this.AMosaicPos == null)
			{
				return false;
			}
			if (!X.BTW(0f, (float)id, (float)this.AMosaicPos.Count))
			{
				return false;
			}
			UIPictureBodyData.MosaicPos mosaicPos = this.AMosaicPos[id];
			if (!mosaicPos.isActive(X.SENSITIVE, this.current_state, this.current_sta))
			{
				return false;
			}
			Bone bone = TargetSpv.FindBone(mosaicPos.bone_key);
			if (bone != null)
			{
				float num = (float)this.mosaic_radius_px * 0.015625f;
				Vector3 vector = TargetSpv.gameObject.transform.TransformPoint(new Vector3(bone.WorldX, bone.WorldY, 0f));
				Out = Matrix4x4.Translate(new Vector3(vector.x, vector.y, 0f)) * Matrix4x4.Scale(new Vector3(num * 2f, num * 2f, 0f));
			}
			return true;
		}

		public override Vector2 getPosition(UIPictureBodyData.POS id)
		{
			int num;
			if (id == UIPictureBodyData.POS.UNDER && base.getMosaicPosDataAppearAlways(out num).valid)
			{
				Matrix4x4 identity = Matrix4x4.identity;
				MeshAttachment meshAttachment = null;
				Slot slot = null;
				if (this.getSensitiveOrMosaicRect(ref identity, num, ref meshAttachment, ref slot))
				{
					Vector3 vector = identity.MultiplyPoint3x4(Vector3.zero);
					Vector2 centerUPos = this.PCon.getCenterUPos(true);
					return new Vector2((vector.x - centerUPos.x) * 64f, (vector.y - centerUPos.y) * 64f);
				}
			}
			return base.getPosition(id);
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"<BODY-SPINE>",
				this.name,
				" ... EMOT:",
				this.Emot.emot_id.ToString(),
				"/MA:",
				TX.join<string>(",", this.Abase_anim, 0, -1)
			});
		}

		public void validateCsv(Material MtrSpine)
		{
			this.Spv.prepareMaterial(MtrSpine);
			if (this.Abase_anim == null)
			{
				X.de("<BODY-SPINE> " + this.name + " に ベースアニメが指定されていません", null);
			}
			else
			{
				this.validateCsvAnim(this.Abase_anim);
			}
			if (this.AMosaicPos != null)
			{
				this.validateCsvBone(this.AMosaicPos);
			}
			int count = this.AAnim.Count;
			for (int i = 0; i < count; i++)
			{
				UIPictureBodySpine.StateVariation stateVariation = this.AAnim[i];
				if (stateVariation.anim_id >= 0)
				{
					this.validateCsvAnim(stateVariation.Aname);
				}
				else
				{
					this.validateCsvSkin(stateVariation.Aname);
				}
			}
		}

		private void validateCsvAnim(string[] As)
		{
			if (As == null)
			{
				return;
			}
			for (int i = 0; i < As.Length; i++)
			{
				try
				{
					if (!this.Spv.existAnim(As[i]))
					{
						X.de(string.Concat(new string[]
						{
							"<BODY-SPINE> ",
							this.name,
							" に アニメ ",
							As[i],
							" が存在しません"
						}), null);
					}
				}
				catch (Exception ex)
				{
					X.de(As[i] + "アニメの検索中にエラー:" + ex.ToString(), null);
				}
			}
		}

		private void validateCsvSkin(string[] As)
		{
			if (As == null)
			{
				return;
			}
			for (int i = 0; i < As.Length; i++)
			{
				if (!this.Spv.existSkin(As[i]))
				{
					X.de(string.Concat(new string[]
					{
						"<BODY-SPINE> ",
						this.name,
						" に スキン ",
						As[i],
						" が存在しません"
					}), null);
				}
			}
		}

		public void validateCsvBone(List<UIPictureBodyData.MosaicPos> As)
		{
			if (As == null)
			{
				return;
			}
			for (int i = 0; i < As.Count; i++)
			{
				if (!this.Spv.existBone(As[i].bone_key))
				{
					X.de(string.Concat(new string[]
					{
						"<BODY-SPINE> ",
						this.name,
						" に ボーン ",
						As[i].bone_key,
						" が存在しません"
					}), null);
				}
			}
		}

		public int first_line;

		private string[] Adefault_skin;

		private float swidth;

		private float sheight;

		private float scale_;

		private SpineViewerNel Spv;

		private string[] Abase_anim;

		private UIEMOT base_emo;

		private int main_loopf = -1000;

		private UIPictureBase.EMSTATE base_state;

		private float timeScale = 1f;

		private const float base_timeScale = 1f;

		private List<UIPictureBodySpine.StateVariation> AAnim;

		private BDic<string, UIPictureBodySpine.StateVariation> OSkin;

		private UIPictureBodySpine.StateVariation[] ATempList;

		private int[] ATempState;

		private string pre_skin;

		private int max_anim_id;

		public float rightshift_px;

		private string[] Aptc_key;

		public int reset_prob_on_same_anim = 100;

		private static List<string> ABufSkin;

		private static List<uint> ABufSkinSt;

		private static float[] Aaf_cache;

		private int af_range_min;

		private int af_range_max;

		private CullMode Cull;

		private struct StateVariation
		{
			public StateVariation(string[] _Aname, SpineViewer Spv, int _loop = -1000)
			{
				this.Aname = _Aname;
				int num = this.Aname.Length;
				if (_loop != -1000)
				{
					for (int i = 0; i < num; i++)
					{
						Spv.setAnmLoopFrame(this.Aname[i], _loop);
					}
				}
				this.alpha = 1f;
				this.anim_id = 0;
				this.enable_skin = "";
				this.reverse_flag = false;
				this.state = UIPictureBase.EMSTATE.NORMAL;
				this.st_add = UIPictureBase.EMSTATE_ADD.NORMAL;
				this.check_or = false;
				this.key_name_ = null;
			}

			public string key_name
			{
				get
				{
					if (this.key_name_ == null)
					{
						this.key_name_ = FEnum<UIPictureBase.EMSTATE>.ToStr(this.state) + "_" + this.anim_id.ToString();
					}
					return this.key_name_;
				}
			}

			public bool checkState(UIPictureBase.EMSTATE st)
			{
				return this.state == UIPictureBase.EMSTATE.NORMAL || (this.check_or ? ((st & this.state) > UIPictureBase.EMSTATE.NORMAL) : ((st & this.state) == this.state)) != this.reverse_flag;
			}

			public bool disableSTA(UIPictureBase.EMSTATE_ADD sta)
			{
				return this.st_add > UIPictureBase.EMSTATE_ADD.NORMAL && (sta & this.st_add) == UIPictureBase.EMSTATE_ADD.NORMAL != this.reverse_flag;
			}

			public float alpha;

			public int anim_id;

			public bool check_or;

			public string[] Aname;

			public string enable_skin;

			public bool reverse_flag;

			public UIPictureBase.EMSTATE state;

			public UIPictureBase.EMSTATE_ADD st_add;

			private string key_name_;
		}
	}
}
