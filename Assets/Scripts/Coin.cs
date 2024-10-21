using System;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public Tile coinTile;

    [SerializeField] LayerMask GroundLayerMask;

    void Start()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 50f, GroundLayerMask))
        {
            coinTile = hit.transform.GetComponent<Tile>();
            //Debug log to check if the coin is on a tile
        }
    }

    public void AcquireCoin()
    {
        CoinManager.Instance.coins.Remove(this);
        CoinManager.Instance.OnCoinAcquiredChangeTarget(coinTile);
        Destroy(gameObject);
    }

    public void AcquireTrashCan()
    {
        CoinManager.Instance.OnCoinAcquiredChangeTarget(coinTile);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.up * 50f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if(tag == "Trash")
            {
                if (!CoinManager.Instance.currentInTrashCan)
                {
                    AcquireCoin();
                }
            }
            else
            {
                AcquireTrashCan();
            }
        }
    }
}