//#define checkInputs // check inputs are within -1 to 1, if you want to debug the network.

namespace Cart;

/// <summary>
/// UI for simple pole angle detection using a neural network.
/// </summary>
public partial class FormPoleDirection : Form
{
    // either side of 0.5 (0.01 is 2% accuracy required).
    const float c_minAcceptableValueFor0 = 0.49f;
    const float c_minAcceptableValueFor1 = 0.51f;

    /// <summary>
    /// Inputs connected to outputs! Output = Sum(each Inputs * associated Weights) + Bias
    /// </summary>
    private readonly NeuralNetwork smallNN = new([AngleImageInputCache.ImageArraySize, 1]);

    /// <summary>
    /// Set to true when the neural network is trained.
    /// </summary>
    private bool _isTrained = false;

    /// <summary>
    /// Constructor.
    /// </summary>
    public FormPoleDirection()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Trains the neural network to associate the pictures of the pole at different angles with left (<0.5) or right (>=0.5).
    /// </summary>
    /// <param name="angleDegrees"></param>
    private void TrainForAngle(float angleDegrees)
    {
        double[] input = AngleImageInputCache.GetNNInput(angleDegrees);

        // pole-cart, 0=left, 1=right. We simulate this by saying if angle is negative, it's left, else right.
        double expectedOutput = angleDegrees < 0 ? 0 : 1;

        // ask the AI what it thinks for the "image" of the pole at this angle
        double[] value = smallNN.FeedForward(input);

        // train until roughly correct
        while (true)
        {
#pragma warning disable S1244 // Floating point numbers should not be tested for equality. The expected output is 0 or 1, so this is safe. It's a double, because nn's use doubles.
            if ((expectedOutput == 0 && value[0] <= c_minAcceptableValueFor0) || (expectedOutput == 1 && value[0] >= c_minAcceptableValueFor1))
            {
                break;
            }
#pragma warning restore S1244 // Floating point numbers should not be tested for equality

            // it gave the wrong answer, so we need to train it.

            smallNN.BackPropagate([expectedOutput]);

            // after one back propagation, does it NOW give the right answer. If so, it exists the loop/
            value = smallNN.FeedForward(input);
        }
    }

    /// <summary>
    /// User changed angle of pole, so update the display with what the neural network.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NudAngle_ValueChanged(object sender, EventArgs e)
    {
        float angleDegrees = (float)numericUpDown1.Value;

        // show the pole for this angle. If it hasn't got one, it will create one
        Bitmap poleAtAngleImage = AngleImageInputCache.GetImage(angleDegrees);
        pictureBoxVideoDisplay.Image = poleAtAngleImage;

        // find out what the neural net returns
        double[] result = smallNN.FeedForward(AngleImageInputCache.GetNNInput(angleDegrees));
        labelNNresult.Text = $"\"{(result[0] < 0.5 ? 0 : 1)}\"    {result[0]:F4}";

        // show the neural network firing based on the image
        pictureBoxNNfiring.Image?.Dispose();
        pictureBoxNNfiring.Image = ImageProcessing.GetNNFiringAsBitmap(smallNN, AngleImageInputCache.c_smallWidth, AngleImageInputCache.c_smallHeight);
    }

    /// <summary>
    /// Checks the angles from -24 to 24 to see if the neural network is trained.
    /// </summary>
    /// <returns></returns>
    private bool IsTrained()
    {
        for (float angleInDegrees = -24; angleInDegrees <= 24; angleInDegrees += 0.25f)
        {
            double[] result = smallNN.FeedForward(AngleImageInputCache.GetNNInput(angleInDegrees));

            // needs to be nearly correct
            if ((angleInDegrees < 0 && result[0] > c_minAcceptableValueFor0) || (angleInDegrees >= 0 && result[0] < c_minAcceptableValueFor1))
            {
                // not trained, so train it for this angle
                TrainForAngle(angleInDegrees);
                return false; // failed training. Reality: after above backprop, it might be trained. But we'll asume not.
            }
        }

        return true; // trained
    }

    /// <summary>
    /// Trains the neural network to associate the pictures of the pole at different angles with left (<0.5) or right (>=0.5).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonTrain_Click(object sender, EventArgs e)
    {
        buttonTrain.Enabled = false; // make sure it's not pressed again
        Application.DoEvents(); // ensure screen paints

        ShowTrainingUpdate("Training -24 to 24...");

        // first we give it a sweep of angles to get it started.
        // We train for -24, then -23.75, -23.5, ... 24. This gives it one train with full sweep.
        for (float angleInDegrees = -24; angleInDegrees <= 24; angleInDegrees += 0.25f)
        {
            TrainForAngle(angleInDegrees);
        }

        ShowTrainingUpdate("Random Training...");

        int epoch = 0;

        // we repeat until trained
        while (!IsTrained())
        {
            // pick a random start angle
            float angleInDegrees = (float)(Utils.GetRandomValuePlusMinus1() * 24f);

            // we the add random amounts increasing the angle (keeping the sign).
            // to ensure it doesn't train one sided, we flip the sign each time.
            for (int i = 0; i < 100; i++)
            {
                angleInDegrees = -angleInDegrees; // flip sign

                angleInDegrees += (float)Math.Abs(Utils.GetRandomValuePlusMinus1() * 2) * Math.Sign(angleInDegrees); // add random amount in same direction

                if (angleInDegrees < -24 || angleInDegrees > 24) break; // don't go out of range, as pole game ends if exceeded.

                TrainForAngle(angleInDegrees);
            }

            ++epoch;

            // it could take a while, so this is a good time to show progress
            if (epoch % 100 == 0)
            {
                ShowTrainingUpdate($"Random Training #{epoch}");

                ShowNNOutput();
            }
        }

        labelStateOfNN.Text = $"Trained in {epoch} epoch{((epoch != 1) ? "s" : "")}";

        // restore pole to match the angle input
        NudAngle_ValueChanged(sender, e);

        _isTrained = true;

        ShowNNOutput();

        // notify use
        MessageBox.Show("Training complete");

        buttonTrain.Enabled = true;
    }

    /// <summary>
    /// Shows a message such as training in progress.
    /// </summary>
    /// <param name="message"></param>
    private void ShowTrainingUpdate(string message)
    {
        labelStateOfNN.Text = message;
        Application.DoEvents();
    }

    /// <summary>
    /// Shows what the NN responds for each angle in a listbox.
    /// If training, we show errors.
    /// If trained, we show all angles.
    /// </summary>
    private void ShowNNOutput()
    {
        // we're outputting new values
        listBoxNNnegativeAngleOutput.Items.Clear();
        listBoxNNpositiveAngleOutput.Items.Clear();

        // show the output for each angle, separated by sign
        for (float angle = -24; angle <= 24; angle += 0.25f)
        {
            double[] result = smallNN.FeedForward(AngleImageInputCache.GetNNInput(angle));
            string outputText = $"{angle:F2} = {(result[0] < 0.5 ? 0 : 1)} raw: {result[0]:F4}";

            // add "error" if it's not correct
            if (angle < 0 && result[0] > c_minAcceptableValueFor0) outputText += " error";
            if (angle >= 0 && result[0] < c_minAcceptableValueFor1) outputText += " error";

            // add to list boxes if it's trained, or if it's an error. For training we show just those in error.
            if (_isTrained || outputText.Contains("error"))
            {
                // because there are positive and negative angles, I chose to separate them (uses more screen space, less scroll)
                if (angle < 0)
                    listBoxNNnegativeAngleOutput.Items.Add(outputText);
                else // 0 or positive
                    listBoxNNpositiveAngleOutput.Items.Add(outputText);
            }
        }

        // neural network images must be disposed of by us.
        pictureBoxNNmap.Image?.Dispose();
        pictureBoxNNmap.Image = ImageProcessing.GetNNAsBitmap(smallNN, AngleImageInputCache.c_smallWidth, AngleImageInputCache.c_smallHeight);

        Application.DoEvents(); // ensure screen paints
    }

    /// <summary>
    /// When the form loads, show the pole upright.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FormCartPoleGym_Load(object sender, EventArgs e)
    {
        NudAngle_ValueChanged(sender, e); // call it, so it shows the pole, and wrong result
        ShowNNOutput();
    }
}