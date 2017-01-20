using System.Collections;

using UnityEngine;
using System.Collections;
//based on http://hunternacho.com/blog/2016/05/15/game-character-movement-in-unity-3d/
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(HumanPlayerTracker))]
[RequireComponent(typeof(Animation))]
[RequireComponent(typeof(AudioSource))]
public class SimpleMovement : MonoBehaviour
{

    public float Speed;
    public float RotateSpeed;

    private CharacterController m_controller;
    private HumanPlayerTracker m_PlayerTrackerComponent;
    private Animation m_characterAnimation;
    private const string m_AttackAimationName = "attack";
    public float m_AttackMinDistance = 5f;
    public string m_EnemyTag = "Enemy";
    private bool m_isPerformingSwordAnimation;
    public float m_SwordWaitingTime=0.5f;
    private AudioSource m_audioSource;

    private void Start()
    {
        m_audioSource = this.GetComponent<AudioSource>();

        m_PlayerTrackerComponent = this.GetComponent<HumanPlayerTracker>();
        m_characterAnimation = this.GetComponent<Animation>();
        m_controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        //______________Fighting Animations
        if (Input.GetKey(KeyCode.H) && !m_isPerformingSwordAnimation)
        {
            if (!m_audioSource.isPlaying)
                m_audioSource.Play();
            StartCoroutine(PerformSingleSwordHit());

        }


        //______________Movement ___________________
       
        transform.Rotate(0, Input.GetAxis("Horizontal") * RotateSpeed, 0);
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        float curSpeed = Speed * Input.GetAxis("Vertical");
        m_controller.SimpleMove(forward * curSpeed);

        if (Mathf.Abs(curSpeed)>0 && m_PlayerTrackerComponent != null)
            m_PlayerTrackerComponent.NotifyPlayerLocationChange(this.transform.position);
    }

    IEnumerator PerformSingleSwordHit()
    {
        m_characterAnimation.Play(m_AttackAimationName);

        m_isPerformingSwordAnimation = true;//disable punching until this punch is complete

        int waitingTime = (int)Mathf.Ceil(Mathf.Max(m_SwordWaitingTime, m_characterAnimation[m_AttackAimationName].length));
        //wait until sword animation is done
        yield return new WaitForSeconds(waitingTime/2);//wait half time to be in middle in animation

        
        //test if nay enemy is hit
        float radiusAttackDistance = m_AttackMinDistance / 2.0f;
        Vector3 frontSphereCenter = this.transform.position + radiusAttackDistance * this.transform.forward;

        Collider[] collidedObjects = Physics.OverlapSphere(frontSphereCenter, radiusAttackDistance);
        foreach (Collider col in collidedObjects)
        {
            if (col.tag == m_EnemyTag)
            {
                EnemyActionManager actionComponent = col.GetComponent<EnemyActionManager>();
                actionComponent.UpdateHealth((int)-m_PlayerTrackerComponent.GetSingleHitEffect());
            }
        }

        yield return new WaitForSeconds(waitingTime / 2);

        m_isPerformingSwordAnimation = false;//done punch
    }
}