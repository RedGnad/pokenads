using UnityEngine;
using TMPro;
using System;

public class ResetCountdown : MonoBehaviour
{
    // Assignez ce champ dans l'inspecteur (TextMeshProUGUI affichant le compte à rebours)
    public TextMeshProUGUI countdownText;

    void Update()
    {
        // Date actuelle en UTC
        DateTime now = DateTime.UtcNow;
        // Calcul de minuit du jour suivant
        DateTime tomorrowMidnight = now.Date.AddDays(1);
        TimeSpan timeLeft = tomorrowMidnight - now;

        countdownText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
            timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds);
    }
}