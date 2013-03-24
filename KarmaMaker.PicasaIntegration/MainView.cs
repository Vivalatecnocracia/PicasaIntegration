using System;
using Android.Widget;
using Android.Content;
using Android.Graphics;
using Android.Views;

namespace KarmaMaker.PicasaIntegration
{
	public class MainView : LinearLayout
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

