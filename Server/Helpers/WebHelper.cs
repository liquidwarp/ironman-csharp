namespace IronManServer.Helpers;

using System.Runtime.CompilerServices;
using SPTarkov.DI.Annotations;
using Web.Shared;

[Injectable]
public class Utils
{
    public List<string> CallerList { get; } = [];

    public IEnumerable<string> StringObjectIDValidation(string value)
    {
        if (!string.IsNullOrEmpty(value) && (value.Length != 24 || !IsHex(value)))
        {
            yield return "Invalid MongoID";
        }
    }

    public IEnumerable<string> StringLengthValidation(string value)
    {
        if (!string.IsNullOrEmpty(value) && value.Length >= 19)
        {
            yield return "Invalid, Name too long";
        }
    }

    private bool IsHex(IEnumerable<char> chars)
    {
        return chars.Select(c => c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F').All(isHex => isHex);

    }

    public bool IsHexAndValidLength(string value)
    {
        return value.Length == 24 && IsHex(value);
    }

    public bool IsStringAndValidLength(string value)
    {
        return value.Length <= 19;
    }

    public void UpdateViewBool(bool holder, bool actual)
    {
        if (holder != actual)
        {
            MainLayout.EnableUnsavedChangesButton();
        }
    }

    public void UpdateView(
        bool holder,
        bool originalConfigValue,
        [CallerMemberName] string caller = "")
    {
        UpdatePendingChange(
            holder != originalConfigValue,
            caller
        );
    }

    public void UpdateView(
        int holder,
        int originalConfigValue,
        [CallerMemberName] string caller = "")
    {
        UpdatePendingChange(
            holder != originalConfigValue,
            caller
        );
    }

    public void UpdateView(
        double holder,
        double originalConfigValue,
        [CallerMemberName] string caller = "")
    {
        UpdatePendingChange(
            Math.Abs(holder - originalConfigValue) > 0.001f,
            caller
        );
    }

    public void UpdateView(
        List<string> holder,
        List<string> originalConfigValue,
        [CallerMemberName] string caller = "")
    {
        UpdatePendingChange(
            !holder.SequenceEqual(originalConfigValue),
            caller
        );
    }

    public void UpdateView(
        List<int> holder,
        List<int> originalConfigValue,
        [CallerMemberName] string caller = "")
    {
        UpdatePendingChange(
            !holder.SequenceEqual(originalConfigValue),
            caller
        );
    }

    public void UpdateView([CallerMemberName] string caller = "")
    {
        if (MainLayout.PendingChanges.Contains(caller))
        {
            MainLayout.PendingChanges.Remove(caller);
        }
        else
        {
            MainLayout.PendingChanges.Add(caller);
        }

        MainLayout.TriggerUIRefresh();
    }

    public void UpdateView(
        bool isWeightChange,
        [CallerMemberName] string caller = "")
    {
        if (isWeightChange && !MainLayout.PendingChanges.Contains(caller))
        {
            MainLayout.PendingChanges.Add(caller);
        }

        MainLayout.TriggerUIRefresh();
    }

    public void UpdateView(
        string holder,
        string originalConfigValue,
        [CallerMemberName] string caller = "")
    {
        UpdatePendingChange(
            holder != originalConfigValue,
            caller
        );
    }

    private void UpdatePendingChange(bool hasChanged, string caller)
    {
        if (MainLayout.PendingChanges.Contains(caller))
        {
            if (hasChanged)
            {
                return;
            }

            MainLayout.PendingChanges.Remove(caller);
        }
        else if (hasChanged)
        {
            MainLayout.PendingChanges.Add(caller);
        }

        MainLayout.TriggerUIRefresh();
    }
}