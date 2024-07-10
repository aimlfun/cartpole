using System.Drawing.Imaging;

namespace Cart;

/// <summary>
/// Manages "frames" (screenshots) for the game.
/// This allows us to step thru the action frame by frame after the game has finished
/// </summary>
internal class FrameManager
{
    /// <summary>
    /// The list of frames being tracked
    /// </summary>
    private readonly List<Bitmap> Frames = [];

    /// <summary>
    /// Definition of callback when a frame gets added.
    /// </summary>
    /// <param name="FrameCount"></param>
    public delegate void FrameAddedEvent(int FrameCount);

    /// <summary>
    /// Definition of callback when the frames are reset.
    /// </summary>
    public delegate void ResetEvent();

    /// <summary>
    /// Subscribe to know when frames were added.
    /// Example: you can display the number of frames, or the frame itself.
    /// </summary>
    public event FrameAddedEvent? OnFrameAdded;

    /// <summary>
    /// Subscribe to know when frames were reset. i.e. there are none. WARNING: Images of the frames are disposed.
    /// Example: when the game is reset, and you are displaying a frame somewhere.
    /// </summary>
    public event ResetEvent? OnReset;

    /// <summary>
    /// Returns the number of frames.
    /// </summary>
    internal int FrameCount => Frames.Count;

    /// <summary>
    /// Adds frame, and notifies subscribers.
    /// </summary>
    /// <param name="frame"></param>
    internal void AddFrame(Bitmap frame)
    {
        Frames.Add(frame);

        // notify subscribers
        OnFrameAdded?.Invoke(Frames.Count);
    }

    /// <summary>
    /// Retrieves a frame "image" 0..frame-count-1. WARNING: It does not clone the image, so do not dispose of it.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal Bitmap GetFrame(int index)
    {
        if (index < 0 || index >= Frames.Count)
            throw new ArgumentOutOfRangeException(nameof(index), "Index out of range.");

        return Frames[index];
    }

    /// <summary>
    /// Disposes existing frames, and notifies subscribers.
    /// </summary>
    internal void Reset()
    {
        foreach(Bitmap frame in Frames)
        {
            frame.Dispose();
        }

        Frames.Clear();

        OnReset?.Invoke();
    }

    /// <summary>
    /// Computes the difference between two frames.
    /// Caller must .Dispose() of image.
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal unsafe Bitmap? DiffOfFrames(int frame)
    {
        int frame1 = frame - 1;
        int frame2 = frame;

        if (frame == 0) // we cannot have frame -1.
        {
            frame1 = 0;
        }

        if (frame2 >= Frames.Count) throw new ArgumentOutOfRangeException(nameof(frame), "Index out of range.");
        if (frame1 < 0 || frame1 >= Frames.Count) throw new ArgumentOutOfRangeException(nameof(frame), "Index out of range.");

        // retrieve the images of the 2 frames
        Bitmap bitmap1 = Frames[frame1];
        Bitmap bitmap2 = Frames[frame2];

        // create a bitmap that will be the difference between the two images
        Bitmap diff = new(bitmap1.Width, bitmap1.Height);

        // lock the bitmaps to access the pixel data
        BitmapData data1 = bitmap1.LockBits(new Rectangle(0, 0, bitmap1.Width, bitmap1.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        BitmapData data2;
        
        // if the frames are the same, we don't need to lock it, as it's locked above.
        if (frame1 != frame2) data2 = bitmap2.LockBits(new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb); else data2 = data1;

        BitmapData diffData = diff.LockBits(new Rectangle(0, 0, diff.Width, 110), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        // get the pointers
        byte* data1Ptr = (byte*)data1.Scan0.ToPointer();
        byte* data2Ptr = (byte*)data2.Scan0.ToPointer();
        byte* diffPtr = (byte*)diffData.Scan0.ToPointer();
        
        int startRaster = 100 * bitmap1.Width * 4; // we don't care about any region other than the cart and pole
        int endRaster = 210 * bitmap1.Width * 4; // therefore, we clip those lines of pixels

        // iterate through the images and calculate the difference
        for (int i = startRaster; i < endRaster; i += 4)
        {
            diffPtr[i - startRaster + 3] = 255; // 100% alpha

            if (data1Ptr[i] == 255 && data2Ptr[i] == 255) continue; // white background

            if (data1Ptr[i] > data2Ptr[i])
            {
                // if the pixel in the first image is greater than the pixel in the second image, set the pixel to red
                diffPtr[i - startRaster + 1] = 255; // g
            }
            else
            {
                if (data1Ptr[i] < data2Ptr[i])
                    // if the pixel in the first image is less than the pixel in the second image, set the pixel to blue
                    diffPtr[i - startRaster + 2] = 255; // r
                else
                    // equal
                    diffPtr[i - startRaster + 0] = 255; // b
            }
        }

        // unlock the bitmaps - we locked them for read/write, so we need to unlock them
        bitmap1.UnlockBits(data1);

        if (frame1 != frame2) bitmap2.UnlockBits(data2); // if the frames are the same, we don't need to unlock it

        diff.UnlockBits(diffData);

        return diff;
    }
}
