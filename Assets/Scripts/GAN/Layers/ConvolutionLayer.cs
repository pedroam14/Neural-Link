﻿using System;
using SharpGAN.Common;
using SharpGAN.Initializers;
using SharpGAN.Logger;
using SharpGAN.Regularizers;

namespace SharpGAN.Layer
{
    /// <summary>
    /// Class for realization of convolution layer in CNN for 3D data
    /// </summary>
    [Serializable]
    public class ConvolutionLayer : AbstractLayer, ILearnable
    {

        #region Constants
        /// <summary>
        /// Default filter size (depth of conv layer)
        /// </summary>
        private const int DEFAULT_FILTER_SIZE = 3;
        /// <summary>
        /// Default stride of filter in conv layer
        /// </summary>
        private const int DEFAULT_STRIDE = 1;
        /// <summary>
        /// Default information about usage of zero padding in
        /// conv layer
        /// </summary>
        private const bool DEFAULT_ZERO_PADDING = false;
        /// <summary>
        /// Default filters count, how many filter will be init
        /// if user does not set it
        /// </summary>
        private const int DEFAULT_FILTER_COUNT = 20;
        /// <summary>
        /// Name of layer which will be displayed in summary
        /// </summary>
        public const string CONV_LAYER_NAME = "Convolution";
        #endregion

        #region Local variables
        /// <summary>
        /// Public property which contains input dimension of layer
        /// or null if is not already set (should be set during call
        /// compile)
        /// </summary>
        public Dimension InputDimension
        {
            get { return inputDimension; }
            set { inputDimension = value; }
        }
        /// <summary>
        /// Size of used filters
        /// </summary>
        private int filterSize;
        /// <summary>
        /// Stride of filters which will be used
        /// </summary>
        private int stride;
        /// <summary>
        /// Zero padding value, if is true zero-padding will be used
        /// </summary>
        private bool zeroPadding;
        /// <summary>
        /// Size of output from the CONV layer
        /// </summary>
        private int outputSizeW;
        /// <summary>
        /// Count of filters which will be used
        /// </summary>
        private int filterCount;
        /// <summary>
        /// Filters weights which are used for convolution and trained
        /// during apllying backpropagation algorithm
        /// </summary>
        private double[][][][] filters;
        /// <summary>
        /// Public property for weights/filters which is used during
        /// learning.
        /// </summary>
        public double[][][][] Weights
        {
            get { return filters; }
            set { filters = value; }
        }
        /// <summary>
        /// Biases which will be used for computing convolution
        /// </summary>
        private double[] biases;
        /// <summary>
        /// Public property for biases which is used for
        /// biases updates during learning.
        /// </summary>
        public double[] Biases
        {
            get { return biases; }
            set { biases = value; }
        }
        /// <summary>
        /// Weight gradient of current layer.
        /// </summary>
        private double[][][][] dWeights;
        /// <summary>
        /// Public property for weights which is used for
        /// weights updates during learning.
        /// </summary>
        public double[][][][] Dweights
        {
            get { return dWeights; }
            set { }
        }
        /// <summary>
        /// Bias gradient of current layer.
        /// </summary>
        private double[] dBiases;
        /// <summary>
        /// Public property for biases gradient which is used
        /// for biases updates during learning.
        /// </summary>
        public double[] Dbiases
        {
            get { return dBiases; }
            set { }
        }
        /// <summary>
        /// Last input into the layer.
        /// </summary>
        private double[][][][] lastInput;
        /// <summary>
        /// Instance of initializer which is used for
        /// initialization of biases
        /// </summary>
        private AbstractBiasInitializer biasInitializer;
        /// <summary>
        /// Instance of initializer which is used for
        /// initialization of weights
        /// </summary>
        private AbstractWeightInitializer weightInitializer;
        /// <summary>
        /// Instance of regularizer, which can be used according
        /// to setting of layer
        /// </summary>
        private AbstractRegularizer regularizer;
        /// <summary>
        /// Public property for regularizer.
        /// </summary>
        public AbstractRegularizer Regularizer
        {
            get { return regularizer; }
            set { regularizer = value; }
        }
        #endregion

        /// <summary>
        /// Constructor for initialization of convolution layer
        /// </summary>
        /// <param name="inputDimension">dimension of input data</param>
        /// <param name="filterSize">size of filters which will be used</param>
        /// <param name="filterCount">count of filters which will be used</param>
        /// <param name="stride">stride of filters during convolution</param>
        /// <param name="zeroPadding">if is true zero padding will be used</param>
        public ConvolutionLayer(Dimension inputDimension = null, int filterSize = DEFAULT_FILTER_SIZE,
            int filterCount = DEFAULT_FILTER_COUNT, int stride = DEFAULT_STRIDE,
            bool zeroPadding = DEFAULT_ZERO_PADDING, AbstractBiasInitializer biasInitializer = null,
            AbstractWeightInitializer weightInitializer = null, AbstractRegularizer regularizer = null)
        {
            // init vaiiables
            this.filterSize = filterSize;
            this.filterCount = filterCount;
            this.stride = stride;
            this.zeroPadding = zeroPadding;
            this.inputDimension = inputDimension;
            this.regularizer = regularizer;
            layerName = CONV_LAYER_NAME;
            outputSizeW = 0;

            this.biasInitializer = biasInitializer;
            this.weightInitializer = weightInitializer;
        }

        public override double[][][][] ForwardPropagation(double[][][][] input, int startIndex, int endIndex, bool predict = false)
        {
            #region Test hyperparameters
            int currImageCount = input.Length;
            int currDepth = input[0].Length;
            int currWidth = input[0][0].Length;
            int currHeight = input[0][0][0].Length;

            if (!IsInputDataSameAsDim(currDepth, currWidth, currHeight))
            {
                string msg = "Input data have not same dimension as initialized dimension in convolution layer.";
                Utils.ThrowException(msg);
            }

            if (startIndex == DEFAULT_INDEX && endIndex == startIndex)
            {
                startIndex = 0;
                endIndex = currImageCount;
            }
            else
            {
                currImageCount = endIndex - startIndex;
            }

            if (IsDimensionChanged(currImageCount, currDepth, currWidth, currHeight))
            {
                activations = Utils.Init4dArr(inputDimension.imageCount, filterCount, outputSizeW, outputSizeW);
            }


            // TestDimension(currImageCount, currDepth, currWidth, currHeight);
            TestHyperParametersAndSetOutputSize(currWidth, currHeight);
            #endregion

            // save input for backpropagation step
            lastInput = input;
            // temp indexes
            int yStart, yEnd, xStart, xEnd, xIndex, yIndex, filterIndexX, filterIndexY;
            // stride to left and right from center item _ X _
            // (middle X middle)
            int middle = (int)filterSize / 2;
            // starting point from convolution filter
            int start = zeroPadding ? 0 : middle;
            // sum of convolution for current filter
            double sum;
            // activation index - needed if we have batches
            int activationIndex = 0;
            // iterate through all training images
            for (int imageIndex = startIndex; imageIndex < endIndex; imageIndex++)
            {
                // iterate through all filters
                for (int filterIndex = 0; filterIndex < filterCount; filterIndex++)
                {
                    yIndex = 0;
                    // iterate through height of current image according to stride
                    for (int y = start; y < currHeight - start; y += stride)
                    {
                        yStart = y - middle;
                        yEnd = filterSize % 2 == 0 ? y + middle : y + middle + 1;
                        xIndex = 0;
                        // iterate through width of current image according to stride
                        for (int x = start; x < currWidth - start; x += stride)
                        {
                            sum = 0;
                            xStart = x - middle;
                            // iterate through all channels of current pixel
                            for (int channel = 0; channel < currDepth; channel++)
                            {
                                xEnd = filterSize % 2 == 0 ? x + middle : x + middle + 1;
                                filterIndexY = 0;
                                // sum(X[:filterSize,:filterSize,:channels] * Wx) + bx
                                for (int yCurr = yStart; yCurr < yEnd; yCurr++)
                                {
                                    filterIndexX = 0;
                                    for (int xCurr = xStart; xCurr < xEnd; xCurr++)
                                    {
                                        // convolution through channels
                                        if (xCurr > -1 && yCurr > -1 && xCurr < currWidth && yCurr < currHeight)
                                        {
                                            sum += input[imageIndex][channel][xCurr][yCurr] * filters[filterIndex][channel][filterIndexX][filterIndexY];
                                        }
                                        filterIndexX++;
                                    }
                                    filterIndexY++;
                                }
                            }
                            // add bias and save the convolution value
                            activations[activationIndex][filterIndex][xIndex][yIndex] = sum + biases[filterIndex];
                            xIndex++;
                        }
                        yIndex++;
                    }
                }
                // increase index of currently processed image
                activationIndex++;
            }

            return activations;
        }

        public override double[][][][] BackwardPropagation(double[][][][] input, int startIndex, int endIndex)
        {
            int currImageCount = input.Length;
            int currWidth = input[0][0].Length;
            int currHeight = input[0][0][0].Length;
            int filtersChannels = filters[0].Length;
            int filtersCount = filters.Length;
            int filterWidth = filters[0][0].Length;
            int filterHeight = filters[0][0][0].Length;
            int middleWidthOfFilter = filterWidth / 2;
            int middleHeightOfFilter = filterHeight / 2;

            double normValue = 1 / currImageCount;

            double gradientValue;
            int yOffsetMin, yOffsetMax, xOffsetMin, xOffsetMax, imgY, imgX, subY, subX;
            double[][][][] imageGradient = Utils.Init4dArr(inputDimension.imageCount, inputDimension.depth,
                inputDimension.width, inputDimension.height);
            dWeights = Utils.Init4dArr(filtersCount, filtersChannels, filterWidth, filterHeight);
            for (int i = 0; i < currImageCount; i++)
            {
                for (int convChannel = 0; convChannel < filtersChannels; convChannel++)
                {
                    for (int y = 0; y < currWidth; y++)
                    {
                        yOffsetMin = Math.Max(-y, -middleWidthOfFilter);
                        yOffsetMax = Math.Min(currWidth - y, middleWidthOfFilter + 1);
                        for (int x = 0; x < currHeight; x++)
                        {

                            gradientValue = input[i][convChannel][x][y];


                            gradientValue = input[i][convChannel][x][y];
                            xOffsetMin = Math.Max(-x, -middleHeightOfFilter);
                            xOffsetMax = Math.Min(currHeight - x, middleHeightOfFilter + 1);
                            for (int verticalOffset = yOffsetMin; verticalOffset < yOffsetMax; verticalOffset++)
                            {
                                for (int horizontalOffset = xOffsetMin; horizontalOffset < xOffsetMax; horizontalOffset++)
                                {
                                    imgY = y + verticalOffset;
                                    imgX = x + horizontalOffset;
                                    subY = middleHeightOfFilter + verticalOffset;
                                    subX = middleWidthOfFilter + horizontalOffset;
                                    for (int imageChannel = 0; imageChannel < filtersChannels; imageChannel++)
                                    {
                                        imageGradient[i][imageChannel][imgY][imgX] += filters[imageChannel][convChannel][subY][subX] * gradientValue;
                                        dWeights[imageChannel][convChannel][subY][subX] += (lastInput[i][imageChannel][imgY][imgX] * gradientValue) * normValue;

                                        //regularization 
                                        //http://cs231n.github.io/neural-networks-2/#reg それは必要ないですけれどもそれを読んで下さい
                                        if (regularizer != null)
                                        {
                                            //lambda/m * W

                                            //clean version with some method calls, both add the regularization coef
                                            double[][][][] regMatrix = regularizer.Regularize(lastInput, currImageCount);
                                            dWeights = MatOp.Add(dWeights, MatOp.Transpose(regMatrix));
                                            /* 
                                            //if you're going to use this version instead, transpose the regMatrix, also comment the regmatrix above
                                            regMatrix = MatOp.Transpose(regularizer.Regularize(lastInput, currImageCount));
                                            for (int dL0 = 0; dL0 < dWeights.Length; dL0++)
                                            {
                                                for (int dL1 = 0; dL1 < dWeights[0].Length; dL1++)
                                                {
                                                    for (int dL2 = 0; dL2 < dWeights[0][0].Length; dL2++)
                                                    {
                                                        for (int dL3 = 0; dL3 < dWeights[0][0][0].Length; dL3++)
                                                        {
                                                            dWeights[dL0][dL1][dL2][dL3] = dWeights[dL0][dL1][dL2][dL3] + regMatrix[dL0][dL1][dL2][dL3];
                                                        }
                                                    }
                                                }
                                            }
                                            //*/
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            dBiases = SumBiases(input, biases.Length, currImageCount);
            dWeights = MatOp.Substract(dWeights, filters);

            return imageGradient;
        }

        public override AbstractLayer Compile(AbstractLayer previousLayer)
        {
            if (previousLayer == null && inputDimension == null)
            {
                string msg = "Dimension of first layer of network not set!";
                Utils.ThrowException(msg);
            }
            else if (previousLayer != null)
            {
                inputDimension = previousLayer.outputDimension;
                index = previousLayer.Index + 1;
            }

            TestHyperParametersAndSetOutputSize(this.inputDimension.width, this.inputDimension.height);

            outputDimension = new Dimension(this.inputDimension.imageCount, filterCount,
                 outputSizeW, outputSizeW);
            #region Init initializers
            if (biasInitializer == null)
            {
                biasInitializer = new ConstantBiasInitializer(0d);
            }
            biases = biasInitializer.Initialize(filterCount);

            //https://stats.stackexchange.com/questions/198840/cnn-xavier-weight-initialization
            //https://github.com/keras-team/keras/blob/998efc04eefa0c14057c1fa87cab71df5b24bf7e/keras/initializations.py
            int fanIn = filterSize * filterSize * inputDimension.depth;
            int fanOut = filterCount;
            if (weightInitializer == null)
            {
                weightInitializer = new XavierWeightInitializer(fanIn, fanOut);
            }
            #endregion Init initializers

            filters = weightInitializer.Initialize(new Dimension(filterCount, inputDimension.depth, filterSize, filterSize));

            //activations will be init
            activations = Utils.Init4dArr(inputDimension.imageCount, filterCount, outputSizeW, outputSizeW);
            compiled = true;

            return this;
        }

        #region Initialization for testing
        /// <summary>
        /// Set filters according to example in
        /// http://cs231n.github.io/convolutional-networks/
        /// after that we can check the functionality of
        /// forward convolution
        /// </summary>
        /// <param name="channels">number of used channels</param>
        private void InitTestFilters(int channels)
        {
            filters = new double[][][][]{
                // two weigts 3x3 filters with dimension of 3
                new double[][][] {
                    // three elements for width
                    new double[][] {
                        // three elements for height
                        new double[] { 0, 1, 1 },
                        new double[] { 1, 1, 1 },
                        new double[] { 1, -1, 0 }
                    },
                    new double[][] {
                        new double[] { 1, 0, -1 },
                        new double[] { 0, -1, 1 },
                        new double[] { 1, -1, 0 }
                    },
                    new double[][] {
                        new double[] { 0, -1, -1 },
                        new double[] { 1, 0, 0 },
                        new double[] { 0, -1, -1 }
                    }
                },
                new double[][][] {
                    // three elements for width
                    new double[][] {
                        // three elements for height
                        new double[] { 1, 1, 0 },
                        new double[] { -1, 1, 0 },
                        new double[] { -1, 0, 1 }
                    },
                    new double[][] {
                        new double[] { 0, 0, -1 },
                        new double[] { -1, -1, 0 },
                        new double[] { -1, 0, 0 }
                    },
                    new double[][] {
                        new double[] { -1, 0, -1 },
                        new double[] { 1, 0, 1 },
                        new double[] { 0, -1, 1 }
                    }
                }
            };
        }

        /// <summary>
        /// Set biases according to example in
        /// http://cs231n.github.io/convolutional-networks/
        /// after that we can check the functionality of
        /// forward convolution
        /// </summary>
        private void InitTestBiases()
        {
            biases = new double[] { 1, 0 };
        }
        #endregion

        #region Test input params
        /// <summary>
        /// Test inserted hyperparameters
        /// if something is wrong program will end
        /// </summary>
        /// <param name="inputWidth">width of input images</param>
        private void TestHyperParametersAndSetOutputSize(int inputWidth, int inputHeight)
        {
            if (inputWidth != inputHeight || inputHeight <= 1)
            {
                UnityEngine.Debug.Log("Expected square image in 3D (channels, width, height) in Convolution2D, let's see how a rectangle goes :)");
                //Utils.ThrowException(msg);
            }

            double paddingVal = this.zeroPadding ? ((filterSize - 1d) / 2d) : 0d;
            if (paddingVal != 0 && paddingVal % Math.Floor(paddingVal) != 0)
            {
                string msg = "It is not possible to use zero-padding parameter.";
                Utils.ThrowException(msg);
            }

            double diff = (double)(inputWidth - this.filterSize + 2 * paddingVal) / this.stride + 1;
            double diff2 = (double)(inputHeight - this.filterSize + 2 * paddingVal) / this.stride + 1;
            if (diff % Math.Floor(diff) != 0 || diff <= 0)
            {
                string msg = "Invalid hyperparameters.";
                Utils.ThrowException(msg);
            }

            outputSizeW = (int)diff;
        }

        /// <summary>
        /// Test dimension of input data and expected dimension
        /// </summary>
        /// <param name="currImageCount">count of images in data (batch size)</param>
        /// <param name="currDepth">depth of images - num of channels</param>
        /// <param name="currWidth">width of images</param>
        /// <param name="currHeight">height of images</param>
        private void TestDimension(int currImageCount, int currDepth,
            int currWidth, int currHeight)
        {
            if (!this.inputDimension.IsSame(currImageCount, currDepth,
                currWidth, currHeight))
            {
                string msg = "Input dimension is different than expected.";
                Utils.ThrowException(msg);
            }
        }

        private bool IsDimensionChanged(int currImageCount, int currDepth, int currWidth, int currHeight)
        {
            if (inputDimension == null || !inputDimension.IsSame(currImageCount, currDepth, currWidth, currHeight))
            {
                inputDimension = new Dimension();
                inputDimension.imageCount = currImageCount;
                inputDimension.depth = currDepth;
                inputDimension.width = currWidth;
                inputDimension.height = currHeight;
                return true;
            }
            return false;
        }
        #endregion

        public override long Summary()
        {
            int depth = activations[0].GetLength(0);
            int width = activations[0][0].GetLength(0);
            int height = activations[0][0].GetLength(0);
            long parameters = filters.GetLength(0) * filters[0].GetLength(0)
                * filters[0][0].GetLength(0) * filters[0][0][0].GetLength(0)
                + biases.GetLength(0);

            string output = String.Format("{0,-15} {1, -30} {2, -45}", layerName.ToString(),
                "(" + "None" + ", " + width + ", " + height
                + ", " + depth + ")", parameters.ToString());
            Log.Info(output);

            return parameters;
        }

        /// <summary>
        /// Sum up all biases according to size fo bias array.
        /// </summary>
        /// <param name="input">input matrix</param>
        /// <param name="size">size of bias array</param>
        /// <param name="divisor">divisor for possibly normalization</param>
        /// <returns>sum of biases</returns>
        private double[] SumBiases(double[][][][] input, int size, int divisor = 1)
        {
            double multiplicator = 1d / (double)divisor;
            dBiases = new double[size];

            int imageCount = input.Length;
            int filtersCount = input[0].Length;
            int height = input[0][0].Length;
            int width = input[0][0][0].Length;

            for (int i = 0; i < imageCount; i++)
            {
                for (int index = 0; index < filtersCount; index++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        for (int k = 0; k < width; k++)
                        {
                            dBiases[index] += input[i][index][j][k];
                        }
                    }
                }
            }

            for (int i = 0; i < filtersCount; i++)
            {
                dBiases[i] *= multiplicator;
            }

            return dBiases;
        }
    }
}