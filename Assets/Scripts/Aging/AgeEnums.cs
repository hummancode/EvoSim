
using System;
using UnityEngine;

public enum LifeStage
{
    Baby,
    Child,
    Adult,
    Elderly
}

[Serializable]
public struct AgeConfiguration
{
    public float maxAge;
    public float maturityAge;
    public float ageRate;
    public float minScale;
    public float maxScale;
    public Color babyColor;
    public Color adultColor;

    public static AgeConfiguration Default => new AgeConfiguration
    {
        maxAge = 100f,
        maturityAge = 20f,
        ageRate = 1f,
        minScale = 0.5f,
        maxScale = 1f,
        babyColor = new Color(0.2f, 0.4f, 0.8f, 1f),
        adultColor = Color.white
    };
}