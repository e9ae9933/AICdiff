using System;
using System.IO;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public class MTIOneImage : MTI
	{
		public MTIOneImage(string key, string load_key = "_", string _image_key = null)
			: base(key, null)
		{
			this.image_key = _image_key ?? X.basename(key);
			if (load_key != null)
			{
				base.addLoadKey(load_key, false);
			}
		}

		public override bool isAsyncLoadFinished()
		{
			return base.isAsyncLoadFinished() && this.LImage_ != null;
		}

		public void ReplaceExternalPngForPxl(PxlCharacter Pc, bool _do_not_destruct = false)
		{
			if (this.Bset == null)
			{
				return;
			}
			Texture[] array = new Texture[this.Bset.GetAllAssetNames().Length];
			array[0] = this.Image;
			if (array.Length == 2)
			{
				MImage pxlMtiParts = this.getPxlMtiParts();
				array[1] = ((pxlMtiParts != null) ? pxlMtiParts.Tx : this.Image);
			}
			Pc.ReplaceExternalPng(array, _do_not_destruct);
		}

		public MImage getPxlMtiParts()
		{
			string text = this.image_key;
			if (TX.isEnd(text, "_0"))
			{
				text = TX.slice(text, 0, text.Length - 2) + "_1";
			}
			return base.LoadImage(text);
		}

		protected override void initializeResource(AssetBundle _Bset = null)
		{
			base.initializeResource(_Bset);
			if (this.AsyncCreate != null)
			{
				this.QImageLoad = _Bset.LoadAssetAsync<Texture>(this.image_key);
				this.QImageLoad.completed += delegate(AsyncOperation AOp)
				{
					if (this.QImageLoad == AOp)
					{
						this.QImageLoad = null;
						if (this.LImage_ == null)
						{
							this.LImage_ = base.LoadImage(Path.GetFileName(this.image_key));
						}
					}
				};
				return;
			}
			this.QImageLoad = null;
			MImage mimage = base.LoadImage(Path.GetFileName(this.image_key));
			this.LImage_ = mimage;
		}

		public MImage MI
		{
			get
			{
				return this.LImage_;
			}
		}

		public Texture Image
		{
			get
			{
				if (this.LImage_ == null)
				{
					return null;
				}
				return this.LImage_.Tx;
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			this.LImage_ = null;
			this.QImageLoad = null;
		}

		private MImage LImage_;

		public readonly string image_key;

		private AssetBundleRequest QImageLoad;
	}
}
