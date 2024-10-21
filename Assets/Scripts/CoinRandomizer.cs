using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinRandomizer : MonoBehaviour
{
    public Transform gridParent;
    public List<Transform> tileList = new List<Transform>();
    public GameObject coinPrefab;
    public int nbCoins = 10;
    public Transform trashCanParent;

    private List<Transform> occupiedTiles = new List<Transform>();

    private void Start()
    {
        Invoke("StartWithDelay", 1f);
    }

    private void StartWithDelay()
    {
        foreach (Transform trashCan in trashCanParent)
        {
            Coin coinScript = trashCan.GetComponent<Coin>();
            if (coinScript != null && coinScript.coinTile != null)
            {
                occupiedTiles.Add(coinScript.coinTile.transform);
            }
        }

        foreach (Transform grid in gridParent)
        {
            foreach (Transform tile in grid)
            {
                if (!occupiedTiles.Contains(tile))
                {
                    tileList.Add(tile);
                }
            }
        }

        SpawnCoins();
    }

    void SpawnCoins()
    {
        int coinsToSpawn = Mathf.Min(nbCoins, tileList.Count);
        List<int> usedIndexes = new List<int>();

        for (int i = 0; i < coinsToSpawn; i++)
        {
            int randomIndex;

            do
            {
                randomIndex = Random.Range(0, tileList.Count);
            } while (usedIndexes.Contains(randomIndex));

            usedIndexes.Add(randomIndex);

            Transform selectedTile = tileList[randomIndex];
            Vector3 spawnPosition = selectedTile.position + Vector3.up * 0.35f;
            Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
