using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.WindowTypes;

public class MainScreen : LayoutWindowType {

    private GenericComponent comp1;
    private ButtonComponent comp2;
    private ListComponent list;
    private ProgressComponent progressComponent;

    public Texture texture;
    
    public override void OnInit() {
        
        base.OnInit();
        
        this.GetLayoutComponent(out this.comp1, Algorithm.GetFirstTypeAny);
        this.GetLayoutComponent(out this.comp2);
        this.GetLayoutComponent(out this.list);
        this.GetLayoutComponent(out this.progressComponent);

    }

    public override void OnShowBegin() {
        
        base.OnShowBegin();
        
        Debug.Log("OnShowBegin");
        
        this.comp2.SetCallback(this.HideCurrentWindow);
        
        this.progressComponent.SetNormalizedValue(0.5f);
        this.progressComponent.SetCallback((value) => { Debug.Log("Value: " + value); });
        
        this.comp1.Get<ImageComponent>().SetImage(this.texture);
        this.comp1.Get<TextComponent>().SetValue(1234, SourceValue.Seconds, TimeResult.TimeMS);
        this.comp1.Get<TextComponent>().SetValue(1234, SourceValue.Seconds, TimeResult.TimeHMS);
        this.comp1.Get<TextComponent>().SetTimeResultString(TimeValue.Days, new TextComponent.FormatTimeString(@"d{0}", " days "));
        this.comp1.Get<TextComponent>().SetValue(1234, SourceValue.Seconds, TimeResult.TimeDHMS);
        this.comp1.Get<TextComponent>().SetValue(12345678, SourceValue.Seconds, TimeResult.TimeDHMS, TimeResult.TimeMS);
        
        this.list.SetItems<ButtonComponent>(20, (item, index) => {
            
            item.Show();
            
        });
        
    }

}
