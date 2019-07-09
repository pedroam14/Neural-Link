using UnityEngine;
using System;


[Serializable]
public class Kernel
{
    public double[,,] kernelWeights;
    public int size, channels;
    public Kernel(int size, int channels)
    {
        if (size % 2 == 0)
        {
            Debug.Log("Invalid Kernel");
        }
        else
        {
            this.size = size;
            this.channels = channels;
            kernelWeights = new double[size, size, channels];
            Randomize();
        }
    }
    public void LoadWeights(double[,,] weights)
    {
        this.kernelWeights = weights;
    }

    private void Randomize()
    {
        for (int channel = 0; channel < channels; channel++)
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    kernelWeights[x, y, channel] = RandomUtil.random.NextDouble();
                }
            }
        }
    }
}