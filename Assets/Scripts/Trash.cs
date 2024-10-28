using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trash : MonoBehaviour
{
    public bool hasBeenPickedUp = false;
    public Tile trashTile;

    [SerializeField] LayerMask GroundLayerMask;


    public void ReStart()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 150f, GroundLayerMask))
        {
            trashTile = hit.transform.GetComponent<Tile>();
        }
    }

    public void Start()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 150f, GroundLayerMask))
        {
            trashTile = hit.transform.GetComponent<Tile>();
        }
    }

    public void OnTrashPickup()
    {
        hasBeenPickedUp = true;
        Destroy(transform.GetChild(1).gameObject);
        StartCoroutine(GetTrash());
    }

    private IEnumerator GetTrash()
    {
        AgentManager.Instance.RotateTowards(transform.GetChild(0));
        AgentManager.Instance.pickUpTrash = false;
        AgentManager.Instance.trashPieces.Remove(this);
        AgentManager.Instance.OnTrashPickedUpChangeTarget(trashTile);
        yield return new WaitUntil(() => AgentManager.Instance.pickUpTrash);

        AgentManager.Instance.pickUpTrash = false;
        Rigidbody rb = transform.GetChild(0).GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        transform.GetChild(0).transform.localPosition = Vector3.zero;
        transform.GetChild(0).transform.localRotation = Quaternion.identity;
        transform.parent = AgentManager.Instance.handTransform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        yield return new WaitUntil(() => AgentManager.Instance.pickUpTrash);

        AgentManager.Instance.pickUpTrash = false;
        transform.parent = AgentManager.Instance.trashBagTransform;
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
        AgentManager.Instance.RotateTowards(transform);
        AgentManager.Instance.OnTrashPickedUpChangeTarget(trashTile);
        yield return new WaitUntil(() => AgentManager.Instance.pickUpTrash);
        AgentManager.Instance.pickUpTrash = false;

        foreach (Transform child in AgentManager.Instance.trashBagTransform)
        {
            AgentManager.Instance.currentValue++;
            AgentManager.Instance.currentTXT.text = AgentManager.Instance.currentValue.ToString();
            StartCoroutine(LerpTrashToTrashCan(child, 0.6f));
        }
        
        if(AgentManager.Instance.currentValue >= AgentManager.Instance.trashRandomizer.NumberofTrashToSpawn)
        {
            AgentManager.Instance.winImg.SetActive(true);
            yield return new WaitForSeconds(1.25f);
            Time.timeScale = 0f;
        }
    }

    public void StartTrashcanCoroutine()
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
        if (hasBeenPickedUp)
        {
            return;
        }

        if (other.tag == "Player")
        {
            if(tag == "Trash")
            {
                if (!AgentManager.Instance.currentlyOnTrashcan)
                {
                    OnTrashPickup();
                }
            }
            else
            {
                if (AgentManager.Instance.currentlyOnTrashcan)
                {
                    StartTrashcanCoroutine();
                }
            }
        }
    }
}