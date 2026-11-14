using vmPing.Configuration;

namespace vmPing.Probes
{
    public partial class Probe
    {
        private bool _isLatencyHigh;

        // Updates statistics and status after a host replies.
        private void HandleReply(long roundTripTime)
        {
            Statistics.Received++;
            Statistics.AddRoundTripTime(roundTripTime);
            IndeterminateCount = 0;

            // If this is a new probe, record the initial 'up' state to the status history.
            if (Status == ProbeStatus.Inactive)
            {
                AddStatusHistory(ProbeStatus.Up, true);
                Status = ProbeStatus.Up;
            }

            // Check if status changed from down to up.
            if (Status == ProbeStatus.Down)
            {
                OnStatusChange(ProbeStatus.Up, "up");

                // Latency tracking starts over after an outage.
                _isLatencyHigh = false;
                HighLatencyCount = 0;
            }

            UpdateLatency(roundTripTime);
        }

        private void UpdateLatency(long roundTripTime)
        {
            bool isSlow =
                (ApplicationOptions.LatencyDetectionMode == LatencyMode.Fixed &&
                roundTripTime >= ApplicationOptions.HighLatencyMilliseconds) ||
                (ApplicationOptions.LatencyDetectionMode == LatencyMode.Dynamic &&
                roundTripTime >= Statistics.MinimumTime + ApplicationOptions.HighLatencyMilliseconds);

            // Count up on each slow reply and back down on each normal reply.
            if (isSlow)
            {
                if (HighLatencyCount < ApplicationOptions.HighLatencyAlertTriggerCount)
                {
                    HighLatencyCount++;
                }
            }
            else if (HighLatencyCount > 0)
            {
                HighLatencyCount--;
            }

            // Alert once when latency becomes high, and once when it returns to normal.
            if (!_isLatencyHigh && isSlow && HighLatencyCount >= ApplicationOptions.HighLatencyAlertTriggerCount)
            {
                _isLatencyHigh = true;
                OnStatusChange(ProbeStatus.LatencyHigh, "high latency");
            }
            else if (_isLatencyHigh && HighLatencyCount == 0)
            {
                _isLatencyHigh = false;
                OnStatusChange(ProbeStatus.LatencyNormal, "normal latency");
            }

            if (_isLatencyHigh)
            {
                Status = ProbeStatus.LatencyHigh;
            }
            else if (isSlow)
            {
                Status = ProbeStatus.Indeterminate;
            }
            else
            {
                Status = ProbeStatus.Up;
            }
        }

        // Updates statistics and status after a host fails to reply.
        private void HandleNoReply()
        {
            Statistics.Lost++;
            IndeterminateCount++;

            if (Status == ProbeStatus.Up || Status == ProbeStatus.LatencyHigh)
            {
                Status = ProbeStatus.Indeterminate;
            }
            else if (Status == ProbeStatus.Inactive)
            {
                // Because this is a new probe, ignore the indeterminate count
                // and immediately mark the host as down.
                // Also, record the initial 'down' state to the status history.
                AddStatusHistory(ProbeStatus.Down, true);
                Status = ProbeStatus.Down;
            }

            // Check for status change.
            if (Status == ProbeStatus.Indeterminate &&
                IndeterminateCount >= ApplicationOptions.AlertThreshold)
            {
                Status = ProbeStatus.Down;
                OnStatusChange(ProbeStatus.Down, "down");
            }
        }
    }
}
