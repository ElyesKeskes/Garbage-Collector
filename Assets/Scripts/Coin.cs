using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public bool isAchieved = false;
    public Tile coinTile;

    [SerializeField] LayerMask GroundLayerMask;

    void Start()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 50f, GroundLayerMask))
        {
            coinTile = hit.transform.GetComponent<Tile>();
        }
    }

    public void AcquireCoin()
    {
        isAchieved = true;
        StartCoroutine(GetTrash());
    }

    private IEnumerator GetTrash()
    {
        CoinManager.Instance.pickUpTrash = false;
        CoinManager.Instance.coins.Remove(this);
        CoinManager.Instance.OnCoinAcquiredChangeTarget(coinTile);
        yield return new WaitUntil(() => CoinManager.Instance.pickUpTrash);
        transform.parent = CoinManager.Instance.handTransform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
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
        if (isAchieved)
        {
            return;
        }

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