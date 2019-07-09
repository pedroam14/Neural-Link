#if UNITY_EDITOR
using System;
using UnityEngine;
using Unity;
using System.Collections.Generic;
using UnityEditor;
using SharpGAN.Model;
using SharpGAN.Layer;
using SharpGAN.CostFunctions;
using SharpGAN.Layers.ActivationFunctions;
using SharpGAN.Optimizers;
using SharpGAN.Common;
public class GAN : MonoBehaviour
{
    public Texture2D[] inputImages;
    //public DNet dNet = new DNet(DNet.Activation.LeakyReLU);
    SequentialModel dNet = new SequentialModel();
    SequentialModel gNet = new SequentialModel();
    public Texture2D textureFromNoise;
    public int epochs;
    public double guess1, guess2;
    public int dLosses, gLosses, tie;
    double[][][][] inputVolume;
    public Dimension inputDimension;
    public int rounds;
    private double[][][][] noiseVolume;
    double[][][][] trueImage;
    double[][][][] falseImage;

    public void RunGAN()
    {
        ClearNetworks();
        dLosses = 0; tie = 0; gLosses = 0;
        System.Random random = new System.Random();
        //the dimension in this case will have a depth of 4 since it is an ARGB image, thus having 3 color channels and one alpha channel
        inputDimension = new Dimension(imageCount: 1, depth: 4, width: inputImages[0].width, height: inputImages[0].height);
        inputVolume = Utils.Init4dArr(1, 4, inputDimension.width, inputDimension.height);
        noiseVolume = Utils.Init4dArr(1, 4, inputDimension.width, inputDimension.height);
        CreateDNet();
        CreateGNet();
        for (int i = 0; i < epochs; i++)
        {
            var pixels = inputImages[random.Next(0, inputImages.Length - 1)].GetPixels();
            for (int y = 0; y < inputDimension.height; y++)
            {
                for (int x = 0; x < inputDimension.width; x++)
                {
                    inputVolume[0][0][x][y] = pixels[x + y * inputDimension.width].a;
                    inputVolume[0][1][x][y] = pixels[x + y * inputDimension.width].r;
                    inputVolume[0][2][x][y] = pixels[x + y * inputDimension.width].g;
                    inputVolume[0][3][x][y] = pixels[x + y * inputDimension.width].b;
                }
            }
            for (int y = 0; y < inputDimension.height; y++)
            {
                for (int x = 0; x < inputDimension.width; x++)
                {
                    noiseVolume[0][0][x][y] = Mathf.PerlinNoise((float)x / (float)inputDimension.width * (float)random.NextDouble(), (float)y / (float)inputDimension.height);
                    noiseVolume[0][1][x][y] = Mathf.PerlinNoise((float)x / (float)inputDimension.width * (float)random.NextDouble(), (float)y / (float)inputDimension.height);
                    noiseVolume[0][2][x][y] = Mathf.PerlinNoise((float)x / (float)inputDimension.width * (float)random.NextDouble(), (float)y / (float)inputDimension.height);
                    noiseVolume[0][3][x][y] = Mathf.PerlinNoise((float)x / (float)inputDimension.width * (float)random.NextDouble(), (float)y / (float)inputDimension.height);
                }
            }
            var detectedImage = dNet.ForwardPropagation(input: inputVolume);
            trueImage = Utils.Init4dArr(detectedImage.Length, detectedImage[0].Length, detectedImage[0][0].Length, detectedImage[0][0][0].Length);
            falseImage = trueImage;
            trueImage[0][0][0][0] = 1;
            guess1 = detectedImage[0][0][0][0];
            var generatedImage = gNet.ForwardPropagation(noiseVolume);
            var generatedImage2 = dNet.ForwardPropagation(generatedImage);
            guess2 = generatedImage2[0][0][0][0];

            if (guess1 > guess2)
            {
                gLosses++;
                gNet.GBackwardPropagation(actual: generatedImage, expected: inputVolume);
            }
            else if (guess1 == guess2)
            {
                tie++;
                Debug.Log("Tie!");
                SetPixels(textureFromNoise, generatedImage);
                break;
            }
            else
            {
                dLosses++;
                dNet.DBackwardPropagation(actual: detectedImage, expected: trueImage);
                dNet.DBackwardPropagation(actual: generatedImage2, expected: falseImage);
            }
        }
    }

    private void SetPixels(Texture2D textureFromNoise, double[][][][] generatedImage)
    {
        Color[] colors = new Color[inputDimension.width * inputDimension.height];
        for (int y = 0; y < inputDimension.height; y++)
        {
            for (int x = 0; x < inputDimension.width; x++)
            {
                colors[x + y * inputDimension.width].a = Mathf.Clamp((float)generatedImage[0][0][x][y], 0, 1);
                colors[x + y * inputDimension.width].r = Mathf.Clamp((float)generatedImage[0][1][x][y], 0, 1);
                colors[x + y * inputDimension.width].g = Mathf.Clamp((float)generatedImage[0][2][x][y], 0, 1);
                colors[x + y * inputDimension.width].b = Mathf.Clamp((float)generatedImage[0][3][x][y], 0, 1);
            }
        }
        textureFromNoise.SetPixels(colors);
        textureFromNoise.Apply();
        Debug.Log("Complete :)");
    }

    private void ClearNetworks()
    {
        gNet.Clear();
        dNet.Clear();
    }

    private void CreateGNet()
    {
        ConvolutionLayer conv1 = new ConvolutionLayer(inputDimension, filterSize: 3, filterCount: 4, zeroPadding: true);
        ActivationLayer act = new ActivationLayer(new Relu(leaky: true));
        ConvolutionLayer conv = new ConvolutionLayer(inputDimension, filterSize: 3, filterCount: 4, zeroPadding: true);
        ActivationLayer act1 = new ActivationLayer(new Relu(leaky: true));
        ConvolutionLayer convolutionOut = new ConvolutionLayer(inputDimension, filterSize: 3, filterCount: 4, zeroPadding: true);
        ActivationLayer actOut = new ActivationLayer(new Relu(leaky: true));
        gNet.Add(conv1);
        gNet.Add(act);
        gNet.Add(conv);
        gNet.Add(act1);
        gNet.Add(convolutionOut);
        gNet.Compile(new BinaryCrossEntropy(), new Adam(0.001d));
    }

    private void CreateDNet()
    {
        ConvolutionLayer conv0 = new ConvolutionLayer(inputDimension, filterSize: 3, filterCount: 32, zeroPadding: true);
        ActivationLayer activation0 = new ActivationLayer(new Relu(leaky: true));
        MaxPooling2DLayer pool0 = new MaxPooling2DLayer();
        ConvolutionLayer conv1 = new ConvolutionLayer(inputDimension, filterSize: 3, filterCount: 32, zeroPadding: true);
        ActivationLayer activation1 = new ActivationLayer(new Relu(leaky: true));
        MaxPooling2DLayer pool1 = new MaxPooling2DLayer();
        FlattenLayer flatten = new FlattenLayer();
        LinearLayer linear0 = new LinearLayer(numNeurons: 128);
        ActivationLayer activation2 = new ActivationLayer(new Relu(leaky: true));
        LinearLayer linear1 = new LinearLayer(numNeurons: 1);
        ActivationLayer activation3 = new ActivationLayer(new Sigmoid());
        dNet.Add(conv0);
        dNet.Add(activation0);
        dNet.Add(pool0);
        dNet.Add(conv1);
        dNet.Add(activation1);
        dNet.Add(pool1);
        dNet.Add(flatten);
        dNet.Add(linear0);
        dNet.Add(activation2);
        dNet.Add(linear1);
        dNet.Add(activation3);
        dNet.Compile(new BinaryCrossEntropy(), new Adam(0.001d));
    }
}
[CustomEditor(typeof(GAN))]
public class GANEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GAN gAN = (GAN)target;
        if (GUILayout.Button("Run GAN"))
        {
            gAN.RunGAN();
        }
    }
}
#endif