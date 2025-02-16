using System;
using Spine;
using Spine.Unity;
using UnityEngine;
using XX;

namespace nel.fatal
{
	internal sealed class SpvLoader
	{
		public SpvLoader(string dir, string _key, string _atlas_key)
		{
			if (dir == null)
			{
				dir = "Fatal/";
			}
			this.key = _key;
			this.mti_loadkey = "_FT_" + _key;
			string text = _atlas_key ?? _key;
			this.MtiImage = MTI.LoadContainerOneImage(dir + text, this.mti_loadkey, text);
			this.MtiText = MTI.LoadContainerSpine(dir, text, this.mti_loadkey);
			SpineViewer.prepareAtlasAssetsS(this.MtiText, out this.SpAtlasAsset, out this.SpDataAsset, this.key);
		}

		public void initTexture(string key)
		{
			if (this.Tx == null)
			{
				this.Tx = this.MtiImage.Image;
			}
		}

		public void destruct()
		{
			this.MtiImage.remLoadKey(this.mti_loadkey);
			this.MtiText.remLoadKey(this.mti_loadkey);
			if (this.Tx != null)
			{
				this.Tx = null;
			}
		}

		public AtlasRegion GetImage(string ikey)
		{
			this.SpDataAsset.GetSkeletonData(false);
			return this.SpAtlasAsset.GetAtlas(false).FindRegion(ikey);
		}

		private const string spine_dir = "Fatal/";

		public string key;

		public SpineAtlasAsset SpAtlasAsset;

		public SkeletonDataAsset SpDataAsset;

		public Texture Tx;

		private MTISpine MtiText;

		public readonly MTIOneImage MtiImage;

		public float width;

		public float height;

		public readonly string mti_loadkey;
	}
}
