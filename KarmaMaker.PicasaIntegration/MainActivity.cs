using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Provider;
using Android.Util;
using Java.IO;
using Java.Lang;

namespace KarmaMaker.PicasaIntegration
{
	[Activity (Label = "KarmaMaker.PicasaIntegration", MainLauncher = true)]
	public class MainActivity : Activity
	{
		private const int PickImageRequestCode = 0x0100;
		private const string Tag = "MainActivity";

		private System.Text.StringBuilder LogCache { get; set; }
		private MainView MainView { get; set; }
		private File ImageTempFile { get; set; }

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			LogCache = new System.Text.StringBuilder();
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
				AddToLog("SelectedImageUri == {0}", selectedImgUri);

				if(IsImageFromPicasa(selectedImgUri))
				{
					AddToLog("Load from Picasa");

					using(var inputStream = ContentResolver.OpenInputStream(selectedImgUri))
					{
						MainView.SetImageBitmap(BitmapFactory.DecodeStream(inputStream));
					} 
				}
				else
				{
					AddToLog("Load from local storage");

					string[] filePathColumn = { MediaStore.MediaColumns.Data };
					var cursor = ContentResolver.Query (selectedImgUri, filePathColumn, null, null, null);
					if (cursor == null) AddToLog ("Panic: cursor is null");

					cursor.MoveToFirst();
					int columnIndex = cursor.GetColumnIndex(MediaStore.MediaColumns.Data);
					if (columnIndex != 0) AddToLog("ColumnIndex == {0}", columnIndex);

					string picturePath = cursor.GetString(columnIndex);
					cursor.Close();
					AddToLog("PicturePath == {0}", picturePath);

					MainView.SetImageBitmap(BitmapFactory.DecodeFile(picturePath));
				}
				AddToLog("Memory usage: {0}MB from {1}MB", Runtime.GetRuntime().TotalMemory() / (1024 * 1024), Runtime.GetRuntime().MaxMemory() / (1024 * 1024));
			
			}
			else
			{
				AddToLog("RequestCode == {0} | ResultCode == {1} | Data == {2}", requestCode, resultCode, data);

			}
		}

		private void OnSendLogViaEmail ()
		{
			var sendEmail = new Intent(Intent.ActionSendto);
			sendEmail.SetType("text");
			sendEmail.PutExtra(Intent.ExtraSubject, "Log");
			sendEmail.SetData(Android.Net.Uri.Parse("mailto: vivalatecnocracia@gmail.com"));
			sendEmail.PutExtra(Intent.ExtraText, LogCache.ToString());

			if(0 < PackageManager.QueryIntentActivities(sendEmail, 0).Count) 
			{
				StartActivity(sendEmail);
				LogCache.Clear();
			}
			else
			{
				AddToLog("Your mail client has not been setup properly");
			}
		}

		private bool IsImageFromPicasa(Android.Net.Uri imageUri)
		{
			// This is quite stupid, I know.
			return imageUri.ToString().Contains("//com.google.android.gallery3d.provider/picasa/item/");
		}

		private void AddToLog(string msg, params object[] args)
		{
			msg = string.Format(msg, args);
			LogCache.Append(msg); // Yeap it can be huge
			RunOnUiThread(() => Toast.MakeText(this, msg, ToastLength.Short).Show());
			Log.Info(Tag, msg);
		}
	}

	class MainView : LinearLayout
	{
		public event Action LoadFromGallery;
		public event Action SendLogViaEmail;

		Bitmap ImageBitmap;
		private ImageView ImageView{ get; set; }

		public MainView(Context context) : base(context)
		{
			SetBackgroundColor(Color.Azure);

			SetGravity(GravityFlags.Center);
			Orientation = Orientation.Vertical;

			ImageView = new ImageView(context);
			{
				ImageView.SetBackgroundColor (Color.Bisque);
				ImageView.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, 0, 1);
			}
			AddView(ImageView);

			var loadFromGalleryBtn = new Button(context);
			{
				loadFromGalleryBtn.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent, 0);
				loadFromGalleryBtn.Text = "Load from Gallery";
				loadFromGalleryBtn.Click += (sender, e) => LoadFromGallery.Invoke();
			}
			AddView(loadFromGalleryBtn);

			var sendLogViaEmailBtn = new Button(context);
			{
				sendLogViaEmailBtn.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent, 0);
				sendLogViaEmailBtn.Text = "Send log via email";
				sendLogViaEmailBtn.Click += (sender, e) => SendLogViaEmail.Invoke();
			}
			AddView(sendLogViaEmailBtn);
		}

		public void SetImageBitmap(Bitmap imageBitmap)
		{
			if(ImageBitmap != null)
			{
				ImageBitmap.Recycle();
			}
			ImageView.SetImageBitmap(ImageBitmap = imageBitmap); 

		}
	}
}


