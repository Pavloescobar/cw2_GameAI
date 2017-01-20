using UnityEngine.UI;
using UnityEngine;
using System;
using System.Collections;


public class HumanPlayerTracker : MonoBehaviour
{

    public RoomGraphManager m_currentRoomManager;
    public const int m_MaxHealth = 100, m_MaxPower = 20;
    public int m_Health = m_MaxHealth;
    public int m_Power = 1;
    public int m_Score = 0;
    //effect of each unit of power on enemies
    public int m_PowerUnitEffectOnEnemies = 5;

    //names of some used Tags in Design
    public string m_RoomTag = "RoomTag", m_PowerTag = "PowerItem", m_HealthTag = "HeathItem";

    public Text m_ScoreText, m_PowerText, m_HealthText, m_MessageText;
    public GameObject m_MessagePanel;

    //update values
    const int m_healthUpdateAmount = 10;
    const int m_ScoreUpdateAmount = 1;
    const int m_PowerUpdateAMount = 1;

  
    // Use this for initialization
    void Start()
    {
       
        //show initial score and player states
        UpdateScore();
        UpdatePower();
        UpdateHealth();

        //increase score by time
        StartCoroutine(IcreaseScoreByTime());//get 1 point for each new second
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float GetSingleHitEffect()
    {
        return m_Power*m_PowerUnitEffectOnEnemies;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == m_RoomTag)
        {
            //new room entered
            m_currentRoomManager = other.gameObject.GetComponent<RoomGraphManager>();

        }
        else if (other.tag == m_PowerTag)
        {
            
            UpdatePower(m_PowerUpdateAMount);

            if (m_currentRoomManager != null)
            {
                m_currentRoomManager.RemovePowerUnit(other.gameObject);
            }
            Destroy(other.gameObject);
        }
        else if (other.tag == m_HealthTag)
        {
            
            UpdateHealth(m_healthUpdateAmount);

            if (m_currentRoomManager != null)
            {
                m_currentRoomManager.RemoveHealthUnit(other.gameObject);
            }
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == m_RoomTag)
        {
            //leaving current room
            m_currentRoomManager = null;

        }
    }

    public void NotifyPlayerLocationChange(Vector3 PlayerNewLocation)
    {
        if (m_currentRoomManager != null)
            m_currentRoomManager.NotifyPlayerLocationChange(PlayerNewLocation);
    }

    public void UpdateScore(int points = 0)
    {
        if (m_Score < int.MaxValue)
            m_Score += points;

        if (m_ScoreText != null)
            m_ScoreText.text = "Score: " + m_Score;
    }

    public void UpdateHealth(int healthPoints = 0)
    {
        m_Health += healthPoints;
        if (m_Health > m_MaxHealth) m_Health = m_MaxHealth;

        if (m_HealthText != null)
            m_HealthText.text = "Health: " + m_Health;
        if (m_Health <= 0)
            NotifyEndOfGame();
    }

    private void NotifyEndOfGame()
    {
        if (m_MessagePanel != null && m_MessageText != null)
        {
            m_MessagePanel.SetActive(true);
            m_MessageText.text = "End of Game, Your Score is \r\n" + m_Score;
        }
        //stop game
        Time.timeScale = 0;
    }

    public void UpdatePower(int powerPoints = 0)
    {

        m_Power += powerPoints;
        if (m_Power > m_MaxPower) m_Power = m_MaxPower;
        if (m_PowerText != null)
            m_PowerText.text = "Power: " + m_Power;
    }

    IEnumerator IcreaseScoreByTime()
    {
        WaitForSeconds oneSecondWait = new WaitForSeconds(1);
        while (m_Health > 0)
        {
            
            UpdateScore(m_ScoreUpdateAmount);
            yield return oneSecondWait;
        }
    }
}
