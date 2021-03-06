﻿using System;
using System.ComponentModel;
using Xamarin.Forms.Platform.UWP;

using SKFormsView = SkiaSharp.Views.Forms.SKGLView;
using SKNativeView = SkiaSharp.Views.SKSwapChainPanel;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKGLViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	internal class SKGLViewRenderer : ViewRenderer<SKFormsView, SKNativeView>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<SKFormsView> e)
		{
			if (e.OldElement != null)
			{
				var oldController = (ISKGLViewController)e.OldElement;

				// unsubscribe from events
				oldController.SurfaceInvalidated -= OnSurfaceInvalidated;
			}

			if (e.NewElement != null)
			{
				var newController = (ISKGLViewController)e.NewElement;

				// create the native view
				var view = new InternalView(newController);
				SetNativeControl(view);

				// subscribe to events from the user
				newController.SurfaceInvalidated += OnSurfaceInvalidated;

				// start the rendering
				SetRenderMode();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			// refresh the render loop
			if (e.PropertyName == SKFormsView.HasRenderLoopProperty.PropertyName)
			{
				SetRenderMode();
			}
		}

		protected override void Dispose(bool disposing)
		{
			// detach all events before disposing
			var controller = (ISKGLViewController)Element;
			if (controller != null)
			{
				controller.SurfaceInvalidated -= OnSurfaceInvalidated;
			}

			base.Dispose(disposing);
		}

		// the user asked to repaint
		private void OnSurfaceInvalidated(object sender, EventArgs eventArgs)
		{
			// if we aren't in a loop, then refresh once
			if (!Element.HasRenderLoop)
			{
				Control.Invalidate();
			}
		}

		private void SetRenderMode()
		{
			Control.EnableRenderLoop = Element.HasRenderLoop;
		}

		private class InternalView : SKNativeView
		{
			private readonly ISKGLViewController controller;

			public InternalView(ISKGLViewController controller)
			{
				this.controller = controller;
			}

			protected override void OnPaintSurface(SkiaSharp.Views.SKPaintGLSurfaceEventArgs e)
			{
				base.OnPaintSurface(e);

				controller.OnPaintSurface(new SKPaintGLSurfaceEventArgs(e.Surface, e.RenderTarget));
			}
		}
	}
}
