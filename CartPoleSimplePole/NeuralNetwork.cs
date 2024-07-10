//#define checkInputs // check inputs are within -1 to 1, if you want to debug the network.
//#define GaussianWeights // use Gaussian weights. Although recommended for neural nets (learns quicker), they create a huge amount of noise in the image and don't impair the network.
namespace Cart;

/// <summary>
///    _   _                      _   _   _      _                      _
///   | \ | | ___ _   _ _ __ __ _| | | \ | | ___| |___      _____  _ __| | __
///   |  \| |/ _ \ | | | '__/ _` | | |  \| |/ _ \ __\ \ /\ / / _ \| '__| |/ /
///   | |\  |  __/ |_| | | | (_| | | | |\  |  __/ |_ \ V  V / (_) | |  |   < 
///   |_| \_|\___|\__,_|_|  \__,_|_| |_| \_|\___|\__| \_/\_/ \___/|_|  |_|\_\
///
/// </summary>
public class NeuralNetwork
{
    #region CONSTANTS
    /// <summary>
    /// The rate at which the network learns, i.e. is multiplied by the gradient.
    /// </summary>
    private const float c_learning_rate = 0.01f;

    /// <summary>
    /// The maximum gradient allowed, to prevent the network from blowing up.
    /// </summary>
    private const double c_maxGradient = 1f;

    /// <summary>
    /// SELU alpha.
    /// </summary>
    const float c_alpha = 1.6732f;

    /// <summary>
    /// SELU lambda.
    /// </summary>
    const float c_lambda = 1.0507f;
    #endregion

    #region Properties
    /// <summary>
    /// How many layers of neurons.
    /// 1 => input is output, and feed forward will crash.
    /// </summary>
    private readonly int[] _layers;

    /// <summary>
    /// The neurons.
    /// [layer][neuron]
    /// </summary>
    private double[][] _neurons;

    /// <summary>
    /// NN Biases. Either improves or lowers the chance of this neuron fully firing.
    /// [layer][neuron]
    /// </summary>
    private double[][] _biases;

    /// <summary>
    /// NN weights. Reduces or amplifies the output for the relationship between neurons in each layer
    /// [layer][neuron][neuron]
    /// </summary>
    private double[][][] _weights;

    /// <summary>
    /// Store the gradients for each neuron.
    /// </summary>
    private double[][] _gradients;
    #endregion

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="_id">Unique ID of the neuron.</param>
    /// <param name="layerDefinition">Defines size of the layers.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Init*() set the fields.
    internal NeuralNetwork(int[] layerDefinition)
#pragma warning restore CS8618
    {
        // (1) INPUT (2) HIDDEN (3) OUTPUT. Less than 3 would be INPUT->OUTPUT; hardly "AI" but actually works
        if (layerDefinition.Length < 2) throw new ArgumentException("layer definition must be >2", nameof(layerDefinition));

        // copy layerDefinition to Layers
        _layers = new int[layerDefinition.Length];

        for (int layer = 0; layer < layerDefinition.Length; layer++)
        {
            _layers[layer] = layerDefinition[layer];
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
        InitialiseGradients();
    }

    /// <summary>
    /// Creates an empty storage array for the gradients in the network.
    /// </summary>
    private void InitialiseGradients()
    {
        List<double[]> gradientsList = [];

        for (int layer = 0; layer < _layers.Length; layer++)
        {
            gradientsList.Add(new double[_layers[layer]]);
        }

        _gradients = [.. gradientsList];
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

        for (int layer = 0; layer < _layers.Length; layer++)
        {
            neuronsList.Add(new double[_layers[layer]]);
        }

        _neurons = [.. neuronsList];
    }

    /// <summary>
    /// Initializes and populates biases.
    /// </summary>
    private void InitialiseBiases()
    {
        List<double[]> biasList = [];

        // for each layer of neurons, we have to set biases.
        for (int layer = 0; layer < _layers.Length; layer++)
        {
            double[] bias = new double[_layers[layer]];

            // bias[biasLayer] = 0 // https://cs224d.stanford.edu/lecture_notes/LectureNotes3.pdf should be 0.

            biasList.Add(bias);
        }

        _biases = [.. biasList];
    }

    /// <summary>
    /// Initialises the weights, using a uniform Gaussian distribution.
    /// </summary>
    private void InitialiseWeights()
    {
        List<double[][]> weightsList = []; // used to construct weights, as dynamic arrays aren't supported

        for (int layer = 1; layer < _layers.Length; layer++)
        {
            List<double[]> layerWeightsList = [];

            int neuronsInPreviousLayer = _layers[layer - 1];
            int neuronsInCurrentLayer = _layers[layer];


            for (int neuronIndexInLayer = 0; neuronIndexInLayer < neuronsInCurrentLayer; neuronIndexInLayer++)
            {
                double[] neuronWeights = new double[neuronsInPreviousLayer];
                double variance = 1.0f / neuronsInPreviousLayer;

                for (int neuronIndexInPreviousLayer = 0; neuronIndexInPreviousLayer < neuronsInPreviousLayer; neuronIndexInPreviousLayer++)
                {
                    // Initialize weights from a Gaussian distribution
#if GaussianWeights
                    neuronWeights[neuronIndexInPreviousLayer] = NextGaussian(0, Math.Sqrt(variance));
#endif
                }

                layerWeightsList.Add(neuronWeights);
            }

            weightsList.Add([.. layerWeightsList]);
        }

        _weights = [.. weightsList];
    }

#if GaussianWeights
    /// <summary>
    /// Returns a Gaussian uniform random numbers, code from GitHub Copilot.
    /// </summary>
    /// <param name="mean"></param>
    /// <param name="stdDev"></param>
    /// <returns></returns>
    public static double NextGaussian(double mean, double stdDev)
    {
        double u1 = 1.0 - Math.Abs(Utils.GetRandomValuePlusMinus1());
        double u2 = 1.0 - Math.Abs(Utils.GetRandomValuePlusMinus1());
        double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        
        return mean + stdDev * z;
    }
#endif

    /// <summary>
    /// Feed forward, inputs >==> outputs.
    /// </summary>
    /// <param name="inputs"></param>
    /// <returns></returns>
    internal double[] FeedForward(double[] inputs)
    {
        // assign the input values to the input neurons
        for (int i = 0; i < inputs.Length; i++)
        {
#if checkInputs // enable if you think you might not be getting the right inputs
            if (inputs[i]>1 || inputs[i]<-1)
            {
                Debugger.Break();
            }
#endif
            _neurons[0][i] = inputs[i];
        }

        // iterate over the neurons in the network
        for (int layer = 1; layer < _layers.Length; layer++)
        {
            for (int neuronIndexInLayer = 0; neuronIndexInLayer < _neurons[layer].Length; neuronIndexInLayer++)
            {
                double value = _biases[layer][neuronIndexInLayer];

                for (int neuronIndexInPreviousLayer = 0; neuronIndexInPreviousLayer < _neurons[layer - 1].Length; neuronIndexInPreviousLayer++)
                {
                    value += _weights[layer - 1][neuronIndexInLayer][neuronIndexInPreviousLayer] * _neurons[layer - 1][neuronIndexInPreviousLayer];
                }

                _neurons[layer][neuronIndexInLayer] = ActivationFunction(value);
            }
        }

        return _neurons[^1];
    }
    
    /// <summary>
    /// SELU activation function.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static double ActivationFunction(double value)
    {
        return value > 0 ? c_lambda * value : c_lambda * c_alpha * (Math.Exp(value) - 1);
    }

    /// <summary>
    /// SELU derivative function.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static double ActivationFunctionDerivative(double value)
    {
        return value > 0 ? c_lambda: c_lambda * c_alpha * Math.Exp(value);
    }

    /// <summary>
    /// Perform back-propagation (adjust weights/biases to match expected output).
    /// </summary>
    /// <param name="expectedOutput"></param>
    public void BackPropagate(double[] expectedOutput)
    {
        double[] output = _neurons[^1];

        // calculate the output layer gradient
        for (int neuronIndex = 0; neuronIndex < output.Length; neuronIndex++)
        {
            double error = expectedOutput[neuronIndex] - output[neuronIndex];
            _gradients[^1][neuronIndex] = ClampGradient(error * ActivationFunctionDerivative(output[neuronIndex]));
        }

        // calculate the hidden layer gradients
        for (int layer = _layers.Length - 2; layer >= 0; layer--)
        {
            for (int neuronIndex = 0; neuronIndex < _neurons[layer].Length; neuronIndex++)
            {
                double sum = 0;

                for (int nextNeuronIndex = 0; nextNeuronIndex < _neurons[layer + 1].Length; nextNeuronIndex++)
                {
                    sum += _weights[layer][nextNeuronIndex][neuronIndex] * _gradients[layer + 1][nextNeuronIndex];
                }

                _gradients[layer][neuronIndex] = ClampGradient(sum * ActivationFunctionDerivative(_neurons[layer][neuronIndex]));
            }
        }

        // update the weights
        for (int layer = 1; layer < _layers.Length; layer++)
        {
            for (int neuronIndex = 0; neuronIndex < _neurons[layer].Length; neuronIndex++)
            {
                for (int neuronIndexInPreviousLayer = 0; neuronIndexInPreviousLayer < _neurons[layer - 1].Length; neuronIndexInPreviousLayer++)
                {
                    double delta = c_learning_rate * _gradients[layer][neuronIndex] * _neurons[layer - 1][neuronIndexInPreviousLayer];
                    _weights[layer - 1][neuronIndex][neuronIndexInPreviousLayer] += delta;
                }
            }
        }

        // update the biases
        for (int layer = 0; layer < _layers.Length; layer++)
        {
            for (int neuronIndex = 0; neuronIndex < _neurons[layer].Length; neuronIndex++)
            {
                double delta = c_learning_rate * _gradients[layer][neuronIndex];
                _biases[layer][neuronIndex] += delta;
            }
        }
    }

    /// <summary>
    /// To prevent the gradient from becoming too large, we clamp it.
    /// SELU doesn't have a positive limit, so it's quite possible to blow up and get Infinity -> NaN.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static double ClampGradient(double value)
    {
        if (value < -c_maxGradient) return -c_maxGradient;
        if (value > c_maxGradient) return c_maxGradient;
        return value;
    }

    /// <summary>
    /// Returns the neural network response if a "1" is input into a particular neuron.
    /// This enables us to map the effect each neuron has on the output.
    /// </summary>
    /// <param name="neuron"></param>
    /// <returns></returns>
    internal double GetNeuron(int neuron)
    {
        return TraceFromSingleInputNeuronToOutput(neuron, 1);
    }

    /// <summary>
    /// Returns the neural network response to a particular neuron containing the actual value from the image.
    /// </summary>
    /// <param name="neuron"></param>
    /// <returns></returns>
    internal double GetActualNeuron(int neuron)
    {
        return TraceFromSingleInputNeuronToOutput(neuron, _neurons[0][neuron]);
    }

    /// <summary>
    /// Enables us to trace the impact of input neurons on the output. We set all to 0, apart from the neuron we're interested in.
    /// Same as feedback, and is DESTRUCTIVE in that it updates neurons. It does not impact weightings or bias.
    /// </summary>
    /// <param name="neuronIndexInputLayer0"></param>
    /// <param name="inputValue"></param>
    /// <returns></returns>
    internal double TraceFromSingleInputNeuronToOutput(int neuronIndexInputLayer0, double inputValue = 1f)
    {
        // empty the neurons
        List<double[]> neuronsList = [];

        for (int layer = 0; layer < _layers.Length; layer++)
        {
            neuronsList.Add(new double[_layers[layer]]);
        }

        double[][] neurons = [.. neuronsList];

        // plug in this neuron to the input layer
        neurons[0][neuronIndexInputLayer0] = inputValue;

        // iterate over the neurons in the network
        for (int layer = 1; layer < _layers.Length; layer++)
        {
            for (int neuronIndexInLayer = 0; neuronIndexInLayer < _neurons[layer].Length; neuronIndexInLayer++)
            {
                double value = _biases[layer][neuronIndexInLayer];

                for (int neuronIndexInPreviousLayer = 0; neuronIndexInPreviousLayer < neurons[layer - 1].Length; neuronIndexInPreviousLayer++)
                {
                    value += _weights[layer - 1][neuronIndexInLayer][neuronIndexInPreviousLayer] * neurons[layer - 1][neuronIndexInPreviousLayer];
                }

                neurons[layer][neuronIndexInLayer] = ActivationFunction(value);
            }
        }

        return neurons[^1][0];
    }
}