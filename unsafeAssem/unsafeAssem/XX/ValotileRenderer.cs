using System;
using System.Collections.Generic;
using UnityEngine;

namespace XX
{
	public class ValotileRenderer : MonoBehaviour, ICameraRenderBinder
	{
		public void Init(Renderer _Mrd, bool _enabled = true)
		{
			this.Mrd = _Mrd;
			base.enabled = _enabled;
			if (this.Mrd != null)
			{
				this.Mrd.enabled = !base.enabled;
			}
			if (this.Md != null)
			{
				this.source_attached = true;
				if (this.Md.Valot != null && this.Md.Valot != this)
				{
					throw new Exception("既にValotが設定されている MeshDrawer が渡されました");
				}
				this.Md.Valot = this;
			}
			if (this.Adraw_tri_i == null)
			{
				this.Adraw_tri_i = new int[1];
			}
			this.Trs = base.transform;
		}

		public static ValotileRenderer Create(MeshDrawer Md, GameObject Gob, bool _enabled = true)
		{
			return ValotileRenderer.Create(Md, IN.GetOrAdd<MeshRenderer>(Gob), true);
		}

		public static ValotileRenderer Create(MeshDrawer Md, MeshRenderer Mrd, bool _enabled = true)
		{
			ValotileRenderer orAdd = IN.GetOrAdd<ValotileRenderer>(Mrd.gameObject);
			orAdd.Init(Md, Mrd, _enabled);
			return orAdd;
		}

		public static bool RecheckUse(MeshDrawer MyMesh, MeshRenderer MyMeshRenderer, ref ValotileRenderer MyValot, bool enable)
		{
			if (MyMeshRenderer != null)
			{
				if (MyValot == null && enable)
				{
					MyValot = ValotileRenderer.Create(MyMesh, MyMeshRenderer, true);
				}
				if (MyValot != null)
				{
					MyValot.enabled = enable;
					return enable;
				}
			}
			return false;
		}

		public static bool RecheckUse(MultiMeshRenderer MMRD, bool enable)
		{
			if (MMRD != null)
			{
				if (!MMRD.use_valotile && enable)
				{
					MMRD.use_valotile = true;
				}
				if (MMRD.use_valotile)
				{
					MMRD.valotile_enabled = enable;
					return enable;
				}
			}
			return false;
		}

		public void Init(MeshDrawer _Md, MeshRenderer _Mrd, bool _enabled = true)
		{
			this.Md = _Md;
			this.Init(_Mrd, _enabled);
		}

		public void Init(MeshDrawer _Md, MeshRenderer _Mrd, CameraBidingsBehaviour _BelongCB)
		{
			this.Init(_Md, _Mrd, true);
			this.BelongCB = _BelongCB;
			this.OnEnable();
		}

		public void InitUI(MeshDrawer _Md, MeshRenderer _Mrd)
		{
			this.Init(_Md, _Mrd, true);
			this.Start();
		}

		public void Init(CameraBidingsBehaviour _BelongCB)
		{
			this.BelongCB = _BelongCB;
			this.OnEnable();
		}

		public void ResortZ()
		{
			if (this.BelongCB != null)
			{
				this.BelongCB.need_sort_binds_post = true;
			}
			if (this.ABelongXC != null)
			{
				for (int i = this.ABelongXC.Count - 1; i >= 0; i--)
				{
					this.ABelongXC[i].need_sort_binds = true;
				}
			}
		}

		public void Start()
		{
			if (this.BelongCB == null && this.ABelongXC == null)
			{
				if (this.ValotConnectable == null || !this.ValotConnectable.connectToBinder(this))
				{
					this.connectUI();
				}
				this.OnEnable();
			}
		}

		public void connectUI()
		{
			if (this.BelongCB != null)
			{
				this.BelongCB.deassignPostRenderFunc(this);
			}
			IN.getGUICamera();
			this.BelongCB = CameraBidingsBehaviour.UiBind;
			if (this.assigned)
			{
				this.BelongCB.assignPostRenderFunc(this);
			}
		}

		public void OnEnable()
		{
			if (this.assigned || this.Trs == null)
			{
				return;
			}
			if (this.BelongCB != null)
			{
				this.BelongCB.assignPostRenderFunc(this);
				this.assigned = true;
			}
			if (this.ABelongXC != null)
			{
				this.assigned = true;
				for (int i = this.ABelongXC.Count - 1; i >= 0; i--)
				{
					this.ABelongXC[i].assignRenderFunc(this);
				}
			}
		}

		public virtual void ReleaseBinding(bool release_link = false, bool no_mesh_resample = false, bool release_mf = false)
		{
			if (!no_mesh_resample)
			{
				this.meshResample();
			}
			this.assigned = false;
			if (this.BelongCB != null)
			{
				this.BelongCB.deassignPostRenderFunc(this);
				if (release_link)
				{
					this.BelongCB = null;
				}
			}
			if (this.ABelongXC != null)
			{
				for (int i = this.ABelongXC.Count - 1; i >= 0; i--)
				{
					this.ABelongXC[i].deassignRenderFunc(this);
				}
				if (release_link)
				{
					this.ABelongXC = null;
				}
			}
		}

		public void Add(XCameraBase XC)
		{
			if (this.ABelongXC == null)
			{
				this.ABelongXC = new List<XCameraBase>(2);
			}
			this.ABelongXC.Add(XC);
			this.assigned = true;
			this._tostring = null;
		}

		public void aliasFrom(ValotileRenderer SrcValot, Material[] AMtr)
		{
			this.SrcAlias = SrcValot;
			this.AMtrForAlias = AMtr;
		}

		public void clearCount()
		{
			for (int i = this.Adraw_tri_i.Length - 1; i >= 0; i--)
			{
				this.Adraw_tri_i[i] = 0;
			}
			this.draw_ver_i = 0;
		}

		public void arraySubtraction(bool temporary = false, MeshDrawerTriMultipleManager TriMulti = null)
		{
			if (this.bounds_clip_ == 1)
			{
				this.bounds_clip_ = 2;
			}
			if (this.valotile)
			{
				this.need_mesh_resample = true;
			}
			this.draw_ver_i = this.Md.getVertexMax();
			if (TriMulti != null)
			{
				if (this.Adraw_tri_i.Length < TriMulti.use_count)
				{
					Array.Resize<int>(ref this.Adraw_tri_i, TriMulti.use_count);
				}
				for (int i = TriMulti.use_count - 1; i >= 0; i--)
				{
					MeshDrawerTriMultipleManager.TMItem tmitem = TriMulti.Get(i);
					this.Adraw_tri_i[i] = ((tmitem == null) ? (-1) : tmitem.tri_max);
				}
				return;
			}
			this.Adraw_tri_i[0] = this.Md.getTriMax();
		}

		public virtual float getFarLength()
		{
			return this.Trs.position.z + ((this.Md != null) ? this.Md.base_z : 0f);
		}

		public void OnDisable()
		{
			if (this.Mrd != null && !this.Mrd.enabled)
			{
				this.Mrd.enabled = true;
			}
			if (this.need_mesh_resample && base.gameObject.activeInHierarchy)
			{
				this.meshResample();
			}
		}

		public void OnDestroy()
		{
			this.ReleaseBinding(true, true, false);
			if (this.Md != null && this.Md.Valot == this)
			{
				this.Md.Valot = null;
			}
			this.Mrd = null;
		}

		public bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
		{
			this.pre_drawn = false;
			if (!base.gameObject.activeInHierarchy || !this.source_attached)
			{
				this.assigned = false;
				return false;
			}
			if (base.enabled)
			{
				if (this.Mrd != null && this.Mrd.enabled)
				{
					this.Mrd.enabled = false;
				}
				Matrix4x4 matrix4x = this.Trs.localToWorldMatrix * (this.use_raw_z ? ValotileRenderer.Mx_z_m1 : ValotileRenderer.Mx_z_zero);
				bool flag;
				if (this.SrcAlias != null)
				{
					flag = this.SrcAlias.Render(ref this.pre_drawn, ref this.bounds_clip_, ref this.RcBounds, JCon, matrix4x, this.AMtrForAlias);
				}
				else
				{
					flag = this.Render(ref this.pre_drawn, ref this.bounds_clip_, ref this.RcBounds, JCon, matrix4x, null);
				}
				if (!flag)
				{
					this.assigned = false;
					return false;
				}
			}
			else
			{
				this.OnDisable();
			}
			return true;
		}

		protected virtual bool Render(ref bool pre_drawn, ref byte bounds_clip_, ref DRect RcBounds, ProjectionContainer JCon, Matrix4x4 Mx, Material[] AMtrForAlias = null)
		{
			if (bounds_clip_ == 2)
			{
				bounds_clip_ = 1;
				RcBounds = this.Md.calcBounds(RcBounds, this.draw_ver_i);
			}
			Mx = JCon.CameraProjectionTransformed * Mx;
			if (bounds_clip_ == 1)
			{
				ValotileRenderer.VBuf.Set(RcBounds.x, RcBounds.y, 0f);
				Vector3 vector = Mx.MultiplyPoint3x4(ValotileRenderer.VBuf);
				float num = vector.x;
				float num2 = vector.x;
				float num3 = vector.y;
				float num4 = vector.y;
				ValotileRenderer.VBuf.Set(RcBounds.x, RcBounds.bottom, 0f);
				vector = Mx.MultiplyPoint3x4(ValotileRenderer.VBuf);
				num = X.Mn(num, vector.x);
				num2 = X.Mx(num2, vector.x);
				num3 = X.Mn(num3, vector.y);
				num4 = X.Mx(num4, vector.y);
				ValotileRenderer.VBuf.Set(RcBounds.right, RcBounds.bottom, 0f);
				vector = Mx.MultiplyPoint3x4(ValotileRenderer.VBuf);
				num = X.Mn(num, vector.x);
				num2 = X.Mx(num2, vector.x);
				num3 = X.Mn(num3, vector.y);
				num4 = X.Mx(num4, vector.y);
				ValotileRenderer.VBuf.Set(RcBounds.right, RcBounds.y, 0f);
				vector = Mx.MultiplyPoint3x4(ValotileRenderer.VBuf);
				num = X.Mn(num, vector.x);
				num2 = X.Mx(num2, vector.x);
				num3 = X.Mn(num3, vector.y);
				num4 = X.Mx(num4, vector.y);
				if (!X.isCovering(-1f, 1f, num, num2, 0f) || !X.isCovering(-1f, 1f, num3, num4, 0f))
				{
					return true;
				}
			}
			GL.LoadProjectionMatrix(Mx);
			pre_drawn = ValotileRenderer.renderOneMd(this.Md, this.Adraw_tri_i, this.Mpb, AMtrForAlias, this.mulitmesh_reverse);
			return pre_drawn;
		}

		public static bool renderOneMd(MeshDrawer Md, int[] Adraw_tri_i = null, MProperty Mpb = null, Material[] AMtrForAlias = null, bool mulitmesh_reverse = false)
		{
			if (Md.hasMultipleTriangle())
			{
				bool flag = false;
				int num = ((Adraw_tri_i == null) ? Md.getSubMeshCount(false) : Adraw_tri_i.Length);
				for (int i = 0; i < num; i++)
				{
					int num2 = (mulitmesh_reverse ? (num - 1 - i) : i);
					Material material = ((AMtrForAlias != null) ? (X.BTW(0f, (float)num2, (float)AMtrForAlias.Length) ? AMtrForAlias[num2] : null) : Md.getSubMeshMaterial(num2));
					if (!(material == null))
					{
						int num3 = -1;
						flag = true;
						if (Adraw_tri_i == null)
						{
							if (Md.draw_gl_only && Md.getCurrentSubMeshIndex() == num2)
							{
								num3 = Md.draw_triangle_count;
							}
							else
							{
								Md.getSubMeshData(num2, ref num3);
							}
						}
						else
						{
							num3 = Adraw_tri_i[num2];
						}
						if (num3 > 0)
						{
							MProperty mproperty = null;
							if (Mpb != null && Mpb.isUseable(num2))
							{
								mproperty = Mpb;
								mproperty.Push(material);
							}
							material.SetPass(0);
							BLIT.RenderToGLImmediate001(Md, num3, num2, false, true, null);
							if (mproperty != null)
							{
								mproperty.Pop(material);
							}
						}
					}
				}
				return flag;
			}
			if (Adraw_tri_i == null || Adraw_tri_i[0] > 0)
			{
				Material material2 = ((AMtrForAlias != null) ? AMtrForAlias[0] : Md.getMaterial());
				if (material2 == null)
				{
					return false;
				}
				int num4 = ((Adraw_tri_i == null) ? (-1) : Adraw_tri_i[0]);
				if (num4 < 0)
				{
					num4 = Md.draw_triangle_count;
				}
				if (num4 > 0)
				{
					MProperty mproperty2 = null;
					if (Mpb != null && Mpb.isUseable(0))
					{
						mproperty2 = Mpb;
						mproperty2.Push(material2);
					}
					material2.SetPass(0);
					BLIT.RenderToGLImmediate(Md, num4);
					if (mproperty2 != null)
					{
						mproperty2.Pop(material2);
					}
				}
			}
			return true;
		}

		private void meshResample()
		{
			if (this.SrcAlias == null && this.need_mesh_resample && this.Md != null)
			{
				this.need_mesh_resample = false;
				int vertexMax = this.Md.getVertexMax();
				int triMax = this.Md.getTriMax();
				this.Md.revertVerAndTriIndex(this.draw_ver_i, this.Adraw_tri_i[0], false);
				if (this.Md.hasMultipleTriangle())
				{
					int num = X.Mn(this.Md.getSubMeshCount(false), this.Adraw_tri_i.Length);
					int currentSubMeshIndex = this.Md.getCurrentSubMeshIndex();
					for (int i = 0; i < num; i++)
					{
						int num2 = this.Adraw_tri_i[i];
						if (num2 >= 0)
						{
							this.Md.chooseSubMesh(i, false, false);
							this.Md.revertVerAndTriIndex(this.draw_ver_i, num2, false);
						}
					}
					this.Md.chooseSubMesh(currentSubMeshIndex, false, false);
				}
				ValotileRenderer.resampling = true;
				this.Md.updateForMeshRenderer(true);
				ValotileRenderer.resampling = false;
				this.Md.revertVerAndTriIndex(vertexMax, triMax, false);
			}
		}

		public bool bounds_clip
		{
			get
			{
				return this.bounds_clip_ > 0;
			}
			set
			{
				this.bounds_clip_ = (value ? 2 : 0);
			}
		}

		public bool valotile
		{
			get
			{
				return !ValotileRenderer.resampling && base.gameObject.activeInHierarchy && base.enabled;
			}
		}

		public bool isAssigned()
		{
			return this.assigned;
		}

		public string getInfo(string delimiter = "\n")
		{
			if (this.BelongCB != null)
			{
				if (this.BelongCB == CameraBidingsBehaviour.UiBind)
				{
					return "UI binding";
				}
				return "BelongCB binding: =>" + this.BelongCB.name;
			}
			else
			{
				if (this.ABelongXC != null)
				{
					STB stb = TX.PopBld(null, 0);
					int count = this.ABelongXC.Count;
					for (int i = 0; i < count; i++)
					{
						stb.Add("=>" + this.ABelongXC[i].key, delimiter);
					}
					string text = stb.Add(delimiter, " / assigned to ", count.ToString(), " XC").ToString();
					TX.ReleaseBld(stb);
					return text;
				}
				return "Target is missing";
			}
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				STB stb = TX.PopBld(null, 0);
				this._tostring = stb.Add("Valot(", base.gameObject.name, ")- ", this.getInfo(" & ")).ToString();
				TX.ReleaseBld(stb);
			}
			return this._tostring;
		}

		protected Renderer Mrd;

		protected MeshDrawer Md;

		public MProperty Mpb;

		protected bool source_attached;

		private CameraBidingsBehaviour BelongCB;

		private List<XCameraBase> ABelongXC;

		protected Transform Trs;

		private int[] Adraw_tri_i;

		public ValotileRenderer.IValotConnetcable ValotConnectable;

		private int draw_ver_i;

		private bool assigned;

		public bool use_raw_z;

		private byte bounds_clip_;

		private DRect RcBounds;

		private string _tostring;

		public bool pre_drawn;

		public bool mulitmesh_reverse;

		private Material[] AMtrForAlias;

		private ValotileRenderer SrcAlias;

		private bool need_mesh_resample;

		private static bool resampling;

		public static Matrix4x4 Mx_z_zero = Matrix4x4.Scale(new Vector3(1f, 1f, 0f));

		public static Matrix4x4 Mx_z_m1 = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));

		private static Vector3 VBuf = Vector3.zero;

		public interface IValotConnetcable
		{
			bool connectToBinder(ValotileRenderer Target);

			XCameraBase assignRenderFuncXC(ICameraRenderBinder Fn, int layer, bool assign_only_last = false, ValotileRenderer ValotAssigning = null);

			void deassignRenderFunc(ICameraRenderBinder Fn, int layer);
		}
	}
}
