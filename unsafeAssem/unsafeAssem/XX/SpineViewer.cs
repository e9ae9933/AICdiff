using System;
using System.Collections.Generic;
using Better;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace XX
{
	public class SpineViewer : ITeColor
	{
		private event AnimationState.TrackEntryDelegate EvComplete;

		private event AnimationState.TrackEntryEventDelegate EvEvent;

		public SpineViewer(string _key)
		{
			this.key_ = _key;
		}

		public string key
		{
			get
			{
				return this.key_;
			}
		}

		public SpineViewer reloadAnimKey(string _key)
		{
			if (this.key_ == _key)
			{
				return this;
			}
			this.key_ = _key;
			if (this.Tex != null && this.atlas_key == null)
			{
				this.Tex = null;
			}
			this.SpAtlasAsset = null;
			return this;
		}

		public void attachTextMTI(MTISpine _Mti)
		{
			this.releaseTextMTI();
			this.TextMti = _Mti;
		}

		public void releaseTextMTI()
		{
			if (this.TextMti != null)
			{
				this.TextMti.remLoadKey("_spineviewer_temporary");
			}
			this.TextMti = null;
		}

		public void attachImageMTI(MTI _Mti)
		{
			this.releaseImageMTI();
			this.ImageMti = _Mti;
		}

		public void attachImageMTITemporary(string mti_key)
		{
			this.releaseImageMTI();
			this.ImageMti = new MTI(mti_key, "_spineviewer_temporary");
		}

		public void releaseImageMTI()
		{
			if (this.ImageMti != null)
			{
				this.ImageMti.remLoadKey("_spineviewer_temporary");
			}
			this.ImageMti = null;
		}

		public void destruct()
		{
			this.releaseTextMTI();
			this.releaseImageMTI();
		}

		public virtual GameObject initGameObject(GameObject Gob)
		{
			this.charaAnim = IN.GetOrAdd<SkeletonAnimation>(Gob);
			this.Mrd = IN.GetOrAdd<MeshRenderer>(Gob);
			this.Mpb = new MProperty(this.Mrd, -1);
			MeshDrawer.initMrd(this.Mrd);
			IN.GetOrAdd<MeshFilter>(Gob);
			this.charaAnim.enabled = false;
			return Gob;
		}

		public virtual Texture prepareTexture()
		{
			if (this.Tex == null)
			{
				this.Tex = ((this.ImageMti != null) ? this.ImageMti.Load<Texture2D>(this.atlas_key ?? this.key_) : Resources.Load<Texture2D>("SpineAnim/" + (this.atlas_key ?? this.key_)));
			}
			return this.Tex;
		}

		public Texture prepareTexturePreLoaded(Texture PreLoadedTexture)
		{
			this.Tex = PreLoadedTexture;
			return this.Tex;
		}

		public void attachPreloadAssets(SpineAtlasAsset _SpAtlasAsset, SkeletonDataAsset _SpDataAsset, Texture _Tx, Material _Mtr)
		{
			this.SpAtlasAsset = _SpAtlasAsset;
			this.SpDataAsset = _SpDataAsset;
			this.prepareTexturePreLoaded(_Tx);
			this.Mtr = _Mtr;
			this.fineSpAtlasFirstMtr();
			this.fineAtlasMaterial();
		}

		private void fineSpAtlasFirstMtr()
		{
			if (this.SpAtlasAsset.materials == null || this.SpAtlasAsset.materials.Length == 0)
			{
				this.SpAtlasAsset.materials = new Material[1];
			}
			this.SpAtlasAsset.materials[0] = this.Mtr;
		}

		protected virtual void prepareAtlasAssets(ref SpineAtlasAsset SpAtlasAsset, ref SkeletonDataAsset SpDataAsset)
		{
			if (this.TextMti != null)
			{
				SpineViewer.prepareAtlasAssetsS(this.TextMti, out SpAtlasAsset, out SpDataAsset, this.key_);
			}
			else
			{
				SpineViewer.prepareAtlasAssetsS(this.atlas_key ?? this.key_, this.key_, null, out SpAtlasAsset, out SpDataAsset);
			}
			this.fineSpAtlasFirstMtr();
		}

		public static void prepareAtlasAssetsS(MTISpine Mti, out SpineAtlasAsset SpAtlasAsset, out SkeletonDataAsset SpDataAsset, string json_key = null)
		{
			SpineViewer.prepareAtlasAssetsS(Mti.atlas_key, json_key ?? Mti.default_json_key, Mti, out SpAtlasAsset, out SpDataAsset);
		}

		private static void prepareAtlasAssetsS(string atlas_key, string json_key, MTISpine Mti, out SpineAtlasAsset SpAtlasAsset, out SkeletonDataAsset SpDataAsset)
		{
			SpAtlasAsset = null;
			SpDataAsset = null;
			if (Mti != null)
			{
				Mti.getCachedAssets(ref json_key, out SpAtlasAsset, out SpDataAsset);
			}
			bool flag = SpAtlasAsset == null;
			bool flag2 = SpDataAsset == null;
			if (!flag && !flag2)
			{
				return;
			}
			if (flag)
			{
				SpAtlasAsset = ScriptableObject.CreateInstance<SpineAtlasAsset>();
				TextAsset textAsset;
				if (Mti != null)
				{
					textAsset = Mti.Load<TextAsset>(Mti.atlas_key);
				}
				else
				{
					atlas_key += ".atlas";
					textAsset = Resources.Load<TextAsset>("SpineAnim/" + atlas_key);
				}
				if (textAsset == null)
				{
					X.de("テキストアトラスが不明: " + atlas_key, null);
				}
				SpAtlasAsset.Clear();
				SpAtlasAsset.atlasFile = textAsset;
			}
			if (flag2)
			{
				TextAsset textAsset2;
				if (Mti != null)
				{
					textAsset2 = Mti.Load<TextAsset>(json_key);
				}
				else
				{
					textAsset2 = Resources.Load<TextAsset>("SpineAnim/" + json_key);
				}
				if (textAsset2 == null)
				{
					X.de("テキスト .json が不明: " + json_key, null);
				}
				SpDataAsset = ScriptableObject.CreateInstance<SkeletonDataAsset>();
				SpDataAsset.Clear();
				SkeletonDataAsset skeletonDataAsset = SpDataAsset;
				AtlasAssetBase[] array = new SpineAtlasAsset[] { SpAtlasAsset };
				skeletonDataAsset.atlasAssets = array;
				SpDataAsset.skeletonJSON = textAsset2;
				SpDataAsset.scale = 0.015625f;
				SpDataAsset.fromAnimation = new string[0];
				SpDataAsset.toAnimation = new string[0];
				SpDataAsset.duration = new float[0];
				SpDataAsset.defaultMix = 0.2f;
			}
			if (Mti != null)
			{
				Mti.saveCache(json_key, SpAtlasAsset, SpDataAsset);
			}
		}

		public Material prepareMaterial(Material _Mtr = null)
		{
			this.Mtr = ((_Mtr == null) ? (this.Mtr ?? MTRX.MtrSpineDefault) : _Mtr);
			if (TX.noe(this.key))
			{
				return this.Mtr;
			}
			if (this.TextMti != null && this.TextMti.isWrong(this.SpAtlasAsset))
			{
				this.SpAtlasAsset = null;
			}
			if (this.SpAtlasAsset == null)
			{
				this.prepareAtlasAssets(ref this.SpAtlasAsset, ref this.SpDataAsset);
				this.fineAtlasMaterial();
				try
				{
					this.AtlasContainer = this.SpAtlasAsset.GetAtlas(false);
					goto IL_00B4;
				}
				catch
				{
					throw new Exception("Spine アニメーションが不明: Skeleton: " + this.key_);
				}
			}
			this.fineSpAtlasFirstMtr();
			this.fineAtlasMaterial();
			IL_00B4:
			return this.Mtr;
		}

		public virtual void fineAtlasMaterial()
		{
			Texture texture = this.prepareTexture();
			try
			{
				this.Mtr.mainTexture = texture;
			}
			catch
			{
				X.de("エラー:破棄された Material にアクセスしています" + this.key + " / " + this.atlas_key, null);
			}
			Material[] materials = this.SpAtlasAsset.materials;
			for (int i = materials.Length - 1; i >= 0; i--)
			{
				materials[i].mainTexture = texture;
			}
			this.Mrd.sharedMaterials = materials;
		}

		public void copyAnimationFrom(SpineViewer Spw, bool timescale_zero = false)
		{
			this.SpDataAsset = Spw.SpDataAsset;
			this.SpAtlasAsset = Spw.SpAtlasAsset;
			this.atlas_key = Spw.atlas_key;
			this.key_ = Spw.key;
			this.Tex = Spw.Tex;
			this.TextMti = Spw.TextMti;
			this.id_max = Spw.id_max;
			for (int i = 0; i <= this.id_max; i++)
			{
				TrackEntry current = Spw.charaState.GetCurrent(i);
				if (current != null && current.Animation != null)
				{
					if (i == 0 || this.charaState == null)
					{
						this.initializeAnimState(null);
						this.enabled_ = true;
						List<Skin> skinList = Spw.charaAnim.Skeleton.SkinList;
						if (skinList != null)
						{
							int count = skinList.Count;
							for (int j = 0; j < count; j++)
							{
								if (j == 0)
								{
									this.charaAnim.Skeleton.SetSkin(skinList[j], true);
								}
								else
								{
									this.charaAnim.Skeleton.MergeSkin(skinList[j]);
								}
							}
						}
					}
					TrackEntry trackEntry = this.charaState.SetAnimation(i, current.Animation.Name, false);
					trackEntry.TrackTime = current.TrackTime;
					trackEntry.TimeScale = (timescale_zero ? 0f : current.TimeScale);
				}
			}
			this.charaAnim.Skeleton.UpdateCache();
		}

		public virtual void clearAnim(string anim_name, int loopTo_frame = -1000, string skin_name = null)
		{
			this.prepareMaterial(this.Mtr);
			if (skin_name == null)
			{
				SkeletonData skeletonData = this.SpDataAsset.GetSkeletonData(true);
				if (skeletonData != null)
				{
					ExposedList<Skin> skins = skeletonData.Skins;
					if (skins != null && skins.Count > 0)
					{
						skin_name = skins.Items[0].Name;
					}
				}
			}
			this.setAnmLoopFrame(anim_name, loopTo_frame);
			this.initializeAnimState(skin_name);
			this.charaState.SetAnimation(0, anim_name, false);
			this.id_max = 0;
			this.timedelta_cache = 0f;
			this.fineAtlasMaterial();
			this.charaAnim.enabled = false;
			this.enabled_ = true;
		}

		protected virtual void initializeAnimState(string skin_name)
		{
			this.charaAnim.ClearState();
			this.charaAnim.UpdateMode = UpdateMode.FullUpdate;
			this.charaAnim.loop = false;
			this.charaAnim.timeScale = this.timeScale_;
			bool flag = true;
			bool flag2 = false;
			if (this.charaAnim.skeletonDataAsset != this.SpDataAsset)
			{
				this.charaAnim.skeletonDataAsset = this.SpDataAsset;
				SpineViewer.ReservedSklData reservedSklData;
				if (this.ORsvData != null && this.ORsvData.TryGetValue(this.SpDataAsset, out reservedSklData))
				{
					this.charaAnim.assignReservedSkeleton(reservedSklData.SkeletonData, skin_name, true);
					this.charaAnim.assignReservedState(reservedSklData.StateData, null, true);
					flag = false;
					flag2 = true;
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				this.charaAnim.initialSkinName = skin_name;
				this.charaState = null;
			}
			this.charaAnim.Initialize(flag, false);
			if (!flag && skin_name != null)
			{
				this.charaAnim.skeleton.SetSkin(skin_name);
			}
			if (this.charaState == null)
			{
				this.charaState = this.charaAnim.state;
				this.charaState.Complete += this.fnAnimComplete;
				this.charaState.Event += this.fnAnimEvent;
				if (this.ORsvData != null)
				{
					this.ORsvData[this.SpDataAsset] = new SpineViewer.ReservedSklData(this.charaAnim.skeleton, this.charaState);
				}
			}
			else if (flag2)
			{
				this.ORsvData[this.SpDataAsset] = new SpineViewer.ReservedSklData(this.charaAnim.skeleton, this.charaState);
				this.charaAnim.ClearState();
				this.charaAnim.UpdateMode = UpdateMode.FullUpdate;
				this.charaAnim.loop = false;
			}
			this.charaAnim.timeScale = this.timeScale_;
		}

		public virtual void mergeSkins(string[] Askin, int start_i = 1)
		{
			if (Askin == null)
			{
				return;
			}
			Skeleton skeleton = this.charaAnim.Skeleton;
			int num = Askin.Length;
			for (int i = start_i; i < num; i++)
			{
				skeleton.MergeSkin(Askin[i]);
			}
			skeleton.UpdateCache();
		}

		public virtual void updateAnim(bool draw_flag = true, float fcnt = 1f)
		{
			if (this.enabled_ || !this.charaAnim.wasUpdatedAfterInit)
			{
				this.timedelta_cache += (this.consider_playing_timescale ? (IN.deltaFrame * 0.016666668f) : 0.008333334f) * fcnt;
				if (draw_flag)
				{
					float num = X.Mn(this.timedelta_cache, 0.050000004f * fcnt);
					this.timedelta_cache = 0f;
					this.charaAnim.Update(num);
				}
				else
				{
					if (this.charaAnim.wasUpdatedAfterInit)
					{
						return;
					}
					this.charaAnim.Update(0f);
				}
				this.charaAnim.LateUpdate();
			}
		}

		public TrackEntry addAnim(int id, string anim_name, int loopf = -1000, float mixtime = 0f, float alpha = 1f)
		{
			TrackEntry trackEntry = this.charaState.SetAnimation(id, anim_name, false);
			if (loopf != -1000)
			{
				this.setAnmLoopFrame(anim_name, loopf);
			}
			trackEntry.Loop = trackEntry.AnimationEnd == 0f;
			this.id_max = X.Mx(id, this.id_max);
			trackEntry.Alpha = alpha;
			trackEntry.MixTime = mixtime;
			return trackEntry;
		}

		public TrackEntry setAlpha(int id, float alpha)
		{
			TrackEntry current = this.charaState.GetCurrent(id);
			if (current != null)
			{
				current.Alpha = alpha;
			}
			return current;
		}

		public TrackEntry setTimePosition(int id, float t)
		{
			TrackEntry current = this.charaState.GetCurrent(id);
			if (current != null && current.AnimationEnd > 0f)
			{
				current.TrackTime = t;
			}
			return current;
		}

		public SpineViewer progressTimePositionAll(float t_second)
		{
			if (this.charaState == null)
			{
				return this;
			}
			for (int i = 0; i <= this.id_max; i++)
			{
				TrackEntry current = this.charaState.GetCurrent(i);
				if (current != null && current.AnimationEnd > 0f)
				{
					current.TrackTime += t_second;
				}
			}
			return this;
		}

		public SpineViewer setTimePositionAll(float t_second)
		{
			for (int i = 0; i <= this.id_max; i++)
			{
				TrackEntry current = this.charaState.GetCurrent(i);
				if (current != null && current.AnimationEnd > 0f)
				{
					current.TrackTime = t_second;
				}
			}
			return this;
		}

		public SpineViewer setTimePositionAll(SpineViewer Src)
		{
			if (this.charaState == null || Src.charaState == null)
			{
				return this;
			}
			for (int i = 0; i <= this.id_max; i++)
			{
				TrackEntry current = this.charaState.GetCurrent(i);
				TrackEntry current2 = Src.charaState.GetCurrent(i);
				if (current != null && current.AnimationEnd > 0f)
				{
					current.TrackTime = current2.TrackTime;
				}
			}
			return this;
		}

		public void setAlpha(float alpha)
		{
			for (int i = 0; i <= this.id_max; i++)
			{
				TrackEntry current = this.charaState.GetCurrent(i);
				if (current != null)
				{
					current.Alpha = alpha;
				}
			}
		}

		public void setTimePosition(float t)
		{
			for (int i = 0; i <= this.id_max; i++)
			{
				TrackEntry current = this.charaState.GetCurrent(i);
				if (current != null && current.AnimationEnd > 0f)
				{
					current.TrackTime = t;
				}
			}
		}

		public int getTrackLength()
		{
			return this.id_max + 1;
		}

		private void fnAnimComplete(TrackEntry trackEntry)
		{
			int anmLoopFrame = this.getAnmLoopFrame(trackEntry.Animation.Name);
			if (anmLoopFrame >= 0)
			{
				if (this.whole_loop_set && trackEntry.TrackIndex == 0)
				{
					for (int i = 1; i <= this.id_max; i++)
					{
						TrackEntry current = this.charaState.GetCurrent(i);
						if (current != null)
						{
							current.TrackTime = X.Mx((float)this.getAnmLoopFrame(current.Animation.Name) * 0.033333335f, 0f);
						}
					}
				}
				trackEntry.TrackTime = X.Mx((float)anmLoopFrame * 0.033333335f, 0f);
			}
			if ((trackEntry.TrackIndex != 0 || this.call_complete_evnet_when_track0) && this.EvComplete != null)
			{
				this.EvComplete(trackEntry);
			}
		}

		private void fnAnimEvent(TrackEntry trackEntry, global::Spine.Event e)
		{
			if (this.EvEvent != null)
			{
				this.EvEvent(trackEntry, e);
			}
		}

		public void addListenerComplete(AnimationState.TrackEntryDelegate Fn)
		{
			this.EvComplete += Fn;
		}

		public void addListenerEvent(AnimationState.TrackEntryEventDelegate Fn)
		{
			this.EvEvent += Fn;
		}

		public void remListenerComplete(AnimationState.TrackEntryDelegate Fn)
		{
			this.EvComplete -= Fn;
		}

		public void remListenerEvent(AnimationState.TrackEntryEventDelegate Fn)
		{
			this.EvEvent -= Fn;
		}

		public SkeletonAnimation getAnimationBehaviour()
		{
			return this.charaAnim;
		}

		public float timeScale
		{
			get
			{
				return this.timeScale_;
			}
			set
			{
				this.timeScale_ = value;
				if (this.charaAnim != null && this.charaAnim.timeScale != 0f)
				{
					this.charaAnim.timeScale = this.timeScale_;
				}
			}
		}

		public float stencil_ref
		{
			get
			{
				if (!this.Mtr.HasProperty("_StencilRef"))
				{
					return -1f;
				}
				return this.Mtr.GetFloat("_StencilRef");
			}
			set
			{
				this.Mtr.SetFloat("_StencilRef", value);
			}
		}

		public bool isActive()
		{
			return this.charaAnim != null && this.enabled;
		}

		public bool enabled
		{
			get
			{
				return this.enabled_;
			}
			set
			{
				if (value)
				{
					this.activate();
					return;
				}
				this.deactivate();
			}
		}

		public void activate()
		{
			if (this.enabled_ || this.charaAnim == null)
			{
				return;
			}
			this.enabled_ = true;
		}

		public void deactivate()
		{
			if (!this.enabled_)
			{
				return;
			}
			this.enabled_ = false;
		}

		public Color32 getColorTe()
		{
			Color32 color;
			if (this.Mpb != null && this.Mpb.TryGetValue("_Color", out color))
			{
				return color;
			}
			return MTRX.ColWhite;
		}

		public void setColorTe(C32 Col, C32 CMul, C32 CAdd)
		{
			if (this.Mpb != null)
			{
				this.Mpb.SetColor("_Color", CMul.C, false);
				this.Mpb.SetColor("_AddColor", CAdd.C, false);
				this.Mpb.SetFloat("_UseAddColor", 1f, false);
				this.Mpb.fineMpb();
			}
		}

		public void setColor(Color32 Col)
		{
			if (this.Mpb != null)
			{
				this.Mpb.SetColor("_Color", Col, false);
				this.Mpb.fineMpb();
			}
		}

		public bool isPause()
		{
			return this.charaAnim == null || this.charaAnim.timeScale == 0f;
		}

		public void setPause(bool f)
		{
			if (f)
			{
				this.charaAnim.timeScale = 0f;
				return;
			}
			this.charaAnim.timeScale = this.timeScale_;
		}

		public GameObject gameObject
		{
			get
			{
				return this.charaAnim.gameObject;
			}
		}

		public void setAnmLoopFrameSecond(string anim_name, float second)
		{
			if (second != -1000f)
			{
				this.setAnmLoopFrame(anim_name, (int)(second * 30f));
			}
		}

		public void setAnmLoopFrame(string anim_name, int f)
		{
			if (f != -1000)
			{
				BDic<string, int> bdic;
				if (!SpineViewer.OOanm2loop.TryGetValue(this.key_, out bdic))
				{
					bdic = (SpineViewer.OOanm2loop[this.key_] = new BDic<string, int>(1));
				}
				bdic[anim_name] = f;
			}
		}

		public int getAnmLoopFrame(string anim_name)
		{
			BDic<string, int> bdic;
			int num;
			if (SpineViewer.OOanm2loop.TryGetValue(this.key_, out bdic) && bdic.TryGetValue(anim_name, out num))
			{
				return num;
			}
			return -1;
		}

		public string getBaseAnimName()
		{
			if (this.charaState == null)
			{
				return null;
			}
			TrackEntry current = this.charaState.GetCurrent(0);
			if (current == null || current.Animation == null)
			{
				return null;
			}
			return current.Animation.Name;
		}

		public bool hasAnim(string anm)
		{
			if (this.charaState == null)
			{
				return false;
			}
			int count = this.charaAnim.state.Tracks.Count;
			for (int i = 0; i < count; i++)
			{
				TrackEntry track = this.getTrack(i);
				if (track != null && track.Animation != null && track.Animation.Name == anm)
				{
					return true;
				}
			}
			return false;
		}

		public Texture getTexture()
		{
			return this.prepareTexture();
		}

		public Material getMaterial()
		{
			return this.Mtr;
		}

		public TrackEntry getTrack(int i)
		{
			return this.charaAnim.state.GetCurrent(i);
		}

		public float getAnmPlayTime(int i)
		{
			TrackEntry trackEntry = ((i >= this.charaAnim.state.Tracks.Count) ? null : this.getTrack(i));
			if (trackEntry == null)
			{
				return -1f;
			}
			return trackEntry.TrackTime;
		}

		public Bone FindBone(string name)
		{
			if (name == null)
			{
				return null;
			}
			return this.charaAnim.skeleton.FindBone(name);
		}

		public SkeletonAnimation GetSkeletonAnimation()
		{
			return this.charaAnim;
		}

		public bool force_enable_mesh_render
		{
			get
			{
				return this.charaAnim != null && this.charaAnim.force_enable_mesh_render;
			}
			set
			{
				if (this.charaAnim == null)
				{
					return;
				}
				this.charaAnim.force_enable_mesh_render = value;
			}
		}

		public bool update_sharedmaterials_array
		{
			get
			{
				return this.charaAnim != null && this.charaAnim.update_sharedmaterials_array;
			}
			set
			{
				if (this.charaAnim == null)
				{
					return;
				}
				this.charaAnim.update_sharedmaterials_array = value;
			}
		}

		public bool use_reserved_data
		{
			get
			{
				return this.ORsvData != null;
			}
			set
			{
				if (value == this.use_reserved_data)
				{
					return;
				}
				if (value)
				{
					this.ORsvData = new BDic<SkeletonDataAsset, SpineViewer.ReservedSklData>(16);
					return;
				}
				this.ORsvData = null;
			}
		}

		public MeshRenderer GetRenderer()
		{
			return this.Mrd;
		}

		public Skeleton GetSkeleton()
		{
			return this.charaAnim.skeleton;
		}

		public SkeletonData GetSkeletonData()
		{
			return this.SpDataAsset.GetSkeletonData(true);
		}

		public bool existSkin(string name)
		{
			Skin[] items = this.SpDataAsset.GetSkeletonData(true).Skins.Items;
			int num = items.Length;
			for (int i = 0; i < num; i++)
			{
				if (items[i].Name == name)
				{
					return true;
				}
			}
			return false;
		}

		public bool existAnim(string name)
		{
			return this.SpDataAsset.GetSkeletonData(true).FindAnimation(name) != null;
		}

		public bool existBone(string name)
		{
			return this.SpDataAsset.GetSkeletonData(true).FindBone(name) != null;
		}

		public const string spine_dir = "SpineAnim/";

		private string key_;

		public string atlas_key;

		private MTISpine TextMti;

		private MTI ImageMti;

		private MProperty Mpb;

		protected Texture Tex;

		protected SpineAtlasAsset SpAtlasAsset;

		protected SkeletonDataAsset SpDataAsset;

		protected Atlas AtlasContainer;

		protected SkeletonAnimation charaAnim;

		protected AnimationState charaState;

		protected MeshRenderer Mrd;

		protected Material Mtr;

		private bool enabled_;

		public bool call_complete_evnet_when_track0;

		private int id_max;

		public const int fps = 30;

		public const float fpsr = 0.033333335f;

		private float timeScale_ = 1f;

		public float timedelta_cache;

		public bool whole_loop_set;

		public bool consider_playing_timescale = true;

		private const float frame_anim_TS = 0.008333334f;

		private BDic<SkeletonDataAsset, SpineViewer.ReservedSklData> ORsvData;

		private static BDic<string, BDic<string, int>> OOanm2loop = new BDic<string, BDic<string, int>>();

		private class ReservedSklData
		{
			public ReservedSklData(Skeleton _SkeletonData, AnimationState _StateData)
			{
				this.SkeletonData = _SkeletonData;
				this.StateData = _StateData;
			}

			public Skeleton SkeletonData;

			public AnimationState StateData;
		}
	}
}
