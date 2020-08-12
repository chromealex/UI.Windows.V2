﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    using Modules;
    
    public enum FocusState {

        None,
        Focused,
        Unfocused,

    }

    public abstract class WindowBase : WindowObject {

        public WindowPreferences preferences = WindowPreferences.Default;
        public WindowModules modules = new WindowModules();
        public Breadcrumb breadcrumb;

        public int identifier;
        
        public FocusState focusState;

        [HideInInspector] public Camera workCamera;

        private float currentDepth;
        private float currentZDepth;

        public WindowSystem.WindowItem GetBreadcrumbPrevious() {

            return this.breadcrumb.GetPreviousWindow(this);

        }
        
        public void SetAsPerspective() {

            this.preferences.cameraMode = UIWSCameraMode.Perspective;
            this.ApplyCamera();

        }

        public void SetAsOrthographic() {

            this.preferences.cameraMode = UIWSCameraMode.Orthographic;
            this.ApplyCamera();

        }

        public override void Hide(TransitionParameters parameters = default) {

            var newParameters = parameters.ReplaceCallback(() => {

                this.PushToPool();
                parameters.RaiseCallback();

            });

            base.Hide(newParameters);

        }

        public virtual int GetCanvasOrder() {

            return 0;

        }

        public void SetInitialParameters(InitialParameters parameters) {

            {

                if (parameters.overrideLayer == true) this.preferences.layer = parameters.layer;
                if (parameters.overrideSingleInstance == true) this.preferences.singleInstance = parameters.singleInstance;

            }

            this.ApplyDepth();
            this.ApplyCamera();

        }

        internal void DoFocusTookInternal() {

            if (this.focusState == FocusState.Focused) return;
            this.focusState = FocusState.Focused;

            this.DoSendFocusTook();

        }

        internal void DoFocusLostInternal() {

            if (this.focusState == FocusState.Unfocused) return;
            this.focusState = FocusState.Unfocused;

            this.DoSendFocusLost();

        }

        internal override void OnHideEndInternal() {
            
            WindowSystem.RemoveWindow(this);

            base.OnHideEndInternal();
            
        }

        internal override void OnShowEndInternal() {

            WindowSystem.SendFullCoverageOnShowEnd(this);

            base.OnShowEndInternal();

        }

        internal override void OnShowBeginInternal() {

            WindowSystem.SendFocusOnShowBegin(this);

            base.OnShowBeginInternal();

        }

        internal override void OnHideBeginInternal() {

            WindowSystem.SendFullCoverageOnHideBegin(this);
            WindowSystem.SendFocusOnHideBegin(this);

            base.OnHideBeginInternal();

        }

        public override void OnDeInit() {

            base.OnDeInit();

        }

        public float GetZDepth() {

            return this.currentZDepth;

        }

        public float GetDepth() {

            return this.currentDepth;

        }

        internal void ApplyCamera() {

            var settings = WindowSystem.GetSettings();
            switch (this.preferences.cameraMode) {

                case UIWSCameraMode.UseSettings:
                    this.workCamera.orthographic = settings.camera.orthographicDefault;
                    break;

                case UIWSCameraMode.Orthographic:
                    this.workCamera.orthographic = true;
                    this.workCamera.orthographicSize = settings.camera.orthographicSize;
                    this.workCamera.nearClipPlane = settings.camera.orthographicNearClippingPlane;
                    this.workCamera.farClipPlane = settings.camera.orthographicFarClippingPlane;
                    break;

                case UIWSCameraMode.Perspective:
                    this.workCamera.orthographic = false;
                    this.workCamera.fieldOfView = settings.camera.perspectiveSize;
                    this.workCamera.nearClipPlane = settings.camera.perspectiveNearClippingPlane;
                    this.workCamera.farClipPlane = settings.camera.perspectiveFarClippingPlane;
                    break;

            }

        }

        internal void ApplyDepth() {

            var depth = WindowSystem.GetNextDepth(this.preferences.layer);
            var zDepth = WindowSystem.GetNextZDepth(this.preferences.layer);

            this.currentDepth = depth;
            this.currentZDepth = zDepth;

            var tr = this.transform;
            this.workCamera.depth = depth;
            var pos = tr.position;
            pos.z = zDepth;
            tr.position = pos;

        }

        public virtual void LoadAsync(System.Action onComplete) {

            this.modules.LoadAsync(this, onComplete);

        }

    }

}