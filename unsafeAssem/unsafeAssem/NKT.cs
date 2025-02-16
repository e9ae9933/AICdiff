using System;
using System.Diagnostics;
using System.IO;
using PixelLiner.PixelLinerLib;
using UnityEngine;
using XX;

public static class NKT
{
	public static bool saveText(string path, string text)
	{
		try
		{
			using (StreamWriter streamWriter = new StreamWriter(Path.Combine(Application.dataPath, path), false))
			{
				streamWriter.Write(text);
				streamWriter.Flush();
				streamWriter.Close();
			}
		}
		catch (Exception ex)
		{
			X.dl(ex.Message, null, false, false);
			return false;
		}
		return true;
	}

	public static string appDirectory
	{
		get
		{
			return AppDomain.CurrentDomain.BaseDirectory;
		}
	}

	private static void prepareSdDirectory()
	{
		NKT.prepareDirectory(Application.persistentDataPath);
	}

	public static void prepareDirectory(string path)
	{
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}

	public static string readStreamingText(string path, bool no_error = false)
	{
		string text = "";
		try
		{
			using (StreamReader streamReader = new StreamReader(Path.Combine(Application.streamingAssetsPath, path)))
			{
				text = streamReader.ReadToEnd();
				streamReader.Close();
			}
		}
		catch (Exception ex)
		{
			if (!no_error)
			{
				X.dl(ex.Message, null, true, false);
			}
		}
		return text;
	}

	public static string readSpecificStreamingText(string path, bool no_error = false)
	{
		string text = "";
		try
		{
			if (path.IndexOf("..") >= 0)
			{
				throw new Exception("invalid path string");
			}
			using (StreamReader streamReader = new StreamReader(path))
			{
				text = streamReader.ReadToEnd();
				streamReader.Close();
			}
		}
		catch (Exception ex)
		{
			if (!no_error)
			{
				X.dl(ex.Message, null, true, false);
			}
		}
		return text;
	}

	private static byte[] readSpecificFileBinary(string path, bool no_error = false)
	{
		return NKT.readSpecificFileBinary(path, 0, 0, no_error);
	}

	public static byte[] readSpecificFileBinary(string path, int start_pos = 0, int read_len = 0, bool no_error = false)
	{
		FileStream fileStream = null;
		byte[] array;
		try
		{
			if (path.IndexOf("..") >= 0)
			{
				throw new Exception("invalid path string");
			}
			fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			long num = fileStream.Length - (long)start_pos;
			if (read_len > 0)
			{
				num = X.Mn(num, (long)read_len);
			}
			array = new byte[num];
			int num2 = 0;
			if (start_pos > 0)
			{
				fileStream.Position = (long)start_pos;
			}
			while (num > 0L)
			{
				int num3 = fileStream.Read(array, num2, (int)X.Mn(1024L, num));
				num2 += num3;
				num -= (long)num3;
			}
			fileStream.Dispose();
		}
		catch (Exception ex)
		{
			array = null;
			if (fileStream != null)
			{
				fileStream.Dispose();
			}
			if (!no_error)
			{
				X.dl(ex.Message, null, true, false);
			}
		}
		return array;
	}

	public static string writeSdBinary(string path, ByteArray Ba, bool no_error = false)
	{
		return NKT.writeSpecificFileBinary(Path.Combine(Application.persistentDataPath, path), Ba, no_error);
	}

	public static string writeSpecificFileBinary(string path, ByteArray Ba, bool no_error = false)
	{
		FileStream fileStream = null;
		BinaryWriter binaryWriter = null;
		string text = null;
		try
		{
			NKT.prepareSdDirectory();
			fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
			binaryWriter = Ba.writeToFileStream(fileStream, null);
			binaryWriter.Close();
			fileStream.Dispose();
		}
		catch (Exception ex)
		{
			if (binaryWriter != null)
			{
				binaryWriter.Close();
			}
			if (fileStream != null)
			{
				fileStream.Dispose();
			}
			text = ex.Message;
			if (!no_error)
			{
				X.dl(ex.Message, null, true, false);
			}
		}
		return text;
	}

	public static void writeSpecificFileBinary(string path, STB Std, bool no_error = false)
	{
		FileStream fileStream = null;
		BinaryWriter binaryWriter = null;
		try
		{
			NKT.prepareSdDirectory();
			fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
			binaryWriter = new BinaryWriter(fileStream);
			int length = Std.Length;
			for (int i = 0; i < length; i++)
			{
				binaryWriter.Write(Std[i]);
			}
			binaryWriter.Close();
			fileStream.Dispose();
		}
		catch (Exception ex)
		{
			if (binaryWriter != null)
			{
				binaryWriter.Close();
			}
			if (fileStream != null)
			{
				fileStream.Dispose();
			}
			if (!no_error)
			{
				X.dl(ex.Message, null, true, false);
			}
		}
	}

	public static ByteArray readSdBinary(string path, bool no_error = false)
	{
		byte[] array = NKT.readSpecificFileBinary(Path.Combine(Application.persistentDataPath, path), no_error);
		if (array == null)
		{
			return null;
		}
		return new ByteArray(array, false, false);
	}

	public static ByteReaderFS PopSdFileStream(string path, bool no_error = false)
	{
		Path.Combine(Application.persistentDataPath, path);
		return new ByteReaderFS(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), 0L, null, -1L, 1024, null);
	}

	public static ByteReaderFS PopSpecificFileStream(string path, int start_pos = 0, int read_len = 0, bool no_error = false, int buffer_one_len = 1024, byte[] Abuffer_prepared = null)
	{
		return new ByteReaderFS(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), (long)start_pos, null, (read_len <= 0) ? (-1L) : ((long)read_len), buffer_one_len, Abuffer_prepared);
	}

	public static void openInExplorer(string url)
	{
		try
		{
			Process.Start("Explorer.exe", "/select, " + url.Replace('/', '\\'));
		}
		catch
		{
		}
	}
}
