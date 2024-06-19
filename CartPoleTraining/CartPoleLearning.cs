namespace Cart;

/// <summary>
/// A class to AI to play the cart-pole game.
/// </summary>
internal static class CartPoleLearning
{
    /// <summary>
    /// How many random carts we throw in during the mutate phase, as a percentage of the total number of carts.
    /// </summary>
    private const int c_percentageRandomDuringMutate = 10;

    /// <summary>
    /// The number of parallel AI carts we run during the simulation. It runs in parallel to improve performance, so this can be set quite high (at least on my i7!).
    /// </summary>
    private const int c_numberOfAICarts = 5000;

    /// <summary>
    /// The number generations we have run the simulation for (so far).
    /// </summary>
    private static int s_numberOfGenerations = 0;

    /// <summary>
    /// We use this to determine when to call out a better score.
    /// </summary>
    private static int s_lastScore = -int.MaxValue;

    /// <summary>
    /// The "id" of the best performing cart.
    /// </summary>
    private static int s_bestAICartIndex = -1;

    /// <summary>
    /// The cart + environment, keyed by their id.
    /// </summary>
    private static Dictionary<int, CartPoleEnv> s_cartPole = [];

    /// <summary>
    /// Creates the cat, each one has it's own neural network.
    /// </summary>
    private static void CreateCarts()
    {
        s_cartPole.Clear();

        // create the required number of cart + their simulated environment

        for (int i = 0; i < c_numberOfAICarts; i++)
        {
            s_cartPole.Add(i, new CartPoleEnv(i));
        }
    }

    /// <summary>
    /// This puts the carts back to their starting position ready for next game
    /// </summary>
    private static void ResetAllCarts()
    {
        foreach (int cartIndex in s_cartPole.Keys)
        {
            s_cartPole[cartIndex].ResetEnvironment();
        }
    }

    /// <summary>
    /// Mutates the carts. i.e. creates a new generation of carts based off the best performing cartss.
    /// </summary>
    private static void MutateCartAIs()
    {
        // sort them in order of score (ascending). Highest score = best. So we replace top half with a clone the bottom half mutated.
        s_cartPole = s_cartPole.OrderBy(x => x.Value.Score).ToDictionary(x => x.Key, x => x.Value);

        CartPoleEnv[] arrayOfCarts = [.. s_cartPole.Values];

        // the best performing carts is the last one in the array
        s_bestAICartIndex = arrayOfCarts[c_numberOfAICarts - 1].Id;

        // replace the 50% worse offenders with the best, then mutate them.
        // we do this by overwriting the top half (lowest fitness) with bottom half.
        for (int worstNeuralNetworkIndex = 0; worstNeuralNetworkIndex < c_numberOfAICarts / 2; worstNeuralNetworkIndex++)
        {
            // 50..100 (in 100 neural networks) are in the top performing
            int neuralNetworkToCloneFromIndex = worstNeuralNetworkIndex + c_numberOfAICarts / 2; // +50% -> top 50% 

            // replace the worst with the best
            NeuralNetwork.CopyFromTo(arrayOfCarts[neuralNetworkToCloneFromIndex].Brain, arrayOfCarts[worstNeuralNetworkIndex].Brain);

            arrayOfCarts[worstNeuralNetworkIndex].Brain.Mutate(25, 0.5F); // mutate
        }

        // throw in one random ones, just to keep things interesting. It could be a random one performs better than the best.
        // There is probably a break even point depending on size of population. For example it might get to a point where the best is so good that it's not worth mutating it, it also might work out better to keep 1 best,
        // and mutate a few clones but keep 75% random. It's all about finding the right balance.

        int numberOfRandom = c_numberOfAICarts * c_percentageRandomDuringMutate / 100;

#pragma warning disable S2583 // Conditionally executed code should be reachable. FALSE POSITIVE. This is reachable, it's just that the number of random can be 0.
        if (numberOfRandom == 0)
        {
            arrayOfCarts[0].AddNewBrain(); // if we have no random, just make one random
        }
        else
        {
            for (int i = 0; i < numberOfRandom; i++)
            {
                arrayOfCarts[i].AddNewBrain();
            }
        }
#pragma warning restore S2583 // Conditionally executed code should be reachable
    }

    /// <summary>
    /// Runs the simulation: creating cartss, running them, mutating them, and repeating. Eventually it should come up with a cart that can wins.
    /// </summary>
    internal static void RunSimulation()
    {
        Console.WriteLine("Using AI to train a cart steering");

        Console.WriteLine("");

        Console.WriteLine($"Manufacturing {c_numberOfAICarts} cart(s)...");
        CreateCarts(); // gives us some carts to work with

        Console.WriteLine("Running Simulation... ctrl-c to end");

        Console.TreatControlCAsInput = true; // stop ctrl-c from killing the app without clean shutdown

        while (true)
        {
            // check to see if user has pressed ctrl-c
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.C && key.Modifiers == ConsoleModifiers.Control)
                {
                    Console.WriteLine("** User requested termination of simulation **");
                    break;
                }
            }

            s_numberOfGenerations++;

            // run the simulation for each cart in parallel to improve performance.
            Parallel.ForEach(s_cartPole.Values, cart =>
            {
                cart.PlayGame();
            });

            // time to mutate all but the best and try again.
            MutateCartAIs();

            // output some stats to the console. We do it only when the score improves - to keep the console from getting too cluttered, and to keep the performance up.
            if (s_bestAICartIndex > -1 && s_lastScore != s_cartPole[s_bestAICartIndex].Score)
            {
                s_lastScore = s_cartPole[s_bestAICartIndex].Score;

                Console.WriteLine($"Epoch: {s_numberOfGenerations} | Score: {s_cartPole[s_bestAICartIndex].Score} (higher is better) | Rewards: {s_cartPole[s_bestAICartIndex].TotalRewards}");
                Console.WriteLine("C# code for cart's brain:");
                Console.WriteLine(s_cartPole[s_bestAICartIndex].Brain.Formula());
                Console.WriteLine("");
            }

            ResetAllCarts();
        }
    }
}