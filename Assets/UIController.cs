using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public RotateScript[] rotateScripts;
   public AgentManager _agentManager;
    public TextMeshProUGUI PlayerWins;
    public AdHocCharacter _adHocCharacter;
    public MonteCarloAgent _monteCarloAgent;
    public TextMeshProUGUI _agentManagerText, _adHocCharacterText, _monteCarloAgentText;
    public GameObject[] Cams;
    int index = 0;
    public TextMeshProUGUI _agentManagerCurrentTrashCountText, _adHocCharacterCurrentTrashCountText, _monteCarloAgentCurrentTrashCountText;
    public TextMeshProUGUI _agentManagerCapText, _adHocCharacterCapText, _monteCarloAgentCapText;
    // Update is called once per frame
    void Update()
    {
        _agentManagerText.text = _agentManager.TotalTrashCollected.ToString();
        _adHocCharacterText.text = _adHocCharacter.TotalTrashCollected.ToString();
        _monteCarloAgentText.text = _monteCarloAgent.TotalTrashCollected.ToString();



        _agentManagerCurrentTrashCountText.text = _agentManager.currentTrashCount.ToString();
        _adHocCharacterCurrentTrashCountText.text = _adHocCharacter.currentTrashCount.ToString();
        _monteCarloAgentCurrentTrashCountText.text = _monteCarloAgent.currentTrashCount.ToString();

        if (Input.GetKey(KeyCode.Mouse0)) {
            changeCam();
        }
        if(_agentManager.trashRandomizer.NumberofTrashToSpawn - 1 == (_monteCarloAgent.TotalTrashCollected + _adHocCharacter.TotalTrashCollected + _agentManager.TotalTrashCollected) ||
            _agentManager.trashRandomizer.NumberofTrashToSpawn == (_monteCarloAgent.TotalTrashCollected + _adHocCharacter.TotalTrashCollected + _agentManager.TotalTrashCollected))
        {
            Compare(_agentManager.TotalTrashCollected, _adHocCharacter.TotalTrashCollected, _monteCarloAgent.TotalTrashCollected);
        }
        else
        {
            if(_agentManager.trashRandomizer.NumberofTrashToSpawn - 1 <= (_monteCarloAgent.currentTrashCount + _adHocCharacter.currentTrashCount + _agentManager.currentTrashCount) + (_monteCarloAgent.TotalTrashCollected + _adHocCharacter.TotalTrashCollected + _agentManager.TotalTrashCollected))
            {
                Compare(_agentManager.TotalTrashCollected, _adHocCharacter.TotalTrashCollected, _monteCarloAgent.TotalTrashCollected);
            }
        }
    }

    private void Compare(int a, int b, int c)
    {
        PlayerWins.gameObject.SetActive(true);
        if(a < b) {
            if (b < c)
            {
                PlayerWins.text = " MONTE CARLO WINS!";
            }
            else {
                PlayerWins.text = "ADHOC WINS!";
            }
        }
        else
        {
            if (a < c)
            {
                PlayerWins.text = " MONTE CARLO WINS!";
            }
            else {
                PlayerWins.text = "A* WINS!";
            }
        }
        Time.timeScale = 0f;
    }

    private void changeCam()
    {
        Cams[index].gameObject.SetActive(false);
        index++;
        if (index == Cams.Length)
        {
            index = 0;

        }
        Cams[index].gameObject.SetActive(true);
        foreach (var rotateScript in rotateScripts) {
            rotateScript.cam = Cams[index].transform.GetChild(0).gameObject;
        }

    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(3f);
        _agentManagerCapText.text = (_agentManager.trashRandomizer.NumberofTrashToSpawn - 1).ToString();
        _adHocCharacterCapText.text = (_agentManager.trashRandomizer.NumberofTrashToSpawn - 1).ToString();
        _monteCarloAgentCapText.text = (_agentManager.trashRandomizer.NumberofTrashToSpawn - 1).ToString();
    }
    
}
