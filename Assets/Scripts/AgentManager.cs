//keeps a list of coins

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AgentManager : Singleton<AgentManager>
{
    public List<Trash> trashPieces = new List<Trash>();
    public List<Trash> trashCans = new List<Trash>();
    public Trash currentTarget;

    public Character agentCharacter;
    public Pathfinder pathfinder;
    Path Lastpath;
    Tile currentTile;
    private int currentTrashCount = 0;
    public int trashCapacity = 3;

    public Animator _animator;

    public bool currentlyOnTrashcan = false;

    public bool moveOn = false;

    public bool pickUpTrash = false;

    public Transform handTransform;

    public Transform trashBagTransform;

    public TrashRandomizer trashRandomizer;
    public TextMeshProUGUI currentTXT;
    public TextMeshProUGUI totalTXT;
    public int currentValue = 0;

    public GameObject winImg;

    public bool gotUp = true;
    // public float upPushForce = 1.5f;

    public LayerMask GroundLayerMask;

     public Transform raycastOrigin;

    public Path oldPath;

    public void RotateTowards(Transform target)
    {
        StartCoroutine(RotateSmoothly(target, 0.5f));
    }

    private IEnumerator RotateSmoothly(Transform target, float duration)
    {
        Transform child = transform.GetChild(0);

        Quaternion startRotation = child.rotation;

        Vector3 directionToTarget = (target.position - child.position).normalized;

        Quaternion endRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));

        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            child.rotation = Quaternion.Slerp(startRotation, endRotation, timeElapsed / duration);

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        child.rotation = endRotation;
    }


    public void SetTarget(Trash coin)
    {
        currentTarget = coin;
        currentTile = coin.trashTile;
    }

    void GetAllCoinsByTag()
    {
        trashPieces = GameObject.FindGameObjectsWithTag("Trash").ToList().ConvertAll(x => x.GetComponent<Trash>());
        trashCans = GameObject.FindGameObjectsWithTag("Trashcan").ToList().ConvertAll(x => x.GetComponent<Trash>());
    }

    void GetClosestTrash()
    {
        float minDistance = Mathf.Infinity;
        Trash closestCoin = null;

        foreach (Trash coin in trashPieces)
        { 
            Debug.Log("Get Closest Coin foreach loop for coin == " + coin);
            if (coin)
            {
                float distance = Vector3.Distance(agentCharacter.transform.position, coin.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCoin = coin;
                }
            }
        }

        SetTarget(closestCoin);
    }
    void GetClosestTrashCan()
    {
        float minDistance = Mathf.Infinity;
        Trash closestCoin = null;

        foreach (Trash trashCan in trashCans)
        {
            if (trashCan)
            {
                float distance = Vector3.Distance(agentCharacter.transform.position, trashCan.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCoin = trashCan;
                }
            }
        }

        SetTarget(closestCoin);
    }

    public void GetHitByDog()
    {
        gotUp = false;
        agentCharacter.Moving = false;
        agentCharacter.stopMoving = true;
        /*selectedCharacter.stopMoving = true;
        selectedCharacter.Moving = false;
        selectedCharacter.characterTile.Occupied = true;
        selectedCharacter.characterTile.occupyingCharacter = selectedCharacter;
        */
        if (Physics.Raycast(raycastOrigin.position, -transform.up, out RaycastHit hit, 50f, GroundLayerMask))
        {
            Debug.Log("<color=red>Found a tile below me</color>");
            agentCharacter.characterTile = hit.transform.GetComponent<Tile>();
            Debug.Log("<color=red>selectedCharacter.characterTile : </color>" + agentCharacter.characterTile);
        }
        _animator.SetTrigger("GetHit"); 
        StartCoroutine(AwaitGetUp());
    }

    private IEnumerator AwaitGetUp()
    {
        /*List<Transform> trashChildList = new List<Transform>();
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
        */
        yield return new WaitUntil(() => gotUp);
        Debug.Log("I GOT UP YAZEBI");

        agentCharacter.Moving = false;
        moveOn = true;
  

        // agentCharacter.StartMove(oldPath);
        StartCoroutine(SelectNextTarget());
    }

    // private IEnumerator TrashReset(Transform trashChild)
    // {
    //     Trash _coin = trashChild.GetComponent<Trash>();
    //     coins.Add(_coin);
    //     _coin.ReStart();
    //     yield return new WaitForSeconds(6f);
    //     Rigidbody rb = trashChild.GetComponent<Rigidbody>();
    //     rb.constraints = RigidbodyConstraints.FreezeAll;
    //     rb.useGravity = false;
    //     rb.isKinematic = true;
    //     _coin.ReStart();
    // }

    void Start()
    {
        Invoke("DelayedStart", 3f);
        totalTXT.text = trashRandomizer.NumberofTrashToSpawn.ToString();
    }

    void DelayedStart()
    {
        if (pathfinder == null)
            pathfinder = GameObject.Find("Pathfinder").GetComponent<Pathfinder>();
        GetAllCoinsByTag();
        GetClosestTrash();
        _animator.SetTrigger("Walk");
        NavigateToTile();
    }

    public void OnTrashPickedUpChangeTarget(Tile coinTile)
    {
        agentCharacter.stopMoving = true;
        agentCharacter.Moving = false;
        agentCharacter.characterTile = coinTile;
        agentCharacter.characterTile.Occupied = true;
        agentCharacter.characterTile.occupyingCharacter = agentCharacter;

        if (currentlyOnTrashcan)
        {
            _animator.SetTrigger("Throw");
            currentlyOnTrashcan = !currentlyOnTrashcan;
            currentTrashCount = 0;
        }
        else
        {
            _animator.SetTrigger("PickUp");
            currentTrashCount++;
        }

        if(currentTrashCount >= trashCapacity)
        {
            currentlyOnTrashcan = !currentlyOnTrashcan;
            currentTrashCount = 0;
        }

        StartCoroutine(SelectNextTarget());
    }

    private IEnumerator SelectNextTarget()
    {
        yield return new WaitUntil(() => moveOn);
        moveOn = false;
        Debug.Log("I'm Here");
        if (currentlyOnTrashcan)
        {
            GetClosestTrashCan();
        }
        else
        {
            GetClosestTrash();
        }

        
        NavigateToTile();
    }

    private void NavigateToTile()
    {   Debug.Log("3ASBA TREYA");
        if (agentCharacter == null || agentCharacter.Moving == true)
        {
            return;
        }

        Debug.Log("i'm retrieving path");


        if (RetrievePath(out Path newPath))
        {
            oldPath = newPath;
            agentCharacter.stopMoving = false;
            agentCharacter.StartMove(newPath);
            //   selectedCharacter = null;
        }
    }

    bool RetrievePath(out Path path)
    {
        Debug.Log("CurrentTile: " + currentTile);
        path = pathfinder.FindPath(agentCharacter.characterTile, currentTile);

        if (path == null || path == Lastpath)
            return false;
        return true;
    }



}