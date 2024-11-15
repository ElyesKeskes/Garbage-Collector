using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trash : MonoBehaviour
{
    public bool hasBeenPickedUp = false;
    public Tile trashTile;

    [SerializeField] LayerMask GroundLayerMask;

    public void ReStart(AgentManager _agentManager)
    {
        Debug.Log("Restarting : " + transform.GetChild(0).name);
        if (Physics.Raycast(transform.GetChild(0).position, -transform.GetChild(0).up, out RaycastHit hit, 150f, GroundLayerMask))
        {
            trashTile = hit.transform.GetComponent<Tile>();
            Debug.Log("Found a tile for : " + transform.GetChild(0).name + " its the tile : " + trashTile.name);
            transform.position = new Vector3(trashTile.transform.position.x, trashTile.transform.position.y + 0.5f, trashTile.transform.position.z);
        }
        else
        {
            Transform newPos = _agentManager.agentCharacter.characterTile.connectedTile.transform;
            transform.position = new Vector3(newPos.position.x, newPos.position.y + 0.5f, newPos.position.z);
        }
    }

    public void Start()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 150f, GroundLayerMask))
        {
            trashTile = hit.transform.GetComponent<Tile>();
        }
    }

    public void OnTrashPickup(AgentManager _agentManager)
    {
        hasBeenPickedUp = true;
        transform.GetChild(1).gameObject.SetActive(false);
        StartCoroutine(GetTrash(_agentManager));
    }

    private IEnumerator GetTrash(AgentManager _agentManager)
    {
        _agentManager.RotateTowards(transform.GetChild(0));
        _agentManager.pickUpTrash = false;
        _agentManager.trashPieces.Remove(this);
        _agentManager.OnTrashPickedUpChangeTarget(trashTile);
        yield return new WaitUntil(() => _agentManager.pickUpTrash);

        _agentManager.pickUpTrash = false;
        Rigidbody rb = transform.GetChild(0).GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        transform.GetChild(0).transform.localPosition = Vector3.zero;
        transform.GetChild(0).transform.localRotation = Quaternion.identity;
        transform.parent = _agentManager.handTransform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        yield return new WaitUntil(() => _agentManager.pickUpTrash);

        _agentManager.pickUpTrash = false;
        transform.parent = _agentManager.trashBagTransform;
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


    private IEnumerator TrashCanCoroutine(AgentManager _agentManager)
    {
        _agentManager.RotateTowards(transform);
        _agentManager.OnTrashPickedUpChangeTarget(trashTile);
        yield return new WaitUntil(() => _agentManager.pickUpTrash);
        _agentManager.pickUpTrash = false;

        foreach (Transform child in _agentManager.trashBagTransform)
        {
            _agentManager.currentValue++;
            _agentManager.currentTXT.text = _agentManager.currentValue.ToString();
            StartCoroutine(LerpTrashToTrashCan(child, 0.6f));
        }
        
        if(_agentManager.currentValue >= _agentManager.trashRandomizer.NumberofTrashToSpawn)
        {
            _agentManager.winImg.SetActive(true);
            yield return new WaitForSeconds(1.25f);
            Time.timeScale = 0f;
        }
    }

    public void StartTrashcanCoroutine(AgentManager _agentManager)
    {
        StartCoroutine(TrashCanCoroutine(_agentManager));
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
            AgentManager _agentManager = other.transform.parent.GetComponent<AgentManager>();

            if(tag == "Trash")
            {
                if (!_agentManager.currentlyOnTrashcan)
                {
                    OnTrashPickup(_agentManager);
                }
            }
            else
            {
                if (_agentManager.currentlyOnTrashcan)
                {
                    StartTrashcanCoroutine(_agentManager);
                }
            }
        }
    }
}