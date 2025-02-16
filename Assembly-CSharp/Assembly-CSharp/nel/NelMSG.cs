using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using evt;
using m2d;
using PixelLiner;
using UnityEngine;
using XX;

namespace nel
{
	public class NelMSG : MonoBehaviour, INelMSG, ConfirmAnnouncer.IConfirmHolder
	{
		public bool all_char_shown
		{
			get
			{
				return this.all_char_shown_;
			}
			set
			{
				if (this.all_char_shown_ == value)
				{
					return;
				}
				this.all_char_shown_ = value;
				if (value)
				{
					this.repositConfirmer(false);
					if (this.t_auto_progress < 0f && this.maxt_auto_progress >= 0f)
					{
						this.t_auto_progress = 0f;
					}
				}
			}
		}

		public string talker_snd
		{
			get
			{
				return this.talker_snd_;
			}
			set
			{
				this.talker_snd_ = value;
			}
		}

		private bool draw_hkds_tail
		{
			get
			{
				return (this.nodrawtail & NelMSG.NODRAW._TAIL) == NelMSG.NODRAW.NONE;
			}
		}

		private bool draw_talker
		{
			get
			{
				return (this.nodrawtail & NelMSG.NODRAW.TALKERMESH) == NelMSG.NODRAW.NONE;
			}
		}

		public static NelMSG createInstance(NelMSGContainer _Con, int count, byte _msg_index)
		{
			NelMSG nelMSG = IN.CreateGob(EV.Gob, "-Msg_" + count.ToString()).AddComponent<NelMSG>();
			nelMSG.initialize(_Con, _msg_index);
			nelMSG.gameObject.SetActive(false);
			return nelMSG;
		}

		private void initialize(NelMSGContainer _Con, byte _msg_index)
		{
			this.Con = _Con;
			this.msg_index = _msg_index;
			this.book_stabilize_id = "NelMSG-" + _msg_index.ToString();
			this.BaseMtr = MTRX.newMtr(MTRX.ShaderMeshMask);
			int num = (int)(58 + this.msg_index);
			this.initStencilRef(this.BaseMtr, false);
			this.GobMdB = IN.CreateGob(base.gameObject, "-MdB");
			this.MdB = MeshDrawer.prepareMeshRenderer(this.GobMdB, MTRX.getMtr(BLEND.NORMAL, -1), 0.0015f, -1, null, false, false);
			this.MrdB = this.GobMdB.GetComponent<MeshRenderer>();
			this.Md = MeshDrawer.prepareMeshRenderer(base.gameObject, this.BaseMtr, 0.001f, -1, null, false, false);
			this.Mrd = base.gameObject.GetComponent<MeshRenderer>();
			this.Ma = new MdArranger(this.Md);
			this.MaHkds = new MdArranger(this.Md);
			this.GobMdC = IN.CreateGob(base.gameObject, "-MdC");
			this.MdC = MeshDrawer.prepareMeshRenderer(this.GobMdC, MTRX.MIicon.getMtr(BLEND.NORMAL, num), 5E-05f, -1, null, false, false);
			this.MrdC = this.GobMdC.GetComponent<MeshRenderer>();
			this.MaC = new MdArranger(this.MdC);
			this.Tx = IN.CreateGob(base.gameObject, "-Tx").AddComponent<NelEvTextRenderer>();
			this.Tx.StencilRef(num);
			this.Tx.initNelMsg(this);
			this.Tx.aligny = ALIGNY.TOP;
			this.Tx.alignx = ALIGN.LEFT;
			this.TxCap = IN.CreateGob(this.GobMdC, "-TxCap").AddComponent<TextRenderer>();
			this.TxCap.StencilRef(num);
			this.TxCap.Size(20f);
			this.use_valotile = true;
			this.use_valotile = false;
			this.TxReposit();
		}

		public void OnDestroy()
		{
			if (this.MdB != null)
			{
				this.Md.destruct();
				this.MdB.destruct();
				this.MdC.destruct();
			}
		}

		public bool use_valotile
		{
			get
			{
				return this.TxCap.use_valotile;
			}
			set
			{
				ValotileRenderer.RecheckUse(this.Md, this.Mrd, ref this.Valot, value);
				ValotileRenderer.RecheckUse(this.MdB, this.MrdB, ref this.ValotB, value);
				ValotileRenderer.RecheckUse(this.MdC, this.MrdC, ref this.ValotC, value);
				this.Tx.use_valotile = value;
				this.TxCap.use_valotile = value;
				if (EV.Confirmer.targetIs(this))
				{
					EV.Confirmer.use_valotile = value;
				}
				NelBookDrawer bk = this.Bk;
			}
		}

		public void initStencilRef(Material Mtr, bool stencil_override = false)
		{
			int num = (int)(58 + this.msg_index);
			Mtr.SetFloat("_StencilRef", (float)num);
			Mtr.SetFloat("_StencilComp", 8f);
			if (stencil_override)
			{
				Mtr.SetFloat("_StencilOp", 2f);
			}
		}

		private void TxReposit()
		{
			float num = -this.rect_w * 0.5f;
			float num2 = 0f;
			switch (this.bounds_type)
			{
			case NelMSG.HKDS_BOUNDS.NORMAL:
				num += 45f;
				goto IL_007F;
			case NelMSG.HKDS_BOUNDS.TT:
				num += 45f;
				num2 += 4f;
				goto IL_007F;
			case NelMSG.HKDS_BOUNDS.WIDE_TT:
				num += 67.5f;
				num2 += 4f;
				goto IL_007F;
			case NelMSG.HKDS_BOUNDS.ONELINE:
				num += 12f;
				goto IL_007F;
			}
			num += 67.5f;
			IL_007F:
			IN.PosP(this.TxCap.transform, num - 12f, this.rect_h * 0.5f - 16f, -5E-05f);
			this.TxCap.max_swidth_px = -(num - 12f) * 2f;
			IN.PosP(this.Tx.transform, -num * (float)this.Tx.alignx, (-(this.rect_h * 0.5f - 30f) + 40f) * (float)this.Tx.aligny + num2, 0f);
			this.Tx.max_swidth_px = -num * 2f;
			if (this.bounds_type == NelMSG.HKDS_BOUNDS.BOOK)
			{
				this.Tx.LineSpacePixel(25f);
			}
			else
			{
				this.Tx.LineSpacing(1.35f);
			}
			this.Tx.LetterSpacing(1f).Size(20f);
			this.repositConfirmer(false);
		}

		private void repositConfirmer(bool no_setpos = false)
		{
			if (this.Confirmer.targetIs(this))
			{
				if (!no_setpos)
				{
					this.Confirmer.Pivot = this.ConfirmerPivot;
				}
				if (this.all_char_shown)
				{
					this.Confirmer.alpha_blink = true;
					this.Confirmer.base_alpha = 0.8f;
				}
				else
				{
					this.Confirmer.alpha_blink = false;
					this.Confirmer.base_alpha = 0.4f;
				}
				this.Confirmer.need_fine_position = true;
				this.Confirmer.Col((this.type == NelMSG.HKDSTYPE.BOOK) ? uint.MaxValue : 4283780170U);
				this.Confirmer.BorderCol((this.type == NelMSG.HKDSTYPE.BOOK) ? 4278190080U : 0U);
				this.Confirmer.transform.localEulerAngles = new Vector3(0f, 0f, (this.type == NelMSG.HKDSTYPE.BOOK) ? 1.87f : 0f);
				this.fineConfirmerSuffix();
			}
		}

		private void fineConfirmerSuffix()
		{
			if (!this.Tx.hasReservedContent() && this.Con.is_last_message)
			{
				this.Confirmer.content_suffix = "<shape check/>";
				return;
			}
			this.Confirmer.content_suffix = "<shape right_arrow/>";
		}

		public Vector3 ConfirmerPivot
		{
			get
			{
				return new Vector3(this.rect_w / 2f - 26f, -this.rect_h / 2f + 30f, -0.02f);
			}
		}

		public void initConfirmAnnouncer(ConfirmAnnouncer Confirmer, out string content, out ALIGN align, out ALIGNY aligny, out Vector3 Pivot)
		{
			align = ALIGN.RIGHT;
			aligny = ALIGNY.BOTTOM;
			Pivot = this.ConfirmerPivot;
			content = null;
			Confirmer.use_valotile = this.use_valotile;
			Confirmer.Size(14f);
			this.repositConfirmer(true);
		}

		public void reservePersonForNest(string hkds_id, NelMSG Src)
		{
			this.id = hkds_id;
			this.localized_talker_name_ = Src.localized_talker_name_;
			this.fineCapContent();
			if (this.t < 0f)
			{
				this.t = -30f;
			}
		}

		public NelMSG initDrawer(string _id, bool force = false)
		{
			if (_id == null)
			{
				return this;
			}
			if (this.id != _id)
			{
				force = true;
			}
			if (this.t < 0f)
			{
				this.person_index = -1;
				force = true;
				this.fix_first_pos_to_follow = 2;
			}
			if (force)
			{
				this.t = (this.t_hkds = (this.t_talker = (this.t_move = 0f)));
				this.Tx.container_TS = 1f;
				this.x = (this.depx = 0f);
				this.sx = 0;
				this.depy = 20f;
				this.y = (float)(this.sy = -20);
				this.position_fixed = null;
				this.PosMv = null;
				this.FollowTo = null;
				this.hkds_fine_agr_t = 5f;
				this.auto_hide_time = 0f;
				this.releaseDrawer(false);
				this.setBoundsType(NelMSG.HKDS_BOUNDS.NORMAL, true);
				this.nodrawtail = NelMSG.NODRAW.NONE;
				this.id = _id;
				this.t_auto_progress = -1f;
				this.maxt_auto_progress = -1f;
				this.P = null;
				this.always_focus_color = false;
				this.behind_flag = false;
				this.use_valotile = this.Con.use_valotile;
				this.Con.AddActive(this);
				if (this.Tx.hasReservedContent())
				{
					this.TxCap.alpha = 1f;
					this.Tx.alpha = 1f;
				}
				this.ran_t0 = IN.totalframe;
				base.gameObject.SetActive(true);
				if (this.nest_shuffle > 0 && this.Tx.hasReservedContent())
				{
					this.t_move = 19f;
					this.t_hkds = this.t_hkds_maxt * 0.75f;
					this.t_talker = 31f;
				}
				else
				{
					this.nest_shuffle = -1;
					this.t_nest_shuffle = -1f;
				}
			}
			return this;
		}

		public void initNestShuffle(int _nest_shuffle, float _t_nest_shuffle)
		{
			if (_nest_shuffle < 0)
			{
				this.nest_shuffle = -1;
				this.t_nest_shuffle = -1f;
				return;
			}
			this.nest_shuffle = (short)_nest_shuffle;
			this.t_nest_shuffle = _t_nest_shuffle;
			if (this.Tx.hasReservedContent() && this.nest_shuffle > 0)
			{
				this.Tx.showImmediate(true, false);
			}
			if (this.t < 0f)
			{
				this.t = -30f;
				base.gameObject.SetActive(false);
			}
		}

		public void initPerson(string _person_key, EvPerson _P)
		{
			IHkdsFollowable hkdsFollowable = null;
			if (this.P != _P || this.id != _person_key)
			{
				this.P = _P;
				this.id = _person_key;
				this.talker_snd = _P.talk_snd;
				if (_P != null)
				{
					hkdsFollowable = EV.getTalkerDrawer(this.P.key);
					this.localized_talker_name_ = this.P.localized_name;
				}
				else
				{
					this.localized_talker_name_ = null;
				}
				this.Con.need_person_reindex_flag = true;
			}
			else if (this.person_index == -1)
			{
				this.Con.need_person_reindex_flag = true;
			}
			if (hkdsFollowable != null)
			{
				this.FollowTo = hkdsFollowable;
			}
		}

		public void readInfo(NelMSGContainer.HkdsInfo Info)
		{
			if (Info == null)
			{
				return;
			}
			Info.used = true;
			this.ReservedInfo = Info;
		}

		private void readInfoInner()
		{
			if (this.ReservedInfo == null)
			{
				return;
			}
			NelMSGContainer.HkdsInfo reservedInfo = this.ReservedInfo;
			this.ReservedInfo = null;
			if (reservedInfo.follow_to_key != "=")
			{
				string text = reservedInfo.follow_to_key;
				if (TX.noe(text) && this.P != null)
				{
					text = this.P.key;
				}
				IHkdsFollowable hkdsFollowable;
				if (REG.match(text, NelMSG.RegVpTalkerPosFollow))
				{
					hkdsFollowable = TalkDrawer.getVpTalkerFirstPositionFollowable(REG.R1.ToUpper());
				}
				else
				{
					hkdsFollowable = EV.getHkdsFollowableObject(text);
				}
				if (hkdsFollowable != this.FollowTo)
				{
					this.FollowTo = hkdsFollowable;
				}
			}
			if (reservedInfo.pos_fix_key != "=")
			{
				this.initPositionFix(reservedInfo.pos_fix_key);
			}
			if (reservedInfo.bounds_key != "=")
			{
				NelMSG.HKDS_BOUNDS hkds_BOUNDS;
				if (!FEnum<NelMSG.HKDS_BOUNDS>.TryParse(reservedInfo.bounds_key.ToUpper(), out hkds_BOUNDS, true))
				{
					hkds_BOUNDS = NelMSG.HKDS_BOUNDS.NORMAL;
				}
				this.setBoundsType(hkds_BOUNDS, false);
			}
			if (reservedInfo.talker_replace_key != "=")
			{
				this.localized_talker_name_ = (TX.valid(reservedInfo.talker_replace_key) ? TX.Get("Talker_" + reservedInfo.talker_replace_key, "") : null);
			}
			if (reservedInfo.talker_snd_key != "=")
			{
				this.talker_snd = ((reservedInfo.talker_snd_key != null && reservedInfo.talker_snd_key != " ") ? reservedInfo.talker_snd_key : ((this.P != null) ? this.P.talk_snd : "talk_progress"));
			}
			if (this.person_index >= 0 != (this.DrawerPersonFollowing != null))
			{
				this.Con.need_person_reindex_flag = true;
			}
		}

		public void fineTargetFont(MFont F)
		{
			this.Tx.TargetFont = F;
			this.TxCap.TargetFont = F;
		}

		public void releaseDrawer(bool set_gob_deactive = true)
		{
			this.Md.clear(false, false);
			this.MdB.clear(false, false);
			this.MdC.clear(false, false);
			this.mesh_create_level = -1f;
			this.Tx.text_content = "";
			this.talker_snd = null;
			this.position_fixed = null;
			this.PosMv = null;
			this.t_hkds = 0f;
			this.setHkdsType(NelMSG.HKDSTYPE.NORMAL);
			this.Con.RemActive(this);
			if (this.Confirmer.targetIs(this))
			{
				this.Confirmer.deactivate();
			}
			this.PosMv = null;
			this.always_focus_color = false;
			if (this.use_valotile)
			{
				this.use_valotile = false;
			}
			if (set_gob_deactive)
			{
				this.person_index = -1;
				this.P = null;
				base.gameObject.SetActive(false);
				this.id = null;
			}
			if (this.DeviceMessageMtr != null)
			{
				IN.DestroyOne(this.DeviceMessageMtr);
				this.DeviceMessageMtr = null;
			}
			if (this.EvilMessageMtr != null)
			{
				if (M2DBase.Instance != null)
				{
					(M2DBase.Instance as NelM2DBase).ENMTR.remove(this.EvilMessageMtr);
				}
				IN.DestroyOne(this.EvilMessageMtr);
				this.EvilMessageMtr = null;
			}
		}

		public NelMSG activateFront()
		{
			if (this.t_front < 0f)
			{
				this.t_front = X.MMX(0f, 30f + this.t_front, 30f);
			}
			else
			{
				this.t_front = X.Mn(this.t_front, 30f);
			}
			return this;
		}

		public bool is_temporary_hiding
		{
			get
			{
				return this.Con.isHidingTemporary();
			}
		}

		public NelMSG deactivateFront()
		{
			if (this.t_front >= 0f)
			{
				this.t_front = X.MMX(-28f, -30f + this.t_front, -1f);
			}
			return this;
		}

		public void initPosition(float _depx, float _depy, bool start_yfix = false)
		{
			if (this.fix_first_pos_to_follow > 0)
			{
				this.fix_first_pos_to_follow -= 1;
			}
			this.depx = _depx;
			this.depy = _depy;
			if (this.nest_shuffle > 0 && this.t_nest_shuffle >= 0f)
			{
				this.t_move = ((this.t_move < 0f) ? 0f : X.Mn(this.t_move, 20f));
			}
			else
			{
				if (this.t <= 1f)
				{
					this.x = this.depx * 0.5f;
					this.y = (start_yfix ? 1f : 0.5f) * this.depy;
				}
				this.t_move = 0f;
			}
			this.sx = (int)this.x;
			this.sy = (int)this.y;
			this.reposit(true);
		}

		public void initPositionFix(string fixpos_key)
		{
			Vector4 zero = Vector4.zero;
			if (fixpos_key == "=")
			{
				return;
			}
			if (TX.noe(fixpos_key))
			{
				if (this.position_fixed != null)
				{
					this.position_fixed = null;
					this.PosMv = null;
					this.Con.need_reposit_flag = true;
				}
				return;
			}
			if (M2DBase.Instance != null && M2DBase.Instance.curMap != null && REG.match(fixpos_key, M2DBase.RegFindMover))
			{
				if (this.position_fixed == fixpos_key)
				{
					return;
				}
				M2Mover moverByName = M2DBase.Instance.curMap.getMoverByName(REG.R1, false);
				if (moverByName != null)
				{
					this.position_fixed = fixpos_key;
					this.Con.need_reposit_flag = true;
					this.posmv_top = true;
					this.PosMv = moverByName;
					this.repositFixMover(true);
					if (this.t <= 0f)
					{
						this.reposit(true);
					}
					return;
				}
				X.de("不明なフィックス先Mover:" + REG.R1, null);
				this.PosMv = null;
				return;
			}
			else
			{
				this.PosMv = null;
				if (!TalkDrawer.getDefinedPosition(fixpos_key, out zero))
				{
					X.de("フィックス先 vp_pos 不明:" + fixpos_key, null);
					return;
				}
				if (this.position_fixed == fixpos_key)
				{
					return;
				}
				this.position_fixed = fixpos_key;
				this.Con.need_reposit_flag = true;
				this.depx = zero.x;
				this.depy = zero.y;
				if (this.nest_shuffle > 0 && this.t_nest_shuffle >= 0f)
				{
					this.t_move = ((this.t_move < 0f) ? 0f : X.Mn(this.t_move, 20f));
				}
				else
				{
					this.t_move = 0f;
				}
				if (this.t <= 0f)
				{
					this.x = (float)((int)zero.z);
					this.y = (float)((int)zero.w);
				}
				this.sx = (int)this.x;
				this.sy = (int)this.y;
				this.reposit(true);
				return;
			}
		}

		public bool repositFixMover(bool force = false)
		{
			if (this.PosMv == null || (!force && !X.D))
			{
				return false;
			}
			Vector3 hkdsDepertPos = this.PosMv.getHkdsDepertPos();
			if (hkdsDepertPos.z == 0f)
			{
				this.PosMv = null;
				this.position_fixed = null;
				return false;
			}
			M2DBase m2D = this.PosMv.M2D;
			float num = -IN.wh + this.rect_w * 0.5f + X.Mx(m2D.ui_shift_x * 2f, 0f);
			float num2 = IN.wh - this.rect_w * 0.5f + X.Mn(m2D.ui_shift_x * 2f, 0f);
			hkdsDepertPos.x = X.MMX(num, hkdsDepertPos.x, num2);
			if (!this.posmv_top)
			{
				if (hkdsDepertPos.y < -IN.hh * 0.3f)
				{
					this.posmv_top = true;
				}
			}
			else if (hkdsDepertPos.y > IN.hh * 0.3f)
			{
				this.posmv_top = false;
			}
			float num3 = -IN.hh + this.rect_h * 0.5f + ((!this.posmv_top) ? 56f : 0f);
			float num4 = IN.hh - this.rect_h * 0.5f - (this.posmv_top ? 56f : 0f);
			hkdsDepertPos.y = X.MMX(num3, hkdsDepertPos.y, num4);
			this.sx = (int)hkdsDepertPos.x;
			this.sy = (int)hkdsDepertPos.y;
			this.depx = (float)((int)hkdsDepertPos.x);
			this.depy = (float)((int)(hkdsDepertPos.y + 56f * (float)X.MPF(this.posmv_top)));
			if (this.t_move >= 0f)
			{
				this.t_move = X.Mn(this.t_move, 20f);
			}
			else
			{
				this.t_move = X.Mx(this.t_move, -30f);
			}
			return true;
		}

		public void reposit(bool force = false)
		{
			if (this.FollowTo == null)
			{
				this.fkds_agR = -1.5707964f;
			}
			float num;
			if (this.t_move < 0f)
			{
				num = X.ZSIN(-this.t_move, 30f);
			}
			else if (this.t_move <= 30f)
			{
				num = 1f - X.ZSIN2(this.t_move, 20f);
			}
			else
			{
				if (!force)
				{
					return;
				}
				num = (float)((this.t_move >= 0f) ? 0 : 1);
			}
			this.x = X.NI(this.depx, (float)this.sx, num);
			this.y = X.NI(this.depy, (float)this.sy, num);
			Vector3 localPosition = base.transform.localPosition;
			localPosition.x = this.x * 0.015625f;
			localPosition.y = this.y * 0.015625f;
			this.hkds_fine_agr_t = 5f;
			base.transform.localPosition = localPosition;
			if (this.Confirmer.targetIs(this))
			{
				this.Confirmer.need_fine_position = true;
			}
		}

		public void setHkdsType(NelMSG.HKDSTYPE _type)
		{
			this.next_hkds_type = _type;
			if (this.t_hkds >= 0f && this.type != _type)
			{
				if (this.type == NelMSG.HKDSTYPE.BOOK)
				{
					this.releaseBookDrawer();
					if (this.bounds_type == NelMSG.HKDS_BOUNDS.BOOK)
					{
						this.setBoundsType(NelMSG.HKDS_BOUNDS.NORMAL, false);
					}
				}
				this.type = _type;
				this.t_hkds = X.Mn(10f, this.t_hkds);
				this.mesh_create_level = 0f;
				this.t_talker = X.Mn(this.t_talker, 31f);
				this.nodrawtail &= (NelMSG.NODRAW)253;
				this.topcolor = 4293321691U;
				this.bottomcolor = 4290689711U;
				this.talker_mesh_color = 4283780170U;
				this.talker_mesh_color_offline = 4285095516U;
				this.talker_text_color = 4293321691U;
				Material material = this.BaseMtr;
				switch (this.type)
				{
				case NelMSG.HKDSTYPE.NORMAL:
					this.t_hkds_maxt = 20f;
					goto IL_02BF;
				case NelMSG.HKDSTYPE.DEVICE:
					this.t_hkds_maxt = 5f;
					this.topcolor = 4283782485U;
					this.bottomcolor = 3431039361U;
					this.talker_mesh_color = 3388999913U;
					this.talker_mesh_color_offline = 2852129001U;
					this.talker_text_color = uint.MaxValue;
					if (this.DeviceMessageMtr == null)
					{
						this.DeviceMessageMtr = MTRX.newMtr(MTR.MtrNoisyMessage);
					}
					this.initStencilRef(this.DeviceMessageMtr, false);
					material = this.DeviceMessageMtr;
					goto IL_02BF;
				case NelMSG.HKDSTYPE.EVIL:
					this.t_hkds_maxt = 20f;
					this.topcolor = 4283650899U;
					this.bottomcolor = 3432918574U;
					this.talker_mesh_color = 4293591040U;
					this.talker_mesh_color_offline = 4289161320U;
					this.talker_text_color = 4278190080U;
					if (M2DBase.Instance != null)
					{
						if (this.EvilMessageMtr == null)
						{
							this.EvilMessageMtr = MTRX.newMtr(MTR.ShaderEnemyDark);
							this.EvilMessageMtr.mainTexture = MTRX.MIicon.Tx;
						}
						(M2DBase.Instance as NelM2DBase).ENMTR.assign(this.EvilMessageMtr);
						this.initStencilRef(this.EvilMessageMtr, true);
						material = this.EvilMessageMtr;
						goto IL_02BF;
					}
					goto IL_02BF;
				case NelMSG.HKDSTYPE.BOOK:
					this.t_hkds_maxt = 80f;
					this.nodrawtail |= NelMSG.NODRAW.TAIL_FROM_TYPE;
					this.setBoundsType(NelMSG.HKDS_BOUNDS.BOOK, false);
					if (this.Bk == null)
					{
						this.initBookDrawer();
						goto IL_02BF;
					}
					this.Bk.activate();
					this.progressNextParagraphBook();
					this.Bk.MAXT_OPEN = 30f;
					goto IL_02BF;
				case NelMSG.HKDSTYPE.ONELINE:
					this.t_hkds_maxt = 20f;
					this.topcolor = 4282531647U;
					this.bottomcolor = 3426893631U;
					goto IL_02BF;
				}
				this.t_hkds_maxt = 40f;
				IL_02BF:
				if (this.Md.getMaterial() != material)
				{
					this.Md.setMaterial(material, false);
					this.Mrd.sharedMaterial = material;
				}
			}
		}

		public void setBoundsType(NelMSG.HKDS_BOUNDS _btype, bool force = false)
		{
			if (this.bounds_type != _btype || force)
			{
				bool special_hktype = this.special_hktype;
				this.bounds_type = _btype;
				this.mesh_create_level = 0f;
				if (this.t_hkds > 0f)
				{
					this.t_hkds = X.Mx(0f, X.Mn(this.t_hkds_maxt - 8f, this.t_hkds));
				}
				this.t_talker = X.Mn(this.t_talker, 31f);
				bool flag = !this.GobMdC.activeSelf;
				this.nodrawtail &= (NelMSG.NODRAW)186;
				ALIGN align = ALIGN.LEFT;
				ALIGNY aligny = ALIGNY.TOP;
				switch (this.bounds_type)
				{
				case NelMSG.HKDS_BOUNDS.TT:
					this.rect_w = 420f;
					this.rect_h = 160f;
					this.nodrawtail |= NelMSG.NODRAW.NORMAL_SKEW;
					break;
				case NelMSG.HKDS_BOUNDS.WIDE:
					this.rect_w = 800f;
					this.rect_h = 240f;
					this.nodrawtail |= NelMSG.NODRAW.NORMAL_SKEW;
					break;
				case NelMSG.HKDS_BOUNDS.WIDE_TT:
				case NelMSG.HKDS_BOUNDS.MONOLOGUE:
					this.rect_w = 800f;
					this.rect_h = 160f;
					this.nodrawtail |= NelMSG.NODRAW.NORMAL_SKEW;
					if (this.bounds_type == NelMSG.HKDS_BOUNDS.MONOLOGUE)
					{
						this.nodrawtail |= (NelMSG.NODRAW)5;
						align = ALIGN.CENTER;
						aligny = ALIGNY.MIDDLE;
					}
					break;
				case NelMSG.HKDS_BOUNDS.BOOK:
					this.rect_w = 870f;
					this.rect_h = 480f;
					this.nodrawtail |= (NelMSG.NODRAW)5;
					break;
				case NelMSG.HKDS_BOUNDS.ONELINE:
					this.rect_w = 346f;
					this.rect_h = 64f;
					align = ALIGN.CENTER;
					aligny = ALIGNY.MIDDLE;
					this.nodrawtail |= (NelMSG.NODRAW)68;
					break;
				default:
					this.rect_w = 420f;
					this.rect_h = 240f;
					break;
				}
				if (!this.draw_talker)
				{
					this.MdC.clear(false, false);
					this.GobMdC.SetActive(false);
				}
				else if (flag)
				{
					this.MdC.clear(false, false);
					this.GobMdC.SetActive(true);
				}
				if (TX.getCurrentFamily().is_english)
				{
					this.Tx.auto_condense_line = (this.Tx.auto_condense = false);
					this.Tx.auto_wrap = true;
				}
				else
				{
					this.Tx.auto_wrap = false;
					if (this.bounds_type == NelMSG.HKDS_BOUNDS.BOOK)
					{
						this.Tx.auto_condense_line = true;
					}
					else
					{
						this.Tx.auto_condense = true;
					}
				}
				this.Tx.alignx = align;
				this.Tx.aligny = aligny;
				this.rc_tan = this.rect_h / this.rect_w;
				if (this.bounds_type != NelMSG.HKDS_BOUNDS.BOOK)
				{
					if (special_hktype != this.special_hktype)
					{
						this.setHkdsTypeToDefault(true);
					}
					this.TxReposit();
				}
				this.repositConfirmer(false);
			}
		}

		public bool makeMessage(string label, EvPerson P, bool set_hkds_def = true, bool merge_flag = false)
		{
			bool flag;
			using (BList<string> blist = ListBuffer<string>.Pop(0))
			{
				if (!NelMSGResource.getContent(label, blist, false, false, false))
				{
					flag = false;
				}
				else
				{
					flag = this.makeMessage(blist, P, set_hkds_def, merge_flag);
				}
			}
			return flag;
		}

		public bool makeMessage(List<string> Amsg, EvPerson P, bool set_hkds_def = true, bool merge_flag = false)
		{
			if (this.ReservedInfo != null)
			{
				this.readInfoInner();
			}
			this.t_nest_shuffle = -1f;
			this.nest_shuffle = -1;
			this.all_char_shown = false;
			this.always_focus_color = false;
			if (set_hkds_def && this.t <= 2f)
			{
				this.setHkdsTypeToDefault(true);
			}
			this.Tx.reserveText(Amsg, (int)this.t, merge_flag);
			this.maxt_auto_progress = -1f;
			this.t_auto_progress = -1f;
			this.auto_hide_time = 0f;
			this.Tx.alpha = 1f;
			if (this.Bk == null)
			{
				if (TX.noe(this.localized_talker_name_))
				{
					this.localized_talker_name_ = ((P != null) ? P.localized_name : TX.Get("Talker_Mob", ""));
				}
				this.fineCapContent();
				this.hkds_fine_agr_t = 5f;
			}
			return true;
		}

		public void replaceTxInjection(List<string> A)
		{
			this.Tx.replaceTxInjection(A);
		}

		private void fineCapContent()
		{
			if (!this.TxCap.textIs(this.localized_talker_name_))
			{
				this.TxCap.letter_spacing = 1.05f;
				this.TxCap.text_content = this.localized_talker_name_;
				if (this.TxCap.get_swidth_px() >= this.rect_w - 190f)
				{
					this.TxCap.letter_spacing = 0.9f;
				}
			}
			if (this.t >= 0f)
			{
				this.TxCap.alpha = 1f;
			}
			this.need_fine_color = true;
			this.TxCap.Col(C32.d2c(this.talker_text_color));
		}

		public bool run(float fcnt, bool skipping)
		{
			bool flag = true;
			if (this.t >= 0f)
			{
				fcnt *= this.Tx.container_TS;
				if (this.ReservedInfo != null)
				{
					this.readInfoInner();
				}
				if (this.next_hkds_type != NelMSG.HKDSTYPE._MAX && this.type != this.next_hkds_type)
				{
					this.setHkdsType(this.next_hkds_type);
				}
				if (this.t_move <= 20f || this.t <= 20f)
				{
					this.need_fine_color = true;
				}
				this.t += fcnt;
				if (this.t_auto_progress >= 0f && this.maxt_auto_progress >= 0f)
				{
					this.t_auto_progress += (skipping ? (this.maxt_auto_progress / 4f) : fcnt);
					if (this.t_auto_progress >= this.maxt_auto_progress)
					{
						this.t_auto_progress = -1f;
						if (!this.Tx.hasReservedContent())
						{
							this.hideMsg(false);
						}
						else
						{
							this.Tx.progressNextParagraph(false, null);
						}
					}
				}
				if (skipping && !this.all_char_shown)
				{
					this.showImmediate(false, false);
				}
				if (this.t >= 0f && this.auto_hide_time > 0f && this.t >= this.auto_hide_time)
				{
					if (!this.all_char_shown)
					{
						this.showImmediate(false, false);
					}
					this.hideMsg(false);
				}
				if (this.t_nest_shuffle > 0f && this.all_char_shown)
				{
					this.t_nest_shuffle = X.Mx(this.t_nest_shuffle - fcnt * (float)(skipping ? 3 : 1), 0f);
					if (this.t_nest_shuffle == 0f && !this.Con.progressNestShuffle(this, this.id, this.P))
					{
					}
				}
			}
			else
			{
				if (this.t_nest_shuffle >= 0f && this.Tx.hasReservedContent())
				{
					flag = false;
				}
				else
				{
					if (this.t <= ((this.Bk != null) ? (-18f) : (-30f)))
					{
						this.releaseDrawer(true);
						return false;
					}
					this.t -= fcnt;
				}
				this.need_fine_color = true;
				if (this.t_front >= 0f)
				{
					this.t_front = -1f;
				}
				if (this.t_hkds >= 0f)
				{
					this.t_hkds = -1f;
				}
				if (this.t_talker >= 0f)
				{
					this.t_talker = -1f;
				}
				if (this.t_move >= 0f)
				{
					this.t_move = -1f;
					if (this.Bk == null)
					{
						if (this.position_fixed == null)
						{
							this.sy = (int)this.depy;
							this.sx = (int)((X.BTW(-1.5707964f, this.fkds_agR, 1.5707964f) ? ((float)EV.pw * 0.5f - this.depx) : ((float)(-(float)EV.pw) * 0.5f - this.depx)) * 0.5f + this.depx);
						}
					}
					else
					{
						this.Bk.MAXT_OPEN = 11f;
						this.Bk.deactivate();
						SND.Ui.play("book_close", false);
					}
				}
			}
			if (this.PosMv != null)
			{
				if (this.repositFixMover(false))
				{
					this.fineHkdsDepertAngle();
				}
			}
			else if (this.draw_hkds_tail && this.FollowTo != null && this.FollowTo.checkPositionMoved(this.Con))
			{
				this.fineHkdsDepertAngle();
			}
			bool flag2 = false;
			if (this.hkds_fine_agr_t > 0f && this.draw_hkds_tail)
			{
				if (this.FollowTo == null)
				{
					this.hkds_fine_agr_t = 0f;
				}
				else
				{
					Vector3 hkdsDepertPos = this.FollowTo.getHkdsDepertPos();
					if (hkdsDepertPos.z != 1f)
					{
						this.hkds_fine_agr_t -= fcnt;
					}
					else
					{
						if (this.fix_first_pos_to_follow > 0)
						{
							this.sx = (int)hkdsDepertPos.x;
							this.sy = (int)hkdsDepertPos.y;
						}
						float num = X.GAR2(this.x, this.y, hkdsDepertPos.x, hkdsDepertPos.y);
						if (num != this.fkds_agR)
						{
							if (this.mesh_create_level <= 0f)
							{
								this.fkds_agR = num;
							}
							else
							{
								this.fkds_agR = X.MULWALKANGLER(this.fkds_agR, num, (this.t <= 25f) ? 0.4f : 0.084f);
							}
							this.hkds_fine_agr_t = 5f;
							flag2 = true;
						}
						else
						{
							this.hkds_fine_agr_t -= fcnt;
						}
					}
				}
			}
			if (this.t_move >= 0f)
			{
				if (this.t_move <= 20f)
				{
					this.t_move += fcnt;
					this.reposit(false);
				}
			}
			else if (this.t_move >= -30f)
			{
				this.t_move -= fcnt;
				this.reposit(false);
			}
			if (this.t_hkds >= 0f)
			{
				if (this.t_hkds <= this.t_hkds_maxt + 2f || flag2)
				{
					this.need_fine_mesh = true;
				}
				this.t_hkds += fcnt;
			}
			else
			{
				this.need_fine_mesh = true;
				this.t_hkds -= fcnt;
			}
			float num2 = -1f;
			if (this.need_fine_mesh)
			{
				if (num2 < 0f)
				{
					num2 = this.hkds_z;
				}
				this.recreateMesh(num2);
				this.need_fine_mesh = false;
				this.need_fine_color = true;
			}
			if (this.t_talker >= 0f)
			{
				if (this.t_talker <= 32f)
				{
					this.t_talker += fcnt;
					this.recreateTalkerMesh();
					this.need_fine_color = true;
				}
			}
			else if (this.t_talker >= -32f)
			{
				this.t_talker -= fcnt;
				this.recreateTalkerMesh();
				this.need_fine_color = true;
			}
			if (this.t_front >= 0f)
			{
				if (this.t_front <= 38f)
				{
					this.need_fine_color = true;
				}
				this.repositConfirmer(false);
				if (this.t < 20f)
				{
					this.t_front = X.Mx(this.t_front, 30f);
				}
				this.t_front += fcnt;
			}
			else if (this.t_front >= -30f)
			{
				this.t_front -= fcnt;
				this.need_fine_color = true;
			}
			if (this.need_fine_color)
			{
				if (num2 < 0f)
				{
					num2 = this.hkds_z;
				}
				this.fineColorMesh(num2);
				this.need_fine_color = false;
				this.need_fine_talker_color = false;
			}
			else if (this.need_fine_talker_color)
			{
				this.MdC.updateForMeshRenderer(true);
			}
			if (this.Bk != null)
			{
				this.Bk.run(fcnt);
			}
			if (flag)
			{
				this.Tx.run(this.Con.TS, skipping);
			}
			return true;
		}

		private float hkds_z
		{
			get
			{
				if (this.t_hkds < 0f)
				{
					return 1f - X.ZSIN(-this.t_hkds, 20f);
				}
				return X.ZSIN(this.t_hkds, this.t_hkds_maxt);
			}
		}

		private void recreateMesh(float lv)
		{
			if (this.Bk == null)
			{
				float num = this.rect_w * 0.5f;
				float num2 = this.rect_h * 0.5f;
				float num3 = 0.7f;
				bool flag = false;
				bool flag2 = false;
				float num4 = 1f;
				if (lv != this.mesh_create_level)
				{
					this.Md.clear(true, false);
					this.mesh_create_level = lv;
					this.Md.Identity();
					this.Md.Scale(1f, X.ZSIN(lv, 0.5f), false);
					this.MdB.setCurrentMatrix(this.Md.getCurrentMatrix(), false);
					this.MdB.Col = this.MdB.ColGrd.Set(4283780170U).mulA((this.t >= 0f) ? (0.5f + 0.5f * X.ZLINE(this.t, 20f)) : (1f - X.ZLINE(-this.t, 20f))).C;
					this.MdB.ColGrd.setA(0f);
					this.MdB.KadomaruRect(0f, 0f, this.rect_w + 50f, this.rect_h + 50f, this.rect_h * 0.5f, 75f, false, 1f, 0f, true);
					this.MdB.updateForMeshRenderer(false);
					this.Ma.Set(true);
					switch (this.type)
					{
					case NelMSG.HKDSTYPE.CIRC:
					case NelMSG.HKDSTYPE.THINK:
					{
						int num5 = ((this.type == NelMSG.HKDSTYPE.CIRC) ? 16 : 0);
						num4 = X.NI(0.5f, 1f, lv);
						float num6 = this.rect_h * num4 / 2f * 0.89f;
						this.Md.KadomaruRect(0f, 0f, this.rect_w, this.rect_h * num4, num6, 0f, false, 0f, 0f, false);
						for (int i = 0; i < 4; i++)
						{
							AIM aim = AIM.LT + (uint)i;
							this.Md.Circle((this.rect_w * 0.5f - num6 * 0.462f) * (float)CAim._XD(aim, 1), (this.rect_h * 0.5f - num6 * 0.288f) * (float)CAim._YD(aim, 1) * num4, num6 * 0.31f * lv, 0f, false, 0f, 0f);
						}
						for (int j = 0; j < num5; j++)
						{
							float num7 = (lv - 0.2f - (float)(j / 3) * 0.1f) / 0.37f;
							if (num7 > 0f)
							{
								uint ran = X.GETRAN2(this.ran_t0 + 50 + j * 7, (int)(3 + ((this.P == null) ? 15 : (this.P.index % 13))));
								float num8 = -1f + ((float)j + 0.5f + 0.2f * (-0.5f + X.RAN(ran, 474))) / (float)num5 * 2f;
								float num9 = (float)X.MPF(X.BTW(-0.5f, num8, 0.5f)) * (this.rect_w / 2f - num6) * 0.015625f;
								num8 *= 3.1415927f;
								float num10 = X.Sin(num8);
								float num11 = X.NI(0.04f, 0.13f, X.RAN(ran, 1838)) * this.rect_h * 0.015625f * (X.ZSIN2(num7, 0.8f) * 1.25f - X.ZCOS(num7 - 0.7f, 0.3f) * 0.25f) * X.NI(0.5f, 1f, X.Abs(num10));
								float num12 = num9 + X.Cos(num8) * num6 * 0.015625f;
								float num13 = num10 * num6 * 0.015625f * num4;
								this.Md.Circle(num12, num13, num11, 0f, true, 0f, 0f);
							}
						}
						goto IL_0B7A;
					}
					case NelMSG.HKDSTYPE.ANGRY:
					{
						this.Md.KadomaruRect(0f, 0f, this.rect_w, this.rect_h, 30f, 0f, false, 0f, 0f, false);
						for (int k = 0; k < 14; k++)
						{
							float num14 = (lv - 0.2f - (float)(k / 4) * 0.1f) / 0.37f;
							if (num14 > 0f)
							{
								uint ran2 = X.GETRAN2(this.ran_t0 + k * 7, (int)(4 + ((this.P == null) ? 15 : (this.P.index % 11))));
								Vector3 vector = X.RANBORDER(this.rect_w * 0.015625f, this.rect_h * 0.015625f, (0.5f + (float)k - 0.3f + 0.6f * X.RAN(ran2, 586)) / 14f);
								bool flag3 = ((vector.z == 1f) ? (vector.x > 0f) : ((vector.z == 3f) ? (vector.x < 0f) : ((vector.z == 2f) ? (vector.y < 0f) : (vector.y > 0f))));
								float num15 = 1f;
								float num16;
								if (vector.z == 1f || vector.z == 3f)
								{
									vector.y -= (float)(X.MPF(vector.y > 0f) * 20) * 0.015625f;
									num16 = X.NI(0.14f, 0.16f, X.RAN(ran2, 1838)) * this.rect_h;
								}
								else
								{
									num15 = 0.6f;
									vector.y = X.NI((float)X.MPF(vector.y > 0f) * this.rect_h * 0.5f * 0.015625f, vector.y, 0.125f);
									vector.x -= (float)(X.MPF(vector.x > 0f) * 20) * 0.015625f;
									num16 = X.NI(0.14f, 0.16f, X.RAN(ran2, 1838)) * this.rect_h;
								}
								num16 = num16 * 0.015625f * (X.ZSIN2(num14, 0.8f) * 1.25f - X.ZCOS(num14 - 0.7f, 0.3f) * 0.25f);
								float num17 = X.ZSIN(num14, 0.875f) * this.rect_h;
								Vector2 vector2 = CAim.get_V2(CAim.get_clockwise2((AIM)vector.z, true)) * num17 * 0.22f * 0.015625f * num15;
								float num18 = X.GAR2(0f, 0f, vector.x, vector.y * 3f);
								if (flag3)
								{
									this.Md.Tri(0, 1, 2, false).Pos(vector.x + vector2.x, vector.y + vector2.y, null);
								}
								else
								{
									this.Md.Tri(0, 2, 1, false).Pos(vector.x - vector2.x, vector.y - vector2.y, null);
								}
								this.Md.Pos(vector.x + num16 * X.Cos(num18), vector.y + num16 * X.Sin(num18) * num15, null);
								this.Md.Pos(vector.x, vector.y, null);
							}
						}
						goto IL_0B7A;
					}
					case NelMSG.HKDSTYPE.DEVICE:
						if ((this.nodrawtail & NelMSG.NODRAW.NORMAL_SKEW) == NelMSG.NODRAW.NONE)
						{
							this.Md.Skew(0f, -9f * X.NI(0f, 1f, lv) / num, false);
						}
						this.Md.uvRectN(0.38f, 0.38f);
						this.Md.Uv2(0.38f, 1f, true);
						this.Md.Rect(0f, 0f, this.rect_w, this.rect_h, false);
						flag = true;
						num3 *= 0.75f;
						goto IL_0B7A;
					case NelMSG.HKDSTYPE.EVIL:
					{
						if ((this.nodrawtail & NelMSG.NODRAW.NORMAL_SKEW) == NelMSG.NODRAW.NONE)
						{
							this.Md.Skew(0f, -9f * X.NI(0f, 1f, lv) / num, false);
						}
						float num19 = (22f + this.rect_w * 0.5f) * 0.015625f;
						float num20 = (22f + this.rect_w * 0.5f) * 0.015625f;
						this.Md.uvRect(-num19, -num20, num19 * 2f, num20 * 2f, MTR.SqEffectFireBall.getImage(0, 0), false, true);
						num4 = X.NI(0.125f, 1f, X.ZCOS(lv));
						this.Md.Uv2(0.33f, 0.33f, false).Uv3(1f, 1f, false);
						this.Md.KadomaruRect(0f, 0f, this.rect_w, this.rect_h * num4, 30f, 0f, false, 0f, 0f, false);
						flag2 = (flag = true);
						goto IL_0B7A;
					}
					case NelMSG.HKDSTYPE.ONELINE:
						this.Md.KadomaruRect(0f, 0f, this.rect_w, this.rect_h, 8f, 0f, false, 0f, 0f, false);
						num3 *= 0.75f;
						goto IL_0B7A;
					}
					if ((this.nodrawtail & NelMSG.NODRAW.NORMAL_SKEW) == NelMSG.NODRAW.NONE)
					{
						this.Md.Skew(0f, -9f * X.NI(0f, 1f, lv) / num, false);
					}
					num4 = X.NI(0.5f, 1f, lv);
					this.Md.KadomaruRect(0f, 0f, this.rect_w, this.rect_h * num4, 30f, 0f, false, 0f, 0f, false);
					IL_0B7A:
					this.MaHkds.SetWhole(true);
				}
				else
				{
					this.MaHkds.revertVerAndTriIndexSaved(false);
				}
				if (this.draw_hkds_tail)
				{
					Vector3 vector3 = Vector3.zero;
					Vector3 vector4 = Vector3.zero;
					Vector3 vector5 = Vector3.zero;
					float num21 = this.fkds_agR;
					AIM tanArea = X.getTanArea(num21, this.rc_tan);
					if (tanArea == AIM.B)
					{
						vector3.Set(num2 * X.Cos(num21), -num2, 0f);
						vector4.Set(14f, 1f, 0f);
						vector5.Set(-14f, 1f, 0f);
					}
					else if (tanArea == AIM.R)
					{
						vector3.Set(num, (num2 - 20f) * X.Sin(num21), 0f);
						vector4.Set(-1f, 14f, 0f);
						vector5.Set(-1f, -14f, 0f);
					}
					else if (tanArea == AIM.T)
					{
						vector3.Set((num2 - 25f) * X.Cos(num21), num2, 0f);
						vector4.Set(-14f, -1f, 0f);
						vector5.Set(14f, -1f, 0f);
					}
					else
					{
						vector3.Set(-num, (num2 - 20f) * X.Sin(num21), 0f);
						vector4.Set(1f, -14f, 0f);
						vector5.Set(1f, 14f, 0f);
					}
					Matrix4x4 currentMatrix = this.Md.getCurrentMatrix();
					vector3 = currentMatrix.MultiplyPoint(vector3) * 0.015625f;
					vector4 = currentMatrix.MultiplyPoint(vector4) * 0.015625f;
					vector5 = currentMatrix.MultiplyPoint(vector5) * 0.015625f;
					this.Md.Identity().Scale(1f, num4, false);
					if (this.type == NelMSG.HKDSTYPE.THINK)
					{
						Vector3 vector6 = (vector5 + vector4) * 0.5f + vector3;
						Vector3 vector7 = vector6;
						vector7.x += num3 * X.Cos(num21);
						vector7.y += num3 * 1.5f * X.Sin(num21);
						for (int l = 0; l < 3; l++)
						{
							float num22 = (0.35f + (float)l) / 3f;
							Vector3 vector8 = X.NI(vector6, vector7, num22);
							this.Md.Circle(vector8.x, vector8.y, X.NI(14, 3, num22) * 0.015625f, 0f, true, 0f, 0f);
						}
					}
					else
					{
						this.Md.Tri(0, 2, 1, false).Pos(vector3.x + vector4.x, vector3.y + vector4.y, null).Pos(vector3.x + vector5.x, vector3.y + vector5.y, null);
						this.Md.Pos(vector3.x + num3 * X.Cos(num21), vector3.y + num3 * X.Sin(num21), null);
					}
					this.Md.setCurrentMatrix(currentMatrix, false);
				}
				if (flag)
				{
					this.Md.allocUv2(0, true);
				}
				if (flag2)
				{
					this.Md.allocUv3(0, true);
				}
				this.Ma.SetWhole(true);
				return;
			}
			if (lv == this.mesh_create_level)
			{
				return;
			}
			this.MdB.clear(true, false);
			this.MdB.Scale(2.2f, 1.5f, false);
			this.MdB.Col = this.MdB.ColGrd.Set(4280886565U).setA1(0.7f * lv).C;
			this.MdB.ColGrd.mulA(0f);
			float num23 = IN.hh * 0.3f;
			float num24 = IN.hh * 0.67f;
			this.MdB.BlurPoly2(110f, 0f, num23, 0f, 24, num24, num24, MeshDrawer.ColBuf0.Set(this.MdB.Col), this.MdB.ColGrd);
			this.MdB.Identity();
			this.Bk.Col = C32.MulA(2861403533U, lv);
			this.MdB.updateForMeshRenderer(false);
			this.mesh_create_level = lv;
		}

		public bool talker_mesh_slide
		{
			get
			{
				return this.type == NelMSG.HKDSTYPE.THINK || this.type == NelMSG.HKDSTYPE.CIRC;
			}
		}

		private void recreateTalkerMesh()
		{
			if (this.Bk != null)
			{
				return;
			}
			this.MdC.clear(true, false);
			this.MaC.Set(true);
			if (!this.draw_talker)
			{
				return;
			}
			this.MdC.Col = C32.d2c(this.talker_mesh_color);
			float num = (float)X.IntR(((this.t_talker >= 0f) ? X.ZSIN(this.t_talker - 4f, 28f) : (1f - X.ZSINV(-this.t_talker, 32f))) * (this.rect_w + 56f + 8f));
			for (int i = 0; i < 2; i++)
			{
				PxlImage img = MTR.AMsgTalkerBg[i].getLayer(0).Img;
				float num2 = (float)img.width;
				float num3 = (float)img.height;
				float num4 = num + (float)((i == 0) ? 56 : 0);
				float num5 = ((i == 0) ? (this.rect_h / 2f - 60f) : (-this.rect_h / 2f - 4f));
				float num6 = ((i == 0) ? (this.rect_w / 2f + 56f - num2 + (float)(this.talker_mesh_slide ? (-11) : 0)) : (-this.rect_w / 2f + 2f + (float)(this.talker_mesh_slide ? 14 : 0)));
				this.MdC.uvRect(num6 * 0.015625f, num5 * 0.015625f, num2 * 0.015625f, num3 * 0.015625f, img, true, false);
				float num7 = X.Mn(num4, num2);
				if (i == 0)
				{
					this.MdC.RectBL(num6 + num2 - num7, num5, num7, num3, false);
					num7 = num4 - num2;
					if (num7 > 0f)
					{
						this.MdC.initForImg(MTRX.IconWhite, 0);
						this.MdC.RectBL(num6 - num7, num5 + 24f, num7 + 1f, num3 - 24f, false);
					}
				}
				else
				{
					this.MdC.RectBL(num6, num5, num7, num3, false);
					num7 = num4 - num2;
					if (num7 > 0f)
					{
						this.MdC.initForImg(MTRX.IconWhite, 0);
						this.MdC.RectBL(num6 + num2, num5 + 23f, num7 + 1f, 2f, false);
					}
				}
			}
			this.MaC.Set(false);
		}

		private void fineColorMesh(float fine_mesh_lv)
		{
			if (this.Bk != null)
			{
				Color32[] colorArray = this.Md.getColorArray();
				byte b = (byte)(190f * fine_mesh_lv);
				int num = 48;
				if (colorArray.Length <= num)
				{
					return;
				}
				colorArray[0].a = b;
				for (int i = 1; i <= num; i += 2)
				{
					colorArray[i].a = b;
				}
				this.Tx.alpha = ((this.t >= 0f) ? 1f : fine_mesh_lv);
				this.Md.updateForMeshRenderer(true);
				return;
			}
			else
			{
				float num2 = ((this.t >= 0f) ? (0.5f + 0.5f * X.ZLINE(this.t, 20f)) : (1f - X.ZLINE(-this.t, 20f)));
				float num3 = ((this.t_front >= 0f) ? X.ZLINE(this.t_front, 30f) : (1f - X.ZLINE(-this.t_front, 30f)));
				this.Ma.setColAll(this.Md.ColGrd.Set(this.topcolor).blend(this.bottomcolor, 1f - num3).mulA(num2)
					.C, false);
				this.Md.updateForMeshRenderer(true);
				this.MaC.setColAll(C32.MulA(this.TalkerBaseColor, num2), false);
				this.MdC.updateForMeshRenderer(true);
				if (this.t < 0f)
				{
					this.Tx.alpha = (this.TxCap.alpha = num2);
					return;
				}
				this.Tx.alpha = (this.TxCap.alpha = 1f);
				return;
			}
		}

		public Color32 TalkerBaseColor
		{
			get
			{
				float num = ((this.t_front >= 0f) ? X.ZLINE(this.t_front, 30f) : (1f - X.ZLINE(-this.t_front, 30f)));
				return this.MdC.ColGrd.Set(this.talker_mesh_color).blend(this.talker_mesh_color_offline, 1f - num).C;
			}
		}

		private void initBookDrawer()
		{
			this.Md.clear(false, false);
			this.MdB.clear(false, false);
			this.MdC.clear(false, false);
			this.Ma.Set(true);
			this.MaC.Set(true);
			NelMSG.prepareBookDrawer(this.id, base.gameObject, out this.Bk, out this.GobBook, false);
			if (M2DBase.Instance != null)
			{
				M2DBase.Instance.FlagValotStabilize.Add(this.book_stabilize_id);
			}
			this.Bk.MAXT_PAGE_PARAPARA = 10f;
			IN.setZ(this.GobBook.transform, 0.5f);
			this.Tx.stencil_ref = -1;
			this.Tx.LineSpacing(1.8f).LetterSpacing(0.95f).Size(20f);
			this.Tx.max_swidth_px = 440f;
			this.Tx.alpha = 1f;
			this.Tx.BorderCol(3137339392U);
			this.Tx.aligny = ALIGNY.MIDDLE;
			this.Tx.DELAY_FIRST = 42;
			this.Tx.DELAY_CONTINUE = 20;
			this.MdB.clear(false, false);
			IN.PosP(this.Tx.transform, 0f, 30f, 0f);
			this.t_hkds = 0f;
		}

		public static void prepareBookDrawer(string name, GameObject Gob, out NelBookDrawer Bk, out GameObject GobBook, bool use_valotile = false)
		{
			Bk = new NelBookDrawer(name, null, null);
			GobBook = IN.CreateGob(Gob, "-book");
			Bk.attachGob(GobBook, use_valotile);
			Bk.Col = C32.d2c(2861403533U);
			Bk.MAXT_PAGE = 40f;
		}

		private void releaseBookDrawer()
		{
			if (this.Bk == null)
			{
				return;
			}
			this.Tx.FnMeshUpdated = null;
			this.TxReposit();
			if (M2DBase.Instance != null)
			{
				M2DBase.Instance.FlagValotStabilize.Rem(this.book_stabilize_id);
			}
			int num = (int)(58 + this.msg_index);
			this.Tx.aligny = ALIGNY.TOP;
			this.Tx.DELAY_FIRST = 20;
			this.Tx.DELAY_CONTINUE = 14;
			this.Tx.BorderCol(0U);
			this.Tx.StencilRef(num);
			global::UnityEngine.Object.Destroy(this.GobBook);
			this.Bk.destruct();
			this.Bk = null;
		}

		private void makeMessageBook()
		{
		}

		private void progressNextParagraphBook()
		{
			this.Bk.progressPage(true);
		}

		public void TextRendererUpdated()
		{
			NelBookDrawer bk = this.Bk;
		}

		public void FixTextContent(STB Stb)
		{
			string prefixForTalker = this.Con.getPrefixForTalker(this.id);
			if (TX.valid(prefixForTalker))
			{
				Stb.Insert(0, prefixForTalker);
			}
		}

		public string popReservedContent()
		{
			return this.Tx.popReservedContent(1);
		}

		private int countReserved()
		{
			return this.Tx.getReservedCountMax();
		}

		public EMOT default_emot
		{
			get
			{
				if (this.bounds_type != NelMSG.HKDS_BOUNDS.BOOK)
				{
					return EMOT.NORMAL;
				}
				return EMOT.FADEIN;
			}
		}

		public uint default_col
		{
			get
			{
				NelMSG.HKDSTYPE hkdstype = this.type;
				if (hkdstype == NelMSG.HKDSTYPE.DEVICE || hkdstype - NelMSG.HKDSTYPE.BOOK <= 1)
				{
					return uint.MaxValue;
				}
				return 4283780170U;
			}
		}

		private bool special_hktype
		{
			get
			{
				return this.bounds_type == NelMSG.HKDS_BOUNDS.BOOK || this.bounds_type == NelMSG.HKDS_BOUNDS.ONELINE;
			}
		}

		public void setHkdsTypeToDefault(bool reserve = false)
		{
			NelMSG.HKDS_BOUNDS hkds_BOUNDS = this.bounds_type;
			NelMSG.HKDSTYPE hkdstype;
			if (hkds_BOUNDS != NelMSG.HKDS_BOUNDS.BOOK)
			{
				if (hkds_BOUNDS != NelMSG.HKDS_BOUNDS.ONELINE)
				{
					hkdstype = NelMSG.HKDSTYPE.NORMAL;
				}
				else
				{
					hkdstype = NelMSG.HKDSTYPE.ONELINE;
				}
			}
			else
			{
				hkdstype = NelMSG.HKDSTYPE.BOOK;
			}
			if (reserve)
			{
				this.next_hkds_type = hkdstype;
				return;
			}
			this.setHkdsType(hkdstype);
		}

		public bool is_book
		{
			get
			{
				return this.Bk != null;
			}
		}

		public void executeRestMsgCmd(int count = 0)
		{
			this.Con.executeRestMsgCmd(this, count);
		}

		public void fineHkdsDepertAngle()
		{
			this.hkds_fine_agr_t = 5f;
		}

		public bool progressNextParagraph()
		{
			if (this.Tx.progressNextParagraph(false, null))
			{
				if (this.Bk != null)
				{
					this.progressNextParagraphBook();
				}
				this.all_char_shown = false;
				return true;
			}
			if (this.t_nest_shuffle > 0f)
			{
				if (this.t_nest_shuffle > 0f)
				{
					this.t_nest_shuffle = X.Mn(this.t_nest_shuffle, 0.01f);
				}
				return true;
			}
			return (this.t_nest_shuffle == 0f && this.Con.progressNestShuffle(this, this.id, this.P)) || (this.nest_shuffle >= 0 && this.Con.progressgNestShuffleFinal(this, this.id));
		}

		public void quitNestShuffle(bool is_front_msg)
		{
			this.t_nest_shuffle = -1f;
			if (is_front_msg && this.isActive())
			{
				this.Confirmer.init(this, base.transform, true);
			}
		}

		public bool has_nest_shuffle_buffer
		{
			get
			{
				return this.t_nest_shuffle >= 0f;
			}
		}

		public void progressReserved()
		{
			this.setHkdsTypeToDefault(true);
			this.t_auto_progress = -1f;
			if (this.Confirmer.targetIs(this))
			{
				this.fineConfirmerSuffix();
			}
		}

		public bool hasReservedContent()
		{
			return this.Tx.hasReservedContent();
		}

		public bool isCompletelyHidden()
		{
			return !base.gameObject.activeSelf;
		}

		public bool isSame(string _id)
		{
			return this.id == _id;
		}

		public bool isSamePerson(NelMSG Src)
		{
			return this.id == Src.id;
		}

		public bool isSame(IHkdsFollowable Dc)
		{
			return this.FollowTo == Dc;
		}

		public bool isSame(string _person_key, EvPerson _P)
		{
			if (this.P != null)
			{
				return _P == this.P;
			}
			return _P == null && this.id == _person_key;
		}

		public float dx_real
		{
			get
			{
				return this.depx;
			}
		}

		public float dy_real
		{
			get
			{
				return this.depy;
			}
		}

		public bool isFront()
		{
			return this.t_front >= 0f;
		}

		public float shown_level
		{
			get
			{
				if (this.t < 0f)
				{
					return -1f;
				}
				return X.Mn(this.t, 20f);
			}
		}

		public TalkDrawer DrawerPersonFollowing
		{
			get
			{
				if (this.P == null || this.position_fixed != null)
				{
					return null;
				}
				return this.FollowTo as TalkDrawer;
			}
		}

		public void showImmediate(bool show_only_first_char = false, bool play_snd = false)
		{
			this.t = ((this.t >= 0f) ? X.Mx(this.t, 18f) : X.Mn(this.t, -28f));
			if (this.isFront())
			{
				this.repositConfirmer(false);
			}
			if (X.BTW(0f, this.t_hkds, this.t_hkds_maxt))
			{
				this.t_hkds = this.t_hkds_maxt;
			}
			if (X.BTW(0f, this.t_talker, 32f))
			{
				this.t_talker = 31f;
			}
			if (X.BTW(0f, this.t_move, 20f))
			{
				this.t_move = 19f;
			}
			this.t_front = ((this.t_front >= 0f) ? X.Mx(this.t_front, 28f) : X.Mn(this.t_front, -28f));
			this.Tx.showImmediate(show_only_first_char, play_snd);
			if (this.maxt_auto_progress >= 0f && this.t_auto_progress < 0f)
			{
				this.t_auto_progress = 0f;
			}
			if (this.Bk != null)
			{
				this.Bk.showImmediate();
			}
		}

		public int getCurrentMsgKey(out string person_key)
		{
			person_key = this.id;
			return this.Tx.getReservedProgress();
		}

		public float container_TS
		{
			get
			{
				return this.Tx.container_TS;
			}
			set
			{
				this.Tx.container_TS = value;
			}
		}

		public void releaseReserved()
		{
			this.Tx.releaseReserved();
		}

		public void hideMsg(bool immediate = false)
		{
			this.t = X.Mn(this.t, immediate ? (-28f) : (-1f));
			this.t_nest_shuffle = -1f;
			this.ReservedInfo = null;
			this.all_char_shown = false;
			if (this.Bk != null)
			{
				this.Bk.deactivate();
			}
			this.Con.msgHided(this);
		}

		public ConfirmAnnouncer Confirmer
		{
			get
			{
				return EV.Confirmer;
			}
		}

		public void releaseInfo()
		{
			this.ReservedInfo = null;
		}

		public bool isActive()
		{
			return this.t >= 0f;
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"MSG ",
				this.id,
				" (",
				this.Tx.text_content,
				")"
			});
		}

		public bool isAllCharsShown()
		{
			return this.all_char_shown;
		}

		private float x;

		private float y;

		private float depx;

		private float depy;

		private int sx;

		private int sy;

		private NelMSG.HKDSTYPE type = NelMSG.HKDSTYPE._MAX;

		private NelMSG.HKDS_BOUNDS bounds_type;

		public NelMSGContainer Con;

		private byte msg_index;

		private string book_stabilize_id;

		private string id;

		private string localized_talker_name_;

		public IHkdsFollowable FollowTo;

		private EvPerson P;

		private string position_fixed;

		private bool posmv_top = true;

		private M2Mover PosMv;

		private MeshDrawer Md;

		private MeshRenderer Mrd;

		private ValotileRenderer Valot;

		private MeshDrawer MdB;

		private MeshRenderer MrdB;

		private ValotileRenderer ValotB;

		private MdArranger Ma;

		private MdArranger MaHkds;

		private MeshDrawer MdC;

		private MeshRenderer MrdC;

		private ValotileRenderer ValotC;

		private MdArranger MaC;

		private GameObject GobMdB;

		private GameObject GobMdC;

		private NelEvTextRenderer Tx;

		private TextRenderer TxCap;

		private Material BaseMtr;

		private Material DeviceMessageMtr;

		private Material EvilMessageMtr;

		private NelMSG.HKDSTYPE next_hkds_type = NelMSG.HKDSTYPE._MAX;

		private float t = -30f;

		private float t_hkds;

		private float t_move;

		private float t_talker;

		private float t_front;

		private float t_auto_progress = -1f;

		public float maxt_auto_progress = -1f;

		public float auto_hide_time;

		private int ran_t0;

		private float mesh_create_level;

		private bool all_char_shown_ = true;

		public bool always_focus_color;

		public bool behind_flag;

		private uint topcolor;

		private uint bottomcolor;

		private uint talker_mesh_color;

		private uint talker_mesh_color_offline;

		private uint talker_text_color;

		public float rect_w = 420f;

		public float rect_h = 240f;

		public float rc_tan = 1f;

		public const float rect_w_basic = 420f;

		public const float rect_w_wide = 800f;

		public const float rect_h_basic = 240f;

		public const float rect_w_book = 870f;

		public const float rect_h_book = 480f;

		public const float text_inner_margin_x = 45f;

		public const float text_inner_margin_y = 30f;

		public const float caption_h = 26f;

		public const float hkds_w = 28f;

		public const float hkds_wh = 14f;

		public const byte book_shadow_alpha = 190;

		public const int T_MOVE = 20;

		public const float T_FADEOUT = 30f;

		public const float T_FADE = 30f;

		public const float T_TALKER_MAXT = 32f;

		private float t_hkds_maxt = 20f;

		private float t_nest_shuffle;

		public byte fix_first_pos_to_follow;

		private const int BOOK_BEHIND_KAKU = 24;

		private const float book_angle360 = -1.87f;

		private const float GOBC_Z = 5E-05f;

		private const float Z_MDB = 0.0015f;

		private const float Z_MD = 0.001f;

		public int person_index;

		public short nest_shuffle = -1;

		private float fkds_agR;

		private float hkds_fine_agr_t = 5f;

		private string talker_snd_;

		private NelBookDrawer Bk;

		private NelMSGContainer.HkdsInfo ReservedInfo;

		private const float CONFIRMER_BASE_ALPHA = 0.8f;

		private NelMSG.NODRAW nodrawtail;

		private static readonly Regex RegVpTalkerPosFollow = new Regex("^\\@([\\w]+)");

		private bool need_fine_mesh;

		private bool need_fine_color;

		private bool need_fine_talker_color;

		private GameObject GobBook;

		public enum HKDSTYPE
		{
			NORMAL,
			CIRC,
			THINK,
			ANGRY,
			DEVICE,
			EVIL,
			BOOK,
			ONELINE,
			_MAX
		}

		public enum HKDS_BOUNDS
		{
			NORMAL,
			TT,
			WIDE,
			WIDE_TT,
			BOOK,
			MONOLOGUE,
			ONELINE,
			_OFFLINE
		}

		private enum NODRAW : byte
		{
			NONE,
			TAIL_FROM_BOUNDS,
			TAIL_FROM_TYPE,
			_TAIL,
			TALKERMESH,
			NORMAL_SKEW = 64
		}
	}
}
