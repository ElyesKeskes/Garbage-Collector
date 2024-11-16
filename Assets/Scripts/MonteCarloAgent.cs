using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonteCarloAgent : MonoBehaviour
{
    public AgentManager _agentManager;
    public Animator _animator;

    public float decisionTime = 1f; // Time for MCTS to decide the best path
    public Transform handTransform;
    public Transform trashBagTransform;
    public NavMeshAgent navMeshAgent;
    public Transform targetItem;
    public int currentTrashCount = 0;
    public bool currentlyOnTrashcan = false;
    public bool pickUpTrash = false;
    public int currentValue = 0;
    public bool moveOn = false;
    public bool gotUp = true;
    public float upPushForce = 1.5f;
    public int TotalTrashCollected = 0;
    
    private MCTreeNode rootNode;
    void Start()
    {
        rootNode = new MCTreeNode(transform, null); // Root node represents the agent's initial state
        StartCoroutine(RunMCTSv2());
    }

    public IEnumerator RunMCTS()
    {
        float bestScore = float.MinValue;
        Transform bestItem = null;

        // Start Monte Carlo Tree Search to find the best item
        float startTime = Time.time;
        while (Time.time - startTime < decisionTime)
        {
            if (currentlyOnTrashcan || (_agentManager.trashPieces.Count == 0))
            {
                foreach (Trash item in _agentManager.trashCans)
                {
                    float score = Simulate(item.transform);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestItem = item.transform;
                    }
                }
            }else
            {
                foreach (Trash item in _agentManager.trashPieces)
                {
                    float score = Simulate(item.transform);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestItem = item.transform;
                    }
                }

                if (!bestItem)
                {
                    foreach (Trash item in _agentManager.trashCans)
                    {
                        float score = Simulate(item.transform);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestItem = item.transform;
                        }
                    }
                }
            }
            
            
            yield return null;
        }

        targetItem = bestItem;
        if (targetItem != null)
        {
            navMeshAgent.SetDestination(targetItem.position);
            _animator.SetTrigger("Walk");
        }
    }

    public IEnumerator RunMCTSv2()
{
    // Start the MCTS loop
    float startTime = Time.time;

    while (Time.time - startTime < decisionTime)
    {
        // Selection
        MCTreeNode selectedNode = Selection(rootNode);

        // Expansion
        if (!selectedNode.IsFullyExpanded())
        {
            selectedNode = Expansion(selectedNode);
        }

        // Simulation
        float reward = Simulation(selectedNode);

        // Backpropagation
        Backpropagation(selectedNode, reward);

        yield return null;
    }

    // After MCTS, decide the best target item
    MCTreeNode bestChild = rootNode.GetBestChild();

    if (bestChild != null)
    {
        targetItem = bestChild.State;
        navMeshAgent.SetDestination(targetItem.position);
        _animator.SetTrigger("Walk");
    }
    else
    {
        Debug.LogWarning("No valid target found after MCTS. Falling back to default behavior.");
        HandleNoValidTarget();
    }
}

    float Simulate(Transform item)
    {
        // Simple simulation: use the negative distance as the score
        float distance = Vector3.Distance(transform.position, item.position);
        return -distance; // Closer items have a higher score
    }

    //Additions

    public void OnTrashPickedUpChangeTarget()
    {
        if (currentlyOnTrashcan)
        {
            _animator.SetTrigger("Throw");
            currentlyOnTrashcan = !currentlyOnTrashcan;
            TotalTrashCollected += currentTrashCount;
            currentTrashCount = 0;
        }
        else
        {
            _animator.SetTrigger("PickUp");
            currentTrashCount++;
        }

        if (currentTrashCount >= _agentManager.trashCapacity)
        {
            currentlyOnTrashcan = !currentlyOnTrashcan;
          
            //currentTrashCount = 0;
        }

        StartCoroutine(OnTrashPickedUpChangeTargetContinuation());
    }

    private IEnumerator OnTrashPickedUpChangeTargetContinuation()
    {
        yield return new WaitUntil(() => moveOn);
        moveOn = false;

        StartCoroutine(RunMCTS());
    }

    public void GetHitByDog()
    {
        gotUp = false;
        navMeshAgent.SetDestination(transform.position);
        _animator.SetTrigger("GetHit");
        currentTrashCount = 0;
        StartCoroutine(AwaitGetUp());
    }

    private IEnumerator AwaitGetUp()
    {
        List<Transform> trashChildList = new List<Transform>();
        foreach (Transform trashChild in trashBagTransform)
        {
            trashChildList.Add(trashChild);
        }

        foreach (Transform trashChild in trashChildList)
        {
            trashChild.parent = null;
            Rigidbody rb = trashChild.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.None;
            rb.useGravity = true;
            rb.isKinematic = false;

            Vector3 randomDirection = Vector3.up * upPushForce + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));
            rb.AddForce(randomDirection.normalized * 20f, ForceMode.Impulse);

            StartCoroutine(TrashReset(trashChild));
        }


        yield return new WaitUntil(() => gotUp);

        navMeshAgent.SetDestination(transform.position);
        moveOn = true;


        // agentCharacter.StartMove(oldPath);
        StartCoroutine(RunMCTS());
    }

    private IEnumerator TrashReset(Transform trashChild)
    {
        Trash _coin = trashChild.GetComponent<Trash>();
        yield return new WaitForSeconds(2.25f);
        Rigidbody rb = trashChild.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        rb.isKinematic = true;

        Rigidbody newRb = trashChild.GetChild(0).GetComponent<Rigidbody>();
        newRb.isKinematic = false;
        newRb.useGravity = true;
        newRb.constraints = RigidbodyConstraints.None;

        _coin.hasBeenPickedUp = false;

        _coin.ReStart(this);

        _coin.transform.GetChild(1).gameObject.SetActive(true);
        _agentManager.trashPieces.Add(_coin);
    }


    //REWORKED

   

    private MCTreeNode Selection(MCTreeNode node)
    {
        while (node.IsFullyExpanded() && !node.IsLeaf())
        {
            node = node.GetBestUCTChild();
        }
        return node;
    }

    private MCTreeNode Expansion(MCTreeNode node)
    {
        List<Transform> possibleActions = GetPossibleActions(node);
        foreach (Transform action in possibleActions)
        {
            if (!node.HasChildForAction(action))
            {
                return node.AddChild(action, this);
            }
        }
        return node; // All actions already expanded
    }

  private float Simulation(MCTreeNode node)
{
    // Perform simulation on the node's state
    Transform item = node.State;
    if (item == null) return float.MinValue;

    // Example heuristic: Negative distance from agent to item
    float distance = Vector3.Distance(transform.position, item.position);
    return -distance; // Closer items are prioritized
}
private void HandleNoValidTarget()
{
    // Fallback logic if no valid target is found
    if (_agentManager.trashPieces.Count > 0)
    {
        targetItem = _agentManager.trashPieces[0].transform;
    }
    else if (_agentManager.trashCans.Count > 0)
    {
        targetItem = _agentManager.trashCans[0].transform;
    }

    if (targetItem != null)
    {
        navMeshAgent.SetDestination(targetItem.position);
        _animator.SetTrigger("Walk");
    }
}

    private void Backpropagation(MCTreeNode node, float reward)
    {
        while (node != null)
        {
            node.VisitCount++;
            node.TotalReward += reward;
            node = node.Parent;
        }
    }

private List<Transform> GetPossibleActions(MCTreeNode node)
{
    List<Transform> possibleActions = new List<Transform>();

    // Condition: Either collecting trash or going to the trashcan
    if (currentlyOnTrashcan || _agentManager.trashPieces.Count == 0)
    {
        foreach (Trash item in _agentManager.trashCans)
        {
            if (!node.HasChildForAction(item.transform))
            {
                possibleActions.Add(item.transform);
            }
        }
    }
    else
    {
        foreach (Trash item in _agentManager.trashPieces)
        {
            if (!node.HasChildForAction(item.transform))
            {
                possibleActions.Add(item.transform);
            }
        }

        if (possibleActions.Count == 0)
        {
            foreach (Trash item in _agentManager.trashCans)
            {
                if (!node.HasChildForAction(item.transform))
                {
                    possibleActions.Add(item.transform);
                }
            }
        }
    }

    return possibleActions;
}



    


// MCTS Tree Node
public class MCTreeNode
{
    public Transform State; // Represents the state (e.g., position of the item)
    public MCTreeNode Parent;
    public List<MCTreeNode> Children;
    public int VisitCount;
    public float TotalReward;

    private Dictionary<Transform, MCTreeNode> childNodes;

    public MCTreeNode(Transform state, MCTreeNode parent)
    {
        State = state;
        Parent = parent;
        Children = new List<MCTreeNode>();
        VisitCount = 0;
        TotalReward = 0;
        childNodes = new Dictionary<Transform, MCTreeNode>();
    }

    public bool IsFullyExpanded()
    {
        return childNodes.Count == Children.Count;
    }

    public bool IsLeaf()
    {
        return Children.Count == 0;
    }

    public MCTreeNode AddChild(Transform state, MonoBehaviour agent)
    {
        MCTreeNode child = new MCTreeNode(state, this);
        Children.Add(child);
        childNodes[state] = child;
        return child;
    }

    public bool HasChildForAction(Transform action)
    {
        return childNodes.ContainsKey(action);
    }

    public MCTreeNode GetBestUCTChild()
    {
        MCTreeNode bestChild = null;
        float bestUCTValue = float.MinValue;

        foreach (MCTreeNode child in Children)
        {
            float uctValue = CalculateUCT(child);
            if (uctValue > bestUCTValue)
            {
                bestUCTValue = uctValue;
                bestChild = child;
            }
        }
        return bestChild;
    }

    private float CalculateUCT(MCTreeNode node)
    {
        if (node.VisitCount == 0)
        {
            return float.MaxValue; // Favor unexplored nodes
        }
        float exploitation = node.TotalReward / node.VisitCount;
        float exploration = Mathf.Sqrt(2 * Mathf.Log(VisitCount) / node.VisitCount);
        return exploitation + exploration;
    }

    public MCTreeNode GetBestChild()
    {
        MCTreeNode bestChild = null;
        float bestReward = float.MinValue;

        foreach (MCTreeNode child in Children)
        {
            float averageReward = child.TotalReward / child.VisitCount;
            if (averageReward > bestReward)
            {
                bestReward = averageReward;
                bestChild = child;
            }
        }
        return bestChild;
    }
}
}
