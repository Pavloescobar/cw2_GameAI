
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animation))]
//[RequireComponent(typeof(Animator))]
public class EnemyActionManager : MonoBehaviour
{

    private Animation m_characterAnimation;

    private const string m_PunchAimationName = "punch",
        m_HitAnimationName = "hit",
        m_DeathAnimationName = "death",
        m_WalkAnimationName = "walk",
        m_IdleAnimationName = "idle";

    //private const string m_isWalkingParamName = "isWalking", m_isHitParamName = "isHit",
    //    m_isDiedParamName = "isDead",
    //    m_isPunchingParamName = "idle";
    private Animator m_animator;
    public GameObject m_Player;
    public float m_AttackMinDistance = 2; //starting attcking player from this distance

    //public EnemyState m_State = EnemyState.None;
    public int m_EnemyHitEffect = 2;

    private float m_Health = 20;
    public RoomGraphManager m_ownerRoomManager;

    /// <summary>
    /// time of each single punch before next one
    /// </summary>
    public int m_PunchDurationTime = 2;

    private bool m_isPerformingPunchAnimation = false;

    private bool m_isPlayerLive = true;
    // Use this for initialization
    void Start()
    {
        m_characterAnimation = this.GetComponent<Animation>();
        //m_animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Player != null && m_Health> 0)
        {
            if (!m_isPerformingPunchAnimation && m_isPlayerLive &&
                Vector3.Distance(this.transform.position, m_Player.transform.position) < m_AttackMinDistance)
            {
                StartCoroutine(PerformSinglePunch());
            }
        }
    }

    IEnumerator PerformSinglePunch()
    {
        HumanPlayerTracker tracker = m_Player.GetComponent<HumanPlayerTracker>();
        if (tracker.m_Health <= 0)
        {
            //exit if player is dead
            m_isPlayerLive = false; //end of algorith

        }
        else
        {
            PlayPunchAnimation(m_Player.transform.position);
            m_isPerformingPunchAnimation = true; //disable punching until this punch is complete

            tracker.UpdateHealth(-m_EnemyHitEffect);

            int waitingTime =
                (int) Mathf.Ceil(Mathf.Max(m_PunchDurationTime, m_characterAnimation[m_PunchAimationName].length));
            //wait until punch animation is done
            yield return new WaitForSeconds(waitingTime);
            m_isPerformingPunchAnimation = false; //done punch

        }

    }

    public void PlayWalkAnimation()
    {
        if (!m_characterAnimation.IsPlaying(m_WalkAnimationName))
            m_characterAnimation.PlayQueued(m_WalkAnimationName, QueueMode.PlayNow);
    }

    public void PlayIdleAnimation()
    {
        if (!m_characterAnimation.IsPlaying(m_IdleAnimationName))
            m_characterAnimation.PlayQueued(m_IdleAnimationName, QueueMode.CompleteOthers);
    }

    public void PlayHitAnimation()
    {
        if (!m_characterAnimation.IsPlaying(m_HitAnimationName))
            m_characterAnimation.PlayQueued(m_HitAnimationName, QueueMode.PlayNow);
    }

    public void PlayPunchAnimation(Vector3 playPosition)
    {
        if (!m_characterAnimation.IsPlaying(m_PunchAimationName))
        {
            this.transform.LookAt(playPosition);
            m_characterAnimation.PlayQueued(m_PunchAimationName, QueueMode.PlayNow);
        }
    }

    public void UpdateHealth(int healthPoints = 0)
    {
        m_Health += healthPoints;
        m_characterAnimation.PlayQueued(m_HitAnimationName, QueueMode.PlayNow);
        if (m_Health <= 0)
        {
            StartCoroutine(Die());
        }
    }

    IEnumerator Die()
    {
        m_characterAnimation.PlayQueued(m_DeathAnimationName, QueueMode.PlayNow);
        yield return new WaitForSeconds(m_characterAnimation[m_DeathAnimationName].length);
        this.gameObject.SetActive(false);
        if (m_ownerRoomManager != null)
            m_ownerRoomManager.RemoveAgent(this);
        Destroy(this.gameObject);
    }
}

//public enum EnemyState
//{
//   None, walking, Attacking, Dying
//}