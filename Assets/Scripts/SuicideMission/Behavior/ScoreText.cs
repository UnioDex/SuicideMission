﻿using System.Collections;
using SuicideMission.Objects;
using TMPro;
using UnityEngine;

namespace SuicideMission.Behavior
{
    public class ScoreText : MonoBehaviour
    {
        private TextMeshProUGUI scoreText;
        private GameSession gameSession;

        private bool loading = true;

        private IEnumerator Start()
        {
            yield return new WaitForFixedUpdate();
            gameSession = FindObjectOfType<GameSession>();
            scoreText = GetComponent<TextMeshProUGUI>();
            loading = false;
        }

        private void Update()
        {
            if (loading) return;
            scoreText.text = gameSession.GetScore().ToString();
        }
    }
}