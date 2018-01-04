﻿/* ------------------------------------------------------------------------- */
//
// Copyright (c) 2010 CubeSoft, Inc.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/* ------------------------------------------------------------------------- */
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Cube.Xui;
using Cube.Xui.Triggers;

namespace Cube.Net.App.Rss.Reader
{
    /* --------------------------------------------------------------------- */
    ///
    /// RegisterViewModel
    ///
    /// <summary>
    /// 新規 URL の登録画面とモデルを関連付けるための ViewModel です。
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public class RegisterViewModel : ViewModelBase
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// RegisterViewModel
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RegisterViewModel(Func<string, Task> callback) : base(new Messenger())
        {
            _callback = callback;

            Url.PropertyChanged  += (s, e) => Execute.RaiseCanExecuteChanged();
            Busy.PropertyChanged += (s, e) => Execute.RaiseCanExecuteChanged();
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Busy
        /// 
        /// <summary>
        /// 処理中かどうかを示す値を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<bool> Busy { get; } = new Bindable<bool>(false);

        /* ----------------------------------------------------------------- */
        ///
        /// Url
        /// 
        /// <summary>
        /// 追加するフィードの URL を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Bindable<string> Url { get; } = new Bindable<string>();

        /* ----------------------------------------------------------------- */
        ///
        /// Messenger
        /// 
        /// <summary>
        /// Messenger オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IMessenger Messenger => MessengerInstance;

        #endregion

        #region Commands

        /* ----------------------------------------------------------------- */
        ///
        /// Execute
        /// 
        /// <summary>
        /// 新しい RSS フィードを登録するコマンドを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public RelayCommand Execute =>
            _execute = _execute ?? new RelayCommand(
                async () =>
                {
                    try
                    {
                        Busy.Value = true;
                        await _callback?.Invoke(Url.Value);
                        Close();
                    }
                    catch (Exception err) { Error(err); }
                    finally { Busy.Value = false; }
                },
                () => !string.IsNullOrEmpty(Url.Value) && !Busy.Value
            );

        /* ----------------------------------------------------------------- */
        ///
        /// Cancel
        /// 
        /// <summary>
        /// キャンセルコマンドを実行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICommand Cancel => _cancel = _cancel ?? new RelayCommand(() => Close());

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Close
        /// 
        /// <summary>
        /// ウィンドウを閉じます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Close() => Messenger.Send(new CloseMessage());

        /* ----------------------------------------------------------------- */
        ///
        /// Error
        /// 
        /// <summary>
        /// エラーメッセージを表示します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Error(string message)
            => Messenger.Send(new MessageBox(message));

        /* ----------------------------------------------------------------- */
        ///
        /// Error
        /// 
        /// <summary>
        /// エラーメッセージを表示します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Error(Exception err)
        {
            var user = err.GetType() == typeof(ArgumentException);
            var msg  = user ? err.Message : $"{err.Message} ({err.GetType().Name})";
            var ss   = new System.Text.StringBuilder();

            ss.AppendLine(Properties.Resources.ErrorUnexpected);
            ss.AppendLine();
            ss.AppendLine(msg);

            Error(ss.ToString());
        }

        #endregion

        #region Fields
        private Func<string, Task> _callback;
        private RelayCommand _execute;
        private RelayCommand _cancel;
        #endregion
    }
}
