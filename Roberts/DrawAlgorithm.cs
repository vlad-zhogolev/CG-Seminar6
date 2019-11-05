using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Roberts
{
    public class DrawAlgorithm
    {
        public static void SetPixel(int x, int y, Color color, WriteableBitmap writeableBitmap)
        {
            int column = x;
            int row = y;

            try
            {
                // Reserve the back buffer for updates.
                writeableBitmap.Lock();

                unsafe
                {
                    // Get a pointer to the back buffer.
                    IntPtr pBackBuffer = writeableBitmap.BackBuffer;

                    // Find the address of the pixel to draw.
                    pBackBuffer += row * writeableBitmap.BackBufferStride;
                    pBackBuffer += column * 4;

                    // Compute the pixel's color.
                    int color_data = color.R << 16; // R 
                    color_data |= color.G << 8;   // G
                    color_data |= color.B << 0;   // B

                    // Assign the color data to the pixel.
                    *((int*)pBackBuffer) = color_data;
                }

                // Specify the area of the bitmap that changed.
                writeableBitmap.AddDirtyRect(new Int32Rect(column, row, 1, 1));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                writeableBitmap.Unlock();
            }
        }

        public static void SetPixelIfPossible(int x, int y, Color color, WriteableBitmap bitmap)
        {
            if (x < 0 || x >= bitmap.Width || y < 0 || y >= bitmap.Height)
            {
                return;
            }
            SetPixel(x, y, color, bitmap);
        }

        public static void DrawLine(WriteableBitmap bitmap, Color color, int x1, int y1, int x2, int y2)
        {
            if (x1 == x2 && y1 == y2)
            {
                SetPixelIfPossible(x1, y1, color, bitmap);
                return;
            }

            int x = x1;
            int y = y1;
            int deltaX = Math.Abs(x1 - x2);
            int deltaY = Math.Abs(y1 - y2);
            int signX = Math.Sign(x2 - x1);
            int signY = Math.Sign(y2 - y1);
            bool isSwapped = false;

            if (deltaY > deltaX)
            {
                var tmp = deltaX;
                deltaX = deltaY;
                deltaY = tmp;
                isSwapped = true;
            }
            int error = 2 * deltaY - deltaX;
            for (int i = 0; i <= deltaX; ++i)
            {
                SetPixelIfPossible(x, y, color, bitmap);
                if (error >= 0)
                {
                    if (isSwapped)
                    {
                        x += signX;
                    }
                    else
                    {
                        y += signY;
                    }
                    error -= 2 * deltaX;
                }
                if (isSwapped)
                {
                    y += signY;
                }
                else
                {
                    x += signX;
                }
                error += 2 * deltaY;
            }
        }

        public static void ResetColor(Color color, WriteableBitmap bitmap)
        {
            Int32Rect rect = new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
            int bytesPerPixel = bitmap.Format.BitsPerPixel / 8; // typically 4 (BGR32)
            byte[] empty = new byte[rect.Width * rect.Height * bytesPerPixel]; // cache this one
            int emptyStride = rect.Width * bytesPerPixel;
            bitmap.WritePixels(rect, empty, emptyStride, 0);
        }
    }
}
