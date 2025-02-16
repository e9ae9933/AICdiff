using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Better;
using PixelLiner;
using UnityEngine;

namespace XX
{
	public sealed class EfParticle : EfParticleVarContainer
	{
		public override string key
		{
			get
			{
				return this.__key;
			}
			set
			{
				this.__key = value;
			}
		}

		public string rep_z
		{
			get
			{
				return this.rep_z__;
			}
			set
			{
				this.rep_z__ = ((value != "") ? value : null);
				if (this.rep_z__ != null && !this.inputValueProp(this.rep_z__, 0f))
				{
					X.de("不明な z 指定子: " + this.rep_z__, null);
					this.rep_z__ = null;
				}
			}
		}

		public string rep_time
		{
			get
			{
				return this.rep_time__;
			}
			set
			{
				this.rep_time__ = ((value != "") ? value : null);
				if (this.rep_time__ != null && !this.inputValuePropInt(this.rep_time__, 0))
				{
					X.de("不明な time 指定子: " + this.rep_time__, null);
					this.rep_time__ = null;
				}
			}
		}

		public static void initEfParticle()
		{
			if (EfParticle.AinitFn != null)
			{
				return;
			}
			EfParticle.Otypes = new BDic<string, int>();
			EfParticle.AinitFn = new FnPtcInit[0];
			EfParticle.AdrawFn = new FnPtcDraw[0];
			EfParticle.OexpandsFn = new BDic<string, FnPtcExpand>();
			EfParticle.OexpandsFn["daia"] = new FnPtcExpand(EfParticle.fnExpandDaia);
			EfParticle.OexpandsFn["square"] = new FnPtcExpand(EfParticle.fnExpandSquare);
			EfParticle.col_buf_s = new C32();
			EfParticle.col_buf_e = new C32();
		}

		public static bool assignType(string type_name, FnPtcInit fnInit = null, FnPtcDraw fnDraw = null)
		{
			if (EfParticle.AinitFn == null)
			{
				EfParticle.initEfParticle();
			}
			if (EfParticle.Otypes.ContainsKey(type_name))
			{
				X.de("EfParticle::assignType - 定義済み type " + type_name, null);
				return false;
			}
			EfParticle.Otypes[type_name] = EfParticle.AinitFn.Length;
			X.push<FnPtcInit>(ref EfParticle.AinitFn, fnInit, -1);
			X.push<FnPtcDraw>(ref EfParticle.AdrawFn, fnDraw, -1);
			return true;
		}

		public static int getTypeId(string type_name)
		{
			return X.Get<string, int>(EfParticle.Otypes, type_name, -1000);
		}

		public static bool assignExpandType(string type_name, FnPtcExpand fnExpand = null)
		{
			if (EfParticle.AinitFn == null)
			{
				EfParticle.initEfParticle();
			}
			EfParticle.OexpandsFn[type_name] = fnExpand;
			return true;
		}

		public EfParticle(EfParticleVarContainer _Loader, string _key)
		{
			this.id = ++EfParticle.particle_id;
			this.col_s0 = new C32(uint.MaxValue);
			this.col_s1 = new C32(uint.MaxValue);
			this.col_e0 = new C32(uint.MaxValue);
			this.col_e1 = new C32(uint.MaxValue);
			this.Loader = _Loader;
			this.key = _key;
		}

		public EfParticle clone()
		{
			EfParticle efParticle = new EfParticle(this, this.key);
			efParticle.initParticle();
			return efParticle;
		}

		public override EfParticleVarContainer initParticle()
		{
			if (this.Loader == null)
			{
				return this;
			}
			EfParticleVarContainer efParticleVarContainer = this.Loader.initParticle();
			this.Loader = null;
			this.SetV(efParticleVarContainer, "type", -1);
			this.SetV0D(efParticleVarContainer, "layer", 0);
			float num = 0f;
			int num2 = this.type;
			if (num2 != -1000)
			{
				switch (num2)
				{
				case -33:
					this.initColor(efParticleVarContainer);
					this.SetV0D(efParticleVarContainer, "line_len_min", 1);
					this.SetV0D(efParticleVarContainer, "line_len_max", this.line_len_min);
					this.line_len_dif = this.line_len_max - this.line_len_min;
					this.defineZmFunc(efParticleVarContainer, "line_ex_type", "Z1");
					this.SetV0D(efParticleVarContainer, "attr", 0);
					break;
				case -32:
				case -31:
				case -29:
					this.initColor(efParticleVarContainer);
					this.SetV0D(efParticleVarContainer, "line_len_min", 1);
					this.SetV0D(efParticleVarContainer, "line_len_max", this.line_len_min);
					this.line_len_dif = this.line_len_max - this.line_len_min;
					this.defineZmFunc(efParticleVarContainer, "line_ex_type", "Z1");
					this.SetV0D(efParticleVarContainer, "attr", 1600);
					break;
				case -30:
					this.initColor(efParticleVarContainer);
					this.SetV0D(efParticleVarContainer, "attr", 1600);
					this.SetV0D(efParticleVarContainer, "line_len_min", 1);
					this.SetV0D(efParticleVarContainer, "line_len_max", this.line_len_min);
					this.line_len_dif = this.line_len_max - this.line_len_min;
					this.defineZmFunc(efParticleVarContainer, "line_ex_type", "Z1");
					break;
				case -28:
				case -27:
				case -26:
				case -23:
				case -21:
				case -20:
					this.initColor(efParticleVarContainer);
					this.SetV0D(efParticleVarContainer, "attr", 5);
					this.SetV0D(efParticleVarContainer, "line_len_min", 1);
					this.SetV0D(efParticleVarContainer, "line_len_max", this.line_len_min);
					this.line_len_dif = this.line_len_max - this.line_len_min;
					this.SetV0D(efParticleVarContainer, "line_ex_time_min", 0);
					this.SetV0D(efParticleVarContainer, "line_ex_time_max", this.line_ex_time_min);
					this.line_ex_time_dif = this.line_ex_time_max - this.line_ex_time_min;
					this.defineZmFunc(efParticleVarContainer, "line_ex_type", "Z1");
					break;
				case -25:
					this.initColor(efParticleVarContainer);
					this.SetV0D(efParticleVarContainer, "line_len_min", 0.125f);
					this.SetV0D(efParticleVarContainer, "line_len_max", this.line_len_min);
					this.line_len_dif = this.line_len_max - this.line_len_min;
					this.defineZmFunc(efParticleVarContainer, "line_ex_type", "Z1");
					break;
				case -24:
					num = 1f;
					this.initColor(efParticleVarContainer);
					this.SetV0D(efParticleVarContainer, "attr", 4);
					this.SetV0D(efParticleVarContainer, "line_len_min", 0.125f);
					this.SetV0D(efParticleVarContainer, "line_len_max", this.line_len_min);
					this.line_len_dif = this.line_len_max - this.line_len_min;
					this.defineZmFunc(efParticleVarContainer, "line_ex_type", "Z1");
					break;
				case -22:
					this.initColor(efParticleVarContainer);
					this.SetV0D(efParticleVarContainer, "attr", 0);
					this.SetV0D(efParticleVarContainer, "line_len_min", 0);
					this.SetV0D(efParticleVarContainer, "line_len_max", this.line_len_min);
					this.line_len_dif = this.line_len_max - this.line_len_min;
					this.defineZmFunc(efParticleVarContainer, "line_ex_type", "Z1");
					break;
				case -19:
					this.initColor(efParticleVarContainer);
					this.SetV0D(efParticleVarContainer, "line_len_min", 1);
					this.SetV0D(efParticleVarContainer, "line_len_max", this.line_len_min);
					this.line_len_dif = this.line_len_max - this.line_len_min;
					this.defineZmFunc(efParticleVarContainer, "line_ex_type", "Z1");
					break;
				case -18:
				case -17:
					this.initColor(efParticleVarContainer);
					this.SetV0D(efParticleVarContainer, "attr", 3);
					this.SetV0D(efParticleVarContainer, "line_len_min", 1);
					this.SetV0D(efParticleVarContainer, "line_len_max", this.line_len_min);
					this.line_len_dif = this.line_len_max - this.line_len_min;
					this.SetV0D(efParticleVarContainer, "line_ex_time_min", 0);
					this.SetV0D(efParticleVarContainer, "line_ex_time_max", this.line_ex_time_min);
					this.line_ex_time_dif = this.line_ex_time_max - this.line_ex_time_min;
					this.defineZmFunc(efParticleVarContainer, "line_ex_type", "Z1");
					break;
				case -16:
					this.initColor(efParticleVarContainer);
					this.SetV0D(efParticleVarContainer, "attr", 0);
					this.SetV0D(efParticleVarContainer, "line_len_min", 1);
					this.SetV0D(efParticleVarContainer, "line_len_max", this.line_len_min);
					this.line_len_dif = this.line_len_max - this.line_len_min;
					this.defineZmFunc(efParticleVarContainer, "line_ex_type", "Z1");
					break;
				case -15:
				case -14:
					this.initColor(efParticleVarContainer);
					this.BmL = ((this.type == -15) ? MTRX.SqDotKiraAnim : MTRX.SqDotKira);
					break;
				case -13:
					this.initColor(efParticleVarContainer);
					this.SetV0D(efParticleVarContainer, "line_len_min", 1);
					this.SetV0D(efParticleVarContainer, "line_len_max", this.line_len_min);
					this.line_len_dif = this.line_len_max - this.line_len_min;
					this.SetV0D(efParticleVarContainer, "line_ex_time_min", 0);
					this.SetV0D(efParticleVarContainer, "line_ex_time_max", this.line_ex_time_min);
					this.line_ex_time_dif = this.line_ex_time_max - this.line_ex_time_min;
					this.defineZmFunc(efParticleVarContainer, "line_ex_type", "Z1");
					break;
				case -12:
				case -2:
					this.initColor(efParticleVarContainer);
					this.SetV0D(efParticleVarContainer, "line_len_min", 1);
					this.SetV0D(efParticleVarContainer, "line_len_max", this.line_len_min);
					this.line_len_dif = this.line_len_max - this.line_len_min;
					this.SetV0D(efParticleVarContainer, "line_ex_time_min", 0);
					this.SetV0D(efParticleVarContainer, "line_ex_time_max", this.line_ex_time_min);
					this.line_ex_time_dif = this.line_ex_time_max - this.line_ex_time_min;
					this.defineZmFunc(efParticleVarContainer, "line_ex_type", "ZSIN");
					break;
				case -11:
				case -10:
					this.initColor(efParticleVarContainer);
					if (!efParticleVarContainer.IsSet("attr"))
					{
						this.SetV(efParticleVarContainer, "attr", 100);
					}
					break;
				case -9:
				case -6:
				case -5:
				case -3:
					this.initColor(efParticleVarContainer);
					break;
				case -8:
					this.initColor(efParticleVarContainer);
					this.SetV0D(efParticleVarContainer, "attr", 3);
					this.attr = X.Mx(3, this.attr);
					break;
				case -7:
				case -4:
				case -1:
					this.initColor(efParticleVarContainer);
					break;
				default:
					if (EfParticle.AinitFn[this.type] != null)
					{
						EfParticle.AinitFn[this.type](this, efParticleVarContainer);
					}
					else
					{
						this.initColor(efParticleVarContainer);
					}
					break;
				}
			}
			if (efParticleVarContainer.IsSet("Fn_expand"))
			{
				this.Fn_expand = (FnPtcExpand)efParticleVarContainer.GetV("Fn_expand", null);
			}
			else if (efParticleVarContainer.IsSet("expand"))
			{
				string text = efParticleVarContainer.GetV("expand", "").ToString();
				this.Fn_expand = X.Get<string, FnPtcExpand>(EfParticle.OexpandsFn, text);
			}
			this.rep_z = efParticleVarContainer.GetV("z", "").ToString();
			this.rep_time = efParticleVarContainer.GetV("time", "").ToString();
			this.SetV(efParticleVarContainer, "layer", 0);
			this.SetV(efParticleVarContainer, "layer_key", "");
			this.SetV(efParticleVarContainer, "time_lock_factor", 0);
			this.SetV0D(efParticleVarContainer, "count", 1);
			this.SetV0D(efParticleVarContainer, "maxt", 1);
			this.SetV0D(efParticleVarContainer, "attr", this.attr);
			this.SetV(efParticleVarContainer, "delay", 0);
			this.SetV(efParticleVarContainer, "t_fast_hrhb", 0);
			float num3 = X.Nm(efParticleVarContainer.GetV("df_min", 0).ToString(), 0f, false);
			float num4 = X.Nm(efParticleVarContainer.GetV0D("df_max", 0, true).ToString(), 0f, false);
			this.SetV(efParticleVarContainer, "xdf_min", num3);
			this.SetV(efParticleVarContainer, "ydf_min", num3);
			this.SetV(efParticleVarContainer, "zdf_min", 0);
			this.SetV0D(efParticleVarContainer, "xdf_max", num4);
			if (this.xdf_max == 0f)
			{
				this.xdf_max = this.xdf_min;
			}
			this.xdf_dif = this.xdf_max - this.xdf_min;
			this.SetV0D(efParticleVarContainer, "ydf_max", num4);
			if (this.ydf_max == 0f)
			{
				this.ydf_max = this.ydf_min;
			}
			this.ydf_dif = this.ydf_max - this.ydf_min;
			if (this.IsSet("df_anti"))
			{
				this.xdf_anti = (this.ydf_anti = (float)efParticleVarContainer.GetV("df_anti", 50f));
			}
			this.SetV(efParticleVarContainer, "xdf_anti", this.xdf_anti);
			this.SetV(efParticleVarContainer, "ydf_anti", this.ydf_anti);
			this.SetV0D(efParticleVarContainer, "zdf_max", this.zdf_min);
			this.zdf_dif = this.zdf_max - this.zdf_min;
			float num5 = (float)efParticleVarContainer.GetV("df_i", 0f);
			this.SetV0D(efParticleVarContainer, "xdf_i", num5);
			this.SetV0D(efParticleVarContainer, "ydf_i", num5);
			this.SetV(efParticleVarContainer, "zdf_i", 0);
			this.defineZmFunc(efParticleVarContainer, "df_i_type", "ZLINE");
			bool flag;
			if (efParticleVarContainer is EfParticle)
			{
				flag = (bool)efParticleVarContainer.GetV("dirR_defined", false);
			}
			else
			{
				flag = efParticleVarContainer.IsSet("dirR") || efParticleVarContainer.IsSet("dir_base_s");
			}
			if (flag)
			{
				this.dirR_defined = true;
				this.SetV(efParticleVarContainer, "dirR", 0);
				this.SetV(efParticleVarContainer, "dirR_hrhb", 0);
				this.SetV0D(efParticleVarContainer, "dir_base_s", this.dir_base_s);
			}
			this.SetV(efParticleVarContainer, "mv_min", 0);
			this.SetV0D(efParticleVarContainer, "mv_max", this.mv_min);
			this.mv_dif = this.mv_max - this.mv_min;
			this.defineZmFunc(efParticleVarContainer, "mv_type", "ZLINE");
			this.SetV(efParticleVarContainer, "slen_min", 0);
			this.SetV0D(efParticleVarContainer, "slen_max", this.slen_min);
			this.slen_dif = this.slen_max - this.slen_min;
			this.SetV(efParticleVarContainer, "slen_agR", 0);
			this.SetV(efParticleVarContainer, "slen_hrhb_min_agR", 0);
			this.SetV(efParticleVarContainer, "slen_hrhb_agR", 6.2831855f);
			float num6 = (float)efParticleVarContainer.GetV("slen_i", 0f);
			this.SetV0D(efParticleVarContainer, "slen_i_min", num6);
			this.SetV0D(efParticleVarContainer, "slen_i_max", this.slen_i_min);
			this.slen_i_dif = this.slen_i_max - this.slen_i_min;
			float num7 = (float)efParticleVarContainer.GetV("slen_agR_i", 0f);
			this.SetV0D(efParticleVarContainer, "slen_agR_i_min", num7);
			this.SetV0D(efParticleVarContainer, "slen_agR_i_max", this.slen_agR_i_min);
			this.slen_agR_i_dif = this.slen_agR_i_max - this.slen_agR_i_min;
			this.SetV(efParticleVarContainer, "ex_min", 0);
			this.SetV0D(efParticleVarContainer, "ex_max", this.ex_min);
			this.ex_dif = this.ex_max - this.ex_min;
			this.SetV(efParticleVarContainer, "ex_y_ratio", 1);
			this.SetV(efParticleVarContainer, "exagR_min", 0);
			this.SetV0D(efParticleVarContainer, "exagR_max", this.exagR_min);
			this.exagR_dif = this.exagR_max - this.exagR_min;
			this.defineZmFunc(efParticleVarContainer, "ex_type", "ZLINE");
			this.defineZmFunc(efParticleVarContainer, "exagR_type", "ZLINE");
			this.SetV(efParticleVarContainer, "exagR_lvl", 1);
			this.SetV(efParticleVarContainer, "bezier_min", 0);
			this.SetV0D(efParticleVarContainer, "bezier_max", this.bezier_min);
			this.bezier_dif = this.bezier_max - this.bezier_min;
			this.SetV(efParticleVarContainer, "bezier_center_min", 0);
			this.SetV0D(efParticleVarContainer, "bezier_center_max", this.bezier_center_min);
			this.bezier_center_dif = this.bezier_center_max - this.bezier_center_min;
			this.SetV(efParticleVarContainer, "zex_min", 0);
			this.SetV0D(efParticleVarContainer, "zex_max", this.zex_min);
			this.zex_dif = this.zex_max - this.zex_min;
			this.defineZmFunc(efParticleVarContainer, "zex_type", "ZLINE");
			this.SetV(efParticleVarContainer, "zm_min", 1);
			this.SetV0D(efParticleVarContainer, "zm_max", this.zm_min);
			this.defineZmFunc(efParticleVarContainer, "zm_type", "Z1");
			this.zm_dif = this.zm_max - this.zm_min;
			this.defineZmFunc(efParticleVarContainer, "zex_type", "Z1");
			this.SetV(efParticleVarContainer, "mesh_scale_y_min", 100);
			this.SetV(efParticleVarContainer, "mesh_scale_y_max", this.mesh_scale_y_min);
			this.mesh_scale_y_dif = this.mesh_scale_y_max - this.mesh_scale_y_min;
			this.defineZmFunc(efParticleVarContainer, "mesh_scale_y_type", "Z1");
			this.SetV(efParticleVarContainer, "mesh_rot_agR_min", 0);
			this.SetV(efParticleVarContainer, "mesh_rot_agR_max", this.mesh_rot_agR_min);
			this.mesh_rot_agR_dif = this.mesh_rot_agR_max - this.mesh_rot_agR_min;
			this.SetV(efParticleVarContainer, "mesh_rot_agR_i", 0);
			this.defineZmFunc(efParticleVarContainer, "mesh_rot_agR_type", "Z1");
			this.SetV0D(efParticleVarContainer, "mesh_rot_agR_base_s", this.mesh_rot_agR_base_s);
			this.SetV(efParticleVarContainer, "mesh_translate_min", 0);
			this.SetV(efParticleVarContainer, "mesh_translate_max", this.mesh_translate_min);
			this.mesh_translate_dif = this.mesh_translate_max - this.mesh_translate_min;
			this.defineZmFunc(efParticleVarContainer, "mesh_translate_type", "Z1");
			this.SetV(efParticleVarContainer, "mesh_translate_dirR", 0);
			this.SetV(efParticleVarContainer, "mesh_translate_dirR_hrhb", 0);
			this.SetV(efParticleVarContainer, "zmadd_min", 0);
			this.SetV0D(efParticleVarContainer, "zmadd_max", this.zmadd_min);
			this.zmadd_dif = this.zmadd_max - this.zmadd_min;
			this.SetV(efParticleVarContainer, "thick_min", num);
			this.SetV0D(efParticleVarContainer, "thick_max", this.thick_min);
			this.thick_dif = this.thick_max - this.thick_min;
			this.defineZmFunc(efParticleVarContainer, "thick_type", "Z1");
			this.SetV(efParticleVarContainer, "draw_sagR_min", 0);
			this.SetV0D(efParticleVarContainer, "draw_sagR_max", this.draw_sagR_min);
			this.SetV(efParticleVarContainer, "draw_eagR_min", 0);
			this.SetV0D(efParticleVarContainer, "draw_eagR_max", this.draw_eagR_min);
			this.SetV(efParticleVarContainer, "draw_sagR_min", 0);
			this.SetV(efParticleVarContainer, "draw_eagR_min", 0);
			this.SetV(efParticleVarContainer, "camera_check_zm_mul", this.camera_check_zm_mul);
			this.SetV(efParticleVarContainer, "camera_check_zm_add", this.camera_check_zm_add);
			this.defineZmFunc(efParticleVarContainer, "draw_agR_type", "ZLINE");
			this.SetV(efParticleVarContainer, "draw_eagR_anti", 0);
			this.SetV(efParticleVarContainer, "draw_agR_base_s", false);
			this.SetV(efParticleVarContainer, "alp_s", 0);
			this.SetV(efParticleVarContainer, "alp_e", 1);
			this.defineZmFunc(efParticleVarContainer, "alp_type", "Z1");
			if (efParticleVarContainer.IsSet("zfall_g"))
			{
				this.SetV(efParticleVarContainer, "zfall_g", 0);
				this.zfall_g = ((this.zfall_g != 0f) ? (-X.Abs(this.zfall_g)) : 0f);
				this.SetV(efParticleVarContainer, "zspd0_min", 0);
				this.SetV(efParticleVarContainer, "zspd0_max", 0);
				this.zspd0_dif = this.zspd0_max - this.zspd0_min;
				this.SetV(efParticleVarContainer, "z_bound", 0);
			}
			this.FD_EfRun = (EffectItem Ef) => this.drawMain(Ef, 0f, (this.time_lock_factor > 0) ? ((float)this.time_lock_factor / 100f) : 0f, false);
			return this;
		}

		public override object GetV(string mde, object default_value)
		{
			return this.GetV0D(mde, default_value, true);
		}

		public EfParticle SetV(EfParticleVarContainer mde, string k, object default_value)
		{
			try
			{
				if (k != null && k == "time_lock_factor")
				{
					int num = (int)mde.GetV(k, default_value);
					this.time_lock_factor = (byte)num;
				}
				else
				{
					typeof(EfParticle).GetField(k, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(this, mde.GetV(k, default_value));
				}
			}
			catch
			{
				X.de(string.Concat(new string[] { "EfParticle settingV error: ", k, " (key: ", this.key, ")" }), null);
			}
			return this;
		}

		public EfParticle SetV0D(EfParticleVarContainer mde, string k, object default_value)
		{
			typeof(EfParticle).GetField(k, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(this, mde.GetV0D(k, default_value, true));
			return this;
		}

		public override object GetV0D(string k, object default_val, bool default_on_false = true)
		{
			if (k == "z")
			{
				return this.rep_z;
			}
			if (k == "time")
			{
				return this.rep_time;
			}
			if (k == "time_lock_factor")
			{
				return (int)this.time_lock_factor;
			}
			try
			{
				return typeof(EfParticle).GetField(k, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(this);
			}
			catch
			{
				FieldInfo field = typeof(EfParticle).GetField("Fn_" + k, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null && field.GetType().FullName != "")
				{
					return field.GetValue(this);
				}
			}
			return default_val;
		}

		public static string getValType(string k)
		{
			if (k == "z" || k == "time" || k == "line_ex_type" || k == "color_type" || k == "expand" || k == "key" || k == "layer_key")
			{
				return "string";
			}
			try
			{
				FieldInfo field = typeof(EfParticle).GetField(k, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null)
				{
					Type fieldType = field.FieldType;
					if (fieldType == typeof(int) || fieldType == typeof(uint) || fieldType == typeof(byte))
					{
						return "int";
					}
					if (fieldType == typeof(float) || fieldType == typeof(double))
					{
						return "float";
					}
					if (fieldType == typeof(bool))
					{
						return "bool";
					}
					fieldType == typeof(string);
					return "string";
				}
			}
			catch
			{
			}
			try
			{
				FieldInfo field2 = typeof(EfParticle).GetField("Fn_" + k, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field2 != null && field2.GetType().FullName != "")
				{
					return "string";
				}
			}
			catch
			{
			}
			return "float";
		}

		public override bool IsSet(string k)
		{
			if (k == "key")
			{
				return true;
			}
			try
			{
				if (typeof(EfParticle).GetField(k, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
				{
					return true;
				}
			}
			catch
			{
			}
			try
			{
				if (typeof(EfParticle).GetField("Fn_" + k, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
				{
					return true;
				}
			}
			catch
			{
			}
			return false;
		}

		public EfParticle initColor(EfParticleVarContainer mde)
		{
			this.color_initted = true;
			this.SetV(mde, "gradation", "");
			this.col_s0 = (mde.IsSet("col_s0") ? this.newColorInstance("col_s0", mde) : new C32(uint.MaxValue));
			if (mde.IsSet("col_e0"))
			{
				this.col_e0 = this.newColorInstance("col_e0", mde);
				this.col_e_setted = true;
			}
			else
			{
				this.col_e0 = new C32(this.col_s0);
			}
			this.Fn_color_type = this.defineZmFunc(mde.GetV("color_type", "").ToString(), "ZLINE");
			if (mde.IsSet("col_s1"))
			{
				this.col_index_s = true;
				this.col_s1 = this.newColorInstance("col_s1", mde);
			}
			if (mde.IsSet("col_e1"))
			{
				if (!this.col_e_setted)
				{
					this.col_e0 = this.newColorInstance("col_e1", mde);
					this.col_e_setted = true;
				}
				else
				{
					this.col_index_e = true;
					this.col_e1 = this.newColorInstance("col_e1", mde);
				}
			}
			if (this.col_index_s || this.col_index_e)
			{
				this.SetV(mde, "color_resolution", this.color_resolution);
			}
			return this;
		}

		private C32 newColorInstance(string t, EfParticleVarContainer mde)
		{
			t = mde.GetV(t, uint.MaxValue).ToString();
			float num = 1f;
			for (;;)
			{
				if (REG.match(t, EfParticle.RegColor0))
				{
					t = REG.otherContext;
					num /= X.Nm(REG.R1, 1f, true);
				}
				else
				{
					if (!REG.match(t, EfParticle.RegColor1))
					{
						break;
					}
					t = REG.otherContext;
					num = X.Nm(REG.R1, 1f, true);
				}
			}
			return new C32(t).multiply(num, true);
		}

		public static float RANMUL(float v, int ran_hash)
		{
			if (v != 0f)
			{
				return v * X.RAN(EfParticle.ran, ran_hash);
			}
			return 0f;
		}

		public static float FnMUL(float v, EfParticleFuncCalc Fc)
		{
			if (v != 0f)
			{
				return v * Fc.Get(EfParticle.CurDrawing, (EfParticle.recalc_fn_value > 0) ? EfParticle.tz : (-2f));
			}
			return 0f;
		}

		public static float FnMUL(float v, EfParticleFuncCalc Fc, float tz)
		{
			if (v != 0f)
			{
				return v * Fc.Get(null, tz);
			}
			return 0f;
		}

		public bool drawMain(EffectItem E, float particle_loop = 0f, float time_lock = 0f, bool not_mesh_image_replace = false)
		{
			if (this.Fn_zex_type == null)
			{
				return false;
			}
			if (this.rep_z__ != null)
			{
				this.inputValueProp(this.rep_z__, E.z);
			}
			if (this.rep_time__ != null)
			{
				this.inputValuePropInt(this.rep_time__, E.time);
			}
			E.Md = (EfParticle.Md = null);
			EfParticle.af = E.af;
			int num = E.getParticleCount(E, this.count);
			float num2 = E.getParticleSpeed(E, this.count, (float)this.maxt);
			if (time_lock > 0f)
			{
				num = X.IntC(X.NI(num, this.count, time_lock));
				num2 = X.NI(num2, 1f, time_lock);
			}
			if (particle_loop <= 0f)
			{
				if (EfParticle.af >= (float)this.maxt * num2 + this.delay * (float)(num - 1))
				{
					return false;
				}
			}
			else
			{
				particle_loop /= (float)this.maxt;
			}
			if ((this.camera_check_zm_mul > 0f || this.camera_check_zm_add > 0f) && E.EF != null)
			{
				float num3 = this.slen_min + this.slen_dif + X.Mx(0f, this.ex_min + this.ex_dif) + X.Abs(this.mv_min + this.mv_dif) + X.Abs(this.mesh_translate_min + this.mesh_translate_dif) + this.camera_check_zm_add;
				float num4 = (this.xdf_min + this.xdf_dif) / 0.35f;
				float num5 = (this.ydf_min + this.ydf_dif) / 0.35f;
				if (this.type == -16)
				{
					num3 += (this.line_len_min + this.line_len_dif) * this.camera_check_zm_mul;
				}
				else
				{
					num3 += (this.zm_min + this.zm_dif) * this.camera_check_zm_mul;
					if (this.type == -18 || this.type == -17)
					{
						num3 += (this.line_len_min + this.line_len_dif) * this.camera_check_zm_mul;
					}
				}
				float num6 = -num3;
				float num7 = num3;
				float num8 = -num3;
				float num9 = num3;
				if (this.xdf_anti < 100f)
				{
					num7 += num4;
				}
				if (this.xdf_anti > 0f)
				{
					num6 -= num4;
				}
				if (this.ydf_anti < 100f)
				{
					num9 += num5;
				}
				if (this.ydf_anti > 0f)
				{
					num8 -= num5;
				}
				if (!E.EF.isinCameraPtc(E, num6, num8, num7, num9, this.camera_check_zm_mul, this.camera_check_zm_add))
				{
					return true;
				}
			}
			float num10 = 1f / (float)this.maxt;
			EfParticle.tz0 = EfParticle.af * num10 / num2;
			float num11 = this.delay * num10;
			EfParticle.CurDrawing = this;
			EfParticle.Md = null;
			EfParticle.cx0 = 0f;
			EfParticle.cy0 = 0f;
			FnPtcDraw fnPtcDraw = null;
			if (this.type >= 0)
			{
				fnPtcDraw = EfParticle.AdrawFn[this.type];
			}
			float num12 = 1f / (float)num;
			EfParticle.iz = 0f;
			bool flag = this.draw_sagR_min != 0f || this.draw_sagR_max != 0f || this.draw_eagR_min != 0f || this.draw_eagR_max != 0f;
			float num13 = 0f;
			float num14 = 0f;
			if (flag)
			{
				num13 = this.draw_sagR_max - this.draw_sagR_min;
				num14 = this.draw_eagR_max - this.draw_eagR_min;
			}
			float num15 = 0f;
			float num16 = (float)((this.gradation == "LINE") ? 1 : 0);
			EfParticle.recalc_fn_value = 1;
			EfParticle.i = 0;
			while (EfParticle.i < num)
			{
				EfParticle.no_draw = false;
				EfParticle.iz += num12;
				EfParticle.ran = X.GETRAN3((int)((long)(E.f0 + EfParticle.i) + (long)((ulong)(E.index * 43U)) + (long)(this.id * 39)), EfParticle.i, (int)((long)(this.id * 431) + (long)((ulong)(41785U * E.index))));
				EfParticle.tz = EfParticle.tz0;
				if (this.delay == 0f)
				{
					goto IL_0419;
				}
				EfParticle.tz -= num11 * (float)EfParticle.i;
				if (EfParticle.tz >= 0f)
				{
					goto IL_0419;
				}
				IL_232E:
				EfParticle.i++;
				continue;
				IL_0419:
				if (this.t_fast_hrhb != 0f)
				{
					EfParticle.tz = EfParticle.tz0 / (1f - EfParticle.RANMUL(this.t_fast_hrhb, 2292));
				}
				if (float.IsInfinity(EfParticle.tz))
				{
					goto IL_232E;
				}
				if (EfParticle.tz >= 1f)
				{
					if (particle_loop <= 0f)
					{
						goto IL_232E;
					}
					EfParticle.tz -= (float)X.Int(EfParticle.tz / particle_loop) * particle_loop;
					if (EfParticle.tz >= 1f)
					{
						goto IL_232E;
					}
				}
				EfParticle.exz = 0f;
				EfParticle.slen = this.slen_min + EfParticle.RANMUL(this.slen_dif, 1707) + (this.slen_i_min + EfParticle.RANMUL(this.slen_i_dif, 1696)) * EfParticle.iz;
				EfParticle.saR = (EfParticle.saR0 = this.slen_agR + (float)((EfParticle.ran % 7752U % 2U == 1U) ? 1 : (-1)) * (this.slen_hrhb_min_agR + EfParticle.RANMUL(0.5f, 1591) * this.slen_hrhb_agR) + (this.slen_agR_i_min + EfParticle.RANMUL(this.slen_agR_i_dif, 1907)) * EfParticle.iz);
				if (this.exagR_max != 0f)
				{
					EfParticle.saR += EfParticle.FnMUL(this.exagR_min + EfParticle.RANMUL(this.exagR_dif, 544), this.Fn_exagR_type) * this.exagR_lvl;
				}
				if (this.mv_max != 0f)
				{
					EfParticle.mvlen = this.mv_min + EfParticle.RANMUL(this.mv_dif, 1787);
					EfParticle.mvz = EfParticle.FnMUL(1f, this.Fn_mv_type);
				}
				EfParticle._agR = (this.draw_agR_base_s ? EfParticle.saR : 0f);
				if (this.ex_max != 0f)
				{
					EfParticle.elen = this.ex_min + EfParticle.RANMUL(this.ex_dif, 2374);
					EfParticle.exz = EfParticle.FnMUL(1f, this.Fn_ex_type);
				}
				this.calcXY(E, false, false);
				if (EfParticle.no_draw)
				{
					goto IL_232E;
				}
				EfParticle._zm = EfParticle.FnMUL(this.zm_min + EfParticle.RANMUL(this.zm_dif, 303), this.Fn_zm_type);
				if (this.zmadd_min != 0f || this.zmadd_dif != 0f)
				{
					EfParticle._zm += this.zmadd_min + EfParticle.RANMUL(this.zmadd_dif, 2110);
				}
				if (this.thick_min != 0f || this.thick_dif != 0f)
				{
					EfParticle._thick = EfParticle.FnMUL(this.thick_min + ((this.thick_dif != 0f) ? EfParticle.RANMUL(this.thick_dif, 4491) : 0f), this.Fn_thick_type);
					if (EfParticle._thick < 0f)
					{
						EfParticle._thick = X.Mx(0f, EfParticle._zm + EfParticle._thick * ((this.type == -13) ? EfParticle._zm : 1f));
						if (EfParticle._thick <= 0f)
						{
							goto IL_232E;
						}
					}
				}
				else
				{
					EfParticle._thick = 0f;
				}
				if (flag)
				{
					EfParticle._agR += ((num13 != 0f) ? (this.draw_sagR_min + X.RAN(EfParticle.ran, 2829) * num13) : this.draw_sagR_min);
					float num17 = ((num14 != 0f) ? (this.draw_eagR_min + X.RAN(EfParticle.ran, 1654) * num14) : this.draw_eagR_min);
					if (num17 != 0f)
					{
						if (this.draw_eagR_anti != 0 && EfParticle.RANMUL(100f, 799) < (float)this.draw_eagR_anti)
						{
							num17 *= -1f;
						}
						EfParticle._agR += EfParticle.FnMUL(num17, this.Fn_draw_agR_type);
					}
				}
				EfParticle.calpha = ((this.alp_s == this.alp_e) ? this.alp_s : X.MMX(0f, X.NAIBUN_I(this.alp_s, this.alp_e, EfParticle.FnMUL(1f, this.Fn_alp_type)), 1f));
				switch (this.type)
				{
				case -33:
				{
					float num18 = EfParticle.FnMUL(this.line_len_min + EfParticle.RANMUL(this.line_len_dif, 2592), this.Fn_line_ex_type);
					this.GetMesh(E, null, true);
					EfParticle.Md.ColGrd.Set(EfParticle.Md.Col).mulA((float)this.attr / 255f);
					EfParticle.Md.Arc2(0f, 0f, EfParticle._zm * num18, EfParticle._zm, EfParticle._agR - 1.5707964f, EfParticle._agR + 1.5707964f, EfParticle._thick, 0f, 1f);
					break;
				}
				case -32:
				case -31:
				{
					float num19 = EfParticle.FnMUL(this.line_len_min + EfParticle.RANMUL(this.line_len_dif, 2592), this.Fn_line_ex_type);
					this.GetMesh(E, null, true);
					float num20 = EfParticle._zm + num19;
					float zm = EfParticle._zm;
					EfParticle.Md.TranslateP(EfParticle.cx, EfParticle.cy, true).Rotate(EfParticle._agR, true);
					if (this.type == -31)
					{
						EfParticle.Md.Box(0f, 0f, num20, zm, EfParticle._thick, false);
					}
					else if (this.type == -32)
					{
						EfParticle.Md.KadomaruRect(0f, 0f, num20, zm, (float)this.attr, EfParticle._thick, false, 0f, 0f, false);
					}
					break;
				}
				case -30:
					if (EfParticle._thick > 0f)
					{
						float num21 = this.line_len_min + EfParticle.RANMUL(this.line_len_dif, 1712);
						WindDrawer windDrawer = MTRX.Wind.Set(EfParticle.ran, EfParticle._thick, num21, (float)this.attr, 1f, EfParticle._zm);
						this.GetMesh(E, null, true);
						windDrawer.Col = EfParticle.Md.Col;
						windDrawer.drawTo(EfParticle.Md, EfParticle.cx, EfParticle.cy, EfParticle._agR, EfParticle.tz);
					}
					break;
				case -29:
					if (EfParticle._thick < EfParticle._zm)
					{
						this.hold();
						this.GetMesh(E, null, true);
						float num22 = 1f * num10;
						float num23;
						if (EfParticle.tz < num22)
						{
							EfParticle.tz += num22;
							EfParticle.af += 1f;
							this.calcXY(E, true, true);
							num23 = X.GAR2(EfParticle.cxf, EfParticle.cyf, EfParticle.cx, EfParticle.cy);
							EfParticle.af -= 1f;
						}
						else
						{
							EfParticle.tz -= num22;
							EfParticle.af -= 1f;
							this.calcXY(E, true, true);
							num23 = X.GAR2(EfParticle.cx, EfParticle.cy, EfParticle.cxf, EfParticle.cyf);
							EfParticle.af += 1f;
						}
						float num24 = this.line_len_min + EfParticle.RANMUL(this.line_len_dif, 1712);
						EfParticle.Md.TranslateP(EfParticle.cxf, EfParticle.cyf, true).Rotate(num23, true).Scale(1f, num24, true);
						if (EfParticle._thick <= 0f)
						{
							EfParticle.Md.Circle(0f, 0f, EfParticle._zm, 0f, false, num15, num16);
						}
						else
						{
							EfParticle.Md.CircleB(0f, 0f, EfParticle._zm, 0f, 0f, (EfParticle._zm - EfParticle._thick) / EfParticle._zm, num15, num16);
						}
					}
					break;
				case -28:
				case -27:
				case -26:
				{
					ShockRippleDrawer shockRipple = MTRX.ShockRipple;
					this.GetMesh(E, shockRipple.TargetMI ?? MTRX.MIicon, true);
					if (this.gradation != "LINE")
					{
						EfParticle.Md.ColGrd.Set(EfParticle.Md.Col).setA(0f);
					}
					shockRipple.DivideCount(this.attr);
					shockRipple.thick_randomize = EfParticle.FnMUL(this.line_len_min + EfParticle.RANMUL(this.line_len_dif, 2592), this.Fn_line_ex_type);
					shockRipple.radius_randomize_px = this.line_ex_time_min + EfParticle.RANMUL(this.line_ex_time_dif, 2877);
					float num25 = (float)((this.type == -28) ? 1 : 0);
					float num26 = (float)((this.type == -28) ? 0 : 1);
					shockRipple.texture_h_scale = X.Mx(0.25f, EfParticle._agR);
					shockRipple.gradation_focus = ((this.type == -26) ? 0.66f : 0f);
					shockRipple.drawTo(EfParticle.Md, EfParticle.cx, EfParticle.cy, EfParticle._zm, X.Mn(EfParticle._zm * 2f, EfParticle._thick), num26, num25);
					break;
				}
				case -25:
				{
					this.GetMesh(E, null, true);
					if (this.gradation != "LINE")
					{
						EfParticle.Md.ColGrd.Set(EfParticle.Md.Col).setA(0f);
					}
					float num27 = EfParticle._thick / 2f;
					float num28 = EfParticle.FnMUL(this.line_len_min + EfParticle.RANMUL(this.line_len_dif, 2592), this.Fn_line_ex_type);
					if (EfParticle.i == 0 && EfParticle._zm >= 1f)
					{
						EfParticle.Md.Circle(EfParticle.cx, EfParticle.cy, EfParticle._zm, 0f, false, 1f, 0f);
					}
					EfParticle.Md.Tri(0, 1, 2, false);
					EfParticle.Md.PosD(EfParticle.cx, EfParticle.cy, null).PosD(EfParticle.cx + num28 * X.Cos(EfParticle._agR + num27), EfParticle.cy + num28 * X.Sin(EfParticle._agR + num27), EfParticle.Md.ColGrd).PosD(EfParticle.cx + num28 * X.Cos(EfParticle._agR - num27), EfParticle.cy + num28 * X.Sin(EfParticle._agR - num27), EfParticle.Md.ColGrd);
					break;
				}
				case -24:
				{
					float num29 = EfParticle.FnMUL(this.line_len_min + EfParticle.RANMUL(this.line_len_dif, 1712), this.Fn_line_ex_type);
					UniKiraDrawer uniKiraDrawer = MTRX.UniKira.Kira(this.attr).Dent(num29, -1f).Radius(EfParticle._zm, -1f);
					this.GetMesh(E, null, true);
					EfParticle.Md.TranslateP(EfParticle.cx, EfParticle.cy, true).Scale(EfParticle._thick, 1f, true);
					uniKiraDrawer.drawTo(EfParticle.Md, 0f, 0f, EfParticle._agR, true, 0f, 0f);
					break;
				}
				case -23:
				{
					float num30 = EfParticle.FnMUL(this.line_len_min + EfParticle.RANMUL(this.line_len_dif, 1712), this.Fn_line_ex_type);
					this.GetMesh(E, null, true);
					EfParticle.Md.CircleB(EfParticle.cx, EfParticle.cy, EfParticle._zm, X.Cos(EfParticle._agR) * EfParticle._zm * num30, X.Sin(EfParticle._agR) * EfParticle._zm * num30, EfParticle._thick, 0f, 0f);
					break;
				}
				case -22:
				{
					float num31 = EfParticle.FnMUL(this.line_len_min + EfParticle.RANMUL(this.line_len_dif, 1712), this.Fn_line_ex_type);
					this.GetMesh(E, null, true);
					float num32 = X.Cos(EfParticle._agR) * EfParticle._zm / 2f;
					float num33 = X.Sin(EfParticle._agR) * EfParticle._zm / 2f;
					float num34 = (float)(((this.attr & 1) == 0) ? 1 : 0);
					float num35 = (float)(((this.attr & 2) == 0) ? 1 : 0);
					if (num31 > 0f)
					{
						EfParticle.Md.Line(EfParticle.cx - num32, EfParticle.cy - num33 * num35, EfParticle.cx - num32 * num31, EfParticle.cy - num33 * num31 * num35, EfParticle._thick, false, 0f, 0f);
						EfParticle.Md.Line(EfParticle.cx + num32, EfParticle.cy + num33 * num34, EfParticle.cx + num32 * num31, EfParticle.cy + num33 * num31 * num34, EfParticle._thick, false, 0f, 0f);
					}
					else
					{
						EfParticle.Md.Line(EfParticle.cx - num32, EfParticle.cy - num33 * num35, EfParticle.cx + num32, EfParticle.cy + num33 * num34, EfParticle._thick, false, 0f, 0f);
					}
					break;
				}
				case -21:
				{
					float num36 = EfParticle.FnMUL(this.line_len_min + EfParticle.RANMUL(this.line_len_dif, 1712), this.Fn_line_ex_type);
					ElecDrawer elec = MTRX.Elec;
					this.GetMesh(E, null, true);
					EfParticle.Md.ColGrd.Set(EfParticle.Md.Col).setA1(0f);
					elec.Ran(EfParticle.ran).BallRadius(3f, 2f).Thick(EfParticle._thick * 0.5f, EfParticle._thick)
						.JumpRatio(0.3f)
						.JumpHeight(EfParticle._zm * 0.66f, EfParticle._zm)
						.DivideWidth((float)this.attr)
						.finePos(EfParticle.cx, EfParticle.cy, EfParticle.cx + X.Cos(EfParticle._agR) * num36, EfParticle.cy + X.Sin(EfParticle._agR) * num36)
						.drawTz(EfParticle.Md, EfParticle.tz, true);
					break;
				}
				case -20:
				case -12:
				case -2:
				{
					float num37 = this.line_len_min + EfParticle.RANMUL(this.line_len_dif, 1712);
					int num38 = (int)(this.line_ex_time_min + EfParticle.RANMUL(this.line_ex_time_dif, 2449));
					if (num38 == 0)
					{
						num38 = (int)((float)this.maxt * 0.35f);
					}
					if (num38 < 0)
					{
						num38 = this.maxt;
					}
					EfParticle._thick = X.Mx(1f, EfParticle._thick);
					if (this.type == -2 || this.type == -20)
					{
						if (this.ex_reverse == -1)
						{
							this.ex_reverse = ((this.Fn_ex_type.Get(null, 0f) < this.Fn_ex_type.Get(null, 1f)) ? 1 : 0);
						}
						if (this.mv_reverse == -1)
						{
							this.mv_reverse = ((this.Fn_mv_type.Get(null, 0f) < this.Fn_mv_type.Get(null, 1f)) ? 1 : 0);
						}
						float num39;
						bool flag2;
						float num40;
						float num41;
						if (this.ex_max != 0f)
						{
							num39 = EfParticle.exz;
							flag2 = this.ex_reverse > 0;
							num40 = EfParticle.elen;
							num41 = EfParticle.exz;
						}
						else
						{
							if (this.mv_max == 0f)
							{
								EfParticle.i = num;
								break;
							}
							num39 = EfParticle.mvz;
							flag2 = this.mv_reverse > 0;
							num40 = EfParticle.mvlen;
							num41 = EfParticle.mvz;
						}
						float num42 = X.MMX(0f, num41 + EfParticle.FnMUL(num37 / num40 * (float)(flag2 ? (-1) : 1), this.Fn_line_ex_type, ((particle_loop != 0f) ? (EfParticle.tz * (float)this.maxt) : (EfParticle.tz * (float)num38)) / (float)num38), 1f);
						this.GetMesh(E, null, true);
						if (this.type == -20)
						{
							if (this.ex_max != 0f)
							{
								EfParticle.exz = num42;
							}
							else if (this.mv_max != 0f)
							{
								EfParticle.mvz = num42;
							}
							this.hold().calcXY(E, false, true);
							EfParticle.Md.ColGrd.Set(EfParticle.Md.Col).setA1(0f);
							MTRX.Elec.Ran(EfParticle.ran).BallRadius(3f, 2f).Thick(EfParticle._thick * 0.75f, EfParticle._thick)
								.JumpRatio(0.3f)
								.JumpHeight(EfParticle._zm * 0.45f, EfParticle._zm)
								.DivideWidth((float)this.attr)
								.finePos(EfParticle.cxf, EfParticle.cyf, EfParticle.cx, EfParticle.cy)
								.drawTz(EfParticle.Md, EfParticle.tz, true);
						}
						else if (this.bezier_max == 0f)
						{
							if (this.ex_max != 0f)
							{
								EfParticle.exz = num42;
							}
							else if (this.mv_max != 0f)
							{
								EfParticle.mvz = num42;
							}
							this.hold().calcXY(E, false, true);
							EfParticle.Md.Line(EfParticle.cxf, EfParticle.cyf, EfParticle.cx, EfParticle.cy, EfParticle._thick, false, num15, num16);
						}
						else
						{
							int num43 = X.IntC(X.Abs(num42 - num39) / (6f / EfParticle.elen));
							float num44 = (num42 - num39) / (float)num43;
							float num45 = (float)((this.gradation != "LINE") ? 0 : ((num44 < 0f) ? 1 : 0));
							float num46 = ((this.gradation != "LINE") ? 0f : (1f / (float)num43 * (float)((num44 < 0f) ? (-1) : 1)));
							while (--num43 >= 0)
							{
								if (this.ex_max != 0f)
								{
									EfParticle.exz += num44;
								}
								else if (this.mv_max != 0f)
								{
									EfParticle.mvz += num44;
								}
								float num47 = num45 + num46;
								this.hold().calcXY(E, false, true);
								EfParticle.Md.Line(EfParticle.cxf, EfParticle.cyf, EfParticle.cx, EfParticle.cy, EfParticle._thick, false, num45, num47);
								num45 = num47;
							}
						}
					}
					else if (this.type == -12)
					{
						float num48 = 7f;
						float num39 = 1f / X.Mx(X.Mx(16f, EfParticle.mvlen / num48 * 2f), EfParticle.elen / num48 * 2f);
						float num44 = num39;
						float num40 = num37;
						while (EfParticle.tz > 0f)
						{
							if (num40 <= 0f)
							{
								break;
							}
							this.hold();
							float num49 = EfParticle.tz;
							EfParticle.tz = X.Mx(EfParticle.tz - num44, 0f);
							this.calcXY(E, true, true).GetMesh(E, null, true);
							EfParticle.Md.Line(EfParticle.cxf, EfParticle.cyf, EfParticle.cx, EfParticle.cy, (float)X.IntR(EfParticle._zm), false, 0f, 0f);
							float num41 = X.LENGTHXY(EfParticle.cx, EfParticle.cy, EfParticle.cxf, EfParticle.cyf);
							num40 -= num41;
							num44 = X.MMX(num39 * 0.125f, num39 * num48 / X.Mx(num48 * 0.2f, num41), num39 * 2f);
						}
					}
					break;
				}
				case -19:
					if (EfParticle._zm != 0f)
					{
						this.GetMesh(E, null, true);
						MTRX.Drip.Set(EfParticle._thick, EfParticle._zm).drawTo(EfParticle.Md, EfParticle.cx, EfParticle.cy, EfParticle.FnMUL(this.line_len_min + EfParticle.RANMUL(this.line_len_dif, 1712), this.Fn_line_ex_type) / EfParticle._zm, false);
					}
					break;
				case -18:
				case -17:
				{
					float num50 = EfParticle.FnMUL(this.line_len_min + EfParticle.RANMUL(this.line_len_dif, 1712), this.Fn_line_ex_type);
					float num51 = this.line_ex_time_min + EfParticle.RANMUL(this.line_ex_time_dif, 2449);
					if (this.type == -18 && num51 <= 0f && !not_mesh_image_replace)
					{
						if (EfParticle._agR >= 1f && EfParticle._zm <= num50 * 0.125f && num50 > 0f && num50 <= 500f)
						{
							if (EfParticle.Md == null || EfParticle.Md.uv_settype != UV_SETTYPE.IMG)
							{
								EfParticle.Md = null;
							}
							this.GetMesh(E, MTRX.MIicon, true);
							EfParticle.Md.initForImg(MTRX.EffBlurCircle245, 0);
							EfParticle.Md.RectBL(EfParticle.cx - num50, EfParticle.cy - num50, num50 * 2f, num50 * 2f, false);
							break;
						}
						if ((EfParticle._agR >= 1f || EfParticle._agR <= 0f) && num50 <= EfParticle._zm * 0.125f && EfParticle._zm <= EfParticle._thick && EfParticle._zm <= 500f)
						{
							if (EfParticle.Md == null || EfParticle.Md.uv_settype != UV_SETTYPE.IMG)
							{
								EfParticle.Md = null;
							}
							this.GetMesh(E, MTRX.MIicon, true);
							EfParticle.Md.initForImg((EfParticle._agR <= 0f) ? MTRX.EffRippleCircle245 : MTRX.EffCircle128, 0);
							EfParticle.Md.RectBL(EfParticle.cx - EfParticle._zm, EfParticle.cy - EfParticle._zm, EfParticle._zm * 2f, EfParticle._zm * 2f, false);
							break;
						}
					}
					if (EfParticle.Md != null && EfParticle.Md.uv_settype == UV_SETTYPE.IMG)
					{
						EfParticle.Md = null;
					}
					this.GetMesh(E, null, true);
					C32 c = MeshDrawer.ColBuf0.Set(EfParticle.Md.Col).mulA(num51);
					if (this.type == -17)
					{
						if (EfParticle._zm == 0f && num50 > 0f)
						{
							EfParticle.Md.ColGrd.Set(c);
							EfParticle.Md.Poly(EfParticle.cx, EfParticle.cy, num50, EfParticle._agR, this.attr, 0f, false, 1f, 0f);
						}
						else if (num50 <= 0f && EfParticle._zm <= EfParticle._thick)
						{
							EfParticle.Md.ColGrd.Set(c);
							EfParticle.Md.Poly(EfParticle.cx, EfParticle.cy, EfParticle._zm, EfParticle._agR, this.attr, 0f, false, 0f, 1f);
						}
						else
						{
							EfParticle.Md.BlurPoly2(EfParticle.cx, EfParticle.cy, EfParticle._zm, EfParticle._agR, this.attr, EfParticle._thick, num50, c, c);
						}
					}
					else
					{
						C32 c2 = MeshDrawer.ColBuf1.Set(EfParticle.Md.Col).mulA(EfParticle._agR);
						if (EfParticle._zm == 0f && num50 > 0f)
						{
							EfParticle.Md.ColGrd.Set(c2);
							EfParticle.Md.Circle(EfParticle.cx, EfParticle.cy, num50, 0f, false, 1f, 0f);
						}
						else if (num50 <= 0f && EfParticle._zm <= EfParticle._thick)
						{
							EfParticle.Md.ColGrd.Set(c2);
							EfParticle.Md.Circle(EfParticle.cx, EfParticle.cy, EfParticle._zm, 0f, false, 0f, 1f);
						}
						else
						{
							EfParticle.Md.BlurPoly2(EfParticle.cx, EfParticle.cy, EfParticle._zm, EfParticle.RANMUL(3.1415927f, 2880), X.Mx(8, this.attr), EfParticle._thick, num50, c2, c);
						}
					}
					break;
				}
				case -16:
				{
					float num52 = EfParticle.FnMUL(this.line_len_min + EfParticle.RANMUL(this.line_len_dif, 1712), this.Fn_line_ex_type);
					HaloIDrawer haloI = MTRX.HaloI;
					this.GetMesh(E, haloI.SrcImg, haloI.MI);
					if (EfParticle.i == 0)
					{
						haloI.allocTriVer(EfParticle.Md, this.attr, num * ((num52 > 0f) ? 2 : 1));
					}
					haloI.drawTo(EfParticle.Md, this.attr, EfParticle.cx, EfParticle.cy, EfParticle._zm + num52 * 0.65f, EfParticle._thick * 0.35f, EfParticle._agR, 1f, false);
					if (num52 > 0f)
					{
						MTRX.HaloIA.drawTo(EfParticle.Md, this.attr, EfParticle.cx, EfParticle.cy, EfParticle._zm + num52 * 1.25f, EfParticle._thick, EfParticle._agR, 1f, false);
					}
					break;
				}
				case -15:
				{
					int num53 = (int)((ulong)EfParticle.ran % (ulong)((long)(this.BmL.countFrames() / 5)));
					PxlImage image = this.BmL.getImage(num53 * 5 + (int)(5f * EfParticle.tz), 0);
					this.GetMesh(E, image, MTRX.MIicon);
					EfParticle.Md.RotaGraph(EfParticle.cx, EfParticle.cy, EfParticle._zm, EfParticle._agR, null, false);
					break;
				}
				case -14:
				case -4:
				{
					int num54;
					if (this.attr <= 0)
					{
						num54 = (int)(EfParticle.ran % (uint)this.BmL.countFrames());
					}
					else
					{
						num54 = (int)(X.RAN(EfParticle.ran, 379 + (int)(EfParticle.af / (float)this.attr) % 3 * 7) * (float)this.BmL.countFrames());
					}
					PxlImage image2 = this.BmL.getImage(num54, 0);
					this.GetMesh(E, image2, MTRX.getMI(image2));
					EfParticle.Md.RotaGraph(EfParticle.cx, EfParticle.cy, EfParticle._zm, EfParticle._agR, null, false);
					break;
				}
				case -13:
				{
					float num55 = X.Abs(this.line_len_min + EfParticle.RANMUL(this.line_len_dif, 1739)) * 0.5f;
					int num56 = (int)(this.line_ex_time_min + EfParticle.RANMUL(this.line_ex_time_dif, 2471));
					if (num56 > 0)
					{
						num55 = EfParticle.FnMUL(num55, this.Fn_line_ex_type, EfParticle.af / (float)num56);
					}
					this.GetMesh(E, null, true);
					if (EfParticle._thick >= 0f)
					{
						EfParticle.Md.Arc(EfParticle.cx, EfParticle.cy, EfParticle._zm, EfParticle._agR - num55, EfParticle._agR + num55, EfParticle._thick);
					}
					else
					{
						EfParticle.Md.Arc(EfParticle.cx, EfParticle.cy, EfParticle._zm, EfParticle._agR - num55, EfParticle._agR + num55, EfParticle._zm + EfParticle._thick * EfParticle._zm);
					}
					break;
				}
				case -11:
				case -10:
				{
					float zm2 = EfParticle._zm;
					float num57 = EfParticle._zm * (float)this.attr / 100f;
					EfParticle.cx = (float)((int)EfParticle.cx);
					EfParticle.cy = (float)((int)EfParticle.cy);
					this.GetMesh(E, null, true);
					if (EfParticle._zm >= 0f)
					{
						EfParticle.Md.Daia(EfParticle.cx, EfParticle.cy, zm2, num57, false);
					}
					else
					{
						EfParticle.Md.Line(EfParticle.cx + zm2, EfParticle.cy + num57, EfParticle.cx - zm2, EfParticle.cy - num57, EfParticle._thick, false, 0f, 0f);
						EfParticle.Md.Line(EfParticle.cx - zm2, EfParticle.cy + num57, EfParticle.cx + zm2, EfParticle.cy - num57, EfParticle._thick, false, 0f, 0f);
					}
					break;
				}
				case -9:
					this.GetMesh(E, null, true);
					EfParticle.Md.Star(EfParticle.cx, EfParticle.cy, EfParticle._zm, EfParticle._agR, 5, 0.65f, EfParticle._thick, false, num15, num16);
					break;
				case -8:
					this.GetMesh(E, null, true);
					EfParticle.Md.Poly(EfParticle.cx, EfParticle.cy, EfParticle._zm, EfParticle._agR, this.attr, EfParticle._thick, false, num15, num16);
					break;
				case -7:
				{
					int num58 = X.IntR(EfParticle._zm * (0.6f + 0.4f * EfParticle.tz)) * 2;
					int num59 = num58 / 2;
					int num60 = X.IntR((float)num58 * X.ZSIN(EfParticle.tz) * 0.5f) * 2;
					this.GetMesh(E, null, true);
					EfParticle.Md.RectDoughnut(EfParticle.cx - (float)num60 * 0.5f, EfParticle.cy - (float)num60 * 0.5f, (float)num60, (float)num60, EfParticle.cx - (float)num59, EfParticle.cy - (float)num59, EfParticle.cx + (float)num59, EfParticle.cy + (float)num59, false, 0f, 0f, false);
					break;
				}
				case -6:
					EfParticle.DrawBasicBMLR(E, this, EfParticle.ran);
					break;
				case -5:
					EfParticle.DrawBasicBML(E, this, EfParticle.ran);
					break;
				case -3:
					if (EfParticle._thick < EfParticle._zm)
					{
						if (EfParticle._thick <= 0f && EfParticle._zm <= 400f && !not_mesh_image_replace)
						{
							if (EfParticle.Md == null || EfParticle.Md.uv_settype != UV_SETTYPE.IMG)
							{
								EfParticle.Md = null;
							}
							this.GetMesh(E, MTRX.MIicon, false);
							EfParticle.Md.initForImg(MTRX.EffCircle128, 0);
							EfParticle.Md.RectBL(EfParticle.cx - EfParticle._zm, EfParticle.cy - EfParticle._zm, EfParticle._zm * 2f, EfParticle._zm * 2f, false);
						}
						else
						{
							if (EfParticle.Md != null && EfParticle.Md.uv_settype == UV_SETTYPE.IMG)
							{
								EfParticle.Md = null;
							}
							this.GetMesh(E, null, true);
							if (EfParticle._thick <= 0f)
							{
								EfParticle.Md.Circle(EfParticle.cx, EfParticle.cy, EfParticle._zm, 0f, false, num15, num16);
							}
							else
							{
								EfParticle.Md.CircleB(EfParticle.cx, EfParticle.cy, EfParticle._zm, 0f, 0f, (EfParticle._zm - EfParticle._thick) / EfParticle._zm, num15, num16);
							}
						}
					}
					break;
				case -1:
				{
					int num61 = X.Mx(1, X.IntR(EfParticle._zm));
					this.GetMesh(E, null, true);
					EfParticle.Md.Rect(EfParticle.cx, EfParticle.cy, (float)num61, (float)num61, false);
					break;
				}
				default:
					if (!fnPtcDraw(E, this, EfParticle.ran))
					{
						EfParticle.i = num;
					}
					break;
				}
				if (EfParticle.recalc_fn_value == 1)
				{
					EfParticle.recalc_fn_value = ((num11 != 0f) ? 2 : 0);
					goto IL_232E;
				}
				goto IL_232E;
			}
			if (EfParticle.Md != null)
			{
				EfParticle.Md.Identity();
			}
			EfParticle.Md = null;
			return true;
		}

		public EfParticleFuncCalc defineZmFunc(string type, string _default_fn = "ZLINE")
		{
			if (type == null || type == "")
			{
				return new EfParticleFuncCalc(_default_fn, "ZLINE", 1f);
			}
			return new EfParticleFuncCalc(type, _default_fn, 1f);
		}

		private EfParticleFuncCalc defineZmFunc(EfParticleFuncCalc type, string _default_fn = "ZLINE")
		{
			if (type == null)
			{
				return new EfParticleFuncCalc(_default_fn, "ZLINE", 1f);
			}
			return type;
		}

		public EfParticleFuncCalc defineZmFunc(EfParticleVarContainer mde, string type_name, string _default_fn = "ZLINE")
		{
			FieldInfo field = typeof(EfParticle).GetField("Fn_" + type_name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			EfParticleFuncCalc efParticleFuncCalc = (mde.IsSet(type_name) ? this.defineZmFunc(mde.GetV(type_name, _default_fn).ToString(), "ZLINE") : new EfParticleFuncCalc(_default_fn, "ZLINE", 1f));
			field.SetValue(this, efParticleFuncCalc);
			return efParticleFuncCalc;
		}

		private EfParticle hold()
		{
			EfParticle.cxf = EfParticle.cx;
			EfParticle.cyf = EfParticle.cy;
			EfParticle.czf = EfParticle.cz;
			EfParticle.recalc_fn_value = 2;
			return this;
		}

		private EfParticle calcXY(EffectItem E, bool recalc = false, bool recalc_mesh = true)
		{
			EfParticle.cx = 0f;
			EfParticle.cy = 0f;
			EfParticle.cz = 0f;
			float num = EfParticle.slen;
			if (recalc)
			{
				if (this.mv_max != 0f)
				{
					EfParticle.mvz = EfParticle.FnMUL(1f, this.Fn_mv_type);
				}
				if (this.ex_max != 0f)
				{
					EfParticle.exz = EfParticle.FnMUL(1f, this.Fn_ex_type);
				}
				if (this.exagR_max != 0f)
				{
					EfParticle.saR = EfParticle.saR0 + EfParticle.FnMUL(this.exagR_min + EfParticle.RANMUL(this.exagR_dif, 544), this.Fn_exagR_type) * this.exagR_lvl;
				}
			}
			if (this.ex_max != 0f)
			{
				if (this.bezier_max != 0f)
				{
					this.calcBezier();
				}
				else
				{
					num += EfParticle.elen * EfParticle.exz;
				}
			}
			if (this.Fn_expand == null)
			{
				EfParticle.cx += num * X.Cos(EfParticle.saR);
				EfParticle.cy += num * X.Sin(EfParticle.saR) * this.ex_y_ratio;
			}
			else if (!this.Fn_expand(E, this, num, EfParticle.ran))
			{
				EfParticle.no_draw = true;
			}
			if (EfParticle.no_draw)
			{
				return this;
			}
			if (this.zex_max != 0f || this.zex_min != 0f)
			{
				float num2 = this.zex_min + EfParticle.RANMUL(this.zex_dif, 1387);
				EfParticle.cz += EfParticle.FnMUL(num2, this.Fn_zex_type);
			}
			if (this.dirR_defined && this.mv_max != 0f)
			{
				float num3 = this.dirR + (X.RAN(EfParticle.ran, 607) - 0.5f) * this.dirR_hrhb + (this.dir_base_s ? EfParticle.saR : 0f);
				float num4 = EfParticle.mvlen * EfParticle.mvz;
				EfParticle.cx += num4 * X.Cos(num3);
				EfParticle.cy += num4 * X.Sin(num3);
			}
			if (this.xdf_i != 0f)
			{
				EfParticle.cx += EfParticle.FnMUL(this.xdf_i, this.Fn_df_i_type, EfParticle.iz);
			}
			if (this.ydf_i != 0f)
			{
				EfParticle.cy += EfParticle.FnMUL(this.ydf_i, this.Fn_df_i_type, EfParticle.iz);
			}
			if (this.zdf_i != 0f)
			{
				EfParticle.cz += EfParticle.FnMUL(this.zdf_i, this.Fn_df_i_type, EfParticle.iz);
			}
			if (this.xdf_dif != 0f || this.xdf_min != 0f)
			{
				EfParticle.cx += (this.xdf_min + EfParticle.RANMUL(this.xdf_dif, 1038)) * 0.35f * (float)((EfParticle.RANMUL(100f, 1099) < this.xdf_anti) ? (-1) : 1);
			}
			if (this.ydf_dif != 0f || this.ydf_min != 0f)
			{
				EfParticle.cy += (this.ydf_min + EfParticle.RANMUL(this.ydf_dif, 2791)) * 0.35f * (float)((EfParticle.RANMUL(100f, 2836) < this.ydf_anti) ? (-1) : 1);
			}
			if (this.zdf_dif != 0f || this.zdf_min != 0f)
			{
				EfParticle.cz += this.zdf_min + EfParticle.RANMUL(this.zdf_dif, 1408);
			}
			if (this.zfall_g != 0f)
			{
				if (EfParticle.cz < 0f)
				{
					EfParticle.cz = 0f;
				}
				else
				{
					float num5 = EfParticle.af;
					float num6 = this.zspd0_min + EfParticle.RANMUL(this.zspd0_dif, 1655);
					float num7 = EfParticle.cz;
					for (;;)
					{
						EfParticle.cz = num7 + num6 * num5 + 0.5f * this.zfall_g * num5 * num5;
						if (EfParticle.cz > 0f || num5 <= 4f)
						{
							goto IL_04BF;
						}
						float num8 = -Mathf.Sqrt(num6 * num6 - 2f * this.zfall_g * num7) / this.zfall_g;
						if (num8 < 3f || float.IsNaN(num8))
						{
							break;
						}
						if (this.z_bound < 0f)
						{
							goto Block_27;
						}
						int num9 = (int)(-num6 / this.zfall_g + num8);
						num7 = 0f;
						num6 = -(1f - this.z_bound) * (num6 + this.zfall_g * (float)num9);
						num5 -= (float)num9;
					}
					EfParticle.cz = 0f;
					goto IL_04BF;
					Block_27:
					EfParticle.no_draw = true;
					EfParticle.cz = 0f;
					IL_04BF:
					if (EfParticle.cz < 0f)
					{
						EfParticle.cz = 0f;
					}
				}
			}
			if (recalc_mesh && E.EF != null)
			{
				E.EF.calcParticlePosition(EfParticle.Md, ref EfParticle.cx, ref EfParticle.cy, ref EfParticle.cz, false);
			}
			return this;
		}

		private static bool fnExpandDaia(EffectItem E, EfParticle EP, float _slen, uint ran)
		{
			float num = EfParticle.saR;
			num = num / 6.2831855f + 512f;
			num = (num - (float)((int)num)) * 4f;
			float num2 = (float)X.MPF(num < 2f);
			float num3 = (float)X.MPF(num < 1f || num >= 3f);
			num -= (float)((int)num);
			if (_slen >= 0f)
			{
				EfParticle.cx += ((num3 * num2 == 1f) ? (1f - num) : num) * num3 * _slen;
				EfParticle.cy += ((num3 * num2 == 1f) ? num : (1f - num)) * num2 * _slen * EP.ex_y_ratio;
			}
			else
			{
				EfParticle.cx -= num * num3 * _slen;
				EfParticle.cy -= num * num2 * _slen * EP.ex_y_ratio;
			}
			return true;
		}

		private static bool fnExpandSquare(EffectItem E, EfParticle EP, float _slen, uint ran)
		{
			float num = X.correctangleR(EfParticle.saR);
			if (X.BTW(0.7853982f, num, 2.3561945f))
			{
				EfParticle.cy = _slen;
				EfParticle.cx = _slen * (1f - 2f * (num - 0.7853982f) / 1.5707964f);
			}
			else if (X.BTW(-0.7853982f, num, 1.5707964f))
			{
				EfParticle.cx = _slen;
				EfParticle.cy = _slen * (-1f + 2f * (num + 0.7853982f) / 1.5707964f);
			}
			else if (X.BTW(-2.3561945f, num, 0f))
			{
				EfParticle.cy = -_slen;
				EfParticle.cx = _slen * (-1f + 2f * (num + 2.3561945f) / 1.5707964f);
			}
			else
			{
				EfParticle.cx = -_slen;
				if (num < 0f)
				{
					EfParticle.cy = _slen * (-(num + 3.1415927f) / 1.5707964f);
				}
				else
				{
					EfParticle.cy = _slen * (1f - (num - 3.1415927f) / 1.5707964f);
				}
			}
			EfParticle.cy *= EP.ex_y_ratio;
			return true;
		}

		public static bool DrawBasicBMLR(EffectItem E, EfParticle EP, uint ran)
		{
			int num;
			if (EP.BmL == null || (num = EP.BmL.countFrames()) == 0)
			{
				return true;
			}
			PxlImage image = EP.BmL.getImage(X.MMX(0, (int)(X.RAN(ran, 5735) * (float)num), num - 1), 0);
			EP.GetMesh(E, image, MTRX.getMI(image));
			MeshDrawer md = EfParticle.Md;
			md.Col = EP.getColor(true, true).C;
			md.initForImg(image, 0).RotaGraph(EfParticle.cx, EfParticle.cy, EfParticle._zm, EfParticle._agR, null, false);
			return true;
		}

		public static bool DrawBasicBML(EffectItem E, EfParticle EP, uint ran)
		{
			int num;
			if (EP.BmL == null || (num = EP.BmL.countFrames()) == 0)
			{
				return true;
			}
			PxlImage image = EP.BmL.getImage(X.MMX(0, (int)(EfParticle.tz * (float)num), num - 1), 0);
			EP.GetMesh(E, image, MTRX.getMI(image));
			MeshDrawer md = EfParticle.Md;
			md.Col = EP.getColor(true, true).C;
			md.initForImg(image, 0).RotaGraph(EfParticle.cx, EfParticle.cy, EfParticle._zm, EfParticle._agR, null, false);
			return true;
		}

		public EfParticle GetMesh(EffectItem E, PxlImage Img, MImage MI)
		{
			this.GetMesh(E, MI, false);
			EfParticle.Md.initForImg(Img, 0);
			return this;
		}

		public EfParticle GetMesh(EffectItem E, MImage MI = null, bool use_gradation = true)
		{
			return this.GetMesh(E, (BLEND)(this.layer & -257), (this.layer & 256) == 0, use_gradation, MI);
		}

		public EfParticle GetMesh(EffectItem E, BLEND blnd, bool bottom_flag, bool use_gradation = true, MImage MI = null)
		{
			if (use_gradation && (this.gradation == "" || this.gradation == "LINE"))
			{
				use_gradation = false;
			}
			bool flag = false;
			if (EfParticle.Md == null)
			{
				if (MI != null)
				{
					if (E.EF != null)
					{
						EfParticle.Md = E.EF.MeshInitImg(this.mesh_key, E.x, E.y, MI, out flag, blnd, bottom_flag);
					}
					else
					{
						EfParticle.Md = E.GetMeshImg(this.mesh_key, MI, blnd, bottom_flag);
					}
					EfParticle.Md.Col = this.getColor(true, false).C;
				}
				else
				{
					if (E.EF != null)
					{
						EfParticle.Md = E.EF.MeshInit(this.mesh_key, E.x, E.y, this.getColor(true, use_gradation || this.gradation == "LINE").C, out flag, blnd, bottom_flag);
					}
					else
					{
						EfParticle.Md = E.GetMesh(this.mesh_key, uint.MaxValue, blnd, bottom_flag);
						EfParticle.Md.Col = this.getColor(true, use_gradation || this.gradation == "LINE").C;
					}
					if (this.gradation == "LINE")
					{
						EfParticle.Md.ColGrd.Set(this.getColor2(true));
					}
				}
			}
			else
			{
				if (MI != null && !use_gradation)
				{
					EfParticle.Md.Col = this.getColor(true, false).C;
				}
				else
				{
					EfParticle.Md.Col = this.getColor(true, use_gradation || this.gradation == "LINE").C;
				}
				if (use_gradation || this.gradation == "LINE")
				{
					EfParticle.Md.ColGrd.Set(this.getColor2(true));
				}
			}
			if (E.EF != null)
			{
				E.EF.calcParticlePosition(EfParticle.Md, ref EfParticle.cx, ref EfParticle.cy, ref EfParticle.cz, flag);
			}
			EfParticle.Md.Identity();
			if (this.Fn_mesh_scale_y_type.ToString() != "Z1" || this.mesh_scale_y_min != 100f || this.mesh_scale_y_dif != 0f)
			{
				EfParticle.Md.Scale(1f, EfParticle.FnMUL(this.mesh_scale_y_min + EfParticle.RANMUL(this.mesh_scale_y_dif, 1952), this.Fn_mesh_scale_y_type) * 0.01f, false);
			}
			if (this.mesh_rot_agR_base_s || this.mesh_rot_agR_min != 0f || this.mesh_rot_agR_dif != 0f || this.mesh_rot_agR_i != 0f)
			{
				EfParticle.Md.Rotate((this.mesh_rot_agR_base_s ? EfParticle.saR : 0f) + EfParticle.FnMUL(this.mesh_rot_agR_min + EfParticle.RANMUL(this.mesh_rot_agR_dif, 596), this.Fn_mesh_rot_agR_type) + this.mesh_rot_agR_i * (float)EfParticle.i, false);
			}
			if (this.mesh_translate_dif != 0f || this.mesh_translate_min != 0f)
			{
				float num = this.mesh_translate_dirR + ((this.mesh_translate_dirR_hrhb != 0f) ? (X.NI(-1f, 1f, X.RAN(EfParticle.ran, 1476)) * this.mesh_translate_dirR_hrhb) : 0f);
				float num2 = EfParticle.FnMUL(this.mesh_translate_min + EfParticle.RANMUL(this.mesh_translate_dif, 2582), this.Fn_mesh_translate_type);
				EfParticle.Md.TranslateP(num2 * X.Cos(num), num2 * X.Sin(num), false);
			}
			return this;
		}

		public C32 getColor(bool consider_alpha = true, bool use_gradation = true)
		{
			if (use_gradation && this.gradation == "")
			{
				use_gradation = false;
			}
			if (!this.col_index_s || use_gradation)
			{
				EfParticle.col_buf_s.Set(this.col_s0);
			}
			else
			{
				EfParticle.col_buf_s.Set(this.col_s0).blend(this.col_s1, this.RAN_RESO(708, this.color_resolution));
			}
			C32 c = EfParticle.col_buf_s;
			if (this.col_e_setted)
			{
				C32 c2 = ((this.col_index_e && !use_gradation) ? EfParticle.col_buf_e.Set(this.col_e0).blend(this.col_e1, this.RAN_RESO(1440, this.color_resolution)) : this.col_e0);
				c = EfParticle.col_buf_s.blend(c2, EfParticle.FnMUL(1f, this.Fn_color_type));
			}
			if (consider_alpha)
			{
				c.setA((float)c.a * EfParticle.calpha);
			}
			return c;
		}

		public C32 getColor2(bool consider_alpha = true)
		{
			C32 c = EfParticle.col_buf_e.Set(this.col_s1);
			if (this.col_e_setted)
			{
				c.blend(this.col_e1, EfParticle.FnMUL(1f, this.Fn_color_type));
			}
			if (consider_alpha)
			{
				c.setA((float)c.a * EfParticle.calpha);
			}
			return c;
		}

		public string mesh_key
		{
			get
			{
				return this.layer_key;
			}
		}

		public float all_maxt
		{
			get
			{
				return (float)this.maxt + this.delay * (float)(this.count - 1);
			}
		}

		private EfParticle calcBezier()
		{
			if (this.bezier_max < 0f)
			{
				return this;
			}
			float num = (this.bezier_min + EfParticle.RANMUL(this.bezier_dif, 2209)) * (float)(((double)X.RAN(EfParticle.ran, 358) >= 0.5) ? (-1) : 1);
			float num2 = this.bezier_center_min + EfParticle.RANMUL(this.bezier_center_dif, 2542);
			float num3 = EfParticle.saR + 1.5707964f;
			float num4 = EfParticle.elen * X.Cos(EfParticle.saR);
			float num5 = EfParticle.elen * X.Sin(EfParticle.saR);
			float num6 = num4 * num2 + num * X.Cos(num3);
			float num7 = num5 * num2 + num * X.Sin(num3);
			float num8 = X.BEZIER_I(0f, num6, num6, num4, EfParticle.exz);
			float num9 = X.BEZIER_I(0f, num7, num7, num5, EfParticle.exz);
			EfParticle.cx += num8;
			EfParticle.cy += num9;
			return this;
		}

		public EfParticle makeN(float x, float y, float z = 0f, int time = 0, int saf = 0)
		{
			if (EfParticle.defEF != null)
			{
				EfParticle.defEF.PtcN(this, x, y, z, time, saf);
			}
			return this;
		}

		public float RAN(int rani)
		{
			return X.RAN(EfParticle.ran, rani);
		}

		public float RAN_RESO(int rani, int reso)
		{
			if (reso <= 0)
			{
				return X.RAN(EfParticle.ran, rani);
			}
			return (float)((int)(X.RAN(EfParticle.ran, rani) * (float)reso)) / (float)reso;
		}

		public float bounds_wh
		{
			get
			{
				float num = this.slen_min + this.slen_dif + X.Mx(0f, this.ex_min + X.Abs(this.ex_dif)) + X.Mx(0f, this.mv_min + X.Abs(this.mv_max));
				num += X.Mx(this.xdf_min + this.xdf_dif, this.ydf_min + this.ydf_dif);
				if (this.BmL != null)
				{
					PxlImage image = this.BmL.getImage(0, 0);
					num += (float)X.Mx(image.width, image.height) * (X.Abs(this.zm_min) + X.Abs(this.zm_dif));
				}
				else if (this.Bm0 != null)
				{
					num += X.Mx(this.Bm0.rect.width, this.Bm0.rect.height) * (X.Abs(this.zm_min) + X.Abs(this.zm_dif));
				}
				return num;
			}
		}

		public override EfParticleLoader getLoader()
		{
			return this.Loader.getLoader();
		}

		public static EfParticle Get(string key, bool no_error = false)
		{
			return EfParticleManager.Get(key, false, no_error);
		}

		public static EfParticle Get(StringKey key, bool no_error = false)
		{
			return EfParticleManager.Get(key, no_error);
		}

		public static BDic<string, int> getTypeObject()
		{
			return EfParticle.Otypes;
		}

		public bool inputValuePropInt(string k, int v)
		{
			if (k != null)
			{
				if (!(k == "col_s0"))
				{
					if (!(k == "col_s1"))
					{
						if (!(k == "col_e0"))
						{
							if (!(k == "col_e1"))
							{
								goto IL_0094;
							}
							if (this.col_e1 != null)
							{
								this.col_e1.rgb = (uint)v;
							}
						}
						else if (this.col_e0 != null)
						{
							this.col_e0.rgb = (uint)v;
						}
					}
					else if (this.col_s1 != null)
					{
						this.col_s1.rgb = (uint)v;
					}
				}
				else if (this.col_s0 != null)
				{
					this.col_s0.rgb = (uint)v;
				}
				return true;
			}
			IL_0094:
			return this.inputValueProp(k, (float)v);
		}

		public bool inputValueProp(string k, float v)
		{
			if (k != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(k);
				if (num <= 2539003799U)
				{
					if (num <= 1441675805U)
					{
						if (num <= 744348338U)
						{
							if (num <= 279460018U)
							{
								if (num <= 119310951U)
								{
									if (num != 31627867U)
									{
										if (num != 49074238U)
										{
											if (num != 119310951U)
											{
												return false;
											}
											if (!(k == "dirR_hrhb"))
											{
												return false;
											}
											this.dirR_hrhb = v;
											this.dirR_defined = true;
										}
										else
										{
											if (!(k == "dir_base_s"))
											{
												return false;
											}
											this.dir_base_s = v != 0f;
										}
									}
									else
									{
										if (!(k == "mesh_rot_agR_dif"))
										{
											return false;
										}
										this.mesh_rot_agR_dif = v;
									}
								}
								else if (num != 128336118U)
								{
									if (num != 228252846U)
									{
										if (num != 279460018U)
										{
											return false;
										}
										if (!(k == "attr"))
										{
											return false;
										}
										this.attr = (int)v;
									}
									else
									{
										if (!(k == "line_len_max"))
										{
											return false;
										}
										this.line_len_max = v;
									}
								}
								else
								{
									if (!(k == "layer"))
									{
										return false;
									}
									this.layer = (int)v;
								}
							}
							else if (num <= 531529916U)
							{
								if (num != 299135836U)
								{
									if (num != 428387310U)
									{
										if (num != 531529916U)
										{
											return false;
										}
										if (!(k == "line_len_min"))
										{
											return false;
										}
										this.line_len_min = v;
									}
									else
									{
										if (!(k == "line_ex_time_max"))
										{
											return false;
										}
										this.line_ex_time_max = v;
									}
								}
								else
								{
									if (!(k == "slen_i_max"))
									{
										return false;
									}
									this.slen_i_max = v;
								}
							}
							else if (num != 605347213U)
							{
								if (num != 731664380U)
								{
									if (num != 744348338U)
									{
										return false;
									}
									if (!(k == "dirR"))
									{
										return false;
									}
									this.dirR = v;
									this.dirR_defined = true;
								}
								else
								{
									if (!(k == "line_ex_time_min"))
									{
										return false;
									}
									this.line_ex_time_min = v;
								}
							}
							else
							{
								if (!(k == "slen_i_dif"))
								{
									return false;
								}
								this.slen_i_dif = v;
							}
						}
						else if (num <= 1082353355U)
						{
							if (num <= 926444256U)
							{
								if (num != 821656525U)
								{
									if (num != 898939271U)
									{
										if (num != 926444256U)
										{
											return false;
										}
										if (!(k == "id"))
										{
											return false;
										}
										this.id = (int)v;
									}
									else
									{
										if (!(k == "mesh_rot_agR_base_s"))
										{
											return false;
										}
										this.mesh_rot_agR_base_s = v != 0f;
									}
								}
								else
								{
									if (!(k == "xdf_i"))
									{
										return false;
									}
									this.xdf_i = v;
								}
							}
							else if (num != 967958004U)
							{
								if (num != 1004495142U)
								{
									if (num != 1082353355U)
									{
										return false;
									}
									if (!(k == "exagR_lvl"))
									{
										return false;
									}
									this.exagR_lvl = v;
								}
								else
								{
									if (!(k == "thick_dif"))
									{
										return false;
									}
									this.thick_dif = v;
								}
							}
							else
							{
								if (!(k == "count"))
								{
									return false;
								}
								this.count = (int)v;
							}
						}
						else if (num <= 1318313586U)
						{
							if (num != 1126642112U)
							{
								if (num != 1243596043U)
								{
									if (num != 1318313586U)
									{
										return false;
									}
									if (!(k == "xdf_anti"))
									{
										return false;
									}
									this.xdf_anti = v;
								}
								else
								{
									if (!(k == "thick_max"))
									{
										return false;
									}
									this.thick_max = v;
								}
							}
							else
							{
								if (!(k == "zex_dif"))
								{
									return false;
								}
								this.zex_dif = v;
							}
						}
						else if (num <= 1329543406U)
						{
							if (num != 1322381784U)
							{
								if (num != 1329543406U)
								{
									return false;
								}
								if (!(k == "bezier_dif"))
								{
									return false;
								}
								this.bezier_dif = v;
							}
							else
							{
								if (!(k == "delay"))
								{
									return false;
								}
								this.delay = v;
							}
						}
						else if (num != 1382896855U)
						{
							if (num != 1441675805U)
							{
								return false;
							}
							if (!(k == "bezier_min"))
							{
								return false;
							}
							this.bezier_min = v;
						}
						else
						{
							if (!(k == "slen_hrhb_agR"))
							{
								return false;
							}
							this.slen_hrhb_agR = v;
						}
					}
					else if (num <= 1998986858U)
					{
						if (num <= 1624210000U)
						{
							if (num <= 1590201195U)
							{
								if (num != 1443765147U)
								{
									if (num != 1444112940U)
									{
										if (num != 1590201195U)
										{
											return false;
										}
										if (!(k == "mesh_rot_agR_i"))
										{
											return false;
										}
										this.mesh_rot_agR_i = v;
									}
									else
									{
										if (!(k == "slen_agR_i_dif"))
										{
											return false;
										}
										this.slen_agR_i_dif = v;
									}
								}
								else
								{
									if (!(k == "line_len_dif"))
									{
										return false;
									}
									this.line_len_dif = v;
								}
							}
							else if (num != 1608172067U)
							{
								if (num != 1611423733U)
								{
									if (num != 1624210000U)
									{
										return false;
									}
									if (!(k == "slen_hrhb_min_agR"))
									{
										return false;
									}
									this.slen_hrhb_min_agR = v;
								}
								else
								{
									if (!(k == "thick_min"))
									{
										return false;
									}
									this.thick_min = v;
								}
							}
							else
							{
								if (!(k == "bezier_max"))
								{
									return false;
								}
								this.bezier_max = v;
							}
						}
						else if (num <= 1843120354U)
						{
							if (num != 1643899611U)
							{
								if (num != 1787826528U)
								{
									if (num != 1843120354U)
									{
										return false;
									}
									if (!(k == "draw_eagR_max"))
									{
										return false;
									}
									this.draw_eagR_max = v;
								}
								else
								{
									if (!(k == "zm_dif"))
									{
										return false;
									}
									this.zm_dif = v;
								}
							}
							else
							{
								if (!(k == "line_ex_time_dif"))
								{
									return false;
								}
								this.line_ex_time_dif = v;
							}
						}
						else if (num != 1876762533U)
						{
							if (num != 1887605106U)
							{
								if (num != 1998986858U)
								{
									return false;
								}
								if (!(k == "alp_s"))
								{
									return false;
								}
								this.alp_s = v;
							}
							else
							{
								if (!(k == "mesh_scale_y_min"))
								{
									return false;
								}
								this.mesh_scale_y_min = v;
							}
						}
						else
						{
							if (!(k == "slen_agR_i_max"))
							{
								return false;
							}
							this.slen_agR_i_max = v;
						}
					}
					else if (num <= 2159948154U)
					{
						if (num <= 2045818651U)
						{
							if (num != 2009616616U)
							{
								if (num != 2044100223U)
								{
									if (num != 2045818651U)
									{
										return false;
									}
									if (!(k == "slen_agR_i_min"))
									{
										return false;
									}
									this.slen_agR_i_min = v;
								}
								else
								{
									if (!(k == "zex_min"))
									{
										return false;
									}
									this.zex_min = v;
								}
							}
							else
							{
								if (!(k == "draw_eagR_min"))
								{
									return false;
								}
								this.draw_eagR_min = v;
							}
						}
						else if (num != 2054101368U)
						{
							if (num != 2103264170U)
							{
								if (num != 2159948154U)
								{
									return false;
								}
								if (!(k == "zspd0_dif"))
								{
									return false;
								}
								this.zspd0_dif = v;
							}
							else
							{
								if (!(k == "mv_dif"))
								{
									return false;
								}
								this.mv_dif = v;
							}
						}
						else
						{
							if (!(k == "mesh_scale_y_max"))
							{
								return false;
							}
							this.mesh_scale_y_max = v;
						}
					}
					else if (num <= 2238286585U)
					{
						if (num != 2166763048U)
						{
							if (num != 2211067306U)
							{
								if (num != 2238286585U)
								{
									return false;
								}
								if (!(k == "zspd0_min"))
								{
									return false;
								}
								this.zspd0_min = v;
							}
							else
							{
								if (!(k == "ydf_i"))
								{
									return false;
								}
								this.ydf_i = v;
							}
						}
						else
						{
							if (!(k == "alp_e"))
							{
								return false;
							}
							this.alp_e = v;
						}
					}
					else if (num <= 2472361685U)
					{
						if (num != 2277603793U)
						{
							if (num != 2472361685U)
							{
								return false;
							}
							if (!(k == "ex_reverse"))
							{
								return false;
							}
							this.ex_reverse = (int)v;
						}
						else
						{
							if (!(k == "zex_max"))
							{
								return false;
							}
							this.zex_max = v;
						}
					}
					else if (num != 2528994631U)
					{
						if (num != 2539003799U)
						{
							return false;
						}
						if (!(k == "zspd0_max"))
						{
							return false;
						}
						this.zspd0_max = v;
					}
					else
					{
						if (!(k == "t_fast_hrhb"))
						{
							return false;
						}
						this.t_fast_hrhb = v;
					}
				}
				else if (num <= 3387802471U)
				{
					if (num <= 3000775713U)
					{
						if (num <= 2748685026U)
						{
							if (num <= 2651503564U)
							{
								if (num != 2628306596U)
								{
									if (num != 2646701067U)
									{
										if (num != 2651503564U)
										{
											return false;
										}
										if (!(k == "time_lock_factor"))
										{
											return false;
										}
										this.time_lock_factor = (byte)v;
										return true;
									}
									else
									{
										if (!(k == "ydf_anti"))
										{
											return false;
										}
										this.ydf_anti = v;
									}
								}
								else
								{
									if (!(k == "draw_eagR_anti"))
									{
										return false;
									}
									this.draw_eagR_anti = (int)v;
								}
							}
							else if (num != 2663707935U)
							{
								if (num != 2747364868U)
								{
									if (num != 2748685026U)
									{
										return false;
									}
									if (!(k == "zmadd_max"))
									{
										return false;
									}
									this.zmadd_max = v;
								}
								else
								{
									if (!(k == "draw_sagR_max"))
									{
										return false;
									}
									this.draw_sagR_max = v;
								}
							}
							else
							{
								if (!(k == "zm_min"))
								{
									return false;
								}
								this.zm_min = v;
							}
						}
						else if (num <= 2915181288U)
						{
							if (num != 2767168975U)
							{
								if (num != 2897314673U)
								{
									if (num != 2915181288U)
									{
										return false;
									}
									if (!(k == "zmadd_min"))
									{
										return false;
									}
									this.zmadd_min = v;
								}
								else
								{
									if (!(k == "zm_max"))
									{
										return false;
									}
									this.zm_max = v;
								}
							}
							else
							{
								if (!(k == "ydf_max"))
								{
									return false;
								}
								this.ydf_max = v;
							}
						}
						else if (num != 2928171074U)
						{
							if (num != 2980868438U)
							{
								if (num != 3000775713U)
								{
									return false;
								}
								if (!(k == "ydf_min"))
								{
									return false;
								}
								this.ydf_min = v;
							}
							else
							{
								if (!(k == "draw_sagR_min"))
								{
									return false;
								}
								this.draw_sagR_min = v;
							}
						}
						else
						{
							if (!(k == "ydf_dif"))
							{
								return false;
							}
							this.ydf_dif = v;
						}
					}
					else if (num <= 3087085257U)
					{
						if (num <= 3050565098U)
						{
							if (num != 3004782853U)
							{
								if (num != 3050334567U)
								{
									if (num != 3050565098U)
									{
										return false;
									}
									if (!(k == "mesh_translate_dirR_hrhb"))
									{
										return false;
									}
									this.mesh_translate_dirR_hrhb = v;
								}
								else
								{
									if (!(k == "mv_max"))
									{
										return false;
									}
									this.mv_max = v;
								}
							}
							else
							{
								if (!(k == "bezier_center_min"))
								{
									return false;
								}
								this.bezier_center_min = v;
							}
						}
						else if (num != 3052388801U)
						{
							if (num != 3066050270U)
							{
								if (num != 3087085257U)
								{
									return false;
								}
								if (!(k == "ex_max"))
								{
									return false;
								}
								this.ex_max = v;
							}
							else
							{
								if (!(k == "z_bound"))
								{
									return false;
								}
								this.z_bound = v;
							}
						}
						else
						{
							if (!(k == "mesh_translate_dirR"))
							{
								return false;
							}
							this.mesh_translate_dirR = v;
						}
					}
					else if (num <= 3173942139U)
					{
						if (num != 3091099912U)
						{
							if (num != 3111082862U)
							{
								if (num != 3173942139U)
								{
									return false;
								}
								if (!(k == "bezier_center_max"))
								{
									return false;
								}
								this.bezier_center_max = v;
							}
							else
							{
								if (!(k == "mesh_rot_agR_max"))
								{
									return false;
								}
								this.mesh_rot_agR_max = v;
							}
						}
						else
						{
							if (!(k == "xdf_max"))
							{
								return false;
							}
							this.xdf_max = v;
						}
					}
					else if (num <= 3286501161U)
					{
						if (num != 3223083556U)
						{
							if (num != 3286501161U)
							{
								return false;
							}
							if (!(k == "mv_min"))
							{
								return false;
							}
							this.mv_min = v;
						}
						else
						{
							if (!(k == "mesh_translate_max"))
							{
								return false;
							}
							this.mesh_translate_max = v;
						}
					}
					else if (num != 3297810465U)
					{
						if (num != 3387802471U)
						{
							return false;
						}
						if (!(k == "ex_min"))
						{
							return false;
						}
						this.ex_min = v;
					}
					else
					{
						if (!(k == "mesh_scale_y_dif"))
						{
							return false;
						}
						this.mesh_scale_y_dif = v;
					}
				}
				else if (num <= 3808267793U)
				{
					if (num <= 3471725046U)
					{
						if (num <= 3456690294U)
						{
							if (num != 3414359932U)
							{
								if (num != 3432589299U)
								{
									if (num != 3456690294U)
									{
										return false;
									}
									if (!(k == "mesh_translate_min"))
									{
										return false;
									}
									this.mesh_translate_min = v;
								}
								else
								{
									if (!(k == "zdf_dif"))
									{
										return false;
									}
									this.zdf_dif = v;
								}
							}
							else
							{
								if (!(k == "mesh_rot_agR_min"))
								{
									return false;
								}
								this.mesh_rot_agR_min = v;
							}
						}
						else if (num != 3461487458U)
						{
							if (num != 3464126998U)
							{
								if (num != 3471725046U)
								{
									return false;
								}
								if (!(k == "bezier_center_dif"))
								{
									return false;
								}
								this.bezier_center_dif = v;
							}
							else
							{
								if (!(k == "zfall_g"))
								{
									return false;
								}
								this.zfall_g = v;
							}
						}
						else
						{
							if (!(k == "xdf_min"))
							{
								return false;
							}
							this.xdf_min = v;
						}
					}
					else if (num <= 3600672552U)
					{
						if (num != 3527245989U)
						{
							if (num != 3586199592U)
							{
								if (num != 3600672552U)
								{
									return false;
								}
								if (!(k == "ex_y_ratio"))
								{
									return false;
								}
								this.ex_y_ratio = v;
							}
							else
							{
								if (!(k == "ex_dif"))
								{
									return false;
								}
								this.ex_dif = v;
							}
						}
						else
						{
							if (!(k == "mesh_translate_dif"))
							{
								return false;
							}
							this.mesh_translate_dif = v;
						}
					}
					else if (num != 3751129572U)
					{
						if (num != 3798729879U)
						{
							if (num != 3808267793U)
							{
								return false;
							}
							if (!(k == "xdf_dif"))
							{
								return false;
							}
							this.xdf_dif = v;
						}
						else
						{
							if (!(k == "maxt"))
							{
								return false;
							}
							this.maxt = (int)v;
						}
					}
					else
					{
						if (!(k == "slen_max"))
						{
							return false;
						}
						this.slen_max = v;
					}
				}
				else if (num <= 4016135344U)
				{
					if (num <= 3862330040U)
					{
						if (num != 3832536231U)
						{
							if (num != 3862033302U)
							{
								if (num != 3862330040U)
								{
									return false;
								}
								if (!(k == "slen_agR"))
								{
									return false;
								}
								this.slen_agR = v;
							}
							else
							{
								if (!(k == "zdf_max"))
								{
									return false;
								}
								this.zdf_max = v;
							}
						}
						else
						{
							if (!(k == "zmadd_dif"))
							{
								return false;
							}
							this.zmadd_dif = v;
						}
					}
					else if (num != 3897962959U)
					{
						if (num != 3984736310U)
						{
							if (num != 4016135344U)
							{
								return false;
							}
							if (!(k == "draw_agR_base_s"))
							{
								return false;
							}
							this.draw_agR_base_s = v != 0f;
						}
						else
						{
							if (!(k == "slen_min"))
							{
								return false;
							}
							this.slen_min = v;
						}
					}
					else
					{
						if (!(k == "exagR_max"))
						{
							return false;
						}
						this.exagR_max = v;
					}
				}
				else if (num <= 4102146371U)
				{
					if (num != 4058965058U)
					{
						if (num != 4096868709U)
						{
							if (num != 4102146371U)
							{
								return false;
							}
							if (!(k == "zdf_i"))
							{
								return false;
							}
							this.zdf_i = v;
						}
						else
						{
							if (!(k == "slen_dif"))
							{
								return false;
							}
							this.slen_dif = v;
						}
					}
					else
					{
						if (!(k == "exagR_dif"))
						{
							return false;
						}
						this.exagR_dif = v;
					}
				}
				else if (num <= 4154148699U)
				{
					if (num != 4131569697U)
					{
						if (num != 4154148699U)
						{
							return false;
						}
						if (!(k == "mv_reverse"))
						{
							return false;
						}
						this.mv_reverse = (int)v;
					}
					else
					{
						if (!(k == "exagR_min"))
						{
							return false;
						}
						this.exagR_min = v;
					}
				}
				else if (num != 4162750516U)
				{
					if (num != 4293385918U)
					{
						return false;
					}
					if (!(k == "slen_i_min"))
					{
						return false;
					}
					this.slen_i_min = v;
				}
				else
				{
					if (!(k == "zdf_min"))
					{
						return false;
					}
					this.zdf_min = v;
				}
				return true;
			}
			return false;
		}

		public bool getValueProp(string k, out float ret)
		{
			if (k != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(k);
				if (num <= 2539003799U)
				{
					if (num <= 1441675805U)
					{
						if (num <= 744348338U)
						{
							if (num <= 279460018U)
							{
								if (num <= 119310951U)
								{
									if (num != 31627867U)
									{
										if (num != 49074238U)
										{
											if (num == 119310951U)
											{
												if (k == "dirR_hrhb")
												{
													ret = this.dirR_hrhb;
													return true;
												}
											}
										}
										else if (k == "dir_base_s")
										{
											ret = (float)(this.dir_base_s ? 1 : 0);
											return true;
										}
									}
									else if (k == "mesh_rot_agR_dif")
									{
										ret = this.mesh_rot_agR_dif;
										return true;
									}
								}
								else if (num != 128336118U)
								{
									if (num != 228252846U)
									{
										if (num == 279460018U)
										{
											if (k == "attr")
											{
												ret = (float)this.attr;
												return true;
											}
										}
									}
									else if (k == "line_len_max")
									{
										ret = this.line_len_max;
										return true;
									}
								}
								else if (k == "layer")
								{
									ret = (float)this.layer;
									return true;
								}
							}
							else if (num <= 531529916U)
							{
								if (num != 299135836U)
								{
									if (num != 428387310U)
									{
										if (num == 531529916U)
										{
											if (k == "line_len_min")
											{
												ret = this.line_len_min;
												return true;
											}
										}
									}
									else if (k == "line_ex_time_max")
									{
										ret = this.line_ex_time_max;
										return true;
									}
								}
								else if (k == "slen_i_max")
								{
									ret = this.slen_i_max;
									return true;
								}
							}
							else if (num != 605347213U)
							{
								if (num != 731664380U)
								{
									if (num == 744348338U)
									{
										if (k == "dirR")
										{
											ret = this.dirR;
											return true;
										}
									}
								}
								else if (k == "line_ex_time_min")
								{
									ret = this.line_ex_time_min;
									return true;
								}
							}
							else if (k == "slen_i_dif")
							{
								ret = this.slen_i_dif;
								return true;
							}
						}
						else if (num <= 1082353355U)
						{
							if (num <= 926444256U)
							{
								if (num != 821656525U)
								{
									if (num != 898939271U)
									{
										if (num == 926444256U)
										{
											if (k == "id")
											{
												ret = (float)this.id;
												return true;
											}
										}
									}
									else if (k == "mesh_rot_agR_base_s")
									{
										ret = (float)(this.mesh_rot_agR_base_s ? 1 : 0);
										return true;
									}
								}
								else if (k == "xdf_i")
								{
									ret = this.xdf_i;
									return true;
								}
							}
							else if (num != 967958004U)
							{
								if (num != 1004495142U)
								{
									if (num == 1082353355U)
									{
										if (k == "exagR_lvl")
										{
											ret = this.exagR_lvl;
											return true;
										}
									}
								}
								else if (k == "thick_dif")
								{
									ret = this.thick_dif;
									return true;
								}
							}
							else if (k == "count")
							{
								ret = (float)this.count;
								return true;
							}
						}
						else if (num <= 1318313586U)
						{
							if (num != 1126642112U)
							{
								if (num != 1243596043U)
								{
									if (num == 1318313586U)
									{
										if (k == "xdf_anti")
										{
											ret = this.xdf_anti;
											return true;
										}
									}
								}
								else if (k == "thick_max")
								{
									ret = this.thick_max;
									return true;
								}
							}
							else if (k == "zex_dif")
							{
								ret = this.zex_dif;
								return true;
							}
						}
						else if (num <= 1329543406U)
						{
							if (num != 1322381784U)
							{
								if (num == 1329543406U)
								{
									if (k == "bezier_dif")
									{
										ret = this.bezier_dif;
										return true;
									}
								}
							}
							else if (k == "delay")
							{
								ret = this.delay;
								return true;
							}
						}
						else if (num != 1382896855U)
						{
							if (num == 1441675805U)
							{
								if (k == "bezier_min")
								{
									ret = this.bezier_min;
									return true;
								}
							}
						}
						else if (k == "slen_hrhb_agR")
						{
							ret = this.slen_hrhb_agR;
							return true;
						}
					}
					else if (num <= 1998986858U)
					{
						if (num <= 1624210000U)
						{
							if (num <= 1590201195U)
							{
								if (num != 1443765147U)
								{
									if (num != 1444112940U)
									{
										if (num == 1590201195U)
										{
											if (k == "mesh_rot_agR_i")
											{
												ret = this.mesh_rot_agR_i;
												return true;
											}
										}
									}
									else if (k == "slen_agR_i_dif")
									{
										ret = this.slen_agR_i_dif;
										return true;
									}
								}
								else if (k == "line_len_dif")
								{
									ret = this.line_len_dif;
									return true;
								}
							}
							else if (num != 1608172067U)
							{
								if (num != 1611423733U)
								{
									if (num == 1624210000U)
									{
										if (k == "slen_hrhb_min_agR")
										{
											ret = this.slen_hrhb_min_agR;
											return true;
										}
									}
								}
								else if (k == "thick_min")
								{
									ret = this.thick_min;
									return true;
								}
							}
							else if (k == "bezier_max")
							{
								ret = this.bezier_max;
								return true;
							}
						}
						else if (num <= 1843120354U)
						{
							if (num != 1643899611U)
							{
								if (num != 1787826528U)
								{
									if (num == 1843120354U)
									{
										if (k == "draw_eagR_max")
										{
											ret = this.draw_eagR_max;
											return true;
										}
									}
								}
								else if (k == "zm_dif")
								{
									ret = this.zm_dif;
									return true;
								}
							}
							else if (k == "line_ex_time_dif")
							{
								ret = this.line_ex_time_dif;
								return true;
							}
						}
						else if (num != 1876762533U)
						{
							if (num != 1887605106U)
							{
								if (num == 1998986858U)
								{
									if (k == "alp_s")
									{
										ret = this.alp_s;
										return true;
									}
								}
							}
							else if (k == "mesh_scale_y_min")
							{
								ret = this.mesh_scale_y_min;
								return true;
							}
						}
						else if (k == "slen_agR_i_max")
						{
							ret = this.slen_agR_i_max;
							return true;
						}
					}
					else if (num <= 2159948154U)
					{
						if (num <= 2045818651U)
						{
							if (num != 2009616616U)
							{
								if (num != 2044100223U)
								{
									if (num == 2045818651U)
									{
										if (k == "slen_agR_i_min")
										{
											ret = this.slen_agR_i_min;
											return true;
										}
									}
								}
								else if (k == "zex_min")
								{
									ret = this.zex_min;
									return true;
								}
							}
							else if (k == "draw_eagR_min")
							{
								ret = this.draw_eagR_min;
								return true;
							}
						}
						else if (num != 2054101368U)
						{
							if (num != 2103264170U)
							{
								if (num == 2159948154U)
								{
									if (k == "zspd0_dif")
									{
										ret = this.zspd0_dif;
										return true;
									}
								}
							}
							else if (k == "mv_dif")
							{
								ret = this.mv_dif;
								return true;
							}
						}
						else if (k == "mesh_scale_y_max")
						{
							ret = this.mesh_scale_y_max;
							return true;
						}
					}
					else if (num <= 2238286585U)
					{
						if (num != 2166763048U)
						{
							if (num != 2211067306U)
							{
								if (num == 2238286585U)
								{
									if (k == "zspd0_min")
									{
										ret = this.zspd0_min;
										return true;
									}
								}
							}
							else if (k == "ydf_i")
							{
								ret = this.ydf_i;
								return true;
							}
						}
						else if (k == "alp_e")
						{
							ret = this.alp_e;
							return true;
						}
					}
					else if (num <= 2472361685U)
					{
						if (num != 2277603793U)
						{
							if (num == 2472361685U)
							{
								if (k == "ex_reverse")
								{
									ret = (float)this.ex_reverse;
									return true;
								}
							}
						}
						else if (k == "zex_max")
						{
							ret = this.zex_max;
							return true;
						}
					}
					else if (num != 2528994631U)
					{
						if (num == 2539003799U)
						{
							if (k == "zspd0_max")
							{
								ret = this.zspd0_max;
								return true;
							}
						}
					}
					else if (k == "t_fast_hrhb")
					{
						ret = this.t_fast_hrhb;
						return true;
					}
				}
				else if (num <= 3387802471U)
				{
					if (num <= 3000775713U)
					{
						if (num <= 2748685026U)
						{
							if (num <= 2651503564U)
							{
								if (num != 2628306596U)
								{
									if (num != 2646701067U)
									{
										if (num == 2651503564U)
										{
											if (k == "time_lock_factor")
											{
												ret = (float)this.time_lock_factor;
												return true;
											}
										}
									}
									else if (k == "ydf_anti")
									{
										ret = this.ydf_anti;
										return true;
									}
								}
								else if (k == "draw_eagR_anti")
								{
									ret = (float)this.draw_eagR_anti;
									return true;
								}
							}
							else if (num != 2663707935U)
							{
								if (num != 2747364868U)
								{
									if (num == 2748685026U)
									{
										if (k == "zmadd_max")
										{
											ret = this.zmadd_max;
											return true;
										}
									}
								}
								else if (k == "draw_sagR_max")
								{
									ret = this.draw_sagR_max;
									return true;
								}
							}
							else if (k == "zm_min")
							{
								ret = this.zm_min;
								return true;
							}
						}
						else if (num <= 2915181288U)
						{
							if (num != 2767168975U)
							{
								if (num != 2897314673U)
								{
									if (num == 2915181288U)
									{
										if (k == "zmadd_min")
										{
											ret = this.zmadd_min;
											return true;
										}
									}
								}
								else if (k == "zm_max")
								{
									ret = this.zm_max;
									return true;
								}
							}
							else if (k == "ydf_max")
							{
								ret = this.ydf_max;
								return true;
							}
						}
						else if (num != 2928171074U)
						{
							if (num != 2980868438U)
							{
								if (num == 3000775713U)
								{
									if (k == "ydf_min")
									{
										ret = this.ydf_min;
										return true;
									}
								}
							}
							else if (k == "draw_sagR_min")
							{
								ret = this.draw_sagR_min;
								return true;
							}
						}
						else if (k == "ydf_dif")
						{
							ret = this.ydf_dif;
							return true;
						}
					}
					else if (num <= 3087085257U)
					{
						if (num <= 3050565098U)
						{
							if (num != 3004782853U)
							{
								if (num != 3050334567U)
								{
									if (num == 3050565098U)
									{
										if (k == "mesh_translate_dirR_hrhb")
										{
											ret = this.mesh_translate_dirR_hrhb;
											return true;
										}
									}
								}
								else if (k == "mv_max")
								{
									ret = this.mv_max;
									return true;
								}
							}
							else if (k == "bezier_center_min")
							{
								ret = this.bezier_center_min;
								return true;
							}
						}
						else if (num != 3052388801U)
						{
							if (num != 3066050270U)
							{
								if (num == 3087085257U)
								{
									if (k == "ex_max")
									{
										ret = this.ex_max;
										return true;
									}
								}
							}
							else if (k == "z_bound")
							{
								ret = this.z_bound;
								return true;
							}
						}
						else if (k == "mesh_translate_dirR")
						{
							ret = this.mesh_translate_dirR;
							return true;
						}
					}
					else if (num <= 3173942139U)
					{
						if (num != 3091099912U)
						{
							if (num != 3111082862U)
							{
								if (num == 3173942139U)
								{
									if (k == "bezier_center_max")
									{
										ret = this.bezier_center_max;
										return true;
									}
								}
							}
							else if (k == "mesh_rot_agR_max")
							{
								ret = this.mesh_rot_agR_max;
								return true;
							}
						}
						else if (k == "xdf_max")
						{
							ret = this.xdf_max;
							return true;
						}
					}
					else if (num <= 3286501161U)
					{
						if (num != 3223083556U)
						{
							if (num == 3286501161U)
							{
								if (k == "mv_min")
								{
									ret = this.mv_min;
									return true;
								}
							}
						}
						else if (k == "mesh_translate_max")
						{
							ret = this.mesh_translate_max;
							return true;
						}
					}
					else if (num != 3297810465U)
					{
						if (num == 3387802471U)
						{
							if (k == "ex_min")
							{
								ret = this.ex_min;
								return true;
							}
						}
					}
					else if (k == "mesh_scale_y_dif")
					{
						ret = this.mesh_scale_y_dif;
						return true;
					}
				}
				else if (num <= 3808267793U)
				{
					if (num <= 3471725046U)
					{
						if (num <= 3456690294U)
						{
							if (num != 3414359932U)
							{
								if (num != 3432589299U)
								{
									if (num == 3456690294U)
									{
										if (k == "mesh_translate_min")
										{
											ret = this.mesh_translate_min;
											return true;
										}
									}
								}
								else if (k == "zdf_dif")
								{
									ret = this.zdf_dif;
									return true;
								}
							}
							else if (k == "mesh_rot_agR_min")
							{
								ret = this.mesh_rot_agR_min;
								return true;
							}
						}
						else if (num != 3461487458U)
						{
							if (num != 3464126998U)
							{
								if (num == 3471725046U)
								{
									if (k == "bezier_center_dif")
									{
										ret = this.bezier_center_dif;
										return true;
									}
								}
							}
							else if (k == "zfall_g")
							{
								ret = this.zfall_g;
								return true;
							}
						}
						else if (k == "xdf_min")
						{
							ret = this.xdf_min;
							return true;
						}
					}
					else if (num <= 3600672552U)
					{
						if (num != 3527245989U)
						{
							if (num != 3586199592U)
							{
								if (num == 3600672552U)
								{
									if (k == "ex_y_ratio")
									{
										ret = this.ex_y_ratio;
										return true;
									}
								}
							}
							else if (k == "ex_dif")
							{
								ret = this.ex_dif;
								return true;
							}
						}
						else if (k == "mesh_translate_dif")
						{
							ret = this.mesh_translate_dif;
							return true;
						}
					}
					else if (num != 3751129572U)
					{
						if (num != 3798729879U)
						{
							if (num == 3808267793U)
							{
								if (k == "xdf_dif")
								{
									ret = this.xdf_dif;
									return true;
								}
							}
						}
						else if (k == "maxt")
						{
							ret = (float)this.maxt;
							return true;
						}
					}
					else if (k == "slen_max")
					{
						ret = this.slen_max;
						return true;
					}
				}
				else if (num <= 4016135344U)
				{
					if (num <= 3862330040U)
					{
						if (num != 3832536231U)
						{
							if (num != 3862033302U)
							{
								if (num == 3862330040U)
								{
									if (k == "slen_agR")
									{
										ret = this.slen_agR;
										return true;
									}
								}
							}
							else if (k == "zdf_max")
							{
								ret = this.zdf_max;
								return true;
							}
						}
						else if (k == "zmadd_dif")
						{
							ret = this.zmadd_dif;
							return true;
						}
					}
					else if (num != 3897962959U)
					{
						if (num != 3984736310U)
						{
							if (num == 4016135344U)
							{
								if (k == "draw_agR_base_s")
								{
									ret = (float)(this.draw_agR_base_s ? 1 : 0);
									return true;
								}
							}
						}
						else if (k == "slen_min")
						{
							ret = this.slen_min;
							return true;
						}
					}
					else if (k == "exagR_max")
					{
						ret = this.exagR_max;
						return true;
					}
				}
				else if (num <= 4102146371U)
				{
					if (num != 4058965058U)
					{
						if (num != 4096868709U)
						{
							if (num == 4102146371U)
							{
								if (k == "zdf_i")
								{
									ret = this.zdf_i;
									return true;
								}
							}
						}
						else if (k == "slen_dif")
						{
							ret = this.slen_dif;
							return true;
						}
					}
					else if (k == "exagR_dif")
					{
						ret = this.exagR_dif;
						return true;
					}
				}
				else if (num <= 4154148699U)
				{
					if (num != 4131569697U)
					{
						if (num == 4154148699U)
						{
							if (k == "mv_reverse")
							{
								ret = (float)this.mv_reverse;
								return true;
							}
						}
					}
					else if (k == "exagR_min")
					{
						ret = this.exagR_min;
						return true;
					}
				}
				else if (num != 4162750516U)
				{
					if (num == 4293385918U)
					{
						if (k == "slen_i_min")
						{
							ret = this.slen_i_min;
							return true;
						}
					}
				}
				else if (k == "zdf_min")
				{
					ret = this.zdf_min;
					return true;
				}
			}
			ret = 0f;
			return false;
		}

		public static IEffectSetter defEF;

		private static int particle_id = 0;

		public string __key = "";

		public EfParticleVarContainer Loader;

		public int type = -1;

		public FnPtcExpand Fn_expand;

		public float line_len_min;

		public float line_len_max;

		public float line_ex_time_min;

		public float line_ex_time_max;

		public EfParticleFuncCalc Fn_line_ex_type;

		public Sprite Bm0;

		public PxlSequence BmL;

		public C32 col_s0;

		public C32 col_e0;

		public EfParticleFuncCalc Fn_color_type;

		public C32 col_s1;

		public C32 col_e1;

		private bool col_index_s;

		private bool col_index_e;

		private bool col_e_setted;

		public int color_resolution = 8;

		private static C32 col_buf_s;

		private static C32 col_buf_e;

		public float camera_check_zm_mul = 1f;

		public float camera_check_zm_add;

		public string rep_z__;

		public string rep_time__;

		public int count = 1;

		public int maxt = 1;

		public int attr;

		public byte time_lock_factor;

		public float delay;

		public float t_fast_hrhb;

		public float xdf_min;

		public float xdf_max;

		public float xdf_anti = 50f;

		public float ydf_min;

		public float ydf_max;

		public float ydf_anti = 50f;

		public float zdf_min;

		public float zdf_max;

		public const float XYDIF_RATIO = 0.35f;

		public float xdf_i;

		public float ydf_i;

		public float zdf_i;

		public EfParticleFuncCalc Fn_df_i_type;

		public float slen_min;

		public float slen_max;

		public float slen_agR;

		public float slen_hrhb_min_agR;

		public float slen_hrhb_agR;

		public float slen_i_min;

		public float slen_i_max;

		public float slen_i_dif;

		public float slen_agR_i_min;

		public float slen_agR_i_max;

		public float slen_agR_i_dif;

		public float exagR_min;

		public float exagR_max;

		public float exagR_lvl = 1f;

		public EfParticleFuncCalc Fn_exagR_type;

		public float ex_min;

		public float ex_max;

		public float zex_min;

		public float zex_max;

		public float ex_y_ratio = 1f;

		public float bezier_min;

		public float bezier_max;

		public float bezier_center_min = 0.5f;

		public float bezier_center_max = 0.5f;

		public EfParticleFuncCalc Fn_ex_type;

		private int ex_reverse = -1;

		public EfParticleFuncCalc Fn_zex_type;

		public float dirR;

		public float dirR_hrhb;

		public bool dir_base_s;

		public float mv_min;

		public float mv_max;

		public EfParticleFuncCalc Fn_mv_type;

		private int mv_reverse = -1;

		public float zm_min;

		public float zm_max;

		public EfParticleFuncCalc Fn_zm_type;

		public float thick_min;

		public float thick_max;

		public EfParticleFuncCalc Fn_thick_type;

		public float zmadd_min;

		public float zmadd_max;

		public float draw_sagR_min;

		public float draw_sagR_max;

		public float draw_eagR_min;

		public float draw_eagR_max;

		public int draw_eagR_anti;

		public EfParticleFuncCalc Fn_draw_agR_type;

		public bool draw_agR_base_s;

		public float alp_s = 1f;

		public float alp_e = 1f;

		public EfParticleFuncCalc Fn_alp_type;

		public float zfall_g;

		public float zspd0_min;

		public float zspd0_max;

		public float zspd0_dif;

		public float z_bound = 0.3f;

		public float mesh_scale_y_min = 100f;

		public float mesh_scale_y_max = 100f;

		public EfParticleFuncCalc Fn_mesh_scale_y_type;

		public float mesh_rot_agR_min;

		public float mesh_rot_agR_max;

		public EfParticleFuncCalc Fn_mesh_rot_agR_type;

		public float mesh_rot_agR_i;

		public bool mesh_rot_agR_base_s;

		public float mesh_translate_min;

		public float mesh_translate_max;

		public EfParticleFuncCalc Fn_mesh_translate_type;

		public float mesh_translate_dirR;

		public float mesh_translate_dirR_hrhb;

		public string gradation = "";

		public int id;

		public int layer;

		public string layer_key = "";

		public float xdf_dif;

		public float ydf_dif;

		public float zdf_dif;

		private float ex_dif;

		private float thick_dif;

		private float exagR_dif;

		private float bezier_dif;

		private float bezier_center_dif;

		private float zex_dif;

		private float mv_dif;

		private float slen_dif;

		public float line_len_dif;

		public float zm_dif;

		public float zmadd_dif;

		public bool dirR_defined;

		public float line_ex_time_dif;

		private float mesh_scale_y_dif;

		private float mesh_rot_agR_dif;

		private float mesh_translate_dif;

		private bool color_initted;

		private static BDic<string, int> Otypes;

		private static BDic<string, FnPtcExpand> OexpandsFn;

		private static FnPtcInit[] AinitFn;

		private static FnPtcDraw[] AdrawFn;

		public static int frame_rate = 1;

		private static Regex RegColor0 = new Regex(" *\\/ *([\\d\\.]+)");

		private static Regex RegColor1 = new Regex(" *\\* *([\\d\\.]+)");

		public static EfParticle CurDrawing;

		public static MeshDrawer Md;

		public static uint ran;

		public static float af;

		public static float slen;

		public static float elen;

		public static float saR;

		public static float saR0;

		public static float cx;

		public static float cy;

		public static float cz;

		public static float _zm;

		public static float _agR;

		public static float _thick;

		public static float exz;

		public static float cxf;

		public static float cyf;

		public static float czf;

		public static float cx0;

		public static float cy0;

		public static float tz0;

		public static float tz;

		public static float mvlen;

		public static float mvz;

		public static bool no_draw;

		public static float calpha;

		private static byte recalc_fn_value;

		public static int i;

		public static float iz;

		public FnEffectRun FD_EfRun;
	}
}
