using System;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2MeshContainer : MultiMeshRenderer
	{
		public override void Awake()
		{
			base.Awake();
		}

		public static void prepareMeshContainer(GameObject Gob, Map2d _Mp, M2SubMap _SM, ref M2MeshContainer MMRD, bool stabilize_flag, M2DBase M2D)
		{
			bool flag = Map2d.editor_decline_lighting || stabilize_flag || true;
			if (MMRD != null && MMRD.stabilize_draw != flag)
			{
				if (_Mp == null)
				{
					IN.DestroyOne(MMRD);
				}
				else
				{
					IN.DestroyOne(_Mp.get_MMRD());
					_Mp.releaseMeshLink(false, false);
				}
				MMRD = null;
			}
			if (MMRD == null)
			{
				MMRD = Gob.AddComponent<M2MeshContainer>();
			}
			MMRD.use_valotile = false;
			MMRD.use_cache = Map2d.editor_decline_lighting;
			MMRD.SM = _SM;
			MMRD.stabilize_draw = flag;
			MMRD.is_camera = _Mp == null;
			MMRD.Mp = _Mp;
			MMRD.M2D = M2D;
		}

		public void CreateMesh(ref MdMap Md, string name_suffix, float base_z, Material Mtr, int layer, bool always_show = false, bool allow_simplify = false, bool force_stabilize = true, bool stabilize_mesh_no_make = false)
		{
			int num = ((Md == null) ? (-1) : X.isinC<MeshDrawer>(this.AMesh, Md));
			if (num == -1)
			{
				force_stabilize = force_stabilize || this.stabilize_draw;
				if (this.AMd2 == null)
				{
					this.AMd2 = new MdMap[12];
				}
				if (force_stabilize || !stabilize_mesh_no_make)
				{
					Md = new MdMap(this, this.now_gob_i, null);
					base.Make(MTRX.ColGray, BLEND.NORMAL, Mtr, Md);
					num = this.now_gob_i - 1;
					this.useable_bits |= 1U << num;
					this.stabilize_bits |= (force_stabilize ? (1U << num) : 0U);
					X.pushToEmptyS<MdMap>(ref this.AMd2, Md, ref num, 1);
					Md.connectRendererToTriMulti(base.GetMeshRenderer(Md));
					num--;
				}
				else
				{
					Md = new MdMap(this, this.now_gob_i, null);
					base.allocCapacity(-1);
					num = this.now_gob_i;
					this.AMesh[this.now_gob_i] = Md;
					X.pushToEmptyS<MdMap>(ref this.AMd2, Md, ref this.now_gob_i, 1);
				}
				Md.activateM2((this.is_camera ? "Cam-" : this.Mp.key) + "_" + num.ToString(), Mtr);
			}
			else
			{
				this.setMaterial(num, Mtr, false);
				force_stabilize = (this.stabilize_bits & (1U << num)) > 0U;
			}
			Md.allow_simplify = allow_simplify;
			if (always_show)
			{
				this.no_auto_hide_bits |= 1U << num;
				Md.use_cache = true;
			}
			else
			{
				Md.use_cache = this.use_cache;
			}
			Md.dep_layer = layer;
			Md.base_z = (force_stabilize ? 0f : base_z);
			GameObject gameObject = this.AGob[num];
			if (gameObject != null)
			{
				if (this.Mp != null)
				{
					gameObject.isStatic = this.Mp.gameObject == null || this.Mp.gameObject.isStatic;
					gameObject.name = this.Mp.key + " (MpMMRD)" + name_suffix;
				}
				else
				{
					gameObject.name = "MpMMRD- " + name_suffix;
				}
				gameObject.layer = layer;
				if (base_z < 0f)
				{
					IN.setZAbs(gameObject.transform, -base_z);
					return;
				}
				IN.setZ(gameObject.transform, base_z);
			}
		}

		public void Valotize(M2Camera Cam, bool use_clip = false)
		{
			int length = base.Length;
			this.ValotConnetcable_ = Cam;
			this.AValo = new ValotileRenderer[this.AMesh.Length];
			for (int i = 0; i < length; i++)
			{
				MdMap mdMap = this.AMd2[i];
				if (mdMap != null && !mdMap.isEmpty())
				{
					MeshRenderer meshRenderer = base.GetMeshRenderer(i);
					ValotileRenderer valotileRenderer;
					if (!IN.GetOrAdd<ValotileRenderer>(meshRenderer.gameObject, out valotileRenderer))
					{
						valotileRenderer.ReleaseBinding(false, true, false);
					}
					this.AValo[i] = valotileRenderer;
					valotileRenderer.Init(mdMap, meshRenderer, true);
					valotileRenderer.mulitmesh_reverse = mdMap.reverse_mesh_simplify;
					if (use_clip)
					{
						valotileRenderer.bounds_clip = true;
					}
					Cam.assignRenderFunc(valotileRenderer, valotileRenderer.gameObject.layer, false, valotileRenderer);
				}
			}
		}

		public override MeshDrawer Make(Color32 col, BLEND blend = BLEND.NORMAL, Material Mtr = null, MeshDrawer Md = null)
		{
			return base.Make(col, blend, Mtr, Md);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			this.simplified_bits = 0U;
			this.useable_bits = (this.stabilize_bits = 0U);
			this.AMd2 = null;
		}

		public void clearBit(params MdMap[] AMd)
		{
			int num = AMd.Length;
			for (int i = 0; i < num; i++)
			{
				int num2 = X.isinC<MdMap>(this.AMd2, AMd[i]);
				if (num2 >= 0)
				{
					this.simplified_bits &= ~(1U << num2);
					this.useable_bits |= 1U << num2;
				}
			}
		}

		public MdMap getUnsitabilizeMesh(int i)
		{
			return this.AMd2[i];
		}

		public void finishReentry()
		{
			for (int i = 0; i < this.now_gob_i; i++)
			{
				MeshDrawer meshDrawer = this.AMesh[i];
			}
		}

		protected override void InitMeshFilterArray()
		{
			base.InitMeshFilterArray();
		}

		protected override void InitMeshRendererArray()
		{
			base.InitMeshRendererArray();
		}

		public void transformCamTrace(MeshDrawer Md)
		{
			int index = base.getIndex(Md);
			if (index >= 0)
			{
				if (((ulong)this.stabilize_bits & (ulong)(1L << (index & 31))) != 0UL)
				{
					this.M2D.Cam.addTransformWithCam(base.GetGob(Md).transform);
				}
				this.AMd2[index].transform_cam_trace = true;
			}
		}

		public override MultiMeshRenderer setMaterial(int i, Material Mtr, bool no_replace_mesh_mtr = false)
		{
			if (this.AGob[i] != null)
			{
				base.setMaterial(i, Mtr, no_replace_mesh_mtr);
			}
			else
			{
				this.AMesh[i].setMaterial(Mtr, false);
				this.AMtr[i] = Mtr;
			}
			return this;
		}

		public void protectSimplifiedTextureWhenClosing()
		{
			if (this.AMd2 == null)
			{
				return;
			}
			int now_gob_i = this.now_gob_i;
			for (int i = 0; i < now_gob_i; i++)
			{
				this.AMd2[i].MeshReleaseToEmpty();
				this.AMd2[i].releaseTextureWithoutReleasingMemory();
			}
			this.simplified_bits = 0U;
		}

		public void clearRendered()
		{
			if (this.AMd2 == null)
			{
				return;
			}
			int now_gob_i = this.now_gob_i;
			for (int i = 0; i < now_gob_i; i++)
			{
				this.AMd2[i].releaseTexture();
			}
			this.simplified_bits = 0U;
		}

		internal MeshDrawer PoolSimplifyMesh(MdMap MdFor, bool erasing = false)
		{
			MeshDrawer meshDrawer = M2MeshContainer.PoolMdSimplify.Pool();
			if (!erasing)
			{
				this.setMaterialForSimplify(meshDrawer);
			}
			else
			{
				meshDrawer.setMaterial(this.M2D.IMGS.MIchip.getMtr(MTRX.ShaderGDTErase, -1), false);
			}
			meshDrawer.Identity();
			meshDrawer.activation_key = "_simplify";
			return meshDrawer;
		}

		internal void setMaterialForSimplify(MeshDrawer MdCacheSimplify)
		{
			MdCacheSimplify.setMaterial(this.M2D.curMap.Dgn.getMaterialForSimplifySubMap(MdCacheSimplify), false);
		}

		internal MeshDrawer ReleaseSimplifyMesh(MeshDrawer Md)
		{
			if (Md != null)
			{
				Md.clear(false, true);
				Md.setMaterial(null, false);
				Md.MeshReleaseToEmpty();
				M2MeshContainer.PoolMdSimplify.Release(Md);
			}
			return null;
		}

		public void Remove(ref MdMap Md)
		{
			if (Md != null)
			{
				Md.releaseTexture();
				base.DestroyOne(Md);
			}
			Md = null;
		}

		public void fineActivateState(M2SubMap SM, bool deleting = false)
		{
			bool flag = SM != null && SM.temporary_duped;
			int now_gob_i = this.now_gob_i;
			for (int i = 0; i < now_gob_i; i++)
			{
				if ((this.no_auto_hide_bits & (1U << i)) == 0U)
				{
					MdMap mdMap = this.AMd2[i];
					if (!mdMap.isEmpty() || Map2d.editor_decline_lighting)
					{
						this.useable_bits |= 1U << i;
						if (this.AGob[i] != null)
						{
							this.AGob[i].SetActive((this.stabilize_bits & (1U << i)) > 0U);
							if (SM == null && (this.stabilize_bits & (1U << i)) == 0U)
							{
								mdMap.assignRenderFuncToCam();
							}
						}
						if (!flag && (this.simplified_bits & (1U << i)) == 0U && mdMap.simplifyExecution(SM))
						{
							this.simplified_bits |= 1U << i;
							if ((this.stabilize_bits & (1U << i)) != 0U && SM == null)
							{
								this.setMaterial(i, mdMap.getMaterial(), true);
							}
						}
					}
					else
					{
						this.useable_bits &= ~(1U << i);
						mdMap.releaseTexture();
						if (this.AGob[i] != null)
						{
							if (deleting)
							{
								IN.DestroyOne(this.AGob[i]);
								this.AGob[i] = null;
								if (this.AMrd != null)
								{
									this.AMrd[i] = null;
								}
							}
							else
							{
								this.AGob[i].SetActive(false);
							}
						}
					}
				}
			}
			if (!X.DEBUGSTABILIZE_DRAW && !Map2d.editor_decline_lighting)
			{
				this.Valotize(M2DBase.Instance.Cam, this.Mp.is_submap);
				return;
			}
			base.use_valotile = false;
		}

		public DRect GetRcPreDefined(int i, bool no_make = false)
		{
			if (this.Mp != null)
			{
				return this.Mp.GetRcPreDefined(i, no_make);
			}
			if (!no_make)
			{
				return new DRect("", 0f, 0f, 0f, 0f, 0f);
			}
			return null;
		}

		public void fineMaterialNightColor(M2SubMap CheckSub)
		{
			int num = this.AMd2.Length;
			for (int i = 0; i < num; i++)
			{
				MdMap mdMap = this.AMd2[i];
				if (mdMap != null)
				{
					int subMeshCount = mdMap.getSubMeshCount(false);
					MeshRenderer meshRenderer = base.GetMeshRenderer(i);
					Material[] array = ((meshRenderer != null) ? meshRenderer.sharedMaterials : null);
					if (array != null && array.Length != subMeshCount)
					{
						array = new Material[subMeshCount];
					}
					for (int j = 0; j < subMeshCount; j++)
					{
						if (subMeshCount > 1)
						{
							mdMap.chooseSubMesh(j, false, false);
						}
						Material material = mdMap.getMaterial();
						if (material != null)
						{
							this.Mp.fineMaterialSubMapAttribute(mdMap, material, CheckSub, false);
							if (array != null)
							{
								array[j] = material;
							}
						}
					}
					if (subMeshCount > 1)
					{
						mdMap.chooseSubMesh(1, false, false);
					}
					if (meshRenderer != null)
					{
						meshRenderer.sharedMaterials = array;
					}
				}
			}
		}

		public override MultiMeshRenderer activate()
		{
			for (int i = 0; i < this.now_gob_i; i++)
			{
				if ((this.useable_bits & (1U << i)) != 0U)
				{
					try
					{
						base.GetMeshRenderer(i).enabled = true;
					}
					catch
					{
						X.de(" 削除されたMrdに対する activate : " + i.ToString(), null);
					}
				}
			}
			return this;
		}

		public override void DestroyOne(int i)
		{
			if (this.AMd2 != null && X.BTW(0f, (float)i, (float)this.now_gob_i))
			{
				if (this.AMd2[i] != null)
				{
					this.AMd2[i].clear(false, false);
				}
				X.shiftEmpty<MdMap>(this.AMd2, 1, i, -1);
			}
			base.DestroyOne(i);
		}

		public M2MeshContainer Identity()
		{
			M2MeshContainer.MatrixTransform = Matrix4x4.identity;
			return this;
		}

		public M2MeshContainer Scale(float scalex, float scaley, bool pre_transform = false)
		{
			M2MeshContainer.MatrixTransform = (pre_transform ? (M2MeshContainer.MatrixTransform * Matrix4x4.Scale(new Vector3(scalex, scaley, 1f))) : (Matrix4x4.Scale(new Vector3(scalex, scaley, 1f)) * M2MeshContainer.MatrixTransform));
			return this;
		}

		public M2MeshContainer Rotate(float rotR, bool pre_transform = false)
		{
			Matrix4x4 matrix4x = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, rotR / 3.1415927f * 180f));
			M2MeshContainer.MatrixTransform = (pre_transform ? (M2MeshContainer.MatrixTransform * matrix4x) : (matrix4x * M2MeshContainer.MatrixTransform));
			return this;
		}

		public M2MeshContainer Rotate01(float rot01, bool pre_transform = false)
		{
			return this.Rotate(rot01 * 6.2831855f, pre_transform);
		}

		public M2MeshContainer Translate(float translatex, float translatey, bool pre_transform = false)
		{
			M2MeshContainer.MatrixTransform = (pre_transform ? (M2MeshContainer.MatrixTransform * Matrix4x4.Translate(new Vector3(translatex, translatey, 0f))) : (Matrix4x4.Translate(new Vector3(translatex, translatey, 0f)) * M2MeshContainer.MatrixTransform));
			return this;
		}

		public M2MeshContainer Translate(Vector3 V, bool pre_transform = false)
		{
			M2MeshContainer.MatrixTransform = (pre_transform ? (M2MeshContainer.MatrixTransform * Matrix4x4.Translate(V)) : (Matrix4x4.Translate(V) * M2MeshContainer.MatrixTransform));
			return this;
		}

		public int getLayer(MeshDrawer Md)
		{
			return this.getLayer(base.getIndex(Md));
		}

		public bool isSimplified(int i)
		{
			return ((ulong)this.simplified_bits & (ulong)(1L << (i & 31))) > 0UL;
		}

		public int getLayer(int i)
		{
			if (i < 0)
			{
				return base.gameObject.layer;
			}
			GameObject gameObject = this.AGob[i];
			if (gameObject != null)
			{
				return gameObject.layer;
			}
			if (this.AMd2 != null)
			{
				return this.AMd2[i].dep_layer;
			}
			return base.gameObject.layer;
		}

		public Map2d getMap()
		{
			return this.Mp;
		}

		public bool hasSimplified()
		{
			return this.simplified_bits > 0U;
		}

		public bool stabilize_draw;

		public bool is_camera;

		private MdMap[] AMd2;

		private Map2d Mp;

		public M2DBase M2D;

		public M2SubMap SM;

		public uint useable_bits;

		public uint no_auto_hide_bits;

		public uint stabilize_bits;

		public uint simplified_bits;

		public static Matrix4x4 MatrixTransform;

		private static ClsPool<MeshDrawer> PoolMdSimplify = new ClsPool<MeshDrawer>(delegate
		{
			MeshDrawer meshDrawer = new MeshDrawer(null, 4, 6);
			meshDrawer.draw_gl_only = true;
			meshDrawer.activate("_simplify", MTRX.MtrMeshNormal, false, MTRX.ColWhite, null);
			return meshDrawer;
		}, 6);
	}
}
