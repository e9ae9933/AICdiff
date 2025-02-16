using System;
using UnityEngine;

namespace XX
{
	public class Effect<T> : RBase<T>, IEffectSetter where T : EffectItem
	{
		public Effect(GameObject _Gob, int _max = 240)
			: base(_max, false, false, true)
		{
			this.Gob = _Gob;
			this.MaxCount = _max;
			this.ppu = 64f;
			if (this.Gob != null)
			{
				this.Trs = this.Gob.transform;
				this.layer_effect_top = (this.layer_effect_bottom = this.Gob.layer);
			}
		}

		public override T Create()
		{
			return this.fnCreateEffectItem(this, "", 0f, 0f, 0f, 0, 0);
		}

		public virtual Effect<T> initEffect(string _effect_name, Camera _CameraForMesh, Effect<T>.FnCreateEffectItem _fnCreateEffectItem, EFCON_TYPE _ef_type = EFCON_TYPE.NORMAL)
		{
			this.effect_name = _effect_name;
			this.CameraForMesh = _CameraForMesh;
			this.fnCreateEffectItem = _fnCreateEffectItem;
			for (int i = this.MaxCount / 4; i >= 0; i--)
			{
				this.AItems[i] = this.Create();
			}
			this.alloc_instance = true;
			this.ef_type = _ef_type | (this.ef_type & (EFCON_TYPE)6);
			if (this.ppu == 0f)
			{
				this.ppu = 64f;
			}
			if (this.mesh_color == null)
			{
				if (this.useMeshDrawer)
				{
					this.MeshCon = new EffectMeshManager(this.effect_name);
					this.MeshCon.draw_gl_only = this.draw_gl_only;
				}
				this.mesh_color = new C32();
				this.mesh_color2 = new C32();
				if (this.ppu == 0f)
				{
					this.ppu = 64f;
				}
			}
			EffectItem.initEffectItem();
			return this;
		}

		public IEffectSetter setLayer(int layer, int layer_b = -1)
		{
			this.layer_effect_top = layer;
			this.layer_effect_bottom = ((layer_b < 0) ? layer : layer_b);
			return this;
		}

		public override void clear()
		{
			for (int i = 0; i < this.LEN; i++)
			{
				this.AItems[i].index = 2147483647U;
				this.AItems[i].destruct();
			}
			base.clear();
			if (this.MeshCon != null)
			{
				this.MeshCon.clear();
			}
			this.mesh_color.White();
			if (this.PtcSTCon != null)
			{
				this.PtcSTCon.clear();
			}
			this.run(0f);
		}

		private void OnDestroy()
		{
			this.destruct();
		}

		public override void destruct()
		{
			base.destruct();
			if (this.MeshCon != null)
			{
				this.MeshCon.destruct();
			}
			this.PtcSTCon = null;
			this.CameraForMesh = null;
		}

		public virtual EffectItem setE(string etype, float _x, float _y, float _z, int _time, int _saf = 0)
		{
			if (this.LEN >= this.MaxCount)
			{
				if (!this.no_overset_error)
				{
					this.dl("量が多い", true);
				}
				return null;
			}
			T t = base.Pop(32);
			if (t == null)
			{
				return null;
			}
			t.clear(this, etype, _x, _y, _z, _time, _saf);
			EffectItem effectItem = t.initEffect("");
			if (effectItem == null)
			{
				return null;
			}
			return effectItem;
		}

		public T setE(T E)
		{
			if (this.LEN >= this.MaxCount)
			{
				if (!this.no_overset_error)
				{
					this.dl("量が多い", true);
				}
				return default(T);
			}
			if (E.initEffect("") == null)
			{
				return default(T);
			}
			base.Add(E, 64);
			return E;
		}

		public virtual EffectItem setEffectWithSpecificFn(string individual_name, float x, float y, float z, int time, int saf, FnEffectRun Fn)
		{
			T t = base.Pop(32);
			if (t == null)
			{
				return null;
			}
			t.clear(this, individual_name, x, y, z, time, saf);
			t.setEffectSpecificFn(Fn);
			EffectItem effectItem = t.initEffect("");
			if (effectItem.FnDef == null)
			{
				this.LEN--;
				return null;
			}
			return effectItem;
		}

		public virtual EffectItem PtcN(string ptc_name, float _x, float _y, float _z, int _time, int _saf = 0)
		{
			if (!this.useMeshDrawer)
			{
				return null;
			}
			EfParticle efParticle = EfParticle.Get(ptc_name, false);
			if (efParticle == null)
			{
				if (!this.no_overset_error)
				{
					this.dl("Ptc不明" + ptc_name, false);
				}
				return null;
			}
			return this.PtcN(efParticle, _x, _y, _z, _time, _saf);
		}

		public virtual EffectItem PtcN(EfParticle Ptc, float _x, float _y, float _z, int _time, int _saf = 0)
		{
			if (this.LEN >= this.MaxCount)
			{
				if (!this.no_overset_error)
				{
					this.dl("量が多い", true);
				}
				return null;
			}
			T t = base.Pop(32);
			if (t == null)
			{
				return null;
			}
			t.clear(this, "particle", _x, _y, _z, _time, _saf);
			t.setFunction(Ptc.FD_EfRun, Ptc.key);
			return t;
		}

		public EffectItem setAgdEffect(string agd_key, string agd_fn_name, float _x, float _y, float _z, int _time, int _saf = 0)
		{
			return this.setAgdEffect(EfParticleManager.GetAGD(agd_key), agd_fn_name, _x, _y, _z, _time, _saf);
		}

		public EffectItem setAgdEffect(AttackGhostDrawer Agd, string agd_fn_name, float _x, float _y, float _z, int _time, int _saf = 0)
		{
			if (!this.useMeshDrawer)
			{
				return null;
			}
			if (this.LEN >= this.MaxCount)
			{
				if (!this.no_overset_error)
				{
					this.dl("量が多い", true);
				}
				return null;
			}
			T t = base.Pop(32);
			if (t == null)
			{
				return null;
			}
			t.clear(this, agd_fn_name, _x, _y, _z, _time, _saf);
			t.setFunction(Agd.FD_EfDraw, null);
			return t;
		}

		public virtual PTCThreadRunner getThreadRunner()
		{
			if (this.PtcSTCon == null)
			{
				this.PtcSTCon = new PTCThreadRunner(this);
			}
			return this.PtcSTCon;
		}

		public IEffectSetter PtcSTsetVar(string var_name, double val)
		{
			if (this.VarP == null)
			{
				this.VarP = new VariableP(8);
			}
			this.VarP.Add(var_name, val);
			return this;
		}

		public IEffectSetter PtcSTsetVar(string var_name, string val)
		{
			if (this.VarP == null)
			{
				this.VarP = new VariableP(8);
			}
			this.VarP.Add(var_name, val);
			return this;
		}

		public PTCThread PtcST(string ptcst_name, IEfPInteractale Listener, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW, VariableP VarP = null)
		{
			return this.getThreadRunner().makeST(ptcst_name, Listener, follow, VarP ?? this.VarP);
		}

		public void runSetter(float fcnt = 1f)
		{
			if (this.PtcSTCon != null)
			{
				this.PtcSTCon.runSetter(fcnt * (float)this.AF);
			}
		}

		public virtual void runDraw(float fcnt = 1f, bool runsetter = true)
		{
			if (runsetter)
			{
				this.runSetter(fcnt);
			}
			this.run(fcnt * (float)this.AF);
			if (this.MeshCon != null)
			{
				int num = this.layer_effect_top;
				int num2 = this.layer_effect_bottom;
				if (this.no_graphic_render || this.MeshCon.getAssignedMMRD() != null)
				{
					num2 = (num = -1);
				}
				this.MeshCon.Draw(num, num2, this.MeshCon.MatrixTransform, false, this.CameraForMesh);
			}
		}

		public void runDrawOrRedrawMesh(bool flag, float fcnt, float base_TS = 1f)
		{
			this.runSetter(base_TS);
			if (flag)
			{
				this.runDraw(fcnt * base_TS, false);
				return;
			}
			if (!this.no_graphic_render && this.MeshCon != null && this.MeshCon.getAssignedMMRD() == null)
			{
				this.MeshCon.RedrawSameMesh(this.layer_effect_top, this.layer_effect_bottom, this.MeshCon.MatrixTransform, this.CameraForMesh, false);
			}
		}

		public void setEffectBasePos(float ux, float uy, float scale)
		{
			if (this.MeshCon == null)
			{
				return;
			}
			MultiMeshRenderer assignedMMRD = this.MeshCon.getAssignedMMRD();
			if (assignedMMRD != null)
			{
				Transform transform = assignedMMRD.transform;
				transform.localPosition = new Vector3(ux, uy, 0f);
				transform.localScale = new Vector3(scale, scale, 1f);
				return;
			}
			this.MeshCon.MatrixTransform = Matrix4x4.Translate(new Vector3(ux, uy, 0f)) * Matrix4x4.Scale(new Vector3(scale, scale, 1f));
		}

		public void setEffectMatrix(Matrix4x4 Mx)
		{
			if (this.MeshCon != null)
			{
				this.MeshCon.MatrixTransform = Mx;
			}
		}

		public void RenderOneSide(bool bottom_flag, Matrix4x4 Multiple, Camera Cam = null, bool no_z_scale2zero = false)
		{
			this.MeshCon.RedrawSameMesh(bottom_flag ? (-1) : this.layer_effect_top, bottom_flag ? this.layer_effect_bottom : (-1), Multiple * this.MeshCon.MatrixTransform, (Cam != null) ? Cam : this.CameraForMesh, no_z_scale2zero);
		}

		public GameObject AttachObject(GameObject Gob, float _x, float _y)
		{
			if (Gob != null)
			{
				Gob = Object.Instantiate<GameObject>(Gob, new Vector3(_x, _y, 0f), Quaternion.identity);
				Gob.transform.SetParent(this.Trs, false);
			}
			return Gob;
		}

		public GameObject RemoveObject(GameObject Gob)
		{
			if (Gob != null)
			{
				IN.DestroyOne(Gob);
			}
			return null;
		}

		public override bool isActive()
		{
			return this.LEN > 0 || (this.PtcSTCon != null && this.PtcSTCon.isActive());
		}

		public int uPos2scrX(float x, float addition = 0f)
		{
			return (int)((x + this.Trs.position.x) * this.ppu + IN.w * 0.5f) + (int)addition;
		}

		public int uPos2scrY(float y, float addition = 0f)
		{
			return (int)((-y - this.Trs.position.y) * this.ppu + IN.h * 0.5f) - (int)addition;
		}

		public int getParticleCount(EffectItem Ef, int default_cnt)
		{
			if (default_cnt <= 2)
			{
				return default_cnt;
			}
			return X.IntR(X.Mx(2f, (float)default_cnt * (((this.ef_type & EFCON_TYPE.UI) == EFCON_TYPE.NORMAL) ? X.EF_LEVEL_NORMAL : X.EF_LEVEL_UI)));
		}

		public float getParticleSpeed(EffectItem Ef, int default_cnt, float default_maxt)
		{
			float num = 1f;
			if (default_cnt <= 2)
			{
				num *= (((this.ef_type & EFCON_TYPE.UI) == EFCON_TYPE.NORMAL) ? X.EF_LEVEL_NORMAL : X.EF_LEVEL_UI);
			}
			return num * (((this.ef_type & EFCON_TYPE.UI) == EFCON_TYPE.NORMAL) ? 1f : X.EF_TIMESCALE_UI);
		}

		public void killPtc(string key, IEfPInteractale Mv)
		{
			if (this.PtcSTCon != null)
			{
				this.PtcSTCon.killPtc(key, Mv);
			}
		}

		public void clearMeshContent()
		{
			if (this.MeshCon != null)
			{
				this.MeshCon.clear();
			}
		}

		public MeshDrawer MeshInit(string key, float _mesh_base_x, float _mesh_base_y, Color32 C, out bool z_resetted, BLEND blend = BLEND.NORMAL, bool bottom_flag = false)
		{
			return this.MeshInit(key, _mesh_base_x, _mesh_base_y, this.mesh_color.Set(C), MTRX.getMtr(blend, -1), out z_resetted, bottom_flag, null, false);
		}

		public MeshDrawer MeshInit(string key, float _mesh_base_x, float _mesh_base_y, uint rgba, out bool z_resetted, BLEND blend = BLEND.NORMAL, bool bottom_flag = false)
		{
			return this.MeshInit(key, _mesh_base_x, _mesh_base_y, this.mesh_color.Set(rgba), MTRX.getMtr(blend, -1), out z_resetted, bottom_flag, null, false);
		}

		public Vector2 calcMeshXY(float _mesh_base_x, float _mesh_base_y)
		{
			bool flag;
			return this.calcMeshXY(_mesh_base_x, _mesh_base_y, null, out flag);
		}

		public virtual Vector3 calcMeshXY(float _mesh_base_x, float _mesh_base_y, MeshDrawer Md, out bool force_reset_z)
		{
			force_reset_z = false;
			return new Vector3(_mesh_base_x, _mesh_base_y, (Md != null && Md.is_bottom) ? this.bottomBaseZ : this.topBaseZ);
		}

		public virtual void calcParticlePosition(MeshDrawer Md, ref float cx, ref float cy, ref float cz, bool first)
		{
			cy += cz;
			cz = 0f;
		}

		public virtual MeshDrawer MeshInit(string key, float _mesh_base_x, float _mesh_base_y, C32 C, Material Mtr, out bool z_resetted, bool bottom_flag = false, C32 CGrd = null, bool colormatch = false)
		{
			Bench.P("mesh get");
			MeshDrawer meshDrawer = this.MeshCon.Get(key, Mtr, bottom_flag, C.C, CGrd, colormatch, bottom_flag ? this.mesh_min_render_queue : this.mesh_min_render_queue_top);
			Bench.Pend("mesh get");
			Bench.P("mesh calc");
			bool flag;
			Vector3 vector = this.calcMeshXY(_mesh_base_x, _mesh_base_y, meshDrawer, out flag);
			meshDrawer.base_x = vector.x;
			meshDrawer.base_y = vector.y;
			Bench.Pend("mesh calc");
			Bench.P("mesh tri");
			z_resetted = false;
			if (meshDrawer.getTriMax() == 0 || flag)
			{
				meshDrawer.base_z = vector.z;
				meshDrawer.base_z += MTRX.zshift_blend(meshDrawer.getMaterial(), meshDrawer.shader_name) * 0.001f;
				z_resetted = true;
			}
			meshDrawer.pixelsPerUnit = this.ppu;
			Bench.Pend("mesh tri");
			return meshDrawer;
		}

		public MeshDrawer MeshInitGradation(string key, float _mesh_base_x, float _mesh_base_y, uint rgba, uint rgba2, GRD grd, out bool z_resetted, BLEND blend = BLEND.NORMAL, bool bottom_flag = false)
		{
			return this.MeshInit(key, _mesh_base_x, _mesh_base_y, this.mesh_color.Set(rgba), MTRX.getMtr(blend, -1), out z_resetted, bottom_flag, this.mesh_color2.Set(rgba2), true);
		}

		public MeshDrawer MeshInitImg(string key, float _mesh_base_x, float _mesh_base_y, MImage MI, out bool z_resetted, BLEND blend = BLEND.NORMAL, bool bottom_flag = false)
		{
			MeshDrawer meshDrawer = this.MeshInit(key, _mesh_base_x, _mesh_base_y, this.mesh_color.White(), MI.getMtr(blend, -1), out z_resetted, bottom_flag, null, false);
			if (meshDrawer == null)
			{
				return null;
			}
			return meshDrawer.initForImg(MI);
		}

		public void assignMMRDForMeshDrawerContainer(MultiMeshRenderer MMRD)
		{
			if (this.MeshCon != null)
			{
				this.MeshCon.assignMMRD(MMRD);
			}
		}

		public MultiMeshRenderer getMMRDForMeshDrawerContainer()
		{
			if (this.MeshCon == null)
			{
				return null;
			}
			return this.MeshCon.getAssignedMMRD();
		}

		public EffectMeshManager getEffectMeshManager()
		{
			return this.MeshCon;
		}

		public void setPosition(ref float cx, ref float cy, ref float cz)
		{
		}

		public virtual bool isinCameraPtc(EffectItem Ef, float cleft, float cbtm, float cright, float ctop, float camera_check_zm_mul, float camera_check_zm_add)
		{
			return true;
		}

		public bool no_graphic_render
		{
			get
			{
				return (this.ef_type & EFCON_TYPE.NO_GRAPHIC_RENDER) > EFCON_TYPE.NORMAL;
			}
			set
			{
				if (value == this.no_graphic_render)
				{
					return;
				}
				if (value)
				{
					this.ef_type |= EFCON_TYPE.NO_GRAPHIC_RENDER;
					return;
				}
				this.ef_type &= (EFCON_TYPE)253;
			}
		}

		public bool draw_gl_only
		{
			get
			{
				return (this.ef_type & EFCON_TYPE.DRAW_GL_ONLY) > EFCON_TYPE.NORMAL;
			}
			set
			{
				if (value == this.draw_gl_only)
				{
					return;
				}
				if (value)
				{
					if (this.MeshCon != null && this.MeshCon.getAssignedMMRD())
					{
						this.dl("MMRDがアサインされた EF には draw_gl_only を設定できません", false);
						return;
					}
					this.ef_type |= EFCON_TYPE.DRAW_GL_ONLY;
				}
				else
				{
					this.ef_type &= (EFCON_TYPE)251;
				}
				if (this.MeshCon != null)
				{
					this.MeshCon.draw_gl_only = value;
				}
			}
		}

		public void spliceNotActiveMeshes()
		{
			if (this.MeshCon != null)
			{
				this.MeshCon.spliceNotActive();
			}
		}

		public void dl(string s, bool do_not_write_log = false)
		{
			if (do_not_write_log)
			{
				return;
			}
			X.dl("EF<" + this.effect_name + "> : " + s, null, false, false);
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"<Effect>",
				this.effect_name,
				" - L:",
				this.layer_effect_bottom.ToString(),
				",",
				this.layer_effect_top.ToString(),
				" / C:",
				base.Length.ToString()
			});
		}

		public int getTargetLayer(bool is_bottom)
		{
			if (!is_bottom)
			{
				return this.layer_effect_top;
			}
			return this.layer_effect_bottom;
		}

		public Camera CameraForMesh;

		public string effect_name = "EF";

		private EFCON_TYPE ef_type;

		protected PTCThreadRunner PtcSTCon;

		public int MaxCount;

		public float ppu;

		public int layer_effect_top;

		public int layer_effect_bottom;

		public float topBaseZ = 3f;

		public float bottomBaseZ = -0.5f;

		public bool useMeshDrawer = true;

		public int mesh_min_render_queue;

		public int mesh_min_render_queue_top;

		private int AF = 1;

		protected EffectMeshManager MeshCon;

		protected C32 mesh_color;

		protected C32 mesh_color2;

		private bool transformed;

		public static Vector3 BufTrs;

		public readonly GameObject Gob;

		public readonly Transform Trs;

		public bool no_overset_error;

		public Effect<T>.FnCreateEffectItem fnCreateEffectItem;

		public VariableP VarP;

		public delegate T FnCreateEffectItem(IEffectSetter _this, string etype, float _x, float _y, float _z, int _time, int _saf);
	}
}
