using System.Diagnostics;

namespace Cart;

/// <summary>
/// UI for CartPole gym.
/// </summary>
public partial class FormCartPoleGym : Form
{
    /// <summary>
    /// Our simulation environment
    /// </summary>
    private readonly CartPoleEnv _env;

    /// <summary>
    /// Direction user/ai selected using buttons, keys or brain.
    /// </summary>
    private int _direction = 0;

    /// <summary>
    /// When true AI will play the game.
    /// </summary>
    private bool _aiIsPlayingGame = false;

    /// <summary>
    /// Manages frames/
    /// </summary>
    private readonly FrameManager _frameManager = new();

    /// <summary>
    /// The frame we are currently on
    /// </summary>
    private int currentFrame = -1;

    /// <summary>
    /// Constructor.
    /// </summary>
    public FormCartPoleGym()
    {
        InitializeComponent();

        // we need to intercept keys (left/right) even before buttons
        KeyPreview = true;

        // make a cart environment
        _env = new(pictureBoxCartEnvironmentDisplay);

        // inform user to start, they can also let the AI play.
        ShowState("Press \"Play\" button to start.");

        // ensure the 2 start buttons are enabled, and steering not
        SetButtons(false);

        // we display the frames in the right hand side, and diff, this enables us to react to changes.
        _frameManager.OnFrameAdded += FrameManager_OnFrameAdded;
        _frameManager.OnReset += FrameManager_OnReset;
    }

    /// <summary>
    /// start > playing > game over
    /// </summary>
    /// <param name="text"></param>
    private void ShowState(string text)
    {
        labelState.Text = text;
    }

    /// <summary>
    /// When the game ends, we stop the timer and inform user.
    /// </summary>
    private void EndGame()
    {
        // stop the timer that calls env.Step() i.e. this, and tell user game is over.
        timerMove.Enabled = false;

        ShowState("G A M E   O V E R");

        SetButtons(false);
    }

    /// <summary>
    /// This tells the cart to move left.
    /// </summary>
    private void SteerCartLeft()
    {
        // cart goes left. "-1" would be better, but that's not the way OpenAI gym works and I chose
        // to stay consistent so that the AI output is comparable.
        _direction = 0;
    }

    /// <summary>
    /// This tells the cart to move to the right.
    /// </summary>
    private void SteerCartRight()
    {
        _direction = 1;
    }

    /// <summary>
    /// Required to intercept [LEFT]/[RIGHT] keys.
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="keyData"></param>
    /// <returns></returns>
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // if ai is playing, we don't want to intercept keys
        if (!_aiIsPlayingGame)
        {
            switch (keyData)
            {
                case Keys.Left: // left arrow key
                    SteerCartLeft();
                    return true;

                case Keys.Right: // right arrow key
                    SteerCartRight();
                    return true;
            }
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    /// <summary>
    /// Starts the game.
    /// </summary>
    private void StartGame()
    {
        _frameManager.Reset();
        SetButtons(true);

        Bitmap? initialDisplay = _env.Reset();

        // reset the environment
        AddSnapshotOfScreenToRightHandSide(initialDisplay);

        // enable timer that moves the cart
        timerMove.Enabled = true;
    }

    /// <summary>
    /// Show/hide buttons based on whether AI is playing or user is playing (or not started).
    /// </summary>
    /// <param name="steerState"></param>
    private void SetButtons(bool steerState)
    {
        bool state = steerState;

        // if AI is playing, we don't want to allow user to steer
        if (_aiIsPlayingGame)
        {
            state = false;
        }

        // disabled if ai is playing, as it is steering
        buttonLeft.Enabled = state;
        buttonRight.Enabled = state;

        //  if in a game, these are disabled as the game is already started
        buttonAIPlay.Enabled = !steerState;
        buttonYouPlay.Enabled = !steerState;
    }

    /// <summary>
    /// Frame by frame, we add a snapshot of the screen to the right hand side.
    /// </summary>
    /// <param name="screenImage"></param>
    private void AddSnapshotOfScreenToRightHandSide(Bitmap? screenImage)
    {
        if (screenImage == null) return;

        // create image half the size of screenImage
        Bitmap? screenImage2 = new(screenImage, new Size(400, 267));

        _frameManager.AddFrame(screenImage2);
    }

    /// <summary>
    /// Displays the frame number and count.
    /// </summary>
    private void ShowFrameNumber()
    {
        if (currentFrame < 0)
        {
            labelFrameByFrame.Text = "no frames available.";
        }
        else
        {
            labelFrameByFrame.Text = $"Frame {currentFrame + 1} of {_frameManager.FrameCount}";
        }
    }

    /// <summary>
    /// Shows the selected frame, along with a diff image at the bottom.
    /// </summary>
    private void ShowFrame()
    {
        ShowFrameNumber();

        if (currentFrame < 0 || currentFrame >= _frameManager.FrameCount)
        {
            return;
        }

        pictureBoxFrame.Image = _frameManager.GetFrame(currentFrame);

        pictureBoxFrameDiff.Image?.Dispose();
        pictureBoxFrameDiff.Image = _frameManager.DiffOfFrames(currentFrame);
    }

    #region EVENT HANDLERS
    /// <summary>
    /// [Space] starts the game.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Form1_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyValue == (int)Keys.Space)
        {
            _env.Reset();
        }
    }

    /// <summary>
    /// Starts a game that you can play.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonYouPlay_Click(object sender, EventArgs e)
    {
        _aiIsPlayingGame = false;
        ShowState("Game in progress. Good luck!");

        StartGame();
    }

    /// <summary>
    /// Start the game button. User can also press [space].
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonAIPlays_Click(object sender, EventArgs e)
    {
        _aiIsPlayingGame = true;

        ShowState("Game in progress. AI is playing, no luck required.");

        StartGame();
    }

    /// <summary>
    /// Set the direction flag to left. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonLeft_Click(object sender, EventArgs e)
    {
        // if ai is playing, we don't want to allow user to steer
        if (_aiIsPlayingGame) return;

        SteerCartLeft();
    }

    /// <summary>
    /// Set the direction flag to right.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonRight_Click(object sender, EventArgs e)
    {
        // if ai is playing, we don't want to allow user to steer
        if (_aiIsPlayingGame) return;

        SteerCartRight();
    }

    /// <summary>
    /// Moves cart based on user or AI input.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimerMove_Tick(object sender, EventArgs e)
    {
        // game is terminated? if so, we can stop
        if (_env.Terminated)
        {
            return;
        }

        // we trained it. 2 nanoseconds later, here's the formula.
        if (_aiIsPlayingGame)
        {
            Debug.Assert(_env.State != null, "env.State != null");

            _direction = AI.GetCorrectDirection(_env.State);
        }

        // The cart will by default move in the last direction.
        // User can influence using left/right keys...

        // move based on control
        Bitmap? screenImage = _env.Step(_direction);

        // we add frame by frame snapshots to the right hand side
        AddSnapshotOfScreenToRightHandSide(screenImage);

        // is game over as a result of last step?
        if (_env.Terminated)
        {
            EndGame();
        }
    }

    /// <summary>
    /// Callback when the frame manager resets (new game).
    /// </summary>
    private void FrameManager_OnReset()
    {
        currentFrame = -1;
        ShowFrame();
    }

    /// <summary>
    /// Callback when a frame is added.
    /// </summary>
    /// <param name="FrameCount"></param>
    private void FrameManager_OnFrameAdded(int FrameCount)
    {
        if (FrameCount == 1)
        {
            currentFrame = 0;
            ShowFrame();
        }
        else
        {
            ShowFrameNumber();
        }
    }

    /// <summary>
    /// User clicked "<" previous frame.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonPreviousFrame_Click(object sender, EventArgs e)
    {
        if (currentFrame <= 0)
        {
            return;
        }

        --currentFrame;

        ShowFrame();
    }

    /// <summary>
    /// User clicked ">" next frame.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonNextFrame_Click(object sender, EventArgs e)
    {
        if (currentFrame >= _frameManager.FrameCount - 1)
        {
            return;
        }

        ++currentFrame;

        ShowFrame();
    }
    #endregion

    /// <summary>
    /// Make it more playable for a user, slowing it down to 5 moves per second. The original is supposed to be 50fps.
    /// At 50fps, you've lost before you even register it moving, but the AI can handle it.
    /// I base "50fps" on tau being 0.02. 1/0.02 = 50.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBoxMakeItEasier_CheckedChanged(object sender, EventArgs e)
    {
        if (checkBoxMakeItEasier.Checked)
        {
            timerMove.Interval = 200; // 5 moves per second
        }
        else
        {
            timerMove.Interval = 20; // 50 moves per second
        }
    }    
}