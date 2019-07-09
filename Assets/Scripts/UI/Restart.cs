using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Restart : MonoBehaviour
{
    public GameObject pauseMenu;
    public EnemyScript enemy;
    public void RestartGame()
    {
        pauseMenu.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // loads current scene
    }
    public void LowerDifficulty()
    {
        enemy.perfectionRate = enemy.perfectionRate * 0.9f;
    }
    public void RaiseDifficulty()
    {
        enemy.perfectionRate = enemy.perfectionRate * 1.1f;
    }

}