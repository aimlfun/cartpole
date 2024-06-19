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
/// 
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

    // # seconds between state updates
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
    internal int Step(int action)
    {
        if (Terminated) return 0;

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

        Render();

        ++TotalRewards;

        // Since the goal is to keep the pole upright for as long as possible, a reward of +1 for every step taken,
        // including the termination step, is allotted. The threshold for rewards is 475 for v1.
        return 1;
    }

    /// <summary>
    /// Returns float between +/-0.05
    /// </summary>
    /// <returns></returns>
    private static float GetRandomValuePlusMinus0point05()
    {
        return ((float)RandomNumberGenerator.GetInt32(-5000, 5000)) / 100000f;
    }

    /// <summary>
    /// Resets the environment.
    /// </summary>
    internal void Reset()
    {
        // All observations are assigned a uniformly random value in `(-0.05, 0.05)`
        State = new StateObject()
        {
            CartPosition = GetRandomValuePlusMinus0point05(),
            CartVelocity = GetRandomValuePlusMinus0point05(),
            PoleAngle = GetRandomValuePlusMinus0point05(),
            PoleAngularVelocity = GetRandomValuePlusMinus0point05()
        };

        TotalRewards = 0;
        Terminated = false;

        Render();
    }

    /// <summary>
    /// Renders the cart and pole.
    /// </summary>
    private void Render()
    {
        Bitmap surf = new(c_screenWidth, c_screenHeight);
        using Graphics g = Graphics.FromImage(surf);

        float world_width = c_xThreshold * 2;
        float scale = c_screenWidth / world_width;
        float polewidth = 10.0f;
        float polelen = scale * (2 * c_length);
        float cartwidth = 50.0f;
        float cartheight = 30.0f;
        float l, r, t, b;

        if (State is null)
        {
            return;
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

        //-------------------------------

        // draw cart       
        (l, r, t, b) = (-cartwidth / 2, cartwidth / 2, cartheight / 2, -cartheight / 2);

        float axleoffset = cartheight / 4.0f;
        float cartx = State.CartPosition * scale + c_screenWidth / 2.0f; //  # MIDDLE OF CART
        float carty = 100;  //# TOP OF CART
        PointF[] cart_coords = [new PointF(l + cartx, c_screenHeight - (b + carty)), new PointF(l + cartx, c_screenHeight - (t + carty)), new PointF(r + cartx, c_screenHeight - (t + carty)), new PointF(r + cartx, c_screenHeight - (b + carty))];

        g.FillPolygon(Brushes.Black, cart_coords);

        (l, r, t, b) = (-polewidth / 2, polewidth / 2, (polelen - polewidth / 2), (-polewidth / 2));

        //-------------------------------

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

        g.DrawLine(Pens.Black, 0, c_screenHeight - carty, c_screenWidth, c_screenHeight - carty);

        using SolidBrush brush = new(Color.FromArgb(202, 152, 101));
        g.FillPolygon(brush, pole_coords);

        // draw axle circle

        using SolidBrush circleBrush = new(Color.FromArgb(129, 132, 203));

        g.FillEllipse(circleBrush, cartx - polewidth / 2, c_screenHeight - (carty + axleoffset + polewidth / 2), polewidth, polewidth);

        // write total score
        using Font font = new("Arial", 10);

        g.DrawString("Score: " + TotalRewards.ToString(), font, Brushes.Black, 10, 10);

        // update the screen
        _screen.Image?.Dispose();
        _screen.Image = surf;
    }
}