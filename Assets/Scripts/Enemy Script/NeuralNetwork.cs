using System;
using UnityEngine;
using Unity;
using System.Collections;
using System.Collections.Generic;
public class NeuralNetwork
{
    public static double alphaRate = 0.33f;
    public enum state
    {
        leakyReLU,
        tanH
    };
    uint test;
    int[] layer; //layer information
    Layer[] layers; //layers in the network
    List<string> weightsToSave = new List<string>(); //all the weighs we'll be saving in string format
                                                     //constructor setting up layers

    public void CreateNetwork(int[] layer)
    {
        //deep copy layers
        this.layer = new int[layer.Length];
        for (int i = 0; i < layer.Length; i++)
            this.layer[i] = layer[i];

        //creates neural layers
        layers = new Layer[layer.Length - 1];

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(layer[i], layer[i + 1]);
            
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

    //each convolution layer in the network

    //each individual fully connected (MLP) layer in the neural network
    public class Layer
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
        public Layer(int numberOfInputs, int numberOfOuputs)
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
}