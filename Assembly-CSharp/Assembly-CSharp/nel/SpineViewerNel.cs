using System;
using Spine.Unity;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class SpineViewerNel : SpineViewer
	{
		public SpineViewerNel(UIPictureBodySpine _Con, string _key, string chara_key)
			: base(_key)
		{
			this.Con = _Con;
			this.BetoMng = BetobetoManager.GetManager(chara_key);
			this.CurSvt = this.BetoMng.Assign(_key);
		}

		public override GameObject initGameObject(GameObject Gob)
		{
			GameObject gameObject = base.initGameObject(Gob);
			base.update_sharedmaterials_array = false;
			return gameObject;
		}

		public void setMaterial(Material Mtr)
		{
			this.Mtr = Mtr;
		}

		public void initializeLoad(Material Mtr)
		{
			this.CurSvt.initialize_load = true;
			base.prepareMaterial(Mtr);
		}

		public BetobetoManager.SvTexture getSvTexture()
		{
			return this.CurSvt;
		}

		public bool isPreparedResource()
		{
			return this.CurSvt.isAsyncLoadFinished();
		}

		public bool is_initialize_svtexture
		{
			get
			{
				return this.CurSvt.initialize_load;
			}
		}

		protected override void prepareAtlasAssets(ref SpineAtlasAsset SpAtlasAsset, ref SkeletonDataAsset SpDataAsset)
		{
			if (this.AMaterial == null)
			{
				this.AMaterial = new Material[] { this.Mtr };
			}
			else
			{
				this.AMaterial[0] = this.Mtr;
			}
			this.CurSvt.prepareAtlasAssets(out SpAtlasAsset, out SpDataAsset, this.AMaterial, this.replace_json_key);
		}

		public override Texture prepareTexture()
		{
			this.CurSvt.preapreAtlasDepth();
			this.Tex = this.CurSvt.prepareTexture(false);
			return this.Tex;
		}

		public override void clearAnim(string anim_name, int loopTo_frame = -1000, string skin_name = null)
		{
			base.clearAnim(anim_name, loopTo_frame, skin_name);
			this.BetoMng.Current = this.CurSvt;
			this.checkNeedUpdateTexture(false);
		}

		public bool checkNeedUpdateTexture(bool fine_material = true)
		{
			if (fine_material)
			{
				if (this.prepareTexture() == null)
				{
					return false;
				}
				base.prepareMaterial(null);
			}
			SpineViewerNel.CurDrawUpdating = this;
			return SpineViewerNel.updateTexture();
		}

		public override void fineAtlasMaterial()
		{
			Texture texture = this.prepareTexture();
			try
			{
				this.Mtr.mainTexture = texture;
			}
			catch
			{
				X.de("エラー:破棄された Material にアクセスしています" + base.key + " / " + this.atlas_key, null);
			}
			this.Mrd.sharedMaterial = this.Mtr;
		}

		public void fineTextureReloadImmediately(ref bool fine_material)
		{
			bool texture_prepared = this.CurSvt.texture_prepared;
			this.CurSvt.prepareTexture(true);
			if (!texture_prepared && this.CurSvt.texture_prepared)
			{
				fine_material = true;
			}
			this.BetoTextureUpdated(fine_material, false);
		}

		public void BetoTextureUpdated(bool mateial_reupdated, bool _fine_material = true)
		{
			if (this.CurSvt.texture_prepared && !SpineViewerNel.lock_betotexture_updated && (mateial_reupdated || this.CurSvt.dirt_index == 0 || this.CurSvt.dirt_index == 1))
			{
				SpineViewerNel.lock_betotexture_updated = true;
				try
				{
					if (_fine_material)
					{
						this.fineAtlasMaterial();
					}
					if (this.Con != null && this.cur_dirt > 0 && this.Con.PCon != null)
					{
						int frozen_stone_index = this.BetoMng.frozen_stone_index;
						if (frozen_stone_index >= 0 && this.CurSvt.dirt_index >= 0)
						{
							while (frozen_stone_index > this.CurSvt.dirt_index && !this.CurSvt.runBetobeto(this.cur_dirt, this.BetoMng))
							{
							}
						}
					}
				}
				catch
				{
				}
				SpineViewerNel.lock_betotexture_updated = false;
			}
		}

		public int cur_dirt
		{
			get
			{
				return this.BetoMng.get_current_dirt();
			}
		}

		public void closeEmot()
		{
			if (SpineViewerNel.CurDrawUpdating == this)
			{
				SpineViewerNel.CurDrawUpdating = null;
			}
		}

		public BetobetoManager getBetobetoManager()
		{
			return this.BetoMng;
		}

		public static void setNeedUpdate(SpineViewerNel _V)
		{
			SpineViewerNel.CurDrawUpdating = _V;
		}

		public static bool need_update_texture
		{
			get
			{
				return SpineViewerNel.CurDrawUpdating != null;
			}
		}

		public void releaseWithSvTextureSpine(BetobetoManager.SvTexture Svt)
		{
			if (Svt == this.CurSvt)
			{
				this.SpAtlasAsset = null;
				this.SpDataAsset = null;
			}
		}

		public static bool updateTexture()
		{
			if (SpineViewerNel.CurDrawUpdating == null)
			{
				return true;
			}
			bool texture_prepared = SpineViewerNel.CurDrawUpdating.CurSvt.texture_prepared;
			bool flag = SpineViewerNel.CurDrawUpdating.cur_dirt <= 0;
			bool flag2 = SpineViewerNel.CurDrawUpdating.CurSvt.runBetobeto(SpineViewerNel.CurDrawUpdating.cur_dirt, SpineViewerNel.CurDrawUpdating.BetoMng);
			SpineViewerNel.CurDrawUpdating.BetoTextureUpdated(flag || (!texture_prepared && SpineViewerNel.CurDrawUpdating.CurSvt.texture_prepared), true);
			if (flag2)
			{
				SpineViewerNel.CurDrawUpdating = null;
				return true;
			}
			return false;
		}

		public bool isSame(BetobetoManager.SvTexture _SvT)
		{
			return this.CurSvt == _SvT;
		}

		private UIPictureBodySpine Con;

		private BetobetoManager.SvTexture CurSvt;

		private BetobetoManager BetoMng;

		private Material[] AMaterial;

		public string replace_json_key;

		private static SpineViewerNel CurDrawUpdating;

		private static bool lock_betotexture_updated;
	}
}
