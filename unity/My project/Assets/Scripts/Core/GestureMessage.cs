using System;

[Serializable]
public class GestureParams
{
    public float x;
    public float y;
    public float pinchStrength;
    public float angleDelta;
    public float totalAngle;
}

[Serializable]
public class GestureMessage
{
    public string type;
    public int seq;
    public double timestamp;
    public string hand;
    public string gesture;
    public string state;
    public float confidence;
    public GestureParams @params;

    public bool IsGesture(string name, string expectedState = null)
    {
        if (type != "gesture" || gesture != name) return false;
        return expectedState == null || state == expectedState;
    }
}

