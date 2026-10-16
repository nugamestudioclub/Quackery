using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class TASDriver : MonoBehaviour
{
    [SerializeField] private TextAsset tasSource;

    static float timeAddAll = 1.0f; // Add a little time to prevent dumb stuff in unity editor

    private class KeyTimeRange {
        public float start;
        public float stop;
        public bool used;
        public bool usedRelease;

        public KeyTimeRange(float start_, float stop_) {
            start = start_ + TASDriver.timeAddAll;
            stop = stop_ + TASDriver.timeAddAll;
            used = false;
            usedRelease = false;
        }
    }

    private class CamMovePoint {
        public float start;
        public float x;
        public float y;
        public bool used;

        public CamMovePoint(float start_, float x_, float y_) {
            start = start_ + TASDriver.timeAddAll;
            x = x_;
            y = y_;
            used = false;
        }
    }

    private Dictionary<string, List<KeyTimeRange>> keyTimeMap = new Dictionary<string, List<KeyTimeRange>>();
    private List<CamMovePoint> camMovePoints = new List<CamMovePoint>();
    private float curTime = 0f;

    private void Start()
    {
        if (tasSource != null) {
            string content = tasSource.text;

            StringReader reader = new StringReader(content);
            string line;
            while ((line = reader.ReadLine()) != null) {
                if (line == "") {
                    continue;
                }

                // start|stop|cmd
                // OR: start|0|cam|x|y         (cam is the command to use here)
                string[] parts = line.Split('|');
                string startTime = parts[0];
                string stopTime = parts[1];
                string cmd = parts[2];

                if (cmd == "cam") {
                    camMovePoints.Add(new CamMovePoint(float.Parse(startTime), float.Parse(parts[3]), float.Parse(parts[4])));
                }
                else {
                    if (keyTimeMap.ContainsKey(cmd)) {
                        keyTimeMap[cmd].Add(new KeyTimeRange(float.Parse(startTime), float.Parse(stopTime)));
                    }
                    else {
                        keyTimeMap.Add(cmd, new List<KeyTimeRange>());
                        keyTimeMap[cmd].Add(new KeyTimeRange(float.Parse(startTime), float.Parse(stopTime)));
                    }
                }
            }
        }
    }

    private void Update()
    {
        curTime += Time.unscaledDeltaTime;

        //foreach (CamMovePoint p in camMovePoints) {
        //    Debug.Log($"{p.start}, {p.x}, {p.y}");
        //}
    }

    private bool isActive(string cmd) {
        if (!keyTimeMap.ContainsKey(cmd)) {
            return false;
        }
        foreach (KeyTimeRange k in keyTimeMap[cmd]) {
            if (k.start < curTime && k.stop > curTime) {
                return true;
            }
        }
        return false;
    }

    private bool isActiveAndUse(string cmd) {
        if (!keyTimeMap.ContainsKey(cmd)) {
            return false;
        }
        foreach (KeyTimeRange k in keyTimeMap[cmd]) {
            if (!k.used && k.start < curTime) {
                k.used = true;
                return true;
            }
        }
        return false;
    }

    public Vector2 GetMoveInput() {
        int x = 0;
        int y = 0;

        if (isActive("w")) { y += 1; }
        if (isActive("s")) { y -= 1; }
        if (isActive("d")) { x += 1; }
        if (isActive("a")) { x -= 1; }

        return new Vector2(x, y);
    }

    public Vector2 GetLookInput() {
        foreach (CamMovePoint c in camMovePoints) {
            if (!c.used && c.start < curTime) {
                c.used = true;
                return new Vector2(c.x, c.y);
            }
        }
        return new Vector2(0, 0);
    }

    public bool GetJumpInput() {
        if (!keyTimeMap.ContainsKey("jump")) {
            return false;
        }
        foreach (KeyTimeRange k in keyTimeMap["jump"]) {
            if (!k.used && k.start < curTime) {
                k.used = true;
                return true;
            }
        }
        return false;
    }

    public bool GetJumpReleasedInput() {
        if (!keyTimeMap.ContainsKey("jump")) {
            return false;
        }
        foreach (KeyTimeRange k in keyTimeMap["jump"]) {
            if (!k.usedRelease && k.stop < curTime) {
                k.usedRelease = true;
                return true;
            }
        }
        return false;
    }

    public bool GetUsePrimaryInput() {
        if (!keyTimeMap.ContainsKey("item")) {
            return false;
        }
        foreach (KeyTimeRange k in keyTimeMap["item"]) {
            if (!k.used && k.start < curTime) {
                k.used = true;
                return true;
            }
        }
        return false;
    }

    public bool GetReleasePrimaryInput() {
        if (!keyTimeMap.ContainsKey("item")) {
            return false;
        }
        foreach (KeyTimeRange k in keyTimeMap["item"]) {
            if (!k.usedRelease && k.stop < curTime) {
                k.usedRelease = true;
                return true;
            }
        }
        return false;
    }

    public bool GetHb1Input() {
        return isActiveAndUse("1");
    }

    public bool GetHb2Input() {
        return isActiveAndUse("2");
    }

    public bool GetHb3Input() {
        return isActiveAndUse("3");
    }

    public bool GetRestartInput() {
        return isActiveAndUse("restart");
    }
}
