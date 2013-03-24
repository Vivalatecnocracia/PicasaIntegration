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

namespace KarmaMaker.PicasaIntegration
{
	[Activity (Label = "KarmaMaker.PicasaIntegration", MainLauncher = true)]
	public class MainActivity : Activity
	{
		private const int PickImageRequestCode = 0x0100;

		private MainView MainView { get; set; }
		private File ImageTempFile { get; set; }

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			MainView = new MainView(this);
			MainView.LoadFromGallery += OnLoadFromGallery;

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

				RunOnUiThread(() => Toast.MakeText(this, string.Format("SelectedImageUri == {0}", selectedImgUri), ToastLength.Long).Show());

				if(IsImageFromPicasa(selectedImgUri))
				{
					RunOnUiThread(() => Toast.MakeText(this, string.Format("Load from Picasa", selectedImgUri), ToastLength.Long).Show());
					using(var inputStream = ContentResolver.OpenInputStream(selectedImgUri))
					{
						MainView.SetImageBitmap(BitmapFactory.DecodeStream(inputStream));
					} 
				}
				else
				{
					RunOnUiThread(() => Toast.MakeText(this, string.Format("Load from local storage", selectedImgUri), ToastLength.Long).Show());
					String[] filePathColumn = { MediaStore.MediaColumns.Data };
					var cursor = ContentResolver.Query (selectedImgUri, filePathColumn, null, null, null);
					cursor.MoveToFirst();
					int columnIndex = cursor.GetColumnIndex(MediaStore.MediaColumns.Data);
					String picturePath = cursor.GetString(columnIndex);
					cursor.Close();
					MainView.SetImageBitmap(BitmapFactory.DecodeFile(picturePath));
				}
			}
			else
			{
				RunOnUiThread(() => Toast.MakeText(this, string.Format("RequestCode == {0} | ResultCode == {1} | Data == {2}", requestCode, resultCode, data), ToastLength.Long).Show());
			}
		}

		private bool IsImageFromPicasa(Android.Net.Uri imageUri)
		{
			// This is quite stupid, I know.
			return imageUri.ToString().Contains("//com.google.android.gallery3d.provider/picasa/item/");
		}
	}
	
	class MainView : LinearLayout
	{
		public event Action LoadFromGallery;
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


