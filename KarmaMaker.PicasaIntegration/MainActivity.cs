using System;

using Android.App;
using Android.Content;
using Android.Provider;
using Java.IO;
using Java.Lang;
using Android.Graphics;
using Android.OS;

namespace KarmaMaker.PicasaIntegration
{
	[Activity (Label = "KarmaMaker.PicasaIntegration", MainLauncher = true)]
	public class MainActivity : Activity
	{
		private const int PickImageRequestCode = 0x0100;

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

				using(var inputStream = ContentResolver.OpenInputStream(selectedImgUri))
				{
					MainView.SetImageBitmap(BitmapFactory.DecodeStream(inputStream));
				}

				Log.AddMsg("Memory usage: {0}MB from {1}MB", Runtime.GetRuntime().TotalMemory() / (1024 * 1024), Runtime.GetRuntime().MaxMemory() / (1024 * 1024));
			}
			else
			{
				Log.AddMsg("RequestCode == {0} | ResultCode == {1} | Data == {2}", requestCode, resultCode, data);
			}
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

		private bool IsImageFromPicasa(Android.Net.Uri imageUri)
		{
			// This is quite stupid, I know.
			return imageUri.ToString().Contains("//com.google.android.gallery3d.provider/picasa/item/");
		}
	}


}
