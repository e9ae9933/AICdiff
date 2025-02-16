using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using GGEZ;
using PixelLiner;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

namespace XX
{
	[RequireComponent(typeof(PxlsLoader))]
	public sealed class IN : MonoBehaviour
	{
		private void Awake()
		{
			Logger.scene_changing = false;
			PxlsLoader.do_not_create_mesh_in_pxl = true;
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("ja-JP");
			IN.GuiCamera = null;
			IN.FlgUiUse = new Flagger(delegate(FlaggerT<string> V)
			{
				IN.Click.need_fine = true;
			}, delegate(FlaggerT<string> V)
			{
				IN.Click.Blur();
			});
			IN.ACameraGob = new List<GameObject>(10);
			if (CameraBidingsBehaviour.UiBind != null)
			{
				CameraBidingsBehaviour.UiBind.ClearBindings();
				IN.DestroyOne(CameraBidingsBehaviour.UiBind);
				CameraBidingsBehaviour.UiBind = null;
			}
			Application.targetFrameRate = 60;
			IN.getGUICamera();
			CURS.fineScene();
			IN.ARunner = new List<IRunAndDestroy>(128);
			IN.Click = new CLICK(64);
			if (IN._stage != null)
			{
				IN.ValotBounds.connectUI();
				IN.ValotBounds.OnEnable();
				global::UnityEngine.Object.Destroy(base.gameObject);
				global::UnityEngine.Object.Destroy(this);
				Logger.fineTimeStampViewer();
				return;
			}
			Logger.InitLogger();
			PxlsLoader.InitInstance(base.GetComponent<PxlsLoader>());
			IN.screen_width = Screen.width;
			IN.screen_height = Screen.height;
			IN.gui_layer = IN.LAY(IN.gui_layer_name);
			Application.targetFrameRate = 60;
			if (!IN.checkLocalFileAvailable())
			{
				SceneManager.LoadScene("SceneCannotLaunch");
				global::UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			REG.initReg();
			X.loadDebug();
			IN.FD_WindowSizeChanged = new IN.FnWindowSizeChanged(IN.fnWindowSizeChanged);
			Application.targetFrameRate = 60;
			IN.wh = IN.w / 2f;
			IN.hh = IN.h / 2f;
			IN.pixel_scale = X.Mx(0.25f, PlayerPrefs.GetFloat(Application.productName + "-PixelScale", 1f));
			X._stage = this;
			IN._stage = this;
			IN.Trs = base.transform;
			IN.Pauser = new PAUSER();
			TX.initTx();
			SND.initSoundList();
			X.init1();
			MTRX.init1();
			EffectItemGbg.initEffectItemGbg();
			if (IN.init_key_and_text_when_awake)
			{
				IN.createKeyInstanceInner();
			}
			else
			{
				base.enabled = false;
			}
			base.gameObject.layer = IN.gui_layer;
			IN.GuiPxCamera.OnEnable();
		}

		private static void createKeyInstanceInner()
		{
			IN.KA = new KEY(IN._stage.GetComponent<PlayerInput>(), null);
			IN.KA.PlayerCon.actions["MWheel"].performed += IN.OnMouseWheelChanged;
			ReadOnlyArray<InputDevice> devices = InputSystem.devices;
			for (int i = devices.Count - 1; i >= 0; i--)
			{
				if (devices[i].layout.IndexOf("HID::") >= 0)
				{
					IN.connected_hid_device = devices[i].layout;
					break;
				}
			}
			IN.KA.fine_pad_input_label = true;
			IN.KA.countupId();
			InputSystem.onDeviceChange += delegate(InputDevice device, InputDeviceChange change)
			{
				switch (change)
				{
				case InputDeviceChange.Added:
				case InputDeviceChange.Reconnected:
					if (device.layout.IndexOf("HID::") >= 0)
					{
						IN.connected_hid_device = device.layout;
					}
					IN.KA.fine_pad_input_label = true;
					IN.KA.countupId();
					IN.KA.PlayerCon.SwitchCurrentControlScheme(new InputDevice[] { device });
					IN.KA.UpdateControleScheme();
					break;
				case InputDeviceChange.Removed:
					if (device.layout == IN.connected_hid_device)
					{
						IN.connected_hid_device = null;
					}
					break;
				}
				IN.KA.FnSwitchedDevice();
			};
		}

		public static void Loa1dFinished()
		{
			IN.MdBounds = MeshDrawer.prepareMeshRenderer(IN._stage.gameObject, MTRX.MtrMeshNormal, -9.4999f, 3999, null, true, false);
			IN.ValotBounds = IN._stage.gameObject.GetComponent<ValotileRenderer>();
			IN.FD_WindowSizeChanged(Screen.width, Screen.height);
		}

		private void OnApplicationFocus(bool focus)
		{
			IN.application_focus = focus;
			if (!focus)
			{
				if (IN.KA != null)
				{
					IN.KA.clearPress();
				}
				IN.clearCursDown();
			}
			ReadOnlyArray<InputDevice> devices = InputSystem.devices;
			for (int i = devices.Count - 1; i >= 0; i--)
			{
				try
				{
					InputSystem.ResetDevice(devices[i], true);
				}
				catch
				{
				}
			}
			if (IN.KA != null)
			{
				if (focus)
				{
					IN.KA.clearPushDown(true);
					IN.KA.clearArrowPD(15U, true, 1);
					IN.clearCursDown();
				}
				IN.KA.FnSwitchedDevice();
				IN.fineMousePosition(global::UnityEngine.InputSystem.Pointer.current);
			}
		}

		public static void LoadScene(string scenename)
		{
			Logger.scene_changing = true;
			SceneManager.LoadScene(scenename);
		}

		public void OnApplicationQuit()
		{
			Logger.close(true);
		}

		public static string getApplicationName(string default_title)
		{
			try
			{
				string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.exe");
				if (files == null || files.Length == 0)
				{
					return default_title;
				}
				int num = files.Length;
				for (int i = 0; i < num; i++)
				{
					string fileName = Path.GetFileName(files[i]);
					if (TX.isStart(fileName, default_title, 0))
					{
						return fileName;
					}
				}
				return Path.GetFileName(files[0]);
			}
			catch
			{
			}
			return default_title;
		}

		private static bool checkLocalFileAvailable()
		{
			string text = "mti_shader.dat";
			return File.Exists(Path.Combine(Application.streamingAssetsPath, text));
		}

		public static void initKeyAndTextLocalization(bool if_not_loaded = false, bool reload_tx = true)
		{
			if (if_not_loaded && TX.isInitted)
			{
				return;
			}
			if (reload_tx)
			{
				TX.reloadTx(false);
			}
			if (IN.KA == null)
			{
				IN.createKeyInstanceInner();
			}
			IN._stage.enabled = true;
		}

		public static void setFullScreenMode(bool is_fullscreen, float _set_scale = 0f)
		{
			X.dl("FULLSCREEN TO " + is_fullscreen.ToString(), null, false, false);
			float num = X.Mx(0.25f, (_set_scale > 0f) ? _set_scale : IN.pixel_scale);
			Screen.SetResolution((int)(IN.w * num), (int)(IN.h * num), is_fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed, 60);
			IN._stage.StartCoroutine("resolution_set_finished");
		}

		private IEnumerator resolution_set_finished()
		{
			yield return null;
			IN.getGUICamera();
			try
			{
				IN.FD_WindowSizeChanged(Screen.width, Screen.height);
				yield break;
			}
			catch
			{
				yield break;
			}
			yield break;
		}

		public static void addCameraObject(GameObject Gob, bool fix_rect = true)
		{
			if (IN.ACameraGob != null)
			{
				IN.ACameraGob.Add(Gob);
				if (fix_rect)
				{
					IN.cameraFixSize(Gob, IN.getCameraRectForApp());
				}
			}
		}

		public static void remCameraObject(GameObject Gob)
		{
			if (IN.ACameraGob != null)
			{
				IN.ACameraGob.Remove(Gob);
			}
		}

		public static Rect getCameraRectForApp()
		{
			float num = 1.7777778f;
			float num2 = (float)IN.screen_width / (float)IN.screen_height / num;
			Rect rect = new Rect(0f, 0f, 0f, 0f);
			if (num2 < 1f)
			{
				rect.width = 1f;
				rect.height = num2;
				rect.x = 0f;
				rect.y = (1f - num2) / 2f;
			}
			else
			{
				float num3 = 1f / num2;
				rect.width = num3;
				rect.height = 1f;
				rect.x = (1f - num3) / 2f;
				rect.y = 0f;
			}
			return rect;
		}

		private static void fnWindowSizeChanged(int width, int height)
		{
			int count = IN.ACameraGob.Count;
			IN.screen_width = width;
			IN.screen_height = height;
			Rect cameraRectForApp = IN.getCameraRectForApp();
			for (int i = 0; i < count; i++)
			{
				IN.cameraFixSize(IN.ACameraGob[i], cameraRectForApp);
			}
			float num = ((IN.GuiPxCamera != null) ? IN.GuiPxCamera.pixel_scale : 1f);
			if (IN.pixel_scale != num)
			{
				IN.pixel_scale = num;
				PlayerPrefs.SetFloat(Application.productName + "-PixelScale", IN.pixel_scale);
				IN.save_prefs = true;
			}
			if (IN.ValotBounds.enabled)
			{
				IN.redrawBounds();
			}
			else
			{
				IN.setScreenOrtho(true);
			}
			CURS.mouse_base_scale = (Screen.fullScreen ? (IN.pixel_scale * 0.5f) : 1f);
		}

		private static void redrawBounds()
		{
			if ((float)IN.screen_width > IN.w * IN.pixel_scale || (float)IN.screen_height > IN.h * IN.pixel_scale)
			{
				IN.MdBounds.Col = MTRX.ColBlack;
				IN.MdBounds.RectDoughnut(0f, 0f, (float)(IN.screen_width + 64) / IN.pixel_scale, (float)(IN.screen_height + 64) / IN.pixel_scale, 0f, 0f, IN.w, IN.h, false, 0f, 0f, false);
				IN.MdBounds.updateForMeshRenderer(false);
				return;
			}
			IN.MdBounds.clear(false, false);
		}

		private static void cameraFixSize(GameObject Gob, Rect Rc)
		{
			try
			{
				Camera camera;
				if (Gob.TryGetComponent<Camera>(out camera))
				{
					PerfectPixelCamera perfectPixelCamera;
					if (Gob.TryGetComponent<PerfectPixelCamera>(out perfectPixelCamera))
					{
						perfectPixelCamera.cameraFixSize(Rc);
						CameraBidingsBehaviour cameraBidingsBehaviour;
						if (Gob.TryGetComponent<CameraBidingsBehaviour>(out cameraBidingsBehaviour))
						{
							cameraBidingsBehaviour.need_fine_ortho = true;
						}
					}
					else
					{
						camera.rect = Rc;
					}
				}
			}
			catch
			{
			}
		}

		private void OnDestroy()
		{
			if (IN._stage == this)
			{
				IN._stage = null;
				CURS.destruct();
			}
		}

		private void Start()
		{
			global::UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}

		public static GameObject CreateGob(MonoBehaviour Parent, string append_name = "-child")
		{
			return IN.CreateGob((Parent == null) ? null : Parent.gameObject, append_name);
		}

		public static GameObject CreateGob(GameObject Parent, string append_name = "-child")
		{
			GameObject gameObject;
			if (Parent != null)
			{
				gameObject = new GameObject(Parent.name + append_name);
				gameObject.transform.SetParent(Parent.transform, false);
				gameObject.layer = Parent.layer;
				gameObject.tag = Parent.tag;
			}
			else
			{
				gameObject = new GameObject(append_name);
			}
			return gameObject;
		}

		public static GameObject CreateGobGUI(GameObject Parent, string append_name = "-child")
		{
			GameObject gameObject = IN.CreateGob(Parent, append_name);
			gameObject.layer = IN.LAY(IN.gui_layer_name);
			return gameObject;
		}

		public static GameObject AttachZero(GameObject _S, Transform Parent = null)
		{
			GameObject gameObject = global::UnityEngine.Object.Instantiate<GameObject>(_S, Vector3.zero, Quaternion.identity);
			if (Parent != null)
			{
				gameObject.transform.SetParent(Parent);
			}
			return gameObject;
		}

		public static Transform FindChild(Transform Trs, string name)
		{
			if (Trs == null)
			{
				Trs = IN._stage.transform.root;
			}
			foreach (object obj in Trs.transform)
			{
				Transform transform = (Transform)obj;
				if (transform.gameObject.name == name)
				{
					return transform;
				}
			}
			return null;
		}

		public static Camera getGUICamera()
		{
			if (IN.GuiCamera == null)
			{
				GameObject gameObject = GameObject.Find("UCamera");
				if (gameObject != null)
				{
					IN.GuiCamera = gameObject.GetComponent<Camera>();
				}
				if (IN.GuiCamera == null)
				{
					IN.GuiCamera = Camera.main;
				}
				if (IN.KA != null)
				{
					IN.KA.PlayerCon.camera = IN.GuiCamera;
				}
				gameObject = IN.GuiCamera.gameObject;
				IN.GuiPxCamera = IN.GuiCamera.GetComponent<PerfectPixelCamera>();
				IN.GuiPxCamera != null;
				IN.cameraFixSize(gameObject, IN.getCameraRectForApp());
				CameraBidingsBehaviour.UiBind = IN.GetOrAdd<CameraBidingsBehaviour>(gameObject);
				IN.addCameraObject(gameObject, true);
			}
			return IN.GuiCamera;
		}

		public static PerfectPixelCamera getGUIPxCamera()
		{
			IN.getGUICamera();
			return IN.GuiPxCamera;
		}

		public static void setScreenOrtho(bool flag)
		{
			if (IN.ValotBounds != null)
			{
				Camera guicamera = IN.getGUICamera();
				if (flag)
				{
					int num = Screen.width / 2;
					int num2 = Screen.height / 2;
					guicamera.projectionMatrix = Matrix4x4.Ortho((float)(-(float)num) * 0.015625f, (float)(Screen.width - num) * 0.015625f, (float)(-(float)num2) * 0.015625f, (float)(Screen.height - num2) * 0.015625f, guicamera.nearClipPlane, guicamera.farClipPlane);
					IN.GuiPxCamera.enabled = false;
					Logger.finePos(flag);
				}
				else
				{
					IN.GuiPxCamera.enabled = true;
				}
				bool flag2 = !flag;
				if (IN.ValotBounds.enabled != flag2)
				{
					if (flag)
					{
						IN.MdBounds.clear(false, false);
					}
					IN.ValotBounds.enabled = flag2;
					CameraBidingsBehaviour.UiBind.need_fine_ortho = true;
					if (!flag)
					{
						Logger.finePos(flag);
						IN.redrawBounds();
					}
				}
			}
		}

		public static bool screen_ortho_flag
		{
			get
			{
				return !IN.ValotBounds.enabled;
			}
		}

		public static float screen_visible_w
		{
			get
			{
				if (!IN.screen_ortho_flag)
				{
					return IN.w;
				}
				return (float)Screen.width;
			}
		}

		public static float screen_visible_h
		{
			get
			{
				if (!IN.screen_ortho_flag)
				{
					return IN.h;
				}
				return (float)Screen.height;
			}
		}

		public static void resort_ui_bind_valotile()
		{
			CameraBidingsBehaviour.UiBind.need_sort_binds = true;
		}

		public static GameObject Find(string name, GameObject Gob = null)
		{
			return IN.Find(name, (Gob == null) ? IN._stage.transform.root : Gob.transform);
		}

		public static GameObject Find(string name, Transform Trs)
		{
			if (Trs == null)
			{
				Trs = IN._stage.transform.root;
			}
			string text = "";
			int num = name.IndexOf("/");
			if (num >= 0)
			{
				text = TX.slice(name, num + 1);
				name = TX.slice(name, 0, num);
			}
			foreach (object obj in Trs.transform)
			{
				Transform transform = (Transform)obj;
				GameObject gameObject = transform.gameObject;
				if (gameObject.name == name)
				{
					return (text != "") ? IN.Find(text, transform) : gameObject;
				}
			}
			return null;
		}

		public static Transform Scl(Transform Tr, float x, float y, float z = 1f)
		{
			Tr.localScale = new Vector3(x, y, z);
			return Tr;
		}

		public static Transform Scl(GameObject Tr, float x, float y, float z = 1f)
		{
			return IN.Scl(Tr.transform, x, y, z);
		}

		public static Transform Scl(MonoBehaviour Tr, float x, float y, float z = 1f)
		{
			return IN.Scl(Tr.transform, x, y, z);
		}

		public static Transform Pos(Transform Tr, float x, float y, float z = 0f)
		{
			Tr.localPosition = new Vector3(x, y, z);
			return Tr;
		}

		public static Transform Pos(GameObject Tr, float x, float y, float z = 0f)
		{
			return IN.Pos(Tr.transform, x, y, z);
		}

		public static Transform Pos(MonoBehaviour Tr, float x, float y, float z = 0f)
		{
			return IN.Pos(Tr.transform, x, y, z);
		}

		public static Transform PosP(Transform Tr, float x, float y, float z = 0f)
		{
			if (float.IsNaN(y))
			{
				y = 0f;
			}
			Tr.localPosition = new Vector3(x * 0.015625f, y * 0.015625f, z);
			return Tr;
		}

		public static Transform PosP(GameObject Tr, float x, float y, float z = 0f)
		{
			if (float.IsNaN(y))
			{
				y = 0f;
			}
			return IN.Pos(Tr.transform, x * 0.015625f, y * 0.015625f, z);
		}

		public static Transform PosP(MonoBehaviour Tr, float x, float y, float z = 0f)
		{
			if (float.IsNaN(y))
			{
				y = 0f;
			}
			return IN.Pos(Tr.transform, x * 0.015625f, y * 0.015625f, z);
		}

		public static Transform Pos2SetNotGoBeyondAbs(IDesignerBlock Blk, float scale = 1f, float margin_px = 0f)
		{
			Transform transform = Blk.getTransform();
			Vector3 position = transform.position;
			float num = (Blk.get_swidth_px() * scale / 2f + margin_px) * 0.015625f;
			float num2 = (Blk.get_sheight_px() * scale / 2f + margin_px) * 0.015625f;
			float num3 = IN.w / 2f * 0.015625f;
			float num4 = IN.h / 2f * 0.015625f;
			position.x = X.MMX(-num3 + num, position.x, num3 - num);
			position.y = X.MMX(-num4 + num2, position.y, num4 - num2);
			transform.position = position;
			return transform;
		}

		public static Transform Pos2(Transform Tr, float x, float y)
		{
			if (float.IsNaN(y))
			{
				y = 0f;
			}
			return IN.Pos(Tr, x, y, Tr.transform.localPosition.z);
		}

		public static Transform Pos2Abs(Transform Tr, float x, float y)
		{
			if (float.IsNaN(y))
			{
				y = 0f;
			}
			Tr.position = new Vector3(x, y, Tr.transform.position.z);
			return Tr;
		}

		public static Transform Pos2(Transform Tr, Vector2 P)
		{
			return IN.Pos(Tr, P.x, P.y, Tr.transform.localPosition.z);
		}

		public static Transform PosP2(Transform Tr, float x, float y)
		{
			if (float.IsNaN(y))
			{
				y = 0f;
			}
			return IN.Pos(Tr, x * 0.015625f, y * 0.015625f, Tr.transform.localPosition.z);
		}

		public static Transform PosP2Abs(Transform Tr, float x, float y)
		{
			return IN.Pos2Abs(Tr, x * 0.015625f, y * 0.015625f);
		}

		public static Transform Pos2GoSide(IDesignerBlock Tr, int aim_bit, float margin_x, float margin_y, IDesignerBlock Base)
		{
			margin_x += Base.get_swidth_px() / 2f + Tr.get_swidth_px() / 2f;
			margin_y += Base.get_sheight_px() / 2f + Tr.get_sheight_px() / 2f;
			return IN.Pos2GoSide(Tr.getTransform(), aim_bit, margin_x, margin_y, Base.getTransform());
		}

		public static Transform Pos2GoSide(Transform Tr, int aim_bit, float margin_x, float margin_y, Transform Base)
		{
			Vector3 position = Base.position;
			if ((aim_bit & 1) > 0 && (aim_bit & 4) > 0)
			{
				if (position.x > 0f)
				{
					aim_bit &= -5;
				}
				else
				{
					aim_bit &= -2;
				}
			}
			if ((aim_bit & 2) > 0 && (aim_bit & 8) > 0)
			{
				if ((double)position.y > (double)(-(double)Screen.height) * 0.25 * 0.015625)
				{
					aim_bit &= -3;
				}
				else
				{
					aim_bit &= -9;
				}
			}
			if ((aim_bit & 1) == 0 && (aim_bit & 4) == 0)
			{
				margin_x = 0f;
			}
			else
			{
				margin_x *= (float)X.MPF((aim_bit & 4) > 0);
			}
			if ((aim_bit & 2) == 0 && (aim_bit & 8) == 0)
			{
				margin_y = 0f;
			}
			else
			{
				margin_y *= (float)X.MPF((aim_bit & 2) > 0);
			}
			position.x += margin_x * 0.015625f;
			position.y += margin_y * 0.015625f;
			position.z = Tr.position.z;
			Tr.position = position;
			return Tr;
		}

		public static Transform setZ(Transform Tr, float z = 0f)
		{
			Vector3 localPosition = Tr.localPosition;
			localPosition.z = z;
			Tr.localPosition = localPosition;
			return Tr;
		}

		public static Transform setZAbs(Transform Tr, float z = 0f)
		{
			Vector3 position = Tr.position;
			position.z = z;
			Tr.position = position;
			return Tr;
		}

		public static int LAYB(string lay_name)
		{
			return 1 << IN.LAY(lay_name);
		}

		public static int LAY(string lay_name)
		{
			return LayerMask.NameToLayer(lay_name);
		}

		public static void setLayerWithChildren(Transform Trs, int layer)
		{
			Trs.gameObject.layer = layer;
			foreach (object obj in Trs)
			{
				IN.setLayerWithChildren((Transform)obj, layer);
			}
		}

		public static T GetOrAdd<T>(GameObject gob) where T : Component
		{
			T t;
			IN.GetOrAdd<T>(gob, out t);
			return t;
		}

		public static bool GetOrAdd<T>(GameObject gob, out T Out) where T : Component
		{
			bool flag = false;
			if (!gob.TryGetComponent<T>(out Out))
			{
				Out = gob.AddComponent<T>();
				flag = true;
			}
			return flag;
		}

		public static void DestroyOne(global::UnityEngine.Object _S)
		{
			if (_S != null)
			{
				global::UnityEngine.Object.Destroy(_S);
			}
		}

		public static void DestroyE(MonoBehaviour _S)
		{
			if (_S != null)
			{
				_S.enabled = false;
				global::UnityEngine.Object.Destroy(_S);
			}
		}

		public static void DestroyE(GameObject _S)
		{
			if (_S != null)
			{
				_S.SetActive(false);
				global::UnityEngine.Object.Destroy(_S);
			}
		}

		public static void clear()
		{
			IN.KA.clearPress();
			IN.pd_bits &= (IN.PB)4294963200U;
			IN.pd_bits |= IN.PB._LOCK_UI_KEY;
			IN.press_mvSubmit = (IN.press_mvCancel = (IN.press_mvL = (IN.press_mvR = (IN.press_mvB = (IN.press_mvT = 0f)))));
		}

		public static void clearCursDown()
		{
			IN.pd_bits &= (IN.PB)4294963200U;
		}

		public static void clearPushDown(bool strong = false)
		{
			IN.pd_bits = IN.PB._LOCK_UI_KEY;
			if (strong)
			{
				IN.press_mvSubmit = (IN.press_mvCancel = (IN.press_mvL = (IN.press_mvR = (IN.press_mvB = (IN.press_mvT = 0f)))));
			}
			IN.clearSubmitPushDown(strong);
		}

		public static void clearArrowPD(uint bits, bool strong = false, int hold_update = 20)
		{
			IN.KA.clearArrowPD(bits, strong, hold_update);
			IN.pd_bits |= IN.PB._LOCK_UI_KEY;
			IN.clearPressHolding(ref IN.press_mvL, strong);
			IN.clearPressHolding(ref IN.press_mvT, strong);
			IN.clearPressHolding(ref IN.press_mvR, strong);
			IN.clearPressHolding(ref IN.press_mvB, strong);
		}

		public static void clearSubmitPushDown(bool strong = false)
		{
			IN.KA.clearPushDown(strong);
			IN.pd_bits &= (IN.PB)4294967039U;
			IN.pd_bits |= IN.PB._LOCK_UI_KEY;
			IN.clearPressHolding(ref IN.press_mvSubmit, strong);
			if (IN.mvMouse > 0f)
			{
				IN.mvMouse = -1024f;
			}
		}

		public static void clearMenuPushDown(bool strong = false)
		{
			IN.KA.clearMenuPushDown(strong);
		}

		public static void clearCancelPushDown(bool strong = false)
		{
			IN.pd_bits &= (IN.PB)4294966783U;
			IN.pd_bits |= IN.PB._LOCK_UI_KEY;
			IN.clearPressHolding(ref IN.press_mvCancel, strong);
			IN.KA.clearCancelPushDown(strong);
		}

		private void Update()
		{
			if (!IN.enable_vsync_)
			{
				IN.run(1f);
			}
		}

		private void FixedUpdate()
		{
			if (IN.enable_vsync_)
			{
				IN.run(1f);
			}
		}

		private static void run(float run_fcnt = 1f)
		{
			Bench.clearClosure();
			if (IN.save_prefs)
			{
				PlayerPrefs.Save();
				IN.save_prefs = false;
			}
			CURS.run(run_fcnt);
			if (SlideAnim.sli_i > 0)
			{
				SlideAnim.runSlideAnimator(1f);
			}
			X.D = (X.D_EF = false);
			int num = 4;
			float num2 = 1f;
			IN.totalFrameProgress(ref num2, ref num, true);
			IN.deltaFrameR_ = 0f;
			if (LabeledInputField.focus_exist_t > 0f)
			{
				LabeledInputField.focus_exist_t = X.Mx(LabeledInputField.focus_exist_t - run_fcnt, 0f);
			}
			if (IN.application_focus)
			{
				InputSystem.Update();
				Bench.P("KA");
				IN.KA.updatePressing(run_fcnt);
				Bench.Pend("KA");
				if (IN.mvMouse > 0f)
				{
					IN.mvMouse += run_fcnt;
				}
				else if (IN.mvMouse < 0f && IN.mvMouse > -1024f)
				{
					IN.mvMouse = 0f;
				}
				IN.pd_bits = (IN.PB)0U;
				IN.checkKeyPressPD(IN.PB.L, IN.KA.mvLA, ref IN.press_mvL, run_fcnt, false, true);
				IN.checkKeyPressPD(IN.PB.R, IN.KA.mvRA, ref IN.press_mvR, run_fcnt, false, true);
				if (IN.press_mvL >= 100f || IN.press_mvR >= 100f)
				{
					X.Mx(IN.KA.mvLA, IN.KA.mvRA);
					IN.checkKeyPressPD(IN.PB.T, IN.KA.mvTA, ref IN.press_mvT, run_fcnt, false, false);
					IN.checkKeyPressPD(IN.PB.B, IN.KA.mvBA, ref IN.press_mvB, run_fcnt, false, false);
					if (IN.KA.mvTA > 0f && IN.press_mvT >= 18f && (IN.pd_bits & IN.PB.LR) != (IN.PB)0U)
					{
						IN.pd_bits |= IN.PB.T;
					}
					if (IN.KA.mvBA > 0f && IN.press_mvB >= 18f && (IN.pd_bits & IN.PB.LR) != (IN.PB)0U)
					{
						IN.pd_bits |= IN.PB.B;
					}
				}
				else
				{
					IN.checkKeyPressPD(IN.PB.T, IN.KA.mvTA, ref IN.press_mvT, run_fcnt, false, true);
					IN.checkKeyPressPD(IN.PB.B, IN.KA.mvBA, ref IN.press_mvB, run_fcnt, false, true);
				}
				IN.checkKeyPressPD(IN.PB.SUBMIT, IN.KA.mvSUBMIT, ref IN.press_mvSubmit, run_fcnt, true, true);
				IN.checkKeyPressPD(IN.PB.CANCEL, IN.KA.mvCANCEL, ref IN.press_mvCancel, run_fcnt, true, true);
				global::UnityEngine.InputSystem.Pointer current = global::UnityEngine.InputSystem.Pointer.current;
				if (current != null)
				{
					IN.fineMousePosition(current);
					if (!IN.mouse_area_out && current.press.isPressed)
					{
						if (IN.mvMouse == 0f)
						{
							IN.mvMouse = 1f;
						}
					}
					else if (IN.mvMouse > 0f)
					{
						IN.mvMouse = -IN.mvMouse;
					}
					else if (IN.mvMouse == -1024f)
					{
						IN.mvMouse = 0f;
					}
				}
				else
				{
					IN.use_mouse = false;
					if (IN.mvMouse > 0f)
					{
						IN.mvMouse = -IN.mvMouse;
					}
				}
			}
			else
			{
				IN.use_mouse = false;
				if (IN.mvMouse > 0f)
				{
					IN.mvMouse = -IN.mvMouse;
				}
				else if (IN.mvMouse < 0f && IN.mvMouse > -1024f)
				{
					IN.mvMouse = 0f;
				}
			}
			Bench.P("BGM");
			BGM.runBGM(run_fcnt);
			Bench.Pend("BGM");
			if (!X.DEBUGNOSND)
			{
				Bench.P("SND");
				SND.runSND();
				Bench.Pend("SND");
			}
			if (MTRX.prepared)
			{
				Bench.P("Storage");
				MTRX.checkStorage();
				Bench.Pend("Storage");
				if ((IN.totalframe & 15) == 0)
				{
					int width = Screen.width;
					int height = Screen.height;
					if (IN.screen_width != width || IN.screen_height != height)
					{
						IN.FD_WindowSizeChanged(width, height);
					}
				}
			}
			if (IN.use_mouse && IN.FlgUiUse.isActive())
			{
				IN.Click.run(IN.getMousePos(IN.getGUICamera()));
			}
			IN.runner_running_i = 0;
			Bench.P("IN-Runner");
			while (IN.runner_running_i < IN.ARunner.Count)
			{
				IRunAndDestroy runAndDestroy = IN.ARunner[IN.runner_running_i];
				string text = Bench.P(runAndDestroy.ToString());
				if (!runAndDestroy.run(run_fcnt))
				{
					if (IN.runner_running_i >= 0 && IN.runner_running_i < IN.ARunner.Count && IN.ARunner[IN.runner_running_i] == runAndDestroy)
					{
						IN.ARunner.RemoveAt(IN.runner_running_i);
					}
					else
					{
						IN.runner_running_i++;
					}
				}
				else
				{
					IN.runner_running_i++;
				}
				Bench.Pend(text);
			}
			Bench.Pend("IN-Runner");
			IN.runner_running_i = -1;
			Bench.P_allClose();
		}

		private static void totalFrameProgress(ref float f, ref int _max_step, bool add_vdelta = false)
		{
			while (f >= 1f)
			{
				int num = _max_step - 1;
				_max_step = num;
				if (num < 0)
				{
					break;
				}
				f -= 1f;
				IN.totalframe++;
				X.D = X.D || X.AF <= 1 || IN.totalframe % X.AF == 0;
				X.D_EF = X.D_EF || X.AF_EF <= 1 || IN.totalframe % X.AF_EF == 0;
			}
		}

		private static float checkKeyPressPD(IN.PB pb, float mv, ref float press_t, float fcnt, bool strict_interval = false, bool over_pressable = true)
		{
			if (mv <= 0f)
			{
				press_t = 0f;
			}
			else if (press_t == 0f)
			{
				if (mv == 1f)
				{
					press_t = fcnt;
					IN.pd_bits |= pb;
				}
			}
			else if (over_pressable)
			{
				bool flag;
				if (press_t < 100f)
				{
					press_t += fcnt;
					flag = press_t >= 18f;
				}
				else
				{
					press_t += fcnt;
					flag = press_t >= 200f;
				}
				if (flag)
				{
					press_t = (float)(200 - (strict_interval ? 18 : 6));
					IN.pd_bits |= pb;
				}
			}
			else if (press_t < 18f)
			{
				press_t += fcnt;
			}
			return press_t;
		}

		private static void clearPressHolding(ref float press_mv, bool strong)
		{
			if (strong)
			{
				press_mv = 0f;
			}
		}

		public static void LoggerStbAdd(STB Stb)
		{
		}

		private static void fineMousePosition(global::UnityEngine.InputSystem.Pointer PtC)
		{
			if (PtC == null)
			{
				return;
			}
			Vector2 vector = PtC.position.ReadValue();
			Vector2 vector2 = new Vector2(X.MMX(0f, vector.x, (float)Screen.width), X.MMX(0f, vector.y, (float)Screen.height));
			IN.mouse_area_out = !vector2.Equals(vector);
			if (!vector.Equals(IN.Mouse))
			{
				if (!IN.use_mouse)
				{
					IN.use_mouse = true;
				}
				if (!IN.mouse_area_out)
				{
					IN.Click.need_fine = true;
				}
				IN.Mouse = vector;
			}
		}

		public static bool use_mouse
		{
			get
			{
				return IN.use_mouse_;
			}
			set
			{
				if (IN.use_mouse == value)
				{
					return;
				}
				IN.use_mouse_ = value;
				if (!IN.FlgUiUse.isActive())
				{
					return;
				}
				if (value)
				{
					IN.Click.need_fine = true;
					return;
				}
				IN.Click.Blur();
			}
		}

		public static bool enable_vsync
		{
			get
			{
				return IN.enable_vsync_;
			}
			set
			{
				IN.enable_vsync_ = value;
				QualitySettings.vSyncCount = (value ? 1 : 0);
				if (value)
				{
					Time.maximumDeltaTime = 0.05f;
					X.dl("V-Sync enabled", null, false, false);
				}
			}
		}

		public static float deltaFrameR
		{
			get
			{
				if (IN.deltaFrameR_ == 0f)
				{
					IN.deltaFrameR_ = 1f / IN.deltaFrame;
				}
				return IN.deltaFrameR_;
			}
		}

		public static float deltaFrame
		{
			get
			{
				if (!IN.enable_vsync)
				{
					return Time.fixedUnscaledDeltaTime * 60f;
				}
				return X.Mn(4f, Time.unscaledDeltaTime * 60f);
			}
		}

		public static float VF
		{
			get
			{
				return IN.deltaFrame;
			}
		}

		public static Vector3 MouseWorld
		{
			get
			{
				return IN.getGUICamera().ScreenToWorldPoint(IN.Mouse);
			}
		}

		public static bool isPadCancelOrSubmit()
		{
			return false;
		}

		public static bool isR()
		{
			return (IN.pd_bits & IN.PB.R) > (IN.PB)0U;
		}

		public static bool isL()
		{
			return (IN.pd_bits & IN.PB.L) > (IN.PB)0U;
		}

		public static bool isT()
		{
			return (IN.pd_bits & IN.PB.T) > (IN.PB)0U;
		}

		public static bool isB()
		{
			return (IN.pd_bits & IN.PB.B) > (IN.PB)0U;
		}

		public static int cursXD(bool pressing = false)
		{
			if (!pressing)
			{
				return (IN.isL() ? (-1) : 0) + (IN.isR() ? 1 : 0);
			}
			return (IN.isLO(0) ? (-1) : 0) + (IN.isRO(0) ? 1 : 0);
		}

		public static int cursYD(bool pressing = false)
		{
			if (!pressing)
			{
				return (IN.isT() ? 1 : 0) + (IN.isB() ? (-1) : 0);
			}
			return (IN.isTO(0) ? 1 : 0) + (IN.isBO(0) ? (-1) : 0);
		}

		public static bool isRP(int press_max = 1)
		{
			return IN.KA.mvRA >= 1f && IN.KA.mvRA <= (float)press_max;
		}

		public static bool isLP(int press_max = 1)
		{
			return IN.KA.mvLA >= 1f && IN.KA.mvLA <= (float)press_max;
		}

		public static bool isTP(int press_max = 1)
		{
			return IN.KA.mvTA >= 1f && IN.KA.mvTA <= (float)press_max;
		}

		public static bool isBP(int press_max = 1)
		{
			return IN.KA.mvBA >= 1f && IN.KA.mvBA <= (float)press_max;
		}

		public static bool isRO(int press = 0)
		{
			return IN.KA.mvRA > (float)press;
		}

		public static bool isLO(int press = 0)
		{
			return IN.KA.mvLA > (float)press;
		}

		public static bool isTO(int press = 0)
		{
			return IN.KA.mvTA > (float)press;
		}

		public static bool isBO(int press = 0)
		{
			return IN.KA.mvBA > (float)press;
		}

		public static bool isRO_Long(int interval = 3)
		{
			return IN.KA.mvRA >= 15f && (IN.KA.mvRA - 15f) % (float)interval == 0f;
		}

		public static bool isLO_Long(int interval = 3)
		{
			return IN.KA.mvLA >= 15f && (IN.KA.mvLA - 15f) % (float)interval == 0f;
		}

		public static bool isTO_Long(int interval = 3)
		{
			return IN.KA.mvTA >= 15f && (IN.KA.mvTA - 15f) % (float)interval == 0f;
		}

		public static bool isBO_Long(int interval = 3)
		{
			return IN.KA.mvBA >= 15f && (IN.KA.mvBA - 15f) % (float)interval == 0f;
		}

		public static bool isRU()
		{
			return IN.KA.mvRA < 0f;
		}

		public static bool isLU()
		{
			return IN.KA.mvLA < 0f;
		}

		public static void clearLROutRelease()
		{
			IN.KA.mvRA_out = 0;
			IN.KA.mvLA_out = 0;
		}

		public static bool isRU(int holded_alloc, bool kill_flag = true)
		{
			if (IN.KA.mvRA_out > 0 && IN.KA.mvRA_out <= holded_alloc)
			{
				if (kill_flag)
				{
					IN.KA.mvRA_out = 0;
				}
				return true;
			}
			return false;
		}

		public static bool isLU(int holded_alloc, bool kill_flag = true)
		{
			if (IN.KA.mvLA_out > 0 && IN.KA.mvLA_out <= holded_alloc)
			{
				if (kill_flag)
				{
					IN.KA.mvLA_out = 0;
				}
				return true;
			}
			return false;
		}

		public static bool isTU()
		{
			return IN.KA.mvTA < 0f;
		}

		public static bool isBU()
		{
			return IN.KA.mvBA < 0f;
		}

		public static void clearKeyState(string t, bool only_pushdown_clear = false)
		{
			string text = t.ToUpper();
			if (text != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
				if (num <= 3238785555U)
				{
					if (num <= 467490188U)
					{
						if (num != 401953830U)
						{
							if (num != 467490188U)
							{
								return;
							}
							if (!(text == "BA"))
							{
								return;
							}
							if (!only_pushdown_clear)
							{
								IN.KA.mvBA = 0f;
								return;
							}
							if (IN.KA.mvBA == 1f)
							{
								IN.KA.mvBA += 1f;
								return;
							}
						}
						else
						{
							if (!(text == "TA"))
							{
								return;
							}
							if (!only_pushdown_clear)
							{
								IN.KA.mvTA = 0f;
								return;
							}
							if (IN.KA.mvTA == 1f)
							{
								IN.KA.mvTA += 1f;
								return;
							}
						}
					}
					else if (num != 1742883422U)
					{
						if (num != 2080701468U)
						{
							if (num != 3238785555U)
							{
								return;
							}
							if (!(text == "D"))
							{
								return;
							}
							if (!only_pushdown_clear)
							{
								IN.KA.mvD = 0f;
								return;
							}
							if (IN.KA.mvD == 1f)
							{
								IN.KA.mvD += 1f;
							}
						}
						else
						{
							if (!(text == "RA"))
							{
								return;
							}
							if (!only_pushdown_clear)
							{
								IN.KA.mvRA = 0f;
								return;
							}
							if (IN.KA.mvRA == 1f)
							{
								IN.KA.mvRA += 1f;
								return;
							}
						}
					}
					else
					{
						if (!(text == "LA"))
						{
							return;
						}
						if (!only_pushdown_clear)
						{
							IN.KA.mvLA = 0f;
							return;
						}
						if (IN.KA.mvLA == 1f)
						{
							IN.KA.mvLA += 1f;
							return;
						}
					}
				}
				else if (num <= 3322673650U)
				{
					if (num != 3289118412U)
					{
						if (num != 3322673650U)
						{
							return;
						}
						if (!(text == "C"))
						{
							return;
						}
						if (!only_pushdown_clear)
						{
							IN.KA.mvC = 0f;
							return;
						}
						if (IN.KA.mvC == 1f)
						{
							IN.KA.mvC += 1f;
							return;
						}
					}
					else
					{
						if (!(text == "A"))
						{
							return;
						}
						if (!only_pushdown_clear)
						{
							IN.KA.mvA = 0f;
							return;
						}
						if (IN.KA.mvA == 1f)
						{
							IN.KA.mvA += 1f;
							return;
						}
					}
				}
				else if (num != 3591115554U)
				{
					if (num != 3708558887U)
					{
						if (num != 3742114125U)
						{
							return;
						}
						if (!(text == "Z"))
						{
							return;
						}
						if (!only_pushdown_clear)
						{
							IN.KA.mvZ = 0f;
							return;
						}
						if (IN.KA.mvZ == 1f)
						{
							IN.KA.mvZ += 1f;
							return;
						}
					}
					else
					{
						if (!(text == "X"))
						{
							return;
						}
						if (!only_pushdown_clear)
						{
							IN.KA.mvX = 0f;
							return;
						}
						if (IN.KA.mvX == 1f)
						{
							IN.KA.mvX += 1f;
							return;
						}
					}
				}
				else
				{
					if (!(text == "S"))
					{
						return;
					}
					if (!only_pushdown_clear)
					{
						IN.KA.mvS = 0f;
						return;
					}
					if (IN.KA.mvS == 1f)
					{
						IN.KA.mvS += 1f;
						return;
					}
				}
			}
		}

		public static bool isKeyPD(string t)
		{
			string text = t.ToUpper();
			if (text != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
				if (num <= 3238785555U)
				{
					if (num <= 467490188U)
					{
						if (num != 401953830U)
						{
							if (num == 467490188U)
							{
								if (text == "BA")
								{
									return IN.isBP(1);
								}
							}
						}
						else if (text == "TA")
						{
							return IN.isTP(1);
						}
					}
					else if (num != 1742883422U)
					{
						if (num != 2080701468U)
						{
							if (num == 3238785555U)
							{
								if (text == "D")
								{
									return IN.isItmPD(1);
								}
							}
						}
						else if (text == "RA")
						{
							return IN.isRP(1);
						}
					}
					else if (text == "LA")
					{
						return IN.isLP(1);
					}
				}
				else if (num <= 3322673650U)
				{
					if (num != 3289118412U)
					{
						if (num == 3322673650U)
						{
							if (text == "C")
							{
								return IN.isTargettingPD(1);
							}
						}
					}
					else if (text == "A")
					{
						return IN.isMapPD(1);
					}
				}
				else if (num != 3591115554U)
				{
					if (num != 3708558887U)
					{
						if (num == 3742114125U)
						{
							if (text == "Z")
							{
								return IN.isAtkPD(1);
							}
						}
					}
					else if (text == "X")
					{
						return IN.isMagicPD(1);
					}
				}
				else if (text == "S")
				{
					return IN.isSPD();
				}
			}
			return false;
		}

		public static bool isKeyO(string t, int press = 0)
		{
			string text = t.ToUpper();
			if (text != null)
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
				if (num <= 3238785555U)
				{
					if (num <= 467490188U)
					{
						if (num != 401953830U)
						{
							if (num == 467490188U)
							{
								if (text == "BA")
								{
									return IN.isBO(press);
								}
							}
						}
						else if (text == "TA")
						{
							return IN.isTO(press);
						}
					}
					else if (num != 1742883422U)
					{
						if (num != 2080701468U)
						{
							if (num == 3238785555U)
							{
								if (text == "D")
								{
									return IN.isItmO(0);
								}
							}
						}
						else if (text == "RA")
						{
							return IN.isRO(press);
						}
					}
					else if (text == "LA")
					{
						return IN.isLO(press);
					}
				}
				else if (num <= 3322673650U)
				{
					if (num != 3289118412U)
					{
						if (num == 3322673650U)
						{
							if (text == "C")
							{
								return IN.isTargettingO(0);
							}
						}
					}
					else if (text == "A")
					{
						return IN.isMapO(0);
					}
				}
				else if (num != 3591115554U)
				{
					if (num != 3708558887U)
					{
						if (num == 3742114125U)
						{
							if (text == "Z")
							{
								return IN.isAtkO(0);
							}
						}
					}
					else if (text == "X")
					{
						return IN.isMagicO(0);
					}
				}
				else if (text == "S")
				{
					return IN.isSO();
				}
			}
			return false;
		}

		public static int getKeyInputByName(string name)
		{
			KEY.SIMKEY simkey = FEnum<KEY.SIMKEY>.Parse(name, (KEY.SIMKEY)0);
			int num;
			if (simkey <= KEY.SIMKEY.RTAB)
			{
				if (simkey == KEY.SIMKEY.LTAB)
				{
					num = (IN.KA.checkPD_manual(simkey) ? 1 : (IN.KA.checkO_manual(simkey) ? 2 : 0));
					goto IL_0102;
				}
				if (simkey == KEY.SIMKEY.RTAB)
				{
					num = (IN.KA.checkPD_manual(simkey) ? 1 : (IN.KA.checkO_manual(simkey) ? 2 : 0));
					goto IL_0102;
				}
			}
			else
			{
				if (simkey == KEY.SIMKEY.SORT)
				{
					num = (IN.KA.checkPD_manual(simkey) ? 1 : (IN.KA.checkO_manual(simkey) ? 2 : 0));
					goto IL_0102;
				}
				if (simkey == KEY.SIMKEY.ADD)
				{
					num = (IN.KA.checkPD_manual(simkey) ? 1 : (IN.KA.checkO_manual(simkey) ? 2 : 0));
					goto IL_0102;
				}
				if (simkey == KEY.SIMKEY.REM)
				{
					num = (IN.KA.checkPD_manual(simkey) ? 1 : (IN.KA.checkO_manual(simkey) ? 2 : 0));
					goto IL_0102;
				}
			}
			num = -1000;
			IL_0102:
			int num2 = num;
			if (num2 != -1000)
			{
				return num2;
			}
			try
			{
				FieldInfo field = typeof(KEY).GetField("mv" + name.ToUpper());
				if (field == null)
				{
					return -1000;
				}
				return (int)((float)field.GetValue(IN.KA));
			}
			catch
			{
			}
			return -1000;
		}

		public static bool isSubmit()
		{
			return (IN.pd_bits & IN.PB.SUBMIT) > (IN.PB)0U;
		}

		public static bool isSubmitPD(int alloc_pd_frame = 1)
		{
			return IN.KA.mvSUBMIT >= 1f && IN.KA.mvSUBMIT <= (float)alloc_pd_frame;
		}

		public static bool isSubmitUp(int pushing_time = -1)
		{
			return IN.KA.mvSUBMIT < 0f && (pushing_time < 0 || (float)(-(float)pushing_time) <= IN.KA.mvSUBMIT);
		}

		public static bool isSubmitOrMouseUp(int pushing_time = 1)
		{
			return IN.isSubmitUp(pushing_time) || IN.isMouseUp(pushing_time);
		}

		public static bool isCancel()
		{
			if ((IN.pd_bits & IN.PB.CANCEL) != (IN.PB)0U)
			{
				IN.use_mouse = false;
				return true;
			}
			return false;
		}

		public static bool isCancelPD()
		{
			if (IN.KA.mvCANCEL == 1f)
			{
				IN.use_mouse = false;
				return true;
			}
			return false;
		}

		public static bool isCancelOrReturnPD()
		{
			if (IN.KA.mvCANCEL == 1f || (GUI.GetNameOfFocusedControl() == "" && IN.KA.isReturnPD()))
			{
				IN.use_mouse = false;
				return true;
			}
			return false;
		}

		public static bool isSubmitOn(int press = 0)
		{
			return IN.KA.mvSUBMIT > (float)press;
		}

		public static bool isCancelOn(int press = 0)
		{
			return IN.KA.mvCANCEL > (float)press;
		}

		public static bool isCancelU()
		{
			return IN.KA.mvCANCEL < 0f && -1024f < IN.KA.mvCANCEL;
		}

		public static float skippingTS()
		{
			float num = 1f;
			if (IN.isMenuO(0))
			{
				num *= 3f;
			}
			if (IN.isCancelOn(0))
			{
				num *= 2f;
			}
			if (IN.isSubmitOn(0))
			{
				num *= 2f;
			}
			return X.Mn(6f, num);
		}

		public static float getBAOnTime()
		{
			return IN.KA.mvBA;
		}

		public static float getZOnTime()
		{
			return IN.KA.mvZ;
		}

		public static bool isMovingPD()
		{
			return IN.isLP(1) || IN.isRP(1) || IN.isBP(1) || IN.isTP(1) || IN.isJumpPD(1);
		}

		public static bool isAtkPD(int alloc_frame = 1)
		{
			return IN.KA.mvZ > 0f && IN.KA.mvZ <= (float)alloc_frame;
		}

		public static bool isAtkO(int press = 0)
		{
			return IN.KA.mvZ > (float)press;
		}

		public static bool isAtkU()
		{
			return IN.KA.mvZ < 0f && -1024f < IN.KA.mvZ;
		}

		public static bool isRunO(int press = 0)
		{
			return IN.KA.mvRUN > (float)press;
		}

		public static bool isRunPD(int alloc_frame = 1)
		{
			return IN.KA.mvRUN > 0f && IN.KA.mvRUN <= (float)alloc_frame;
		}

		public static bool isMagicPD(int alloc_frame = 1)
		{
			return IN.KA.mvX > 0f && IN.KA.mvX <= (float)alloc_frame;
		}

		public static bool isMagicO(int press = 0)
		{
			return IN.KA.mvX > (float)press;
		}

		public static bool isMagicU()
		{
			return IN.KA.mvX < 0f && -1024f < IN.KA.mvX;
		}

		public static bool isTargettingPD(int alloc_frame = 1)
		{
			return IN.KA.mvC > 0f && IN.KA.mvC <= (float)alloc_frame;
		}

		public static bool isTargettingPDC(int press_first = 18, int press_wait = 6)
		{
			if (IN.KA.mvC > (float)press_first)
			{
				return (IN.KA.mvC - (float)press_first) % (float)press_wait == 0f;
			}
			return IN.KA.mvC == 1f || IN.KA.mvC == (float)press_first;
		}

		public static bool isTargettingO(int press = 0)
		{
			return IN.KA.mvC > (float)press;
		}

		public static bool isTargettingU()
		{
			return IN.KA.mvC < 0f && -1024f < IN.KA.mvC;
		}

		public static bool isMapPD(int alloc_frame = 1)
		{
			return 0f < IN.KA.mvA && IN.KA.mvA <= (float)alloc_frame;
		}

		public static bool isMapPDC(int press_first = 18, int press_wait = 6)
		{
			if (IN.KA.mvA > (float)press_first)
			{
				return (IN.KA.mvA - (float)press_first) % (float)press_wait == 0f;
			}
			return IN.KA.mvA == 1f || IN.KA.mvA == (float)press_first;
		}

		public static bool isMapO(int press = 0)
		{
			return IN.KA.mvA > (float)press;
		}

		public static bool isMapU()
		{
			return IN.KA.mvA < 0f && -1024f < IN.KA.mvA;
		}

		public static bool isMapU(int pushing_time)
		{
			return IN.KA.mvA < 0f && (float)(-(float)pushing_time) < IN.KA.mvA;
		}

		public static bool isJumpPD(int alloc_frame = 1)
		{
			return IN.KA.mvJUMP > 0f && IN.KA.mvJUMP <= (float)alloc_frame;
		}

		public static bool isJumpO(int press = 0)
		{
			return IN.KA.mvJUMP > (float)press;
		}

		public static bool isJumpU()
		{
			return IN.KA.mvJUMP < 0f && -1024f < IN.KA.mvJUMP;
		}

		public static bool isLTabPD()
		{
			return (IN.pd_bits & IN.PB._LOCK_UI_KEY) == (IN.PB)0U && IN.KA.checkPD_manual(KEY.SIMKEY.LTAB);
		}

		public static bool isLTabO()
		{
			return (IN.pd_bits & IN.PB._LOCK_UI_KEY) == (IN.PB)0U && IN.KA.checkO_manual(KEY.SIMKEY.LTAB);
		}

		public static bool isRTabPD()
		{
			return (IN.pd_bits & IN.PB._LOCK_UI_KEY) == (IN.PB)0U && IN.KA.checkPD_manual(KEY.SIMKEY.RTAB);
		}

		public static bool isRTabO()
		{
			return (IN.pd_bits & IN.PB._LOCK_UI_KEY) == (IN.PB)0U && IN.KA.checkO_manual(KEY.SIMKEY.RTAB);
		}

		public static bool isUiAddPD()
		{
			return (IN.pd_bits & IN.PB._LOCK_UI_KEY) == (IN.PB)0U && IN.KA.checkPD_manual(KEY.SIMKEY.ADD);
		}

		public static bool isUiAddO()
		{
			return (IN.pd_bits & IN.PB._LOCK_UI_KEY) == (IN.PB)0U && IN.KA.checkO_manual(KEY.SIMKEY.ADD);
		}

		public static bool isUiRemPD()
		{
			return (IN.pd_bits & IN.PB._LOCK_UI_KEY) == (IN.PB)0U && IN.KA.checkPD_manual(KEY.SIMKEY.REM);
		}

		public static bool isUiRemO()
		{
			return (IN.pd_bits & IN.PB._LOCK_UI_KEY) == (IN.PB)0U && IN.KA.checkO_manual(KEY.SIMKEY.REM);
		}

		public static bool isUiSortPD()
		{
			return (IN.pd_bits & IN.PB._LOCK_UI_KEY) == (IN.PB)0U && IN.KA.checkPD_manual(KEY.SIMKEY.SORT);
		}

		public static bool isUiSortO()
		{
			return (IN.pd_bits & IN.PB._LOCK_UI_KEY) == (IN.PB)0U && IN.KA.checkO_manual(KEY.SIMKEY.SORT);
		}

		public static bool isUiShiftPD()
		{
			return (IN.pd_bits & IN.PB._LOCK_UI_KEY) == (IN.PB)0U && IN.KA.checkPD_manual(KEY.SIMKEY.SHIFT);
		}

		public static bool isUiShiftO()
		{
			return (IN.pd_bits & IN.PB._LOCK_UI_KEY) == (IN.PB)0U && IN.KA.checkO_manual(KEY.SIMKEY.SHIFT);
		}

		public static bool isSPD()
		{
			return IN.KA.mvS == 1f;
		}

		public static bool isSO()
		{
			return IN.KA.mvS > 0f;
		}

		public static bool isSU()
		{
			return IN.KA.mvS < 0f && -1024f < IN.KA.mvS;
		}

		public static bool isItmPD(int alloc_frame = 1)
		{
			return IN.KA.mvD > 0f && IN.KA.mvD <= (float)alloc_frame;
		}

		public static bool isItmO(int press = 0)
		{
			return IN.KA.mvD > (float)press;
		}

		public static bool isItmU(int overhold_time = 1)
		{
			return IN.KA.mvD < 0f && (float)(-(float)overhold_time) >= IN.KA.mvD;
		}

		public static bool isEvadePD(int alloc_frame = 1)
		{
			return IN.KA.mvLSH > 0f && IN.KA.mvLSH <= (float)alloc_frame;
		}

		public static bool isEvadeO(int press = 0)
		{
			return IN.KA.mvLSH > (float)press;
		}

		public static bool isEvadeU()
		{
			return IN.KA.mvLSH < 0f && -1024f < IN.KA.mvLSH;
		}

		public static bool isCheckPD(int alloc_frame = 1)
		{
			return IN.KA.mvCHECK > 0f && IN.KA.mvCHECK <= (float)alloc_frame;
		}

		public static bool isCheckO(int press = 0)
		{
			return IN.KA.mvCHECK > (float)press;
		}

		public static bool isCheckU()
		{
			return IN.KA.mvCHECK < 0f && -1024f < IN.KA.mvCHECK;
		}

		public static bool isMenuPD(int alloc_frame = 1)
		{
			if (0f < IN.KA.mvMENU && IN.KA.mvMENU <= (float)alloc_frame)
			{
				IN.use_mouse = false;
				return true;
			}
			return false;
		}

		public static bool isMenuO(int press = 0)
		{
			return IN.KA.mvMENU > (float)press;
		}

		public static bool isMenuU()
		{
			return IN.KA.mvMENU < 0f && -1024f < IN.KA.mvMENU;
		}

		public static bool isMagicNeutralPD(int alloc_frame = 1)
		{
			return IN.KA.mvM_NEUTRAL > 0f && IN.KA.mvM_NEUTRAL <= (float)alloc_frame;
		}

		public static bool isMagicNeutralO(int press = 0)
		{
			return IN.KA.mvM_NEUTRAL > (float)press;
		}

		public static bool isMagicLPD(int alloc_frame = 1)
		{
			return IN.KA.mvMLA > 0f && IN.KA.mvMLA <= (float)alloc_frame;
		}

		public static bool isMagicLO(int press = 0)
		{
			return IN.KA.mvMLA > (float)press;
		}

		public static bool isMagicTPD(int alloc_frame = 1)
		{
			return IN.KA.mvMTA > 0f && IN.KA.mvMTA <= (float)alloc_frame;
		}

		public static bool isMagicTO(int press = 0)
		{
			return IN.KA.mvMTA > (float)press;
		}

		public static bool isMagicRPD(int alloc_frame = 1)
		{
			return IN.KA.mvMRA > 0f && IN.KA.mvMRA <= (float)alloc_frame;
		}

		public static bool isMagicRO(int press = 0)
		{
			return IN.KA.mvMRA > (float)press;
		}

		public static bool isMagicBPD(int alloc_frame = 1)
		{
			return IN.KA.mvMBA > 0f && IN.KA.mvMBA <= (float)alloc_frame;
		}

		public static bool isMagicBO(int press = 0)
		{
			return IN.KA.mvMBA > (float)press;
		}

		public static bool isMousePushDown(int alloc_pd = 1)
		{
			return X.BTWW(1f, IN.mvMouse, (float)alloc_pd);
		}

		public static bool isMouseOn()
		{
			return IN.mvMouse >= 1f;
		}

		public static bool isMouseUp()
		{
			return IN.mvMouse < 0f;
		}

		public static bool isMouseUp(int pushing_time)
		{
			return IN.mvMouse < 0f && IN.mvMouse >= (float)(-(float)pushing_time);
		}

		public static bool ketteiM3()
		{
			return IN.kettei3() || IN.isMousePushDown(1);
		}

		public static bool getKD(Key name, int modifiers = -1)
		{
			Keyboard current = Keyboard.current;
			return current != null && !LabeledInputField.focus_exist && current[name].wasPressedThisFrame && (modifiers == -1 || ((modifiers & 4) > 0 == current.leftCtrlKey.isPressed && (modifiers & 1) > 0 == current.leftShiftKey.isPressed && (modifiers & 2) > 0 == current.leftAltKey.isPressed && (modifiers & 8) > 0 == current.leftCommandKey.isPressed));
		}

		public static bool getK(Key name, int modifiers = -1)
		{
			Keyboard current = Keyboard.current;
			return current != null && !LabeledInputField.focus_exist && current[name].isPressed && (modifiers == -1 || ((modifiers & 4) > 0 == current.leftCtrlKey.isPressed && (modifiers & 1) > 0 == current.leftShiftKey.isPressed && (modifiers & 2) > 0 == current.leftAltKey.isPressed && (modifiers & 8) > 0 == current.leftCommandKey.isPressed));
		}

		public static bool getKU(Key name, int modifiers = -1)
		{
			Keyboard current = Keyboard.current;
			return current != null && !LabeledInputField.focus_exist && current[name].wasReleasedThisFrame && (modifiers == -1 || ((modifiers & 4) > 0 == current.leftCtrlKey.isPressed && (modifiers & 1) > 0 == current.leftShiftKey.isPressed && (modifiers & 2) > 0 == current.leftAltKey.isPressed && (modifiers & 8) > 0 == current.leftCommandKey.isPressed));
		}

		public static bool getModif(int modifiers = -1)
		{
			return modifiers == -1 || ((modifiers & 4) > 0 == IN.getK(Key.LeftCtrl, -1) && (modifiers & 1) > 0 == IN.getK(Key.LeftShift, -1) && (modifiers & 2) > 0 == IN.getK(Key.LeftAlt, -1) && (modifiers & 8) > 0 == IN.getK(Key.LeftMeta, -1));
		}

		public static int getInputCount()
		{
			return IN.KA.getInputCount();
		}

		public static string getInputName(int i)
		{
			return IN.KA.getInputName(i);
		}

		public static PlayerInput ClonePlayerInput(GameObject Gob, bool enabled = false, bool actions_clone = true)
		{
			PlayerInput orAdd = IN.GetOrAdd<PlayerInput>(Gob);
			PlayerInput playerCon = IN.KA.PlayerCon;
			orAdd.actions = (actions_clone ? global::UnityEngine.Object.Instantiate<InputActionAsset>(playerCon.actions) : playerCon.actions);
			orAdd.defaultControlScheme = playerCon.defaultControlScheme;
			orAdd.neverAutoSwitchControlSchemes = playerCon.neverAutoSwitchControlSchemes;
			orAdd.camera = playerCon.camera;
			orAdd.notificationBehavior = playerCon.notificationBehavior;
			orAdd.enabled = enabled;
			return orAdd;
		}

		public static PxlImage getKeyAssignIcon(string tx_key, ref float left_margin, ref float right_margin, int pad_mode_auto = -1)
		{
			int num = IN.KA.getIconNumForText(tx_key, ref left_margin, ref right_margin, pad_mode_auto);
			if (num == 0)
			{
				return null;
			}
			if (!X.BTW(0f, (float)(--num), (float)MTRX.SqFImgKCIcon.countLayers()))
			{
				return null;
			}
			return MTRX.SqFImgKCIcon.getLayer(num).Img;
		}

		public static PxlImage getKeyAssignIconSmall(string tx_key, int pad_mode_auto = -1)
		{
			int num = IN.KA.getIconNumForText(tx_key, pad_mode_auto);
			if (num == 0)
			{
				return null;
			}
			if (!X.BTW(0f, (float)(--num), (float)MTRX.SqFImgKCIconS.countLayers()))
			{
				return null;
			}
			return MTRX.SqFImgKCIconS.getLayer(num).Img;
		}

		public static bool canDrawIconByShape(string tx_key, int pad_mode_auto = -1)
		{
			return IN.KA.getIconNumForText(tx_key, pad_mode_auto) == 0;
		}

		public static string getKeyAssignLabel(string tx_key, bool replacing_alphabet = true, int pad_mode_auto = -1)
		{
			string labelForText = IN.KA.getLabelForText(tx_key, pad_mode_auto);
			if (!replacing_alphabet)
			{
				return labelForText;
			}
			if (labelForText.Length != 0)
			{
				return TX.replaceAlphabet(labelForText, false);
			}
			return " ";
		}

		public static int getKeyAssignIconId(string tx_key, int pad_mode_auto = -1)
		{
			return IN.KA.getIconNumForText(tx_key, pad_mode_auto);
		}

		public static bool kettei()
		{
			if (IN.isSubmit())
			{
				IN.use_mouse = false;
				return true;
			}
			return IN.isMousePushDown(1);
		}

		public static bool ketteiPD(int alloc_pd = 1)
		{
			if (IN.isSubmitPD(alloc_pd))
			{
				IN.use_mouse = false;
				return true;
			}
			return IN.isMousePushDown(alloc_pd);
		}

		public static bool ketteiOn()
		{
			if (IN.isSubmitOn(0))
			{
				IN.use_mouse = false;
				return true;
			}
			return IN.isMouseOn();
		}

		public static bool kettei3()
		{
			return IN.kettei() || IN.isCancel();
		}

		public static bool kettei3On()
		{
			return IN.ketteiOn() || IN.isCancelOn(0);
		}

		public static void OnMouseWheelChanged(InputAction.CallbackContext _context)
		{
			IN.MouseWheel = _context.ReadValue<Vector2>();
		}

		public void OnMouse(InputValue value)
		{
		}

		public static Vector2 getMouseScreenPos()
		{
			return IN.Mouse;
		}

		public static Vector2 getMousePos(Camera BaseCamera = null)
		{
			if (BaseCamera == null)
			{
				BaseCamera = IN.getGUICamera();
			}
			if (BaseCamera == null)
			{
				BaseCamera = Camera.main;
			}
			return BaseCamera.ScreenToWorldPoint(IN.Mouse);
		}

		public static KEY getCurrentKeyAssignObject()
		{
			return IN.KA;
		}

		public static void addRunner(IRunAndDestroy Runner)
		{
			if (IN.ARunner == null)
			{
				IN.ARunner = new List<IRunAndDestroy>(4);
			}
			IN.ARunner.Add(Runner);
		}

		public static void remRunner(IRunAndDestroy Runner)
		{
			if (IN.ARunner == null)
			{
				return;
			}
			int num = IN.ARunner.IndexOf(Runner);
			if (num < 0)
			{
				return;
			}
			if (IN.runner_running_i >= 0 && IN.runner_running_i >= num)
			{
				IN.runner_running_i--;
			}
			IN.ARunner.RemoveAt(num);
		}

		public static void AssignPauseable(MonoBehaviour Mb)
		{
			IN.Pauser.Assign(Mb);
		}

		public static void AssignPauseable(IPauseable Ps)
		{
			IN.Pauser.Assign(Ps);
		}

		public static void AssignPauseable(Rigidbody2D R2D)
		{
			IN.Pauser.Assign(R2D);
		}

		public static void DeassignPauseable(global::UnityEngine.Object O)
		{
			IN.Pauser.Deassign(O);
		}

		public static void ClearPauseable()
		{
			IN.Pauser.Clear();
		}

		public static bool isPlayer()
		{
			return X.DEBUG_PLAYER || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.LinuxPlayer;
		}

		public static bool quitGame()
		{
			Application.Quit();
			return false;
		}

		public static void PauseMem()
		{
			IN.Pauser.Pause();
		}

		public static void ResumeMem()
		{
			IN.Pauser.Resume();
		}

		public static void Throw(string msg)
		{
			throw new Exception(msg);
		}

		public static KEY cloneKeyAssign(PlayerInput PC)
		{
			return new KEY(PC, IN.KA);
		}

		public static void submitKeyAssign(KEY _KA, bool countup_id = false)
		{
			IN.KA.copyFrom(_KA);
			if (countup_id)
			{
				IN.KA.countupId();
			}
		}

		public static void submitKeyAssign(ByteArray Ba, bool apply_pad_mode = true)
		{
			IN.KA.readSaveString(Ba, apply_pad_mode);
		}

		public static void holdArrowInput()
		{
			IN.KA.holdArrowInput();
		}

		public static int getKeyAssignResetId()
		{
			return IN.KA.text_id_for_tx_renderer;
		}

		public static bool isPadMode()
		{
			return IN.KA.pad_mode;
		}

		public readonly int pixelsPerUnit = 64;

		public static bool init_key_and_text_when_awake = false;

		public static IN _stage;

		public static readonly float w = 1280f;

		public static readonly float h = 720f;

		public static float wh;

		public static float hh;

		private static bool enable_vsync_;

		public static int screen_width = (int)IN.w;

		public static int screen_height = (int)IN.h;

		public static float pixel_scale = 1f;

		public const float ppu = 64f;

		public const float ppur = 0.015625f;

		public static Transform Trs;

		public static int totalframe = 0;

		public static CLICK Click;

		public static Vector2 Mouse;

		public static Vector2 MouseWheel;

		public static bool use_mouse_;

		public static bool mouse_area_out;

		public static string connected_hid_device;

		private static KEY KA;

		private static float mvMouse = 0f;

		public static string gui_layer_name = "GUI";

		public static int gui_layer;

		public const float delta2frame = 60f;

		public const double delta2frameR = 0.016666666666666666;

		public const float delta2frameRF = 0.016666668f;

		private static float deltaFrameR_ = 1f;

		public const int ignoreray_layer = 2;

		private static Camera GuiCamera;

		private static PerfectPixelCamera GuiPxCamera;

		public static Flagger FlgUiUse;

		private static MeshDrawer MdBounds;

		private static ValotileRenderer ValotBounds;

		private static PAUSER Pauser;

		private static IN.PB pd_bits = (IN.PB)0U;

		public static bool save_prefs;

		public static bool application_focus = true;

		public const float VSYNC_TIME_RATIO = 0.96f;

		public const int STENCIL_PRMP = 10;

		public const int STENCIL_GAMEUI = 225;

		public const int STENCIL_GAMEUI_CONTENT = 239;

		public const int STENCIL_GAMEUI_DESC = 230;

		public const int STENCIL_EV = 70;

		public const int STENCIL_MSG = 58;

		public const int STENCIL_EV_DEBUG_0 = 240;

		public const int STENCIL_EV_DEBUG_1 = 241;

		public const int STENCIL_M2DE_0 = 250;

		public const int STENCIL_M2DE_1 = 251;

		public const int STENCIL_M2DE_2 = 252;

		public const int STENCIL_UIBOX = 200;

		public const int STENCIL_UIBOX_O = 180;

		public const int STENCIL_MP_EGG0 = 12;

		public const int STENCIL_MP_EGG1 = 13;

		public const int STENCIL_KEYCON = 11;

		public const int STENCIL_TEXTAREA = 4;

		public const int STENCIL_CUTIN = 20;

		public const int STENCIL_M2D_WATER = 30;

		public const int CURSOR_WAIT_FIRST = 18;

		public const int CURSOR_WAIT = 6;

		private static List<GameObject> ACameraGob;

		public const string dll_name = ",unsafeAssem";

		public static uint MainDarkColor = 4278190080U;

		private static int runner_running_i = -1;

		public static IN.FnWindowSizeChanged FD_WindowSizeChanged;

		private static List<IRunAndDestroy> ARunner;

		private static float press_mvL;

		private static float press_mvR;

		private static float press_mvT;

		private static float press_mvB;

		private static float press_mvSubmit;

		private static float press_mvCancel;

		public delegate void FnWindowSizeChanged(int w, int h);

		private enum PB : uint
		{
			L = 1U,
			LR = 5U,
			T = 2U,
			R = 4U,
			B = 8U,
			SUBMIT = 256U,
			CANCEL = 512U,
			_ALL = 4095U,
			_LOCK_UI_KEY
		}
	}
}
