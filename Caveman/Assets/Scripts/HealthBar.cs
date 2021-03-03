using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider sliderRight;
    public Slider sliderLeft;
    public Text textValue;

    public void SetMaxHealth(int health)
    {
        sliderRight.maxValue = health/2;
        sliderLeft.maxValue = health/2;
        SetText();
    }

    public void SetHealth(int health)
    {
        sliderRight.value = health/2;
        sliderLeft.value = health/2;
        SetText();
    }

    public float GetHealth()
	{
        float health = sliderLeft.maxValue + sliderRight.maxValue;
        return health;
	}

    private void SetText()
    {
        textValue.text = (Mathf.RoundToInt(sliderRight.value + sliderLeft.value)).ToString();
    }
}
