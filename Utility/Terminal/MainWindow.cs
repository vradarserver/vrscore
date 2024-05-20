// Copyright © 2024 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;

namespace VirtualRadar.Utility.Terminal
{
    class MainWindow : Toplevel
    {
        private IServiceProvider _MasterScopeProvider;
        private IServiceScope _CurrentScope;
        private MenuBar _Menu;
        private View _CurrentView;

        public MainWindow(IServiceProvider serviceProvider) : base()
        {
            _MasterScopeProvider = serviceProvider;
            StartNewScope();

            Width = Dim.Fill();
            Height = Dim.Fill();

            _Menu = new(new MenuBarItem[] {
                new() { Title = "_File", Children = new MenuItem[] {
                    new() { Title = "E_xit", Action = () => Application.RequestStop(), },
                }},
            });

            Add(_Menu);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if(disposing) {
                CloseScope();
            }
        }

        /// <summary>
        /// Replaces the existing view with a new one. Closes and starts a new scope.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ShowViewInNewScope<T>()
            where T: View
        {
            CloseView();

            StartNewScope();
            _CurrentView = _CurrentScope.ServiceProvider.GetRequiredService<T>();
            Add(_CurrentView);
        }

        /// <summary>
        /// Replaces the existing view with another. Scope is left unchanged.
        /// </summary>
        /// <param name="view"></param>
        /// <returns>The view that was current before the new one was swapped in.</returns>
        public View SwapViewInCurrentScope(View view)
        {
            var result = _CurrentView;

            if(_CurrentView != null) {
                Remove(_CurrentView);
            }

            _CurrentView = view;

            if(_CurrentView != null) {
                Add(_CurrentView);
            }

            return result;
        }

        public void ShowModalDialogInCurrentScope<T>()
            where T: Dialog
        {
            var dialog = _CurrentScope.ServiceProvider.GetRequiredService<T>();
            Application.Run(dialog);
        }

        public void CloseView()
        {
            if(_CurrentView != null) {
                Remove(_CurrentView);
                _CurrentView.Dispose();
                _CurrentView = null;
            }
        }

        private void StartNewScope()
        {
            CloseScope();
            _CurrentScope = _MasterScopeProvider.CreateScope();
        }

        private void CloseScope()
        {
            if(_CurrentScope != null) {
                _CurrentScope.Dispose();
                _CurrentScope = null;
            }
        }
    }
}
