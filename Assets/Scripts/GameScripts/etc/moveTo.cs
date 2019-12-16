// MoveTo.cs
using UnityEngine;
using System.Collections;
[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class moveTo : MonoBehaviour {

    public Transform goal;

    void Update() {
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (goal) {
            agent.destination = goal.position;
        }
        
    }
}