﻿using UnityEngine;
using System.Collections;

public class KeepInScenes : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
