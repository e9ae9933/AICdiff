using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Better;
using evt;
using m2d;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using Spine;
using Spine.Unity;
using UnityEngine;
using XX;

namespace nel
{
	public sealed class BetobetoManager : IEventWaitListener
	{
		public BetobetoManager(string _key)
		{
			BetobetoManager.initBetobetoManager();
			this.key = _key;
			this.ClearS();
			this.Athread = new BetobetoManager.BetoThread[2];
			this.Xors = new XorsMaker(0U, true);
		}

		private Material ClearS()
		{
			if (this.OTex == null)
			{
				this.OTex = new BDic<string, BetobetoManager.SvTexture>(16);
				this.AInfo = new List<BetoInfo>();
				BetobetoManager.ASeBuf = new List<Skin.SkinEntry>();
			}
			this.OTex.Clear();
			return BetobetoManager.MtrBetoImg;
		}

		public void cleanAll(bool force_clean_texture = false)
		{
			if (this.OTex == null)
			{
				return;
			}
			bool flag = this.cleanBetobeto(force_clean_texture);
			BetobetoManager.immediate_load_material_ = 3;
			this.wetten = false;
			this.torned_count = 0f;
			this.frozen_lv_ = 0;
			this.Xors.init(false, 0U, 0U, 0U, 0U);
			this.total_fill_count = global::XX.X.xors(1024);
			if (flag && this.fnUpdated != null)
			{
				this.fnUpdated(this.Current);
			}
		}

		public BetobetoManager.SvTexture prepareTexture(string key, bool immediate = false)
		{
			if (this.OTex == null)
			{
				return null;
			}
			BetobetoManager.SvTexture svTexture = global::XX.X.Get<string, BetobetoManager.SvTexture>(this.OTex, key);
			if (svTexture == null)
			{
				global::XX.X.de("不明なSvTexture:" + key, null);
				return null;
			}
			if (!svTexture.texture_prepared && UIPicture.Instance != null)
			{
				SpineAtlasAsset spineAtlasAsset;
				SkeletonDataAsset skeletonDataAsset;
				svTexture.prepareAtlasAssets(out spineAtlasAsset, out skeletonDataAsset, new Material[] { UIPicture.Instance.MtrSpine }, null);
				svTexture.prepareTexture(immediate);
				UIPicture.Instance.fineCurrentBodyMaterial();
			}
			return svTexture;
		}

		public bool cleanBetobeto(bool force_clean_texture = false)
		{
			if (this.OTex == null)
			{
				return false;
			}
			bool flag = this.info_use > 0 || force_clean_texture;
			foreach (KeyValuePair<string, BetobetoManager.SvTexture> keyValuePair in this.OTex)
			{
				bool flag2 = keyValuePair.Value.cleanPrepare();
				if (force_clean_texture)
				{
					if (keyValuePair.Value.initialize_load)
					{
						if (flag2)
						{
							keyValuePair.Value.cleanExecute(false);
						}
					}
					else
					{
						keyValuePair.Value.releaseTexture();
					}
				}
			}
			this.info_use = 0;
			this.Athread = new BetobetoManager.BetoThread[2];
			this.thread_active = false;
			if (force_clean_texture)
			{
				NelM2DBase nelM2DBase = M2DBase.Instance as NelM2DBase;
				if (nelM2DBase != null)
				{
					nelM2DBase.need_uipic_remake = true;
				}
			}
			return flag;
		}

		public void releaseAtlasData()
		{
			foreach (KeyValuePair<string, BetobetoManager.SvTexture> keyValuePair in this.OTex)
			{
				keyValuePair.Value.releaseAtlasData();
			}
		}

		public bool redrawAllTexture()
		{
			this.checkLimit(true);
			foreach (KeyValuePair<string, BetobetoManager.SvTexture> keyValuePair in this.OTex)
			{
				keyValuePair.Value.cleanPrepare();
			}
			return this.info_use > 0;
		}

		public BetobetoManager.SvTexture Assign(string _key)
		{
			if (this.OTex == null)
			{
				this.ClearS();
			}
			BetobetoManager.SvTexture svTexture = global::XX.X.Get<string, BetobetoManager.SvTexture>(this.OTex, _key);
			if (svTexture == null)
			{
				svTexture = (this.OTex[_key] = new BetobetoManager.SvTexture(_key));
			}
			return svTexture;
		}

		public BetoInfo getInfo(int index)
		{
			return this.AInfo[index];
		}

		public bool addTorned(PR Pr, float _min, float _max)
		{
			if (CFG.sp_cloth_strength < 50)
			{
				_max += (float)(50 - CFG.sp_cloth_strength) / 100f;
			}
			if (this.torned_count >= 1f || _max <= 0f || global::XX.X.SENSITIVE || CFG.sp_cloth_strength >= 200)
			{
				return false;
			}
			float num = global::XX.X.NIXP(_min, _max) / (((float)CFG.sp_cloth_strength + 20f) / 120f);
			this.torned_count = global::XX.X.ZLINE(this.torned_count + num);
			if (this.torned_count >= 1f)
			{
				if (UIPicture.isPr(this))
				{
					UIBase.Instance.PtcVar("x", 0f).PtcVar("y", 50f).PtcST("ui_noel_break_cloth", null);
				}
				Pr.fineClothTorned();
				return true;
			}
			return false;
		}

		public void setTorned(PR Pr, bool f, bool fine_pr = true)
		{
			this.torned_count = (float)(f ? 1 : 0);
			if (fine_pr)
			{
				Pr.fineClothTorned();
			}
			if (UIPicture.isPr(this))
			{
				UIPicture.Instance.recheck_emot = true;
			}
		}

		public void setWetten(PR Pr, bool f, bool fine_pr = true)
		{
			if (this.wetten == f)
			{
				return;
			}
			this.wetten = f;
			if (fine_pr)
			{
				Pr.fineClothTorned();
			}
			if (UIPicture.isPr(this))
			{
				UIPicture.Instance.recheck_emot = true;
			}
		}

		public bool Check(PR Pr, BetoInfo B, bool force_add = false, bool call_change_func = true)
		{
			bool flag = this.info_use == 0;
			if (this.Check(B, force_add, call_change_func))
			{
				if (Pr != null && flag)
				{
					Pr.NM2D.IMNG.fineSpecialNoelRow(Pr);
				}
				return true;
			}
			return false;
		}

		public bool Check(PR Pr, NelAttackInfo Atk, bool force_add = false, bool call_change_func = true)
		{
			this.addTorned(Pr, Atk.torn_apply_min, Atk.torn_apply_max);
			bool flag = this.info_use == 0;
			if (this.Check(Atk.Beto, force_add, call_change_func))
			{
				if (Pr != null && flag)
				{
					Pr.NM2D.IMNG.fineSpecialNoelRow(Pr);
				}
				return true;
			}
			return false;
		}

		public bool Check(BetoInfo B, bool force_add = false, bool call_change_func = true)
		{
			if (B == null)
			{
				return false;
			}
			bool flag = false;
			if (B.thread >= 0 && !force_add)
			{
				if (this.Athread.Length <= B.thread)
				{
					Array.Resize<BetobetoManager.BetoThread>(ref this.Athread, B.thread + 1);
				}
				BetobetoManager.BetoThread betoThread = this.Athread[B.thread];
				if (betoThread.lockf == 0)
				{
					betoThread.stock += B.power;
					if (B.lockf > 0)
					{
						betoThread.lockf = B.lockf;
						this.thread_active = true;
					}
					this.thread_active = true;
					this.Athread[B.thread] = betoThread;
					flag = (betoThread.stock - 50f * global::XX.X.NI(1, 0, global::XX.X.ZLINE((float)(CFG.ui_effect_dirty - 7), 3f))) * 0.006666667f > global::XX.X.XORSP();
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				if (B.thread >= 0)
				{
					this.Athread[B.thread].stock = 0f;
				}
				while (this.info_use >= this.AInfo.Count)
				{
					this.AInfo.Add(new BetoInfo(null));
				}
				List<BetoInfo> ainfo = this.AInfo;
				int num = this.info_use;
				this.info_use = num + 1;
				B = ainfo[num].CopyFrom(B).FillId(ref this.total_fill_count).Fix();
				if (this.frozen_lv > 0 && B.BloodReplaceCol.a > 0)
				{
					B.Col = B.BloodReplaceCol;
				}
				this.checkLimit(false);
				if (call_change_func && this.fnUpdated != null)
				{
					this.fnUpdated(this.Current);
				}
				return true;
			}
			return false;
		}

		public byte frozen_lv
		{
			get
			{
				return this.frozen_lv_;
			}
			set
			{
				if (value == this.frozen_lv_)
				{
					return;
				}
				this.frozen_lv_ = value;
				bool flag = false;
				int frozen_index = this.frozen_index;
				if (value == 0)
				{
					if (frozen_index >= 0)
					{
						this.info_use--;
						BetoInfo betoInfo = this.AInfo[frozen_index];
						this.AInfo.RemoveAt(frozen_index);
						this.AInfo.Add(betoInfo);
					}
				}
				else
				{
					float num = 0.4f + 0.18f * (float)(value - 1);
					if (frozen_index >= 0)
					{
						BetoInfo betoInfo2 = this.AInfo[frozen_index];
						betoInfo2.level = num;
						this.AInfo.RemoveAt(frozen_index);
						this.AInfo.Insert(this.info_use - 1, betoInfo2);
					}
					else
					{
						while (this.info_use >= this.AInfo.Count)
						{
							this.AInfo.Add(new BetoInfo(null));
						}
						List<BetoInfo> ainfo = this.AInfo;
						int num2 = this.info_use;
						this.info_use = num2 + 1;
						ainfo[num2].CopyFrom(BetoInfo.FREEZE).Fix().FillId(ref this.total_fill_count)
							.level = num;
						flag = true;
					}
				}
				if (!flag)
				{
					foreach (KeyValuePair<string, BetobetoManager.SvTexture> keyValuePair in this.OTex)
					{
						keyValuePair.Value.dirt_index = global::XX.X.Mn(keyValuePair.Value.dirt_index, -1);
					}
				}
				if (this.fnUpdated != null)
				{
					this.fnUpdated(this.Current);
				}
			}
		}

		private void checkLimit(bool force_redraw = false)
		{
			int num = global::XX.X.Mx(2, global::XX.X.IntR(global::XX.X.NI(0.23f, 1f, global::XX.X.ZLINE((float)CFG.ui_effect_dirty, 10f)) * 16f)) + ((this.frozen_lv_ > 0) ? 1 : 0);
			if (this.info_use >= num)
			{
				int num2 = this.info_use - num + 1;
				bool flag = force_redraw || this.frozen_lv_ > 0 || CFG.ui_effect_dirty <= 7;
				foreach (KeyValuePair<string, BetobetoManager.SvTexture> keyValuePair in this.OTex)
				{
					if (keyValuePair.Value.dirt_index > 0)
					{
						if (flag)
						{
							keyValuePair.Value.cleanPrepare();
						}
						else
						{
							keyValuePair.Value.dirt_index -= num2;
						}
					}
				}
				int num3 = ((this.frozen_lv_ == 0) ? (-1) : this.frozen_index);
				if (num3 < 0 || num3 >= num2)
				{
					this.AInfo.RemoveRange(0, num2);
				}
				else
				{
					this.AInfo.RemoveRange(num3 + 1, num2 - num3);
					this.AInfo.RemoveRange(0, num3);
				}
				this.info_use -= num2;
			}
		}

		private int frozen_index
		{
			get
			{
				for (int i = 0; i < this.info_use; i++)
				{
					if (this.AInfo[i].type == BetoInfo.TYPE.FROZEN)
					{
						return i;
					}
				}
				return -1;
			}
		}

		public void cleanThread()
		{
			int num = this.Athread.Length;
			for (int i = 0; i < num; i++)
			{
				this.Athread[i].Clear();
			}
			this.thread_active = false;
			if (this.torned_count < 1f)
			{
				this.torned_count = 0f;
			}
		}

		public int get_current_dirt()
		{
			return this.info_use;
		}

		public void readBinaryFrom(ByteArray Ba)
		{
			this.cleanAll(true);
			int num = Ba.readByte();
			if (num <= 1)
			{
				Ba.readShort();
				for (int i = 0; i < 4; i++)
				{
					Ba.readShort();
				}
				return;
			}
			this.Xors.readBinaryFrom(Ba, false);
			this.total_fill_count = Ba.readInt();
			int num2 = (int)Ba.readUByte();
			if (num2 != this.Athread.Length)
			{
				this.Athread = new BetobetoManager.BetoThread[num2];
			}
			for (int j = 0; j < num2; j++)
			{
				this.Athread[j].readBinaryFrom(Ba, num);
			}
			this.info_use = (int)Ba.readUByte();
			while (this.info_use > this.AInfo.Count)
			{
				this.AInfo.Add(new BetoInfo(null));
			}
			for (int k = 0; k < this.info_use; k++)
			{
				this.AInfo[k].readBinaryFrom(Ba, num >= 4, num >= 5);
			}
			this.torned_count = global::XX.X.ZLINE((float)Ba.readUByte(), 255f);
			if (num >= 3)
			{
				this.wetten = Ba.readUByte() > 0;
			}
		}

		public void writeBinaryTo(ByteArray Ba)
		{
			Ba.writeByte(5);
			this.Xors.writeBinaryTo(Ba);
			Ba.writeInt(this.total_fill_count);
			int num = this.Athread.Length;
			Ba.writeByte(num);
			for (int i = 0; i < num; i++)
			{
				this.Athread[i].writeBinaryTo(Ba);
			}
			Ba.writeByte(this.info_use);
			for (int j = 0; j < this.info_use; j++)
			{
				this.AInfo[j].writeBinaryTo(Ba);
			}
			Ba.writeByte((int)((byte)(255f * global::XX.X.ZLINE(this.torned_count))));
			Ba.writeByte(this.wetten ? 1 : 0);
		}

		public bool is_torned
		{
			get
			{
				return this.torned_count >= 1f;
			}
		}

		public uint randA(uint i)
		{
			return this.Xors.randA[(int)(i & 127U)];
		}

		public uint GETRAN2(int seed1, int seed2)
		{
			return this.Xors.get2((uint)seed1, (uint)seed2);
		}

		public bool isActive()
		{
			return this.info_use > 0;
		}

		private void run(int fcnt)
		{
			if (!this.thread_active)
			{
				return;
			}
			int num = this.Athread.Length;
			this.thread_active = false;
			for (int i = 0; i < num; i++)
			{
				this.thread_active = this.Athread[i].run(fcnt) || this.thread_active;
			}
		}

		public bool addBetoFromEv(string key)
		{
			try
			{
				FieldInfo field = typeof(BetoInfo).GetField(key, BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField);
				if (field != null)
				{
					this.Check((BetoInfo)field.GetValue(null), true, true);
					return true;
				}
			}
			catch
			{
			}
			return false;
		}

		public bool EvtWait(bool is_first = false)
		{
			if (is_first)
			{
				return true;
			}
			foreach (KeyValuePair<string, BetobetoManager.SvTexture> keyValuePair in this.OTex)
			{
				if (keyValuePair.Value.isLoadStarted() && !keyValuePair.Value.isAsyncLoadFinished())
				{
					return true;
				}
			}
			return false;
		}

		public static byte immediate_load_material
		{
			get
			{
				return BetobetoManager.immediate_load_material_;
			}
			set
			{
				BetobetoManager.immediate_load_material_ = global::XX.X.Mx(BetobetoManager.immediate_load_material_, value);
			}
		}

		public static void quitBetobetoManager()
		{
			if (BetobetoManager.MtrBetoImg != null)
			{
				foreach (KeyValuePair<string, BetobetoManager> keyValuePair in BetobetoManager.OChara2Beto)
				{
					try
					{
						keyValuePair.Value.releaseAtlasData();
					}
					catch
					{
					}
				}
			}
		}

		public static void initBetobetoManager()
		{
			if (BetobetoManager.MtrBetoImg == null)
			{
				BetobetoManager.MdTemp = new MeshDrawer(null, 4, 6);
				BetobetoManager.MdTemp.draw_gl_only = true;
				BetobetoManager.BzPict = new BezierPictDrawer();
				BetobetoManager.BzPict.resolution = 12;
				BetobetoManager.MtrBetoImg = MTR.newMtr("Nel/BlitBetobetoImg");
				BetobetoManager.MtrBetoImg.SetTexture("_MainTex", MTR.MIiconL.Tx);
				MTRX.setMaterialST(BetobetoManager.MtrBetoImg, "_NTex", MTRX.SqEfPattern.getImage(6, 0), 0f);
				BetobetoManager.OChara2Beto = new BDic<string, BetobetoManager>();
				BetobetoManager.OChara2Beto["noel"] = new BetobetoManager("noel");
			}
		}

		public static BetobetoManager GetManager(string chara_key)
		{
			if (BetobetoManager.OChara2Beto == null)
			{
				BetobetoManager.initBetobetoManager();
			}
			BetobetoManager betobetoManager;
			if (!BetobetoManager.OChara2Beto.TryGetValue(chara_key, out betobetoManager))
			{
				betobetoManager = (BetobetoManager.OChara2Beto[chara_key] = new BetobetoManager(chara_key));
			}
			return betobetoManager;
		}

		public static bool redrawAll()
		{
			if (BetobetoManager.OChara2Beto == null)
			{
				return false;
			}
			bool flag = false;
			foreach (KeyValuePair<string, BetobetoManager> keyValuePair in BetobetoManager.OChara2Beto)
			{
				flag = keyValuePair.Value.redrawAllTexture() || flag;
			}
			return flag;
		}

		public static void runAll(int fcnt)
		{
			if (BetobetoManager.OChara2Beto == null)
			{
				return;
			}
			Bench.P("Noel-Beto");
			if (BetobetoManager.immediate_load_material_ > 0)
			{
				BetobetoManager.immediate_load_material_ = (byte)global::XX.X.Mx(0, (int)BetobetoManager.immediate_load_material_ - fcnt);
			}
			foreach (KeyValuePair<string, BetobetoManager> keyValuePair in BetobetoManager.OChara2Beto)
			{
				keyValuePair.Value.run(fcnt);
			}
			Bench.Pend("Noel-Beto");
		}

		public static void cleanThreadAll()
		{
			if (BetobetoManager.OChara2Beto == null)
			{
				return;
			}
			foreach (KeyValuePair<string, BetobetoManager> keyValuePair in BetobetoManager.OChara2Beto)
			{
				keyValuePair.Value.cleanThread();
			}
		}

		public static void flushSpecificSvTexture(string key)
		{
			if (BetobetoManager.OChara2Beto == null)
			{
				return;
			}
			foreach (KeyValuePair<string, BetobetoManager> keyValuePair in BetobetoManager.OChara2Beto)
			{
				BetobetoManager.SvTexture svTexture;
				if (keyValuePair.Value.OTex.TryGetValue(key, out svTexture))
				{
					svTexture.releaseAtlasData();
					UIPictureBodyData.releaseWithSvTexture(svTexture);
				}
			}
		}

		public static Material MtrBetoImg;

		public static BezierPictDrawer BzPict;

		private const float POWER_THRESH = 50f;

		private const float POWER_DIV = 0.006666667f;

		private const int DIRT_MAX = 16;

		public readonly string key;

		private BDic<string, BetobetoManager.SvTexture> OTex;

		private static List<Skin.SkinEntry> ASeBuf;

		private List<BetoInfo> AInfo;

		private int info_use;

		private BetobetoManager.BetoThread[] Athread;

		private static MeshDrawer MdTemp;

		private int total_fill_count;

		private XorsMaker Xors;

		public bool thread_active;

		public BetobetoManager.SvTexture Current;

		public float torned_count;

		public bool wetten;

		private byte frozen_lv_;

		public Func<BetobetoManager.SvTexture, bool> fnUpdated;

		private static byte immediate_load_material_;

		private static BDic<string, BetobetoManager> OChara2Beto;

		public sealed class SvTexture
		{
			public SvTexture(string _key)
			{
				this.key = _key;
				this.id = TX.text2id(this.key, 16777215);
				this.MtiText = MTI.LoadContainerSpine("SpineAnim/", this.key, null);
				this.MtiImage0 = MTI.LoadContainerOneImage("SpineAnim/" + _key, null, this.key);
			}

			public bool cleanExecute(bool load_immediately = false)
			{
				this.MtiImage0.addLoadKey("_SV", !load_immediately && !BetobetoManager.SvTexture.force_load_immediately && !this.initialize_load && BetobetoManager.immediate_load_material_ == 0);
				if (!this.MtiImage0.isAsyncLoadFinished())
				{
					return false;
				}
				this.dirt_index = 0;
				Texture image = this.MtiImage0.Image;
				if (this.Base == null)
				{
					this.texture_rev_w = 1f / (float)image.width;
					this.texture_rev_h = 1f / (float)image.height;
					this.Base = new RenderTexture(image.width, image.height, 16, RenderTextureFormat.ARGB32, 1);
					this.Base.name = image.name;
					this.Base.antiAliasing = 2;
					this.Base.wrapMode = TextureWrapMode.Clamp;
					BLIT.Clear(this.Base, 0U, true);
				}
				BLIT.PasteTo(this.Base, image, (float)this.Base.width * 0.5f, (float)this.Base.height * 0.5f, 1f, 0f, 0f, 1f, 1f);
				RenderTexture.active = null;
				return true;
			}

			public void releaseCached(bool do_not_clean = false)
			{
				if (this.dirt_index < 0 && !do_not_clean && !this.cleanExecute(true))
				{
					return;
				}
				this.MtiImage0.remLoadKey("_SV");
			}

			public bool cleanPrepare()
			{
				if (this.dirt_index > 0)
				{
					this.dirt_index = -1;
					return true;
				}
				return false;
			}

			public bool isAsyncLoadFinished()
			{
				if (this.MtiImage0.isAsyncLoadFinished() && this.Base != null)
				{
					return true;
				}
				if (this.dirt_index < 0)
				{
					return this.cleanExecute(false);
				}
				return this.Base != null;
			}

			public void releaseAtlasData()
			{
				this.releaseTexture();
				this.SpAtlasAsset = null;
				this.SpDataAsset = null;
				this.MtiText.remLoadKey("_SV");
				this.MtiImage0.remLoadKey("_SV");
			}

			public RenderTexture getRendered()
			{
				return this.Base;
			}

			public bool isLoadStarted()
			{
				return this.MtiImage0.hasLoadKey("_SV");
			}

			public bool releaseTexture()
			{
				bool flag = this.Base != null;
				this.dirt_index = -1;
				BLIT.nDispose(this.Base);
				this.MtiImage0.remLoadKey("_SV");
				this.Base = null;
				this.atlas_depth_written = false;
				return flag;
			}

			public void prepareAtlasAssets(out SpineAtlasAsset _SpAtlasAsset, out SkeletonDataAsset _SpDataAsset, Material[] AMtr, string replace_json_key = null)
			{
				bool flag = this.initialize_load;
				if (this.MtiText.isWrong(this.SpAtlasAsset))
				{
					this.SpAtlasAsset = null;
				}
				if (this.SpAtlasAsset == null)
				{
					this.MtiText.addLoadKey("_SV", false);
					SpineViewer.prepareAtlasAssetsS(this.MtiText, out this.SpAtlasAsset, out this.SpDataAsset, replace_json_key);
					this.SpAtlasAsset.materials = AMtr;
					if (!this.atlas_depth_written && flag)
					{
						this.readAtlas();
					}
				}
				else
				{
					this.SpAtlasAsset.materials = AMtr;
					if (flag)
					{
						this.preapreAtlasDepth();
					}
				}
				_SpAtlasAsset = this.SpAtlasAsset;
				_SpDataAsset = this.SpDataAsset;
			}

			public BetobetoManager.SvTexture preapreAtlasDepth()
			{
				if (!this.atlas_depth_written)
				{
					this.readAtlas();
				}
				return this;
			}

			private void readAtlas()
			{
				if (this.dirt_index != 0 && !this.cleanExecute(false))
				{
					return;
				}
				if (this.SpDataAsset == null)
				{
					return;
				}
				if (this.AZBufRect == null)
				{
					this.SpAtlasAsset.materials[0].mainTexture = this.Base;
					SkeletonData skeletonData = this.SpDataAsset.GetSkeletonData(false);
					Atlas atlas = this.SpAtlasAsset.GetAtlas(false);
					List<BetobetoManager.SvTexture.AtlRect> list = new List<BetobetoManager.SvTexture.AtlRect>(16);
					if (skeletonData == null)
					{
						throw new Exception("No Skeleton Data:" + this.key);
					}
					ExposedList<Skin> skins = skeletonData.Skins;
					int count = atlas.Regions.Count;
					int num = 0;
					for (int i = 0; i < count; i++)
					{
						AtlasRegion atlasRegion = atlas.Regions[i];
						if (REG.match(atlasRegion.name, this.RegCaptionEM))
						{
							BetobetoManager.SvTexture.AtlRect atlRect = new BetobetoManager.SvTexture.AtlRect(atlasRegion);
							if (atlRect.nd_mode)
							{
								list.Add(atlRect);
								num++;
							}
							else
							{
								list.Insert(list.Count - num, atlRect);
							}
							if (skins != null)
							{
								int count2 = skins.Count;
								for (int j = 0; j < count2; j++)
								{
									Skin skin = skins.Items[j];
									BetobetoManager.ASeBuf.Clear();
									skin.copyAttachmentObject(BetobetoManager.ASeBuf);
									for (int k = BetobetoManager.ASeBuf.Count - 1; k >= 0; k--)
									{
										MeshAttachment meshAttachment = BetobetoManager.ASeBuf[k].Attachment as MeshAttachment;
										if (meshAttachment != null && !(meshAttachment.Path != atlRect.key))
										{
											atlRect.AddTarget(meshAttachment);
										}
									}
								}
							}
						}
					}
					this.AZBufRect = list.ToArray();
				}
				this.writeDepthBuffer();
			}

			private unsafe void writeDepthBuffer()
			{
				MeshDrawer mdTemp = BetobetoManager.MdTemp;
				Material mtrDrawDepth = BLIT.MtrDrawDepth;
				mdTemp.activate("svnel", mtrDrawDepth, true, C32.d2c(4294901760U), null);
				this.atlas_depth_written = true;
				int num = this.AZBufRect.Length;
				Graphics.SetRenderTarget(this.Base);
				GL.LoadOrtho();
				mtrDrawDepth.SetPass(0);
				GL.Begin(4);
				mdTemp.base_z = 0.5f;
				bool flag = false;
				for (int i = 0; i < num; i++)
				{
					BetobetoManager.SvTexture.AtlRect atlRect = this.AZBufRect[i];
					if (atlRect.nd_mode && !flag)
					{
						flag = true;
						mdTemp.base_z = 0.75f;
						GL.End();
						GL.Flush();
						mtrDrawDepth.SetPass(0);
						GL.Begin(4);
					}
					if (atlRect.VTarget != null)
					{
						for (int j = atlRect.VTarget.Count - 1; j >= 0; j--)
						{
							MeshAttachment meshAttachment = atlRect.VTarget[j];
							float[] regionUVs = meshAttachment.RegionUVs;
							int[] triangles = meshAttachment.Triangles;
							if (regionUVs != null && triangles != null)
							{
								fixed (int* ptr = &triangles[0])
								{
									int* ptr2 = ptr;
									int k = triangles.Length;
									int* ptr3 = ptr2;
									mdTemp.allocTri(mdTemp.getTriMax() + k, 60);
									while (k > 0)
									{
										mdTemp.Tri(*(ptr3++), *(ptr3++), *(ptr3++), false);
										k -= 3;
									}
								}
								fixed (float* ptr4 = &regionUVs[0])
								{
									float* ptr5 = ptr4;
									int l = regionUVs.Length;
									mdTemp.allocVer(mdTemp.getVertexMax() + l, 64);
									float* ptr6 = ptr5;
									while (l > 0)
									{
										float num2 = *(ptr6++);
										float num3 = *(ptr6++);
										BetobetoManager.SvTexture.considerSpineMeshPos(ref num2, ref num3, atlRect.degrees);
										mdTemp.Pos(atlRect.x + num2 * atlRect.width, atlRect.y + atlRect.height * num3, null);
										l -= 2;
									}
								}
							}
						}
					}
					else
					{
						mdTemp.RectBL(atlRect.x, atlRect.y, atlRect.width, atlRect.height, true);
					}
					BLIT.RenderToGLOneTask(mdTemp, mdTemp.getTriMax(), false);
				}
				GL.End();
				Graphics.SetRenderTarget(null);
				mdTemp.clearSimple();
				GL.Flush();
			}

			public static void considerSpineMeshPos(ref float rx, ref float ry, int degrees)
			{
				float num;
				float num2;
				if (degrees != 90)
				{
					if (degrees != 180)
					{
						if (degrees != 270)
						{
							num = rx;
							num2 = 1f - ry;
						}
						else
						{
							num = 1f - ry;
							num2 = 1f - rx;
						}
					}
					else
					{
						num = 1f - rx;
						num2 = ry;
					}
				}
				else
				{
					num = ry;
					num2 = rx;
				}
				rx = num;
				ry = num2;
			}

			public bool texture_prepared
			{
				get
				{
					return this.dirt_index >= 0;
				}
			}

			public Texture prepareTexture(bool load_immediately = false)
			{
				if (this.dirt_index < 0 && !this.cleanExecute(load_immediately))
				{
					return null;
				}
				return this.Base;
			}

			public bool runBetobeto(int cur_dirt, BetobetoManager BCon)
			{
				bool flag = this.dirt_index < 0;
				if (this.prepareTexture(false) == null)
				{
					return false;
				}
				if (cur_dirt <= this.dirt_index)
				{
					return !flag;
				}
				if (BCon == null)
				{
					return false;
				}
				BetoInfo info = BCon.getInfo(this.dirt_index);
				if (CFG.ui_effect_dirty == 0 && info.type != BetoInfo.TYPE.FROZEN)
				{
					return !flag;
				}
				float num = info.level * global::XX.X.ZLINE((float)CFG.ui_effect_dirty, 7f);
				float num2 = global::XX.X.NI(0.66f, 1f, global::XX.X.ZLINE((float)CFG.ui_effect_dirty, 7f));
				MeshDrawer mdTemp = BetobetoManager.MdTemp;
				float num3 = info.scale * num2;
				float num4 = info.scale * num2;
				int frozen_index = BCon.frozen_index;
				mdTemp.base_z = ((frozen_index < 0 || this.dirt_index < frozen_index) ? 0.125f : 0.625f);
				uint num5 = BCon.GETRAN2((info.fill_id & 255) + this.id * 27, 1 + info.fill_id % 5 + this.id % 4);
				Graphics.SetRenderTarget(this.Base);
				GL.PushMatrix();
				GL.LoadOrtho();
				GL.MultMatrix(Matrix4x4.Scale(new Vector3(64f / (float)this.Base.width, 64f / (float)this.Base.height, 1f)));
				int num6 = 0;
				int num7 = 0;
				if (info.type == BetoInfo.TYPE.FROZEN)
				{
					Material material = null;
					float num8 = (float)this.Base.width / 512f / info.scale;
					float num9 = (float)this.Base.height / 512f / info.scale;
					for (int i = 2; i >= 0; i--)
					{
						material = ((i == 2) ? MTR.MtrFrozenGdtS : ((i == 1) ? MTR.MtrFrozenGdtA : MTR.MtrFrozen));
						material.SetFloat("_Level", info.level);
						material.SetFloat("_ScaleX", num8);
						material.SetFloat("_ScaleY", num9);
						material.SetFloat("_ZTest", 4f);
						if (i >= 1)
						{
							material.mainTexture = MTR.MIiconL.Tx;
						}
					}
					material.SetColor("_Color", C32.d2c(4288008150U));
					material.SetColor("_BColor", C32.d2c(4279786863U));
					material.SetColor("_WColor", C32.d2c(4292867578U));
					mdTemp.activate("frozen", material, true, info.Col, null);
					mdTemp.initForImgAndTexture(this.Base);
					material.SetPass(0);
					GL.Begin(4);
					mdTemp.Col = MTRX.ColWhite;
					mdTemp.RectBL(0f, 0f, (float)this.Base.width, (float)this.Base.height, false);
					BLIT.RenderToGLOneTask(mdTemp, mdTemp.getTriMax(), false);
					GL.End();
					GL.Flush();
					num6 = global::XX.X.IntC((float)(this.Base.width * this.Base.height) * global::XX.X.ZPOW(info.level - 0.4f, 0.6f) / 6000f);
					num7 = global::XX.X.Mx(1, (int)((float)num6 * 0.45f));
					mdTemp.activate("frozen", MTR.MtrFrozenGdtS, true, info.Col, null);
					mdTemp.Col = C32.d2c(1714631435U);
					MTR.MtrFrozenGdtS.SetPass(0);
					GL.Begin(4);
					while (--num6 >= 0)
					{
						num5 = BCon.GETRAN2(num6 * 13 + (int)(num5 & 255U), num6 % 7 + (int)(num5 & 7U));
						PxlFrame pxlFrame = MTR.ANoelBreakCloth[(long)((ulong)num5 % (ulong)((long)MTR.ANoelBreakCloth.Length))];
						mdTemp.initForImg(pxlFrame.getLayer(0).Img, 0);
						mdTemp.RotaGraph(global::XX.X.RAN(num5, 453) * (float)this.Base.width, global::XX.X.RAN(num5, 1280) * (float)this.Base.height, global::XX.X.NI(2.2f, 2.7f, global::XX.X.RAN(num5, 1143)), global::XX.X.RAN(num5, 1212) * 6.2831855f, null, global::XX.X.RAN(num5, 567) < 0.5f);
						BLIT.RenderToGLOneTask(mdTemp, mdTemp.getTriMax(), false);
						if (--num7 == 0)
						{
							GL.End();
							GL.Flush();
							mdTemp.activate("frozen", MTR.MtrFrozenGdtA, true, info.Col, null);
							MTR.MtrFrozenGdtA.SetPass(0);
							mdTemp.Col = C32.d2c(4282104531U);
							GL.Begin(4);
						}
					}
					GL.End();
				}
				else
				{
					int num10 = (int)(num5 & 3U);
					float num11 = (float)(this.Base.width * this.Base.height) * num * 0.75f * global::XX.X.NI(0.4f, 1f, global::XX.X.ZLINE(info.scale));
					float num12 = num11 * 0.35f;
					float num13 = 0f;
					Material material = BetobetoManager.MtrBetoImg;
					mdTemp.activate("svnel_betobeto", material, true, info.Col, null);
					if (info.BloodReplaceCol.a > 0 && CFG.blood_weaken > 0)
					{
						num13 = global::XX.X.Mx(0.125f, num11 * (1f - global::XX.X.ZSINV(1f - (float)CFG.blood_weaken / (float)CFG.BLOOD_WEAKEN_MAX) * global::XX.X.NI(0.75f, 1f, global::XX.X.RAN(num5, 1953))));
						mdTemp.Col = info.BloodReplaceCol;
					}
					material.SetColor("_BColor", info.Col2);
					material.SetFloat("_TextureScale", info.scale);
					material.SetFloat("_Density", (float)CFG.ui_effect_density / 10f);
					material.SetPass(0);
					GL.Begin(4);
					float num14 = 0f;
					float num15 = 0f;
					float num16 = 0f;
					float num17 = 0f;
					float num18 = global::XX.X.RAN(num5, 2896) * (float)this.Base.width;
					float num19 = global::XX.X.RAN(num5, 526) * (float)this.Base.height;
					float num20 = info.jumprate * global::XX.X.NI(0.6f, 1f, global::XX.X.RAN(num5, 1700));
					while (num14 < num11)
					{
						num5 = BCon.GETRAN2(num6 * 13 + (int)(num5 & 255U), num6 % 44 + (int)(num5 & 31U));
						PxlSequence pxlSequence = null;
						PxlImage pxlImage = null;
						switch (info.type)
						{
						case BetoInfo.TYPE.SMOKE:
							pxlSequence = ((num12 > num14) ? MTR.SqParticleSperm : MTR.SqParticleSplash);
							break;
						case BetoInfo.TYPE.LIQUID:
							pxlSequence = MTR.SqParticleSperm;
							break;
						case BetoInfo.TYPE.STAIN:
							pxlSequence = MTR.SqEfSabi;
							break;
						case BetoInfo.TYPE.CUTTED:
						{
							PxlSequence sqBezierCutted = MTR.SqBezierCutted;
							pxlImage = sqBezierCutted.getFrame((int)((ulong)num5 % (ulong)((long)sqBezierCutted.countFrames()))).getLayer(0).Img;
							break;
						}
						default:
							continue;
						}
						PxlFrame pxlFrame2 = null;
						if (pxlSequence != null)
						{
							pxlFrame2 = pxlSequence.getFrame((int)((ulong)num5 % (ulong)((long)pxlSequence.countFrames())));
							pxlImage = pxlFrame2.getLayer(0).Img;
						}
						float num21 = 1f;
						float num22 = 1f;
						if (num7 > 0 && global::XX.X.RAN(num5, 493) < num20)
						{
							num7 = 0;
						}
						float num25;
						float num26;
						if (num7 == 0)
						{
							float num23 = global::XX.X.RAN(num5, 1494);
							float num24 = global::XX.X.RAN(num5, 670);
							if (((num23 >= 0.5f) ? 1 : 0) + ((num24 >= 0.5f) ? 2 : 0) == num10)
							{
								num6++;
								continue;
							}
							num25 = (float)this.Base.width * num23;
							num26 = (float)this.Base.height * num24;
							num17 = 3.1415927f * global::XX.X.NI(-0.15f, 0.15f, global::XX.X.RAN(num5, 510));
						}
						else
						{
							float num27 = global::XX.X.RAN(num5, 2100) * 6.2831855f;
							float num28 = (global::XX.X.NI(12, 30, global::XX.X.RAN(num5, 728)) + (float)((num7 == 1) ? 25 : 0)) * num3;
							float num29 = global::XX.X.NI(0.06f, 0.3f, global::XX.X.ZPOW(global::XX.X.RAN(num5, 782)));
							num25 = num15 + num28 * global::XX.X.Cos(num27);
							num26 = num16 + num28 * global::XX.X.Sin(num27);
							num21 *= num29;
							num22 *= num29;
						}
						float num30 = global::XX.X.NI(0.75f, 2.25f, global::XX.X.RAN(num5, 1530));
						num21 *= num3 * num30;
						num22 *= num4 * (num30 + global::XX.X.NI(-0.1f, 0.1f, global::XX.X.RAN(num5, 1478)));
						float num31 = (float)pxlImage.width;
						if (info.type == BetoInfo.TYPE.CUTTED)
						{
							BetobetoManager.BzPict.PtCenterPx(-128f, 0f, 0f, (20f + 14f * global::XX.X.RAN(num5, 2754)) * (float)global::XX.X.MPF(global::XX.X.RAN(num5, 2851) < 0.5f), global::XX.X.NI(8, 11, global::XX.X.RAN(num5, 1629)) * 4f, global::XX.X.NI(-0.06f, 0.06f, global::XX.X.RAN(num5, 2989)) * 3.1415927f, 128f, 0f);
							mdTemp.initForImg(pxlImage, 0);
							Matrix4x4 currentMatrix = mdTemp.getCurrentMatrix();
							num31 = global::XX.X.NI(pxlImage.width, 140, 0.6f);
							mdTemp.TranslateP((num25 + num18) % (float)this.Base.width, (num26 + num19) % (float)this.Base.height, true).Rotate(num17 + global::XX.X.NI(-0.03f, 0.03f, global::XX.X.RAN(num5, 2252)) * 3.1415927f, true).Scale(num31 * num21 * 0.015625f / 4f, 1f, true);
							BetobetoManager.BzPict.drawTo(mdTemp, 0f, 0f, num22 * (float)pxlImage.height * 2f * 0.015625f, true);
							mdTemp.setCurrentMatrix(currentMatrix, false);
							num31 *= 1.5f;
						}
						else
						{
							mdTemp.RotaPF((num25 + num18) % (float)this.Base.width, (num26 + num19) % (float)this.Base.height, num21, num22, global::XX.X.RAN(num5, 2342) * 6.2831855f, pxlFrame2, (num5 & 1U) > 0U, false, false, uint.MaxValue, false, 0);
						}
						num15 = num25;
						num16 = num26;
						num6++;
						num7++;
						float num32 = num31 * num21 * (float)pxlImage.height * num22;
						num14 += num32;
						if (num13 > 0f)
						{
							num13 -= num32;
							if (num13 <= 0f)
							{
								mdTemp.Col = info.Col;
							}
						}
						BLIT.RenderToGLOneTask(mdTemp, mdTemp.getTriMax(), false);
					}
					BLIT.RenderToGLOneTask(mdTemp, mdTemp.getTriMax(), false);
					GL.End();
				}
				Graphics.SetRenderTarget(null);
				GL.PopMatrix();
				GL.Flush();
				int num33 = this.dirt_index + 1;
				this.dirt_index = num33;
				return num33 == cur_dirt && !flag;
			}

			private RenderTexture Base;

			public static bool force_load_immediately;

			public readonly int id;

			public readonly string key;

			public readonly MTIOneImage MtiImage0;

			public readonly MTISpine MtiText;

			public int dirt_index = -1;

			public bool atlas_depth_written;

			private Regex RegCaption = new Regex("^[\\w \\-]+$");

			private Regex RegCaptionEM = new Regex("^(EM\\d*_|ND\\d*_|m\\d+_|f\\d+_*m\\d+_)[\\w \\-]+$");

			private Regex RegAttribute = new Regex("([\\w]+)\\:[ \\s\\t]+(\\-?\\d+)\\,[ \\s\\t]*(\\-?\\d+)$");

			public float texture_rev_w;

			public float texture_rev_h;

			public bool initialize_load;

			private const float depth_z_nd = 0.75f;

			private const float depth_z_frozen = 0.625f;

			private const float depth_z_em = 0.5f;

			private const float depth_z_normal = 0.125f;

			private SpineAtlasAsset SpAtlasAsset;

			private SkeletonDataAsset SpDataAsset;

			private BetobetoManager.SvTexture.AtlRect[] AZBufRect;

			private const string mti_load_key = "_SV";

			public sealed class AtlRect : DRect
			{
				public AtlRect(string _key, int _degrees, float a, float b = 0f, float _w = 0f, float _h = 0f)
					: base(_key, a, b, _w, _h, 0f)
				{
					this.degrees = _degrees;
					this.nd_mode = TX.isStart(_key, "ND", 0);
				}

				public AtlRect(AtlasRegion Reg)
					: this(Reg.name, Reg.degrees, Reg.u, Reg.v2, Reg.u2 - Reg.u, Reg.v - Reg.v2)
				{
				}

				public void AddTarget(MeshAttachment V)
				{
					if (this.VTarget == null)
					{
						this.VTarget = new List<MeshAttachment>(1);
						this.VTarget.Add(V);
						return;
					}
					if (this.VTarget[0].Triangles.Length == V.Triangles.Length && this.VTarget[0].Vertices[0] == V.Vertices[0])
					{
						return;
					}
					if (this.nd_mode)
					{
						this.VTarget.Add(V);
						return;
					}
					if (this.VTarget[0].Triangles.Length <= V.Triangles.Length)
					{
						this.VTarget[0] = V;
					}
				}

				public List<MeshAttachment> VTarget;

				public int degrees;

				public bool nd_mode;
			}
		}

		private struct BetoThread
		{
			public void readBinaryFrom(ByteArray Ba, int vers)
			{
				this.stock = Ba.readFloat();
			}

			public void writeBinaryTo(ByteArray Ba)
			{
				Ba.writeFloat(this.stock);
			}

			public bool run(int fcnt)
			{
				if (this.lockf > 0)
				{
					this.lockf = global::XX.X.Mx(this.lockf - fcnt, 0);
				}
				return this.lockf > 0;
			}

			public void Clear()
			{
				this.lockf = 0;
				this.stock = 0f;
			}

			public float stock;

			public int lockf;
		}
	}
}
