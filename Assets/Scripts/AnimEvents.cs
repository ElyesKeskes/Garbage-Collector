using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEvents : MonoBehaviour
{
    public AgentManager _agentManager;
    public Animator _animator;

    private void Update()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("WalkForward"))
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
        }
    }

    public void MoveOn()
    {
        _agentManager.moveOn = true;
    }

    public void CollectTrash()
    {
        _agentManager.pickUpTrash = true;
    }

    public void GetUp()
    {
        _agentManager.gotUp = true;
    }
}
