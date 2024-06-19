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
    private readonly CartPoleEnv env;

    /// <summary>
    /// Direction user/ai selected using buttons, keys or brain.
    /// </summary>
    private int direction = 0;

    /// <summary>
    /// When true AI will play the game.
    /// </summary>
    private bool aiIsPlayingGame = false;

    /// <summary>
    /// Constructor.
    /// </summary>
    public FormCartPoleGym()
    {
        InitializeComponent();

        // we need to intercept keys (left/right) even before buttons
        KeyPreview = true;

        // make a cart environment
        env = new(pictureBox1);

        // inform user to start
        ShowState("<- press button to start");

        // ensure the 2 start buttons are enabled, and steering not
        SetButtons(false);
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
    /// Activate is HardTANH     1_      ___
    /// (hyperbolic tangent)     0_      |
    ///                         -1_   ___|
    ///                         
    ///                                | | |
    ///                     -infinity -1 0 1..infinity
    ///                               
    /// i.e. HardTANH flatters any value to between -1 and +1, without adding unnecessary curve to it.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static double HardTANH(double value)
    {
        if (value < -1) return -1;
        if (value > 1) return 1;
        return value;
    }

    /// <summary>
    /// When the game ends, we stop the timer and inform user.
    /// </summary>
    private void EndGame()
    {
        // stop the timer that calls env.Step() i.e. this, and tell user game is over.
        timerMove.Enabled = false;

        ShowState("game over");

        SetButtons(false);
    }

    /// <summary>
    /// This tells the cart to move left.
    /// </summary>
    private void SteerCartLeft()
    {
        // cart goes left. "-1" would be better, but that's not the way OpenAI gym works and I chose
        // to stay consistent so that the AI output is comparable.
        direction = 0;
    }

    /// <summary>
    /// This tells the cart to move to the right.
    /// </summary>
    private void SteerCartRight()
    {
        direction = 1;
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
        if (!aiIsPlayingGame)
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
        SetButtons(true);

        // reset the environment
        env.Reset();


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
        if (aiIsPlayingGame)
        {
            state = false;
        }

        buttonLeft.Enabled = state;
        buttonRight.Enabled = state;

        //  if in a game, these are disabled
        buttonAIPlay.Enabled = !steerState;
        buttonYouPlay.Enabled = !steerState;
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
            env.Reset();
        }
    }

    /// <summary>
    /// Starts a game that you can play.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonYouPlay_Click(object sender, EventArgs e)
    {
        aiIsPlayingGame = false;
        ShowState("YOU playing");

        StartGame();
    }

    /// <summary>
    /// Start the game button. User can also press [space].
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonAIPlays_Click(object sender, EventArgs e)
    {
        aiIsPlayingGame = true;

        ShowState("AI playing");

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
        if (aiIsPlayingGame) return;

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
        if (aiIsPlayingGame) return;

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
        if (env.Terminated)
        {
            return;
        }

        // we trained it. 2 nanoseconds later, here's the formula.
        if (aiIsPlayingGame)
        {
            Debug.Assert(env.State != null, "env.State != null");

            double[] input = [env.State.CartPosition, env.State.CartVelocity / 4, env.State.PoleAngle / 4, env.State.PoleAngularVelocity / 4];

            double nn = (HardTANH((0.017999999225139618 * input[0]) + (0.4300000071525574 * input[1]) + (0.3140000104904175 * input[2]) + (0.39100000262260437 * input[3]) + 0));

            direction = (nn < 0) ? 0 : 1;
        }

        // The cart will by default move in the last direction.
        // User can influence using left/right keys...

        // move based on input
        env.Step(direction);

        // is game over as a result of last step?
        if (env.Terminated)
        {
            EndGame();
        }
    }
    #endregion
}