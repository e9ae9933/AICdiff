using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace XX
{
	public class Logger : MonoBehaviour
	{
		public static void InitLogger()
		{
			if (Logger.StbLast == null)
			{
				string text = Path.Combine(Application.persistentDataPath, "Logs");
				NKT.prepareDirectory(text);
				Application.logMessageReceived += Logger.LogCalled;
				Logger.StbLast = new STB(128);
				Logger.save_path = PlayerPrefs.GetString("LoggerName", null);
				if (TX.valid(Logger.save_path))
				{
					Logger.LogPath = Path.Combine(text, Logger.save_path);
					byte[] array = NKT.readSpecificFileBinary(Logger.LogPath, 0, "error detected: ".Length + 6 + 1, true);
					try
					{
						if (array != null && array.Length >= "error detected: ".Length + 6 + 1)
						{
							int num = 0;
							for (int i = 0; i < 6; i++)
							{
								char c = (char)array["error detected: ".Length + i];
								if (c != ' ')
								{
									if ('0' > c || c > '9')
									{
										break;
									}
									num = num * 10 + (int)(c - '0');
								}
							}
							if (num > 0)
							{
								Logger.error_occur_path = Logger.LogPath.Replace("\\", "/");
								for (int j = 0; j < 1; j++)
								{
									if (array["error detected: ".Length + 6 + j] == 109)
									{
										Logger.loaded_file_error_flag |= Logger.ERRORFLAG.MISSING_METHOD;
									}
								}
							}
							else
							{
								File.Delete(Logger.LogPath);
							}
						}
					}
					catch
					{
					}
				}
				Logger.save_path = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
				Logger.save_path += ".txt";
				Logger.LogPath = Path.Combine(text, Logger.save_path);
				PlayerPrefs.SetString("LoggerName", Logger.save_path);
				IN.save_prefs = true;
				Logger.StbLast += "error detected: ";
				for (int k = 5; k >= 0; k--)
				{
					Logger.StbLast += '0';
				}
				for (int l = 0; l >= 0; l--)
				{
					Logger.StbLast += ' ';
				}
				Logger.StbLast += '\n';
				Logger.StbLast += "Version: ";
				Logger.StbLast += Application.version;
				Logger.StbLast += " @";
				Logger.StbLast += "Win";
				Logger.StbLast += '\n';
				OperatingSystem osversion = Environment.OSVersion;
				Logger.StbLast += osversion.ToString();
				Logger.StbLast += '\n';
				Logger.StbLast += "// ======================";
				Logger.FileS = new FileStream(Logger.LogPath, FileMode.Create, FileAccess.Write, FileShare.Read);
				Logger.Append(Logger.StbLast, true);
				Logger.StbLast.Clear();
				Logger.last_hr = true;
			}
		}

		public static void close(bool quit_app = true)
		{
			if (Logger.FileS != null)
			{
				Logger.FileS.Dispose();
				Logger.FileS = null;
			}
			if (quit_app)
			{
				Debug.unityLogger.logHandler = new Logger.DefaultLogOverride();
			}
		}

		public static void LogCalled(string condition, string stackTrace, LogType type)
		{
			if (Logger.FileS == null)
			{
				return;
			}
			if (type == LogType.Error || type == LogType.Exception)
			{
				if (!Logger.error_output && !Logger.scene_changing && TX.valid(condition) && !TX.isStart(condition, "<RI.Hid>", 0))
				{
					Logger.error_output = true;
					Logger.de(condition, false, stackTrace);
					Logger.error_output = false;
					return;
				}
			}
			else if (type == LogType.Log && !Logger.scene_changing)
			{
				Logger.dl(condition);
			}
		}

		private static void Append(STB Stb, bool after_return = true)
		{
			if (Logger.FileS == null)
			{
				return;
			}
			try
			{
				int length = Stb.Length;
				for (int i = 0; i < length; i++)
				{
					Logger.FileS.WriteByte((byte)Stb[i]);
				}
				if (after_return)
				{
					Logger.FileS.WriteByte(10);
				}
				Logger.FileS.Flush();
			}
			catch
			{
			}
		}

		private static void Append(string s, int start = 0, int e = -1)
		{
			if (Logger.FileS == null)
			{
				return;
			}
			try
			{
				if (e < 0)
				{
					e = s.Length;
				}
				for (int i = start; i < e; i++)
				{
					Logger.FileS.WriteByte((byte)s[i]);
				}
				Logger.FileS.WriteByte(10);
				Logger.FileS.Flush();
			}
			catch
			{
			}
		}

		public static void dl(string s)
		{
			if (Logger.FileS == null)
			{
				return;
			}
			Logger.last_hr = false;
			Logger.StbLast.Clear();
			if (X.DEBUGTIMESTAMP)
			{
				Logger.StbLast.AddDetailed(DateTime.Now, true);
				Logger.StbLast.Add(" - ");
			}
			Logger.StbLast.AddEncode(s, "utf-8");
			Logger.Append(Logger.StbLast, true);
			Logger.StbLast.Clear();
		}

		public static void dl(STB Stb)
		{
			if (Logger.FileS == null)
			{
				return;
			}
			Logger.last_hr = false;
			Logger.StbLast.Clear();
			if (X.DEBUGTIMESTAMP)
			{
				Logger.StbLast.AddDetailed(DateTime.Now, true);
				Logger.StbLast.Add(" - ");
			}
			Logger.StbLast.AddEncode(Stb, "utf-8");
			Logger.Append(Logger.StbLast, true);
			Logger.StbLast.Clear();
		}

		public static void de(string s, bool convert_utf8 = false, string stack_trace = null)
		{
			if (Logger.FileS == null)
			{
				return;
			}
			using (STB stb = TX.PopBld(null, 0))
			{
				if (convert_utf8)
				{
					stb.AddEncode(s, "utf-8");
				}
				else
				{
					stb.Add(s);
				}
				if (stack_trace != null)
				{
					stb.Add('\n').Add(stack_trace);
				}
				try
				{
					if (!Logger.last_hr)
					{
						Logger.Append("// ======================", 0, -1);
					}
					if (!stb.Equals(Logger.StbLast))
					{
						Logger.StbLast.Set(stb);
						if ((Logger.current_error_flag & Logger.ERRORFLAG.MISSING_METHOD) == Logger.ERRORFLAG.NONE && stb.IndexOf("MissingMethodException", 0, -1) >= 0)
						{
							Logger.current_error_flag |= Logger.ERRORFLAG.MISSING_METHOD;
							Logger.FileS.Seek((long)("error detected: ".Length + 6), SeekOrigin.Begin);
							Logger.FileS.WriteByte(109);
						}
						Logger.FileS.Seek(0L, SeekOrigin.End);
						Logger.Append(stb, true);
						stb.Clear();
						Logger.Append(Bench.CopyCurrentClosure(stb, "Error Closure: "), true);
						Logger.Append("// ======================", 0, -1);
						Logger.last_hr = true;
					}
					Logger.FileS.Seek((long)"error detected: ".Length, SeekOrigin.Begin);
					stb.Clear();
					if (Logger.error_total < 999999)
					{
						Logger.error_total++;
					}
					stb.spr0(Logger.error_total, 6, '0');
					Logger.Append(stb, false);
					Logger.FileS.Seek(0L, SeekOrigin.End);
				}
				catch
				{
				}
			}
			Logger.view_fps_t = -30f;
		}

		public static void BenchClosure()
		{
			STB stb = TX.PopBld(null, 0);
			Logger.Append(Bench.CopyCurrentClosure(stb, ""), true);
			TX.ReleaseBld(stb);
		}

		public static void fineTimeStampViewer()
		{
			if (X.DEBUGTIMESTAMP)
			{
				if (Logger.FpsInstance != null)
				{
					IN.DestroyOne(Logger.FpsInstance.gameObject);
				}
				Logger.FpsInstance = IN.CreateGobGUI(null, "Logger-fps").AddComponent<Logger>();
			}
		}

		public static void finePos(bool fix_to_screen_pos)
		{
			if (Logger.FpsInstance != null)
			{
				if (fix_to_screen_pos)
				{
					IN.PosP(Logger.FpsInstance.transform, (float)(-(float)IN.screen_width) * 0.5f, (float)(-(float)IN.screen_height) * 0.5f + 12f, -9.499f);
					return;
				}
				IN.PosP(Logger.FpsInstance.transform, -IN.wh, -IN.hh + 12f, -9.499f);
			}
		}

		public void Update()
		{
			if (this.InnerStb == null)
			{
				if (!MTRX.prepared)
				{
					return;
				}
				Logger.view_fps_t = 0f;
				this.pre_totalframe = IN.totalframe;
				this.PreTDate = DateTime.Now;
				Object.DontDestroyOnLoad(this);
				this.InnerStb = new STB();
				this.Md = MeshDrawer.prepareMeshRenderer(base.gameObject, MTRX.MIicon.getMtr(BLEND.NORMAL, -1), 0f, -1, null, true, true);
				this.Valot = base.gameObject.GetComponent<ValotileRenderer>();
				this.Valot.InitUI(this.Md, base.GetComponent<MeshRenderer>());
				Logger.finePos(IN.screen_ortho_flag);
			}
			Color32 color = MTRX.ColWhite;
			this.execute_count += 1;
			float deltaFrame = IN.deltaFrame;
			if (Logger.view_fps_t >= 0f)
			{
				Logger.view_fps_t -= deltaFrame;
				if (Logger.view_fps_t > 0f)
				{
					return;
				}
				Logger.view_fps_t = 30f;
			}
			else
			{
				Logger.view_fps_t += deltaFrame;
				if (Logger.view_fps_t >= 0f)
				{
					Logger.view_fps_t = 30f;
				}
				else
				{
					color = this.Md.ColGrd.White().blend(16776960U, X.ZLINE(-Logger.view_fps_t, 60f)).C;
				}
			}
			this.Md.clear(false, false);
			DateTime now = DateTime.Now;
			if ((now - this.PreTDate).TotalMilliseconds >= 500.0)
			{
				if (this.pre_totalframe > 0)
				{
					this.fps = (int)(this.execute_count * 2);
				}
				this.PreTDate = now;
				this.pre_totalframe = IN.totalframe;
				this.execute_count = 0;
			}
			this.InnerStb.Clear();
			this.InnerStb.AddDetailed(DateTime.Now, false);
			this.Md.Col = color;
			if (this.fps >= 0)
			{
				this.InnerStb.Add(" f");
				this.InnerStb.Add(this.fps);
			}
			IN.LoggerStbAdd(this.InnerStb);
			MTRX.ChrL.DrawStringTo(this.Md, this.InnerStb, 0f, 0f, ALIGN.LEFT, ALIGNY.TOP, false, 0f, 0f, null);
			this.Md.updateForMeshRenderer(false);
		}

		public static bool scene_changing;

		public static STB StbLast;

		public static string save_path;

		public static int error_total;

		public const string header_error = "error detected: ";

		public const string hr = "// ======================";

		private const int error_total_char_max = 6;

		private const int error_flag_char_max = 1;

		private const int error_total_max = 999999;

		public static string error_occur_path;

		public static Logger.ERRORFLAG loaded_file_error_flag;

		public static Logger.ERRORFLAG current_error_flag;

		private static string LogPath;

		private static bool last_hr;

		private static FileStream FileS;

		private const string extension = ".txt";

		private static bool error_output;

		private const char ERRORFLAG_MISSING_METHOD = 'm';

		private static Logger FpsInstance;

		private MeshDrawer Md;

		private ValotileRenderer Valot;

		private static float view_fps_t;

		private short execute_count;

		private STB InnerStb;

		private int pre_totalframe;

		private DateTime PreTDate;

		private int fps = -1;

		public enum ERRORFLAG : byte
		{
			NONE,
			MISSING_METHOD
		}

		private class DefaultLogOverride : ILogHandler
		{
			public void LogException(Exception exception, Object context)
			{
			}

			public void LogFormat(LogType logType, Object context, string format, params object[] args)
			{
			}
		}
	}
}
