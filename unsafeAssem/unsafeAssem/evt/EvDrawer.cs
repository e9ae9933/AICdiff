using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Better;
using UnityEngine;
using XX;

namespace evt
{
	public class EvDrawer : IHkdsFollowable
	{
		public static uint headKeyToViewType(char s)
		{
			if (s <= 'S')
			{
				if (s <= 'I')
				{
					if (s == '/')
					{
						return 8192U;
					}
					switch (s)
					{
					case 'A':
						return 512U;
					case 'B':
						return 4096U;
					case 'D':
						return 524288U;
					case 'H':
						return 65536U;
					case 'I':
						return 1U;
					}
				}
				else
				{
					if (s == 'N')
					{
						return 256U;
					}
					if (s == 'R')
					{
						return 2097152U;
					}
					if (s == 'S')
					{
						return 1048576U;
					}
				}
			}
			else if (s <= '_')
			{
				if (s == 'W')
				{
					return 262144U;
				}
				if (s == '_')
				{
					return 2048U;
				}
			}
			else
			{
				switch (s)
				{
				case 'f':
					return 1024U;
				case 'g':
					break;
				case 'h':
					return 131072U;
				case 'i':
					return 16384U;
				default:
					if (s == 'r')
					{
						return 32768U;
					}
					if (s == 't')
					{
						return 128U;
					}
					break;
				}
			}
			return 0U;
		}

		public EvDrawer(EvDrawerContainer _DC, string _key, EvDrawerContainer.LAYER _layer)
		{
			this.DC = _DC;
			this.MMRD = ((this.DC != null) ? this.DC.get_MMRD() : null);
			this.key = _key;
			this.layer = _layer;
		}

		public virtual void clearValues(bool clear_position = false)
		{
			this.flip_x = (this.flip_y = false);
			this.vt = (float)(this.vt_max = 0);
			if (clear_position)
			{
				this.base_sx = (this.base_sy = (this.base_dx = (this.base_dy = 0f)));
				this.x = (this.sx = (this.dx = 0f));
				this.y = (this.sy = (this.dy = 0f));
			}
			else
			{
				this.x = this.sx_real;
				this.y = this.sy_real;
			}
			this.z = 1f;
			this.px = (this.py = 0f);
			this.prot = 0f;
			this.pz = (this.palp = 1f);
			this.draw_flag = 0U;
			this.vt_appear = 0;
			if (this.MdImg != null)
			{
				this.MdImg.clear(false, false);
			}
			if (this.MdFill != null)
			{
				this.MdFill.clear(false, false);
			}
			this.releaseFader();
			this.spt = 0f;
			this.sp_movetype = SPMOVE.NONE;
			this.movetype = "ZLINE";
			this.deactivateManpu(true, true);
		}

		public void deactivateManpu(bool immediate = false, bool release_object = false)
		{
			if (this.OMpDr == null)
			{
				return;
			}
			if (release_object)
			{
				immediate = release_object;
			}
			foreach (KeyValuePair<string, ManpuDrawer> keyValuePair in this.OMpDr)
			{
				keyValuePair.Value.deactivate(immediate);
			}
			if (immediate)
			{
				if (release_object)
				{
					this.OMpDr = null;
					return;
				}
				this.OMpDr.Clear();
			}
		}

		public void redrawManpu()
		{
			if (this.OMpDr == null)
			{
				return;
			}
			foreach (KeyValuePair<string, ManpuDrawer> keyValuePair in this.OMpDr)
			{
				keyValuePair.Value.redraw_flag = true;
			}
		}

		public virtual bool isOnRightFor(ManpuDrawer Mp)
		{
			if (this.OMpDr == null)
			{
				return false;
			}
			foreach (KeyValuePair<string, ManpuDrawer> keyValuePair in this.OMpDr)
			{
				Vector2 vector;
				if (keyValuePair.Value == Mp && this.gImg != null && this.gImg.getFacePosition(EV.Pics, out vector, Mp.pos_id, 0f))
				{
					return vector.x > 0f;
				}
			}
			return false;
		}

		public static float getBaseVal(string s)
		{
			if (s != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(s);
				if (num <= 3607893173U)
				{
					if (num <= 3356228888U)
					{
						if (num != 3322673650U)
						{
							if (num != 3339451269U)
							{
								if (num != 3356228888U)
								{
									goto IL_01A4;
								}
								if (!(s == "M"))
								{
									goto IL_01A4;
								}
								goto IL_0198;
							}
							else
							{
								if (!(s == "B"))
								{
									goto IL_01A4;
								}
								goto IL_019E;
							}
						}
						else if (!(s == "C"))
						{
							goto IL_01A4;
						}
					}
					else if (num != 3373006507U)
					{
						if (num != 3507227459U)
						{
							if (num != 3607893173U)
							{
								goto IL_01A4;
							}
							if (!(s == "R"))
							{
								goto IL_01A4;
							}
							goto IL_0192;
						}
						else
						{
							if (!(s == "T"))
							{
								goto IL_01A4;
							}
							goto IL_018C;
						}
					}
					else
					{
						if (!(s == "L"))
						{
							goto IL_01A4;
						}
						goto IL_0186;
					}
				}
				else if (num <= 3893112696U)
				{
					if (num != 3859557458U)
					{
						if (num != 3876335077U)
						{
							if (num != 3893112696U)
							{
								goto IL_01A4;
							}
							if (!(s == "m"))
							{
								goto IL_01A4;
							}
							goto IL_0198;
						}
						else
						{
							if (!(s == "b"))
							{
								goto IL_01A4;
							}
							goto IL_019E;
						}
					}
					else if (!(s == "c"))
					{
						goto IL_01A4;
					}
				}
				else if (num != 3909890315U)
				{
					if (num != 4044111267U)
					{
						if (num != 4144776981U)
						{
							goto IL_01A4;
						}
						if (!(s == "r"))
						{
							goto IL_01A4;
						}
						goto IL_0192;
					}
					else
					{
						if (!(s == "t"))
						{
							goto IL_01A4;
						}
						goto IL_018C;
					}
				}
				else
				{
					if (!(s == "l"))
					{
						goto IL_01A4;
					}
					goto IL_0186;
				}
				return 0f;
				IL_0186:
				return -0.5f;
				IL_018C:
				return 0.5f;
				IL_0192:
				return 0.5f;
				IL_0198:
				return -0.16666667f;
				IL_019E:
				return -0.5f;
			}
			IL_01A4:
			return 0f;
		}

		public EvDrawer initPosition(string dxk, string dyk, string sxk = "", string syk = "", float def_base = 0f)
		{
			if (this.pos_locked)
			{
				X.de("initPosition EvDrawer ::: 場所固定", null);
				return this;
			}
			EvDrawer.initPositionS(dxk, dyk, sxk, syk, out this.base_dx, out this.base_dy, out this.base_sx, out this.base_sy, out this.dx, out this.dy, out this.sx, out this.sy, def_base, this);
			this.draw_flag |= 4U;
			return this;
		}

		public static void initPositionS(string dxk, string dyk, string sxk, string syk, out float base_dx, out float base_dy, out float base_sx, out float base_sy, out float dx, out float dy, out float sx, out float sy, float def_base = 0f, EvDrawer Target = null)
		{
			if (REG.match(dxk, EvDrawer.RegCR))
			{
				base_dx = EvDrawer.getBaseVal(REG.R1);
				dx = 0f;
				dxk = REG.rightContext;
			}
			else if (REG.match(dxk, EvDrawer.RegCRXY))
			{
				string r = REG.R1;
				if (Target != null)
				{
					dx = ((r == "D") ? Target.dx_real : ((r == "S") ? Target.sx_real : ((r == "X") ? Target.x : Target.y)));
				}
				else
				{
					dx = 0f;
				}
				base_dx = 0f;
				dxk = REG.rightContext;
			}
			else
			{
				dx = 0f;
				base_dx = def_base;
			}
			dx += X.Nm(dxk, 0f, true);
			if (REG.match(dyk, EvDrawer.RegCR))
			{
				base_dy = EvDrawer.getBaseVal(REG.R1);
				dy = 0f;
				dyk = REG.rightContext;
			}
			else if (REG.match(dyk, EvDrawer.RegCRXY))
			{
				string r2 = REG.R1;
				if (Target != null)
				{
					dy = ((r2 == "D") ? Target.dy_real : ((r2 == "S") ? Target.sy_real : ((r2 == "X") ? Target.x : Target.y)));
				}
				else
				{
					dy = 0f;
				}
				base_dy = 0f;
				dyk = REG.rightContext;
			}
			else
			{
				dy = 0f;
				base_dy = def_base;
			}
			dy += X.Nm(dyk, 0f, true);
			base_sy = def_base;
			base_sx = def_base;
			if (REG.match(sxk, EvDrawer.RegCR))
			{
				base_sx = EvDrawer.getBaseVal(REG.R1);
				sxk = REG.rightContext;
				sx = 0f;
			}
			else if (REG.match(sxk, EvDrawer.RegCRP) || sxk == "")
			{
				sx = dx;
				base_sx = base_dx;
				sxk = REG.rightContext;
			}
			else
			{
				sx = 0f;
			}
			sx += X.Nm(sxk, 0f, true);
			if (REG.match(syk, EvDrawer.RegCR))
			{
				base_sy = EvDrawer.getBaseVal(REG.R1);
				syk = REG.rightContext;
				sy = 0f;
			}
			else if (REG.match(syk, EvDrawer.RegCRP) || syk == "")
			{
				sy = dy;
				base_sy = base_dy;
				syk = REG.rightContext;
			}
			else
			{
				sy = 0f;
			}
			sy += X.Nm(syk, 0f, true);
		}

		public virtual float sx_real
		{
			get
			{
				return EvDrawer.realX(this.sx, this.base_sx);
			}
		}

		public virtual float dx_real
		{
			get
			{
				return EvDrawer.realX(this.dx, this.base_dx);
			}
		}

		public static float realX(float x, float basex)
		{
			return (float)EV.pw * basex + x;
		}

		public virtual float sy_real
		{
			get
			{
				return EvDrawer.realY(this.sy, this.base_sy);
			}
		}

		public virtual float dy_real
		{
			get
			{
				return EvDrawer.realY(this.dy, this.base_dy);
			}
		}

		public static float realY(float y, float basey)
		{
			return (float)EV.ph * basey + y;
		}

		protected virtual EvDrawer activate(uint view_type = 0U, bool refine_mesh = false)
		{
			bool flag = false;
			if (this.t < 0f)
			{
				this.t = 0f;
				this.mt_max = EvDrawer.MT_NORMAL;
				this.DC.need_sort = true;
				this.clearValues(true);
				flag = true;
			}
			else if ((view_type & 256U) > 0U)
			{
				this.vt = 1f;
				this.vt_max = EvDrawer.VT_NORMALCHANGE;
				this.movetype = "ZSIN2";
			}
			this.showing = true;
			this.gtype &= 4290772991U;
			this.draw_flag |= 12U;
			if ((view_type & 1U) > 0U)
			{
				this.vt = (float)(this.vt_max = 0);
				this.x = this.dx_real;
				this.y = this.dy_real;
				this.t = (float)this.mt_max;
				if (this.Fader != null)
				{
					this.releaseFader();
				}
			}
			if ((view_type & 65536U) != 0U)
			{
				if (this.gImg != null)
				{
					this.y = (this.sy = (this.dy = (float)(-(float)EV.ph / 2) + this.gImg.sheight_px * 0.5f));
					this.base_sy = (this.base_dy = 0f);
				}
				this.gtype |= 67584U | (view_type & 1179648U);
			}
			else if ((view_type & 576512U) > 0U)
			{
				if (!this.pos_locked && flag && (view_type & 32768U) > 0U)
				{
					this.x = (this.sx = (float)(-(float)EV.pw / 2));
					this.y = (this.sy = (float)(-(float)EV.ph / 2));
					this.dx = (float)(EV.pw / 2);
					this.dy = (float)(EV.ph / 2);
					this.base_sx = (this.base_sy = (this.base_dx = (this.base_dy = 0f)));
				}
				if ((view_type & 32768U) > 0U)
				{
					view_type &= 4294966271U;
				}
				if ((view_type & 524288U) > 0U)
				{
					this.DC.DrRad.clear();
				}
				this.gtype |= view_type & 2018304U;
			}
			if ((this.gtype & 262144U) != 0U)
			{
				EV.bg_full_filled = false;
			}
			if (refine_mesh)
			{
				this.draw_flag |= 2U;
				if (this.MdFill == null)
				{
					if ((this.gtype & 558208U) > 0U)
					{
						this.MdFill = this.getMeshDrawer();
						this.MMRD.GetGob(this.MdFill).name = "EvDrawer:" + this.key + " -Fill";
						this.draw_flag |= 16U;
					}
				}
				else
				{
					this.MdFill.clear(false, false);
					if ((this.gtype & 558208U) == 0U)
					{
						this.MMRD.GetGob(this.MdFill).SetActive(false);
						this.MdFill = null;
					}
					this.MaFill = null;
				}
				if (this.MdImg == null)
				{
					if ((this.gtype & 71680U) > 0U)
					{
						this.MdImg = this.getMeshDrawer();
						this.MMRD.GetGob(this.MdImg).name = "EvDrawer:" + this.key + " -Img";
						this.draw_flag |= 16U;
					}
				}
				else
				{
					this.MdImg.clear(false, false);
					if ((this.gtype & 71680U) == 0U)
					{
						this.MMRD.GetGob(this.MdImg).SetActive(false);
						this.MdImg = null;
					}
					this.MaImg = null;
				}
				if ((this.draw_flag & 16U) > 0U)
				{
					this.fineMeshMaterial();
				}
			}
			if ((view_type & 2097152U) != 0U)
			{
				this.fadein(26, 13, false);
			}
			else
			{
				this.sp_movetype = SPMOVE.NONE;
				this.spt = 0f;
			}
			return this;
		}

		public void initSilhouettePic(TalkDrawer.TkDep P, bool set_x = true, bool set_y = false, bool fine_flag = false)
		{
			Vector4 v = P.V4;
			if (set_x)
			{
				this.dx = v.x;
				this.sx = v.z;
				this.base_sx = 0f;
				this.base_dx = 0f;
				if (fine_flag)
				{
					this.x = (this.isActive() ? this.dx : this.sx);
				}
			}
			if (set_y)
			{
				this.dy = v.y;
				this.sy = v.w;
				this.base_sy = 0f;
				this.base_dy = 0f;
				if (fine_flag)
				{
					this.y = (this.isActive() ? this.dy : this.sy);
				}
			}
		}

		public Material getImageMaterial()
		{
			return this.getImageMaterial(this.gtype);
		}

		public virtual Material getImageMaterial(uint gtype)
		{
			if (this.gImg == null)
			{
				return null;
			}
			if ((gtype & 1048576U) != 0U)
			{
				if (this.MtrBuf == null)
				{
					if (this.Fader != null)
					{
						this.MtrBuf = new Material(MTRX.ShaderGDTST);
						this.MtrBuf.SetFloat("_StencilRef", (float)this.stencil_ref);
						this.MtrBuf.SetFloat("_StencilComp", 3f);
					}
					else
					{
						this.MtrBuf = new Material(MTRX.ShaderGDT);
					}
				}
				else if (this.MtrBuf.shader != ((this.Fader != null) ? MTRX.ShaderGDTST : MTRX.ShaderGDT))
				{
					IN.DestroyOne(this.MtrBuf);
					this.MtrBuf = null;
					return this.getImageMaterial(gtype);
				}
				RenderTexture renderTexture = this.PrepareBufferTx(this.gImg.buffer_w, this.gImg.buffer_h, false);
				this.MtrBuf.mainTexture = renderTexture;
				return this.MtrBuf;
			}
			MImage mi = MTRX.getMI(this.gImg.PF);
			if (mi == null)
			{
				return null;
			}
			int num = ((this.Fader != null) ? this.stencil_ref : (-1));
			return mi.getMtr((this.Fader != null) ? BLEND.NORMALST : BLEND.NORMAL, num);
		}

		protected void fineMeshMaterial()
		{
			this.draw_flag &= 4294967279U;
			if (this.MdFill != null)
			{
				Material material = ((this.Fader != null && this.MdImg != null) ? MTRX.getMtr(BLEND.MASK_TRANSPARENT, this.stencil_ref) : MTRX.getMtr(BLEND.NORMAL, -1));
				this.MMRD.setMaterial(this.MdFill, material, false);
				Transform transform = this.MMRD.GetGob(this.MdFill).transform;
				if (this.MdImg != null)
				{
					transform.SetParent(this.MMRD.GetGob(this.MdImg).transform, false);
					transform.localEulerAngles = Vector3.zero;
					transform.localScale = Vector3.one;
					this.fineFillMeshPos(transform);
				}
				else if (transform.parent != this.MMRD.transform)
				{
					transform.SetParent(this.MMRD.transform, true);
					this.draw_flag |= 12U;
					this.DC.need_sort = true;
				}
			}
			if (this.MdImg != null)
			{
				Material material = this.getImageMaterial();
				if (material != null)
				{
					this.MMRD.setMaterial(this.MdImg, material, false);
				}
			}
		}

		protected void fineFillMeshPos(Transform Trs = null)
		{
			if (this.MdImg != null && this.MdFill != null)
			{
				if (Trs == null)
				{
					Trs = this.MMRD.GetGob(this.MdFill).transform;
				}
				Trs.localPosition = new Vector3(this.img_center_shift_x * 0.015625f, this.img_center_shift_y * 0.015625f, 0.1f);
			}
		}

		public virtual MeshDrawer getMeshDrawer()
		{
			if (this.MMRD == null)
			{
				return null;
			}
			int length = this.MMRD.getLength();
			for (int i = this.DC.mmrd_first_count; i < length; i++)
			{
				MeshRenderer meshRenderer = this.MMRD.GetMeshRenderer(i);
				if (!meshRenderer.gameObject.activeSelf)
				{
					meshRenderer.gameObject.SetActive(true);
					meshRenderer.transform.SetParent(this.MMRD.transform, true);
					return this.MMRD.Get(i);
				}
			}
			return this.MMRD.Make(MTRX.ColWhite, BLEND.NORMAL, null, null);
		}

		public uint stringToViewType(string view_key)
		{
			uint num = 0U;
			int length = view_key.Length;
			for (int i = 0; i < length; i++)
			{
				num |= EvDrawer.headKeyToViewType(view_key[i]);
			}
			return num;
		}

		public virtual bool setGrp(string grp, string view_key = "")
		{
			this.releaseFader();
			uint num = this.stringToViewType(view_key);
			if ((num & 512U) == 0U)
			{
				this.gtype = 0U;
			}
			if ((num & 558080U) > 0U)
			{
				num |= 1U;
				uint num2 = TX.str2color(grp, uint.MaxValue);
				this.gcol = num2;
				if ((num & 524288U) == 0U && (this.gcol & 4278190080U) == 4278190080U)
				{
					num |= 262144U;
				}
			}
			else
			{
				if ((num & 16384U) > 0U)
				{
					X.de("GRP_AI 未実装:" + grp, null);
					return false;
				}
				if ((num & 4096U) > 0U && (num & 2097152U) == 0U)
				{
					num |= 1U;
				}
				this.graphic = grp;
				this.gImg = EV.Pics.getPic(grp, true, true);
				if (this.gImg != null)
				{
					if ((num & 8192U) == 0U)
					{
						this.deactivateManpu(true, false);
					}
					if (EV.Pics.isSspBuffer(this.gImg.PF.pPose))
					{
						num |= 1048576U;
					}
					num |= 2048U;
					if (this.MdImg != null && this.MdImg.getMaterial().mainTexture != this.getImageMaterial(num).mainTexture)
					{
						this.releaseMeshDrawer();
					}
				}
			}
			this.activate(num, true);
			return true;
		}

		public void setManpu(string id, string manpu_content, string view_key = "", float face_wpx = -1f, float face_hpx = -1f)
		{
			if (this.MdImg != null)
			{
				this.stringToViewType(view_key);
				if (this.OMpDr == null)
				{
					this.OMpDr = new BDic<string, ManpuDrawer>(1);
				}
				ManpuDrawer manpuDrawer;
				if (!this.OMpDr.TryGetValue(id, out manpuDrawer))
				{
					manpuDrawer = (this.OMpDr[id] = new ManpuDrawer(this, this.DC, id));
				}
				if (face_wpx >= 0f && manpuDrawer.xlen_px != face_wpx)
				{
					manpuDrawer.xlen_px = face_wpx;
					manpuDrawer.finePosition();
				}
				if (face_hpx >= 0f && manpuDrawer.ylen_px != face_hpx)
				{
					manpuDrawer.ylen_px = face_hpx;
					manpuDrawer.finePosition();
				}
				manpuDrawer.activateManpu(this.MMRD.GetGob(this.MdImg).transform, TX.split(manpu_content, "|"), view_key.IndexOf("I") >= 0);
				manpuDrawer.finePosition();
			}
		}

		public virtual bool getFacePosition(out Vector3 Out, string pos_id, float shift_y = 0f)
		{
			Vector2 vector;
			if (this.gImg != null && this.gImg.getFacePosition(EV.Pics, out vector, pos_id, shift_y))
			{
				Out = new Vector3(this.x + vector.x, this.y + vector.y, 1f);
				return true;
			}
			Out = new Vector3(this.x, this.y, 1f);
			return false;
		}

		public bool prepareFader(string anim_pattern, int _default_time = -1, TFKEY tf_key = TFKEY.SD2, bool no_error = false)
		{
			this.releaseFader();
			if ((this.gtype & 32768U) > 0U)
			{
				X.de("RECT には TFADE 未対応 ( FILL を使ってください）", null);
				return false;
			}
			this.releaseFader();
			if (this.vt_max == 0)
			{
				return false;
			}
			bool flag = this.vt_max < 0;
			this.Fader = new EvTransFader(tf_key, (_default_time >= 0) ? ((float)_default_time) : ((this.vt_max > 0) ? ((float)this.vt_max - this.vt) : (-this.vt)), (float)EV.pw, (float)EV.ph, 1f);
			if (!this.Fader.prepare(this, (float)this.vt_max, this.gtype))
			{
				X.de("不明な Fader 定義", null);
				this.releaseFader();
				return false;
			}
			this.gtype |= 128U;
			this.draw_flag |= 16U;
			this.activate(this.gtype, true);
			TFANIM tfanim = TFANIM.WHOLE;
			if (!FEnum<TFANIM>.TryParse(anim_pattern, out tfanim, true))
			{
				X.de("不明な Fader アニメパターン:" + tfanim.ToString(), null);
			}
			this.Fader.resetAnim(tfanim, flag, 0f, 0);
			return true;
		}

		public void releaseFader()
		{
			if (this.Fader == null)
			{
				return;
			}
			this.gtype &= 4294967167U;
			this.Fader = null;
			this.draw_flag |= 24U;
			this.activate(this.gtype, true);
		}

		public virtual EvDrawer prepare()
		{
			this.showing = true;
			this.clearValues(false);
			this.releaseMeshDrawer();
			return this;
		}

		public virtual EvDrawer deactivate()
		{
			if (this.vt_max >= 0)
			{
				this.vt = -40f;
				this.vt_max = (int)this.vt;
			}
			else if (this.vt_max < -40)
			{
				this.vt = (float)((int)(this.vt * (-40f / (float)this.vt_max)));
				this.vt_max = (int)((float)this.vt_max * (-40f / (float)this.vt_max));
			}
			this.vt_appear = this.vt_max;
			this.releaseFader();
			this.movetype = (((this.gtype & 65536U) != 0U) ? "ZSIN" : "Z0");
			this.t = X.Mn(-1f, this.t);
			this.mt_max = X.Abs(this.vt_max);
			this.deactivateManpu(false, false);
			if ((this.gtype & 262144U) != 0U)
			{
				EV.bg_full_filled = false;
			}
			return this;
		}

		public virtual EvDrawer release()
		{
			this.deactivate();
			this.showing = false;
			this.draw_flag = 0U;
			this.releaseFader();
			this.graphic = "";
			this.gtype = 0U;
			this.gcol = 0U;
			this.gImg = null;
			this.ATermTemp = null;
			this.releaseMeshDrawer();
			return null;
		}

		protected virtual void releaseMeshDrawer()
		{
			if (this.MdImg != null)
			{
				this.MdImg.clear(false, false);
				int index = this.MMRD.getIndex(this.MdImg);
				MeshRenderer meshRenderer = this.MMRD.GetMeshRenderer(index);
				this.MMRD.setMaterial(index, null, false);
				meshRenderer.transform.SetParent(this.MMRD.transform);
				meshRenderer.gameObject.SetActive(false);
				this.MdImg = null;
			}
			if (this.MdFill != null)
			{
				this.MdFill.clear(false, false);
				this.MMRD.GetGob(this.MdFill).SetActive(false);
				this.MdFill = null;
			}
			if (this.TxBuf != null)
			{
				BLIT.nDispose(this.TxBuf);
				this.TxBuf = null;
			}
			if (this.MtrBuf != null)
			{
				IN.DestroyOne(this.MtrBuf);
				this.MtrBuf = null;
			}
			this.deactivateManpu(true, true);
			this.clearMeshArranger();
			this.draw_flag |= 8U;
		}

		public void clearMeshArranger()
		{
			this.MaFill = (this.MaImg = null);
		}

		public virtual bool runDraw(float fcnt, bool deleting = false)
		{
			if (this.vt_max > 0)
			{
				if (this.vt < (float)this.vt_max)
				{
					this.vt = X.Mn(this.vt + fcnt, (float)this.vt_max);
					if (this.Fader == null)
					{
						this.draw_flag |= 1U;
					}
				}
			}
			else if (this.vt_max < 0 && this.vt < 0f)
			{
				this.vt = X.Mn(this.vt + fcnt, 0f);
				if (this.Fader == null)
				{
					this.draw_flag |= 1U;
				}
			}
			if (this.Fader != null)
			{
				if (this.Fader.isFinished())
				{
					if (this.vt_max > 0)
					{
						this.releaseFader();
						this.vt = (float)this.vt_max;
					}
				}
				else
				{
					this.draw_flag |= 32U;
				}
			}
			if (this.t >= 0f)
			{
				if (this.t <= (float)this.mt_max)
				{
					this.t += fcnt;
					float num = this.t / (float)this.mt_max;
					this.x = this.get_move_val(this.sx_real, this.dx_real, num);
					this.y = this.get_move_val(this.sy_real, this.dy_real, num);
					this.draw_flag |= 4U;
				}
				else
				{
					this.t += fcnt;
				}
			}
			else if (this.t > (float)(-(float)this.mt_max))
			{
				this.t -= fcnt;
				this.draw_flag |= 4U;
				float num = -this.t / (float)this.mt_max;
				this.x = this.get_move_val(this.dx_real, this.sx_real, num);
				this.y = this.get_move_val(this.dy_real, this.sy_real, num);
			}
			else
			{
				if (deleting)
				{
					if (this.showing)
					{
						this.release();
					}
					return false;
				}
				this.releaseMeshDrawer();
				this.vt = 0f;
				return true;
			}
			if (X.D)
			{
				this.calc_sp_move((float)X.AF);
			}
			if ((this.gtype & 524288U) != 0U && this.DC.DrRad.needDraw((float)IN.totalframe))
			{
				this.draw_flag |= 2U;
			}
			if ((X.D || (this.draw_flag & 8U) > 0U) && this.draw_flag != 0U)
			{
				bool flag = false;
				bool flag2 = false;
				if ((this.draw_flag & 4U) > 0U)
				{
					flag = true;
					this.repositMesh(-1000f);
				}
				if ((this.draw_flag & 1U) > 0U)
				{
					if (this.MdFill != null && (this.Fader != null || this.MaFill == null))
					{
						this.draw_flag |= 2U;
					}
					if (this.MdImg != null && (this.Fader != null || this.MaImg == null))
					{
						this.draw_flag |= 2U;
					}
				}
				if ((this.draw_flag & 32U) > 0U && (this.draw_flag & 2U) == 0U)
				{
					if (this.fadeDrawPrepare(fcnt))
					{
						this.releaseFader();
						this.draw_flag |= 2U;
					}
					else
					{
						flag2 = (flag = true);
					}
				}
				if ((this.draw_flag & 2U) > 0U)
				{
					flag2 = (flag = true);
					this.redrawMesh(fcnt);
				}
				else if ((this.draw_flag & 32U) <= 0U && (this.draw_flag & 1U) > 0U)
				{
					flag2 = (flag = true);
					this.setDrawnAlpha(-1f);
				}
				if (flag2)
				{
					if (this.MdFill != null)
					{
						this.MdFill.updateForMeshRenderer(true);
					}
					if (this.MdImg != null)
					{
						this.MdImg.updateForMeshRenderer(true);
					}
				}
				if (flag)
				{
					this.draw_flag = 0U;
				}
			}
			if (this.OMpDr != null)
			{
				foreach (KeyValuePair<string, ManpuDrawer> keyValuePair in this.OMpDr)
				{
					keyValuePair.Value.runDraw();
				}
			}
			return true;
		}

		protected float get_move_val(float v1, float v2, float tz)
		{
			string movetype = this.movetype;
			if (movetype != null && movetype == "Z")
			{
				tz = 1f;
			}
			else
			{
				try
				{
					if (this.FnMoveType == null)
					{
						this.FnMoveType = X.getFnZoom(this.movetype);
					}
					tz = this.FnMoveType(tz);
				}
				catch
				{
					X.de("不明な移動関数: " + this.movetype, null);
					this.movetype = "Z";
					return 1f;
				}
			}
			return X.NAIBUN_I(v1, v2, tz);
		}

		public MeshDrawer getBufferStaticMesh()
		{
			if (EvDrawer.BufMd == null)
			{
				EvDrawer.BufMd = new MeshDrawer(null, 4, 6);
				EvDrawer.BufMd.draw_gl_only = true;
			}
			return EvDrawer.BufMd;
		}

		public RenderTexture PrepareBufferTx(int w, int h, bool force_clear = false)
		{
			if (force_clear || this.TxBuf == null || this.TxBuf.width != w || this.TxBuf.height != h)
			{
				BLIT.Alloc(ref this.TxBuf, w, h, true, RenderTextureFormat.ARGB32, 0);
			}
			return this.TxBuf;
		}

		public virtual void addTemporaryTerm(string check_expression, string repl, string term)
		{
			if (this.ATermTemp == null)
			{
				this.ATermTemp = new List<EvPerson.EmotReplaceTerm>(1);
			}
			this.ATermTemp.Add(new EvPerson.EmotReplaceTerm(null, check_expression, repl, term));
		}

		public virtual bool checkTempTermReplace(ref string s)
		{
			return EvPerson.EmotReplaceTerm.checkTermReplace(this.ATermTemp, ref s);
		}

		public EvDrawer fadein(int maxt, int st = 0, bool do_activate = true)
		{
			if (do_activate)
			{
				this.activate(0U, false);
			}
			this.vt = (float)st;
			this.vt_max = maxt;
			this.vt_appear = 0;
			this.gtype &= 4290772991U;
			this.releaseFader();
			if ((this.gtype & 262144U) != 0U)
			{
				EV.bg_full_filled = false;
			}
			return this;
		}

		public EvDrawer fadeout(int maxt)
		{
			this.gtype &= 4290772991U;
			if (this.t >= 0f)
			{
				if (maxt <= 0)
				{
					if (this.vt != 0f || this.vt_max != -1)
					{
						this.vt = 0f;
						this.vt_max = -1;
						this.draw_flag |= 1U;
					}
				}
				else
				{
					this.vt = (float)(this.vt_max = -maxt);
				}
			}
			this.vt_appear = this.vt_max;
			if ((this.gtype & 262144U) != 0U)
			{
				EV.bg_full_filled = false;
			}
			this.releaseFader();
			return this;
		}

		public EvDrawer setFlash(int appear_time, int showing_time, int fade_time)
		{
			showing_time = X.MMX(0, showing_time, 255);
			appear_time = X.MMX(0, appear_time, 255);
			this.vt_appear = (appear_time << 8) | showing_time;
			this.vt_max = -fade_time;
			this.vt = (float)(this.vt_max - showing_time - appear_time);
			this.releaseFader();
			this.gtype |= 4194304U;
			return this;
		}

		public void flip(bool _x = false, bool _y = false, string view_key = "")
		{
			this.flip_x = _x;
			this.flip_y = _y;
			this.draw_flag |= 4U;
			uint num = this.stringToViewType(view_key);
			if (num != 0U)
			{
				this.activate(num, false);
			}
		}

		public virtual EvDrawer fine(bool fine_alpha = true, bool fine_move = true, bool fine_movea = true)
		{
			if (fine_alpha)
			{
				this.gtype &= 4290772991U;
				if (this.vt_max < 0)
				{
					this.vt = 0f;
				}
				if (this.vt_max > 0)
				{
					this.vt = (float)this.vt_max;
				}
				if (this.Fader != null)
				{
					this.draw_flag |= 2U;
					this.releaseFader();
				}
				else
				{
					this.draw_flag |= 1U;
				}
			}
			if (fine_move)
			{
				if (this.t < 0f)
				{
					this.t = (float)(-(float)this.mt_max);
				}
				else
				{
					this.t = (float)this.mt_max;
				}
				this.x = ((this.t >= 0f) ? this.dx_real : this.sx_real);
				this.y = ((this.t >= 0f) ? this.dy_real : this.sy_real);
				this.draw_flag |= 4U;
			}
			if (fine_movea && this.spt >= 0f)
			{
				this.spt = 0f;
			}
			return this;
		}

		public EvDrawer moveTo(string xstr, string ystr, int maxt, string _movetype = "")
		{
			return this.moveTo("C" + this.x.ToString(), "C" + this.y.ToString(), xstr, ystr, maxt, _movetype);
		}

		public virtual EvDrawer moveTo(string sxstr, string systr, string dxstr, string dystr, int maxt, string _movetype = "")
		{
			if (this.pos_locked)
			{
				X.de("moveTo EvDrawer ::: 場所固定", null);
				return this;
			}
			this.initPosition(dxstr, dystr, sxstr, systr, 0f);
			this.initMove(maxt);
			if (_movetype != "")
			{
				this.movetype = _movetype;
			}
			return this;
		}

		public void initMove(int maxt)
		{
			this.t = ((this.t >= 0f) ? 0f : this.t);
			this.mt_max = maxt;
			if (maxt <= 0)
			{
				this.mt_max = 1;
				this.fine(false, true, false);
			}
		}

		public EvDrawer moveA(string key, int time = 20)
		{
			this.sp_movetype = SpMove.Get(key);
			if (this.sp_movetype != SPMOVE.NONE)
			{
				this.af2 = 0f;
				this.spt = (float)time;
			}
			else
			{
				this.spt = 0f;
			}
			return this;
		}

		public virtual void repositMesh(float z = -1000f)
		{
			float num = 0f;
			bool flag = false;
			if (this.flip_x || this.flip_y)
			{
				if (this.flip_x && this.flip_y)
				{
					num = 3.1415927f;
				}
				else
				{
					if (this.flip_y)
					{
						num = 3.1415927f;
					}
					flag = true;
				}
			}
			num = (num + this.prot) * 180f / 3.1415927f;
			for (int i = 0; i < 2; i++)
			{
				MeshDrawer meshDrawer = ((i == 0) ? this.MdFill : this.MdImg);
				float num2 = 0f;
				float num3 = 0f;
				if (meshDrawer != null)
				{
					if (i == 0)
					{
						if (this.MdImg != null)
						{
							goto IL_00D3;
						}
						num2 = this.img_center_shift_x;
						num3 = this.img_center_shift_y;
					}
					this.repositMesh(this.MMRD.GetGob(meshDrawer).transform, (z > -1000f) ? (z - (float)i * 1E-05f) : (-1000f), num, flag ? 1 : 0, num2, num3);
				}
				IL_00D3:;
			}
			if (this.OMpDr != null)
			{
				foreach (KeyValuePair<string, ManpuDrawer> keyValuePair in this.OMpDr)
				{
					keyValuePair.Value.finePosition();
				}
			}
		}

		public void repositMesh(Transform Trs, float z, float rot = 0f, int flip = -1, float shiftx = 0f, float shifty = 0f)
		{
			Vector3 localPosition = Trs.localPosition;
			if (z > -1000f)
			{
				localPosition.z = z;
				Trs.localPosition = localPosition;
				return;
			}
			if (flip == -1)
			{
				flip = 0;
				if (this.flip_x || this.flip_y)
				{
					if (this.flip_x && this.flip_y)
					{
						rot = 3.1415927f;
					}
					else
					{
						if (this.flip_y)
						{
							rot = 3.1415927f;
						}
						flip = 1;
					}
					rot = (rot + this.prot) * 180f / 3.1415927f;
				}
			}
			localPosition.x = (this.x + this.px + shiftx) * 0.015625f;
			localPosition.y = (this.y + this.py + shifty) * 0.015625f;
			Trs.localPosition = localPosition;
			Trs.localEulerAngles = new Vector3(0f, 0f, rot);
			Trs.localScale = new Vector3((float)((flip == 1) ? (-1) : 1), 1f, 1f);
		}

		public virtual void redrawMesh(float fcnt)
		{
			float drawAlpha = this.getDrawAlpha();
			if (drawAlpha <= 0f)
			{
				return;
			}
			bool flag = this.fadeDrawPrepare(fcnt);
			int num = -1;
			int num2 = 0;
			if ((this.gtype & 1024U) > 0U && this.MdFill != null && flag)
			{
				if (this.MaFill == null)
				{
					this.MaFill = new MdArranger(this.MdFill).Set(true);
				}
				else
				{
					this.MaFill.revertVerAndTriIndex(ref num, ref num2);
				}
				this.MdFill.Col = this.MdFill.ColGrd.Set(this.gcol).mulA(drawAlpha).C;
				this.MdFill.Rect(0f, 0f, (float)EV.pw, (float)EV.ph, false);
				if (num >= 0)
				{
					this.MaFill.revertVerAndTriIndexAfter(num, num2, false);
				}
				else
				{
					this.MaFill.Set(false);
				}
			}
			if ((this.gtype & 524288U) > 0U && this.MdFill != null && flag)
			{
				if (this.MaFill == null)
				{
					this.MaFill = new MdArranger(this.MdFill).Set(true);
				}
				else
				{
					this.MaFill.revertVerAndTriIndex(ref num, ref num2);
				}
				this.MdFill.Col = this.MdFill.ColGrd.Set(this.gcol).mulA(drawAlpha).C;
				this.DC.DrRad.drawTo(this.MdFill, 0f, 0f, (float)EV.pw, (float)EV.ph, 2f, (float)IN.totalframe, false, 1f);
				if (num >= 0)
				{
					this.MaFill.revertVerAndTriIndexAfter(num, num2, false);
				}
				else
				{
					this.MaFill.Set(false);
				}
			}
			if ((this.gtype & 16384U) <= 0U)
			{
				if ((this.gtype & 67584U) > 0U)
				{
					this.drawGrp(fcnt, this.gImg, drawAlpha);
					return;
				}
				if ((this.gtype & 32768U) > 0U && (this.gtype & 1024U) == 0U && this.pz != 0f && this.MdFill != null)
				{
					num = 0;
					if (this.MaFill == null)
					{
						this.MaFill = new MdArranger(this.MdFill).Set(true);
					}
					else
					{
						this.MaFill.revertVerAndTriIndex(ref num, ref num2);
					}
					this.MdFill.Col = this.MdFill.ColGrd.Set(this.gcol).mulA(drawAlpha).C;
					this.MdFill.Box(this.sx_real - this.x, this.sy_real - this.y, this.dx_real - this.x, this.dy_real - this.y, this.z * this.pz, false);
					if (num >= 0)
					{
						this.MaFill.revertVerAndTriIndexAfter(num, num2, false);
						return;
					}
					this.MaFill.Set(false);
				}
			}
		}

		public void drawGrp(float fcnt, EvImg Grp, float alp1 = -1f)
		{
			if (alp1 < 0f)
			{
				alp1 = this.getDrawAlpha();
			}
			if (alp1 <= 0f || this.pz == 0f || this.MdImg == null)
			{
				return;
			}
			int num = -1;
			int num2 = 0;
			if (this.MaImg == null)
			{
				this.MaImg = new MdArranger(this.MdImg).Set(true);
			}
			else
			{
				this.MaImg.revertVerAndTriIndex(ref num, ref num2);
			}
			this.MdImg.ColGrd.White().setA1(alp1);
			this.MdImg.Col = this.MdImg.ColGrd.C;
			if ((this.gtype & 1048576U) != 0U)
			{
				MeshDrawer bufferStaticMesh = this.getBufferStaticMesh();
				RenderTexture renderTexture = this.PrepareBufferTx(Grp.buffer_w, Grp.buffer_h, true);
				Material mtr = MTRX.getMI(Grp.PF.pChar, false).getMtr(BLEND.NORMAL, -1);
				bufferStaticMesh.clear(false, false);
				bufferStaticMesh.setMaterial(mtr, false);
				bufferStaticMesh.RotaPF(0f, 0f, 1f, 1f, 0f, Grp.PF, false, false, false, uint.MaxValue, false, 0);
				Graphics.SetRenderTarget(renderTexture);
				BLIT.RenderToGLMtr(bufferStaticMesh, 0f, 0f, 1f, mtr, BLIT.getMatrixForImage(renderTexture, 1f), bufferStaticMesh.getTriMax(), false, false);
				Graphics.SetRenderTarget(null);
				this.MdImg.initForImg(renderTexture);
				this.MdImg.Rect(0f, 0f, (float)renderTexture.width, (float)renderTexture.height, false);
			}
			else
			{
				float num3 = (((this.gtype & 131072U) != 0U) ? 0.5f : 1f);
				this.MdImg.RotaPF(0f, 0f, num3, num3, 0f, Grp.PF, false, false, false, uint.MaxValue, false, 0);
			}
			if (num >= 0)
			{
				this.MaImg.revertVerAndTriIndexAfter(num, num2, false);
				return;
			}
			this.MaImg.Set(false);
		}

		public virtual void setDrawnAlpha(float alp1 = -1f)
		{
			if (alp1 < 0f)
			{
				alp1 = this.getDrawAlpha();
			}
			if (this.MaFill != null && this.Fader == null)
			{
				if ((this.gtype & 525312U) != 0U)
				{
					alp1 *= (float)C32.getAlpha(this.gcol) / 255f;
				}
				this.MaFill.setAlpha1(alp1, false);
			}
			if (this.MaImg != null)
			{
				this.MaImg.setAlpha1(alp1, false);
			}
		}

		public bool fadeDrawPrepare(float fcnt)
		{
			if (this.Fader != null)
			{
				this.MdFill.revertVerAndTriIndex(0, 0, false);
				bool flag = this.Fader.fadeDrawPrepare(this.MdImg, this.MdFill, fcnt);
				if (!flag)
				{
					if (this.vt_max > 0)
					{
						this.vt = (float)this.vt_max;
					}
					this.MaFill = null;
				}
				return flag;
			}
			return true;
		}

		public float getDrawAlpha()
		{
			if (this.t < 0f && this.t <= (float)(-(float)this.mt_max))
			{
				return 0f;
			}
			if (this.Fader != null)
			{
				if (this.Fader.isFinished())
				{
					return (float)(this.Fader.isReverse() ? 0 : 1);
				}
				return this.palp;
			}
			else if ((this.gtype & 4194304U) != 0U)
			{
				int num = (this.vt_appear >> 8) & 255;
				int num2 = this.vt_appear & 255;
				if (this.vt < (float)(-(float)num2 + this.vt_max))
				{
					return this.palp * (1f - X.ZLINE((float)(-(float)num2 + this.vt_max) - this.vt, (float)num));
				}
				if (this.vt < (float)this.vt_max)
				{
					return this.palp;
				}
				return X.ZLINE(-this.vt, (float)(-(float)this.vt_max)) * this.palp;
			}
			else
			{
				if (this.vt_max > 0)
				{
					return X.ZLINE(this.vt, (float)this.vt_max) * this.palp;
				}
				if (this.vt_max >= 0)
				{
					return this.palp;
				}
				if (this.vt < (float)this.vt_appear)
				{
					return 0f;
				}
				return X.ZLINE(-this.vt, (float)(-(float)this.vt_max)) * this.palp;
			}
		}

		private void calc_sp_move(float fcnt = 1f)
		{
			float num = this.px;
			float num2 = this.py;
			float num3 = this.pz;
			float num4 = this.prot;
			float num5 = this.palp;
			this.px = (this.py = (this.prot = 0f));
			this.pz = 1f;
			this.palp = 1f;
			if (this.spt != 0f)
			{
				switch (this.sp_movetype)
				{
				case SPMOVE.HOP:
				{
					int num6 = 15;
					this.py += (-X.Pow(this.af2 - 7.5f, 2) + 64f) * 0.25f;
					if (this.af2 >= (float)num6)
					{
						this.af2 = 0f;
						if (this.spt > 0f)
						{
							this.spt -= 1f;
						}
					}
					break;
				}
				case SPMOVE.SCARY:
				{
					float num7 = X.randA((uint)(this.af2 + 40f)) % 256U / 256f * 5f;
					float num8 = X.randA((uint)(this.af2 + 50f)) % 170U / 170f * 6.2831855f;
					this.px += num7 * X.Cos(num8 * 1.2f);
					this.py += num7 * X.Sin(num8);
					if (this.spt > 0f)
					{
						this.spt = X.VALWALK(this.spt, 0f, fcnt);
					}
					break;
				}
				case SPMOVE.FLY:
					this.py += -7f * X.Sin(this.af2 / 45f * 6.2831855f);
					if (this.spt > 0f)
					{
						this.spt = X.VALWALK(this.spt, 0f, fcnt);
					}
					break;
				case SPMOVE.CAR:
				{
					int num6 = 39;
					float num9 = this.af2 % (float)num6;
					if (num9 < 1f)
					{
						this.py += 5f;
					}
					else if (num9 < 2f)
					{
						this.py += -5f;
					}
					else
					{
						this.py += 3f * (1f - 0.6f * X.ZSIN(num9, (float)num6)) * X.Cos(num9 / 8.7f * 6.2831855f);
					}
					if (this.spt > 0f)
					{
						this.spt = X.VALWALK(this.spt, 0f, fcnt);
					}
					break;
				}
				case SPMOVE.SCARY2:
				{
					float num7 = X.randA((uint)(this.af2 / 2f + 13f)) % 256U / 256f * 15f;
					float num8 = X.randA((uint)(this.af2 / 2f + 45f)) % 170U / 170f * 6.2831855f;
					this.px += num7 * 1.4f * X.Cos(num8 * 1.138f);
					this.py += num7 * X.Sin(num8);
					if (this.spt > 0f)
					{
						this.spt = X.VALWALK(this.spt, 0f, fcnt);
					}
					break;
				}
				case SPMOVE.ANGER:
				{
					if (this.af2 >= 150f)
					{
						this.af2 -= 150f;
						if (this.spt > 0f)
						{
							this.spt -= 1f;
						}
					}
					float num10;
					if (this.af2 >= 99f)
					{
						num10 = this.af2 - 99f;
					}
					else if (this.af2 >= 90f)
					{
						num10 = this.af2 - 90f;
					}
					else
					{
						num10 = this.af2;
					}
					float num11 = X.Mx(5f - num10, 0f) / 5f;
					if (num11 > 0f)
					{
						this.px += (float)(18446744073709551614UL + (ulong)(X.randA((uint)IN.totalframe) % 5U));
						this.py += (float)(18446744073709551614UL + (ulong)(X.randA((uint)IN.totalframe) % 5U));
					}
					this.pz *= 1f + 0.5f * num11;
					this.prot = 0.09f * num11;
					break;
				}
				case SPMOVE.LOVELY:
					if (this.spt > 0f)
					{
						this.spt = X.VALWALK(this.spt, 0f, fcnt);
					}
					this.prot = X.Sin((float)IN.totalframe / 46f * 6.2831855f) * 16f / 360f * 6.2831855f;
					break;
				case SPMOVE.BLINK:
				case SPMOVE.BLINK2:
				{
					int num6 = 30;
					if (this.af2 >= (float)(num6 * 2))
					{
						this.af2 = 0f;
						if (this.spt > 0f)
						{
							this.spt -= 1f;
						}
					}
					else if (this.sp_movetype == SPMOVE.BLINK)
					{
						if (this.af2 >= (float)num6)
						{
							this.palp = 0f;
						}
					}
					else
					{
						this.palp = 0.55f + 0.45f * X.COSI(this.af2, (float)(num6 * 2));
					}
					break;
				}
				case SPMOVE.JUMP:
				case SPMOVE.JUMPB:
				{
					int num6 = 12 + ((this.spt == 1f) ? 1 : 0) * 23;
					int num12 = 6;
					float num13 = 30f;
					float num14 = (float)X.MPF(SPMOVE.JUMP == this.sp_movetype);
					if (this.af2 >= (float)num6)
					{
						this.af2 = 0f;
						if (this.spt > 0f)
						{
							this.spt -= 1f;
						}
					}
					else if (this.af2 < (float)num12)
					{
						this.py += num14 * num13 * X.ZSIN(this.af2, (float)num12);
					}
					else
					{
						float num9 = this.af2 - (float)num12;
						int num15 = num6 - num12;
						float num16 = num9 / (float)num15;
						this.py += num14 * num13 * X.PARABOLA(1f, 0f, 0f, 1f, num16);
					}
					break;
				}
				case SPMOVE.WEEKHITL:
				case SPMOVE.WEEKHITR:
				{
					int num6 = X.Mx(2, (int)this.spt);
					if (this.af2 >= (float)num6)
					{
						this.af2 = 0f;
						this.spt = 0f;
					}
					else
					{
						float num16 = X.ZLINE(this.af2, (float)num6);
						int num17 = (int)((float)(X.MPF(SPMOVE.WEEKHITR == this.sp_movetype) * 15) * X.PARABOLA(0.5f, 1f, 0f, 0f, num16));
						this.px += (float)num17;
					}
					break;
				}
				case SPMOVE.SHAKE:
				{
					float num9 = this.af2 - (19f + this.spt);
					int num18 = 19;
					if (num9 < 0f)
					{
						this.px -= (float)num18 * X.ZSIN(this.af2, 19f);
					}
					else
					{
						float num19 = num9 - (28f + this.spt);
						if (num19 < 0f)
						{
							this.px += (float)(-(float)num18) + (float)(num18 * 2) * X.ZSIN(num9, 28f);
						}
						else
						{
							this.px += (float)num18 - (float)num18 * X.ZSIN(num19, 19f);
							if (num19 >= 20f)
							{
								this.af2 = 0f;
								this.spt = 0f;
							}
						}
					}
					break;
				}
				case SPMOVE.ZOOM2:
				case SPMOVE.ZOOM3:
				case SPMOVE.ZOOM4:
				{
					float num20 = (float)((this.sp_movetype == SPMOVE.ZOOM2) ? 1 : ((this.sp_movetype == SPMOVE.ZOOM3) ? 2 : ((this.sp_movetype == SPMOVE.ZOOM4) ? 3 : 0)));
					if (this.spt > 0f)
					{
						this.pz = 1f + num20 * X.ZLINE(this.af2, this.spt);
					}
					else
					{
						this.pz = 1f + num20;
					}
					break;
				}
				case SPMOVE.ALP50:
					this.palp = 0.5f;
					if (this.spt > 0f)
					{
						this.spt = X.VALWALK(this.spt, 0f, fcnt);
					}
					break;
				case SPMOVE.DANCE:
				{
					if (this.spt > 0f)
					{
						this.spt = X.VALWALK(this.spt, 0f, fcnt);
					}
					float num21 = X.ANMP((int)this.af2, 50, 1f);
					float num14 = (float)X.MPF((int)(this.af2 / 50f) % 2 == 1);
					float num16 = ((this.spt >= 0f) ? X.ZSIN(this.spt, 60f) : 1f);
					float num22 = ((this.af2 < 50f) ? (-X.ZSINV(num21) * 0.5f) : (num14 * (X.ZSINV(num21) - 0.5f)));
					this.prot = num22 * 11f / 360f * 6.2831855f * num16;
					this.px -= num22 * 30f * num16;
					this.py += X.Abs(X.Sin(this.af2 / 50f * 3.1415927f)) * 11f * num16;
					break;
				}
				case SPMOVE.SIN_H:
				case SPMOVE.SIN_V:
				case SPMOVE.SIN_H2:
				case SPMOVE.SIN_V2:
				{
					bool flag = this.sp_movetype == SPMOVE.SIN_H2 || this.sp_movetype == SPMOVE.SIN_V2;
					float num23 = 10f * X.SINI(this.af2, (float)(flag ? 40 : 120));
					if (this.spt > 0f)
					{
						this.spt = X.VALWALK(this.spt, 0f, fcnt);
						num23 *= X.ZLINE(this.spt, 20f);
						if (flag)
						{
							num23 *= 1.55f;
						}
					}
					if (this.sp_movetype == SPMOVE.SIN_H || this.sp_movetype == SPMOVE.SIN_H2)
					{
						this.px += num23;
					}
					else
					{
						this.py += num23;
					}
					break;
				}
				case SPMOVE.HANDSHAKE:
				{
					float num16 = 1f;
					if (this.spt > 0f)
					{
						this.spt = X.VALWALK(this.spt, 0f, fcnt);
						num16 = X.ZLINE(this.spt, 80f);
					}
					this.px += (float)X.IntR(X.SINI(this.af2, 380f) * 16f * num16);
					this.py += (float)X.IntR(X.SINI(this.af2 + 90f, 493f) * 16f * num16);
					break;
				}
				}
				this.af2 += (float)X.AF;
			}
			if (num != this.px || num2 != this.py || num3 != this.pz || num4 != this.prot)
			{
				this.draw_flag |= 4U;
			}
			if (num5 != this.palp)
			{
				this.draw_flag |= 1U;
			}
		}

		public int isFilledWholeScreen()
		{
			if ((this.gtype & 262144U) == 0U || this.vt_max < 0)
			{
				return 0;
			}
			if (!((this.Fader != null) ? this.Fader.isFinished() : (this.vt >= (float)this.vt_max)))
			{
				return 2;
			}
			return 1;
		}

		public string get_key()
		{
			return this.key;
		}

		public void rewriteKeyAndLayer(string _key)
		{
			this.key = _key;
			this.layer = EvDrawerContainer.keyToLayer(_key);
		}

		public virtual int id_in_layer
		{
			get
			{
				return X.NmI(TX.slice(this.key, 1), -1, true, false);
			}
			set
			{
			}
		}

		public float get_x()
		{
			return this.x;
		}

		public float get_y()
		{
			return this.y;
		}

		public float get_z()
		{
			return this.z;
		}

		public uint get_gcol()
		{
			return this.gcol;
		}

		public virtual Vector3 getHkdsDepertPos()
		{
			return new Vector3(this.x, this.y, 1f);
		}

		public bool checkPositionMoved(IMessageContainer Msg)
		{
			if (this.t < 0f)
			{
				return this.t > (float)(-(float)this.mt_max);
			}
			return this.t <= (float)this.mt_max;
		}

		public bool isActive()
		{
			return this.t >= 0f;
		}

		public bool isCompletelyHidden()
		{
			return this.t <= (float)(-(float)this.mt_max);
		}

		public int stencil_ref
		{
			get
			{
				if (this.Fader != null)
				{
					return (int)(70 + this.id_in_layer + this.layer * (EvDrawerContainer.LAYER)8);
				}
				return -1;
			}
		}

		public void changeMMRDMaterial(MeshDrawer Md, Material M)
		{
			this.MMRD.setMaterial(Md, M, false);
		}

		public EvDrawer set_z(float _z)
		{
			this.z = _z;
			return this;
		}

		public string movetype
		{
			get
			{
				return this.movetype_;
			}
			set
			{
				this.movetype_ = value;
				this.FnMoveType = null;
			}
		}

		public EvImg getDrawImage()
		{
			if ((this.gtype & 2048U) <= 0U)
			{
				return null;
			}
			return this.gImg;
		}

		protected EvDrawerContainer DC;

		protected MultiMeshRenderer MMRD;

		protected string key;

		protected float sx;

		protected float sy;

		protected float dx;

		protected float dy;

		public const float BASE_C = 0f;

		public const float BASE_L = -0.5f;

		public const float BASE_T = 0.5f;

		public const float BASE_R = 0.5f;

		public const float BASE_B = -0.5f;

		public EvDrawerContainer.LAYER layer;

		protected float base_sx;

		protected float base_sy;

		protected float base_dx;

		protected float base_dy;

		protected bool pos_locked;

		private bool showing;

		protected float x;

		protected float y;

		protected float z = 1f;

		protected static readonly int MT_NORMAL = 18;

		public static readonly int VT_NORMALCHANGE = 18;

		protected float t = (float)(-(float)EvDrawer.MT_NORMAL);

		protected int mt_max = 1;

		private float vt;

		private int vt_max;

		private int vt_appear;

		protected bool flip_x;

		protected bool flip_y;

		protected MeshDrawer MdImg;

		protected MdArranger MaImg;

		protected MeshDrawer MdFill;

		protected MdArranger MaFill;

		protected EvTransFader Fader;

		private string movetype_ = "ZSIN";

		private FnZoom FnMoveType;

		private SPMOVE sp_movetype;

		private float spt;

		private float af2;

		private EvImg gImg;

		private List<EvPerson.EmotReplaceTerm> ATermTemp;

		private string graphic = "";

		private uint gtype;

		private uint gcol;

		protected float px;

		protected float py;

		protected float pz = 1f;

		protected float palp = 1f;

		protected float prot = 1f;

		public const uint GRP_ADDITION = 512U;

		public const uint GRP_FILL = 1024U;

		public const uint GRP_NORMAL = 2048U;

		public const uint GRP_BG = 4096U;

		public const uint GRP_MANPU_NOT_CLEAR = 8192U;

		public const uint GRP_AI = 16384U;

		public const uint GRP_RECT = 32768U;

		public const uint GRP_SILHOUETTE = 65536U;

		public const uint GRP_HALF_SCALE = 131072U;

		public const uint GRP_WHOLE_FILL = 262144U;

		public const uint GRP_RADIATION_LINE = 524288U;

		public const uint GRP_FLASH = 4194304U;

		public const uint GRP_REPLACE = 2097152U;

		public const uint GRP_SSP = 1048576U;

		public const uint GRP__USE_IMAGE = 71680U;

		public const uint GRP__USE_FILL = 558208U;

		public const uint VIEWTYPE_IMMEDIATE = 1U;

		public const uint VIEWTYPE_USEFADER = 128U;

		public const uint VIEWTYPE_NORMALCHANGE = 256U;

		public static readonly Regex RegCR = new Regex("^([LTRBCM])\\+?");

		public static readonly Regex RegCRP = new Regex("^P\\+?");

		public static readonly Regex RegCRXY = new Regex("^([XYSD])\\+?");

		public uint draw_flag;

		public const uint DRAWF_CHANGED_ALPHA = 1U;

		public const uint DRAWF_REDRAW = 2U;

		public const uint DRAWF_POS = 4U;

		public const uint DRAWF_IMMEDIATE = 8U;

		public const uint DRAWF_FINEMATERIAL = 16U;

		public const uint DRAWF_ONLY_FADE = 32U;

		public const float bottom_eyeline_y = 240f;

		public float img_center_shift_x;

		public float img_center_shift_y;

		private RenderTexture TxBuf;

		private Material MtrBuf;

		private static MeshDrawer BufMd;

		private BDic<string, ManpuDrawer> OMpDr;
	}
}
