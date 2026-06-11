using System;

public interface IHandTrackingService
{
    HandTrackingStatus Status { get; }
    event Action<HandTrackingStatus> OnStatusChanged;
}
