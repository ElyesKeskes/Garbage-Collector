using System;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public Tile coinTile;
    public MeshRenderer Crenderer;

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
        Crenderer.enabled = false;
        //Remove this Coin from list coins in CoinManager.Instance.
        CoinManager.Instance.coins.Remove(this);
        Destroy(gameObject);
        //Find and destroy this reference from coin list in coinManager Instance
    }

    //Draw debug lines to show the raycast
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.up * 50f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            AcquireCoin();
            CoinManager.Instance.OnCoinAcquiredChangeTarget();
        }

    }

}