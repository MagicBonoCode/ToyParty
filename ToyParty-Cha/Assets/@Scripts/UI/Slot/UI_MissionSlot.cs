using UnityEngine;

public class UI_MissionSlot : UI_Base
{
    public enum Texts
    {
        Text_MissionCount,
    }

    public enum Images
    { 
        Image_MissionIcon,
    }

    private Sprite _missionIcon;
    private int _missionCount;

    protected override void Awake()
    {
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));
    }

    protected override void Start()
    {
    }

    public void SetInfo(Sprite sprite, int count)
    {
        _missionIcon = sprite;
        _missionCount = count;

        RefreshUI();
    }

    private void RefreshUI()
    {
        GetImage((int)Images.Image_MissionIcon).sprite = _missionIcon;
        GetText((int)Texts.Text_MissionCount).text = _missionCount.ToString();
    }
}
