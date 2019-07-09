using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerState : MonoBehaviour
{
    public Animator animator;
    public bool isAttacking, isBlocking;
    public EnemyScript enemy;
    public Image[] hearts;
    public Sprite heart1, heart2, heart3;
    public int direction;
    public Text message;
    public GameObject pauseMenu;
    public GameObject gameOverButton;
    public AudioManager audioManager;
    public bool paused = false;
    int life1 = 6;
    int life2 = 6;
    // Start is called before the first frame update
    void Start()
    {
        //hearts = GameObject.FindGameObjectWithTag("GUI").GetComponentsInChildren<Image>();
        gameOverButton.SetActive(false);
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        //message = GameObject.FindGameObjectWithTag("GUI").GetComponent<Text>();
        animator = this.GetComponent<Animator>();
        enemy = GameObject.FindGameObjectWithTag("Enemy").GetComponent<EnemyScript>();
        PauseMenu();
    }

    // Update is called once per frame
    void Update()
    {
        if (paused == true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Unpause();
                paused = false;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseMenu();
            }
            if (Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow))
            {
                direction = 3;
                animator.SetBool("IsMoving", true);
            }
            else if (Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow))
            {
                direction = 1;

                animator.SetBool("IsMoving", true);
            }
            else if (Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow))
            {
                direction = 0;
                animator.SetBool("IsMoving", true);
            }
            else if (Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow))
            {
                direction = 2;
                animator.SetBool("IsMoving", true);
            }
            else
            {
                animator.SetBool("IsMoving", false);
            }
            animator.SetInteger("Direction", direction);
            if (Input.GetMouseButton(0))
            {
                animator.SetBool("Attacking", true);
            }
            if (Input.GetMouseButton(1))
            {
                animator.SetBool("Blocking", true);
                isBlocking = true;
            }
            else
            {
                isBlocking = false;
                animator.SetBool("Blocking", false);
            }
            if (!isBlocking && !isAttacking)
            {
                this.transform.position += new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized / 25;
            }
        }

    }

    private void Unpause()
    {
        Time.timeScale = 1f;
        enemy.enabled = true;
        pauseMenu.SetActive(false);
    }

    private void PauseMenu()
    {
        Time.timeScale = 0;
        enemy.enabled = false;
        paused = true;
        pauseMenu.SetActive(true);
    }

    public void Attacking()
    {
        audioManager.PlaySound(1);
        isAttacking = true;
    }

    public void DoneAttacking()
    {
        isAttacking = false;
        animator.SetBool("Attacking", false);

    }
    public void HandleAttacking()
    {
        if (enemy.inputVector[4] == -1)
        {
            if (enemy.blocking)
            {
                audioManager.PlaySound(3);
                Debug.Log("Blocked!");
            }
            else
            {
                audioManager.PlaySound(2);
                Debug.Log("Hit!");
                DecreaseHP(2);
            }
            enemy.transform.position += ((enemy.transform.position - this.transform.position).normalized);
        }

    }

    public void DecreaseHP(int x)
    {
        switch (x)
        {
            case 1:
                life1--;
                switch (life1)
                {
                    case 5:
                        hearts[2].sprite = heart2;
                        break;
                    case 4:
                        hearts[2].sprite = heart3;
                        break;
                    case 3:
                        hearts[1].sprite = heart2;
                        break;
                    case 2:
                        hearts[1].sprite = heart3;
                        break;
                    case 1:
                        hearts[0].sprite = heart2;
                        break;
                    case 0:
                        hearts[0].sprite = heart3;
                        audioManager.PlaySound(4);
                        GameOver();
                        break;
                }
                break;
            case 2:
                life2--;
                Debug.Log("Enemy life: " + life2);
                switch (life2)
                {
                    case 5:
                        hearts[3].sprite = heart2;
                        break;
                    case 4:
                        hearts[3].sprite = heart3;
                        break;
                    case 3:
                        hearts[4].sprite = heart2;
                        break;
                    case 2:
                        hearts[4].sprite = heart3;
                        break;
                    case 1:
                        hearts[5].sprite = heart2;
//                        enemy.neuralNetwork.LoadWeights(enemy.neurons[1].weightsList);
                        break;
                    case 0:
                        hearts[5].sprite = heart3;
                        audioManager.PlaySound(4);
                        Victory();
                        break;
                }
                break;
        }
    }

    private void Victory()
    {
        gameOverButton.SetActive(true); //lazy solution but oh well.
        message.text = "You Win!";
        enemy.enabled = false;
        Time.timeScale = 0;
        this.enabled = false;
    }

    private void GameOver()
    {
        gameOverButton.SetActive(true); //see above
        message.text = "Game Over!";
        Time.timeScale = 0;
        enemy.enabled = false;
        this.enabled = false;
    }
}

