using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vocb.App.Services;

namespace Vocb.App.Views;

/// <summary>
/// Account + preferences. The Firebase project is baked in, so there's nothing to
/// configure here — just the signed-in account (with Log out) and the daily reminder.
/// </summary>
public sealed partial class SettingsPage : Page
{
    // Guards the reminder controls while we populate them on load, so seeding their
    // values doesn't immediately re-save / re-schedule.
    private bool _loading;

    public SettingsPage()
    {
        InitializeComponent();
        LoadState();
    }

    private void LoadState()
    {
        _loading = true;

        EmailText.Text = App.Session.Email is { Length: > 0 } email
            ? $"Signed in as {email}"
            : "Signed in";

        var settings = App.SettingsStore.Load();
        ReminderToggle.IsOn = settings.ReminderEnabled;
        ReminderTime.SelectedTime = new TimeSpan(settings.ReminderHour, settings.ReminderMinute, 0);

        _loading = false;
    }

    private void Logout_Click(object sender, RoutedEventArgs e) => App.Session.SignOut();

    private void Reminder_Changed(object sender, RoutedEventArgs e) => SaveReminder();

    private void ReminderTime_Changed(TimePicker sender, TimePickerSelectedValueChangedEventArgs args) => SaveReminder();

    private void SaveReminder()
    {
        if (_loading) return;

        var time = ReminderTime.SelectedTime ?? new TimeSpan(19, 0, 0);
        var settings = App.SettingsStore.Load();
        settings.ReminderEnabled = ReminderToggle.IsOn;
        settings.ReminderHour = time.Hours;
        settings.ReminderMinute = time.Minutes;
        App.SettingsStore.Save(settings);

        if (settings.ReminderEnabled)
            App.Reminder?.Schedule(settings.ReminderHour, settings.ReminderMinute);
        else
            App.Reminder?.Stop();
    }
}
