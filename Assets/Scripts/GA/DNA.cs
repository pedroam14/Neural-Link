using System;
using System.Threading.Tasks;
public class DNA<T>
{
    public T[] genes { get; private set; }
    public float fitness { get; private set; }
    private Random rand = new Random();
    private Func<T> GetRandomGene;
    private Func<int, float> FitnessFunction;
    public DNA(int size, Func<T> GetRandomGene, Func<int, float> FitnessFunction, bool shouldInitGenes = true)
    {
        genes = new T[size];
        this.GetRandomGene = GetRandomGene;
        this.FitnessFunction = FitnessFunction;
        if (shouldInitGenes)
        {
            for (int i = 0; i < genes.Length; ++i)
            {
                genes[i] = GetRandomGene();
            }
        }
    }
    public float CalcFitness(int index)
    {
        fitness = FitnessFunction(index);
        return fitness;
    }
    public DNA<T> Crossover(DNA<T> otherParent)
    {
        DNA<T> child = new DNA<T>(genes.Length, GetRandomGene, FitnessFunction, false);
        for (int i = 0; i < genes.Length; ++i)
        {
            child.genes[i] = rand.NextDouble() < 0.5 ? genes[i] : otherParent.genes[i];
        }
        return child;
    }
    public void Mutate(float mutationRate)
    {
        for (int i = 0; i < genes.Length; ++i)
        {
            if (rand.NextDouble() < mutationRate)
            {
                genes[i] = GetRandomGene();
            }
        }

    }
}