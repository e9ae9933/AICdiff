using System;
using Better;
using Spine.Unity;

namespace XX
{
	public class MTISpine : MTI
	{
		public MTISpine(string key, string _atlas_key, string load_key = "_")
			: base(key, null)
		{
			this.atlas_key = _atlas_key + ".atlas";
			this.default_json_key = _atlas_key;
			this.OSpDataAsset = new BDic<string, SkeletonDataAsset>(1);
			if (load_key != null)
			{
				base.addLoadKey(load_key, false);
			}
		}

		public bool getCachedAssets(ref string json_key, out SpineAtlasAsset _SpAtlasAsset, out SkeletonDataAsset _SpDataAsset)
		{
			if (this.SpAtlasAsset == null)
			{
				_SpAtlasAsset = null;
				_SpDataAsset = null;
				return false;
			}
			_SpAtlasAsset = this.SpAtlasAsset;
			_SpDataAsset = X.Get<string, SkeletonDataAsset>(this.OSpDataAsset, json_key);
			return true;
		}

		public void saveCache(string json_key, SpineAtlasAsset _SpAtlasAsset, SkeletonDataAsset _SpDataAsset)
		{
			this.SpAtlasAsset = _SpAtlasAsset;
			this.OSpDataAsset[json_key] = _SpDataAsset;
		}

		public bool isWrong(SpineAtlasAsset _SpAtlasAsset)
		{
			return _SpAtlasAsset != this.SpAtlasAsset;
		}

		public override void Dispose()
		{
			base.Dispose();
			this.SpAtlasAsset = null;
			this.OSpDataAsset.Clear();
		}

		public readonly string atlas_key;

		public readonly string default_json_key;

		private SpineAtlasAsset SpAtlasAsset;

		private readonly BDic<string, SkeletonDataAsset> OSpDataAsset;
	}
}
