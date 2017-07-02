﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdaptivePlayerViewRenderer.cs" company="Flush Arcade Pty Ltd.">
//   Copyright (c) 2016 Flush Arcade Pty Ltd. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

[assembly: Xamarin.Forms.ExportRenderer(typeof(AdaptiveStreaming.Controls.AdaptivePlayerView), 
                                        typeof(AdaptiveStreaming.Droid.Renderers.AdaptivePlayerViewRenderer))]

namespace AdaptiveStreaming.Droid.Renderers
{
	using System;

	using Android.Widget;

	using Xamarin.Forms;
	using Xamarin.Forms.Platform.Android;

	using AdaptiveStreaming.Droid.Exo;

	using AdaptiveStreaming.Controls;

    using AdaptiveStreaming.Portable.Models;

    /// <summary>
    /// Adaptive player view renderer.
    /// </summary>
    public class AdaptivePlayerViewRenderer : ViewRenderer<AdaptivePlayerView, AdaptivePlayer>
	{
		#region Private Properties

		/// <summary>
        /// The player.
        /// </summary>
		private AdaptivePlayer _player;

		#endregion

		#region Protected Methods

		/// <summary>
		/// Raises the element changed event.
		/// </summary>
		/// <param name="e">E.</param>
		protected override void OnElementChanged(ElementChangedEventArgs<AdaptivePlayerView> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				_player = new AdaptivePlayer(Context);

				SetNativeControl(_player);
			}

			if (e.OldElement != null)
			{
				e.NewElement.InitPlayer -= HandleInitPlayer;

				// something wrong here, not being called on disposal
				_player.StopAndDispose();
				_player.Dispose();
			}

			if (e.NewElement != null)
			{
				e.NewElement.InitPlayer += HandleInitPlayer;
			}
		}

		/// <summary>
		/// Dispose the specified disposing.
		/// </summary>
		/// <param name="disposing">If set to <c>true</c> disposing.</param>
		protected override void Dispose(bool disposing)
		{
            if (Element != null)
            {
			    Element.InitPlayer -= HandleInitPlayer;
            }

			_player.StopAndDispose();

			base.Dispose(disposing);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Handles the camera initialisation.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">If set to <c>true</c> arguments.</param>
		private void HandleInitPlayer(object sender, Stream args)
		{
			_player.InitStream(args);
            _player.InitializePlayer();
		}

		#endregion
	}
}