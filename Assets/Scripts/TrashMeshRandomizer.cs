using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashMeshRandomizer : MonoBehaviour
{
    private void Awake()
    {
        int childCount = this.transform.childCount;

        if (childCount == 0)
        {
            Debug.LogWarning("No children found on this GameObject.");
            return;
        }

        // Select a random child to keep and enable
        int randomIndex = Random.Range(0, childCount);

        for (int i = 0; i < childCount; i++)
        {
            Transform child = this.transform.GetChild(i);

            if (i == randomIndex)
            {
                // Enable the randomly selected child
                child.gameObject.SetActive(true);
            }
            else
            {
                // Destroy the unselected children
                Destroy(child.gameObject);
            }
        }
    }
}
