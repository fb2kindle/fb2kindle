using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Fb2Kindle {
  internal static class ImagesHelper {
    
    internal static Bitmap MakeGrayscale3(Bitmap original) {
      var newBitmap = new Bitmap(original.Width, original.Height);
      var g = Graphics.FromImage(newBitmap);
      var colorMatrix = new ColorMatrix(new[] {
        new[] {.3f, .3f, .3f, 0, 0},
        new[] {.59f, .59f, .59f, 0, 0},
        new[] {.11f, .11f, .11f, 0, 0},
        new float[] {0, 0, 0, 1, 0},
        new float[] {0, 0, 0, 0, 1}
      });
      var attributes = new ImageAttributes();
      attributes.SetColorMatrix(colorMatrix);
      g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
        0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
      g.Dispose();
      return newBitmap;
    }

    internal static Image GrayScale(Image img, bool fast, ImageFormat format) {
      Stream imageStream = new MemoryStream();
      if (fast) {
        using (var bmp = new Bitmap(img)) {
          var gsBmp = MakeGrayscale3(bmp);
          gsBmp.Save(imageStream, format);
        }
      }
      else {
        using (var bmp = new Bitmap(img)) {
          for (var y = 0; y < bmp.Height; y++)
          for (var x = 0; x < bmp.Width; x++) {
            var c = bmp.GetPixel(x, y);
            var rgb = (c.R + c.G + c.B) / 3;
            bmp.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
          }
          bmp.Save(imageStream, format);
        }
      }
      return Image.FromStream(imageStream);
    }

    internal static string GetMimeType(Image image) {
      return GetMimeType(image.RawFormat);
    }

    internal static string GetMimeType(ImageFormat imageFormat) {
      var codecs = ImageCodecInfo.GetImageEncoders();
      return codecs.First(codec => codec.FormatID == imageFormat.Guid).MimeType;
    }

    internal static ImageFormat GetImageFormatFromMimeType(string contentType, ImageFormat defaultResult) {
      if (GetMimeType(ImageFormat.Jpeg).Equals(contentType, StringComparison.OrdinalIgnoreCase)) {
        return ImageFormat.Jpeg;
      }
      if (GetMimeType(ImageFormat.Bmp).Equals(contentType, StringComparison.OrdinalIgnoreCase)) {
        return ImageFormat.Bmp;
      }
      if (GetMimeType(ImageFormat.Png).Equals(contentType, StringComparison.OrdinalIgnoreCase)) {
        return ImageFormat.Png;
      }

      // foreach (var codecInfo in ImageCodecInfo.GetImageEncoders()) {
      //   if (codecInfo.MimeType.Equals(contentType, StringComparison.OrdinalIgnoreCase)) {
      //     return codecInfo.FormatID;
      //   }
      // }

      return defaultResult;
    }

    internal static void AutoScaleImage(string imageFilePath, bool magnify, int width, int height) {
      Image scaledImage = null;
      var imgFormat = ImageFormat.Png;
      using (var img = Image.FromFile(imageFilePath)) {
        if (img.Size.Width > width && img.Size.Height > height || magnify && img.Size.Width < width && img.Size.Height < height) {
          imgFormat = GetImageFormatFromMimeType(GetMimeType(img), ImageFormat.Png);
          scaledImage = ResizeImage(img, width, height);
        }
      }
      if (scaledImage == null) return;
      scaledImage.Save(imageFilePath, imgFormat);
      scaledImage.Dispose();
    }

    internal static bool AutoScaleImage(byte[] imageBytes, ImageFormat format, bool magnify, int width, int height, out byte[] scaledBytes) {

      scaledBytes = imageBytes;
      using (var img = Image.FromStream(new MemoryStream(imageBytes))) {

        if ((img.Size.Width <= width || img.Size.Height <= height) &&
            (!magnify || img.Size.Width >= width || img.Size.Height >= height)) {
          return false;
        }

        format = GetImageFormatFromMimeType(GetMimeType(img), format);
        using (var scaledImage = ResizeImage(img, width, height)) {
          var output = new MemoryStream();
          scaledImage.Save(output, format);
          scaledBytes = output.ToArray();
          return true;
        }
      }
    }

    internal static double GetScaleFactor(Image original, int width, int height) {
      var originalWidth = original.Width;
      var originalHeight = original.Height;
      double factor;
      if (originalWidth > originalHeight)
        factor = (double)width / originalWidth;
      else
        factor = (double)height / originalHeight;
      return factor;
    }

    internal static Image ResizeImage(Image image, int width, int height) {
      var factor = GetScaleFactor(image, width, height);
      width = (int)Math.Round(image.Width * factor);
      height = (int)Math.Round(image.Height * factor);
      var dstRect = new Rectangle(0, 0, width, height);
      var dstImage = new Bitmap(width, height);
      dstImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
      using (var graphics = Graphics.FromImage(dstImage)) {
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        using (var wrapMode = new ImageAttributes()) {
          wrapMode.SetWrapMode(WrapMode.TileFlipXY);
          graphics.DrawImage(image, dstRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
        }
      }
      return dstImage;
    }
    
    // private static ImageCodecInfo GetEncoderInfo(string extension) {
    //   extension = extension.ToLower();
    //   var codecs = ImageCodecInfo.GetImageEncoders();
    //   for (var i = 0; i < codecs.Length; i++)
    //     if (codecs[i].FilenameExtension.ToLower().Contains(extension))
    //       return codecs[i];
    //   return null;
    // }
    //
    // private static ImageCodecInfo GetEncoderInfo(ImageFormat format) {
    //   return ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.FormatID.Equals(format.Guid));
    // }
    
  }
}