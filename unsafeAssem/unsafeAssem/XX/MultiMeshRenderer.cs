using System;
using GGEZ;
using UnityEngine;

namespace XX
{
	public class MultiMeshRenderer : MonoBehaviour, IValotileSetable
	{
		public virtual void Awake()
		{
			if (this.AGob != null)
			{
				return;
			}
			this.AGob = new GameObject[8];
			this.AMesh = new MeshDrawer[8];
			this.AMtr = new Material[8];
			base.enabled = false;
		}

		public MultiMeshRenderer Queue(int _queue)
		{
			this.queue = _queue;
			return this;
		}

		public MultiMeshRenderer BaseZ(float z)
		{
			this.base_z = z;
			return this;
		}

		public MeshDrawer Make(Material Mtr)
		{
			return this.Make(MTRX.ColWhite, BLEND.NORMAL, Mtr, null);
		}

		public MeshDrawer Make(BLEND blend, MImage MI)
		{
			Material mtr = MI.getMtr(blend, this.stencil_ref);
			MeshDrawer meshDrawer = this.Make(MTRX.ColWhite, blend, mtr, null);
			meshDrawer.initForImg(MI);
			return meshDrawer;
		}

		protected void allocCapacity(int c = -1)
		{
			if (c < 0)
			{
				c = this.now_gob_i;
			}
			if (c >= this.AGob.Length)
			{
				Array.Resize<GameObject>(ref this.AGob, c * 2);
				Array.Resize<MeshDrawer>(ref this.AMesh, c * 2);
				Array.Resize<Material>(ref this.AMtr, c * 2);
				if (this.AMrd != null)
				{
					Array.Resize<MeshRenderer>(ref this.AMrd, c * 2);
				}
				if (this.AMf != null)
				{
					Array.Resize<MeshFilter>(ref this.AMf, c * 2);
				}
				if (this.AMda != null)
				{
					Array.Resize<MdArranger>(ref this.AMda, c * 2);
				}
				if (this.AValo != null)
				{
					Array.Resize<ValotileRenderer>(ref this.AValo, c * 2);
				}
				if (this.AMpb != null)
				{
					Array.Resize<MProperty>(ref this.AMpb, c * 2);
				}
			}
		}

		public virtual MeshDrawer Make(Color32 col, BLEND blend = BLEND.NORMAL, Material Mtr = null, MeshDrawer Md = null)
		{
			if (this.AGob == null)
			{
				this.Awake();
			}
			if (Mtr == null)
			{
				Mtr = MTRX.getMtr(blend, this.stencil_ref);
			}
			this.allocCapacity(-1);
			string[] array = new string[5];
			array[0] = "(";
			array[1] = base.gameObject.name;
			array[2] = "-MF-";
			int num = 3;
			int num2 = this.created_count;
			this.created_count = num2 + 1;
			array[num] = num2.ToString();
			array[4] = ")";
			GameObject gameObject = new GameObject(string.Concat(array));
			gameObject.layer = base.gameObject.layer;
			gameObject.transform.SetParent(base.transform, false);
			this.AGob[this.now_gob_i] = gameObject;
			if (this.z_apply_to_gameobject)
			{
				gameObject.transform.localPosition = new Vector3(0f, 0f, this.base_z);
			}
			if (Md == null)
			{
				Md = new MeshDrawer(null, 4, 6);
			}
			Md = MeshDrawer.prepareMeshRenderer(gameObject, Mtr, this.z_apply_to_gameobject ? 0f : this.base_z, this.queue, Md, false, false);
			Mtr = (this.AMtr[this.now_gob_i] = Md.getMaterial());
			this.aliased_bits &= ~(1U << this.now_gob_i);
			if (this.AMrd != null)
			{
				this.AMrd[this.now_gob_i] = null;
			}
			this.AMesh[this.now_gob_i] = Md;
			if (Mtr.shader.name == "Hachan/ShaderGDTBorder8" || Mtr.shader.name == "Hachan/ShaderGDTSTBorder8")
			{
				Md.allocUv2(64, false).allocUv3(64, false);
			}
			if (this.use_cache)
			{
				Md.use_cache = true;
			}
			Md.Col = col;
			this.base_z -= this.slip_z;
			if (this.AMda != null)
			{
				this.AMda[this.now_gob_i] = new MdArranger(Md).Set(false);
			}
			this.now_gob_i++;
			if (this.AValo != null)
			{
				ValotileRenderer valotileRenderer = (this.AValo[this.now_gob_i - 1] = IN.GetOrAdd<ValotileRenderer>(gameObject));
				this.ValoInit(valotileRenderer, Md, this.GetMeshRenderer(this.now_gob_i - 1));
				if (this.AMpb != null)
				{
					valotileRenderer.Mpb = ((this.AMpb.Length >= this.now_gob_i) ? this.AMpb[this.now_gob_i - 1] : null);
					if (valotileRenderer.Mpb != null)
					{
						valotileRenderer.Mpb.Clear(true);
					}
				}
			}
			return Md;
		}

		public MultiMeshRenderer CopyTo(MultiMeshRenderer Dest, int replace_stencil_ref = -1)
		{
			int length = Dest.Length;
			Dest.stencil_ref = replace_stencil_ref;
			this.InitMeshRendererArray();
			Dest.InitMeshFilterArray();
			Dest.InitMeshRendererArray();
			for (int i = 0; i < this.now_gob_i; i++)
			{
				GameObject gob = this.GetGob(i);
				Material material = MTRX.replaceST(this.getMaterial(i), replace_stencil_ref);
				MeshDrawer meshDrawer = this.AMesh[i];
				Dest.Make(meshDrawer.Col, BLEND.NORMAL, material, null).base_z = meshDrawer.base_z;
				Dest.MeshReplace(length + i, meshDrawer, this.AMrd[i], false, true);
				Dest.GetGob(length + i).transform.position = gob.transform.position;
			}
			return Dest;
		}

		public MeshDrawer Get(int i)
		{
			return this.AMesh[i];
		}

		public int Length
		{
			get
			{
				return this.now_gob_i;
			}
		}

		public GameObject GetGob(int i)
		{
			return this.AGob[i];
		}

		public MeshDrawer Get(Mesh Ms)
		{
			int index = this.getIndex(Ms);
			if (index < 0)
			{
				return null;
			}
			return this.AMesh[index];
		}

		public GameObject GetGob(MeshDrawer Md)
		{
			int num = X.isinC<MeshDrawer>(this.AMesh, Md);
			if (!X.BTW(0f, (float)num, (float)this.now_gob_i))
			{
				return null;
			}
			return this.AGob[num];
		}

		public MeshDrawer Get(Material Mtr)
		{
			int num = X.isinC<Material>(this.AMtr, Mtr);
			if (!X.BTW(0f, (float)num, (float)this.now_gob_i))
			{
				return null;
			}
			return this.AMesh[num];
		}

		public MultiMeshRenderer MeshReplace(int i, MeshDrawer RepTo, MeshRenderer RepToMrd, bool copy_me_not_material_paramator = false, bool destruct_mesh = false)
		{
			if (!X.BTW(0f, (float)i, (float)this.now_gob_i) || RepTo == null)
			{
				return this;
			}
			this.AMesh[i].MeshReplace(RepTo, copy_me_not_material_paramator, destruct_mesh);
			this.InitMeshFilterArray();
			this.InitMeshRendererArray();
			if (this.AMf == null)
			{
				return this;
			}
			MeshFilter meshFilter = this.AMf[i];
			if (meshFilter == null)
			{
				meshFilter = (this.AMf[i] = this.AGob[i].GetComponent<MeshFilter>());
			}
			if (RepToMrd != null)
			{
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				RepToMrd.GetPropertyBlock(materialPropertyBlock);
				this.GetMeshRenderer(i).SetPropertyBlock(materialPropertyBlock);
			}
			meshFilter.sharedMesh = RepTo.getMesh();
			return this;
		}

		protected virtual void InitMeshFilterArray()
		{
			if (this.AMf == null)
			{
				this.AMf = new MeshFilter[this.AMesh.Length];
				for (int i = 0; i < this.now_gob_i; i++)
				{
					this.AMf[i] = this.AGob[i].GetComponent<MeshFilter>();
				}
			}
		}

		protected virtual void InitMeshRendererArray()
		{
			if (this.AMrd == null)
			{
				this.AMrd = new MeshRenderer[this.AMesh.Length];
				for (int i = 0; i < this.now_gob_i; i++)
				{
					GameObject gameObject = this.AGob[i];
					this.AMrd[i] = ((gameObject != null) ? gameObject.GetComponent<MeshRenderer>() : null);
				}
			}
		}

		public MultiMeshRenderer SetRenderQueue(MeshDrawer Md, int render_queue)
		{
			return this.SetRenderQueue(X.isinC<MeshDrawer>(this.AMesh, Md), render_queue);
		}

		public MultiMeshRenderer SetRenderQueue(int i, int render_queue)
		{
			if (!X.BTW(0f, (float)i, (float)this.now_gob_i))
			{
				return this;
			}
			MeshDrawer meshDrawer = this.AMesh[i];
			if (meshDrawer.render_queue != render_queue)
			{
				meshDrawer.render_queue = render_queue;
				this.setMaterial(i, meshDrawer.getMaterial(), true);
			}
			return this;
		}

		public MProperty preparePropertyBlock(int i, int mpb_target_id = -1, bool no_make = false)
		{
			if (!X.BTW(0f, (float)i, (float)this.now_gob_i))
			{
				return null;
			}
			if (this.AMpb == null)
			{
				this.AMpb = new MProperty[this.AMesh.Length];
			}
			MProperty mproperty = this.AMpb[i];
			if (mproperty == null)
			{
				if (no_make)
				{
					return null;
				}
				mproperty = (this.AMpb[i] = new MProperty(this.GetMeshRenderer(i), -1));
			}
			mproperty.mpb_target_id = mpb_target_id;
			if (this.AValo != null)
			{
				ValotileRenderer valotileRenderer = this.AValo[i];
				if (valotileRenderer != null)
				{
					valotileRenderer.Mpb = mproperty;
				}
			}
			return mproperty;
		}

		public void DestroyOne(MeshDrawer Md)
		{
			if (Md == null)
			{
				return;
			}
			this.DestroyOne(X.isinC<MeshDrawer>(this.AMesh, Md));
		}

		public void DestroyOne(GameObject Gob)
		{
			if (Gob == null)
			{
				return;
			}
			this.DestroyOne(X.isinC<GameObject>(this.AGob, Gob));
		}

		public virtual void DestroyOne(int i)
		{
			if (!X.BTW(0f, (float)i, (float)this.now_gob_i))
			{
				return;
			}
			try
			{
				IN.DestroyOne(this.AGob[i]);
			}
			catch
			{
			}
			MeshDrawer meshDrawer = this.AMesh[i];
			if (meshDrawer != null)
			{
				meshDrawer.destruct();
			}
			X.shiftEmpty<GameObject>(this.AGob, 1, i, -1);
			X.shiftEmpty<MeshDrawer>(this.AMesh, 1, i, -1);
			X.shiftEmpty<Material>(this.AMtr, 1, i, -1);
			if (this.AMrd != null)
			{
				X.shiftEmpty<MeshRenderer>(this.AMrd, 1, i, -1);
			}
			if (this.AMf != null)
			{
				X.shiftEmpty<MeshFilter>(this.AMf, 1, i, -1);
			}
			if (this.AMda != null)
			{
				X.shiftEmpty<MdArranger>(this.AMda, 1, i, -1);
			}
			this.now_gob_i--;
		}

		public int getLength()
		{
			return this.now_gob_i;
		}

		public MeshRenderer GetMeshRenderer(MeshDrawer Md)
		{
			return this.GetMeshRenderer(X.isinC<MeshDrawer>(this.AMesh, Md));
		}

		public MeshRenderer GetMeshRenderer(int i)
		{
			if (i < 0)
			{
				return null;
			}
			this.InitMeshRendererArray();
			if (this.AMrd == null)
			{
				return null;
			}
			MeshRenderer meshRenderer = this.AMrd[i];
			if (meshRenderer == null && this.AGob[i] != null)
			{
				meshRenderer = (this.AMrd[i] = this.AGob[i].GetComponent<MeshRenderer>());
			}
			return meshRenderer;
		}

		public ValotileRenderer GetValotileRenderer(MeshDrawer Md)
		{
			if (!this.use_valotile)
			{
				return null;
			}
			return this.GetValotileRenderer(X.isinC<MeshDrawer>(this.AMesh, Md));
		}

		public ValotileRenderer GetValotileRenderer(int i)
		{
			if (i >= 0 && this.use_valotile)
			{
				return this.AValo[i];
			}
			return null;
		}

		public MdArranger GetArranger(MeshDrawer Md)
		{
			return this.GetArranger(X.isinC<MeshDrawer>(this.AMesh, Md));
		}

		public MdArranger GetArranger(int i)
		{
			if (X.BTW(0f, (float)i, (float)this.now_gob_i))
			{
				if (this.AMda == null)
				{
					this.AMda = new MdArranger[this.AGob.Length];
				}
				if (this.AMda[i] == null)
				{
					(this.AMda[i] = new MdArranger(this.AMesh[i])).SetWhole(true);
				}
				return this.AMda[i];
			}
			return null;
		}

		public Camera initForRenderBuffer(int i, RenderTexture Img, int layer, Color32 BaseCol, Camera BaseCamera = null)
		{
			if (!X.BTW(0f, (float)i, (float)this.now_gob_i))
			{
				return null;
			}
			return MultiMeshRenderer.MakeRenderBufferAndCamera(Img, layer, BaseCol, BaseCamera, this.AGob[i], false);
		}

		public Camera initForRenderBuffer(int i, int width, int height, int layer, Color32 BaseCol, Camera BaseCamera = null)
		{
			if (!X.BTW(0f, (float)i, (float)this.now_gob_i))
			{
				return null;
			}
			return MultiMeshRenderer.MakeRenderBufferAndCamera(width, height, layer, BaseCol, BaseCamera, this.AGob[i], false);
		}

		public Camera initForRenderBuffer(MeshDrawer Md, RenderTexture Img, int layer, Color32 BaseCol, Camera BaseCamera = null)
		{
			return this.initForRenderBuffer(X.isinC<MeshDrawer>(this.AMesh, Md), Img, layer, BaseCol, BaseCamera);
		}

		public Camera initForRenderBuffer(MeshDrawer Md, int width, int height, int layer, Color32 BaseCol, Camera BaseCamera = null)
		{
			return this.initForRenderBuffer(X.isinC<MeshDrawer>(this.AMesh, Md), width, height, layer, BaseCol, BaseCamera);
		}

		public MultiMeshRenderer setMaterial(MeshDrawer Md, Material Mtr, bool no_replace_mesh_mtr = false)
		{
			return this.setMaterial(X.isinC<MeshDrawer>(this.AMesh, Md), Mtr, no_replace_mesh_mtr);
		}

		public virtual MultiMeshRenderer setMaterial(int i, Material Mtr, bool no_replace_mesh_mtr = false)
		{
			if (i >= 0)
			{
				this.AMtr[i] = Mtr;
				if (!no_replace_mesh_mtr)
				{
					this.AMesh[i].setMaterial(Mtr, false);
				}
				this.InitMeshRendererArray();
				if (this.AMrd == null || this.AGob[i] == null)
				{
					return this;
				}
				MeshRenderer meshRenderer = this.AMrd[i];
				if (meshRenderer == null)
				{
					meshRenderer = (this.AMrd[i] = this.AGob[i].GetComponent<MeshRenderer>());
				}
				Material[] materialArray = this.AMesh[i].getMaterialArray(false);
				int num = materialArray.Length;
				meshRenderer.sharedMaterials = materialArray;
			}
			return this;
		}

		public Material getMaterial(MeshDrawer Md)
		{
			return this.getMaterial(X.isinC<MeshDrawer>(this.AMesh, Md));
		}

		public MultiMeshRenderer replaceAllMaterial(MultiMeshRenderer Src, bool consider_multiple_tri = false)
		{
			int num = X.Mn(Src.Length, this.Length);
			for (int i = 0; i < num; i++)
			{
				if (consider_multiple_tri)
				{
					MeshDrawer meshDrawer = Src.Get(i);
					this.Get(i).copyMultipleTriangleAndMaterial(meshDrawer, false);
					this.setMaterial(i, this.getMaterial(i), true);
				}
				else
				{
					this.setMaterial(i, Src.getMaterial(i), false);
				}
			}
			return this;
		}

		public Material getMaterial(int i)
		{
			if (i < 0)
			{
				return null;
			}
			return this.AMtr[i];
		}

		public int getIndex(MeshDrawer Md)
		{
			for (int i = 0; i < this.now_gob_i; i++)
			{
				if (this.AMesh[i] == Md)
				{
					return i;
				}
			}
			return -1;
		}

		public int getIndex(Mesh Ms)
		{
			for (int i = 0; i < this.now_gob_i; i++)
			{
				if (this.AMesh[i].getMesh() == Ms)
				{
					return i;
				}
			}
			return -1;
		}

		public MultiMeshRenderer SetColor(int i, Color32 col)
		{
			if (i >= 0)
			{
				Material material = this.AMtr[i];
				material.SetColor("_Color", col);
				material.SetColor("_TintColor", col);
			}
			return this;
		}

		public MultiMeshRenderer SetColor(MeshDrawer Md, Color32 col)
		{
			return this.SetColor(X.isinC<MeshDrawer>(this.AMesh, Md), col);
		}

		public MultiMeshRenderer setLayer(int layer)
		{
			int num = this.now_gob_i;
			for (int i = 0; i < num; i++)
			{
				this.AGob[i].layer = layer;
			}
			return this;
		}

		public MultiMeshRenderer Clear(int start_i = 0)
		{
			if (this.AGob == null)
			{
				this.Awake();
			}
			int num = this.now_gob_i;
			for (int i = start_i; i < num; i++)
			{
				this.AMesh[i].clear(false, false);
			}
			return this;
		}

		public int createAlias(int mesh_cnt, MultiMeshRenderer SrcMMRD)
		{
			int length = SrcMMRD.Length;
			for (int i = 0; i < length; i++)
			{
				MeshDrawer meshDrawer = SrcMMRD.Get(i);
				if (this.createAlias(mesh_cnt, meshDrawer, SrcMMRD.AGob[i], -1, null, null, null))
				{
					mesh_cnt++;
				}
			}
			return mesh_cnt;
		}

		public bool createAlias(int mesh_cnt, MeshDrawer SrcMd, GameObject SrcGob, int layer = -1, Material[] AMtrArray = null, MaterialPropertyBlock[] P = null, ValotileRenderer SrcValot = null)
		{
			if (this.AGob == null)
			{
				this.Awake();
			}
			this.InitMeshRendererArray();
			this.allocCapacity(mesh_cnt);
			if (SrcGob == null)
			{
				return false;
			}
			GameObject gameObject = this.AGob[mesh_cnt];
			MeshRenderer meshRenderer = null;
			if (gameObject == null)
			{
				GameObject[] agob = this.AGob;
				GameObject gameObject2 = base.gameObject;
				string text = "-MF-Alias-";
				int num = this.created_count;
				this.created_count = num + 1;
				gameObject = (agob[mesh_cnt] = IN.CreateGob(gameObject2, text + num.ToString() + ")"));
				gameObject.transform.localPosition = SrcGob.transform.localPosition;
			}
			else
			{
				meshRenderer = ((this.AMrd != null) ? this.AMrd[mesh_cnt] : gameObject.GetComponent<MeshRenderer>());
			}
			gameObject.layer = ((layer == -1) ? SrcGob.layer : layer);
			gameObject.isStatic = false;
			if ((this.aliased_bits & (1U << mesh_cnt)) == 0U && this.AMesh[mesh_cnt] != null)
			{
				this.AMesh[mesh_cnt].destruct();
			}
			this.aliased_bits |= 1U << mesh_cnt;
			this.AMesh[mesh_cnt] = SrcMd;
			Material material = (this.AMtr[mesh_cnt] = SrcMd.getMaterial());
			if (meshRenderer == null)
			{
				MeshDrawer.makeNormalMeshRendererAndFilter(gameObject, material, out meshRenderer);
			}
			meshRenderer.sharedMaterials = ((AMtrArray == null) ? SrcMd.getMaterialArray(false) : AMtrArray);
			meshRenderer.enabled = true;
			MultiMeshRenderer.setPropertyList(meshRenderer, P);
			MeshFilter component = gameObject.GetComponent<MeshFilter>();
			Mesh mesh = SrcMd.getMesh();
			component.sharedMesh = mesh;
			if (this.AMf != null)
			{
				this.AMf[mesh_cnt] = component;
			}
			if (this.AMrd != null)
			{
				this.AMrd[mesh_cnt] = meshRenderer;
			}
			this.now_gob_i = X.Mx(mesh_cnt + 1, this.now_gob_i);
			if (this.ValotConnetcable_ != null)
			{
				ValotileRendererFilter orAdd = IN.GetOrAdd<ValotileRendererFilter>(gameObject);
				if (this.AValo != null)
				{
					this.AValo[mesh_cnt] = orAdd;
				}
				this.ValoInit(orAdd, component, meshRenderer);
				if (SrcValot != null)
				{
					orAdd.aliasFrom(SrcValot, AMtrArray);
				}
			}
			return true;
		}

		public static void setPropertyList(MeshRenderer Mrd, MaterialPropertyBlock[] P)
		{
			if (P == null)
			{
				return;
			}
			int num = P.Length;
			for (int i = 0; i < num; i++)
			{
				try
				{
					Mrd.SetPropertyBlock(P[i], i);
				}
				catch
				{
				}
			}
		}

		public virtual void OnDestroy()
		{
			this.Trim(0);
		}

		public void Trim(int start_i)
		{
			for (int i = this.AMtr.Length - 1; i >= start_i; i--)
			{
				this.AMtr[i] = null;
				if (this.AMrd != null)
				{
					this.AMrd[i] = null;
				}
				if (this.AMf != null && X.BTW(0f, (float)i, (float)this.AMf.Length) && this.AMf[i] != null)
				{
					this.AMf[i].sharedMesh = null;
					this.AMf[i] = null;
				}
				if (this.AMda != null)
				{
					this.AMda[i] = null;
				}
				if (this.AValo != null && X.BTW(0f, (float)i, (float)this.AValo.Length))
				{
					ValotileRenderer valotileRenderer = this.AValo[i];
					this.AValo[i] = null;
					if (valotileRenderer != null)
					{
						valotileRenderer.ReleaseBinding(true, false, false);
					}
				}
				if (this.AMesh != null && this.AMesh[i] != null)
				{
					GameObject gameObject = this.AGob[i];
					this.AGob[i] = null;
					if ((this.aliased_bits & (1U << i)) == 0U)
					{
						this.AMesh[i].destruct();
					}
					this.AMesh[i] = null;
					if (gameObject != null)
					{
						Object.Destroy(gameObject);
					}
				}
				if (this.AMpb != null)
				{
					this.AMpb[i] = null;
				}
				this.aliased_bits &= ~(1U << i);
			}
			this.now_gob_i = X.Mn(start_i, this.now_gob_i);
			if (start_i == 0)
			{
				this.created_count = 0;
			}
		}

		public MultiMeshRenderer deactivate(bool clear_mesh = false)
		{
			for (int i = 0; i < this.now_gob_i; i++)
			{
				try
				{
					if (clear_mesh)
					{
						this.AMesh[i].clear(false, false);
						this.AMesh[i].updateForMeshRenderer(false);
						if (this.AMda != null)
						{
							this.AMda[i].Set(true);
						}
					}
					if (this.AGob[i] != null)
					{
						this.GetMeshRenderer(i).enabled = false;
					}
				}
				catch
				{
					X.de(" 削除されたMrdに対する deactivate : " + i.ToString(), null);
				}
			}
			return this;
		}

		public virtual MultiMeshRenderer activate()
		{
			for (int i = 0; i < this.now_gob_i; i++)
			{
				try
				{
					this.GetMeshRenderer(i).enabled = true;
				}
				catch
				{
					X.de(" 削除されたMrdに対する activate : " + i.ToString(), null);
				}
			}
			return this;
		}

		public MultiMeshRenderer Release()
		{
			int num = this.now_gob_i;
			for (int i = 0; i < num; i++)
			{
				this.AMesh[i].clear(false, false);
			}
			return this;
		}

		public MultiMeshRenderer setMtrColor(string property_title, Color32 C)
		{
			for (int i = 0; i < this.now_gob_i; i++)
			{
				this.AMesh[i].setMtrColor(property_title, C);
			}
			return this;
		}

		public MultiMeshRenderer setPointsAlpha(float f)
		{
			for (int i = 0; i < this.now_gob_i; i++)
			{
				this.AMesh[i].setPointsAlpha(f);
			}
			return this;
		}

		public static Camera MakeRenderBufferAndCamera(int width, int height, int layer, Color32 BaseCol, Camera BaseCamera = null, GameObject BaseObject = null, bool enable_pixel_perfect = false)
		{
			return MultiMeshRenderer.MakeRenderBufferAndCamera(new RenderTexture((width > 0) ? width : Screen.width, (height > 0) ? height : Screen.height, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default), layer, BaseCol, BaseCamera, BaseObject, enable_pixel_perfect);
		}

		public static Camera MakeRenderBufferAndCamera(RenderTexture Img, int layer, Color32 BaseCol, Camera BaseCamera = null, GameObject BaseObject = null, bool enable_pixel_perfect = false)
		{
			if (BaseCamera == null)
			{
				BaseCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
			}
			GameObject gameObject = new GameObject("Camera-MMRD-Buf" + layer.ToString());
			PerfectPixelCamera perfectPixelCamera = null;
			if (enable_pixel_perfect)
			{
				perfectPixelCamera = gameObject.AddComponent<PerfectPixelCamera>();
				perfectPixelCamera.TexturePixelsPerWorldUnit = 64;
			}
			Camera camera = gameObject.AddComponent<Camera>();
			gameObject.isStatic = false;
			Rect rect = new Rect(0f, 0f, 1f, 1f);
			rect.x = 0.5f - (float)Img.width / (float)Screen.width;
			rect.y = 0.5f - (float)Img.height / (float)Screen.height;
			rect.width = 0.5f + (float)Img.width / (float)Screen.width - rect.x;
			rect.height = 0.5f + (float)Img.height / (float)Screen.height - rect.y;
			camera.CopyFrom(BaseCamera);
			camera.clearFlags = CameraClearFlags.Color;
			camera.targetTexture = Img;
			camera.backgroundColor = BaseCol;
			camera.rect = rect;
			Matrix4x4 matrix4x = camera.projectionMatrix;
			matrix4x = Matrix4x4.Scale(new Vector3((float)Screen.width / (float)Img.width, (float)Screen.height / (float)Img.height, 1f)) * matrix4x;
			gameObject.layer = layer;
			camera.projectionMatrix = matrix4x;
			camera.cullingMask = 1 << layer;
			gameObject.transform.position = BaseCamera.transform.position;
			if (enable_pixel_perfect)
			{
				perfectPixelCamera.OnEnable();
			}
			if (BaseObject != null)
			{
				BaseObject.layer = layer;
				gameObject.transform.SetParent(BaseObject.transform, true);
			}
			return camera;
		}

		public static Camera DestroyRenderBufferAndCamera(Camera Cam)
		{
			IN.DestroyOne(Cam.gameObject);
			Object targetTexture = Cam.targetTexture;
			Cam.targetTexture = null;
			IN.DestroyOne(targetTexture);
			IN.DestroyOne(Cam);
			return null;
		}

		public void arrangeScaleAll(float xscl, float yscl, float center_x = 0f, float center_y = 0f, bool no_divide_ppu = false)
		{
			if (!no_divide_ppu)
			{
				center_x /= 64f;
				center_y /= 64f;
			}
			if (this.AMda == null)
			{
				this.AMda = new MdArranger[this.AGob.Length];
			}
			for (int i = 0; i < this.now_gob_i; i++)
			{
				this.AMda[i].SetWhole(true).scaleAll(xscl, yscl, center_x, center_y, true);
			}
		}

		public void updateAll(bool temporary = false)
		{
			for (int i = 0; i < this.now_gob_i; i++)
			{
				this.AMesh[i].updateForMeshRenderer(temporary);
			}
		}

		protected void ValoInit(ValotileRenderer Valo, MeshDrawer Md, MeshRenderer Mrd)
		{
			Valo.Init(Md, Mrd, this.valotile_enabled_);
			Valo.bounds_clip = this.valotile_clip_;
			if (this.ValotConnetcable_ != null)
			{
				this.ValotConnetcable_.connectToBinder(Valo);
			}
		}

		protected void ValoInit(ValotileRendererFilter Valo, MeshFilter Filter, MeshRenderer Mrd)
		{
			Valo.Init(Filter, Mrd, this.valotile_enabled_);
			Valo.bounds_clip = this.valotile_clip_;
			if (this.ValotConnetcable_ != null)
			{
				this.ValotConnetcable_.connectToBinder(Valo);
			}
		}

		public ValotileRenderer.IValotConnetcable ValotConnetcable
		{
			get
			{
				return this.ValotConnetcable_;
			}
			set
			{
				this.ValotConnetcable_ = value;
				if (value != null)
				{
					this.use_valotile = true;
				}
			}
		}

		public bool use_valotile
		{
			get
			{
				return this.AValo != null;
			}
			set
			{
				if (this.use_valotile == value)
				{
					return;
				}
				if (value)
				{
					this.AValo = new ValotileRenderer[this.AMesh.Length];
					for (int i = this.Length - 1; i >= 0; i--)
					{
						if (!(this.AGob[i] == null))
						{
							MeshDrawer meshDrawer = this.AMesh[i];
							MeshRenderer meshRenderer = this.GetMeshRenderer(i);
							ValotileRenderer valotileRenderer = (this.AValo[i] = IN.GetOrAdd<ValotileRenderer>(this.AGob[i]));
							this.ValoInit(valotileRenderer, meshDrawer, meshRenderer);
						}
					}
					return;
				}
				for (int j = this.Length - 1; j >= 0; j--)
				{
					ValotileRenderer valotileRenderer2 = this.AValo[j];
					if (valotileRenderer2 != null)
					{
						IN.DestroyOne(valotileRenderer2);
					}
				}
				this.AValo = null;
			}
		}

		public void valotileRelease(bool release_link = true, bool do_not_mesh_resample = false)
		{
			this.ValotConnetcable_ = null;
			if (this.AValo == null)
			{
				return;
			}
			for (int i = this.Length - 1; i >= 0; i--)
			{
				ValotileRenderer valotileRenderer = this.AValo[i];
				if (valotileRenderer != null)
				{
					valotileRenderer.ReleaseBinding(release_link, do_not_mesh_resample, false);
				}
			}
			this.AValo = null;
		}

		public bool valotile_enabled
		{
			get
			{
				return this.valotile_enabled_;
			}
			set
			{
				if (this.valotile_enabled == value)
				{
					return;
				}
				this.valotile_enabled_ = value;
				if (this.AValo == null)
				{
					return;
				}
				for (int i = this.Length - 1; i >= 0; i--)
				{
					ValotileRenderer valotileRenderer = this.AValo[i];
					if (valotileRenderer != null)
					{
						valotileRenderer.enabled = this.valotile_enabled_;
					}
					MeshRenderer meshRenderer = this.GetMeshRenderer(i);
					if (meshRenderer != null)
					{
						meshRenderer.enabled = !this.valotile_enabled_;
					}
				}
			}
		}

		public bool valotile_clip
		{
			get
			{
				return this.valotile_clip_;
			}
			set
			{
				if (this.valotile_clip == value)
				{
					return;
				}
				this.valotile_clip_ = value;
				if (this.AValo == null)
				{
					return;
				}
				for (int i = this.Length - 1; i >= 0; i--)
				{
					ValotileRenderer valotileRenderer = this.AValo[i];
					if (valotileRenderer != null)
					{
						valotileRenderer.bounds_clip = this.valotile_clip_;
					}
				}
			}
		}

		public void RenderToCamManual()
		{
			int length = this.Length;
			for (int i = 0; i < length; i++)
			{
				MeshDrawer meshDrawer = this.AMesh[i];
				if (meshDrawer != null)
				{
					ValotileRenderer.renderOneMd(meshDrawer, null, null, null, false);
				}
			}
		}

		public float base_z;

		protected GameObject[] AGob;

		protected Material[] AMtr;

		protected MeshDrawer[] AMesh;

		protected MdArranger[] AMda;

		protected MeshRenderer[] AMrd;

		protected MeshFilter[] AMf;

		protected ValotileRenderer[] AValo;

		protected MProperty[] AMpb;

		protected ValotileRenderer.IValotConnetcable ValotConnetcable_;

		private int queue = -1;

		public uint aliased_bits;

		protected int now_gob_i;

		public float slip_z = 0.000125f;

		public bool z_apply_to_gameobject = true;

		public int stencil_ref = -1;

		private int created_count;

		public bool use_cache;

		public bool merge_to_one_gob;

		private bool valotile_enabled_ = true;

		private bool valotile_clip_;
	}
}
