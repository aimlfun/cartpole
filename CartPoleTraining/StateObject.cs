namespace Cart;

/// <summary>
/// State object for the cart-pole game.
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
    /// Velocity at the tip of the pole.
    /// </summary>
    internal float PoleAngularVelocity;
}