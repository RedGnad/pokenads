using UnityEngine;
using TMPro;
using System;

public class ResetCountdown : MonoBehaviour
{
    public TextMeshProUGUI countdownText;

    void Update()
    {
        DateTime now = DateTime.UtcNow;
        DateTime tomorrowMidnight = now.Date.AddDays(1);
        TimeSpan timeLeft = tomorrowMidnight - now;

        countdownText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
            timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds);
    }
}