#define goodQualityLines
namespace Cart;

/// <summary>
/// Caches images of the pole at different angles, and the corresponding neural network input.
/// </summary>
internal static class AngleImageInputCache
{
    // ok, so for purists out there, the cache should be key of angle, object with bitmap + nn as properties. I chose not to. Doing so ensures both exist for a given angle, and that they are in sync.

    /// <summary>
    /// Cache of images of the pole at different angles.
    /// </summary>
    readonly static Dictionary<float, Bitmap> s_cacheOfAngleBitmaps = [];

    /// <summary>
    /// Cache of the NN input for the images of the pole at different angles.
    /// </summary>
    readonly static Dictionary<float, double[]> s_cacheOfAngleNNinput = [];

    /// <summary>
    /// Radius of pole
    /// </summary>
    const int c_smallRadius = 25;

    /// <summary>
    /// Width of image.
    /// </summary>
    internal const int c_smallWidth = c_smallRadius + 17;

    /// <summary>
    /// Height of image.
    /// </summary>
    internal const int c_smallHeight = 26;

    /// <summary>
    /// The size of the image array, and therefore NN input size
    /// </summary>
    internal static int ImageArraySize => c_smallWidth * c_smallHeight;

    /// <summary>
    /// Returns the image of the pole at the given angle.
    /// </summary>
    /// <param name="angleDegrees"></param>
    /// <returns></returns>
    internal static Bitmap GetImage(float angleDegrees)
    {
        GenerateImageIfNotInCache(angleDegrees);
        return s_cacheOfAngleBitmaps[angleDegrees];
    }

    /// <summary>
    /// Returns the image as a double array, suitable for input to a neural network.
    /// </summary>
    /// <param name="angleDegrees"></param>
    /// <returns></returns>
    internal static double[] GetNNInput(float angleDegrees)
    {
        GenerateImageIfNotInCache(angleDegrees);
        return s_cacheOfAngleNNinput[angleDegrees];
    }

    /// <summary>
    /// Paints the image, grabs the bytes, and stores them in the cache.
    /// </summary>
    /// <param name="angleDegrees"></param>
    internal static void GenerateImageIfNotInCache(float angleDegrees)
    {
        if (s_cacheOfAngleBitmaps.ContainsKey(angleDegrees))
        {
            return;
        }

        // we have arbitrarily chosen 0 = up, -45..45.
        // but 0 degrees is horizontal. if we draw that, then the pole will be half off the image (+45), and wrong way (-45).
        // so we draw after rotating 90 degrees, so "0" points up.
        float angleRadians = (angleDegrees + 90) * (float)Math.PI / 180; // sin/cos want radians

        int midpointX = c_smallWidth / 2;

        Bitmap poleImage = new(c_smallWidth, c_smallHeight);

        using Graphics g = Graphics.FromImage(poleImage);
        g.Clear(Color.Black);

#if goodQualityLines
        // good quality lines
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
#endif
        // simple math, to rotate pole
        float y = (float)Math.Sin(angleRadians) * c_smallRadius;
        float x = (float)Math.Cos(angleRadians) * c_smallRadius;

        // draw pole
        using Pen p = new(Color.White, 2);

        g.DrawLine(p, midpointX - x, c_smallHeight - y, midpointX, c_smallHeight);

        g.FillRectangle(Brushes.White, midpointX - 5, c_smallHeight-4, 10, c_smallHeight);

        s_cacheOfAngleBitmaps.Add(angleDegrees, poleImage);

        byte[] image = ImageProcessing.GetBytesFromBitmap(poleImage, 1);

        //using Bitmap test = ImageProcessing.GetBitmapFromBytes(image, c_smallWidth, c_smallHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb)  <- uncomment and add ";" to check the image
        //test.Save(@"c:\temp\bit-map-bytes.png")

        double[] input = ImageProcessing.BitmapBytesToDouble(image);

        s_cacheOfAngleNNinput.Add(angleDegrees, input);
    }
}