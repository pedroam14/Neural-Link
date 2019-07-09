using System;
using UnityEngine;
using Unity;
using System.Collections;
using System.Collections.Generic;
public class GNet : MonoBehaviour
{
    public Texture2D inputImage;
    public static double alphaRate = 0.33f;
    public enum state
    {
        leakyReLU,
        tanH
    };
    System.Random random;
    int[] layer; //layer information
    FullyConnectedLayer[] layers; //layers in the network
    List<string> weightsToSave = new List<string>(); //all the weighs we'll be saving in string format

    //constructor setting up layers
    public void CreateNetwork(int[] layer)
    {
        //deep copy layers
        this.layer = new int[layer.Length];
        for (int i = 0; i < layer.Length; i++)
        {
            this.layer[i] = layer[i];
        }

        //creates neural layers
        layers = new FullyConnectedLayer[layer.Length - 1];

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new FullyConnectedLayer(layer[i], layer[i + 1]);
        }
    }
    Color[] bufferedPixels, newColors;
    void Start()
    {
        random = new System.Random();

        bufferedPixels = inputImage.GetPixels();
        newColors = new Color[bufferedPixels.Length];
        for (int i = 0; i < inputImage.width; i++)
        {
            for (int j = 0; j < inputImage.height; j++)
            {
                newColors[i * inputImage.height + j] = new Color(
                    Mathf.PerlinNoise((float)i / inputImage.width, (float)j / inputImage.height) * (float)random.NextDouble(),
                    Mathf.PerlinNoise((float)i / inputImage.width, (float)j / inputImage.height) * (float)random.NextDouble(),
                    Mathf.PerlinNoise((float)i / inputImage.width, (float)j / inputImage.height) * (float)random.NextDouble(),
                    Mathf.PerlinNoise((float)i / inputImage.width, (float)j / inputImage.height) * (float)random.NextDouble()
                    );
            }
        }
        inputImage.SetPixels(newColors);
        inputImage.Apply();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            inputImage.SetPixels(bufferedPixels);
            inputImage.Apply();
        }
    }

    //high level feedforward for this network
    //the inputs to be feed forwared will be the main parameter
    public double[] FeedForward(double[] inputs)
    {
        //feed forward
        layers[0].FeedForward(inputs);
        for (int i = 1; i < layers.Length; i++)
        {
            layers[i].FeedForward(layers[i - 1].outputs);
        }

        return layers[layers.Length - 1].outputs; //return output of last layer
    }

    public void SaveWeights(List<double> neuronList) //storing weighs in every single layer
    {
        for (int x = 0; x < layers.Length; x++)
        {
            for (int y = 0; y < layers[x].weights.GetLength(0); y++)
            {
                for (int z = 0; z < layers[x].weights.GetLength(1); z++)
                {
                    neuronList.Add(layers[x].weights[y, z]); //store every weight as a line in our list of strings
                }
            }
        }
    }
    public void LoadWeights(List<double> neuronWeights) //same but loading this time around
    {
        Debug.Log("Loading weights...");
        Queue<double> queue = new Queue<double>(neuronWeights);
        for (int x = 0; x < layers.Length; x++)
        {
            for (int y = 0; y < layers[x].weights.GetLength(0); y++)
            {
                for (int z = 0; z < layers[x].weights.GetLength(1); z++)
                {
                    layers[x].weights[y, z] = queue.Dequeue();
                }
            }
        }
    }




    //it is expexted the one feed forward was done before this back prop, or there won't be anything to compare this to
    public void BackProp(double[] expected)
    {
        // run over all layers backwards
        for (int i = layers.Length - 1; i >= 0; i--)
        {
            if (i == layers.Length - 1)
            {
                layers[i].BackPropOutput(expected); //back prop output
            }
            else
            {
                layers[i].BackPropHidden(layers[i + 1].gamma, layers[i + 1].weights); //back prop hidden
            }
        }

        //update weights
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].UpdateWeights();
        }
    }
    public class Kernel
    {

    }
    public class ConvLayer
    {
        int depth, stride, bias;
        int[,,] kernel;
        double[,,] outputVolume;
        int[,,] zeroPadding;
        public ConvLayer(int depth, int stride, int[,,] zeroPadding, int[,,] kernel)
        {
            this.depth = depth;
            this.stride = stride;
            this.zeroPadding = zeroPadding;
            this.kernel = kernel;
            bias = 1;
        }

        //constructor that doesn't specify the zero padding, only use if the kernel has an uneven dimension size
        public ConvLayer(int depth, int stride, int[,,] kernel)
        {
            this.depth = depth;
            this.stride = stride;
            this.kernel = kernel;
            this.zeroPadding = new int[(kernel.GetLength(0) - 1) / 2, (kernel.GetLength(1) - 1) / 2, (kernel.GetLength(2) - 1) / 2];
            bias = 1;
        }
        void ZeroPad()
        {

        }
        public double[,,] Convolution(double[,,] inputVolume)
        {
            outputVolume = new double[(inputVolume.GetLength(0) - kernel.GetLength(0) + 2 * zeroPadding.GetLength(0)) / stride + 1,
                                      (inputVolume.GetLength(1) - kernel.GetLength(1) + 2 * zeroPadding.GetLength(1)) / stride + 1,
                                      (inputVolume.GetLength(2) - kernel.GetLength(2) + 2 * zeroPadding.GetLength(2)) / stride + 1];
            for (int i = 0; i < kernel.GetLength(0); i++)
            {
                for (int j = 0; j < kernel.GetLength(1); j++)
                {
                    for (int k = 0; k < kernel.GetLength(2); k++)
                    {
                        //outputVolume[i, j, k] = kernel[i, j, k] * inputVolume[1, 2, 3];
                    }
                }
            }
            return outputVolume;
        }



    }
    public class ImageInputLayer
    {
        double[] outputs;
        Texture2D inputImage;
        //Color[] pixelColors;
        double[,,] inputVolume;
        public ImageInputLayer(Texture2D inputImage)
        {
            this.inputImage = inputImage;
        }
        public void GetInputVolume()
        {
            var pixelColors = inputImage.GetPixels();
            inputVolume = new double[inputImage.width, inputImage.height, 4]; //4 on the z axis since the image is ARGB, greyscale = 1, RGB = 3 etc
            for (int i = 0; i < inputImage.width; i++)
            {
                for (int j = 0; j < inputImage.height; j++)
                {
                    inputVolume[i, j, 0] = pixelColors[j + i * inputImage.height].a;
                    inputVolume[i, j, 1] = pixelColors[j + i * inputImage.height].r;
                    inputVolume[i, j, 2] = pixelColors[j + i * inputImage.height].g;
                    inputVolume[i, j, 3] = pixelColors[j + i * inputImage.height].b;
                }
            }
            Debug.Log("Input Volume Filled!");
        }
    }
    //each individual layer in the neural network
    public class FullyConnectedLayer
    {
        int numberOfInputs; //# of neurons in the previous layer
        int numberOfOuputs; //# of neurons in the current layer
        public double alpha = 0.001f;


        public double[] outputs; //outputs of this layer
        public double[] inputs; //inputs in into this layer
        public double[,] weights; //weights of this layer
        public double[,] weightsDelta; //deltas of this layer
        public double[] gamma; //gamma of this layer
        public double[] error; //error of the output layer

        public static System.Random random = new System.Random(); //Static random class variable

        //constructor initilizes our data structures with the number of neurons in the previous and current layers
        public FullyConnectedLayer(int numberOfInputs, int numberOfOuputs)
        {
            this.numberOfInputs = numberOfInputs;
            this.numberOfOuputs = numberOfOuputs;

            //initilize datastructures
            outputs = new double[numberOfOuputs];
            inputs = new double[numberOfInputs];
            weights = new double[numberOfOuputs, numberOfInputs];
            weightsDelta = new double[numberOfOuputs, numberOfInputs];
            gamma = new double[numberOfOuputs];
            error = new double[numberOfOuputs];

            InitilizeWeights(); //initilize weights
        }

        //initilize weights between -0.5 and 0.5 << 後でこれをちゃんと見て
        public void InitilizeWeights()
        {
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weights[i, j] = random.NextDouble() - 0.5f;
                }
            }
        }
        public void InitializeWeightsApprox()
        {
            //add code here
        }

        //feedforward this layer with a given input, get the output of the previous layer

        public double[] FeedForward(double[] inputs)
        {
            this.inputs = inputs;// keep shallow copy which can be used for back propagation

            //feed forwards
            for (int i = 0; i < numberOfOuputs; ++i)
            {
                outputs[i] = 0;
                for (int j = 0; j < numberOfInputs; j++)
                {
                    outputs[i] += inputs[j] * weights[i, j];
                }
                outputs[i] = Math.Tanh(outputs[i]);
            };

            return outputs;
        }


        //hyperbolic tangent derivative, value will be an already computed hyperbolic tangential value
        public double TanHDer(double value)
        {
            return (1 - Math.Pow(value, 2));
        }


        //back propagation for the output layer, requires the expected output as a parameter
        public void BackPropOutput(double[] expected)
        {
            //error dervative of the cost function
            for (int i = 0; i < numberOfOuputs; i++)
            {
                error[i] = outputs[i] - expected[i];
            }

            //gamma calculation
            for (int i = 0; i < numberOfOuputs; i++)
            {
                gamma[i] = error[i] * TanHDer(outputs[i]);
            }

            //caluclating detla weights

            for (int i = 0; i < numberOfOuputs; ++i)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weightsDelta[i, j] = gamma[i] * inputs[j];
                }
            };
        }

        //back propagation for the hidden layers, with gammafoward being the gamma of the forward layer and weightsforward being the weights of the forward layer
        public void BackPropHidden(double[] gammaForward, double[,] weightsFoward)
        {
            //Caluclate new gamma using gamma sums of the forward layer
            for (int i = 0; i < numberOfOuputs; ++i)
            {
                gamma[i] = 0;

                for (int j = 0; j < gammaForward.Length; j++)
                {
                    gamma[i] += gammaForward[j] * weightsFoward[j, i];
                }

                gamma[i] *= TanHDer(outputs[i]);
            };

            //calculating delta weights
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weightsDelta[i, j] = gamma[i] * inputs[j];
                }
            };
        }
        //updating weights, as the name very well implies
        public void UpdateWeights()
        {
            for (int i = 0; i < numberOfOuputs; ++i)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weights[i, j] -= weightsDelta[i, j] * 0.033f;
                }
            };
        }
        public static double Sigmoid(double val)
        {
            return 1f / (1f + Math.Pow(Math.E, -val));
        }
        public static double SignoidDer(double val)
        {
            return Sigmoid(val) * Sigmoid(1 - Sigmoid(val));
        }
        public static double LeakyReLU(double val)
        {
            if (val > 0)
            {
                return val;
            }
            else
            {
                return 0.01f * val;
            }
        }
        public static double LeakyReLUDer(double val)
        {
            if (val > 0)
            {
                return 1;
            }
            else
            {
                return 0.01f;
            }
        }
    }
    public class LeakyReLULayer
    {

        int numberOfInputs; //# of neurons in the previous layer
        int numberOfOuputs; //# of neurons in the current layer
        public double alpha = 0.001f;


        public double[] outputs; //outputs of this layer
        public double[] inputs; //inputs in into this layer
        public double[,] weights; //weights of this layer
        public double[,] weightsDelta; //deltas of this layer
        public double[] gamma; //gamma of this layer
        public double[] error; //error of the output layer

        public static System.Random random = new System.Random(); //Static random class variable

        //constructor initilizes our data structures with the number of neurons in the previous and current layers
        public LeakyReLULayer(int numberOfInputs, int numberOfOuputs)
        {
            this.numberOfInputs = numberOfInputs;
            this.numberOfOuputs = numberOfOuputs;

            //initilize datastructures
            outputs = new double[numberOfOuputs];
            inputs = new double[numberOfInputs];
            weights = new double[numberOfOuputs, numberOfInputs];
            weightsDelta = new double[numberOfOuputs, numberOfInputs];
            gamma = new double[numberOfOuputs];
            error = new double[numberOfOuputs];

            InitilizeWeights(); //initilize weights
        }

        //initilize weights between -0.5 and 0.5 << 後でこれをちゃんと見て
        public void InitilizeWeights()
        {
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weights[i, j] = random.NextDouble() - 0.5f;
                }
            }
        }
        public void InitializeWeightsApprox()
        {
            //add code here
        }

        //feedforward this layer with a given input, get the output of the previous layer

        public double[] FeedForward(double[] inputs)
        {
            this.inputs = inputs;// keep shallow copy which can be used for back propagation

            //feed forwards
            for (int i = 0; i < numberOfOuputs; ++i)
            {
                outputs[i] = 0;
                for (int j = 0; j < numberOfInputs; j++)
                {
                    outputs[i] += inputs[j] * weights[i, j];
                }
                outputs[i] = Math.Tanh(outputs[i]);
            };

            return outputs;
        }


        //hyperbolic tangent derivative, value will be an already computed hyperbolic tangential value
        public double TanHDer(double value)
        {
            return (1 - Math.Pow(value, 2));
        }


        //back propagation for the output layer, requires the expected output as a parameter
        public void BackPropOutput(double[] expected)
        {
            //error dervative of the cost function
            for (int i = 0; i < numberOfOuputs; i++)
            {
                error[i] = outputs[i] - expected[i];
            }

            //gamma calculation
            for (int i = 0; i < numberOfOuputs; i++)
            {
                gamma[i] = error[i] * LeakyReLUDer(outputs[i]);
            }

            //caluclating detla weights

            for (int i = 0; i < numberOfOuputs; ++i)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weightsDelta[i, j] = gamma[i] * inputs[j];
                }
            };
        }

        //back propagation for the hidden layers, with gammafoward being the gamma of the forward layer and weightsforward being the weights of the forward layer
        public void BackPropHidden(double[] gammaForward, double[,] weightsFoward)
        {
            //Caluclate new gamma using gamma sums of the forward layer
            for (int i = 0; i < numberOfOuputs; ++i)
            {
                gamma[i] = 0;

                for (int j = 0; j < gammaForward.Length; j++)
                {
                    gamma[i] += gammaForward[j] * weightsFoward[j, i];
                }

                gamma[i] *= LeakyReLUDer(outputs[i]);
            };

            //calculating delta weights
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weightsDelta[i, j] = gamma[i] * inputs[j];
                }
            };
        }
        //updating weights, as the name very well implies
        public void UpdateWeights()
        {
            for (int i = 0; i < numberOfOuputs; ++i)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weights[i, j] -= weightsDelta[i, j] * 0.033f;
                }
            };
        }
        public static double Sigmoid(double val)
        {
            return 1f / (1f + Math.Pow(Math.E, -val));
        }
        public static double SignoidDer(double val)
        {
            return Sigmoid(val) * Sigmoid(1 - Sigmoid(val));
        }
        public static double LeakyReLU(double val)
        {
            if (val > 0)
            {
                return val;
            }
            else
            {
                return 0.01f * val;
            }
        }
        public static double LeakyReLUDer(double val)
        {
            if (val > 0)
            {
                return 1;
            }
            else
            {
                return 0.01f;
            }
        }
    }
    public class SigmoidOutLayer
    {

        int numberOfInputs; //# of neurons in the previous layer
        int numberOfOuputs; //# of neurons in the current layer
        public double alpha = 0.001f;


        public double[] outputs; //outputs of this layer
        public double[] inputs; //inputs in into this layer
        public double[,] weights; //weights of this layer
        public double[,] weightsDelta; //deltas of this layer
        public double[] gamma; //gamma of this layer
        public double[] error; //error of the output layer

        public static System.Random random = new System.Random(); //Static random class variable

        //constructor initilizes our data structures with the number of neurons in the previous and current layers
        public SigmoidOutLayer(int numberOfInputs, int numberOfOuputs)
        {
            this.numberOfInputs = numberOfInputs;
            this.numberOfOuputs = numberOfOuputs;

            //initilize datastructures
            outputs = new double[numberOfOuputs];
            inputs = new double[numberOfInputs];
            weights = new double[numberOfOuputs, numberOfInputs];
            weightsDelta = new double[numberOfOuputs, numberOfInputs];
            gamma = new double[numberOfOuputs];
            error = new double[numberOfOuputs];

            InitilizeWeights(); //initilize weights
        }

        //initilize weights between -0.5 and 0.5 << 後でこれをちゃんと見て
        public void InitilizeWeights()
        {
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weights[i, j] = random.NextDouble() - 0.5f;
                }
            }
        }
        public void InitializeWeightsApprox()
        {
            //add code here
        }

        //feedforward this layer with a given input, get the output of the previous layer

        public double[] FeedForward(double[] inputs)
        {
            this.inputs = inputs;// keep shallow copy which can be used for back propagation

            //feed forwards
            for (int i = 0; i < numberOfOuputs; ++i)
            {
                outputs[i] = 0;
                for (int j = 0; j < numberOfInputs; j++)
                {
                    outputs[i] += inputs[j] * weights[i, j];
                }
                outputs[i] = Math.Tanh(outputs[i]);
            };

            return outputs;
        }


        //hyperbolic tangent derivative, value will be an already computed hyperbolic tangential value
        public double TanHDer(double value)
        {
            return (1 - Math.Pow(value, 2));
        }


        //back propagation for the output layer, requires the expected output as a parameter
        public void BackPropOutput(double[] expected)
        {
            //error dervative of the cost function
            for (int i = 0; i < numberOfOuputs; i++)
            {
                error[i] = outputs[i] - expected[i];
            }

            //gamma calculation
            for (int i = 0; i < numberOfOuputs; i++)
            {
                gamma[i] = error[i] * TanHDer(outputs[i]);
            }

            //caluclating detla weights

            for (int i = 0; i < numberOfOuputs; ++i)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weightsDelta[i, j] = gamma[i] * inputs[j];
                }
            };
        }

        //back propagation for the hidden layers, with gammafoward being the gamma of the forward layer and weightsforward being the weights of the forward layer
        public void BackPropHidden(double[] gammaForward, double[,] weightsFoward)
        {
            //Caluclate new gamma using gamma sums of the forward layer
            for (int i = 0; i < numberOfOuputs; ++i)
            {
                gamma[i] = 0;

                for (int j = 0; j < gammaForward.Length; j++)
                {
                    gamma[i] += gammaForward[j] * weightsFoward[j, i];
                }

                gamma[i] *= TanHDer(outputs[i]);
            };

            //calculating delta weights
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weightsDelta[i, j] = gamma[i] * inputs[j];
                }
            };
        }
        //updating weights, as the name very well implies
        public void UpdateWeights()
        {
            for (int i = 0; i < numberOfOuputs; ++i)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weights[i, j] -= weightsDelta[i, j] * 0.033f;
                }
            };
        }
        public static double LeakyReLU(double val)
        {
            if (val > 0)
            {
                return val;
            }
            else
            {
                return 0.01f * val;
            }
        }
        public static double LeakyReLUDer(double val)
        {
            if (val > 0)
            {
                return 1;
            }
            else
            {
                return 0.01f;
            }
        }
    }
}