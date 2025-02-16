using System;
using m2d;
using PixelLiner.PixelLinerLib;
using XX;

namespace nel
{
	public class DirtManager
	{
		public DirtManager(PR _Pr)
		{
			this.Pr = _Pr;
		}

		public void applyDirt(int id)
		{
			this.dirt_check = -150;
			this.Adirt_occur[id] = 450 / ((id == 0) ? 2 : 1);
			this.dirt_occurence = 0;
			for (int i = 0; i < 4; i++)
			{
				if (this.Adirt_occur[i] > 0)
				{
					this.dirt_occurence += 1;
				}
			}
		}

		public bool clearDirt(bool change_default = false)
		{
			bool flag = this.dirt_occurence > 0;
			this.dirt_occurence = 0;
			this.dirt_check = 0;
			this.chk_thresh = 0f;
			for (int i = 0; i < 4; i++)
			{
				this.Adirt_occur[i] = 0;
			}
			if (flag && change_default)
			{
				this.Pr.recheck_emot = true;
			}
			return flag;
		}

		public void addDirtCheckCount(ushort time = 1)
		{
			int num = 3;
			for (int i = 1; i < 4; i++)
			{
				if (this.Adirt_occur[i] > 0)
				{
					this.Adirt_occur[i] = X.Mn(this.Adirt_occur[i] + 11, 450);
					num--;
				}
			}
			if (this.dirt_occurence < 4 && this.dirt_check >= 0)
			{
				this.dirt_check += (short)(time * 11);
			}
			if (this.dirt_check >= 110)
			{
				if (num == 0)
				{
					this.applyDirt(0);
					return;
				}
				int num2 = X.xors(num);
				for (int j = 1; j < 4; j++)
				{
					if (this.Adirt_occur[j] <= 0 && num2-- == 0)
					{
						this.applyDirt(j);
						return;
					}
				}
			}
		}

		public void run(float fcnt)
		{
			this.chk_thresh += fcnt;
			int num = ((this.Pr.enemy_targetted > 0) ? 32 : 4);
			if (this.chk_thresh < (float)num)
			{
				return;
			}
			this.chk_thresh = 0f;
			if (!this.Pr.isWormTrapped() && !this.Pr.isDamagingOrKo() && !this.Pr.isGaraakiState() && !this.Pr.isGameoverRecover())
			{
				if (this.dirt_check > 0)
				{
					this.dirt_check = (short)X.VALWALK((int)this.dirt_check, 0, 1);
				}
				if (this.dirt_occurence != 0)
				{
					for (int i = 0; i < 4; i++)
					{
						if ((i != 0 || !this.Pr.Ser.has(SER.EGGED)) && this.Adirt_occur[i] > 0)
						{
							int[] adirt_occur = this.Adirt_occur;
							int num2 = i;
							int num3 = adirt_occur[num2] - 1;
							adirt_occur[num2] = num3;
							if (num3 == 0)
							{
								this.dirt_occurence -= 1;
							}
						}
					}
				}
			}
			if (this.dirt_check < 0)
			{
				this.dirt_check = (short)X.VALWALK((int)this.dirt_check, 0, num / 4);
			}
		}

		public bool isActive()
		{
			return this.dirt_occurence > 0;
		}

		public bool isActive(int id)
		{
			return this.Adirt_occur[id] > 0;
		}

		public void readBinaryFrom(ByteReader Ba)
		{
			Ba.readByte();
			this.dirt_check = Ba.readShort();
			this.dirt_occurence = 0;
			for (int i = 0; i < 4; i++)
			{
				this.Adirt_occur[i] = (int)Ba.readShort();
				this.dirt_occurence += ((this.Adirt_occur[i] > 0) ? 1 : 0);
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(1);
			Ba.writeShort(this.dirt_check);
			for (int i = 0; i < 4; i++)
			{
				Ba.writeShort((short)this.Adirt_occur[i]);
			}
		}

		public readonly PR Pr;

		private short dirt_check;

		private byte dirt_occurence;

		private const int check_time_margin = 4;

		private const int check_time_margin_battle = 32;

		private const short DIRT_ONEHIT_TIME = 11;

		private const short DIRT_THRESH = 110;

		private const short DIRT_OCCUR_TIMEOUT = 450;

		private const short DIRT_OCCURED_WAIT = 150;

		public int[] Adirt_occur = new int[4];

		private float chk_thresh;
	}
}
