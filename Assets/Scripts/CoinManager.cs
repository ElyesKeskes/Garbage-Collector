//keeps a list of coins

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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


    void Start()
    {
        Invoke("DelayedStart", 3f);
    }

    void DelayedStart()
    {
        if (pathfinder == null)
            pathfinder = GameObject.Find("Pathfinder").GetComponent<Pathfinder>();
        GetAllCoinsByTag();
        GetClosestTrash();
        NavigateToTile();
    }

    public void OnCoinAcquiredChangeTarget(Tile coinTile)
    {
        selectedCharacter.stopMoving = true;
        selectedCharacter.Moving = false;

        selectedCharacter.characterTile = coinTile;

        //selectedCharacter.transform.position = coinTile.transform.position;
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
            

        if (RetrievePath(out Path newPath))
        {
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