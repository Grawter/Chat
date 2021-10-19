using System;
using System.Windows;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.Helpers.ViewModel
{
    public class DisplayRootRegistry
    {
        static Dictionary<Type, Type> vmToWindowMapping = new Dictionary<Type, Type>();

        public void RegisterWindowType<VM, Win>() where Win : Window, new() where VM : class
        {
            var vmType = typeof(VM);
            if (vmType.IsInterface)
                throw new ArgumentException("Cannot register interfaces");
            if (vmToWindowMapping.ContainsKey(vmType))
                throw new InvalidOperationException($"Type {vmType.FullName} is already registered");

            vmToWindowMapping[vmType] = typeof(Win);
        }

        public void UnregisterWindowType<VM>()
        {
            var vmType = typeof(VM);
            if (vmType.IsInterface)
                throw new ArgumentException("Cannot register interfaces");
            if (!vmToWindowMapping.ContainsKey(vmType))
                throw new InvalidOperationException($"Type {vmType.FullName} is not registered");

            vmToWindowMapping.Remove(vmType);
        }

        public Window CreateWindowInstanceWithVM(object vm)
        {
            if (vm == null)
                throw new ArgumentNullException("VM is null");
            Type windowType = null;

            var vmType = vm.GetType();
            while (vmType != null && !vmToWindowMapping.TryGetValue(vmType, out windowType))
                vmType = vmType.BaseType;

            if (windowType == null)
                throw new ArgumentException(
                    $"No registered window type for argument type {vm.GetType().FullName}");

            var window = (Window)Activator.CreateInstance(windowType);
            window.DataContext = vm;
            return window;
        }


        Dictionary<object, object> relationsWindows = new Dictionary<object, object>();
        public void SetParent(object children, object parent, bool rewriteOFF = true)
        {
            if (children == null || parent == null)
                throw new ArgumentNullException("VM is null");
            if (!rewriteOFF)
            {
                if (relationsWindows.ContainsKey(children))
                    throw new InvalidOperationException("There is already a parent for this VM");
            }            

            relationsWindows[children] = parent;
        }

        public void DeleteParent(object children)
        {
            if (children == null)
                throw new ArgumentNullException("VM is null");
            if (!relationsWindows.ContainsKey(children))
                throw new InvalidOperationException("Parent for this VM is not found");

            relationsWindows.Remove(children);
        }

        Dictionary<object, Window> openWindows = new Dictionary<object, Window>();
        public void ShowParent(object vm)
        {
            Window window;
            object parent;

            if (vm == null)
                throw new ArgumentNullException("VM is null");
            if (!relationsWindows.TryGetValue(vm, out parent))
                throw new InvalidOperationException("Parent for this VM is not found");

            window = CreateWindowInstanceWithVM(parent);
            window.Show();
            openWindows[parent] = window;
        }

        public object GetParent(object vm)
        {
            object parent;

            if (vm == null)
                throw new ArgumentNullException("VM is null");
            if (!relationsWindows.TryGetValue(vm, out parent))
                throw new InvalidOperationException("Parent for this VM is not found");

            return parent;
        }

        public void ShowPresentation(object vm)
        {
            Window window;

            if (vm == null)
                throw new ArgumentNullException("VM is null");
            if (openWindows.ContainsKey(vm))
                throw new InvalidOperationException("UI for this VM is already displayed");
            if (!hideWindows.TryGetValue(vm, out window))
                window = CreateWindowInstanceWithVM(vm);

            window.Show();
            openWindows[vm] = window;
            hideWindows.Remove(vm);
        }

        public async Task ShowModalPresentation(object vm)
        {
            var window = CreateWindowInstanceWithVM(vm);
            await window.Dispatcher.InvokeAsync(() => window.ShowDialog());        
        }

        Dictionary<object, Window> hideWindows = new Dictionary<object, Window>();
        public void HidePresentation(object vm)
        {
            Window window;
            if (!openWindows.TryGetValue(vm, out window))
                throw new InvalidOperationException("UI for this VM is not displayed");
            window.Hide();           
            hideWindows[vm] = window;
            openWindows.Remove(vm);
        }

        public void ClosePresentation(object vm)
        {
            Window window;
            if (!openWindows.TryGetValue(vm, out window))
                throw new InvalidOperationException("UI for this VM is not displayed");
            window.Close();
            openWindows.Remove(vm);
        }

    }
}