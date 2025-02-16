using System;
using System.Collections.Generic;
using m2d;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class PrEggManager
	{
		public static bool no_auto_getout(PrEggManager.CATEG c)
		{
			return c == PrEggManager.CATEG.GOLEM_OD;
		}

		public static bool is_liquid(PrEggManager.CATEG c)
		{
			return c == PrEggManager.CATEG.GOLEM_OD || c == PrEggManager.CATEG.FOX;
		}

		public PrEggManager(PR _Pr)
		{
			this.Pr = _Pr;
			this.AItm = new PrEggManager.PrEggItem[5];
		}

		public PrEggManager.PrEggItem Get(PrEggManager.CATEG categ)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				PrEggManager.PrEggItem prEggItem = this.AItm[i];
				if (prEggItem != null && prEggItem.categ == categ)
				{
					return prEggItem;
				}
			}
			return null;
		}

		public PrEggManager clear(bool fine_up_after = false)
		{
			this.LEN = 0;
			this.worm_total = 0;
			this.egg_water_exist = 0;
			this.total_ = (this.status_reduce_max_ = -1);
			this.check_holded_mp = 0f;
			if (this.Pr.UP != null)
			{
				UIPictureBase.FlgStopAutoFade.Rem("EggCon");
			}
			if (!this.lock_fine)
			{
				this.egg_laying = 0;
				this.fineActivate();
			}
			if (fine_up_after)
			{
				if (UIStatus.PrIs(this.Pr))
				{
					UIStatus.Instance.finePlantedEgg();
					UIStatus.Instance.fineMpRatio(false, false);
				}
				this.Pr.recheck_emot = true;
			}
			return this;
		}

		public void fineActivate()
		{
			if (this.lock_fine)
			{
				return;
			}
			if (UIStatus.PrIs(this.Pr))
			{
				UIStatus.Instance.finePlantedEgg();
			}
			this.fineSer();
		}

		private void fineSer()
		{
			if (this.isActive())
			{
				if (this.egg_laying == 0)
				{
					this.Pr.Ser.Add(SER.EGGED, -1, 99, false);
				}
				else
				{
					M2SerItem m2SerItem = this.Pr.Ser.Find(SER.EGGED);
					if (m2SerItem != null)
					{
						m2SerItem.setReleaseTime(90);
					}
				}
				this.Pr.Ser.checkSer();
				this.Pr.GSaver.GsMp.Fine(false);
				this.Pr.recheck_emot = true;
				return;
			}
			this.Pr.Ser.Cure(SER.EGGED);
		}

		public PrEggManager fine()
		{
			if (!this.isActive())
			{
				return this;
			}
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				if (this.AItm[i].val <= 0f)
				{
					global::XX.X.spliceEmpty<PrEggManager.PrEggItem>(this.AItm, i, 1);
					this.total_ = (this.status_reduce_max_ = (this.egg_water_exist = -1));
					this.LEN--;
				}
			}
			if (this.LEN == 0)
			{
				this.clear(false);
			}
			return this;
		}

		public int Remove(PrEggManager.CATEG categ)
		{
			int num = 0;
			this.total_ = -1;
			this.status_reduce_max_ = -1;
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				PrEggManager.PrEggItem prEggItem = this.AItm[i];
				if (prEggItem.categ == categ)
				{
					num += (int)prEggItem.val;
					global::XX.X.spliceEmpty<PrEggManager.PrEggItem>(this.AItm, i, 1);
					this.total_ = (this.status_reduce_max_ = (this.egg_water_exist = -1));
					this.LEN--;
				}
			}
			if (UIStatus.PrIs(this.Pr))
			{
				UIStatus.Instance.finePlantedEgg();
				UIStatus.Instance.fineMpRatio(false, false);
			}
			if (this.LEN == 0)
			{
				this.clear(false);
			}
			return num;
		}

		public int Reduce(PrEggManager.CATEG categ, float level)
		{
			int num = 0;
			this.total_ = -1;
			this.status_reduce_max_ = -1;
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				PrEggManager.PrEggItem prEggItem = this.AItm[i];
				if (prEggItem.categ == categ && prEggItem.val > 0f)
				{
					int num2 = (int)prEggItem.val;
					prEggItem.val = global::XX.X.Mx(prEggItem.val * (1f - level), global::XX.X.Mn(prEggItem.val, 5f));
					num += num2 - (int)prEggItem.val;
					prEggItem.val_absorbed *= 1f - level;
					prEggItem.val_clip = (int)global::XX.X.Mx(0f, (float)prEggItem.val_clip * (1f - level));
				}
			}
			if (UIStatus.PrIs(this.Pr))
			{
				UIStatus.Instance.finePlantedEgg();
				UIStatus.Instance.fineMpRatio(false, false);
			}
			if (this.LEN == 0)
			{
				this.clear(false);
			}
			return num;
		}

		public void dropEggItem(PrEggManager.CATEG categ, int count, int grade, float vx, float vy)
		{
			if (!this.Pr.is_alive)
			{
				return;
			}
			if (categ == PrEggManager.CATEG.WORM)
			{
				count = global::XX.X.Mx(0, MDAT.getWormEggLayableMax(this.Pr) - this.worm_total);
				this.worm_total += count;
			}
			if (count <= 0)
			{
				return;
			}
			bool flag = PrEggManager.is_liquid(categ);
			Vector3 hipPos = this.Pr.getHipPos();
			int num = global::XX.CAim._XD((int)hipPos.z, 1);
			this.Pr.NM2D.IMNG.dropManual(NelItem.GetById(flag ? "mtr_essence0" : "mtr_noel_egg", false), count, global::XX.X.MMX(0, grade, 4), hipPos.x + 0.4f * (float)num, hipPos.y, vx * global::XX.X.NIXP(1f, 1.4f), vy - 0.03f, null, false, NelItemManager.TYPE.NORMAL);
		}

		public void progressAfterBattle()
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				this.AItm[i].progressAfterBattle();
			}
		}

		public void run(float fcnt)
		{
			if (!this.isActive())
			{
				return;
			}
			bool flag = false;
			bool flag2 = this.Pr.M2D.pre_map_active;
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				PrEggManager.PrEggItem prEggItem = this.AItm[i];
				if (!prEggItem.run(fcnt, (this.egg_laying >= 3 || (!this.Pr.Ser.has(SER.DO_NOT_LAY_EGG) && !this.Pr.Ser.has(SER.EGGED))) && flag2))
				{
					global::XX.X.spliceEmpty<PrEggManager.PrEggItem>(this.AItm, i, 1);
					this.total_ = (this.status_reduce_max_ = (this.egg_water_exist = -1));
					this.LEN--;
					flag2 = false;
				}
				else if (flag2 && prEggItem.isLayingEgg())
				{
					if (this.egg_laying == 1 || this.egg_laying == 2)
					{
						if (this.Pr.runLayingEggCheck(true))
						{
							this.egg_laying = 3;
							float num = 40f;
							for (int j = 0; j <= i; j++)
							{
								num += global::XX.X.Mx(-this.AItm[j].add_cushion / 0.35f, 200f);
							}
							this.egg_voice_time = global::XX.X.NIXP(110f, 180f);
							this.Pr.PtcST("laying_egg_activate", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
							PostEffect.IT.setPEfadeinout(POSTM.ZOOM2, 60f, num, 1f, 0);
							PostEffect.IT.setPEfadeinout(POSTM.LAYING_EGG, 140f, num, 1f, 0);
							PostEffect.IT.setPEfadeinout(POSTM.HEARTBEAT, 150f, global::XX.X.Mx(num - 120f, 0f), 1f, 0);
						}
						else if (this.egg_laying == 1)
						{
							this.egg_laying = 2;
							this.fineSer();
							UILog.Instance.AddAlertTX("Alert_nearly_laying", UILogRow.TYPE.ALERT_EGG);
						}
					}
					flag2 = false;
				}
				if (this.egg_laying > 0 && !flag)
				{
					flag = prEggItem.isLayingEgg();
				}
			}
			if (this.egg_laying > 0)
			{
				if (flag)
				{
					M2SerItem m2SerItem = this.Pr.Ser.Find(SER.LAYING_EGG);
					if (m2SerItem != null)
					{
						m2SerItem.fineSer(100);
					}
					if (this.egg_laying == 3)
					{
						this.egg_voice_time -= fcnt;
						if (this.egg_voice_time < 0f)
						{
							this.Pr.playVo("laying_s", false, false);
							this.Pr.PadVib("laying_egg_s", global::XX.X.NIXP(0.4f, 1f));
							this.egg_voice_time = global::XX.X.NIXP(40f, 90f);
						}
					}
				}
				else
				{
					this.egg_laying = 0;
				}
			}
			if (this.t_need_fine_mp > 0f)
			{
				this.t_need_fine_mp -= fcnt;
				if (this.t_need_fine_mp <= 0f)
				{
					this.fineMp();
				}
			}
			if (this.LEN == 0)
			{
				this.clear(false);
				this.Pr.NM2D.IMNG.fineSpecialNoelRow(this.Pr);
				this.Pr.UP.recheck(80, 100);
				this.Pr.fineMpClip(false, false);
			}
		}

		public PrEggManager.PrEggItem Add(PrEggManager.CATEG categ, int val)
		{
			bool flag = this.LEN > 0;
			PrEggManager.PrEggItem prEggItem = this.Get(categ);
			if (prEggItem == null)
			{
				PrEggManager.PrEggItem[] aitm = this.AItm;
				int len = this.LEN;
				this.LEN = len + 1;
				prEggItem = (aitm[len] = new PrEggManager.PrEggItem(this, categ));
				if (!EnemySummoner.isActiveBorder())
				{
					prEggItem.progressAfterBattle();
				}
			}
			this.total_ = (this.status_reduce_max_ = -1);
			if (this.egg_water_exist >= 0)
			{
				this.egg_water_exist |= (prEggItem.no_auto_getout ? 2 : 1);
			}
			prEggItem.val += (float)val;
			prEggItem.add_cushion = (float)val;
			prEggItem.t_cushion = 0f;
			for (int i = 0; i < this.LEN; i++)
			{
				PrEggManager.PrEggItem prEggItem2 = this.AItm[i];
				if (prEggItem != prEggItem2 && prEggItem2.add_cushion > 0f)
				{
					prEggItem2.add_cushion = 0f;
				}
			}
			if (!flag)
			{
				this.need_fine_mp = false;
				this.fineActivate();
				this.Pr.NM2D.IMNG.fineSpecialNoelRow(this.Pr);
			}
			if (PrEggManager.is_liquid(categ))
			{
				this.Pr.EpCon.addEggLayCount(categ, val);
			}
			if (this.isEgged2Active())
			{
				for (int j = 0; j < this.LEN; j++)
				{
					PrEggManager.PrEggItem prEggItem3 = this.AItm[j];
					if (prEggItem.val_absorbed > 0f)
					{
						prEggItem3.val_absorbed = global::XX.X.Mn(prEggItem3.val_absorbed, (float)prEggItem3.val_clip);
					}
				}
			}
			this.fineSer();
			return prEggItem;
		}

		public void clipLevel(float val)
		{
			this.total_ = (this.status_reduce_max_ = -1);
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				this.AItm[i].clipLevel(val);
			}
		}

		public bool need_fine_mp
		{
			get
			{
				return this.t_need_fine_mp > 0f;
			}
			set
			{
				if (value && this.LEN > 0)
				{
					this.t_need_fine_mp = 28f;
				}
			}
		}

		public bool fineMp()
		{
			this.t_need_fine_mp = 0f;
			if (this.egg_laying > 0)
			{
				this.check_holded_mp = 0f;
				return false;
			}
			bool flag = this.Pr.canProgressLayingEgg();
			float num = (flag ? this.check_holded_mp : 0f);
			this.check_holded_mp = 0f;
			for (int i = 0; i < this.LEN; i++)
			{
				PrEggManager.PrEggItem prEggItem = this.AItm[i];
				if (!prEggItem.no_auto_getout || !EnemySummoner.isActiveBorder())
				{
					if (num > 0f)
					{
						float num2 = num * prEggItem.mp_hold_absorb_ratio;
						prEggItem.val_absorbed += global::XX.X.Mn(num2, prEggItem.val * prEggItem.mp_hold_absorb_ratio);
						if (num2 < prEggItem.val)
						{
							num = 0f;
						}
						else
						{
							num -= prEggItem.val;
						}
					}
					if (flag && global::XX.X.XORSP() < 1f * ((float)prEggItem.val_absorbed_clip / prEggItem.val - 0.5f) / 0.5f)
					{
						this.status_reduce_max_ = -1;
						this.egg_laying = 1;
						for (int j = 0; j <= i; j++)
						{
							this.AItm[j].initLayingEgg(false);
						}
						return true;
					}
				}
			}
			return false;
		}

		public bool forcePushout(bool only_no_getout_item = false, bool decline_egg_item = false)
		{
			bool flag = false;
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				PrEggManager.PrEggItem prEggItem = this.AItm[i];
				if (!only_no_getout_item || prEggItem.no_auto_getout)
				{
					if (!prEggItem.isLayingEgg())
					{
						this.status_reduce_max_ = -1;
						if (this.egg_laying <= 0)
						{
							this.egg_laying = 1;
						}
						prEggItem.initLayingEgg(decline_egg_item);
						flag = true;
					}
					else if (decline_egg_item)
					{
						prEggItem.eggitem_count = 0;
					}
				}
			}
			return flag;
		}

		public float DrawTo(MeshDrawer Md, float x, float y, float total_width, float h, float cushion_mp_ratio = 0f)
		{
			if (this.LEN == 0)
			{
				return 0f;
			}
			float maxmp = this.Pr.get_maxmp();
			float num = global::XX.X.Mx(this.Pr.get_mp(), this.Pr.GSaver.GsMp.saved_gauge_value);
			float num2 = 0f;
			float num3 = (float)this.Pr.Skill.getHoldingMp(false);
			float num4 = num3;
			if (this.Pr.canProgressLayingEgg() && num3 > 0f)
			{
				for (int i = 0; i < this.LEN; i++)
				{
					PrEggManager.PrEggItem prEggItem = this.AItm[i];
					float num5 = num3 * prEggItem.mp_hold_absorb_ratio;
					if (num5 < prEggItem.val)
					{
						num2 += num5;
						break;
					}
					num2 += prEggItem.val;
					num3 -= prEggItem.val;
					if (num3 <= 0f)
					{
						break;
					}
				}
			}
			float num6 = (float)((int)global::XX.X.Mx(0f, total_width * (global::XX.X.Mn(maxmp - (float)this.total, num) - num2) / maxmp - cushion_mp_ratio));
			float num7 = (this.Pr.canProgressLayingEgg() ? num4 : 0f);
			float num8 = 0f;
			for (int j = 0; j < this.LEN; j++)
			{
				float num9 = this.AItm[j].DrawTo(Md, j, x + num6, y, total_width, h, maxmp, num8, ref num7);
				num8 += num9;
				x += num9;
			}
			return num8;
		}

		public int total_real
		{
			get
			{
				if (this.total_ < 0)
				{
					float num = 0f;
					for (int i = 0; i < this.LEN; i++)
					{
						PrEggManager.PrEggItem prEggItem = this.AItm[i];
						if (prEggItem != null)
						{
							num += prEggItem.val;
						}
					}
					this.total_ = (int)num;
				}
				return this.total_;
			}
		}

		public int total
		{
			get
			{
				return global::XX.X.Mn(this.total_real, (int)this.Pr.get_maxmp());
			}
		}

		private void fineEggWaterFlag()
		{
			this.egg_water_exist = 0;
			for (int i = 0; i < this.LEN; i++)
			{
				PrEggManager.PrEggItem prEggItem = this.AItm[i];
				if (prEggItem != null)
				{
					this.egg_water_exist |= (prEggItem.no_auto_getout ? 2 : 1);
				}
			}
		}

		public bool egg_exist
		{
			get
			{
				if (this.egg_water_exist < 0)
				{
					this.fineEggWaterFlag();
				}
				return (this.egg_water_exist & 1) != 0;
			}
		}

		public bool no_getout_exist
		{
			get
			{
				if (this.egg_water_exist < 0)
				{
					this.fineEggWaterFlag();
				}
				return (this.egg_water_exist & 2) != 0;
			}
		}

		public int status_reduce_max
		{
			get
			{
				if (this.status_reduce_max_ < 0)
				{
					float num = 0f;
					bool flag = false;
					for (int i = 0; i < this.LEN; i++)
					{
						PrEggManager.PrEggItem prEggItem = this.AItm[i];
						if (prEggItem != null)
						{
							if (prEggItem.add_cushion > 0f)
							{
								flag = true;
								num += prEggItem.val * (prEggItem.t_cushion / 70f);
							}
							else if (prEggItem.add_cushion < 0f)
							{
								flag = true;
								num += global::XX.X.Mx(0f, prEggItem.val + prEggItem.t_cushion);
							}
							else
							{
								num += prEggItem.val;
							}
						}
					}
					int num2 = (int)num;
					if (!flag)
					{
						this.status_reduce_max_ = num2;
					}
					return num2;
				}
				return this.status_reduce_max_;
			}
		}

		public bool hasNearlyLayingEgg(PrEggManager.CATEG categ)
		{
			for (int i = 0; i < this.LEN; i++)
			{
				PrEggManager.PrEggItem prEggItem = this.AItm[i];
				if (prEggItem.isLayingEgg() && prEggItem.categ == categ)
				{
					return true;
				}
			}
			return false;
		}

		public bool hasNearlyLayingEgg()
		{
			for (int i = 0; i < this.LEN; i++)
			{
				if (this.AItm[i].isLayingEgg())
				{
					return true;
				}
			}
			return false;
		}

		public int listupCountAndGrade(List<PrEggManager.EggInfo> Aout, bool no_random = false)
		{
			int num = 0;
			for (int i = 0; i < this.LEN; i++)
			{
				PrEggManager.PrEggItem prEggItem = this.AItm[i];
				byte b;
				byte b2;
				prEggItem.getCountAndGrade(out b, out b2, no_random);
				num += (int)b;
				Aout.Add(new PrEggManager.EggInfo(b, b2, prEggItem.categ));
			}
			return num;
		}

		public bool status_reduce_animating
		{
			get
			{
				return this.status_reduce_max_ < 0;
			}
		}

		public void checkDripUi(float fcnt)
		{
			if (this.LEN == 0 || this.Pr.Ser.has(SER.WORM_TRAPPED))
			{
				return;
			}
			for (int i = 0; i < this.LEN; i++)
			{
				this.AItm[i].checkDripUi(fcnt);
			}
		}

		public bool isLaying()
		{
			return this.egg_laying > 0;
		}

		private bool lock_fine
		{
			get
			{
				return this.egg_laying == byte.MaxValue;
			}
			set
			{
				if (value)
				{
					this.egg_laying = byte.MaxValue;
				}
			}
		}

		public bool isActive()
		{
			return this.LEN > 0;
		}

		public bool isEgged2Active()
		{
			return this.isActive() && this.Pr.get_maxmp() - (float)this.total_real < this.Pr.get_maxmp() * 0.15f;
		}

		public int category_count
		{
			get
			{
				return this.LEN;
			}
		}

		public PrEggManager.PrEggItem getEggItem(int g)
		{
			return this.AItm[g];
		}

		public void readBinaryFrom(ByteArray Ba, bool read_clip)
		{
			this.lock_fine = true;
			this.clear(false);
			byte b = Ba.readUByte();
			int num = (int)Ba.readUByte();
			for (int i = 0; i < num; i++)
			{
				float num2 = Ba.readFloat();
				PrEggManager.CATEG categ = (PrEggManager.CATEG)Ba.readByte();
				PrEggManager.PrEggItem prEggItem = this.Add(categ, global::XX.X.IntC(num2));
				prEggItem.val = num2;
				prEggItem.readBinaryFrom(Ba, read_clip);
			}
			if (read_clip)
			{
				this.worm_total = Ba.readInt();
			}
			this.egg_laying = b;
			this.egg_water_exist = -1;
			this.fine();
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			if (this.t_need_fine_mp > 0f || this.check_holded_mp != 0f)
			{
				this.t_need_fine_mp = 0f;
				this.fineMp();
			}
			this.fine();
			Ba.writeByte((int)this.egg_laying);
			Ba.writeByte(this.LEN);
			for (int i = 0; i < this.LEN; i++)
			{
				PrEggManager.PrEggItem prEggItem = this.AItm[i];
				Ba.writeFloat(prEggItem.val);
				Ba.writeByte((int)prEggItem.categ);
				prEggItem.writeBinaryTo(Ba);
			}
			Ba.writeInt(this.worm_total);
		}

		private const float lay_egg_ratio = 1f;

		private const float lay_egg_threshold = 0.5f;

		private const float laying_egg_min_time = 200f;

		private const float laying_egg_ser_delay = 60f;

		private PrEggManager.PrEggItem[] AItm;

		private int LEN;

		public float check_holded_mp;

		private float t_need_fine_mp;

		private byte egg_laying;

		public readonly PR Pr;

		private float egg_voice_time;

		private int total_ = -1;

		private int status_reduce_max_ = -1;

		public int worm_total;

		private int egg_water_exist = -1;

		public struct EggInfo
		{
			public bool is_liquid
			{
				get
				{
					return PrEggManager.is_liquid(this.categ);
				}
			}

			public EggInfo(byte _count, byte _grade, PrEggManager.CATEG _categ)
			{
				this.count = _count;
				this.grade = _grade;
				this.categ = _categ;
			}

			public string getEggLocalizedTitle()
			{
				return EpManager.getEggLocalizedTitle(this.categ);
			}

			public byte count;

			public byte grade;

			public PrEggManager.CATEG categ;
		}

		public sealed class PrEggItem
		{
			public PrEggItem(PrEggManager _Con, PrEggManager.CATEG _categ)
			{
				this.Con = _Con;
				this.categ = _categ;
				this.ADrw = new WaterShakeDrawer[2];
				for (int i = 0; i < 2; i++)
				{
					WaterShakeDrawer waterShakeDrawer = (this.ADrw[i] = new WaterShakeDrawer(2));
					waterShakeDrawer.min_spd = 0.5f;
					waterShakeDrawer.max_spd = 1.3f;
					waterShakeDrawer.min_w = 110f;
					waterShakeDrawer.max_w = 160f;
					waterShakeDrawer.min_draw_height = 0f;
					waterShakeDrawer.resolution = 2f;
				}
				this.fnRunDropEggEffect = (M2DropObject Dro, float fcnt) => this.runDropEggEffectInner(Dro, fcnt);
				this.fnRunDropWormEffect = (M2DropObject Dro, float fcnt) => this.runDropWormEffectInner(Dro, fcnt);
				this.ADrw[1].grd_level_t = 1f;
				this.t_drip = global::XX.X.NI(30, 90, global::XX.X.XORSP());
				this.eggitem_count = (this.eggitem_grade = 0);
			}

			public void getCountAndGrade(out byte count, out byte grade, bool no_random = false)
			{
				if (!no_random)
				{
					count = (byte)global::XX.X.Mx(1, 2 + global::XX.X.xors(3) + global::XX.X.IntR(global::XX.X.ZSIN((float)this.val_absorbed_clip, this.Con.Pr.get_maxmp()) * 4f));
				}
				else
				{
					count = (byte)global::XX.X.Mx(1, 3 + global::XX.X.IntR(global::XX.X.ZSIN((float)this.val_absorbed_clip, this.Con.Pr.get_maxmp()) * 4f));
				}
				grade = (byte)(global::XX.X.xors(global::XX.X.Mx(1, global::XX.X.IntC((float)this.Con.total / this.Pr.get_maxmp() / 0.3f))) + this.Pr.NM2D.IMNG.getNoelJuiceQuality(1.8f));
				grade = (byte)global::XX.X.MMX(0, (int)grade + this.Pr.EpCon.getNoelEggQualityAdd(), 4);
			}

			public void initLayingEgg(bool decline_egg_item = false)
			{
				this.add_cushion = -this.val;
				this.val_absorbed = global::XX.X.Abs(this.add_cushion);
				this.t_effect_egg = -1000f;
				this.getCountAndGrade(out this.eggitem_count, out this.eggitem_grade, false);
				this.effect_egg_count = global::XX.X.IntC(this.val_absorbed / this.Con.Pr.get_maxmp() / 0.125f) + (4 + global::XX.X.xors(3) + (int)this.eggitem_count);
				this.effect_egg_count_layed = 0;
				if (decline_egg_item)
				{
					this.eggitem_count = 0;
				}
			}

			public void publishDrip(int count, bool with_speed = false)
			{
				this.Mp.DropCon.setLoveJuice(this.Pr, count, 15988208U, (float)(with_speed ? 1 : 0), !with_speed);
			}

			public float DrawTo(MeshDrawer Md, int meshi, float x, float y, float total_width, float h, float maxmp, float drawn_width, ref float hold_mp_value)
			{
				int num = 0;
				int num2 = 1;
				float num3 = global::XX.X.MMX(1f, (float)global::XX.X.IntR(total_width * this.val / maxmp), total_width - drawn_width);
				float num4 = global::XX.X.MMX(0f, (float)((int)(num3 + ((this.add_cushion < 0f) ? (total_width * this.t_cushion / maxmp) : 0f))), total_width - drawn_width);
				if (num4 <= 0f)
				{
					return 0f;
				}
				int vertexMax = Md.getVertexMax();
				float num5 = 0f;
				if (hold_mp_value > 0f)
				{
					float num6 = hold_mp_value * this.mp_hold_absorb_ratio;
					if (num6 < this.val)
					{
						num5 += num6;
						hold_mp_value = 0f;
					}
					else
					{
						num5 += this.val;
						hold_mp_value = global::XX.X.Mx(hold_mp_value - this.val, 0f);
					}
				}
				Color32 color = MTRX.ColWhite;
				Color32 color2 = MTRX.ColWhite;
				Color32 color3 = MTRX.ColWhite;
				Color32 color4 = MTRX.ColWhite;
				Color32 color5 = MTRX.ColWhite;
				switch (this.categ)
				{
				case PrEggManager.CATEG.WORM:
					color3 = C32.d2c(2866030007U);
					color4 = C32.d2c(3435846325U);
					color5 = C32.d2c(577536347U);
					color = C32.d2c(3432886674U);
					color2 = C32.d2c(3428280662U);
					break;
				case PrEggManager.CATEG.SLIME:
					color3 = C32.d2c(2860154759U);
					color4 = C32.d2c(3436635893U);
					color5 = C32.d2c(1440147189U);
					color = C32.d2c(3437223903U);
					color2 = C32.d2c(3435246538U);
					break;
				case PrEggManager.CATEG.MUSH:
					color3 = C32.d2c(2856023513U);
					color4 = C32.d2c(3431984353U);
					color5 = C32.d2c(1439104509U);
					color = C32.d2c(3432864938U);
					color2 = C32.d2c(3435628190U);
					break;
				case PrEggManager.CATEG.GOLEM_OD:
					color3 = C32.d2c(2860154759U);
					color4 = C32.d2c(3429918291U);
					color5 = C32.d2c(1427181322U);
					color = C32.d2c(3435051955U);
					color2 = C32.d2c(3428933466U);
					break;
				case PrEggManager.CATEG.FOX:
					color3 = C32.d2c(2859794432U);
					color4 = C32.d2c(3439306015U);
					color5 = C32.d2c(1442830943U);
					color = C32.d2c(3439276337U);
					color2 = C32.d2c(3433693184U);
					break;
				}
				if (this.add_cushion > 0f)
				{
					float num7 = global::XX.X.ZPOW(this.t_cushion - 40f, 30f);
					color3 = MTRX.cola.Set(4288256409U).blend(color3, num7).C;
					color4 = MTRX.cola.White().blend(color4, num7).C;
					color5 = MTRX.cola.White().blend(color5, num7).C;
					color = MTRX.cola.Black().blend(color, num7).C;
					color2 = MTRX.cola.Black().blend(color2, num7).C;
				}
				float num8 = ((this.add_cushion > 0f) ? global::XX.X.ZSIN(this.t_cushion, 70f) : 1f);
				float num9 = num8 * (h - 3f);
				Md.chooseSubMesh(num, false, false);
				if (num8 == 1f)
				{
					Md.ColGrd.Set(color4);
					Md.Col = color5;
					Md.RectBLGradation(x, y, num4, num9, GRD.BOTTOM2TOP, false);
				}
				else
				{
					for (int i = 0; i < 2; i++)
					{
						WaterShakeDrawer waterShakeDrawer = this.ADrw[i];
						waterShakeDrawer.min_h = 2f * num8;
						waterShakeDrawer.max_h = 3f * num8;
						waterShakeDrawer.max_draw_height = h;
						waterShakeDrawer.under_resolution = (float)((this.add_cushion < 0f || num5 > 0f) ? 4 : 0);
						if (i == 0)
						{
							Md.Col = color3;
						}
						else
						{
							Md.ColGrd.Set(color4);
							Md.Col = color5;
						}
						waterShakeDrawer.drawBL(Md, x, y, num4, num9 - (float)i);
					}
				}
				Md.chooseSubMesh(num2, false, false);
				Md.initForImg(MTRX.EffCircle128, 0);
				int num10 = global::XX.X.Mx(2, (int)this.val / 4);
				for (int j = 0; j < num10; j++)
				{
					int num11 = IN.totalframe - j * 7;
					if (num11 >= 0)
					{
						num11 %= 130;
						uint ran = global::XX.X.GETRAN2((int)(this.categ * (PrEggManager.CATEG)7 + num11 / 130 * 9 + j), (int)(14 + j % 5 + this.categ * PrEggManager.CATEG._ALL));
						float num12 = (float)num11 / 130f;
						float num13 = (1f + global::XX.X.RAN(ran, 2063) * 5f) * global::XX.X.ZLINE(num12, 0.125f) * (1f - global::XX.X.ZLINE(num12 - 0.875f, 0.125f));
						float num14 = global::XX.X.RAN(ran, 2250) * 5f + 2f;
						float num15 = y - num13 - num14 + ((num14 + num13) * 2f + h) * num12;
						float num16 = -3f + global::XX.X.RAN(ran, 1469) * (num3 + 6f);
						Md.Col = MTRX.cola.Set(color).blend(color2, global::XX.X.RAN(ran, 1351)).C;
						Md.Rect(x + num16, num15, num13 * 2f, num13 * 2f, false);
					}
				}
				if (this.add_cushion < 0f)
				{
					float num17 = total_width * (this.val + this.add_cushion) / maxmp - 1f;
					float num18 = 0.25f + 0.25f * global::XX.X.COSIT(77f);
					Color32[] colorArray = Md.getColorArray();
					Vector3[] vertexArray = Md.getVertexArray();
					num10 = Md.getVertexMax();
					for (int k = vertexMax; k < num10; k++)
					{
						if (vertexArray[k].x * 64f - x >= num17)
						{
							Color32 color6 = colorArray[k];
							color6.g = (byte)((float)color6.g * num18);
							color6.b = (byte)((float)color6.b * num18);
							colorArray[k] = color6;
						}
					}
				}
				else if (num5 > 0f)
				{
					float num19 = total_width * num5 / maxmp;
					float num20 = 0.125f + 0.125f * global::XX.X.COSIT(157f);
					Color32[] colorArray2 = Md.getColorArray();
					Vector3[] vertexArray2 = Md.getVertexArray();
					num10 = Md.getVertexMax();
					for (int l = vertexMax; l < num10; l++)
					{
						if (vertexArray2[l].x * 64f - x <= num19)
						{
							Color32 color7 = colorArray2[l];
							color7.r = (byte)((float)color7.r * num20);
							color7.g = (byte)((float)color7.g * num20);
							colorArray2[l] = color7;
						}
					}
				}
				return num4;
			}

			public bool run(float fcnt, bool can_progress_laying)
			{
				if (this.add_cushion < 0f)
				{
					if (!can_progress_laying)
					{
						return true;
					}
					if (this.t_cushion > 0f)
					{
						this.t_cushion = 0f;
					}
					float num = global::XX.X.Mn(-this.add_cushion / 200f, 0.35f);
					bool flag = false;
					if (this.Pr.Ser.has(SER.EGGED_2))
					{
						num /= 2f;
						this.Pr.Ser.Fine(SER.EGGED_2, 60);
						flag = true;
					}
					if (!this.Pr.is_alive || this.Pr.isGameoverRecover())
					{
						num /= 2f;
					}
					this.t_cushion -= fcnt * num;
					if (this.t_cushion < this.add_cushion)
					{
						this.t_cushion = 0f;
						this.val += this.add_cushion;
						this.add_cushion = 0f;
						this.effect_egg_count_layed = this.effect_egg_count;
						if (this.val <= 0f)
						{
							this.t_lay_egg_ser_delay = -1f;
							return false;
						}
					}
					if (this.t_effect_egg == -1000f)
					{
						this.t_effect_egg = (float)((int)global::XX.X.NIXP(25f, 35f));
						UILog.Instance.AddAlertTX("Alert_laying_egg_" + this.categ.ToString().ToLower(), UILogRow.TYPE.ALERT_EGG);
					}
					if (this.t_effect_egg >= 0f)
					{
						this.t_effect_egg -= fcnt;
						if (this.t_effect_egg <= 0f)
						{
							int num2 = this.effect_egg_count_layed;
							this.effect_egg_count_layed = num2 + 1;
							if (num2 < this.effect_egg_count)
							{
								this.t_effect_egg = (float)((int)global::XX.X.NIXP(25f, 50f));
								this.layEggEffect();
								if (this.eggitem_count > 0 && global::XX.X.Abs(this.val) < 40f)
								{
									this.effect_egg_count++;
									this.t_effect_egg /= 4f;
								}
							}
							else
							{
								this.t_effect_egg = -1f;
							}
						}
					}
					if (this.t_lay_egg_ser_delay < 0f)
					{
						this.t_lay_egg_ser_delay = 60f;
					}
					else
					{
						this.t_lay_egg_ser_delay -= fcnt;
						if (this.t_lay_egg_ser_delay <= 0f)
						{
							this.t_lay_egg_ser_delay = 60f;
							this.applySerDamage();
							if (!PrEggManager.is_liquid(this.categ))
							{
								this.Pr.EpCon.applyEpDamage(flag ? MDAT.EpAtkLayEgg2 : MDAT.EpAtkLayEgg, null, EPCATEG_BITS._ALL, 1f, true);
							}
						}
					}
					if (global::XX.X.XORSP() < 0.04f)
					{
						this.Pr.publishVaginaSplash(MTR.col_blood_egg, 3, 1.7f);
					}
					if (this.t_drip > 0f)
					{
						this.t_drip = 0f;
						this.Pr.Mp.playSnd("laying_egg_air");
						this.publishDrip((int)global::XX.X.NIXP(1f, 3f), true);
					}
					this.t_drip += fcnt;
					if (this.t_drip >= 0f)
					{
						this.t_drip = -((this.t_effect_egg >= 0f) ? global::XX.X.NI(1, 15, global::XX.X.XORSP()) : global::XX.X.NI(18, 115, global::XX.X.XORSP()));
						this.publishDrip((int)global::XX.X.NIXP(1f, 3f), true);
						if (global::XX.X.XORSP() < 0.4f)
						{
							this.Pr.Mp.playSnd("laying_egg");
							if (this.Pr.UP.isPoseLayingEgg())
							{
								UIPictureBase.FlgStopAutoFade.Add("EggCon");
								this.Pr.UP.applyLayingEgg(false);
							}
						}
					}
				}
				else
				{
					if (this.add_cushion > 0f)
					{
						this.t_cushion += fcnt;
						if (this.t_cushion >= 70f)
						{
							this.t_cushion = 0f;
							this.add_cushion = 0f;
						}
					}
					bool flag2 = this.Pr.isDamagingOrKo() && !this.Pr.isDownState();
					if (flag2)
					{
						if (this.t_drip > 0f)
						{
							this.t_drip = 0f;
							this.publishDrip((int)global::XX.X.NIXP(2f, 6f), true);
						}
						this.t_drip += fcnt;
						if (this.t_drip >= 0f)
						{
							this.t_drip = -global::XX.X.NI(3, 35, global::XX.X.Pow(global::XX.X.XORSP(), 2));
							this.publishDrip(1, true);
						}
					}
					else if (!this.Pr.isPoseDown(false))
					{
						if (this.t_drip < 0f)
						{
							this.t_drip = global::XX.X.NI(30, 90, global::XX.X.XORSP());
						}
						this.t_drip -= fcnt * (float)(flag2 ? 4 : 1);
						if (this.t_drip < 0f)
						{
							this.t_drip = global::XX.X.NI(30, 90, global::XX.X.XORSP());
							this.publishDrip(1, false);
						}
					}
				}
				return true;
			}

			public void checkDripUi(float fcnt)
			{
				if (this.t_drip_ui < 0f)
				{
					this.t_drip_ui = global::XX.X.NI(20, 60, global::XX.X.XORSP());
				}
				this.t_drip_ui -= fcnt * (float)(this.isLayingEgg() ? 6 : 1);
				if (this.t_drip_ui < 0f)
				{
					this.t_drip_ui = global::XX.X.NI(30, 70, global::XX.X.XORSP()) * global::XX.X.NIL(1f, 3f, (float)this.val_absorbed_clip + global::XX.X.Mx(0f, this.val - 80f), global::XX.X.Mn(140f, this.val));
					this.Pr.UP.applyDrip(false);
				}
			}

			public bool isLayingEgg()
			{
				return this.add_cushion < 0f;
			}

			public void clipLevel(float lvl)
			{
				if (this.val > 0f)
				{
					this.val = (float)global::XX.X.IntR(this.val * lvl);
					this.val_absorbed = (float)global::XX.X.IntR(this.val_absorbed * lvl);
				}
				if (this.add_cushion < 0f)
				{
					this.add_cushion *= this.val;
				}
			}

			private void layEggEffect()
			{
				Vector3 hipPos = this.Pr.getHipPos();
				bool flag = this.Pr.isPoseDown(false);
				bool flag2 = PrEggManager.is_liquid(this.categ);
				int num = global::XX.CAim._XD((int)hipPos.z, 1);
				float num2 = (flag ? global::XX.X.NIXP(0.013f, 0.03f) : 0f) * (float)num;
				float num3 = (flag ? (-global::XX.X.NIXP(0.003f, 0.06f)) : 0f) - (float)global::XX.CAim._YD((int)hipPos.z, 1) * 0.12f;
				if (!flag2)
				{
					bool flag3 = global::XX.X.XORSP() < 0.3f;
					float num4 = hipPos.x;
					if (num == 0)
					{
						num4 += (float)global::XX.X.MPFXP() * global::XX.X.NIXP(0.12f, 0.2f);
					}
					float num5 = 0.158f;
					PrEggManager.CATEG categ = this.categ;
					M2DropObject.FnDropObjectDraw fnDropObjectDraw;
					if (categ != PrEggManager.CATEG.SLIME)
					{
						if (categ != PrEggManager.CATEG.MUSH)
						{
							fnDropObjectDraw = M2DropObjectReader.GetFn("LayedEffectEgg");
						}
						else
						{
							fnDropObjectDraw = M2DropObjectReader.GetFn("LayedEffectEggSlime");
							flag3 = false;
							num2 *= 2f;
							num5 = global::XX.X.NIXP(0.07f, 0.1f);
						}
					}
					else
					{
						fnDropObjectDraw = M2DropObjectReader.GetFn("LayedEffectEggSlime");
					}
					M2DropObject m2DropObject = this.Mp.DropCon.Add(fnDropObjectDraw, hipPos.x, hipPos.y, num2, num3, (float)(flag3 ? 1 : 0), -1f);
					m2DropObject.FnRun = this.fnRunDropEggEffect;
					m2DropObject.bounce_x_reduce = 0f;
					m2DropObject.bounce_y_reduce = 0f;
					m2DropObject.gravity_scale = num5;
					m2DropObject.type = (DROP_TYPE)516;
					if (flag3)
					{
						this.layEggWormEffect(m2DropObject.x, m2DropObject.y, false);
					}
					this.Pr.EpCon.addEggLayCount(this.categ, 1);
				}
				else
				{
					float num6 = 1f;
					uint num7;
					uint num8;
					if (this.categ == PrEggManager.CATEG.FOX)
					{
						num6 = 0.75f;
						num7 = 16756323U;
						num8 = 15814181U;
					}
					else
					{
						num7 = 7827054U;
						num8 = 131328U;
					}
					int num9 = global::XX.X.IntR(global::XX.X.NIXP(9f, 16f));
					M2DropObject.FnDropObjectDraw fn = M2DropObjectReader.GetFn("splash_sperma");
					for (int i = num9 - 1; i >= 0; i--)
					{
						num7 = MTRX.cola.Set(num7).blend(num8, global::XX.X.XORSP()).rgba;
						M2DropObject m2DropObject2 = this.Mp.DropCon.Add(fn, hipPos.x + 0.04f * global::XX.X.XORSPSH(), hipPos.y + 0.08f * global::XX.X.XORSPSH(), num2 + global::XX.X.NIXP(-0.02f, 0.08f) * (float)num, num3 + global::XX.X.NIXP(-0.026f, 0.072f), num6, (float)num7);
						m2DropObject2.bounce_x_reduce = 0f;
						m2DropObject2.bounce_y_reduce = 0f;
						m2DropObject2.gravity_scale = 0.1f;
						m2DropObject2.type = (DROP_TYPE)516;
					}
				}
				if ((this.eggitem_count > 0) ? (global::XX.X.XORSP() < global::XX.X.NIL(global::XX.X.Abs(this.val), this.val_absorbed, 0.88f, 0.12f)) : (global::XX.X.XORSP() < 0.035f))
				{
					if (this.eggitem_count > 0)
					{
						this.eggitem_count -= 1;
						this.Con.dropEggItem(this.categ, 1, (int)this.eggitem_grade, num2, num3);
					}
					this.Pr.PtcST("laying_egg_flash", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
					this.Pr.UP.applyLayingEggCutin(flag2, false, false);
				}
				this.Pr.PtcVar("x", (double)hipPos.x).PtcVar("y", (double)hipPos.y).PtcVar("ra", (double)global::XX.CAim._XD((int)hipPos.z, 1))
					.PtcST("laying_egg", PtcHolder.PTC_HOLD.NORMAL, PTCThread.StFollow.NO_FOLLOW);
				this.publishDrip((int)global::XX.X.NIXP(1f, 6f), true);
			}

			public void progressAfterBattle()
			{
				if (this.add_cushion < 0f)
				{
					return;
				}
				this.val_clip += global::XX.X.IntC(this.Con.Pr.get_maxmp() * 0.3f);
				this.val_absorbed = global::XX.X.Mn(this.val * 0.875f, global::XX.X.Mn((float)this.val_clip * 0.8f, this.val_absorbed));
			}

			private void applySerDamage()
			{
				FlagCounter<SER> flagCounter;
				if (this.categ == PrEggManager.CATEG.FOX)
				{
					flagCounter = MDAT.SerWhenLayEggFox;
				}
				else
				{
					flagCounter = MDAT.SerWhenLayEgg;
				}
				this.Pr.Ser.applySerDamage(flagCounter, this.Pr.getSerApplyRatio(), -1);
			}

			private void layEggWormEffect(float x, float y, bool from_ground_egg)
			{
				bool flag = global::XX.X.XORSP() >= 0.5f;
				float num = (float)(flag ? (-1) : 1);
				PrEggManager.CATEG categ = this.categ;
				M2DropObject.FnDropObjectDraw fnDropObjectDraw;
				if (categ != PrEggManager.CATEG.SLIME)
				{
					if (categ != PrEggManager.CATEG.MUSH)
					{
						fnDropObjectDraw = M2DropObjectReader.GetFn("LayedEffectWorm");
					}
					else
					{
						fnDropObjectDraw = M2DropObjectReader.GetFn("LayedEffectMush");
					}
				}
				else
				{
					fnDropObjectDraw = M2DropObjectReader.GetFn("LayedEffectSlime");
				}
				M2DropObject m2DropObject = this.Mp.DropCon.Add(fnDropObjectDraw, x, y, from_ground_egg ? (num * global::XX.X.NIXP(0.008f, 0.011f)) : 0.0001f, from_ground_egg ? global::XX.X.NIXP(-0.012f, -0.028f) : 0f, (float)(flag ? (-1) : 0), -1f);
				m2DropObject.FnRun = this.fnRunDropWormEffect;
				m2DropObject.bounce_x_reduce = 0.7f;
				m2DropObject.bounce_y_reduce = 0f;
				m2DropObject.gravity_scale = 0.175f;
				m2DropObject.type = DROP_TYPE.NO_OPTION;
			}

			private bool runDropEggEffectInner(M2DropObject Dro, float fcnt)
			{
				if (Dro.on_ground && Dro.af_ground > 4f)
				{
					if (Dro.z == 0f)
					{
						this.layEggWormEffect(Dro.x, Dro.y, true);
					}
					if (Dro.z < 2f)
					{
						this.Mp.PtcSTsetVar("x", (double)Dro.x).PtcSTsetVar("y", (double)Dro.y).PtcST("laying_egg_worm", null, PTCThread.StFollow.NO_FOLLOW);
						Dro.z = 2f;
					}
					if (Dro.af_ground > 950f)
					{
						return false;
					}
				}
				return true;
			}

			private bool runDropWormEffectInner(M2DropObject Dro, float fcnt)
			{
				uint ran = global::XX.X.GETRAN2(Dro.index, Dro.index % 7);
				float num = (float)((Dro.z != 0f) ? (-1) : 1);
				if (Dro.on_ground)
				{
					float num2 = 1f;
					if (this.categ == PrEggManager.CATEG.MUSH)
					{
						num2 = 0f;
					}
					Dro.vx = num * global::XX.X.NI(0.005f, 0.01f, global::XX.X.RAN(ran, 1571)) * num2;
					if (Dro.af_ground > 700f)
					{
						return false;
					}
				}
				return true;
			}

			public PR Pr
			{
				get
				{
					return this.Con.Pr;
				}
			}

			public Map2d Mp
			{
				get
				{
					return this.Con.Pr.Mp;
				}
			}

			public bool no_auto_getout
			{
				get
				{
					return PrEggManager.no_auto_getout(this.categ);
				}
			}

			public int val_absorbed_clip
			{
				get
				{
					if (this.add_cushion < 0f || this.Con.isEgged2Active())
					{
						return (int)this.val_absorbed;
					}
					return (int)global::XX.X.Mn(this.val_absorbed, (float)this.val_clip);
				}
			}

			public int val_absorbed_view
			{
				get
				{
					if (this.add_cushion < 0f || this.Con.isEgged2Active())
					{
						return (int)this.val_absorbed;
					}
					return (int)global::XX.X.Mn(this.val_absorbed, global::XX.X.Mn((float)this.val_clip + this.Con.Pr.get_maxmp() * 0.3f, this.val));
				}
			}

			public void readBinaryFrom(ByteArray Ba, bool read_clip)
			{
				this.add_cushion = 0f;
				this.t_cushion = 0f;
				this.val_absorbed = Ba.readFloat();
				this.mp_hold_absorb_ratio = Ba.readFloat();
				if (read_clip)
				{
					this.val_clip = Ba.readInt();
					return;
				}
				this.val_clip = (int)this.val;
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				Ba.writeFloat(this.val_absorbed);
				Ba.writeFloat(this.mp_hold_absorb_ratio);
				Ba.writeInt(this.val_clip);
			}

			public readonly PrEggManager Con;

			public float mp_hold_absorb_ratio = 0.55f;

			public float val;

			public float val_absorbed;

			public int val_clip;

			public float add_cushion;

			public float t_cushion;

			public const int T_FADE_ADD = 70;

			public float t_drip;

			public float t_drip_ui;

			public float t_effect_egg;

			public int effect_egg_count;

			public int effect_egg_count_layed;

			public float t_lay_egg_ser_delay = -1f;

			public byte eggitem_count;

			private byte eggitem_grade;

			public const float laying_speed_default = 0.35f;

			public PrEggManager.CATEG categ;

			private WaterShakeDrawer[] ADrw;

			private M2DropObject.FnDropObjectRun fnRunDropEggEffect;

			private M2DropObject.FnDropObjectRun fnRunDropWormEffect;
		}

		public enum CATEG
		{
			WORM,
			SLIME,
			MUSH,
			GOLEM_OD,
			FOX,
			_ALL
		}
	}
}
