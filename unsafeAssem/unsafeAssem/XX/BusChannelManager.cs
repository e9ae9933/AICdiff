using System;

namespace XX
{
	public class BusChannelManager
	{
		public BusChannelManager()
		{
			this.Alvl = new float[3];
			this.Alvl_pre = new float[3];
			this.Alvl[2] = (this.Alvl_pre[2] = 1f);
		}

		public void clear(bool set_fine_flag = false)
		{
			if (!set_fine_flag)
			{
				this.need_check_flag = false;
			}
			if (this.normal_flag > 0)
			{
				if (this.normal_flag >= 2)
				{
					return;
				}
				this.normal_flag += 1;
			}
			for (int i = 0; i < 2; i++)
			{
				if (set_fine_flag)
				{
					if (!this.fine_flag && this.Alvl[i] != 0f)
					{
						this.fine_flag = true;
					}
				}
				else if ((this.Alvl_pre[i] = this.Alvl[i]) != 0f)
				{
					this.need_check_flag = true;
				}
				this.Alvl[i] = 0f;
			}
			this.Alvl_pre[2] = this.Alvl[2];
			this.Alvl[2] = 1f;
		}

		public BusChannelManager Set(BusChannelManager.CHN chn, float lvl)
		{
			if (this.Alvl[(int)chn] == lvl)
			{
				return this;
			}
			this.Alvl[(int)chn] = lvl;
			if (lvl > 0f)
			{
				this.need_check_flag = true;
			}
			return this;
		}

		public BusChannelManager Check()
		{
			if (!this.need_check_flag)
			{
				return this;
			}
			this.need_check_flag = false;
			float num = 0f;
			for (int i = 0; i < 2; i++)
			{
				num += this.Alvl[i];
			}
			if (num == 0f)
			{
				this.Alvl[2] = 1f;
				if (this.normal_flag == 0)
				{
					this.normal_flag = 1;
				}
			}
			else if (num <= 1f)
			{
				this.Alvl[2] = 1f - num;
				this.normal_flag = 0;
			}
			else
			{
				this.Alvl[2] = 0f;
				num = 1f / num;
				for (int j = 0; j < 2; j++)
				{
					this.Alvl[j] *= num;
				}
				this.normal_flag = 0;
			}
			if (!this.fine_flag)
			{
				for (int k = 0; k <= 2; k++)
				{
					if (this.Alvl[k] != this.Alvl_pre[k])
					{
						this.fine_flag = true;
						break;
					}
				}
			}
			return this;
		}

		public void updateSound(SndPlayer S)
		{
			if (S == null)
			{
				return;
			}
			for (int i = 0; i <= 2; i++)
			{
				string text = ((i == 2) ? "MasterOut" : FEnum<BusChannelManager.CHN>.ToStr((BusChannelManager.CHN)i));
				S.SetBusSendLevelAndOffset(text, 0f, this.Alvl[i]);
			}
		}

		private const string def_channel = "MasterOut";

		private const int maxi = 2;

		private float[] Alvl;

		private float[] Alvl_pre;

		private byte normal_flag;

		public bool fine_flag;

		private bool need_check_flag;

		public enum CHN
		{
			BUS_EFFECT,
			BUS_WATER,
			_MAX
		}
	}
}
