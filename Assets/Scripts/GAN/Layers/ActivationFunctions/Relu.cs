using System;

namespace SharpGAN.Layers.ActivationFunctions
{
    /// <summary>
    /// Class Relu for implementation relu activation function
    /// </summary>
    [Serializable]
    public class Relu : ActivationFunction
    {
        /// <summary>
        /// Empty constructor for creating instance of Relu class
        /// for later usage
        /// </summary>
        public bool leaky;
        public Relu(bool leaky)
        {
            this.leaky = leaky;
        }
        double alpha = 0;
        public override double[][][][] Derivate(double[][][][] values)
        {
            int l0 = values.Length;
            int l1 = values[0].Length;
            int l2 = values[0][0].Length;
            int l3 = values[0][0][0].Length;
            if (!leaky)
            {
                for (int i = 0; i < l0; i++)
                {
                    for (int j = 0; j < l1; j++)
                    {
                        for (int k = 0; k < l2; k++)
                        {
                            for (int l = 0; l < l3; l++)
                                // value[value < 0] = alpha
                                values[i][j][k][l] = (values[i][j][k][l] > 0) ? 1 : 0;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < l0; i++)
                {
                    for (int j = 0; j < l1; j++)
                    {
                        for (int k = 0; k < l2; k++)
                        {
                            for (int l = 0; l < l3; l++)
                                // value[value < 0] = alpha
                                values[i][j][k][l] = (values[i][j][k][l] > 0) ? 1 : alpha;
                        }
                    }
                }
            }

            return values;
        }

        public override double[][][][] Compute(double[][][][] values)
        {
            int l0 = values.Length;
            int l1 = values[0].Length;
            int l2 = values[0][0].Length;
            int l3 = values[0][0][0].Length;
            if (!leaky)
            {
                for (int i = 0; i < l0; i++)
                {
                    for (int j = 0; j < l1; j++)
                    {
                        for (int k = 0; k < l2; k++)
                        {
                            for (int l = 0; l < l3; l++)
                            {
                                values[i][j][k][l] = (values[i][j][k][l] > 0) ? values[i][j][k][l] : 0;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < l0; i++)
                {
                    for (int j = 0; j < l1; j++)
                    {
                        for (int k = 0; k < l2; k++)
                        {
                            for (int l = 0; l < l3; l++)
                            {
                                values[i][j][k][l] = (values[i][j][k][l] > 0) ? values[i][j][k][l] : values[i][j][k][l] * alpha;
                            }
                        }
                    }
                }
            }
            return values;
        }
    }
}