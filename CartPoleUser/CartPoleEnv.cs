using System.Diagnostics;
using System.Security.Cryptography;

/*
	Classic cart-pole system implemented by Rich Sutton et al.
	Copied from http://incompleteideas.net/sutton/book/code/pole.c
	permalink: https://perma.cc/C9ZM-652R
*/

/*
    See: https://www.gymlibrary.dev/environments/classic_control/cart_pole/

    ### Description

    This environment corresponds to the version of the cart-pole problem described by Barto, Sutton, and Anderson in
    ["Neuronlike Adaptive Elements That Can Solve Difficult Learning Control Problem"](https://ieeexplore.ieee.org/document/6313077).

    A pole is attached by an un-actuated joint to a cart, which moves along a frictionless track.
    The pendulum is placed upright on the cart and the goal is to balance the pole by applying forces
    in the left and right direction on the cart.


    ### Observation Space

    The observation is a `ndarray` with shape `(4,)` with the values corresponding to the following positions and velocities:

    | Num | Observation           | Min                 | Max               |
    |-----|-----------------------|---------------------|-------------------|
    | 0   | Cart Position         | -4.8                | 4.8               |
    | 1   | Cart Velocity         | -Inf                | Inf               |
    | 2   | Pole Angle            | ~ -0.418 rad (-24°) | ~ 0.418 rad (24°) |
    | 3   | Pole Angular Velocity | -Inf                | Inf               |

    **Note:** While the ranges above denote the possible values for observation space of each element,
        it is not reflective of the allowed values of the state space in an unterminated episode. Particularly:
    -  The cart x-position (index 0) can be take values between `(-4.8, 4.8)`, but the episode terminates
       if the cart leaves the `(-2.4, 2.4)` range.
    -  The pole angle can be observed between  `(-.418, .418)` radians (or **±24°**), but the episode terminates
       if the pole angle is not in the range `(-.2095, .2095)` (or **±12°**)

    ### Rewards

    Since the goal is to keep the pole upright for as long as possible, a reward of `+1` for every step taken,
    including the termination step, is allotted. The threshold for rewards is 475 for v1.

    ### Starting State

    All observations are assigned a uniformly random value in `(-0.05, 0.05)`

    ### Episode End

    The episode ends if any one of the following occurs:

    1. Termination: Pole Angle is greater than ±12°
    2. Termination: Cart Position is greater than ±2.4 (center of the cart reaches the edge of the display)
    3. Truncation: Episode length is greater than 500 (200 for v0)
    
*/

namespace Cart;

/// <summary>
/// Represents the cart-pole environment,
/// </summary>
internal class CartPoleEnv
{
    private const float c_gravity = 9.8f;
    private const float c_massCart = 1.0f;
    private const float c_massPole = 0.1f;

    //  # actually half the pole's length
    private const float c_length = 0.5f;
    private const float c_forceMag = 10.0f;
    private const float c_xThreshold = 2.4f;

    // # seconds between state updates (50fps)
    private const float c_tau = 0.02f;

    // size of the screen
    private const int c_screenWidth = 600;
    private const int c_screenHeight = 400;

    private const string c_kinematics_integrator = "euler";

    private readonly float _totalMass;
    private readonly float _poleMassLength;
    private readonly float thetaThresholdInRadians;

    /// <summary>
    /// Where it renders the environment.
    /// </summary>
    private readonly PictureBox _screen;

    /// <summary>
    /// Game is terminated? (i.e. pole fell or cart went off screen, or 501 steps reached)
    /// </summary>
    internal bool Terminated { get; private set; }

    /// <summary>
    /// Tracks the state of the cart and pole.
    /// </summary>
    internal StateObject? State { get; private set; } = null;

    /// <summary>
    /// How many points this cart has received.
    /// </summary>
    internal int TotalRewards { get; private set; } = 0;

    // to track the max / in of the parameters the cart controls

    float mincartX = float.MaxValue, maxcartX = float.MinValue;
    float maxPoleAngle = float.MinValue, minPoleAngle = float.MaxValue;
    float maxPoleVelocity = float.MinValue, minPoleVelocity = float.MaxValue;
    float maxCartVelocity = float.MinValue, minCartVelocity = float.MaxValue;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="screenToPaintOn"></param>
    internal CartPoleEnv(PictureBox screenToPaintOn)
    {
        _screen = screenToPaintOn;

        _totalMass = c_massPole + c_massCart;
        _poleMassLength = c_massPole * c_length;

        // # Angle at which to fail the episode
        thetaThresholdInRadians = (float)(12 * 2 * Math.PI / 360);
        TotalRewards = 0;

        // # Angle limit set to 2 * theta_threshold_radians so failing observation
        // # is still within bounds.

        Reset();
    }

    /// <summary>
    /// Moves the cart left or right. You are not allowed to refuse to move it.
    /// ### Action Space
    /// The velocity that is reduced or increased by the applied force is not fixed and it depends on the angle
    /// the pole is pointing. The center of gravity of the pole varies the amount of energy needed to move the cart underneath it.
    /// </summary>
    /// <param name="action">0 = left, 1 = right</param>
    /// <returns>reward (1=alive, 0=terminated)</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal Bitmap? Step(int action)
    {
        if (Terminated) return null;

        // |Num  | Action                 |
        // |-----| -----------------------|
        // |  0  | Push cart to the left  |
        // |  1  | Push cart to the right |

        if (action is not 0 && action is not 1)
        {
            throw new ArgumentOutOfRangeException(nameof(action), "Invalid action");
        }

        if (State is null)
        {
            throw new NullReferenceException("State is null");
        }

        float x = State.CartPosition;
        float x_dot = State.CartVelocity;
        float theta = State.PoleAngle;
        float theta_dot = State.PoleAngularVelocity;

        float force = action == 1 ? c_forceMag : -c_forceMag;

        float costheta = (float)Math.Cos(theta);
        float sintheta = (float)Math.Sin(theta);

        // # For the interested reader:
        // # https://coneural.org/florian/papers/05_cart_pole.pdf

        float temp = (force + _poleMassLength * theta_dot * theta_dot * sintheta) / _totalMass;

        float thetaacc = (c_gravity * sintheta - costheta * temp) / (c_length * (4.0f / 3.0f - c_massPole * (costheta * costheta) / _totalMass));

        float xacc = temp - _poleMassLength * thetaacc * costheta / _totalMass;

        if (c_kinematics_integrator == "euler")
        {
            x += c_tau * x_dot;
            x_dot += c_tau * xacc;
            theta += c_tau * theta_dot;
            theta_dot += c_tau * thetaacc;
        }
        else
        {
            // # semi-implicit euler
            x_dot += c_tau * xacc;
            x += c_tau * x_dot;
            theta_dot += c_tau * thetaacc;
            theta += c_tau * theta_dot;
        }

        State = new StateObject
        {
            CartPosition = x,
            CartVelocity = x_dot,
            PoleAngle = theta,
            PoleAngularVelocity = theta_dot
        };

        // Termination: Pole Angle is greater than ±12°
        // Termination: Cart Position is greater than ±2.4 (center of the cart reaches the edge of the display)
        // Truncation: Episode length is greater than 500 (200 for v0)
        Terminated =
            (x < -c_xThreshold) ||
            (x > c_xThreshold) ||
            (theta < -thetaThresholdInRadians) ||
            (theta > thetaThresholdInRadians) ||
            TotalRewards > 499; // we'll increment => 500


        // Since the goal is to keep the pole upright for as long as possible, a reward of +1 for every step taken,
        // including the termination step, is allotted. The threshold for rewards is 475 for v1.
        ++TotalRewards; // we add before render, as render shows state of play, and should include reward

        Bitmap? videoDisplay = Render();

        return videoDisplay;
    }

    /// <summary>
    /// Returns float between +/-0.05
    /// </summary>
    /// <returns></returns>
    internal static float GetRandomValuePlusMinus0point05()
    {
        byte[] randomBytes = new byte[8];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        ulong randomInt = BitConverter.ToUInt64(randomBytes, 0);

        // large int between  0 and Uint64.MaxValue turned into a float 0..1 , then scaled to -0.05..0.05
        float result = ((float)randomInt / (float)UInt64.MaxValue) * 0.1f - 0.05f;

        return result;
    }

    /// <summary>
    /// Resets the environment.
    /// </summary>
    internal Bitmap? Reset()
    {
        // All observations are assigned a uniformly random value in `(-0.05, 0.05)`
        State = new StateObject()
        {
            CartPosition = GetRandomValuePlusMinus0point05(),
            CartVelocity = GetRandomValuePlusMinus0point05(),
            PoleAngle = GetRandomValuePlusMinus0point05(),
            PoleAngularVelocity =  GetRandomValuePlusMinus0point05()
        };

        if (State.InitIsOutOfBounds())
        {
            throw new InvalidOperationException("Initial state is out of bounds");
        }

        TotalRewards = 0;
        Terminated = false;

        return Render();
    }

    /// <summary>
    /// Renders the cart and pole.
    /// </summary>
    private Bitmap? Render()
    {
        Bitmap surf = new(c_screenWidth, c_screenHeight);

        using Graphics g = Graphics.FromImage(surf);

        float world_width = c_xThreshold * 2;
        float scale = c_screenWidth / world_width;
        float polewidth = 10.0f;
        float polelen = scale * (2 * c_length);
        float cartwidth = 50.0f;
        float cartheight = 30.0f;

        if (State is null)
        {
            return null;
        }

        /*
            UI looks like this...
                          ___
                          | |
                          | |
                          | |   < pole
                          | |
                          | |
                      ____| |____
           ___________|   |o|   |____________
                      |_________| 

                        ^ cart
         
        */
        g.Clear(Color.White);
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        
        // dotted vertical line
        using Pen penVerticalCentreLine = new(Color.FromArgb(255, 200, 200, 200));
        penVerticalCentreLine.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
        g.DrawLine(penVerticalCentreLine, c_screenWidth / 2.0f, 0, c_screenWidth / 2.0f, c_screenHeight);

        DrawCart(g, scale, polewidth, polelen, cartwidth, cartheight, out float l, out float r, out float t, out float b, out float axleoffset, out float cartx, out float carty);

        // horizontal line through the cart, drawn before the pole
        g.DrawLine(Pens.Black, 0, c_screenHeight - carty, c_screenWidth, c_screenHeight - carty);

        DrawPole(g, l, r, t, b, axleoffset, cartx, carty);
        DrawAxle(g, polewidth, axleoffset, cartx, carty);

        int action = AI.GetCorrectDirection(State);
        WriteTelemetry(g, cartx);

        // show the direction the cart steering request is for.
        DrawArrowShowingDirectionOfAction(action, g, cartx, carty);
        DrawLineShowingMinMaxCartMovement(g, carty);

        // update the screen
        _screen.Image = surf;
        return surf;
    }

    /// <summary>
    /// Writes the score / angle / speed etc to the image.
    /// </summary>
    /// <param name="g"></param>
    /// <param name="cartx"></param>
    private void WriteTelemetry(Graphics g, float cartx)
    {
        Debug.Assert(State != null, "State != null");
        // write total score
        using Font font = new("Courier New", 10);

        MinMax(ref minPoleVelocity, ref maxPoleVelocity, State.PoleAngularVelocity);
        MinMax(ref mincartX, ref maxcartX, cartx);
        MinMax(ref minPoleAngle, ref maxPoleAngle, State.PoleAngle);
        MinMax(ref minCartVelocity, ref maxCartVelocity, State.CartVelocity);

        g.DrawString(TotalRewards == 0 ? "initial state" : $"Rewards: {TotalRewards}", font, Brushes.Black, 10, 5);
        g.DrawString($"Cart velocity: {NumberWithPlusMinus4Dp(State.CartVelocity)}  min: {NumberWithPlusMinus4Dp(minCartVelocity)}  max: {NumberWithPlusMinus4Dp(maxCartVelocity)}", font, Brushes.Black, 10, 20);
        g.DrawString($"Pole angle:    {NumberWithPlusMinus4Dp(State.PoleAngle)}  min: {NumberWithPlusMinus4Dp(minPoleAngle)}  max: {NumberWithPlusMinus4Dp(maxPoleAngle)}", font, Brushes.Black, 10, 35);
        g.DrawString($"Pole velocity: {NumberWithPlusMinus4Dp(State.PoleAngularVelocity)}  min: {NumberWithPlusMinus4Dp(minPoleVelocity)}  max: {NumberWithPlusMinus4Dp(maxPoleVelocity)}", font, Brushes.Black, 10, 50);
        g.DrawString($"Last action calc: {NumberWithPlusMinus4Dp(AI.GetCalc(State))}", font, Brushes.Black, 380, 5); // what the "AI" is thinking
    }

    /// <summary>
    /// Output the number to 4 decimal places, with a + or - sign.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    private static string NumberWithPlusMinus4Dp(float number)
    {
        return number.ToString("+#0.0000;-#0.0000");
    }

    /// <summary>
    /// Draws the circular axle between cart and pole.
    /// </summary>
    /// <param name="g"></param>
    /// <param name="polewidth"></param>
    /// <param name="axleoffset"></param>
    /// <param name="cartx"></param>
    /// <param name="carty"></param>
    private static void DrawAxle(Graphics g, float polewidth, float axleoffset, float cartx, float carty)
    {
        // draw axle circle

        using SolidBrush circleBrush = new(Color.FromArgb(129, 132, 203));

        g.FillEllipse(circleBrush, cartx - polewidth / 2, c_screenHeight - (carty + axleoffset + polewidth / 2), polewidth, polewidth);
    }

    /// <summary>
    /// Draws the cart.
    /// </summary>
    /// <param name="g"></param>
    /// <param name="scale"></param>
    /// <param name="polewidth"></param>
    /// <param name="polelen"></param>
    /// <param name="cartwidth"></param>
    /// <param name="cartheight"></param>
    /// <param name="l"></param>
    /// <param name="r"></param>
    /// <param name="t"></param>
    /// <param name="b"></param>
    /// <param name="axleoffset"></param>
    /// <param name="cartx"></param>
    /// <param name="carty"></param>
    private void DrawCart(Graphics g, float scale, float polewidth, float polelen, float cartwidth, float cartheight, out float l, out float r, out float t, out float b, out float axleoffset, out float cartx, out float carty)
    {
        Debug.Assert(State != null, "State != null");
        
        // draw cart
        (l, r, t, b) = (-cartwidth / 2, cartwidth / 2, cartheight / 2, -cartheight / 2);

        axleoffset = cartheight / 4.0f;
        cartx = State.CartPosition * scale + c_screenWidth / 2.0f;
        carty = 100;
        PointF[] cart_coords = [new PointF(l + cartx, c_screenHeight - (b + carty)), new PointF(l + cartx, c_screenHeight - (t + carty)), new PointF(r + cartx, c_screenHeight - (t + carty)), new PointF(r + cartx, c_screenHeight - (b + carty))];

        g.FillPolygon(Brushes.Black, cart_coords);

        (l, r, t, b) = (-polewidth / 2, polewidth / 2, (polelen - polewidth / 2), (-polewidth / 2));
    }

    /// <summary>
    /// Draw the pole rotated to reflect the amount it is leaning.
    /// </summary>
    /// <param name="g"></param>
    /// <param name="l"></param>
    /// <param name="r"></param>
    /// <param name="t"></param>
    /// <param name="b"></param>
    /// <param name="axleoffset"></param>
    /// <param name="cartx"></param>
    /// <param name="carty"></param>
    private void DrawPole(Graphics g, float l, float r, float t, float b, float axleoffset, float cartx, float carty)
    {
        Debug.Assert(State != null, "State != null");
        // draw pole

        PointF[] pole_coords = [new PointF(l, b), new PointF(l, t), new PointF(r, t), new PointF(r, b)];

        // the pole is rotate to reflect the amount it is leaning
        for (int i = 0; i < pole_coords.Length; i++)
        {
            double angle = State.PoleAngle;

            // we need to rotate using sin/cos because we are rotating around the origin
            float vx = pole_coords[i].X * (float)Math.Cos(angle) - pole_coords[i].Y * (float)Math.Sin(angle);
            float vy = pole_coords[i].X * (float)Math.Sin(angle) + pole_coords[i].Y * (float)Math.Cos(angle);

            // we add the axleoffset to the y value because the pole is attached to the cart, and after rotation
            pole_coords[i] = new PointF(vx + cartx, c_screenHeight - (vy + carty + axleoffset));
        }

        using SolidBrush brush = new(Color.FromArgb(202, 152, 101));
        g.FillPolygon(brush, pole_coords);
    }

    /// <summary>
    /// Draws a dotted line under the cart indicating how much the car has moved left or right.
    /// </summary>
    /// <param name="g"></param>
    /// <param name="carty"></param>
    private void DrawLineShowingMinMaxCartMovement(Graphics g, float carty)
    {
        using Pen pen = new(Color.FromArgb(255, 0, 0, 0));

        pen.StartCap = System.Drawing.Drawing2D.LineCap.DiamondAnchor;
        pen.EndCap = System.Drawing.Drawing2D.LineCap.DiamondAnchor;
        pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

        g.DrawLine(pen, mincartX, c_screenHeight - (carty - 18), maxcartX, c_screenHeight - (carty - 18));
    }

    /// <summary>
    /// Underneath the cart, we draw an arrow to show the direction the cart is being steered.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="g"></param>
    /// <param name="cartx"></param>
    /// <param name="carty"></param>
    private static void DrawArrowShowingDirectionOfAction(int action, Graphics g, float cartx, float carty)
    {
        PointF[] arrowpoints;

        float arrowy = carty - 10;
        int direction = action == 0 ? -1 : 1;


        //      |             |
        //      +------|------+
        //             ^ (cartx, c_screenHeight - (cartHeight/2 + carty))

        //         /|
        //        / '-+     <- draw these
        //        \ .-+  
        //         \|

        // arrow
        arrowpoints = [
                    new PointF(cartx, c_screenHeight - (arrowy-20)),
                        new PointF(cartx+direction*10, c_screenHeight - (arrowy-20)),
                        new PointF(cartx+direction*10, c_screenHeight - (arrowy-10)),
                        new PointF(cartx+direction*20, c_screenHeight - (arrowy-25)),
                        new PointF(cartx+direction*10, c_screenHeight - (arrowy-40)),
                        new PointF(cartx+direction*10, c_screenHeight - (arrowy-30)),
                        new PointF(cartx, c_screenHeight - (arrowy-30)),
                        new PointF(cartx, c_screenHeight - (arrowy-20))
                    ];

        g.FillPolygon(Brushes.Black, arrowpoints);
    }

    /// <summary>
    /// Sets min/max values.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="value"></param>
    static void MinMax(ref float min, ref float max, float value)
    {
        if (value < min)
        {
            min = value;
        }

        if (value > max)
        {
            max = value;
        }
    }
}