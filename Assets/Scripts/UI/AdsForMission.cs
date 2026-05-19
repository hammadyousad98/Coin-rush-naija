using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AdsForMission : MonoBehaviour
{
    public MissionUI missionUI;

    public Text newMissionText;
    public Button adsButton;

    void OnEnable ()
    {
        adsButton.gameObject.SetActive(true);
        newMissionText.gameObject.SetActive(true);
    }

    public void ShowAds()
    {
        if (AdManager.instance != null)
        {
            AdManager.instance.ShowRewardedAd((reward) => {
                AddNewMission();
            });
        }
    }

    void AddNewMission()
    {
        PlayerData.instance.AddMission();
        PlayerData.instance.Save();
        StartCoroutine(missionUI.Open());
    }
}
