using System;
using System.Collections.Generic;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

namespace m2d
{
	public class M2SubMap : IM2OrderDrawItem, IIdvName
	{
		public M2SubMap(Map2d _Base, string _key, int _index = -1)
		{
			this.M2D = _Base.M2D;
			this.key_ = _key;
			this.Base = _Base;
			this.index = _index;
			this.Mp = this.Base.M2D.Get(_key, false);
			if (this.index != -2)
			{
				this.AChild = new M2SubMap[0];
			}
		}

		public M2SubMap copyFrom(M2SubMap M, bool copy_source = true, bool copy_repeating = true)
		{
			this.order = M.order;
			this.posx = M.posx;
			this.posy = M.posy;
			this.basex = M.basex;
			this.basey = M.basey;
			this.scalex = M.scalex;
			this.scaley = M.scaley;
			this.scrollx = M.scrollx;
			this.scrolly = M.scrolly;
			this.camera_length_ = M.camera_length_;
			if (copy_source)
			{
				this.Mp = M.Mp;
				this.ASimplify_arranged = M.ASimplify_arranged;
				this.AMtrCache = M.AMtrCache;
			}
			if (copy_repeating)
			{
				this.repeatx = M.repeatx;
				this.repeaty = M.repeaty;
				this.repeat_intvx = M.repeat_intvx;
				this.repeat_intvy = M.repeat_intvy;
			}
			this.reposit_flag = true;
			this.refine_mesh_flag_ |= 1;
			return this;
		}

		public string key
		{
			get
			{
				if (this.Mp == null)
				{
					return this.key_;
				}
				return this.Mp.key;
			}
			set
			{
				this.key_ = value;
				this.Mp = this.Base.M2D.Get(this.key_, false);
				this.reposit_flag = true;
				this.refine_mesh_flag_ |= 1;
			}
		}

		public bool temporary_duped
		{
			get
			{
				return this.Mp == null || this.Mp.SubMapData != this;
			}
		}

		public M2SubMap open(string child_key = "")
		{
			if (this.Mp == null)
			{
				return this;
			}
			if (this.Unstb == null || this.Unstb.Mp != this.Mp)
			{
				if (this.Unstb != null)
				{
					this.Unstb.destruct(false);
				}
				this.Unstb = new M2UnstabilizeMapItem(this.Mp, this);
			}
			else
			{
				this.Unstb.deactivateAll(true);
				this.Unstb.openMap();
			}
			if (!Map2d.editor_decline_lighting)
			{
				this.Unstb.Resume();
			}
			this.reposit_flag = true;
			bool flag = false;
			try
			{
				flag = this.M2D.popStockedForSubMap(this);
			}
			catch
			{
				global::XX.X.dl("ストック復元に失敗: " + this.Mp.ToString(), null, false, false);
			}
			if (flag || !this.Mp.opened)
			{
				if (!this.Mp.opened)
				{
					this.Mp.open(this.Base.gameObject, MAPMODE.SUBMAP, this);
				}
				if (this.TempGob != null)
				{
					try
					{
						IN.DestroyOne(this.TempGob);
					}
					catch
					{
					}
					this.TempGob = null;
				}
				this.MMRD = this.Mp.get_MMRD();
				this.MyTrs = this.Mp.gameObject.transform;
				if (!flag)
				{
					this.Mp.need_reentry_flag = true;
					this.Mp.addUpdateMesh(16384, false);
					this.Mp.drawCheck(0f);
				}
				else
				{
					M2BlurMeshDrawer blurDrawerContainer = this.Mp.getBlurDrawerContainer();
					if (blurDrawerContainer != null)
					{
						blurDrawerContainer.activate(false);
						if (blurDrawerContainer.need_reentry)
						{
							this.Mp.need_reentry_flag = true;
						}
					}
					if (!global::XX.X.DEBUGSTABILIZE_DRAW && !Map2d.editor_decline_lighting)
					{
						this.MMRD.ValotConnetcable = M2DBase.Instance.Cam;
						this.MMRD.valotile_clip = true;
					}
					this.Mp.addUpdateMesh(2048, false);
				}
			}
			else
			{
				if (this.TempGob == null)
				{
					this.TempGob = IN.CreateGob(this.Base.gameObject, (child_key == "") ? ("-tempgob(" + this.index.ToString() + ")-" + this.Mp.key) : child_key);
				}
				if (this.MMRD == null)
				{
					this.MMRD = this.TempGob.AddComponent<MultiMeshRenderer>();
					if (!global::XX.X.DEBUGSTABILIZE_DRAW && !Map2d.editor_decline_lighting)
					{
						this.MMRD.ValotConnetcable = M2DBase.Instance.Cam;
						this.MMRD.valotile_clip = true;
					}
				}
				this.Mp.Dgn = this.getBaseDgn();
				this.MyTrs = this.TempGob.transform;
			}
			this.initEffect();
			this._tostring = null;
			this.refine_mesh_flag_ = 0;
			this.FineScale();
			this.fineMeshAndMaterials(true, true);
			return this;
		}

		public void fineMeshAndMaterials(bool reset_mesh_attribute = false, bool first = false)
		{
			if (this.Mp.need_reentry_flag || this.Base.update_mesh_flag_has_splicemesh || this.MMRD == null)
			{
				if (this.Mp.need_reentry_flag)
				{
					this.clearRendered();
				}
				this.refine_mesh_flag_ |= 1 | (reset_mesh_attribute ? 2 : 0) | (first ? 4 : 0);
				return;
			}
			if ((this.refine_mesh_flag_ & 2) != 0)
			{
				reset_mesh_attribute = true;
			}
			if ((this.refine_mesh_flag_ & 4) != 0)
			{
				first = true;
			}
			this.refine_mesh_flag_ = 0;
			M2MeshContainer mmrd = this.Mp.get_MMRD();
			int num;
			if (this.temporary_duped)
			{
				num = this.MMRD.Length;
				for (int i = 0; i < num; i++)
				{
					GameObject gob = this.MMRD.GetGob(i);
					if (gob != null)
					{
						gob.SetActive(false);
					}
				}
			}
			if (first && this.index != -2)
			{
				this.clearRendered();
				this.ASimplify_arranged.Clear();
				this.AMtrCache.Clear();
			}
			num = mmrd.Length;
			int num2 = 0;
			while (this.ASimplify_arranged.Count < num)
			{
				this.ASimplify_arranged.Add(null);
				this.AMtrCache.Add(null);
			}
			this.Base.Dgn.initMap(this.Mp, this);
			bool is_flipped = this.is_flipped;
			if (this.Unstb != null)
			{
				this.Unstb.FineAuto();
			}
			for (int j = 0; j < num; j++)
			{
				MeshDrawer meshDrawer = mmrd.Get(j);
				if (meshDrawer != null && (meshDrawer.draw_triangle_count > 0 || meshDrawer.hasMultipleTriangle()))
				{
					int num3 = this.Mp.fineMaterialLayer(meshDrawer);
					Material[] materialArray = meshDrawer.getMaterialArray(false);
					if (materialArray.Length >= 2)
					{
						Material material = this.ASimplify_arranged[j];
						Material material2 = materialArray[0];
						if (material == null)
						{
							if (first)
							{
								material2 = (materialArray[0] = this.Mp.fineMaterialSubMapAttribute(meshDrawer, material2, this, true));
								material2.name = string.Concat(new string[]
								{
									this.MMRD.name,
									"-(SM-",
									this.index.ToString(),
									".",
									j.ToString(),
									")"
								});
							}
							RenderTexture renderTexture = null;
							try
							{
								renderTexture = material2.mainTexture as RenderTexture;
							}
							catch
							{
							}
							if (renderTexture != null)
							{
								RenderTexture renderTexture2 = this.Base.Dgn.ArrangeSimplifyMaterial(meshDrawer, material2, renderTexture, this, this.Mp.getLayer2UpdateFlag(meshDrawer));
								if (renderTexture2 != renderTexture && renderTexture2 != null)
								{
									material = (this.ASimplify_arranged[j] = MTRX.newMtr(material2));
									material.SetTexture("_MainTex", renderTexture2);
								}
							}
						}
						if (material != null)
						{
							if (reset_mesh_attribute && !first)
							{
								this.Mp.fineMaterialSubMapAttribute(meshDrawer, material, this, false);
							}
							materialArray[0] = material;
						}
						else if (reset_mesh_attribute && !first)
						{
							this.Mp.fineMaterialSubMapAttribute(meshDrawer, material2, this, false);
						}
						meshDrawer.chooseSubMesh(1, false, false);
					}
					Material material3 = meshDrawer.getMaterial();
					if (material3 != null)
					{
						if (this.temporary_duped)
						{
							Material material4 = this.AMtrCache[j];
							if (material4 == null || first)
							{
								material4 = (this.AMtrCache[j] = this.Mp.fineMaterialSubMapAttribute(meshDrawer, material4, this, true));
								material4.name = string.Concat(new string[]
								{
									this.MMRD.name,
									"-(SM-",
									this.index.ToString(),
									".",
									j.ToString(),
									")"
								});
							}
							else if (reset_mesh_attribute && material4 != null)
							{
								this.Mp.fineMaterialSubMapAttribute(meshDrawer, material4, this, false);
							}
							materialArray[(materialArray.Length == 1) ? 0 : 1] = material4;
						}
						else
						{
							Material material5 = (this.AMtrCache[j] = meshDrawer.getMaterial());
							if (reset_mesh_attribute && material5 != null)
							{
								this.AMtrCache[j] = this.Mp.fineMaterialSubMapAttribute(meshDrawer, this.AMtrCache[j], this, false);
							}
						}
						material3.mainTexture = this.M2D.MIchip.Tx;
					}
					bool flag = false;
					if (!Map2d.editor_decline_lighting && this.Unstb != null && this.Base.Dgn.isCreateFloatMesh(meshDrawer))
					{
						this.Unstb.AddSmMeshDrawer("SM_MESH_" + j.ToString(), meshDrawer, mmrd, num3, materialArray);
						flag = true;
					}
					if (this.temporary_duped)
					{
						if (this.MMRD.createAlias(num2, meshDrawer, mmrd.GetGob(j), num3, materialArray, null, mmrd.GetValotileRenderer(j)))
						{
							GameObject gob2 = this.MMRD.GetGob(num2);
							num2++;
							if (gob2 != null)
							{
								gob2.SetActive(!flag);
							}
						}
					}
					else
					{
						MeshRenderer meshRenderer = this.MMRD.GetMeshRenderer(j);
						if (meshRenderer != null)
						{
							meshRenderer.sharedMaterials = materialArray;
						}
					}
					if (is_flipped)
					{
						this.MMRD.preparePropertyBlock(j, -1, false).SetFloat("_Cull", 1f, true);
					}
					else
					{
						MProperty mproperty = this.MMRD.preparePropertyBlock(j, -1, true);
						if (mproperty != null)
						{
							mproperty.Clear(true);
						}
					}
				}
			}
			if (this.temporary_duped)
			{
				this.MMRD.Trim(num2);
			}
			if (this.AChild != null)
			{
				num = this.AChild.Length;
				for (int k = 0; k < num; k++)
				{
					this.AChild[k].fineMeshAndMaterials(reset_mesh_attribute, false);
				}
			}
		}

		public void fineMaterialNightColor(Dungeon Dgn = null, M2SubMap CheckSub = null)
		{
			this.Mp.fineMaterialNightColor(Dgn, this);
		}

		public M2SubMap FineScale()
		{
			if (this.MyTrs != null && this.stabilize_draw)
			{
				this.MyTrs.localScale = new Vector3(this.scalex, this.scaley, 1f);
			}
			if (this.refine_mesh_flag_ != 0)
			{
				this.fineMeshAndMaterials(true, false);
			}
			if (this.AChild != null)
			{
				int num = this.repeatx * this.repeaty - 1;
				for (int i = this.AChild.Length - 1; i >= num; i--)
				{
					if (this.AChild[i] != null)
					{
						this.AChild[i].close(true);
					}
				}
				if (this.AChild.Length != num)
				{
					Array.Resize<M2SubMap>(ref this.AChild, num);
				}
				for (int j = 0; j < num; j++)
				{
					if (this.AChild[j] == null)
					{
						this.AChild[j] = new M2SubMap(this.Base, this.key, -2).copyFrom(this, true, false).open("-sm_" + this.index.ToString() + "__duped_" + j.ToString());
					}
				}
			}
			return this.FineScroll();
		}

		public void Order(int _order)
		{
			this.order = (SMORDER)_order;
			this.Pos.z = this.base_z;
			if (this.AChild != null)
			{
				int num = this.AChild.Length;
				for (int i = 0; i < num; i++)
				{
					M2SubMap m2SubMap = this.AChild[i];
					if (m2SubMap != null)
					{
						m2SubMap.copyFrom(this, true, false).FineScale();
					}
				}
			}
			if (this.Mp.is_submap && !this.temporary_duped)
			{
				IEffectSetter effectForChip = this.Mp.getEffectForChip();
				GameObject gobForMd = this.getGobForMd(this.Mp.MyDrawerB);
				if (effectForChip != null && gobForMd != null)
				{
					effectForChip.setLayer(gobForMd.layer, gobForMd.layer);
				}
			}
		}

		public float base_z
		{
			get
			{
				float num;
				switch (this.order)
				{
				case SMORDER.BACK:
					num = 460f;
					break;
				case SMORDER.GROUND:
					num = 70f;
					break;
				case SMORDER.TOP:
					num = 45f;
					break;
				default:
					num = 600f;
					break;
				}
				return num - (4f * this.camera_length - (float)this.index * 1E-05f);
			}
		}

		public int get_order()
		{
			return (int)this.order;
		}

		public M2SubMap FineScroll()
		{
			if (this.Mp == null)
			{
				return this;
			}
			this.reposit_flag = true;
			if (!this.temporary_duped)
			{
				this.Mp.Dgn = this.Base.Dgn;
			}
			this.Order((int)this.order);
			return this;
		}

		public float effect_z
		{
			get
			{
				return this.Pos.z + this.Base.Dgn.getDrawZ(MAPMODE.SUBMAP, 1) - 0.005f;
			}
		}

		public bool isinCamera(float mapl, float mapt, float mapr, float mapb, float extend_pixel)
		{
			float num = global::XX.X.Abs(this.scalex);
			float num2 = global::XX.X.Abs(this.scaley);
			float num3 = extend_pixel * num * this.Mp.rCLEN;
			float num4 = extend_pixel * num2 * this.Mp.rCLEN;
			Transform transform;
			if (!this.temporary_duped)
			{
				transform = this.Mp.gameObject.transform;
			}
			else
			{
				transform = this.TempGob.transform;
			}
			Vector2 vector = transform.localToWorldMatrix.MultiplyPoint3x4(new Vector3(this.Mp.map2ux(mapl - num3), this.Mp.map2uy(mapt - num4)));
			Vector2 vector2 = transform.localToWorldMatrix.MultiplyPoint3x4(new Vector3(this.Mp.map2ux(mapr + num3), this.Mp.map2uy(mapb + num4)));
			if (this.Mp.M2D.Cam.isCoveringMp(this.Base.globaluxToMapx(vector.x), this.Base.globaluyToMapy(vector.y), this.Base.globaluxToMapx(vector2.x), this.Base.globaluyToMapy(vector2.y), 0f))
			{
				return true;
			}
			if (this.AChild != null)
			{
				for (int i = this.AChild.Length - 1; i >= 0; i--)
				{
					if (this.AChild[i].isinCamera(mapl, mapt, mapr, mapb, extend_pixel))
					{
						return true;
					}
				}
			}
			return false;
		}

		public MultiMeshRenderer get_MMRD()
		{
			return this.MMRD;
		}

		public Material getChipMtr(Material Src)
		{
			return this.Base.M2D.DGN.getMtr(Src.shader, -2 - this.index);
		}

		public Material getChipMtr(Shader Shd)
		{
			return this.Base.M2D.DGN.getMtr(Shd, -2 - this.index);
		}

		public GameObject getGobForMd(MeshDrawer Md)
		{
			if (this.MMRD == null)
			{
				return null;
			}
			int length = this.MMRD.Length;
			Mesh mesh = Md.getMesh();
			for (int i = 0; i < length; i++)
			{
				if (this.MMRD.Get(i).getMesh() == mesh)
				{
					return this.MMRD.GetGob(i);
				}
			}
			return null;
		}

		public void subMapClose()
		{
			this.TempGob == null;
		}

		public bool is_flipped
		{
			get
			{
				return this.scalex * this.scaley < 0f;
			}
		}

		public Matrix4x4 getTransformForRender(bool for_render = false)
		{
			float num = (float)(for_render ? 2 : 1);
			return Matrix4x4.Translate(new Vector3(this.Pos.x * num, this.Pos.y * num, this.Pos.z)) * Matrix4x4.Scale(new Vector3(this.scalex, this.scaley, 1f));
		}

		public Matrix4x4 getScaleMinusOne()
		{
			return Matrix4x4.Scale(new Vector3(this.scalex - 1f, this.scaley - 1f, 0f));
		}

		public Matrix4x4 getTransformForCamera(bool for_render = false)
		{
			return Matrix4x4.Scale(new Vector3(this.Mp.base_scale, this.Mp.base_scale, 0f)) * this.getTransformForRender(for_render);
		}

		public bool needReentry(M2SubMap NewSrc)
		{
			if (NewSrc == null)
			{
				return false;
			}
			int num = ((this.order == SMORDER.SKY) ? 0 : ((this.order == SMORDER.TOP) ? 2 : 1));
			int num2 = ((NewSrc.order == SMORDER.SKY) ? 0 : ((NewSrc.order == SMORDER.TOP) ? 2 : 1));
			return num != num2;
		}

		public void close(bool using_stock_sm)
		{
			bool flag = false;
			if (using_stock_sm && this.TempGob == null)
			{
				M2BlurMeshDrawer blurDrawerContainer = this.Mp.getBlurDrawerContainer();
				if (blurDrawerContainer != null)
				{
					blurDrawerContainer.deactivate(false);
				}
				this.M2D.stockSubMap(this);
				flag = true;
			}
			if (this.EfBinder != null)
			{
				this.EfBinder.unbindToCamera();
				this.EfBinder = null;
			}
			if (this.TempGob == null)
			{
				this.Mp.destructEffectForChip();
			}
			this.EffectM = null;
			this._tostring = null;
			if (!using_stock_sm && this.index != -2)
			{
				this.clearRendered();
			}
			if (this.Unstb != null)
			{
				this.Unstb.closeMap(true);
			}
			bool flag2 = true;
			if (this.MyTrs == null || this.MMRD == null)
			{
				IN.DestroyOne(this.TempGob);
			}
			else
			{
				if (this.TempGob != null)
				{
					this.subMapClose();
					IN.DestroyOne(this.TempGob);
				}
				else
				{
					if (!flag)
					{
						if (this.Mp.gameObject != null)
						{
							this.Mp.gameObject.SetActive(true);
							this.Mp.close();
							this.Mp.Dgn = null;
						}
					}
					else
					{
						flag2 = false;
					}
					this.MMRD.valotileRelease(true, true);
				}
				this.MMRDEffect = (this.MMRDSource = null);
			}
			if (this.AChild != null)
			{
				int num = this.AChild.Length;
				for (int i = 0; i < num; i++)
				{
					this.AChild[i].close(using_stock_sm);
				}
				Array.Resize<M2SubMap>(ref this.AChild, 0);
			}
			if (flag2)
			{
				this.TempGob = null;
				this.MMRD = null;
				this.MyTrs = null;
			}
		}

		public bool closed
		{
			get
			{
				return this.MyTrs == null;
			}
		}

		public void releaseSimplifiedTexture()
		{
			if (this.TempGob == null && this.MMRD != null)
			{
				this.Mp.get_MMRD().clearRendered();
				this.Mp.need_reentry_flag = true;
			}
			this.clearRendered();
			this.refine_mesh_flag_ |= 1;
		}

		private void initEffect()
		{
			this.MMRDSource = null;
			if (this.MMRDEffect != null)
			{
				try
				{
					IN.DestroyOne(this.MMRDEffect.gameObject);
				}
				catch
				{
				}
				this.MMRDEffect = null;
			}
			this.EffectM = null;
			IEffectSetter effectForChip = this.Mp.getEffectForChip();
			if (effectForChip == null)
			{
				return;
			}
			if (effectForChip is IMapChipEffectSetter)
			{
				this.EffectM = effectForChip as IMapChipEffectSetter;
			}
			if (this.TempGob != null)
			{
				if (this.EffectM != null)
				{
					Dungeon dgn = this.Mp.Dgn;
					this.EffectM.addSubMapBinder(this.EfBinder = new M2SubMap.EfSubMapEffectBinder(this, effectForChip, this.EffectM, dgn.getLayerForChip(0, Dungeon.MESHTYPE.EFFECT), dgn.getLayerForChip(1, Dungeon.MESHTYPE.EFFECT)));
					return;
				}
				this.MMRDSource = effectForChip.getMMRDForMeshDrawerContainer();
				if (this.MMRDSource == null)
				{
					return;
				}
				this.MMRDEffect = IN.CreateGob(this.TempGob, "-EF").AddComponent<MultiMeshRenderer>();
			}
		}

		public void runEffect(float fcnt)
		{
			if (!this.temporary_duped)
			{
				this.Mp.runEffect();
				return;
			}
			if (this.MMRDEffect == null)
			{
				return;
			}
			IEffectSetter effectForChip = this.Mp.getEffectForChip();
			if (effectForChip == null)
			{
				return;
			}
			if (!effectForChip.isActive())
			{
				if (this.MMRDEffect.enabled)
				{
					this.MMRDEffect.OnDestroy();
					this.MMRDEffect.enabled = false;
				}
				return;
			}
			if (!this.MMRDEffect.enabled)
			{
				this.MMRDEffect.enabled = true;
			}
			int num = this.MMRDEffect.createAlias(0, this.MMRDSource);
			int length = this.MMRDEffect.Length;
			for (int i = num; i < length; i++)
			{
				MeshRenderer meshRenderer = this.MMRDEffect.GetMeshRenderer(i);
				if (meshRenderer != null)
				{
					meshRenderer.enabled = false;
				}
			}
		}

		public void fineCam(float camx, float camy)
		{
			if (this.Mp == null || this.AChild == null)
			{
				return;
			}
			float num = (this.basex + 0.5f) * this.Base.CLEN;
			float num2 = (this.basey + 0.5f) * this.Base.CLEN;
			float num3 = ((this.scalex >= 0f) ? (this.posx + 0.5f) : ((float)this.Mp.clms - 0.5f - this.posx)) * this.Base.CLEN;
			float num4 = ((this.scaley >= 0f) ? (this.posy + 0.5f) : ((float)this.Mp.rows - 0.5f - this.posy)) * this.Base.CLEN;
			float num5 = camx - num;
			float num6 = camy - num2;
			if (this.difx == num5 && this.dify == num6 && !this.reposit_flag)
			{
				return;
			}
			this.reposit_flag = false;
			this.difx = num5;
			this.dify = num6;
			float num7 = num + num5 * (1f - this.scrollx) - global::XX.X.Abs(this.scalex) * (float)this.Mp.width * (float)(this.repeatx - 1) * 0.5f * this.repeat_intvx;
			float num8 = num2 + num6 * (1f - this.scrolly) - global::XX.X.Abs(this.scaley) * (float)this.Mp.height * (float)(this.repeaty - 1) * 0.5f * this.repeat_intvy;
			this.fineBoundsLT(num7 - num3 * global::XX.X.Abs(this.scalex), num8 - num4 * global::XX.X.Abs(this.scaley));
		}

		private void fineBoundsLT(float pixelx, float pixely)
		{
			this.bounds_x_ = pixelx;
			this.bounds_y_ = pixely;
			if (this.MyTrs != null)
			{
				this.Pos.x = this.Base.pixel2ux(this.bounds_x_ + (float)(this.Mp.width / 2) * global::XX.X.Abs(this.scalex));
				this.Pos.y = this.Base.pixel2uy(this.bounds_y_ + (float)(this.Mp.height / 2) * global::XX.X.Abs(this.scaley));
				if (this.stabilize_draw)
				{
					this.MyTrs.localPosition = this.Pos;
				}
				if (this.EffectM != null)
				{
					if (this.TempGob == null)
					{
						this.EffectM.setGraphicMatrix(this.getTransformForCamera(false));
					}
					else if (this.EfBinder != null)
					{
						this.EfBinder.setGraphicMatrix(this.getTransformForCamera(false));
					}
				}
				if (this.AChild != null)
				{
					int num = this.AChild.Length;
					if (num == 0)
					{
						return;
					}
					int num2 = 1;
					float scaled_repeat_shift_pixelx = this.scaled_repeat_shift_pixelx;
					float scaled_repeat_shift_pixely = this.scaled_repeat_shift_pixely;
					for (int i = 0; i < num; i++)
					{
						if (num2 >= this.repeatx)
						{
							num2 = 0;
							pixelx = this.bounds_x_;
							pixely += scaled_repeat_shift_pixely;
						}
						else
						{
							pixelx += scaled_repeat_shift_pixelx;
						}
						this.AChild[i].fineBoundsLT(pixelx, pixely);
						num2++;
					}
				}
			}
		}

		public float scaled_repeat_shift_pixelx
		{
			get
			{
				return (float)this.Mp.width * global::XX.X.Abs(this.scalex) * this.repeat_intvx;
			}
		}

		public float scaled_repeat_shift_pixely
		{
			get
			{
				return (float)this.Mp.height * global::XX.X.Abs(this.scaley) * this.repeat_intvy;
			}
		}

		public float bounds_x
		{
			get
			{
				if (this.Mp == null)
				{
					return this.basex + 0.5f;
				}
				return this.bounds_x_ / this.Base.CLEN;
			}
		}

		public float bounds_y
		{
			get
			{
				if (this.Mp == null)
				{
					return this.basey + 0.5f;
				}
				return this.bounds_y_ / this.Base.CLEN;
			}
		}

		public float bounds_w
		{
			get
			{
				if (this.Mp == null)
				{
					return 0f;
				}
				return ((float)this.Mp.clms + (float)(this.Mp.clms * (this.repeatx - 1)) * this.repeat_intvx) * global::XX.X.Abs(this.scalex);
			}
		}

		public float bounds_h
		{
			get
			{
				if (this.Mp == null)
				{
					return 0f;
				}
				return ((float)this.Mp.rows + (float)(this.Mp.rows * (this.repeaty - 1)) * this.repeat_intvy) * global::XX.X.Abs(this.scaley);
			}
		}

		public bool visible
		{
			get
			{
				return this.visible_;
			}
			set
			{
				this.visible_ = value;
				if (this.MyTrs != null)
				{
					if (this.TempGob != null)
					{
						this.TempGob.SetActive(this.visible);
					}
					else
					{
						this.Mp.visible = this.visible;
					}
				}
				if (this.AChild != null)
				{
					int num = this.AChild.Length;
					for (int i = 0; i < num; i++)
					{
						this.AChild[i].visible = value;
					}
				}
			}
		}

		public float camera_length_raw
		{
			get
			{
				return this.camera_length_;
			}
			set
			{
				this.camera_length_ = value;
			}
		}

		public float camera_length
		{
			get
			{
				if (this.camera_length_ >= 0f)
				{
					return this.camera_length_;
				}
				if (this.order == SMORDER.BACK || this.order == SMORDER.SKY)
				{
					return global::XX.X.Mn(this.scrollx - 1f, this.scrolly - 1f) * 1.2f + 1f;
				}
				if (this.order == SMORDER.TOP)
				{
					return global::XX.X.Mx(this.scrollx - 1f, this.scrolly - 1f) * 1.5f + 1f;
				}
				return (this.scrollx + this.scrolly) / 2f;
			}
		}

		public Dungeon getBaseDgn()
		{
			return this.Base.Dgn;
		}

		public Map2d getBaseMap()
		{
			return this.Base;
		}

		public Map2d getTargetMap()
		{
			return this.Mp;
		}

		public static M2SubMap readBytesContentSm(ByteArray Ba, Map2d Mp, int index, out string sm_key, int vers)
		{
			string text;
			sm_key = (text = Ba.readPascalString("utf-8", false));
			string text2 = text;
			float num = Ba.readFloat();
			float num2 = Ba.readFloat();
			float num3 = Ba.readFloat();
			float num4 = Ba.readFloat();
			float num5 = Ba.readFloat();
			float num6 = Ba.readFloat();
			float num7 = Ba.readFloat();
			float num8 = Ba.readFloat();
			SMORDER smorder = (SMORDER)Ba.readByte();
			int num9 = global::XX.X.Mx(Ba.readByte(), 1);
			int num10 = global::XX.X.Mx(Ba.readByte(), 1);
			float num11 = global::XX.X.Mx(Ba.readFloat(), 0f);
			float num12 = global::XX.X.Mx(Ba.readFloat(), 0f);
			float num13 = -1f;
			if (vers >= 3)
			{
				num13 = Ba.readFloat();
			}
			if (Mp == null)
			{
				return null;
			}
			M2SubMap m2SubMap = new M2SubMap(Mp, text2, index);
			if (m2SubMap.getTargetMap() == null)
			{
				return null;
			}
			m2SubMap.posx = num;
			m2SubMap.posy = num2;
			m2SubMap.basex = num3;
			m2SubMap.basey = num4;
			m2SubMap.scalex = num5;
			m2SubMap.scaley = num6;
			m2SubMap.scrollx = num7;
			m2SubMap.scrolly = num8;
			m2SubMap.order = smorder;
			m2SubMap.repeatx = num9;
			m2SubMap.repeaty = num10;
			m2SubMap.repeat_intvx = num11;
			m2SubMap.repeat_intvy = num12;
			m2SubMap.camera_length_raw = num13;
			return m2SubMap;
		}

		public void writeSaveBytesTo(ByteArray Ba)
		{
			Ba.writeByte(86);
			Ba.writePascalString(this.key, "utf-8");
			Ba.writeFloat(this.posx);
			Ba.writeFloat(this.posy);
			Ba.writeFloat(this.basex);
			Ba.writeFloat(this.basey);
			Ba.writeFloat(this.scalex);
			Ba.writeFloat(this.scaley);
			Ba.writeFloat(this.scrollx);
			Ba.writeFloat(this.scrolly);
			Ba.writeByte((int)this.order);
			Ba.writeByte(this.repeatx);
			Ba.writeByte(this.repeaty);
			Ba.writeFloat(this.repeat_intvx);
			Ba.writeFloat(this.repeat_intvy);
			Ba.writeFloat(this.camera_length_);
		}

		public string get_individual_key()
		{
			return this.key_;
		}

		public bool stabilize_draw
		{
			get
			{
				return Map2d.editor_decline_lighting || true;
			}
		}

		public void clearRendered()
		{
			int count = this.ASimplify_arranged.Count;
			for (int i = 0; i < count; i++)
			{
				Material material = this.ASimplify_arranged[i];
				this.ASimplify_arranged[i] = null;
				this.AMtrCache[i] = null;
				if (material != null && material.HasProperty("_MainTex"))
				{
					RenderTexture renderTexture = material.GetTexture("_MainTex") as RenderTexture;
					if (renderTexture != null)
					{
						material.SetTexture("_MainTex", null);
						BLIT.nDispose(renderTexture);
					}
				}
			}
		}

		public bool refine_mesh_flag
		{
			get
			{
				return this.refine_mesh_flag_ != 0;
			}
			set
			{
				this.refine_mesh_flag_ |= (value ? 1 : 0);
			}
		}

		public override string ToString()
		{
			if (this._tostring == null)
			{
				this._tostring = "<SM> " + this.Mp.key + ((this.TempGob != null) ? " (Temporary)" : "");
			}
			return this._tostring;
		}

		private string key_;

		public readonly M2DBase M2D;

		private readonly Map2d Base;

		private Map2d Mp;

		private MultiMeshRenderer MMRD;

		public int index;

		private bool visible_ = true;

		public float posx;

		public float posy;

		public float basex;

		public float basey;

		public float scalex = 1f;

		public float scaley = 1f;

		public float scrollx = 1f;

		public float scrolly = 1f;

		public int repeatx = 1;

		public int repeaty = 1;

		public float repeat_intvx = 1f;

		public float repeat_intvy = 1f;

		private float camera_length_ = -1f;

		public SMORDER order;

		public bool reposit_flag = true;

		private int refine_mesh_flag_;

		public List<Material> AMtrCache = new List<Material>(12);

		public List<Material> ASimplify_arranged = new List<Material>(12);

		private M2SubMap[] AChild;

		private float difx;

		private float dify;

		private float bounds_x_;

		private float bounds_y_;

		public Vector3 Pos;

		private GameObject TempGob;

		private M2UnstabilizeMapItem Unstb;

		private IMapChipEffectSetter EffectM;

		private Transform MyTrs;

		private MultiMeshRenderer MMRDEffect;

		private MultiMeshRenderer MMRDSource;

		private M2SubMap.EfSubMapEffectBinder EfBinder;

		private string _tostring;

		public class SmCameraRenderBinderMesh : CameraRenderBinderMesh
		{
			public SmCameraRenderBinderMesh(M2SubMap _Sm, string _key, MeshDrawer _Md, GameObject Gob)
				: base(_key, _Md, 0, true, -1000f)
			{
				this.Sm = _Sm;
				this.FineAttr(Gob);
			}

			public M2SubMap.SmCameraRenderBinderMesh FineAttr(GameObject Gob)
			{
				Vector3 position = Gob.transform.position;
				this.z_far = position.z;
				this.layer = Gob.layer;
				return this;
			}

			public override bool RenderToCam(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
			{
				this.Trs = this.Sm.getTransformForCamera(false);
				return base.RenderToCam(XCon, JCon, Cam);
			}

			private readonly M2SubMap Sm;
		}

		public class EfSubMapEffectBinder
		{
			public EfSubMapEffectBinder(M2SubMap _SM, IEffectSetter _Ef, IMapChipEffectSetter _Im, int _lay_bottom, int _lay_top)
			{
				this.SM = _SM;
				this.Ef = _Ef;
				this.Im = _Im;
				this.GMx = this.SM.getTransformForCamera(false);
				this.lay_bottom = _lay_bottom;
				this.lay_top = _lay_top;
			}

			public void bindToCamera()
			{
				if (!global::XX.X.DEBUGSTABILIZE_DRAW)
				{
					Dungeon dgn = this.SM.Mp.Dgn;
					this.unbindToCamera();
					this.SM.Base.M2D.Cam.assignRenderFunc(this.BindB = new CameraRenderBinderFunc("<SM>" + this.SM.getTargetMap().ToString() + "::BindB", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
					{
						this.Ef.RenderOneSide(true, this.GMx, Cam, false);
						return true;
					}, this.SM.effect_z), this.lay_bottom, false, null);
					this.SM.Base.M2D.Cam.assignRenderFunc(this.BindT = new CameraRenderBinderFunc("<SM>" + this.SM.getTargetMap().ToString() + "::BindT", delegate(XCameraBase XCon, ProjectionContainer JCon, Camera Cam)
					{
						this.Ef.RenderOneSide(false, this.GMx, Cam, false);
						return true;
					}, this.SM.effect_z - 1E-06f), this.lay_top, false, null);
				}
			}

			public void unbindToCamera()
			{
				if (this.BindB != null)
				{
					this.SM.Base.M2D.Cam.deassignRenderFunc(this.BindB, this.lay_bottom);
					this.SM.Base.M2D.Cam.deassignRenderFunc(this.BindT, this.lay_top);
					this.BindB = (this.BindT = null);
				}
			}

			public void setGraphicMatrix(Matrix4x4 Mx)
			{
				this.GMx = Mx;
			}

			private readonly M2SubMap SM;

			private readonly IEffectSetter Ef;

			private readonly IMapChipEffectSetter Im;

			private readonly int lay_bottom;

			private readonly int lay_top;

			private CameraRenderBinderFunc BindT;

			private CameraRenderBinderFunc BindB;

			public Matrix4x4 GMx;
		}
	}
}
