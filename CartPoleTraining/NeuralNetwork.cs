// I discovered by a silly mistake that a winning formula does not need floating point weights / biases.
// (My mistake was that the random function I created was returning numbers that were >1.)

// When defined, the random weights will exceed 1. Because hardTan treats >1 as 1, and <-1 as -1, weights in excess are sort of redundant if the network is input connected to output
// as nothing can scale them. It equates to sum( i[0] x weight0 + i[1] x weight1 + i[2] x weight2 + i[3] x weight3 + bias) = output. If weights exceed 1, then the bias is semi-redundant.
// After running it over 9,000 games the simple formula, shows it doesn't care about cart position (0x), and the bias is zero. Even more funny is the 1x multiplier.

//      Epoch: 9423 | Score: 942359550(higher is better) | Rewards: 501 | WIN: 9423 | LOSE: 0.00 | AGE: 9423
//      direction = (Activate((0 * input[0]) + (1 * input[1]) + (1 * input[2]) + (1 * input[3]) + 0))
//
// And that means:
//      int direction = (State.CartVelocity + State.PoleAngle + State.PoleAngularVelocity ) / 4 < 0 ? 0 : 1 // direction 0 = left, 1 = right.
// But divide everything by 4 has no impact on +/-, so...
//      int direction = State.CartVelocity + State.PoleAngle + State.PoleAngularVelocity < 0 ? 0 : 1 // direction 0 = left, 1 = right.
//
// PROVE ME WRONG. :)

#define brillianceByDumbMistake

using System.Security.Cryptography;
using System.Text;

namespace Cart;

/// <summary>
/// Implementation of a feedforward neural network.
/// </summary>
public class NeuralNetwork
{
    /// <summary>
    /// The "id" (index) of the brain, should also align to the "id" of the cart it is attached.
    /// </summary>
    internal int Id;

    /// <summary>
    /// How many layers of neurons (3+). Do not do 2 or 1.
    /// 2 => input connected to output.
    /// 1 => input is output, and feed forward will crash.
    /// </summary>
    internal readonly int[] Layers;

    /// <summary>
    /// The neurons.
    /// [layer][neuron]
    /// </summary>
    internal double[][] Neurons;

    /// <summary>
    /// NN Biases. Either improves or lowers the chance of this neuron fully firing.
    /// [layer][neuron]
    /// </summary>
    internal double[][] Biases;

    /// <summary>
    /// NN weights. Reduces or amplifies the output for the relationship between neurons in each layer
    /// [layer][neuron][neuron]
    /// </summary>
    internal double[][][] Weights;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="_id">Unique ID of the neuron.</param>
    /// <param name="layerDefinition">Defines size of the layers.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Init*() set the fields.
    internal NeuralNetwork(int _id, int[] layerDefinition)
#pragma warning restore CS8618
    {
        // (1) INPUT (2) HIDDEN (3) OUTPUT. Less than 3 would be INPUT->OUTPUT; hardly "AI" but actually works
        if (layerDefinition.Length < 2) throw new ArgumentException("layer definition must be >1", nameof(layerDefinition));

        Id = _id; // used to reference this network

        // copy layerDefinition to Layers
        Layers = new int[layerDefinition.Length];

        for (int layer = 0; layer < layerDefinition.Length; layer++)
        {
            Layers[layer] = layerDefinition[layer];
        }

        // if layerDefinition is [2,3,2] then...
        // 
        // Neurons :      (o) (o)    <-2  INPUT
        //              (o) (o) (o)  <-3
        //                (o) (o)    <-2  OUTPUT
        //

        InitialiseNeurons();
        InitialiseBiases();
        InitialiseWeights();
    }

    /// <summary>
    /// Create empty storage array for the neurons in the network.
    /// </summary>
    private void InitialiseNeurons()
    {
        List<double[]> neuronsList = [];

        // if layerDefinition is [2,3,2] ..   float[]
        // Neurons :      (o) (o)    <-2  ... [ 0, 0 ]
        //              (o) (o) (o)  <-3  ... [ 0, 0, 0 ]
        //                (o) (o)    <-2  ... [ 0, 0 ]
        //

        for (int layer = 0; layer < Layers.Length; layer++)
        {
            neuronsList.Add(new double[Layers[layer]]);
        }

        Neurons = [.. neuronsList];
    }

    /// <summary>
    /// Generate a random number between -0.5...+0.5.
    /// </summary>
    /// <returns></returns>
    private static float RandomFloatBetweenMinusHalfToPlusHalf()
    {
        return (float)(RandomNumberGenerator.GetInt32(-5000000, 5000000))/10000000f;
    }

    /// <summary>
    /// initializes and populates biases.
    /// </summary>
    private void InitialiseBiases()
    {
        List<double[]> biasList = [];

        // for each layer of neurons, we have to set biases.
        for (int layer = 0; layer < Layers.Length; layer++)
        {
            double[] bias = new double[Layers[layer]];

            for (int biasLayer = 0; biasLayer < Layers[layer]; biasLayer++)
            {
#if brillianceByDumbMistake
                bias[biasLayer] = 0;
#else
                bias[biasLayer] = RandomFloatBetweenMinusHalfToPlusHalf() / 10f;
#endif
            }

            biasList.Add(bias);
        }

        Biases = [.. biasList];
    }

    /// <summary>
    /// Initializes random array for the weights being held in the network.
    /// </summary>
    private void InitialiseWeights()
    {
        List<double[][]> weightsList = []; // used to construct weights, as dynamic arrays aren't supported

        for (int layer = 1; layer < Layers.Length; layer++)
        {
            List<double[]> layerWeightsList = [];

            int neuronsInPreviousLayer = Layers[layer - 1];

            for (int neuronIndexInLayer = 0; neuronIndexInLayer < Neurons[layer].Length; neuronIndexInLayer++)
            {
                double[] neuronWeights = new double[neuronsInPreviousLayer];

                for (int neuronIndexInPreviousLayer = 0; neuronIndexInPreviousLayer < neuronsInPreviousLayer; neuronIndexInPreviousLayer++)
                {
#if brillianceByDumbMistake
                    neuronWeights[neuronIndexInPreviousLayer] = (int) (RandomFloatBetweenMinusHalfToPlusHalf()*10);
#else
                    neuronWeights[neuronIndexInPreviousLayer] = (int) RandomFloatBetweenMinusHalfToPlusHalf();
#endif
                }

                layerWeightsList.Add(neuronWeights);
            }

            weightsList.Add([.. layerWeightsList]);
        }

        Weights = [.. weightsList];
    }

    /// <summary>
    /// Feed forward, inputs >==> outputs.
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    internal double[] FeedForward(double[] inputs)
    {
        // put the INPUT values into layer 0 neurons
        for (int i = 0; i < inputs.Length; i++)
        {
            Neurons[0][i] = inputs[i];
        }

        // we start on layer 1 as we are computing values from prior layers (layer 0 is inputs)
        for (int layer = 1; layer < Layers.Length; layer++)
        {
            for (int neuronIndexForLayer = 0; neuronIndexForLayer < Neurons[layer].Length; neuronIndexForLayer++)
            {
                // sum of outputs from the previous layer
                double value = 0F;

                for (int neuronIndexInPreviousLayer = 0; neuronIndexInPreviousLayer < Neurons[layer - 1].Length; neuronIndexInPreviousLayer++)
                {
                    // remember: the "weight" amplifies or reduces, so we take the output of the prior neuron and "amplify/reduce" it's output here
                    value += Weights[layer - 1][neuronIndexForLayer][neuronIndexInPreviousLayer] * Neurons[layer - 1][neuronIndexInPreviousLayer];
                }

                // any neuron fires or not based on the input. The point of a bias is to move the activation up or down.
                // e.g. the value could be 0.3, adding a bias of 0.5 takes it to 0.8. You might think why not just use the weights to achieve this
                // but remember weights are individual per prior layer neurons, the bias affects the SUM() of them.

                Neurons[layer][neuronIndexForLayer] = Activate(value + Biases[layer][neuronIndexForLayer]);
            }
        }

        return Neurons[^1]; // final* layer contains OUTPUT
    }

    /// <summary>
    /// Activate is hardTANH     1_      ___
    /// (hyperbolic tangent)     0_      |
    ///                         -1_   ___|
    ///                                | | |
    ///                     -infinity -1 0 1..infinity
    ///                               
    /// i.e. TANH flatters any value to between -1 and +1.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static double Activate(double value)
    {
        if (value < -1) return -1;
        if (value > 1) return 1;
        return value;
    }

    /// <summary>
    /// Provides a random mutation value (i.e. what is added to weights and biaseses).
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    private static float RandomMutationValue(float val)
    {
        return RandomFloatBetweenMinusHalfToPlusHalf()/10*val;
    }

    /// <summary>
    /// A simple mutation function for any genetic implementations, ensuring it DOES mutate.
    /// </summary>
    /// <param name="pctChance"></param>
    /// <param name="val"></param>
    internal void Mutate(int pctChance, float val)
    {
        bool mutated = false;

        while (!mutated) // ensure SOMETHING changes, otherwise we'll get two identical brains.
        {
            for (int layerIndex = 0; layerIndex < Biases.Length; layerIndex++)
            {
                for (int neuronIndex = 0; neuronIndex < Biases[layerIndex].Length; neuronIndex++)
                {
                    if (RandomNumberGenerator.GetInt32(0, 100) <= pctChance)
                    {
                        mutated = true;
                        Biases[layerIndex][neuronIndex] += RandomMutationValue(val);
                    }
                }
            }

            for (int layerIndex = 0; layerIndex < Weights.Length; layerIndex++)
            {
                for (int neuronIndexForLayer = 0; neuronIndexForLayer < Weights[layerIndex].Length; neuronIndexForLayer++)
                {
                    for (int neuronIndexInPreviousLayer = 0; neuronIndexInPreviousLayer < Weights[layerIndex][neuronIndexForLayer].Length; neuronIndexInPreviousLayer++)
                    {
                        if (RandomNumberGenerator.GetInt32(0, 100) <= pctChance)
                        {
                            mutated = true;
                            Weights[layerIndex][neuronIndexForLayer][neuronIndexInPreviousLayer] += RandomMutationValue(val); 
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Copies from one NN to another.
    /// </summary>
    /// <param name="neuralNetworkToCloneFrom"></param>
    /// <param name="neuralNetworkCloneTo"></param>
    internal static void CopyFromTo(NeuralNetwork neuralNetworkToCloneFrom, NeuralNetwork neuralNetworkCloneTo)
    {
        for (int layerIndex = 0; layerIndex < neuralNetworkToCloneFrom.Biases.Length; layerIndex++)
        {
            for (int neuronIndex = 0; neuronIndex < neuralNetworkToCloneFrom.Biases[layerIndex].Length; neuronIndex++)
            {
                neuralNetworkCloneTo.Biases[layerIndex][neuronIndex] = neuralNetworkToCloneFrom.Biases[layerIndex][neuronIndex];
            }
        }

        for (int layerIndex = 0; layerIndex < neuralNetworkToCloneFrom.Weights.Length; layerIndex++)
        {
            for (int neuronIndexInLayer = 0; neuronIndexInLayer < neuralNetworkToCloneFrom.Weights[layerIndex].Length; neuronIndexInLayer++)
            {
                for (int neuronIndexInPreviousLayer = 0; neuronIndexInPreviousLayer < neuralNetworkToCloneFrom.Weights[layerIndex][neuronIndexInLayer].Length; neuronIndexInPreviousLayer++)
                {
                    neuralNetworkCloneTo.Weights[layerIndex][neuronIndexInLayer][neuronIndexInPreviousLayer] = neuralNetworkToCloneFrom.Weights[layerIndex][neuronIndexInLayer][neuronIndexInPreviousLayer];
                }
            }
        }
    }

    /// <summary>
    /// Returns the NN formula.
    /// </summary>
    /// <returns></returns>
    internal string Formula()
    {
        Dictionary<string, string> values = [];

        int neurons = 0;
        for (int layer = 0; layer < Layers.Length; layer++)
        {
            neurons += Neurons[layer].Length;
            if (neurons > 30) return "too big to output (exceed 30 neurons)";
        }

        for (int neuronIndexForLayer = 0; neuronIndexForLayer < Neurons[0].Length; neuronIndexForLayer++)
        {
            values.Add($"0-{neuronIndexForLayer}", $"input[{neuronIndexForLayer}]");
        }

        // we start on layer 1 as we are computing values from prior layers (layer 0 is inputs)
        for (int layer = 1; layer < Layers.Length; layer++)
        {
            List<string> dictionaryEntriesWeShouldRemove = [];

            for (int neuronIndexForLayer = 0; neuronIndexForLayer < Neurons[layer].Length; neuronIndexForLayer++)
            {
                // sum of outputs from the previous layer
                StringBuilder valueFormula = new(20);

                for (int neuronIndexInPreviousLayer = 0; neuronIndexInPreviousLayer < Neurons[layer - 1].Length; neuronIndexInPreviousLayer++)
                {
                    // remember: the "weight" amplifies or reduces, so we take the output of the prior neuron and "amplify/reduce" it's output here
                    string weight = Weights[layer - 1][neuronIndexForLayer][neuronIndexInPreviousLayer].ToString();

                    string key = $"{layer - 1}-{neuronIndexInPreviousLayer}";
                    string neuronvalue = values[key];

                    if (!dictionaryEntriesWeShouldRemove.Contains(key)) dictionaryEntriesWeShouldRemove.Add(key);

                    valueFormula.Append($"({weight}*{neuronvalue})+");
                }

                string value = valueFormula.ToString().Trim('+');


                values.Add($"{layer}-{neuronIndexForLayer}", $"Activate({value}+{Biases[layer][neuronIndexForLayer]})");
            }

            // reduce dictionary, as each iteration embeds the previous layer
            foreach (string key in dictionaryEntriesWeShouldRemove) values.Remove(key);
        }

        string result = ($"direction = ({values[(Layers.Length - 1).ToString() + "-0"]});").Replace("+-", "-");

        return result;
    }
}