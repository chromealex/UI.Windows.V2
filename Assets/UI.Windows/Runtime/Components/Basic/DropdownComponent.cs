using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Components {

    public class DropdownComponent : GenericComponent {

        public enum Side {

            Top,
            Left,
            Right,
            Bottom,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,

        }
        
        public ButtonComponent label;
        public ListComponent list;
        public ScrollRect scrollRect;

        public Side side;
        public float minSize;
        public float maxSize;
        
        private System.Action callback;
        private System.Action<ButtonComponent> callbackWithInstance;

        public override void OnInit() {
            
            base.OnInit();
            
            this.label.SetCallback(this.DoToggleDropdown);
            
        }

        public override void OnDeInit() {
            
            base.OnDeInit();
            
        }

        public void DoToggleDropdown() {

            if (this.list.IsVisibleSelf() == true) {
                
                this.list.Hide();
                
            } else {
                
                this.list.Show();
                
            }
            
        }
        
        public void SetInteractable(bool state) {

            this.label.SetInteractable(state);

        }
        
        public void SetCallback(System.Action callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void SetCallback(System.Action<ButtonComponent> callback) {

            this.RemoveCallbacks();
            this.AddCallback(callback);

        }

        public void AddCallback(System.Action callback) {

            this.callback += callback;

        }

        public void AddCallback(System.Action<ButtonComponent> callback) {

            this.callbackWithInstance += callback;

        }

        public void RemoveCallback(System.Action callback) {

            this.callback -= callback;

        }

        public void RemoveCallback(System.Action<ButtonComponent> callback) {

            this.callbackWithInstance -= callback;

        }
        
        public virtual void RemoveCallbacks() {
            
            this.callback = null;
            this.callbackWithInstance = null;
            
        }

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if (this.label == null) this.label = this.GetComponentInChildren<ButtonComponent>(true);
            if (this.list == null) this.list = this.GetComponentInChildren<ListComponent>(true);

            if (this.list != null) {

                this.scrollRect = this.list.GetComponent<ScrollRect>();
                
            }
            
        }

    }

}