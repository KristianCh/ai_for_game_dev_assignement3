using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerPointsUI : MonoBehaviour
{
    [SerializeField]
    private Image avatarImage;

    [SerializeField]
    private TMP_Text scoreText;

    private AbstractPlayer player;

    public void Initialize(AbstractPlayer player)
    {
        this.player = player;
        
        avatarImage.sprite = player.Sprite;
        avatarImage.color = player.SpriteColor;
        
        UpdateScore();
    }

    public void UpdateScore()
    {
        scoreText.text = player.Points.ToString();
    }
}
