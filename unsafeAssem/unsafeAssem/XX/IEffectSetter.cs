using System;
using UnityEngine;

namespace XX
{
	public interface IEffectSetter
	{
		EffectItem setE(string etype, float _x, float _y, float _z, int _time, int _saf);

		EffectItem setEffectWithSpecificFn(string individual_name, float x, float y, float z, int time, int saf, FnEffectRun Fn);

		EffectItem PtcN(string ptc_name, float _x, float _y, float _z, int _time, int _saf = 0);

		EffectItem PtcN(EfParticle ptc_name, float _x, float _y, float _z, int _time, int _saf = 0);

		IEffectSetter PtcSTsetVar(string var_name, double val);

		IEffectSetter PtcSTsetVar(string var_name, string val);

		PTCThread PtcST(string ptcst_name, IEfPInteractale Listener, PTCThread.StFollow follow = PTCThread.StFollow.NO_FOLLOW, VariableP VarP = null);

		MeshDrawer MeshInit(string key, float _mesh_base_x, float _mesh_base_y, Color32 C, out bool z_resetted, BLEND blend = BLEND.NORMAL, bool bottom_flag = false);

		MeshDrawer MeshInit(string key, float _mesh_base_x, float _mesh_base_y, uint rgba, out bool z_resetted, BLEND blend = BLEND.NORMAL, bool bottom_flag = false);

		MeshDrawer MeshInit(string key, float _mesh_base_x, float _mesh_base_y, C32 C, Material Mtr, out bool z_resetted, bool bottom_flag = false, C32 CGrd = null, bool colormatch = false);

		MeshDrawer MeshInitGradation(string key, float _mesh_base_x, float _mesh_base_y, uint rgba, uint rgba2, GRD grd, out bool z_resetted, BLEND blend = BLEND.NORMAL, bool bottom_flag = false);

		MeshDrawer MeshInitImg(string key, float _mesh_base_x, float _mesh_base_y, MImage MI, out bool z_resetted, BLEND blend = BLEND.NORMAL, bool bottom_flag = false);

		Vector3 calcMeshXY(float _x, float _y, MeshDrawer Md, out bool force_reset_z);

		GameObject RemoveObject(GameObject Gob);

		int getParticleCount(EffectItem Ef, int default_cnt);

		float getParticleSpeed(EffectItem Ef, int cnt, float default_maxt);

		bool isActive();

		MultiMeshRenderer getMMRDForMeshDrawerContainer();

		EffectMeshManager getEffectMeshManager();

		IEffectSetter setLayer(int layer, int layer_b = -1);

		void RenderOneSide(bool bottom_flag, Matrix4x4 Multiple, Camera Cam = null, bool no_z_scale2zero = false);

		int getTargetLayer(bool bottom_flag);

		bool isinCameraPtc(EffectItem Ef, float cleft_px, float cbtm_px, float cright_px, float ctop_px, float camera_check_zm_mul = 1f, float camera_check_zm_add = 0f);

		void runDrawOrRedrawMesh(bool flag, float fcnt, float base_TS = 1f);

		void calcParticlePosition(MeshDrawer Md, ref float cx, ref float cy, ref float cz, bool first);

		void destruct();
	}
}
