using System;

using Android.App;
using Android.Content;
using Android.Provider;
using Java.IO;
using Java.Lang;
using Android.Graphics;
using Android.OS;
using System.Threading.Tasks;

namespace KarmaMaker.PicasaIntegration
{
	[Activity (Label = "KarmaMaker.PicasaIntegration", MainLauncher = true)]
	public class MainActivity : Activity
	{
		private const int PickImageRequestCode = 0x0100;
		private const string BufferFilePath = "ImageBuffer.jpg";

		private MainView MainView { get; set; }
		private KarmaLog Log { get; set; }

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			Log = new KarmaLog(this);
			MainView = new MainView(this);
			MainView.LoadFromGallery += OnLoadFromGallery;
			MainView.SendLogViaEmail += OnSendLogViaEmail;

			SetContentView(MainView);
		}

		private void OnLoadFromGallery()
		{
			StartPickIntent();
		}

		private void StartPickIntent()
		{
			Intent intent = new Intent(Intent.ActionPick, MediaStore.Images.Media.ExternalContentUri);
			StartActivityForResult(intent, PickImageRequestCode);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if(requestCode == PickImageRequestCode && resultCode == Result.Ok && data != null)
			{
				Android.Net.Uri selectedImgUri = data.Data;
				Log.AddMsg("SelectedImageUri == {0}", selectedImgUri);

				var getFilePathTask = new Task<string>(() => GetFilePath(selectedImgUri));
				getFilePathTask.ContinueWith((Task<string> getFilePath) => RunOnUiThread(() => MainView.SetImageBitmap(BitmapFactory.DecodeFile(getFilePath.Result))));

				getFilePathTask.Start();

				Log.AddMsg("Memory usage: {0}MB from {1}MB", Runtime.GetRuntime().TotalMemory() / (1024 * 1024), Runtime.GetRuntime().MaxMemory() / (1024 * 1024));
			}
			else
			{
				Log.AddMsg("RequestCode == {0} | ResultCode == {1} | Data == {2}", requestCode, resultCode, data);
			}
		}

		private string GetFilePath(Android.Net.Uri imgUri)
		{
			// If image is from Picasa
			if(imgUri.ToString().Contains("//com.google.android.gallery3d.provider/picasa/item/"))
			{
				return GetPicasaFilePath(imgUri);
			}
			else
			{
				return GetUsualFilePath(imgUri);
			}
		}

		private string GetUsualFilePath(Android.Net.Uri imgUri)
		{
			Log.AddMsg("Load from local storage");
			
			string[] filePathColumn = { MediaStore.MediaColumns.Data };
			var cursor = ContentResolver.Query (imgUri, filePathColumn, null, null, null);
			if (cursor == null) Log.AddMsg ("Panic: cursor is null");
			
			cursor.MoveToFirst();
			int columnIndex = cursor.GetColumnIndex(MediaStore.MediaColumns.Data);
			if (columnIndex != 0) Log.AddMsg ("ColumnIndex == {0}", columnIndex);
			
			string picturePath = cursor.GetString(columnIndex);
			cursor.Close();
			Log.AddMsg("PicturePath == {0}", picturePath);

			return picturePath;
		}

		private string GetPicasaFilePath(Android.Net.Uri imgUri)
		{
			Log.AddMsg("Load from picasa");

			var picturePath = CacheDir + "/" + BufferFilePath;
			var imgIStream = ContentResolver.OpenInputStream(imgUri);
			var tempFileOStream = System.IO.File.OpenWrite(picturePath);
			
			imgIStream.CopyTo(tempFileOStream);

			tempFileOStream.Close();
			imgIStream.Close();
			return picturePath;
		}

		private void OnSendLogViaEmail ()
		{
			var sendEmail = new Intent(Intent.ActionSendto);
			sendEmail.SetType("text");
			sendEmail.PutExtra(Intent.ExtraSubject, "Log");
			sendEmail.SetData(Android.Net.Uri.Parse("mailto: vivalatecnocracia@gmail.com"));
			sendEmail.PutExtra(Intent.ExtraText, Log.GetLogCache ());

			if(0 < PackageManager.QueryIntentActivities(sendEmail, 0).Count) 
			{
				StartActivity(sendEmail);
				Log.ClearLogCache ();
			}
			else
			{
				Log.AddMsg("Your mail client has not been setup properly");
			}
		}
	}


}
