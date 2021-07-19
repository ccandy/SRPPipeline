using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class AvgFPSCounter : MonoBehaviour
{
    Text m_Text = null;

    int MaxFrameInCalculation = 200;

    int[] UsingTimeInFrame = null;

    int TotalTime = 0;

    int CalCount = 0;

    bool HasFullList = false;

    public int AvgFPS = 0;

    const float fpsMeasurePeriod = 0.5f;
    private int m_FpsAccumulator = 0;
    private float m_FpsNextPeriod = 0;
    private int m_CurrentFps;
    const string display = "{0}fps({1}ms)";

    void Start()
    {
        m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        m_Text = GetComponent<Text>();
        //InitFPS();
    }

    void Update()
    {
        if (!m_Text.enabled) return;
        m_FpsAccumulator++;
        if (Time.realtimeSinceStartup > m_FpsNextPeriod)
        {
            m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
            m_FpsAccumulator = 0;
            m_FpsNextPeriod += fpsMeasurePeriod;

        }

        //UpdateFPS( (int)(Time.unscaledDeltaTime * 1000));

        //m_Text.text = string.Format(display, m_CurrentFps, AvgFPS);
        m_Text.text = string.Format(display, m_CurrentFps, m_CurrentFps > 0 ? 1000 / m_CurrentFps : 0);
    }

    void OnDestroy()
    {

    }

    void OnShowFPSChange(params object[] Param)
    {
        if (m_Text == null)
            return;

    }

    public void InitFPS()
    {
        UsingTimeInFrame = new int[MaxFrameInCalculation];
        CalCount = 0;
        HasFullList = false;
    }

    public void InitFPS(int NewMaxFrameInCalculation)
    {
        MaxFrameInCalculation = NewMaxFrameInCalculation;
        InitFPS();
    }

    public void UpdateFPS(int FrameUsingMSTime)
    {
        if (HasFullList)
        {
            TotalTime -= UsingTimeInFrame[CalCount];
        }


        UsingTimeInFrame[CalCount] = FrameUsingMSTime;
        TotalTime += FrameUsingMSTime;

        ++CalCount;

        HasFullList |= CalCount >= MaxFrameInCalculation;
        CalCount %= MaxFrameInCalculation;

        if (TotalTime == 0)
            return;

        if (HasFullList)
        {
            AvgFPS = MaxFrameInCalculation * 1000 / TotalTime;
        }
        else
        {
            AvgFPS = CalCount * 1000 / TotalTime;
        }
    }
}