﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomImageRenderer.cs" company="Flush Arcade">
//   Copyright (c) 2016 Flush Arcade All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

[assembly: Xamarin.Forms.ExportRenderer(typeof(Camera.Controls.CustomImage), 
                                        typeof(Camera.Droid.Renderers.CustomImageRenderer))]

namespace Camera.Droid.Renderers
{
	using System;
	using System.Threading.Tasks;
	using System.IO;
	using System.Threading;
	using System.Linq;

	using Xamarin.Forms;
	using Xamarin.Forms.Platform.Android;

	using Android.Graphics;
	using Android.Widget;

	using Camera.Controls;
	using Camera.Portable.Ioc;
	using Camera.Portable.Logging;

	/// <summary>
	/// Custom image renderer.
	/// </summary>
	public class CustomImageRenderer : ViewRenderer<CustomImage, ImageView> 
	{
		/// <summary>
		/// The tag.
		/// </summary>
		private readonly string _tag;

		/// <summary>
		/// The image view.
		/// </summary>
		private ImageView _imageView;

		/// <summary>
		/// The custom image.
		/// </summary>
		private CustomImage _customImage;

		/// <summary>
		/// The log.
		/// </summary>
		private ILogger _log;

		/// <summary>
		/// The bitmap.
		/// </summary>
		private Bitmap _bitmap;

		/// <summary>
		/// Initializes a new instance of the <see cref="LogIt.Droid.Renderers.CustomImageRenderer"/> class.
		/// </summary>
		public CustomImageRenderer()
		{
			_log = IoC.Resolve<ILogger> ();
			_tag = string.Format ("{0} ", GetType ());
		}

		/// <summary>
		/// Raises the element changed event.
		/// </summary>
		/// <param name="e">E.</param>
		protected override void OnElementChanged (ElementChangedEventArgs<CustomImage> e)
		{
			base.OnElementChanged (e);

			if (Control == null)
			{
				_imageView = new ImageView(Context);

				// Instantiate the native control
				SetNativeControl(_imageView);
			}

			if (e.OldElement != null)
			{
				_imageView.Dispose();

				// Unsubscribe from event handlers and cleanup any resources
				e.OldElement.CustomPropertyChanged -= HandleCustomPropertyChanged;
			}

			if (e.NewElement != null)
			{
				_customImage = e.NewElement;

				SetAspect();

				Android.App.Application.SynchronizationContext.Post(state =>
				{
					UpdateControlColor();
				}, null);

				LoadImage().ConfigureAwait(false);

				e.NewElement.CustomPropertyChanged += HandleCustomPropertyChanged;
				// Configure the control and subscribe to event handlers
			}
		}

		/// <summary>
		/// Sets the aspect.
		/// </summary>
		private void SetAspect()
		{
			if (Element != null)
			{
				switch (Element.Aspect) 
				{
				case Aspect.AspectFill:
					_imageView.SetScaleType (ImageView.ScaleType.FitXy);
					break;
				case Aspect.AspectFit:
					_imageView.SetScaleType (ImageView.ScaleType.FitCenter);
					break;
				case Aspect.Fill:
					_imageView.SetScaleType (ImageView.ScaleType.FitXy);
					break;
				default:
					_imageView.SetScaleType (ImageView.ScaleType.FitCenter);
					break;
				}
			}
		}

		/// <summary>
		/// Handles the custom property changed.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="propertyName">Property name.</param>
		private void HandleCustomPropertyChanged (object sender, string propertyName)
		{
			switch (propertyName)
			{
				case "TintColorString":
				case "TintOn":
					Android.App.Application.SynchronizationContext.Post(state =>
					{
						UpdateControlColor();
					}, null);
					break;
				case "Path":
					LoadImage().ConfigureAwait(false);
					break;
			}
		}

		/// <summary>
		/// Loads the image.
		/// </summary>
		private async Task LoadImage()
		{
			try
			{
				_bitmap = await ReadBitmapImageFromStorage(Element.Path);

				if (_imageView != null && _bitmap != null)
				{
					// use this for anuimated images as Device.StartTimer runs on a different thread, we need to force this on the UI thread.
					Android.App.Application.SynchronizationContext.Post(state => _imageView.SetImageBitmap(_bitmap), null);
				}
			}
			catch (Exception error)
			{
				_log.WriteLineTime(_tag + "\n" +
					"LoadAsync() Failed to load view model.  \n " +
					"ErrorMessage: \n" +
					error.Message + "\n" +
					"Stacktrace: \n " +
					error.StackTrace);
			}
		}

		/// <summary>
		/// Updates the color of the control.
		/// </summary>
		private void UpdateControlColor()
		{
			try 
			{
				if (_customImage.TintOn && !string.IsNullOrEmpty(_customImage.TintColorString))
				{
					var color = Android.Graphics.Color.ParseColor(_customImage.TintColorString.Replace(" ", ""));
					_imageView.SetColorFilter (color, PorterDuff.Mode.SrcAtop);
				}
			}
			catch (Exception e) 
			{
				_log.WriteLineTime ("CustomImageRenderer: " + e);
			}
		}

		/// <summary>
		/// Reads the bitmap image from storage.
		/// </summary>
		/// <returns>The bitmap image from storage.</returns>
		/// <param name="fn">Fn.</param>
		public async Task<Bitmap> ReadBitmapImageFromStorage(string fn)
		{
			try
			{
				if (!string.IsNullOrEmpty(fn))
				{
					var file = fn.Split('.').FirstOrDefault();

					var id = Resources.GetIdentifier(file, "drawable", Context.PackageName);

					using (Stream stream = Resources.OpenRawResource(id))
					{
						if (stream != null)
						{
							return await BitmapFactory.DecodeResourceAsync(Resources, id);
						}
					}
				}
			}
			catch (Exception error)
			{
				_log.WriteLineTime(
					"MyCareManager.Droid.Renderers.CustomImageRenderer; \n" +
					"ErrorMessage: Failed to load image " + fn + "\n " +
					"Stacktrace: Login Error  \n " +
					error);
			}

			return null;
		}

		/// <summary>
		/// Dispose the specified disposing.
		/// </summary>
		/// <param name="disposing">If set to <c>true</c> disposing.</param>
		protected override void Dispose (bool disposing)
		{
			if (Element != null) 
			{
				Element.CustomPropertyChanged -= HandleCustomPropertyChanged;
			}

			if (_bitmap != null) 
			{
				_bitmap.Recycle ();
				_bitmap.Dispose ();
			}

			base.Dispose (disposing);
		}
	}
}