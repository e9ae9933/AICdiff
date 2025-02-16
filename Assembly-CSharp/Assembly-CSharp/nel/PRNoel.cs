using System;
using evt;
using m2d;
using nel.gm;
using PixelLiner.PixelLinerLib;

namespace nel
{
	public sealed class PRNoel : PR
	{
		public bool isSpOutFit()
		{
			return this.outfit_type_ == NoelAnimator.OUTFIT.BABYDOLL || this.outfit_type_ == NoelAnimator.OUTFIT.DOJO;
		}

		public override void newGame()
		{
			this.hp = (this.maxhp = 150);
			this.mp = (this.maxmp = 200);
			base.newGame();
			this.setOutfitType(NoelAnimator.OUTFIT.NORMAL, false, false);
			this.Ser.clear();
			this.jamming_count = 0;
			this.EpCon.newGame();
			this.EggCon.clear(false);
			this.GaugeBrk.reset();
			UiBenchMenu.newGame();
			base.key = "Noel";
			base.water_drunk = 30;
			this.water_drunk_cache = 0;
			this.outfit_type_ = NoelAnimator.OUTFIT.NORMAL;
			if (this.Anm != null && this.Anm.FlgDropCane != null)
			{
				this.Anm.FlgDropCane.Clear();
			}
		}

		protected override void createAnimator(M2PxlAnimator PAnm)
		{
			this.Anm = new NoelAnimator(this, PAnm as M2PxlAnimatorRT);
		}

		public override void appear(Map2d Mp)
		{
			base.appear(Mp);
			if (this.jamming_count > 0)
			{
				this.Ser.Cure(SER.JAMMING);
			}
			this.jamming_count = 0;
		}

		public override void runPre()
		{
			base.runPre();
			if (this.jamming_count > 0)
			{
				this.Ser.Add(SER.JAMMING, 70, this.jamming_count - 1, false);
				this.jamming_count = 0;
			}
		}

		public bool hasJamming()
		{
			return this.jamming_count > 0 || this.Ser.has(SER.JAMMING);
		}

		public void addJamming()
		{
			this.jamming_count++;
		}

		public void readBinaryFrom(ByteArray Ba, SVD.sFile Sf)
		{
			this.newGame();
			this.maxhp = (int)Sf.maxhp_noel;
			this.maxmp = (int)Sf.maxmp_noel;
			this.hp = (int)Sf.hp_noel;
			this.mp = (int)Sf.mp_noel;
			int num = Ba.readByte();
			int num2 = (int)Ba.readUShort();
			int num3 = (int)Ba.readUShort();
			Sf.last_saved_x = (float)num2;
			Sf.last_saved_y = (float)num3;
			Sf.revert_pos = true;
			if (num >= 1)
			{
				base.water_drunk = Ba.readInt();
				if (num >= 8)
				{
					this.water_drunk_cache = Ba.readInt();
					if (num >= 9)
					{
						this.juice_stock = Ba.readByte();
					}
				}
			}
			this.Ser.readBinaryFrom(Ba, num >= 10);
			this.EpCon.readBinaryFrom(Ba);
			this.EggCon.readBinaryFrom(Ba, num >= 12);
			this.GaugeBrk.readBinaryFrom(Ba, num >= 11);
			if (num >= 4)
			{
				SkillManager.readBinaryFrom(Ba, (int)Sf.version);
				UiBenchMenu.readBinaryFrom(Ba);
			}
			this.Skill.readBinaryFrom(Ba, num);
			if (num >= 2)
			{
				this.BetoMng.readBinaryFrom(Ba);
				if (num >= 5)
				{
					this.setOutfitType((NoelAnimator.OUTFIT)Ba.readByte(), true, false);
					if (num >= 6)
					{
						this.pee_lock = (byte)Ba.readByte();
						if (num >= 7)
						{
							this.GSaver.readBinaryFrom(Ba);
						}
					}
				}
			}
			this.EggCon.fineActivate();
			this.EpCon.fineCounter();
			this.GSaver.FineAll(true);
			base.fineFrozenAppearance();
			this.pre_camera_y = -1000f;
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(12);
			Ba.writeUShort((ushort)base.x);
			Ba.writeUShort((ushort)(base.mbottom + 0.12f));
			Ba.writeInt(base.water_drunk);
			Ba.writeInt(this.water_drunk_cache);
			Ba.writeByte(this.juice_stock);
			this.Ser.writeBinaryTo(Ba);
			this.EpCon.writeBinaryTo(Ba);
			this.EggCon.writeBinaryTo(Ba);
			this.GaugeBrk.writeBinaryTo(Ba);
			SkillManager.writeBinaryTo(Ba);
			UiBenchMenu.writeBinaryTo(Ba);
			this.Skill.writeBinaryTo(Ba);
			this.BetoMng.writeBinaryTo(Ba);
			Ba.writeByte((int)((byte)this.outfit_type_));
			Ba.writeByte((int)this.pee_lock);
			this.GSaver.writeBinaryTo(Ba);
		}

		public NoelAnimator.OUTFIT outfit_type
		{
			get
			{
				return this.outfit_type_;
			}
		}

		public void fineOutfitType()
		{
			this.setOutfitType(this.outfit_type_, true, false);
		}

		public void setOutfitType(NoelAnimator.OUTFIT type, bool force = false, bool fine_state = false)
		{
			if (type == this.outfit_type_ && !force)
			{
				return;
			}
			this.outfit_type_ = type;
			if (this.outfit_type_ == NoelAnimator.OUTFIT.BABYDOLL)
			{
				this.outfit_default_mnp = (M2MoverPr.PR_MNP)35;
				this.outfit_default_dcl = M2MoverPr.DECL.STOP_SPECIAL_OUTFIT;
			}
			else
			{
				this.outfit_default_mnp = (M2MoverPr.PR_MNP)0;
				this.outfit_default_dcl = (M2MoverPr.DECL)0;
			}
			if (this.Anm != null)
			{
				this.Anm.finePose(0);
			}
			this.fineFootStampType();
			base.recheck_emot = true;
			if (fine_state)
			{
				base.changeState(this.state);
			}
		}

		public override void fineFootStampType()
		{
			if (this.Phy != null)
			{
				this.Phy.getFootManager().footstamp_type = ((this.outfit_type_ == NoelAnimator.OUTFIT.BABYDOLL) ? FOOTSTAMP.BAREFOOT : FOOTSTAMP.SHOES);
			}
		}

		public override bool canStartFrustratedMasturbate(bool starting = true)
		{
			return this.outfit_type_ == NoelAnimator.OUTFIT.NORMAL && base.canStartFrustratedMasturbate(starting);
		}

		public override bool isWetPose()
		{
			return !this.isSpOutFit() && base.isWetPose();
		}

		public override bool isWeakPose()
		{
			return !this.isSpOutFit() && base.isWeakPose();
		}

		public override bool canUseBombState(NelItem Itm)
		{
			return !this.isSpOutFit() && base.canUseBombState(Itm);
		}

		public override bool getEmotDefault(out string fade_key, ref UIPictureBase.EMSTATE estate, ref bool force_change)
		{
			base.getEmotDefault(out fade_key, ref estate, ref force_change);
			if (this.isSpOutFit())
			{
				fade_key = "pajama";
			}
			else if (base.isLayingEgg() && this.Ser.has(SER.LAYING_EGG))
			{
				fade_key = "laying_egg";
			}
			else if (base.isOrgasm())
			{
				fade_key = (base.isPoseBackDown(false) ? "down_b" : "down");
			}
			else if (base.isGameoverRecover())
			{
				estate |= UIPictureBase.EMSTATE.TORNED;
				fade_key = (base.isPoseBackDown(false) ? "down_b" : "down");
			}
			else if (base.isSleepingDownState())
			{
				fade_key = (base.isPoseBackDown(false) ? "down_b" : "down");
			}
			else if (base.isPiyoStunState())
			{
				fade_key = "crouch";
			}
			else if (this.isDamagingOrKo())
			{
				if (this.UP.isPreTortureEmot() == 1 && base.isAbsorbState())
				{
					return false;
				}
				if (base.isDownState() || base.isPoseCrouch(false) || this.state == PR.STATE.DOWN_STUN)
				{
					fade_key = (base.isPoseBackDown(false) ? "down_b" : (base.isPoseDown(false) ? "down" : ((this.state == PR.STATE.DOWN_STUN) ? "down_b" : "crouch")));
				}
			}
			else if (base.isBreatheStop(true, true))
			{
				fade_key = "damage_gas_hit";
			}
			else if (base.isBreatheStop(false, false))
			{
				fade_key = "damage_gas";
			}
			else if (base.isMasturbateState() && this.Onnie != null)
			{
				fade_key = "masturbate";
				estate |= this.Onnie.checkUiPlayerState(UIPictureBase.EMSTATE.NORMAL);
				force_change = true;
			}
			else if (base.isOnBench(true))
			{
				if (base.poseIsBenchMusturbOrgasm())
				{
					force_change = true;
					if (this.Onnie != null)
					{
						estate |= this.Onnie.checkUiPlayerState(UIPictureBase.EMSTATE.NORMAL);
					}
					fade_key = "masturbate";
				}
				else
				{
					fade_key = "bench";
				}
			}
			else
			{
				fade_key = "_";
			}
			return true;
		}

		public override UIPictureBase.EMSTATE getEmotState()
		{
			UIPictureBase.EMSTATE emstate = base.getEmotState();
			if (this.isSpOutFit())
			{
				emstate &= UIPictureBase.EMSTATE.SHAMED;
				if (this.BetoMng.isActive())
				{
					emstate |= UIPictureBase.EMSTATE.SLEEP;
				}
			}
			else if (this.EggCon.total > (int)(base.get_maxmp() * (float)CFG.sp_threshold_pregnant * 0.01f))
			{
				emstate |= UIPictureBase.EMSTATE.SHAMED | UIPictureBase.EMSTATE.LOWMP | UIPictureBase.EMSTATE.BOTE;
			}
			return emstate;
		}

		public bool isCastingSpecificMagic(MGKIND k)
		{
			return this.Skill.isCastingSpecificMagic(k);
		}

		public PRNoel.WaitListenerNoelExplodeMagic getWaitListenerNoelExplodeMagic()
		{
			return new PRNoel.WaitListenerNoelExplodeMagic(this);
		}

		public PRNoel.WaitListenerNoelExplodeBurst getWaitListenerNoelBurst()
		{
			return new PRNoel.WaitListenerNoelExplodeBurst(this);
		}

		private NoelAnimator.OUTFIT outfit_type_;

		public int jamming_count;

		public class WaitListenerNoelExplodeMagic : IEventWaitListener
		{
			public WaitListenerNoelExplodeMagic(PRNoel _Pr)
			{
				this.Pr = _Pr;
			}

			public bool EvtWait(bool is_first = false)
			{
				return !this.Pr.magic_exploded;
			}

			private readonly PRNoel Pr;
		}

		public class WaitListenerNoelExplodeBurst : IEventWaitListener
		{
			public WaitListenerNoelExplodeBurst(PRNoel _Pr)
			{
				this.Pr = _Pr;
			}

			public bool EvtWait(bool is_first = false)
			{
				if (!this.exploded)
				{
					if (this.Pr.isBurstState())
					{
						this.exploded = true;
					}
					return true;
				}
				return this.Pr.isBurstState();
			}

			private readonly PRNoel Pr;

			private bool exploded;
		}
	}
}
