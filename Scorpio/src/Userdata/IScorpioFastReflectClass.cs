﻿using System;
using Scorpio;
namespace Scorpio.Userdata {
    public interface IScorpioFastReflectClass {
        FastReflectUserdataMethod GetConstructor();
        Type GetVariableType(string name);
        object GetValue(object obj, string name);
        void SetValue(object obj, string name, ScriptObject value);
    }
}
