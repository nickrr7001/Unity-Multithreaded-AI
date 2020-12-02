using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using Unity.Jobs;
using Unity.Collections;
public class AI_Manager : MonoBehaviour
{
    public Dictionary<Behaviour, bool> AILookUP = new Dictionary<Behaviour, bool>();
    private List<Behaviour> AI = new List<Behaviour>();
    public Transform player;
    public LayerMask mask;
    private void Start()
    {
        player = FindObjectOfType<FirstPersonController>().transform;
    }
    public void Register(Behaviour ai)
    {
        AI.Add(ai);
        AILookUP.Add(ai, false);
    }
    public void Remove(Behaviour ai)
    {
        AI.Remove(ai);
        AILookUP.Remove(ai);
    }
    private void Update()
    {
        NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(AI.Count, Allocator.TempJob);
        NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(AI.Count, Allocator.TempJob);
        JobHandle job;
        for (int i = 0; i < AI.Count; i++)
        {
            Vector3 origin = new Vector3(AI[i].transform.position.x, AI[i].transform.position.y + 0.5f, AI[i].transform.position.z);
            Vector3 heading = new Vector3(player.position.x, player.position.y, player.position.z) - AI[i].transform.position;
            Vector3 direction = heading / heading.magnitude;
            commands[i] = new RaycastCommand(origin, direction, 100f, mask);
        }
        job = RaycastCommand.ScheduleBatch(commands, results, 1, default(JobHandle));
        job.Complete();
        for (int i = 0; i < AI.Count; i++)
        {
            RaycastHit hit = results[i];
            if (hit.collider != null)
            {
                if (hit.collider.GetComponent<FirstPersonController>() != null)
                {
                    Vector3 TargetDir = hit.transform.position - AI[i].transform.position;
                    float angle = Vector3.Angle(TargetDir, AI[i].transform.forward);
                    if (Mathf.Abs(angle) < 75)
                    {
                        AILookUP[AI[i]] = true;
                        continue;
                    }
                }
            }
            AILookUP[AI[i]] = false;
        }
        results.Dispose();
        commands.Dispose();
    }
}
