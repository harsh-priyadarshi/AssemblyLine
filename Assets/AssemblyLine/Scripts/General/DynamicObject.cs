﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL
{
    public class DynamicObject : MonoBehaviour, IResettable
    {
        CustomTransform originalTransorm;

        public void Init()
        {
            originalTransorm = new CustomTransform();
            originalTransorm.Extract(transform);
        }

        public void OnReset()
        {
            originalTransorm.Apply(transform);
        }

    }
}
