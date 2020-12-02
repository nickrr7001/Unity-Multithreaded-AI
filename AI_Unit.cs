using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class AI_Unit : MonoBehaviour
{
    private AIManager manager;
    public bool isVisible = false;
    public bool inRange = false;
    public bool alerted = false;
    [SerializeField] private LayerMask mask;
    [SerializeField]private GameObject muzzleFlash;
    [SerializeField]private Transform firePoint;
    private Animator anim;
    private float runTimer = 0f;
    public float distance;
    public NavMeshAgent agent;
    public int ammo = 10;
    private AudioSource audioSource;
    [SerializeField]private AudioClip[] footSteps;
    [SerializeField] private GameObject blood;
    [SerializeField] private GameObject dirtImpact;
    [SerializeField] private GameObject weaponPath;
    public void footStep()
    {
        audioSource.clip = footSteps[Random.Range(0, footSteps.Length)];
        audioSource.Play();
    }
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        manager = FindObjectOfType<AIManager>();
        manager.addUnit(this);
        mask = manager.mask;
        agent = GetComponent<NavMeshAgent>();
    }
    private void Update()
    {
        if (GetComponent<NPC_Health>().isDead())
        {
            manager.removeUnit(this);
            Destroy(this);
        }
        if (!inRange)
        {
            return;
        }
        if (isVisible)
        {
            alerted = true;
            anim.SetTrigger("Alerted");
            anim.SetBool("Run", false);
            anim.SetBool("wait", false);
            transform.LookAt(new Vector3(manager.player.position.x, transform.position.y, manager.player.position.z));
            runTimer = 0;
            agent.isStopped = true;
        }
        else if (alerted)
        {
            if (runTimer > 7.5 || distance > 20) {
                anim.SetBool("Run", true);
                anim.SetBool("wait", false);
                agent.isStopped = true;
            }
            else
            {
                anim.SetBool("Run", false);
                anim.SetBool("wait", true);
                runTimer += Time.deltaTime;
                agent.isStopped = false;
                agent.SetDestination(manager.player.position);
            }
            
        }
        else
        {
            runTimer = 0;
            anim.SetBool("Run", false);
            agent.isStopped = true;
        }
    }
    public void shoot()
    {
        ammo -= 1;
        if (ammo <= 0)
        {
            anim.SetTrigger("Reload");
            ammo = 10;
        }
        Instantiate(muzzleFlash, firePoint);
        RaycastHit hit;
        Vector3 player = manager.player.position;
        Vector3 origin = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        Vector3 heading = new Vector3(player.x, player.y, player.z) - transform.position;
        Vector3 direction = heading / heading.magnitude;
        Vector3 dir = new Vector3(
            direction.x + Random.Range(-0.03f, 0.03f),
            direction.y + Random.Range(-0.03f, 0.03f),
            direction.z + Random.Range(-0.03f, 0.03f)
            );
        GameObject pathTrace = Instantiate(weaponPath, firePoint.position, Quaternion.identity);
        pathTrace.GetComponent<LineRenderPosition>().setTarget(transform.position + (dir * 100));
        if (Physics.Raycast(transform.position, dir, out hit, 999f, mask))
        {
            if (hit.collider.GetComponent<PlayerHealth>() != null)
            {
                hit.collider.GetComponent<PlayerHealth>().Damage(10);
                Instantiate(blood, hit.point, Quaternion.identity);
            }
            else if (hit.collider != null)
            {
                Instantiate(dirtImpact, hit.point, Quaternion.identity);
            }
        }
    }
}
