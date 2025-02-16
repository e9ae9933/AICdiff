using System;
using UnityEngine;

namespace XX
{
	public class TransEffecter : Effect<TransEffecterItem>
	{
		public TransEffecter(string effect_name, GameObject _Gob, int alloc_cnt, int _layer_effect_top, int _layer_effect_bottom, EFCON_TYPE _ef_type = EFCON_TYPE.NORMAL)
			: base(_Gob, alloc_cnt)
		{
			this.useMeshDrawer = false;
			this.ColMul = new C32(uint.MaxValue);
			this.ColAdd = new C32(0);
			this.initEffect("TE-" + effect_name, null, null, _ef_type);
			this.ACol = new TransEffecter.TeColorize[2];
			this.APos = new TransEffecter.TePosit[2];
			this.AScl = new TransEffecter.TeScaler[2];
		}

		public TransEffecter RegisterCol(MeshDrawer Md)
		{
			X.pushToEmptyS<TransEffecter.TeColorize>(ref this.ACol, new TransEffecter.TeColorize
			{
				Md = Md,
				ColDef = Md.getColorTe()
			}, ref this.col_len, 16);
			return this;
		}

		public TransEffecter RegisterCol(SpriteRenderer Spr, bool default_is_gray = false)
		{
			float num = (default_is_gray ? 0.5f : 1f);
			X.pushToEmptyS<TransEffecter.TeColorize>(ref this.ACol, new TransEffecter.TeColorize
			{
				Spr = Spr,
				ColDef = Spr.color,
				add_apply_level = num
			}, ref this.col_len, 16);
			return this;
		}

		public TransEffecter RegisterCol(ITeColor Colr, bool default_is_gray = false)
		{
			float num = (default_is_gray ? 0.5f : 1f);
			X.pushToEmptyS<TransEffecter.TeColorize>(ref this.ACol, new TransEffecter.TeColorize
			{
				Colr = Colr,
				ColDef = Colr.getColorTe(),
				add_apply_level = num
			}, ref this.col_len, 16);
			return this;
		}

		public TransEffecter UnregisterCol(ITeColor Colr, bool default_is_gray = false)
		{
			for (int i = this.col_len - 1; i >= 0; i--)
			{
				if (this.ACol[i].Colr == Colr)
				{
					X.shiftNotInput1<TransEffecter.TeColorize>(this.ACol, i, ref this.col_len);
					break;
				}
			}
			return this;
		}

		public TransEffecter RegisterPos(Transform _Trs)
		{
			X.pushToEmptyS<TransEffecter.TePosit>(ref this.APos, new TransEffecter.TePosit
			{
				Trs = _Trs,
				PosDef = _Trs.localPosition
			}, ref this.pos_len, 16);
			return this;
		}

		public TransEffecter RegisterPos(GameObject _Gob)
		{
			X.pushToEmptyS<TransEffecter.TePosit>(ref this.APos, new TransEffecter.TePosit
			{
				Trs = _Gob.transform,
				PosDef = _Gob.transform.localPosition
			}, ref this.pos_len, 16);
			return this;
		}

		public TransEffecter RegisterPos(ITeShift _Trs)
		{
			X.pushToEmptyS<TransEffecter.TePosit>(ref this.APos, new TransEffecter.TePosit
			{
				Shifter = _Trs,
				PosDef = _Trs.getShiftTe()
			}, ref this.pos_len, 16);
			return this;
		}

		public TransEffecter RegisterScl(Transform _Trs)
		{
			X.pushToEmptyS<TransEffecter.TeScaler>(ref this.AScl, new TransEffecter.TeScaler
			{
				Trs = _Trs,
				SclDef = _Trs.localScale
			}, ref this.scl_len, 16);
			return this;
		}

		public TransEffecter RegisterScl(GameObject _Gob)
		{
			X.pushToEmptyS<TransEffecter.TeScaler>(ref this.AScl, new TransEffecter.TeScaler
			{
				Trs = _Gob.transform,
				SclDef = _Gob.transform.localScale
			}, ref this.scl_len, 16);
			return this;
		}

		public TransEffecter RegisterScl(ITeScaler _Trs)
		{
			X.pushToEmptyS<TransEffecter.TeScaler>(ref this.AScl, new TransEffecter.TeScaler
			{
				Scaler = _Trs,
				SclDef = _Trs.getScaleTe()
			}, ref this.scl_len, 16);
			return this;
		}

		public void clearRegistered()
		{
			this.clear();
			this.col_len = 0;
			this.scl_len = 0;
			this.pos_len = 0;
		}

		public override void destruct()
		{
			this.clearRegistered();
			base.destruct();
		}

		public override void clear()
		{
			this.fineColor(true);
			this.finePos(true);
			this.fineScale(true);
			base.clear();
		}

		public override TransEffecterItem Create()
		{
			return new TransEffecterItem(this);
		}

		private TransEffecterItem Pop(TEKIND kind, FnEffectRun Fn, float _x, float _y, float _z, int _time, int _saf)
		{
			TransEffecterItem transEffecterItem = base.setEffectWithSpecificFn("te", _x, _y, _z, _time, _saf, Fn) as TransEffecterItem;
			if (transEffecterItem == null)
			{
				return null;
			}
			transEffecterItem.Kind(kind);
			return transEffecterItem;
		}

		public void removeCategoryDupe(TEKIND kind, TransEffecterItem Te = null, int te_index = -1)
		{
			int num = (int)(kind / (TEKIND)10);
			if (kind < TEKIND.__CANNOT_OVERRIDE)
			{
				for (int i = this.LEN - 1; i >= 0; i--)
				{
					TransEffecterItem transEffecterItem = this.AItems[i];
					if (transEffecterItem != Te && transEffecterItem.FnDef != null && transEffecterItem.kind_top == num && transEffecterItem.saf <= 0f)
					{
						transEffecterItem.FnDef = null;
					}
				}
			}
		}

		public void removeSpecific(TEKIND kind)
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				TransEffecterItem transEffecterItem = this.AItems[i];
				if (transEffecterItem.FnDef != null && transEffecterItem.kind == kind)
				{
					transEffecterItem.FnDef = null;
				}
			}
		}

		public bool existSpecific(TEKIND kind)
		{
			for (int i = this.LEN - 1; i >= 0; i--)
			{
				if (this.AItems[i].kind == kind)
				{
					return true;
				}
			}
			return false;
		}

		public FnEffectRun debugDelegate()
		{
			return (EffectItem Ef) => TransEffecterItem.fnRunDraw_quake_vib(Ef);
		}

		public TransEffecterItem setQuake(float _slevel, int _time, float _elevel = -1f, int _saf = 0)
		{
			return this.Pop(TEKIND.QUAKE_VIB, (EffectItem Ef) => TransEffecterItem.fnRunDraw_quake_vib(Ef), _slevel, _elevel, 0f, _time, _saf);
		}

		public TransEffecterItem setQuakeSinH(float _slevel, int _time, float sin_interval, float _elevel = -1f, int _saf = 0)
		{
			return this.Pop(TEKIND.QUAKE_SINH, (EffectItem Ef) => TransEffecterItem.fnRunDraw_quake_sinh(Ef), _slevel, _elevel, sin_interval, _time, _saf);
		}

		public TransEffecterItem setQuakeSinV(float _slevel, int _time, float sin_interval, float _elevel = -1f, int _saf = 0)
		{
			return this.Pop(TEKIND.QUAKE_SINV, (EffectItem Ef) => TransEffecterItem.fnRunDraw_quake_sinv(Ef), _slevel, _elevel, sin_interval, _time, _saf);
		}

		public TransEffecterItem setDmgBlink(MGATTR attr, float maxt = 0f, float mul_ratio = 1f, float add_ratio = 0f, int _saf = 0)
		{
			return this.Pop(TEKIND.DMG_BLINK, (EffectItem Ef) => TransEffecterItem.fnRunDraw_dmg_blink(Ef), mul_ratio, add_ratio, maxt, (int)attr, _saf);
		}

		public TransEffecterItem setDmgBlinkEnemy(MGATTR attr, float maxt = 0f, float mul_ratio = 1f, float add_ratio = 0f, int _saf = 0)
		{
			return this.Pop(TEKIND.DMG_BLINK_ENEMY, (EffectItem Ef) => TransEffecterItem.fnRunDraw_dmg_blink_enemy(Ef), mul_ratio, add_ratio, maxt, (int)attr, _saf);
		}

		public TransEffecterItem setDmgBlinkFading(MGATTR attr, float maxt = 0f, float mul_ratio = 1f, float add_ratio = 0f, int _saf = 0)
		{
			return this.Pop(TEKIND.DMG_BLINK_FADING, (EffectItem Ef) => TransEffecterItem.fnRunDraw_dmg_blink_fading(Ef), mul_ratio, add_ratio, maxt, (int)attr, _saf);
		}

		public TransEffecterItem setColorBlink(float fadein_t, float fadeout_t, float alpha, int color_without_alpha, int _saf = 0)
		{
			return this.Pop(TEKIND.COLOR_BLINK, (EffectItem Ef) => TransEffecterItem.fnRunDraw_color_blink(Ef), fadein_t, fadeout_t, alpha, color_without_alpha, _saf);
		}

		public TransEffecterItem setColorBlinkBush(float hold_t, float fadeout_t, float alpha, int color_without_alpha, int _saf = 0)
		{
			return this.Pop(TEKIND.COLOR_BLINK_BUSH, (EffectItem Ef) => TransEffecterItem.fnRunDraw_color_blink_bush(Ef), hold_t, fadeout_t, alpha, color_without_alpha, _saf);
		}

		public TransEffecterItem setGasColorBlink(float fadein_t, float fadeout_t, float alpha, int color_without_alpha, int _saf = 0)
		{
			return this.Pop(TEKIND.GAS_COLOR_BLINK, (EffectItem Ef) => TransEffecterItem.fnRunDraw_gas_color_blink(Ef), fadein_t, fadeout_t, alpha, color_without_alpha, _saf);
		}

		public TransEffecterItem setColorBlinkAdd(float fadein_t, float fadeout_t, float alpha, int color_without_alpha, int _saf = 0)
		{
			return this.Pop(TEKIND.COLOR_BLINK_ADD, (EffectItem Ef) => TransEffecterItem.fnRunDraw_color_blink_add(Ef), fadein_t, fadeout_t, alpha, color_without_alpha, _saf);
		}

		public TransEffecterItem setColorBlinkAddFadeout(float interval, float maxt, float alpha, int color_without_alpha, int _saf = 0)
		{
			return this.Pop(TEKIND.COLOR_BLINK_ADD_FADEOUT, (EffectItem Ef) => TransEffecterItem.fnRunDraw_color_blink_add_fadeout(Ef), interval, alpha, maxt, color_without_alpha, _saf);
		}

		public TransEffecterItem setEvadeBlink(float maxt = 0f)
		{
			return this.Pop(TEKIND.EVADE_BLINK, (EffectItem Ef) => TransEffecterItem.fnRunDraw_evade_blink(Ef), 0f, 0f, 0f, (int)maxt, 0);
		}

		public TransEffecterItem setUiDmgDarken(MGATTR attr, float maxt = 0f, float alpha = 1f)
		{
			return this.Pop(TEKIND.UI_DMG_DARKEN, (EffectItem Ef) => TransEffecterItem.fnRunDraw_ui_dmg_darken(Ef), 0f, alpha, maxt, (int)attr, 0);
		}

		public TransEffecterItem setFadeIn(float maxt = 0f, float salpha = 0f)
		{
			return this.Pop(TEKIND.FADEIN, (EffectItem Ef) => TransEffecterItem.fnRunDraw_fadein(Ef), 0f, 0f, salpha, (int)maxt, 0);
		}

		public TransEffecterItem setFadeOut(float maxt = 0f, float depalpha = 0f)
		{
			return this.Pop(TEKIND.FADEOUT, (EffectItem Ef) => TransEffecterItem.fnRunDraw_fadeout(Ef), 0f, 0f, depalpha, (int)maxt, 0);
		}

		public TransEffecterItem setFadeOut_in(float maxt = 0f, float depalpha = 0f, int saf = 0)
		{
			return this.Pop(TEKIND.FADEOUT_IN, (EffectItem Ef) => TransEffecterItem.fnRunDraw_fadeout_in(Ef), 0f, 0f, depalpha, (int)X.absMx(2f, maxt), saf);
		}

		public TransEffecterItem setColorFadeOut(float maxt = 0f, uint color_without_alpha = 0U, float alpha = 1f)
		{
			return this.Pop(TEKIND.COLOR_FADEOUT, (EffectItem Ef) => TransEffecterItem.fnRunDraw_color_fadeout(Ef), 0f, alpha, maxt, (int)(color_without_alpha & 16777215U), 0);
		}

		public TransEffecterItem setAppearFrom(AIM from_aim, float len, float maxt = 0f)
		{
			TransEffecterItem transEffecterItem = this.Pop(TEKIND.APPEAR_FROM, (EffectItem Ef) => TransEffecterItem.fnRunDraw_appear_from(Ef), (float)from_aim, len, 0f, (int)maxt, 0);
			this.need_reposit_after = true;
			return transEffecterItem;
		}

		public TransEffecterItem setDisappearPowTo(AIM to_aim, float len, float maxt = 0f)
		{
			return this.Pop(TEKIND.DISAPPEAR_POW_TO, (EffectItem Ef) => TransEffecterItem.fnRunDraw_disappear_pow_to(Ef), (float)to_aim, len, 0f, (int)maxt, 0);
		}

		public TransEffecterItem setBounceZoomIn(float mul_level, float time = 0f, int _saf = 0)
		{
			return this.Pop(TEKIND.BOUNCE_ZOOM_IN, (EffectItem Ef) => TransEffecterItem.fnRunDraw_bounce_zoom_in(Ef), mul_level, time, time, 0, _saf);
		}

		public TransEffecterItem setBounceZoomIn(float mul_level, float fadein_time, float fadeout_time, int _saf)
		{
			return this.Pop(TEKIND.BOUNCE_ZOOM_IN, (EffectItem Ef) => TransEffecterItem.fnRunDraw_bounce_zoom_in(Ef), mul_level, fadeout_time, fadein_time, 0, _saf);
		}

		public TransEffecterItem setBounceZoomIn2(float mul_level, float fadein_time, float fadeout_time, int _saf)
		{
			return this.Pop(TEKIND.BOUNCE_ZOOM_IN2, (EffectItem Ef) => TransEffecterItem.fnRunDraw_bounce_zoom_in2(Ef), mul_level, fadeout_time, fadein_time, 0, _saf);
		}

		public TransEffecterItem setBounceZoomInFix(float mul_level, float fadein_time, float fadeout_time, int _saf)
		{
			return this.Pop(TEKIND.BOUNCE_ZOOM_IN_FIX, (EffectItem Ef) => TransEffecterItem.fnRunDraw_bounce_zoom_in_fix(Ef), mul_level, fadeout_time, fadein_time, 0, _saf);
		}

		public TransEffecterItem setEnlargeBouncy(float mul_level_x, float mul_level_y, float time = 0f, int _saf = 0)
		{
			return this.Pop(TEKIND.ENLARGE_BOUNCY, (EffectItem Ef) => TransEffecterItem.fnRunDraw_enlarge_bouncy(Ef), mul_level_x, mul_level_y, time, 0, _saf);
		}

		public TransEffecterItem setEnlargeAbsorbed(float mul_level_x, float mul_level_y, float time = 0f, int _saf = 0)
		{
			return this.Pop(TEKIND.ABSORB_BOUNCY, (EffectItem Ef) => TransEffecterItem.fnRunDraw_absorb_bouncy(Ef), mul_level_x, mul_level_y, time, 0, _saf);
		}

		public TransEffecterItem setEnlargeTransform(float mul_level_x, float mul_level_y, float in_time = 0f, float out_t = 0f, int _saf = 0)
		{
			return this.Pop(TEKIND.ENLARGE_TRANSFORM, (EffectItem Ef) => TransEffecterItem.fnRunDraw_enlarge_transform(Ef), mul_level_x, mul_level_y, in_time, (int)out_t, _saf);
		}

		public TransEffecterItem setInitCarryBouncy(float pixel_y, int time = 0, int _saf = 0)
		{
			return this.Pop(TEKIND.INITCARRY_BOUNCE, (EffectItem Ef) => TransEffecterItem.fnRunDraw_initcarry_bounce(Ef), 0f, pixel_y, 0f, time, _saf);
		}

		public override void runDraw(float fcnt = 1f, bool runsetter = true)
		{
			if (this.col_changed)
			{
				this.fineColor(true);
			}
			if (this.pos_changed)
			{
				this.finePos(true);
			}
			if (this.scl_changed)
			{
				this.fineScale(true);
			}
			if (this.isActive())
			{
				for (int i = 0; i < this.LEN; i++)
				{
					TransEffecterItem transEffecterItem = this.AItems[i];
					if (transEffecterItem.FnDef != null && transEffecterItem.saf <= 0f && transEffecterItem.af < 0f)
					{
						transEffecterItem.af = -1f - transEffecterItem.af;
						this.removeCategoryDupe(transEffecterItem.kind, transEffecterItem, i);
					}
				}
				TransEffecterItem.TE = this;
				base.runDraw(fcnt, runsetter);
				TransEffecterItem.TE = null;
				if (this.col_changed || this.alpha_changed)
				{
					this.fineColor(false);
				}
				if (this.pos_changed)
				{
					this.finePos(false);
				}
				if (this.scl_changed)
				{
					this.fineScale(false);
					return;
				}
			}
			else if (this.alpha_changed)
			{
				this.fineColor(false);
			}
		}

		public void checkRepositImmediate()
		{
			if (this.need_reposit_after)
			{
				this.need_reposit_after = false;
				this.runDraw(0f, false);
			}
		}

		public void fineColor(bool resetting = false)
		{
			if (resetting)
			{
				this.ColMul.Set(uint.MaxValue);
				this.ColAdd.Set(0);
			}
			else
			{
				this.alpha_changed = false;
			}
			if (this.base_alpha_ < 1f)
			{
				this.ColMul.mulA(this.base_alpha_);
				this.ColAdd.mulA(this.base_alpha_);
			}
			for (int i = this.col_len - 1; i >= 0; i--)
			{
				if (!this.ACol[i].Set(this.mesh_color, this.ColMul, this.ColAdd))
				{
					TransEffecter.TeColorize[] acol = this.ACol;
					int num = 0;
					int num2 = i;
					int num3 = this.col_len;
					this.col_len = num3 - 1;
					X.shiftEmpty<TransEffecter.TeColorize>(acol, num, num2, num3);
				}
			}
			this.col_changed = !resetting;
		}

		public void finePos(bool resetting = false)
		{
			if (resetting)
			{
				this.PosTranslate.Set(0f, 0f, 0f);
			}
			for (int i = this.pos_len - 1; i >= 0; i--)
			{
				if (!this.APos[i].Set(this.PosTranslate))
				{
					TransEffecter.TePosit[] apos = this.APos;
					int num = 0;
					int num2 = i;
					int num3 = this.pos_len;
					this.pos_len = num3 - 1;
					X.shiftEmpty<TransEffecter.TePosit>(apos, num, num2, num3);
				}
			}
			this.pos_changed = !resetting;
		}

		public void fineScale(bool resetting = false)
		{
			if (resetting)
			{
				this.PosScale.Set(1f, 1f);
			}
			for (int i = this.scl_len - 1; i >= 0; i--)
			{
				if (!this.AScl[i].Set(this.PosScale))
				{
					TransEffecter.TeScaler[] ascl = this.AScl;
					int num = 0;
					int num2 = i;
					int num3 = this.scl_len;
					this.scl_len = num3 - 1;
					X.shiftEmpty<TransEffecter.TeScaler>(ascl, num, num2, num3);
				}
			}
			this.scl_changed = !resetting;
		}

		public void mulColor(C32 Col, bool apply_to_alpha = false)
		{
			this.ColMul.multiply(Col.C, apply_to_alpha);
			this.col_changed = true;
		}

		public void addColor(C32 Col, bool apply_to_alpha = false)
		{
			this.ColAdd.add(Col.C, apply_to_alpha, 1f);
			this.col_changed = true;
		}

		public void translatePos(float x, float y)
		{
			this.PosTranslate.x = this.PosTranslate.x + x;
			this.PosTranslate.y = this.PosTranslate.y + y;
			this.pos_changed = true;
		}

		public void scaleMul(float lv)
		{
			this.PosScale.x = this.PosScale.x * lv;
			this.PosScale.y = this.PosScale.y * lv;
			this.scl_changed = true;
		}

		public void scaleMul(float x, float y)
		{
			this.PosScale.x = this.PosScale.x * x;
			this.PosScale.y = this.PosScale.y * y;
			this.scl_changed = true;
		}

		public float base_alpha
		{
			get
			{
				return this.base_alpha_;
			}
			set
			{
				if (value != this.base_alpha_)
				{
					this.base_alpha_ = value;
					this.alpha_changed = true;
				}
			}
		}

		public const int KIND_TOP_DIDIVE = 10;

		private TransEffecter.TeColorize[] ACol;

		private int col_len;

		private TransEffecter.TePosit[] APos;

		private int pos_len;

		private TransEffecter.TeScaler[] AScl;

		private int scl_len;

		private readonly C32 ColMul;

		private readonly C32 ColAdd;

		private Vector3 PosTranslate;

		private Vector2 PosScale;

		private bool pos_changed;

		private bool col_changed;

		private bool scl_changed;

		private bool alpha_changed;

		public bool need_reposit_after;

		private float base_alpha_ = 1f;

		public bool is_player;

		private struct TeColorize
		{
			public bool Set(C32 Buf, C32 CMul, C32 CAdd)
			{
				try
				{
					Buf.Set(this.ColDef).multiply(CMul.C, true).add(CAdd.C, true, this.add_apply_level);
					if (this.Colr != null)
					{
						this.Colr.setColorTe(Buf, CMul, CAdd);
					}
					if (this.Spr != null)
					{
						this.Spr.color = Buf.C;
					}
					if (this.Md != null)
					{
						this.Md.setColorTe(Buf, CMul, CAdd);
					}
				}
				catch (Exception)
				{
					return false;
				}
				return true;
			}

			public SpriteRenderer Spr;

			public MeshDrawer Md;

			public ITeColor Colr;

			public Color32 ColDef;

			public float add_apply_level;
		}

		private struct TePosit
		{
			public bool Set(Vector3 PosT)
			{
				try
				{
					if (this.Trs != null)
					{
						PosT = PosT * 0.015625f + this.PosDef;
						this.Trs.localPosition = PosT;
					}
					if (this.Shifter != null)
					{
						PosT += this.PosDef;
						this.Shifter.setShiftTe(PosT);
					}
				}
				catch (Exception)
				{
					return false;
				}
				return true;
			}

			public ITeShift Shifter;

			public Transform Trs;

			public Vector3 PosDef;
		}

		private struct TeScaler
		{
			public bool Set(Vector3 PosT)
			{
				try
				{
					PosT *= this.SclDef;
					if (this.Trs != null)
					{
						Vector3 localScale = this.Trs.localScale;
						localScale.x = PosT.x;
						localScale.y = PosT.y;
						this.Trs.localScale = localScale;
					}
					if (this.Scaler != null)
					{
						this.Scaler.setScaleTe(PosT);
					}
				}
				catch (Exception)
				{
					return false;
				}
				return true;
			}

			public ITeScaler Scaler;

			public Transform Trs;

			public Vector2 SclDef;
		}
	}
}
