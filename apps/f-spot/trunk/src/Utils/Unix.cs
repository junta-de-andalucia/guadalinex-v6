using System;
using System.Runtime.InteropServices;

namespace FSpot.Utils {
	public static class Unix {
		static class NativeMethods
		{
			[DllImport ("libc", EntryPoint="rename", CharSet = CharSet.Auto)]
			public static extern int Rename (string oldpath, string newpath);

			[DllImport ("libc", EntryPoint="mkstemp")]
			public static extern int MkSTemp (byte []template);

			[DllImport("libc", EntryPoint="prctl")]
			public static extern int PrCtl(int option, string name, ulong arg3, ulong arg4, ulong arg5);

			[DllImport("libc", EntryPoint="utime")]
			public static extern int UTime (string filename, IntPtr buf);
		}

		public static int Rename (string oldpath, string newpath)
		{
			return NativeMethods.Rename (oldpath, newpath);
		}


		public static Mono.Unix.UnixStream MakeSafeTemp (ref string template)
		{
			byte [] template_bytes = System.Text.Encoding.UTF8.GetBytes (template + ".XXXXXX\0");

			int fd = NativeMethods.MkSTemp (template_bytes);

			if (fd < 0) {
				//Mono.Unix.Error error = Mono.Unix.Stdlib.GetLastError ();
				throw new System.ApplicationException (Mono.Unix.Catalog.GetString ("Unable to create temporary file"));
			}

			template = System.Text.Encoding.UTF8.GetString (template_bytes, 0, template_bytes.Length - 1);
			return new Mono.Unix.UnixStream (fd);
		}

		public static void SetProcessName (string name)
		{
			try {
				if (NativeMethods.PrCtl(15 /* PR_SET_NAME */, name, 0, 0, 0) != 0)
					Log.Warning ("Error setting process name: " + Mono.Unix.Native.Stdlib.GetLastError());
			} catch (DllNotFoundException) {
				/* noop */
			} catch (EntryPointNotFoundException) {
		    		/* noop */
			}
		}

		public static void Touch (string filename)
		{
			if (NativeMethods.UTime (filename, IntPtr.Zero) != 0)
				Log.DebugFormat ("touch on {0} failed", filename);
		}
	}
}
