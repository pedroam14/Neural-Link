using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MZGenerationFailureException : Exception {
    public MZGenerationFailureException(string message) : base(message) {
    }

    public MZGenerationFailureException(string message, Exception cause) : base(message) {
    }

}
