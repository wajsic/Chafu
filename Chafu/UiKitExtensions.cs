﻿using System;
using CoreAnimation;
using CoreGraphics;
using CoreMedia;
using UIKit;

namespace Chafu
{
    /// <summary>
    /// Extension method for <see cref="UIKit"/> classes
    /// </summary>
    public static class UiKitExtensions
    {
        /// <summary>
        /// Name for border added to tab items
        /// </summary>
        public const string BorderLayerName = "ChafuBottomBorder";

        /// <summary>
        /// Add bottom border to UIView
        /// </summary>
        /// <param name="self"><see cref="UIView"/> to add border to</param>
        /// <param name="color"><see cref="UIColor"/> of the border</param>
        /// <param name="width">Border width</param>
        public static void AddBottomBorder(this UIView self, UIColor color, float width)
        {
            if (self == null) return;
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be bigger than 0");

            var border = new CALayer
            {
                BorderColor = color.CGColor,
                Frame = new CGRect(0, self.Frame.Height - width, self.Frame.Width, width),
                BorderWidth = width,
                Name = BorderLayerName
            };
            self.Layer.AddSublayer(border);
        }

        /// <summary>
        /// Alignment for scaling images
        /// </summary>
        public enum UIImageAlignment
        {
            /// <summary>
            /// Center align
            /// </summary>
            Center,
            /// <summary>
            /// Left align
            /// </summary>
            Left,
            /// <summary>
            /// Top align
            /// </summary>
            Top,
            /// <summary>
            /// Right align
            /// </summary>
            Right,
            /// <summary>
            /// Bottom align
            /// </summary>
            Bottom,
            /// <summary>
            /// Top-left align
            /// </summary>
            TopLeft,
            /// <summary>
            /// Bottom-right align
            /// </summary>
            BottomRight,
            /// <summary>
            /// Bottom-left align
            /// </summary>
            BottomLeft,
            /// <summary>
            /// Top-right align
            /// </summary>
            TopRight
        }

        /// <summary>
        /// Scale mode for scaling images
        /// </summary>
        public enum UIImageScaleMode
        {
            /// <summary>
            /// Fill
            /// </summary>
            Fill,
            /// <summary>
            /// Fill with aspect ratio preserved
            /// </summary>
            AspectFill,
            /// <summary>
            /// Fit image with aspect ratio preserved
            /// </summary>
            AspectFit
        }

        /// <summary>
        /// Scale image
        /// </summary>
        /// <param name="image"><see cref="UIImage"/> to scale</param>
        /// <param name="size"><see cref="CGSize"/> size to scale to</param>
        /// <param name="scaleMode"><see cref="UIImageScaleMode"/> scale mode</param>
        /// <param name="alignment"><see cref="UIImageAlignment"/> alignment</param>
        /// <param name="trim">Trim blank space</param>
        /// <returns></returns>
        public static UIImage ScaleImage(this UIImage image, CGSize size, UIImageScaleMode scaleMode = UIImageScaleMode.AspectFit,
            UIImageAlignment alignment = UIImageAlignment.Center, bool trim = false)
        {
            if (image == null) return null;

            var width = size == CGSize.Empty ? 1 : size.Width;
            var height = size == CGSize.Empty ? 1 : size.Height;

            var widthScale = width / image.Size.Width;
            var heightScale = height / image.Size.Height;

            switch (scaleMode)
            {
                case UIImageScaleMode.AspectFit:
                    {
                        var scale = (nfloat)Math.Min(widthScale, heightScale);
                        widthScale = scale;
                        heightScale = scale;
                        break;
                    }
                case UIImageScaleMode.AspectFill:
                    {
                        var scale = (nfloat)Math.Max(widthScale, heightScale);
                        widthScale = scale;
                        heightScale = scale;
                        break;
                    }
            }

            var newWidth = widthScale * image.Size.Width;
            var newHeight = heightScale * image.Size.Height;

            var canvasWidth = trim ? newWidth : size.Width;
            var canvasHeight = trim ? newHeight : size.Height;

            UIGraphics.BeginImageContextWithOptions(new CGSize(canvasWidth, canvasHeight), false, 0);

            var originX = 0f;
            var originY = 0f;

            if (scaleMode == UIImageScaleMode.AspectFit)
            {
                switch (alignment)
                {
                    case UIImageAlignment.Center:
                        originX = (float)((canvasWidth - newWidth) / 2);
                        originY = (float)((canvasHeight - newHeight) / 2);
                        break;
                    case UIImageAlignment.Top:
                        originX = (float)((canvasWidth - newWidth) / 2);
                        break;
                    case UIImageAlignment.Left:
                        originY = (float)((canvasHeight - newHeight) / 2);
                        break;
                    case UIImageAlignment.Bottom:
                        originX = (float)((canvasWidth - newWidth) / 2);
                        originY = (float)(canvasHeight - newHeight);
                        break;
                    case UIImageAlignment.Right:
                        originX = (float)(canvasWidth - newWidth);
                        originY = (float)((canvasHeight - newHeight) / 2);
                        break;
                    case UIImageAlignment.TopRight:
                        originX = (float)(canvasWidth - newWidth);
                        break;
                    case UIImageAlignment.BottomLeft:
                        originY = (float)(canvasHeight - newHeight);
                        break;
                    case UIImageAlignment.BottomRight:
                        originX = (float)(canvasWidth - newWidth);
                        originY = (float)(canvasHeight - newHeight);
                        break;
                    case UIImageAlignment.TopLeft:
                    default:
                        break;
                }
            }

            image.Draw(new CGRect(originX, originY, newWidth, newHeight));
            var scaledImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return scaledImage;
        }

        /// <summary>
        /// Convert <see cref="CMTime"/> to double
        /// </summary>
        /// <param name="duration"><see cref="CMTime"/> to convert</param>
        /// <returns><see cref="double"/> with seconds</returns>
        public static double ToDouble(this CMTime duration)
        {
            if (duration.IsIndefinite)
                return double.NaN;

            if (duration.IsNegativeInfinity)
                return double.NegativeInfinity;

            if (duration.IsPositiveInfinity)
                return double.PositiveInfinity;

            return duration.Seconds;
        }
    }

    /// <summary>
    /// Extension methods for CoreAnimation classes
    /// </summary>
    public static class CoreAnimationExtensions
    {
        /// <summary>
        /// Make perspective, such that layers can be rotated around Z-axis
        /// </summary>
        /// <param name="transform"><see cref="CATransform3D"/> to add perspective to</param>
        /// <param name="eyePosition">Eye position</param>
        /// <returns>The <see cref="CATransform3D"/> for the perspective.</returns>
        public static CATransform3D MakePerspective(this CATransform3D transform,
                                                    nfloat eyePosition)
        {
            transform.m34 = -1.0f / eyePosition;
            return transform;
        }

        /// <summary>
        /// Creates the transform for the roll angle around the Z axis
        /// </summary>
        /// <returns>The roll <see cref="CATransform3D"/>.</returns>
        /// <param name="rollAngle">Roll angle as <see cref="nfloat"/>.</param>
        public static CATransform3D ToRollTransform(this nfloat rollAngle)
        {
            var radians = rollAngle.ToRadians();
            return CATransform3D.MakeRotation(radians, 0.0f, 0.0f, 1.0f);
        }

        /// <summary>
        /// Creates the transform for the yaw angle around the Y axis, while
        /// taking the orientation of the screen in consideration.
        /// </summary>
        /// <returns>The yaw <see cref="CATransform3D"/>.</returns>
        /// <param name="yawAngle">Yaw angle as <see cref="nfloat"/>.</param>
        public static CATransform3D ToYawTransform(this nfloat yawAngle)
        {
            var radians = yawAngle.ToRadians();

            var yawTransform = CATransform3D.MakeRotation(radians, 0.0f, -1.0f, 0.0f);
            var orientationTransform = OrientationTransform();
            return orientationTransform.Concat(yawTransform);
        }

        private static CATransform3D OrientationTransform()
        {
            return OrientationTransform(UIDevice.CurrentDevice.Orientation);
        }

        /// <summary>
        /// Creates rotation tranformation around the Z axis for the orientation of 
        /// the device.
        /// </summary>
        /// <returns>The orientation <see cref="CATransform3D"/>.</returns>
        /// <param name="orientation">Orientation.</param>
        public static CATransform3D OrientationTransform(
            this UIDeviceOrientation orientation)
        {
            nfloat angle = 0.0f;
            switch (orientation)
            {
                case UIDeviceOrientation.PortraitUpsideDown:
                    angle = (nfloat)Math.PI;
                    break;
                case UIDeviceOrientation.LandscapeRight:
                    angle = (nfloat)(-Math.PI / 2.0f);
                    break;
                case UIDeviceOrientation.LandscapeLeft:
                    angle = (nfloat)Math.PI / 2.0f;
                    break;
                default:
                    angle = 0.0f;
                    break;
            }

            return CATransform3D.MakeRotation(angle, 0.0f, 0.0f, 1.0f);
        }
    }

    /// <summary>
    /// Maths extensions
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Convert degrees to radians
        /// </summary>
        /// <param name="degrees">Angle in degrees</param>
        /// <returns>Angle in radians</returns>
        public static nfloat ToRadians(this nfloat degrees) =>
            degrees * (nfloat)Math.PI / 180f;
    }
}