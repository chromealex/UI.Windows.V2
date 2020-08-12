using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Components {

    using Utilities;
    
    public abstract class ListBaseComponent : WindowComponent, UnityEngine.EventSystems.IBeginDragHandler, UnityEngine.EventSystems.IDragHandler, UnityEngine.EventSystems.IEndDragHandler {

        [UnityEngine.UI.Windows.Modules.ResourceTypeAttribute(typeof(WindowComponent))]
        public Resource source;
        public Transform customRoot;

        public List<WindowComponent> items = new List<WindowComponent>();
        private HashSet<Object> loadedAssets = new HashSet<Object>();
        
        public override void ValidateEditor() {
            
            base.ValidateEditor();

            var editorObj = this.source.GetEditorRef<WindowComponent>();
            if (editorObj != null) {
            
                if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(editorObj) == false) {

                    editorObj.allowRegisterInRoot = false;
                    editorObj.AddEditorParametersRegistry(new EditorParametersRegistry() {
                        holder = this,
                        allowRegisterInRoot = true,
                        allowRegisterInRootDescription = "Hold by ListComponent"
                    });

                    editorObj.gameObject.SetActive(false);

                }

            }
            
        }

        public override void OnInit() {
            
            base.OnInit();

            WindowSystem.onPointerUp += this.OnPointerUp;

        }

        public override void OnDeInit() {
            
            base.OnDeInit();
            
            WindowSystem.onPointerUp -= this.OnPointerUp;
            
            var resources = WindowSystem.GetResources();
            foreach (var asset in this.loadedAssets) {
            
                resources.Delete(this, asset);

            }
            this.loadedAssets.Clear();
            
        }

        private void OnPointerUp() {
            
            var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            for (int i = 0; i < this.componentModules.modules.Length; ++i) {

                var module = this.componentModules.modules[i] as ListComponentModule;
                if (module == null) continue;
                
                module.OnDragEnd(eventData);

            }

        }
        
        void UnityEngine.EventSystems.IBeginDragHandler.OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData) {
            
            for (int i = 0; i < this.componentModules.modules.Length; ++i) {

                var module = this.componentModules.modules[i] as ListComponentModule;
                if (module == null) continue;
                
                module.OnDragBegin(eventData);

            }
            
        }

        void UnityEngine.EventSystems.IDragHandler.OnDrag(UnityEngine.EventSystems.PointerEventData eventData) {
            
            for (int i = 0; i < this.componentModules.modules.Length; ++i) {

                var module = this.componentModules.modules[i] as ListComponentModule;
                if (module == null) continue;
                
                module.OnDragMove(eventData);

            }

        }

        void UnityEngine.EventSystems.IEndDragHandler.OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData) {
            
            for (int i = 0; i < this.componentModules.modules.Length; ++i) {

                var module = this.componentModules.modules[i] as ListComponentModule;
                if (module == null) continue;
                
                module.OnDragEnd(eventData);

            }

        }

        public Transform GetRoot() {

            if (this.customRoot != null) return this.customRoot;
            
            return this.transform;

        }

        public virtual void Clear() {

            var pools = WindowSystem.GetPools();
            for (int i = this.items.Count - 1; i >= 0; --i) {

                this.UnRegisterSubObject(this.items[i]);
                pools.Despawn(this.items[i]);
                
            }
            this.items.Clear();
            this.OnElementsChanged();
            
        }

        public virtual void OnElementsChanged() {

            for (int i = 0; i < this.componentModules.modules.Length; ++i) {

                var module = this.componentModules.modules[i] as ListComponentModule;
                if (module == null) continue;
                
                module.OnElementsChanged();

            }
            
        }
        
        public virtual void AddItem(System.Action<WindowComponent> onComplete = null) {
            
            this.AddItem(this.source, onComplete);
            
        }

        public virtual void AddItem<T>(Resource source, System.Action<T> onComplete = null) where T : WindowComponent {

            var resources = WindowSystem.GetResources();
            var pools = WindowSystem.GetPools();
            Coroutines.Run(resources.LoadAsync<T>(this, source, (asset) => {

                if (this.loadedAssets.Contains(asset) == false) this.loadedAssets.Add(asset);
                
                var instance = pools.Spawn(asset, this.GetRoot());
                this.RegisterSubObject(instance);
                if (onComplete != null) onComplete.Invoke(instance);
                this.items.Add(instance);
                this.OnElementsChanged();

            }));
            
        }

        public virtual void RemoveItem(int index) {

            if (index < this.items.Count) {

                var pools = WindowSystem.GetPools();
                this.UnRegisterSubObject(this.items[index]);
                pools.Despawn(this.items[index]);
                this.items.RemoveAt(index);
                this.OnElementsChanged();

            }

        }

        public virtual void SetItems<T>(int count, System.Action<T, int> onItem, System.Action onComplete = null) where T : WindowComponent {
            
            this.SetItems(count, this.source, onItem, onComplete);
            
        }

        public virtual void SetItems<T>(int count, Resource source, System.Action<T, int> onItem, System.Action onComplete = null) where T : WindowComponent {

            this.Clear();
            var loaded = 0;
            for (int i = 0; i < count; ++i) {

                var index = i;
                this.AddItem<T>(source, (item) => {

                    onItem.Invoke(item, index);
                    
                    ++loaded;
                    if (loaded == count) {
                        
                        if (onComplete != null) onComplete.Invoke();
                        
                    }
                    
                });
                
            }

        }

    }

}