﻿//
// OscKlak - OSC (Open Sound Control) extension for Klak
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using UnityEngine.Events;
using System;
using Klak.Math;

namespace Klak.Osc
{
    [AddComponentMenu("Klak/OSC/OSC Input")]
    public class OscInput : MonoBehaviour
    {
        #region Nested Public Classes

        public enum EventType {
            Trigger, Gate, Value
        }

        [Serializable]
        public class ValueEvent : UnityEvent<float> {}

        #endregion

        #region Editable Properties

        [SerializeField]
        string _address = "/1/fader1";

        [SerializeField]
        AnimationCurve _inputCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        EventType _eventType = EventType.Value;

        [SerializeField]
        float _outputValue0 = 0.0f;

        [SerializeField]
        float _outputValue1 = 1.0f;

        [SerializeField]
        FloatInterpolator.Config _interpolator;

        [SerializeField]
        UnityEvent _triggerEvent;

        [SerializeField]
        UnityEvent _gateOnEvent;

        [SerializeField]
        UnityEvent _gateOffEvent;

        [SerializeField]
        ValueEvent _valueEvent;

        #endregion

        #region Private Variables And Methods

        string _registeredAddress;
        FloatInterpolator _value;
        float _lastInputValue;

        float CalculateTargetValue(float inputValue)
        {
            var p = _inputCurve.Evaluate(inputValue);
            return BasicMath.Lerp(_outputValue0, _outputValue1, p);
        }

        void OscDataCallback(float data)
        {
            UpdateState(data);
        }

        void UpdateState(float inputValue)
        {
            if (_eventType == EventType.Value)
            {
                // update the target value for the interpolator
                _value.targetValue =
                    BasicMath.Lerp(_outputValue0, _outputValue1, inputValue);
            }

            _lastInputValue = inputValue;
        }

        #endregion

        #region MonoBehaviour Functions

        void OnEnable()
        {
            if (!String.IsNullOrEmpty(_address))
                OscMaster.messageHandler.
                    AddDataCallback(_address, OscDataCallback);

            _registeredAddress = _address;
        }

        void OnDisable()
        {
            if (!String.IsNullOrEmpty(_registeredAddress))
                OscMaster.messageHandler.
                    RemoveDataCallback(_registeredAddress, OscDataCallback);
        }

        void Start()
        {
            _value = new FloatInterpolator(0, _interpolator);
        }

        void Update()
        {
            if (_eventType == EventType.Value)
                _valueEvent.Invoke(_value.Step());

            #if UNITY_EDITOR
            // re-register the osc data callback
            if (_address != _registeredAddress)
            {
                OnDisable();
                OnEnable();
            }
            #endif
        }

        #endregion
    }
}
