using System;
using System.Reflection;
using Better;
using UnityEngine;

namespace XX
{
	public class EffectItem : EffectItemBase, IRunAndDestroy
	{
		public static float _x
		{
			get
			{
				return EffectItem.E0.x;
			}
		}

		public static float _y
		{
			get
			{
				return EffectItem.E0.y;
			}
		}

		public virtual int getParticleCount(EffectItem Ef, int default_cnt)
		{
			if (this.EF == null)
			{
				return default_cnt;
			}
			return this.EF.getParticleCount(this, default_cnt);
		}

		public virtual float getParticleSpeed(EffectItem Ef, int cnt, float default_maxt)
		{
			if (this.EF == null)
			{
				return 1f;
			}
			return this.EF.getParticleSpeed(this, cnt, default_maxt);
		}

		public static void initEffectItem()
		{
			if (EffectItem.Col1 != null)
			{
				return;
			}
			EffectItem.Col1 = new C32();
			EffectItem.Col2 = new C32();
			EffectItem.OFnCache = new BDic<string, FnEffectRun>(64);
			EffectItem.initParticleType();
		}

		public static void initParticleType()
		{
			EfParticle.assignType("CROSS", new FnPtcInit(EffectItem.fnPtcInitColor), new FnPtcDraw(EffectItem.fnPtcDrawCross));
			EfParticle.assignType("SUNABOKORI", new FnPtcInit(EffectItem.fnPtcInitSunabokori), new FnPtcDraw(EfParticle.DrawBasicBML));
			if (MTRX.auto_load_efparticle)
			{
				EfParticleManager.reloadParticleCsv(false);
			}
		}

		public static bool fnPtcInitColor(EfParticle EFP, EfParticleVarContainer mde)
		{
			EFP.initColor(mde);
			return true;
		}

		public static bool fnPtcInitSunabokori(EfParticle EFP, EfParticleVarContainer mde)
		{
			EFP.initColor(mde);
			EFP.BmL = MTRX.SqEfSunabokori;
			return true;
		}

		public static bool fnPtcDrawCross(EffectItem E, EfParticle EP, uint ran)
		{
			EP.GetMesh(E, null, true);
			MeshDrawer md = EfParticle.Md;
			float num = X.Cos(EfParticle._agR);
			float num2 = X.Sin(EfParticle._agR);
			float num3 = EfParticle._zm / 2f;
			md.Line(EfParticle.cx - num * num3, EfParticle.cy - num2 * num3, EfParticle.cx + num * num3, EfParticle.cy + num2 * num3, EfParticle._thick, false, 0f, 0f);
			md.Line(EfParticle.cx - num2 * num3, EfParticle.cy + num * num3, EfParticle.cx + num2 * num3, EfParticle.cy - num * num3, EfParticle._thick, false, 0f, 0f);
			return true;
		}

		public EffectItem(IEffectSetter _EF, string _title = "", float _x = 0f, float _y = 0f, float _z = 0f, int _time = 0, int _saf = 0)
			: base("", 0f, 0f, 0f, 0)
		{
			this.clear(_EF, _title, _x, _y, _z, _time, _saf);
		}

		public void setFunction(FnEffectRun _Fn, string _title = null)
		{
			this.FnDef_ = _Fn;
			if (_title != null)
			{
				this.title = _title;
			}
		}

		public override void destruct()
		{
			this.EF = null;
			this.FnDef_ = null;
			base.destruct();
		}

		public virtual void clear(IEffectSetter _EF, string _title = "", float _x = 0f, float _y = 0f, float _z = 0f, int _time = 0, int _saf = 0)
		{
			this.EF = _EF;
			base.clearBase(_title, _x, _y, _z, _time);
			this.f0 += (int)this.saf;
			if (_saf >= 0)
			{
				this.saf = (float)_saf;
			}
			else
			{
				base.addaf((float)(-(float)_saf));
				this.saf = 0f;
			}
			this.Md = null;
			this.title == "";
		}

		public virtual EffectItem initEffect(string type_name = "")
		{
			if (this.FnDef_ != null || type_name == null)
			{
				return this;
			}
			string title = this.title;
			if (title == null || !(title == "particle"))
			{
				this.FnDef_ = X.Get<string, FnEffectRun>(EffectItem.OFnCache, this.title);
				if (this.FnDef_ != null)
				{
					return this;
				}
				MethodInfo method = Type.GetType((type_name == "") ? "XX.EffectItem,unsafeAssem" : type_name).GetMethod("fnRunDraw_" + this.title);
				if (method != null)
				{
					EffectItem.OFnCache[this.title] = (this.FnDef_ = (FnEffectRun)Delegate.CreateDelegate(typeof(FnEffectRun), method));
				}
				else
				{
					X.de("不明な EffectItem タイトル: " + this.title, null);
				}
			}
			return this;
		}

		public EffectItem setEffectSpecificFn(FnEffectRun Fn)
		{
			this.FnDef_ = Fn;
			return this;
		}

		public virtual bool run(float fcnt = 1f)
		{
			if (this.saf > 0f)
			{
				if (this.FnDef_ == null)
				{
					return false;
				}
				this.saf = X.Mx(0f, this.saf - fcnt);
			}
			else
			{
				string text = (TX.valid(this.title) ? this.title : "_");
				Bench.P(text);
				if (this.FnDef_ == null || !this.FnDef_(this))
				{
					Bench.Pend(text);
					return false;
				}
				base.addaf(fcnt);
				Bench.Pend(text);
			}
			return true;
		}

		public static EffectItem fnCreateOne(IEffectSetter _EF, string _title = "", float _x = 0f, float _y = 0f, float _z = 0f, int _time = 0, int _saf = 0)
		{
			return new EffectItem(_EF, _title, _x, _y, _z, _time, _saf);
		}

		public FnEffectRun FnDef
		{
			get
			{
				return this.FnDef_;
			}
			set
			{
				if (this.FnDef_ != null && value == null)
				{
					this.saf = 0f;
				}
				this.FnDef_ = value;
			}
		}

		public virtual MeshDrawer GetMesh(string title, Material Mtr, bool bottom_flag = false)
		{
			bool flag;
			this.Md = this.EF.MeshInit(title, this.x, this.y, EffectItem.Col1, Mtr, out flag, bottom_flag, null, false);
			return this.Md;
		}

		public virtual MeshDrawer GetMesh(string title, uint col, BLEND blend = BLEND.NORMAL, bool bottom_flag = false)
		{
			bool flag;
			this.Md = this.EF.MeshInit(title, this.x, this.y, col, out flag, blend, bottom_flag);
			return this.Md;
		}

		public MeshDrawer GetMesh(string title, C32 Cd, BLEND blend = BLEND.NORMAL, bool bottom_flag = false)
		{
			return this.GetMesh(title, Cd.rgba, blend, bottom_flag);
		}

		public MeshDrawer GetMeshGradation(string title, C32 Cd, C32 Cd2, GRD grd, BLEND blend = BLEND.NORMAL, bool bottom_flag = false)
		{
			return this.GetMeshGradation(title, Cd.rgba, Cd2.rgba, grd, blend, bottom_flag);
		}

		public virtual MeshDrawer GetMeshGradation(string title, uint col, uint col2, GRD grd, BLEND blend = BLEND.NORMAL, bool bottom_flag = false)
		{
			Color32 color = C32.codeToColor32(col);
			Color32 color2 = C32.codeToColor32(col2);
			bool flag;
			this.Md = this.EF.MeshInitGradation(title, this.x, this.y, C32.Color32ToCode(color), C32.Color32ToCode(color2), grd, out flag, blend, bottom_flag);
			return this.Md;
		}

		public virtual MeshDrawer GetMeshImg(string title, MImage MI, BLEND blend = BLEND.NORMAL, bool bottom_flag = false)
		{
			bool flag;
			this.Md = this.EF.MeshInitImg(title, this.x, this.y, MI, out flag, blend, bottom_flag);
			return this.Md;
		}

		public bool isinCameraPtc(float left_px, float btm_px, float right_px, float top_px, float margin_px = 0f)
		{
			return this.EF.isinCameraPtc(this, left_px - margin_px, btm_px - margin_px, right_px + margin_px, top_px + margin_px, 1f, 0f);
		}

		public bool isinCameraPtcCen(float margin_pixel)
		{
			return this.EF.isinCameraPtc(this, -margin_pixel, -margin_pixel, margin_pixel, margin_pixel, 1f, 0f);
		}

		public bool isinCameraPtcCen(float margin_x, float margin_y)
		{
			return this.EF.isinCameraPtc(this, -margin_x, -margin_y, margin_x, margin_y, 1f, 0f);
		}

		public float saf;

		public IEffectSetter EF;

		public static EffectItem E0;

		private FnEffectRun FnDef_;

		public static C32 Col1;

		private static BDic<string, FnEffectRun> OFnCache;

		public static C32 Col2;

		public MeshDrawer Md;

		public enum EFTYPE : byte
		{
			NORMAL,
			UI
		}
	}
}
