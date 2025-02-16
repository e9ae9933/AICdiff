using System;
using System.Collections.Generic;
using Better;
using m2d;
using XX;

namespace nel
{
	public class FDSummonerInfo
	{
		public FDSummonerInfo(string _summoner_key, SupplyManager.SmnSupLink _SupLink)
		{
			this.summoner_key = _summoner_key;
			this.SupLink = _SupLink;
			this.AMp = new List<Map2d>(1);
			this.sf_key_suffix = ".." + this.summoner_key;
		}

		public FDSummonerInfo check(NelM2DBase M2D, string sf_key)
		{
			if (!this.noticed)
			{
				this.noticed = true;
				this.Summoner = EnemySummoner.Get(this.summoner_key, false);
			}
			int num = sf_key.IndexOf(this.sf_key_suffix);
			if (this.Summoner != null && num >= 0)
			{
				Map2d map2d = M2D.Get(TX.slice(sf_key, 0, num), false);
				if (map2d != null)
				{
					X.pushIdentical<Map2d>(this.AMp, map2d);
					NightController.SummonerData lpInfo = M2D.NightCon.GetLpInfo(sf_key, null, false);
					if (this.SInfo == null)
					{
						this.SInfo = lpInfo;
						this.Summoner.prepared = true;
					}
					if (this.OSmnData == null)
					{
						this.OSmnData = new BDic<string, NightController.SummonerData>(1);
					}
					if (!this.OSmnData.ContainsKey(sf_key))
					{
						this.OSmnData[sf_key] = lpInfo;
						this.defeat_count += lpInfo.defeat_count;
					}
				}
			}
			return this;
		}

		public Map2d SInfoMp
		{
			get
			{
				if (this.AMp.Count <= 0)
				{
					return null;
				}
				return this.AMp[0];
			}
		}

		public WmDeperture GetDeperture(NelM2DBase M2D)
		{
			Map2d sinfoMp = this.SInfoMp;
			if (this.SInfoMp != null)
			{
				WholeMapItem wholeFor = M2D.WM.GetWholeFor(sinfoMp, false);
				if (wholeFor != null)
				{
					return new WmDeperture(wholeFor.text_key, sinfoMp.key);
				}
			}
			return default(WmDeperture);
		}

		public bool valid
		{
			get
			{
				return this.SInfo != null && this.SInfoMp != null;
			}
		}

		public bool fd_favorite
		{
			get
			{
				return this.SInfo != null && this.SInfo.fd_favorite;
			}
			set
			{
				if (this.SInfo != null)
				{
					this.SInfo.fd_favorite = value;
				}
			}
		}

		public readonly string summoner_key;

		public readonly string sf_key_suffix;

		public readonly SupplyManager.SmnSupLink SupLink;

		public int defeat_count;

		public bool noticed;

		public readonly List<Map2d> AMp;

		public NightController.SummonerData SInfo;

		private BDic<string, NightController.SummonerData> OSmnData;

		public EnemySummoner Summoner;

		public aBtnFDRow B;
	}
}
