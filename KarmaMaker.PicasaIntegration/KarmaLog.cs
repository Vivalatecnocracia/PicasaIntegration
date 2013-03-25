using System;
using Android.App;
using System.Text;
using Android.Widget;
using Android.Util;

namespace KarmaMaker.PicasaIntegration
{
	public class KarmaLog
	{
		private const string Tag = "PicasaIntegration";

		private Activity Context { get; set; } 
		private StringBuilder LogCache { get; set; }

		public KarmaLog(Activity context)
		{
			Context = context;
			LogCache = new StringBuilder();
		}

		public void AddMsg(string msg, params object[] args)
		{
			msg = string.Format(msg, args);
			LogCache.AppendLine(msg); // Yeap, it can be huge
			Context.RunOnUiThread(() => Toast.MakeText(Context, msg, ToastLength.Short).Show());
			Log.Info(Tag, msg);
		}

		public string GetLogCache()
		{
			return LogCache.ToString();
		}

		public void ClearLogCache()
		{
			LogCache.Clear();
		}
	}
}

