//keeps a list of coins

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CoinManager : Singleton<CoinManager>
{
    public List<Coin> coins = new List<Coin>();
    public Coin currentTarget;

    public Character selectedCharacter;
    public Pathfinder pathfinder;
    Path Lastpath;
    Tile currentTile;

    public void SetTarget(Coin coin)
    {
        currentTarget = coin;
        currentTile = coin.coinTile;
    }

    void GetAllCoinsByTag()
    {
        coins = GameObject.FindGameObjectsWithTag("Coin").ToList().ConvertAll(x => x.GetComponent<Coin>());
    }

    void Update()
    {

    }

    void GetClosestCoin()
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


    void Start()
    {
        Invoke("DelayedStart", 3f);
    }

    void DelayedStart()
    {
        if (pathfinder == null)
            pathfinder = GameObject.Find("Pathfinder").GetComponent<Pathfinder>();
        GetAllCoinsByTag();
        GetClosestCoin();
        NavigateToTile();
    }

    public void OnCoinAcquiredChangeTarget()
    {
        selectedCharacter.Moving = false;

        GetClosestCoin();
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