using System;
using UnityEngine;

namespace XX
{
	public class EffectMeshManager
	{
		public EffectMeshManager(string _name)
		{
			this.AMd = new MeshDrawer[16];
			if (EffectMeshManager.Sorter == null)
			{
				EffectMeshManager.Sorter = new SORT<MeshDrawer>(new Comparison<MeshDrawer>(MeshDrawer.fnSortMeshDrawer));
			}
			this.name = _name;
		}

		public MeshDrawer Get(string key, Material Mtr, bool is_bottom, Color32 _Col, C32 ColForGrd = null, bool _colorassign2mtr = false, int mesh_min_render_queue = 0)
		{
			int num = this.AMd.Length;
			MeshDrawer meshDrawer;
			for (int i = 0; i < num; i++)
			{
				meshDrawer = this.AMd[i];
				if (meshDrawer == null)
				{
					meshDrawer = (this.AMd[i] = ((this.MMRD != null) ? this.fineMMRD(i, null, 0) : new MeshDrawer(null, 4, 6)).activate(key, Mtr, is_bottom, _Col, ColForGrd));
					this.fineMMRD(i, meshDrawer, mesh_min_render_queue);
					meshDrawer.draw_gl_only = this.draw_gl_only_;
					this.draw_max = i + 1;
					this.need_sort = true;
					return meshDrawer;
				}
				if (meshDrawer.activateIf(key, Mtr, is_bottom, _Col, ColForGrd, _colorassign2mtr, this.no_activation_mode))
				{
					this.draw_max = X.Mx(i + 1, this.draw_max);
					meshDrawer.draw_gl_only = this.draw_gl_only_;
					this.fineMMRD(i, meshDrawer, mesh_min_render_queue);
					if (!this.no_activation_mode)
					{
						this.need_sort = true;
					}
					return meshDrawer;
				}
			}
			this.need_sort = true;
			Array.Resize<MeshDrawer>(ref this.AMd, num + 32);
			meshDrawer = (this.AMd[num] = ((this.MMRD != null) ? this.fineMMRD(num, null, 0) : new MeshDrawer(null, 4, 6)).activate(key, Mtr, is_bottom, _Col, ColForGrd));
			meshDrawer.draw_gl_only = this.draw_gl_only_;
			this.fineMMRD(num, meshDrawer, mesh_min_render_queue);
			this.draw_max = num + 1;
			return meshDrawer;
		}

		public void spliceNotActive()
		{
			if (this.MMRD == null)
			{
				return;
			}
			int num = this.AMd.Length;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				MeshDrawer meshDrawer = this.AMd[i];
				if (meshDrawer != null)
				{
					num2++;
					Vector3[] vertices = meshDrawer.getMesh().vertices;
					if (vertices == null || vertices.Length == 0)
					{
						this.AMd[i] = null;
					}
				}
			}
			EffectMeshManager.Sorter.qSort(this.AMd, (MeshDrawer a, MeshDrawer b) => MeshDrawer.fnSortMeshDrawerExistsNull(a, b), num);
			if (this.AMd.Length > num2 + 32)
			{
				Array.Resize<MeshDrawer>(ref this.AMd, num2 + 32);
			}
			this.need_sort = false;
		}

		public void assignMMRD(MultiMeshRenderer _MMRD)
		{
			this.MMRD = _MMRD;
			this.MMRD.slip_z = 0f;
			this.MMRD.base_z = 0f;
			this.MMRD.z_apply_to_gameobject = false;
			this.draw_gl_only_ = false;
			this.no_activation_mode = false;
		}

		public MultiMeshRenderer getAssignedMMRD()
		{
			return this.MMRD;
		}

		private MeshDrawer fineMMRD(int index, MeshDrawer _Md, int mesh_min_render_queue = 0)
		{
			if (this.MMRD == null)
			{
				return null;
			}
			while (this.MMRD.Length <= index)
			{
				this.MMRD.Make(MTRX.ColWhite, BLEND.NORMAL, null, null);
			}
			MeshDrawer meshDrawer;
			if (_Md != null)
			{
				Material material = _Md.getMaterial();
				if (mesh_min_render_queue > _Md.render_queue)
				{
					material = MTRX.newMtr(material);
					material.renderQueue = mesh_min_render_queue;
				}
				this.MMRD.setMaterial(index, material, false);
				meshDrawer = _Md;
			}
			else
			{
				meshDrawer = this.MMRD.Get(index);
			}
			return meshDrawer;
		}

		public void Draw(int top_layer, int bottom_layer, Matrix4x4 DrawMatrix, bool temporary = false, Camera CameraForMesh = null)
		{
			if (this.pre_draw_max > this.draw_max && this.MMRD != null)
			{
				for (int i = this.draw_max; i < this.pre_draw_max; i++)
				{
					MeshDrawer meshDrawer = this.AMd[i];
					meshDrawer.clearSimple();
					meshDrawer.updateForMeshRenderer(false);
				}
			}
			if (this.draw_max == 0)
			{
				this.pre_draw_max = 0;
				return;
			}
			if (this.MMRD != null)
			{
				for (int j = 0; j < this.draw_max; j++)
				{
					MeshDrawer meshDrawer2 = this.AMd[j];
					if (!meshDrawer2.isActive())
					{
						meshDrawer2.clearSimple();
						meshDrawer2.updateForMeshRenderer(false);
					}
					else
					{
						int num = (meshDrawer2.is_bottom ? bottom_layer : top_layer);
						if (num >= 0)
						{
							this.MMRD.GetGob(j).layer = num;
						}
						meshDrawer2.Offline().updateForMeshRenderer(false);
					}
				}
			}
			else
			{
				if (this.need_sort)
				{
					this.need_sort = false;
					if (this.draw_gl_only_)
					{
						EffectMeshManager.Sorter.qSort(this.AMd, null, this.draw_max);
					}
				}
				for (int k = 0; k < this.draw_max; k++)
				{
					MeshDrawer meshDrawer3 = this.AMd[k];
					if (!meshDrawer3.isActive())
					{
						meshDrawer3.clearSimple();
						if (!this.draw_gl_only_)
						{
							meshDrawer3.updateForMeshRenderer(false);
						}
					}
					else
					{
						meshDrawer3.Draw(top_layer, bottom_layer, DrawMatrix, temporary, CameraForMesh);
					}
				}
			}
			this.pre_draw_max = this.draw_max;
			this.draw_max = 0;
		}

		public bool draw_gl_only
		{
			get
			{
				return this.draw_gl_only_;
			}
			set
			{
				if (this.draw_gl_only_ == value || this.MMRD != null)
				{
					return;
				}
				this.draw_gl_only_ = value;
				for (int i = 0; i < this.pre_draw_max; i++)
				{
					MeshDrawer meshDrawer = this.AMd[i];
					meshDrawer.draw_gl_only = value;
					if (!value)
					{
						meshDrawer.arraysSubtraction();
					}
				}
			}
		}

		public void RedrawSameMesh(int top_layer, int bottom_layer, Matrix4x4 DrawMatrix, Camera CameraForMesh = null, bool no_z_scale2zero = false)
		{
			if (this.pre_draw_max == 0 || this.MMRD != null)
			{
				return;
			}
			for (int i = 0; i < this.pre_draw_max; i++)
			{
				this.AMd[i].RedrawSameMesh(top_layer, bottom_layer, DrawMatrix, CameraForMesh, no_z_scale2zero);
			}
		}

		public void clear()
		{
			if (this.MMRD != null)
			{
				for (int i = 0; i < this.pre_draw_max; i++)
				{
					MeshDrawer meshDrawer = this.AMd[i];
					if (meshDrawer != null)
					{
						meshDrawer.clear(false, false);
						meshDrawer.updateForMeshRenderer(false);
					}
				}
			}
			this.AMd = new MeshDrawer[16];
			this.draw_max = 0;
			this.pre_draw_max = 0;
		}

		public override string ToString()
		{
			return "EffectMeshManager - " + this.name;
		}

		public int Count
		{
			get
			{
				return this.pre_draw_max;
			}
		}

		public MeshDrawer this[int i]
		{
			get
			{
				return this.AMd[i];
			}
		}

		public void destruct()
		{
			int num = this.AMd.Length;
			for (int i = 0; i < num; i++)
			{
				MeshDrawer meshDrawer = this.AMd[i];
				if (meshDrawer != null)
				{
					meshDrawer.destruct();
				}
			}
		}

		public string name;

		private MeshDrawer[] AMd;

		public Matrix4x4 MatrixTransform = Matrix4x4.identity;

		private int draw_max;

		private int pre_draw_max;

		public bool need_sort;

		private MultiMeshRenderer MMRD;

		public bool draw_gl_only_ = true;

		public bool no_activation_mode = true;

		private static SORT<MeshDrawer> Sorter;
	}
}
