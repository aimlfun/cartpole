using System.Diagnostics;
namespace Cart;

/// <summary>
/// Yes, doesn't this look off the chart level of crazy? It's not. It's a simple formula that works.
/// 
/// I derived it using a mutation neural network.
/// 
/// The simplicity part came about by partly by accident, partly stupidity on my part. I changed the weightings 
/// to non pseudo random numbers and got the divisor wrong. I then realised it worked anyway with whole numbers.
/// 
/// The formula my AI gave was 
///    direction = (Activate((0*input[0])+(1*input[1])+(1*input[2])+(1*input[3])+0));
///    
/// input 0 is the cart position, input 1 is the cart velocity, input 2 is the pole angle, input 3 is the pole angular velocity.  All inputs were /4.
/// 0 x anything = 0, so it doesn't care where the cart is. 
/// 
/// If it's negative, we go left.
/// 
/// I guess the point it realises is that you need to take an action that is opposite to the largest problem, unless the
/// other settings negate it. i.e. if the angular velocity is compensated by cart velocity / angle, no action is needed.
/// 
/// If you manage that control effectively, the cart will remain positioned near the start.
/// </summary>
internal static class AI
{
    /// <summary>
    /// Returns the raw data for the direction.
    /// </summary>
    /// <param name="state"></param>
    /// <returns>A direction in magnitude/sign: <0 = left, >=0 right.</returns>
    internal static float GetCalc(StateObject state)
    {
        Debug.Assert(state != null, "env.State != null");

        // we don't divide by 4 because it doesn't change the sign
        float rawDirection = state.CartVelocity + state.PoleAngle + state.PoleAngularVelocity;

        return rawDirection;
    }

    /// <summary>
    /// Returns a direction the cart should head, based on our "AI" calculation.
    /// </summary>
    /// <param name="state"></param>
    /// <returns>0 = left, 1 = right</returns>
    internal static int GetCorrectDirection(StateObject state)
    {
        Debug.Assert(state != null, "env.State != null");

        float rawDirection = GetCalc(state); // we separate the formula, because we display the raw value.

        return (rawDirection < 0) ? 0 : 1;
    }
}