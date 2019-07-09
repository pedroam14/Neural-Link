using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

public class EnemyScript : MonoBehaviour
{

    public PlayerState playerState;
    public Transform playerPosition;
    public NeuralNetwork neuralNetwork;
    public int[] layerArchitecture;
    public int epochs = 1000;
    public double[] inputVector;
    StreamReader sr;
    public bool blocking, attacking;
    public double[][] enemyBehavior;
    public double[][] playerActions;
    public double[] outputVector;
    public Animator darkAnimator;
    public double perfectionRate = 0.8f;
    private System.Random rand = new System.Random();
    public AudioManager audioManager;
    public Neurons[] neurons;
    public double range = 1;
    string[] lines;
    private void Awake()
    {
        /*
        sr = new StreamReader("Assets/Presets/PlayerActions.csv");
        playerActions = new double[24][];
        enemyBehavior = new double[24][];
        for (int i = 0; i < playerActions.GetLength(0); i++)
        {
            playerActions[i] = new double[5];
            enemyBehavior[i] = new double[4];
        }
        for (int y = 0; y < playerActions.GetLength(0); y++)
        {
            lines = sr.ReadLine().Split(',');
            for (int z = 0; z < lines.Length; z++)
            {
                playerActions[y][z] = double.Parse(lines[z]); //load every weight stored on the file and parses it as a double
                Debug.Log(playerActions[y][z]);
            }
        }
        sr.Close();
        sr = new StreamReader("Assets/Presets/EnemyReactions.csv");
        for (int y = 0; y < playerActions.GetLength(0); y++)
        {
            lines = sr.ReadLine().Split(',');
            for (int z = 0; z < lines.Length; z++)
            {
                enemyBehavior[y][z] = double.Parse(lines[z]); //load every weight stored on the file and parses it as a double
                Debug.Log(enemyBehavior[y][z]);
            }
        }
        sr.Close();
        //*/

        //Training();

        /*
        Debug.Log("Training Neural Network....");
        for (int i = 0; i < epochs; ++i)
        {
            for (int j = 0; j < playerActions.GetLength(0); j++)
            {
                neuralNetwork.FeedForward(playerActions[j]);
                neuralNetwork.BackProp(enemyBehavior[j]);
            }
        }
        Debug.Log("Network Trained!");
        neuralNetwork.SaveWeights(neurons.weightsList);
        //*/
    }
    public void TrainBrain()
    {
        //        neuralNetwork.LoadWeights(neurons[0].weightsList);
    }
    public void Training()
    {

        sr = new StreamReader("Assets/Presets/PlayerActions.csv");
        playerActions = new double[24][];
        enemyBehavior = new double[24][];
        for (int i = 0; i < playerActions.GetLength(0); i++)
        {
            playerActions[i] = new double[5];
            enemyBehavior[i] = new double[4];
        }
        for (int y = 0; y < playerActions.GetLength(0); y++)
        {
            lines = sr.ReadLine().Split(',');
            for (int z = 0; z < lines.Length; z++)
            {
                playerActions[y][z] = double.Parse(lines[z]); //load every weight stored on the file and parses it as a double
                Debug.Log(playerActions[y][z]);
            }
        }
        sr.Close();
        sr = new StreamReader("Assets/Presets/EnemyReactions.csv");
        for (int y = 0; y < playerActions.GetLength(0); y++)
        {
            lines = sr.ReadLine().Split(',');
            for (int z = 0; z < lines.Length; z++)
            {
                enemyBehavior[y][z] = double.Parse(lines[z]); //load every weight stored on the file and parses it as a double
                Debug.Log(enemyBehavior[y][z]);
            }
        }
        sr.Close();
        Debug.Log("Training Neural Network....");
        for (int i = 0; i < epochs; ++i)
        {
            for (int j = 0; j < playerActions.GetLength(0); j++)
            {
                neuralNetwork.FeedForward(playerActions[j]);
                //                neuralNetwork.BackProp(enemyBehavior[j]);
            }
        }
        Debug.Log("Network Trained!");
        //        neuralNetwork.SaveWeights(neurons[1].weightsList);
    }
    private void OnApplicationQuit()
    {
        //neurons.weights.Clear();
    }
    void Start()
    {
        playerState = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();
        playerPosition = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        darkAnimator = this.GetComponent<Animator>();

    }

    private double[] InputVector(Vector3 position)
    {
        inputVector[0] = (position.x - playerPosition.position.x).CompareTo(0);
        inputVector[1] = (position.y - playerPosition.position.y).CompareTo(0);
        if (playerState.isAttacking)
        {
            inputVector[2] = 1;
        }
        else
        {
            inputVector[2] = -1;
        }
        if (playerState.isBlocking)
        {
            inputVector[3] = 1;
        }
        else
        {
            inputVector[3] = -1;
        }
        inputVector[4] = (position - playerPosition.position).sqrMagnitude.CompareTo(range);
        return inputVector;
    }

    private void Update()
    {
        outputVector = neuralNetwork.FeedForward(InputVector(this.transform.position));
        if (outputVector[3] > 0.8)
        {
            if (perfectionRate > rand.NextDouble())
            {
                blocking = true;
                darkAnimator.SetBool("Blocking", true);
            }
        }

        if (outputVector[2] > 0.8)
        {
            if (perfectionRate > rand.NextDouble())
            {
                darkAnimator.SetBool("Attacking", true);
            }

        }
        darkAnimator.SetInteger("X", outputVector[0].CompareTo(0));
        darkAnimator.SetInteger("Y", outputVector[1].CompareTo(0));
        darkAnimator.SetInteger("Emphasis", Math.Abs(this.transform.position.x - playerPosition.position.x).CompareTo(Math.Abs(this.transform.position.y - playerPosition.transform.position.y)));
        this.transform.position += new Vector3(outputVector[0].CompareTo(0), outputVector[1].CompareTo(0)) / 50;
    }
    public void Attacking()
    {
        audioManager.PlaySound(1);
        attacking = true;
    }
    public void HandleAttacking()
    {
        if (inputVector[4] == -1)
        {
            if (playerState.isBlocking)
            {
                audioManager.PlaySound(3);
                Debug.Log("Player Blocked!");
            }
            else
            {
                //add being hit logic
                Debug.Log("Player Hit!");
                audioManager.PlaySound(2);
                playerState.DecreaseHP(1);
            }
            playerState.transform.position += ((playerState.transform.position - this.transform.position).normalized);
        }
    }
    public void HandleBlocking()
    {
        blocking = true;
    }
    public void DoneBlocking()
    {
        blocking = false;
        darkAnimator.SetBool("Blocking", false);
    }
    public void DoneAttacking()
    {
        attacking = false;
        darkAnimator.SetBool("Attacking", false);
    }
}

