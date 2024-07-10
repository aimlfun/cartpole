//#define withoutAngularVelocity // don't undefine this. It does not work without angular velocity.
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
    internal float wins = 0;
    internal float losses = 0;

#if drawingImage
    // size of the screen
    private const int c_screenWidth = 600;
    private const int c_screenHeight = 400;
#endif

    private const string c_kinematics_integrator = "euler";

    private readonly float _totalMass;
    private readonly float _poleMassLength;
    private readonly float thetaThresholdInRadians;

    /// <summary>
    /// The unique id of the cart.
    /// </summary>
    internal int Id { get; }

    /// <summary>
    /// This contains the neural network that controls the cart.
    /// </summary>
    internal NeuralNetwork Brain { get; private set; }

    /// <summary>
    /// Whether the cart has been terminated.
    /// </summary>
    internal bool Terminated { get; private set; }

    /// <summary>
    /// Tracks the state of the cart.
    /// </summary>
    internal StateObject State { get; private set; }

    /// <summary>
    /// Contains the total rewards the AI has received.
    /// </summary>
    internal int TotalRewards { get; private set; } = 0;

    /// <summary>
    /// Contains the "score" we attribute to the AI's perfomance (based on how well it kept the cart stable, and for how long).
    /// </summary>
    internal float Score { get; private set; } = 0;

    /// <summary>
    /// 
    /// </summary>
    internal float Age { get; set; } = 0;

    /// <summary>
    /// Constructor.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable. FALSE POSITIVE. It is set in ResetEnvironment().
    internal CartPoleEnv(int id)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        Id = id;
        _totalMass = c_massPole + c_massCart;
        _poleMassLength = c_massPole * c_length;

        // # Angle at which to fail the episode
        thetaThresholdInRadians = (float)(12 * 2 * Math.PI / 360);
        TotalRewards = 0;

        // # Angle limit set to 2 * theta_threshold_radians so failing observation
        // # is still within bounds.

        AddNewBrain();
        ResetEnvironment();
    }

    /// <summary>
    /// Assign a neural network to the cart that is the size configured in the AI settings.
    /// </summary>
    internal void AddNewBrain()
    {
#if withoutAngularVelocity
        Brain = new NeuralNetwork(Id, [2/* inputs */, 1 /* left or right */]);
#else
        Brain = new NeuralNetwork(Id, [4/* inputs */, 1 /* left or right */]);
#endif
        wins = 0;
        losses = 0;
        Age = 0;
    }

    /// <summary>
    /// Plays the cart-pole game, and assigns a score to the AI at the end.
    /// </summary>
    internal void PlayGame()
    {
        int steps = 0;
        float totVelocity = 0;
        float totPoleAngle = 0;

        while (!Terminated)
        {
            int action = GetActionFromAI(); // what does the AI want to do? left, or right?

            Step(action); // move the cart

            // track the number of steps and the total velocity, so we can work out the average speed.
            // We intentionally ignore sign of the velocity (I guess that makes it a speed, as it has no direction?.
            steps++;
            totVelocity += Math.Abs(State.CartVelocity);
            totPoleAngle += Math.Abs(State.PoleAngle);
        }

        ++Age;

        // points for steps, minus points for excessive velocity, and severely whacked for losing.
        if (steps > 500)
        {
            ++wins;
            Score = (steps * 100f - 10f * totVelocity / (float)steps) - losses * 99999999f + wins * 100000f + -5 * (totPoleAngle / (float)steps) + Age;
            if (losses > 0) losses -= 0.001f;
        }
        else
        {
            ++losses;
            Score = (steps * 1000f - 100f * totVelocity / (float)steps);
            Age = 0;
        }
    }

    /// <summary>
    /// AI chooses an action.
    /// </summary>
    /// <returns></returns>
    private int GetActionFromAI()
    {
        // We divide by 4 to ensure they are within -1..1 range.
#if withoutAngularVelocity
        double[] inputs = [State.CartVelocity / 4, State.PoleAngle / 4];
#else
        double[] inputs = [State.CartPosition, State.CartVelocity / 4, State.PoleAngle / 4, State.PoleAngularVelocity / 4];
#endif
        double[] outputFromNeuralNetwork = Brain.FeedForward(inputs); // process inputs

        // it's a left or right, 0 = left
        if (outputFromNeuralNetwork[0] < 0)
        {
            return 0;
        }

        return 1;
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
        // Termination: Cart Position is greater than ±2.4(center of the cart reaches the edge of the display)
        // Truncation: Episode length is greater than 500(200 for v0)
        Terminated =
            (x < -c_xThreshold) ||
            (x > c_xThreshold) ||
            (theta < -thetaThresholdInRadians) ||
            (theta > thetaThresholdInRadians) ||
            TotalRewards > 499; // we'll increment => 500

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
        byte[] randomBytes = new byte[4];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        int randomInt = BitConverter.ToInt32(randomBytes, 0) & Int32.MaxValue;
        float result = ((float)randomInt / Int32.MaxValue) * 0.1f - 0.05f;

        if (result < -0.0500f || result > 0.0500f)
        {
            Debugger.Break();
        }

        return result;
    }

    /// <summary>
    /// Resets the environment.
    /// </summary>
    internal void ResetEnvironment()
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
    }
}