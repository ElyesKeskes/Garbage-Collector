//keeps a list of coins

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CoinManager : Singleton<CoinManager>
{
    public List<Coin> coins = new List<Coin>();
    public List<Coin> trashCans = new List<Coin>();
    public Coin currentTarget;

    public Character selectedCharacter;
    public Pathfinder pathfinder;
    Path Lastpath;
    Tile currentTile;

    private int currentTrashLevel = 0;
    public int trashCapacity = 3;

    public Animator _animator;

    public bool currentInTrashCan = false;

    public bool moveOn = false;

    public bool pickUpTrash = false;

    public Transform handTransform;

    public Transform trashBagTransform;

    public CoinRandomizer coinRandomizer;
    public TextMeshProUGUI currentTXT;
    public TextMeshProUGUI totalTXT;
    public int currentValue = 0;

    public GameObject winImg;

    public bool gotUp = true;
    public float upPushForce = 1.5f;

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


    public void SetTarget(Coin coin)
    {
        currentTarget = coin;
        currentTile = coin.coinTile;
    }

    void GetAllCoinsByTag()
    {
        coins = GameObject.FindGameObjectsWithTag("Trash").ToList().ConvertAll(x => x.GetComponent<Coin>());
        trashCans = GameObject.FindGameObjectsWithTag("Trashcan").ToList().ConvertAll(x => x.GetComponent<Coin>());
    }

    void GetClosestTrash()
    {
        float minDistance = Mathf.Infinity;
        Coin closestCoin = null;

        foreach (Coin coin in coins)
        { 
            Debug.Log("Get Closest Coin foreach loop for coin == " + coin);
            if (coin)
            {
                float distance = Vector3.Distance(selectedCharacter.transform.position, coin.transform.position);
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
        Coin closestCoin = null;

        foreach (Coin trashCan in trashCans)
        {
            if (trashCan)
            {
                float distance = Vector3.Distance(selectedCharacter.transform.position, trashCan.transform.position);
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
        selectedCharacter.Moving = false;
        selectedCharacter.stopMoving = true;
        /*selectedCharacter.stopMoving = true;
        selectedCharacter.Moving = false;
        selectedCharacter.characterTile.Occupied = true;
        selectedCharacter.characterTile.occupyingCharacter = selectedCharacter;
        if (Physics.Raycast(raycastOrigin.position, -transform.up, out RaycastHit hit, 50f, GroundLayerMask))
        {
            Debug.Log("Found a tile below me");
            selectedCharacter.characterTile = hit.transform.GetComponent<Tile>();
            Debug.Log("selectedCharacter.characterTile : " + selectedCharacter.characterTile);
        }*/
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

        Debug.Log("I just got up");

        Debug.Log("Let's raycast down");

        selectedCharacter.Moving = true;
        selectedCharacter.stopMoving = false;

        selectedCharacter.StartMove(oldPath);
        //StartCoroutine(SelectNextTarget());
    }

    private IEnumerator TrashReset(Transform trashChild)
    {
        Coin _coin = trashChild.GetComponent<Coin>();
        coins.Add(_coin);
        _coin.ReStart();
        yield return new WaitForSeconds(6f);
        Rigidbody rb = trashChild.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        rb.isKinematic = true;
        _coin.ReStart();
    }

    void Start()
    {
        Invoke("DelayedStart", 3f);
        totalTXT.text = coinRandomizer.nbCoins.ToString();
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

    public void OnCoinAcquiredChangeTarget(Tile coinTile)
    {
        selectedCharacter.stopMoving = true;
        selectedCharacter.Moving = false;

        selectedCharacter.characterTile = coinTile;

        selectedCharacter.Moving = false;
        selectedCharacter.characterTile.Occupied = true;
        selectedCharacter.characterTile.occupyingCharacter = selectedCharacter;

        if (currentInTrashCan)
        {
            _animator.SetTrigger("Throw");
            currentInTrashCan = !currentInTrashCan;
            currentTrashLevel = 0;
        }
        else
        {
            _animator.SetTrigger("PickUp");
            currentTrashLevel++;
        }

        if(currentTrashLevel >= trashCapacity)
        {
            currentInTrashCan = !currentInTrashCan;
            currentTrashLevel = 0;
        }

        StartCoroutine(SelectNextTarget());
    }

    private IEnumerator SelectNextTarget()
    {
        yield return new WaitUntil(() => moveOn);
        moveOn = false;
        Debug.Log("I'm Here");
        if (currentInTrashCan)
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
    {
        if (selectedCharacter == null || selectedCharacter.Moving == true)
        {
            return;
        }

        Debug.Log("i'm retrieving path");


        if (RetrievePath(out Path newPath))
        {
            oldPath = newPath;
            selectedCharacter.stopMoving = false;
            selectedCharacter.StartMove(newPath);
            //   selectedCharacter = null;
        }
    }

    bool RetrievePath(out Path path)
    {
        Debug.Log("CurrentTile: " + currentTile);
        path = pathfinder.FindPath(selectedCharacter.characterTile, currentTile);

        if (path == null || path == Lastpath)
            return false;
        return true;
    }



}