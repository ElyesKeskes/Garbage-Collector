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
        CoinManager.Instance.RotateTowards(transform.GetChild(0));
        CoinManager.Instance.pickUpTrash = false;
        CoinManager.Instance.coins.Remove(this);
        CoinManager.Instance.OnCoinAcquiredChangeTarget(coinTile);
        yield return new WaitUntil(() => CoinManager.Instance.pickUpTrash);

        CoinManager.Instance.pickUpTrash = false;
        Rigidbody rb = transform.GetChild(0).GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        transform.GetChild(0).transform.localPosition = Vector3.zero;
        transform.GetChild(0).transform.localRotation = Quaternion.identity;
        transform.parent = CoinManager.Instance.handTransform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        yield return new WaitUntil(() => CoinManager.Instance.pickUpTrash);

        CoinManager.Instance.pickUpTrash = false;
        transform.parent = CoinManager.Instance.trashBagTransform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    private IEnumerator LerpTrashToTrashCan(Transform trash, float duration)
    {
       

        Vector3 startPosition = trash.position;

        Vector3 targetPosition = transform.position;

        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            trash.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        trash.position = targetPosition;

        trash.parent = transform;
    }


    private IEnumerator TrashCanCoroutine()
    {
        CoinManager.Instance.RotateTowards(transform);
        CoinManager.Instance.OnCoinAcquiredChangeTarget(coinTile);
        yield return new WaitUntil(() => CoinManager.Instance.pickUpTrash);
        CoinManager.Instance.pickUpTrash = false;

        foreach (Transform child in CoinManager.Instance.trashBagTransform)
        {
            CoinManager.Instance.currentValue++;
            CoinManager.Instance.currentTXT.text = CoinManager.Instance.currentValue.ToString();
            StartCoroutine(LerpTrashToTrashCan(child, 0.6f));
        }
        
        if(CoinManager.Instance.currentValue >= CoinManager.Instance.coinRandomizer.nbCoins)
        {
            CoinManager.Instance.winImg.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void AcquireTrashCan()
    {
        StartCoroutine(TrashCanCoroutine());
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
                if (CoinManager.Instance.currentInTrashCan)
                {
                    AcquireTrashCan();
                }
            }
        }
    }
}