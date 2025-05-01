using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Messaging;

using Firefly.Models.Messages;

namespace Firefly.Views;

/// <summary>
/// CccfCombinedQueryBarView.xaml 的交互逻辑
/// </summary>
public partial class CccfCombinedQueryBarView : UserControl
{
    public CccfCombinedQueryBarView()
    {
        InitializeComponent();

        if (App.IsInDesignMode)
        {
            return;
        }

        WeakReferenceMessenger.Default.Register<FocusOnCombinedQueryBarMessage>(this,
            (r, m) => RestoreFocus(m.NeedsSelectAll, m.IsRestore));
    }

    /// <summary>
    /// 只需让各可见容器的最后一个元素订阅此事件（因为 XAML 渲染顺序从上到下）。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Input_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is false)
        {
            return;
        }

        RestoreFocus();
    }

    private void RestoreFocus(bool needsSelectAll = false, bool isRestore = true)
    {
        IEnumerable<FrameworkElement> GetBars()
        {
            yield return tbEnt;
            yield return tbName;
            yield return tbModel;
            yield return tbCertNo;
            yield return tbReportNo;
            yield return cboStatus;
            yield return dpCertStart;
            yield return dpCertEnd;
            yield return dpIssuedStart;
            yield return dpIssuedEnd;
            yield return cboTestingCenter;
        }

        void Restore()
        {
            // 需要 element 作为焦点范围的元素（FocusManager.IsFocusScope="True"），这里 element 即 this（UserControl）
            if (FocusManager.GetFocusedElement(this) is IInputElement lastFocused)
            {
                lastFocused.Focus();
                Keyboard.Focus(lastFocused);

                if (lastFocused is TextBox textBox)
                {
                    textBox.CaretIndex = textBox.Text.Length;

                    if (needsSelectAll && !String.IsNullOrEmpty(textBox.Text))
                    {
                        textBox.SelectAll();
                    }
                }
            }
            else
            {
                // 回退到第一个输入框
                tbEnt.Focus();
                Keyboard.Focus(tbEnt);
            }
        }

        if (isRestore)
        {
            Restore();
        }
        else
        {
            bool focused = false;

            foreach (var bar in GetBars())
            {
                if (bar is TextBox textBox && !String.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Focus();
                    Keyboard.Focus(textBox);

                    if (needsSelectAll)
                    {
                        textBox.SelectAll();
                    }

                    focused = true;

                    break;
                }
                else if (bar is ComboBox comboBox && !String.IsNullOrWhiteSpace(comboBox.Text))
                {
                    comboBox.Focus();
                    Keyboard.Focus(comboBox);
                    focused = true;

                    break;
                }
                else if (bar is DatePicker datePicker && !String.IsNullOrWhiteSpace(datePicker.Text))
                {
                    datePicker.Focus();
                    Keyboard.Focus(datePicker);
                    focused = true;

                    break;
                }
            }

            if (!focused)
            {
                Restore();
            }
        }
    }
}
