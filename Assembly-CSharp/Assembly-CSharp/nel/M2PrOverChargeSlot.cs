using System;
using PixelLiner;
using XX;

namespace nel
{
	public sealed class M2PrOverChargeSlot : M2PrAssistant
	{
		public M2PrOverChargeSlot(PR _Pr)
			: base(_Pr)
		{
			this.Ptc0 = new EfParticleOnce("ui_oc_slot_0", EFCON_TYPE.UI);
			this.Ptc1 = new EfParticleOnce("ui_oc_slot_1", EFCON_TYPE.UI);
		}

		public override void initS()
		{
			base.initS();
			bool flag = this.charged_count > 0 || this.stock_mp > 0f;
			this.stock_mp = 0f;
			this.lock_t = -1f;
			this.effect_t = (this.effect_use_t = -1f);
			this.effect_use_count = 0;
			this.charged_count = 0;
			this.overchageable_t = -1f;
			this.chargeable_mp_ = -1;
			this.effect_appeared_slot = 0;
			this.clearMagic();
			if (flag)
			{
				this.fineUi(true);
			}
		}

		public void clearMagic()
		{
			if (this.AMg != null)
			{
				if (this.AMg[0] != null)
				{
					this.fineUi(false);
				}
				X.ALLN<MagicItem>(this.AMg);
			}
		}

		private void fineUi(bool fine_effect = false)
		{
			if (UIStatus.Instance != null)
			{
				UIStatus.Instance.fineOcSlot(this, this.charged_count > 0, fine_effect);
			}
		}

		public override void newGame()
		{
			base.newGame();
			this.AMg = new MagicItem[4];
			this.max_slot = 0;
			this.initS();
		}

		public void fineSlots(bool force = true)
		{
			int count = this.NM2D.IMNG.getInventoryPrecious().getCount(NelItem.GetById("oc_slot", false), -1);
			if (this.max_slot != count || force)
			{
				this.max_slot = count;
				this.fineUi(false);
				if (this.AMg.Length < this.max_slot)
				{
					Array.Resize<MagicItem>(ref this.AMg, this.max_slot);
				}
			}
		}

		public int chargeable_mp
		{
			get
			{
				if (this.chargeable_mp_ < 0)
				{
					this.chargeable_mp_ = X.Mx(0, (this.max_slot - this.charged_count) * 28 - (int)this.stock_mp);
				}
				return this.chargeable_mp_;
			}
		}

		public void getChargeableMp(out int o_charged, out int o_charge_max)
		{
			o_charge_max = this.max_slot * 28;
			o_charged = this.charged_count * 28 + (int)this.stock_mp;
		}

		public void runEffect(int fcnt)
		{
			if (this.charged_count == 0)
			{
				return;
			}
			this.anm_t += fcnt;
			if (this.anm_t >= 3)
			{
				this.anm_t -= 3;
				this.fineUi(false);
			}
		}

		public void runPost(float fcnt)
		{
			if (this.overchageable_t >= 0f)
			{
				if (this.Pr.Ser.overchargeable_ratio > 0f)
				{
					this.overchageable_t = 80f;
				}
				else
				{
					this.overchageable_t -= fcnt;
					if (this.overchageable_t <= 0f)
					{
						this.overchageable_t = -1f;
					}
				}
			}
			if (this.effect_t >= 0f)
			{
				this.effect_t += fcnt;
				if (this.effect_t >= 20f)
				{
					this.fineUi(true);
					if (this.effect_appeared_slot >= this.charged_count)
					{
						this.effect_t = -1f;
					}
					else
					{
						SND.Ui.play("mag_overcharge_" + X.Mn(7, this.effect_appeared_slot).ToString(), false);
						this.effect_appeared_slot++;
						this.effect_t = 0f;
					}
				}
			}
			if (this.effect_use_t >= 0f)
			{
				this.effect_use_t += fcnt;
				if (this.effect_use_t >= 20f)
				{
					if (this.effect_use_count < 0)
					{
						this.effect_use_count = X.Abs(this.effect_use_count);
					}
					this.fineUi(true);
					if (this.effect_use_count >= this.charged_count || this.AMg[this.effect_use_count] == null)
					{
						this.effect_use_count = -this.effect_use_count;
						this.effect_use_t = -1f;
					}
					else
					{
						SND.Ui.play("mag_overcharge_use_" + X.Mn(7, this.effect_use_count).ToString(), false);
						this.effect_use_count++;
						this.effect_use_t = 0f;
					}
				}
			}
			if (this.explode_lock_t >= 0f)
			{
				this.explode_lock_t -= fcnt;
			}
			if (this.lock_t < 0f || EnemySummoner.isActiveBorder())
			{
				return;
			}
			this.lock_t -= fcnt;
			if (this.lock_t <= 0f)
			{
				this.fineUi(false);
				if (this.stock_mp >= 0f)
				{
					this.stock_mp = X.Mx(this.stock_mp + -0.16666667f, 0f);
					this.lock_t = (float)((this.stock_mp <= 0f) ? (-1) : 1);
					return;
				}
				this.lock_t = -1f;
			}
		}

		public float ser_overchargeable_ratio
		{
			get
			{
				if (this.overchageable_t < 0f && this.Pr.Ser.overchargeable_ratio > 0f)
				{
					this.overchageable_t = 80f;
				}
				if (this.overchageable_t < 0f)
				{
					return 0f;
				}
				return this.Pr.Ser.overchargeable_ratio;
			}
		}

		public int getMana(float mp, ref int add0)
		{
			if (this.charged_count >= this.max_slot || mp <= 0f)
			{
				return 0;
			}
			if (this.explode_lock_t > 0f)
			{
				mp *= 0.125f;
			}
			this.lock_t = 140f;
			this.stock_mp += mp;
			int num = 0;
			this.fineUi(false);
			this.chargeable_mp_ = -1;
			while (this.stock_mp >= 28f)
			{
				this.stock_mp -= 28f;
				if (this.effect_t < 0f)
				{
					this.effect_t = 20f;
				}
				num += 28;
				int num2 = this.charged_count + 1;
				this.charged_count = num2;
				if (num2 >= this.max_slot)
				{
					num = X.Mx(0, num - (int)this.stock_mp);
					this.stock_mp = 0f;
					break;
				}
			}
			add0 -= num;
			this.draw_stock_id = (int)(7f * this.stock_mp / 28f);
			if (this.stock_mp == 0f)
			{
				this.lock_t = -1f;
			}
			return num;
		}

		public bool drawTo(MeshDrawer Md, float x, float y)
		{
			if (this.max_slot == 0)
			{
				return false;
			}
			PxlSequence aocSlots = MTR.AOcSlots;
			int num = this.draw_stock_id;
			for (int i = 0; i < this.max_slot; i++)
			{
				if (i < this.effect_appeared_slot)
				{
					if (this.AMg[i] != null)
					{
						Md.RotaPF(x, y, 0.5f, 0.5f, 0f, aocSlots[15 + X.ANM(IN.totalframe, 9, 3f)], false, false, false, uint.MaxValue, false, 0);
					}
					else
					{
						Md.RotaPF(x, y, 0.5f, 0.5f, 0f, aocSlots[7 + X.ANM(IN.totalframe, 8, 3f)], false, false, false, uint.MaxValue, false, 0);
					}
				}
				else
				{
					Md.RotaPF(x, y, 0.5f, 0.5f, 0f, aocSlots[num], false, false, false, uint.MaxValue, false, 0);
					num = 0;
				}
				x += 8f;
			}
			return this.effect_t >= 0f || this.effect_use_t >= 0f;
		}

		public bool drawEffectTo(MeshDrawer Md, float x, float y)
		{
			if (this.max_slot == 0)
			{
				return false;
			}
			bool flag = false;
			Md.Col = MTRX.ColWhite;
			if (this.effect_t >= 0f && this.effect_appeared_slot > 0)
			{
				float num = x + (float)(this.effect_appeared_slot - 1) * 8f;
				if (this.effect_t >= 4f)
				{
					flag = this.Ptc1.drawTo(Md, num, y, this.effect_t - 4f, 0f) || flag;
				}
				flag = this.Ptc0.drawTo(Md, num, y, this.effect_t, 0f) || flag;
			}
			if (this.effect_use_t >= 0f && this.effect_use_count > 0)
			{
				float num2 = x + (float)(this.effect_use_count - 1) * 8f;
				Md.StripedDaia(num2, y, 28f, X.ANMPT(13, 1f), X.Mn(0.5f, 1f - X.ZLINE(this.effect_use_t, 20f)), 5f, false);
			}
			return flag;
		}

		public int clearMagic(MagicItem Mg, bool consume = false)
		{
			if (Mg == null)
			{
				return 0;
			}
			consume = consume || this.reserve_consume_flag;
			int num = 0;
			for (int i = this.max_slot - 1; i >= 0; i--)
			{
				if (this.AMg[i] == Mg)
				{
					X.spliceEmpty<MagicItem>(this.AMg, i, 1);
					num++;
				}
			}
			if (num > 0)
			{
				if (consume)
				{
					this.charged_count = X.Mx(this.charged_count - num, 0);
					this.effect_appeared_slot = X.Mn(this.effect_appeared_slot, this.charged_count);
					this.chargeable_mp_ = -1;
					this.explode_lock_t = X.Mx(this.explode_lock_t, 120f);
				}
				bool flag = false;
				if (this.effect_use_count != 0)
				{
					this.effect_use_count = 0;
					this.effect_use_t = -1f;
					flag = true;
				}
				this.fineUi(flag);
				return num * 28;
			}
			return 0;
		}

		public void replace(MagicItem Pre, MagicItem NewItem)
		{
			if (Pre == null || NewItem == null)
			{
				return;
			}
			for (int i = this.max_slot - 1; i >= 0; i--)
			{
				if (this.AMg[i] == Pre)
				{
					this.AMg[i] = NewItem;
				}
			}
		}

		public bool UseCheck(MagicItem Mg, int stock_count = 1)
		{
			if (Mg == null || this.Pr.isPuzzleManagingMp())
			{
				return false;
			}
			int num = 0;
			for (int i = 0; i < this.charged_count; i++)
			{
				if (this.AMg[i] == null)
				{
					if (++num >= stock_count)
					{
						for (int j = i + stock_count - 1; j >= i; j--)
						{
							this.AMg[j] = Mg;
						}
						if (this.effect_use_t < 0f)
						{
							this.effect_use_t = 20f;
						}
						this.effect_use_count = X.Abs(this.effect_use_count);
						this.fineUi(true);
						return true;
					}
				}
				else if (num > 0)
				{
					return false;
				}
			}
			return false;
		}

		public int currentHoldMpForUi(MagicItem Mg, int real_mp_hold)
		{
			for (int i = 0; i < this.charged_count; i++)
			{
				if (this.AMg[i] == Mg)
				{
					return 0;
				}
			}
			return real_mp_hold;
		}

		public bool isActive(MagicItem Mg)
		{
			if (Mg == null)
			{
				return false;
			}
			for (int i = 0; i < this.charged_count; i++)
			{
				if (this.AMg[i] == Mg)
				{
					return true;
				}
			}
			return false;
		}

		public int getUseableStock()
		{
			return this.charged_count;
		}

		public bool isActive()
		{
			return this.max_slot > 0;
		}

		public bool isEffectActive()
		{
			return this.effect_t >= 0f || this.effect_use_t >= 0f;
		}

		public bool isUseHolding()
		{
			return this.charged_count > 0 && this.AMg[0] != null;
		}

		public const int mp_one_slot = 28;

		public const int LOCK_TIME_MAX = 140;

		public const int EFFECT_DELAY = 20;

		public const int OVERCHARGE_DELAY = 80;

		public const float EXLOCK_RATIO = 0.125f;

		public const float reduce_spd = -0.16666667f;

		public const string item_key = "oc_slot";

		private float stock_mp;

		private float lock_t = -1f;

		private float explode_lock_t;

		private float effect_t = -1f;

		private float effect_use_t = -1f;

		private int effect_use_count;

		private int effect_appeared_slot;

		private int max_slot;

		private int charged_count;

		public int chargeable_mp_;

		public float overchageable_t = -1f;

		public const float EXPLODE_LOCK = 120f;

		public bool reserve_consume_flag;

		private int draw_stock_id;

		private int anm_t;

		private const int ANM_SPEED = 3;

		private const float intv_x = 8f;

		public MagicItem[] AMg;

		private EfParticleOnce Ptc0;

		private EfParticleOnce Ptc1;
	}
}
