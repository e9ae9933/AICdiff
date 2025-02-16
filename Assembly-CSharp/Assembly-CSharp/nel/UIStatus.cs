using System;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class UIStatus
	{
		public UIStatus(UIBase _Base)
		{
			this.Base = _Base;
			UIStatus.Instance = this;
			this.Chr = MTRX.ChrL;
			this.MdBase = this.Base.MakeMesh(BLEND.NORMAL, MTRX.MIicon);
			this.Gob = this.Base.GetGob(this.MdBase);
			this.MdH = this.MakeMesh(MTRX.ColWhite, BLEND.NORMAL, null);
			this.MdM = this.MakeMesh(MTRX.ColWhite, BLEND.NORMAL, null);
			this.MdMpEgg = this.MakeMesh(MTRX.ColWhite, BLEND.NORMAL, null);
			this.MdMpEgg.InitSubMeshContainer(0);
			this.MdMpEgg.setMaterial(MTRX.getMtr(BLEND.MASK, 12), false);
			this.MdMpEgg.chooseSubMesh(1, false, false);
			this.MdMpEgg.setMaterial(MTRX.MIicon.getMtr(BLEND.NORMALST, 12), false);
			this.MdMpEgg.connectRendererToTriMulti(this.Base.GetGob(this.MdMpEgg).GetComponent<MeshRenderer>());
			this.MdIconT = this.MakeMesh(BLEND.NORMAL, MTRX.MIicon);
			this.MdGageT = this.MakeMesh(BLEND.NORMAL, MTRX.MIicon);
			this.MdSerRestTime = this.MakeMesh(MTRX.ColWhite, BLEND.NORMAL, null);
			this.MdAdd = this.MakeMesh(MTRX.ColWhite, BLEND.ADD, null);
			this.GobA = this.Base.GetGob(this.MdAdd);
			this.MaBaseH = new MdArranger(this.MdBase);
			this.MaBaseM = new MdArranger(this.MdBase);
			this.MaGageTH = new MdArranger(this.MdGageT);
			this.MaGageTM = new MdArranger(this.MdGageT);
			this.MaGageTS = new MdArranger(this.MdGageT);
			this.MaIconTOc = new MdArranger(this.MdIconT);
			this.MaIconTCrack = new MdArranger(this.MdIconT);
			MeshDrawer meshDrawer = this.MakeMesh(MTRX.ColWhite, BLEND.ADD, null);
			GameObject gob = this.Base.GetGob(meshDrawer);
			this.O2Gage = new UISpecialGage(meshDrawer, gob, MTRX.getPF("O2_title"), 100f, 100f, -0.25f);
			this.O2Gage.tcol = 4289057791U;
			this.O2Gage.tcol_pinch = 4294931834U;
			this.O2Gage.tcol_vibrate = 4291559424U;
			this.O2Gage.vib_draw_width = 16f;
			this.O2Gage.vib_draw_thresh_level = 0.125f;
			meshDrawer.base_px_y = 30f;
			this.O2Gage.slidein_y = -0.28125f;
			this.Gob.transform.localScale = new Vector3(2f, 2f, 1f);
			this.QuH = new Quaker(null);
			this.QuM = new Quaker(null);
			this.C = new C32();
			this.CIn = new C32();
			this.Aser = new UIStatus.SerFrame[52];
			this.Gob.SetActive(false);
			this.init_flag = (this.redraw_gage = (this.redraw_gage_back = true));
		}

		public void changePlayerIn(PR _Pr)
		{
			this.Pr = _Pr;
			this.pre_x = this.Pr.x;
			this.pre_y = this.Pr.y;
			this.fineLoad();
		}

		private MeshDrawer MakeMesh(Color32 Col, BLEND blnd, Material Mtr = null)
		{
			MeshDrawer meshDrawer = this.Base.MakeMesh(Col, blnd, Mtr);
			this.Base.GetGob(meshDrawer).transform.SetParent(this.Gob.transform, false);
			Transform transform = this.Base.GetGob(meshDrawer).transform;
			IN.setZ(transform, transform.localPosition.z - this.Gob.transform.localPosition.z);
			return meshDrawer;
		}

		private MeshDrawer MakeMesh(BLEND blnd, MImage MI)
		{
			MeshDrawer meshDrawer = this.Base.MakeMesh(blnd, MI);
			this.Base.GetGob(meshDrawer).transform.SetParent(this.Gob.transform, false);
			Transform transform = this.Base.GetGob(meshDrawer).transform;
			IN.setZ(transform, transform.localPosition.z - this.Gob.transform.localPosition.z);
			return meshDrawer;
		}

		private void fineMpMeshMaterial()
		{
			if (this.draw_puzzle_mp)
			{
				this.Base.get_MMRD().setMaterial(this.MdM, MTRX.getMtr(BLEND.ADD, -1), false);
				return;
			}
			this.Base.get_MMRD().setMaterial(this.MdM, MTRX.getMtr(BLEND.NORMAL, -1), false);
		}

		public void destruct()
		{
			if (UIStatus.Instance == this)
			{
				UIStatus.Instance = null;
			}
			UIStatus.FlgStatusHide.Clear();
			this.MdBase = null;
		}

		public bool need_reposit
		{
			get
			{
				return (this.reset_flags & 1) > 0;
			}
			set
			{
				this.reset_flags = (value ? (this.reset_flags | 1) : (this.reset_flags & -2));
			}
		}

		public bool redraw_hp
		{
			get
			{
				return (this.redraw_flags & 1U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 1U) : (this.redraw_flags & 4294967294U));
			}
		}

		public bool redraw_mp
		{
			get
			{
				return (this.redraw_flags & 2U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 2U) : (this.redraw_flags & 4294967293U));
			}
		}

		public bool redraw_bar_num
		{
			get
			{
				return (this.redraw_flags & 4U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 4U) : (this.redraw_flags & 4294967291U));
			}
		}

		public bool drawn_hp
		{
			get
			{
				return (this.redraw_flags & 256U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 256U) : (this.redraw_flags & 4294967039U));
			}
		}

		public bool drawn_mp
		{
			get
			{
				return (this.redraw_flags & 512U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 512U) : (this.redraw_flags & 4294966783U));
			}
		}

		public bool drawn_mpegg
		{
			get
			{
				return (this.redraw_flags & 65536U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 65536U) : (this.redraw_flags & 4294901759U));
			}
		}

		public bool redraw_mpegg
		{
			get
			{
				return (this.redraw_flags & 131072U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 131072U) : (this.redraw_flags & 4294836223U));
			}
		}

		public bool redraw_gage
		{
			get
			{
				return (this.redraw_flags & 2048U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 2048U) : (this.redraw_flags & 4294965247U));
			}
		}

		public bool redraw_gage_back
		{
			get
			{
				return (this.redraw_flags & 32768U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 32768U) : (this.redraw_flags & 4294934527U));
			}
		}

		public bool init_flag
		{
			get
			{
				return (this.redraw_flags & 1024U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 1024U) : (this.redraw_flags & 4294966271U));
			}
		}

		public bool draw_crack
		{
			get
			{
				return (this.redraw_flags & 4096U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 4096U) : (this.redraw_flags & 4294963199U));
			}
		}

		public bool drawn_icont
		{
			get
			{
				return (this.redraw_flags & 8192U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 8192U) : (this.redraw_flags & 4294959103U));
			}
		}

		public bool check_gage_crack
		{
			get
			{
				return (this.redraw_flags & 16384U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 16384U) : (this.redraw_flags & 4294950911U));
			}
		}

		public bool fine_gage_special
		{
			get
			{
				return (this.redraw_flags & 262144U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 262144U) : (this.redraw_flags & 4294705151U));
			}
		}

		public bool drawn_gage_back
		{
			get
			{
				return (this.redraw_flags & 524288U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 524288U) : (this.redraw_flags & 4294443007U));
			}
		}

		public bool drawn_gage_t
		{
			get
			{
				return (this.redraw_flags & 1048576U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 1048576U) : (this.redraw_flags & 4293918719U));
			}
		}

		public bool draw_oc_slot
		{
			get
			{
				return (this.redraw_flags & 2097152U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 2097152U) : (this.redraw_flags & 4292870143U));
			}
		}

		public bool redraw_ser
		{
			get
			{
				return (this.redraw_flags & 4194304U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 4194304U) : (this.redraw_flags & 4290772991U));
			}
		}

		public bool redraw_other_gaget_item
		{
			get
			{
				return (this.redraw_flags & 8388608U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 8388608U) : (this.redraw_flags & 4286578687U));
			}
		}

		public bool draw_oc_slot_re
		{
			get
			{
				return (this.redraw_flags & 16777216U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 16777216U) : (this.redraw_flags & 4278190079U));
			}
		}

		public bool redraw_ser_rest
		{
			get
			{
				return (this.redraw_flags & 33554432U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 33554432U) : (this.redraw_flags & 4261412863U));
			}
		}

		public bool drawn_sp
		{
			get
			{
				return (this.redraw_flags & 67108864U) > 0U;
			}
			set
			{
				this.redraw_flags = (value ? (this.redraw_flags | 67108864U) : (this.redraw_flags & 4227858431U));
			}
		}

		public void forceHide(bool strong = false)
		{
			int num = (strong ? (-10) : 0);
			if (this.ui_hold_time != (float)num)
			{
				this.ui_hold_time = (float)num;
				this.need_reposit = true;
			}
		}

		public void run(float fcnt)
		{
			if (this.init_flag)
			{
				this.MHBarInit();
			}
			if (UIStatus.FlgStatusHide.isActive())
			{
				this.forceHide(true);
			}
			if (this.base_y_level <= 0f)
			{
				if (this.need_reposit)
				{
					this.setBasePosExecute();
				}
				if (this.fine_gage_special)
				{
					this.draw();
				}
				return;
			}
			bool flag = false;
			if (X.D)
			{
				flag = this.O2Gage.run(X.AF) || flag;
				if (flag && this.ui_hold_time >= 0f)
				{
					this.ui_hold_time = X.Mx(this.ui_hold_time, (float)this.O2Gage.rest_time);
				}
			}
			int num = ((this.ui_hold_time > 0f) ? 3 : ((this.ui_hold_time < 0f) ? 0 : ((IN.totalframe % 4 == 0) ? this.checkPlayerActivation() : ((this.t >= 0f) ? 1 : 0))));
			if (this.t >= 0f)
			{
				if (num == 0)
				{
					this.t = X.Mn(-70f + (this.t - 60f), -1f);
					this.need_reposit = true;
				}
				else if (this.t < 120f)
				{
					if (num >= 2 && this.t < 60f)
					{
						this.t = 60f;
					}
					this.t += (float)((num >= 3) ? 2 : 1) * fcnt;
					if (this.t >= 60f)
					{
						this.need_reposit = true;
					}
				}
				if (this.egg_need_draw)
				{
					this.redraw_mpegg = true;
				}
			}
			else if (num > 0)
			{
				this.t = ((this.t <= -70f) ? 0f : X.Mx(120f + this.t + 10f, 0f));
				this.need_reposit = true;
				if (this.egg_need_draw)
				{
					this.redraw_mpegg = true;
				}
			}
			else if (this.t > -70f)
			{
				this.t -= (float)((this.ui_hold_time < 0f) ? ((this.t > -40f) ? 6 : 2) : 1) * fcnt;
				if (this.t <= -10f)
				{
					this.need_reposit = true;
				}
				if (this.egg_need_draw)
				{
					this.redraw_mpegg = true;
				}
			}
			if (X.D && this.need_reposit)
			{
				this.setBasePosExecute();
			}
			if (this.ui_hold_time < 0f || (this.ui_hold_time > 0f && this.cushion_hp == 0f && this.cushion_mp == 0f))
			{
				this.ui_hold_time = X.VALWALK(this.ui_hold_time, 0f, fcnt);
			}
			if (this.Pr.EggCon.status_reduce_animating)
			{
				this.redraw_bar_num = true;
				this.mpk_flags |= UIStatus.KF.EGGED_BLINK;
			}
			else if ((this.mpk_flags & UIStatus.KF.EGGED_BLINK) != UIStatus.KF.NONE)
			{
				this.redraw_bar_num = true;
				this.mpk_flags &= (UIStatus.KF)(-257);
			}
			bool flag2 = false;
			UIStatus.KF kf = this.hpk_flags & UIStatus.KF.FINISH_FADE;
			if (this.cushion_hp != 0f)
			{
				kf |= ((this.cushion_hp > 0f) ? UIStatus.KF.CURE : UIStatus.KF.DAMAGE);
				this.cushion_hp = X.VALWALK(this.cushion_hp, 0f, 0.003f * fcnt);
				flag2 = (this.redraw_hp = true);
				this.redraw_bar_num = true;
			}
			if (this.hpk_flags != kf)
			{
				if (kf == UIStatus.KF.NONE)
				{
					if (this.ch_anim_t != 0f && (this.hpk_flags & (UIStatus.KF)12) != UIStatus.KF.NONE)
					{
						this.redraw_hp = true;
						this.hpk_flags |= UIStatus.KF.FINISH_FADE;
						this.ch_anim_t = X.VALWALK(this.ch_anim_t, 0f, fcnt);
					}
					else
					{
						this.ch_anim_t = 0f;
						this.hpk_flags = kf;
					}
				}
				else
				{
					this.ch_anim_t = 4f;
					this.hpk_flags = kf;
				}
				this.redraw_bar_num = true;
			}
			kf = this.mpk_flags & (UIStatus.KF)3616;
			int num2 = (int)X.Mn(this.Pr.get_maxmp(), (float)this.Pr.Skill.getHoldingMp(false));
			int num3 = (int)X.Mn(X.Mn(this.Pr.get_maxmp() - this.Pr.get_mp(), (float)num2), (float)this.Pr.Skill.getOverHoldingMp(true));
			if (this.mp_hold != num2)
			{
				this.mp_hold = num2;
				this.anm_delay_mphold = 0;
				flag2 = (this.redraw_mp = true);
				this.redraw_bar_num = true;
				this.redraw_other_gaget_item = true;
				this.mp_ratio = this.Pr.Skill.getCastableMpRatioForUi();
				if (this.mp_hold == 0)
				{
					kf |= UIStatus.KF.FINISH_FADE;
				}
				if (this.mp_ratio < 0.15f)
				{
					kf |= UIStatus.KF.HUNGER;
				}
			}
			else if (num2 != 0)
			{
				int num4 = this.anm_delay_mphold + 1;
				this.anm_delay_mphold = num4;
				if (num4 >= 2)
				{
					this.redraw_mp = true;
					this.redraw_bar_num = true;
					this.redraw_other_gaget_item = true;
					this.anm_delay_mphold = 0;
				}
			}
			if (num2 != 0)
			{
				kf |= UIStatus.KF.HOLD;
				if (num3 > 0)
				{
					kf |= UIStatus.KF.OVERHOLD;
				}
			}
			if (!this.Pr.isPuzzleManagingMp())
			{
				if (this.Pr.GaugeBrk.isReducing())
				{
					kf |= UIStatus.KF.REDUCE_SPILIT;
					this.redraw_gage_back = true;
				}
				else if ((this.mpk_flags & UIStatus.KF.REDUCE_SPILIT) != UIStatus.KF.NONE)
				{
					this.redraw_gage_back = true;
				}
			}
			else if (PUZ.IT.puzz_magic_count_max == -1)
			{
				this.redraw_mp = true;
			}
			if (this.cushion_mp != 0f)
			{
				this.cushion_mp = X.VALWALK(this.cushion_mp, 0f, (this.Pr.isPuzzleManagingMp() ? 0.03125f : ((this.mp_hold != 0 && this.cushion_mp > 0f) ? 0.007f : 0.003f)) * fcnt);
				kf |= ((this.cushion_mp == 0f) ? UIStatus.KF.FINISH_FADE : (((this.cushion_mp > 0f) ? UIStatus.KF.CURE : UIStatus.KF.DAMAGE) | (this.mpk_flags & UIStatus.KF.HOLD_RELEASE)));
				flag2 = (this.redraw_mp = true);
				this.redraw_bar_num = true;
			}
			if (this.mpk_flags != kf)
			{
				if (kf == UIStatus.KF.NONE)
				{
					if (this.cm_anim_t != 0f && (this.mpk_flags & (UIStatus.KF)12) != UIStatus.KF.NONE)
					{
						this.redraw_mp = true;
						this.mpk_flags |= UIStatus.KF.FINISH_FADE;
						this.cm_anim_t = X.VALWALK(this.cm_anim_t, 0f, fcnt);
					}
					else
					{
						this.cm_anim_t = 0f;
						this.mpk_flags = kf;
					}
				}
				else
				{
					this.cm_anim_t = 4f;
					this.mpk_flags = kf;
				}
				this.redraw_bar_num = true;
			}
			if (X.D)
			{
				if (flag2)
				{
					this.Base.fineHpMpRatio(this.hp_ratio - this.cushion_hp, this.mp_ratio - this.cushion_mp);
				}
				if (this.Pr.Mp != null && !COOK.reloading)
				{
					ulong bits = this.Pr.Ser.get_bits();
					try
					{
						if (bits != 0UL || this.ser_max != 0)
						{
							int i = 0;
							this.ser_bits = 0UL;
							while (i < this.ser_max)
							{
								UIStatus.SerFrame serFrame = this.Aser[i];
								ulong num5 = 1UL << (int)serFrame.ser;
								if (serFrame.t < 0f)
								{
									this.redraw_ser = true;
									serFrame.draw_rest_lv = -1f;
									if ((bits & num5) != 0UL)
									{
										this.SerFrameSpliceFrom(0);
										X.shiftEmpty<UIStatus.SerFrame>(this.Aser, 1, i, -1);
										UIStatus.SerFrame[] aser = this.Aser;
										int num4 = this.ser_max - 1;
										this.ser_max = num4;
										aser[num4] = serFrame;
										continue;
									}
									if ((serFrame.t -= (float)X.AF) < -40f)
									{
										this.checkSpecialEffectSer(serFrame, null, true);
										this.SerFrameSpliceFrom(0);
										X.shiftEmpty<UIStatus.SerFrame>(this.Aser, 1, i, -1);
										UIStatus.SerFrame[] aser2 = this.Aser;
										int num4 = this.ser_max - 1;
										this.ser_max = num4;
										aser2[num4] = serFrame;
										continue;
									}
								}
								else if ((bits & num5) == 0UL)
								{
									serFrame.t = -1f;
									this.checkSpecialEffectSer(serFrame, null, true);
								}
								else
								{
									if (serFrame.t < 30f)
									{
										this.redraw_ser = true;
									}
									serFrame.t += (float)X.AF;
									if (!serFrame.snd && serFrame.t >= 16f)
									{
										if (!this.Pr.NM2D.GM.isBenchMenuActive())
										{
											SND.Ui.play("ser_init", false);
										}
										serFrame.snd = true;
									}
								}
								if (serFrame.t >= 0f)
								{
									this.ser_bits |= num5;
								}
								i++;
							}
							int length = this.Pr.Ser.Length;
							for (i = 0; i < length; i++)
							{
								M2SerItem m2SerItem = this.Pr.Ser.Get(i);
								ulong num6 = 1UL << (int)m2SerItem.id;
								if (m2SerItem.isActive() && (this.ser_bits & num6) <= 0UL)
								{
									this.ser_bits |= num6;
									PxlFrame iconImage = m2SerItem.getIconImage();
									if (iconImage != null)
									{
										UIStatus.SerFrame serFrame2 = this.Aser[this.ser_max];
										if (serFrame2 == null)
										{
											serFrame2 = (this.Aser[this.ser_max] = new UIStatus.SerFrame());
										}
										serFrame2.ser = m2SerItem.id;
										this.checkSpecialEffectSer(serFrame2, m2SerItem, false);
										serFrame2.t = 0f;
										serFrame2.SerItm = m2SerItem;
										serFrame2.PF = iconImage;
										serFrame2.snd = false;
										serFrame2.draw_rest_lv = -1f;
										this.redraw_ser = true;
										this.redraw_ser_rest = true;
										this.ser_max++;
									}
								}
							}
						}
					}
					catch
					{
					}
				}
				float x = this.QuH.x;
				float y = this.QuH.y;
				float x2 = this.QuM.x;
				float y2 = this.QuM.y;
				if ((this.hpk_flags & UIStatus.KF.FINISH_FADE) != UIStatus.KF.NONE)
				{
					this.redraw_hp = true;
				}
				if ((this.mpk_flags & UIStatus.KF.FINISH_FADE) != UIStatus.KF.NONE)
				{
					this.redraw_hp = true;
				}
				if ((this.mpk_flags & UIStatus.KF.OC_ANIMATE) != UIStatus.KF.NONE)
				{
					this.Pr.getSkillManager().getOverChargeSlots().runEffect(X.AF);
					if ((this.mpk_flags & UIStatus.KF.OC_HOLDING_ANIMATE) != UIStatus.KF.NONE && (this.anm_delay_barnum += X.AF) >= 5)
					{
						this.redraw_bar_num = true;
						this.anm_delay_barnum -= 5;
					}
				}
				if (this.t_hit_gsaver_hp > 0f)
				{
					this.t_hit_gsaver_hp -= (float)X.AF;
					if (this.t_hit_gsaver_hp <= 0f)
					{
						this.t_hit_gsaver_hp = 0f;
						this.redraw_hp = true;
					}
				}
				if (this.t_hit_gsaver_mp > 0f)
				{
					this.t_hit_gsaver_mp -= (float)X.AF;
					if (this.t_hit_gsaver_mp <= 0f)
					{
						this.t_hit_gsaver_mp = 0f;
						this.redraw_mp = true;
					}
				}
				if (this.QuH.run((float)X.AF))
				{
					this.redraw_bar_num = (this.redraw_hp = true);
					if (this.QuH.Length == 0)
					{
						this.redraw_gage_back = (this.redraw_gage = true);
					}
					else
					{
						if (!this.redraw_gage)
						{
							this.MaGageTH.translateAll(-x + this.QuH.x, -y + this.QuH.y, true);
							this.drawn_gage_t = true;
						}
						if (!this.redraw_gage_back)
						{
							this.MaBaseH.translateAll(-x + this.QuH.x, -y + this.QuH.y, true);
							this.drawn_gage_back = true;
						}
					}
					if (this.Pr.DMG.hp_crack > 0)
					{
						this.draw_crack = true;
					}
				}
				else if (this.hp_ratio < 0.33f && this.Pr.is_alive)
				{
					this.redraw_gage_back = true;
				}
				if (this.QuM.run((float)X.AF))
				{
					this.redraw_bar_num = (this.redraw_mpegg = (this.redraw_mp = true));
					if (this.QuM.Length == 0)
					{
						this.redraw_gage_back = (this.redraw_gage = (this.draw_crack = true));
					}
					else
					{
						if (!this.redraw_gage)
						{
							this.MaGageTM.translateAll(-x2 + this.QuM.x, -y2 + this.QuM.y, true);
							this.drawn_gage_t = true;
						}
						if (!this.redraw_gage_back)
						{
							this.MaBaseM.translateAll(-x2 + this.QuM.x, -y2 + this.QuM.y, true);
							this.drawn_gage_back = true;
						}
						if (!this.draw_crack)
						{
							if (this.Pr.DMG.hp_crack > 0)
							{
								this.draw_crack = true;
							}
							else
							{
								this.MaIconTCrack.translateAll(-x2 + this.QuM.x, -y2 + this.QuM.y, true);
								this.drawn_icont = true;
							}
						}
					}
				}
				else if (this.mp_ratio < 0.33f && this.Pr.is_alive)
				{
					this.redraw_gage_back = true;
				}
				this.draw();
				this.hpk_flags &= (UIStatus.KF)(-33);
				this.mpk_flags &= (UIStatus.KF)(-33);
			}
		}

		public void levelupStatusImage(M2SerItem Ser, SER ser, bool level_down = false)
		{
			ulong num = 1UL << (int)ser;
			if ((this.ser_bits & num) == 0UL)
			{
				return;
			}
			for (int i = 0; i < this.ser_max; i++)
			{
				UIStatus.SerFrame serFrame = this.Aser[i];
				if (serFrame.ser == ser)
				{
					if (Ser.isActive())
					{
						if (!level_down)
						{
							serFrame.snd = false;
						}
						serFrame.t = 0f;
					}
					serFrame.PF = Ser.getIconImage();
					this.checkSpecialEffectSer(serFrame, Ser, false);
					return;
				}
			}
		}

		private void checkSpecialEffectSer(UIStatus.SerFrame Si, M2SerItem Ser, bool remove = false)
		{
			if (Si.ser == SER.CONFUSE)
			{
				if (!remove && Ser.level >= 1)
				{
					if (this.ConfuseCurtain == null)
					{
						this.ConfuseCurtain = new ConfuseDrawer();
						this.fine_gage_special = true;
						return;
					}
					if (!this.ConfuseCurtain.isActive())
					{
						this.ConfuseCurtain.activate();
						this.fine_gage_special = true;
						return;
					}
				}
				else if (this.ConfuseCurtain != null)
				{
					this.ConfuseCurtain.deactivate();
					this.fine_gage_special = true;
				}
			}
		}

		public void fineHpRatio(bool use_cushion = false, bool use_quake = false)
		{
			float num = this.Pr.hp_ratio;
			if (num < 0.2f != this.hp_ratio < 0.2f)
			{
				this.redraw_gage_back = true;
			}
			float num2 = num - this.hp_ratio;
			if (num2 != 0f)
			{
				if (use_cushion)
				{
					if (this.cushion_hp != 0f && num2 > 0f != this.cushion_hp > 0f)
					{
						this.cushion_hp = 0f;
					}
					this.cushion_hp += num2;
				}
				else
				{
					this.redraw_hp = (this.redraw_bar_num = true);
				}
				this.ui_hold_time = X.Mx(this.ui_hold_time, 150f);
			}
			if (use_quake && num2 <= 0f)
			{
				this.QuH.Vib(2f, 13f, 1f, 0);
				if (use_cushion && this.cushion_hp == 0f)
				{
					this.hpk_flags |= (UIStatus.KF)40;
					this.ch_anim_t = X.Mx(this.ch_anim_t, 2f);
				}
			}
			this.hp_ratio = this.Pr.hp_ratio;
		}

		public void fineMpRatio(bool use_cushion = false, bool use_quake = false)
		{
			float castableMpRatioForUi = this.Pr.Skill.getCastableMpRatioForUi();
			if (castableMpRatioForUi < 0.15f != this.mp_ratio < 0.15f)
			{
				this.redraw_gage_back = true;
			}
			float num = castableMpRatioForUi - this.mp_ratio;
			if (num != 0f)
			{
				if (use_cushion)
				{
					if (this.cushion_mp != 0f && num > 0f != this.cushion_mp > 0f)
					{
						this.cushion_mp = 0f;
					}
					this.cushion_mp += num;
				}
				else
				{
					this.redraw_mp = (this.redraw_bar_num = true);
				}
				this.ui_hold_time = X.Mx(this.ui_hold_time, 150f);
			}
			if (use_quake && num <= 0f)
			{
				this.QuM.Vib(2f, 13f, 1f, 0);
				if (use_cushion && this.cushion_mp == 0f)
				{
					this.mpk_flags |= (UIStatus.KF)40;
					this.cm_anim_t = X.Mx(this.cm_anim_t, 2f);
				}
				this.redraw_bar_num = true;
			}
			if (!use_quake && num <= 0f && (this.mpk_flags & UIStatus.KF.HOLD) != UIStatus.KF.NONE)
			{
				this.mpk_flags |= UIStatus.KF.HOLD_RELEASE;
			}
			this.mp_ratio = castableMpRatioForUi;
		}

		public void finePlantedEgg()
		{
			this.redraw_bar_num = true;
			if (this.egg_need_draw)
			{
				this.ui_hold_time = X.Mx(this.ui_hold_time, 150f);
				return;
			}
			this.MdMpEgg.clear(false, false);
		}

		public void fineGSaverHpHit()
		{
			if (this.t_hit_gsaver_hp == 0f)
			{
				this.redraw_hp = true;
			}
			this.t_hit_gsaver_hp = 8f;
		}

		public void fineGSaverMpHit()
		{
			if (this.t_hit_gsaver_mp == 0f)
			{
				this.redraw_mp = true;
			}
			this.t_hit_gsaver_mp = 8f;
		}

		public void fineOcSlot(M2PrOverChargeSlot OcSlots, bool use_anim, bool fine_effect = false)
		{
			this.draw_oc_slot = true;
			if (fine_effect)
			{
				this.fine_gage_special = true;
			}
			bool flag = false;
			if (use_anim)
			{
				this.mpk_flags |= UIStatus.KF.OC_ANIMATE;
				flag = OcSlots.isUseHolding();
			}
			else
			{
				this.mpk_flags &= (UIStatus.KF)(-1025);
			}
			if ((this.mpk_flags & UIStatus.KF.OC_HOLDING_ANIMATE) > UIStatus.KF.NONE != flag)
			{
				if (flag)
				{
					this.mpk_flags |= UIStatus.KF.OC_HOLDING_ANIMATE;
				}
				else
				{
					this.mpk_flags &= (UIStatus.KF)(-2049);
				}
				this.redraw_bar_num = true;
			}
		}

		public void fineO2()
		{
			M2PrMistApplier mistApplier = this.Pr.getMistApplier();
			if (mistApplier != null)
			{
				mistApplier.fineUi();
				return;
			}
			this.fineO2(100f, 0f);
		}

		public void fineO2(float o2_point, float t_water)
		{
			this.O2Gage.value = o2_point;
			this.O2Gage.vib_level = this.O2Gage.vib_draw_thresh_level * X.ZLINE(100f - o2_point, 25f) + (1f - this.O2Gage.vib_draw_thresh_level) * X.ZLINE(t_water, 200f);
		}

		public void showO2Gage(int alloc_t = 30)
		{
			this.O2Gage.showForce(alloc_t);
		}

		public void playerUiEffectClear()
		{
			if (this.Pr != null)
			{
				this.Pr.EpCon.releaseEffect();
			}
		}

		public void fineLoad()
		{
			this.Base.clear();
			this.MHBarInit();
			this.ser_max = 0;
			if (this.ConfuseCurtain != null)
			{
				this.ConfuseCurtain.deactivate();
			}
			this.ser_bits = 0UL;
			this.ConfuseCurtain = null;
			this.mpk_flags = UIStatus.KF.NONE;
			this.hpk_flags = UIStatus.KF.NONE;
			this.cushion_hp = (this.cushion_mp = 0f);
			this.mp_hold = this.Pr.Skill.getHoldingMp(false);
			this.t_hit_gsaver_hp = (this.t_hit_gsaver_mp = 0f);
			this.hcrk_t = -1;
			this.finePlantedEgg();
			this.QuH.clear();
			this.QuM.clear();
			this.redraw_flags = uint.MaxValue;
			this.t = 0f;
			this.ch_anim_t = 0f;
			this.cm_anim_t = 0f;
			this.bounds_hp_w = (this.bounds_mp_w = 28f);
			this.Base.fineHpMpRatio(this.hp_ratio, this.mp_ratio);
			this.Base.fineUiDmgCounterDraw();
			this.MdBase.clear(false, true);
			this.MdAdd.clear(false, true);
			this.MdAdd.connectRendererToTriMulti(this.GobA.GetComponent<MeshRenderer>());
			this.setBasePosExecute();
			if (this.Pr.UP != null)
			{
				this.Pr.UP.newGame();
			}
			this.Pr.EpCon.releaseEffect();
			this.Pr.EpCon.fineEffect(false);
			M2PrSkill skill = this.Pr.Skill;
			if (skill != null)
			{
				skill.getOverChargeSlots().fineSlots(true);
			}
			this.Pr.NM2D.IMNG.USel.prepareResource(M2DBase.Instance);
		}

		public void finePuzzleMagicManaging(PR _Pr, bool force = false)
		{
			if (_Pr != this.Pr || PUZ.IT == null)
			{
				return;
			}
			if (this.init_flag)
			{
				this.MHBarInit();
			}
			bool flag = PUZ.IT.barrier_active && PUZ.IT.puzz_magic_count_max > -2;
			if (flag != (this.mpk_flags & UIStatus.KF.PUZZLE_MANAGING_MP) > UIStatus.KF.NONE || force)
			{
				if (flag)
				{
					this.mpk_flags |= UIStatus.KF.PUZZLE_MANAGING_MP;
					this.cushion_mp = (this.mp_ratio = this.Pr.Skill.getCastableMpRatioForUi());
				}
				else
				{
					this.mpk_flags &= (UIStatus.KF)(-513);
					this.fineMpRatio(false, false);
					this.cushion_mp = 0f;
				}
				this.ui_hold_time = X.Mx(this.ui_hold_time, 150f);
				this.redraw_hp = (this.redraw_mp = (this.redraw_bar_num = (this.draw_crack = (this.redraw_gage_back = (this.redraw_gage = true)))));
				this.fineMpMeshMaterial();
				this.finePlantedEgg();
			}
		}

		public void draw()
		{
			if (this.redraw_gage || this.redraw_gage_back)
			{
				Bench.P("Status-gauge");
				this.drawGauge();
				Bench.Pend("Status-gauge");
			}
			Bench.P("OC/crack");
			bool flag = false;
			bool flag2 = false;
			if (this.draw_oc_slot)
			{
				this.drawOcSlots(false);
			}
			if (this.draw_crack)
			{
				flag2 = this.drawCrack();
			}
			Bench.Pend("OC/crack");
			bool flag3 = false;
			bool flag4 = false;
			Bench.P("barH");
			if (this.redraw_hp)
			{
				this.MHBar(this.MdH, ref flag4, false, 1f);
			}
			else if (this.draw_crack)
			{
				this.MHBar(this.MdH, ref flag4, true, 1f);
			}
			Bench.Pend("barH");
			Bench.P("barM");
			if (this.redraw_mp)
			{
				this.MHBar(this.MdM, ref flag4, false, 1f);
			}
			else if (this.draw_crack)
			{
				this.MHBar(this.MdM, ref flag4, true, 1f);
			}
			Bench.Pend("barM");
			if (this.redraw_mpegg)
			{
				Bench.P("egg");
				this.drawMpEgg();
				Bench.Pend("egg");
			}
			if (this.redraw_ser)
			{
				Bench.P("ser");
				this.drawSerIcons();
				Bench.Pend("ser");
			}
			if (this.redraw_bar_num)
			{
				Bench.P("barnum");
				flag3 = this.redrawBarNumber(1f);
				Bench.Pend("barnum");
			}
			if (this.drawn_icont)
			{
				this.MdIconT.updateForMeshRenderer(true);
			}
			if (this.drawn_gage_t)
			{
				this.MdGageT.updateForMeshRenderer(true);
			}
			if (this.drawn_hp)
			{
				this.MdH.updateForMeshRenderer(false);
			}
			if (this.drawn_mp)
			{
				this.MdM.updateForMeshRenderer(false);
			}
			if (this.drawn_mpegg)
			{
				this.MdMpEgg.updateForMeshRenderer(false);
			}
			if (this.fine_gage_special)
			{
				Bench.P("gauge_sp");
				flag = this.drawGageSpecial();
				Bench.Pend("gauge_sp");
			}
			bool flag5 = this.drawSerRestTime();
			if (this.drawn_gage_back)
			{
				this.MdBase.updateForMeshRenderer(true);
			}
			if (this.drawn_sp)
			{
				this.MdAdd.updateForMeshRenderer(true);
			}
			if (flag5)
			{
				this.MdSerRestTime.updateForMeshRenderer(true);
			}
			this.redraw_flags = 0U;
			if (flag2)
			{
				this.draw_crack = true;
			}
			if (flag)
			{
				this.fine_gage_special = true;
			}
			if (flag3)
			{
				this.redraw_gage = (this.redraw_gage_back = true);
			}
		}

		public void fineBasePos(float tz)
		{
			if (this.base_y_level != tz)
			{
				this.base_y_level = tz;
				this.need_reposit = true;
			}
		}

		private void setBasePosExecute()
		{
			this.need_reposit = false;
			this.O2Gage.alpha = this.base_y_level;
			if (this.base_y_level <= 0f)
			{
				this.Gob.SetActive(false);
				this.t = 0f;
				this.gage_tz = 0f;
			}
			else if (this.t < 0f || this.t > 60f)
			{
				if (!this.Gob.activeSelf)
				{
					this.Gob.SetActive(true);
				}
				float num = ((this.t >= 0f) ? X.ZSIN(this.t - 60f, 60f) : X.ZSIN(60f + this.t + 10f, 60f));
				num = X.Mn(this.base_y_level, num);
				Vector3 localPosition = this.Gob.transform.localPosition;
				if (this.gage_tz == 0f && num > 0f)
				{
					this.fine_gage_special = true;
				}
				this.gage_tz = num;
				float num2 = 120f * num;
				localPosition.x = 0f;
				float num3 = (float)((CFGSP.uipic_lr == CFGSP.UIPIC_LR.NONE) ? 0 : ((CFGSP.uipic_lr == CFGSP.UIPIC_LR.R) ? (-1) : 1));
				localPosition.x += (IN.wh - UIBase.gamewh) * (1f - this.Base.gamemenu_slide_z) * 0.015625f * num3;
				if (!this.Base.event_center_uipic)
				{
					localPosition.x -= 240f * this.Base.gamemenu_bench_slide_z * 0.015625f;
				}
				localPosition.y = (-IN.hh - 40f + num2) * 0.015625f;
				this.Gob.transform.localPosition = localPosition;
			}
			this.Base.fineLogPos();
		}

		private void MHBarInit()
		{
			this.hp_ratio = this.Pr.hp_ratio;
			this.mp_ratio = this.Pr.Skill.getCastableMpRatioForUi();
			this.crack_t = -1;
			this.MdGageT.clear(false, false);
			this.MdIconT.clear(false, false);
			this.redraw_gage = (this.redraw_gage_back = true);
			this.redraw_hp = true;
			this.redraw_mp = true;
			this.redraw_mpegg = true;
			this.redraw_ser = true;
			this.redraw_ser_rest = true;
			this.draw_crack = true;
			this.fine_gage_special = true;
			this.draw_oc_slot = true;
			this.redraw_bar_num = true;
			this.need_reposit = true;
			this.fineO2();
			this.init_flag = false;
		}

		private void drawGauge()
		{
			PxlLayer[] agauge = MTR.AGauge;
			bool flag = false;
			this.MdBase.getVertexMax();
			if (this.redraw_gage_back)
			{
				this.MdBase.getTriMax();
				this.MdBase.revertVerAndTriIndex(0, 0, false);
				this.MdBase.base_z = 0f;
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			if (this.redraw_gage)
			{
				num = this.MdGageT.getVertexMax();
				num2 = this.MdGageT.getTriMax();
				num3 = this.MaGageTH.Length + this.MaGageTM.Length;
				this.MdGageT.revertVerAndTriIndex(0, 0, false);
				this.MdGageT.Col = MTRX.ColWhite;
			}
			for (int i = 0; i < 2; i++)
			{
				float num4 = ((i == 0) ? this.QuH.x : this.QuM.x) * 64f;
				float num5 = ((i == 0) ? this.QuH.y : this.QuM.y) * 64f;
				bool flag2 = i == 1 && this.draw_puzzle_mp;
				if (this.redraw_gage_back)
				{
					if (i == 0)
					{
						this.MaBaseH.Set(true);
					}
					else
					{
						this.MaBaseM.Set(true);
					}
				}
				if (this.redraw_gage)
				{
					if (i == 0)
					{
						this.MaGageTH.Set(true);
					}
					else
					{
						this.MaGageTM.Set(true);
					}
				}
				if (i == 1 && (this.mpk_flags & UIStatus.KF.REDUCE_SPILIT) != UIStatus.KF.NONE && this.redraw_gage_back)
				{
					this.MdBase.Identity();
					this.MdBase.Col = C32.d2c(4288659924U);
					this.MdBase.ColGrd.Set(1144459866);
					this.MdBase.RotaPF(num4 + this.mp_cenx + 140f * this.mp_ratio + 4.5f, num5 + this.mp_ceny - 10f - 7.5f, 0.5f, 1f, 0f, MTR.AGaugeSplit[X.ANMT(7, 6f)], false, false, false, uint.MaxValue, false, 0);
					Color32[] colorArray = this.MdBase.getColorArray();
					colorArray[this.MdBase.getVertexMax() - 4] = this.MdBase.ColGrd.C;
					colorArray[this.MdBase.getVertexMax() - 1] = this.MdBase.ColGrd.C;
				}
				num4 += -140f + (float)i;
				if (this.redraw_gage_back)
				{
					this.MdBase.Col = this.MdBase.ColGrd.Set(flag2 ? uint.MaxValue : 2572835418U).C;
					this.DrawGageLay(this.MdBase, num4 - 4f, num5, agauge[flag2 ? 9 : 1], flag, 140f, ALIGN.LEFT);
				}
				if (this.redraw_gage)
				{
					if (flag2)
					{
						int puzz_magic_count_max = PUZ.IT.puzz_magic_count_max;
						for (int j = 1; j < puzz_magic_count_max; j++)
						{
							this.DrawGageLay(this.MdGageT, num4 - 4f + 140f * (float)j / (float)puzz_magic_count_max, num5, agauge[11], flag, 0f, ALIGN.CENTER);
						}
					}
					this.DrawGageLay(this.MdGageT, num4 - 4f, num5, agauge[0], flag, 0f, ALIGN.LEFT);
					this.DrawGageLay(this.MdGageT, num4 - 4f, num5, agauge[flag2 ? 10 : 2], flag, 140f, ALIGN.LEFT);
				}
				num4 += 140f;
				float num6 = 31f;
				float num7 = ((i == 0) ? this.bounds_hp_w : this.bounds_mp_w) - -20f;
				if (this.redraw_gage)
				{
					this.DrawGageLay(this.MdGageT, num4, num5, agauge[4], flag, 0f, ALIGN.LEFT);
					this.DrawGageLay(this.MdGageT, num4 - (num7 - num6) + 4f, num5, agauge[6], flag, 0f, ALIGN.LEFT);
					this.DrawGageLay(this.MdGageT, num4, num5, agauge[8], flag, num7 - num6, ALIGN.RIGHT);
					if (i == 0)
					{
						this.MaGageTH.Set(false);
					}
					else
					{
						this.MaGageTM.Set(false);
					}
				}
				if (this.redraw_gage_back)
				{
					this.MdBase.Col = (((i == 0) ? (this.hp_ratio < 0.2f) : (this.mp_ratio < 0.15f)) ? this.MdBase.ColGrd.Set(4294937226U).blend(4286997861U, 0.5f + 0.5f * X.COSIT(140f)).C : this.MdBase.ColGrd.Set(4284111450U).C);
					this.DrawGageLay(this.MdBase, num4, num5, agauge[3], flag, 0f, ALIGN.LEFT);
					this.DrawGageLay(this.MdBase, num4 - (num7 - num6) + 4f, num5, agauge[5], flag, 0f, ALIGN.LEFT);
					this.DrawGageLay(this.MdBase, num4, num5, agauge[7], flag, num7 - num6, ALIGN.RIGHT);
					if (i == 0)
					{
						this.MaBaseH.Set(false);
					}
					else
					{
						this.MaBaseM.Set(false);
					}
				}
				flag = true;
			}
			if (this.redraw_gage_back)
			{
				this.drawn_gage_back = true;
				this.redraw_gage_back = false;
				this.MdBase.Col = MTRX.ColWhite;
			}
			if (this.redraw_gage)
			{
				this.drawn_gage_t = true;
				this.redraw_gage = false;
				if (this.MaGageTH.Length + this.MaGageTM.Length != num3 || num <= this.MdGageT.getVertexMax())
				{
					this.redraw_ser = true;
					this.redraw_ser_rest = true;
					this.redraw_bar_num = true;
					return;
				}
				this.MdGageT.revertVerAndTriIndex(num, num2, false);
			}
		}

		private bool drawGageSpecial()
		{
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			this.MdAdd.clearSimple();
			if (this.Pr.getSkillManager().getOverChargeSlots().isEffectActive())
			{
				this.MdAdd.chooseSubMesh(0, false, true);
				flag2 = this.drawOcSlots(true) || flag2;
				this.drawn_sp = true;
			}
			else
			{
				this.MdAdd.chooseSubMesh(0, false, true);
			}
			if (this.ConfuseCurtain != null)
			{
				if (this.MdAdd.getSubMeshData(1, ref num) == null)
				{
					this.MdAdd.chooseSubMesh(1, false, true);
					this.MdAdd.setMaterial(MTR.MtrConfuseCurtain, false);
					this.MdAdd.base_z = -0.05f;
					flag = true;
				}
				else
				{
					this.MdAdd.chooseSubMesh(1, false, true);
				}
				this.drawn_sp = true;
				if (this.gage_tz > 0f)
				{
					bool flag3 = this.ConfuseCurtain.run((float)X.AF, this.MdAdd);
					if (!flag3 && !this.ConfuseCurtain.isActive())
					{
						this.ConfuseCurtain = null;
					}
					flag2 = flag2 || flag3;
				}
			}
			if (flag)
			{
				this.MdAdd.connectRendererToTriMulti(this.GobA.GetComponent<MeshRenderer>());
			}
			return flag2;
		}

		private void DrawGageLay(MeshDrawer Md, float x, float y, PxlLayer L, bool flip, float draww = 0f, ALIGN align = ALIGN.LEFT)
		{
			PxlImage img = L.Img;
			float num = 1f;
			if (draww > 0f)
			{
				num = draww / (float)img.width;
			}
			if (align == ALIGN.LEFT)
			{
				x += (float)(-(float)(img.width / 2)) + (float)(img.width / 2) * num;
			}
			else if (align == ALIGN.RIGHT)
			{
				x += (float)X.IntC((float)img.width / 2f) - (float)(img.width / 2) * num;
			}
			Md.Identity();
			Md.initForImg(img, 0).RotaGraph3((float)(flip ? (-1) : 1) * (x + L.x), y - L.y + (float)(img.height / 2), 0.5f, 1f, num, 1f, 0f, null, flip);
		}

		private float hp_cenx
		{
			get
			{
				return -4f;
			}
		}

		private float hp_ceny
		{
			get
			{
				return 4f;
			}
		}

		private float mp_cenx
		{
			get
			{
				return 3f;
			}
		}

		private float mp_ceny
		{
			get
			{
				return 4f;
			}
		}

		private void drawBase(float tz)
		{
		}

		private float tratio(float tz)
		{
			if (tz < 1f)
			{
				return 0.5f + 0.5f * X.ZSIN(tz);
			}
			return 1f;
		}

		private bool redrawBarNumber(float tz = 1f)
		{
			bool flag = false;
			Bench.P("Bar 1");
			this.redraw_bar_num = false;
			this.drawn_gage_t = true;
			this.redraw_other_gaget_item = true;
			float num = this.hp_cenx + 2f + this.QuH.x * 64f * 1.5f;
			float num2 = this.hp_ceny - 13.5f + this.QuH.y * 64f * 1.5f;
			this.MaGageTS.revertVerAndTriIndexSaved(false);
			C32 c = this.MdGageT.ColGrd.White();
			STB stb = TX.PopBld(null, 0);
			stb += (int)this.Pr.get_hp();
			stb += "/";
			stb += (int)this.Pr.get_maxhp();
			Bench.Pend("Bar 1");
			Bench.P("Bar 2");
			float num3 = this.bounds_hp_w;
			int num4 = ((this.hpk_flags == UIStatus.KF.NONE) ? 0 : X.ANMT(2, 5f));
			if ((this.hpk_flags & UIStatus.KF.DAMAGE) != UIStatus.KF.NONE)
			{
				c.Set((num4 == 0) ? 4292707974U : 4291123031U);
			}
			else if ((this.hpk_flags & UIStatus.KF.CURE) != UIStatus.KF.NONE)
			{
				c.Set((num4 == 0) ? 4288741359U : uint.MaxValue);
			}
			else
			{
				c.Set(uint.MaxValue);
			}
			if ((this.hpk_flags & UIStatus.KF.FINISH_FADE) != UIStatus.KF.NONE)
			{
				this.MdGageT.Col = c.blend(uint.MaxValue, X.ZLINE(4f - this.ch_anim_t, 4f)).C;
			}
			else
			{
				this.MdGageT.Col = c.C;
			}
			Bench.Pend("Bar 2");
			Bench.P("Bar 3");
			float num5 = this.Chr.DrawStringTo(null, stb, 0f, 0f, ALIGN.LEFT, ALIGNY.TOP, false, 0f, 0f, null) * 0.5f;
			this.bounds_hp_w = X.Mx(num5 + -20f, this.bounds_hp_w);
			this.Chr.DrawScaleStringTo(this.MdGageT, stb, num - (this.bounds_hp_w - -20f) * 0.5f - num5 * 0.5f, num2, 0.5f, 0.5f, ALIGN.LEFT, ALIGNY.BOTTOM, false, this.bounds_hp_w, 0f, null);
			Bench.Pend("Bar 3");
			if (num3 < this.bounds_hp_w)
			{
				flag = true;
			}
			Bench.P("Bar 4");
			c.White();
			num = this.mp_cenx - 4f + this.QuM.x * 64f * 1.5f;
			num2 = this.mp_ceny - 13.5f + this.QuM.y * 64f * 1.5f;
			stb.Clear();
			this.Pr.getPuzzleMpRatioStringForUi(stb);
			int num6 = ((this.mpk_flags == UIStatus.KF.NONE) ? 0 : X.ANMT(2, 5f));
			Bench.Pend("Bar 4");
			if (stb.Length > 0)
			{
				this.MdGageT.Col = C32.d2c(4288935380U);
				if ((this.mpk_flags & UIStatus.KF.HOLD) != UIStatus.KF.NONE && X.ANMT(2, 5f) == 1)
				{
					this.MdGageT.Col = C32.d2c(4289265660U);
				}
				num5 = this.Chr.DrawStringTo(null, stb, 0f, 0f, ALIGN.LEFT, ALIGNY.TOP, false, 0f, 0f, null) * 0.5f;
				this.bounds_mp_w = X.Mx(num5, this.bounds_mp_w);
				num += (this.bounds_mp_w - -20f) * 0.5f - num5 * 0.5f;
				num += this.Chr.DrawScaleStringTo(this.MdGageT, stb, num, num2, 0.5f, 0.5f, ALIGN.LEFT, ALIGNY.BOTTOM, false, 0f, 0f, null);
			}
			else
			{
				stb.Set((int)this.Pr.getCastableMp());
				stb.Add("/");
				int status_reduce_max = this.Pr.EggCon.status_reduce_max;
				int num7 = (int)(this.Pr.get_maxmp() - (float)this.Pr.EggCon.status_reduce_max);
				num4 = ((this.mpk_flags == UIStatus.KF.NONE) ? 0 : X.ANMT(2, 5f));
				if ((this.mpk_flags & UIStatus.KF.DAMAGE) != UIStatus.KF.NONE)
				{
					c.Set((num4 == 0) ? 4292707974U : 4291123031U);
				}
				else if ((this.mpk_flags & UIStatus.KF.REDUCE_SPILIT) != UIStatus.KF.NONE)
				{
					c.Set((num4 == 0) ? 4293382318U : 4289835441U);
				}
				else if ((this.mpk_flags & UIStatus.KF.OC_HOLDING_ANIMATE) != UIStatus.KF.NONE)
				{
					c.Set((num4 == 0) ? 4283957152U : 4278491495U);
				}
				else if ((this.mpk_flags & UIStatus.KF.HUNGER) != UIStatus.KF.NONE && (this.mpk_flags & UIStatus.KF.HOLD) != UIStatus.KF.NONE)
				{
					c.Set((num4 == 0) ? 4294965569U : 4284462711U);
				}
				else if ((this.mpk_flags & UIStatus.KF.HOLD) != UIStatus.KF.NONE)
				{
					c.Set((num4 == 0) ? (((this.mpk_flags & UIStatus.KF.REDUCE_SPILIT) != UIStatus.KF.NONE) ? 4286091150U : 4289265660U) : 4281755135U);
				}
				else if ((this.mpk_flags & UIStatus.KF.CURE) != UIStatus.KF.NONE)
				{
					c.Set((num4 == 0) ? 4288741359U : uint.MaxValue);
				}
				else
				{
					c.Set(uint.MaxValue);
				}
				if ((this.mpk_flags & UIStatus.KF.FINISH_FADE) != UIStatus.KF.NONE)
				{
					this.MdGageT.Col = this.MdGageT.ColGrd.blend(uint.MaxValue, X.ZLINE(4f - this.cm_anim_t, 4f)).C;
				}
				else
				{
					this.MdGageT.Col = c.C;
				}
				int length = stb.Length;
				stb += num7;
				num5 = this.Chr.DrawStringTo(null, stb, 0f, 0f, ALIGN.LEFT, ALIGNY.TOP, false, 0f, 0f, null) * 0.5f;
				this.bounds_mp_w = X.Mx(num5 + -20f, this.bounds_mp_w);
				stb.Length = length;
				num += (this.bounds_mp_w - -20f) * 0.5f - num5 * 0.5f;
				num += this.Chr.DrawScaleStringTo(this.MdGageT, stb, num, num2, 0.5f, 0.5f, ALIGN.LEFT, ALIGNY.BOTTOM, false, 0f, 0f, null);
				if (status_reduce_max > 0)
				{
					if ((this.mpk_flags & UIStatus.KF.EGGED_BLINK) != UIStatus.KF.NONE && num4 == 1)
					{
						c.Set(4289168289U);
						this.MdGageT.Col = c.C;
					}
					if ((this.mpk_flags & (UIStatus.KF)(-257)) == UIStatus.KF.NONE || num4 == 0)
					{
						c.Set(4291276483U);
						this.MdGageT.Col = c.C;
					}
				}
				this.Chr.DrawScaleStringTo(this.MdGageT, stb.Set(num7), num, num2, 0.5f, 0.5f, ALIGN.LEFT, ALIGNY.BOTTOM, false, 0f, 0f, null);
			}
			TX.ReleaseBld(stb);
			if (num3 < this.bounds_mp_w)
			{
				flag = true;
			}
			return flag;
		}

		private void MHBar(MeshDrawer Md, ref bool drawn_icon, bool draw_only_marker = false, float tz = 1f)
		{
			float num = 0f;
			float num2 = 1f;
			float num3 = 12f;
			uint num4 = 1426063360U;
			bool flag = false;
			bool flag2 = false;
			float num6;
			float num5;
			float num7;
			float num8;
			float num10;
			float num9;
			float num11;
			C32 c;
			UIStatus.KF kf;
			if (Md == this.MdH)
			{
				num5 = (num6 = this.hp_cenx + this.QuH.x * 64f);
				num7 = this.hp_ceny + this.QuH.y * 64f - num3;
				num8 = this.cushion_hp;
				num9 = (num10 = this.hp_ratio);
				num2 = -1f;
				num3 *= -1f;
				num11 = (float)((int)this.Pr.GSaver.GsHp.saved_gauge_value) - this.Pr.get_hp();
				if (num11 > 0f)
				{
					num11 /= this.Pr.get_maxhp();
				}
				Md.Col = C32.d2c(3150220716U);
				c = Md.ColGrd.Set(3152651967U);
				kf = this.hpk_flags;
			}
			else
			{
				num5 = (num6 = this.mp_cenx + this.QuM.x * 64f);
				num7 = this.mp_ceny + this.QuM.y * 64f;
				num8 = this.cushion_mp;
				num = (float)this.mp_hold;
				if (num != 0f)
				{
					num /= this.Pr.Skill.getMaxMpForUi();
				}
				num9 = (num10 = X.Mx(0f, this.mp_ratio));
				kf = this.mpk_flags;
				if (this.draw_puzzle_mp)
				{
					num11 = 0f;
					num4 = 2568301660U;
					Md.Col = C32.d2c(3721654783U);
					c = Md.ColGrd.Set(3714037745U);
					if (PUZ.IT.puzz_magic_count_max == 0)
					{
						if (!draw_only_marker)
						{
							Md.ColGrd.mulA(0f);
							Md.BlurLine(num5, num7, num5 + 140f * num2, num7 - num3, 8f, 3, 4f, false);
						}
						flag2 = true;
					}
					if (PUZ.IT.puzz_magic_count_max == -1)
					{
						flag = true;
						num = 0f;
						kf &= (UIStatus.KF)(-34);
					}
				}
				else
				{
					num11 = (float)((int)this.Pr.GSaver.GsMp.saved_gauge_value) - this.Pr.getCastableMp();
					if (num11 > 0f)
					{
						num11 /= this.Pr.get_maxmp();
					}
					Md.Col = C32.d2c(3148333542U);
					c = Md.ColGrd.Set(3146093241U);
				}
			}
			if (!flag2)
			{
				if ((kf & UIStatus.KF.FINISH_FADE) == UIStatus.KF.NONE)
				{
					this.MdIconT.Col = MTRX.ColWhite;
					if ((kf & UIStatus.KF.HOLD) != UIStatus.KF.NONE && (kf & UIStatus.KF.OVERHOLD) != UIStatus.KF.NONE)
					{
						num10 += num;
						this.MdIconT.Col = C32.d2c(4293017921U);
					}
					num10 += (((kf & UIStatus.KF.DAMAGE) != UIStatus.KF.NONE && (kf & UIStatus.KF.HOLD_RELEASE) == UIStatus.KF.NONE) ? (-num8) : 0f);
					if ((kf & UIStatus.KF.HOLD) == UIStatus.KF.NONE && (kf & (UIStatus.KF)12) == UIStatus.KF.NONE)
					{
						num10 = -1f;
					}
				}
				else
				{
					num10 = -2f;
				}
				if (num10 != -1f)
				{
					if (!drawn_icon)
					{
						drawn_icon = true;
						this.MaIconTCrack.revertVerAndTriIndexSaved(false);
					}
					if (num10 >= 0f)
					{
						this.MdIconT.initForImg(MTR.SqUiBarBright.getImage(X.ANMT(3, 2f), 0), 0);
						this.MdIconT.DrawCen(num5 + 140f * num10 * num2, num7 - num3 * 0.5f, null);
					}
					this.drawn_icont = true;
				}
				if (draw_only_marker)
				{
					return;
				}
				if (Md == this.MdM && this.egg_need_draw)
				{
					this.drawMpEgg(num5, num7);
				}
				float num12 = (float)X.IntC(140f * (num9 - ((num8 > 0f) ? num8 : 0f)) * tz) * num2;
				if (num12 != 0f)
				{
					Md.Tri(0).Tri(1).Tri(3)
						.Tri(1)
						.Tri(2)
						.Tri(3);
					Md.PosD(num5, num7, null).PosD(num5 + num12, num7, null).PosD(num5 + num12, num7 - num3, c)
						.PosD(num5, num7 - num3, c);
					if ((kf & UIStatus.KF.PUZZLE_MANAGING_MP) != UIStatus.KF.NONE)
					{
						float num13 = num2 * 14f;
						float num14 = (float)(((num3 > 0f) ? 1 : (-1)) * 14);
						Md.Col = c.mulA(0.5f).C;
						c.setA(0f);
						Md.Tri(4, 5, 0, false).Tri(0, 5, 1, false).Tri(1, 5, 6, false)
							.Tri(1, 6, 2, false)
							.Tri(2, 6, 7, false)
							.Tri(3, 2, 7, false)
							.Tri(4, 0, 3, false)
							.Tri(4, 3, 7, false);
						Md.PosD(num5, num7, null).PosD(num5 + num12, num7, null).PosD(num5 + num12, num7 - num3, null)
							.PosD(num5, num7 - num3, null);
						Md.PosD(num5 - num13, num7 + num14, c).PosD(num5 + num12 + num13, num7 + num14, c).PosD(num5 + num12 + num13, num7 - num3 - num14, c)
							.PosD(num5 - num13, num7 - num3 - num14, c);
					}
					if (flag)
					{
						Md.Col = C32.d2c(uint.MaxValue);
						Md.StripedRectBL(num5, num7 - num3, num12, num3, X.ANMPT(36, 1f), 0.5f, 4f, false);
					}
				}
				num5 += num12;
				float num15 = 0f;
				float num16 = 0f;
				float num17 = num5;
				if (num11 > 0f)
				{
					num15 = (float)X.IntC(140f * num11 * tz) * num2;
					if (((Md == this.MdH) ? this.t_hit_gsaver_hp : this.t_hit_gsaver_mp) > 0f)
					{
						Md.Col = c.Set(3724283149U).C;
					}
					else
					{
						Md.Col = c.Set(3144247657U).C;
						c.mulA(0.5f);
					}
					Md.Tri(0).Tri(1).Tri(3)
						.Tri(1)
						.Tri(2)
						.Tri(3);
					Md.PosD(num5, num7, c).PosD(num5 + num15, num7, c).PosD(num5 + num15, num7 - num3, null)
						.PosD(num5, num7 - num3, null);
				}
				if (num8 != 0f)
				{
					num16 = (num12 = (float)X.IntC(140f * X.Abs(num8) * tz) * num2);
					Md.Col = this.C.Set((num8 < 0f) ? 4289200128U : 4293984247U).blend((num8 < 0f) ? 4284683302U : uint.MaxValue, 0.5f + 0.5f * X.COSIT(50f)).C;
					Md.Tri(0).Tri(1).Tri(3)
						.Tri(1)
						.Tri(2)
						.Tri(3);
					Md.PosD(num5, num7, null).PosD(num5 + num12, num7, null).PosD(num5 + num12, num7 - num3, null)
						.PosD(num5, num7 - num3, null);
					num17 += num12;
				}
				num12 = 0f;
				if (num > 0f)
				{
					num12 = (float)X.IntC(140f * num * tz) * num2;
					if ((kf & UIStatus.KF.PUZZLE_MANAGING_MP) != UIStatus.KF.NONE)
					{
						Md.Col = Md.ColGrd.Set(3714037745U).mulA(0.8f).C;
						Md.StripedRectBL(num17, num7 - num3, num12, num3, X.ANMPT(36, 1f), 0.5f, 4f, false);
					}
					else
					{
						uint num18 = (((kf & UIStatus.KF.OVERHOLD) != UIStatus.KF.NONE) ? 3154096072U : 3150638820U);
						uint num19 = (((kf & UIStatus.KF.OVERHOLD) != UIStatus.KF.NONE) ? 3152740426U : 3145047468U);
						float num20 = 1f - X.ANMPT(45, 1f);
						for (int i = 0; i < 3; i++)
						{
							int num21 = i * 2;
							Md.Tri(num21).Tri(num21 + 2).Tri(num21 + 1)
								.Tri(num21 + 1)
								.Tri(num21 + 2)
								.Tri(num21 + 3);
						}
						Md.Col = this.C.Set(num19).blend(num18, 2f * X.Abs(num20 - 0.5f)).C;
						Md.PosD(num17, num7, null).PosD(num17, num7 - num3, null);
						float num22 = ((num20 >= 0.5f) ? (num20 - 0.5f) : num20);
						Md.Col = this.C.Set((num20 >= 0.5f) ? num19 : num18).C;
						Md.PosD(num17 + num12 * num22, num7, null).PosD(num17 + num12 * num22, num7 - num3, null);
						num22 = ((num20 >= 0.5f) ? num20 : (num20 + 0.5f));
						Md.Col = this.C.Set((num20 >= 0.5f) ? num18 : num19).C;
						Md.PosD(num17 + num12 * num22, num7, null).PosD(num17 + num12 * num22, num7 - num3, null);
						float num23 = 1f - num20;
						Md.Col = this.C.Set(num19).blend(num18, 2f * X.Abs(num23 - 0.5f)).C;
						Md.PosD(num17 + num12, num7, null).PosD(num17 + num12, num7 - num3, null);
					}
				}
				num12 = ((num2 < 0f) ? X.Mn(num15, num12 + num16) : X.Mx(num15, num12 + num16));
				num5 += num12;
				num12 = num6 + 140f * num2 - num5;
				if (X.Abs(num12) > 0f)
				{
					Md.Col = C32.d2c(4278190080U);
					c = Md.ColGrd.Set(num4);
					Md.Tri(0).Tri(1).Tri(3)
						.Tri(1)
						.Tri(2)
						.Tri(3);
					Md.PosD(num5, num7, null).PosD(num5 + num12 + 1f, num7, null).PosD(num5 + num12 + 1f, num7 - num3, c)
						.PosD(num5, num7 - num3, c);
				}
			}
			else if (draw_only_marker)
			{
				return;
			}
			if (Md == this.MdH)
			{
				this.redraw_hp = false;
				this.drawn_hp = true;
				return;
			}
			this.redraw_mp = false;
			this.drawn_mp = true;
		}

		private void drawMpEgg()
		{
			this.drawMpEgg(this.mp_cenx + this.QuM.x * 64f, this.mp_ceny + this.QuM.y * 64f);
		}

		private void drawMpEgg(float x, float y)
		{
			if (!this.draw_puzzle_mp)
			{
				this.MdMpEgg.clear(false, false);
				this.Pr.EggCon.DrawTo(this.MdMpEgg, Mathf.Floor(x), Mathf.Floor(y) - 10f, 140f, 10f, this.cushion_mp);
			}
			this.redraw_mpegg = false;
			this.drawn_mpegg = true;
		}

		public float ser_first_x
		{
			get
			{
				return this.hp_cenx - 20f - (float)((int)(this.bounds_hp_w / 2f)) - 28f;
			}
		}

		public void drawSerIcons()
		{
			MeshDrawer mdGageT = this.MdGageT;
			int vertexMax = this.MdGageT.getVertexMax();
			int triMax = this.MdGageT.getTriMax();
			this.MaGageTM.revertVerAndTriIndexSaved(false);
			int length = this.MaGageTS.Length;
			this.MaGageTS.Set(true);
			float num = this.ser_first_x;
			float num2 = -this.mp_ceny - 9.5f;
			mdGageT.Identity();
			for (int i = 0; i < this.ser_max; i++)
			{
				UIStatus.SerFrame serFrame = this.Aser[i];
				PxlFrame pxlFrame = serFrame.PF;
				float num3 = 0f;
				float num4 = 10f;
				mdGageT.Col = MTRX.ColWhite;
				if (serFrame.t >= 0f)
				{
					if (serFrame.t <= 4f)
					{
						pxlFrame = MTRX.AUiSerIcon[0];
					}
					float num5 = 16f;
					float num6 = 0.5f;
					if (serFrame.t >= num5)
					{
						float num7 = serFrame.t - num5;
						if (num7 <= 3f)
						{
							pxlFrame = MTRX.AUiSerIcon[0];
						}
						if (num7 <= 7f)
						{
							num3 = (0.04f + 0.008f * X.COSI(serFrame.t, 6.72f)) * 3.1415927f;
							if (num7 >= 3f)
							{
								num6 = 1f;
							}
						}
					}
					else
					{
						num6 = 0.5f * (1f + 3f * X.ZLINE(num5 - serFrame.t, num5));
					}
					mdGageT.RotaPF(num, num2, num6, num6, num3, pxlFrame, false, false, false, uint.MaxValue, false, 0);
				}
				else
				{
					float num8 = -serFrame.t;
					if (num8 < 20f)
					{
						float num9 = X.ZLINE(num8, 20f);
						mdGageT.Col = MTRX.cola.White().mulA(1f - num9).C;
						num3 = X.ZCOS(num9) * 0.12f * 3.1415927f;
						mdGageT.RotaPF(num, num2 - X.ZPOW(num9) * 10f, 0.5f, 0.5f, num3, pxlFrame, false, false, false, uint.MaxValue, false, 0);
					}
					num4 *= serFrame.x_shift_level;
				}
				num -= num4;
			}
			mdGageT.Col = MTRX.ColWhite;
			this.MaGageTS.Set(false);
			this.drawn_gage_t = true;
			if (this.MaGageTS.Length != length || vertexMax <= this.MdGageT.getVertexMax())
			{
				this.redraw_bar_num = true;
				return;
			}
			this.MdGageT.revertVerAndTriIndex(vertexMax, triMax, false);
		}

		public void SerFrameSpliceFrom(int i)
		{
			while (i < this.ser_max)
			{
				UIStatus.SerFrame serFrame = this.Aser[i];
				if (serFrame != null)
				{
					serFrame.draw_rest_lv = -1f;
				}
				i++;
			}
		}

		public bool drawSerRestTime()
		{
			bool flag = this.redraw_ser_rest;
			float num = this.ser_first_x - 4f;
			float num2 = -this.mp_ceny - 15.5f;
			bool flag2 = flag;
			if (!flag2)
			{
				for (int i = 0; i < this.ser_max; i++)
				{
					UIStatus.SerFrame serFrame = this.Aser[i];
					M2SerItem serItm = serFrame.SerItm;
					float num3 = (float)X.IntC(serItm.time_level_ui * 8f) / 8f;
					if (serFrame.draw_rest_lv < 0f || serFrame.is_fill != UIStatus.SerFrame.isFill(num3))
					{
						flag2 = true;
						break;
					}
					if (serFrame.draw_rest_lv != serItm.time_level_ui)
					{
						flag2 = true;
						break;
					}
				}
			}
			if (flag2)
			{
				this.MdSerRestTime.resetVertexCount();
				for (int j = 0; j < this.ser_max; j++)
				{
					UIStatus.SerFrame serFrame2 = this.Aser[j];
					float num4 = (float)X.IntC(serFrame2.SerItm.time_level_ui * 8f) / 8f;
					flag = true;
					serFrame2.draw_rest_lv = num4;
					int num5 = (int)num4;
					float x_shift_level = serFrame2.x_shift_level;
					this.MdSerRestTime.Col = C32.MulA(4283780170U, x_shift_level);
					this.MdSerRestTime.RectBL(num - 1f, num2 - 1f, 10f, 3f, false);
					float num6;
					if (serFrame2.is_fill)
					{
						num6 = ((num4 >= 1f) ? 1f : num4);
						this.MdSerRestTime.Col = UIStatus.getSerRestTimeCol(this.MdSerRestTime.ColGrd, num5 - 1).mulA(x_shift_level).C;
					}
					else
					{
						this.MdSerRestTime.Col = UIStatus.getSerRestTimeCol(this.MdSerRestTime.ColGrd, num5 - 1).mulA(x_shift_level).C;
						this.MdSerRestTime.RectBL(num, num2, 8f, 1f, false);
						num6 = X.frac(num4);
						this.MdSerRestTime.Col = UIStatus.getSerRestTimeCol(this.MdSerRestTime.ColGrd, num5).mulA(x_shift_level).C;
					}
					float num7 = (float)X.IntC(8f * num6 * 2f) * 0.5f;
					this.MdSerRestTime.RectBL(num + 8f - num7, num2, num7, 1f, false);
					num -= 10f * x_shift_level;
				}
			}
			else if (this.MdSerRestTime.getVertexMax() > 0 && this.ser_max == 0)
			{
				this.MdSerRestTime.resetVertexCount();
				flag = true;
			}
			return flag;
		}

		public static C32 getSerRestTimeCol(C32 C, int level)
		{
			if (level < 0)
			{
				level = 0;
			}
			switch (level)
			{
			case 0:
				C.Set(4294953540U);
				return C;
			case 1:
				C.Set(4294923333U);
				return C;
			case 2:
				C.Set(4293332711U);
				return C;
			case 3:
				C.Set(4288155368U);
				return C;
			case 4:
				C.Set(4280694713U);
				return C;
			case 5:
				C.Set(4283143836U);
				return C;
			default:
				return UIStatus.getSerRestTimeCol(C, level - 5).blend(4278190080U, 0.45f);
			}
		}

		private bool drawOcSlots(bool draw_effect = false)
		{
			M2PrOverChargeSlot overChargeSlots = this.Pr.getSkillManager().getOverChargeSlots();
			float num = this.mp_cenx + 28f + (float)((int)(this.bounds_mp_w / 2f)) + 28f;
			float num2 = -this.mp_ceny - 11.5f;
			if (draw_effect)
			{
				return overChargeSlots.drawEffectTo(this.MdAdd, num, num2);
			}
			int length = this.MaIconTOc.Length;
			int vertexMax = this.MdIconT.getVertexMax();
			int triMax = this.MdIconT.getTriMax();
			this.MdIconT.revertVerAndTriIndex(0, 0, false);
			this.MaIconTOc.Set(true);
			if (overChargeSlots.drawTo(this.MdIconT, num, num2))
			{
				this.fine_gage_special = true;
			}
			this.drawn_icont = true;
			this.MaIconTOc.Set(false);
			if (this.MaIconTOc.Length != length || vertexMax <= this.MdIconT.getVertexMax())
			{
				this.draw_crack = true;
			}
			else
			{
				this.MdIconT.revertVerAndTriIndex(vertexMax, triMax, false);
			}
			return false;
		}

		public void initCrack()
		{
			this.crack_t = 0;
			this.draw_crack = true;
			this.crack_cure_count = 0;
		}

		public void quitCrack()
		{
			this.draw_crack = true;
			this.crack_cure_count = 0;
		}

		public void initHpCrack(bool cure = false)
		{
			this.draw_crack = true;
			if (!cure)
			{
				this.hcrk_t = 0;
				this.QuH.Vib(13f, 10f, 9f, 0);
			}
		}

		public void initCrackCure(int _crack_cure_count)
		{
			this.crack_t = -2;
			this.crack_cure_count = (byte)_crack_cure_count;
			this.draw_crack = true;
		}

		private bool drawCrack()
		{
			MpGaugeBreaker gaugeBrk = this.Pr.GaugeBrk;
			bool flag = false;
			this.MaIconTOc.revertVerAndTriIndexSaved(false);
			this.MaIconTCrack.Set(true);
			this.drawn_icont = true;
			if (!gaugeBrk.isActive() || this.draw_puzzle_mp)
			{
				this.crack_t = -1;
			}
			else
			{
				MeshDrawer mdIconT = this.MdIconT;
				float num = this.mp_cenx + this.QuM.x * 64f + 140f;
				float num2 = this.mp_ceny + this.QuM.y * 64f - 10f;
				float num3 = num2 + 5f;
				bool flag2 = 0 <= this.crack_t && this.crack_t < 6;
				for (int i = 0; i < 3; i++)
				{
					int breakLevel = gaugeBrk.getBreakLevel(i);
					float num4 = gaugeBrk.getBreakWidth(i) * 140f;
					int num5 = X.IntR(num4 / 18f + 0.1f);
					float num6 = num - num4 + 9f;
					mdIconT.Col = mdIconT.ColGrd.Set(flag2 ? 4294901760U : uint.MaxValue).C;
					int num7 = breakLevel - (flag2 ? 0 : gaugeBrk.getCureCount(i));
					for (int j = 0; j < breakLevel; j++)
					{
						if (!flag2 && j == num7)
						{
							mdIconT.Col = C32.d2c(2009179881U);
						}
						for (int k = 0; k < num5; k++)
						{
							mdIconT.initForImg(MTR.SqGageCrack.getImage(j % 5, i), 0);
							mdIconT.RotaGraph(num6 + (float)(k * 18), num3, 1f, (j / 5 % 2 == 1) ? 3.1415927f : 0f, null, false);
						}
						num6 -= num4;
					}
				}
				if (this.crack_t != -1)
				{
					if (this.crack_t >= 0 && this.crack_t <= 50)
					{
						float breakWidth = gaugeBrk.getBreakWidth(-1);
						mdIconT.initForImg(MTRX.IconWhite, 0);
						num = this.mp_cenx + this.QuM.x * 64f + 140f * gaugeBrk.getBreakDep(-1, true);
						this.drawCrackPoly(mdIconT, num, num2, breakWidth * 140f, this.crack_t);
						this.crack_t += X.AF;
						if (this.crack_t >= 50)
						{
							this.crack_t = -1;
						}
					}
					else
					{
						float num8 = X.ZLINE((float)(-(float)this.crack_t - 2), 50f);
						mdIconT.ColGrd.Set(4290881257U);
						int currentCuredIndex = gaugeBrk.getCurrentCuredIndex();
						float num9 = gaugeBrk.getBreakWidth(currentCuredIndex) * (float)this.crack_cure_count;
						num = this.mp_cenx + this.QuM.x * 64f + 140f * gaugeBrk.getBreakDep(currentCuredIndex, true) - num9 * 140f;
						mdIconT.initForImg(MTRX.IconWhite, 0);
						mdIconT.Col = mdIconT.ColGrd.mulA(1f - num8).C;
						mdIconT.RectBL(num, num2, 140f * num9, 10f, false);
						num += 140f * num9 * 0.5f;
						mdIconT.Col = mdIconT.ColGrd.setA1(1f - X.ZLINE(num8 - 0.6f, 0.39999998f)).C;
						for (int l = (int)(8 * this.crack_cure_count); l > 0; l--)
						{
							uint ran = X.GETRAN2(l * 11, l + 5);
							float num10 = X.NI(-0.6f, 0.6f, X.RAN(ran, 1460)) * num9 * 140f;
							float num11 = X.NI(0.4f, 0.7f, X.RAN(ran, 413)) + 10f * X.NI(0.5f, 1.2f, X.RAN(ran, 1103)) * num8;
							mdIconT.initForImg(MTRX.SqDotKira.getImage((int)((ulong)ran % (ulong)((long)MTRX.SqDotKira.countFrames())), 0), 0);
							mdIconT.DrawCen(num + num10, num2 + num11, null);
						}
						this.crack_t -= X.AF;
						if (this.crack_t <= -52)
						{
							this.crack_cure_count = 0;
							this.crack_t = -1;
						}
					}
					flag = true;
				}
			}
			flag = this.drawHpCrack() || flag;
			this.MaIconTCrack.Set(false);
			return flag;
		}

		private bool drawHpCrack()
		{
			if (this.Pr.DMG.hp_crack <= 0)
			{
				return false;
			}
			bool flag = false;
			bool flag2 = 0 <= this.hcrk_t && this.hcrk_t < 6;
			float num = this.hp_cenx + this.QuH.x * 64f;
			float num2 = this.hp_ceny + this.QuH.y * 64f - 10f;
			float num3 = num2 + 5f;
			MeshDrawer meshDrawer = this.MdIconT.Identity();
			int num4 = 0;
			for (int i = 0; i < this.Pr.DMG.hp_crack; i++)
			{
				float num5;
				float num6;
				int num7;
				if (i < 3)
				{
					num5 = -50f + (float)i * 140f * 0.2f * 0.666f;
					num6 = -28f;
					num7 = 4;
				}
				else
				{
					num6 = -14f;
					num5 = -4f + ((i == 3) ? (num6 * 0.5f) : 0f);
					num7 = 9;
				}
				num5 += num;
				int num8 = i % 3;
				bool flag3 = i == this.Pr.DMG.hp_crack - 1;
				meshDrawer.Col = meshDrawer.ColGrd.Set(flag2 ? 4294901760U : uint.MaxValue).C;
				for (int j = 0; j < num7; j++)
				{
					meshDrawer.initForImg(MTR.SqGageCrack.getImage(num4 % 5, num8), 0);
					meshDrawer.RotaGraph(num5, num3, 1f, (num4 / 5 % 2 == 1) ? 3.1415927f : 0f, null, false);
					if (flag3 && this.hcrk_t >= 0)
					{
						this.drawCrackPoly(meshDrawer, num5 - 9f, num2, 18f, this.hcrk_t);
						meshDrawer.Col = meshDrawer.ColGrd.C;
					}
					num5 += num6;
					num4++;
				}
			}
			if (this.hcrk_t >= 0)
			{
				flag = true;
				this.hcrk_t += X.AF;
				if (this.hcrk_t >= 50)
				{
					this.hcrk_t = -1;
				}
			}
			return flag;
		}

		private void drawCrackPoly(MeshDrawer Md, float x, float y, float brk_width, int crack_t)
		{
			if (crack_t <= 6)
			{
				Md.Col = C32.d2c(4294901760U);
				Md.RectBL(x, y, brk_width, 10f, false);
			}
			else
			{
				x += brk_width * 0.5f;
				int num = 14;
				float num2 = X.ZLINE((float)(crack_t - 6), 44f);
				float num3 = num2 * num2;
				Md.Col = MTRX.cola.White().setA(1f - X.ZLINE(num2 - 0.6f, 0.4f)).C;
				for (int i = 0; i < num; i++)
				{
					uint ran = X.GETRAN2(i * 7, i + 3);
					float num4 = x + X.NI(-10, 10, X.RAN(ran, 2873));
					float num5 = X.NI(-10, 10, X.RAN(ran, 2145)) + X.NI(3, 7, X.RAN(ran, 1669)) * (float)X.MPF(num4 > x);
					float num6 = X.NI(-40, -60, X.RAN(ran, 2353));
					Md.Rect(num4 + num5 * num2, y + num6 * num3, 1f, 1f, false);
				}
				Md.Col = C32.d2c(4294901760U);
				num2 = X.ZSIN((float)(crack_t - 6), 13f);
				if (num2 < 1f)
				{
					for (int j = 0; j < 9; j++)
					{
						uint ran2 = X.GETRAN2(j * 11, j + 5);
						float num7 = X.NI(0.3f, 0.5f, X.RAN(ran2, 2873)) * (float)X.MPF(X.RAN(ran2, 867) > 0.5f) * brk_width;
						float num8 = X.NI(0.4f, 0.7f, X.RAN(ran2, 413)) * (float)X.MPF(X.RAN(ran2, 2540) > 0.5f) * 10f;
						float num9 = num7 * X.NI(1.2f, 1.6f, X.RAN(ran2, 2421)) * num2;
						float num10 = num8 * X.NI(1.7f, 2.1f, X.RAN(ran2, 409)) * num2;
						float num11 = (1f - num2) * X.NI(4, 7, X.RAN(ran2, 1935));
						Md.Poly(x + num7 + num9, y + num8 + num10, num11, 0f, 4, 0f, false, 0f, 0f);
					}
				}
			}
			Md.Col = MTRX.ColWhite;
		}

		public GameObject GetGob()
		{
			return this.Gob;
		}

		public bool draw_puzzle_mp
		{
			get
			{
				return (this.mpk_flags & UIStatus.KF.PUZZLE_MANAGING_MP) > UIStatus.KF.NONE;
			}
		}

		public bool egg_need_draw
		{
			get
			{
				return (this.mpk_flags & UIStatus.KF.PUZZLE_MANAGING_MP) == UIStatus.KF.NONE && this.Pr.EggCon.isActive();
			}
		}

		private bool isActive()
		{
			return this.t >= 0f;
		}

		public static bool PrIs(PR Pr)
		{
			return UIStatus.isPr(Pr);
		}

		public bool isPlayerAssigned()
		{
			return this.Pr != null;
		}

		public static bool isPr(PR Pr)
		{
			return UIStatus.Instance != null && UIStatus.Instance.Pr == Pr;
		}

		public static void showHold(int t, bool execute_if_confusing = false)
		{
			if (!execute_if_confusing && UIStatus.Instance.ConfuseCurtain != null)
			{
				return;
			}
			UIStatus.Instance.ui_hold_time = X.Mx(UIStatus.Instance.ui_hold_time, (float)t);
		}

		private int checkPlayerActivation()
		{
			PR pr = this.Pr;
			if (this.Pr == null || this.ConfuseCurtain != null)
			{
				return 0;
			}
			if (pr.isBusySituation())
			{
				return 2;
			}
			int num = (((double)X.LENGTHXYS(this.pre_x, this.pre_y, pr.x, pr.y) < 0.125) ? 1 : 0);
			this.pre_x = pr.x;
			this.pre_y = pr.y;
			return num;
		}

		private readonly UIBase Base;

		public static UIStatus Instance;

		public float gage_tz;

		private MeshDrawer MdBase;

		private MdArranger MaBaseH;

		private MdArranger MaGageTH;

		private MdArranger MaBaseM;

		private MdArranger MaGageTM;

		private MdArranger MaGageTS;

		private MeshDrawer MdAdd;

		private MdArranger MaIconTOc;

		private MdArranger MaIconTCrack;

		private MeshDrawer MdH;

		private MeshDrawer MdM;

		private MeshDrawer MdMpEgg;

		private MeshDrawer MdGageT;

		private MeshDrawer MdIconT;

		private MeshDrawer MdSerRestTime;

		private GameObject Gob;

		private GameObject GobA;

		private BMListChars Chr;

		private float t;

		private float ui_hold_time;

		private const int UI_HOLD_MAXT = 150;

		private const int POS_FADE_T = 60;

		private const int POS_FADE_WAIT_T = 60;

		private const int POS_HIDE_WAIT_T = 10;

		private C32 C;

		private C32 CIn;

		public const uint hpcol = 3152651967U;

		public const uint mpcol = 3148333542U;

		public const uint hpcol_b = 3150220716U;

		public const uint mpcol_b = 3146093241U;

		public const uint mpcol_puzzle = 3721654783U;

		public const uint mpcol_puzzle_b = 3714037745U;

		public const uint bar_bg_col = 4278190080U;

		public const uint bar_bg_col_b = 1426063360U;

		public const uint bar_bg_col_b_puzzle = 2568301660U;

		public ulong ser_bits;

		public float cushion_hp;

		public float cushion_mp;

		public float t_hit_gsaver_hp;

		public float t_hit_gsaver_mp;

		public float ch_anim_t;

		public float cm_anim_t;

		public Quaker QuH;

		public Quaker QuM;

		private float hp_ratio;

		private float mp_ratio;

		private int mp_hold;

		private float bounds_hp_w;

		private float bounds_mp_w;

		private const float wh_bar = 10f;

		public const float barlen = 140f;

		private const float numlen = 48f;

		private const float bounds_num_margin = -20f;

		private uint redraw_flags;

		private const int CUSHION_FADE_T = 4;

		public const float gage_set_pixel_y = 80f;

		private const float pinch_ratio = 0.33f;

		public int crack_t = -1;

		public float pre_x;

		public float pre_y;

		public int hcrk_t = -1;

		public int anm_delay_mphold;

		public int anm_delay_barnum;

		public const int ANM_SPEED = 5;

		private byte crack_cure_count;

		private int reset_flags;

		private float base_y_level = 1f;

		private ConfuseDrawer ConfuseCurtain;

		private UISpecialGage O2Gage;

		public static readonly Flagger FlgStatusHide = new Flagger(null, null);

		private UIStatus.KF mpk_flags;

		private UIStatus.KF hpk_flags;

		private UIStatus.SerFrame[] Aser;

		public int ser_max;

		public const float SER_FADE_T = 40f;

		public const float SER_APPEAR_T = 22f;

		public const float SER_SND_T = 16f;

		private const float ser_rest_gage_width = 8f;

		private PR Pr;

		private const float ser_icon_shift_x = 10f;

		private const int CRACK_ANIM_MAXT = 50;

		private const int CRACK_REDFILL_T = 6;

		private const int CRACK_W = 18;

		private const int CRACK_WH = 9;

		private class SerFrame
		{
			public bool is_fill
			{
				get
				{
					return UIStatus.SerFrame.isFill(this.draw_rest_lv);
				}
			}

			public static bool isFill(float level)
			{
				return level < 1f || level == (float)((int)level);
			}

			public float x_shift_level
			{
				get
				{
					if (this.t <= -20f)
					{
						float num = X.ZLINE(-this.t - 20f, 20f);
						return 1f - (X.ZSINV(num, 0.4f) * 0.6f + X.ZSIN2(num - 0.4f, 0.6f) * 0.4f);
					}
					return 1f;
				}
			}

			public SER ser;

			public M2SerItem SerItm;

			public bool snd;

			public float t;

			public float draw_rest_lv;

			public PxlFrame PF;
		}

		public enum EGGMESH
		{
			WATER,
			ST
		}

		public enum BASEMESH
		{
			ADD,
			CONFUSE
		}

		private enum KF
		{
			NONE,
			HOLD,
			OVERHOLD,
			CURE = 4,
			DAMAGE = 8,
			HUNGER = 16,
			FINISH_FADE = 32,
			HOLD_RELEASE = 64,
			REDUCE_SPILIT = 128,
			EGGED_BLINK = 256,
			PUZZLE_MANAGING_MP = 512,
			OC_ANIMATE = 1024,
			OC_HOLDING_ANIMATE = 2048
		}
	}
}
