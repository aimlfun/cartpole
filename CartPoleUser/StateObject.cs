namespace Cart;

/// <summary>
/// CartPole state object.
/// </summary>
internal class StateObject
{
    /// <summary>
    /// Position of the cart.
    /// </summary>
    internal float CartPosition;

    /// <summary>
    /// Velocity of the cart.
    /// </summary>
    internal float CartVelocity;

    /// <summary>
    /// Angle of the pole.
    /// </summary>
    internal float PoleAngle;

    /// <summary>
    /// Angular velocity of the pole.
    /// </summary>
    internal float PoleAngularVelocity;

    /// <summary>
    /// Sanity check to ensure we don't start with a state that is out of bounds.
    /// It shouldn't happen, if our random number generator is correct.
    /// </summary>
    /// <returns></returns>
    internal bool InitIsOutOfBounds()
    {
        if (CartVelocity < -0.05f || CartVelocity > 0.05 || CartPosition < -0.05f || CartPosition > 0.05f || PoleAngle < -0.05f || PoleAngle > 0.05f)
            return true;

        return false;
    }
}