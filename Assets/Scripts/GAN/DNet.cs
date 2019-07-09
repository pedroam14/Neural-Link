using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using Unity;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

public class DNet
{
    public static double alphaRate = 0.33f;
    [Range(0, 10)]
    public int zeroPadding;

    internal void ClearNets()
    {
        this.convNet.convolutionalLayers.Clear();
        this.convNet.poolingLayers.Clear();
        this.pureConvNet.ClearNet();
    }

    [Range(1, 5)]
    public int stride;
    [Range(1, 20)]
    public int nOfFilters;
    TraditionalConvolutionalNetwork convNet;
    PureConvolutionalNetwork pureConvNet;
    public int filterSize;
    public enum PoolingMode
    {
        average,
        maximum
    };
    public enum Activation
    {
        LeakyReLU,
        Sigmoid,
        TanH
    }
    Activation act;
    public DNet(Activation act)
    {
        this.act = act;
        convNet = new TraditionalConvolutionalNetwork(act);
        pureConvNet = new PureConvolutionalNetwork(act);
    }
    public DNet()
    {
        this.act = Activation.LeakyReLU;

    }
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
    public void PrintOutputs()
    {
        //convNet.PrintOutput();
    }
    public int nOfImages;
    public void CreateDNetwork(Texture2D[] inputImages)
    {
        nOfImages = inputImages.Length;
        CreateInputLayer(inputImages[0], typeof(TraditionalConvolutionalNetwork));
        Debug.Log("Convolution on picture " + 0);
        convNet.PrintOutput();
        for (int i = 1; i < inputImages.Length; i++)
        {
            var inputValues = inputImages[i].GetPixels();
            var inputVolume = new double[inputImages[i].width, inputImages[i].height, 4];
            //getpixels flip the image upside down, idk why 
            for (int y = 0; y < inputImages[i].height; y++)
            {
                for (int x = 0; x < inputImages[i].width; x++)
                {
                    //Debug.Log(inputImage.height - y - 1);
                    inputVolume[x, inputImages[i].height - y - 1, 0] = inputValues[x + y * inputImages[i].width].a;
                    inputVolume[x, inputImages[i].height - y - 1, 1] = inputValues[x + y * inputImages[i].width].r;
                    inputVolume[x, inputImages[i].height - y - 1, 2] = inputValues[x + y * inputImages[i].width].g;
                    inputVolume[x, inputImages[i].height - y - 1, 3] = inputValues[x + y * inputImages[i].width].b;
                }
            }
            convNet.FeedForward(inputVolume);
            //for debugging, please comment this later
            Debug.Log("Convolution on picture " + i);
            convNet.PrintOutput();
        }
    }
    public void CreatePureDNetwork(Texture2D[] inputImages)
    {
        CreateInputLayer(inputImages[0], typeof(PureConvolutionalNetwork));
        Debug.Log("Convolution on picture " + 0);
        pureConvNet.PrintOutput();
        for (int i = 1; i < inputImages.Length; i++)
        {
            var inputValues = inputImages[i].GetPixels();
            var inputVolume = new double[inputImages[i].width, inputImages[i].height, 4];
            //getpixels flip the image upside down, idk why 
            for (int y = 0; y < inputImages[i].height; y++)
            {
                for (int x = 0; x < inputImages[i].width; x++)
                {
                    //Debug.Log(inputImage.height - y - 1);
                    inputVolume[x, inputImages[i].height - y - 1, 0] = inputValues[x + y * inputImages[i].width].a;
                    inputVolume[x, inputImages[i].height - y - 1, 1] = inputValues[x + y * inputImages[i].width].r;
                    inputVolume[x, inputImages[i].height - y - 1, 2] = inputValues[x + y * inputImages[i].width].g;
                    inputVolume[x, inputImages[i].height - y - 1, 3] = inputValues[x + y * inputImages[i].width].b;
                }
            }
            pureConvNet.FeedForward(inputVolume);
            //for debugging, please comment this later
            Debug.Log("Convolution on picture " + i);
            pureConvNet.PrintOutput();
        }

    }
    public void CreateInputLayer(Texture2D inputImage, Type type)
    {
        var inputValues = inputImage.GetPixels();
        var inputVolume = new double[inputImage.width, inputImage.height, 4];
        //getpixels flip the image upside down, idk why 
        for (int y = 0; y < inputImage.height; y++)
        {
            for (int x = 0; x < inputImage.width; x++)
            {
                //Debug.Log(inputImage.height - y - 1);
                inputVolume[x, inputImage.height - y - 1, 0] = inputValues[x + y * inputImage.width].a;
                inputVolume[x, inputImage.height - y - 1, 1] = inputValues[x + y * inputImage.width].r;
                inputVolume[x, inputImage.height - y - 1, 2] = inputValues[x + y * inputImage.width].g;
                inputVolume[x, inputImage.height - y - 1, 3] = inputValues[x + y * inputImage.width].b;
            }
        }
        if (type == typeof(TraditionalConvolutionalNetwork))
        {
            convNet.CreateNet(2, inputVolume);
        }
        else
        {
            pureConvNet.CreateNet(2, inputVolume);
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

    public void SaveKernelWeights()
    {
        foreach (var convLayer in convNet.convolutionalLayers)
        {
            convLayer.SerializeFilters();
        }
    }
    public void LoadKernelWeights()
    {
        foreach (var convLayer in convNet.convolutionalLayers)
        {
            convLayer.DeserializeFilters();
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

    private class TraditionalConvolutionalNetwork
    {
        public List<ConvolutionalLayer> convolutionalLayers = new List<ConvolutionalLayer>();
        public List<PoolingLayer> poolingLayers = new List<PoolingLayer>();
        Activation act;
        public TraditionalConvolutionalNetwork(Activation act)
        {
            this.act = act;
        }
        public void PrintOutput()
        {
            convolutionalLayers[convolutionalLayers.Count - 1].PrintOutput();
            poolingLayers[poolingLayers.Count - 1].PrintOutput();
        }


        //basically does what the FF function does but doesn't return the output
        public void Compute(double[,,] input)
        {
            convolutionalLayers[0].FeedForward(input);
            poolingLayers[0].FeedForward(convolutionalLayers[0].FeedForward());
            for (int i = 1; i < convolutionalLayers.Count; i++)
            {
                convolutionalLayers[i].FeedForward(poolingLayers[i - 1].FeedForward());
                poolingLayers[i].FeedForward(convolutionalLayers[i].FeedForward());
            }
        }
        public void CreateNet(int layers, double[,,] input)
        {
            convolutionalLayers.Add(new ConvolutionalLayer(input, act, 0));
            poolingLayers.Add(new PoolingLayer(convolutionalLayers[0].FeedForward(), PoolingMode.maximum, 2, 2));
            for (int i = 1; i < layers; i++)
            {
                convolutionalLayers.Add(new ConvolutionalLayer(poolingLayers[i - 1].FeedForward(), act, i));
                poolingLayers.Add(new PoolingLayer(convolutionalLayers[i].FeedForward(), PoolingMode.maximum, 2, 2));
            }
        }

        //returns the output of the last pooling layer
        public double[,,] FeedForward(double[,,] input)
        {
            convolutionalLayers[0].FeedForward(input);
            poolingLayers[0].FeedForward(convolutionalLayers[0].FeedForward());
            for (int i = 1; i < convolutionalLayers.Count; i++)
            {
                convolutionalLayers[i].FeedForward(poolingLayers[i - 1].FeedForward());
                poolingLayers[i].FeedForward(convolutionalLayers[i].FeedForward());
            }
            return poolingLayers[poolingLayers.Count - 1].FeedForward();
        }
        public void LoadFilters()
        {
            for (int layer = 0; layer < convolutionalLayers.Count; layer++)
            {
                convolutionalLayers[layer].DeserializeFilters();
            }
        }

    }
    /* 
    discarding pooling layers has also been found to be important in training good generative models
    such as variational autoencoders (VAEs) or generative adversarial networks (GANs)
    */
    private class PureConvolutionalNetwork
    {
        List<ConvolutionalLayer> convolutionalLayers = new List<ConvolutionalLayer>();
        Activation act;
        public PureConvolutionalNetwork(Activation act)
        {
            this.act = act;
        }
        public void CreateNet(int layers, double[,,] input)
        {
            convolutionalLayers.Clear();
            convolutionalLayers.Add(new ConvolutionalLayer(input, act, 0, 1));
            for (int i = 1; i < layers; i++)
            {
                convolutionalLayers.Add(new ConvolutionalLayer(convolutionalLayers[i - 1].FeedForward(), act, i, i + 1));
            }
        }
        public void ClearNet()
        {
            convolutionalLayers.Clear();
        }

        internal double[,,] FeedForward(double[,,] inputVolume)
        {
            convolutionalLayers[0].FeedForward(inputVolume);
            for (int i = 1; i < convolutionalLayers.Count; i++)
            {
                convolutionalLayers[i].FeedForward(convolutionalLayers[i - 1].FeedForward());
            }
            return convolutionalLayers[convolutionalLayers.Count - 1].FeedForward();
        }

        internal void PrintOutput()
        {
            convolutionalLayers[convolutionalLayers.Count - 1].PrintOutput();
        }
    }
    //each convolution layer in the network
    private class ConvolutionalLayer
    {
        IFormatter serializedKernels = new BinaryFormatter();
        public List<Kernel> filters = new List<Kernel>();

        double[,,] inputVolume;
        double[,,] outputVolume;
        List<double[,,]> kernelDeltas;
        double[] kernelGammas;
        int[] zeroPadding;
        int stride;
        int nOfFilters;
        int layerDepth;
        Activation act;
        string debuggerino;
        public ConvolutionalLayer(int stride, int zeroPadding, double[,,] inputVolume, int nOfFilters, int filterSize, Activation act)
        {
            this.stride = stride;
            this.zeroPadding = new int[4] { zeroPadding, zeroPadding, zeroPadding, zeroPadding };
            this.nOfFilters = nOfFilters;
            Debug.Log("Kernels created.");
            this.filters = new List<Kernel>();
            for (int kernels = 0; kernels < nOfFilters; kernels++)
            {
                filters[kernels] = new Kernel(3, inputVolume.GetLength(2));
            }
            this.zeroPadding = new int[4] { zeroPadding, zeroPadding, zeroPadding, zeroPadding };
            this.inputVolume = ZeroPad(inputVolume);
            outputVolume = new double[(inputVolume.GetLength(0) - 3 + 2 * zeroPadding) / stride + 1, (inputVolume.GetLength(1) - 3 + 2 * zeroPadding) / stride + 1, 5];
            ConvoluteOutput(act);
            Debug.Log("Convolution operation complete..... or is it???");
        }


        //serialize and deserialize kernels, so that they'll keep their value
        public void SerializeFilters()
        {
            for (int kernel = 0; kernel < nOfFilters; kernel++)
            {
                var filename = "KernelDepth" + layerDepth + "k" + kernel + ".ker";
                Stream stream = new FileStream("Assets/Scripts/GAN/Kernels/" + filename, FileMode.OpenOrCreate, FileAccess.Write);
                serializedKernels.Serialize(stream, filters[kernel].kernelWeights);
                stream.Close();
            }
        }
        public void DeserializeFilters()
        {
            for (int kernel = 0; kernel < nOfFilters; kernel++)
            {
                this.filters.Add(new Kernel(3, 4));
                var filename = "KernelDepth" + layerDepth + "k" + kernel + ".ker";
                Stream stream = new FileStream("Assets/Scripts/GAN/Kernels/" + filename, FileMode.Open, FileAccess.Read);
                var filtersW = (double[,,])serializedKernels.Deserialize(stream);
                this.filters[kernel].LoadWeights(filtersW);
                //Debug.Log("Test.");
            }
            //Debug.Log("test.");
        }
        public ConvolutionalLayer(int stride, double[,,] inputVolume, int nOfFilters, int filterSize, Activation act, int depth)
        {
            this.layerDepth = depth;
            this.stride = stride;
            this.inputVolume = inputVolume;
        }
        //empty construct will create an output volume proportional to the input volume and 5 filters of size 3 for whatever reason :)
        public ConvolutionalLayer(double[,,] inputVolume, Activation act, int depth)
        {
            this.stride = 1;
            //lets try this with more filters now!
            nOfFilters = 10;

            this.layerDepth = depth;
            for (int kernel = 0; kernel < nOfFilters; kernel++)
            {
                var filename = "KernelDepth" + layerDepth + "k" + kernel + ".ker";
                if (File.Exists("Assets/Scripts/GAN/Kernels/" + filename))
                {
                    Debug.Log("Loading kernels from file...");
                    DeserializeFilters();
                }
                else
                {
                    if (filters.Count == 0)
                    {
                        filters = new List<Kernel>();
                    }
                    Debug.Log("Creating new Kernels.");
                    filters.Add(new Kernel(3, 4));
                }
            }

            /* 
            for (int i = 0; i < nOfFilters; i++)
            {
                filters[i] = new Kernel(3, inputVolume.GetLength(2));
            }
            */
            this.act = act;
            int zp = (filters[0].size - 1) / 2;
            zeroPadding = new int[4] { zp, zp, zp, zp };
            //the last bit of the output volume is actually how many layers of outputs you want, and not the channels, as the channels are all sort of lumped together during the convolution operations
            outputVolume = new double[(inputVolume.GetLength(0) - 3 + 2 * zp) / stride + 1, (inputVolume.GetLength(1) - 3 + 2 * zp) / stride + 1, 5];
            this.inputVolume = ZeroPad(inputVolume);
            FlipKernel();
            ConvoluteOutput(act);

            //Debug.Log("Convolution operation complete..... or is it???");
        }
        public ConvolutionalLayer(double[,,] inputVolume, Activation act, int depth, int stride)
        {
            this.stride = stride;
            //lets try this with more filters now!
            nOfFilters = 10;

            this.layerDepth = depth;
            for (int kernel = 0; kernel < nOfFilters; kernel++)
            {
                var filename = "KernelDepth" + layerDepth + "k" + kernel + ".ker";
                if (File.Exists("Assets/Scripts/GAN/Kernels/" + filename))
                {
                    Debug.Log("Loading kernels from file...");
                    DeserializeFilters();
                }
                else
                {
                    if (filters.Count == 0)
                    {
                        filters = new List<Kernel>();
                    }
                    Debug.Log("Creating new Kernels.");
                    filters.Add(new Kernel(3, 4));
                }
            }

            /* 
            for (int i = 0; i < nOfFilters; i++)
            {
                filters[i] = new Kernel(3, inputVolume.GetLength(2));
            }
            */
            this.act = act;
            int zp;
            if (depth == 0)
            {
                zp = (filters[0].size - 1) / 2;
            }
            else
            {
                zp = 0;
            }
            zeroPadding = new int[4] { zp, zp, zp, zp };
            //the last bit of the output volume is actually how many layers of outputs you want, and not the channels, as the channels are all sort of lumped together during the convolution operations
            outputVolume = new double[(inputVolume.GetLength(0) - 3 + 2 * zp) / stride + 1, (inputVolume.GetLength(1) - 3 + 2 * zp) / stride + 1, 5];
            if (zp != 0)
            {
                this.inputVolume = ZeroPad(inputVolume);
            }
            else
            {
                this.inputVolume = inputVolume;
            }
            ConvoluteOutput(act);

            //Debug.Log("Convolution operation complete..... or is it???");
        }
        public double[,,] FeedForward(double[,,] inputVolume)
        {
            this.inputVolume = ZeroPad(inputVolume);
            ConvoluteOutput(act);
            return FeedForward();
        }
        public ConvolutionalLayer(double[,,] inputVolume, int stride, int depth, Activation act)
        {
            this.stride = stride;
            //lets try this with more filters now!
            nOfFilters = 10;

            this.layerDepth = depth;
            for (int kernel = 0; kernel < nOfFilters; kernel++)
            {
                var filename = "KernelDepth" + layerDepth + "k" + kernel + ".ker";
                if (File.Exists("Assets/Scripts/GAN/Kernels/" + filename))
                {
                    Debug.Log("Loading kernels from file...");
                    DeserializeFilters();
                }
                else
                {
                    if (filters.Count == 0)
                    {
                        filters = new List<Kernel>();
                    }
                    Debug.Log("Creating new Kernels.");
                    filters.Add(new Kernel(3, 4));
                }
            }

            /* 
            for (int i = 0; i < nOfFilters; i++)
            {
                filters[i] = new Kernel(3, inputVolume.GetLength(2));
            }
            */
            this.act = act;
            int zp = (filters[0].size - 1) / 2;
            zeroPadding = new int[4] { zp, zp, zp, zp };
            //the last bit of the output volume is actually how many layers of outputs you want, and not the channels, as the channels are all sort of lumped together during the convolution operations
            outputVolume = new double[(inputVolume.GetLength(0) - 3 + 2 * zp) / stride + 1, (inputVolume.GetLength(1) - 3 + 2 * zp) / stride + 1, 5];
            this.inputVolume = ZeroPad(inputVolume);
            ConvoluteOutput(act);

            //Debug.Log("Convolution operation complete..... or is it???");
        }

        //adds zeroes as the name implies and returns a new, padded, input volume
        public double[,,] ZeroPad(double[,,] inputVolume)
        {
            var newInputVolume = new double[inputVolume.GetLength(0) + zeroPadding[0] * 2, inputVolume.GetLength(1) + zeroPadding[1] * 2, inputVolume.GetLength(2)];
            for (int i = 0; i < zeroPadding.Length; i++)
            {
                for (int y = zeroPadding[1]; y < newInputVolume.GetLength(1) - zeroPadding[1]; y++)
                {
                    for (int x = zeroPadding[0]; x < newInputVolume.GetLength(0) - zeroPadding[0]; x++)
                    {
                        newInputVolume[x, y, i] = inputVolume[x - zeroPadding[0], y - zeroPadding[1], i];
                    }
                }
            }
            //in case you want to see what the padded channels look like
            /*
            for (int i = 0; i < zeroPadding.Length; i++)
            {
                for (int y = 0; y < newInputVolume.GetLength(1); y++)
                {
                    for (int x = 0; x < newInputVolume.GetLength(0); x++)
                    {
                        debuggerino += newInputVolume[x, y, i] + ",";
                    }
                    debuggerino += "\n";
                }
                Debug.Log(debuggerino);
                debuggerino = "";
            }
            //*/
            return newInputVolume;
        }
        public void FlipKernel()
        {
            foreach (Kernel filter in filters)
            {
                var holder = filter.kernelWeights;
                for (int channel = 0; channel < filter.channels; channel++)
                {
                    for (int y = 0; y < filter.size; y++)
                    {
                        for (int x = 0; x < filter.size; x++)
                        {
                            filter.kernelWeights[x, y, channel] = holder[holder.GetLength(0) - (1 + x), holder.GetLength(1) - (1 + y), holder.GetLength(2) - (1 + channel)];
                        }
                    }
                }
            }
        }
        public void ConvoluteOutput(Activation act)
        {
            if (act == Activation.LeakyReLU)
            {
                outputVolume = LeakyReLU(Convolution(filters, nOfFilters));
            }
            else if (act == Activation.Sigmoid)
            {
                outputVolume = Sigmoid(Convolution(filters, nOfFilters));
            }
        }

        //FF became a glorified getter since all the math was already done elsewhere (see method: ConvoluteOuput)
        public double[,,] FeedForward()
        {
            return outputVolume;
        }


        /*
        generally the ReLU or Leaky ReLU stuff is done in a different layer
        there is no reason as to why it should be another class however, so it'll be applied on the values resulting from the convolution
        the values derived from the LeakyReLU operation will be the input of the pooling layer, in this case we might want to use an average pooling
        every paper under the sun seems to recommend max pooling instead
        俺のネットだからね、心配しないで下さい
         */
        public static double[,,] LeakyReLU(double[,,] output)
        {
            Parallel.For(0, output.GetLength(2), layerN =>
            {
                for (int y = 0; y < output.GetLength(1); y++)
                {
                    for (int x = 0; x < output.GetLength(0); x++)
                    {
                        if (output[x, y, layerN] < 0)
                        {
                            /*
                            this 0.001 is the leakage (also called alpha), toy around with the values later
                            正直またコンストラクタを作りたくないから、このナンバーを使う               
                            皆は0.0002を推奨しますでもそれはただの噂、あいつら何も知らないよね
                            */
                            output[x, y, layerN] = 0.001 * output[x, y, layerN];
                        }
                    }
                }
            });
            return output;
        }
        public double[,,] Sigmoid(double[,,] output)
        {
            Parallel.For(0, output.GetLength(2), layerN =>
            {
                for (int y = 0; y < output.GetLength(1); y++)
                {
                    for (int x = 0; x < output.GetLength(0); x++)
                    {
                        output[x, y, layerN] = 1f / (1f + Math.Pow(Math.E, -output[x, y, layerN]));
                    }
                }
            });
            return output;
        }
        public void AdjustWeights(Kernel kernel)
        {
            for (int channel = 0; channel < kernel.channels; channel++)
            {
                for (int y = 0; y < kernel.size; y++)
                {
                    for (int x = 0; x < kernel.size; x++)
                    {

                    }
                }
            }
        }

        protected double[,,] Convolution(List<Kernel> filters, int outputLayers)
        {
            //mess around with the bias later duderino
            var bias = 1;
            var output = new double[(inputVolume.GetLength(0) - 3 + 2 * zeroPadding[0]) / stride + 1, (inputVolume.GetLength(1) - 3 + 2 * zeroPadding[0]) / stride + 1, outputLayers];

            for (int filterN = 0; filterN < outputLayers; filterN++)
            {
                for (int channel = 0; channel < inputVolume.GetLength(2); channel++)
                {
                    for (int verticalOffset = 0; verticalOffset < inputVolume.GetLength(1) - filters[filterN].size + zeroPadding[0]; verticalOffset += stride)
                    {
                        for (int horizontalOffset = 0; horizontalOffset < inputVolume.GetLength(0) - filters[filterN].size + zeroPadding[0]; horizontalOffset += stride)
                        {
                            for (int y = 0; y < filters[filterN].size; y++)
                            {
                                for (int x = 0; x < filters[filterN].size; x++)
                                {

                                    output[horizontalOffset, verticalOffset, filterN] += inputVolume[horizontalOffset + x, verticalOffset + y, channel] * filters[filterN].kernelWeights[x, y, channel];

                                }
                            }
                            output[horizontalOffset, verticalOffset, filterN] += bias;
                        }
                    }
                }
            }
            /*
            for (int i = 0; i < output.GetLength(2); i++)
            {
                Debug.Log("Output matrix " + i + ":");
                for (int y = 0; y < output.GetLength(1); y++)
                {
                    for (int x = 0; x < output.GetLength(0); x++)
                    {
                        debuggerino += output[x, y, i] + ",";
                    }
                    debuggerino += "\n";
                }
                Debug.Log(debuggerino);
                debuggerino = "";
            }
            //*/
            return output;
        }
        public void PrintOutput()
        {
            for (int i = 0; i < outputVolume.GetLength(2); i++)
            {
                Debug.Log("Output matrix " + i + ":");
                for (int y = 0; y < outputVolume.GetLength(1); y++)
                {
                    for (int x = 0; x < outputVolume.GetLength(0); x++)
                    {
                        debuggerino += outputVolume[x, y, i] + ",";
                    }
                    debuggerino += "\n";
                }
                Debug.Log(debuggerino);
                debuggerino = "";
            }
        }
        
    }

    //pooling layer, classes of this sort have stride and window size as hyperparameters and should only be concerned with its own feedforward and backpropagation
    private class PoolingLayer
    {
        double[,,] input;
        double[,,] output;
        int stride, windowSize;
        string debuggerino = "";
        //making up this hyperparameter so we can have both average pooling and max pooling
        private PoolingMode mode;

        //this thingy here will try to keep track of all the places where a maximum was located, useful (maybe?) when ding backprop
        public List<double[]> maxCoordinates;

        public double[,,] deltas;

        public PoolingLayer(double[,,] input, PoolingMode mode, int stride, int windowSize)
        {
            this.input = input;
            this.mode = mode;
            this.stride = stride;
            this.windowSize = windowSize;
            maxCoordinates = new List<double[]>();
            this.deltas = new double[input.GetLength(0), input.GetLength(1), input.GetLength(2)];
            output = new double[(input.GetLength(0) - windowSize) / stride - 1, (input.GetLength(1) - windowSize) / stride - 1, input.GetLength(2)];
            if (this.mode == PoolingMode.average)
            {
                AveragePooling();
            }
            else
            {
                MaximumPooling();
            }
        }
        private void AveragePooling()
        {
            for (int layer = 0; layer < input.GetLength(2); layer++)
            {
                for (int verticalOffset = 0; verticalOffset < input.GetLength(1); verticalOffset += stride)
                {
                    for (int horizontalOffset = 0; horizontalOffset < input.GetLength(0); horizontalOffset += stride)
                    {
                        for (int y = 0; y < output.GetLength(1); y++)
                        {
                            for (int x = 0; x < output.GetLength(0); x++)
                            {
                                output[x, y, layer] += input[x + horizontalOffset, y + verticalOffset, layer] / (windowSize * windowSize);
                            }
                        }
                    }
                }
            }

        }




        private void MaximumPooling()
        {
            if (maxCoordinates == null)
            {
                maxCoordinates = new List<double[]>();
            }
            var auxX = 0; var auxY = 0;
            for (int layer = 0; layer < input.GetLength(2); layer++)
            {
                for (int verticalOffset = 0; verticalOffset < output.GetLength(1); verticalOffset++)
                {
                    for (int horizontalOffset = 0; horizontalOffset < output.GetLength(0); horizontalOffset++)
                    {
                        var max = double.MinValue;
                        for (int y = 0; y < windowSize; y++)
                        {
                            for (int x = 0; x < windowSize; x++)
                            {
                                if (input[horizontalOffset * stride + x, verticalOffset * stride + y, layer] > max)
                                {
                                    max = input[horizontalOffset * stride + x, verticalOffset * stride + y, layer];
                                    //Debug.Log("x:"+(x + horizontalOffset / 2)+", y:" +(y + verticalOffset / 2) +", layer" + (layer));
                                    auxX = x;
                                    auxY = y;
                                }

                            }
                        }
                        output[horizontalOffset, verticalOffset, layer] = max;
                        maxCoordinates.Add(new double[3] { horizontalOffset * stride + auxX, verticalOffset * stride + auxY, layer });
                    }
                }
            }
            /*
            for (int i = 0; i < output.GetLength(2); i++)
            {
                Debug.Log("Output matrix " + i + ":");
                for (int y = 0; y < output.GetLength(1); y++)
                {
                    for (int x = 0; x < output.GetLength(0); x++)
                    {
                        debuggerino += output[x, y, i] + ",";
                    }
                    debuggerino += "\n";
                }
                Debug.Log(debuggerino);
                debuggerino = "";
            }
            //*/
        }
        public void FeedForward(double[,,] input)
        {
            this.input = input;
            if (this.mode == PoolingMode.maximum)
            {
                MaximumPooling();
            }
            else
            {
                AveragePooling();
            }
        }
        public double[,,] FeedForward()
        {
            return output;
        }


        //function meant to feedforward in the very last pooling layer, where it connects to the fully connected layer
        //気をつけてくださいね
        public double[] FeedForwardToMLP()
        {
            var outputFlattened = new double[this.output.GetLength(0) * this.output.GetLength(1) * this.output.GetLength(2)];
            for (int layer = 0; layer < output.GetLength(2); layer++)
            {
                for (int y = 0; y < output.GetLength(1); y++)
                {
                    for (int x = 0; x < output.GetLength(0); x++)
                    {
                        outputFlattened[x + y * output.GetLength(0) + layer * output.GetLength(0) * output.GetLength(1)] = output[x, y, layer];
                    }
                }
            }
            return outputFlattened;
        }


        public void PrintOutput()
        {
            for (int i = 0; i < output.GetLength(2); i++)
            {
                Debug.Log("Output matrix " + i + ":");
                for (int y = 0; y < output.GetLength(1); y++)
                {
                    for (int x = 0; x < output.GetLength(0); x++)
                    {
                        debuggerino += output[x, y, i] + ",";
                    }
                    debuggerino += "\n";
                }
                Debug.Log(debuggerino);
                debuggerino = "";
            }
        }
        public void BackPropMLP(double[,] weightsDelta)
        {
            for (int layer = 0; layer < input.GetLength(2); layer++)
            {
                for (int y = 0; y < output.GetLength(1); y++)
                {
                    for (int x = 0; x < output.GetLength(0); x++)
                    {
                        for (int yy = 0; yy < windowSize; yy++)
                        {
                            for (int xx = 0; xx < windowSize; xx++)
                            {
                                //if it's the spot where the max for this window is, then we pass the gradient of the forward layer, otherwise we'll pass a big fat zero
                                if (x * windowSize + xx == maxCoordinates[x + y * output.GetLength(0) + layer * output.GetLength(0) * output.GetLength(1)][0] && y * windowSize + yy == maxCoordinates[x + y * output.GetLength(0) + layer * output.GetLength(0) * output.GetLength(1)][1] && layer == maxCoordinates[x + y * output.GetLength(0) + layer * output.GetLength(0) * output.GetLength(1)][3])
                                {
                                    for (int i = 0; i < weightsDelta.GetLength(0); i++)
                                    {
                                        deltas[y * windowSize + yy, x * windowSize + xx, layer] += weightsDelta[i, xx + yy * windowSize + x * windowSize * windowSize + y * windowSize * windowSize * output.GetLength(0) + layer * windowSize * windowSize * output.GetLength(0) * output.GetLength(1)];
                                    }
                                }
                                else
                                {
                                    deltas[y * windowSize + yy, x * windowSize + xx, layer] = 0;
                                }
                            }
                        }
                    }
                }
            }
        }
        public void BackPropConv(double[,,] gradientsForward)
        {
            for (int layer = 0; layer < input.GetLength(2); layer++)
            {
                for (int y = 0; y < input.GetLength(1); y++)
                {
                    for (int x = 0; x < input.GetLength(0); x++)
                    {
                        for (int yy = 0; yy < windowSize; yy++)
                        {
                            for (int xx = 0; xx < windowSize; xx++)
                            {
                                //if it's the spot where the max for this window is, then we pass the gradient of the forward layer, otherwise we'll pass a big fat zero
                                if (x * windowSize + xx == maxCoordinates[x + y * output.GetLength(0) + layer * output.GetLength(0) * output.GetLength(1)][0] && y * windowSize + yy == maxCoordinates[x + y * output.GetLength(0) + layer * output.GetLength(0) * output.GetLength(1)][1] && layer == maxCoordinates[x + y * output.GetLength(0) + layer * output.GetLength(0) * output.GetLength(1)][3])
                                {
                                    for (int gradChannel = 0; gradChannel < gradientsForward.GetLength(2); gradChannel++)
                                    {
                                        for (int gradX = 0; gradX < gradientsForward.GetLength(1); gradX++)
                                        {
                                            for (int gradY = 0; gradY < gradientsForward.GetLength(0); gradY++)
                                            {
                                                deltas[x * windowSize + xx, y * windowSize + yy, layer] += gradientsForward[gradX, gradY, gradChannel];
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    deltas[x * windowSize + xx, y * windowSize + yy, layer] = 0;
                                }
                            }
                        }
                    }
                }
            }
        }
    }



    //each individual fully connected (MLP) layer in the neural network
    private class Layer
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
    public static class FlattenLayer
    {
        public static double[] Flatten(double[,,] input)
        {
            var output = new double[input.GetLength(0) * input.GetLength(1) * input.GetLength(2)];
            for (int layer = 0; layer < input.GetLength(2); layer++)
            {
                for (int y = 0; y < input.GetLength(1); y++)
                {
                    for (int x = 0; x < input.GetLength(0); x++)
                    {
                        output[x + y * input.GetLength(0) + layer * input.GetLength(0) * input.GetLength(1)] = input[x, y, layer];
                    }
                }
            }
            return output;
        }
    }
}