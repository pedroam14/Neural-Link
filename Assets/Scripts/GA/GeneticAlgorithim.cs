using System;
using System.Collections.Generic;
using System.Threading.Tasks;
public class GeneticAlgorithim<T>
{
    public List<DNA<T>> population { get; private set; }
    public List<DNA<T>> newPopulation;
    public int generation { get; private set; }
    public float bestFitness { get; private set; }
    public T[] bestGenes { get; private set; }
    public float mutationRate;
    private float fitnessSum;
    public int elitism;
    private int dnaSize;
    private Func<T> GetRandomGene;
    private Random rand = new Random(); 
    private Func<int, float> FitnessFunction;
    public GeneticAlgorithim(int populationSize, int dnaSize, Func<T> GetRandomGene, Func<int, float> FitnessFunction, int _elitism, float mutationRate = 0.01f)
    {
        generation = 1;
        this.mutationRate = mutationRate;
        population = new List<DNA<T>>(populationSize);
        newPopulation = new List<DNA<T>>(populationSize);
        bestGenes = new T[dnaSize];
        this.GetRandomGene = GetRandomGene;
        this.FitnessFunction = FitnessFunction;
        this.dnaSize = dnaSize;
        elitism = _elitism;
        Parallel.For (0, populationSize,i=>
        {
            population.Add(new DNA<T>(dnaSize, GetRandomGene, FitnessFunction, shouldInitGenes: true));
        });
    }
    public void NewGeneration(int numNewDNA = 0, bool crossoverNewDNA = false)
    {
        int finalCount = population.Count + numNewDNA;
        if (finalCount <= 0)
        {
            return;
        }
        if (population.Count > 0)
        {
            CalculateFitness();
            population.Sort(CompareDNA);
        }
        newPopulation.Clear();
        for (int i = 0; i < finalCount; ++i)
        {
            if (i < elitism && i < population.Count)
            {
                newPopulation.Add(population[i]);
            }
            else if (i < population.Count||crossoverNewDNA)
            {
                DNA<T> parent1 = ChooseParent();
                DNA<T> parent2 = ChooseParent();
                DNA<T> child = parent1.Crossover(parent2);
                child.Mutate(mutationRate);
                newPopulation.Add(child);
            }
            else
            {
                population.Add(new DNA<T>(dnaSize,GetRandomGene,FitnessFunction,shouldInitGenes:true));
            }
        }
        List<DNA<T>> tempList = population;
        population = newPopulation;
        newPopulation = tempList;
        generation++;
    }
    public void CalculateFitness()
    {
        fitnessSum = 0;
        DNA<T> best = population[0];
        Parallel.For (0, population.Count,i=>
        {
            fitnessSum += population[i].CalcFitness(i);
            if (population[i].fitness > best.fitness)
            {
                best = population[i];
            }
        });
        bestFitness = best.fitness;
        best.genes.CopyTo(bestGenes, 0);
    }
    public int CompareDNA(DNA<T> a, DNA<T> b)
    {
        if (a.fitness > b.fitness)
        {
            return -1;
        }
        else if (a.fitness < b.fitness)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
    private DNA<T> ChooseParent()
    {
        double randNumber = rand.NextDouble() * fitnessSum;
        for (int i = 0; i < population.Count; ++i)
        {
            if (randNumber < population[i].fitness)
            {
                return population[i];
            }
            randNumber -= population[i].fitness;
        }
        return null;
    }
}