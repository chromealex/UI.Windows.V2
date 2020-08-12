using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI.Windows;

public class OpenWindow : MonoBehaviour {

    public WindowBase source;
    private WindowBase instance;
    
    public UnityEngine.UI.Button button;
    public UnityEngine.UI.Button close;
    public UnityEngine.UI.Button closeAll;
    public UnityEngine.UI.Button closeAndClean;
    
    public void Start() {
        
        this.button.onClick.AddListener(this.DoOpen);
        this.close.onClick.AddListener(this.DoClose);
        this.closeAndClean.onClick.AddListener(this.DoCloseAndClean);
        this.closeAll.onClick.AddListener(this.DoCloseAll);
        
    }

    public void DoClose() {
        
        if (this.instance != null) this.instance.Hide();
        this.instance = null;

    }

    public void DoCloseAndClean() {

        if (this.instance != null) {
            
            this.instance.Hide(TransitionParameters.Default.ReplaceCallback(() => {
                
                WindowSystem.Clean(this.instance);
                this.instance = null;
                
            }));

        }

    }

    public void DoCloseAll() {

        WindowSystem.HideAll();

    }

    public void DoOpen() {
        
        WindowSystem.Show(this.source, (x) => this.instance = x);
        
    }

}
