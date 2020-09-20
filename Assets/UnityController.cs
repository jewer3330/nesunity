using System.Collections;
using System.Collections.Generic;
using dotNES;
using dotNES.Controllers;
using UnityEngine;

class UnityController : IController
{
    private int data;
    private int serialData;
    private bool strobing;

    public bool debug;
    // bit:   	 7     6     5     4     3     2     1     0
    // button:	 A     B  Select  Start  Up   Down  Left   Right

    public readonly Dictionary<UnityEngine.KeyCode, int> _keyMapping = new Dictionary<UnityEngine.KeyCode, int>
        {
            {UnityEngine.KeyCode.K, 7},
            {UnityEngine.KeyCode.J, 6},
            {UnityEngine.KeyCode.G, 5},
            {UnityEngine.KeyCode.H, 4},
            {UnityEngine.KeyCode.W, 3},
            {UnityEngine.KeyCode.S, 2},
            {UnityEngine.KeyCode.A, 1},
            {UnityEngine.KeyCode.D, 0},
        };

    public void Strobe(bool on)
    {
        serialData = data;
        strobing = on;
    }

    public int ReadState()
    {
        int ret = ((serialData & 0x80) > 0).AsByte();
        if (!strobing)
        {
            serialData <<= 1;
            serialData &= 0xFF;
        }
        return ret;
    }

    public void PressKey(UnityEngine.KeyCode e)
    {
        if (e == UnityEngine.KeyCode.P) debug ^= true;
        if (!_keyMapping.ContainsKey(e)) return;
        data |= 1 << _keyMapping[e];
    }

    public void ReleaseKey(UnityEngine.KeyCode e)
    {
        if (!_keyMapping.ContainsKey(e)) return;
        data &= ~(1 << _keyMapping[e]);
    }
}

