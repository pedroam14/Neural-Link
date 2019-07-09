using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TestDungeon : MonoBehaviour
{
    [Header("GA")]
    [SerializeField] string targetString = "これはただのテストですよ";
    [SerializeField] string validCharacters = "かけきこくられりろるまめみもむだでぢどづたてちとつさせしそすざぜじぞずはへひほふなねにのぬカケキコクラレリロルマメミモムダデヂドヅタテチトツサセシソスザゼジゾズハヘヒホフナネニノヌやよゆ";
    [SerializeField] int populationSize = 200;
    [SerializeField] float mutationRate = 0.01f;
    [SerializeField] int elitism = 5;

    [Header("Other")]
    [SerializeField] int numCharsPerText = 15000;
    [SerializeField] Text targetText;
    [SerializeField] Text bestText;
    [SerializeField] Text bestFitnessText;
    [SerializeField] Text numGenText;
    [SerializeField] Transform populationTextParent;
    [SerializeField] Text textPrefab;
    GeneticAlgorithim<char> ga;
    private System.Random random;
    private void Start()
    {
        targetText.text = targetString;
        if (string.IsNullOrEmpty(targetString))
        {
            Debug.Log("You forgot the target string you dummy");
        }
        random = new System.Random();
        ga = new GeneticAlgorithim<char>(populationSize, targetString.Length, GetRandomCharacter, FitnessEvaluation, elitism, mutationRate);
    }
    private char GetRandomCharacter()
    {
        return validCharacters[random.Next(validCharacters.Length)];
    }
    private float FitnessEvaluation(int index)
    {
        float score = 0;
        DNA<char> dna = ga.population[index];
        for (int i = 0; i < ga.population[index].genes.Length; ++i)
        {
            if (dna.genes[i].Equals(targetString[i]))
            {
                score++;
            }
        }
        score /= targetString.Length;
        return score;
    }
    private void Update()
    {
        ga.NewGeneration();
        UpdateText(ga.bestGenes, ga.bestFitness, ga.generation, ga.population.Count, (j) => ga.population[j].genes);
        if (ga.bestFitness == 1)
        {
            this.enabled = false;
        }
    }
    private int numCharsPerTextObj;
    private List<Text> textList = new List<Text>();


    void Awake()
    {
        numCharsPerTextObj = numCharsPerText / validCharacters.Length;
        if (numCharsPerTextObj > populationSize) numCharsPerTextObj = populationSize;

        int numTextObjects = Mathf.CeilToInt((float)populationSize / numCharsPerTextObj);

        for (int i = 0; i < numTextObjects; i++)
        {
            textList.Add(Instantiate(textPrefab, populationTextParent));
        }
    }

    private void UpdateText(char[] bestGenes, float bestFitness, int generation, int populationSize, Func<int, char[]> getGenes)
    {
        bestText.text = CharArrayToString(bestGenes);
        bestFitnessText.text = bestFitness.ToString();

        numGenText.text = generation.ToString();

        for (int i = 0; i < textList.Count; i++)
        {
            var sb = new StringBuilder();
            int endIndex = i == textList.Count - 1 ? populationSize : (i + 1) * numCharsPerTextObj;
            for (int j = i * numCharsPerTextObj; j < endIndex; j++)
            {
                foreach (var c in getGenes(j))
                {
                    sb.Append(c);
                }
                if (j < endIndex - 1) sb.AppendLine();
            }

            textList[i].text = sb.ToString();
        }
    }

    private string CharArrayToString(char[] charArray)
    {
        var sb = new StringBuilder();
        foreach (var c in charArray)
        {
            sb.Append(c);
        }

        return sb.ToString();
    }
}
