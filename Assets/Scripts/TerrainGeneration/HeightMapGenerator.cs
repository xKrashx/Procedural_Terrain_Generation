﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {
    
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre) {

        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);

        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

        float maxValue = float.MinValue;
        float minValue = float.MaxValue;

        for (int i = 0; i < width; i++) {

            for (int j = 0; j < height; j++) {

                values[i, j] *= heightCurve_threadsafe.Evaluate(values[i, j]) * settings.heightMultiplier;

                if (values[i, j] > maxValue) maxValue = values[i, j];
                if (values[i, j] < minValue) minValue = values[i, j];

            }

        }

        return new HeightMap(values, minValue, maxValue);

    }

}

public struct HeightMap {

    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;

    public HeightMap(float[,] heightMap, float minValue, float maxValue) {

        this.values = heightMap;
        this.minValue = minValue;
        this.maxValue = maxValue;

    }
}