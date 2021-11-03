using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextFormatter
{
    // ex) 15 -> "D-15"
    public static string GetBasicDDay(float value)
    {
        return "D" + string.Format("{0:-0;+0}", value);
    }

    // ex) 780.0 -> "1:00 PM"
    public static string GetBasicTime(float value)
    {
        int valueInt = (int)value;
        string curTime;
        int t = valueInt / 60 - 12 * (valueInt / 720);
        if (valueInt % 720 < 60) t += 12;    // 12:00am~12:50am or 12:00pm~12:50pm
        curTime = t.ToString("D2") + ":" + (valueInt % 60).ToString("D2");

        return (valueInt >= 720) ?
        curTime += " PM" :
        curTime += " AM";
    }

    // ex) 350.0 -> "6시간 50분"
    public static string GetEstimatedTime(float value)
    {
        string estTime = "";
        if(((int)value/60)>0) estTime += (int)value / 60 + "시간 ";
        if ((int)value % 60 != 0) estTime += (int)value % 60 + "분";
        return estTime;
    }

    // ex) 3000 -> "3,000$"
    public static string GetMoney(float value)
    {
        return string.Format("{0:#,##0}", (int)value) + "$";
    }

    public static string DefStatFormat(string name, float cur, float max, float effect)
    {
        return name + ": " + cur + "  [" + (cur - effect) + "(/" + max + ")" + string.Format("{0:+0;-0}", effect) + "]";
    }

    public static string BatStatFormat(string[] names, float[] curPoints)
    {
        string[] stats = new string[3];

        for (int i = 0; i < stats.Length; i++)
        {
            if (i == 1) stats[i] = names[i] + ":" + string.Format("{0,7}", curPoints[i].ToString("F2"));
            else stats[i] = names[i] + ":" + string.Format("{0,7}", curPoints[i]);
        }

        return stats[0]+"\n"+stats[1]+"\n"+stats[2];
    }

    public static string AOutOfB(float a, float b)
    {
        return a+"/"+b;
    }
}
