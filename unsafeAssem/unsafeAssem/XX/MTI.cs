using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Better;
using UnityEngine;

namespace XX
{
	public class MTI : IDisposable
	{
		public static void initMTI()
		{
			MTI.OCon = new BDic<string, MTI>();
		}

		public static MTI LoadContainer(string asset_key, string load_key = "_")
		{
			MTI mti = X.Get<string, MTI>(MTI.OCon, asset_key);
			if (mti == null)
			{
				mti = (MTI.OCon[asset_key] = new MTI(asset_key, null));
			}
			mti.addLoadKey(load_key, false);
			return mti;
		}

		public static MTISpine LoadContainerSpine(string directory_or_asset_key, string atlas_key, string load_key = "_")
		{
			string text = (TX.isEnd(directory_or_asset_key, ".atlas") ? directory_or_asset_key : (directory_or_asset_key + atlas_key + ".atlas"));
			MTISpine mtispine = X.Get<string, MTI>(MTI.OCon, text) as MTISpine;
			if (mtispine == null)
			{
				mtispine = (MTI.OCon[text] = new MTISpine(text, atlas_key, null));
			}
			mtispine.addLoadKey(load_key, false);
			return mtispine;
		}

		public static MTIOneImage LoadContainerOneImage(string asset_key, string load_key = "_", string image_key = null)
		{
			MTIOneImage mtioneImage = X.Get<string, MTI>(MTI.OCon, asset_key) as MTIOneImage;
			if (mtioneImage == null)
			{
				mtioneImage = (MTI.OCon[asset_key] = new MTIOneImage(asset_key, null, image_key));
			}
			mtioneImage.addLoadKey(load_key, false);
			return mtioneImage;
		}

		public static MTI ReleaseContainer(string asset_key, string load_key = "_")
		{
			MTI mti = X.Get<string, MTI>(MTI.OCon, asset_key);
			if (mti != null)
			{
				mti.remLoadKey(load_key);
			}
			return null;
		}

		public static MTIOneImage GetOneImageFor(MImage MI)
		{
			if (MI == null)
			{
				return null;
			}
			foreach (KeyValuePair<string, MTI> keyValuePair in MTI.OCon)
			{
				if (keyValuePair.Value is MTIOneImage)
				{
					MTIOneImage mtioneImage = keyValuePair.Value as MTIOneImage;
					if (mtioneImage.MI == MI)
					{
						return mtioneImage;
					}
				}
			}
			return null;
		}

		public MTI(string key, string load_key = "_")
		{
			this.Aload_keys = new List<string>(4);
			this.OImg = new BDic<string, MImage>(2);
			string text = "";
			string text2;
			if (REG.match(key, MTI.RegDirectory))
			{
				text = REG.leftContext + "/";
				text2 = REG.R1.ToLower();
			}
			else
			{
				text2 = key.ToLower();
			}
			this.file_valid = true;
			this.asset_path = (TX.isStart(text, "/", 0) ? (TX.slice(text, 1) + text2 + ".dat") : Path.Combine(Application.streamingAssetsPath, text + text2 + ".dat"));
			this.resources_path = "Assets/Editor/AssetBundlesSrc/" + key + "/";
			if (load_key != null)
			{
				this.addLoadKey(load_key, false);
			}
		}

		public static bool existsFile(string key)
		{
			return File.Exists(Path.Combine(Application.streamingAssetsPath, key + ".dat"));
		}

		public bool hasLoadKey(string load_key)
		{
			return load_key != null && this.Aload_keys.IndexOf(load_key) >= 0;
		}

		public bool addLoadKey(string load_key, bool async = false)
		{
			if (load_key == null)
			{
				return false;
			}
			if (this.Aload_keys.IndexOf(load_key) == -1)
			{
				this.Aload_keys.Add(load_key);
				if (this.Aload_keys.Count == 1)
				{
					if (this.AsyncCreateAborted != null)
					{
						AssetBundleCreateRequest asyncCreateAborted = this.AsyncCreateAborted;
						this.AsyncCreate = asyncCreateAborted;
						this.AsyncCreateAborted = null;
						return true;
					}
					if (async)
					{
						this.initializeLoadASync();
					}
					else
					{
						this.initializeResource(null);
					}
					return true;
				}
			}
			return false;
		}

		public virtual bool isAsyncLoadFinished()
		{
			return this.Bset != null;
		}

		protected virtual void initializeLoadASync()
		{
			this.AsyncCreate = AssetBundle.LoadFromFileAsync(this.asset_path);
			if (this.FD_AsyncCompleted == null)
			{
				this.FD_AsyncCompleted = new Action<AsyncOperation>(this.AsyncCompleted);
			}
			this.AsyncCreate.completed += this.FD_AsyncCompleted;
		}

		protected void AsyncCompleted(AsyncOperation AOp)
		{
			AssetBundleCreateRequest assetBundleCreateRequest = AOp as AssetBundleCreateRequest;
			if (this.AsyncCreate != assetBundleCreateRequest)
			{
				assetBundleCreateRequest.assetBundle.Unload(true);
			}
			else
			{
				this.initializeResource(assetBundleCreateRequest.assetBundle);
				this.AsyncCreate = null;
			}
			this.AsyncCreateAborted = null;
		}

		protected virtual void initializeResource(AssetBundle _Bset = null)
		{
			this.Bset = _Bset ?? AssetBundle.LoadFromFile(this.asset_path);
		}

		public bool remLoadKey(string load_key)
		{
			int num = this.Aload_keys.IndexOf(load_key);
			if (num >= 0)
			{
				this.Aload_keys.RemoveAt(num);
				if (this.Aload_keys.Count == 0)
				{
					if (this.Bset != null || this.AsyncCreate != null)
					{
						this.Dispose();
					}
					return false;
				}
			}
			return true;
		}

		public bool isActive()
		{
			return this.Aload_keys.Count > 0;
		}

		public virtual void Dispose()
		{
			if (this.Bset != null)
			{
				this.Bset.Unload(true);
				this.Bset = null;
			}
			this.AsyncCreateAborted = this.AsyncCreate;
			this.AsyncCreate = null;
			foreach (KeyValuePair<string, MImage> keyValuePair in this.OImg)
			{
				keyValuePair.Value.Dispose();
			}
			this.Aload_keys.Clear();
			this.OImg.Clear();
			if (this.Oshader != null)
			{
				this.Oshader.Clear();
			}
		}

		public void UnloadAll()
		{
			if (this.Bset != null)
			{
				this.Bset.Unload(true);
			}
		}

		public T Load<T>(string path) where T : global::UnityEngine.Object
		{
			if (this.Aload_keys.Count == 0)
			{
				return default(T);
			}
			if (path.IndexOf("/") >= 0)
			{
				path = this.resources_path + path;
			}
			return this.Bset.LoadAsset<T>(path);
		}

		public string LoadText(string path, string debug_resource_ext = null, bool one_clip_resource = true)
		{
			if (this.Aload_keys.Count == 0)
			{
				return null;
			}
			TextAsset textAsset = this.Load<TextAsset>(path);
			if (!(textAsset != null))
			{
				return null;
			}
			return textAsset.text;
		}

		public byte[] LoadBytes(string path)
		{
			if (this.Aload_keys.Count == 0)
			{
				return null;
			}
			TextAsset textAsset = this.Load<TextAsset>(path);
			if (!(textAsset != null))
			{
				return null;
			}
			return textAsset.bytes;
		}

		public MImage LoadImage(string path)
		{
			if (this.Aload_keys.Count == 0)
			{
				return null;
			}
			MImage mimage = X.Get<string, MImage>(this.OImg, path);
			if (mimage == null)
			{
				mimage = (this.OImg[path] = new MImage(this.Load<Texture>(path)));
			}
			return mimage;
		}

		public Shader LoadShader(string name)
		{
			if (this.Oshader == null || this.Oshader.Count == 0)
			{
				this.LoadAllShader();
			}
			Shader shader;
			if (this.Oshader.TryGetValue(name, out shader))
			{
				return shader;
			}
			throw new Exception("No Shader " + name + " in " + this.resources_path);
		}

		public void LoadAllShader()
		{
			string[] allAssetNames = this.Bset.GetAllAssetNames();
			this.Oshader = this.Oshader ?? new BDic<string, Shader>(allAssetNames.Length);
			for (int i = allAssetNames.Length - 1; i >= 0; i--)
			{
				string text = allAssetNames[i];
				if (TX.isEnd(text, ".shader"))
				{
					Shader shader = this.Bset.LoadAsset<Shader>(text);
					this.Oshader[shader.name] = shader;
				}
			}
		}

		public void listUpAllFiles(string prefix, string suffix, List<string> Aout)
		{
			string[] allAssetNames = this.Bset.GetAllAssetNames();
			if (prefix == null && suffix == null)
			{
				Aout.AddRange(allAssetNames);
				return;
			}
			int num = allAssetNames.Length;
			for (int i = 0; i < num; i++)
			{
				string text = allAssetNames[i];
				if ((prefix == null || TX.isStart(text, prefix, 0)) && (suffix == null || TX.isEnd(text, suffix)))
				{
					Aout.Add(text);
				}
			}
		}

		private static BDic<string, MTI> OCon;

		private BDic<string, MImage> OImg;

		private List<string> Aload_keys;

		private BDic<string, Shader> Oshader;

		protected AssetBundle Bset;

		protected AssetBundleCreateRequest AsyncCreate;

		private AssetBundleCreateRequest AsyncCreateAborted;

		private string asset_path;

		private string resources_path;

		private static readonly Regex RegDirectory = new Regex("\\/([^\\/]*)$");

		private Action<AsyncOperation> FD_AsyncCompleted;

		public readonly bool file_valid;
	}
}
