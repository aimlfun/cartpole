using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Cart;

internal static class ImageProcessing
{
    /// <summary>
    /// Extracts the raw bytes from a bitmap.
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    internal static unsafe byte[] GetBytesFromBitmap(Bitmap bitmap, int channel)
    {
        if (channel < 0 || channel > 3)
        {
            throw new ArgumentException("Channel must be between 0 and 3");
        }

        int width = bitmap.Width;
        int height = bitmap.Height;

        int pixelsPerByte = GetBytesPerPixel(bitmap);

        byte[] bitmapBytes = new byte[width * height * pixelsPerByte];

        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

        try
        {
            Marshal.Copy(data.Scan0, bitmapBytes, 0, bitmapBytes.Length);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        // extract all the pixels for a specific channel

        byte[] channelBytes = new byte[width * height];

        fixed (byte* pBuffer = bitmapBytes)
        {
            byte* pPixel = pBuffer + channel; // start offset of first pixel channel (0,1,2,3A).

            for (int aiBufferIndex = 0; aiBufferIndex < channelBytes.Length; aiBufferIndex++)
            {
                channelBytes[aiBufferIndex] = *(pPixel);

                pPixel += pixelsPerByte; // add (typically) 4 to move to next pixel
            }
        }

        return channelBytes;
    }

    /// <summary>
    /// Converts a bitmap byte array back into a bitmap.
    /// </summary>
    /// <param name="imageAsBytes"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="pixelFormat">The type of bitmap you want.</param>
    /// <returns></returns>
    internal unsafe static Bitmap GetBitmapFromBytes(byte[] imageAsBytes, int width, int height, PixelFormat pixelFormat)
    {
        int pixelsPerByte = GetBytesPerPixel(pixelFormat);

        Bitmap outputBitmap = new(width, height, pixelFormat);
        BitmapData data = outputBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, pixelFormat);
        
        try
        {
            int pixelPos = 0;

            byte* dataPtr = (byte*)data.Scan0.ToPointer();

            byte* pBuffer = dataPtr;

            for (int i = 0; i < imageAsBytes.Length; i++)
            {
                pBuffer[pixelPos + 2] = imageAsBytes[i]; // set red channel pixel

                // set the alpha, without this the pixel won't show
                if (pixelFormat == PixelFormat.Format32bppArgb)
                {
                    pBuffer[pixelPos + 3] = 255;
                }

                pixelPos += pixelsPerByte;
            }
        }
        finally
        {
            outputBitmap.UnlockBits(data);
        }

        return outputBitmap;
    }

    /// <summary>
    /// Returns a bitmap (represented in bytes) as a double array.
    /// This does little more than convert pixel values from 0-255 to 0-1.
    /// </summary>
    /// <param name="imageAsBytes"></param>
    /// <returns></returns>
    internal static double[] BitmapBytesToDouble(byte[] imageAsBytes)
    {
        double[] imageAsDoubles = new double[imageAsBytes.Length];

        for (int i = 0; i < imageAsBytes.Length; i++)
        {
            imageAsDoubles[i] = imageAsBytes[i] / 255f;
        }

        return imageAsDoubles;
    }

    /// <summary>
    /// Gets the number of bytes per pixel for the given bitmap.
    /// </summary>
    /// <param name="b">The bitmap.</param>
    /// <returns>The number of bytes per pixel.</returns>
    private static int GetBytesPerPixel(Bitmap b)
    {
        return GetBytesPerPixel(b.PixelFormat);
    }

    /// <summary>
    /// Gets the number of bytes per pixel for the given pixel format.
    /// </summary>
    /// <param name="pixelFormat">The pixel format.</param>
    /// <returns>The number of bytes per pixel.</returns>
    /// <exception cref="Exception">Thrown when the pixel format is unsupported.</exception>
    private static int GetBytesPerPixel(PixelFormat pixelFormat)
    {
        return pixelFormat switch
        {
            PixelFormat.Format24bppRgb => 3,
            PixelFormat.Format32bppArgb => 4,
            _ => throw new ArgumentException("Unsupported pixel format: " + pixelFormat),
        };
    }

    /// <summary>
    /// Visualise the neural network.
    /// What we are trying to achieve is to see how it changed the weightings, and how that
    /// manifests itself in the image.
    /// We set each neuron individually to full "on" (and all others off), and see what the output 
    /// is. We then normalise the output to 0..1.
    /// </summary>
    /// <param name="neuralNetwork"></param>
    /// <returns>Returns 32bppArgb bitmap.</returns>
    internal static Bitmap GetNNAsBitmap(NeuralNetwork neuralNetwork, int width, int height)
    {
        int sizeLength = width * height; // one double per pixel

        double[] nn = new double[sizeLength];
        double min = double.MaxValue;
        double max = -double.MaxValue;

        // plug in a "1" as an input into each neuron and see the response.
        // at the same time, find out min/max across all neurons
        for (int i = 0; i < sizeLength; i++)
        {
            // value of the neuron
            double value = neuralNetwork.GetNeuron(i);

            // track min/max
            if (value < min) min = value;
            if (value > max) max = value;

            nn[i] = value;
        }

        // image we want is RGB, 3 channels
        byte[] imageConstructed = new byte[4 * nn.Length];

        int pixeloffset = 0;

        // normalise the delta
        for (int i = 0; i < nn.Length; i++)
        {
            byte value = (byte)(255f * (nn[i] - max) / (max - min));

            // 20 for all channels not set
            imageConstructed[pixeloffset + 0] = (byte)(nn[i] == 0.0 ? 0 : 20); // blue
            imageConstructed[pixeloffset + 1] = (nn[i] > 0) ? value : (byte)20; // green
            imageConstructed[pixeloffset + 2] = (nn[i] < 0) ? value : (byte)20; // red
            imageConstructed[pixeloffset + 3] = 255; // alpha

            pixeloffset += 4;
        }

        // make a bitmap to return
        Bitmap newBitmap = new(width, height, PixelFormat.Format32bppArgb);

        // we have to lock the rectangle to set the bitmap data
        BitmapData newBitmapData = newBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            // copies the image byte array into the bitmap
            Marshal.Copy(imageConstructed, 0, (int)newBitmapData.Scan0, imageConstructed.Length);
        }
        finally
        {
            // unlock the bitmap, otherwise it will cause a problem later!
            newBitmap.UnlockBits(newBitmapData);
        }

        return newBitmap;
    }

    /// <summary>
    /// Paints which neurons are firing, and what colour on the image.
    /// </summary>
    /// <param name="neuralNetwork"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    internal static Bitmap GetNNFiringAsBitmap(NeuralNetwork neuralNetwork, int width, int height)
    {
        int sizeLength = width * height;

        double[] nn = new double[sizeLength];
        double min = double.MaxValue;
        double max = -double.MaxValue;

        for (int i = 0; i < sizeLength; i++)
        {
            // actual values of the input, for the image being processsed
            double value = neuralNetwork.GetActualNeuron(i);

            if (value < min) min = value;
            if (value > max) max = value;

            nn[i] = value;
        }

        byte[] imageConstructed = new byte[4 * nn.Length];

        int pixeloffset = 0;
        double x = Math.Max(Math.Abs(max), Math.Abs(min));

        // normalise the delta
        for (int i = 0; i < nn.Length; i++)
        {
            byte value = (byte)(Math.Abs(nn[i] * (128 / x)));

            imageConstructed[pixeloffset + 0] = (byte)(nn[i] == 0.0 ? 0 : 20); // blue
            imageConstructed[pixeloffset + 1] = (byte)(nn[i] > 0 ? 127 + value : 20); // green
            imageConstructed[pixeloffset + 2] = (byte)(nn[i] < 0 ? 127 + value : 20); // red
            imageConstructed[pixeloffset + 3] = 255; // alpha
            pixeloffset += 4;
        }

        // make a bitmap to return
        Bitmap newBitmap = new(width, height, PixelFormat.Format32bppArgb);

        // we have to lock the rectangle to set the bitmap data
        BitmapData newBitmapData = newBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            // copies the image byte array into the bitmap
            Marshal.Copy(imageConstructed, 0, (int)newBitmapData.Scan0, imageConstructed.Length);
        }
        finally
        {
            // unlock the bitmap, otherwise it will cause a problem later!
            newBitmap.UnlockBits(newBitmapData);
        }

        return newBitmap;
    }
}